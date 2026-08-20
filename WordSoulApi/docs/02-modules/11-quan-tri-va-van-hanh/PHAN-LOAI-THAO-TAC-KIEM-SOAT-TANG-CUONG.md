# Phân loại thao tác cần kiểm soát tăng cường M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-ENHANCED-CONTROL-1.0` |
| Task | M11-T007 |
| Phạm vi | 44 Action ID trong `M11-ACTION-1.0` và permission tương ứng trong `M11-PERM-1.0` |
| Quyết định nền | D-001, D-008, D-032–D-037; M11-D002, D005, D006, D023 |
| Nguyên tắc | Một actor có quyền cố định tự thực hiện; không có duyệt hai người, quyền tạm thời, đặc quyền khẩn hay impersonation |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và ranh giới

Tài liệu này biến risk floor của catalog thành kiểm soát có thể kiểm thử: re-auth, reason/case/change, hạn mức, bằng chứng trước–sau và hành vi từ chối. Đây là contract đích, không phải bằng chứng route hiện tại đã thực thi đúng.

- Module nguồn vẫn sở hữu invariant và durable truth; M11 không sửa trực tiếp để vượt reject.
- Permission, scope, current account/session/policy và Action ID phải đạt trước mọi kiểm soát tăng cường.
- R3/R4 luôn dùng admin session current-state và re-auth purpose-bound không quá 5 phút.
- Không biến `approval`, `reviewedBy`, ticket hay incident thành quyền. Nếu có ghi nhận quyết định, đó là metadata truy vết của chính actor có quyền.
- Mọi ngưỡng runtime được phép chặt hơn safety ceiling dưới đây; thiếu, lỗi, âm, bằng 0 ngoài nghĩa được công bố hoặc không xác định được ngưỡng phải fail-closed.

## 2. Bốn lớp kiểm soát

| Lớp | Risk | Khi dùng | Re-auth | Ngữ cảnh bắt buộc | Bằng chứng | Khi thiếu kiểm soát |
|---|---|---|---|---|---|---|
| EC-1 | R1 | Đọc aggregate/internal đã công bố, không PII nhạy cảm | Không | Scope + freshness/version | Actor, permission/scope, query class, result count/status | Deny; không trả dữ liệu dự phòng rộng hơn |
| EC-2 | R2 | Đọc/cập nhật nội bộ có Personal đã che hoặc effect hẹp, hồi phục được | Không mặc định | Purpose/reason; case nếu chạm chủ thể | EC-1 + resource/version + field policy + before/after cho mutation | Deny trước đọc/effect; không bỏ mask/audit |
| EC-3 | R3 | Đọc Sensitive/Personal chi tiết hoặc mutation có blast radius vừa | Bắt buộc ≤5 phút | Reason; case/change theo action; expected version | Decision input digest, before/after allowlist, operation/result, access audit | Challenge nếu proof stale; deny nếu trường khác thiếu; không effect |
| EC-4 | R4 | Quyền, tài sản, xóa/xuất, publish, bulk, maintenance, secret, incident hoặc effect khó đảo | Bắt buộc ≤5 phút | Reason + case/change/incident, preview/impact và rollback/reconcile | EC-3 + immutable intent/version, preview digest, idempotency, per-part/final result, notification khi áp dụng | Fail-closed trước effect; timeout sau commit thành `unknown` và reconcile, không retry mù |

Risk chỉ được nâng, không được hạ tại entry point. Bulk, production, cross-tenant/cross-module, dữ liệu nhạy cảm hơn hoặc blast radius lớn hơn nâng tối thiểu một lớp và tối đa EC-4.

## 3. Registry hạn mức

| Limit ID | Safety ceiling bắt buộc | Quy tắc vượt/ngưỡng lỗi |
|---|---|---|
| LIM-01 | Aggregate/internal read: page ≤100, window ≤31 ngày, export không ngầm định | Muốn rộng hơn phải dùng action export/bulk phù hợp; query không bound bị từ chối |
| LIM-02 | Personal/sensitive support read: một subject hoặc một case, page ≤25, window ≤31 ngày | Đổi subject/case tạo operation mới; field ngoài allowlist không trả |
| LIM-03 | Single-resource mutation: đúng một resource/version/operation | Bulk selector không được tách thành vòng lặp gọi route đơn |
| LIM-04 | Bulk/job: selector snapshot bất biến, batch ≤100, tổng ≤1.000 item/operation, một active operation/scope | Vượt 1.000 phải chia change có preview riêng; scope overlap bị chặn |
| LIM-05 | Export/disposition: một subject/request/manifest, một artifact active, expiry tải ≤24 giờ | Không recipient/expiry/hold check thì deny; không ghép nhiều subject ngầm |
| LIM-06 | Ledger/asset: một subject + asset type + case; amount/value phải nằm trong ngưỡng M06 theo role/scope | Thiếu ngưỡng hoặc vượt vai trò hiện tại thì deny/escalate sang fixed higher role; không tạo approval hai người |
| LIM-07 | Broadcast: một immutable audience snapshot/campaign; recipient ceiling và quota theo channel phải có cấu hình hợp lệ | Missing/stale preview, quota hoặc consent snapshot thì deny; không gửi một phần âm thầm |
| LIM-08 | Maintenance/capability/secret: một registered action hoặc reference + environment; concurrency một operation/scope | Secret value không qua M11; scope overlap hoặc action chưa đăng ký bị deny |
| LIM-09 | Authorization: một target assignment/operation; cấm self-change; conflict/last-owner guard luôn bật | Không bulk role elevation; target/role/scope đổi làm invalid proof/preview |

Các quota tốc độ bổ sung dùng policy M12-RATE-1.0 đồng thời với safety ceiling; trusted/internal workload không được miễn trừ.

## 4. Ma trận 44 action

`Evidence` dùng `E1` = decision/access audit; `E2` = E1 + version và before/after; `E3` = E2 + re-auth/reason/case/change; `E4` = E3 + preview digest/idempotency/result/reconcile/notification phù hợp. Tất cả bằng chứng chỉ chứa metadata allowlist.

| Action ID | Lớp | Limit | Evidence | Điều kiện tăng cường và hành vi từ chối đặc thù |
|---|---|---|---|---|
| M11-ACT-001 | EC-2 | LIM-02 | E1 | Purpose + case + mask; thiếu thì không trả support summary |
| M11-ACT-002 | EC-3 | LIM-02 | E3 | Sensitive allowlist + access audit; proof stale phải challenge trước query |
| M11-ACT-003 | EC-4 | LIM-03 | E4 | Restriction preview/expiry + notification; audit/version lỗi thì không đổi state |
| M11-ACT-004 | EC-4 | LIM-03 | E4 | Exact family hoặc explicit all + revoke result; scope mơ hồ bị deny |
| M11-ACT-005 | EC-4 | LIM-09 | E4 | Non-self/conflict/last-owner/current authority; không có đường emergency fallback |
| M11-ACT-006 | EC-4 | LIM-05 | E4 | Owner proof, manifest, recipient/expiry; không trả export inline |
| M11-ACT-007 | EC-4 | LIM-05 | E4 | Reversible point, hold/reference check; hard-delete trực tiếp bị deny |
| M11-ACT-008 | EC-2 | LIM-01 | E1 | Assigned content scope + Personal mask; null ownership không mở rộng quyền |
| M11-ACT-009 | EC-2 | LIM-03 | E2 | Validation + expected version; conflict không ghi đè |
| M11-ACT-010 | EC-3 | LIM-03 | E3 | Impact/reference + effective time; thiếu rollback thì không publish |
| M11-ACT-011 | EC-4 | LIM-03 | E4 | Case/change + reference/retention preview; còn hold thì deny/deprecate |
| M11-ACT-012 | EC-3 | LIM-03 | E3 | Author và publish cùng giữ EC-3; authority không suy từ nullable actor |
| M11-ACT-013 | EC-4 | LIM-03 | E4 | Dual-source validation + M06 catalog boundary; mismatch thì không effect |
| M11-ACT-014 | EC-3 | LIM-02 | E3 | Exact session/case + mask; không cho query theo ID ngoài case |
| M11-ACT-015 | EC-4 | LIM-03 | E4 | Chỉ tạo remediation request; sửa trực tiếp history bị deny |
| M11-ACT-016 | EC-3 | LIM-02 | E3 | Exact subject/case/time bound; thiếu access audit thì không đọc |
| M11-ACT-017 | EC-4 | LIM-04 | E4 | Registered job + dry-run/checkpoint; không retry nếu effect unknown |
| M11-ACT-018 | EC-3 | LIM-02 | E3 | Consent/purpose, metadata only; raw audio luôn bị chặn |
| M11-ACT-019 | EC-4 | LIM-05 | E4 | Exact manifest + hold/owner proof; không chuyển raw asset qua audit |
| M11-ACT-020 | EC-3 | LIM-02 | E3 | Ledger range + amount mask; balance summary không thay ledger truth |
| M11-ACT-021 | EC-4 | LIM-06 | E4 | Preview + operation ID + no-negative; vượt role limit thì deny |
| M11-ACT-022 | EC-3 | LIM-03 | E3 | Publish/deprecate cần impact/reference; delete khi còn reference bị deny |
| M11-ACT-023 | EC-1 | LIM-01 | E1 | Version/freshness bắt buộc; stale không trình bày như current |
| M11-ACT-024 | EC-3 | LIM-03 | E3 | Definition validation + expected version; conflict không overwrite |
| M11-ACT-025 | EC-4 | LIM-03 | E4 | Reward/reference impact + effectiveAt; thiếu rollback thì không activate |
| M11-ACT-026 | EC-3 | LIM-02 | E3 | Exact battle/gym/case; hidden answer/secret luôn bị loại |
| M11-ACT-027 | EC-4 | LIM-03 | E4 | Current-state preview + operation ID; already-terminal trả idempotent result |
| M11-ACT-028 | EC-4 | LIM-03 | E4 | Immutable rule snapshot + impact/effectiveAt; active version không sửa tại chỗ |
| M11-ACT-029 | EC-2 | LIM-02 | E1 | Exact group/member/case + mask; enumeration bị deny/rate-limit |
| M11-ACT-030 | EC-3 | LIM-03 | E3 | Human permission v1.0 đang disabled; không fallback về Admin/SuperAdmin |
| M11-ACT-031 | EC-4 | LIM-03 | E4 | Owner/reference/retention manifest; không delete trực tiếp |
| M11-ACT-032 | EC-1 | LIM-01 | E1 | Aggregate approved cohort + small-group suppression; không drill-down ngầm |
| M11-ACT-033 | EC-3 | LIM-03 | E3 | Variable allowlist + preview/version; publish cần proof mới |
| M11-ACT-034 | EC-4 | LIM-07 | E4 | Audience/consent/quota snapshot + stop/reconcile; stale preview bị deny |
| M11-ACT-035 | EC-3 | LIM-02 | E3 | Exact recipient/case hoặc aggregate; raw device token/content bị chặn |
| M11-ACT-036 | EC-4 | LIM-04 | E4 | Registered key set + validation/effectiveAt/rollback; sửa tại chỗ bị deny |
| M11-ACT-037 | EC-4 | LIM-02 | E4 | Exact case/time/source + query budget; audit lượt xem lỗi thì không trả log |
| M11-ACT-038 | EC-4 | LIM-04 | E4 | Registered job + dry-run/checkpoint/idempotency; orphan job bị deny |
| M11-ACT-039 | EC-3 | LIM-03 | E3 | Assigned queue/subject + reason/SLA; note ngoài redaction policy bị chặn |
| M11-ACT-040 | EC-4 | LIM-08 | E4 | Incident/playbook/timeline + containment scope; không có emergency privilege |
| M11-ACT-041 | EC-4 | LIM-08 | E4 | Registered maintenance + preview/stop; broad cache/DB command bị deny |
| M11-ACT-042 | EC-4 | LIM-08 | E4 | Capability/config/limiter preview + canary; không bypass limiter/fail-open |
| M11-ACT-043 | EC-4 | LIM-08 | E4 | Protected secret reference only + rotation proof; secret value bị reject |
| M11-ACT-044 | EC-2 | LIM-01 | E1 | Freshness/source/confidence; HTTP 200 không được suy thành healthy/correct |

Phân bố: EC-1 = 2, EC-2 = 5, EC-3 = 14, EC-4 = 23; tổng 44, không có Action ID chưa phân loại.

## 5. Evaluation và commit boundary

1. Resolve Action ID, actor, fixed role assignment, permission, scope và resource từ server-side truth.
2. Đánh giá current account/session/policy/security/authorization version; unknown hoặc stale là deny.
3. Chọn lớp cao nhất từ risk floor và modifier; resolve limit registry/version.
4. Kiểm tra re-auth, reason/case/change/incident, expected version, preview digest và idempotency theo lớp.
5. Reserve immutable audit intent trước EC-4 effect; nếu audit bắt buộc không durable thì fail-closed.
6. Module nguồn kiểm invariant và commit effect/idempotency; M11 không biến reject/unknown thành success.
7. Ghi before/after/result đã che. Timeout sau điểm commit trả `unknown`, enqueue reconcile và chặn operation xung đột.
8. Notification/rollback/reconcile chạy theo result thật; lỗi hậu xử lý không xóa audit hoặc phát lại effect.

## 6. Regression gate

| Gate ID | Kiểm tra | Điều kiện đạt |
|---|---|---|
| EC-G01 | Catalog coverage | Đủ 44/44 Action ID có đúng một lớp và Limit ID hợp lệ |
| EC-G02 | Risk monotonicity | Entry point modifier chỉ nâng lớp; R3/R4 không bỏ re-auth |
| EC-G03 | Single-actor policy | Không approval/two-person/temp/emergency/impersonation/bypass path |
| EC-G04 | Limit safety | Missing/invalid/stale limit fail-closed; internal workload cũng chịu limit |
| EC-G05 | Evidence durability | EC-4 không effect nếu không reserve được audit bắt buộc |
| EC-G06 | Source ownership | Module reject/unknown không bị M11 override hoặc báo success |
| EC-G07 | Secret/data boundary | Audit/preview/result chỉ metadata allowlist, không secret/raw payload |
| EC-G08 | Concurrency | Expected version/snapshot/idempotency phát hiện conflict và duplicate |
| EC-G09 | Unknown outcome | Không retry mù; reconcile giữ cùng operation identity |
| EC-G10 | Runtime allow/deny | Test mỗi action với allowed, denied, stale proof, missing context và audit loss |

## 7. Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| EC07-01 | R3 request có permission nhưng proof 6 phút | Challenge/deny trước query/effect |
| EC07-02 | R4 request có proof mới nhưng thiếu reason/case/change | Deny; không reserve success audit, không effect |
| EC07-03 | Actor tự thay assignment | Deny bởi LIM-09, kể cả actor giữ R13 |
| EC07-04 | Asset adjustment vượt ngưỡng R05 hiện tại | Deny; không tạo approval hoặc temporary elevation |
| EC07-05 | Bulk 1.001 item hoặc selector đổi sau preview | Deny/re-preview; không tách ngầm thành route đơn |
| EC07-06 | Audit store lỗi trước maintenance | Fail-closed; không chạy lệnh |
| EC07-07 | Timeout sau module commit | `unknown` + reconcile cùng operation ID; không gọi lại như mới |
| EC07-08 | Support search đổi subject nhưng tái dùng case/request | Deny hoặc operation mới; audit đúng subject |
| EC07-09 | Secret rotate payload chứa secret value | Reject/redact trước persistence; không log payload |
| EC07-10 | Internal worker gọi capability change không limit | Deny; trusted identity không phải bypass |
| EC07-11 | Role đúng nhưng permission/scope stale | Deny current-state; không tin claim cũ |
| EC07-12 | Content publish bị module nguồn reject reference | Giữ reject; không mark published/success |
| EC07-13 | Health trả 200 với dữ liệu stale | Trả metadata stale/unknown; không kết luận healthy |
| EC07-14 | Broadcast consent snapshot stale | Deny và yêu cầu preview mới |
| EC07-15 | Hai operation overlap cùng scope | Một operation thắng CAS/lease; operation kia conflict, không ghi đè |
| EC07-16 | Route mới không Action ID/EC class | Coverage gate fail; deny release |

## 8. Đối chiếu hiện trạng và finding

| Finding ID | Khoảng trống quan sát/bằng chứng còn thiếu | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-EC-I01 | Route hiện tại chủ yếu dựa Admin/SuperAdmin, chưa central EC evaluation | Contract này không hợp thức hóa broad role | M11-T049; M01-T028–T032 |
| M11-EC-I02 | Mutation trực tiếp chưa đồng nhất re-auth/reason/case/version/preview | EC-3/EC-4 fail-closed | M11-T008–T021, T030, T038–T044 |
| M11-EC-I03 | Activity/System log chưa chứng minh immutable audit reservation | EC-4 không effect khi audit bắt buộc lỗi | M11-T031–T035 |
| M11-EC-I04 | Chưa có runtime limit registry/coverage tests cho 44 action | Safety ceiling và missing-limit deny giữ nguyên | M11-T012–T017, T049; M12-T034 |
| M11-EC-F01 | Decision/execution API và runtime enhanced-control context | Immutable change-request schema đã chốt; protected refs, server-derived actor/resource | M11-T009–T011; M11-T049 |
| M11-EC-F02 | Runtime allow/deny/audit-loss/concurrency evidence | EC-G01–G10 và EC07-01–16 | M11-T031–T035, T049; A-G02 |
| M11-EC-F03 | Module-specific value/audience/job ceilings | Missing/unverified limit deny; stricter module limit wins | M06; M10; M11-T012–T017, T038–T042 |
| M11-EC-F04 | REL-02 audit/redaction/durability/retention closure | Không kết luận REL-02/A-G02 đạt từ design artifact | M11-T031–T035; REL-02 |

## 9. Tự kiểm M11-T007

- 44/44 Action ID được phân lớp: 2 EC-1, 5 EC-2, 14 EC-3 và 23 EC-4; không có action mồ côi.
- Chín Limit ID bao phủ read, subject-sensitive, single mutation, bulk/job, export/disposition, asset, broadcast, maintenance/secret và authorization.
- Re-auth, reason/context, evidence, fail-closed, commit boundary, unknown/reconcile và modifier nâng rủi ro đã rõ.
- 10 regression gate và 16 case bao phủ G02-C01–C10 ở mức thiết kế, bao gồm self-grant, thiếu re-auth/lý do, audit loss và concurrency.
- Không thêm duyệt hai người hay quyền tạm thời/khẩn cấp. A-G02/REL-02 vẫn chờ runtime permission/audit/redaction/durability evidence.

## 10. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Phân loại đủ 44 action, 9 hạn mức, evidence/refusal semantics, 10 gate và 16 case | WSA-7K2 |
