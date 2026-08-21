# Xác định giới hạn số từ trong phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-ITEM-LIMIT-1.0` |
| Task | M03-T006 |
| Đầu vào | M02-VOCAB-SET-CRITERIA-1.0 (M02-T015), M03-SESSION-POLICY-1.0 (M03-T002), M04-PROGRESS |
| Phạm vi | Quy định số lượng từ tối thiểu và tối đa trong 1 phiên học mới hoặc phiên ôn tập, thuật toán chia nhỏ bộ từ lớn |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định hạn mức số lượng từ vựng được đưa vào trong 1 phiên học nhằm tối ưu khả năng ghi nhớ ngắn hạn của não bộ.

- **Hạn mức Số từ theo Loại phiên (`Item Limit Invariant`)**:
  - *Phiên học mới (`NewLearningSession`)*: Tối thiểu $5$ từ, Tối đa $20$ từ (Mặc định: $10$ từ).
  - *Phiên ôn tập (`ReviewSession`)*: Tối thiểu $5$ từ, Tối đa $30$ từ (Mặc định: $20$ từ).
- **Thuật toán Phân đoạn Bộ từ Lớn (`Batch Splitting Engine`)**: Nếu một bộ từ vựng chứa 50 từ, hệ thống tự động chia bộ từ thành các chặng (Sub-batch) từ 10-15 từ để người học hoàn thành từng phiên nhỏ.

## 2. Quy tắc Xử lý Danh sách Thiếu từ (Insufficient Items Handling)

| Tình huống | Loại phiên | Hành vi xử lý của Hệ thống |
|---|---|---|
| Bộ từ chỉ có $< 5$ từ | Học mới | Từ chối tạo phiên với lỗi `NOT_ENOUGH_WORDS_IN_SET`. Yêu cầu bộ từ $\ge 5$ từ. |
| Hàng đợi ôn chỉ có $1 \to 4$ từ | Ôn tập | Cho phép tạo "Phiên ôn tập nhanh" (Quick Review Session) với số từ thực tế $1 \to 4$. |
| Bộ từ có 35 từ | Học mới | Chia thành 3 phiên học nhỏ: Phiên 1 (12 từ), Phiên 2 (12 từ), Phiên 3 (11 từ). |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IL-G01`: 100% phiên học mới không chứa quá $20$ từ trong một phiên.
- `IL-G02`: Bộ từ có ít hơn $5$ từ bị chặn không cho tạo phiên học mới.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IL06-01` | Tạo phiên học mới từ bộ từ 50 từ | Hệ thống khởi tạo phiên học với đúng 10 từ đầu tiên. |
| `IL06-02` | Thử tạo phiên học từ bộ từ có 3 từ | System reject với lỗi `NOT_ENOUGH_WORDS_IN_SET`. |
| `IL06-03` | Kiểm thử hoàn tất luồng M03-SESSION-ITEM-LIMIT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-IL-F01` | Tham số `DefaultNewSessionSize` cần đưa vào cấu hình M11 | Cho phép admin điều chỉnh quy mô phiên | M03-T007 |

## 5. Tự kiểm M03-T006
- Đã xác định giới hạn số từ trong phiên M03-T006.
- Ghi nhận 2 Regression Gates (`IL-G01`–`IL-G02`) và 3 Test Cases (`IL06-01`–`IL06-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xác định giới hạn số từ trong phiên M03-T006 | WSA-7K2 |
