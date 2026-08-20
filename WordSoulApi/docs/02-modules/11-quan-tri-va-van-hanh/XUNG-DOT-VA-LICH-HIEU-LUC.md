# Xung đột và lịch hiệu lực thay đổi quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CHANGE-SCHEDULE-1.0` |
| Task | M11-T010 |
| Đầu vào | M11-CHANGE-REQUEST-1.0, M11-CHANGE-DECISION-1.0, M12-STATE-REG-1.0 |
| Quyết định nền | D-001, D-008, D-023–D-025, D-038–D-040; M11-D007–D010, D021 |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Contract này bảo đảm hai thay đổi chồng phạm vi/thời gian không ghi đè âm thầm và lịch luôn quy về một instant UTC xác định.

- SQL là durable truth của schedule, reservation, lease/fencing và transition; Redis chỉ được làm hint/cache/wakeup, mất Redis không được bỏ kiểm tra SQL.
- Conflict/reservation/lease là coordination, không phải permission hay temporary privilege. Scheduler/workload vẫn dùng fixed identity và exact valid decision/revision.
- `effectiveAtUtc`, window và conflict key là canonical decision input. Đổi instant/window/write set tạo revision + decision mới; không “reschedule” ngầm decision cũ.
- Mọi claim execute kiểm lại decision validity, current authority, target/source/evidence/limit, conflict/fencing và audit availability.
- Unknown overlap, missing registry, stale selector hoặc không lấy được durable reservation đều fail-closed.

## 2. Conflict set

Mỗi revision khai ba tập server-normalized:

| Tập | Nghĩa | Ví dụ |
|---|---|---|
| `writeSet` | Resource/policy/subject/cohort sẽ thay đổi hoặc khóa | Config key set, role assignment, ledger subject+asset, campaign audience snapshot |
| `guardSet` | Source/version/hold/consent/limit mà thay đổi sẽ làm intent stale | Expected config version, target account security version, consent snapshot |
| `barrierSet` | Capability/environment cần độc quyền rộng trong maintenance/incident | `prod:m12:email-delivery`, `prod:global:write-barrier` |

Conflict nếu window overlap và có `write/write`, `write/guard`, `guard/write` hoặc barrier ancestor/descendant overlap. `guard/guard` không conflict nhưng mỗi claim vẫn revalidate version. Selector không chứng minh disjoint được coi overlap.

## 3. Conflict key chuẩn

| Thành phần | Quy tắc |
|---|---|
| `environment` | Registry enum, không client text; production/staging/local tách rõ |
| `ownerModule` | M01–M12 hoặc platform registry owner |
| `resourceType` | Stable type ID, không tên bảng/controller |
| `scopeType` + `scopeId` | Tenant/module/content-group/subject/cohort/capability/global; ID canonical/protected |
| `resourceIdOrSelectorDigest` | Exact resource ID hoặc digest immutable selector snapshot |
| `subKey` | Config key, asset type, secret purpose… khi registry cho phép |
| `keyVersion` | Version normalization/schema |

Canonical form do server tạo: `{env}/{owner}/{resourceType}/{scopeType}:{scopeRef}/{resourceOrSelector}/{subKey}`. Key có quan hệ cây; write ở ancestor conflict mọi descendant. `global` là ancestor cuối cùng, chỉ dùng cho action đã đăng ký.

## 4. Conflict mode registry

| Mode | Khi dùng | Concurrent behavior |
|---|---|---|
| `exclusive` | Quyền, delete, version activation, ledger, secret, maintenance, same source resource | Mọi overlap bị chặn |
| `serial` | Job/bulk/campaign có thể xếp tuần tự và module chứng minh idempotent/checkpoint | Chỉ một claim; request sau phải có window/validity còn đủ, không tự kéo dài |
| `commutative` | Chỉ operation registry chứng minh thứ tự không đổi invariant/result | Có thể đồng thời trên exact declared subkeys; mặc định không áp dụng |
| `barrier` | Global/capability maintenance hoặc incident containment | Chặn descendant write/execute; read chỉ khi registry đánh dấu safe |

Nếu mode thiếu/unknown hoặc hai mode khác nhau overlap, dùng mode chặt hơn: `barrier > exclusive > serial > commutative`. Actor/role cao hơn không override conflict.

## 5. Reservation và lease/fencing record

| Trường | Yêu cầu |
|---|---|
| `reservationId`, `scheduleVersion` | UUID + monotonic version, append-only history |
| `revisionId`, `decisionId`, digests | Exact immutable source |
| `conflictKeys`, `mode`, `windowStartUtc`, `windowEndUtc` | Server-normalized và bounded |
| `state` | `held`, `claimed`, `released`, `expired`; không xóa history |
| `stateVersion` | CAS cho cancel/claim/release race |
| `fencingToken` | Số tăng đơn điệu theo conflict key set/owner; executor cũ bị source adapter reject |
| `leaseOwner`, `leaseUntilUtc`, `heartbeatAtUtc` | Fixed workload identity; không user impersonation |
| `createdAt`, `releasedAt`, `releaseReason` | DB UTC time + reason code |

- Acquire toàn bộ key theo canonical sort trong một transaction; không giữ một phần rồi chờ phần còn lại.
- `held` reservation tồn tại từ lúc schedule commit. Đến due time, scheduler atomically revalidate + tăng fencing token + chuyển lifecycle sang `executing` + reservation sang `claimed` + ghi outbox/audit intent.
- Lease expiry không chứng minh effect chưa xảy ra. Worker mới phải reconcile operation/fencing trước takeover, không phát lại mù.
- Source adapter nhận operation ID + fencing token và reject token thấp hơn token durable mới nhất.
- Release chỉ sau cancel trước claim, terminal verified result hoặc recovery đã đạt; partial/unknown/rollingBack tiếp tục chặn overlap.

## 6. Time contract

| Trường | Quy tắc |
|---|---|
| Nguồn so sánh | UTC database/server-authoritative time tại transaction; không client/device clock |
| `effectiveAtUtc` | `DateTimeOffset`/instant UTC; bắt buộc cho absolute schedule |
| `windowStartUtc`, `windowEndUtc` | Start inclusive, end exclusive; end > start và ≤ `validUntil` |
| `displayTimezone` | Canonical IANA ID để trình bày; không dùng làm execution truth |
| `inputLocalTime`, `inputOffset` | Chỉ metadata giải thích conversion; redaction/retention allowlist |
| `timezoneRulesVersion` | Phiên bản tzdb/mapping dùng khi convert |
| Precision | Persist tối thiểu millisecond; compare theo DB precision đã công bố |

### Local time và DST

- API ưu tiên nhận UTC instant. Nếu nhận local wall time, bắt buộc IANA timezone và server conversion.
- Local time rơi vào DST gap (không tồn tại) bị reject; không tự đẩy sang giờ kế tiếp.
- Local time bị lặp khi DST overlap phải kèm explicit UTC offset khớp timezone rules; thiếu/mismatch bị reject.
- Sau conversion, UTC instant là canonical. Tzdb/mapping đổi không dịch lịch đã sealed; chỉ display lại và cảnh báo nếu biểu diễn địa phương khác.
- Không hỗ trợ recurring schedule trong v1.0. Mỗi lần hiệu lực là một revision/decision/reservation độc lập.
- M01-T025-A có thể chuẩn hóa timezone hồ sơ người dùng sau; timezone hồ sơ không được thay đổi schedule quản trị đã sealed.

## 7. Schedule policy

| Trường hợp | Policy |
|---|---|
| EC-2 | Có thể immediate hoặc absolute/window nếu request/decision còn valid |
| EC-3 | Absolute/window mặc định; immediate chỉ khi Action registry cho phép và mọi current guard đạt |
| EC-4 | Absolute/maintenance window; immediate chỉ cho registered contain/revoke/stop action, vẫn đủ EC-4/audit/recovery |
| Window | Bounded bởi `validUntil`, monitoring/notification lead time và module constraint; thiếu end bị reject |
| Due claim | `dbUtc >= windowStartUtc` và `< min(windowEndUtc, validUntil)`; exact decision/revision vẫn valid |
| Missed window | Không chạy muộn; chuyển `expired` với `scheduleMissed`, release held reservation, tạo revision mới nếu cần |
| Scheduler lag | Metric/cảnh báo; không đổi effective time hoặc báo executed nếu chưa claim |

Không có priority `emergency` hay actor override. Incident containment nhanh dùng registered immediate action/fixed role, không chen ngang operation đã claim nếu source không chứng minh safe; khi cần barrier phải acquire theo cùng contract.

## 8. Schedule, cancel và đổi lịch

| Command | Kết quả |
|---|---|
| Schedule authorized revision | Validate UTC/window/TTL/conflict sets; acquire held reservation + `scheduled` bằng một transaction |
| Duplicate schedule same key/fingerprint | Trả cùng reservation/schedule result |
| Duplicate key, khác time/window/key set | Conflict; không mutate schedule cũ |
| Cancel trước claim | CAS lifecycle + reservation `released`; audit reason; terminal `canceled` |
| Cancel đồng thời claim | Một CAS thắng. Claim thắng thì cancel thành stop request, không release/mark canceled giả |
| Đổi UTC instant/window/conflict set | Cancel cũ nếu chưa claim, tạo revision/digest + decision + reservation mới; không copy decision |
| Chỉ đổi display timezone/label | Được nếu UTC instant/window/key/digest/effect không đổi và field nằm noncanonical allowlist; audit metadata update |
| Claim đã bắt đầu | Không reschedule/cancel terminal; dùng stop/reconcile/rollback T011 |

## 9. Conflict resolution

1. Build conflict/guard/barrier sets từ source registry và selector snapshot; client key chỉ là hint.
2. Query durable held/claimed/partial/recovery reservations có key tree + window overlap.
3. Nếu disjoint được chứng minh và mode cho phép, tiếp tục; unknown coi conflict.
4. Với `serial`, có thể đề xuất window kế tiếp nhưng không tự thay request/decision. Actor phải tạo revision/decision cho instant mới.
5. Với `exclusive/barrier`, trả conflict metadata-safe: conflict class, owner module, earliest known release/window; không lộ target/actor sensitive.
6. Acquire atomically theo sorted keys, kiểm stateVersion/decision validity lần cuối và ghi audit/outbox.

Không dùng “last write wins”, route order, in-memory mutex, Redis-only lock, role priority hay timestamp client để giải quyết.

## 10. Failure semantics

| Failure | Hành vi |
|---|---|
| SQL/reservation store unavailable | Fail-closed schedule/claim/cancel; không effect |
| Redis/cache/wakeup unavailable | Đọc/poll durable SQL có bound; nếu không làm được thì delay + alert, không bypass |
| Timezone ID/rules unavailable | Reject local conversion; UTC input hợp lệ vẫn dùng được nếu policy cho phép |
| Clock/database time unavailable | Không claim new execution; health degraded/alert |
| Partial key acquisition | Rollback transaction; không reservation một phần |
| Lease heartbeat lost | Mark suspect, reconcile; không assume free/effect absent |
| Worker stale fencing token | Source adapter reject; audit `staleFencingToken` |
| Audit unavailable | Sensitive schedule/claim/cancel fail-closed |
| Outbox publish delayed sau transaction | State giữ durable pending dispatch; retry same event/operation, không rollback lịch giả |

## 11. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CS-G01 | Mọi mutation revision có server-derived write/guard/barrier set và conflict mode |
| CS-G02 | Key hierarchy + W/W, W/G, G/W + unknown-overlap semantics được test |
| CS-G03 | SQL durable reservation, atomic sorted-key acquire, stateVersion CAS và monotonic fencing |
| CS-G04 | UTC canonical; local DST gap/overlap/tzdb mapping có kết quả xác định |
| CS-G05 | Window bounded bởi validUntil; missed window không chạy muộn |
| CS-G06 | Đổi canonical schedule tạo revision/decision mới; display-only không đổi effect |
| CS-G07 | Cancel/claim/reschedule race không release operation đã claim hoặc duplicate effect |
| CS-G08 | Redis/clock/SQL/audit failure không bypass conflict/schedule guard |
| CS-G09 | Partial/unknown/recovery giữ reservation đến verified terminal state |
| CS-G10 | Immediate incident action không tạo priority/temporary privilege/emergency bypass |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CS10-01 | Hai config request cùng key/window | Một reservation; request kia conflict |
| CS10-02 | Write parent scope và write descendant | Conflict theo hierarchy |
| CS10-03 | Write target và guard cùng target | Conflict; không dùng stale preview |
| CS10-04 | Hai selector không chứng minh disjoint | Conservative conflict |
| CS10-05 | Registered commutative operations ở subkey disjoint | Có thể reserve đồng thời theo registry evidence |
| CS10-06 | Acquire ba key, key thứ ba conflict | Transaction rollback; không giữ hai key đầu |
| CS10-07 | Local time trong DST gap | Reject, không auto-shift |
| CS10-08 | Local time DST overlap thiếu offset | Reject; yêu cầu offset rõ |
| CS10-09 | Tzdb đổi sau seal | Giữ UTC instant; chỉ display/cảnh báo thay đổi |
| CS10-10 | Window end sau decision validUntil | Reject schedule |
| CS10-11 | Scheduler thức sau window end | `expired/scheduleMissed`; không execute muộn |
| CS10-12 | Cancel và claim cùng stateVersion | Một CAS thắng; không canceled giả sau claim |
| CS10-13 | Đổi effectiveAt sau authorize | Revision/digest + decision mới, reservation cũ release nếu cancel thắng |
| CS10-14 | Chỉ đổi display timezone, UTC không đổi | Cho metadata update allowlist + audit |
| CS10-15 | Redis lock mất giữa schedule | SQL reservation vẫn chặn overlap |
| CS10-16 | Lease hết hạn khi effect unknown | Reconcile/fencing; không takeover phát lại mù |
| CS10-17 | Worker cũ gửi fencing token thấp | Source reject; không effect |
| CS10-18 | SQL/audit unavailable khi claim EC-4 | Fail-closed; giữ scheduled/pending, alert |
| CS10-19 | Partial execution có manifest | Giữ claimed reservation/block overlap đến recovery verified |
| CS10-20 | High role yêu cầu override conflict | Deny; role không bypass coordination |

## 12. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CS-I01 | Configuration bulk chỉ chặn duplicate key trong cùng request | Không expected version/CAS hoặc conflict giữa request/instance | M11-T014; M11-T049 |
| M11-CS-I02 | Không thấy schedule/reservation/lease/fencing aggregate | Không có durable ordering/cancel/claim semantics | M11-T014–T017; M11-T049 |
| M11-CS-I03 | Không thấy timezone/DST conversion contract cho admin change | Local time có thể mơ hồ; scheduler chưa có source | M01-T025-A; M11-T049 |
| M11-CS-I04 | Maintenance/config/broadcast mutation gọi trực tiếp | Bỏ schedule/conflict/window guard | M11-T016, T041–T044; M11-T049 |
| M11-CS-I05 | Không thấy source adapter fencing token check | Worker stale có thể effect sau lease loss | Module adapters; M11-T049 |
| M11-CS-F01 | Durable reservation/index/key-tree/fencing schema và migration | SQL truth, atomic multi-key acquire, monotonic fencing | M11-T049 |
| M11-CS-F02 | Scheduler/outbox/worker claim và missed-window implementation | Exact decision/revision, UTC DB time, no late execution | M11-T049 |
| M11-CS-F03 | Module conflict-key/commutativity/barrier registry | Unknown overlap/mode fail-closed | M11-T012–T021, T038–T044 |
| M11-CS-F04 | Source adapter operation/fencing/reconcile support | Execution contract đã chốt; stale token reject, partial/unknown keeps block | Module-owner tasks; M11-T049 |
| M11-CS-F05 | Runtime race/failure/DST/audit evidence | CS-G01–G10, CS10-01–20 | M11-T049; A-G02/A-G06 |

## 13. Tự kiểm M11-T010

- Chốt write/guard/barrier set, key hierarchy, bốn conflict mode và W/W–W/G–G/W/unknown-overlap semantics.
- Durable reservation có bốn state, stateVersion, atomic sorted-key acquire, monotonic fencing và lease-reconcile behavior.
- UTC là execution truth; IANA display timezone, DST gap/overlap, tzdb change, missed window và clock/store failure có kết quả xác định.
- Canonical reschedule tạo revision/decision mới; cancel/claim race dùng CAS; partial/unknown giữ block đến recovery.
- 10 gate và 20 case bao phủ conflict, multi-key rollback, DST, expiry, Redis/SQL/audit failure, lease/fencing và privilege override.
- A-G02/A-G06 vẫn chờ execution/source-adapter/runtime evidence; không kết luận gate đạt.

## 14. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt conflict set/key/mode, durable reservation/fencing, UTC/DST, cancel/reschedule/race/failure | WSA-7K2 |
