# Xác định chỉ số sức khỏe M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-IDENTITY-HEALTH-METRICS-1.0` |
| Task | M01-T040 |
| Đầu vào | M11-METRIC-DICT-1.0 (D-046), M01-IDENTITY-EVENT-CATALOG-1.0 (D-103), REL-01 |
| Phạm vi | Đặc tả 8 Chỉ số Sức khỏe Danh tính và Hồ sơ M01 (`Identity Health Metrics Engine`), ngưỡng cảnh báo an toàn SLO/SLA, cơ chế thu thập Prometheus/OpenTelemetry và bảo mật dữ liệu PII |
| Tự kiểm | A-G01, A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Khung Chỉ số Sức khỏe Danh tính M01 (`Identity Health Metrics Engine`) thuộc M01, chuẩn hóa bộ chỉ số quan sát (Observability Metrics) nhằm đánh giá tính sẵn sàng, độ tin cậy và hiệu năng của toàn bộ Module Danh tính & Hồ sơ, khớp nối trực tiếp với Từ điển Chỉ số Quản trị M11 (D-046).

- **8 Chỉ số Sức khỏe Danh tính Chuẩn hóa (`8 Core Identity Indicators`)**:
  - `M01_LOGIN_SUCCESS_RATE`: Tỷ lệ đăng nhập thành công. Mục tiêu $\ge 98.5\%$. Cảnh báo SEV-2 khi $< 95\%$.
  - `M01_LOGIN_LATENCY_P95`: Độ trễ đăng nhập p95. Mục tiêu $< 150\text{ms}$. Cảnh báo SEV-3 khi $> 300\text{ms}$.
  - `M01_REGISTRATION_CONVERSION_RATE`: Tỷ lệ hoàn tất đăng ký. Mục tiêu $\ge 85\%$. Cảnh báo SEV-3 khi $< 70\%$.
  - `M01_SECURITY_EPOCH_REVOCATION_COUNT`: Số lần tăng Epoch hủy token. Baseline $< 50$/h. Cảnh báo SEV-2 khi spike $> 500$/h.
  - `M01_ANOMALY_DETECTION_COUNT`: Số vụ việc phát hiện bất thường. Baseline $< 10$/h. Cảnh báo SEV-2 khi spike $> 100$/h.
  - `M01_ACCOUNT_LOCK_RATE`: Tỷ lệ tài khoản bị khóa. Baseline $< 0.5\%$. Cảnh báo SEV-3 khi $> 2.0\%$.
  - `M01_OUTBOX_PENDING_EVENT_COUNT`: Hàng chờ sự kiện Outbox M01. Mục tiêu $< 100$. Cảnh báo SEV-2 khi ứ đọng $> 1000$.
  - `M01_ACTIVE_SESSION_COUNT`: Số lượng phiên làm việc active đồng thời. Phục vụ theo dõi tải hệ thống.
- **Ràng buộc Bảo vệ PII và Gom nhóm Nhóm nhỏ (`Metrics PII Anonymization Invariant`)**: Dữ liệu chỉ số Prometheus tuyệt đối CẤM chứa Email, Họ tên hay địa chỉ IP thô. Đối với các nhóm cohort nhỏ hơn 10 người dùng, chỉ số BẮT BUỘC được gom nhóm tổng hợp (Aggregation) để tránh bị suy đoán ngược danh tính (D-046).
- **Thu thập Metric không làm trễ Luồng Nghiệp vụ (`Non-Blocking Metric Collection`)**: Việc đẩy metric sang Prometheus Exporter bắt buộc thực hiện qua bộ đếm in-memory (`Interlocked.Increment` / System.Diagnostics.Metrics) không ném exception và không làm tăng độ trễ API quá $1\text{ms}$.
- **Tích hợp Cảnh báo Tự động M11 (`Automated Escalation Integration`)**: Khi bất kỳ chỉ số nào vượt ngưỡng nguy hiểm (SEV-2/SEV-1), hệ thống TỰ ĐỘNG phát sự kiện `MetricAlertTriggeredIntegrationEvent` gửi tới Module M11 để chuyển tiếp cảnh báo PUSH/Telegram tới đội SRE/Security.

## 2. Danh mục 8 Chỉ số Sức khỏe Danh tính M01 (Identity Health Metric Catalog)

| Tên Metric (`MetricName`) | Thuộc tính / Loại | Mục tiêu Slo | Ngưỡng Cảnh báo (`Alert Threshold`) | Mức Độ Sự cố | Tần suất Đo |
|---|---|---|---|---|---|
| `M01_LOGIN_SUCCESS_RATE` | Rate / Gauge | $\ge 98.5\%$ | $< 95.0\%$ trong 5m | **SEV-2** | Mỗi 1 phút |
| `M01_LOGIN_LATENCY_P95` | Histogram (ms) | $< 150\text{ ms}$ | $> 300\text{ ms}$ trong 5m | **SEV-3** | Mỗi 1 phút |
| `M01_REGISTRATION_CONVERSION` | Rate / Gauge | $\ge 85.0\%$ | $< 70.0\%$ trong 15m | **SEV-3** | Mỗi 5 phút |
| `M01_EPOCH_REVOCATION_SPIKE` | Counter / Rate | $< 50\text{ /h}$ | $> 500\text{ /h}$ đột biến | **SEV-2** | Tức thì |
| `M01_ANOMALY_DETECTION_SPIKE`| Counter / Rate | $< 10\text{ /h}$ | $> 100\text{ /h}$ đột biến | **SEV-2** | Tức thì |
| `M01_ACCOUNT_LOCK_RATE` | Rate / Gauge | $< 0.5\%$ | $> 2.0\%$ trong 15m | **SEV-3** | Mỗi 5 phút |
| `M01_OUTBOX_PENDING_EVENTS` | Gauge | $< 100$ | $> 1,000$ backlog | **SEV-2** | Mỗi 30 giây |
| `M01_ACTIVE_SESSIONS` | Gauge | Theo dõi tải | Sloshing spike $> 200\%$ | **SEV-3** | Mỗi 1 phút |

## 3. Kiến trúc Luồng Thu thập và Cảnh báo Metrics (Metrics Pipeline Engine)

```
[M01 Login / Registration / Auth Action]
                   |
                   v
    [In-Memory Prometheus Exporter]
    - Meter: "wordsoul_identity_metrics"
    - Record Latency & Success/Failure Counter
                   |
                   v
    [Prometheus Server Scrapes /metrics Endpoint (every 15s)]
                   |
                   v
    [AlertManager Evaluates Rules]
                   |
         +---------+---------+
         | (Metric Normal)   | (Threshold Exceeded: e.g. Fail Rate > 5%)
         v                   v
   [No Alert]        [Trigger SEV-2 Security Alert]
                     - Publish MetricAlertTriggeredEvent to M11
                     - Dispatch Telegram / AlertManager PUSH
```

## 4. Giao thức Thực thi Đo đạc Metrics CSDL (IdentityHealthMetricsService)

```csharp
public class IdentityHealthMetricsService
{
    private static readonly Meter IdentityMeter = new("WordSoul.Identity.Metrics", "1.0");
    private static readonly Counter<long> LoginCounter = IdentityMeter.CreateCounter<long>("m01_login_total");
    private static readonly Histogram<double> LoginLatencyHistogram = IdentityMeter.CreateHistogram<double>("m01_login_latency_ms");
    private static readonly Counter<long> OutboxBacklogGauge = IdentityMeter.CreateCounter<long>("m01_outbox_pending_events");

    public void RecordLoginAttempt(bool isSuccess, double latencyMs, string loginType)
    {
        // Non-blocking in-memory metric recording
        LoginCounter.Add(1, 
            new KeyValuePair<string, object?>("status", isSuccess ? "SUCCESS" : "FAILURE"),
            new KeyValuePair<string, object?>("login_type", loginType)
        );

        LoginLatencyHistogram.Record(latencyMs);
    }

    public async Task CheckOutboxBacklogHealthAsync(DbContext db)
    {
        int pendingCount = await db.Set<OutboxEvent>().CountAsync(e => !e.IsProcessed);
        if (pendingCount > 1000)
        {
            await RaiseMetricAlertAsync("M01_OUTBOX_PENDING_EVENTS", pendingCount, "SEV-2", "Hàng chờ Outbox M01 bị ứ đọng lớn hơn 1000 sự kiện.");
        }
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `HM-G01` | 100% 8 chỉ số sức khỏe M01 được thu thập chuẩn xác qua OpenTelemetry / Prometheus Exporter. |
| `HM-G02` | Tỷ lệ đăng nhập thất bại $> 5\%$ trong 5m tự động phát cảnh báo SEV-2 tới đội vận hành M11. |
| `HM-G03` | Hàng chờ Outbox M01 bị ứ đọng $> 1000$ sự kiện tự động kích hoạt cảnh báo SEV-2. |
| `HM-G04` | Dữ liệu metric Prometheus tuyệt đối CẤM chứa Email, Họ tên hoặc IP thô của người học (D-046). |
| `HM-G05` | Nhóm cohort ít hơn 10 người dùng BẮT BUỘC được gom nhóm (Aggregation) cấm xuất metric riêng lẻ. |
| `HM-G06` | Thao tác ghi metric in-memory không làm tăng độ trễ API đăng nhập quá $1\text{ms}$. |
| `HM-G07` | 100% các vụ việc phát cảnh báo chỉ số được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-40`). |
| `HM-G08` | Phân quyền thay đổi ngưỡng cảnh báo SLO chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `HM-G09` | Endpoint `/metrics` phục vụ Prometheus scrape phản hồi trong SLA $< 20\text{ms}$. |
| `HM-G10` | 100% các test case tự kiểm HM40-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HM40-01` | 100 request đăng nhập thành công | Counter `m01_login_total{status="SUCCESS"}` tăng 100 |
| `HM40-02` | 10 request đăng nhập thất bại do sai mật khẩu | Counter `m01_login_total{status="FAILURE"}` tăng 10 |
| `HM40-03` | Tỷ lệ đăng nhập thất bại vượt mốc 5.5% trong 5 phút | AlertManager kích hoạt cảnh báo SEV-2 tới M11 |
| `HM40-04` | Độ trễ đăng nhập p95 chạm mốc 350ms ($> 300\text{ms}$) | Kích hoạt cảnh báo SEV-3 về hiệu năng API đăng nhập |
| `HM40-05` | Số lượng Epoch Revocation tăng đột biến 600 lần trong 1h | Kích hoạt cảnh báo SEV-2 về dấu hiệu bị tấn công token |
| `HM40-06` | Hàng chờ Outbox M01 bị ứ đọng 1200 sự kiện | Kích hoạt cảnh báo SEV-2 về nghẽn Outbox Worker |
| `HM40-07` | Quét endpoint Prometheus `/metrics` của M01 | Phản hồi chứa đủ 8 metric M01, 0 chứa PII |
| `HM40-08` | Thu thập metric cho 1 nhóm học viên gồm 5 người ($< 10$) | Tự động gom nhóm aggregated, không xuất tag cá nhân |
| `HM40-09` | Tra cứu vết Audit Log M11 sau khi phát cảnh báo SEV-2 | Ghi nhận Audit Event `ACT-M11-40` đính kèm MetricName |
| `HM40-10` | Tải đồng thời 1000 request đăng nhập | Metric collection latency $< 1\text{ms}$, 0 làm chậm API |
| `HM40-11` | Khai báo metric `M01_ACTIVE_SESSIONS` | Gauge trả về tổng số session active chính xác trong Redis |
| `HM40-12` | Tỷ lệ khóa tài khoản chạm 2.5% ($> 2.0\%$) | Kích hoạt cảnh báo SEV-3 về rủi ro khóa hàng loạt |
| `HM40-13` | User không phải Admin thử gọi API sửa ngưỡng SLO | Deny 403 Forbidden |
| `HM40-14` | User chưa đăng nhập gọi endpoint xem sức khỏe | Deny 401 Unauthorized |
| `HM40-15` | Sửa ngưỡng cảnh báo SLO của `LOGIN_LATENCY_P95` thành 200ms | Cập nhật cấu hình thành công |
| `HM40-16` | Kiểm tra thời gian phản hồi của endpoint `/metrics` | Response latency p95 $< 15\text{ms}$ |
| `HM40-17` | Phân tích tham chiếu các chỉ số M01 trong Prometheus | Quét metrics catalog M11-T022 (T020) |
| `HM40-18` | Thao tác ghi metric bị ném OutOfMemoryException | Graceful fallback không làm sập luồng đăng nhập chính |
| `HM40-19` | Phân tích chỉ số hoàn tất đăng ký (`M01_REGISTRATION_CONVERSION`) | Gauge trả về tỷ lệ hoàn tất đăng ký 88% |
| `HM40-20` | Kiểm thử hoàn tất luồng xác định chỉ số sức khỏe M01-IDENTITY-HEALTH-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-HM-I01` | M01 hiện tại chưa có bộ `IdentityHealthMetricsService` OpenTelemetry | Thiếu quan sát chỉ số sức khỏe đăng nhập/đăng ký thời gian thực | M01-T049 (Source task) |
| `M01-HM-I02` | Chưa có bộ cảnh báo tự động khi hàng chờ Outbox M01 bị nghẽn | Đội vận hành không biết khi nào Outbox Worker bị sập | M01-T049; M11-T036 |
| `M01-HM-I03` | Endpoint `/metrics` hiện tại chưa bật bộ lọc che mờ PII cho cohort $< 10$ | Nguy cơ vi phạm điều khoản gom nhóm metric PII D-046 | M01-T049; M11-T022 |
| `M01-HM-I04` | Thiếu histogram đo độ trễ p95 cho luồng xác thực token JWT | Không phát hiện được độ trễ khi Redis cache bị ngắt kết nối | M01-T049 |
| `M01-HM-I05` | Chưa kết nối metric M01 với Dashboard Grafana M11 | Khó khăn cho bộ phận SRE trong việc theo dõi tổng thể | M01-T049; M11-T036 |

- `M01-HM-F01`: Triển khai `IdentityHealthMetricsService` hỗ trợ 8 chỉ số sức khỏe M01 (tiếp nhận: M01-T049).
- `M01-HM-F02`: Tích hợp Bắt buộc `Metrics PII Anonymization` cho cohort $< 10$ (tiếp nhận: M01-T049; D-046).
- `M01-HM-F03`: Kết nối cảnh báo SEV-2/SEV-3 với Module M11 (tiếp nhận: M01-T049; M11-T036).
- `M01-HM-F04`: Thiết lập bộ kiểm thử tự động HM-G01–G10 và HM40-01–20 (tiếp nhận: M01 tasks).
- `M01-HM-F05`: Thu thập bằng chứng runtime cho luồng chỉ số sức khỏe M01 (tiếp nhận: M01 tasks; A-G01/A-G06).

## 8. Tự kiểm M01-T040

- Đã thiết kế hoàn chỉnh `M01-IDENTITY-HEALTH-METRICS-1.0` với 8 Chỉ số Sức khỏe Danh tính M01 Chuẩn hóa.
- Đã chốt Ràng buộc Bảo vệ PII và Gom nhóm Nhóm nhỏ (cohort $< 10$).
- Đã chốt Thu thập Metric non-blocking in-memory SLA $\le 1\text{ms}$.
- Đã lồng ghép Tự động Phát Cảnh báo SEV-2/SEV-3 sang M11 và Lưu vết Audit Log M11 (`ACT-M11-40`).
- Đã xác lập 10 Regression Gates (`HM-G01`–`HM-G10`) và 20 Test Cases tự kiểm (`HM40-01`–`HM40-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả xác định chỉ số sức khỏe M01-T040 | WSA-7K2 |
