# Xây dựng dòng thời gian hỗ trợ M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-SUPPORT-TIMELINE-BUILDER-1.0` |
| Task | M11-T028 |
| Đầu vào | M11-SAFE-USER-SEARCH-1.0 (D-074), M01-IDENTITY-EVENT-CATALOG-1.0 (D-103), REL-07 |
| Phạm vi | Đặc tả Giao thức Tổng hợp Dòng Thời gian Hỗ trợ (`Support Timeline Builder Engine`), truy vấn sự kiện lịch sử 5 module (M01, M03, M06, M10, M11), tự động che mờ PII cho SupportAgent và thời hạn lưu giữ 90 ngày |
| Tự kiểm | A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Xây dựng Dòng Thời gian Hỗ trợ (`Support Timeline Builder Engine`) thuộc M11, tổng hợp toàn bộ lịch sử tương tác và sự kiện nghiệp vụ của một học viên trong cửa sổ 90 ngày thành một giao diện dòng thời gian duy nhất (`Unified Support Timeline DTO`), phục vụ chuyên viên hỗ trợ (SupportAgent) khi tiếp nhận các vụ việc khiếu nại hoặc sự cố tài khoản.

- **Tổng hợp Sự kiện 5 Module Chuẩn hóa (`5-Module Event Aggregation Invariant`)**: Dòng thời gian hỗ trợ tổng hợp sự kiện từ 5 nguồn dữ liệu chính:
  - *M01 Identity Events*: Đăng ký, đăng nhập, đổi mật khẩu, đổi vai trò, khóa/mở tài khoản, thu hồi thiết bị.
  - *M03 Learning Events*: Hoàn thành bài học, nộp bài kiểm tra, mở khóa bộ từ.
  - *M06 Gamification Events*: Nhận Exp, Gold, mở khóa danh hiệu, tiêu dùng phần thưởng.
  - *M10 Messaging Events*: Đã gửi PUSH Notification, mở tin nhắn.
  - *M11 Support Actions*: Nhật ký thao tác hỗ trợ cũ, thay đổi PII bởi SupportAgent.
- **Ràng buộc Thời hạn Lưu giữ Dòng thời gian (`Timeline Retention Invariant`)**: Dòng thời gian hỗ trợ active duy trì 90 ngày (`ActiveRetention = 90d`). Dữ liệu sự kiện cũ hơn 90 ngày được nén chuyển sang kho lưu trữ lịch sử (`ArchivedTimeline = 365d`).
- **Che mờ PII Mặc định cho SupportAgent (`Default PII Masking Invariant`)**: $100\%$ thuộc tính dữ liệu cá nhân trực tiếp (Họ tên, Email, SĐT) trên Dòng thời gian BẮT BUỘC che mờ. CHỈ giải băm PII khi Ticket hỗ trợ được nâng cấp lên `ESCALATED` và có phê duyệt của `SecurityAdmin` (REL-07).
- **Ràng buộc Mã Ticket Hỗ trợ Active (`Active Support Ticket Enforcement`)**: Tra cứu Dòng thời gian hỗ trợ BẮT BUỘC truyền mã Ticket hợp lệ (`X-Support-Ticket-Id`). $100\%$ thao tác xem dòng thời gian đều được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-28`).

## 2. Cấu trúc DTO Dòng Thời gian Hỗ trợ (UserSupportTimelineDto Schema)

```json
{
  "targetUserId": "USR-10024",
  "maskedDisplayName": "Trần *** Nhanh",
  "maskedEmail": "n***h@gmail.com",
  "ticketId": "TCK-SUP-2026-0821-008",
  "timelineRangeDays": 90,
  "totalEvents": 4,
  "events": [
    {
      "eventId": "EVT-M01-20260821-001",
      "moduleOrigin": "M01",
      "eventType": "UserAccountLockedIntegrationEvent",
      "summary": "Tài khoản bị khóa tự động 30m do thử sai mật khẩu 5 lần",
      "timestampUtc": "2026-08-21T09:00:00Z",
      "severity": "WARNING"
    },
    {
      "eventId": "EVT-M10-20260821-002",
      "moduleOrigin": "M10",
      "eventType": "PushNotificationSentIntegrationEvent",
      "summary": "Đã gửi PUSH cảnh báo an ninh tới thiết bị Android",
      "timestampUtc": "2026-08-21T09:00:05Z",
      "severity": "INFO"
    },
    {
      "eventId": "EVT-M03-20260821-003",
      "moduleOrigin": "M03",
      "eventType": "LessonCompletedIntegrationEvent",
      "summary": "Hoàn thành Bài học 05 thuộc Bộ từ 'IELTS Academic'",
      "timestampUtc": "2026-08-21T08:30:00Z",
      "severity": "INFO"
    },
    {
      "eventId": "EVT-M06-20260821-004",
      "moduleOrigin": "M06",
      "eventType": "GoldRewardClaimedIntegrationEvent",
      "summary": "Nhận 50 Gold từ nhiệm vụ hàng ngày",
      "timestampUtc": "2026-08-21T08:31:00Z",
      "severity": "INFO"
    }
  ]
}
```

## 3. Quy trình Tổng hợp Dòng Thời gian (Timeline Builder Pipeline)

```
[SupportAgent Calls GET /api/v1/support/timeline/{targetUserId}]
                           |
                           v
         [Validate Active Ticket & Agent Authority]
         - Header: X-Support-Ticket-Id
                           |
                           v
         [Fetch Parallel Events from 5 Module Stores (90-day Window)]
         - M01 Audit Logs & Outbox Events
         - M03 Learning Progress Logs
         - M06 Reward Claim Logs
         - M10 Push Notification Logs
         - M11 Support History Logs
                           |
                           v
         [Sort Aggregated Events Chronologically (TimestampUtc Desc)]
                           |
                           v
         [Apply PII Masking Filter on Direct Attributes]
                           |
                           v
         [Record Audit Event ACT-M11-28 in DB]
                           |
                           v
         [Return UserSupportTimelineDto (Latency < 50ms)]
```

## 4. Giao thức Thực thi Tổng hợp CSDL (SupportTimelineBuilderService)

```csharp
public async Task<UserSupportTimelineDto> BuildUserSupportTimelineAsync(
    string targetUserId, 
    string ticketId, 
    string actorUserId, 
    int days = 90)
{
    // 1. Validate Active Support Ticket
    bool isTicketValid = await _supportTicketService.ValidateTicketActiveAsync(ticketId, targetUserId, actorUserId);
    if (!isTicketValid)
    {
        throw new UnauthorizedAccessException("INVALID_OR_CLOSED_SUPPORT_TICKET: Yêu cầu mã Ticket hỗ trợ đang hoạt động.");
    }

    var fromDateUtc = DateTime.UtcNow.AddDays(-days);

    // 2. Fetch Events Parallely from N-Modules
    var m01Task = _m01EventRepo.GetEventsAsync(targetUserId, fromDateUtc);
    var m03Task = _m03EventRepo.GetEventsAsync(targetUserId, fromDateUtc);
    var m06Task = _m06EventRepo.GetEventsAsync(targetUserId, fromDateUtc);
    var m10Task = _m10EventRepo.GetEventsAsync(targetUserId, fromDateUtc);

    await Task.WhenAll(m01Task, m03Task, m06Task, m10Task);

    // 3. Aggregate & Sort Chronologically
    var allEvents = m01Task.Result
        .Concat(m03Task.Result)
        .Concat(m06Task.Result)
        .Concat(m10Task.Result)
        .OrderByDescending(e => e.TimestampUtc)
        .ToList();

    // 4. Apply PII Masking Filter
    var maskedProfile = await _piiService.GetMaskedProfileAsync(targetUserId);

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-28", actorUserId, new {
        TargetUserId = targetUserId,
        TicketId = ticketId,
        EventCount = allEvents.Count
    });

    return new UserSupportTimelineDto {
        TargetUserId = targetUserId,
        MaskedDisplayName = maskedProfile.MaskedDisplayName,
        MaskedEmail = maskedProfile.MaskedEmail,
        TicketId = ticketId,
        TimelineRangeDays = days,
        TotalEvents = allEvents.Count,
        Events = allEvents
    };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `ST-G01` | Dòng thời gian hỗ trợ phải tổng hợp đủ sự kiện từ cả 5 module (M01, M03, M06, M10, M11). |
| `ST-G02` | Tra cứu Dòng thời gian BẮT BUỘC có mã Ticket hỗ trợ active (`X-Support-Ticket-Id`). |
| `ST-G03` | 100% thuộc tính PII trực tiếp trên Dòng thời gian phải được che mờ mặc định đối với SupportAgent. |
| `ST-G04` | Cửa sổ dữ liệu mặc định của Dòng thời gian active là 90 ngày (`ActiveRetention = 90d`). |
| `ST-G05` | Các sự kiện trong Dòng thời gian phải được sắp xếp chính xác theo thứ tự thời gian giảm dần (`TimestampUtc Desc`). |
| `ST-G06` | 100% các lần xây dựng Dòng thời gian hỗ trợ được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-28`). |
| `ST-G07` | Phân quyền truy cập API Dòng thời gian chỉ dành riêng cho `SupportAgent`, `SecurityAdmin` và `SuperAdmin`. |
| `ST-G08` | Vai trò `ContentAdmin` tuyệt đối CẤM truy cập Dòng thời gian hỗ trợ người dùng (REL-02). |
| `ST-G09` | SLA tổng hợp và trả về Dòng thời gian hỗ trợ $< 50\text{ms}$. |
| `ST-G10` | 100% các test case tự kiểm ST28-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `ST28-01` | SupportAgent xem Dòng thời gian của User B với TicketId active hợp lệ | Trả về DTO tổng hợp đủ các sự kiện M01, M03, M06 trong 90 ngày |
| `ST28-02` | SupportAgent xem Dòng thời gian nhưng truyền TicketId đã bị CLOSED | Reject 403 `INVALID_OR_CLOSED_SUPPORT_TICKET` |
| `ST28-03` | SupportAgent xem Dòng thời gian nhưng không truyền TicketId | Reject 400 `ACTIVE_TICKET_REQUIRED` |
| `ST28-04` | ContentAdmin thử gọi API xem Dòng thời gian của User B | Deny 403 Forbidden (REL-02) |
| `ST28-05` | Tra cứu các sự kiện xảy ra từ 120 ngày trước ($> 90$ ngày) | Không xuất hiện trong active timeline, yêu cầu truy vấn Archive Store |
| `ST28-06` | Quét thông tin Họ tên và Email hiển thị trên Dòng thời gian | 100% Email đã được che mờ `n***h@gmail.com` |
| `ST28-07` | Tra cứu vết Audit Log M11 sau khi SupportAgent tạo Timeline | Ghi nhận Audit Event `ACT-M11-28` đính kèm TicketId |
| `ST28-08` | User B vừa hoàn thành 1 bài học ở M03 | Dòng thời gian cập nhật ngay sự kiện `LessonCompleted` mới nhất |
| `ST28-09` | User B vừa bị khóa tài khoản ở M01 | Dòng thời gian hiển thị sự kiện `AccountLocked` ở đầu danh sách |
| `ST28-10` | Tải đồng thời 50 request xem Dòng thời gian cho 50 học viên khác nhau | Aggregate processing latency p95 $< 45\text{ms}$ |
| `ST28-11` | Chuyên viên hỗ trợ mở Ticket mức `ESCALATED` và được SecurityAdmin phê duyệt | Giải băm PII hiển thị email thô để liên hệ trực tiếp |
| `ST28-12` | Tra cứu Dòng thời gian của một người dùng mới đăng ký (0 sự kiện) | Trả về DTO danh sách rỗng (`TotalEvents = 0`), 200 OK |
| `ST28-13` | User không phải Admin/SupportAgent xin xem Dòng thời gian người khác | Deny 403 Forbidden |
| `ST28-14` | User chưa đăng nhập gọi API Dòng thời gian hỗ trợ | Deny 401 Unauthorized |
| `ST28-15` | Sắp xếp các sự kiện cùng milisecond trong Dòng thời gian | Ưu tiên hiển thị sự kiện M01 an ninh lên trên |
| `ST28-16` | Tra cứu danh sách tất cả các loại sự kiện được hỗ trợ trong Timeline | Trả về Danh mục 5 Module Events hoàn chỉnh |
| `ST28-17` | Phân tích tham chiếu schema Dòng thời gian trong CSDL M11 | Quét schema `M11_SupportTimeline` (T020) |
| `ST28-18` | Một trong 5 Module DB bị ngắt kết nối (ví dụ M06 bị sập) | Graceful degraded fallback: Trả về kết quả từ 4 Module còn lại kèm cờ `IsPartialData = true` |
| `ST28-19` | Yêu cầu xem Dòng thời gian kéo dài 30 ngày (`days = 30`) | Trả về DTO nén trong phạm vi 30 ngày chuẩn xác |
| `ST28-20` | Kiểm thử hoàn tất luồng xây dựng dòng thời gian hỗ trợ M11-SUPPORT-TIMELINE-BUILDER-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-ST-I01` | M11 hiện tại chưa có bộ `SupportTimelineBuilderService` tổng hợp liên module | Chuyên viên hỗ trợ phải tra cứu thủ công rải rác từng CSDL module | M11-T049 (Source task) |
| `M11-ST-I02` | Chưa có cơ chế Degraded Fallback (`IsPartialData = true`) khi 1 DB module bị sập | Một module bị ngắt kết nối làm treo toàn bộ API Dòng thời gian | M11-T049; M12-T005 |
| `M11-ST-I03` | Thiếu bộ lọc che mờ PII mặc định trên Dòng thời gian hỗ trợ | SupportAgent có thể nhìn thấy Email và SĐT thô của người học | M11-T049; REL-07 |
| `M11-ST-I04` | Thiếu luồng nén Archive Data cho các sự kiện cũ quá 90 ngày | Bảng Timeline bị phình to làm giảm tốc độ truy vấn CSDL M11 | M11-T049; M11-T035 |
| `M11-ST-I05` | Chưa đối soát TicketId active khi gọi API Dòng thời gian | Risk lạm dụng quyền tra cứu thông tin cá nhân ngoài phạm vi vụ việc | M11-T049; M11-T029 |

- `M11-ST-F01`: Triển khai `SupportTimelineBuilderService` tổng hợp sự kiện 5 module (tiếp nhận: M11-T049).
- `M11-ST-F02`: Tích hợp Bắt buộc `Active Ticket Requirement` & PII Masking Default (tiếp nhận: M11-T049; REL-07).
- `M11-ST-F03`: Triển khai Fallback `IsPartialData` & Archive Store 90d (tiếp nhận: M11-T049; M12-T005).
- `M11-ST-F04`: Thiết lập bộ kiểm thử tự động ST-G01–G10 và ST28-01–20 (tiếp nhận: M11 tasks).
- `M11-ST-F05`: Thu thập bằng chứng runtime cho luồng dòng thời gian M11 (tiếp nhận: M11 tasks; A-G02).

## 8. Tự kiểm M11-T028

- Đã thiết kế hoàn chỉnh `M11-SUPPORT-TIMELINE-BUILDER-1.0` với Giao thức Tổng hợp Sự kiện 5 Module.
- Đã chốt Ràng buộc Thời hạn Lưu giữ Dòng thời gian (Active 90d, Archive 365d).
- Đã chốt Ràng buộc Che mờ PII Mặc định cho SupportAgent và Xử lý Degraded Fallback (`IsPartialData`).
- Đã lồng ghép Yêu cầu Mã Ticket Hỗ trợ Active (`X-Support-Ticket-Id`) và Lưu vết Audit Log M11 (`ACT-M11-28`).
- Đã xác lập 10 Regression Gates (`ST-G01`–`ST-G10`) và 20 Test Cases tự kiểm (`ST28-01`–`ST28-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả xây dựng dòng thời gian hỗ trợ M11-T028 | WSA-7K2 |
