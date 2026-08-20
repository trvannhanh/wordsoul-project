# Ma trận quyền tối thiểu M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T004 |
| Matrix ID / phiên bản | M11-PERM-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-032–D-035; M11-D001–D006; M11-DICT-1.0; M11-ACTION-1.0; M11-ROLE-1.0; M01-SESSION-1.0 |
| Phạm vi | Permission/action/role/scope/data mapping, deny rule, evaluation order, read/write separation và audit obligations |
| Ngoài phạm vi | Grant/revoke workflow T005; no-emergency verification T006-A; action change lifecycle T007–T011; source/runtime implementation |

## 1. Bất biến authorization

- Default deny: action/permission/role/scope/policy/resource version không biết hoặc mapping thiếu đều `DENY`, không fallback `Admin/SuperAdmin`.
- Permission ID là stable capability to request an Action ID; role chỉ được xét qua versioned assignment. Không permission wildcard, role nesting hoặc implicit CRUD expansion.
- Read, export, author, publish, delete, operate và administer-access là permission khác nhau; có write không tự có sensitive read/export và ngược lại.
- Server enforcement lấy actor/session/current account + authorization version từ trusted context; path/body user ID, null ID, header hoặc UI state không là authority.
- Module nguồn re-check resource ownership/invariant/version trước effect. M11 allow chỉ là điều kiện cần, không ép module trả success.
- R3/R4 bắt buộc admin session current-state + re-auth ≤5 phút + reason + immutable audit. Case/change/incident, preview, notification và reconcile áp theo action obligations.
- Explicit deny, conflict rule, state/restriction, data-class ceiling và resource policy luôn thắng allow. Claim/cache stale không thắng source truth.

## 2. Ký hiệu role và scope

| Mã | Role |
|---|---|
| R01 | SupportAgent |
| R02 | IdentitySecurityOperator |
| R03 | ContentAuthor |
| R04 | ContentPublisher |
| R05 | AssetOperator |
| R06 | BattleOperator |
| R07 | NotificationOperator |
| R08 | PlatformOperator |
| R09 | DataAnalyst |
| R10 | AuditInvestigator |
| R11 | PrivacyOperator |
| R12 | AccessAdministrator |
| R13 | PlatformOwner |

`I` = initiate/request only; `E` = execute source effect; `R` = read; `A` = author; `P` = publish/activate; `O` = operate. Role không nêu trong row bị deny. `None/workload` nghĩa human admin không có permission execute; module workload thực thi từ request đã kiểm soát.

## 3. Ma trận action–permission–role

| Action ID | Permission ID | Role/mode cho phép | Scope/data ceiling | Obligations thêm |
|---|---|---|---|---|
| M11-ACT-001 | `m01.account.support-summary.read` | R01-R, R02-R, R11-R | Exact subject/case; Personal masked | Purpose + access audit |
| M11-ACT-002 | `m01.account.sensitive-detail.read` | R01-R, R02-R | Exact subject/case; Sensitive allowlist | RA + case + field mask |
| M11-ACT-003 | `m01.account.restriction.change` | R02-E | Exact subject/restriction; production M01 | Incident/case + before/after + N + RC |
| M11-ACT-004 | `m01.session.family.revoke` | R02-E | Exact subject/family or explicit all | Case/incident + revoke result + N |
| M11-ACT-005 | `m11.authorization.grant.change` | R12-E non-R13 grants; R13-E R12/R13 continuity only | Exact actor/role/scope/environment | RA + non-self + conflict + version + N + RC |
| M11-ACT-006 | `m01.data-export.workflow` | R01-I, R11-E/R | Exact request/subject/manifest | Owner proof + preview + recipient/expiry + access audit |
| M11-ACT-007 | `m01.deletion.workflow` | R01-I, R11-E/R | Exact request/subject/manifest | Owner proof + reversible point + hold/reference + N + RC |
| M11-ACT-008 | `m02.content-draft.read` | R03-R, R04-R | Assigned content group; Internal/Personal masked | Version + field allowlist |
| M11-ACT-009 | `m02.content-draft.author` | R03-A | Assigned content group; Internal | Expected version + validation + RC |
| M11-ACT-010 | `m02.content-version.publish` | R04-P | Assigned content group/version | RA + impact/reference + effectiveAt + RC |
| M11-ACT-011 | `m02.content.merge-or-delete` | R04-E | Exact resource/version; no hold/reference | RA + case/change + preview + retention + RC |
| M11-ACT-012 | `m02.vocabulary-set.author-or-publish` | R03-A, R04-P | Owned/assigned set group | Ownership from source + version; P adds RA/impact |
| M11-ACT-013 | `m02-m06.set-reward.change` | R05-E | Exact set/reward catalog ref; M06 limit | Case/change + dual-source validation + preview + RC |
| M11-ACT-014 | `m03.learning-session.support-read` | R01-R | Exact subject/session/case; Personal masked | Purpose + field allowlist + access audit |
| M11-ACT-015 | `m03.learning-record.remediation-request` | R01-I; source workload-E | Exact record/case; Sensitive | Evidence + preview; no direct history mutation; reconcile |
| M11-ACT-016 | `m04.review-history.support-read` | R01-R | Exact subject/case; Personal masked | Purpose + time bound + access audit |
| M11-ACT-017 | `m04.review-projection.rebuild` | R08-O; source workload-E | Registered job + bounded cohort | RA + case/change + dry-run/checkpoint + RC |
| M11-ACT-018 | `m05.pronunciation-metadata.support-read` | R01-R | Exact subject/case; Sensitive metadata only | Consent/purpose + no raw audio + access audit |
| M11-ACT-019 | `m05.pronunciation-data.disposition` | R11-E | Exact request/asset manifest | RA + owner proof/hold + no raw audio + RC |
| M11-ACT-020 | `m06.ledger.support-read` | R01-R masked, R05-R | Exact subject/case/ledger range | Purpose + amount/data mask + access audit |
| M11-ACT-021 | `m06.ledger.adjustment.execute` | R05-E | Exact case/asset type/value limit | RA + preview + operation ID + no-negative + N + RC |
| M11-ACT-022 | `m06.asset-catalog.version` | R05-A/P | Assigned item/pet catalog scope | RA for P/deprecate + reference/impact + RC |
| M11-ACT-023 | `m07.quest-achievement.read` | R03-R, R04-R | Assigned definition group; Internal | Version/freshness |
| M11-ACT-024 | `m07.quest-achievement.author` | R03-A | Assigned definition group/version | Validation + expected version + RC |
| M11-ACT-025 | `m07.quest-achievement.publish-or-retire` | R04-P | Assigned definition/version | RA + reference/reward impact + effectiveAt + RC |
| M11-ACT-026 | `m08.battle-gym.support-read` | R01-R masked, R06-R | Exact battle/gym/case; Personal/Sensitive allowlist | Purpose + no hidden answer/secret + access audit |
| M11-ACT-027 | `m08.battle.force-interrupt` | R06-E | Exact active battle/case | RA + preview current state + operation ID + N + RC |
| M11-ACT-028 | `m08.gym-rule.version-operate` | R06-A/P | Assigned gym/rule version | RA for P + snapshot/effectiveAt/impact + RC |
| M11-ACT-029 | `m09.group.support-read` | R01-R | Exact group/member/case; Personal masked | Purpose + member field mask + access audit |
| M11-ACT-030 | `m09.group.admin-mutation` | None/workload | Disabled for human in v1.0 | Await M09 owner lifecycle/role decision; no legacy broad fallback |
| M11-ACT-031 | `m09.group.delete-or-owner-remediate` | R11-I; source workload-E | Exact case/group/manifest | Reference/owner policy + retention + N + RC |
| M11-ACT-032 | `m09.ranking.aggregate-read` | R09-R | Published aggregate/approved cohort | Metric version/freshness + small-group suppression |
| M11-ACT-033 | `m10.notification-template.author` | R07-A/P | Assigned channel/template group | Variable allowlist + preview + version; P adds RA |
| M11-ACT-034 | `m10.broadcast-campaign.operate` | R07-O | Bounded audience/channel/campaign | RA + case/change + audience preview + quota/consent + stop + RC |
| M11-ACT-035 | `m10.delivery.support-read` | R01-R masked, R07-R aggregate | Exact recipient/case or aggregate campaign | No raw device token/content + access audit |
| M11-ACT-036 | `m11.configuration.version-operate` | R08-A/P/O | Assigned namespace/environment; Sensitive metadata | RA + change + validation/preview/effectiveAt + rollback |
| M11-ACT-037 | `m11.audit-log.investigate` | R10-R/export, R11-hold metadata only | Exact case/incident/time/source; Sensitive allowlist | RA + query budget + access audit; no edit/delete |
| M11-ACT-038 | `m11.job-run.operate` | R08-O | Registered job/environment/cohort | RA + case/change + dry-run/checkpoint/idempotency + RC |
| M11-ACT-039 | `m11.support-case.manage` | R01-E | Assigned case queue/subject | Reason/assignment/SLA + note redaction + audit |
| M11-ACT-040 | `m11.incident.manage` | R08-O | Assigned incident/capability/environment | RA + incident role + timeline/playbook + N + RC |
| M11-ACT-041 | `m11.maintenance.operate` | R08-O | Exact registered maintenance action/scope | RA + incident/change + preview + operation ID + stop/RC |
| M11-ACT-042 | `m12.capability-control.change` | R08-O | Assigned capability/config/limiter; metadata only | RA + change/incident + preview/canary + N + RC |
| M11-ACT-043 | `m12.secret-lifecycle.operate` | R08-O | Exact secret reference/purpose/environment; no value | RA + case/change + custody separation + rotation proof + RC |
| M11-ACT-044 | `m12.health-capability.read` | R08-R, R09-R aggregate, R10-R incident | Assigned capability/environment; Internal | Freshness/source/confidence; no secret/provider payload |

Action 030 bị deny cho human không phải thiếu coverage: target v1.0 cố ý không cho role hiện tại quản trị group trực tiếp trước khi M09 chốt owner/lifecycle. R01 chỉ mở case/request; workload/module source mới được effect theo contract sau.

## 4. Obligations theo risk/data

| Boundary | R0 | R1 | R2 | R3 | R4 |
|---|---|---|---|---|---|
| Session | Anonymous/public policy | Full/scoped session | Full current-state session | Admin session + RA ≤5 phút | Admin session + RA ≤5 phút, security/risk known |
| Reason/context | Không | Purpose nếu non-public | Purpose; case khi subject-specific support | Reason + change/case | Reason + exact case/change/incident + impact |
| Concurrency | Cache/version nếu relevant | Snapshot/freshness | Query snapshot/limits | Expected version/CAS/idempotency | CAS/idempotency + unknown reconcile + compensation/rollback |
| Audit | Access/metric theo policy | Actor/action/result | Access audit + protected target/data class | Immutable before/after/reason/result | Immutable before/after/effect/revoke/notify/reconcile |
| Failure | No data widening | Bounded error | Deny/redact on policy unknown | Fail-closed before effect | Fail-closed; durable deny/revoke/hold after uncertain dependency |

Sensitive read/export không được kế thừa từ mutation permission. Secret value và Provider-native payload luôn deny ở DTO/query/log/audit; `m12.secret-lifecycle.operate` chỉ nhận protected secret reference và operation metadata.

## 5. Evaluation order

1. Resolve trusted actor/workload, admin session class, current account/security/authorization/policy version; reject legacy-only authority.
2. Resolve Action ID + Permission ID từ server route/job registry; mismatch/missing/disabled deny trước resource lookup tốn kém.
3. Load active role grants; validate role/catalog version, environment, review status và conflict rules.
4. Intersect grant scope với requested module/resource/data/operation/subject/case; null/unknown scope deny.
5. Apply explicit deny, account/restriction/risk state, re-auth freshness, data ceiling, separation rule và action obligations.
6. Module owner loads resource/current version and validates ownership/invariant/hold/reference/quota.
7. Mutation reserves operation/CAS/audit boundary; module effect returns semantic result. Unknown commit reconciles, không rerun action mới.
8. Record allow/deny/challenge/failure audit allowlist; return field-minimized result. Denial must not reveal hidden resource/subject existence.

Policy decision cache chỉ key theo actor authorization version + action/permission + normalized scope/resource policy version + environment; TTL ngắn hơn state/revocation requirement. Revocation/change invalidates cache; cache/store failure on R2–R4 denies.

## 6. Explicit deny rules

| Deny ID | Điều kiện | Áp dụng |
|---|---|---|
| DENY-01 | Legacy Admin/SuperAdmin claim nhưng không active target grant | Mọi Action ID |
| DENY-02 | Actor tự grant/nâng/review role hoặc last-owner unsafe change | ACT-005 |
| DENY-03 | Role conflict active trong same environment/scope/case | Các action thuộc cả hai conflict domains |
| DENY-04 | Missing/null/global-unspecified scope hoặc resource ID từ client không được source authorize | Mọi non-public action |
| DENY-05 | Re-auth >5 phút, wrong purpose hoặc security/risk/policy unknown | R3/R4 |
| DENY-06 | Missing mandatory case/change/incident/reason/preview/audit | Theo row obligations |
| DENY-07 | Data requested vượt ceiling/allowlist; Secret/Provider-native value | Mọi read/export/mutation payload |
| DENY-08 | Resource version stale, hold/reference/invariant/ownership fail | Mutation/delete/publish |
| DENY-09 | Audit/operation/result store unavailable hoặc commit outcome unknown | R3/R4 new effect |
| DENY-10 | Human gọi workload-only effect hoặc action disabled | ACT-015/017/030/031 source effect |

## 7. Audit decision schema

| Field | Quy tắc |
|---|---|
| `decisionId`, `occurredAt`, `correlationId` | Opaque/server UTC; unique; no PII |
| `actorRef`, `sessionFamilyRef`, `authorizationVersion` | Protected immutable refs/current versions |
| `roleGrantRefs`, `roleCatalogVersion` | Chỉ grants góp vào decision; không display role string làm truth |
| `actionId`, `permissionId`, `resourceType/ref/version` | Stable catalog refs; protected resource ID |
| `requestedScope`, `effectiveScope`, `dataClass` | Normalized/bounded; không raw selector/query/body |
| `decision`, `reasonCategory`, `obligations` | Allow/Deny/Challenge/Error + stable category; no policy internals oracle |
| `case/change/incidentRef`, `operationId` | Required theo row; opaque |
| `effectResult`, `beforeAfterRefs` | Semantic result/protected snapshot refs; no secret/raw PII |

Denied attempts R2–R4 vẫn audit/risk signal theo budget, không chứa target existence nếu actor chưa đủ scope. Audit failure trước R3/R4 effect làm deny; after-effect uncertainty dùng durable operation/reconcile, không bỏ record im lặng.

## 8. Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M11-PERM-I01 | `[Authorize(Roles=...)]` là cơ chế chính | Không Action/Permission ID, scope, obligations hoặc current authorization version | M11-T005, T049; A-G02 |
| M11-PERM-I02 | `UserRole` enum đơn trị User/Admin/SuperAdmin | Không multi-role/scoped grants, conflict hoặc review lifecycle | M11-T005; M01-T028–T032 |
| M11-PERM-I03 | Nullable requesting user ID có thể bỏ ownership | Caller/service convention thay trusted policy decision | M02-T016; M11-T049 |
| M11-PERM-I04 | Route account/role/status/delete, balance/pet, broadcast/config/maintenance chạy trực tiếp | Không re-auth/reason/case/preview/CAS/reconcile nhất quán | M11-T005–T011, T041–T044 |
| M11-PERM-I05 | Read user/activity/log/battle/progress theo ID và broad role | Không purpose/case/data ceiling/redaction/access audit | M11-T025–T035; REL-07 |
| M11-PERM-I06 | Không thấy central policy decision/audit schema hoặc permission coverage test | Không chứng minh deny-by-default và route mới có thể mồ côi | M11-T031, T049; REL-02 |
| M11-PERM-I07 | Access JWT role claim sống lâu và session model hiện chưa authorization-versioned | Role revoke/change không chặn quyền cũ kịp thời | M01-T016, T029; M11-T005 |

I01–I07 là release blocker; matrix là target contract, không là runtime evidence.

## 9. Ma trận nghiệm thu

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| PM04-01 | Route không Action/Permission mapping | Deny + coverage gate fail |
| PM04-02 | Legacy Admin claim, không target grant | Deny; không fallback broad role |
| PM04-03 | Permission đúng, module/resource ngoài scope | Deny trước effect, audit normalized scope |
| PM04-04 | Missing/null scope | Deny, không hiểu global |
| PM04-05 | Sensitive read với mutation permission nhưng thiếu read permission | Deny; no implicit CRUD/read expansion |
| PM04-06 | R4 re-auth 6 phút | Challenge/deny, no effect |
| PM04-07 | R4 thiếu audit/case/reason/preview | Fail-closed trước mutation |
| PM04-08 | Self-grant hoặc conflict role assignment | DENY-02/03; authorization version không đổi |
| PM04-09 | Claim/grant revoked nhưng cache stale | Current version invalidates/denies |
| PM04-10 | Path target ID khác source-owned resource | Deny; client ID không authority |
| PM04-11 | Module invariant/hold/reference reject | M11 cannot override; no success/DB write |
| PM04-12 | Same mutation operation retry | One effect/audit; same semantic result |
| PM04-13 | Commit outcome unknown | HOLD/reconcile, no second operation |
| PM04-14 | Secret/raw provider field requested | Deny/redact regardless role |
| PM04-15 | Audit query vượt case/time/data ceiling | Deny/bounded; access attempt audited |
| PM04-16 | Human invokes workload-only effect | DENY-10; only controlled request/job path |
| PM04-17 | DataAnalyst requests row-level user email | Deny; aggregate/small-group policy only |
| PM04-18 | PlatformOwner calls asset/broadcast/content action | Deny absent specialized scoped role; no wildcard |
| PM04-19 | Authorization store/policy unknown on R3/R4 | Fail-closed; no cached allow extension |
| PM04-20 | Permission/catalog version changes | Migration/dual-policy canary + invalidation; unknown-safe deny |

## 10. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-PERM-F01 | M01 admin lifecycle và runtime no-bypass regression | M11-GRANT-1.0 + M11-NO-EMERGENCY-A-1.0; stale authorization/legacy-only authority deny | M01-T029–T032; M11-T049 |
| M11-PERM-F02 | R3/R4 schedule/execution behavior | Enhanced-control + request/decision contract đã chốt; no two-person assumption | M11-T010–T011 |
| M11-PERM-F03 | Audit/redaction/query/retention implementation | R2–R4 access/effect without required audit fail-closed | M11-T031–T035; REL-02 |
| M11-PERM-F04 | Route/job/workload registry, legacy migration và runtime deny suite | Legacy-only authority denied; no orphan entry point | M11-T049; A-G02 |

## 11. Tự kiểm M11-T004, A-G02 và REL-02

- Đủ 44 Action ID có Permission ID, allowed role/mode, scope/data ceiling và obligations; ACT-030 human-disabled được ghi rõ, không bỏ coverage.
- 13 role không wildcard; read/write/export/publish/delete/operate/access-admin tách biệt và explicit deny/current state luôn thắng.
- 10 deny rule + 8-step evaluation buộc trusted actor, current authorization version, re-auth, source invariant, audit và reconcile.
- 20 case bao phủ orphan route, legacy claim, scope/data/read-write separation, stale cache, R4 controls, source reject, idempotency/unknown, Secret/workload và version migration.
- 7 sai lệch + 4 finding mở có task tiếp nhận; A-G02/REL-02 còn chờ grant lifecycle, audit/runtime enforcement.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 44 permission mapping, 13 role, deny/evaluation/audit schema và 20 case | WSA-7K2 |
