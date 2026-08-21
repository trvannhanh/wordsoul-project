# Chốt mục tiêu phục hồi và diễn tập M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-DISASTER-RECOVERY-DRILL-TARGETS-1.0` |
| Task | M11-T048 |
| Đầu vào | M11-CRITICAL-INCIDENT-PLAYBOOK-1.0 (D-141), M11-INCIDENT-SEVERITY-MODEL-1.0 (D-140), REL-02, REL-03 |
| Phạm vi | Đặc tả Giao thức Chốt Mục tiêu Phục hồi và Diễn tập Thảm họa (`Disaster Recovery Targets & GameDay Drill Protocol`), cam kết chỉ số RTO $\le 1$ giờ và RPO $\le 5$ phút, quy trình diễn tập thảm họa GameDay định kỳ hàng quý và lưu vết kiểm toán M11 |
| Tự kiểm | A-G06; REL-02, REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chốt Mục tiêu Phục hồi và Diễn tập Thảm họa (`Disaster Recovery Targets & GameDay Drill Protocol`) thuộc M11, xác lập các chỉ số cam kết khôi phục thảm họa (RTO - Recovery Time Objective và RPO - Recovery Point Objective) cho toàn bộ hệ thống WordSoul, đồng thời ban hành quy chế diễn tập giả lập sự cố GameDay định kỳ hàng quý nhằm kiểm thử khả năng chịu lỗi thực tế của đội ngũ vận hành (REL-02, REL-03).

- **Chỉ số Cam kết Phục hồi Thảm họa System RTO & RPO (`System RTO & RPO Invariant`)**:
  - `Recovery Time Objective (RTO)`: Tổng thời gian tối đa để hệ thống khôi phục trạng thái hoạt động bình thường sau sự cố thảm họa `SEV-1` BẮT BUỘC $\le 1$ giờ (`MaxRtoHours = 1h`).
  - `Recovery Point Objective (RPO)`: Dung lượng dữ liệu mất mát tối đa cho phép BẮT BUỘC $\le 5$ phút (`MaxRpoMinutes = 5m`). Đạt được nhờ tiến trình lưu trữ nhật ký ghi trước CSDL (Continuous WAL Archiving) và sao lưu PITR snapshot mỗi 5 phút.
- **Quy chế Diễn tập Giả lập Sự cố GameDay Hàng Quý (`Quarterly GameDay Chaos Drill Invariant`)**: 100% các thành phần hạ tầng cốt lõi (Database Cluster, Redis Sentinel, S3 Region Storage, API Gateway Nodes) BẮT BUỘC trải qua buổi diễn tập thảm họa GameDay định kỳ 3 tháng/lần (`DrillFrequencyMonths = 3m`). CẤM bỏ qua lịch diễn tập.
- **Ràng buộc Môi trường Diễn tập Giả lập An toàn (`Safe Drill Sandbox Guard`)**: Các đợt diễn tập GameDay BẮT BUỘC thực hiện trên môi trường Staging/Sandbox được gắn cờ cô lập (`IsDrillEnvironment = true`). TUYỆT ĐỐI CẤM chạy kịch bản làm hỏng dữ liệu trên CSDL Production thực tế.
- **Lưu vết Sổ Kiểm toán Diễn tập M11 (`Disaster Drill Audit Trail`)**: $100\%$ các cuộc diễn tập GameDay, bao gồm các chỉ số RTO/RPO đo lường thực tế, được ghi vết bất biến `ACT-M11-48-DRILL` trong Sổ Kiểm toán M11.

## 2. Ma trận Mục tiêu Phục hồi và Lịch Diễn tập (DR Targets & Drill Matrix)

| Cấp độ Sự cố / Thành phần | Cam kết RTO (`Max RTO`) | Cam kết RPO (`Max RPO`) | Tần suất Diễn tập GameDay | Tiêu chuẩn Đánh giá Đạt | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`SQL_DATABASE_CLUSTER`** | **$\le 1$ Giờ** | **$\le 5$ Phút** | **Hàng Quý (3 Tháng)** | Multi-Region Failover $\le 10\text{m}$ | `ACT-M11-48-DBDRILL` |
| `REDIS_CACHE_SENTINEL` | $\le 15$ Phút | $\le 1$ Phút | Hàng Quý | Sentinel Auto-Failover $\le 30\text{s}$ | `ACT-M11-48-REDISDRILL` |
| `S3_MEDIA_STORAGE` | $\le 30$ Phút | $\le 0$ Phút (Sync) | Hàng Quý | Cross-Region Replication OK | `ACT-M11-48-S3DRILL` |
| `API_GATEWAY_NODES` | $\le 5$ Phút | $\le 0$ Phút (Stateless) | Hàng Tháng | Auto-Scaling Re-deploy $\le 3\text{m}$ | `ACT-M11-48-GW48` |

## 3. Kiến trúc Luồng Diễn tập Giả lập Sự cố GameDay M11 (Drill Engine Pipeline)

```
[Schedule Quarterly GameDay Chaos Drill (Q1/Q2/Q3/Q4)]
                          |
                          v
     [Verify Safe Drill Sandbox Guard (IsDrillEnvironment = true)]
                          |
                          v
     [Inject Simulated Disaster Fault (e.g. Primary DB Node Down)]
                          |
                          v
     [Start Timer: Measure Actual RTO & Actual RPO]
                          |
     +--------------------+--------------------+
     | (Actual RTO <= 1h & RPO <= 5m)          | (RTO > 1h or RPO > 5m)
     v                                         v
[Mark Drill Status: PASSED]             [Mark Drill Status: FAILED]
[Generate GameDay Summary Report]       [Generate Action Item Tickets]
[Record Audit Log ACT-M11-48-DRILL]     [Record Audit Log ACT-M11-48-FAIL]
```

## 4. Giao thức Thực thi Quản lý Diễn tập CSDL (DisasterRecoveryDrillService)

```csharp
public async Task<DrillResultDto> ExecuteGameDayDrillAsync(string drillId, string engineerUserId)
{
    // 1. Verify Safe Sandbox Guard
    if (!_env.IsStaging() && !_env.IsEnvironment("Sandbox"))
    {
        throw new InvalidOperationException("DRILL_ENVIRONMENT_RESTRICTION: Các cuộc diễn tập GameDay tuyệt đối CẤM chạy trực tiếp trên CSDL Production.");
    }

    var drill = await _db.GameDayDrillSchedules.FirstOrDefaultAsync(d => d.DrillId == drillId);
    if (drill == null) throw new KeyNotFoundException("DRILL_NOT_FOUND");

    var result = new DrillResultDto { DrillId = drillId, StartedAtUtc = DateTime.UtcNow };
    var sw = Stopwatch.StartNew();

    // 2. Simulate Fault Injection (Primary DB Failover Test)
    await _chaosInjector.InjectFaultAsync(drill.TargetComponent, FaultType.NODE_OUTAGE);

    // 3. Trigger Auto Recovery Procedure M11-T046
    bool isRecovered = await _recoveryService.TriggerAutoFailoverAsync(drill.TargetComponent);
    sw.Stop();

    double actualRtoMinutes = sw.Elapsed.TotalMinutes;
    double actualRpoMinutes = await _recoveryService.CalculateActualRpoMinutesAsync(drill.TargetComponent);

    // 4. Evaluate RTO/RPO Compliance
    bool isSuccess = actualRtoMinutes <= 60.0 && actualRpoMinutes <= 5.0; // RTO <= 1h, RPO <= 5m
    drill.Status = isSuccess ? DrillStatus.PASSED : DrillStatus.FAILED;
    drill.ActualRtoMinutes = actualRtoMinutes;
    drill.ActualRpoMinutes = actualRpoMinutes;
    await _db.SaveChangesAsync();

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-48-DRILL", engineerUserId, new {
        DrillId = drillId,
        Status = drill.Status.ToString(),
        ActualRtoMinutes = actualRtoMinutes,
        ActualRpoMinutes = actualRpoMinutes
    });

    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DR-G01` | Chỉ số cam kết RTO cho sự cố thảm họa `SEV-1` BẮT BUỘC đạt $\le 1$ giờ (`MaxRtoHours = 1h`). |
| `DR-G02` | Chỉ số cam kết RPO cho dữ liệu CSDL BẮT BUỘC đạt $\le 5$ phút (`MaxRpoMinutes = 5m`). |
| `DR-G03` | Diễn tập thảm họa GameDay BẮT BUỘC tổ chức định kỳ 3 tháng/lần cho toàn bộ các thành phần hạ tầng cốt lõi. |
| `DR-G04` | Các đợt diễn tập GameDay TUYỆT ĐỐI CẤM thực hiện trực tiếp trên môi trường CSDL Production thực tế. |
| `DR-G05` | 100% các cuộc diễn tập GameDay không đạt chỉ số RTO/RPO BẮT BUỘC sinh Ticket khắc phục hạ tầng khẩn cấp. |
| `DR-G06` | 100% các cuộc diễn tập GameDay được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-48-DRILL`). |
| `DR-G07` | SLA thực thi tính toán đo lường chỉ số RTO/RPO thực tế sau khi khôi phục $< 1.0\text{s}$. |
| `DR-G08` | Phân quyền phê duyệt kịch bản diễn tập GameDay chỉ dành cho `ChaosEngineer` và `DevOpsLead`. |
| `DR-G09` | Khả năng khôi phục sau diễn tập đảm bảo đưa môi trường Staging/Sandbox về trạng thái nguyên vẹn 100%. |
| `DR-G10` | 100% các test case tự kiểm DR48-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DR48-01` | Diễn tập sập Primary DB Node trên Staging, khôi phục xong sau 25 phút | Đánh dấu `PASSED` (Đạt RTO 25m $< 60\text{m}$, RPO 2m $< 5\text{m}$) |
| `DR48-02` | Diễn tập sập Redis Sentinel, khôi phục xong sau 45 giây | Đánh dấu `PASSED` (Đạt RTO 45s $< 15\text{m}$, RPO 0s) |
| `DR48-03` | Diễn tập thảm họa S3 Storage, mất 1h 15m mới khôi phục xong ($> 1\text{h}$) | Đánh dấu `FAILED` (Vi phạm RTO $> 60\text{m}$), phát sinh Jira Ticket |
| `DR48-04` | Thử kích hoạt kịch bản diễn tập GameDay trên môi trường Production | Reject 400 `DRILL_ENVIRONMENT_RESTRICTION` |
| `DR48-05` | ChaosEngineer xác thực lại 2 phút trước bấm nút "Khởi chạy GameDay Drill" | Thực thi diễn tập thành công trên Staging Sandbox |
| `DR48-06` | Tra cứu vết Audit Log M11 sau khi hoàn tất buổi diễn tập GameDay | Ghi nhận Audit Event `ACT-M11-48-DRILL` |
| `DR48-07` | ChaosEngineer thử chạy diễn tập khi lần Re-Auth cuối là 8 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `DR48-08` | Tra cứu báo cáo kết quả diễn tập GameDay Q1 | Trả về DTO báo cáo RTO/RPO thực tế |
| `DR48-09` | Tải đồng thời 100 request tra cứu lịch sử các đợt diễn tập | Query SLA $< 12\text{ms}$ |
| `DR48-10` | Kiểm tra thời gian vô hiệu hóa các injector giả lập sự cố sau GameDay | Cleanup SLA $< 5\text{s}$ |
| `DR48-11` | Thử nạp mã `TargetComponent` không thuộc danh mục quản lý | Reject 400 `INVALID_TARGET_COMPONENT` |
| `DR48-12` | Gửi request khởi chạy diễn tập khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `DR48-13` | User không phải ChaosEngineer/DevOpsLead thử khởi chạy diễn tập | Deny 403 Forbidden |
| `DR48-14` | User chưa đăng nhập gọi API tra cứu kết quả diễn tập GameDay M11 | Deny 401 Unauthorized |
| `DR48-15` | Diễn tập sập API Gateway Nodes, auto-scaling khôi phục sau 2 phút | Đánh dấu `PASSED` (Đạt RTO 2m $< 5\text{m}$) |
| `DR48-16` | Kiểm tra độ trễ đồng bộ kết quả GameDay sang CSDL | Sync SLA $< 150\text{ms}$ |
| `DR48-17` | Phân tích tham chiếu các bản ghi `GameDayDrillSchedules` trong CSDL | Quét schema `M11_GameDayDrills` (T020) |
| `DR48-18` | Công cụ fault injector bị vướng lỗi ngắt luồng giữa chừng | Abort đợt diễn tập, rollback môi trường Staging về nguyên vẹn |
| `DR48-19` | Tra cứu danh sách các buổi diễn tập GameDay dự kiến trong năm | Trả về DTO danh sách ScheduledDrills |
| `DR48-20` | Kiểm thử hoàn tất luồng mục tiêu phục hồi và diễn tập M11-DISASTER-RECOVERY-DRILL-TARGETS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-DR-I01` | M11 hiện tại chưa có `DisasterRecoveryDrillService` quản lý GameDay | Risk chưa kiểm chứng được khả năng chịu lỗi thực tế của DB | M11-T049 (Source task) |
| `M11-DR-I02` | Thiếu cờ cam kết RTO $\le 1$h & RPO $\le 5$m khi xảy ra thảm họa SEV-1 | Không có thước đo chuẩn cho công tác khôi phục hạ tầng | M11-T049; REL-02 |
| `M11-DR-I03` | Thiếu cờ Chặn Diễn tập trên Production (`IsDrillEnvironment = true`) | Risk tự gây ra sự cố thảm họa thật trên CSDL Production | M11-T049; REL-03 |
| `M11-DR-I04` | Thiếu luồng tự động tạo Ticket khắc phục khi GameDay bị FAILED | Không theo dõi triệt để các lỗ hổng phát hiện qua diễn tập | M11-DR-F04; M11-T046 |
| `M11-DR-I05` | Chưa kết nối sự kiện diễn tập GameDay với Audit Log M11 (`ACT-M11-48-DRILL`) | Không ghi vết được lịch sử diễn tập thảm họa hàng quý | M11-T049; M11-T031 |

- `M11-DR-F01`: Triển khai `DisasterRecoveryDrillService` với System RTO $\le 1$h & RPO $\le 5$m (tiếp nhận: M11-T049).
- `M11-DR-F02`: Tích hợp Bắt buộc Safe Drill Sandbox Guard & Quarterly GameDay Drills (tiếp nhận: M11-T049; REL-03).
- `M11-DR-F03`: Triển khai Auto Failed Ticket Generation & Chaos Fault Injection (tiếp nhận: M11-T049; M11-T046).
- `M11-DR-F04`: Thiết lập bộ kiểm thử tự động DR-G01–G10 và DR48-01–20 (tiếp nhận: M11 tasks).
- `M11-DR-F05`: Thu thập bằng chứng runtime cho luồng diễn tập M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T048

- Đã thiết kế hoàn chỉnh `M11-DISASTER-RECOVERY-DRILL-TARGETS-1.0` với Ma trận Mục tiêu Phục hồi và Lịch Diễn tập.
- Đã chốt Ràng buộc Chỉ số Cam kết Phục hồi Thảm họa System RTO $\le 1$h & RPO $\le 5$m.
- Đã chốt Ràng buộc Quy chế Diễn tập Giả lập Sự cố GameDay Hàng Quý (`Quarterly GameDay Chaos Drill`).
- Đã lồng ghép Ràng buộc Môi trường Diễn tập Giả lập An toàn (`Safe Drill Sandbox Guard`), Tự động phát Ticket khi FAILED và Audit Log M11 (`ACT-M11-48-DRILL`).
- Đã xác lập 10 Regression Gates (`DR-G01`–`DR-G10`) và 20 Test Cases tự kiểm (`DR48-01`–`DR48-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt mục tiêu phục hồi và diễn tập M11-T048 | WSA-7K2 |
