# Quyết định xử lý lỗi chính tả gần đúng M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-TYPO-TOLERANCE-1.0` |
| Task | M03-T029 |
| Đầu vào | M03-TEXT-ANSWER-NORMALIZATION-1.0 (M03-T028), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Thuật toán khoảng cách Levenshtein (`Levenshtein Distance`), quy tắc chấp nhận sai sót nhỏ (`Almost Correct / Typo Warning`) và cấm chấp nhận từ sai nghĩa |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chính sách xử lý khi người học gõ đáp án văn bản bị sai sót chính tả nhỏ trong M03.

- **Ngưỡng Khoảng cách Levenshtein An toàn (`Safe Levenshtein Threshold Invariant`)**:
  - *Từ ngắn ($L \le 4$ ký tự)*: Khoảng cách $D = 0$ (Chính xác 100%, KHÔNG chấp nhận lỗi chính tả).
  - *Từ trung bình ($5 \le L \le 8$ ký tự)*: Chấp nhận tối đa $D = 1$ ký tự sai sót (trả kết quả `ALMOST_CORRECT`).
  - *Từ dài ($L \ge 9$ ký tự)*: Chấp nhận tối đa $D = 2$ ký tự sai sót.
- **Cấm Chấp nhận Từ Sai nghĩa (`No Semantic Shift Invariant`)**: Nếu lỗi chính tả biến từ nhập vào thành một từ vựng có thực khác trong từ điển M02 nhưng mang nét nghĩa hoàn toàn khác (ví dụ: *cat* vs *bat*, *ship* vs *sheep*), hệ thống BẮT BUỘC chấm `INCORRECT`.
- **Đánh dấu Gợi nhớ Đủ Tiêu chuẩn (`SRS Evidence Rating`)**: Câu trả lời đạt `ALMOST_CORRECT` được tính là đúng trong bước phiên học hiện tại, nhưng ghi nhận `IsTypoWarning = true` để M04 có thể giảm bớt điểm thành thạo khi tính SRS.

## 2. Thuật toán Chấm điểm Khoảng cách Levenshtein (Typo Matcher Engine)

```csharp
public EvaluationResultDto EvaluateTypoTolerance(string userAnswer, string expectedAnswer)
{
    string normUser = TextNormalizer.Normalize(userAnswer);
    string normExpected = TextNormalizer.Normalize(expectedAnswer);

    if (normUser == normExpected)
    {
        return new EvaluationResultDto { Status = AnswerStatus.EXACT_MATCH, IsCorrect = true };
    }

    int distance = LevenshteinDistance.Compute(normUser, normExpected);
    int length = normExpected.Length;

    int maxAllowedDistance = length <= 4 ? 0 : (length <= 8 ? 1 : 2);

    if (distance <= maxAllowedDistance)
    {
        // Kiểm tra xem từ nhập vào có bị biến thành từ khác nghĩa không
        if (_vocabDict.IsExistingVocabulary(normUser))
        {
            return new EvaluationResultDto { Status = AnswerStatus.INCORRECT, IsCorrect = false, Reason = "DIFFERENT_WORD_MEANING" };
        }

        return new EvaluationResultDto { 
            Status = AnswerStatus.ALMOST_CORRECT, 
            IsCorrect = true, 
            IsTypoWarning = true,
            FeedbackMessage = $"Gần đúng! Đáp án chính xác là: '{expectedAnswer}'"
        };
    }

    return new EvaluationResultDto { Status = AnswerStatus.INCORRECT, IsCorrect = false };
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TT-G01`: 100% từ có độ dài $\le 4$ ký tự bị từ chối nếu có 1 ký tự gõ sai.
- `TT-G02`: Nhập sai 1 ký tự trên từ dài 7 ký tự trả về trạng thái `ALMOST_CORRECT` kèm thông báo nhắc đáp án chuẩn.
- `TT-G03`: Gõ nhầm từ có thực mang nghĩa khác (ví dụ gõ *"bat"* thay cho *"cat"*) bị chấm `INCORRECT`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TT29-01` | Đáp án `"beautiful"`, người học gõ `"beautifull"` ($D=1, L=9$) | Trả về `ALMOST_CORRECT`, `IsCorrect = true`, `IsTypoWarning = true`. |
| `TT29-02` | Đáp án `"cat"`, người học gõ `"bat"` ($D=1, L=3$) | Trả về `INCORRECT` (do $L \le 4$). |
| `TT29-03` | Đáp án `"house"`, người học gõ `"mouse"` | Trả về `INCORRECT` (do `"mouse"` là từ có thực mang nghĩa khác). |
| `TT29-04` | Kiểm thử hoàn tất luồng M03-TYPO-TOLERANCE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-TT-F01` | Cần tích hợp `LevenshteinDistance` helper trong `WordSoul.Domain.Common` | Chưa có thư viện tính khoảng cách Levenshtein | M03-T028 |

## 5. Tự kiểm M03-T029
- Đã hoàn thành đặc tả `M03-TYPO-TOLERANCE-1.0`.
- Chốt thuật toán Levenshtein 3 cấp độ và nguyên tắc chống sai nghĩa.
- Ghi nhận 3 Regression Gates (`TT-G01`–`TT-G03`) và 4 Test Cases (`TT29-01`–`TT29-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả quyết định xử lý lỗi chính tả gần đúng M03-T029 | WSA-7K2 |
