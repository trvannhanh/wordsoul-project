# Đặc tả tín hiệu cho M10 M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-SIGNAL-M10-CONTRACT-1.0` |
| Task | M07-T041 |
| Đầu vào | M07-QUEST-CYCLE-RESET-SPEC-1.0 (M07-T025), M07-QUEST-COMPLETION-STATE-TRANSITION-1.0 (M07-T026), M10-QUEST-EVENT-REMINDER-SPEC-1.0 (M10-T028) |
| Phạm vi | Danh mục sự kiện tích hợp M07 phát sang M10 (`M07 Integration Event Outbox`), bao gồm `QuestAssignedEvent`, `QuestCompletedEvent`, `UnclaimedQuestExpiringEvent` |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa hợp đồng phát tín hiệu sự kiện từ M07 sang M10 (`Quest Signal Outbox Contract`).

- **M07 Không Tự Quyết Kênh Gửi (`No Channel Decision Invariant`)**:
  - M07 CHỈ PHÁT SỰ KIỆN NGUYÊN BẢN (`QuestCompletedIntegrationEvent`). M07 CẤM can thiệp vào quyết định gửi qua Push Notification, Email hay In-App Inbox (quyền thuộc M10).
- **Mã Tín hiệu Chống Lặp Idempotency Key (`Signal Idempotency Invariant`)**: 100% sự kiện phát ra BẮT BUỘC chứa `EventId = evt_quest_{userQuestId}_{action}` duy nhất.

## 2. Danh mục Sự kiện Tích hợp M07 phát sang M10 (Outbox Event Catalog)

| Mã Sự kiện | Thời điểm Phát | Nội dung Payload Mới | Tác động bên M10 |
|---|---|---|---|
| `QuestCompletedIntegrationEvent` | Chuyển `COMPLETED_UNCLAIMED` | `userQuestId`, `userId`, `title` | Hủy nhắc nhở cũ & phát Push chúc mừng |
| `UnclaimedQuestExpiringEvent` | Trước khi hết hạn 2 tiếng | `userQuestId`, `userId`, `title` | Gửi Push nhắc nhận thưởng gấp |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QS-G01`: 100% sự kiện phát từ M07 đi kèm mã `EventId` duy nhất trong Outbox Table.
- `QS-G02`: Payload sự kiện M07 không chứa bất kỳ chỉ định kênh gửi (Push/Email) nào.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QS41-01` | Learner hoàn thành 100% nhiệm vụ ngày | M07 lưu 1 bản ghi `QuestCompletedIntegrationEvent` vào DB Outbox Table. |
| `QS41-02` | Outbox Publisher phát sự kiện sang Message Broker | M10 tiêu thụ sự kiện thành công, tiến hành kẹp trần rate limit và phát Push. |
| `QS41-03` | Kiểm thử hoàn tất luồng M07-QUEST-SIGNAL-M10-CONTRACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QS-F01` | Sử dụng Outbox Pattern trong M07 DbContext | Bảo đảm gửi sự kiện reliable $100\%$ | M07-T026 |

## 5. Tự kiểm M07-T041
- Đã hoàn thành đặc tả `M07-QUEST-SIGNAL-M10-CONTRACT-1.0`.
- Chốt danh mục sự kiện Outbox M07 và nguyên tắc M07 không quyết định kênh gửi.
- Ghi nhận 2 Regression Gates (`QS-G01`–`QS-G02`) và 3 Test Cases (`QS41-01`–`QS41-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả tín hiệu cho M10 M07-T041 | WSA-7K2 |
