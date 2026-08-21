# Thiết kế đăng xuất và quản lý phiên M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-LOGOUT-SESSION-MANAGEMENT-1.0` |
| Task | M01-T018 |
| Đầu vào | M01-SESSION-1.0 (D-028), M01-REVOKE-PUSH-DEVICE-A-1.0 (D-091), REL-01, REL-06 |
| Phạm vi | Đặc tả Giao thức Đăng xuất và Quản lý Phiên Làm việc Active (`Logout & Active Session Management Protocol`), 3 luồng đăng xuất (Đơn thiết bị, Toàn bộ thiết bị, Thiết bị chọn lọc), tự động thu hồi Push Device Token và vô hiệu phiên JWT SLA $\le 5$ giây |
| Tự kiểm | A-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đăng xuất và Quản lý Phiên làm việc (`Logout & Active Session Management Protocol`) thuộc M01, chuẩn hóa cơ chế hủy bỏ phiên đăng nhập active, thu hồi Refresh Token Family trong Redis và tự động vô hiệu hóa Push Device Token tương ứng ở M10 (D-091), bảo vệ an toàn cho người dùng khi mất thiết bị hoặc nghi ngờ lộ thông tin bảo mật.

- **3 Giao thức Đăng xuất Thống nhất (`3 Unified Logout Protocols Invariant`)**:
  - *Đăng xuất Đơn thiết bị (`Single Device Logout`)*: Hủy bỏ Refresh Token Family của duy nhất thiết bị hiện tại, thu hồi Push Token thiết bị đó SLA $\le 10$ giây (D-091).
  - *Đăng xuất Toàn bộ Thiết bị (`Logout All Devices`)*: Tăng giá trị `SecurityEpoch` $+1$ trong CSDL/Redis SLA $\le 5$ giây, vô hiệu toàn bộ Refresh Token Families và Push Device Tokens của người dùng trên mọi nền tảng.
  - *Đăng xuất Thiết bị Chọn lọc (`Revoke Specific Session`)*: Người dùng tra cứu danh sách thiết bị active và chủ động ngắt kết nối một thiết bị cụ thể từ xa (`sessionId`).
- **Gia tăng SecurityEpoch Tức thì SLA $\le 5\text{s}$ (`Instant SecurityEpoch Invalidation`)**: Khi thực hiện `LogoutAllDevices` hoặc ngắt phiên từ xa, giá trị `SecurityEpoch` của người dùng được tăng nguyên tử $+1$. Tất cả các JWT Access Token cũ chứa `security_epoch` thấp hơn sẽ bị từ chối ngay lập tức tại Middleware API Gateway (SLA $\le 5$ giây).
- **Tự động Thu hồi Push Device Token M10 (`Push Token Auto-Revocation`)**: Thao tác đăng xuất BẮT BUỘC phát sự kiện `PushDeviceRevokedIntegrationEvent` tới Module M10 để gỡ bỏ Push Token của thiết bị bị đăng xuất, ngăn ngừa việc gửi PUSH notification nhầm sang người dùng mới (D-091).
- **Lưu vết Sổ Kiểm toán M11 (`Logout Audit Trail`)**: $100\%$ các lệnh đăng xuất bắt buộc được ghi vết bất biến `ACT-M11-18` trong Sổ Kiểm toán M11, bao gồm `UserId`, `LogoutType` (`SINGLE`, `ALL`, `SPECIFIC`), `DeviceId` và `NewSecurityEpoch`.

## 2. Ma trận Giao thức Đăng xuất và Quản lý Phiên (Logout Matrix)

| Loại Đăng xuất (`LogoutType`) | API Endpoint | Phạm vi Tác động | Tác động SecurityEpoch | Thu hồi Push Token M10 | SLA Xử lý Redis |
|---|---|---|---|---|---|
| `SINGLE_DEVICE` | `POST /api/v1/auth/logout` | Duy nhất thiết bị hiện tại | Giữ nguyên | Thu hồi Token thiết bị này (D-091) | SLA $\le 10\text{ms}$ |
| `ALL_DEVICES` | `POST /api/v1/auth/logout-all` | Toàn bộ thiết bị active | Tăng nguyên tử $+1$ | Thu hồi $100\%$ Push Tokens | SLA $\le 5\text{ms}$ |
| `SPECIFIC_SESSION` | `DELETE /api/v1/auth/sessions/{sessionId}` | 1 Thiết bị được chọn | Giữ nguyên (Hủy Token Family) | Thu hồi Token thiết bị chọn | SLA $\le 10\text{ms}$ |
| `INACTIVE_TIMEOUT` | Tự động sau 14 ngày | Phiên không hoạt động | Giữ nguyên | Tự động chuyển `INACTIVE` | SLA $\le 1\text{m}$ |

## 3. Kiến trúc Luồng Đăng xuất và Quản lý Phiên (Logout Engine)

```
[User Initiates Logout Request (Single / All / Specific Session)]
                                |
                                v
               [Validate Current JWT Access Token]
                                |
         +----------------------+----------------------+
         | (Single Device)      | (Logout All)         | (Specific Session)
         v                      v                      v
   [Delete Refresh Token] [Increment SecurityEpoch +1] [Delete Specific Session]
   [Family in Redis]      [Clear ALL Sessions]         [Family in Redis]
         |                      |                      |
         +----------------------+----------------------+
                                |
                                v
             [Publish PushDeviceRevokedIntegrationEvent to M10]
                                |
                                v
             [Record Audit Event ACT-M11-18 in DB]
                                |
                                v
             [Return 200 OK Logout Success Response]
```

## 4. Giao thức Thực thi Đăng xuất CSDL (SessionLogoutManagementService)

```csharp
public async Task<bool> ExecuteLogoutAsync(string userId, string sessionId, LogoutType logoutType, string currentDeviceId)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    if (logoutType == LogoutType.SINGLE_DEVICE)
    {
        // 1. Revoke current session family in Redis
        string familyKey = $"wordsoul:session_family:{userId}:{sessionId}";
        await _redisDb.KeyDeleteAsync(familyKey);

        // 2. Publish Push Device Revocation Event to M10 SLA <= 10s
        if (!string.IsNullOrEmpty(currentDeviceId))
        {
            await _eventPublisher.PublishAsync(new PushDeviceRevokedIntegrationEvent {
                UserId = userId,
                DeviceId = currentDeviceId,
                Reason = "SINGLE_DEVICE_LOGOUT"
            });
        }
    }
    else if (logoutType == LogoutType.ALL_DEVICES)
    {
        // 3. Increment SecurityEpoch +1 in DB & Redis SLA <= 5s
        user.SecurityEpoch += 1;
        await _db.SaveChangesAsync();

        string epochKey = $"wordsoul:security_epoch:{userId}";
        await _redisDb.StringSetAsync(epochKey, user.SecurityEpoch);

        // Clear all Redis session keys for this user
        var server = _redisDb.GetServer(_redisConnection);
        foreach (var key in server.Keys(pattern: $"wordsoul:session_family:{userId}:*"))
        {
            await _redisDb.KeyDeleteAsync(key);
        }

        // Publish Revoke ALL Push Devices Event
        await _eventPublisher.PublishAsync(new PushDeviceRevokedIntegrationEvent {
            UserId = userId,
            DeviceId = "ALL",
            Reason = "LOGOUT_ALL_DEVICES"
        });
    }

    // 4. Record Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-18", userId, new {
        LogoutType = logoutType.ToString(),
        SessionId = sessionId,
        DeviceId = currentDeviceId,
        NewSecurityEpoch = user.SecurityEpoch
    });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LS-G01` | Đăng xuất đơn thiết bị gỡ bỏ Refresh Token Family của duy nhất thiết bị đó trong Redis. |
| `LS-G02` | Đăng xuất toàn bộ thiết bị (`LogoutAll`) tăng `SecurityEpoch` $+1$ trong CSDL/Redis SLA $\le 5$ giây. |
| `LS-G03` | Middleware API Gateway từ chối ngay lập tức (HTTP 401) các JWT chứa `security_epoch` cũ sau `LogoutAll`. |
| `LS-G04` | Thao tác đăng xuất tự động phát sự kiện thu hồi Push Device Token tương ứng tới M10 (D-091). |
| `LS-G05` | API tra cứu active sessions trả về đầy đủ danh sách thiết bị active (DeviceName, Platform, IP Masked). |
| `LS-G06` | 100% các lệnh đăng xuất được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-18`). |
| `LS-G07` | Người dùng được phép ngắt kết nối một thiết bị chọn lọc từ xa (`DELETE /sessions/{sessionId}`). |
| `LS-G08` | Phân quyền truy cập API xem và ngắt phiên chỉ dành cho chính chủ người học hoặc Quản trị an ninh. |
| `LS-G09` | SLA xử lý API đăng xuất đơn thiết bị $< 10\text{ms}$; đăng xuất toàn bộ $< 20\text{ms}$. |
| `LS-G10` | 100% các test case tự kiểm LS18-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LS18-01` | Người dùng đăng xuất đơn thiết bị tại Android App | Hủy session Android trong Redis, giữ nguyên session trên Web |
| `LS18-02` | Người dùng bấm "Đăng xuất khỏi tất cả thiết bị" | Tăng SecurityEpoch $+1$, ngắt phiên Android, iOS và Web |
| `LS18-03` | Gửi request dùng JWT cũ sau khi người dùng đã bấm LogoutAll | Reject 401 `INVALID_SECURITY_EPOCH` |
| `LS18-04` | Đăng xuất đơn thiết bị trên Android | M10 nhận sự kiện `PushDeviceRevokedIntegrationEvent` cho Android DeviceId |
| `LS18-05` | Tra cứu danh sách active sessions khi đang đăng nhập trên 3 thiết bị | Trả về DTO danh sách 3 thiết bị với thông tin IP đã che mờ |
| `LS18-06` | Người dùng trên Web bấm xóa phiên của iOS App | Session iOS bị ngắt tức thì, iOS App chuyển về màn hình đăng nhập |
| `LS18-07` | Tra cứu vết Audit Log M11 sau khi bấm LogoutAll | Ghi nhận Audit Event `ACT-M11-18` đính kèm SecurityEpoch mới |
| `LS18-08` | Thử ngắt phiên của một `sessionId` không thuộc sở hữu của mình | Reject 403 `SESSION_ACCESS_DENIED` |
| `LS18-09` | Tải đồng thời 100 request đăng xuất từ 100 người dùng | Logout processing latency p95 $< 15\text{ms}$ |
| `LS18-10` | Đăng xuất khi thiết bị chưa đăng ký Push Token | Đăng xuất bình thường, bỏ qua bước gửi event M10 |
| `LS18-11` | Thử gửi request `LogoutAll` khi JWT đã bị hết hạn | Reject 401 Unauthorized |
| `LS18-12` | CSDL SQL bị gián đoạn khi bấm `LogoutAll` | Fallback xóa session Redis trước, retry update SQL qua Outbox |
| `LS18-13` | User không phải Admin thử xem danh sách session của người khác | Deny 403 Forbidden |
| `LS18-14` | User chưa đăng nhập gọi API đăng xuất | Deny 401 Unauthorized |
| `LS18-15` | Đăng nhập mới trên thiết bị mới sau khi vừa `LogoutAll` | Cấp phiên mới với `SecurityEpoch` mới chính xác |
| `LS18-16` | Kiểm tra độ trễ vô hiệu JWT Access Token tại Gateway | Invalidation SLA $< 50\text{ms}$ |
| `LS18-17` | Phân tích tham chiếu danh sách Refresh Token Families trong Redis | Quét namespace `wordsoul:{env}:session_family` (T020) |
| `LS18-18` | Phiên làm việc tự động hết hạn sau 14 ngày không hoạt động | Worker tự động dọn dẹp Redis key |
| `LS18-19` | Người học đổi mật khẩu (M01-T020) kích hoạt tự động `LogoutAll` | Tự động tăng SecurityEpoch $+1$, ngắt sạch thiết bị khác |
| `LS18-20` | Kiểm thử hoàn tất luồng đăng xuất và quản lý phiên M01-LOGOUT-SESSION-MANAGEMENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-LS-I01` | API đăng xuất hiện tại chỉ xóa token phía Client, chưa hủy Refresh Token trong Redis | Risk Refresh Token cũ vẫn dùng để xin cấp JWT mới được | M01-T049 (Source task) |
| `M01-LS-I02` | Chưa kết nối luồng đăng xuất với sự kiện thu hồi Push Device Token M10 | Người dùng đăng xuất nhưng vẫn nhận PUSH notification | M01-T049; M01-T027-A |
| `M01-LS-I03` | Thiếu API `GET /api/v1/auth/sessions` cho người dùng xem các thiết bị active | Người dùng không kiểm tra được tài khoản có bị đăng nhập lạ không | M01-T049 |
| `M01-LS-I04` | Thiếu tính năng ngắt kết nối chọn lọc một thiết bị từ xa | Không cho phép người dùng buộc đăng xuất thiết bị bị mất | M01-T049 |
| `M01-LS-I05` | Chưa kết nối sự kiện đăng xuất với Sổ Kiểm toán M11 (`ACT-M11-18`) | Không ghi log đối soát thời điểm đăng xuất của người học | M01-T049; M11-T031 |

- `M01-LS-F01`: Triển khai `SessionLogoutManagementService` quản lý 3 luồng đăng xuất (tiếp nhận: M01-T049).
- `M01-LS-F02`: Tích hợp Bắt buộc `PushDeviceRevokedIntegrationEvent` sang M10 (tiếp nhận: M01-T049; M01-T027-A).
- `M01-LS-F03`: Triển khai API tra cứu & ngắt phiên từ xa & SecurityEpoch Invalidation SLA $\le 5\text{s}$ (tiếp nhận: M01-T049).
- `M01-LS-F04`: Thiết lập bộ kiểm thử tự động LS-G01–G10 và LS18-01–20 (tiếp nhận: M01 tasks).
- `M01-LS-F05`: Thu thập bằng chứng runtime cho luồng đăng xuất M01 (tiếp nhận: M01 tasks; A-G01).

## 8. Tự kiểm M01-T018

- Đã thiết kế hoàn chỉnh `M01-LOGOUT-SESSION-MANAGEMENT-1.0` với 3 Giao thức Đăng xuất Thống nhất.
- Đã chốt Ràng buộc Gia tăng SecurityEpoch Tức thì SLA $\le 5$ giây cho `LogoutAllDevices`.
- Đã chốt Ràng buộc Tự động Thu hồi Push Device Token M10 (`PushDeviceRevokedIntegrationEvent`).
- Đã lồng ghép API Tra cứu Active Sessions, Ngắt phiên từ xa và Lưu vết Audit Log M11 (`ACT-M11-18`).
- Đã xác lập 10 Regression Gates (`LS-G01`–`LS-G10`) và 20 Test Cases tự kiểm (`LS18-01`–`LS18-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế đăng xuất và quản lý phiên M01-T018 | WSA-7K2 |
