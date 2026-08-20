# Thiết kế thu hồi thiết bị nhận tin — lát A M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-REVOKE-PUSH-DEVICE-A-1.0` |
| Task | M01-T027-A |
| Đầu vào | M01-SESSION-MANAGEMENT-1.0 (D-018), M01-MULTI-DEVICE-PUSH-REGISTER-A-1.0 (D-090), M10-NOTIFICATION-SCHEDULE-1.0, REL-06 |
| Phạm vi | Giao thức thu hồi và hủy kích hoạt Token PUSH Notification của thiết bị người học, 3 kích hoạt thu hồi (Thủ công, Đăng xuất, Epoch An ninh) và đồng bộ với bộ phân phối M10 |
| Tự kiểm | A-G01, A-G05; REL-06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thu hồi và Hủy Kích hoạt Thiết bị Nhận tin — Lát A (`Revoke & Unregister PUSH Device Protocol - Slice A`) thuộc M01, đảm bảo các tin nhắn PUSH Notification không bao giờ bị gửi nhầm đến thiết bị đã đăng xuất hoặc thiết bị bị thu hồi quyền truy cập.

- **3 Kích hoạt Thu hồi Thiết bị Nhận tin (`3-Trigger Revocation Invariant`)**:
  - *Thủ công (`USER_MANUAL_UNREGISTER`)*: Người dùng chọn hủy kích hoạt một thiết bị cụ thể trong cài đặt hồ sơ.
  - *Đăng xuất (`USER_LOGOUT`)*: Người dùng đăng xuất khỏi 1 thiết bị hoặc chọn "Đăng xuất khỏi tất cả các thiết bị". Hệ thống tự động hủy kích hoạt các thiết bị PUSH tương ứng.
  - *Sự cố An ninh (`SECURITY_EPOCH_REVOCATION`)*: Khi người dùng đổi mật khẩu hoặc `SecurityEpoch` bị tăng $+1$ (M01-T018 / D-027), TOÀN BỘ thiết bị PUSH của tài khoản đó tự động chuyển sang `IsActive = false`.
- **Ràng buộc Thời gian Ngắt Tin PUSH SLA $\le 10\text{s}$ (`Immediate PUSH Halting Invariant`)**: Ngay khi thiết bị bị thu hồi, hệ thống phát sự kiện `PushDeviceRevokedIntegrationEvent` tới Module M10. Bộ phân phối tin M10 ngừng gửi tin PUSH tới DeviceToken đó trong thời gian $\le 10$ giây.
- **Bảo toàn Bản ghi Kiểm toán Soft-Deactivation (`Soft-Deactivation Audit Trail`)**: Thu hồi thiết bị KHÔNG xóa cứng bản ghi trong CSDL, mà chuyển `IsActive = false` và cập nhật `UnregisteredAtUtc` phục vụ đối soát và phân tích an ninh.
- **Ràng buộc Quyền Thu hồi (`Revocation Authorization Guard`)**: Người học chỉ được thu hồi thiết bị PUSH thuộc tài khoản của mình. Quyền `SecurityAdmin` hoặc `SuperAdmin` có thể thu hồi thiết bị của bất kỳ tài khoản nào để ngăn ngừa rủi ro rò rỉ dữ liệu.

## 2. Các Kích hoạt Thu hồi Thiết bị (Revocation Triggers & Effects)

| Mã Kích hoạt (`TriggerCode`) | Nguồn Khởi tạo | Phạm vi Thu hồi | Trạng thái Mới | Sự kiện Phát ra |
|---|---|---|---|---|
| `REV_MANUAL_SINGLE` | User chủ sở hữu | 1 Thiết bị (`DeviceId` cụ thể) | `IsActive = false` | `PushDeviceRevokedIntegrationEvent` |
| `REV_LOGOUT_CURRENT` | User đăng xuất | 1 Thiết bị hiện tại | `IsActive = false` | `PushDeviceRevokedIntegrationEvent` |
| `REV_LOGOUT_ALL` | User chọn Logout All | $100\%$ Thiết bị của User | `IsActive = false` | `PushDeviceRevokedIntegrationEvent` |
| `REV_SECURITY_EPOCH` | Đổi MK / SecurityAdmin | $100\%$ Thiết bị của User | `IsActive = false` | `PushDeviceRevokedIntegrationEvent` |
| `REV_EXPIRED_ROTATE` | Tự động khi đăng ký TB 6 | 1 Thiết bị cũ nhất | `IsActive = false` | `PushDeviceRevokedIntegrationEvent` |

## 3. Quy trình Thu hồi Thiết bị PUSH (Device Revocation Engine)

```
[Trigger Revocation Event (Manual / Logout / Security Epoch)]
                               |
                               v
           [Locate Active Push Devices in DB]
           - Filter: UserId == targetUserId AND IsActive == true
           - (Optional): DeviceId == targetDeviceId
                               |
                               v
           [Update DB State: Soft-Deactivation]
           - Set IsActive = false
           - Set UnregisteredAtUtc = DateTime.UtcNow
           - Set RevocationReason = TriggerCode
                               |
                               v
           [Publish PushDeviceRevokedIntegrationEvent]
           - DeviceToken, DeviceId, UserId
                               |
                               v
           [Module M10 Receives Event (SLA <= 10s)]
           - Purge DeviceToken from Redis Dispatch Queue
           - Stop pending PUSH notifications
```

## 4. Giao thức Thực thi Thu hồi Thiết bị (RevokePushDeviceService)

```csharp
public async Task<bool> RevokeDeviceAsync(string currentUserId, string targetDeviceId, string currentUserRole)
{
    var device = await _db.UserPushDevices.FirstOrDefaultAsync(d => d.DeviceId == targetDeviceId);
    if (device == null) throw new InvalidOperationException("PUSH_DEVICE_NOT_FOUND");

    // 1. Kiểm tra Quyền Thu hồi
    if (device.UserId != currentUserId && currentUserRole != "SecurityAdmin" && currentUserRole != "SuperAdmin")
    {
        throw new UnauthorizedAccessException("REVOKE_DEVICE_FORBIDDEN");
    }

    if (!device.IsActive) return true; // Đã inactive sẵn

    // 2. Soft-Deactivation
    device.IsActive = false;
    device.UnregisteredAtUtc = DateTime.UtcNow;
    device.RevocationReason = "USER_MANUAL_UNREGISTER";

    await _db.SaveChangesAsync();

    // 3. Ghi vết Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-05", currentUserId, new { Action = "REVOKE_PUSH_DEVICE", DeviceId = targetDeviceId, TargetUserId = device.UserId });

    // 4. Phát sự kiện ngắt PUSH sang Module M10
    await _eventPublisher.PublishAsync(new PushDeviceRevokedIntegrationEvent
    {
        UserId = device.UserId,
        DeviceId = device.DeviceId,
        DeviceToken = device.DeviceToken,
        RevokedAtUtc = device.UnregisteredAtUtc.Value
    });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RD-G01` | Đăng xuất khỏi thiết bị hiện tại tự động chuyển `IsActive = false` cho thiết bị PUSH tương ứng. |
| `RD-G02` | Lệnh Đăng xuất khỏi tất cả các thiết bị (`LogoutAll`) tự động thu hồi $100\%$ thiết bị PUSH của tài khoản. |
| `RD-G03` | Sự kiện tăng `SecurityEpoch` (đổi MK / Admin khóa) tự động thu hồi $100\%$ thiết bị PUSH của tài khoản. |
| `RD-G04` | Thu hồi thiết bị PUSH phát sự kiện `PushDeviceRevokedIntegrationEvent` tới M10 trong SLA $\le 10$ giây. |
| `RD-G05` | Thu hồi thiết bị duy trì Soft-Deactivation (`IsActive = false`, giữ lại lịch sử đối soát CSDL). |
| `RD-G06` | Cấm người học thu hồi thiết bị PUSH thuộc sở hữu của người học khác (Deny 403). |
| `RD-G07` | `SecurityAdmin` và `SuperAdmin` có thẩm quyền thu hồi thiết bị PUSH của bất kỳ tài khoản nào. |
| `RD-G08` | 100% thao tác thu hồi thiết bị PUSH ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-05`). |
| `RD-G09` | SLA thực thi API thu hồi thiết bị PUSH và phát sự kiện $< 25\text{ms}$. |
| `RD-G10` | 100% các test case tự kiểm RD27-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RD27-01` | Người học A tự chọn thu hồi thiết bị PUSH iPhone 15 | Thu hồi thành công, `IsActive = false`, cập nhật `UnregisteredAtUtc` |
| `RD27-02` | Người học A đăng xuất khỏi ứng dụng trên Android Tablet | Tự động thu hồi token PUSH của Android Tablet |
| `RD27-03` | Người học A thực hiện "Đăng xuất khỏi tất cả các thiết bị" | Chuyển `IsActive = false` cho toàn bộ 5 thiết bị PUSH của A |
| `RD27-04` | Người học A đổi mật khẩu tài khoản thành công | Tự động tăng `SecurityEpoch` $+1$ và thu hồi toàn bộ thiết bị PUSH |
| `RD27-05` | `SecurityAdmin` thực hiện khóa tài khoản A | Tự động thu hồi toàn bộ 5 thiết bị PUSH của A |
| `RD27-06` | Kiểm tra sự kiện tích hợp phát ra sau khi thu hồi thiết bị | Module M10 nhận `PushDeviceRevokedIntegrationEvent` trong $< 5\text{s}$ |
| `RD27-07` | Bộ phân phối tin M10 thử gửi PUSH tới DeviceToken vừa bị thu hồi | Job M10 loại bỏ token bị thu hồi khỏi hàng chờ gửi tin |
| `RD27-08` | Người học A chọn thu hồi 1 thiết bị đã inactive sẵn | API trả về 200 OK thành công mà không báo lỗi |
| `RD27-09` | Người học A thử thu hồi thiết bị PUSH của Người học B | System deny 403 `REVOKE_DEVICE_FORBIDDEN` |
| `RD27-10` | `SecurityAdmin` thu hồi thiết bị PUSH của Người học B | Thu hồi thành công, ghi vết Audit Log M11 |
| `RD27-11` | Tra cứu vết Audit Log M11 sau khi thu hồi thiết bị | Ghi nhận Audit Event `ACT-M11-05` với mã `REVOKE_PUSH_DEVICE` |
| `RD27-12` | Tải đồng thời 50 request thu hồi thiết bị PUSH | Response latency p95 $< 30\text{ms}$ |
| `RD27-13` | User chưa đăng nhập thử gọi API thu hồi thiết bị | Deny 401 Unauthorized |
| `RD27-14` | Đăng ký lại thiết bị PUSH vừa bị thu hồi trước đó | Tái kích hoạt `IsActive = true`, cập nhật token mới thành công |
| `RD27-15` | Tra cứu danh sách thiết bị đã thu hồi của tài khoản A | Trả về các bản ghi `IsActive = false` kèm lý do thu hồi |
| `RD27-16` | Thu hồi thiết bị PUSH khi tài khoản ở trạng thái `Locked` | Thực hiện thu hồi thành công |
| `RD27-17` | Phân tích tham chiếu các thiết bị PUSH khi thu hồi tài khoản | Quét CSDL `UserPushDevices` để đồng bộ trạng thái (T020) |
| `RD27-18` | Thao tác thu hồi thiết bị bị gián đoạn giữa chừng do lỗi DB | Rollback transaction, trạng thái active thiết bị giữ nguyên |
| `RD27-19` | Thu hồi thiết bị Nền tảng Web Push Browser | Hủy kích hoạt Web Push Token thành công |
| `RD27-20` | Kiểm thử hoàn tất luồng thiết kế thu hồi thiết bị M01-REVOKE-PUSH-DEVICE-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-RD-I01` | Luồng đăng xuất hiện tại chưa vô hiệu hóa Token PUSH Notification | Thiết bị đã đăng xuất vẫn nhận được PUSH Notification của người dùng | M01-T049 (Source task) |
| `M01-RD-I02` | Đổi mật khẩu chưa tự động hủy các thiết bị PUSH đã đăng ký trước đó | Rủi ro người cầm thiết bị cũ vẫn nhận được thông báo riêng tư | M01-T049; REL-06 |
| `M01-RD-I03` | Thiếu phát sự kiện `PushDeviceRevokedIntegrationEvent` cho Module M10 | Module M10 tiếp tục thử gửi tin PUSH gây tốn tài nguyên | M01-T049; M10-T008 |
| `M01-RD-I04` | Thiếu cờ `RevocationReason` để phân tích lý do ngắt nhận tin | Không phân biệt được do người dùng hủy hay do sự cố an ninh | M01-T049 |
| `M01-RD-I05` | Chưa phân quyền `SecurityAdmin` được phép thu hồi thiết bị PUSH từ xa | Khó khăn cho bộ phận hỗ trợ khi xử lý khiếu nại lộ thông tin | M01-T049 |

- `M01-RD-F01`: Tích hợp luồng thu hồi Push Device vào `LogoutAsync` và `ChangePasswordAsync` (tiếp nhận: M01-T049).
- `M01-RD-F02`: Triển khai `RevokePushDeviceService` với 3 Kích hoạt Thu hồi (tiếp nhận: M01-T049; REL-06).
- `M01-RD-F03`: Tích hợp phát `PushDeviceRevokedIntegrationEvent` sang Module M10 (tiếp nhận: M01-T049; M10-T008).
- `M01-RD-F04`: Thiết lập bộ kiểm thử tự động RD-G01–G10 và RD27-01–20 (tiếp nhận: M01 tasks).
- `M01-RD-F05`: Thu thập bằng chứng runtime cho luồng thu hồi thiết bị PUSH M01 (tiếp nhận: M01 tasks; A-G01/A-G05).

## 8. Tự kiểm M01-T027-A

- Đã thiết kế hoàn chỉnh `M01-REVOKE-PUSH-DEVICE-A-1.0` với Giao thức Thu hồi Thiết bị 3 Kích hoạt.
- Đã chốt Ràng buộc Thời gian Ngắt Tin PUSH SLA $\le 10\text{s}$ đối với Module M10.
- Đã chốt Ràng buộc Soft-Deactivation giữ lại vết lịch sử kiểm toán CSDL.
- Đã lồng ghép Tích hợp Thu hồi tự động khi Đổi mật khẩu (`SecurityEpoch`) và Phát sự kiện M10 (REL-06).
- Đã xác lập 10 Regression Gates (`RD-G01`–`RD-G10`) và 20 Test Cases tự kiểm (`RD27-01`–`RD27-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế thu hồi thiết bị nhận tin M01-T027-A | WSA-7K2 |
