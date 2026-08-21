# Đặc tả mô hình hộp thư M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-INBOX-MODEL-SPEC-1.0` |
| Task | M10-T016 |
| Đầu vào | M10-SIGNAL-CONTRACT-1.0 (M10-T004), M10-NOTIFICATION-TEMPLATE-SPEC-1.0 (M10-T011) |
| Phạm vi | Mô hình Hộp thư In-App (`NotificationInbox`) quản lý danh sách thông báo cá nhân của người học, số lượng chưa đọc (`UnreadCount`) và thời gian hết hạn (`ExpiresAtUtc`) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cấu trúc bảng Hộp thư thông báo trong M10.

- **Tính Chính xác của Chỉ số Chưa đọc (`Unread Counter Accuracy Invariant`)**: Số thông báo chưa đọc `UnreadCount` của người học BẮT BUỘC bằng chính xác tổng số bản ghi trong `NotificationInbox` thỏa mãn `TargetUserId == UserId AND IsRead == false AND ExpiresAtUtc > UtcNow`.
- **Ràng buộc Dọn dẹp Hết hạn (`Expiration Cleanup Invariant`)**: Các thông báo đã hết hạn (`ExpiresAtUtc <= UtcNow`) tự động ẩn khỏi danh sách Hộp thư người dùng và được dọn dẹp qua Cronjob định kỳ.

## 2. Dynamic Notification Inbox Schema

```csharp
public class NotificationInbox
{
    public Guid NotificationId { get; set; }
    public Guid TargetUserId { get; set; }
    public string TriggerEventId { get; set; }
    
    public string CategoryCode { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string ActionDeepLink { get; set; } // Deeplink điều hướng khi bấm vào thông báo
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAtUtc { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IM-G01`: 100% truy vấn `UnreadCount` chỉ tính các thông báo chưa đọc và chưa hết hạn.
- `IM-G02`: Bấm vào thông báo chuyển `IsRead = true`, `ReadAtUtc = UtcNow` và làm giảm `UnreadCount` đúng `-1`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IM16-01` | Người dùng có 3 thông báo chưa đọc, đọc 1 thông báo | `UnreadCount` giảm từ 3 xuống 2. |
| `IM16-02` | Thông báo hết hạn TTL 24 giờ | Tự động ẩn khỏi danh sách Hộp thư trên App. |
| `IM16-03` | Kiểm thử hoàn tất luồng M10-INBOX-MODEL-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-IM-F01` | Cần thuộc tính `ActionDeepLink` để mở thẳng màn hình bài học/nhiệm vụ | Tăng tỷ lệ tương tác người học | M10-T017 |

## 5. Tự kiểm M10-T016
- Đã đặc tả mô hình hộp thư M10-T016.
- Ghi nhận 2 Regression Gates (`IM-G01`–`IM-G02`) và 3 Test Cases (`IM16-01`–`IM16-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả mô hình hộp thư M10-T016 | WSA-7K2 |
