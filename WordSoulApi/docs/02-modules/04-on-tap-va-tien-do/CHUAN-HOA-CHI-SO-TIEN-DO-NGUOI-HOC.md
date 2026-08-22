# Chuẩn hóa chỉ số tiến độ người dùng M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-USER-PROGRESS-METRICS-1.0` |
| Task | M04-T032 |
| Đầu vào | M04-SCHEDULE-TIMEZONE-1.0 (M04-T017), M04-RETENTION-SCORE-CALCULATION-1.0 (M04-T018), M04-DUE-ITEM-SELECTION-CRITERIA-1.0 (M04-T020) |
| Phạm vi | Bộ chỉ số tổng hợp tiến độ học tập cá nhân của người học (`UserProgressMetricsSummary`), bao gồm tổng số từ đã thuộc (`MasteredWords`), số từ đang học (`LearningWords`), chỉ số thuộc từ trung bình (`AverageRetentionScore`) và chuỗi ngày học (`StreakDays`) |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa công thức và bộ chỉ số tiến độ tổng hợp (`UserProgressMetricsSummary`) hiển thị trên bảng điều khiển cá nhân trong M04.

- **Đếm Không Trùng lặp theo Nét nghĩa (`Unique Sense Counting Invariant`)**:
  - Các chỉ số `TotalLearnedSenses`, `MasteredSensesCount`, `LearningSensesCount` BẮT BUỘC đếm theo cặp khóa duy nhất `(UserId, VocabularySenseId)`.
  - Tuyệt đối CẤM đếm theo mặt chữ thô (Headword) gây sai lệch dữ liệu khi một từ có nhiều nét nghĩa khác nhau.
- **Tính toán Đúng Múi giờ Địa phương (`Timezone Consistent Streak Invariant`)**: Chuỗi ngày học `StreakDays` được xác định dựa trên ranh giới 00:00:00 - 23:59:59 của múi giờ cá nhân người học `UserTimeZoneId`.

## 2. Bảng Danh mục Chỉ số Tiến độ Cá nhân (User Progress Metrics Catalog)

| Mã Chỉ số | Tên Chỉ số | Công thức tính | Mục đích sử dụng |
|---|---|---|---|
| `TOTAL_LEARNED` | Tổng nét nghĩa đã học | $\text{Count}(UserSenseProgress)$ | Báo cáo quy mô vốn từ |
| `MASTERED_COUNT` | Số từ đã thuộc | $\text{Count}(State == MASTERED)$ | Đo lường năng lực dài hạn |
| `AVG_RETENTION` | Điểm duy trì TB | $\frac{\sum RetentionScore}{TotalLearnedSenses}$ | Đo độ "tươi" của bộ nhớ |
| `DUE_TODAY_COUNT` | Số từ đến hạn hôm nay | $\text{Count}(DueDateUtc \le LocalDayEndUtc)$ | Nhắc nhở khối lượng ôn ngày |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `UP-G01`: 100% chỉ số đếm số từ được thực hiện đếm theo `VocabularySenseId` duy nhất.
- `UP-G02`: `AverageRetentionScore` tính ra kẹp chính xác trong khoảng $[0.0, 1.0]$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `UP32-01` | Learner đã học 20 nét nghĩa từ vựng, có 15 nét nghĩa `MASTERED` | DTO trả về `TotalLearned = 20`, `MasteredCount = 15`. |
| `UP32-02` | Learner ở múi giờ UTC+7 thực hiện học lúc 11:30 PM | Chuỗi `StreakDays` tính nhận điểm cho ngày hôm đó theo giờ local UTC+7. |
| `UP32-03` | Kiểm thử hoàn tất luồng M04-USER-PROGRESS-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-UP-F01` | Tạo API endpoint `GET /api/v1/progress/summary` trong M04 | Cung cấp dữ liệu cho Dashboard màn chính | M04-T035 |

## 5. Tự kiểm M04-T032
- Đã hoàn thành đặc tả `M04-USER-PROGRESS-METRICS-1.0`.
- Chốt danh mục 4 chỉ số tiến độ cá nhân và nguyên tắc đếm theo nét nghĩa duy nhất.
- Ghi nhận 2 Regression Gates (`UP-G01`–`UP-G02`) và 3 Test Cases (`UP32-01`–`UP32-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa chỉ số tiến độ người dùng M04-T032 | WSA-7K2 |
