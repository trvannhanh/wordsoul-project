# Xây dựng kiểm thử hợp đồng và canary — lát A M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-CONTRACT-TESTING-CANARY-A-1.0` |
| Task | M12-T047-A |
| Đầu vào | M12-USAGE-COST-BUDGET-TRACKING-1.0 (D-143), M12-CIRCUIT-BREAKER-BULKHEAD-1.0 (D-101), REL-03 |
| Phạm vi | Đặc tả Giao thức Kiểm thử Hợp đồng và Xác minh Canary Lát A (`Integration Contract Testing & Canary Verification Protocol - Slice A`), kiểm thử Pact Consumer-Driven Contract, tiến trình worker `CanaryVerificationWorker` chạy luồng điều hướng 1% lưu lượng canary, tự động Rollback SLA $\le 10$ giây khi có sự cố và lưu vết kiểm toán M12 |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Kiểm thử Hợp đồng và Xác minh Canary Lát A (`Integration Contract Testing & Canary Verification Protocol - Slice A`) thuộc M12, xác lập quy trình kiểm thử tự động hợp đồng API đối tác (Pact Consumer-Driven Contracts) trong CI/CD pipeline và chiến lược triển khai Canary (Canary Deployment) phân luồng thử nghiệm $1\%$ lưu lượng thực tế sang các endpoint đối tác mới nâng cấp trước khi chuyển giao $100\%$ production (REL-03).

- **Quy trình Kiểm thử Hợp đồng Pact Tự động (`Consumer-Driven Contract Testing Invariant`)**: 100% các tích hợp với nhà cung cấp dịch vụ ngoài (Google OAuth, Gemini AI, S3 Storage, Firebase Push) BẮT BUỘC có bộ kiểm thử Hợp đồng Pact (`PactContractTests`) xác minh Schema JSON, Headers, Status Codes và Payload định dạng trong môi trường CI/CD trước khi phát hành code mới.
- **Chiến lược Điều hướng Canary 1% Lưu lượng (`1% Canary Traffic Allocation`)**: Tiến trình `CanaryVerificationWorker` điều hướng chính xác $1\%$ lưu lượng request thực tế (`CanaryTrafficPercent = 1%`) sang phiên bản SDK/Endpoint đối tác mới trong khoảng thời gian khởi động 5 phút (`WarmupMinutes = 5m`).
- **Tự động Rollback Khẩn cấp SLA $\le 10\text{s}$ (`Auto Rollback SLA on Canary Failure`)**: Trong suốt 5 phút warm-up, nếu tỷ lệ lỗi của Canary Endpoint vượt quá $1\%$ (`CanaryErrorRate > 1%`), hệ thống TỰ ĐỘNG hủy luồng Canary, đưa $100\%$ lưu lượng về lại phiên bản Stable cũ trong SLA $\le 10$ giây và phát cảnh báo `P2_HIGH` sang Slack `#deployments` (REL-03).
- **Lưu vết Sổ Kiểm toán Canary M12 (`Canary Verification Audit Trail`)**: $100\%$ các đợt phát hành Canary, bao gồm đợt thăng cấp thành công (Promoted 100%) hoặc tự động Rollback, được ghi vết bất biến `ACT-M12-47-CANARY` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Thử nghiệm Canary (Canary Deployment Matrix)

| Giai đoạn Canary (`CanaryPhase`) | Tỷ lệ Lưu lượng (`Traffic %`) | Thời gian Giám sát (`WarmupWindow`) | Tiêu chuẩn Thăng cấp (`Promotion Criteria`) | Hành vi Khi Thất bại | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **`PHASE_1_PROBE`** | **1% Traffic** | **5 Phút (Warm-up)** | ErrorRate $< 1.0\%$, Latency p95 OK | **Auto Rollback SLA $\le 10\text{s}$** | `ACT-M12-47-PROBE` |
| `PHASE_2_EXPAND` | 10% Traffic | 10 Phút | ErrorRate $< 0.5\%$, Latency p95 OK | Auto Rollback SLA $\le 10\text{s}$ | `ACT-M12-47-EXPAND` |
| `PHASE_3_FULL` | 100% Traffic | Live Production | Sức khỏe M11-T036 `HEALTHY` | Rollback qua Playbook M11 | `ACT-M12-47-PROMOTED` |

## 3. Kiến trúc Luồng Kiểm thử Hợp đồng và Canary M12 (Canary Engine Pipeline)

```
[CI/CD Build Pipeline: Execute Pact Contract Tests]
                        |
                        v
 [Pass Contract Tests -> Deploy Provider Endpoint (Canary 1%)]
                        |
                        v
 [CanaryVerificationWorker: Route 1% Traffic to Canary Provider]
                        |
                        v
 [Monitor 5-Minute Warm-Up Metrics (Error Rate & Latency)]
                        |
        +---------------+---------------+
        | (ErrorRate <= 1.0%)           | (ErrorRate > 1.0%)
        v                               v
[Promote to 10% -> 100% Traffic] [AUTO ROLLBACK SLA <= 10s]
[Record Audit ACT-M12-47-PROMOTED] [Route 100% Traffic Back to Stable]
                                 [Trigger Alert P2_HIGH (D-132)]
                                 [Record Audit ACT-M12-47-ROLLBACK]
```

## 4. Giao thức Thực thi Worker Canary CSDL (CanaryVerificationService)

```csharp
public async Task MonitorCanaryDeploymentAsync(string deploymentId, string providerKey)
{
    var db = _redis.GetDatabase();
    string canaryMetricsKey = $"wordsoul:canary:metrics:{deploymentId}";

    // 1. Read 5-Minute Canary Probe Metrics
    long totalRequests = await db.HashGetAsync(canaryMetricsKey, "TotalRequests").ToLongAsync();
    long errorRequests = await db.HashGetAsync(canaryMetricsKey, "ErrorRequests").ToLongAsync();

    if (totalRequests < 100) return; // Minimum sample size

    double canaryErrorRate = (double)errorRequests / totalRequests;

    // 2. Auto Rollback SLA <= 10s if Error Rate > 1%
    if (canaryErrorRate > 0.01)
    {
        // Instantly revert Redis Traffic Router to Stable Version
        await db.StringSetAsync($"wordsoul:provider:active_version:{providerKey}", "STABLE_VERSION");

        await _alertService.DispatchIncidentAlertAsync(providerKey, SeverityLevel.P2_HIGH, 
            $"Canary deployment {deploymentId} failed: ErrorRate = {canaryErrorRate:P2} (> 1%). Auto-rolled back SLA <= 10s.");

        await _auditLog.RecordEventAsync("ACT-M12-47-ROLLBACK", "CANARY_SYSTEM", new {
            DeploymentId = deploymentId,
            ProviderKey = providerKey,
            CanaryErrorRate = canaryErrorRate,
            Action = "AUTO_ROLLBACK_EXECUTED_SLA_10S"
        });
    }
    else if (totalRequests >= 1000)
    {
        // Promote Canary to 100% Production
        await db.StringSetAsync($"wordsoul:provider:active_version:{providerKey}", deploymentId);

        await _auditLog.RecordEventAsync("ACT-M12-47-PROMOTED", "CANARY_SYSTEM", new {
            DeploymentId = deploymentId,
            ProviderKey = providerKey,
            Action = "CANARY_PROMOTED_TO_100_PERCENT"
        });
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CT-G01` | 100% các thay đổi hợp đồng tích hợp BẮT BUỘC pass bộ Pact Contract Tests trong CI/CD pipeline trước khi deploy. |
| `CT-G02` | Tiến trình Canary BẮT BUỘC phân luồng chính xác $1\%$ lưu lượng thực tế (`CanaryTrafficPercent = 1%`) trong giai đoạn 1. |
| `CT-G03` | Thời gian warm-up theo dõi chỉ số Canary giai đoạn 1 BẮT BUỘC kéo dài 5 phút (`WarmupMinutes = 5m`). |
| `CT-G04` | Khi Canary Error Rate vượt $1\%$, hệ thống BẮT BUỘC tự động Rollback về phiên bản Stable SLA $\le 10$ giây (REL-03). |
| `CT-G05` | Lệnh Rollback tự động BẮT BUỘC kích hoạt cảnh báo `P2_HIGH` sang Slack `#deployments` (D-132). |
| `CT-G06` | 100% các đợt phát hành Canary hoặc Auto-Rollback được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M12-47-CANARY`). |
| `CT-G07` | SLA thực thi điều hướng luồng Canary 1% tại API Gateway $< 1.0\text{ms}$ per request. |
| `CT-G08` | Phân quyền thăng cấp thủ công hoặc hủy Canary chỉ dành cho `ReleaseManager` và `DevOps`. |
| `CT-G09` | Khả năng tự động Rollback không làm rò rỉ hoặc xáo trộn dữ liệu phiên làm việc người học. |
| `CT-G10` | 100% các test case tự kiểm CT47-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CT47-01` | Chạy Pact Contract Test cho Google OAuth API có Schema hợp lệ | Pass 100% contract assertions trong CI/CD |
| `CT47-02` | Chạy Pact Contract Test cho Gemini AI API phát hiện thiếu trường `candidates` | Fail build CI/CD SLA $< 5\text{s}$, chặn deploy |
| `CT47-03` | Khởi tạo đợt Canary 1% cho Gemini AI SDK v2 mới nâng cấp | Phân chính xác 1% requests sang v2, 99% sang Stable v1 |
| `CT47-04` | Phiên bản Canary Gemini v2 bị lỗi 2% ($> 1\%$) trong 3 phút warm-up | Tự động Rollback 100% luồng về v1 SLA $< 8\text{s}$, phát alert P2 |
| `CT47-05` | Phiên bản Canary Gemini v2 đạt Error Rate 0.1% ($< 1\%$) sau 5 phút | Thăng cấp sang Phase 2 (10% Traffic) |
| `CT47-06` | Tra cứu vết Audit Log M11 sau khi Auto-Rollback đợt Canary lỗi | Ghi nhận Audit Event `ACT-M12-47-ROLLBACK` |
| `CT47-07` | ReleaseManager bấm nút "Rollback Manual" trong khi Canary đang chạy | Revert 100% traffic về Stable ngay lập tức |
| `CT47-08` | Tra cứu vết Audit Log M11 sau khi thăng cấp Canary 100% thành công | Ghi nhận Audit Event `ACT-M12-47-PROMOTED` |
| `CT47-09` | Tải đồng thời 2,000 request qua API Gateway kiểm tra Router Canary | Router processing latency p95 $< 0.9\text{ms}$ |
| `CT47-10` | Kiểm tra thời gian vô hiệu cờ Canary khi Rollback hoàn tất | Invalidation SLA $< 200\text{ms}$ trên Redis |
| `CT47-11` | Thử nạp tỷ lệ phân luồng Canary không hợp lệ (Ví dụ: 150%) | Reject 400 `INVALID_CANARY_TRAFFIC_PERCENTAGE` |
| `CT47-12` | Gửi request cấu hình đợt Canary khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `CT47-13` | User không phải ReleaseManager/DevOps thử hủy đợt Canary | Deny 403 Forbidden |
| `CT47-14` | User chưa đăng nhập gọi API tra cứu trạng thái đợt Canary M12 | Deny 401 Unauthorized |
| `CT47-15` | Chạy đợt Canary cho S3 Storage Direct Upload URL mới | Phân 1% upload URL mới, theo dõi tỷ lệ thành công |
| `CT47-16` | Kiểm tra độ trễ phát cảnh báo P2_HIGH sang Slack khi Rollback | Dispatch SLA $< 4\text{s}$ |
| `CT47-17` | Phân tích tham chiếu các bản ghi `CanaryDeployments` trong CSDL | Quét schema `M12_CanaryDeployments` (T020) |
| `CT47-18` | Dịch vụ Redis bị gián đoạn trong lúc Router điều hướng Canary | Fallback 100% traffic sang phiên bản Stable mặc định |
| `CT47-19` | Tra cứu danh sách các đợt triển khai Canary đang active | Trả về DTO danh sách ActiveCanaryDeployments |
| `CT47-20` | Kiểm thử hoàn tất luồng kiểm thử hợp đồng & canary M12-CONTRACT-TESTING-CANARY-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-CT-I01` | M12 hiện tại chưa có `CanaryVerificationService` phân luồng 1% | Risk phát hành SDK/Endpoint đối tác lỗi làm sập 100% hệ thống | M12-T049 (Source task) |
| `M12-CT-I02` | Thiếu cờ Tự động Rollback SLA $\le 10\text{s}$ khi Canary Error Rate $> 1\%$ | Sự cố đợt deploy mới kéo dài làm ảnh hưởng người dùng | M12-T049; REL-03 |
| `M12-CT-I03` | Thiếu bộ kiểm thử Pact Consumer-Driven Contracts trong CI/CD | Chuyển lỗi sai lệch Schema đối tác sang môi trường Production | M12-T049; M12-T004 |
| `M12-CT-I04` | Thiếu cờ Warm-up 5 phút theo dõi metric cho đợt Canary Probe | Đánh giá vội vã khi số lượng sample size chưa đủ đại diện | M12-CT-F04; M12-T045 |
| `M12-CT-I05` | Chưa kết nối sự kiện Canary Rollback với Audit Log M11 (`ACT-M12-47-CANARY`) | Không ghi vết được lịch sử thất bại của đợt phát hành | M12-T049; M11-T031 |

- `M12-CT-F01`: Triển khai `CanaryVerificationService` với 1% Traffic Allocation (tiếp nhận: M12-T049).
- `M12-CT-F02`: Tích hợp Bắt buộc Pact Contract Testing & Auto Rollback SLA $\le 10\text{s}$ (tiếp nhận: M12-T049; REL-03).
- `M12-CT-F03`: Triển khai 5-Minute Warm-Up Window & Alert P2 Integration (tiếp nhận: M12-T049; M11-T037).
- `M12-CT-F04`: Thiết lập bộ kiểm thử tự động CT-G01–G10 và CT47-01–20 (tiếp nhận: M12 tasks).
- `M12-CT-F05`: Thu thập bằng chứng runtime cho luồng Canary M12 (tiếp nhận: M12 tasks; A-G04/A-G06).

## 8. Tự kiểm M12-T047-A

- Đã thiết kế hoàn chỉnh `M12-CONTRACT-TESTING-CANARY-A-1.0` với Ma trận Quy trình Thử nghiệm Canary.
- Đã chốt Ràng buộc Quy trình Kiểm thử Hợp đồng Pact Tự động (`Consumer-Driven Contract Testing`).
- Đã chốt Ràng buộc Chiến lược Điều hướng Canary 1% Lưu lượng (`1% Canary Traffic Allocation`).
- Đã lồng ghép Tự động Rollback Khẩn cấp SLA $\le 10\text{s}$ khi Error Rate $> 1\%$ (REL-03) và Audit Log M11 (`ACT-M12-47-CANARY`).
- Đã xác lập 10 Regression Gates (`CT-G01`–`CT-G10`) và 20 Test Cases tự kiểm (`CT47-01`–`CT47-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả kiểm thử hợp đồng và canary Lát A M12-T047-A | WSA-7K2 |
