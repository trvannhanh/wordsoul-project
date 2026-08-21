# Đặc tả bằng chứng đồng ý M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-CONSENT-EVIDENCE-AUDIT-1.0` |
| Task | M10-T008 |
| Đầu vào | M10-NOTIFICATION-PREFERENCE-MATRIX-1.0 (M10-T007) |
| Phạm vi | Mô hình lưu trữ nhật ký kiểm toán bằng chứng chấp thuận/rút đồng ý nhận thông báo (`NotificationConsentLogs`) tuân thủ quy định bảo vệ dữ liệu cá nhân |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình lưu trữ nhật ký bằng chứng đồng ý tùy chọn nhận tin của người học.

- **Tính Bất biến của Nhật ký Đồng ý (`Append-Only Consent Audit Invariant`)**: Bảng `NotificationConsentLogs` BẮT BUỘC chỉ tiếp nhận lệnh `INSERT`. CẤM xóa hoặc chỉnh sửa bản ghi lịch sử thay đổi tùy chọn Push/Email của người dùng.
- **Ràng buộc Lưu trữ Bằng chứng Vấn tin (`Consent Proof Schema Invariant`)**: Mỗi bản ghi thay đổi BẮT BUỘC chứa: `UserId`, `CategoryCode`, `ChannelType`, `Action` (`OPT_IN`, `OPT_OUT`), `ClientIpAddress`, `DeviceAgent` và `TimestampUtc`.

## 2. Dynamic Consent Log Schema

```csharp
public class NotificationConsentLog
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }
    
    public string CategoryCode { get; set; }
    public ChannelType Channel { get; set; }
    public ConsentAction Action { get; set; } // OPT_IN, OPT_OUT
    
    public string ClientIpAddress { get; set; }
    public string DeviceAgent { get; set; }
    
    public DateTime TimestampUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `CE-G01`: 100% thao tác Opt-Out/Opt-In trên Settings tự động tạo bản ghi trong `NotificationConsentLogs`.
- `CE-G02`: Thử gọi lệnh `DELETE` trên bảng `NotificationConsentLogs` bị CSDL chặn hoàn toàn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CE08-01` | Người dùng tắt tin Push nhóm `REWARD` trên thiết bị iOS | Tạo 1 bản ghi `NotificationConsentLog` với `Action = OPT_OUT`. |
| `CE08-02` | Truy vấn lịch sử đồng ý của người dùng | Trả về chuỗi các bản ghi append-only theo thứ tự thời gian. |
| `CE08-03` | Kiểm thử hoàn tất luồng M10-CONSENT-EVIDENCE-AUDIT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-CE-F01` | Thêm DB Trigger chặn DELETE/UPDATE trên bảng `NotificationConsentLogs` | Đảm bảo tính pháp lý audit log | M10-T009 |

## 5. Tự kiểm M10-T008
- Đã đặc tả bằng chứng đồng ý M10-T008.
- Ghi nhận 2 Regression Gates (`CE-G01`–`CE-G02`) và 3 Test Cases (`CE08-01`–`CE08-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả bằng chứng đồng ý M10-T008 | WSA-7K2 |
