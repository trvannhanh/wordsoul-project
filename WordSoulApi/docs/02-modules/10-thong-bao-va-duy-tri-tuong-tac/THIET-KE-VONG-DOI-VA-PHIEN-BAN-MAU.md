# Thiết kế vòng đời và phiên bản mẫu M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-TEMPLATE-LIFECYCLE-VERSIONING-1.0` |
| Task | M10-T012 |
| Đầu vào | M10-NOTIFICATION-TEMPLATE-SPEC-1.0 (M10-T011) |
| Phạm vi | Quản lý vòng đời phát hành và phiên bản (Draft $\to$ Active $\to$ Deprecated) của mẫu nội dung thông báo |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế quản lý phiên bản mẫu thông báo để tránh phá vỡ giao diện trên các bản ứng dụng di động cũ.

- **Tính Bất biến của Thông báo Đã Tạo (`Historical Inbox Content Invariant`)**: Khi Admin cập nhật một `NotificationTemplate` lên phiên bản mới ($v1 \to v2$), các thông báo cũ đã chèn vào `NotificationInbox` BẮT BUỘC giữ nguyên nội dung chụp lại (Snapshot Title/Body) tại thời điểm tạo.
- **Ràng buộc Mẫu Đã Duyệt (`Approved Template Invariant`)**: CHỈ các mẫu nội dung ở trạng thái `ACTIVE` mới được tiêu thụ để render thông báo PUSH/In-App.

## 2. Dynamic Template Versioning Table

```csharp
public class NotificationTemplateVersion
{
    public Guid TemplateId { get; set; }
    public string TemplateCode { get; set; }
    public int Version { get; set; }
    
    public string TitlePattern { get; set; }
    public string BodyPattern { get; set; }
    
    public TemplateStatus Status { get; set; } // DRAFT, ACTIVE, DEPRECATED
    public DateTime EffectiveFromUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TV-G01`: 100% bản ghi thông báo trong `NotificationInbox` lưu giữ đúng Snapshot nội dung rendered, không bị đổi theo template mới.
- `TV-G02`: Thử áp dụng template ở trạng thái `DRAFT` bị chặn hoàn toàn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TV12-01` | Admin cập nhật nội dung template `TPL_DUE_REVIEW` từ v1 sang v2 | Thông báo mới áp dụng mẫu v2, thông báo cũ trong Inbox giữ nguyên mẫu v1. |
| `TV12-02` | Gọi render với template `DEPRECATED` | System fallback về phiên bản `ACTIVE` gần nhất. |
| `TV12-03` | Kiểm thử hoàn tất luồng M10-TEMPLATE-LIFECYCLE-VERSIONING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-TV-F01` | Cần thuộc tính `TemplateVersion` trong `NotificationInbox` | Phục vụ đối soát và phân tích | M10-T013 |

## 5. Tự kiểm M10-T012
- Đã đặc tả thiết kế vòng đời và phiên bản mẫu M10-T012.
- Ghi nhận 2 Regression Gates (`TV-G01`–`TV-G02`) và 3 Test Cases (`TV12-01`–`TV12-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế vòng đời và phiên bản mẫu M10-T012 | WSA-7K2 |
