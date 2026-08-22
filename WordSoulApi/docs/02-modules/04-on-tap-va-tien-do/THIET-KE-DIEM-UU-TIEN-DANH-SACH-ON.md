# Thiết kế điểm ưu tiên danh sách ôn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-PRIORITY-SCORE-1.0` |
| Task | M04-T021 |
| Đầu vào | M04-RETENTION-SCORE-CALCULATION-1.0 (M04-T018), M04-DUE-ITEM-SELECTION-CRITERIA-1.0 (M04-T020) |
| Phạm vi | Thuật toán tính Điểm ưu tiên sắp xếp hàng đợi ôn tập (`ReviewPriorityScore`), quy tắc Tie-breaking và giải thích lý do ưu tiên cho từng mục từ |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa công thức tính Điểm ưu tiên (`ReviewPriorityScore` $P$) để sắp xếp thứ tự các từ vựng đến hạn cần đưa vào phiên ôn tập trong M04.

- **Công thức Tính Điểm Ưu tiên Đa tiêu chí (`Multi-Criteria Priority Invariant`)**:
  - Điểm ưu tiên $P$ được tính theo công thức:
    $$P = (1.0 - R) \times 100 + \text{StateBonus} + \text{OverdueDays} \times 2$$
    Trong đó:
    - $R$: Điểm duy trì ghi nhớ ($R \in [0.0, 1.0]$).
    - $\text{StateBonus}$: $50$ điểm cho trạng thái `RELEARNING`, $20$ điểm cho `LEARNING`, $0$ điểm cho `REVIEWING`.
    - $\text{OverdueDays}$: Số ngày quá hạn $\max(0, \lfloor(\text{NowUtc} - \text{DueDateUtc}).\text{TotalDays}\rfloor)$.
- **Thứ tự Sắp xếp Định hình 100% (`Deterministic Sorting Invariant`)**:
  - Hàng đợi ôn tập sắp xếp giảm dần theo $P$.
  - Nếu $P$ bằng nhau (Tie-break), ưu tiên từ có $EaseFactor$ nhỏ hơn (từ khó hơn), sau đó đến `VocabularySenseId` (Guid) để đảm bảo kết quả sắp xếp bất biến giữa các lần query.

## 2. Quy tắc Phân hạng và Sắp xếp Ưu tiên (Priority Queue Rule Matrix)

| Trạng thái | Điểm duy trì $R$ | Overdue | StateBonus | Point Range $P$ | Thứ tự Ưu tiên trong Queue |
|---|---|---|---|---|---|
| `RELEARNING` | $0.20$ | 2 ngày | $+50$ | $80 + 50 + 4 = \mathbf{134}$ | **Hàng đầu tiên (Rất cao)** |
| `LEARNING` | $0.35$ | 0 ngày | $+20$ | $65 + 20 + 0 = \mathbf{85}$ | **Hàng thứ hai (Cao)** |
| `REVIEWING` | $0.15$ | 5 ngày | $+0$ | $85 + 0 + 10 = \mathbf{95}$ | **Hàng thứ ba (Trung bình)** |
| `REVIEWING` | $0.368$ | 0 ngày | $+0$ | $63.2 + 0 + 0 = \mathbf{63.2}$ | **Hàng cuối (Bình thường)** |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PS-G01`: 100% mục từ ở trạng thái `RELEARNING` luôn có điểm ưu tiên $P$ cao hơn các từ `REVIEWING` thông thường.
- `PS-G02`: Cùng một tập dữ liệu đầu vào luôn trả về kết quả thứ tự sắp xếp hàng đợi ôn khớp 100%.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PS21-01` | Queue gồm từ A (`RELEARNING`, $R=0.2$) và từ B (`REVIEWING`, $R=0.1$) | Từ A được xếp đứng trước từ B do nhận $StateBonus = +50$. |
| `PS21-02` | Queue gồm 2 từ có điểm $P = 85.0$ bằng nhau | Sắp xếp từ có $EaseFactor$ nhỏ hơn đứng trước. |
| `PS21-03` | Kiểm thử hoàn tất luồng M04-REVIEW-PRIORITY-SCORE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PS-F01` | Thêm thuộc tính `PriorityScore` trong DTO trả về cho M03 khi lấy danh sách ôn | M03 dùng thứ tự này để đưa vào phiên ôn | M04-T023 |

## 5. Tự kiểm M04-T021
- Đã hoàn thành đặc tả `M04-REVIEW-PRIORITY-SCORE-1.0`.
- Chốt công thức điểm ưu tiên $P$ và quy tắc Tie-break định hình 100%.
- Ghi nhận 2 Regression Gates (`PS-G01`–`PS-G02`) và 3 Test Cases (`PS21-01`–`PS21-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế điểm ưu tiên danh sách ôn M04-T021 | WSA-7K2 |
