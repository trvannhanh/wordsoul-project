# Chốt đọc, mở và đọc tất cả đúng quyền M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-INBOX-MARK-READ-OPERATIONS-1.0` |
| Task | M10-T018 |
| Đầu vào | M10-INBOX-LIST-UNREAD-COUNT-1.0 (M10-T017) |
| Phạm vi | Các API thao tác đánh dấu đã đọc thông báo (`PUT /api/v1/notifications/{id}/read`, `POST /api/v1/notifications/read-all`) bảo đảm đúng quyền sở hữu người dùng |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định các điều kiện an ninh và xử lý dữ liệu khi đánh dấu đã đọc thông báo.

- **Ràng buộc Quyền Sở hữu Thông báo (`Inbox Authorization Invariant`)**: Người học CHỈ CÓ QUYỀN đánh dấu đã đọc cho các thông báo thuộc sở hữu của chính mình (`TargetUserId == CurrentUserId`). Thao tác trên thông báo của người khác BẮT BUỘC bị chặn với lỗi HTTP 403 `FORBIDDEN_NOTIFICATION_ACCESS`.
- **Cập nhật Mốc Đọc Tất cả An toàn (`Mark All Read Timestamp Guard Invariant`)**: Thao tác "Đọc tất cả" (`read-all`) BẮT BUỘC cập nhật `IsRead = true` dựa trên mốc thời gian chốt `ReadCutoffUtc = UtcNow`. Các thông báo mới chèn vào ngay sau đó CẤM bị đánh dấu đọc nhầm.

## 2. Dynamic Mark Read Operations Logic

```csharp
public async Task MarkAllAsReadAsync(Guid userId)
{
    var cutoffUtc = DateTime.UtcNow;
    
    // Cập nhật CSDL cho các tin chưa đọc sinh ra trước mốc cutoff
    var unreadItems = await _dbContext.NotificationInbox
        .Where(n => n.TargetUserId == userId && !n.IsRead && n.CreatedAtUtc <= cutoffUtc)
        .ToListAsync();
        
    foreach (var item in unreadItems)
    {
        item.IsRead = true;
        item.ReadAtUtc = cutoffUtc;
    }
    
    await _dbContext.SaveChangesAsync();
    
    // Reset cache unread count về 0
    await _cache.SetAsync($"user_unread_count_{userId}", 0);
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `MR-G01`: 100% request đánh dấu đọc thông báo của người dùng khác bị trả lỗi HTTP 403.
- `MR-G02`: Gọi `read-all` cập nhật thành công toàn bộ tin chưa đọc và reset `UnreadCount` về 0.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MR18-01` | Người dùng A gọi `PUT /notifications/{id_cua_B}/read` | System reject với lỗi HTTP 403 `FORBIDDEN_NOTIFICATION_ACCESS`. |
| `MR18-02` | Gọi `POST /notifications/read-all` khi có 5 tin chưa đọc | Cập nhật 5 tin thành đã đọc, `UnreadCount = 0`. |
| `MR18-03` | Kiểm thử hoàn tất luồng M10-INBOX-MARK-READ-OPERATIONS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-MR-F01` | Cần bổ sung Audit Log khi thực hiện `read-all` | Đảm bảo tính minh bạch dữ liệu | M10-T019 |

## 5. Tự kiểm M10-T018
- Đã đặc tả chốt đọc, mở và đọc tất cả đúng quyền M10-T018.
- Ghi nhận 2 Regression Gates (`MR-G01`–`MR-G02`) và 3 Test Cases (`MR18-01`–`MR18-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt đọc, mở và đọc tất cả đúng quyền M10-T018 | WSA-7K2 |
