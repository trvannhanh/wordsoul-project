# Sổ đăng ký năng lực tích hợp M12

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M12-T002 |
| Registry ID / phiên bản | M12-CAP-REG-1.0 |
| Trạng thái | Baseline kiểm kê tĩnh có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-019; D-010; D-018; M12-D001, D003, D005, D016, D021–D023 |
| Phạm vi | OAuth, AI, ảnh/blob, speech, email, push, realtime, SQL, Redis cache/coordination/rate limit |
| Giới hạn | “Đăng ký trong DI” không chứng minh enabled, configured, healthy hoặc được phép phát hành |

## Trạng thái registry

| Trạng thái | Ý nghĩa |
|---|---|
| `implemented` | Có code/registration quan sát được; chưa kết luận runtime |
| `disabled-by-policy` | Có code nhưng không được phép có endpoint/UI/job/provider traffic trong phạm vi hiện hành |
| `activation-unknown` | Không có bằng chứng cấu hình + traffic + owner approval cho môi trường phát hành |
| `active-unverified` | Có đường gọi được quan sát nhưng chưa có health/contract/runtime evidence |
| `active-verified` | Chỉ dùng khi có evidence runtime đúng contract, health và policy; hiện chưa capability nào đạt trạng thái này |
| `retired` | Không còn được gọi; secret/route/job đã thu hồi và data lifecycle xử lý |

Mọi capability mặc định `activation-unknown` trừ khi decision/policy nói rõ hơn. Không suy secret/config file tồn tại thành enabled hoặc healthy.

## Registry năng lực

| ID | Năng lực chuẩn | Owner / consumer | Implementation/provider quan sát | Dữ liệu tối thiểu dự kiến | Source of truth | Trạng thái A/B | Suy giảm an toàn hiện hành | Task tiếp nhận |
|---|---|---|---|---|---|---|---|---|
| CAP-001 | Durable relational store | M12 platform / mọi module | EF Core + SQL Server | Domain records theo owner | SQL theo module owner | `active-unverified` | Core mutation fail-closed; không dùng cache làm truth | M12-T003–T005, M11-T036 |
| CAP-002 | Distributed cache | M12 / M02 và consumer đăng ký | `IDistributedCache` + Redis | Key/version/value tối thiểu, không secret/PII ngoài policy | Durable module store | `active-unverified` registration; runtime unknown | Cache miss/failure không đổi truth; bypass cache nếu business cho phép | M12-T031–T032, M12-T035 |
| CAP-003 | Distributed rate limiting | M12 / auth, AI, audio, cost-sensitive routes | `RedisRateLimiter`; hiện không thấy consumer trực tiếp, endpoint policies chủ yếu in-memory | Partition protected key, policy/version, counters/TTL | Policy registry + Redis state tạm | `implemented`, activation/coverage unknown | Không allow-all cho identity/cost/mutation; local/conservative mode theo capability | M12-T034–T035 |
| CAP-004 | External identity | M12/M01 | `GoogleOAuthService`, Google OAuth/OIDC endpoints | Code/token exchange nội bộ, provider subject, verified email claim tối thiểu | M01 account/link registry | `active-unverified`; config flag tồn tại nhưng enforcement chưa chứng minh | Không login mới fail-open; direct login/session hợp lệ vẫn độc lập | M12-T006–T010 |
| CAP-005 | Generative vocabulary metadata | M12/M02 | `GeminiAiService` | Học liệu tối thiểu; không identity/progress | M02 approved content | `disabled-by-policy` theo D-010 | Không provider traffic; dùng content đã duyệt/manual flow | D-010; M12-T011–T014 hoãn |
| CAP-006 | AI result cache | M12/M02 | `VocabularyAiCacheService` + Redis | Cache key theo source/model/prompt/version; generated result | M02 approved content, không cache | `disabled-by-policy` cùng CAP-005 | Không đọc/ghi cache để mở AI; dữ liệu cũ xử lý theo retention task | M12-T014, M12-T032; D-010 |
| CAP-007 | External image discovery | M12/M02 | `UnsplashService` | Query học liệu tối thiểu, asset URL/source metadata | M02 asset selection/approval | `activation-unknown` | Không ảnh ngoài thì upload/chọn asset đã duyệt; không auto-publish | M12-T003–T005, M12-T015, M12-T042-A |
| CAP-008 | Managed media upload/distribution | M12 / M01, M02, M06 | `UploadAssetsService`, Cloudinary | File stream + asset type/owner/purpose; metadata quyền | Asset registry/owner module | `active-unverified`; DI registration thấy rõ | Upload lỗi không tạo URL/ownership giả; giữ durable record nhất quán | M12-T021–T025, M12-T042-A |
| CAP-009 | Speech synthesis + audio blob | M12/M02 | `AzureSpeechService`, Azure Speech + Blob Storage | Text học liệu, locale/voice/version, output asset ref | M02 approved content + asset registry | `activation-unknown` | Không audio thì học bằng text; không coi null là success | M12-T003–T005, M12-T021–T025 |
| CAP-010 | User pronunciation assessment | M12/M05 | `AzurePronunciationService` | Audio người dùng + reference text tối thiểu | M05 attempt/result; raw provider không là truth | `disabled-by-policy` theo D-010 | Không provider traffic/thu audio cho future use; luyện không chấm nếu flow được phép | D-010; M12-T018–T020 hoãn |
| CAP-011 | Email dispatch | M12 / M01, M10, M11 jobs | `SendGridEmailService` | Recipient ref, template/version, variables allowlist, expiry | Source module intent + delivery registry | `active-unverified`; reminder/admin callers tồn tại | Pending/retry cùng message; accepted không là delivered | M12-T026–T030, M12-T042-A |
| CAP-012 | Push dispatch | M12/M10, M01 device registry | `FcmService`, Firebase Admin conditional initialization | Device endpoint ref, template/payload allowlist, expiry/action ref | M10 notification + M01 device/consent | `activation-unknown`; init phụ thuộc config/file | Không push thì inbox/state bền vững; invalid endpoint phải revoke | M12-T027, M12-T029–T030, M12-T042-A |
| CAP-013 | Realtime delivery | M12 / M08, M10 | ASP.NET Core SignalR, `SignalRNotificationService`, battle hubs | Authenticated subject/group, event ID/version/sequence | Durable M08/M10 state | `active-unverified` | Reconnect/reload durable state; không coi delivery là state commit | M12-T028–T030 |
| CAP-014 | In-process matchmaking coordination | M08/M12 platform | `MatchmakingQueueService` singleton + SignalR notifier | User/session protected refs, queue metadata | Durable battle/session state cần xác định | `active-unverified`, single-process boundary | Không bắt đầu match khi ownership/sync không chắc; restart không được bịa queue truth | M12-T031–T033; M08 tasks |
| CAP-015 | Service configuration registry | M11 / mọi capability | `SystemConfigurationService` + SQL configuration rows | Key/value typed, version, effective time, owner/audit | M11 durable config store | `active-unverified` | Invalid/missing config theo criticality; không dùng default bí mật/allow-all | M11-T012–T017; M12-T003 |

## Ma trận môi trường và activation

| Capability group | Local/dev | Test/CI | Production/release | Kết luận hiện tại |
|---|---|---|---|---|
| SQL/Redis | Registration/config key quan sát được; runtime không kiểm chứng trong task docs | Test artifacts/build output tồn tại nhưng không là health evidence | Chưa có baseline nguồn thật được chốt | `unknown` ngoài code registration |
| Google/SendGrid/Unsplash/Cloudinary/Azure/Firebase | Code đọc config/credential; không đọc hoặc ghi secret vào tài liệu | Không xác nhận sandbox/fake contract | Chưa có provider allowlist + health + owner approval tập trung | `activation-unknown` hoặc `active-unverified` như registry |
| Gemini/user pronunciation | Code tồn tại | Không được bật traffic thật | Bị D-010 tắt trong A/B | `disabled-by-policy` |
| SignalR/in-process coordination | Route/service registration quan sát được | Chưa có multi-instance/restart evidence | Runtime deployment topology chưa chốt | `active-unverified` |

Registry không chứa endpoint, account, region, key, secret, project ID hoặc credential file content. Những trường đó thuộc secret/config store có kiểm soát.

## Dòng dữ liệu tối thiểu và privacy owner

| Nhóm dữ liệu | Capability | Owner phê duyệt purpose/minimization | Trạng thái mapping |
|---|---|---|---|
| Provider subject + verified email claim | CAP-004 | M01 + M12/privacy | Chờ M12-T006, M12-T042-A; không tự liên kết email |
| Học liệu/query/generated metadata | CAP-005–CAP-007, CAP-009 | M02 + M12/privacy | AI tắt; image/TTS activation unknown; chờ M12-T042-A |
| Media/avatar/content asset | CAP-008–CAP-009 | Owner M01/M02/M06 + M12 asset/privacy | Chờ M12-T021–T025, M12-T042-A |
| User audio/reference/result | CAP-010 | M05 + M01 consent + M12/privacy | Tắt theo D-010; retention cuối chờ task/REL |
| Recipient/device/notification variables | CAP-011–CAP-012 | M01/M10 + M12/privacy | Chờ consent/device/delivery/data-flow contract |
| Realtime subject/group/event | CAP-013–CAP-014 | M08/M10 + M12 | Chờ auth/group lifecycle và topology contract |
| Cache/rate-limit keys | CAP-002–CAP-003 | Source module + M12/privacy/security | Phải protected/minimized; TTL/namespace chờ M12-T032/T034 |

## Kiểm kê secret và cấu hình — chỉ metadata

| Finding ID | Quan sát an toàn | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M12-REG-I01 | `Program.cs` đọc config cho SQL, Redis, Google, Cloudinary, Firebase, Gemini, Unsplash, Azure và SendGrid | Không có registry owner/rotation/environment source thống nhất | M12-T040–T041; REL-03 |
| M12-REG-I02 | Có đường dẫn file mang tên giống Firebase service-account trong working/build tree nhưng không nằm trong `git ls-files`; không đọc nội dung | Nguy cơ credential cục bộ bị sao chép vào build/deploy artifact dù chưa được Git theo dõi | M12-T040–T041; REL-03 |
| M12-REG-I03 | Firebase init có fallback đọc JSON file local và chỉ warning khi thiếu | Activation phụ thuộc file ngoài registry; service vẫn được đăng ký khi provider chưa sẵn sàng | M12-T003–T005, M12-T040–T041 |
| M12-REG-I04 | Redis connection tạo singleton trực tiếp khi startup | Redis lỗi có thể làm startup/health khác với cache degradation dự kiến | M12-T003, M12-T035–T038, M11-T036 |
| M12-REG-I05 | `RedisRateLimiter` được đăng ký nhưng không tìm thấy consumer trực tiếp | Registry/code comment có thể báo coverage không tồn tại | M12-T034–T035, M12-T047-A |
| M12-REG-I06 | Google enable flag tồn tại trong data migration nhưng route enforcement chưa được quan sát | Kill/config state có thể không kiểm soát capability thực tế | M12-T006–T010; M11-T012–T017 |
| M12-REG-I07 | Pronunciation entity/migration lưu `AzureRawResponse` | Raw provider payload vượt adapter và tạo rủi ro privacy/retention | M12-T018–T020, M12-T042-A–T043; D-010 |
| M12-REG-I08 | Email/push services nhận recipient/device token và content trực tiếp | PII/endpoint/provider payload không được protected-ref/template hóa | M12-T026–T030, M12-T042-A–T043 |

## Finding còn mở

| Mã | Phần chưa có bằng chứng | Baseline an toàn | Nguồn/task xử lý |
|---|---|---|---|
| M12-REG-F01 | Provider/config thực sự bật theo từng environment | Giữ `activation-unknown`; không phát hành dựa trên DI/config presence | REL-03; M12-T040–T041; M11-T036 |
| M12-REG-F02 | Criticality/tolerance/SLO/fallback cuối từng capability | Core truth/mutation fail-closed; AI/user speech tắt; optional flow degrade | M12-T003, M12-T030, M12-T035, M12-T045 |
| M12-REG-F03 | Contract/result/error/deadline/idempotency chuẩn | Không mở rộng caller mới qua interface raw hiện tại | M12-T004–T005, M12-T036–T037 |
| M12-REG-F04 | External data map, region, retention, consent, subprocessor | Không thêm data flow/provider traffic mới; dùng minimization và protected refs | M12-T042-A–T043; REL-01/REL-03/REL-04 |

## Tự kiểm M12-T002, A-G04, A-G05 và REL-03

- 15 capability bao phủ OAuth, AI/cache, image/media, speech, email, push, realtime, SQL, Redis, rate limit và configuration.
- Mỗi capability có owner/consumer, implementation/provider quan sát, dữ liệu tối thiểu, source of truth, activation, degradation và task tiếp nhận.
- Ba trạng thái implementation/config/activation được tách; không capability nào bị nâng thành `active-verified` khi chưa có runtime evidence.
- Tám finding hiện trạng và bốn finding mở có baseline an toàn; không đọc, sao chép hoặc lưu secret/credential content.
- A-G04/A-G05/REL-03 vẫn mở vì criticality, contract, data-flow, secret lifecycle, health và runtime degradation evidence chưa hoàn thành.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Kiểm kê 15 capability, activation/environment, data flow và secret/config metadata | WSA-7K2 |
