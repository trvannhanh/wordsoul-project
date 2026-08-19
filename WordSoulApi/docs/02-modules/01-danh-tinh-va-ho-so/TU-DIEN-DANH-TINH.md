# Từ điển danh tính M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T001 |
| Phiên bản | 1.0 |
| Trạng thái | Chuẩn thuật ngữ có hiệu lực từ 2026-08-19 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Phạm vi áp dụng | M01 và mọi hợp đồng API, dữ liệu, UI, audit hoặc tích hợp tiêu thụ danh tính WordSoul |
| Quy tắc thay đổi | Đổi nghĩa phải tăng phiên bản, ghi tác động và xử lý xung đột trước khi module tiêu thụ áp dụng |

## Quy tắc sử dụng

- API là nguồn sự thật cho danh tính, trạng thái tài khoản, vai trò, quyền và quan hệ sở hữu. Client không tự suy ra hoặc nâng quyền từ trường hiển thị.
- Trong hợp đồng mới, dùng thuật ngữ chuẩn và định danh bất biến. Tên trường hiện hữu như `Username`, `IsActive` hoặc `RefreshToken` là nhãn triển khai cũ, không được dùng để mở rộng nghĩa nghiệp vụ.
- Email, tên hiển thị và định danh provider không thay thế định danh người dùng nội bộ.
- Trạng thái tài khoản, trạng thái phiên và trạng thái thiết bị là ba miền riêng; một cờ `IsActive` không đại diện đầy đủ cho cả ba.
- Không ghi thông tin đăng nhập, access token, refresh credential hoặc bằng chứng xác minh vào log, audit hay tài liệu.

## Thuật ngữ chuẩn

| Thuật ngữ | Định nghĩa chuẩn | Chủ ngữ nghĩa | Không được hiểu là |
|---|---|---|---|
| Định danh người dùng | Khóa nội bộ bất biến và không tái sử dụng, đại diện cho một chủ thể người dùng trong WordSoul | M01 | Email, tên hiển thị, tên đăng nhập hoặc định danh provider |
| Danh tính người dùng | Tập quan hệ và bằng chứng đã xác minh xoay quanh một định danh người dùng để WordSoul nhận biết cùng một chủ thể | M01 | Một hồ sơ hiển thị hoặc một phương thức đăng nhập đơn lẻ |
| Tài khoản | Vòng đời truy cập gắn với đúng một danh tính người dùng, gồm trạng thái, phương thức đăng nhập và chính sách áp dụng | M01 | Hồ sơ, vai trò hoặc một phiên truy cập |
| Hồ sơ | Tập thuộc tính người dùng được phép xem, sửa hoặc công khai; mỗi trường có nguồn sự thật và quyền riêng | M01 | Nguồn sự thật của tiến độ, tài sản, quyền hoặc xếp hạng |
| Tên hiển thị | Thuộc tính hồ sơ dùng để trình bày cho người khác, có thể trùng và không dùng để xác thực | M01 | Tên đăng nhập duy nhất hoặc bằng chứng sở hữu tài khoản |
| Định danh đăng nhập trực tiếp | Email đã chuẩn hóa dùng để tìm phương thức đăng nhập trực tiếp theo M01-D003 | M01 | Tên hiển thị, định danh người dùng công khai hoặc bằng chứng đã xác thực |
| Thông tin đăng nhập | Bí mật hoặc bằng chứng được bảo vệ dùng để chứng minh quyền kiểm soát một phương thức đăng nhập | M01 | Dữ liệu được trả về client, ghi vào audit hoặc hiển thị cho quản trị viên |
| Xác thực | Quá trình kiểm tra bằng chứng để xác định chủ thể đang yêu cầu truy cập | M01 | Quyết định chủ thể được phép thực hiện hành động nào |
| Phân quyền | Quyết định một chủ thể đã xác thực có được thực hiện hành động trên tài nguyên cụ thể trong trạng thái hiện tại hay không | M01 | Chỉ kiểm tra đã đăng nhập hoặc chỉ kiểm tra tên vai trò |
| Vai trò | Nhóm trách nhiệm nghiệp vụ dùng để gom các quyền tối thiểu đã duyệt | M01 | Quyền toàn cục mặc định hoặc nguồn thay thế cho kiểm tra ownership |
| Quyền | Năng lực nguyên tử cho phép một hành động có phạm vi; được gán qua chính sách/ma trận vai trò và vẫn chịu kiểm tra trạng thái, ownership | M01 | Tên màn hình, cờ admin chung hoặc khả năng tự cấp quyền |
| Danh tính ngoài | Cặp định danh provider và subject bất biến do nhà cung cấp bên ngoài phát hành, nhận qua hợp đồng tích hợp | M12 | Tài khoản WordSoul, email hoặc tên do provider trả về |
| Liên kết danh tính | Quan hệ được xác nhận giữa một tài khoản WordSoul và một danh tính ngoài sau khi chứng minh quyền sở hữu | M01 | Tự ghép tài khoản vì email trùng |
| Xung đột danh tính | Trường hợp bằng chứng đăng nhập hoặc định danh ngoài trùng/đã liên kết và chưa thể quyết định an toàn | M01 | Lý do tự động hợp nhất hoặc ghi đè quan hệ hiện có |
| Xác minh thư điện tử | Chứng minh quyền kiểm soát địa chỉ thư bằng bằng chứng một lần, có hạn và chống dùng lại | M01 | Xác minh danh tính pháp lý hoặc quyền liên kết tài khoản hiện có |
| Đồng ý | Bản ghi lựa chọn có loại, phiên bản chính sách, phạm vi, thời điểm, nguồn và trạng thái rút lại | M01 | Một cờ không phiên bản hoặc kết luận tuân thủ pháp lý |
| Phiên truy cập | Quyền truy cập có hạn được phát hành sau xác thực, gắn với tài khoản và ngữ cảnh thiết bị/phạm vi, có thể thu hồi | M01 | Trạng thái tài khoản, token thô hoặc quyền truy cập vĩnh viễn |
| Họ phiên | Nhóm phiên truy cập và refresh credential có cùng nguồn phát hành để phát hiện dùng lại và thu hồi đúng phạm vi | M01 | Một refresh token dùng chung cho mọi thiết bị |
| Access token | Bằng chứng truy cập sống ngắn đại diện cho một phiên/phạm vi tại thời điểm phát hành | M01 | Phiên bền vững, nguồn sự thật trạng thái hiện tại hoặc dữ liệu được phép ghi log |
| Refresh credential | Bí mật sống dài hơn dùng để luân chuyển phiên theo chính sách, phải chống dùng lại và có thể thu hồi | M01 | Access token, mật khẩu hoặc giá trị lưu/so sánh ở dạng rõ |
| Thiết bị nhận biết | Bản ghi thiết bị có định danh, chủ tài khoản, trạng thái và lịch sử hoạt động để người dùng nhận biết/quản lý | M01 | Thiết bị luôn tin cậy, phiên truy cập hoặc ngoại lệ khỏi xác minh/limiter |
| Trạng thái tài khoản | Trạng thái nghiệp vụ quyết định khả năng truy cập của tài khoản và tác động bắt buộc lên phiên | M01 | Cờ hoạt động dùng chung cho nội dung, thiết bị hoặc tài nguyên khác |
| Chờ xác minh | Trạng thái tài khoản chưa đáp ứng xác minh bắt buộc và chỉ có quyền giới hạn đã công bố | M01 | Tài khoản hoạt động đầy đủ |
| Hoạt động | Trạng thái tài khoản đủ điều kiện truy cập theo vai trò, quyền, ownership và các hạn chế hiện hành | M01 | Được phép mọi hành động hoặc mọi phiên hiện hữu vẫn hợp lệ |
| Hạn chế theo chức năng | Giới hạn có lý do, phạm vi, thời hạn và đường khiếu nại áp dụng cho một số năng lực | M01 | Khóa toàn bộ, xóa tài khoản hoặc thay đổi vai trò |
| Khóa tài khoản | Trạng thái từ chối truy cập theo chính sách và thu hồi/chặn mọi phiên thuộc phạm vi | M01 | Chỉ đổi cờ hiển thị trong quản trị |
| Chờ xóa | Trạng thái sau khi yêu cầu xóa được xác minh, trong thời gian chờ hoặc có thể hủy theo chính sách | M01 | Đã xóa vật lý mọi dữ liệu |
| Xóa dữ liệu | Loại bỏ dữ liệu thuộc diện xóa theo ma trận M01 điều phối, do từng module dữ liệu thực thi và ghi kết quả đối soát | M01 | Xóa bản ghi người dùng chính là hoàn tất toàn bộ |
| Ẩn danh hóa | Biến đổi theo chính sách M01 điều phối để dữ liệu lịch sử không thể liên kết lại với danh tính cá nhân | M01 | Chỉ che email hoặc tên ở giao diện |
| Đăng ký lại | Tạo danh tính mới sau khi quy trình xóa hoàn tất và điều kiện tái sử dụng được đáp ứng | M01 | Khôi phục hoặc nối lại dữ liệu danh tính cũ |

## Hợp đồng thuật ngữ liên module

| Module tiêu thụ | Được dùng | Không được suy ra hoặc sở hữu thay M01 |
|---|---|---|
| M02 — Học liệu | Định danh người dùng cho người tạo/chủ sở hữu và trạng thái đủ điều kiện thao tác | Email, thông tin đăng nhập, trạng thái phiên hoặc quyền chỉ từ tên hiển thị |
| M11 — Quản trị/vận hành | Định danh người dùng, trạng thái tài khoản, vai trò/quyền và sự kiện đã che dữ liệu | Tự tạo danh tính, tự cấp quyền, đọc bí mật hoặc dùng audit làm nguồn trạng thái hiện tại |
| M12 — Tích hợp nền tảng | Danh tính ngoài, kênh xác minh, hợp đồng token/provider và định danh tương quan tối thiểu | Tự liên kết tài khoản từ email, biến subject provider thành định danh người dùng hoặc quyết định quyền nghiệp vụ |

## Sổ xung đột thuật ngữ đã xử lý

| Mã | Xung đột quan sát | Cách khóa ở phiên bản 1.0 | Trạng thái / việc triển khai tiếp theo |
|---|---|---|---|
| M01-DICT-C01 | `Username` đang được dùng vừa để đăng nhập vừa để hiển thị | Đăng nhập trực tiếp dùng email; UI dùng “tên hiển thị”; định danh nội bộ dùng “định danh người dùng” | Đã xử lý ngữ nghĩa; hợp đồng/mã cũ được chuyển đổi trong các task M01 sau |
| M01-DICT-C02 | “User”, “account” và “profile” được dùng thay nhau | Tách thành định danh/danh tính người dùng, tài khoản và hồ sơ theo các định nghĩa riêng | Đã xử lý; mọi hợp đồng mới phải chọn đúng khái niệm |
| M01-DICT-C03 | “Trusted device” có thể bị hiểu là miễn xác minh hoặc limiter | Chỉ dùng “thiết bị nhận biết”; ngoại lệ xác minh phải là chính sách riêng, có phạm vi và kiểm toán | Đã xử lý; không có miễn trừ ngầm |
| M01-DICT-C04 | “Delete account” có thể bị hiểu là xóa một bản ghi | Tách yêu cầu/chờ xóa, xóa dữ liệu và ẩn danh hóa; đăng ký lại luôn tạo danh tính mới | Đã xử lý ngữ nghĩa; chi tiết dữ liệu chờ REL-07 và task xóa |
| M01-DICT-C05 | `IsActive` có thể được dùng như trạng thái chung cho tài khoản, thiết bị hoặc tài nguyên | Dùng “trạng thái tài khoản” cho M01; trạng thái thiết bị/tài nguyên phải có tên miền riêng | Đã xử lý ngữ nghĩa; mô hình trạng thái được cụ thể hóa ở M01-T002 |
| M01-DICT-C06 | Một `RefreshToken` trên người dùng có thể bị hiểu là toàn bộ phiên | Phân biệt phiên truy cập, họ phiên, access token và refresh credential; không dùng token thô làm trạng thái | Đã xử lý ngữ nghĩa; vòng đời/lưu trữ được cụ thể hóa ở M01-T016–T018 |

## Tự kiểm M01-T001 và A-G01

- 30 thuật ngữ có một định nghĩa và đúng một chủ ngữ nghĩa; ranh giới M02/M11/M12 đã được chốt.
- Sáu xung đột tên gọi đã có cách dùng chuẩn và nơi xử lý triển khai tiếp theo; không còn xung đột thuật ngữ vô chủ.
- Từ điển hỗ trợ các biên A-G01 về không tự liên kết do email trùng, phân biệt trạng thái tài khoản/phiên và thu hồi theo phạm vi.
- Tài liệu này chỉ là baseline ngữ nghĩa: không kết luận A-G01 đạt và không thay thế các case runtime G01-C01–G01-C10, REL-01 hoặc REL-07.
- M01-T002, M01-T003 và các hợp đồng liên module phải tham chiếu phiên bản 1.0 hoặc phiên bản thay thế đã được quyết định.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa bản nháp từ phân tích M01 và quyết định đã chốt | Chưa gán |
| 2026-08-19 | 1.0 | Chốt chủ ngữ nghĩa, ranh giới liên module và sáu xung đột; tự kiểm đầu vào A-G01 | WSA-7K2 |
