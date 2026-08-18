# REL-03 — Bí mật và tích hợp

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T003 |
| Trạng thái | Mở — chờ gán người và kiểm kê |
| Chủ trì / xác nhận | Chủ M12 / An toàn hệ thống và vận hành |
| Cá nhân thực tế | Chưa gán |
| Chặn | A-G04, A-G05 và A-G06 |

## Phạm vi

Kiểm kê năng lực tích hợp và bí mật liên quan, xác định chủ sở hữu, dữ liệu đi ra ngoài, hạn mức, failure mode, phương án suy giảm và xử lý mọi bí mật nghi lộ. Hồ sơ tuyệt đối không lưu giá trị bí mật.

## Cần xác nhận

- Danh sách tích hợp đang hoạt động, mục đích, module tiêu thụ và mức quan trọng.
- Vị trí quản lý bí mật theo loại, chủ sở hữu, vòng đời và mức phơi lộ; không ghi giá trị.
- Bí mật nào cần thu hồi, xoay vòng hoặc thay thế và thẩm quyền xác nhận hoàn thành.
- Hành vi khi timeout, quota, outage, phản hồi không chắc chắn và hệ thống phụ thuộc lỗi.
- Phạm vi cần kill switch, giới hạn lưu lượng, retry, suy giảm và cảnh báo.

## Bằng chứng và tiêu chí đóng

Registry tích hợp, inventory bí mật đã duyệt, bản đồ dữ liệu ngoài, biên bản thu hồi/xoay vòng và kết quả kiểm thử timeout–quota–outage–kill switch phải truy vết được. Không đóng nếu còn tích hợp không chủ, bí mật nghi lộ chưa xử lý hoặc đường bỏ qua giới hạn lưu lượng.
