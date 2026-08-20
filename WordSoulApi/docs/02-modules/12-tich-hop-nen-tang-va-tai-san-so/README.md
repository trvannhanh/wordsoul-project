# M12 — Tích hợp nền tảng và tài sản số

## Tài liệu phân tích

- [Phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Task backlog](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)
- [Đánh giá hiện trạng A-WP04](DANH-GIA-HIEN-TRANG-A-WP04.md)
- [Từ điển tích hợp M12](TU-DIEN-TICH-HOP.md)
- [Sổ đăng ký năng lực tích hợp M12](SO-DANG-KY-NANG-LUC-TICH-HOP.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Tích hợp nền tảng và tài sản số |
| Mục đích | Cung cấp một ranh giới thống nhất giữa nghiệp vụ WordSoul với các năng lực bên ngoài và năng lực hạ tầng dùng chung, giúp các module nghiệp vụ không phụ thuộc trực tiếp vào đặc thù từng nhà cung cấp. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** kết nối nhà cung cấp danh tính; hỗ trợ tạo nội dung bằng trí tuệ nhân tạo; tìm và lưu tài sản hình ảnh; tổng hợp/đánh giá giọng nói; gửi thư và thông báo đẩy; truyền dữ liệu tức thời; lưu tạm dữ liệu phù hợp; kiểm soát tần suất; báo cáo trạng thái và lỗi tích hợp. **Không chịu trách nhiệm:** quyết định nội dung nào được xuất bản, người dùng phát âm đạt hay không, thời điểm cần nhắc học, ai thắng trận hoặc ai được nhận thưởng. |
| Đầu vào (Input) | Yêu cầu có mục đích rõ ràng từ module nghiệp vụ; dữ liệu cần xử lý ở mức tối thiểu; thông tin cấu hình và quyền truy cập dịch vụ từ vận hành; phản hồi hoặc sự kiện từ nhà cung cấp bên ngoài. |
| Đầu ra (Output) | Kết quả đã chuẩn hóa cho module gọi; tài sản số và vị trí tham chiếu; trạng thái gửi; kết quả xử lý giọng nói; nội dung gợi ý; lỗi và chỉ số sử dụng cho quản trị; phương án suy giảm chức năng khi dịch vụ ngoài không sẵn sàng. |
| Phụ thuộc (Dependencies) | Các nhà cung cấp danh tính, trí tuệ nhân tạo, giọng nói, hình ảnh, thư và thông báo đẩy; quản trị, cấu hình và quan sát hệ thống; kho lưu trữ và năng lực vận hành dùng chung. |
| Người dùng/vai trò liên quan | Không có người dùng nghiệp vụ trực tiếp; người học và quản trị viên tương tác gián tiếp qua các module sử dụng; đội vận hành theo dõi chất lượng và chi phí. |
| Độ ưu tiên | Trung bình |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Phụ thuộc nhà cung cấp; chi phí tăng không kiểm soát; giới hạn lưu lượng; lộ thông tin bí mật hoặc dữ liệu cá nhân; điều khoản bản quyền; độ trễ; kết quả không ổn định; thay đổi dịch vụ ngoài; thiếu phương án dự phòng; lưu tạm dữ liệu cũ gây kết quả sai. |

## Năng lực nghiệp vụ chính

- Chuẩn hóa trao đổi với các dịch vụ bên ngoài để module nghiệp vụ chỉ nhận kết quả cần thiết.
- Quản lý vòng đời tải lên, lưu trữ và loại bỏ tài sản số.
- Theo dõi mức dùng, chi phí, lỗi và chất lượng theo từng năng lực.
- Giới hạn tần suất và ngăn lạm dụng các hoạt động tốn chi phí.
- Cho phép suy giảm có kiểm soát: vẫn học được khi chức năng phụ trợ tạm gián đoạn, nếu nghiệp vụ cho phép.
- Bảo vệ thông tin kết nối và giảm tối đa dữ liệu cá nhân gửi ra ngoài.

## Điểm cần làm rõ

- Dịch vụ bên ngoài nào là lựa chọn chính thức và cam kết chất lượng của từng dịch vụ là gì?
- Dữ liệu nào được phép rời khỏi hệ thống, đặc biệt là âm thanh và thông tin cá nhân?
- Mức chi phí tối đa theo người dùng/tháng và ngưỡng cảnh báo là bao nhiêu?
- Năng lực nào cần nhà cung cấp dự phòng và tiêu chí chuyển đổi là gì?
