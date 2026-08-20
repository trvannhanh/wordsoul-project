# Hợp đồng dữ liệu chuẩn tích hợp M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T004 |
| Contract ID / phiên bản | M12-CONTRACT-1.0 |
| Trạng thái | Baseline schema và semantics có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-007, D-008, D-018–D-021; M12-D003–D004; M12-D019; M12-D028 |
| Phạm vi | Request, result, attempt nội bộ và inbound event cho mọi capability M12 |
| Ngoài phạm vi | Danh mục status/error chi tiết (M12-T005); timeout/retry/idempotency runtime (M12-T036–T037); provider-specific mapping |

## 1. Bất biến boundary

1. Module nguồn gọi **capability/use case**, không gọi provider. Tên provider, SDK type, HTTP status, exception text và payload thô không xuất hiện trong contract consumer.
2. Module nguồn tạo `operationId` cho một business intent và giữ nguyên qua retry/provider switch. M12 tạo `attemptId`; `correlationId` chỉ để truy vết.
3. Cùng `operationId` phải có cùng `requestFingerprint`. Khác fingerprint là conflict, không phải operation mới hoặc retry.
4. `deadlineAt` là thời điểm tuyệt đối theo UTC; timeout kỹ thuật của attempt không được suy thành effect chưa xảy ra.
5. Chỉ field có trong allowlist của schema/use case được truyền. Field vắng mặt khác `null`; `null` chỉ hợp lệ khi schema của field cho phép rõ.
6. `data` hoặc `reference` chỉ có khi status cho phép. Provider accepted/queued không tự trở thành business success.
7. Secret, credential, raw access token, password, verification code, signed private URL và provider payload không được nằm trong envelope, fingerprint, log hoặc evidence.
8. Criticality/purpose do module nguồn khai báo theo mã đăng ký và M12 đối chiếu registry; client bên ngoài không được tự chọn để né policy.

## 2. Mô hình contract

```text
Source module
  -> CapabilityRequest<TInput>
  -> M12 policy + adapter boundary
  -> ProviderRequest (nội bộ, tối thiểu, không bền vững mặc định)
  <- ProviderOutcome (nội bộ, phải ánh xạ/redact)
  <- CapabilityResult<TData|TReference>
```

Mỗi capability có một schema con versioned cho `input`, `data` và `reference`, nhưng luôn dùng cùng envelope. Adapter có thể đổi provider mà không đổi semantics consumer; nếu semantics đổi thì phải tăng contract version.

## 3. Envelope yêu cầu

| Field | Kiểu / bắt buộc | Chủ tạo | Ràng buộc |
|---|---|---|---|
| `contractVersion` | string / có | Source SDK/server | Mã allowlist dạng `m12.<capability>.<major>.<minor>`; không phải provider API version |
| `capabilityId` | string / có | Source server | Một `CAP-001`–`CAP-015` trong registry đang hiệu lực |
| `useCaseId` | string / có | Source server | Mã lát đã đăng ký; quyết định criticality, purpose, policy và schema con |
| `operationId` | opaque UUID/ULID / có | Source module | Duy nhất cho intent; không chứa user ID/email; bất biến qua retry |
| `requestFingerprint` | keyed digest / có | Source module library | HMAC/digest có khóa của canonical semantic fields; không dùng hash trực tiếp trường PII miền nhỏ; thuật toán/key-version là metadata quản trị |
| `sourceModule` | enum / có | Source server | Module owner, không nhận từ client ngoài như trust truth |
| `actorRef` | protected opaque ref / tùy lát | Source server | Ref chủ thể/workload đã xác thực; không phải email, device token hoặc provider subject thô |
| `purposeCode` | enum / có | Source server | Purpose allowlist gắn data map/retention/consent; cấm free text |
| `criticality` | C0–C3 / có | Source server + M12 validate | Phải khớp M12-CRIT-1.0; mismatch bị từ chối và audit |
| `requestedAt` | UTC timestamp / có | Source server | Thời điểm server; không dùng client clock làm truth |
| `deadlineAt` | UTC timestamp / có | Business owner | Sau hạn không áp dụng late result; phải lớn hơn `requestedAt` trong policy |
| `policyContext` | object/ref / theo lát | Source server | Chỉ policy/consent/legal-basis/version refs tối thiểu; không chép toàn bộ consent record |
| `input` | typed object / có | Source server | Chỉ schema con allowlist; validation trước dispatch |
| `assetRefs` | array protected ref / tùy lát | Source module | Asset ID/version/owner class; không dùng URL làm ownership truth |
| `traceContext` | object / tùy chọn | Platform | Correlation/tracing IDs không chứa business data; không quyết idempotency |

### Canonical fingerprint

- Schema con công bố danh sách semantic fields, thứ tự canonical, chuẩn Unicode, biểu diễn timestamp/number và cách phân biệt absent/null.
- Loại bỏ field quan sát (`traceContext`, timestamps do platform tạo), attempt/provider route và field bị cấm trước khi tạo fingerprint; trường PII hợp lệ phải được token hóa hoặc đưa qua keyed digest để không thể dò từ hash phổ thông.
- Không tự trim/case-fold dữ liệu nếu semantics use case không cho phép. Thay đổi canonicalization phải tăng version fingerprint và có migration/dual-read rõ.
- M12 lưu fingerprint cùng operation record đủ lâu theo idempotency/retention của use case; không lưu raw request chỉ để đối chiếu.

## 4. Envelope kết quả consumer

| Field | Kiểu / bắt buộc | Semantics |
|---|---|---|
| `contractVersion` | string / có | Version đã thực thi; consumer phải kiểm tra major hỗ trợ |
| `capabilityId`, `useCaseId` | string / có | Khớp request, không phản ánh provider route |
| `operationId` | opaque ID / có | Khớp intent nguồn |
| `attemptId` | opaque ID / có khi đã dispatch | Attempt tạo ra result hiện tại; không dùng để áp effect nghiệp vụ |
| `correlationId` | opaque ID / có | Truy vết; không chứa/reforge business identity |
| `status` | enum / có | Taxonomy được chốt tại M12-T005; không dùng boolean/null/HTTP status thay thế |
| `reasonCategory` | enum an toàn / theo status | Category ổn định, độc lập provider; không chứa message/response thô |
| `data` | typed object / có điều kiện | Dữ liệu tối thiểu đã validate; chỉ dùng cho status/schema cho phép |
| `reference` | typed protected ref / có điều kiện | Asset/message/operation ref; một URL không đủ làm ownership truth |
| `retryAdvice` | object / tùy status | Chỉ hint đã policy hóa (`allowed`, `notBefore`, `budgetRemainingClass`); consumer không tự retry theo provider header |
| `reconcileAdvice` | object / tùy status | Cho biết cần/được query bằng cùng `operationId`; không chứa provider lookup key |
| `completedAt` | UTC timestamp / theo status | Thời điểm capability xác định result; late result vẫn không được áp sau deadline |
| `observability` | metadata object / có | Duration class, attempt count, degraded flag; không có PII/secret/provider payload |

Ràng buộc payload:

- `data` và `reference` là one-of theo schema use case; không trả object rỗng để giả có kết quả.
- Result cuối của một operation là đơn điệu. Event/response muộn không ghi đè result cuối; sai khác đi vào reconcile/incident.
- Consumer áp result theo `operationId` một cách idempotent và tự quyết business state. M12 không tự publish, chấm đạt, cấp thưởng hoặc liên kết account.
- Chi tiết provider chỉ ở operational record có quyền/retention/redaction riêng; consumer có thể nhận `providerClass` tổng quát cho quản trị nếu contract cho phép, không nhận vendor identity để branch logic.

## 5. Attempt record nội bộ M12

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `attemptId`, `operationId` | Có | Attempt mới, operation cũ; liên kết không đổi |
| `attemptNumber` | Có | Tăng đơn điệu trong operation; không phải retry budget truth duy nhất |
| `routeRef` | Có | Ref nội bộ tới provider/config version; không trả consumer |
| `startedAt`, `attemptDeadlineAt`, `endedAt` | Theo trạng thái | UTC server time; attempt deadline không vượt business deadline |
| `outcomeCategory` | Có khi kết thúc | Provider outcome đã map; payload/exception thô không persist mặc định |
| `redactedDiagnosticRef` | Tùy chọn | Ref tới diagnostic có kiểm soát; không chép diagnostic vào result/log phổ thông |
| `costClass`, `quotaClass` | Tùy capability | Bucket/định mức tổng quát; không lộ account/quota provider cho consumer |

Attempt record phục vụ retry, reconcile, cost và incident; nó không là source of truth cho domain effect. Retention và quyền truy cập được chốt cùng M12-T042-A–T043/M11 logging.

## 6. Inbound event/callback

| Giai đoạn | Yêu cầu |
|---|---|
| Nhận | Xác minh endpoint, signature/timestamp/replay window theo provider adapter trước khi parse business fields; thiếu/không chắc chắn phải fail-closed |
| Định danh | Map provider event ID sang `inboundEventId` nội bộ và operation/message ref nếu tìm được; deduplicate không dựa vào payload text |
| Chuẩn hóa | Adapter chỉ chọn field allowlist, phân loại outcome và loại bỏ raw token/header/body khỏi consumer event |
| Áp dụng | Handler dùng operation/state bền vững, kiểm tra deadline/version và idempotency; callback không tự ghi đè truth |
| Không khớp | Giữ category + protected diagnostic ref để reconcile; không tạo operation/domain effect mới từ event mồ côi |
| Phản hồi | Provider acknowledgement chỉ xác nhận receipt/validation theo adapter; không che lỗi apply nội bộ và phải có hàng đợi/reconcile phù hợp |

Schema event chuẩn gồm `eventContractVersion`, `inboundEventId`, `capabilityId`, `eventType`, `operationId?`, `receivedAt`, `occurredAt?`, `status/reasonCategory`, `data/reference` allowlist và `correlationId`; không có provider body.

## 7. Phân loại và lưu chuyển dữ liệu

| Lớp | Ví dụ hợp lệ trong contract | Điều kiện | Log/evidence |
|---|---|---|---|
| Public | Locale, content version, public asset attribution ref | Đúng purpose/schema | Metadata allowlist |
| Internal | Capability/use-case ID, operation ID, policy version | Protected opaque ID; quyền service-to-service | Cho phép theo allowlist |
| Personal | Subject/device/recipient protected ref | Purpose + minimization + retention + owner; map external flow trước activation | Chỉ masked/aggregate hoặc protected ref |
| Sensitive | Private asset/audio ref, consent ref, auth/security intent | C0 controls, explicit access, encryption/retention/deletion | Không value thô; chỉ event/category/ref được phép |
| Secret | Password, token, key, verification code, signature secret | **Cấm trong envelope chuẩn**; adapter/secret store boundary riêng | Cấm hoàn toàn |
| Provider-native | Raw request/response/header/exception | Chỉ transient tối thiểu trong adapter; persistence cần allowlist và quyết định riêng | Cấm trong log/evidence phổ thông |

Mỗi field gửi ra ngoài phải ánh xạ tới Flow ID của M12-T042-A với purpose, provider/nơi xử lý, consent/legal basis, region, retention, deletion và owner. Chưa có mapping thì không mở traffic mới; AI sinh nội dung và audio người dùng vẫn tắt theo D-010.

## 8. Versioning và tương thích

| Thay đổi | Xử lý |
|---|---|
| Thêm field optional không đổi semantics | Tăng minor; consumer phải ignore field minor chưa biết |
| Thêm enum status/reason mà consumer phải xử lý | Major, hoặc consumer đã có nhánh `unknown-safe` được kiểm chứng |
| Đổi required/meaning/canonicalization/data class | Tăng major; migration + dual contract/canary + stop criteria |
| Provider API/model đổi nhưng semantics giữ nguyên | Không đổi consumer contract; đổi route/adapter version và kiểm thử contract |
| Xóa field | Deprecate, đo consumer, tăng major; không xóa âm thầm |

Consumer không hỗ trợ major phải từ chối trước dispatch, không parse best-effort. Version negotiation diễn ra server-to-server qua registry/deployment compatibility, không nhận phiên bản tùy ý từ public client.

## 9. Mẫu minh họa không chứa dữ liệu thật

```json
{
  "contractVersion": "m12.email-security.1.0",
  "capabilityId": "CAP-011",
  "useCaseId": "email.security-intent",
  "operationId": "op_opaque",
  "requestFingerprint": "fp_versioned_digest",
  "sourceModule": "M01",
  "actorRef": "subject_protected_ref",
  "purposeCode": "ACCOUNT_VERIFICATION",
  "criticality": "C0",
  "requestedAt": "server_utc_timestamp",
  "deadlineAt": "server_utc_timestamp",
  "policyContext": { "policyVersionRef": "policy_ref" },
  "input": { "recipientRef": "recipient_protected_ref", "templateRef": "template_version_ref" }
}
```

Ví dụ không phải payload triển khai hoàn chỉnh: verification secret được resolve ở boundary có kiểm soát, không đi trong envelope chuẩn hoặc tài liệu.

## 10. Ma trận tự kiểm contract

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| C04-01 | Cùng operation ID, cùng canonical payload | Trả result hiện có/tiếp tục cùng operation; không tạo effect mới |
| C04-02 | Cùng operation ID, khác fingerprint | Conflict an toàn + audit; không dispatch |
| C04-03 | Thiếu purpose/use case/deadline/version | Reject trước provider |
| C04-04 | Criticality khai báo thấp hơn registry | Reject + audit; không hạ control |
| C04-05 | Input có field ngoài allowlist hoặc secret | Reject toàn request, chỉ ghi category đã redact; không log value hoặc tiếp tục bằng payload đã sửa |
| C04-06 | Consumer gửi provider name/API version | Reject/ignore theo schema; route chỉ do M12 policy quyết định |
| C04-07 | Provider timeout sau mutation có thể đã commit | Result không được giả permanent failure; chuyển taxonomy unknown/reconcile của T005 |
| C04-08 | Provider trả success nhưng schema data sai | Không trả succeeded; giữ diagnostic được redact |
| C04-09 | Result đến sau deadline/cancel/final result | Không áp business effect; trace/reconcile theo operation |
| C04-10 | Callback signature sai/replay | Fail-closed trước apply; ghi category/audit không chứa body/token |
| C04-11 | Consumer không hỗ trợ major | Reject trước dispatch; không best-effort parse |
| C04-12 | Dữ liệu cá nhân chưa có Flow ID/purpose mapping | Không mở provider traffic mới |

## 11. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M12-CON-F01 | Status/reason/retry/reconcile matrix chưa chốt | Không dùng null/boolean/HTTP/exception làm taxonomy | M12-T005 |
| M12-CON-F02 | Adapter hiện tại chưa mang operation/version/purpose/deadline nhất quán | Không mở rộng caller mới qua interface raw | M12-T006–T030; M12-T036–T037 |
| M12-CON-F03 | Chưa có external Flow ID/field catalog và retention đầy đủ | Không traffic dữ liệu mới; protected refs + minimization | M12-T042-A–T043; REL-01/REL-03 |
| M12-CON-F04 | Inbound callback authentication/dedup/reconcile theo provider chưa kiểm chứng | Callback không xác thực/không khớp không được apply | M12-T026–T030, T047-A |
| M12-CON-F05 | Chưa có schema registry/code generation/compatibility tests | Contract này là baseline thiết kế, chưa là runtime enforcement | M12-T047-A–T048; REL-03 |

## 12. Tự kiểm M12-T004, A-G04, A-G05 và REL-03

- Request/result/attempt/event đã có owner, field bắt buộc, semantics và ranh giới độc lập provider; operation/attempt/correlation không bị đồng nhất.
- Contract bắt buộc version, purpose, criticality, deadline, fingerprint, protected refs và validation trước dispatch; late/unknown không tạo success giả.
- A-G04 có baseline cho idempotency, deadline, unknown/reconcile và provider isolation; runtime retry/degradation evidence vẫn chờ T005/T036–T038/T047-A.
- A-G05 có data class, field cấm, protected ref, metadata allowlist và yêu cầu Flow ID trước activation; data map/secret inventory/redaction tests vẫn chưa hoàn thành.
- 12 case tự kiểm bao phủ replay, conflict, schema drift, secret/PII, callback, late result và unsupported version.
- REL-03 vẫn mở vì chưa có schema enforcement, provider mapping, contract tests/canary và runtime evidence.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt envelope request/result/attempt/event, data classes, versioning và 12 case contract | WSA-7K2 |
