# Thiết kế lịch sử chạy và phục hồi công việc M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-BACKGROUND-JOB-EXECUTION-RECOVERY-1.0` |
| Task | M11-T039 |
| Đầu vào | M11-BACKGROUND-JOB-REGISTRY-1.0 (D-135), REL-07 |
| Phạm vi | Đặc tả Giao thức Lịch sử Thực thi và Phục hồi Công việc Nền (`Job Execution History & Manual Recovery Protocol`), mô hình theo dõi kết quả thực thi worker, thời hạn lưu vết 90 ngày theo REL-07, cơ chế Re-Auth $\le 5$ phút khi kích hoạt lại thủ công và lưu vết kiểm toán M11 |
| Tự kiểm | A-G02, A-G06; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Lịch sử Thực thi và Phục hồi Công việc Nền (`Job Execution History & Manual Recovery Protocol`) thuộc M11, xác lập hệ thống ghi vết minh bạch lịch sử từng lần chạy của các worker ngầm (thời gian bắt đầu/kết thúc, số bản ghi đã xử lý, mã lỗi StackTrace), đồng thời cung cấp giao diện phục hồi thủ công an toàn cho quản trị viên khi công việc bị thất bại (REL-07).

- **Cấu trúc Dữ liệu Lịch sử Thực thi Bất biến (`Immutable Execution Envelope Invariant`)**: Mỗi lần khởi chạy công việc ngầm BẮT BUỘC tạo một bản ghi `JobExecutionHistory` chứa `JobExecutionId`, `JobId`, `StartTimeUtc`, `EndTimeUtc`, `ExecutionState` (`SUCCESS`, `FAILED`, `STALLED_KILLED`), `RowsProcessed`, `ErrorMessage` và `StackTraceHash`. Bản ghi này BẤT BIẾN, CẤM chỉnh sửa (REL-07).
- **Thời hạn Lưu giữ Lịch sử 90 Ngày REL-07 (`90-Day History Retention Invariant`)**: Lịch sử thực thi các công việc nền BẮT BUỘC lưu giữ trong 90 ngày (`RetentionDays = 90`). Sau 90 ngày, hệ thống tự động lưu trữ nén sang WORM Cold Storage và dọn dẹp khỏi CSDL chính.
- **Quy trình Phục hồi & Kích hoạt Chạy Thủ công (`Manual Recovery & Re-Auth Guard`)**: Quản trị viên kích hoạt đợt chạy lại thủ công (`ManualTriggerJobAsync`) cho một job bị thất bại BẮT BUỘC phải thực hiện xác thực lại mật khẩu local trong 5 phút gần nhất (`ReAuthMinutes <= 5m`). CẤM tự động chạy lại vô hạn lần khi chưa làm rõ nguyên nhân lỗi.
- **Lưu vết Sổ Kiểm toán Phục hồi Job M11 (`Job Recovery Audit Trail`)**: $100\%$ các thao tác kích hoạt chạy lại thủ công, hủy job đang chạy hoặc thay đổi cấu hình đợt chạy được ghi vết bất biến `ACT-M11-39-JOBRECOVER` trong Sổ Kiểm toán M11.

## 2. Ma trận Trạng thái Thực thi và Phục hồi Công việc (Execution Recovery Matrix)

| Trạng thái Thực thi (`ExecutionState`) | Điều kiện Kết thúc | Hành vi Lưu vết CSDL | Quyền Phục hồi Thủ công | Cảnh báo An ninh M11 | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `SUCCESS` | Hoàn tất không lỗi | Ghi `EndTimeUtc`, `RowsProcessed` | Không cần phục hồi | N/A (Bình thường) | `ACT-M11-39-SUCCESS` |
| `FAILED` | Bị ngắt do Exception | Ghi `ErrorMessage` & StackTrace | Cần Re-Auth $\le 5\text{m}$ để Retry | Alert `P3_WARN` | `ACT-M11-39-FAILED` |
| `STALLED_KILLED` | Vượt SLA 15 phút | Đánh dấu bị Kill tự động | Cần Re-Auth $\le 5\text{m}$ để Retry | **Alert `P2_HIGH`** | `ACT-M11-39-STALLED` |
| `MANUAL_RETRIGGER` | Admin gọi kích hoạt | Tạo `JobExecutionId` mới (Ref Old) | Yêu cầu Re-Auth $\le 5\text{m}$ | Alert `P4_INFO` | `ACT-M11-39-JOBRECOVER` |

## 3. Kiến trúc Luồng Theo dõi và Phục hồi Công việc Nền (Execution Engine Pipeline)

```
[Background Job Worker Starts Execution]
                   |
                   v
   [Create JobExecutionHistory Record (State: RUNNING)]
                   |
                   v
     [Execute Business Action Logic]
                   |
     +-------------+-------------+
     | (Success)                 | (Unhandled Exception / Timeout > 15m)
     v                           v
[Update State: SUCCESS]         [Update State: FAILED / STALLED_KILLED]
[Record RowsProcessed]          [Save ErrorMessage & StackTraceHash]
                                [Send Alert P2_HIGH if Stalled]
                                             |
                                             v
                              [Admin Requests Manual Recovery]
                                             |
                                             v
                              [Verify Password Re-Auth <= 5m Guard]
                                             |
                              [Trigger New Job Execution Record]
                              [Record Audit Log ACT-M11-39-JOBRECOVER]
```

## 4. Giao thức Thực thi Phục hồi Công việc CSDL (JobExecutionRecoveryService)

```csharp
public async Task<JobExecutionHistoryDto> TriggerManualJobRecoveryAsync(
    string jobId, 
    string failedExecutionId, 
    string adminUserId)
{
    // 1. Re-Auth Guard <= 5m
    var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
    if (adminUser == null || adminUser.LastReAuthenticatedAtUtc == null || 
        (DateTime.UtcNow - adminUser.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi kích hoạt chạy lại công việc nền.");
    }

    var failedExecution = await _db.JobExecutionHistories.FirstOrDefaultAsync(h => h.JobExecutionId == failedExecutionId);
    if (failedExecution == null) throw new KeyNotFoundException("EXECUTION_HISTORY_NOT_FOUND");

    // 2. Trigger New Execution Instance
    string newExecutionId = Guid.NewGuid().ToString("N");
    var newExecution = new JobExecutionHistory {
        JobExecutionId = newExecutionId,
        JobId = jobId,
        RetriggeredFromExecutionId = failedExecutionId,
        TriggeredByUserId = adminUserId,
        State = JobExecutionState.RUNNING,
        StartTimeUtc = DateTime.UtcNow
    };

    _db.JobExecutionHistories.Add(newExecution);
    await _db.SaveChangesAsync();

    // 3. Dispatch Background Job Trigger via Outbox M12-T037
    await _jobDispatcher.DispatchJobAsync(jobId, newExecutionId);

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-39-JOBRECOVER", adminUserId, new {
        JobId = jobId,
        FailedExecutionId = failedExecutionId,
        NewExecutionId = newExecutionId
    });

    return newExecution.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `ER-G01` | 100% các đợt chạy công việc nền BẮT BUỘC khởi tạo bản ghi `JobExecutionHistory` bất biến trong CSDL. |
| `ER-G02` | Lịch sử thực thi BẮT BUỘC lưu giữ trong 90 ngày (`RetentionDays = 90`) trước khi nén chuyển sang Cold Storage. |
| `ER-G03` | Kích hoạt chạy lại thủ công BẮT BUỘC xác thực lại mật khẩu admin trong 5 phút gần nhất (`ReAuthMinutes <= 5m`). |
| `ER-G04` | Đợt chạy lại thủ công BẮT BUỘC sinh ra `NewExecutionId` mới và giữ con trỏ tham chiếu `RetriggeredFromExecutionId`. |
| `ER-G05` | Công việc bị treo vượt mốc SLA 15m (`STALLED_KILLED`) tự động kích hoạt cảnh báo an ninh `P2_HIGH` (D-132). |
| `ER-G06` | 100% các thao tác phục hồi thủ công được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-39-JOBRECOVER`). |
| `ER-G07` | SLA thực thi API tra cứu lịch sử chạy job theo `JobId` từ Redis Cache $< 2\text{ms}$. |
| `ER-G08` | Phân quyền kích hoạt chạy lại thủ công công việc nền chỉ dành cho `SystemAdmin` và `DevOps`. |
| `ER-G09` | Hệ thống chịu tải lưu giữ 100,000 bản ghi lịch sử chạy job không làm chậm truy vấn CSDL. |
| `ER-G10` | 100% các test case tự kiểm ER39-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `ER39-01` | Worker `OrphanAssetCleanupWorker` hoàn tất chạy 50 tệp mồ côi | Lưu lịch sử `SUCCESS`, `RowsProcessed = 50`, `EndTimeUtc` chuẩn |
| `ER39-02` | Worker `LogIngestionShipperWorker` bị crash do ngoại lệ CSDL | Lưu lịch sử `FAILED`, lưu `ErrorMessage` và `StackTraceHash` |
| `ER39-03` | Admin xác thực lại 2 phút trước bấm nút "Retry Manual" cho job `FAILED` | Sinh `NewExecutionId` mới, khởi chạy job, 200 OK |
| `ER39-04` | Admin thử bấm nút "Retry Manual" khi lần xác thực cuối là 8 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `ER39-05` | Tra cứu vết Audit Log M11 sau khi phục hồi thủ công công việc ngầm | Ghi nhận Audit Event `ACT-M11-39-JOBRECOVER` |
| `ER39-06` | Worker `SessionCleanupWorker` bị treo quá 15 phút ($> 15\text{m}$) | Lưu lịch sử `STALLED_KILLED`, phát cảnh báo P2_HIGH |
| `ER39-07` | Tra cứu lịch sử đợt chạy job cách đây 85 ngày ($< 90$d) | Trả về DTO lịch sử đợt chạy đầy đủ |
| `ER39-08` | Tìm kiếm lịch sử đợt chạy job cách đây 95 ngày ($> 90$d) | Đã được nén di chuyển sang Cold Storage, không ở DB chính |
| `ER39-09` | Tải đồng thời 50 request tra cứu lịch sử job từ 50 kỹ sư DevOps | Processing latency p95 $< 12\text{ms}$ |
| `ER39-10` | Kiểm tra tính bất biến của bản ghi `JobExecutionHistory` | Trực tiếp sửa cột `State` trong DB bị ngắt bởi Trigger SQL |
| `ER39-11` | Thử kích hoạt chạy lại thủ công cho một `JobExecutionId` không tồn tại | Reject 404 `EXECUTION_HISTORY_NOT_FOUND` |
| `ER39-12` | Gửi request phục hồi job khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `ER39-13` | User không phải Admin/DevOps thử kích hoạt phục hồi job | Deny 403 Forbidden |
| `ER39-14` | User chưa đăng nhập gọi API tra cứu lịch sử chạy job | Deny 401 Unauthorized |
| `ER39-15` | Phục hồi thủ công một đợt chạy job thành công 2 lần liên tiếp | Sinh 2 `JobExecutionId` độc lập, lưu đủ vết |
| `ER39-16` | Kiểm tra thời gian cập nhật trạng thái `SUCCESS` sau khi job xong | Update SLA $< 2\text{ms}$ |
| `ER39-17` | Phân tích tham chiếu các bản ghi `JobExecutionHistories` trong CSDL | Quét schema `M11_JobExecutionHistories` (T020) |
| `ER39-18` | Dịch vụ Outbox M12-T037 bị gián đoạn khi phát lệnh Retry | Đổi trạng thái DB trước, retry phát lệnh qua Outbox queue |
| `ER39-19` | Tra cứu danh sách 20 đợt chạy thất bại gần nhất của hệ thống | Trả về DTO danh sách FAILED jobs kèm ErrorMessage |
| `ER39-20` | Kiểm thử hoàn tất luồng lịch sử và phục hồi job M11-BACKGROUND-JOB-EXECUTION-RECOVERY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-ER-I01` | M11 hiện tại chưa có `JobExecutionRecoveryService` quản lý lịch sử job | Risk không theo dõi được các đợt chạy worker bị lỗi | M11-T049 (Source task) |
| `M11-ER-I02` | Thiếu cờ Re-Auth Guard $\le 5\text{m}$ khi kích hoạt phục hồi thủ công | Kẻ xấu có thể mượn máy Admin tự ý kích hoạt lại job ngầm | M11-T049; REL-01 |
| `M11-ER-I03` | Thiếu luồng lưu trữ lịch sử 90 ngày theo quy định REL-07 | Dữ liệu lịch sử chạy worker bị xáo trộn hoặc xóa mất | M11-T049; REL-07 |
| `M11-ER-I04` | Thiếu cờ cảnh báo `P2_HIGH` khi job bị tiêu hủy tự động do treo $> 15\text{m}$ | Sự cố worker vướng lặp vô tận không được phát hiện | M11-ER-F04; M11-T037 |
| `M11-ER-I05` | Chưa kết nối sự kiện phục hồi job với Audit Log M11 (`ACT-M11-39-JOBRECOVER`) | Không ghi vết được người đã ấn nút trigger chạy lại worker | M11-T049; M11-T031 |

- `M11-ER-F01`: Triển khai `JobExecutionRecoveryService` với Immutable Execution Envelope (tiếp nhận: M11-T049).
- `M11-ER-F02`: Tích hợp Bắt buộc Re-Auth Guard $\le 5\text{m}$ & Retrigger Envelope (tiếp nhận: M11-T049; REL-01).
- `M11-ER-F03`: Triển khai 90-Day History Retention & Stalled Job P2 Alert (tiếp nhận: M11-T049; REL-07).
- `M11-ER-F04`: Thiết lập bộ kiểm thử tự động ER-G01–G10 và ER39-01–20 (tiếp nhận: M11 tasks).
- `M11-ER-F05`: Thu thập bằng chứng runtime cho luồng phục hồi job M11 (tiếp nhận: M11 tasks; A-G02/A-G06).

## 8. Tự kiểm M11-T039

- Đã thiết kế hoàn chỉnh `M11-BACKGROUND-JOB-EXECUTION-RECOVERY-1.0` với Ma trận Trạng thái Thực thi và Phục hồi Công việc.
- Đã chốt Ràng buộc Cấu trúc Dữ liệu Lịch sử Thực thi Bất biến (`Immutable Execution Envelope`).
- Đã chốt Ràng buộc Thời hạn Lưu giữ Lịch sử 90 Ngày REL-07 (`90-Day History Retention`).
- Đã lồng ghép Quy trình Phục hồi & Kích hoạt Chạy Thủ công (Re-Auth Guard $\le 5\text{m}$) và Audit Log M11 (`ACT-M11-39-JOBRECOVER`).
- Đã xác lập 10 Regression Gates (`ER-G01`–`ER-G10`) và 20 Test Cases tự kiểm (`ER39-01`–`ER39-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế lịch sử chạy và phục hồi công việc M11-T039 | WSA-7K2 |
