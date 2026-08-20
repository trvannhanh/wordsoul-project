# Đặc tả đăng nhập trực tiếp M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T010 |
| Contract ID / phiên bản | M01-LOGIN-1.0 |
| Trạng thái | Baseline đăng nhập trực tiếp có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-017; D-012; M01-D003; M01-CRED-1.0; vòng đời tài khoản M01 v1.0 |
| Phạm vi | Email/password login, response trung tính, state/policy gate, audit handoff và session issuance boundary |
| Ngoài phạm vi | Threshold chống thử sai M01-T011; state UX chi tiết M01-T012; session/token lifecycle M01-T016–T018; external login M01-T013–T015 |

## Bất biến

- Định danh đăng nhập trực tiếp là email canonical theo M01-REG-1.0; `Username` chỉ là tên hiển thị và không dùng để lookup/xác thực.
- Client chỉ gửi email, password, contract version và context kỹ thuật tối thiểu; không gửi account ID, role, account state, email-verified hoặc policy truth.
- Account không tồn tại, không có direct credential và password sai dùng cùng public failure; lookup và verify path phải giảm timing distinction thực tế.
- Chỉ account đủ state, email verification và policy/consent hiện hành mới được handoff tạo full session.
- Kiểm tra password thành công không tự nghĩa là được truy cập. State/policy/limiter/audit/session-store đều có thể từ chối an toàn.
- Không chạy daily quest, onboarding, reward hoặc business side effect trong login. Những tác vụ đó thuộc module/worker riêng.

## Request

| Trường | Bắt buộc | Quy tắc |
|---|---|---|
| `contractVersion` | Có | `M01-LOGIN-1.0`; version không hỗ trợ bị từ chối trước lookup |
| `email` | Có | String, giới hạn byte/ký tự như M01-REG-1.0; canonicalize server-side; không trim password theo email |
| `password` | Có | String nguyên trạng; không log/bind sang model chung; giới hạn transport 128 ký tự theo M01-CRED-1.0 |
| `clientContext` | Có | App/channel/version allowlist; device/source chỉ là tín hiệu, không là identity truth |
| `correlationId` | Có | Opaque hoặc server cấp; không chứa PII |

Unknown fields bị từ chối. Body không nhận username, role, remember-me TTL, redirect URL, token scope, account status hoặc limiter bypass.

## Pipeline quyết định

1. Kiểm tra transport, content type, size, strict schema và limiter nền trước lookup.
2. Canonicalize email bằng cùng thư viện/version với registration; invalid trả generic invalid request, không lookup.
3. Lookup bằng canonical email qua unique identity index. Nếu không có direct credential, chạy dummy verifier có cost profile tương đương.
4. Verify password bằng policy/hash metadata hiện hành. Hash legacy hợp lệ chỉ được rehash sau authentication và bằng update an toàn; rehash lỗi không làm phát token nếu session boundary chưa commit.
5. Nếu credential sai/không có, ghi attempt metadata allowlist và trả failure chung.
6. Nếu credential đúng, kiểm tra account state/version, email verification, required policy/consent snapshot, restrictions và security epoch.
7. Chỉ kết quả `eligible` mới gửi command tạo session M01-T016 bằng server-derived subject/role/state snapshot. Session store/audit bắt buộc lỗi thì không trả token.
8. Ghi login outcome bằng protected refs; response không chứa lý do nội bộ, password/hash, limiter key hoặc policy rule.

## Ma trận eligibility tối thiểu

| State/điều kiện | Full session | Public result | Next action |
|---|---|---|---|
| `Hoạt động`, email verified, policy current, không restriction chặn | Có, qua M01-T016 | `200 authenticated` | Session response theo contract riêng |
| `Chờ xác minh thư` | Không full session; có limited session theo M01-INACTIVE-1.0 | `200 LOGIN_LIMITED` sau credential đúng và session/audit commit | Limited learning envelope + action ticket resend/verify; không full token/scope |
| `Chờ điều kiện tuổi/đồng ý` | Không | `403 LOGIN_ACTION_REQUIRED` sau credential đúng | Action ticket chỉ cho policy journey đã resolve |
| `Tạm khóa do rủi ro` | Không | `403 LOGIN_UNAVAILABLE` | Recovery/support generic; không tiết lộ rule/tín hiệu |
| `Khóa quản trị` / `Ngừng hoạt động` | Không | `403 LOGIN_UNAVAILABLE` | Kênh support chung nếu policy cho phép |
| `Chờ xóa` / `Đã xóa hoặc ẩn danh` | Không | `403 LOGIN_UNAVAILABLE` hoặc generic credential failure theo retention state | Chỉ recovery/cancel-delete contract được duyệt |
| Restriction không chặn login | Session scope giảm theo state truth | `200 authenticated` | Capability enforcement tại API, không chỉ claim |
| Policy/consent `unknown` hoặc stale | Không | `503 LOGIN_POLICY_UNAVAILABLE` | Retry policy journey; không mặc định eligible |

Action ticket là opaque, TTL ngắn, audience/purpose/account generation bound, one-time hoặc replay-safe và không phải access/refresh token. Chi tiết chốt tại M01-T012/M01-T016.

## Response và lỗi

| Trường hợp | HTTP/semantic | Nội dung tối đa |
|---|---|---|
| Eligible và session commit | `200 LOGIN_AUTHENTICATED` | Session envelope versioned; subject/role tối thiểu theo M01-T016 |
| Chờ xác minh và limited session commit | `200 LOGIN_LIMITED` | Session scope học giới hạn + verify action ticket; không full role/capability |
| Email không tồn tại/no direct credential/password sai | `401 LOGIN_INVALID_CREDENTIALS` | Thông điệp và latency envelope chung |
| Credential đúng nhưng cần verify/policy | `403 LOGIN_ACTION_REQUIRED` | Generic nextAction + action ticket; không role/tài sản/internal state |
| Credential đúng nhưng blocked state | `403 LOGIN_UNAVAILABLE` | Support/retry guidance chung; không reason nội bộ |
| Schema/email format sai | `400 LOGIN_REQUEST_INVALID` | Field category tối thiểu; không echo email/password |
| Rate limited | `429 LOGIN_RATE_LIMITED` | Retry hint chung, không nói account/source bucket |
| Policy/audit/session dependency unavailable | `503 LOGIN_TEMPORARILY_UNAVAILABLE` | Correlation; không trả token nửa vời |

Không dùng khác biệt status/body/header/cookie dễ quan sát để phân biệt nonexistent với wrong password. State-specific response chỉ sau khi credential đúng; vẫn không trả lý do khóa nhạy cảm.

## Session handoff

| Input server-derived | Yêu cầu |
|---|---|
| Subject/account generation | Protected immutable reference, không lấy ID từ request |
| Authentication method/time | `direct-password`, UTC server time và credential/hash version |
| Account/security/policy snapshot | State version, security epoch, role source version, consent snapshot; M01-T016 quyết claim tối thiểu |
| Client/device context | Normalized allowlist; không biến device ID thành authentication factor ngoài policy |
| Correlation/attempt ref | Opaque refs dùng audit/reconcile |

- Session creation phải atomic với durable session/refresh record và audit bắt buộc; access token không được trả trước commit.
- Login không tự refresh/reuse token cũ. Mỗi successful login tạo session family mới theo M01-T016.
- API authorization vẫn kiểm tra account/security state phù hợp; claim cũ không vượt qua lock/delete/consent withdrawal.

## Audit và observability

| Event | Metadata allowlist |
|---|---|
| `login.attempt.received` | Attempt/correlation ref, channel/app version, coarse source bucket |
| `login.authentication.failed` | Result/reason category nội bộ, verifier cost/version, limiter outcome, latency bucket |
| `login.eligibility.denied` | Subject protected ref, state/policy reason category, account/security version |
| `login.session.requested_or_failed` | Subject/session request ref, result category, latency, correlation |
| `login.succeeded` | Subject/session protected ref, auth method, account/security/policy versions, correlation |

- Không log email đầy đủ, password, hash, token, action ticket, request body, raw IP/user-agent, policy payload hoặc internal exception response.
- Failed attempts cho account không tồn tại dùng protected/coarse key phù hợp retention; không tạo durable “fake account”.
- Metric tối thiểu: success/failure/action-required/blocked/dependency-failure, latency distribution, limiter result và hash migration result; không label bằng email/account raw.

## Ma trận nghiệm thu M01-LOGIN-1.0

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| LOGIN-C01 | Email/password đúng, account eligible | Một session family commit, response 200, audit success |
| LOGIN-C02 | Email không tồn tại | Generic 401, dummy verify, không session/PII log |
| LOGIN-C03 | Password sai | Cùng public envelope với C02; failed attempt đúng một lần |
| LOGIN-C04 | Account chỉ có external credential | Cùng generic 401; không lộ provider/link state |
| LOGIN-C05 | Email khác case/Unicode domain canonical tương đương | Lookup đúng một identity theo canonicalization version |
| LOGIN-C06 | Dùng display name/username | Reject schema hoặc generic failure; không lookup username |
| LOGIN-C07 | Password có leading/trailing whitespace | Verify nguyên trạng; không trim/normalize |
| LOGIN-C08 | Password/body quá giới hạn hoặc unknown fields | 400 trước lookup; không log/echo secret |
| LOGIN-C09 | Chờ xác minh, credential đúng | Limited session đúng scope sau commit + verify ticket đúng purpose/TTL; không full session |
| LOGIN-C10 | Chờ consent/policy, credential đúng | Không full session; policy action fail-closed nếu resolver unknown |
| LOGIN-C11 | Tạm khóa/khóa quản trị/ngừng hoạt động | Không session; generic unavailable, audit state denial |
| LOGIN-C12 | Chờ xóa/đã xóa | Không session/khôi phục ngầm; response theo retention policy |
| LOGIN-C13 | Restriction không chặn login | Session scope đúng snapshot; API vẫn enforce restriction |
| LOGIN-C14 | Limiter từ chối trước lookup | 429 chung; không verifier/session/downstream call |
| LOGIN-C15 | Hai login đúng đồng thời | Hai session family riêng nếu policy cho phép; không ghi đè token dùng chung |
| LOGIN-C16 | Audit/session store lỗi | 503, không token/cookie/session nửa vời |
| LOGIN-C17 | Legacy hash đúng và rehash | Authentication đúng; update có concurrency; không hạ policy hoặc log hash |
| LOGIN-C18 | Hash verifier lỗi/cost metadata không hỗ trợ | Fail-closed 503/401 theo taxonomy; không bypass |
| LOGIN-C19 | Account state/security epoch đổi giữa verify và session commit | Optimistic check từ chối; không token stale |
| LOGIN-C20 | Timing/redaction/enumeration sweep | Nonexistent/wrong/no-direct không phân biệt thực tế; không secret/PII trong response/log/audit |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả quan sát ngày 2026-08-20; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-LOGIN-I01 | `LoginDto` và repository lookup bằng `Username` | Trái email canonical login và tên hiển thị không duy nhất | M01-T023-A; M01-T042-A |
| M01-LOGIN-I02 | `LoginAsync` không kiểm tra `IsActive` hoặc account lifecycle state | Account bị khóa/ngừng/chờ xóa vẫn có thể nhận token | M01-T012, M01-T016–T018 |
| M01-LOGIN-I03 | Không có email verification/policy/consent state để kiểm tra | Có thể mở full quyền trước điều kiện | M01-T012, M01-T016; M01-T042-A |
| M01-LOGIN-I04 | Login success gọi tạo daily quest | Authentication gây side effect ngoài session và có thể làm login lỗi/đếm lặp | M07-T022–T025; M01-T042-A |
| M01-LOGIN-I05 | Refresh token lưu trực tiếp một cặp trên `User` và bị ghi đè mỗi login | Không hỗ trợ session family/multi-device/revocation đúng phạm vi | M01-T016–T018 |
| M01-LOGIN-I06 | Access token sống một ngày và chỉ có name/id/role claim | Claim stale lâu, thiếu session/security/account version | M01-T016–T018 |
| M01-LOGIN-I07 | Login chỉ audit thành công qua activity service | Không có failed/state/dependency outcome để điều tra | M01-T038, M01-T041; M11-T031–T035 |
| M01-LOGIN-I08 | Controller trả string `Invalid username or password` | Contract/public vocabulary còn dùng username và chưa versioned | M01-T042-A |
| M01-LOGIN-I09 | Không thấy dummy verifier hoặc timing-equalized nonexistent path | Có thể dò tồn tại qua timing | M01-T011; M01-T042-A |

## Finding còn mở

| Mã | Phần chưa chốt | Baseline an toàn | Nguồn/task xử lý |
|---|---|---|---|
| M01-LOGIN-F02 | Recovery proof/session handoff phía sau action ticket | Ticket 10 phút/purpose-bound theo M01-INACTIVE-1.0; không full session ngoài state; recovery không hạ chuẩn | M01-T016, M01-T019 |
| M01-LOGIN-F04 | Market/age/consent eligibility cuối | Policy unknown/stale không cấp session; không tự coi adult/consented | REL-01; M01-T012 |

## Tự kiểm M01-T010 và A-G01

- M01-T002/M01-T004 đã hoàn thành; email canonical, generic failure và eligibility gate khớp các contract trước.
- Request/pipeline/response/session handoff/audit boundary xác định rõ và không mở rộng sang threshold/session lifecycle.
- Hai mươi case bao phủ enumeration, canonicalization, state/policy, limiter, concurrency, audit/session failure, hash migration và timing/redaction.
- Chín finding triển khai và hai finding mở có baseline an toàn cùng task/module tiếp nhận; M01-LOGIN-F01 đã đóng bằng M01-ABUSE-1.0/M12-RATE-1.0/M12-FAIL-1.0, M01-LOGIN-F03 đã đóng bằng M01-SESSION-1.0.
- A-G01 vẫn mở vì code hiện tại chưa thực thi contract và chưa có runtime evidence.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt M01-LOGIN-1.0, eligibility/session boundary, 20 case và finding hiện trạng | WSA-7K2 |
