# Xác định từ khó và khoảng trống học tập M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-HARD-WORDS-LEARNING-GAPS-1.0` |
| Task | M04-T033 |
| Đầu vào | M04-REVIEW-LOG-SCHEMA-1.0 (M04-T024), M04-USER-PROGRESS-METRICS-1.0 (M04-T032) |
| Phạm vi | Thuật toán nhận diện từ vựng khó (`Hard Words Detection`) và khoảng trống kiến thức (`Learning Gaps Analyzer`), dựa trên tần suất trả lời sai lặp lại và chỉ số EaseFactor thấp |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa quy tắc phát hiện các mục từ khó (`Hard Words`) và khoảng trống kiến thức (`Learning Gaps`) của người học trong M04.

- **Tiêu chí Nhận diện Từ Khó Chuẩn xác (`Hard Word Classification Invariant`)**:
  - Mục từ vựng BẮT BUỘC gắn nhãn `IsHardWord = true` khi thỏa mãn ít nhất 1 trong 2 điều kiện:
    1. Chỉ số `EaseFactor <= 1.50` (sau nhiều lần reset interval).
    2. Số lần quên/trả lời sai trong các phiên ôn `FailedReviewCount >= 3` trong 30 ngày gần nhất.
- **Tự động Loại bỏ Nội dung Thu hồi (`Quarantine Clean Invariant`)**: 100% các từ nằm trong các bộ từ bị thu hồi (`RECALLED`) hoặc tạm ngưng BẮT BUỘC bị loại trừ khỏi danh sách báo cáo khoảng trống học tập.

## 2. Bảng Ma trận Nhận diện Từ khó và Khoảng trống Kiến thức (Hard Words Matrix)

| Mức độ Khó | Thuộc tính `EaseFactor` | Số lần sai (30d) | Trạng thái SRS | Gợi ý Hành động UI |
|---|---|---|---|---|
| `CRITICAL_HARD` | $EF \le 1.40$ | $\ge 4$ lần | `RELEARNING` | Đề xuất bài tập riêng (Flashcard + Gõ từ) |
| `MODERATE_HARD` | $1.40 < EF \le 1.50$ | $3$ lần | `LEARNING` | Tăng tần suất xuất hiện trong phiên ôn |
| `NORMAL_ITEM` | $EF > 1.50$ | $< 3$ lần | `REVIEWING` | Theo dõi lịch ôn chuẩn |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `HW-G01`: 100% mục từ có $EaseFactor \le 1.50$ được đánh dấu `IsHardWord = true` trong API tra cứu.
- `HW-G02`: Báo cáo khoảng trống học tập loại bỏ $100\%$ các từ thuộc bộ bị thu hồi.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HW33-01` | Learner trả lời sai từ A 3 lần trong tuần qua, $EaseFactor$ giảm xuống $1.45$ | DTO trả về `IsHardWord = true`, `HardSeverity = MODERATE_HARD`. |
| `HW33-02` | Admin thu hồi bộ từ B1 chứa từ A | Báo cáo khoảng trống học tập tự động xóa từ A khỏi danh sách gợi ý luyện tập. |
| `HW33-03` | Kiểm thử hoàn tất luồng M04-HARD-WORDS-LEARNING-GAPS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-HW-F01` | Thêm cờ `IsHardWord` trong `UserSenseProgress` Entity | Phục vụ truy vấn nhanh danh sách từ khó | M04-T002 |

## 5. Tự kiểm M04-T033
- Đã hoàn thành đặc tả `M04-HARD-WORDS-LEARNING-GAPS-1.0`.
- Chốt tiêu chí nhận diện từ khó kép ($EF \le 1.50$ hoặc sai $\ge 3$ lần/30d).
- Ghi nhận 2 Regression Gates (`HW-G01`–`HW-G02`) và 3 Test Cases (`HW33-01`–`HW33-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định từ khó và khoảng trống học tập M04-T033 | WSA-7K2 |
