# Đặc tả yêu cầu thay đổi quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CHANGE-REQUEST-1.0` |
| Task | M11-T008 |
| Đầu vào | M11-ACTION-1.0, M11-PERM-1.0, M11-ENHANCED-CONTROL-1.0 |
| Quyết định nền | D-001, D-008, D-032–D-038; M11-D005–D010, D021–D023 |
| Tự kiểm | A-G02, A-G06; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và phạm vi

`ChangeRequest` là hồ sơ bất biến mô tả **ý định** trước effect: mục đích, đối tượng, before/after, tác động, kiểm chứng, lịch, theo dõi và đường quay lại. Nó không phải permission, approval, job hay audit event và không cho phép M11 chiếm durable truth của module nguồn.

- Áp dụng cho phần mutation của 31 action: M11-ACT-003–007, 009–013, 015, 017, 019, 021–022, 024–025, 027–028, 030–031, 033–034 và 036–043.
- Mười ba action chỉ đọc (001, 002, 008, 014, 016, 018, 020, 023, 026, 029, 032, 035, 044) dùng purpose/case/access audit, không tạo change request giả.
- M11-ACT-037 chỉ cần change request cho export/hold mutation; search/detail read vẫn theo case và access audit.
- Một actor có fixed role phù hợp có thể tạo và tự quyết định revision của mình theo D-001/M11-D005–D006. Không có `approver`, two-person gate, temporary/emergency privilege hay impersonation.
- Incident/containment có thể yêu cầu hiệu lực ngay, nhưng vẫn là action EC-4 bằng quyền cố định, có reason, incident ref, audit, stop/recovery; chữ “khẩn” không tạo đường bypass.

## 2. Loại yêu cầu

| Type ID | Phạm vi | Action điển hình | Owner quyết định invariant |
|---|---|---|---|
| CR-AUTH | Role/permission/restriction/session | 003–005 | M01/M11 authorization source |
| CR-DATA | Export, deletion, retention/disposition | 006–007, 019, 031, 037-hold/export | Module dữ liệu + privacy/hold policy |
| CR-CONTENT | Author/publish/merge/deprecate content/rule/template | 009–013, 022, 024–025, 028, 033 | Module M02/M06/M07/M08/M10 |
| CR-ASSET | Ledger/asset adjustment | 021 | M06 ledger/catalog |
| CR-COMM | Broadcast campaign | 034 | M10 delivery/consent |
| CR-CONFIG | Configuration/capability/limiter change | 036, 042 | Registry + module nguồn/M12 |
| CR-JOB | Rebuild/backfill/run/rerun/compensate | 017, 038 | Job registry + module nguồn |
| CR-SUPPORT | Remediation/case mutation | 015, 030, 039 | Module nguồn + support case |
| CR-OPS | Incident/maintenance/secret lifecycle | 027, 040–041, 043 | M08/M11/M12 source contract |

## 3. Identity, revision và tính bất biến

| Trường | Contract |
|---|---|
| `requestId` | UUID server tạo, ổn định cho toàn chuỗi revision |
| `revision` | Số nguyên tăng liên tục từ 1; mỗi lần lưu server tạo record mới, không update record cũ |
| `revisionId` | `{requestId}:{revision}`; là đơn vị validate/preview/decision/execute |
| `supersedesRevisionId` | Revision liền trước hoặc null ở revision 1; không cho nhánh mơ hồ |
| `canonicalDigest` | Hash canonical của toàn bộ decision input allowlist; loại metadata trình bày |
| `createdAt`, `createdByActorId` | UTC server time và actor nội bộ; display name không là identity |
| `sealedAt` | Có khi gửi sang bước quyết định; sealed revision không sửa/xóa |
| `validationState` | `notEvaluated`, `invalid`, `ready`; đây không phải lifecycle quyết định T009 |
| `expiresAt` | Tính từ `sealedAt`: EC-2 tối đa 30 ngày, EC-3 tối đa 7 ngày, EC-4 tối đa 24 giờ |

Thay Action ID, target/scope, operation, expected version, impact/preview, schedule, limit, rollback hoặc evidence tạo revision mới và digest mới. Decision/execution chỉ tham chiếu đúng `revisionId + canonicalDigest`; revision mới không kế thừa decision cũ.

## 4. Schema bắt buộc

Ký hiệu: `A` bắt buộc; `C` theo điều kiện ghi ở cột quy tắc; `—` không áp dụng. Client có thể đề xuất dữ liệu nhưng actor, permission/scope, effective EC class, current version và policy version phải được server resolve lại.

| Nhóm/trường | EC-2 mutation | EC-3 | EC-4 | Quy tắc |
|---|---|---|---|---|
| Identity: request/revision/action/type | A | A | A | Stable ID, Action ID đã đăng ký |
| Actor, fixed assignment, permission/scope snapshot | A | A | A | Server-derived, current-state; không nhận trusted actor từ body |
| Resource owner/type/IDs/environment/scope | A | A | A | Exact target; selector chỉ dùng khi action cho bulk |
| Purpose và reason code/text | A | A | A | Text 10–1.000 ký tự; không PII/secret/raw payload |
| Case/change/incident reference | C | A theo catalog | A | Protected internal ref; tồn tại, đúng scope/trạng thái |
| Before reference + expected version | A | A | A | Snapshot/ref từ source; không copy full sensitive payload |
| Desired operations + after summary | A | A | A | Typed allowlist; cấm SQL/script/provider payload/direct DB instruction |
| Effective EC class + modifier + Limit ID/version | A | A | A | Server-calculated; chỉ nâng, không hạ risk floor |
| Validation evidence refs | A | A | A | Module validation, permission/scope/current-state, data/secret policy |
| Impact/preview ref + digest/freshness | C | A | A | EC-2 cần khi effect không hiển nhiên; không lưu sample PII |
| Affected count/cost/risk/assumption summary | C | A | A | Bound và nguồn dữ liệu rõ; unknown không thành zero |
| Schedule mode/time/window/timezone | A | A | A | EC-2 có thể `immediate`; EC-3/4 mặc định absolute UTC schedule |
| Conflict keys/lease scope | C | A | A | Tập resource/policy/environment có thể overlap |
| Rollback/recovery plan | A | A | A | Theo RB-1..RB-3; “không cần” không hợp lệ |
| Verification/monitoring/stop criteria | C | A | A | Chỉ số, nguồn, baseline, cửa sổ và chủ hành động |
| Notification plan | C | C | A khi catalog có N | Recipient class/template/timing; không nhúng PII |
| Client idempotency key | A | A | A | Unique theo actor + intent tối thiểu 24 giờ; payload mismatch là conflict |
| Canonical digest/sealed/expiry | A khi submit | A | A | Server tạo; không cho client tự khai digest/time |

## 5. Desired operation contract

| Trường | Yêu cầu |
|---|---|
| `operationType` | Enum đã đăng ký theo Action ID, ví dụ `setVersion`, `activate`, `deprecate`, `revoke`, `adjust`, `schedule`, `stop`, `rotateReference` |
| `resourceRef` | Protected ref + source owner; không URL tùy ý, secret value hoặc raw payload |
| `expectedVersion` | Bắt buộc cho mọi mutable source; `null` chỉ khi create và source xác nhận chưa tồn tại |
| `typedParameters` | Schema/version allowlist; unknown field bị reject, không silently ignore |
| `preconditions` | Current state, policy/config/dependency version, hold/reference/consent/limit snapshot phù hợp action |
| `postconditionSummary` | Metadata dự kiến để verify; không được dùng làm success truth trước execution |

Một request có nhiều operation chỉ khi registry khai atomic policy set hoặc bulk contract. Thứ tự, dependency, atomicity/partial semantics và compensation phải explicit; không biểu diễn bulk bằng danh sách route đơn tùy ý.

## 6. Impact và evidence bundle

| Evidence ref | Nội dung tối thiểu | Freshness tối đa |
|---|---|---|
| `validationRef` | Validator version, source versions, pass/fail/error codes | Đến khi input/source version đổi |
| `previewRef` | Revision digest, selector snapshot, affected count/range, exclusions, assumptions | EC-2 30 ngày; EC-3 7 ngày; EC-4 24 giờ và không vượt expiry revision |
| `referenceRef` | Inbound dependency/hold/retention compatibility | Đến khi dependency version/hold state đổi |
| `limitRef` | Limit ID/version, evaluated value/range, actor role ceiling | Đến khi limit/assignment/target/value đổi |
| `rollbackTestRef` | Dry-run/sandbox/previous execution hoặc validator chứng minh plan executable | Theo source version; EC-4 phải có kết quả trên revision hiện tại |
| `monitoringRef` | Metric/source/baseline/freshness/threshold/owner | Đến khi metric contract hoặc baseline window đổi |

Artifact chỉ lưu protected ref, digest và metadata allowlist. Evidence missing, stale, unreadable, mismatched revision hoặc trả `unknown` làm request `invalid`; không coi timeout/null là pass.

## 7. Lịch và hiệu lực đề xuất

- `schedule.mode`: `immediate`, `absolute`, hoặc `maintenanceWindow`; recurring/relative client time không hợp lệ.
- Lưu `effectiveAtUtc`, `windowStartUtc`, `windowEndUtc`; `displayTimezone` chỉ để hiển thị và phải là IANA/Windows mapping đã chuẩn hóa. UTC là nguồn so sánh.
- EC-2 được `immediate` sau khi ready/decision hợp lệ. EC-3/EC-4 dùng `absolute` hoặc `maintenanceWindow`, trừ incident containment/revoke/stop đã đăng ký có thể `immediate`.
- `effectiveAtUtc` phải trước `expiresAt` và đủ thời gian notification/monitoring; lịch nằm quá expiry bị reject, không tự gia hạn.
- T008 chỉ đặc tả dữ liệu lịch; conflict/lease, reschedule/cancel và DST behavior được chốt ở M11-T010.

## 8. Rollback/recovery plan

| Class | Khi dùng | Trường bắt buộc | Điều kiện submit |
|---|---|---|---|
| RB-1 Version rollback | Cấu hình/content/rule/template có phiên bản trước tương thích | Target previous version, compatibility, trigger, owner, verify/stop window | Previous version tồn tại và validator xác nhận apply được |
| RB-2 Compensating action | Ledger/job/notification hoặc effect không transactionally đảo | Compensation Action ID, bounded manifest, idempotency, residual risk, reconcile | Module nguồn đăng ký compensation và limit; không “sửa DB” |
| RB-3 Contain/forward recover | Delete, revoke, secret rotation, incident hoặc effect không thể phục hồi nguyên trạng | Irreversibility reason, prevention/backup/hold, stop/contain, forward fix, user/data handling | Actor xác nhận irreversible flag; preview/evidence chứng minh safeguards |

Mọi request mutation phải chọn đúng một class. Không có plan, plan dùng action chưa đăng ký, target version không tồn tại, compensation vượt quyền/limit hoặc plan chứa secret/raw payload đều chặn `ready`.

## 9. Validation pipeline và error taxonomy

| Bước | Kiểm tra | Error code chính |
|---|---|---|
| CR-V01 | Schema/version/unknown field/size/canonicalization | `schemaInvalid` |
| CR-V02 | Action/type/operation registry coverage | `actionUnregistered` |
| CR-V03 | Actor/session/reauth/assignment/permission/scope current-state | `authorityDenied`, `reauthRequired` |
| CR-V04 | Target existence/owner/current version/precondition | `targetInvalid`, `versionConflict` |
| CR-V05 | EC modifier/Limit ID/value/quota | `riskOrLimitInvalid` |
| CR-V06 | Module business/cross-dependency validation | `sourceRejected`, `dependencyUnknown` |
| CR-V07 | Evidence/preview/reference freshness và digest | `evidenceMissing`, `evidenceStale` |
| CR-V08 | Schedule/window/expiry/timezone | `scheduleInvalid` |
| CR-V09 | Conflict-key completeness/preliminary overlap | `conflictScopeInvalid` |
| CR-V10 | Rollback/recovery executability | `rollbackInvalid` |
| CR-V11 | Monitoring/notification/stop criteria | `controlPlanInvalid` |
| CR-V12 | Audit reservation và secret/PII/payload policy | `auditUnavailable`, `dataBoundaryViolation` |

Validation chạy lại khi submit và trước decision/execution. Một lỗi làm `validationState=invalid`; response trả field-safe error codes, không echo secret/raw input. Chỉ `ready` revision được T009 nhận để quyết định.

## 10. API/command semantics tối thiểu

| Command | Hành vi |
|---|---|
| Create revision | Nhận idempotency key + typed intent; tạo revision mới hoặc trả cùng result nếu fingerprint trùng |
| Validate | Resolve server truth, chạy CR-V01–V12, lưu result/digest/version refs; không effect |
| Generate/attach preview | Tạo evidence ref gắn revision digest; preview không cấp permission |
| Seal/submit | Chỉ khi validation `ready`; set sealedAt/expiresAt atomically, immutable từ đó |
| Read | Theo permission/scope/case; field allowlist, pagination và access audit |

Retry cùng idempotency key nhưng fingerprint khác trả conflict. Timeout khi chưa chắc create/submit đã commit phải tra theo key; không tạo request/revision mới mù.

## 11. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CR-G01 | 31 mutation action map đúng request type; 13 read action không bị dùng change request để thay access control |
| CR-G02 | Mỗi persisted save tạo immutable revision/digest; sealed revision không sửa/xóa |
| CR-G03 | EC-2/3/4 field matrix, expiry 30d/7d/24h và modifier risk được enforce server-side |
| CR-G04 | Evidence stale/missing/unknown và rollback invalid đều không thể `ready` |
| CR-G05 | Không approver/two-person/temp/emergency/impersonation hoặc authority từ client body |
| CR-G06 | Typed operation không cho SQL/script/secret/raw payload/direct DB instruction |
| CR-G07 | Idempotency/timeout/concurrent revision không tạo duplicate hoặc ghi đè |
| CR-G08 | Module-source reject/version conflict giữ nguyên; M11 không báo success giả |
| CR-G09 | Schedule UTC/expiry và incident immediate exception có kết quả xác định |
| CR-G10 | Audit unavailable/data-boundary violation fail-closed trước seal/effect |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CR08-01 | EC-4 thiếu rollback plan | `rollbackInvalid`; không seal |
| CR08-02 | Irreversible delete chọn RB-1 nhưng không previous version | Reject; yêu cầu RB-3 + safeguards |
| CR08-03 | Preview thuộc revision 2 gắn revision 3 | `evidenceStale`; re-preview |
| CR08-04 | Client khai EC-2 cho Action risk R4 | Server nâng EC-4; field EC-4 thiếu thì invalid |
| CR08-05 | Actor ID/role trong body khác server identity | Bỏ dữ liệu client và deny/audit anomaly |
| CR08-06 | Same idempotency key, same fingerprint | Trả cùng request/revision result |
| CR08-07 | Same key, khác target/operation | Conflict; không tạo revision |
| CR08-08 | Expected version stale | `versionConflict`; không seal/effect |
| CR08-09 | Schedule EC-4 sau expiry 24 giờ | `scheduleInvalid`; không tự kéo dài expiry |
| CR08-10 | Incident containment immediate | Cho phép nếu action/permission/incident/EC-4 plan hợp lệ; không bypass |
| CR08-11 | typedParameters chứa secret value | `dataBoundaryViolation`; reject, không persist/echo |
| CR08-12 | Module validator timeout | `dependencyUnknown`; không coi pass |
| CR08-13 | Bulk selector 1.001 item | `riskOrLimitInvalid`; không chia ngầm |
| CR08-14 | Sealed revision bị update | Reject; tạo revision mới và decision cũ không chuyển theo |
| CR08-15 | Audit store không reserve được lúc seal | `auditUnavailable`; fail-closed |
| CR08-16 | Read-only support action tạo request để vượt case | Vẫn deny theo access control; request không cấp authority |
| CR08-17 | Rollback compensation Action ID chưa đăng ký | `rollbackInvalid` |
| CR08-18 | Timeout sau submit commit | Lookup idempotency key/digest; không duplicate |

## 12. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CR-I01 | Không thấy ChangeRequest entity/DTO/service/state trong tracked C#/JSON | Chưa có durable immutable request/revision contract | M11-T009–T011; M11-T049 |
| M11-CR-I02 | Configuration, balance, broadcast, delete và nhiều content action gọi effect trực tiếp | Thiếu request ID, before/after, impact, schedule, rollback và idempotency chung | M11-T009–T021, T030, T038–T044 |
| M11-CR-I03 | Configuration update ghi actor từ `User.Identity.Name` fallback `SuperAdmin` | Actor/audit identity không đủ tin cậy và có fallback chuỗi | M11-T031; M11-T049 |
| M11-CR-I04 | Maintenance route placeholder trả success dù chưa effect thật | Không có request/result/reconcile; success giả | M11-T011, T036, T041–T044 |
| M11-CR-I05 | Không thấy scheduling, immutable preview/evidence hoặc rollback-plan model | Không thể chứng minh đúng revision/impact/recovery | M11-T009–T017 |
| M11-CR-F01 | Durable schema/index/idempotency/concurrency và migration | Append-only revision, unique request/revision/key/digest | M11-T009–T011; M11-T049 |
| M11-CR-F04 | Module validation/preview/rollback implementations | Missing/stale/unknown evidence không ready | M11-T013–T017; module-owner tasks |
| M11-CR-F05 | Audit/redaction/durability/runtime regression evidence | Metadata allowlist; audit loss fail-closed | M11-T031–T035, T049; A-G02/A-G06 |

## 13. Tự kiểm M11-T008

- Đủ 31 mutation action map chín request type; 13 read action giữ đúng access-control path.
- Chốt immutable revision/digest, 18 nhóm trường theo EC class, typed operation, evidence freshness, schedule/expiry và ba rollback class.
- CR-V01–V12, 10 gate và 18 case bao phủ missing evidence/rollback, stale preview/version, risk downgrade, client authority, idempotency, timeout, secret và audit loss.
- Không thêm phê duyệt hai người hay đặc quyền khẩn; request không cấp authority và không thay module-source truth.
- A-G02/A-G06/REL-02 có contract design nhưng vẫn chờ lifecycle/execution/audit/runtime evidence; không kết luận gate đạt.

## 14. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt request type, immutable revision/schema, validation/evidence/schedule/rollback, gate và case | WSA-7K2 |
