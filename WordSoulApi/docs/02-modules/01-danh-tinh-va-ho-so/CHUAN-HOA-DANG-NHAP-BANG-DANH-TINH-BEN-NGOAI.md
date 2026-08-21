# Chuẩn hóa đăng nhập bằng danh tính bên ngoài M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-EXTERNAL-LOGIN-1.0` |
| Task | M01-T013 |
| Đầu vào | M01-IDENTITY-1.0 (D-002), M12-MINIMAL-EXTERNAL-IDENTITY-1.0 (D-110), M12-ANTI-FORGERY-REPLAY-1.0 (D-111), M12-EXTERNAL-TOKEN-LIFECYCLE-1.0 (D-112), REL-01, REL-03 |
| Phạm vi | Đặc tả Giao thức Đăng nhập Thống nhất bằng Danh tính ngoài (`External Identity Login Protocol`), 2 nhánh xử lý (Tài khoản hiện có & Khởi tạo tài khoản tự động Auto-Provisioning), quy tắc an toàn CT-02 và cấp phát phiên JWT |
| Tự kiểm | A-G01, A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đăng nhập bằng Danh tính ngoài (`External Identity Login Protocol`) thuộc M01, chuẩn hóa luồng xử lý đăng nhập qua Google OAuth, Apple Sign in with Apple (SIWA) và Facebook OAuth, kết nối với bộ Adapter M12 (D-119) để xác thực người học và cấp phát phiên làm việc an toàn (REL-01, REL-03).

- **Ràng buộc Khởi tạo Tài khoản Tự động An toàn (`Safe Auto-Provisioning Invariant`)**: Khi một danh tính ngoài (`Provider`, `ProviderSub`) chưa từng tồn tại trong hệ thống, M01 TỰ ĐỘNG tạo tài khoản hồ sơ mới (Auto-Provisioning), sinh `UserId` GUID mới, thiết lập `IsEmailVerified = true` nếu OIDC claim `email_verified == true`, và CẤM tự động ghép nối với bất kỳ tài khoản local cũ nào mà chưa được người học xác thực (CT-02, D-110).
- **Ràng buộc Kiểm tra Trạng thái Hồ sơ Trước Cấp phiên (`Account Status Overlay Invariant`)**: Ngay cả khi token ngoài hợp lệ, M01 BẮT BUỘC kiểm tra cờ `AccountStatus` trong CSDL. Nếu tài khoản ở trạng thái `LOCKED` hoặc `INACTIVE`, hệ thống TỪ CHỐI cấp JWT Session và trả về HTTP 403 `ACCOUNT_LOCKED_OR_INACTIVE` (D-027).
- **Nguyên tắc Không Lưu vết Access Token ngoài (`Zero-Persistence Access Token`)**: Access Token ngoài nhận được từ Provider CHỈ ĐƯỢC DÙNG 1 LẦN trong memory để lấy OIDC claims và HỦY BỎ NGHAY LẬP TỨC. Tuyệt đối không lưu Access Token vào CSDL (REL-03, D-112).
- **Lưu vết Sổ Kiểm toán Đăng nhập ngoài M11 (`External Login Audit Trail`)**: $100\%$ các giao dịch đăng nhập ngoài bắt buộc được ghi vết bất biến `ACT-M11-13` trong Sổ Kiểm toán M11, bao gồm `UserId`, `Provider`, `ProviderSub`, `IsNewUser`, `IpAddressMasked` và `UserAgent`.

## 2. Ma trận Luồng Đăng nhập bằng Danh tính ngoài (External Login Flow Matrix)

| Kịch bản (`Scenario`) | Kết quả Tra cứu `UserExternalLogins` | Trạng thái Email Local (CT-02) | Hành vi Hệ thống (`System Action`) | Kết quả Phản hồi API | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `EXISTING_LINKED_USER` | Đã tồn tại `(Provider, ProviderSub)` | N/A | Cấp phát JWT Session Tokens (`AccessToken`, `RefreshToken`) | 200 OK (Auth Success) | `ACT-M11-13-EXISTING` |
| `NEW_EXTERNAL_USER` | Chưa tồn tại | Email chưa được dùng | Tạo User mới, tạo `UserExternalLogins`, Cấp JWT Session | 201 Created (Auth Success) | `ACT-M11-13-NEW` |
| `EMAIL_CONFLICT_CT02` | Chưa tồn tại | Email đã gán cho User local khác | **REJECT auto-link**, yêu cầu đăng nhập local để liên kết | 409 Conflict `LINKING_CONFIRMATION_REQUIRED` | `ACT-M11-13-CONFLICT` |
| `ACCOUNT_LOCKED` | Đã tồn tại | N/A | Refuse session grant | 403 Forbidden `ACCOUNT_LOCKED` | `ACT-M11-13-LOCKED` |

## 3. Kiến trúc Luồng Đăng nhập Danh tính ngoài M01 (External Login Pipeline)

```
[Front-End OAuth Callback (AuthCode, State, CodeVerifier)]
                           |
                           v
        [M12-ExternalLinkUnlink: Validate Anti-Forgery & Exchange Code]
                           |
                           v
        [Extract Minimal Claims: sub, provider, email, email_verified]
                           |
                           v
        [M01-IdentityService: Lookup (Provider, ProviderSub) in DB]
                           |
          +----------------+----------------+
          | (Found Existing User)           | (Not Found: New ProviderSub)
          v                                 v
   [Check AccountStatus]             [Check Email Uniqueness CT-02]
   - Locked -> 403 Forbidden         - Email Exists -> 409 LINKING_CONFIRMATION
   - Active -> Grant Session         - Email Unique -> Auto-Provision New User
                                                         |
                                                         v
                                              [Grant JWT Session Tokens]
                                                         |
                                                         v
                                              [Record Audit Event ACT-M11-13]
```

## 4. Giao thức Thực thi Đăng nhập Danh tính ngoài CSDL (ExternalLoginService)

```csharp
public async Task<AuthTokenResultDto> ProcessExternalLoginAsync(
    string provider, 
    string authCode, 
    string codeVerifier, 
    string stateNonce)
{
    // 1. Exchange Auth Code & Validate Anti-Forgery in M12
    var oidcClaims = await _externalOAuthService.ValidateAndExchangeCodeAsync(provider, authCode, codeVerifier, stateNonce);
    if (oidcClaims == null || !oidcClaims.IsValid)
    {
        throw new SecurityException("INVALID_EXTERNAL_AUTH_CODE");
    }

    // 2. Lookup existing external login in DB
    var existingLogin = await _db.UserExternalLogins
        .Include(l => l.User)
        .FirstOrDefaultAsync(l => l.Provider == provider && l.ProviderSub == oidcClaims.Sub);

    User targetUser;
    bool isNewUser = false;

    if (existingLogin != null)
    {
        // 3. Existing User Branch: Overlay Status Check
        targetUser = existingLogin.User;
        if (targetUser.AccountStatus == AccountStatus.LOCKED || targetUser.AccountStatus == AccountStatus.INACTIVE)
        {
            throw new UnauthorizedAccessException("ACCOUNT_LOCKED_OR_INACTIVE");
        }
    }
    else
    {
        // 4. New External User Branch: CT-02 Guard Verification
        var emailConflictUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == oidcClaims.Email);
        if (emailConflictUser != null)
        {
            throw new InvalidOperationException("LINKING_CONFIRMATION_REQUIRED: Email này đã được đăng ký. Vui lòng đăng nhập tài khoản cũ để thực hiện liên kết.");
        }

        // Auto-Provisioning New User
        isNewUser = true;
        targetUser = new User {
            Id = Guid.NewGuid().ToString("N"),
            Email = oidcClaims.Email,
            DisplayName = oidcClaims.Name ?? $"User_{oidcClaims.Sub[..8]}",
            IsEmailVerified = oidcClaims.IsEmailVerified,
            AccountStatus = AccountStatus.ACTIVE,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(targetUser);
        _db.UserExternalLogins.Add(new UserExternalLogin {
            UserId = targetUser.Id,
            Provider = provider,
            ProviderSub = oidcClaims.Sub,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    // 5. Issue JWT Session Tokens (M01-T016)
    var sessionTokens = await _sessionService.CreateUserSessionAsync(targetUser.Id, SessionClass.PRIMARY);

    // 6. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-13", targetUser.Id, new {
        Provider = provider,
        ProviderSub = oidcClaims.Sub,
        IsNewUser = isNewUser
    });

    return sessionTokens;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `EL-G01` | Đăng nhập ngoài với danh tính chưa tồn tại tự động tạo tài khoản hồ sơ mới (`Auto-Provisioning`). |
| `EL-G02` | Tuyệt đối CẤM tự động ghép nối tài khoản ngoài mới với tài khoản local sẵn có nếu chưa qua xác nhận (CT-02). |
| `EL-G03` | Tài khoản hồ sơ ở trạng thái `LOCKED` hoặc `INACTIVE` tuyệt đối CẤM được cấp phiên JWT (D-027). |
| `EL-G04` | OAuth Access Token ngoài tuyệt đối CẤM lưu vết đĩa CSDL hoặc Redis Cache cố định (REL-03, D-112). |
| `EL-G05` | 100% các giao dịch đăng nhập ngoài thành công hoặc thất bại được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-13`). |
| `EL-G06` | Cấp phát thành công cặp JWT Tokens (`AccessToken`, `RefreshToken`, `SecurityEpoch`) tương thích M01-T016. |
| `EL-G07` | Phân quyền truy cập API đăng nhập ngoài công khai cho toàn bộ người dùng chưa đăng nhập. |
| `EL-G08` | SLA xử lý toàn bộ luồng đăng nhập ngoài từ callback tới cấp JWT Session $< 500\text{ms}$. |
| `EL-G09` | Hỗ trợ 3 nhà cung cấp danh tính ngoài chuẩn hóa: `Google`, `Apple`, `Facebook`. |
| `EL-G10` | 100% các test case tự kiểm EL13-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EL13-01` | Đăng nhập bằng Google với tài khoản đã được liên kết từ trước | Cấp JWT Session Tokens 200 OK, `IsNewUser = false` |
| `EL13-02` | Đăng nhập bằng Google lần đầu với email chưa tồn tại trong CSDL | Auto-Provisioning tài khoản mới, trả về 201 Created |
| `EL13-03` | Đăng nhập bằng Google lần đầu nhưng email trỏ tới User B local cũ | Reject 409 `LINKING_CONFIRMATION_REQUIRED` (CT-02 Guard) |
| `EL13-04` | Đăng nhập bằng Apple với tài khoản đang bị cờ `LOCKED` | Reject 403 Forbidden `ACCOUNT_LOCKED_OR_INACTIVE` |
| `EL13-05` | Gửi request đăng nhập ngoài với Authorization Code bị sai/hết hạn | Reject 400 `INVALID_EXTERNAL_AUTH_CODE` |
| `EL13-06` | Tra cứu vết Audit Log M11 sau khi đăng nhập Google thành công | Ghi nhận Audit Event `ACT-M11-13` đính kèm Provider |
| `EL13-07` | Đăng nhập bằng Apple với tính năng Hide My Email (`privaterelay.appleid.com`) | Auto-Provisioning với email private relay hợp lệ |
| `EL13-08` | Kiểm tra việc lưu vết Access Token ngoài sau khi đăng nhập xong | 0 bản ghi Access Token lưu trong CSDL đĩa |
| `EL13-09` | Tải đồng thời 100 request đăng nhập Google từ 100 người dùng | End-to-End latency p95 $< 450\text{ms}$ |
| `EL13-10` | Google API gặp sự cố timeout khi đăng nhập | Chuyển luồng sang suy giảm M12-T010 (Return 503) |
| `EL13-11` | Đăng nhập bằng Facebook thành công | Cấp JWT Session Tokens 200 OK chuẩn hóa |
| `EL13-12` | Đăng nhập bằng Provider không được hỗ trợ (e.g. Github) | Reject 400 `UNSUPPORTED_EXTERNAL_PROVIDER` |
| `EL13-13` | User đang đăng nhập thử tự gọi API đăng nhập ngoài | Chuyển hướng sang luồng liên kết tài khoản M01-T014 |
| `EL13-14` | Đăng nhập bằng Google với `email_verified == false` | Tạo tài khoản nhưng set `IsEmailVerified = false` |
| `EL13-15` | Đổi tên hiển thị từ OIDC claim `name` khi Auto-Provisioning | Tên hiển thị được tạo chuẩn xác từ OIDC name claim |
| `EL13-16` | Kiểm tra độ trễ cấp phiên JWT sau khi xác thực OIDC thành công | Issuance SLA $< 20\text{ms}$ |
| `EL13-17` | Phân tích tham chiếu các bản ghi `UserExternalLogins` | Quét schema `M01_UserExternalLogins` (T020) |
| `EL13-18` | Thao tác ghi log Audit M11 bị chậm ngắt mạng | Retry tự động theo Outbox Pattern M12-T037 |
| `EL13-19` | Tra cứu lịch sử các lần đăng nhập ngoài của người học | Trả về danh sách provider đã từng dùng để đăng nhập |
| `EL13-20` | Kiểm thử hoàn tất luồng chuẩn hóa đăng nhập ngoài M01-EXTERNAL-LOGIN-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-EL-I01` | M01 hiện tại chưa có bộ `ExternalLoginService` hoàn chỉnh | Chưa hỗ trợ luồng đăng nhập 1 chạm qua Google / Apple | M01-T049 (Source task) |
| `M01-EL-I02` | Chưa cài cờ chặn CT-02 từ chối tự động ghép nối theo email | Risk tài khoản bị xâm nhập do tin tưởng email provider | M01-T049; REL-01 |
| `M01-EL-I03` | Thiếu cờ overlay kiểm tra `AccountStatus` locked/inactive | Tài khoản bị khóa vẫn đăng nhập được nếu dùng Google | M01-T049; M01-T012 |
| `M01-EL-I04` | Thiếu kết nối với luồng suy giảm M12-T010 khi Provider lỗi | App bị treo spinner khi Google API bị gián đoạn | M01-T049; M12-T010 |
| `M01-EL-I05` | Chưa kết nối sự kiện đăng nhập ngoài với Audit Log M11 (`ACT-M11-13`) | Không ghi vết được thời điểm đăng nhập bằng danh tính ngoài | M01-T049; M11-T031 |

- `M01-EL-F01`: Triển khai `ExternalLoginService` với Auto-Provisioning an toàn (tiếp nhận: M01-T049).
- `M01-EL-F02`: Tích hợp Bắt buộc CT-02 Guard & Account Status Overlay (tiếp nhận: M01-T049; REL-01).
- `M01-EL-F03`: Triển khai Zero-Persistence Access Token & Integration M12-T010 (tiếp nhận: M01-T049; M12-T010).
- `M01-EL-F04`: Thiết lập bộ kiểm thử tự động EL-G01–G10 và EL13-01–20 (tiếp nhận: M01 tasks).
- `M01-EL-F05`: Thu thập bằng chứng runtime cho luồng đăng nhập ngoài M01 (tiếp nhận: M01 tasks; A-G01/A-G04).

## 8. Tự kiểm M01-T013

- Đã thiết kế hoàn chỉnh `M01-EXTERNAL-LOGIN-1.0` với Ma trận Luồng Đăng nhập bằng Danh tính ngoài.
- Đã chốt Ràng buộc Khởi tạo Tài khoản Tự động An toàn (`Safe Auto-Provisioning`).
- Đã chốt Ràng buộc Kiểm tra Trạng thái Hồ sơ Trước Cấp phiên (`Account Status Overlay`).
- Đã lồng ghép Nguyên tắc Không Lưu vết Access Token ngoài và Lưu vết Audit Log M11 (`ACT-M11-13`).
- Đã xác lập 10 Regression Gates (`EL-G01`–`EL-G10`) và 20 Test Cases tự kiểm (`EL13-01`–`EL13-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chuẩn hóa đăng nhập bằng danh tính bên ngoài M01-T013 | WSA-7K2 |
