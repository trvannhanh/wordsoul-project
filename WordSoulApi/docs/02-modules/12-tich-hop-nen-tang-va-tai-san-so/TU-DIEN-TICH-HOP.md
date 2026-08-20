# Từ điển tích hợp M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T001 |
| Phiên bản | M12-DICT-1.0 |
| Trạng thái | Baseline thuật ngữ có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-018; D-007, D-008; M12-D001–D004, D019, D021–D025 |
| Phạm vi | Năng lực ngoài/nền tảng, request/attempt/result/error, resilience, shared state, asset và observability |
| Ngoài phạm vi | Registry instance M12-T002; criticality M12-T003; schema contract M12-T004; taxonomy result/error M12-T005 |

## Nguyên tắc dùng từ

- Module nghiệp vụ chỉ phụ thuộc **năng lực** và contract chuẩn, không phụ thuộc tên provider, SDK, HTTP status hoặc exception text.
- Kết quả provider là bằng chứng đầu vào cho adapter; chỉ **kết quả chuẩn** mới được trả cho consumer. Provider accepted không tự nghĩa business completed/delivered.
- `null`, chuỗi rỗng, timeout hoặc exception không phải taxonomy kết quả. Chúng phải được ánh xạ có chủ đích hoặc giữ là `unknown`.
- Mã hoạt động nghiệp vụ do source module tạo và không đổi qua retry; M12 tạo attempt/correlation riêng nhưng không thay identity gốc.
- Cache/shared state không là nguồn sự thật bền vững. Fallback/degradation không được bịa kết quả hoặc ghi đè durable truth.

## Chủ ngữ và ranh giới

| ID | Thuật ngữ | Định nghĩa chuẩn | Không phải |
|---|---|---|---|
| INT-001 | Năng lực (`capability`) | Chức năng ổn định mà M12 cung cấp theo contract, độc lập implementation/provider | Một SDK, endpoint URL hoặc vendor account |
| INT-002 | Source module | Module sở hữu intent/nghiệp vụ và tạo business operation ID | Provider hoặc adapter tự quyết định nghiệp vụ |
| INT-003 | Consumer | Thành phần dùng kết quả chuẩn để tiếp tục journey | Thành phần được quyền suy diễn từ payload provider |
| INT-004 | Provider | Hệ thống/dịch vụ thực thi bên ngoài hoặc platform dependency cụ thể | Nguồn sự thật cho quyền/kết quả nghiệp vụ WordSoul |
| INT-005 | Adapter | Boundary chuyển contract chuẩn sang provider request và ngược lại | Nơi chứa policy sản phẩm, ownership hoặc scoring |
| INT-006 | Capability owner | Chủ chịu trách nhiệm contract, registry, degradation và readiness của năng lực | Chủ tài khoản vendor mặc nhiên |
| INT-007 | Business decision owner | Module quyết định có gọi, chấp nhận/fallback và áp dụng kết quả hay không | M12/provider tự xuất bản/cấp thưởng/chấm đạt |
| INT-008 | Operational owner | Chủ cấu hình, secret, quota, health, incident và provider change | Người được quyền thay contract nghiệp vụ âm thầm |

## Định danh và yêu cầu

| ID | Thuật ngữ | Định nghĩa chuẩn | Bất biến |
|---|---|---|---|
| INT-009 | Business operation ID | ID opaque, duy nhất do source module tạo cho một intent nghiệp vụ | Ổn định qua retry/provider switch; fingerprint khác phải conflict |
| INT-010 | Request fingerprint | Digest canonical của trường contract ảnh hưởng semantics | Không chứa secret/PII thô; dùng phát hiện cùng ID khác payload |
| INT-011 | Attempt ID | ID cho một lần thực thi kỹ thuật của operation | Mỗi attempt mới có ID mới nhưng cùng business operation ID |
| INT-012 | Correlation ID | ID truy vết xuyên boundary cho một journey/trace | Không là idempotency key, credential hoặc business truth |
| INT-013 | Contract version | Phiên bản schema + semantics giữa source/consumer và capability | Provider API version là metadata riêng |
| INT-014 | Purpose | Lý do xử lý đã allowlist, gắn minimization/retention/consent | Free text do client/provider gửi |
| INT-015 | Deadline | Thời điểm tuyệt đối sau đó kết quả không còn được phép áp dụng cho intent | Không đồng nghĩa timeout từng attempt |
| INT-016 | Timeout | Ngân sách chờ tối đa của một attempt/call | Không chứng minh provider chưa áp dụng effect |
| INT-017 | Cancellation | Tín hiệu ngừng công việc chưa cần tiếp tục | Không bảo đảm remote effect đã bị hủy |

## Kết quả chuẩn

| ID | Thuật ngữ | Định nghĩa chuẩn | Consumer bắt buộc |
|---|---|---|---|
| INT-018 | Kết quả chuẩn (`capability result`) | Envelope versioned gồm operation/attempt, status, data/ref tối thiểu, reason category và timing | Chỉ dùng envelope này, không branch theo provider text/status |
| INT-019 | `succeeded` | Contract được thực thi và effect/kết quả xác định hợp lệ | Áp dụng idempotently theo business rule |
| INT-020 | `noData` | Request hợp lệ và xác định không có dữ liệu/kết quả | Phân biệt với lỗi/unknown; chỉ coi hợp lệ nếu business cho phép |
| INT-021 | `unknown` | Không xác định provider/effect đã xảy ra hay chưa | Không giả success/failure; reconcile bằng operation ID |
| INT-022 | `temporaryFailure` | Thất bại phân loại có thể retry trong deadline/budget | Retry giới hạn cùng operation ID nếu policy cho phép |
| INT-023 | `permanentFailure` | Thất bại cuối với request/destination hiện tại | Không retry mù; sửa intent/input hoặc kết thúc an toàn |
| INT-024 | `expired` | Deadline/TTL nghiệp vụ đã qua | Không áp dụng late result; request mới cần operation mới nếu policy cho phép |
| INT-025 | `cancelled` | Intent bị hủy trước khi hoàn tất | Dừng dispatch; reconcile remote effect nếu còn unknown |
| INT-026 | `rejected` | Capability từ chối trước/không thực thi do policy, auth, quota cuối hoặc contract | Không coi là provider outage; trả reason category an toàn |
| INT-027 | Late result | Response/event tới sau deadline, cancellation hoặc result cuối | Lưu trace/reconcile; không ghi đè business truth |
| INT-028 | Provider outcome | Status/payload thô từ provider trong adapter boundary | Không trả thẳng cho module, log hoặc evidence |

## Khả năng phục hồi và suy giảm

| ID | Thuật ngữ | Định nghĩa chuẩn | Ranh giới |
|---|---|---|---|
| INT-029 | Retry | Attempt mới cho cùng operation sau temporary/unknown theo policy | Không đổi operation/fingerprint và không vượt deadline/budget |
| INT-030 | Reconciliation | Đối chiếu operation với remote/local durable truth khi result unknown/partial | Không phải retry mù hoặc compensation tự động |
| INT-031 | Compensation | Mutation nghiệp vụ mới để trung hòa effect đã xác định | Có ID/audit/policy riêng; không xóa history trực tiếp |
| INT-032 | Fallback | Cách thay thế đã duyệt để đạt cùng/giảm semantics | Không tự switch provider hay dữ liệu thiếu kiểm chứng |
| INT-033 | Degradation | Trải nghiệm/năng lực giảm có chủ đích khi dependency không đáp ứng | Không biến lỗi thành success hoặc bỏ control |
| INT-034 | Fail-closed | Từ chối/dừng khi dependency/control không xác định | Không nhất thiết trả lỗi chi tiết cho user |
| INT-035 | Fail-open | Cho tiếp tục trong phạm vi đã duyệt khi control phụ lỗi | Không dùng cho auth, quyền, cost-sensitive mutation mặc định |
| INT-036 | Conservative mode | Cho phép phạm vi đọc/low-risk có trần/local guard, còn mutation nhạy cảm dừng | Không đồng nghĩa allow-all |
| INT-037 | Circuit breaker | Ngăn gọi dependency đang lỗi và thăm dò phục hồi có kiểm soát | Không thay health truth hoặc business fallback |
| INT-038 | Bulkhead | Cô lập concurrency/queue/resource theo capability/provider | Không là rate limit người dùng |
| INT-039 | Retry budget | Trần attempt/thời gian/chi phí cho operation/capability | Không reset khi worker restart/provider switch |

## Trạng thái dùng chung và điều phối

| ID | Thuật ngữ | Định nghĩa chuẩn | Không phải |
|---|---|---|---|
| INT-040 | Shared state | Cache/coordination state dùng chung giữa instance, có namespace/TTL/owner | Durable business source mặc định |
| INT-041 | Cache hit | Có entry đúng key/version và còn hiệu lực | Business success nếu entry stale/không được phép dùng |
| INT-042 | Cache miss | Không có entry usable | Provider failure hoặc noData |
| INT-043 | Stale entry | Entry quá TTL/version/invalidation condition | Fallback hợp lệ nếu contract chưa cho phép stale |
| INT-044 | Distributed lock/lease | Quyền tạm thời để phối hợp một critical section | Bằng chứng effect chỉ xảy ra một lần |
| INT-045 | Fencing token | Số/epoch tăng để durable writer từ chối owner lease cũ | Correlation ID hoặc timestamp client |
| INT-046 | Rate limit | Giới hạn tần suất/chi phí theo partition và cửa sổ/bucket | Provider quota hoặc concurrency bulkhead |
| INT-047 | Provider quota | Giới hạn do provider/account áp đặt | Product rate limit; không được expose nguyên trạng |
| INT-048 | Workload identity | Danh tính xác thực của service/worker nội bộ | Cờ/header tự khai để bypass quota |

## Dữ liệu, tài sản và vận hành

| ID | Thuật ngữ | Định nghĩa chuẩn | Yêu cầu |
|---|---|---|---|
| INT-049 | External data flow | Trường/nhóm dữ liệu rời trust boundary để provider xử lý | Có purpose, minimization, region, retention, consent/legal-basis ref và deletion |
| INT-050 | Secret | Credential/key/token cho service/provider | Không vào Git/log/evidence/payload consumer; có owner/rotation/revocation |
| INT-051 | Provider payload | Request/response/event dạng provider-native | Chỉ trong adapter tối thiểu; redact/không persist trừ allowlist |
| INT-052 | Asset reference | ID bất biến trỏ tới metadata/quyền/location versioned | Không dùng public URL làm ownership truth |
| INT-053 | Source of truth | Durable owner quyết trạng thái cuối của dữ liệu/effect | Cache, health probe hoặc provider accepted status |
| INT-054 | Health | Tín hiệu đo được về khả năng đáp ứng contract của capability | Config tồn tại hoặc process sống |
| INT-055 | Availability | Tỷ lệ request/journey đáp ứng semantics trong cửa sổ | Provider uptime nguyên bản |
| INT-056 | SLO | Mục tiêu đo được cho capability/journey do WordSoul sở hữu | SLA provider sao chép nguyên trạng |
| INT-057 | Kill switch | Control có audit để dừng capability/provider/operation class an toàn | Xóa config/secret thủ công không truy vết |
| INT-058 | Canary | Phạm vi traffic/tenant giả hoặc allowlist nhỏ để kiểm chứng version/provider mới | Mở rộng production không có stop criteria |

## Ma trận quyền quyết định

| Quyết định | Chủ | M12 được làm | M12 không được làm |
|---|---|---|---|
| Có gọi capability hay không | Source/business module | Validate contract/purpose, enforce platform policy | Tự tạo intent nghiệp vụ |
| Kết quả có làm đổi business state | Business decision owner | Trả status/data/ref chuẩn | Tự publish, chấm đạt, cấp thưởng, liên kết account |
| Chọn/switch provider | Capability + operational owner theo policy | Route theo registry/health/canary đã duyệt | Đổi semantics hoặc data purpose |
| Retry/reconcile | Capability owner trong deadline/budget | Tạo attempt, query operation, emit status | Tạo operation mới để né idempotency |
| Fallback/degradation | Business owner + capability contract | Thực thi mode đã đăng ký | Tự bịa data/success hoặc fail-open control nhạy cảm |
| Secret/quota/health | Operational owner | Enforce/measure/alert/kill switch | Expose secret/raw quota/provider payload cho consumer |

## Xung đột hiện trạng cần hội tụ

Kết quả quan sát tĩnh ngày 2026-08-20; chưa phải bằng chứng runtime.

| Mã | Xung đột | Hệ quả | Task tiếp nhận |
|---|---|---|---|
| M12-DICT-I01 | Nhiều adapter trả `null` cho no-data, provider error, exception và invalid response | Consumer không phân biệt `noData`, `unknown`, temporary/permanent failure | M12-T004–T005 |
| M12-DICT-I02 | `SendGridEmailService` trả `Task`, log status rồi nuốt exception | Không có capability result; caller có thể coi failure là success | M12-T004–T005, M12-T026–T029 |
| M12-DICT-I03 | FCM/realtime interfaces nhận token/object/provider-shaped payload trực tiếp | Provider identity/payload xuyên boundary nghiệp vụ | M12-T004, M12-T027–T028 |
| M12-DICT-I04 | Gemini exception chứa/log provider response body | Provider payload có thể rời adapter/log boundary | M12-T040–T043, M11-T033 |
| M12-DICT-I05 | Cache Redis trả `null` cho miss lẫn failure | Miss bị đồng nhất dependency outage, khó quyết fallback/metric | M12-T005, M12-T031–T032 |
| M12-DICT-I06 | Redis limiter mô tả và thực thi allow-all khi store lỗi | `fail-open` bị dùng như global default trái M12-D022/CT-05 | M12-T034–T035 |
| M12-DICT-I07 | Interface hiện thiếu operation/attempt/deadline/purpose/contract version | Không chống lặp, dừng late result hoặc truy vết xuyên provider | M12-T004, M12-T036–T037 |
| M12-DICT-I08 | Provider HTTP status/exception text là taxonomy lỗi thực tế | Module/UX có thể phụ thuộc vendor và đổi semantics khi provider đổi | M12-T004–T005, M12-T048 |

## Finding còn mở

| Mã | Phần chưa chốt | Baseline an toàn | Nguồn/task xử lý |
|---|---|---|---|
| M12-DICT-F01 | Danh sách capability/provider/environment đang hoạt động | Không suy từ config là enabled/healthy; registry ghi `unknown` đến khi kiểm chứng | M12-T002; REL-03 |
| M12-DICT-F02 | Criticality, tolerance và SLO cụ thể | Không sao chép SLA provider; mutation nhạy cảm fail-closed/conservative | M12-T003, M12-T045 |
| M12-DICT-F03 | Schema result/error và mapping từng provider | Không trả null/raw payload; giữ `unknown` khi chưa xác định effect | M12-T004–T005 |
| M12-DICT-F04 | Fallback/fail mode theo capability | Không allow-all mặc định; chỉ mode được business owner đăng ký | M12-T030, M12-T035; REL-03 |

## Tự kiểm M12-T001 và A-G04

- 58 thuật ngữ phân biệt capability/provider, business/provider result, operation/attempt/correlation, timeout/unknown và fallback/degradation/fail mode.
- Chủ quyết định được xác định cho intent, business effect, provider route, retry, fallback và vận hành.
- Tám xung đột hiện trạng và bốn finding mở có baseline an toàn cùng task tiếp nhận.
- Từ điển cung cấp vocabulary cho registry, contract, error taxonomy và bộ bằng chứng A-G04; không tự kết luận capability đang healthy.
- A-G04 vẫn mở vì registry, implementation và runtime degradation evidence chưa có.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 58 thuật ngữ, ma trận quyền quyết định và xung đột hiện trạng | WSA-7K2 |
