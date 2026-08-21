# Xây dựng ma trận lựa chọn nhận tin M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-PREFERENCE-MATRIX-1.0` |
| Task | M10-T007 |
| Đầu vào | M10-NOTIFICATION-TAXONOMY-1.0 (M10-T002), REL-06 |
| Phạm vi | Cấu trúc ma trận tùy chọn bật/tắt nhận thông báo (`UserNotificationPreferences`) theo từng kênh (In-App, Push, Email) và từng nhóm nội dung |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cấu trúc ma trận và logic áp dụng tùy chọn nhận thông báo của người học.

- **Ngoại lệ An ninh Bắt buộc (`Mandatory Security Alert Exception Invariant`)**: Nhóm cảnh báo an ninh `SECURITY` (đăng nhập thiết bị mới, đổi mật khẩu) BẮT BUỘC luôn ở trạng thái BẬT (`IsPushEnabled = true, IsEmailEnabled = true`). Người dùng CẤM tuyệt đối việc Opt-Out khỏi nhóm này.
- **Giá trị Mặc định Minh bạch (`Default Opt-In Policy Invariant`)**: Khi người dùng mới khởi tạo tài khoản, các nhóm `STUDY` và `REWARD` được mặc định BẬT Push và In-App để đảm bảo duy trì tương tác học tập.

## 2. Dynamic User Notification Preferences Schema

```csharp
public class UserNotificationPreference
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }
    
    public string CategoryCode { get; set; } // SECURITY, STUDY, REWARD, SYSTEM
    
    public bool IsInAppEnabled { get; set; } = true;
    public bool IsPushEnabled { get; set; } = true;
    public bool IsEmailEnabled { get; set; } = false;
    
    public DateTime UpdatedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `NP-G01`: 100% request cập nhật Opt-Out cho nhóm `SECURITY` bị hệ thống từ chối với lỗi HTTP 400 `CANNOT_OPT_OUT_SECURITY_ALERTS`.
- `NP-G02`: Người dùng Opt-Out nhóm `STUDY` vẫn nhận được tin PUSH khi có sự kiện cảnh báo `SECURITY`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `NP07-01` | Người dùng thử tắt tin PUSH nhóm `SECURITY` trong Settings | System reject với lỗi `CANNOT_OPT_OUT_SECURITY_ALERTS`. |
| `NP07-02` | Người dùng tắt tin PUSH nhóm `STUDY` | Bảng `UserNotificationPreferences` ghi nhận `IsPushEnabled = false` cho `STUDY`. |
| `NP07-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-PREFERENCE-MATRIX-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-NP-F01` | Cần Seeder tạo 4 bản ghi preferences mặc định cho người dùng mới | Khởi tạo ngay khi đăng ký tài khoản M01 | M10-T008 |

## 5. Tự kiểm M10-T007
- Đã đặc tả xây dựng ma trận lựa chọn nhận tin M10-T007.
- Ghi nhận 2 Regression Gates (`NP-G01`–`NP-G02`) và 3 Test Cases (`NP07-01`–`NP07-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xây dựng ma trận lựa chọn nhận tin M10-T007 | WSA-7K2 |
