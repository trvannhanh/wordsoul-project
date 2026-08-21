# Thiết kế vòng đời ảnh đại diện M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-AVATAR-LIFECYCLE-1.0` |
| Task | M01-T024 |
| Đầu vào | M01-PROFILE-ACCESS-A-1.0 (D-087), M12-ASSET-IMMUTABLE-METADATA-1.0 (D-126), M12-ASSET-ACCESS-DISTRIBUTION-1.0 (D-127), M12-ASSET-REPLACEMENT-ORPHAN-CLEANUP-1.0 (D-128), REL-04 |
| Phạm vi | Đặc tả Giao thức Vòng đời Ảnh đại diện Hồ sơ Người học (`UserProfile Avatar Lifecycle Protocol`), luồng nộp ảnh đại diện tùy chỉnh, kiểm duyệt AI An toàn hình ảnh, phân phối CDN Signed URL (TTL 60m) và cơ chế dọn dẹp ảnh cũ |
| Tự kiểm | A-G01, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Vòng đời Ảnh đại diện Hồ sơ Người học (`UserProfile Avatar Lifecycle Protocol`) thuộc M01, chuẩn hóa luồng tải lên, kiểm duyệt an toàn, phân phối và thay thế/xóa ảnh đại diện (Avatar) cá nhân, đảm bảo không phơi nhiễm hình ảnh nhạy cảm và tối ưu hóa tài sản số M12 (REL-04).

- **Upload qua Pre-Signed PUT URL M12 (TTL 15m) (`Pre-Signed Avatar Upload Invariant`)**: Người học tải ảnh đại diện tùy chỉnh BẮT BUỘC sử dụng Pre-Signed PUT URL trực tiếp tới Cloud Storage (M12-T022/D-125). CHỈ chấp nhận định dạng `image/webp`, `image/png`, `image/jpeg` với dung lượng $\le 1\text{MB}$.
- **Ràng buộc Cổng Kiểm duyệt AI An toàn Hình ảnh (`AI Safety Image Moderation Gate`)**: Ảnh đại diện tải lên BẮT BUỘC trải qua bộ lọc kiểm duyệt AI tự động (NSFW / Vi phạm chuẩn mực cộng đồng). Nếu chỉ số độc hại/nhạy cảm $> 0.01$, hệ thống LẬP TỨC HỦY ảnh và giữ nguyên Avatar mặc định cũ (D-072).
- **Phân phối CDN Signed URL cho Ảnh Đại diện Tùy chỉnh (`Private CDN Signed URL Distribution`)**: Ảnh đại diện tùy chỉnh cá nhân được phân phối qua CDN Signed URL ngắn hạn (TTL đúng 60 phút D-127) để bảo vệ quyền riêng tư. Ảnh đại diện mặc định hệ thống (Default System Avatars) được phân phối qua Public CDN.
- **Quy trình Thay thế & Dọn dẹp Ảnh cũ M12 (`Avatar Replacement & RefCount Decrement`)**: Khi người học thay đổi sang ảnh đại diện mới hoặc quay về ảnh mặc định, hệ thống tự động cập nhật con trỏ `AvatarAssetId` mới, giảm `ActiveRefCount` của `OldAvatarAssetId` đi 1 trong M12-T025, và phát lệnh hủy Cache CDN SLA $\le 60$ giây.

## 2. Ma trận Vòng đời Ảnh Đại diện (Avatar Lifecycle Matrix)

| Kịch bản Ảnh Đại diện (`Scenario`) | Loại Avatar | Kết quả Kiểm duyệt AI | Phương thức Phân phối CDN | Xử lý Tệp cũ trên Storage | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `SYSTEM_DEFAULT` | System Asset | Auto Approved | Public CDN (`cdn.wordsoul.com`) | RefCount tệp cũ $-1$ | `ACT-M11-01-AVATAR_DEFAULT` |
| `CUSTOM_UPLOAD_SUCCESS` | Custom Upload | Passed ($<0.01$) | Private CDN Signed URL (TTL 60m) | RefCount tệp cũ $-1$, Hủy Cache | `ACT-M11-01-AVATAR_CUSTOM` |
| `CUSTOM_SAFETY_REJECT` | Custom Upload | **Failed ($\ge 0.01$)** | Giữ Avatar cũ | Tiêu hủy tệp staging mới | `ACT-M11-01-AVATAR_REJECT` |
| `DELETE_AVATAR_RESET` | System Default | N/A | Public CDN Default | RefCount custom old $-1$ | `ACT-M11-01-AVATAR_RESET` |

## 3. Kiến trúc Luồng Xử lý Ảnh Đại diện Hồ sơ (Avatar Engine Pipeline)

```
[User Initiates Custom Avatar Upload Intent]
                     |
                     v
   [Request Pre-Signed Upload URL M12 (TTL 15m, Max 1MB)]
                     |
                     v
   [User Uploads Image File directly to Staging Storage]
                     |
                     v
   [User Calls UpdateAvatar API (StagingAssetId)]
                     |
                     v
   [Execute AI Safety Image Moderation Gate (D-072)]
                     |
        +------------+------------+
        | (NSFW / Toxic >= 0.01)  | (Clean < 0.01)
        v                         v
[REJECT: 400 AVATAR_IMAGE_UNSAFE] [Move Image to Production Storage]
[Purge Staging File]              [Update Profile AvatarAssetId]
                                  [Decrement RefCount on Old Avatar M12-T025]
                                  [Invalidate CDN Cache SLA <= 60s]
                                  [Record Audit Event ACT-M11-01-AVATAR]
```

## 4. Giao thức Thực thi Vòng đời Ảnh Đại diện CSDL (UserProfileAvatarService)

```csharp
public async Task<UserProfileDto> UpdateUserAvatarAsync(
    string userId, 
    string stagingAssetId, 
    bool isResetToDefault, 
    string actorUserId)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) throw new KeyNotFoundException("USER_NOT_FOUND");

    // Authorization Guard: Self or Admin
    if (userId != actorUserId && !_permissionService.IsAdmin(actorUserId))
    {
        throw new UnauthorizedAccessException("PROFILE_ACCESS_DENIED");
    }

    string oldAvatarAssetId = user.AvatarAssetId;

    // 1. Reset to System Default Branch
    if (isResetToDefault)
    {
        user.AvatarAssetId = _systemConfig.DefaultAvatarAssetId;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(oldAvatarAssetId) && oldAvatarAssetId != _systemConfig.DefaultAvatarAssetId)
        {
            await _assetCleanupService.DecrementRefCountAsync(oldAvatarAssetId);
        }

        await _auditLog.RecordEventAsync("ACT-M11-01-AVATAR_RESET", actorUserId, new { UserId = userId });
        return user.ToDto();
    }

    // 2. Custom Upload Branch: AI Safety Moderation Gate
    var stagingAsset = await _assetMetadataService.GetStagingAssetAsync(stagingAssetId);
    if (stagingAsset == null) throw new ArgumentException("INVALID_STAGING_ASSET");

    bool isSafeImage = await _aiModerationService.ScanImageSafetyAsync(stagingAsset.FileStream);
    if (!isSafeImage)
    {
        await _assetMetadataService.PurgeStagingAssetAsync(stagingAssetId);
        throw new InvalidOperationException("AVATAR_IMAGE_UNSAFE: Ảnh đại diện vi phạm tiêu chuẩn cộng đồng an toàn hình ảnh.");
    }

    // 3. Register Production Asset in M12-T023
    var prodAsset = await _assetMetadataService.RegisterAssetMetadataAsync(
        stagingAsset.StagingObjectKey, 
        new AssetRightsEnvelopeDto { RightsCleared = true, LicenseType = "USER_PERMITTED", MimeType = stagingAsset.ContentType, Category = "IMAGE_AVATAR" }, 
        userId);

    // 4. Update Profile Record
    user.AvatarAssetId = prodAsset.AssetId;
    user.UpdatedAtUtc = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    // Increment New & Decrement Old Asset RefCounts in M12-T025
    await _assetCleanupService.IncrementRefCountAsync(prodAsset.AssetId);
    if (!string.IsNullOrEmpty(oldAvatarAssetId) && oldAvatarAssetId != _systemConfig.DefaultAvatarAssetId)
    {
        await _assetCleanupService.DecrementRefCountAsync(oldAvatarAssetId);
        await _cdnDistributionService.InvalidateCdnCacheAsync(oldAvatarAssetId, actorUserId);
    }

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-01-AVATAR_CUSTOM", actorUserId, new {
        UserId = userId,
        OldAvatarAssetId = oldAvatarAssetId,
        NewAvatarAssetId = prodAsset.AssetId
    });

    return user.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AV-G01` | Upload ảnh đại diện tùy chỉnh BẮT BUỘC dùng Pre-Signed PUT URL trực tiếp tới Cloud Storage (TTL $\le 15$ phút). |
| `AV-G02` | Định dạng tệp ảnh đại diện CHỈ chấp nhận `image/webp`, `image/png`, `image/jpeg` với dung lượng $\le 1\text{MB}$. |
| `AV-G03` | Ảnh đại diện BẮT BUỘC trải qua bộ lọc kiểm duyệt AI an toàn hình ảnh, reject ngay nếu độc hại $\ge 0.01$. |
| `AV-G04` | Phân phối ảnh đại diện tùy chỉnh cá nhân BẮT BUỘC dùng CDN Signed URL có chữ ký số (TTL $\le 60$ phút D-127). |
| `AV-G05` | Thay đổi ảnh đại diện mới tự động giảm `ActiveRefCount` tệp cũ trong M12-T025 và xóa Cache CDN SLA $\le 60\text{s}$. |
| `AV-G06` | 100% các thao tác cập nhật hoặc reset ảnh đại diện được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-01-AVATAR`). |
| `AV-G07` | SLA thực thi API cập nhật ảnh đại diện CSDL $< 25\text{ms}$; SLA quét AI Safety $< 300\text{ms}$. |
| `AV-G08` | Phân quyền thay đổi ảnh đại diện chỉ dành cho chính chủ tài khoản hoặc `ProfileAdmin`. |
| `AV-G09` | Trả về ảnh đại diện mặc định hệ thống (System Default Avatar) nếu người dùng chưa đặt avatar tùy chỉnh. |
| `AV-G10` | 100% các test case tự kiểm AV24-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AV24-01` | User A nộp ảnh đại diện tùy chỉnh WebP dung lượng $500\text{KB}$ hợp lệ | Cập nhật avatar thành công, trả về CDN Signed URL TTL 60m |
| `AV24-02` | User A nộp ảnh đại diện PNG dung lượng $1.5\text{MB}$ ($> 1\text{MB}$) | Reject 400 `INVALID_FILE_TYPE_OR_SIZE` |
| `AV24-03` | User A nộp ảnh chứa nội dung NSFW hoặc nhạy cảm | AI Moderation phát hiện, Hủy tệp staging, Reject 400 `AVATAR_IMAGE_UNSAFE` |
| `AV24-04` | User A chọn nút "Reset về Ảnh mặc định hệ thống" | Gán `AvatarAssetId` default, RefCount custom old $-1$, 200 OK |
| `AV24-05` | User B thử cập nhật ảnh đại diện cho User A | Deny 403 `PROFILE_ACCESS_DENIED` |
| `AV24-06` | Tra cứu vết Audit Log M11 sau khi cập nhật ảnh đại diện | Ghi nhận Audit Event `ACT-M11-01-AVATAR_CUSTOM` |
| `AV24-07` | Tra cứu URL ảnh đại diện của User A từ góc nhìn người học khác | Trả về CDN Signed URL hợp lệ |
| `AV24-08` | Thử nộp tệp kịch bản `.html` giả dạng ảnh đại diện | Reject 400 `FORBIDDEN_FILE_EXTENSION` |
| `AV24-09` | Tải đồng thời 100 yêu cầu cập nhật ảnh đại diện từ 100 người dùng | Processing latency p95 $< 22\text{ms}$ |
| `AV24-10` | Cập nhật ảnh đại diện mới khi ảnh cũ vẫn đang được lưu đệm CDN | Phát lệnh CDN Invalidation cho `OldAvatarAssetId` SLA $< 30\text{s}$ |
| `AV24-11` | Thử nộp tệp ảnh GIF có chuyển động (Animated GIF) | Reject 400 `ANIMATED_IMAGES_NOT_ALLOWED` |
| `AV24-12` | Gửi request cập nhật ảnh đại diện khi JWT Access Token đã bị hết hạn | Deny 401 Unauthorized |
| `AV24-13` | ProfileAdmin thực hiện reset ảnh đại diện cho User A vi phạm | Reset về Default thành công (Admin Override) |
| `AV24-14` | User chưa đăng nhập gọi API cập nhật ảnh đại diện | Deny 401 Unauthorized |
| `AV24-15` | Người dùng mới khởi tạo tài khoản kiểm tra Avatar mặc định | Tự động gán `DefaultAvatarAssetId` hệ thống |
| `AV24-16` | Kiểm tra thời gian vô hiệu CDN Signed URL của ảnh đại diện cũ | Invalidation SLA $< 1\text{s}$ trên Cloud Storage |
| `AV24-17` | Phân tích tham chiếu liên kết `AvatarAssetId` trong CSDL | Quét schema `M01_UserProfiles` (T020) |
| `AV24-18` | Dịch vụ AI Moderation bị tạm ngắt kết nối mạng | Chờ quét lại qua Outbox Pattern M12-T037 |
| `AV24-19` | Tra cứu danh sách các avatar tùy chỉnh đã tải lên của User A | Trả về lịch sử AssetId avatar cũ |
| `AV24-20` | Kiểm thử hoàn tất luồng thiết kế vòng đời ảnh đại diện M01-AVATAR-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-AV-I01` | M01 hiện tại chưa có `UserProfileAvatarService` xử lý vòng đời avatar | Risk phơi nhiễm hình ảnh cá nhân và phình bộ nhớ | M01-T049 (Source task) |
| `M01-AV-I02` | Thiếu cờ kiểm duyệt AI Safety An toàn Hình ảnh | Kẻ xấu có thể đăng tải hình ảnh vi phạm thuần phong mỹ tục | M01-T049; D-072 |
| `M01-AV-I03` | Thiếu luồng CDN Signed URL phân phối ảnh đại diện tùy chỉnh | Trực tiếp mở URL public làm phơi nhiễm thông tin người học | M01-T049; D-127 |
| `M01-AV-I04` | Thiếu luồng tự động giảm RefCount tệp avatar cũ trong M12-T025 | Dữ liệu hình ảnh avatar cũ bị phình to dạng mồ côi | M01-T049; M12-T025 |
| `M01-AV-I05` | Chưa kết nối sự kiện cập nhật avatar với Audit Log M11 (`ACT-M11-01-AVATAR`) | Không ghi vết được lịch sử thay đổi ảnh hồ sơ | M01-T049; M11-T031 |

- `M01-AV-F01`: Triển khai `UserProfileAvatarService` với Pre-Signed Upload M12 (tiếp nhận: M01-T049).
- `M01-AV-F02`: Tích hợp Bắt buộc AI Safety Image Moderation Gate (tiếp nhận: M01-T049; D-072).
- `M01-AV-F03`: Triển khai Private CDN Signed URL Distribution & RefCount Decrement M12-T025 (tiếp nhận: M01-T049; D-127).
- `M01-AV-F04`: Thiết lập bộ kiểm thử tự động AV-G01–G10 và AV24-01–20 (tiếp nhận: M01 tasks).
- `M01-AV-F05`: Thu thập bằng chứng runtime cho luồng ảnh đại diện M01 (tiếp nhận: M01 tasks; A-G01/A-G05).

## 8. Tự kiểm M01-T024

- Đã thiết kế hoàn chỉnh `M01-AVATAR-LIFECYCLE-1.0` với Ma trận Vòng đời Ảnh Đại diện.
- Đã chốt Ràng buộc Upload qua Pre-Signed PUT URL M12 (TTL 15m, Max 1MB).
- Đã chốt Ràng buộc Cổng Kiểm duyệt AI An toàn Hình ảnh (`AI Safety Image Moderation Gate`).
- Đã lồng ghép Phân phối CDN Signed URL cho Ảnh Đại diện Tùy chỉnh (TTL 60m), Quy trình Thay thế & Dọn dẹp Ảnh cũ M12-T025 và Audit Log M11 (`ACT-M11-01-AVATAR`).
- Đã xác lập 10 Regression Gates (`AV-G01`–`AV-G10`) và 20 Test Cases tự kiểm (`AV24-01`–`AV24-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế vòng đời ảnh đại diện M01-T024 | WSA-7K2 |
