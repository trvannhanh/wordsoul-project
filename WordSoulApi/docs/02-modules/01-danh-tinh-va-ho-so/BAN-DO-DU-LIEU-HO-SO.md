# Bản đồ dữ liệu hồ sơ M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T003 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp — chờ M01-T001, REL-01 và REL-07 được xác nhận |
| Phạm vi | Dữ liệu do M01 sở hữu hoặc tham chiếu trực tiếp trong hồ sơ; bản đồ liên module đầy đủ thuộc M01-T033 |
| Reviewer bắt buộc | Chủ M01, riêng tư/pháp lý, an toàn hệ thống và chủ module nguồn |

## Quy ước phân loại

| Nhãn | Ý nghĩa |
|---|---|
| Công khai có lựa chọn | Chỉ hiển thị khi người dùng chủ động bật và chính sách cộng đồng cho phép |
| Riêng tư | Chủ tài khoản xem; vai trò hỗ trợ chỉ xem theo vụ việc, mục đích và phạm vi |
| Nhạy cảm | Dùng cho xác thực, an toàn hoặc quyền; không hiển thị toàn phần và mọi lượt truy cập đặc quyền phải audit |
| Bí mật | Không trả về giao diện hoặc log; chỉ hệ thống xác thực được dùng qua cơ chế bảo vệ chuyên biệt |
| Dẫn xuất/tham chiếu | M01 không phải nguồn sự thật; chỉ đọc từ module sở hữu và không cho sửa tại M01 |

## Danh mục trường nền

| Nhóm/trường logic | Mục đích | Phân loại | Nguồn sự thật | Người được xem | Người được sửa | Lưu giữ sơ bộ/hành động khi xóa | Trạng thái owner |
|---|---|---|---|---|---|---|---|
| Account ID nội bộ | Liên kết danh tính ổn định | Nhạy cảm | M01 | Chủ tài khoản ở dạng cần thiết; dịch vụ/nhân sự đúng quyền | Hệ thống tạo, không sửa | Giữ khóa thay thế không thể liên kết lại khi cần toàn vẹn; chờ REL-07 | Chờ chủ M01 |
| Thư điện tử chuẩn hóa | Đăng nhập, xác minh, phục hồi và cảnh báo | Nhạy cảm | M01 | Chủ tài khoản; hỗ trợ theo vụ việc, có che | Chủ tài khoản qua luồng xác minh; không sửa trực tiếp bởi hỗ trợ | Xóa nhận dạng trực tiếp khi xóa hoàn tất; retention ngoại lệ chờ REL-07 | Chờ chủ M01 |
| Trạng thái xác minh thư | Quyết định năng lực trước/sau xác minh | Riêng tư | M01 | Chủ tài khoản; dịch vụ cần quyết định; hỗ trợ đúng quyền | Luồng xác minh M01 | Xóa/ẩn danh cùng danh tính; audit riêng theo chính sách | Chờ M01-T006 |
| Bằng chứng thông tin bảo mật | Xác thực trực tiếp | Bí mật | M01/auth store | Không vai trò người dùng/quản trị nào xem | Chỉ luồng đặt/đổi/khôi phục; không khôi phục giá trị cũ | Xóa khi danh tính bị xóa; không ghi payload vào audit/log | Chờ M01-T004 |
| Tên hiển thị | Nhận diện trong trải nghiệm/cộng đồng | Công khai có lựa chọn | M01 | Chủ tài khoản; người khác theo lựa chọn và chính sách | Chủ tài khoản theo chính sách đổi tên | Giá trị hiện tại xóa/ẩn danh; lịch sử chống lạm dụng theo retention đã duyệt | Chờ chủ M01/M09 |
| Tham chiếu ảnh đại diện | Hiển thị hồ sơ | Công khai có lựa chọn | M01 sở hữu lựa chọn; M12 sở hữu tài sản | Theo phạm vi hiển thị hồ sơ | Chủ tài khoản qua luồng tài sản | Gỡ liên kết và điều phối xử lý tài sản theo REL-04/07 | Chờ M01-T024/M12 |
| Múi giờ đã xác nhận | Tính ranh giới ngày và gợi ý thời gian | Riêng tư | M01 | Chủ tài khoản; module cần lập lịch | Chủ tài khoản; thiết bị chỉ đề xuất | Xóa cùng lựa chọn hồ sơ | Chờ M01-T025-A |
| Giờ học mong muốn | Cá nhân hóa nhắc học | Riêng tư | M01 | Chủ tài khoản; M10 theo consent/phạm vi | Chủ tài khoản | Xóa cùng lựa chọn hồ sơ | Chờ M01-T025-A/M10 |
| Nhóm tuổi/khu vực tự khai | Chọn chính sách áp dụng | Nhạy cảm | M01 | Chủ tài khoản; dịch vụ quyết định; vai trò chuyên trách | Chủ tài khoản theo luồng có kiểm soát | Hành động chờ REL-01/07; không suy diễn ngày sinh | Chờ REL-01 |
| Bản ghi đồng ý và policy version | Chứng minh điều kiện sử dụng/riêng tư | Nhạy cảm | M01/policy registry | Chủ tài khoản ở dạng dễ hiểu; pháp lý/hỗ trợ đúng quyền | Chỉ append/thu hồi theo luồng | Theo nghĩa vụ và REL-01/07; không xóa làm mất bằng chứng bắt buộc | Chờ M01-T007 |
| Trạng thái tài khoản/hạn chế | Quyết định truy cập | Nhạy cảm | M01 | Chủ tài khoản ở mức phù hợp; dịch vụ/nhân sự đúng quyền | Luồng hệ thống hoặc quản trị có quyền | Trạng thái hiện tại xóa/ẩn danh; audit giữ theo chính sách | Chờ M01-T002/M01-T031 |
| Vai trò và phạm vi | Phân quyền | Nhạy cảm | M01/M11 theo hợp đồng quyền | Chủ tài khoản ở mức phù hợp; quản trị đúng quyền | Luồng cấp/thu hồi có kiểm soát | Thu hồi khi đóng; audit giữ theo chính sách | Chờ M01-T028/M11 |
| Liên kết danh tính ngoài | Đăng nhập qua provider | Nhạy cảm | M01 sở hữu liên kết; M12 sở hữu hợp đồng provider | Chủ tài khoản xem provider đã gắn; hỗ trợ đúng quyền | Luồng liên kết/gỡ có xác minh | Gỡ và xóa định danh provider khi xóa; không tự nối lại | Chờ M01-T013–T015/M12-T006 |
| Phiên và thiết bị đăng nhập | Duy trì/thu hồi truy cập | Nhạy cảm | M01 | Chủ tài khoản xem metadata nhận diện; an toàn/hỗ trợ đúng quyền | Hệ thống; chủ tài khoản chỉ thu hồi | Hết hạn/thu hồi theo chính sách; metadata an toàn giữ theo audit | Chờ M01-T016–T018 |
| Thiết bị nhận tin | Gửi thông báo đúng thiết bị | Nhạy cảm | M01 sở hữu gắn kết; M10 tiêu thụ | Chủ tài khoản xem metadata; M10 chỉ dùng phạm vi gửi | Luồng đăng ký/thu hồi thiết bị | Xóa token và liên kết khi đăng xuất/thu hồi/xóa | Chờ M01-T026-A/T027-A |
| Số dư/tài sản hiển thị | Hiển thị tổng quan gamification | Dẫn xuất/tham chiếu | M06 | Chủ tài khoản; vai trò đúng quyền theo M06 | Không sửa tại M01 | Không sao chép làm nguồn; hành động xóa theo M06/REL-07 | Chờ chủ M06 |
| Xếp hạng/nhóm hiển thị | Hiển thị trạng thái cộng đồng | Dẫn xuất/tham chiếu | M09 | Theo chính sách hiển thị M09 | Không sửa tại M01 | Không sao chép làm nguồn; hành động xóa theo M09/REL-07 | Chờ chủ M09 |

## Quy tắc truy cập và thay đổi

- API/giao diện hồ sơ dùng allowlist theo góc nhìn; không tuần tự hóa đối tượng lưu trữ rồi che sau.
- Trường nhạy cảm phải che theo ngữ cảnh; không dùng email làm định danh công khai hoặc tiêu chí tự ghép tài khoản.
- Nhân sự hỗ trợ chỉ tra cứu khi có vụ việc, mục đích, phạm vi và audit; quyền xem không kéo theo quyền sửa.
- Trường dẫn xuất luôn kèm module nguồn/độ mới; M01 không điều chỉnh giá trị thay chủ sở hữu.
- Xuất/xóa dùng manifest theo trường và module; phần lỗi không được báo thành công toàn phần.

## Khoảng trống phải đóng

| Mã | Khoảng trống | Nguồn gỡ |
|---|---|---|
| M01-DATA-O01 | Retention số cụ thể và legal hold của từng trường | REL-01/REL-07; reviewer pháp lý/riêng tư |
| M01-DATA-O02 | Schema/định danh vật lý và nơi lưu thực tế | Kiểm kê triển khai M01/M12; không suy ra từ tài liệu nghiệp vụ |
| M01-DATA-O03 | Quyền công khai chi tiết cho tên/ảnh/số liệu học | M01-T022-A; chủ M09/sản phẩm |
| M01-DATA-O04 | Trường tối thiểu rời hệ thống cho provider ngoài | M12-T006/M12-T042-A; REL-03 |

## Điều kiện duyệt M01-T003

- Mỗi trường có mục đích, phân loại, nguồn sự thật, quyền xem/sửa và retention sơ bộ.
- Chủ M06/M09/M10/M12 xác nhận các tham chiếu/đầu ra họ sở hữu; không có hai nguồn sự thật.
- M01-DATA-O01–O04 được đóng hoặc chuyển thành finding/task có chủ.
- Có ca kiểm tra từ chối xem/sửa, che dữ liệu, xuất/xóa phần lỗi và không lộ dữ liệu trên thiết bị dùng chung.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo bản đồ trường hồ sơ nền, phân loại và quyền xem/sửa | Chưa gán |
