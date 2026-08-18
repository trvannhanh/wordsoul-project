# CT-02 — Không tự ghép tài khoản theo thư điện tử

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T007 |
| Trạng thái | Có hiệu lực từ 2026-08-18; triển khai kỹ thuật và bằng chứng runtime chờ các task liên quan |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Phạm vi áp dụng | Mọi entry point đăng ký, đăng nhập ngoài, liên kết/gỡ provider, khôi phục và hỗ trợ thủ công trong Giai đoạn A |
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

Các Evidence ID chỉ được đăng ký khi có kết quả thực tế kiểm tra được; tài liệu chính sách hoặc ảnh chụp riêng lẻ không đủ. Dữ liệu thử phải là dữ liệu giả và bằng chứng không được chứa token/PII thật.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh sách hành trình đăng nhập/liên kết; kịch bản thư trùng; bằng chứng từ chối và audit |
| Nhịp kiểm tra | Khi thêm/sửa entry point danh tính; trước A-G01; và sau sự cố ghép/liên kết nghi vấn |
| Khi vi phạm | Thu hồi phiên liên quan, khóa liên kết nghi vấn, điều tra phạm vi và hỗ trợ khôi phục |
| Điều kiện gỡ | M01-T014–M01-T015 và M12-T007–M12-T009 đạt |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng cập nhật task/quyết định phù hợp sau khi điều kiện gỡ và A-G01 đạt |

## Kết quả kích hoạt và điều kiện gỡ

- WSA-7K2 là người duy trì và tự nghiệm thu theo D-001; ma trận entry point ban đầu và phạm vi cấm đã được ghi nhận trong hồ sơ.
- CT-02 có hiệu lực ở mức guardrail dự án cho mọi entry point nêu trên; không diễn giải điều này thành bằng chứng hệ thống hiện tại đã đạt.
- A0-E021/A0-E022 chỉ được đăng ký khi có artifact runtime thật; G01-C02 và G01-C05 phải chứng minh từ chối an toàn bằng dữ liệu giả.
- Chỉ gỡ sau khi M01-T014–T015, M12-T007–T009 và G01-C02/C05 đạt, không còn finding nghiêm trọng/rất cao liên quan và WSA-7K2 ghi nhận kết luận tự kiểm.

## Phụ thuộc và lịch sử

- Đọc trực tiếp đầu ra M12-T006–T010 như phụ thuộc nội bộ; CT-02 dùng các artifact này cho tự kiểm runtime, không yêu cầu reviewer độc lập.
- Finding hiện trạng tự ghép email là nghiêm trọng; kiểm soát chưa được coi là đạt trước khi có bằng chứng runtime/re-test.

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-15 | WSA-7K2 | Chuẩn bị phạm vi entry point, A0-E021/A0-E022 và tiêu chí kích hoạt/gỡ | Không có bằng chứng runtime; chờ gán người |
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-02 theo workflow một người; chốt chủ trì, phạm vi, nhịp kiểm tra và tự kiểm G01-C02/C05 | D-001, D-008; chưa có bằng chứng runtime |
