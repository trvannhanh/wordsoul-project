# Thiết kế chống giả mạo và phát lại M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ANTI-FORGERY-REPLAY-1.0` |
| Task | M12-T007 |
| Đầu vào | M12-MINIMAL-EXTERNAL-IDENTITY-1.0 (D-110), REL-03 |
| Phạm vi | Đặc tả Giao thức Chống Giả mạo và Tấn công Phát lại OAuth (`Anti-Forgery & Replay Protection Protocol`), 4 hàng rào bảo vệ ngẫu nhiên (State Nonce, PKCE S256, OIDC Nonce, Clock Skew), cơ chế hủy 1 lần và ghi log kiểm toán M11 |
| Tự kiểm | A-G01, A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chống Giả mạo và Tấn công Phát lại (`Anti-Forgery & Replay Protection Protocol`) thuộc M12, thiết lập 4 hàng rào bảo vệ an ninh mã hóa cho luồng tích hợp OAuth 2.0 / OIDC với các Nhà cung cấp Danh tính ngoài (Google, Apple, Facebook), ngăn chặn triệt để các cuộc tấn công CSRF, Authorization Code Interception và Replay ID Token.

- **4 Hàng rào Bảo vệ Chống Tấn công OAuth (`4 Anti-Forgery Guards Invariant`)**:
  - *State Nonce Guard*: Tạo mã `state` ngẫu nhiên 256-bit bất biến gán với Session người dùng trong Redis (`TTL = 10` phút). BẮT BUỘC xóa ngay sau lần đọc đầu tiên (Atomic DEL).
  - *PKCE Verifier Guard*: Bắt buộc dùng `code_challenge` (S256 SHA-256) tại Authorization Request và đối soát `code_verifier` thô tại Token Endpoint.
  - *OIDC Nonce Replay Guard*: Mã `nonce` trong OIDC ID Token được lưu vết trong Redis 24h (`wordsoul:oidc_nonce:{nonce}`). Nếu phát hiện `nonce` tái sử dụng $\to$ Từ chối ngay lập tức.
  - *Clock Skew Window Guard*: Xác thực thời gian phát hành `iat` và thời gian hiệu lực `nbf`/`exp` của ID Token nằm trong phạm vi lệch đồng hồ cho phép $\pm 60$ giây.
- **Ràng buộc Hủy Mã 1 Lần Duy nhất (`Single-Use Token/State Invariant`)**: $100\%$ các mã `state`, `code_verifier` và `authorization_code` CHỈ ĐƯỢC PHÉP XỬ LÝ 1 LẦN DUY NHẤT. Mọi nỗ lực tái sử dụng đều bị từ chối với HTTP 400 Bad Request (`REPLAY_ATTACK_DETECTED`).
- **Phát hiện Tấn công và Lưu vết Audit Log M11 (`Security Violation Audit Trail`)**: Ngay khi phát hiện sai lệch `state`, hỏng chữ ký PKCE hoặc nỗ lực phát lại ID Token, hệ thống TỰ ĐỘNG ngắt kết nối và ghi vết bất biến `ACT-M11-07` trong Sổ Kiểm toán M11.

## 2. Kiến trúc 4 Hàng rào Chống Giả mạo và Phát lại (Anti-Forgery Engine)

```
[Client Initiates External Login]
               |
               v
 [Generate Cryptographic State (256-bit) & PKCE Verifier]
 - Store State in Redis (TTL = 10m)
 - Send State & S256 Code Challenge to Identity Provider
               |
               v
 [Provider Callbacks to Callback Endpoint with State & Code]
               |
               v
 +-------------------------------------------------------+
 | 1. State Nonce Guard: Check Redis & Atomic DEL       | -> Mismatch? Reject 400 (CSRF)
 | 2. PKCE Verifier Guard: Hash & Compare with Challenge | -> Mismatch? Reject 400 (Code Stolen)
 | 3. OIDC Nonce Guard: Check Redis 24h Nonce Cache      | -> Found? Reject 401 (Replay Attack)
 | 4. Clock Skew Guard: Verify iat/nbf/exp within +-60s | -> Out of Window? Reject 401 (Stale Token)
 +-------------------------------------------------------+
               |
               v
  [All 4 Guards PASSED -> Issue User Session JWT]
```

## 3. Giao thức Thực thi Chuẩn hóa CSDL (AntiForgeryProtectionService)

```csharp
public async Task<bool> ValidateOAuthCallbackSecurityAsync(
    string state, 
    string codeVerifier, 
    string idToken, 
    string sessionKey)
{
    // 1. Guard 1: State Nonce Guard (Atomic Read & Delete)
    string redisStateKey = $"wordsoul:oauth_state:{sessionKey}";
    string storedState = await _redisDb.StringGetDeleteAsync(redisStateKey);
    if (string.IsNullOrEmpty(storedState) || storedState != state)
    {
        await _auditLog.RecordEventAsync("ACT-M11-07", "SYSTEM", new { Violation = "CSRF_STATE_MISMATCH", StateReceived = state });
        throw new InvalidOperationException("CSRF_ATTACK_DETECTED: Mã state không hợp lệ hoặc đã hết hạn.");
    }

    // 2. Guard 2: PKCE Verifier Guard
    string redisChallengeKey = $"wordsoul:pkce_challenge:{sessionKey}";
    string storedChallenge = await _redisDb.StringGetDeleteAsync(redisChallengeKey);
    string computedChallenge = Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));
    if (storedChallenge != computedChallenge)
    {
        await _auditLog.RecordEventAsync("ACT-M11-07", "SYSTEM", new { Violation = "PKCE_VERIFIER_MISMATCH" });
        throw new InvalidOperationException("PKCE_VERIFIER_INVALID: Mã code verifier không khớp.");
    }

    // 3. Guard 3: OIDC Nonce Replay Guard
    var parsedToken = _jwtHandler.ReadJwtToken(idToken);
    string nonceClaim = parsedToken.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
    if (!string.IsNullOrEmpty(nonceClaim))
    {
        string nonceCacheKey = $"wordsoul:oidc_nonce:{nonceClaim}";
        bool setSuccess = await _redisDb.StringSetAsync(nonceCacheKey, "USED", TimeSpan.FromHours(24), StackExchange.Redis.When.WhenNotExists);
        if (!setSuccess)
        {
            await _auditLog.RecordEventAsync("ACT-M11-07", "SYSTEM", new { Violation = "OIDC_NONCE_REPLAY", Nonce = nonceClaim });
            throw new UnauthorizedAccessException("REPLAY_ATTACK_DETECTED: OIDC ID Token đã được sử dụng trước đó.");
        }
    }

    // 4. Guard 4: Clock Skew Window Guard (+-60s)
    var nowUtc = DateTime.UtcNow;
    if (parsedToken.ValidFrom > nowUtc.AddSeconds(60) || parsedToken.ValidTo < nowUtc.AddSeconds(-60))
    {
        throw new UnauthorizedAccessException("EXPIRED_OR_FUTURE_TOKEN: ID Token vượt quá cửa sổ thời gian cho phép.");
    }

    return true;
}
```

## 4. Regression Gate và Case tự kiểm

### 4.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AF-G01` | 100% request OAuth callback bắt buộc kiểm tra `state` ngẫu nhiên 256-bit chống CSRF. |
| `AF-G02` | Mã `state` và `code_verifier` BẮT BUỘC bị xóa ngay sau lần đọc đầu tiên (Atomic GET & DEL). |
| `AF-G03` | 100% luồng đăng nhập OAuth bắt buộc đối soát `code_challenge` SHA-256 (PKCE S256). |
| `AF-G04` | Mã `nonce` trong OIDC ID Token được ghi nhớ 24h trong Redis để chặn nỗ lực phát lại Token. |
| `AF-G05` | Phạm vi chênh lệch đồng hồ cho phép (`Clock Skew Window`) tối đa $\pm 60$ giây. |
| `AF-G06` | 100% các vụ vi phạm an ninh OAuth được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-07`). |
| `AF-G07` | SLA thực thi bộ kiểm tra 4 hàng rào an ninh $< 5\text{ms}$ (sử dụng Redis In-Memory). |
| `AF-G08` | Phân quyền thay đổi TTL bộ nhớ đệm `state`/`nonce` chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `AF-G09` | Thử tái sử dụng cùng 1 OAuth Auth Code phải bị chối ngắt ngừ lập tức. |
| `AF-G10` | 100% các test case tự kiểm AF07-01–20 đạt thành công trong bộ suite kiểm thử. |

### 4.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AF07-01` | Đăng nhập OAuth hợp lệ với `state`, PKCE `code_verifier` và `nonce` mới | Xác thực 4 hàng rào thành công, cho phép đăng nhập |
| `AF07-02` | Gửi request OAuth Callback với mã `state` bị sai lệch | Reject 400 `CSRF_ATTACK_DETECTED`, ghi log `ACT-M11-07` |
| `AF07-03` | Thử gửi lại Callback lần 2 với cùng một mã `state` | Reject 400 (do state đã bị Atomic DEL ở lần 1) |
| `AF07-04` | Gửi request Token với `code_verifier` không khớp với `code_challenge` S256 | Reject 400 `PKCE_VERIFIER_INVALID` |
| `AF07-05` | Thử phát lại (Replay) một OIDC ID Token đã được dùng 10 phút trước | Reject 401 `REPLAY_ATTACK_DETECTED` (Nonce trùng trong Redis) |
| `AF07-06` | ID Token có mốc `iat` Nhanh hơn đồng hồ server 90 giây ($> 60\text{s}$) | Reject 401 `EXPIRED_OR_FUTURE_TOKEN` |
| `AF07-07` | Tra cứu vết Audit Log M11 sau khi chặn vụ tấn công CSRF | Ghi nhận Audit Event `ACT-M11-07` đính kèm chi tiết vi phạm |
| `AF07-08` | Mã `state` được gửi sau 15 phút ($> 10$ phút TTL) | Redis key đã hết hạn $\to$ Reject 400 `CSRF_ATTACK_DETECTED` |
| `AF07-09` | Tải đồng thời 500 request kiểm tra `state` ngẫu nhiên trong Redis | Nonce evaluation latency p95 $< 3\text{ms}$ |
| `AF07-10` | Đăng nhập qua Apple Sign-In không truyền claim `nonce` | Yêu cầu bắt buộc `nonce` đối với luồng OIDC |
| `AF07-11` | Chèn ký tự null / SQL injection vào tham số `state` | Sanitize & reject 400 `INVALID_STATE_FORMAT` |
| `AF07-12` | Gửi `code_verifier` chứa ký tự cấm (quá ngắn $< 43$ char) | Reject 400 `INVALID_PKCE_VERIFIER_LENGTH` |
| `AF07-13` | User không phải Admin thử gọi API sửa TTL của Nonce Cache | Deny 403 Forbidden |
| `AF07-14` | User chưa đăng nhập gọi API callback với `state` rỗng | Reject 400 Bad Request |
| `AF07-15` | Kiểm tra thời gian lưu trữ `nonce` trong Redis | Expiration TTL đúng 24 giờ kể từ thời điểm phát sinh |
| `AF07-16` | Thao tác ghi Redis Nonce bị gián đoạn do ngắt mạng | Fallback fail-closed từ chối cho phép đăng nhập |
| `AF07-17` | Phân tích tham chiếu các khóa Nonce trong CSDL Redis | Quét namespace `wordsoul:{env}:oidc_nonce` (T020) |
| `AF07-18` | Thử sử dụng phương thức mã hóa PKCE yếu `plain` | Reject 400 `UNSUPPORTED_PKCE_METHOD` (Chỉ chấp nhận S256) |
| `AF07-19` | ID Token có thời gian phát hành hợp lệ nằm trong $\pm 30\text{s}$ | Chấp nhận token (nằm trong phạm vi skew $\pm 60\text{s}$) |
| `AF07-20` | Kiểm thử hoàn tất luồng chống giả mạo và phát lại M12-ANTI-FORGERY-REPLAY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-AF-I01` | Codebase hiện tại chưa có bộ `AntiForgeryProtectionService` | Thiếu bộ kiểm tra 4 hàng rào an ninh OAuth thời gian thực | M12-T047-A (Source task) |
| `M12-AF-I02` | Chưa cài đặt lệnh Atomic `StringGetDeleteAsync` cho mã `state` | Risk mã `state` bị đọc 2 lần nếu có 2 request đồng thời | M12-T047-A; M12-T032 |
| `M12-AF-I03` | Thiếu bộ nhớ đệm `wordsoul:oidc_nonce` 24h trong Redis | ID Token có thể bị phát lại nhiều lần trước khi hết hạn JWT | M12-T047-A; M12-T032 |
| `M12-AF-I04` | Thiếu validation Clock Skew Window $\pm 60\text{s}$ cho ID Token | Token bị lệch đồng hồ giữa server ngoài và server local gây lỗi | M12-T047-A |
| `M12-AF-I05` | Chưa kết loại sự kiện vi phạm an ninh OAuth với Audit Log M11 | Đội vận hành SRE không nhận được tín hiệu khi có tấn công CSRF | M12-T047-A; M11-T031 |

- `M12-AF-F01`: Triển khai `AntiForgeryProtectionService` bảo vệ 4 hàng rào an ninh (tiếp nhận: M12-T047-A).
- `M12-AF-F02`: Tích hợp Bắt buộc Atomic `StringGetDeleteAsync` cho State Nonce (tiếp nhận: M12-T047-A; M12-T032).
- `M12-AF-F03`: Khởi tạo Redis Nonce Cache 24h & Clock Skew Window $\pm 60\text{s}$ (tiếp nhận: M12-T047-A; M12-T032).
- `M12-AF-F04`: Thiết lập bộ kiểm thử tự động AF-G01–G10 và AF07-01–20 (tiếp nhận: M12 tasks).
- `M12-AF-F05`: Thu thập bằng chứng runtime cho luồng chống giả mạo M12 (tiếp nhận: M12 tasks; A-G01/A-G04).

## 8. Tự kiểm M12-T007

- Đã thiết kế hoàn chỉnh `M12-ANTI-FORGERY-REPLAY-1.0` với 4 Hàng rào Bảo vệ Chống Tấn công OAuth.
- Đã chốt Ràng buộc Hủy Mã 1 Lần Duy nhất (`Atomic GET & DEL` cho state và PKCE verifier).
- Đã chốt Ràng buộc Chặn Phát lại OIDC ID Token trong Redis 24h và Clock Skew Window $\pm 60\text{s}$.
- Đã lồng ghép Tự động Phát hiện Tấn công CSRF/Replay và Lưu vết Audit Log M11 (`ACT-M11-07`).
- Đã xác lập 10 Regression Gates (`AF-G01`–`AF-G10`) và 20 Test Cases tự kiểm (`AF07-01`–`AF07-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế chống giả mạo và phát lại M12-T007 | WSA-7K2 |
