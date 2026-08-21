# Thiết kế chống đếm lặp M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-IDEMPOTENT-PROGRESS-1.0` |
| Task | M07-T013 |
| Đầu vào | M07-QUEST-EVENT-CONTRACT-1.0 (M07-T012), M12-RETRY-IDEMPOTENCY-1.0 (M12-T037) |
| Phạm vi | Cơ chế Idempotency chống đếm lặp tiến độ nhiệm vụ khi nhận lại cùng 1 sự kiện `EventId` từ M03/M04 |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế Idempotency đảm bảo 1 sự kiện nguồn chỉ được đếm tiến độ nhiệm vụ đúng 1 lần.

- **Tính Duy nhất của Event Tracking (`Single Event Counter Invariant`)**: Một `EventId` từ M03 CHỈ ĐƯỢC làm tăng tiến độ đếm đúng 1 lần. Gửi lặp sự kiện trùng `EventId` CẤM làm tăng thêm $CurrentCount$ của bất kỳ nhiệm vụ ngày nào.
- **Ràng buộc Khóa Chống lặp (`Event Tracking Log Invariant`)**: Lưu vết mốc tiêu thụ trong bảng `QuestEventLogs` với bộ khóa chính `(UserId, QuestId, EventId)`.

## 2. Dynamic Idempotent Quest Progress Update Logic

```csharp
public async Task IncrementQuestProgressIdempotentAsync(Guid userId, Guid questId, int amount, string eventId)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    // 1. Kiểm tra xem EventId đã được tính cho Quest này chưa
    bool alreadyCounted = await _dbContext.QuestEventLogs.AnyAsync(l => 
        l.UserId == userId && l.QuestId == questId && l.EventId == eventId);
        
    if (alreadyCounted)
    {
        await transaction.RollbackAsync();
        return; // Skip, idempotent ack
    }
    
    // 2. Cập nhật tiến độ nhiệm vụ
    var userQuest = await _dbContext.UserQuests.FirstOrDefaultAsync(q => q.UserId == userId && q.QuestId == questId);
    if (userQuest != null && userQuest.State == QuestState.IN_PROGRESS)
    {
        userQuest.CurrentCount += amount;
        if (userQuest.CurrentCount >= userQuest.TargetCount)
        {
            userQuest.State = QuestState.CLAIMABLE;
        }
        
        // 3. Ghi log event đã tính
        _dbContext.QuestEventLogs.Add(new QuestEventLog {
            LogId = Guid.NewGuid(),
            UserId = userId,
            QuestId = questId,
            EventId = eventId,
            ProcessedAtUtc = DateTime.UtcNow
        });
        
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IP-G01`: Gửi 5 message trùng `EventId` chỉ làm tăng `CurrentCount` đúng `+1`.
- `IP-G02`: Bảng `QuestEventLogs` lưu trữ đúng mã `EventId` tham chiếu.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IP13-01` | Message hoàn thành phiên được bus gửi 3 lần | Tiến độ nhiệm vụ chỉ tăng đúng 1 lần. |
| `IP13-02` | Kiểm tra trạng thái nhiệm vụ sau khi nhận 3 message trùng | Trạng thái không bị nhảy đúp, `CurrentCount` chính xác. |
| `IP13-03` | Kiểm thử hoàn tất luồng M07-QUEST-IDEMPOTENT-PROGRESS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-IP-F01` | Thêm bảng `QuestEventLogs` vào DbContext M07 | Đảm bảo tính chống trùng cấp DB | M07-T014 |

## 5. Tự kiểm M07-T013
- Đã đặc tả thiết kế chống đếm lặp M07-T013.
- Ghi nhận 2 Regression Gates (`IP-G01`–`IP-G02`) và 3 Test Cases (`IP13-01`–`IP13-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế chống đếm lặp M07-T013 | WSA-7K2 |
