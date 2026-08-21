# Xây dựng playbook sự cố trọng yếu M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CRITICAL-INCIDENT-PLAYBOOK-1.0` |
| Task | M11-T046 |
| Đầu vào | M11-INCIDENT-SEVERITY-MODEL-1.0 (D-140), M11-ALERT-ESCALATION-1.0 (D-132), M11-KILL-SWITCH-EMERGENCY-HALT-1.0 (D-139), M11-MAINTENANCE-MODE-A-1.0 (D-138), REL-02, REL-03, REL-07 |
| Phạm vi | Đặc tả Giao thức Playbook Xử lý Sự cố Trọng yếu (`Critical Incident Action Playbook`), quy trình thao tác chuẩn (SOPs) cho 3 kịch bản sự cố thảm họa nghiêm trọng, quy tắc cô lập khẩn cấp và lưu vết kiểm toán |
| Tự kiểm | A-G06; REL-02, REL-03, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Playbook Xử lý Sự cố Trọng yếu (`Critical Incident Action Playbook`) thuộc M11, cung cấp bộ Quy trình Thao tác Chuẩn (SOP - Standard Operating Procedures) từng bước cho đội ngũ On-Call Engineer và Incident Commander phản ứng tức thì khi xảy ra các thảm họa hệ thống (Rò rỉ dữ liệu, Hỏng CSDL SQL, Sập tích hợp AI/OAuth), giảm thiểu tối đa thời gian gián đoạn MTTR (REL-02, REL-03, REL-07).

- **Quy trình Thao tác Chuẩn 3 Kịch bản Thảm họa (`3 Core Incident SOP Playbooks Invariant`)**:
  - `PLAYBOOK-01 (Data Breach / Zero-Day Exploit)`: Kích hoạt ngay lệnh `GLOBAL_EMERGENCY_HALT` (D-139 SLA $\le 5\text{s}$), tự động tăng `SecurityEpoch += 1` vô hiệu $100\%$ Refresh Tokens (D-091), xoay vòng khóa bí mật Secret Keys, thông báo Security Lead SLA $\le 15\text{m}$.
  - `PLAYBOOK-02 (Database Corruption / Discrepancy >= 1%)`: Chuyển hệ thống sang `READ_ONLY_MAINTENANCE` (D-138), kích hoạt `DataReconciliationWorker` (D-137), khôi phục dữ liệu từ bản sao lưu Point-in-Time Restore (PITR), kiểm tra đối soát băm integrity.
  - `PLAYBOOK-03 (External Provider Outage / Gemini AI Failure)`: Kích hoạt ngắt mạch `CIRCUIT_OPEN` (D-131 SLA $\le 2\text{ms}$), bật cờ `KILL_AI_GEMINI` (D-139), tự động chuyển sang chế độ từ điển tĩnh fallback không chặn luồng học tập.
- **Ràng buộc Thực thi Thao tác Khóa An toàn Re-Auth $\le 5\text{m}$ (`Action Re-Auth Guard`)**: Tất cả các bước thao tác khẩn cấp can thiệp vào CSDL hoặc hạ cấp dịch vụ trong Playbook BẮT BUỘC yêu cầu Kỹ sư On-Call xác thực lại mật khẩu local trong 5 phút gần nhất (`ReAuthMinutes <= 5m`).
- **Ràng buộc Lưu trữ Hồ sơ Playbook & Post-Mortem 48h (`Post-Mortem Record Invariant`)**: Mỗi lần kích hoạt Playbook xử lý sự cố BẮT BUỘC tạo một hồ sơ theo dõi `IncidentPlaybookExecution` và hoàn tất báo cáo nguyên nhân gốc Post-Mortem công khai trong 48 giờ (REL-07).
- **Lưu vết Sổ Kiểm toán Playbook M11 (`Playbook Audit Trail`)**: $100\%$ các bước kích hoạt hoặc chuyển trạng thái Playbook được ghi vết bất biến `ACT-M11-46-PLAYBOOK` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Thao tác Chuẩn Playbook (Incident Playbook SOP Matrix)

| Mã Playbook (`PlaybookId`) | Kịch bản Sự cố Thảm họa | Bước Can thiệp Khẩn cấp 1 | Bước Can thiệp Khẩn cấp 2 | Trạng thái Hệ thống Dự phòng | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`PLAYBOOK-01`** | **Data Breach / 0-Day Vulnerability** | `GLOBAL_EMERGENCY_HALT` (D-139) | Increase `SecurityEpoch += 1` (D-091) | Total Lockout (503) | `ACT-M11-46-BREACH` |
| **`PLAYBOOK-02`** | **Database Corruption ($\ge 1\%$)** | `READ_ONLY_MAINTENANCE` (D-138) | Run PITR Snapshot Restore | Read-Only Mode | `ACT-M11-46-DBCORRUPT` |
| **`PLAYBOOK-03`** | **External AI / OAuth Outage** | Trip `CIRCUIT_OPEN` (D-131) | Enable `KILL_AI_GEMINI` (D-139) | Static Cache Fallback | `ACT-M11-46-PROVIDER` |

## 3. Kiến trúc Luồng Thực thi Playbook Xử lý Sự cố M11 (Playbook Execution Engine)

```
[Critical Incident Alert Triggered (SEV-1 / SEV-2)]
                         |
                         v
   [On-Call Engineer Selects Applicable Playbook (1, 2, or 3)]
                         |
                         v
     [Verify Password Re-Auth <= 5m Guard for Engineer]
                         |
      +------------------+------------------+
      | (PLAYBOOK-01)                       | (PLAYBOOK-02)
      v                                     v
[Execute Emergency Halt (D-139)]     [Switch to READ_ONLY_MAINT (D-138)]
[Increment SecurityEpoch += 1]       [Execute PITR DB Restore]
[Rotate Secrets & Tokens]            [Run Data Reconciliation (D-137)]
            |                                   |
            +------------------+----------------+
                               |
                               v
             [Validate Resolution & Restore Operations]
             [Start 48-Hour Post-Mortem Countdown REL-07]
             [Record Audit Log ACT-M11-46-PLAYBOOK]
```

## 4. Giao thức Thực thi Playbook CSDL (IncidentPlaybookService)

```csharp
public async Task<PlaybookExecutionResultDto> ExecuteIncidentPlaybookAsync(
    string incidentId, 
    string playbookId, 
    string engineerUserId)
{
    // 1. Re-Auth Guard <= 5m
    var engineer = await _db.Users.FirstOrDefaultAsync(u => u.Id == engineerUserId);
    if (engineer == null || engineer.LastReAuthenticatedAtUtc == null || 
        (DateTime.UtcNow - engineer.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi kích hoạt Playbook sự cố.");
    }

    var incident = await _db.IncidentRecords.FirstOrDefaultAsync(i => i.IncidentId == incidentId);
    if (incident == null) throw new KeyNotFoundException("INCIDENT_NOT_FOUND");

    var result = new PlaybookExecutionResultDto { PlaybookId = playbookId, ExecutedAtUtc = DateTime.UtcNow };

    // 2. Branch Execution by PlaybookId
    switch (playbookId)
    {
        case "PLAYBOOK-01": // Data Breach
            await _emergencyHaltService.TriggerGlobalEmergencyHaltAsync(engineerUserId, "SYSTEM", "BREACH_AUTOTOKEN", "Data Breach Isolation");
            result.ActionTaken = "GLOBAL_EMERGENCY_HALT_ACTIVATED_SECURITY_EPOCH_INCREMENTED";
            break;

        case "PLAYBOOK-02": // Database Corruption
            await _maintenanceService.SetMaintenanceModeAsync(MaintenanceMode.READ_ONLY_MAINTENANCE, "DB Integrity Restoration");
            await _reconciliationService.RunDataReconciliationAsync(engineerUserId);
            result.ActionTaken = "READ_ONLY_MAINTENANCE_ACTIVATED_RECONCILIATION_RUNNING";
            break;

        case "PLAYBOOK-03": // Provider Outage
            await _killSwitchService.SetFeatureKillSwitchAsync("KILL_AI_GEMINI", true, engineerUserId);
            result.ActionTaken = "KILL_AI_GEMINI_ENABLED_STATIC_FALLBACK_ACTIVE";
            break;

        default:
            throw new ArgumentException("INVALID_PLAYBOOK_ID");
    }

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-46-PLAYBOOK", engineerUserId, new {
        IncidentId = incidentId,
        PlaybookId = playbookId,
        ActionTaken = result.ActionTaken
    });

    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `PB-G01` | Quy trình thao tác Playbook BẮT BUỘC bao phủ 3 kịch bản thảm họa chính (`PLAYBOOK-01`, `PLAYBOOK-02`, `PLAYBOOK-03`). |
| `PB-G02` | Kích hoạt Playbook BẮT BUỘC có cờ xác thực lại mật khẩu local trong 5 phút (`ReAuthMinutes <= 5m`). |
| `PB-G03` | `PLAYBOOK-01` BẮT BUỘC cô lập 100% người học bằng `GLOBAL_EMERGENCY_HALT` và tăng `SecurityEpoch += 1` (D-091). |
| `PB-G04` | `PLAYBOOK-02` BẮT BUỘC đưa hệ thống về `READ_ONLY_MAINTENANCE` trước khi thực hiện restore CSDL. |
| `PB-G05` | `PLAYBOOK-03` BẮT BUỘC chuyển sang chế độ từ điển tĩnh fallback SLA $\le 100\text{ms}$ khi ngắt AI Gemini. |
| `PB-G06` | 100% các bước kích hoạt Playbook được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-46-PLAYBOOK`). |
| `PB-G07` | SLA thực thi kích hoạt từng bước can thiệp trong Playbook $< 500\text{ms}$. |
| `PB-G08` | Phân quyền khởi chạy Playbook sự cố trọng yếu chỉ dành cho `OnCallEngineer`, `IncidentCommander` và `DevOps`. |
| `PB-G09` | Hồ sơ đợt chạy Playbook lưu giữ minh bạch phục vụ kiểm toán nguyên nhân gốc Post-Mortem 48h (REL-07). |
| `PB-G10` | 100% các test case tự kiểm PB46-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PB46-01` | Kích hoạt `PLAYBOOK-01` khi nghi ngờ có rò rỉ dữ liệu người dùng | `GLOBAL_EMERGENCY_HALT` bật, thu hồi 100% Refresh Tokens |
| `PB46-02` | Kích hoạt `PLAYBOOK-02` khi phát hiện sai lệch CSDL SQL $> 15\%$ | Chuyển `READ_ONLY_MAINTENANCE`, chạy đối soát tự động |
| `PB46-03` | Kích hoạt `PLAYBOOK-03` khi Google Gemini API báo lỗi 500 liên tục | Bật `KILL_AI_GEMINI`, học viên chuyển đọc cache từ điển tĩnh |
| `PB46-04` | On-Call Engineer xác thực lại 2 phút trước bấm kích hoạt `PLAYBOOK-01` | Thực thi Playbook thành công 200 OK |
| `PB46-05` | On-Call Engineer thử kích hoạt `PLAYBOOK-01` khi lần Re-Auth cuối là 10 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `PB46-06` | Tra cứu vết Audit Log M11 sau khi hoàn tất đợt chạy `PLAYBOOK-01` | Ghi nhận Audit Event `ACT-M11-46-BREACH` |
| `PB46-07` | Hoàn tất các bước trong `PLAYBOOK-02` và mở lại hệ thống sang Normal | Đưa hệ thống về `NORMAL_OPERATIONS`, ghi nhận Audit Log |
| `PB46-08` | Kiểm tra thời gian kích hoạt các bước can thiệp khẩn cấp trong Playbook | Execution SLA $< 420\text{ms}$ |
| `PB46-09` | Tải đồng thời 20 truy vấn kiểm tra trạng thái Playbook đang chạy | Status SLA $< 5\text{ms}$ per request |
| `PB46-10` | Tự động tạo bản thảo báo cáo Post-Mortem 48h sau khi kết thúc Playbook | Khởi tạo DTO PostMortemTemplate kèm IncidentId |
| `PB46-11` | Thử nạp mã `PlaybookId` không hợp lệ (Ví dụ: `PLAYBOOK-99`) | Reject 400 `INVALID_PLAYBOOK_ID` |
| `PB46-12` | Gửi request kích hoạt Playbook khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `PB46-13` | User không phải On-Call/DevOps thử kích hoạt Playbook sự cố | Deny 403 Forbidden |
| `PB46-14` | User chưa đăng nhập gọi API tra cứu danh mục Playbook M11 | Deny 401 Unauthorized |
| `PB46-15` | Kích hoạt `PLAYBOOK-03` khôi phục lại AI Gemini sau khi kết thúc sự cố | Tắt cờ `KILL_AI_GEMINI`, đưa Circuit Breaker về CLOSED |
| `PB46-16` | Kiểm tra thời gian chuyển chế độ từ điển tĩnh fallback trong `PLAYBOOK-03` | Switch SLA $< 60\text{ms}$ |
| `PB46-17` | Phân tích tham chiếu các bản ghi `IncidentPlaybookExecutions` trong CSDL | Quét schema `M11_PlaybookExecutions` (T020) |
| `PB46-18` | Dịch vụ Outbox M12-T037 bị gián đoạn trong lúc phát sự kiện Playbook | Ghi trực tiếp CSDL trước, retry phát event qua Outbox |
| `PB46-19` | Tra cứu danh sách các đợt chạy Playbook sự cố trong 30 ngày qua | Trả về DTO danh sách PlaybookExecutions |
| `PB46-20` | Kiểm thử hoàn tất luồng playbook sự cố trọng yếu M11-CRITICAL-INCIDENT-PLAYBOOK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-PB-I01` | M11 hiện tại chưa có `IncidentPlaybookService` quản lý SOPs | Kỹ sư On-Call lúng túng thao tác thủ công khi sự cố xảy ra | M11-T049 (Source task) |
| `M11-PB-I02` | Thiếu cờ Re-Auth Guard $\le 5\text{m}$ khi kích hoạt Playbook sự cố | Kẻ xấu mượn máy On-Call tự ý kích hoạt ngắt dịch vụ | M11-T049; REL-02 |
| `M11-PB-I03` | Thiếu liên kết tự động giữa PLAYBOOK-01 và `GLOBAL_EMERGENCY_HALT` | Không ngắt sạch được các kết nối nguy hiểm khi bị Data Breach | M11-T049; M11-T044 |
| `M11-PB-I04` | Thiếu luồng tự động tạo bản thảo Post-Mortem 48h sau khi kết thúc | Bỏ sót hồ sơ đúc kết bài học sau sự cố thảm họa | M11-PB-F04; REL-07 |
| `M11-PB-I05` | Chưa kết nối sự kiện chạy Playbook với Audit Log M11 (`ACT-M11-46-PLAYBOOK`) | Không ghi vết được lịch sử can thiệp của On-Call Engineer | M11-T049; M11-T031 |

- `M11-PB-F01`: Triển khai `IncidentPlaybookService` với 3 Core SOP Playbooks (tiếp nhận: M11-T049).
- `M11-PB-F02`: Tích hợp Bắt buộc Re-Auth Guard $\le 5\text{m}$ & Integration Isolation (tiếp nhận: M11-T049; REL-02).
- `M11-PB-F03`: Triển khai Auto Post-Mortem Template Generation 48h (tiếp nhận: M11-T049; REL-07).
- `M11-PB-F04`: Thiết lập bộ kiểm thử tự động PB-G01–G10 và PB46-01–20 (tiếp nhận: M11 tasks).
- `M11-PB-F05`: Thu thập bằng chứng runtime cho luồng playbook M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T046

- Đã thiết kế hoàn chỉnh `M11-CRITICAL-INCIDENT-PLAYBOOK-1.0` với Ma trận Quy trình Thao tác Chuẩn Playbook.
- Đã chốt Ràng buộc Quy trình Thao tác Chuẩn 3 Kịch bản Thảm họa (`PLAYBOOK-01`, `PLAYBOOK-02`, `PLAYBOOK-03`).
- Đã chốt Ràng buộc Thực thi Thao tác Khóa An toàn Re-Auth $\le 5\text{m}$ (`Action Re-Auth Guard`).
- Đã lồng ghép Ràng buộc Lưu trữ Hồ sơ Playbook & Post-Mortem 48h (REL-07) và Audit Log M11 (`ACT-M11-46-PLAYBOOK`).
- Đã xác lập 10 Regression Gates (`PB-G01`–`PB-G10`) và 20 Test Cases tự kiểm (`PB46-01`–`PB46-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả playbook sự cố trọng yếu M11-T046 | WSA-7K2 |
