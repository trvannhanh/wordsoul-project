# CT-05 — Cấm bỏ qua giới hạn lưu lượng

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T010 |
| Trạng thái | Chờ REL-03 được mở |
| Chủ trì / xác nhận | Chủ M12 / An toàn hệ thống |
| Rủi ro kiểm soát | Nguồn gọi tự nhận trusted/internal để bỏ qua giới hạn và gây lạm dụng hoặc quá tải |

## Quy tắc tạm thời

- Không nguồn gọi nào được miễn giới hạn chỉ dựa trên dấu hiệu do yêu cầu cung cấp.
- Hành trình nội bộ và bên ngoài đều phải có chính sách giới hạn được xác định rõ.
- Khi không xác minh được trạng thái giới hạn, hành vi phải suy giảm an toàn theo mức quan trọng.
- Mọi thay đổi chính sách phải có chủ, lý do, phạm vi, thời hạn và audit.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / xác nhận | Chưa gán / Chưa gán |
| Bằng chứng định kỳ | Danh mục nguồn gọi, chính sách áp dụng, kịch bản giả mạo dấu hiệu và kết quả từ chối |
| Khi vi phạm | Dừng đường miễn, giới hạn nguồn ảnh hưởng, điều tra lưu lượng và đánh giá dữ liệu bị tác động |
| Điều kiện gỡ | M12-T034–M12-T035 đạt |
| Thẩm quyền gỡ | Chủ M12 và an toàn hệ thống |
