# Danh mục vai trò quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T003 |
| Catalog ID / phiên bản | M11-ROLE-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-032–D-034; M11-D001–D006; M01-D016; M11-DICT-1.0; M11-ACTION-1.0 |
| Phạm vi | Role purpose, scope ceiling, exclusions, separation of duties, composition và migration legacy Admin/SuperAdmin |
| Ngoài phạm vi | Permission/action assignment chi tiết T004; grant/revoke lifecycle T005; highest-role recovery T030; runtime authorization implementation |

## 1. Bất biến

- Role là bundle trách nhiệm có version, không là kết quả authorization. Server vẫn kiểm permission, scope, state, re-auth, resource policy và Action ID.
- Không role nào—kể cả `PlatformOwner`—nhận wildcard `*`, tự động sở hữu mọi action hoặc bỏ qua module invariant/audit/limiter.
- Mỗi assignment có scope rõ; thiếu scope không được hiểu là global. Global scope chỉ cho role/action được allowlist và có lý do.
- Có thể giữ nhiều role không xung đột; permission union chỉ trong giao scope hợp lệ, explicit deny/current-state/resource policy luôn thắng.
- Không hỗ trợ temporary role, emergency privilege, impersonation hoặc shared admin account. “Khẩn cấp” dùng role cố định đã cấp trước + re-auth/reason/incident/audit.
- Không có two-person approval bắt buộc. Separation of duties được thực thi bằng cấm tổ hợp role/action tự cấp–tự kiểm toán/che giấu, không tạo phê duyệt giả.
- Role name/claim legacy không được dùng làm target permission. Assignment/change/revoke luôn tăng authorization version để chặn session/claim cũ.

## 2. Scope model

| Scope dimension | Giá trị/nguồn | Quy tắc |
|---|---|---|
| `environment` | production/staging/development từ trusted deployment identity | Production không suy từ client/header; assignment khác environment không có hiệu lực |
| `module` | M01–M12 allowlist | Module owner/action catalog quyết định; `all` chỉ exceptional assignment có expiry review, không temporary permission |
| `resourceGroup` | Content collection, config namespace, capability, product area | Protected ID/version; không path/filter tùy ý |
| `dataClassCeiling` | Public/Internal/Personal/Sensitive | Secret/Provider-native value luôn ngoài admin payload dù role cao |
| `operationScope` | Read, author, publish, operate, investigate, administer-access | Permission map T004; role không tự mở operation khác |
| `caseOrIncident` | Optional/required protected case/change/incident ref | Bắt buộc cho support/investigation/R4 action theo M11-ACTION-1.0 |
| `subjectSet` | Self, exact protected refs, bounded cohort hoặc none | Không wildcard user search/export; cohort có snapshot/preview |

Assignment global phải khai `environment=production`, exact role/version và từng module/operation; không dùng missing/null scope. Scope narrowing ở request được intersect với assignment, không replace.

## 3. Catalog vai trò

| Role ID | Mục đích | Scope ceiling | Cho phép về trách nhiệm | Loại trừ bắt buộc |
|---|---|---|---|---|
| `M11-R01 SupportAgent` | Xử lý vụ việc người dùng với dữ liệu tối thiểu | M01/M03/M04/M06/M08/M09/M10; Personal; case-bound read | Tìm/đọc đã che, timeline, tạo/cập nhật case, khởi tạo request có kiểm soát | Không role/lock trực tiếp, credential, balance/asset mutation, raw log/export/broadcast |
| `M11-R02 IdentitySecurityOperator` | Ứng phó account/session/identity security | M01; Sensitive; subject/incident bound | Review security signal, lock/revoke session, initiate owner recovery/alert theo contract | Không grant role, đặt credential/email thay user, xóa identity hoặc xem Secret |
| `M11-R03 ContentAuthor` | Tạo/sửa draft học liệu/nhiệm vụ/thành tựu | M02/M07/M08 content; Internal/Personal bounded | Author draft/version, validate, preview, submit/schedule khi policy cho phép | Không asset adjustment, config platform, user data, hard-delete published history |
| `M11-R04 ContentPublisher` | Kích hoạt/ngừng content version đã hợp lệ | M02/M07/M08 content; Internal bounded | Publish/deprecate/rollback content, review reference/impact | Không sửa draft sau decision, user/asset/config/role mutation; không bypass module validation |
| `M11-R05 AssetOperator` | Điều phối adjustment qua ledger M06 | M06; Sensitive; case/value/type limits | Read ledger đã che, preview và submit/execute adjustment trong hạn mức | Không sửa balance/ownership DB, role/user lifecycle, tự thay limit hoặc che audit |
| `M11-R06 BattleOperator` | Hỗ trợ và vận hành gym/trận | M08; Personal/Sensitive; battle/gym scope | Read battle detail case-bound, force-interrupt idempotent, change versioned gym/rule | Không sửa result/reward/history, asset ledger, user role hoặc broad maintenance |
| `M11-R07 NotificationOperator` | Quản lý template/campaign và delivery operations | M10; Personal; channel/audience ceiling | Author/version template, preview/schedule/send/stop bounded campaign, view aggregate delivery | Không raw device token, bypass consent/quiet hours/quota, arbitrary body/recipient hoặc security mail spoof |
| `M11-R08 PlatformOperator` | Vận hành configuration/capability/job/maintenance đã đăng ký | M11/M12 + module config namespaces; Sensitive metadata | Config version/rollout/rollback, job operate, scoped maintenance/kill theo incident | Không Secret value, user role/credential, asset adjustment, audit deletion hoặc module DB mutation |
| `M11-R09 DataAnalyst` | Xem metric/dashboard/report đã định nghĩa | M01–M12 aggregate; Public/Internal, Personal only approved aggregate | Dashboard/drill-down aggregate, bounded report/export đã che | Không row-level PII mặc định, mutation, support search, raw logs/audio/device data |
| `M11-R10 AuditInvestigator` | Điều tra audit/security evidence bất biến | M01/M11/M12; Sensitive metadata; incident/case-bound | Search/read/export audit allowlist, integrity check, hold request | Không mutate subject/resource, delete/edit audit, grant access hoặc vận hành system cùng scope điều tra |
| `M11-R11 PrivacyOperator` | Điều phối export/delete/retention/hold theo policy | M01 + module data maps; Sensitive; request/case-bound | Verify manifest/status, start/monitor workflows, apply approved disposition/hold metadata | Không đọc Secret/raw content không cần, direct hard-delete, change role/credential hoặc tự đặt legal basis |
| `M11-R12 AccessAdministrator` | Quản lý role assignment/scope/review/revoke | M01/M11 authorization plane; Sensitive | Grant/revoke approved non-self roles/scopes, review drift và authorization version | Không tự cấp/nâng scope, grant PlatformOwner, điều tra audit của chính assignment hoặc business mutation |
| `M11-R13 PlatformOwner` | Bảo vệ control plane và highest-role continuity | Production control plane; exact allowlist; Sensitive metadata | Quản lý PlatformOwner/AccessAdministrator theo last-owner policy, approve fixed role catalog/version | Không wildcard business action, Secret value, module invariant bypass, self-demotion/last-owner removal hoặc audit bypass |

`ContentAuthor` và `ContentPublisher` có thể cùng một người theo M11-D005 nếu assignment không xung đột khác; hệ thống không giả định hai người. Publish vẫn là action riêng, re-auth/version/impact/audit đầy đủ.

## 4. Tổ hợp role

| Pair/quy tắc | Kết quả | Lý do |
|---|---|---|
| AccessAdministrator + PlatformOwner trên cùng actor | Cấm | Có thể tự mở rộng/quản lý highest-role path |
| AccessAdministrator + AuditInvestigator cùng environment/scope | Cấm | Tự cấp và tự kiểm toán/che giấu assignment |
| PlatformOperator + AuditInvestigator cùng module/environment | Cấm | Vừa vận hành/maintenance vừa là investigator độc lập của chính effect |
| AssetOperator + AuditInvestigator cùng M06 scope | Cấm | Adjustment và điều tra ledger cùng phạm vi |
| PrivacyOperator + AuditInvestigator cùng request/hold scope | Cấm | Disposition/hold và đánh giá bằng chứng cùng vụ việc |
| IdentitySecurityOperator + AccessAdministrator cùng M01 production scope | Cấm | Lock/recovery/session response kết hợp quyền cấp role quá rộng |
| SupportAgent + DataAnalyst | Cho phép nếu scope tách | Support case read không mở row-level analyst export; intersection/deny áp dụng |
| ContentAuthor + ContentPublisher | Cho phép | Không two-person requirement; action publish riêng, current version/re-auth/audit |
| BattleOperator + AssetOperator | Cấm cùng incident/case | Không tự sửa reward sau khi can thiệp trận |
| Nhiều assignment cùng role | Hợp nhất giao scope nhỏ nhất hoặc giữ grants riêng có ID | Không biến null/union thành global; revoke từng grant xác định |

Khi phát hiện conflict, assignment mới bị từ chối trước effect; không âm thầm bỏ một permission. Existing conflict được freeze sensitive actions, tạo remediation case và thu hồi/re-auth session sau change commit.

## 5. Legacy mapping và migration

| Legacy role | Target handling | Không được làm |
|---|---|---|
| `User` | Identity người học, không phải admin role; session/scope theo M01 | Map sang SupportAgent hoặc role quản trị vì có tài khoản |
| `Admin` | Freeze new broad grants; inventory actual responsibilities rồi cấp một hay nhiều M11-R01–R11 scoped grants | Tự động map sang tất cả specialized roles hoặc giữ `[Authorize(Roles="Admin")]` như target |
| `SuperAdmin` | Freeze new grants; review actor-by-actor; chỉ designated control-plane custodian nhận M11-R13, các trách nhiệm khác cấp explicit scoped roles | Map mặc định sang wildcard/all actions, dùng shared account hoặc coi là emergency access |

Migration order: inventory actor/route/action → map intended responsibilities → simulate deny/allow → create versioned assignments → force re-auth/session authorization-version refresh → canary enforcement → remove legacy role checks/grants → reconcile zero orphan/wildcard. Không xóa legacy role cho đến khi rollback mapping và last-PlatformOwner protection được kiểm chứng; legacy và target overlap dùng deny-most-restrictive, không union rộng.

## 6. Assignment/decision payload tối thiểu

| Field | Quy tắc |
|---|---|
| `roleId`, `roleVersion`, `grantId` | Stable, immutable refs; không display role string làm key |
| `actorRef` | Protected immutable identity ref; không username/email |
| `scope` | Exact environment/module/resource/data/operation ceilings; non-null |
| `status` | Proposed/Active/Suspended/Revoked/Expired namespace authorization grant |
| `reason`, `requestRef`, `grantedBy` | Required protected metadata; actor không tự grant |
| `createdAt`, `effectiveAt`, `reviewAt`, `revokedAt` | Server UTC; fixed roles vẫn cần review, không thành temporary privilege |
| `authorizationVersion` | Increment on grant/scope/status/catalog change; current enforcement checks it |
| `conflictEvaluation` | Catalog version + pair/rule result; unknown rule fail-closed |

`reviewAt` là thời điểm phải rà soát quyền cố định, không phải automatic temporary grant. Nếu review quá hạn, R3/R4 action bị suspend/conservative theo policy T005; không tự gia hạn.

## 7. Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M11-ROLE-I01 | `UserRole` chỉ có User/Admin/SuperAdmin | Không biểu diễn 13 trách nhiệm/scope hoặc separation of duties | M11-T004–T005; M01-T028–T030 |
| M11-ROLE-I02 | Controller authorization dùng role string trực tiếp | Không role version/grant/scope/current-state decision | M11-T004, T049; A-G02 |
| M11-ROLE-I03 | Admin và SuperAdmin có broad CRUD/read across user/content/asset/log | Excess privilege và blast radius; mục đích role không rõ | M11-T004–T007 |
| M11-ROLE-I04 | Một số route chỉ Admin, một số SuperAdmin/cả hai | Không có catalog/policy reason cho khác biệt; drift khó phát hiện | M11-T004, T049 |
| M11-ROLE-I05 | Không thấy grant entity, scope, conflict rule, reviewAt hoặc authorization version | Không thể lifecycle/review/revoke stale claims an toàn | M11-T005; M01-T029–T032 |
| M11-ROLE-I06 | Không thấy admin re-auth hoặc fixed-role emergency policy enforcement | Broad session có thể làm R3/R4; quyết định “không emergency privilege” chưa chứng minh | M11-T005–T007; M01-T016, T029–T030 |

I01–I06 là release gap; catalog không thay permission matrix hoặc migration/runtime evidence.

## 8. Ma trận nghiệm thu

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| RR03-01 | SupportAgent đọc user trong case/scope | Chỉ field đã che; purpose/access audit; không mutation |
| RR03-02 | SupportAgent thử đổi role/balance | Deny dù UI/legacy Admin claim có route |
| RR03-03 | ContentAuthor publish khi không có Publisher role | Deny; draft giữ nguyên |
| RR03-04 | Cùng actor có Author + Publisher hợp lệ | Publish được sau re-auth/version/impact/audit; không đòi actor thứ hai |
| RR03-05 | AssetOperator sửa balance DB trực tiếp | Deny; chỉ ledger operation ID/preview/reconcile |
| RR03-06 | BattleOperator + AssetOperator cùng incident | Conflict reject/freeze; không can thiệp trận rồi tự bù reward |
| RR03-07 | PlatformOperator yêu cầu secret value | Deny/redact; chỉ secret reference/rotation operation |
| RR03-08 | AccessAdministrator tự grant/nâng scope | Deny + high audit/alert; no grant effect |
| RR03-09 | AccessAdministrator đồng thời AuditInvestigator | Assignment conflict reject trước activation |
| RR03-10 | Scope field null/missing | Deny; không hiểu global |
| RR03-11 | Grant revoked nhưng JWT role cũ còn hạn | Current authorization version deny; session re-auth/revoke theo policy |
| RR03-12 | ReviewAt quá hạn | R3/R4 suspend/conservative, không tự gia hạn hoặc xóa history |
| RR03-13 | Incident cần quyền khẩn cấp | Dùng fixed scoped role; không mint temporary/emergency privilege |
| RR03-14 | Legacy Admin migration | Explicit role/scope simulation; deny-most-restrictive during overlap |
| RR03-15 | Legacy SuperAdmin là last holder | Không xóa trước designated PlatformOwner/rollback/continuity check |
| RR03-16 | Module owner rejects action | Role không bypass invariant hoặc ghi nguồn trực tiếp |
| RR03-17 | User bị xóa/khóa | Admin grant/session không còn effect theo current M01 state; audit refs giữ |
| RR03-18 | Role catalog version changes | New authorization version, conflict reevaluation, session/cache invalidation và rollback plan |

## 9. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-ROLE-F02 | Grant/revoke/review/session invalidation lifecycle | Không self-grant/temp/emergency; stale authorization fail-closed | M11-T005–T006-A; M01-T029–T032 |
| M11-ROLE-F03 | R3/R4 action classification/reason/change workflow | Re-auth ≤5 phút + reason/audit; không two-person giả | M11-T007–T011 |
| M11-ROLE-F04 | Legacy route/role migration và runtime allow/deny/conflict tests | Freeze broad grants, explicit mapping, deny-most-restrictive | M11-T049; A-G02; REL-02 |

## 10. Tự kiểm M11-T003, A-G02 và REL-02

- 13 role có purpose, scope ceiling, responsibility và exclusion; không role wildcard hoặc source-invariant bypass.
- Bảy scope dimension, 10 composition/conflict rule và assignment payload chặn missing-global, self-grant, self-audit và stale authorization.
- Legacy User/Admin/SuperAdmin có migration/freeze/canary/rollback rõ; không auto-map broad role sang target permission.
- 18 case bao phủ least privilege, multi-role, conflict, review, emergency request, migration, current state và catalog version.
- 6 sai lệch + 3 finding mở có task tiếp nhận; M11-ROLE-F01 đã được đóng bởi M11-PERM-1.0; A-G02/REL-02 còn chờ grant lifecycle và runtime evidence.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 13 role, scope/composition/conflict, legacy migration và 18 case | WSA-7K2 |
