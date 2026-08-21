# Đặc tả hợp đồng tín hiệu M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-SIGNAL-CONTRACT-1.0` |
| Task | M10-T004 |
| Đầu vào | M10-NOTIFICATION-SIGNAL-CATALOG-1.0 (M10-T003), M10-NOTIFICATION-TAXONOMY-1.0 (M10-T002) |
| Phạm vi | Cấu trúc DTO hợp đồng tín hiệu thông báo (`NotificationSignalEvent`) và logic tiêu thụ Idempotent tại M10 Consumer |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy chuẩn giao thức hợp đồng nhận tín hiệu tạo thông báo gửi về M10.

- **Tính Duy nhất của TriggerEventId (`Trigger Event Idempotency Invariant`)**: Mỗi tín hiệu gửi về M10 BẮT BUỘC mang một `TriggerEventId` duy nhất. Gửi lặp tín hiệu trùng `TriggerEventId` CẤM tạo thêm bản ghi thông báo mới trong Hộp thư (`NotificationInbox`).
- **Tuân thủ Cấu hình Opt-Out (`Opt-Out Matrix Compliance Invariant`)**: Trước khi khởi tạo bản ghi gửi tin PUSH/Email, M10 Consumer BẮT BUỘC kiểm tra bảng thiết lập `UserNotificationPreferences`. Nếu người dùng đã Opt-Out nhóm đó, hệ thống chỉ tạo bản ghi In-App và hủy kênh Push.

## 2. Dynamic Notification Signal Consumer Code

```csharp
public class NotificationSignalConsumer : IConsumer<NotificationSignalEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IUserPreferenceService _preferenceService;
    
    public async Task Consume(ConsumeContext<NotificationSignalEvent> context)
    {
        var signal = context.Message;
        
        // 1. Kiểm tra Idempotency chống tạo trùng thông báo
        if (await _notificationService.IsSignalProcessedAsync(signal.TriggerEventId)) return;
        
        // 2. Kiểm tra ma trận Opt-Out của người dùng
        bool isPushEnabled = await _preferenceService.IsCategoryEnabledAsync(
            signal.TargetUserId, 
            signal.CategoryCode, 
            ChannelType.PUSH
        );
        
        // 3. Khởi tạo bản ghi Hộp thư In-App (Luôn tạo)
        var inboxItem = await _notificationService.CreateInboxItemAsync(signal);
        
        // 4. Đưa vào hàng chờ phát Push nếu được phép và không dính Quiet Hours
        if (isPushEnabled && signal.CategoryCode != "SECURITY" && IsInQuietHours(signal.TargetUserTimeZone))
        {
            await _notificationService.ScheduleDeferredPushAsync(inboxItem.NotificationId, GetNextMorning7AmUtc());
        }
        else if (isPushEnabled)
        {
            await _notificationService.EnqueueImmediatePushAsync(inboxItem.NotificationId);
        }
        
        await _notificationService.MarkSignalProcessedAsync(signal.TriggerEventId);
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SC-G01`: 100% tín hiệu từ người dùng đã Opt-Out nhóm tương ứng bị hủy kênh PUSH.
- `SC-G02`: Tín hiệu trùng `TriggerEventId` không tạo bản ghi thông báo thứ 2 trong `NotificationInbox`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SC04-01` | Người dùng Opt-Out nhóm `STUDY`, nhận tín hiệu `DUE_REVIEWS_REMINDER` | Tạo bản ghi In-App Inbox, không gửi tin Push. |
| `SC04-02` | Tín hiệu `SECURITY` đến từ thiết bị lạ khi đã Opt-Out toàn bộ | Bỏ qua Opt-Out, gửi ngay tin PUSH cảnh báo. |
| `SC04-03` | Kiểm thử hoàn tất luồng M10-SIGNAL-CONTRACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-SC-F01` | Thêm Unique Index `UX_NotificationInbox_TriggerEvent` trong CSDL | Đảm bảo tính chống đếm lặp Hộp thư | M10-T005 |

## 5. Tự kiểm M10-T004
- Đã đặc tả hợp đồng tín hiệu M10-T004.
- Ghi nhận 2 Regression Gates (`SC-G01`–`SC-G02`) và 3 Test Cases (`SC04-01`–`SC04-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả đặc tả hợp đồng tín hiệu M10-T004 | WSA-7K2 |
