# Chuẩn hóa khóa và mở tài khoản M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ACCOUNT-LOCK-UNLOCK-1.0` |
| Task | M01-T031 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M01-SESSION-POLICY-1.0 (D-027), M01-REVOKE-PUSH-DEVICE-A-1.0 (D-091), M11-AUDIT-EVENTS-1.0 (D-054) |
| Phạm vi | Quy trình khóa tự động do sai mật khẩu 5 lần, khóa quản trị vĩnh viễn do Quản trị an ninh (`SecurityAdmin`), giao thức vô hiệu hóa phiên tức thì và mở khóa an toàn |
| Tự kiểm | A-G01, A-G02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chuẩn hóa Khóa và Mở Khóa Tài khoản (`Account Lock & Unlock Engine`) thuộc M01, nhằm ngăn chặn tấn công brute-force mật khẩu, vô hiệu hóa kịp thời các tài khoản vi phạm chính sách và đảm bảo quyền hạn mở khóa được kiểm soát chặt chẽ qua phân quyền RBAC M01-T028.

- **2 Cơ chế Khóa Tài khoản Rõ ràng (`2-Type Account Locking Invariant`)**:
  - *Khóa Tạm thời Tự động (`TEMPORARY_AUTO_LOCK`)*: Tự động khóa 30 phút (`LockoutEndUtc = Now + 30m`) khi đăng nhập sai mật khẩu quá 5 lần liên tiếp (`FailedLoginCount >= 5`). Hết 30 phút hệ thống tự động cho phép thử lại.
  - *Khóa Quản trị Vĩnh viễn (`ADMIN_PERMANENT_LOCK`)*: Khóa do `SecurityAdmin` hoặc `SuperAdmin` thực thi khi phát hiện gian lận hoặc vi phạm (`AccountStatus = Locked`, `LockoutEndUtc = null`). Yêu cầu nhập lý do khóa `LockReason` ($\ge 15$ ký tự) và mã `ticketId`.
- **Ràng buộc Vô hiệu hóa Phiên và PUSH Tức thì SLA $\le 5\text{s}$ (`Instant Session & Push Revocation`)**: Khi tài khoản bị khóa (dù tạm thời hay vĩnh viễn), hệ thống TỰ ĐỘNG tăng `SecurityEpoch` $+1$ và chuyển trạng thái toàn bộ PUSH Device sang `IsActive = false` (M01-T027-A). Toàn bộ JWT Token đang hoạt động bị vô hiệu hóa trong SLA $\le 5$ giây.
- **Ràng buộc Mở khóa Quản trị (`Admin Unlock Protocol`)**: Chỉ `SecurityAdmin` và `SuperAdmin` có thẩm quyền thực hiện API mở khóa tài khoản bị khóa vĩnh viễn. Mở khóa thành công đặt lại `AccountStatus = Active`, `FailedLoginCount = 0`, `LockoutEndUtc = null` và phát sự kiện `UserAccountUnlockedEvent`.
- **Lưu vết Sổ Kiểm toán Bất biến M11 (`Lockout Audit Trail`)**: $100\%$ thao tác khóa và mở khóa tài khoản bắt buộc lưu vết bất biến `ACT-M11-05` (hoặc `ACT-M11-31`) đính kèm `ActorUserId`, `TargetUserId`, `LockType`, `LockReason` và `SecurityEpoch`.

## 2. Mô hình Thuộc tính Trạng thái Khóa Tài khoản (Account Lockout Schema)

```json
{
  "userId": 10024,
  "canonicalEmail": "user10024@gmail.com",
  "accountStatus": "Locked",
  "lockType": "ADMIN_PERMANENT_LOCK",
  "failedLoginCount": 0,
  "lockoutEndUtc": null,
  "lockedAtUtc": "2026-08-20T10:00:00Z",
  "lockedByActorId": "USR-SEC-002",
  "lockReason": "Phát hiện hành vi gian lận điểm thưởng M06 và spam hệ thống.",
  "securityEpoch": 4
}
```

## 3. Kiến trúc Luồng Khóa Tự động và Khóa Quản trị (Account Lockout Engine)

```
                       [User Login Request / Admin Action]
                                       |
                   +-------------------+-------------------+
                   | (Failed Password Login)               | (Admin Lock Action)
                   v                                       v
         [FailedLoginCount++]                 [SecurityAdmin / SuperAdmin]
                   |                                       |
         +---------+---------+                             v
         | (Count < 5)       | (Count >= 5)    [Set AccountStatus = Locked]
         v                   v                 - Set LockType = ADMIN_PERMANENT_LOCK
  [Return 401]        [Set Auto Lock 30m]      - Set LockoutEndUtc = null
                      - LockoutEndUtc = Now+30m            |
                      - LockType = AUTO_TEMP               |
                             |                             |
                             +-------------->+<------------+
                                             |
                                             v
                           [Increment SecurityEpoch += 1]
                           - Invalidate all JWT Tokens in Redis
                           - Deactivate all Push Devices (M01-T027-A)
                                             |
                                             v
                           [Publish UserAccountLockedEvent & M11 Log]
```

## 4. Giao thức Thực thi Khóa và Mở Khóa CSDL (AccountLockService)

```csharp
public async Task<bool> AdminLockAccountAsync(string targetUserId, string lockReason, string ticketId, string actorUserId, string actorRole)
{
    // 1. Kiểm tra Quyền RBAC (SecurityAdmin / SuperAdmin)
    if (actorRole != "SecurityAdmin" && actorRole != "SuperAdmin")
    {
        throw new UnauthorizedAccessException("ADMIN_LOCK_FORBIDDEN");
    }

    if (string.IsNullOrEmpty(lockReason) || lockReason.Length < 15)
    {
        throw new ArgumentException("LOCK_REASON_MIN_LENGTH_15");
    }

    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId);
    if (user == null) throw new InvalidOperationException("TARGET_USER_NOT_FOUND");

    // 2. Thực thi Khóa Vĩnh viễn
    user.AccountStatus = UserAccountStatus.Locked;
    user.LockType = "ADMIN_PERMANENT_LOCK";
    user.LockoutEndUtc = null;
    user.LockedAtUtc = DateTime.UtcNow;
    user.LockedByActorId = actorUserId;
    user.LockReason = lockReason;
    user.SecurityEpoch += 1; // Vô hiệu hóa mọi JWT Session trong Redis SLA <= 5s

    // 3. Hủy Kích hoạt tất cả PUSH Devices
    await _pushDeviceService.RevokeAllDevicesForUserAsync(targetUserId, "ACCOUNT_LOCKED");

    await _db.SaveChangesAsync();

    // 4. Ghi vết Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-31", actorUserId, new { TargetUserId = targetUserId, LockType = "ADMIN_PERMANENT_LOCK", Reason = lockReason, TicketId = ticketId });

    return true;
}

public async Task<bool> AdminUnlockAccountAsync(string targetUserId, string unlockReason, string actorUserId, string actorRole)
{
    if (actorRole != "SecurityAdmin" && actorRole != "SuperAdmin")
    {
        throw new UnauthorizedAccessException("ADMIN_UNLOCK_FORBIDDEN");
    }

    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId);
    if (user == null) throw new InvalidOperationException("TARGET_USER_NOT_FOUND");

    user.AccountStatus = UserAccountStatus.Active;
    user.LockType = null;
    user.LockoutEndUtc = null;
    user.FailedLoginCount = 0;
    user.LockReason = null;

    await _db.SaveChangesAsync();
    await _auditLog.RecordEventAsync("ACT-M11-31", actorUserId, new { Action = "UNLOCK", TargetUserId = targetUserId, Reason = unlockReason });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AL-G01` | Sai mật khẩu liên tiếp 5 lần tự động chuyển trạng thái Khóa Tạm thời 30 phút (`LockoutEndUtc = Now + 30m`). |
| `AL-G02` | Hết 30 phút khóa tạm thời, người dùng tự động được phép đăng nhập lại nếu gõ đúng mật khẩu. |
| `AL-G03` | Khóa quản trị vĩnh viễn (`ADMIN_PERMANENT_LOCK`) bắt buộc nhập `LockReason >= 15` ký tự và mã `ticketId`. |
| `AL-G04` | Khóa tài khoản thành công tự động tăng `SecurityEpoch` $+1$, vô hiệu hóa JWT sessions trong SLA $\le 5$ giây. |
| `AL-G05` | Khóa tài khoản thành công tự động chuyển $100\%$ Push Devices của tài khoản sang `IsActive = false`. |
| `AL-G06` | Chỉ vai trò `SecurityAdmin` và `SuperAdmin` mới có quyền mở khóa tài khoản bị khóa vĩnh viễn. |
| `AL-G07` | Mở khóa thành công đặt lại `FailedLoginCount = 0`, `AccountStatus = Active` và `LockoutEndUtc = null`. |
| `AL-G08` | 100% thao tác khóa và mở khóa ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-31`). |
| `AL-G09` | SLA thực thi API khóa/mở khóa tài khoản $< 30\text{ms}$. |
| `AL-G10` | 100% các test case tự kiểm AL31-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AL31-01` | Người học gõ sai mật khẩu 5 lần liên tiếp | Đăng nhập lần 5 bị khóa tạm thời 30m, trả về 403 Lockout |
| `AL31-02` | Thử đăng nhập lại ở phút thứ 15 sau khi bị khóa tự động | Deny 403 Lockout, trả về số phút còn lại |
| `AL31-03` | Đăng nhập đúng mật khẩu ở phút thứ 31 sau khi hết hạn khóa | Đăng nhập thành công, reset `FailedLoginCount = 0` |
| `AL31-04` | `SecurityAdmin` gửi request khóa vĩnh viễn tài khoản User B | `AccountStatus = Locked`, tăng `SecurityEpoch` $+1$ |
| `AL31-05` | Thử khóa quản trị vĩnh viễn với `lockReason` ngắn 10 ký tự ($< 15$) | Reject 400 `LOCK_REASON_MIN_LENGTH_15` |
| `AL31-06` | User B bị khóa thực hiện gọi API nạp bài học M03 với JWT cũ | Deny 401 Unauthorized do `SecurityEpoch` đã thay đổi |
| `AL31-07` | Kiểm tra trạng thái 5 Push Devices của User B sau khi bị khóa | Cả 5 thiết bị đều chuyển `IsActive = false` |
| `AL31-08` | `SecurityAdmin` thực hiện mở khóa tài khoản User B | `AccountStatus = Active`, cho phép đăng nhập lại |
| `AL31-09` | User vai trò `Learner` hoặc `SupportAgent` thử gọi API mở khóa | Deny 403 Forbidden |
| `AL31-10` | User chưa đăng nhập thử gọi API khóa tài khoản | Deny 401 Unauthorized |
| `AL31-11` | Tra cứu vết Audit Log M11 sau khi khóa tài khoản | Ghi nhận Audit Event `ACT-M11-31` đính kèm `LockReason` |
| `AL31-12` | Tải đồng thời 50 request kiểm tra trạng thái khóa tài khoản | Response latency p95 $< 20\text{ms}$ |
| `AL31-13` | Đăng nhập đúng mật khẩu ở lần thứ 4 (sau 4 lần sai) | Đăng nhập thành công, reset `FailedLoginCount = 0` |
| `AL31-14` | Khóa tài khoản đang có 10 phiên học M03 đang mở | 10 phiên M03 bị hủy ngắt kết nối lập tức |
| `AL31-15` | `SuperAdmin` thực hiện khóa tài khoản của 1 `ContentAdmin` | Khóa thành công, vô hiệu hóa mọi quyền admin |
| `AL31-16` | Thử đăng ký tài khoản mới trùng email với tài khoản đang bị `Locked` | Reject 400 `EMAIL_ALREADY_EXISTS` |
| `AL31-17` | Phân tích tham chiếu tài khoản bị khóa trong toàn bộ hệ thống | Quét cache Redis session và push tokens (T020) |
| `AL31-18` | Thao tác khóa tài khoản bị gián đoạn do lỗi DB | Rollback transaction, trạng thái tài khoản giữ nguyên `Active` |
| `AL31-19` | Mở khóa tài khoản bị khóa tạm thời tự động bởi Admin trước thời hạn 30m | Mở khóa thành công, cho phép đăng nhập ngay |
| `AL31-20` | Kiểm thử hoàn tất luồng chuẩn hóa khóa mở tài khoản M01-ACCOUNT-LOCK-UNLOCK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-AL-I01` | Entity `User.cs` hiện chưa có các thuộc tính `LockType` và `LockReason` | Thiếu thông tin phân loại lý do khóa để trình diễn UI | M01-T049 (Source task) |
| `M01-AL-I02` | Chưa tự động hủy active toàn bộ Push Devices khi khóa tài khoản | Tài khoản bị khóa vẫn tiếp tục nhận được Push Notification | M01-T049; M01-T027-A |
| `M01-AL-I03` | Thiếu validation độ dài tối thiểu 15 ký tự cho `lockReason` | Admin có thể nhập lý do quá ngắn không đủ bằng chứng kiểm toán | M01-T049 |
| `M01-AL-I04` | Thiếu tính năng mở khóa quản trị dành cho `SecurityAdmin` | Khó khăn cho ban quản trị khi muốn khôi phục tài khoản oan | M01-T049 |
| `M01-AL-I05` | Chưa phát sự kiện `UserAccountLockedEvent` sang Sổ Kiểm toán M11 | Thiếu vết sự kiện an ninh quan trọng trong log vận hành | M01-T049; M11-T031 |

- `M01-AL-F01`: Thêm `LockType`, `LockReason`, `LockedByActorId` vào `User.cs` (tiếp nhận: M01-T049).
- `M01-AL-F02`: Triển khai `AccountLockService` với 2 cơ chế khóa (Auto Temp & Admin Permanent) (tiếp nhận: M01-T049).
- `M01-AL-F03`: Tích hợp tăng `SecurityEpoch` $+1$ và hủy Push Devices khi khóa (tiếp nhận: M01-T049; M01-T027-A).
- `M01-AL-F04`: Thiết lập bộ kiểm thử tự động AL-G01–G10 và AL31-01–20 (tiếp nhận: M01 tasks).
- `M01-AL-F05`: Thu thập bằng chứng runtime cho luồng khóa mở tài khoản M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T031

- Đã thiết kế hoàn chỉnh `M01-ACCOUNT-LOCK-UNLOCK-1.0` với Giao thức Khóa và Mở Khóa Tài khoản 2 Nhánh.
- Đã chốt Ràng buộc Khóa Tạm thời Tự động 30 phút (sai 5 lần) và Khóa Quản trị Vĩnh viễn.
- Đã chốt Ràng buộc Vô hiệu hóa Phiên JWT (`SecurityEpoch`) và PUSH Device tức thì SLA $\le 5\text{s}$.
- Đã lồng ghép Quy trình Mở khóa Quản trị an toàn và Lưu vết Audit Log M11 (`ACT-M11-31`).
- Đã xác lập 10 Regression Gates (`AL-G01`–`AL-G10`) và 20 Test Cases tự kiểm (`AL31-01`–`AL31-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa khóa và mở tài khoản M01-T031 | WSA-7K2 |
