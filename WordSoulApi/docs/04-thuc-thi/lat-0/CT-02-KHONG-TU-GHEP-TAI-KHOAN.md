# CT-02 — Không tự ghép tài khoản theo thư điện tử

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T007 |
| Trạng thái | Bản nháp sẵn sàng — chưa kích hoạt |
| Chủ trì / xác nhận | Chủ M01 / An toàn hệ thống |
| Cá nhân thực tế | Chưa gán / Chưa gán |
| Rủi ro kiểm soát | Ghép nhầm hoặc chiếm tài khoản khi danh tính ngoài có thư điện tử trùng |

## Quy tắc tạm thời

- Thư điện tử trùng không phải bằng chứng đủ để liên kết hai danh tính.
- Tài khoản ngoài chưa liên kết phải đi qua hành trình xác minh lại rõ ràng.
- Xung đột phải được từ chối an toàn; không âm thầm tạo, ghép hoặc chuyển quyền.
- Không cho ngoại lệ thủ công nếu thiếu bằng chứng chủ thể và audit.

## Phạm vi áp dụng

| Hành trình | Trường hợp phải từ chối/giữ tách | Hành vi được phép |
|---|---|---|
| Đăng nhập ngoài lần đầu | Provider trả email trùng tài khoản nội bộ nhưng chưa có liên kết đã xác nhận | Yêu cầu đăng nhập lại tài khoản nội bộ hoặc quy trình khôi phục được kiểm soát |
| Liên kết provider mới | Người dùng chưa xác minh lại phiên/phương thức hiện có | Không tạo liên kết; trả kết quả trung tính và có audit tối thiểu |
| Danh tính ngoài đã gắn tài khoản khác | Có xung đột định danh provider | Từ chối, không chuyển liên kết; mở vụ việc hỗ trợ khi đủ điều kiện |
| Gỡ phương thức đăng nhập | Đây là phương thức cuối và chưa có đường phục hồi | Giữ liên kết đến khi có phương thức thay thế được xác minh |
| Thao tác hỗ trợ thủ công | Thiếu bằng chứng chủ thể, lý do/vụ việc hoặc audit | Từ chối thao tác; không dùng email trùng làm ngoại lệ |

## Kịch bản kiểm tra và bằng chứng dự kiến

| Evidence ID | Kịch bản | Kết quả yêu cầu | Trạng thái |
|---|---|---|---|
| A0-E021 | Ma trận toàn bộ entry point đăng nhập/liên kết/gỡ/hỗ trợ và kiểm tra provider trả email trùng | Không entry point nào tự ghép hoặc tạo quyền dựa riêng vào email | Chưa tạo |
| A0-E022 | Ca từ chối, xác minh lại, gửi lặp, xung đột và audit bằng dữ liệu giả | Không tạo trùng/chuyển quyền; kết quả trung tính; audit không chứa token/PII thật | Chưa tạo |

Các Evidence ID chỉ được đăng ký khi có kết quả thực tế và reviewer; tài liệu chính sách hoặc ảnh chụp riêng lẻ không đủ.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / xác nhận | Chưa gán / Chưa gán |
| Bằng chứng định kỳ | Danh sách hành trình đăng nhập/liên kết; kịch bản thư trùng; bằng chứng từ chối và audit |
| Nhịp kiểm tra | Chưa xác định; người điều phối đặt sau khi gán chủ trì/reviewer |
| Khi vi phạm | Thu hồi phiên liên quan, khóa liên kết nghi vấn, điều tra phạm vi và hỗ trợ khôi phục |
| Điều kiện gỡ | M01-T014–M01-T015 và M12-T007–M12-T009 đạt |
| Thẩm quyền gỡ | Chủ M01 và an toàn hệ thống |

## Điều kiện kích hoạt và gỡ

- Chuyển A0-T007 sang `Đang thực hiện` khi có cá nhân duy trì/chủ M01, reviewer an toàn và danh sách entry point ban đầu.
- Chỉ coi CT-02 có hiệu lực khi mọi entry point đã được thông báo phạm vi cấm và có cách kiểm tra định kỳ.
- Chỉ chuyển `Chờ xác nhận` khi A0-E021/A0-E022 có artifact thật và được đăng ký.
- Chỉ gỡ sau khi M01-T014–T015, M12-T007–T009 và ca G01-C02/C05 đạt; chủ M01 và an toàn hệ thống xác nhận.

## Phụ thuộc và lịch sử

- Đọc trực tiếp đầu ra M12-T006–T010 như phụ thuộc nội bộ; CT-02 chỉ được dùng khi chính sách và entry point đã được reviewer xác nhận.
- Finding hiện trạng tự ghép email là nghiêm trọng; kiểm soát chưa được coi là đạt trước khi có bằng chứng runtime/re-test.

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-15 | WSA-7K2 | Chuẩn bị phạm vi entry point, A0-E021/A0-E022 và tiêu chí kích hoạt/gỡ | Không có bằng chứng runtime; chờ gán người |
