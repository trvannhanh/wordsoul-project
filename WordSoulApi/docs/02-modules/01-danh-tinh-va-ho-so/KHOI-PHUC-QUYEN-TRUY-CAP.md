# Khôi phục quyền truy cập M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T019 |
| Contract ID / phiên bản | M01-RECOVERY-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012, D-014, D-024, D-028–D-030; M01-CRED-1.0; M01-VER-1.0; M01-SESSION-1.0; M01-ABUSE-1.0; M12-RATE-1.0; M12-FAIL-1.0 |
| Phạm vi | Yêu cầu quên mật khẩu, phát hành/gửi lại/tiêu thụ bằng chứng, đặt verifier mới, thu hồi phiên, risk state, audit và thông báo |
| Ngoài phạm vi | Đổi chủ động credential/email T020; hỗ trợ khi mất mọi kênh T021; delivery contract đầy đủ M12-T026–T030; triển khai source/runtime |

## 1. Bất biến an toàn

- Email đã xác minh là kênh tự phục vụ duy nhất trong baseline; không dùng tên hiển thị, câu hỏi bí mật, số điện thoại chưa được quyết định hoặc email do caller thay thế.
- Request/resend luôn trả tiếp nhận trung tính cho account tồn tại, không tồn tại, external-only, chưa xác minh, không đủ trạng thái hoặc đã vượt quota.
- Recovery intent chỉ có purpose `reset-direct-credential`, gắn subject/email/security/state/policy version và dùng một lần. Không tái dùng verification intent/code.
- Code chỉ lưu bằng keyed verifier, so sánh constant-time; password, code, verifier, request body và email đầy đủ không vào URL/log/audit/analytics.
- Consume proof, ghi verifier mới, tăng credential/security epoch, thu hồi mọi session family và ghi audit/outbox là một transaction hoặc workflow có trạng thái đối soát rõ; không trả thành công khi effect chưa chắc chắn.
- Recovery hoàn tất không cấp access/refresh token. Người dùng luôn đăng nhập lại; token/session cũ bị current-state và security-epoch gate chặn ngay.
- Recovery không tự đổi email/provider/role, không mở khóa quản trị/ngừng hoạt động/chờ xóa và không phục hồi identity đã xóa.
- Support/admin chỉ được khởi tạo hành trình theo quyền/case/audit; không xem code, chọn mật khẩu, nhận session hoặc bỏ qua proof thay người dùng.

## 2. Tham số baseline

| Tham số | Giá trị M01-RECOVERY-1.0 |
|---|---|
| Purpose | `reset-direct-credential` |
| Intent TTL | 15 phút từ lúc commit; resend không kéo dài intent cũ |
| Code | 12 ký tự Crockford Base32, không phân biệt hoa/thường, chỉ nhận trong POST body |
| Lần thử | Tối đa 5 code sai/intent; lần sai thứ 5 khóa intent atomically |
| Resend cooldown | 60 giây cho account + purpose |
| Request budget | RL-008: 5/24 giờ/account candidate, 10/60 phút/IP, 10/24 giờ/device; mọi partition đều áp dụng |
| Dispatch budget | RL-017 và request budget cùng áp dụng; đổi provider/recipient/workload không reset quota |
| Nhiều intent | Intent mới commit thu hồi mọi intent cùng purpose cũ chưa consume; retry cùng operation không tạo generation mới |
| Operation retention | Giữ metadata idempotency/reconcile tối thiểu 24 giờ, không chứa code/password/email đầy đủ |
| Audit retention | 12 tháng mặc định theo M01-D025; secret verifier xóa khi consume/revoke/expire |

## 3. Hợp đồng request và resend

### Request

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-RECOVERY-1.0`; unsupported version reject theo schema, không lookup account |
| `email` | Có | Canonicalize server-side theo M01-DICT/M01-LOGIN; giới hạn kích thước trước lookup |
| `operationId` | Có | Opaque random ≥128 bit, ổn định qua retry cùng intent yêu cầu |
| `clientContext` | Có | Channel/app version + protected device evidence nếu có; không trusted flag |
| `correlationId` | Có | Opaque trace ID; không là idempotency/business key |

Public response hợp lệ luôn là `202 RECOVERY_REQUEST_ACCEPTED`, kể cả không dispatch. Schema malformed/oversized có thể nhận `400 INVALID_REQUEST` nhưng không phản ánh account state; quota có thể trả cùng neutral body và `Retry-After` coarse theo boundary không lộ partition.

### Phát hành

1. Kiểm schema/version, trusted ingress, RL-008 composite buckets và risk signal trước lookup tốn kém.
2. Canonicalize email; lookup bằng protected candidate. Nonexistent path vẫn chạy timing/limiter/audit category tương đương.
3. Chỉ tạo intent nếu email đang verified/current, account có direct credential, state cho self-service recovery và policy/risk/audit/store đều known.
4. Commit intent generation, keyed code verifier và MAIL-A-1.0 outbox với cùng operation; sau đó mới trả neutral response.
5. Retry cùng operation/fingerprint trả cùng public result và không tạo code/message mới. Same operation khác fingerprint là conflict bảo mật, không dispatch.
6. Resend hợp lệ tạo generation/code/message mới và revoke generation cũ trong cùng transaction. Delivery `accepted` không đồng nghĩa delivered; temporary/unknown retry cùng message ID.

Email chỉ chứa hướng dẫn, `intentId`, code, expiry display và cảnh báo bỏ qua nếu không yêu cầu. Không đặt code trong link/query/fragment hoặc template analytics; người dùng nhập code vào ứng dụng.

## 4. Hợp đồng hoàn tất reset

| Field | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-RECOVERY-1.0` |
| `intentId` | Có | Opaque selector, không phải evidence |
| `code` | Có | 12 ký tự, body-only; không normalize ngoài case-insensitive Crockford mapping |
| `newPassword` | Có | Chuỗi chính xác 12–128 Unicode scalar, không trim/cắt; toàn bộ M01-CRED-1.0 |
| `operationId` | Có | Opaque ≥128 bit, ổn định qua retry cùng reset intent |
| `clientContext`, `correlationId` | Có | Cùng class/binding đã phát hành; không PII/authority do client tự khai |

Trình tự bắt buộc:

1. Validate schema/version/rate, lookup intent và verify code constant-time; attempt counter tăng atomically khi code sai.
2. Kiểm intent Active, purpose, expiry, generation, email/subject/security/state/policy version và client binding. Mismatch trả `RECOVERY_INVALID_OR_EXPIRED`, không nêu lý do.
3. Sau proof đúng mới trả lỗi mật khẩu có thể hành động (`PASSWORD_POLICY_NOT_MET` hoặc dependency unavailable) mà không lộ account; proof chưa consume nhưng vẫn hết hạn theo deadline gốc.
4. Kiểm new password với policy/blocklist/breached source; nguồn bắt buộc lỗi thì fail-closed, không consume proof.
5. Transaction CAS `intent.status == Active`: ghi verifier mới, tăng credential/security epoch, revoke mọi family/refresh/access, consume intent, revoke sibling intents, đóng/giảm risk episode chỉ khi rule hiện hành cho phép, ghi audit và security-notification outbox.
6. Commit trước response. Thành công trả `RECOVERY_COMPLETED` + action `login_again`, không token/session/profile.
7. Commit outcome unknown được reconcile bằng `(intentId, operationId)`. Không ghi verifier lần hai, không phục hồi session và không kết luận thất bại khi chưa biết effect.

Retry cùng operation sau commit trả cùng kết quả không bí mật. Consumed proof với operation khác trả generic invalid/replay và tạo security signal; không lặp mutation/thông báo. Mật khẩu mới không được bằng mật khẩu hiện tại theo M01-CRED-1.0.

## 5. Trạng thái và eligibility

| Trạng thái/phương thức | Tạo intent | Hoàn tất | Hành vi sau proof |
|---|---|---|---|
| Active + direct credential + verified email | Có | Có nếu version/current policy còn hợp lệ | Reset, revoke all, login lại |
| Elevated/Protected automated risk | Có theo risk policy | Có; không bỏ qua signal khác | Reset/revoke all; episode chỉ giảm theo rule/audit, có thể vẫn cần step-up |
| Recovery required/confirmed compromise | Có | Có với security response bắt buộc | Reset, tăng epoch, revoke all, đóng incident chỉ khi điều kiện đủ |
| Chờ xác minh email | Không | Không | Dùng M01-VER-1.0; không coi unverified mailbox là recovery channel |
| External-only, không direct credential | Không | Không | Đăng nhập provider hoặc safe link/create credential qua T013–T015/T020 |
| Khóa quản trị hoặc ngừng hoạt động lâu dài | Không tự phục vụ | Không tự mở | Appeal/support theo T021/T031; reset không đổi lifecycle state |
| Chờ xóa | Không | Không | Chỉ review/cancel deletion contract; recovery không né deletion |
| Đã xóa/ẩn danh/tombstone | Không | Không | Final; không recover/relink; đăng ký lại là identity mới |
| Email version đổi/unverified sau phát hành | Intent bị revoke/invalid | Không | Hành trình theo current channel/state |

## 6. Đồng thời, replay và bất định

| Tình huống | Kết quả xác định |
|---|---|
| Hai request cùng operation | Một intent/message; cùng neutral response |
| Hai request khác operation | Quota áp dụng; generation commit sau revoke generation trước; email cũ vô hiệu |
| Resend và completion race | CAS/version quyết định; hoặc old completion commit trước rồi resend không tạo intent sau epoch đổi, hoặc resend thắng và old code invalid |
| Hai completion cùng proof/same operation | Một mutation; request còn lại đọc committed result |
| Hai completion khác operation | Một mutation; loser là replay signal, không đổi verifier lần nữa |
| Password-policy failure rồi retry | Intent còn Active nếu còn hạn/attempt; operation mới hoặc semantics retry đã công bố, không consume proof |
| Timeout trước/giữa commit | Reconcile operation; không báo success hoặc tạo verifier/session thay thế |
| Email đến trễ sau resend/reset | Generation/epoch cũ invalid; worker dừng dispatch khi revoke/expire/consume |

## 7. Thông báo, audit và riêng tư

| Event | Metadata allowlist |
|---|---|
| `identity.recovery.requested` | Protected candidate class, operation/correlation, channel, result category |
| `identity.recovery.intent_created_or_revoked` | Protected subject/intent ref, generation, purpose, expiry class, reason category |
| `identity.recovery.proof_rejected` | Intent ref, attempt bucket, expired/locked/replay category, coarse client match |
| `identity.credential.reset` | Subject ref, credential/security version before/after, policy version, operation |
| `identity.sessions.revoked_for_recovery` | Subject ref, family count bucket, security epoch, result |
| `identity.recovery.dependency_failed` | Dependency/category, operation, commit-known/unknown, retry class |

Thông báo bảo mật bắt buộc khi reset hoàn tất và khi replay/abuse nghiêm trọng; dùng verified safe channel/current policy, durable outbox và dedup theo operation/security version. Notification lỗi không rollback credential/revocation; outbox pending và operations alert. Nội dung không chứa password/code/token, full device/IP hoặc link cấp quyền; có thời điểm, loại hành động và đường báo cáo an toàn.

Metric không dùng high-cardinality/raw identifiers: request accepted/eligible/limited, dispatch semantic result, proof success/wrong/expired/locked/replay, recovery latency, revoke family count bucket, risk transition, dependency/commit-unknown và notification backlog.

## 8. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| Limiter/risk/account/intent/audit store unavailable | Neutral response cho request nhưng không tạo/dispatch; completion fail-closed, không reset |
| Email adapter temporary/unknown | Intent giữ pending; retry cùng message ID trong deadline/budget, không gửi code mới |
| Email adapter permanent failure | Dừng dispatch; intent revoke/expire an toàn; public request vẫn neutral |
| Breached-password/policy service unavailable | Không consume proof hoặc đổi verifier; trả dependency unavailable sau proof hợp lệ |
| CAS/commit outcome unknown | Reconcile cùng operation; không chạy transaction thứ hai |
| Revocation index/cache unavailable sau durable reset | Credential/epoch/revoke truth vẫn commit; protected APIs fail-closed đến khi propagation đối soát |
| Notification unavailable | Reset/revoke giữ nguyên; outbox pending + operational alert |
| Clock/config/key anomaly | Dừng tạo/consume intent; không nới TTL/attempt/quota hoặc dùng key cũ không được phép |

## 9. Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-REC-I01 | Không tìm thấy forgot/reset endpoint, DTO hoặc service | Không có request trung tính, proof, expiry, replay/concurrency hay reset flow | M01-T042-A |
| M01-REC-I02 | `User` chỉ có `PasswordHash`; không recovery intent/generation/attempt/security epoch | Không thể one-time CAS, revoke sibling hoặc chặn stale session | M01-T042-A |
| M01-REC-I03 | `AuthService` tạo `PasswordHasher<User>` trực tiếp | Không có M01-CRED policy/version, blocklist/breached check hoặc upgrade contract | M01-T020, T042-A |
| M01-REC-I04 | Session hiện là một raw refresh token trên `User`, access JWT sống một ngày | Không thể revoke all family/current access theo D-009 | M01-T018, T042-A |
| M01-REC-I05 | Email service hiện nhận recipient/subject/body ad hoc và không có semantic result đầy đủ | Không chứng minh template allowlist, idempotent dispatch, unknown/reconcile hoặc redaction | M12-T026–T030; REL-03 |
| M01-REC-I06 | Auth limiter quan sát chỉ theo IP và không thấy recovery route | Thiếu RL-008 composite/durable quota và chống enumeration nhiều instance | M12-T047-A; REL-03 |
| M01-REC-I07 | Không thấy audit/security alert cho recovery/reset/replay | Không truy vết hoặc cảnh báo được takeover/reset bất thường | M01-T038–T039, T042-A |

I01–I07 là release blocker; hiện trạng không có runtime evidence cho recovery an toàn.

## 10. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| RC19-01 | Request email eligible | 202 neutral; một intent/outbox sau commit |
| RC19-02 | Nonexistent/external-only/unverified/deleted | Cùng public response/timing class; không dispatch/state leak |
| RC19-03 | Retry same request operation | Cùng result, không intent/message/code mới |
| RC19-04 | Vượt RL-008/RL-017 | Không dispatch; neutral/429 boundary không lộ account |
| RC19-05 | Resend trước 60 giây | Không generation/message mới |
| RC19-06 | Resend hợp lệ | Intent mới duy nhất, old revoked, quota không reset |
| RC19-07 | Code đúng + password đạt policy | Atomic verifier/epoch/revoke-all/consume/audit/outbox; login_again, no token |
| RC19-08 | Code sai 4 lần rồi đúng | Lần đúng được phép nếu còn hạn; counter atomic |
| RC19-09 | Code sai lần 5 | Intent locked; mọi lần sau generic reject |
| RC19-10 | Expired/revoked/wrong purpose/generation | Không mutation; generic invalid/expired |
| RC19-11 | Password policy/breached dependency reject | Proof chưa consume, verifier/session không đổi; actionable only after valid proof |
| RC19-12 | Hai completion same operation | Một mutation, cùng committed non-secret result |
| RC19-13 | Replay proof khác operation | Không mutation lần hai; security signal/alert |
| RC19-14 | Resend/completion race | Một generation/effect xác định bởi CAS; không cả hai credential outcome |
| RC19-15 | Risk-locked recovery | Reset/revoke all; state chỉ giảm khi current risk rule đủ điều kiện |
| RC19-16 | Admin-locked/inactive/deleting | Không self-unlock/né deletion; support/action phù hợp |
| RC19-17 | Store/audit/CAS outcome unknown | Fail-closed + reconcile same operation; không duplicate reset |
| RC19-18 | Email temporary/permanent/late | Retry same message hoặc dừng; old/late code không hợp lệ |
| RC19-19 | Revocation cache/notification lỗi sau commit | Access fail-closed; reset giữ nguyên; outbox/alert pending |
| RC19-20 | Scan URL/log/audit/error/metric | Không password/code/verifier/full email/body/token/high-cardinality ID |

## 11. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-REC-F02 | Mất email/mọi kênh và support evidence/authority | Không self-service fallback, support không đặt password/cấp session | M01-T021; M11-T027–T035 |
| M01-REC-F03 | Alert catalog, recipient fallback và incident closure | Durable mandatory security intent; không rollback reset vì delivery | M01-T038–T039; M10 |
| M01-REC-F04 | Intent/schema/key/email/session enforcement và runtime tests | Không release trên current no-recovery/single-token design | M01-T042-A; M12-T026–T030, T040–T041, T047-A; REL-03 |

## 12. Tự kiểm M01-T019 và A-G01

- Request/resend trung tính, eligibility theo verified email/current state và RL-008/RL-017 đã được chốt; không có đường dò account hoặc đổi kênh né quota.
- Code 12 ký tự, TTL 15 phút, tối đa 5 lần thử, one-time CAS/generation và resend invalidation có hành vi concurrency/replay xác định.
- Reset atomically đổi verifier, tăng epoch, revoke mọi family, audit/outbox và bắt login lại; không token/session được cấp từ recovery.
- State/risk/admin/deletion, dependency failure, commit unknown, late email và notification/revocation propagation đều có fail-safe behavior.
- 20 case bao phủ success/failure, enumeration, resend, replay, concurrency, policy, state, provider/store và redaction; A-G01 có baseline nhưng chưa runtime.
- 7 sai lệch và 3 finding mở có task tiếp nhận; M01-REC-F01 đã được đóng bởi M01-SEC-CHANGE-1.0; không kết luận A-G01 đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt recovery request/proof, TTL/attempt/resend, atomic reset/revoke-all và 20 case | WSA-7K2 |
