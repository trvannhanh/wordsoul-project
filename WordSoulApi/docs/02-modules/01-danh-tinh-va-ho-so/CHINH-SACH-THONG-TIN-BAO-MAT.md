# Chính sách thông tin bảo mật M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T004 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp — chờ M01-T001, REL-02 và reviewer an toàn hệ thống xác nhận |
| Cơ sở đã chốt | M01-D004: tối thiểu 12 ký tự, cho phép cụm từ dài/trình quản lý mật khẩu, chặn giá trị phổ biến/đã lộ, không ép đổi định kỳ nếu không có rủi ro |
| Phạm vi | Thông tin bảo mật đăng nhập trực tiếp; token phiên/provider/bí mật hệ thống thuộc hợp đồng riêng |

## Chính sách nền

| Chủ đề | Yêu cầu | Hành vi từ chối/ngoại lệ |
|---|---|---|
| Độ dài | Tối thiểu 12 ký tự sau khi nhận đúng chuỗi người dùng nhập; giới hạn tối đa kỹ thuật phải đủ cho cụm từ dài và được công bố | Từ chối dưới ngưỡng; giới hạn tối đa cụ thể chờ triển khai/an toàn xác nhận |
| Tập ký tự | Cho phép khoảng trắng và Unicode hợp lệ; không bắt buộc tổ hợp chữ hoa/thường/số/ký hiệu | Chỉ từ chối dữ liệu không thể xử lý an toàn; không âm thầm cắt khoảng trắng hay ký tự |
| Danh sách phổ biến/đã lộ | Kiểm tra bằng cơ chế không làm lộ giá trị thô; từ chối giá trị có rủi ro đã xác nhận | Khi nguồn kiểm tra lỗi, hành vi fail-closed/degraded phải được reviewer an toàn và M12 chốt trước phát hành |
| Tái sử dụng | Không chấp nhận chính giá trị hiện tại; chính sách lịch sử phải cân bằng rủi ro và lưu trữ an toàn | Không tự đặt số lượng lịch sử; chờ M01-SEC-O01 |
| Đổi định kỳ | Không ép đổi theo lịch chỉ vì thời gian | Buộc đổi/đặt lại khi có bằng chứng rủi ro, khôi phục, quyết định an toàn hoặc thay đổi bảo vệ tài khoản |
| Lưu trữ | Chỉ lưu verifier dẫn xuất bằng thuật toán/tham số được an toàn hệ thống phê duyệt; salt riêng; không lưu/ghi log giá trị thô hoặc dạng có thể giải mã | Tham số/thuật toán cụ thể là chuẩn kỹ thuật có phiên bản, không tự chốt trong tài liệu này |
| So sánh và truyền | Chỉ qua kênh bảo vệ; không đưa vào URL, analytics, telemetry, audit payload hoặc thông báo lỗi | Lỗi ghi bằng mã/kết quả, không phản chiếu dữ liệu nhập |
| Thay đổi chủ động | Xác minh lại gần thời điểm thực hiện; kiểm tra chính sách mới; cảnh báo chủ tài khoản | Theo M01-D009: giữ thiết bị hiện tại khi phiên an toàn, thu hồi phiên khác |
| Đặt lại/chiếm quyền | Bằng chứng một lần, có hạn; kiểm tra chính sách mới; thu hồi mọi phiên; cảnh báo | Không tiết lộ tài khoản tồn tại và không dùng câu hỏi bí mật làm bằng chứng mặc định |
| Quản trị/hỗ trợ | Không được xem, đặt giá trị tùy ý hoặc yêu cầu người dùng cung cấp giá trị hiện tại | Chỉ khởi tạo quy trình khôi phục có vụ việc/audit và không nhận bí mật qua kênh hỗ trợ |

## Phản hồi người dùng và audit

- Phản hồi tạo/đổi giải thích quy tắc chưa đạt nhưng không tiết lộ danh sách kiểm tra, trạng thái tài khoản hoặc dữ liệu bí mật.
- Phản hồi yêu cầu khôi phục là trung tính cho tài khoản tồn tại/không tồn tại.
- Audit chỉ ghi loại hành động, actor phù hợp, account reference bảo vệ, policy version, kết quả, reason code và correlation; không ghi giá trị, hash/verifier hoặc bằng chứng một lần.
- Lỗi phụ thuộc không được hạ tiêu chuẩn âm thầm; mọi degraded mode có chỉ số, cảnh báo và thời hạn.

## Ngoại lệ

Ngoại lệ chỉ hợp lệ khi có chủ, lý do, phạm vi tài khoản/hành trình, đánh giá rủi ro, kiểm soát bù trừ, thời hạn và bằng chứng chấp nhận của reviewer an toàn. Ngoại lệ không được cho phép lưu/ghi log bí mật thô, bỏ xác minh chủ thể hoặc tự cấp quyền quản trị.

## Điểm chưa được tự chốt

| Mã | Điểm chờ | Nguồn gỡ |
|---|---|---|
| M01-SEC-O01 | Có lưu lịch sử chống tái sử dụng hay chỉ chặn giá trị hiện tại; nếu có thì độ sâu/thời hạn | Reviewer an toàn và riêng tư |
| M01-SEC-O02 | Giới hạn tối đa đầu vào, thuật toán dẫn xuất và tham số phiên bản hiện hành | Chuẩn kỹ thuật/an toàn hệ thống |
| M01-SEC-O03 | Hành vi khi nguồn kiểm tra giá trị đã lộ không khả dụng | M12 capability/fail-closed contract; REL-03 |
| M01-SEC-O04 | Tiêu chí bằng chứng rủi ro bắt buộc đổi và thời hạn hoàn tất | M01-T011/M01-T039; an toàn hệ thống |

## Điều kiện duyệt M01-T004

- M01-SEC-O01–O04 được đóng hoặc chuyển thành finding/task có chủ.
- Có kiểm thử đo được cho ngưỡng, Unicode/khoảng trắng, danh sách đã lộ, gửi lặp, lỗi phụ thuộc và không rò rỉ log.
- Luồng đăng ký, đổi, đặt lại và hỗ trợ dùng cùng policy version; thay đổi policy có kế hoạch tương thích.
- Reviewer an toàn hệ thống xác nhận; WSA-7K2 không tự thay thẩm quyền này.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa chính sách từ M01-D004 và khóa các giá trị cần reviewer kỹ thuật | Chưa gán |
