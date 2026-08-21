# Phân loại nhiệm vụ và thành tựu M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-ACHIEVEMENT-TAXONOMY-1.0` |
| Task | M07-T002 |
| Đầu vào | M07-QUEST-DICT-1.0 (M07-T001) |
| Phạm vi | Phân loại nhiệm vụ ngày (`DailyQuest`), nhiệm vụ tuần (`WeeklyQuest`) và thành tựu dài hạn (`Achievement`), ranh giới hoãn tính năng thành tựu dài hạn |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này phân loại phân khúc nhiệm vụ và xác định ranh giới tính năng nhiệm vụ cho M07.

- **Bảo lưu Hoãn Thành tựu Dài hạn (`Achievement Postponement Invariant`)**:
  - Giai đoạn B CHỈ kích hoạt Nhiệm vụ ngày (`DailyQuest`).
  - Thành tựu dài hạn (`Long-Term Achievement`) tiếp tục giữ ở trạng thái TẮT / Hoãn lại theo đúng thiết kế scope freeze.
- **Phân loại Đơn vị Đếm Tiến độ (`Progress Unit Taxonomy`)**: Nhiệm vụ ngày được đếm theo 3 đơn vị chính:
  - `SESSIONS_COMPLETED`: Số phiên học hoàn thành trong ngày (M03).
  - `ITEMS_REVIEWED`: Số từ vựng ôn tập trong ngày (M04).
  - `STREAK_MAINTAINED`: Duy trì chuỗi ngày học liên tục.

## 2. Bảng Phân loại Nhiệm vụ (Quest Taxonomy Matrix)

| Nhóm Nhiệm vụ | Chu kỳ | Kích hoạt B | Đơn vị đếm tiến độ | Thưởng mặc định |
|---|---|---|---|---|
| `DAILY_LEARNING` | 24 giờ | **CÓ** | Số phiên học / số từ mới | Gold + Exp |
| `DAILY_REVIEW` | 24 giờ | **CÓ** | Số từ ôn tập đến hạn | Gold + Exp |
| `DAILY_STREAK` | 24 giờ | **CÓ** | 1 bài học/ngày | Streak Count + Gems |
| `LONG_TERM_ACHIEVEMENT` | Vĩnh viễn | **TẮT** | Hoãn lại Giai đoạn C | Hoãn lại |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QT-G01`: 100% nhiệm vụ hoạt động trong Giai đoạn B thuộc loại `DAILY_QUEST`.
- `QT-G02`: Các API thành tựu dài hạn trả về HTTP 404 / 503 `FEATURE_DISABLED`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QT02-01` | Người học truy vấn danh sách nhiệm vụ hôm nay | Trả về 3-5 `DailyQuest`, không chứa `Achievement`. |
| `QT02-02` | Gọi API nhận thưởng thành tựu dài hạn chưa kích hoạt | System trả về lỗi `FEATURE_DISABLED`. |
| `QT02-03` | Kiểm thử hoàn tất luồng M07-QUEST-ACHIEVEMENT-TAXONOMY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QT-F01` | Enum `QuestCategory` cần gắn nhãn `IsActiveInPhaseB` | Đảm bảo chặn API nhiệm vụ hoãn | M07-T003 |

## 5. Tự kiểm M07-T002
- Đã phân loại nhiệm vụ M07-T002 và cam kết hoãn thành tựu dài hạn.
- Ghi nhận 2 Regression Gates (`QT-G01`–`QT-G02`) và 3 Test Cases (`QT02-01`–`QT02-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả phân loại nhiệm vụ và thành tựu M07-T002 | WSA-7K2 |
