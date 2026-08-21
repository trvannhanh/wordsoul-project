# Thiết kế truyền thông và hậu kiểm — lát A M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-COMMUNICATION-POST-MORTEM-A-1.0` |
| Task | M11-T047-A |
| Đầu vào | M11-CRITICAL-INCIDENT-PLAYBOOK-1.0 (D-141), M11-AUDIT-EVENT-1.0 (D-054), REL-06 |
| Phạm vi | Đặc tả Giao thức Truyền thông Sự cố và Hậu kiểm Lát A (`Incident Communication & Post-Mortem Protocol - Slice A`), ma trận truyền thông nội bộ/bên ngoài, quy trình đánh giá hậu kiểm Blameless Post-Mortem 48h, giao nhiệm vụ khắc phục và lưu vết kiểm toán |
| Tự kiểm | A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Truyền thông Sự cố và Hậu kiểm Lát A (`Incident Communication & Post-Mortem Protocol - Slice A`) thuộc M11, chuẩn hóa kênh phát tin thông báo khi xảy ra sự cố nghiêm trọng cho cả đội ngũ nội bộ và người học bên ngoài, đồng thời quy định quy trình họp hậu kiểm rút kinh nghiệm không quy chụp trách nhiệm (Blameless Post-Mortem) hoàn tất trong 48 giờ để cải tiến hệ thống bền vững (REL-06).

- **Ma trận Truyền thông Sự cố 2 Chiều (`2-Way Incident Communication Matrix Invariant`)**:
  - *Nội bộ (Internal)*: Kênh Slack `#incidents-warroom`. Khi xảy ra sự cố mức `SEV-1`, BẮT BUỘC cập nhật tiến độ xử lý mỗi 15 phút (`UpdateFrequencyMinutes = 15m`).
  - *Bên ngoài (External/Learner)*: Trang Status Page `status.wordsoul.app` và M10 In-App Banners. BẮT BUỘC phát tin thông báo tới người học trong vòng SLA $\le 5$ phút sau khi khai báo `SEV-1` hoặc `SEV-2` (REL-06).
- **Quy trình Hậu kiểm Không Quy chụp Blameless Post-Mortem 48h (`Blameless Post-Mortem 48h Invariant`)**: 100% sự cố mức `SEV-1` và `SEV-2` BẮT BUỘC tổ chức cuộc họp hậu kiểm Blameless Post-Mortem. Báo cáo hoàn tất BẮT BUỘC xuất bản công khai trong vòng 48 giờ (`PostMortemSlaHours = 48h`), tập trung vào nguyên nhân gốc (Root Cause) và điểm yếu hệ thống thay vì đổ lỗi cá nhân.
- **Ràng buộc Tạo Ticket Nhiệm vụ Khắc phục Action Items (`Action Item Ticket Enforcement`)**: 100% các đề xuất hành động ngăn ngừa (Preventative Action Items) từ báo cáo Post-Mortem BẮT BUỘC được chuyển thành Ticket nhiệm vụ trên hệ thống Jira/GitHub kèm người chịu trách nhiệm (`ActionItemOwnerRequired = true`) và thời hạn hoàn tất $\le 14$ ngày.
- **Lưu vết Sổ Kiểm toán Truyền thông M11 (`Communication Audit Trail`)**: $100\%$ các đợt phát tin thông báo sự cố, xuất bản báo cáo Post-Mortem hoặc tạo Action Item ticket được ghi vết bất biến `ACT-M11-47-COMM` trong Sổ Kiểm toán M11.

## 2. Ma trận Truyền thông Sự cố (Incident Communication Matrix)

| Mức độ Sự cố (`SeverityLevel`) | Kênh Truyền thông Nội bộ | Tần suất Cập nhật Nội bộ | Kênh Truyền thông Bên ngoài | SLA Phát tin Bên ngoài | Thời hạn Post-Mortem | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **`SEV-1 (CRITICAL)`** | Slack `#incidents-warroom` | **Mỗi 15 Phút** | Status Page + M10 In-App Banner | **SLA $\le 5$ Phút** | **48 Giờ (Bắt buộc)** | `ACT-M11-47-COMM1` |
| **`SEV-2 (HIGH)`** | Slack `#incidents-critical` | Mỗi 30 Phút | Status Page + M10 Banner | **SLA $\le 15$ Phút** | **48 Giờ (Bắt buộc)** | `ACT-M11-47-COMM2` |
| `SEV-3 (MODERATE)` | Slack `#incidents-moderate` | Khi có tiến triển | Status Page Component Status | SLA $\le 60$ Phút | Không bắt buộc | `ACT-M11-47-COMM3` |
| `SEV-4 (LOW)` | Internal Jira Ticket | N/A | N/A (Không phát tin) | N/A | Không bắt buộc | `ACT-M11-47-COMM4` |

## 3. Kiến trúc Luồng Truyền thông và Hậu kiểm M11 (Communication & Post-Mortem Pipeline)

```
[Incident Declared (SEV-1 / SEV-2)]
                 |
                 v
 [IncidentCommunicationEngine: Dispatch Alerts]
                 |
       +---------+---------+
       | (Internal)        | (External)
       v                   v
[Post Slack #warroom]  [Update Status Page & M10 Banner SLA <= 5m]
[Update Every 15m]     [Inform Learners of Progress]
       |                   |
       +---------+---------+
                 |
                 v
    [Incident Resolved -> Start 48h Countdown]
                 |
                 v
    [Hold Blameless Post-Mortem Meeting]
                 |
                 v
    [Publish Post-Mortem Report & Assign Action Item Tickets <= 14d]
    [Record Audit Log ACT-M11-47-COMM]
```

## 4. Giao thức Thực thi Truyền thông CSDL (IncidentCommunicationService)

```csharp
public async Task DispatchIncidentCommunicationAsync(
    string incidentId, 
    string statusMessage, 
    string actorUserId)
{
    var incident = await _db.IncidentRecords.FirstOrDefaultAsync(i => i.IncidentId == incidentId);
    if (incident == null) throw new KeyNotFoundException("INCIDENT_NOT_FOUND");

    // 1. Dispatch Internal Communication via Slack Webhook
    string channel = incident.Severity == SeverityLevel.SEV_1 ? "#incidents-warroom" : "#incidents-critical";
    await _slackClient.SendMessageAsync(channel, $"[{incident.Severity}] Incident {incidentId}: {statusMessage}");

    // 2. Dispatch External Communication via Status Page & M10 Banners SLA <= 5m
    if (incident.Severity == SeverityLevel.SEV_1 || incident.Severity == SeverityLevel.SEV_2)
    {
        await _statusPageClient.UpdateStatusAsync(incident.IncidentId, statusMessage);

        // Publish In-App Banner Event via M10
        await _eventBus.PublishAsync(new IncidentPublicBannerIntegrationEvent {
            IncidentId = incidentId,
            Severity = incident.Severity.ToString(),
            PublicMessage = "Hệ thống đang gặp sự cố kết nối gián đoạn. Đội ngũ kỹ thuật đang khắc phục khẩn cấp.",
            UpdatedAtUtc = DateTime.UtcNow
        });
    }

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-47-COMM", actorUserId, new {
        IncidentId = incidentId,
        Severity = incident.Severity.ToString(),
        StatusMessage = statusMessage,
        Channel = channel
    });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CP-G01` | Ma trận truyền thông BẮT BUỘC phân định rõ kênh phát tin nội bộ và kênh phát tin bên ngoài người học. |
| `CP-G02` | Sự cố mức `SEV-1` BẮT BUỘC phát tin thông báo tới người học qua Status Page/M10 Banner SLA $\le 5$ phút. |
| `CP-G03` | Cập nhật tiến độ xử lý nội bộ cho sự cố `SEV-1` BẮT BUỘC duy trì mỗi 15 phút trên Slack `#incidents-warroom`. |
| `CP-G04` | Cuộc họp hậu kiểm BẮT BUỘC tuân thủ nguyên tắc Blameless (Không quy chụp cá nhân, tập trung cải tiến hệ thống). |
| `CP-G05` | 100% báo cáo Post-Mortem sự cố SEV-1/SEV-2 BẮT BUỘC xuất bản hoàn tất trong 48 giờ kể từ khi đóng sự cố. |
| `CP-G06` | 100% các đề xuất hành động (Action Items) BẮT BUỘC được gán Ticket nhiệm vụ Jira/GitHub có người chịu trách nhiệm. |
| `CP-G07` | SLA thực thi đẩy thông báo PUSH/Banner sự cố tới M10 Event Bus $< 500\text{ms}$. |
| `CP-G08` | Phân quyền phê duyệt phát tin thông báo công khai bên ngoài chỉ dành cho `IncidentCommander` và `PRLead`. |
| `CP-G09` | Hệ thống Status Page độc lập với hạ tầng chính, đảm bảo vẫn hoạt động khi API Server sập. |
| `CP-G10` | 100% các test case tự kiểm CP47-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CP47-01` | Khai báo sự cố SEV-1 ngắt kết nối CSDL toàn phần | Tự động đăng tin công khai trên Status Page SLA $< 3.5\text{m}$ |
| `CP47-02` | Sự cố SEV-1 diễn ra trong 45 phút | Phát đủ 3 bản cập nhật tiến độ nội bộ trên Slack (mỗi 15m) |
| `CP47-03` | Sự cố SEV-1 được đóng lúc 10:00 UTC, tổ chức họp Blameless Post-Mortem | Hoàn tất xuất bản báo cáo Post-Mortem trước 48h SLA |
| `CP47-04` | Tạo 3 Action Items từ báo cáo Post-Mortem nhưng KHÔNG gán Assignee | Reject 400 `ACTION_ITEM_OWNER_REQUIRED` |
| `CP47-05` | IncidentCommander xác thực lại 2 phút trước bấm nút "Phát tin Công khai" | Đăng tin thông báo lên Banner ứng dụng M10 thành công |
| `CP47-06` | Tra cứu vết Audit Log M11 sau khi phát tin thông báo sự cố SEV-1 | Ghi nhận Audit Event `ACT-M11-47-COMM1` |
| `CP47-07` | SupportAgent thử tự ý đăng tin cảnh báo thảm họa lên Status Page | Reject 403 `STATUS_PAGE_UPDATE_DENIED` |
| `CP47-08` | Tra cứu báo cáo Post-Mortem sự cố SEV-1 đã xuất bản | Trả về DTO báo cáo đầy đủ Root Cause & Timeline |
| `CP47-09` | Tải đồng thời 1,000 request đọc trang tin Status Page | Status Page response SLA $< 45\text{ms}$ |
| `CP47-10` | Tự động đóng Banner sự cố trên ứng dụng M10 khi sự cố chuyển RESOLVED | Phát event thu hồi Banner tới người học SLA $< 5\text{s}$ |
| `CP47-11` | Thử nạp nội dung thông báo chứa mã HTML/JavaScript độc hại | Sanitize sạch thông điệp trước khi đăng lên Status Page |
| `CP47-12` | Gửi request phát tin truyền thông khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `CP47-13` | User không phải IncidentCommander/PRLead thử cập nhật Status Page | Deny 403 Forbidden |
| `CP47-14` | User chưa đăng nhập gọi API tra cứu danh sách Post-Mortems M11 | Deny 401 Unauthorized |
| `CP47-15` | Cập nhật thời hạn hoàn tất Action Item ticket thành 20 ngày ($> 14$d) | Reject 400 `ACTION_ITEM_DUE_DATE_EXCEEDED_14D` |
| `CP47-16` | Kiểm tra thời gian đồng bộ trạng thái sự cố sang M10 Integration Event | Propagation SLA $< 120\text{ms}$ |
| `CP47-17` | Phân tích tham chiếu các bản ghi `PostMortemReports` trong CSDL | Quét schema `M11_PostMortemReports` (T020) |
| `CP47-18` | Kênh Slack Webhook bị lỗi khi phát tin truyền thông nội bộ | Fallback gửi Email thông báo khẩn tới Incident Warroom |
| `CP47-19` | Tra cứu danh sách các Action Items chưa hoàn tất của các sự cố cũ | Trả về DTO danh sách PendingActionItems |
| `CP47-20` | Kiểm thử hoàn tất luồng truyền thông và hậu kiểm Lát A M11-COMMUNICATION-POST-MORTEM-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-CP-I01` | M11 hiện tại chưa có `IncidentCommunicationService` phát tin | Risk truyền thông chậm trễ gây hoảng loạn cho người học | M11-T049 (Source task) |
| `M11-CP-I02` | Thiếu cờ SLA $\le 5$ phút phát tin bên ngoài qua M10 Banner | Người học không biết lý do hệ thống bị ngắt kết nối | M11-T049; REL-06 |
| `M11-CP-I03` | Thiếu luồng Quản lý Báo cáo Blameless Post-Mortem 48h | Không lưu trữ được bài học kinh nghiệm từ sự cố | M11-T049; M11-T046 |
| `M11-CP-I04` | Thiếu cờ Bắt buộc Assignee cho các Action Item tickets | Đề xuất khắc phục bị bỏ quên không người thực hiện | M11-CP-F04; M11-T004 |
| `M11-CP-I05` | Chưa kết nối sự kiện truyền thông với Audit Log M11 (`ACT-M11-47-COMM`) | Không ghi vết được ai đã duyệt đăng thông điệp ra ngoài | M11-T049; M11-T031 |

- `M11-CP-F01`: Triển khai `IncidentCommunicationService` với 2-Way Matrix (tiếp nhận: M11-T049).
- `M11-CP-F02`: Tích hợp Bắt buộc Status Page & M10 Banner Broadcast SLA $\le 5\text{m}$ (tiếp nhận: M11-T049; REL-06).
- `M11-CP-F03`: Triển khai Blameless Post-Mortem 48h & Mandatory Action Item Assignees (tiếp nhận: M11-T049; M11-T046).
- `M11-CP-F04`: Thiết lập bộ kiểm thử tự động CP-G01–G10 và CP47-01–20 (tiếp nhận: M11 tasks).
- `M11-CP-F05`: Thu thập bằng chứng runtime cho luồng truyền thông M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T047-A

- Đã thiết kế hoàn chỉnh `M11-COMMUNICATION-POST-MORTEM-A-1.0` với Ma trận Truyền thông Sự cố.
- Đã chốt Ràng buộc Ma trận Truyền thông Sự cố 2 Chiều (Nội bộ Slack #warroom 15m, Bên ngoài Status Page/M10 SLA $\le 5\text{m}$ REL-06).
- Đã chốt Ràng buộc Quy trình Hậu kiểm Không Quy chụp Blameless Post-Mortem 48h (`PostMortemSlaHours = 48h`).
- Đã lồng ghép Ràng buộc Tạo Ticket Nhiệm vụ Khắc phục Action Items (Bắt buộc Assignee, thời hạn $\le 14$d) và Audit Log M11 (`ACT-M11-47-COMM`).
- Đã xác lập 10 Regression Gates (`CP-G01`–`CP-G10`) và 20 Test Cases tự kiểm (`CP47-01`–`CP47-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả truyền thông và hậu kiểm Lát A M11-T047-A | WSA-7K2 |
