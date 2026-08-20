# Vòng đời quyết định thay đổi quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CHANGE-DECISION-1.0` |
| Task | M11-T009 |
| Đầu vào | M11-GRANT-1.0, M11-ENHANCED-CONTROL-1.0, M11-CHANGE-REQUEST-1.0 |
| Quyết định nền | D-001, D-008, D-034–D-039; M11-D002, D005–D008, D021 |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Nguyên tắc

`ChangeDecision` là quyết định bất biến của một actor có fixed permission đối với đúng một sealed `revisionId + canonicalDigest`. Nó không phải approval của người thứ hai, permission assignment, token đặc quyền hay nguồn business truth.

- Người tạo request được tự `authorize` hoặc `reject` revision của mình nếu current fixed role/permission/scope, conflict rules và re-auth đều đạt; không có self-approval exception vì không tồn tại approval gate hai người.
- `authorized` chỉ là business instruction có thời hạn, không đóng băng quyền. Trước execute phải kiểm lại account/session policy, assignment/permission/scope, authorization version, target/source/evidence/limit và audit availability.
- Scheduled workload thực thi bằng fixed workload identity + exact decision/revision; decision không mint temporary user privilege. Actor bị revoke/suspend hoặc policy đổi làm instruction không còn executable.
- Mọi transition dùng expected `stateVersion`/CAS, idempotency key và append-only lifecycle event; không update/xóa decision hoặc history.
- Module nguồn quyết định invariant/effect. M11 không đổi reject/unknown/partial thành success.

## 2. Ba lớp record

| Record | Identity | Mutable? | Chức năng |
|---|---|---|---|
| Change request revision | `revisionId`, `canonicalDigest` | Không sau mỗi persisted revision; sealed tuyệt đối bất biến | Ý định, evidence, schedule, rollback/recovery |
| Change decision | `decisionId`, `revisionId`, `decisionDigest` | Không | Actor quyết định authorize/reject và phạm vi hiệu lực |
| Lifecycle aggregate | `revisionId`, `state`, `stateVersion`; `requestId` chỉ nhóm chuỗi revision | Chỉ qua CAS transition; event history append-only | Trạng thái của đúng revision để điều phối |

## 3. State registry

| State | Terminal | Nghĩa | Effect được phép |
|---|---|---|---|
| `draft` | Không | Revision đang được tạo; chưa sealed | Không business effect |
| `readyForDecision` | Không | Revision sealed, validation ready, chưa có decision | Chỉ authorize/reject/cancel/invalidate/expire |
| `authorized` | Không | Có decision authorize hợp lệ, chưa xếp lịch/execute | Chỉ schedule/execute/cancel/invalidate/expire |
| `rejected` | Có | Actor đã từ chối đúng revision với reason | Không effect; muốn tiếp tục tạo revision mới |
| `scheduled` | Không | Exact decision/revision đã có effective time/window | Chỉ execute/cancel/reschedule qua T010/invalidate/expire |
| `executing` | Không | Một execution operation đã claim bằng CAS | Effect chỉ qua T011; duplicate trả same operation |
| `succeeded` | Có | Postcondition của toàn scope đã verify | Không phát lại effect |
| `partiallySucceeded` | Không | Có effect một phần, manifest rõ; cần reconcile/rollback decision | Không báo success; chặn overlap |
| `failed` | Không | Execution fail xác định, chưa có effect hoặc effect cần xử lý rõ | Retry chỉ nếu contract chứng minh safe; nếu có effect chuyển recovery |
| `rollbackRequired` | Không | Stop criteria/partial/failure yêu cầu RB plan | Chỉ start rollback/recovery |
| `rollingBack` | Không | Một rollback/recovery operation đã claim | Không execution mới/overlap |
| `rolledBack` | Có | RB-1/RB-2 đã verify hoặc RB-3 contain/forward recovery đạt target | Giữ residual-impact metadata |
| `recoveryFailed` | Có | Rollback/recovery không đạt và cần incident/finding mới | Không tự retry/báo success |
| `canceled` | Có | Hủy hợp lệ trước execution claim | Không effect; history/decision vẫn giữ |
| `expired` | Có | Revision/decision hết validity trước execution claim | Không tự gia hạn; tạo/revalidate revision mới |
| `invalidated` | Có | Input/source/authority/control đổi làm decision không còn dùng được | Không execute; tạo/revalidate revision mới |

`failed` chỉ terminal về attempt, không terminal về aggregate nếu effect cần recovery. Orchestrator phải chuyển atomically sang `rollbackRequired` khi failure/unknown reconciliation xác nhận có effect cần xử lý.

## 4. Decision record

| Trường | Yêu cầu |
|---|---|
| `decisionId` | UUID server tạo; unique |
| `requestId`, `revisionId`, `canonicalDigest` | Exact sealed input; digest mismatch bị reject |
| `decision` | Chỉ `authorize` hoặc `reject`; không `autoApprove`, `emergency`, `override` |
| `decidedByActorId` | Server identity; không display name/client actor |
| `assignmentIds`, `authorizationVersion` | Fixed assignments/permission/scope đã dùng tại decision time |
| `permissionDecisionRef` | Protected immutable ref tới evaluation allow/deny + policy versions |
| `reauthProofRef`, `reauthAt` | Bắt buộc EC-3/EC-4, purpose-bound, freshness ≤5 phút lúc decision |
| `reasonCode`, `reasonText` | Bắt buộc cho authorize và reject; 10–1.000 ký tự, metadata-safe |
| `riskClass`, `limitRef`, `evidenceDigests` | Phải khớp sealed revision và server evaluation |
| `decidedAt`, `validUntil` | UTC server time; validity không vượt revision expiry |
| `decisionDigest` | Hash canonical toàn decision input/result |
| `idempotencyKey` | Unique theo actor/revision/decision intent; mismatch fingerprint là conflict |

Không có `approverId`, `secondApprover`, quorum, temporary role, break-glass flag hoặc impersonated actor. Tên UI cũ `approved` nếu còn phải migrate thành display label của `authorized`, không thay semantic contract.

## 5. Validity và expiry

| EC class | Decision TTL tối đa từ `decidedAt` | Revision TTL tối đa từ `sealedAt` | `validUntil` |
|---|---|---|---|
| EC-2 | 30 ngày | 30 ngày | Min(decidedAt + 30 ngày, revision.expiresAt) |
| EC-3 | 7 ngày | 7 ngày | Min(decidedAt + 7 ngày, revision.expiresAt) |
| EC-4 | 24 giờ | 24 giờ | Min(decidedAt + 24 giờ, revision.expiresAt) |

- Execute claim phải commit trước `validUntil`; hết hạn trong khi operation đã claim không hủy mù effect đang chạy mà để T011 hoàn tất/reconcile theo same operation.
- Schedule ngoài validity bị từ chối. Không auto-renew/extend, không đổi timestamp và không copy decision sang revision mới.
- EC class chỉ được nâng sau decision; việc nâng làm decision `invalidated` và yêu cầu decision mới với TTL chặt hơn.

## 6. Transition matrix

| Từ | Command/event | Sang | Guard chính | Audit/result |
|---|---|---|---|---|
| draft | Persist replacement revision | invalidated; revision mới ở draft | Parent revision/CAS + idempotency; một latest draft | `revisionSuperseded` + `revisionCreated` |
| draft | Seal ready revision | readyForDecision | CR-V01–V12 pass, audit durable | `requestReady` |
| draft | Cancel | canceled | Current actor có scope, no effect | `requestCanceled` + reason |
| readyForDecision | Authorize | authorized | Current permission, EC control, reason, re-auth, validity | Immutable `decisionAuthorized` |
| readyForDecision | Reject | rejected | Current actor có decision permission + reason | Immutable `decisionRejected` |
| readyForDecision | Cancel | canceled | Expected stateVersion | `requestCanceled` |
| readyForDecision | TTL elapsed | expired | Server UTC ≥ revision expiry | `requestExpired` |
| readyForDecision | Canonical/source/control change | invalidated | Trigger I01–I10 | `requestInvalidated` + code |
| authorized | Schedule | scheduled | T010 conflict/time validation + within validity | `changeScheduled` |
| authorized | Claim immediate execute | executing | Recheck all guards + operation CAS | `executionClaimed` |
| authorized | Cancel | canceled | Chưa execution claim | `authorizationCanceled` |
| authorized | TTL elapsed | expired | No execution claim | `decisionExpired` |
| authorized | Invalidation trigger | invalidated | No execution claim | `decisionInvalidated` |
| scheduled | Claim at window | executing | T010 lease + all current-state guards | `executionClaimed` |
| scheduled | Cancel/reschedule | canceled hoặc scheduled | T010 semantics; no claim | Event reason + old/new schedule ref |
| scheduled | TTL elapsed | expired | No execution claim | Release reservation/lease intent |
| scheduled | Invalidation trigger | invalidated | No execution claim | Release reservation; notify owner |
| executing | Verify all postconditions | succeeded | Exact operation/revision, durable result/audit | `executionSucceeded` |
| executing | Deterministic no-effect failure | failed | Module proves no effect | `executionFailed` |
| executing | Partial/unknown reconciled partial | partiallySucceeded | Per-item/effect manifest | `executionPartial` + block overlap |
| executing | Stop criterion/effect needs recovery | rollbackRequired | RB plan applicable | `rollbackRequired` |
| failed | Proven retry-safe retry | executing | Same operation identity/attempt policy | `executionRetried` |
| failed | Confirm no effect and abandon | canceled | Reason + no-effect evidence | `executionAbandoned` |
| failed | Effect discovered | rollbackRequired | Reconcile evidence | `effectDiscovered` |
| partiallySucceeded | Choose registered recovery | rollbackRequired | Manifest + RB-1/2/3 valid | `recoveryPlanned` |
| rollbackRequired | Claim recovery | rollingBack | Fixed workload/action permission + CAS | `recoveryClaimed` |
| rollingBack | Verify recovery target | rolledBack | Post-recovery checks pass | `recoverySucceeded` |
| rollingBack | Recovery exhausts/fails unsafe | recoveryFailed | Durable residual manifest + incident | `recoveryFailed` |

Mọi transition không liệt kê là deny + audit attempt. Terminal state không quay lại; tiếp tục công việc tạo request/revision mới liên kết `causedByRequestId` thay vì sửa history.

## 7. Invalidation registry

| Trigger | Điều kiện | Kết quả trước execute |
|---|---|---|
| I01 Revision/digest | Có revision mới hoặc digest không khớp | `invalidated`; decision không chuyển theo |
| I02 Target/source version | Expected/current version, lifecycle hoặc owner đổi | `invalidated` hoặc version conflict |
| I03 Selector/impact | Selector snapshot, affected count, assumption hoặc preview stale | `invalidated`; re-preview/revalidate |
| I04 Evidence/reference/hold | Evidence hết freshness, hold/reference/consent đổi | `invalidated` |
| I05 Risk/limit | EC class tăng, Limit ID/version/value/ceiling đổi | `invalidated`; không grandfather |
| I06 Authority | Actor account/assignment/permission/scope/security/policy version đổi | `invalidated`/deny; revoke có hiệu lực ngay |
| I07 Conflict/schedule | Overlap/new reservation/window không còn hợp lệ | `invalidated` hoặc reschedule theo T010 |
| I08 Rollback/monitoring | Target version, compensation, metric/source/stop plan không còn executable | `invalidated` |
| I09 Registry/deployment | Action/operation/config/job/capability bị deprecate/disabled | `invalidated` |
| I10 Audit/data policy | Audit không durable hoặc redaction/schema policy không đáp ứng | Fail-closed; `invalidated` nếu kéo dài |

Thay đổi chỉ ở display metadata không thuộc canonical digest có thể không invalidate, nhưng phải dùng allowlist/version và không được thay target/effect/risk/evidence.

## 8. Cancel, reject và stop khác nhau

| Khái niệm | Khi dùng | Có business effect? | Có thể tiếp tục cùng revision? |
|---|---|---|---|
| Reject | Quyết định không cho revision đi tiếp | Không | Không; tạo revision mới nếu muốn sửa |
| Cancel | Rút request/decision/schedule trước execution claim | Không | Không; tạo request/revision mới |
| Invalidate | Hệ thống phát hiện input/authority/control không còn đúng | Không trước claim | Không; revalidate revision mới |
| Expire | Validity hết trước claim | Không | Không; không auto-renew |
| Stop request | Execution đã claim, yêu cầu dừng an toàn | Có thể đã có | Không đổi ngay thành canceled; T011 reconcile/rollback |

## 9. Concurrency, idempotency và audit

- Aggregate transition dùng `WHERE revisionId/stateVersion/currentState`; zero row là conflict, phải đọc state mới. Mỗi request chỉ có một latest non-execution revision; revision cũ được supersede atomically.
- Một request chỉ có một active execution/recovery operation. Duplicate command cùng key/fingerprint trả cùng transition result; khác fingerprint trả conflict.
- Scheduler/API/worker cùng dùng một transition service và Action ID; không có route nội bộ bỏ permission, limit hoặc audit.
- Audit intent phải durable trước authorize/reject/cancel/invalidate/claim/recovery transition EC-3/EC-4; audit loss làm fail-closed.
- Event tối thiểu: event ID/time, request/revision/digest, from/to/stateVersion, actor/workload, permission/scope/policy refs, command/idempotency/correlation, reason/result/error code và metadata allowlist.
- Không audit secret, token, raw evidence, payload, full before/after PII hoặc exception detail chưa redaction.

## 10. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CD-G01 | Đủ 16 state và chỉ transition allowlist; terminal không reopen |
| CD-G02 | Decision khóa exact revision/digest, immutable và idempotent |
| CD-G03 | Creator được tự quyết định chỉ bằng current fixed authority; không approver/two-person/temp/emergency path |
| CD-G04 | EC-3/4 re-auth ≤5 phút tại decision; execute recheck current authority và sealed instruction |
| CD-G05 | TTL 30d/7d/24h, schedule ≤validUntil, không renew/copy decision |
| CD-G06 | I01–I10 invalidation fail-closed trước claim |
| CD-G07 | StateVersion/CAS ngăn authorize/cancel/execute/recovery race |
| CD-G08 | Partial/unknown không thành success; overlap bị block đến reconcile/recovery |
| CD-G09 | Audit loss chặn sensitive transition; event metadata-safe |
| CD-G10 | Module-source result/invariant giữ nguyên, terminal/recovery result được verify |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CD09-01 | Creator có fixed permission authorize revision của mình | Cho phép, ghi single-actor decision đầy đủ |
| CD09-02 | Creator không còn permission lúc authorize | Deny; không decision |
| CD09-03 | EC-4 re-auth 6 phút | Challenge/deny; state giữ ready |
| CD09-04 | Authorize và reject đồng thời cùng stateVersion | Một transition thắng; transition kia conflict |
| CD09-05 | Revision đổi sau authorize | Decision cũ invalidated, không execute revision mới |
| CD09-06 | EC-4 schedule sau 24 giờ | Reject schedule vì vượt validUntil |
| CD09-07 | Actor bị revoke sau schedule | Invalidate/deny claim; workload không dùng decision như privilege |
| CD09-08 | Limit ceiling giảm trước claim | Invalidate; revalidate/decision mới |
| CD09-09 | Duplicate authorize cùng key/fingerprint | Trả cùng decisionId/result |
| CD09-10 | Same key nhưng decision/reason khác | Conflict; không ghi decision thứ hai |
| CD09-11 | Cancel và scheduler claim đồng thời | CAS chọn một; nếu claim thắng thì cancel thành stop request, không canceled giả |
| CD09-12 | Execution timeout chưa biết effect | Không success/retry mù; reconcile same operation |
| CD09-13 | Reconcile xác nhận partial | `partiallySucceeded`, block overlap, chọn RB plan |
| CD09-14 | Failure chứng minh không effect và retry-safe | Retry cùng operation theo policy; không request mới mù |
| CD09-15 | Rollback target verify đạt | `rolledBack` + residual metadata; không xóa failed history |
| CD09-16 | Recovery thất bại | `recoveryFailed` + incident/finding; không báo success |
| CD09-17 | Audit store lỗi khi authorize EC-4 | Fail-closed; state không đổi |
| CD09-18 | UI gửi `approverId`/`emergency=true` | Unknown/forbidden field reject; không tạo authority |
| CD09-19 | Terminal rejected bị yêu cầu reopen | Deny; tạo revision/request mới có liên kết |
| CD09-20 | Scheduled workload có fixed identity nhưng decision digest mismatch | Deny + audit anomaly; không effect |

## 11. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CD-I01 | Không thấy ChangeRequest/Decision aggregate, state machine hoặc transition service | Chưa có durable lifecycle/CAS/idempotency | M11-T010–T011; M11-T049 |
| M11-CD-I02 | Các route mutation gọi service effect trực tiếp | Không có ready/authorized/scheduled/executing/result binding | M11-T010–T021, T030, T038–T044 |
| M11-CD-I03 | Không thấy decision expiry/invalidation khi revision/authority/source đổi | Có thể dùng ý định hoặc quyền stale | M11-T010–T011; M01-T029–T032 |
| M11-CD-I04 | Placeholder maintenance trả success và log text | Không có operation/reconcile/recovery state | M11-T011, T036, T041–T044 |
| M11-CD-I05 | Activity/System log không chứng minh immutable lifecycle audit | Transition nhạy cảm có thể thiếu/mất dấu vết | M11-T031–T035 |
| M11-CD-F01 | Durable aggregate/event/outbox schema, indexes và migration | StateVersion CAS, append-only decision/event, unique idempotency | M11-T011; M11-T049 |
| M11-CD-F02 | Conflict/lease/schedule/reschedule semantics | Authorized không được claim khi overlap/expired | M11-T010 |
| M11-CD-F03 | Execution/partial/unknown/retry/rollback implementation | Exact revision/decision/operation và verified result | M11-T011 |
| M11-CD-F04 | Current-authority/session invalidation integration | Revoke/suspend/policy drift chặn claim ngay | M01-T029–T032; M11-T049 |
| M11-CD-F05 | Audit durability/redaction/runtime transition suite | Sensitive transition audit loss fail-closed | M11-T031–T035, T049; A-G02 |

## 12. Tự kiểm M11-T009

- Chốt 16 state, 28 transition, 10 invalidation trigger và ranh reject/cancel/invalidate/expire/stop.
- Decision record bất biến khóa exact revision/digest, dùng fixed current authority, re-auth và TTL 30 ngày/7 ngày/24 giờ.
- Creator được tự quyết định theo lựa chọn sản phẩm nhưng request/decision không cấp privilege; scheduled workload không impersonate actor.
- 10 gate và 20 case bao phủ self-decision, stale authority/revision/limit, CAS race, expiry, audit loss, partial/unknown và recovery.
- A-G02/REL-02 vẫn chờ conflict/execution/audit/runtime evidence; không kết luận gate đạt.

## 13. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt state/transition, decision record, TTL/invalidation/CAS/audit và recovery states | WSA-7K2 |
