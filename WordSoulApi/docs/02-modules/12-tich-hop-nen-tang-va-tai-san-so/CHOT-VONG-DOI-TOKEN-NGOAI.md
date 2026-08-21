# Chốt vòng đời token ngoài M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-EXTERNAL-TOKEN-LIFECYCLE-1.0` |
| Task | M12-T008 |
| Đầu vào | M12-MINIMAL-EXTERNAL-IDENTITY-1.0 (D-110), M12-SECRET-INVENTORY-1.0 (D-069), REL-03 |
| Phạm vi | Đặc tả Giao thức Quản lý Vòng đời Token ngoài (`External Token Lifecycle Protocol`), nguyên tắc không lưu vết Access Token, mã hóa AES-256-GCM cho Refresh Token và cơ chế chủ động gia hạn/thu hồi Token |
| Tự kiểm | A-G01, A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quản lý Vòng đời Token ngoài (`External Token Lifecycle Protocol`) thuộc M12, chuẩn hóa quy trình tiếp nhận, mã hóa, sử dụng, chủ động gia hạn và thu hồi các loại Token nhận được từ các Nhà cung cấp OAuth ngoài (Google, Apple, Facebook), ngăn ngừa rủi ro rò rỉ Token (Token Leakage) và tuân thủ nguyên tắc bảo vệ bí mật REL-03.

- **Nguyên tắc Không Lưu vết Access Token ngoài (`Zero-Persistence Access Token Invariant`)**: OAuth Access Token nhận được từ Provider CHỈ ĐƯỢC DÙNG 1 LẦN trong bộ nhớ tạm (In-Memory Transient Variable) để lấy thông tin hồ sơ OIDC ban đầu. TUYỆT ĐỐI CẤM lưu Access Token vào CSDL đĩa hoặc Redis Cache công khai (REL-03).
- **Mã hóa Bắt buộc Refresh Token bằng AES-256-GCM (`AES-256-GCM Token Encryption`)**: Trong trường hợp tính năng yêu cầu lưu Refresh Token ngoài (ví dụ: Google Calendar/Drive sync), Refresh Token BẮT BUỘC mã hóa bằng thuật toán `AES-256-GCM` trước khi lưu vào CSDL, sử dụng Master Encryption Key quản lý bởi Secret Manager M12-T040 (D-069).
- **Chủ động Gia hạn Token Trước Hết hạn 5 Phút (`Proactive 5m Refresh Window`)**: Hệ thống tự động kích hoạt luồng Refresh Access Token khi thời hạn hiệu lực còn $\le 5$ phút (`TimeRemaining <= 300s`).
- **Thu hồi Tức thì khi Ngắt Liên kết / Xóa Tài khoản (`Instant Remote Token Revocation`)**: Khi người dùng ngắt liên kết tài khoản ngoài (M12-T009) hoặc yêu cầu xóa tài khoản (M01-T035), hệ thống TỰ ĐỘNG phát API Revocation tới Endpoint chính thức của Provider (Google/Apple) để hủy bỏ hoàn toàn Refresh Token ngoài SLA $\le 5$ giây.

## 2. Ma trận Vòng đời Các loại Token ngoài (Token Lifecycle Matrix)

| Loại Token (`TokenType`) | Mục đích Sử dụng | Nơi Lưu trữ (`Storage`) | Thời hạn Hiệu lực (`TTL`) | Quy tắc Mã hóa | Giao thức Thu hồi (`Revocation`) |
|---|---|---|---|---|---|
| `OAuth State Token` | Chống CSRF Callback | Redis In-Memory | 10 phút | Trực tiếp (Random 256-bit) | Atomic DEL sau lần đọc đầu |
| `OAuth Authorization Code` | Đổi Token qua PKCE | In-Memory Transient | 1–5 phút | Không lưu CSDL | 1 Lần đổi duy nhất |
| `External ID Token` | OIDC Claims Ingestion | In-Memory Transient | 60 phút | Kiểm tra Signature RSA | OIDC Nonce Cache 24h |
| `External Access Token` | Gọi API Provider | In-Memory Transient | 60 phút | **KHÔNG LƯU CSDL** | Hủy biến bộ nhớ sau khi gọi |
| `External Refresh Token` | Gia hạn Access Token | CSDL `UserExternalLogins` | 90–365 ngày | **Mã hóa AES-256-GCM** | Gọi API Revoke Provider SLA $\le 5\text{s}$ |

## 3. Kiến trúc Luồng Gia hạn và Thu hồi Token ngoài (Token Lifecycle Engine)

```
[External OAuth Token Ingestion]
               |
               v
 [Extract Minimal OIDC Claims -> Destroy Access Token in Memory]
               |
               v
 [Is Refresh Token Required for Background Sync?]
               |
     +---------+---------+
     | (NO)              | (YES)
     v                   v
 [Do NOT Save Token] [Encrypt Refresh Token with AES-256-GCM]
                         |
                         v
                    [Store Ciphertext in DB]
                         |
                         v
            [Background Proactive Refresh Job]
            - Triggers when TimeRemaining <= 5m
            - Decrypt Ciphertext -> Call Provider Token Endpoint
            - Update New Refresh Token Ciphertext
                         |
                         v
            [User Unlinks Account / Account Deleted]
            - Send API Revoke Request to Google/Apple Endpoint
            - Purge Ciphertext from DB SLA <= 5s
            - Record Audit Event ACT-M11-08
```

## 4. Giao thức Thực thi Mã hóa và Thu hồi CSDL (ExternalTokenLifecycleService)

```csharp
public async Task<bool> ProcessExternalTokenStorageAndRevocationAsync(
    string userId, 
    string provider, 
    string rawRefreshToken, 
    TokenAction action)
{
    if (action == TokenAction.STORE_ENCRYPTED)
    {
        // 1. Encrypt Refresh Token using AES-256-GCM via Secret Manager Key
        byte[] masterKey = await _secretManager.GetMasterEncryptionKeyAsync();
        string encryptedCiphertext = AesGcmEncryptor.Encrypt(rawRefreshToken, masterKey);

        var loginRecord = await _db.UserExternalLogins
            .FirstOrDefaultAsync(l => l.UserId == userId && l.Provider == provider);

        if (loginRecord != null)
        {
            loginRecord.EncryptedRefreshToken = encryptedCiphertext;
            loginRecord.TokenUpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return true;
    }
    else if (action == TokenAction.REVOKE_AND_PURGE)
    {
        // 2. Fetch Ciphertext & Decrypt for Revocation Call
        var loginRecord = await _db.UserExternalLogins
            .FirstOrDefaultAsync(l => l.UserId == userId && l.Provider == provider);

        if (loginRecord != null && !string.IsNullOrEmpty(loginRecord.EncryptedRefreshToken))
        {
            byte[] masterKey = await _secretManager.GetMasterEncryptionKeyAsync();
            string plainRefreshToken = AesGcmEncryptor.Decrypt(loginRecord.EncryptedRefreshToken, masterKey);

            // 3. Call Remote Provider Revoke Endpoint SLA <= 5s
            await _externalOAuthAdapter.RevokeTokenAsync(provider, plainRefreshToken);

            // 4. Purge Ciphertext from DB
            loginRecord.EncryptedRefreshToken = null;
            await _db.SaveChangesAsync();

            // 5. Record Audit Event M11
            await _auditLog.RecordEventAsync("ACT-M11-08", userId, new { Provider = provider, Action = "REVOKED_REMOTE_TOKEN" });
        }
        return true;
    }

    return false;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `ET-G01` | Access Token ngoài tuyệt đối CẤM lưu vết cố định vào CSDL đĩa hoặc Redis Cache (REL-03). |
| `ET-G02` | Refresh Token ngoài bắt buộc phải được mã hóa bằng `AES-256-GCM` trước khi lưu vào CSDL. |
| `ET-G03` | Master Encryption Key dùng để mã hóa Token ngoài phải quản lý qua Secret Manager M12-T040 (D-069). |
| `ET-G04` | Luồng chủ động Refresh Token kích hoạt tự động khi thời gian còn lại $\le 5$ phút (`TimeRemaining <= 300s`). |
| `ET-G05` | Ngắt liên kết hoặc xóa tài khoản tự động phát lệnh Revoke Token tới Provider SLA $\le 5$ giây. |
| `ET-G06` | 100% các lần mã hóa/hủy Refresh Token ngoài được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-08`). |
| `ET-G07` | SLA thực thi mã hóa/giải mã AES-256-GCM In-Memory $< 2\text{ms}$. |
| `ET-G08` | Phân quyền truy cập dịch vụ giải mã Refresh Token chỉ dành riêng cho `SecurityAdmin` và `System Worker`. |
| `ET-G09` | Thất bại khi Revoke Token ngoài phía Provider phải được retry tự động với Exponential Backoff (M12-T037). |
| `ET-G10` | 100% các test case tự kiểm ET08-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AF08-01` | Tiếp nhận OAuth Access Token từ Google | Access Token xử lý xong trong memory, 0 ghi vết CSDL |
| `AF08-02` | Tiếp nhận Refresh Token từ Google | Mã hóa `AES-256-GCM` thành công, lưu ciphertext vào CSDL |
| `AF08-03` | Giải mã Refresh Token bằng Master Key sai | Ném ngoại lệ `CryptographicException`, reject access |
| `AF08-04` | Refresh Token của Google còn 4 phút hết hạn | Proactive Refresh Job tự động gia hạn thành công |
| `AF08-05` | Người học bấm ngắt liên kết tài khoản Google | Gửi API Revoke tới Google SLA $< 3\text{s}$, dọn dẹp ciphertext CSDL |
| `AF08-06` | Quét nội dung bảng `M01_UserExternalLogins` trong CSDL | 100% Refresh Token hiển thị dạng AES Ciphertext, 0 thô |
| `AF08-07` | Tra cứu vết Audit Log M11 sau khi thu hồi Token ngoài | Ghi nhận Audit Event `ACT-M11-08` đính kèm Provider |
| `AF08-08` | Thử mã hóa Refresh Token bằng thuật toán yếu (DES/RC4) | Reject 400 `UNSUPPORTED_CIPHER_ALGORITHM` |
| `AF08-09` | Tải đồng thời 100 thao tác mã hóa/giải mã AES-256-GCM Token | Encryption processing latency p95 $< 1.5\text{ms}$ |
| `AF08-10` | Provider Google trả về Refresh Token mới khi gia hạn | UPSERT ciphertext mã hóa mới vào CSDL |
| `AF08-11` | Revoke Endpoint của Apple bị timeout ngắt kết nối | Retry tự động 3 lần theo Exponential Backoff (D-100) |
| `AF08-12` | Yêu cầu mã hóa Token khi Master Key chưa được nạp | Reject 500 `SECRET_KEY_NOT_INITIALIZED` |
| `AF08-13` | User không phải System Worker thử gọi API giải mã Refresh Token | Deny 403 Forbidden |
| `AF08-14` | User chưa đăng nhập gọi API thu hồi Token ngoài | Deny 401 Unauthorized |
| `AF08-15` | Xóa Master Key khỏi Secret Manager | Toàn bộ luồng giải mã Token ngắt an toàn fail-closed |
| `AF08-16` | Kiểm tra độ dài Initialization Vector (IV) của AES-256-GCM | IV đúng 12 bytes chuẩn hóa cryptographic |
| `AF08-17` | Phân tích tham chiếu cột EncryptedRefreshToken trong CSDL | Quét schema `M01_UserExternalLogins` (T020) |
| `AF08-18` | Refresh Token bị nhà cung cấp ngoài chủ động thu hồi (Invalid Grant) | Đánh dấu cờ `IsRevoked = true` và thông báo tới người học |
| `AF08-19` | Thực hiện xoay khóa Master Encryption Key (Key Rotation 90d) | Tự động mã hóa lại toàn bộ ciphertext theo key mới |
| `AF08-20` | Kiểm thử hoàn tất luồng chốt vòng đời token ngoài M12-EXTERNAL-TOKEN-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-ET-I01` | M12 hiện tại chưa có bộ `ExternalTokenLifecycleService` mã hóa AES-256-GCM | Refresh Token nếu có risk lưu thô vào CSDL | M12-T047-A (Source task) |
| `M12-ET-I02` | Chưa có bộ `ProactiveTokenRefreshJob` gia hạn token trước 5 phút | Risk token hết hạn giữa chừng làm gián đoạn API | M12-T047-A; M11-T038 |
| `M12-ET-I03` | Thiếu luồng gửi API Revoke Token tự động sang Provider ngoài | Refresh Token vẫn còn hiệu lực phía Provider sau khi ngắt liên kết | M12-T047-A; M12-T009 |
| `M12-ET-I04` | Chưa tích hợp Master Encryption Key từ Secret Manager M12-T040 | Dùng hardcoded key mã hóa trong ứng dụng | M12-T047-A; M12-T040 |
| `M12-ET-I05` | chưa có luồng re-encrypt token khi thực hiện Key Rotation 90 ngày | Dữ liệu mã hóa cũ bị hỏng khi master key thay đổi | M12-T047-A; M12-T041 |

- `M12-ET-F01`: Triển khai `ExternalTokenLifecycleService` với mã hóa AES-256-GCM (tiếp nhận: M12-T047-A).
- `M12-ET-F02`: Tích hợp Bắt buộc `ProactiveTokenRefreshJob` 5m (tiếp nhận: M12-T047-A; M11-T038).
- `M12-ET-F03`: Triển khai Remote Token Revocation SLA $\le 5\text{s}$ & Key Rotation re-encrypt (tiếp nhận: M12-T047-A; M12-T009).
- `M12-ET-F04`: Thiết lập bộ kiểm thử tự động ET-G01–G10 và ET08-01–20 (tiếp nhận: M12 tasks).
- `M12-ET-F05`: Thu thập bằng chứng runtime cho luồng token ngoài M12 (tiếp nhận: M12 tasks; A-G01/A-G05).

## 8. Tự kiểm M12-T008

- Đã thiết kế hoàn chỉnh `M12-EXTERNAL-TOKEN-LIFECYCLE-1.0` với Ma trận Vòng đời Các loại Token ngoài.
- Đã chốt Ràng buộc Nguyên tắc Không Lưu vết Access Token ngoài trong CSDL.
- Đã chốt Ràng buộc Mã hóa Bắt buộc Refresh Token bằng `AES-256-GCM` via Secret Manager.
- Đã lồng ghép Gia hạn Token Trước Hết hạn 5 Phút, Thu hồi Tức thì SLA $\le 5\text{s}$ và Log Audit M11 (`ACT-M11-08`).
- Đã xác lập 10 Regression Gates (`ET-G01`–`ET-G10`) và 20 Test Cases tự kiểm (`ET08-01`–`ET08-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt vòng đời token ngoài M12-T008 | WSA-7K2 |
