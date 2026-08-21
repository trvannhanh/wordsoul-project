# Chuẩn hóa từ điển thông báo và hộp thư M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIF-INBOX-DICT-1.0` |
| Task | M10-T001 |
| Đầu vào | M01-T001 (Từ điển danh tính), M01-T025-A (Preferences Timezone), REL-06 (Consent & Quiet Hours) |
| Phạm vi | Chuẩn hóa từ điển thông báo, Hộp thư In-App (In-App Inbox), Kênh Push/Email, Giờ yên lặng (Quiet Hours) và Ma trận Đồng ý nhận tin (Opt-In / Opt-Out Matrix) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định thuật ngữ, phân loại kênh và quy tắc tôn trọng quyền riêng tư của Module M10 (Thông báo và duy trì tương tác).

- **Nguyên tắc Tôn trọng Đồng ý và Giờ Yên lặng (`Consent & Quiet Hours Invariant - REL-06`)**:
  - KHÔNG BAO GIỜ gửi thông báo Push/Email ngoài giờ cho phép nếu người học bật Giờ yên lặng (`QuietHoursEnabled = true`, ví dụ: 22:00 - 07:00 theo múi giờ địa phương `PreferredTimezone`).
  - Tuyệt đối tuân thủ lựa chọn hủy nhận tin (`Opt-Out`). Nếu người học tắt kênh Email/Push cho mục `"Nhắc học tập"`, hệ thống BẮT BUỘC bỏ qua việc gửi tin nhắn đó.
- **Phân tách Hộp thư In-App và Thông báo Đẩy (`In-App Inbox vs Push Notification`)**:
  - *In-App Inbox*: Thông điệp lưu giữ trong hệ thống người học có thể đọc lại (thông báo hệ thống, phần thưởng, cập nhật chính sách).
  - *Push Notification / Email*: Kênh gửi tin ra bên ngoài qua Firebase FCM / SMTP Server (tuân thủ M12 delivery contract).

## 2. Bảng Từ điển Thuật ngữ Thông báo M10 (Notification Lexicon)

| Thuật ngữ | Tên tiếng Việt | Mô tả & Quy tắc trong WordSoul | Kênh áp dụng |
|---|---|---|---|
| `InAppNotification` | Thông báo hộp thư | Tin nhắn hiển thị trong ứng dụng, có trạng thái `Unread` / `Read`. | In-App Inbox |
| `PushNotification` | Thông báo đẩy | Tin nhắn PUSH tới thiết bị di động đã đăng ký (M01-T026-A). | FCM / APNS |
| `EmailNotification` | Thư điện tử | Email hệ thống hoặc nhắc nhở học tập gửi tới email xác minh. | SMTP / Provider |
| `QuietHours` | Giờ yên lặng | Khung giờ địa phương cấm gửi Push notification (Mặc định 22:00 -> 07:00). | Push / Email |
| `NotificationCategory` | Danh mục thông báo | Phân loại: `SYSTEM_SECURITY` (Bắt buộc), `STUDY_REMINDER` (Tùy chọn), `REWARD_PROMO` (Tùy chọn). | All Channels |
| `OptInMatrix` | Ma trận đồng ý | Bảng cấu hình lưu lựa chọn bật/tắt nhận tin theo từng danh mục và kênh của người học. | Preferences |

## 3. Ma trận Lựa chọn Nhận tin (Opt-In / Opt-Out Matrix)

| Danh mục Thông báo | Loại | Mặc định | Quyền Hủy (Opt-Out) | Giờ Yên lặng Áp dụng |
|---|---|---|---|---|
| `SECURITY_ALERT` | Cảnh báo an ninh | `ALWAYS_ON` | CẤM TẮT | Bỏ qua (Gửi ngay SLA <= 10s) |
| `ACCOUNT_SYSTEM` | Thông báo tài khoản | `ALWAYS_ON` | CẤM TẮT | Áp dụng |
| `STUDY_REMINDER` | Nhắc học tập / Ôn tập | `OPT_IN` | CHO PHÉP TẮT | Áp dụng (Chờ hết giờ yên lặng) |
| `QUEST_PROGRESS` | Nhắc nhiệm vụ | `OPT_IN` | CHO PHÉP TẮT | Áp dụng |
| `MARKETING_NEWS` | Tin tức & Khuyến mãi | `OPT_OUT` | CHO PHÉP TẮT | Áp dụng |

## 4. Thuật toán Kiểm tra Điều kiện Gửi Thông báo (Notification Delivery Guard)

```csharp
public bool CanSendNotification(UserId userId, NotificationCategory category, ChannelType channel)
{
    // 1. Cảnh báo an ninh luôn luôn được gửi
    if (category == NotificationCategory.SECURITY_ALERT) return true;

    // 2. Kiểm tra Opt-In Matrix của người dùng
    var preference = _userPrefRepo.GetPreference(userId);
    if (!preference.IsChannelEnabled(category, channel))
    {
        return false; // Người học đã Opt-Out -> Bỏ qua
    }

    // 3. Kiểm tra Giờ Yên Lặng (Quiet Hours)
    if (preference.QuietHoursEnabled && channel == ChannelType.PUSH)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, preference.UserTimeZone);
        if (localTime.Hour >= preference.QuietHourStart || localTime.Hour < preference.QuietHourEnd)
        {
            return false; // Trong giờ yên lặng -> Bỏ qua hoặc hoãn gửi
        }
    }

    return true;
}
```

## 5. Regression Gates và Test Cases

### 5.1. Regression Gates
- `ND-G01`: 100% thông báo loại `STUDY_REMINDER` bị chặn gửi PUSH trong khung giờ yên lặng của người dùng (REL-06).
- `ND-G02`: Người học tắt nhận email `STUDY_REMINDER` thì hệ thống 100% không phát lệnh gửi email sang M12.
- `ND-G03`: Thông báo cảnh báo an ninh `SECURITY_ALERT` bỏ qua giờ yên lặng và luôn được gửi ngay lập tức.
- `ND-G04`: 100% tin nhắn Hộp thư In-App lưu đúng trạng thái `IsRead = false` khi mới tạo.

### 5.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `ND01-01` | Đến giờ nhắc học tập lúc 23:00 (Trong giờ yên lặng 22:00-07:00) | System hoãn/bỏ qua lệnh gửi Push notification. |
| `ND01-02` | Phát cảnh báo đăng nhập lạ lúc 02:00 sáng (`SECURITY_ALERT`) | System gửi Push/Email lập tức không bị chặn bởi Giờ yên lặng. |
| `ND01-03` | Người học tắt kênh Push cho `STUDY_REMINDER` trong cài đặt | Hệ thống bỏ qua không tạo Push notification khi có từ đến hạn ôn. |
| `ND01-04` | Tạo thông báo Hộp thư In-App mới | Hộp thư hiển thị thông báo với `IsRead = false` và `UnreadCount` tăng 1. |
| `ND01-05` | Người học bấm "Đánh dấu đã đọc" thông báo trong Hộp thư | `IsRead = true`, `UnreadCount` giảm 1. |
| `ND01-06` | Kiểm thử hoàn tất luồng M10-NOTIF-INBOX-DICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 6. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-ND-F01` | Cần tạo bảng `UserNotificationPreferences` lưu Opt-In matrix và Giờ yên lặng | Chưa có schema lưu cấu hình nhận tin người dùng | M10-T002 |

## 7. Tự kiểm M10-T001
- Đã hoàn thành từ điển thông báo M10, bảo đảm tuân thủ REL-06 về Giờ yên lặng và Opt-Out matrix.
- Xác lập 4 Regression Gates (`ND-G01`–`ND-G04`) và 6 Test Cases (`ND01-01`–`ND01-06`).

## 8. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa từ điển thông báo và hộp thư M10-T001 | WSA-7K2 |
