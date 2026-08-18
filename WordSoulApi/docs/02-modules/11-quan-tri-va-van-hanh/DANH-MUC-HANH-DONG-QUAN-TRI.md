# Danh mục hành động quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T002 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp — chờ M11-T001 và các chủ module xác nhận |
| Chủ catalog | Chủ M11 |
| Quy tắc | Module nguồn sở hữu hành động/validation nghiệp vụ; M11 sở hữu catalog, quyền, luồng quản trị và audit |

## Chuẩn phân loại

| Thuộc tính | Giá trị chuẩn |
|---|---|
| Động từ | Xem, tạo, sửa, gửi rà soát, xác nhận, kích hoạt, ngừng, điều chỉnh, xóa/ẩn danh, xuất, chạy/chạy lại, bảo trì/dừng |
| Mức rủi ro | Thấp, trung bình, cao, rất cao; dựa trên quyền, dữ liệu, phạm vi người dùng, tài sản và khả năng phục hồi |
| Kiểm soát tối thiểu | Quyền/phạm vi, xác minh lại khi nhạy cảm, lý do/vụ việc, preview khi cần, audit và hành vi fail-closed |
| Trạng thái owner | Chưa xác nhận, đã xác nhận, thay thế/không áp dụng |

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

## Khoảng trống cần chủ module xác nhận

| Mã | Khoảng trống | Điều kiện đóng |
|---|---|---|
| M11-ACT-O01 | Action group mới chỉ ở mức nhóm, chưa tách endpoint/command nghiệp vụ | Mỗi chủ module xác nhận/tách danh sách hành động thực tế |
| M11-ACT-O02 | Mức rủi ro là mức trần dự kiến | An toàn hệ thống và chủ module xác nhận tiêu chí/ngưỡng |
| M11-ACT-O03 | Vai trò được phép chưa được gán | Chờ M11-T003/M11-T004; catalog không tự cấp quyền |
| M11-ACT-O04 | Một số thuật ngữ backlog cũ còn giả định duyệt/quyền khẩn | Áp dụng TU-DIEN-QUAN-TRI và M11-T006-A trước nghiệm thu |

## Điều kiện duyệt M11-T002

- M11-T001 được xác nhận và mọi action group có chủ module duy nhất.
- Bao phủ xem/tạo/sửa/xác nhận/kích hoạt/điều chỉnh/xóa/xuất/chạy lại hoặc ghi rõ không áp dụng.
- Mỗi action có tài nguyên, mức rủi ro, điều kiện từ chối và bằng chứng bắt buộc.
- M11-ACT-O01–O04 được đóng hoặc chuyển thành task/finding có chủ.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo catalog nhóm hành động M01–M12 và quy tắc bao phủ động từ | Chưa gán |
