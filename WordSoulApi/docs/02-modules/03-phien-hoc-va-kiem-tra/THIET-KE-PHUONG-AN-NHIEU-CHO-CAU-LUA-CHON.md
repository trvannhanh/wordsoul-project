# Thiết kế phương án nhiễu cho câu lựa chọn M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-DISTRACTOR-GENERATION-1.0` |
| Task | M03-T020 |
| Đầu vào | M02-MULTI-SENSE-1.0 (M02-T002), M02-CEFR-DIFFICULTY-1.0 (M02-T005), M03-QUESTION-DATA-TYPES-1.0 (M03-T019) |
| Phạm vi | Thuật toán tạo 3 phương án nhiễu (Distractors) cho bài tập trắc nghiệm 4 lựa chọn, tiêu chí cùng loại từ POS và CEFR |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định thuật toán tự động sinh 3 phương án nhiễu (`Distractor Options`) cho các câu hỏi trắc nghiệm trong M03.

- **Tính Duy nhất của Đáp án Đúng (`Single Correct Answer Invariant`)**: Trong 4 lựa chọn trả về cho Client, BẮT BUỘC chỉ có duy nhất 1 lựa chọn là nét nghĩa đúng (`CorrectSense`). 3 phương án nhiễu CẤM trùng lặp hoặc chứa nét nghĩa tương đương ($SemanticSimilarity < 85\%$).
- **Đồng nhất Loại từ và Cấp độ CEFR (`POS & CEFR Alignment Invariant`)**:
  - 3 phương án nhiễu BẮT BUỘC có cùng Loại từ POS với từ gốc (ví dụ: Từ gốc là Động từ $\implies$ 3 phương án nhiễu đều là Động từ).
  - Phương án nhiễu được ưu tiên chọn trong cùng phân khúc cấp độ CEFR ($\pm 1$ level) để đảm bảo độ thách thức tương đương.

## 2. Thuật toán Sinh Phương án Nhiễu (Distractor Generator Engine)

```csharp
public async Task<List<QuestionOptionDto>> Generate4OptionsAsync(VocabularySense targetSense)
{
    // 1. Tạo Lựa chọn Đúng (Correct Option)
    var correctOption = new QuestionOptionDto {
        OptionId = Guid.NewGuid(),
        OptionText = targetSense.VietnameseMeaning
    };
    
    // 2. Truy vấn DB các nét nghĩa ứng viên nhiễu (Cùng POS, cùng/kế CEFR, khác WordId)
    var candidateSenses = await _senseRepo.GetDistractorCandidatesAsync(
        targetSense.POS, 
        targetSense.CEFRLevel, 
        excludeWordId: targetSense.VocabularyId,
        limit: 20
    );
    
    // 3. Trộn ngẫu nhiên (Shuffle) & chọn 3 phương án nhiễu không trùng nghĩa
    var selectedDistractors = candidateSenses
        .Where(s => !IsSemanticDuplicate(s.VietnameseMeaning, targetSense.VietnameseMeaning))
        .OrderBy(_ => Guid.NewGuid())
        .Take(3)
        .Select(s => new QuestionOptionDto {
            OptionId = Guid.NewGuid(),
            OptionText = s.VietnameseMeaning
        }).ToList();
        
    // 4. Gộp 4 options & xáo trộn thứ tự trước khi trả về
    var all4Options = selectedDistractors.Concat(new[] { correctOption })
        .OrderBy(_ => Guid.NewGuid())
        .ToList();
        
    return all4Options;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `DG-G01`: 100% câu hỏi trắc nghiệm có 4 phương án lựa chọn, trong đó 3 phương án nhiễu có cùng loại từ POS với từ gốc.
- `DG-G02`: Không tồn tại 2 lựa chọn có văn bản nghĩa trùng lặp trong cùng 1 câu hỏi.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DG20-01` | Sinh phương án nhiễu cho Động từ `"run"` (A1) | Trả về 1 đáp án đúng và 3 phương án nhiễu đều là Động từ A1/A2. |
| `DG20-02` | Kho từ vựng hiếm không đủ 3 từ cùng POS | Fallback lấy nét nghĩa nhiễu khác POS nhưng có nhãn rõ ràng, không crash. |
| `DG20-03` | Kiểm thử hoàn tất luồng M03-DISTRACTOR-GENERATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-DG-F01` | Cần cache danh sách Candidate Senses theo (POS, CEFR) trong Redis | Giảm thời gian query DB khi sinh distractors | M03-T021 |

## 5. Tự kiểm M03-T020
- Đã đặc tả thuật toán sinh phương án nhiễu M03-T020.
- Ghi nhận 2 Regression Gates (`DG-G01`–`DG-G02`) và 3 Test Cases (`DG20-01`–`DG20-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế phương án nhiễu cho câu lựa chọn M03-T020 | WSA-7K2 |
