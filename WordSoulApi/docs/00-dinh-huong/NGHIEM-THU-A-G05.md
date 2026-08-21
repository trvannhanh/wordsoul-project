# Nghiệm thu A-G05 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG05-1.0` |
| Task | A5-T007 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG04-1.0 (D-158), RFC 7807 Problem Details Standard, REL-01, REL-03, REL-04 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G05 (`Phase A Acceptance Criteria A-G05 Verification Protocol`), thẩm định 100% API phản hồi lỗi chuẩn RFC 7807 (`application/problem+json`), mã lỗi máy tính scannable, không rò rỉ stack trace PII và lưu vết M11 |
| Tự kiểm | A-G05; REL-01, REL-03, REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G05 (`Phase A Acceptance Criteria A-G05 Verification Protocol`) thuộc A5, thực hiện quy trình kiểm định chính thức tiêu chí chất lượng **A-G05** (Chuẩn hóa Phản hồi Lỗi RFC 7807): Xác nhận $100\%$ các phản hồi lỗi từ API Gateway và tất cả 12 Module (M01–M12) tuân thủ định dạng chuẩn quốc tế RFC 7807 (`ProblemDetails`), bao gồm đầy đủ `type`, `title`, `status`, `detail`, `instance`, `errorCode` và `traceId`, bảo đảm không rò rỉ chi tiết stack trace nội bộ (REL-01, REL-03, REL-04).

- **Quy tắc Chuẩn hóa RFC 7807 Phản hồi Lỗi (`RFC 7807 Problem Details Invariant`)**: 100% phản hồi lỗi (status code 4xx và 5xx) BẮT BUỘC có Content-Type Header `application/problem+json` và cấu trúc JSON chuẩn:
  ```json
  {
    "type": "https://wordsoul.app/errors/EXPIRED_TOKEN",
    "title": "Token expired",
    "status": 401,
    "detail": "Phiên truy cập đã hết hạn. Vui lòng làm mới token.",
    "instance": "/api/v1/auth/refresh",
    "errorCode": "ERR_AUTH_EXPIRED_TOKEN",
    "traceId": "0HMV891K20A9:00000001"
  }
  ```
- **Ràng buộc Tuyệt đối Không Rò rỉ Stack Trace Nội bộ (`Zero Stack Trace Leakage Invariant`)**: Thẩm định $100\%$ các tình huống lỗi hệ thống HTTP 500/502/503. TUYỆT ĐỐI CẤM rò rỉ mã nguồn, SQL stack trace, đường dẫn đĩa local hoặc biến môi trường PII trong trường `detail` trên môi trường Production (REL-01, REL-03).
- **Phán quyết Nghiệm thu Tiêu chí A-G05 (`A-G05 Acceptance Verdict Invariant`)**: Tiêu chí A-G05 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` when cả 10 Acceptance Gates (`AG05-G01` đến `AG05-G10`) và 20 Test Cases nghiệm thu (`AG05-01` đến `AG05-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G05 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G05 được ghi vết bất biến `ACT-A5-07-AG05` trong Sổ Kiểm toán M11.

## 2. Ma trận Phản hồi Lỗi RFC 7807 theo Nhóm Lỗi (RFC 7807 Error Matrix)

| Nhóm Lỗi (`ErrorDomain`) | HTTP Status Code | Chuẩn Header | Định dạng JSON Response | Ẩn Stack Trace | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **Xác thực & Phiên (M01)** | HTTP 401 / 403 | `application/problem+json` | RFC 7807 + `errorCode` | **100% Masked** | **PASSED** | `ACT-A5-07-AUTH` |
| **Dữ liệu Không Hợp lệ** | HTTP 400 Bad Req | `application/problem+json` | RFC 7807 + `invalidParams` | **100% Masked** | **PASSED** | `ACT-A5-07-VALIDATION` |
| **Vượt Hạn ngạch (M05/M12)** | HTTP 429 Limit | `application/problem+json` | RFC 7807 + `retryAfter` | **100% Masked** | **PASSED** | `ACT-A5-07-RATELIMIT` |
| **Không Tìm thấy Thực thể** | HTTP 404 Not Found | `application/problem+json` | RFC 7807 + `resourceId` | **100% Masked** | **PASSED** | `ACT-A5-07-NOTFOUND` |
| **Lỗi Hệ thống Nội bộ** | HTTP 500 / 503 | `application/problem+json` | RFC 7807 + `traceId` | **100% Masked (Zero Leak)** | **PASSED** | `ACT-A5-07-SYS500` |
| **TỔNG** | **100% API Errors** | **RFC 7807 Compliant** | **Machine Readable Codes** | **Zero Stack Trace Leak** | **PASSED A-G05** | `ACT-A5-07-AG05` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G05 A5 (A-G05 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G05 Verification (A5-T007)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Scan 100% API Error Responses: Verify RFC 7807 Standard Schema|
 | 2. Verify Content-Type Header: application/problem+json         |
 | 3. Verify Zero Stack Trace Leakage in Production 500 Errors     |
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G05 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G05 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-07-AG05]                  [Record Audit Log ACT-A5-07-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG05Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG05ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G05", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify 100% Error Responses conform to RFC 7807 ProblemDetails Schema
    bool isRfc7807Compliant = VerifyAllErrorResponsesRfc7807Schema();

    // 2. Verify Zero Stack Trace Leakage in Production (HTTP 500 responses)
    bool isZeroStackLeak = VerifyZeroStackTraceInProdErrorResponses();

    // 3. Verify Content-Type Header 'application/problem+json'
    bool isContentTypeCompliant = VerifyErrorResponseContentTypeHeader();

    verdict.IsApproved = isRfc7807Compliant && isZeroStackLeak && isContentTypeCompliant;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-07-AG05", leadAuditorUserId, new {
        CriterionId = "A-G05",
        IsApproved = verdict.IsApproved,
        IsRfc7807Compliant = isRfc7807Compliant,
        IsZeroStackLeak = isZeroStackLeak,
        IsContentTypeCompliant = isContentTypeCompliant
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG05-G01` | 100% phản hồi lỗi (4xx và 5xx) BẮT BUỘC sử dụng Content-Type `application/problem+json`. |
| `AG05-G02` | Cấu trúc JSON lỗi BẮT BUỘC tuân thủ mẫu RFC 7807 (`type`, `title`, `status`, `detail`, `instance`). |
| `AG05-G03` | Phản hồi lỗi BẮT BUỘC bổ sung trường `errorCode` duy nhất dạng chuỗi (Ví dụ: `ERR_AUTH_EXPIRED_TOKEN`). |
| `AG05-G04` | Phản hồi lỗi BẮT BUỘC kèm mã `traceId` ngẫu nhiên để hỗ trợ tra cứu log M11 (REL-03). |
| `AG05-G05` | Phản hồi lỗi HTTP 500 trên Production TUYỆT ĐỐI CẤM rò rỉ mã nguồn hoặc CSDL stack trace (REL-01). |
| `AG05-G06` | Biên bản nghiệm thu tiêu chí A-G05 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-07-AG05`). |
| `AG05-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G05 trên CSDL SQL $< 1.5$ giây. |
| `AG05-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G05 chỉ dành cho `LeadAuditor` và `ApiArchitect`. |
| `AG05-G09` | Chữ ký số biên bản A-G05 BẮT BUỘC được lưu cố định trong CSDL A5. |
| `AG05-G10` | 100% các test case tự kiểm AG05-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG05-01` | Chạy thẩm định A-G05 khi 100% API error responses đạt chuẩn RFC 7807 | Phán quyết `A-G05 PASSED`, ký duyệt biên bản |
| `AG05-02` | Giả lập 1 API M06 trả về lỗi HTTP 400 dạng chuỗi plain text `Error occurred` | Reject nghiệm thu HTTP 400 `NON_RFC7807_ERROR_DETECTED` |
| `AG05-03` | Giả lập API HTTP 500 rò rỉ CSDL Exception trace `NpgsqlException at line 45` | Reject nghiệm thu HTTP 400 `STACK_TRACE_LEAKAGE_DETECTED` |
| `AG05-04` | Kiểm tra Content-Type Header của phản hồi lỗi 401 Unauthorized | Trả về chính xác `application/problem+json` |
| `AG05-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G05 | Ghi nhận Audit Event `ACT-A5-07-AG05` đính kèm signature |
| `AG05-06` | Tra cứu chứng nhận tuân thủ A-G05 cho Module M01 | Trả về Status `COMPLIANT_PASSED` |
| `AG05-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G05 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG05-08` | Tra cứu từ điển mã lỗi `errorCode` toàn hệ thống | Trả về DTO danh sách StandardErrorCodes |
| `AG05-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G05 | Response latency p95 $< 6\text{ms}$ |
| `AG05-10` | Kiểm tra độ trễ phát thông báo A-G05 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG05-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG05-12` | Gửi request nghiệm thu A-G05 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG05-13` | User không phải LeadAuditor/ApiArchitect thử bấm duyệt A-G05 | Deny 403 Forbidden |
| `AG05-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G05 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG05-15` | Thử phê duyệt nghiệm thu A-G05 khi Task A5-T006 (A-G04) chưa hoàn thành | Reject 400 `PREVIOUS_CRITERION_AG04_REQUIRED_FIRST` |
| `AG05-16` | Kiểm tra tính duy nhất của tất cả mã `errorCode` trên 12 Module | Zero duplicate error codes |
| `AG05-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG05-18` | Dịch vụ kiểm tra RFC 7807 bị ngắt kết nối CSDL | Catch exception, rollback transaction, trả về 500 |
| `AG05-19` | Tra cứu danh sách các mã lỗi RFC 7807 được phát ra nhiều nhất trong ngày | Trả về DTO danh sách TopErrorMetrics |
| `AG05-20` | Kiểm thử hoàn tất nghiệm thu A-G05 A5-ACCEPTANCE-CRITERIA-AG05-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG05-I01` | A5 hiện tại chưa có `AcceptanceAG05Service` thẩm định RFC 7807 | Risk các API phát định dạng lỗi không đồng nhất | A5-T008 (Source task) |
| `A5-AG05-I02` | Thiếu luồng tự động quét `application/problem+json` header | Khó kiểm soát định dạng header lỗi trên 100% endpoints | A5-T008; A-G05 |
| `A5-AG05-I03` | Thiếu cờ xác minh Zero Stack Trace Leakage trên Production HTTP 500 | Risk lộ mã nguồn và đường dẫn server khi phát sinh lỗi | A5-T008; REL-01 |
| `A5-AG05-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G05 cho từng Module | API Architect không xem được danh mục mã lỗi chuẩn | A5-AG05-F04; ApiArch |
| `A5-AG05-I05` | Chưa kết nối sự kiện Nghiệm thu A-G05 với Audit Log M11 (`ACT-A5-07-AG05`) | Không ghi vết được biên bản nghiệm thu A-G05 | A5-T008; M11-T031 |

- `A5-AG05-F01`: Triển khai `AcceptanceAG05Service` với Automated RFC 7807 Schema Inspector (tiếp nhận: A5-T008).
- `A5-AG05-F02`: Tích hợp Bắt buộc Zero Stack Trace Leakage Verifier & ProblemDetails Header Check (tiếp nhận: A5-T008; REL-01).
- `A5-AG05-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T008; ApiArch).
- `A5-AG05-F04`: Thiết lập bộ kiểm thử tự động AG05-G01–G10 và AG05-01–20 (tiếp nhận: A5 tasks).
- `A5-AG05-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G05 A5 (tiếp nhận: A5 tasks; A-G05/REL-01/REL-03).

## 8. Tự kiểm A5-T007

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG05-1.0` với Ma trận Phản hồi Lỗi RFC 7807 theo Nhóm Lỗi.
- Đã chốt Ràng buộc Chuẩn hóa RFC 7807 Phản hồi Lỗi (`RFC 7807 Problem Details Invariant` `application/problem+json`).
- Đã chốt Ràng buộc Tuyệt đối Không Rò rỉ Stack Trace Nội bộ (`Zero Stack Trace Leakage Invariant` REL-01, REL-03).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G05 (`A-G05 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-07-AG05`).
- Đã xác lập 10 Regression Gates (`AG05-G01`–`AG05-G10`) và 20 Test Cases tự kiểm (`AG05-01`–`AG05-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G05 A5-T007 | WSA-7K2 |
