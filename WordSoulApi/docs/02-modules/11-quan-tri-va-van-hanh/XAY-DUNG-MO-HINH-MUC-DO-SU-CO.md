# Xây dựng mô hình mức độ sự cố M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-INCIDENT-SEVERITY-MODEL-1.0` |
| Task | M11-T045 |
| Đầu vào | M11-ALERT-ESCALATION-1.0 (D-132), M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), REL-03 |
| Phạm vi | Đặc tả Giao thức Mô hình Phân loại Mức độ Sự cố (`Incident Severity Taxonomy Model`), 4 mốc mức độ sự cố SEV-1 đến SEV-4, chỉ số SLA phản hồi và khắc phục MTTR, quy trình tự động nâng cấp mức độ sự cố Auto-Promotion và lưu vết kiểm toán |
| Tự kiểm | A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Mô hình Phân loại Mức độ Sự cố (`Incident Severity Taxonomy Model`) thuộc M11, xác lập tiêu chuẩn phân định mức độ nghiêm trọng của sự cố vận hành (Pháp lý, Hạ tầng, CSDL, AI Gemini, Thanh toán), cam kết chỉ số SLA thời gian tiếp nhận và khắc phục sự cố (MTTR - Mean Time To Resolution) tương ứng cho đội ngũ kỹ thuật.

- **Phân loại 4 Mức độ Sự cố Vận hành (`4 Incident Severity Levels Invariant`)**:
  - `SEV-1 (CRITICAL)`: Toàn bộ hệ thống sập hoặc trên $30\%$ người dùng bị ngắt quãng. Tiếp nhận SLA $\le 15\text{m}$, Khắc phục MTTR SLA $\le 1\text{h}$.
  - `SEV-2 (HIGH)`: Năng lực cốt lõi (Xác thực M01, Học tập M03) bị suy giảm $> 15\%$. Tiếp nhận SLA $\le 30\text{m}$, Khắc phục MTTR SLA $\le 4\text{h}$.
  - `SEV-3 (MODERATE)`: Một tính năng phụ bị ngắt (Upload avatar, Thống kê). Tiếp nhận SLA $\le 2\text{h}$, Khắc phục MTTR SLA $\le 24\text{h}$.
  - `SEV-4 (LOW)`: Lỗi giao diện nhỏ hoặc cải tiến non-blocking. Tiếp nhận SLA $\le 24\text{h}$, Khắc phục MTTR SLA $\le 7\text{d}$.
- **Quy trình Tự động Nâng cấp Mức độ Sự cố (`Automatic Severity Auto-Promotion`)**: Nếu sự cố mức `SEV-2` không có Kỹ sư On-Call tiếp nhận xử lý trong vòng 30 phút (`UnTriagedMinutes > 30m`), hệ thống TỰ ĐỘNG nâng cấp sự cố lên `SEV-1` và kích hoạt báo động PagerDuty tới Engineering Lead.
- **Ràng buộc Báo cáo Phân tích Nguyên nhân Gốc Post-Mortem 48h (`48-Hour Post-Mortem SLA`)**: 100% các sự kiện mức `SEV-1` và `SEV-2` BẮT BUỘC có báo cáo phân tích nguyên nhân gốc (Post-Mortem Root Cause Analysis) hoàn tất công khai trong vòng 48 giờ kể từ khi sự cố kết thúc (`PostMortemSlaHours = 48h`).
- **Lưu vết Sổ Kiểm toán Sự cố M11 (`Incident Taxonomy Audit Trail`)**: $100\%$ các đợt phân loại, nâng cấp hoặc đóng hồ sơ sự cố được ghi vết bất biến `ACT-M11-45-SEV` trong Sổ Kiểm toán M11.

## 2. Ma trận Phân loại Mức độ Sự cố và Chỉ số SLA (Incident Severity Matrix)

| Mức độ Sự cố (`SeverityLevel`) | Điều kiện Nhận diện Sự cố | SLA Tiếp nhận (`Response SLA`) | SLA Khắc phục (`MTTR SLA`) | Kênh Cảnh báo & Escalation | Thời hạn Post-Mortem | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **`SEV-1 (CRITICAL)`** | Global Outage, $>30\%$ users affected | **$\le 15$ phút** | **$\le 1$ giờ** | PagerDuty + Call On-Call | **48 Giờ (Bắt buộc)** | `ACT-M11-45-SEV1` |
| **`SEV-2 (HIGH)`** | Core Capability Degraded $>15\%$ | $\le 30$ phút | $\le 4$ giờ | Slack `#incidents-critical` | **48 Giờ (Bắt buộc)** | `ACT-M11-45-SEV2` |
| `SEV-3 (MODERATE)` | Non-core Feature Impaired | $\le 2$ giờ | $\le 24$ giờ | Slack `#incidents-moderate` | Không bắt buộc | `ACT-M11-45-SEV3` |
| `SEV-4 (LOW)` | Minor UI Bug / Non-blocking | $\le 24$ giờ | $\le 7$ ngày | Jira Backlog Queue | Không bắt buộc | `ACT-M11-45-SEV4` |

## 3. Kiến trúc Luồng Đánh giá và Leo thang Sự cố M11 (Incident Severity Pipeline)

```
[System Health Alert / User Incident Report Generated]
                         |
                         v
   [IncidentSeverityEngine: Classify Metrics & Thresholds]
                         |
      +------------------+------------------+
      | (Impact > 30%)                      | (Impact 15% - 30%)
      v                                     v
[Classify: SEV-1 CRITICAL]            [Classify: SEV-2 HIGH]
- SLA Response <= 15m                 - SLA Response <= 30m
- Call On-Call Engineer               - Alert Slack #incidents-critical
- Mandatory Post-Mortem 48h           - Start Un-Triaged Timer (30m)
                                                    |
                                      +-------------+-------------+
                                      | (Triaged <= 30m)          | (Un-Triaged > 30m)
                                      v                           v
                             [Maintain SEV-2]            [AUTO-PROMOTE TO SEV-1]
                             [Record Audit Log]          [Call Eng Lead PagerDuty]
```

## 4. Giao thức Thực thi Quản lý Mức độ Sự cố CSDL (IncidentSeverityService)

```csharp
public async Task<IncidentRecordDto> DeclareIncidentAsync(DeclareIncidentCommand cmd, string reporterUserId)
{
    // 1. Calculate Initial Severity Level
    var severity = CalculateSeverityLevel(cmd.AffectedUserPercentage, cmd.IsCoreCapabilityDown);

    var incident = new IncidentRecord {
        IncidentId = Guid.NewGuid().ToString("N"),
        Title = cmd.Title,
        Severity = severity,
        Status = IncidentStatus.DECLARED,
        DeclaredAtUtc = DateTime.UtcNow,
        ResponseSlaDeadlineUtc = DateTime.UtcNow.AddMinutes(severity == SeverityLevel.SEV_1 ? 15 : 30),
        MttrSlaDeadlineUtc = DateTime.UtcNow.AddHours(severity == SeverityLevel.SEV_1 ? 1 : 4)
    };

    _db.IncidentRecords.Add(incident);
    await _db.SaveChangesAsync();

    // 2. Dispatch Escalation Alert D-132
    await _alertService.DispatchIncidentAlertAsync(incident.IncidentId, severity, cmd.Title);

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-45-SEV", reporterUserId, new {
        IncidentId = incident.IncidentId,
        Severity = severity.ToString(),
        ResponseSlaDeadline = incident.ResponseSlaDeadlineUtc
    });

    return incident.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IS-G01` | Phân loại sự cố BẮT BUỘC tuân thủ 4 mốc chuẩn (`SEV-1`, `SEV-2`, `SEV-3`, `SEV-4`). |
| `IS-G02` | Sự cố mức `SEV-1` BẮT BUỘC đảm bảo SLA tiếp nhận $\le 15$ phút và MTTR khắc phục $\le 1$ giờ. |
| `IS-G03` | Sự cố `SEV-2` không được tiếp nhận xử lý trong 30 phút BẮT BUỘC tự động nâng cấp lên `SEV-1`. |
| `IS-G04` | 100% các sự cố mức `SEV-1` và `SEV-2` BẮT BUỘC hoàn tất báo cáo Post-Mortem trong vòng 48 giờ. |
| `IS-G05` | SLA thực thi đánh giá phân loại mức độ sự cố tự động từ chỉ số metric $< 10\text{ms}$. |
| `IS-G06` | 100% các thao tác khai báo, nâng cấp hoặc đóng hồ sơ sự cố được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-45-SEV`). |
| `IS-G07` | Phân quyền hạ mức độ sự cố (Downgrade Severity) chỉ dành cho `IncidentCommander` và `EngLead`. |
| `IS-G08` | Hệ thống hỗ trợ quản lý theo dõi đồng thời 20 sự cố đang diễn ra mà không bị xáo trộn SLA. |
| `IS-G09` | Tự động đồng bộ trạng thái sự cố sang hệ thống PagerDuty/Slack công khai cho toàn công ty. |
| `IS-G10` | 100% các test case tự kiểm IS45-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IS45-01` | Khai báo sự cố ngắt kết nối CSDL toàn phần (Tác động $>30\%$ users) | Tự động phân loại `SEV-1`, tạo SLA tiếp nhận 15m |
| `IS45-02` | Khai báo sự cố dịch vụ AI Gemini bị nghẽn (Tác động $20\%$ users) | Tự động phân loại `SEV-2`, tạo SLA tiếp nhận 30m |
| `IS45-03` | Sự cố `SEV-2` chưa được On-Call Engineer bấm "Acknowledge" sau 32 phút | Auto-Promote sang `SEV-1`, gọi PagerDuty Eng Lead |
| `IS45-04` | Sự cố `SEV-1` đã được khắc phục xong sau 40 phút (Đạt MTTR $< 1\text{h}$) | Đánh dấu RESOLVED, kích hoạt countdown SLA 48h Post-Mortem |
| `IS45-05` | IncidentCommander gửi báo cáo Post-Mortem 36 giờ sau khi xong sự cố SEV-1 | Nhận báo cáo thành công (Đạt SLA $< 48\text{h}$) |
| `IS45-06` | Tra cứu vết Audit Log M11 sau khi tự động Auto-Promote sự cố | Ghi nhận Audit Event `ACT-M11-45-PROMOTE` |
| `IS45-07` | Thử hạ mức độ sự cố từ `SEV-1` xuống `SEV-3` bởi SupportAgent | Reject 403 `INCIDENT_DOWNGRADE_DENIED` |
| `IS45-08` | Khai báo sự cố lỗi hiển thị typo ở trang Profile | Tự động phân loại `SEV-4`, tạo SLA MTTR 7 ngày |
| `IS45-09` | Tải đồng thời 50 request cập nhật tiến độ sự cố từ Incident Team | Update SLA $< 8\text{ms}$ per request |
| `IS45-10` | Kiểm tra thời gian kích hoạt PagerDuty Call khi sự cố SEV-1 phát sinh | Dispatch SLA $< 30\text{s}$ |
| `IS45-11` | Thử khai báo sự cố với thông số tác động âm ($< 0\%$) | Reject 400 `INVALID_AFFECTED_PERCENTAGE` |
| `IS45-12` | Gửi request khai báo sự cố khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `IS45-13` | User không phải On-Call/Admin thử đóng hồ sơ sự cố SEV-1 | Deny 403 Forbidden |
| `IS45-14` | User chưa đăng nhập gọi API tra cứu bảng tổng hợp sự cố M11 | Deny 401 Unauthorized |
| `IS45-15` | Đóng hồ sơ sự cố SEV-1 nhưng CHƯA nộp báo cáo Post-Mortem quá 48h | Cảnh báo WARN `POST_MORTEM_OVERDUE` tới Eng Lead |
| `IS45-16` | Kiểm tra thời gian tính toán lại MTTR thực tế sau khi closed | Calculation SLA $< 1\text{ms}$ |
| `IS45-17` | Phân tích tham chiếu các bản ghi `IncidentRecords` trong CSDL | Quét schema `M11_IncidentRecords` (T020) |
| `IS45-18` | Dịch vụ Slack bị đứt khi phát tin thông báo sự cố | Fallback thông báo qua Email On-Call List |
| `IS45-19` | Tra cứu danh sách các sự cố `SEV-1` và `SEV-2` đang diễn ra | Trả về DTO danh sách Active Critical Incidents |
| `IS45-20` | Kiểm thử hoàn tất mô hình mức độ sự cố M11-INCIDENT-SEVERITY-MODEL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-IS-I01` | M11 hiện tại chưa có `IncidentSeverityEngine` phân loại tự động | Risk xử lý chậm các sự cố nghiêm trọng do đánh giá cảm tính | M11-T049 (Source task) |
| `M11-IS-I02` | Thiếu cờ Tự động Nâng cấp Auto-Promotion cho SEV-2 un-triaged 30m | Sự cố quan trọng bị bỏ quên khi On-Call vắng mặt | M11-T049; REL-03 |
| `M11-IS-I03` | Thiếu luồng theo dõi SLA Post-Mortem 48h cho SEV-1/SEV-2 | Đội ngũ không rút kinh nghiệm để ngăn sự cố lặp lại | M11-T049; M11-T046 |
| `M11-IS-I04` | Thiếu cờ Phân quyền Hạ mức độ sự cố nghiêm ngặt cho IncidentCommander | Risk người khai báo tự ý hạ mức độ sự cố để giảm áp lực | M11-IS-F04; M11-T004 |
| `M11-IS-I05` | Chưa kết nối sự kiện phân loại sự cố với Audit Log M11 (`ACT-M11-45-SEV`) | Không ghi vết được lịch sử thay đổi mức độ sự cố | M11-T049; M11-T031 |

- `M11-IS-F01`: Triển khai `IncidentSeverityEngine` với 4 Mức độ Sự cố Standard (tiếp nhận: M11-T049).
- `M11-IS-F02`: Tích hợp Bắt buộc Auto-Promotion Guard cho SEV-2 Un-Triaged 30m (tiếp nhận: M11-T049; REL-03).
- `M11-IS-F03`: Triển khai 48-Hour Post-Mortem SLA & Downgrade Restriction (tiếp nhận: M11-T049; M11-T046).
- `M11-IS-F04`: Thiết lập bộ kiểm thử tự động IS-G01–G10 và IS45-01–20 (tiếp nhận: M11 tasks).
- `M11-IS-F05`: Thu thập bằng chứng runtime cho luồng sự cố M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T045

- Đã thiết kế hoàn chỉnh `M11-INCIDENT-SEVERITY-MODEL-1.0` với Ma trận Phân loại Mức độ Sự cố và Chỉ số SLA.
- Đã chốt Ràng buộc Phân loại 4 Mức độ Sự cố Vận hành (`SEV-1` đến `SEV-4`).
- Đã chốt Ràng buộc Quy trình Tự động Nâng cấp Mức độ Sự cố (`Automatic Severity Auto-Promotion` 30m).
- Đã lồng ghép Ràng buộc Báo cáo Phân tích Nguyên nhân Gốc Post-Mortem 48h (`48-Hour Post-Mortem SLA`) và Audit Log M11 (`ACT-M11-45-SEV`).
- Đã xác lập 10 Regression Gates (`IS-G01`–`IS-G10`) và 20 Test Cases tự kiểm (`IS45-01`–`IS45-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả mô hình mức độ sự cố M11-T045 | WSA-7K2 |
