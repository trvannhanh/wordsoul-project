# Chuẩn hóa múi giờ và giờ học mong muốn — lát A M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-TIMEZONE-PREFERENCE-A-1.0` |
| Task | M01-T025-A |
| Đầu vào | M01-PROFILE-ACCESS-A-1.0 (D-087), M10-NOTIFICATION-SCHEDULE-1.0, REL-06 |
| Phạm vi | Quy trình chuẩn hóa Múi giờ IANA (`TimeZoneId`), Giờ học mong muốn local (`PreferredLearningHourLocal`) và tự động quy đổi sang giờ UTC phục vụ lập lịch thông báo nhắc học REL-06 |
| Tự kiểm | A-G01; REL-06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chuẩn hóa Múi giờ và Thói quen Giờ học — Lát A (`Timezone & Preferred Learning Hour Protocol - Slice A`) thuộc M01, tạo nền tảng định thời chuẩn xác cho các hệ thống thông báo PUSH nhắc học (M10 / REL-06) theo đúng múi giờ địa phương của từng người học.

- **Chuẩn hóa Chuỗi Múi giờ IANA (`IANA Timezone Validation Invariant`)**: Thuộc tính `TimeZoneId` bắt buộc là một mã múi giờ IANA chuẩn hợp lệ (ví dụ: `"Asia/Ho_Chi_Minh"`, `"America/New_York"`). Giá trị mặc định nếu không truyền là `"Asia/Ho_Chi_Minh"`.
- **Tự động Quy đổi Giờ Học UTC (`Automatic UTC Conversion Invariant`)**: Hệ thống tự động chuyển đổi Giờ học mong muốn theo giờ địa phương (`PreferredLearningHourLocal` từ 0 đến 23) thành Giờ học UTC (`PreferredLearningHourUtc`) dựa trên offset múi giờ tại thời điểm lưu.
- **Ràng buộc Mục tiêu Học tập theo Ngày ($5 \le \text{Minutes} \le 300$)**: Thuộc tính `TargetStudyMinutesPerDay` đại diện cho mục tiêu thời gian học mỗi ngày của người học, bắt buộc nằm trong khoảng từ 5 phút đến 300 phút (5 giờ).
- **Đồng bộ Thông báo Nhắc học M10 / REL-06 (`Notification Scheduling Integration`)**: Khi người học thay đổi `TimeZoneId` hoặc `PreferredLearningHourLocal`, hệ thống phát sự kiện `UserTimezoneUpdatedIntegrationEvent` để Module M10 tính toán lại thời gian gửi PUSH notification.

## 2. Mô hình Thuộc tính Múi giờ và Giờ học (Timezone Attributes Schema)

```json
{
  "userId": 10024,
  "timeZoneId": "Asia/Ho_Chi_Minh",
  "utcOffsetMinutes": 420,
  "preferredLearningHourLocal": 20,
  "preferredLearningHourUtc": 13,
  "targetStudyMinutesPerDay": 30,
  "lastTimezoneUpdatedUtc": "2026-08-20T10:00:00Z"
}
```

## 3. Quy trình Tính toán Quy đổi Múi giờ (UTC Conversion Flow)

```
[User Submits Profile Preferences (TimeZoneId, PreferredLearningHourLocal)]
                                |
                                v
               [Validate IANA Timezone Identifier]
               - System.TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId)
                                |
                      +---------+---------+
                      | (Invalid ID)      | (Valid IANA ID)
                      v                   v
               [Reject 400        [Get Current UTC Offset]
                Invalid TZ]       - e.g. UTC+7 (+420 minutes)
                                          |
                                          v
                              [Calculate Hour UTC]
                              - PreferredLearningHourUtc = 
                                (LocalHour - OffsetHours + 24) % 24
                                          |
                                          v
                              [Save Settings to DB]
                              - Publish UserTimezoneUpdatedEvent
```

## 4. Giao thức Xử lý Cập nhật Múi giờ (UserTimezoneService)

```csharp
public async Task<UserTimezoneDto> UpdateUserTimezoneAsync(string currentUserId, UpdateTimezoneRequestDto dto)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    // 1. Validate IANA TimeZoneId
    TimeZoneInfo tzInfo;
    try
    {
        tzInfo = TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZoneId ?? "Asia/Ho_Chi_Minh");
    }
    catch (TimeZoneNotFoundException)
    {
        throw new ArgumentException($"INVALID_IANA_TIMEZONE_ID: Múi giờ '{dto.TimeZoneId}' không hợp lệ.");
    }

    // 2. Validate PreferredLearningHourLocal (0..23)
    if (dto.PreferredLearningHourLocal < 0 || dto.PreferredLearningHourLocal > 23)
    {
        throw new ArgumentException("PREFERRED_LEARNING_HOUR_MUST_BE_0_TO_23");
    }

    // 3. Validate TargetStudyMinutesPerDay (5..300)
    if (dto.TargetStudyMinutesPerDay < 5 || dto.TargetStudyMinutesPerDay > 300)
    {
        throw new ArgumentException("TARGET_MINUTES_MUST_BE_5_TO_300");
    }

    // 4. Quy đổi sang PreferredLearningHourUtc
    var nowUtc = DateTime.UtcNow;
    var localOffset = tzInfo.GetUtcOffset(nowUtc);
    int localOffsetHours = (int)localOffset.TotalHours;
    int utcHour = (dto.PreferredLearningHourLocal - localOffsetHours + 24) % 24;

    user.TimeZoneId = tzInfo.Id;
    user.PreferredLearningHourLocal = dto.PreferredLearningHourLocal;
    user.PreferredLearningHourUtc = utcHour;
    user.TargetStudyMinutesPerDay = dto.TargetStudyMinutesPerDay;
    user.LastTimezoneUpdatedUtc = nowUtc;

    await _db.SaveChangesAsync();

    // 5. Phát sự kiện tích hợp sang Module M10
    await _eventPublisher.PublishAsync(new UserTimezoneUpdatedIntegrationEvent
    {
        UserId = user.UserId,
        TimeZoneId = user.TimeZoneId,
        PreferredLearningHourUtc = user.PreferredLearningHourUtc,
        TargetStudyMinutesPerDay = user.TargetStudyMinutesPerDay
    });

    return MapToDto(user);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `TZ-G01` | Cấm lưu chuỗi `TimeZoneId` không thuộc danh mục múi giờ IANA chuẩn hợp lệ. |
| `TZ-G02` | Tự động quy đổi `PreferredLearningHourLocal` sang `PreferredLearningHourUtc` chuẩn xác theo offset. |
| `TZ-G03` | Cấm `PreferredLearningHourLocal` nhỏ hơn 0 hoặc lớn hơn 23. |
| `TZ-G04` | Cấm `TargetStudyMinutesPerDay` nhỏ hơn 5 phút hoặc lớn hơn 300 phút. |
| `TZ-G05` | Cập nhật múi giờ thành công tự động phát sự kiện tích hợp `UserTimezoneUpdatedIntegrationEvent`. |
| `TZ-G06` | Mặc định `TimeZoneId = "Asia/Ho_Chi_Minh"` nếu người dùng chưa từng thiết lập múi giờ. |
| `TZ-G07` | 100% thao tác thay đổi múi giờ ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-05`). |
| `TZ-G08` | Phân quyền thực hiện cập nhật múi giờ tuân thủ ma trận vai trò M01-T028. |
| `TZ-G09` | SLA thực thi API quy đổi và lưu múi giờ $< 25\text{ms}$. |
| `TZ-G10` | 100% các test case tự kiểm TZ25-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TZ25-01` | Đặt múi giờ `"Asia/Ho_Chi_Minh"` (UTC+7) và giờ học local 20:00 | Tính toán `preferredLearningHourUtc = 13` (13:00 UTC) |
| `TZ25-02` | Đặt múi giờ `"America/New_York"` (UTC-4) và giờ học local 20:00 | Tính toán `preferredLearningHourUtc = 0` (00:00 UTC) |
| `TZ25-03` | Thử đặt `TimeZoneId = "Invalid/Timezone_Name"` | Reject 400 `INVALID_IANA_TIMEZONE_ID` |
| `TZ25-04` | Thử đặt `preferredLearningHourLocal = 25` ($> 23$) | Reject 400 `PREFERRED_LEARNING_HOUR_MUST_BE_0_TO_23` |
| `TZ25-05` | Thử đặt `targetStudyMinutesPerDay = 2` ($< 5$) | Reject 400 `TARGET_MINUTES_MUST_BE_5_TO_300` |
| `TZ25-06` | Thử đặt `targetStudyMinutesPerDay = 500` ($> 300$) | Reject 400 `TARGET_MINUTES_MUST_BE_5_TO_300` |
| `TZ25-07` | Đặt mục tiêu học tập 45 phút/ngày | Cập nhật `TargetStudyMinutesPerDay = 45` thành công |
| `TZ25-08` | Đặt giờ học local 00:00 (Nửa đêm) tại múi giờ UTC+7 | Tính toán `preferredLearningHourUtc = 17` (17:00 UTC ngày trước) |
| `TZ25-09` | Kiểm tra sự kiện tích hợp phát ra sau khi đổi múi giờ | M10 nhận được `UserTimezoneUpdatedIntegrationEvent` |
| `TZ25-10` | Nạp thông tin múi giờ cho tài khoản mới chưa cài đặt | Trả về `TimeZoneId = "Asia/Ho_Chi_Minh"`, `PreferredLearningHourLocal = 20` |
| `TZ25-11` | Tra cứu vết Audit Log M11 sau khi cập nhật múi giờ | Ghi nhận Audit Event `ACT-M11-05` với offset chi tiết |
| `TZ25-12` | Tải đồng thời 50 request cập nhật múi giờ từ 50 tài khoản | Response latency p95 $< 30\text{ms}$ |
| `TZ25-13` | Người học A thử đổi múi giờ của Người học B | Deny 403 Forbidden |
| `TZ25-14` | User chưa đăng nhập thử gọi API cập nhật múi giờ | Deny 401 Unauthorized |
| `TZ25-15` | Đổi múi giờ trong thời điểm Daylight Saving Time (DST) của Mỹ | Tự động tính toán đúng offset DST tại mốc thời gian đó |
| `TZ25-16` | Kiểm tra thời gian phản hồi API lấy múi giờ cá nhân | Response latency $< 15\text{ms}$ |
| `TZ25-17` | Phân tích tham chiếu các job lập lịch PUSH M10 khi đổi múi giờ | Quét lịch nhắc học hiện tại trong Module M10 (REL-06) |
| `TZ25-18` | Thao tác cập nhật múi giờ bị gián đoạn do lỗi DB | Rollback transaction, giữ nguyên múi giờ cũ |
| `TZ25-19` | Đặt múi giờ `"Europe/London"` (UTC+0/1) và giờ học local 08:00 | Tính toán `preferredLearningHourUtc = 7` (hoặc 8 theo DST) |
| `TZ25-20` | Kiểm thử hoàn tất luồng chuẩn hóa múi giờ M01-TIMEZONE-PREFERENCE-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-TZ-I01` | Entity `User.cs` chưa có các thuộc tính `TimeZoneId` và `PreferredLearningHourUtc` | Không có chỗ lưu dữ liệu múi giờ để M10 lập lịch PUSH | M01-T049 (Source task) |
| `M01-TZ-I02` | Chưa có bộ quy đổi tự động từ local hour sang UTC hour | Lịch nhắc học M10 bị gửi sai giờ khi người dùng ở nước ngoài | M01-T049; M10-T008 |
| `M01-TZ-I03` | Thiếu validation chuỗi IANA Timezone ID hợp lệ | Người dùng có thể truyền chuỗi ngẫu nhiên gây exception | M01-T049 |
| `M01-TZ-I04` | Thiếu validation khoảng mục tiêu học tập $5 \le N \le 300$ phút | Rủi ro chèn số âm hoặc số quá lớn gây lỗi UI | M01-T049 |
| `M01-TZ-I05` | Chưa phát sự kiện `UserTimezoneUpdatedIntegrationEvent` sang M10 | M10 không cập nhật lại cron schedule của nhắc học PUSH | M01-T049; M10-T008 |

- `M01-TZ-F01`: Thêm `TimeZoneId`, `PreferredLearningHourLocal`, `PreferredLearningHourUtc` vào `User.cs` (tiếp nhận: M01-T049).
- `M01-TZ-F02`: Triển khai `UserTimezoneService` quy đổi giờ UTC (tiếp nhận: M01-T049; REL-06).
- `M01-TZ-F03`: Phát sự kiện `UserTimezoneUpdatedIntegrationEvent` cho Module M10 (tiếp nhận: M01-T049; M10-T008).
- `M01-TZ-F04`: Thiết lập bộ kiểm thử tự động TZ-G01–G10 và TZ25-01–20 (tiếp nhận: M01 tasks).
- `M01-TZ-F05`: Thu thập bằng chứng runtime cho luồng múi giờ M01 (tiếp nhận: M01 tasks; A-G01/REL-06).

## 8. Tự kiểm M01-T025-A

- Đã thiết kế hoàn chỉnh `M01-TIMEZONE-PREFERENCE-A-1.0` với Giao thức Quy đổi Múi giờ Tự động.
- Đã chốt Ràng buộc Chuẩn hóa Chuỗi Múi giờ IANA (`TimeZoneId`).
- Đã chốt Ràng buộc Mục tiêu Học tập theo Ngày ($5 \le N \le 300$ phút).
- Đã lồng ghép Sự kiện Tích hợp `UserTimezoneUpdatedIntegrationEvent` gửi Module M10 (REL-06).
- Đã xác lập 10 Regression Gates (`TZ-G01`–`TZ-G10`) và 20 Test Cases tự kiểm (`TZ25-01`–`TZ25-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa múi giờ và giờ học M01-T025-A | WSA-7K2 |
