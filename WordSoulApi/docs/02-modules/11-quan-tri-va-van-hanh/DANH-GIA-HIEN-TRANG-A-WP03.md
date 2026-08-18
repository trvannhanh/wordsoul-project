# Đánh giá hiện trạng A-WP03 — M11 Quản trị và vận hành nền

## 1. Phạm vi và phương pháp

Tài liệu đối chiếu 42 task được chọn cho A-WP03 với mã nguồn, cấu hình dữ liệu, tác vụ nền và kiểm thử hiện có tại ngày 2026-08-14. Phạm vi gồm M11-T001–M11-T022, M11-T027–M11-T040 và M11-T043–M11-T048.

Đây là đánh giá tĩnh, chỉ đọc. Trạng thái phản ánh mức đáp ứng toàn bộ Definition of Done của task, không chỉ sự tồn tại của màn hình hoặc điểm truy cập quản trị.

| Trạng thái | Ý nghĩa |
|---|---|
| Đã đáp ứng | Có đủ hành vi, kiểm soát, bằng chứng và kiểm thử theo Definition of Done |
| Đáp ứng một phần | Có nền tảng nhưng thiếu một hoặc nhiều kiểm soát/nghiệm thu bắt buộc |
| Chưa có | Không tìm thấy bằng chứng đủ để xác nhận năng lực |
| Không còn phù hợp | Task mâu thuẫn với quyết định đã chốt và phải loại hoặc viết lại phạm vi có lý do |

## 2. Kết quả tổng hợp

| Kết quả | Số task | Tỷ lệ |
|---|---:|---:|
| Đã đáp ứng | 0 | 0% |
| Đáp ứng một phần | 28 | 66,7% |
| Chưa có | 13 | 31,0% |
| Không còn phù hợp | 1 | 2,3% |
| Tổng | 42 | 100% |

Hệ thống đã có nền quản trị thực tế gồm vai trò Admin/SuperAdmin, CRUD nội dung, cấu hình có kiểu và giới hạn, dashboard, tra cứu người dùng, activity log, system log, tác vụ nền và một số điểm bảo trì. Tuy nhiên, phần lớn mới là công cụ thao tác; chưa hình thành lớp kiểm soát vận hành theo các quyết định M11 về quyền tối thiểu, xác minh lại, yêu cầu thay đổi, phiên bản bất biến, rollback, audit không mất, support case và quản lý sự cố.

## 3. Bằng chứng hiện trạng nổi bật

- Các điểm quản trị tập trung phần lớn dưới quyền SuperAdmin; một số điểm ghi thêm quyền Admin nhưng quyền ở cấp controller vẫn làm phạm vi thực tế hẹp hơn dự kiến tại [AdminController.cs](../../../WordSoul.Api/Controllers/AdminController.cs:17).
- Cấu hình hỗ trợ kiểu dữ liệu, giới hạn số, cờ sửa trực tiếp và cập nhật theo lô; một nhóm chính sách học có số phiên bản tăng dần tại [SystemConfigurationService.cs](../../../WordSoul.Application/Services/SystemConfigurationService.cs:31).
- Cấu hình hiện tại sửa trực tiếp bản ghi đang dùng, không tạo lịch sử bất biến, lịch hiệu lực, yêu cầu thay đổi, xem trước hoặc rollback; xóa chỉ bảo vệ một nhóm khóa đặc biệt tại [SystemConfigurationService.cs](../../../WordSoul.Application/Services/SystemConfigurationService.cs:159).
- Điểm health quản trị và health công khai luôn trả trạng thái khỏe mà không kiểm tra cơ sở dữ liệu hoặc phụ thuộc tại [AdminController.cs](../../../WordSoul.Api/Controllers/AdminController.cs:103) và [Program.cs](../../../WordSoul.Api/Program.cs:427).
- Hai thao tác bảo trì hiện chỉ ghi thông tin và trả kết quả thành công; phần xử lý thực tế còn là placeholder tại [AdminController.cs](../../../WordSoul.Api/Controllers/AdminController.cs:117).
- Middleware ghi toàn bộ request/response payload tối đa 4.000 ký tự mà không có danh sách cho phép hoặc che bí mật tại [RequestResponseLoggingMiddleware.cs](../../../WordSoul.Api/Middlewares/RequestResponseLoggingMiddleware.cs:25).
- System log lưu payload, địa chỉ mạng và người dùng; trang chi tiết trả toàn bộ bản ghi cho Admin/SuperAdmin tại [SystemLog.cs](../../../WordSoul.Domain/Entities/SystemLog.cs:5) và [SystemLogsController.cs](../../../WordSoul.Api/Controllers/SystemLogsController.cs:69).
- Hàng đợi system log có dung lượng 10.000 và loại bản ghi cũ nhất khi đầy, trái M11-D021 tại [SystemLogQueue.cs](../../../WordSoul.Infrastructure/BackgroundServices/SystemLogQueue.cs:8).
- Activity log thử ghi ba lần rồi bỏ qua lỗi, không ghi đúng tác nhân quản trị và bị xóa dây chuyền cùng người dùng tại [ActivityLogService.cs](../../../WordSoul.Application/Services/ActivityLogService.cs:26) và [WordSoulDbContext.cs](../../../WordSoul.Infrastructure/Persistence/WordSoulDbContext.cs:54).
- System log mặc định chỉ giữ bảy ngày, không phân tầng 30 ngày vận hành và 12 tháng bảo mật/kiểm toán theo quyết định tại [LogCleanupBackgroundWorker.cs](../../../WordSoul.Infrastructure/BackgroundServices/LogCleanupBackgroundWorker.cs:24).
- Điều chỉnh XP, AP và lượt gợi ý đang sửa trực tiếp bản ghi người dùng, gồm cả AP đã bị loại bỏ, thay vì đi qua sổ biến động M06 tại [UserService.cs](../../../WordSoul.Application/Services/UserService.cs:270).

## 4. Ma trận đánh giá 42 task

### 4.1. Quyền quản trị và thay đổi nhạy cảm

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M11-T001 | Đáp ứng một phần | Tài liệu M11 có thuật ngữ và ranh giới quản trị | Chưa có từ điển được duyệt thống nhất giữa quyền, trạng thái, cấu hình, audit, activity và system log | Trung bình |
| M11-T002 | Đáp ứng một phần | Có nhiều điểm xem/tạo/sửa/xóa, phát tin, điều chỉnh và bảo trì | Chưa có danh mục đầy đủ gắn từng hành động với module sở hữu, độ nhạy cảm, bằng chứng và điều kiện từ chối | Rất cao |
| M11-T003 | Đáp ứng một phần | Có Admin và SuperAdmin | Hai vai trò quá rộng, không tách hỗ trợ, nội dung, vận hành, bảo mật và dữ liệu; một controller gom nhiều quyền không liên quan | Rất cao |
| M11-T004 | Đáp ứng một phần | Phần lớn điểm quản trị có kiểm tra vai trò | Chưa có ma trận quyền tối thiểu; quyền xem/sửa chưa tách; thiếu phạm vi dữ liệu, mục đích truy cập và kiểm soát mặc định từ chối theo hành động | Rất cao |
| M11-T005 | Đáp ứng một phần | Có thay đổi vai trò và thu hồi refresh token khi khóa | Admin có thể cấp SuperAdmin; thiếu xác minh lại, tác nhân, lý do, hiệu lực phiên và rà soát quyền định kỳ | Nghiêm trọng |
| M11-T006 | Không còn phù hợp | Không tìm thấy phiên đặc quyền khẩn | Quyết định đã chốt không cho quyền tạm thời hoặc quyền khẩn cấp; task phải được loại có lý do hoặc đổi thành kiểm chứng rằng không có đường nâng quyền khẩn | Cao |
| M11-T007 | Đáp ứng một phần | Một số thao tác chỉ dành cho SuperAdmin | Chưa phân loại rủi ro/tự phục vụ/một người thực hiện; chưa thể hiện yêu cầu xác minh lại, hạn mức và bằng chứng theo từng thao tác | Rất cao |
| M11-T008 | Chưa có | Không tìm thấy hồ sơ yêu cầu thay đổi | Thiếu mục đích, phạm vi, tác động, bằng chứng, lịch, phương án quay lui và trạng thái yêu cầu | Rất cao |
| M11-T009 | Chưa có | Không có vòng đời phê duyệt; phù hợp với quyết định không duyệt hai người nhưng vẫn thiếu kiểm soát một người | Thiếu trạng thái, hết hạn theo rủi ro, vô hiệu khi nội dung đổi, lý do và truy vết quyết định | Rất cao |
| M11-T010 | Đáp ứng một phần | Cập nhật cấu hình theo lô kiểm tra khóa trùng | Chưa phát hiện xung đột phiên bản, chưa có lịch hiệu lực, múi giờ, hủy/đổi lịch và xử lý hai thay đổi cùng phạm vi | Cao |
| M11-T011 | Chưa có | Không thấy thực thi từ bản yêu cầu đã duyệt hoặc rollback cấu hình | Thiếu định danh chống lặp, trạng thái từng phần, xác minh kết quả và bằng chứng quay lui | Nghiêm trọng |

### 4.2. Cấu hình và quản trị nội dung chéo module

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M11-T012 | Đáp ứng một phần | Cấu hình có khóa, giá trị, kiểu, mô tả, danh mục, giới hạn và cờ sửa trực tiếp | Thiếu chủ sở hữu, module tiêu thụ, mức nhạy cảm, thời hạn lưu, mặc định tin cậy và xác nhận khóa được phép sửa trực tiếp | Cao |
| M11-T013 | Đáp ứng một phần | Có kiểm tra kiểu, giới hạn và tổ hợp chính sách SRS | Kiểm tra chéo chỉ bao phủ một miền; module nguồn chưa xác nhận mọi cấu hình và thông báo lỗi chưa chuẩn hóa toàn hệ thống | Cao |
| M11-T014 | Đáp ứng một phần | Một nhóm chính sách có số phiên bản tăng dần | Giá trị cũ bị sửa tại chỗ; không có bản bất biến, bộ chính sách nguyên tử, lịch sử hiệu lực hoặc khôi phục theo phiên bản | Nghiêm trọng |
| M11-T015 | Chưa có | Không tìm thấy xem trước/mô phỏng thay đổi | Thiếu baseline, đối tượng ảnh hưởng, chi phí, rủi ro, giới hạn dữ liệu và kết quả gắn phiên bản nháp | Cao |
| M11-T016 | Chưa có | Không tìm thấy rollout cấu hình theo nhóm ổn định | Thiếu phân khúc đủ điều kiện, nhóm thử ổn định, chỉ số dừng và loại trừ nhóm nhạy cảm | Cao |
| M11-T017 | Đáp ứng một phần | Một số khóa SRS bị chặn xóa và chỉ sửa theo lô | Chưa có rollback, kiểm tra tương thích, deprecate thay xóa cho mọi cấu hình, phân tích tham chiếu hoặc cảnh báo sai lệch | Nghiêm trọng |
| M11-T018 | Đáp ứng một phần | Có công cụ quản trị từ vựng, bộ từ, nhiệm vụ, thành tựu, vật phẩm, thú cưng, người dùng và cấu hình | Không có ma trận loại nội dung–module nguồn–vòng đời–quyền–mức rủi ro; quyền đang phân tán và rất rộng | Rất cao |
| M11-T019 | Đáp ứng một phần | Nhiều loại nội dung có tạo, sửa, bật/tắt hoặc xóa | Chưa có vòng đời chuẩn, phiên bản, lịch hiệu lực, yêu cầu thay đổi và audit xuyên module; một số thao tác xóa cứng trực tiếp | Rất cao |
| M11-T020 | Đáp ứng một phần | Một số quan hệ dữ liệu hạn chế xóa khi còn tham chiếu | Không có báo cáo tham chiếu trước thay đổi/xóa, phạm vi bản đang chạy/lịch sử và phương án thay thế | Rất cao |
| M11-T021 | Chưa có | Không thấy kiểm soát đồng thời chung cho cấu hình và nội dung quản trị | Có nguy cơ ghi đè bản cũ; thiếu so sánh thay đổi, báo xung đột và yêu cầu đánh giá lại | Rất cao |
| M11-T022 | Đáp ứng một phần | Dashboard có một số chỉ số người dùng, phiên, nội dung, PvP và tiến độ | Chưa có từ điển chỉ số, mẫu số, chủ sở hữu, độ mới, nguồn, giới hạn và phân biệt cùng tên khác công thức | Cao |

### 4.3. Hỗ trợ người dùng, kiểm toán và system log

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M11-T027 | Đáp ứng một phần | Admin tra cứu người dùng theo định danh, tên hoặc thư | Tìm theo tên/thư không yêu cầu vụ việc/lý do, không che kết quả, không giới hạn riêng và không audit lượt tìm | Nghiêm trọng |
| M11-T028 | Đáp ứng một phần | Có activity log, tiến độ học, lịch sử ôn, phiên và trận | Dữ liệu nằm ở nhiều màn hình, chưa có dòng thời gian thống nhất, đánh dấu mâu thuẫn/độ mới hoặc bảo vệ khỏi sửa nguồn | Cao |
| M11-T029 | Chưa có | Không tìm thấy mô hình vụ việc hỗ trợ | Thiếu chủ, SLA, lý do, bằng chứng, ghi chú, trạng thái, liên kết vụ việc trùng và audit truy cập | Rất cao |
| M11-T030 | Đáp ứng một phần | Có khóa/mở, đổi vai trò, xóa, điều chỉnh số dư và dừng trận | Các thao tác không gắn vụ việc, không xem trước đầy đủ, không xác minh lại; điều chỉnh tài sản sửa trực tiếp và vẫn dùng AP | Nghiêm trọng |
| M11-T031 | Đáp ứng một phần | Có activity log và system log cho nhiều thao tác/yêu cầu | Sự kiện không có định danh tác nhân bất biến, đối tượng, kết quả, mức độ, lý do và trước/sau chuẩn; không bao phủ đầy đủ thất bại | Nghiêm trọng |
| M11-T032 | Đáp ứng một phần | Activity log và system log là hai loại bản ghi khác nhau | Ranh giới bằng chứng chưa được cưỡng chế; thao tác nhạy cảm vẫn dựa nhiều vào log vận hành; chưa có activity riêng cho mọi hành động quản trị | Rất cao |
| M11-T033 | Chưa có | Middleware bỏ qua một số đường dẫn và tệp tải lên | Request/response được lưu toàn bộ không che; có thể chứa thông tin đăng nhập, token, thư, nội dung riêng tư hoặc dữ liệu nhạy cảm | Nghiêm trọng |
| M11-T034 | Đáp ứng một phần | Có hàng đợi giới hạn và worker ghi system log | Hàng đợi loại bản ghi cũ khi đầy; không phân cấp audit/bảo mật, không đo lượng mất, không cảnh báo tồn đọng và lỗi ghi không được phục hồi | Nghiêm trọng |
| M11-T035 | Đáp ứng một phần | Admin/SuperAdmin có tra cứu, lọc và phân trang; có dọn system log theo cấu hình | Trang chi tiết trả payload/IP; lượt xem không được audit; mặc định giữ 7 ngày thay vì 30 ngày/12 tháng; activity log bị xóa cùng người dùng; không có legal hold | Nghiêm trọng |

### 4.4. Sức khỏe, công việc nền, bảo trì và sự cố

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M11-T036 | Đáp ứng một phần | Có health endpoint và dashboard đơn giản | Health luôn trả khỏe, không kiểm tra dữ liệu/phụ thuộc/độ mới; thiếu sổ năng lực, chủ sở hữu và trạng thái suy giảm | Nghiêm trọng |
| M11-T037 | Chưa có | Có logger và một số lỗi được ghi | Không có cảnh báo theo ngưỡng, mức độ, chủ, hướng dẫn, nhận/đóng, chống bão và escalation | Nghiêm trọng |
| M11-T038 | Đáp ứng một phần | Có nhiều background service cho log, dọn log, thông báo, email, phân tích và matchmaking | Chưa có sổ đăng ký lịch, timeout, khóa, chạy lại, chống lặp, độ mới, kết quả và chủ sở hữu | Rất cao |
| M11-T039 | Đáp ứng một phần | Worker có vòng chạy, bắt lỗi và một số xử lý lặp | Không có lịch sử chạy chuẩn, checkpoint chung, chạy lại có phạm vi/xem trước, bằng chứng chống tác dụng phụ lặp hoặc quy trình bù | Rất cao |
| M11-T040 | Chưa có | Có một số đối soát cục bộ ở từng miền | Không có sổ luồng trọng yếu, khóa đối soát, ngưỡng, vụ việc và sửa sai không ghi đè lịch sử ở cấp vận hành | Rất cao |
| M11-T043 | Đáp ứng một phần | Có cấu hình MaintenanceMode và hai điểm bảo trì | Chưa thấy cưỡng chế khóa theo năng lực; thao tác bảo trì chỉ là placeholder; thiếu phạm vi, hết hạn, trạng thái thao tác đang chạy, thông báo và điều kiện mở lại | Nghiêm trọng |
| M11-T044 | Chưa có | Có thể đổi cấu hình hoặc gọi thao tác bảo trì thủ công | Không có công tắc dừng theo năng lực, phạm vi tối thiểu, trạng thái người dùng đang chạy, audit bắt buộc và khôi phục có kiểm chứng | Nghiêm trọng |
| M11-T045 | Đáp ứng một phần | Sổ quyết định đã chốt bốn mức sự cố và mục tiêu phản hồi/khôi phục | Chưa có mô hình vận hành, người chỉ huy, escalation, ví dụ phân loại và bằng chứng đo SLA | Rất cao |
| M11-T046 | Chưa có | Không tìm thấy playbook được duyệt | Thiếu hướng dẫn cho mất dịch vụ, lệch dữ liệu, cấp thưởng lặp, gửi sai diện rộng, trận treo và rò dữ liệu | Nghiêm trọng |
| M11-T047 | Chưa có | Có thể gửi email/thông báo nhưng không phải quy trình sự cố | Thiếu nhịp truyền thông, đối tượng, phê duyệt nội dung, hậu kiểm, chủ hành động và theo dõi hoàn tất | Cao |
| M11-T048 | Đáp ứng một phần | Quyết định có nguyên tắc phân tầng mục tiêu phục hồi | Chưa có giá trị theo năng lực, kịch bản diễn tập, kết quả/khoảng trống và đối soát sau phục hồi | Rất cao |

## 5. Sai lệch nghiêm trọng cần ưu tiên

| Thứ tự | Sai lệch | Task liên quan | Tác động |
|---:|---|---|---|
| 1 | System log lưu toàn bộ request/response chưa che và cho xem chi tiết | M11-T031–M11-T035 | Có thể lưu/lộ bí mật đăng nhập, token và dữ liệu cá nhân |
| 2 | Hàng đợi system log loại bản ghi cũ; activity log bỏ lỗi sau ba lần | M11-T031, M11-T034 | Mất bằng chứng đúng lúc hệ thống quá tải hoặc gặp sự cố |
| 3 | Health luôn báo khỏe và thao tác bảo trì trả thành công dù chưa thực hiện | M11-T036, M11-T043 | Vận hành ra quyết định dựa trên trạng thái sai |
| 4 | Điều chỉnh XP/AP/lượt gợi ý sửa trực tiếp hồ sơ người dùng | M11-T030, M06 | Phá nguồn sự thật M06, vẫn duy trì AP đã bị loại bỏ và khó đối soát |
| 5 | Cấu hình sửa tại chỗ, thiếu lịch sử bất biến, xem trước, lịch hiệu lực và rollback | M11-T008–M11-T017 | Không giải thích hoặc khôi phục đáng tin cậy sau thay đổi sai |
| 6 | Vai trò quá rộng, không xác minh lại, không ghi tác nhân/lý do đầy đủ | M11-T002–M11-T007 | Vượt quyền và không đủ bằng chứng khi thao tác nhạy cảm |
| 7 | Tra cứu người dùng/log không cần vụ việc hoặc mục đích và không audit lượt xem | M11-T027–M11-T035 | Dò dữ liệu, truy cập quá phạm vi và thiếu trách nhiệm giải trình |
| 8 | Không có cảnh báo, playbook, kill switch thật và diễn tập | M11-T037, M11-T043–M11-T048 | Phát hiện/chống lan rộng/khôi phục sự cố chậm và không nhất quán |

## 6. Thứ tự nâng cấp đề xuất

### Nhóm 1 — Ngăn rò dữ liệu và mất audit

Ưu tiên M11-T031–M11-T035. Dừng lưu payload toàn phần, xác lập metadata được phép, tách audit khỏi log vận hành, bảo đảm audit/bảo mật không bị loại và sửa thời hạn lưu theo quyết định.

### Nhóm 2 — Thu hẹp quyền và kiểm soát thao tác nhạy cảm

Thực hiện M11-T001–M11-T010 cùng kết quả A-WP01. Loại M11-T006 theo quyết định; mọi thao tác còn lại phải đúng quyền, xác minh lại, có lý do, hạn mức, xem trước và audit.

### Nhóm 3 — Phiên bản hóa cấu hình và nội dung quản trị

Thực hiện M11-T011–M11-T021. Không cho thay đổi rủi ro cao tiếp tục dùng luồng sửa trực tiếp trước khi có phiên bản, bộ chính sách, thời điểm hiệu lực, phân tích tham chiếu và rollback.

### Nhóm 4 — Hỗ trợ có vụ việc và đối soát

Thực hiện M11-T027–M11-T030 và M11-T038–M11-T040. Tìm kiếm, thao tác hỗ trợ, chạy lại và sửa sai phải gắn vụ việc/phạm vi, không sửa nguồn sự thật trực tiếp.

### Nhóm 5 — Health, bảo trì và ứng phó sự cố

Thực hiện M11-T036–M11-T037 và M11-T043–M11-T048. Thay health giả định bằng tín hiệu thực, tạo cảnh báo có chủ, bảo trì/kill switch theo năng lực, playbook và diễn tập có bằng chứng.

## 7. Cổng A-WP03

A-WP03 chưa đạt cổng Giai đoạn A. Điều kiện tối thiểu để chuyển trạng thái:

- Không lưu toàn bộ request/response; mọi metadata được phép phải che dữ liệu nhạy cảm.
- Audit và sự kiện bảo mật không bị loại khi hàng đợi đầy; thao tác nhạy cảm dừng nếu không ghi được bằng chứng bắt buộc.
- Quyền quản trị tách theo trách nhiệm, không có đường cấp quyền khẩn/tạm thời, mọi thao tác nhạy cảm xác minh lại và ghi tác nhân/lý do/trước–sau.
- Cấu hình rủi ro cao có phiên bản bất biến, thời điểm hiệu lực, xem trước, phân tích tham chiếu và rollback.
- Tra cứu/hỗ trợ/xuất dữ liệu theo vụ việc, phạm vi, lý do, che dữ liệu và audit lượt truy cập.
- Health phản ánh trạng thái thật của năng lực/phụ thuộc; cảnh báo có mức, chủ và hướng dẫn xử lý.
- Bảo trì và công tắc dừng hoạt động theo phạm vi; không trả thành công cho thao tác chưa được thực hiện.
- Có bốn mức sự cố, playbook, truyền thông, mục tiêu phục hồi và kết quả diễn tập.
- Điều chỉnh tài sản đi qua M06; AP không còn là giá trị được vận hành.

## 8. Giới hạn đánh giá

- Chưa chạy kiểm thử hoặc kiểm tra môi trường triển khai/quan sát bên ngoài.
- Chưa đánh giá giao diện quản trị trong `wordsoul-admin`; tài liệu này tập trung WordSoulApi.
- Không đọc giá trị bí mật hoặc dữ liệu sản xuất.
- Các task dashboard nâng cao M11-T023–M11-T026 và điều chỉnh/chạy bù M11-T041–M11-T042 thuộc B-WP07, không được tính trong 42 task A-WP03.
