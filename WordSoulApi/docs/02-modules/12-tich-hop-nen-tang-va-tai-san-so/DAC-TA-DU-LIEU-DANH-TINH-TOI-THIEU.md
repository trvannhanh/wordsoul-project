# Đặc tả dữ liệu danh tính tối thiểu M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-MINIMAL-EXTERNAL-IDENTITY-1.0` |
| Task | M12-T006 |
| Đầu vào | M12-CONTRACT-1.0 (D-021), M01-PROFILE-MAP-1.0 (D-003), CT-02 (D-008), REL-01, REL-03 |
| Phạm vi | Đặc tả Giao thức Thu thập Dữ liệu Danh tính Tối thiểu Bên ngoài (`Minimal External Identity Protocol`), danh mục 5 thuộc tính được phép, danh mục trường cấm và quy tắc cấm tự động ghép nối theo email CT-02 |
| Tự kiểm | A-G01, A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thu thập Dữ liệu Danh tính Tối thiểu Bên ngoài (`Minimal External Identity Protocol`) thuộc M12, quy định chính xác danh mục thuộc tính thông tin nhận được từ các Nhà cung cấp Danh tính ngoài (Google, Apple, Facebook OAuth 2.0 / OIDC), bảo đảm nguyên tắc thu thập dữ liệu tối thiểu REL-01 và tuân thủ tuyệt đối quy định CT-02 (CẤM tự động ghép nối tài khoản theo email).

- **5 Thuộc tính Danh tính Tối thiểu Được phép (`5 Allowed External Claims Invariant`)**:
  - `ProviderSubjectId` (`sub` claim): Định danh duy nhất bất biến của người dùng tại nhà cung cấp ngoài (Bắt buộc).
  - `ExternalProvider`: Mã nhà cung cấp (`GOOGLE`, `APPLE`, `FACEBOOK`) (Bắt buộc).
  - `Email`: Địa chỉ email đăng ký tại nhà cung cấp ngoài (Không bắt buộc với Apple Hide My Email).
  - `EmailVerified`: Cờ xác minh email từ nhà cung cấp ngoài.
  - `DisplayName`: Tên hiển thị (Tự động lọc ký tự tặc và kiểm duyệt độc hại M01-T023-A).
- **Tuyệt đối CẤM Thu thập Dữ liệu Nhạy cảm Tùy tiện (`Forbidden External Data Invariant`)**: CẤM YÊU CẦU HOẶC LƯU TRỮ: Danh sách bạn bè (`friends_list`), SĐT thô ngoài (`phone_number`), Vị trí địa lý GPS thời gian thực (`location`), Nhật ký bài viết (`posts`) hoặc Quyền truy cập kho ảnh/tệp tin của người dùng (REL-01).
- **Ràng buộc Cấm Tự động Ghép nối Tài khoản theo Email CT-02 (`No Auto-Linking by Email Invariant`)**: Khi người dùng đăng nhập bằng OAuth ngoài với email $E$, nếu CSDL đã tồn tại tài khoản local dùng email $E$, hệ thống TUYỆT ĐỐI CẤM tự động gộp chung tài khoản. Bắt buộc hiển thị màn hình xác nhận liên kết và yêu cầu nhập mật khẩu tài khoản local hiện có (CT-02 / D-008).
- **Chuẩn hóa Mã hóa Luồng OAuth PKCE (`OAuth 2.0 PKCE Invariant`)**: 100% luồng đăng nhập OAuth/OIDC ngoài BẮT BUỘC sử dụng Authorization Code Flow với PKCE (`CodeChallengeMethod = S256`) và cờ `state` chống tấn công CSRF / Replay Attack (M12-T007).

## 2. Cấu trúc Schema Payloads Danh tính Ngoài Chuẩn hóa (ExternalIdentityPayload Schema)

```json
{
  "externalProvider": "GOOGLE",
  "providerSubjectId": "109283746592837465012",
  "email": "user.example@gmail.com",
  "isEmailVerified": true,
  "displayName": "Tran Nhanh",
  "avatarUrl": "https://lh3.googleusercontent.com/a/ACg8oc...",
  "idTokenIssuedAtUtc": "2026-08-21T09:30:00Z",
  "idTokenExpiresAtUtc": "2026-08-21T10:30:00Z"
}
```

## 3. Quy trình Tiếp nhận Danh tính Ngoài và Kiểm soát Liên kết CT-02

```
[User Initiates External Login (Google / Apple)]
                       |
                       v
         [Exchange Auth Code via PKCE (S256)]
                       |
                       v
     [Validate OIDC ID Token & Public Keys]
                       |
                       v
    [Extract 5 Allowed Minimal Claims (sub, email, etc.)]
                       |
                       v
    [Check Existing External Identity Mapping in M01]
                       |
         +-------------+-------------+
         | (Already Mapped)          | (Not Mapped Yet)
         v                           v
   [Issue Session JWT]    [Check If Local Email Exists]
                                     |
                       +-------------+-------------+
                       | (Email Exists in DB)       | (Email New)
                       v                            v
                 [Trigger CT-02 Guard]        [Create New User & Map]
                 - DO NOT Auto-Link
                 - Require Password Verification
```

## 4. Giao thức Thực thi Chuẩn hóa CSDL (ExternalIdentityIngestionService)

```csharp
public async Task<ExternalIdentityIngestionResultDto> IngestExternalIdentityAsync(string provider, string idToken, string codeVerifier)
{
    // 1. Validate & Parse OIDC Token
    var claims = await _oidcValidator.ValidateAndExtractClaimsAsync(provider, idToken);

    var externalId = new ExternalIdentityDto {
        ExternalProvider = provider.ToUpperInvariant(),
        ProviderSubjectId = claims.SubjectId,
        Email = claims.Email,
        IsEmailVerified = claims.IsEmailVerified,
        DisplayName = SanitizeDisplayName(claims.DisplayName)
    };

    // 2. Check Existing Mapping
    var existingMapping = await _db.UserExternalLogins
        .FirstOrDefaultAsync(l => l.Provider == externalId.ExternalProvider && l.ProviderKey == externalId.ProviderSubjectId);

    if (existingMapping != null)
    {
        return new ExternalIdentityIngestionResultDto { Action = IngestionAction.LOGIN_SUCCESS, UserId = existingMapping.UserId };
    }

    // 3. CT-02 Guard: Check if email exists in local accounts
    if (!string.IsNullOrEmpty(externalId.Email))
    {
        var localUserWithSameEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == externalId.Email);
        if (localUserWithSameEmail != null)
        {
            // PROHIBIT AUTO-LINKING (CT-02)
            return new ExternalIdentityIngestionResultDto {
                Action = IngestionAction.LINKING_CONFIRMATION_REQUIRED,
                PendingToken = GenerateLinkingToken(externalId, localUserWithSameEmail.Id),
                Message = "Tài khoản email đã tồn tại. Vui lòng xác thực mật khẩu để liên kết."
            };
        }
    }

    // 4. Create New Account & Map Identity
    var newUserId = await _userOnboardingService.CreateExternalUserAsync(externalId);
    return new ExternalIdentityIngestionResultDto { Action = IngestionAction.ACCOUNT_CREATED, UserId = newUserId };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `EI-G01` | Chỉ thu thập tối đa 5 thuộc tính danh tính tối thiểu (`sub`, `provider`, `email`, `email_verified`, `name`). |
| `EI-G02` | Tuyệt đối CẤM yêu cầu hoặc lưu trữ phone thô, friends list, location GPS hoặc posts từ nhà cung cấp ngoài (REL-01). |
| `EI-G03` | Tuân thủ tuyệt đối quy định CT-02: CẤM tự động ghép nối tài khoản theo email mà không qua xác thực mật khẩu. |
| `EI-G04` | 100% luồng OAuth ngoài bắt buộc dùng Authorization Code Flow với PKCE (`S256`) và kiểm tra `state` nonce. |
| `EI-G05` | Tên hiển thị (`DisplayName`) thu thập từ ngoài phải đi qua bộ lọc làm sạch ký tự độc hại (M01-T023-A). |
| `EI-G06` | Dữ liệu `id_token` từ ngoài phải được xác thực chữ ký số RSA/ECDSA với JWKS Endpoint chính thức của Provider. |
| `EI-G07` | SLA xử lý xác thực và tiếp nhận danh tính ngoài $< 40\text{ms}$ (chưa tính latency mạng của Provider). |
| `EI-G08` | Phân quyền cấu hình OAuth Client Keys chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `EI-G09` | 100% các vụ việc tiếp nhận danh tính ngoài được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-06`). |
| `EI-G10` | 100% các test case tự kiểm EI06-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EI06-01` | Người dùng đăng nhập thành công qua Google OAuth2 OIDC | Trả về 5 claim tối thiểu, tạo tài khoản local thành công |
| `EI06-02` | Đăng nhập Google với email trùng với tài khoản local đã có | Kích hoạt CT-02 Guard, trả về `LINKING_CONFIRMATION_REQUIRED` |
| `EI06-03` | Người dùng nhập đúng mật khẩu local để xác nhận liên kết CT-02 | Liên kết `UserExternalLogins` thành công, phát sự kiện liên kết |
| `EI06-04` | Đăng nhập Apple Sign-In với chế độ `Hide My Email` (`privaterelay.appleid.com`) | Tiếp nhận email ẩn danh bình thường không gây lỗi hệ thống |
| `EI06-05` | Provider ngoài trả về claim `friends_list` hoặc `location` | Bộ lọc DTO tự động loại bỏ (Drop) các claim không được phép |
| `EI06-06` | Thử gửi Auth Code OAuth mà không có tham số PKCE `code_verifier` | Reject 400 `PKCE_VERIFIER_REQUIRED` |
| `EI06-07` | Thử gửi `id_token` giả mạo chữ ký số JWT | Reject 401 `INVALID_OIDC_TOKEN_SIGNATURE` |
| `EI06-08` | Tra cứu vết Audit Log M11 sau khi đăng nhập danh tính ngoài | Ghi nhận Audit Event `ACT-M11-06` đính kèm Provider |
| `EI06-09` | Đăng nhập lại lần 2 bằng Google đã được liên kết trước đó | Đăng nhập thành công tức thì trong SLA $< 20\text{ms}$ |
| `EI06-10` | DisplayName từ Google chứa mã độc Script HTML (`<script>alert(1)</script>`) | Bộ lọc HTML Encoder làm sạch thành `&lt;script&gt;` |
| `EI06-11` | ID Token bị hết hạn (`exp` time trong quá khứ) | Reject 401 `EXPIRED_OIDC_TOKEN` |
| `EI06-12` | Tải đồng thời 100 request tiếp nhận OAuth Token từ Google/Apple | Processing latency p95 $< 35\text{ms}$ |
| `EI06-13` | User không phải Admin thử gọi API sửa OAuth Client ID | Deny 403 Forbidden |
| `EI06-14` | User chưa đăng nhập gọi API callback OAuth không kèm Auth Code | Deny 400 Bad Request |
| `EI06-15` | Đăng nhập với Provider ngoài không nằm trong danh sách hỗ trợ (`GITHUB`) | Reject 400 `UNSUPPORTED_EXTERNAL_PROVIDER` |
| `EI06-16` | Kiểm tra độ trễ xác thực JWKS Public Key từ Cache Redis | Latency $< 2\text{ms}$ |
| `EI06-17` | Phân tích tham chiếu các thuộc tính ngoài trong CSDL | Quét schema `M01_UserExternalLogins` (T020) |
| `EI06-18` | JWKS Endpoint của Google bị ngắt kết nối tạm thời | Fallback dùng Public Key cache trong Redis (TTL 24h) |
| `EI06-19` | Đăng nhập ngoài khi cờ `IsEmailVerified = false` từ Facebook | Yêu cầu người dùng xác minh email trước khi kích hoạt |
| `EI06-20` | Kiểm thử hoàn tất luồng đặc tả dữ liệu danh tính tối thiểu M12-MINIMAL-EXTERNAL-IDENTITY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-EI-I01` | Codebase hiện tại chưa có bộ `ExternalIdentityIngestionService` OIDC chuẩn | Đăng nhập ngoài chưa được lọc 5 claim tối thiểu | M12-T047-A (Source task) |
| `M12-EI-I02` | Chưa cài đặt bộ gác `CT-02 Guard` ngăn tự động ghép tài khoản theo email | Risk tự động ghép lầm tài khoản khi dùng chung email | M12-T047-A; M01-T014 |
| `M12-EI-I03` | Thiếu bộ nhớ đệm JWKS Public Key trong Redis | Mỗi request OAuth phải gọi API ngoài JWKS gây tăng latency | M12-T047-A; M12-T032 |
| `M12-EI-I04` | Chưa bắt buộc Authorization Code Flow với PKCE (`S256`) | Risk bị tấn công Authorization Code Interception trên Mobile | M12-T047-A; M12-T007 |
| `M12-EI-I05` | Chưa xử lý chuẩn cho email ẩn danh Apple `privaterelay.appleid.com` | Có thể gây lỗi validate định dạng email ở M01 | M12-T047-A; M01-T005 |

- `M12-EI-F01`: Triển khai `ExternalIdentityIngestionService` lọc 5 claim tối thiểu (tiếp nhận: M12-T047-A).
- `M12-EI-F02`: Tích hợp Bắt buộc `CT-02 Guard` & OAuth PKCE `S256` (tiếp nhận: M12-T047-A; M01-T014).
- `M12-EI-F03`: Triển khai JWKS Public Key Redis Cache TTL 24h (tiếp nhận: M12-T047-A; M12-T032).
- `M12-EI-F04`: Thiết lập bộ kiểm thử tự động EI-G01–G10 và EI06-01–20 (tiếp nhận: M12 tasks).
- `M12-EI-F05`: Thu thập bằng chứng runtime cho luồng danh tính ngoài M12 (tiếp nhận: M12 tasks; A-G01/A-G05).

## 8. Tự kiểm M12-T006

- Đã thiết kế hoàn chỉnh `M12-MINIMAL-EXTERNAL-IDENTITY-1.0` với 5 Thuộc tính Danh tính Tối thiểu Được phép.
- Đã chốt Ràng buộc Tuyệt đối CẤM Thu thập Dữ liệu Nhạy cảm Tùy tiện (Phone, Friends list, Location GPS, Posts).
- Đã chốt Ràng buộc Cấm Tự động Ghép nối Tài khoản theo Email CT-02 (`LINKING_CONFIRMATION_REQUIRED`).
- Đã lồng ghép Chuẩn hóa OAuth 2.0 PKCE (`S256`) và Lưu vết Audit Log M11 (`ACT-M11-06`).
- Đã xác lập 10 Regression Gates (`EI-G01`–`EI-G10`) và 20 Test Cases tự kiểm (`EI06-01`–`EI06-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả dữ liệu danh tính tối thiểu M12-T006 | WSA-7K2 |
