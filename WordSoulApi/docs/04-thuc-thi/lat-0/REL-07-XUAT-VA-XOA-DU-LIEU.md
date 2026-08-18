# REL-07 — Xuất và xóa dữ liệu

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T005 |
| Trạng thái | Chờ phạm vi sơ bộ từ REL-01 |
| Chủ trì / xác nhận | Chủ M01 / Chủ M11, riêng tư và các chủ module dữ liệu |
| Cá nhân thực tế | Chưa gán |
| Chặn | A-G01 và A-G02; phát hành đầy đủ |

## Phạm vi

Xác định hành trình yêu cầu, xác minh chủ thể, xuất, xóa hoặc ẩn danh hóa và đối soát dữ liệu trên toàn bộ module. Bao gồm yêu cầu tự phục vụ, yêu cầu qua hỗ trợ, chạy lại phần lỗi và bằng chứng hoàn tất.

## Cần xác nhận

- Module và loại dữ liệu nằm trong phạm vi, chủ dữ liệu và quy tắc lưu giữ.
- Mức xác minh chủ thể theo từng kênh yêu cầu và trường hợp mất quyền truy cập.
- Thời hạn phản hồi, định dạng/manifest kết quả và cách chuyển giao an toàn.
- Dữ liệu phải xóa, được ẩn danh hóa hoặc được giữ vì nghĩa vụ hợp lệ.
- Cách xử lý phần lỗi, chạy lại, đối soát và thông báo kết quả cho người dùng.

## Bằng chứng và tiêu chí đóng

Bản đồ dữ liệu có xác nhận của mọi module, kịch bản xuyên module, manifest xuất, bằng chứng xóa/ẩn danh, báo cáo phần lỗi và kết quả đối soát phải đạt. Không đóng khi còn module chưa xác nhận hoặc khi xóa bản ghi chính nhưng dữ liệu dẫn xuất vẫn nhận diện được người dùng ngoài chính sách.
