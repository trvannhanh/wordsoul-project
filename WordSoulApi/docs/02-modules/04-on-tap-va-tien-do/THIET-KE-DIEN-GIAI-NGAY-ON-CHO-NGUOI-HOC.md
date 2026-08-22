# Thiết kế diễn giải ngày ôn cho người học M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-SCHEDULE-EXPLANATION-1.0` |
| Task | M04-T026 |
| Đầu vào | M04-MEMORY-STATES-DEFINITIONS-1.0 (M04-T015), M04-SCHEDULE-TIMEZONE-1.0 (M04-T017), M04-REVIEW-LOG-SCHEMA-1.0 (M04-T024) |
| Phạm vi | Chuẩn hóa thông điệp diễn giải lý do đến hạn ôn tập (`Schedule Explanation Engine`) hiển thị minh bạch cho người học trên UI |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa quy tắc phát sinh văn bản diễn giải lý do ngày ôn (`Schedule Explanation Messages`) trong M04.

- **Văn bản Diễn giải Dễ hiểu và Minh bạch (`Transparent Schedule Explanation Invariant`)**:
  - Mỗi mục từ trong danh sách ôn BẮT BUỘC đi kèm một chuỗi diễn giải lý do `ExplanationText` và mã lý do `ReasonCode`.
  - CẤM hiển thị các thuật ngữ toán học hay công thức SM-2 phức tạp (như "EaseFactor = 1.30") trên giao diện người dùng. Thay bằng thuật ngữ thân thiện (như "Từ vựng khó, cần ôn lại thường xuyên").
- **Tôn trọng Múi giờ và Ngày Địa phương (`Local Timezone Respect Invariant`)**: Văn bản thời gian (ví dụ "Hôm nay", "Ngày mai") BẮT BUỘC quy đổi theo múi giờ cá nhân người dùng `UserTimeZoneId`.

## 2. Bảng Ma trận Diễn giải Lý do Lịch Ôn (Schedule Reason Matrix)

| Mã Lý do `ReasonCode` | Điều kiện Kích hoạt | Chuỗi Diễn giải Hiển thị UI |
|---|---|---|
| `DUE_NORMAL` | Trạng thái `REVIEWING`, $DueDateUtc \le NowUtc$ | "Đã đến lịch ôn tập định kỳ" |
| `RELEARNING_FORGOT` | Trạng thái `RELEARNING` (Vừa quên) | "Từ vựng bạn đã quên, cần ôn lại ngay" |
| `HARD_ITEM` | $EaseFactor \le 1.50$ | "Từ vựng bạn gặp khó khăn, khoảng ôn ngắn hơn" |
| `BACKLOG_OVERDUE` | $OverdueDays > 7$ | "Từ vựng quá hạn ôn lâu ngày, hãy khôi phục phong độ" |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SE-G01`: 100% mục từ trong API trả về danh sách ôn đều chứa `ReasonCode` và `ExplanationText` hợp lệ.
- `SE-G02`: Không chứa các từ khóa kỹ thuật thô (`EaseFactor`, `RowVersion`, `Guid`) trong văn bản UI.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SE26-01` | Từ vựng vừa bị trả lời sai ở phiên trước (`RELEARNING`) | API trả về `ReasonCode = RELEARNING_FORGOT`, Text: "Từ vựng bạn đã quên, cần ôn lại ngay". |
| `SE26-02` | Người dùng ở múi giờ UTC+7 xem lịch từ vựng đến hạn ngày mai | UI hiển thị chính xác "Đến hạn vào Ngày mai, 23 tháng 8". |
| `SE26-03` | Kiểm thử hoàn tất luồng M04-SCHEDULE-EXPLANATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-SE-F01` | Nhúng `ScheduleExplanationDto` vào DTO trả về cho M03/M04 API | Phục vụ thẻ thông tin lý do ôn trên mobile app | M04-T023 |

## 5. Tự kiểm M04-T026
- Đã hoàn thành đặc tả `M04-SCHEDULE-EXPLANATION-1.0`.
- Chốt ma trận 4 mã lý do diễn giải lịch ôn thân thiện với người dùng.
- Ghi nhận 2 Regression Gates (`SE-G01`–`SE-G02`) và 3 Test Cases (`SE26-01`–`SE26-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế diễn giải ngày ôn cho người học M04-T026 | WSA-7K2 |
