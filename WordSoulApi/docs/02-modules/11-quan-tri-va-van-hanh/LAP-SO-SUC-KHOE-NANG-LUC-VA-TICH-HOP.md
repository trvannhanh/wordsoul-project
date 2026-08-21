# Lập sổ sức khỏe năng lực và tích hợp M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0` |
| Task | M11-T036 |
| Đầu vào | M11-PLATFORM-HEALTH-1.0 (D-053), M12-CAPABILITY-1.0 (D-020), M12-FAIL-1.0 (D-025), REL-03 |
| Phạm vi | Đặc tả Giao thức Sổ Đăng ký Sức khỏe Năng lực và Tích hợp (`Capability & Integration Health Registry Protocol`), theo dõi 4 trạng thái sức khỏe của 15 năng lực tích hợp M12, cơ chế tính toán Sliding Window Error Rate 60s, tự động kích hoạt Circuit Breaker SLA $\le 2\text{ms}$ và lưu vết kiểm toán |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Sổ Đăng ký Sức khỏe Năng lực và Tích hợp (`Capability & Integration Health Registry Protocol`) thuộc M11, thiết lập trung tâm giám sát sức khỏe thời gian thực cho toàn bộ 15 năng lực tích hợp M12 (Google OAuth, Apple SIWA, Facebook, AI Gemini, Cloud Storage, SendGrid Email, FCM Push, v.v.), đảm bảo khả năng ngắt mạch tự động (Circuit Breaker) khi dịch vụ đối tác suy giảm (REL-03).

- **Máy Trạng thái Sức khỏe 4 Cấp độ (`4-State Capability Health Envelope Invariant`)**: Mỗi năng lực tích hợp BẮT BUỘC duy trì 1 trong 4 trạng thái sức khỏe thời gian thực: `HEALTHY` (Bình thường), `DEGRADED` (Tỷ lệ lỗi $10\% \dots 49\%$), `UNHEALTHY` (Tỷ lệ lỗi $\ge 50\%$), và `CIRCUIT_OPEN` (Đã ngắt mạch, chuyển sang Fail-Closed mode D-025).
- **Cơ chế Cửa sổ Trượt Tính Toán Lỗi 60s (`60-Second Sliding Window Aggregation`)**: Sổ sức khỏe thu thập chỉ số đếm thành công/thất bại và độ trễ latency p99 qua cửa sổ trượt 60 giây (`WindowDuration = 60s`). Chỉ số tự động cập nhật mỗi 5 giây vào Redis Cluster.
- **Ràng buộc Kích hoạt Ngắt mạch SLA $\le 2\text{ms}$ (`Circuit Breaker Auto-Tripping SLA`)**: Khi tỷ lệ lỗi $> 50\%$ hoặc độ trễ p99 $> 3000\text{ms}$ kéo dài qua 30s trong cửa sổ 60s, hệ thống TỰ ĐỘNG chuyển trạng thái sang `CIRCUIT_OPEN` trong SLA $\le 2\text{ms}$. Toàn bộ các cuộc gọi tới năng lực bị ngắt mạch lập tức trả về lỗi nhanh mà không chờ timeout (REL-03).
- **Lưu vết Sổ Kiểm toán Sức khỏe Năng lực M11 (`Health Registry Audit Trail`)**: $100\%$ các sự kiện chuyển đổi trạng thái sức khỏe năng lực (ví dụ: `HEALTHY` $\to$ `CIRCUIT_OPEN`) được ghi vết bất biến `ACT-M11-36-HEALTH` trong Sổ Kiểm toán M11.

## 2. Ma trận Trạng thái Sức khỏe Năng lực Tích hợp (Capability Health Matrix)

| Trạng thái Sức khỏe (`HealthState`) | Điều kiện Tỷ lệ Lỗi (`ErrorRate`) | Điều kiện Latency p99 | Hành vi Hệ thống (`System Behavior`) | Mode Xử lý M12 (D-025) | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `HEALTHY` | ErrorRate $< 10\%$ | p99 $< 800\text{ms}$ | Phân phối cuộc gọi bình thường | Normal Operation | N/A (Trạng thái chuẩn) |
| `DEGRADED` | $10\% \le \text{ErrorRate} < 50\%$ | $800\text{ms} \le \text{p99} < 3000\text{ms}$ | Gửi cảnh báo Warning WARN-01 | Graceful Degradation | `ACT-M11-36-DEGRADED` |
| `UNHEALTHY` | $\text{ErrorRate} \ge 50\%$ (15s) | $\text{p99} \ge 3000\text{ms}$ | Chuẩn bị ngắt mạch (Pre-Trip) | Fallback / Circuit Ready | `ACT-M11-36-UNHEALTHY` |
| `CIRCUIT_OPEN` | Auto-Tripped (30s) | Hard Fail Triggered | **Fast-Fail Fast Return SLA $\le 2\text{ms}$** | **Fail-Closed (Strict)** | `ACT-M11-36-CIRCUIT` |

## 3. Kiến trúc Luồng Giám sát Sức khỏe Năng lực M11 (Health Registry Engine)

```
[M12 Capability Call Executed (e.g. AI Gemini / Google OAuth)]
                                 |
                                 v
    [Record Success/Failure + Latency in Redis 60s Sliding Window]
                                 |
                                 v
            [CapabilityHealthRegistryWorker (Every 5s)]
                                 |
         +-----------------------+-----------------------+
         | (ErrorRate < 50%)                             | (ErrorRate >= 50% / p99 > 3s)
         v                                               v
[State: HEALTHY / DEGRADED]                     [Auto-Trip Circuit Breaker SLA <= 2ms]
- Continue Routing Calls                        - Set State: CIRCUIT_OPEN
                                                - Route to Fail-Closed Mode (D-025)
                                                - Record Audit Log ACT-M11-36-CIRCUIT
```

## 4. Giao thức Thực thi Sổ Đăng ký Sức khỏe CSDL (CapabilityHealthRegistryService)

```csharp
public async Task UpdateCapabilityTelemetryAsync(string capabilityId, bool isSuccess, long latencyMs)
{
    string redisKey = $"wordsoul:health:{capabilityId}:window";
    long nowTicks = DateTime.UtcNow.Ticks;

    // 1. Add Telemetry Entry to Redis ZSET Sliding Window (60s)
    await _redisDb.SortedSetAddAsync(redisKey, $"{nowTicks}:{(isSuccess ? 1 : 0)}:{latencyMs}", nowTicks);
    await _redisDb.SortedSetRemoveRangeByScoreAsync(redisKey, 0, nowTicks - TimeSpan.FromSeconds(60).Ticks);

    // 2. Compute Window Error Rate & p99 Latency
    var entries = await _redisDb.SortedSetRangeByRankAsync(redisKey);
    int totalCount = entries.Length;
    if (totalCount < 10) return; // Min sample size threshold

    int failCount = entries.Count(e => e.ToString().Split(':')[1] == "0");
    double errorRate = (double)failCount / totalCount;

    var latencies = entries.Select(e => long.Parse(e.ToString().Split(':')[2])).OrderBy(l => l).ToList();
    long p99Latency = latencies[(int)(totalCount * 0.99)];

    // 3. Evaluate Circuit Breaker Tripping SLA <= 2ms
    if (errorRate >= 0.50 || p99Latency >= 3000)
    {
        await TripCircuitBreakerAsync(capabilityId, errorRate, p99Latency);
    }
}

private async Task TripCircuitBreakerAsync(string capabilityId, double errorRate, long p99Latency)
{
    string statusKey = $"wordsoul:health:{capabilityId}:state";
    string currentStatus = await _redisDb.StringGetAsync(statusKey);

    if (currentStatus != "CIRCUIT_OPEN")
    {
        await _redisDb.StringSetAsync(statusKey, "CIRCUIT_OPEN", TimeSpan.FromMinutes(5)); // Half-Open after 5m

        // 4. Record Audit Event M11
        await _auditLog.RecordEventAsync("ACT-M11-36-CIRCUIT", "SYSTEM_HEALTH_WORKER", new {
            CapabilityId = capabilityId,
            ErrorRate = errorRate,
            P99LatencyMs = p99Latency,
            Action = "CIRCUIT_BREAKER_TRIPPED"
        });
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `HR-G01` | Sổ sức khỏe BẮT BUỘC giám sát thời gian thực $100\%$ 15 năng lực tích hợp M12 qua 4 trạng thái chuẩn. |
| `HR-G02` | Chỉ số tỷ lệ lỗi và độ trễ p99 BẮT BUỘC tính toán dựa trên Cửa sổ trượt 60 giây (`60s Sliding Window`). |
| `HR-G03` | Khi ErrorRate $\ge 50\%$ hoặc p99 $\ge 3000\text{ms}$, ngắt mạch Circuit Breaker kích hoạt SLA $\le 2\text{ms}$. |
| `HR-G04` | Năng lực ở trạng thái `CIRCUIT_OPEN` BẮT BUỘC trả về lỗi Fast-Fail lập tức theo Fail-Closed mode (D-025). |
| `HR-G05` | Sau 5 phút ở `CIRCUIT_OPEN`, hệ thống tự động chuyển sang `HALF_OPEN` để thử nghiệm 5% lưu lượng khôi phục. |
| `HR-G06` | 100% các sự kiện chuyển đổi trạng thái ngắt mạch được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-36-CIRCUIT`). |
| `HR-G07` | SLA thực thi cập nhật chỉ số Telemetry vào Redis Cluster $< 1\text{ms}$ per request. |
| `HR-G08` | Phân quyền can thiệp bật/tắt thủ công trạng thái Circuit Breaker chỉ dành cho `SecurityAdmin` và `SystemAdmin`. |
| `HR-G09` | Hệ thống chịu tải giám sát đến 20,000 telemetry metrics/giây không gây treo Redis. |
| `HR-G10` | 100% các test case tự kiểm HR36-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HR36-01` | Năng lực Google OAuth đạt tỷ lệ lỗi 0% và p99 150ms | Trạng thái hiển thị `HEALTHY`, routing cuộc gọi bình thường |
| `HR36-02` | Năng lực AI Gemini bị tăng tỷ lệ lỗi lên 25% ($10\% \dots 49\%$) | Trạng thái chuyển `DEGRADED`, phát cảnh báo Warning |
| `HR36-03` | Năng lực Apple SIWA bị lỗi 60% ($> 50\%$) trong 30 giây | Circuit Breaker tự ngắt SLA $< 2\text{ms}$, chuyển `CIRCUIT_OPEN` |
| `HR36-04` | Gửi request tới Apple SIWA khi đang ở trạng thái `CIRCUIT_OPEN` | Trả về lỗi nhanh 503 `CIRCUIT_BREAKER_OPEN_FAIL_CLOSED` |
| `HR36-05` | Năng lực ở trạng thái `CIRCUIT_OPEN` sau 5 phút không có cuộc gọi lỗi | Tự động chuyển sang `HALF_OPEN`, cho phép 5% traffic thử nghiệm |
| `HR36-06` | Tra cứu vết Audit Log M11 sau khi Circuit Breaker bị ngắt | Ghi nhận Audit Event `ACT-M11-36-CIRCUIT` đính kèm ErrorRate |
| `HR36-07` | 5% traffic thử nghiệm ở `HALF_OPEN` đạt 100% thành công | Tự động khôi phục về trạng thái `HEALTHY` |
| `HR36-08` | 1 request trong 5% traffic thử nghiệm ở `HALF_OPEN` bị thất bại | Lập tức quay lại trạng thái `CIRCUIT_OPEN` thêm 5 phút |
| `HR36-09` | Tải đồng thời 5,000 telemetry metric writes/giây vào Redis Health Registry | Latency processing p95 $< 0.8\text{ms}$ per write |
| `HR36-10` | SecurityAdmin phát lệnh ngắt mạch thủ công cho năng lực FCM Push | Chuyển `CIRCUIT_OPEN` lập tức, ghi vết Audit Log |
| `HR36-11` | Thử cập nhật telemetry khi số lượng mẫu $< 10$ requests | Bỏ qua chưa tính toán Circuit Breaker (Min sample threshold) |
| `HR36-12` | Gửi request cập nhật telemetry khi Redis Cluster bị gián đoạn | Fallback ghi đệm local memory, retry async |
| `HR36-13` | User không phải Admin thử thay đổi trạng thái Circuit Breaker | Deny 403 Forbidden |
| `HR36-14` | User chưa đăng nhập gọi API tra cứu sổ sức khỏe M11 | Deny 401 Unauthorized |
| `HR36-15` | Giám sát năng lực Cloud Storage khi độ trễ p99 vượt 3500ms ($> 3000\text{ms}$) | Kích hoạt Circuit Breaker do Latency breach SLA $< 2\text{ms}$ |
| `HR36-16` | Kiểm tra thời gian tính toán lại tỷ lệ lỗi cửa sổ 60s | Calculation SLA $< 1.5\text{ms}$ |
| `HR36-17` | Phân tích tham chiếu các bản ghi `CapabilityHealths` trong CSDL | Quét schema `M11_CapabilityHealths` (T020) |
| `HR36-18` | Tiến trình `CapabilityHealthRegistryWorker` gặp sự cố sập ngắt | Lock phân tán M12-T033 tự nhả, worker dự phòng tiếp quản |
| `HR36-19` | Tra cứu bảng tổng hợp sức khỏe của 15 năng lực tích hợp | Trả về DTO danh sách 15 capabilities kèm status |
| `HR36-20` | Kiểm thử hoàn tất luồng lập sổ sức khỏe M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-HR-I01` | M11 hiện tại chưa có `CapabilityHealthRegistryService` thời gian thực | Không nắm được sức khỏe các dịch vụ tích hợp ngoài | M11-T049 (Source task) |
| `M11-HR-I02` | Thiếu cờ Cửa sổ trượt 60s tính toán tỷ lệ lỗi trong Redis | Không đo được chính xác xu hướng suy giảm năng lực | M11-T049; REL-03 |
| `M11-HR-I03` | Thiếu cơ chế ngắt mạch Circuit Breaker tự động SLA $\le 2\text{ms}$ | Ứng dụng bị treo chờ timeout khi dịch vụ ngoài bị đứt | M11-T049; M12-T038 |
| `M11-HR-I04` | Thiếu máy trạng thái `HALF_OPEN` tự động khôi phục sau 5 phút | Năng lực bị khóa vĩnh viễn dù dịch vụ ngoài đã hết lỗi | M11-T049 |
| `M11-HR-I05` | Chưa kết nối sự kiện ngắt mạch với Audit Log M11 (`ACT-M11-36-CIRCUIT`) | Không ghi vết được nguyên nhân dịch vụ bị gián đoạn | M11-T049; M11-T031 |

- `M11-HR-F01`: Triển khai `CapabilityHealthRegistryService` với 4-State Health Envelope (tiếp nhận: M11-T049).
- `M11-HR-F02`: Tích hợp Bắt buộc 60s Sliding Window & Redis Telemetry (tiếp nhận: M11-T049; REL-03).
- `M11-HR-F03`: Triển khai Auto-Tripping Circuit Breaker SLA $\le 2\text{ms}$ & Half-Open Recovery (tiếp nhận: M11-T049; M12-T038).
- `M11-HR-F04`: Thiết lập bộ kiểm thử tự động HR-G01–G10 và HR36-01–20 (tiếp nhận: M11 tasks).
- `M11-HR-F05`: Thu thập bằng chứng runtime cho luồng sổ sức khỏe M11 (tiếp nhận: M11 tasks; A-G04/A-G06).

## 8. Tự kiểm M11-T036

- Đã thiết kế hoàn chỉnh `M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0` với Ma trận Trạng thái Sức khỏe Năng lực Tích hợp.
- Đã chốt Ràng buộc Máy Trạng thái Sức khỏe 4 Cấp độ (`4-State Capability Health Envelope`).
- Đã chốt Cơ chế Cửa sổ Trượt Tính Toán Lỗi 60s (`60-Second Sliding Window Aggregation`).
- Đã lồng ghép Ràng buộc Kích hoạt Ngắt mạch SLA $\le 2\text{ms}$ (`Circuit Breaker Auto-Tripping SLA`), luồng Half-Open và Audit Log M11 (`ACT-M11-36-CIRCUIT`).
- Đã xác lập 10 Regression Gates (`HR-G01`–`HR-G10`) và 20 Test Cases tự kiểm (`HR36-01`–`HR36-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả lập sổ sức khỏe năng lực M11-T036 | WSA-7K2 |
