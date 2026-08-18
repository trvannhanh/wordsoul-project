# Đánh giá hiện trạng A-WP01 — M01 Danh tính và hồ sơ

## 1. Phạm vi và phương pháp

Tài liệu đối chiếu 43 task M01 trong Giai đoạn A với mã nguồn, cấu hình dữ liệu và kiểm thử hiện có tại ngày 2026-08-14. Hoạt động đánh giá chỉ đọc; không thay đổi mã nguồn và không chạy thay đổi dữ liệu.

Kết quả dựa trên bằng chứng tĩnh trong kho mã. Một năng lực không được coi là hoàn thành chỉ vì đã có màn hình hoặc luồng cơ bản; phải đáp ứng toàn bộ Definition of Done tại `TASK-BACKLOG.md`.

| Trạng thái | Ý nghĩa |
|---|---|
| Đã đáp ứng | Có đủ bằng chứng cho toàn bộ Definition of Done và kiểm thử trọng yếu |
| Đáp ứng một phần | Có nền tảng hoặc một phần luồng nhưng còn thiếu quy tắc, bảo vệ, truy vết hoặc kiểm thử |
| Chưa có | Không tìm thấy bằng chứng đủ để xác nhận năng lực |
| Không còn phù hợp | Task không còn phù hợp với quyết định đã chốt; phải loại có lý do |

## 2. Kết quả tổng hợp

| Kết quả | Số task | Tỷ lệ |
|---|---:|---:|
| Đã đáp ứng | 0 | 0% |
| Đáp ứng một phần | 24 | 55,8% |
| Chưa có | 19 | 44,2% |
| Không còn phù hợp | 0 | 0% |
| Tổng | 43 | 100% |

Không có task nào được đánh dấu “Đã đáp ứng” vì chưa có bằng chứng nghiệm thu xuyên luồng theo toàn bộ quyết định M01. Hệ thống hiện có nền tảng đáng kể về đăng ký, đăng nhập, OAuth, quyền vai trò, khóa tài khoản, hồ sơ, JWT, giới hạn lưu lượng và activity log; phần thiếu tập trung ở vòng đời, quyền riêng tư, nhiều thiết bị, khôi phục, xóa/ẩn danh hóa, cảnh báo và kiểm toán đầy đủ.

### 2.1. Theo mốc backlog

| Mốc | Phạm vi task | Đáp ứng một phần | Chưa có | Nhận định |
|---|---|---:|---:|---|
| Chính sách nền | M01-T001–M01-T004 | 3 | 1 | Có tài liệu phân tích nhưng chưa có chính sách bảo mật đo được và bản đồ dữ liệu hoàn chỉnh |
| Truy cập an toàn | M01-T005–M01-T021 | 9 | 8 | Có luồng cơ bản nhưng thiếu xác minh thư, đồng ý, liên kết an toàn, quản lý phiên và khôi phục |
| Hồ sơ và quản trị | M01-T022–M01-T032 | 8 | 3 | Có hồ sơ, avatar, vai trò, khóa và một thiết bị; thiếu nhiều thiết bị, xung đột và kiểm soát sâu |
| Quyền dữ liệu và quan sát | M01-T033–M01-T041 | 3 | 6 | Xóa cứng và activity log hiện tại chưa đáp ứng vòng đời dữ liệu/kiểm toán đã chốt |
| Sẵn sàng bàn giao | M01-T042–M01-T043 | 1 | 1 | Có bộ tài liệu nền nhưng chưa có nghiệm thu xuyên chức năng |

## 3. Bằng chứng hiện trạng nổi bật

- Bản ghi người dùng có tên, thư điện tử, thông tin bảo mật, vai trò, trạng thái hoạt động, refresh token, giờ học mong muốn, một mã thiết bị nhận tin và một số chỉ số gamification tại [User.cs](../../../WordSoul.Domain/Entities/User.cs:7).
- Đăng ký, đăng nhập trực tiếp, làm mới phiên và Google OAuth đã tồn tại tại [AuthService.cs](../../../WordSoul.Application/Services/AuthService.cs:50); kiểm thử đơn vị bao phủ các nhánh cơ bản tại [AuthServiceTests.cs](../../../WordSoul.Tests/Services/AuthServiceTests.cs:112).
- Đăng ký và đăng nhập có giới hạn theo địa chỉ mạng, nhưng làm mới phiên chưa dùng cùng chính sách tại [AuthController.cs](../../../WordSoul.Api/Controllers/AuthController.cs:28).
- Google OAuth yêu cầu thư đã xác minh nhưng tự liên kết với tài khoản có cùng thư tại [AuthService.cs](../../../WordSoul.Application/Services/AuthService.cs:119), trái với M01-D005 và M12-D006.
- Callback OAuth đưa access token và refresh token vào chuỗi truy vấn tại [AuthController.cs](../../../WordSoul.Api/Controllers/AuthController.cs:129), làm tăng nguy cơ lộ qua lịch sử, nhật ký và bên trung gian.
- Đăng nhập và làm mới phiên không kiểm tra trạng thái hoạt động; khóa tài khoản chỉ xóa refresh token hiện tại tại [AuthService.cs](../../../WordSoul.Application/Services/AuthService.cs:50) và [UserService.cs](../../../WordSoul.Application/Services/UserService.cs:174).
- Mỗi người dùng chỉ có một refresh token và một mã thiết bị nhận tin, chưa biểu diễn nhiều phiên/nhiều thiết bị tại [User.cs](../../../WordSoul.Domain/Entities/User.cs:28).
- Vai trò gồm User, Admin và SuperAdmin tại [UserRole.cs](../../../WordSoul.Domain/Enums/UserRole.cs:4); thao tác quản trị có kiểm tra vai trò ở controller nhưng thay đổi vai trò chưa ghi tác nhân, lý do hoặc bảo vệ quản trị cao nhất cuối cùng.
- Xóa người dùng hiện là xóa vật lý, trong khi nhiều quan hệ vừa xóa dây chuyền vừa hạn chế xóa; chưa có thời gian chờ, hủy, ẩn danh hóa hoặc đối soát tại [UserRepository.cs](../../../WordSoul.Infrastructure/Persistence/Repositories/UserRepository.cs:131) và [WordSoulDbContext.cs](../../../WordSoul.Infrastructure/Persistence/WordSoulDbContext.cs:54).
- Activity log có sự kiện đăng ký/đăng nhập và một số thao tác, nhưng chỉ lưu người dùng mục tiêu, hành động, chi tiết ngắn và thời điểm; ghi thất bại ba lần thì bỏ qua, chưa đáp ứng yêu cầu kiểm toán bất biến/không mất tại [ActivityLogService.cs](../../../WordSoul.Application/Services/ActivityLogService.cs:26).
- Không tìm thấy kiểm thử riêng cho cập nhật hồ sơ, vai trò, khóa/mở, xóa, nhiều thiết bị, khôi phục, xuất dữ liệu hoặc xung đột quản trị.

## 4. Ma trận đánh giá 43 task

### 4.1. Chính sách nền và đăng ký

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Mức rủi ro |
|---|---|---|---|---|
| M01-T001 | Đáp ứng một phần | `PHAN-TICH-CHUYEN-SAU.md` có từ điển nghiệp vụ sơ bộ | Từ điển vẫn ghi “sơ bộ”, chưa có bằng chứng phê duyệt và đối chiếu chủ sở hữu với M06/M09/M10/M11/M12 | Trung bình |
| M01-T002 | Đáp ứng một phần | Có trạng thái hoạt động và xóa vật lý | Chưa có vòng đời đầy đủ cho chưa xác minh, khóa tạm, hạn chế theo chức năng, chờ xóa, đã ẩn danh hóa và tác động phiên | Rất cao |
| M01-T003 | Đáp ứng một phần | Bản ghi người dùng và các dữ liệu trả về đã xác định một phần trường hiện có | Thiếu tuổi, quốc gia, đồng ý, múi giờ, quyền hiển thị, mục đích, thời hạn lưu và nguồn sự thật liên module; M01 vẫn giữ AP và số dư | Rất cao |
| M01-T004 | Chưa có | Có cơ chế băm thông tin bảo mật | Không có chính sách độ mạnh, chuẩn lỗi, tái sử dụng, thay đổi hoặc ngoại lệ đo được; dữ liệu đầu vào chưa có ràng buộc tương ứng | Rất cao |
| M01-T005 | Đáp ứng một phần | Có kiểm tra trùng tên/thư ở tầng nghiệp vụ và giới hạn độ dài trên bản ghi | Chưa chuẩn hóa chữ hoa/thường, khoảng trắng, định dạng thư, độ mạnh thông tin bảo mật; không thấy ràng buộc duy nhất ở dữ liệu cho tên/thư nên có rủi ro đồng thời | Cao |
| M01-T006 | Chưa có | Google trả về trạng thái thư đã xác minh cho đăng nhập ngoài | Không có luồng xác minh thư cho đăng ký trực tiếp, bằng chứng một lần, hết hạn, gửi lại hoặc quyền hạn trước xác minh | Rất cao |
| M01-T007 | Chưa có | Không tìm thấy dữ liệu đồng ý | Thiếu loại đồng ý, phiên bản chính sách, thời điểm, rút lại và tác động; không thể chứng minh đồng ý người giám hộ | Rất cao |
| M01-T008 | Đáp ứng một phần | Đăng ký tạo danh tính rồi khởi tạo thành tựu, thú cưng và nhiệm vụ; có một số kiểm tra tránh gán lặp | Nhiều lần lưu tách rời, không có mã chống lặp cho toàn luồng hoặc trạng thái tiếp tục; thất bại giữa chừng có thể để dữ liệu khởi tạo một phần | Rất cao |
| M01-T009 | Đáp ứng một phần | Có kiểm thử đăng ký thành công, trùng tên và trùng thư | Chưa kiểm thử xác minh, đồng ý, chuẩn hóa, gửi lặp, đồng thời, lỗi khởi tạo từng phần và rollback/tiếp tục | Cao |

### 4.2. Đăng nhập, danh tính ngoài, phiên và khôi phục

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Mức rủi ro |
|---|---|---|---|---|
| M01-T010 | Đáp ứng một phần | Phản hồi đăng nhập sai dùng thông báo chung; đăng nhập thành công có activity log | Không kiểm tra tài khoản hoạt động/đủ điều kiện và đang dùng tên thay vì thư như quyết định; thiếu truy vết đăng nhập thất bại | Rất cao |
| M01-T011 | Đáp ứng một phần | Có giới hạn đăng ký/đăng nhập theo địa chỉ mạng | Chưa có ngưỡng theo tài khoản, cảnh báo, phục hồi, kiểm soát phân tán và bằng chứng chống khóa nhầm | Cao |
| M01-T012 | Đáp ứng một phần | Có cờ hoạt động và thao tác khóa/mở | Đăng nhập, làm mới và access token đang tồn tại chưa bị kiểm tra/thu hồi đầy đủ; chưa có trạng thái chưa xác minh, khóa tạm, chờ xóa hoặc đường khôi phục | Rất cao |
| M01-T013 | Đáp ứng một phần | Google OAuth lấy dữ liệu hồ sơ tối thiểu, từ chối thư chưa xác minh và có định danh ngoài duy nhất theo cặp | Chưa xác minh state chống giả mạo; token trả qua URL; chưa có ma trận lỗi/hết hạn/suy giảm và chống tạo trùng xuyên toàn luồng | Rất cao |
| M01-T014 | Chưa có | Có nhánh tìm tài khoản bằng thư | Nhánh hiện tại tự liên kết ngay khi thư trùng, không yêu cầu đăng nhập lại/xác minh chủ tài khoản, không thông báo liên kết và không có audit phù hợp | Nghiêm trọng |
| M01-T015 | Chưa có | Có ràng buộc duy nhất cho cặp nhà cung cấp–định danh | Mô hình chỉ giữ một nhà cung cấp trên bản ghi người dùng; không có tự quản lý nhiều liên kết, gỡ liên kết, xung đột hoặc bảo vệ phương thức cuối | Rất cao |
| M01-T016 | Đáp ứng một phần | Access token có hạn một ngày, refresh token bảy ngày | Chỉ có một refresh token cho mọi thiết bị, chưa có phiên quản trị ngắn hơn, thời hạn tối đa 30 ngày hoạt động, danh sách phiên, thu hồi theo thiết bị hoặc xác minh lại thao tác nhạy cảm | Rất cao |
| M01-T017 | Đáp ứng một phần | Làm mới hợp lệ tạo refresh token mới; token cũ sau đó không còn khớp | Token lưu trực tiếp, không có họ phiên/thiết bị, dấu vết tái sử dụng, xử lý đồng thời xác định, cảnh báo hoặc kiểm tra trạng thái tài khoản; endpoint chưa có giới hạn riêng | Rất cao |
| M01-T018 | Chưa có | Có năng lực ghi sự kiện đăng xuất nhưng không thấy luồng sử dụng | Không có danh sách phiên, đăng xuất hiện tại, thu hồi một thiết bị, thu hồi tất cả hoặc xác nhận kết quả | Rất cao |
| M01-T019 | Chưa có | Có năng lực gửi thư dùng chung | Không có quên/đặt lại thông tin bảo mật, bằng chứng một lần, hết hạn, giới hạn gửi lại, phản hồi trung tính hoặc thu hồi phiên | Rất cao |
| M01-T020 | Chưa có | Không tìm thấy luồng thay đổi thông tin bảo mật | Thiếu xác minh lại, đổi chủ động, thông báo bảo mật, thu hồi các phiên khác và audit | Rất cao |
| M01-T021 | Chưa có | Quản trị viên có thể tra cứu và cập nhật/xóa tài khoản | Không có vụ việc hỗ trợ, tiêu chí bằng chứng, phạm vi quyền, lý do, quyết định từ chối an toàn hoặc lịch sử xử lý | Cao |

### 4.3. Hồ sơ, thiết bị, vai trò và trạng thái

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Mức rủi ro |
|---|---|---|---|---|
| M01-T022 | Đáp ứng một phần | Người dùng xem hồ sơ mình; quản trị xem danh sách/chi tiết; kiểm tra ngăn người dùng sửa hồ sơ khác | Chưa có ma trận trường–vai trò; phản hồi hồ sơ trộn dữ liệu riêng tư và gamification; chưa có lựa chọn công khai; quản trị chưa cần lý do khi xem | Rất cao |
| M01-T023 | Đáp ứng một phần | Có đổi tên và kiểm tra trùng | Quyết định cho phép tên trùng nhưng hiện tại chặn trùng; thiếu kiểm duyệt nội dung, giới hạn một lần/30 ngày và xử lý lịch sử hiển thị | Trung bình |
| M01-T024 | Đáp ứng một phần | Có tải avatar, ảnh mặc định, xóa giá trị avatar và giữ ảnh hiện tại nếu tải lên thất bại | Chưa thấy kiểm duyệt nội dung, vòng đời/xóa ảnh cũ, quyền tài sản, chống tệp giả hoặc bằng chứng xử lý vi phạm | Cao |
| M01-T025 | Đáp ứng một phần | Có giờ học mong muốn và theo dõi hoạt động gần nhất | Không có múi giờ hồ sơ, nguồn/đề xuất thiết bị, xác nhận người dùng, thời điểm thay đổi hoặc xử lý đổi vùng; chưa có luồng cập nhật giờ học rõ trong M01 | Cao |
| M01-T026 | Chưa có | Có một mã FCM trên bản ghi người dùng | Không hỗ trợ nhiều thiết bị, định danh thiết bị, trạng thái/lần hoạt động, đồng ý nhận tin hoặc chống đăng ký lặp | Rất cao |
| M01-T027 | Chưa có | Mã thiết bị mới ghi đè mã cũ | Không có thu hồi đúng thiết bị khi đăng xuất/hết hiệu lực/rút quyền, không truyền trạng thái chi tiết sang M10 và không audit | Rất cao |
| M01-T028 | Đáp ứng một phần | Có ba vai trò và kiểm tra vai trò tại nhiều thao tác | Chưa có ma trận quyền tối thiểu theo trách nhiệm, chủ sở hữu quyền, quyền xem tách khỏi sửa hoặc kiểm tra quyền mơ hồ/trùng | Rất cao |
| M01-T029 | Đáp ứng một phần | Có thay đổi vai trò và activity log trước/sau | Admin có thể gán cả SuperAdmin; không xác minh lại, không ghi tác nhân/lý do, không giới hạn thẩm quyền và access token cũ giữ quyền đến hết hạn | Nghiêm trọng |
| M01-T030 | Đáp ứng một phần | Không cho khóa tài khoản đang mang vai trò SuperAdmin | Không ngăn hạ/xóa quản trị cao nhất cuối cùng, không bảo vệ tự thay đổi và không có quy trình khôi phục được duyệt | Nghiêm trọng |
| M01-T031 | Đáp ứng một phần | Có bật/tắt hoạt động, ngăn khóa SuperAdmin, xóa refresh token khi khóa và ghi activity log | Chỉ có cờ bật/tắt; thiếu loại, lý do, thời hạn, phạm vi, thông báo, khiếu nại; access token hiện tại vẫn có thể dùng đến hết hạn | Nghiêm trọng |
| M01-T032 | Chưa có | Không thấy kiểm soát phiên bản/đồng thời trên bản ghi người dùng | Hai thay đổi quản trị có thể ghi đè; không có so sánh trước/sau đáng tin cậy, báo xung đột hoặc lịch sử đầy đủ | Rất cao |

### 4.4. Quyền dữ liệu, kiểm toán và bàn giao

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Mức rủi ro |
|---|---|---|---|---|
| M01-T033 | Chưa có | Có mô hình quan hệ dữ liệu giữa người dùng và nhiều module | Chưa có danh mục được duyệt về chủ sở hữu, mục đích, độ nhạy cảm, thời hạn lưu và hành động khi xóa cho từng loại dữ liệu | Rất cao |
| M01-T034 | Chưa có | Không tìm thấy luồng xuất dữ liệu cá nhân | Thiếu tự phục vụ, xác minh lại, phạm vi, trạng thái, thời hạn, đường tải có hạn, xử lý dữ liệu người khác và thất bại từng phần | Rất cao |
| M01-T035 | Đáp ứng một phần | Có xóa tài khoản do Admin thực hiện | Không có yêu cầu hỗ trợ/vụ việc, xác minh chủ thể, cảnh báo tác động, thời gian chờ, khả năng hủy, thu hồi phiên đúng thời điểm hoặc chống xóa nhầm | Nghiêm trọng |
| M01-T036 | Chưa có | Một số quan hệ xóa dây chuyền, một số quan hệ chặn xóa | Không có ma trận xóa/giữ/ẩn danh hóa; xóa vật lý vừa có thể mất lịch sử vừa có thể thất bại do quan hệ hạn chế; không có điều phối/đối soát | Nghiêm trọng |
| M01-T037 | Chưa có | Sau xóa vật lý, thư/tên có thể hết bị chiếm dụng nếu thao tác xóa thành công | Chưa có thời điểm đăng ký lại, danh tính mới, chống nối lại OAuth, tách dữ liệu ẩn danh và quy tắc không phục hồi tiến độ/tài sản | Rất cao |
| M01-T038 | Đáp ứng một phần | Có activity log cho đăng ký, đăng nhập, khóa/mở, vai trò và một số hoạt động | Không bao phủ đăng nhập thất bại, liên kết ngoài, hồ sơ, thiết bị, tra cứu/xuất/xóa; thiếu mức độ, thời hạn 12 tháng, tác nhân và dữ liệu trước/sau chuẩn | Rất cao |
| M01-T039 | Chưa có | Có giới hạn lưu lượng và ghi log vận hành | Không có cảnh báo thiết bị mới/bất thường, khôi phục, liên kết, đổi quyền; thiếu ngưỡng, chủ xử lý, thời gian phản hồi và đóng cảnh báo | Rất cao |
| M01-T040 | Chưa có | Có một số log và thời điểm hoạt động | Không có từ điển chỉ số đăng ký/xác minh/đăng nhập/khôi phục/phiên/khóa với công thức, nguồn, chu kỳ, phân đoạn và ngưỡng | Cao |
| M01-T041 | Đáp ứng một phần | Admin/SuperAdmin có thể tra cứu activity log theo người dùng hoặc toàn hệ thống | Không che trường nhạy cảm theo vai trò, không yêu cầu lý do, không ghi lượt xem/xuất lịch sử; log bị xóa dây chuyền khi xóa người dùng và có thể bị bỏ nếu ghi thất bại | Nghiêm trọng |
| M01-T042 | Chưa có | Có kiểm thử đơn vị cho đăng ký, đăng nhập, refresh token và Google OAuth | Không có bộ nghiệm thu xuyên đăng ký–xác minh–đồng ý–liên kết–phiên–khôi phục–khóa–xóa; thiếu kịch bản đồng thời, quyền, audit và lỗi từng phần | Rất cao |
| M01-T043 | Đáp ứng một phần | Có README, phân tích chuyên sâu, sổ quyết định và backlog M01; 25 quyết định đã chốt | Chưa có bằng chứng duyệt gói bàn giao, kết quả M01-T042 và ma trận giao nhận đầu vào/đầu ra đã nghiệm thu với các module phụ thuộc | Cao |

## 5. Sai lệch nghiêm trọng cần ưu tiên

| Thứ tự | Sai lệch | Task liên quan | Tác động |
|---:|---|---|---|
| 1 | Tự động liên kết Google với tài khoản chỉ vì thư trùng | M01-T014, M01-T015 | Có thể dẫn tới liên kết sai hoặc chiếm quyền tài khoản |
| 2 | Tài khoản bị khóa vẫn có thể đăng nhập/làm mới hoặc dùng access token còn hạn | M01-T010, M01-T012, M01-T016, M01-T031 | Khóa tài khoản không chấm dứt quyền truy cập như quyết định |
| 3 | Admin có thể gán vai trò SuperAdmin; nhật ký ghi người bị đổi thay vì tác nhân | M01-T028–M01-T030, M01-T038 | Vượt thẩm quyền và thiếu bằng chứng điều tra |
| 4 | OAuth callback không chứng minh state và trả token qua URL | M01-T013, M01-T016 | Nguy cơ giả mạo luồng, rò token và chiếm phiên |
| 5 | Không có xác minh thư, đồng ý và dữ liệu tuổi/quốc gia | M01-T002, M01-T003, M01-T006, M01-T007 | Không thực thi được giới hạn trước xác minh hoặc chính sách người chưa thành niên |
| 6 | Một refresh token và một FCM token cho mọi thiết bị | M01-T016–M01-T018, M01-T026–M01-T027 | Không quản lý/thu hồi đúng phiên hoặc thiết bị; có thể gửi nhầm thiết bị dùng chung |
| 7 | Xóa vật lý không có yêu cầu, thời gian chờ, ẩn danh hóa và đối soát | M01-T033–M01-T037 | Mất lịch sử, lỗi toàn vẹn hoặc không đáp ứng quyền dữ liệu đã chốt |
| 8 | Activity log có thể bị bỏ, xóa dây chuyền và không ghi tác nhân/lượt tra cứu | M01-T038–M01-T041 | Không đáp ứng kiểm toán bất biến và điều tra sự cố |

## 6. Thứ tự task nâng cấp đề xuất

### Nhóm 1 — Khóa rủi ro chiếm quyền và vượt quyền

M01-T010, M01-T012–M01-T018, M01-T028–M01-T032 và phần kiểm toán tương ứng của M01-T038/M01-T041. Hoàn thành nhóm này trước khi mở thêm người dùng hoặc quyền quản trị.

### Nhóm 2 — Hoàn thiện đăng ký, xác minh và đồng ý

M01-T002–M01-T009. Nhóm này phải chạy cùng hồ sơ REL-01 vì việc cho mọi độ tuổi truy cập đầy đủ khi thiếu đồng ý là cổng pháp lý, không thể đóng chỉ bằng thay đổi hệ thống.

### Nhóm 3 — Khôi phục và quản lý nhiều phiên/thiết bị

M01-T016–M01-T021 và M01-T025–M01-T027. Đầu ra phải thống nhất với lựa chọn nhận tin của M10 và kênh ngoài của M12.

### Nhóm 4 — Quyền dữ liệu và xóa/ẩn danh hóa

M01-T033–M01-T037. Không sử dụng xóa vật lý hiện tại làm luồng chính thức trước khi có ma trận dữ liệu, thời gian chờ, đối soát và bằng chứng ẩn danh hóa.

### Nhóm 5 — Nghiệm thu và bàn giao

M01-T001, M01-T022–M01-T024, M01-T039–M01-T043. Hoàn thiện từ điển, quyền hồ sơ, cảnh báo, chỉ số và bộ nghiệm thu sau khi các luồng rủi ro cao đã ổn định.

## 7. Cổng A-WP01

A-WP01 chưa đạt cổng Giai đoạn A. Điều kiện tối thiểu để chuyển trạng thái:

- Không còn tự động liên kết danh tính ngoài chỉ bằng thư trùng.
- Mọi đường cấp/gia hạn quyền kiểm tra trạng thái tài khoản; khóa có thể chấm dứt quyền theo chính sách.
- Thay đổi vai trò không vượt thẩm quyền, bảo vệ quản trị cao nhất cuối cùng và ghi đúng tác nhân/lý do/trước–sau.
- Có xác minh thư, đồng ý, tuổi/quốc gia theo quyết định và kết quả rà soát REL-01.
- Có mô hình nhiều phiên/nhiều thiết bị và khả năng thu hồi đúng mục tiêu.
- Có khôi phục quyền truy cập và thay đổi thông tin bảo mật an toàn.
- Có luồng xuất dữ liệu, yêu cầu xóa qua hỗ trợ và ma trận xóa/ẩn danh hóa xuyên module.
- Có danh mục sự kiện, cảnh báo, chỉ số và kiểm toán không mất cho các thao tác nhạy cảm.
- M01-T042 có bằng chứng nghiệm thu thành công; M01-T043 được các module nhận đầu ra xác nhận.

## 8. Giới hạn của đánh giá

- Chưa chạy kiểm thử hoặc tương tác với môi trường triển khai; không xác nhận hành vi runtime hay dữ liệu sản xuất.
- Không đánh giá giao diện web/mobile trong bước này.
- Không đọc giá trị bí mật hoặc thông tin nhạy cảm trong cấu hình.
- Không coi comment hoặc tên thành phần là bằng chứng hoàn thành nếu thiếu hành vi và kiểm thử.
- Mọi trạng thái có thể thay đổi khi xuất hiện bằng chứng ngoài kho mã, tài liệu vận hành hoặc kết quả kiểm thử bổ sung.
