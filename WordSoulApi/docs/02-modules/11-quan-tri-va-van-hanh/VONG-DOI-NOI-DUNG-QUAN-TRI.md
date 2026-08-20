# Vòng đời nội dung quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CONTENT-LIFECYCLE-1.0` |
| Task | M11-T019 |
| Đầu vào | M11-CHANGE-DECISION-1.0, M11-CROSS-CONTENT-MATRIX-1.0, M02-QUALITY-1.0, REL-04 |
| Phạm vi | Chuẩn hóa quy trình tạo, kiểm duyệt, lên lịch, xuất bản, thu hồi và lưu trữ nội dung quản trị chéo module |
| Tự kiểm | A-G02, A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa toàn bộ vòng đời quản lý nội dung quản trị (Content Lifecycle) đối với các thực thể nội dung được điều khiển bởi M11 (Bộ từ vựng M02, Quy tắc/Bài học M03, Nhiệm vụ/Thành tựu M07, Quy tắc phòng đấu M08, Mẫu thông báo M10).

- **Kiểm soát trạng thái bất biến**: Mọi sự kiện chuyển trạng thái nội dung phải được ghi lại dưới dạng event log bất biến (`ContentLifecycleEvent`), trỏ tới exact `contentId`, `contentVersion`, `changeDecisionId` và `actorId`.
- **Ràng buộc bản quyền và quyền tài sản (REL-04 / CT-01 / A-G03)**: Tuyệt đối KHÔNG ĐƯỢC chuyển nội dung sang trạng thái `approved` hoặc `published` nếu bản ghi tài sản số chưa đạt trạng thái xác minh bản quyền (`rightsCleared == true`). Vi phạm quy tắc này lập tức bị chặn theo CT-01.
- **Phân tách thẩm quyền Tác giả và Kiểm duyệt**: Người tạo nội dung (`author`) ở trạng thái `draft` KHÔNG ĐƯỢC tự phê duyệt (`approved`) chính nội dung của mình nếu thuộc quy định kiểm soát tăng cường EC-2/EC-3/EC-4.
- **Quy trình Thu hồi Khẩn cấp (`Emergency Recall Protocol`)**: Khi phát hiện nội dung đã công khai có sai sót nghiêm trọng, vi phạm bản quyền hoặc an toàn thông tin, hệ thống cho phép kích hoạt lệnh thu hồi khẩn (`Recall`), lập tức ẩn nội dung khỏi giao diện người học trong 60 giây và bảo toàn bằng chứng phục vụ điều tra.

## 2. Ephemeral & Durable Model cho Vòng đời Nội dung

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `ContentEntityRecord` | `contentId:contentVersion` | `domainModule`, `contentType`, `title`, `authorId`, `rightsCleared`, `licenseId`, `currentLifecycleState` | Immutable version |
| `ContentLifecycleEvent` | `eventId` | `contentId`, `contentVersion`, `fromState`, `toState`, `decisionId`, `actorId`, `reason`, `timestampUtc` | Append-only Audit |
| `ContentQualityReview` | `reviewId` | `contentId`, `contentVersion`, `reviewerId`, `qualityGateResults`, `rightsVerificationState`, `reviewComments` | Bất biến |
| `ContentRecallRecord` | `recallId` | `contentId`, `contentVersion`, `recalledBy`, `recallReason`, `recalledAtUtc`, `affectedUsersCount` | Bất biến / Audit |

Sơ đồ chuyển trạng thái nội dung (`Content Lifecycle State Machine`):
```
[Draft] ---> (Submit for Review) ---> [Submitted] ---> (Assign Reviewer) ---> [In Review]
                                                                                |
                       +--------------------------------------------------------+--------------------------------------------------------+
                       | (Reject)                                                                                                        | (Approve & Rights Cleared REL-04)
                       v                                                                                                                 v
                  [Rejected]                                                                                                        [Approved]
                       |                                                                                                                 |
                       +----------------------------------------------+------------------------------------------------------------------+
                                                                      | (Schedule Effective Interval)
                                                                      v
                                                                 [Scheduled]
                                                                      | (Effective From UTC Reached)
                                                                      v
                                                                 [Published]
                                                                      |
                       +----------------------------------------------+------------------------------------------------------------------+
                       | (Soft Deprecate)                                                                                                | (Emergency Recall CT-01 / Breach)
                       v                                                                                                                 v
                  [Deprecated]                                                                                                      [Recalled]
                       |                                                                                                                 |
                       +----------------------------------------------+------------------------------------------------------------------+
                                                                      | (Zero Active References & Retire Scan)
                                                                      v
                                                                  [Archived]
```

## 3. Quy tắc Chuyển trạng thái chi tiết

| Trạng thái từ | Trạng thái tới | Điều kiện kích hoạt & Kiểm tra an toàn | Quyền thực thi |
|---|---|---|---|
| `draft` | `submitted` | Đã điền đầy đủ metadata bắt buộc, kiểm tra cấu trúc hợp lệ (CV-01..12). | Tác giả (Author / Content Creator) |
| `submitted` | `in_review` | Phân công Reviewer có vai trò R03 Content Admin hoặc R04 Learning Admin. | Content Admin / System Auto |
| `in_review` | `approved` | Kiếm tra chất lượng đạt, **xác minh bản quyền REL-04 đạt (`rightsCleared == true`)**. | Reviewer (R03, R04, R12) |
| `in_review` | `rejected` | Nội dung vi phạm chất lượng hoặc bản quyền; ghi nhận lý do từ chối. | Reviewer (R03, R04) |
| `approved` | `scheduled` | Đã lên lịch thời gian có hiệu lực `[effectiveFromUtc, effectiveToUtc)` không xung đột T010. | Admin Scheduler (R02, R08) |
| `scheduled` | `published` | Đã đến thời điểm `effectiveFromUtc`, hệ thống cập nhật CAS pointer công khai. | System Worker / Auto |
| `published` | `deprecated` | Nội dung hết thời hạn áp dụng hoặc chuẩn bị thay thế bằng phiên bản mới. | Content Admin (R03, R04) |
| `published` | `recalled` | Kích hoạt Lệnh Thu hồi Khẩn cấp khi có báo cáo vi phạm bản quyền hoặc an toàn. | Security Admin (R12), Owner (R13) |
| `deprecated` | `archived` | Hoàn tất bài quét tham chiếu (`Reference Scan`), $0$ người dùng/phiên học đang sử dụng. | System Worker / Archive Job |

## 4. Giao thức Thu hồi Khẩn cấp (Emergency Recall Protocol)

1. **Khởi chạy Lệnh Thu hồi**: Quản trị viên R12/R13 gọi API Recall kèm `contentId`, `contentVersion` và `recallReason`.
2. **Ẩn tức thời (Instant Eviction)**: Hệ thống phát tín hiệu Invalidation evict Redis cache và vô hiệu hóa endpoint công khai của nội dung trong vòng 60 giây.
3. **Cách ly Trạng thái**: Chuyển `currentLifecycleState` thành `recalled`. Mọi yêu cầu đọc nội dung từ client bị từ chối với lỗi `CONTENT_RECALLED_FOR_SAFETY`.
4. **Bảo tồn Bằng chứng**: Toàn bộ dữ liệu bản thảo, nhật ký kiểm duyệt và lý do thu hồi được đóng gói vào `ContentRecallRecord` bất biến phục vụ điều tra.

## 5. Kiểm soát Bản quyền và Quyền Tài sản Số (REL-04 / CT-01 / A-G03 / A-G05)

- **Cổng G03-C01 / CT-01**: Cấm công khai nội dung chưa duyệt bản quyền. Validator chặn chuyển `approved` nếu `rightsCleared == false`.
- **Ranh giới G05-L01**: Metadata tài sản phương tiện không được chứa đường dẫn bí mật hoặc thông tin cá nhân.
- **Xác minh Giấy phép (`License Verification`)**: Mỗi tài sản phương tiện đi kèm nội dung phải có `licenseId` hợp lệ được đăng ký trong sổ quyền tài sản số `REL-04`.

## 6. Phân quyền và Kiểm soát Tăng cường

- **Role Separation**: Tác giả tạo `draft` không được tự phê duyệt `approved` nếu nội dung thuộc danh mục kiểm soát EC-3/EC-4.
- **Audit Logging**: Mọi hành động duyệt, từ chối, lên lịch, thu hồi phải tạo một bản ghi `ContentLifecycleEvent` lưu trữ lâu dài.

## 7. Regression Gate và Case tự kiểm

### 7.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CL-G01` | Mọi nội dung quản trị đều được quản lý theo đúng ma trận 8 trạng thái vòng đời chuẩn. |
| `CL-G02` | Cấm chuyển nội dung sang `approved` hoặc `published` nếu chưa đạt xác minh bản quyền REL-04. |
| `CL-G03` | Lệnh Thu hồi Khẩn cấp (`Recall`) ẩn nội dung khỏi cache/API công khai trong $\le 60$ giây. |
| `CL-G04` | Tác giả tạo bản thảo không thể tự phê duyệt nội dung của mình ở các danh mục EC-2/3/4. |
| `CL-G05` | Chuyển trạng thái `scheduled` bắt buộc kiểm tra xung đột lịch hiệu lực UTC (T010). |
| `CL-G06` | Lưu trữ `archived` bắt buộc qua bài quét tham chiếu chứng minh $0$ active session/user reading. |
| `CL-G07` | Mọi sự kiện chuyển trạng thái tạo bản ghi `ContentLifecycleEvent` bất biến phục vụ kiểm toán. |
| `CL-G08` | Nội dung ở trạng thái `rejected` hoặc `recalled` cấm hiển thị trên ứng dụng người học. |
| `CL-G09` | Phân quyền thực thi tuân thủ nghiêm ngặt ma trận vai trò và quyền `M11-PERM-1.0`. |
| `CL-G10` | 100% các test case tự kiểm CL19-01–20 đạt thành công trong bộ suite kiểm thử. |

### 7.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CL19-01` | Tạo bản thảo bộ từ M02 hợp lệ | Đăng ký ở trạng thái `draft` với `contentVersion = v1.0` |
| `CL19-02` | Thử chuyển `submitted` $\to$ `approved` khi `rightsCleared == false` | Reject request với lỗi `REL04_RIGHTS_NOT_CLEARED` |
| `CL19-03` | Phê duyệt nội dung M02 khi đã đạt kiểm duyệt chất lượng và REL-04 | Chuyển thành công sang `approved` |
| `CL19-04` | Tác giả tạo draft tự gọi API phê duyệt `approved` cho nội dung EC-3 | Deny request với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `CL19-05` | Lên lịch phát hành nội dung M07 trùng khoảng thời gian xung đột | Reject request với lỗi `SCHEDULE_CONFLICT_T010` |
| `CL19-06` | Đến mốc `effectiveFromUtc` của nội dung `scheduled` | Worker tự động chuyển nội dung sang `published` |
| `CL19-07` | Kích hoạt Lệnh Thu hồi Khẩn cấp (`Recall`) cho bài học M03 | Chuyển sang `recalled`, evict cache trong 60 giây |
| `CL19-08` | Người học truy vấn bài học M03 đang ở trạng thái `recalled` | Trả về 404 / `CONTENT_RECALLED_FOR_SAFETY` |
| `CL19-09` | Thử chuyển nội dung `published` sang `archived` khi còn 5 phiên học active | Reject request với lỗi `ACTIVE_REFERENCES_EXIST` |
| `CL19-10` | Chuyển nội dung sang `deprecated` và phát cảnh báo tới consumer | Phát warning trong log, ứng dụng chuẩn bị chuyển phiên bản |
| `CL19-11` | Từ chối bản thảo M07 do vi phạm chất lượng | Chuyển trạng thái `rejected`, ghi nhận lý do chi tiết |
| `CL19-12` | Thử chỉnh sửa trực tiếp nội dung đang ở trạng thái `published` | Deny request; bắt buộc tạo version bản thảo mới `v1.1` |
| `CL19-13` | Quét tham chiếu tự động hoàn tất với 0 active reference | Chuyển nội dung từ `deprecated` sang `archived` |
| `CL19-14` | User không có quyền `R03 Content Admin` thực hiện duyệt nội dung | Deny 403 Forbidden |
| `CL19-15` | Thử xóa cứng (Physical Delete) bản ghi `ContentLifecycleEvent` | DB deny operation vi phạm tính bất biến |
| `CL19-16` | Thu hồi nội dung M10 thông báo rác đã gửi | Dừng tiến trình gửi tin, đánh dấu `recalled` |
| `CL19-17` | Phê duyệt bộ từ M02 có tài sản âm thanh đi kèm | Verify `licenseId` âm thanh đạt REL-04 trước khi approve |
| `CL19-18` | Tải đồng thời 30 yêu cầu chuyển trạng thái vòng đời nội dung | Hệ thống xử lý mượt mà, lưu event log chính xác |
| `CL19-19` | Tra cứu lịch sử vòng đời của một bộ từ M02 | Trả về danh sách đầy đủ các sự kiện từ Draft $\to$ Published |
| `CL19-20` | Kiểm thử hoàn tất vòng đời đầy đủ của một thực thể nội dung | Toàn bộ quy tắc an toàn và kiểm toán được thực thi đúng |

## 8. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-CL-I01` | Trong source `WordSoulApi`, các thực thể nội dung (VocabularySet, LearningSession) thiếu state machine vòng đời chuẩn | Rủi ro phát hành nội dung chưa qua kiểm duyệt | M02 tasks; M11-T049 |
| `M11-CL-I02` | Chưa có cơ chế tích hợp kiểm tra bản quyền REL-04 trước khi duyệt nội dung | Rủi ro vi phạm bản quyền khi công khai bài học/mục từ | M02 tasks; M11-T049 |
| `M11-CL-I03` | Thiếu Lệnh Thu hồi Khẩn cấp (`Emergency Recall Protocol`) ẩn nội dung trong 60 giây | Không thể ngăn chặn kịp thời sự cố nội dung độc hại/lỗi | M11-T049 |
| `M11-CL-I04` | Chưa có bảng lưu trữ bất biến `ContentLifecycleEvent` và `ContentRecallRecord` | Thiếu vết kiểm toán phục vụ tra cứu lịch sử nội dung | M11-T049 |
| `M11-CL-I05` | Chưa có quy định chặn tự phê duyệt (`Self-Approval Guard`) cho tác giả nội dung | Rủi ro lạm dụng quyền tạo và tự duyệt nội dung vi phạm | M11-T049 |

- `M11-CL-F01`: Triển khai `ContentLifecycleManager` và State Machine cho các thực thể nội dung (tiếp nhận: M02 tasks; M11-T049).
- `M11-CL-F02`: Tích hợp bộ kiểm tra bản quyền REL-04 vào workflow duyệt nội dung (tiếp nhận: M02 tasks; M11-T049).
- `M11-CL-F03`: Xây dựng dịch vụ Thu hồi Khẩn cấp (`Emergency Recall Service`) (tiếp nhận: M11-T049).
- `M11-CL-F04`: Thiết lập bộ kiểm thử tự động CL-G01–G10 và CL19-01–20 (tiếp nhận: M11-T049).
- `M11-CL-F05`: Thu thập bằng chứng runtime cho luồng chuyển trạng thái vòng đời nội dung (tiếp nhận: M11-T049; A-G02/A-G03/REL-04).

## 9. Tự kiểm M11-T019

- Đã thiết kế hoàn chỉnh `M11-CONTENT-LIFECYCLE-1.0` bao phủ 8 trạng thái vòng đời nội dung chuẩn.
- Đã chốt chặt chẽ các điều kiện chuyển trạng thái và cổng xác minh bản quyền REL-04 / CT-01.
- Đã xây dựng Giao thức Thu hồi Khẩn cấp (`Emergency Recall Protocol`) ẩn nội dung khỏi cache/API trong 60s.
- Đã xác lập quy tắc chặn tự phê duyệt (`Self-Approval Guard`) cho tác giả nội dung.
- Đã xác lập 10 Regression Gates (`CL-G01`–`CL-G10`) và 20 Test Cases tự kiểm (`CL19-01`–`CL19-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 10. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa vòng đời nội dung quản trị M11-T019 | WSA-7K2 |
