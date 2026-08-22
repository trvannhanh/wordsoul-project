# Thiết kế bàn giao nhắc ôn cho M10 M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-REMINDER-HANDOFF-M10-1.0` |
| Task | M04-T037 |
| Đầu vào | M01-USER-TIMEZONE-1.0 (M01-T025), M04-REVIEW-SCHEDULE-CHANGED-SIGNAL-1.0 (M04-T036), M10-NOTIFICATION-SIGNAL-CATALOG-1.0 (M10-T003) |
| Phạm vi | Quy trình bàn giao tín hiệu nhắc ôn tập (`Review Reminder Handoff Payload`) từ M04 sang M10, bảo đảm M04 không trực tiếp quyết định kênh hay tự phát Push Notification |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa quy trình bàn giao dữ liệu nhắc ôn tập (`Review Reminder Handoff Payload`) từ M04 sang M10.

- **Phân định Ranh giới Trách nhiệm Kênh (`Channel Separation Invariant`)**:
  - M04 CHỈ CUNG CẤP dữ liệu thô: `UserId`, `DueItemCount`, `OverdueItemCount`, `SuggestedRemindTimeUtc`.
  - M04 KHÔNG ĐƯỢC QUYẾT ĐỊNH gửi Push/Email hay tự gọi nhà cung cấp thông báo. Việc chọn kênh, xử lý giờ yên lặng và kiểm tra Opt-In/Opt-Out thuộc trách nhiệm duy nhất của M10.
- **Tự động Hủy Tín hiệu khi không còn từ Đến hạn (`Auto Recall Idle Signal Rule`)**: Nếu người học vừa hoàn thành xong bài ôn làm `DueItemCount == 0`, M04 BẮT BUỘC phát sự kiện hủy nhắc `CancelReviewReminderSignalEvent` sang M10.

## 2. Hợp đồng Payload Bàn giao Nhắc Ôn (Handoff Payload Schema)

```csharp
public class ReviewReminderHandoffPayloadDto
{
    public Guid SignalId { get; set; }
    public Guid UserId { get; set; }
    public int DueItemCount { get; set; }
    public int OverdueItemCount { get; set; }
    public DateTime SuggestedRemindTimeUtc { get; set; }
    public string UserTimeZoneId { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RH-G01`: 100% payload bàn giao từ M04 không chứa thông tin cấu hình kênh Push Notification.
- `RH-G02`: Sự kiện `CancelReviewReminderSignalEvent` tự động phát ra ngay khi `DueItemCount` chạm mức 0.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RH37-01` | Learner có 15 từ đến hạn ôn tập lúc 18:00 UTC | M04 phát `ReviewReminderHandoffPayloadDto` sang M10 với `DueItemCount = 15`. |
| `RH37-02` | Learner hoàn thành ôn hết 15 từ ngay sau đó | M04 phát `CancelReviewReminderSignalEvent` làm M10 gỡ tín hiệu nhắc ôn khỏi hàng đợi. |
| `RH37-03` | Kiểm thử hoàn tất luồng M04-REVIEW-REMINDER-HANDOFF-M10-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-RH-F01` | Lắng nghe `CancelReviewReminderSignalEvent` tại M10 | Xóa thông báo nhắc ôn chưa gửi khỏi queue | M10-T027 |

## 5. Tự kiểm M04-T037
- Đã hoàn thành đặc tả `M04-REVIEW-REMINDER-HANDOFF-M10-1.0`.
- Chốt nguyên tắc phân định trách nhiệm kênh và phát tín hiệu hủy nhắc tự động.
- Ghi nhận 2 Regression Gates (`RH-G01`–`RH-G02`) và 3 Test Cases (`RH37-01`–`RH37-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế bàn giao nhắc ôn cho M10 M04-T037 | WSA-7K2 |
