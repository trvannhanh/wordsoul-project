# Chốt phiên bản và ảnh chụp M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-SNAPSHOT-VERSIONING-1.0` |
| Task | M07-T005 |
| Đầu vào | M07-QUEST-TARGET-SPEC-1.0 (M07-T003), M07-QUEST-CHANGE-LIFECYCLE-1.0 (M07-T004) |
| Phạm vi | Cơ chế chụp ảnh dữ liệu (`QuestSnapshot`) khi phân bổ nhiệm vụ ngày cho người dùng, đảm bảo tính ổn định của chỉ tiêu và phần thưởng |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc đóng băng ảnh chụp chỉ tiêu và phần thưởng cho bản nhiệm vụ ngày cá nhân.

- **Tính Bất biến của Ảnh chụp Nhiệm vụ (`Assigned Quest Snapshot Invariant`)**: Khi một nhiệm vụ ngày được phân bổ cho người học (`UserQuest`), các thuộc tính `Title`, `TargetCount`, `RewardConfigCode` BẮT BUỘC được lưu dưới dạng ảnh chụp (Snapshot) trong bảng `UserQuests`. Việc Admin chỉnh sửa template gốc sau mốc phân bổ CẤM làm thay đổi dữ liệu ảnh chụp này.
- **Ràng buộc Phiên bản Template (`Template Version Tracking Invariant`)**: Mỗi ảnh chụp `UserQuest` BẮT BUỘC lưu trữ đúng mã `TemplateVersion` được dùng để phân bổ.

## 2. Dynamic User Quest Snapshot Schema

```csharp
public class UserQuest
{
    public Guid UserQuestId { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestId { get; set; }
    
    public string BusinessDayKey { get; set; } // yyyy-MM-dd (UTC)
    public int TemplateVersion { get; set; }
    
    // Dữ liệu Snapshot bất biến
    public string SnapshotTitle { get; set; }
    public int TargetCount { get; set; }
    public string SnapshotRewardCode { get; set; }
    
    public int CurrentCount { get; set; }
    public QuestState State { get; set; } // IN_PROGRESS, CLAIMABLE, CLAIMED
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QS-G01`: 100% bản ghi `UserQuest` chứa đầy đủ 3 thuộc tính snapshot `SnapshotTitle`, `TargetCount`, `SnapshotRewardCode`.
- `QS-G02`: Chỉnh sửa template gốc không thay đổi `TargetCount` của các `UserQuest` đã sinh ra trước đó.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QS05-01` | Khởi tạo nhiệm vụ ngày "Ôn 20 từ" lúc 00:01 UTC | Bản ghi `UserQuest` lưu `TargetCount = 20`. |
| `QS05-02` | Admin sửa template gốc "Ôn 20 từ" $\to$ "Ôn 30 từ" lúc 10:00 | Nhiệm vụ đã phân bổ giữ nguyên `TargetCount = 20`. |
| `QS05-03` | Kiểm thử hoàn tất luồng M07-QUEST-SNAPSHOT-VERSIONING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QS-F01` | Cần thuộc tính `TemplateVersion` trong `UserQuests` | Đảm bảo khả năng truy vết dữ liệu gốc | M07-T006 |

## 5. Tự kiểm M07-T005
- Đã đặc tả chốt phiên bản và ảnh chụp M07-T005.
- Ghi nhận 2 Regression Gates (`QS-G01`–`QS-G02`) và 3 Test Cases (`QS05-01`–`QS05-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt phiên bản và ảnh chụp M07-T005 | WSA-7K2 |
