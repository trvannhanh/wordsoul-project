# CT-03 — Không dùng payload thô làm bằng chứng

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T008 |
| Trạng thái | Chờ REL-03 được mở |
| Chủ trì / xác nhận | Chủ M11 / An toàn hệ thống và riêng tư |
| Rủi ro kiểm soát | Token, bí mật hoặc dữ liệu cá nhân xuất hiện trong log và hồ sơ bằng chứng |

## Quy tắc tạm thời

- Payload hoặc phản hồi thô không được coi là bằng chứng vận hành hợp lệ.
- Không sao chép dữ liệu thô vào tài liệu, công cụ quản lý công việc hoặc kênh trao đổi chung.
- Chỉ dùng dữ liệu giả, trường đã cho phép và giá trị đã che khi cần minh họa.
- Nguồn log hiện hữu có rủi ro phải được giới hạn quyền xem và có kế hoạch thay thế.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / xác nhận | Chưa gán / Chưa gán |
| Bằng chứng định kỳ | Danh sách nguồn log, mức nhạy cảm, quyền truy cập, mẫu đã che và kết quả rà soát |
| Khi vi phạm | Hạn chế truy cập, loại bằng chứng, xử lý dữ liệu lộ và đánh giá nhu cầu xoay vòng bí mật |
| Điều kiện gỡ | M11-T031–M11-T035 và M12-T040–M12-T043 đạt |
| Thẩm quyền gỡ | An toàn hệ thống và riêng tư |
