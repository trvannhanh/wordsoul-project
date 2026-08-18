# Từ điển danh tính M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T001 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp chờ chủ M01 và các chủ M02/M11/M12 xác nhận |
| Chủ thuật ngữ | Chủ M01; chủ module liên quan xác nhận thuật ngữ giao tiếp |
| Quy tắc thay đổi | Đổi nghĩa phải tạo phiên bản mới, ghi tác động và xử lý xung đột trước khi module tiêu thụ áp dụng |

## Thuật ngữ chuẩn

| Thuật ngữ | Định nghĩa chuẩn | Chủ sở hữu | Không được hiểu là |
|---|---|---|---|
| Danh tính người dùng | Định danh nội bộ bất biến đại diện cho một chủ thể người dùng trong WordSoul | M01 | Email, tên hiển thị hoặc định danh provider |
| Tài khoản | Vòng đời truy cập gắn với một danh tính, gồm trạng thái, phương thức đăng nhập và chính sách áp dụng | M01 | Hồ sơ hiển thị hoặc một phiên truy cập |
| Hồ sơ | Tập trường người dùng được phép xem/sửa hoặc chọn công khai, có nguồn và quyền riêng cho từng trường | M01 | Nguồn sự thật của tiến độ, tài sản hoặc xếp hạng |
| Thông tin đăng nhập | Bằng chứng bí mật hoặc phương thức đã xác minh dùng để chứng minh quyền truy cập | M01 | Dữ liệu được ghi vào audit/log |
| Danh tính ngoài | Định danh do provider bên ngoài phát hành, được nhận qua hợp đồng M12 | M12; M01 tiêu thụ | Tài khoản WordSoul hoặc email đã đủ để liên kết |
| Liên kết danh tính | Quan hệ được xác nhận giữa tài khoản WordSoul và danh tính ngoài sau khi chứng minh quyền sở hữu | M01/M12 | Tự ghép vì email trùng |
| Xung đột danh tính | Trường hợp bằng chứng đăng nhập/định danh ngoài trùng hoặc đã thuộc quan hệ khác và chưa thể quyết định an toàn | M01 | Lý do tự động hợp nhất tài khoản |
| Xác minh thư điện tử | Chứng minh quyền kiểm soát địa chỉ thư bằng bằng chứng một lần, có hạn và chống dùng lại | M01; M12 cung cấp kênh | Xác minh danh tính pháp lý hoặc quyền liên kết tài khoản hiện có |
| Đồng ý | Bản ghi lựa chọn có loại, phiên bản chính sách, phạm vi, thời điểm, nguồn và trạng thái rút lại | M01/riêng tư | Một cờ không phiên bản hoặc kết luận pháp lý |
| Phiên truy cập | Quyền truy cập có hạn, gắn với tài khoản, thiết bị/phạm vi và có thể thu hồi | M01 | Trạng thái tài khoản hoặc thiết bị được miễn xác minh lại |
| Họ phiên | Nhóm phiên/gia hạn có cùng nguồn phát hành để phát hiện dùng lại và thu hồi đúng phạm vi | M01 | Một token dùng chung cho mọi thiết bị |
| Thiết bị nhận biết | Thiết bị có định danh và lịch sử hoạt động được người dùng nhận biết | M01 | Thiết bị luôn tin cậy hoặc được bỏ qua kiểm soát |
| Chờ xác minh | Trạng thái tài khoản chưa đáp ứng xác minh bắt buộc và chỉ có quyền giới hạn đã công bố | M01 | Tài khoản hoạt động đầy đủ |
| Hoạt động | Trạng thái tài khoản đủ điều kiện truy cập theo vai trò và các hạn chế hiện hành | M01 | Được phép mọi hành động |
| Hạn chế theo chức năng | Giới hạn có lý do, phạm vi, thời hạn và đường khiếu nại áp dụng cho một số năng lực | M01 | Khóa toàn bộ hoặc xóa tài khoản |
| Khóa tài khoản | Trạng thái từ chối quyền truy cập theo chính sách và thu hồi/chặn phiên thuộc phạm vi | M01 | Chỉ đổi cờ hiển thị trong quản trị |
| Chờ xóa | Trạng thái sau khi yêu cầu xóa được xác minh, trong thời gian chờ/có thể hủy theo chính sách | M01 | Đã xóa vật lý mọi dữ liệu |
| Xóa dữ liệu | Loại bỏ dữ liệu thuộc diện xóa theo ma trận đã duyệt và ghi kết quả đối soát | Module sở hữu dữ liệu; M01 điều phối | Xóa bản ghi người dùng chính là hoàn tất toàn bộ |
| Ẩn danh hóa | Biến đổi khiến dữ liệu lịch sử không thể liên kết lại với danh tính cá nhân theo chính sách | Module sở hữu dữ liệu | Chỉ che email/tên ở giao diện |
| Đăng ký lại | Tạo danh tính mới sau khi quy trình xóa hoàn tất và điều kiện tái sử dụng được đáp ứng | M01 | Khôi phục hoặc nối lại dữ liệu danh tính cũ |

## Sổ xung đột thuật ngữ

| Mã | Xung đột cần xử lý | Nguồn/miền liên quan | Cách khóa trong bản nháp | Trạng thái |
|---|---|---|---|---|
| M01-DICT-C01 | “Tên người dùng” đang được dùng vừa như thông tin đăng nhập vừa như tên hiển thị | M01 hiện trạng; M09/community | Dùng email cho đăng nhập trực tiếp; dùng “tên hiển thị” cho hồ sơ; chờ xác nhận contract | Chờ xác nhận |
| M01-DICT-C02 | “User”, “account” và “profile” có nguy cơ bị dùng thay nhau | M01–M12 | Tách theo ba định nghĩa ở trên; giao tiếp dùng định danh nội bộ | Chờ các module tiêu thụ |
| M01-DICT-C03 | “Trusted device” có thể bị hiểu là miễn xác minh/limiter | M01/M12 | Dùng “thiết bị nhận biết”; mọi miễn trừ phải có quyết định riêng | Chờ an toàn hệ thống |
| M01-DICT-C04 | “Delete account” có thể bị hiểu là xóa vật lý một bản ghi | M01–M12 | Dùng “yêu cầu xóa”, “chờ xóa”, “xóa dữ liệu” và “ẩn danh hóa” theo từng bước | Chờ REL-07 |

## Điều kiện duyệt M01-T001

- Mỗi thuật ngữ có đúng một nghĩa và một chủ sở hữu.
- Chủ M02/M11/M12 xác nhận các thuật ngữ giao tiếp không mâu thuẫn hợp đồng của họ.
- M01-DICT-C01–C04 được đóng hoặc chuyển thành task/finding có chủ.
- Phiên bản được ghi trong các artifact M01-T002/M01-T003 và hợp đồng liên module sử dụng nó.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa bản nháp từ phân tích M01 và quyết định đã chốt | Chưa gán |
