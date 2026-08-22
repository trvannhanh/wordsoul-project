# Chuẩn hóa điểm duy trì M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-RETENTION-SCORE-CALCULATION-1.0` |
| Task | M04-T018 |
| Đầu vào | M04-SRS-INTERVAL-CALCULATION-1.0 (M04-T016), M04-SCHEDULE-TIMEZONE-1.0 (M04-T017) |
| Phạm vi | Thuật toán ước tính chỉ số xác suất ghi nhớ (Retention Probability / Memory Strength $R \in [0.0, 1.0]$) theo đường cong quên Ebbinghaus suy rộng |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa công thức tính chỉ số Điểm duy trì ghi nhớ (`Retention Score` $R$) cho từng nét nghĩa từ vựng trong M04.

- **Công thức Đường cong Quên Ebbinghaus Suy rộng (`Ebbinghaus Forgetting Curve Invariant`)**:
  - Tỷ lệ xác suất gợi nhớ $R$ được ước tính theo công thức suy rộng:
    $$R(t) = \exp\left( -\frac{t}{S} \right)$$
    Trong đó:
    - $t$: Số ngày đã trôi qua kể từ lần ôn cuối cùng ($t = \text{NowUtc} - \text{LastReviewedAtUtc}$).
    - $S$: Độ bền trí nhớ (Memory Stability), tính bằng $S = \text{IntervalDays} \times \frac{\text{EaseFactor}}{2.50}$.
- **Kẹp Khoảng An toàn $R \in [0.0, 1.0]$ (`Retention Score Clamping`)**: Chỉ số $R$ kẹp tuyệt đối trong khoảng $[0.0, 1.0]$ (quy đổi phần trăm $0.0\% \to 100.0\%$). Khi $t = 0$, $R = 1.0$ ($100\%$). Khi $t = \text{IntervalDays}$, $R \approx 0.368$ ($36.8\%$).

## 2. Ma trận Giá trị Điểm Duy trì Mẫu (Retention Score Decay Table)

| Khoảng ôn $Interval$ | Số ngày trôi qua $t$ | Hệ số $EF$ | Độ bền $S$ | Điểm duy trì $R$ | Trạng thái hiển thị UI |
|---|---|---|---|---|---|
| 1 ngày | 0 ngày | 2.50 | 1.0 | **$100.0\%$** | Mới ôn xong (Tươi mới) |
| 6 ngày | 3 ngày | 2.50 | 6.0 | **$60.7\%$** | Vừa phải |
| 6 ngày | 6 ngày (Đến hạn) | 2.50 | 6.0 | **$36.8\%$** | Đến hạn ôn tập |
| 6 ngày | 12 ngày (Quá hạn 6d) | 2.50 | 6.0 | **$13.5\%$** | Quá hạn sâu (Nguy cơ quên) |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RS-G01`: 100% giá trị `RetentionScore` tính ra nằm kẹp trong khoảng $[0.0, 1.0]$.
- `RS-G02`: $t = 0$ luôn cho kết quả $RetentionScore = 1.0$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RS18-01` | Tính điểm duy trì ngay sau khi vừa hoàn thành phiên học ($t = 0$) | Trả về `RetentionScore = 1.0` ($100\%$). |
| `RS18-02` | Từ vựng $Interval = 6$ ngày, đã trôi qua $t = 6$ ngày kể từ lần ôn cuối | Trả về `RetentionScore` xấp xỉ $0.368$ ($36.8\%$). |
| `RS18-03` | Kiểm thử hoàn tất luồng M04-RETENTION-SCORE-CALCULATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-RS-F01` | Thêm thuộc tính `RetentionScore` kiểu `double` trong DTO tra cứu tiến độ M04 | Cung cấp chỉ số thanh đo tiến độ cho UI | M04-T032 |

## 5. Tự kiểm M04-T018
- Đã hoàn thành đặc tả `M04-RETENTION-SCORE-CALCULATION-1.0`.
- Chốt công thức Ebbinghaus suy rộng $R(t) = \exp(-t/S)$ và kẹp khoảng $[0.0, 1.0]$.
- Ghi nhận 2 Regression Gates (`RS-G01`–`RS-G02`) và 3 Test Cases (`RS18-01`–`RS18-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa điểm duy trì M04-T018 | WSA-7K2 |
