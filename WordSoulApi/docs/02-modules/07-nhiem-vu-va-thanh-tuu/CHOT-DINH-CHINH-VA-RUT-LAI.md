# Chốt đính chính và rút lại M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-CORRECTION-ROLLBACK-1.0` |
| Task | M07-T016 |
| Đầu vào | M07-QUEST-IDEMPOTENT-PROGRESS-1.0 (M07-T013), M07-CONCURRENCY-ORDERING-1.0 (M07-T014) |
| Phạm vi | Quy trình đính chính hoặc thu hồi tiến độ nhiệm vụ ngày khi phát hiện sự kiện bị gian lận hoặc lỗi dữ liệu kỹ thuật từ M03 |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế thu hồi/đính chính tiến độ nhiệm vụ và phần thưởng liên đới trong M07.

- **Bàn giao Thu hồi Sang M06 (`Compensation Delegation Invariant`)**: Nếu một nhiệm vụ bị phát hiện gian lận và thu hồi sau khi người học đã bấm nhận thưởng (`CLAIMED`), M07 CẤM tự ý trừ âm tài sản. M07 BẮT BUỘC gửi lệnh đính chính `RewardCompensationEvent` sang M06 để tạo bản ghi biến động trừ bù trong `AssetLedgerEntries`.
- **Minh bạch Lịch sử Đính chính (`Audit Correction History Invariant`)**: Mọi thao tác rút lại tiến độ BẮT BUỘC ghi bản ghi vào `QuestCorrectionAuditLogs`.

## 2. Dynamic Quest Progress Rollback Logic

```csharp
public async Task RollbackQuestProgressAsync(Guid userId, Guid questId, int amount, string reason)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    var userQuest = await _dbContext.UserQuests.FirstOrDefaultAsync(q => q.UserId == userId && q.QuestId == questId);
    if (userQuest == null) return;
    
    // 1. Giảm tiến độ (không âm)
    userQuest.CurrentCount = Math.Max(0, userQuest.CurrentCount - amount);
    
    // 2. Nếu đã CLAIMED, phát event sang M06 để bù tài sản
    if (userQuest.State == QuestState.CLAIMED)
    {
        await _bus.Publish(new RewardCompensationEvent {
            UserId = userId,
            SourceModule = "M07_QUEST",
            ReferenceEventId = questId.ToString(),
            Reason = reason
        });
    }
    else if (userQuest.CurrentCount < userQuest.TargetCount)
    {
        userQuest.State = QuestState.IN_PROGRESS;
    }
    
    // 3. Ghi log audit
    _dbContext.QuestCorrectionAuditLogs.Add(new QuestCorrectionAuditLog {
        LogId = Guid.NewGuid(),
        UserId = userId,
        QuestId = questId,
        AmountDeducted = amount,
        Reason = reason,
        CreatedAtUtc = DateTime.UtcNow
    });
    
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `CR-G01`: 100% lệnh thu hồi tiến độ trên nhiệm vụ đã `CLAIMED` phát hành thành công `RewardCompensationEvent` sang M06.
- `CR-G02`: $CurrentCount$ sau khi rút lại không bao giờ bị âm ($CurrentCount \ge 0$).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CR16-01` | Rút lại 1 phiên gian lận khi nhiệm vụ đang `IN_PROGRESS` (2/3) | Tiến độ giảm về $1/3$, trạng thái giữ `IN_PROGRESS`. |
| `CR16-02` | Rút lại phiên gian lận khi nhiệm vụ đã `CLAIMED` | Gửi tin sang M06 thu hồi Gold thưởng, ghi log audit. |
| `CR16-03` | Kiểm thử hoàn tất luồng M07-QUEST-CORRECTION-ROLLBACK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-CR-F01` | Cần Consumer `RewardCompensationConsumer` tại M06 | Xử lý biến động tài sản trừ bù | M06-T017 |

## 5. Tự kiểm M07-T016
- Đã đặc tả chốt đính chính và rút lại M07-T016.
- Ghi nhận 2 Regression Gates (`CR-G01`–`CR-G02`) và 3 Test Cases (`CR16-01`–`CR16-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt đính chính và rút lại M07-T016 | WSA-7K2 |
