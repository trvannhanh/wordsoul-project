# Phiên bản cấu hình bất biến M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CONFIG-VERSION-1.0` |
| Task | M11-T014 |
| Đầu vào | M11-CONFIG-REG/VALIDATION-1.0, M11-CHANGE-REQUEST/DECISION/SCHEDULE/EXECUTION-1.0 |
| Phạm vi | CFG-001–029, bảy policy set, global scope v1.0 |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Mọi thay đổi cấu hình tạo record/version mới; không sửa giá trị, metadata hay lịch sử đang/đã có hiệu lực tại chỗ. “Current” là pointer/assignment có CAS, không phải một row mutable mang toàn bộ truth.

- Policy set là đơn vị validate/authorize/schedule/activate/rollback. Consumer không ghép key từ nhiều version.
- Version identity và digest khóa exact definition/value/exposure/effect/validator/consumer contract; metadata canonical đổi cũng tạo version mới.
- Lịch hiệu lực là interval UTC `[effectiveFrom, effectiveTo)`, không overlap cho cùng policy set + scope.
- Rollback tạo activation/assignment mới trỏ một compatible prior version; không xóa/sửa version lỗi hoặc quay ngược số version.
- Consumer snapshot version tại operation/session/job start và giữ đến boundary đã đăng ký; cache không là source truth.
- Secret value không nằm trong version; chỉ protected secret reference/metadata theo M12.

## 2. Durable model

| Record | Identity | Nội dung | Mutable? |
|---|---|---|---|
| ConfigDefinitionVersion | `configId:definitionVersion` | Key/type/unit/range/owner/exposure/effect/lifecycle/validator/consumer schema | Không |
| ConfigPolicySetVersion | `policySetId:setVersionId` | Full member map definition+typed value/ref, digest, compatibility/rollback metadata | Không |
| ConfigValidationResult | `validationId` | Candidate digest, CV-01–12 result/error/evidence/source versions | Không |
| ConfigEffectiveAssignment | `assignmentId` | Policy set/scope/version, interval UTC, change/decision/execution refs | Không sau commit; cancel tạo event trước effective |
| ConfigCurrentPointer | `policySetId+scope` | Active assignment/version/digest, pointerVersion | Chỉ CAS transition; history không đặt ở đây |
| ConfigVersionEvent | `eventId` | Created/validated/scheduled/activated/superseded/failed/rollback/deprecated/read-drift event | Append-only |
| ConsumerVersionObservation | `consumerId+instance+observedAt` | Reported set version/digest, freshness, state | Append-only/retention; không truth thay pointer |

DB constraints: unique `(configId, definitionVersion)`, `(policySetId, setVersionNo)`, digest uniqueness theo policy set, một open assignment interval cho scope, pointerVersion CAS và immutable-row update/delete guard.

## 3. Version identity và canonical digest

| Trường | Quy tắc |
|---|---|
| `setVersionNo` | Monotonic integer per policy set; không tái dùng, kể cả candidate fail/cancel nếu đã persist |
| `setVersionId` | UUID/server ID; external ref ổn định |
| `parentVersionId` | Version dùng làm base diff; null cho bootstrap |
| `memberMap` | Đủ mọi required key theo policy set, sorted canonical Config ID |
| `typedValues` | Canonical typed serialization; không locale/text coercion |
| `registry/validator/consumerVersions` | Exact contract versions bind digest |
| `scopeSchema`, `exposure`, `effectBoundary` | Canonical decision input |
| `canonicalDigest` | Hash toàn member/contract metadata; không chứa presentation-only annotation |
| `createdBy/At`, `changeRevisionId` | Server actor/time + immutable request ref |

SRS `CFG-001 SrsPolicyVersion` là business-visible derived member bằng `setVersionNo`/mapping đã đăng ký. Không nhận giá trị client và không increment row riêng trước khi policy-set activation commit.

## 4. Version lifecycle

| State | Entry | Allowed next | Business read? |
|---|---|---|---|
| `candidate` | Immutable full member set persisted | validated/invalid/canceled | Không |
| `validated` | Exact digest pass CV-01–10 | authorized/invalidated/expired | Không |
| `authorized` | Single-actor decision exact digest | scheduled/executing/canceled/invalidated/expired | Không |
| `scheduled` | Durable assignment/reservation valid | executing/canceled/invalidated/expired | Không trước interval |
| `effective` | Execution + pointer CAS + postcondition verify | superseded/deprecated/rollback target | Có trong interval |
| `superseded` | Pointer chuyển version mới | deprecated/rollback target/retired khi safe | Chỉ historical/snapshot đã bind |
| `deprecated` | Không cho candidate/consumer mới chọn | rollback target nếu compatibility explicit; retired | Historical only |
| `invalid` | Validation/source/compatibility reject | Terminal version; tạo candidate mới | Không |
| `failed` | Activation/postcondition failure | rollback/recovery qua operation mới | Không current nếu pointer CAS chưa đổi; nếu partial giữ block |
| `canceled` | Hủy trước execution claim | Terminal | Không |
| `expired` | Decision/schedule validity hết | Terminal | Không |
| `retired` | Không ref/hold/consumer/runtime read | Terminal metadata | Không |

State history là event/assignment; không update immutable version payload để “đổi state”. Materialized status có thể derive hoặc CAS projection và phải rebuild được từ durable events.

## 5. Candidate và diff

- Candidate luôn chứa full member map, kể cả UI gửi patch; server đọc exact parent và materialize full set trước validation.
- Diff gồm added/changed/deprecated members, typed before/after summary, exposure/effect/consumer/validator changes; không raw sensitive values.
- Parent/current mismatch là conflict; không rebase tự động. Actor tạo candidate mới từ current version và chạy lại validation/preview/decision.
- Definition change (type/range/unit/key/owner/exposure/effect) cần compatibility/migration plan; không dùng value-only path.
- CFG-014 AP không được đưa vào active economy candidate mới; bootstrap history có thể giữ protected legacy member để truy vết nhưng consumer-new bị deny.

## 6. Effective assignment và timeline

| Trường | Contract |
|---|---|
| `policySetId`, `scopeRef`, `setVersionId/digest` | Exact version/scope |
| `effectiveFromUtc`, `effectiveToUtc` | Start inclusive/end exclusive; end null chỉ current open interval |
| `schedule/reservation/fencing refs` | M11-CHANGE-SCHEDULE exact refs |
| `decision/execution refs` | Exact authorize/verified operation |
| `assignmentVersion` | CAS cho cancel/activate/supersede race |
| `activationReason` | publish/rollout/rollback/migration/deprecation replacement |

Activation transaction (local boundary): verify no overlapping interval, close previous open interval at same UTC instant, create new assignment, CAS current pointer, append audit/outbox. Source adapter then verifies consumer-visible version; nếu cross-store unknown, state/reconcile giữ block và không báo success.

Scheduled future assignment không được consumer đọc trước `effectiveFromUtc`. Missed/expired schedule không tự trở thành current. Hai assignment cùng start/scope conflict theo T010.

## 7. Consumer read/snapshot contract

| Bước | Rule |
|---|---|
| Resolve | Consumer gửi policySetId/scope/operation time; resolver trả exact effective assignment/version/digest |
| Validate | Consumer hỗ trợ consumer contract + definition/value schema; mismatch fail-closed/degraded only if registry permits |
| Snapshot | Persist setVersionId/digest vào session/job/review/ledger/config operation theo boundary |
| Use | Đọc full typed set; không query từng key rồi trộn version |
| Cache | Cache key gồm policy set + scope + version/digest; pointer event invalidates hint, durable resolver remains truth |
| Report | Consumer observation version/freshness/health; missing/stale không tự coi converged |

New session/operation dùng current version tại start. Existing session/review/reward/job giữ snapshot; policy change không đổi fairness/result giữa chừng. Consumer không hỗ trợ current version làm activation fail/rollback theo plan, không silently dùng hardcoded default.

## 8. Rollback và compatibility

| Compatibility | Nghĩa | Rollback policy |
|---|---|---|
| `backwardCompatible` | Current consumers đọc được prior/next schema | Có thể activate prior version bằng RB-1 sau current validation |
| `requiresConsumerMigration` | Schema/semantic/boundary đổi | Chỉ activate khi consumer matrix/rollout đạt; prior rollback cần migration compatibility |
| `irreversibleDataEffect` | Config đã tạo durable business effect không đảo bằng version | Version rollback chỉ ngăn effect mới; RB-2/RB-3 xử lý effect manifest |
| `incompatible` | Prior version không còn safe/valid | Không dùng làm rollback target; forward recovery/incident |

Rollback workflow tạo `ConfigEffectiveAssignment` mới với `activationReason=rollback`, setVersionNo mới **không bắt buộc** nếu tái kích hoạt exact immutable prior version; assignment/event luôn mới. Nếu policy yêu cầu patch prior values/schema, đó là candidate/version mới. Verify current pointer, consumer convergence, metrics và residual business effects trước `rolledBack`.

## 9. Retention, deprecation và deletion

- Version/event/assignment/decision/execution metadata giữ tối thiểu 12 tháng và lâu hơn khi session/ledger/audit/legal/incident reference hoặc module policy yêu cầu.
- Không hard-delete effective/superseded/deprecated version còn reference, rollback target, consumer observation hoặc hold.
- Physical key rename tạo Config ID/definition mới + alias/migration; old key deprecated, không tái dùng cho nghĩa khác.
- Retired chỉ sau reference scan, consumer zero-read window, backup/restore test và deletion manifest. T017 chốt chi tiết deprecation/removal.
- Current-value materialization nếu có là rebuildable projection; xóa projection không xóa version history.

## 10. Bootstrap/migration từ mutable rows

| Step | Hành động |
|---|---|
| VM-01 | Kiểm runtime schema/inventory 29 key theo environment; extra/missing quarantined, không copy secret-like value vào artifact |
| VM-02 | Validate current values bằng T013; invalid/unknown không active baseline tự động |
| VM-03 | Materialize seven full policy-set baseline versions với source/migration digest |
| VM-04 | Map legacy `LastUpdatedAt/By` chỉ thành provenance không tin cậy; không gọi là complete audit |
| VM-05 | Create effective assignment/current pointer bằng controlled migration + DB UTC |
| VM-06 | Deploy dual-read shadow comparison; legacy row và version resolver mismatch alert/block promotion |
| VM-07 | Switch consumers theo set, persist version snapshot và report observation |
| VM-08 | Freeze direct create/update/delete; legacy table trở thành projection hoặc retire theo manifest |

CFG-029 seed 7 và CFG-014 AP không được auto-promote thành compliant active target. Chúng cần explicit migration/removal plan, impact/reconcile và decision; bootstrap không hợp thức hóa chúng.

## 11. Failure semantics

| Failure | Hành vi |
|---|---|
| Version store unavailable | Không create/validate/activate; consumer giữ proven snapshot hoặc fail-safe theo registry |
| Pointer CAS conflict | Không last-write-wins; reload/revalidate candidate |
| Assignment overlap | Reject schedule/activation; không truncate interval ngầm |
| Validation/evidence stale | Invalidate candidate/decision; không copy pass |
| Pointer committed, consumer visibility unknown | Reconcile exact assignment/version; giữ block/monitor, không activate lần hai |
| Consumer incompatible/stale | Stop rollout/rollbackRequired; không hardcoded fallback |
| Audit/outbox unavailable before sensitive activation | Fail-closed |
| Cache stale | Resolver/source version wins; stale cache evicted, không truth |
| Rollback target missing/incompatible | Không rollback mù; forward recovery/incident |

## 12. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| FV-G01 | 29 key thuộc đúng seven full-member policy set; version/digest stable/unique |
| FV-G02 | Version payload immutable; DB/API không update/delete in-place |
| FV-G03 | Candidate parent/current CAS, validation/decision/schedule exact digest |
| FV-G04 | Effective interval không overlap; pointer/assignment/audit/outbox boundary xác định |
| FV-G05 | Consumer đọc full set + snapshot, không trộn key/version hoặc silent fallback |
| FV-G06 | SRS derived version atomic; existing session/review giữ snapshot |
| FV-G07 | Rollback tạo assignment/operation mới, verify compatibility/convergence/residual |
| FV-G08 | Reference/hold/deprecation/retention chặn hard-delete/reuse key |
| FV-G09 | Bootstrap invalid/AP/retention legacy không auto-promote; shadow drift có evidence |
| FV-G10 | Runtime concurrency/cache/store/consumer/audit failure suite đạt |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| FV14-01 | UI patch một SRS key | Server materialize full set; validate/digest/version toàn set |
| FV14-02 | Hai candidate cùng parent activate | Một pointer CAS thắng; candidate kia conflict/stale |
| FV14-03 | Sửa immutable version row | DB/API deny + audit anomaly |
| FV14-04 | Candidate digest đổi sau validation | Validation/decision stale; không schedule |
| FV14-05 | Future assignment được query trước start | Resolver vẫn trả current version |
| FV14-06 | Hai assignment overlap same scope | Reject; không truncate/last-write-wins |
| FV14-07 | Session đang chạy khi WordsPerSession đổi | Session giữ old snapshot; session mới dùng new version |
| FV14-08 | SRS set activate | CFG-001 derived +1 atomically, không row increment rời |
| FV14-09 | Consumer lấy từng key qua cache khác version | Contract reject/migration; full-set resolver required |
| FV14-10 | Cache trả digest stale | Durable pointer wins; evict/alert |
| FV14-11 | Consumer không hỗ trợ new schema | Activation/rollout stop; không silent default |
| FV14-12 | Pointer commit nhưng response timeout | Reconcile assignment/operation; không activate duplicate |
| FV14-13 | Rollback exact compatible prior version | New assignment/event, old failed history giữ nguyên |
| FV14-14 | Prior version incompatible | Deny RB-1; forward recovery/incident |
| FV14-15 | Config rollback nhưng ledger effects đã tạo | Version chỉ chặn effect mới; RB-2 manifest xử lý residual |
| FV14-16 | Delete superseded version còn session ref | Deny; giữ đến reference/retention clear |
| FV14-17 | Rename key rồi tái dùng old key cho nghĩa khác | Deny; new Config ID, old alias/deprecated |
| FV14-18 | Bootstrap CFG-014/CFG-029 legacy values | Không auto-promote compliant active; migration finding |
| FV14-19 | Runtime có extra/missing key | Quarantine/drift; không create current pointer mù |
| FV14-20 | Audit unavailable trước EC-4 activation | Fail-closed; pointer không đổi |

## 13. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-FV-I01 | `SystemConfiguration` primary key là Key và Value/metadata được update tại chỗ | Không immutable version/parent/digest/history | M11-T049 |
| M11-FV-I02 | `UpdateBulkAsync` + SaveChanges cập nhật entity hiện hành | Không pointer CAS/effective assignment/outbox/common rollback | M11-T049 |
| M11-FV-I03 | SRS service tăng `SrsPolicyVersion` trong same mutable collection | Có policy grouping bước đầu nhưng version row vẫn mutable và không timeline | M04 tasks; M11-T015–T017, T049 |
| M11-FV-I04 | Consumer đọc key riêng/hardcoded fallback | Có thể trộn version, mất snapshot và che invalid config | Module consumer tasks; M11-T049 |
| M11-FV-I05 | Generic API create/update/delete key vật lý | Không lifecycle/reference/retention/deprecation guard | M11-T017; M11-T049 |
| M11-FV-I06 | Không thấy config version/current pointer/assignment/consumer observation tables | Chưa có runtime source cho contract này | M11-T049 |
| M11-FV-F01 | Durable schema/constraints/migration/dual-read cutover | Immutable records, CAS pointer, interval non-overlap | M11-T049 |
| M11-FV-F02 | Preview/impact/consumer matrix và rollout implementation | Exact candidate/version/digest | M11-T015–T017 |
| M11-FV-F03 | Audit/redaction/retention/reference scan | No raw value/secret; hold wins deletion | M11-T031–T035; M11-T049 |
| M11-FV-F04 | Module consumer snapshot/adapters and convergence reporting | No mixed version/silent fallback | M03/M04/M06/M09/M01/M12 tasks |
| M11-FV-F05 | Runtime race/cache/store/migration/rollback evidence | FV-G01–G10, FV14-01–20 | M11-T049; A-G02/A-G06 |

I01–I06 là release gap; tài liệu không xác nhận mutable table hiện tại đáp ứng version contract.

## 14. Tự kiểm M11-T014

- Chốt bảy durable record, 12 lifecycle state, version/digest/full-member set, effective interval/current pointer CAS và consumer snapshot.
- Rollback/compatibility, retention/deprecation và VM-01–VM-08 migration không sửa/xóa history hoặc auto-promote legacy-invalid values.
- 10 gate và 20 case bao phủ immutable write, parent/pointer race, overlap, cache/consumer drift, exact rollback, references và audit loss.
- Sáu sai lệch + năm finding có task tiếp nhận. A-G02/A-G06 vẫn chờ preview/rollout/audit/schema/runtime evidence; không kết luận gate đạt.

## 15. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt immutable records/lifecycle, effective assignment/pointer, consumer snapshot, rollback và migration | WSA-7K2 |
