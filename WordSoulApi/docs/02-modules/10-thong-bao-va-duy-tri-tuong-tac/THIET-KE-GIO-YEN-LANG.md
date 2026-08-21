# Thiết kế giờ yên lặng M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-QUIET-HOURS-DEFERRAL-1.0` |
| Task | M10-T026 |
| Đầu vào | M10-EFFECTIVE-CHANGE-EMERGENCY-EXCEPTION-1.0 (M10-T010), M10-USER-TIMEZONE-EVALUATION-1.0 (M10-T025), REL-06 |
| Phạm vi | Thuật toán hoãn phát tin PUSH trong khoảng Giờ yên lặng (`22:00 - 07:00` giờ địa phương), quản lý hàng chờ hoãn Redis `DeferredPushQueue` |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả thuật toán kẹp giữ tin PUSH không làm phiền người học trong đêm.

- **Ràng buộc Khung Giờ Yên lặng chuẩn REL-06 (`REL-06 Quiet Hours Window Invariant`)**: Khung giờ yên tĩnh mặc định là từ **22:00:00 tối hôm trước đến 07:00:00 sáng hôm sau** (theo múi giờ địa phương của người học). Các tin Push thuộc nhóm `STUDY` và `REWARD` phát ra trong khoảng này BẮT BUỘC hoãn sang đúng 07:01:00 sáng hôm sau.
- **Ràng buộc Xóa tin Hoãn Hết hạn (`Deferred Push Expiration Invariant`)**: Nếu tin Push nằm trong hàng chờ hoãn có `ExpiresAtUtc <= 07:01`, task gửi Push tự động bị hủy để tránh gửi tin hết hiệu lực.

## 2. Dynamic Quiet Hours Evaluation Logic

```csharp
public bool IsInQuietHours(DateTime localTime)
{
    int hour = localTime.Hour;
    // Khung 22:00 - 07:00 (từ 22h tối đến 7h sáng)
    return hour >= 22 || hour < 7;
}

public DateTime GetNextMorning7AmUtc(string userTimeZoneId)
{
    var tz = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
    var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    
    var nextMorning7AmLocal = localNow.Date.AddDays(localNow.Hour >= 7 ? 1 : 0).AddHours(7).AddMinutes(1);
    return TimeZoneInfo.ConvertTimeToUtc(nextMorning7AmLocal, tz);
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QH-G01`: 100% tin Push phát vào lúc 23:00 local time được đẩy vào `DeferredPushQueue` với thời gian chạy = 07:01 sáng hôm sau.
- `QH-G02`: Tin `SECURITY` phát vào lúc 03:00 sáng không bị hoãn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QH26-01` | Nhắc từ vựng đến hạn lúc 22:30 (UTC+7) | Tạo In-App Inbox ngay, hoãn Push sang 07:01 sáng hôm sau. |
| `QH26-02` | Cảnh báo đăng nhập thiết bị mới lúc 02:00 sáng | Bỏ qua Giờ yên tĩnh, gửi Push ngay. |
| `QH26-03` | Kiểm thử hoàn tất luồng M10-QUIET-HOURS-DEFERRAL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-QH-F01` | Cần Cronjob `DeferredPushDispatcherJob` chạy mỗi phút để xả tin | Xả tin hoãn khi đến 07:01 sáng | M10-T027 |

## 5. Tự kiểm M10-T026
- Đã đặc tả thiết kế giờ yên lặng M10-T026.
- Ghi nhận 2 Regression Gates (`QH-G01`–`QH-G02`) và 3 Test Cases (`QH26-01`–`QH26-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế giờ yên lặng M10-T026 | WSA-7K2 |
