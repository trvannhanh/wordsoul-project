# Định nghĩa SLO và health từng năng lực M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-CAPABILITY-SLO-HEALTH-DEFINITIONS-1.0` |
| Task | M12-T045 |
| Đầu vào | M11-CAPABILITY-INTEGRATION-HEALTH-REGISTRY-1.0 (D-131), M12-CIRCUIT-BREAKER-BULKHEAD-1.0 (D-101), REL-03 |
| Phạm vi | Đặc tả Giao thức Định nghĩa SLO và Sức khỏe Từng Năng lực Tích hợp (`Integration Capability SLO & Health Specification`), ma trận mục tiêu SLO 4 dịch vụ đối tác (Google OAuth, Gemini AI, S3 Asset, Firebase Push), thuật toán tính cửa sổ trượt 60s và lưu vết kiểm toán M12 |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Định nghĩa SLO và Sức khỏe Từng Năng lực Tích hợp (`Integration Capability SLO & Health Specification`) thuộc M12, chuẩn hóa các mục tiêu mức độ dịch vụ (SLO - Service Level Objectives) rõ ràng cho từng năng lực đối tác bên ngoài (Google OAuth/OIDC, Google Gemini AI, S3/CDN Media Storage, Firebase FCM Push), đồng thời tích hợp trực tiếp với Sổ Sức khỏe M11-T036 để tự động ngắt mạch Circuit Breaker khi vi phạm cam kết chất lượng (REL-03).

- **Ma trận Mục tiêu SLO 4 Dịch vụ Đối tác Tích hợp (`4 Core Capability SLO Targets Invariant`)**:
  - `CAPABILITY_GOOGLE_OAUTH`: Sẵn sàng (Availability) SLO $99.9\%$, Độ trễ p95 SLA $\le 300\text{ms}$, Tỷ lệ lỗi Error Rate SLA $< 0.1\%$.
  - `CAPABILITY_GEMINI_AI`: Sẵn sàng SLO $99.5\%$, Độ trễ p95 SLA $\le 1,200\text{ms}$, Tỷ lệ lỗi Error Rate SLA $< 1.0\%$.
  - `CAPABILITY_S3_STORAGE`: Sẵn sàng SLO $99.99\%$, Độ trễ Direct Upload SLA $\le 250\text{ms}$, Tỷ lệ lỗi Error Rate SLA $< 0.01\%$.
  - `CAPABILITY_FIREBASE_PUSH`: Sẵn sàng SLO $99.9\%$, Thời gian phân phối PUSH SLA $\le 5\text{s}$, Tỷ lệ lỗi Error Rate SLA $< 0.5\%$.
- **Ràng buộc Tính toán Sức khỏe theo Cửa sổ Trượt 60 Giây (`60-Second Sliding Window Health Metric`)**: Hệ thống BẮT BUỘC sử dụng bộ nhớ đệm Redis Ring Buffer để aggregated chỉ số Error Rate và Latency p95 trong cửa sổ trượt 60 giây (`SlidingWindowSeconds = 60s`).
- **Tự động Ngắt mạch Circuit Breaker khi Vi phạm SLO SLA $\le 2\text{ms}$ (`Auto Circuit Breaker Tripping SLA`)**: Ngay khi chỉ số Error Rate của một năng lực tích hợp vượt mốc ngưỡng cảnh báo ($> 5\%$ trong 60s), hệ thống tự động chuyển trạng thái năng lực đó sang `CIRCUIT_OPEN` tại Sổ M11-T036 trong SLA $\le 2\text{ms}$ (REL-03).
- **Lưu vết Sổ Kiểm toán SLO M12 (`Capability SLO Audit Trail`)**: $100\%$ các lần vi phạm SLO hoặc chuyển trạng thái sức khỏe năng lực được ghi vết bất biến `ACT-M12-45-SLO` trong Sổ Kiểm toán M11.

## 2. Ma trận Chỉ số SLO và Ngưỡng Sức khỏe (Capability SLO & Health Matrix)

| Mã Năng lực (`CapabilityId`) | Mục tiêu Sẵn sàng (`Availability SLO`) | Độ trễ p95 SLA (`Latency p95 SLA`) | Tỷ lệ Lỗi SLA (`Error Rate SLA`) | Ngưỡng Trip Circuit Breaker | Hành vi Fallback Dự phòng | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| `CAP_GOOGLE_OAUTH` | **99.9%** | **$\le 300\text{ms}$** | $< 0.1\%$ | Error Rate $> 5\%$ | Local Password Auth | `ACT-M12-45-OAUTH` |
| `CAP_GEMINI_AI` | **99.5%** | **$\le 1,200\text{ms}$** | $< 1.0\%$ | Error Rate $> 10\%$ | Static Dictionary Cache | `ACT-M12-45-GEMINI` |
| `CAP_S3_STORAGE` | **99.99%** | **$\le 250\text{ms}$** | $< 0.01\%$ | Error Rate $> 2\%$ | Local Disk Spooling M11 | `ACT-M12-45-S3` |
| `CAP_FIREBASE_PUSH` | **99.9%** | **$\le 5,000\text{ms}$** | $< 0.5\%$ | Error Rate $> 5\%$ | In-App Banner Only | `ACT-M12-45-PUSH` |

## 3. Kiến trúc Luồng Giám sát SLO và Chuyển Trạng thái Sức khỏe (SLO Engine Pipeline)

```
[Integration Capability API Execution (Google / Gemini / S3 / Firebase)]
                                   |
                                   v
        [Record Call Metrics into 60s Redis Sliding Window Ring Buffer]
                                   |
                                   v
        [Calculate Sliding Window Metrics (Availability %, Latency p95, ErrorRate %)]
                                   |
           +-----------------------+-----------------------+
           | (Metrics Within SLO)                          | (ErrorRate > Threshold)
           v                                               v
[Set Capability State: HEALTHY]                 [Set Capability State: CIRCUIT_OPEN]
[Update M11 Capability Registry (D-131)]        [Trip Circuit Breaker SLA <= 2ms]
                                                [Trigger Fallback Action]
                                                [Record Audit Log ACT-M12-45-SLO]
```

## 4. Giao thức Thực thi Giám sát SLO CSDL (IntegrationSloMonitorService)

```csharp
public async Task EvaluateCapabilitySloAsync(string capabilityId)
{
    string redisKey = $"wordsoul:metrics:sliding_60s:{capabilityId}";
    var db = _redis.GetDatabase();

    // 1. Read 60-Second Sliding Window Ring Buffer Metrics
    var rawMetrics = await db.ListRangeAsync(redisKey, 0, -1);
    if (!rawMetrics.Any()) return;

    int totalCalls = rawMetrics.Length;
    int errorCalls = rawMetrics.Count(m => m.ToString().StartsWith("ERR"));
    double errorRate = (double)errorCalls / totalCalls;

    var latencies = rawMetrics.Where(m => m.ToString().StartsWith("OK"))
        .Select(m => long.Parse(m.ToString().Split(':')[1]))
        .OrderBy(x => x).ToList();

    long p95Latency = latencies.Any() ? latencies[(int)(latencies.Count * 0.95)] : 0;

    // 2. Check SLO Violation Thresholds
    var targetSlo = GetTargetSlo(capabilityId);
    bool isViolated = errorRate > targetSlo.MaxErrorRate || p95Latency > targetSlo.MaxP95LatencyMs;

    if (isViolated)
    {
        // 3. Trip Circuit Breaker SLA <= 2ms (Update M11 Registry D-131)
        await _capabilityRegistry.UpdateHealthStateAsync(capabilityId, CapabilityHealthState.CIRCUIT_OPEN);

        // 4. Record Audit Event M11
        await _auditLog.RecordEventAsync("ACT-M12-45-SLO", "SLO_MONITOR", new {
            CapabilityId = capabilityId,
            ErrorRate = errorRate,
            P95Latency = p95Latency,
            Status = "CIRCUIT_OPEN_SLO_VIOLATED"
        });
    }
    else
    {
        await _capabilityRegistry.UpdateHealthStateAsync(capabilityId, CapabilityHealthState.HEALTHY);
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SH-G01` | 100% dịch vụ tích hợp đối tác BẮT BUỘC có mục tiêu SLO được định nghĩa minh bạch trong ma trận. |
| `SH-G02` | Chỉ số sức khỏe SLO BẮT BUỘC được tính toán liên tục qua Cửa sổ trượt 60 giây (`SlidingWindowSeconds = 60s`). |
| `SH-G03` | Khi Error Rate vượt ngưỡng cho phép, Circuit Breaker BẮT BUỘC tự động ngắt mạch `CIRCUIT_OPEN` SLA $\le 2\text{ms}$. |
| `SH-G04` | Dịch vụ AI Gemini khi vi phạm SLO p95 $> 1,200\text{ms}$ BẮT BUỘC tự động chuyển sang chế độ từ điển tĩnh fallback. |
| `SH-G05` | Dịch vụ Google OAuth khi vi phạm SLO BẮT BUỘC giữ vững tính năng đăng nhập mật khẩu local thông thường. |
| `SH-G06` | 100% các đợt vi phạm SLO hoặc chuyển trạng thái sức khỏe được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M12-45-SLO`). |
| `SH-G07` | SLA thực thi đánh giá metrics SLO từ Redis Ring Buffer $< 3.0\text{ms}$. |
| `SH-G08` | Phân quyền điều chỉnh ngưỡng SLO chỉ dành riêng cho `IntegrationAdmin` và `SystemAdmin`. |
| `SH-G09` | Hệ thống hỗ trợ thu thập tới 10,000 metrics/giây trên Redis Ring Buffer mà không gây tràn RAM. |
| `SH-G10` | 100% các test case tự kiểm SH45-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SH45-01` | Gửi 1,000 request tới Google OAuth với Error Rate = 0.05% ($< 0.1\%$) | Đánh dấu `HEALTHY`, giữ kết nối bình thường |
| `SH45-02` | Gửi 1,000 request tới Google OAuth với Error Rate = 6% ($> 5\%$) | Tự động ngắt `CIRCUIT_OPEN` SLA $< 1.8\text{ms}$, ghi vết Audit Log |
| `SH45-03` | Dịch vụ AI Gemini có độ trễ p95 = 1,500ms ($> 1,200\text{ms}$) trong 60s | Chuyển trạng thái `DEGRADED`, kích hoạt từ điển tĩnh fallback |
| `SH45-04` | S3 Storage Upload đạt độ trễ p95 = 180ms ($< 250\text{ms}$) | Đánh dấu `HEALTHY`, duy trì Direct Upload URL |
| `SH45-05` | Firebase FCM Push đạt tỷ lệ lỗi 0.3% ($< 0.5\%$) | Đánh dấu `HEALTHY`, cho phép gửi PUSH bình thường |
| `SH45-06` | Tra cứu vết Audit Log M11 sau khi Gemini AI bị vi phạm SLO | Ghi nhận Audit Event `ACT-M12-45-GEMINI` |
| `SH45-07` | Dịch vụ S3 bị gián đoạn (Error Rate = 10% $> 2\%$) | Tự động chuyển sang `LOCAL_DISK_SPOOLING` M11 |
| `SH45-08` | Khôi phục chỉ số Google OAuth về Error Rate 0% sau 5 phút | Chuyển từ `CIRCUIT_OPEN` sang `HALF_OPEN` chạy 3 trial requests |
| `SH45-09` | Tải đồng thời 200 request ghi metric vào Redis Ring Buffer | Record latency p95 $< 0.4\text{ms}$ per call |
| `SH45-10` | Kiểm tra thời gian vô hiệu hóa các bản ghi metric quá 60 giây | Auto-evict khỏi Ring Buffer SLA $< 1\text{s}$ |
| `SH45-11` | Thử nạp `CapabilityId` không nằm trong danh mục tích hợp | Reject 400 `INVALID_CAPABILITY_ID` |
| `SH45-12` | Gửi request cấu hình ngưỡng SLO khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `SH45-13` | User không phải IntegrationAdmin thử điều chỉnh SLO Gemini | Deny 403 Forbidden |
| `SH45-14` | User chưa đăng nhập gọi API tra cứu báo cáo SLO M12 | Deny 401 Unauthorized |
| `SH45-15` | Cập nhật ngưỡng MaxP95LatencyMs cho Gemini từ 1200ms lên 1500ms | Lưu cấu hình mới, cập nhật tiêu chuẩn đánh giá |
| `SH45-16` | Kiểm tra độ trễ đồng bộ cờ CIRCUIT_OPEN tới API Gateway | Propagation SLA $< 100\text{ms}$ |
| `SH45-17` | Phân tích tham chiếu các bản ghi `CapabilitySloHistories` trong CSDL | Quét schema `M12_CapabilitySloHistories` (T020) |
| `SH45-18` | Dịch vụ Redis bị gián đoạn trong lúc đánh giá SLO | Fallback dùng giá trị health gần nhất trong 30s |
| `SH45-19` | Tra cứu bảng tổng hợp SLO Compliance Rate của 4 tích hợp | Trả về DTO báo cáo tỷ lệ tuân thủ SLO (%) |
| `SH45-20` | Kiểm thử hoàn tất luồng định nghĩa SLO và health M12-CAPABILITY-SLO-HEALTH-DEFINITIONS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-SH-I01` | M12 hiện tại chưa có `IntegrationSloMonitorService` giám sát SLO | Risk không biết được chất lượng tích hợp đối tác suy giảm | M12-T049 (Source task) |
| `M12-SH-I02` | Thiếu luồng tính toán Cửa sổ trượt 60 giây qua Redis Ring Buffer | Đánh giá sai lệch do dùng số liệu cộng dồn toàn thời gian | M12-T049; REL-03 |
| `M12-SH-I03` | Thiếu cờ Tự động Ngắt mạch Circuit Breaker SLA $\le 2\text{ms}$ khi vi phạm SLO | Dịch vụ bị treo cứng khi đối tác bên ngoài bị gián đoạn | M12-T049; M12-T038 |
| `M12-SH-I04` | Thiếu luồng tự động bật Static Fallback khi Gemini AI vi phạm SLO | Người học bị gián đoạn trải nghiệm học từ vựng | M12-SH-F04; M12-T005 |
| `M12-SH-I05` | Chưa kết nối sự kiện vi phạm SLO với Audit Log M11 (`ACT-M12-45-SLO`) | Không ghi vết được lịch sử suy giảm chất lượng tích hợp | M12-T049; M11-T031 |

- `M12-SH-F01`: Triển khai `IntegrationSloMonitorService` với 4 Core Capability SLO Targets (tiếp nhận: M12-T049).
- `M12-SH-F02`: Tích hợp Bắt buộc 60s Sliding Window Ring Buffer & Auto Circuit Breaker SLA $\le 2\text{ms}$ (tiếp nhận: M12-T049; REL-03).
- `M12-SH-F03`: Triển khai Fallback Action Automation for Gemini & S3 (tiếp nhận: M12-T049; M12-T038).
- `M12-SH-F04`: Thiết lập bộ kiểm thử tự động SH-G01–G10 và SH45-01–20 (tiếp nhận: M12 tasks).
- `M12-SH-F05`: Thu thập bằng chứng runtime cho luồng SLO M12 (tiếp nhận: M12 tasks; A-G04/A-G06).

## 8. Tự kiểm M12-T045

- Đã thiết kế hoàn chỉnh `M12-CAPABILITY-SLO-HEALTH-DEFINITIONS-1.0` với Ma trận Chỉ số SLO và Ngưỡng Sức khỏe.
- Đã chốt Ràng buộc Ma trận Mục tiêu SLO 4 Dịch vụ Đối tác Tích hợp (Google OAuth, Gemini AI, S3 Storage, Firebase Push).
- Đã chốt Ràng buộc Tính toán Sức khỏe theo Cửa sổ Trượt 60 Giây (`SlidingWindowSeconds = 60s`).
- Đã lồng ghép Tự động Ngắt mạch Circuit Breaker khi Vi phạm SLO SLA $\le 2\text{ms}$ (REL-03) và Audit Log M11 (`ACT-M12-45-SLO`).
- Đã xác lập 10 Regression Gates (`SH-G01`–`SH-G10`) và 20 Test Cases tự kiểm (`SH45-01`–`SH45-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả định nghĩa SLO và health từng năng lực M12-T045 | WSA-7K2 |
