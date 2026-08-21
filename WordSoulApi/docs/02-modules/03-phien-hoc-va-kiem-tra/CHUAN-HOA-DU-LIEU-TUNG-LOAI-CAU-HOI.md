# Chuẩn hóa dữ liệu từng loại câu hỏi M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-QUESTION-DATA-TYPES-1.0` |
| Task | M03-T019 |
| Đầu vào | M02-LESSON-CONTENT-1.0 (M02-T009-A), M03-NEW-LEARNING-FLOW-1.0 (M03-T015) |
| Phạm vi | Chuẩn hóa DTO và cấu trúc dữ liệu cho 4 loại câu hỏi chính: Flashcard, Multiple Choice, Fill-in-the-blank, Audio Listening Quiz |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cấu trúc DTO dữ liệu của các dạng bài tập/câu hỏi trong M03.

- **Bảo mật Dữ liệu Đáp án (`Answer Security Invariant`)**: 100% Payload DTO gửi xuống Client cho các loại câu hỏi kiểm tra (Quiz/Fill-in) CẤM chứa thông tin đáp án đúng trực tiếp (ví dụ: `CorrectOptionId` hoặc `CorrectText`). Việc chấm điểm BẮT BUỘC thực hiện tại Server backend.
- **Tính Đầy đủ Tài sản Hỗ trợ (`Asset Completeness Invariant`)**: DTO câu hỏi dạng Nghe (`Audio Listening`) phải chứa URL âm thanh MP3 hợp lệ từ M12 Asset Catalog. Nếu tài sản âm thanh bị lỗi/thiếu, hệ thống tự động suy giảm sang dạng văn bản theo M02-T013.

## 2. Bảng Phân loại Loại Câu hỏi và DTO Payload (Question Type Registry)

| Mã Loại | Tên Loại Bài tập | Mô tả | DTO Payload chính | Chấm điểm tại |
|---|---|---|---|---|
| `FLASHCARD` | Thẻ học liệu | Màn hình xem từ, phát âm, ví dụ | `VocabularyCanonical`, `IPA`, `SenseMeaning`, `AudioUrl`, `ExampleSentence` | N/A (Xem) |
| `MULTIPLE_CHOICE` | Trắc nghiệm | Chọn 1 nghĩa đúng trong 4 phương án | `QuestionText`, `OptionsList [OptionId, OptionText]` | **Server** |
| `FILL_IN_BLANK` | Điền từ | Điền từ còn thiếu vào câu ví dụ | `SentenceWithBlank`, `HintFirstLetter`, `MaxChars` | **Server** |
| `AUDIO_LISTENING` | Nghe và chọn | Nghe phát âm và chọn nghĩa/từ đúng | `AudioUrl`, `OptionsList [OptionId, OptionText]` | **Server** |

## 3. Dynamic Question DTO Envelopes

```csharp
public class SessionStepDto
{
    public Guid StepId { get; set; }
    public QuestionType Type { get; set; } // FLASHCARD, MULTIPLE_CHOICE, FILL_IN_BLANK, AUDIO_LISTENING
    public Guid VocabularySenseId { get; set; }
    public string PromptText { get; set; }
    public string? AudioUrl { get; set; }
    
    // Dữ liệu lựa chọn cho Trắc nghiệm (CẤM chứa CorrectFlag)
    public List<QuestionOptionDto>? Options { get; set; }
    
    // Dữ liệu gợi ý cho Điền từ
    public string? BlankSentence { get; set; }
    public string? FirstLetterHint { get; set; }
}

public class QuestionOptionDto
{
    public Guid OptionId { get; set; }
    public string OptionText { get; set; }
}
```

## 4. Regression Gates và Test Cases

### 4.1. Regression Gates
- `QD-G01`: 100% DTO `SessionStepDto` loại bài tập trắc nghiệm không chứa trường đáp án đúng.
- `QD-G02`: Chấm điểm bài tập thực hiện 100% ở Server backend qua endpoint `POST /api/v1/sessions/submit-answer`.

### 4.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QD19-01` | Lấy dữ liệu 10 câu hỏi trắc nghiệm trong phiên học | Trả về danh sách options không có thông tin `CorrectOptionId`. |
| `QD19-02` | Gửi đáp án `OptionId` lên Server | Server kiểm tra CSDL và trả về `IsCorrect = true / false` kèm giải thích. |
| `QD19-03` | Kiểm thử hoàn tất luồng M03-QUESTION-DATA-TYPES-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 5. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-QD-F01` | Cần tách interface `IQuestionAnswerValidator` | Đảm bảo kiến trúc extensible cho các dạng bài tập mới | M03-T028 |

## 6. Tự kiểm M03-T019
- Đã đặc tả chuẩn hóa dữ liệu từng loại câu hỏi M03-T019.
- Ghi nhận 2 Regression Gates (`QD-G01`–`QD-G02`) và 3 Test Cases (`QD19-01`–`QD19-03`).

## 7. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa dữ liệu từng loại câu hỏi M03-T019 | WSA-7K2 |
