# Điều phối khởi tạo người dùng mới M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T008 |
| Contract ID / phiên bản | M01-ONB-1.0 |
| Trạng thái | Baseline điều phối onboarding có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-016; D-011; M01-REG-1.0, M01-VER-1.0, M01-CONS-1.0; M06-D001, D005–D007, D012 |
| Hợp đồng M06 | M06-ONB-A-1.0 tối thiểu trong tài liệu này; M06-T003–T005/T012–T019 hoàn thiện sổ và cấp giá trị |
| Phạm vi | Receipt, state machine, outbox, cấp starter pet qua module sở hữu, retry/reconcile và trạng thái phần lỗi |
| Ngoài phạm vi | Catalog/cân bằng starter cuối cùng; triển khai sổ M06; nhiệm vụ/thành tựu M07 đầy đủ; chuyển đổi/xóa AP |

## Quyết định biên

- Tạo danh tính và điều phối onboarding là hai boundary: account hợp lệ phải commit trước; lỗi hậu xử lý không xóa hoặc tạo lại danh tính.
- Registration không nhận `starterPetId`, tài sản, XP, AP, hint, achievement hay role. Lựa chọn starter là bước onboarding đã xác thực sau khi account đủ điều kiện.
- M01 sở hữu trạng thái workflow và operation ID; M06 sở hữu catalog, quyền sở hữu thú, sổ biến động và kết quả cấp.
- Không cấp AP, không tạo dependency AP và không ghi số dư trực tiếp. Mọi thay đổi tài sản phải giải thích được bằng operation/source record của module sở hữu.
- Mỗi step có idempotency key ổn định; retry, timeout và worker restart không tạo quyền lợi lặp.

## Điều kiện bắt đầu

| Điều kiện | Yêu cầu |
|---|---|
| Account | Danh tính đã commit theo M01-REG-1.0 và chưa xóa/chờ xóa/ngừng hoạt động |
| Email | Đã xác minh theo M01-VER-1.0 nếu step cần tài khoản hoạt động |
| Policy | Required acknowledgement/consent hiện hành đã đạt; policy unknown/stale giữ chờ |
| Catalog | Starter offer/version đang published và item/pet hợp lệ theo M06 |
| Receipt | Một `onboardingId` duy nhất cho account generation; có registration receipt và correlation bảo vệ |

Account chưa đủ email/policy vẫn có onboarding record `waitingEligibility`, nhưng không gọi cấp tài sản. Khóa tạm giữ step đang chờ; khóa dài hạn/chờ xóa dừng dispatch và chuyển reconcile theo policy.

## Mô hình workflow

| Trường logic | Quy tắc |
|---|---|
| `onboardingId` | Opaque, duy nhất theo account generation |
| `accountRef` / `accountGeneration` | Internal protected ref; không dùng email/tên |
| `contractVersion` / `policySetVersion` | `M01-ONB-1.0` và policy snapshot đã kiểm tra |
| `offerId` / `offerVersion` | Gói starter published; không nhận quantity/reward tùy ý từ client |
| `state` | `waitingEligibility`, `ready`, `inProgress`, `waitingRetry`, `needsReconciliation`, `completed`, `cancelled` |
| `stepStates` | Trạng thái riêng cho profile baseline, M06 starter ownership và các hook module sở hữu |
| `operationIds` | Ổn định theo `onboardingId + step + version`; không sinh lại khi retry |
| `attempt` / `nextAttemptAt` | Retry có budget/backoff; không retry lỗi cuối hoặc sau cancellation |
| `resultVersion` | Optimistic concurrency và response idempotent |
| `createdAt` / `completedAt` | UTC do server cấp; không lấy thời gian client làm truth |

## State machine

| Từ | Sang | Điều kiện | Cấm |
|---|---|---|---|
| Chưa có | `waitingEligibility` | Account commit và outbox cùng transaction logic | Gọi M06 inline trước account commit |
| `waitingEligibility` | `ready` | Email/policy/account state và offer đều hợp lệ | Tự coi policy unknown là đạt |
| `ready` | `inProgress` | Worker claim bằng lease/fencing token | Hai worker cùng sở hữu step |
| `inProgress` | `waitingRetry` | Dependency trả temporary failure | Sinh operation ID mới |
| `inProgress` | `needsReconciliation` | Timeout/unknown hoặc local result store lỗi sau remote effect | Tự cấp bù lần hai |
| `inProgress` | `completed` | Mọi step bắt buộc applied/alreadyApplied và audit/outbox hoàn tất | Hoàn tất khi có partial/unknown |
| Bất kỳ chưa xong | `cancelled` | Account bị xóa/chờ xóa/ngừng hoạt động hoặc offer bị thu hồi theo policy | Xóa lịch sử operation |
| `waitingRetry`/`needsReconciliation` | `inProgress` | Retry/reconcile cùng operation ID và còn hợp lệ | Reset toàn workflow |

## M06-ONB-A-1.0 — hợp đồng cấp starter tối thiểu

### Request

| Trường | Bắt buộc | Yêu cầu |
|---|---|---|
| `operationId` | Có | Idempotency key duy nhất, ổn định cho step |
| `source` | Có | `account-onboarding`; không nhận source tùy ý từ client |
| `subjectRef` | Có | Internal account reference đã đủ điều kiện |
| `offerId` / `offerVersion` | Có | Offer published do server resolve |
| `selectionId` | Tùy offer | Chỉ một lựa chọn thuộc allowlist hiện hành; không random trong M01 |
| `accountStateVersion` | Có | Snapshot để M06 từ chối account stale/không hợp lệ |
| `occurredAt` / `expiresAt` | Có | Không cấp sau expiry/cancellation |
| `correlationId` | Có | Opaque, không PII |

Request không chứa AP, role, balance mới, quantity tùy ý, pet stats, ledger rows hoặc cờ “đã sở hữu”. M06 tự kiểm catalog và durable ownership.

### Result

| Kết quả | Ý nghĩa | Hành vi M01 |
|---|---|---|
| `applied` | M06 đã ghi source/ownership/ledger đúng một lần | Lưu protected result ref và hoàn tất step |
| `alreadyApplied` | Operation này đã có cùng fingerprint/result | Xử lý như thành công idempotent |
| `temporaryFailure` | Có thể retry | Retry cùng operation ID theo budget |
| `rejected` | Account/offer/selection/expiry không hợp lệ | Không retry mù; đưa user sửa lựa chọn hoặc operator xử lý |
| `unknown` | Mất response hoặc chưa biết effect | Reconcile bằng operation ID; không gửi grant mới |

M06 phải conflict khi cùng operation ID có fingerprint khác; unique source operation và ownership constraint là boundary cuối chống cấp lặp.

## Trình tự điều phối

1. Registration commit account, registration receipt, onboarding `waitingEligibility` và outbox đánh giá điều kiện trong cùng logic; response không hứa starter đã cấp.
2. Worker kiểm tra account state, email, policy snapshot và cancellation trước mỗi claim.
3. Khi đủ điều kiện, server trả offer/version cho client. Client chỉ chọn `selectionId` allowlist bằng request idempotent.
4. Worker dispatch M06-ONB-A-1.0 bằng operation ID cố định. Timeout chuyển reconcile, không tự suy thất bại.
5. M06 `applied/alreadyApplied` cho phép hoàn tất step; hooks achievement/quest chỉ gọi module sở hữu bằng operation riêng và không quyết định tài sản M06.
6. Workflow chỉ `completed` khi mọi step bắt buộc có kết quả xác định. Step tùy chọn lỗi được hiển thị là pending/degraded theo policy, không báo thành công giả.

## Retry, reconcile và sửa phần lỗi

| Tình huống | Hành vi bắt buộc |
|---|---|
| Worker chết trước dispatch | Lease hết hạn; worker khác tiếp tục cùng operation ID |
| Worker chết sau remote apply | Reconcile operation trước retry; `alreadyApplied` không tạo quyền lợi mới |
| Local save lỗi sau response | State `needsReconciliation`; đọc result theo operation ID |
| M06 tạm lỗi | Backoff có jitter/budget; account vẫn tồn tại và UI thấy pending |
| Selection bị retire trước apply | `rejected`; lấy offer mới, operation selection mới có liên kết supersedes |
| Account bị khóa/chờ xóa | Dừng dispatch; không cấp pending reward; giữ record để audit/reconcile |
| Một hook không bắt buộc lỗi | Không rollback identity/M06 effect; retry riêng và hiển thị trạng thái trung thực |
| Dữ liệu hiện tại đã cấp một phần | Không backfill tự động; lập migration/reconciliation có dry-run và source ID riêng |

Không compensation bằng cách xóa pet/sổ trực tiếp. Nếu source grant sai, M06 xử lý adjustment/case theo M06-D004/D008; M01 chỉ gửi yêu cầu có lý do và theo dõi kết quả.

## Response, quyền và privacy

| Journey | Response |
|---|---|
| Registration | Receipt + trạng thái account/onboarding tổng quát; không trả tài sản/token |
| Get onboarding | State, bước user có thể làm, offer presentation; không trả ledger/internal failure |
| Submit selection | `202 accepted` hoặc idempotent result; không nói granted trước M06 result |
| Dependency pending/unknown | `pending`, retry-after phù hợp; không tạo account/selection mới |
| Completed | Result presentation từ protected refs; client đọc ownership truth từ M06 |

- Chỉ subject/session phù hợp xem onboarding; operator cần quyền/lý do/audit. Không tin account ID, role hoặc ownership từ client.
- Log/audit allowlist gồm onboarding/operation protected ref, step/state, offer/version, result category, attempt, latency và correlation; không email, tên, secret, request body hoặc provider payload.

## Ma trận nghiệm thu M01-ONB-1.0

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| ONB-C01 | Registration commit | Một account/receipt/onboarding/outbox, chưa cấp tài sản inline |
| ONB-C02 | Chưa verify/consent | `waitingEligibility`, không gọi M06 |
| ONB-C03 | Đủ điều kiện và chọn starter hợp lệ | Một operation M06, applied đúng một lần |
| ONB-C04 | Retry cùng selection request | Cùng semantic result, không thêm operation/grant |
| ONB-C05 | Cùng key khác selection | Conflict, không đổi lựa chọn âm thầm |
| ONB-C06 | Hai worker claim đồng thời | Một lease/fencing owner dispatch |
| ONB-C07 | Timeout sau M06 apply | Reconcile thành alreadyApplied, không cấp lặp |
| ONB-C08 | M06 temporary failure | Account còn nguyên, retry cùng operation ID |
| ONB-C09 | Offer/selection invalid hoặc retired | Rejected; không random/fallback ngầm |
| ONB-C10 | Account khóa/chờ xóa trước dispatch | Cancel/hold, không cấp pending asset |
| ONB-C11 | Hook achievement/quest lỗi sau M06 | Không rollback identity/pet; step riêng retry được |
| ONB-C12 | Worker restart nhiều lần | Tiếp tục từ durable state, không reset workflow |
| ONB-C13 | Duplicate account registration | Registration receipt cũ; không tạo onboarding thứ hai |
| ONB-C14 | Request chứa AP/XP/quantity/stats | Từ chối schema, không mutation |
| ONB-C15 | M06 operation fingerprint conflict | `needsReconciliation`, cảnh báo; không tự retry payload khác |
| ONB-C16 | Local audit/store lỗi trước dispatch | Fail-closed, không remote effect |
| ONB-C17 | Result store lỗi sau remote effect | Reconcile bằng operation ID, không compensation mù |
| ONB-C18 | Inspect DB/log/audit | Mọi effect có source operation; không AP mới/PII/payload thô |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả quan sát ngày 2026-08-20; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-ONB-I01 | `RegisterDto` nhận `StarterPetId`, trái boundary M01-REG-1.0 | Client registration chi phối tài sản ngoài phạm vi | M01-T009; M06-T006, M06-T019 |
| M01-ONB-I02 | `InitializeNewUserAsync` chạy inline sau account `SaveChanges` | Lỗi sau commit để trạng thái một phần nhưng response có thể thất bại | M01-T009; M11-T038–T040-A |
| M01-ONB-I03 | Achievement, pet và daily quest lưu theo nhiều lần riêng | Không có workflow/result toàn cục để tiếp tục/đối soát | M01-T009; M07-T022–T025, M07-T036; M11-T038–T040-A |
| M01-ONB-I04 | Null starter được random từ ID cứng 1/4/7 dù DTO nói null không gán | Hành vi không nhất quán, catalog/version không kiểm soát | M01-T009; M06-T006, M06-T019 |
| M01-ONB-I05 | Chống lặp hiện dựa trên “có bất kỳ achievement/pet” | Partial set hoặc race có thể bị coi hoàn tất/sinh trùng | M06-T012, M06-T019; M11-T040-A |
| M01-ONB-I06 | Không có onboarding receipt/outbox/operation ID/state | Không phân biệt chưa chạy, lỗi, timeout hay đã áp dụng | M01-T009; M11-T038–T040-A |
| M01-ONB-I07 | Starter ownership ghi trực tiếp qua repository M01 flow | Bỏ qua contract/sổ/source truth của M06 | M06-T003–T005, M06-T012–T019 |
| M01-ONB-I08 | `User` còn cột AP và đặt `HintBalance = 5` trực tiếp trên bản ghi | Giữ dependency AP và tạo số dư gợi ý ngoài sổ | REL-05; M06-T002–T005; B-G03 |

## Finding còn mở và chuẩn bị B-G03

| Mã | Phần chưa chốt | Baseline an toàn | Nguồn/task xử lý |
|---|---|---|---|
| M01-ONB-F01 | Starter offer/catalog/version và eligibility cuối | Không hard-code/random; chỉ offer published từ M06 | M06-T006, M06-T019 |
| M01-ONB-F02 | M06 ledger/grant contract đầy đủ | Dùng M06-ONB-A-1.0, operation duy nhất, không ghi ownership/balance từ M01 | M06-T003–T005, M06-T012–T019 |
| M01-ONB-F03 | Achievement/quest onboarding thuộc M07 | Hook riêng, idempotent; lỗi không làm mất identity/M06 result | M07-T022–T025, M07-T036; M11-T038–T040-A |
| M01-ONB-F04 | AP hiện hữu và migration | Không cấp/dùng AP mới; giữ lịch sử, chờ inventory/reconcile/rollback | D-011, REL-05, M06-T002–T005; B-G03 |

Chuẩn bị B-G03 đạt ở mức contract: mọi effect mới có source operation, module owner, semantic result và reconcile path; AP bị cấm. B-G03 chưa đạt cho đến khi sổ M06, migration AP, runtime duplicate/recovery test và đối soát hoàn thành.

## Tự kiểm M01-T008

- M01-T005–T007 đã hoàn thành; M06-ONB-A-1.0 đáp ứng dependency tối thiểu mà không đóng các task M06 đầy đủ.
- Identity commit tách khỏi side effect; state machine, operation ID, outbox, retry/unknown/reconcile và partial failure đã xác định.
- Mười tám case bao phủ duplicate, concurrency, timeout, restart, invalid offer, account state, partial failure, AP và audit.
- Tám finding triển khai và bốn finding mở có baseline an toàn cùng task/module tiếp nhận.
- A-G01 và B-G03 vẫn mở vì chưa có triển khai/runtime evidence, sổ M06 hoặc migration/đối soát AP.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt M01-ONB-1.0, M06-ONB-A-1.0, 18 case và finding hiện trạng | WSA-7K2 |
