# Xác định bằng chứng được phép chấm M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-VALID-EVIDENCE-RULES-1.0` |
| Task | M04-T010 |
| Đầu vào | M03-INITIAL-RECALL-CAPTURE-1.0 (M03-T034), M04-MEMORY-DICT-1.0 (M04-T001) |
| Phạm vi | Quy định loại bằng chứng được phép làm thay đổi khoảng cách lặp lại SRS trong M04 và loại trừ các bằng chứng không hợp lệ |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định các điều kiện để một bằng chứng phản hồi (Evidence) được chấp nhận cập nhật thuật toán SRS.

- **Bằng chứng Gợi nhớ Đủ điều kiện (`Eligible Recall Evidence Invariant`)**: CHỈ chấp nhận bằng chứng trả lời ĐẦU TIÊN (`IsCorrectFirstTry`) trong các phiên học chính thức (`NewLearningSession`, `ReviewSession`).
- **Loại trừ Thao tác Xem và Luyện tập Tự do (`Excluded Interactions Invariant`)**:
  - Thao tác lật xem thẻ Flashcard (Card Display) KHÔNG ĐƯỢC tính là bằng chứng gợi nhớ.
  - Lần làm bài tập trong chế độ "Luyện tự do không tính điểm" (Practice Sandbox) KHÔNG ĐƯỢC làm thay đổi lịch ôn đến hạn.

## 2. Bảng Phân loại Bằng chứng và Quyền Tác động SRS (Evidence Qualification Matrix)

| Nguồn Thao tác | Loại Phiên M03 | Lần thử | Chấp nhận làm Bằng chứng SRS M04 | Tác động SRS |
|---|---|---|---|---|
| Flashcard View | Học mới | N/A | **KHÔNG** | Không tác động |
| Multiple Choice Quiz | Học mới / Ôn tập | Lần 1 | **CÓ** | Cập nhật $Interval$ & $EF$ |
| Multiple Choice Retry | Học mới / Ôn tập | Lần 2, 3 | **KHÔNG** | Bỏ qua |
| Free Practice Sandbox | Sandbox | Mọi lần | **KHÔNG** | Bỏ qua |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `VE-G01`: 100% lượt lật xem Flashcard không làm thay đổi `DueDateUtc` trong DB M04.
- `VE-G02`: Chỉ bằng chứng Lần 1 (`IsCorrectFirstTry`) được đưa vào công thức tính $Interval$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VE10-01` | Người dùng lật xem 50 Flashcard trong thư viện | M04 giữ nguyên toàn bộ hồ sơ nhớ ở trạng thái cũ. |
| `VE10-02` | Người dùng làm bài tập Sandbox tự do | M04 không tạo log cập nhật lịch ôn. |
| `VE10-03` | Kiểm thử hoàn tất luồng M04-VALID-EVIDENCE-RULES-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-VE-F01` | Thêm thuộc tính `IsEligibleForSrs` trong DTO nhận bằng chứng | Lọc bằng chứng ngay tại bước validation | M04-T011 |

## 5. Tự kiểm M04-T010
- Đã đặc tả xác định bằng chứng được phép chấm M04-T010.
- Ghi nhận 2 Regression Gates (`VE-G01`–`VE-G02`) và 3 Test Cases (`VE10-01`–`VE10-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xác định bằng chứng được phép chấm M04-T010 | WSA-7K2 |
