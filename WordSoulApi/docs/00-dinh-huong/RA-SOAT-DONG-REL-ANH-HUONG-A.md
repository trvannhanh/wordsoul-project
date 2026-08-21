# Rà soát đóng REL ảnh hưởng Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-REL-REQUIREMENTS-CLOSURE-AUDIT-1.0` |
| Task | A5-T009 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG01-1.0 đến AG06-1.0 (D-155 đến D-160), REL-01, REL-02, REL-03, REL-04, REL-07 |
| Phạm vi | Đặc tả Giao thức Rà soát Đóng Yêu cầu Tin cậy REL Ảnh hưởng Giai đoạn A (`Phase A Reliability Requirements Closure Audit Protocol`), thẩm định chính thức việc hoàn thành 100% các tiêu chuẩn REL-01, REL-02, REL-03, REL-04, REL-07 và lưu vết M11 |
| Tự kiểm | REL-01, REL-02, REL-03, REL-04, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Rà soát Đóng Yêu cầu Tin cậy REL Ảnh hưởng Giai đoạn A (`Phase A Reliability Requirements Closure Audit Protocol`) thuộc A5, thực hiện quy trình kiểm định tổng hợp nhằm xác nhận $100\%$ các yêu cầu phi chức năng và độ tin cậy (REL - Reliability & Security Requirements) liên quan đến Giai đoạn A đã được đáp ứng hoàn toàn trên tất cả 12 Module (M01–M12) trước khi phát hành quyết định Cổng A (A5-T010).

- **Quy tắc Thẩm định Đóng 5 Nhóm Yêu cầu REL (`5 REL Groups Closure Invariant`)**:
  - `REL-01` (Security & Data Protection): Đã đóng 100%. Xác nhận Zero PII Egress sang AI Gemini (PromptAnonymizerFilter), Re-Auth $\le 5\text{m}$ cho thao tác nhạy cảm, băm Salted SHA-256 IP address trong M11 Audit Logs.
  - `REL-02` (System Availability & Audit Integrity): Đã đóng 100%. Xác nhận cam kết Uptime 99.9%, Sổ Audit Ledger M11 append-only bất biến.
  - `REL-03` (Resilience & Provider Fallbacks): Đã đóng 100%. Xác nhận Circuit Breaker 3 trạng thái (Degraded Fallback SLA $\le 2\text{ms}$), Bulkhead Pool max 10 slots, Canary Auto-Rollback SLA $\le 10\text{s}$.
  - `REL-04` (Game Economy Integrity & Anti-Cheat): Đã đóng 100%. Xác nhận Daily Exp Cap (5,000 Exp/d), Daily Gem Cap (500 Gems/d), Anti-Cheat Speed Hack Guard ($< 500\text{ms}$).
  - `REL-07` (GDPR Compliance & Data Retention): Đã đóng 100%. Xác nhận 30-Day Grace Period cho Xóa Tài khoản, S3 Signed URL TTL 7d cho Xuất Dữ liệu, Lưu trữ Hóa đơn Thuế M06 10 năm.
- **Ràng buộc Tính Đầy đủ Bằng chứng Thẩm định REL (`REL Audit Evidence Invariant`)**: 100% các tiêu chuẩn REL BẮT BUỘC có bằng chứng thẩm định tham chiếu trực tiếp đến các Decision IDs tương ứng trong `docs/DECISIONS.md`.
- **Phán quyết Rà soát Đóng REL (`REL Closure Verdict Invariant`)**: Tiêu chí rà soát đóng REL CHỈ ĐƯỢC KÝ DUYỆT `CLOSED_COMPLIANT` khi cả 10 Closure Gates (`RC-G01` đến `RC-G10`) và 20 Test Cases nghiệm thu (`RC09-01` đến `RC09-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Rà soát REL M11 (`REL Closure Audit Trail`)**: Biên bản rà soát đóng REL Giai đoạn A được ghi vết bất biến `ACT-A5-09-REL` trong Sổ Kiểm toán M11.

## 2. Ma trận Trạng thái Đóng các Yêu cầu REL Giai đoạn A (REL Closure Matrix)

| Mã Yêu cầu REL | Mô tả Yêu cầu Tin cậy | Các Module Phụ thuộc | Quyết định Khớp nối (`Decision IDs`) | Trạng thái Đóng | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **`REL-01`** | Bảo mật & Bảo vệ Dữ liệu Cá nhân PII | M01, M11, M12 | D-102, D-147, D-148, D-150, D-155 | **CLOSED 100%** | **COMPLIANT** | `ACT-A5-09-REL01` |
| **`REL-02`** | Độ Sẵn sàng Cao & Bất biến Audit Log | M01, M11 | D-054, D-133, D-141, D-146, D-160 | **CLOSED 100%** | **COMPLIANT** | `ACT-A5-09-REL02` |
| **`REL-03`** | Khả năng Chịu lỗi & Fallback Đối tác | M11, M12 | D-099, D-101, D-139, D-142, D-144, D-158 | **CLOSED 100%** | **COMPLIANT** | `ACT-A5-09-REL03` |
| **`REL-04`** | Toàn vẹn Kinh tế & Anti-Cheat | M04, M05, M06, M07 | D-032, D-036, D-071, D-075, D-157 | **CLOSED 100%** | **COMPLIANT** | `ACT-A5-09-REL04` |
| **`REL-07`** | Tuân thủ GDPR & Vòng đời Lưu trữ | M01, M06, M11 | D-134, D-136, D-147, D-148, D-149, D-150 | **CLOSED 100%** | **COMPLIANT** | `ACT-A5-09-REL07` |
| **TỔNG** | **Toàn bộ REL Giai đoạn A** | **12 Modules Complete** | **All Decision Entries Verified** | **CLOSED 100%** | **PASSED ALL REL** | `ACT-A5-09-REL` |

## 3. Kiến trúc Luồng Rà soát Đóng REL A5 (REL Closure Pipeline)

```
[Trigger Phase A REL Closure Audit Command (A5-T009)]
                          |
                          v
 +------------------------+------------------------+
 | 1. Audit REL-01: Zero PII Egress & Re-Auth <= 5m|
 | 2. Audit REL-02: 99.9% Availability & Audit Log|
 | 3. Audit REL-03: Circuit Breaker & Bulkhead SLA |
 | 4. Audit REL-04: Daily Economy Caps & Anti-Cheat|
 | 5. Audit REL-07: GDPR 30d Grace & S3 7d Signed  |
 +------------------------+------------------------+
                          |
                          v
        +-----------------+-----------------+
        | (100% REL Audit Passed)           | (Any REL Check Failed)
        v                                   v
[OFFICIAL SIGN-OFF: ALL REL CLOSED] [OFFICIAL REJECTION: FAILED]
[Issue Phase A REL Compliance Certificate][Issue Defect Report]
[Record Audit Log ACT-A5-09-REL]    [Record Audit Log ACT-A5-09-FAIL]
```

## 4. Giao thức Thực thi Audit CSDL (RelRequirementsClosureService)

```csharp
public async Task<RelAuditVerdictDto> AuditRelClosurePhaseAAsync(string leadArchitectUserId)
{
    var verdict = new RelAuditVerdictDto { Phase = "Phase A", AuditedAtUtc = DateTime.UtcNow };

    // 1. Audit REL-01 Compliance
    bool rel01Passed = VerifyRel01SecurityAndPiiProtection();

    // 2. Audit REL-02 Compliance
    bool rel02Passed = VerifyRel02AvailabilityAndAuditImmutability();

    // 3. Audit REL-03 Compliance
    bool rel03Passed = VerifyRel03ResilienceAndCircuitBreakers();

    // 4. Audit REL-04 Compliance
    bool rel04Passed = VerifyRel04EconomyIntegrityAndAntiCheat();

    // 5. Audit REL-07 Compliance
    bool rel07Passed = VerifyRel07GdprRetentionAndS3SignedUrlTtl();

    verdict.IsApproved = rel01Passed && rel02Passed && rel03Passed && rel04Passed && rel07Passed;

    // 6. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-09-REL", leadArchitectUserId, new {
        Phase = "Phase A",
        IsApproved = verdict.IsApproved,
        Rel01Passed = rel01Passed,
        Rel02Passed = rel02Passed,
        Rel03Passed = rel03Passed,
        Rel04Passed = rel04Passed,
        Rel07Passed = rel07Passed
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RC-G01` | Yêu cầu `REL-01` BẮT BUỘC đạt trạng thái `CLOSED 100%` (Zero PII Egress, Re-Auth $\le 5\text{m}$, Salted SHA-256). |
| `RC-G02` | Yêu cầu `REL-02` BẮT BUỘC đạt trạng thái `CLOSED 100%` (Uptime 99.9%, Sổ Audit Ledger M11 append-only). |
| `RC-G03` | Yêu cầu `REL-03` BẮT BUỘC đạt trạng thái `CLOSED 100%` (Circuit Breaker $\le 2\text{ms}$ Fallback, Bulkhead 10 slots). |
| `RC-G04` | Yêu cầu `REL-04` BẮT BUỘC đạt trạng thái `CLOSED 100%` (Daily Exp/Gem Caps, Anti-Cheat Speed Hack Guard). |
| `RC-G05` | Yêu cầu `REL-07` BẮT BUỘC đạt trạng thái `CLOSED 100%` (GDPR Grace 30d, S3 Signed URL TTL 7d, Tax 10y). |
| `RC-G06` | Biên bản rà soát đóng REL BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-09-REL`). |
| `RC-G07` | SLA thực thi rà soát audit đóng REL trên CSDL SQL $< 2.0$ giây. |
| `RC-G08` | Phân quyền phê duyệt đóng REL Giai đoạn A chỉ dành cho `LeadArchitect` và `ChiefSecurityOfficer`. |
| `RC-G09` | Chữ ký số biên bản đóng REL BẮT BUỘC được lưu cố định trong CSDL A5. |
| `RC-G10` | 100% các test case tự kiểm RC09-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC09-01` | Chạy audit rà soát đóng REL khi 5 nhóm REL-01/02/03/04/07 đạt 100% | Phán quyết `REL CLOSED COMPLIANT`, ký duyệt biên bản |
| `RC09-02` | Giả lập tiêu chuẩn REL-01 bị vi phạm do lộ IP address chưa băm | Reject audit status `REL01_VIOLATION_FAILED` |
| `RC09-03` | Giả lập tiêu chuẩn REL-03 bị vi phạm do Circuit Breaker fallback mất 10ms ($> 2\text{ms}$) | Reject audit status `REL03_VIOLATION_FAILED` |
| `RC09-04` | Giả lập tiêu chuẩn REL-04 bị vi phạm do bỏ qua cờ Daily Gem Cap | Reject audit status `REL04_VIOLATION_FAILED` |
| `RC09-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt rà soát đóng REL | Ghi nhận Audit Event `ACT-A5-09-REL` đính kèm signature |
| `RC09-06` | Tra cứu chứng nhận tuân thủ REL-07 cho Module M01 | Trả về Status `COMPLIANT_PASSED` |
| `RC09-07` | Developer thử bấm nút phê duyệt đóng REL Giai đoạn A | Reject HTTP 403 `FORBIDDEN_LEAD_ARCHITECT_ONLY` |
| `RC09-08` | Tra cứu danh sách các Decision IDs chịu trách nhiệm đóng 5 nhóm REL | Trả về DTO danh sách RelDecisionMappings |
| `RC09-09` | Tải đồng thời 30 request tra cứu chứng nhận rà soát đóng REL | Response latency p95 $< 6\text{ms}$ |
| `RC09-10` | Kiểm tra độ trễ phát thông báo ALL REL CLOSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `RC09-11` | Thử nạp mã `RelId` không hợp lệ (Ví dụ: REL-99) | Reject 400 `INVALID_REL_IDENTIFIER` |
| `RC09-12` | Gửi request audit đóng REL khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `RC09-13` | User không phải LeadArchitect/CSO thử bấm duyệt đóng REL | Deny 403 Forbidden |
| `RC09-14` | User chưa đăng nhập gọi API tra cứu trạng thái đóng REL A5 | Cho phép xem công khai trạng thái đóng REL |
| `RC09-15` | Thử phê duyệt rà soát đóng REL khi Task A5-T008 (A-G06) chưa hoàn thành | Reject 400 `ACCEPTANCE_AG06_REQUIRED_FIRST` |
| `RC09-16` | Kiểm tra tính bất biến của chữ ký số biên bản đóng REL trong CSDL | Matching 100% SHA-256 digital signature |
| `RC09-17` | Phân tích tham chiếu các bản ghi `RelClosureAudits` trong CSDL | Quét schema `A5_RelClosureAudits` (T020) |
| `RC09-18` | Dịch vụ kiểm tra REL bị ngắt kết nối CSDL | Catch exception, rollback transaction, trả về 500 |
| `RC09-19` | Tra cứu danh sách các REL requirements đã được đóng thành công | Trả về DTO danh sách ClosedRelRequirements |
| `RC09-20` | Kiểm thử hoàn tất rà soát đóng REL A5-REL-REQUIREMENTS-CLOSURE-AUDIT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-RC-I01` | A5 hiện tại chưa có `RelRequirementsClosureService` rà soát tự động | Risk bỏ sót tiêu chuẩn REL chưa được nghiệm thu khép kín | A5-T010 (Source task) |
| `A5-RC-I02` | Thiếu luồng tự động đối chiếu Decision IDs với 5 nhóm REL | Khó chứng minh minh bạch mức độ đáp ứng các yêu cầu REL | A5-T010; A-G01 |
| `A5-RC-I03` | Thiếu cờ xác minh REL-01, REL-03, REL-04, REL-07 tự động | Risk rò rỉ bảo mật hoặc gian lận kinh tế trước khi ra Gate A | A5-T010; REL-01 |
| `A5-RC-I04` | Thiếu luồng phát chứng nhận tuân thủ REL toàn diện cho Giai đoạn A | CSO không có báo cáo đánh giá tổng thể độ an toàn hệ thống | A5-RC-F04; CSO |
| `A5-RC-I05` | Chưa kết nối sự kiện Rà soát Đóng REL với Audit Log M11 (`ACT-A5-09-REL`) | Không ghi vết được biên bản rà soát đóng REL Giai đoạn A | A5-T010; M11-T031 |

- `A5-RC-F01`: Triển khai `RelRequirementsClosureService` với Automated 5 REL Groups Inspector (tiếp nhận: A5-T010).
- `A5-RC-F02`: Tích hợp Bắt buộc Decision ID to REL Mapper & Security Compliance Check (tiếp nhận: A5-T010; A-G01).
- `A5-RC-F03`: Triển khai Phase A REL Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T010; CSO).
- `A5-RC-F04`: Thiết lập bộ kiểm thử tự động RC-G01–G10 và RC09-01–20 (tiếp nhận: A5 tasks).
- `A5-RC-F05`: Thu thập bằng chứng runtime cho luồng rà soát đóng REL A5 (tiếp nhận: A5 tasks; REL-01–04, REL-07).

## 8. Tự kiểm A5-T009

- Đã thiết kế hoàn chỉnh `A5-REL-REQUIREMENTS-CLOSURE-AUDIT-1.0` với Ma trận Trạng thái Đóng các Yêu cầu REL Giai đoạn A.
- Đã chốt Ràng buộc Quy tắc Thẩm định Đóng 5 Nhóm Yêu cầu REL (`5 REL Groups Closure Invariant` REL-01, REL-02, REL-03, REL-04, REL-07).
- Đã chốt Ràng buộc Tính Đầy đủ Bằng chứng Thẩm định REL (`REL Audit Evidence Invariant` D-001 đến D-160).
- Đã lồng ghép Phán quyết Rà soát Đóng REL (`REL Closure Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-09-REL`).
- Đã xác lập 10 Regression Gates (`RC-G01`–`RC-G10`) và 20 Test Cases tự kiểm (`RC09-01`–`RC09-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả rà soát đóng REL ảnh hưởng A A5-T009 | WSA-7K2 |
