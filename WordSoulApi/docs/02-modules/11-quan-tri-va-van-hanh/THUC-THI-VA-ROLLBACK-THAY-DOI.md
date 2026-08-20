# Thực thi và rollback thay đổi quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CHANGE-EXECUTION-1.0` |
| Task | M11-T011 |
| Đầu vào | M11-CHANGE-REQUEST/DECISION/SCHEDULE-1.0, M11-ENHANCED-CONTROL-1.0, M12-CONTRACT/RESULT-1.0 |
| Quyết định nền | D-001, D-007, D-008, D-021–D-025, D-038–D-041; M11-D005–D010, D021–D023 |
| Tự kiểm | A-G02, A-G06; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Execution chỉ biến **đúng sealed revision đã được authorize** thành effect qua module-source adapter, sau đó verify kết quả thật. Rollback/recovery là operation riêng, không xóa history hay biến failure thành “chưa từng xảy ra”.

- M11 điều phối; module nguồn giữ invariant, transaction và durable business truth. Cấm direct table/ledger/history/config mutation để vượt adapter.
- Một `operationId` ổn định cho toàn execution; retry là `attempt` mới của cùng operation, không tạo business operation mới.
- Claim atomically khóa exact revision/decision/digest, reservation/fencing và lifecycle `executing` trước dispatch.
- `accepted`, HTTP 2xx, queue publish hoặc command return không phải success. Chỉ postcondition đã verify mới `succeeded`.
- Timeout/null/exception sau điểm có thể effect là `unknown`; reconcile trước mọi retry/rollback.
- Partial/unknown giữ conflict reservation và chặn overlap đến khi verified recovery/terminal outcome.
- Scheduled workload dùng fixed identity + sealed instruction, không impersonate actor, không tạo temporary/emergency privilege.

## 2. Durable record model

| Record | Identity | Nội dung bắt buộc | Quy tắc |
|---|---|---|---|
| ExecutionOperation | `operationId` | revision/decision/digests, Action ID, adapter contract, conflict keys/fencing, state/result | Một operation cho một execution intent; immutable identity |
| ExecutionAttempt | `operationId:attemptNo` | start/end, workload, deadline, request/result refs, transport/status/error taxonomy | Append-only; attempt tăng CAS |
| EffectManifest | `operationId:itemKey` | expected/actual version, effect state, source receipt/ref, verify result | Không raw payload/PII/secret; per-item durable |
| RecoveryOperation | `recoveryOperationId` + `causedByOperationId` | RB class/plan digest, target/compensation, fencing, state/result | Idempotent riêng, luôn liên kết original operation |
| ExecutionEvent | `eventId` | lifecycle transition, actor/workload, correlation, metadata allowlist | Append-only/outbox; audit/security event không được drop |

### Operation state

| State | Nghĩa |
|---|---|
| `claimed` | CAS lifecycle/reservation/fencing đã commit, chưa dispatch |
| `preflighting` | Đang kiểm current guard và adapter readiness |
| `applying` | Source adapter có thể tạo effect |
| `verifying` | Đang đọc source receipt/postcondition/metrics |
| `reconciling` | Outcome chưa chắc chắn hoặc manifest chưa đầy đủ |
| `succeeded` | Toàn scope verify đạt |
| `noEffectFailed` | Chứng minh không effect và attempt kết thúc lỗi |
| `partiallySucceeded` | Có subset effect xác định |
| `rollbackRequired` | Có effect cần RB-1/RB-2/RB-3 |
| `rollingBack` | Recovery operation đã claim |
| `rolledBack` | Recovery target verify đạt |
| `recoveryFailed` | Recovery không đạt; residual manifest + incident |

Không có `successAssumed`, `fireAndForgetDone` hoặc `unknownAsSuccess`.

## 3. Source adapter contract

| Input | Yêu cầu |
|---|---|
| `operationId`, `attemptNo`, `idempotencyKey` | Source deduplicate theo operation, không chỉ transport request |
| `revisionId`, `canonicalDigest`, `decisionId` | Exact immutable instruction; mismatch reject |
| `actionId`, `operationType`, typed parameters | Registry/version allowlist; không SQL/script/raw provider payload |
| `resource/scope refs`, `expectedVersions` | Source resolve và CAS; M11 không tin client target truth |
| `fencingToken`, conflict key digest | Source reject stale token trước effect |
| `deadlineUtc`, `cancellation/stopRef` | Deadline propagation; cancel không đồng nghĩa rollback |
| `actorContextRef`, `workloadIdentity` | Protected actor decision context + fixed executing identity; không impersonation |
| `auditIntentRef`, `correlationId` | Durable ref tồn tại trước EC-3/EC-4 effect |

| Output | Yêu cầu |
|---|---|
| `sourceOperationRef` | Stable receipt để query/reconcile |
| `resultStatus` | `succeeded`, `rejected`, `failedTemporary`, `failedPermanent`, `partial`, `unknown` theo M12-RESULT-1.0 mapping |
| `effectManifestRef` | Per-item/range state, version trước/sau allowlist |
| `committedAt` | Source durable time nếu effect commit |
| `postconditionRef` | Queryable truth/version/checksum metadata để verify |
| `retrySafe` + constraints | Source chứng minh, không để orchestrator tự suy |
| `errorCode` | Taxonomy ổn định, không exception/raw payload |

Adapter phải có `apply`, `getOutcome/reconcile`, `verify` và recovery operation đã đăng ký. Thiếu bất kỳ capability bắt buộc theo action làm preflight fail-closed.

## 4. Execution pipeline

| Phase | Hành động | Durable boundary | Failure behavior |
|---|---|---|---|
| EX-01 Claim | Recheck revision/decision validity, current actor authority, source/evidence/limit, window/conflict, audit; CAS lifecycle + reservation + fencing | Operation `claimed`, lifecycle `executing`, audit intent và outbox cùng transaction | Zero-row/conflict/expired/unknown guard: không claim/effect |
| EX-02 Preflight | Resolve adapter/version, target versions, rollback/monitor/notification plan, deadline và source health/readiness | Attempt `preflighting` + immutable input digest | Reject/unknown: `noEffectFailed`, release chỉ khi chứng minh no effect |
| EX-03 Prepare | Build bounded manifest/batches, source idempotency reservation nếu hỗ trợ | Manifest expected rows + attempt metadata | Partial prepare không effect; rollback local prepare |
| EX-04 Apply | Gọi source adapter bằng operation/fencing/deadline; checkpoint per batch | Source receipt/result + per-item manifest | Timeout/transport ambiguity → `reconciling`, không retry mù |
| EX-05 Verify | Query durable source outcome/version/postcondition và monitoring guard | Verify result/digest/dataThrough | Mismatch/partial/unknown → reconcile hoặc rollbackRequired |
| EX-06 Finalize | CAS terminal state, audit result, outbox notification, release reservation nếu safe | Lifecycle/result/audit/outbox atomic theo local boundary | Outbox delay retry same event; không đảo success truth |

Preflight được lặp lại dù request từng valid; thời gian và external state có thể thay đổi. `execute` không nhận revised parameters từ caller.

## 5. Atomic, batch và partial semantics

| Mode | Khi dùng | Contract |
|---|---|---|
| `sourceAtomic` | Một source có transaction/CAS cho toàn bounded change | All-or-no-effect receipt; timeout vẫn reconcile |
| `policySetAtomic` | Nhiều config key cùng owner phải hiệu lực nhất quán | Source tạo một version/policy-set commit, không update từng key rời |
| `checkpointedBatch` | Bulk/job/campaign tối đa theo LIM-04/LIM-07 | Immutable selector, batch ≤100, checkpoint + per-item manifest; stop ở boundary |
| `sagaCompensated` | Nhiều source/effect không atomic nhưng registry có compensation | Ordered steps, forward/compensate dependency, residual risk; không gọi là atomic |

- Partial response phải nêu processed/succeeded/failed/unknown counts và protected manifest ref; tổng phải khớp selector snapshot.
- Unknown item không được đếm failed/no-effect cho đến reconcile.
- Một item đã succeeded không apply lại; retry chỉ item proven no-effect/retry-safe bằng same operation/item identity.
- Atomic mode trả partial là contract violation, giữ block và mở incident/reconcile.

## 6. Retry và reconcile

| Outcome | Retry? | Hành vi |
|---|---|---|
| Rejected/source invariant | Không | Final no-effect failure; cần revision mới nếu input đổi |
| Permanent failure, proven no effect | Không tự retry | `noEffectFailed`, reason/finding |
| Temporary failure, proven no effect + retrySafe | Có bound | Attempt mới, same operation/fingerprint, backoff/deadline/attempt cap từ registry |
| Timeout/transport/worker loss sau dispatch | Không trước reconcile | Query `sourceOperationRef`/operation ID; state `reconciling` |
| Source says succeeded | Không apply lại | Verify postcondition rồi finalize |
| Source says partial | Không blind retry | Freeze manifest, retry only proven no-effect items nếu contract cho phép hoặc recovery |
| Source unavailable khi reconcile | Không | Giữ unknown/block, alert/escalate; không release fencing/reservation |

Attempt cap/timeout là registry data. Missing/zero/invalid values không thành infinite retry. Late outcome của attempt cũ không được ghi đè terminal result; so operation/stateVersion/fencing và lưu như late evidence.

## 7. Verification contract

| Check | Bắt buộc |
|---|---|
| Identity | Receipt/postcondition thuộc exact operation/revision/source/fencing |
| Version | Actual version đúng expected successor hoặc registered transition |
| Scope | Manifest keys/count nằm trong selector snapshot, không extra effect |
| Business postcondition | Module-source query xác nhận invariant/result, không dựa response DTO đơn thuần |
| Cross-reference | Dependent source versions/holds/consent/ledger/config consumers phù hợp |
| Monitoring | Metric source/freshness/baseline/threshold và observation window theo sealed plan |
| Audit | Intent/result/denied/error events durable, linked, redacted |
| Notification | Required recipient/result event queued idempotently; failure không phát lại effect |

Verify trả `pass`, `fail`, `partial`, `unknown`; chỉ `pass` cho toàn required checks mới succeeded. Health HTTP 200 hoặc không thấy alert không phải `pass`.

## 8. Rollback và recovery

Decision ban đầu pre-authorize đúng sealed RB plan trong revision; vì vậy trigger tự động theo stop criteria không cần người duyệt thứ hai. Nó không cấp quyền: recovery workload/action/fencing/current policy vẫn được kiểm lại. Plan ngoài digest hoặc mở rộng scope cần request/decision mới, trừ registered incident containment chỉ thu hẹp/stop effect.

| RB class | Execution | Verify target | Khi không đạt |
|---|---|---|---|
| RB-1 Version rollback | Activate exact compatible previous version bằng recovery operation/fencing | Source current version + consumer convergence + monitoring threshold | `recoveryFailed`, giữ barrier, incident/forward fix |
| RB-2 Compensation | Gọi registered inverse/ledger/job/item compensation theo immutable effect manifest | Per-item compensation result + invariant/reconcile; residual ghi rõ | Không sửa DB thủ công; residual thành finding/case |
| RB-3 Contain/forward recover | Stop/revoke/isolate/restore backup hoặc apply forward version theo sealed safeguards | Harm contained, source integrity, access/data handling và recovery milestone | Incident + barrier; không tuyên bố rolledBack nếu chỉ dừng lan rộng |

### Trigger

- Hard stop: security/data/integrity violation, extra-scope effect, stale fencing accepted, source atomicity violation → dừng mở rộng, `rollbackRequired`/incident ngay.
- Threshold stop: sealed metric vượt threshold trong observation window → stop next batch/rollout, reconcile rồi RB plan.
- Manual stop: actor có fixed incident/operation permission yêu cầu stop; orchestrator dừng ở safe checkpoint, không mark canceled.
- Recovery chỉ terminal `rolledBack` khi verification target đạt. RB-3 containment chưa khôi phục target phải ghi đúng `recoveryFailed` hoặc state incident/recovery còn mở, không success giả.

## 9. Failure matrix

| Failure point | Effect certainty | State/result | Reservation |
|---|---|---|---|
| Trước claim transaction | No effect | Giữ authorized/scheduled hoặc invalidated/expired | Không acquire/release atomic |
| Sau claim, trước adapter dispatch | Proven no effect | `noEffectFailed` hoặc retry-safe same operation | Có thể release sau durable proof |
| Sau dispatch, không receipt | Unknown | `reconciling` | Giữ claimed/block |
| Receipt reject | Proven no effect | `noEffectFailed` | Release sau finalize |
| Receipt success, verify unknown | Effect likely/unknown correctness | `reconciling` | Giữ block |
| Verify partial/fail có effect | Known partial/harm | `partiallySucceeded`/`rollbackRequired` | Giữ block |
| Finalize DB lỗi sau source success | Source effect known, local state unknown | Reconcile by operation ID; không reapply | Giữ/reacquire bằng durable fencing state |
| Recovery timeout | Unknown recovery | Reconcile recoveryOperationId | Giữ barrier |
| Audit store unavailable trước effect | No effect | Fail-closed | Không claim/apply |
| Notification/outbox delivery lỗi | Effect/result không đổi | Retry event idempotently | Release theo business recovery, không chờ gửi vô hạn |

## 10. Security và dữ liệu

- Actor context chỉ protected IDs/permission refs; scheduled worker không mint user session/token hoặc gọi downstream như actor.
- Request/attempt/manifest/audit cấm password/token/code/secret value/raw audio/provider payload/full PII; before/after dùng field allowlist, digest và protected ref.
- Source error/exception được map taxonomy; response/log không echo payload hoặc connection detail.
- Read quyền cao không suy execution/recovery permission. Recovery Action ID và scope phải explicit.
- Internal header, loopback, CLI, scheduler và admin API dùng cùng operation/permission/limit/audit contract; không bypass limiter/CT-05.

## 11. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CE-G01 | Exact revision/decision/digests bind tới một stable operation; caller không đổi parameters lúc execute |
| CE-G02 | Claim/lifecycle/reservation/fencing/audit intent/outbox có atomic local boundary và CAS |
| CE-G03 | Source adapter deduplicate operation, enforce expected version/fencing và giữ invariant |
| CE-G04 | Apply response không tự thành success; postcondition verify bắt buộc |
| CE-G05 | Timeout/unknown/partial không retry mù, không release overlap block, không success giả |
| CE-G06 | Retry chỉ proven no-effect + retrySafe, bounded, same operation/item identity |
| CE-G07 | RB-1/2/3 dùng sealed plan, recovery operation riêng và verified target/residual |
| CE-G08 | Fixed actor/workload authority; không impersonation/temp/emergency/internal bypass |
| CE-G09 | Audit/redaction/secret boundary fail-closed trước sensitive effect |
| CE-G10 | Runtime suite bao phủ atomic/batch/saga, crash windows, late outcome, recovery và source rejection |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CE11-01 | Caller execute revision khác decision | Deny + audit; không claim/effect |
| CE11-02 | Actor permission revoked trước scheduled claim | Invalidate/deny; worker không dùng stale decision như privilege |
| CE11-03 | Hai worker claim cùng stateVersion | Một operation claim; worker kia trả same/conflict |
| CE11-04 | Source nhận duplicate same operation | Trả same receipt/outcome; không effect lần hai |
| CE11-05 | Source nhận fencing token thấp | Reject trước effect |
| CE11-06 | Adapter 2xx nhưng verify version sai | Không success; reconcile/rollbackRequired |
| CE11-07 | Timeout trước dispatch được chứng minh | Có thể retry same operation theo bound |
| CE11-08 | Timeout sau dispatch không receipt | `reconciling`; không retry mù |
| CE11-09 | Late success từ attempt cũ sau retry outcome | Không overwrite; reconcile theo operation/fencing/stateVersion |
| CE11-10 | Atomic adapter trả partial | Contract violation + block + incident/recovery |
| CE11-11 | Batch 100 item: 80 success, 10 fail, 10 unknown | Manifest khớp; không báo success; reconcile unknown trước retry/RB |
| CE11-12 | Retry item đã success | Source dedupe/deny; không duplicate effect |
| CE11-13 | RB-1 previous version không còn compatible | Không start RB-1; `recoveryFailed`/incident + forward plan |
| CE11-14 | RB-2 compensation vượt original manifest | Deny extra scope; giữ residual finding |
| CE11-15 | RB-3 chỉ contain nhưng chưa restore | Không `rolledBack`; ghi recovery còn mở/residual |
| CE11-16 | Worker crash sau source commit trước local finalize | Reconcile operation receipt rồi finalize, không reapply |
| CE11-17 | Audit unavailable trước EC-4 apply | Fail-closed, no effect |
| CE11-18 | Notification outbox lỗi sau verified success | Business result giữ success; retry notification same event |
| CE11-19 | Internal loopback gọi adapter bỏ limiter | Deny/coverage gate fail; không bypass CT-05 |
| CE11-20 | Secret value xuất hiện trong adapter error | Redact/reject persistence; incident nếu exposure, không echo |

## 12. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CE-I01 | Không thấy execution operation/attempt/effect manifest/recovery aggregate | Chưa có idempotent orchestration/reconcile/rollback state | M11-T049; module-owner tasks |
| M11-CE-I02 | Configuration/balance/broadcast/delete/content action gọi service effect trực tiếp | Không bind revision/decision/fencing hoặc common verification | M11-T014–T021, T030, T038–T044; M11-T049 |
| M11-CE-I03 | Maintenance placeholder trả success không thực thi | Vi phạm verified-result; success giả | M11-T036, T041–T044 |
| M11-CE-I04 | Không thấy source adapter fencing/idempotency/reconcile contract | Duplicate/stale worker/unknown outcome có thể gây effect lặp | M11-T038–T044; module-owner tasks |
| M11-CE-I05 | Activity/System log không chứng minh audit intent/result durable | Audit loss không fail-closed và manifest có thể lộ dữ liệu | M11-T031–T035 |
| M11-CE-F01 | Operation/attempt/manifest/recovery/outbox schema và migration | Stable IDs, CAS, unique idempotency, metadata allowlist | M11-T049 |
| M11-CE-F02 | Source adapter implementation cho action/config/content/job/ops | Expected version/fencing/dedupe/reconcile/verify/recovery | M11-T014–T021, T030, T038–T044; module tasks |
| M11-CE-F03 | Durable audit/redaction/retention/access | EC-3/4 audit loss fail-closed, no raw payload | M11-T031–T035; REL-02 |
| M11-CE-F04 | Runtime chaos/race/crash-window/recovery evidence | CE-G01–G10, CE11-01–20 | M11-T049; A-G02/A-G06 |
| M11-CE-F05 | Deployment/config/worker/IAM inventory ngoài tracked code | Unknown không được coi ready/safe | M11-T049; REL-03 |

## 13. Tự kiểm M11-T011

- Chốt năm durable record, 12 operation state, source adapter input/output và sáu phase EX-01–EX-06.
- Phân biệt sourceAtomic/policySetAtomic/checkpointedBatch/sagaCompensated; partial/unknown có manifest, block và reconcile xác định.
- Retry chỉ khi proven no-effect + retrySafe; late outcome, worker crash và local-finalize gap dùng same operation/fencing.
- RB-1/2/3 có recovery operation, trigger/verification/residual semantics; contain không bị gọi nhầm rolledBack.
- 10 gate và 20 case bao phủ exact binding, stale authority/fencing, duplicate/timeout/partial, rollback, audit/outbox và secret/internal bypass.
- A-G02/A-G06/REL-02 vẫn chờ adapter/audit/runtime evidence; không kết luận gate đạt.

## 14. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt operation/attempt/manifest, pipeline, source adapter, retry/reconcile, verification và RB-1..3 | WSA-7K2 |
