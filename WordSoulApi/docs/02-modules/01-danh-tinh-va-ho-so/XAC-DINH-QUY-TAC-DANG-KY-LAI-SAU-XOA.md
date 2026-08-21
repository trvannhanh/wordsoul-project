# Xác định quy tắc đăng ký lại sau xóa M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-RE-REGISTRATION-AFTER-DELETION-1.0` |
| Task | M01-T037 |
| Đầu vào | M01-ANONYMIZATION-DELETION-MATRIX-1.0 (D-149), M01-ACCOUNT-DELETION-REQUEST-1.0 (D-148), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Quy tắc Đăng ký lại Sau Xóa Tài khoản (`Re-Registration After Account Deletion Protocol`), quy định kiểm tra điều kiện email/external identity, nguyên tắc không thừa kế dữ liệu cũ (Zero-History Inheritance) và lưu vết kiểm toán M11 |
| Tự kiểm | A-G01; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quy tắc Đăng ký lại Sau Xóa Tài khoản (`Re-Registration After Account Deletion Protocol`) thuộc M01, xác lập bộ quy tắc ứng xử của hệ thống khi một địa chỉ email hoặc danh tính ngoài (Google OAuth) từng bị xóa trước đó thực hiện đăng ký lại tài khoản mới tại WordSoul (REL-01, REL-07).

- **Quy tắc Kiểm tra Trạng thái Đăng ký lại (`Re-Registration Status Eligibility`)**:
  - *Trong 30 ngày Grace Period (`PENDING_DELETION`)*: Chặn $100\%$ các yêu cầu đăng ký mới bằng Email/External Provider đó với lỗi HTTP 400 `REGISTRATION_BLOCKED_PENDING_DELETION`. Hệ thống cung cấp nút bấm khôi phục tài khoản cũ cho người học.
  - *Sau 30 ngày (Đã xóa vĩnh viễn `DELETED`)*: Email và External Provider hoàn toàn ĐỦ ĐIỀU KIỆN để tạo một tài khoản mới (`NEW_ACCOUNT_PROVISIONED`).
- **Ràng buộc Không Thừa kế Dữ liệu Cũ (`Zero-History Inheritance Invariant`)**: Tài khoản đăng ký lại BẮT BUỘC được khởi tạo như một người học hoàn toàn mới với `UserId` GUID mới ngẫu nhiên. TUYỆT ĐỐI CẤM khôi phục hoặc thừa kế bất kỳ dữ liệu nào từ tài khoản cũ (Hồ sơ, Chuỗi ngày học Streak, Tiến trình SRS M04, Lịch sử giao dịch M06).
- **Phòng chống Spam Đăng ký - Xóa Liên tục (`Email Rate-Limiting Abuse Protection`)**: Hệ thống giới hạn mỗi địa chỉ email chỉ được phép hoàn tất chu kỳ xóa - đăng ký lại tối đa 3 lần trong vòng 1 năm (`MaxReRegistrationsPerYear = 3`). Nếu vượt quá 3 lần, email bị tạm khóa đăng ký mới trong 90 ngày.
- **Lưu vết Sổ Kiểm toán Đăng ký lại M11 (`Re-Registration Audit Trail`)**: $100\%$ các đợt đăng ký lại tài khoản mới bằng Email/External Provider đã từng bị xóa được ghi vết bất biến `ACT-M01-37-REREG` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy định Đăng ký lại Tài khoản (Re-Registration Matrix)

| Trạng thái Tài khoản Cũ (`OldAccountStatus`) | Thời điểm Thử Đăng ký lại | Phản hồi Đăng ký (`Registration Response`) | Cấp `UserId` Mới | Thừa kế Dữ liệu Cũ | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`PENDING_DELETION`** | **Trong 30 ngày Grace Period** | **HTTP 400 `BLOCKED_PENDING_DELETION`** | Không | N/A (Hướng dẫn Cancel Deletion) | `ACT-M01-37-BLOCKED` |
| **`DELETED`** | **Sau 30 ngày (Permanently Anonymized)** | **HTTP 201 `NEW_ACCOUNT_PROVISIONED`** | **Cấp `UserId` GUID Mới** | **CẤM (Zero-History Inheritance)** | `ACT-M01-37-NEWACC` |
| `DELETED` (Lần 4/năm) | Vượt mốc 3 lần/năm | HTTP 429 `RE_REGISTRATION_RATE_LIMITED` | Không | N/A | `ACT-M01-37-RATELIMIT` |

## 3. Kiến trúc Luồng Kiểm tra Đăng ký lại M01 (Re-Registration Engine Pipeline)

```
[Learner Submits Registration Request (Email or External Provider)]
                                 |
                                 v
        [Check Existing Email / Identity State in Database]
                                 |
        +------------------------+------------------------+
        | (Account Exists: PENDING_DELETION)              | (Account Status: DELETED / Not Found)
        v                                                 v
[REJECT: 400 BLOCKED_PENDING_DELETION]         [Check Rate Limit: <= 3 Registrations/Year]
- Prompt Learner to Cancel Deletion                      |
- Record Audit Log ACT-M01-37-BLOCKED          +---------+---------+
                                               | (Over Limit)      | (Within Limit)
                                               v                   v
                                      [REJECT: 429 LIMIT]   [Provision Brand New Account]
                                                            - Assign New UserId GUID
                                                            - Set Zero-History Baseline
                                                            - Record Audit ACT-M01-37-NEWACC
```

## 4. Giao thức Thực thi Đăng ký lại CSDL (ReRegistrationService)

```csharp
public async Task<RegistrationResultDto> EvaluateReRegistrationEligibilityAsync(string email, string providerKey, string externalSub)
{
    // 1. Check if email/sub is currently PENDING_DELETION
    var pendingUser = await _db.Users.FirstOrDefaultAsync(u => 
        (u.Email == email || u.ExternalSub == externalSub) && u.AccountStatus == AccountStatus.PENDING_DELETION);

    if (pendingUser != null)
    {
        throw new InvalidOperationException("REGISTRATION_BLOCKED_PENDING_DELETION: Email hoặc tài khoản này đang trong thời gian chờ xóa 30 ngày. Vui lòng đăng nhập lại để hủy yêu cầu xóa.");
    }

    // 2. Check Yearly Re-Registration Rate Limit (Max 3 / Year)
    string emailHash = ComputeSaltedSha256(email, _config["SystemAuditSalt"]);
    int reRegCountThisYear = await _db.ReRegistrationAudits.CountAsync(r => 
        r.EmailHash == emailHash && r.RegisteredAtUtc > DateTime.UtcNow.AddYears(-1));

    if (reRegCountThisYear >= 3)
    {
        throw new InvalidOperationException("RE_REGISTRATION_RATE_LIMITED: Địa chỉ email này đã vượt quá giới hạn 3 lần đăng ký lại trong vòng 1 năm. Vui lòng thử lại sau.");
    }

    // 3. Provision Brand-New User with NEW UserId GUID (Zero-History Invariant)
    string newUserId = Guid.NewGuid().ToString("N");
    var newUser = new User {
        Id = newUserId,
        Email = email,
        AccountStatus = AccountStatus.ACTIVE,
        CreatedAtUtc = DateTime.UtcNow
        // Zero-History Baseline: No Streak, No old M04 SRS progress, No M06 transactions
    };

    _db.Users.Add(newUser);

    // Save Re-Registration Audit Record
    _db.ReRegistrationAudits.Add(new ReRegistrationAudit {
        AuditId = Guid.NewGuid().ToString("N"),
        NewUserId = newUserId,
        EmailHash = emailHash,
        RegisteredAtUtc = DateTime.UtcNow
    });

    await _db.SaveChangesAsync();

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M01-37-REREG", newUserId, new {
        EmailHash = emailHash,
        Action = "NEW_ACCOUNT_PROVISIONED_ZERO_HISTORY"
    });

    return newUser.ToRegistrationDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RR-G01` | Trong 30 ngày Grace Period (`PENDING_DELETION`), hệ thống BẮT BUỘC chặn $100\%$ yêu cầu đăng ký mới bằng Email/Provider đó. |
| `RR-G02` | Đăng ký lại sau 30 ngày BẮT BUỘC khởi tạo tài khoản hoàn toàn mới với `UserId` GUID độc lập. |
| `RR-G03` | Tài khoản mới BẮT BUỘC tuân thủ nguyên tắc Zero-History (CẤM thừa kế Streak, SRS progress M04, Lịch sử M06). |
| `RR-G04` | Giới hạn số lần đăng ký lại BẮT BUỘC duy trì tối đa 3 lần / 1 năm per email (`MaxReRegistrationsPerYear = 3`). |
| `RR-G05` | Khi bị từ chối do `PENDING_DELETION`, giao diện BẮT BUỘC hiển thị hướng dẫn đăng nhập khôi phục tài khoản cũ. |
| `RR-G06` | 100% các đợt đăng ký lại bằng email đã từng bị xóa được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M01-37-REREG`). |
| `RR-G07` | SLA thực thi kiểm tra điều kiện đủ đăng ký lại trong CSDL SQL $< 5\text{ms}$. |
| `RR-G08` | Phân quyền khởi tạo tài khoản mới tuân thủ đúng quy trình đăng ký chuẩn của Module M01. |
| `RR-G09` | Kiểm tra tính bất biến của bản ghi băm `EmailHash` trong sổ kiểm toán đăng ký lại. |
| `RR-G10` | 100% các test case tự kiểm RR37-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RR37-01` | Thử đăng ký tài khoản mới bằng email đang ở trạng thái `PENDING_DELETION` (Ngày 15) | Reject 400 `REGISTRATION_BLOCKED_PENDING_DELETION` |
| `RR37-02` | Đăng ký tài khoản mới bằng email của tài khoản đã xóa vĩnh viễn ở ngày 35 ($> 30$d) | Khởi tạo tài khoản mới 201 Created, cấp `UserId` GUID mới |
| `RR37-03` | Kiểm tra chuỗi ngày học Streak của tài khoản vừa đăng ký lại ở Case RR37-02 | Streak = 0, Exp = 0 (Zero-History Baseline) |
| `RR37-04` | Kiểm tra tiến trình từ vựng M04 của tài khoản vừa đăng ký lại ở Case RR37-02 | Trống 0 cards (Zero-History Baseline) |
| `RR37-05` | Thử đăng ký lại lần thứ 4 trong vòng 12 tháng bằng cùng 1 địa chỉ email | Reject 429 `RE_REGISTRATION_RATE_LIMITED` |
| `RR37-06` | Tra cứu vết Audit Log M11 sau khi đăng ký lại tài khoản mới thành công | Ghi nhận Audit Event `ACT-M01-37-NEWACC` đính kèm EmailHash |
| `RR37-07` | Tra cứu vết Audit Log M11 khi bị từ chối đăng ký do `PENDING_DELETION` | Ghi nhận Audit Event `ACT-M01-37-BLOCKED` |
| `RR37-08` | Đăng ký lại bằng Google OAuth Provider đã bị xóa vĩnh viễn trước đó | Cấp `UserId` mới, liên kết Google `sub` mới |
| `RR37-09` | Tải đồng thời 50 request đăng ký lại từ 50 email đã bị xóa | Processing latency p95 $< 12\text{ms}$ |
| `RR37-10` | Kiểm tra thời gian hết hạn cờ rate limit đăng ký lại 3 lần/năm | Tự động mở khóa đăng ký sau 365 ngày |
| `RR37-11` | Thử truyền định dạng email không hợp lệ khi kiểm tra đăng ký lại | Reject 400 `INVALID_EMAIL_FORMAT` |
| `RR37-12` | Gửi request đánh giá điều kiện đăng ký lại khi chưa qua kiểm tra captcha | Reject 400 `CAPTCHA_REQUIRED` |
| `RR37-13` | User bị cấm IP thử gửi yêu cầu đăng ký lại | Deny 403 Forbidden |
| `RR37-14` | User chưa đăng nhập gọi API kiểm tra điều kiện đăng ký lại M01 | Cho phép gọi công khai (Public Registration API) |
| `RR37-15` | Đăng ký lại lần thứ 2 sau 18 tháng kể từ lần xóa thứ nhất ($> 12$m) | Cho phép đăng ký thành công (Không tính lần 1 đã quá 1 năm) |
| `RR37-16` | Kiểm tra độ trễ ghi bản ghi `ReRegistrationAudits` vào CSDL | Insert SLA $< 3\text{ms}$ |
| `RR37-17` | Phân tích tham chiếu các bản ghi `ReRegistrationAudits` trong CSDL | Quét schema `M01_ReRegistrationAudits` (T020) |
| `RR37-18` | Dịch vụ kiểm tra băm `EmailHash` bị đứt kết nối CSDL | Fallback tạm dừng đăng ký mới, trả về 503 |
| `RR37-19` | Tra cứu danh sách các địa chỉ email đang bị rate limit đăng ký lại | Trả về DTO danh sách BlockedEmailsForReRegistration |
| `RR37-20` | Kiểm thử hoàn tất luồng quy tắc đăng ký lại sau xóa M01-RE-REGISTRATION-AFTER-DELETION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-RR-I01` | M01 hiện tại chưa có `ReRegistrationService` kiểm tra điều kiện đăng ký lại | Risk tạo tài khoản trùng lặp trong thời gian 30d Grace Period | M01-T049 (Source task) |
| `M01-RR-I02` | Thiếu cờ Bắt buộc Zero-History Inheritance cho tài khoản đăng ký lại | Risk khôi phục nhầm dữ liệu của người dùng cũ | M01-T049; REL-07 |
| `M01-RR-I03` | Thiếu cờ Giới hạn Tần suất Đăng ký lại Max 3 lần/năm per email | Kẻ xấu lạm dụng tạo - xóa tài khoản liên tục gây tốn tài nguyên | M01-T049; REL-01 |
| `M01-RR-I04` | Thiếu luồng hướng dẫn Cancel Deletion khi bị chặn do `PENDING_DELETION` | Trải nghiệm người dùng kém khi không biết cách khôi phục | M01-RR-F04; M01-T035 |
| `M01-RR-I05` | Chưa kết nối sự kiện đăng ký lại với Audit Log M11 (`ACT-M01-37-REREG`) | Không ghi vết được lịch sử tái đăng ký của các email | M01-T049; M11-T031 |

- `M01-RR-F01`: Triểnkai `ReRegistrationService` với Eligibility Evaluation Engine (tiếp nhận: M01-T049).
- `M01-RR-F02`: Tích hợp Bắt buộc Zero-History Inheritance & Grace Period Block (tiếp nhận: M01-T049; REL-07).
- `M01-RR-F03`: Triển khai Rate Limit Max 3 Re-Registrations/Year per Email (tiếp nhận: M01-T049; REL-01).
- `M01-RR-F04`: Thiết lập bộ kiểm thử tự động RR-G01–G10 và RR37-01–20 (tiếp nhận: M01 tasks).
- `M01-RR-F05`: Thu thập bằng chứng runtime cho luồng đăng ký lại M01 (tiếp nhận: M01 tasks; A-G01).

## 8. Tự kiểm M01-T037

- Đã thiết kế hoàn chỉnh `M01-RE-REGISTRATION-AFTER-DELETION-1.0` với Ma trận Quy định Đăng ký lại Tài khoản.
- Đã chốt Ràng buộc Quy tắc Kiểm tra Trạng thái Đăng ký lại (Chặn trong 30d Grace Period, Cho phép sau 30d).
- Đã chốt Ràng buộc Không Thừa kế Dữ liệu Cũ (`Zero-History Inheritance Invariant`).
- Đã lồng ghép Phòng chống Spam Đăng ký - Xóa Liên tục (Max 3 lần/năm per email REL-01, REL-07) và Audit Log M11 (`ACT-M01-37-REREG`).
- Đã xác lập 10 Regression Gates (`RR-G01`–`RR-G10`) và 20 Test Cases tự kiểm (`RR37-01`–`RR37-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả quy tắc đăng ký lại sau xóa M01-T037 | WSA-7K2 |
