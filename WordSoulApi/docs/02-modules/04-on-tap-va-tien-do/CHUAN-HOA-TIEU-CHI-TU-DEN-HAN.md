# Chuẩn hóa tiêu chí từ đến hạn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-DUE-ITEM-SELECTION-CRITERIA-1.0` |
| Task | M04-T020 |
| Đầu vào | M04-SRS-INTERVAL-CALCULATION-1.0 (M04-T016), M03-SESSION-POLICY-1.0 (M03-T002) |
| Phạm vi | Điều kiện lọc và thuật toán sắp xếp danh sách từ vựng đến hạn ôn tập (`DueReviewItemsQueue`) bàn giao cho M03 |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định tiêu chí truy vấn và chọn lọc danh sách mục từ vựng đến hạn ôn tập cho người học.

- **Tiêu chí Đếm Đến hạn (`Due Condition Invariant`)**: Một mục từ vựng được tính là "Đến hạn" khi và chỉ khi `DueDateUtc <= CurrentTimeUtc` và trạng thái `State != NEW` và `IsQuarantined == false`.
- **Thuật toán Ưu tiên Sắp xếp (`Priority Sorting Order Invariant`)**: Hàng đợi từ đến hạn được sắp xếp theo thứ tự ưu tiên:
  1. `State == RELEARNING` (Từ vừa bị quên - Ưu tiên cao nhất).
  2. `OverdueDays` giảm dần (Từ quá hạn lâu nhất).
  3. `EaseFactor` tăng dần (Từ khó hơn được ôn trước).

## 2. Dynamic Due Item Selection Query Code

```csharp
public async Task<List<DueItemDto>> GetDueReviewQueueAsync(Guid userId, int limit)
{
    DateTime nowUtc = DateTime.UtcNow;
    
    var dueItems = await _dbContext.UserSenseProgresses
        .Where(p => p.UserId == userId 
                 && p.State != MemoryState.NEW 
                 && p.DueDateUtc <= nowUtc)
        .OrderByDescending(p => p.State == MemoryState.RELEARNING) // Relearning first
        .ThenByDescending(p => EF.Functions.DateDiffDay(p.DueDateUtc, nowUtc)) // Overdue days
        .ThenBy(p => p.EaseFactor) // Harder items first
        .Take(limit)
        .Select(p => new DueItemDto {
            VocabularySenseId = p.VocabularySenseId,
            State = p.State,
            DueDateUtc = p.DueDateUtc,
            EaseFactor = p.EaseFactor
        })
        .ToListAsync();
        
    return dueItems;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `DS-G01`: 100% mục từ có `DueDateUtc > CurrentTimeUtc` bị loại khỏi hàng đợi đến hạn.
- `DS-G02`: Từ ở trạng thái `RELEARNING` luôn xếp trên các từ `REVIEWING` trong hàng đợi.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DS20-01` | Người học có 5 từ quá hạn, trong đó 1 từ `RELEARNING` | Từ `RELEARNING` xuất hiện ở vị trí đầu tiên của hàng đợi. |
| `DS20-02` | Truy vấn từ đến hạn khi không có từ nào quá hạn | Trả về danh sách rỗng (`Count = 0`). |
| `DS20-03` | Kiểm thử hoàn tất luồng M04-DUE-ITEM-SELECTION-CRITERIA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-DS-F01` | Cần tạo Index DB `(UserId, DueDateUtc, State)` trong Postgre/SQL Server | Tối ưu tốc độ query hàng đợi ôn | M04-T021 |

## 5. Tự kiểm M04-T020
- Đã đặc tả chuẩn hóa tiêu chí từ đến hạn M04-T020.
- Ghi nhận 2 Regression Gates (`DS-G01`–`DS-G02`) và 3 Test Cases (`DS20-01`–`DS20-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa tiêu chí từ đến hạn M04-T020 | WSA-7K2 |
