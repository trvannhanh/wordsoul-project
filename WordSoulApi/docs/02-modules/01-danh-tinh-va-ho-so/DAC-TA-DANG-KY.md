# Đặc tả dữ liệu đăng ký M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T005 |
| Contract ID / phiên bản | M01-REG-1.0 |
| Trạng thái | Baseline đăng ký trực tiếp có hiệu lực từ 2026-08-19; policy tuổi/đồng ý tiếp tục bị điều kiện bởi REL-01 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-013, D-012; M01-D002–D005, M01-D011; bản đồ dữ liệu M01 v1.0; M01-CRED-1.0 |
| Phạm vi | Request/response đăng ký trực tiếp, canonicalization, conflict, idempotency, trạng thái ban đầu và lỗi an toàn |
| Ngoài phạm vi | Xác minh email chi tiết M01-T006; consent record M01-T007; khởi tạo quyền lợi M01-T008; đăng nhập ngoài M01-T013–T015 |

## Nguyên tắc bất biến

- Email canonical là định danh đăng nhập trực tiếp; tên hiển thị là thuộc tính hồ sơ có thể trùng và không dùng để xác thực.
- Request không được tự đặt account ID, role, quyền, trạng thái, email-verified, provider link, XP/AP/hint, vật phẩm, thú cưng hoặc tài sản khác.
- Đăng ký thành công chỉ tạo danh tính/tài khoản ở `Chờ xác minh thư`; không trả access token/refresh credential và không tự chuyển `Hoạt động`.
- Email đã tồn tại không tự liên kết, không tạo tài khoản thứ hai và không làm phản hồi public tiết lộ sự tồn tại.
- API không tạo quyền lợi M06/M07 inline. Hậu xử lý dùng contract idempotent của M01-T008; AP luôn bị đóng băng theo D-011.
- Khi không xác định được policy tuổi/thị trường/đồng ý áp dụng, fail-closed trước khi lưu dữ liệu cá nhân; không tự dùng giả định “mọi độ tuổi”.
- Request/response/log/audit không chứa password, verifier, token, payload provider hoặc dữ liệu cá nhân ngoài allowlist.

## Envelope request

| Thành phần | Bắt buộc | Contract | Xử lý |
|---|---|---|---|
| Contract version | Có | `M01-REG-1.0` qua version endpoint/header đã thống nhất | Version không hỗ trợ bị từ chối trước mutation |
| `Idempotency-Key` | Có | 16–64 ký tự ASCII `[A-Za-z0-9._:-]`, opaque, không chứa PII | Scope theo client + endpoint; giữ kết quả/fingerprint tối thiểu 24 giờ |
| Content type | Có | JSON UTF-8, body theo allowlist | Field không biết hoặc trùng key bị từ chối; không bind vào domain entity |
| Correlation ID | Hệ thống bảo đảm | Opaque, không chứa PII; client có thể gửi theo contract chung | Dùng cho trace/audit, không thay idempotency key |

## Schema dữ liệu đầu vào

| Trường logic | Bắt buộc | Chuẩn hóa và validation | Lưu trữ/nguồn thật | Lỗi an toàn |
|---|---|---|---|---|
| `email` | Có | Address-only; trim ASCII space/tab ở hai đầu; local-part ASCII 1–64; domain chuyển IDNA ASCII và lowercase; canonical lowercase toàn địa chỉ; tổng canonical tối đa 254 ký tự; không comment/display-name/control/newline | Lưu canonical email tại M01; không dùng làm public ID | Invalid trả field error; collision/trùng trả envelope tiếp nhận trung tính |
| `password` | Có | Giữ nguyên chuỗi, không trim/normalize; áp dụng M01-CRED-1.0 gồm 12–128 scalar, blocklist và breached check | Chỉ ghi verifier bảo vệ; không lưu chuỗi thô | Policy reject không phản chiếu giá trị; dependency check lỗi thì fail-closed |
| `displayName` | Có | NFC; trim khoảng trắng ngoài và gộp khoảng trắng liên tiếp; 3–50 Unicode scalar; không control/newline; moderation/cooldown đầy đủ thuộc M01-T023-A | Hồ sơ M01; không bắt buộc duy nhất | Tên trùng được chấp nhận; không tiết lộ hồ sơ khác |
| `policySetId` | Có khi REL-01 đã freeze | Opaque/versioned, phải khớp policy registry cho phạm vi phát hành | M01/policy registry | Thiếu, hết hiệu lực hoặc không khớp trả policy error; không tạo account |
| `regionCode` | Chỉ khi policy yêu cầu | Giá trị allowlist do REL-01 duyệt; không suy ra từ IP làm nguồn thật | M01 | Không thu thập trước khi có mục đích/căn cứ; giá trị ngoài allowlist bị từ chối |
| `ageBand` | Chỉ khi policy yêu cầu | Enum allowlist do REL-01 duyệt; không nhận ngày sinh nếu chưa có quyết định | M01 | Thiếu/không hợp lệ không mở năng lực hoặc tạo account đầy đủ |
| `consents` | Chỉ khi policy yêu cầu | Mỗi mục gồm type, policyVersion, decision và source theo M01-T007; không gộp mục đích | M01 append-only consent record | Thiếu consent bắt buộc hoặc version cũ bị từ chối trước mutation |

## Field bị cấm trong request

| Nhóm | Ví dụ | Hành vi |
|---|---|---|
| Định danh/trạng thái do hệ thống cấp | `id`, `userId`, `isActive`, `emailVerified`, `createdAt` | Từ chối request, không bỏ qua âm thầm |
| Quyền/quản trị | `role`, `permissions`, `isAdmin`, `isSuperAdmin` | Từ chối và ghi security metadata phù hợp, không echo giá trị |
| Tài sản/tiến độ | `xp`, `ap`, `hintBalance`, `starterPetId`, `items`, `achievements` | Từ chối; xử lý onboarding/quyền lợi qua M01-T008/M06 |
| Danh tính ngoài | `provider`, `providerSubject`, `providerEmail`, provider token | Từ chối; dùng endpoint/hợp đồng OAuth riêng |
| Bí mật/session | `passwordHash`, `refreshToken`, `accessToken`, reset/verification token | Từ chối và không ghi log/body |

## Canonicalization và conflict boundary

1. Parse strict envelope, kiểm tra version, kích thước và allowlist trước khi tạo domain object.
2. Canonicalize email/display name theo đúng thứ tự; password không bị biến đổi.
3. Resolve `policySetId` và yêu cầu REL-01; nếu policy chưa xác định/không khả dụng thì dừng trước khi lưu PII.
4. Kiểm tra M01-CRED-1.0 và anti-abuse/rate limit; không gọi breached-password bằng giá trị thô.
5. Mở idempotency boundary; tính fingerprint bảo vệ trên canonical request bằng server-side key, không lưu/log password hay fingerprint có thể dùng offline.
6. Trong transaction, dùng unique constraint trên canonical email và idempotency key; check-then-insert ở application không thay thế constraint.
7. Nếu email đã tồn tại, không mutate/liên kết và tạo response trung tính tương đương request mới được tiếp nhận.
8. Nếu email mới, ghi account `Chờ xác minh thư`, verifier, policy/consent cần thiết, audit metadata và outbox yêu cầu xác minh trong một commit logic.
9. Sau commit, email worker và M01-T008 xử lý idempotent. Lỗi phụ thuộc không rollback danh tính đã commit và không báo tài khoản đã hoạt động.

## Idempotency và concurrency

| Tình huống | Kết quả bắt buộc |
|---|---|
| Cùng key + cùng fingerprint trong 24 giờ | Trả cùng status/envelope/receipt, không lặp mutation hoặc side effect |
| Cùng key + fingerprint khác | `409 REG_IDEMPOTENCY_CONFLICT`, không mutate |
| Khác key + cùng canonical email tuần tự/đồng thời | Tối đa một account; mọi response public hợp lệ vẫn trung tính |
| Timeout sau commit | Retry lấy lại receipt/kết quả cũ; không tạo account hoặc outbox trùng |
| Worker email/khởi tạo retry | Dedupe theo account + operation/version; không gửi/cấp lặp ngoài policy |
| Key hết retention | Request mới vẫn chịu unique email; không dùng key hết hạn để tạo account thứ hai |

## Response public

### Request hợp lệ về schema và được tiếp nhận

- HTTP `202 Accepted` cho cả email mới và canonical email đã tồn tại.
- Envelope chỉ gồm `receiptId` opaque, `status = accepted`, `nextAction` trung tính, contract version và correlation ID.
- Không trả account ID, email đầy đủ, role, trạng thái nội bộ, verification state chi tiết, token, tài sản hoặc kết quả “email đã tồn tại”.
- `nextAction` hướng người dùng kiểm tra email hoặc đăng nhập nếu đã có tài khoản; không xác nhận email đã được gửi.

### Lỗi trước mutation

| HTTP | Code | Khi dùng | Rò rỉ bị cấm |
|---:|---|---|---|
| 400 | `REG_INVALID_REQUEST` | JSON/version/field không hợp lệ hoặc có field bị cấm | Không echo body/field bí mật |
| 422 | `REG_CREDENTIAL_POLICY_REJECTED` | Password không đạt policy nhưng security check chạy được | Không echo password/blocklist/breach detail |
| 422 | `REG_POLICY_REQUIREMENT_MISSING` | Policy đã xác định nhưng context/consent bắt buộc thiếu hoặc sai version | Không suy diễn luật ngoài policySet |
| 409 | `REG_IDEMPOTENCY_CONFLICT` | Cùng key dùng cho intent khác | Không trả fingerprint/body cũ |
| 429 | `REG_RATE_LIMITED` | Vượt limiter theo M01-T011/M12 | Không tiết lộ account/email tồn tại |
| 503 | `REG_POLICY_UNAVAILABLE` | Chưa có/không resolve được policy phát hành | Không tạo account hoặc lưu context chưa duyệt |
| 503 | `REG_SECURITY_CHECK_UNAVAILABLE` | Không thể chạy blocklist/breached check bắt buộc | Không hạ chuẩn hoặc tự bỏ qua |

Sau khi transaction đã commit, lỗi gửi email/worker không đổi `202` thành lỗi mơ hồ khiến client tạo attempt mới; trạng thái nội bộ và resend được xử lý qua receipt/hành trình M01-T006.

## Audit và dữ liệu được phép

| Sự kiện | Metadata allowlist | Dữ liệu luôn loại |
|---|---|---|
| Attempt accepted/rejected | Event ID, thời điểm, policy/contract version, result/reason code, correlation, idempotency reference bảo vệ, limiter bucket | Password, verifier, email đầy đủ, display name, body, consent payload thô |
| Account created | Internal subject reference, state mới, policy version, transaction/outbox reference | Password/verifier, token, email/display name đầy đủ |
| Duplicate/collision | Kết quả chung và constraint/category | Canonical email, account ID đối chiếu trong log public, dữ liệu account hiện có |
| Worker email/init | Operation ID, account reference bảo vệ, attempt/result/retry count | Email body, token/link, provider response, tài sản payload |

## Ma trận nghiệm thu M01-REG-1.0

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| REG-C01 | Email/password/display name biên hợp lệ, policy context hợp lệ | Một account `Chờ xác minh thư`, outbox một lần, response `202` không token |
| REG-C02 | Email khác hoa/thường hoặc Unicode domain cùng canonical | Tối đa một account; response trùng trung tính |
| REG-C03 | Hai request đồng thời, key khác, cùng canonical email | Unique constraint chọn một mutation; không lỗi lộ account |
| REG-C04 | Cùng key/cùng body retry | Cùng receipt/response, không lặp account/outbox |
| REG-C05 | Cùng key/body khác | `409`, không mutate và không lộ fingerprint cũ |
| REG-C06 | Timeout sau commit rồi retry | Thu hồi kết quả cũ, không tạo account thứ hai |
| REG-C07 | Email sai cấu trúc/quá dài/control | `400`, không lưu PII và không gọi downstream |
| REG-C08 | Hai người dùng dùng cùng display name hợp lệ | Cả hai có thể đăng ký; định danh nội bộ phân biệt |
| REG-C09 | Display name có Unicode/khoảng trắng | Chuẩn hóa NFC/whitespace đúng một lần; không đổi password |
| REG-C10 | Password ở biên 12/128 và 11/129 | Khớp M01-CRED-1.0, không cắt/trim |
| REG-C11 | Password phổ biến/đã lộ hoặc security check outage | Reject/fail-closed, không tạo verifier/account và không lộ password |
| REG-C12 | REL-01/policy resolver chưa có kết luận | `503 REG_POLICY_UNAVAILABLE`, không tạo account hoặc lưu tuổi/consent |
| REG-C13 | Consent bắt buộc thiếu/sai version | `422`, không tạo account; không tự coi consent=true |
| REG-C14 | Request chứa role/status/AP/starter pet/token | `400`, không bind hoặc mutate field bị cấm |
| REG-C15 | Email đã tồn tại | `202` envelope trung tính; không liên kết, không đổi verifier hoặc gửi dữ liệu account |
| REG-C16 | Email worker lỗi sau commit | Account vẫn pending; retry/resend idempotent, không báo active/full success |
| REG-C17 | Post-create M01-T008/M06 retry | Không cấp achievement/pet/hint lặp và không tạo AP |
| REG-C18 | Log/audit/exception cho success/reject/conflict/outage | Không có password/verifier/token/body/email đầy đủ; chỉ metadata allowlist |
| REG-C19 | Response accepted và mọi error | Không có account ID, role, internal state, token, tài sản hoặc dấu hiệu email tồn tại |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-19; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-REG-I01 | `RegisterDto` nhận `Username`, `Email`, `Password`, `StarterPetId` và không có validation | Contract trộn hồ sơ/tài sản, thiếu allowlist/canonicalization/policy | M01-T008, M01-T009 |
| M01-REG-I02 | `Username` được kiểm tra duy nhất và dùng như định danh đăng nhập | Mâu thuẫn email-login/tên hiển thị không duy nhất theo M01-D003/D011 | M01-T009, M01-T010, M01-T023-A |
| M01-REG-I03 | Email được so sánh trực tiếp trong repository; `User.Email` giới hạn 100 ký tự | Không có canonicalization/IDNA contract; collision hoa-thường phụ thuộc DB collation | M01-T009, M01-T010, M01-T033 |
| M01-REG-I04 | Không thấy unique index cho canonical email/username trong cấu hình `User` đã rà | Check-then-insert có race và có thể tạo danh tính trùng | M01-T009; M11-T021 |
| M01-REG-I05 | `RegisterAsync` tạo `User` mặc định `IsActive = true`; không có trạng thái xác minh bền vững | Có thể mở quyền trước xác minh/policy | M01-T006, M01-T009, M01-T012 |
| M01-REG-I06 | Endpoint trả `200 UserDto` gồm ID, email, role và `IsActive`; duplicate trả `400` | Không có envelope trung tính và có thể hỗ trợ account enumeration | M01-T009 |
| M01-REG-I07 | Không có idempotency key/store/fingerprint được quan sát | Retry/timeout/concurrency có thể tạo side effect hoặc danh tính trùng | M01-T008, M01-T009; M11-T021 |
| M01-REG-I08 | Account được commit trước `InitializeNewUserAsync`; quyền lợi/achievement/pet chạy inline sau commit | Partial failure và retry boundary chưa có receipt/outbox rõ | M01-T008; M06 contract |
| M01-REG-I09 | Không có field/record tuổi, khu vực, policySet hoặc consent | Không thể áp dụng/giải thích REL-01; không được coi default là hợp lệ | M01-T007, M01-T033; REL-01 |
| M01-REG-I10 | Không thấy verification outbox/trạng thái email hoặc security policy service trong luồng đăng ký | Chưa đáp ứng xác minh, M01-CRED-1.0 và dependency failure contract | M01-T006, M01-T009; M12 email contract |

## Finding còn mở có trạng thái an toàn

| Mã | Phần chưa chốt | Baseline an toàn hiện hành | Nguồn/task xử lý |
|---|---|---|---|
| M01-REG-F01 | Giá trị cụ thể cho market/ageBand/consent và policySet phát hành | Không tạo account nếu không resolve được policy; không thu ngày sinh/consent giả | REL01-Q01–Q06; M01-T007 |
| M01-REG-F02 | Contract gửi/xác minh email, TTL và resend | Commit account pending + outbox idempotent; không trả active/token | M01-T006; M12 contract tối thiểu |
| M01-REG-F03 | Contract khởi tạo quyền lợi và compensation | Không nhận starterPet/tài sản trong registration; job idempotent, không AP | M01-T008; M06; chuẩn bị B-G03 |
| M01-REG-F04 | Schema vật lý/idempotency store/unique constraint và retention sau 24 giờ | 24 giờ là minimum contract; thiếu durable constraint/store thì không mở endpoint v1 | M01-T008–T009; M11-T021 |

## Tự kiểm M01-T005, A-G01 và REL-01

- M01-T003/M01-T004 đã hoàn thành; dependency của task được đáp ứng.
- Bảy nhóm input được allowlist, năm nhóm field bị cấm, canonicalization email/display name và password bất biến được định nghĩa đo được.
- Sáu tình huống idempotency/concurrency, bảy error code và response `202` trung tính ngăn duplicate/account enumeration theo contract.
- Mười chín case bao phủ boundary, canonical collision, concurrency, timeout, policy/consent, dependency error, side effect và redaction.
- Mười finding triển khai và bốn finding policy/contract có baseline an toàn cùng task tiếp nhận; không còn khoảng trống vô chủ trong M01-T005.
- REL-01 vẫn mở: market/age/consent/policySet chưa được tự chốt. Tài liệu này không kết luận A-G01/REL-01 đạt và không thay thế triển khai hoặc kiểm thử runtime.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema, luồng quyết định và ca nghiệm thu đăng ký | Chưa gán |
| 2026-08-19 | 1.0 | Chốt M01-REG-1.0, canonicalization, idempotency, lỗi an toàn, 19 case và finding hiện trạng | WSA-7K2 |
