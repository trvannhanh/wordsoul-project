# Chốt xử lý chỉnh sửa đồng thời M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CONCURRENT-EDITING-1.0` |
| Task | M11-T021 |
| Đầu vào | M11-CONTENT-LIFECYCLE-1.0, M11-CROSS-CONTENT-MATRIX-1.0, M11-CHANGE-DECISION-1.0 |
| Phạm vi | Cơ chế kiểm soát xung đột chỉnh sửa đồng thời, khóa mềm phiên biên tập và giao thức Rebase dữ liệu |
| Tự kiểm | A-G02, A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập giao thức xử lý xung đột khi có nhiều quản trị viên hoặc tiến trình hệ thống cùng chỉnh sửa, phê duyệt hoặc chuyển trạng thái một thực thể nội dung quản trị (Bộ từ M02, Loại phiên M03, Nhiệm vụ M07, Phòng đấu M08, Thông báo M10) hoặc cấu hình hệ thống (M11) tại cùng một thời điểm.

- **Tuyệt đối cấm Ghi đè Âm thầm (No Silent Overwrite / Lost Update)**: Không cho phép nguyên tắc "Last-Write-Wins" ghi đè dữ liệu của quản trị viên khác mà không qua kiểm tra phiên bản.
- **Kiểm soát Lạc quan bằng CAS (Optimistic Concurrency Control - OCC)**: Mọi thao tác lưu bản thảo hoặc chuyển trạng thái BẮT BUỘC phải truyền `expectedVersionDigest` hoặc `rowVersion`. Giao dịch SQL / Redis thực thi bằng thuật toán Compare-And-Swap (CAS).
- **Khóa mềm Phiên biên tập (Pessimistic Soft Edit Lease)**: Khi một Admin mở giao diện chỉnh sửa bản thảo, hệ thống cấp một `editLeaseToken` có TTL $15$ phút trong Redis. Các Admin khác truy cập cùng lúc sẽ nhận thông báo hiện diện ("Presence Warning") và bị khóa chế độ chỉnh sửa (Read-Only Mode) trừ khi người giữ lease nhượng quyền.
- **Giao thức Rebase bắt buộc khi Xung đột (Conflict Rebase Protocol)**: Khi xảy ra xung đột CAS ($409\text{ Conflict}$), hệ thống KHÔNG tự động hợp nhất ngầm (No Silent Auto-Merge). Hệ thống trả về bản so sánh vi sản (`Structural Diff`) giữa bản ghi hiện tại và bản thảo của Admin, yêu cầu Admin tự rebase và xác nhận lại trước khi commit.

## 2. Ephemeral & Durable Model cho Concurrency Control

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `EditLeaseRecord` | `leaseId` | `entityId`, `entityType`, `editorAdminId`, `issuedAtUtc`, `expiresAtUtc`, `editLeaseToken` | Ephemeral Redis / TTL 15m |
| `EditorPresenceSignal` | `signalId` | `entityId`, `entityType`, `adminId`, `adminName`, `heartbeatAtUtc` | Ephemeral PubSub |
| `ConcurrencyConflictReport` | `conflictId` | `entityId`, `entityType`, `attemptedVersionDigest`, `currentVersionDigest`, `conflictingDiff`, `createdAtUtc` | Ephemeral Response |
| `RebaseCommitRecord` | `rebaseId` | `entityId`, `baseVersion`, `targetVersion`, `rebasedBy`, `resolvedDiff`, `committedAtUtc` | Bất biến / Audit |

Sơ đồ xử lý Xung đột và Khóa mềm:
```
[Admin A opens Edit Page] ---> (Requests Soft Lease) ---> [Acquires EditLeaseToken (TTL 15m)]
                                                                    |
[Admin B opens Edit Page] ---> (Checks Soft Lease)   ---> [Read-Only Warning: Admin A Editing]
                                                                    |
[Admin A Commits]         ---> (CAS Verification)    ---> [Pass: Bumps Version to v1.1, Releases Lease]
                                                                    |
[Admin B Tries Commit]    ---> (CAS Verification)    ---> [FAIL: 409 Conflict (v1.0 != v1.1)]
                                                                    |
                                                                    v
                                                     [Returns Structural Diff vs v1.1]
                                                                    |
                                                                    v
                                                     [Admin B Reviews & Rebases Draft]
                                                                    |
                                                                    v
                                                     [Admin B Commits Rebased v1.2]
```

## 3. Cơ chế Khóa mềm Phiên biên tập (Pessimistic Soft Lease)

1. **Cấp Lease**: Khi Admin gửi yêu cầu `AcquireEditLease`, hệ thống kiểm tra Redis Key `lock:edit:{entityType}:{entityId}`. Nếu chưa có, tạo Key trỏ `adminId` kèm TTL $15$ phút và trả về `editLeaseToken`.
2. **Duy trì Heartbeat**: Trình duyệt Admin gửi heartbeat mỗi 60 giây để gia hạn TTL lease thêm 15 phút.
3. **Giải phóng Lease**: Lease tự động giải phóng khi Admin bấm "Lưu", "Hủy", đóng tab trình duyệt (beacon signal) hoặc hết thời gian TTL 15 phút không có heartbeat.
4. **Cướp quyền Lease (`Lease Takeover`)**: Nếu Admin A giữ lease nhưng treo máy, Admin B có vai trò cao hơn (R12/R13) có thể thực hiện lệnh `Force Release Lease` kèm lý do kiểm toán.

## 4. Giao thức Rebase dữ liệu khi xảy ra Xung đột (Conflict Rebase Protocol)

Khi một yêu cầu lưu bản thảo bị từ chối do lỗi $409\text{ Concurrency Conflict}$:
1. **Phân tích Vi sản**: Hệ thống tính toán vi sản giữa 3 bản ghi: `Base Ancestor Version`, `Current Published Version (Server)` và `Draft Version (Admin Client)`.
2. **Hiển thị giao diện Rebase (Diff Matrix)**: Trả về cho Client danh sách các trường bị xung đột:
   - Các trường Admin B sửa không trùng trường Server sửa $\to$ Tự động gợi ý gộp (`Auto-Merge Suggestion`).
   - Các trường cùng bị sửa $\to$ Đánh dấu xung đột cứng (`Hard Conflict`).
3. **Commit bản Rebase**: Admin B chọn giá trị cuối cùng cho từng trường xung đột, hệ thống tạo bản ghi `RebaseCommitRecord` và commit thành công `Version v1.2`.

## 5. Tín hiệu Hiện diện Thời gian thực (Real-Time Presence Signals)

- Khi có nhiều Admin truy cập vào cùng 1 trang quản lý thực thể, hệ thống sử dụng WebSocket/SSE push tín hiệu hiện diện:
  - `"User Admin-02 (R03 Content Admin) is currently viewing this record."`
  - `"User Admin-01 (R04 Learning Admin) is editing section: Vocabulary Meanings."`
- Nhờ tín hiệu hiện diện, các Admin chủ động trao đổi ngoài kênh để tránh tình trạng 2 người cùng biên tập 1 mục từ.

## 6. Bảo mật và Phân quyền

- **Thẩm quyền Force Release Lease**: Chỉ người giữ vai trò R12 Security Admin hoặc R13 System Owner mới có quyền giải phóng lease của Admin khác.
- **Audit Logging**: Mọi thao tác Force Release Lease, CAS Conflict, và Rebase Commit đều phải ghi log kiểm toán bất biến.

## 7. Regression Gate và Case tự kiểm

### 7.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CE-G01` | 100% thao tác lưu/duyệt nội dung và cấu hình phải truyền `expectedVersionDigest` để xác minh CAS. |
| `CE-G02` | Tuyệt đối cấm nguyên tắc ghi đè âm thầm (Last-Write-Wins) trong toàn bộ các Controller M11. |
| `CE-G03` | Khóa mềm `editLeaseToken` có TTL tối đa 15 phút và tự động giải phóng khi hết hiệu lực. |
| `CE-G04` | Xử lý xung đột $409\text{ Conflict}$ trả về đầy đủ vi sản cấu trúc (`Structural Diff`) phục vụ Rebase. |
| `CE-G05` | Tín hiệu hiện diện (`Presence Signal`) được gửi tới các Client đang xem cùng 1 thực thể. |
| `CE-G06` | Thao tác Force Release Lease bắt buộc yêu cầu vai trò cao hơn (R12/R13) và ghi log kiểm toán. |
| `CE-G07` | Đảm bảo tính nguyên tử (Atomic CAS) khi cập nhật pointer phiên bản trong SQL / Redis. |
| `CE-G08` | Mọi bản ghi Rebase thành công đều lưu vết `RebaseCommitRecord` phục vụ truy vết lịch sử. |
| `CE-G09` | Phân quyền và truy soát an toàn tuân thủ nghiêm ngặt ma trận vai trò `M11-PERM-1.0`. |
| `CE-G10` | 100% các test case tự kiểm CE21-01–20 đạt thành công trong bộ suite kiểm thử. |

### 7.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CE21-01` | Admin A mở trang sửa bộ từ M02 | Cấp `editLeaseToken` TTL 15m thành công |
| `CE21-02` | Admin B mở cùng bộ từ M02 khi Admin A đang giữ lease | Trả về thông báo Read-Only + thông tin hiện diện Admin A |
| `CE21-03` | Admin A lưu bản thảo thành công | Bumps version lên `v1.1`, tự động giải phóng lease |
| `CE21-04` | Admin B cố tình commit bản thảo dựa trên base `v1.0` | System reject với $409\text{ Concurrency Conflict}$ |
| `CE21-05` | Response $409$ trả về đúng vi sản giữa draft Admin B và `v1.1` | Trả về `Structural Diff` chính xác |
| `CE21-06` | Admin B thực hiện Rebase draft trên `v1.1` và commit | Commit thành công version `v1.2`, lưu `RebaseCommitRecord` |
| `CE21-07` | Admin A ngắt kết nối mạng 15 phút không gửi heartbeat | Lease tự động giải phóng khi hết TTL 15m |
| `CE21-08` | R12 Admin thực hiện Force Release Lease của Admin A | Giải phóng lease thành công, ghi log audit |
| `CE21-09` | User không phải R12/R13 thực hiện Force Release Lease | Deny 403 Forbidden |
| `CE21-10` | Hai Admin cùng gửi request commit trong cùng 1 millisecond | Một request CAS thắng, request còn lại nhận $409\text{ Conflict}$ |
| `CE21-11` | Heartbeat của Admin A gia hạn lease thành công | TTL của lease được reset về 15 phút |
| `CE21-12` | Admin A đóng tab trình duyệt (gửi beacon signal release) | Lease được giải phóng lập tức trong Redis |
| `CE21-13` | Thử update entity mà không truyền `expectedVersionDigest` | Reject request với lỗi `MISSING_EXPECTED_VERSION_DIGEST` |
| `CE21-14` | Kiểm tra tính bất biến của bản ghi `RebaseCommitRecord` | API deny mọi thao tác Update/Delete |
| `CE21-15` | Redis cache bị nổ trong khi đang giữ soft lease | Fallback sang DB atomic CAS, không gây crash ứng dụng |
| `CE21-16` | Admin B rebase 2 trường không trùng nhau | Hệ thống auto-merge 2 trường an toàn |
| `CE21-17` | Tải đồng thời 100 request CAS update trên các thực thể khác nhau | 100% request xử lý nguyên tử không rò rỉ lock |
| `CE21-18` | Broadcast tín hiệu hiện diện qua WebSocket cho 5 Admin | Cả 5 Admin nhận tín hiệu hiện diện chính xác |
| `CE21-19` | Admin cố tình sửa `rowVersion` trên Client | CAS verification detect tampering và reject |
| `CE21-20` | Kiểm thử hoàn tất luồng: Soft Lease $\to$ CAS Conflict $\to$ Rebase $\to$ Commit | Toàn bộ luồng thực thi chuẩn xác theo hợp đồng |

## 8. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-CE-I01` | Trong source `WordSoulApi`, các API Update hiện tại không kiểm tra `rowVersion` hay CAS | Có nguy cơ bị ghi đè âm thầm (Lost Update) | M11-T049 |
| `M11-CE-I02` | Chưa có cơ chế cấp khóa mềm `editLeaseToken` trong Redis khi biên tập | Hai Admin có thể cùng sửa 1 bản thảo mà không hay biết | M11-T049 |
| `M11-CE-I03` | Thiếu giao thức Rebase dữ liệu và API trả về `Structural Diff` khi gặp lỗi 409 | Quản trị viên không thể xử lý xung đột một cách văn minh | M11-T049 |
| `M11-CE-I04` | Thiếu tín hiệu hiện diện thời gian thực (`Editor Presence Signals`) | Thiếu trải nghiệm trải nghiệm cộng tác đa quản trị viên | M11-T049 |
| `M11-CE-I05` | Chưa có bảng lưu vết `RebaseCommitRecord` | Thiếu nhật ký kiểm toán phục vụ tra cứu lịch sử rebase | M11-T049 |

- `M11-CE-F01`: Triển khai `ConcurrencyControlService` và giao thức CAS cho toàn bộ API update (tiếp nhận: M11-T049).
- `M11-CE-F02`: Xây dựng dịch vụ quản lý Khóa mềm `SoftLeaseManager` trên Redis (tiếp nhận: M11-T049).
- `M11-CE-F03`: Triển khai `DiffAndRebaseEngine` xử lý xung đột 409 (tiếp nhận: M11-T049).
- `M11-CE-F04`: Thiết lập bộ kiểm thử tự động CE-G01–G10 và CE21-01–20 (tiếp nhận: M11-T049).
- `M11-CE-F05`: Thu thập bằng chứng runtime cho luồng xử lý chỉnh sửa đồng thời (tiếp nhận: M11-T049; A-G02/A-G03).

## 9. Tự kiểm M11-T021

- Đã thiết kế hoàn chỉnh `M11-CONCURRENT-EDITING-1.0` loại bỏ nguyên tắc Last-Write-Wins.
- Đã chốt cơ chế Optimistic Concurrency Control (OCC) bằng thuật toán CAS.
- Đã xây dựng cơ chế Khóa mềm phiên biên tập (`Pessimistic Soft Lease`) với TTL 15m trong Redis.
- Đã chốt Giao thức Rebase dữ liệu (`Conflict Rebase Protocol`) và giao diện hiển thị `Structural Diff` khi gặp $409\text{ Conflict}$.
- Đã xác lập 10 Regression Gates (`CE-G01`–`CE-G10`) và 20 Test Cases tự kiểm (`CE21-01`–`CE21-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 10. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chốt xử lý chỉnh sửa đồng thời M11-T021 | WSA-7K2 |
