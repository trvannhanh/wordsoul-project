# Vòng đời tài khoản M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T002 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp — chờ M01-T001 và REL-01 được xác nhận |
| Chủ nghiệp vụ | Chủ M01 |
| Reviewer bắt buộc | An toàn hệ thống, sản phẩm; pháp lý/riêng tư cho nhánh tuổi–đồng ý–xóa |

## Nguyên tắc

- Trạng thái tài khoản là nguồn quyết định quyền truy cập; giao diện, token còn hạn hoặc provider ngoài không được bỏ qua trạng thái.
- Mọi chuyển trạng thái có tác nhân, lý do, thời điểm, nguồn yêu cầu và audit phù hợp.
- Thay đổi trạng thái phải xác định tác động tới access session, refresh session, thiết bị và năng lực liên module.
- Các nhánh tuổi/đồng ý giữ điều kiện `Chờ REL-01`; số ngày chờ xóa là cấu hình chưa được tự đặt.

## Ma trận trạng thái

| Trạng thái | Điều kiện vào | Quyền tối đa | Tác động phiên | Điều kiện thoát | Chủ quyết định |
|---|---|---|---|---|---|
| Chờ xác minh thư | Đăng ký trực tiếp hợp lệ nhưng chưa hoàn tất bằng chứng thư | Xem học liệu và học giới hạn; khóa xuất bản, xã hội, PvP, thông báo ngoài ứng dụng và thao tác nhạy cảm | Chỉ cấp quyền giới hạn; refresh không được nâng quyền | Xác minh hợp lệ hoặc yêu cầu bị hết hạn/hủy theo chính sách | M01 |
| Chờ điều kiện tuổi/đồng ý | Thị trường/nhóm tuổi yêu cầu điều kiện chưa có hoặc đã rút | Chưa chốt; mặc định không mở năng lực nhạy cảm | Không cấp/gia hạn quyền vượt phạm vi đã xác nhận | Điều kiện REL-01 được đáp ứng hoặc tài khoản chuyển trạng thái an toàn khác | Sản phẩm/pháp lý/M01 |
| Hoạt động | Đã đáp ứng xác minh và điều kiện bắt buộc hiện hành | Theo vai trò và hạn chế đang áp dụng | Phiên hợp lệ được duy trì theo chính sách; thao tác nhạy cảm xác minh lại | Bị hạn chế/khóa, yêu cầu xóa hoặc đóng tài khoản | M01/chủ kiểm soát liên quan |
| Hạn chế theo chức năng | Có quyết định hạn chế với lý do, phạm vi và thời hạn | Chỉ các năng lực không bị hạn chế | Thu hồi/chặn quyền của năng lực bị ảnh hưởng; phiên khác không tự mở rộng | Hết hạn hoặc quyết định mở lại/khóa rộng hơn | M01/chủ module |
| Tạm khóa do rủi ro | Có tín hiệu chiếm quyền/rủi ro theo chính sách | Chỉ đường khôi phục/hỗ trợ an toàn | Chặn cấp mới; thu hồi phạm vi phiên theo chính sách sự cố | Xác minh khôi phục đạt hoặc chuyển khóa quản trị | M01/an toàn hệ thống |
| Khóa quản trị | Người có quyền áp dụng khóa với lý do/phạm vi/thời hạn | Chỉ đường khiếu nại/hỗ trợ được phép | Mọi đường cấp/gia hạn bị chặn; phiên hiện có bị thu hồi/chặn | Quyết định mở lại có audit hoặc chuyển chờ xóa | M01/M11 theo quyền |
| Ngừng hoạt động lâu dài | Chính sách/vụ việc xác định tài khoản không còn được sử dụng | Không đăng nhập; chỉ xử lý dữ liệu/hỗ trợ được phép | Thu hồi/chặn mọi phiên | Mở lại có thẩm quyền hoặc chuyển chờ xóa | M01/M11 |
| Chờ xóa | Yêu cầu qua hỗ trợ đã xác minh và bắt đầu thời gian chờ | Không thực hiện hành động nghiệp vụ mới; chỉ xem trạng thái/hủy nếu chính sách cho phép | Thu hồi/chặn phiên đúng thời điểm đã duyệt | Hủy hợp lệ hoặc điều phối xóa/ẩn danh hoàn tất | M01/riêng tư |
| Đã xóa/ẩn danh | Ma trận dữ liệu đã chạy và đối soát đạt | Không có quyền truy cập | Không còn phiên/phương thức đăng nhập hợp lệ | Không mở lại; đăng ký lại tạo danh tính mới | Các chủ dữ liệu; M01 điều phối |

## Chuyển trạng thái hợp lệ

| Từ | Sang | Trigger/bằng chứng tối thiểu | Từ chối khi | Audit bắt buộc |
|---|---|---|---|---|
| Chờ xác minh thư | Hoạt động | Bằng chứng một lần còn hạn; điều kiện REL-01 đã đạt nếu áp dụng | Bằng chứng hết hạn/dùng lại hoặc thiếu đồng ý bắt buộc | Actor, evidence metadata, trước/sau, kết quả |
| Chờ xác minh thư | Chờ điều kiện tuổi/đồng ý | Xác minh thư đạt nhưng điều kiện REL-01 chưa đạt | Không xác định được chính sách áp dụng | Lý do và policy version |
| Hoạt động | Hạn chế theo chức năng | Quyết định có phạm vi, lý do, thời hạn | Thiếu quyền/lý do hoặc không ghi được audit | Trước/sau, phạm vi, thời hạn |
| Hoạt động hoặc Hạn chế theo chức năng | Tạm khóa do rủi ro | Tín hiệu/rule/vụ việc theo chính sách | Không có nguồn hoặc correlation | Tín hiệu, tác nhân, phạm vi phiên |
| Hoạt động, Hạn chế theo chức năng hoặc Tạm khóa do rủi ro | Khóa quản trị | Người có quyền, lý do và kiểm tra bảo vệ quản trị cao nhất | Tự khóa trái chính sách, thiếu quyền hoặc audit lỗi | Actor/role, reason, before/after, revoke result |
| Khóa quản trị hoặc Tạm khóa do rủi ro | Hoạt động hoặc Hạn chế theo chức năng | Điều kiện mở lại và xác minh chủ thể đạt | Chưa giải quyết nguyên nhân hoặc quyền không đủ | Kết quả xác minh và quyền phục hồi |
| Bất kỳ trạng thái nào trừ Đã xóa/ẩn danh | Chờ xóa | Vụ việc hỗ trợ, xác minh lại và cảnh báo tác động | Thiếu chủ thể, phạm vi hoặc ma trận dữ liệu | Request ID, policy version, thời gian chờ |
| Chờ xóa | Trạng thái trước phù hợp | Hủy trong thời gian cho phép và tái kiểm tra an toàn | Đã qua điểm không thể hủy | Actor, reason, restored scope |
| Chờ xóa | Đã xóa/ẩn danh | Job/manifest từng module và đối soát đạt | Còn phần lỗi chưa xử lý hoặc module chưa xác nhận | Manifest, kết quả từng phần, reconciliation |

## Điểm chưa được tự chốt

| Mã | Điểm chờ | Nguồn gỡ | Task ảnh hưởng |
|---|---|---|---|
| M01-LC-O01 | Thị trường/nhóm tuổi nào cần trạng thái chờ đồng ý và quyền cụ thể | REL-01 | M01-T007, A-G01 |
| M01-LC-O02 | Ngưỡng/rule đưa tài khoản vào tạm khóa do rủi ro | M01-T011/M01-T039 | M01-T012, M01-T031 |
| M01-LC-O03 | Thời hạn quản trị, thời gian chờ xóa và mốc không thể hủy | Cấu hình M11; REL-07 | M01-T016, M01-T035–T036 |
| M01-LC-O04 | Thời điểm chính xác thu hồi access/refresh theo từng chuyển trạng thái | M01-T016–T018 | A-G01 |

## Điều kiện duyệt M01-T002

- M01-T001 và REL-01 được xác nhận đúng phiên bản.
- Mọi trạng thái có điều kiện vào/ra, quyền và tác động phiên rõ ràng.
- M01-LC-O01–O04 được đóng hoặc chuyển thành task/finding có chủ.
- A-G01 sử dụng cùng tên trạng thái và có ít nhất một ca cho phép/từ chối cho mỗi chuyển nhạy cảm.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo ma trận trạng thái/chuyển trạng thái từ quyết định M01 và REL-01 draft | Chưa gán |
