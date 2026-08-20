# Thay đổi thông tin bảo mật M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T020 |
| Contract ID / phiên bản | M01-SEC-CHANGE-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012, D-024, D-028, D-030–D-031; M01-CRED-1.0; M01-SESSION-1.0; M01-RECOVERY-1.0; M01-VER-1.0; M12-RATE-1.0; M12-FAIL-1.0 |
| Phạm vi | Đổi chủ động mật khẩu đăng nhập trực tiếp và email đăng nhập/khôi phục, re-auth, proof, session impact, concurrency, audit và cảnh báo |
| Ngoài phạm vi | Quên/reset T019; mất mọi kênh T021; link/unlink provider T014–T015; profile/display name T022–T023; admin/support mutation |

## 1. Bất biến

- Chỉ subject của một session family Active, không limited, đủ scope và current-state/current-policy mới khởi tạo thay đổi; request không nhận user ID hoặc target subject.
- Access token/thiết bị được nhận biết không đủ. Phải có re-auth purpose-bound còn mới tối đa 5 phút; password account dùng current password, external-only dùng provider re-auth contract đã xác minh.
- Password, code, provider token, verifier và request body không vào URL/log/audit/analytics. Proof chỉ lưu keyed verifier/digest + metadata tối thiểu.
- Mỗi security-change intent gắn subject, current family, purpose, credential/email/security/policy version, desired-value protected fingerprint và dùng một lần bằng CAS.
- Đổi mật khẩu không đổi email/role/state/provider. Đổi email không tự link/unlink provider, không đổi external provider email và không nối account chỉ vì địa chỉ trùng.
- Mọi security mutation, session action, audit và notification outbox phải commit atomically hoặc có outcome `unknown` để reconcile; không báo success giả.
- Admin/support không được đọc/nhập mật khẩu, code hoặc thay đổi email/mật khẩu thay user; họ chỉ có thể mở recovery/support workflow có quyền/case/audit riêng.

## 2. Lớp re-auth và intent

| Thành phần | Baseline M01-SEC-CHANGE-1.0 |
|---|---|
| Re-auth freshness | Tối đa 5 phút từ server verification; single-purpose, one-time/CAS cho commit |
| Password proof | Current password verifier đúng; rate/protected credential probe áp dụng; không dùng recovery code đã consume |
| External proof | Provider subject/state/nonce/code one-time theo contract ngoài; không chỉ email claim/cookie provider |
| Change intent TTL | 30 phút; re-auth phải còn mới tại commit, intent không kéo dài freshness |
| Verification code | 10 ký tự Crockford Base32, TTL 30 phút, tối đa 10 lần thử mỗi channel intent, body-only |
| Resend | Cooldown 60 giây, tối đa 5/24 giờ/recipient/purpose; generation mới revoke code cũ |
| Operation ID | Opaque random ≥128 bit; ổn định qua retry cùng mutation; metadata giữ tối thiểu 24 giờ |
| Version gate | Account/credential/email/security/policy/current-family version phải khớp lúc commit; mismatch bắt đầu lại |

Risk Elevated có thể yêu cầu proof bổ sung theo M01-ABUSE-1.0. Protected/Recovery-required, admin-locked, inactive, deleting/deleted hoặc policy unknown không dùng active-session change; chuyển đúng recovery/support path.

## 3. Đổi mật khẩu chủ động

### Request

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-SEC-CHANGE-1.0` |
| `currentPassword` | Có với direct credential | Exact string, không trim; chỉ dùng verifier, không log/echo |
| `newPassword` | Có | M01-CRED-1.0: 12–128 Unicode scalar, blocklist/breached, không trùng current |
| `operationId`, `clientContext`, `correlationId` | Có | Idempotency/binding/trace; không authority hoặc user ID client-supplied |

### Commit

1. Validate current full session, state/scope/security/policy/restriction và re-auth rate/risk.
2. Verify current password constant-time; failure dùng generic re-auth error và abuse counters, không tiết lộ verifier state.
3. Chỉ sau current proof đúng mới trả lỗi new-password policy có thể hành động; breached-policy dependency lỗi thì fail-closed.
4. CAS account/credential/current-family version: ghi verifier mới, tăng credential version; revoke mọi family khác; rotate current family refresh generation, tăng family access version; ghi audit + alert outbox.
5. Commit trước response. Trả replacement access/refresh cho đúng current family theo M01-SESSION/M01-REFRESH; token cũ của family bị chặn, family khác revoked. Không tạo family mới.

Same operation retry trả cùng committed protected response bằng escrow ngắn theo M01-REFRESH-1.0; không hash/rotate/revoke lần hai. Escrow hết hạn không tái tạo token: mutation vẫn thành công, client login lại. Outcome unknown phải reconcile operation trước retry.

## 4. Đổi email đăng nhập/khôi phục

### Khởi tạo

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-SEC-CHANGE-1.0` |
| `newEmail` | Có | Server canonicalization, size/schema; khác current canonical email |
| `reauthProof` | Có | Current password hoặc external re-auth đúng phương thức account |
| `operationId`, `clientContext`, `correlationId` | Có | Không subject/user ID; stable qua retry |

Sau re-auth, server kiểm uniqueness/reservation và eligibility nhưng trả generic `SECURITY_CHANGE_PENDING` khi request hợp lệ về schema. Nếu không thể reserve new canonical email, không dispatch proof và không tiết lộ account sở hữu. Reservation gắn intent, có TTL 30 phút, unique constraint là truth và tự giải phóng khi expire/revoke/consume.

Một intent hợp lệ tạo hai proof độc lập:

- `confirm-current-email`: gửi tới email verified hiện tại để xác nhận/rút yêu cầu.
- `confirm-new-email`: gửi tới địa chỉ mới để chứng minh quyền kiểm soát.

Mỗi proof có intent ID/code/generation riêng nhưng cùng change ID; code không nằm trong link. Retry/resend cùng channel giữ idempotency; generation mới chỉ revoke proof của channel đó. Đổi target email luôn revoke toàn bộ change cũ và tạo operation mới.

### Commit

1. Cả current-email proof và new-email proof phải consumed/verified trong cùng change window; re-auth freshness 5 phút phải được làm mới trước final commit nếu đã quá hạn.
2. Recheck state, current email/version, new email reservation/unique constraint, provider-link boundary, policy/security/current-family version.
3. CAS commit canonical email mới + verified metadata/version, tăng credential/security epoch, revoke mọi session family/refresh/access, revoke mọi verification/recovery/security-change intent cũ, audit + alert outbox cho email cũ và mới.
4. Trả `EMAIL_CHANGE_COMPLETED` + `login_again`, không token/session. Login direct sau đó chỉ dùng email mới; email cũ không còn là recovery channel.

Nếu current email không truy cập được, self-service change dừng và chuyển T021; new-email proof hoặc password/session riêng lẻ không thay thế proof kênh cũ. Thông báo tới email cũ ghi rõ thay đổi và đường báo cáo, nhưng không chứa email mới đầy đủ/code/token.

## 5. Hủy, hết hạn và đồng thời

| Tình huống | Kết quả bắt buộc |
|---|---|
| User hủy trước commit | CAS intent `Cancelled`, revoke proofs/reservation; không đổi security data/session |
| Current-email proof chọn deny | Revoke change/proofs/reservation; giữ account; high security alert/risk signal |
| Intent/proof/re-auth hết hạn | Không mutation; restart bằng version/current state mới |
| Hai password change khác operation | Một CAS thắng; loser stale-version, không ghi đè verifier |
| Password change và recovery race | Một credential-version CAS thắng; recovery nếu commit thì revoke all, active change stale |
| Password change và email change race | Security/account version quyết định; loser restart, không partial session/email |
| Hai email change khác target | Một reservation/change active theo policy; target mới revoke old hoặc conflict rõ sau re-auth |
| Email uniqueness thay đổi trước commit | Unique constraint/CAS reject; rollback mutation, revoke/rescope intent an toàn |
| Same operation retry | Cùng result/effect; không resend/revoke/notify lần hai |
| Commit outcome unknown | Reconcile operation; không tạo intent/mutation kế tiếp cho đến khi known |

## 6. Kết quả và session impact

| Hành trình | Success | Session | Failure public |
|---|---|---|---|
| Password change | `PASSWORD_CHANGE_COMPLETED` | Giữ/reissue current family; revoke all others | Re-auth generic trước proof; policy actionable sau proof; unknown không success |
| Email change initiate | `SECURITY_CHANGE_PENDING` | Current family giữ nhưng sensitive scope có thể step-up; intent không là session | Không lộ email target/account collision |
| Email change complete | `EMAIL_CHANGE_COMPLETED` | Revoke all; login lại bằng email mới | Generic expired/conflict/stale; no partial mutation |
| Cancel/deny | `SECURITY_CHANGE_CANCELLED` | Giữ family trừ khi risk response quyết revoke | Không lộ proof/channel detail ngoài authenticated intent |

`credentialVersion`, `securityEpoch` và family version là server truth. JWT claim cũ không giữ quyền; API nhạy cảm kiểm current state/version. Không chờ access token tự hết hạn để thực thi revoke.

## 7. Audit, cảnh báo và privacy

| Event | Metadata allowlist |
|---|---|
| `identity.security_change.requested` | Subject/current-family protected ref, purpose, operation, risk/state/policy version |
| `identity.security_change.reauth_succeeded_or_failed` | Proof class, result category, protected actor/session ref, attempt bucket |
| `identity.security_change.channel_proof` | Change/channel protected ref, generation, result category, expiry class |
| `identity.password.changed` | Subject ref, credential/family version before/after, revoke scope, operation |
| `identity.email.changed` | Subject ref, old/new protected email version refs, security epoch, operation |
| `identity.security_change.cancelled_or_conflicted` | Change ref, reason category, actor/channel class, version |
| `identity.security_change.dependency_failed` | Dependency, commit-known/unknown, operation, retry class |

Thông báo bắt buộc: password change tới verified safe channel; email change initiation/deny/completion tới email cũ và completion tới email mới. Dùng durable outbox/dedup; delivery lỗi không rollback committed security mutation. Security inbox chỉ hiển thị metadata đã che và không là kênh duy nhất khi mọi family đã revoke.

Cấm password/code/verifier/token, full old/new email, provider payload, raw IP/user-agent/device, request body và exception body. Metric: request/reauth/proof/commit/cancel/conflict theo purpose/result/channel class; re-auth age bucket; session revoke scope; dependency/unknown; alert backlog—không high-cardinality ID.

## 8. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| State/session/security/audit store unavailable | Không tạo proof/mutation; fail-closed, current session không được coi fresh |
| Limiter/risk store unavailable | Không re-auth hoặc dispatch proof mới; conservative generic response |
| Breached-password policy unavailable | Không đổi password/verifier; current session giữ theo state hiện tại |
| Email dispatch temporary/unknown | Retry cùng message ID trong deadline; không proof generation mới |
| Email dispatch permanent | Intent giữ/cancel theo policy; không đổi email hoặc báo verified |
| Reservation/unique store lỗi | Không dispatch/commit email change; không bypass uniqueness |
| CAS/commit outcome unknown | HOLD + reconcile same operation; không retry mutation mù |
| Token escrow/signing lỗi khi password commit | Abort atomically nếu có thể; nếu verifier commit đã chắc, giữ revoke và yêu cầu login lại, không trả token giả |
| Revocation propagation lỗi | Durable version/revoke commit; protected APIs fail-closed đến đối soát |
| Notification lỗi | Mutation/session action giữ nguyên; outbox pending + operational alert |

## 9. Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-CHG-I01 | Không tìm thấy endpoint/DTO/service đổi mật khẩu hoặc email | Không có re-auth, proof, CAS, session action hay alert | M01-T042-A |
| M01-CHG-I02 | `UpdateUserDto` chỉ có `Username`, `AvatarUrl`; profile endpoint nhận path user ID và cho admin sửa user khác | Chưa tách security mutation; nếu thêm field vào DTO sẽ tạo confused-deputy/IDOR risk | M01-T022, T041, T042-A |
| M01-CHG-I03 | `AuthService` dùng `PasswordHasher<User>` trực tiếp | Không policy/version/blocklist/breached/rehash service dùng chung | M01-T042-A |
| M01-CHG-I04 | `User` không có verified email version, credential/security epoch hoặc change intent | Không dual proof, stale-version/CAS, reservation hoặc current-token invalidation | M01-T033, T042-A |
| M01-CHG-I05 | Session hiện là raw refresh token đơn trên `User`, JWT access một ngày | Không giữ exact current family/revoke others hoặc revoke-all email change | M01-T018, T042-A |
| M01-CHG-I06 | Email service ad hoc subject/body, thiếu semantic result/idempotency quan sát | Không chứng minh dual-channel proof/alert delivery/reconcile/redaction | M12-T026–T030; REL-03 |
| M01-CHG-I07 | Không thấy audit/security event cho password/email change | Không điều tra actor, proof, before/after version, revoke hoặc alert | M01-T038–T041; M11-T031–T035 |

I01–I07 là release blocker; không có runtime evidence cho security change hiện hành.

## 10. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| SC20-01 | Password change, current proof + new policy valid | Atomic verifier/version; current family replacement; others revoked; alert |
| SC20-02 | Access token/device only, no recent proof | Re-auth required; no mutation |
| SC20-03 | Current password wrong/replayed | Generic reject, abuse counter; no new-policy/account oracle |
| SC20-04 | New password weak/current/breached | Actionable only after proof; verifier/session unchanged |
| SC20-05 | Password policy dependency down | Fail-closed; proof deadline unchanged, no consume/mutation |
| SC20-06 | Same password operation retry | One mutation/alert; same response while escrow valid, else login again |
| SC20-07 | Two password operations race | One CAS winner; loser stale, no last-write-wins |
| SC20-08 | Email change start valid | Reservation + two channel proofs; current email unchanged |
| SC20-09 | New email exists/reserved/invalid | Generic pending/cannot-complete, no owner disclosure/mutation |
| SC20-10 | Only old or new channel proof | No email mutation; intent remains/ends by policy |
| SC20-11 | Both channel proofs but re-auth >5 phút | Fresh re-auth required before commit |
| SC20-12 | Current channel deny | Cancel/revoke reservation/proofs; risk alert; no mutation |
| SC20-13 | Email change success | New verified email/version, revoke all/intents, alerts both channels, login_again |
| SC20-14 | Old email lost | No self-service bypass; T021 support path, no new email mutation |
| SC20-15 | Password/recovery/email concurrency | Version/CAS yields one coherent effect; stale loser restarts |
| SC20-16 | Protected/admin-locked/inactive/deleting/deleted | No active-session change or self-unlock/recovery bypass |
| SC20-17 | Dispatch temporary/permanent/late | Same message retry or stop; expired/revoked code cannot commit |
| SC20-18 | Store/audit/CAS outcome unknown | Fail-closed/reconcile same operation; no duplicate/partial mutation |
| SC20-19 | Revocation/token/notification dependency failure | No success giả; durable version wins, access fail-closed, alert pending |
| SC20-20 | Scan URL/log/audit/error/metric | Không secret/code/token/full email/body/provider payload/high-cardinality ID |

## 11. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-CHG-F01 | Mất current email/provider và manual support authority/evidence | Không dual-proof bypass; support không đặt credential/cấp session | M01-T021; M11-T027–T035 |
| M01-CHG-F02 | Provider link/unlink và thay provider email | Không auto-link theo email, không gỡ phương thức cuối | M01-T013–T015; M12-T006–T010 |
| M01-CHG-F03 | Security alert catalog, escalation và user dispute | Durable mandatory intents; deny không rollback mutation đã commit | M01-T038–T039; M10 |
| M01-CHG-F04 | Intent/reservation/key/session/email enforcement và runtime test | Không release qua profile DTO/current single-token design | M01-T042-A; M12-T026–T030, T040–T041, T047-A; REL-03 |

## 12. Tự kiểm M01-T020, A-G01 và A-G02

- Password/email mutation tách khỏi profile/admin endpoint; không nhận target user ID và luôn cần full current session + re-auth ≤5 phút.
- Password change giữ/reissue exact current family, revoke others; email change dual-proof + reservation rồi revoke all/login lại; cả hai có CAS/version/idempotency.
- Risk/state/support/provider boundary, concurrency với recovery, expiry/resend và dependency/commit unknown đều có hành vi fail-safe.
- Audit/alert/redaction cho trước/sau/security/session effect đã chốt; admin/support không xem/đặt credential hoặc bypass proof.
- 20 case bao phủ proof/policy/race/state/provider/store/token/notification/privacy; A-G01/A-G02 có baseline nhưng chưa runtime/quyền chung.
- 7 sai lệch và 4 finding mở có task tiếp nhận; không kết luận A-G01/A-G02 đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt re-auth, password/email change, dual proof, session impact và 20 case | WSA-7K2 |
