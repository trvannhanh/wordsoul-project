# Chuẩn hóa tiến độ theo bộ từ M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-VOCAB-SET-PROGRESS-SUMMARY-1.0` |
| Task | M04-T031 |
| Đầu vào | M02-SET-STATUS-EFFECTIVITY-1.0 (M02-T023), M04-USER-SENSE-UNIT-1.0 (M04-T002), M04-MEMORY-STATES-DEFINITIONS-1.0 (M04-T015) |
| Phạm vi | Mô hình tính toán tổng hợp phần trăm tiến độ học của cả bộ từ vựng (`VocabSetProgressSummary`), quy tắc tính mẫu số khi bộ từ bị thay đổi số mục từ |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa công thức tính tiến độ hoàn thành cho từng bộ từ vựng (`VocabSetProgressSummary`) trong M04.

- **Công thức Tính Tiến độ Bộ từ Định hình (`Set Progress Percentage Invariant`)**:
  - Phần trăm hoàn thành của một bộ từ vựng $CompletionRate_{set}$ được tính bằng:
    $$CompletionRate_{set} = \frac{\text{Số mục từ đã đạt trạng thái MASTERED}}{\text{Tổng số mục từ trong phiên bản bộ từ hiện tại}} \times 100\%$$
- **Ứng xử khi Bộ từ Thay đổi Thành phần (`Set Mutation Invariant`)**:
  - Khi tác giả thêm/bớt từ vựng trong bộ từ (thay đổi `VersionHash`), mẫu số tổng số từ tự động cập nhật.
  - Lịch sử tiến độ các từ đã thuộc giữ nguyên 100%, không bị reset hay tính lại âm thầm.

## 2. Bảng Tổng hợp Tiến độ Bộ từ Vựng (Set Progress Summary DTO Envelope)

```csharp
public class VocabSetProgressSummaryDto
{
    public Guid VocabSetId { get; set; }
    public string VocabSetTitle { get; set; }
    public string SetVersionHash { get; set; }
    
    public int TotalSensesCount { get; set; }
    public int LearningSensesCount { get; set; }
    public int ReviewingSensesCount { get; set; }
    public int MasteredSensesCount { get; set; }
    
    public double SetMasteryPercentage { get; set; }
    public DateTime? LastStudiedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SP-G01`: 100% `SetMasteryPercentage` được tính chính xác bằng `MasteredSensesCount / TotalSensesCount * 100`.
- `SP-G02`: Thêm 2 từ mới vào bộ từ làm mẫu số tăng thêm 2 nhưng không thay đổi `MasteredSensesCount`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SP31-01` | Bộ từ có 10 từ, người học đã đạt `MASTERED` 4 từ | `SetMasteryPercentage = 40.0%`. |
| `SP31-02` | Tác giả bộ từ thêm 2 từ mới làm tổng số từ tăng từ 10 lên 12 | `SetMasteryPercentage` tự động cập nhật thành $4/12 \times 100 = 33.3\%$. |
| `SP31-03` | Kiểm thử hoàn tất luồng M04-VOCAB-SET-PROGRESS-SUMMARY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-SP-F01` | Đưa `VocabSetProgressSummaryDto` vào endpoint GET `/api/v1/library/sets` | Hiển thị thanh phần trăm tiến độ bộ từ trong Thư viện | M02-T039 |

## 5. Tự kiểm M04-T031
- Đã hoàn thành đặc tả `M04-VOCAB-SET-PROGRESS-SUMMARY-1.0`.
- Chốt công thức phần trăm tiến độ bộ từ và nguyên tắc bảo lưu lịch sử khi bộ từ đổi thành phần.
- Ghi nhận 2 Regression Gates (`SP-G01`–`SP-G02`) và 3 Test Cases (`SP31-01`–`SP31-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa tiến độ theo bộ từ M04-T031 | WSA-7K2 |
