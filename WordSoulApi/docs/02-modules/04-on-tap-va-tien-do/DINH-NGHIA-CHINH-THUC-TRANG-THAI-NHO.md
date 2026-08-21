# Định nghĩa chính thức trạng thái nhớ M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-MEMORY-STATES-DEFINITIONS-1.0` |
| Task | M04-T015 |
| Đầu vào | M04-MEMORY-DICT-1.0 (M04-T001), M04-USER-SENSE-UNIT-1.0 (M04-T002) |
| Phạm vi | Máy trạng thái ghi nhớ chính thức (`MemoryState`), điều kiện chuyển dịch giữa 5 trạng thái: `NEW`, `LEARNING`, `REVIEWING`, `MASTERED`, `RELEARNING` |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này định nghĩa chính thức 5 trạng thái ghi nhớ của mục từ vựng trong M04 và điều kiện chuyển trạng thái.

- **5 Trạng thái Chuẩn hóa (`5 Standardized Memory States Invariant`)**:
  - `NEW`: Chưa bắt đầu học.
  - `LEARNING`: Đang học chặng đầu ($1 \le Interval \le 6$ ngày).
  - `REVIEWING`: Đang ôn tập củng cố ($7 \le Interval \le 20$ ngày).
  - `MASTERED`: Thành thạo dài hạn ($Interval \ge 21$ ngày & $MasteryScore \ge 85\%$).
  - `RELEARNING`: Quên từ đã thuộc, đang học lại ($Interval$ bị kẹp về 1 ngày).
- **Ràng buộc Chuyển trạng thái khi Quên (`Forget Demotion Guard`)**: Khi một mục từ ở trạng thái `MASTERED` bị trả lời sai trong phiên ôn, trạng thái BẮT BUỘC rớt về `RELEARNING`.

## 2. Máy Trạng thái Ghi nhớ (Memory State Transition Matrix)

| Trạng thái Gốc | Kết quả Gợi nhớ | Trạng thái Đích | Khoảng ngày ôn ($Interval$) mới |
|---|---|---|---|
| `NEW` | Hoàn thành phiên học mới | `LEARNING` | 1 ngày |
| `LEARNING` | Trả lời Đúng ($Repetition \ge 3$) | `REVIEWING` | $I_{new} = I_{old} \times EF$ (7-15 ngày) |
| `REVIEWING` | Trả lời Đúng ($Interval \ge 21d$) | `MASTERED` | $I_{new} = I_{old} \times EF$ ($\ge 21$ ngày) |
| `MASTERED` | Trả lời Sai (Forget) | `RELEARNING` | **Reset về 1 ngày** |
| `RELEARNING` | Trả lời Đúng lại | `REVIEWING` | 7 ngày |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `MS-G01`: 100% mục từ trả lời sai khi đang `MASTERED` chuyển ngay sang `RELEARNING` với $Interval = 1$ ngày.
- `MS-G02`: Trạng thái `MASTERED` chỉ đạt được khi $Interval \ge 21$ ngày.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MS15-01` | Từ đang `MASTERED` bị người học trả lời sai trong bài ôn | Trạng thái chuyển `RELEARNING`, $Interval = 1$ ngày. |
| `MS15-02` | Từ đang `LEARNING` ôn đúng 3 lần liên tiếp ($Interval = 8$ ngày) | Trạng thái chuyển `REVIEWING`. |
| `MS15-03` | Kiểm thử hoàn tất luồng M04-MEMORY-STATES-DEFINITIONS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-MS-F01` | Cần thuộc tính `State` dạng Enum `MemoryState` trong CSDL | Đảm bảo ràng buộc trạng thái nhớ trong DB | M04-T016 |

## 5. Tự kiểm M04-T015
- Đã đặc tả định nghĩa chính thức trạng thái nhớ M04-T015.
- Ghi nhận 2 Regression Gates (`MS-G01`–`MS-G02`) and 3 Test Cases (`MS15-01`–`MS15-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả định nghĩa chính thức trạng thái nhớ M04-T015 | WSA-7K2 |
