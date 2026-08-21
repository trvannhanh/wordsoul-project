# Chốt ẩn, xóa trải nghiệm và lưu giữ M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-INBOX-HIDE-DELETE-RETENTION-1.0` |
| Task | M10-T019 |
| Đầu vào | M10-INBOX-MARK-READ-OPERATIONS-1.0 (M10-T018) |
| Phạm vi | API ẩn/xóa thông báo khỏi giao diện Hộp thư người dùng (`DELETE /api/v1/notifications/{id}`) và chính sách lưu giữ dữ liệu (Retention Policy) 90 ngày |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy tắc ẩn thông báo trải nghiệm người dùng và thời hạn lưu giữ dữ liệu kiểm toán.

- **Chế độ Ẩn Mềm Trải nghiệm (`Soft-Delete UX Invariant`)**: Thao tác xóa thông báo của người dùng CHỈ CÓ TÁC DỤNG đánh dấu `IsHiddenByUser = true`. Dữ liệu bản ghi BẮT BUỘC được lưu giữ trong CSDL trong vòng $90$ ngày để phục vụ kiểm toán trước khi dọn dẹp cứng.
- **Tự động Dọn dẹp Hậu trường (`Retention Purge Invariant`)**: Cronjob `NotificationRetentionPurgeJob` chạy định kỳ 02:00 UTC mỗi ngày tự động xóa cứng các thông báo đã hết hạn TTL quá $90$ ngày.

## 2. Dynamic Retention Purge Logic

```csharp
public async Task PurgeExpiredNotificationsAsync()
{
    var retentionThreshold = DateTime.UtcNow.AddDays(-90);
    
    // Xóa cứng các thông báo hết hạn quá 90 ngày
    var expiredItems = await _dbContext.NotificationInbox
        .Where(n => n.ExpiresAtUtc <= retentionThreshold)
        .ToListAsync();
        
    _dbContext.NotificationInbox.RemoveRange(expiredItems);
    await _dbContext.SaveChangesAsync();
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `HDR-G01`: 100% thông báo gọi API `DELETE` được chuyển `IsHiddenByUser = true`, không xuất hiện lại trong danh sách Inbox.
- `HDR-G02`: Thông báo hết hạn quá 90 ngày được dọn dẹp sạch khỏi CSDL.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HDR19-01` | Người dùng bấm "Xóa" 1 thông báo trong Hộp thư | HTTP 200 OK, `IsHiddenByUser` = true, ẩn khỏi danh sách UI. |
| `HDR19-02` | Chạy Cronjob dọn dẹp dữ liệu lưu giữ trên CSDL | Xóa cứng các bản ghi `ExpiresAtUtc <= UtcNow - 90 days`. |
| `HDR19-03` | Kiểm thử hoàn tất luồng M10-INBOX-HIDE-DELETE-RETENTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-HDR-F01` | Cần thuộc tính `IsHiddenByUser` trong bảng `NotificationInbox` | Phục vụ Soft-Delete UX | M10-T020 |

## 5. Tự kiểm M10-T019
- Đã đặc tả chốt ẩn, xóa trải nghiệm và lưu giữ M10-T019.
- Ghi nhận 2 Regression Gates (`HDR-G01`–`HDR-G02`) và 3 Test Cases (`HDR19-01`–`HDR19-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt ẩn, xóa trải nghiệm và lưu giữ M10-T019 | WSA-7K2 |
