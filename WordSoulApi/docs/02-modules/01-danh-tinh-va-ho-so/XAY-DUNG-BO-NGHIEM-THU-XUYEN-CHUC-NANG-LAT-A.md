# Xây dựng bộ nghiệm thu xuyên chức năng M01 — lát A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-CROSS-FUNCTIONAL-ACCEPTANCE-A-1.0` |
| Task | M01-T042-A |
| Đầu vào | M01-IDENTITY-HISTORY-AUTHORIZATION-1.0 (D-106), M01-RE-REGISTRATION-AFTER-DELETION-1.0 (D-150), M01-ACCOUNT-DELETION-REQUEST-1.0 (D-148), REL-01, REL-02, REL-07 |
| Phạm vi | Đặc tả Bộ Nghiệm thu Xuyên Chức năng Module M01 Lát A (`Cross-Functional Acceptance Suite - Slice A`), kiểm thử tích hợp toàn vẹn vòng đời danh tính, quản lý phiên truy cập, thu hồi thiết bị, bảo mật dữ liệu GDPR và lưu vết kiểm toán M11 |
| Tự kiểm | A-G01, A-G02; REL-01, REL-02, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Bộ Nghiệm thu Xuyên Chức năng Module M01 Lát A (`Cross-Functional Acceptance Suite - Slice A`) thuộc M01, đóng vai trò như bộ kịch bản kiểm thử tích hợp E2E (End-to-End Integration Test Suite) bao phủ $100\%$ các luồng chức năng trọng yếu của danh tính và hồ sơ người dùng: Từ khởi tạo đăng ký, đăng nhập, gia hạn phiên, quản lý thiết bị PUSH đến xuất dữ liệu cá nhân, yêu cầu xóa tài khoản và tái đăng ký (REL-01, REL-02, REL-07).

- **Kiểm thử Tích hợp Xuyên Chức năng Vòng đời Danh tính (`E2E Identity Lifecycle Suite`)**: 100% các thành phần logic M01 BẮT BUỘC trải qua kịch bản kiểm thử tích hợp liên tục từ đầu đến cuối (E2E Integration Flow) xác minh tính khớp nối và không phát sinh lỗi bất biến giữa các dịch vụ.
- **Ràng buộc Bảo vệ Dữ liệu Cá nhân & Không Rò rỉ PII (`Zero PII Egress Invariant`)**: Bộ test suite nghiệm thu xác nhận rằng KHÔNG CÓ bất kỳ dữ liệu nhạy cảm PII nào (Mật khẩu, Refresh Token, OTP Code, IP address chưa băm) bị rò rỉ ra các kênh bên ngoài (AI Gemini, Public Audit Logs) trong suốt vòng đời sử dụng (REL-01, REL-07).
- **Ràng buộc SLA Thực thi Phiên & Vô hiệu hóa Khẩn cấp (`Session Revocation SLA Guard`)**: Xác minh thời gian thu hồi phiên tức thì qua cờ `SecurityEpoch += 1` khi đổi mật khẩu hoặc xóa tài khoản BẮT BUỘC đạt SLA $\le 5$ giây trên toàn hệ thống (D-091, REL-02).
- **Lưu vết Sổ Kiểm toán Bộ Nghiệm thu M11 (`Acceptance Suite Audit Trail`)**: $100\%$ các lần chạy suite nghiệm thu xuyên chức năng M01 Lát A được ghi vết bất biến `ACT-M01-42-SUITE` trong Sổ Kiểm toán M11.

## 2. Ma trận Kịch bản Nghiệm thu Xuyên Chức năng (Cross-Functional Scenario Matrix)

| Mã Luồng Nghiệm thu (`SuiteFlowId`) | Mô tả Luồng Xuyên Chức năng | Các Component Tham gia | SLA Tổng thể | Tiêu chuẩn Nghiệm thu Đạt | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`FLOW_01_AUTH_SESSION`** | Đăng ký $\to$ Đăng nhập $\to$ Refresh Token $\to$ Phát hiện Re-use | M01 (T009, T015, T018) | SLA $\le 500\text{ms}$ | Revoke Token khi phát hiện Re-use | `ACT-M01-42-AUTH` |
| **`FLOW_02_PUSH_DEVICE`** | Đăng ký Device $\to$ Gửi PUSH $\to$ Thu hồi Device (T027-A) | M01, M10, M12 | SLA $\le 800\text{ms}$ | Device Token bị hủy tức thì | `ACT-M01-42-PUSH` |
| **`FLOW_03_GDPR_EXPORT`** | Re-Auth $\le 5\text{m} \to$ Request Export $\to$ ZIP AES-256 (T034) | M01, M11 (T038), S3 | SLA $\le 60\text{s}$ | ZIP mã hóa, TTL S3 Signed URL 7d | `ACT-M01-42-EXPORT` |
| **`FLOW_04_GDPR_DELETE`** | Re-Auth + OTP $\to$ Grace 30d $\to$ Anonymize (T035, T036) | M01, M04, M06, M11 | SLA $\le 15\text{s}$ | PII bị xóa/băm Salted SHA-256 | `ACT-M01-42-DELETE` |
| **`FLOW_05_RE_REGISTER`** | Block 30d $\to$ Grace Expired $\to$ Re-Register Zero-History | M01 (T037), M11 | SLA $\le 10\text{ms}$ | User mới GUID, Streak 0 | `ACT-M01-42-REREG` |

## 3. Kiến trúc Luồng Bộ Nghiệm thu Xuyên Chức năng M01 (Acceptance Runner Pipeline)

```
[Execute M01-T042-A Cross-Functional Acceptance Suite]
                          |
                          v
 +------------------------+------------------------+
 | FLOW_01: Auth & Refresh Token Lifecycle Test     |
 | FLOW_02: Push Device Reg & Instant Revocation   |
 | FLOW_03: GDPR Data Export AES-256 ZIP Test      |
 | FLOW_04: Account Deletion & 30d Anonymization   |
 | FLOW_05: Re-Registration & Zero-History Test    |
 +------------------------+------------------------+
                          |
                          v
     [Evaluate All 10 Acceptance Gates (CA-G01 to CA-G10)]
                          |
        +-----------------+-----------------+
        | (100% Gates Passed)              | (Any Gate Failed)
        v                                    v
[SUITE STATUS: PASSED]             [SUITE STATUS: FAILED]
[Generate E2E Acceptance Report]   [Block Release & Trigger Alert P2]
[Record Audit ACT-M01-42-SUITE]    [Record Audit ACT-M01-42-FAIL]
```

## 4. Giao thức Thực thi Runner Nghiệm thu CSDL (M01AcceptanceSuiteRunner)

```csharp
public async Task<SuiteRunResultDto> RunSliceAAcceptanceSuiteAsync(string engineerUserId)
{
    var result = new SuiteRunResultDto { SuiteId = "M01-CROSS-FUNCTIONAL-ACCEPTANCE-A-1.0", StartedAtUtc = DateTime.UtcNow };
    var sw = Stopwatch.StartNew();

    // 1. Run Flow 01: Auth & Session Lifecycle
    bool flow1Passed = await TestAuthAndSessionLifecycleAsync();

    // 2. Run Flow 02: Push Device Registration & Revocation (M01-T027-A)
    bool flow2Passed = await TestPushDeviceRevocationAsync();

    // 3. Run Flow 03: GDPR Personal Data Export (M01-T034)
    bool flow3Passed = await TestDataExportRequestAsync();

    // 4. Run Flow 04: Account Deletion & Anonymization (M01-T035, T036)
    bool flow4Passed = await TestAccountDeletionAndAnonymizationAsync();

    // 5. Run Flow 05: Re-Registration & Zero-History (M01-T037)
    bool flow5Passed = await TestReRegistrationZeroHistoryAsync();

    sw.Stop();
    result.TotalExecutionTimeMs = sw.ElapsedMilliseconds;
    result.IsSuccess = flow1Passed && flow2Passed && flow3Passed && flow4Passed && flow5Passed;

    // 6. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M01-42-SUITE", engineerUserId, new {
        IsSuccess = result.IsSuccess,
        TotalExecutionTimeMs = result.TotalExecutionTimeMs,
        FlowResults = new { Flow1 = flow1Passed, Flow2 = flow2Passed, Flow3 = flow3Passed, Flow4 = flow4Passed, Flow5 = flow5Passed }
    });

    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CA-G01` | Bộ nghiệm thu Lát A BẮT BUỘC bao phủ $100\%$ các luồng chức năng M01-T009 đến M01-T037. |
| `CA-G02` | Kịch bản Đăng ký $\to$ Đăng nhập $\to$ Gia hạn phiên BẮT BUỘC phát hiện và vô hiệu token tái sử dụng. |
| `CA-G03` | Kịch bản Thu hồi Thiết bị PUSH (M01-T027-A) BẮT BUỘC đạt SLA vô hiệu $\le 5$ giây. |
| `CA-G04` | Kịch bản Xuất Dữ liệu GDPR (M01-T034) BẮT BUỘC tạo tệp ZIP mã hóa AES-256 với Signed URL TTL 7d. |
| `CA-G05` | Kịch bản Xóa Tài khoản (M01-T035/T036) BẮT BUỘC tuân thủ Grace 30d và ẩn danh hóa PII (Salted SHA-256). |
| `CA-G06` | Kịch bản Đăng ký lại (M01-T037) BẮT BUỘC chặn trong 30d Grace và cấp tài khoản Zero-History sau 30d. |
| `CA-G07` | SLA tổng thời gian thực thi toàn bộ test suite nghiệm thu Lát A $< 90$ giây. |
| `CA-G08` | Phân quyền khởi chạy bộ suite nghiệm thu chỉ dành cho `QAEngineer` và `DevOpsLead`. |
| `CA-G09` | Zero PII Egress Guard xác nhận $100\%$ không rò rỉ dữ liệu PII ra log công khai hoặc AI Gemini. |
| `CA-G10` | 100% các test case tự kiểm CA42-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CA42-01` | Chạy E2E Flow 01: Đăng nhập, lấy Refresh Token, dùng lại token cũ | Thu hồi toàn bộ gia đình token, ghi Audit `ACT-M01-42-AUTH` |
| `CA42-02` | Chạy E2E Flow 02: Đăng ký PUSH Device, gửi event thu hồi thiết bị | Hủy active status của Device Token SLA $< 1.5\text{s}$ |
| `CA42-03` | Chạy E2E Flow 03: Re-Auth 2m trước, gửi yêu cầu xuất dữ liệu GDPR | Tạo tệp nén ZIP AES-256, link S3 Signed URL có TTL 7d |
| `CA42-04` | Chạy E2E Flow 04: Xin xóa tài khoản, đợi 31d cho worker ẩn danh hóa | PII bị xóa, Audit Log chuyển sang Salted SHA-256 |
| `CA42-05` | Chạy E2E Flow 05: Thử đăng ký lại ở ngày 15, sau đó thử lại ở ngày 35 | Block ở ngày 15, cấp `UserId` GUID mới Zero-History ở ngày 35 |
| `CA42-06` | Tra cứu vết Audit Log M11 sau khi chạy thành công bộ suite | Ghi nhận Audit Event `ACT-M01-42-SUITE` |
| `CA42-07` | Thử kích hoạt bộ suite nghiệm thu từ môi trường chưa xác thực | Deny 401 Unauthorized |
| `CA42-08` | Tra cứu báo cáo kết quả chi tiết từng Flow nghiệm thu Lát A | Trả về DTO báo cáo kết quả E2E |
| `CA42-09` | Tải đồng thời 10 runner test suite nghiệm thu song song | Suite runner processing SLA $< 85\text{s}$ |
| `CA42-10` | Kiểm tra thời gian dọn dẹp các tài khoản test tạm sau khi nghiệm thu xong | Cleanup temporary test accounts SLA $< 5\text{s}$ |
| `CA42-11` | Thử nạp mã `FlowId` nghiệm thu không hợp lệ | Reject 400 `INVALID_SUITE_FLOW_ID` |
| `CA42-12` | Gửi request chạy suite nghiệm thu khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `CA42-13` | User không phải QAEngineer/DevOpsLead thử khởi chạy suite nghiệm thu | Deny 403 Forbidden |
| `CA42-14` | User chưa đăng nhập gọi API tra cứu kết quả suite nghiệm thu M01 | Deny 401 Unauthorized |
| `CA42-15` | Giả lập lỗi ở Flow 03 (S3 Upload Failed) | Suite đánh dấu `FAILED`, phát alert P2 |
| `CA42-16` | Kiểm tra độ trễ phát thông báo P2 khi test suite bị FAILED | Dispatch SLA $< 3\text{s}$ |
| `CA42-17` | Phân tích tham chiếu các bản ghi `AcceptanceSuiteRuns` trong CSDL | Quét schema `M01_AcceptanceSuiteRuns` (T020) |
| `CA42-18` | Dịch vụ Redis bị ngắt kết nối giữa lúc test Flow 01 | Catch exception, báo lỗi Flow 01 nhưng vẫn chạy tiếp Flow khác |
| `CA42-19` | Tra cứu lịch sử các đợt chạy test suite nghiệm thu Lát A trong tháng | Trả về DTO danh sách HistoricalSuiteRuns |
| `CA42-20` | Kiểm thử hoàn tất bộ nghiệm thu xuyên chức năng M01-CROSS-FUNCTIONAL-ACCEPTANCE-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-CA-I01` | M01 hiện tại chưa có `M01AcceptanceSuiteRunner` E2E Integration | Risk không kiểm chứng được tính khớp nối giữa các task M01 | M01-T049 (Source task) |
| `M01-CA-I02` | Thiếu luồng tự động kiểm thử 5 E2E Integration Flows Lát A | Cần thực hiện kiểm thử thủ công tốn thời gian và dễ sót lỗi | M01-T049; REL-01 |
| `M01-CA-I03` | Thiếu cờ Zero PII Egress Guard xác minh rò rỉ dữ liệu | Risk lọt thông tin cá nhân ra ngoài qua log hoặc AI | M01-T049; REL-07 |
| `M01-CA-I04` | Thiếu luồng tự động phát alert P2 khi suite nghiệm thu bị FAILED | Đội ngũ kỹ thuật không biết sớm khi hệ thống bị sập luồng | M01-CA-F04; M11-T037 |
| `M01-CA-I05` | Chưa kết nối sự kiện chạy suite nghiệm thu với Audit Log M11 (`ACT-M01-42-SUITE`) | Không ghi vết được lịch sử kiểm thử nghiệm thu Lát A | M01-T049; M11-T031 |

- `M01-CA-F01`: Triển khai `M01AcceptanceSuiteRunner` với 5 E2E Integration Flows (tiếp nhận: M01-T049).
- `M01-CA-F02`: Tích hợp Bắt buộc Zero PII Egress Guard & E2E Validation (tiếp nhận: M01-T049; REL-07).
- `M01-CA-F03`: Triển khai Automated Alert P2 & Temporary Account Cleanup (tiếp nhận: M01-T049; M11-T037).
- `M01-CA-F04`: Thiết lập bộ kiểm thử tự động CA-G01–G10 và CA42-01–20 (tiếp nhận: M01 tasks).
- `M01-CA-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T042-A

- Đã thiết kế hoàn chỉnh `M01-CROSS-FUNCTIONAL-ACCEPTANCE-A-1.0` với Ma trận Kịch bản Nghiệm thu Xuyên Chức năng.
- Đã chốt Ràng buộc Kiểm thử Tích hợp Xuyên Chức năng Vòng đời Danh tính (`E2E Identity Lifecycle Suite` 5 Flows).
- Đã chốt Ràng buộc Bảo vệ Dữ liệu Cá nhân & Không Rò rỉ PII (`Zero PII Egress Invariant`).
- Đã lồng ghép Ràng buộc SLA Thực thi Phiên & Vô hiệu hóa Khẩn cấp ($\le 5\text{s}$ REL-02), Cleanup Tài khoản Test tạm và Audit Log M11 (`ACT-M01-42-SUITE`).
- Đã xác lập 10 Regression Gates (`CA-G01`–`CA-G10`) và 20 Test Cases tự kiểm (`CA42-01`–`CA42-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả bộ nghiệm thu xuyên chức năng Lát A M01-T042-A | WSA-7K2 |
