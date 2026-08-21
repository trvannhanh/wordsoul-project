# Thiết kế tiếp nhận đúng một lần M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-EXACTLY-ONCE-SIGNAL-CONSUMPTION-1.0` |
| Task | M10-T005 |
| Đầu vào | M10-SIGNAL-CONTRACT-1.0 (M10-T004), M12-RETRY-IDEMPOTENCY-1.0 (M12-T037) |
| Phạm vi | Cơ chế Idempotency chống tạo trùng bản ghi thông báo Hộp thư khi nhận lặp tín hiệu nguồn `TriggerEventId` |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả logic bảo vệ Idempotency tiếp nhận tín hiệu thông báo tại M10.

- **Tính Duy nhất của Bản ghi Hộp thư (`Single Inbox Record Invariant`)**: Một `TriggerEventId` từ module nguồn CHỈ ĐƯỢC PHÉP sinh ra đúng 1 bản ghi `NotificationInbox` cho 1 người dùng. Sự kiện phát lại trùng `TriggerEventId` CẤM tạo thêm bản ghi thứ 2 trong Hộp thư.
- **Ràng buộc Duy nhất ở CSDL (`Unique Index Guard`)**: Bảng `NotificationInbox` bổ sung chỉ mục duy nhất `UX_NotificationInbox_Target_TriggerEvent` trên bộ đôi `(TargetUserId, TriggerEventId)`.

## 2. Dynamic Exactly-Once Consumer Logic

```csharp
public async Task<bool> ProcessSignalExactlyOnceAsync(NotificationSignalEvent signal)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    // 1. Kiểm tra tồn tại trong NotificationInbox
    bool exists = await _dbContext.NotificationInbox.AnyAsync(n => 
        n.TargetUserId == signal.TargetUserId && 
        n.TriggerEventId == signal.TriggerEventId);
        
    if (exists)
    {
        await transaction.RollbackAsync();
        return true; // Skip idempotent
    }
    
    // 2. Tạo bản ghi Hộp thư In-App
    var inboxItem = new NotificationInbox {
        NotificationId = Guid.NewGuid(),
        TargetUserId = signal.TargetUserId,
        TriggerEventId = signal.TriggerEventId,
        CategoryCode = signal.CategoryCode,
        Title = signal.Title,
        Body = signal.Body,
        IsRead = false,
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = DateTime.UtcNow.AddSeconds(signal.TimeToLiveSeconds)
    };
    
    _dbContext.NotificationInbox.Add(inboxItem);
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
    return true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `EO-G01`: Gửi 3 tín hiệu trùng `TriggerEventId` chỉ tạo đúng 1 bản ghi trong `NotificationInbox`.
- `EO-G02`: Đếm số lượng thông báo chưa đọc `UnreadCount` không bị tăng 2 lần.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EO05-01` | Consumer nhận 2 message `DUE_REVIEWS_REMINDER` trùng `TriggerEventId` | Tạo 1 item trong Inbox, message 2 bị bỏ qua và Acknowledge ngay. |
| `EO05-02` | Kiểm tra DB `NotificationInbox` sau khi gửi duplicate | `Count = 1`. |
| `EO05-03` | Kiểm thử hoàn tất luồng M10-EXACTLY-ONCE-SIGNAL-CONSUMPTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-EO-F01` | Bổ sung unique index `UX_NotificationInbox_Target_TriggerEvent` | Đảm bảo tính chống trùng cấp DB | M10-T007 |

## 5. Tự kiểm M10-T005
- Đã đặc tả thiết kế tiếp nhận đúng một lần M10-T005.
- Ghi nhận 2 Regression Gates (`EO-G01`–`EO-G02`) và 3 Test Cases (`EO05-01`–`EO05-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế tiếp nhận đúng một lần M10-T005 | WSA-7K2 |
