# Xác định chỉ số chất lượng phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-QUALITY-METRICS-1.0` |
| Task | M03-T043 |
| Đầu vào | M03-SESSION-EVENT-CATALOG-1.0 (M03-T042), M11-REALTIME-FRESHNESS-1.0 (M11-T023) |
| Phạm vi | Bộ chỉ số chất lượng vận hành phiên học (`Session Quality Metrics`), tỷ lệ hoàn thành (`Completion Rate`), độ chính xác gợi nhớ lần đầu (`First-Try Accuracy`) và ngưỡng cảnh báo bất thường |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này định nghĩa bộ chỉ số đo lường chất lượng vận hành phiên học (`Session Quality Metrics`) trong M03.

- **Tách biệt Độ chính xác Gợi nhớ Lần đầu với Hoàn thành Phiên (`First-Try Accuracy vs Completion Invariant`)**:
  - Tỷ lệ hoàn thành phiên (`CompletionRate`) CHỈ ĐO phần trăm người dùng làm đến bước cuối.
  - Tỷ lệ chính xác gợi nhớ lần đầu (`FirstTryAccuracy`) ĐO RÊNG khả năng nhớ từ mà không qua thử lại/gợi ý. CẤM đồng nhất tỷ lệ hoàn thành $100\%$ với độ thuộc từ $100\%$.
- **Ngưỡng Cảnh báo Tốc độ Bất thường (`Abnormal Speed Threshold Alert`)**: Nếu tốc độ làm bài trung bình per item $< 1.5\text{s}$, hệ thống gắn cờ cảnh báo `SUSPICIOUS_SPEED_SESSION` để M11 theo dõi.

## 2. Bảng Danh mục Chỉ số Chất lượng Phiên (Session Quality Metrics Catalog)

| Mã Chỉ số | Tên Chỉ số | Công thức tính | Ngưỡng Theo dõi | Tác động M11 |
|---|---|---|---|---|
| `SESS_COMP_RATE` | Tỷ lệ Hoàn thành | $\frac{\text{Số phiên COMPLETED}}{\text{Tổng số phiên CREATED}} \times 100\%$ | Target $\ge 80\%$ | Đánh giá độ dài/độ khó phiên |
| `FIRST_TRY_ACC` | Độ chính xác Lần 1 | $\frac{\text{Số câu trả lời ĐÚNG ngay lần 1}}{\text{Tổng số câu hỏi}} \times 100\%$ | Target $\ge 70\%$ | Đo chất lượng ghi nhớ thực tế |
| `AVG_TIME_PER_ITEM` | Thời gian TB / Từ | $\frac{\text{Tổng thời gian làm bài thực tế}}{\text{Số mục từ trong phiên}}$ | $[5\text{s}, 45\text{s}]$ | Cảnh báo bot / gian lận |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SQ-G01`: 100% phiên hoàn thành tính toán chính xác 2 chỉ số `CompletionRate` và `FirstTryAccuracy` tách biệt.
- `SQ-G02`: Phiên học có `AVG_TIME_PER_ITEM < 1.5s` được gắn cờ `SUSPICIOUS_SPEED_SESSION`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SQ43-01` | Người học làm 10 từ, đúng 8 từ ngay lần đầu, trả lời sai 2 từ nhưng thử lại làm đúng hết để hoàn thành phiên | `CompletionRate = 100%`, `FirstTryAccuracy = 80.0%`. |
| `SQ43-02` | Bot hoàn thành phiên 10 từ trong 8 giây (TB 0.8s/từ) | Ghi nhận cờ `SUSPICIOUS_SPEED_SESSION = true` trong sự kiện hoàn thành. |
| `SQ43-03` | Kiểm thử hoàn tất luồng M03-SESSION-QUALITY-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SQ-F01` | Truyền `FirstTryAccuracy` trong payload event `LearningSessionCompleted` | Phục vụ báo cáo phân tích M11 | M03-T040 |

## 5. Tự kiểm M03-T043
- Đã hoàn thành đặc tả `M03-SESSION-QUALITY-METRICS-1.0`.
- Chốt danh mục 3 chỉ số chất lượng phiên và ngưỡng cảnh báo gian lận.
- Ghi nhận 2 Regression Gates (`SQ-G01`–`SQ-G02`) và 3 Test Cases (`SQ43-01`–`SQ43-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định chỉ số chất lượng phiên M03-T043 | WSA-7K2 |
