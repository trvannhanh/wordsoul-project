# Lập danh mục sự kiện nguồn M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-EVENT-CATALOG-1.0` |
| Task | M07-T011 |
| Đầu vào | M07-QUEST-ACHIEVEMENT-TAXONOMY-1.0 (M07-T002), M03-SESSION-COMPLETED-EVENT-1.0 (M03-T040) |
| Phạm vi | Danh mục các sự kiện nghiệp vụ nguồn được phép kích hoạt cập nhật tiến độ nhiệm vụ ngày (`DailyQuestProgress`) trong M07 |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định danh mục các sự kiện nguồn được phép lắng nghe để tính toán tiến độ nhiệm vụ M07.

- **Tính Khép kín của Tín hiệu Nguồn (`Closed Source Signal Invariant`)**: Tiến độ nhiệm vụ ngày trong Giai đoạn B CHỈ ĐƯỢC CẬP NHẬT từ 2 sự kiện chính thức:
  1. `LearningSessionCompletedEvent` (phát từ M03 khi hoàn thành phiên học/ôn).
  2. `ItemReviewedEvent` (phát từ M04 khi hoàn thành 1 từ đến hạn).
  CẤM tiếp nhận các tín hiệu tiến độ không xuất phát từ M03/M04.

## 2. Bảng Danh mục Sự kiện Nguồn Nhiệm vụ (Quest Source Event Catalog)

| Mã Sự kiện | Module Nguồn | Đơn vị tính tiến độ | Điều kiện chấp nhận | Tác động Nhiệm vụ |
|---|---|---|---|---|
| `LearningSessionCompleted` | M03 | `SESSIONS_COMPLETED` | Phiên ở trạng thái `COMPLETED` | Tiến độ +1 phiên |
| `ItemReviewed` | M04 | `ITEMS_REVIEWED` | `IsCorrectFirstTry == true` | Tiến độ +N từ |
| `StreakMaintained` | M07 Engine | `STREAK_MAINTAINED` | Có nhất quyết 1 phiên học/ngày | Tiến độ Streak +1 ngày |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QC-G01`: 100% sự kiện không thuộc danh mục được cấp phép bị M07 Consumer từ chối xử lý.
- `QC-G02`: Sự kiện hoàn thành phiên bị hủy/bỏ dở không được tính vào tiến độ nhiệm vụ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QC11-01` | M07 tiêu thụ `LearningSessionCompletedEvent` của 1 phiên 10 từ | Cập nhật nhiệm vụ "Hoàn thành 1 phiên" (+1) và "Ôn tập 10 từ" (+10). |
| `QC11-02` | Nhận sự kiện từ module không ủy quyền | M07 reject event. |
| `QC11-03` | Kiểm thử hoàn tất luồng M07-QUEST-EVENT-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QC-F01` | Cần Consumer `QuestProgressConsumer` lắng nghe Message Bus | Xử lý tiến độ bất đồng bộ | M07-T012 |

## 5. Tự kiểm M07-T011
- Đã đặc tả lập danh mục sự kiện nguồn M07-T011.
- Ghi nhận 2 Regression Gates (`QC-G01`–`QC-G02`) và 3 Test Cases (`QC11-01`–`QC11-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả lập danh mục sự kiện nguồn M07-T011 | WSA-7K2 |
