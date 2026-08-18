# Phân tích chuyên sâu M11 — Quản trị, cấu hình và quan sát hệ thống

> Thuật ngữ chuẩn và các cách dùng lỗi thời cần loại theo M11-T001 được quản trị tại [Từ điển quản trị M11](TU-DIEN-QUAN-TRI.md).

## 1. Mục tiêu và phạm vi

M11 cung cấp mặt phẳng điều hành an toàn cho toàn hệ thống: đúng người mới được xem hoặc thay đổi đúng phạm vi; thay đổi có kiểm tra, phê duyệt, hiệu lực và khả năng quay lại; sự cố được phát hiện, xử lý và rút kinh nghiệm; mọi hành động nhạy cảm có bằng chứng mà không làm lộ dữ liệu người học.

### Trong phạm vi

- Danh mục vai trò quản trị, quyền tối thiểu, phân tách nhiệm vụ và quyền tạm thời.
- Yêu cầu thay đổi, phê duyệt, lịch hiệu lực và kiểm toán thao tác nhạy cảm.
- Đăng ký cấu hình, kiểm tra, phiên bản, xem trước tác động, triển khai giới hạn và quay lại.
- Điều phối vòng đời nội dung/quy tắc quản trị thuộc các module nghiệp vụ.
- Bảng điều hành, từ điển chỉ số, độ mới, chất lượng dữ liệu và báo cáo.
- Tra cứu hỗ trợ người dùng có che dữ liệu, vụ việc và lịch sử truy cập.
- Nhật ký kiểm toán/vận hành, liên kết truy vết, lưu giữ và truy cập.
- Sức khỏe năng lực, cảnh báo, công việc nền, đối soát và thao tác phục hồi.
- Xử lý sự cố, bảo trì, liên tục nghiệp vụ và diễn tập khôi phục.

### Ngoài phạm vi

- Sở hữu hoặc thay thế quy tắc nghiệp vụ của M01–M10.
- Tạo kết quả học, trận, nhiệm vụ hoặc tài sản mà không có nguồn nghiệp vụ hợp lệ.
- Trực tiếp sửa lịch sử bất biến của module nguồn.
- Quản lý hạ tầng tích hợp chi tiết thuộc M12, dù M11 quan sát và điều phối sự cố liên quan.

## 2. Vai trò quản trị đề xuất

| Vai trò | Phạm vi chính | Không được mặc định |
|---|---|---|
| Hỗ trợ người dùng | Tra cứu hồ sơ tối thiểu, lịch sử liên quan và quản lý vụ việc | Xem bí mật, sửa cấu hình, điều chỉnh tài sản, đổi vai trò |
| Quản trị nội dung | Soạn và quản lý vòng đời học liệu/nhiệm vụ/thành tựu/phòng | Điều chỉnh tài sản, quản trị tài khoản, tự duyệt thay đổi nhạy cảm |
| Vận hành hệ thống | Theo dõi sức khỏe, công việc nền, cảnh báo và phục hồi được duyệt | Xem nội dung cá nhân không cần thiết, thay luật sản phẩm |
| Chuyên viên kinh tế/cân bằng | Mô phỏng và đề xuất phần thưởng, hiệu ứng, điểm | Tự áp dụng thay đổi lớn hoặc sửa số dư trực tiếp |
| Phân tích sản phẩm | Xem số liệu tổng hợp/ẩn danh và đánh giá thử nghiệm | Xem hồ sơ chi tiết, thao tác vận hành hoặc thay cấu hình |
| Người phê duyệt | Duyệt yêu cầu trong miền được ủy quyền | Tự duyệt yêu cầu do mình tạo, vượt phạm vi ủy quyền |
| Quản trị cấp cao | Quản lý vai trò, chính sách và can thiệp khẩn có kiểm soát | Hoạt động thường ngày không cần thiết; bỏ qua kiểm toán/phê duyệt |

## 3. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Có vai trò quản trị và quản trị cấp cao; nhiều khu vực quản trị đã được bảo vệ theo vai trò.
- Cấu hình có khóa, giá trị, kiểu, khoảng min/max, nhóm, mô tả, khả năng sửa trực tiếp và người/thời điểm cập nhật.
- Cấu hình liên quan thuật toán ôn tập đã có cập nhật theo nhóm và tăng phiên bản chính sách.
- Có bảng điều hành người dùng, phiên học, tiến độ, từ khó và xếp hạng thi đấu.
- Có nhật ký hoạt động, nhật ký yêu cầu/phản hồi, tìm kiếm log và dọn log theo thời hạn.
- Có công việc nền cho thông báo, email, ghép trận, phân tích thời điểm và ghi/dọn log.
- Có thao tác quản trị người dùng, nhóm, nội dung, phòng, nhiệm vụ, thành tựu, thông báo diện rộng và điều chỉnh giá trị có lý do.

### Khoảng trống và rủi ro

- Vai trò Admin/SuperAdmin chưa đủ tách hỗ trợ, nội dung, vận hành, tài chính/cân bằng, phân tích và phê duyệt.
- Nhiều thay đổi có hiệu lực trực tiếp; chưa có bản nháp, so sánh, tác động, phê duyệt hai bước, lịch hiệu lực và quay lại thống nhất.
- Chỉ một nhóm cấu hình có phiên bản; chưa có lịch sử bất biến cho mọi thay đổi, phụ thuộc chéo và cấu hình bí mật/công khai.
- Xóa cấu hình/nội dung có thể làm module dùng giá trị mặc định hoặc phá tham chiếu mà không được nhận biết.
- Nhật ký hoạt động thiếu mô hình mục tiêu, trước/sau, kết quả, lý do, định danh liên kết và tính bất biến đầy đủ.
- Nhật ký yêu cầu có thể chứa payload, thông tin liên hệ, mã xác thực hoặc dữ liệu học nhạy cảm; quy tắc che/lọc chưa rõ.
- Hàng chờ log trong bộ nhớ có thể bỏ bản ghi cũ khi đầy và mất khi khởi động lại; chưa có chỉ số mất log.
- Dashboard tính trực tiếp từ dữ liệu hiện hành, có thể thiếu định nghĩa, độ mới, phiên bản và kiểm tra chất lượng; đôi khi dùng email làm tên hiển thị.
- Điều chỉnh XP/AP/lượt gợi ý trực tiếp xung đột nguồn sự thật kinh tế M06 và khó đối soát.
- Công việc nền chưa có sổ đăng ký, quyền chạy lại, khóa đồng thời, trạng thái bền vững, lịch sử lần chạy và playbook thất bại thống nhất.
- Chưa thấy quy trình sự cố, mức nghiêm trọng, chỉ huy, thông báo, phục hồi, hậu kiểm và diễn tập liên tục nghiệp vụ.

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Danh tính quản trị và quyền tối thiểu | Ngăn quyền quá rộng/lạm dụng | Vai trò, quyền, phạm vi, cấp/thu hồi, phiên đặc quyền | Xác thực gốc | M01, M12 |
| SF02 | Yêu cầu thay đổi và phê duyệt | Kiểm soát thao tác nhạy cảm | Bản nháp, tác động, duyệt, hiệu lực, hủy, khẩn cấp | Quy tắc nghiệp vụ nguồn | M01–M10 |
| SF03 | Cấu hình và triển khai chính sách | Thay đổi an toàn, phiên bản hóa | Danh mục, kiểm tra, phụ thuộc, mô phỏng, rollout, rollback | Bí mật nền tảng chi tiết | M01–M10, M12 |
| SF04 | Vòng đời nội dung quản trị chéo module | Điều phối nhưng không chiếm quyền nghiệp vụ | Nháp, duyệt, lịch, ngừng, tham chiếu, kiểm toán | Biên soạn nội dung nghiệp vụ chi tiết | M02, M06, M07, M08, M10 |
| SF05 | Bảng điều hành và chất lượng số liệu | Cung cấp số liệu đúng nghĩa, đúng độ mới | Từ điển, nguồn, phân quyền, độ mới, đối soát, xuất báo cáo | Phân tích chuyên sâu ngoài phạm vi | M01–M10 |
| SF06 | Hỗ trợ người dùng và vụ việc | Giải quyết vấn đề với dữ liệu tối thiểu | Tìm kiếm, hồ sơ hỗ trợ, vụ việc, ghi chú, thao tác có kiểm soát | Chỉnh lịch sử nguồn trực tiếp | M01–M10 |
| SF07 | Kiểm toán, nhật ký và lưu giữ | Tạo bằng chứng an toàn, đáng tin | Audit, log vận hành, che dữ liệu, liên kết, truy cập, retention | Lưu bí mật/PII không cần thiết | M01, M12 |
| SF08 | Sức khỏe, cảnh báo và công việc nền | Phát hiện/phục hồi lỗi trước khi tác động lớn | Chỉ số, ngưỡng, hàng đợi, công việc, chạy lại, đối soát | Hạ tầng nhà cung cấp chi tiết | M01–M10, M12 |
| SF09 | Thao tác nhạy cảm và bảo trì | Thực hiện điều chỉnh an toàn, có thể đối soát | Điều chỉnh, thu hồi, chạy bù, bảo trì, khóa, xem trước | Sửa trực tiếp dữ liệu nguồn | M01, M06, M10, M12 |
| SF10 | Sự cố và liên tục nghiệp vụ | Giảm tác động và khôi phục có kiểm chứng | Mức độ, chỉ huy, playbook, truyền thông, hậu kiểm, diễn tập | Triển khai hạ tầng chi tiết | M10, M12 |

## 5. Phân tích chi tiết

### SF01 — Danh tính quản trị và quyền tối thiểu

**Business flow:** quản trị cấp cao định nghĩa vai trò/phạm vi; yêu cầu cấp quyền nêu mục đích và thời hạn; người có thẩm quyền duyệt; quyền có hiệu lực và được kiểm tra ở mọi thao tác/xem dữ liệu; quyền nhạy cảm có phiên đặc quyền ngắn; rà soát định kỳ phát hiện quyền thừa; thu hồi/khóa ngay khi đổi vai trò hoặc có sự cố.

**Edge case:** tự cấp quyền; quản trị cuối cùng bị khóa; tài khoản dùng chung; quyền lưu tạm sau thu hồi; nhiều vai trò xung đột; hỗ trợ cần truy cập khẩn; người phê duyệt vắng mặt; tài khoản quản trị bị chiếm; quyền từ vai trò cũ còn hiệu lực.

**DoD:** ma trận vai trò–hành động–dữ liệu được duyệt; mặc định từ chối; không tự cấp/duyệt; quyền tạm có hạn; mọi cấp/thu hồi/đặc quyền có bằng chứng; kiểm thử cả cho phép và từ chối.

### SF02 — Yêu cầu thay đổi và phê duyệt

**Business flow:** người có quyền tạo yêu cầu với mục tiêu, phạm vi, trước/sau, tác động, kế hoạch kiểm chứng/quay lại và lịch; hệ thống kiểm tra; người phê duyệt độc lập xem so sánh/mô phỏng; duyệt/từ chối; đến hiệu lực M11 điều phối module sở hữu thực thi; theo dõi kết quả; lỗi tự động hoặc thủ công quay lại theo kế hoạch.

**Edge case:** người tạo tự duyệt; thay đổi chồng nhau; dữ liệu gốc đổi sau duyệt; lịch qua giờ mùa hè; hủy khi đang thực thi; phê duyệt hết hạn; thay đổi khẩn; thực thi một phần; module nguồn không phản hồi; quay lại không còn hợp lệ.

**DoD:** thao tác nhạy cảm có yêu cầu bất biến và phê duyệt phù hợp; so sánh/tác động/hiệu lực/chủ rõ; thực thi dùng đúng bản đã duyệt; trạng thái một phần/khôi phục truy vết được.

### SF03 — Cấu hình và triển khai chính sách

**Business flow:** cấu hình được đăng ký với chủ sở hữu, loại, phạm vi, mặc định, giới hạn, phụ thuộc và mức rủi ro; quản trị tạo phiên bản; kiểm tra giá trị đơn/chéo; mô phỏng tác động; duyệt; triển khai toàn bộ hoặc giới hạn; module tiêu thụ xác nhận phiên bản; theo dõi chỉ số; quay lại phiên bản trước khi vi phạm ngưỡng.

**Edge case:** khóa lạ; trùng khóa; min lớn hơn max; xóa khóa đang dùng; module dùng mặc định khác; nhiều cấu hình cần đổi nguyên khối; triển khai một phần; bản cũ không tương thích; cấu hình bí mật hiển thị; hai thay đổi cùng hiệu lực.

**DoD:** mọi cấu hình có chủ/kiểu/giới hạn/phân loại/phiên bản; thay đổi bất biến; kiểm tra chéo; có ảnh hưởng và kế hoạch quay lại; module báo phiên bản hiệu lực; không xóa cứng khóa đang được tiêu thụ.

### SF04 — Vòng đời nội dung quản trị chéo module

**Business flow:** M11 cung cấp quy trình chung nháp–kiểm tra–duyệt–lên lịch–hoạt động–ngừng; module nguồn xác thực nghiệp vụ và sở hữu phiên bản; M11 hiển thị tham chiếu/tác động; sau duyệt module nguồn kích hoạt; thay đổi/xóa tuân chính sách ảnh chụp và bảo toàn lịch sử; M11 kiểm toán toàn bộ.

**Edge case:** nhiệm vụ tham chiếu phần thưởng ngừng dùng; phòng dùng học liệu bị ẩn; mẫu thông báo thiếu; hai người cùng sửa; nội dung đang được người dùng dùng; hủy lịch sau khi một phần hiệu lực; quyền quản trị khác miền; phụ thuộc vòng.

**DoD:** trách nhiệm M11/module nguồn rõ; không vượt kiểm tra nghiệp vụ; tham chiếu trước khi ngừng/xóa được liệt kê; phiên bản đang dùng không đổi ngầm; lịch sử và người duyệt được giữ.

### SF05 — Bảng điều hành và chất lượng số liệu

**Business flow:** chủ sản phẩm định nghĩa câu hỏi/chỉ số; M11 ghi công thức, nguồn, cửa sổ, múi giờ, độ mới, phân khúc và quyền; dữ liệu được tổng hợp; kiểm tra đủ/đúng/nhất quán; dashboard hiển thị thời điểm và phiên bản; người dùng drill-down trong quyền; xuất báo cáo được kiểm toán; sai lệch tạo vụ việc.

**Edge case:** nguồn đến muộn; thay công thức; dữ liệu xóa; múi giờ khác; đếm trùng; số tổng không khớp chi tiết; phân khúc quá nhỏ lộ cá nhân; dashboard chậm; dùng email làm tên; xuất dữ liệu vượt quyền.

**DoD:** mỗi chỉ số có từ điển và chủ; độ mới/chất lượng hiển thị; tổng đối soát với nguồn trong ngưỡng; che nhóm nhỏ/PII; thay công thức tạo phiên bản; xuất có phạm vi/hạn/kiểm toán.

### SF06 — Hỗ trợ người dùng và vụ việc

**Business flow:** nhân viên hỗ trợ tìm người dùng bằng định danh tối thiểu; chọn lý do truy cập; M11 hiển thị hồ sơ đã che và dòng thời gian liên module; tạo vụ việc; thu thập bằng chứng; nếu cần thao tác nhạy cảm thì tạo yêu cầu phê duyệt sang SF02/SF09; ghi trao đổi/quyết định; đóng và phân loại nguyên nhân.

**Edge case:** nhầm người; tìm bằng email lộ dữ liệu; người hỗ trợ xem người quen; tài khoản đã xóa; dữ liệu các module mâu thuẫn; vụ việc trùng; yêu cầu người dùng xóa dữ liệu; bằng chứng có bí mật; truy cập khẩn; mở lại vụ việc.

**DoD:** tìm kiếm giảm tối đa lộ dữ liệu; truy cập có lý do và audit; không sửa nguồn trực tiếp; vụ việc có chủ/trạng thái/thời hạn/bằng chứng/kết quả; dữ liệu nhạy cảm theo vai trò.

### SF07 — Kiểm toán, nhật ký và lưu giữ

**Business flow:** hành động nhạy cảm phát sự kiện audit có tác nhân, vai trò, mục tiêu, trước/sau đã che, lý do, kết quả, thời điểm và định danh liên kết; log vận hành ghi sự kiện kỹ thuật tối thiểu; quy tắc lọc bỏ bí mật/PII áp dụng trước lưu; hàng đợi bền vững ghi theo mức ưu tiên; người có quyền tìm/đối soát; hết hạn thì xóa/ẩn danh theo loại.

**Edge case:** log đầy/mất; khởi động lại; payload chứa mật khẩu/mã xác thực; thao tác thất bại không log; người dùng bị xóa làm mất audit; thời gian lệch; quản trị sửa/xóa log; truy vấn log quá rộng; retention cấu hình sai; log là bằng chứng sự cố.

**DoD:** audit bất biến và không phụ thuộc vòng đời người dùng; danh mục trường nhạy cảm bị chặn; tỷ lệ mất log đo được/cảnh báo; truy cập log có audit; retention theo loại/pháp lý; định danh liên kết xuyên module.

### SF08 — Sức khỏe, cảnh báo và công việc nền

**Business flow:** mỗi module/kênh/công việc đăng ký chỉ số sức khỏe, chủ, lịch, thời hạn và phụ thuộc; M11 thu trạng thái; ngưỡng tạo cảnh báo được gom/chống lặp và chuyển đúng người; công việc lưu lần chạy, khóa, tiến độ, checkpoint và kết quả; lỗi được thử lại hoặc chạy lại có quyền; đối soát phát hiện mất/chênh lệch.

**Edge case:** công việc chạy trùng; treo không lỗi; cảnh báo bão; đồng hồ lệch; lỗi nhà cung cấp; trạng thái xanh giả; mất checkpoint; chạy lại tạo tác dụng phụ; công việc hoàn thành một phần; người trực không phản hồi.

**DoD:** có sổ sức khỏe/công việc; mỗi lần chạy định danh và bền vững; chạy lại an toàn hoặc có bù; cảnh báo có ngưỡng/chủ/escalation; dashboard hiển thị độ mới; diễn tập lỗi được thực hiện.

### SF09 — Thao tác nhạy cảm và bảo trì

**Business flow:** quản trị chọn thao tác như điều chỉnh tài sản, thu hồi, chạy bù, đối soát hoặc khóa bảo trì; nhập lý do/vụ việc/bằng chứng; xem trước phạm vi/tác động; phê duyệt theo ngưỡng; M11 yêu cầu module nguồn thực thi với định danh; nhận kết quả; đối soát trước/sau; lỗi một phần được phục hồi/bù; thông báo người bị ảnh hưởng theo chính sách.

**Edge case:** số dư âm; người dùng đang giao dịch; hai điều chỉnh; tự duyệt; nhập sai dấu; chạy bù toàn hệ thống; mất phản hồi; module nguồn đã làm nhưng M11 nghĩ lỗi; bảo trì kéo dài; thu hồi tài sản đã dùng; hủy giữa thao tác.

**DoD:** không sửa dữ liệu nguồn trực tiếp; mọi thao tác có xem trước/định danh/phê duyệt/lý do; gửi lại không áp hai lần; M06 sở hữu biến động tài sản; có trạng thái một phần, bù và đối soát.

### SF10 — Sự cố và liên tục nghiệp vụ

**Business flow:** cảnh báo/vụ việc được phân loại mức độ và tác động; chỉ định người chỉ huy và kênh phối hợp; áp playbook cô lập/giảm tác động; cập nhật trạng thái theo nhịp; khôi phục và kiểm chứng dữ liệu/chức năng; thông báo người dùng qua M10 nếu cần; đóng sự cố; hậu kiểm không đổ lỗi tạo hành động; diễn tập phục hồi theo lịch.

**Edge case:** nhiều sự cố liên quan; sai phân loại; mất công cụ quan sát; người chủ chốt vắng; rò dữ liệu; khôi phục dịch vụ nhưng dữ liệu lệch; bản sao lưu không dùng được; thông báo sai; sự cố kéo dài qua ca; hành động hậu kiểm quá hạn.

**DoD:** mức độ/owner/escalation/playbook rõ; thời gian phát hiện–phản hồi–khôi phục đo được; khôi phục có kiểm tra nghiệp vụ/đối soát; hậu kiểm có nguyên nhân/hành động/chủ/hạn; diễn tập cho kịch bản quan trọng có bằng chứng.

## 6. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Thiết lập quyền tối thiểu và kiểm soát thay đổi | SF01, SF02 | M11-T001–M11-T011 |
| Quản lý cấu hình và nội dung an toàn | SF03, SF04 | M11-T012–M11-T021 |
| Cung cấp số liệu và hỗ trợ đúng quyền | SF05, SF06 | M11-T022–M11-T030 |
| Tạo bằng chứng, quan sát và vận hành công việc | SF07, SF08 | M11-T031–M11-T040 |
| Kiểm soát thao tác nhạy cảm và sự cố | SF09, SF10 | M11-T041–M11-T048 |
| Nghiệm thu và bàn giao | SF01–SF10 | M11-T049–M11-T050 |

## 7. Thứ tự thực hiện đề xuất

1. Chốt vai trò, quyền, đặc quyền tạm thời và luồng yêu cầu/phê duyệt.
2. Chốt danh mục cấu hình, phiên bản, kiểm tra chéo, rollout/rollback và vòng đời nội dung.
3. Chốt từ điển chỉ số, chất lượng số liệu, hỗ trợ người dùng và che dữ liệu.
4. Chốt audit/log, sức khỏe, cảnh báo, công việc nền và đối soát.
5. Chốt thao tác nhạy cảm, bảo trì, sự cố, liên tục nghiệp vụ và nghiệm thu xuyên module.

## 8. Cơ sở quyết định đã chốt

M11 có 27/27 quyết định đã chốt. Các nguyên tắc đã chốt gồm quyền quản trị tách theo trách nhiệm và quyền tối thiểu; không dùng phê duyệt hai người, quyền tạm thời hoặc quyền khẩn cấp. Mọi thao tác nhạy cảm phải xác minh lại, ghi lý do, trước/sau và nhật ký truy cập; không module nào được điều chỉnh tài sản ngoài sổ biến động M06. Phê duyệt hết hạn theo mức rủi ro sau 24 giờ, 7 ngày hoặc 30 ngày và vô hiệu ngay khi nội dung/phạm vi đánh giá thay đổi. Cấu hình rủi ro thấp có thể hiệu lực ngay; mức trung bình/cao phải lên lịch theo mốc tuyệt đối, có thông báo, theo dõi và phương án quay lui. Chỉ cấu hình đã đăng ký sửa trực tiếp và được module sở hữu xác nhận an toàn mới được thay đổi khi hệ thống đang hoạt động. Mọi thay đổi cấu hình tạo phiên bản; các cấu hình liên quan được đóng thành bộ chính sách để áp dụng và quay lui nhất quán. Cấu hình ngừng sử dụng không bị xóa khi còn tham chiếu; xóa vật lý chỉ sau thời hạn lưu giữ và kiểm tra liên kết. Rollout giới hạn phân nhóm ngẫu nhiên ổn định trong phân khúc đủ điều kiện, lưu phiên bản và loại nhóm nhạy cảm chưa được phê duyệt. Module nguồn sở hữu kiểm tra hợp lệ nghiệp vụ; M11 sở hữu quyền, quy trình, lịch hiệu lực và kiểm toán. Sức khỏe, sự cố, bảo mật và sai lệch tài sản cập nhật mục tiêu trong một phút; chỉ số khác theo giờ/ngày và công bố độ mới. Báo cáo lưu/đối soát theo UTC và hiển thị nhất quán theo một múi giờ được công bố. Xuất dữ liệu mặc định ở dạng tổng hợp/đã che; dữ liệu chi tiết chỉ theo vai trò, vụ việc, mục đích, phạm vi và thời hạn có kiểm toán. Hỗ trợ ưu tiên tìm bằng định danh chính xác; email/tên chỉ theo vụ việc, được che, giới hạn và ghi nhật ký. Nhật ký kiểm toán không cho sửa/xóa thường lệ, có kiểm tra toàn vẹn và chính sách hết hạn/lưu giữ điều tra riêng. Nhật ký không lưu toàn bộ nội dung mặc định; chỉ metadata/trường cho phép đã che và không lưu bí mật hoặc âm thanh thô. Khi hàng đợi đầy, nhật ký kiểm toán và bảo mật không được loại bỏ; thao tác nhạy cảm bị từ chối nếu không thể ghi bằng chứng bắt buộc. Nhật ký vận hành ưu tiên thấp chỉ được lấy mẫu theo quy tắc công bố, có đo lượng mất mát, cảnh báo và phương án khôi phục. Chỉ công việc nền đã chứng minh có thể lặp an toàn, có điểm tiếp tục hoặc quy trình bù đã duyệt mới được chạy lại thủ công; mỗi lần chạy phải xác định phạm vi, xem trước tác động và ghi nhật ký. Điều chỉnh tài sản không cần người thứ hai phê duyệt nhưng bắt buộc đúng quyền, đúng vụ việc, có lý do, xác minh lại, hạn mức, xem trước tác động và nhật ký; trường hợp lớn hoặc nhạy cảm cần vai trò cao hơn. Bảo trì ưu tiên khóa đúng năng lực bị ảnh hưởng, chỉ khóa toàn hệ thống để bảo vệ tính toàn vẹn hoặc an toàn chung. Sự cố dùng bốn cấp với mục tiêu phản hồi và khôi phục đã ghi trong sổ quyết định.

Cấu hình ảnh hưởng học thuật, kinh tế, trận hoặc thông báo phải có phiên bản, thời điểm hiệu lực, xem trước tác động và khả năng quay lui. Công việc nền phải chống chạy lặp; dashboard phải công bố định nghĩa, độ mới và nguồn. Nhật ký vận hành giữ mặc định 30 ngày, bảo mật/kiểm toán 12 tháng, trừ lưu giữ điều tra có phạm vi.

## 9. Rủi ro còn hiệu lực

- Quyền quản trị quá rộng hoặc tài khoản dùng chung làm mất trách nhiệm cá nhân.
- Thay đổi trực tiếp, không phiên bản/rollback có thể làm sai học thuật, kinh tế hoặc trận đang chạy.
- Nhật ký raw chứa bí mật/PII nhưng vẫn có thể mất khi hàng đợi đầy, tạo cả rủi ro lộ và thiếu bằng chứng.
- Dashboard sai công thức/độ mới dẫn tới quyết định sản phẩm sai và hỗ trợ người dùng nhầm.
- Điều chỉnh tài sản ngoài M06 làm số dư không giải thích được và có thể áp lặp.
- Công việc nền chạy trùng hoặc phục hồi không idempotent gây gửi trùng, cộng điểm lặp hoặc lệch tiến độ.
- Không có playbook/diễn tập khiến sự cố kéo dài và khôi phục dịch vụ nhưng bỏ lại dữ liệu không nhất quán.

## 10. Điều kiện sẵn sàng triển khai

- M01 chốt định danh quản trị, trạng thái tài khoản và quy tắc xác thực đặc quyền.
- M01–M10 cung cấp danh mục quyền, cấu hình, chỉ số, sự kiện audit, sức khỏe và thao tác phục hồi.
- M06 chốt mọi điều chỉnh tài sản qua sổ biến động; M10 chốt truyền thông sự cố/diện rộng.
- M12 chốt nguồn sức khỏe, bí mật, sao lưu/khôi phục, nhà cung cấp và chỉ số nền tảng.
- Tất cả 27 quyết định đã chốt được truy vết; playbook và nghiệm thu quyền/rollback/sự cố được duyệt.

Toàn bộ quyết định M11 đã được xác nhận. `QUYET-DINH-MO.md` là nguồn trạng thái và nội dung quyết định chi tiết.
