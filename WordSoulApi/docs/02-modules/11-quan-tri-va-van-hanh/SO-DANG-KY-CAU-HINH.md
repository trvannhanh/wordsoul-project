# Sổ đăng ký cấu hình M11

| Thuộc tính | Giá trị |
|---|---|
| Registry ID | `M11-CONFIG-REG-1.0` |
| Task | M11-T012 |
| Phạm vi | 29 `SystemConfigurations` key có trong model snapshot + ranh cấu hình deployment/provider |
| Nguồn tĩnh | Entity/service/controller, model snapshot, migrations và consumer C# tracked tại 2026-08-20 |
| Quyết định nền | D-007, D-008, D-011, D-032, D-039–D-042; M11-D008–D011, D020–D021 |
| Tự kiểm | A-G02, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Phạm vi và ranh giới

Registry này quản lý metadata của khóa cấu hình nghiệp vụ/runtime trong bảng `SystemConfigurations`; không sao chép secret, connection string, provider credential hoặc giá trị environment.

- M11 sở hữu schema/lifecycle/change orchestration; module ghi ở cột Owner sở hữu nghĩa, validation, default và consumer behavior.
- M12 sở hữu deployment/provider/secret/configuration-source inventory ngoài bảng; M11 chỉ giữ protected reference khi một change cần chúng.
- Model snapshot là inventory tĩnh, không chứng minh database từng môi trường đã migrate/không drift. Runtime inventory và checksum còn là finding.
- `IsLiveEditable=true` hiện tại chỉ là cờ code quan sát được, **không** là bằng chứng khóa an toàn để live-change. Cột “Hiệu lực đích” dưới đây mới là policy target.
- Category không quyết định độ nhạy. `GENERAL` hiện bị endpoint anonymous trả toàn bộ nên mọi key phải có exposure policy riêng.

## 2. Schema registry bắt buộc

| Trường | Contract |
|---|---|
| `configId`, `physicalKey`, `keyVersion` | Stable ID/key, canonical case; không tái dùng key cho nghĩa khác |
| `ownerModule`, `ownerContract` | Một module chịu invariant; không có key vô chủ |
| `consumerIds`, `consumerContractVersion` | API/service/job/client đọc; behavior khi missing/invalid/stale |
| `policySetId`, `atomicity` | Nhóm khóa phải version/apply/rollback cùng nhau |
| `dataType`, `unit`, `defaultSource` | Invariant culture; default explicit và có nguồn |
| `rangeOrEnum`, `crossRules` | Constraint đơn và chéo; chi tiết T013 |
| `scope`, `precedence` | Global/environment/market/cohort…; source precedence rõ |
| `dataClass`, `exposure` | Public/Internal/Sensitive/SecretRef; endpoint được phép đọc |
| `riskClass`, `changeClass`, `limitId` | R1–R4/EC-1–EC-4 và enhanced-control limit |
| `editPermission`, `readPermission` | Permission/scope server-side; không dựa category/role string đơn thuần |
| `effectMode` | Derived, immediate, scheduled live, restart, release hoặc disabled |
| `effectiveBoundary` | New request/session/job/version hay tất cả consumer; không đổi giữa business operation |
| `fallbackPolicy` | Fail-closed/default đã đăng ký/degraded; không hardcode ngầm |
| `version/rollback` | Immutable version, previous-compatible target và verify plan |
| `monitoring/health` | Metric/source/freshness/stop condition |
| `retention` | Lịch sử cấu hình tối thiểu 12 tháng; legal/incident hold thắng deletion |
| `lifecycle/deprecation` | proposed/registered/active/deprecated/retired; reference check trước retire |

## 3. Permission/exposure profile

| Profile | Read | Change | Exposure rule |
|---|---|---|---|
| CP-01 Public presentation | Anonymous qua exact public allowlist; admin R08-R | R08-A/P/O + change contract | Chỉ branding/contact public, URL/text validated |
| CP-02 Internal product policy | Fixed workload consumer; R08-R metadata | R08-A/P/O + owner validation | Không anonymous; value chỉ consumer đúng purpose |
| CP-03 Security/access switch | M01/M12 fixed workload; R08/R12 metadata theo scope | R08-A/P/O + owner M01/M12 invariant, EC-4 | Không anonymous; missing/unknown fail-closed |
| CP-04 Economy/ledger policy | M03/M06 fixed workload; R05/R08 metadata | R08-A/P/O + M06 owner validation, EC-4 | Không anonymous; không sửa lịch sử/ledger |
| CP-05 Operations/retention | M11 fixed workload; R08/R10 metadata | R08-A/P/O, EC-4 | Không anonymous; audit/security retention không dùng chung log thường |

## 4. Policy set registry

| Policy set | Owner | Physical keys | Consumer/boundary | Risk | Hiệu lực đích |
|---|---|---|---|---|---|
| CFG-SRS-ALGORITHM | M04 | 9 key `Srs*` | M04 algorithm, M03/review outcome; snapshot per review/session | R4 | Scheduled live as one immutable set; version auto-derived; existing operation giữ snapshot |
| CFG-LEARNING-SESSION | M03 | `WordsPerSession` | New learning/review session only | R3 | Scheduled live; không đổi vocabulary list của session đang chạy |
| CFG-ECONOMY-REWARD | M06 | Catch/XP/AP keys | M03 producer → M06 ledger/asset invariant | R4 | Scheduled version; new operation only; AP key disabled/deprecated theo D-011 |
| CFG-IDENTITY-ACCESS | M01/M12 | Registration/Google-login switches | Auth/OAuth entry point | R4 | Disabled until consumer + fail-closed runtime mapping được chứng minh |
| CFG-PUBLIC-PRESENTATION | M11/platform UI | 10 branding/contact key; public subset 8 | Public/admin settings API và client display | R2 | Immediate/scheduled live theo exact exposure allowlist; cache/version/URL validation required |
| CFG-COMMUNITY-LIMIT | M09 | `MaxGroupSize` | Group create/join invariant | R3 | Disabled until M09 consumer mapping; then scheduled new mutation only |
| CFG-OPERATIONS | M11 | Maintenance/log-retention keys | Maintenance surface/log cleanup | R4 | Maintenance key không phải real kill switch; retention disabled for change until T035 policy |

## 5. Physical key inventory — algorithm, learning và economy

`Default` là seed/model-snapshot value, không chứng minh runtime value. Range rỗng nghĩa metadata hiện tại thiếu và T013 phải chặn activate cho tới khi rule được đăng ký.

| Config ID / key | Owner / consumer | Type; seed default; range | Class/profile | Hiệu lực đích | Retention/deprecation |
|---|---|---|---|---|---|
| CFG-001 `SrsPolicyVersion` | M04 / SRS, review history | Integer; 1; ≥1 | Internal R4 / CP-02 | Derived, non-editable; increment atomically với set | Giữ vĩnh viễn trong history refs; không delete |
| CFG-002 `SrsMinEf` | M04 / SRS | Float; 1.3; 1–3 | Internal R4 / CP-02 | CFG-SRS scheduled set | 12 tháng + referenced versions |
| CFG-003 `SrsMaxEf` | M04 / SRS | Float; 4.0; 1.3–5 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-004 `SrsDefaultEf` | M04 / SRS | Float; 2.5; 1.3–4 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-005 `SrsInitialInterval1` | M04 / SRS | Integer days; 1; 0–30 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-006 `SrsInitialInterval2` | M04 / SRS | Integer days; 6; 1–90 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-007 `SrsMasteredIntervalDays` | M04 / SRS | Integer days; 21; 7–365 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-008 `SrsRetentionBonusPerRepetition` | M04 / SRS | Float points; 2; 0–10 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-009 `SrsRetentionBonusMax` | M04 / SRS | Float points; 20; 0–100 | Internal R4 / CP-02 | CFG-SRS scheduled set | Như CFG-002 |
| CFG-010 `WordsPerSession` | M03 / LearningSession | Integer words; 5; 1–30 | Internal R3 / CP-02 | Scheduled; chỉ session mới | 12 tháng + session policy ref |
| CFG-011 `CatchRateWrongPenalty` | M06 / M03 learning + pet outcome | Float ratio; 0.05; 0–1 | Sensitive R4 / CP-04 | Economy version; new operation only | 12 tháng + ledger/session refs |
| CFG-012 `XpRewardNewSession` | M06 / M03 → ledger | Integer XP; 20; 0–10.000 | Sensitive R4 / CP-04 | Economy version; new reward operation | 12 tháng + ledger refs |
| CFG-013 `XpRewardReviewSession` | M06 / M03 → ledger | Integer XP; 100; 0–10.000 | Sensitive R4 / CP-04 | Economy version; new reward operation | 12 tháng + ledger refs |
| CFG-014 `ReviewBaseAP` | M06 / M03 currently | Integer AP; 3; 0–1.000 | Sensitive R4 / CP-04 | **Disabled/deprecated**; không edit/cấp AP mới theo D-011 | Giữ history đến khi AP removal/reconcile hoàn tất; không hard-delete |

## 6. Physical key inventory — access, presentation, community và operations

| Config ID / key | Owner / consumer | Type; seed default; range/allowlist | Class/profile | Hiệu lực đích | Retention/deprecation |
|---|---|---|---|---|---|
| CFG-015 `AllowRegistration` | M01 / hiện chỉ public settings seed/read | Boolean; true | Sensitive R4 / CP-03 | Disabled until auth consumer + policy mapping; không public | 12 tháng; deprecate nếu thay feature flag registry |
| CFG-016 `AllowGoogleLogin` | M01/M12 / hiện chỉ public settings seed/read | Boolean; true | Sensitive R4 / CP-03 | Disabled until OAuth consumer + REL-03 mapping; không public | 12 tháng; giữ provider-policy refs |
| CFG-017 `MaintenanceMode` | M11 / hiện chỉ public settings seed/read | Boolean; false | Internal R4 / CP-05 | Không phải kill switch/maintenance truth; deprecate hoặc nối T043 | 12 tháng + incident refs |
| CFG-018 `MaxGroupSize` | M09 / hiện chỉ public settings seed/read | Integer members; 50; **range thiếu** | Internal R3 / CP-02 | Disabled until M09 validation/consumer; không public | 12 tháng + group-policy refs |
| CFG-019 `AppDisplayName` | M11/platform UI / public API | String; `VocaMon`; text allowlist thiếu | Public R2 / CP-01 | Public presentation allowlist | 12 tháng; supersede, không delete used version |
| CFG-020 `AdminAppName` | M11/admin UI / public API hiện tại | String; `VocaMon Admin`; allowlist thiếu | Internal R2 / target CP-02 | **Không public**; client mapping/version cần chứng minh | 12 tháng |
| CFG-021 `AdminAppLogo` | M11/admin UI / public API hiện tại | URL; seeded public asset URL; host allowlist thiếu | Internal R2 / target CP-02 | Không public; validated asset ref, không arbitrary URL | 12 tháng + asset ref |
| CFG-022 `WebAppName` | M11/platform UI / public API | String; `VocaMon`; allowlist thiếu | Public R2 / CP-01 | Public presentation allowlist | 12 tháng |
| CFG-023 `WebAppSubtitle` | M11/platform UI / public API | String; seeded Vietnamese subtitle; allowlist thiếu | Public R2 / CP-01 | Public presentation allowlist | 12 tháng |
| CFG-024 `WebAppLogo` | M11/platform UI / public API | URL; seeded public asset URL; host allowlist thiếu | Public R2 / CP-01 | Validated public asset ref | 12 tháng + asset ref |
| CFG-025 `WebAppFavicon` | M11/platform UI / public API | URL; seeded public asset URL; host allowlist thiếu | Public R2 / CP-01 | Validated public asset ref | 12 tháng + asset ref |
| CFG-026 `ContactEmail` | M11/support presentation / public API | String/email; seeded public support address; validation thiếu | Public R2 / CP-01 | Public allowlist, canonical email validation | 12 tháng |
| CFG-027 `FooterCopyright` | M11/platform UI / public API | String; seeded copyright text; allowlist thiếu | Public R2 / CP-01 | Public presentation allowlist | 12 tháng |
| CFG-028 `FacebookUrl` | M11/platform UI / public API | URL; seeded public social URL; host allowlist thiếu | Public R2 / CP-01 | Validated public URL allowlist | 12 tháng |
| CFG-029 `LogRetentionDays` | M11 / LogCleanup worker | Integer days; 7; **range thiếu** | Sensitive R4 / CP-05 | Change disabled until T035; target operational log 30d, audit/security 12mo tách riêng | 12 tháng config history + hold; key deprecate khi retention registry thay thế |

## 7. Consumer/fallback contract quan sát được và target

| Consumer | Keys | Hiện trạng | Target bắt buộc |
|---|---|---|---|
| `SrsAlgorithmSettings` | CFG-001–009 | Parse invariant culture, hardcoded defaults, cross-rule validation; service increments version khi algorithm key đổi | Một immutable policy set/version; missing/invalid fail-closed cho new operation, snapshot per session/review |
| `LearningSessionService` | CFG-010–014, CFG-011–013 | `GetValueAsync` dùng hardcoded default; `WordsPerSession <=0` tự về 5; reward/AP đọc runtime | Registry version snapshot; no silent invalid fallback; economy effect qua M06; AP dependency removal |
| Public Settings API | GENERAL CFG-015–028 | Trả anonymous mọi key category GENERAL | Exact CP-01 allowlist; CFG-015–018/020–021 không public; response có config version/freshness |
| `LogCleanupBackgroundWorker` | CFG-029 | Mặc định 7 nếu missing/invalid; chạy delete mỗi 24h | Retention policy tách log/audit/security, bound + hold, fail-safe không delete khi invalid/unknown |
| Admin Settings APIs | Tất cả | Admin/SuperAdmin read; SuperAdmin create/update/delete; arbitrary category/key và direct mutation | M11-ACT-036 + R08 scope, registered key only, immutable change/version/rollback; no hard-delete |

## 8. Lifecycle và precedence

| State | Entry | Allowed next |
|---|---|---|
| `proposed` | Key metadata draft, chưa consumer owner | registered/canceled |
| `registered` | Đủ owner/schema/exposure/risk/validation/effect/fallback | active/deprecated |
| `active` | Có immutable version + consumer coverage/runtime evidence | deprecated |
| `deprecated` | Không dùng cho consumer/operation mới; reference còn được đọc | retired hoặc active qua revision/decision mới |
| `retired` | Không reference/hold/runtime read, migration verify | Terminal metadata; physical cleanup riêng có manifest |

Precedence target: sealed operation/session snapshot > scoped published policy version > global published version > registered default. Environment variable/appsettings không được âm thầm override business policy; nếu là source hợp lệ phải có source ID/version/owner và được M12 registry ghi nhận. Missing/invalid/unknown không tự trở thành default, trừ CP-01 presentation đã đăng ký degraded default không đổi business truth.

## 9. Registry gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CFG-G01 | Snapshot coverage đúng 29/29 key, stable ID duy nhất, không orphan owner |
| CFG-G02 | Mỗi key có type/default/range status, scope/class/risk/profile/effect/retention |
| CFG-G03 | Consumer/fallback thực tế và target được phân biệt; seed/flag không là runtime evidence |
| CFG-G04 | Public endpoint dùng exact CP-01 allowlist, không category-only exposure |
| CFG-G05 | Security/economy/retention key EC-4; AP frozen/deprecated; secret value không vào registry |
| CFG-G06 | Live-change chỉ theo policy set/allowlist + immutable version/schedule/rollback |
| CFG-G07 | Missing/invalid/unknown fail-safe theo consumer; không silent unsafe fallback |
| CFG-G08 | Create/update/delete chỉ registered key/change contract; reference/hold chặn hard-delete |
| CFG-G09 | Runtime DB inventory/schema/value digest/drift có evidence, không copy sensitive value |
| CFG-G10 | Consumer coverage test và current-version reporting chặn orphan/drift/stale read |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CFG12-01 | Snapshot có key không trong registry | Coverage gate fail; deny release/change |
| CFG12-02 | Create arbitrary GENERAL key | Deny; phải proposed→registered trước |
| CFG12-03 | Anonymous GET settings | Chỉ CFG-019, 022–028 theo exact allowlist; không access/security/internal key |
| CFG12-04 | `AllowRegistration` missing/invalid | Auth policy fail-closed/known disabled; không default allow-all |
| CFG12-05 | Một SRS key đổi không kèm policy set/version | Reject; không partial policy apply |
| CFG12-06 | SRS default ngoài min/max/cross-rule | T013 reject; không publish |
| CFG12-07 | WordsPerSession đổi giữa session | Existing session giữ snapshot, session mới dùng version mới |
| CFG12-08 | XP reward change cố sửa ledger cũ | Deny; new operation only, no history mutation |
| CFG12-09 | ReviewBaseAP change | Deny/deprecated theo D-011; không cấp AP mới |
| CFG12-10 | MaxGroupSize chưa có range/consumer | Không activate/change effect; finding giữ mở |
| CFG12-11 | MaintenanceMode=true | Không tuyên bố real maintenance/kill switch nếu T043 chưa verify |
| CFG12-12 | LogRetentionDays invalid/zero/missing | Không delete log; alert/finding, không fallback 7 để xóa |
| CFG12-13 | Audit/security log dùng CFG-029 | Deny contract; retention class riêng 12 tháng/hold |
| CFG12-14 | URL branding trỏ host không allowlist | Reject version; không public arbitrary URL |
| CFG12-15 | Delete key còn consumer/reference | Deny/deprecate; không hard-delete |
| CFG12-16 | `IsLiveEditable=true` nhưng registry effect disabled | Deny; registry target thắng cờ legacy |
| CFG12-17 | Runtime DB thiếu migration key | Drift alert/fail-safe; không tự insert ngoài controlled migration/change |
| CFG12-18 | Runtime có extra key | Quarantine/not active; không public/consume/change |
| CFG12-19 | Appsettings/environment override business key | Deny hoặc registry source/version explicit; không hidden precedence |
| CFG12-20 | Secret-like value/category gửi create/update | Reject/redact; dùng M12 secret reference contract |

## 10. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CFG-I01 | Snapshot có 29 key, entity chỉ chứa key/value/type/min/max/live/category/update metadata | Registry + validation design đã chốt; runtime schema còn thiếu version/effect/fallback/retention/deprecation | M11-T014–T017; M11-T049 |
| M11-CFG-I02 | 28/29 key `IsLiveEditable=true`; admin/settings route mutate trực tiếp | Cờ legacy quá rộng, không request/version/schedule/rollback | M11-T014–T017; M11-T049 |
| M11-CFG-I03 | Anonymous endpoint trả mọi category GENERAL | Lộ access/maintenance/community/admin config metadata; category thay allowlist | M11-T033; M11-T049 |
| M11-CFG-I04 | 14 key learning/economy đọc với hardcoded fallback; AP vẫn được đọc | Validation target đã chốt; runtime còn silent policy drift/unsafe fallback/AP dependency | M03/M04/M06 tasks; M11-T014–T017 |
| M11-CFG-I05 | Log cleanup missing/invalid fallback 7 và xóa trực tiếp | Không bound/hold/class split; trái target 30d/12mo | M11-T035; M11-T039–T040-A |
| M11-CFG-I06 | Không thấy consumer business cho access/community/maintenance switches | Seed/public visibility không chứng minh effect | M01/M09/M11-T043; M11-T049 |
| M11-CFG-I07 | Create arbitrary key/category và delete vật lý qua SuperAdmin route | Orphan/public exposure/reference loss có thể phát sinh | M11-T014, T017; M11-T049 |
| M11-CFG-I08 | Không có runtime DB inventory/drift evidence trong docs | Snapshot không chứng minh deployed values/schema | M11-T049; REL-03 |
| M11-CFG-F02 | Immutable policy-set version/current/scheduled history | Không update row tại chỗ | M11-T014 |
| M11-CFG-F03 | Preview/impact/metric/rollout/deprecation implementation | Target contract đã xác định theo policy set | M11-T015–T017 |
| M11-CFG-F04 | Audit/redaction/access/retention và public allowlist runtime | CP-01..05 + no secret/category exposure | M11-T031–T035; M11-T049 |
| M11-CFG-F05 | Runtime DB/appsettings/env/provider inventory và consumer coverage | Unknown/drift không active; secret values không thu vào artifact | M11-T049; M12-T040–T043; REL-03 |

## 11. Tự kiểm M11-T012

- Inventory đủ 29 model-snapshot key, cấp stable CFG-001–029, owner/consumer/type/default/range status/class/risk/effect/retention và không để key vô chủ.
- Bảy policy set, năm permission/exposure profile và năm consumer contract phân tách observed behavior với target.
- Xác định rõ 9 SRS key atomic set, 5 learning/economy key, AP deprecated, 14 GENERAL key exposure và log-retention gap.
- 10 gate và 20 case bao phủ orphan/extra/missing key, public exposure, live flag, fallback, version/reference, drift, AP và secret boundary.
- Tám sai lệch + bốn finding còn mở có task tiếp nhận; validation design đã chốt ở M11-CONFIG-VALIDATION-1.0. A-G02/A-G06 vẫn chờ T014–T017, audit và runtime coverage; không kết luận gate đạt.

## 12. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema và seed 12 nhóm dự kiến | Chưa gán |
| 2026-08-20 | 1.0 | Chốt 29 physical key, 7 policy set, owner/consumer/exposure/effect/lifecycle, gate/case/finding | WSA-7K2 |
