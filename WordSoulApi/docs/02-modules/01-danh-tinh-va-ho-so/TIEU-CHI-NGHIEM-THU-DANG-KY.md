# Tiêu chí nghiệm thu đăng ký M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T009 |
| Suite ID / phiên bản | M01-REG-ACC-1.0 |
| Trạng thái | Baseline nghiệm thu đăng ký có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Contract nguồn | M01-REG-1.0, M01-VER-1.0, MAIL-A-1.0, M01-CONS-1.0, M01-ONB-1.0, M06-ONB-A-1.0 |
| Phạm vi | Đăng ký trực tiếp xuyên validation, policy/consent, account commit, xác minh email và onboarding |
| Ngoài phạm vi | Đăng nhập ngoài M01-T013–T015; triển khai test/runtime; kết luận pháp lý REL-01; gate A-G01 toàn phần |

## Điều kiện vào và nguyên tắc đạt

| Nhóm | Điều kiện |
|---|---|
| Build | Schema/API/worker/provider adapter dưới kiểm thử có version xác định; migration áp dụng trên database sạch và snapshot gần production |
| Dữ liệu | Chỉ fixture giả, catalog/policy/template versioned; không dùng PII, secret hoặc provider payload thật |
| Dependency | Có fake/contract double điều khiển được success, reject, timeout, unknown, retry và duplicate cho mail/M06/policy/audit |
| Clock/concurrency | Clock injectable; chạy được expiry/deadline, parallel requests, worker crash/restart và transaction fault |
| Quan sát | Truy vết bằng correlation/operation protected refs; kiểm tra được outbox, audit allowlist và absence của dữ liệu cấm |

Một case chỉ đạt khi assertions về response, durable state, side effect, audit/log và retry state cùng đạt. HTTP thành công đơn lẻ không đủ. Suite fail nếu có một critical case thất bại, có account/tài sản lặp, mở quyền sớm, tự grant consent, lộ secret/PII hoặc tạo AP mới.

## Tầng kiểm thử bắt buộc

| Tầng | Mục tiêu | Tối thiểu |
|---|---|---|
| Unit/property | Canonicalization, strict schema, password/code boundary, resolver và state transition | Boundary + generated variants cho Unicode/case/whitespace và invalid transition |
| Contract | Mail, policy resolver, M06 onboarding, semantic error/idempotency | Success/reject/temporary/unknown; cùng key cùng/khác fingerprint |
| Integration | Database unique/transaction/outbox/audit, optimistic concurrency | Database thật; parallel request và injected failure trước/sau commit |
| Worker/recovery | Dispatch, retry, lease/fencing, restart và reconcile | Crash trước/sau remote effect; stale/expired/cancelled work |
| API journey | Response trung tính, nextAction và quyền theo state | Anonymous/limited/authenticated contexts; không tin payload truth |
| Privacy/security | Redaction, enumeration, replay, rate limit và forbidden fields | Inspect response/log/audit/telemetry/store với dữ liệu giả đánh dấu |

## Ma trận nghiệm thu xuyên luồng

| Case ID | Kịch bản | Assertions bắt buộc | Tầng |
|---|---|---|---|
| RGA-C01 | Happy path: policy resolve → register → dispatch → verify → consent đủ → starter | Một account; state đúng từng bước; một intent/message/onboarding/M06 operation; không token/tài sản trước điều kiện | Integration + journey |
| RGA-C02 | Email canonical collision tuần tự | Response trung tính như accepted; không account/intent mới, không tự liên kết | API + database |
| RGA-C03 | Hai đăng ký đồng thời cùng canonical email | Unique durable chọn một account; hai response không lộ winner/identity | Integration concurrency |
| RGA-C04 | Retry cùng registration key/body | Cùng receipt/response; không lặp account/outbox/onboarding | Integration idempotency |
| RGA-C05 | Cùng registration key khác body | Conflict; không mutate và không lộ fingerprint cũ | API + database |
| RGA-C06 | Timeout sau account commit rồi retry | Thu hồi result cũ; một account/receipt/outbox | Recovery |
| RGA-C07 | Schema/forbidden fields gồm role/AP/starter/token | Reject trước mutation/downstream; không mass assignment | Unit + API |
| RGA-C08 | Email/display name/password boundary | Đúng canonical/NFC/whitespace và 12–128; invalid không lưu PII | Unit/property + API |
| RGA-C09 | Password phổ biến/đã lộ và checker outage | Reject/fail-closed; không account/verifier/log secret | Contract + API |
| RGA-C10 | Policy resolver unavailable/ambiguous | `CONS/REG_POLICY_UNAVAILABLE`; không account/decision | Contract + API |
| RGA-C11 | Required acknowledgement thiếu/decline | Không tự grant/kích hoạt; response/consequence đúng policy | Journey |
| RGA-C12 | Optional consent decline, purpose tách biệt | Core chỉ mở theo policy; optional purpose tắt, ledger decision độc lập | Integration |
| RGA-C13 | Consent policy stale khi submit | Không ghi version cũ; trả policy mới; retry idempotent | Contract + integration |
| RGA-C14 | Consent write race/cùng key khác payload | Optimistic conflict/order rõ; không mất hoặc ghi đè decision | Integration concurrency |
| RGA-C15 | Registration commit nhưng mail temporary/unknown | Account pending; retry cùng message ID, không intent/message mới | Worker/recovery |
| RGA-C16 | Mail permanent failure | Account pending; không báo delivered/verified, có recovery path | Contract + journey |
| RGA-C17 | Verification code đúng trong TTL | Consume một lần; email version verified; state theo consent policy | Integration |
| RGA-C18 | Code sai/hết hạn/locked/sai purpose/account | Generic error; attempt/lock atomically; không leak state | API + security |
| RGA-C19 | Replay/two verify concurrent | Một state event/effect; request kia idempotent hoặc conflict an toàn | Integration concurrency |
| RGA-C20 | Resend cooldown/budget/new generation | Limit đúng; intent cũ revoked; retry không tăng budget hai lần | Integration + clock |
| RGA-C21 | Verify sau account khóa/chờ xóa | Không kích hoạt/cấp session/onboarding asset | Journey + authorization |
| RGA-C22 | Verify thành công nhưng consent còn thiếu/stale | State `Chờ điều kiện tuổi/đồng ý`; quyền nhạy cảm vẫn khóa | Journey |
| RGA-C23 | Đủ eligibility và chọn starter hợp lệ | Một M06 operation applied; ownership truth ở M06, không AP | Contract + integration |
| RGA-C24 | Retry/parallel starter selection | Cùng request idempotent; khác selection conflict; một grant | Integration concurrency |
| RGA-C25 | M06 timeout sau apply | Reconcile `alreadyApplied`; không grant/compensation lặp | Recovery |
| RGA-C26 | Worker crash trước/sau từng side effect | Durable workflow tiếp tục đúng step/operation; không reset account | Worker/recovery |
| RGA-C27 | Hook achievement/quest partial failure | Identity/M06 result giữ nguyên; step lỗi retry độc lập, UI không báo hoàn tất giả | Integration + recovery |
| RGA-C28 | Audit/store lỗi trước mutation bắt buộc | Fail-closed, không state/side effect một phần | Fault injection |
| RGA-C29 | Audit/result-store lỗi sau remote effect | Reconcile bằng protected operation; không gọi cấp mới | Fault injection |
| RGA-C30 | Token/claim/cache cũ qua verify/consent withdrawal | Không tự nâng hoặc giữ quyền; API kiểm tra state/snapshot version | Security + journey |
| RGA-C31 | Enumeration/redaction sweep mọi response/error/log/audit | Không password/code/token/body/email đầy đủ/guardian evidence/provider body | Security/privacy |
| RGA-C32 | Kiểm kê durable effect sau suite | Mỗi effect có source/operation; không orphan/duplicate/AP mới; counts đối soát bằng 0 sai lệch | Reconciliation |

## Ma trận truy vết contract nguồn

| Contract/case nguồn | Case xuyên luồng | Coverage bổ sung |
|---|---|---|
| REG-C01–C06 | RGA-C01–C06 | Account uniqueness, idempotency, timeout-after-commit |
| REG-C07–C11 | RGA-C07–C09 | Strict schema, canonical/display/password/security dependency |
| REG-C12–C15 | RGA-C02, C10–C14 | Policy/consent và response trung tính |
| REG-C16–C19 | RGA-C15–C16, C23–C32 | Hậu commit, redaction và response contract |
| VER-C01–C04 | RGA-C01, C15–C16 | Intent/outbox và mail semantic result |
| VER-C05–C10 | RGA-C17–C19 | TTL, attempt, replay và binding |
| VER-C11–C13 | RGA-C19–C20 | Resend và concurrency |
| VER-C14–C20 | RGA-C21–C22, C28–C31 | Account state, audit, privacy và stale worker |
| CONS-C01–C07 | RGA-C01, C10–C13 | Resolver và explicit/separate decisions |
| CONS-C08–C15 | RGA-C13–C14, C30 | Idempotency, race, version/re-consent/withdraw |
| CONS-C16–C20 | RGA-C10, C28–C31 | Guardian fail-closed, consumer/cache và privacy |
| ONB-C01–C06 | RGA-C01, C23–C24, C26 | Post-commit workflow, selection, duplicate worker |
| ONB-C07–C13 | RGA-C03–C06, C25–C27 | Timeout, dependency failure, restart và duplicate registration |
| ONB-C14–C18 | RGA-C07, C28–C32 | Forbidden value, conflict, audit/reconcile/AP |

Không được xóa case nguồn vì ma trận này gom journey; unit/contract boundary chi tiết vẫn nằm trong từng contract.

## Fixture và oracle

| Fixture | Yêu cầu |
|---|---|
| Account/email | Địa chỉ reserved `.test`, canonical collisions có chủ đích; không dùng hộp thư thật |
| Policy | Ít nhất hai version, hai purpose độc lập, required + optional và một context unresolved |
| Verification | Clock cố định, code/verifier giả không xuất hiện trong log; có generation cũ/mới |
| Mail/M06 | Fake theo operation/message ID, lưu fingerprint và cho điều khiển lost response sau apply |
| Catalog | Offer published/retired, selection hợp lệ/không hợp lệ; không hard-code ID production |
| Faults | Exception trước commit, sau remote apply, audit failure, worker restart và concurrent barrier |

Oracle là durable state và source operation tại module sở hữu, không phải DTO/mock invocation riêng. Assertions phải kiểm tra cả “không xảy ra”: không token sớm, không consent giả, không liên kết email, không duplicate và không AP.

## Kết quả và bằng chứng tối thiểu khi triển khai

| Thành phần | Nội dung |
|---|---|
| Run manifest | Commit/schema/contract version, môi trường giả lập, clock seed, test filters |
| Case result | Case ID, pass/fail, duration, correlation protected refs và failure category |
| Reconciliation | Counts account/intent/message/decision/onboarding/operation/ownership; duplicate/orphan/AP delta |
| Privacy scan | Pattern/marker scan response, log, audit, trace; không lưu raw payload |
| Defect | Severity, contract violated, safe state, owner task và retest result |

Evidence ID là tùy chọn theo D-006; artifact test thật mới được đăng ký. Không đánh dấu A-G01 đạt từ checklist chưa chạy.

## Đối chiếu bộ test hiện có

Kết quả quan sát ngày 2026-08-20; chưa chạy runtime trong task tài liệu này.

| Mã | Finding | Khoảng trống | Task tiếp nhận |
|---|---|---|---|
| M01-RGA-I01 | `AuthServiceTests` chỉ có register success, username exists và email exists | Không bao phủ canonical/schema/password/policy/idempotency/concurrency | M01-T042-A |
| M01-RGA-I02 | Test dùng mock repository/service | Không chứng minh unique constraint, transaction, outbox hoặc durable retry | M01-T042-A; M11-T038–T040-A |
| M01-RGA-I03 | Không có test verification email trực tiếp | TTL/attempt/replay/resend/account state chưa kiểm chứng | M01-T042-A |
| M01-RGA-I04 | Không có policy/consent ledger test | Explicit choice/version/withdraw/re-consent chưa kiểm chứng | M01-T042-A |
| M01-RGA-I05 | Onboarding được mock theo từng repository và gọi inline | Crash/restart/unknown/reconcile không được kiểm chứng | M01-T042-A; M11-T040-A |
| M01-RGA-I06 | Existing success test chấp nhận `StarterPetId` trong registration | Củng cố contract cũ trái M01-REG-1.0 | M01-T042-A |
| M01-RGA-I07 | Không có privacy/enumeration marker scan | Có thể lộ email/password/code/token qua response/log/audit | M01-T042-A; M11-T033 |
| M01-RGA-I08 | Không có reconciliation assertion cho AP/ownership/source operation | Chưa chuẩn bị được bằng chứng B-G03 runtime | M01-T042-A; M06-T003–T005; B-G03 |
| M01-RGA-I09 | `Register_UsernameExists_ReturnsNull` và `AuthService` vẫn coi username là duy nhất | Trái contract tên hiển thị được phép trùng và email canonical là định danh đăng nhập | M01-T010; M01-T023-A; M01-T042-A |

## Finding còn mở

| Mã | Phần chưa chốt | Baseline an toàn | Nguồn/task xử lý |
|---|---|---|---|
| M01-RGA-F01 | Market/age/guardian matrix và policy fixtures cuối | Giữ unresolved fixture fail-closed; không suy mặc định phát hành | REL01-Q01–Q07; REL-01 |
| M01-RGA-F02 | Physical schema/API/worker implementation | Không gọi checklist là bằng chứng; chỉ chạy khi contract implementation tồn tại | M01-T042-A, M01-T043-A |
| M01-RGA-F03 | M06/M07 contract double và ledger truth đầy đủ | Dùng minimum contracts; không ghi trực tiếp tài sản/AP từ M01 | M06-T003–T019; M07-T022–T036 |
| M01-RGA-F04 | CI/runtime environment và evidence retention | Fixture giả, secret injection ngoài Git, artifact allowlist theo policy | M11-T038–T040-A; M12-T047-A |

## Tự kiểm M01-T009 và A-G01

- M01-T005–T008 đã hoàn thành; 32 case xuyên luồng và ma trận trace bao phủ toàn bộ 77 case nguồn.
- Mỗi nhóm quy tắc có success và failure/unknown/concurrency/recovery/privacy assertion kiểm chứng được.
- Tầng unit, contract, integration, worker, journey và privacy tách rõ; durable state/module owner là oracle.
- Chín finding hiện trạng và bốn finding mở có baseline an toàn cùng task/module tiếp nhận.
- A-G01/REL-01 vẫn mở: suite chưa được triển khai/chạy và market-age-consent chưa có kết luận phát hành.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt M01-REG-ACC-1.0 với 32 case xuyên luồng và trace 77 case nguồn | WSA-7K2 |
