# Xem trước và mô phỏng tác động M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-PREVIEW-1.0` |
| Task | M11-T015 |
| Đầu vào | M11-CHANGE-REQUEST-1.0, M11-CHANGE-DECISION-1.0, M11-CHANGE-SCHEDULE-1.0, M11-CHANGE-EXECUTION-1.0, M11-CONFIG-REG-1.0, M11-CONFIG-VALIDATION-1.0, M11-CONFIG-VERSION-1.0, M11-METRIC-DICT-1.0 |
| Phạm vi | Mô phỏng tác động thay đổi quản trị và phiên bản cấu hình trước khi phê duyệt |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Xem trước và mô phỏng tác động (Preview & Impact Simulation) cung cấp khả năng dự báo phạm vi ảnh hưởng, khác biệt cấu hình, tải lượng/ngân sách và biến động chỉ số vận hành cho các yêu cầu thay đổi quản trị (Change Request) và cấu hình hệ thống (System Configurations) trước khi quyết định phê duyệt và thực thi.

- **Chỉ đọc và độc lập tác quyền**: Preview/Simulation là phép tính hoàn toàn read-only projection. Preview KHÔNG ghi đè DB, KHÔNG chuyển pointer cấu hình, KHÔNG cấp quyền phê duyệt/thực thi, KHÔNG thay thế vòng đời quyết định (D-040) hay pipeline thực thi (D-042).
- **Ràng buộc ngữ cảnh bất biến**: Mỗi Preview phải gắn chặt với exact `changeRevisionId` + `revisionDigest`, exact candidate/policy-set version (`setVersionId` + digest), exact `metricVersion` và exact source snapshot timestamp (`snapshotAtUtc`).
- **Phân tách 3 tầng độc lập**:
  1. *Deterministic Validation*: Kiểm tra quy tắc cứng (CV-01..12, schema scalar/range, permissions/scope, schedule overlap sanity).
  2. *Impact Estimate / Projection*: Ước tính số lượng thực thể bị ảnh hưởng, structural diff trước/sau, tải lượng/ngân sách sử dụng, và delta chỉ số dự kiến.
  3. *Runtime Verification*: Kiểm thử và xác minh sau thực thi thực tế (nằm ở T011/T016).
- **Fail-Closed & Bounded Uncertainty**: Khi nguồn dữ liệu bị thiếu, cũ (`stale`), một phần (`partial`) hoặc không xác định (`unknown`), simulation KHÔNG được hiển thị 0 tác động hay màu xanh an toàn. Phải ghi nhận `uncertaintyBound` / `degradedSimulation` / `unknownImpact` và chặn tự động phê duyệt.
- **Bảo mật và riêng tư**: Mọi dữ liệu xem trước phải loại bỏ bí mật, token, PII người dùng và tuân thủ quy tắc áp chế cohort nhỏ ($k < 10$).
- **Thời hạn hiệu lực của Preview Token**: Kết quả xem trước hợp lệ trả về `previewToken` có TTL cố định (15 phút). Token hết hạn không được dùng làm căn cứ ràng buộc cho Decision record.

## 2. Ephemeral & Durable Preview Model

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `ChangePreviewRequest` | `previewId` | `changeRevisionId`, `revisionDigest`, `configSetVersionId`, `metricVersion`, `requestedBy`, `requestedAt`, `scopeRef`, `parameters` | Bất biến |
| `ChangePreviewResult` | `previewId` | `previewToken`, `previewDigest`, `evaluationState`, `deterministicValidationResult`, `impactProjectionResult`, `metricDeltaResult`, `generatedAtUtc`, `expiresAtUtc` | Bất biến |
| `ImpactProjection` | `projectionId` | `affectedScopes`, `affectedEntitiesCount`, `beforeAfterDiff`, `limitBudgetUsage`, `dependencyImpact` | Ephemeral calculation |
| `MetricProjectionDelta` | `deltaId` | `metricId`, `metricVersion`, `baselineValue`, `projectedValue`, `projectedDelta`, `confidenceInterval`, `uncertaintyReason` | Ephemeral calculation |
| `PreviewTokenLease` | `previewToken` | `previewDigest`, `changeRevisionId`, `revisionDigest`, `issuedAtUtc`, `expiresAtUtc`, `isRevoked` | Ephemeral / TTL 15m |

Constraints DB & Runtime:
- Unique `(changeRevisionId, configSetVersionId, metricVersion, parametersHash)` cho preview caching trong TTL.
- Hard check `expiresAtUtc > UtcNow` và `revisionDigest == currentRevisionDigest` khi bind `previewToken` vào `ChangeDecisionRecord`.

## 3. Vòng đời và trạng thái Preview

```
[Requested] ---> (Deterministic Validation) ---> [Validating]
                                                    |
                         +--------------------------+--------------------------+
                         | (Validation Failed)                                 | (Pass Validation)
                         v                                                     v
                 [Validation Failed]                                    [Projecting]
                         |                                                     |
                         |                                  +------------------+------------------+
                         |                                  | (Data Complete)                     | (Data Degraded/Stale)
                         v                                  v                                     v
                 [Evaluation Error]                          [Completed: Valid Clean]             [Completed: Degraded/Uncertain]
                         |                                         |                                     |
                         +-----------------------------------------+-------------------------------------+
                                                                   | (TTL 15m Expired)
                                                                   v
                                                               [Expired]
```

| Trạng thái kết quả | Ý nghĩa | Quyền gắn vào Decision? |
|---|---|---|
| `valid_clean` | Kiểm tra quy tắc cứng đạt, nguồn dữ liệu đầy đủ, mô phỏng tin cậy 100%. | Cho phép |
| `valid_with_warnings` | Kiểm tra quy tắc đạt nhưng có cảnh báo tiệm cận hạn mức hoặc delta chỉ số tăng cao. | Cho phép (yêu cầu xem lại cảnh báo) |
| `uncertainty_bounded` | Có nguồn dữ liệu bị thiếu/stale; tác động được giới hạn trong biên độ trên/dưới. | Cho phép (bắt buộc đánh dấu manual review) |
| `source_stale_degraded` | Nguồn dữ liệu suy giảm nặng; không thể tính toán đáng tin cậy. | Cấm tự động; yêu cầu làm tươi dữ liệu |
| `validation_failed` | Vi phạm quy tắc cứng CV-01..12, xung đột lịch hoặc không đủ quyền. | Cấm |
| `evaluation_error` | Lỗi hệ thống trong quá trình tính toán mô phỏng. | Cấm |

## 4. Phân tách Deterministic Validation, Impact Projection và Runtime Verification

| Tiêu chí | Deterministic Validation | Impact Projection | Runtime Verification |
|---|---|---|---|
| **Thời điểm chạy** | Ngay khi gửi Preview Request | Sau khi Deterministic Validation đạt | Sau khi Execution Pipeline thực thi (T011/T016) |
| **Bản chất tính toán** | Rule-based exact assertion (CV-01..12) | Statistical / Derived Estimation | Real runtime post-condition check |
| **Đầu vào** | Schema, Range, Scope, RBAC, Schedule | History logs, current metrics, active entity counts | Actual state after CAS pointer update |
| **Kết quả** | Pass / Fail (Binary) | Absolute count, % diff, delta range, confidence | Success / Reconcile Required / Rollback Triggered |
| **Tác động dữ liệu** | Không | Không | Thay đổi pointer / state thực tế |
| **Trách nhiệm** | T013 / T015 | T015 | T011 / T016 |

## 5. Phân tích Phạm vi (Scope), Vi sai (Diff) và Tác động phụ thuộc

### 5.1. Xác định phạm vi bị ảnh hưởng (`Affected Scopes`)
Mô phỏng phải xác định chính xác các lớp đối tượng chịu tác động:
1. **Global Scope**: Ảnh hưởng toàn bộ người dùng và hệ thống (vd: thay đổi `LogRetentionDays`, `SrsPolicyVersion`).
2. **Module Scope**: Chỉ ảnh hưởng 1 module cụ thể (vd: `M04` Vocab Progress review interval).
3. **User Cohort Scope**: Ảnh hưởng nhóm người dùng theo phân khúc (vd: nhóm người dùng mới tạo trong 7 ngày).
4. **Integration Capability Scope**: Ảnh hưởng đến các tích hợp ngoài (vd: `M12` Mail/Push notification delivery rate).

### 5.2. Cấu trúc Vi sai Trước/Sau (`Before/After Diff`)
Diff được tổng hợp theo định dạng JSON-patch chuẩn hóa kèm che giấu dữ liệu nhạy cảm:
```json
{
  "policySetId": "CFG-SET-SRS-01",
  "baseVersion": "v1.4",
  "targetVersion": "v1.5",
  "differences": [
    {
      "key": "WordsPerSession",
      "before": 10,
      "after": 15,
      "impactType": "USER_EXPERIENCE_CAPACITY",
      "riskLevel": "MEDIUM"
    },
    {
      "key": "ReviewIntervalHours",
      "before": 24,
      "after": 12,
      "impactType": "STUDY_WORKLOAD",
      "riskLevel": "HIGH"
    }
  ]
}
```

### 5.3. Tiêu thụ Hạn mức & Ngân sách (`Limit & Budget Consumption`)
Mô phỏng phải tính toán biến động hạn mức:
- **Rate Limiter Quota**: Biến động % tiêu thụ lưu lượng API khả dụng (`M12-RATE-1.0`).
- **Storage & Log Footprint**: Dự báo dung lượng lưu trữ tăng/giảm (MB/GB mỗi ngày).
- **Worker Queue Capacity**: Tải lượng công việc nền gia tăng dự kiến.

## 6. Dự báo Biến động Chỉ số (Metric Projection Delta)

Tích hợp trực tiếp với 12 metric v1 thuộc `M11-METRIC-DICT-1.0`:

| Metric ID v1 | Công thức mô phỏng | Mức độ tin cậy ước tính | Điều kiện suy giảm (`Degraded`) |
|---|---|---|---|
| `MET-OPS-HEALTHY-CAP` | Tỷ lệ slice capability dự kiến giữ trạng thái Healthy dựa trên ngưỡng timeout/retry mới | High (nếu M12 adapter online) | Thâm hụt tín hiệu health từ M12 |
| `MET-SEC-AUTH-ANOMALY` | Số sự kiện bất thường dự báo tăng/giảm theo ngưỡng abuse rate limit mới | Medium | M01 security ruleset chưa cập nhật |
| `MET-M06-RECON-DIFF` | Rủi ro sai lệch tài sản khi thay đổi hạn mức giao dịch | High | M06 reconcile service không phản hồi |
| `MET-INC-OPEN` | Số lượng sự cố dự báo phát sinh dựa trên lịch sử thay đổi tương tự | Low / Heuristic | Không đủ dữ liệu lịch sử sự cố |
| `MET-JOB-SUCCESS` | Tỷ lệ job thành công ước tính dựa trên batch size và worker concurrency mới | High | M11 job registry thiếu telemetry |
| `MET-M01-REG-COMPLETE` | Biến động tỷ lệ hoàn thành đăng ký khi đổi quy tắc verification TTL/resend | Medium | Nguồn dữ liệu cohort đăng ký bị stale |
| `MET-M01-VERIFY-LAG` | Độ trễ xác minh ước tính theo kênh email/SMS | Medium | M10 delivery metrics thiếu watermark |
| `MET-M02-CONTENT-READY` | Số lượng mục từ đạt kiểm duyệt theo bộ quy tắc chất lượng mới | High | M02 content quality engine offline |
| `MET-M03-LEARNING-COMPLETE` | Tỷ lệ hoàn thành phiên học dự báo khi tăng/giảm `WordsPerSession` | High | M03 learning session history missing |
| `MET-M04-REVIEW-DUE` | Số lượng mục từ đến hạn ôn tập tồn đọng dự báo trong 24h kế tiếp | High | Dữ liệu tiến độ ôn tập M04 bị lệch |
| `MET-M08-MATCH-COMPLETE` | Tỷ lệ trận đấu PvP hoàn tất dự báo khi đổi matchmaking timeout | Medium | M08 battle telemetry unavailable |
| `MET-M10-DELIVERY` | Khả năng đáp ứng lưu lượng gửi tin nhắn dựa trên quota provider mới | High | M12 provider status unknown |

## 7. Xử lý Không chắc chắn và Suy giảm (Uncertainty & Bounded Degradation)

Khi mô phỏng gặp dữ liệu không đầy đủ, áp dụng các quy tắc sau:
1. **Bounded Uncertainty Range**: Trả về khoảng dự báo $[Min, Max]$ thay vì 1 con số đơn lẻ.
2. **Explicit Uncertainty Reason**: Ghi rõ nguyên nhân (vd: `RECON_SERVICE_OFFLINE`, `STALE_METRIC_WATERMARK_EXCEEDED_2H`).
3. **No False Green Policy**: Tuyệt đối không đánh dấu simulation là `valid_clean` nếu có ít nhất 1 nguồn dữ liệu bị suy giảm.
4. **Manual Approval Mandatory**: Bắt buộc yêu cầu quản trị viên xác nhận thủ công nếu `evaluationState == uncertainty_bounded` hoặc `source_stale_degraded`.

## 8. Bảo mật, Quyền và Che Dữ liệu (Privacy & Redaction)

- **Quyền thực thi Preview**: Yêu cầu quyền `M11-PERM-CHANGE-PREVIEW` (gắn với các vai trò R01, R02, R08, R12).
- **Loại bỏ Secret & Token**: Các cấu hình chứa API key, connection string, password salt bắt buộc phải hiển thị dưới dạng `[REDACTED_SECRET]`.
- **Áp chế Cohort nhỏ ($k < 10$)**: Nếu mô phỏng tác động lên nhóm người dùng có quy mô $< 10$ cá thể, kết quả danh sách thực thể phải bị suppress và chuyển thành thông số tổng hợp `[SUPPRESSED_SMALL_COHORT]`.

## 9. Quy trình tích hợp vào Change Decision (D-040 / T009)

1. **Khởi tạo Request**: Admin tạo Change Request Revision mới (`changeRevisionId`).
2. **Chạy Mô phỏng**: Admin gọi endpoint Preview. Hệ thống thực thi Deterministic Validation -> Impact Projection -> Metric Delta -> Lưu `ChangePreviewResult` và cấp `previewToken` (TTL 15 phút).
3. **Gửi Phê duyệt**: Admin gửi Yêu cầu Phê duyệt kèm `previewToken`.
4. **Xác minh Token trước Decision**: Hệ thống kiểm tra:
   - `previewToken` còn hiệu lực (`expiresAtUtc > UtcNow`).
   - `previewDigest` khớp chính xác với `revisionDigest` hiện tại của Change Request.
   - Trạng thái `evaluationState` thuộc danh sách được phép.
5. **Ghi nhận Decision**: Nếu xác minh đạt, `previewToken` được khóa cố định vào `ChangeDecisionRecord`.

## 10. Regression Gate và Case tự kiểm

### 10.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `PV-G01` | Mô phỏng là read-only 100%, không phát sinh side effect hay ghi DB nghiệp vụ. |
| `PV-G02` | Khóa chính xác `changeRevisionId`, `revisionDigest`, `configSetVersionId`, `metricVersion` và `snapshotAtUtc`. |
| `PV-G03` | Phân tách rõ ràng Deterministic Validation, Impact Projection và Runtime Verification. |
| `PV-G04` | Mọi nguồn dữ liệu thiếu/stale/partial/unknown đều chuyển sang `uncertainty_bounded` hoặc `degraded`, không trả 0/green. |
| `PV-G05` | Dự báo biến động bao phủ đầy đủ 12 metric v1 thuộc `M11-METRIC-DICT-1.0`. |
| `PV-G06` | PII người dùng, secret, token được che giấu; cohort $< 10$ bị áp chế hiển thị. |
| `PV-G07` | `previewToken` có TTL 15 phút và được xác minh digest tính toán trước khi bind vào Decision. |
| `PV-G08` | Thay đổi cấu hình hoặc Change Request Revision làm vô hiệu hóa lập tức các `previewToken` cũ liên quan. |
| `PV-G09` | Tổng hợp vi sản Before/After chuẩn JSON-patch kèm phân loại mức độ rủi ro. |
| `PV-G10` | Toàn bộ case kiểm thử tự động PV15-01–20 đạt 100% trong bộ test suite. |

### 10.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PV15-01` | Mô phỏng Change Request hợp lệ với đầy đủ nguồn dữ liệu | Trả về `valid_clean`, cấp `previewToken` TTL 15m |
| `PV15-02` | Vi phạm quy tắc cứng CV-01 (MaxGroupSize out of range) | Trả về `validation_failed`, không chạy Impact Projection |
| `PV15-03` | Nguồn metric M12 offline khi đang chạy mô phỏng | Trả về `uncertainty_bounded`, gắn lý do `M12_METRIC_OFFLINE` |
| `PV15-04` | Nguồn tiến độ học tập M04 bị stale vượt 2h | Trả về `source_stale_degraded`, cấm tự động duyệt |
| `PV15-05` | Thử dùng `previewToken` đã hết hạn 15 phút để tạo Decision | Hợp đồng Decision reject token với lỗi `PREVIEW_TOKEN_EXPIRED` |
| `PV15-06` | Change Request Revision bị chỉnh sửa sau khi đã sinh Preview | `previewToken` cũ bị invalidate do mismatch `revisionDigest` |
| `PV15-07` | Mô phỏng cấu hình chứa Secret connection string | Trường Secret hiển thị `[REDACTED_SECRET]` trong Diff |
| `PV15-08` | Mô phỏng tác động ảnh hưởng đến cohort 5 người dùng | Danh sách người dùng bị chuyển thành `[SUPPRESSED_SMALL_COHORT]` |
| `PV15-09` | Thay đổi `WordsPerSession` từ 10 lên 20 | Metric delta `MET-M03-LEARNING-COMPLETE` và `MET-M04-REVIEW-DUE` dự báo đúng hướng |
| `PV15-10` | Mô phỏng thay đổi rate limit API | Ngân sách tiêu thụ quota dự báo chính xác % biến động |
| `PV15-11` | Yêu cầu mô phỏng không truyền `metricVersion` | Reject request với lỗi `MISSING_METRIC_VERSION` |
| `PV15-12` | Yêu cầu mô phỏng do User không có quyền `M11-PERM-CHANGE-PREVIEW` | Reject với 403 Forbidden |
| `PV15-13` | Mô phỏng 2 Change Request cùng lúc không gây lock contention DB | Read-only calculation chạy song song thành công |
| `PV15-14` | Kiểm tra tính bất biến của `ChangePreviewResult` đã lưu | API deny mọi thao tác Update/Delete trên Result record |
| `PV15-15` | Nguồn dữ liệu Redis cache bị rỗng | Tự động đọc từ SQL durable store và gắn warning độ trễ |
| `PV15-16` | Mô phỏng thay đổi lịch `effectiveFromUtc` trùng xung đột | Trả về `validation_failed` do xung đột lịch T010 |
| `PV15-17` | Mô phỏng thay đổi `LogRetentionDays` từ 30 lên 90 ngày | Dự báo dung lượng DB tăng 3x trong phần Budget Consumption |
| `PV15-18` | Thay đổi cấu hình không ảnh hưởng metric nào | Metric projection delta trả về danh sách trống với status `NO_METRIC_IMPACT` |
| `PV15-19` | Thử ghi dữ liệu DB trong hàm Preview service | DB transaction rollback và ném exception vi phạm Read-Only |
| `PV15-20` | Kiểm thử tải 100 request preview đồng thời | Thời gian phản hồi p95 $< 500\text{ms}$, không rò rỉ bộ nhớ |

## 11. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-PV-I01` | Chưa có DTO, Controller hay Service cho Change Preview & Simulation trong `WordSoulApi` | Thiếu toàn bộ mô hình và engine xem trước thay đổi | M11-T049 |
| `M11-PV-I02` | Backend hiện tại chỉ cập nhật trực tiếp `SystemConfiguration` mutable row | Không có bước Deterministic Validation hay Impact Projection | M11-T049 |
| `M11-PV-I03` | Thiếu cơ chế phát hành và kiểm tra `previewToken` có TTL 15m | Quyết định thay đổi không được khóa vào kết quả xem trước | M11-T049 |
| `M11-PV-I04` | Thiếu tích hợp dự báo biến động metric với `M11-METRIC-DICT-1.0` | Không thể đánh giá rủi ro chỉ số trước khi apply cấu hình | M11-T049 |
| `M11-PV-I05` | Chưa có cơ chế PII redaction và cohort suppression ($k < 10$) trong API xem trước | Rủi ro rò rỉ dữ liệu cá nhân khi xem trước tác động | M11-T049 |
| `M11-PV-I06` | Chưa có bảng lưu trữ bất biến `ChangePreviewResult` và `PreviewTokenLease` | Không thể truy vết bằng chứng xem trước phục vụ kiểm toán | M11-T049 |

- `M11-PV-F01`: Triển khai `ChangePreviewService` và API endpoints cho M11 (tiếp nhận: M11-T049).
- `M11-PV-F02`: Xây dựng engine tính toán Vi sai (Diff Engine) và Ngân sách/Hạn mức (tiếp nhận: M11-T049).
- `M11-PV-F03`: Triển khai tích hợp dự báo 12 metric v1 thuộc `M11-METRIC-DICT-1.0` (tiếp nhận: M11-T049).
- `M11-PV-F04`: Thiết lập bộ kiểm thử tự động PV-G01–G10 và PV15-01–20 (tiếp nhận: M11-T049).
- `M11-PV-F05`: Thu thập bằng chứng runtime verification cho toàn bộ luồng Preview -> Decision -> Execution (tiếp nhận: M11-T049; A-G02/A-G06).

## 12. Tự kiểm M11-T015

- Đã thiết kế hoàn chỉnh `M11-PREVIEW-1.0` ràng buộc chặt chẽ với `changeRevisionId`, `setVersionId`, `metricVersion` và `snapshotAtUtc`.
- Đã phân tách rõ ràng 3 tầng: Deterministic Validation, Impact Projection và Runtime Verification.
- Đã xác lập nguyên tắc Fail-Closed & Bounded Uncertainty khi nguồn dữ liệu bị thiếu/stale/unknown.
- Bao phủ đầy đủ dự báo biến động cho 12 metric v1 thuộc `M11-METRIC-DICT-1.0`.
- Đã chốt cơ chế `previewToken` có TTL 15 phút, quy tắc che giấu bí mật/PII và áp chế cohort nhỏ ($k < 10$).
- Đã thiết lập 10 Regression Gates (`PV-G01`–`PV-G10`) và 20 Test Cases tự kiểm (`PV15-01`–`PV15-20`).
- Ghi nhận 6 quan sát sai lệch tĩnh và 5 finding tiếp nhận cho các task sau.

## 13. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế xem trước và mô phỏng tác động M11-T015 | WSA-7K2 |
