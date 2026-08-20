# Thiết kế đăng ký nhiều thiết bị nhận tin — lát A M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-MULTI-DEVICE-PUSH-REGISTER-A-1.0` |
| Task | M01-T026-A |
| Đầu vào | M01-SESSION-POLICY-1.0 (D-027), M01-TIMEZONE-PREFERENCE-A-1.0 (D-089), M10-NOTIFICATION-SCHEDULE-1.0, REL-06 |
| Phạm vi | Giao thức đăng ký và quản lý danh sách nhiều thiết bị nhận tin PUSH Notification (FCM/APNS Token), quy tắc giới hạn tối đa 5 thiết bị active và tự động thu hồi thiết bị cũ |
| Tự kiểm | A-G01, A-G05; REL-06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đăng ký Nhiều Thiết bị Nhận tin Notification PUSH — Lát A (`Multi-Device PUSH Token Registration Protocol - Slice A`) thuộc M01, cho phép một người học nhận tin thông báo PUSH nhắc học (REL-06) trên nhiều thiết bị cùng lúc (Điện thoại iOS, Máy tính bảng Android, Trình duyệt Web).

- **Bảo toàn Giới hạn Tối đa 5 Thiết bị Active (`5-Active-Device Limit Invariant`)**: Mỗi tài khoản người học được phép có tối đa 5 thiết bị nhận tin PUSH active đồng thời (`MaxActivePushDevices = 5`). Khi đăng ký thiết bị thứ 6, hệ thống TỰ ĐỘNG hủy kích hoạt (`IsActive = false`) thiết bị có mốc `LastActiveAtUtc` cũ nhất.
- **Ràng buộc Định danh Thiết bị Duy nhất (`Unique Device Identity Invariant`)**: Cặp khóa `(UserId, DeviceId)` là duy nhất trong CSDL. Khi ứng dụng làm mới Token PUSH (FCM/APNS Token Refresh), hệ thống thực hiện Cập nhật (`UPSERT`) bản ghi cũ thay vì chèn trùng lặp.
- **Phân loại Nền tảng và Phiên bản Đăng ký (`Platform Enum & AppVersion`)**: Mỗi đăng ký phải khai báo rõ Nền tảng (`iOS`, `Android`, `Web`) và `AppVersion`. CẤM lưu token không kèm theo định danh nền tảng.
- **Tự động Cập nhật Mốc Hoạt động (`LastActiveAtUtc Auto-Touch`)**: Khi thiết bị gửi request tương tác học tập hoặc đăng ký lại token, mốc `LastActiveAtUtc` tự động làm mới để bảo vệ thiết bị khỏi bị xoay vòng loại bỏ.

## 2. Mô hình Thực thể Thiết bị Nhận tin (UserPushDevice Schema)

```json
{
  "userDeviceId": 100241,
  "userId": "USR-10024",
  "deviceId": "DEVICE-IOS-IPHONE15-PRO-MAX-001",
  "deviceToken": "fcm_token_eX9K3mP8q2N...:APA91bH...",
  "platform": "iOS",
  "deviceModel": "iPhone 15 Pro Max",
  "appVersion": "1.4.2",
  "isActive": true,
  "registeredAtUtc": "2026-08-20T08:00:00Z",
  "lastActiveAtUtc": "2026-08-20T10:30:00Z"
}
```

## 3. Kiến trúc Đăng ký Thiết bị và Tự động Thu hồi Thiết bị Cũ (Device Registration Engine)

```
[Client App Sends Push Registration Request (DeviceId, DeviceToken, Platform)]
                                      |
                                      v
                      [Check Existing (UserId, DeviceId)]
                                      |
                 +--------------------+--------------------+
                 | (Device Exists)                         | (New Device)
                 v                                         v
        [Update DeviceToken & LastActive]        [Check Active Devices Count]
        - Set IsActive = true                    - Count(IsActive == true)
                 |                                         |
                 |                       +-----------------+-----------------+
                 |                       | (Count >= 5)                      | (Count < 5)
                 |                       v                                   v
                 |           [Deactivate Oldest Active Device]       [Insert New Device Record]
                 |           - Find Min(LastActiveAtUtc)             - Set IsActive = true
                 |           - Set IsActive = false                          |
                 +----------------------->+<---------------------------------+
                                          |
                                          v
                              [Publish PushDeviceRegisteredEvent]
                              - Notify Module M10 for PUSH target updating
```

## 4. Giao thức Thực thi Đăng ký Thiết bị (RegisterPushDeviceService)

```csharp
public async Task<UserPushDeviceDto> RegisterDeviceAsync(string currentUserId, RegisterPushDeviceRequestDto dto)
{
    // 1. Validate Input
    if (string.IsNullOrEmpty(dto.DeviceId) || dto.DeviceId.Length > 100)
        throw new ArgumentException("INVALID_DEVICE_ID");

    if (string.IsNullOrEmpty(dto.DeviceToken) || dto.DeviceToken.Length > 500)
        throw new ArgumentException("INVALID_DEVICE_TOKEN");

    if (!Enum.IsDefined(typeof(PushPlatform), dto.Platform))
        throw new ArgumentException("INVALID_PUSH_PLATFORM");

    var nowUtc = DateTime.UtcNow;

    // 2. Tìm bản ghi thiết bị đã tồn tại (UserId, DeviceId)
    var device = await _db.UserPushDevices
        .FirstOrDefaultAsync(d => d.UserId == currentUserId && d.DeviceId == dto.DeviceId);

    if (device != null)
    {
        // Update Token & LastActive
        device.DeviceToken = dto.DeviceToken;
        device.Platform = dto.Platform;
        device.AppVersion = dto.AppVersion;
        device.DeviceModel = dto.DeviceModel;
        device.IsActive = true;
        device.LastActiveAtUtc = nowUtc;
    }
    else
    {
        // 3. Nếu là thiết bị mới -> Kiểm tra số lượng thiết bị active (Max = 5)
        var activeDevices = await _db.UserPushDevices
            .Where(d => d.UserId == currentUserId && d.IsActive)
            .OrderBy(d => d.LastActiveAtUtc)
            .ToListAsync();

        if (activeDevices.Count >= 5)
        {
            // Tự động thu hồi thiết bị active cũ nhất
            var oldestDevice = activeDevices.First();
            oldestDevice.IsActive = false;
        }

        // Chèn bản ghi thiết bị mới
        device = new UserPushDevice
        {
            UserId = currentUserId,
            DeviceId = dto.DeviceId,
            DeviceToken = dto.DeviceToken,
            Platform = dto.Platform,
            DeviceModel = dto.DeviceModel,
            AppVersion = dto.AppVersion,
            IsActive = true,
            RegisteredAtUtc = nowUtc,
            LastActiveAtUtc = nowUtc
        };
        _db.UserPushDevices.Add(device);
    }

    await _db.SaveChangesAsync();

    // 4. Phát sự kiện đăng ký thiết bị sang M10
    await _eventPublisher.PublishAsync(new PushDeviceRegisteredIntegrationEvent
    {
        UserId = currentUserId,
        DeviceId = device.DeviceId,
        DeviceToken = device.DeviceToken,
        Platform = device.Platform.ToString()
    });

    return MapToDto(device);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `MD-G01` | Cấm 1 tài khoản có quá 5 thiết bị nhận tin PUSH active cùng lúc (`MaxActivePushDevices = 5`). |
| `MD-G02` | Đăng ký thiết bị thứ 6 tự động hủy kích hoạt thiết bị active có `LastActiveAtUtc` cũ nhất. |
| `MD-G03` | Cặp khóa `(UserId, DeviceId)` là duy nhất, tự động UPSERT khi làm mới Token PUSH. |
| `MD-G04` | Cấm đăng ký Token PUSH nếu thiếu `Platform` (`iOS`, `Android`, `Web`) hoặc `DeviceId`. |
| `MD-G05` | Thao tác tương tác học tập tự động chạm làm mới mốc `LastActiveAtUtc` của thiết bị tương ứng. |
| `MD-G06` | Đăng ký thiết bị thành công tự động phát sự kiện `PushDeviceRegisteredIntegrationEvent` cho M10. |
| `MD-G07` | 100% thao tác đăng ký thiết bị ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-05`). |
| `MD-G08` | Phân quyền thực hiện đăng ký thiết bị tuân thủ ma trận vai trò M01-T028 (`Learner` tự đăng ký). |
| `MD-G09` | SLA thực thi API đăng ký thiết bị và phát sự kiện M10 $< 30\text{ms}$. |
| `MD-G10` | 100% các test case tự kiểm MD26-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MD26-01` | Người học A đăng ký thiết bị PUSH đầu tiên (iPhone 15) | Đăng ký thành công, `IsActive = true`, số thiết bị active = 1 |
| `MD26-02` | Người học A đăng ký tiếp 4 thiết bị PUSH nữa (đủ 5 thiết bị) | Cả 5 thiết bị đều ở trạng thái `IsActive = true` |
| `MD26-03` | Người học A đăng ký thiết bị PUSH thứ 6 (Web Browser) | Thiết bị thứ 6 active, thiết bị cũ nhất (iPhone 15) tự động `IsActive = false` |
| `MD26-04` | iPhone 15 mở lại ứng dụng và gửi request đăng ký lại | iPhone 15 trở lại `IsActive = true`, thiết bị đứng thứ 2 bị hủy active |
| `MD26-05` | Làm mới Token FCM cho thiết bị đã đăng ký | UPSERT bản ghi cũ, cập nhật `DeviceToken` mới, giữ nguyên 1 bản ghi |
| `MD26-06` | Thử đăng ký với `Platform` không hợp lệ (`"WindowsPhone"`) | Reject 400 `INVALID_PUSH_PLATFORM` |
| `MD26-07` | Thử đăng ký với `DeviceId` rỗng | Reject 400 `INVALID_DEVICE_ID` |
| `MD26-08` | Thử đăng ký với `DeviceToken` rỗng | Reject 400 `INVALID_DEVICE_TOKEN` |
| `MD26-09` | Kiểm tra sự kiện tích hợp phát ra sau khi đăng ký thiết bị | Module M10 nhận được `PushDeviceRegisteredIntegrationEvent` |
| `MD26-10` | Người học A nộp bài học M03 từ Android Tablet | Mốc `LastActiveAtUtc` của Android Tablet tự động cập nhật giờ hiện tại |
| `MD26-11` | Tra cứu vết Audit Log M11 sau khi đăng ký thiết bị mới | Ghi nhận Audit Event `ACT-M11-05` với đính kèm `DeviceId` |
| `MD26-12` | Tải đồng thời 50 request đăng ký thiết bị từ 50 người dùng | Response latency p95 $< 35\text{ms}$ |
| `MD26-13` | Người học A thử đăng ký thiết bị PUSH cho Người học B | Deny 403 Forbidden |
| `MD26-14` | User chưa đăng nhập thử gọi API đăng ký thiết bị PUSH | Deny 401 Unauthorized |
| `MD26-15` | Đăng ký thiết bị PUSH trên nền tảng Web (`Web Push Standard`) | Đăng ký thành công với `Platform = Web` |
| `MD26-16` | Tra cứu danh sách các thiết bị active của người dùng A | Trả về danh sách 5 thiết bị active kèm thông tin model |
| `MD26-17` | Phân tích tham chiếu các thiết bị khi hủy tài khoản M01 | Quét toàn bộ danh sách `UserPushDevices` để vô hiệu hóa (T020) |
| `MD26-18` | Thao tác đăng ký thiết bị bị gián đoạn do lỗi DB | Rollback transaction, danh sách thiết bị active giữ nguyên |
| `MD26-19` | Đăng ký thiết bị với `DeviceId` dài 100 ký tự tối đa | Đăng ký thành công |
| `MD26-20` | Kiểm thử hoàn tất luồng đăng ký nhiều thiết bị nhận tin M01-MULTI-DEVICE-PUSH-REGISTER-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-MD-I01` | Entity `User.cs` hiện tại chỉ lưu 1 `PushToken` đơn lẻ | Người học đăng nhập thiết bị 2 sẽ làm đứt token PUSH của thiết bị 1 | M01-T049 (Source task) |
| `M01-MD-I02` | Chưa có bảng `UserPushDevices` quản lý danh sách nhiều thiết bị | Rủi ro gửi tin PUSH trùng lặp hoặc không đến đúng thiết bị active | M01-T049; M10-T008 |
| `M01-MD-I03` | Thiếu quy tắc xoay vòng tự động hủy active thiết bị thứ 6 | Số lượng token rác tích tụ làm tăng chi phí gửi PUSH FCM/APNS | M01-T049 |
| `M01-MD-I04` | Thiếu cờ `Platform` (`iOS`, `Android`, `Web`) trong payload token | Không tùy chỉnh được định dạng notification theo nền tảng | M01-T049 |
| `M01-MD-I05` | Chưa phát sự kiện `PushDeviceRegisteredIntegrationEvent` sang M10 | Module M10 không cập nhật danh sách thiết bị đích nhận PUSH | M01-T049; M10-T008 |

- `M01-MD-F01`: Tạo entity `UserPushDevice.cs` và CSDL Migration (tiếp nhận: M01-T049).
- `M01-MD-F02`: Triển khai `RegisterPushDeviceService` với xoay vòng Max 5 active devices (tiếp nhận: M01-T049; REL-06).
- `M01-MD-F03`: Tích hợp phát `PushDeviceRegisteredIntegrationEvent` cho Module M10 (tiếp nhận: M01-T049; M10-T008).
- `M01-MD-F04`: Thiết lập bộ kiểm thử tự động MD-G01–G10 và MD26-01–20 (tiếp nhận: M01 tasks).
- `M01-MD-F05`: Thu thập bằng chứng runtime cho luồng đăng ký nhiều thiết bị M01 (tiếp nhận: M01 tasks; A-G01/A-G05).

## 8. Tự kiểm M01-T026-A

- Đã thiết kế hoàn chỉnh `M01-MULTI-DEVICE-PUSH-REGISTER-A-1.0` với Giao thức Đăng ký Nhiều Thiết bị 4 Bước.
- Đã chốt Ràng buộc Giới hạn Tối đa 5 Thiết bị Active (`MaxActivePushDevices = 5`).
- Đã chốt Ràng buộc Định danh Duy nhất `(UserId, DeviceId)` và tự động UPSERT.
- Đã lồng ghép Tự động Thu hồi Thiết bị Cũ nhất và Phát Sự kiện Tích hợp cho Module M10 (REL-06).
- Đã xác lập 10 Regression Gates (`MD-G01`–`MD-G10`) và 20 Test Cases tự kiểm (`MD26-01`–`MD26-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế đăng ký nhiều thiết bị nhận tin M01-T026-A | WSA-7K2 |
