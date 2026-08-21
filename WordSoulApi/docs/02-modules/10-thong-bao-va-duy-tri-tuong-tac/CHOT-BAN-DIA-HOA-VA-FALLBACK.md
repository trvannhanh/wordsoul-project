# Chốt bản địa hóa và fallback M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-LOCALIZATION-FALLBACK-SPEC-1.0` |
| Task | M10-T013 |
| Đầu vào | M10-NOTIFICATION-TEMPLATE-SPEC-1.0 (M10-T011), M10-TEMPLATE-LIFECYCLE-VERSIONING-1.0 (M10-T012) |
| Phạm vi | Quy chuẩn đa ngôn ngữ (Multi-language Localization: `vi-VN`, `en-US`) cho nội dung thông báo và chiến lược fallback ngôn ngữ mặc định |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế dịch đa ngôn ngữ và fallback nội dung thông báo trong M10.

- **Ngôn ngữ Mặc định An toàn (`Default Language Fallback Invariant`)**: Nếu người dùng thiết lập ngôn ngữ chưa được hỗ trợ (ví dụ: `ja-JP`), hệ thống BẮT BUỘC fallback render nội dung theo ngôn ngữ mặc định `vi-VN`.
- **Ràng buộc Không Trộn Ngôn ngữ (`No Mixed Language Invariant`)**: Tiêu đề và nội dung của 1 bản ghi thông báo BẮT BUỘC render theo đúng 1 ngôn ngữ nhất quán. CẤM trộn lẫn câu tiếng Anh và tiếng Việt trong cùng một Push notification.

## 2. Dynamic Localization Rendering Logic

```csharp
public (string title, string body) RenderLocalizedNotification(string templateCode, string preferredCulture, Dictionary<string, string> args)
{
    // 1. Kiểm tra hỗ trợ culture, fallback vi-VN
    string culture = IsSupportedCulture(preferredCulture) ? preferredCulture : "vi-VN";
    
    // 2. Lấy bản ghi template theo culture
    var template = GetTemplatePattern(templateCode, culture);
    
    // 3. Nội suy biến động
    string title = Interpolate(template.TitlePattern, args);
    string body = Interpolate(template.BodyPattern, args);
    
    return (title, body);
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LF-G01`: 100% người dùng có `PreferredLanguage == "ja-JP"` nhận được thông báo ngôn ngữ `vi-VN` chuẩn.
- `LF-G02`: Không có bản ghi thông báo nào chứa placeholder chưa được thế giá trị (như `{Title}`).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LF13-01` | Người dùng chọn tiếng Anh `en-US`, nhận thông báo nhắc ôn | Render tiêu đề: "Review Time!", nội dung: "You have 15 items due." |
| `LF13-02` | Người dùng chọn ngôn ngữ chưa có dịch `fr-FR` | Fallback render tiếng Việt `vi-VN`. |
| `LF13-03` | Kiểm thử hoàn tất luồng M10-LOCALIZATION-FALLBACK-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-LF-F01` | Bổ sung trường `PreferredLanguage` trong User Profile M01 | Phục vụ đọc ngôn ngữ người dùng | M10-T014 |

## 5. Tự kiểm M10-T013
- Đã đặc tả chốt bản địa hóa và fallback M10-T013.
- Ghi nhận 2 Regression Gates (`LF-G01`–`LF-G02`) và 3 Test Cases (`LF13-01`–`LF13-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt bản địa hóa và fallback M10-T013 | WSA-7K2 |
