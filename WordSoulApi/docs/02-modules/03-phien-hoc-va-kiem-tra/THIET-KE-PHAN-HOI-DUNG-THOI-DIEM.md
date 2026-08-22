# Thiết kế phản hồi đúng thời điểm M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-TIMELY-FEEDBACK-1.0` |
| Task | M03-T032 |
| Đầu vào | M03-NEW-LEARNING-FLOW-1.0 (M03-T015), M03-TEXT-ANSWER-NORMALIZATION-1.0 (M03-T028) |
| Phạm vi | Quy định thời điểm hiển thị phản hồi (Immediate Feedback), cấu trúc giải thích câu sai (Error Explanation Envelope) và ngăn ngừa tiết lộ trước đáp án |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế trả về phản hồi (`Feedback Engine`) ngay lập tức sau khi người học gửi câu trả lời trong M03.

- **Phản hồi Tức thì Sau khi Chấm (`Immediate Post-Submit Feedback Invariant`)**:
  - Phản hồi đúng/sai, âm thanh chúc mừng/nhắc nhở và đáp án chuẩn CHỈ ĐƯỢC hiển thị TỨC THÌ ngay sau khi client nhận kết quả HTTP 200 OK từ API chấm điểm `SubmitAnswer`.
  - Tuyệt đối CẤM tiết lộ đáp án đúng trong DTO câu hỏi trước khi người học gửi câu trả lời (`Server-side Hidden Answer Invariant`).
- **Nội dung Giải thích Định hướng Sửa sai (`Constructive Feedback Content Invariant`)**:
  - Khi người học trả lời sai, phản hồi bắt buộc bao gồm:
    1. Trạng thái `INCORRECT`.
    2. Đáp án đúng chuẩn (`CorrectAnswer`).
    3. Nghĩa tiếng Việt của nét nghĩa đang kiểm tra.
    4. Gợi ý nguyên nhân sai (ví dụ: gõ nhầm chữ cái, nhầm loại từ, chọn sai nghĩa).

## 2. Cấu trúc Phản hồi Chấm điểm (Feedback Response Envelope)

```csharp
public class AnswerFeedbackDto
{
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public AnswerStatus Status { get; set; } // EXACT_MATCH, ALMOST_CORRECT, INCORRECT
    public string CorrectAnswer { get; set; }
    public string MeaningExplanation { get; set; }
    public string? AudioPhoneticUrl { get; set; }
    public string? FeedbackHintMessage { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TF-G01`: 100% DTO câu hỏi trả về trước khi làm bài KHÔNG chứa trường `CorrectAnswer` (bảo mật đáp án phía server).
- `TF-G02`: Kết quả phản hồi trả về đầy đủ đáp án đúng và lời giải thích nghĩa khi trả lời sai.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TF32-01` | Người học gửi câu trả lời sai cho câu trắc nghiệm | API trả HTTP 200 OK với `IsCorrect = false`, `CorrectAnswer = "buy"`, và lời giải thích nghĩa. |
| `TF32-02` | Khởi tạo DTO câu hỏi kiểm tra | Kiểm tra JSON response không chứa trường `CorrectAnswer`. |
| `TF32-03` | Kiểm thử hoàn tất luồng M03-TIMELY-FEEDBACK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-TF-F01` | Cần tách `QuestionPromptDto` (không có đáp án) và `AnswerFeedbackDto` (có đáp án) | Đảm bảo chống gian lận soi code client | M03-T019 |

## 5. Tự kiểm M03-T032
- Đã hoàn thành đặc tả `M03-TIMELY-FEEDBACK-1.0`.
- Chốt nguyên tắc bảo mật đáp án phía server và cấu hình phản hồi tức thì.
- Ghi nhận 2 Regression Gates (`TF-G01`–`TF-G02`) và 3 Test Cases (`TF32-01`–`TF32-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế phản hồi đúng thời điểm M03-T032 | WSA-7K2 |
