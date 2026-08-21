# Xác định thời gian thực và độ mới chỉ số M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-REALTIME-FRESHNESS-1.0` |
| Task | M11-T023 |
| Đầu vào | M11-METRIC-DICT-1.0 (M11-T022) |
| Phạm vi | Ranh giới thời gian thực (Real-time vs Near Real-time vs Batch), Cam kết SLA độ mới chỉ số (Data Freshness SLAs) và hiển thị chỉ báo suy giảm dữ liệu (Degraded Data Indicator) |
| Tự kiểm | B-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định tiêu chuẩn đo lường độ mới chỉ số (Metric Freshness) và ranh giới xử lý thời gian thực trong dashboard vận hành M11.

- **Cam kết SLA Độ mới Chỉ số (`Data Freshness SLA Invariant`)**:
  - *Chỉ số An ninh / Sự cố (P1/P2 Alerts)*: Độ mới $\le 10\text{s}$ (Real-time).
  - *Chỉ số Vận hành & Hệ thống (System Health Metrics)*: Độ mới $\le 60\text{s}$ (Near Real-time).
  - *Chỉ số Học thuật & Đối soát (Reconciliation & Analytics)*: Độ mới $\le 15\text{m}$ (Batch / Scheduled Aggregation).
- **Chỉ báo Suy giảm Dữ liệu (`Degraded Data Indicator Invariant`)**: khi nguồn dữ liệu chỉ số bị gián đoạn hoặc trễ quá ngưỡng SLA Freshness, 100% dashboard/API hiển thị phải gắn nhãn `IsPartialData = true` hoặc `FreshnessStatus = DEGRADED`. CẤM hiển thị thông số 0 hoặc giả định hệ thống "KHỎE" khi dữ liệu trễ.

## 2. Thang Phân loại Độ mới Chỉ số (Metric Freshness Classification)

| Phân lớp | Mức SLA Tối đa | Phương thức Thu thập | Chỉ số Áp dụng |
|---|---|---|---|
| `REALTIME` | $\le 10\text{s}$ | In-Memory Counter / Prometheus Push | Brute Force Attacks, Circuit Breaker State, Active Sessions |
| `NEAR_REALTIME` | $\le 60\text{s}$ | Async Redis Buffer Ingestion | Request Rates (RPS), Error Rates, Latency p95/p99 |
| `BATCH` | $\le 15\text{m}$ | SQL Aggregation / Cron Worker | Active Learners Daily, Total Exp Granted, Storage Metering |

## 3. Thuật toán Đánh giá Độ mới Chỉ số (Metric Freshness Evaluation)

```csharp
public MetricValueDto EvaluateMetricFreshness(string metricCode, MetricRawValue rawValue)
{
    var freshnessLimit = GetSlaLimit(metricCode); // e.g. 60 seconds
    var dataAgeSeconds = (DateTime.UtcNow - rawValue.TimestampUtc).TotalSeconds;

    bool isDegraded = dataAgeSeconds > freshnessLimit.TotalSeconds;

    return new MetricValueDto
    {
        MetricCode = metricCode,
        Value = rawValue.Value,
        DataAgeSeconds = Math.Round(dataAgeSeconds, 1),
        FreshnessStatus = isDegraded ? FreshnessStatus.DEGRADED : FreshnessStatus.FRESH,
        IsPartialData = isDegraded
    };
}
```

## 4. Regression Gates và Test Cases

### 4.1. Regression Gates
- `RF-G01`: 100% chỉ số vận hành trễ quá thời gian SLA đều được tự động gán nhãn `FreshnessStatus = DEGRADED`.
- `RF-G02`: Chỉ số an ninh P1/P2 luôn duy trì SLA độ mới $\le 10\text{s}$.
- `RF-G03`: CẤM trả về giá trị 0 hoặc trạng thái giả định "HEALTHY" khi nạp metric thất bại.

### 4.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RF23-01` | Lấy chỉ số RPS hệ thống với dữ liệu thu thập cách đây 5 giây | Trả `FreshnessStatus = FRESH`, `IsPartialData = false`. |
| `RF23-02` | Lấy chỉ số RPS hệ thống khi pipeline nạp bị kẹt 120 giây ($> 60\text{s}$) | Trả `FreshnessStatus = DEGRADED`, `IsPartialData = true`. |
| `RF23-03` | Nguồn dữ liệu metric bị ngắt kết nối hoàn toàn | Trả `Status = UNKNOWN` kèm cảnh báo dữ liệu không sẵn sàng, cấm trả về 0. |
| `RF23-04` | Kiểm thử hoàn tất luồng M11-REALTIME-FRESHNESS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 5. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-RF-F01` | Thêm trường `DataAgeSeconds` vào `MetricValueDto` | Đảm bảo UI dashboard nhận diện độ trễ dữ liệu | M11-T024 |

## 6. Tự kiểm M11-T023
- Đã hoàn thành đặc tả `M11-REALTIME-FRESHNESS-1.0` với SLA 3 tầng (Realtime 10s, Near Realtime 60s, Batch 15m).
- Chốt nguyên tắc Degraded Data Indicator.
- Xác lập 3 Regression Gates (`RF-G01`–`RF-G03`) và 4 Test Cases (`RF23-01`–`RF23-04`).

## 7. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thời gian thực và độ mới chỉ số M11-T023 | WSA-7K2 |
