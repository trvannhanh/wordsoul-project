# Lập sổ đăng ký công việc nền M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-BACKGROUND-JOB-REGISTRY-1.0` |
| Task | M11-T038 |
| Đầu vào | M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), M12-FAIL-1.0 (D-025), REL-03, REL-07 |
| Phạm vi | Đặc tả Giao thức Sổ Đăng ký Công việc Nền và Khóa Phân tán (`Background Job Registry & Distributed Lock Protocol`), danh mục tập trung các Background Workers, cơ chế Redis Redlock/Lease Lock (TTL 30s) đảm bảo Single Leader Execution, giới hạn thời gian chạy SLA $\le 15$ phút và lưu vết kiểm toán M11 |
| Tự kiểm | A-G06; REL-03, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Sổ Đăng ký Công việc Nền và Khóa Phân tán (`Background Job Registry & Distributed Lock Protocol`) thuộc M11, chuẩn hóa danh mục quản lý toàn bộ các tiến trình xử lý ngầm định kỳ (Background Workers / Scheduled Jobs) trong hệ thống WordSoul, đảm bảo nguyên tắc duy nhất một Node thực thi job (Single Leader Execution) và ngăn ngừa xung đột dữ liệu trên môi trường phân tán Multi-Node (REL-03, REL-07).

- **Danh mục Đăng ký Công việc Nền Tập trung (`Centralized Job Catalog Invariant`)**: 100% các worker xử lý ngầm (`OrphanAssetCleanupWorker`, `LogIngestionShipperWorker`, `OutboxDispatcherWorker`, `SessionCleanupWorker`, `CapabilityHealthCheckWorker`, v.v.) BẮT BUỘC được khai báo và đăng ký thông số quản trị tại Sổ `BackgroundJobRegistry`.
- **Ràng buộc Khóa Phân tán Single Leader Execution (`Redis Redlock & Lease Invariant`)**: Trước khi khởi chạy một công việc định kỳ, Worker BẮT BUỘC phải chiếm giữ thành công Khóa Phân tán (Redis Redlock hoặc SQL Lease Lock) với thời gian vô hiệu hóa tự động (`LockTtl = 30s`). Nếu không lấy được khóa, Worker BẮT BUỘC bỏ qua đợt chạy này mà không gây lỗi (REL-03).
- **Ràng buộc Thời gian Thực thi Tối đa SLA $\le 15\text{m}$ (`Max Job Execution Duration SLA`)**: Thời gian thực thi cho một đợt công việc nền TUYỆT ĐỐI KHÔNG vượt quá 15 phút (`MaxJobDurationMinutes = 15m`). Công việc bị treo (Stalled Job) vượt quá 15 phút tự động bị hệ thống tiêu hủy (Kill) và phát cảnh báo `P2_HIGH` (D-132).
- **Lưu vết Sổ Kiểm toán Công việc Nền M11 (`Background Job Audit Trail`)**: $100\%$ các lần khởi tạo, chiếm khóa, hoàn tất hoặc thất bại của công việc nền được ghi vết bất biến `ACT-M11-38-JOB` trong Sổ Kiểm toán M11.

## 2. Ma trận Danh mục Công việc Nền Hệ thống (Background Job Catalog Matrix)

| Mã Công việc (`JobId`) | Tên Worker (`WorkerName`) | Chu kỳ Chạy (`Schedule Cron`) | Khóa Phân tán (`Lock Key`) | Thời gian Khóa (`LockTTL`) | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `JOB_ORPHAN_CLEANUP` | `OrphanAssetCleanupWorker` | Daily `0 2 * * *` (02:00 UTC) | `lock:job:orphan_cleanup` | **30 giây (Renewal)** | `ACT-M11-38-ORPHAN` |
| `JOB_LOG_SHIPPER` | `LogIngestionShipperWorker` | Continuous (5s Delay) | `lock:job:log_shipper` | 30 giây | `ACT-M11-38-LOGSHIP` |
| `JOB_OUTBOX_DISPATCH` | `OutboxDispatcherWorker` | Continuous (2s Delay) | `lock:job:outbox_dispatch` | 30 giây | `ACT-M11-38-OUTBOX` |
| `JOB_SESSION_CLEANUP` | `SessionCleanupWorker` | Hourly `0 * * * *` | `lock:job:session_cleanup` | 30 giây | `ACT-M11-38-SESSION` |
| `JOB_HEALTH_CHECK` | `CapabilityHealthCheckWorker` | Every 5s `*/5 * * * * *` | `lock:job:health_check` | 10 giây | `ACT-M11-38-HEALTH` |

## 3. Kiến trúc Luồng Khóa Phân tán và Quản lý Job (Job Registry Engine)

```
[Cron Scheduler / Worker Loop Triggers Job Execution]
                          |
                          v
         [Try Acquire Redis Redlock (LockTTL = 30s)]
                          |
         +----------------+----------------+
         | (Lock Acquisition Failed)       | (Lock Acquired - Single Leader)
         v                                 v
[Skip Execution - Node Standby]   [Start Heartbeat Lock Renewal Task (Every 10s)]
                                  [Execute Business Job Logic (Max SLA <= 15m)]
                                                 |
                                  +--------------+--------------+
                                  | (Completed <= 15m)          | (Stalled > 15m)
                                  v                             v
                         [Release Redlock]            [Kill Stalled Job]
                         [Update Job State: OK]       [Trigger Alert P2_HIGH]
                         [Record Audit ACT-M11-38]   [Record Audit ACT-M11-38-STALLED]
```

## 4. Giao thức Thực thi Quản lý Khóa Phân tán CSDL (BackgroundJobRegistryService)

```csharp
public async Task ExecuteRegisteredJobAsync(string jobId, Func<CancellationToken, Task> jobAction)
{
    var jobMeta = await _db.BackgroundJobMetadatas.FirstOrDefaultAsync(j => j.JobId == jobId);
    if (jobMeta == null || !jobMeta.IsEnabled) return;

    string lockKey = $"wordsoul:lock:job:{jobId}";
    string lockValue = Guid.NewGuid().ToString("N");

    // 1. Acquire Redis Redlock with LockTTL = 30s
    bool isLockAcquired = await _redisDb.StringSetAsync(lockKey, lockValue, TimeSpan.FromSeconds(30), When.NotExists);
    if (!isLockAcquired)
    {
        Log.Information("Job {JobId} skipped execution on this node (Lock held by another node)", jobId);
        return; // Single Leader Execution Invariant
    }

    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15)); // Max SLA 15m
    using var heartbeatCts = new CancellationTokenSource();

    try
    {
        // 2. Start Lock Renewal Heartbeat Task (Every 10s)
        var heartbeatTask = Task.Run(async () => {
            while (!heartbeatCts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), heartbeatCts.Token);
                await _redisDb.KeyExpireAsync(lockKey, TimeSpan.FromSeconds(30));
            }
        });

        // 3. Execute Job Action
        await jobAction(cts.Token);

        heartbeatCts.Cancel();
        
        // 4. Record Audit Event M11
        await _auditLog.RecordEventAsync("ACT-M11-38-JOB", "JOB_SYSTEM", new {
            JobId = jobId,
            Status = "SUCCESS",
            NodeId = Environment.MachineName
        });
    }
    catch (OperationCanceledException)
    {
        await _auditLog.RecordEventAsync("ACT-M11-38-STALLED", "JOB_SYSTEM", new {
            JobId = jobId,
            Status = "KILLED_STALLED_EXCEEDED_15M"
        });
        throw new TimeoutException($"JOB_STALLED_TIMEOUT: Job {jobId} bị tiêu hủy do vượt quá 15 phút.");
    }
    finally
    {
        // Release Lock atomically if value matches
        var script = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await _redisDb.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `BJ-G01` | 100% công việc nền định kỳ BẮT BUỘC đăng ký tại Sổ `BackgroundJobRegistry` và khởi tạo qua Khóa Phân tán. |
| `BJ-G02` | Khóa Phân tán BẮT BUỘC đảm bảo duy nhất một Node thực thi công việc (`Single Leader Execution`). |
| `BJ-G03` | Thời gian sống mặc định của khóa đệm Redlock là 30 giây (`LockTTL = 30s`) kèm cơ chế gia hạn Heartbeat 10s. |
| `BJ-G04` | Thời gian thực thi một công việc nền TUYỆT ĐỐI KHÔNG vượt quá 15 phút (`MaxJobDurationMinutes = 15m`). |
| `BJ-G05` | Công việc bị treo vượt mốc 15 phút phải bị Kill tự động và kích hoạt cảnh báo an toàn `P2_HIGH` (D-132). |
| `BJ-G06` | 100% các đợt khởi chạy, chiếm khóa hoặc hoàn tất công việc được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-38-JOB`). |
| `BJ-G07` | SLA thực thi kiểm tra chiếm giữ Khóa Phân tán Redis Redlock $< 2\text{ms}$. |
| `BJ-G08` | Phân quyền bật/tắt cờ `IsEnabled` của các công việc nền chỉ dành cho `SystemAdmin` và `DevOps`. |
| `BJ-G09` | Hệ thống hỗ trợ quản lý danh mục đến 50 công việc nền đồng thời mà không bị xung đột khóa. |
| `BJ-G10` | 100% các test case tự kiểm BJ38-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `BJ38-01` | Node 1 và Node 2 cùng kích hoạt `OrphanAssetCleanupWorker` lúc 02:00 UTC | Node 1 chiếm khóa chạy job, Node 2 thấy lock thì bỏ qua (Single Leader) |
| `BJ38-02` | Khóa Phân tán được gia hạn thành công mỗi 10s trong suốt đợt chạy 40s | Khóa không bị nhả giữa chừng, job hoàn tất 200 OK |
| `BJ38-03` | Công việc ngầm bị treo quá 15 phút do vướng vòng lặp vô tận | CancellationToken kích hoạt Kill job SLA $< 1\text{s}$, phát cảnh báo P2 |
| `BJ38-04` | Node 1 bị sập nguồn điện đột ngột khi đang nắm giữ Khóa Phân tán | Khóa tự động vô hiệu sau 30s (`LockTTL = 30s`), Node 2 tiếp quản đợt sau |
| `BJ38-05` | SystemAdmin phát lệnh vô hiệu hóa cờ `IsEnabled` cho job SessionCleanup | Worker tự động dừng các đợt chạy định kỳ tiếp theo |
| `BJ38-06` | Tra cứu vết Audit Log M11 sau khi công việc ngầm hoàn tất thành công | Ghi nhận Audit Event `ACT-M11-38-JOB` đính kèm NodeId |
| `BJ38-07` | Thực hiện giải phóng khóa phân tán Redlock sau khi job hoàn thành | Khóa bị xóa khỏi Redis SLA $< 1\text{ms}$ |
| `BJ38-08` | Thử giải phóng khóa phân tán của Node khác đang nắm giữ | Kịch bản Lua script từ chối xóa khóa, trả về 0 |
| `BJ38-09` | Tải đồng thời 100 yêu cầu chiếm khóa phân tán từ 100 worker threads | Lock acquisition latency p95 $< 1.8\text{ms}$ |
| `BJ38-10` | Kiểm tra thời gian vô hiệu khóa phân tán khi job kết thúc sớm | Deletion SLA $< 1\text{ms}$ |
| `BJ38-11` | Thử đăng ký một Job mới với `JobId` trùng lặp | Reject 400 `JOB_ID_ALREADY_EXISTS` |
| `BJ38-12` | Gửi request cấu hình sổ công việc ngầm khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `BJ38-13` | User không phải SystemAdmin thử gọi API kích hoạt thủ công Job | Deny 403 Forbidden |
| `BJ38-14` | User chưa đăng nhập gọi API tra cứu danh mục công việc ngầm M11 | Deny 401 Unauthorized |
| `BJ38-15` | Khởi chạy công việc ngầm có gắn cờ `IsExclusive = true` | Tự động tạm dừng các job không ưu tiên trong ca chạy |
| `BJ38-16` | Kiểm tra độ trễ nhả khóa phân tán Redlock khi Redis bị chập chờn | Fallback giải phóng qua SQL Lease Lock |
| `BJ38-17` | Phân tích tham chiếu các bản ghi `BackgroundJobMetadatas` trong CSDL | Quét schema `M11_BackgroundJobs` (T020) |
| `BJ38-18` | Tiến trình Heartbeat gia hạn khóa bị mất kết nối Redis | Worker hủy chạy job khẩn cấp ngắt luồng |
| `BJ38-19` | Tra cứu danh sách các công việc nền đang ở trạng thái `RUNNING` | Trả về DTO danh sách Jobs kèm NodeId nắm khóa |
| `BJ38-20` | Kiểm thử hoàn tất luồng lập sổ công việc nền M11-BACKGROUND-JOB-REGISTRY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-BJ-I01` | M11 hiện tại chưa có `BackgroundJobRegistryService` quản lý khóa | Risk chạy trùng lặp worker trên nhiều nodes gây hỏng data | M11-T049 (Source task) |
| `M11-BJ-I02` | Thiếu cờ Redis Redlock / Lease Lock Single Leader Execution | Xảy ra đua dữ liệu khi scale API Server lên 5-10 nodes | M11-T049; REL-03 |
| `M11-BJ-I03` | Thiếu cờ khống chế thời gian chạy tối đa Max SLA 15m | Job bị treo làm nghẽn tài nguyên CPU/Memory | M11-T049; REL-07 |
| `M11-BJ-I04` | Thiếu tiến trình Heartbeat gia hạn khóa phân tán an toàn | Khóa bị nhả sớm khi đợt chạy job kéo dài quá 30s | M11-BJ-F04; M12-T040 |
| `M11-BJ-I05` | Chưa kết nối sự kiện hoàn tất/treo job với Audit Log M11 (`ACT-M11-38-JOB`) | Không ghi vết được lịch sử chạy và lỗi của các worker | M11-T049; M11-T031 |

- `M11-BJ-F01`: Triển khai `BackgroundJobRegistryService` với Centralized Catalog (tiếp nhận: M11-T049).
- `M11-BJ-F02`: Tích hợp Bắt buộc Redis Redlock Single Leader Execution (tiếp nhận: M11-T049; REL-03).
- `M11-BJ-F03`: Triển khai Max SLA 15m Duration Guard & Heartbeat Renewal (tiếp nhận: M11-T049; REL-07).
- `M11-BJ-F04`: Thiết lập bộ kiểm thử tự động BJ-G01–G10 và BJ38-01–20 (tiếp nhận: M11 tasks).
- `M11-BJ-F05`: Thu thập bằng chứng runtime cho luồng sổ công việc nền M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T038

- Đã thiết kế hoàn chỉnh `M11-BACKGROUND-JOB-REGISTRY-1.0` với Ma trận Danh mục Công việc Nền Hệ thống.
- Đã chốt Ràng buộc Danh mục Đăng ký Công việc Nền Tập trung (`Centralized Job Catalog`).
- Đã chốt Ràng buộc Khóa Phân tán Single Leader Execution (`Redis Redlock & Lease Invariant`).
- Đã lồng ghép Ràng buộc Thời gian Thực thi Tối đa SLA $\le 15$ phút, Heartbeat Renewal 10s và Audit Log M11 (`ACT-M11-38-JOB`).
- Đã xác lập 10 Regression Gates (`BJ-G01`–`BJ-G10`) và 20 Test Cases tự kiểm (`BJ38-01`–`BJ38-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả lập sổ đăng ký công việc nền M11-T038 | WSA-7K2 |
