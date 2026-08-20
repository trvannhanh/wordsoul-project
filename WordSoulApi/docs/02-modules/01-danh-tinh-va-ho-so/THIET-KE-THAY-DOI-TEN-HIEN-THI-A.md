# Thiết kế thay đổi tên hiển thị — lát A M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-DISPLAY-NAME-CHANGE-A-1.0` |
| Task | M01-T023-A |
| Đầu vào | M01-PROFILE-ACCESS-A-1.0 (D-087), M02-ASSET-MODERATION-1.0 (D-072) |
| Phạm vi | Giao thức kiểm soát thay đổi Tên hiển thị (`DisplayName`) của người học, quy tắc giới hạn thời gian chờ Cooldown 7 ngày, kiểm duyệt từ khóa mạo danh và an toàn AI |
| Tự kiểm | A-G01 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thay đổi Tên Hiển thị — Lát A (`Display Name Change Protocol - Slice A`) thuộc M01, quy định các điều kiện an toàn, chống mạo danh quản trị viên và giới hạn tần suất khi người học cập nhật Tên hiển thị (`DisplayName`).

- **Ràng buộc Thời gian Chờ Đổi Tên 7 Ngày (`7-Day Cooldown Invariant`)**: Mỗi tài khoản chỉ được phép đổi Tên hiển thị tối đa 1 lần mỗi 7 ngày ($168$ giờ). CẤM đổi tên liên tục gây nhiễu giao diện cộng đồng và bảng xếp hạng M09.
- **Ràng buộc Chống Mạo danh Quản trị viên (`Impersonation Guard Invariant`)**: Tên hiển thị CẤM chứa các từ khóa hệ thống bảo lưu (`Admin`, `Administrator`, `WordSoul`, `System`, `Mod`, `Support`, `Official`).
- **Ràng buộc Kiểm duyệt AI Safety Screening (`Real-time AI Toxicity Filter`)**: Tên hiển thị mới bắt buộc vượt qua Bộ lọc An toàn AI trong thời gian thực ($Toxicity < 0.05$). CẤM chứa ngôn từ thù hận, thô tục, phân biệt chủng tộc hoặc kích động.
- **Giới hạn Độ dài Chuẩn hóa ($3 \le \text{Length} \le 50$)**: Tên hiển thị sau khi trim khoảng trắng dư thừa bắt buộc có độ dài từ 3 đến 50 ký tự. CẤM tên chỉ chứa ký tự khoảng trắng hoặc ký tự ẩn Unicode gâylỗi UI.

## 2. Quy trình Thực thi Thay đổi Tên Hiển thị (Display Name Change Engine)

```
[User Submits New DisplayName]
              |
              v
 [Check 7-Day Cooldown Period]
 (LastDisplayNameChangedAtUtc <= Now - 7 days)
              |
      +-------+-------+
      | (Cooldow Active) | (Cooldown Expired)
      v                  v
 [Reject 400 Cooldown] [Validate Length 3..50 & Unicode Cleanup]
                         |
                         v
               [Check Impersonation Guard Keywords]
                         |
                 +-------+-------+
                 | (Reserved)    | (Clean)
                 v               v
           [Reject 400   [Run AI Safety Toxicity Scan]
            Reserved]            |
                         +-------+-------+
                         | (Tox >= 0.05) | (Tox < 0.05)
                         v               v
                   [Reject 400    [Update DisplayName in DB]
                    Safety]       - Set LastDisplayNameChangedAtUtc
                                  - Record Audit Event ACT-M11-05
```

## 3. Cấu trúc Response DTO Thay đổi Tên Hiển thị (ChangeDisplayNameResponseDto)

```json
{
  "userId": 10024,
  "oldDisplayName": "Tran Nhanh",
  "newDisplayName": "Tran Van Nhanh",
  "changedAtUtc": "2026-08-20T10:00:00Z",
  "nextAllowedChangeAtUtc": "2026-08-27T10:00:00Z",
  "cooldownDaysRemaining": 7
}
```

## 4. Giao thức Xử lý Thay đổi Tên Hiển thị CSDL (ChangeDisplayNameService)

```csharp
public async Task<ChangeDisplayNameResponseDto> ChangeDisplayNameAsync(string currentUserId, string newDisplayName)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    // 1. Kiểm tra Cooldown 7 ngày (168 giờ)
    if (user.LastDisplayNameChangedAtUtc.HasValue && 
        user.LastDisplayNameChangedAtUtc.Value.AddDays(7) > DateTime.UtcNow)
    {
        var nextAllowed = user.LastDisplayNameChangedAtUtc.Value.AddDays(7);
        throw new InvalidOperationException($"DISPLAY_NAME_COOLDOWN_ACTIVE: Bạn chỉ có thể đổi tên sau {nextAllowed:yyyy-MM-dd HH:mm:ss} UTC.");
    }

    // 2. Validate độ dài và ký tự
    var cleanName = newDisplayName?.Trim();
    if (string.IsNullOrEmpty(cleanName) || cleanName.Length < 3 || cleanName.Length > 50)
    {
        throw new ArgumentException("DISPLAY_NAME_LENGTH_3_TO_50");
    }

    // 3. Kiểm tra Impersonation Guard
    var reservedKeywords = new[] { "admin", "administrator", "wordsoul", "system", "moderator", "support", "official" };
    if (reservedKeywords.Any(kw => cleanName.ToLowerInvariant().Contains(kw)))
    {
        throw new InvalidOperationException("DISPLAY_NAME_RESERVED_KEYWORD_FORBIDDEN");
    }

    // 4. Quét AI Safety Screening
    await _aiSafetyFilter.ValidateContentAsync(cleanName);

    // 5. Cập nhật và lưu Audit Log
    string oldName = user.DisplayName;
    user.DisplayName = cleanName;
    user.LastDisplayNameChangedAtUtc = DateTime.UtcNow;

    await _auditLog.RecordEventAsync("ACT-M11-05", currentUserId, new { OldName = oldName, NewName = cleanName });
    await _db.SaveChangesAsync();

    return new ChangeDisplayNameResponseDto
    {
        UserId = user.UserId,
        OldDisplayName = oldName,
        NewDisplayName = cleanName,
        ChangedAtUtc = user.LastDisplayNameChangedAtUtc.Value,
        NextAllowedChangeAtUtc = user.LastDisplayNameChangedAtUtc.Value.AddDays(7),
        CooldownDaysRemaining = 7
    };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DN-G01` | Cấm đổi Tên hiển thị nếu thời gian lần đổi gần nhất chưa đủ 7 ngày ($168$ giờ). |
| `DN-G02` | Cấm Tên hiển thị chứa các từ khóa hệ thống bảo lưu (`Admin`, `WordSoul`, `System`, `Support`). |
| `DN-G03` | Cấm Tên hiển thị ngắn hơn 3 ký tự hoặc dài hơn 50 ký tự sau khi trim. |
| `DN-G04` | Tên hiển thị mới bắt buộc vượt qua Bộ lọc AI Safety Screening ($Toxicity < 0.05$). |
| `DN-G05` | Đổi tên hiển thị thành công tự động cập nhật `LastDisplayNameChangedAtUtc = DateTime.UtcNow`. |
| `DN-G06` | 100% thao tác đổi tên hiển thị ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-05`). |
| `DN-G07` | Tên hiển thị mới tự động đồng bộ lên Bảng xếp hạng M09 và Hồ sơ Công khai M01. |
| `DN-G08` | Phân quyền thực hiện đổi tên tuân thủ ma trận vai trò M01-T028 (`Learner` tự đổi cho mình). |
| `DN-G09` | SLA thực thi API đổi tên hiển thị (bao gồm AI scan) $< 40\text{ms}$. |
| `DN-G10` | 100% các test case tự kiểm DN23-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DN23-01` | Đổi Tên hiển thị từ "Tran Nhanh" thành "Tran Van Nhanh" | Đổi tên thành công, trả về nextAllowedAt sau 7 ngày |
| `DN23-02` | Thử đổi tên lần 2 ngay sau khi vừa đổi thành công | Reject 400 `DISPLAY_NAME_COOLDOWN_ACTIVE` |
| `DN23-03` | Thử đổi tên thành "WordSoul_Admin" | Reject 400 `DISPLAY_NAME_RESERVED_KEYWORD_FORBIDDEN` |
| `DN23-04` | Thử đổi tên thành "System_Official" | Reject 400 `DISPLAY_NAME_RESERVED_KEYWORD_FORBIDDEN` |
| `DN23-05` | Thử đổi tên thành "An" ($< 3$ ký tự) | Reject 400 `DISPLAY_NAME_LENGTH_3_TO_50` |
| `DN23-06` | Thử đổi tên chứa chuỗi từ ngữ độc hại AI Toxicity | Reject 400 bởi AI Safety Filter, ghi log an ninh |
| `DN23-07` | Đổi tên sau đúng 7 ngày 1 giờ kể từ lần đổi trước | Đổi tên thành công, cập nhật mốc cooldown mới |
| `DN23-08` | Tra cứu vết Audit Log M11 sau khi đổi tên | Ghi nhận Audit Event `ACT-M11-05` kèm `OldName` & `NewName` |
| `DN23-09` | Kiểm tra tên hiển thị trên Bảng xếp hạng M09 sau khi đổi tên | Tên mới tự động hiển thị trên BXH M09 |
| `DN23-10` | Thử truyền chuỗi toàn khoảng trắng `"     "` | Reject 400 `DISPLAY_NAME_LENGTH_3_TO_50` |
| `DN23-11` | Thử truyền chuỗi Unicode chứa ký tự ẩn | Tự động làm sạch Unicode, đổi tên an toàn |
| `DN23-12` | Tải đồng thời 50 request đổi tên từ 50 tài khoản khác nhau | Response latency p95 $< 45\text{ms}$ |
| `DN23-13` | Người học A thử đổi tên hiển thị của Người học B | Deny 403 Forbidden |
| `DN23-14` | User chưa đăng nhập thử gọi API đổi tên hiển thị | Deny 401 Unauthorized |
| `DN23-15` | Admin thực hiện đổi đè tên hiển thị vi phạm cho người dùng | Bỏ qua Cooldown 7 ngày, đổi tên thành "User_10024" |
| `DN23-16` | Kiểm tra giá trị `cooldownDaysRemaining` trong Response DTO | Trả về chính xác số ngày còn lại (7 ngày) |
| `DN23-17` | Phân tích tham chiếu các vị trí hiển thị `DisplayName` trong hệ thống | Quét cache Redis hồ sơ người dùng (T020) |
| `DN23-18` | Thao tác đổi tên bị gián đoạn giữa chừng do lỗi DB | Rollback transaction, giữ nguyên tên cũ và chưa tính cooldown |
| `DN23-19` | Đổi tên hiển thị có độ dài đúng 50 ký tự tối đa | Đổi tên thành công, chấp nhận độ dài 50 char |
| `DN23-20` | Kiểm thử hoàn tất luồng thiết kế thay đổi tên hiển thị M01-DISPLAY-NAME-CHANGE-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-DN-I01` | Entity `User.cs` chưa có thuộc tính `LastDisplayNameChangedAtUtc` | Chưa ghi nhận được mốc thời gian đổi tên để tính Cooldown | M01-T049 (Source task) |
| `M01-DN-I02` | Chưa có bộ từ khóa mạo danh `reservedKeywords` chặn đổi tên | Người dùng có thể đặt tên mạo danh Quản trị viên WordSoul | M01-T049 |
| `M01-DN-I03` | Thiếu validation Cooldown 7 ngày trong `ProfileService.cs` | Người dùng có thể spam đổi tên liên tục trong ngày | M01-T049 |
| `M01-DN-I04` | Thiếu AI Safety Screening đối với tên hiển thị mới | Rủi ro chèn từ ngữ xúc phạm vào Tên hiển thị public | M01-T049; M02-T012 |
| `M01-DN-I05` | Chưa đồng bộ tên hiển thị mới sang Redis Cache của BXH M09 | BXH M09 vẫn hiển thị tên cũ của người học trong 24h | M01-T049; M09-T010 |

- `M01-DN-F01`: Thêm `LastDisplayNameChangedAtUtc` vào `User.cs` và CSDL Migration (tiếp nhận: M01-T049).
- `M01-DN-F02`: Triển khai `ChangeDisplayNameService` với Impersonation Guard & Cooldown 7d (tiếp nhận: M01-T049).
- `M01-DN-F03`: Tích hợp AI Safety Screening Filter cho luồng đổi tên (tiếp nhận: M01-T049).
- `M01-DN-F04`: Thiết lập bộ kiểm thử tự động DN-G01–G10 và DN23-01–20 (tiếp nhận: M01 tasks).
- `M01-DN-F05`: Thu thập bằng chứng runtime cho luồng thay đổi tên hiển thị M01 (tiếp nhận: M01 tasks; A-G01).

## 8. Tự kiểm M01-T023-A

- Đã thiết kế hoàn chỉnh `M01-DISPLAY-NAME-CHANGE-A-1.0` với Giao thức Thay đổi Tên Hiển thị 5 Bước.
- Đã chốt Ràng buộc Thời gian Chờ Đổi Tên 7 Ngày (`168` giờ).
- Đã chốt Ràng buộc Chống Mạo danh Quản trị viên (`Impersonation Guard`).
- Đã lồng ghép Bộ lọc An toàn AI Toxicity ($< 0.05$) và Lưu vết Audit Log M11 (`ACT-M11-05`).
- Đã xác lập 10 Regression Gates (`DN-G01`–`DN-G10`) và 20 Test Cases tự kiểm (`DN23-01`–`DN23-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế thay đổi tên hiển thị M01-T023-A | WSA-7K2 |
