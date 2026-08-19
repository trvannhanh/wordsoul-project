# Luồng xác minh thư điện tử M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T006 |
| Contract ID / phiên bản | M01-VER-1.0 |
| Trạng thái | Baseline xác minh email có hiệu lực từ 2026-08-19 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-014, D-013; M01-D002; M12-D019; M01-REG-1.0; vòng đời tài khoản M01 v1.0 |
| Hợp đồng thư | MAIL-A-1.0 tối thiểu trong tài liệu này; M12-T026–T030 đầy đủ thuộc Giai đoạn B |
| Phạm vi | Phát hành, gửi/gửi lại, hết hạn, xác nhận, chống replay/abuse, chuyển trạng thái và privacy |
| Ngoài phạm vi | Đổi email M01-T020; khôi phục M01-T019–T021; hợp đồng delivery/bounce/complaint đầy đủ M12-T026–T030 |

## Quyết định và giới hạn

| Tham số | Giá trị M01-VER-1.0 |
|---|---|
| Purpose | `verify-registration-email` duy nhất; không tái dùng cho reset/đổi email |
| Intent TTL | 30 phút từ thời điểm phát hành |
| Mã xác minh | 10 ký tự Crockford Base32, không phân biệt hoa/thường, loại ký tự dễ nhầm; chỉ nhận trong request body |
| Lần thử | Tối đa 10 lần sai cho một intent; lần kế tiếp khóa intent |
| Cooldown resend | 60 giây cho account + canonical email |
| Hạn resend | Tối đa 5 dispatch request trong 24 giờ cho account + canonical email; limiter IP/device bổ sung theo M12-T034–T035 |
| Hiệu lực nhiều intent | Intent mới commit sẽ thu hồi mọi intent verify-registration-email cũ chưa consume |
| Lưu evidence | Chỉ lưu verifier/HMAC có server-side key và metadata; không lưu mã thô |
| Retention | Xóa verifier khi consume/revoke/hết hạn; metadata audit theo policy M01-D025, không chứa email/mã đầy đủ |

## Nguyên tắc bất biến

- `intentId` là định danh opaque không bí mật; mã xác minh mới là evidence. Mã không xuất hiện trong URL path/query/fragment, log, audit, analytics hoặc bằng chứng.
- Email chứa `intentId`/hướng dẫn và mã để người dùng nhập; client gửi `intentId + code` bằng POST body qua kênh bảo vệ.
- Xác minh email chứng minh quyền kiểm soát hộp thư tại thời điểm đó; không chứng minh danh tính pháp lý, tuổi, đồng ý hoặc quyền liên kết tài khoản khác.
- Mọi response public cho request/resend phải trung tính với account/email tồn tại hay không; không trả trạng thái delivery chi tiết.
- Consume intent và cập nhật trạng thái email/account là một transaction idempotent; concurrency chỉ có một kết quả chuyển trạng thái.
- Token/claim cũ không tự nâng quyền sau xác minh. Client phải lấy lại state/session theo M01-T016; API luôn kiểm tra trạng thái nguồn M01.
- Tài khoản khóa, ngừng hoạt động, chờ xóa hoặc đã xóa không được kích hoạt lại bằng mã xác minh.

## MAIL-A-1.0 — hợp đồng thư tối thiểu Giai đoạn A

Hợp đồng này chỉ đủ cho M01-T006/M01-T019 trong A; không thay thế contract delivery đầy đủ M12-T026–T030.

### Dispatch request

| Trường | Bắt buộc | Yêu cầu |
|---|---|---|
| `messageId` | Có | Opaque, duy nhất và là idempotency key cho một message intent |
| `purpose` | Có | `verify-registration-email`; allowlist, không nhận subject/body tùy ý từ client |
| `recipientRef` | Có | Internal protected reference; adapter resolve canonical email qua quyền tối thiểu |
| `templateId` / `templateVersion` | Có | Template đã đăng ký, nội dung/biến có allowlist |
| `locale` | Có | Locale hỗ trợ hoặc fallback đã công bố; không lấy từ payload provider |
| `variables` | Có | Chỉ display name đã xử lý, `intentId`, code và expiry display; không password/token/profile đầy đủ |
| `expiresAt` | Có | Không dispatch/retry sau expiry |
| `correlationId` | Có | Opaque, không PII; dùng trace/audit |

### Dispatch result

| Trạng thái | Ý nghĩa | Hành vi M01 |
|---|---|---|
| `accepted` | Adapter/provider đã nhận request hợp lệ | Ghi dispatch accepted; không coi delivered/verified |
| `temporaryFailure` | Có thể retry trong hạn | Retry cùng `messageId` theo backoff/budget; không tạo intent mới |
| `permanentFailure` | Địa chỉ/request không thể gửi theo contract | Dừng retry; account vẫn chờ xác minh; cho hành trình sửa/khôi phục đúng task |
| `unknown` | Timeout/mất response, chưa biết provider nhận hay chưa | Reconcile/retry cùng `messageId`; không tạo message/intent mới |

### Idempotency và privacy của adapter

- Cùng `messageId` + cùng fingerprint trả cùng semantic result, không tạo hiệu ứng nghiệp vụ thứ hai; fingerprint khác bị conflict.
- Retry dừng khi intent hết hạn, bị revoke/consume, account không còn hợp lệ hoặc destination bị vô hiệu.
- Provider delivery/bounce/complaint không trực tiếp chuyển account sang verified; chỉ M01 consume code hợp lệ được phép.
- Adapter không log recipient đầy đủ, code, template output, provider response body hoặc message content; log allowlist `messageId`, template/version, status category, attempt, latency và correlation.
- `accepted` không được hiển thị cho user như “đã giao”; response public chỉ nói yêu cầu đã được tiếp nhận nếu phù hợp.

## Mô hình intent

| Trường logic | Quy tắc |
|---|---|
| `intentId` | Opaque, duy nhất, không tuần tự; được phép dùng như reference không bí mật |
| `accountId` / `emailVersion` | Gắn đúng account và phiên bản canonical email; đổi email làm intent cũ vô hiệu |
| `purpose` | Cố định `verify-registration-email`; so khớp bắt buộc khi consume |
| `codeVerifier` | HMAC/verifier của code bằng server-side key; constant-time compare; không dùng hash nhanh không khóa để lưu code ngắn |
| `issuedAt` / `expiresAt` | Thời điểm chuẩn; expiry = issued + 30 phút |
| `attemptCount` | Tăng atomically khi code sai; khóa sau 10 lần |
| `generation` | Tăng theo resend; chỉ generation mới nhất có thể consume |
| `status` | `pending`, `dispatchAccepted`, `consumed`, `expired`, `revoked`, `locked` |
| `messageId` | Tham chiếu dispatch MAIL-A-1.0, không dùng delivery status làm verification truth |
| `policyVersion` | M01-VER-1.0 và account policy set để quyết định trạng thái sau xác minh |

## Luồng phát hành và gửi

1. Registration transaction tạo account `Chờ xác minh thư`, intent generation đầu, verifier, audit metadata và outbox MAIL-A-1.0 trong cùng commit logic.
2. Worker đọc outbox, kiểm tra account/email version/intent status/expiry rồi dispatch bằng `messageId` cố định.
3. `accepted` chỉ cập nhật dispatch metadata. `temporaryFailure`/`unknown` retry cùng message ID; `permanentFailure` dừng và giữ account pending.
4. Worker không tạo account, thay trạng thái xác minh hoặc cấp session/tài sản.
5. Nếu message hết hạn trước dispatch thành công, intent chuyển expired và người dùng phải resend theo limiter.

## Luồng gửi lại

| Bước | Yêu cầu |
|---|---|
| Tiếp nhận | Dùng registration receipt hoặc limited authenticated account reference + idempotency key; response luôn trung tính |
| Kiểm tra | Cooldown 60 giây, budget 5/24 giờ, limiter nguồn và account state; không tiết lộ điều kiện nào bị trúng |
| Mutation | Trong transaction: tạo generation/code/message mới, revoke intent cũ, ghi outbox và audit metadata |
| Retry | Cùng resend idempotency key trả cùng intent/message semantic result; không tăng budget lần hai |
| Đã verified/không tồn tại | Trả envelope trung tính, không tạo message hoặc tiết lộ trạng thái |
| Provider lỗi | Account vẫn pending; retry cùng message, không tự tạo generation mới |

## Luồng xác nhận

1. Nhận strict POST body gồm contract version, `intentId`, `code` và correlation; không nhận account ID/email/role/state từ client.
2. Áp limiter trước lookup sâu; chuẩn hóa code chỉ bằng uppercase và loại separator được UI công bố, không sửa ký tự khác.
3. Trong transaction, khóa intent và kiểm tra purpose, generation mới nhất, account/email version, state, expiry, status và attempt count.
4. So sánh verifier constant-time. Sai thì tăng attempt atomically; lần sai thứ 10 chuyển `locked`. Response không phân biệt sai/hết hạn/revoked/không tồn tại.
5. Đúng thì consume intent một lần, revoke intent cùng purpose còn lại, ghi `emailVerifiedAt`/email version đã verified và audit metadata.
6. Nếu mọi điều kiện bắt buộc hiện hành đạt, chuyển account sang `Hoạt động`; nếu REL-01 còn yêu cầu, chuyển `Chờ điều kiện tuổi/đồng ý`. Trạng thái khóa/xóa không đổi.
7. Retry sau commit trả semantic result idempotent, không tiêu thụ lại hoặc phát event quyền hai lần.

## Response và lỗi an toàn

| Hành trình | HTTP/semantic public | Ghi chú |
|---|---|---|
| Request/resend hợp lệ về schema | `202 accepted` | Giống nhau cho account tồn tại, đã verified hoặc không tồn tại |
| Verify thành công | `200 verified` với nextAction tổng quát | Không trả token/role/quyền/tài sản; client refresh state qua flow riêng |
| Verify đã thành công, retry cùng intent | `200 verified` idempotent | Không phát event/chuyển trạng thái lần hai |
| Code/intent sai, hết hạn, revoked, locked hoặc không tồn tại | `400 VER_INVALID_OR_EXPIRED` | Một lỗi chung, không tiết lộ account/intent status |
| Rate limit | `429 VER_RATE_LIMITED` | Retry hint không tiết lộ account/email |
| Policy/state không cho phép kích hoạt | `409 VER_STATE_CONFLICT` hoặc lỗi chung theo authenticated context | Public/anonymous không được biết state nội bộ |
| Store/audit bắt buộc lỗi trước consume | `503 VER_TEMPORARILY_UNAVAILABLE` | Fail-closed, không consume/chuyển trạng thái nửa vời |

## Quyền trước và sau xác minh

| Năng lực | Chờ xác minh thư | Sau xác minh email |
|---|---|---|
| Xem học liệu/học giới hạn | Được theo M01-D002 và policy phát hành | Theo trạng thái/role/policy hiện hành |
| Xuất bản, xã hội, PvP | Từ chối | Chỉ mở nếu account `Hoạt động` và quyền khác đạt |
| Thông báo ngoài ứng dụng không bắt buộc | Từ chối | Theo consent/preference; verification không tự tạo consent |
| Thay đổi email/credential/khôi phục | Chỉ hành trình tối thiểu được phép và có xác minh lại phù hợp | Theo M01-CRED/M01-T019–T020 |
| Tài sản/onboarding | Không cấp dựa trên verify callback | M01-T008/M06 xử lý idempotent theo điều kiện riêng, không AP |
| Session/token | Không cấp scope đầy đủ; claim cũ không là truth | Phải lấy session/state mới; API vẫn kiểm tra account state |

## Audit và observability

| Event | Metadata allowlist |
|---|---|
| `verification.intent.created` | Event/intent/message protected reference, generation, policy version, expiry bucket, correlation |
| `verification.dispatch.result` | Message reference, status category, attempt, latency, template/version, correlation |
| `verification.resend.accepted_or_limited` | Result category, limiter bucket, generation metadata; không recipient/code |
| `verification.attempt.rejected` | Generic reason category, attempt bucket, source limiter, correlation; không code/email |
| `verification.completed` | Subject reference, email version, before/after account state, policy version, correlation |

- Không log/audit email đầy đủ, code/verifier, URL bí mật, template body, provider response body hoặc request DTO.
- Chỉ số tối thiểu: intent created, accepted/temporary/permanent/unknown, age-to-dispatch, verify success/reject/expired/locked, resend limited và queue age.
- Mutation consume/state transition fail-closed khi audit bắt buộc không ghi được; dispatch observability lỗi không được biến `accepted` giả nhưng dùng recovery/buffer theo M11.

## Ma trận nghiệm thu M01-VER-1.0

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| VER-C01 | Registration hợp lệ | Một intent/outbox, account pending, không token/full permission |
| VER-C02 | Dispatch accepted | Chỉ đánh dấu accepted; không verified/delivered |
| VER-C03 | Temporary failure/timeout unknown | Retry cùng message ID, không tạo email intent thứ hai |
| VER-C04 | Permanent failure | Dừng retry, account vẫn pending, không báo delivered |
| VER-C05 | Code đúng trong 30 phút | Consume một lần, ghi email version verified và chuyển state đúng policy |
| VER-C06 | Code sai 9 lần rồi đúng | Lần đúng được phép nếu intent còn hạn; attempt atomically đúng |
| VER-C07 | Code sai lần thứ 10 | Intent locked; mọi lần sau trả lỗi chung |
| VER-C08 | Code hết hạn | Lỗi chung, không đổi state; resend tạo generation mới |
| VER-C09 | Replay code đã consume | Kết quả idempotent nếu cùng intent/subject; không phát effect lần hai |
| VER-C10 | Sai purpose/account/email version | Lỗi chung, không tiết lộ mismatch và không consume intent khác |
| VER-C11 | Resend trước 60 giây hoặc vượt 5/24 giờ | Response trung tính/limited; không tạo generation/outbox |
| VER-C12 | Resend hợp lệ | Intent cũ revoked, generation mới duy nhất, retry idempotent |
| VER-C13 | Hai verify đồng thời cùng code | Chỉ một transaction consume/state event; request kia idempotent/conflict an toàn |
| VER-C14 | Account bị khóa/ngừng hoạt động/chờ xóa trước verify | Không kích hoạt lại hoặc cấp session; state giữ an toàn |
| VER-C15 | REL-01 yêu cầu điều kiện còn thiếu | Email verified nhưng account chuyển/giữ `Chờ điều kiện tuổi/đồng ý` |
| VER-C16 | Audit/store lỗi trước consume | Fail-closed, không cập nhật một phần; retry an toàn |
| VER-C17 | Request/resend cho email không tồn tại/đã verified | Cùng `202` trung tính, không tạo message và không lộ trạng thái |
| VER-C18 | Inspect URL/log/audit/error/provider telemetry | Không có code/verifier/email đầy đủ/template body/provider body |
| VER-C19 | Token/claim cũ sau verify | Không tự có quyền mới; API dựa state và session mới |
| VER-C20 | Worker retry sau expiry/revoke/consume | Dừng dispatch, không gửi email lỗi thời |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-19; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-VER-I01 | `User` không có email verification state/version/timestamp | Không thể cưỡng chế hoặc giải thích quyền trước/sau xác minh | M01-T009, M01-T012, M01-T033 |
| M01-VER-I02 | Không tìm thấy verification intent/verifier/generation/attempt entity hoặc endpoint | Không có expiry, one-time consume, replay/concurrency protection | M01-T009, M01-T038 |
| M01-VER-I03 | Registration hiện không tạo verification outbox/dispatch | Account được tạo mà không có hành trình verify | M01-T008, M01-T009 |
| M01-VER-I04 | `IEmailService.SendEmailAsync` chỉ nhận email/subject/html và trả `Task` | Không có message ID, template/version, expiry, idempotency hoặc semantic result | M12-T026–T030; M01-T009 |
| M01-VER-I05 | `SendGridEmailService` nuốt exception và không trả failure/status cho caller | Caller có thể không biết accepted/failed/unknown và báo thành công giả | M12-T005, M12-T026, M12-T029 |
| M01-VER-I06 | Email service log recipient đầy đủ ở success/failure/exception | Vi phạm privacy/redaction và A-G05 dù M01-T006 chỉ tự kiểm A-G01 | M11-T033, M12-T041 |
| M01-VER-I07 | Email service nhận subject/html tùy ý thay vì template allowlist | Khó kiểm soát nội dung, biến nhạy cảm và version | M12-T026, M12-T042 |
| M01-VER-I08 | Không thấy resend limiter/budget/idempotency hoặc delivery reconciliation | Dễ spam, gửi lặp và không xử lý unknown/timeout an toàn | M01-T011; M12-T029, M12-T034–T035 |
| M01-VER-I09 | `AuthController`/`AuthService` hiện có thể trả user/token mà không kiểm tra email verified | Quyền đầy đủ có thể mở trước xác minh | M01-T009, M01-T010, M01-T016 |

## Finding còn mở có trạng thái an toàn

| Mã | Phần chưa chốt | Baseline an toàn hiện hành | Nguồn/task xử lý |
|---|---|---|---|
| M01-VER-F01 | Provider/template/locale/region/retention và delivery event đầy đủ | Chỉ dùng MAIL-A-1.0 allowlist; không suy `accepted = delivered`; không log recipient/content | M12-T026–T030, M12-T042; REL-03 |
| M01-VER-F02 | Entry-point allowlist chi tiết cho học giới hạn | Chỉ scope M01-D002; mọi social/PvP/publish/out-of-app notification bị từ chối | M01-T009; M03/M08/M09/M10 |
| M01-VER-F03 | Session refresh/reissue chính xác sau verify | Không nâng claim/token cũ; API kiểm tra state, full session chờ contract | M01-T016–T018 |
| M01-VER-F04 | Policy tuổi/đồng ý sau email verified | Không chuyển Active khi REL-01 yêu cầu điều kiện chưa đạt | REL01-Q01–Q06; M01-T007 |

## Tự kiểm M01-T006 và A-G01

- M01-T002/M01-T005 đã hoàn thành; MAIL-A-1.0 cung cấp dependency thư tối thiểu và giữ M12-T026 đầy đủ ở Giai đoạn B.
- TTL/cooldown/budget/attempt/code/invalidation được chốt; intent, dispatch, resend, consume và state transition có boundary idempotent.
- MAIL-A-1.0 phân biệt bốn semantic result, `accepted` với `delivered` và quy tắc privacy/retry tối thiểu.
- Hai mươi case bao phủ expiry, replay, sai purpose/account, resend, limiter, concurrency, dependency/audit failure, locked/delete state và stale claim.
- Chín finding triển khai và bốn finding contract/policy có baseline an toàn cùng task tiếp nhận; không còn khoảng trống vô chủ trong M01-T006.
- A-G01 vẫn mở: chưa có triển khai, runtime evidence hoặc REL-01/REL-07 kết luận. Tài liệu này không kết luận gate đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo hợp đồng intent/evidence, quyền giới hạn và nhánh lỗi | Chưa gán |
| 2026-08-19 | 1.0 | Chốt M01-VER-1.0, MAIL-A-1.0, limiter/expiry/replay, 20 case và finding hiện trạng | WSA-7K2 |
