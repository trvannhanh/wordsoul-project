# Ma trận giới hạn lưu lượng M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T034 |
| Policy catalog ID / phiên bản | M12-RATE-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012–D-014, D-018–D-024; CT-05; M12-CRIT-1.0; M12-STATE-REG-1.0; M12-D022–D023, D027 |
| Phạm vi | Public/user/admin API, callback, internal workload và capability có abuse/cost/quota |
| Giới hạn | Ngưỡng là baseline sản phẩm; budget tiền/tháng và provider quota thật chốt tại M12-T046/M11; runtime coverage chưa được chứng minh |

## 1. Bất biến

1. Không có trusted/internal bypass. Service/job dùng workload identity đã xác minh và quota riêng; header/claim/loopback tự thân không cấp miễn trừ.
2. Một operation có thể chịu nhiều bucket. Request chỉ được phép khi **tất cả** bucket áp dụng còn quota; không chọn bucket dễ nhất.
3. Partition dùng identity server xác lập: subject/workload protected ref, keyed digest của account identifier, device binding đã xác minh hoặc client IP sau trusted-proxy processing.
4. C0/C1 cần aggregate scope xuyên instance. Local bucket chỉ là conservative fallback/cap bổ sung, không chứng minh distributed coverage.
5. Limiter không thay authorization, idempotency, business uniqueness, concurrency control, provider quota hoặc cost budget.
6. Queue mặc định bằng 0 cho auth/mutation/cost action để tránh request hết hạn nằm chờ. Read-only C1/C2 chỉ có queue khi deadline/capacity đã chốt.
7. `Retry-After` lấy từ policy state đã clamp vào deadline, là số giây dương; client/worker không retry vô hạn và không được chuyển sang entry point miễn giới hạn.
8. Policy/config phải có version, effective time, owner, audit và validation. Missing/zero/âm không được diễn giải thành unlimited cho C0/C1.

## 2. Định danh partition và công bằng

| Dimension | Nguồn đáng tin | Dùng cho | Bảo vệ/công bằng |
|---|---|---|---|
| Client IP | Socket peer; chỉ nhận forwarding chain từ proxy CIDR được cấu hình, chọn hop theo thuật toán cố định | Coarse anonymous/NAT/attack guard | Không tin `X-Forwarded-For` trực tiếp; IPv6 normalization/prefix policy; không log IP đầy đủ quá retention |
| Account candidate | HMAC của email canonical/username sau server normalization, kể cả account không tồn tại | Login/reset chống phân tán IP | Cùng response cho tồn tại/không tồn tại; key-version/rotation; không metric label hoặc log raw identifier |
| Authenticated subject | Subject từ session/token đã validate và account state | General/user/cost/mutation | Không nhận user ID từ body/query làm partition truth |
| Device | Protected device ref gắn subject/attestation/session | Anomaly/login/push abuse bổ sung | Không tin device ID tự khai; không dùng fingerprint xâm lấn làm căn cứ duy nhất |
| Resource/intent | Durable protected ref như verification intent, battle round, asset owner | Enforce one-per-intent/domain cap | Limiter bổ sung unique/idempotency; không thay domain constraint |
| Workload | mTLS/workload identity/server credential đã xác minh | Job/batch/callback adapter | Quota theo workload+capability+environment; không dùng header `internal` |
| Provider callback | Verified signature/key + provider/capability route | Callback/replay/capacity | Signature trước business apply; limiter không thay replay window/idempotency |
| Admin actor + target | Admin session, re-auth, permission + protected target ref | Sensitive/admin mutation | Target bucket ngăn chia nhỏ qua nhiều admin; mọi reject/change có audit |

Trong NAT/shared network, IP là coarse guard chứ không là hình phạt account duy nhất. Auth dùng account/device/intent bucket để một IP đông người không khóa toàn bộ hành trình quá sớm; ngưỡng IP được theo dõi reject-rate và có thay đổi versioned, không bypass thủ công.

## 3. Ma trận policy baseline

Ký hiệu: FW = fixed window, SW = sliding window, TB = token bucket, CC = concurrency cap, DU = durable uniqueness/idempotency. Các quota cùng dòng được áp dụng đồng thời.

| Policy ID | Entry point / trạng thái | Criticality | Partition bắt buộc | Algorithm và quota baseline | Aggregate / local guard | Khi vượt | Owner / handoff |
|---|---|---|---|---|---|---|---|
| RL-001 | Anonymous/public read không có policy riêng | C1/C2 | Client IP + route class | FW 100 request/60 giây/IP | Distributed; local cap 30/60 giây/IP khi state lỗi | 429 + Retry-After; cached public response nếu hợp lệ | M12; T035 |
| RL-002 | Authenticated general read/write không có policy riêng | C1; C0 route thêm policy | Subject + route class; IP coarse 600/60 giây | SW 300/60 giây/subject, 6 segment | Distributed; local cap 90/60 giây/subject | 429; không bỏ policy chuyên biệt | M12; T035 |
| RL-003 | Direct registration | C0 | IP + account-candidate | FW 5/60 phút/IP và 3/24 giờ/account-candidate | Distributed + local conservative | Phản hồi trung tính, Retry-After không lộ account | M01/M12; M01-T003, M01-T011 |
| RL-004 | Direct login | C0 | IP + account-candidate + verified device nếu có | FW 10/15 phút/IP; 5/15 phút/account; 10/15 phút/device; DU attempt/audit | Distributed + local conservative; anomaly layer có thể siết, không nới | Failure chung/429 theo boundary; không xác nhận account tồn tại | M01/M12; M01-T010–T011 |
| RL-005 | External identity start/callback/link | C0 | IP/subject + provider class + one-time state/intent | FW 10/15 phút/subject-or-IP/provider; DU state dùng đúng một lần | Distributed + durable state | Reject/restart intent an toàn; không đổi sang auto-link | M01/M12; M12-T006–T010 |
| RL-006 | Verify email code | C0 | Verification intent + account protected ref | DU tối đa 10 lần thử trong 30 phút/intent; FW 20/24 giờ/account | Durable intent counter; local không là truth | Intent locked/expired; response trung tính | M01/M12; D-014 |
| RL-007 | Resend verification | C0/C1 | Account + purpose + IP coarse | 1 lần/60 giây/account/purpose; tối đa 5/24 giờ/account; FW 20/24 giờ/IP | Durable intent/account counter; distributed IP guard | Giữ cùng semantics, intent mới revoke intent cũ nhưng không reset quota | M01/M12; D-014 |
| RL-008 | Password reset/security recovery request | C0 | Account-candidate + IP + device nếu có | FW 5/24 giờ/account; 10/60 phút/IP; 10/24 giờ/device | Distributed + durable intent | Phản hồi trung tính; không gửi thêm khi budget hết | M01/M12; M01-T019 |
| RL-009 | AI generation | C3 **disabled** | Subject/workload + capability + cost class | Quota runtime = 0 trong A/B; nếu được mở lại: TB 20, refill 5/30 giây/subject trước cost budget | Không traffic hiện tại; mở lại cần distributed + hard cost budget | `CAPABILITY_DISABLED`; không fallback provider | M02/M12; D-010, M12-T046 |
| RL-010 | External image discovery | C2 | Admin/subject + capability | TB 30, refill 10/60 giây/actor; CC 2/actor | Distributed quota; bulkhead riêng | 429/no-result UI; upload/manual asset flow | M02/M12; M12-T015, T038 |
| RL-011 | Managed media upload | C0 ownership, C2 public content | Subject/workload + asset type + owner | FW 20/10 phút/subject; CC 2/subject; bytes/file quota thuộc T021–T025 | Distributed rate + node bulkhead | Reject trước nhận body nếu có thể; không tạo asset ref giả | Asset owner/M12; M12-T021–T025 |
| RL-012 | TTS audio generation | C2 cost | Subject/workload + locale/voice cost class | FW 30/5 phút/subject; CC 2/subject; workload có quota tương đương theo job | Distributed + provider bulkhead | Học bằng text; không retry storm | M02/M12; M12-T017, T036–T038 |
| RL-013 | User pronunciation assessment | C3 **disabled** | Subject + device + capability | Quota runtime = 0 trong A/B | Không traffic/không thu audio | `CAPABILITY_DISABLED`; luyện không chấm nếu flow cho phép | M05/M12; D-010 |
| RL-014 | Matchmaking queue join/leave | C1 | Subject + device; DU một active queue intent/subject | FW 10/60 giây/subject; 20/60 giây/device; DU one-active-entry | Distributed rate + durable queue ownership | 429 hoặc trả entry hiện có idempotently; không ghép trùng | M08/M12; M08-T018–T020 |
| RL-015 | Gym/PvE/PvP battle start | C0 mutation/reward | Subject + battle mode + target | TB 5, refill 1/120 giây/subject/mode; DU active/start operation | Distributed + durable battle constraint | Reject/pending existing operation; không cấp/khởi tạo bù | M08/M12; M08-T009, M08-T025 |
| RL-016 | Battle answer/action | C0 fairness | Subject + battle + round | FW 20/60 giây/subject/battle; DU tối đa một final answer/round | Distributed guard + durable round CAS | Duplicate trả result cũ; over-limit không thành wrong answer | M08/M12; M08-T026–T030 |
| RL-017 | Security email dispatch | C0 recipient binding; C1 delivery | Workload + purpose + recipient protected ref | FW 5/15 phút/recipient/purpose; 20/60 phút/workload/purpose; DU message operation | Durable intent + distributed quota | Pending/rejected an toàn; không đổi recipient/provider để né | M01/M12; M12-T026, T029 |
| RL-018 | Reminder/push notification dispatch | C2 | Workload + channel + recipient + campaign | FW 20/24 giờ/recipient/channel; TB 100, refill 100/60 giây/workload/channel; DU notification | Distributed + durable inbox/intent | Hoãn/drop khi expired theo contract; không retry vô hạn | M10/M12; M12-T026–T030 |
| RL-019 | Provider callback/webhook | C0/C1 theo effect | Verified provider class + capability + endpoint | TB 300, refill 300/60 giây/provider/capability; replay DU; CC 20/capability | Edge/distributed + adapter bulkhead | Reject/buffer bounded theo T035; signature fail luôn reject | M12; M12-T026–T030, T038 |
| RL-020 | Internal job/batch mặc định; policy capability chuyên biệt vẫn áp dụng nếu chặt hơn | Theo operation, cao nhất C0 | Workload identity + capability + job type | TB 100, refill 100/60 giây/workload/capability; CC 2/job type; cost budget riêng | Distributed; không bypass local/global policy bằng header | Hoãn/dừng với backoff; không reset budget khi restart | Module/M11/M12; M11-T038–T039, M12-T037, T046 |
| RL-021 | Sensitive admin/support mutation | C0 | Admin actor + action + protected target | FW 10/15 phút/actor/action; 20/60 phút/target/action; CC 1/actor/action | Distributed + durable audit/re-auth | 429/reject; không thực hiện nếu audit/limiter không chắc | M11/M12; M11-T012–T017 |
| RL-022 | Admin read/export/report | C1 privacy/capacity | Admin actor + report/export class | FW 30/5 phút/actor; CC 1 export, 2 report/actor | Distributed + worker bulkhead | Queue bounded/429; không giảm filter/quyền để chạy | M11/M12; M11-T026 |
| RL-023 | Health/readiness probe | C2 operational | Trusted ingress/probe identity + endpoint | Không app queue; ingress FW 600/60 giây/source và CC 10/node; probe không gọi provider tốn phí/mutation | Edge/node | Shed excess; liveness vẫn rẻ, readiness phản ánh degraded | M11/M12; M11-T036 |

Các số là baseline v1.0, không thay provider quota hoặc budget tiền. M12-T046 phải chọn hard budget nhỏ hơn provider/account limit và gắn kill switch trước khi capability trả phí được activation.

## 4. Scope, thuật toán và key

| Thành phần | Yêu cầu |
|---|---|
| Policy key | `rl:<env>:<policy-version>:<policy-id>:<partition-type>:<protected-partition>`; không chứa email/token/raw IP nếu store/log policy không cho phép |
| Clock | Distributed algorithm dùng server/store time; không dùng client timestamp; clock anomaly phát metric và conservative mode |
| Atomicity | Một bucket update atomic; multi-bucket cần reserve/rollback an toàn hoặc thứ tự cố định, không để bucket phụ tạo bypass |
| Topology | Catalog khai báo `distributed`, `node`, hoặc `edge`; test ít nhất 2 instance cho C0/C1, không suy từ Lua/Redis registration; local fallback phải có max-instance/ingress cap để tổng emergency capacity không vượt trần an toàn |
| Local fallback | Key/version/partition tương đương nhưng cap bảo thủ thấp hơn; không cộng quota mỗi instance như capacity hợp lệ |
| Queue | Auth/mutation/cost: 0; read/report nếu queue phải có deadline, max depth, cancellation và không giữ request body/secret |
| Provider quota | Map về quota class/cost budget nội bộ; không trả raw provider account/quota cho consumer |

## 5. Phản hồi và retry

- API reject dùng Problem Details ổn định `RATE_LIMIT_EXCEEDED`, HTTP 429 và `Retry-After`; không trả remaining quota/account identifier/provider name nếu gây lộ.
- Capability contract dùng `temporaryFailure` hoặc `rejected` với `CONTROL.RATE_LIMITED` theo deadline/finality; HTTP 429 chỉ là transport.
- `Retry-After = min(thời điểm bucket sớm nhất có permit, deadline còn lại, policy max retry delay)` và tối thiểu 1 giây. Hết deadline trả expired/rejected phù hợp, không hẹn retry ngoài hạn.
- Worker dùng jitter/backoff + retry budget cùng operation ID; không chuyển workload identity, route/provider hoặc tạo operation mới để lấy bucket mới.
- Client nhận cùng semantics cho account tồn tại/không tồn tại; limiter log chỉ category/protected partition class, không raw account candidate.

## 6. Governance thay đổi policy

| Trường bắt buộc | Quy tắc |
|---|---|
| Policy/version/effective time | Bất biến sau khi publish; thay threshold/partition/algorithm tăng version |
| Owner + lý do | Business/capability owner và operational owner; nêu abuse/cost/capacity/fairness signal |
| Guard rails | Typed positive values; max/min an toàn trong code/config schema; `0` chỉ là deny-all được ghi rõ, không là unlimited |
| Rollout | Test contract + 429/Retry-After + spoof/NAT/multi-instance; canary có stop criteria |
| Audit | Before/after, actor, reason, scope, expiry nếu tạm thời; cấm manual Redis key/delete làm policy change |
| Rollback | Quay lại version đã kiểm chứng; không disable limiter; security incident có thể siết deny/conservative bằng kill switch audit |

## 7. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát ngày 2026-08-20 | Sai lệch so với baseline | Task tiếp nhận |
|---|---|---|---|
| M12-RATE-I01 | 7 policy ASP.NET được đăng ký, nhưng source chỉ cho thấy `auth-endpoints`, `ai-vocabulary`, `matchmaking-join` gắn endpoint | 4 policy không có coverage quan sát; registration không chứng minh enforcement | M12-T035, T047-A |
| M12-RATE-I02 | Không có `appsettings*.json` được theo dõi trong checkout; option primitive có default chung, trong khi comment route nêu ngưỡng khác cho auth/audio/match/gym | Effective production threshold/source/version không xác định | M11-T012–T017; M12-T040, T047-A |
| M12-RATE-I03 | `GetClientIp` nhận leftmost `X-Forwarded-For` nếu parse được mà không kiểm tra trusted proxy chain | Caller có thể chọn partition/bypass hoặc đánh người khác nếu ingress không strip header | M12-T035, T047-A; A-G01/A-G04 |
| M12-RATE-I04 | Header `X-Internal-Worker` + loopback trả `GetNoLimiter` ở cả 7 policy | Không có workload identity/quota riêng; trái CT-05/M12-D023 | M12-T035, T047-A |
| M12-RATE-I05 | `RedisRateLimiter` đăng ký nhưng không thấy consumer; mọi exception trả `true` | Không có distributed coverage và C0/cost route có thiết kế allow-all | M12-T035, T047-A |
| M12-RATE-I06 | Test production code chỉ chứng minh writer 429/Problem Details và minimal auth policy in-process; fixture tin `X-Forwarded-For` trực tiếp | Chưa test host thật, spoof, multi-instance, Redis failure, workload, composite bucket hoặc coverage | M12-T047-A; REL-03 |
| M12-RATE-I07 | Test yêu cầu health không bao giờ 429 | Chỉ hợp lệ nếu ingress/concurrency cap và probe rẻ theo RL-023 được chứng minh | M11-T036; M12-T047-A |
| M12-RATE-I08 | Chưa thấy policy cho upload, callback, notification, admin/export và internal jobs | Phạm vi CT-05 chưa bao phủ | M12-T021–T030, T035, T047-A; M11-T026, T038–T039 |
| M12-RATE-I09 | AI endpoint có attribute limiter dù capability bị D-010 tắt; activation enforcement chưa chứng minh | Limiter có thể bị nhầm là kill switch; quota khác policy disable | M12-T040–T041, T047-A |

## 8. Ma trận tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| RL34-01 | Giả `X-Forwarded-For` từ socket không phải trusted proxy | Header bị bỏ; bucket theo socket peer |
| RL34-02 | Giả header/claim internal từ ngoài và loopback không có workload identity | Không có bypass; dùng public/user bucket hoặc reject |
| RL34-03 | Cùng account bị thử từ nhiều IP | Account-candidate bucket chặn, response không lộ tồn tại |
| RL34-04 | Nhiều account hợp lệ sau cùng NAT | IP coarse guard có telemetry/fairness; account/device bucket độc lập, không manual bypass |
| RL34-05 | Hai API instance cùng subject | Tổng quota không nhân đôi; distributed counter/coverage được chứng minh |
| RL34-06 | Redis/limiter state lỗi trên auth/battle/admin mutation | Không allow-all; mode theo M12-T035 |
| RL34-07 | Worker retry sau restart | Cùng operation/workload budget; không reset quota hoặc bão retry |
| RL34-08 | Callback signature sai nhưng bucket còn | Reject trước apply; limiter không thay authentication/replay |
| RL34-09 | Request vượt quota | 429/Problem Details + Retry-After hợp lệ; không enqueue mutation hết hạn |
| RL34-10 | AI/pronunciation bị disable | Deny-all trước provider và trước thu dữ liệu; limiter không mở capability |
| RL34-11 | Thay threshold/config | Version/effective/audit/canary/rollback đầy đủ; zero/missing không thành unlimited |
| RL34-12 | Entry point mới không có policy ID | Build/contract coverage test thất bại; không phát hành |

## 9. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M12-RATE-F01 | Fail-open/fail-closed và local conservative behavior từng policy chưa chốt chi tiết | C0 deny/conservative; không global allow-all | M12-T035 |
| M12-RATE-F02 | Budget tiền/provider quota thật chưa có | Capability trả phí không activation chỉ dựa rate count | M12-T046 |
| M12-RATE-F03 | Bytes/file/asset quota và batch/export size chưa chốt | Reject trước body/queue khi có thể; bounded concurrency | M12-T021–T025; M11-T026 |
| M12-RATE-F04 | Coverage/runtime/multi-instance/spoof tests chưa có | Không coi catalog là enforcement evidence | M12-T047-A; REL-03 |

## 10. Tự kiểm M12-T034, A-G01, A-G04 và REL-03

- 23 policy bao phủ auth, identity ngoài, AI/image/speech, upload, battle, email/push, callback, workload, admin/export và health; mỗi policy có partition, algorithm/quota, scope/fallback, response và owner.
- IP/user/device/account/resource/workload/provider/admin partition đều có trust boundary; XFF/header internal tự khai không tạo bypass.
- Auth có composite buckets và phản hồi trung tính, phù hợp A-G01; limiter không thay credential/account-state/anomaly/idempotency controls.
- A-G04/CT-05 có baseline cho aggregate multi-instance, conservative local guard, provider quota, 429/Retry-After, config governance và 12 case.
- 9 finding hiện trạng cùng 4 finding mở có task tiếp nhận; đặc biệt tách registration, endpoint coverage và distributed enforcement.
- REL-03 vẫn mở vì config thật, proxy topology, host coverage, Redis failure, workload identity, multi-instance và canary evidence chưa có.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 23 policy, trust boundary, quota/scope, governance, 9 sai lệch và 12 case | WSA-7K2 |
