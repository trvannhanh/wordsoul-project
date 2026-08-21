# Đặc tả liên kết và ngắt liên kết M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-EXTERNAL-LINK-UNLINK-1.0` |
| Task | M12-T009 |
| Đầu vào | M12-EXTERNAL-TOKEN-LIFECYCLE-1.0 (D-112), M01-LINK-UNLINK-CONFLICT-1.0 (D-118), REL-03 |
| Phạm vi | Đặc tả Giao thức Bộ Thích ứng Liên kết và Ngắt Liên kết Danh tính ngoài (`External Provider Integration Adapter Protocol`), chuẩn hóa các Adapter OAuth/OIDC (Google, Apple, Facebook), cơ chế thu hồi Token xa SLA $\le 5$ giây và cô lập lỗi giữa các Nhà cung cấp |
| Tự kiểm | A-G01; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thích ứng Tích hợp OAuth/OIDC ngoài (`External Provider Integration Adapter Protocol`) thuộc M12, chuẩn hóa kiến trúc các bộ thích ứng (Adapters) đối thoại với Google, Apple và Facebook để phục vụ tính năng đăng nhập và liên kết/gỡ liên kết tài khoản ở M01, đồng thời cô lập rủi ro và tuân thủ nguyên tắc bảo vệ bí mật REL-03.

- **Kiến trúc Adapter Cô lập cho từng Nhà cung cấp (`Isolated OAuth Adapter Pattern`)**: Mỗi nhà cung cấp OAuth/OIDC ngoài (Google, Apple, Facebook) BẮT BUỘC được đóng gói trong bộ Adapter riêng biệt triển khai interface `IExternalOAuthAdapter`. Sự cố ngắt mạng hoặc thay đổi API từ 1 nhà cung cấp KHÔNG ĐƯỢC phép ảnh hưởng tới các nhà cung cấp khác (REL-03).
- **Chuẩn hóa Giao thức Thu hồi Token Xa SLA $\le 5\text{s}$ (`Unified Remote Revocation Endpoint`)**: Các Adapter BẮT BUỘC triển khai API thu hồi token xa (`RevokeTokenAsync`) gửi HTTP POST trực tiếp tới Endpoint thu hồi chính thức của Provider (Google `oauth2.googleapis.com/revoke`, Apple `appleid.apple.com/auth/revoke`) khi nhận tín hiệu gỡ liên kết từ M01 (D-118).
- **Ràng buộc Thẩm định Chữ ký Chặt chẽ (`Strict Signature & Claim Validation`)**: 100% ID Tokens nhận được từ Google/Apple BẮT BUỘC phải thẩm định chữ ký số RSA public keys (JWKS), kiểm tra chính xác `iss` (Issuer), `aud` (Audience Client ID), `exp` (Expiration) và `nonce` trước khi trích xuất `sub` (D-110).
- **Nhật ký Đánh dấu Tích hợp Provider M11 (`External Integration Audit Trail`)**: $100\%$ các giao dịch trao đổi mã (Code Exchange), liên kết hoặc thu hồi token ngoài được ghi vết bất biến `ACT-M11-09-ADAPTER` trong Sổ Kiểm toán M11, tuyệt đối không ghi thô Token hay Secret (REL-03).

## 2. Ma trận Bộ Thích ứng Tích hợp Nhà cung cấp ngoài (Provider Adapter Matrix)

| Nhà cung cấp (`Provider`) | Chuẩn Giao thức | JWKS Endpoint / Method | Token Revoke Endpoint | Quy tắc Thẩm định Chữ ký | SLA Trao đổi Token |
|---|---|---|---|---|---|
| `Google` | OAuth 2.0 + OIDC | `www.googleapis.com/oauth2/v3/certs` | `oauth2.googleapis.com/revoke` | `iss == https://accounts.google.com` | SLA $\le 2\text{s}$ |
| `Apple` | Sign in with Apple | `appleid.apple.com/auth/keys` | `appleid.apple.com/auth/revoke` | `iss == https://appleid.apple.com` | SLA $\le 3\text{s}$ |
| `Facebook` | OAuth 2.0 Graph API | `/debug_token` Graph API Call | `/me/permissions` DELETE Call | Validate `app_id == ClientAppId` | SLA $\le 2\text{s}$ |

## 3. Kiến trúc Bộ Thích ứng OAuth/OIDC M12 (OAuth Adapter Engine)

```
[M01 Identity Module Request: Link / Unlink External Account]
                              |
                              v
             [IExternalOAuthAdapter Factory]
                              |
       +----------------------+----------------------+
       | (Provider: Google)   | (Provider: Apple)    | (Provider: Facebook)
       v                      v                      v
[GoogleOAuthAdapter]   [AppleOAuthAdapter]   [FacebookOAuthAdapter]
       |                      |                      |
       +----------------------+----------------------+
                              |
                              v
             [Execute Provider-Specific API Call]
             - Link: Exchange PKCE Code -> Validate JWKS ID Token Signature
             - Unlink: Call Remote Revoke Endpoint SLA <= 5s
                              |
                              v
             [Record Audit Log ACT-M11-09-ADAPTER in M11]
```

## 4. Giao thức Thích ứng Thu hồi Token CSDL (GoogleOAuthAdapter Implementation)

```csharp
public class GoogleOAuthAdapter : IExternalOAuthAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IAuditLogService _auditLog;

    public string ProviderName => "Google";

    public async Task<bool> RevokeTokenAsync(string rawRefreshToken)
    {
        if (string.IsNullOrEmpty(rawRefreshToken)) return true;

        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", rawRefreshToken)
        });

        // 1. Send HTTP POST to Google Revoke Endpoint SLA <= 5s
        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/revoke", requestContent);

        bool isSuccess = response.IsSuccessStatusCode;

        // 2. Record Audit Event M11
        await _auditLog.RecordEventAsync("ACT-M11-09-ADAPTER", "SYSTEM", new {
            Provider = ProviderName,
            Action = "REMOTE_REVOCATION",
            Success = isSuccess,
            StatusCode = (int)response.StatusCode
        });

        return isSuccess;
    }

    public async Task<ExternalOidcClaimsDto> ValidateAndExtractClaimsAsync(string authCode, string codeVerifier, string expectedNonce)
    {
        // Token exchange implementation with PKCE & JWKS signature verification...
        return new ExternalOidcClaimsDto();
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `EL-G01` | Mỗi Provider (Google, Apple, Facebook) phải triển khai bộ Adapter độc lập kế thừa `IExternalOAuthAdapter`. |
| `EL-G02` | Thao tác thu hồi Token từ xa (`RevokeTokenAsync`) hoàn tất cuộc gọi HTTP POST tới Provider SLA $\le 5$ giây. |
| `EL-G03` | 100% ID Tokens từ Google/Apple bắt buộc phải thẩm định chữ ký số RSA public key qua JWKS Endpoint. |
| `EL-G04` | Thất bại khi kết nối API Provider này không được phép ảnh hưởng tới luồng xác thực của Provider khác. |
| `EL-G05` | 100% các cuộc gọi tích hợp Provider được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-09-ADAPTER`). |
| `EL-G06` | Tuyệt đối CẤM ghi vết Access Token, Refresh Token hay Client Secret thô vào log ứng dụng (REL-03). |
| `EL-G07` | Tự động áp dụng Circuit Breaker M12-T038 ngắt kết nối khi Provider API gặp sự cố sập đứt liên kết. |
| `EL-G08` | SLA trao đổi Authorization Code sang Tokens với Google/Apple $< 2.5\text{s}$. |
| `EL-G09` | Phân quyền truy cập dịch vụ Adapter chỉ dành riêng cho `M01-IdentityService` và System Workers. |
| `EL-G10` | 100% các test case tự kiểm EL09-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EL09-01` | Gọi `GoogleOAuthAdapter.RevokeTokenAsync` với Refresh Token hợp lệ | HTTP 200 OK từ Google, trả về `true` |
| `EL09-02` | Gọi `AppleOAuthAdapter.RevokeTokenAsync` với Refresh Token hợp lệ | HTTP 200 OK từ Apple, trả về `true` |
| `EL09-03` | Thẩm định Google ID Token với `iss` sai (`https://fake.issuer.com`) | Reject 400 `INVALID_OIDC_ISSUER` |
| `EL09-04` | Thẩm định Apple ID Token với `aud` sai (`DifferentAppId`) | Reject 400 `INVALID_OIDC_AUDIENCE` |
| `EL09-05` | Endpoint JWKS của Google tạm thời ngắt kết nối | Dùng JWKS Public Keys đã cached trong Redis (TTL 24h) |
| `EL09-06` | Tra cứu vết Audit Log M11 sau khi Revoke Token ngoài | Ghi nhận Audit Event `ACT-M11-09-ADAPTER` đính kèm Provider |
| `EL09-07` | Thử truyền Refresh Token rỗng vào hàm `RevokeTokenAsync` | Bỏ qua cuộc gọi API, trả về `true` trơn tru |
| `EL09-08` | Trao đổi Authorization Code với Google bị hết hạn ($> 5$ phút) | Reject 400 `AUTHORIZATION_CODE_EXPIRED` |
| `EL09-09` | Tải đồng thời 50 yêu cầu thu hồi Token tới Google Revoke Endpoint | Processing latency p95 $< 1.8\text{s}$ |
| `EL09-10` | Provider API trả về lỗi HTTP 503 Service Unavailable | Retry 3 lần theo Exponential Backoff M12-T037 |
| `EL09-11` | Thẩm định chữ ký Apple ID Token bị hết hạn (`exp` đã qua) | Reject 400 `ID_TOKEN_EXPIRED` |
| `EL09-12` | Thử mã hóa JWT client_secret cho Apple bằng key hỏng | Ném ngoại lệ `CryptographicException` |
| `EL09-13` | User không phải System Worker gọi trực tiếp API Adapter | Deny 403 Forbidden |
| `EL09-14` | User chưa đăng nhập gọi API Adapter | Deny 401 Unauthorized |
| `EL09-15` | Thẩm định Nonce OIDC của Google không khớp với Session | Reject 400 `OIDC_NONCE_MISMATCH` |
| `EL09-16` | Kiểm tra độ trễ cuộc gọi Revoke Token từ xa tới Apple | Remote API SLA $< 2.8\text{s}$ |
| `EL09-17` | Phân tích cấu hình các Provider Adapters trong CSDL | Quét schema `M12_ExternalOAuthProviders` (T020) |
| `EL09-18` | Circuit Breaker M12-T038 bật cờ OPEN cho Facebook Adapter | Chuyển sang fallback trả lỗi `PROVIDER_TEMPORARILY_UNAVAILABLE` |
| `EL09-19` | Tra cứu danh sách các Provider Adapters active trong hệ thống | Trả về DTO danh sách Google, Apple, Facebook |
| `EL09-20` | Kiểm thử hoàn tất luồng đặc tả liên kết và ngắt liên kết M12-EXTERNAL-LINK-UNLINK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-EL-I01` | M12 hiện tại chưa có interface `IExternalOAuthAdapter` thống nhất | Code tích hợp OAuth bị phân tán trong các service khác nhau | M12-T047-A (Source task) |
| `M12-EL-I02` | Chưa cài đặt `AppleOAuthAdapter` hỗ trợ Sign in with Apple | Thiếu khả năng liên kết tài khoản Apple theo tiêu chuẩn iOS App Store | M12-T047-A; M01-T014 |
| `M12-EL-I03` | Thiếu bộ nạp đệm `JWKS Cache` Redis 24h cho Google/Apple Public Keys | Hệ thống sập xác thực khi JWKS Endpoint của Google/Apple chập chập | M12-T047-A; M12-T032 |
| `M12-EL-I04` | Chưa tích hợp Circuit Breaker M12-T038 vào các cuộc gọi Provider API | Sự cố ngắt mạng của Facebook làm chậm toàn bộ thread pool hệ thống | M12-T047-A; M12-T038 |
| `M12-EL-I05` | Chưa kết nối sự kiện Revoke Token với Audit Log M11 (`ACT-M11-09-ADAPTER`) | Không ghi log đối soát kết quả thu hồi token từ xa với Provider | M12-T047-A; M11-T031 |

- `M12-EL-F01`: Triển khai `IExternalOAuthAdapter` và 3 Adapters (Google, Apple, Facebook) (tiếp nhận: M12-T047-A).
- `M12-EL-F02`: Tích hợp Bắt buộc Remote Revocation SLA $\le 5\text{s}$ (tiếp nhận: M12-T047-A; M01-T015).
- `M12-EL-F03`: Triển khai JWKS Cache 24h & Circuit Breaker M12-T038 (tiếp nhận: M12-T047-A; M12-T038).
- `M12-EL-F04`: Thiết lập bộ kiểm thử tự động EL-G01–G10 và EL09-01–20 (tiếp nhận: M12 tasks).
- `M12-EL-F05`: Thu thập bằng chứng runtime cho luồng tích hợp Provider M12 (tiếp nhận: M12 tasks; A-G01).

## 8. Tự kiểm M12-T009

- Đã thiết kế hoàn chỉnh `M12-EXTERNAL-LINK-UNLINK-1.0` với Ma trận Bộ Thích ứng Tích hợp Nhà cung cấp ngoài.
- Đã chốt Ràng buộc Kiến trúc Adapter Cô lập cho từng Nhà cung cấp (`IExternalOAuthAdapter`).
- Đã chốt Chuẩn hóa Giao thức Thu hồi Token Xa SLA $\le 5$ giây (`RevokeTokenAsync`).
- Đã lồng ghép Thẩm định Chữ ký Chặt chẽ qua JWKS và Lưu vết Audit Log M11 (`ACT-M11-09-ADAPTER`).
- Đã xác lập 10 Regression Gates (`EL-G01`–`EL-G10`) và 20 Test Cases tự kiểm (`EL09-01`–`EL09-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả liên kết và ngắt liên kết M12-T009 | WSA-7K2 |
