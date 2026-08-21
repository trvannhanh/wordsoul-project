# Thiết kế cảnh báo và escalation M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-ALERT-ESCALATION-1.0` |
| Task | M11-T037 |
| Đầu vào | M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), M12-FAIL-1.0 (D-025), REL-03 |
| Phạm vi | Đặc tả Giao thức Cảnh báo An ninh và Thang Leo Sự cố (`Alerting & Incident Escalation Protocol`), phân định 4 mức độ nghiêm trọng (P1–P4), quy trình chuyển tiếp cuộc gọi PagerDuty/Slack, cơ chế nén cảnh báo trùng lặp (Throttling 15m) và lưu vết kiểm toán |
| Tự kiểm | A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Cảnh báo An ninh và Thang Leo Sự cố (`Alerting & Incident Escalation Protocol`) thuộc M11, xác lập hệ thống phát hiện, phân loại và leo thang tự động đối với các sự cố suy giảm dịch vụ hoặc vi phạm an ninh hệ thống, đảm bảo đội ngũ On-Call tiếp nhận xử lý trong SLA cam kết (REL-03).

- **Phân định 4 Mức độ Nghiêm trọng Cảnh báo (`4 Severity Levels Invariant`)**:
  - `P1_CRITICAL`: Sự cố ngắt mạch năng lực (`CIRCUIT_OPEN`), gián đoạn toàn bộ hệ thống đăng nhập/thanh toán. Phát cảnh báo lập tức SLA $\le 60\text{s}$.
  - `P2_HIGH`: Suy giảm năng lực nghiêm trọng (`DEGRADED` / `UNHEALTHY`), độ trễ p99 $> 2000\text{ms}$. Phát cảnh báo SLA $\le 5\text{m}$.
  - `P3_WARN`: Vi phạm ngưỡng phụ, thử lại nhiều lần. Cảnh báo dạng digest hàng giờ.
  - `P4_INFO`: Thông tin vận hành chuẩn. Ghi log tổng hợp hàng ngày.
- **Quy trình Thang Leo Tự động (`Automatic Escalation Timer Rules`)**: Đối với sự cố `P1_CRITICAL`, nếu Kỹ sư On-Call Level 1 không phản hồi xác nhận (Acknowledge) trong vòng 15 phút (`AckTimeout = 15m`), hệ thống TỰ ĐỘNG leo thang phát tin nhắn thoại/SMS cuộc gọi khẩn cấp tới Kỹ sư Trưởng On-Call Level 2 (Engineering Lead).
- **Cơ chế Nén & Khống chế Cảnh báo Trùng lặp (`Alert Throttling & Deduplication`)**: CẤM phát liên tục các cảnh báo giống hệt nhau cho cùng một năng lực. Hệ thống gộp các cảnh báo trùng trong cửa sổ 15 phút (`ThrottleWindow = 15m`) thành 1 thông báo tổng hợp duy nhất, tránh hiện tượng nhiễu cảnh báo (Alert Fatigue).
- **Lưu vết Sổ Kiểm toán Sự cố M11 (`Alert Escalation Audit Trail`)**: $100\%$ các sự kiện phát cảnh báo, xác nhận hoặc leo thang sự cố được ghi vết bất biến `ACT-M11-37-ALERT` trong Sổ Kiểm toán M11, ghi nhận `IncidentId`, `Severity`, `Channel`, `EscalationLevel` và `AckTimeSec`.

## 2. Ma trận Quy trình Cảnh báo và Thang Leo Sự cố (Alert Escalation Matrix)

| Mức độ (`Severity`) | Kịch bản Trigger mẫu | Kênh Phản ứng ban đầu | SLA Phát tin | Hạn ngạch Ack (`AckTimeout`) | Thang Leo Cấp 2 (`Level 2 Escalation`) |
|---|---|---|---|---|---|
| `P1_CRITICAL` | Circuit Breaker Open, DB Down | PagerDuty Call + Slack `#alerts-critical` | **SLA $\le 60\text{s}$** | **15 phút** | Cuộc gọi SMS/Voice tới Eng Lead |
| `P2_HIGH` | Degraded ErrorRate $> 30\%$ | Slack `#alerts-warning` + Email | SLA $\le 5\text{m}$ | 30 phút | PagerDuty Notification |
| `P3_WARN` | Retry Spike, High Latency | Slack `#alerts-digest` | SLA $\le 1\text{h}$ | N/A | Log vào Dashboard |
| `P4_INFO` | Daily Cleanup Complete | Log System Output | SLA $\le 24\text{h}$ | N/A | N/A (Chỉ ghi vết) |

## 3. Kiến trúc Luồng Xử lý Cảnh báo và Thang Leo (Alert Escalation Engine)

```
[Health Registry Triggers Alert Event (P1_CRITICAL)]
                         |
                         v
       [Check Alert Throttling Window (15m)]
                         |
        +----------------+----------------+
        | (Already Alerted in 15m)        | (New Alert)
        v                                 v
[Suppress Duplicate Alert]      [Dispatch PagerDuty & Slack #alerts-critical]
                                [Start Escalation Timer (15m)]
                                                 |
                                +----------------+----------------+
                                | (Acknowledged in <=15m)         | (Unacknowledged > 15m)
                                v                                 v
                        [Stop Escalation Timer]          [ESCALATE TO LEVEL 2]
                        [Record Audit ACT-M11-37]        - Dispatch SMS/Voice Call to Eng Lead
                                                         - Record Audit ACT-M11-37
```

## 4. Giao thức Thực thi Quản lý Cảnh báo CSDL (AlertEscalationService)

```csharp
public async Task DispatchIncidentAlertAsync(string capabilityId, SeverityLevel severity, string message)
{
    string throttleKey = $"wordsoul:alert:throttle:{capabilityId}:{severity}";
    
    // 1. Throttling & Deduplication Check (15m)
    bool isThrottled = await _redisDb.KeyExistsAsync(throttleKey);
    if (isThrottled)
    {
        Log.Information("Alert throttled for {CapabilityId} severity {Severity}", capabilityId, severity);
        return;
    }

    await _redisDb.StringSetAsync(throttleKey, "ALERTED", TimeSpan.FromMinutes(15));

    // 2. Generate Incident Record
    string incidentId = Guid.NewGuid().ToString("N");
    var incident = new IncidentRecord {
        IncidentId = incidentId,
        CapabilityId = capabilityId,
        Severity = severity,
        Message = message,
        State = IncidentState.DISPATCHED,
        EscalationLevel = 1,
        CreatedAtUtc = DateTime.UtcNow
    };

    _db.Incidents.Add(incident);
    await _db.SaveChangesAsync();

    // 3. Dispatch Channel Notification by Severity
    if (severity == SeverityLevel.P1_CRITICAL)
    {
        await _pagerDutyClient.TriggerIncidentAsync(incidentId, capabilityId, message);
        await _slackClient.SendAlertAsync("#alerts-critical", $"🚨 [P1 CRITICAL] {capabilityId}: {message}");

        // Start 15m Escalation Timer Worker
        await _backgroundJobs.ScheduleEscalationCheckAsync(incidentId, TimeSpan.FromMinutes(15));
    }
    else if (severity == SeverityLevel.P2_HIGH)
    {
        await _slackClient.SendAlertAsync("#alerts-warning", $"⚠️ [P2 HIGH] {capabilityId}: {message}");
    }

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-37-ALERT", "ALERT_SYSTEM", new {
        IncidentId = incidentId,
        CapabilityId = capabilityId,
        Severity = severity.ToString(),
        Action = "DISPATCHED"
    });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AE-G01` | Cảnh báo sự cố `P1_CRITICAL` BẮT BUỘC phát qua PagerDuty & Slack trong SLA $\le 60$ giây. |
| `AE-G02` | Nếu sự cố `P1_CRITICAL` không được Ack trong 15 phút, tự động leo thang tới Eng Lead Cấp 2 (`AckTimeout = 15m`). |
| `AE-G03` | Cảnh báo trùng lặp cùng năng lực trong 15 phút BẮT BUỘC bị nén khống chế (`ThrottlingWindow = 15m`). |
| `AE-G04` | Kỹ sư On-Call có thể bấm nút Ack hoặc Resolve sự cố trực tiếp qua Slack webhook hoặc API M11. |
| `AE-G05` | 100% các thao tác phát cảnh báo, Ack và leo thang sự cố được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-37-ALERT`). |
| `AE-G06` | SLA gửi thông báo qua kênh PagerDuty API $< 3\text{s}$; Slack Webhook $< 1\text{s}$. |
| `AE-G07` | Phân quyền xác nhận hoặc đóng sự cố P1/P2 chỉ dành cho Kỹ sư On-Call, `SecurityAdmin` hoặc Admin. |
| `AE-G08` | Hệ thống hỗ trợ nạp cấu hình danh sách On-Call Rotation hàng tuần từ Secret Manager M12-T040. |
| `AE-G09` | Chịu tải xử lý tới 1,000 cảnh báo đồng thời không làm treo luồng chính ứng dụng. |
| `AE-G10` | 100% các test case tự kiểm AE37-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AE37-01` | Phát cảnh báo `P1_CRITICAL` khi Google OAuth bị Circuit Open | Phát PagerDuty & Slack `#alerts-critical` SLA $< 20\text{s}$, 200 OK |
| `AE37-02` | Kỹ sư On-Call bấm nút Acknowledge trên PagerDuty ở phút thứ 5 | Ngắt timer leo thang, đổi trạng thái sự cố sang `ACKNOWLEDGED` |
| `AE37-03` | Sự cố `P1_CRITICAL` không được Ack sau 15 phút ($> 15\text{m}$) | Leo thang Cấp 2, gọi điện SMS/Voice cho Eng Lead, ghi Audit Log |
| `AE37-04` | Phát cảnh báo `P1_CRITICAL` lần 2 cho cùng năng lực sau 5 phút ($< 15\text{m}$) | Nén cảnh báo trùng (Throttling), không phát lại PagerDuty |
| `AE37-05` | Phát cảnh báo `P1_CRITICAL` lần 2 cho cùng năng lực sau 20 phút ($> 15\text{m}$) | Cho phép phát cảnh báo mới do đã hết cửa sổ Throttling |
| `AE37-06` | Tra cứu vết Audit Log M11 sau khi sự cố được leo thang Cấp 2 | Ghi nhận Audit Event `ACT-M11-37-ALERT` đính kèm EscalationLevel=2 |
| `AE37-07` | Phát cảnh báo `P2_HIGH` cho suy giảm năng lực AI Gemini | Gửi thông báo sang kênh Slack `#alerts-warning` SLA $< 2\text{s}$ |
| `AE37-08` | Kỹ sư On-Call giải quyết xong sự cố bấm "Resolve Incident" | Đổi trạng thái sang `RESOLVED`, giải phóng cờ Throttling |
| `AE37-09` | Tải đồng thời 200 cảnh báo P1 từ 200 năng lực khác nhau | Processing latency p95 $< 1.2\text{s}$ |
| `AE37-10` | Dịch vụ PagerDuty API gặp sự cố ngắt kết nối mạng | Fallback gửi SMS khẩn cấp qua kênh dự phòng Twilio |
| `AE37-11` | Thử Ack một sự cố đã được đánh dấu `RESOLVED` từ trước | Reject 400 `INCIDENT_ALREADY_RESOLVED` |
| `AE37-12` | Gửi request Ack sự cố khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `AE37-13` | User không phải On-Call/Admin thử gọi API đóng sự cố P1 | Deny 403 Forbidden |
| `AE37-14` | User chưa đăng nhập gọi API cấu hình kênh cảnh báo | Deny 401 Unauthorized |
| `AE37-15` | Phát cảnh báo `P3_WARN` khi đợt thử lại tăng nhẹ | Nạp cảnh báo vào bản tin Digest hàng giờ |
| `AE37-16` | Kiểm tra thời gian vô hiệu timer leo thang khi có lượt Ack | Cancellation SLA $< 100\text{ms}$ |
| `AE37-17` | Phân tích tham chiếu danh sách `IncidentRecords` trong CSDL | Quét schema `M11_Incidents` (T020) |
| `AE37-18` | Tiến trình `EscalationWorker` gặp sự cố sập giữa chừng | Lock phân tán M12-T033 tự nhả, worker dự phòng tiếp quản |
| `AE37-19` | Tra cứu danh sách các sự cố P1 đang chờ Ack trong ca trực | Trả về DTO danh sách Incidents kèm thời gian còn lại |
| `AE37-20` | Kiểm thử hoàn tất luồng cảnh báo và escalation M11-ALERT-ESCALATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-AE-I01` | M11 hiện tại chưa có `AlertEscalationService` xử lý sự cố P1 | Risk sự cố ngắt mạch không được thông báo kịp thời | M11-T049 (Source task) |
| `M11-AE-I02` | Thiếu cờ Throttling & Deduplication 15m cho cảnh báo | Gây hiện tượng rác cảnh báo (Alert Fatigue) làm trôi tin P1 | M11-T049; REL-03 |
| `M11-AE-I03` | Thiếu luồng Escalation Timer 15m leo thang tự động Cấp 2 | Sự cố đêm khuya có thể bị bỏ quên nếu On-Call ngủ quên | M11-T049; M11-T045 |
| `M11-AE-I04` | Thiếu kênh Fallback SMS/Voice khi PagerDuty API bị gián đoạn | Rủi ro mất hoàn toàn khả năng báo động khi sự cố mạng | M11-AE-F04; M12-T040 |
| `M11-AE-I05` | Chưa kết nối sự kiện cảnh báo với Audit Log M11 (`ACT-M11-37-ALERT`) | Không ghi vết được lịch sử leo thang và thời gian Ack | M11-T049; M11-T031 |

- `M11-AE-F01`: Triển khai `AlertEscalationService` với 4 Severity Levels (tiếp nhận: M11-T049).
- `M11-AE-F02`: Tích hợp Bắt buộc 15m Alert Throttling & Deduplication (tiếp nhận: M11-T049; REL-03).
- `M11-AE-F03`: Triển khai Auto Escalation Timer 15m (Level 1 $\to$ Level 2 Eng Lead) (tiếp nhận: M11-T049; M11-T045).
- `M11-AE-F04`: Thiết lập bộ kiểm thử tự động AE-G01–G10 và AE37-01–20 (tiếp nhận: M11 tasks).
- `M11-AE-F05`: Thu thập bằng chứng runtime cho luồng cảnh báo M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T037

- Đã thiết kế hoàn chỉnh `M11-ALERT-ESCALATION-1.0` với Ma trận Quy trình Cảnh báo và Thang Leo Sự cố.
- Đã chốt Ràng buộc Phân định 4 Mức độ Nghiêm trọng Cảnh báo (`P1_CRITICAL` $\to$ `P4_INFO`).
- Đã chốt Quy trình Thang Leo Tự động (`Escalation Timer 15m`).
- Đã lồng ghép Cơ chế Nén & Khống chế Cảnh báo Trùng lặp (Throttling 15m) và Audit Log M11 (`ACT-M11-37-ALERT`).
- Đã xác lập 10 Regression Gates (`AE-G01`–`AE-G10`) và 20 Test Cases tự kiểm (`AE37-01`–`AE37-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế cảnh báo và escalation M11-T037 | WSA-7K2 |
