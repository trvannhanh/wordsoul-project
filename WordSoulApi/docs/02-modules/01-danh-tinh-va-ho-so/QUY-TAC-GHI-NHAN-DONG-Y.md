# Quy tắc ghi nhận đồng ý M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T007 |
| Contract ID / phiên bản | M01-CONS-1.0 |
| Trạng thái | Baseline ghi nhận đồng ý có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-015; D-001, D-008; M01-D001; vòng đời tài khoản M01 v1.0; bản đồ dữ liệu hồ sơ v1.0 |
| Phạm vi | Registry chính sách, bản ghi quyết định, grant/decline/withdraw, đổi phiên bản, truy vấn hiệu lực và consumer propagation |
| Ngoài phạm vi | Kết luận pháp lý theo thị trường/tuổi; chứng minh quan hệ giám hộ; retention/xóa cuối cùng; nội dung pháp lý thực tế |

## Ranh giới ngữ nghĩa

| Loại | Ý nghĩa | Có được gộp không |
|---|---|---|
| `acknowledgement` | Xác nhận đã được trình bày/đã chấp nhận điều khoản bắt buộc để dùng một phạm vi sản phẩm khi policy cho phép | Không gộp với lựa chọn marketing, analytics hoặc kênh tùy chọn |
| `consent` | Lựa chọn tự nguyện, cụ thể cho một purpose có thể grant/decline/withdraw | Mỗi purpose là một lựa chọn độc lập; không dùng một cờ “đồng ý tất cả” |
| `preference` | Cách cá nhân hóa không phải căn cứ cho xử lý cần consent | Không được nâng thành consent hoặc acknowledgement |
| `legalBasisReference` | Tham chiếu cấu hình/đánh giá cho phép xử lý mà không dựa trên consent | Không phải quyết định của người dùng và không lưu như `granted` |

- Im lặng, dùng tiếp sản phẩm, checkbox được chọn sẵn hoặc thiếu response không bao giờ là `granted`.
- M01-CONS-1.0 mô tả cách ghi và thực thi quyết định; không tự kết luận purpose nào bắt buộc, nhóm tuổi nào được dùng hoặc thị trường nào được phát hành.
- Khi policy resolver không xác định được policy set hợp lệ, hành trình cần quyết định phải fail-closed và không thu payload “để xử lý sau”.

## Registry chính sách bất biến

| Trường | Yêu cầu |
|---|---|
| `policySetId` / `version` | Định danh opaque + phiên bản bất biến; version đã publish không sửa nội dung |
| `documentType` | Terms, privacy notice, consent notice hoặc loại allowlist khác |
| `purpose` | Một mục đích xử lý cụ thể; acknowledgement bắt buộc cũng có purpose riêng |
| `locale` / `contentDigest` | Đúng nội dung thực tế được trình bày; digest không thay thế bản nội dung lưu theo policy |
| `effectiveFrom` / `retiredAt` | Khoảng hiệu lực; không publish hồi tố |
| `audienceRuleRef` | Tham chiếu rule thị trường/nhóm tuổi/account state; không nhúng kết luận pháp lý vào client |
| `decisionKind` / `requiredness` | `acknowledgement` hoặc `consent`; required/optional do policy đã duyệt quyết định |
| `reconsentMode` | `none`, `before-next-use`, `by-deadline` hoặc `immediate-stop`; chỉ có sau quyết định được duyệt |
| `supersedes` | Phiên bản trước và migration decision; không tự suy phiên bản mới tương đương |
| `status` | `draft`, `published`, `retired`; chỉ `published` được resolver trả về |

Publish là mutation quản trị có authorization, optimistic concurrency, audit và rollback bằng phiên bản mới; không sửa bản đã publish.

## Bản ghi quyết định append-only

| Trường logic | Quy tắc M01-CONS-1.0 |
|---|---|
| `decisionId` | Opaque, duy nhất; là idempotency result cho một mutation |
| `subjectRef` / `accountRef` | Định danh nội bộ được bảo vệ; không dùng email/tên làm khóa |
| `actorType` / `actorRef` | `self`, `guardian`, `authorized-operator`; guardian chỉ được dùng khi REL-01 có mô hình được duyệt |
| `relationshipEvidenceRef` | Chỉ tham chiếu bằng chứng được bảo vệ; nullable và không nhận payload thô |
| `decisionKind` / `purpose` | Khớp registry; không để client tự tạo purpose |
| `policySetId` / `policyVersion` | Đúng phiên bản đã resolve và trình bày |
| `locale` / `contentDigest` | Bản nội dung thực tế; phải khớp registry |
| `decision` | `granted`, `declined`, `withdrawn`; acknowledgement không nhận `withdrawn` nếu policy không định nghĩa quyền rút |
| `occurredAt` / `recordedAt` | UTC; server cấp `recordedAt`, client time chỉ là metadata không tin cậy |
| `effectiveAt` | Thời điểm quyết định có hiệu lực theo policy; không được trước `recordedAt` nếu không có migration được duyệt |
| `source` | Journey/channel/application version allowlist; không lưu user-agent/IP thô trong ledger |
| `supersedesDecisionId` | Liên kết bản trước cùng subject/purpose khi decline/withdraw/re-consent |
| `idempotencyKey` / `requestFingerprint` | Key scoped theo subject + journey; retry cùng fingerprint trả cùng result, khác fingerprint conflict |
| `policyContextRef` | Tham chiếu market/age-band resolution đã dùng; không lưu ngày sinh hoặc vị trí chi tiết nếu không cần |

Ledger không update/delete bản ghi lịch sử thông thường. Correction tạo event mới có reason và liên kết bản bị thay thế; retention/erasure chỉ chạy theo REL-01/REL-07 và giữ metadata tối thiểu khi có căn cứ.

## Policy resolution

1. Server nhận journey, authenticated/registration subject reference và context tối thiểu đã được phép thu.
2. Resolver chọn đúng một `policySetId + version` đã publish theo market/age band/client capability; client không gửi requiredness hoặc version đáng tin cậy.
3. Response trình từng document/purpose độc lập, locale, digest, requiredness và consequence dễ hiểu khi decline/withdraw.
4. Mutation chỉ chấp nhận decision cho đúng policy context còn hiệu lực. Version cũ hoặc context đổi trả stale-policy response và không ghi quyết định giả.
5. Nếu không resolve được policy, trả unavailable/fail-closed; không mặc định người lớn, không mặc định consent và không kích hoạt account.

## Luồng grant, decline và acknowledgement

1. Client gửi contract version, policy set/version, danh sách decision rõ ràng, idempotency key và correlation trong POST body.
2. Server kiểm tra authentication/registration receipt, actor, policy context, registry status, purpose allowlist và requiredness.
3. Trong một transaction, append các decision, tính consent snapshot có version, ghi audit metadata và tạo outbox cho consumer bị ảnh hưởng.
4. Required acknowledgement bị decline giữ account ở `Chờ điều kiện tuổi/đồng ý` hoặc từ chối journey theo policy; không tự ghi `granted`.
5. Optional consent bị decline không chặn năng lực cốt lõi trừ khi REL-01/policy đã duyệt nói khác; purpose liên quan giữ tắt.
6. Retry cùng idempotency key/fingerprint trả cùng semantic result và không append/outbox lần hai.

## Luồng withdraw và thay đổi quyết định

| Bước | Yêu cầu |
|---|---|
| Tiếp nhận | Chủ thể đã xác thực hoặc actor được ủy quyền; xác minh lại khi purpose/policy yêu cầu |
| Kiểm tra | Purpose hiện hành, actor/subject, decision đang hiệu lực, account state và optimistic version |
| Commit | Append `withdrawn`/`declined`, supersede bản trước, tăng consent snapshot version, audit và outbox trong cùng logic |
| Hiệu lực trung tâm | Resolver trả `not-effective` ngay sau commit; consumer không được tiếp tục chỉ vì cache/event chậm |
| Propagation | Consumer idempotently dừng xử lý tương lai, thu hồi endpoint/job phù hợp và trả acknowledgement |
| Partial failure | Giữ trạng thái trung tâm, retry/reconcile consumer, cảnh báo sai lệch; không rollback withdrawal thành granted |
| Dữ liệu đã xử lý | Xóa/ẩn danh/giữ theo purpose và REL-01/REL-07; withdrawal không tự hứa xóa mọi lịch sử |

Thông báo bảo mật bắt buộc theo M01-D024 không được coi là marketing consent. Kênh tùy chọn và thiết bị nhận tin phải kiểm tra consent/preference tương ứng theo M01-T026-A/M01-T027-A và REL-06.

## Đổi phiên bản và re-consent

| Tình huống | Baseline an toàn |
|---|---|
| Thay đổi trình bày không đổi mục đích/căn cứ | Chỉ kế thừa khi migration decision đã duyệt ghi rõ; nếu không, coi chưa xác định |
| Thay đổi purpose, dữ liệu, provider, audience hoặc consequence | Không kế thừa; policy quyết định re-consent mode trước khi publish |
| `before-next-use` | Chặn purpose tại lần dùng tiếp theo cho đến khi có quyết định mới |
| `by-deadline` | Bản cũ chỉ còn hiệu lực đến deadline công bố; sau đó fail-closed cho purpose |
| `immediate-stop` | Dừng purpose khi version mới có hiệu lực; session/cache cũ không được bỏ qua |
| Resolver/registry lỗi | Không coi phiên bản cũ hợp lệ vô hạn; dùng policy snapshot có thời hạn đã duyệt hoặc fail-closed |

Version mới không sửa ledger cũ. Re-consent tạo decision mới, liên kết bản trước và chỉ mở lại purpose sau commit thành công.

## Hợp đồng truy vấn hiệu lực cho consumer

| Input | Output bắt buộc |
|---|---|
| Subject protected ref, purpose, policy context/version yêu cầu, thời điểm đánh giá | `effective`, `notEffective`, `policyStale`, `unknown`; decision/snapshot version, evaluatedAt, reason category và maxCacheUntil |

- Consumer chỉ mở capability khi kết quả `effective` đúng purpose/version/context; `unknown` và `policyStale` fail-closed cho xử lý cần consent.
- Snapshot/cache phải gắn subject + purpose + policy version + consent snapshot version và có TTL ngắn theo criticality; withdrawal event làm invalidation ngay.
- Không trả actor identity, relationship evidence, nội dung policy hoặc lịch sử ledger cho consumer chỉ cần boolean hiệu lực.
- Consumer mutation quan trọng phải ghi consent snapshot/version đã kiểm tra để đối soát, không copy toàn bộ bản ghi consent.

## Response và lỗi an toàn

| Trường hợp | Semantic public | Hành vi |
|---|---|---|
| Query policy hợp lệ | `200 policy-required` | Chỉ trả nội dung/metadata cần trình bày, không lộ rule nội bộ |
| Ghi decision thành công | `200 decision-recorded` | Trả decision reference và nextAction tổng quát |
| Retry cùng request | `200 decision-recorded` idempotent | Không tạo event/outbox thứ hai |
| Policy/version/context stale | `409 CONS_POLICY_STALE` | Trả policy presentation mới; không ghi payload cũ |
| Purpose/decision/schema sai | `422 CONS_DECISION_INVALID` | Không mutation; không echo nội dung nhạy cảm |
| Không có policy áp dụng | `503 CONS_POLICY_UNAVAILABLE` | Fail-closed, không suy diễn default |
| Concurrent decision conflict | `409 CONS_VERSION_CONFLICT` | Client lấy snapshot mới; thứ tự quyết định không bị mất |
| Store/audit bắt buộc lỗi | `503 CONS_TEMPORARILY_UNAVAILABLE` | Không ghi một phần |

## Audit, privacy và retention

| Event | Metadata allowlist |
|---|---|
| `consent.policy.resolved_or_failed` | Policy/version, purpose set, result/reason, context ref, correlation |
| `consent.decision.recorded` | Decision/subject protected ref, actor type, purpose, policy/version, decision, effective time, source category |
| `consent.decision.withdrawn` | Decision refs, purpose, policy/version, effective time, correlation |
| `consent.consumer.propagated` | Consumer, purpose, snapshot version, result/attempt/latency, correlation |
| `consent.policy.published` | Policy/version/digest, audience rule ref, reconsent mode, actor ref, change reason |

- Không log/audit email, tên, ngày sinh, vị trí chi tiết, nội dung policy đầy đủ, payload decision thô, IP/user-agent thô hoặc bằng chứng giám hộ.
- Quyền xem ledger chi tiết tách khỏi quyền kiểm tra hiệu lực; self/export và operator access theo M01-T022-A/M01-T028–T034.
- Retention cụ thể vẫn chờ REL01-Q05/Q07 và REL-07. Cho đến khi có kết luận, không xóa âm thầm, không giữ vô hạn bằng giả định; registry phải từ chối publish policy thiếu retention reference.

## Ma trận nghiệm thu M01-CONS-1.0

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| CONS-C01 | Resolve đúng context | Trả đúng một policy set published, version/digest/locale và purpose tách biệt |
| CONS-C02 | Resolver không xác định được market/age policy | `503`, không ghi decision/kích hoạt account |
| CONS-C03 | Grant required acknowledgement hợp lệ | Append một decision, snapshot/outbox/audit cùng commit |
| CONS-C04 | Decline required acknowledgement | Không tự grant; account/journey giữ trạng thái an toàn |
| CONS-C05 | Decline optional consent | Core capability không bị chặn ngoài policy; optional purpose giữ tắt |
| CONS-C06 | Checkbox chọn sẵn/im lặng/thiếu decision | Không tạo `granted` |
| CONS-C07 | Hai purpose trong một màn hình | Hai decision độc lập; rút một không đổi purpose kia |
| CONS-C08 | Retry cùng idempotency key/fingerprint | Cùng result, không duplicate ledger/outbox |
| CONS-C09 | Cùng key nhưng fingerprint khác | Conflict, không mutation |
| CONS-C10 | Hai quyết định đồng thời | Optimistic conflict/order rõ; không mất decision |
| CONS-C11 | Withdraw consent hiệu lực | Trung tâm trả not-effective ngay; consumer nhận event idempotent |
| CONS-C12 | Consumer lỗi khi withdraw | Ledger vẫn withdrawn; retry/reconcile và cảnh báo sai lệch |
| CONS-C13 | Policy version stale | Không ghi bản cũ; client nhận yêu cầu tải policy mới |
| CONS-C14 | Version mới cần re-consent | Purpose dừng theo mode/deadline; không kế thừa âm thầm |
| CONS-C15 | Correction ledger | Append correction/supersedes; không update/delete history |
| CONS-C16 | Actor guardian khi chưa có mô hình REL-01 | Từ chối, không lưu quan hệ/bằng chứng giả |
| CONS-C17 | Consumer query `unknown`/`policyStale` | Fail-closed cho purpose cần consent |
| CONS-C18 | Cache/session cũ sau withdrawal | Không bỏ qua state trung tâm; invalidation/version check chặn xử lý mới |
| CONS-C19 | Audit/store lỗi trước commit | Fail-closed, không decision/snapshot/outbox một phần |
| CONS-C20 | Inspect log/audit/error/export không đúng quyền | Không có PII/payload thô/bằng chứng giám hộ/nội dung policy đầy đủ |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-20; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-CONS-I01 | `User` không có consent/policy/age-band/market state | Không thể resolve hoặc cưỡng chế policy theo subject | M01-T008–T009, M01-T033 |
| M01-CONS-I02 | Không tìm thấy policy registry hoặc consent ledger entity/repository | Không có lịch sử append-only, version hay withdrawal | M01-T008–T009; M11-T012–T014 |
| M01-CONS-I03 | Registration không resolve/trình bày/ghi consent | Có thể tạo account khi điều kiện policy chưa xác định | M01-T008–T009 |
| M01-CONS-I04 | Login/token hiện không kiểm tra consent snapshot/account policy state | Phiên có thể mở quyền sau decline/withdraw/stale policy | M01-T010, M01-T016–T018 |
| M01-CONS-I05 | Không thấy idempotency/concurrency cho consent decision | Retry/race có thể mất hoặc đảo quyết định | M01-T009, M01-T038 |
| M01-CONS-I06 | Một `FcmToken` nằm trực tiếp trên `User`, không có consent/device registry | Khó thu hồi theo purpose và tránh gửi nhầm thiết bị dùng chung | M01-T026-A, M01-T027-A |
| M01-CONS-I07 | Không có consumer propagation/reconciliation cho withdrawal | Xử lý tương lai có thể tiếp tục sau khi rút | M01-T027-A; M11-T038–T040-A |
| M01-CONS-I08 | Chưa có access/export/retention enforcement cho ledger | Có thể lộ hoặc giữ/xóa bằng chứng sai chính sách | M01-T022-A, M01-T033–T036; M11-T033 |

## Finding còn mở có baseline an toàn

| Mã | Phần chưa chốt | Baseline an toàn hiện hành | Nguồn/task xử lý |
|---|---|---|---|
| M01-CONS-F01 | Market/age threshold, required consent và guardian model | Không resolve được policy thì không grant/kích hoạt; không thu bằng chứng giám hộ | REL01-Q01–Q04; M01-T033 |
| M01-CONS-F02 | Thay đổi nào bắt buộc re-consent và deadline | Không tự kế thừa; policy mới phải có migration/reconsent decision trước publish | REL01-Q06; M11-T008–T017 |
| M01-CONS-F03 | Retention/export/delete từng purpose | Không publish policy thiếu retention reference; không xóa/giữ vô hạn bằng giả định | REL01-Q05/Q07; M01-T033–T036; REL-07 |
| M01-CONS-F04 | Default notification theo market/channel/age | Kênh tùy chọn mặc định tắt; security notice tách riêng, không gọi là marketing consent | REL-06; M01-T025-A–T027-A |

## Tự kiểm M01-T007, A-G01 và REL

- M01-T002/M01-T003/A0-T001 đã hoàn thành; contract kỹ thuật không tự đóng REL-01 hoặc REL-06.
- Registry bất biến và ledger append-only lưu loại, purpose, policy/version/digest/locale, actor, thời điểm, source, supersedes và context reference.
- Grant/decline/withdraw/re-consent có idempotency, concurrency, audit, propagation và fail-closed boundary rõ.
- Hai mươi case bao phủ policy unavailable/stale, explicit choice, retry/race, withdrawal, consumer failure, guardian, cache và privacy.
- Tám finding triển khai và bốn finding mở đều có baseline an toàn cùng task/REL tiếp nhận.
- A-G01 vẫn mở vì chưa có triển khai/runtime evidence và ma trận thị trường–tuổi–đồng ý của REL-01 chưa được kết luận. REL-06 vẫn mở cho Giai đoạn B; optional notification mặc định tắt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo mô hình append-only và quy tắc hiệu lực/thu hồi | Chưa gán |
| 2026-08-20 | 1.0 | Chốt M01-CONS-1.0, registry/ledger, 20 case và finding hiện trạng | WSA-7K2 |
