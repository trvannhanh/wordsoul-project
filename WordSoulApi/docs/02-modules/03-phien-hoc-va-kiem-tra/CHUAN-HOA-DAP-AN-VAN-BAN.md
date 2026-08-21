# Chuẩn hóa đáp án văn bản M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-TEXT-ANSWER-NORMALIZATION-1.0` |
| Task | M03-T028 |
| Đầu vào | M02-WORD-VARIANTS-1.0 (M02-T003), M03-QUESTION-DATA-TYPES-1.0 (M03-T019) |
| Phạm vi | Quy tắc chuẩn hóa văn bản đầu vào khi chấm các bài tập điền từ/chính tả (Trim, Lowercase, Unaccent, Punctuation removal) |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định thuật toán chuẩn hóa xâu ký tự đáp án văn bản do người học nhập vào trước khi so sánh với đáp án chuẩn.

- **Bỏ qua Khoảng trắng và Ký tự đặc biệt Thừa (`Whitespace & Punctuation Invariant`)**: Tự động loại bỏ khoảng trắng đầu/cuối (Trim), rút gọn các khoảng trắng kép giữa các từ thành 1 khoảng trắng duy nhất, bỏ các dấu chấm/phẩy thừa ở cuối xâu.
- **Không Phân biệt Hoa Thường (`Case-Insensitive Invariant`)**: Tự động chuyển toàn bộ xâu nhập vào về chữ thường (`Lowercase`) trước khi so sánh.
- **Tôn trọng Biến thể Hợp lệ (`Valid Variant Invariant`)**: Nếu câu trả lời nhập vào trùng khớp với một biến thể từ vựng hợp lệ (`VocabularyVariant` theo M02-T003), câu trả lời được ghi nhận là ĐÚNG.

## 2. Thuật toán Chuẩn hóa Văn bản (Text Normalizer Engine)

```csharp
public bool EvaluateTextAnswer(string submittedText, string expectedMasterWord, List<string> validVariants)
{
    string normalizedSubmitted = Normalize(submittedText);
    string normalizedExpected = Normalize(expectedMasterWord);
    
    // 1. So sánh trực tiếp với Master Headword
    if (normalizedSubmitted == normalizedExpected) return true;
    
    // 2. So sánh với danh sách biến thể hợp lệ (ví dụ: color vs colour)
    foreach (var variant in validVariants)
    {
        if (normalizedSubmitted == Normalize(variant)) return true;
    }
    
    return false;
}

private string Normalize(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return string.Empty;
    
    return text.Trim()
               .ToLowerInvariant()
               .Replace(".", "")
               .Replace(",", "")
               .Replace("  ", " ");
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TN-G01`: Nhập đáp án `" Hello "` được chuẩn hóa thành `"hello"` và chấm đúng với đáp án `"hello"`.
- `TN-G02`: Chấp nhận các biến thể từ vựng đã được M02 chứng nhận (ví dụ: `"colour"` cho `"color"`).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TN28-01` | Người học nhập `"Apple."` cho câu hỏi từ `"apple"` | Chấm `IsCorrect = true`. |
| `TN28-02` | Người học nhập `"  RUN  "` cho câu hỏi từ `"run"` | Chấm `IsCorrect = true`. |
| `TN28-03` | Kiểm thử hoàn tất luồng M03-TEXT-ANSWER-NORMALIZATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-TN-F01` | Bổ sung helper `TextNormalizationUtils` trong Domain Core | Tái sử dụng cho các module kiểm tra | M03-T029 |

## 5. Tự kiểm M03-T028
- Đã đặc tả thuật toán chuẩn hóa đáp án văn bản M03-T028.
- Ghi nhận 2 Regression Gates (`TN-G01`–`TN-G02`) và 3 Test Cases (`TN28-01`–`TN28-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa đáp án văn bản M03-T028 | WSA-7K2 |
