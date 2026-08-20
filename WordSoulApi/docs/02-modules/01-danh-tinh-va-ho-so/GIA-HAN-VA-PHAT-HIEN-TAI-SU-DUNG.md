# Gia hạn phiên và phát hiện tái sử dụng M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T017 |
| Contract ID / phiên bản | M01-REFRESH-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-017, D-027–D-029; M01-SESSION-1.0; M01-INACTIVE-1.0; M12-CONTRACT-1.0; M12-RESULT-1.0 |
| Phạm vi | Refresh request/rotation, idempotency, concurrent use, reuse detection, compromise response và lost response |
| Ngoài phạm vi | Logout/session management UI T018; recovery T019; transport SDK implementation; global incident playbook |

## 1. Bất biến

- Refresh credential one-time: một generation chỉ có đúng một successor. CAS/unique constraint là truth, không process lock/cache.
- Request không nhận user/account ID. Opaque token gồm random selector + secret; selector chỉ tìm record, secret được constant-time verify với keyed digest.
- Rotation giữ family ID, tăng generation, không kéo `absoluteExpiresAt`; idle expiry tính lại nhưng bị clamp bởi absolute expiry/current policy.
- Same operation retry có thể trả lại **đúng successor đã commit** trong escrow ngắn; different operation dùng generation đã consumed là reuse, không phải retry.
- Effect/commit unknown không phát token mới, không chấp nhận token cũ và không tạo successor khác; reconcile bằng operation ID.
- Reuse response fail-closed, không trả chi tiết family/device/account existence. Access hiện tại của family bị chặn server-side ngay.
- Raw token không vào log/audit/URL/DB. Escrow nếu cần chứa response **mã hóa**, TTL ngắn, key/purpose riêng và không là session truth.

## 2. Request và response

### Request

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-REFRESH-1.0`; unsupported reject trước token lookup |
| `refreshToken` | Có | Opaque `selector.secret`, ≥256-bit random secret; transport/body or protected cookie, never query/header log |
| `operationId` | Có | Random opaque ≥128 bit do trusted client SDK tạo, ổn định qua retry của cùng refresh intent |
| `clientContext` | Có | Channel/app version + protected device binding evidence nếu có; không user ID/trusted flag |
| `correlationId` | Có | Opaque trace ID; không idempotency/business truth |

Unknown field, malformed/oversized token hoặc operation ID bị từ chối trước expensive lookup; response không echo input.

### Success response

| Field | Quy tắc |
|---|---|
| `accessToken` | Access mới theo class/current state; TTL từ M01-SESSION-1.0 |
| `refreshToken` | Successor raw secret chỉ trả qua protected response/escrow; generation cũ không còn hợp lệ |
| `accessExpiresAt`, `refreshIdleExpiresAt`, `familyAbsoluteExpiresAt` | UTC server timestamps; client không tự tính policy |
| `sessionId`, `familyId`, `sessionClass`, `scope` | Opaque/minimal metadata cho quản lý; không subject/email/role payload thừa |
| `rotationGeneration` | Monotonic non-secret metadata; không dùng thay token |

Public failures dùng một envelope `REFRESH_UNAVAILABLE` cho malformed/not found/wrong secret/expired/revoked/reused khi cần tránh oracle. Authenticated session-management context có thể nhận action `login_again`/`review_sessions`, không nhận raw reason/digest.

## 3. Rotation transaction

1. Validate transport/schema/version/rate/client context và parse selector an toàn.
2. Lookup refresh record bằng selector; fake/not-found path dùng timing class phù hợp, không query subject từ request.
3. Verify keyed digest constant-time; load family + account state/security/policy/restriction/device snapshot in transaction/consistent boundary.
4. Reject if family not Active/StepUp-compatible, credential expired/revoked, idle/absolute expiry passed, account/state/policy disallows, or signing/audit/session store unhealthy.
5. Check idempotency record `(familyId, refreshId, operationId, fingerprint)`:
   - committed same fingerprint and escrow valid → return same encrypted response;
   - same operation different fingerprint → conflict + family security event;
   - no record → proceed CAS.
6. CAS `usedAt == null && version == expected`; create exactly one successor generation/digest, new access/session metadata, audit and encrypted response escrow atomically.
7. Commit before returning. Escrow TTL 60 giây, bound to operation/family/client class; delete/expire automatically and never extend family/token TTL.
8. If response loss, same operation within 60 giây decrypts same response. After escrow expiry, do not recreate raw successor; require login/re-auth while keeping family state safe.

Escrow encryption key is a dedicated workload secret with owner/rotation/audit. Ciphertext is excluded from generic backup/log/evidence where possible; access is only refresh response path. If compliant escrow cannot be provided, safe baseline is no replay response and user logs in again—never deterministic token derivation or storing raw token.

## 4. Phân loại duplicate/reuse

| Tình huống | Phân loại | Hành vi |
|---|---|---|
| Same token + same operation/fingerprint, concurrent | Idempotent duplicate | Một CAS/response; loser waits/reads committed escrow; no alert/revoke |
| Same token + same operation/fingerprint, response lost ≤60s | Idempotent retry | Same successor response; no new generation |
| Same operation, different fingerprint/client contract | Idempotency conflict | No token; mark suspicious, revoke affected family unless proven server bug |
| Same consumed token + different operation | Reuse suspected | Revoke/Compromised affected family immediately; alert/audit; no successor |
| Same consumed token after escrow expiry, same operation | Indeterminate expired retry | No token; family remains active with successor but client must login again; audit category, not automatically theft |
| Two different operations race before consume | One rotate, other reuse | Safety wins: affected family Compromised/revoked; one issued access is denied by family status |
| Revoked/expired token reused | Invalid/reuse telemetry | No token; state remains monotonic; aggregate alert by policy |
| Token from deleted/old account generation | Invalid identity generation | No token/relink; protected security event |

Server/network retry must preserve operation ID. Client mở nhiều tab/process phải có single-flight/coordination; thiếu coordination không được server nới one-time semantics.

## 5. Reuse response theo session class

| Family class | Immediate action | Other families | User/operations |
|---|---|---|---|
| Learner limited/full | Mark affected family `Compromised`, revoke all generations/access by family index | Giữ family khác nếu không có correlated signal; risk episode Elevated và current-state check | Security inbox/email one intent; prompt login + review/revoke all |
| Admin/support | Revoke all admin/support families của subject, tăng admin security epoch | Learner families giữ/chặn theo security incident assessment; sensitive action denied pending review | Operational high alert + mandatory step-up/recovery; audit/case |
| Multiple family reuse/correlated compromise | Revoke all subject families, increase security epoch | None active until recovery | High security alert/incident; M01-T019/T020 |
| Server/storage inconsistency suspected | Isolate issuance/refresh capability, preserve evidence | Do not mass-revoke until scoped, but protected operations fail-closed | Operational critical; reconcile store/audit before recovery |

Failed refresh/reuse alone does not admin-lock or delete account. Escalation to lifecycle risk lock requires confirmed rule/owner/audit per M01-INACTIVE-1.0.

## 6. Expiry, state và scope

- Access TTL follows current session class; successful refresh re-evaluates class/scope. It may reduce scope or reject, never silently widen because prior token had more claims.
- Pending verification becoming Active requires state transition and new/full session issuance path; refresh limited family does not auto-elevate scope.
- Active becoming pending/locked/inactive/deleting/deleted causes refresh reject/revoke; old access denied via current state/security epoch.
- Restriction/role/policy/consent changes are checked each rotation. Stale admin permission cannot survive refresh; unknown policy fails closed.
- Idle expiry uses last committed rotation; failed/duplicate/reuse requests do not extend it. Absolute expiry immutable from original login.
- Clock skew policy applies only validation tolerance, never extends stored absolute expiry.

## 7. Audit, metric và alert

| Event | Metadata allowlist |
|---|---|
| `session.refresh.requested` | Protected family/refresh selector ref, operation/correlation, client class, generation |
| `session.refresh.rotated` | Family, old/new generation refs, expiry class, policy/security/state versions |
| `session.refresh.idempotent_replay` | Family/operation, escrow age bucket, result |
| `session.refresh.reuse_detected` | Family/class, generation, reason category, coarse source/device match, response scope |
| `session.family.compromised_or_revoked` | Family/subject protected refs, scope, reason, security epoch version |
| `session.refresh.dependency_failed` | Dependency/category, operation, commit-known/unknown, latency class |

Cấm raw selector/secret/token, email/name, raw IP/user-agent/device, response escrow/ciphertext, request body và exception/provider payload. Metric: rotation success/failure/reuse/idempotent/lost-response, CAS conflict, escrow hit/expiry/decrypt failure, family revoke scope, refresh latency, store/audit/signing health; no high-cardinality IDs as labels.

Reuse alert dedup theo family + generation + incident version; notification failure giữ durable alert pending, không restore family hoặc retry token.

## 8. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| Refresh/family/account/audit store unavailable | No rotation/token; generic 503/unknown; old token not marked safe for retry until reconcile |
| CAS outcome unknown | Query by operation/refresh ID; no second CAS/new successor |
| Escrow encrypt/commit failure | Rotation transaction abort; no token; if commit uncertain reconcile |
| Escrow decrypt/expired | No reconstructed/deterministic token; login again; family/successor state remains |
| Signing key/config unavailable | No access token/rotation commit unless transaction can abort fully |
| Revocation index unavailable after reuse | Durable family Compromised commit + protected APIs fail-closed; incident alert |
| Notification unavailable | Reuse/revoke remains; alert outbox pending |
| Clock/store-time anomaly | Stop refresh issuance; no expiry extension |

## 9. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát | Sai lệch / rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-REF-I01 | Refresh request nhận `Id` + raw token | Caller supplies subject lookup; oracle/IDOR boundary and no selector contract | M01-T042-A |
| M01-REF-I02 | Raw refresh token lưu/so sánh trực tiếp trên `User` | Secret at rest, no family/generation/digest/history | M01-T018, T042-A |
| M01-REF-I03 | Mỗi refresh gọi create response và ghi đè token field | Không CAS/idempotency/reuse detection; concurrent rotations race | M01-T042-A |
| M01-REF-I04 | Refresh chỉ kiểm token equality + expiry | Không account state/security/policy/restriction/family/device check | M01-T018, T031, T042-A |
| M01-REF-I05 | Access token 1 ngày và claims name/id/role | Reuse response blast radius lớn, stale claims/current-state gap | M01-T018, T042-A |
| M01-REF-I06 | Không thấy failed/reuse/rotation audit hoặc user alert | Không điều tra/response được theft/concurrency | M01-T038–T039, T042-A |

I01–I06 là release blocker; không có runtime evidence cho rotation/reuse hiện hành.

## 10. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| RF17-01 | Refresh hợp lệ | One CAS successor, old used, audit commit trước response |
| RF17-02 | Same operation concurrent | Một successor, cùng escrow response, no revoke |
| RF17-03 | Lost response, retry same op ≤60s | Exact same successor response, no generation mới |
| RF17-04 | Same op khác fingerprint | Conflict, no token, security response |
| RF17-05 | Consumed token different op | Affected family Compromised/revoked, no token, alert |
| RF17-06 | Same op after escrow expiry | No token/reconstruction; login again; không tự theft revoke |
| RF17-07 | Two different ops race | One rotate then family revoked; issued access denied by family status |
| RF17-08 | Learner family reuse | Revoke affected only + Elevated risk, other family preserved absent correlation |
| RF17-09 | Admin family reuse | All admin families revoke + high alert/step-up |
| RF17-10 | Correlated multi-family reuse | All families/security epoch revoke; recovery required |
| RF17-11 | Idle/absolute expiry | Reject; failed request không extend; absolute never slides |
| RF17-12 | State/role/restriction changes | No stale/elevated scope; reject/reduce/reissue safely |
| RF17-13 | Limited family after verification | Không auto-full scope qua refresh |
| RF17-14 | Store/CAS outcome unknown | Reconcile same operation; no second successor/token |
| RF17-15 | Escrow/key/decrypt failure | No raw/deterministic fallback; login again/503 |
| RF17-16 | Revocation cache failure on reuse | Durable compromised state + protected API fail-closed |
| RF17-17 | Malformed/nonexistent/wrong/revoked sweep | Generic response/timing class; no subject oracle |
| RF17-18 | Log/audit/metric scan | No raw token/selector/PII/escrow/high-cardinality labels |

## 11. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-REF-F01 | Logout/session-list API consuming family model | Revoke remains monotonic/exact; no raw metadata | M01-T018 |
| M01-REF-F02 | Recovery/change credential after confirmed reuse | Reuse response matrix applies; no family restore | M01-T019–T020 |
| M01-REF-F03 | Risk/user/operational alert integration | Durable dedup intent, state unaffected by delivery | M01-T038–T039 |
| M01-REF-F04 | Store/escrow/key schema and runtime tests | No raw token; no release on current single-token design | M01-T042-A; M12-T040–T041; REL-03 |

## 12. Tự kiểm M01-T017 và A-G01

- Request không nhận user ID; token selector/secret, keyed digest, CAS generation và family/current-state validation đã chốt.
- Same-operation retry dùng encrypted escrow 60 giây; different operation reuse revoke an toàn; expired escrow không tái tạo secret.
- Learner/admin/correlated/server-inconsistency reuse có blast-radius response riêng; failed refresh không tự admin-lock/delete.
- Idle/absolute TTL/scope/current state không bị refresh kéo dài/nâng quyền; limited family không auto-full.
- 18 case bao phủ concurrency, lost response, reuse, expiry, state, failure, oracle/redaction; A-G01 có baseline nhưng chưa runtime.
- 6 sai lệch và 4 finding mở có task tiếp nhận; không kết luận A-G01 đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt refresh contract, one-time CAS, escrow/idempotency, reuse response và 18 case | WSA-7K2 |
