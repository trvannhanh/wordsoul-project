# Từ điển quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T001 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp chờ chủ M11 và các chủ M01/M02/M12 xác nhận |
| Chủ thuật ngữ | Chủ M11; module nguồn sở hữu nghĩa nghiệp vụ của tài nguyên/hành động |
| Quy tắc thay đổi | Không dùng thuật ngữ để tạo quyền, bypass hoặc logic nghiệp vụ chưa được quyết định |

## Thuật ngữ chuẩn

| Thuật ngữ | Định nghĩa chuẩn | Chủ sở hữu | Không được hiểu là |
|---|---|---|---|
| Vai trò quản trị | Nhóm quyền theo trách nhiệm vận hành đã xác định | M11/M01 | Một cờ Admin cho phép mọi hành động |
| Quyền | Khả năng thực hiện một hành động cụ thể trên tài nguyên và phạm vi cụ thể | M11; module nguồn xác nhận hành động | Vai trò hoặc quyền suy diễn từ giao diện |
| Phạm vi quyền | Giới hạn module, dữ liệu, nội dung, môi trường hoặc đối tượng mà quyền áp dụng | M11/module nguồn | Quyền toàn hệ thống mặc định |
| Quyền tối thiểu | Chỉ cấp các quyền cần cho trách nhiệm hiện tại; mọi quyền khác mặc định từ chối | M11/an toàn hệ thống | Cấp rộng rồi dựa vào hướng dẫn thủ công |
| Xác minh lại | Bằng chứng xác thực mới gần thời điểm thao tác nhạy cảm | M01/M11 | Phê duyệt hai người hoặc quyền khẩn cấp |
| Yêu cầu thay đổi | Bản ghi mục tiêu, phạm vi, trước/sau, tác động, kiểm chứng, lịch và rollback | M11/module nguồn | Quyền thực thi nếu người tạo không có quyền |
| Rà soát/xác nhận | Hoạt động kiểm tra artifact hoặc bằng chứng bởi vai trò phù hợp | Chủ artifact/gate | Bước duyệt hai người bắt buộc cho mọi thay đổi |
| Hiệu lực | Mốc thời gian/phạm vi mà phiên bản đã kiểm tra được module nguồn áp dụng | M11/module nguồn | Thời điểm lưu bản nháp |
| Rollback | Áp dụng quy trình quay lại phiên bản/trạng thái tương thích và kiểm chứng kết quả | M11/module nguồn | Xóa lịch sử hoặc ghi đè không truy vết |
| Cấu hình | Giá trị có chủ, kiểu, mặc định, phạm vi, độ nhạy và module tiêu thụ | M11/module nguồn | Bí mật hoặc logic nghiệp vụ vô chủ |
| Phiên bản cấu hình | Snapshot bất biến của một hoặc một bộ cấu hình có hiệu lực xác định | M11 | Chỉ số tăng không có nội dung/truy vết |
| Audit | Bằng chứng bất biến về tác nhân, quyền, hành động, đối tượng, lý do, trước/sau và kết quả | M11 | Request log hoặc activity chung |
| Activity | Sự kiện hoạt động nghiệp vụ phục vụ lịch sử/trải nghiệm theo chính sách module | Module nguồn | Bằng chứng đủ cho thao tác nhạy cảm |
| Log vận hành | Metadata chẩn đoán sức khỏe/lỗi đã qua allowlist và redaction | M11/M12 | Payload thô, bí mật hoặc audit |
| Vụ việc hỗ trợ | Hồ sơ có chủ, lý do, phạm vi truy cập, SLA, bằng chứng và kết quả | M11 | Quyền sửa trực tiếp dữ liệu nguồn |
| Chỉ số | Giá trị có tên, công thức, nguồn, cửa sổ, múi giờ, độ mới và chủ | M11/module nguồn | Con số dashboard không định nghĩa |
| Health | Tín hiệu sống/sẵn sàng/correctness/freshness/dependency có chủ và ngưỡng | M11/M12 | Ping cố định hoặc luôn xanh |
| Cảnh báo | Tín hiệu vượt ngưỡng có mức, chủ, chống lặp, escalation và playbook | M11/vận hành | Một log lỗi không có người xử lý |
| Công việc nền | Đơn vị chạy có chủ, input, lịch/trigger, timeout, khóa, idempotency và kết quả | Module nguồn; M11 đăng ký | Script có thể chạy lại tùy ý |
| Đối soát | So sánh nguồn–đích bằng khóa/quy tắc đã định để phát hiện và xử lý sai lệch | Module nguồn/M11 | Ghi đè lịch sử để làm số liệu khớp |
| Bảo trì | Chế độ hạn chế đúng năng lực/phạm vi nhằm bảo toàn an toàn/toàn vẹn | M11/vận hành | Quyền khẩn cấp hoặc tắt toàn hệ thống mặc định |
| Kill switch | Cơ chế đã đăng ký để dừng năng lực gây hại bằng quyền cố định đã cấp trước | M11/module nguồn | Đường cấp quyền khẩn cấp/bypass audit |
| Sự cố | Sự kiện có tác động cần phân mức, chỉ huy, khống chế, khôi phục, truyền thông và hậu kiểm | M11/vận hành | Mọi lỗi kỹ thuật đơn lẻ |

## Thuật ngữ bị cấm hoặc cần diễn giải lại

| Mã | Thuật ngữ/cách dùng | Quy tắc chuẩn | Nguồn quyết định | Trạng thái |
|---|---|---|---|---|
| M11-DICT-C01 | Quyền tạm thời/đặc quyền khẩn cấp | Không được hỗ trợ trong Giai đoạn A; M11-T006-A kiểm chứng không có đường này | M11-D004 | Khóa theo quyết định |
| M11-DICT-C02 | Duyệt hai người là kiểm soát bắt buộc | Không có bước duyệt hai người bắt buộc; dùng quyền tối thiểu, xác minh lại, lý do và audit | M11-D005–D006 | Khóa theo quyết định |
| M11-DICT-C03 | Người phê duyệt là vai trò bắt buộc thường trực | Chỉ dùng “reviewer/người xác nhận” cho artifact/gate hoặc authority được giao; không tự tạo workflow hai người | M11-D005–D007 | Chờ rà soát tài liệu cũ |
| M11-DICT-C04 | Admin/SuperAdmin đủ mô tả quyền | Mọi hành động phải ánh xạ quyền và phạm vi; vai trò rộng là ngoại lệ | M11-D001–D003 | Chờ ma trận M11-T002–T004 |
| M11-DICT-C05 | Log, activity và audit thay thế nhau | Ba loại có mục đích, trường, quyền và retention riêng | M11-D018–D021 | Chờ M11-T031–T035 |

## Điều kiện duyệt M11-T001

- Các khái niệm quyền, thay đổi, audit, log, sự cố và hỗ trợ chỉ có một nghĩa.
- M01/M02/M12 xác nhận thuật ngữ giao tiếp; module nguồn vẫn sở hữu luật nghiệp vụ.
- M11-DICT-C01–C05 được phản ánh trong mọi tài liệu đang hoạt động hoặc có task sửa rõ ràng.
- Phiên bản từ điển được tham chiếu bởi M11-T002, M11-T012 và M11-T022.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa bản nháp từ quyết định M11 và ghi các thuật ngữ lỗi thời cần loại | Chưa gán |
