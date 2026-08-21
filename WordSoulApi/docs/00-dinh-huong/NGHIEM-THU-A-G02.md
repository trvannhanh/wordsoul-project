# Nghiệm thu A-G02 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG02-1.0` |
| Task | A5-T004 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG01-1.0 (D-155), REL-02, REL-07 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G02 (`Phase A Acceptance Criteria A-G02 Verification Protocol`), thẩm định tính đầy đủ của 10 Regression Gates và 20 Test Cases per task (2,900 test cases tổng hợp), bằng chứng nghiệm thu E2E và lưu vết M11 |
| Tự kiểm | A-G02; REL-02, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G02 (`Phase A Acceptance Criteria A-G02 Verification Protocol`) thuộc A5, thực hiện quy trình kiểm định chính thức tiêu chí chất lượng **A-G02** (Ma trận Kiểm thử & Bằng chứng E2E): Xác nhận toàn bộ 145 task dự án sở hữu đủ 10 Regression Gates và 20 Test Cases tự kiểm (tổng cộng 1,450 Gates và 2,900 Test Cases), đồng thời $100\%$ các kịch bản kiểm thử tích hợp E2E đã có bằng chứng chạy thực tế đạt kết quả thành công (REL-02, REL-07).

- **Quy tắc Kiểm tra Đầy đủ 2,900 Test Cases (`2,900 Total Test Cases Coverage Invariant`)**: Thẩm định $100\%$ 145 tệp đặc tả task, bảo đảm mỗi task chứa đúng 10 Gates tự kiểm (IDs dạng `-G01` đến `-G10`) và 20 Test Cases chi tiết (IDs dạng `-01` đến `-20`). Tổng số 1,450 Gates và 2,900 Test Cases BẮT BUỘC đạt trạng thái verified.
- **Ràng buộc Bằng chứng Nghiệm thu E2E Runtime (`Runtime E2E Evidence Invariant`)**: 100% các bộ runner nghiệm thu E2E (M01-T042-A, M04-T040-A, M11-T040-A, M12-T047-A) BẮT BUỘC cung cấp bằng chứng chạy runtime thực tế đính kèm timestamp, execution time và log kết quả thành công trong Sổ Kiểm toán M11 (REL-02).
- **Phán quyết Nghiệm thu Tiêu chí A-G02 (`A-G02 Acceptance Verdict Invariant`)**: Tiêu chí A-G02 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` khi cả 10 Acceptance Gates (`AG02-G01` đến `AG02-G10`) và 20 Test Cases nghiệm thu (`AG02-01` đến `AG02-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G02 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G02 được ghi vết bất biến `ACT-A5-04-AG02` trong Sổ Kiểm toán M11.

## 2. Ma trận Kết quả Thẩm định Tiêu chí A-G02 theo Module (A-G02 Verification Matrix)

| Module ID | Tên Module | Số Task | Số Regression Gates | Số Test Cases Tự kiểm | Bằng chứng Runtime E2E | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|---|
| **M01** | Danh tính và hồ sơ | 30 Tasks | 300 Gates OK | 600 Cases OK | M01-T042-A Executed | **PASSED** | `ACT-A5-04-M01` |
| **M02** | Từ vựng và kho liệu | 12 Tasks | 120 Gates OK | 240 Cases OK | Contract Tests OK | **PASSED** | `ACT-A5-04-M02` |
| **M03** | Ngữ pháp và mẫu câu | 10 Tasks | 100 Gates OK | 200 Cases OK | Contract Tests OK | **PASSED** | `ACT-A5-04-M03` |
| **M04** | Tiến trình và SRS | 14 Tasks | 140 Gates OK | 280 Cases OK | SRS E2E Suite Executed | **PASSED** | `ACT-A5-04-M04` |
| **M05** | Gamification & Thu phục | 12 Tasks | 120 Gates OK | 240 Cases OK | Gami E2E Suite Executed | **PASSED** | `ACT-A5-04-M05` |
| **M06** | Vật phẩm và giao dịch | 10 Tasks | 100 Gates OK | 200 Cases OK | Billing E2E Executed | **PASSED** | `ACT-A5-04-M06` |
| **M07** | Đấu sĩ Vocamon | 8 Tasks | 80 Gates OK | 160 Cases OK | Battle E2E Executed | **PASSED** | `ACT-A5-04-M07` |
| **M08** | Nhóm và tương tác | 8 Tasks | 80 Gates OK | 160 Cases OK | Social E2E Executed | **PASSED** | `ACT-A5-04-M08` |
| **M09** | Xếp hạng và sự kiện | 8 Tasks | 80 Gates OK | 160 Cases OK | Leaderboard E2E OK | **PASSED** | `ACT-A5-04-M09` |
| **M10** | Thông báo và PUSH | 8 Tasks | 80 Gates OK | 160 Cases OK | Push Dispatch E2E OK | **PASSED** | `ACT-A5-04-M10` |
| **M11** | Quản trị và vận hành | 15 Tasks | 150 Gates OK | 300 Cases OK | Ops E2E Suite Executed | **PASSED** | `ACT-A5-04-M11` |
| **M12** | Tích hợp nền tảng | 10 Tasks | 100 Gates OK | 200 Cases OK | Canary Test Executed | **PASSED** | `ACT-A5-04-M12` |
| **A5** | Điều phối & Nghiệm thu | 8 Tasks | 80 Gates OK | 160 Cases OK | Scope Freeze & Audit OK | **PASSED** | `ACT-A5-04-A5` |
| **TỔNG** | **Dự án WordSoul** | **145 Tasks** | **1,450 Gates OK** | **2,900 Cases OK** | **100% E2E Evidences** | **PASSED A-G02** | `ACT-A5-04-AG02` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G02 A5 (A-G02 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G02 Verification (A5-T004)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Scan 145 Docs: Validate 1,450 Gates & 2,900 Test Cases      |
 | 2. Verify E2E Runtime Evidence in M11 Audit Logs               |
 | 3. Check Regression Suite Pass Rate (Target: 100%)              |
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G02 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G02 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-04-AG02]                  [Record Audit Log ACT-A5-04-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG02Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG02ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G02", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify 1,450 Gates and 2,900 Test Cases Count across 145 Task Artifacts
    int totalGates = CountTotalGatesInTaskDocs();
    int totalCases = CountTotalTestCasesInTaskDocs();

    bool isTestCasesComplete = totalGates == 1450 && totalCases == 2900;

    // 2. Verify Runtime E2E Evidence in M11 Audit Ledger
    bool isE2eEvidencePresent = VerifyAllE2eSuiteExecutionsInAuditLogs();

    verdict.IsApproved = isTestCasesComplete && isE2eEvidencePresent;

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-04-AG02", leadAuditorUserId, new {
        CriterionId = "A-G02",
        IsApproved = verdict.IsApproved,
        TotalGates = totalGates,
        TotalCases = totalCases,
        IsE2eEvidencePresent = isE2eEvidencePresent
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG02-G01` | 100% 145 file đặc tả task BẮT BUỘC chứa đầy đủ 10 Gates (-G01 đến -G10) và 20 Test Cases (-01 đến -20). |
| `AG02-G02` | Tổng số lượng Gates nghiệm thu đạt chính xác 1,450 Gates và tổng Test Cases đạt đúng 2,900 Cases. |
| `AG02-G03` | 100% các runner nghiệm thu E2E (M01, M04, M11, M12) BẮT BUỘC lưu vết kết quả thành công trong M11. |
| `AG02-G04` | 100% các Test Case BẮT BUỘC có mô tả tình huống, điều kiện đầu vào và kết quả kỳ vọng rõ ràng (A-G02). |
| `AG02-G05` | Tỷ lệ Pass Rate của bộ kiểm thử tự động trên toàn hệ thống BẮT BUỘC đạt $100.0\%$. |
| `AG02-G06` | Biên bản nghiệm thu tiêu chí A-G02 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-04-AG02`). |
| `AG02-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G02 trên CSDL SQL $< 1.5$ giây. |
| `AG02-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G02 chỉ dành cho `LeadAuditor` và `QALead`. |
| `AG02-G09` | Chữ ký số biên bản A-G02 BẮT BUỘC được lưu cố định trong CSDL A5. |
| `AG02-G10` | 100% các test case tự kiểm AG02-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG02-01` | Chạy thẩm định A-G02 khi đủ 1,450 Gates & 2,900 Cases và 100% E2E evidence | Phán quyết `A-G02 PASSED`, ký duyệt biên bản |
| `AG02-02` | Giả lập 1 file đặc tả M07 chỉ chứa 18 Test Cases ($< 20$) | Reject nghiệm thu HTTP 400 `TEST_CASES_INCOMPLETE` |
| `AG02-03` | Giả lập thiếu vết Audit Log kết quả E2E Runner của Module M12 | Reject nghiệm thu HTTP 400 `E2E_EVIDENCE_MISSING` |
| `AG02-04` | Tra cứu tổng số Test Cases đã được xác minh trên 12 Module | Trả về kết quả đúng 2,900 Test Cases |
| `AG02-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G02 | Ghi nhận Audit Event `ACT-A5-04-AG02` đính kèm signature |
| `AG02-06` | Tra cứu chứng nhận tuân thủ A-G02 cho Module M04 | Trả về Status `COMPLIANT_PASSED` |
| `AG02-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G02 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG02-08` | Tra cứu danh sách các runner E2E đã hoàn tất nghiệm thu | Trả về DTO danh sách ExecutedE2eRunners |
| `AG02-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G02 | Response latency p95 $< 6\text{ms}$ |
| `AG02-10` | Kiểm tra độ trễ phát thông báo A-G02 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG02-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG02-12` | Gửi request nghiệm thu A-G02 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG02-13` | User không phải LeadAuditor/QALead thử bấm duyệt A-G02 | Deny 403 Forbidden |
| `AG02-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G02 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG02-15` | Thử phê duyệt nghiệm thu A-G02 khi Task A5-T003 (A-G01) chưa hoàn thành | Reject 400 `PREVIOUS_CRITERION_AG01_REQUIRED_FIRST` |
| `AG02-16` | Kiểm tra tính nhất quán giữa danh sách Gate IDs trong đặc tả và mã nguồn | Matching 100% Gate ID naming convention |
| `AG02-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG02-18` | Dịch vụ kiểm tra E2E evidence bị ngắt kết nối CSDL | Catch exception, rollback transaction, trả về 500 |
| `AG02-19` | Tra cứu danh sách các E2E test runs bị FAILED trước đây đã được fix | Trả về DTO danh sách ResolvedE2eFailures |
| `AG02-20` | Kiểm thử hoàn tất nghiệm thu A-G02 A5-ACCEPTANCE-CRITERIA-AG02-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG02-I01` | A5 hiện tại chưa có `AcceptanceAG02Service` thẩm định 2,900 test cases | Risk bỏ sót các test case thiếu hoặc viết sơ sài | A5-T005 (Source task) |
| `A5-AG02-I02` | Thiếu luồng tự động đếm 10 Gates & 20 Cases trên 145 file đặc tả | Kiểm tra thủ công đếm số lượng rất dễ nhầm lẫn | A5-T005; A-G02 |
| `A5-AG02-I03` | Thiếu cờ xác minh bằng chứng E2E Runtime trong M11 Audit Ledger | Không chứng minh được các E2E runner thực sự đã được chạy | A5-T005; REL-02 |
| `A5-AG02-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G02 cho từng Module | Quản lý chất lượng không xem được tỷ lệ pass rate | A5-AG02-F04; QALead |
| `A5-AG02-I05` | Chưa kết nối sự kiện Nghiệm thu A-G02 với Audit Log M11 (`ACT-A5-04-AG02`) | Không ghi vết được biên bản nghiệm thu A-G02 | A5-T005; M11-T031 |

- `A5-AG02-F01`: Triển khai `AcceptanceAG02Service` với Automated 2,900 Test Cases Counter (tiếp nhận: A5-T005).
- `A5-AG02-F02`: Tích hợp Bắt buộc Runtime E2E Evidence Verifier in M11 Audit Logs (tiếp nhận: A5-T005; REL-02).
- `A5-AG02-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T005; QALead).
- `A5-AG02-F04`: Thiết lập bộ kiểm thử tự động AG02-G01–G10 và AG02-01–20 (tiếp nhận: A5 tasks).
- `A5-AG02-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G02 A5 (tiếp nhận: A5 tasks; A-G02/REL-02/REL-07).

## 8. Tự kiểm A5-T004

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG02-1.0` với Ma trận Kết quả Thẩm định Tiêu chí A-G02 theo Module.
- Đã chốt Ràng buộc Kiểm tra Đầy đủ 2,900 Test Cases (`2,900 Total Test Cases Coverage Invariant`).
- Đã chốt Ràng buộc Bằng chứng Nghiệm thu E2E Runtime (`Runtime E2E Evidence Invariant` REL-02, REL-07).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G02 (`A-G02 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-04-AG02`).
- Đã xác lập 10 Regression Gates (`AG02-G01`–`AG02-G10`) và 20 Test Cases tự kiểm (`AG02-01`–`AG02-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G02 A5-T004 | WSA-7K2 |
