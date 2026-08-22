# Chuẩn hóa kết quả hoàn thành từng từ M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-ITEM-COMPLETION-RESULT-1.0` |
| Task | M03-T035 |
| Đầu vào | M03-TEXT-ANSWER-NORMALIZATION-1.0 (M03-T028) đến M03-INITIAL-RECALL-CAPTURE-1.0 (M03-T034), M04-SESSION-RESULT-CONTRACT-1.0 (M04-T006) |
| Phạm vi | Đóng băng DTO kết quả hoàn thành của từng mục từ vựng trong phiên (`CompletedItemResultDto`), bao gồm bằng chứng gợi nhớ đầu tiên (`InitialRecall`), tổng số lần thử, sử dụng gợi ý và tốc độ làm bài |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa cấu trúc DTO kết quả hoàn thành của từng mục từ vựng (`CompletedItemResultDto`) khi chốt phiên học trong M03.

- **Tính Bất biến của Bằng chứng Gợi nhớ Đầu tiên (`Immutable Initial Recall Invariant`)**:
  - DTO kết quả hoàn thành của từ BẮT BUỘC chứa thuộc tính `IsCorrectFirstTry` và `FirstTryDurationMs` ghi nhận kết quả của LẦN THỬ ĐẦU TIÊN.
  - Việc người học trả lời sai ở lần 1 rồi thử lại làm đúng ở lần 2 KHÔNG ĐƯỢC làm thay đổi thuộc tính `IsCorrectFirstTry = false`.
- **Đầy đủ Dữ liệu Chấm điểm SRS (`Complete SRS Payload Invariant`)**: 100% mục từ hoàn thành BẮT BUỘC cung cấp đủ 5 trường: `VocabularySenseId`, `IsCorrectFirstTry`, `TotalAttemptsCount`, `UsedHintCount`, `AverageDurationMs`.

## 2. Cấu trúc DTO Kết quả Hoàn thành Mục từ (CompletedItemResult Envelope)

```csharp
public class CompletedItemResultDto
{
    public Guid VocabularyId { get; set; }
    public Guid VocabularySenseId { get; set; }
    public string Headword { get; set; }
    
    public bool IsCorrectFirstTry { get; set; }
    public int FirstTryDurationMs { get; set; }
    public int TotalAttemptsCount { get; set; }
    public int UsedHintCount { get; set; }
    public bool IsTypoWarning { get; set; }
    
    public ItemCompletionStatus FinalStatus { get; set; } // PASSED, PASSED_WITH_HINT, FAILED
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IR-G01`: 100% `CompletedItemResultDto` phát ra chứa `VocabularySenseId` hợp lệ và `IsCorrectFirstTry` phản ánh đúng lần thử đầu tiên.
- `IR-G02`: Thử lại đúng ở lần 2 kẹp `IsCorrectFirstTry = false` và `TotalAttemptsCount = 2`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IR35-01` | Người học làm đúng từ A ngay lần thử đầu tiên trong 3 giây | DTO trả về `IsCorrectFirstTry = true`, `FirstTryDurationMs = 3000`, `TotalAttemptsCount = 1`. |
| `IR35-02` | Người học trả lời sai lần 1, bấm thử lại và làm đúng lần 2 | DTO trả về `IsCorrectFirstTry = false`, `TotalAttemptsCount = 2`, `FinalStatus = PASSED`. |
| `IR35-03` | Kiểm thử hoàn tất luồng M03-ITEM-COMPLETION-RESULT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-IR-F01` | Nhúng `CompletedItemResultDto` làm phần tử danh sách trong `LearningSessionCompletedIntegrationEvent` | Đảm bảo M04 nhận đủ dữ liệu chấm SRS | M03-T040 |

## 5. Tự kiểm M03-T035
- Đã hoàn thành đặc tả `M03-ITEM-COMPLETION-RESULT-1.0`.
- Chốt DTO kết quả hoàn thành mục từ và nguyên tắc bất biến bằng chứng gợi nhớ đầu.
- Ghi nhận 2 Regression Gates (`IR-G01`–`IR-G02`) và 3 Test Cases (`IR35-01`–`IR35-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa kết quả hoàn thành từng từ M03-T035 | WSA-7K2 |
