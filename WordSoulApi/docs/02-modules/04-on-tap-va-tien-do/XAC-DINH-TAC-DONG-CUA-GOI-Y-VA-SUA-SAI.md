# Xác định tác động của gợi ý và sửa sai M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-HINT-RETRY-IMPACT-1.0` |
| Task | M04-T013 |
| Đầu vào | M03-HINT-COST-POLICY-1.0 (M03-T030), M04-QUALITY-RATING-POLICY-1.0 (M04-T011) |
| Phạm vi | Quy định mức phạt điểm chất lượng SRS $q$ khi người học sử dụng gợi ý (`Hint`) hoặc phải thử lại nhiều lần (`Retries`) để trả lời đúng |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc tính mức phạt điểm chất lượng $q$ trong thuật toán SRS M04 khi có yếu tố sử dụng gợi ý hoặc thử lại nhiều lần trong M03.

- **Kẹp Điểm Tối đa khi Dùng Gợi ý (`Hint Penalty Ceiling Invariant`)**:
  - Việc sử dụng bất kỳ loại gợi ý nào (`UsedHintCount > 0`) trong câu hỏi kẹp điểm chất lượng tối đa là $q = 3$ (Pass). Tuyệt đối CẤM cho điểm $q = 4$ hoặc $q = 5$ đối với câu hỏi có trợ giúp gợi ý.
- **Phạt khi Thử lại Nhiều lần (`Retry Penalty Rules`)**:
  - Trả lời ĐÚNG ở lần thử thứ 1: Không bị phạt.
  - Trả lời ĐÚNG ở lần thử thứ 2: Điểm $q = 2$ (coi như Hard Fail, reset $Interval = 1$).
  - Trả lời ĐÚNG ở lần thử thứ 3: Điểm $q = 1$ (coi như Blackout).

## 2. Bảng Ma trận Tác động Gợi ý và Thử lại (Hint & Retry Impact Matrix)

| Yếu tố | Số lần thử | Điểm $q$ tối đa | Trạng thái SRS sau phiên |
|---|---|---|---|
| Không gợi ý | 1 | **5** (Perfect) | $Interval$ tăng tối đa |
| Có gợi ý | 1 | **3** (Pass) | $Interval$ tăng nhẹ |
| Không/Có gợi ý | 2 | **2** (Hard Fail) | Reset $Interval = 1$ ngày |
| Không/Có gợi ý | $\ge 3$ | **1** (Blackout) | Reset $Interval = 1$ ngày |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `HR-G01`: 100% kết quả câu hỏi có `UsedHintCount > 0` nhận điểm chất lượng $q \le 3$.
- `HR-G02`: Trả lời đúng ở lần thử thứ 2 luôn làm reset $Interval = 1$ ngày.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HR13-01` | Người học dùng Gợi ý chữ cái đầu và trả lời đúng ngay lần 1 | Ánh xạ đạt điểm $q = 3$, $Interval$ không bị reset về 1 ngày nhưng tăng chậm hơn. |
| `HR13-02` | Người học trả lời sai lần 1, chọn lại đúng ở lần 2 | Ánh xạ đạt điểm $q = 2$, $Interval$ bị reset về 1 ngày. |
| `HR13-03` | Kiểm thử hoàn tất luồng M04-HINT-RETRY-IMPACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-HR-F01` | Tích hợp thuộc tính `InitialRecallAttemptCount` trong contract M03$\to$M04 | Phân biệt chính xác số lần thử khi chấm điểm | M04-T006 |

## 5. Tự kiểm M04-T013
- Đã hoàn thành đặc tả `M04-HINT-RETRY-IMPACT-1.0`.
- Chốt kẹp trần điểm $q=3$ cho gợi ý và quy tắc phạt reset khi thử lại lần 2+.
- Ghi nhận 2 Regression Gates (`HR-G01`–`HR-G02`) và 3 Test Cases (`HR13-01`–`HR13-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định tác động của gợi ý và sửa sai M04-T013 | WSA-7K2 |
