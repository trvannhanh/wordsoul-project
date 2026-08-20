# Ma trận fail mode và suy giảm M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T035 |
| Matrix ID / phiên bản | M12-FAIL-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-008, D-010, D-018–D-025; CT-05; M12-CRIT-1.0; M12-RATE-1.0; M12-RESULT-1.0 |
| Phạm vi | 15 capability, 23 rate policy, config/control/shared-state failure và recovery gate |
| Giới hạn | Chưa phải circuit/timeout/retry implementation, SLO/health threshold hoặc runtime recovery evidence |

## 1. Ngôn ngữ fail mode

| Mã | Tên | Được làm | Không được làm |
|---|---|---|---|
| FC | Fail-closed | Từ chối/dừng lát cần control/truth không chắc chắn; giữ intent/audit khi có thể | Trả success giả, tạo cấp phát/mutation hoặc lộ chi tiết nhạy cảm |
| CM | Conservative mode | Dùng local/edge cap thấp hơn, read-only scope hoặc deny một phần đã đăng ký | Allow-all, nhân quota theo instance, nới criticality |
| BT | Bypass-to-truth | Bỏ read cache và đọc durable owner khi capacity/deadline cho phép | Dùng cache stale làm auth/durable truth hoặc gây stampede không giới hạn |
| HOLD | Pending/hold | Giữ durable intent `unknown`/pending, queue bounded, retry/reconcile cùng operation | Ack completed/delivered, cấp bù mù hoặc giữ vô hạn |
| DG | Degrade feature | Bỏ chức năng phụ, dùng text/placeholder/manual/durable reload đã duyệt | Bịa data, tự đổi provider/purpose hoặc đổi business result |
| OFF | Disabled | Deny trước thu dữ liệu/dispatch theo policy/kill switch | Dùng limiter/cache/error làm cách mở capability |
| FO | Security fail-open | **Không có baseline được phép trong M12-FAIL-1.0** | Bỏ authorization, limiter, audit, policy, ownership, consent, quota hoặc durable constraint |

**Tiếp tục hành trình lõi không đồng nghĩa fail-open.** Ví dụ TTS lỗi nhưng người học tiếp tục bằng text là DG; Redis cache lỗi và đọc SQL là BT; push lỗi nhưng notification đã commit trong inbox là HOLD/DG. Không trường hợp nào bỏ qua control của operation đang được bảo vệ.

## 2. Quy tắc chọn mode

1. Nếu dependency quyết định authorization, consent, secret, audit bắt buộc, critical config, limiter C0, ownership, durable mutation/cấp phát hoặc idempotency → FC/HOLD; không FO.
2. Nếu effect remote có thể đã xảy ra → HOLD + `unknown` + reconcile; không retry mù và không gọi provider khác.
3. Nếu dependency chỉ là read cache → BT về durable truth, nhưng phải có bulkhead/stampede guard; quá capacity thì reject/degrade thay vì làm sập truth store.
4. Nếu capability C2 phụ trợ → DG; source module tiếp tục bằng state/content đã duyệt và biểu đạt trạng thái thiếu rõ.
5. Nếu C3 → OFF kể cả dependency healthy/config tồn tại. Muốn mở cần quyết định mới, data map, rate/cost budget, health và release evidence.
6. Nếu limiter/shared state lỗi → mode của policy có criticality cao nhất đang chạy. Không dùng mode thấp của read route cho auth/mutation.
7. Config missing/invalid/unknown activation không tự dùng default cho C0/C1. Chỉ validated last-known snapshot được dùng nếu policy của field cho phép và chưa quá hạn; activation/kill/security config mặc định FC/OFF.

## 3. Ma trận capability

| Capability | Lát / trigger | Mode | Hành vi caller/user | Result chuẩn | Điều kiện phục hồi |
|---|---|---|---|---|---|
| CAP-001 SQL | Core mutation/audit/durable truth unavailable hoặc commit unknown | FC/HOLD | Dừng mutation; giữ pending khi commit có thể đã xảy ra; read cached public chỉ nếu contract riêng cho phép | `temporaryFailure` nếu chắc no-effect, ngược lại `unknown` | DB health thật + reconcile/consistency check; không chỉ TCP/process up |
| CAP-002 Cache | Redis/local cache miss, timeout, stale hoặc corrupt | BT/DG | Đọc durable owner có stampede guard; nếu quá tải thì trả degraded/retry, không dùng stale cho quyền | Cache miss tách khỏi `temporaryFailure`; business result theo durable read | Probe get/set/version + capacity ổn định; warm có giới hạn |
| CAP-003 Rate limiting | Distributed state/clock/config/partition không chắc | FC cho C0; CM cho C1/C2 | C0 deny/local cap bảo thủ; read C1/C2 dùng local+edge cap thấp; không allow-all | `rejected`/`temporaryFailure` + `CONTROL.SAFETY_UNAVAILABLE` | Distributed atomic/clock/config/coverage health + canary; không chỉ Redis connect |
| CAP-004 External identity | Provider/config/callback unavailable hoặc response invalid | FC/DG | Không login/link mới; direct login và session nội bộ còn hợp lệ tiếp tục độc lập | Rejected/temporary/unknown theo điểm dispatch; không auto-link | OIDC contract/callback/state test + health/canary; session không bị thu hồi chỉ vì provider outage |
| CAP-005 AI metadata | Mọi trạng thái trong A/B | OFF | Dùng content đã duyệt/manual; không provider traffic | `rejected` / `POLICY.CAPABILITY_DISABLED` | Chỉ quyết định mới sau gate/REL, không auto-recover |
| CAP-006 AI cache | Mọi trạng thái trong A/B | OFF | Không read/write để kích hoạt/fallback AI | `rejected` hoặc không gọi | Cùng CAP-005 + retention/invalidation xử lý |
| CAP-007 Image discovery | Provider/quota/invalid response | DG | Upload/chọn asset đã duyệt/placeholder; không auto-publish | `noData` chỉ khi xác định empty; lỗi tạm/expired rõ | Contract/terms/health/quota probe + canary |
| CAP-008 Media | Private upload timeout/ownership/config unknown; public delivery lỗi | FC/HOLD cho upload, DG cho public read | Không tạo URL/ref/ownership giả; unknown upload reconcile orphan; public asset dùng placeholder | Unknown/temporary/permanent theo lifecycle | Storage write/read/delete/auth probe + orphan reconcile; signed URL không là truth |
| CAP-009 TTS/audio blob | Speech/blob/quota lỗi hoặc chậm | DG/HOLD nếu output có thể đã tạo | Học bằng text; output unknown reconcile asset; không phát audio null | Temporary/unknown/expired; không succeeded với null | Speech + blob contract probe, cost/asset reconcile và canary |
| CAP-010 Pronunciation | Mọi trạng thái trong A/B | OFF | Không thu/gửi audio; luyện không chấm nếu journey cho phép | `rejected` / disabled | Chỉ quyết định mới + consent/retention/terms/gates |
| CAP-011 Email | Security dispatch unavailable/timeout; reminder lỗi | HOLD cho security, DG/expire cho reminder | Giữ intent; accepted không delivered; reminder có thể hoãn/bỏ khi hết hạn | Temporary/unknown/expired; delivery status riêng | Provider contract/webhook + message reconcile + bounce/complaint path |
| CAP-012 Push | Provider/device/config lỗi | DG/HOLD | Durable inbox/state vẫn đúng; revoke invalid token; không đưa secret/PII vào fallback | Temporary/permanent destination/expired | Send + invalid-token callback/receipt contract + canary |
| CAP-013 Realtime | Connection/backplane/instance lỗi | DG/HOLD | Reload durable state; PvP chuyển interrupted/grace, không xử thua tức thời chỉ từ disconnect | Delivery temporary; domain state từ durable owner | Multi-instance reconnect/sequence/group auth test + state reconcile |
| CAP-014 Matchmaking | Queue/ownership/worker/realtime lỗi | FC/HOLD | Không start match khi claim không chắc; entry cũ lookup/rejoin an toàn; notify loss không tạo match mới | Unknown/pending/rejected; durable session quyết final | Queue recovery + atomic claim/fencing + duplicate-match test |
| CAP-015 Configuration | Missing/invalid/stale critical field hoặc config store lỗi | FC/OFF; validated snapshot chỉ theo field policy | Activation/secret/limit/security unknown → deny/off; noncritical display config có thể last-known | Rejected/safety unavailable; không silent default | Typed/version/effective/audit validation + owner-approved snapshot freshness |

## 4. Ma trận rate-policy failure

| Policy | Khi distributed/config/partition không chắc | Local/edge hành vi | Có được tiếp tục operation? |
|---|---|---|---|
| RL-001 anonymous read | CM | Edge cap + local 30/60 giây/IP; topology cap | Có trong cap; quá cap 429 |
| RL-002 authenticated general | CM/FC theo route | Local 90/60 giây/subject; C0 route dùng policy chuyên biệt | Chỉ C1/C2 trong cap |
| RL-003 registration | FC/CM | Local/account+IP cap bảo thủ; thiếu trusted partition thì deny | Không ngoài cap; response trung tính |
| RL-004 login | FC/CM | Composite local cap; account/partition/control unknown thì deny | Chỉ attempt trong cap; không bypass credential gate |
| RL-005 external identity | FC | Dừng start/link/callback apply; state durable | Không login/link mới |
| RL-006 verification attempt | FC | Counter durable không chắc thì lock/deny | Không consume code ngoài counter |
| RL-007 resend verification | FC/HOLD | Giữ intent, không gửi nếu quota state không chắc | Không dispatch mới |
| RL-008 recovery | FC/HOLD | Phản hồi trung tính, không gửi security mail mới | Không dispatch mới |
| RL-009 AI | OFF | Deny-all | Không |
| RL-010 image discovery | DG/CM | Không gọi provider hoặc local actor cap thấp | Chỉ manual/placeholder; provider call trong cap |
| RL-011 upload | FC/HOLD | Reject trước body; in-flight unknown reconcile | Không upload mới; public reads độc lập |
| RL-012 TTS | DG/CM | Text fallback; local cap thấp cho call đã duyệt | Journey có, provider call chỉ trong cap |
| RL-013 pronunciation | OFF | Deny-all trước audio collection | Không |
| RL-014 matchmaking join | FC/CM | Local cap + durable one-entry constraint; ownership unknown deny | Chỉ lookup/leave/rejoin an toàn |
| RL-015 battle start | FC/HOLD | Durable active/start operation bắt buộc | Không start/cấp phát mới |
| RL-016 battle answer | FC/HOLD | Durable one-answer/round/CAS; limiter unknown không chấm sai | Chỉ idempotent lookup/pending |
| RL-017 security email | FC/HOLD | Durable message intent; quota unknown không dispatch | Không dispatch mới; intent còn pending |
| RL-018 reminder/push | DG/HOLD | Bounded queue/local workload cap; expired thì drop có trạng thái | Có thể hoãn; không vượt expiry |
| RL-019 callback | FC hoặc bounded isolate | Signature/replay trước; edge/local CC cap; buffer durable nếu contract có | C0 callback không apply khi control unknown |
| RL-020 job/batch | HOLD/FC | Pause/backoff; workload local cap, checkpoint durable | Không side effect mới ngoài idempotent resume |
| RL-021 sensitive admin | FC | Deny nếu limiter/audit/re-auth/config không chắc | Không mutation |
| RL-022 admin export/report | HOLD/CM | Queue bounded/CC 1; privacy filter giữ nguyên | Có thể chờ/429; không nới scope |
| RL-023 health probe | CM | Edge FW + node CC; liveness rẻ, readiness degraded | Probe trong cap; không gọi dependency tốn phí |

Không policy nào chuyển sang `GetNoLimiter` hoặc `return true` vì caller tự nhận internal, Redis exception, config missing hay lỗi không phân loại.

## 5. Failure trigger và state machine

| Trạng thái | Vào khi | Traffic | Ra khi |
|---|---|---|---|
| Normal | Health/contract/config/coverage trong baseline | Policy đầy đủ | Trigger threshold của T038/T045 hoặc kill switch |
| Degraded | Dependency chậm/lỗi/quota nhưng mode BT/CM/DG/HOLD còn an toàn | Chỉ lát/mức đã đăng ký; emit degraded reason | Health ổn định qua ngưỡng phục hồi + backlog/reconcile trong cap |
| Isolated/OFF | C0 control/truth không chắc, security incident, config/contract drift hoặc C3 | Dừng route/provider/lát; bảo toàn evidence | Owner-approved recovery/canary; C3 cần quyết định mới |
| Recovering | Dependency có tín hiệu hồi nhưng chưa đủ tin cậy | Probe/canary allowlist, capacity nhỏ; C0 mutation vẫn hạn chế theo playbook | Contract/data reconcile đạt và stop criteria không kích hoạt |

Trigger phải dựa SLI thật: contract success/latency/unknown rate, limiter decision/coverage, queue age/depth/drop, reconcile backlog, config freshness/version, provider quota và data correctness. Process sống, config tồn tại hoặc TCP connect không đủ.

## 6. Recovery, backlog và late result

1. Recovery không tự xóa circuit/deny chỉ vì một probe thành công. T038/T045 chốt ngưỡng, cửa sổ và hysteresis theo capability.
2. Trước mở C0 mutation: kiểm tra source-of-truth consistency, unresolved unknown, duplicate/late result, quota state và audit path.
3. HOLD backlog có owner, max age/depth, expiry, retry budget và DLQ/reconcile. Quá hạn chuyển expired/permanent theo contract, không chạy bù mù.
4. Late result không ghi đè final. Nếu remote effect tồn tại ngoài business state, tạo reconcile/compensation operation có audit.
5. Canary dùng synthetic/allowlist operation không chứa dữ liệu thật nếu có thể; có stop criteria cho schema drift, unknown, privacy, cost và correctness.
6. Provider switch là route change có contract/data-purpose/terms/cost/canary riêng; không phải fallback tự động khi lỗi.

## 7. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M12-FAIL-I01 | `RedisRateLimiter` trả `true` cho RedisException và exception khác | Global FO trái CAP-003/RL C0/CT-05; là release blocker đến khi source được sửa và kiểm chứng | M12-T047-A; REL-03 |
| M12-FAIL-I02 | ASP.NET policy cho internal loopback header dùng `GetNoLimiter` | Workload không quota, có đường bypass; là release blocker đến khi source được sửa và kiểm chứng | M12-T047-A; REL-03 |
| M12-FAIL-I03 | `VocabularyAiCacheService` đồng nhất miss/Redis failure thành null rồi mô tả fallback Gemini | Không tách cache outage; nếu route hoạt động có thể mở traffic AI trái D-010 | M12-T032, T040–T041, T047-A |
| M12-FAIL-I04 | Adapter/service hiện có đường trả null, nuốt exception hoặc chỉ log provider status | Caller có thể hiểu failure là success/noData | M12-T006–T030, T047-A–T048 |
| M12-FAIL-I05 | PvP answer và matchmaking ownership ở memory/singleton process | Restart/multi-instance không có HOLD/reconcile/fencing đáng tin | M12-T033; M08-T018–T020, T026–T030 |
| M12-FAIL-I06 | System-log queue drop-oldest và write lỗi không requeue | C0 audit có thể tiếp tục mà mất evidence | M11-T034–T035, T043 |
| M12-FAIL-I07 | Firebase init có thể warning rồi service vẫn đăng ký; activation/health không được truyền thành result chuẩn | Caller/runtime có thể không biết capability unavailable | M12-T040–T041, T045, T047-A |
| M12-FAIL-I08 | Chưa có health/SLO/circuit/recovery state machine runtime | Không chứng minh vào/ra degraded hoặc recovery an toàn | M12-T038, T045–T047; M11-T036 |

## 8. Ma trận tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| FM35-01 | Redis limiter lỗi trên login/admin/battle start | FC/CM bảo thủ; không allow-all |
| FM35-02 | Redis cache lỗi khi đọc nội dung | BT có stampede guard; outage không giả cache miss/noData |
| FM35-03 | Timeout upload/email sau điểm có thể commit | HOLD/unknown + reconcile cùng operation; không retry mù |
| FM35-04 | External identity provider lỗi | Không login/link mới; direct login/session hợp lệ độc lập |
| FM35-05 | AI/pronunciation config/endpoint tồn tại | Vẫn OFF theo D-010; không traffic/thu dữ liệu |
| FM35-06 | TTS/image/push lỗi | Text/manual/placeholder/inbox; không success/data giả |
| FM35-07 | Matchmaking/PvP process restart | Không ghép/chấm/forfeit từ missing memory; durable recovery/interrupted state |
| FM35-08 | Critical config missing/stale | FC/OFF; không silent default/last-known ngoài field policy |
| FM35-09 | Provider hồi một probe | Chỉ Recovering/canary; chưa mở toàn traffic |
| FM35-10 | Late result mâu thuẫn final | Final giữ nguyên; reconcile/incident/compensation riêng |
| FM35-11 | Internal header hoặc provider switch để né quota | Reject; workload quota/route policy giữ nguyên operation budget |
| FM35-12 | Queue/backlog đầy hoặc quá hạn | Backpressure/reject/expired; không drop C0 hoặc giữ vô hạn |

## 9. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M12-FAIL-F01 | Namespace/TTL/invalidation và lease/fencing chưa chốt | BT/HOLD không được triển khai bằng state vô chủ/local-only cho C0 | M12-T032–T033 |
| M12-FAIL-F02 | Timeout/retry/circuit/bulkhead chưa chốt | Unknown không retry; optional degrade, C0 isolate | M12-T036–T038 |
| M12-FAIL-F03 | Health/SLO/alert/playbook/recovery threshold chưa có | Không auto-recover production | M11-T036–T048; M12-T045–T047 |
| M12-FAIL-F04 | Contract/provider mapping và runtime tests chưa có | Matrix không phải enforcement evidence | M12-T047-A–T048; REL-03 |

## 10. Tự kiểm M12-T035, A-G04 và REL-03

- Bảy mode FC/CM/BT/HOLD/DG/OFF/FO được định nghĩa; FO không có baseline được phép, còn tiếp tục core chỉ dùng truth/degradation đã duyệt.
- Đủ 15 capability và 23 rate policy có trigger/mode/hành vi/result/recovery; C0/C3 không bị hạ bởi availability.
- Unknown/late result/backlog/provider switch/config stale/recovery canary có quy tắc không tạo success, duplicate hoặc bypass.
- A-G04/CT-05 có 12 case cho limiter/cache/provider/shared-state/config/recovery; ma trận xác định rõ current `return true`/`GetNoLimiter` là sai lệch.
- 8 finding hiện trạng và 4 finding mở có task tiếp nhận; source chưa bị sửa vì workflow hiện tại chỉ cho tài liệu.
- REL-03 vẫn mở đến khi source enforcement, health/SLO, circuit, recovery, multi-instance và contract/canary evidence đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt fail mode cho 15 capability/23 rate policy, recovery state và 12 case | WSA-7K2 |
