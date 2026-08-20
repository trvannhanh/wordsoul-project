# Từ điển chỉ số quản trị M11

| Thuộc tính | Giá trị |
|---|---|
| Dictionary ID | `M11-METRIC-DICT-1.0` |
| Task | M11-T022 |
| Phạm vi | 12 metric điều hành Giai đoạn A; dashboard hiện hành chỉ là nguồn đối chiếu tĩnh |
| Quyết định nền | D-001, D-008, D-020, D-032; M11-D014–D016, D019–D021 |
| Tự kiểm | A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Nguyên tắc

- Metric trả lời một câu hỏi quyết định đã nêu; không thu thập/hiển thị chỉ vì dữ liệu sẵn có.
- Metric không là durable business truth và không tự cấp quyền/change/incident action. Drill-down phải quay về source có quyền.
- Mỗi version khóa tử số, mẫu số, eligibility, exclusion, event/source contract, window/time semantics, dimensions và correction policy.
- `0`, `noData`, `partial`, `stale`, `unknown` khác nhau. Không có source/denominator không được hiển thị 0.
- Event time và processing time đều UTC. Dashboard luôn hiển thị `asOf`, `dataThrough`, freshness/quality state, timezone và metric version.
- Không dùng email/username/IP/device/token/raw payload làm dimension/label. Cohort cá nhân có mẫu số <10 bị suppress; không drill-down ngầm từ aggregate.
- M11 định nghĩa catalog/quality presentation; module nguồn sở hữu event/table semantics và correctness.

## 2. Schema metric bắt buộc

| Trường | Contract |
|---|---|
| `metricId`, `metricVersion`, display key | Stable ID; đổi nghĩa/công thức tạo version mới |
| Decision question/owner | Quyết định/cảnh báo được hỗ trợ và module chịu semantics |
| Numerator/denominator | Exact event/state, distinct key, eligibility, exclusions |
| Unit/aggregation | Count, ratio, distribution/percentile, snapshot; rounding chỉ ở display |
| Source contract | Event/table/projection + schema/version/source owner |
| Event/processing time | UTC fields, late-arrival/correction watermark |
| Window | Tumbling/sliding/snapshot/cohort; start inclusive/end exclusive |
| Dimensions | Exact allowlist, cardinality/small-group policy |
| Freshness target | Class/target, measured from event watermark tới served result |
| Quality checks | Missing/duplicate/range/reconcile/denominator/coverage |
| Result state | fresh/stale/partial/noData/unknown/suppressed |
| Permission/export | Aggregate role, purpose/case for detail, retention/watermark |
| Retention/correction | Raw/aggregate lifetime, backfill/reopen policy |
| Limitations | Điều không được suy luận hoặc dùng làm automated truth |
| Formula/source versions | Effective interval và compatibility/dashboard binding |

## 3. Freshness baseline và result state

T023 sẽ chốt chi tiết late data/aggregation. T022 đặt target ban đầu để không có metric “nhanh” vô hạn:

| Class | Target | Dùng cho |
|---|---|---|
| F0 | dataThrough lag ≤1 phút, evaluate/serve ≤1 phút | Security, incident, critical capability/reconcile signals khi source thật hỗ trợ |
| F1 | lag ≤15 phút, serve ≤5 phút sau watermark | Near-real-time operations |
| F2 | lag ≤2 giờ | Product/learning/battle/delivery/job hourly view |
| F3 | Daily window ready trước 06:00 UTC ngày kế | Content/cohort daily reporting |

| Result | Nghĩa |
|---|---|
| `fresh` | Source/quality complete và lag trong target |
| `stale` | Last known result có provenance nhưng vượt freshness |
| `partial` | Một phần source/window chưa complete, coverage/known gaps explicit |
| `noData` | Source healthy, complete window và zero eligible records |
| `unknown` | Source/contract/watermark/quality không xác định; không hiển thị như 0 |
| `suppressed` | Có dữ liệu nhưng privacy/small-group policy không cho hiển thị |

## 4. Permission và dimension profile

| Profile | Vai trò | Dữ liệu/dimensions | Export |
|---|---|---|---|
| MP-01 Operations | R08-O/R, R09-R aggregate | capability/module/environment/severity/job/version; không actor/user | Aggregate bounded export theo change/incident |
| MP-02 Security | R10-R, exact incident/case | rule version/result class/coarse source; không raw email/IP/device | Case-bound, redacted, access-audited |
| MP-03 Product/learning | R09-R aggregate | market/age policy cohort/content/session/version; k≥10 | Aggregate only; no user list |
| MP-04 Economy | R05-R, R09-R aggregate | asset type/policy/reconcile state; không cộng khác unit | Case-bound detail qua M06 source, không dashboard export raw |
| MP-05 Content | R03/R04/R09 theo scope | content type/group/version/quality state | Assigned scope aggregate |

## 5. Metric catalog — critical/operations

| Metric ID v1 | Formula/eligibility/exclusion | Source/owner/status | Window/freshness | Dimensions/profile | Limitation |
|---|---|---|---|---|---|
| MET-OPS-HEALTHY-CAP | `count(expected critical capability slices with liveness+readiness+dependency+freshness all pass) / count(expected critical slices)`; denominator from versioned capability registry, disabled C3 excluded but shown separately | M12 capability/health + owner; **unavailable**: current health có static/partial signal | Current snapshot, event time UTC; F0 | capability/module/environment/criticality; MP-01 | Không suy correctness từ HTTP 200; unknown dependency làm numerator fail và result partial/unknown |
| MET-SEC-AUTH-ANOMALY | Distinct `authRiskEventId` classified anomalous by exact rule version; duplicate correlation/replay/test traffic excluded | M01 security event catalog; **unavailable** đến M01-T038/39 | Sliding 5m/1h UTC; F0 | rule/result/coarse channel; MP-02 | Không phải unique attackers/users; không hiển thị raw email/IP/device |
| MET-M06-RECON-DIFF | Count of distinct subject+asset reconciliation findings where expected≠actual, split open/resolved/unknown; value totals only within same unit/type | M06 ledger reconciliation + M11 finding; **unavailable** | Current + sliding 1h; F0 | asset type/reconcile state/policy version; MP-04 | Không cộng XP/AP/item/pet thành “tổng tiền”; AP historical only theo D-011 |
| MET-INC-OPEN | Count distinct incident IDs not terminal closed, grouped by severity version; drill-down exact incident | M11 incident registry; **unavailable** | Current snapshot; F0 | severity/capability/environment/age bucket; MP-01 | Count không đo impact/users; severity changes need event history |
| MET-JOB-SUCCESS | `terminal succeeded run count / terminal eligible run count`; denominator includes succeeded/failed/recoveryFailed, excludes running/canceled-before-effect/test; logical run dedupes retries | M11 job registry + module source; **unavailable** | Tumbling 1h/24h UTC; F2 | job/version/module/environment/result; MP-01 | Không coi queue accepted/start là success; low volume shows count+no ratio if denominator<10 |

## 6. Metric catalog — identity/content/learning

| Metric ID v1 | Formula/eligibility/exclusion | Source/owner/status | Window/freshness | Dimensions/profile | Limitation |
|---|---|---|---|---|---|
| MET-M01-REG-COMPLETE | `distinct eligible registrationAttemptId reaching verified/onboarding-complete terminal / distinct eligible attempt started`; exclude synthetic/test/blocked-before-eligibility per rule version; attempt remains in cohort by startedAt | M01 registration/verification/onboarding events; **unavailable** as unified attempt event | Cohort hour/day UTC; F2 | market/policy version/channel/result; MP-03 | Không dùng User row count thay attempt denominator; late completion updates cohort with correction |
| MET-M01-VERIFY-LAG | Distribution p50/p95/p99 of `verifiedAt - firstDeliveryConfirmedAt` for eligible same verification generation; exclude no confirmed delivery, expired/unverified censored and test | M01 intent + M10 delivery evidence; **partial/unavailable** vì accepted không chứng minh delivered | Cohort daily UTC; F3 | channel/provider-result/policy version, k≥10; MP-03 | Không tính từ request/accepted time; phải công bố censored/excluded counts |
| MET-M02-CONTENT-READY | `distinct submitted contentVersion reaching all quality gates / distinct eligible submitted version`; same version, exclude canceled/test; late decision corrects original submit cohort | M02 content version/quality lifecycle; **unavailable** | Daily submit cohort UTC; F3 | content type/group/quality version; MP-05 | Không đồng nhất ready với published/right-cleared nếu REL-04 chưa đạt |
| MET-M03-LEARNING-COMPLETE | `distinct eligible sessionId terminal completed / distinct eligible sessionId started`; exclude test/invalidated-before-start; timeout/abandoned shown separate, not numerator | LearningSessions M03; **partial observed** (`StartTime`, `IsCompleted`) nhưng eligibility/terminal version thiếu | Tumbling hour/day UTC; F2 | session type/content/policy version/cohort k≥10; MP-03 | Completion không đo learning effectiveness; current implementation counts all started in range without timeout policy |
| MET-M04-REVIEW-DUE | Count distinct vocabulary progress items with `nextReviewAt ≤ snapshotAt` and not completed/deleted/ineligible under exact algorithm/policy version | M04 progress/review source; **partial observed** | Hourly snapshot UTC; F2 | algorithm version/due-age bucket/content group, k≥10; MP-03 | Snapshot count không là backlog work time; timezone chỉ display, due truth UTC |

## 7. Metric catalog — battle/notification

| Metric ID v1 | Formula/eligibility/exclusion | Source/owner/status | Window/freshness | Dimensions/profile | Limitation |
|---|---|---|---|---|---|
| MET-M08-MATCH-COMPLETE | `distinct eligible matchId with verified valid terminal result / distinct eligible matchId started`; canceled/abandoned/timeout/error separate; dedupe rematch/retry by match ID | BattleSessions/M08; **partial observed** status/time, terminal validity/version incomplete | Tumbling hour/day UTC; F2 | match type/rule version/result/environment; MP-01/03 | Không dùng rating/win row làm proof match correctness; partial battle not success |
| MET-M10-DELIVERY | `distinct eligible deliveryAttemptId terminal delivered / distinct eligible deliveryAttemptId`; accepted/queued/retried not numerator, invalid endpoint/consent suppression separate | M10 delivery attempt + M12 provider result; **unavailable** as durable terminal receipt | Tumbling hour/day UTC; F2 | channel/provider/campaign/template/result, k≥10; MP-01 | Accepted không phải delivered; provider comparison invalid nếu eligibility/retry policies differ |

## 8. Current dashboard mapping

| Current field/view | Physical calculation observed | Dictionary mapping | Gap/risk |
|---|---|---|---|
| TotalUsers | Count all Users | Không thuộc 12 metric v1 | Không account lifecycle/deleted/market/policy definition; raw count không health |
| ActiveUsersToday | Distinct user with LearningSession StartTime ≥ UTC today | Không gọi DAU; possible future metric | “Active” chỉ học session, thiếu other activity/eligibility/timezone/version |
| TotalVocabularySets | Count all VocabularySets | Không map MET-M02-CONTENT-READY | Không lifecycle/quality/published/right state |
| TotalLearningSessions | Count all LearningSessions | Supporting count only | Không window/eligibility/version/quality |
| NewUsersThisWeek | User CreatedAt ≥ UTC now-7d date | Supporting count only | Sliding/tumbling semantics và account eligibility chưa rõ |
| Session CompletionRate | completed / all loaded sessions since UTC date | Candidate input cho MET-M03 | Missing eligibility/terminal timeout/source version; loads rows to memory |
| OverallAccuracy/response/hints | AnswerRecords in selected sessions | Ngoài v1 | Không question/version/invalid/test rules; response-time distribution không nên chỉ average |
| TopActiveUsers | Completed session count, returns username fallback email | Cấm trong aggregate dashboard v1 | PII/email exposure, no case, small-group, access audit |
| TopXpUsers | User XP/AP order, returns username fallback email | Cấm trong v1 | AP frozen, profile balance not ledger truth, PII/ranking risk |
| PvP leaderboard | User rating/wins/losses | Không map MET-M08 completion | Aggregate player row không chứng minh match terminal validity |
| Admin health | Static “Healthy/Connected” response | Không map MET-OPS-HEALTHY-CAP | Giả định health; không dependency/source/freshness |

## 9. Formula/version/correction rules

- Formula/source/eligibility/exclusion/window/dimension/privacy/freshness thay đổi tạo metric version mới và effective interval; dashboard/query bind exact version.
- Ratio lưu numerator + denominator + result/coverage, không chỉ percentage; denominator zero với complete healthy source là `noData`, không 0%.
- Distinct key là server-owned stable event/operation/entity ID; retry/correlation dedupe rule versioned.
- Late event đi vào original event-time window đến watermark. Sau finalization, correction tạo revision với old/new counts, reason, processedAt và affected windows; không rewrite report im lặng.
- Backfill dùng registered job/operation ID, source version, bounded window và reconciliation. Duplicate backfill không double count.
- Source schema/version unknown, missing partition hoặc reconciliation mismatch làm partial/unknown; dashboard không giữ màu xanh từ result cũ mà không gắn stale.
- Cross-metric comparison chỉ khi eligibility/window/unit/version tương thích; nếu không phải ghi “not comparable”.

## 10. Quality baseline

| Check ID | Áp dụng | Điều kiện |
|---|---|---|
| MQ-01 Source coverage | Tất cả | Expected partitions/sources vs received; missing explicit |
| MQ-02 Uniqueness | Event/attempt/run/session/match | Duplicate stable ID/correlation/retry không double count |
| MQ-03 Range/state | Tất cả | Negative count, impossible timestamp/state/version reject/quarantine |
| MQ-04 Numerator≤denominator | Ratio | Vi phạm làm invalid/incident, không clamp |
| MQ-05 Watermark/freshness | Tất cả | dataThrough/lag/late count và class target |
| MQ-06 Reconciliation | Economy/job/delivery/battle | Aggregate khớp source terminal manifest trong tolerance/version |
| MQ-07 Dimension cardinality/privacy | Tất cả | Allowlist, k≥10, no PII/free-text label |
| MQ-08 Formula/source version | Tất cả | Exact compatible version, không mixed deployment |

T023/T024 sẽ chốt aggregation/late-data và threshold/escalation chi tiết; baseline này đã đủ chặn hiển thị chắc chắn khi quality unknown.

## 11. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| MET-G01 | 12/12 metric có stable ID/version, exact formula/eligibility/exclusion/source/window/freshness/dimensions/profile/limitation |
| MET-G02 | Numerator/denominator/distinct key và zero/noData/unknown/partial semantics được test |
| MET-G03 | Formula/source/window/dimension đổi tạo version, không rewrite history |
| MET-G04 | Event time/watermark/correction/backfill idempotency giữ đúng window |
| MET-G05 | Dashboard hiển thị asOf/dataThrough/freshness/quality/timezone/version |
| MET-G06 | MP-01–05 + k≥10/no PII dimension/field allowlist chặn leak |
| MET-G07 | MQ-01–08 chặn stale/partial/invalid hiển thị như fresh/certain |
| MET-G08 | Missing physical source giữ unavailable/unknown, không synthetic zero/health |
| MET-G09 | Current dashboard fields không được gắn nhãn metric v1 khi chưa semantic mapping |
| MET-G10 | Runtime source/metric/dashboard reconciliation evidence theo exact version |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| MET22-01 | Denominator 0, source complete/healthy | `noData`, numerator/denominator shown; không 0% giả |
| MET22-02 | Denominator/source unknown | `unknown`, không reuse green/zero |
| MET22-03 | Numerator > denominator | Quality invalid + alert/finding; không clamp 100% |
| MET22-04 | Duplicate retry same attempt ID | Count once theo versioned dedupe |
| MET22-05 | Late completion after daily watermark | Correction original cohort + revision, không silently add current day |
| MET22-06 | Formula changes but same display name | New metric version/effective interval |
| MET22-07 | Source schema mixed versions | Partial/unknown; no aggregate merge nếu incompatible |
| MET22-08 | Cohort denominator 9 | `suppressed`; không drill-down/export |
| MET22-09 | Dimension contains email/IP/userName | Reject/redact dimension trước store/serve |
| MET22-10 | Health endpoint 200, dependency stale | Capability not healthy; result partial/unknown |
| MET22-11 | Registration User count dùng thay attempt denominator | Contract reject; not MET-M01-REG-COMPLETE |
| MET22-12 | Verification accepted email nhưng không delivered proof | Exclude/censored; không tính lag từ accepted |
| MET22-13 | Learning completed without eligibility version | Partial/unknown; not publish v1 ratio |
| MET22-14 | Review due timezone display đổi | UTC snapshot count không đổi; label/timezone metadata đổi |
| MET22-15 | Battle abandoned counted completed | Exclude numerator, show result dimension separately |
| MET22-16 | Notification accepted counted delivered | Reject quality check |
| MET22-17 | Job retry tạo two run rows same logical run | Denominator dedupe logical run; attempts separate |
| MET22-18 | Reconcile value sums XP+item/AP | Reject incompatible unit; AP historical only |
| MET22-19 | Dashboard top users fallback email | Field blocked; aggregate/no user list |
| MET22-20 | Backfill replay same operation | Idempotent correction, no double count |
| MET22-21 | Metric stale vượt target | Display stale/asOf/dataThrough; không green |
| MET22-22 | Export aggregate có cohort nhỏ | Suppress/generalize; purpose/expiry/audit required |
| MET22-23 | Metric used to auto-adjust account/asset | Deny; source workflow/case/change required |
| MET22-24 | Missing source model metric | State unavailable/unknown + finding; không seed synthetic value |

## 12. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-MET-I01 | Dashboard counts Users/Sets/Sessions without metric registry/version | Labels không có eligibility/exclusion/quality/freshness | M11-T023–T025; M11-T049 |
| M11-MET-I02 | ActiveUsersToday chỉ distinct LearningSession user theo UTC today | Không phải toàn bộ activity/DAU nhưng tên dễ suy rộng | M11-T023–T025 |
| M11-MET-I03 | Session analytics computes ratio in memory over rows since date | Missing terminal/eligibility/version/watermark; scalability/bound gap | M11-T023–T025; M03 |
| M11-MET-I04 | Top active/XP/PvP views return username fallback email; XP view includes AP | PII/small-group/access audit gap; AP frozen, profile not ledger truth | M11-T025–T026; M11-T033; M06 |
| M11-MET-I05 | Health endpoint returns static healthy/connected | Không có real dependency/freshness source | M11-T036; A-G06 |
| M11-MET-I06 | Không thấy durable auth anomaly/reconcile/incident/job metric sources | 4 critical metric unavailable | M01-T038–T040; M11-T036–T040-A; M06 |
| M11-MET-I07 | Delivery accepted/result hiện tại chưa chứng minh terminal delivered receipt | MET-M10-DELIVERY unavailable | M10/M12; M11-T036 |
| M11-MET-I08 | Không có metric version/result-state/quality/correction records | Không thể truy vết/rebuild/so sánh dashboard | M11-T023–T025; M11-T049 |
| M11-MET-F01 | Freshness/aggregation/watermark/late correction policy per metric | F0–F3 baseline, exact dataThrough required | M11-T023 |
| M11-MET-F02 | Quality thresholds/check execution/reconciliation/escalation | MQ-01–08 baseline | M11-T024; M11-T037 |
| M11-MET-F03 | Role dashboard/drill-down/export/redaction | MP-01–05/k≥10/no PII | M11-T025–T026; M11-T033–T035 |
| M11-MET-F04 | Physical source contracts for unavailable/partial metrics | Unknown không synthetic zero | Module owner tasks; M11-T036–T040-A |
| M11-MET-F05 | Metric store/version/correction/runtime evidence | Exact source/formula/result-state | M11-T049; A-G06 |

## 13. Tự kiểm M11-T022

- 12 metric có exact tử/mẫu/eligibility/exclusion, source-owner-status, window/F0–F3 target, dimensions/profile và limitation.
- Năm permission profile, sáu result state, MQ-01–08 và formula/version/correction rules chặn PII, cohort nhỏ, stale/partial/unknown/zero nhập nhằng.
- Mapping 11 current dashboard field/view chỉ rõ dữ liệu nào không được gắn nhãn metric v1; missing source giữ unavailable.
- 10 gate và 24 case bao phủ denominator, dedupe, late correction, version, privacy, semantic mismatch, health/delivery/job/economy và backfill.
- Tám sai lệch + năm finding có task tiếp nhận. A-G06 vẫn chờ T023–T025, source/health/job và runtime evidence; không kết luận gate đạt.

## 14. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema metric và seed 12 chỉ số dự kiến | Chưa gán |
| 2026-08-20 | 1.0 | Chốt 12 metric, F0–F3/result/profile/quality, current mapping, gate/case/finding | WSA-7K2 |
