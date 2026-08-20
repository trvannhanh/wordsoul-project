# Xử lý tài khoản không hoạt động M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T012 |
| Policy ID / phiên bản | M01-INACTIVE-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-017, D-026–D-027; M01 lifecycle 1.0; M01-LOGIN-1.0; M01-ABUSE-1.0; M01-D002, D017–D021 |
| Phạm vi | Login, action ticket, refresh/existing session, public response, notification và recovery cho 8 trạng thái + restriction overlay |
| Giới hạn | Session family/token chi tiết T016–T018; recovery/support T019–T021; admin lock T028–T032; deletion T033–T037 |

## 1. Bất biến

- Trạng thái/account version ở durable M01 store là truth tại login, refresh và sensitive API; token/provider assertion/client state không được mở quyền trái state hiện tại.
- State-specific response chỉ xuất hiện **sau credential hợp lệ** hoặc session/action-ticket evidence hợp lệ. Nonexistent/wrong/no-direct/deleted credential path giữ generic failure.
- Không trạng thái nào trả full session trước khi state/policy/security/audit/session commit đều hợp lệ. Limited session cho `Chờ xác minh thư` vẫn phải commit như session và bị scope/state gate cưỡng chế.
- Action ticket là quyền tối thiểu cho một journey, không phải access/refresh token; không dùng để gọi API nghiệp vụ khác hoặc nâng quyền.
- Automated login risk state `Elevated/Protected/Recovery required` của M01-ABUSE-1.0 là lớp kiểm soát tạm, **không** tự đổi lifecycle thành `Tạm khóa do rủi ro`. Chỉ confirmed compromise/rule có owner/audit mới chuyển lifecycle và áp thu hồi phiên.
- Notification lỗi không hoàn tác state an toàn hoặc mở quyền. Alert intent giữ pending/idempotent; audit/state commit lỗi thì transition/login fail-closed.
- Không auto-unlock admin/long-term inactive/deletion bằng hết timer login. Mở lại/hủy phải qua transition contract có thẩm quyền.

## 2. Public result và action allowlist

| Result | Khi được trả | Nội dung tối đa |
|---|---|---|
| `LOGIN_INVALID_CREDENTIALS` | Nonexistent, no direct credential, wrong password, deleted identity không còn credential | Generic 401; không next action/state/account existence |
| `LOGIN_LIMITED` | Credential đúng, `Chờ xác minh thư`, policy resolver known | 200 + limited session envelope + `verify_email` action ticket; không full scope |
| `LOGIN_ACTION_REQUIRED` | Credential đúng nhưng state cho phép journey hẹp | Generic 403 + opaque ticket + `nextAction` allowlist |
| `LOGIN_UNAVAILABLE` | Credential đúng nhưng state chặn mọi login journey thông thường | Generic 403 + support/retry guidance không nêu reason nội bộ |
| `LOGIN_POLICY_UNAVAILABLE` | State/policy/consent resolver unknown/stale | Generic 503; không action ticket hoặc session |
| `LOGIN_TEMPORARILY_UNAVAILABLE` | Audit/session/state/control dependency lỗi | Generic 503 + correlation protected |
| `LOGIN_AUTHENTICATED` | Active/restriction-compatible và session commit | 200 + session envelope T016 |

`nextAction` chỉ gồm `verify_email`, `complete_required_policy`, `recover_security`, `review_or_cancel_deletion`. Client không nhận admin-lock reason, risk rule, deletion job stage, internal state enum hoặc support case detail.

## 3. Ma trận state–login–session

| Lifecycle state / overlay | Login sau credential đúng | Action ticket / quyền hẹp | Refresh và phiên hiện có | Public profile/data | Thoát an toàn / thông báo |
|---|---|---|---|---|---|
| `Chờ xác minh thư` | `LOGIN_LIMITED`: session học giới hạn đã commit, không full session | `verify_email`: xem trạng thái xác minh, resend trong quota, submit proof, logout; ticket không cấp quyền học | Limited session cho xem học liệu/học giới hạn; cấm xuất bản, xã hội, PvP, off-app notification, export và thay đổi nhạy cảm; refresh không nâng quyền | Riêng tư tối thiểu, không public | Proof M01-VER-1.0 + điều kiện khác; email/inbox trạng thái idempotent; phát session mới sau transition |
| `Chờ điều kiện tuổi/đồng ý` | Không full session; action chỉ khi REL-01/policy resolver xác định | `complete_required_policy`: xem policy đã publish và submit grant/decline/withdraw đúng purpose; không mặc định adult/consented | Không refresh vượt scope; policy unknown/stale → 503, không ticket | Private/minimal; không public từ giả định tuổi | Ledger/policy version hợp lệ; notification theo policy, không ép consent |
| `Hoạt động` | Full session nếu eligibility/risk/restriction đều đạt | Không action ticket mặc định | Duy trì theo T016–T018 và security epoch | Theo profile/privacy/role | Transition khác có audit; security alert theo M01-D024 |
| `Tạm khóa do rủi ro` lifecycle | Không full session; generic unavailable hoặc action-required chỉ theo recovery policy | `recover_security`, purpose-bound; không xem dữ liệu nghiệp vụ | Chặn cấp/gia hạn; confirmed compromise thu hồi phạm vi đã quyết, không chỉ dựa failed attempts | Không mutation/public change; existing public visibility xử lý theo incident/privacy policy | Recovery T019/T020, rotate credential/security epoch; alert bắt buộc + operational case |
| `Khóa quản trị` | `LOGIN_UNAVAILABLE`; không ticket tự mở khóa | Chỉ support/appeal entry không mang account access; case evidence riêng | Chặn cấp/gia hạn và mọi session theo lock scope; no old-session reuse | Không sửa/public change; support view đã che theo quyền | Authorized unlock T031/T032, reason/audit/appeal; notification không lộ moderator nội bộ |
| `Ngừng hoạt động lâu dài` | `LOGIN_UNAVAILABLE`; không automatic recovery | Support/appeal nếu policy cho phép, không access ticket | Mọi phiên/credential grant bị chặn; mở lại không tái dùng session cũ | Không public; retention/support access theo policy | Authorized reopen sau data/state check hoặc chuyển deletion; notification/case audit |
| `Chờ xóa`, còn trong cửa sổ hủy | Không login thường; `LOGIN_ACTION_REQUIRED` sau credential + deletion state check | `review_or_cancel_deletion`: chỉ xem trạng thái/tác động và submit cancel đã re-auth; không mutation khác | Session thường revoked; ticket 10 phút, deletion/state version bound; refresh không dùng được | Gỡ public, dữ liệu giữ ổn định đến deletion workflow | Cancel có thẩm quyền/proof về safe prior state hoặc manifest hoàn tất; notification mỗi transition |
| `Chờ xóa`, qua điểm không thể hủy | `LOGIN_UNAVAILABLE`; không cancel ticket | Chỉ support status generic nếu legal/policy cho phép | Không session/refresh | Không public; deletion jobs tiếp tục | Chỉ `Đã xóa/ẩn danh` sau reconcile; không quay Active |
| `Đã xóa/ẩn danh` | Generic invalid credentials; không xác nhận tombstone | Không ticket/recovery/link | Không session/refresh/provider link; callback cũ không phục hồi | Không identifiable profile; retained data không relink | Final; re-registration tạo identity mới theo T037 |
| Restriction overlay không chặn login | Session scope giảm từ state truth | Ticket chỉ nếu restriction có appeal journey riêng; không tự gỡ | API kiểm current restriction/version, không chỉ claim | Field/capability theo scope restriction | Expiry không tự nâng token; reissue/re-evaluate + audit |
| Restriction overlay chặn login | Generic unavailable sau credential đúng | Support/appeal hẹp nếu policy cho phép | Chặn refresh/session theo scope | Không mutation/public change trong scope | Authorized/automatic rule transition có audit; không client toggle |

## 4. Action ticket baseline

| Thuộc tính | Quy tắc |
|---|---|
| Identity | Opaque random ID; server record giữ protected subject/account generation; không chứa email/state trong URL |
| Purpose/audience | Một `nextAction`, đúng M01 endpoint/audience; không đổi purpose hoặc dùng cho API nghiệp vụ |
| TTL | Tối đa 10 phút; deletion cancel vẫn không vượt cửa sổ hủy; verification/recovery proof có TTL riêng và không được ticket kéo dài |
| Binding | Account generation + lifecycle/state version + security epoch + policy/deletion intent version + client channel class |
| Replay | One-time cho state-changing submit; status view replay-safe có counter; rotate/revoke ticket cũ sau transition/new issue |
| Rights | Chỉ endpoint allowlist, no bearer access/refresh, no role/profile/business claims |
| Storage/log | Store keyed digest/metadata tối thiểu; không log ticket/raw subject; audit issue/use/reject/expire bằng protected ref |
| Failure | Store/audit/state CAS không chắc → không issue/consume; return generic 503, không fallback unsigned/self-contained ticket |

Ticket không phải email verification/recovery/deletion evidence. Nó chỉ cho phép bước journey lấy/submit evidence theo contract chủ; evidence cuối vẫn kiểm deadline, one-time use, state/version và owner.

## 5. Thứ tự kiểm tra và race

1. Limiter/risk/schema trước lookup; credential verification theo M01-LOGIN-1.0.
2. Đọc lifecycle + restriction + state version + security epoch + policy/deletion/risk refs trong consistent snapshot.
3. Ánh xạ state sang result/action; unknown state, unknown enum hoặc impossible combination → fail-closed + audit/alert, không dùng `IsActive=true` default.
4. Nếu full session: session commit CAS với state/security versions. Nếu ticket: ticket record commit CAS với state/action versions.
5. State đổi giữa snapshot và commit → reject/503, revoke artifact chưa trả; đọc lại không được tự retry phát token/ticket khác quá budget.
6. Transition sau khi session đã phát phải làm enforcement current-state nhận biết ngay theo T016–T018; notification không phải enforcement.
7. Duplicate login/action request trả cùng result/artifact hợp lệ hoặc conflict an toàn theo operation/fingerprint; không tạo nhiều email/ticket/session ngoài contract.

## 6. Notification và support boundary

| Sự kiện | Người nhận / nội dung | Dedup/failure |
|---|---|---|
| Vào/ra lifecycle risk lock | User security inbox + verified channel, operational security | Một intent/transition version; pending nếu delivery lỗi |
| Admin lock/unlock/long-term inactive | User nếu policy cho phép + support/security case | Không nêu reporter/internal evidence; audit bắt buộc |
| Deletion requested/cancelled/irreversible/completed | Verified channel + support case | Một intent/state version; không link chứa long-lived access |
| Restriction applied/removed/expired | User theo scope/appeal policy | Versioned; expiry worker không tự cấp token mới |
| Pending verification/policy | Theo consent/channel policy và quota | Không spam; failed delivery không mở full session |

Support search/case không nhận action ticket như proof danh tính. Support chỉ xem field đã che theo M11 quyền/case purpose; không đổi state trực tiếp ngoài command có actor/reason/version/audit.

## 7. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| Lifecycle/restriction row thiếu hoặc enum không biết | 503/unavailable; alert data integrity; không coi Active |
| Policy/consent resolver unknown | 503 policy unavailable; không issue action/full session |
| State/audit/session/ticket store lỗi | Không artifact; generic 503; reconcile orphan nếu commit outcome unknown |
| Notification/email lỗi | State/ticket enforcement giữ nguyên; durable notification pending/expire, không retry mù |
| Revocation propagation chậm | API current-state check chặn sensitive/all access theo state; alert incident, không dựa token TTL |
| Concurrent unlock/delete/lock/login | CAS/version conflict; deny artifact và reconcile; transition priority T031/T035 quyết cuối |
| Deleted tombstone/provider callback tới muộn | Không relink/recover; record late event protected + reject |

## 8. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát | Sai lệch / rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-INACT-I01 | `User` chỉ có `IsActive`; không có lifecycle/reason/version/restriction | Không biểu diễn hoặc enforce 8 state/overlay/action | M01-T031, T042-A |
| M01-INACT-I02 | Direct login không kiểm `IsActive`/lifecycle trước cấp token | Locked/inactive/deleting account có thể login | M01-T016–T018, T042-A |
| M01-INACT-I03 | Refresh path không có current lifecycle/security epoch contract | Token có thể tiếp tục sau state change | M01-T016–T018 |
| M01-INACT-I04 | Chưa có action-ticket store/contract | Pending/recovery/deletion journey dễ dùng full token hoặc response ad hoc | M01-T016, T019, T035 |
| M01-INACT-I05 | Một refresh token nằm trực tiếp trên `User` | Không revoke theo family/device/scope và dễ ghi đè concurrent | M01-T016–T018 |
| M01-INACT-I06 | Google login có đường tự link email trùng | Provider callback có thể bỏ qua state/link boundary | M12-T006–T010; M01-T013–T015 |

I01–I06 là release blocker cho state enforcement; baseline tài liệu không thay source/runtime evidence.

## 9. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| IA12-01 | Pending verification + correct credential | Limited session đã commit + action ticket verify 10 phút; cấm full/publish/social/PvP/sensitive scope |
| IA12-02 | Pending consent/policy + resolver known | Policy action only; decline không bị đổi thành grant |
| IA12-03 | Policy unknown/stale | 503, không ticket/session/default Active |
| IA12-04 | Active eligible | Một session commit theo T016 |
| IA12-05 | Automated Elevated risk | Step-up theo M01-ABUSE; không tự lifecycle risk lock/revoke sessions |
| IA12-06 | Confirmed lifecycle risk lock | Không login/refresh; recovery only; revocation theo incident decision |
| IA12-07 | Admin lock/long inactive | Generic unavailable; no self-unlock ticket/session |
| IA12-08 | Pending deletion còn hủy | Ticket review/cancel only, version-bound; no normal refresh |
| IA12-09 | Pending deletion irreversible | Không cancel/login; deletion continues |
| IA12-10 | Deleted/tombstoned | Generic invalid credentials, no recovery/link; re-register new identity only |
| IA12-11 | Restriction không chặn login | Scoped session + current restriction enforcement; expiry không auto-elevate token |
| IA12-12 | Restriction chặn login | Generic unavailable; support/appeal only if policy allows |
| IA12-13 | State đổi giữa verify và session/ticket commit | CAS reject, không artifact stale |
| IA12-14 | State store/audit lỗi | Generic 503; no token/ticket |
| IA12-15 | Notification lỗi sau state commit | Enforcement giữ, one pending intent; no rollback/open access |
| IA12-16 | Callback/provider/token cũ sau delete/lock | Reject, không restore/link; protected late-event audit |
| IA12-17 | Duplicate action/login request | Idempotent same artifact/result hoặc conflict; không multi-send/session |
| IA12-18 | Enumeration/redaction sweep | Wrong/nonexistent/deleted/no-direct không phân biệt; state-specific chỉ sau proof |

## 10. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-INACT-F02 | Recovery/support/admin lock và transition priority | Không self-unlock hoặc auto-unlock authorized states | M01-T019–T021, T028–T032 |
| M01-INACT-F03 | Deletion window/irreversible point/manifest | Không giả số; pending delete không mutation/login | M01-T033–T037 |
| M01-INACT-F04 | REL-01 market/age/consent final behavior | Policy unknown không Active/session | REL-01; M01-T033 |

## 11. Tự kiểm M01-T012 và A-G01

- Đủ 8 lifecycle state, hai restriction behavior và automated risk overlay; mỗi trạng thái có login result, ticket/quyền, refresh/session, profile, exit/notification.
- Bảy public result và bốn next-action giữ state-specific detail sau proof; deleted/nonexistent/wrong/no-direct không bị phân biệt.
- Action ticket 10 phút, purpose/audience/state/security/policy bound, one-time/CAS và không là access/refresh token.
- Failed attempts không tự lifecycle-lock/revoke; confirmed risk/admin/deletion không auto-unlock; current-state truth thắng token/provider.
- 18 case bao phủ state/race/failure/notification/restriction/deletion/enumeration; A-G01 có baseline nhưng runtime evidence còn thiếu.
- 6 sai lệch và 3 finding mở có task tiếp nhận; M01-INACT-F01 đã đóng bằng M01-SESSION-1.0, chưa có runtime evidence nên không kết luận A-G01 đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt state/login/session/action-ticket/notification matrix và 18 case | WSA-7K2 |
