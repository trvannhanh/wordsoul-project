# Phân loại mức quan trọng và tác động tích hợp M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T003 |
| Classification ID / phiên bản | M12-CRIT-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-008, D-010, D-018–D-020; M12-CAP-REG-1.0; CT-05 |
| Phạm vi | 15 capability đã đăng ký và các lát use case có mức tác động khác nhau |
| Giới hạn | Đây là phân loại thiết kế, không thay cho SLO, health runtime, playbook hoặc bằng chứng diễn tập |

## 1. Hai trục không được trộn lẫn

- **Criticality C0–C3** là yêu cầu thiết kế ổn định của một lát use case: nó quyết định mặc định fail-closed, mức cô lập, dữ liệu cần bảo toàn và điều kiện được phép suy giảm.
- **Severity SEV-1–SEV-4** là tác động của một sự cố đang xảy ra: nó phụ thuộc phạm vi thật, thời lượng, dữ liệu và số người bị ảnh hưởng. Một capability C2 vẫn có thể tạo SEV-1 nếu làm lộ dữ liệu trên diện rộng.
- Phân loại theo **lát use case**, không gán một nhãn duy nhất cho toàn bộ provider. Khi chưa nhận diện được lát, dùng mức cao nhất đã đăng ký cho capability đó.
- SLO, thời gian phản hồi/khôi phục và error budget được chốt tại M12-T045 cùng M11-T036; tài liệu này không tự đặt con số vận hành thiếu bằng chứng.

## 2. Mức criticality

| Mức | Điều kiện vào mức | Mặc định khi phụ thuộc không chắc chắn | Yêu cầu tối thiểu |
|---|---|---|---|
| C0 — Safety/truth | Sai có thể vượt quyền, lộ bí mật/dữ liệu nhạy cảm, làm sai durable truth, tạo cấp phát/chi phí trái phép hoặc mất khả năng đối soát | Fail-closed hoặc dừng lát bị ảnh hưởng; không trả thành công giả | Durable operation ID; audit; idempotency/reconcile; giới hạn bảo thủ; health thật; không dùng cache/provider response làm truth |
| C1 — Core journey | Gián đoạn làm hỏng hành trình lõi nhưng không được thay đổi quyền hoặc truth; có thể hoãn/retry/reload | Giữ trạng thái pending/unknown hoặc dùng đường lõi độc lập đã duyệt | Deadline; kết quả chuẩn; retry có kiểm soát; durable intent/state; degraded state rõ |
| C2 — Supporting | Chức năng phụ trợ có thể thiếu mà hành trình lõi và dữ liệu vẫn đúng | Bỏ qua có thông báo hoặc dùng nội dung/dữ liệu đã duyệt | Bulkhead; quota; cache không là truth; không auto-publish; telemetry |
| C3 — Disabled/experimental | Bị policy tắt hoặc chưa được phép phát hành/thu dữ liệu | Không traffic, không thu dữ liệu để dùng sau, không fallback sang provider khác | Kill switch/activation fail-closed; không endpoint/UI/job hoạt động; muốn mở phải có quyết định và REL phù hợp |

Quy tắc nâng mức bắt buộc:

1. Bất kỳ lát nào quyết định authorization/consent, xử lý credential/secret hoặc dữ liệu nhạy cảm thô, thực hiện durable mutation, cấp phát, thanh toán/chi phí hay audit đều tối thiểu C0. Việc vận chuyển dữ liệu cá nhân tối thiểu vẫn phải đánh giá privacy nhưng không tự biến mọi delivery channel thành durable truth.
2. Delivery channel không trở thành source of truth. Email/push/realtime chỉ C0 khi bản thân operation mang quyền hoặc bí mật; trạng thái nghiệp vụ vẫn phải ở durable store.
3. Capability dùng chung nhiều module lấy mức cao nhất của lát đang chạy; không dùng mức thấp của cache/notification để nới kiểm soát cho identity hoặc mutation.
4. Trạng thái `activation-unknown`/`active-unverified` không hạ criticality và không đủ điều kiện phát hành.

## 3. Ma trận capability và lát use case

| Capability | Lát có mức cao nhất | Mức | Tác động chính nếu sai/gián đoạn | Baseline an toàn | Task hoàn thiện |
|---|---|---|---|---|---|
| CAP-001 Durable relational store | Authorization, account/progress/battle/config mutation, audit | C0 | Mất/sai durable truth, vượt quyền, không đối soát được | Core mutation fail-closed; transaction/idempotency/reconcile; không lấy cache làm truth | M11-T036–T048; M12-T036–T038, T045 |
| CAP-002 Distributed cache | Cache đọc nội dung/metadata đã duyệt | C2 | Cũ/chậm; nếu dùng sai có thể trả dữ liệu hoặc quyền sai | Miss/bypass về durable truth; key có namespace/TTL; không cache secret/authorization truth | M12-T031–T032, T035 |
| CAP-003 Distributed rate limiting | Identity, mutation, cost/abuse-sensitive route | C0 | Brute force, lạm dụng, chi phí hoặc mutation không kiểm soát | Không allow-all khi state limiter không chắc chắn; local/conservative mode theo lát | M12-T034–T035, T047-A |
| CAP-004 External identity | Đăng nhập/liên kết danh tính ngoài | C0 | Account takeover, liên kết nhầm, login sai | Login/link mới fail-closed; direct login/session hợp lệ độc lập; không tự link bằng email | M12-T006–T010 |
| CAP-005 Generative vocabulary metadata | AI sinh học liệu | C3 | Nội dung sai, chi phí, dữ liệu rời hệ thống | Tắt theo D-010; dùng content đã duyệt/manual flow | D-010; M12-T011–T014 hoãn |
| CAP-006 AI result cache | Cache kết quả AI | C3 | Nội dung cũ/sai mở ngầm AI | Không đọc/ghi để mở AI; xử lý retention dữ liệu cũ | M12-T014, T032; D-010 |
| CAP-007 External image discovery | Tìm ảnh gợi ý chưa xuất bản | C2 | Thiếu ảnh, bản quyền/nội dung không phù hợp, chi phí | Không auto-publish; upload/chọn asset đã duyệt; query tối thiểu | M12-T015, T042-A |
| CAP-008 Managed media upload/distribution | Private asset ownership/access, asset commit | C0 | Lộ tài sản, ownership/URL giả, orphan asset | Authorization và ownership fail-closed; chỉ công bố reference sau commit; reconcile orphan | M12-T021–T025, T042-A |
| CAP-008 Managed media upload/distribution | Public content delivery đã duyệt | C2 | Ảnh/audio thiếu hoặc chậm | Placeholder/asset đã duyệt; durable metadata không đổi | M12-T021–T025 |
| CAP-009 Speech synthesis + audio blob | Tạo audio từ học liệu đã duyệt | C2 | Thiếu audio, chi phí/độ trễ, asset orphan | Học bằng text; null không là success; output qua asset lifecycle | M12-T021–T025, T036–T038 |
| CAP-010 User pronunciation assessment | Thu/chấm audio người dùng | C3 | Privacy/retention, kết quả chấm sai | Tắt theo D-010; không traffic hoặc thu audio để dùng sau | M12-T018–T020 hoãn; T042-A–T043 |
| CAP-011 Email dispatch | Ràng buộc đúng người nhận và dựng verification/reset secret | C0 | Secret gửi sai người có thể chiếm tài khoản; delivery lỗi làm người dùng không hoàn thành | Recipient binding fail-closed; durable intent + expiry; `accepted` không là `delivered`; cùng message ID khi retry | M12-T026–T030, T036–T037 |
| CAP-011 Email dispatch | Reminder/engagement | C2 | Thiếu/lặp nhắc, spam hoặc chi phí | Preference/consent; expiry; bỏ qua an toàn, không đổi state lõi | M12-T026–T030, T042-A |
| CAP-012 Push dispatch | Notification tới device endpoint | C2 | Thiếu/lặp push, lộ payload trên màn hình, endpoint cũ | Inbox/state bền vững; payload tối thiểu; revoke token lỗi; push không là commit | M12-T027, T029–T030, T042-A |
| CAP-013 Realtime delivery | Live battle state/event | C1 | Mất thứ tự, mất kết nối, trải nghiệm trận sai | Reload durable state; sequence/version; delivery không là state commit | M12-T028–T030; M08 tasks |
| CAP-013 Realtime delivery | Notification realtime | C2 | Thông báo chậm/mất/lặp | Reload inbox/state; deduplicate event | M12-T028–T030 |
| CAP-014 In-process matchmaking coordination | Ghép trận và ownership phiên | C1 | Ghép trùng/sai, mất queue khi restart | Không start khi ownership không chắc; durable session/reconcile; topology rõ | M12-T031–T033; M08 tasks |
| CAP-015 Service configuration registry | Security/policy/activation/limit configuration | C0 | Mở capability, allow-all hoặc dùng secret/default sai | Typed/versioned/effective config; invalid/missing fail theo criticality; audit thay đổi | M11-T012–T017; M12-T040–T041 |

## 4. Mức tác động sự cố

| Mức | Tiêu chí đủ để phân mức | Ví dụ tích hợp | Hành động thiết kế tương ứng |
|---|---|---|---|
| SEV-1 | Đang hoặc có khả năng cao vượt quyền/lộ secret hay dữ liệu; durable truth sai/mất diện rộng; core mutation/cấp phát trái phép; không còn khả năng đối soát | External identity link nhầm; private asset lộ; SQL mutation sai; limiter cho phép abuse diện rộng | Cô lập/kill switch lát liên quan, bảo toàn bằng chứng, dừng mutation không chắc chắn, bắt đầu reconcile và quy trình ứng phó |
| SEV-2 | Hành trình lõi gián đoạn đáng kể, dữ liệu có thể pending/unknown nhưng còn bảo toàn và đối soát; blast radius có ý nghĩa | Login provider lỗi diện rộng; battle realtime/matchmaking không dùng được; email verification đình trệ | Suy giảm đã duyệt, ngăn lan rộng, giữ intent/state, thông báo và khôi phục theo playbook |
| SEV-3 | Chức năng phụ trợ suy giảm, phạm vi hạn chế; core journey và truth đúng | Image discovery/TTS/push/reminder lỗi; cache miss tăng tải nhưng hệ thống còn kiểm soát | Bypass/fallback/queue; theo dõi quota và dependency; sửa trong vận hành thường lệ |
| SEV-4 | Lỗi nhỏ/cosmetic hoặc tín hiệu sớm chưa ảnh hưởng đáng kể | Một provider probe chập chờn, một notification hết hạn bị bỏ | Ghi nhận, quan sát xu hướng, xử lý theo backlog nếu không tăng mức |

Luôn nâng lên SEV-1 nếu phát hiện secret/credential có thể đã lộ, authorization fail-open, dữ liệu riêng tư gửi sai đích hoặc durable mutation không còn đối soát được. Chỉ hạ mức khi blast radius đã biết, đường gây hại bị khóa, dữ liệu được bảo toàn và tiêu chí playbook được đáp ứng; không hạ chỉ vì provider đã hồi phục.

## 5. Ma trận loại tác động

| Loại tác động | Dấu hiệu cần đánh giá | Capability nhạy cảm | Hành vi không được phép |
|---|---|---|---|
| Quyền/bảo mật | Login/link sai, bypass limiter, config/secret sai, private asset truy cập chéo | CAP-003, 004, 008, 015 | Fail-open, log token/secret, dùng email đơn thuần để link account |
| Toàn vẹn/durable truth | Commit một phần, kết quả `unknown` bị coi success, delivery/cache thành truth | CAP-001, 008, 013, 014 | Cấp bù mù, tạo URL/ownership giả, ghi nhận event delivery là domain commit |
| Privacy/consent | Dữ liệu rời hệ thống vượt purpose, raw payload được giữ, notification lộ nội dung | CAP-004–012 | Thu để dùng sau, gửi payload thô/PII không cần thiết, kéo dài retention ngầm |
| Availability/journey | Login, verification, live battle hoặc matchmaking dừng | CAP-001, 004, 011, 013, 014 | Trả success giả, mất pending intent, retry không giới hạn |
| Chi phí/lạm dụng | Request provider tăng đột biến, quota hết, retry storm | CAP-003, 005, 007–012 | Bỏ qua CT-05, retry không budget, fallback sang provider có phí không được duyệt |
| Recoverability/đối soát | Không biết operation đã commit, queue mất sau restart, asset orphan | CAP-001, 008, 011, 014 | Dùng operation ID mới để retry, xóa dấu vết trước reconcile |

## 6. Quy tắc truyền mức vào hợp đồng và vận hành

1. Caller gắn capability/use-case ID; adapter không tự đoán mức từ provider hay endpoint.
2. M12-T004/T005 phải đưa `criticality`, operation ID, trạng thái chuẩn và lỗi có khả năng retry/reconcile vào contract mà không lộ lỗi provider thô.
3. M12-T034/T035 lấy mức này làm đầu vào để chốt limiter và fail-open/fail-closed; C0 không được hạ vì Redis/provider lỗi.
4. M12-T036–T038 định nghĩa deadline, retry, circuit và bulkhead theo lát; timeout không biến thành failure chắc chắn nếu operation có thể đã commit.
5. M12-T045 và M11-T036 gắn SLI/SLO/health thật cho từng lát C0/C1 trước; trạng thái degraded phải phản ánh dependency và data path thật.
6. A-G06 dùng severity thực tế, không dùng criticality thay cho đánh giá blast radius. Playbook phải chỉ rõ kill switch, bảo toàn dữ liệu và điều kiện đóng/hạ mức.

## 7. Finding còn mở

| Finding ID | Khoảng trống | Baseline hiện tại | Task tiếp nhận |
|---|---|---|---|
| M12-CRIT-F01 | Chưa có contract mang use-case/criticality và kết quả chuẩn | Caller mới không được mở rộng qua raw provider interface | M12-T004–T005 |
| M12-CRIT-F02 | Limiter hiện chưa chứng minh coverage và có đường fail-open toàn cục | C0 không được allow-all; giữ activation unverified | M12-T034–T035, T047-A |
| M12-CRIT-F03 | Chưa có deadline/retry/circuit/bulkhead theo lát | Không retry mù; không coi timeout là failure chắc chắn | M12-T036–T038 |
| M12-CRIT-F04 | Chưa có SLO, health runtime, ngưỡng cảnh báo và playbook đã kiểm chứng | Không kết luận A-G06/REL-03 đạt | M11-T036–T048; M12-T045–T047 |
| M12-CRIT-F05 | Activation và secret/config source theo environment chưa chốt | Unknown/unverified không đủ phát hành | M12-T040–T041; REL-03 |

## 8. Tự kiểm M12-T003, A-G04, A-G06 và REL-03

- Đủ bốn mức C0–C3, quy tắc nâng mức và tách rõ criticality khỏi severity SEV-1–SEV-4.
- Cả 15 capability trong M12-CAP-REG-1.0 đều được phân loại; capability đa mục đích được tách thành lát thay vì áp một nhãn gây fail-open sai.
- Sáu loại tác động bao phủ bảo mật, toàn vẹn, privacy, availability, chi phí và recoverability; mỗi loại có hành vi cấm.
- Baseline A-G04 đã chốt: C0 fail-closed/dừng lát khi không chắc chắn; optional capability suy giảm mà không đổi durable truth; CT-05 không bị bỏ qua.
- Baseline A-G06 đã chốt tiêu chí severity và điều kiện nâng/hạ mức, nhưng gate vẫn mở đến khi có SLO, health, playbook và diễn tập thật.
- REL-03 vẫn mở: classification là đầu vào thiết kế, chưa phải runtime evidence về provider, limiter, health hoặc recovery.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Phân loại 15 capability theo lát use case, bốn criticality, bốn severity và sáu loại tác động | WSA-7K2 |
