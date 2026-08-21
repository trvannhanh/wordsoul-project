# Nghiệm thu A-G04 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG04-1.0` |
| Task | A5-T006 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG03-1.0 (D-157), M12 Circuit Breaker & Bulkhead (D-101), REL-03 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G04 (`Phase A Acceptance Criteria A-G04 Verification Protocol`), thẩm định khả năng chịu lỗi, Circuit Breaker 3 trạng thái, Bulkhead Isolation, Degraded Fallback SLA $\le 2\text{ms}$ và lưu vết M11 |
| Tự kiểm | A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G04 (`Phase A Acceptance Criteria A-G04 Verification Protocol`) thuộc A5, thực hiện quy trình kiểm định chính thức tiêu chí chất lượng **A-G04** (Khả năng Chịu lỗi & Resilience): Xác nhận toàn bộ 12 Module (đặc biệt là M12 Tích hợp Nền tảng) tuân thủ nghiêm ngặt mô hình ngắt mạch Circuit Breaker 3 trạng thái, phân lập luồng Bulkhead Isolation Pool và khả năng chuyển đổi Degraded Fallback SLA $\le 2\text{ms}$ khi đối tác thứ 3 ngắt kết nối (REL-03).

- **Quy tắc Thẩm định Circuit Breaker 3 Trạng thái (`3-State Circuit Breaker Invariant`)**: Thẩm định $100\%$ các provider tích hợp ngoài (Google OAuth, Gemini AI, S3, Firebase Push), bảo đảm khi tỷ lệ lỗi vượt $50\%$ trong cửa sổ 30s, Circuit Breaker TỰ ĐỘNG chuyển trạng thái `OPEN` trong 30 giây (SLA $\le 2\text{ms}$), từ chối gọi provider ngoài và trả về kết quả Fallback degraded cho người dùng (D-101, REL-03).
- **Ràng buộc Phân lập Luồng Bulkhead Isolator (`Bulkhead Pool Isolation Invariant`)**: Thẩm định cơ chế phân lập hàng chờ per-provider (Max 10 slots per queue pool). Khi provider A bị tắc nghẽn (vượt 10 slots), hệ thống trả về HTTP 429 `BULKHEAD_QUEUE_OVERFLOW` độc lập mà KHÔNG ảnh hưởng đến khả năng xử lý của các provider B, C, D khác.
- **Phán quyết Nghiệm thu Tiêu chí A-G04 (`A-G04 Acceptance Verdict Invariant`)**: Tiêu chí A-G04 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` khi cả 10 Acceptance Gates (`AG04-G01` đến `AG04-G10`) và 20 Test Cases nghiệm thu (`AG04-01` đến `AG04-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G04 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G04 được ghi vết bất biến `ACT-A5-06-AG04` trong Sổ Kiểm toán M11.

## 2. Ma trận Kết quả Thẩm định Tiêu chí A-G04 theo Provider (A-G04 Resilience Matrix)

| Provider Tích hợp | Tỷ lệ Lỗi Trip Threshold | Thời gian Break (`OPEN`) | Bulkhead Pool Slots | Fallback Action SLA | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **Google OAuth** | Tỷ lệ lỗi $> 50\%$ / 30s | 30 Giờ Break Window | Max 10 Slots | Static Auth Error SLA $< 1\text{ms}$ | **PASSED** | `ACT-A5-06-OAUTH` |
| **Gemini AI** | Tỷ lệ lỗi $> 50\%$ / 30s | 30 Giờ Break Window | Max 10 Slots | Fallback Static Cards SLA $< 2\text{ms}$ | **PASSED** | `ACT-A5-06-GEMINI` |
| **S3 Direct Upload** | Tỷ lệ lỗi $> 50\%$ / 30s | 30 Giờ Break Window | Max 10 Slots | Local Spool Fallback SLA $< 1\text{ms}$ | **PASSED** | `ACT-A5-06-S3` |
| **Firebase Push** | Tỷ lệ lỗi $> 50\%$ / 30s | 30 Giờ Break Window | Max 10 Slots | Queue Retry Spool SLA $< 1\text{ms}$ | **PASSED** | `ACT-A5-06-PUSH` |
| **TỔNG** | **100% Active Providers** | **3-State Breaker Active** | **Bulkhead Isolated** | **Degraded SLA $\le 2\text{ms}$** | **PASSED A-G04** | `ACT-A5-06-AG04` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G04 A5 (A-G04 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G04 Verification (A5-T006)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Verify M12 Circuit Breaker 3 States (CLOSED, OPEN, HALF-OPEN)|
 | 2. Verify Provider Bulkhead Pool Isolation (Max 10 Slots Queue) |
 | 3. Verify Degraded Fallback Response SLA <= 2ms                 |
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G04 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G04 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-06-AG04]                  [Record Audit Log ACT-A5-06-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG04Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG04ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G04", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify M12 Circuit Breaker 3-State Tripping SLA <= 2ms
    bool isCircuitBreakerCompliant = VerifyM12CircuitBreakerTripping();

    // 2. Verify Bulkhead Pool Queue Overflow (Max 10 Slots HTTP 429)
    bool isBulkheadCompliant = VerifyM12BulkheadQueueIsolation();

    // 3. Verify Fallback Action Response SLA <= 2ms
    bool isFallbackSlaCompliant = VerifyM12FallbackResponseSla();

    verdict.IsApproved = isCircuitBreakerCompliant && isBulkheadCompliant && isFallbackSlaCompliant;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-06-AG04", leadAuditorUserId, new {
        CriterionId = "A-G04",
        IsApproved = verdict.IsApproved,
        IsCircuitBreakerCompliant = isCircuitBreakerCompliant,
        IsBulkheadCompliant = isBulkheadCompliant,
        IsFallbackSlaCompliant = isFallbackSlaCompliant
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG04-G01` | Mô hình Circuit Breaker BẮT BUỘC hỗ trợ đủ 3 trạng thái: `CLOSED`, `OPEN` (30s), `HALF-OPEN` (3 trials). |
| `AG04-G02` | Tỷ lệ lỗi ngắt mạch Circuit Breaker BẮT BUỘC đạt mốc $> 50\%$ trong cửa sổ trượt 30 giây (REL-03). |
| `AG04-G03` | Hành động Fallback degraded BẮT BUỘC phản hồi tức thì với SLA $\le 2\text{ms}$ khi ngắt mạch. |
| `AG04-G04` | Bulkhead Queue Pool BẮT BUỘC phân lập per-provider với sức chứa tối đa 10 request slots. |
| `AG04-G05` | Khi Bulkhead Queue tràn slots ($> 10$), hệ thống BẮT BUỘC phản hồi HTTP 429 `BULKHEAD_QUEUE_OVERFLOW`. |
| `AG04-G06` | Biên bản nghiệm thu tiêu chí A-G04 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-06-AG04`). |
| `AG04-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G04 trên CSDL SQL $< 1.5$ giây. |
| `AG04-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G04 chỉ dành cho `LeadAuditor` và `SiteReliabilityLead`. |
| `AG04-G09` | Chữ ký số biên bản A-G04 BẮT BUỘC được lưu cố định trong CSDL A5. |
| `AG04-G10` | 100% các test case tự kiểm AG04-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG04-01` | Chạy thẩm định A-G04 khi 100% Circuit Breaker & Bulkhead hoạt động đạt chuẩn | Phán quyết `A-G04 PASSED`, ký duyệt biên bản |
| `AG04-02` | Giả lập Provider Gemini AI bị lỗi 60% ($> 50\%$) trong 30s | Circuit Breaker chuyển `OPEN` SLA $< 2\text{ms}$, trả về static fallback |
| `AG04-03` | Giả lập Bulkhead Pool Gemini AI bị dồn 12 requests ($> 10$ slots) | Reject request thứ 11 & 12 với HTTP 429 `BULKHEAD_QUEUE_OVERFLOW` |
| `AG04-04` | Giả lập Provider S3 sập nhưng Google OAuth vẫn hoạt động bình thường | S3 Fallback đĩa local M11, OAuth đăng nhập bình thường (Bulkhead Isolation) |
| `AG04-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G04 | Ghi nhận Audit Event `ACT-A5-06-AG04` đính kèm signature |
| `AG04-06` | Tra cứu chứng nhận tuân thủ A-G04 cho Module M12 | Trả về Status `COMPLIANT_PASSED` |
| `AG04-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G04 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG04-08` | Tra cứu danh sách các Provider đang trong trạng thái Circuit Breaker `OPEN` | Trả về DTO danh sách ActiveOpenBreakers |
| `AG04-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G04 | Response latency p95 $< 6\text{ms}$ |
| `AG04-10` | Kiểm tra độ trễ phát thông báo A-G04 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG04-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG04-12` | Gửi request nghiệm thu A-G04 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG04-13` | User không phải LeadAuditor/SiteReliabilityLead thử bấm duyệt A-G04 | Deny 403 Forbidden |
| `AG04-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G04 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG04-15` | Thử phê duyệt nghiệm thu A-G04 khi Task A5-T005 (A-G03) chưa hoàn thành | Reject 400 `PREVIOUS_CRITERION_AG03_REQUIRED_FIRST` |
| `AG04-16` | Kiểm tra tính nhất quán giữa cấu hình Circuit Breaker và Redis Sentinel | Matching 100% failover parameters |
| `AG04-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG04-18` | Dịch vụ kiểm tra Circuit Breaker bị ngắt kết nối Redis | Catch exception, rollback transaction, trả về 500 |
| `AG04-19` | Tra cứu danh sách các đợt Circuit Breaker tự khôi phục về `CLOSED` | Trả về DTO danh sách RecoveredBreakers |
| `AG04-20` | Kiểm thử hoàn tất nghiệm thu A-G04 A5-ACCEPTANCE-CRITERIA-AG04-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG04-I01` | A5 hiện tại chưa có `AcceptanceAG04Service` thẩm định khả năng chịu lỗi | Risk duyệt nghiệm thu thủ công thiếu công cụ đo SLA Fallback | A5-T007 (Source task) |
| `A5-AG04-I02` | Thiếu luồng tự động giả lập sập Provider để kiểm tra 3-State Breaker | Khó đảm bảo Circuit Breaker hoạt động chính xác khi có sự cố | A5-T007; REL-03 |
| `A5-AG04-I03` | Thiếu cờ xác minh Bulkhead Queue Isolation max 10 slots per pool | Risk 1 provider ngắt kết nối làm nghẽn toàn bộ API Gateway | A5-T007; REL-03 |
| `A5-AG04-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G04 cho từng Module | SRE Lead không xem được chỉ số chịu lỗi toàn hệ thống | A5-AG04-F04; SRELead |
| `A5-AG04-I05` | Chưa kết nối sự kiện Nghiệm thu A-G04 với Audit Log M11 (`ACT-A5-06-AG04`) | Không ghi vết được biên bản nghiệm thu A-G04 | A5-T007; M11-T031 |

- `A5-AG04-F01`: Triển khai `AcceptanceAG04Service` với Automated Resilience Tester (tiếp nhận: A5-T007).
- `A5-AG04-F02`: Tích hợp Bắt buộc 3-State Circuit Breaker & Bulkhead Pool Verifier (tiếp nhận: A5-T007; REL-03).
- `A5-AG04-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T007; SRELead).
- `A5-AG04-F04`: Thiết lập bộ kiểm thử tự động AG04-G01–G10 và AG04-01–20 (tiếp nhận: A5 tasks).
- `A5-AG04-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G04 A5 (tiếp nhận: A5 tasks; A-G04/REL-03).

## 8. Tự kiểm A5-T006

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG04-1.0` với Ma trận Kết quả Thẩm định Tiêu chí A-G04 theo Provider.
- Đã chốt Ràng buộc Quy tắc Thẩm định Circuit Breaker 3 Trạng thái (`3-State Circuit Breaker Invariant` D-101).
- Đã chốt Ràng buộc Phân lập Luồng Bulkhead Isolator (`Bulkhead Pool Isolation Invariant` Max 10 slots HTTP 429).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G04 (`A-G04 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-06-AG04`).
- Đã xác lập 10 Regression Gates (`AG04-G01`–`AG04-G10`) và 20 Test Cases tự kiểm (`AG04-01`–`AG04-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G04 A5-T006 | WSA-7K2 |
