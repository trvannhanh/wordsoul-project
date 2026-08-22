# Thiết kế ổn định câu hỏi khi tải lại M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-RELOAD-STABILITY-1.0` |
| Task | M03-T021 |
| Đầu vào | M03-SESSION-CONTENT-SNAPSHOT-1.0 (M03-T007), M03-DISTRACTOR-GENERATION-1.0 (M03-T020) |
| Phạm vi | Cơ chế giữ cố định nội dung câu hỏi và danh sách phương án nhiễu (`Deterministic Options Placement`) khi client tải lại hoặc mất kết nối |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả tính ổn định của câu hỏi (`Reload Stability`) trong M03.

- **Cố định Phương án Nhiễu và Thứ tự Trộn (`Deterministic Options & Seeded Shuffle Invariant`)**:
  - Khi client bấm F5/Tải lại màn hình hoặc mất kết nối mạng ở bước hiện tại, nội dung câu hỏi, danh sách các phương án nhiễu và vị trí xáo trộn (Option Placement) BẮT BUỘC giữ nguyên 100%.
  - Tuyệt đối CẤM đổi đáp án hoặc sinh lại tập phương án nhiễu mới khiến người học có thể "F5 để tìm câu dễ hơn".
- **Hàm Trộn theo Seed Cố định (`Seeded Pseudo-Random Shuffle`)**: Thứ tự hiển thị phương án trắc nghiệm được sinh bởi hàm random có seed cố định: `Seed = Hash(SessionId + StepIndex + VocabularyId)`.

## 2. Thuật toán Xáo trộn Phương án Cố định (Seeded Option Shuffler)

```csharp
public List<QuestionOptionDto> GenerateDeterministicOptions(Guid sessionId, int stepIndex, Guid vocabId, string correctAnswer, List<string> distractors)
{
    var options = new List<QuestionOptionDto>();
    options.Add(new QuestionOptionDto { OptionId = "OPT_CORRECT", Text = correctAnswer });
    
    for (int i = 0; i < distractors.Count; i++)
    {
        options.Add(new QuestionOptionDto { OptionId = $"OPT_DISTRACTOR_{i}", Text = distractors[i] });
    }

    // Sinh Seed cố định dựa trên Hash kết hợp
    int seed = HashCode.Combine(sessionId, stepIndex, vocabId);
    var random = new Random(seed);

    // Trộn danh sách bằng Knuth/Fisher-Yates Shuffle
    for (int i = options.Count - 1; i > 0; i--)
    {
        int k = random.Next(i + 1);
        var temp = options[i];
        options[i] = options[k];
        options[k] = temp;
    }

    return options;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RLS-G01`: Tải lại API lấy câu hỏi ở cùng `StepIndex` 100 lần trả về vị trí các phương án A, B, C, D giống hệt 100%.
- `RLS-G02`: Không đổi tập phương án nhiễu khi người dùng reload trang.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RLS21-01` | Người học F5 lại trình duyệt ở bước 3 (đang có 4 lựa chọn) | Trả về đúng 4 lựa chọn với đúng thứ tự A, B, C, D ban đầu. |
| `RLS21-02` | Mất kết nối 3G và gửi lại request `GetStepQuestion` | DTO câu hỏi trả về khớp 100% hash với câu hỏi trước khi mất mạng. |
| `RLS21-03` | Kiểm thử hoàn tất luồng M03-RELOAD-STABILITY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-RLS-F01` | Sử dụng `Random(Seed)` trong `QuestionGeneratorService` | Đảm bảo tính nhất quán giữa các lần gọi API | M03-T019 |

## 5. Tự kiểm M03-T021
- Đã hoàn thành đặc tả `M03-RELOAD-STABILITY-1.0`.
- Chốt thuật toán Fisher-Yates Shuffle theo Seed cố định.
- Ghi nhận 2 Regression Gates (`RLS-G01`–`RLS-G02`) và 3 Test Cases (`RLS21-01`–`RLS21-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế ổn định câu hỏi khi tải lại M03-T021 | WSA-7K2 |
