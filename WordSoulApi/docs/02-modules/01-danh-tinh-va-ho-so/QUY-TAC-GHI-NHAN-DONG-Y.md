# Quy tắc ghi nhận đồng ý M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T007 |
| Phiên bản/trạng thái | 0.1-draft — chờ M01-T002/M01-T003 và REL-01 |
| Giới hạn | Không thay kết luận pháp lý; WSA-7K2 chỉ đặc tả khả năng truy vết và thực thi |

## Mô hình bản ghi append-only

| Trường | Yêu cầu |
|---|---|
| Consent type/purpose | Một mục đích rõ; không gộp điều khoản bắt buộc với nhận tin/tùy chọn độc lập |
| Policy version/locale | Phiên bản nội dung và ngôn ngữ mà người dùng thực sự được trình bày |
| Subject/account | Định danh bảo vệ; quan hệ người giám hộ chỉ khi REL-01 yêu cầu và có mô hình được duyệt |
| Decision | Granted, declined hoặc withdrawn; không suy diễn im lặng thành granted |
| Occurred/recorded time | Event time và thời điểm hệ thống ghi, lưu chuẩn UTC |
| Collection context | Journey/channel/application version và evidence metadata tối thiểu, không payload thô |
| Supersedes | Liên kết bản ghi trước khi thay phiên bản/thu hồi; không sửa xóa lịch sử thường lệ |

## Quy tắc thực thi

- Mỗi năng lực khai báo consent type/policy version cần thiết và hành vi khi thiếu, hết hiệu lực hoặc rút lại.
- Policy version mới không tự kế thừa đồng ý nếu reviewer pháp lý xác định cần chấp thuận lại.
- Thu hồi có hiệu lực theo mốc được duyệt, chặn xử lý tương lai và phát sự kiện cho consumer; phần đã xử lý/retention theo REL-01/07.
- Retry cùng quyết định là idempotent; quyết định cạnh tranh dùng event/version order xác định, không ghi đè âm thầm.
- Consumer không cache quyết định quá thời hạn hợp đồng và phải fail-closed cho năng lực cần đồng ý bắt buộc.

## Ma trận trạng thái tối thiểu

| Tình huống | Kết quả mặc định an toàn | Audit |
|---|---|---|
| Thiếu bản ghi bắt buộc | Không mở năng lực liên quan; chỉ đường bổ sung đồng ý | Missing type/version, consumer, correlation |
| Declined/withdrawn | Dừng xử lý tương lai theo purpose và thu hồi consumer đã đăng ký | Actor, prior/new state, effective time |
| Policy version thay đổi | Dùng quyết định migration/re-consent đã duyệt; không tự coi bản cũ hợp lệ | Old/new version, decision owner |
| Consumer lỗi khi thu hồi | Trạng thái trung tâm vẫn ghi nhận; tạo retry/reconciliation và báo phần lỗi | Consumer, attempt, result, next action |
| REL-01 chưa có kết luận | Giữ cổng/chức năng nhạy cảm chưa mở; không tự đặt mặc định pháp lý | Gate/blocker reference |

## Điểm chờ và điều kiện duyệt

| Mã | Điểm chờ | Nguồn gỡ |
|---|---|---|
| M01-CONS-O01 | Consent types bắt buộc/tùy chọn theo thị trường/nhóm tuổi | REL-01/pháp lý/sản phẩm |
| M01-CONS-O02 | Khi nào policy mới yêu cầu re-consent | Chủ chính sách + pháp lý |
| M01-CONS-O03 | Consumer, SLA thu hồi và retention từng purpose | M10/M12/các chủ xử lý; REL-07 |

Chỉ duyệt khi REL-01 có authority thực tế, O01–O03 được đóng và có ca grant/decline/withdraw/version change/retry/consumer partial failure.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo mô hình append-only và quy tắc hiệu lực/thu hồi | Chưa gán |
