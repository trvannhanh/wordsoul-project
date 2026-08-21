# Lập bản đồ dữ liệu cá nhân liên module M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-CROSS-MODULE-PII-MAP-1.0` |
| Task | M01-T033 |
| Đầu vào | M01-PROFILE-MAP-1.0 (D-004), M11-CROSS-CONTENT-MATRIX-1.0 (D-050), M12-OUTBOUND-DATA-MAP-1.0 (D-070), REL-01, REL-07 |
| Phạm vi | Bản đồ kiểm kê luồng dữ liệu cá nhân (PII Data Flow Map) xuyên suốt 12 Module (M01–M12), phân loại cấp độ PII (`PII_DIRECT`, `PII_INDIRECT`, `PII_SENSITIVE`) và ranh giới chia sẻ dữ liệu an toàn |
| Tự kiểm | A-G01, A-G05; REL-01, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Bản đồ Dữ liệu Cá nhân Liên Module (`Cross-Module PII Data Map Engine`) thuộc M01, chuẩn hóa luồng lưu chuyển và ranh giới quản lý dữ liệu cá nhân (PII - Personally Identifiable Information) trong toàn bộ hệ thống WordSoulApi, đáp ứng nghiêm ngặt các điều khoản bảo mật REL-01 (Bảo vệ thông tin cá nhân) và REL-07 (Quyền xuất/xóa dữ liệu cá nhân GDPR/PDPA).

- **Phân loại 3 Cấp độ Dữ liệu Cá nhân (`3-Tier PII Classification Invariant`)**:
  - *Cấp 1 - PII_DIRECT (Trực tiếp)*: Họ tên, Email, Số điện thoại, Avatar URL. Chỉ lưu duy nhất tại M01 CSDL Gốc (`Single Source of Truth`). 100% log và AI prompt xuất ra ngoài bắt buộc được che mờ (`Masked`).
  - *Cấp 2 - PII_INDIRECT (Gián tiếp)*: Địa chỉ IP, UserAgent, DeviceId, Múi giờ, Tọa độ địa lý. Bắt buộc băm SHA-256 kèm Salt trước khi lưu vào Sổ Kiểm toán M11 hoặc log Serilog.
  - *Cấp 3 - PII_SENSITIVE (Bảo mật Tối cao)*: Mật khẩu băm (BCrypt), Security Stamps, Refresh Tokens, Khóa MFA. CHỈ lưu trong CSDL M01 mã hóa (Envelope Encryption), CẤM xuất ra ngoài hoặc truyền liên module dưới mọi hình thức.
- **Ràng buộc Cách ly Tham chiếu Định danh GUID (`GUID-Only Cross-Module Reference Invariant`)**: Các module nghiệp vụ khác (M02, M03, M06, M09, M10) CHỈ ĐƯỢC THAM CHIẾU đến người dùng qua mã GUID định danh `UserId`. Tuyệt đối CẤM lưu trữ trực tiếp Email, Họ tên hoặc SĐT trong bảng CSDL của module khác.
- **Không Rò rỉ PII sang Hạ tầng AI / Bên ngoài (`Zero PII Outbound Egress SLA`)**: Khi truyền dữ liệu sang Module M12 (AI Gemini LLM, Audio Synthesis), mọi tham số đính kèm bắt buộc chạy qua bộ lọc `PromptAnonymizerFilter` loại bỏ toàn bộ PII_DIRECT và PII_INDIRECT trước khi egress khỏi ranh giới hệ thống (D-070).
- **Đồng bộ Vòng đời Xuất/Xóa Dữ liệu Cá nhân (`GDPR Data Lifecyle Sync`)**: Bản đồ PII làm cơ sở dữ liệu gốc phục vụ cho luồng Xuất dữ liệu (M01-T034) và Xóa dữ liệu (M01-T035/T036). Khi nhận lệnh xóa, toàn bộ bản đồ PII liên module phải được ẩn danh hóa hoặc xóa sạch theo ma trận phân quyền REL-07.

## 2. Bản đồ Dữ liệu Cá nhân Liên Module (Cross-Module PII Matrix)

| Module Tham chiếu | Loại Dữ liệu PII Tiếp nhận | Trường Dữ liệu Cụ thể | Mục đích Sử dụng | Biện pháp Bảo vệ / Che mờ |
|---|---|---|---|---|
| **M01 (Identity Provider)** | PII_DIRECT, INDIRECT, SENSITIVE | FullName, Email, PasswordHash, Phone, SecurityEpoch, SaltedIP | Quản lý danh tính & đăng nhập | Master Encryption at Rest, Salted Hash |
| **M02 (Vocab Sets)** | GUID Reference | `CreatorUserId` (GUID) | Xác định chủ sở hữu Bộ từ | 0 PII direct stored, chỉ lưu GUID |
| **M03 (Learning Core)** | GUID Reference | `LearnerUserId` (GUID) | Ghi nhận tiến trình học tập | 0 PII direct stored, chỉ lưu GUID |
| **M06 (Gamification)** | GUID Reference | `UserId` (GUID) | Quản lý điểm thưởng Exp, Gold | 0 PII direct stored, chỉ lưu GUID |
| **M09 (Leaderboard)** | PII_DIRECT (Hiển thị) | `DisplayName`, `AvatarUrl`, `UserId` | Bảng xếp hạng thi đua công khai | Dùng DisplayName bí danh, 0 lộ Email |
| **M10 (Messaging/Push)**| PII_INDIRECT + Token | `PushDeviceToken`, `UserId` | Gửi PUSH Notification thiết bị | Tự động hủy token khi Logout (D-091) |
| **M11 (Governance/Audit)**| PII_INDIRECT (Băm) | `ActorUserId`, `TargetUserId`, `SaltedIPHash` | Lưu vết Sổ Kiểm toán bất biến | Băm SHA-256 IP/UserAgent, 0 PII thô |
| **M12 (Platform Integration)**| ZERO PII (Đã lọc) | Prompt sạch đã qua Anonymizer | Gọi AI Gemini / TTS Synthesis | Quét và lọc 100% PII qua Filter (D-070) |

## 3. Kiến trúc Luồng Luân chuyển và Lọc PII (PII Data Flow Engine)

```
[M01 Identity DB (Master Source of Truth)]
 - Stores: FullName, Email, PasswordHash, Phone
                     |
                     +---------------------------------------+
                     | (API Request for User Profile)        | (Trigger AI Prompt Generation)
                     v                                       v
         [M01 Masking Engine]                    [M12 PromptAnonymizerFilter]
         - FullName: "Trần *** Nhanh"            - Strip Email, Phone, Real Name
         - Email: "n***@gmail.com"               - Replace with Anonymized Token
                     |                                       |
                     v                                       v
         [Return Masked DTO to UI]               [Egress Clean Prompt to AI Gemini]
```

## 4. Giao thức Thực thi Lọc và Anonymize PII CSDL (CrossModulePiiService)

```csharp
public class CrossModulePiiService
{
    private const string SaltKey = "WORDSOUL_PII_SALT_2026";

    // 1. Anonymize IP Address for M11 Audit Logs
    public string AnonymizeIpAddress(string rawIpAddress)
    {
        if (string.IsNullOrEmpty(rawIpAddress)) return "0.0.0.0";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawIpAddress + SaltKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash)[..16]; // Return 16-char salted hash
    }

    // 2. Mask Direct PII for UI Display
    public UserPublicProfileDto MaskDirectPii(User user)
    {
        return new UserPublicProfileDto {
            UserId = user.UserId,
            DisplayName = user.DisplayName ?? "Học viên ẩn danh",
            MaskedEmail = MaskEmail(user.Email),
            AvatarUrl = user.AvatarUrl
        };
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return "***";
        var parts = email.Split('@');
        var name = parts[0];
        var maskedName = name.Length > 2 ? $"{name[0]}***{name[^1]}" : "***";
        return $"{maskedName}@{parts[1]}";
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `PM-G01` | M01 là nơi DUY NHẤT lưu trữ dữ liệu PII_DIRECT; các module khác CẤM lưu trùng lặp Email/SĐT. |
| `PM-G02` | Mọi bảng CSDL của M02, M03, M06, M10, M11 chỉ được lưu vết qua `UserId` định danh dạng GUID. |
| `PM-G03` | Địa chỉ IP và UserAgent lưu trong Sổ Kiểm toán M11 BẮT BUỘC băm SHA-256 kèm Salt (`SaltedIPHash`). |
| `PM-G04` | Dữ liệu Egress sang AI Gemini (M12) tuyệt đối không chứa PII_DIRECT hoặc PII_INDIRECT (D-070). |
| `PM-G05` | Mật khẩu băm (BCrypt) và Refresh Tokens CẤM xuất hiện trong bất kỳ DTO response hoặc API công khai nào. |
| `PM-G06` | Bảng xếp hạng M09 chỉ dùng `DisplayName` và `AvatarUrl`, cấm hiển thị Email hoặc SĐT người học. |
| `PM-G07` | 100% luồng luân chuyển dữ liệu PII phải đăng ký trong Bản đồ PII Liên Module M01-T033. |
| `PM-G08` | Phân quyền truy cập tra cứu dữ liệu PII thô chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin` (REL-01). |
| `PM-G09` | SLA thực thi hàm che mờ PII (`MaskDirectPii`) $< 1\text{ms}$. |
| `PM-G10` | 100% các test case tự kiểm PM33-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PM33-01` | Quét toàn bộ CSDL M02, M03, M06 kiểm tra lưu trữ Email người học | 0 bảng nào chứa cột Email, 100% dùng `UserId` GUID |
| `PM33-02` | Gọi API lấy thông tin Profile công khai của User A | Trả về Email đã che mờ `n***h@gmail.com` và DisplayName |
| `PM33-03` | Tra cứu vết địa chỉ IP trong Sổ Kiểm toán M11 sau khi đăng nhập | Trả về chuỗi Salted Hash SHA-256 16 ký tự, 0 IP thô |
| `PM33-04` | Kiểm tra Prompt gửi sang AI Gemini từ Module M12 | `PromptAnonymizerFilter` xóa toàn bộ thông tin cá nhân |
| `PM33-05` | Thử truy vấn cột PasswordHash qua DTO response công khai | Field không tồn tại trong DTO contract |
| `PM33-06` | Truy vấn thông tin người học trên Bảng xếp hạng M09 | Chỉ hiển thị DisplayName bí danh, 0 lộ Email/SĐT |
| `PM33-07` | Đăng xuất khỏi thiết bị (Logout) | Token FCM Push trong M10 bị de-activate ngay SLA $\le 10\text{s}$ |
| `PM33-08` | Tra cứu vết Audit Log M11 khi Admin tra cứu PII thô | Ghi nhận Audit Event `ACT-M11-33` đính kèm TicketId |
| `PM33-09` | Kiểm tra thời gian che mờ PII cho 1000 người học | Processing time $< 3\text{ms}$ |
| `PM33-10` | Kiểm tra mã hóa CSDL M01 cho các trường sensitive | Dữ liệu `PasswordHash` và `SecurityStamps` mã hóa at rest |
| `PM33-11` | Thử nộp bản ghi PII có định dạng SĐT không hợp lệ | System reject 400 `INVALID_PHONE_FORMAT` |
| `PM33-12` | Tải đồng thời 100 request tra cứu PII liên module | 100% luồng luân chuyển tuân thủ đúng PII Map |
| `PM33-13` | SupportAgent thử xem mật khẩu băm của người dùng | Deny 403 Forbidden |
| `PM33-14` | User chưa đăng nhập gọi API tra cứu PII liên module | Deny 401 Unauthorized |
| `PM33-15` | Sửa thông tin Họ tên tại M01 | M01 cập nhật Master DB, các module khác không cần đổi dữ liệu |
| `PM33-16` | Tra cứu danh sách tất cả các điểm đụng PII trong hệ thống | Trả về Bản đồ PII M01-T033 hoàn chỉnh |
| `PM33-17` | Phân tích tham chiếu các thuộc tính PII trong CSDL PostgreSQL | Quét schema CSDL M01-M12 (T020) |
| `PM33-18` | Thao tác che mờ PII bị ném exception do chuỗi null | Fallback trả về chuỗi ẩn danh mặc định `"***"` |
| `PM33-19` | Yêu cầu xuất dữ liệu cá nhân (GDPR Export M01-T034) | Quét đủ 12 module theo đúng Bản đồ PII M01-T033 |
| `PM33-20` | Kiểm thử hoàn tất luồng bản đồ PII liên module M01-CROSS-MODULE-PII-MAP-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-PM-I01` | Một số bảng log Serilog hiện tại vẫn ghi địa chỉ IP dạng thô | Nguy cơ vi phạm quy định bảo mật quyền riêng tư REL-01 | M01-T049 (Source task) |
| `M01-PM-I02` | Thiếu bộ lọc `PromptAnonymizerFilter` trên một số luồng AI M12 | Nguy cơ lỡ tay truyền tên người học sang AI Gemini | M01-T049; M12-T042-A |
| `M01-PM-I03` | Chưa có hàm `AnonymizeIpAddress` băm Salted SHA-256 chuẩn | Các IP trong M11 hiện đang lưu dạng plaintext | M01-T049; M11-T031 |
| `M01-PM-I04` | Thiếu validation cấm lưu email/SĐT trong CSDL M02 và M06 | Rủi ro nhân bản PII gây khó khăn khi xử lý yêu cầu xóa tài khoản | M01-T049 |
| `M01-PM-I05` | Chưa có API tập hợp danh mục PII phục vụ cho GDPR Export | Khó khăn khi triển khai task xuất/xóa dữ liệu M01-T034/T035 | M01-T049; M01-T034 |

- `M01-PM-F01`: Triển khai `CrossModulePiiService` với Salted SHA-256 IP Hashing (tiếp nhận: M01-T049).
- `M01-PM-F02`: Tích hợp Bắt buộc `GUID-Only Reference` trên toàn bộ DB Schemas M02-M12 (tiếp nhận: M01-T049; REL-01).
- `M01-PM-F03`: Kết nối PII Map với luồng GDPR Data Export / Erase (tiếp nhận: M01-T049; M01-T034/T035).
- `M01-PM-F04`: Thiết lập bộ kiểm thử tự động PM-G01–G10 và PM33-01–20 (tiếp nhận: M01 tasks).
- `M01-PM-F05`: Thu thập bằng chứng runtime cho luồng bản đồ PII M01 (tiếp nhận: M01 tasks; A-G01/A-G05).

## 8. Tự kiểm M01-T033

- Đã thiết kế hoàn chỉnh `M01-CROSS-MODULE-PII-MAP-1.0` với Phân loại 3 Cấp độ PII (`PII_DIRECT`, `PII_INDIRECT`, `PII_SENSITIVE`).
- Đã chốt Ràng buộc Cách ly Tham chiếu Định danh `GUID-Only` liên module.
- Đã chốt Ràng buộc 0 Rò rỉ PII sang AI Gemini (M12) và Băm Salted SHA-256 IP cho Audit Log M11.
- Đã lồng ghép Đồng bộ Vòng đời Xuất/Xóa Dữ liệu Cá nhân GDPR/PDPA (REL-07).
- Đã xác lập 10 Regression Gates (`PM-G01`–`PM-G10`) và 20 Test Cases tự kiểm (`PM33-01`–`PM33-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả lập bản đồ dữ liệu cá nhân liên module M01-T033 | WSA-7K2 |
