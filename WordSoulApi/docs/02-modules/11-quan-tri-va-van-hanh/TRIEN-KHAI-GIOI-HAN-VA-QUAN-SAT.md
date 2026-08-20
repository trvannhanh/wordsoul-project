# Triển khai giới hạn và quan sát M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CANARY-ROLLOUT-1.0` |
| Task | M11-T016 |
| Đầu vào | M11-CHANGE-EXECUTION-1.0, M11-PREVIEW-1.0, M11-CONFIG-VERSION-1.0, M11-METRIC-DICT-1.0 |
| Phạm vi | Triển khai phân tầng (Canary/Staged Rollout) và quan sát an toàn cho thay đổi quản trị và phiên bản cấu hình |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Triển khai giới hạn và quan sát (Controlled Rollout & Canary Observation) quy định chiến lược áp dụng thay đổi cấu hình hệ thống và quản trị theo từng giai đoạn thử nghiệm giới hạn, kết hợp giám sát telemetry thời gian thực để tự động phát hiện bất thường và kích hoạt quay lại (Rollback) kịp thời trước khi sự cố lan rộng toàn bộ hệ thống.

- **Nguyên tắc phân tầng bắt buộc**: Mọi thay đổi cấu hình thuộc nhóm EC-2, EC-3, EC-4 hoặc có rủi ro `HIGH`/`CRITICAL` không được phát hành $100\%$ ngay lập tức. Bắt buộc trải qua các bước triển khai giới hạn (Canary / Staged Rollout).
- **Ràng buộc bằng chứng xem trước**: Mọi kế hoạch rollout phải tham chiếu exact `previewToken` và `previewDigest` còn hiệu lực từ `M11-PREVIEW-1.0`.
- **Cửa sổ quan sát (Observation Window)**: Mỗi giai đoạn triển khai ($1\%, 10\%, 50\%$) có một thời gian quan sát tối thiểu (vd: 15-30 phút). Không được nâng cấp sang giai đoạn kế tiếp khi chưa hoàn thành cửa sổ quan sát và xác minh tính hội tụ của consumer.
- **Tự động tạm dừng và quay lại (Auto-Abort & Auto-Rollback)**: Nếu bất kỳ chỉ số quan sát nào (lỗi API, latency p99, M12 circuit breaker, telemetry anomaly) vượt ngưỡng cảnh báo trong cửa sổ quan sát, hệ thống lập tức tự động dừng triển khai, đóng băng trạng thái và kích hoạt quy trình Rollback (T011).
- **Fail-Closed khi mất tín hiệu Telemetry**: Khi nguồn dữ liệu chỉ số quan sát bị gián đoạn, rỗng hoặc stale, hệ thống BẮT BUỘC tự động tạm dừng nâng cấp giai đoạn (`Auto-Pause`). Tuyệt đối KHÔNG tự động chuyển giai đoạn (`Auto-Promote`) trong điều kiện thiếu telemetry.

## 2. Ephemeral & Durable Canary Model

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `RolloutPlanRecord` | `planId` | `executionId`, `setVersionId`, `previewToken`, `rolloutStrategy`, `stagesDefinition`, `healthThresholds`, `createdBy`, `createdAtUtc` | Bất biến |
| `RolloutExecutionState` | `executionId` | `currentStageIndex`, `currentTargetPercentage`, `stageStartedAtUtc`, `stageObservationExpiresAtUtc`, `status`, `convergedConsumerCount` | CAS Mutable State |
| `RolloutObservationSnapshot` | `snapshotId` | `executionId`, `stageIndex`, `telemetryTimestampUtc`, `metricsData`, `healthCheckStatus`, `anomaliesDetected` | Append-only / Telemetry log |
| `RolloutAbortEvent` | `eventId` | `executionId`, `stageIndex`, `triggeredBy`, `breachedMetricId`, `breachedValue`, `thresholdLimit`, `actionTaken` | Append-only Audit |

Trạng thái Rollout Status:
`created` -> `canary_stage_0` (1%) -> `staged_stage_1` (10%) -> `staged_stage_2` (50%) -> `fully_promoted` (100%)
Hoặc: `paused_manual` / `auto_paused_telemetry_missing` / `aborted_auto_rollback` / `aborted_manual`.

## 3. Chiến lược triển khai giới hạn (Rollout Strategies)

| Chiến lược | Phạm vi áp dụng | Cơ chế phân chia tải lượng (`Cohort Partitioning`) | Cửa sổ quan sát |
|---|---|---|---|
| **Canary Rollout** | Cấu hình rủi ro cao (EC-3, EC-4), thay đổi thuật toán SRS/M04 | Trỏ $1\%$ traffic hoặc 1 nút worker cụ thể | $30$ phút cho mỗi mốc ($1\% \to 10\% \to 50\% \to 100\%$) |
| **Tenant / Org Staged** | Thay đổi chính sách áp dụng theo trường học / tổ chức | Áp dụng theo `OrganizationId` được chọn trước | $60$ phút giữa các nhóm tổ chức |
| **User Cohort Staged** | Tính năng học tập, giới hạn phiên, UX | Áp dụng theo `UserId % 100` ($5\% \to 25\% \to 100\%$) | $15$ phút cho mỗi nấc |
| **Shadow / Direct** | Cấu hình đọc thuần túy (Read-only metadata, EC-1) | Chạy song song không đổi truth hoặc áp dụng $100\%$ | $10$ phút shadow observation |

## 4. Quy trình nâng tầng và Kiểm tra hội tụ (Convergence Check)

### 4.1. Tiêu chí chuyển tầng (Promotion Criteria)
Để chuyển từ Stage $N$ lên Stage $N+1$, bắt buộc đạt đủ 4 điều kiện:
1. **Thời gian quan sát**: Đã trải qua đủ `minObservationDurationMinutes` của Stage $N$.
2. **Telemetry Health Check**: 100% các kiểm tra chỉ số thuộc `M11-METRIC-DICT-1.0` đạt ngưỡng an toàn.
3. **Consumer Snapshot Convergence**: Ít nhất $99\%$ các nút/phiên tiêu dùng dữ liệu (`Consumer Instances`) báo cáo đã nhận và chuyển sang phiên bản cấu hình mới (`reportedSetVersionId == targetSetVersionId`).
4. **Không có Cảnh báo Sự cố (No Open High/Critical Anomaly)**: Không phát sinh sự cố SEV-1/SEV-2 liên quan.

```
[Start Stage N] ---> (Start Observation Window) ---> [Collecting Telemetry & Observations]
                                                                  |
                         +----------------------------------------+----------------------------------------+
                         | (Metric Breached / Anomaly)                                                     | (Telemetry Missing / Stale)
                         v                                                                                 v
             [ABORT & AUTO-ROLLBACK (RB-1/2)]                                                    [AUTO-PAUSE ROLLOUT]
                         ^                                                                                 |
                         | (Manual Override Abort)                                                         v
                         +---------------------------------------------------------------------- [Manual Resolution]
                                                                                                           | (Telemetry Restored & Pass)
                                                                                                           v
                                                                                              (Check Convergence & Telemetry)
                                                                                                           | (Pass All Criteria)
                                                                                                           v
                                                                                                  [PROMOTE TO STAGE N+1]
```

## 5. Giám sát Telemetry và Ngưỡng kích hoạt Tự động Quay lại (Auto-Rollback Triggers)

Hệ thống liên tục theo dõi các chỉ số quan sát từ `M11-METRIC-DICT-1.0` trong suốt quá trình Rollout:

| Chỉ số quan sát | Nguồn chỉ số | Ngưỡng vi phạm kích hoạt Auto-Rollback (`Abort Threshold`) | Hành động tự động |
|---|---|---|---|
| **API Error Rate** | `MET-OPS-HEALTHY-CAP` | Tỷ lệ lỗi HTTP 5xx tăng $> 0.5\%$ so với baseline | Dừng Rollback, kích hoạt RB-1 khẩn cấp |
| **Latency Degradation** | Response telemetry | Thời gian phản hồi p99 tăng $> 25\%$ so với baseline | Tạm dừng Rollback, phát cảnh báo |
| **Reconciliation Discrepancy** | `MET-M06-RECON-DIFF` | Phát sinh sai lệch tài sản $> 0$ trong canary cohort | Lập tức hủy Rollout, kích hoạt RB-2 recovery |
| **Job Failure Spike** | `MET-JOB-SUCCESS` | Tỷ lệ thất bại công việc nền trong canary node $> 2\%$ | Dừng Rollout, cô lập worker node |
| **M12 Circuit Breaker Trip** | M12 Integration | Phát sinh $\ge 2$ lần ngắt mạch tích hợp ngoài | Hủy Rollout, quay lại phiên bản cấu hình cũ |

## 6. Xử lý Mất tín hiệu Telemetry & Tạm dừng Tự động (Auto-Pause)

- Nếu dịch vụ giám sát bị gián đoạn, mất kết nối Redis/Telemetry metrics hoặc chỉ số báo cáo bị `stale` vượt quá $5$ phút:
  1. Hệ thống chuyển trạng thái `RolloutExecutionState` thành `auto_paused_telemetry_missing`.
  2. Khóa tiến trình nâng tầng tự động (`Auto-Promote Blocked`).
  3. Gửi thông báo cảnh báo mức High đến kênh vận hành quản trị.
  4. Nếu sau $15$ phút không khôi phục được tín hiệu telemetry, tự động chuyển thành `aborted_auto_rollback` để đảm bảo an toàn tuyệt đối.

## 7. Bảo mật, Quyền và Che giấu Dữ liệu kiểm toán

- **Quyền điều khiển Rollout**: Yêu cầu quyền `M11-PERM-ROLLOUT-MANAGE` (gắn với vai trò R02 Operations Admin, R12 Security Admin).
- **Manual Override**: Chỉ người dùng có vai trò R12 mới được phép thực thi lệnh "Manual Force Promote" trong trường hợp telemetry bị nhiễu nhưng đã xác minh an toàn ngoài kênh.
- **Audit Logging**: Mọi sự kiện chuyển stage, tạm dừng, kích hoạt auto-rollback phải được ghi vào nhật ký kiểm toán bất biến `RolloutAbortEvent` kèm lý do và telemetry snapshot.

## 8. Tích hợp với Pipeline Thực thi và Rollback (T011 / D-042)

1. Khi Rollout Plan khởi chạy, `M11-CHANGE-EXECUTION-1.0` tạo bản ghi execution ở chế độ `CANARY_STAGED`.
2. Hệ thống cập nhật CAS pointer cho nhóm Canary target (Stage 0).
3. Nếu hết cửa sổ quan sát đạt tiêu chí -> Hệ thống tự động mở rộng CAS pointer cho Stage tiếp theo.
4. Nếu vi phạm ngưỡng Abort -> Execution Service gọi phương thức Rollback execution (RB-1 tái kích hoạt version cũ hoặc RB-2 khôi phục trạng thái), đồng thời ghi nhận kết quả thực thi là `FAILED_ROLLED_BACK`.

## 9. Regression Gate và Case tự kiểm

### 9.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CR-G01` | Mọi kế hoạch Rollout thuộc EC-2/3/4 bắt buộc có `RolloutPlanRecord` với phân tầng rõ ràng. |
| `CR-G02` | Khóa chính xác `previewToken`, `setVersionId` và `executionId` trước khi bắt đầu Stage 0. |
| `CR-G03` | Cửa sổ quan sát của mỗi Stage phải được thực thi đủ thời gian tối thiểu trước khi chuyển nấc. |
| `CR-G04` | Kiểm tra hội tụ (Convergence Check) đạt $\ge 99\%$ consumer instances báo cáo đã nhận version mới. |
| `CR-G05` | Ngưỡng vi phạm chỉ số (Error rate, Latency, Recon diff) tự động kích hoạt Auto-Rollback không cần can thiệp tay. |
| `CR-G06` | Mất tín hiệu Telemetry lập tức chuyển trạng thái thành `auto_paused_telemetry_missing`, cấm Auto-Promote. |
| `CR-G07` | Toàn bộ sự kiện Auto-Rollback hay Pause phải ghi nhận `RolloutAbortEvent` bất biến phục vụ kiểm toán. |
| `CR-G08` | Nguồn dữ liệu Rollout Plan và State phải đảm bảo tính bất biến/CAS transition an toàn. |
| `CR-G09` | Quyền điều chỉnh và can thiệp Rollout tuân thủ nghiêm ngặt ma trận phẩn quyền `M11-PERM-1.0`. |
| `CR-G10` | 100% các test case tự kiểm CR16-01–20 đạt thành công trong bộ suite kiểm thử. |

### 9.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CR16-01` | Khởi tạo Canary Rollout 1% cho cấu hình SRS hợp lệ | Tạo `RolloutPlanRecord` ở Stage 0, đếm đủ 1% canary target |
| `CR16-02` | Stage 0 hoàn tất 30 phút quan sát với chỉ số an toàn và convergence 100% | Tự động chuyển sang Stage 1 (10%) |
| `CR16-03` | Phát sinh tỷ lệ lỗi 1% (vượt ngưỡng 0.5%) ở Stage 1 | Tự động kích hoạt `aborted_auto_rollback`, gọi RB-1 về version cũ |
| `CR16-04` | Mất kết nối Telemetry Redis trong lúc đang quan sát Stage 2 | Tự động chuyển thành `auto_paused_telemetry_missing`, chặn Auto-Promote |
| `CR16-05` | Telemetry khôi phục sau 5 phút tạm dừng và chỉ số an toàn | Tiếp tục cửa sổ quan sát và cho phép chuyển nấc |
| `CR16-06` | Telemetry không khôi phục sau 15 phút tạm dừng | Tự động chuyển thành `aborted_auto_rollback` để đảm bảo an toàn |
| `CR16-07` | Consumer instance báo cáo convergence mới chỉ đạt 80% khi hết giờ quan sát | Chờ cho đến khi convergence đạt $\ge 99\%$ mới cho phép chuyển stage |
| `CR16-08` | Quản trị viên cố tình gọi Auto-Promote khi Telemetry đang bị thiếu | Hệ thống từ chối với lỗi `PROMOTION_BLOCKED_TELEMETRY_MISSING` |
| `CR16-09` | R12 Admin thực hiện Manual Override Abort | Hệ thống dừng ngay lập tức và chuyển sang trạng thái Rollback |
| `CR16-10` | Phát sinh `MET-M06-RECON-DIFF` sai lệch tài sản trong Canary cohort | Auto-rollback lập tức kích hoạt quy trình RB-2 khôi phục dữ liệu |
| `CR16-11` | Thử khởi tạo Rollout Plan với `previewToken` đã hết hạn | Reject request với lỗi `PREVIEW_TOKEN_EXPIRED` |
| `CR16-12` | Thử khởi tạo Canary Rollout không truyền danh sách Stage | Reject request với lỗi `INVALID_STAGES_DEFINITION` |
| `CR16-13` | Thực thi Canary Rollout đồng thời trên 2 policy set độc lập | Cả 2 rollout plan chạy song song không tranh chấp CAS |
| `CR16-14` | Thay đổi cấu hình đột ngột khi Rollout đang ở Stage 1 | Rollout hiện tại bị hủy (`aborted`), yêu cầu tạo Plan mới |
| `CR16-15` | Tự kiểm tra tính bất biến của bản ghi `RolloutAbortEvent` | API deny mọi thao tác Update/Delete trên Audit event |
| `CR16-16` | Thử nghiệm Shadow Rollout cho cấu hình Read-Only EC-1 | Áp dụng 100% sau 10 phút shadow observation không phát sinh lỗi |
| `CR16-17` | Latency p99 tăng 30% ở Stage 0 | Tự động tạm dừng Rollout và gửi alert High đến Slack/Email |
| `CR16-18` | User không có quyền `M11-PERM-ROLLOUT-MANAGE` gọi lệnh Pause Rollout | Deny 403 Forbidden |
| `CR16-19` | Tải đồng thời 50 telemetry snapshots trong cửa sổ quan sát | Hệ thống xử lý mượt mà, ghi nhận snapshot chính xác |
| `CR16-20` | Kiểm tra luồng Rollout từ Stage 0 -> Stage 3 (100%) hoàn tất thành công | Trạng thái chuyển thành `fully_promoted`, đóng plan |

## 10. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-CR-I01` | Chưa có DTO, Controller hay Service triển khai Canary/Staged Rollout trong `WordSoulApi` | Thiếu toàn bộ engine triển khai giới hạn và đếm cohort | M11-T049 |
| `M11-CR-I02` | Chưa có cơ chế giám sát telemetry thời gian thực để kích hoạt Auto-Rollback | Hệ thống không thể tự phục hồi khi có sự cố canary | M11-T049 |
| `M11-CR-I03` | Thiếu cơ chế đếm tỷ lệ hội tụ (`Consumer Snapshot Convergence`) từ client/nút | Không biết chính xác bao nhiêu node đã áp dụng cấu hình | M11-T049 |
| `M11-CR-I04` | Thiếu trạng thái `auto_paused_telemetry_missing` và quy tắc Fail-Closed | Có nguy cơ tự động nâng tầng mù khi mất telemetry | M11-T049 |
| `M11-CR-I05` | Chưa có bảng lưu trữ `RolloutPlanRecord`, `RolloutExecutionState` và `RolloutAbortEvent` | Thiếu cơ sở dữ liệu phục vụ điều khiển và kiểm toán | M11-T049 |
| `M11-CR-I06` | Chưa có tích hợp giữa Rollout Engine với Execution Rollback Services (T011) | Sự cố canary không tự động kích hoạt RB-1/RB-2 | M11-T049 |

- `M11-CR-F01`: Xây dựng `CanaryRolloutService` và API quản lý Rollout Plan (tiếp nhận: M11-T049).
- `M11-CR-F02`: Triển khai Telemetry Monitoring Engine và quy tắc Auto-Rollback Trigger (tiếp nhận: M11-T049).
- `M11-CR-F03`: Xây dựng cơ chế thu nhận báo cáo Convergence từ Consumer Instances (tiếp nhận: M11-T049).
- `M11-CR-F04`: Thiết lập bộ kiểm thử tự động CR-G01–G10 và CR16-01–20 (tiếp nhận: M11-T049).
- `M11-CR-F05`: Thu thập bằng chứng runtime cho toàn bộ hành trình Canary Rollout -> Auto-Rollback (tiếp nhận: M11-T049; A-G02/A-G06).

## 11. Tự kiểm M11-T016

- Đã thiết kế đầy đủ `M11-CANARY-ROLLOUT-1.0` bao phủ các chiến lược Canary, Staged, Shadow và Direct Rollout.
- Đã chốt quy trình kiểm tra hội tụ (`Convergence Check`) $\ge 99\%$ và các mốc thời gian quan sát tối thiểu.
- Đã xây dựng ma trận chỉ số telemetry và ngưỡng tự động kích hoạt Auto-Rollback (API error rate, latency p99, recon diff, M12 circuit breaker).
- Đã thiết lập nguyên tắc Fail-Closed `Auto-Pause` khi mất tín hiệu Telemetry.
- Đã xác lập 10 Regression Gates (`CR-G01`–`CR-G10`) và 20 Test Cases tự kiểm (`CR16-01`–`CR16-20`).
- Đã ghi nhận 6 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 12. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả triển khai giới hạn và quan sát M11-T016 | WSA-7K2 |
