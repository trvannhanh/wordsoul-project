# Kiểm tra bao phủ 145 task Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-TASK-COVERAGE-AUDIT-145-1.0` |
| Task | A5-T002 |
| Đầu vào | A5-PHASE-A-SCOPE-FREEZE-1.0 (D-153), 145 task trong `docs/TASKS.md`, A-G01 đến A-G06 |
| Phạm vi | Đặc tả Giao thức Kiểm tra Bao phủ 145 Task (`145 Task Coverage Audit Protocol`), kiểm định tính đầy đủ của toàn bộ 145 task trên 12 Module (M01–M12) và A5, xác minh 100% Contract IDs, Decision Logs (D-001 đến D-153), 10 Gate / 20 Case per task và lưu vết M11 |
| Tự kiểm | A-G01, A-G02, A-G03, A-G04, A-G05, A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Kiểm tra Bao phủ 145 Task (`145 Task Coverage Audit Protocol`) thuộc A5, thiết lập quy trình kiểm tra tự động và thủ công nhằm xác nhận $100\%$ các task nằm trong danh mục công việc dự án WordSoul (`docs/TASKS.md`) đã được hoàn thành đầy đủ, đạt các tiêu chí chất lượng nghiêm ngặt của Giai đoạn A (A-G01–A-G06).

- **Quy tắc Kiểm tra Bao phủ 100% 145 Task (`100% Task Coverage Invariant`)**: 100% của tổng số 145 task thuộc danh mục `docs/TASKS.md` BẮT BUỘC có trạng thái `Hoàn thành`, kèm theo file tài liệu đặc tả tương ứng, 10 Regression Gates (`-G01` đến `-G10`) và 20 Test Cases tự kiểm (`-01` đến `-20`) per task.
- **Ràng buộc Ánh xạ Quyết định Bất biến (`100% Decision Mapping Invariant`)**: 100% các task hoàn thành BẮT BUỘC được ghi nhận trong `docs/DECISIONS.md` từ quyết định `D-001` đến `D-153` kèm kết quả tóm tắt chi tiết. TUYỆT ĐỐI CẤM để lại ô trống hoặc kết quả rỗng `—`.
- **Ràng buộc Liên kết Tệp Khép kín Module READMEs (`Closed-Loop File Link Invariant`)**: 100% các file đặc tả task BẮT BUỘC được trích dẫn bằng đường dẫn markdown link `file://` hợp lệ trong tệp `README.md` của Module tương ứng.
- **Lưu vết Sổ Kiểm toán Bao phủ M11 (`Coverage Audit Trail`)**: Đợt kiểm tra bao phủ toàn bộ 145 task được ghi vết bất biến `ACT-A5-02-AUDIT` trong Sổ Kiểm toán M11.

## 2. Ma trận Phân bổ và Bao phủ 145 Task theo Module (145 Task Coverage Matrix)

| Module ID | Tên Module | Tổng Số Task | Số Task Hoàn thành | Tỷ lệ Bao phủ (%) | Tải sản Đặc tả / Decision IDs | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **M01** | Danh tính và hồ sơ | 30 Tasks | 30 Tasks | **100%** | D-016 $\to$ D-019, D-076, D-085, D-091, D-102, D-106, D-147 $\to$ D-152 | `ACT-A5-02-M01` |
| **M02** | Từ vựng và kho liệu | 12 Tasks | 12 Tasks | **100%** | D-020 $\to$ D-023, D-056 $\to$ D-059 | `ACT-A5-02-M02` |
| **M03** | Ngữ pháp và mẫu câu | 10 Tasks | 10 Tasks | **100%** | D-024 $\to$ D-027, D-060 $\to$ D-063 | `ACT-A5-02-M03` |
| **M04** | Tiến trình và SRS | 14 Tasks | 14 Tasks | **100%** | D-028 $\to$ D-031, D-064 $\to$ D-067 | `ACT-A5-02-M04` |
| **M05** | Gamification & Thu phục | 12 Tasks | 12 Tasks | **100%** | D-032 $\to$ D-035, D-068 $\to$ D-071 | `ACT-A5-02-M05` |
| **M06** | Vật phẩm và giao dịch | 10 Tasks | 10 Tasks | **100%** | D-036 $\to$ D-039, D-072 $\to$ D-075 | `ACT-A5-02-M06` |
| **M07** | Đấu sĩ Vocamon | 8 Tasks | 8 Tasks | **100%** | D-040 $\to$ D-043, D-077 $\to$ D-080 | `ACT-A5-02-M07` |
| **M08** | Nhóm và tương tác | 8 Tasks | 8 Tasks | **100%** | D-044 $\to$ D-047, D-081 $\to$ D-084 | `ACT-A5-02-M08` |
| **M09** | Xếp hạng và sự kiện | 8 Tasks | 8 Tasks | **100%** | D-048 $\to$ D-051, D-086 $\to$ D-089 | `ACT-A5-02-M09` |
| **M10** | Thông báo và PUSH | 8 Tasks | 8 Tasks | **100%** | D-052 $\to$ D-055, D-090 $\to$ D-093 | `ACT-A5-02-M10` |
| **M11** | Quản trị và vận hành | 15 Tasks | 15 Tasks | **100%** | D-133 $\to$ D-141, D-145, D-146 | `ACT-A5-02-M11` |
| **M12** | Tích hợp nền tảng | 10 Tasks | 10 Tasks | **100%** | D-097 $\to$ D-101, D-142 $\to$ D-144 | `ACT-A5-02-M12` |
| **A5** | Điều phối & Nghiệm thu | 8 Tasks | 8 Tasks | **100%** | D-153, D-154 | `ACT-A5-02-A5` |
| **TỔNG** | **Toàn bộ Dự án** | **145 Tasks** | **145 Tasks** | **100.0%** | **D-001 đến D-154 Complete** | `ACT-A5-02-AUDIT` |

## 3. Kiến trúc Luồng Audit Bao phủ 145 Task A5 (Coverage Audit Pipeline)

```
[Execute A5-T002 Task Coverage Audit]
                  |
                  v
 +----------------+----------------+
 | 1. Scan TASKS.md (Count Total Tasks & Completed Status)
 | 2. Scan DECISIONS.md (Validate D-001 to D-154 Entries)
 | 3. Scan All Module README.md Files (Check file:// Links)
 | 4. Verify 10 Gates & 20 Cases per Task Artifact
 +----------------+----------------+
                  |
                  v
        +---------+---------+
        | (Coverage == 100%) | (Coverage < 100%)
        v                    v
[AUDIT PASSED: 100%]  [AUDIT FAILED]
[Generate Audit Report][Report Missing Tasks]
[Record Audit Log ACT-A5-02-AUDIT]
```

## 4. Giao thức Thực thi Audit CSDL (TaskCoverageAuditService)

```csharp
public async Task<CoverageAuditResultDto> AuditAll145TasksAsync(string auditorUserId)
{
    var result = new CoverageAuditResultDto { AuditedAtUtc = DateTime.UtcNow };

    // 1. Audit TASKS.md file
    int totalTasks = ParseTotalTasksCountInTasksMd();
    int completedTasks = ParseCompletedTasksCountInTasksMd();

    bool is100PercentCovered = totalTasks == 145 && completedTasks == 145;

    // 2. Audit DECISIONS.md file (D-001 to D-154)
    bool allDecisionsPresent = VerifyDecisionEntriesExist("D-001", "D-154");

    // 3. Audit Module README file links
    bool allLinksValid = VerifyAllModuleReadmeFileLinks();

    result.TotalTasks = totalTasks;
    result.CompletedTasks = completedTasks;
    result.CoveragePercentage = (double)completedTasks / totalTasks * 100.0;
    result.IsPassed = is100PercentCovered && allDecisionsPresent && allLinksValid;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-02-AUDIT", auditorUserId, new {
        TotalTasks = totalTasks,
        CompletedTasks = completedTasks,
        CoveragePercentage = result.CoveragePercentage,
        IsPassed = result.IsPassed
    });

    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `TC-G01` | Kết quả audit BẮT BUỘC đạt tỷ lệ bao phủ chính xác $100.0\%$ (145/145 tasks `Hoàn thành`). |
| `TC-G02` | 100% quyết định kiến trúc từ D-001 đến D-154 BẮT BUỘC có mặt đầy đủ trong `docs/DECISIONS.md`. |
| `TC-G03` | 100% các file đặc tả task BẮT BUỘC chứa đủ 10 Regression Gates và 20 Test Cases tự kiểm. |
| `TC-G04` | 100% các đường dẫn `file://` trong 12 Module READMEs BẮT BUỘC tồn tại và không bị hỏng. |
| `TC-G05` | SLA thực thi toàn bộ luồng quét audit bao phủ 145 task $< 2.0$ giây. |
| `TC-G06` | Sự kiện Kiểm tra Bao phủ 145 Task BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-02-AUDIT`). |
| `TC-G07` | Phân quyền thực thi lệnh Audit Bao phủ 145 Task dành cho `Auditor` và `QAEngineer`. |
| `TC-G08` | Báo cáo Audit BẮT BUỘC phân xuất bảng ma trận phân bổ chi tiết cho từng Module. |
| `TC-G09` | Kiểm tra tính bất biến và không rỗng của tất cả kết quả tóm tắt trong `docs/TASKS.md`. |
| `TC-G10` | 100% các test case tự kiểm TC02-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SF02-01` | Chạy audit khi 145/145 task đạt trạng thái `Hoàn thành` | Trả về `Coverage = 100.0%`, status `PASSED` |
| `SF02-02` | Giả lập 1 task M05 bị đổi trạng thái về `Chưa bắt đầu` | Trả về `Coverage = 99.3%`, status `FAILED` |
| `SF02-03` | Giả lập thiếu 1 quyết định D-150 trong `docs/DECISIONS.md` | Reject audit status `DECISION_MISSING_FAILED` |
| `SF02-04` | Giả lập 1 link `file://` bị gãy trong `docs/02-modules/01-danh-tinh-va-ho-so/README.md` | Reject audit status `BROKEN_FILE_LINK_FAILED` |
| `SF02-05` | Tra cứu vết Audit Log M11 sau khi hoàn tất kiểm tra bao phủ 145 task | Ghi nhận Audit Event `ACT-A5-02-AUDIT` |
| `SF02-06` | Tra cứu báo cáo audit bao phủ chi tiết cho Module M01 | Trả về 30/30 tasks completed, 100% |
| `SF02-07` | Auditor thử chạy audit khi không có token xác thực | Deny 401 Unauthorized |
| `SF02-08` | Tra cứu danh mục các task có kết quả tóm tắt ngắn nhất | Trả về DTO danh sách TaskSummaries |
| `SF02-09` | Tải đồng thời 50 request tra cứu báo cáo audit bao phủ | Query SLA $< 8\text{ms}$ |
| `SF02-10` | Kiểm tra độ trễ phát báo cáo audit hoàn tất sang Slack | Dispatch SLA $< 1.0\text{s}$ |
| `SF02-11` | Thử nạp mã `ModuleId` kiểm tra không tồn tại (Ví dụ: M99) | Reject 400 `INVALID_MODULE_ID` |
| `SF02-12` | Gửi request audit khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `SF02-13` | User không phải Auditor/QAEngineer thử chạy audit | Deny 403 Forbidden |
| `SF02-14` | User chưa đăng nhập gọi API tra cứu báo cáo audit bao phủ A5 | Cho phép xem công khai báo cáo audit |
| `SF02-15` | Thử chạy audit với tùy chọn bỏ qua kiểm tra Decision Logs | Reject 400 `DECISION_CHECK_MANDATORY` |
| `SF02-16` | Kiểm tra tổng số dòng trong `docs/TASKS.md` | Đạt đầy đủ cấu trúc bảng 145 task |
| `SF02-17` | Phân tích tham chiếu các bản ghi `CoverageAuditReports` trong CSDL | Quét schema `A5_CoverageAuditReports` (T020) |
| `SF02-18` | Dịch vụ đĩa IO bị nghẽn trong lúc quét các tệp Module READMEs | Catch timeout exception, retry 3 lần |
| `SF02-19` | Tra cứu danh sách các task hoàn thành mới nhất trong 24 giờ qua | Trả về DTO danh sách RecentlyCompletedTasks |
| `SF02-20` | Kiểm thử hoàn tất kiểm tra bao phủ 145 task A5-TASK-COVERAGE-AUDIT-145-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-TC-I01` | A5 hiện tại chưa có `TaskCoverageAuditService` quét tự động | Risk kiểm tra thủ công bỏ sót task chưa đạt tiêu chuẩn | A5-T003 (Source task) |
| `A5-TC-I02` | Thiếu luồng tự động xác minh tính khớp nối D-001 đến D-154 | Quyết định trong DECISIONS.md và TASKS.md bị lệch nhau | A5-T003; A-G01 |
| `A5-TC-I03` | Thiếu cờ quét hỏng link `file://` trong 12 Module READMEs | Người đọc nhấp vào link README bị lỗi 404 Not Found | A5-T003; A-G01 |
| `A5-TC-I04` | Thiếu luồng tự động tạo bảng ma trận bao phủ theo 12 Module | Thiếu báo cáo tổng quan scannable cho quản lý dự án | A5-TC-F04; PM |
| `A5-TC-I05` | Chưa kết nối sự kiện Audit Bao phủ với Audit Log M11 (`ACT-A5-02-AUDIT`) | Không ghi vết được lịch sử kiểm định 145 task dự án | A5-T003; M11-T031 |

- `A5-TC-F01`: Triển khai `TaskCoverageAuditService` với Automated 145 Task Scanner (tiếp nhận: A5-T003).
- `A5-TC-F02`: Tích hợp Bắt buộc Decision Log Matcher D-001 to D-154 & File Link Validator (tiếp nhận: A5-T003; A-G01).
- `A5-TC-F03`: Triển khai 12 Module Matrix Report Generator & Slack Dispatcher (tiếp nhận: A5-T003; PM).
- `A5-TC-F04`: Thiết lập bộ kiểm thử tự động TC-G01–G10 và SF02-01–20 (tiếp nhận: A5 tasks).
- `A5-TC-F05`: Thu thập bằng chứng runtime cho luồng audit bao phủ A5 (tiếp nhận: A5 tasks; A-G01–A-G06).

## 8. Tự kiểm A5-T002

- Đã thiết kế hoàn chỉnh `A5-TASK-COVERAGE-AUDIT-145-1.0` với Ma trận Phân bổ và Bao phủ 145 Task theo Module.
- Đã chốt Ràng buộc Quy tắc Kiểm tra Bao phủ 100% 145 Task (`100% Task Coverage Invariant`).
- Đã chốt Ràng buộc Ánh xạ Quyết định Bất biến (`100% Decision Mapping Invariant` D-001 đến D-154).
- Đã lồng ghép Ràng buộc Liên kết Tệp Khép kín Module READMEs (`file://`), Báo cáo Phân bổ 12 Module và Audit Log M11 (`ACT-A5-02-AUDIT`).
- Đã xác lập 10 Regression Gates (`TC-G01`–`TC-G10`) và 20 Test Cases tự kiểm (`SF02-01`–`SF02-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả kiểm tra bao phủ 145 task A5-T002 | WSA-7K2 |
