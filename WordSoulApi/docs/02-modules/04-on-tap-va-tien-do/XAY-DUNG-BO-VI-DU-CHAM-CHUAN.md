# Xây dựng bộ ví dụ chấm chuẩn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-STANDARD-GRADING-EXAMPLES-1.0` |
| Task | M04-T014 |
| Đầu vào | M04-QUALITY-RATING-POLICY-1.0 (M04-T011), M04-RESPONSE-SPEED-THRESHOLDS-1.0 (M04-T012), M04-HINT-RETRY-IMPACT-1.0 (M04-T013) |
| Phạm vi | Tập kiểm thử ví dụ chấm chuẩn (`Golden Grading Test Matrix`) bao phủ đầy đủ 6 mức điểm $q \in [0, 5]$ và các kịch bản biên bất thường |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này xây dựng tập bộ mẫu ví dụ chấm chuẩn (`Golden Grading Dataset`) phục vụ kiểm thử tự động thuật toán chấm điểm SRS trong M04.

- **Độ bao phủ Kịch bản 100% (`100% Case Coverage Invariant`)**:
  - Tập ví dụ chấm chuẩn BẮT BUỘC bao phủ đủ 6 mức điểm chất lượng $q \in \{0, 1, 2, 3, 4, 5\}$.
  - Mọi thay đổi code trong `QualityRatingCalculator` BẮT BUỘC phải vượt qua 100% các case mẫu trong tài liệu này mà không làm biến đổi kết quả mong đợi.

## 2. Bảng Bộ Mẫu Ví dụ Chấm chuẩn (Golden Grading Test Dataset)

| Mã Mẫu | Dạng Thẻ | Đúng/Sai | Tốc độ | Số lần thử | Dùng Gợi ý | Gõ gần đúng | Điểm $q$ mong đợi | Ghi chú tác động SRS |
|---|---|---|---|---|---|---|---|---|
| `EX-01` | Trắc nghiệm | Đúng | 3.2s | 1 | Không | Không | **5** | $EF +0.10$, $Interval$ max |
| `EX-02` | Gõ từ | Đúng | 6.5s | 1 | Không | Không | **5** | $EF +0.10$, $Interval$ max |
| `EX-03` | Trắc nghiệm | Đúng | 12.0s | 1 | Không | Không | **4** | $EF \pm 0.00$, $Interval$ chuẩn |
| `EX-04` | Điền từ | Đúng | 4.1s | 1 | Có (Chữ cái đầu) | Không | **3** | Kẹp $q=3$ do dùng gợi ý |
| `EX-05` | Gõ từ | Đúng | 7.0s | 1 | Không | Có (Levenshtein 1) | **3** | Kẹp $q=3$ do gõ gần đúng |
| `EX-06` | Trắc nghiệm | Sai Lần 1, Đúng Lần 2 | 5.0s | 2 | Không | Không | **2** | Hard Fail, Reset $Interval = 1$ |
| `EX-07` | Nghe chép từ | Sai 3 lần | 25.0s | 3 | Không | Không | **1** | Blackout, Reset $Interval = 1$ |
| `EX-08` | Trắc nghiệm | Timeout | 65.0s | 0 | Không | Không | **0** | Complete Blank, Reset $Interval = 1$ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `GE-G01`: 100% các case mẫu `EX-01` đến `EX-08` chạy qua unit test trả về đúng điểm $q$ kịch bản.
- `GE-G02`: Không có kịch bản chấm điểm nào sinh ra điểm $q < 0$ hoặc $q > 5$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `GE14-01` | Chạy bộ test suite `QualityRatingCalculatorTests` với file data `GoldenGradingDataset.json` | 8/8 test cases pass 100%. |
| `GE14-02` | Thêm case biên bot auto-click `Duration = 100ms` | Clamper kẹp $200\text{ms}$, chấm theo mức $q=5$ và gắn nhãn `SUSPICIOUS_SPEED`. |
| `GE14-03` | Kiểm thử hoàn tất luồng M04-STANDARD-GRADING-EXAMPLES-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-GE-F01` | Đưa `GoldenGradingDataset.json` vào dự án test `WordSoul.Domain.Tests` | Đảm bảo CI/CD tự động kiểm tra mỗi khi build | M04-T011 |

## 5. Tự kiểm M04-T014
- Đã hoàn thành đặc tả `M04-STANDARD-GRADING-EXAMPLES-1.0`.
- Chốt 8 case mẫu chấm chuẩn bao phủ $q \in [0, 5]$.
- Ghi nhận 2 Regression Gates (`GE-G01`–`GE-G02`) và 3 Test Cases (`GE14-01`–`GE14-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng bộ ví dụ chấm chuẩn M04-T014 | WSA-7K2 |
