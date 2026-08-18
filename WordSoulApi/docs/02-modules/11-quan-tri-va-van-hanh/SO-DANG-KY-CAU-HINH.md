# Sổ đăng ký cấu hình M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T012 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp khung — chờ M11-T001 và từng chủ module kiểm kê/xác nhận |
| Cơ sở quyết định | M11-D008–D011: live-change theo allowlist, mọi thay đổi tạo phiên bản, policy set quay lui nhất quán, khóa ngừng dùng không bị xóa khi còn tham chiếu |
| Chủ registry | M11; module nguồn sở hữu ý nghĩa, validation và xác nhận live/restart |

## Schema bắt buộc cho mỗi khóa

| Trường | Yêu cầu |
|---|---|
| Config ID/key | Định danh ổn định, không chứa bí mật hoặc giá trị theo môi trường |
| Module owner/contact | Một module và danh tính chủ thực tế; khóa vô chủ bị chặn kích hoạt |
| Mục đích/người tiêu thụ | Quyết định được điều khiển và danh sách năng lực đọc |
| Kiểu/đơn vị/default | Kiểu dữ liệu, đơn vị, default có nghĩa và nguồn default |
| Phạm vi | Toàn cục, thị trường, phân khúc, tenant hoặc năng lực; thứ tự ưu tiên rõ |
| Nhạy cảm/hiển thị | Công khai, nội bộ, nhạy cảm hoặc secret reference; không lưu secret value trong registry |
| Validation/phụ thuộc | Min/max/tập giá trị, quy tắc chéo và module thực thi validation |
| Mức rủi ro | Thấp/trung bình/cao/rất cao cùng lý do và kiểm soát thay đổi |
| Cơ chế hiệu lực | Live allowlist hoặc cần restart/release/maintenance; không để giá trị không xác định |
| Consumer/version | Phiên bản hợp đồng đọc và hành vi khi thiếu/không hợp lệ |
| Retention/deprecation | Thời hạn lịch sử, điều kiện ngừng dùng và kiểm tra tham chiếu trước xóa |
| Quan sát/rollback | Chỉ số theo dõi, cửa sổ quan sát, phiên bản/bộ policy để quay lui |

## Seed registry cần chủ xác nhận

| Config group | Owner dự kiến | Consumer chính | Phạm vi/loại | Rủi ro | Hiệu lực dự kiến | Điều kiện trước khi dùng | Owner status |
|---|---|---|---|---|---|---|---|
| CFG-M01-ACCOUNT-POLICY | M01 | Auth/profile | Policy set; nội bộ | Rất cao | Chờ xác nhận live/release | Phiên bản, REL-01/02, validation và rollback | Chưa xác nhận |
| CFG-M01-SESSION-POLICY | M01 | Auth/admin | Policy set; nhạy cảm | Rất cao | Chờ xác nhận | Thời hạn/thu hồi nhất quán, kiểm thử phiên cũ | Chưa xác nhận |
| CFG-M02-CONTENT-POLICY | M02 | Nội dung/học | Policy set; nội bộ | Cao | Chờ chủ M02 | Version nội dung, reference impact, CT-01/REL-04 | Chưa xác nhận |
| CFG-M03-LEARNING-POLICY | M03 | Phiên học | Policy set; nội bộ | Cao | Chờ chủ M03 | Tương thích tiến độ, idempotency và rollback | Chưa xác nhận |
| CFG-M04-REVIEW-POLICY | M04 | Lịch ôn | Policy set; nội bộ | Cao | Chờ chủ M04 | Mô phỏng lịch/độ trễ, version thuật toán | Chưa xác nhận |
| CFG-M05-PRONUNCIATION-POLICY | M05 | Phát âm | Policy set; nội bộ | Cao | Chờ chủ M05 | Ngưỡng/chất lượng, consent và degraded mode | Chưa xác nhận |
| CFG-M06-ECONOMY-POLICY | M06 | Ledger/reward | Policy set; nhạy cảm | Rất cao | Chờ chủ M06 | Preview tác động, hạn mức, reconciliation | Chưa xác nhận |
| CFG-M07-MISSION-POLICY | M07 | Nhiệm vụ/thành tựu | Policy set; nội bộ | Cao | Chờ chủ M07 | Version, tham chiếu, lịch hiệu lực | Chưa xác nhận |
| CFG-M08-BATTLE-POLICY | M08 | Phòng/trận | Policy set; nội bộ | Rất cao | Chờ chủ M08 | Match version, fairness, rollout/rollback | Chưa xác nhận |
| CFG-M09-COMMUNITY-POLICY | M09 | Nhóm/xếp hạng | Policy set; nội bộ | Cao | Chờ chủ M09 | Riêng tư, ranh giới kỳ, reference impact | Chưa xác nhận |
| CFG-M10-NOTIFICATION-POLICY | M10 | Gửi tin | Policy set; nhạy cảm | Rất cao | Chờ chủ M10 | Consent, audience preview, limiter/kill switch | Chưa xác nhận |
| CFG-M11-OPS-POLICY | M11 | Admin/jobs/log/incident | Policy set; nhạy cảm | Rất cao | Theo từng khóa allowlist | Quyền, audit, health và rollback | Chưa xác nhận |
| CFG-M12-CAPABILITY-POLICY | M12 | Provider/limiter/assets | Policy set; secret reference | Rất cao | Chờ chủ M12 | REL-03, không chứa secret value, fail-open/closed | Chưa xác nhận |

## Trạng thái và kiểm soát registry

- Vòng đời khóa: `Đề xuất` → `Đã đăng ký` → `Đang dùng` → `Ngừng dùng` → `Đủ điều kiện xóa`; chỉ chủ module xác nhận chuyển qua mốc nghiệp vụ.
- Mọi giá trị hiệu lực thuộc phiên bản bất biến; thay đổi nhiều khóa liên quan dùng cùng policy-set version.
- Cấu hình trung bình/cao phải lên lịch tuyệt đối, có thông báo, cửa sổ theo dõi và rollback; mức thấp chỉ hiệu lực ngay khi registry cho phép rõ.
- Đọc khóa thiếu/không hợp lệ phải theo hợp đồng consumer; không tự fallback sang giá trị ngầm không đăng ký.
- Registry chỉ lưu secret reference/metadata; giá trị bí mật thuộc M12 và không xuất hiện trong preview/audit.

## Khoảng trống phải đóng

| Mã | Khoảng trống | Điều kiện đóng |
|---|---|---|
| M11-CFG-O01 | Chưa có key vật lý, default và consumer thực tế | Từng chủ module nộp inventory có phiên bản |
| M11-CFG-O02 | Chưa phân loại live/restart/release cho từng key | Chủ module và vận hành kiểm chứng, không suy đoán |
| M11-CFG-O03 | Chưa có retention/deprecation số cụ thể | M11/riêng tư/vận hành xác nhận theo loại |
| M11-CFG-O04 | Chưa có ánh xạ health/rollback cho từng policy set | M11-T015–T017 và M11-T036 |

## Điều kiện duyệt M11-T012

- Không có key vô chủ; mỗi key có đầy đủ schema bắt buộc và consumer xác nhận.
- Live-change là allowlist được kiểm chứng; key ngoài allowlist có cơ chế release/maintenance rõ.
- Secret chỉ là reference; default/fallback, retention và deprecation được phiên bản hóa.
- M11-CFG-O01–O04 được đóng hoặc thành finding/task có chủ.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema registry và seed 12 nhóm cấu hình xuyên module | Chưa gán |
