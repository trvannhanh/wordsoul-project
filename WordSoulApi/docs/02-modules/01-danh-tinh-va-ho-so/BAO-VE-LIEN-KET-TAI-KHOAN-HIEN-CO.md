# Bảo vệ liên kết tài khoản hiện có M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ACCOUNT-LINKING-PROTECTION-1.0` |
| Task | M01-T014 |
| Đầu vào | M01-EXTERNAL-LOGIN-1.0 (D-121), M01-LINK-UNLINK-CONFLICT-1.0 (D-118), REL-01, REL-03 |
| Phạm vi | Đặc tả Giao thức Bảo vệ Liên kết Tài khoản ngoài với Hồ sơ Hiện có (`Account Linking & Anti-Hijacking Protection Protocol`), yêu cầu xác thực lại $\le 5$ phút, quy trình xác minh qua Email OTP khi ghép nối tài khoản, giới hạn 5 Provider |
| Tự kiểm | A-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Bảo vệ Liên kết Tài khoản ngoài (`Account Linking & Anti-Hijacking Protection Protocol`) thuộc M01, chuẩn hóa cơ chế chủ động liên kết thêm tài khoản ngoài (Google, Apple, Facebook) vào một hồ sơ người học sẵn có, ngăn ngừa các cuộc tấn công chiếm đoạt tài khoản (Account Hijacking) thông qua việc mạo danh danh tính ngoài (REL-01, CT-02).

- **Ràng buộc Phiên Đăng nhập Active & Xác thực lại $\le 5\text{m}$ (`Re-Authentication Guard`)**: Thao tác liên kết tài khoản ngoài BẮT BUỘC thực hiện từ một phiên JWT active hợp lệ. Người học BẮT BUỘC phải thực hiện xác thực lại mật khẩu local hoặc OTP trong vòng 5 phút gần nhất (`ReAuthMinutes <= 5m`) trước khi hệ thống chấp nhận mã Authorization Code liên kết.
- **Ràng buộc Chống Chiếm đoạt qua Email Trùng khớp (`Anti-Hijacking Email OTP Verification`)**: Khi người học chủ động liên kết một tài khoản ngoài có email TRÙNG KHỚP với email local hiện tại, hệ thống BẮT BUỘC phát sinh mã Email OTP 6 chữ số gửi về hòm thư local. Liên kết CHỈ THÀNH CÔNG khi người học xác thực thành công mã OTP này.
- **Giới hạn Tối đa 5 Nhà cung cấp Liên kết (`Max 5 Linked Providers Invariant`)**: Mỗi tài khoản hồ sơ local chỉ được liên kết tối đa 5 nhà cung cấp danh tính ngoài khác nhau. Yêu cầu liên kết nhà cung cấp thứ 6 bị REJECT ngay tại middleware với HTTP 400 `MAX_LINKED_PROVIDERS_EXCEEDED`.
- **Lưu vết Sổ Kiểm toán M11 (`Account Linking Audit Trail`)**: $100\%$ các giao dịch liên kết tài khoản ngoài thành công hoặc thất bại được ghi vết bất biến `ACT-M11-14-LINK` trong Sổ Kiểm toán M11, bao gồm `UserId`, `Provider`, `ProviderSub`, `LinkedEmail`, `ReAuthMethod` và `IpAddressMasked`.

## 2. Ma trận Giao thức Bảo vệ Liên kết Tài khoản (Linking Matrix)

| Kịch bản Liên kết (`Scenario`) | Trạng thái Re-Auth | Trạng thái Email Provider | Hành vi Hệ thống (`System Action`) | Kết quả Phản hồi API | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `DIRECT_LINK_SUCCESS` | Verified $\le 5\text{m}$ | Email mới / Trùng đã OTP | Lưu `UserExternalLogins`, mã hóa Refresh Token M12 | 200 OK (Link Success) | `ACT-M11-14-LINK` |
| `REAUTH_EXPIRED` | Quá 5 phút | N/A | Refuse Link Request | 401 Unauthorized `REAUTH_REQUIRED` | `ACT-M11-14-REAUTH-FAIL` |
| `EMAIL_MATCH_OTP_REQUIRED` | Verified $\le 5\text{m}$ | Trùng email local (Chưa OTP) | Gửi 6-digit OTP qua M10, chờ xác thực | 202 Accepted `OTP_REQUIRED` | `ACT-M11-14-OTP-SENT` |
| `SUB_ALREADY_LINKED` | Verified $\le 5\text{m}$ | N/A | ProviderSub đã thuộc về User khác | 409 Conflict `PROVIDER_ALREADY_LINKED` | `ACT-M11-14-CONFLICT` |
| `MAX_LIMIT_REACHED` | Verified $\le 5\text{m}$ | N/A | Đã có 5 Providers | 400 Bad Request `MAX_PROVIDERS_EXCEEDED` | `ACT-M11-14-LIMIT` |

## 3. Kiến trúc Luồng Bảo vệ Liên kết Tài khoản (Linking Engine Pipeline)

```
[User Initiates Link Provider (e.g. Apple) from Active Session]
                              |
                              v
             [Validate Re-Auth Timestamp (<= 5m?)]
                              |
         +--------------------+--------------------+
         | (Expired > 5m)                          | (Valid <= 5m)
         v                                         v
 [REJECT: 401 REAUTH_REQUIRED]             [Check Max Providers Count (< 5?)]
                                                   |
                                  +----------------+----------------+
                                  | (Count >= 5)                    | (Count < 5)
                                  v                                 v
                          [400 MAX_EXCEEDED]               [Check Sub Uniqueness DB]
                                                                    |
                                                   +----------------+----------------+
                                                   | (Sub Conflict)                  | (Sub Unique)
                                                   v                                 v
                                           [409 CONFLICT]                  [Check Email OTP Requirement]
                                                                                     |
                                                                   +-----------------+-----------------+
                                                                   | (Email Matches & No OTP)          | (OTP Passed / No Match)
                                                                   v                                   v
                                                           [202 OTP_REQUIRED]                [Save UserExternalLogin Row]
                                                           - Send OTP via M10                - Encrypt Refresh Token M12
                                                                                             - Record Audit ACT-M11-14
```

## 4. Giao thức Thực thi Liên kết Tài khoản CSDL (AccountLinkingService)

```csharp
public async Task<LinkResultDto> LinkExternalAccountAsync(
    string userId, 
    string provider, 
    string authCode, 
    string codeVerifier, 
    string stateNonce, 
    string otpCode = null)
{
    var user = await _db.Users
        .Include(u => u.ExternalLogins)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    // 1. Re-Auth Guard <= 5m
    if (user.LastReAuthenticatedAtUtc == null || (DateTime.UtcNow - user.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi liên kết tài khoản.");
    }

    // 2. Max Providers Limit Guard (Max 5)
    if (user.ExternalLogins.Count >= 5)
    {
        throw new InvalidOperationException("MAX_PROVIDERS_EXCEEDED: Mỗi tài khoản chỉ được liên kết tối đa 5 nhà cung cấp ngoài.");
    }

    // 3. Exchange Code & Extract Claims in M12
    var oidcClaims = await _externalOAuthService.ValidateAndExchangeCodeAsync(provider, authCode, codeVerifier, stateNonce);
    
    // 4. Sub Uniqueness Guard
    bool subExists = await _db.UserExternalLogins.AnyAsync(l => l.Provider == provider && l.ProviderSub == oidcClaims.Sub);
    if (subExists)
    {
        throw new InvalidOperationException("PROVIDER_ALREADY_LINKED: Tài khoản ngoài này đã được liên kết với một người dùng khác.");
    }

    // 5. Anti-Hijacking Email OTP Verification Guard
    if (oidcClaims.Email != null && oidcClaims.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrEmpty(otpCode))
        {
            // Trigger 6-digit OTP via M10
            await _otpService.SendLinkingOtpAsync(userId, user.Email);
            return new LinkResultDto { Status = "OTP_REQUIRED", Message = "Mã OTP xác thực đã được gửi tới email của bạn." };
        }

        bool isOtpValid = await _otpService.VerifyLinkingOtpAsync(userId, otpCode);
        if (!isOtpValid) throw new ArgumentException("INVALID_OTP_CODE");
    }

    // 6. Save Link Record & Encrypt Refresh Token M12
    var newLogin = new UserExternalLogin {
        UserId = userId,
        Provider = provider,
        ProviderSub = oidcClaims.Sub,
        CreatedAtUtc = DateTime.UtcNow
    };

    _db.UserExternalLogins.Add(newLogin);
    await _db.SaveChangesAsync();

    if (!string.IsNullOrEmpty(oidcClaims.RefreshToken))
    {
        await _externalTokenLifecycle.ProcessExternalTokenStorageAndRevocationAsync(
            userId, provider, oidcClaims.RefreshToken, TokenAction.STORE_ENCRYPTED);
    }

    // 7. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-14-LINK", userId, new {
        Provider = provider,
        ProviderSub = oidcClaims.Sub,
        LinkedEmail = oidcClaims.Email
    });

    return new LinkResultDto { Status = "SUCCESS", LinkedProvider = provider };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LP-G01` | Liên kết tài khoản ngoài BẮT BUỘC thực hiện từ phiên JWT active và xác thực lại $\le 5$ phút (`ReAuthMinutes <= 5m`). |
| `LP-G02` | Mỗi tài khoản hồ sơ local chỉ được phép liên kết tối đa 5 nhà cung cấp ngoài khác nhau (`Max = 5`). |
| `LP-G03` | Trùng email local đòi hỏi người học BẮT BUỘC xác thực thành công mã Email OTP 6 chữ số trước khi liên kết. |
| `LP-G04` | Liên kết một tài khoản ngoài đã thuộc về người dùng khác trả về lỗi HTTP 409 `PROVIDER_ALREADY_LINKED`. |
| `LP-G05` | Refresh Token ngoài nếu nhận được BẮT BUỘC được mã hóa bằng `AES-256-GCM` via Secret Manager (D-069, D-112). |
| `LP-G06` | 100% các giao dịch liên kết thành công hoặc thất bại được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-14-LINK`). |
| `LP-G07` | Phân quyền thực thi liên kết tài khoản ngoài chỉ dành cho chính chủ tài khoản từ phiên authenticated. |
| `LP-G08` | SLA thực thi API liên kết CSDL $< 25\text{ms}$; SLA mã hóa token ngoài $< 2\text{ms}$. |
| `LP-G09` | Hỗ trợ liên kết đồng thời nhiều provider (Google, Apple, Facebook) trên cùng 1 tài khoản local. |
| `LP-G10` | 100% các test case tự kiểm LP14-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LP14-01` | Người dùng đã xác thực lại cách đây 2 phút thực hiện liên kết Google thành công | Liên kết thành công, ghi CSDL và mã hóa token |
| `LP14-02` | Người dùng thử liên kết Google khi lần xác thực lại cuối là 7 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `LP14-03` | Người dùng đã liên kết 5 Provider bấm liên kết thêm Provider thứ 6 | Reject 400 `MAX_PROVIDERS_EXCEEDED` |
| `LP14-04` | Liên kết tài khoản Apple có email trùng khớp với email local hiện tại | Trả về 202 `OTP_REQUIRED`, gửi OTP qua M10 |
| `LP14-05` | Nhập mã OTP 6 chữ số chính xác sau khi nhận được yêu cầu xác thực | Liên kết Apple hoàn tất 200 OK |
| `LP14-06` | Nhập sai mã OTP 3 lần liên tiếp | Reject 400 `INVALID_OTP_CODE` |
| `LP14-07` | Liên kết ProviderSub `sub_Y` đã thuộc về người dùng B | Reject 409 `PROVIDER_ALREADY_LINKED` |
| `LP14-08` | Tra cứu vết Audit Log M11 sau khi liên kết Google thành công | Ghi nhận Audit Event `ACT-M11-14-LINK` đính kèm Provider |
| `LP14-09` | Tải đồng thời 50 request liên kết từ 50 người dùng | Processing latency p95 $< 22\text{ms}$ |
| `LP14-10` | OAuth Provider trả về Refresh Token mới khi liên kết | Mã hóa Refresh Token bằng AES-256-GCM lưu vào CSDL |
| `LP14-11` | Thử liên kết lại chính Provider đã được liên kết từ trước trên tài khoản này | Reject 400 `PROVIDER_ALREADY_LINKED_SELF` |
| `LP14-12` | Gửi request liên kết khi JWT Access Token đã bị thu hồi | Reject 401 Unauthorized |
| `LP14-13` | User A thử gọi API liên kết tài khoản cho User B | Deny 403 Forbidden |
| `LP14-14` | User chưa đăng nhập gọi API liên kết tài khoản | Deny 401 Unauthorized |
| `LP14-15` | Người dùng thực hiện xác thực lại mật khẩu local thành công rồi bấm liên kết lại | Cho phép liên kết trơn tru |
| `LP14-16` | Kiểm tra thời gian vô hiệu mã OTP liên kết sau khi gửi | OTP TTL đúng mốc 10 phút |
| `LP14-17` | Phân tích tham chiếu các liên kết tài khoản ngoài trong CSDL | Quét schema `M01_UserExternalLogins` (T020) |
| `LP14-18` | Thao tác gửi Email OTP qua M10 bị gián đoạn do ngắt kết nối | Retry tự động theo Outbox Pattern M12-T037 |
| `LP14-19` | Tra cứu danh sách các tài khoản ngoài đã liên kết trong trang Hồ sơ | Trả về danh sách Provider và ngày gắn |
| `LP14-20` | Kiểm thử hoàn tất luồng bảo vệ liên kết tài khoản M01-ACCOUNT-LINKING-PROTECTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-LP-I01` | M01 hiện tại chưa có bộ `AccountLinkingService` bảo vệ luồng liên kết | Người dùng bị phơi nhiễm nguy cơ Account Hijacking | M01-T049 (Source task) |
| `M01-LP-I02` | Thiếu cờ Re-Authentication Guard $\le 5$ phút trước khi cho phép liên kết | Kẻ xấu mượn máy có thể tự ý gán tài khoản Google của chúng vào | M01-T049; M01-T020 |
| `M01-LP-I03` | Thiếu luồng phát mã Email OTP khi email tài khoản ngoài trùng email local | Risk bị chiếm đoạt tài khoản thông qua mạo danh email provider | M01-T049; REL-01 |
| `M01-LP-I04` | Thiếu cờ giới hạn tối đa 5 Provider trên một tài khoản hồ sơ | Rủi ro phình to dữ liệu liên kết rác | M01-T049 |
| `M01-LP-I05` | Chưa kết nối sự kiện liên kết tài khoản với Audit Log M11 (`ACT-M11-14-LINK`) | Không ghi vết được lịch sử gán ghép tài khoản ngoài | M01-T049; M11-T031 |

- `M01-LP-F01`: Triển khai `AccountLinkingService` với Re-Auth Guard $\le 5\text{m}$ (tiếp nhận: M01-T049).
- `M01-LP-F02`: Tích hợp Bắt buộc Anti-Hijacking Email OTP Verification (tiếp nhận: M01-T049; REL-01).
- `M01-LP-F03`: Triển khai Max 5 Providers Guard & Mã hóa AES-256-GCM Token M12 (tiếp nhận: M01-T049; M12-T008).
- `M01-LP-F04`: Thiết lập bộ kiểm thử tự động LP-G01–G10 và LP14-01–20 (tiếp nhận: M01 tasks).
- `M01-LP-F05`: Thu thập bằng chứng runtime cho luồng liên kết M01 (tiếp nhận: M01 tasks; A-G01).

## 8. Tự kiểm M01-T014

- Đã thiết kế hoàn chỉnh `M01-ACCOUNT-LINKING-PROTECTION-1.0` với Ma trận Giao thức Bảo vệ Liên kết Tài khoản.
- Đã chốt Ràng buộc Phiên Đăng nhập Active & Xác thực lại $\le 5$ phút (`ReAuthMinutes <= 5m`).
- Đã chốt Ràng buộc Chống Chiếm đoạt qua Email Trùng khớp (`Anti-Hijacking Email OTP Verification`).
- Đã lồng ghép Giới hạn Tối đa 5 Provider và Lưu vết Audit Log M11 (`ACT-M11-14-LINK`).
- Đã xác lập 10 Regression Gates (`LP-G01`–`LP-G10`) và 20 Test Cases tự kiểm (`LP14-01`–`LP14-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả bảo vệ liên kết tài khoản hiện có M01-T014 | WSA-7K2 |
