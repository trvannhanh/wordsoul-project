# Đặc tả định nghĩa mục tiêu M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-TARGET-SPEC-1.0` |
| Task | M07-T003 |
| Đầu vào | M07-QUEST-ACHIEVEMENT-TAXONOMY-1.0 (M07-T002) |
| Phạm vi | Cấu trúc định nghĩa 1 mục tiêu nhiệm vụ (`QuestDefinition`), bao gồm ngưỡng chỉ tiêu (`TargetCount`), loại tiến độ và phần thưởng gắn kèm |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cấu trúc dữ liệu bản mẫu định nghĩa các mục tiêu nhiệm vụ ngày trong M07.

- **Tính Khả thi của Ngưỡng Chỉ tiêu (`Achievable Target Invariant`)**: Ngưỡng chỉ tiêu `TargetCount` của Nhiệm vụ ngày BẮT BUỘC nằm trong khoảng khả thi đối với người học trung bình:
  - `SESSIONS_COMPLETED`: $1 \le TargetCount \le 5$ phiên/ngày.
  - `ITEMS_REVIEWED`: $10 \le TargetCount \le 50$ từ/ngày.
- **Tính Bất biến của Định nghĩa Đã Phân bổ (`Assigned Quest Immutability Invariant`)**: Khi nhiệm vụ ngày đã được phân bổ cho người học (`UserQuest`), việc chỉnh sửa `TargetCount` hoặc phần thưởng trong template Admin CẤM làm thay đổi tiến độ của bản đã cấp.

## 2. Dynamic Quest Definition Schema

```csharp
public class QuestDefinition
{
    public Guid QuestId { get; set; }
    public string QuestCode { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    public QuestCategory Category { get; set; } // DAILY_LEARNING, DAILY_REVIEW, DAILY_STREAK
    public ProgressUnit ProgressUnit { get; set; } // SESSIONS_COMPLETED, ITEMS_REVIEWED, STREAK_MAINTAINED
    
    public int TargetCount { get; set; }
    public string RewardConfigCode { get; set; } // Mã gói thưởng M06
    
    public bool IsActiveInPhaseB { get; set; } = true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QT-G01`: 100% template `QuestDefinition` active trong Giai đoạn B có `IsActiveInPhaseB == true`.
- `QT-G02`: `TargetCount` của nhiệm vụ ôn tập kẹp trong $[10, 50]$ từ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QT03-01` | Tạo định nghĩa nhiệm vụ ngày "Ôn 20 từ" | Validation hợp lệ, `TargetCount = 20`. |
| `QT03-02` | Thử tạo nhiệm vụ ngày có `TargetCount = 500` từ | System reject với lỗi `TARGET_COUNT_OUT_OF_BOUNDS`. |
| `QT03-03` | Kiểm thử hoàn tất luồng M07-QUEST-TARGET-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QT-F01` | Bổ sung Seeder khởi tạo 5 Nhiệm vụ ngày chuẩn | Sẵn sàng dữ liệu mẫu khi khởi chạy DB | M07-T004 |

## 5. Tự kiểm M07-T003
- Đã đặc tả định nghĩa mục tiêu M07-T003.
- Ghi nhận 2 Regression Gates (`QT-G01`–`QT-G02`) và 3 Test Cases (`QT03-01`–`QT03-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả đặc tả định nghĩa mục tiêu M07-T003 | WSA-7K2 |
