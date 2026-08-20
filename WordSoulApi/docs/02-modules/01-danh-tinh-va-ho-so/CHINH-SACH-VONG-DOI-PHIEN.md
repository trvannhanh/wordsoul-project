# Chính sách vòng đời phiên M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T016 |
| Policy ID / phiên bản | M01-SESSION-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012, D-017, D-027–D-028; M01 lifecycle 1.0; M01-LOGIN-1.0; M01-INACTIVE-1.0; M01-D007–D009, D015, D024 |
| Phạm vi | Session family, access/refresh lifetime, multiple device, claims, rotation/revocation, re-auth và state enforcement |
| Ngoài phạm vi | Reuse algorithm/response chi tiết T017; logout UI/API T018; recovery/change credential T019–T020; device notification endpoint T026–T027 |

## 1. Bất biến

- Mỗi login thành công tạo một **session family** riêng; không lưu một refresh token chung trên `User` và không ghi đè session của thiết bị khác.
- Access token ngắn hạn chỉ là bằng chứng đã ký, không là current account/role/restriction truth. Session store + account security epoch/state/version quyết quyền hiện hành.
- Refresh credential là opaque random secret, chỉ lưu keyed hash/digest; one-time rotation trong transaction. Raw refresh/access token không vào DB, log, URL, analytics hoặc audit.
- Token chỉ được trả sau session family/refresh record và audit bắt buộc commit. Commit outcome unknown → không phát lại mù; reconcile theo login operation.
- Client/device ID tự khai không là trust proof. Session bound tới protected device installation ref khi đã đăng ký; unknown/new device vẫn là session riêng và phát security alert.
- Logout/revoke/lock/delete/security change có hiệu lực server-side; không chờ access token tự hết hạn để bảo vệ C0/sensitive API.
- Expiry dùng UTC server time và mốc tuyệt đối. Refresh không kéo family qua absolute expiry hoặc scope/state hiện hành.

## 2. Loại phiên và thời hạn baseline

| Session class | Access TTL | Refresh idle TTL | Absolute family TTL | Max active family | Re-auth freshness | Scope |
|---|---:|---:|---:|---:|---:|---|
| Learner full | 15 phút | 7 ngày từ lần rotation hợp lệ | 30 ngày từ login | 10 / subject | 10 phút cho thao tác nhạy cảm | Theo role/state/restriction/policy current |
| Learner limited (`Chờ xác minh thư`) | 10 phút | 24 giờ | 7 ngày | 3 / subject | Mọi sensitive action bị cấm; verification dùng proof riêng | Read learning + limited learning + verify/logout only |
| Admin/support | 10 phút | 30 phút | 8 giờ | 3 / subject | 5 phút + action/case scope cho critical mutation | Least privilege role/action; no learner-token elevation |
| Recovery/action ticket | Không phải access token | Không refresh | 10 phút ticket; proof owner có TTL riêng | Theo intent/quota | Mỗi submit kiểm proof/state lại | Một purpose/audience endpoint allowlist |

- TTL là trần v1.0. Config có thể ngắn hơn theo environment/risk, không dài hơn hoặc đổi semantics nếu chưa có policy version/decision mới.
- Idle TTL tính từ rotation hợp lệ gần nhất; absolute TTL không trượt. Đến absolute expiry phải login/re-auth mới tạo family mới.
- Khi đạt max family, không tự revoke family cũ. Sau credential/risk gate trả action quản lý phiên; user revoke family đã chọn hoặc all. Điều này tránh attacker dùng login để đẩy session hợp lệ ra ngoài.
- New/unknown device có thể yêu cầu step-up theo risk policy và luôn tạo alert sau session commit; không tự đánh dấu trusted chỉ vì login thành công.

## 3. Mô hình dữ liệu tối thiểu

### Session family

| Field | Quy tắc |
|---|---|
| `familyId`, `sessionId` | Opaque immutable IDs; family định danh chuỗi refresh, session/current login context; không chứa subject/device |
| `subjectRef`, `accountGeneration` | Protected immutable ref; ngăn relink sau delete/re-register |
| `deviceRef?`, `clientClass`, `authMethod` | Server-derived/validated allowlist; không raw user-agent/provider token |
| `sessionClass`, `scopeSet`, `roleVersion` | Limited/full/admin + scope allowlist; role source/version, không free-form claims |
| `createdAt`, `lastRotatedAt`, `idleExpiresAt`, `absoluteExpiresAt` | UTC server time; absolute immutable |
| `authTime`, `stepUpTime?`, `authStrength` | Dùng freshness; không client timestamp |
| `accountStateVersion`, `securityEpoch`, `policyVersion`, `restrictionVersion` | Snapshot để phát hiện stale; current store vẫn là truth |
| `status`, `revokedAt?`, `revokeReason?` | Active, StepUpRequired, Revoked, Expired, Compromised; transition monotonic/audited |
| `version` | Concurrency token cho rotate/revoke/state update |

### Refresh record

| Field | Quy tắc |
|---|---|
| `refreshId`, `familyId`, `generation` | Opaque ID + số tăng đơn điệu; unique trong family |
| `secretDigest`, `digestKeyVersion` | HMAC/keyed digest; constant-time compare; key trong secret store |
| `issuedAt`, `idleExpiresAt`, `usedAt?` | UTC; one-time use |
| `replacedById?` | Liên kết rotation; không chứa raw token |
| `revokedAt?`, `reason?` | Revoke individual/family/all; history bất biến |
| `operationId`, `version` | Idempotency/CAS cho refresh transaction |

Không cascade xóa history khi revoke; retention/privacy theo M01-T033/M11, raw credential không bao giờ giữ làm evidence.

## 4. Access token claims allowlist

| Claim | Yêu cầu |
|---|---|
| `iss`, `aud`, `iat`, `nbf`, `exp`, `jti` | Chuẩn validation; issuer/audience exact allowlist, clock skew nhỏ có cấu hình |
| `sub` | Protected subject ID; không email/username/display name |
| `sid`, `fid` | Session/family IDs phục vụ revoke/current-state check |
| `scope`, `session_class` | Allowlist từ server; limited/full/admin không được client chọn |
| `auth_time`, `amr` | Server auth time/method class; không provider raw metadata |
| `account_generation`, `security_epoch` | Phát hiện delete/re-register/security change |
| `state_version`, `role_version`, `policy_version`, `restriction_version` | Snapshot; API vẫn kiểm current version cho protected operation |

Cấm email, password/verifier, refresh token, provider token/subject, raw device/IP/user-agent, consent payload, support case content, full profile và permission list động. Role có thể là compact server-derived reference/version; authorization lấy current policy, không tin role claim stale cho admin/C0 mutation.

## 5. Phát hành và rotation boundary

1. Login/action policy xác định session class/scope và current account/security/policy/restriction snapshot.
2. Kiểm max families, risk/device step-up và signing/refresh secret configuration. Unknown/missing → fail-closed.
3. Tạo family + refresh generation 0 digest + audit trong transaction/outbox nhất quán. Access token chỉ ký sau durable commit; response chỉ chứa raw refresh một lần.
4. Refresh request xác minh digest, family/status, one-time state, idle/absolute TTL, current account/state/security/policy/restriction và client binding.
5. Rotation CAS đánh dấu current `usedAt`, tạo generation mới/digest/expiry và audit atomically. Chỉ sau commit mới trả access + raw refresh mới.
6. Response bị mất sau commit → retry cùng operation ID trả result/reconcile an toàn; không tạo generation mới vô hạn hoặc chấp nhận token cũ lần hai.
7. Reuse/duplicate/concurrency semantics chi tiết T017; baseline là một generation chỉ được tiêu thụ một lần và uncertainty không cấp token.

## 6. Current-state enforcement

| API class | Kiểm tra bắt buộc |
|---|---|
| Public | Không session; rate/abuse policy riêng |
| Limited learning read/write | Signature/expiry + current session/family active + state vẫn pending verification + exact limited scope |
| Normal authenticated read | Signature/expiry + session/security epoch cache có freshness; cache failure dùng durable lookup/conservative deny theo criticality |
| Sensitive account/asset/export/social/PvP/mutation | Current family, account state, security epoch, policy/consent, restriction và role/ownership version; re-auth freshness |
| Admin/support | Mọi kiểm tra sensitive + current role/permission/case scope + 5-minute re-auth + audit availability |
| Refresh | Luôn durable/CAS check đầy đủ; không chỉ validate access token/subject ID |

State/lock/delete/revoke propagation dùng server-side session index/security epoch. Nếu control store unavailable, C0/sensitive/admin fail-closed; limited/normal read chỉ conservative theo policy đã đăng ký, không allow-all từ token còn hạn.

## 7. Revocation matrix

| Trigger | Phạm vi revoke/chuyển | Existing access behavior | Thông báo/audit |
|---|---|---|---|
| Logout current | Current family | Bị chặn bằng family status/index; no refresh | Audit family; notification optional |
| Logout selected device/family | Selected family | Selected token bị chặn | Audit actor/target protected refs |
| Logout all | All families subject | Mọi token/session bị chặn | Security notification + audit |
| Password change trong safe current session | Mọi family khác; giữ current sau re-auth và rotate | Other access blocked; current gets new security snapshot | Mandatory security alert/audit |
| Password reset/recovery | All families, tăng security epoch | Mọi access blocked; login lại | Mandatory alert/audit; D-009 |
| Confirmed compromise/reuse | Family liên quan hoặc all theo blast radius; mark Compromised | Immediate deny, no refresh | High alert/incident/audit |
| Lifecycle risk lock/admin lock/inactive/delete | All hoặc exact lock scope; security/state version update | Current-state check denies; no refresh | State transition alert/audit |
| Restriction/policy/consent change | Recompute scope; revoke/reissue affected family, never widen old token | Affected operation denied immediately | Audit before/after; user notice per policy |
| Role/permission change | Admin families revoke/re-auth; learner scope re-evaluate | Stale admin role cannot mutate | Mandatory admin audit/alert |
| Unlink auth method | Families authenticated solely by removed method re-evaluate/revoke; never relink by email | No access if no valid method/scope | Security alert/audit |

Revocation is idempotent and monotonic. “Already revoked/expired” returns success-equivalent state without changing reason history; concurrent refresh/revoke uses CAS, safety transition wins.

## 8. Multiple device và client storage

- Mỗi app installation/browser profile có protected device ref riêng sau registration; session family vẫn tồn tại khi device metadata absent, nhưng được đánh new/unknown và không nhận trust privileges.
- Web refresh credential dùng `Secure`, `HttpOnly`, appropriate `SameSite` cookie và CSRF protection; access token ở memory. Mobile dùng OS secure storage. Cấm localStorage/plain preferences/database/log/backup không bảo vệ.
- Session list hiển thị device label đã sanitize, client class, coarse last activity, created/last-used time và current marker; không raw IP/user-agent/location.
- User được revoke one/all; không được xem raw token, digest, exact protected device ID hoặc session của subject khác.
- FCM/push device endpoint có lifecycle riêng T026–T027; revoke session không mặc nhiên xóa mọi device record, nhưng logout/revoke phải ngừng security-sensitive delivery theo binding policy.

## 9. Failure mode và signing keys

| Failure | Hành vi bắt buộc |
|---|---|
| Session/refresh/audit store lỗi | Không token/rotation/revoke success giả; 503/unknown + reconcile |
| Current account/policy/restriction unknown | Không issue/refresh/elevate; protected API deny |
| Signing key missing/invalid/expired | Không issue; validation chỉ chấp nhận active/grace key IDs theo rotation policy; alert critical |
| Revocation index/cache lỗi | Durable lookup or fail-closed theo API criticality; không tin token alone |
| Concurrent refresh | Một CAS thắng; lượt còn lại đi T017 reuse/benign-race decision, không hai successors |
| Response lost after rotation commit | Same operation reconcile/result; old token không được dùng lại để tạo generation khác |
| Notification lỗi | Session/revoke state vẫn hiệu lực; durable alert pending |
| Clock anomaly | Không nới expiry; isolate issuance/refresh nếu server/store time không đáng tin |

Signing/refresh HMAC keys thuộc M12-T040–T041: owner, environment, purpose, `kid`/version, rotation/revocation và audit; không hardcode/config plaintext/Git/log.

## 10. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát | Sai lệch / rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-SESS-I01 | `User` lưu một raw `RefreshToken` + expiry | Không family/device/history/digest/rotation/revoke scope; login mới ghi đè token cũ | M01-T017–T018, T042-A |
| M01-SESS-I02 | Access JWT sống 1 ngày, chỉ có name/id/role | Vượt TTL v1.0, chứa display identity, thiếu sid/fid/security/state/policy/restriction versions | M01-T017, T042-A |
| M01-SESS-I03 | Refresh nhận user ID + token, so sánh chuỗi và gọi `CreateTokenResponse` | Không one-time CAS/reuse detection; rotate ghi đè; không current state/security check | M01-T017, T042-A |
| M01-SESS-I04 | Login/Google login không kiểm lifecycle và chạy daily quest side effect | Session có thể cấp sai state; auth gây business mutation | M01-T017; M07-T022–T025; M01-T042-A |
| M01-SESS-I05 | Ban chỉ null một refresh field; access JWT vẫn còn hạn | Revocation không immediate và không bao phủ family/device/access | M01-T018, T031, T042-A |
| M01-SESS-I06 | Secret ký đọc từ config; chưa thấy key registry/rotation/readiness | Missing/weak/stale key lifecycle chưa fail-safe hoặc audit | M12-T040–T041; M11-T036; REL-03 |
| M01-SESS-I07 | Token DTO chỉ trả hai raw string, không contract/session metadata | Client không biết expiry/class/scope/family/current device hoặc rotation semantics | M01-T017–T018, T042-A |

I01–I07 là release blocker; tài liệu không thay source/runtime/secret evidence.

## 11. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| SS16-01 | Learner login eligible | Family riêng, access 15m, idle 7d, absolute 30d; commit/audit trước token |
| SS16-02 | Pending verification login | Limited access 10m, idle 24h, absolute 7d; exact scope |
| SS16-03 | Admin login | Access 10m, idle 30m, absolute 8h, re-auth 5m, current permission check |
| SS16-04 | 11th learner/4th admin family | Không auto-evict; manage/revoke action sau proof |
| SS16-05 | Two devices login | Hai family độc lập; token/rotation/revoke không ghi đè nhau |
| SS16-06 | Refresh hợp lệ | One CAS successor, old marked used, TTL không vượt absolute |
| SS16-07 | Concurrent refresh same generation | Không hai successor; outcome theo T017, no duplicate token family |
| SS16-08 | Lost response after rotation | Reconcile same operation; old credential không tạo successor mới |
| SS16-09 | Logout selected/all | Exact family/all deny immediate server-side; idempotent audit |
| SS16-10 | Safe password change | Current kept/re-authenticated; all other families revoked |
| SS16-11 | Reset/confirmed compromise | All family revoke + security epoch; login lại |
| SS16-12 | Account lock/delete after token issue | Protected APIs/refresh deny before token expiry |
| SS16-13 | Restriction/role/policy change | No stale scope elevation; affected family re-evaluate/revoke |
| SS16-14 | Session/audit commit fails | No token/cookie/raw refresh returned |
| SS16-15 | Revocation cache unavailable | Durable lookup/FC by criticality; no token-only allow |
| SS16-16 | Token claim/redaction scan | No email/name/raw device/provider/permission payload/secret |
| SS16-17 | Client storage/browser CSRF | Refresh not script-readable; cookie/CSRF attributes enforced; no localStorage |
| SS16-18 | Signing key/config unhealthy | No issue/unsafe validation; alert/readiness degraded |
| SS16-19 | Absolute expiry reached despite activity | Refresh denied; new login required |
| SS16-20 | Deleted identity re-registers | New account generation/families; old token/family cannot relink |

## 12. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-SESS-F02 | Logout/session-list/device UX và notification binding | Revoke exact family/all, no raw metadata | M01-T018, T026-A–T027-A |
| M01-SESS-F04 | Store/schema/key runtime implementation/evidence | Current single-token design không release-ready | M01-T042-A; M12-T040–T041; REL-03 |

## 13. Tự kiểm M01-T016, A-G01 và A-G02

- Bốn session class có access/idle/absolute TTL, max families, re-auth freshness và scope; learner family không vượt 30 ngày.
- Session family/refresh record/claim allowlist tách raw secret khỏi DB/log, giữ account generation/security/state/policy/restriction versions và CAS.
- Rotation/revocation matrix bao phủ logout, password change/reset, compromise, state/delete, restriction/role và unlink; access không sống vượt current state.
- Multi-device không auto-evict; web/mobile storage, new-device alert và session-list privacy đã chốt.
- 20 case bao phủ TTL, multiple device, concurrency/lost response, revoke/state race, claims/storage/key failure; A-G01/A-G02 có baseline nhưng chưa runtime.
- 7 sai lệch và 2 finding mở có task tiếp nhận; M01-SESS-F01 đã được đóng bởi M01-REFRESH-1.0, M01-SESS-F03 bởi M01-RECOVERY-1.0/M01-SEC-CHANGE-1.0; không kết luận gate đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt session classes/TTL, family/refresh/claims, multi-device/revocation và 20 case | WSA-7K2 |
