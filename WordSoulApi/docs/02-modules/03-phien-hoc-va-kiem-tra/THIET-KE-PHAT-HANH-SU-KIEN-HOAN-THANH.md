# Thiết kế phát hành sự kiện hoàn thành M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-COMPLETED-EVENT-1.0` |
| Task | M03-T040 |
| Đầu vào | M03-SINGLE-FINALIZATION-GUARANTEE-1.0 (M03-T038), M04, M06, M07, M11 |
| Phạm vi | Cấu trúc payload sự kiện `LearningSessionCompletedEvent`, cơ chế Outbox Pattern phát sự kiện tích hợp với M04 (SRS), M06 (Kinh tế) và M07 (Nhiệm vụ) |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cấu trúc và quy trình phát hành sự kiện hoàn thành phiên học (`LearningSessionCompletedEvent`) từ M03 sang các module tiếp nhận.

- **Mô hình Outbox Đảm bảo Phát hành (`Outbox Pattern Invariant`)**: Sự kiện BẮT BUỘC được ghi vào bảng `OutboxEvents` trong cùng một Database Transaction với lệnh chốt phiên. Worker Outbox sẽ đảm nhận việc gửi message sang Message Bus (RabbitMQ/Redis PubSub) với đảm bảo At-Least-Once Delivery.
- **Tính Bất biến của Event Payload (`Immutable Payload Invariant`)**: Payload của `LearningSessionCompletedEvent` chứa đầy đủ danh sách `InitialRecallResults`, số Gold/Exp xứng đáng và Metadata phiên học. CẤM chỉnh sửa thông tin sự kiện sau khi đã phát hành.

## 2. Event Payload Schema (LearningSessionCompletedEvent)

```json
{
  "eventId": "evt_9b1deb4d-3b7d-4bad-9bdd-2b0d7b3d0040",
  "eventType": "LearningSessionCompleted",
  "occurredAtUtc": "2026-08-21T08:55:00Z",
  "sessionId": "ses_12345678-1234-1234-1234-123456789012",
  "userId": "usr_87654321-4321-4321-4321-210987654321",
  "sessionType": "NEW_LEARNING",
  "vocabularySetId": "set_11111111-2222-3333-4444-555555555555",
  "summary": {
    "totalWords": 10,
    "correctFirstTryCount": 8,
    "accuracyPercentage": 80.0,
    "durationSeconds": 450,
    "goldReward": 50,
    "expReward": 100
  },
  "initialRecallResults": [
    {
      "vocabularySenseId": "sen_aaa11111-2222-3333-4444-555555555555",
      "isCorrectFirstTry": true,
      "responseDurationMs": 1200,
      "usedHint": false
    }
  ]
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SE-G01`: 100% sự kiện hoàn thành phiên được lưu thành công vào `OutboxEvents` trước khi API trả phản hồi cho Client.
- `SE-G02`: Nhận trùng `eventId` tại Consumer (M04, M06, M07) không làm cập nhật lặp tiến độ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SE40-01` | Chốt phiên học 10 từ thành công | Phát `LearningSessionCompletedEvent` chứa đủ 10 kết quả gợi nhớ đầu. |
| `SE40-02` | Mô phỏng đứt kết nối Message Bus khi chốt phiên | Event được lưu an toàn trong Outbox Table, Worker retry gửi lại khi Bus khôi phục. |
| `SE40-03` | Kiểm thử hoàn tất luồng M03-SESSION-COMPLETED-EVENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SE-F01` | Đăng ký Consumer `LearningSessionCompletedConsumer` tại M04, M06, M07 | Đảm bảo xử lý sự kiện bất đồng bộ | M04-T006 |

## 5. Tự kiểm M03-T040
- Đã đặc tả thiết kế phát hành sự kiện hoàn thành M03-T040.
- Ghi nhận 2 Regression Gates (`SE-G01`–`SE-G02`) và 3 Test Cases (`SE40-01`–`SE40-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế phát hành sự kiện hoàn thành M03-T040 | WSA-7K2 |
