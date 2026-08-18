# Ma trận nội dung quản trị chéo module M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T018 |
| Phiên bản/trạng thái | 0.1-draft — chờ M11-T002 và chủ M01–M12 xác nhận |
| Nguyên tắc | Module nguồn sở hữu mô hình, validation và quyết định hợp lệ; M11 chỉ sở hữu quyền, workflow, lịch hiệu lực, giao diện điều hành và audit |

## Ma trận trách nhiệm

| Miền nội dung/thay đổi | Module nguồn | M11 được làm | M11 không được làm | Hợp đồng/bằng chứng tối thiểu | Owner status |
|---|---|---|---|---|---|
| Danh tính, trạng thái, vai trò | M01 | Điều phối thao tác đúng quyền, hiển thị trạng thái/kết quả | Tự diễn giải trạng thái, sửa credential hoặc bỏ thu hồi phiên | Command/version, reason, before/after, revoke result | Chưa xác nhận |
| Mục từ/nghĩa/bộ từ | M02 | Quản lý draft/review/effective state theo action catalog | Tự xác nhận chất lượng học liệu hoặc sửa bản đang hiệu lực | Content version, validation, reference impact, decision | Chưa xác nhận |
| Quy tắc phiên học | M03 | Lên lịch/kích hoạt policy version đã hợp lệ | Sửa lịch sử/kết quả phiên trực tiếp | Policy version, compatibility, rollout/rollback | Chưa xác nhận |
| Thuật toán/lịch ôn | M04 | Điều phối version, preview, rollout và rollback | Thay đổi tiến độ nguồn hoặc chạy lại thiếu idempotency | Simulation, cohort, checkpoint/reconciliation | Chưa xác nhận |
| Ngưỡng/chính sách phát âm | M05 | Quản lý policy và trạng thái provider theo quyền | Xem âm thanh thô hoặc tự đặt ngưỡng học thuật | Policy version, consent, quality/degraded result | Chưa xác nhận |
| Kinh tế/tài sản | M06 | Gửi mutation có vụ việc, preview, hạn mức và audit | Sửa số dư ngoài ledger hoặc tự tạo bút toán | Mutation ID, reason, ledger result, reconciliation | Chưa xác nhận |
| Nhiệm vụ/thành tựu | M07 | Quản lý vòng đời/version/lịch hiệu lực | Sửa version đang dùng hoặc bỏ phân tích tham chiếu | Content version, validation, impact, rollback | Chưa xác nhận |
| Quy tắc/phòng thi đấu | M08 | Điều phối config/version/kill phạm vi | Sửa kết quả trận hoặc thay rule giữa trận trái hợp đồng | Match/config version, fairness check, recovery | Chưa xác nhận |
| Nhóm/xếp hạng | M09 | Điều phối hạn chế/quy tắc version | Sửa lịch sử/xếp hạng hoặc lộ thành viên ngoài quyền | Case/version, privacy scope, before/after | Chưa xác nhận |
| Mẫu/chiến dịch thông báo | M10 | Draft/test/schedule/stop theo quyền | Bỏ consent, audience preview, limiter hoặc kill switch | Template version, audience count, consent, delivery result | Chưa xác nhận |
| Cấu hình/chỉ số/job/log | M11 + module nguồn | Registry, workflow, quyền, audit, health/reconciliation | Tự sở hữu validation nghiệp vụ của module khác | Registry/version, owner validation, execution evidence | Chưa xác nhận |
| Provider/tài sản/bí mật/limiter | M12 | Điều phối metadata/capability state qua hợp đồng | Xem/ghi secret value hoặc bỏ fail-open/closed đã duyệt | Capability/config version, secret reference, health/result | Chưa xác nhận |

## Quy tắc xung đột

- M11 từ chối thay đổi nếu module nguồn không xác nhận version/validation hoặc contract version không tương thích.
- Hai thay đổi cùng tài nguyên phải phát hiện version conflict; người sau nhận trạng thái mới và tạo yêu cầu mới/merge có chủ, không ghi đè âm thầm.
- Thay đổi xuyên module dùng change set có từng phần, thứ tự, rollback/bù và reconciliation; không báo thành công toàn phần khi còn phần lỗi.
- M11 không tạo “duyệt hai người” mặc định; kiểm soát tăng cường dùng quyền, xác minh lại, lý do, preview, audit và vai trò cao hơn theo quyết định đã chốt.

## Điểm chờ và điều kiện duyệt

| Mã | Điểm chờ | Nguồn gỡ |
|---|---|---|
| M11-XMOD-O01 | Resource/action cụ thể và contract version từng module | Chủ M01–M12 |
| M11-XMOD-O02 | Ranh giới transaction/compensation xuyên module | M11-T008–T011 và chủ nguồn |
| M11-XMOD-O03 | Reference graph cho nội dung/tài sản đang dùng | M11-T019/T020; REL-04 |

Chỉ duyệt khi mỗi dòng có một owner, contract/link phiên bản, hành vi conflict/partial failure và xác nhận dùng được của chủ module.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo ma trận ranh giới M01–M12 và quy tắc xung đột | Chưa gán |
