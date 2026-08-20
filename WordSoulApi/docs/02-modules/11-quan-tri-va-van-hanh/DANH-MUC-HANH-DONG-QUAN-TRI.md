# Danh mục hành động quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T002 |
| Catalog ID / phiên bản | M11-ACTION-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-032–D-033; M11-D001–D006, D013, D016–D023; M11-DICT-1.0 |
| Chủ catalog | M11 sở hữu ID/risk/control; module nguồn sở hữu resource/invariant/effect; M01 sở hữu actor/session/re-auth |
| Quy tắc | Action chưa đăng ký hoặc owner/policy/version unknown bị deny release; route/UI/role hiện tại không tự trở thành permission chuẩn |

## Chuẩn phân loại

| Thuộc tính | Giá trị chuẩn |
|---|---|
| Động từ | Xem, tạo, sửa, gửi rà soát, xác nhận, kích hoạt, ngừng, điều chỉnh, xóa/ẩn danh, xuất, chạy/chạy lại, bảo trì/dừng |
| Mức rủi ro | `R0` public/aggregate read; `R1` scoped reversible read; `R2` sensitive read/content mutation; `R3` broad/config/job mutation; `R4` identity/role/asset/delete/broadcast/maintenance/secret effect |
| Lớp dữ liệu | Public, Internal, Personal, Sensitive, Secret hoặc Provider-native theo M12-CONTRACT-1.0; Secret/Provider-native không vào catalog payload/audit |
| Kiểm soát tối thiểu | Quyền/phạm vi, xác minh lại khi nhạy cảm, lý do/vụ việc, preview khi cần, audit và hành vi fail-closed |
| Trạng thái owner | Chưa xác nhận, đã xác nhận, thay thế/không áp dụng |

Risk là **mức sàn** theo effect/data/scope; policy/module có thể nâng nhưng không hạ nếu chưa có decision version. `R3/R4` luôn cần full admin session, current-state decision, re-auth ≤5 phút, reason, immutable audit và idempotency/CAS cho mutation. `R4` thêm preview/impact hoặc case/incident, explicit scope, notification/reconcile/rollback khi applicable; thiếu audit/state/limiter truth thì fail-closed.

## Catalog hành động nền

| Action group | Module sở hữu | Tài nguyên/dữ liệu | Động từ trong phạm vi | Rủi ro tối đa | Điều kiện từ chối tối thiểu | Bằng chứng bắt buộc | Owner status |
|---|---|---|---|---|---|---|---|
| ADM-M01-ACCOUNT | M01 | Tài khoản, trạng thái, phiên | Xem, hạn chế, khóa/mở, thu hồi phiên | Rất cao | Thiếu quyền/phạm vi, xác minh lại, lý do hoặc audit | Actor/role, reason, trước/sau, revoke result | Chờ chủ M01 |
| ADM-M01-ROLE | M01/M11 | Vai trò và quyền người dùng/quản trị | Xem, cấp, thu hồi, thay đổi | Rất cao | Tự cấp quyền cao nhất, xung đột vai trò, audit lỗi | Ma trận quyền, kiểm thử từ chối, trước/sau | Chờ M01/M11 |
| ADM-M01-DATA | M01 | Hồ sơ và yêu cầu dữ liệu | Tra cứu, xuất, yêu cầu xóa, hủy yêu cầu | Rất cao | Không có vụ việc/chủ thể/phạm vi; dữ liệu ngoài quyền | Purpose/case, manifest, thời hạn, audit lượt truy cập | Chờ REL-01/07 |
| ADM-M02-CONTENT | M02 | Mục từ, nghĩa, bộ từ, phiên bản | Xem, tạo, sửa, gửi rà soát, xác nhận, công khai, thu hồi | Cao | Module nguồn không hợp lệ, version sai, thiếu quyền/bằng chứng quyền tài sản | Version, checklist, actor/decision, reference impact | Chờ chủ M02 |
| ADM-M03-SESSION | M03 | Phiên học/kiểm tra và lịch sử | Xem hỗ trợ, vô hiệu hóa dữ liệu lỗi theo quy trình | Cao | Sửa trực tiếp kết quả/lịch sử, thiếu vụ việc | Case, nguồn sự thật, trước/sau, reconciliation | Chờ chủ M03 |
| ADM-M04-REVIEW | M04 | Lịch ôn/tiến độ dẫn xuất | Xem, chạy lại/backfill có kiểm soát | Cao | Chạy lại không idempotent, thiếu checkpoint/preview | Job ID, dry-run, checkpoint, reconciliation | Chờ chủ M04 |
| ADM-M05-PRONUNCIATION | M05 | Bài luyện/âm thanh/kết quả | Xem có che, thu hồi dữ liệu theo chính sách | Cao | Lộ bản ghi âm/PII, sửa kết quả nguồn | Purpose, data scope, redaction, audit | Chờ chủ M05 |
| ADM-M06-ASSET | M06 | XP/AP/gợi ý/item/pet/ledger | Xem, yêu cầu điều chỉnh/thu hồi | Rất cao | Sửa số dư trực tiếp, thiếu vụ việc/hạn mức/xác minh lại | Ledger mutation ID, preview, reason, reconciliation | Chờ chủ M06 |
| ADM-M07-MISSION | M07 | Nhiệm vụ/thành tựu/quy tắc | Tạo, sửa version, kích hoạt/ngừng | Cao | Sửa version đang dùng, thiếu reference impact/rollback | Version, preview, actor, rollout/rollback | Chờ chủ M07 |
| ADM-M08-BATTLE | M08 | Phòng/trận/quy tắc thi đấu | Xem hỗ trợ, cấu hình theo version, dừng năng lực | Rất cao | Sửa kết quả trận, kill thiếu phạm vi/audit | Match/case ID, config version, stop/recovery result | Chờ chủ M08 |
| ADM-M09-GROUP | M09 | Nhóm/xếp hạng | Xem, hạn chế, xử lý chủ không hoạt động | Cao | Thay đổi lịch sử/xếp hạng trực tiếp, lộ thành viên | Case, source version, before/after, audit | Chờ chủ M09 |
| ADM-M10-NOTIFICATION | M10 | Mẫu, chiến dịch, thiết bị/nhận tin | Tạo/sửa, gửi thử, lên lịch, dừng/thu hồi | Rất cao | Thiếu audience preview, consent, hạn mức hoặc kill switch | Template version, audience count, approval context, delivery result | Chờ chủ M10 |
| ADM-M11-CONFIG | M11/module nguồn | Cấu hình/chính sách | Xem, tạo draft, sửa, lên lịch, kích hoạt, rollback, ngừng | Rất cao | Khóa vô chủ, validation lỗi, thiếu impact/rollback/audit | Config version, validation, preview, execution result | Chờ registry M11-T012 |
| ADM-M11-LOG | M11 | Audit/activity/log | Tìm kiếm, xem chi tiết, xuất, retention/hold | Rất cao | Thiếu vụ việc/phạm vi; payload/bí mật; audit lượt xem lỗi | Query scope, purpose, redaction, access audit | Chờ REL-02 |
| ADM-M11-JOB | M11/module nguồn | Công việc nền và lần chạy | Xem, dừng, chạy/chạy lại, bù | Rất cao | Job vô chủ, không idempotent/checkpoint/preview | Job/run ID, dry-run, checkpoint, result/reconciliation | Chờ M11-T038–T040 |
| ADM-M11-INCIDENT | M11/vận hành | Sự cố, bảo trì, kill switch | Mở/phân mức, bảo trì, dừng, khôi phục, đóng | Rất cao | Không có quyền cố định, phạm vi, playbook hoặc audit | Incident ID, timeline, action result, post-check | Chờ M11 Lát 4 |
| ADM-M12-INTEGRATION | M12 | Provider, bí mật, limiter, asset store | Xem metadata, cấu hình theo hợp đồng, rotate/revoke, dừng provider | Rất cao | Ghi/hiển thị secret, bypass limiter, fail-open | Capability/config version, secret reference, test degradation | Chờ chủ M12 |

## Catalog action nguyên tử v1.0

`Control` viết tắt: `P` permission + scope; `S` current state/session/policy; `RA` re-auth ≤5 phút; `R` reason; `C` case/change/incident; `V` version/CAS/idempotency; `PV` preview/impact; `A` immutable audit; `N` notification; `RC` reconcile/rollback. Mọi action authenticated luôn có `P,S,A`; bảng chỉ nêu tập đầy đủ để nghiệm thu.

| Action ID | Owner | Resource/action | Data class | Risk | Control bắt buộc | Hiện trạng tĩnh |
|---|---|---|---|---|---|---|
| M11-ACT-001 | M01 | Account support summary read | Personal | R2 | P,S,R,C,A | Broad Admin/SuperAdmin user read; thiếu case/redaction |
| M11-ACT-002 | M01 | Account sensitive detail/activity read | Sensitive | R3 | P,S,RA,R,C,A | Route hiện có; thiếu purpose/case/access audit |
| M11-ACT-003 | M01 | Account restriction/lock/unlock | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Boolean status route; thiếu restriction/version/expiry |
| M11-ACT-004 | M01 | Session family revoke one/all | Sensitive | R4 | P,S,RA,R,C,V,A,N,RC | Chưa có family management route |
| M11-ACT-005 | M01/M11 | Role/permission grant/revoke | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Gán role string; chưa permission/scope/separation |
| M11-ACT-006 | M01 | Data export request/read result | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Chưa có |
| M11-ACT-007 | M01 | Deletion request/cancel/status | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Admin hard-delete route không đạt contract |
| M11-ACT-008 | M02 | Vocabulary/meaning read sensitive draft | Personal | R2 | P,S,R,A | Broad Admin/User read, chưa draft/data scope |
| M11-ACT-009 | M02 | Vocabulary/meaning create/update | Internal | R2 | P,S,R,V,A,RC | Admin/SuperAdmin CRUD, chưa version/concurrency |
| M11-ACT-010 | M02 | Vocabulary/meaning publish/deprecate | Internal | R3 | P,S,RA,R,V,PV,A,N,RC | Publish/deprecate lifecycle chưa chuẩn |
| M11-ACT-011 | M02 | Vocabulary/meaning delete/merge | Personal | R4 | P,S,RA,R,C,V,PV,A,N,RC | Delete trực tiếp; thiếu reference/retention |
| M11-ACT-012 | M02 | Vocabulary set create/update/publish | Personal | R3 | P,S,RA,R,V,PV,A,RC | Admin null-owner bypass quan sát được |
| M11-ACT-013 | M02/M06 | Set reward association change | Internal | R4 | P,S,RA,R,C,V,PV,A,RC | CRUD reward-pet trực tiếp, thiếu ledger/catalog boundary |
| M11-ACT-014 | M03 | Learning session/history support read | Personal | R3 | P,S,RA,R,C,A | Dashboard read có ID, thiếu case/redaction |
| M11-ACT-015 | M03 | Invalid learning record quarantine/correct request | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Chưa có; cấm sửa trực tiếp source history |
| M11-ACT-016 | M04 | Review history/progress support read | Personal | R3 | P,S,RA,R,C,A | Admin review-history route; thiếu case/access audit |
| M11-ACT-017 | M04 | Review projection rebuild/backfill | Sensitive | R4 | P,S,RA,R,C,V,PV,A,RC | Chưa có job/checkpoint/reconcile contract |
| M11-ACT-018 | M05 | Pronunciation metadata/private asset read | Sensitive | R3 | P,S,RA,R,C,A | Không admin flow; audio capability disabled |
| M11-ACT-019 | M05 | Pronunciation asset retention/delete request | Sensitive | R4 | P,S,RA,R,C,V,PV,A,RC | Chưa có; raw audio không qua M11 |
| M11-ACT-020 | M06 | Ledger/balance/ownership support read | Personal | R3 | P,S,RA,R,C,A | Balance read/adjust nằm admin controller, chưa ledger view |
| M11-ACT-021 | M06 | Asset grant/revoke/adjust | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Có balance/pet mutation trực tiếp, thiếu ledger/idempotency |
| M11-ACT-022 | M06 | Asset catalog item/pet create/update/deprecate | Internal | R3 | P,S,RA,R,V,PV,A,RC | CRUD trực tiếp; delete thay vì deprecate |
| M11-ACT-023 | M07 | Quest/achievement definition read | Internal | R1 | P,S,A | Admin routes hiện có |
| M11-ACT-024 | M07 | Quest/achievement create/update/version | Internal | R3 | P,S,RA,R,V,PV,A,RC | CRUD/toggle, thiếu version/effective/rollback |
| M11-ACT-025 | M07 | Quest/achievement activate/deprecate/delete | Internal | R4 | P,S,RA,R,C,V,PV,A,N,RC | Toggle/delete trực tiếp, thiếu reference impact |
| M11-ACT-026 | M08 | Battle/gym support detail read | Personal | R3 | P,S,RA,R,C,A | Dashboard/gym routes; thiếu case/redaction |
| M11-ACT-027 | M08 | Battle force interrupt/abandon | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Route hiện có; thiếu reason/idempotency/reconcile |
| M11-ACT-028 | M08 | Gym/rule version change/activate | Internal | R4 | P,S,RA,R,C,V,PV,A,RC | Update trực tiếp, thiếu version/snapshot effect |
| M11-ACT-029 | M09 | Group/member support read | Personal | R2 | P,S,R,C,A | Broad group read, thiếu purpose/member redaction |
| M11-ACT-030 | M09 | Group create/update/membership change | Personal | R3 | P,S,RA,R,C,V,A,N,RC | CRUD route, chưa owner/concurrency/audit chuẩn |
| M11-ACT-031 | M09 | Group delete/owner remediation | Personal | R4 | P,S,RA,R,C,V,PV,A,N,RC | SuperAdmin delete trực tiếp, thiếu reference/lifecycle |
| M11-ACT-032 | M09 | Ranking/leaderboard aggregate read | Public/Internal | R1 | P,S,A | Dashboard/public reads; metric/version chưa chuẩn |
| M11-ACT-033 | M10 | Notification template create/update/version | Personal | R3 | P,S,RA,R,V,PV,A,RC | Chưa template registry/version chuẩn |
| M11-ACT-034 | M10 | Broadcast preview/schedule/send/stop | Personal | R4 | P,S,RA,R,C,V,PV,A,N,RC | Broadcast route hiện có; thiếu audience preview/kill/reconcile |
| M11-ACT-035 | M10 | Device/recipient delivery support read | Personal | R3 | P,S,RA,R,C,A | Chưa support view chuẩn; cấm raw token |
| M11-ACT-036 | M11/module | Configuration read/version/activate/rollback | Internal/Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Settings CRUD + admin config update, thiếu registry/version |
| M11-ACT-037 | M11 | Audit/log search/detail/export/hold | Sensitive | R4 | P,S,RA,R,C,V,PV,A,RC | System/activity logs broad; payload/retention/access-audit gap |
| M11-ACT-038 | M11/module | Job read/run/rerun/pause/compensate | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Chưa job registry/run lifecycle chung |
| M11-ACT-039 | M11 | Support case create/assign/resolve/reopen | Personal | R3 | P,S,R,C,V,A,N | Chưa case model |
| M11-ACT-040 | M11 | Incident declare/contain/recover/close | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Chưa incident model/playbook |
| M11-ACT-041 | M11/M12 | Maintenance cache flush/DB cleanup | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Routes hiện có; thiếu scoped preview/idempotency/result |
| M11-ACT-042 | M12 | Capability/config/limiter activation change | Sensitive | R4 | P,S,RA,R,C,V,PV,A,N,RC | Config/rate controls phân tán; không activation workflow |
| M11-ACT-043 | M12 | Secret rotate/revoke metadata operation | Secret | R4 | P,S,RA,R,C,V,PV,A,N,RC | Chưa inventory/lifecycle; secret value cấm qua M11 |
| M11-ACT-044 | M11/M12 | Health/capability metadata read | Internal | R2 | P,S,A | Health route có static/partial signal; không chứng minh correctness |

## Quy tắc entry-point và lifecycle catalog

| Chủ đề | Quy tắc bắt buộc |
|---|---|
| Stable ID | Action ID bất biến; đổi owner/effect/data class/risk/control tạo catalog version mới, không tái dùng ID cũ cho nghĩa khác |
| Entry point | Mọi API/UI/job/CLI/support tool map đúng một primary Action ID và các nested effect ID; coverage test chặn entry point mồ côi |
| Resource | Khai resource type/owner/version/identifier class; client ID không thay authorization/ownership lookup |
| Permission | M11-T004 map action → permission → scope; không map thẳng action → role trong code mới |
| Re-auth | R3/R4 dùng admin session class + proof freshness 5 phút; proof purpose-bound, current state/policy/security |
| Reason/case | R2 sensitive read cần purpose; R3/R4 mutation cần reason; action support/identity/asset/delete cần case/change/incident theo catalog |
| Mutation | Operation ID, expected version/CAS, semantic result, retry/reconcile và audit commit boundary; timeout unknown không retry mù |
| Bulk | Khai selector snapshot, maximum, preview count/sample, exclusion, stop/rollback và per-item/final result; không vòng lặp route đơn không kiểm soát |
| Read/export | Field allowlist/redaction, page/size/time bound, access audit; export thêm recipient, expiry, watermark và deletion |
| Deprecation | Action/resource ngừng dùng giữ mapping lịch sử; consumer mới bị chặn, không hard-delete khi còn audit/reference |

## Quy tắc bao phủ động từ

| Động từ | Điều kiện catalog đạt |
|---|---|
| Xem | Tách dữ liệu tổng hợp/chi tiết/nhạy cảm; lượt xem nhạy cảm có purpose và audit |
| Tạo/sửa | Module nguồn validation; concurrency/version rõ; không ghi đè âm thầm |
| Gửi rà soát/xác nhận | Không tự tạo duyệt hai người bắt buộc; ghi đúng authority và phạm vi quyết định khi có |
| Kích hoạt/ngừng | Có version, hiệu lực, impact, quyền và rollback/recovery |
| Điều chỉnh | Không sửa nguồn trực tiếp; dùng hợp đồng mutation có idempotency và đối soát |
| Xóa/ẩn danh | Có reference analysis, retention/hold và manifest phần lỗi |
| Xuất | Có purpose, phạm vi, che dữ liệu, người nhận, thời hạn và audit |
| Chạy/chạy lại | Job có owner, checkpoint, idempotency/bù, dry-run và reconciliation |

## Đối chiếu tĩnh hiện trạng

| Finding ID | Quan sát | Sai lệch/rủi ro | Task tiếp nhận |
|---|---|---|---|
| M11-ACT-I01 | Authorization chủ yếu là chuỗi role trên controller | Không permission/scope/resource/policy decision; role rộng thành authority | M11-T003–T005; A-G02 |
| M11-ACT-I02 | Admin/SuperAdmin có thể bỏ ownership bằng `requestingUserId = null` ở vocabulary set | Authority suy từ nullable argument, không trusted actor/resource decision | M11-T004; M02-T016; REL-02 |
| M11-ACT-I03 | Role coverage không nhất quán: có route chỉ Admin, chỉ SuperAdmin hoặc cả hai không kèm catalog reason | Không giải thích least privilege/risk hoặc phát hiện drift | M11-T003–T004 |
| M11-ACT-I04 | Role/status/balance/pet, config, broadcast, force-abandon và maintenance có route mutation trực tiếp | Thiếu re-auth/reason/case/version/preview/idempotency/reconcile đồng nhất | M11-T005–T011, T041–T044 |
| M11-ACT-I05 | Delete trực tiếp xuất hiện ở user/content/group/catalog | Chưa reference analysis, retention/hold/deprecation/rollback | M11-T017, T019–T021; module lifecycle tasks |
| M11-ACT-I06 | Activity/System log được dùng làm dấu vết một số admin action | Không chứng minh immutable audit commit, before/after redaction và access audit | M11-T031–T035 |
| M11-ACT-I07 | Dashboard/support read trả dữ liệu theo ID rộng | Thiếu purpose/case, field allowlist, masking và query audit | M11-T025–T030; REL-07 |
| M11-ACT-I08 | Không thấy central action registry/coverage test | Entry point mới có thể phát hành ngoài permission/audit/risk policy | M11-T049; A-G02 |

I01–I08 là release gap; catalog mô tả target contract và không xác nhận route hiện tại an toàn.

## Ma trận tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| AC02-01 | Route admin mới không Action ID | Coverage/build gate fail; deny release |
| AC02-02 | Role đúng nhưng thiếu permission/scope | Deny tại server enforcement point |
| AC02-03 | R4 mutation không recent re-auth/reason | Challenge/deny; không effect/audit success |
| AC02-04 | Actor và target cùng ID trong self-role change | Vẫn áp separation/last-admin/resource policy; không tự cấp |
| AC02-05 | `requestingUserId = null` | Không suy admin hoặc bỏ ownership |
| AC02-06 | Bulk selector thay đổi sau preview | Version/snapshot mismatch; re-preview/re-authorize |
| AC02-07 | Mutation timeout sau điểm commit | Unknown + reconcile operation; không resend như action mới |
| AC02-08 | Audit store lỗi trước R4 effect | Fail-closed; không mutation |
| AC02-09 | Module nguồn reject invariant | M11 giữ reject; không sửa DB/báo success |
| AC02-10 | Export sensitive data | Purpose/case/scope/redaction/recipient/expiry/access audit đầy đủ |
| AC02-11 | Delete resource còn reference/hold | Deny/deprecate; không hard-delete |
| AC02-12 | Maintenance request không preview scope | Deny; không cache flush/cleanup rộng |
| AC02-13 | Secret rotation qua admin UI payload | Secret value bị chặn; chỉ protected reference/operation metadata |
| AC02-14 | Health HTTP 200 nhưng dependency stale | Chỉ read metadata; không suy correctness/activation |
| AC02-15 | Same action gọi qua API và job | Cùng Action ID/policy/audit semantics, workload identity riêng |
| AC02-16 | Action ID đổi owner/risk/control | Version/migration + consumer coverage; ID cũ deprecate, không đổi nghĩa ngầm |

## Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M11-ACT-F01 | Permission/scope assignment và grant lifecycle cho 13 role | 44 action deny-by-default, role catalog M11-ROLE-1.0 không authorize trực tiếp | M11-T004–T007; M01-T028–T030 |
| M11-ACT-F02 | Change/case/config/content/job/incident lifecycle details | R3/R4 giữ re-auth/reason/version/preview/audit/reconcile tối thiểu | M11-T008–T021, T027–T030, T038–T048 |
| M11-ACT-F03 | Audit/redaction/access/retention enforcement | Sensitive action/read thiếu immutable audit bị fail-closed | M11-T031–T035; REL-02 |
| M11-ACT-F04 | Route/job/CLI coverage mapping và runtime deny tests | Entry point không catalog bị chặn phát hành | M11-T049; A-G02 |

## Tự kiểm M11-T002, A-G02 và REL-02

- 17 group bao phủ M01–M12; 44 action nguyên tử có owner, resource/effect, data class, risk floor, control và trạng thái tĩnh.
- Read/create/update/review/activate/deprecate/adjust/delete/export/run/rerun/maintenance/kill đều có policy; action không đăng ký deny release.
- R0–R4 và 10 control viết tắt tách permission/scope, state, re-auth, reason/case, version, preview, audit, notification và reconcile.
- 8 sai lệch hiện trạng và 4 finding mở có task tiếp nhận; catalog không gán role hoặc hợp thức hóa broad route hiện tại.
- 16 case bao phủ route mồ côi, nullable-authority, bulk/race/unknown/audit failure, source invariant, export/delete/secret/health và versioning.
- A-G02/REL-02 có baseline nhưng chưa role matrix, audit contract hoặc runtime deny evidence; không kết luận gate đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo catalog nhóm hành động M01–M12 và quy tắc bao phủ động từ | Chưa gán |
| 2026-08-20 | 1.0 | Chốt 17 group, 44 action nguyên tử, risk/control, 8 sai lệch và 16 case | WSA-7K2 |
