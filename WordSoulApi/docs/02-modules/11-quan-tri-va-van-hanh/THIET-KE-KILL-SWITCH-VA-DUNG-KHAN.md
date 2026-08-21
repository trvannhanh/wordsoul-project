# Thiết kế kill switch và dừng khẩn M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-KILL-SWITCH-EMERGENCY-HALT-1.0` |
| Task | M11-T044 |
| Đầu vào | M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), M12-FAIL-1.0 (D-025), M01-REVOKE-PUSH-DEVICE-A-1.0 (D-091), REL-02, REL-03 |
| Phạm vi | Đặc tả Giao thức Công tắc Ngắt Tích hợp và Dừng Khẩn Cấp (`Emergency Halt & Feature Kill Switch Protocol`), ma trận công tắc Feature Kill Switches, quy trình ngắt hệ thống toàn phần SLA $\le 5$ giây, cơ chế xác thực 2 người (Two-Person Approval) và lưu vết kiểm toán |
| Tự kiểm | A-G04, A-G06; REL-02, REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Công tắc Ngắt Tích hợp và Dừng Khẩn Cấp (`Emergency Halt & Feature Kill Switch Protocol`) thuộc M11, xác lập khả năng ngắt ngay lập tức một tính năng bị lỗi/bị tấn công hoặc dừng toàn bộ hoạt động hệ thống trong các kịch bản sự cố nghiêm trọng (Rò rỉ dữ liệu, Tấn công 0-day, Vi phạm an ninh mạng), đảm bảo hạn chế tối đa thiệt hại (REL-02, REL-03).

- **Sổ Đăng ký Công tắc Ngắt Tính năng (`Feature Kill Switch Registry Invariant`)**: 100% các tính năng nhạy cảm (`KILL_OAUTH_GOOGLE`, `KILL_AI_GEMINI`, `KILL_PAYMENT_GATEWAY`, `KILL_ASSET_UPLOADS`, `KILL_SUPPORT_MUTATIONS`) BẮT BUỘC có công tắc ngắt độc lập trong Redis Pub/Sub. Khi cờ Kill Switch bật, tính năng bị vô hiệu hóa lập tức SLA $\le 100\text{ms}$ tại tất cả các node.
- **Ràng buộc Giao thức Dừng Khẩn Cấp Hệ thống SLA $\le 5\text{s}$ (`Emergency System Halt SLA`)**: Khi kích hoạt lệnh Dừng Khẩn Cấp Toàn phần (`GLOBAL_EMERGENCY_HALT`), hệ thống TỰ ĐỘNG thực hiện 3 hành động đồng thời: (1) Tăng cờ `SecurityEpoch += 1` thu hồi toàn bộ phiên làm việc (D-091), (2) Dừng tất cả các Worker công việc ngầm M11-T038, và (3) API Gateway trả về HTTP 503 `EMERGENCY_SYSTEM_HALT` trong SLA $\le 5$ giây trên toàn bộ cụm Server.
- **Ràng buộc Phê duyệt 2 Người & Re-Auth $\le 5\text{m}$ (`Two-Person Approval & Re-Auth Guard`)**: Lệnh Dừng Khẩn Cấp Toàn phần BẮT BUỘC phải được phê duyệt đồng thời bởi 2 Quản trị viên cấp cao (`SecurityAdmin` + `SuperAdmin`) với cờ xác thực lại mật khẩu local trong 5 phút gần nhất (`ReAuthMinutes <= 5m`). CẤM một cá nhân đơn lẻ tự ý dừng toàn bộ hệ thống.
- **Lưu vết Sổ Kiểm toán Dừng Khẩn M11 (`Emergency Halt Audit Trail`)**: $100\%$ các đợt bật/tắt Kill Switch hoặc Dừng Khẩn Cấp được ghi vết bất biến `ACT-M11-44-KILL` trong Sổ Kiểm toán M11.

## 2. Ma trận Công tắc Ngắt và Dừng Khẩn Cấp (Kill Switch Matrix)

| Loại Công tắc (`SwitchType`) | Phạm vi Tác động (`Scope`) | Yêu cầu Phân quyền | SLA Thực thi Ngắt | Trạng thái Phản hồi API | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `KILL_OAUTH_GOOGLE` | Tính năng Đăng nhập Google | `SecurityAdmin` (1 Actor) | **SLA $\le 100\text{ms}$** | HTTP 503 `FEATURE_DISABLED` | `ACT-M11-44-FEATURE` |
| `KILL_AI_GEMINI` | Dịch vụ AI Gemini M12 | `SecurityAdmin` (1 Actor) | SLA $\le 100\text{ms}$ | Fallback Static Content | `ACT-M11-44-FEATURE` |
| `KILL_PAYMENT_GATEWAY` | Giao dịch Thanh toán M06 | `SecurityAdmin` (1 Actor) | SLA $\le 100\text{ms}$ | HTTP 503 `PAYMENT_DISABLED` | `ACT-M11-44-FEATURE` |
| **`GLOBAL_EMERGENCY_HALT`** | **Toàn bộ Hệ thống WordSoul** | **2 Admins + Re-Auth $\le 5\text{m}$** | **SLA $\le 5\text{s}$ (Global)** | **HTTP 503 `EMERGENCY_HALT`** | `ACT-M11-44-GLOBAL` |

## 3. Kiến trúc Luồng Xử lý Dừng Khẩn Cấp M11 (Kill Switch Engine)

```
[SecurityAdmin & SuperAdmin Initiate GLOBAL_EMERGENCY_HALT]
                           |
                           v
     [Verify Password Re-Auth <= 5m + Two-Person Approval Token]
                           |
         +-----------------+-----------------+
         | (Validation Failed)               | (Validated OK)
         v                                   v
[REJECT: 403 TWO_PERSON_REQUIRED]    [Publish Redis Pub/Sub Global Kill Signal]
                                                 |
                                  +--------------+--------------+
                                  |                             |
                                  v                             v
                   [SecurityEpoch += 1 (D-091)]     [Stop All Background Workers]
                   - Invalidate 100% User Sessions  - Pause M11-T038 Workers
                                  |                             |
                                  +--------------+--------------+
                                                 |
                                                 v
                                  [API Gateway Returns HTTP 503 SLA <= 5s]
                                  [Record Audit Log ACT-M11-44-GLOBAL]
```

## 4. Giao thức Thực thi Dừng Khẩn Cấp CSDL (EmergencyHaltService)

```csharp
public async Task TriggerGlobalEmergencyHaltAsync(
    string primaryAdminId, 
    string secondaryAdminId, 
    string secondaryApprovalToken, 
    string reason)
{
    // 1. Re-Auth Guard <= 5m for Primary Admin
    var primaryAdmin = await _db.Users.FirstOrDefaultAsync(u => u.Id == primaryAdminId);
    if (primaryAdmin == null || primaryAdmin.LastReAuthenticatedAtUtc == null || 
        (DateTime.UtcNow - primaryAdmin.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi phát lệnh Dừng Khẩn Cấp.");
    }

    // 2. Two-Person Approval Guard
    bool isSecondaryValid = await _approvalService.VerifySecondaryAdminTokenAsync(secondaryAdminId, secondaryApprovalToken);
    if (!isSecondaryValid)
    {
        throw new InvalidOperationException("TWO_PERSON_APPROVAL_REQUIRED: Lệnh Dừng Khẩn Cấp bắt buộc có sự phê duyệt đồng thời của Quản trị viên thứ hai.");
    }

    // 3. Increment Global SecurityEpoch (Revoke 100% Sessions SLA <= 5s)
    await _securityEpochService.IncrementGlobalSecurityEpochAsync();

    // 4. Publish Redis Pub/Sub Kill Signal SLA <= 100ms
    var sub = _redis.GetSubscriber();
    await sub.PublishAsync("wordsoul:channel:emergency", "GLOBAL_EMERGENCY_HALT");

    // Update DB System State
    var sysState = await _db.SystemStates.FirstOrDefaultAsync();
    sysState.IsEmergencyHaltActive = true;
    sysState.HaltReason = reason;
    sysState.HaltedAtUtc = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-44-GLOBAL", primaryAdminId, new {
        SecondaryAdminId = secondaryAdminId,
        Reason = reason,
        Action = "GLOBAL_EMERGENCY_HALT_ACTIVATED"
    });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `KS-G01` | Lệnh ngắt tính năng đơn lẻ (Feature Kill Switch) BẮT BUỘC lan truyền vô hiệu hóa SLA $\le 100\text{ms}$. |
| `KS-G02` | Lệnh Dừng Khẩn Cấp Toàn phần (`GLOBAL_EMERGENCY_HALT`) BẮT BUỘC hoàn tất vô hiệu hệ thống SLA $\le 5$ giây. |
| `KS-G03` | Lệnh Dừng Khẩn Cấp Toàn phần BẮT BUỘC có cờ Phê duyệt 2 Người (`Two-Person Approval`) từ 2 Admins độc lập. |
| `KS-G04` | Cả 2 Admins phê duyệt Dừng Khẩn Cấp BẮT BUỘC xác thực lại mật khẩu local trong 5 phút (`ReAuthMinutes <= 5m`). |
| `KS-G05` | Kích hoạt Dừng Khẩn Cấp tự động tăng `SecurityEpoch += 1` thu hồi $100\%$ phiên làm việc người dùng (D-091). |
| `KS-G06` | 100% các thao tác ngắt tính năng hoặc Dừng Khẩn Cấp được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-44-KILL`). |
| `KS-G07` | API Gateway trả về phản hồi HTTP 503 `EMERGENCY_SYSTEM_HALT` cho $100\%$ cuộc gọi từ người học trong thời gian dừng. |
| `KS-G08` | Phân quyền khôi phục hệ thống từ trạng thái Dừng Khẩn Cấp chỉ dành riêng cho `SuperAdmin` với cờ Re-Auth $\le 5\text{m}$. |
| `KS-G09` | Khả năng tự ngắt mạch cục bộ không gây hỏng hóc hoặc lặp lỗi treo hệ thống CSDL. |
| `KS-G10` | 100% các test case tự kiểm KS44-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `KS44-01` | Bật cờ `KILL_OAUTH_GOOGLE` khi phát hiện Google OAuth API bị lỗi | Vô hiệu đăng nhập Google SLA $< 50\text{ms}$, trả về 503 |
| `KS44-02` | Kích hoạt `GLOBAL_EMERGENCY_HALT` với đủ 2 Admins Re-Auth $\le 5\text{m}$ | Thu hồi 100% phiên, dừng workers, 503 SLA $< 3.2\text{s}$ |
| `KS44-03` | Admin 1 phát lệnh `GLOBAL_EMERGENCY_HALT` nhưng KHÔNG có phê duyệt của Admin 2 | Reject 400 `TWO_PERSON_APPROVAL_REQUIRED` |
| `KS44-04` | Admin 1 thử phát lệnh `GLOBAL_EMERGENCY_HALT` khi lần Re-Auth cuối là 8 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `KS44-05` | Người học gửi request tới ứng dụng sau khi `GLOBAL_EMERGENCY_HALT` được bật | Trả về HTTP 503 `EMERGENCY_SYSTEM_HALT` lập tức |
| `KS44-06` | Tra cứu vết Audit Log M11 sau khi phát lệnh Dừng Khẩn Cấp Toàn phần | Ghi nhận Audit Event `ACT-M11-44-GLOBAL` đính kèm 2 AdminIds |
| `KS44-07` | Bật cờ `KILL_AI_GEMINI` khi phát hiện AI prompt rò rỉ dữ liệu | Dịch vụ AI dừng phát tin SLA $< 40\text{ms}$, chuyển static fallback |
| `KS44-08` | SuperAdmin phát lệnh khôi phục hệ thống sau khi hết sự cố | Đặt cờ `IsEmergencyHaltActive = false`, mở lại API 200 OK |
| `KS44-09` | Tải đồng thời 10,000 request bị từ chối 503 trong thời gian Dừng Khẩn Cấp | Gate latency p95 $< 1.1\text{ms}$ per request |
| `KS44-10` | Kênh Redis Pub/Sub bị gián đoạn khi phát tín hiệu Kill Switch | Fallback qua Redis Key polling 1s SLA $< 1\text{s}$ |
| `KS44-11` | Thử nạp cờ Kill Switch với tên tính năng không tồn tại | Reject 400 `INVALID_FEATURE_KILL_SWITCH_KEY` |
| `KS44-12` | Gửi request kích hoạt Kill Switch khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `KS44-13` | User không phải SecurityAdmin thử bật cờ `KILL_PAYMENT_GATEWAY` | Deny 403 Forbidden |
| `KS44-14` | User chưa đăng nhập gọi API cấu hình Kill Switch M11 | Deny 401 Unauthorized |
| `KS44-15` | Bật cờ Kill Switch cho dịch vụ nộp tài sản `KILL_ASSET_UPLOADS` | Từ chối Pre-Signed Upload URLs M12-T022 |
| `KS44-16` | Kiểm tra độ trễ thu hồi $100\%$ Refresh Tokens khi SecurityEpoch tăng | Revocation SLA $< 800\text{ms}$ qua Redis Cache |
| `KS44-17` | Phân tích tham chiếu các bản ghi `SystemStates` trong CSDL | Quét schema `M11_SystemStates` (T020) |
| `KS44-18` | Một Node API Gateway bị treo không nhận tín hiệu Redis Pub/Sub | Local health check worker tự ngắt node khỏi Load Balancer |
| `KS44-19` | Tra cứu danh sách các Feature Kill Switches đang bật | Trả về DTO danh sách các công tắc bị ngắt |
| `KS44-20` | Kiểm thử hoàn tất luồng kill switch và dừng khẩn M11-KILL-SWITCH-EMERGENCY-HALT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-KS-I01` | M11 hiện tại chưa có `EmergencyHaltService` ngắt hệ thống | Risk phơi nhiễm dữ liệu khi xảy ra tấn công 0-day | M11-T049 (Source task) |
| `M11-KS-I02` | Thiếu cờ Phê duyệt 2 Người (`Two-Person Approval`) khi dừng khẩn | Risk 1 cá nhân bất mãn tự ý đánh sập toàn bộ hệ thống | M11-T049; REL-02 |
| `M11-KS-I03` | Thiếu luồng tự động tăng `SecurityEpoch += 1` thu hồi phiên | Người học vẫn có thể thao tác API khi đã phát lệnh dừng | M11-T049; M01-T018 |
| `M11-KS-I04` | Thiếu tín hiệu Redis Pub/Sub ngắt tức thì SLA $\le 100\text{ms}$ | Các node API khác nhận lệnh chậm gây lệch trạng thái | M11-KS-F04; M12-T040 |
| `M11-KS-I05` | Chưa kết nối sự kiện Dừng Khẩn Cấp với Audit Log M11 (`ACT-M11-44-GLOBAL`) | Không ghi vết được 2 cá nhân chịu trách nhiệm phát lệnh | M11-T049; M11-T031 |

- `M11-KS-F01`: Triển khai `EmergencyHaltService` với Feature Kill Switch Registry (tiếp nhận: M11-T049).
- `M11-KS-F02`: Tích hợp Bắt buộc Two-Person Approval & Re-Auth Guard $\le 5\text{m}$ (tiếp nhận: M11-T049; REL-02).
- `M11-KS-F03`: Triển khai Redis Pub/Sub Kill Signal SLA $\le 100\text{ms}$ & SecurityEpoch Increment (tiếp nhận: M11-T049; M01-T018).
- `M11-KS-F04`: Thiết lập bộ kiểm thử tự động KS-G01–G10 và KS44-01–20 (tiếp nhận: M11 tasks).
- `M11-KS-F05`: Thu thập bằng chứng runtime cho luồng kill switch M11 (tiếp nhận: M11 tasks; A-G04/A-G06).

## 8. Tự kiểm M11-T044

- Đã thiết kế hoàn chỉnh `M11-KILL-SWITCH-EMERGENCY-HALT-1.0` với Ma trận Công tắc Ngắt và Dừng Khẩn Cấp.
- Đã chốt Ràng buộc Sổ Đăng ký Công tắc Ngắt Tính năng (`Feature Kill Switch Registry`).
- Đã chốt Ràng buộc Giao thức Dừng Khẩn Cấp Hệ thống SLA $\le 5$ giây (`Emergency System Halt`).
- Đã lồng ghép Ràng buộc Phê duyệt 2 Người & Re-Auth $\le 5\text{m}$ (`Two-Person Approval & Re-Auth Guard`), Tăng SecurityEpoch += 1 và Audit Log M11 (`ACT-M11-44-GLOBAL`).
- Đã xác lập 10 Regression Gates (`KS-G01`–`KS-G10`) và 20 Test Cases tự kiểm (`KS44-01`–`KS44-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả kill switch và dừng khẩn M11-T044 | WSA-7K2 |
