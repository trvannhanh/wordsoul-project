# Chuẩn hóa tín hiệu lịch ôn thay đổi M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-SCHEDULE-CHANGED-SIGNAL-1.0` |
| Task | M04-T036 |
| Đầu vào | M04-DUE-ITEM-SELECTION-CRITERIA-1.0 (M04-T020), M04-REVIEW-QUEUE-HANDOFF-1.0 (M04-T023) |
| Phạm vi | Đặc tả hợp đồng sự kiện phát đi khi danh sách hoặc số lượng từ đến hạn ôn tập của người học thay đổi (`ReviewScheduleChangedIntegrationEvent`), dùng cho M10 (Push Reminders) và M03 |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa hợp đồng sự kiện tích hợp phát sinh khi lịch ôn tập có biến động (`ReviewScheduleChangedIntegrationEvent`) trong M04.

- **Chống Phát Sự kiện Dồn dập (`Event Rate Limiting Invariant`)**:
  - Sự kiện `ReviewScheduleChangedIntegrationEvent` CHỈ ĐƯỢC PHÁT khi tổng số từ đến hạn ôn trong ngày `DueCount` thay đổi vượt ngưỡng $\Delta Due >= 5$ từ hoặc sau khoảng thời gian đệm `DebounceWindow = 5` phút.
  - Tránh hiện tượng spam sự kiện khi người học vừa làm bài vừa cập nhật từng từ một.
- **Tính Bất biến và Tham chiếu Thời điểm (`Event Timestamp Invariant`)**: Sự kiện BẮT BUỘC chứa `SnapshotCreatedAtUtc` và `CurrentDueCount` để các module tiêu thụ (M10, M03) xác định bản tin mới nhất.

## 2. Hợp đồng Sự kiện Tích hợp Lịch Ôn Thay đổi (Integration Event Schema)

```csharp
public class ReviewScheduleChangedIntegrationEvent
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SnapshotCreatedAtUtc { get; set; }
    
    public int CurrentDueCount { get; set; }
    public int OverdueCount { get; set; }
    public DateTime NextDueDateUtc { get; set; }
    
    public string DeduplicationKey => $"sched_change_{UserId}_{SnapshotCreatedAtUtc:yyyyMMddHHmm}";
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SC-G01`: 100% sự kiện phát ra chứa `DeduplicationKey` hợp lệ dựa trên UserId và phút phát sinh.
- `SC-G02`: M10 nhận sự kiện cũ hơn sự kiện đã xử lý tự động bỏ qua không gửi thông báo trùng.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SC36-01` | Learner vừa hoàn thành 10 từ ôn làm `DueCount` giảm từ 50 xuống 40 | M04 phát 1 sự kiện `ReviewScheduleChangedIntegrationEvent` với `CurrentDueCount = 40`. |
| `SC36-02` | M04 phát 2 sự kiện liên tiếp trong vòng 10 giây do 2 worker xử lý xong | Outbox Publisher gom thành 1 sự kiện duy nhất nhờ `DeduplicationKey`. |
| `SC36-03` | Kiểm thử hoàn tất luồng M04-REVIEW-SCHEDULE-CHANGED-SIGNAL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-SC-F01` | Lắng nghe sự kiện tại MassTransit Consumer của M10 | Phục vụ lập lịch gửi thông báo nhắc ôn Push Notification | M10-T027 |

## 5. Tự kiểm M04-T036
- Đã hoàn thành đặc tả `M04-REVIEW-SCHEDULE-CHANGED-SIGNAL-1.0`.
- Chốt schema sự kiện `ReviewScheduleChangedIntegrationEvent` và cơ chế gom tin Debounce 5 phút.
- Ghi nhận 2 Regression Gates (`SC-G01`–`SC-G02`) and 3 Test Cases (`SC36-01`–`SC36-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa tín hiệu lịch ôn thay đổi M04-T036 | WSA-7K2 |
