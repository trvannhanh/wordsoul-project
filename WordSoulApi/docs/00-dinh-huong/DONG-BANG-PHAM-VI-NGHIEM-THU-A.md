# Đóng băng phạm vi nghiệm thu Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-PHASE-A-SCOPE-FREEZE-1.0` |
| Task | A5-T001 |
| Đầu vào | Tất cả các task thuộc Lát 0 đến Lát 4 (M01-M12), A-G01 đến A-G06 |
| Phạm vi | Đặc tả Giao thức Đóng băng Phạm vi Nghiệm thu Giai đoạn A (`Phase A Scope Freeze Protocol`), chính thức khóa baseline kiến trúc của 140+ task thuộc Lát 0–4, quy trình kiểm soát thay đổi khẩn cấp (CCB) và lưu vết kiểm toán M11 |
| Tự kiểm | A-G01, A-G02, A-G03, A-G04, A-G05, A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đóng băng Phạm vi Nghiệm thu Giai đoạn A (`Phase A Scope Freeze Protocol`) thuộc A5 (Điều phối & Nghiệm thu), chính thức khóa phạm vi kiến trúc và đặc tả kỹ thuật cho toàn bộ 140+ task thuộc Lát 0 đến Lát 4 trên 12 Module (M01–M12) của dự án WordSoul, ngăn chặn tình trạng phình phạm vi (Scope Creep) và bảo đảm tính sẵn sàng tuyệt đối cho công tác nghiệm thu chất lượng Giai đoạn A (A-G01–A-G06).

- **Quy tắc Đóng băng Phạm vi Baseline 100% (`100% Phase A Baseline Freeze Invariant`)**: Kể từ thời điểm đóng băng phạm vi (Scope Freeze Baseline Timestamp: 2026-08-21T05:00:00Z), $100\%$ các file tài liệu đặc tả, hợp đồng API, từ điển dữ liệu và ma trận phân quyền thuộc Lát 0 đến Lát 4 BẮT BUỘC giữ nguyên trạng thái khóa. TUYỆT ĐỐI CẤM tự ý sửa đổi hoặc bổ sung tính năng mới ngoài phạm vi.
- **Quy trình Kiểm soát Thay đổi Khẩn cấp CCB (`Emergency Change Control Board - CCB Protocol`)**: Mọi sửa đổi phát sinh do lỗi bảo mật nghiêm trọng (`SEV-1`) hoặc sai lệch hợp đồng API sau thời điểm Scope Freeze BẮT BUỘC trải qua quy trình phê duyệt 2 cấp của Hội đồng Kiểm soát Thay đổi (CCB Board: Lead Architect, Security Lead, Product Owner) kèm biên bản giải trình tác động.
- **Ràng buộc Tính Đầy đủ & Khép kín 140+ Task (`140+ Task Coverage Invariant`)**: Đảm bảo toàn bộ 140+ task thuộc Lát 0–4 đã chuyển trạng thái `Hoàn thành` trong `docs/TASKS.md`, có đầy đủ Decision Entry (D-001 đến D-152) và link `file://` hợp lệ trong Module READMEs.
- **Lưu vết Sổ Kiểm toán Đóng băng M11 (`Scope Freeze Audit Trail`)**: Sự kiện chính thức Đóng băng Phạm vi Nghiệm thu Giai đoạn A được ghi vết bất biến `ACT-A5-01-FREEZE` trong Sổ Kiểm toán M11.

## 2. Ma trận Phạm vi Đóng băng Giai đoạn A theo Lát (Phase A Slice Freeze Matrix)

| Lát Phạm vi (`SliceId`) | Mô tả Phạm vi Đóng băng | Số lượng Task | Trạng thái Đóng băng | Cơ chế Khóa | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`SLICE_0`** | Hạ tầng cốt lõi, Docker, CSDL Postgres/Redis | 15 Tasks | **FROZEN 100%** | Baseline Commit Lock | `ACT-A5-01-SLICE0` |
| **`SLICE_1`** | Danh tính M01, Xác thực OAuth, Tokens | 28 Tasks | **FROZEN 100%** | Baseline Commit Lock | `ACT-A5-01-SLICE1` |
| **`SLICE_2`** | Từ vựng M02, SRS Learning Engine M04 | 32 Tasks | **FROZEN 100%** | Baseline Commit Lock | `ACT-A5-01-SLICE2` |
| **`SLICE_3`** | Gamification M05, Thanh toán M06, Social M08 | 35 Tasks | **FROZEN 100%** | Baseline Commit Lock | `ACT-A5-01-SLICE3` |
| **`SLICE_4`** | Push M10, Quản trị M11, Tích hợp M12 | 30+ Tasks | **FROZEN 100%** | Baseline Commit Lock | `ACT-A5-01-SLICE4` |

## 3. Kiến trúc Luồng Đóng băng Phạm vi A5 (Scope Freeze Pipeline)

```
[Trigger Phase A Scope Freeze Command (A5-T001)]
                        |
                        v
    [Verify All 140+ Tasks in TASKS.md Status == 'Hoàn thành']
    [Verify All Decisions D-001 to D-152 in DECISIONS.md]
    [Verify All file:// Markdown Links in Module READMEs]
                        |
                        v
        +---------------+---------------+
        | (100% Check Passed)           | (Any Missing Task/Decision)
        v                               v
[Apply Baseline Commit Lock]     [REJECT Scope Freeze Command]
[Set ScopeFreezeStatus: FROZEN]  [Report Unfinished Items]
[Publish Scope Freeze Event]
[Record Audit ACT-A5-01-FREEZE]
```

## 4. Giao thức Thực thi Đóng băng CSDL (PhaseAScopeFreezeService)

```csharp
public async Task<ScopeFreezeResultDto> ExecutePhaseAScopeFreezeAsync(string leadArchitectUserId)
{
    var result = new ScopeFreezeResultDto { Phase = "Phase A", FrozenAtUtc = DateTime.UtcNow };

    // 1. Verify 100% Tasks Completed in TASKS.md
    bool allTasksCompleted = VerifyAllSlice0To4TasksCompleted();
    if (!allTasksCompleted)
    {
        throw new InvalidOperationException("SCOPE_FREEZE_REJECTED: Không thể đóng băng phạm vi vì vẫn còn task Lát 0-4 chưa chuyển trạng thái Hoàn thành.");
    }

    // 2. Lock Baseline Commit SHA in Database
    string currentGitCommitSha = GetCurrentGitCommitHash();
    
    var freezeRecord = new PhaseAScopeFreezeRecord {
        FreezeId = Guid.NewGuid().ToString("N"),
        BaselineCommitSha = currentGitCommitSha,
        Status = FreezeStatus.FROZEN,
        FrozenAtUtc = DateTime.UtcNow,
        FrozenByUserId = leadArchitectUserId
    };

    _db.ScopeFreezeRecords.Add(freezeRecord);
    await _db.SaveChangesAsync();

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-01-FREEZE", leadArchitectUserId, new {
        BaselineCommitSha = currentGitCommitSha,
        Status = "FROZEN",
        Action = "PHASE_A_SCOPE_FREEZE_EXECUTED"
    });

    result.IsSuccess = true;
    result.BaselineCommitSha = currentGitCommitSha;
    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SF-G01` | Lệnh Đóng băng Phạm vi BẮT BUỘC chỉ thành công khi 100% task Lát 0–4 đạt trạng thái `Hoàn thành`. |
| `SF-G02` | Tất cả quyết định từ D-001 đến D-152 BẮT BUỘC được ghi nhận đầy đủ trong `docs/DECISIONS.md`. |
| `SF-G03` | Sau thời điểm Scope Freeze, mọi thay đổi baseline BẮT BUỘC phải qua phê duyệt CCB Board. |
| `SF-G04` | 100% các liên kết tệp `file://` trong tài liệu 12 Module READMEs BẮT BUỘC hoạt động chuẩn xác. |
| `SF-G05` | SLA thực thi kiểm tra tính đầy đủ của 140+ task trước khi đóng băng $< 3.0$ giây. |
| `SF-G06` | Sự kiện Đóng băng Phạm vi Giai đoạn A BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-01-FREEZE`). |
| `SF-G07` | Phân quyền thực thi lệnh Scope Freeze chỉ dành riêng cho `LeadArchitect`. |
| `SF-G08` | Mã Git Commit SHA baseline đóng băng BẮT BUỘC được lưu cố định trong CSDL quản trị A5. |
| `SF-G09` | Kiểm tra tính đóng khép của ma trận phân quyền RBAC và hợp đồng API trên 12 Module. |
| `SF-G10` | 100% các test case tự kiểm SF01-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SF01-01` | Chạy lệnh Scope Freeze khi 100% task Lát 0–4 đã hoàn thành | Khóa baseline `Status = FROZEN`, lưu Git Commit SHA |
| `SF01-02` | Thử chạy Scope Freeze khi có 1 task Lát 3 đang ở trạng thái `Đang thực hiện` | Reject HTTP 400 `SCOPE_FREEZE_REJECTED` |
| `SF01-03` | Thử tự ý sửa đổi file đặc tả M01 sau thời điểm Scope Freeze mà KHÔNG có CCB Approval | Reject commit/merge, yêu cầu biên bản CCB |
| `SF01-04` | Nạp biên bản CCB được duyệt bởi Lead Architect & Security Lead để sửa lỗi hotfix SEV-1 | Cho phép mở khóa tạm thời sửa 1 file đặc tả |
| `SF01-05` | Tra cứu vết Audit Log M11 sau khi thực thi lệnh Scope Freeze | Ghi nhận Audit Event `ACT-A5-01-FREEZE` |
| `SF01-06` | Tra cứu mã Git Commit SHA baseline đã đóng băng từ CSDL A5 | Trả về chuỗi Git Commit SHA chính xác |
| `SF01-07` | Developer thử bấm nút Scope Freeze khi không có quyền LeadArchitect | Reject HTTP 403 `FORBIDDEN_LEAD_ARCHITECT_ONLY` |
| `SF01-08` | Tra cứu trạng thái Đóng băng Phạm vi Giai đoạn A qua API public | Trả về status `FROZEN` kèm timestamp |
| `SF01-09` | Tải đồng thời 50 request tra cứu trạng thái Scope Freeze | Response latency p95 $< 5\text{ms}$ |
| `SF01-10` | Kiểm tra độ trễ đồng bộ sự kiện Scope Freeze sang Slack #announcements | Dispatch SLA $< 1.5\text{s}$ |
| `SF01-11` | Thử nạp mã `Phase` không hợp lệ (Ví dụ: Phase Z) | Reject 400 `INVALID_PHASE_IDENTIFIER` |
| `SF01-12` | Gửi request Scope Freeze khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `SF01-13` | User không phải LeadArchitect thử khởi chạy Scope Freeze | Deny 403 Forbidden |
| `SF01-14` | User chưa đăng nhập gọi API tra cứu trạng thái Scope Freeze A5 | Cho phép xem công khai trạng thái đóng băng |
| `SF01-15` | Thử hủy lệnh Scope Freeze đã đóng bằng bằng tài khoản thường | Reject 403 `UNFREEZE_DENIED` |
| `SF01-16` | Kiểm tra tính tương thích của mã Commit SHA baseline với Git repository | Matching 100% git HEAD commit |
| `SF01-17` | Phân tích tham chiếu các bản ghi `ScopeFreezeRecords` trong CSDL | Quét schema `A5_ScopeFreezeRecords` (T020) |
| `SF01-18` | Dịch vụ CSDL bị gián đoạn khi đang ghi nhận bản ghi Scope Freeze | Rollback transaction, giữ trạng thái chưa đóng băng |
| `SF01-19` | Tra cứu danh sách các tài liệu thuộc phạm vi đóng băng của Lát 2 | Trả về DTO danh sách Slice2FrozenArtifacts |
| `SF01-20` | Kiểm thử hoàn tất đóng băng phạm vi nghiệm thu A5-PHASE-A-SCOPE-FREEZE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-SF-I01` | A5 hiện tại chưa có `PhaseAScopeFreezeService` khóa baseline | Risk tiếp tục sửa đổi phình phạm vi gây xáo trộn tài liệu | A5-T002 (Source task) |
| `A5-SF-I02` | Thiếu luồng tự động kiểm tra tính đầy đủ của 140+ task trước khi đóng băng | Phải kiểm tra thủ công dễ bỏ sót task chưa hoàn thành | A5-T002; A-G01 |
| `A5-SF-I03` | Thiếu quy trình Phê duyệt Thay đổi Khẩn cấp CCB Board | Không có cơ chế quản lý các hotfix phát sinh sau đóng băng | A5-T002; A-G06 |
| `A5-SF-I04` | Thiếu cờ lưu trữ Git Commit SHA baseline bất biến trong CSDL A5 | Không chứng minh được trạng thái chính xác tại thời điểm khóa | A5-SF-F04; DevOps |
| `A5-SF-I05` | Chưa kết nối sự kiện Đóng băng Phạm vi với Audit Log M11 (`ACT-A5-01-FREEZE`) | Không ghi vết được lịch sử đóng băng phạm vi Giai đoạn A | A5-T002; M11-T031 |

- `A5-SF-F01`: Triển khai `PhaseAScopeFreezeService` với 100% Task Completion Checker (tiếp nhận: A5-T002).
- `A5-SF-F02`: Tích hợp Bắt buộc Git Commit SHA Baseline Locking & CCB Approval Engine (tiếp nhận: A5-T002; A-G01).
- `A5-SF-F03`: Triển khai Emergency Change Control Board & Slack Announcement (tiếp nhận: A5-T002; DevOps).
- `A5-SF-F04`: Thiết lập bộ kiểm thử tự động SF-G01–G10 và SF01-01–20 (tiếp nhận: A5 tasks).
- `A5-SF-F05`: Thu thập bằng chứng runtime cho luồng đóng băng phạm vi A5 (tiếp nhận: A5 tasks; A-G01–A-G06).

## 8. Tự kiểm A5-T001

- Đã thiết kế hoàn chỉnh `A5-PHASE-A-SCOPE-FREEZE-1.0` với Ma trận Phạm vi Đóng băng Giai đoạn A theo Lát.
- Đã chốt Ràng buộc Quy tắc Đóng băng Phạm vi Baseline 100% (`100% Phase A Baseline Freeze Invariant`).
- Đã chốt Ràng buộc Quy trình Kiểm soát Thay đổi Khẩn cấp CCB (`Emergency Change Control Board - CCB Protocol`).
- Đã lồng ghép Ràng buộc Tính Đầy đủ & Khép kín 140+ Task (D-001 đến D-152), Git Commit SHA Baseline Locking và Audit Log M11 (`ACT-A5-01-FREEZE`).
- Đã xác lập 10 Regression Gates (`SF-G01`–`SF-G10`) và 20 Test Cases tự kiểm (`SF01-01`–`SF01-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả đóng băng phạm vi nghiệm thu A A5-T001 | WSA-7K2 |
