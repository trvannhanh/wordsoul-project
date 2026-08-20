# Quay lại và xử lý khóa ngừng dùng M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-ROLLBACK-DEPRECATION-1.0` |
| Task | M11-T017 |
| Đầu vào | M11-CONFIG-VERSION-1.0, M11-CANARY-ROLLOUT-1.0, M11-CHANGE-EXECUTION-1.0, M11-CONFIG-REG-1.0 |
| Phạm vi | Cơ chế khôi phục phiên bản cấu hình an toàn (Rollback Execution) và vòng đời loại bỏ khóa cấu hình cũ (Key Deprecation & Soft Retirement) |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy trình quay lại phiên bản cấu hình trước đó khi phát sinh sự cố (Config Rollback Execution) và vòng đời ngừng sử dụng/loại bỏ các khóa cấu hình hệ thống cũ (Key Deprecation & Retirement Lifecycle) mà không làm gián đoạn dịch vụ hoặc gây rò rỉ dữ liệu.

- **Nguyên tắc bất biến khi Rollback**: Quay lại cấu hình KHÔNG xóa, KHÔNG ghi đè, KHÔNG sửa đổi các bản ghi phiên bản cũ (`setVersionId`). Rollback tạo ra một `ConfigEffectiveAssignment` mới trỏ tới phiên bản cũ tương thích (`compatible prior version`), đồng thời ghi nhận sự kiện `ConfigVersionEvent` loại `ROLLBACK`.
- **Phân loại cấp độ Rollback**:
  - `RB-1`: Chuyển pointer tức thì về phiên bản cũ tương thích (dành cho thay đổi giá trị cấu hình thuần túy).
  - `RB-2`: Khôi phục pointer + Thực thi thao tác đền bù (`Compensating Operations`) xử lý các trạng thái nghiệp vụ trung gian đã phát sinh trong thời gian chạy phiên bản lỗi.
  - `RB-3`: Cô lập rủi ro diện rộng (`Incident Freeze & Isolation`), chuyển hệ thống về chế độ an toàn tối thiểu và tạo hồ sơ sự cố theo dõi.
- **Vòng đời Ngừng dùng Khóa (`Key Deprecation Lifecycle`)**:
  - Khóa cấu hình chuyển từ `Active` $\to$ `Deprecated` $\to$ `Retired`.
  - Khóa ở trạng thái `Deprecated` vẫn cho phép consumer đọc giá trị fallback nhưng KHÔNG cho phép tạo candidate/version mới chứa khóa này.
  - Chuyển sang `Retired` BẮT BUỘC trải qua quét tham chiếu (`Reference Scan`) chứng minh $0$ consumer/session/job nào đang sử dụng và đủ thời gian lưu trữ tối thiểu.
- **Ràng buộc giữ bản ghi (`Hold & Reference Protection`)**: Không được phép xóa cứng (Physical Delete) hoặc giải phóng khóa cấu hình khi còn bất kỳ phiên người dùng, phiên học, trận đấu, hay hồ sơ kiểm toán nào đang trỏ tới.

## 2. Ephemeral & Durable Model cho Rollback & Deprecation

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `ConfigRollbackExecutionRecord` | `rollbackId` | `executionId`, `targetSetVersionId`, `baseSetVersionId`, `rollbackMode` (`RB-1/2/3`), `triggeredBy`, `reason`, `executedAtUtc` | Bất biến |
| `ConfigKeyDeprecationRecord` | `deprecationId` | `configId`, `deprecatedAtUtc`, `targetRetirementAtUtc`, `replacementConfigId`, `fallbackValue`, `reason`, `status` | CAS Mutable |
| `KeyReferenceScanResult` | `scanId` | `configId`, `scannedAtUtc`, `activeConsumersCount`, `activeSessionsCount`, `durableReferencesCount`, `isSafeToRetire` | Ephemeral Scan |
| `KeyRetirementManifest` | `manifestId` | `configId`, `retiredAtUtc`, `approvedBy`, `scanSummary`, `archiveDigest` | Bất biến / Audit |

Vòng đời trạng thái khóa (`Key Lifecycle`):
`Active` $\to$ `Deprecated` (Phát warning/chặn candidate mới) $\to$ `Pending Retirement` (Quét tham chiếu) $\to$ `Retired` (Lưu trữ và ngừng đọc)

## 3. Quy trình thực thi Quay lại Cấu hình (Rollback Workflow)

```
                       +---------------------------------------+
                       | Trigger Rollback (Auto or Manual)     |
                       +---------------------------------------+
                                           |
                                           v
                       +---------------------------------------+
                       | Check Compatibility & Prior Version   |
                       +---------------------------------------+
                                           |
                         +-----------------+-----------------+
                         | (Compatible)                      | (Incompatible / Irreversible)
                         v                                   v
             [Select Rollback Mode]              [Trigger Incident Response (RB-3)]
                         |                                   |
         +---------------+---------------+                   v
         | (RB-1)                        | (RB-2)    [Forward Recovery / Incident]
         v                               v
[CAS Pointer Switch]           [Execute Compensating Ops]
         |                               |
         +---------------+---------------+
                         |
                         v
       +-----------------------------------+
       | Verify Consumer Convergence       |
       +-----------------------------------+
                         |
                         v
       +-----------------------------------+
       | Emit Audit Event & Update Metrics |
       +-----------------------------------+
```

### 3.1. Các bước thực thi Rollback chuẩn (RB-1)
1. **Xác định phiên bản mục tiêu**: Chọn `targetSetVersionId` tương thích gần nhất đã được xác minh (`validated` & `backwardCompatible`).
2. **Kiểm tra tính tương thích (`Compatibility Check`)**: Nếu `compatibility == incompatible` hoặc `irreversibleDataEffect`, cấm chạy RB-1; bắt buộc chuyển sang quy trình khôi phục tiến (`Forward Recovery`) hoặc sự cố (RB-3).
3. **Cập nhật CAS Pointer**: Thực thi giao dịch nguyên tử (Atomic CAS) chuyển `ConfigCurrentPointer` trỏ về `targetSetVersionId`.
4. **Thông báo Invalidations**: Gửi tín hiệu evict cache tới toàn bộ consumer nodes.
5. **Xác minh hội tụ**: Theo dõi báo cáo `ConsumerVersionObservation` đảm bảo $99\%$ node đã quay về version cũ.

## 4. Xử lý Khóa Ngừng dùng (Key Deprecation & Soft Retirement)

### 4.1. Quy tắc Soft Deprecation
Khi một khóa cấu hình (`Config ID`) không còn phù hợp hoặc được thay thế bằng khóa mới:
1. Đánh dấu `status = Deprecated` và lưu `deprecatedAtUtc`.
2. Ghi nhận `replacementConfigId` (nếu có) và `fallbackValue`.
3. Khi consumer truy vấn khóa `Deprecated`, hệ thống trả về giá trị kèm thông điệp cảnh báo `DEPRECATED_KEY_WARNING` trong header/log.
4. Chặn toàn bộ các `ChangeRequestCandidate` mới cố tình bổ sung hoặc chỉnh sửa khóa đã `Deprecated`.

### 4.2. Quét tham chiếu tự động (`Automated Reference Scan`)
Trước khi chuyển khóa từ `Deprecated` sang `Retired`, hệ thống chạy bài quét `KeyReferenceScanResult` rà soát 5 ranh giới:
1. **Active Consumer Instances**: Kiểm tra xem có microservice instance nào còn trỏ đọc khóa cũ không.
2. **Active User & Review Sessions**: Kiểm tra session snapshot trong Redis/SQL.
3. **Background Jobs**: Kiểm tra công việc nền đang xếp hàng hoặc đang chạy.
4. **Durable Ledger & Audit References**: Kiểm tra các bản ghi tài chính M06 hoặc audit log M11.
5. **Legal & Compliance Holds**: Kiểm tra các yêu cầu giữ dữ liệu phục vụ điều tra.

Chỉ khi `activeConsumersCount == 0`, `activeSessionsCount == 0`, `durableReferencesCount == 0` và qua cửa sổ an toàn tối thiểu ($30$ ngày), khóa mới được chuyển sang `Retired`.

### 4.3. Đổi tên khóa và Bí danh (`Key Renaming & Aliasing`)
Khi đổi tên một khóa cấu hình (vd: `CFG-001` $\to$ `CFG-001-NEW`):
- KHÔNG xóa ngay khóa cũ `CFG-001`.
- Khóa cũ `CFG-001` tự động trở thành một **Alias** trỏ tới `CFG-001-NEW`.
- Khóa `CFG-001` chuyển sang trạng thái `Deprecated` với thời hạn chuyển đổi tối thiểu 90 ngày trước khi cho phép `Retired`.

## 5. Ràng buộc Giữ bản ghi và Bảo vệ Kiểm toán (Hold Policy)

- **Cấm Xóa Cứng Khóa Cấu hình**: Tuyệt đối không xóa physical row của khóa cấu hình đã từng có hiệu lực trong môi trường Production.
- **Hold Protection**: Ngay cả khi khóa ở trạng thái `Retired`, metadata và lịch sử phiên bản của khóa vẫn được lưu trữ bất biến phục vụ truy vết sự cố và kiểm toán pháp lý.

## 6. Bảo mật và Phân quyền

- **Quyền Rollback Execution**: Yêu cầu quyền `M11-PERM-CONFIG-ROLLBACK` (gắn với R02 Operations Admin, R12 Security Admin).
- **Quyền Deprecate / Retire Key**: Yêu cầu quyền `M11-PERM-CONFIG-DEPRECATE` (gắn với R12 Security Admin, R13 System Owner).
- **Audit Requirement**: Toàn bộ thao tác Rollback, Deprecate và Retire bắt buộc phải ghi log kiểm toán kèm lý do và chứng từ liên quan.

## 7. Regression Gate và Case tự kiểm

### 7.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RD-G01` | Rollback tạo `ConfigEffectiveAssignment` mới trỏ phiên bản cũ tương thích, không xóa/sửa lịch sử. |
| `RD-G02` | Cấm thực thi RB-1 nếu phiên bản mục tiêu đánh dấu `incompatible` hoặc `irreversibleDataEffect`. |
| `RD-G03` | Cập nhật CAS Pointer trong giao dịch nguyên tử và xác minh hội tụ consumer đạt $\ge 99\%$. |
| `RD-G04` | Khóa ở trạng thái `Deprecated` chặn 100% các Candidate mới cố tình bổ sung/chỉnh sửa khóa này. |
| `RD-G05` | Quét tham chiếu (`Reference Scan`) phát hiện còn session/job/hold active lập tức chặn chuyển `Retired`. |
| `RD-G06` | Đổi tên khóa tự động tạo Alias và gắn `Deprecated` với thời hạn chuyển đổi tối thiểu 90 ngày. |
| `RD-G07` | Cấm xóa cứng (Physical Delete) các khóa cấu hình đã từng có hiệu lực trong Production. |
| `RD-G08` | Mọi thao tác Rollback/Deprecate/Retire ghi nhận bản ghi Audit bất biến kèm lý do. |
| `RD-G09` | Phân quyền thao tác tuân thủ nghiêm ngặt ma trận quyền tối thiểu `M11-PERM-1.0`. |
| `RD-G10` | 100% các test case tự kiểm RD17-01–20 đạt thành công trong bộ suite kiểm thử. |

### 7.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RD17-01` | Thực thi RB-1 quay lại phiên bản `v1.3` tương thích hợp lệ | Tạo assignment mới trỏ `v1.3`, cập nhật CAS pointer thành công |
| `RD17-02` | Cố tình gọi RB-1 tới phiên bản `v1.1` đánh dấu `incompatible` | System reject với lỗi `ROLLBACK_TARGET_INCOMPATIBLE` |
| `RD17-03` | Thực thi RB-2 trên phiên bản có tác động tài sản | Gọi compensating operations khôi phục dữ liệu M06 |
| `RD17-04` | Chuyển khóa `MaxGroupSize` sang trạng thái `Deprecated` | Phát cảnh báo khi consumer đọc, chặn tạo Candidate mới với khóa này |
| `RD17-05` | Thử chuyển khóa `Deprecated` sang `Retired` khi còn 1 session active | `Reference Scan` trả về `activeSessionsCount = 1`, reject request |
| `RD17-06` | Đổi tên khóa `CFG-008` sang `CFG-008-V2` | `CFG-008` thành Alias `Deprecated`, trỏ dữ liệu sang `CFG-008-V2` |
| `RD17-07` | Thử thực hiện SQL `DELETE FROM SystemConfigurations` trên khóa `Retired` | DB trigger deny operation với lỗi `PHYSICAL_DELETE_FORBIDDEN` |
| `RD17-08` | User không có quyền `M11-PERM-CONFIG-ROLLBACK` thực thi Rollback | Deny 403 Forbidden |
| `RD17-09` | Rollback thực thi thành công nhưng consumer instance bị timeout | Hệ thống theo dõi hội tụ đánh dấu warning và gửi alert |
| `RD17-10` | Thực thi Rollback khi đang có 1 Rollout Plan khác đang chạy | Lock CAS ngăn chặn xung đột, yêu cầu hủy Rollout cũ |
| `RD17-11` | Thử gọi RB-1 tới `setVersionId` không tồn tại | Reject request với lỗi `VERSION_NOT_FOUND` |
| `RD17-12` | Quét tham chiếu tự động hoàn tất với 0 active reference | Khóa chuyển thành `Pending Retirement` an toàn |
| `RD17-13` | Consumer truy vấn khóa `Retired` không có fallback | Trả về lỗi `CONFIG_KEY_RETIRED` |
| `RD17-14` | Rollback phiên bản cấu hình chứa secret | Protected secret reference được khôi phục chính xác |
| `RD17-15` | Thử tạo candidate chứa khóa đã `Retired` | Candidate validation reject với lỗi `CANNOT_USE_RETIRED_KEY` |
| `RD17-16` | Thực thi RB-3 khẩn cấp cho sự cố SEV-1 | Đóng băng giao dịch, tạo hồ sơ sự cố M11 |
| `RD17-17` | Kiểm tra tính bất biến của bản ghi `ConfigRollbackExecutionRecord` | API deny mọi thao tác Update/Delete |
| `RD17-18` | Khóa `Deprecated` hết hạn 90 ngày nhưng còn Audit Hold | Giữ trạng thái `Deprecated`, không cho `Retire` |
| `RD17-19` | Tải đồng thời 20 lệnh Rollback request trên các policy set khác nhau | Các lệnh Rollback xử lý độc lập thành công |
| `RD17-20` | Kiểm thử hoàn tất vòng đời khóa từ Active $\to$ Deprecated $\to$ Retired | Toàn bộ sự kiện chuyển trạng thái được lưu trong Audit |

## 8. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-RD-I01` | Chưa có API hay Service thực thi Rollback phiên bản cấu hình trong `WordSoulApi` | Không thể khôi phục cấu hình an toàn khi gặp sự cố | M11-T049 |
| `M11-RD-I02` | Khóa cấu hình hiện tại được sửa trực tiếp trên DB, không có trạng thái `Deprecated` hay `Retired` | Rủi ro gây lỗi crash app khi xóa hoặc sửa tên khóa | M11-T049 |
| `M11-RD-I03` | Thiếu công cụ quét tham chiếu (`Reference Scan`) kiểm tra độ an toàn trước khi retire khóa | Có nguy cơ ngắt đột ngột các session/job đang sử dụng | M11-T049 |
| `M11-RD-I04` | Thiếu cơ chế đền bù (`Compensating Operations - RB-2`) cho các thay đổi có tác động dữ liệu | Khó khắc phục hậu quả khi rollback cấu hình tài chính/điểm | M11-T049 |
| `M11-RD-I05` | Chưa có bảng lưu vết `ConfigRollbackExecutionRecord` và `ConfigKeyDeprecationRecord` | Thiếu nhật ký kiểm toán phục vụ tra cứu lịch sử khôi phục | M11-T049 |

- `M11-RD-F01`: Triển khai `ConfigRollbackService` với các mode RB-1, RB-2, RB-3 (tiếp nhận: M11-T049).
- `M11-RD-F02`: Xây dựng engine quản lý vòng đời khóa (`Key Deprecation & Retirement Manager`) (tiếp nhận: M11-T049).
- `M11-RD-F03`: Triển khai dịch vụ quét tham chiếu tự động (`Automated Reference Scanner`) (tiếp nhận: M11-T049).
- `M11-RD-F04`: Thiết lập bộ test suite tự động RD-G01–G10 và RD17-01–20 (tiếp nhận: M11-T049).
- `M11-RD-F05`: Thu thập bằng chứng runtime cho luồng Rollback và Key Retirement (tiếp nhận: M11-T049; A-G02/A-G06).

## 9. Tự kiểm M11-T017

- Đã thiết kế hoàn chỉnh `M11-ROLLBACK-DEPRECATION-1.0` bao phủ các mode Rollback RB-1, RB-2, RB-3.
- Đã chốt vòng đời quản lý khóa cấu hình: Active $\to$ Deprecated $\to$ Pending Retirement $\to$ Retired.
- Đã xây dựng quy trình quét tham chiếu tự động (`Reference Scan`) rà soát 5 ranh giới trước khi retire khóa.
- Đã thiết lập nguyên tắc cấm xóa cứng (`No Physical Delete`) và bảo vệ bằng chứng kiểm toán (`Hold Policy`).
- Đã xác lập 10 Regression Gates (`RD-G01`–`RD-G10`) và 20 Test Cases tự kiểm (`RD17-01`–`RD17-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task sau.

## 10. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả quay lại và xử lý khóa ngừng dùng M11-T017 | WSA-7K2 |
