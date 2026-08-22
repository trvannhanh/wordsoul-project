# Thiết kế gom và thay thế thông báo M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-COLLAPSE-REPLACE-1.0` |
| Task | M10-T023 |
| Đầu vào | M10-DUPLICATE-DISPATCH-LOCK-1.0 (M10-T021), M10-NOTIFICATION-RATE-LIMITING-1.0 (M10-T022) |
| Phạm vi | Cơ chế gom nhóm (`Collapse Key Engine`) và thay thế thông báo Push Notification hiển thị trên khay hệ thống (System Notification Tray) của Android/iOS |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy tắc gom nhóm và thay thế thông báo Push (`Notification Collapse & Replace Engine`) trong M10.

- **Dùng Collapse Key Chuẩn hóa trên Khay Hệ thống (`Standardized Collapse Key Invariant`)**:
  - 100% thông báo Push Notification thuộc cùng loại (ví dụ: nhắc ôn tập `REVIEW_REMINDER`) BẮT BUỘC gắn mã `CollapseKey = collapse_review_{userId}`.
  - Khi thông báo mới cùng `CollapseKey` tới thiết bị, nó BẮT BUỘC ghi đè lên thông báo cũ chưa đọc trên khay thông báo Android/iOS thay vì xếp đè dồn dập.
- **Không Đổi Trạng thái Đã đọc của Bản ghi Hộp thư (`Inbox Record Independence Rule`)**: Thao tác ghi đè Push trên khay hệ thống KHÔNG ĐƯỢC PHÉP làm thay đổi trạng thái của các bản ghi cũ đã lưu trong bảng `NotificationInbox`.

## 2. Quy trình Gom và Ghi đè Thông báo trên Khay Hệ thống (Collapse Engine Flow)

```mermaid
graph TD
    Req[New Notification Request] --> AssignKey[Assign CollapseKey = collapse_{category}_{userId}]
    AssignKey --> BuildFCMPayload[Build FCM Message Payload with collapse_key]
    BuildFCMPayload --> SendFCM[Send FCM Push Payload]
    SendFCM --> MobileOS[Mobile OS Replaces Old Notification with Same CollapseKey]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `CR-G01`: 100% payload FCM gửi đi cho loại nhắc ôn tập chứa thuộc tính `collapse_key` hợp lệ.
- `CR-G02`: Việc ghi đè Push trên khay thiết bị giữ nguyên $100\%$ tính toàn vẹn các dòng bản ghi trong DB `NotificationInbox`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CR23-01` | Server gửi 3 Push nhắc ôn tập liên tiếp trong 2 giờ cho Learner A | Khay thông báo điện thoại chỉ hiển thị 1 thông báo duy nhất (bản mới nhất). DB Inbox lưu đủ 3 dòng. |
| `CR23-02` | Push nhắc nhiệm vụ ngày tới với `CollapseKey = collapse_quest_user123` | Ghi đè thông báo nhiệm vụ cũ, không ảnh hưởng đến thông báo nhắc ôn tập. |
| `CR23-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-COLLAPSE-REPLACE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-CR-F01` | Cấu hình trường `CollapseKey` trong FCM Payload Builder | Đảm bảo trải nghiệm khay thông báo gọn gàng | M10-T020 |

## 5. Tự kiểm M10-T023
- Đã hoàn thành đặc tả `M10-NOTIFICATION-COLLAPSE-REPLACE-1.0`.
- Chốt mã `CollapseKey` theo danh mục và bảo toàn lịch sử bản ghi DB Inbox.
- Ghi nhận 2 Regression Gates (`CR-G01`–`CR-G02`) và 3 Test Cases (`CR23-01`–`CR23-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế gom và thay thế thông báo M10-T023 | WSA-7K2 |
