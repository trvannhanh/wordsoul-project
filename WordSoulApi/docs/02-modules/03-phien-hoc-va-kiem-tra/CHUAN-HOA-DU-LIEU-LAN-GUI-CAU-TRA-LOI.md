# Chuẩn hóa dữ liệu lần gửi câu trả lời M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SUBMIT-ANSWER-DATA-1.0` |
| Task | M03-T024 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-QUESTION-DATA-TYPES-1.0 (M03-T019) |
| Phạm vi | Cấu trúc DTO request gửi đáp án, validation dữ liệu đầu vào và nguyên tắc không tin thời gian thiết bị tuyệt đối |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cấu trúc dữ liệu và logic xác thực cho API gửi đáp án (`SubmitAnswer`).

- **Cấm Tin Thời gian Thiết bị Tuyệt đối (`Zero Client Clock Trust Invariant`)**: Thời gian suy nghĩ/phản hồi (`ResponseDurationMs`) gửi từ Client BẮT BUỘC phải được Server kiểm tra lại với mốc thời gian phát câu hỏi (`StepServedAtUtc`). Nếu thời gian phản hồi bất thường ($< 200\text{ms}$ hoặc vượt quá thời lượng phiên), hệ thống sẽ kẹp hoặc đánh dấu nghi vấn gian lận.
- **Tính Bắt buộc của Tham số Request (`Mandatory Submit Params Invariant`)**: Request gửi đáp án BẮT BUỘC chứa: `SessionId`, `StepId`, `ClientSubmissionToken` (chống lặp) và câu trả lời (`SelectedOptionId` hoặc `InputText`).

## 2. Dynamic Submit Answer DTO Envelope

```csharp
public class SubmitAnswerRequestDto
{
    public Guid SessionId { get; set; }
    public Guid StepId { get; set; }
    public string ClientSubmissionToken { get; set; } // GUID duy nhất cho lần bấm gửi
    
    public Guid? SelectedOptionId { get; set; } // Dùng cho Trắc nghiệm
    public string? SubmittedText { get; set; } // Dùng cho Điền từ
    
    public long ClientResponseDurationMs { get; set; }
}

public class SubmitAnswerResultDto
{
    public bool IsCorrect { get; set; }
    public string ExplanationText { get; set; }
    public bool IsSessionCompleted { get; set; }
    public int RemainingSteps { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SA-G01`: 100% request thiếu `SessionId` hoặc `StepId` bị từ chối với lỗi HTTP 400 `INVALID_SUBMIT_PAYLOAD`.
- `SA-G02`: Thời gian suy nghĩ Client gửi $< 200\text{ms}$ tự động bị hệ thống kẹp về minimum $200\text{ms}$ để tránh gian lận speed hack.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SA24-01` | Gửi đáp án hợp lệ cho câu hỏi trắc nghiệm | Trả về `SubmitAnswerResultDto` chứa `IsCorrect = true` và số câu còn lại. |
| `SA24-02` | Client gửi `ClientResponseDurationMs = 1ms` (giả lập hack) | Server kẹp thời gian phản hồi về $200\text{ms}$ khi lưu vết. |
| `SA24-03` | Kiểm thử hoàn tất luồng M03-SUBMIT-ANSWER-DATA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SA-F01` | Thêm thuộc tính `StepServedAtUtc` vào bản ghi phiên trong Redis | Đảm bảo tính toán chính xác thời gian suy nghĩ tại Server | M03-T025 |

## 5. Tự kiểm M03-T024
- Đã đặc tả chuẩn hóa dữ liệu lần gửi câu trả lời M03-T024.
- Ghi nhận 2 Regression Gates (`SA-G01`–`SA-G02`) và 3 Test Cases (`SA24-01`–`SA24-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa dữ liệu lần gửi câu trả lời M03-T024 | WSA-7K2 |
