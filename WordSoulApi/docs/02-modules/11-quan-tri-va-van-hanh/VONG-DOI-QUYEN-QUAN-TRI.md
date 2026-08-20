# Vòng đời cấp và thu hồi quyền quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T005 |
| Contract ID / phiên bản | M11-GRANT-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-028, D-032–D-036; M11-D001–D006; M11-ROLE-1.0; M11-PERM-1.0; M01-SESSION-1.0 |
| Phạm vi | Request/grant/scope change/review/suspend/revoke, conflict/last-owner guard, session/cache effect, concurrency, audit và migration |
| Ngoài phạm vi | Kiểm chứng no-temporary/emergency T006-A; action approval/change T007–T011; highest-role business recovery T030; runtime implementation |

## 1. Bất biến

- Chỉ role cố định trong M11-ROLE-1.0 được cấp; không ad-hoc permission, wildcard, role nesting, temporary/emergency grant, shared actor hoặc impersonation.
- Actor không tự grant, nâng scope, giữ assignment của chính mình, tự xác nhận review hoặc tự gỡ conflict. R12 quản lý non-R13; R13 quản lý R12/R13 continuity theo last-owner guard.
- Grant không có scope hoặc scope unknown bị từ chối; mọi thay đổi scope là assignment version mới/CAS, không sửa row âm thầm.
- Một authorized actor được tự thực hiện grant/revoke đúng quyền; không tạo two-person approval bắt buộc. Re-auth ≤5 phút, reason, request record và immutable audit là bắt buộc.
- Quyền mới không xuất hiện trong session cũ. Grant/scope expansion tăng authorization version và buộc target mở admin session mới/re-auth; revoke/suspend/scope reduction chặn ngay mọi access cũ.
- Current assignment/account/session/security/policy/conflict/review state là truth tại enforcement point. JWT/cache/UI/DB replica stale không kéo dài quyền.
- Audit/grant/session commit không chắc chắn thì không trả applied; reconcile cùng operation ID. Không retry thành request mới hoặc tự phục hồi grant đã revoke.

## 2. Hai state machine tách biệt

### Grant-change request

| State | Ý nghĩa | Chuyển hợp lệ |
|---|---|---|
| `Draft` | Actor nhập target/role/scope/reason, chưa có effect | Validating, Cancelled |
| `Validating` | Kiểm actor/target/catalog/scope/conflict/least-privilege/re-auth/current state | Ready, Rejected, Cancelled |
| `Ready` | Snapshot/version hợp lệ cho commit ngay; không phải approval | Applying, Expired, Cancelled |
| `Applying` | CAS/audit/session effect đang commit | Applied, Failed, Unknown |
| `Applied` | Assignment version/effect đã commit | Final |
| `Rejected` | Validation/policy từ chối, không effect | Final; request mới nếu input đổi |
| `Failed` | Chắc chắn không commit | Retry cùng operation nếu policy/deadline cho phép |
| `Unknown` | Outcome chưa biết | Reconcile → Applied/Failed; không apply lần hai |
| `Expired`/`Cancelled` | Deadline qua hoặc actor hủy trước Applying | Final, không effect |

Không có `PendingApproval`/`Approved`: product decision không yêu cầu hai người. `Ready` chỉ nói validation snapshot hợp lệ và hết hạn sau 10 phút hoặc ngay khi actor/target/catalog/scope/resource/policy version đổi.

### Role assignment

| State | Ý nghĩa | Hành vi permission |
|---|---|---|
| `Active` | Fixed assignment đã commit, review còn hạn, account/policy hợp lệ | Permission trong exact scope có thể được xét |
| `Suspended` | Tạm chặn do review quá hạn, conflict/risk/account/policy/owner remediation | Deny mọi R3/R4; R0–R2 chỉ theo explicit incident/support policy, mặc định deny admin action |
| `Revoked` | Thu hồi cuối, immutable history | Deny; không restore row, cấp lại tạo grant/version mới |

Assignment không có `Temporary` hoặc automatic expiry-based privilege. `reviewAt` là deadline governance; quá hạn chuyển `Suspended`, không tự gia hạn và không xóa history. Proposed/rejected state thuộc request, không thuộc assignment truth.

## 3. Request và validation

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M11-GRANT-1.0` |
| `operationId` | Có | Opaque ≥128 bit, stable qua retry cùng intent |
| `changeType` | Có | `grant`, `narrow_scope`, `expand_scope`, `suspend`, `resume`, `revoke`, `review_keep` |
| `targetActorRef` | Có | Protected immutable M01 ref; không email/username/path actor tự khai |
| `roleId`, `roleVersion` | Có | Published M11-ROLE version; unknown/deprecated deny |
| `expectedGrantVersion` | Có khi assignment tồn tại | CAS; stale request reject với current protected metadata |
| `requestedScope` | Có | Exact environment/module/resource/data/operation/case ceilings; non-null |
| `reasonCode`, `reasonText`, `requestRef` | Có | Code allowlist + bounded redacted text + immutable request/case reference |
| `reviewAt` | Server tính | R12/R13 và R02/R05/R08/R10/R11: 90 ngày; R01/R03/R04/R06/R07/R09: 180 ngày |
| `correlationId`, `clientContext` | Có | Opaque trace + trusted admin channel/device evidence; không authority flag |

Validation order:

1. Schema/version/rate/size and trusted actor/admin session; current account/security/authorization/policy known; re-auth ≤5 phút đúng `manage-authorization` purpose.
2. Load actor grant and check permission `m11.authorization.grant.change`, scope ceiling and R12/R13 boundary.
3. Load target M01 state and current assignment set; locked/deleting/deleted target không nhận/expand/resume role, nhưng revoke vẫn được phép.
4. Validate published role version, exact scope, requested responsibility/evidence and least-privilege diff; no missing/global inference.
5. Evaluate M11-ROLE conflict pairs, self-grant/self-review, environment overlap, last-PlatformOwner and deny rules.
6. Build before/after permission/scope diff and affected admin-family count; actor confirms exact snapshot within Ready TTL.
7. CAS commit request + assignment + target authorization version + session action + audit + notification outbox.

## 4. Quyền thực hiện thay đổi

| Thay đổi | Actor đủ quyền | Guard bổ sung |
|---|---|---|
| Grant/revoke R01–R11 | R12 AccessAdministrator | Không self/over-scope/conflict; exact scope; target eligibility |
| Grant/revoke R12 | R13 PlatformOwner | Không self; phải còn ≥1 R13 Active khác hoặc same transaction tạo continuity an toàn |
| Grant/revoke R13 | R13 PlatformOwner khác | Không self; last-owner protection; target dedicated control-plane eligibility |
| Narrow/suspend/revoke do security | R12 cho R01–R11; R13 cho R12/R13; R02 chỉ tạo security request/suspend-session signal | IdentitySecurityOperator không tự sửa grant truth |
| Review keep | R12 cho R01–R11; R13 cho R12/R13 | Reviewer không là target; re-evaluate current role/scope/conflicts/use evidence |
| Resume | Cùng authority như grant | Nguyên nhân suspension đã resolved + full validation/re-auth; không restore session cũ |

Không có delegation từ R12/R13 cho actor khác ngoài versioned role assignment. PlatformOwner không được grant business permission trực tiếp; chỉ catalog/continuity actions được M11-PERM-1.0 cho phép.

## 5. Session, cache và hiệu lực

| Change | Authorization version | Admin sessions target | Thời điểm effect |
|---|---|---|---|
| Grant/scope expansion | Tăng | Revoke/suspend mọi admin family; target login/re-auth tạo session mới | Permission mới chỉ sau commit + new session/current decision |
| Scope narrowing | Tăng | Revoke mọi admin family có grant/version cũ | Deny old scope ngay tại source/version gate |
| Suspend/review overdue | Tăng | Revoke/suspend mọi admin family | Deny admin action ngay; không chờ JWT expiry |
| Revoke | Tăng | Revoke all admin/support families bound actor; alert | Monotonic final; regrant tạo new grant ID/version |
| Review keep, không đổi scope | Tăng review version | Existing family re-auth required trước R3/R4; read policy có thể continue nếu current version resolver confirms | ReviewAt mới sau audit commit |
| Role catalog/conflict policy version đổi | Tăng cho affected actors hoặc global policy epoch | Re-evaluate/invalidate affected family/cache | Deny unknown/stale until migration complete |

Decision/cache key chứa actor authorization version, role/grant/catalog versions, action/permission, normalized scope/resource/policy/environment. Revocation invalidation là optimization; API phải kiểm durable/current version hoặc revocation index. Cache/store unavailable trên admin action fail-closed.

## 6. Review và thu hồi tự động theo nguồn sự thật

| Trigger | Hành vi |
|---|---|
| `reviewAt` đến hạn | Durable job CAS Active → Suspended nếu chưa review; alert target/owner; no auto-renew |
| Target account locked/inactive/deleting/deleted | Suspend hoặc revoke theo M01 lifecycle; admin sessions revoke; deleted không regrant old identity |
| Employment/responsibility ended signal đã xác minh | Revoke affected grants; session/cache invalidation; preserve audit refs |
| Scope/resource/module retired | Narrow/revoke/deprecate grant after reference analysis; no wildcard fallback |
| Conflict policy/catalog change | Suspend overlapping grants conservatively; remediation request decides narrow/revoke |
| Security compromise | Revoke/suspend session immediately; grant suspension/revoke only from verified security policy/authorized request, not failed-login count alone |
| Last PlatformOwner risk | Block self/last revoke; open continuity incident/case; no emergency privilege minting |

Review evidence gồm responsibility owner, last-use aggregate (không raw activity), scope need, conflicts, account/security state, outstanding incidents and proposed outcome. “Không dùng gần đây” là signal, không tự revoke nếu ownership/continuity policy chưa xác nhận; overdue vẫn suspend R3/R4.

## 7. Đồng thời và idempotency

| Tình huống | Kết quả xác định |
|---|---|
| Same operation retry | Một request/effect/audit/notification; trả committed non-secret result |
| Hai grant cùng role/scope | Unique active-grant constraint + CAS; một grant, request kia idempotent/conflict |
| Expand và revoke race | Expected grant/authorization version; một winner, loser stale/reconcile |
| Review keep và automatic overdue suspend race | Store time/CAS quyết định; review chỉ giữ Active nếu commit trước deadline với current snapshot |
| Two actors edit same grant | Một CAS winner; loser thấy current version/diff, không last-write-wins |
| Role catalog changes during Ready | Snapshot invalid, request Rejected/Expired; revalidate new catalog |
| Session issuance races revoke | Authorization-version/session commit boundary; old/new session denied or revoked if grant change wins |
| Commit timeout | Request Unknown; reconcile `(operationId, target, grant)` before any new effect |

## 8. Audit, notification và privacy

| Event | Metadata allowlist |
|---|---|
| `authorization.change.requested` | Actor/target protected refs, change type, role/grant/scope protected refs, reason/request/operation |
| `authorization.change.validated_or_rejected` | Policy/catalog/version, conflict/deny category, before/after permission count bucket |
| `authorization.grant.activated_or_changed` | Grant/version, role/version, scope digest/summary, target authorization version |
| `authorization.grant.suspended_or_revoked` | Grant/version, trigger/reason, effectiveAt, session family count bucket |
| `authorization.review.completed_or_overdue` | Grant/review version, reviewer ref, outcome, next reviewAt or suspension |
| `authorization.session.invalidated` | Target ref, authorization/security version, family count bucket, result |
| `authorization.change.dependency_failed` | Dependency/category, operation, commit-known/unknown, retry class |

Cấm email/username đầy đủ, password/token/code, raw scope selector/query, raw IP/device, free-form employment/HR data, secret/provider payload và request body. Scope audit dùng normalized allowlist + keyed digest/protected refs; before/after export chỉ qua quyền audit/case.

Target và responsibility owner nhận mandatory security notification khi grant/expand/narrow/suspend/resume/revoke/review-overdue; notification lỗi không rollback quyền/session effect, outbox pending + operations alert. Dedup theo grant version + change type.

## 9. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| M01 state/session hoặc authorization store unavailable | Không grant/resume/review; revoke request giữ durable pending/deny path; admin actions fail-closed |
| Role/permission/conflict policy unknown/stale | Không activate/expand; existing R3/R4 deny/conservative until resolved |
| External audit sink unavailable | Grant/expand/resume/review-keep deny; narrow/suspend/revoke chỉ commit khi authorization store ghi atomic durable audit outbox, nếu không affected admin actions fail-closed đến khi đối soát |
| Session invalidation store/cache unavailable | Commit authorization version/revoke truth; all admin APIs fail-closed for target until propagation reconciled |
| Notification unavailable | Change stays committed; outbox pending/alert |
| CAS/commit outcome unknown | Request Unknown + reconcile; no second assignment/effect |
| Review scheduler unavailable | Enforcement checks `reviewAt` on request and denies/suspends R3/R4; alert job health |
| Clock anomaly | Stop activation/review extension; server/store time only, no client timestamps |

## 10. Legacy migration

1. Freeze new legacy Admin/SuperAdmin assignment; inventory exact actors/routes/responsibilities/scopes.
2. Create proposed target grants with M11-ROLE/PERM versions; simulate 44 action allow/deny and conflict/last-owner checks.
3. Authorized R13 seeds first controlled R12/R13 continuity under audited migration operation; no shared/emergency account.
4. Apply scoped grants actor-by-actor; increment authorization version and revoke legacy admin sessions.
5. Require new admin login/re-auth; canary target enforcement with deny-most-restrictive while legacy checks coexist.
6. Remove legacy role authority route-by-route only after coverage/evidence; reconcile no orphan action, wildcard, self/conflict grant or last-owner gap.
7. Deprecate legacy enum assignment path; retain immutable mapping history for audit/rollback window.

Rollback re-enables last verified target mapping/version, not broad legacy wildcard. If migration breaks access, use existing fixed R13 continuity actors and documented restore—not temporary privilege.

## 11. Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M11-GRANT-I01 | Role là enum field đơn trên `User`, cập nhật bằng role name | Không multi-grant/scope/version/review/history/CAS | M01-T029–T032; M11-T049 |
| M11-GRANT-I02 | Admin/SuperAdmin có thể gọi assign-role route | Không thấy self-grant/scope/conflict/last-owner/re-auth guard đầy đủ | M01-T029–T030; M11-T049 |
| M11-GRANT-I03 | Access token mang role claim và sống lâu; current authorization version chưa thấy | Grant/revoke có thể không mất/nhận hiệu lực đúng session boundary | M01-T016, T029; A-G02 |
| M11-GRANT-I04 | Không có grant request/reviewAt/suspend/revoke history model | Không chứng minh review định kỳ, reason/owner hoặc monotonic revoke | M11-T049; REL-02 |
| M11-GRANT-I05 | Không thấy conflict evaluator hoặc temporary/emergency-path coverage | Separation/no-emergency quyết định chưa được cưỡng chế | M11-T006-A, T049 |
| M11-GRANT-I06 | Activity logs dùng username/action detail cho một số admin changes | Không immutable authorization audit schema, protected refs/before-after/session result | M11-T031–T035 |

I01–I06 là release blocker; hiện trạng không có runtime evidence cho grant lifecycle.

## 12. Ma trận nghiệm thu

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| GL05-01 | R12 grant R01 exact scope hợp lệ | One Active grant/version; target auth version tăng; sessions revoke; notification/audit |
| GL05-02 | R12 grant R13 hoặc over-scope | Deny; no assignment/version effect |
| GL05-03 | Actor tự grant/review/nâng scope | Deny + high audit/alert |
| GL05-04 | Missing/null/global-unspecified scope | Reject; no inferred global |
| GL05-05 | Conflict pair same environment/scope | Reject before Active; current grants unchanged |
| GL05-06 | Grant/scope expansion with target session | Permission mới chỉ sau new admin session/re-auth |
| GL05-07 | Narrow/revoke with stale JWT/cache | Current auth version denies immediately; sessions revoked |
| GL05-08 | Grant duplicate same operation | One grant/effect/audit/notification |
| GL05-09 | Two actors edit same grant | One CAS winner; loser stale, no overwrite |
| GL05-10 | Review keep by target itself | Deny; no reviewAt extension |
| GL05-11 | ReviewAt overdue | Assignment Suspended/R3-R4 deny even scheduler/cache failure |
| GL05-12 | Resume after remediation | Full validation/re-auth, new version/session; old session not restored |
| GL05-13 | Target locked/deleting/deleted | Grant/expand/resume deny; revoke allowed; sessions deny |
| GL05-14 | Last R13 self/revoke attempt | Block + continuity incident; no emergency privilege |
| GL05-15 | Catalog/conflict version changes | Affected grants re-evaluate/suspend; no stale broad allow |
| GL05-16 | Session issuance races revoke | Authorization version boundary prevents active stale privilege |
| GL05-17 | External audit sink unavailable | No privilege increase; reduction chỉ commit với durable audit outbox, nếu không affected admin actions fail-closed |
| GL05-18 | Session cache/notification unavailable after commit | Durable version/revoke wins; APIs deny, alert/outbox pending |
| GL05-19 | Commit timeout unknown | Reconcile same operation; no duplicate assignment/change |
| GL05-20 | Legacy migration overlap | Target/legacy deny-most-restrictive; zero wildcard/orphan/conflict required |

## 13. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-GRANT-F01 | Runtime/schema/deployment regression không có temporary/emergency/bypass path | Negative evidence tĩnh đã có; contract cấm và chỉ fixed R13 continuity | M11-T049; A-G02; REL-02/REL-03 |
| M11-GRANT-F02 | Sensitive schedule/execution workflow | EC class/limit + immutable request/decision đã chốt; re-auth/audit/idempotency giữ nguyên | M11-T010–T011 |
| M11-GRANT-F03 | Authorization audit/redaction/access/retention implementation | Mutation thiếu audit fail-closed; protected refs only | M11-T031–T035; REL-02 |
| M11-GRANT-F04 | Grant store/schema/cache/session migration và runtime suite | Legacy broad grants frozen; no release before canary/reconcile | M11-T049; M01-T029–T032; A-G02 |

## 14. Tự kiểm M11-T005, A-G02 và REL-02

- Request và assignment state tách biệt; không có PendingApproval/Temporary/Emergency, revoke monotonic và review overdue suspend an toàn.
- R12/R13 authority, self/conflict/last-owner/scope guard, cadence 90/180 ngày và 10-minute Ready snapshot đã chốt.
- Grant/expand chỉ hiệu lực ở session mới; narrow/suspend/revoke tăng authorization version và chặn session/cache cũ ngay.
- 20 case bao phủ self/over-scope/conflict, grant/review/revoke, session/cache race, target lifecycle, last owner, failure/unknown và migration.
- 6 sai lệch + 4 finding mở có task tiếp nhận; A-G02/REL-02 còn chờ no-emergency verification, audit/store/runtime evidence.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt grant/review/suspend/revoke, session invalidation, migration và 20 case | WSA-7K2 |
