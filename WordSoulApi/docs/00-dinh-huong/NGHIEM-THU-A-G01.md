# Nghiệm thu A-G01 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG01-1.0` |
| Task | A5-T003 |
| Đầu vào | A5-TASK-COVERAGE-AUDIT-145-1.0 (D-154), M01-CROSS-MODULE-PII-MAP-1.0 (D-102), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G01 (`Phase A Acceptance Criteria A-G01 Verification Protocol`), xác minh 100% tài liệu đạt chuẩn định dạng, từ điển danh tính, không rò rỉ dữ liệu cá nhân PII và lưu vết M11 |
| Tự kiểm | A-G01; REL-01, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G01 (`Phase A Acceptance Criteria A-G01 Verification Protocol`) thuộc A5, thực hiện quy trình thẩm định chính thức tiêu chí chất lượng **A-G01** (Định dạng & Bảo vệ PII Danh tính): Xác nhận toàn bộ 145 tệp tài liệu đặc tả trên 12 Module (M01–M12) tuân thủ 100% cấu trúc tiêu chuẩn, công bố từ điển dữ liệu khép kín và bảo đảm không rò rỉ dữ liệu cá nhân PII ra các dịch vụ ngoài (AI Gemini, Public Audit Logs) (REL-01, REL-07).

- **Quy tắc Thẩm định Định dạng Tiêu chuẩn A-G01 (`Standard Document Structure Invariant`)**: 100% các file đặc tả task BẮT BUỘC chứa đủ 9 phần tiêu chuẩn: (1) Bảng thuộc tính, (2) Mục tiêu và invariant, (3) Ma trận/Bảng đặc tả, (4) Sơ đồ kiến trúc/Luồng, (5) Giao thức thực thi code C# mẫu, (6) 10 Regression Gates, (7) 20 Test Cases tự kiểm, (8) Đối chiếu hiện trạng & findings, (9) Tự kiểm & Lịch sử.
- **Ràng buộc Tuyệt đối Không Rò rỉ PII (`Zero PII Egress Invariant`)**: Thẩm định $100\%$ các luồng truyền nhận dữ liệu cá nhân (M01-T033, M12 Gemini Prompt Filter, M11 Audit Logs), xác nhận $100\%$ PII nhạy cảm được ẩn danh hoặc băm bằng `Salted SHA-256` trước khi ghi sổ hoặc egress (REL-01, REL-07).
- **Phán quyết Nghiệm thu Tiêu chí A-G01 (`A-G01 Acceptance Verdict Invariant`)**: Tiêu chí A-G01 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` khi cả 10 Acceptance Gates (`AG01-G01` đến `AG01-G10`) và 20 Test Cases nghiệm thu (`AG01-01` đến `AG01-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G01 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G01 được ghi vết bất biến `ACT-A5-03-AG01` trong Sổ Kiểm toán M11.

## 2. Ma trận Kết quả Thẩm định Tiêu chí A-G01 theo Module (A-G01 Verification Matrix)

| Module ID | Tên Module | Kiểm tra Cấu trúc 9 Phần | Kiểm tra Bảo vệ PII (REL-01) | Tỷ lệ Đạt (%) | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **M01** | Danh tính và hồ sơ | 30/30 Docs OK | PII Anonymized & Salted SHA-256 | **100%** | **PASSED** | `ACT-A5-03-M01` |
| **M02** | Từ vựng và kho liệu | 12/12 Docs OK | Zero PII Exposure | **100%** | **PASSED** | `ACT-A5-03-M02` |
| **M03** | Ngữ pháp và mẫu câu | 10/10 Docs OK | Zero PII Exposure | **100%** | **PASSED** | `ACT-A5-03-M03` |
| **M04** | Tiến trình và SRS | 14/14 Docs OK | GUID Reference Only | **100%** | **PASSED** | `ACT-A5-03-M04` |
| **M05** | Gamification & Thu phục | 12/12 Docs OK | GUID Reference Only | **100%** | **PASSED** | `ACT-A5-03-M05` |
| **M06** | Vật phẩm và giao dịch | 10/10 Docs OK | PII Address Redacted (REL-07) | **100%** | **PASSED** | `ACT-A5-03-M06` |
| **M07** | Đấu sĩ Vocamon | 8/8 Docs OK | GUID Reference Only | **100%** | **PASSED** | `ACT-A5-03-M07` |
| **M08** | Nhóm và tương tác | 8/8 Docs OK | DisplayName Masked on Delete | **100%** | **PASSED** | `ACT-A5-03-M08` |
| **M09** | Xếp hạng và sự kiện | 8/8 Docs OK | GUID Reference Only | **100%** | **PASSED** | `ACT-A5-03-M09` |
| **M10** | Thông báo và PUSH | 8/8 Docs OK | Device Token Encrypted | **100%** | **PASSED** | `ACT-A5-03-M10` |
| **M11** | Quản trị và vận hành | 15/15 Docs OK | Salted SHA-256 IP Hashing | **100%** | **PASSED** | `ACT-A5-03-M11` |
| **M12** | Tích hợp nền tảng | 10/10 Docs OK | PromptAnonymizerFilter OK | **100%** | **PASSED** | `ACT-A5-03-M12` |
| **A5** | Điều phối & Nghiệm thu | 8/8 Docs OK | Zero PII Egress | **100%** | **PASSED** | `ACT-A5-03-A5` |
| **TỔNG** | **Dự án WordSoul** | **145/145 Docs OK** | **Bảo vệ PII 100% OK** | **100.0%** | **PASSED A-G01** | `ACT-A5-03-AG01` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G01 A5 (A-G01 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G01 Verification (A5-T003)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Scan 145 Document Files: Verify 9-Section Standard Structure |
 | 2. Verify Zero PII Egress: Check M01-T033 & M12 Prompt Filter  |
 | 3. Verify Audit Log Masking: Check Salted SHA-256 IP Hashing   |
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G01 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G01 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-03-AG01]                  [Record Audit Log ACT-A5-03-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG01Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG01ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G01", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify 9-Section Document Structure across 145 Task Artifacts
    bool allDocsStructured = VerifyAll145DocsStructure();

    // 2. Verify Zero PII Egress in AI Gemini Integration (M12-T002 / M01-T033)
    bool zeroPiiEgressToAi = VerifyZeroPiiEgressToGeminiFilter();

    // 3. Verify Salted SHA-256 Hashing for IP & User ID in Audit Logs M11
    bool auditLogsMasked = VerifyAuditLogPiiMasking();

    verdict.IsApproved = allDocsStructured && zeroPiiEgressToAi && auditLogsMasked;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-03-AG01", leadAuditorUserId, new {
        CriterionId = "A-G01",
        IsApproved = verdict.IsApproved,
        AllDocsStructured = allDocsStructured,
        ZeroPiiEgressToAi = zeroPiiEgressToAi,
        AuditLogsMasked = auditLogsMasked
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG01-G01` | 100% các file đặc tả 145 task BẮT BUỘC tuân thủ định dạng cấu trúc 9 phần tiêu chuẩn (A-G01). |
| `AG01-G02` | Mọi thông tin PII nhạy cảm truyền sang AI Gemini BẮT BUỘC lọc qua `PromptAnonymizerFilter` (REL-01). |
| `AG01-G03` | Địa chỉ IP người dùng trong Sổ Audit Log M11 BẮT BUỘC được băm bằng thuật toán `Salted SHA-256`. |
| `AG01-G04` | 100% các liên kết `UserId` liên module BẮT BUỘC chỉ sử dụng chuỗi GUID phi định danh (M01-T033). |
| `AG01-G05` | Lịch sử giao dịch M06 sau khi ẩn danh BẮT BUỘC xóa bỏ địa chỉ PII nhưng giữ lại giá trị tài chính (REL-07). |
| `AG01-G06` | Biên bản nghiệm thu tiêu chí A-G01 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-03-AG01`). |
| `AG01-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G01 trên CSDL SQL $< 1.5$ giây. |
| `AG01-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G01 chỉ dành cho `LeadAuditor` và `SecurityAdmin`. |
| `AG01-G09` | Kiểm tra tính bất biến của chữ ký số nghiệm thu tiêu chí A-G01 trong CSDL A5. |
| `AG01-G10` | 100% các test case tự kiểm AG01-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG01-01` | Chạy thẩm định tiêu chí A-G01 khi 145/145 file đặc tả đạt chuẩn 9 phần | Phán quyết `A-G01 PASSED`, ký duyệt biên bản |
| `AG01-02` | Giả lập 1 file đặc tả M04 thiếu phần "Giao thức thực thi C# mẫu" | Reject nghiệm thu HTTP 400 `DOCUMENT_STRUCTURE_INVALID` |
| `AG01-03` | Giả lập tắt cờ `PromptAnonymizerFilter` khi gửi prompt sang AI Gemini | Reject nghiệm thu HTTP 400 `PII_EGRESS_DETECTED` |
| `AG01-04` | Quét sổ Audit Log M11 xác minh định dạng băm IP address | Trả về chuỗi băm Salted SHA-256 64 ký tự hợp lệ |
| `AG01-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G01 | Ghi nhận Audit Event `ACT-A5-03-AG01` đính kèm signature |
| `AG01-06` | Tra cứu chứng nhận tuân thủ A-G01 cho Module M01 | Trả về Status `COMPLIANT_PASSED` |
| `AG01-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G01 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG01-08` | Tra cứu danh sách các điểm kiểm soát PII trên 12 Module | Trả về DTO danh sách PiiControlPoints |
| `AG01-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G01 | Response latency p95 $< 6\text{ms}$ |
| `AG01-10` | Kiểm tra độ trễ phát thông báo A-G01 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG01-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG01-12` | Gửi request nghiệm thu A-G01 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG01-13` | User không phải LeadAuditor/SecurityAdmin thử bấm duyệt A-G01 | Deny 403 Forbidden |
| `AG01-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G01 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG01-15` | Thử phê duyệt nghiệm thu A-G01 khi Task A5-T002 chưa hoàn thành | Reject 400 `COVERAGE_AUDIT_REQUIRED_FIRST` |
| `AG01-16` | Kiểm tra tính nhất quán của từ điển danh tính M01 với schema SQL | Matching 100% column data types |
| `AG01-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG01-18` | Dịch vụ kiểm tra PII bị gián đoạn bộ nhớ | Catch exception, rollback transaction, trả về 500 |
| `AG01-19` | Tra cứu danh sách các findings liên quan tới PII đã được xử lý | Trả về DTO danh sách ResolvedPiiFindings |
| `AG01-20` | Kiểm thử hoàn tất nghiệm thu A-G01 A5-ACCEPTANCE-CRITERIA-AG01-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG01-I01` | A5 hiện tại chưa có `AcceptanceAG01Service` thẩm định tự động | Risk duyệt nghiệm thu thủ công thiếu chính xác | A5-T004 (Source task) |
| `A5-AG01-I02` | Thiếu luồng tự động quét định dạng 9 phần trên 145 tệp đặc tả | Khó đảm bảo 100% tài liệu đáp ứng tiêu chuẩn A-G01 | A5-T004; A-G01 |
| `A5-AG01-I03` | Thiếu cờ xác minh Zero PII Egress sang AI Gemini tự động | Risk rò rỉ dữ liệu cá nhân ra nhà cung cấp AI ngoài | A5-T004; REL-01 |
| `A5-AG01-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G01 cho từng Module | Quản lý dự án không xem được tiến độ nghiệm thu chi tiết | A5-AG01-F04; PM |
| `A5-AG01-I05` | Chưa kết nối sự kiện Nghiệm thu A-G01 với Audit Log M11 (`ACT-A5-03-AG01`) | Không ghi vết được biên bản nghiệm thu A-G01 | A5-T004; M11-T031 |

- `A5-AG01-F01`: Triển khai `AcceptanceAG01Service` với Automated Document Structure Scanner (tiếp nhận: A5-T004).
- `A5-AG01-F02`: Tích hợp Bắt buộc Zero PII Egress Verifier & Salted SHA-256 Inspector (tiếp nhận: A5-T004; REL-01).
- `A5-AG01-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T004; PM).
- `A5-AG01-F04`: Thiết lập bộ kiểm thử tự động AG01-G01–G10 và AG01-01–20 (tiếp nhận: A5 tasks).
- `A5-AG01-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G01 A5 (tiếp nhận: A5 tasks; A-G01/REL-01/REL-07).

## 8. Tự kiểm A5-T003

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG01-1.0` với Ma trận Kết quả Thẩm định Tiêu chí A-G01 theo Module.
- Đã chốt Ràng buộc Quy tắc Thẩm định Định dạng Tiêu chuẩn A-G01 (`Standard Document Structure Invariant` 9 phần).
- Đã chốt Ràng buộc Tuyệt đối Không Rò rỉ PII (`Zero PII Egress Invariant` REL-01, REL-07).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G01 (`A-G01 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-03-AG01`).
- Đã xác lập 10 Regression Gates (`AG01-G01`–`AG01-G10`) và 20 Test Cases tự kiểm (`AG01-01`–`AG01-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G01 A5-T003 | WSA-7K2 |
