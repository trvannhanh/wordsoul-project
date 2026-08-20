# Kiểm tra giá trị và phụ thuộc cấu hình M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CONFIG-VALIDATION-1.0` |
| Task | M11-T013 |
| Đầu vào | M11-CONFIG-REG-1.0, M11-CHANGE-REQUEST/EXECUTION-1.0 |
| Phạm vi | CFG-001–CFG-029 và bảy policy set |
| Tự kiểm | A-G02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Nguyên tắc

Một cấu hình chỉ hợp lệ khi đồng thời đúng schema, canonical form, scalar constraint, cross-rule, owner invariant, exposure/lifecycle và consumer compatibility. Parse thành công hoặc nằm trong min/max chưa đủ.

- Server resolve registry/version và current values; client không chọn validator, risk, owner hoặc dependency version.
- Validation là pure/deterministic với cùng input + registry/source versions; không effect và không sửa/cắt/ép giá trị âm thầm.
- Batch/policy set validate toàn bộ trên candidate snapshot. Một lỗi làm cả candidate invalid; không apply phần hợp lệ.
- Missing/unknown validator/dependency/consumer acknowledgment là fail-closed cho active/business/security/economy/operations policy.
- Error trả stable code + field/config ID + safe parameters; không echo secret/raw payload/internal exception.
- Module owner là architectural owner của invariant, không phải bước phê duyệt người thứ hai.

## 2. Validation pipeline

| Phase | Kiểm tra | Đầu ra |
|---|---|---|
| CV-01 Registry | Config ID/key/version, lifecycle, policy set, owner/consumer tồn tại | Registered candidate hoặc error |
| CV-02 Schema | Type, size, encoding, unknown/duplicate key, forbidden field | Canonical typed candidate |
| CV-03 Scalar | Range/enum/pattern/precision/unit | Per-key result |
| CV-04 Cross-key | Quan hệ trong policy set và derived field | Candidate-set result |
| CV-05 Owner invariant | Module source validator/version | `pass/reject/unknown` + safe code |
| CV-06 Dependency | Source/config/capability/hold/reference/consumer versions | Dependency result/digest |
| CV-07 Exposure/data | Public allowlist, URL/email/text policy, no secret/PII | Exposure result |
| CV-08 Lifecycle/effect | Active/deprecated/disabled, effect boundary, AP freeze | Allowed operation/effect mode |
| CV-09 Compatibility | Consumer contract/version, fallback, snapshot/migration | Compatibility matrix |
| CV-10 Change controls | EC/limit, preview, schedule, rollback, monitoring freshness | Ready/invalid evidence bundle |
| CV-11 Revalidation | Current source/registry/authority/conflict before claim | Execute guard |
| CV-12 Post-apply | Published version parses/loads, consumer convergence/postcondition | Verified result hoặc rollbackRequired |

## 3. Canonical type rules

| Type | Canonical input | Reject |
|---|---|---|
| Boolean | JSON boolean hoặc exact lowercase `true`/`false`; persist canonical lowercase | `1/0`, yes/no, mixed whitespace/case nếu không qua typed API |
| Integer | Base-10 invariant, optional leading `-` only khi range cho phép; no leading `+` | Fraction, exponent, separator, overflow, whitespace |
| Float/decimal | Invariant decimal syntax; normalize without losing declared precision | NaN, infinity, locale comma, exponent nếu registry không cho, overflow |
| Text | Unicode NFC, length in scalar values, no control/bidi override/NUL | Silent trim/truncate, markup/script khi field plain text |
| Email | Canonical mailbox validation, max 254, no control/newline; display value explicit | Header injection, invalid domain/mailbox, hidden whitespace |
| URL | Absolute HTTPS, max 2.048, registered host/path class; no userinfo/fragment | HTTP, IP/private host, data/javascript/file scheme, credentials, secret query |

Value tối đa 500 ký tự của entity hiện tại là storage ceiling, không thay field-specific rule. URL target 2.048 cần schema migration trước khi dùng giá trị dài hơn 500; validator dùng min(field rule, deployed storage capability).

## 4. Scalar rule registry — numeric/boolean

| Config IDs | Type/range hoặc enum | Precision/unit | Rule bổ sung |
|---|---|---|---|
| CFG-001 | Integer ≥1 | policy version | Derived only; client change luôn reject |
| CFG-002 | Float 1.0–3.0 | ≤3 decimal, EF | ≤CFG-003; CFG-004 nằm giữa |
| CFG-003 | Float 1.3–5.0 | ≤3 decimal, EF | ≥CFG-002/CFG-004 |
| CFG-004 | Float 1.3–4.0 | ≤3 decimal, EF | CFG-002≤value≤CFG-003 |
| CFG-005 | Integer 0–30 | days | ≤CFG-006 |
| CFG-006 | Integer 1–90 | days | ≥CFG-005; ≤CFG-007 |
| CFG-007 | Integer 7–365 | days | ≥CFG-006 |
| CFG-008 | Decimal 0–10 | ≤2 decimal, points/repetition | Nếu CFG-009=0 thì value phải 0 |
| CFG-009 | Decimal 0–100 | ≤2 decimal, points | ≥CFG-008 |
| CFG-010 | Integer 1–30 | words/session | Bound selector/session snapshot |
| CFG-011 | Decimal 0–1 | ≤4 decimal, ratio | New-operation only; không đổi existing session |
| CFG-012–013 | Integer 0–10.000 | XP/operation | M06 limit/ledger invariant + preview |
| CFG-014 | Không nhận candidate | AP deprecated | Mọi create/update/activate reject `configDeprecated` |
| CFG-015–017 | Boolean | exact canonical | CFG-015/016 disabled until consumer; CFG-017 không là kill switch truth |
| CFG-018 | Integer 2–500 | members/group | M09 owner validator + existing-group impact required |
| CFG-029 | Integer 30–90 | operational-log days | Không áp dụng audit/security log; hold thắng cleanup |

Seed CFG-029 = 7 và metadata range rỗng là legacy-invalid theo target 30–90; T013 không tự đổi dữ liệu. Consumer phải không xóa khi candidate/current policy invalid hoặc unknown cho tới M11-T035/migration có kiểm chứng.

## 5. Scalar rule registry — presentation

| Config IDs | Rule | Exposure |
|---|---|---|
| CFG-019, CFG-020, CFG-022 | Text 1–80; NFC; plain text; không markup/control/bidi override | CFG-019/022 public; CFG-020 internal admin only |
| CFG-023 | Text 1–160; NFC; plain text | Public |
| CFG-027 | Text 1–200; NFC; plain text; copyright symbol allowed | Public |
| CFG-021, CFG-024, CFG-025 | HTTPS asset URL; registered CDN/asset host/path; content type/image size ownership checked | CFG-021 internal; CFG-024/025 public |
| CFG-026 | Public support email, canonical/max254/no newline | Public exact field only |
| CFG-028 | HTTPS URL; exact approved social host/path; no redirect/query secret | Public |

Host allowlist là versioned registry data do platform/asset owner quản, không hardcode client-provided wildcard. Missing allowlist/asset metadata làm candidate invalid.

## 6. Cross-rule theo policy set

| Rule ID | Policy set | Rule |
|---|---|---|
| CR-SRS-01 | CFG-SRS-ALGORITHM | Candidate luôn gồm CFG-002–009; CFG-001 do server tăng đúng +1 trong same atomic version |
| CR-SRS-02 | CFG-SRS-ALGORITHM | CFG-002 ≤ CFG-004 ≤ CFG-003 |
| CR-SRS-03 | CFG-SRS-ALGORITHM | CFG-005 ≤ CFG-006 ≤ CFG-007 |
| CR-SRS-04 | CFG-SRS-ALGORITHM | CFG-008/009 không âm; CFG-008≤CFG-009; max=0 ⇒ per-repetition=0 |
| CR-SRS-05 | CFG-SRS-ALGORITHM | Owner simulation/compatibility pass; existing review/session giữ policy snapshot |
| CR-LRN-01 | CFG-LEARNING-SESSION | CFG-010 ≤ LIM-04/session selector ceiling và không đổi session đang chạy |
| CR-ECO-01 | CFG-ECONOMY-REWARD | CFG-011–013 dùng M06 asset/ledger version, limit và no-history-mutation; CFG-014 bị loại khỏi candidate |
| CR-ID-01 | CFG-IDENTITY-ACCESS | CFG-015 true chỉ khi registration policy/consent/rate-limit/credential flow active và healthy |
| CR-ID-02 | CFG-IDENTITY-ACCESS | CFG-016 true chỉ khi exact OAuth capability/config/redirect/state/replay controls verified; unknown fail-closed |
| CR-PUB-01 | CFG-PUBLIC-PRESENTATION | Public set chỉ CFG-019, 022–028; admin/internal/access key không được category-only expose |
| CR-GRP-01 | CFG-COMMUNITY-LIMIT | Hạ CFG-018 không tự loại member/group hiện có; impact + remediation policy bắt buộc |
| CR-OPS-01 | CFG-OPERATIONS | CFG-017 không được dùng làm real maintenance/kill-switch success signal |
| CR-OPS-02 | CFG-OPERATIONS | CFG-029 chỉ operational SystemLogs; audit/security retention theo contract riêng 12 tháng/hold |

## 7. Dependency result và freshness

| Result | Nghĩa | Hành vi |
|---|---|---|
| `pass` | Exact validator/source/consumer version xác nhận | Có thể đi tiếp nếu phase khác pass |
| `reject` | Invariant/compatibility không đạt xác định | Invalid; stable error code |
| `unknown` | Timeout/null/version mismatch/source unavailable | Invalid/fail-closed; reconcile, không coi pass |
| `notApplicable` | Registry chứng minh rule không áp dụng cho key/effect | Ghi rule/version/reason; không dùng để né owner validation |

Evidence ref gồm validator ID/version, policy-set digest, source/consumer versions, checkedAt/dataThrough và result. Freshness không vượt change revision expiry; source/consumer/registry version đổi làm stale ngay.

## 8. Error taxonomy

| Code | Khi dùng | Retry hint |
|---|---|---|
| `configUnknown` | Config ID/key/version không đăng ký | Không; registry change required |
| `configDeprecated` | Key disabled/deprecated/retired cho operation mới | Không |
| `typeInvalid` | Không parse/canonical/overflow | Sau khi sửa input |
| `rangeInvalid` | Ngoài min/max/precision/unit | Sau khi sửa input |
| `patternInvalid` | Text/email/URL/host không đạt | Sau khi sửa input |
| `crossRuleInvalid` | Candidate set vi phạm relation | Sau khi sửa full set |
| `duplicateKey` | Candidate có key lặp/canonical collision | Sau khi sửa input |
| `ownerRejected` | Module invariant reject | Không cùng revision |
| `dependencyUnknown` | Dependency/validator timeout/missing/stale | Reconcile/revalidate có bound |
| `consumerIncompatible` | Consumer version/snapshot/fallback không hỗ trợ | Sau migration/rollout plan |
| `exposureViolation` | Public/internal/secret boundary sai | Không; sửa schema/exposure |
| `effectModeDenied` | Immediate/live/restart/release không đúng registry | Sửa change plan |
| `evidenceStale` | Digest/freshness/version không khớp | Generate evidence mới |
| `storageIncompatible` | Candidate vượt deployed schema/precision/size | Sau migration |
| `validationUnavailable` | Không chạy được required phase/audit | Fail-closed; retry có bound |

## 9. Validation API/result

Input: `revisionId`, policy-set candidate typed, expected registry/current/source/consumer versions và protected evidence refs. Output không echo raw input:

| Trường | Nội dung |
|---|---|
| `validationId`, `candidateDigest` | Stable result identity và canonical digest |
| `registryVersion`, `validatorVersions` | Exact contracts dùng |
| `result` | `pass`, `reject`, `unknown` |
| `errors[]` | Config ID, phase, code, safe constraint parameters |
| `warnings[]` | Non-blocking display/migration note; warning không che blocking error |
| `source/consumerVersions` | Protected refs/digests |
| `checkedAt`, `validUntil` | UTC server time/freshness |

Same revision/digest/versions có thể trả cached immutable result. Cùng idempotency key nhưng digest khác là conflict. Validation `pass` không cấp permission, authorize hay bảo đảm execution-time state còn hợp lệ.

## 10. Regression gate và case tự kiểm

| Gate ID | Điều kiện đạt |
|---|---|
| CV-G01 | 29/29 key map type/scalar/cross/lifecycle/exposure rule; không validator orphan |
| CV-G02 | Pipeline CV-01–CV-12 fail-closed với unknown dependency/audit |
| CV-G03 | Seven policy sets validate atomic candidate; không partial accept |
| CV-G04 | SRS five cross-rule, version +1 và session/review snapshot được test |
| CV-G05 | Economy/AP/access/community/retention safeguards chặn unsafe activation |
| CV-G06 | Public exact allowlist + URL/email/text/data-boundary tests đạt |
| CV-G07 | Error taxonomy stable/field-safe; không raw value/secret/exception echo |
| CV-G08 | Validator/source/consumer version/freshness đổi làm evidence stale |
| CV-G09 | Pre-execution revalidation và post-apply consumer convergence bắt buộc |
| CV-G10 | Runtime current values được validate mà artifact không thu secret/sensitive value |

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| CV13-01 | Unknown/case-colliding key | `configUnknown`/`duplicateKey` |
| CV13-02 | Integer `5.0`, `1e2` hoặc whitespace | `typeInvalid`, không coerce |
| CV13-03 | Float NaN/Infinity/comma locale | `typeInvalid` |
| CV13-04 | SrsMinEf > SrsMaxEf | `crossRuleInvalid`; reject whole set |
| CV13-05 | DefaultEf ngoài min/max | Reject whole SRS set |
| CV13-06 | InitialInterval2 > MasteredInterval | Reject whole SRS set |
| CV13-07 | Chỉ gửi một SRS key | Reject missing atomic-set members/version plan |
| CV13-08 | WordsPerSession=0/31 | `rangeInvalid`; không fallback 5 |
| CV13-09 | XpReward=10.001 | `rangeInvalid` + M06 owner reject |
| CV13-10 | Candidate ReviewBaseAP | `configDeprecated` |
| CV13-11 | AllowRegistration=true, auth dependency unknown | `dependencyUnknown`, fail-closed |
| CV13-12 | AllowGoogleLogin=true, OAuth/state/replay evidence stale | Invalid; không enable |
| CV13-13 | MaxGroupSize=1/501 | `rangeInvalid` |
| CV13-14 | Hạ MaxGroupSize dưới group hiện có | Impact/remediation missing → reject |
| CV13-15 | LogRetentionDays=7/0/91 | `rangeInvalid`; cleanup không delete |
| CV13-16 | URL HTTP/private IP/userinfo/javascript | `patternInvalid`/`exposureViolation` |
| CV13-17 | ContactEmail chứa newline | `patternInvalid` |
| CV13-18 | Public candidate chứa AdminAppLogo/AllowRegistration | `exposureViolation` |
| CV13-19 | Validator timeout/null | `unknown`; không pass/retry mù |
| CV13-20 | Consumer version đổi sau validation | `evidenceStale`; revalidate |
| CV13-21 | Candidate dài 800 URL khi DB ceiling 500 | `storageIncompatible` trước persist |
| CV13-22 | Batch 10 key có 1 lỗi | Reject toàn candidate, không apply 9 key |
| CV13-23 | Same validation key, khác digest | Conflict, không trả result cũ |
| CV13-24 | Error chứa secret-like raw input | Redact/drop raw field; stable safe code only |

## 11. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| M11-CV-I01 | Service có type/range validation và một số SRS cross-rule | Baseline tốt nhưng chỉ phủ metadata hiện có/SRS, chưa registry phase đầy đủ | M11-T014; M11-T049 |
| M11-CV-I02 | Generic create DTO không nhận min/max/live metadata; cho arbitrary key/category | Key mới thiếu scalar/effect/exposure rule nhưng vẫn persist được | M11-T014; M11-T049 |
| M11-CV-I03 | Generic update cho đổi value/description/category trực tiếp | Category/exposure có thể đổi không version/impact/public review | M11-T014–T017; M11-T049 |
| M11-CV-I04 | Learning consumer dùng hardcoded fallback và sửa invalid WordsPerSession về 5 | Invalid/missing có thể thành success với policy ngầm | M03/M04/M06 tasks; M11-T049 |
| M11-CV-I05 | GENERAL anonymous exposure dựa category | Không field exposure validator/allowlist | M11-T033; M11-T049 |
| M11-CV-I06 | MaxGroupSize/LogRetentionDays metadata thiếu range | Unsafe values parse được; cleanup có thể xóa quá mức | M09; M11-T035; M11-T049 |
| M11-CV-I07 | Không thấy validator result/version/freshness/consumer compatibility record | Không bind evidence vào revision/execution | M11-T014–T017; M11-T049 |
| M11-CV-F02 | Preview/simulation/consumer compatibility implementations | CV result + exact policy version | M11-T015–T017; module tasks |
| M11-CV-F03 | Public/redaction/audit runtime controls | Field allowlist, safe errors, audit fail-closed | M11-T031–T035; M11-T049 |
| M11-CV-F04 | Runtime DB drift/current-value validation | Không thu secret/sensitive value vào docs | M11-T049; REL-03 |
| M11-CV-F05 | Owner module test vectors và adapters | Owner reject/unknown giữ nguyên | M03/M04/M06/M09/M01/M12 tasks |

## 12. Tự kiểm M11-T013

- 29 key được bao phủ bởi canonical type/scalar/lifecycle/exposure rule; bảy policy set có 13 cross-rule.
- Bổ sung range target CFG-018 = 2–500 và CFG-029 = 30–90 operational days; seed 7 được ghi legacy-invalid, không tự sửa.
- Pipeline CV-01–CV-12 và 15 error code phân biệt reject/unknown/stale/storage/exposure mà không echo raw value.
- 10 gate và 24 case bao phủ numeric/canonical/SRS/batch/economy/AP/access/community/retention/public/data-boundary/freshness.
- Bảy sai lệch + bốn finding còn mở có task tiếp nhận; immutable version design đã chốt. A-G02 vẫn chờ preview/execution/audit/runtime evidence; không kết luận gate đạt.

## 13. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt validation pipeline, scalar/cross/dependency/exposure rules, taxonomy, gate và case | WSA-7K2 |
