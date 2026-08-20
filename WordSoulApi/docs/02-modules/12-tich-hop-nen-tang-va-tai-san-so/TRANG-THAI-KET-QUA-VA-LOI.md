# Trạng thái kết quả và lỗi chuẩn M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T005 |
| Taxonomy ID / phiên bản | M12-RESULT-1.0 |
| Trạng thái | Baseline semantics có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-018, D-020–D-022; M12-CONTRACT-1.0; M12-D004, D019, D024 |
| Phạm vi | Status, reason category, finality, data, retry, reconcile và mapping provider outcome |
| Ngoài phạm vi | Số lần/backoff/timeout cụ thể (M12-T036–T037); delivery lifecycle riêng từng kênh; UX copy |

## 1. Nguyên tắc

1. `status` trả lời **operation đang/kết thúc thế nào**; `reasonCategory` trả lời **vì sao ở trạng thái đó**. Không thay một trục bằng trục kia.
2. Chỉ tám status trong tài liệu này được đi qua M12-CONTRACT-1.0. `null`, boolean, HTTP status, exception, SDK enum và provider text không phải status.
3. `unknown` là trạng thái an toàn khi chưa biết effect; không tự đổi thành failure để retry và không đổi thành success để tiếp tục journey.
4. `noData` là kết quả hợp lệ, không phải outage. Nó chỉ dùng khi provider/capability đã trả lời xác định và use case cho phép rỗng.
5. Retry giữ nguyên operation ID/fingerprint. Operation mới chỉ dành cho business intent mới sau khi operation cũ đã final hoặc policy cho phép rõ.
6. Reason/category được log theo allowlist; message/payload/recipient/token/URL riêng tư của provider không đi kèm.

## 2. Taxonomy status

| Status | Finality | Effect được xác định | `data/reference` | Retry cùng operation | Reconcile | Consumer bắt buộc |
|---|---|---|---|---|---|---|
| `succeeded` | Final | Có, đáp ứng contract | Bắt buộc nếu schema yêu cầu; có thể chỉ reference | Không | Chỉ đối soát định kỳ/incident | Áp business effect idempotently một lần |
| `noData` | Final | Có, xác định không có kết quả | Không; metadata tối thiểu được phép | Không | Không mặc định | Thực hiện nhánh empty hợp lệ; không hiển thị lỗi provider |
| `unknown` | Non-final | **Không biết** đã xảy ra hay chưa | Không có business data để áp | Không retry mù; chỉ policy sau reconcile | Bắt buộc cho mutation/dispatch có thể đã nhận | Giữ pending/unknown, không cấp bù hoặc báo success/failure chắc chắn |
| `temporaryFailure` | Non-final trong deadline/budget | Xác định attempt không tạo effect có thể dùng, hoặc retry đã được chứng minh an toàn | Không | Có điều kiện bởi T036–T037 | Khi adapter không chứng minh no-effect thì phải dùng `unknown` thay vì status này | Retry giới hạn hoặc suy giảm; không tự vòng lặp |
| `permanentFailure` | Final cho payload/destination hiện tại | Xác định không có effect hợp lệ để áp | Không | Không với request không đổi | Không mặc định | Kết thúc an toàn; cần sửa intent/input/destination trước operation mới |
| `expired` | Final cho intent hiện tại | Kết quả không còn được phép áp; remote effect phải đã xác định hoặc theo dõi riêng | Không | Không | Bắt buộc nếu remote effect còn chưa biết—khi đó operation vẫn `unknown`, không `expired` | Không áp late result; intent mới cần operation mới theo policy |
| `cancelled` | Final cho intent hiện tại | Không có effect được phép áp và uncertainty đã xử lý | Không | Không | Nếu remote cancellation/effect chưa chắc chắn thì giữ `unknown` | Dừng dispatch/apply; không xóa history |
| `rejected` | Final, trước effect | Capability/policy từ chối trước dispatch hoặc xác định không thực thi | Không | Không nếu request/policy không đổi | Không | Không coi là outage; sửa quyền/purpose/policy/input trước intent mới |

### Phân biệt các cặp dễ nhầm

| Cặp | Quy tắc quyết định |
|---|---|
| `noData` / `permanentFailure` | Request hợp lệ và empty là outcome trong contract → `noData`; request/destination không thể hoàn tất → `permanentFailure` |
| `temporaryFailure` / `unknown` | Chứng minh attempt không tạo remote effect và có thể thử lại → temporary; mất response sau điểm có thể commit → `unknown` |
| `rejected` / `permanentFailure` | Chặn trước dispatch bởi contract/policy/control → rejected; đã xử lý và xác định payload/destination thất bại cuối → permanent |
| `expired` / `unknown` | Deadline qua và chắc chắn không có effect được áp → expired; effect remote còn chưa biết → unknown kèm reason deadline elapsed |
| `cancelled` / `unknown` | Cancellation đã bảo đảm semantics cuối → cancelled; cancellation chỉ là tín hiệu và remote effect chưa biết → unknown |
| `succeeded` / provider accepted | Chỉ succeeded khi contract/effect yêu cầu đã xác định; accepted/queued chỉ là lifecycle detail hoặc non-final operation state |

## 3. Reason category chuẩn

Reason code dạng `GROUP.CODE`, ổn định và không chứa vendor. Status cuối được quyết theo timing/effect/deadline, không suy chỉ từ reason.

| Group | Reason code | Ý nghĩa an toàn | Status thường gặp | Retry mặc định |
|---|---|---|---|---|
| Contract | `CONTRACT.INVALID_INPUT` | Thiếu/sai field/schema allowlist | `rejected` | Không |
| Contract | `CONTRACT.VERSION_UNSUPPORTED` | Major/version không hỗ trợ | `rejected` | Không với version cũ |
| Contract | `CONTRACT.IDEMPOTENCY_CONFLICT` | Cùng operation ID, khác fingerprint | `rejected` | Không; audit |
| Policy | `POLICY.PURPOSE_NOT_ALLOWED` | Purpose/use case không được phép | `rejected` | Không |
| Policy | `POLICY.CONSENT_REQUIRED` | Thiếu/stale/không khớp consent/policy ref | `rejected` | Không đến khi có quyết định hợp lệ |
| Policy | `POLICY.CAPABILITY_DISABLED` | Capability C3/kill switch/activation không cho phép | `rejected` | Không |
| Access | `ACCESS.UNAUTHENTICATED` | Workload/actor không xác thực | `rejected` | Không với cùng credential state |
| Access | `ACCESS.FORBIDDEN` | Không có quyền cho operation/asset/destination | `rejected` | Không |
| Control | `CONTROL.RATE_LIMITED` | Product limiter từ chối/throttle | `temporaryFailure`, `rejected` hoặc `expired` | Chỉ sau `notBefore` và trong budget |
| Control | `CONTROL.QUOTA_EXHAUSTED` | Quota/cost budget đã chạm trần | `temporaryFailure`/`rejected` | Không nếu quota không hồi trong deadline |
| Control | `CONTROL.SAFETY_UNAVAILABLE` | Control C0 như limiter/policy/audit không chắc chắn | `rejected`/`temporaryFailure` | Không fail-open; chỉ khi control healthy |
| Dependency | `DEPENDENCY.UNAVAILABLE` | Dependency không đáp ứng contract | `temporaryFailure` | Có điều kiện |
| Dependency | `DEPENDENCY.TIMEOUT` | Attempt hết ngân sách chờ | `temporaryFailure` hoặc `unknown` | Chỉ khi chứng minh no-effect |
| Dependency | `DEPENDENCY.THROTTLED` | Provider/account throttle | `temporaryFailure`/`expired` | Có điều kiện; không expose quota thô |
| Dependency | `DEPENDENCY.RESPONSE_INVALID` | Response sai schema/semantics | `temporaryFailure`/`permanentFailure` | Theo mapping/version/circuit policy |
| Dependency | `DEPENDENCY.REJECTED` | Provider từ chối xác định | `permanentFailure` hoặc `temporaryFailure` | Theo nguyên nhân đã map, không theo text |
| Destination | `DESTINATION.INVALID` | Recipient/device/asset destination cuối | `permanentFailure` | Không; revoke/sửa destination |
| Destination | `DESTINATION.GONE` | Đích không còn tồn tại/được phép | `permanentFailure`/`noData` theo contract | Không |
| Data | `DATA.NO_RESULT` | Xác định không có kết quả và empty được phép | `noData` | Không |
| Data | `DATA.QUALITY_REJECTED` | Kết quả không vượt validation/quality gate | `permanentFailure` hoặc `noData` nếu contract định nghĩa | Không tự publish/apply |
| Operation | `OPERATION.RESULT_UNCERTAIN` | Mất response/partial state, effect chưa biết | `unknown` | Reconcile trước |
| Operation | `OPERATION.DEADLINE_ELAPSED` | Business deadline đã qua | `expired` hoặc `unknown` | Không retry |
| Operation | `OPERATION.CANCELLED` | Intent bị hủy với semantics đã xác định | `cancelled` | Không |
| Operation | `OPERATION.LATE_RESULT` | Result/event tới sau final/deadline | Không đổi result cuối; operational event | Không; reconcile/audit |
| Security | `SECURITY.CALLBACK_INVALID` | Signature/replay/source callback không hợp lệ | `rejected` tại inbound boundary | Không; audit/alert theo mức |
| Internal | `INTERNAL.FAILURE` | M12 lỗi nội bộ đã redact | `temporaryFailure` hoặc `unknown` | Chỉ nếu no-effect được chứng minh |

Không dùng `OTHER` để cho phép fallback tùy ý. Reason chưa map dùng `INTERNAL.FAILURE` hoặc `DEPENDENCY.RESPONSE_INVALID` theo boundary, giữ diagnostic protected và mở finding; status vẫn theo effect/finality an toàn.

## 4. Quyết định status theo thứ tự

1. **Trước dispatch:** validate version, schema, operation/fingerprint, actor, purpose, consent, activation, criticality và control. Không đạt → `rejected` với reason tương ứng.
2. **Đã có kết quả hợp lệ:** effect/data đáp ứng contract → `succeeded`; empty được phép và đã xác định → `noData`.
3. **Có thất bại xác định không tạo effect dùng được:** nếu sửa request/destination mới giải quyết → `permanentFailure`; nếu có thể retry an toàn trong deadline/budget → `temporaryFailure`.
4. **Có thể provider đã nhận/commit nhưng không biết:** → `unknown`, không xét exception type/HTTP code để hạ thành temporary.
5. **Deadline/cancellation:** chỉ `expired`/`cancelled` khi uncertainty đã đóng; nếu remote effect chưa biết, giữ `unknown` và thêm reason/context deadline/cancel.
6. **Late response/event:** không ghi đè final result. Nếu mâu thuẫn durable truth, tạo reconcile/incident signal.

## 5. Finality và chuyển trạng thái

| Từ | Được chuyển tới | Không được chuyển trực tiếp |
|---|---|---|
| Chưa có result | Mọi status sau validation/attempt | — |
| `temporaryFailure` | `succeeded`, `noData`, `unknown`, `permanentFailure`, `expired`, `cancelled`, temporary mới có attempt mới | Không tự thành rejected sau khi đã dispatch |
| `unknown` | `succeeded`, `noData`, `permanentFailure`, `expired`, `cancelled` sau reconcile | Không tạo operation mới để né unknown; không retry mù |
| Final status | Không đổi; tạo reconcile/incident/compensation record riêng khi có mâu thuẫn | Bất kỳ status khác ghi đè history |

`rejected` là final cho request/policy context hiện tại; sau khi người dùng hoặc hệ thống tạo intent hợp lệ khác phải dùng operation ID mới. `permanentFailure` cũng final với payload/destination hiện tại, không được đổi payload dưới cùng operation ID.

## 6. Retry và reconcile advice

| Trường | Giá trị cho phép | Quy tắc |
|---|---|---|
| `retryAdvice.allowed` | boolean | Chỉ true cho temporary failure đã chứng minh retry-safe; không là lệnh retry tự động |
| `retryAdvice.notBefore` | UTC timestamp | Đã clamp theo policy/deadline; không chép raw provider header |
| `retryAdvice.budgetRemainingClass` | `none`, `low`, `available` | Không expose quota/account count; `none` buộc dừng |
| `reconcileAdvice.required` | boolean | True cho unknown mutation/dispatch hoặc mismatch |
| `reconcileAdvice.method` | allowlist enum | Query operation, inspect durable owner, await callback; không chứa provider URL/key |
| `reconcileAdvice.nextCheckAt` | UTC timestamp | Trong deadline/retention/playbook; tránh polling storm |

Consumer không tự quyết retry từ reason code. Chính sách M12-T036–T037 xét status, criticality, idempotency, deadline, retry/cost budget, circuit và destination state.

## 7. Mapping outcome tại adapter

| Provider/technical signal | Mapping bắt buộc |
|---|---|
| 2xx/SDK success với payload hợp lệ và effect đáp ứng contract | `succeeded` hoặc `noData` theo semantics, không chỉ theo transport |
| Accepted/queued | Giữ operation non-final trong durable record; chỉ final succeeded nếu contract chỉ yêu cầu acceptance và business owner đã chốt |
| 4xx | Map reason dựa trên contract/policy đã kiểm chứng; không mặc định permanent cho timeout/throttle/conflict |
| 5xx/network fail trước khi gửi chắc chắn | Temporary nếu adapter chứng minh no-effect và retry-safe; nếu không thì unknown |
| Timeout/connection reset sau khi request có thể tới provider | `unknown` + reconcile; không retry mutation/dispatch mù |
| Exception parse/response schema drift | Response invalid; circuit/canary signal; không trả raw body/text |
| Callback invalid/replay | Reject trước apply, audit category; không tạo/đổi operation |
| Duplicate callback/result giống final | Ack/deduplicate idempotently; không tạo business effect mới |
| Duplicate callback/result mâu thuẫn final | Giữ final, tạo reconcile/incident signal |

HTTP/gRPC/queue acknowledgement chỉ là transport. API có thể dùng transport error phù hợp cho request không parse được, nhưng nếu đã nhận diện operation thì response/status record vẫn dùng taxonomy này; consumer không branch semantics bằng HTTP code.

## 8. Log, metric và UX boundary

- Log/metric được phép: capability/use-case, status, reason category, criticality, duration/attempt/budget class, degraded flag và protected IDs theo policy.
- Cấm: provider response/exception text thô, email/device token, verification secret, signed URL, audio/content payload, credential và request body.
- Metric status/reason dùng cardinality hữu hạn; operation/correlation không làm metric label.
- UX không hiển thị reason code kỹ thuật trực tiếp. Module nghiệp vụ map sang thông điệp trung tính và hành động phù hợp; security/account enumeration không bị lộ.
- Operational owner có diagnostic protected riêng theo quyền/retention; diagnostic không được trả consumer hoặc sao chép vào evidence.

## 9. Ma trận kiểm thử tối thiểu

| Case ID | Tình huống | Status/reason bắt buộc |
|---|---|---|
| R05-01 | Tìm ảnh hợp lệ, xác định không có ảnh | `noData` / `DATA.NO_RESULT` |
| R05-02 | Cache miss | Kết quả cache miss theo contract, không `DEPENDENCY.UNAVAILABLE` |
| R05-03 | Redis/cache thật sự lỗi | `temporaryFailure` hoặc degradation đã đăng ký; không giả miss |
| R05-04 | Email provider accepted nhưng chưa delivery | Không báo delivered; lifecycle non-final theo contract kênh |
| R05-05 | Timeout upload sau khi bytes có thể đã commit | `unknown` / `OPERATION.RESULT_UNCERTAIN`, reconcile cùng operation |
| R05-06 | Timeout trước dispatch được chứng minh | `temporaryFailure` / `DEPENDENCY.TIMEOUT`, retry advice theo policy |
| R05-07 | Cùng operation khác fingerprint | `rejected` / `CONTRACT.IDEMPOTENCY_CONFLICT` |
| R05-08 | Capability bị D-010 tắt | `rejected` / `POLICY.CAPABILITY_DISABLED`, không provider traffic |
| R05-09 | Limiter C0 không chắc chắn | `rejected` hoặc temporary / `CONTROL.SAFETY_UNAVAILABLE`, không allow-all |
| R05-10 | Device token xác định invalid | `permanentFailure` / `DESTINATION.INVALID`, revoke endpoint; không retry |
| R05-11 | Response success nhưng schema sai | Không succeeded; `DEPENDENCY.RESPONSE_INVALID` |
| R05-12 | Late callback mâu thuẫn final | Final không đổi; `OPERATION.LATE_RESULT` + reconcile/incident |
| R05-13 | Cancel signal nhưng effect remote chưa biết | `unknown`, không `cancelled` cho đến reconcile |
| R05-14 | Provider exception có PII/secret giả | Result chỉ category; log/evidence không chứa text/payload |

## 10. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M12-RES-F01 | Adapter hiện dùng null/Task/exception/provider code không đồng nhất | Không mở caller mới trước mapping; unknown khi effect chưa chắc | M12-T006–T030, T048 |
| M12-RES-F02 | Retry/deadline/cancellation/idempotency runtime chưa chuẩn | Chỉ temporary đã chứng minh no-effect mới được retry có điều kiện | M12-T036–T037 |
| M12-RES-F03 | Circuit/bulkhead/fail mode chưa gắn status/reason | Không biến dependency failure thành allow-all/success | M12-T035, T038 |
| M12-RES-F04 | Delivery lifecycle accepted/delivered/bounce/complaint chưa là schema con | Accepted không đồng nghĩa delivered; retry cùng message operation | M12-T026–T030 |
| M12-RES-F05 | Chưa có contract tests/canary cho mapping từng provider | Taxonomy là baseline thiết kế, chưa chứng minh runtime | M12-T047-A–T048; REL-03 |

## 11. Tự kiểm M12-T005, A-G04 và REL-03

- Đủ tám status hữu hạn, finality, data/reference, retry, reconcile và hành vi consumer; sáu cặp dễ nhầm có quy tắc riêng.
- 26 reason code thuộc 9 group độc lập provider, có status thường gặp nhưng không ép mapping sai effect/deadline.
- Unknown được giữ khi remote effect có thể đã xảy ra; final result không bị late/duplicate result ghi đè; noData không bị đồng nhất outage.
- A-G04 có baseline cho fail-safe status, limiter/control unavailable, dependency degradation và 14 case kiểm thử; runtime policy/evidence vẫn chờ T035–T038/T047-A.
- Log/metric/UX boundary dùng allowlist và không đưa provider payload/PII/secret qua consumer.
- REL-03 vẫn mở vì adapter mapping, retry/reconcile store, contract tests và canary chưa được triển khai/kiểm chứng.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 8 status, 26 reason code, finality/transition, retry/reconcile và 14 case | WSA-7K2 |
