# Thiết kế yêu cầu sửa và từ chối M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-REJECTION-REVISION-1.0` |
| Task | M02-T031 |
| Đầu vào | M02-SET-SUBMISSION-FLOW-1.0 (D-083), M02-PUBLIC-MODERATION-CHECKLIST-1.0 (D-084), REL-04 |
| Phạm vi | Giao thức xử lý hai nhánh phản hồi của Quản trị viên Biên tập: Yêu cầu Chỉnh sửa (`Request Changes`) và Từ chối vĩnh viễn (`Reject`), phân loại danh mục lý do và đếm số lượt nộp lại |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức Yêu cầu Chỉnh sửa và Từ chối Bộ từ vựng (`Vocabulary Set Rejection & Revision Engine`) thuộc M02, quy định phản hồi của Ban biên tập (`ContentAdmin`) khi một Bộ từ vựng gửi duyệt không đạt các tiêu chuẩn trong Checklist M02-T030.

- **Phân định Rõ ràng giữa Tạm hoãn và Từ chối Vĩnh viễn (`Action Separation Invariant`)**:
  - `REQUEST_CHANGES` (Yêu cầu Chỉnh sửa): Áp dụng cho các lỗi có thể khắc phục (sai chính tả, câu ví dụ chưa rõ nghĩa, ảnh bị mờ). Bộ từ vựng chuyển về `Draft`, mở khóa `IsMutationLocked = false`. Tác giả được phép sửa và nộp lại.
  - `REJECT` (Từ chối Vĩnh viễn): Áp dụng cho vi phạm chính sách nghiêm trọng (ngôn từ độc hại, cố tình xâm phạm bản quyền REL-04 / CT-01). Bộ từ chuyển sang `Archived`, CẤM tác giả gửi duyệt lại bộ từ này.
- **Ràng buộc Danh mục Lý do Chuẩn hóa (`Standardized Rejection Reason Invariant`)**: Mọi quyết định phản hồi bắt buộc đính kèm ít nhất 1 mã lý do chuẩn thuộc `RejectionReasonCatalog` và lời nhắn giải thích chi tiết (`ModeratorNotes` $\ge 20$ ký tự).
- **Giới hạn Số lần Nộp lại Tối đa ($N \le 3$)**: Mỗi Bộ từ vựng chỉ được phép nộp lại tối đa 3 lần cho cùng một Ticket gửi duyệt. Nếu sau 3 lần sửa vẫn không đạt, hệ thống tự động khóa tính năng gửi duyệt của bộ từ đó và chuyển sang hàng chờ phản ánh đặc biệt M11.
- **Lưu vết Lịch sử Phản hồi Bất biến (`Feedback Audit Log Invariant`)**: $100\%$ quyết định phản hồi được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-04`), đính kèm `ScorecardId`, `RejectionReasonCode`, `ModeratorNotes` và `ResubmissionCount`.

## 2. Danh mục Lý do Phản hồi Chuẩn hóa (RejectionReasonCatalog)

| Mã Lý do (`ReasonCode`) | Loại Hành động | Mô tả Chi tiết Lỗi | Khả năng Sửa |
|---|---|---|---|
| `ERR_TYPO_GRAMMAR` | `REQUEST_CHANGES` | Sai chính tả, sai ngữ pháp hoặc dịch nghĩa chưa chuẩn | CÓ |
| `ERR_IMPROPER_EXAMPLE` | `REQUEST_CHANGES` | Câu ví dụ minh họa không đúng ngữ cảnh hoặc tối nghĩa | CÓ |
| `ERR_LOW_IMAGE_QUALITY` | `REQUEST_CHANGES` | Ảnh minh họa mờ, méo hình hoặc không đạt chuẩn WebP | CÓ |
| `ERR_INCOMPLETE_SET` | `REQUEST_CHANGES` | Thiếu mô tả, sai thẻ phân loại hoặc thiếu ảnh bìa | CÓ |
| `ERR_COPYRIGHT_VIOLATION`| `REJECT` | Vi phạm bản quyền hình ảnh/âm thanh nặng (REL-04) | KHÔNG |
| `ERR_POLICY_TOXICITY` | `REJECT` | Chứa ngôn từ kích động, thù hận, vi phạm pháp luật | KHÔNG |
| `ERR_MAX_REVISIONS_EXCEEDED`| `REJECT` | Quá 3 lần nộp lại nhưng vẫn không đạt yêu cầu | KHÔNG |

## 3. Quy trình Xử lý Yêu cầu Chỉnh sửa và Từ chối (Rejection Workflow)

```
[ContentAdmin Evaluates Checklist (M02-T030)]
                    |
          +---------+---------+
          | (Request Changes) | (Reject Permanently)
          v                   v
  [Set Status = Draft]   [Set Status = Archived]
  - IsMutationLocked=false - IsMutationLocked=false
  - ResubmissionCount++    - Block Resubmission
  - Send Email & Push      - Send Rejection Alert
          |                   |
          v                   v
  [Author Fixes Set &    [Record Audit Event ACT-M11-04]
   Resubmits (N <= 3)]
```

## 4. Cấu trúc Response DTO Phản hồi Yêu cầu Duyệt (ModerationFeedbackDto)

```json
{
  "submissionTicketId": "TCK-SUB-2026-0820-0012",
  "vocabularySetId": 108,
  "action": "REQUEST_CHANGES",
  "resubmissionCount": 2,
  "maxAllowedResubmissions": 3,
  "reasonCodes": ["ERR_TYPO_GRAMMAR", "ERR_IMPROPER_EXAMPLE"],
  "moderatorNotes": "Vui lòng kiểm tra lại câu ví dụ của từ 'bank' ở mục thứ 3, đang bị sai thì quá khứ. Ảnh minh họa từ 'account' bị mờ.",
  "itemLevelFeedback": [
    { "vocabularyId": 1024, "displayOrder": 3, "issueNote": "Sai thì động từ trong câu ví dụ" },
    { "vocabularyId": 1029, "displayOrder": 8, "issueNote": "Ảnh minh họa bị mờ độ phân giải thấp" }
  ],
  "evaluatedAtUtc": "2026-08-20T10:30:00Z"
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RR-G01` | Yêu cầu Chỉnh sửa (`REQUEST_CHANGES`) chuyển bộ từ về `Draft` và mở khóa `IsMutationLocked = false`. |
| `RR-G02` | Từ chối Vĩnh viễn (`REJECT`) chuyển bộ từ về `Archived` và CẤM tác giả nộp lại bộ từ đó. |
| `RR-G03` | Cấm gửi quyết định phản hồi nếu không có `RejectionReasonCode` hoặc `ModeratorNotes < 20` ký tự. |
| `RR-G04` | Mỗi lần Yêu cầu Chỉnh sửa tự động tăng `ResubmissionCount` của Bộ từ lên $+1$. |
| `RR-G05` | Khi `ResubmissionCount > 3`, hệ thống tự động chặn gửi duyệt và chuyển mã lý do sang `ERR_MAX_REVISIONS_EXCEEDED`. |
| `RR-G06` | Cấm tác giả tự gửi phản hồi từ chối trên bộ từ do chính mình tạo ra (`Self-Approval Guard`). |
| `RR-G07` | 100% quyết định phản hồi ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-04`) và phát thông báo PUSH. |
| `RR-G08` | Danh mục phản hồi chi tiết tới từng mục từ (`itemLevelFeedback`) được hiển thị rõ ràng trên UI tác giả. |
| `RR-G09` | SLA thực thi API gửi phản hồi từ chối/yêu cầu sửa và phát thông báo $< 50\text{ms}$. |
| `RR-G10` | 100% các test case tự kiểm RR31-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RR31-01` | `ContentAdmin` gửi Yêu cầu Chỉnh sửa cho lỗi chính tả | Chuyển `Draft`, `IsMutationLocked = false`, `ResubmissionCount = 1` |
| `RR31-02` | `ContentAdmin` gửi Từ chối Vĩnh viễn cho vi phạm bản quyền nặng | Chuyển `Archived`, khóa gửi duyệt vĩnh viễn |
| `RR31-03` | Thử gửi phản hồi `REQUEST_CHANGES` với `moderatorNotes` dài 10 ký tự ($< 20$) | Reject với lỗi `MODERATOR_NOTES_TOO_SHORT` |
| `RR31-04` | Thử gửi phản hồi không đính kèm mã lý do `reasonCodes` | Reject với lỗi `MISSING_REJECTION_REASON_CODE` |
| `RR31-05` | Tác giả sửa lại bộ từ và gửi duyệt lần thứ 2 | Nộp bài thành công, giữ `ResubmissionCount = 1` |
| `RR31-06` | Admin gửi Yêu cầu Chỉnh sửa lần thứ 3 | Chuyển `Draft`, `ResubmissionCount = 3` |
| `RR31-07` | Tác giả sửa lại bài và gửi duyệt lần thứ 4 | Reject với lỗi `ERR_MAX_REVISIONS_EXCEEDED` |
| `RR31-08` | Tác giả mở giao diện xem thông tin bài bị Yêu cầu Chỉnh sửa | Hiển thị đầy đủ mã lỗi và ghi chú phản hồi từng item |
| `RR31-09` | Tác giả tự gửi request phản hồi từ chối bài của mình | System reject với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `RR31-10` | Kiểm tra thông báo PUSH gửi đến ứng dụng tác giả sau khi có phản hồi | Nhận thông báo "Bộ từ của bạn cần chỉnh sửa: Sai chính tả..." |
| `RR31-11` | Tra cứu vết Audit Log M11 sau khi gửi phản hồi từ chối | Ghi nhận Audit Event `ACT-M11-04` với đính kèm Feedback DTO |
| `RR31-12` | Tải đồng thời 50 request gửi phản hồi từ chối trên 50 bộ từ | Response latency p95 $< 55\text{ms}$ |
| `RR31-13` | User không phải Admin thử gọi API gửi phản hồi từ chối | Deny 403 Forbidden (M01-ROLE-MATRIX-1.0) |
| `RR31-14` | User chưa đăng nhập thử gọi API gửi phản hồi từ chối | Deny 401 Unauthorized |
| `RR31-15` | Sửa lại bộ từ bị từ chối vĩnh viễn (`Archived`) | System reject vĩnh viễn không cho sửa bộ từ `Archived` |
| `RR31-16` | Gửi phản hồi đính kèm 5 phản hồi chi tiết ở cấp mục từ (`itemLevelFeedback`) | Lưu trữ chính xác mảng `itemLevelFeedback` trong DTO |
| `RR31-17` | Phân tích tham chiếu trước khi lưu kho vĩnh viễn bộ từ | Quét các active session M03 đang mở bộ từ (T020) |
| `RR31-18` | Thao tác gửi phản hồi từ chối bị gián đoạn do lỗi DB | Rollback transaction, bộ từ giữ nguyên trạng thái `Submitted` |
| `RR31-19` | `SuperAdmin` thực hiện hủy lệnh từ chối vĩnh viễn để mở lại `Draft` | Chuyển lại `Draft`, ghi log khôi phục quản trị |
| `RR31-20` | Kiểm thử hoàn tất luồng thiết kế yêu cầu sửa và từ chối M02-REJECTION-REVISION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-RR-I01` | Entity `VocabularySet.cs` chưa có thuộc tính `ResubmissionCount` | Không đếm và giới hạn được số lần nộp lại bài của tác giả | M02-T049 (Source task) |
| `M02-RR-I02` | Chưa có bảng `RejectionReasonCatalog` chuẩn hóa mã lỗi | Lời nhắn từ chối của Admin mang tính ngẫu nhiên, khó phân loại | M02-T049 |
| `M02-RR-I03` | Thiếu validation độ dài tối thiểu 20 ký tự cho `moderatorNotes` | Admin có thể gửi lời nhắn quá ngắn không rõ ý cho tác giả | M02-T049 |
| `M02-RR-I04` | Thiếu cơ chế phản hồi chi tiết tới từng mục từ (`itemLevelFeedback`) | Tác giả không biết mục từ cụ thể nào bị sai để sửa | M02-T049 |
| `M02-RR-I05` | Chưa khóa vĩnh viễn các bộ từ bị Từ chối thẳng (`Archived`) | Tác giả vẫn có thể gửi duyệt lại bộ từ vi phạm bản quyền nặng | M02-T049; REL-04 |

- `M02-RR-F01`: Thêm `ResubmissionCount` vào `VocabularySet.cs` và validation $N \le 3$ (tiếp nhận: M02-T049).
- `M02-RR-F02`: Triển khai `RejectionReasonCatalogEnum` và `SetRejectionService` (tiếp nhận: M02-T049).
- `M02-RR-F03`: Tích hợp `ItemLevelFeedbackDto` cho giao diện phản hồi Admin (tiếp nhận: M02-T049).
- `M02-RR-F04`: Thiết lập bộ kiểm thử tự động RR-G01–G10 và RR31-01–20 (tiếp nhận: M02 tasks).
- `M02-RR-F05`: Thu thập bằng chứng runtime cho luồng yêu cầu sửa và từ chối M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T031

- Đã thiết kế hoàn chỉnh `M02-REJECTION-REVISION-1.0` với Giao thức Phân định 2 Nhánh Phản hồi.
- Đã chốt Ràng buộc Danh mục Lý do Chuẩn hóa (`RejectionReasonCatalog`) và độ dài tối thiểu 20 char.
- Đã quy định Giới hạn Số lần Nộp lại Tối đa ($N \le 3$) và Phản hồi Chi tiết theo mục từ (`itemLevelFeedback`).
- Đã lồng ghép bảo vệ Cấm tự duyệt (`Self-Approval Guard`) và Lưu vết Sổ Kiểm toán M11 (`ACT-M11-04`).
- Đã xác lập 10 Regression Gates (`RR-G01`–`RR-G10`) và 20 Test Cases tự kiểm (`RR31-01`–`RR31-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế yêu cầu sửa và từ chối M02-T031 | WSA-7K2 |
