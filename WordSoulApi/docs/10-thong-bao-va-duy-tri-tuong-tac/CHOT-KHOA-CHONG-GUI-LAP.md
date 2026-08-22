# Chốt khóa chống gửi lặp M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-DUPLICATE-DISPATCH-LOCK-1.0` |
| Task | M10-T021 |
| Đầu vào | M10-EXACTLY-ONCE-SIGNAL-CONSUMPTION-1.0 (M10-T005), M10-CHANNEL-SELECTION-MATRIX-1.0 (M10-T020) |
| Phạm vi | Cơ chế khóa chống gửi lặp Push Notification và Email (`Duplicate Dispatch Lock Engine`), dựa trên Redis Redlock `DispatchLockKey` để ngăn ngừa gửi 2 thông báo cùng nội dung |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế khóa chống gửi lặp thông báo (`Duplicate Dispatch Lock`) trong M10.

- **Cơ chế Khóa Chống Gửi Lặp Redis Redlock (`Dispatch Lock Invariant`)**:
  - Trước khi gửi Push Notification hoặc Email sang nhà cung cấp bên thứ ba (FCM, SendGrid), Worker BẮT BUỘC giữ được khóa Redis Redlock:
    `DispatchLockKey = lock_dispatch_{UserId}_{Category}_{TriggerEventId}` với thời gian giữ khóa `TTL = 10` phút.
  - Nếu không giữ được khóa (do worker khác đang gửi cùng thông báo), Worker hiện tại BẮT BUỘC bỏ qua request.
- **Tính Duy nhất của Bản ghi Hộp thư (`Single Inbox Record Rule`)**: Việc gửi lại hoặc retry KHÔNG ĐƯỢC PHÉP chèn thêm bản ghi thông báo trùng lặp vào bảng `NotificationInbox`.

## 2. Luồng Kiểm tra và Giữ Khóa Chống Gửi Lặp (Dispatch Lock Workflow)

```mermaid
graph TD
    Worker[Notification Dispatch Worker] --> Redlock{Acquire Lock lock_dispatch_{userId}_{category}_{eventId}?}
    Redlock -->|Failed - Lock Held| Skip[Skip Dispatch - Duplicate Detected]
    Redlock -->|Success| CheckSent{Already Sent in NotificationLogs?}
    CheckSent -->|Yes| ReleaseSkip[Release Lock & Skip Dispatch]
    CheckSent -->|No| SendFCM[Send FCM Push Notification]
    SendFCM --> RecordLog[Write NotificationLog]
    RecordLog --> ReleaseLock[Release Lock]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `DL-G01`: 100% request dispatch thông báo có cùng `TriggerEventId` trong 10 phút chỉ gửi duy nhất 1 Push Notification tới thiết bị.
- `DL-G02`: Bảng `NotificationInbox` không tồn tại 2 row có cùng bộ `(TargetUserId, TriggerEventId)`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DL21-01` | 2 Worker đồng thời xử lý sự kiện `REVIEW_REMINDER_EVENT` cho Learner A | Đảm bảo chỉ 1 Worker giữ được Redlock và gửi 1 Push Notification, Worker 2 tự động bỏ qua. |
| `DL21-02` | Worker bị sập mạng giữa chừng sau khi đã giữ khóa 1 phút | Khóa Redlock tự động giải phóng sau TTL 10 phút, worker retry kiểm tra `NotificationLogs` và xử lý an toàn. |
| `DL21-03` | Kiểm thử hoàn tất luồng M10-DUPLICATE-DISPATCH-LOCK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-DL-F01` | Đưa Redis Key `lock_dispatch_{userId}_{category}_{eventId}` vào Dispatch Engine | Bảo đảm an toàn tuyệt đối chống spam push | M10-T020 |

## 5. Tự kiểm M10-T021
- Đã hoàn thành đặc tả `M10-DUPLICATE-DISPATCH-LOCK-1.0`.
- Chốt cơ chế Redis Redlock `DispatchLockKey` TTL 10 phút và kiểm tra bản ghi hộp thư duy nhất.
- Ghi nhận 2 Regression Gates (`DL-G01`–`DL-G02`) và 3 Test Cases (`DL21-01`–`DL21-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chốt khóa chống gửi lặp M10-T021 | WSA-7K2 |
