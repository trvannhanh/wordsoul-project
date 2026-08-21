# Kiểm soát nội dung màn hình khóa M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-LOCKSCREEN-PRIVACY-CONTROL-1.0` |
| Task | M10-T014 |
| Đầu vào | M10-NOTIFICATION-TEMPLATE-SPEC-1.0 (M10-T011), M01-PRIVACY-1.0 (M01-T029) |
| Phạm vi | Quy định bảo mật quyền riêng tư cho thông báo hiển thị ngoài Màn hình khóa (Lockscreen Privacy Control: `PRIVATE` vs `PUBLIC`) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định việc che giấu thông tin nhạy cảm cá nhân trên màn hình khóa điện thoại.

- **Mặc định Che Thông tin Nhạy cảm (`Default Lockscreen Privacy Invariant`)**: Các thông báo chứa thông tin tiến độ riêng tư hoặc tài sản cá nhân BẮT BUỘC đặt `LockscreenVisibility = PRIVATE`. Khi hiển thị ngoài màn hình khóa (khi thiết bị chưa mở khóa), ứng dụng iOS/Android BẮT BUỘC che nội dung bằng thông điệp chung: *"Bạn có 1 thông báo mới từ WordSoul"*.
- **Cấu hình Cho phép Xem chi tiết (`User Privacy Toggle Invariant`)**: Người học có quyền bật tùy chọn "Hiển thị chi tiết trên màn hình khóa" trong cài đặt ứng dụng.

## 2. Dynamic Lockscreen Payload Control

```csharp
public PushNotificationPayload BuildLockscreenPayload(NotificationInbox item, bool isPrivateModeOnLockscreen)
{
    if (isPrivateModeOnLockscreen && item.CategoryCode != "SECURITY")
    {
        return new PushNotificationPayload {
            Title = "WordSoul",
            Body = "Bạn có 1 thông báo mới.",
            Visibility = "PRIVATE"
        };
    }
    
    return new PushNotificationPayload {
        Title = item.Title,
        Body = item.Body,
        Visibility = "PUBLIC"
    };
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LP-G01`: 100% Push notification gửi ra thiết bị khi chưa bật toggle xem chi tiết có `Visibility == PRIVATE`.
- `LP-G02`: Tín hiệu `SECURITY` luôn hiển thị tiêu đề và nội dung đầy đủ ngoài màn hình khóa để bảo vệ tài khoản khẩn cấp.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LP14-01` | Gửi Push nhắc từ vựng khi thiết bị đang khóa, chế độ riêng tư bật | Màn hình khóa hiện: "WordSoul - Bạn có 1 thông báo mới." |
| `LP14-02` | Mở khóa thiết bị FaceID/Vân tay | Nội dung hiện đầy đủ trong Hộp thư App. |
| `LP14-03` | Kiểm thử hoàn tất luồng M10-LOCKSCREEN-PRIVACY-CONTROL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-LP-F01` | Bổ sung cờ `ShowLockscreenDetail` trong User Preferences | Cho phép người dùng tùy chỉnh | M10-T020 |

## 5. Tự kiểm M10-T014
- Đã đặc tả kiểm soát nội dung màn hình khóa M10-T014.
- Ghi nhận 2 Regression Gates (`LP-G01`–`LP-G02`) và 3 Test Cases (`LP14-01`–`LP14-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả kiểm soát nội dung màn hình khóa M10-T014 | WSA-7K2 |
