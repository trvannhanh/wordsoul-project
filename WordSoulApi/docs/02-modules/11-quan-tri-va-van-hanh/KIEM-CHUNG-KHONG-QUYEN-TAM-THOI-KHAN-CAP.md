# Kiểm chứng không có quyền tạm thời hoặc khẩn cấp M11 — lát A

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T006-A |
| Evidence ID / phiên bản | M11-NO-EMERGENCY-A-1.0 |
| Trạng thái | Hoàn tất rà soát tĩnh ngày 2026-08-20; không phải runtime assurance |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-034–D-037; M11-D004–D006; M11-ROLE-1.0; M11-PERM-1.0; M11-GRANT-1.0; CT-05 |
| Phạm vi | 679 file tracked WordSoulApi, gồm 509 C# và 2 config JSON/YAML-like; source auth/admin/role/limiter, migrations và test source |
| Ngoài phạm vi | Runtime/deployed config, database rows, cloud IAM, CI secret/env, binary/generated output và external infrastructure |

## 1. Câu hỏi kiểm chứng và mức kết luận

| Câu hỏi | Kết luận tĩnh | Giới hạn |
|---|---|---|
| Có model/field/state cho temporary role/permission/privilege không? | Không quan sát trong tracked C#/JSON | Absence-of-string không chứng minh runtime/database/cloud không có |
| Có break-glass/emergency/sudo/impersonation/backdoor auth path không? | Không quan sát explicit implementation hoặc keyword trong tracked C#/JSON | Chưa có coverage test/compiled route/config/deployment evidence |
| Có header/debug/loopback tạo admin authorization không? | Không quan sát header/debug branch gán role/claim hoặc bypass authorization | Có header + loopback bypass **rate limiter**, là trust exception riêng và trái target CT-05 |
| Có đường nâng quyền rộng ngoài M11-GRANT-1.0 không? | **Có**: Admin/SuperAdmin có thể gọi route đổi `User.Role` sang bất kỳ enum hợp lệ | Permanent direct elevation, không phải temporary/emergency nhưng là blocker A-G02 |
| Có test chứng minh self-grant, no-emergency, stale-session và last-owner bị chặn không? | Không tìm thấy test chuyên biệt | Không được kết luận target permission/grant lifecycle đã thực thi |

Kết luận task: **đạt mục tiêu kiểm chứng tĩnh lát A** vì không có explicit temporary/emergency/impersonation model được quan sát và mọi trust/elevation gap có owner. Không kết luận A-G02/A-G06/REL-02 đạt; source hiện tại vẫn không release-ready do direct broad role change, stale role claim, limiter bypass và thiếu negative/runtime coverage.

## 2. Phương pháp có thể lặp lại

| Check ID | Phạm vi/cách kiểm | Kỳ vọng | Kết quả 2026-08-20 |
|---|---|---|---|
| NE-A01 | `git ls-files` dưới WordSoulApi; đếm tracked/C#/config | Scope hữu hạn, không suy từ bin/obj/untracked | 679 tracked; 509 C#; 2 config |
| NE-A02 | Case-insensitive tracked grep: break-glass, emergency, temporary/temp role/permission/privilege/access, impersonate/act-as/sudo/backdoor/auth bypass | Không explicit privilege path | Không match trong tracked C#/JSON |
| NE-A03 | Grep `UserRole`, `Role =`, `AssignRole`, `ClaimTypes.Role`, role authorize | Mọi elevation entry point được nhận diện | Enum User/Admin/SuperAdmin, JWT role claim và direct assign route/service được quan sát |
| NE-A04 | Grep `X-Internal`, `GetNoLimiter`, `DisableRateLimiting`, `AllowAnonymous`, `IsDevelopment` | Phân loại trust exception, không nhầm thành admin auth | Public/diagnostic/rate exceptions được liệt kê; không branch gán admin, có limiter bypass finding |
| NE-A05 | Rà auth middleware/order và role mutation body | Không hidden debug auth; role effect/state rõ | JWT auth/authorization chuẩn; role mutation ghi thẳng enum + activity log |
| NE-A06 | Grep test source cho assign role/SuperAdmin/no-emergency/internal-worker | Negative cases tồn tại | Không thấy role/grant/no-emergency tests; limiter test không phủ worker bypass production path |
| NE-A07 | Đối chiếu target role/permission/grant contracts | Không temp/emergency/shared/impersonation; fixed R13 continuity only | Contract nhất quán; runtime chưa triển khai |

Keyword negative scan phải chạy trên tracked text source/config, không trên docs để tránh tự match policy words; generated binary/bin/obj/log/untracked file bị loại khỏi kết luận. Kết quả grep trống được ghi là negative evidence tại thời điểm commit, không là proof vĩnh viễn.

## 3. Inventory trust/elevation entry point

| Entry | Loại | Quan sát | Verdict/action |
|---|---|---|---|
| `PUT api/users/{userId}/role` | Privilege mutation | Admin/SuperAdmin gọi service với role string | Blocker: phải thay bằng M11-GRANT-1.0/M11-PERM-1.0; M01-T029–T030 |
| `AssignRoleToUserAsync` | Privilege effect | Parse bất kỳ `UserRole`, ghi đè field, không actor/re-auth/reason/scope/conflict/last-owner | Blocker A-G02; M01-T029–T032/M11-T049 |
| JWT `ClaimTypes.Role` | Authorization input | Role snapshot trong access token | Stale privilege risk; current authorization version/session enforcement required |
| Controller role attributes | Authorization boundary | Admin/SuperAdmin/User strings trực tiếp | Legacy only; không temp path nhưng broad/unspecialized authority |
| Internal worker header + loopback | Rate-control exception | 7 limiter policy trả no-limiter | Không gán role; vẫn bị cấm bởi CT-05/M12-RATE-1.0, M12-T047-A |
| Health/SignalR `DisableRateLimiting` | Transport/rate exception | Explicit endpoints | Không admin auth; cần coverage/ingress/capacity policy, không dùng cho privileged mutation |
| `AllowAnonymous` auth/public settings | Public entry | Register/login/refresh/OAuth và public settings | Không admin authority theo source; phải giữ field/action allowlist và route coverage |
| Development detailed errors/SignalR | Diagnostic behavior | `IsDevelopment()` đổi error detail | Không role/auth bypass quan sát; privacy/config evidence thuộc M11-T033/M12-T040–T043 |
| Migration `SuperAdmin` | Schema history | Adds enum support/config schema, không temporary grant | Không emergency path; broad legacy role migration vẫn bắt buộc |

## 4. Negative requirement matrix

| Requirement | Target rule | Static result | Residual evidence required |
|---|---|---|---|
| No temporary grant | Assignment chỉ Active/Suspended/Revoked, reviewAt không là temp expiry | Target contract đạt; source không có grant model | Schema/API negative tests; DB constraint; runtime route coverage |
| No emergency/break-glass role | Fixed scoped R13 continuity, không mint role | Không explicit path quan sát | Build scan + permission tests + production config/IAM review |
| No self-grant | Actor != target; R12/R13 boundary | Source direct route chưa guard | M01-T029/M11-T049 deny test |
| No impersonation/shared admin | Trusted actor identity only | Không explicit feature quan sát | Session/workload/SSO/deployment evidence; shared-account control |
| No header authority | Header không tạo role/permission/internal trust | Không auth header observed; limiter header exception tồn tại | Remove no-limiter; workload identity/quota tests M12-T047-A |
| No debug authority | Development flag không mở admin action | Không observed | Production config/canary/route discovery tests |
| No legacy fallback | Target grant required, Admin/SuperAdmin claim denied alone | Chưa triển khai; role attributes là source truth hiện tại | Legacy migration/deny-most-restrictive runtime suite |
| No stale privilege | Current authorization version checks every admin action | Chưa có authorization version | M01 session/role change implementation + race/cache tests |
| No orphan privileged entry point | Every API/job/CLI maps Action/Permission ID | Không central registry/coverage | M11-T049 source/host coverage gate |

## 5. Finding hiện trạng

| Finding ID | Quan sát | Tác động | Task tiếp nhận |
|---|---|---|---|
| M11-NE-I01 | Direct role route/service cho Admin/SuperAdmin parse mọi enum role | Permanent self/peer elevation, không re-auth/scope/conflict/last-owner | M01-T029–T030; M11-T049 |
| M11-NE-I02 | Role là field đơn và JWT role claim, không authorization version | Thu hồi/đổi role có thể stale đến khi token hết hạn | M01-T016, T029; M11-T049 |
| M11-NE-I03 | Không có grant store/state/reviewAt/schema | Target no-temp lifecycle chưa thể cưỡng chế bằng constraint | M11-T049; M01-T029–T032 |
| M11-NE-I04 | Internal worker header + loopback bỏ toàn bộ limiter ở 7 policy | Không auth elevation nhưng là trust/control bypass trái CT-05 | M12-T047-A; REL-03 |
| M11-NE-I05 | Không thấy role/no-emergency/self-grant/last-owner/stale-session tests | Negative assurance không tự động phát hiện regression | M11-T049; A-G02/A-G06 |
| M11-NE-I06 | Không có deployed config/cloud IAM/database inspection | Không thể loại trừ out-of-repo emergency/shared access | REL-02/REL-03; release evidence |

I01–I06 là release gap. Negative keyword result không hạ severity của observed direct elevation path.

## 6. Regression gate đề xuất

| Gate ID | Kiểm tra bắt buộc | Fail khi |
|---|---|---|
| NEG-01 | Tracked source/config keyword scan allowlist | Xuất hiện temp/emergency/break-glass/impersonation/auth-bypass token không có reviewed exception |
| NEG-02 | Privileged entry-point coverage | Route/job/CLI không Action/Permission ID hoặc legacy role-only authority |
| NEG-03 | Grant schema/DTO contract | Có expires-as-privilege/temp flag/wildcard/null scope/role nesting/impersonated actor |
| NEG-04 | Self/peer elevation tests | R12/R13 boundary, self/conflict/last-owner có bất kỳ allow sai |
| NEG-05 | Session/cache race tests | Grant vào session cũ hoặc revoke vẫn cho action stale |
| NEG-06 | Workload/trust tests | Header/loopback/debug flag bypass auth/permission/limiter quota |
| NEG-07 | Runtime/config/IAM inventory | Shared admin/emergency credential/out-of-band role không owner/policy/audit |
| NEG-08 | Audit failure tests | Privilege increase success khi mandatory durable audit không commit |

Exception allowlist chỉ dành cho từ khóa vô hại hoặc public/health transport behavior đã đăng ký, gồm file/line owner/lý do/expiry/test. Không allowlist behavior tạo privilege hoặc `GetNoLimiter` cho workload.

## 7. Ma trận nghiệm thu

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| NE06-01 | Keyword scan source/config hiện tại | No explicit temp/emergency/impersonation/auth-bypass implementation match |
| NE06-02 | Thêm `TemporaryRole` field | NEG-01/03 fail build/review |
| NE06-03 | Thêm `break-glass` endpoint role-only | NEG-01/02/04 fail; no deployment |
| NE06-04 | Admin tự gọi direct role route lên SuperAdmin | Target deny; current implementation flagged blocker |
| NE06-05 | R12 grant self/PlatformOwner | Deny + security audit/alert |
| NE06-06 | Last R13 removal | Deny + continuity incident, no emergency role mint |
| NE06-07 | Header `X-Admin`/debug flag | Never authority; deny absent target grant |
| NE06-08 | Internal worker header from loopback | No limiter bypass in target; workload identity + quota required |
| NE06-09 | Health/SignalR rate exemption | Does not expose privileged mutation/data; separate capacity policy evidence |
| NE06-10 | Role revoked, JWT/cache stale | Current authorization version denies immediately |
| NE06-11 | Grant review overdue | Suspend R3/R4; no auto extension/temp semantics |
| NE06-12 | Deployment contains shared/emergency admin identity | Release gate fails until removed/owned fixed-role migration |
| NE06-13 | Audit unavailable during privilege increase | Deny; no effect |
| NE06-14 | Source scan clean but no runtime tests | Task static evidence remains limited; A-G02/A-G06 not pass |
| NE06-15 | Keyword false positive in content/test data | Reviewed scoped allowlist; behavior/AST/route test still required |
| NE06-16 | New privileged CLI/job added | Must map Action/Permission/workload identity; orphan gate fail |

## 8. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-NE-F01 | Target role/grant/session source implementation | Legacy direct elevation frozen; role-only authority not release-ready | M01-T029–T032; M11-T049 |
| M11-NE-F02 | Production runtime/config/DB/cloud IAM/shared-account inventory | Không kết luận từ repo absence; fixed scoped identity only | REL-02/REL-03; release evidence |
| M11-NE-F03 | AST/route/schema/session/cache/audit negative regression suite | Eight gates mandatory; keyword scan only one layer | M11-T049; A-G02/A-G06 |
| M11-NE-F04 | Workload identity/quota thay internal-header no-limiter | Header/loopback không được trusted bypass | M12-T047-A; CT-05; REL-03 |

## 9. Tự kiểm M11-T006-A, A-G02, A-G06 và REL-02

- Scope kiểm chứng định lượng 679 tracked/509 C#/2 config; negative scan và trust/elevation inventory không lưu, sao chép hoặc đưa secret value vào evidence.
- Không explicit temporary/emergency/break-glass/impersonation/auth-bypass model được quan sát trong tracked C#/JSON; đây là negative evidence có giới hạn.
- Direct permanent elevation, stale role claim, no grant model/tests, limiter bypass và out-of-repo gap được giữ là 6 finding, không bị “scan sạch” che khuất.
- 8 regression gate + 16 case bao phủ source/schema/route/self/last-owner/session/cache/workload/runtime/audit.
- 4 finding mở có owner; A-G02/A-G06/REL-02/REL-03 chưa đạt đến khi source/runtime/deployment evidence hoàn tất.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Rà 679 tracked file, phân loại trust/elevation path, chốt 8 gate và 16 case | WSA-7K2 |
