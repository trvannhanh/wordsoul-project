# Thiết kế yêu cầu xóa tài khoản M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ACCOUNT-DELETION-REQUEST-1.0` |
| Task | M01-T035 |
| Đầu vào | M01-CROSS-MODULE-PII-MAP-1.0 (D-102), M01-REVOKE-PUSH-DEVICE-A-1.0 (D-091), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Yêu cầu Xóa Tài khoản và Quyền được Quên (`Account Deletion & Right to be Forgotten Protocol`), thời gian chờ khôi phục 30 ngày (Grace Period), vô hiệu phiên tức thì (`SecurityEpoch += 1`), xác thực kép Password Re-Auth + Email OTP và lưu vết kiểm toán M11 |
| Tự kiểm | A-G01, A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Yêu cầu Xóa Tài khoản và Quyền được Quên (`Account Deletion & Right to be Forgotten Protocol`) thuộc M01, thực thi quy trình xóa tài khoản an toàn tuân thủ GDPR cho người học WordSoul, vừa đảm bảo tính chủ động hủy yêu cầu trong thời gian chờ 30 ngày (Grace Period), vừa ngăn ngừa kẻ xấu lạm dụng xóa trộm tài khoản (REL-01, REL-07).

- **Quyền được Quên GDPR & Thời gian Chờ 30 Ngày (`GDPR Right to be Forgotten & Grace Period`)**: Khi gửi yêu cầu xóa tài khoản, hệ thống KHÔNG xóa dữ liệu ngay lập tức mà đưa tài khoản vào trạng thái chờ 30 ngày (`GracePeriodDays = 30d`, `AccountStatus = PENDING_DELETION`). Trong 30 ngày này, người học có thể đăng nhập hủy yêu cầu xóa để khôi phục tài khoản.
- **Ràng buộc Thu hồi Tức thì Phiên và Thiết bị PUSH (`Instant Session & Push Revocation`)**: Ngay khi lệnh xóa tài khoản được tiếp nhận, hệ thống BẮT BUỘC thực hiện 2 hành động ngắt tức thì: (1) Tăng cờ `SecurityEpoch += 1` vô hiệu hóa $100\%$ Refresh Tokens và JWT Access Tokens (D-091 SLA $\le 5\text{s}$), (2) Hủy đăng ký tất cả các thiết bị PUSH active ở M01-T027-A.
- **Ràng buộc Phê duyệt Kép Password Re-Auth + Email OTP (`Double Verification Guard`)**: Gửi yêu cầu xóa tài khoản BẮT BUỘC phải thông qua 2 lớp bảo vệ: (1) Re-Auth mật khẩu local trong 5 phút (`ReAuthMinutes <= 5m`), và (2) Xác minh mã OTP 6 chữ số gửi qua Email (`EmailOtpVerified = true`).
- **Worker Tiêu hủy & Ẩn danh hóa Dữ liệu Sau 30 Ngày (`AccountDeletionWorker Protocol`)**: Hết thời hạn 30 ngày, worker ngầm `AccountDeletionWorker` (M11-T038) tự động thực hiện ẩn danh hóa PII cá nhân (M01-T036) và tiêu hủy vĩnh viễn dữ liệu định danh, chỉ giữ lại bản ghi băm `UserIdHash` cho mục đích audit bất biến (REL-07).
- **Lưu vết Sổ Kiểm toán Xóa Tài khoản M11 (`Account Deletion Audit Trail`)**: $100\%$ các sự kiện yêu cầu xóa, hủy xóa hoặc tiêu hủy tài khoản vĩnh viễn được ghi vết bất biến `ACT-M01-35-DELETE` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Xóa Tài khoản (Account Deletion Matrix)

| Trạng thái Tài khoản (`AccountStatus`) | Trạng thái Yêu cầu (`DeletionState`) | Khả năng Đăng nhập | Quyền Hủy Lệnh Xóa | Hành vi Tiêu hủy Dữ liệu | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `ACTIVE` | `NONE` | Bình thường | N/A | Dữ liệu nguyên vẹn | N/A |
| **`PENDING_DELETION`** | **`GRACE_PERIOD_ACTIVE`** | **Blocked (Yêu cầu Re-Auth)** | **Cho phép Hủy trong 30d** | **Thu hồi Session / PUSH Device** | `ACT-M01-35-PENDING` |
| `CANCELLED` | `CANCELLED_BY_USER` | Khôi phục `ACTIVE` | N/A | Hủy đếm ngược 30 ngày | `ACT-M01-35-CANCEL` |
| **`DELETED`** | **`PERMANENTLY_ANONYMIZED`** | **Forbidden 403** | **CẤM Khôi phục** | **Xóa PII / Keep Anonymized Hash** | `ACT-M01-35-HARDDELETE` |

## 3. Kiến trúc Luồng Xóa Tài khoản M01 (Account Deletion Engine Pipeline)

```
[Learner Submits Account Deletion Request]
                    |
                    v
 [Double Guard: Verify Password Re-Auth <= 5m AND Email OTP]
                    |
                    v
 [Set AccountStatus: PENDING_DELETION & Set DeletionDeadline = UTC + 30 Days]
 [Increment SecurityEpoch += 1 (Revoke 100% Active Sessions SLA <= 5s)]
 [Revoke All Active Push Devices (M01-T027-A)]
 [Record Audit Log ACT-M01-35-PENDING]
                    |
        +-----------+-----------+
        | (Learner Cancels within 30 Days) | (30 Days Expire without Cancel)
        v                                  v
[Restore AccountStatus: ACTIVE]    [AccountDeletionWorker (M11-T038) Executes]
[Reset SecurityEpoch]              [Anonymize PII Data across M01, M04, M06]
[Record Audit ACT-M01-35-CANCEL]   [Hard Delete Identity Record]
                                   [Set Status: DELETED]
                                   [Record Audit ACT-M01-35-HARDDELETE]
```

## 4. Giao thức Thực thi Xóa Tài khoản CSDL (AccountDeletionService)

```csharp
public async Task<DeletionRequestResultDto> RequestAccountDeletionAsync(
    string userId, 
    string emailOtpCode)
{
    // 1. Password Re-Auth Guard <= 5m
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null || user.LastReAuthenticatedAtUtc == null || 
        (DateTime.UtcNow - user.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi gửi yêu cầu xóa tài khoản.");
    }

    // 2. Email OTP Verification Guard
    bool isOtpValid = await _otpService.VerifyEmailOtpAsync(user.Email, emailOtpCode, "ACCOUNT_DELETION");
    if (!isOtpValid)
    {
        throw new InvalidOperationException("INVALID_EMAIL_OTP: Mã OTP xác minh email không hợp lệ hoặc đã hết hạn.");
    }

    // 3. Set AccountStatus = PENDING_DELETION & Grace Period 30 Days
    user.AccountStatus = AccountStatus.PENDING_DELETION;
    user.DeletionRequestedAtUtc = DateTime.UtcNow;
    user.DeletionDeadlineUtc = DateTime.UtcNow.AddDays(30);

    // 4. Revoke 100% Active Sessions & Push Devices SLA <= 5s (D-091)
    await _securityEpochService.IncrementUserSecurityEpochAsync(userId);
    await _pushDeviceService.RevokeAllUserDevicesAsync(userId, "ACCOUNT_DELETION");

    await _db.SaveChangesAsync();

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M01-35-PENDING", userId, new {
        DeletionDeadlineUtc = user.DeletionDeadlineUtc,
        Action = "ACCOUNT_DELETION_PENDING_GRACE_30D"
    });

    return new DeletionRequestResultDto {
        Status = "PENDING_DELETION",
        GracePeriodDaysRemaining = 30,
        DeletionDeadlineUtc = user.DeletionDeadlineUtc
    };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AD-G01` | Yêu cầu xóa tài khoản BẮT BUỘC đưa tài khoản vào thời gian chờ 30 ngày (`GracePeriodDays = 30d`). |
| `AD-G02` | Tiếp nhận lệnh xóa BẮT BUỘC thu hồi $100\%$ phiên đăng nhập (`SecurityEpoch += 1`) SLA $\le 5$ giây (D-091). |
| `AD-G03` | Yêu cầu xóa tài khoản BẮT BUỘC trải qua 2 lớp xác thực: Re-Auth mật khẩu $\le 5\text{m}$ và Email OTP. |
| `AD-G04` | Trong thời hạn 30 ngày, người học BẮT BUỘC được quyền đăng nhập hủy yêu cầu xóa để khôi phục tài khoản. |
| `AD-G05` | Sau 30 ngày, Worker ngầm BẮT BUỘC ẩn danh hóa PII và xóa vĩnh viễn tài khoản, CẤM khôi phục (REL-07). |
| `AD-G06` | 100% các sự kiện yêu cầu xóa, hủy hoặc xóa vĩnh viễn được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M01-35-DELETE`). |
| `AD-G07` | SLA thực thi thu hồi phiên và thiết bị PUSH khi vừa nhận yêu cầu xóa $< 2.0\text{s}$. |
| `AD-G08` | Phân quyền hủy yêu cầu xóa tài khoản trong 30d chỉ dành cho chính chủ tài khoản đó. |
| `AD-G09` | Dữ liệu kiểm toán lịch sử được duy trì dưới dạng băm `UserIdHash` đảm bảo không mất tính toàn vẹn Sổ M11. |
| `AD-G10` | 100% các test case tự kiểm AD35-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AD35-01` | Người học Re-Auth 2m trước & nhập đúng Email OTP xin xóa tài khoản | `AccountStatus = PENDING_DELETION`, Grace Period 30d |
| `AD35-02` | Người học xin xóa tài khoản nhưng nhập sai mã Email OTP | Reject 400 `INVALID_EMAIL_OTP` |
| `AD35-03` | Người học xin xóa tài khoản khi lần Re-Auth cuối là 8 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `AD35-04` | Người học đăng nhập lại vào ngày thứ 15 của thời gian chờ 30d và chọn "Hủy xóa" | Khôi phục `AccountStatus = ACTIVE`, reset SecurityEpoch |
| `AD35-05` | Worker `AccountDeletionWorker` chạy vào ngày thứ 31 kể từ khi xin xóa | Ẩn danh hóa PII, xóa vĩnh viễn record `M01_Users` |
| `AD35-06` | Tra cứu vết Audit Log M11 sau khi gửi yêu cầu xóa tài khoản | Ghi nhận Audit Event `ACT-M01-35-PENDING` |
| `AD35-07` | Tra cứu vết Audit Log M11 sau khi tài khoản bị xóa vĩnh viễn ngày 31 | Ghi nhận Audit Event `ACT-M01-35-HARDDELETE` |
| `AD35-08` | Thử đăng nhập vào tài khoản đã bị xóa vĩnh viễn ở ngày 31 | Reject 403 `ACCOUNT_DELETED_PERMANENTLY` |
| `AD35-09` | Tải đồng thời 100 request kiểm tra trạng thái thời gian chờ xóa | Response latency p95 $< 8\text{ms}$ |
| `AD35-10` | Kiểm tra thời gian vô hiệu $100\%$ Refresh Tokens khi vừa xin xóa | Revocation SLA $< 1.2\text{s}$ |
| `AD35-11` | Thử hủy yêu cầu xóa của người học khác | Reject 403 `FORBIDDEN_USER_CROSS_ACCESS` |
| `AD35-12` | Gửi request xóa tài khoản khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `AD35-13` | User bị khóa tài khoản `AccountStatus = BANNED` thử gửi yêu cầu xóa | Reject 403 `ACCOUNT_BANNED` |
| `AD35-14` | User chưa đăng nhập gọi API yêu cầu xóa tài khoản | Deny 401 Unauthorized |
| `AD35-15` | Người học hủy xóa tài khoản ở ngày thứ 20, sau đó 5 ngày lại xin xóa lại | Reset lại thời gian chờ 30 ngày mới tính từ đầu |
| `AD35-16` | Kiểm tra độ trễ thu hồi thiết bị nhận tin PUSH ở M01-T027-A | Revocation SLA $< 800\text{ms}$ |
| `AD35-17` | Phân tích tham chiếu các bản ghi `AccountDeletionRequests` trong CSDL | Quét schema `M01_DeletionRequests` (T020) |
| `AD35-18` | Dịch vụ Email OTP bị gián đoạn trong lúc xin xóa tài khoản | Fallback phát mã OTP qua SMS nếu có đăng ký |
| `AD35-19` | Tra cứu danh sách các tài khoản đang ở trạng thái `PENDING_DELETION` | Trả về DTO danh sách PendingDeletions |
| `AD35-20` | Kiểm thử hoàn tất luồng yêu cầu xóa tài khoản M01-ACCOUNT-DELETION-REQUEST-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-AD-I01` | M01 hiện tại chưa có `AccountDeletionService` xử lý GDPR Right to be Forgotten | Không có luồng xóa tài khoản hợp lệ cho người học | M01-T049 (Source task) |
| `M01-AD-I02` | Thiếu cờ Thời gian Chờ 30 Ngày Grace Period & Trang thái PENDING_DELETION | Risk xóa nhầm tài khoản không thể khôi phục | M01-T049; REL-07 |
| `M01-AD-I03` | Thiếu cờ Thu hồi Session & PUSH Device Tức thì SLA $\le 5\text{s}$ | Kẻ xấu vẫn duy trì được phiên cũ sau khi bấm xóa | M01-T049; M01-T018 |
| `M01-AD-I04` | Thiếu Lớp Bảo vệ Kép Password Re-Auth $\le 5\text{m}$ + Email OTP | Kẻ mượn máy có thể tự ý xóa trộm tài khoản người học | M01-AD-F04; REL-01 |
| `M01-AD-I05` | Chưa kết nối sự kiện xóa tài khoản với Audit Log M11 (`ACT-M01-35-DELETE`) | Không ghi vết được lịch sử xóa và ẩn danh hóa tài khoản | M01-T049; M11-T031 |

- `M01-AD-F01`: Triển khai `AccountDeletionService` với GDPR Right to be Forgotten (tiếp nhận: M01-T049).
- `M01-AD-F02`: Tích hợp Bắt buộc 30-Day Grace Period & Instant Session Revocation (tiếp nhận: M01-T049; REL-07).
- `M01-AD-F03`: Triển khai Double Guard Re-Auth $\le 5\text{m}$ + Email OTP Verification (tiếp nhận: M01-T049; REL-01).
- `M01-AD-F04`: Thiết lập bộ kiểm thử tự động AD-G01–G10 và AD35-01–20 (tiếp nhận: M01 tasks).
- `M01-AD-F05`: Thu thập bằng chứng runtime cho luồng xóa tài khoản M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T035

- Đã thiết kế hoàn chỉnh `M01-ACCOUNT-DELETION-REQUEST-1.0` với Ma trận Quy trình Xóa Tài khoản.
- Đã chốt Ràng buộc Quyền được Quên GDPR & Thời gian Chờ 30 Ngày (`GracePeriodDays = 30d`).
- Đã chốt Ràng buộc Thu hồi Tức thì Phiên và Thiết bị PUSH (`SecurityEpoch += 1` D-091 SLA $\le 5\text{s}$).
- Đã lồng ghép Ràng buộc Phê duyệt Kép Password Re-Auth $\le 5\text{m}$ + Email OTP, Worker Tiêu hủy & Ẩn danh hóa Dữ liệu (REL-07) và Audit Log M11 (`ACT-M01-35-DELETE`).
- Đã xác lập 10 Regression Gates (`AD-G01`–`AD-G10`) và 20 Test Cases tự kiểm (`AD35-01`–`AD35-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế yêu cầu xóa tài khoản M01-T035 | WSA-7K2 |
