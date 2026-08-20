# Chuẩn hóa quyền xem và sửa hồ sơ — lát A M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-PROFILE-ACCESS-A-1.0` |
| Task | M01-T022-A |
| Đầu vào | M01-T003 (D-016), M01-ROLE-MATRIX-1.0 (D-075), M11-T027 (D-074), REL-01, REL-07 |
| Phạm vi | Ma trận phân quyền hiển thị (Visibility) và chỉnh sửa (Mutation) Hồ sơ người dùng, quy tắc bảo vệ thông tin cá nhân PII và phân định giữa Public DTO vs Self DTO |
| Tự kiểm | A-G01, A-G02; REL-01, REL-07 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Phân quyền Xem và Chỉnh sửa Hồ sơ Người dùng — Lát A (`Profile View & Edit Access Matrix - Slice A`) thuộc M01, bảo vệ quyền riêng tư cá nhân theo tiêu chuẩn REL-01 và bảo đảm người dùng không thể tự ý nâng quyền hay thay đổi các trường hệ thống bất biến.

- **Phân tách DTO Công khai và Riêng tư (`Public vs Self DTO Separation Invariant`)**:
  - *Public Profile DTO*: Chỉ trả về `UserId`, `UserPublicId`, `DisplayName`, `AvatarUrl`, `PublicBadgeCount`, `AccountStatus`. Công khai cho $100\%$ người học đã đăng nhập.
  - *Self Profile DTO*: Trả về thêm `CanonicalEmail` (hoặc `maskedEmail`), `PhoneNumber`, `RegisteredAtUtc`, `SecurityEpoch`, `TargetStudyMinutesPerDay`. CHỈ trả về cho chính tài khoản chủ sở hữu (`UserId == CurrentUserId`) hoặc Support Agent có ticket context (M11-T027).
- **Ràng buộc Trường Bất biến (`Read-Only System Attributes Invariant`)**: Các trường `UserId`, `UserPublicId`, `CanonicalEmail`, `Role`, `AccountStatus`, `SecurityEpoch` CẤM cập nhật trực tiếp qua Profile Edit API. Mọi thay đổi các trường này bắt buộc qua giao thức riêng (`M01-T020`, `M01-T029`, `M01-T031`).
- **Kiểm soát Tần suất Đổi Hồ sơ (`Profile Edit Rate Limiter`)**: Thao tác cập nhật hồ sơ (`DisplayName`, `Bio`, `AvatarUrl`) bị giới hạn tối đa 5 lần/ngày per user để tránh cào dữ liệu hoặc spam thay đổi.
- **Lưu vết Audit Log M11 khi Support Agent Xem PII Hồ sơ**: Khi Nhân viên Hỗ trợ xem thông tin PII trong Hồ sơ người dùng, hệ thống bắt buộc lưu vết bất biến `ACT-M11-27` kèm mã `ticketId`.

## 2. Ma trận Quyền Truy cập và Chỉnh sửa Trường Hồ sơ (Profile Field Matrix)

| Trường Hồ sơ (`Field Name`) | Kiểu Dữ liệu | Hiển thị Công khai (`Public Read`) | Xem Riêng tư (`Self Read`) | Quyền Sửa (`Write Access`) | Điểm Lưu ý |
|---|---|---|---|---|---|
| `UserId` | int | CÓ | CÓ | KHÔNG (Readonly) | Định danh nội bộ bất biến |
| `UserPublicId` | GUID | CÓ | CÓ | KHÔNG (Readonly) | Định danh công khai |
| `DisplayName` | string | CÓ | CÓ | CHÍNH CHỦ (`Self`) | Max 50 char, qua AI Safety Filter |
| `AvatarUrl` | string | CÓ | CÓ | CHÍNH CHỦ (`Self`) | URL CDN hợp lệ (M12) |
| `Bio` | string | CÓ | CÓ | CHÍNH CHỦ (`Self`) | Max 200 char, mô tả ngắn |
| `CanonicalEmail` | string | KHÔNG (Ẩn PII) | CÓ (Gốc/Masked) | KHÔNG (Qua M01-T020) | PII nhạy cảm (REL-01) |
| `PhoneNumber` | string | KHÔNG (Ẩn PII) | CÓ (Gốc/Masked) | KHÔNG (Qua M01-T020) | PII nhạy cảm (REL-01) |
| `AccountStatus` | string | CÓ (`Active`/`Locked`)| CÓ | KHÔNG (Qua M01-T031) | Trạng thái tài khoản |
| `Role` | string | KHÔNG | CÓ | KHÔNG (Qua M01-T029) | Vai trò RBAC (M01-T028) |
| `SecurityEpoch` | int | KHÔNG | CÓ | KHÔNG (Readonly) | Epoch vô hiệu hóa session |

## 3. Kiến trúc Bộ dịch vụ Phân quyền Hồ sơ (ProfileAccessEngine)

```csharp
public async Task<object> GetUserProfileAsync(string targetUserId, string currentUserId, string currentUserRole, string ticketId = null)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId || u.UserPublicId == targetUserId);
    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    // 1. Nếu người xem là chính chủ -> Trả về Self Profile DTO đầy đủ
    if (user.UserId == currentUserId)
    {
        return MapToSelfDto(user);
    }

    // 2. Nếu người xem là SupportAgent/Admin có Ticket ID hợp lệ -> Trả về Unmasked Support DTO
    if ((currentUserRole == "SupportAgent" || currentUserRole == "SecurityAdmin") && !string.IsNullOrEmpty(ticketId))
    {
        await _ticketVerifier.ValidateTicketAsync(ticketId, targetUserId);
        await _auditLog.RecordEventAsync("ACT-M11-27", currentUserId, new { TargetUserId = targetUserId, TicketId = ticketId });
        return MapToSupportDto(user, unmaskPii: true);
    }

    // 3. Người học khác xem profile công khai -> Trả về Public Profile DTO (Che mờ PII)
    return MapToPublicDto(user);
}
```

## 4. Quy trình Cập nhật Hồ sơ Cá nhân (Profile Update Workflow)

```csharp
public async Task<UserProfileDto> UpdateSelfProfileAsync(string currentUserId, UpdateProfileRequestDto dto)
{
    // 1. Kiểm tra Rate Limiter (tối đa 5 lần/ngày)
    await _rateLimiter.CheckLimitAsync($"profile_edit:{currentUserId}", maxRequests: 5, windowMinutes: 1440);

    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
    
    // 2. Validate & Sanitize DisplayName
    if (!string.IsNullOrEmpty(dto.DisplayName))
    {
        if (dto.DisplayName.Length < 3 || dto.DisplayName.Length > 50)
            throw new ArgumentException("DISPLAY_NAME_LENGTH_3_TO_50");

        await _aiSafetyFilter.ValidateContentAsync(dto.DisplayName);
        user.DisplayName = dto.DisplayName.Trim();
    }

    // 3. Validate Bio & AvatarUrl
    if (dto.Bio != null)
    {
        if (dto.Bio.Length > 200) throw new ArgumentException("BIO_EXCEEDS_200_CHARS");
        await _aiSafetyFilter.ValidateContentAsync(dto.Bio);
        user.Bio = dto.Bio.Trim();
    }

    if (dto.AvatarUrl != null)
    {
        await _assetValidator.ValidateAvatarUrlAsync(dto.AvatarUrl); // M12
        user.AvatarUrl = dto.AvatarUrl;
    }

    await _db.SaveChangesAsync();
    return MapToSelfDto(user);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `PA-G01` | GET Profile công khai của người học khác tuyệt đối KHÔNG chứa Email hoặc Phone thô (REL-01). |
| `PA-G02` | GET Profile cá nhân của chính mình trả về đầy đủ DTO Self Profile bao gồm email và cài đặt cá nhân. |
| `PA-G03` | Cấm cập nhật các trường `UserId`, `CanonicalEmail`, `Role`, `AccountStatus` qua Profile Edit API. |
| `PA-G04` | Cập nhật `DisplayName` bắt buộc có độ dài từ 3 đến 50 ký tự và vượt qua AI Safety Screening. |
| `PA-G05` | Giới hạn tần suất chỉnh sửa hồ sơ tối đa 5 lần/ngày per user (`PROFILE_EDIT_RATE_LIMIT_EXCEEDED`). |
| `PA-G06` | Support Agent chỉ được xem Unmasked PII khi truyền kèm mã `ticketId` vụ việc hợp lệ (REL-07). |
| `PA-G07` | Xem PII bởi Support Agent tự động ghi vết Audit Event bất biến `ACT-M11-27` trong CSDL. |
| `PA-G08` | Phân quyền truy cập và chỉnh sửa hồ sơ tuân thủ ma trận vai trò M01-T028. |
| `PA-G09` | SLA xử lý API nạp DTO Profile cá nhân $< 20\text{ms}$. |
| `PA-G10` | 100% các test case tự kiểm PA22-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PA22-01` | Người học A xem Hồ sơ của chính mình | Trả về DTO Self Profile với `email = "tranv@gmail.com"` |
| `PA22-02` | Người học A xem Hồ sơ của Người học B | Trả về DTO Public Profile, `email = null`, `phone = null` |
| `PA22-03` | Người học A cập nhật `DisplayName = "Tran Van Nhanh"` | Cập nhật thành công, trả về 200 OK |
| `PA22-04` | Thử cập nhật `DisplayName` dài 2 ký tự ($< 3$) | Reject với lỗi `DISPLAY_NAME_LENGTH_3_TO_50` |
| `PA22-05` | Thử cập nhật `Bio` dài 250 ký tự ($> 200$) | Reject với lỗi `BIO_EXCEEDS_200_CHARS` |
| `PA22-06` | Thử cập nhật `DisplayName` chứa từ ngữ độc hại | Reject bởi AI Safety Filter, ghi log an ninh |
| `PA22-07` | Thử truyền `role = "SuperAdmin"` trong JSON body update profile | Hệ thống lờ đi trường `role`, giữ nguyên vai trò cũ |
| `PA22-08` | Thử truyền `accountStatus = "Active"` trên tài khoản đang bị khóa | Hệ thống lờ đi trường `accountStatus`, giữ nguyên |
| `PA22-09` | Người học A cập nhật hồ sơ 6 lần trong cùng 1 ngày | Lần thứ 6 bị chặn với lỗi `PROFILE_EDIT_RATE_LIMIT_EXCEEDED` |
| `PA22-10` | `SupportAgent` xem hồ sơ User B kèm `ticketId = "TCK-108"` hợp lệ | Trả về Unmasked PII, ghi log `ACT-M11-27` |
| `PA22-11` | `SupportAgent` xem hồ sơ User B nhưng không truyền `ticketId` | Trả về Public DTO che mờ PII |
| `PA22-12` | Tải đồng thời 100 request xem profile công khai của User B | Response latency p95 $< 25\text{ms}$ |
| `PA22-13` | User chưa đăng nhập xem profile công khai của User B | Trả về Public DTO chuẩn hóa |
| `PA22-14` | Cập nhật `AvatarUrl` trỏ về link CDN hợp lệ M12 | Cập nhật `AvatarUrl` thành công |
| `PA22-15` | Xem vết Audit Log M11 sau khi Support Agent xem PII | Ghi nhận Audit Event `ACT-M11-27` kèm `ticketId` |
| `PA22-16` | Cập nhật `TargetStudyMinutesPerDay = 30` | Cập nhật mục tiêu học tập thành công |
| `PA22-17` | Phân tích tham chiếu trước khi ẩn danh hồ sơ | Quét thông tin cá nhân nhạy cảm trong `DisplayName` (T020) |
| `PA22-18` | Thao tác cập nhật hồ sơ bị gián đoạn giữa chừng | Rollback transaction, giữ nguyên dữ liệu hồ sơ cũ |
| `PA22-19` | Xem profile người học đang ở trạng thái `Locked` | Trả về Public DTO với `accountStatus = "Locked"` |
| `PA22-20` | Kiểm thử hoàn tất luồng chuẩn hóa quyền xem và sửa hồ sơ M01-PROFILE-ACCESS-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-PA-I01` | API xem hồ sơ hiện chưa phân tách DTO Public vs DTO Self | Rủi ro trả về email/phone cá nhân cho người dùng khác | M01-T049 (Source task) |
| `M01-PA-I02` | DTO cập nhật hồ sơ chưa chặn cập nhật các trường `role` và `accountStatus` | Người dùng có thể cố tình chèn thuộc tính hệ thống | M01-T049; REL-02 |
| `M01-PA-I03` | Thiếu Rate Limiter giới hạn 5 lần/ngày cho thao tác sửa hồ sơ | Rủi ro bị dùng script đổi tên hiển thị liên tục làm phiền UI | M01-T049 |
| `M01-PA-I04` | Thiếu luồng bắt buộc `ticketId` khi Support Agent xem PII | Nhân viên Hỗ trợ có thể tra cứu PII mà không có vụ việc | M01-T049; M11-T027 |
| `M01-PA-I05` | Chưa chạy AI Safety Screening đối với `DisplayName` và `Bio` | Rủi ro người học đặt tên hiển thị chứa từ ngữ xúc phạm | M01-T049 |

- `M01-PA-F01`: Tách biệt `UserProfilePublicDto` và `UserProfileSelfDto` (tiếp nhận: M01-T049).
- `M01-PA-F02`: Triển khai `ProfileMutationGuard` chặn sửa thuộc tính readonly (tiếp nhận: M01-T049; REL-02).
- `M01-PA-F03`: Thiết lập Profile Edit Rate Limiter 5 req/day (tiếp nhận: M01-T049).
- `M01-PA-F04`: Thiết lập bộ kiểm thử tự động PA-G01–G10 và PA22-01–20 (tiếp nhận: M01 tasks).
- `M01-PA-F05`: Thu thập bằng chứng runtime cho luồng quyền xem sửa hồ sơ M01 (tiếp nhận: M01 tasks; A-G01/REL-01).

## 8. Tự kiểm M01-T022-A

- Đã thiết kế hoàn chỉnh `M01-PROFILE-ACCESS-A-1.0` với Ma trận Phân quyền Xem và Sửa Hồ sơ 10 Trường.
- Đã chốt Ràng buộc Phân tách DTO Công khai và Riêng tư (`Public vs Self DTO Separation`).
- Đã quy định Ràng buộc Trường Bất biến (`Read-Only System Attributes`) và Profile Edit Rate Limiter.
- Đã lồng ghép bảo vệ PII (REL-01) và Yêu cầu Ticket Context đối với Support Agent (REL-07).
- Đã xác lập 10 Regression Gates (`PA-G01`–`PA-G10`) và 20 Test Cases tự kiểm (`PA22-01`–`PA22-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa quyền xem và sửa hồ sơ M01-T022-A | WSA-7K2 |
