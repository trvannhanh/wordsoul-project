# Thiết kế chỉ số chất lượng chính sách M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-POLICY-QUALITY-METRICS-1.0` |
| Task | M04-T043 |
| Đầu vào | M04-USER-PROGRESS-METRICS-1.0 (M04-T032), M04-HARD-WORDS-LEARNING-GAPS-1.0 (M04-T033), M04-ACTIVITY-RETENTION-TRENDS-1.0 (M04-T034), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Bộ chỉ số đánh giá hiệu quả thuật toán SRS (`SRS Policy Quality Indicators`), công thức đo lường tỷ lệ giữ nhớ dài hạn và tỷ lệ quá hạn tồn đọng |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa công thức tính toán bộ chỉ số chất lượng chính sách SRS (`Policy Quality Indicators`) trong M04.

- **Đa chiều Đo lường Ngoại trừ Tỷ lệ Hoàn thành Đơn thuần (`Multi-Dimensional Quality Invariant`)**:
  - Đánh giá chính sách SRS KHÔNG ĐƯỢC PHÉP chỉ dựa vào tỷ lệ hoàn thành phiên học `SessionCompletionRate`.
  - BẮT BUỘC kết hợp 3 chỉ số cốt lõi:
    1. **Tỷ lệ Thuộc từ Dài hạn** `LongTermRetentionRate` (Số từ giữ nhớ sau 30 ngày).
    2. **Tỷ lệ Ôn lại do Quên** `RelearningRelapseRate` (Số từ bị tụt từ `REVIEWING` về `RELEARNING`).
    3. **Tỷ lệ Ôn Tồn đọng Nặng** `SevereBacklogRatio` (Số người dùng có $OverdueTotal > 100$).
- **Cảnh báo khi Chỉ số Chất lượng Tụt dốc (`Quality Degradation Alert Rule`)**: Khi `RelearningRelapseRate > 25.0\%`, hệ thống tự động bắn cảnh báo `POLICY_QUALITY_DEGRADATION_WARNING` sang M11.

## 2. Bảng Danh mục Chỉ số Chất lượng Chính sách SRS (Quality Metrics Catalog)

| Mã Chỉ số | Công thức Tính | Ngưỡng An toàn | Hành động khi Vi phạm |
|---|---|---|---|
| `LONG_TERM_RETENTION` | $\frac{\text{Count}(Interval \ge 30d \text{ and Correct)}}{\text{Total 30d Reviews}} \times 100\%$ | $\ge 75.0\%$ | Cảnh báo Admin đề xuất tăng $EF$ |
| `RELAPSE_RATE` | $\frac{\text{Count}(Transition \to RELEARNING)}{\text{Total Reviews}} \times 100\%$ | $\le 15.0\%$ | Cảnh báo `POLICY_QUALITY_DEGRADATION` |
| `SEVERE_BACKLOG_RATIO` | $\frac{\text{Count}(Users \text{ with Overdue} > 100)}{\text{Active Users}} \times 100\%$ | $\le 10.0\%$ | Kích hoạt Backlog Slicing M04-T022 |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PQ-G01`: 100% kết quả đánh giá chất lượng chính sách bao gồm đủ 3 chỉ số trong danh mục.
- `PQ-G02`: `RelearningRelapseRate > 25.0%` kích hoạt tự động 1 sự kiện cảnh báo sang M11.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PQ43-01` | Thuật toán SRS v1.5 ghi nhận tỷ lệ quên `RelapseRate = 28.0%` tuần qua | System bắn cảnh báo `POLICY_QUALITY_DEGRADATION_WARNING` tới Admin Dashboard M11. |
| `PQ43-02` | Admin tra cứu chất lượng chính sách SRS | API trả về DTO chứa cả 3 chỉ số `LongTermRetention`, `RelapseRate`, `SevereBacklogRatio`. |
| `PQ43-03` | Kiểm thử hoàn tất luồng M04-POLICY-QUALITY-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PQ-F01` | Đăng ký Cron Job `SrsPolicyQualityEvaluatorWorker` chạy 00:00 UTC hàng ngày | Tính toán chỉ số chất lượng chính sách định kỳ | M11-T012 |

## 5. Tự kiểm M04-T043
- Đã hoàn thành đặc tả `M04-POLICY-QUALITY-METRICS-1.0`.
- Chốt bộ 3 chỉ số đa chiều đo lường hiệu quả thuật toán SRS và ngưỡng cảnh báo dốc.
- Ghi nhận 2 Regression Gates (`PQ-G01`–`PQ-G02`) và 3 Test Cases (`PQ43-01`–`PQ43-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế chỉ số chất lượng chính sách M04-T043 | WSA-7K2 |
