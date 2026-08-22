# Chuẩn hóa bàn giao danh sách cho M03 M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-QUEUE-HANDOFF-1.0` |
| Task | M04-T023 |
| Đầu vào | M03-REVIEW-SESSION-CONDITIONS-1.0 (M03-T005), M04-REVIEW-PRIORITY-SCORE-1.0 (M04-T021), M04-BACKLOG-MANAGEMENT-1.0 (M04-T022) |
| Phạm vi | Hợp đồng API và DTO bàn giao danh sách từ vựng đến hạn từ M04 sang M03 (`ReviewQueueHandoffDto`), cơ chế đặt chỗ tạm thời (`Reservation Lock`) chống trùng phiên mở |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa quy trình bàn giao danh sách từ vựng ôn tập từ M04 sang M03 (`Review Queue Handoff Contract`) để khởi tạo phiên ôn.

- **Đóng băng Danh sách Bàn giao Duy nhất (`Frozen Queue Snapshot Invariant`)**:
  - DTO bàn giao `ReviewQueueHandoffDto` chứa một `HandoffToken` và danh sách tối đa 30 mục từ được khóa tạm thời bằng Redis Reservation Lock trong 15 phút.
  - Gọi lại API với cùng `HandoffToken` BẮT BUỘC trả về đúng tập từ vựng đó mà không tự ý đổi danh sách.
- **Tự động Loại bỏ Mục từ Thu hồi (`Quarantine Exclusion Invariant`)**: 100% mục từ nằm trong danh sách thu hồi (`RECALLED`) bị loại bỏ khỏi danh sách bàn giao trước khi gửi sang M03.

## 2. Cấu trúc DTO Bàn giao Danh sách Ôn (ReviewQueueHandoff Envelope)

```csharp
public class ReviewQueueHandoffDto
{
    public Guid HandoffToken { get; set; }
    public Guid UserId { get; set; }
    public DateTime HandoffCreatedAtUtc { get; set; }
    public DateTime ReservationExpiresAtUtc { get; set; }
    
    public bool IsBacklogRecovery { get; set; }
    public int RemainingBacklogCount { get; set; }
    
    public List<ReviewItemHandoffDto> Items { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QH-G01`: 100% request lấy danh sách ôn M04 trả về `HandoffToken` có hiệu lực đặt chỗ trong 15 phút.
- `QH-G02`: Mục từ bị thu hồi trong M02 bị chặn 100% không xuất hiện trong `ReviewQueueHandoffDto`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QH23-01` | M03 gọi M04 lấy danh sách ôn tập cho người dùng A | Trả về `ReviewQueueHandoffDto` chứa 20 từ, `ReservationExpiresAtUtc = Now + 15m`. |
| `QH23-02` | M03 gọi lại lần 2 sau 1 phút với cùng `HandoffToken` | Trả về chính xác tập 20 từ đã đặt chỗ trước đó. |
| `QH23-03` | Kiểm thử hoàn tất luồng M04-REVIEW-QUEUE-HANDOFF-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-QH-F01` | Đưa Redis Key `lock_reservation_{userId}_{token}` để quản lý khóa giữ chỗ | Ngăn ngừa hai thiết bị tạo 2 phiên ôn trùng từ vựng | M03-T005 |

## 5. Tự kiểm M04-T023
- Đã hoàn thành đặc tả `M04-REVIEW-QUEUE-HANDOFF-1.0`.
- Chốt HandoffToken và cơ chế Redis Reservation Lock 15 phút.
- Ghi nhận 2 Regression Gates (`QH-G01`–`QH-G02`) và 3 Test Cases (`QH23-01`–`QH23-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa bàn giao danh sách cho M03 M04-T023 | WSA-7K2 |
