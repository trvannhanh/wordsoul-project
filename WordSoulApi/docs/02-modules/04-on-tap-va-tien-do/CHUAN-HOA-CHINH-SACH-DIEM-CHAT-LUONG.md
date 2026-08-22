# Chuẩn hóa chính sách điểm chất lượng M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-QUALITY-RATING-POLICY-1.0` |
| Task | M04-T011 |
| Đầu vào | M04-VALID-EVIDENCE-RULES-1.0 (M04-T010), M03-SUBMIT-ANSWER-DATA-1.0 (M03-T024), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Quy tắc ánh xạ kết quả gợi nhớ đầu tiên (`InitialRecall`) sang thang điểm chất lượng 6 mức SuperMemo SM-2 ($q \in [0, 5]$) |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy tắc ánh xạ từ dữ liệu gợi nhớ đầu tiên (`InitialRecall`) của M03 sang thang điểm chất lượng $q \in [0, 5]$ trong thuật toán SRS M04.

- **Định danh Điểm Chất lượng Bất biến (`Deterministic Rating Invariant`)**:
  - Với cùng một bộ tham số đầu vào (Đúng/Sai, Tốc độ trả lời, Sử dụng gợi ý, Lỗi chính tả), điểm $q$ sinh ra BẮT BUỘC như nhau 100%.
  - Thang điểm $q$ kẹp tuyệt đối trong tập số nguyên $\{0, 1, 2, 3, 4, 5\}$.
- **Điểm $q < 3$ coi như Quên (`Recall Failure Invariant`)**: Điểm $q \in \{0, 1, 2\}$ đại diện cho trả lời sai hoặc không nhớ, kích hoạt quy trình reset khoảng thời gian ôn về $Interval = 1$ ngày.

## 2. Bảng Ánh xạ Thang điểm Chất lượng (Quality Rating Mapping Table)

| Điểm $q$ | Tên Mức | Điều kiện Ánh xạ từ M03 | Tác động SRS |
|---|---|---|---|
| **5** | Perfect | Trả lời ĐÚNG lần đầu, Không gợi ý, Thời gian $< 5\text{s}$, Không lỗi chính tả | $EF$ tăng $+0.10$, $Interval$ tăng tối đa |
| **4** | Good | Trả lời ĐÚNG lần đầu, Không gợi ý, Thời gian $5\text{s} \to 15\text{s}$ | $EF$ giữ nguyên/tăng nhẹ, $Interval$ tăng chuẩn |
| **3** | Pass | Trả lời ĐÚNG lần đầu nhưng có dùng Gợi ý HOẶC Gõ gần đúng (`ALMOST_CORRECT`) | $EF$ giảm $-0.14$, $Interval$ tăng vừa |
| **2** | Hard Fail | Trả lời SAI lần 1, trả lời ĐÚNG ở lần thử thứ 2 | Reset $Interval = 1$, $EF$ giảm $-0.20$ |
| **1** | Blackout | Trả lời SAI nhiều lần (vượt loop limit) | Reset $Interval = 1$, $EF$ giảm $-0.20$ |
| **0** | Complete Blank | Không đưa ra được câu trả lời (Timeout/Bỏ qua) | Reset $Interval = 1$, $EF$ giảm $-0.20$ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QR-G01`: 100% kết quả trả lời có sử dụng gợi ý (`UsedHintCount > 0`) không bao giờ đạt điểm $q > 3$.
- `QR-G02`: $q \in [0, 5]$ luôn là số nguyên hợp lệ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QR11-01` | Trả lời đúng trong 3 giây, không dùng gợi ý | Ánh xạ đạt điểm $q = 5$. |
| `QR11-02` | Trả lời đúng trong 4 giây nhưng có dùng Gợi ý chữ cái đầu | Ánh xạ kẹp tại điểm $q = 3$. |
| `QR11-03` | Trả lời sai lần đầu, thử lại lần 2 mới đúng | Ánh xạ đạt điểm $q = 2$. |
| `QR11-04` | Kiểm thử hoàn tất luồng M04-QUALITY-RATING-POLICY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-QR-F01` | Triển khai class `QualityRatingCalculator` trong Domain M04 | Đảm bảo tính nhất quán toán học SM-2 | M04-T016 |

## 5. Tự kiểm M04-T011
- Đã hoàn thành đặc tả `M04-QUALITY-RATING-POLICY-1.0`.
- Chốt bảng ánh xạ 6 mức điểm $q \in [0, 5]$ từ bằng chứng M03.
- Ghi nhận 2 Regression Gates (`QR-G01`–`QR-G02`) và 4 Test Cases (`QR11-01`–`QR11-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa chính sách điểm chất lượng M04-T011 | WSA-7K2 |
