# Thiết kế quy trình gửi duyệt M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-SUBMISSION-FLOW-1.0` |
| Task | M02-T029 |
| Đầu vào | M02-HEADWORD-QUALITY-1.0 (D-061), M02-ASSET-MODERATION-1.0 (D-072), M02-SET-LIFECYCLE-1.0 (D-077), M02-SET-VOCAB-OVERRIDE-1.0 (D-081), REL-04 |
| Phạm vi | Quy trình gửi yêu cầu duyệt Bộ từ vựng công khai, bộ kiểm tra tiền điều kiện (Pre-submission Checklist), khóa chỉnh sửa tự động và tạo Ticket Hàng chờ Duyệt M11 |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức và Quy trình Gửi duyệt Bộ từ vựng Công khai (`Public Set Submission Workflow & Gate`) thuộc M02, quản lý luồng chuyển trạng thái từ `Draft` sang `Submitted` để yêu cầu Ban biên tập (`ContentAdmin`) phê duyệt phát hành bộ từ lên catalog công khai.

- **Bộ kiểm tra Tiền điều kiện Gửi duyệt Cứng (`Pre-submission Quality & Copyright Gate`)**: Bộ từ vựng CHỈ ĐƯỢC PHÉP gửi duyệt khi vượt qua 100% các tiêu chí tiền điều kiện tự động:
  - $5 \le ItemCount \le 50$ (M02-T015).
  - $100\%$ mục từ thành phần có `QualityScore >= 80%` (M02-T006).
  - $100\%$ tài sản media đi kèm có `rightsCleared == true` (REL-04 / CT-01).
  - $100\%$ nội dung ghi đè ngữ nghĩa vỡ câu ví dụ qua bộ lọc AI Safety Screening (M02-T012).
- **Ràng buộc Khóa Chỉnh sửa khi Đang Duyệt (`Mutation Lock Invariant`)**: Ngay khi gửi duyệt thành công, bộ từ vựng tự động được khóa chỉnh sửa (`IsMutationLocked = true`). Tác giả KHÔNG được phép thêm/bớt từ, sửa tên hoặc đổi thứ tự từ trong suốt quá trình bộ từ nằm ở trạng thái `Submitted` hoặc `InReview`.
- **Định danh Ticket Duyệt Hàng chờ M11 (`Submission Ticket Generation`)**: Thao tác gửi duyệt tự động tạo một Ticket Duyệt Hàng chờ (`SubmissionTicketId`) trong Module M11, đính kèm thông tin tác giả, danh sách mục từ và ảnh chụp phiên bản (`revisionDigest`).
- **Quy tắc Cấm Tác giả Tự duyệt (`Self-Approval Guard`)**: Tác giả nộp bài tuyệt đối KHÔNG thể tự duyệt bài của mình. Ticket gửi duyệt chỉ được xử lý bởi một `ContentAdmin` độc lập.

## 2. Tiền điều kiện Tiêu chuẩn cho Yêu cầu Gửi duyệt (Pre-submission Checklist)

| STT | Điều kiện Kiểm tra | Thuộc tính Khảo sát | Lỗi trả về nếu Không đạt | Chi tiết Validation |
|---|---|---|---|---|
| 1 | Giới hạn Kích thước | $5 \le ItemCount \le 50$ | `SET_SIZE_OUT_OF_BOUNDS` | Bộ từ phải có từ 5 đến 50 từ |
| 2 | Điểm Chất lượng Mục từ | $100\%$ `QualityScore >= 80%` | `CONTAINS_SUBSTANDARD_HEADWORD` | Không chứa từ Substandard |
| 3 | Bản quyền Tài sản Media | $100\%$ `rightsCleared == true` | `ASSET_RIGHTS_NOT_CLEARED` | Không vi phạm bản quyền REL-04 |
| 4 | An toàn Nội dung Ghi đè | AI Safety Pass = `true` | `SAFETY_SCREENING_FAILED` | Không chứa từ ngữ độc hại |
| 5 | Thông tin Tiêu đề & Mô tả | `Title` $\ge 5$ ký tự, `Description` $\ge 20$ ký tự | `SET_METADATA_INCOMPLETE` | Tiêu đề và mô tả đầy đủ |
| 6 | Ảnh Bìa Bộ từ | `CoverImageUrl != null` & `rightsCleared == true` | `MISSING_SET_COVER_IMAGE` | Bộ từ công khai phải có cover |

## 3. Kiến trúc Bộ gửi Duyệt Bộ từ (SetSubmissionEngine Protocol)

```
[ContentCreator Clicks "Submit for Review"]
                    |
                    v
    [Execute Pre-submission Checklist]
                    |
         +----------+----------+
         | (Validation Failed) | (Validation Passed)
         v                     v
  [Return 400 Bad Request     [Open DB Transaction]
   with Error List]           - VocabularySet.Status = 'Submitted'
                              - VocabularySet.IsMutationLocked = true
                              - Generate revisionDigest
                              |
                              v
                      [Create M11 Submission Ticket]
                      - TicketId: "TCK-SUB-2026-0820-XXXX"
                      - AssignedTo: ContentAdmin Queue
                              |
                              v
                      [Publish SetSubmittedEvent]
```

## 4. Cấu trúc Response DTO Gửi duyệt (SetSubmissionResponseDto)

```json
{
  "vocabularySetId": 108,
  "submissionTicketId": "TCK-SUB-2026-0820-0012",
  "status": "Submitted",
  "isMutationLocked": true,
  "submittedBy": "USR-10024",
  "submittedAtUtc": "2026-08-20T10:00:00Z",
  "revisionDigest": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "checklistSummary": {
    "totalItems": 15,
    "qualityPassCount": 15,
    "copyrightPassCount": 15,
    "safetyPassCount": 15
  }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SF-G01` | Cấm gửi duyệt bộ từ nếu chưa vượt qua 100% tiêu chí tiền điều kiện trong Checklist. |
| `SF-G02` | Gửi duyệt thành công tự động chuyển bộ từ sang `Submitted` và đặt `IsMutationLocked = true`. |
| `SF-G03` | Cấm mọi thao tác thêm/bớt/sửa từ vựng trên bộ từ đang ở trạng thái `Submitted` hoặc `InReview`. |
| `SF-G04` | Gửi duyệt thành công tự động sinh `SubmissionTicketId` trong hàng chờ duyệt M11. |
| `SF-G05` | Cấm tác giả tự duyệt xuất bản bộ từ của chính mình (`Self-Approval Guard`). |
| `SF-G06` | Thao tác gửi duyệt chốt chính xác mã băm `revisionDigest` snapshots tại thời điểm gửi. |
| `SF-G07` | Hủy gửi duyệt (Rút bài) bởi tác giả đưa bộ từ về `Draft` và mở khóa `IsMutationLocked = false`. |
| `SF-G08` | Phân quyền thực hiện gửi duyệt tuân thủ ma trận vai trò M02-T016 (`CreatorId` hoặc `ContentAdmin`). |
| `SF-G09` | SLA thực thi API gửi duyệt và sinh Ticket M11 $< 60\text{ms}$. |
| `SF-G10` | 100% các test case tự kiểm SF29-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SF29-01` | Gửi duyệt Bộ từ `Draft` 15 từ đạt 100% tiêu chí quality & bản quyền | Chuyển `Submitted`, `IsMutationLocked = true`, tạo Ticket M11 |
| `SF29-02` | Thử gửi duyệt Bộ từ chỉ có 3 mục từ ($< 5$) | System reject với lỗi `SET_SIZE_OUT_OF_BOUNDS` |
| `SF29-03` | Thử gửi duyệt Bộ từ chứa 1 mục từ `QualityScore = 60%` | System reject với lỗi `CONTAINS_SUBSTANDARD_HEADWORD` |
| `SF29-04` | Thử gửi duyệt Bộ từ chứa 1 ảnh minh họa `rightsCleared = false` | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `SF29-05` | Tác giả thử thêm 1 từ vựng mới vào Bộ từ vừa gửi duyệt | System reject với lỗi `SET_IS_MUTATION_LOCKED` |
| `SF29-06` | Tác giả thử sửa tên Bộ từ đang nằm trong Hàng chờ Duyệt M11 | System reject với lỗi `SET_IS_MUTATION_LOCKED` |
| `SF29-07` | Tác giả bấm "Hủy gửi duyệt (Withdraw Submission)" | Chuyển bộ từ về `Draft`, mở khóa `IsMutationLocked = false` |
| `SF29-08` | Tác giả gửi lại bài duyệt sau khi chỉnh sửa thêm từ | Sinh `submissionTicketId` mới và `revisionDigest` mới |
| `SF29-09` | Kiểm tra Hàng chờ Duyệt M11 sau khi gửi duyệt thành công | Xuất hiện Ticket `TCK-SUB-2026-0820-XXXX` |
| `SF29-10` | Tác giả tự bấm nút Phê duyệt trên Ticket gửi duyệt của mình | Reject với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `SF29-11` | Thử gửi duyệt Bộ từ chưa có Ảnh bìa (`CoverImageUrl = null`) | System reject với lỗi `MISSING_SET_COVER_IMAGE` |
| `SF29-12` | Tải đồng thời 50 request gửi duyệt các bộ từ khác nhau | Response latency p95 $< 55\text{ms}$ |
| `SF29-13` | User không phải tác giả thử gọi API gửi duyệt Bộ từ Cá nhân | Deny 403 Forbidden (M02-T016) |
| `SF29-14` | User chưa đăng nhập thử gọi API gửi duyệt | Deny 401 Unauthorized |
| `SF29-15` | Xem vết Audit Log M11 sau khi gửi duyệt bộ từ thành công | Ghi nhận Audit Event `ACT-M11-04` với ticket ID |
| `SF29-16` | Gửi duyệt Bộ từ có 50 mục từ tối đa | Kiểm tra 50/50 mục từ đạt quality & rights, gửi duyệt thành công |
| `SF29-17` | Phân tích tham chiếu trước khi gửi duyệt bộ từ vựng | Quét các tham chiếu mục từ thành phần M02 (T020) |
| `SF29-18` | Thao tác gửi duyệt bị gián đoạn giữa chừng do lỗi DB | Rollback transaction, bộ từ giữ nguyên `Draft` và chưa khóa |
| `SF29-19` | Gửi duyệt Bộ từ Cá nhân bởi `ContentCreator` | Chuyển `Submitted`, hiển thị trên dashboard Admin |
| `SF29-20` | Kiểm thử hoàn tất luồng quy trình gửi duyệt M02-SET-SUBMISSION-FLOW-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SF-I01` | Entity `VocabularySet.cs` chưa có cờ `IsMutationLocked` | Tác giả vẫn có thể sửa nội dung bộ từ trong khi Admin đang duyệt | M02-T049 (Source task) |
| `M02-SF-I02` | Chưa có bộ kiểm tra Pre-submission Checklist tập trung | Rủi ro gửi duyệt các bộ từ chưa đạt tiêu chí quality hoặc bản quyền | M02-T049; REL-04 |
| `M02-SF-I03` | Thiếu luồng tự động tạo `SubmissionTicketId` trong M11 khi gửi duyệt | Hàng chờ duyệt Admin M11 không nhận được thông báo ticket | M02-T049; M11-T029 |
| `M02-SF-I04` | Thiếu tính năng "Hủy gửi duyệt (Withdraw)" dành cho tác giả | Tác giả muốn rút lại bài để sửa thêm phải chờ Admin từ chối | M02-T049 |
| `M02-SF-I05` | Chưa kiểm tra `CoverImageUrl` bắt buộc đối với bộ từ công khai | Bộ từ xuất bản công khai bị thiếu ảnh bìa gây xấu UI | M02-T049 |

- `M02-SF-F01`: Thêm `IsMutationLocked` vào `VocabularySet.cs` (tiếp nhận: M02-T049).
- `M02-SF-F02`: Triển khai `SetSubmissionService` với Pre-submission Checklist (tiếp nhận: M02-T049; REL-04).
- `M02-SF-F03`: Tích hợp tự động sinh Ticket Duyệt Hàng chờ M11 (tiếp nhận: M02-T049; M11-T029).
- `M02-SF-F04`: Thiết lập bộ kiểm thử tự động SF-G01–G10 và SF29-01–20 (tiếp nhận: M02 tasks).
- `M02-SF-F05`: Thu thập bằng chứng runtime cho luồng gửi duyệt bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T029

- Đã thiết kế hoàn chỉnh `M02-SET-SUBMISSION-FLOW-1.0` với Pre-submission Checklist 6 Bước.
- Đã chốt Ràng buộc Khóa Chỉnh sửa khi Đang Duyệt (`IsMutationLocked = true`).
- Đã quy định cơ chế Tự động Sinh Ticket Duyệt Hàng chờ M11 và Mã băm `revisionDigest`.
- Đã lồng ghép bảo vệ Cấm tự duyệt (`Self-Approval Guard`) và tính năng Hủy gửi duyệt (Withdraw).
- Đã xác lập 10 Regression Gates (`SF-G01`–`SF-G10`) và 20 Test Cases tự kiểm (`SF29-01`–`SF29-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế quy trình gửi duyệt M02-T029 | WSA-7K2 |
