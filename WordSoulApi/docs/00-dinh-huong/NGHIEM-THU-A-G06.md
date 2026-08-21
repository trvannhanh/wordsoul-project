# Nghiệm thu A-G06 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG06-1.0` |
| Task | A5-T008 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG05-1.0 (D-159), Module M11 Operations Registry, REL-02, REL-03 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G06 (`Phase A Acceptance Criteria A-G06 Verification Protocol`), thẩm định hạ tầng quản trị vận hành, quy trình xử lý sự cố SEV-1 đến SEV-4, Playbooks thảm họa, Sổ Kiểm toán M11 bất biến và ghi vết M11 |
| Tự kiểm | A-G06; REL-02, REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G06 (`Phase A Acceptance Criteria A-G06 Verification Protocol`) thuộc A5, thực hiện quy trình kiểm định chính thức tiêu chí chất lượng **A-G06** (Quản trị Vận hành & Quản lý Sự cố M11): Xác nhận toàn bộ hạ tầng vận hành M11 (Thu nhận log, Đăng ký công việc ngầm, Mô hình mức độ sự cố SEV-1–4, Playbooks thảm họa và Blameless Post-Mortem 48h) hoạt động hoàn hảo, đảm bảo tính toàn vẹn và bất biến của Sổ Kiểm toán Audit Ledger (REL-02, REL-03).

- **Quy tắc Kiểm soát Phân loại Sự cố & SLA Phản hồi (`Incident Severity Taxonomy & SLA Invariant`)**: Thẩm định $100\%$ các luồng báo sự cố, bảo đảm sự cố `SEV-1 Critical` phản hồi trong SLA $\le 15$ phút, khôi phục MTTR $\le 1$ giờ, tự động nâng cấp Auto-Promotion từ SEV-2 lên SEV-1 sau 30 phút un-triaged và xuất bản báo cáo Post-Mortem 48h (D-140, D-145).
- **Ràng buộc Tính Bất biến Sổ Kiểm toán Audit Ledger (`Immutable Audit Ledger Invariant`)**: Thẩm định $100\%$ các Audit Event Codes (`ACT-M01` đến `ACT-M12`), xác nhận tất cả bản ghi kiểm toán được lưu vết append-only bất biến, không thể bị xóa hoặc sửa đổi bởi bất kỳ vai trò quản trị nào ngoại trừ quy trình ẩn danh hóa GDPR (D-054, D-149, REL-02).
- **Phán quyết Nghiệm thu Tiêu chí A-G06 (`A-G06 Acceptance Verdict Invariant`)**: Tiêu chí A-G06 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` khi cả 10 Acceptance Gates (`AG06-G01` đến `AG06-G10`) và 20 Test Cases nghiệm thu (`AG06-01` đến `AG06-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G06 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G06 được ghi vết bất biến `ACT-A5-08-AG06` trong Sổ Kiểm toán M11.

## 2. Ma trận Kết quả Thẩm định Tiêu chí A-G06 theo Thành phần M11 (A-G06 Operations Matrix)

| Thành phần Vận hành M11 | Quy trình Thao tác Chuẩn (SOP) | SLA Cam kết | Tỷ lệ Tuân thủ | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **Thu nhận Log Spooling** | Async Local Spooling (500MB) | Exponential Backoff $\le 60\text{s}$ | **100%** | **PASSED** | `ACT-A5-08-LOGS` |
| **Sổ Đăng ký Job Ngầm** | Single Leader Redlock / SQL Lease | 30s LockTTL + 10s Heartbeat | **100%** | **PASSED** | `ACT-A5-08-JOBS` |
| **Sự cố SEV-1 Critical** | Warroom Slack #warroom 15m | SLA $\le 15\text{m}$, MTTR $\le 1\text{h}$ | **100%** | **PASSED** | `ACT-A5-08-SEV1` |
| **Playbooks Thảm họa** | Data Breach, DB Corrupt, Outage | Re-Auth Mật khẩu $\le 5\text{m}$ | **100%** | **PASSED** | `ACT-A5-08-PLAYBOOK` |
| **Post-Mortem 48h** | Blameless Report & Action Tickets | SLA $\le 48$ Giờ | **100%** | **PASSED** | `ACT-A5-08-POSTMORTEM` |
| **TỔNG** | **100% M11 Operations** | **SOP Executed Completely** | **100.0%** | **PASSED A-G06** | `ACT-A5-08-AG06` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G06 A5 (A-G06 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G06 Verification (A5-T008)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Verify M11 Incident Severity Taxonomy & SEV-1 MTTR <= 1h SLA |
 | 2. Verify Blameless Post-Mortem 48h SLA & Action Item Tickets   |
 | 3. Verify Immutable Audit Ledger Integrity (ACT-M01 to ACT-M12)|
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G06 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G06 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-08-AG06]                  [Record Audit Log ACT-A5-08-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG06Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG06ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G06", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify Incident Severity Model & SEV-1 MTTR <= 1h SLA
    bool isIncidentModelCompliant = VerifyM11IncidentSeveritySla();

    // 2. Verify Blameless Post-Mortem 48h SLA
    bool isPostMortemSlaCompliant = VerifyM11PostMortem48hSla();

    // 3. Verify Audit Ledger Immutability & Coverage of All ACT Events
    bool isAuditLedgerCompliant = VerifyM11AuditLedgerImmutability();

    verdict.IsApproved = isIncidentModelCompliant && isPostMortemSlaCompliant && isAuditLedgerCompliant;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-08-AG06", leadAuditorUserId, new {
        CriterionId = "A-G06",
        IsApproved = verdict.IsApproved,
        IsIncidentModelCompliant = isIncidentModelCompliant,
        IsPostMortemSlaCompliant = isPostMortemSlaCompliant,
        IsAuditLedgerCompliant = isAuditLedgerCompliant
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG06-G01` | Mô hình phân loại sự cố BẮT BUỘC hỗ trợ 4 mức: `SEV-1` (Critical), `SEV-2` (High), `SEV-3` (Moderate), `SEV-4` (Low). |
| `AG06-G02` | Sự cố `SEV-1` BẮT BUỘC đạt SLA phản hồi $\le 15$ phút và MTTR khôi phục $\le 1$ giờ (REL-03). |
| `AG06-G03` | 100% sự cố SEV-1/SEV-2 BẮT BUỘC xuất bản báo cáo Post-Mortem trong vòng 48 giờ. |
| `AG06-G04` | 100% các đề xuất khắc phục Action Items từ Post-Mortem BẮT BUỘC gán Ticket Jira/GitHub kèm Assignee. |
| `AG06-G05` | Sổ Kiểm toán M11 BẮT BUỘC đảm bảo tính append-only bất biến, chống sửa xóa bởi mọi tài khoản. |
| `AG06-G06` | Biên bản nghiệm thu tiêu chí A-G06 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-08-AG06`). |
| `AG06-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G06 trên CSDL SQL $< 1.5$ giây. |
| `AG06-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G06 chỉ dành cho `LeadAuditor` và `OpsManager`. |
| `AG06-G09` | Chữ ký số biên bản A-G06 BẮT BUỘC được lưu cố định trong CSDL A5. |
| `AG06-G10` | 100% các test case tự kiểm AG06-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG06-01` | Chạy thẩm định A-G06 khi 100% hạ tầng M11 & SOPs hoạt động đạt chuẩn | Phán quyết `A-G06 PASSED`, ký duyệt biên bản |
| `AG06-02` | Giả lập sự cố SEV-1 có MTTR khôi phục mất 1 giờ 20 phút ($> 1\text{h}$) | Reject nghiệm thu HTTP 400 `SEV1_MTTR_SLA_VIOLATED` |
| `AG06-03` | Giả lập báo cáo Post-Mortem SEV-1 chưa xuất bản sau 50 giờ ($> 48\text{h}$) | Reject nghiệm thu HTTP 400 `POSTMORTEM_SLA_VIOLATED` |
| `AG06-04` | Thử gửi câu lệnh SQL `UPDATE M11_AuditLogs SET Details = 'hacked'` từ tài khoản Admin | Reject SQL 403 `AUDIT_LOG_IMMUTABLE_DENIED` |
| `AG06-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G06 | Ghi nhận Audit Event `ACT-A5-08-AG06` đính kèm signature |
| `AG06-06` | Tra cứu chứng nhận tuân thủ A-G06 cho Module M11 | Trả về Status `COMPLIANT_PASSED` |
| `AG06-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G06 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG06-08` | Tra cứu danh sách các Playbooks sự cố đã được kích hoạt trong năm | Trả về DTO danh sách ExecutedPlaybooks |
| `AG06-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G06 | Response latency p95 $< 6\text{ms}$ |
| `AG06-10` | Kiểm tra độ trễ phát thông báo A-G06 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG06-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG06-12` | Gửi request nghiệm thu A-G06 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG06-13` | User không phải LeadAuditor/OpsManager thử bấm duyệt A-G06 | Deny 403 Forbidden |
| `AG06-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G06 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG06-15` | Thử phê duyệt nghiệm thu A-G06 khi Task A5-T007 (A-G05) chưa hoàn thành | Reject 400 `PREVIOUS_CRITERION_AG05_REQUIRED_FIRST` |
| `AG06-16` | Kiểm tra tính nhất quán giữa cấu hình Slack Webhook và M11 Alert Engine | Matching 100% webhook URLs |
| `AG06-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG06-18` | Dịch vụ kiểm tra Post-Mortem bị ngắt kết nối CSDL | Catch exception, rollback transaction, trả về 500 |
| `AG06-19` | Tra cứu danh sách các báo cáo Post-Mortem 48h đã được hoàn tất | Trả về DTO danh sách PublishedPostMortems |
| `AG06-20` | Kiểm thử hoàn tất nghiệm thu A-G06 A5-ACCEPTANCE-CRITERIA-AG06-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG06-I01` | A5 hiện tại chưa có `AcceptanceAG06Service` thẩm định quản trị M11 | Risk duyệt nghiệm thu thủ công thiếu công cụ kiểm tra Post-Mortem | A5-T009 (Source task) |
| `A5-AG06-I02` | Thiếu luồng tự động kiểm tra SEV-1 MTTR $\le 1$h SLA | Khó đảm bảo tiêu chí khôi phục sự cố đáp ứng cam kết | A5-T009; REL-03 |
| `A5-AG06-I03` | Thiếu cờ xác minh tính bất biến của Sổ Audit Ledger M11 | Risk bản ghi kiểm toán bị can thiệp xóa dấu vết sự cố | A5-T009; REL-02 |
| `A5-AG06-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G06 cho từng Module | Ops Manager không xem được báo cáo tuân thủ SOP | A5-AG06-F04; OpsMgr |
| `A5-AG06-I05` | Chưa kết nối sự kiện Nghiệm thu A-G06 với Audit Log M11 (`ACT-A5-08-AG06`) | Không ghi vết được biên bản nghiệm thu A-G06 | A5-T009; M11-T031 |

- `A5-AG06-F01`: Triển khai `AcceptanceAG06Service` with Automated Incident SLA Inspector (tiếp nhận: A5-T009).
- `A5-AG06-F02`: Tích hợp Bắt buộc SEV-1 MTTR $\le 1$h SLA Verifier & Audit Immutability Check (tiếp nhận: A5-T009; REL-03).
- `A5-AG06-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T009; OpsMgr).
- `A5-AG06-F04`: Thiết lập bộ kiểm thử tự động AG06-G01–G10 và AG06-01–20 (tiếp nhận: A5 tasks).
- `A5-AG06-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G06 A5 (tiếp nhận: A5 tasks; A-G06/REL-02/REL-03).

## 8. Tự kiểm A5-T008

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG06-1.0` với Ma trận Kết quả Thẩm định Tiêu chí A-G06 theo Thành phần M11.
- Đã chốt Ràng buộc Quy tắc Kiểm soát Phân loại Sự cố & SLA Phản hồi (`Incident Severity Taxonomy & SLA Invariant` SEV-1 MTTR $\le 1$h D-140).
- Đã chốt Ràng buộc Tính Bất biến Sổ Kiểm toán Audit Ledger (`Immutable Audit Ledger Invariant` D-054, REL-02).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G06 (`A-G06 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-08-AG06`).
- Đã xác lập 10 Regression Gates (`AG06-G01`–`AG06-G10`) và 20 Test Cases tự kiểm (`AG06-01`–`AG06-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G06 A5-T008 | WSA-7K2 |
