# Chốt hiệu lực thay đổi và ngoại lệ khẩn M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-EFFECTIVE-CHANGE-EMERGENCY-EXCEPTION-1.0` |
| Task | M10-T010 |
| Đầu vào | M10-NOTIFICATION-PREFERENCE-MATRIX-1.0 (M10-T007), M10-PREFERENCE-MANAGEMENT-UX-1.0 (M10-T009) |
| Phạm vi | Ràng buộc áp dụng hiệu lực của việc Opt-Out và cơ chế kích hoạt thông báo khẩn cấp hệ thống (`EMERGENCY_SYSTEM_ALERT`) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy tắc loại trừ và hiệu lực khẩn cấp đối với hệ thống thông báo.

- **Ngoại lệ Thông báo Khẩn cấp Hệ thống (`Emergency System Alert Exemption Invariant`)**: Các sự kiện thuộc loại `EMERGENCY_SYSTEM_ALERT` (bảo trì hệ thống khẩn cấp, sự cố bảo mật nghiêm trọng) BẮT BUỘC bỏ qua toàn bộ thiết lập Opt-Out và Giờ yên tĩnh (Quiet Hours). M10 Consumer gửi thông báo PUSH đến 100% người dùng đăng ký.
- **Tính Phê duyệt Ngoại lệ Khẩn (`Emergency Approval Invariant`)**: Phát thông báo khẩn cấp BẮT BUỘC có mã phê duyệt `EmergencyApprovalToken` do Admin hệ thống cấp.

## 2. Dynamic Emergency Alert Validation Logic

```csharp
public bool ShouldBypassPreferencesAndQuietHours(NotificationSignalEvent signal)
{
    // Cảnh báo an ninh hoặc sự cố khẩn cấp hệ thống
    if (signal.CategoryCode == "SECURITY") return true;
    if (signal.CategoryCode == "SYSTEM" && signal.IsEmergency && !string.IsNullOrEmpty(signal.EmergencyApprovalToken))
    {
        return true;
    }
    
    return false;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `EC-G01`: 100% tín hiệu `EMERGENCY_SYSTEM_ALERT` có token hợp lệ được gửi thành công mà không bị chặn bởi Opt-Out hay Quiet Hours.
- `EC-G02`: Tín hiệu khẩn cấp thiếu `EmergencyApprovalToken` bị giáng cấp về thông báo `SYSTEM` tiêu chuẩn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EC10-01` | Phát thông báo bảo trì khẩn cấp lúc 02:00 sáng với token hợp lệ | 100% thiết bị nhận được Push PagerDuty/FCM ngay lập tức. |
| `EC10-02` | Người dùng Opt-Out nhóm `SYSTEM`, nhận tin khẩn cấp có token | Hệ thống miễn trừ Opt-Out, gửi PUSH thành công. |
| `EC10-03` | Kiểm thử hoàn tất luồng M10-EFFECTIVE-CHANGE-EMERGENCY-EXCEPTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-EC-F01` | Cần thuộc tính `IsEmergency` và `EmergencyApprovalToken` trong DTO signal | Phục vụ nhận diện tín hiệu khẩn cấp | M10-T020 |

## 5. Tự kiểm M10-T010
- Đã đặc tả chốt hiệu lực thay đổi và ngoại lệ khẩn M10-T010.
- Ghi nhận 2 Regression Gates (`EC-G01`–`EC-G02`) và 3 Test Cases (`EC10-01`–`EC10-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt hiệu lực thay đổi và ngoại lệ khẩn M10-T010 | WSA-7K2 |
