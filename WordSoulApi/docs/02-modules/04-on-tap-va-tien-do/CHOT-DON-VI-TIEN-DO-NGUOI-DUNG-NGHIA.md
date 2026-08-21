# Chốt đơn vị tiến độ người dùng–nghĩa M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-USER-SENSE-UNIT-1.0` |
| Task | M04-T002 |
| Đầu vào | M02-MULTI-SENSE-1.0 (M02-T002), M04-MEMORY-DICT-1.0 (M04-T001) |
| Phạm vi | Ranh giới lưu trữ và tính toán tiến độ ghi nhớ theo từng nét nghĩa độc lập (`VocabularySenseId`), xử lý dữ liệu cũ chỉ lưu theo từ vựng |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định đơn vị cơ sở nhỏ nhất để theo dõi tiến độ học tập và ghi nhớ trong WordSoul M04.

- **Đơn vị Tiến độ Nhất quán (`Sense-Level Granularity Invariant`)**:
  - Mỗi bản ghi tiến độ BẮT BUỘC lưu trữ bộ khóa chính `(UserId, VocabularySenseId)`.
  - CẤM lưu tiến độ chỉ bằng `VocabularyId` mà không có `VocabularySenseId`. Khi một từ vựng có 3 nét nghĩa (ví dụ: *bank* - ngân hàng, *bank* - bờ sông, *bank* - chất đống), người học sẽ có 3 hồ sơ ghi nhớ riêng biệt.
- **Quy tắc Chuyển đổi Dữ liệu Cũ (`Legacy Data Migration Rule`)**: Dữ liệu tiến độ cũ chưa có `VocabularySenseId` sẽ tự động được ánh xạ về Nét nghĩa chính (`PrimarySense` / `SenseOrder = 1`) của từ vựng đó.

## 2. Cấu trúc Khóa và Bản ghi Tiến độ (UserSenseProgress Schema)

```csharp
public class UserSenseProgress
{
    public Guid UserSenseProgressId { get; set; }
    public Guid UserId { get; set; }
    public Guid VocabularyId { get; set; }
    public Guid VocabularySenseId { get; set; } // Khóa định danh nét nghĩa bắt buộc
    
    public MemoryState State { get; set; } = MemoryState.NEW;
    public int IntervalDays { get; set; } = 1;
    public double EaseFactor { get; set; } = 2.50;
    public int RepetitionCount { get; set; } = 0;
    
    public DateTime? LastReviewedAtUtc { get; set; }
    public DateTime DueDateUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `US-G01`: 100% bản ghi `UserSenseProgress` có `VocabularySenseId != Guid.Empty`.
- `US-G02`: Học nét nghĩa 1 của từ đa nghĩa không làm thay đổi trạng thái ghi nhớ của nét nghĩa 2.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `US02-01` | Người học học từ *"bank"* với nét nghĩa `"Ngân hàng"` | Tạo `UserSenseProgress` cho `SenseId_1`, `SenseId_2` ("Bờ sông") vẫn ở trạng thái `NEW`. |
| `US02-02` | Thử tạo bản ghi tiến độ có `VocabularySenseId = null` | System ném exception `MISSING_SENSE_ID`. |
| `US02-03` | Kiểm thử hoàn tất luồng M04-USER-SENSE-UNIT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-US-F01` | Thêm index duy nhất `(UserId, VocabularySenseId)` trong DB | Ngăn ngừa tạo trùng hồ sơ nhớ cho cùng nét nghĩa | M04-T003 |

## 5. Tự kiểm M04-T002
- Đã chốt đơn vị tiến độ người dùng–nghĩa M04-T002.
- Ghi nhận 2 Regression Gates (`US-G01`–`US-G02`) và 3 Test Cases (`US02-01`–`US02-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt đơn vị tiến độ người dùng–nghĩa M04-T002 | WSA-7K2 |
