# Xử lý xung đột và gỡ liên kết M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-LINK-UNLINK-CONFLICT-1.0` |
| Task | M01-T015 |
| Đầu vào | M01-LINK-1.0 (D-016), M01-RECOVERY-1.0 (D-030), M12-EXTERNAL-TOKEN-LIFECYCLE-1.0 (D-112), REL-01 |
| Phạm vi | Đặc tả Giao thức Xử lý Xung đột và Gỡ Liên kết Tài khoản Ngoài (`Link/Unlink Conflict & Protection Protocol`), ràng buộc bảo vệ phương thức đăng nhập duy nhất, dọn dẹp Refresh Token ngoài SLA $\le 5$ giây và giải quyết xung đột ghép nối |
| Tự kiểm | A-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Xử lý Xung đột và Gỡ Liên kết Tài khoản ngoài (`Link/Unlink Conflict & Protection Protocol`) thuộc M01, chuẩn hóa cơ chế ngắt liên kết các tài khoản Google / Apple / Facebook khỏi hồ sơ người học, đồng thời thiết lập rào chắn bảo vệ để ngăn ngừa tình trạng mồ côi tài khoản (Orphaned Account), và xử lý các xung đột ghép nối danh tính (REL-01, CT-02).

- **Ràng buộc Bảo vệ Phương thức Đăng nhập Duy nhất (`Last Auth Method Protection Invariant`)**: Hệ thống TUYỆT ĐỐI CẤM người học ngắt liên kết tài khoản ngoài nếu tài khoản đó KHÔNG CÓ mật khẩu local và KHÔNG CÒN bất kỳ tài khoản ngoài nào khác được liên kết. Người học phải thiết lập mật khẩu local (M01-T020) hoặc liên kết thêm 1 nhà cung cấp khác trước khi được gỡ.
- **Quy trình Gỡ Liên kết Tức thì SLA $\le 5\text{s}$ (`Instant Unlink & Remote Revocation`)**: Thao tác ngắt liên kết BẮT BUỘC thực thi trong SLA $\le 5$ giây: Xóa bản ghi trong `UserExternalLogins`, tiêu hủy Refresh Token ciphertext trong CSDL, và kích hoạt API Revoke Token sang nhà cung cấp ngoài M12 (D-112).
- **Phân xử Xung đột Ghép nối Danh tính (`Identity Link Conflict Resolution`)**: Khi người học cố gắng liên kết một tài khoản ngoài `sub_A` đã được liên kết với người dùng khác `User_B`, hệ thống REJECT request với mã lỗi HTTP 409 `EXTERNAL_ACCOUNT_ALREADY_LINKED` kèm mã Ticket M11 để hướng dẫn khiếu nại.
- **Lưu vết Sổ Kiểm toán M11 (`Link/Unlink Audit Trail`)**: $100\%$ các thao tác liên kết / ngắt liên kết tài khoản ngoài được ghi vết bất biến `ACT-M11-15` trong Sổ Kiểm toán M11, bao gồm `UserId`, `Provider`, `ProviderSub`, `Action` (`LINKED`, `UNLINKED`), `IpAddressMasked`.

## 2. Ma trận Xử lý Xung đột và Gỡ Liên kết (Unlink Conflict Matrix)

| Kịch bản (`Scenario`) | Điều kiện Kiểm tra | Kết quả Xử lý (`Behavior`) | Mã lỗi HTTP | SLA Xử lý CSDL | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `UNLINK_SUCCESS` | Có Mật khẩu local HOẶC $>1$ Provider | Xóa liên kết, gọi API Revoke Provider M12 | 200 OK | SLA $\le 5\text{s}$ | `ACT-M11-15-UNLINK` |
| `LAST_AUTH_METHOD_BLOCKED` | KHÔNG Mật khẩu local VÀ Chỉ có 1 Provider | Refuse Request, yêu cầu cài mật khẩu local | 400 Bad Request | SLA $\le 2\text{ms}$ | `ACT-M11-15-BLOCKED` |
| `LINK_CONFLICT` | Target Provider Sub đã gắn với User khác | Refuse Request, hướng dẫn mở Ticket M11 | 409 Conflict | SLA $\le 5\text{ms}$ | `ACT-M11-15-CONFLICT` |
| `PROVIDER_NOT_FOUND` | Provider thử gỡ chưa bao giờ liên kết | Return Error Provider Not Linked | 404 Not Found | SLA $\le 2\text{ms}$ | `ACT-M11-15-NOTFOUND` |

## 3. Kiến trúc Luồng Gỡ Liên kết và Bảo vệ (Unlink Engine Pipeline)

```
[User Requests Unlink Provider (e.g. Google)]
                       |
                       v
         [Check Account Safety Invariants]
         - Has Local Password? (YES / NO)
         - Total Linked External Providers Count? (N)
                       |
         +-------------+-------------+
         | (No Pass & N == 1)        | (Has Pass OR N > 1)
         v                           v
 [REJECT: 400 LAST_AUTH_METHOD]  [Execute Atomic Unlink]
                                 - Remove Row from UserExternalLogins
                                 - Call External Revoke API (M12-T008)
                                 - Evict Session Redis Cache
                                 - Record Audit Event ACT-M11-15
                                 - Return 200 OK
```

## 4. Giao thức Thực thi Gỡ Liên kết CSDL (AccountUnlinkService)

```csharp
public async Task<UnlinkResultDto> UnlinkExternalProviderAsync(string userId, string provider, string currentPasswordReAuth)
{
    var user = await _db.Users
        .Include(u => u.ExternalLogins)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    var targetLogin = user.ExternalLogins.FirstOrDefault(l => l.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
    if (targetLogin == null)
    {
        throw new KeyNotFoundException("PROVIDER_NOT_LINKED: Tài khoản ngoài này chưa được liên kết.");
    }

    // 1. Invariant Check: Last Auth Method Protection Guard
    bool hasLocalPassword = !string.IsNullOrEmpty(user.PasswordHash);
    int externalLoginsCount = user.ExternalLogins.Count;

    if (!hasLocalPassword && externalLoginsCount <= 1)
    {
        throw new InvalidOperationException("LAST_AUTH_METHOD_PROTECTION: Không thể gỡ liên kết phương thức đăng nhập duy nhất. Vui lòng thiết lập mật khẩu local trước.");
    }

    // 2. Remove External Login Record
    _db.UserExternalLogins.Remove(targetLogin);
    await _db.SaveChangesAsync();

    // 3. Trigger Remote Token Revocation in M12 SLA <= 5s
    await _externalTokenLifecycle.ProcessExternalTokenStorageAndRevocationAsync(
        userId, provider, targetLogin.EncryptedRefreshToken, TokenAction.REVOKE_AND_PURGE);

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-15-UNLINK", userId, new {
        Provider = provider,
        ProviderSub = targetLogin.ProviderSub,
        Action = "UNLINKED"
    });

    return new UnlinkResultDto { Success = true, UnlinkedProvider = provider };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `UC-G01` | Tuyệt đối CẤM ngắt liên kết phương thức đăng nhập duy nhất khi tài khoản không có mật khẩu local. |
| `UC-G02` | Thao tác gỡ liên kết thành công gỡ bỏ bản ghi và thu hồi Refresh Token ngoài SLA $\le 5$ giây. |
| `UC-G03` | Liên kết một tài khoản ngoài đã thuộc về người dùng khác trả về lỗi HTTP 409 `EXTERNAL_ACCOUNT_ALREADY_LINKED`. |
| `UC-G04` | 100% các thao tác gỡ liên kết thành công hoặc thất bại được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-15`). |
| `UC-G05` | Ngắt liên kết không làm mất dữ liệu tiến độ học M03, vật phẩm M06 hoặc dữ liệu cá nhân M01 của người học. |
| `UC-G06` | Yêu cầu xác thực lại mật khẩu local hoặc OTP trước khi thực hiện gỡ liên kết tài khoản ngoài. |
| `UC-G07` | Phân quyền thực thi gỡ liên kết chỉ dành cho chính chủ tài khoản hoặc `SecurityAdmin` / `SuperAdmin`. |
| `UC-G08` | SLA xử lý API gỡ liên kết CSDL $< 15\text{ms}$; SLA lệnh thu hồi token ngoài $< 3\text{s}$. |
| `UC-G09` | Hệ thống hỗ trợ liên kết tối đa 5 nhà cung cấp tài khoản ngoài khác nhau cho 1 tài khoản local. |
| `UC-G10` | 100% các test case tự kiểm UC15-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `UC15-01` | Người dùng có mật khẩu local thực hiện gỡ liên kết Google | Gỡ liên kết thành công, xóa bản ghi CSDL và dọn token |
| `UC15-02` | Người dùng KHÔNG có mật khẩu local và CHỈ liên kết Google bấm gỡ Google | Reject 400 `LAST_AUTH_METHOD_PROTECTION` |
| `UC15-03` | Người dùng KHÔNG có mật khẩu local nhưng có CẢ Google và Apple bấm gỡ Google | Gỡ Google thành công (còn duy trì đăng nhập qua Apple) |
| `UC15-04` | Liên kết tài khoản Apple `sub_X` đã được gắn với User B trước đó | Reject 409 `EXTERNAL_ACCOUNT_ALREADY_LINKED` |
| `UC15-05` | Gỡ liên kết một Provider chưa từng được liên kết với tài khoản | Reject 404 `PROVIDER_NOT_LINKED` |
| `UC15-06` | Tra cứu vết Audit Log M11 sau khi gỡ liên kết Google thành công | Ghi nhận Audit Event `ACT-M11-15-UNLINK` đính kèm Provider |
| `UC15-07` | Đăng nhập lại bằng tài khoản Google vừa gỡ liên kết | Tạo tài khoản mới hoặc yêu cầu liên kết lại (không tự động ghép) |
| `UC15-08` | Thử gỡ liên kết khi chưa nhập mật khẩu xác thực lại | Reject 401 `REAUTHENTICATION_REQUIRED` |
| `UC15-09` | Tải đồng thời 50 request gỡ liên kết từ 50 người dùng | Processing latency p95 $< 18\text{ms}$ |
| `UC15-10` | Provider Revoke API ngoài của Google bị timeout khi gỡ liên kết | Gỡ bản ghi CSDL trước, retry lệnh revoke qua Outbox M12 |
| `UC15-11` | Người dùng liên kết cùng lúc 5 nhà cung cấp (Google, Apple, FB, Github, MS) | Liên kết thành công 5 provider |
| `UC15-12` | Thử liên kết nhà cung cấp thứ 6 ($> 5$) | Reject 400 `MAX_EXTERNAL_PROVIDERS_REACHED` |
| `UC15-13` | User không phải Admin thử gọi API gỡ liên kết tài khoản của người khác | Deny 403 Forbidden |
| `UC15-14` | User chưa đăng nhập gọi API gỡ liên kết tài khoản | Deny 401 Unauthorized |
| `UC15-15` | Người dùng thiết lập mật khẩu local thành công rồi tiến hành gỡ Google | Gỡ liên kết Google thành công |
| `UC15-16` | Kiểm tra độ trễ dọn dẹp Ciphertext Refresh Token trong CSDL | Deletion SLA $< 100\text{ms}$ |
| `UC15-17` | Phân tích tham chiếu danh sách `UserExternalLogins` trong CSDL | Quét schema `M01_UserExternalLogins` (T020) |
| `UC15-18` | Ngắt liên kết Google làm thu hồi tức thì phiên PUSH notification M10 | Phát sự kiện `PushDeviceRevokedIntegrationEvent` sang M10 |
| `UC15-19` | Tra cứu danh sách các provider đang liên kết của tài khoản | Trả về DTO chứa danh sách Provider và ngày liên kết |
| `UC15-20` | Kiểm thử hoàn tất luồng xử lý xung đột và gỡ liên kết M01-LINK-UNLINK-CONFLICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-UC-I01` | M01 hiện tại chưa có bộ `AccountUnlinkService` kiểm tra phương thức đăng nhập cuối | Risk người dùng ngắt Google xong không còn cách nào đăng nhập vào tài khoản | M01-T049 (Source task) |
| `M01-UC-I02` | Chưa cài cờ chặn HTTP 409 khi tài khoản ngoài đã thuộc về người dùng khác | Risk bị ghi đè ghép nhầm tài khoản giữa 2 người dùng | M01-T049; REL-01 |
| `M01-UC-I03` | Thiếu kết nối luồng gỡ liên kết với API Revoke Remote Token M12 | Refresh Token vẫn còn hiệu lực trên Provider ngoài sau khi gỡ | M01-T049; M12-T008 |
| `M01-UC-I04` | Thiếu cờ xác thực lại mật khẩu trước khi cho phép gỡ liên kết | Kẻ xấu cầm máy có thể tự ý gỡ liên kết tài khoản ngoài | M01-T049; M01-T020 |
| `M01-UC-I05` | Chưa kết nối sự kiện gỡ liên kết với Audit Log M11 (`ACT-M11-15`) | Không ghi vết được thời điểm và địa chỉ IP gỡ liên kết | M01-T049; M11-T031 |

- `M01-UC-F01`: Triển khai `AccountUnlinkService` với Ràng buộc Last Auth Method Protection (tiếp nhận: M01-T049).
- `M01-UC-F02`: Tích hợp Bắt buộc Identity Link Conflict Guard HTTP 409 (tiếp nhận: M01-T049; REL-01).
- `M01-UC-F03`: Triển khai Remote Token Revocation M12 SLA $\le 5\text{s}$ & Re-auth Check (tiếp nhận: M01-T049; M12-T008).
- `M01-UC-F04`: Thiết lập bộ kiểm thử tự động UC-G01–G10 và UC15-01–20 (tiếp nhận: M01 tasks).
- `M01-UC-F05`: Thu thập bằng chứng runtime cho luồng gỡ liên kết M01 (tiếp nhận: M01 tasks; A-G01).

## 8. Tự kiểm M01-T015

- Đã thiết kế hoàn chỉnh `M01-LINK-UNLINK-CONFLICT-1.0` với Ma trận Xử lý Xung đột và Gỡ Liên kết.
- Đã chốt Ràng buộc Bảo vệ Phương thức Đăng nhập Duy nhất (`Last Auth Method Protection`).
- Đã chốt Quy trình Gỡ Liên kết Tức thì SLA $\le 5$ giây và thu hồi Token ngoài M12.
- Đã lồng ghép Phân xử Xung đột Ghép nối Danh tính HTTP 409 và Lưu vết Audit Log M11 (`ACT-M11-15`).
- Đã xác lập 10 Regression Gates (`UC-G01`–`UC-G10`) và 20 Test Cases tự kiểm (`UC15-01`–`UC15-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả xử lý xung đột và gỡ liên kết M01-T015 | WSA-7K2 |
