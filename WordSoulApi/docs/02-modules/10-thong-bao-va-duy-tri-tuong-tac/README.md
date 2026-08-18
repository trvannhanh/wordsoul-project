# M10 — Thông báo và duy trì tương tác

## Tài liệu phân tích

- [Phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Task backlog](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Thông báo và duy trì tương tác |
| Mục đích | Đưa thông tin và lời nhắc đúng thời điểm, qua kênh phù hợp, để người học không bỏ lỡ lịch ôn, phần thưởng, hoạt động xã hội hoặc sự kiện hệ thống. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** tạo thông báo từ sự kiện hợp lệ; lưu hộp thư trong hệ thống; trạng thái đã đọc; phát theo thời gian thực; gửi thư hoặc thông báo đẩy; lập lịch nhắc học; gửi thông báo diện rộng theo quyền; tôn trọng lựa chọn nhận tin. **Không chịu trách nhiệm:** quyết định một từ đến hạn, xác định nhiệm vụ hoàn thành, quyết định kết quả trận hoặc quản lý tài khoản thiết bị ngoài thông tin cần để gửi. |
| Đầu vào (Input) | Danh tính, địa chỉ liên hệ, thiết bị và lựa chọn nhận tin; tín hiệu đến hạn từ module tiến độ; sự kiện nhiệm vụ, thành tựu, thi đấu và nhóm; nội dung thông báo diện rộng từ quản trị; mẫu nội dung và lịch gửi; kết quả gửi từ nền tảng bên ngoài. |
| Đầu ra (Output) | Thông báo trong hệ thống; thông báo theo thời gian thực; thư và thông báo đẩy; trạng thái gửi/đọc; thống kê hiệu quả và lỗi gửi cho quản trị; lời nhắc quay lại học cho người dùng. |
| Phụ thuộc (Dependencies) | Danh tính và hồ sơ; ôn tập ngắt quãng và tiến độ; nhiệm vụ và thành tựu; đấu trường; nhóm và xếp hạng; quản trị, cấu hình và quan sát; tích hợp nền tảng cho thư, thông báo đẩy và truyền tin tức thời. |
| Người dùng/vai trò liên quan | Người học nhận và quản lý thông báo; quản trị viên gửi thông báo diện rộng và theo dõi hiệu quả. |
| Độ ưu tiên | Trung bình |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Gửi trùng hoặc quá nhiều; sai múi giờ; gửi nội dung nhạy cảm đến thiết bị khóa; không tôn trọng lựa chọn nhận tin; thông báo diện rộng gửi nhầm; nhà cung cấp gián đoạn; địa chỉ/thiết bị hết hiệu lực; tỷ lệ gửi thành công không phản ánh người dùng đã tương tác. |

## Năng lực nghiệp vụ chính

- Tạo và quản lý hộp thông báo cá nhân trong hệ thống.
- Đánh dấu đã đọc, đọc tất cả và xóa theo quyền người dùng.
- Phát sự kiện cần phản hồi ngay trong phiên sử dụng.
- Gửi nhắc học, thư và thông báo đẩy theo lịch và lựa chọn kênh.
- Phát thông báo diện rộng có kiểm soát và khả năng truy vết.
- Đo hiệu quả từ gửi thành công đến quay lại hoạt động học.

## Điểm cần làm rõ

- Người dùng được bật/tắt từng loại thông báo và từng kênh đến mức nào?
- Có khoảng thời gian yên lặng theo múi giờ cá nhân không?
- Quy tắc chống gửi lặp và giới hạn tần suất cho từng loại sự kiện là gì?
- Thông báo đã xóa có cần giữ bản ghi phục vụ kiểm toán hay không?
