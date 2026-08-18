# Gói task thực thi Lát 4–Lát 5 Giai đoạn A

## 1. Mục đích và phạm vi

Gói cuối Giai đoạn A hoàn thiện resilience, health, ứng phó sự cố, quyền dữ liệu và nghiệm thu Cổng A. Phạm vi gồm:

- 16 task nguồn Lát 4 của M11/M12.
- 7 task nguồn/lát con Lát 5 của M01.
- 10 task điều phối A5 để rà soát 145 task, nghiệm thu A-G01–A-G06 và ra quyết định mở/không mở Giai đoạn B.

Tổng cộng 23 task nguồn duy nhất và 10 task điều phối. Các task M11-T027–M11-T035 và M11-T038–M11-T040 đã được sở hữu bởi Lát 2/3, Lát 5 chỉ sử dụng bằng chứng của chúng và không tạo bản task trùng.

## 2. Quy tắc lát A bổ sung

| Lát A | Task nguồn | Phạm vi A | Phần giữ mở |
|---|---|---|---|
| M11-T043-A | M11-T043 | Bảo trì cho API và năng lực Giai đoạn A | Truyền thông/điều phối kênh M10 đầy đủ |
| M11-T047-A | M11-T047 | Truyền thông nội bộ, thông báo bắt buộc trong phạm vi sẵn có và hậu kiểm | Kênh người dùng tùy chọn/diện rộng M10 |
| M12-T047-A | M12-T047 | Contract test/canary cho provider đang hoạt động trong A/B | AI, giọng nói, realtime chưa phát hành |
| M01-T042-A | M01-T042 | Nghiệm thu M01 trong phạm vi A và các lát A/B đã chốt | Nhánh module/kênh bị hoãn |
| M01-T043-A | M01-T043 | Bàn giao M01 cho phạm vi A/B | Bàn giao bổ sung khi nhánh hoãn được mở |

## 3. Điều kiện vào

- Registry, quyền, audit, redaction, version và asset lifecycle liên quan từ Lát 1–3 đã đạt.
- REL-01–REL-04 và REL-07 có hồ sơ, chủ và danh sách bằng chứng; CT-01–CT-07 vẫn được kiểm tra.
- Sổ bằng chứng và sáu hồ sơ A-G01–A-G06 đã được tạo từ mẫu cho đúng phạm vi phát hành.
- Không dùng điểm health hiện tại, log payload thô hoặc thao tác bảo trì placeholder làm bằng chứng đạt.

## 4. Lát 4 — Resilience, health và ứng phó sự cố

### 4.1. M11 — Vận hành và sự cố

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M11-T036 | Lập sổ sức khỏe năng lực và tích hợp | Từ điển chỉ số; registry M12 | Sổ owner, SLI/SLO, nguồn health thật, độ mới, degraded, RTO/RPO và playbook | Chủ vận hành | Chủ module/M12 | P0 | L | M11-T022; M12-T002–T005 | A-G04, A-G06; REL-03 | Chờ Lát 1–2 |
| M11-T037 | Thiết kế cảnh báo và escalation | M11-T036 | Ngưỡng, severity, chống nhiễu, owner/on-call, escalation, acknowledgement và playbook | Chủ vận hành | An toàn hệ thống/chủ module | P0 | L | M11-T036 | A-G06; REL-03 | Chờ health registry |
| M11-T043-A | Thiết kế chế độ bảo trì — lát A | Health registry; permission/change control | Scope API/năng lực, quyền, lịch, thông báo bắt buộc, behavior request đang chạy và exit validation | Chủ vận hành | Chủ sản phẩm/an toàn hệ thống | P0 | L | M11-T036; hợp đồng M10 tối thiểu | A-G06 | Lát A; không trả thành công giả |
| M11-T044 | Thiết kế kill switch và dừng khẩn | Không có quyền khẩn M11-T006-A; rollback; health | Kill switch theo năng lực, quyền thường đã duyệt, fail state, audit, recovery và đối soát | Chủ an toàn vận hành | Chủ sản phẩm/M11 | P0 | L | M11-T006-A, T017, T036 | A-G04, A-G06; REL-02, REL-03 | Không tạo quyền khẩn cấp |
| M11-T045 | Xây dựng mô hình mức độ sự cố | M11-T036–T037 | SEV-1–SEV-4, tiêu chí, owner, response, update cadence và exit criteria | Incident lead | Chủ sản phẩm/an toàn hệ thống | P0 | M | M11-T036, T037 | A-G06 | Chờ alert model |
| M11-T046 | Xây dựng playbook sự cố trọng yếu | Alerts, reconciliation, maintenance, kill switch, severity | Playbook phát hiện–khống chế–bảo toàn–khôi phục–đối soát cho kịch bản trọng yếu | Incident lead | Chủ module/vận hành | P0 | L | M11-T037, T040-A, T043-A–T045 | A-G06; REL-02, REL-03, REL-07 | Chờ các control trước |
| M11-T047-A | Thiết kế truyền thông và hậu kiểm — lát A | Audit; playbook; hợp đồng kênh tối thiểu | Nhịp nội bộ, thông báo bắt buộc khả dụng, phê duyệt nội dung, post-incident và action owner | Incident communications lead | Chủ sản phẩm/pháp lý khi cần | P0 | L | M11-T031, T046 | A-G06 | Lát A; M10 đầy đủ giữ mở |
| M11-T048 | Chốt mục tiêu phục hồi và diễn tập | Severity/playbook/comms; resilience M12 | RTO/RPO theo năng lực, exercise plan, safety stop, kết quả, finding và re-test | Chủ vận hành | An toàn hệ thống/chủ sản phẩm | P0 | L | M11-T045–T047-A; M12-T036–T047-A | A-G06; REL-02, REL-03 | Chờ playbook/resilience |

### 4.2. M12 — Shared state, resilience và quản trị provider

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M12-T032 | Thiết kế namespace, TTL và invalidation | Shared-state registry; secret/data policy | Namespace env/module/version, key không PII, TTL/invalidation/source fallback và stampede test | Chủ nền tảng M12 | An toàn hệ thống/chủ module | P0 | L | M12-T031, T040 | A-G04, A-G05; REL-03 | Chờ Lát 1 |
| M12-T033 | Chốt khóa phân tán và ownership | Shared-state registry | Owner/fencing/lease/renew/release/lost-lock behavior và duplicate prevention | Chủ nền tảng M12 | Chủ module/an toàn hệ thống | P0 | L | M12-T031 | A-G04; REL-03 | Chờ use-case registry |
| M12-T036 | Chuẩn hóa timeout, deadline và hủy | Criticality; contract/error taxonomy | Budget từng năng lực, deadline propagation, cancellation, late-result rule và metrics | Chủ M12 | Vận hành/chủ module | P0 | L | M12-T003–T005 | A-G04, A-G06; REL-03 | Chờ Lát 1 |
| M12-T037 | Chuẩn hóa retry và idempotency | Error taxonomy; deadline | Retryable errors, backoff/jitter, cap, request identity, reconciliation/compensation | Chủ M12 | An toàn hệ thống/chủ module | P0 | L | M12-T005, T036 | A-G04; REL-03 | Chờ deadline |
| M12-T038 | Thiết kế circuit breaker và bulkhead | Criticality; timeout/retry | Threshold/state/half-open/concurrency/isolation và probe recovery | Chủ nền tảng M12 | Vận hành/an toàn hệ thống | P0 | L | M12-T003, T036, T037 | A-G04, A-G06; REL-03 | Chờ retry |
| M12-T045 | Định nghĩa SLO và health từng năng lực | Criticality/error taxonomy; M11 health model | SLI/SLO/error budget, correctness/freshness/quota, owner và degraded visibility | Chủ M12/vận hành | Chủ module | P0 | L | M12-T003, T005; M11-T036 | A-G04, A-G06; REL-03 | Phối hợp M11-T036 |
| M12-T046 | Thiết kế đo usage, chi phí và ngân sách | Registry; limiter; SLO | Unit cost, attribution không PII, budget/threshold/owner, invoice reconciliation và kill action | Chủ vận hành chi phí | Sản phẩm/tài chính/chủ M12 | P0 | L | M12-T002, T034, T045 | A-G04, A-G06; REL-03 | Chờ SLO/limiter |
| M12-T047-A | Xây dựng kiểm thử hợp đồng và canary — lát A | Hợp đồng/error taxonomy; provider A/B | Test success/error/schema định kỳ, canary stop, deprecation/version drift cho provider đang hoạt động | Chủ chất lượng M12 | Chủ module/vận hành | P0 | L | M12-T004, T005; active-provider contracts | A-G04, A-G06; REL-03 | Lát A theo ngoại lệ kế hoạch |

## 5. Lát 5A — Xuất, xóa và đối soát dữ liệu

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T033 | Lập bản đồ dữ liệu cá nhân liên module | Profile map; data-flow map; module inventories | Field/source/purpose/controller/retention/export/delete/anonymize và owner cho dữ liệu hiện có | Chủ riêng tư M01 | M01–M12 owners | P0 | L | M01-T003; M11/M12 registries | A-G01, A-G05; REL-01, REL-07 | Chờ inventories |
| M01-T034 | Thiết kế yêu cầu xuất dữ liệu | Recovery/subject verification; M01-T033; support jobs | Request lifecycle, verification, scope, manifest, secure delivery, partial failure, expiry và audit | Chủ M01/riêng tư | M11/an toàn hệ thống | P1 | L | M01-T019, T033; M11-T029, T038–T040-A | A-G01, A-G02; REL-07 | Chờ data map/jobs |
| M01-T035 | Thiết kế yêu cầu xóa tài khoản | Recovery/verification; data map; support/change control | Request, cooling-off, cancel, access freeze, job orchestration, status, proof và audit | Chủ M01/riêng tư | M11/an toàn hệ thống | P0 | L | M01-T019, T033; M11-T029, T038–T040-A | A-G01, A-G02; REL-07 | Chờ data map/jobs |
| M01-T036 | Xây dựng ma trận xóa và ẩn danh hóa | M01-T033, T035; module owner decisions | Per-dataset delete/anonymize/retain/legal hold/reference handling và reconciliation | Chủ riêng tư | M01–M12 owners/pháp lý | P0 | L | M01-T033, T035 | A-G01, A-G05; REL-07 | Chờ mọi owner xác nhận |
| M01-T037 | Xác định quy tắc đăng ký lại sau xóa | Registration/linking; deletion matrix | Cooldown, identifier reuse, no unintended relink, retained-history boundary và test cases | Chủ M01 | Riêng tư/an toàn hệ thống | P1 | M | M01-T005, T015, T036 | A-G01; REL-07 | Chờ deletion matrix |

Các bằng chứng Lát 5A bắt buộc tái sử dụng M11-T027–M11-T035 và M11-T038–M11-T040-A. Không tạo task nguồn mới cho các mã này.

## 6. Lát 5B — Nghiệm thu và bàn giao M01

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T042-A | Xây dựng bộ nghiệm thu xuyên chức năng M01 — lát A | M01 task A; mẫu A-G01/A-G02; REL evidence | Test matrix registration/login/OAuth/session/recovery/profile/role/lock/export/delete, results và findings | Chủ chất lượng M01 | An toàn hệ thống/sản phẩm | P0 | L | M01-T009, T015, T018, T021, T027-A, T032, T037, T041 | A-G01, A-G02; REL-01, REL-02, REL-07 | Lát A; không đóng nhánh hoãn |
| M01-T043-A | Hoàn thiện tài liệu bàn giao M01 — lát A | M01-T042-A; registries/playbooks | Gói lifecycle/contracts/operations/evidence/open slices/owners được duyệt | Chủ M01 | M02/M11/M12 và sản phẩm | P0 | M | M01-T042-A | A-G01, A-G02 | Lát A; cập nhật khi nhánh hoãn mở |

## 7. Task điều phối nghiệm thu A5

| Task ID | Tên task | Input | Output/bằng chứng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|
| A5-T001 | Đóng băng phạm vi nghiệm thu A | Release scope; active/deferred capabilities; slice registry | Baseline version, included/excluded list, owner và change-freeze rule | Release lead | Chủ sản phẩm | P0 | S | Lát 0–4 | Chờ phạm vi được đề xuất |
| A5-T002 | Kiểm tra bao phủ 145 task | Bốn baseline; mọi task pack | Ma trận mỗi task có đúng một owner pack, baseline, parent/slice và evidence requirement | Program lead | Kiến trúc sư/chủ module | P0 | M | A5-T001 | Chờ freeze |
| A5-T003 | Nghiệm thu A-G01 | Mẫu A-G01; M01/M12 evidence; REL-01/07 | Hồ sơ case, findings, re-test và kết luận cổng | Chủ M01 | An toàn hệ thống/sản phẩm | P0 | L | Lát 2, 3D, 5A | Chờ bằng chứng |
| A5-T004 | Nghiệm thu A-G02 | Mẫu A-G02; M01/M11 evidence; REL-02/07 | Permission matrix, denial/audit evidence, findings và kết luận | Chủ M11 | An toàn hệ thống | P0 | L | Lát 2, 3A, 5A | Chờ bằng chứng |
| A5-T005 | Nghiệm thu A-G03 | Mẫu A-G03; M02/M11/M12 evidence; REL-04 | Một tập học liệu mẫu end-to-end, findings và kết luận | Chủ M02 | Học thuật/sản phẩm | P0 | L | Lát 3A–3C | Chờ bằng chứng |
| A5-T006 | Nghiệm thu A-G04 | Mẫu A-G04; M12/M11 evidence; REL-03 | Registry, degradation tests, findings và kết luận | Chủ M12 | Vận hành/an toàn hệ thống | P0 | L | Lát 2, Lát 4 | Chờ bằng chứng |
| A5-T007 | Nghiệm thu A-G05 | Mẫu A-G05; secret/data/asset/log evidence; REL-01/03/04 | Inventory/map/redaction/asset-right results, findings và kết luận | Chủ riêng tư/an toàn nền tảng | Pháp lý/sản phẩm khi áp dụng | P0 | L | Lát 1–4 | Chờ bằng chứng |
| A5-T008 | Nghiệm thu A-G06 | Mẫu A-G06; health/alert/playbook/exercise evidence; REL-02/03 | SLO, four severity levels, exercises, findings và kết luận | Incident lead | Chủ vận hành/sản phẩm | P0 | L | Lát 4 | Chờ diễn tập |
| A5-T009 | Rà soát đóng REL ảnh hưởng A | REL-01–04/07 dossiers; gate findings | Trạng thái, authority, acceptance/adjustment, expiry và unresolved blockers | Program lead | Product/legal/security owners | P0 | M | A5-T003–T008 | Chờ kết luận sáu cổng |
| A5-T010 | Ra quyết định Cổng A | Biên bản quyết định; six gate results; REL review | Quyết định mở/không mở/giới hạn B, phạm vi tắt, conditions và signatures | Người có thẩm quyền phát hành | Các chủ cổng | P0 | M | A5-T002–T009 | Mặc định không mở khi thiếu điều kiện |

## 8. Thứ tự kéo task

| Đợt | Task chính | Kết quả mở khóa |
|---:|---|---|
| 1 | M11-T036–T037; M12-T032–T033, T036, T045; M01-T033 | Health/shared-state/deadline/data map nền |
| 2 | M11-T043-A–T045; M12-T037–T038, T046–T047-A; M01-T034–T036 | Maintenance/kill/severity, resilience/cost/canary và export/delete matrix |
| 3 | M11-T046–T048; M01-T037, T042-A–T043-A | Playbook/comms/exercise và nghiệm thu/bàn giao M01 |
| 4 | A5-T001–T002 | Freeze và kiểm tra coverage |
| 5 | A5-T003–T008 | Sáu cổng được nghiệm thu; finding được tạo và re-test |
| 6 | A5-T009–T010 | REL review và quyết định chính thức |

## 9. Definition of Done chung

- Health không phải ping/giá trị cố định; có correctness, freshness, dependency, quota và degraded state.
- Mọi cảnh báo có owner, severity, escalation và playbook; maintenance/kill switch hoạt động đúng phạm vi.
- Resilience test bao phủ timeout, 429, 5xx, malformed response, lost lock, circuit recovery và duplicate.
- Export/delete xác minh chủ thể, có manifest, status phần lỗi, retry idempotent, anonymization/retention và reconciliation.
- Không có evidence chứa secret/PII thật; mọi artifact có Evidence ID, version, environment và reviewer.
- Mọi lát A giữ parent mở cho nhánh hoãn; quyết định chỉ áp dụng đúng release scope đã freeze.
- A-G01–A-G06 không còn finding nghiêm trọng/rất cao; REL ảnh hưởng A có authority và kết luận.

## 10. Bước quyết định mặc định

Nếu bất kỳ điều kiện nào sau đây còn tồn tại, A5-T010 phải kết luận **không mở Giai đoạn B**:

- Một trong A-G01–A-G06 không đạt hoặc thiếu người xác nhận.
- REL-01–REL-04 hoặc REL-07 chưa có kết luận phù hợp với phạm vi.
- Còn đường tự công khai nội dung, tự ghép tài khoản theo email, bỏ qua limiter, ghi payload thô, cấp quyền cao nhất trái chính sách, mất audit hoặc health giả.
- Export/delete hoặc diễn tập sự cố có failure chưa được đối soát/re-test.
- Ma trận 145 task có task thiếu owner, bị tính trùng hoặc lát A làm parent bị đóng sai.
