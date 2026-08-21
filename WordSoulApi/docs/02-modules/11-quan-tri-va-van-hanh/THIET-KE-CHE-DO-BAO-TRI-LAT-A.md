# Thiết kế chế độ bảo trì — lát A M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-MAINTENANCE-MODE-A-1.0` |
| Task | M11-T043-A |
| Đầu vào | M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), M10 Notification Contract, REL-03, REL-06 |
| Phạm vi | Đặc tả Giao thức Chế độ Bảo trì Lát A (`Maintenance Mode Protocol - Slice A`), 3 trạng thái bảo trì hệ thống, Middleware `MaintenanceModeMiddleware` chặn mutation/read tại Gateway, thông báo PUSH/Banner trước 30 phút qua M10 và lưu vết kiểm toán M11 |
| Tự kiểm | A-G06; REL-03, REL-06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chế độ Bảo trì Lát A (`Maintenance Mode Protocol - Slice A`) thuộc M11, xác lập khả năng chủ động đưa toàn bộ hoặc một phần hệ thống WordSoul vào trạng thái bảo trì có kiểm soát (Nâng cấp CSDL, Migration schema, thay thế phần cứng), đảm bảo trải nghiệm người dùng mượt mà và bảo vệ tính toàn vẹn dữ liệu (REL-03, REL-06).

- **Máy Trạng thái Chế độ Bảo trì 3 Cấp độ (`3 Maintenance Modes Invariant`)**:
  - `NORMAL_OPERATIONS`: Hệ thống hoạt động bình thường 100%.
  - `READ_ONLY_MAINTENANCE`: Chỉ cho phép các cuộc gọi truy vấn đọc (`GET`). Từ chối $100\%$ các cuộc gọi thay đổi dữ liệu (`POST`, `PUT`, `DELETE`, `PATCH`) với lỗi HTTP 503 `MAINTENANCE_READ_ONLY_ACTIVE`.
  - `FULL_MAINTENANCE_LOCKDOWN`: Từ chối $100\%$ truy cập từ người học thông thường. CHỈ cho phép địa chỉ IP thuộc danh sách Whitelist Admin (`AdminIpWhitelist`) và endpoint kiểm tra sức khỏe `/healthz`.
- **Ràng buộc Phát Thông báo Trước 30 Phút qua M10 (`Pre-Maintenance Broadcast Notification`)**: Trước khi kích hoạt chế độ bảo trì toàn phần, hệ thống BẮT BUỘC phát thông báo PUSH và Banner trên ứng dụng di động tới toàn bộ người dùng active 30 phút trước thời điểm nâng cấp (`PreNoticeMinutes = 30m`).
- **Giao thức Thực thi Chặn tại Middleware API Gateway (`Maintenance Middleware Enforcement`)**: Bộ lọc `MaintenanceModeMiddleware` đặt ở đầu API Pipeline kiểm tra cờ trạng thái bảo trì trong Redis Cache (SLA $\le 1\text{ms}$). Nếu chế độ bảo trì đang bật, ngắt cuộc gọi ngay tại Gateway và trả về DTO hướng dẫn chi tiết thời gian hoàn tất dự kiến (`EstimatedEndTimeUtc`).
- **Lưu vết Sổ Kiểm toán Chế độ Bảo trì M11 (`Maintenance Audit Trail`)**: $100\%$ các lần bật/tắt hoặc điều chỉnh lịch bảo trì hệ thống được ghi vết bất biến `ACT-M11-43-MAINT` trong Sổ Kiểm toán M11.

## 2. Ma trận Chế độ Bảo trì Hệ thống (Maintenance Mode Matrix)

| Trạng thái Bảo trì (`MaintenanceMode`) | Thao tác Đọc (`GET`) | Thao tác Ghi (`POST/PUT/DELETE`) | Ngoại lệ IP Whitelist | Kết quả HTTP Response | Kênh Thông báo M10 |
|---|---|---|---|---|---|
| `NORMAL_OPERATIONS` | Allowed | Allowed | N/A | HTTP 200 OK | Normal |
| `READ_ONLY_MAINTENANCE` | Allowed | **Blocked** | Admin Allowed | HTTP 503 `MAINTENANCE_READ_ONLY` | In-App Banner |
| `FULL_MAINTENANCE_LOCKDOWN` | **Blocked** | **Blocked** | **Admin Whitelist Only** | HTTP 503 `MAINTENANCE_FULL_LOCKDOWN` | PUSH + In-App Banner |

## 3. Kiến trúc Luồng Thực thi Chế độ Bảo trì M11 (Maintenance Engine Pipeline)

```
[Incoming HTTP Request at API Gateway]
                  |
                  v
[MaintenanceModeMiddleware: Check Redis Cache (SLA <= 1ms)]
                  |
    +-------------+-------------+
    | (Mode == NORMAL_OPERATIONS) | (Mode == READ_ONLY / FULL_LOCKDOWN)
    v                             v
[Pass Request to Controllers]   [Check Admin IP Whitelist / /healthz]
                                  |
                         +--------+--------+
                         | (Is Whitelisted)| (Not Whitelisted)
                         v                 v
                 [Allow Admin Access]  [REJECT: 503 MAINTENANCE_ACTIVE]
                                       - Include EstimatedEndTimeUtc
                                       - Record Audit Log ACT-M11-43
```

## 4. Giao thức Thực thi Middleware Bảo trì CSDL (MaintenanceModeMiddleware)

```csharp
public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;

    public MaintenanceModeMiddleware(RequestDelegate next, IConnectionMultiplexer redis, IConfiguration config)
    {
        _next = next;
        _redis = redis;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var db = _redis.GetDatabase();
        string modeStr = await db.StringGetAsync("wordsoul:system:maintenance_mode");
        var currentMode = Enum.TryParse<MaintenanceMode>(modeStr, out var m) ? m : MaintenanceMode.NORMAL_OPERATIONS;

        if (currentMode == MaintenanceMode.NORMAL_OPERATIONS)
        {
            await _next(context);
            return;
        }

        // Allow Health Check & Admin IP Whitelist
        string clientIp = context.Connection.RemoteIpAddress?.ToString();
        var adminIps = _config.GetSection("AdminIpWhitelist").Get<List<string>>() ?? new();
        
        if (context.Request.Path.StartsWithSegments("/healthz") || adminIps.Contains(clientIp))
        {
            await _next(context);
            return;
        }

        // READ_ONLY Mode: Block Mutations
        if (currentMode == MaintenanceMode.READ_ONLY_MAINTENANCE && context.Request.Method == "GET")
        {
            await _next(context);
            return;
        }

        // Block Request & Return 503 Maintenance DTO
        string estimatedEndTime = await db.StringGetAsync("wordsoul:system:maintenance_end_time");
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json";

        var responseDto = new MaintenanceStatusDto {
            Mode = currentMode.ToString(),
            Message = "Hệ thống đang được nâng cấp bảo trì định kỳ. Vui lòng quay lại sau.",
            EstimatedEndTimeUtc = estimatedEndTime
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(responseDto));
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `MM-G01` | Middleware bảo trì BẮT BUỘC đánh chặn $100\%$ các cuộc gọi tại API Gateway dựa trên cờ trạng thái Redis. |
| `MM-G02` | Chế độ `READ_ONLY_MAINTENANCE` từ chối $100\%$ thao tác POST/PUT/DELETE với mã HTTP 503ServiceUnavailable. |
| `MM-G03` | Chế độ `FULL_MAINTENANCE_LOCKDOWN` từ chối $100\%$ truy cập từ người học ngoại trừ Admin IP Whitelist và `/healthz`. |
| `MM-G04` | Kích hoạt chế độ bảo trì BẮT BUỘC phát tin PUSH/In-App Banner thông báo trước 30 phút qua Module M10 (REL-06). |
| `MM-G05` | Phản hồi HTTP 503 BẮT BUỘC chứa thông điệp hướng dẫn và thời gian hoàn tất dự kiến `EstimatedEndTimeUtc`. |
| `MM-G06` | 100% các sự kiện bật/tắt hoặc thay đổi chế độ bảo trì được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-43-MAINT`). |
| `MM-G07` | SLA thực thi kiểm tra cờ bảo trì tại Middleware API Gateway $< 1.0\text{ms}$ per request. |
| `MM-G08` | Phân quyền thay đổi chế độ bảo trì hệ thống chỉ dành cho `SecurityAdmin` và `SuperAdmin`. |
| `MM-G09` | Chịu tải đến 20,000 requests/giây bị từ chối 503 mà không làm treo CPU của API Gateway nodes. |
| `MM-G10` | 100% các test case tự kiểm MM43-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MM43-01` | Gửi HTTP GET tới API từ vựng khi đang ở `NORMAL_OPERATIONS` | Trả về 200 OK bình thường |
| `MM43-02` | Gửi HTTP POST tạo bộ từ mới khi đang ở `READ_ONLY_MAINTENANCE` | Reject 503 `MAINTENANCE_READ_ONLY`, kèm EstimatedEndTime |
| `MM43-03` | Gửi HTTP GET đọc bài học khi đang ở `READ_ONLY_MAINTENANCE` | Cho phép đọc bình thường 200 OK |
| `MM43-04` | Gửi HTTP GET tới API từ vựng từ IP thường khi đang ở `FULL_MAINTENANCE_LOCKDOWN` | Reject 503 `MAINTENANCE_FULL_LOCKDOWN` |
| `MM43-05` | Gửi HTTP GET tới API từ vựng từ IP thuộc Admin Whitelist khi đang ở `FULL_MAINTENANCE` | Cho phép truy cập 200 OK (Admin Bypass) |
| `MM43-06` | Tra cứu vết Audit Log M11 sau khi bật chế độ `FULL_MAINTENANCE_LOCKDOWN` | Ghi nhận Audit Event `ACT-M11-43-MAINT` đính kèm Mode |
| `MM43-07` | Gọi endpoint `/healthz` từ IP bất kỳ trong chế độ `FULL_MAINTENANCE` | Trả về 200 OK Healthy (Health Check Bypass) |
| `MM43-08` | Lên lịch bảo trì trước 30 phút thông qua API M11 | Phát sự kiện PUSH qua M10 thông báo tới người dùng active |
| `MM43-09` | Tải đồng thời 5,000 request bị từ chối 503 trong chế độ bảo trì | Processing latency p95 $< 1.2\text{ms}$ per request |
| `MM43-10` | Chuyển chế độ bảo trì từ `FULL_MAINTENANCE` về `NORMAL_OPERATIONS` | Xóa cờ Redis SLA $< 50\text{ms}$, thông quan mọi request |
| `MM43-11` | Thử nạp chế độ bảo trì không nằm trong Enum chuẩn | Reject 400 `INVALID_MAINTENANCE_MODE` |
| `MM43-12` | Gửi request bật chế độ bảo trì khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `MM43-13` | User không phải SecurityAdmin/SuperAdmin thử bật chế độ bảo trì | Deny 403 Forbidden |
| `MM43-14` | User chưa đăng nhập gọi API cấu hình chế độ bảo trì | Deny 401 Unauthorized |
| `MM43-15` | Cập nhật `EstimatedEndTimeUtc` trong lúc đang bảo trì | Phản hồi 503 cập nhật thời gian dự kiến mới chính xác |
| `MM43-16` | Kiểm tra thời gian cập nhật cờ bảo trì trên Redis Cluster | Propagation SLA $< 100\text{ms}$ |
| `MM43-17` | Phân tích tham chiếu các bản ghi `MaintenanceHistory` trong CSDL | Quét schema `M11_MaintenanceHistories` (T020) |
| `MM43-18` | Dịch vụ Redis bị gián đoạn khi kiểm tra cờ bảo trì | Fallback đọc cấu hình local config, giữ nguyên Normal |
| `MM43-19` | Tra cứu trạng thái bảo trì hiện tại của hệ thống từ Client API | Trả về DTO trạng thái bảo trì và thông điệp |
| `MM43-20` | Kiểm thử hoàn tất luồng chế độ bảo trì Lát A M11-MAINTENANCE-MODE-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-MM-I01` | M11 hiện tại chưa có `MaintenanceModeMiddleware` ở Gateway | Risk không chặn được các request mutation khi sửa DB | M11-T049 (Source task) |
| `M11-MM-I02` | Thiếu cờ phân định 3 Chế độ Bảo trì (`NORMAL`, `READ_ONLY`, `FULL`) | Không có chế độ chỉ đọc cho học viên học bài trong lúc bảo trì | M11-T049; REL-03 |
| `M11-MM-I03` | Thiếu luồng phát tin PUSH/In-App Banner thông báo trước 30m qua M10 | Người học bị ngắt quãng đột ngột không báo trước | M11-T049; REL-06 |
| `M11-MM-I04` | Thiếu luồng Admin IP Whitelist & Health Check Bypass | Kỹ sư không truy cập được vào hệ thống để thao tác fix lỗi | M11-MM-F04; M11-T004 |
| `M11-MM-I05` | Chưa kết nối sự kiện chuyển chế độ bảo trì với Audit Log M11 (`ACT-M11-43-MAINT`) | Không ghi vết được ai là người đã bật/tắt bảo trì | M11-T049; M11-T031 |

- `M11-MM-F01`: Triển khai `MaintenanceModeMiddleware` tại API Gateway (tiếp nhận: M11-T049).
- `M11-MM-F02`: Tích hợp Bắt buộc 3 Maintenance Modes & HTTP 503 Response (tiếp nhận: M11-T049; REL-03).
- `M11-MM-F03`: Triển khai Pre-Maintenance Broadcast 30m Notification via M10 (tiếp nhận: M11-T049; REL-06).
- `M11-MM-F04`: Thiết lập bộ kiểm thử tự động MM-G01–G10 và MM43-01–20 (tiếp nhận: M11 tasks).
- `M11-MM-F05`: Thu thập bằng chứng runtime cho luồng bảo trì M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T043-A

- Đã thiết kế hoàn chỉnh `M11-MAINTENANCE-MODE-A-1.0` với Ma trận Chế độ Bảo trì Hệ thống.
- Đã chốt Ràng buộc Máy Trạng thái Chế độ Bảo trì 3 Cấp độ (`NORMAL_OPERATIONS`, `READ_ONLY_MAINTENANCE`, `FULL_MAINTENANCE_LOCKDOWN`).
- Đã chốt Ràng buộc Phát Thông báo Trước 30 Phút qua M10 (`Pre-Maintenance Broadcast Notification`).
- Đã lồng ghép Giao thức Thực thi Chặn tại Middleware API Gateway, Admin IP Whitelist Bypass và Audit Log M11 (`ACT-M11-43-MAINT`).
- Đã xác lập 10 Regression Gates (`MM-G01`–`MM-G10`) và 20 Test Cases tự kiểm (`MM43-01`–`MM43-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế chế độ bảo trì Lát A M11-T043-A | WSA-7K2 |
