# Từ điển quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T001 |
| Dictionary ID / phiên bản | M11-DICT-1.0 |
| Trạng thái | Baseline thuật ngữ có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-001, D-008, D-032; M11-D001–D027; M01-DICT-1.0; M12-DICT-1.0 |
| Chủ thuật ngữ | M11 sở hữu nghĩa quản trị; M01 sở hữu identity/session; module nguồn sở hữu tài nguyên, invariant và effect nghiệp vụ; M12 sở hữu capability/dependency semantics |
| Quy tắc thay đổi | Một khái niệm chuẩn có một nghĩa/owner; thay nghĩa hoặc owner tăng phiên bản, cập nhật consumer và không tạo quyền/bypass ngầm |

## Nguyên tắc sử dụng

- Role không phải permission; permission không có scope không đủ để authorize; UI visibility không phải enforcement.
- M11 điều phối quyền, thay đổi, audit và vận hành nhưng không trở thành source of truth cho account, học liệu, tài sản, trận hoặc capability.
- Mọi decision dùng current actor/session/permission/scope/policy/resource version tại enforcement point; claim/cache stale không thắng source truth.
- `Audit`, `activity` và `operational log` là ba loại record khác mục đích, schema, quyền và retention; không dùng thay nhau.
- Trạng thái account, session, request thay đổi, configuration, job, support case và incident thuộc namespace riêng; không dùng `Active`, `Failed`, `Closed` trần trong contract/event.
- “Khẩn cấp” chỉ mô tả urgency/incident path; không tạo temporary role, emergency privilege, audit bypass hoặc two-person workflow ngoài quyết định.

## Thuật ngữ chuẩn

| Thuật ngữ | Định nghĩa chuẩn | Chủ sở hữu | Không được hiểu là |
|---|---|---|---|
| Vai trò quản trị | Nhóm quyền theo trách nhiệm vận hành đã xác định | M11/M01 | Một cờ Admin cho phép mọi hành động |
| Quyền | Khả năng thực hiện một hành động cụ thể trên tài nguyên và phạm vi cụ thể | M11; module nguồn xác nhận hành động | Vai trò hoặc quyền suy diễn từ giao diện |
| Phạm vi quyền | Giới hạn module, dữ liệu, nội dung, môi trường hoặc đối tượng mà quyền áp dụng | M11/module nguồn | Quyền toàn hệ thống mặc định |
| Quyền tối thiểu | Chỉ cấp các quyền cần cho trách nhiệm hiện tại; mọi quyền khác mặc định từ chối | M11/an toàn hệ thống | Cấp rộng rồi dựa vào hướng dẫn thủ công |
| Xác minh lại | Bằng chứng xác thực mới gần thời điểm thao tác nhạy cảm | M01/M11 | Phê duyệt hai người hoặc quyền khẩn cấp |
| Yêu cầu thay đổi | Bản ghi mục tiêu, phạm vi, trước/sau, tác động, kiểm chứng, lịch và rollback | M11/module nguồn | Quyền thực thi nếu người tạo không có quyền |
| Rà soát/xác nhận | Hoạt động kiểm tra artifact hoặc bằng chứng bởi vai trò phù hợp | Chủ artifact/gate | Bước duyệt hai người bắt buộc cho mọi thay đổi |
| Hiệu lực | Mốc thời gian/phạm vi mà phiên bản đã kiểm tra được module nguồn áp dụng | M11/module nguồn | Thời điểm lưu bản nháp |
| Rollback | Áp dụng quy trình quay lại phiên bản/trạng thái tương thích và kiểm chứng kết quả | M11/module nguồn | Xóa lịch sử hoặc ghi đè không truy vết |
| Cấu hình | Giá trị có chủ, kiểu, mặc định, phạm vi, độ nhạy và module tiêu thụ | M11/module nguồn | Bí mật hoặc logic nghiệp vụ vô chủ |
| Phiên bản cấu hình | Snapshot bất biến của một hoặc một bộ cấu hình có hiệu lực xác định | M11 | Chỉ số tăng không có nội dung/truy vết |
| Audit | Bằng chứng bất biến về tác nhân, quyền, hành động, đối tượng, lý do, trước/sau và kết quả | M11 | Request log hoặc activity chung |
| Activity | Sự kiện hoạt động nghiệp vụ phục vụ lịch sử/trải nghiệm theo chính sách module | Module nguồn | Bằng chứng đủ cho thao tác nhạy cảm |
| Log vận hành | Metadata chẩn đoán sức khỏe/lỗi đã qua allowlist và redaction | M11/M12 | Payload thô, bí mật hoặc audit |
| Vụ việc hỗ trợ | Hồ sơ có chủ, lý do, phạm vi truy cập, SLA, bằng chứng và kết quả | M11 | Quyền sửa trực tiếp dữ liệu nguồn |
| Chỉ số | Giá trị có tên, công thức, nguồn, cửa sổ, múi giờ, độ mới và chủ | M11/module nguồn | Con số dashboard không định nghĩa |
| Health | Tín hiệu sống/sẵn sàng/correctness/freshness/dependency có chủ và ngưỡng | M11/M12 | Ping cố định hoặc luôn xanh |
| Cảnh báo | Tín hiệu vượt ngưỡng có mức, chủ, chống lặp, escalation và playbook | M11/vận hành | Một log lỗi không có người xử lý |
| Công việc nền | Đơn vị chạy có chủ, input, lịch/trigger, timeout, khóa, idempotency và kết quả | Module nguồn; M11 đăng ký | Script có thể chạy lại tùy ý |
| Đối soát | So sánh nguồn–đích bằng khóa/quy tắc đã định để phát hiện và xử lý sai lệch | Module nguồn/M11 | Ghi đè lịch sử để làm số liệu khớp |
| Bảo trì | Chế độ hạn chế đúng năng lực/phạm vi nhằm bảo toàn an toàn/toàn vẹn | M11/vận hành | Quyền khẩn cấp hoặc tắt toàn hệ thống mặc định |
| Kill switch | Cơ chế đã đăng ký để dừng năng lực gây hại bằng quyền cố định đã cấp trước | M11/module nguồn | Đường cấp quyền khẩn cấp/bypass audit |
| Sự cố | Sự kiện có tác động cần phân mức, chỉ huy, khống chế, khôi phục, truyền thông và hậu kiểm | M11/vận hành | Mọi lỗi kỹ thuật đơn lẻ |
| Tác nhân quản trị | Identity bất biến được xác thực thực hiện hoặc yêu cầu hành động, kèm actor type và session/credential context | M01/M11 | Username hiển thị, service name tự khai hoặc subject bị tác động |
| Chủ thể bị tác động | Identity/account mà hành động hướng tới; luôn tách khỏi actor dù self-service | Module nguồn/M01 | Actor mặc định hoặc path ID đã được authorize |
| Tài nguyên quản trị | Đối tượng có owner, type, ID/version và invariant mà action đọc hoặc thay đổi | Module nguồn | DTO, route hoặc hàng giao diện |
| Hành động quản trị | Verb ổn định trên resource type, có effect/risk/data class và permission riêng | M11/module nguồn | Tên controller, button hoặc role |
| Điểm cưỡng chế | Boundary server/workload kiểm actor, permission, scope, state, re-auth, resource version và policy trước effect | Module nguồn/M01/M11 | Client guard, attribute role duy nhất hoặc log sau mutation |
| Quyết định cho phép | Kết quả allow/deny/challenge có policy/version/reason và input protected refs tại một thời điểm | M11/M01 | Quyền lưu vĩnh viễn hoặc cache không hết hạn |
| Phiên quản trị | Session class đặc quyền ngắn, gắn actor/security/policy/family version và current-state enforcement | M01 | Access token dài hạn hoặc role claim tự đủ |
| Lý do nghiệp vụ | Mã + mô tả đã giới hạn, gắn case/change/incident và required theo action risk | M11/module nguồn | Comment tùy ý thay permission/proof |
| Bằng chứng | Artifact/metadata tối thiểu chứng minh input, decision hoặc result, có owner/integrity/retention/redaction | Chủ gate/M11 | Payload thô, screenshot không nguồn hoặc log tự khai |
| Yêu cầu phê duyệt | Record review/decision chỉ tồn tại khi action policy yêu cầu; có scope/version/expiry và invalidation | M11 | Hai người bắt buộc cho mọi action hoặc quyền thực thi độc lập |
| Bộ chính sách | Tập configuration/version liên quan được validate, activate và rollback nguyên khối | M11/module nguồn | Nhiều key sửa rời rạc cùng timestamp |
| Rollout giới hạn | Áp version cho cohort ổn định trong eligibility/scope đã duyệt, có metric và stop criteria | M11/module nguồn | Random lại mỗi request hoặc bypass policy cho test account |
| Ngừng sử dụng | Trạng thái không cho consumer mới nhưng giữ version/reference/history đến khi retention và reference check cho phép xóa | Module nguồn/M11 | Xóa cứng hoặc tự fallback sang default |
| Lưu giữ điều tra | Hold có authority, reason, scope, start/expiry/review, chỉ trì hoãn disposition đã nêu | M11/privacy owner | Giữ vô hạn toàn bộ log/PII |
| Kết quả đối soát | So sánh có source snapshot, key, expected/actual, tolerance, finality và action owner | Module nguồn/M11 | Dashboard delta không truy về nguồn |

## Thuật ngữ bị cấm hoặc cần diễn giải lại

| Mã | Thuật ngữ/cách dùng | Quy tắc chuẩn | Nguồn quyết định | Trạng thái |
|---|---|---|---|---|
| M11-DICT-C01 | Quyền tạm thời/đặc quyền khẩn cấp | Không được hỗ trợ trong Giai đoạn A; M11-T006-A không thấy explicit model nhưng giữ runtime/deployment gaps | M11-D004 | Negative evidence M11-NO-EMERGENCY-A-1.0; runtime mở |
| M11-DICT-C02 | Duyệt hai người là kiểm soát bắt buộc | Không có bước duyệt hai người bắt buộc; dùng quyền tối thiểu, xác minh lại, lý do và audit | M11-D005–D006 | Khóa theo quyết định |
| M11-DICT-C03 | Người phê duyệt là vai trò bắt buộc thường trực | Chỉ dùng “reviewer/người xác nhận” cho artifact/gate hoặc authority được giao; không tự tạo workflow hai người | M11-D005–D007 | Chờ rà soát tài liệu cũ |
| M11-DICT-C04 | Admin/SuperAdmin đủ mô tả quyền | Mọi hành động phải ánh xạ quyền và phạm vi; vai trò rộng là ngoại lệ | M11-D001–D003 | Chờ ma trận M11-T002–T004 |
| M11-DICT-C05 | Log, activity và audit thay thế nhau | Ba loại có mục đích, trường, quyền và retention riêng | M11-D018–D021 | Chờ M11-T031–T035 |
| M11-DICT-C06 | Role claim/`Admin` đồng nghĩa được phép | Permission + scope + state + re-auth + resource policy mới quyết định | M11-D001–D003 | Chờ M11-T002–T005 |
| M11-DICT-C07 | `requestingUserId = null` nghĩa là admin hợp lệ | Actor phải từ trusted session/workload; nullable ID không là authorization evidence | M11-D001–D003 | Sai lệch mã, chờ M11-T002–T005 |
| M11-DICT-C08 | `LastUpdatedBy` username là actor audit | Audit dùng immutable protected actor ref + effective role/scope/session, username chỉ là display snapshot nếu được phép | M11-D018–D020 | Chờ M11-T031–T035 |
| M11-DICT-C09 | `Active`/`Failed`/`Closed` dùng chung toàn hệ thống | Luôn namespace: account/session/config/job/case/incident/result state | M11-D008–D022 | Áp dụng ngay cho contract mới |
| M11-DICT-C10 | Health success hoặc HTTP 200 chứng minh correctness | Health gồm liveness/readiness/correctness/freshness/dependency và không thay business reconcile | M11-D014, D022, D026–D027 | Chờ M11-T036–T040 |

## Ranh giới quyền quyết định

| Chủ | Quyết định cuối | M11 được làm | M11 không được làm |
|---|---|---|---|
| M01 | Actor identity, account/session/security state, authentication/re-auth | Yêu cầu proof class/freshness và consume authorization result | Tự phát session, sửa role/account state hoặc tin claim stale |
| Module nguồn M02–M10 | Resource invariant, version, ownership, effect và compensation nghiệp vụ | Đăng ký action/resource, điều phối request, hiển thị result và audit | Bỏ validation, sửa DB/source trực tiếp hoặc tự tạo effect bù |
| M11 | Permission/scope catalog, change/case/audit/incident governance, admin UX | Deny by default, yêu cầu reason/re-auth/case, lưu decision/evidence | Biến role/UI thành permission hoặc che failure nguồn bằng success |
| M12 | Capability/dependency/secret/data-flow/fail-mode semantics | Dùng health/result contract và yêu cầu reconcile/fail-safe | Suy activation/health từ DI/config hoặc đọc secret để “xác nhận” |
| Privacy/legal owner | Data class, retention/disposition/hold và market constraint | Cưỡng chế decision version, che/xóa theo contract | Tự suy consent/legal basis hoặc giữ vô hạn |

## Namespace trạng thái

| Namespace | Ví dụ trạng thái | Source truth | Không được suy từ |
|---|---|---|---|
| `accountState` | PendingVerification, Active, AdminLocked, PendingDeletion, Deleted | M01 account lifecycle | Role claim, login response cũ |
| `sessionFamilyState` | Active, Revoked, Compromised, Expired | M01 session store | JWT expiry riêng lẻ |
| `changeRequestState` | Draft, Validated, Scheduled, Applying, Applied, Failed, RolledBack | M11 change store | Config current value hoặc audit event |
| `configurationState` | DraftVersion, ScheduledVersion, EffectiveVersion, DeprecatedVersion | M11/module configuration store | `IsActive` boolean chung |
| `jobRunState` | Queued, Running, Paused, Succeeded, Failed, Compensating | Module/M11 job store | Process/thread còn sống |
| `supportCaseState` | Open, Assigned, Waiting, Resolved, Closed, Reopened | M11 case store | User account state |
| `incidentState` | Detected, Triaged, Contained, Recovering, Monitoring, Closed | M11 incident store | Alert acknowledged hoặc service HTTP 200 |
| `capabilityState` | Unknown, Disabled, Healthy, Degraded, Isolated | M12 capability registry/runtime health | Config/credential/DI presence |

## Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Xung đột/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M11-DICT-I01 | `UserRole` chỉ có User/Admin/SuperAdmin | Role rộng không biểu diễn trách nhiệm/phạm vi chuyên biệt | M11-T003–T005; M01-T028–T030 |
| M11-DICT-I02 | Nhiều endpoint dùng chuỗi `[Authorize(Roles = ...)]` trực tiếp | Role bị dùng như permission/enforcement đầy đủ; scope/resource/state/re-auth không thấy | M11-T002–T005; A-G02 |
| M11-DICT-I03 | Một số service dùng `requestingUserId = null` để hiểu là admin bỏ ownership check | Nullable value trở thành authority, dễ confused-deputy/bypass | M11-T002–T005; M02/M11 acceptance |
| M11-DICT-I04 | Quyền Admin/SuperAdmin không nhất quán giữa controller; có route chỉ Admin hoặc chỉ SuperAdmin | Cùng tên role không cho biết action/risk/owner hoặc lý do khác biệt | M11-T002–T004 |
| M11-DICT-I05 | `SystemConfiguration.LastUpdatedBy` lưu username | Actor mutable/display value không đủ audit bất biến | M11-T012–T014, T031 |
| M11-DICT-I06 | Activity/System log hiện được dùng cho một số mô tả admin operation | Chưa tách audit evidence khỏi activity/operational diagnostics | M11-T031–T035 |
| M11-DICT-I07 | Không thấy permission/scope/policy-decision model hoặc admin re-auth enforcement chung | Không chứng minh deny-by-default/current-state/least privilege | M11-T003–T007; M01-T028–T030 |

I01–I07 là baseline gap, không phải bằng chứng runtime; action catalog và permission matrix phải dùng thuật ngữ v1.0 thay vì hợp thức hóa role check hiện tại.

## Ma trận tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| MD11-01 | UI ẩn nút nhưng gọi API trực tiếp | Điểm cưỡng chế server deny nếu thiếu permission/scope/state/re-auth |
| MD11-02 | Actor Admin có role nhưng ngoài scope tài nguyên | Deny; role không tự cấp permission toàn hệ thống |
| MD11-03 | Claim role cũ sau thu hồi | Current source decision deny, không chờ token hết hạn |
| MD11-04 | `requestingUserId = null` từ caller | Không suy admin; actor lấy từ trusted context |
| MD11-05 | Activity “AdminUpdated” tồn tại nhưng audit thiếu | Không coi action nhạy cảm có bằng chứng; fail/reconcile theo policy |
| MD11-06 | Operational log chứa request payload/password/token | Redaction reject trước sink; log không thành audit |
| MD11-07 | Config được lưu nhưng chưa đến effective time | Vẫn là scheduled version; không gọi Active trần |
| MD11-08 | Job process chết nhưng record còn Running | Job state/fencing/timeout quyết định; process liveness không là truth |
| MD11-09 | Alert được acknowledge | Incident không tự Closed; cần containment/recovery/verification |
| MD11-10 | Health endpoint 200 nhưng reconciliation stale | Capability không được gọi Healthy/correct chỉ từ HTTP 200 |
| MD11-11 | “Khẩn cấp” cần thao tác | Dùng quyền cố định + re-auth/reason/audit; không tạo temporary/emergency privilege |
| MD11-12 | Review artifact bởi cùng owner | Có thể hợp lệ trong solo workflow; không bị diễn giải thành two-person business approval |
| MD11-13 | Module nguồn từ chối mutation do invariant | M11 giữ result nguồn, không ghi DB hoặc báo success bù |
| MD11-14 | User bị xóa sau admin action | Audit giữ protected immutable actor/subject refs theo retention, không phụ thuộc display identity |
| MD11-15 | Cùng từ `Active` xuất hiện ở account/config/session | Contract/event bắt buộc namespace và owner, không map tự động |
| MD11-16 | Đổi định nghĩa hoặc owner thuật ngữ | Tăng dictionary version, ghi compatibility/consumer migration và decision |

## Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-DICT-F02 | Sensitive-action governance và runtime privilege regression | No-temporary/emergency có negative evidence tĩnh; role/permission/grant lifecycle đã chốt | M11-T007, T049; M01-T029–T030 |
| M11-DICT-F03 | Audit/activity/log schema, integrity, access và retention | Không dùng log/activity thay audit; sensitive mutation thiếu audit fail-closed | M11-T031–T035 |
| M11-DICT-F04 | Health/job/incident runtime state và evidence | Unknown/degraded không thành healthy; retry/recovery không dựa process memory | M11-T036–T048; A-G06 |

## Tự kiểm M11-T001, A-G02 và A-G06

- 38 thuật ngữ chuẩn có một định nghĩa, owner và anti-meaning; actor/subject/resource/action/permission/scope/enforcement được tách rõ.
- Năm nhóm owner và tám namespace state giữ ranh M01/module nguồn/M11/M12/privacy, không dùng status/role chung làm business truth.
- M11-DICT-C01–C10 khóa cách dùng gây bypass hoặc nhập nhằng; 7 sai lệch tĩnh và 3 finding mở có task tiếp nhận; M11-DICT-F01 đã được đóng bởi M11-ACTION-1.0.
- 16 case bao phủ role/permission/scope, stale claim, audit/log, config/job/incident/health và module-source boundary.
- A-G02/A-G06 có vocabulary baseline nhưng chưa permission matrix/runtime evidence; không kết luận gate đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa bản nháp từ quyết định M11 và ghi các thuật ngữ lỗi thời cần loại | Chưa gán |
| 2026-08-20 | 1.0 | Chốt 38 thuật ngữ, ownership/state namespace, 10 xung đột, 7 sai lệch và 16 case | WSA-7K2 |
