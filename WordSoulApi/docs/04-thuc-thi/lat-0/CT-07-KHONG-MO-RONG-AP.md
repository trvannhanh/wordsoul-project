# CT-07 — Không mở rộng AP

| Trường | Nội dung |
|---|---|
| Task | A0-T012 |
| Trạng thái | Có hiệu lực từ 2026-08-18 |
| Quyết định | D-011 |
| Duy trì / xác nhận | WSA-7K2 / WSA-7K2 |
| Phạm vi | API, web, admin, mobile, job nền, cấu hình và dữ liệu |
| Rủi ro kiểm soát | Phụ thuộc mới làm tăng chi phí loại bỏ AP và gây nhầm lẫn giữa XP người dùng, XP thú cưng và tài sản |

## Quy tắc bắt buộc

- Không cấp, tiêu, điều chỉnh hoặc tạo phụ thuộc AP mới trong hành vi, phần thưởng, nâng cấp, xếp hạng, cấu hình, API, UI hay job nền.
- Không khởi tạo AP cho người dùng mới. Các luồng ghi hiện hữu phải từ chối thay đổi AP khác `0` cho tới khi được loại bỏ trong REL-05.
- AP chỉ được giữ dưới dạng dữ liệu lịch sử hoặc hiển thị chỉ đọc phục vụ chuyển đổi. Nếu còn hiển thị, phải ghi rõ là giá trị cũ, không thể sử dụng.
- Nâng cấp hoặc tiến hóa thú cưng dùng vật phẩm chuyên biệt, không dùng AP hay lượt gợi ý.
- XP người dùng và XP thú cưng là hai tiến trình riêng; lượt gợi ý là tài sản có thể sử dụng.
- Hệ thống chưa có tiền mềm, cửa hàng hay giao dịch giữa người dùng; không diễn giải AP thành tiền mềm.
- Không xóa số dư AP trong Giai đoạn A. Việc loại bỏ thuộc REL-05 và phải đáp ứng điều kiện dữ liệu của B-G03.

## Kiểm kê điểm chạm hiện hữu

Kiểm kê dưới đây xác nhận điểm chạm còn tồn tại trong mã nguồn tại ngày kích hoạt. Nó không xác nhận mọi luồng đang chạy ở môi trường phát hành.

| Điểm chạm | Hiện trạng quan sát | Xử lý bắt buộc trước hoặc trong REL-05 |
|---|---|---|
| `User.AP` | Số dư AP còn được lưu trên thực thể người dùng | Giữ để đối soát; xóa số dư theo kế hoạch chuyển đổi, không quy đổi hay bồi hoàn |
| `ReviewBaseAP` và luồng hoàn tất ôn tập | Cấu hình và dịch vụ còn có đường cấp AP | Đóng đường cấp; kiểm chứng hoàn tất ôn tập không đổi AP |
| Phần thưởng nhiệm vụ hằng ngày `RewardType.AP` | Kiểu thưởng và đường cấp AP còn tồn tại | Ngăn cấu hình mới, dừng cấp và xác định cách xử lý cấu hình cũ |
| Hàm cập nhật chung XP/AP | Repository còn có thể cộng AP cùng XP | Tách XP khỏi AP và từ chối AP delta khác `0` |
| Điều chỉnh AP trong quản trị người dùng | Admin/API còn nhận và hiển thị AP delta | Vô hiệu hóa điều chỉnh; giữ lịch sử quản trị cần thiết để truy vết |
| Xếp hạng, hồ sơ và dashboard | Web, admin và mobile còn đọc hoặc sắp xếp theo AP | Gắn nhãn legacy/chỉ đọc trong giai đoạn chuyển tiếp rồi loại bỏ phụ thuộc |
| DTO và giao diện thú cưng | Phản hồi nâng cấp còn mang tổng AP về client | Loại AP khỏi hợp đồng/hiển thị; xác nhận nâng cấp chỉ dùng vật phẩm chuyên biệt |

## Chuẩn bị REL-05 và B-G03

| Hạng mục | Tiêu chí bàn giao |
|---|---|
| Phạm vi dữ liệu | Có danh sách cột, cấu hình, enum, API, UI, job và báo cáo liên quan AP; mỗi điểm có chủ sở hữu và mục đích lịch sử |
| Mốc đối soát | Chụp số dư mở đầu, tổng phát sinh và số dư suy ra từ sổ bất biến; mọi chênh lệch có giải thích |
| Kịch bản an toàn | Kiểm thử cấp lặp, thu hồi, chạy bù/replay và retry không tạo số dư ngoài sổ hoặc thay đổi AP |
| Kế hoạch loại bỏ | Xóa số dư khả dụng không quy đổi, không bồi hoàn; vẫn giữ lịch sử cần thiết theo chính sách dữ liệu |
| Cutover | Có thông báo người dùng, thời điểm khóa ghi, truy vấn xác minh sau chuyển đổi và tiêu chí go/no-go |
| Rollback | Có bản ghi đầu vào bất biến, cách khôi phục kỹ thuật và thẩm quyền kích hoạt nếu đối soát thất bại |
| Xác nhận B-G03 | Mọi thay đổi tài sản giải thích được bằng sổ; không có số dư ngoài sổ hoặc phụ thuộc AP mới |

## Tự kiểm REL-05

- CT-07 chỉ đóng băng mở rộng AP và chuẩn bị đầu vào; không đánh dấu REL-05 hay B-G03 đã đạt.
- Phạm vi đóng băng bao phủ API, các client, job, cấu hình và dữ liệu.
- Các điểm chạm mã nguồn hiện hữu đã được ghi nhận để đưa vào backlog chuyển đổi.
- REL-05 chỉ được đóng sau khi kế hoạch dữ liệu, đối soát, truyền thông, cutover và rollback có bằng chứng thực thi.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / xác nhận | WSA-7K2 / WSA-7K2 |
| Nhịp kiểm tra | Mỗi thay đổi liên quan phần thưởng, số dư, cấu hình, API, UI hoặc job nền |
| Bằng chứng định kỳ | Danh sách điểm chạm AP, chủ sở hữu, mục đích lịch sử và xác nhận không có phụ thuộc mới |
| Khi vi phạm | Dừng thay đổi, ghi sai lệch vào phạm vi REL-05 và đánh giá dữ liệu/phần thưởng bị ảnh hưởng |
| Điều kiện gỡ | REL-05 hoàn thành, B-G03 đạt và có quyết định thay thế D-011 |
| Thẩm quyền gỡ | WSA-7K2 theo workflow solo |

## Lịch sử

| Ngày | Thay đổi |
|---|---|
| 2026-08-18 | Kích hoạt CT-07, ghi nhận D-011, kiểm kê điểm chạm và chuẩn bị khung REL-05/B-G03 |
