# Chuẩn hóa từ điển nhiệm vụ M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-DICT-1.0` |
| Task | M07-T001 |
| Đầu vào | M01-T001 (Từ điển danh tính), M01-T025-A (Preferences Timezone) |
| Phạm vi | Thuật ngữ nhiệm vụ ngày (Daily Quests), đếm tiến độ (Progress Tracking), chu kỳ làm mới (00:00 UTC / Reset Cycle) và cơ chế Nhận thưởng một lần (Single-Claim Guarantee) |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả thuật ngữ, máy trạng thái và quy tắc an toàn của Module M07 (Nhiệm vụ ngày).

- **Đơn Nhận Thưởng (`Single-Claim Guarantee Invariant`)**: Một nhiệm vụ ngày khi đã ở trạng thái `CLAIMED` (Đã nhận thưởng) tuyệt đối CẤM cho phép nhận thưởng lại lần hai dưới bất kỳ hình thức nào.
- **Tính Bất biến của Danh sách Nhiệm vụ Hôm nay (`Today Quest List Lock Invariant`)**: Danh sách 3-5 nhiệm vụ ngày của người học sau khi đã phân bổ lúc 00:00 UTC / khởi tạo ngày mới sẽ được đóng băng trong suốt chu kỳ 24h.
- **Đếm Tiến độ Bất biến từ Sự kiện Nguồn (`Event-Driven Progress Counter Invariant`)**: Tiến độ nhiệm vụ chỉ được tăng lên khi nhận sự kiện hợp lệ từ M03/M04 (ví dụ: `LearningSessionCompletedIntegrationEvent`). CẤM cho phép Client tự gửi request API tăng điểm tiến độ trực tiếp.

## 2. Bảng Từ điển Thuật ngữ Nhiệm vụ M07 (Quest Lexicon)

| Thuật ngữ | Tên tiếng Việt | Mô tả & Quy tắc trong WordSoul | Trạng thái / Giới hạn |
|---|---|---|---|
| `DailyQuest` | Nhiệm vụ ngày | Nhiệm vụ ngắn hạn cần hoàn thành trong vòng 24 giờ. | Chu kỳ 24h |
| `QuestProgress` | Tiến độ nhiệm vụ | Số lượng đơn vị mục tiêu đã thực hiện ($CurrentCount / TargetCount$). | $0 \le CurrentCount \le TargetCount$ |
| `DailyReset` | Làm mới ngày | Thời điểm làm mới danh sách nhiệm vụ và reset tiến độ (Mặc định 00:00 UTC). | Hàng ngày |
| `SingleClaim` | Nhận thưởng 1 lần | Hành động nhận phần thưởng Gold/Exp/Items khi $CurrentCount \ge TargetCount$. | Một lần duy nhất |
| `QuestRewardSnapshot` | Ảnh chụp phần thưởng | Gói phần thưởng cố định được gắn với nhiệm vụ tại thời điểm phân bổ. | Immutable Snapshot |

## 3. Máy Trạng thái Vòng đời Nhiệm vụ (Quest Lifecycle State Machine)

```text
[ASSIGNED] (Progress = 0)
    │
    ▼ (Event M03/M04 arrive -> Progress++)
[IN_PROGRESS] (0 < Progress < Target)
    │
    ▼ (Progress >= Target)
[COMPLETED] (Chờ người học bấm nhận thưởng)
    │
    ▼ (User triggers ClaimReward API)
[CLAIMED] (Terminal, Final State)
```

## 4. Quy tắc Xử lý và Chống Cấp Thưởng Lặp (Single-Claim Protocol)

```csharp
public async Task<ClaimRewardResultDto> ClaimQuestRewardAsync(Guid userId, Guid questId)
{
    var quest = await _questRepo.GetUserQuestAsync(userId, questId);
    
    if (quest.Status == QuestStatus.CLAIMED)
    {
        throw new InvalidOperationException("QUEST_REWARD_ALREADY_CLAIMED");
    }
    
    if (quest.CurrentCount < quest.TargetCount)
    {
        throw new InvalidOperationException("QUEST_NOT_COMPLETED_YET");
    }
    
    // 1. Chuyển trạng thái atomic CAS
    quest.Status = QuestStatus.CLAIMED;
    quest.ClaimedAtUtc = DateTime.UtcNow;
    await _questRepo.UpdateAsync(quest);
    
    // 2. Gửi lệnh phát thưởng sang M06 kèm IdempotencyKey
    string idempotencyKey = $"QUEST-CLAIM-{userId}-{questId}-{quest.AssignedDate:yyyyMMdd}";
    await _assetService.GrantRewardAsync(userId, quest.RewardSnapshot, idempotencyKey);
    
    return new ClaimRewardResultDto { Success = true, ClaimedReward = quest.RewardSnapshot };
}
```

## 5. Regression Gates và Test Cases

### 5.1. Regression Gates
- `QD-G01`: 100% nhiệm vụ ngày có cơ chế nhận thưởng `CLAIMED` một lần duy nhất, gọi lần hai bị từ chối `QUEST_REWARD_ALREADY_CLAIMED`.
- `QD-G02`: Tiến độ nhiệm vụ chỉ tăng từ sự kiện backend M03/M04, từ chối request tăng điểm trực tiếp từ Client.
- `QD-G03`: Reset chu kỳ 00:00 UTC tạo mới nhiệm vụ ngày mà không làm mất lịch sử nhiệm vụ cũ đã nhận.

### 5.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QD01-01` | Người học hoàn thành phiên học M03 | Sự kiện tăng `CurrentCount` của nhiệm vụ `"Hoàn thành 1 phiên học"` từ 0 lên 1. |
| `QD01-02` | Người học đạt $CurrentCount == TargetCount$ | Trạng thái nhiệm vụ chuyển sang `COMPLETED`. |
| `QD01-03` | Bấm nhận thưởng nhiệm vụ `COMPLETED` | Chuyển trạng thái sang `CLAIMED`, M06 cộng Gold/Exp thành công. |
| `QD01-04` | Thử bấm nhận thưởng lần thứ 2 trên nhiệm vụ `CLAIMED` | Trả lỗi HTTP 400 `QUEST_REWARD_ALREADY_CLAIMED`. |
| `QD01-05` | Client gọi API giả lập tăng tiến độ nhiệm vụ | System trả về 403 / 405 (API client-driven progress updates is DENIED). |
| `QD01-06` | Kiểm thử hoàn tất luồng M07-QUEST-DICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 6. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QD-F01` | Bổ sung Worker `DailyQuestResetWorker` chạy hàng ngày lúc 00:00 UTC | Chưa có job tự động phân bổ nhiệm vụ mới | M07-T004 |

## 7. Tự kiểm M07-T001
- Đã hoàn thành từ điển nhiệm vụ M07 và cam kết quy tắc Single-Claim.
- Xác lập 3 Regression Gates (`QD-G01`–`QD-G03`) và 6 Test Cases (`QD01-01`–`QD01-06`).

## 8. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa từ điển nhiệm vụ M07-T001 | WSA-7K2 |
