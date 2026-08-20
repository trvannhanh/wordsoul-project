# Sổ đăng ký use case trạng thái chia sẻ M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T031 |
| Registry ID / phiên bản | M12-STATE-REG-1.0 |
| Trạng thái | Baseline kiểm kê tĩnh có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-008, D-010, D-018–D-023; M12-CAP-REG-1.0; M12-CRIT-1.0; M12-D021–D023 |
| Phạm vi | Redis/cache local, limiter buckets, singleton queue, process lock, SignalR connection/group và operation state còn thiếu |
| Giới hạn | Quan sát source/config mặc định; không chứng minh topology, Redis/runtime health, traffic coverage hoặc production values |

## 1. Quy tắc registry

- Shared state phải có owner, source of truth, namespace, lifetime/TTL, consistency, quota, criticality và failure mode. Thiếu một trường thì giữ `unverified`, không suy từ DI/config là sẵn sàng.
- Cache, connection ID, queue entry, rate bucket và lease không là durable business truth. Restart/eviction/partition không được tạo cấp phát, điểm, quyền hoặc success giả.
- `local-process` chỉ phối hợp trong một process; không được mô tả là distributed. Multi-instance/restart phải có durable recovery hoặc giới hạn topology đã được chứng minh.
- Namespace không chứa secret/PII thô. User/device/recipient dùng protected partition/ref; metric không dùng key cardinality cao làm label.
- TTL hiện trạng không tự là TTL được duyệt. TTL/invalidation cuối thuộc M12-T032; lock/ownership thuộc T033; limiter thuộc T034–T035.

## 2. Loại state và consistency

| Loại | Semantics | Consistency tối thiểu |
|---|---|---|
| Read cache | Bản sao có thể bỏ, rebuild từ durable owner | Version/TTL; miss/failure tách biệt; stale không ghi ngược truth |
| Coordination | Queue/connection/lease giúp nhiều worker phối hợp | Atomic ownership/transition; fencing hoặc durable compare-and-set khi mutation C0 |
| Rate state | Counter/window/token theo policy + partition | Atomic trong phạm vi topology; policy/version/clock rõ; failure mode theo criticality |
| Pending operation | Trạng thái idempotency/result/reconcile trước khi final | Durable, monotonic, unique operation+fingerprint; không mất qua restart |
| Delivery buffer | Công việc chờ ghi/gửi | Bounded + backpressure; durable nếu mất làm vi phạm audit/business intent; dedup khi replay |

## 3. Registry use case quan sát được và bắt buộc

| State ID | Use case / owner | Implementation & scope quan sát | Source of truth | Namespace/partition | TTL/lifetime hiện trạng → yêu cầu | Consistency / quota | Mức | Failure mode an toàn / task |
|---|---|---|---|---|---|---|---|---|
| ST-001 | AI vocabulary preview cache / M02+M12 | Redis `IDistributedCache`; `VocabularyAiCacheService`; C3 disabled | M02 approved content, không cache | `vocab-ai-preview:<normalized-word>` | 7 ngày → phải gồm source/model/prompt/contract version và invalidation | Eventual; chưa có size/key quota | C3 | Không traffic/read-write để mở AI; key hiện thiếu semantic version; T032, D-010 |
| ST-002 | User activity-log page cache / M11 | `IMemoryCache`, per-process | SQL activity log | `ActivityLogs_User_<user>_<page>_<size>` hiện chứa raw user ID | 5 phút; create chỉ remove page 1/size 10 → invalidation toàn lát chưa đủ | Eventual/stale-bound; memory quota chưa quan sát | C2 đọc; dữ liệu log C0/C1 | Bypass về SQL; cache failure không nuốt audit write; protected key + T032 |
| ST-003 | Admin/global activity-log page cache / M11 | `IMemoryCache`, per-process | SQL activity log | Filter/date/page/size; namespace version chưa có | 5 phút; không thấy invalidation khi create | Eventual; memory quota chưa quan sát | C2 đọc; dữ liệu log C0/C1 | Bypass SQL, authorization trước cache; stale rõ; M11-T034, M12-T032 |
| ST-004 | PvP pending round answers / M08 | `IMemoryCache`, per-process trong `ArenaBattleService` | SQL battle session/round là durable owner nhưng pending answer chưa thấy durable | `pvp-<session>-round-<round>-p1-or-p2` | 1 giờ → theo round deadline/grace, xóa khi resolve | Hai key rời; không atomic pair; không quota | C0 fairness/scoring | Không chấm/forfeit từ missing cache; giữ round interrupted và reconcile; cần durable/CAS hoặc topology proof; M12-T033, M08-T026–T030 |
| ST-005 | Matchmaking waiting queue / M08 | Singleton `ConcurrentDictionary`, single-process | Chưa có durable queue truth; SQL battle session chỉ có sau match create | Random queue ID; user/connection lookup bằng scan | Không TTL/expiry quan sát; mất khi restart | Thread-safe dictionary nhưng scan+remove không là distributed ownership; không queue cap | C1 | Không start trùng khi ownership không chắc; restart báo queue mất và rejoin an toàn; durable claim/fencing; M12-T033, M08-T018–T020 |
| ST-006 | SignalR battle group/connection / M08 | SignalR runtime group + client connection ID | SQL battle/session/round | `battle-<session>` + connection IDs | Theo connection; reconnect tạo connection mới | Runtime delivery, topology/backplane chưa chốt | C1 | Reload durable state, reauthorize group, sequence/version; delivery không commit; M12-T028–T030 |
| ST-007 | SignalR notification connection / M10 | SignalR runtime | Durable notification/inbox phải là truth | Subject/group/connection runtime | Theo connection | At-least-once/loss possible; backplane unknown | C2 | Reload inbox, dedup event; notification delivery không là state; M12-T028–T030 |
| ST-008 | Inventory per-user mutation lock / M06/domain owner | Static `Dictionary<int, SemaphoreSlim>`, per-process | SQL user inventory | Raw integer user ID | Không expiry/remove; tăng không giới hạn | Serializes only one process; no fencing/DB concurrency proof | C0 mutation/cấp phát | Không coi local lock là exactly-once; DB constraint/concurrency + operation idempotency; M12-T033 |
| ST-009 | System log delivery queue / M11 | Singleton bounded `Channel<SystemLog>` capacity 10,000, `DropOldest` | SQL `SystemLogs` sau khi worker commit; trước đó chưa durable | Một queue chung, chưa partition priority | Process lifetime; item TTL không thấy | Bounded nhưng drop-oldest; worker log lỗi rồi tiếp tục, không durable replay | C0 security/audit; C1 diagnostics | C0 audit không được drop/success giả; durable buffer/backpressure/DLQ/reconcile; M11-T034–T035, M11-T043 |
| ST-010 | Global anonymous IP limiter / M12 | ASP.NET local fixed window; policy registered, không thấy endpoint consumer/global binding | Policy registry; counter chỉ transient | `global-ip:<ip>` | Config window | Atomic per-process only; queue/permit from config | C1 abuse | Coverage unverified; trusted proxy/IP canonicalization; local conservative fallback only; T034–T035 |
| ST-011 | Authenticated-user general limiter / M12 | ASP.NET local sliding window; registered, không thấy endpoint consumer | Policy registry | `auth-user:<user>` hoặc anonymous IP | Config window/segments | Per-process only | C1/C0 theo route | Không dùng policy chung để hạ C0 route; coverage unverified; T034–T035 |
| ST-012 | AI generation limiter / M12 | ASP.NET local token bucket trên 2 AI endpoints; Redis limiter mô tả cùng use case nhưng không thấy được gọi | Policy registry | `ai-vocab:<user-or-ip>` | Config refill/window | Per-process effective coverage; Redis path unverified | C3 hiện hành | Capability tắt theo D-010; limiter không phải cơ chế activation; nếu mở phải distributed/conservative; T034–T035 |
| ST-013 | Speech/TTS limiter / M12 | ASP.NET local fixed window registered; không thấy endpoint consumer; Redis fixed-window class không thấy được gọi | Policy registry | `audio-gen:<user-or-ip>` | Config window | Per-process; distributed coverage unverified | C2 cost nếu TTS; C3 cho user pronunciation | Không provider traffic ngoài activation; cost route fail conservative; T034–T035 |
| ST-014 | Matchmaking join limiter / M08+M12 | ASP.NET local fixed window trên `POST /api/pvp/queue/join` | Policy registry | `mm-join:<user-or-ip>` | Config window | Per-process; queue capacity khác limiter quota | C1 | Redis lỗi không liên quan vì consumer local; multi-instance aggregate chưa kiểm soát; T034–T035 |
| ST-015 | Auth endpoint limiter / M01+M12 | ASP.NET local fixed window trên login/register | Policy registry; auth attempt/audit durable state cần owner | `auth:<ip>` | Config window | Per-process; chỉ IP partition quan sát | C0 | Không fail-open/mất coverage; cần account/device/risk composite + distributed/conservative mode; T034–T035, M01-T011 |
| ST-016 | Gym battle-start limiter / M06/M08+M12 | ASP.NET local token bucket registered; không thấy endpoint consumer | Policy registry + durable battle/reward truth | `gym-battle:<user>` | Config refill | Per-process; coverage unverified | C0 cấp phát/reward | Không bypass khi store lỗi; durable idempotency/concurrency vẫn bắt buộc; T034–T035 |
| ST-017 | Redis distributed limiter buckets / M12 | `RedisRateLimiter` singleton + Lua fixed/token bucket; không thấy direct consumer | Policy registry | Caller-provided key; token bucket suffix `:tokens`, `:ts` | Fixed window expire; token TTL = limit × refill × 2 | Atomic Redis script; mọi exception hiện trả allow | C0–C2 tùy lát | Không dùng global allow-all; consumer/coverage/key/version/clock cần chốt; T034–T035, T047-A |
| ST-018 | Operation/idempotency/result/reconcile record / source module+M12 | **Required, chưa thấy implementation chuẩn** | Source module durable intent + M12 operation record | Capability/use case + opaque operation ID + keyed fingerprint | Theo deadline/idempotency/retention từng lát | Unique/CAS, monotonic finality, bounded attempts | C0/C1 | Chưa có thì không mở mutation/dispatch mới qua contract chuẩn; T036–T037, T047-A |
| ST-019 | Distributed lease/fencing cho mutation/worker ownership / module+M12 | **Required cho use case multi-instance, chưa thấy primitive chuẩn** | Durable module record/version | Resource protected ref + lease epoch | Lease ngắn, renew có trần; TTL không chứng minh ownership | Compare-and-set + fencing tại durable writer | C0 | Mất lease dừng writer cũ; local semaphore/singleton không đủ; T033, M11 jobs |

## 4. Ma trận lựa chọn store

| Nhu cầu | Store được phép | Không được dùng làm truth |
|---|---|---|
| Read acceleration | Local/Redis cache có version/TTL/invalidation | Cache hit/stale entry |
| Cross-instance limiter | Redis/atomic distributed store + local conservative guard | Counter local đơn lẻ trong multi-instance |
| Battle/inventory/operation mutation | SQL durable row/version/unique constraint; Redis chỉ hỗ trợ coordination | IMemoryCache, connection ID, semaphore/lease không fencing |
| Matchmaking waiting intent | Durable/partitioned queue hoặc topology single-instance được chứng minh + recovery | Singleton dictionary như cam kết multi-instance |
| Audit/security delivery | Durable outbox/queue + retry/DLQ/reconcile | Drop-oldest memory channel |
| Realtime delivery | SignalR/backplane + sequence; durable state để reload | Group/connection/delivery acknowledgement |

## 5. Finding hiện trạng

| Finding ID | Quan sát | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M12-STATE-I01 | AI cache key chỉ theo normalized word, TTL 7 ngày; thiếu source/model/prompt/contract version | Collision/stale semantic nếu capability được mở; hiện bị D-010 tắt | M12-T032 |
| M12-STATE-I02 | Activity-log cache invalidation chỉ xóa một user page; global/admin cache không thấy invalidation | Dữ liệu log đọc cũ/không nhất quán; key chứa raw user ID | M12-T032; M11-T034 |
| M12-STATE-I03 | PvP pending answers ở `IMemoryCache` với hai key rời | Restart/multi-instance/race có thể mất câu trả lời hoặc sai fairness | M12-T033; M08-T026–T030 |
| M12-STATE-I04 | Matchmaking queue singleton không TTL/cap/durable ownership | Restart mất queue; multi-instance ghép không thống nhất | M12-T033; M08-T018–T020 |
| M12-STATE-I05 | Inventory lock static theo user không cleanup và chỉ bảo vệ một process | Memory growth và race/cấp phát sai khi scale-out | M12-T033 |
| M12-STATE-I06 | System log queue `DropOldest`, không durable; write lỗi không requeue | Mất audit/log mà request đã tiếp tục | M11-T034–T035, T043; A-G02/A-G06 |
| M12-STATE-I07 | 7 policy limiter đăng ký nhưng chỉ thấy 3 policy gắn endpoint; Redis limiter không thấy consumer | Comment/DI tạo cảm giác coverage phân tán không tồn tại | M12-T034–T035, T047-A |
| M12-STATE-I08 | Internal loopback header được `GetNoLimiter` ở mọi policy local | Trái nguyên tắc workload identity/quota riêng của M12-D023/CT-05 | M12-T034–T035 |
| M12-STATE-I09 | Redis limiter trả allow cho mọi exception | Identity/cost/mutation C0 có thể fail-open | M12-T035, T047-A |
| M12-STATE-I10 | Chưa thấy operation/result/reconcile store và primitive fencing chuẩn | Retry/worker/mutation không có idempotency/ownership xuyên restart | M12-T033, T036–T037 |

## 6. Yêu cầu handoff cho T032–T035

- **T032:** cấp namespace version, protected partition, TTL, stale rule, invalidation owner/event và size/cardinality quota cho ST-001–ST-003; không bật AI.
- **T033:** chốt durable owner, atomic claim/CAS, lease/fencing/restart recovery cho ST-004–ST-009, ST-018–ST-019.
- **T034:** mỗi route/workload map đúng policy, partition, algorithm, aggregate scope, quota và coverage test; không suy coverage từ policy registration.
- **T035:** chốt Redis/local/control failure mode theo criticality; C0 không allow-all, worker không bypass mà dùng workload identity/quota riêng.

## 7. Tự kiểm M12-T031, A-G04 và REL-03

- 19 use case bao phủ cache AI/log, PvP answer, matchmaking, realtime, inventory lock, system-log queue, 7 limiter policy, Redis limiter và state/lease còn thiếu.
- Mỗi row có source of truth, namespace, TTL/lifetime, consistency/quota, criticality và failure mode/task; local-process và distributed không bị đánh đồng.
- 10 finding hiện trạng có source-observed behavior và receiver; đặc biệt nhận diện coverage limiter không tương đương registration.
- A-G04 có baseline: cache miss tách outage, shared state không là truth, C0 không fail-open, restart/multi-instance không tạo success/cấp phát giả.
- REL-03 vẫn mở vì topology, runtime coverage, health, failover, idempotency/fencing và recovery evidence chưa có.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Kiểm kê 19 shared-state use case, 10 finding và handoff T032–T035 | WSA-7K2 |
