# Chuẩn hóa từ điển ghi nhớ M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-MEMORY-DICT-1.0` |
| Task | M04-T001 |
| Đầu vào | M02-VOCAB-DICT-1.0 (M02-T001), M03-SESSION-DICT-1.0 (M03-T001) |
| Phạm vi | Thuật ngữ ghi nhớ, thuật toán Lặp lại ngắt quãng (Spaced Repetition System - SRS), khoảng thời gian ôn (`Interval`), hệ số dễ nhớ (`EaseFactor`), điểm thành thạo (`MasteryScore`) |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa toàn bộ thuật ngữ, tham số toán học và trạng thái ghi nhớ trong Module M04 (Ôn tập và tiến độ).

- **Phân biệt Hoàn thành Phiên và Thành thạo SRS (`Completion vs Mastery Invariant`)**: Hoàn thành bài kiểm tra M03 chỉ cung cấp 1 điểm dữ liệu gợi nhớ. Trạng thái Thành thạo dài hạn (`Mastered`) CHỈ đạt được khi khoảng thời gian ôn $Interval \ge 21$ ngày và điểm thành thạo $MasteryScore \ge 85\%$.
- **Đơn vị Tiến độ theo Người dùng–Nghĩa (`User-Sense Unit Invariant`)**: Tiến độ ghi nhớ gắn cố định với bộ đôi `(UserId, VocabularySenseId)`. CẤM đếm tiến độ chỉ theo mặt chữ (`WordCanonical`) vì một mặt chữ có thể chứa nhiều nét nghĩa độc lập.

## 2. Bảng Từ điển Thuật ngữ Ghi nhớ M04 (SRS Memory Lexicon)

| Thuật ngữ | Tên tiếng Việt | Mô tả & Công thức trong WordSoul | Phạm vi giá trị |
|---|---|---|---|
| `UserSenseProgress` | Hồ sơ nhớ người dùng–nghĩa | Thực thể lưu trữ trạng thái ghi nhớ của 1 nét nghĩa từ vựng cho 1 người học. | Entity |
| `Interval` | Khoảng ngày ôn | Số ngày chờ đến lần ôn tập tiếp theo ($I_{next} = I_{current} \times EF$). | $1 \le Interval \le 365$ ngày |
| `EaseFactor` ($EF$) | Hệ số dễ nhớ | Chỉ số điều chỉnh độ khó của nét nghĩa đối với người học cụ thể. | $1.30 \le EF \le 2.50$ (Mặc định 2.50) |
| `RepetitionCount` ($n$) | Số lần ôn thành công | Số lần người học trả lời đúng liên tiếp trong các phiên ôn tập. | Số nguyên $\ge 0$ |
| `MasteryScore` | Điểm thành thạo | Tỷ lệ ước tính khả năng ghi nhớ nét nghĩa ($0\% \to 100\%$). | $0.0 \to 100.0\%$ |
| `DueDate` | Ngày đến hạn ôn | Thời điểm UTC mục từ xuất hiện trong Hàng đợi ôn tập (`DueDate = LastReviewedAt + Interval`). | DateTime UTC |
| `OverdueDays` | Số ngày quá hạn | Số ngày vượt quá `DueDate` mà người học chưa thực hiện bài ôn. | Số nguyên $\ge 0$ |

## 3. Máy Trạng thái Tiến độ Ghi nhớ (Memory State Machine)

```text
[NEW] ---> [LEARNING] (Interval: 1-6 ngày)
                │
                ▼ (Repetition >= 3, Interval >= 7)
           [REVIEWING] (Interval: 7-20 ngày)
                │
                ▼ (Interval >= 21 ngày & MasteryScore >= 85%)
           [MASTERED] (Thành thạo dài hạn)
                │
                ▼ (Trả lời sai trong bài ôn -> Forget)
           [RELEARNING] (Interval reset về 1 ngày)
```

## 4. Regression Gates và Test Cases

### 4.1. Regression Gates
- `MD-G01`: 100% hồ sơ nhớ gắn cố định với `VocabularySenseId`, không trộn lẫn nét nghĩa khác của cùng 1 từ.
- `MD-G02`: $EaseFactor$ luôn kẹp trong khoảng $[1.30, 2.50]$, không bị tràn âm hoặc quá cao.
- `MD-G03`: Trạng thái `MASTERED` chỉ đạt được khi $Interval \ge 21$ ngày.

### 4.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MD01-01` | Người học hoàn thành phiên học từ vựng mới | Tạo `UserSenseProgress` trạng thái `LEARNING`, $Interval = 1$ ngày. |
| `MD01-02` | Ôn tập đúng 4 lần liên tiếp | $Interval$ tăng dần $1 \to 6 \to 15 \to 35$ ngày, trạng thái đạt `MASTERED`. |
| `MD01-03` | Trả lời sai khi ôn tập từ đang `MASTERED` | Trạng thái chuyển về `RELEARNING`, $Interval$ reset về 1 ngày. |
| `MD01-04` | Kiểm thử hoàn tất luồng M04-MEMORY-DICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 5. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-MD-F01` | Cần tạo bảng `UserSenseProgresses` trong CSDL | Chưa có schema lưu hồ sơ ghi nhớ SRS | M04-T003 |

## 6. Tự kiểm M04-T001
- Đã hoàn thành từ điển ghi nhớ M04, chốt các tham số SRS toán học và ranh giới nét nghĩa.
- Xác lập 3 Regression Gates (`MD-G01`–`MD-G03`) và 4 Test Cases (`MD01-01`–`MD01-04`).

## 7. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa từ điển ghi nhớ M04-T001 | WSA-7K2 |
