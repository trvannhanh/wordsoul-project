# Đặc tả hợp đồng sự kiện nghiệp vụ M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-EVENT-CONTRACT-1.0` |
| Task | M07-T012 |
| Đầu vào | M07-QUEST-EVENT-CATALOG-1.0 (M07-T011), M03-SESSION-COMPLETED-EVENT-1.0 (M03-T040) |
| Phạm vi | Cấu trúc payload hợp đồng và quy tắc tiêu thụ sự kiện nguồn tại Consumer M07 |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy chuẩn giao thức hợp đồng nhận sự kiện giữa M03/M04 và M07.

- **Ràng buộc Chu kỳ Ngày Nhiệm vụ (`Daily Quest Reset Boundary Invariant`)**: Sự kiện phát ra trong chu kỳ ngày nào (00:00 - 23:59 UTC dựa trên `OccurredAtUtc`) CHỈ ĐƯỢC tính tiến độ cho tập Nhiệm vụ ngày của chính chu kỳ đó.
- **Ràng buộc Trạng thái Chờ nhận (`CLAIMABLE State Invariant`)**: Khi tiến độ nhiệm vụ đạt $100\%$ target ($CurrentCount \ge TargetCount$), trạng thái nhiệm vụ BẮT BUỘC chuyển sang `CLAIMABLE`.

## 2. Dynamic Quest Progress Consumer Code

```csharp
public class QuestProgressConsumer : IConsumer<LearningSessionCompletedEvent>
{
    private readonly IQuestService _questService;
    
    public async Task Consume(ConsumeContext<LearningSessionCompletedEvent> context)
    {
        var msg = context.Message;
        
        // 1. Kiểm tra Idempotency
        if (await _questService.IsEventProcessedAsync(msg.EventId)) return;
        
        // 2. Cập nhật tiến độ nhiệm vụ "Số phiên hoàn thành"
        await _questService.IncrementQuestProgressAsync(
            msg.UserId, 
            QuestCategory.DAILY_LEARNING, 
            amount: 1, 
            msg.OccurredAtUtc
        );
        
        // 3. Cập nhật tiến độ nhiệm vụ "Số từ ôn tập"
        await _questService.IncrementQuestProgressAsync(
            msg.UserId, 
            QuestCategory.DAILY_REVIEW, 
            amount: msg.Summary.TotalWords, 
            msg.OccurredAtUtc
        );
        
        await _questService.MarkEventProcessedAsync(msg.EventId);
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `EC-G01`: 100% nhiệm vụ đạt $CurrentCount \ge TargetCount$ tự động chuyển sang `CLAIMABLE`.
- `EC-G02`: Sự kiện đến muộn sau khi đã reset chu kỳ ngày mới được ghi nhận vào lịch sử nhưng không làm thay đổi nhiệm vụ ngày hôm sau.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EC12-01` | Hoàn thành phiên học giúp nâng tiến độ từ 2/3 lên 3/3 phiên | Trạng thái nhiệm vụ chuyển từ `IN_PROGRESS` sang `CLAIMABLE`. |
| `EC12-02` | Người dùng bấm "Nhận thưởng" khi trạng thái là `CLAIMABLE` | Chuyển `CLAIMED`, phát event sang M06 cộng thưởng. |
| `EC12-03` | Kiểm thử hoàn tất luồng M07-QUEST-EVENT-CONTRACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-EC-F01` | Bổ sung unique index `(UserId, QuestId, EventId)` trong M07 | Chống tăng lặp tiến độ khi retry | M07-T013 |

## 5. Tự kiểm M07-T012
- Đã đặc tả hợp đồng sự kiện nghiệp vụ M07-T012.
- Ghi nhận 2 Regression Gates (`EC-G01`–`EC-G02`) và 3 Test Cases (`EC12-01`–`EC12-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả hợp đồng sự kiện nghiệp vụ M07-T012 | WSA-7K2 |
