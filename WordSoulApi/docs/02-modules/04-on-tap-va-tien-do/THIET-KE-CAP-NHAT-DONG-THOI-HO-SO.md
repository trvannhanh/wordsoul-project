# Thiết kế cập nhật đồng thời hồ sơ M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-CONCURRENT-PROFILE-MUTATION-1.0` |
| Task | M04-T009 |
| Đầu vào | M04-IDEMPOTENT-RESULT-CONSUMPTION-1.0 (M04-T007), M04-OUT-OF-ORDER-RESULT-HANDLING-1.0 (M04-T008) |
| Phạm vi | Xử lý khóa lạc quan (`Optimistic Concurrency Control / RowVersion`), ngăn ngừa xung đột ghi đồng thời khi hai sự kiện hoàn thành phiên cùng cập nhật một hồ sơ `UserSenseProgress` |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chiến lược xử lý xung đột ghi đồng thời (`Concurrent Mutation`) trên thực thể `UserSenseProgress` trong M04.

- **Khóa Lạc quan với RowVersion (`Optimistic Concurrency Invariant`)**:
  - Thực thể `UserSenseProgress` BẮT BUỘC sử dụng thuộc tính `RowVersion` / `ConcurrencyToken` (kiểu `byte[]` trong EF Core).
  - Mọi thao tác cập nhật $Interval$ hay $EaseFactor$ vi phạm xung đột phiên bản `DbUpdateConcurrencyException` BẮT BUỘC được tự động thử lại (`Auto-Retry`) tối đa 3 lần với thuật toán Exponential Backoff.
- **Không Mất Cập nhật (`No Lost Update Invariant`)**: Khi xử lý hai sự kiện đồng thời, hệ thống đảm bảo cả hai cập nhật đều được ghi nhận vào lịch sử mà không bị đè mất dữ liệu.

## 2. Quy trình Thử lại Khóa Lạc quan (Optimistic Concurrency Retry Loop)

```csharp
public async Task UpdateProgressWithRetryAsync(Guid userSenseProgressId, Func<UserSenseProgress, Task> updateAction)
{
    int maxRetries = 3;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var progress = await _dbContext.UserSenseProgresses.FindAsync(userSenseProgressId);
            await updateAction(progress);
            await _dbContext.SaveChangesAsync();
            return; // Thành công
        }
        catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50 * Math.Pow(2, attempt)));
            _dbContext.ChangeTracker.Clear(); // Detach entity cũ để nạp lại bản mới
        }
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `CM-G01`: 100% exception `DbUpdateConcurrencyException` được xử lý tự động thử lại thành công trong 3 lần.
- `CM-G02`: Không phát sinh lỗi thất bại ghi dữ liệu DB khi 10 thread đồng thời cập nhật cùng 1 hồ sơ nhớ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CM09-01` | Chạy 10 thread đồng thời gọi `UpdateProgressWithRetryAsync` trên 1 `UserSenseProgressId` | 100% thread cập nhật thành công qua cơ chế retry, số dư/chỉ số cuối cùng khớp chính xác. |
| `CM09-02` | Giả lập lỗi xung đột RowVersion liên tục vượt quá 3 lần thử | System chuyển handler sang Dead Letter Queue (DLQ) và ghi log cảnh báo M11. |
| `CM09-03` | Kiểm thử hoàn tất luồng M04-CONCURRENT-PROFILE-MUTATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-CM-F01` | Cần bổ sung field `RowVersion` trong EF Core configuration `UserSenseProgressConfig.cs` | Bắt buộc cho Optimistic Locking | M04-T003 |

## 5. Tự kiểm M04-T009
- Đã hoàn thành đặc tả `M04-CONCURRENT-PROFILE-MUTATION-1.0`.
- Chốt thuật toán Optimistic Concurrency Locking và 3 lần retry exponential backoff.
- Ghi nhận 2 Regression Gates (`CM-G01`–`CM-G02`) và 3 Test Cases (`CM09-01`–`CM09-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế cập nhật đồng thời hồ sơ M04-T009 | WSA-7K2 |
