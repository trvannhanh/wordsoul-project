# Phân tích chuyên sâu M09 — Nhóm cộng đồng và xếp hạng

## 1. Mục tiêu và phạm vi

M09 tạo động lực xã hội an toàn cho người học thông qua cộng đồng có quản trị và các phép so sánh thành tích minh bạch. Module phải giúp người học cảm thấy có đồng đội và nhìn thấy tiến bộ tương đối mà không biến việc học thành cuộc đua cày điểm hoặc làm lộ thông tin cá nhân.

### Trong phạm vi

- Định nghĩa loại nhóm, thông tin nhóm, trạng thái và vòng đời.
- Vai trò nội bộ, quyền quản lý và lịch sử thay đổi thành viên.
- Khám phá, lời mời, yêu cầu tham gia, rời nhóm, loại thành viên và chuyển quyền.
- Kiểm duyệt tên, mô tả, hình ảnh và xử lý báo cáo liên quan đến nhóm.
- Định nghĩa bảng xếp hạng, nguồn điểm, công thức, chu kỳ, phạm vi và quy tắc hòa điểm.
- Tổng hợp xếp hạng cá nhân/nhóm, ảnh chụp kết quả và lịch sử mùa.
- Áp dụng lựa chọn riêng tư, chống thao túng và đo tác động đến chất lượng học.

### Ngoài phạm vi

- Xác thực tài khoản và sở hữu hồ sơ gốc.
- Tạo kết quả học, điểm ghi nhớ hoặc kết quả trận đấu gốc.
- Cấp tài sản/phần thưởng và gửi thông báo ra thiết bị.
- Trò chuyện, bài đăng, luồng nội dung xã hội và quan hệ bạn bè nếu chưa được phê duyệt thành module riêng.

## 2. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Đã có thông tin cơ bản của nhóm gồm tên, mô tả, người tạo, thời điểm tạo và thành viên.
- Quản trị có thể tìm, xem chi tiết, tạo, sửa, xóa nhóm, thêm hoặc loại thành viên.
- Đã có cấu hình giới hạn số thành viên, nhưng cần xác nhận việc áp dụng nhất quán.
- Đã có bảng xếp hạng người dùng theo kinh nghiệm/điểm thành tựu tích lũy.
- Đã có xếp hạng thi đấu theo điểm hiện tại, số trận thắng và vị trí.

### Khoảng trống và rủi ro

- Chưa có loại/trạng thái nhóm, quyền riêng tư, vai trò nội bộ hoặc vòng đời ngừng hoạt động.
- Chưa có luồng người học tự khám phá, xin tham gia, nhận lời mời, rời nhóm hay chuyển quyền sở hữu.
- Người tạo nhóm chưa được thể hiện chắc chắn là thành viên/quản lý; xóa cứng nhóm có thể làm mất lịch sử.
- Chưa thấy kiểm tra giới hạn thành viên, quy tắc một người thuộc bao nhiêu nhóm và xử lý đồng thời khi nhóm gần đầy.
- Danh sách thành viên có thể trả thông tin liên hệ và vai trò hệ thống không cần thiết cho người xem.
- Chưa có kiểm duyệt tên/mô tả, báo cáo lạm dụng, đình chỉ hoặc nhật ký quản trị thành viên.
- Xếp hạng theo kinh nghiệm/điểm tích lũy thiên về người dùng lâu năm; xếp hạng thi đấu chưa có mùa hay ảnh chụp cuối kỳ.
- Công thức, phiên bản, điều kiện đủ, hòa điểm, dữ liệu đến muộn và đính chính chưa được quản trị thống nhất.
- Xếp hạng hiện thuộc nhiều luồng khác nhau; cần xác định M09 là nguồn trình bày/tổng hợp, còn M04/M08/M06 là nguồn chỉ số gốc.

## 3. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Mô hình và vòng đời nhóm | Tạo nhóm có mục đích, trạng thái và lịch sử rõ | Loại, thông tin, riêng tư, giới hạn, ngừng/đóng | Nội dung trò chuyện | M01, M11 |
| SF02 | Vai trò và quyền nhóm | Ngăn lạm dụng quyền quản lý | Chủ nhóm, quản lý, thành viên, chuyển quyền, kiểm toán | Quyền quản trị hệ thống | M01, M11 |
| SF03 | Gia nhập và rời nhóm | Tạo vòng đời thành viên an toàn | Khám phá, lời mời, yêu cầu, duyệt, rời, loại, cấm | Quan hệ bạn bè | M01, M10 |
| SF04 | Danh bạ, riêng tư và kiểm duyệt | Bảo vệ người dùng và nội dung cộng đồng | Hồ sơ hiển thị, tìm kiếm, báo cáo, xử lý nội dung | Kiểm duyệt trò chuyện | M01, M10, M11 |
| SF05 | Định nghĩa xếp hạng | Tạo phép so sánh có ý nghĩa và phiên bản | Chỉ số, nguồn, công thức, phạm vi, điều kiện đủ | Tính chỉ số học/trận gốc | M04, M06, M08, M11 |
| SF06 | Chu kỳ, mùa và ảnh chụp | Bảo toàn kết quả theo thời gian | Mùa, ranh giới, đóng kỳ, đính chính, lưu lịch sử | Lịch ôn tập | M01, M04, M08 |
| SF07 | Tổng hợp và xác định thứ hạng | Tính vị trí nhất quán, có thể đối soát | Điểm, hòa điểm, thứ hạng, cập nhật, chạy lại | Quyết định kết quả nguồn | M04, M06, M08 |
| SF08 | Trải nghiệm bảng xếp hạng | Hiển thị so sánh phù hợp, không gây hại | Toàn hệ thống, nhóm, lân cận, vị trí cá nhân, lịch sử | Gửi thông báo thiết bị | M01, M10 |
| SF09 | Xếp hạng nhóm và ghi nhận xã hội | Khuyến khích hợp tác thay vì chỉ cạnh tranh cá nhân | Điểm nhóm, chuẩn hóa quy mô, thành tích, bàn giao thưởng | Cấp thưởng | M06, M07 |
| SF10 | Công bằng, chống thao túng và vận hành | Giữ xếp hạng đáng tin cậy, có giá trị học | Điều kiện đủ, bất thường, kiểm toán, chỉ số tác động | Điều tra ngoài hệ thống | M04, M08, M11 |

## 4. Phân tích chi tiết

### SF01 — Mô hình và vòng đời nhóm

**Business flow:** người có quyền chọn loại nhóm và tạo bản nháp; khai báo tên, mục đích, mô tả, hình ảnh, chế độ khám phá/tham gia và giới hạn; hệ thống kiểm tra hợp lệ/kiểm duyệt; nhóm được kích hoạt; thay đổi nhạy cảm có phê duyệt; khi không còn sử dụng, nhóm chuyển sang hạn chế, lưu trữ hoặc đóng mà vẫn giữ lịch sử.

**Edge case:** tên trùng/gây nhầm lẫn; người tạo bị khóa; nhóm không có thành viên; vượt giới hạn; đổi công khai thành riêng tư; đóng nhóm đang có mùa xếp hạng; xóa hình ảnh; nhiều loại nhóm chồng mục đích.

**DoD:** loại, trạng thái, chế độ tham gia và giới hạn được định nghĩa; người tạo có vai trò hợp lệ; không xóa cứng nhóm có lịch sử; mọi thay đổi nhạy cảm có tác động, lý do và kiểm toán.

### SF02 — Vai trò và quyền nhóm

**Business flow:** nhóm có một chủ sở hữu và các vai trò được phép; chủ gán/thu hồi vai trò trong giới hạn; hệ thống kiểm tra người thao tác và người bị tác động; chuyển quyền cần xác nhận của bên nhận; hành động quản lý được ghi lịch sử; khi chủ rời hoặc bị khóa, chính sách kế nhiệm được áp dụng.

**Edge case:** tự nâng quyền; quản lý loại chủ; chủ cuối cùng rời; hai yêu cầu chuyển quyền; người nhận không còn thành viên; hạ quyền trong lúc có yêu cầu đang chờ; quản trị can thiệp; nhóm do hệ thống quản lý.

**DoD:** ma trận quyền bao phủ mọi hành động; luôn có chủ hợp lệ hoặc trạng thái quản trị; không tự phê duyệt ngoài chính sách; chuyển quyền xác định một kết quả; lịch sử trước/sau truy vết được.

### SF03 — Gia nhập và rời nhóm

**Business flow:** người học khám phá nhóm được phép; gửi yêu cầu hoặc dùng lời mời; hệ thống kiểm tra điều kiện, giới hạn và cấm; người có quyền duyệt; tư cách thành viên bắt đầu một lần; người học có thể rời, quản lý có thể loại theo lý do; lời mời/yêu cầu hết hạn; M10 nhận tín hiệu phù hợp.

**Edge case:** lời mời và yêu cầu đồng thời; duyệt khi nhóm vừa đầy; gửi lặp; một người thuộc quá nhiều nhóm; người bị cấm; chủ muốn rời; nhóm đóng trong lúc chờ; lời mời chuyển tiếp; hai quản lý cùng duyệt; người dùng bị xóa.

**DoD:** mỗi người/nhóm có một trạng thái thành viên hiện hành; thao tác gửi lại an toàn; giới hạn được kiểm tra tại thời điểm quyết định; rời/loại giữ lịch sử; quyền và thông báo không bị lặp.

### SF04 — Danh bạ, riêng tư và kiểm duyệt

**Business flow:** M09 lấy tên/ảnh hiển thị được phép từ M01; áp dụng phạm vi công khai, thành viên hoặc quản lý; người dùng tìm nhóm theo chính sách; nội dung mới/đổi được kiểm tra; người xem có thể báo cáo; M11 phân loại và xử lý cảnh báo, ẩn, yêu cầu sửa, đình chỉ hoặc đóng.

**Edge case:** email bị lộ; hồ sơ riêng tư xuất hiện trên xếp hạng; tên xúc phạm hoặc giả mạo; hình ảnh bị thay sau duyệt; báo cáo hàng loạt ác ý; người chưa đăng nhập; dữ liệu đã lưu tạm sau khi đổi riêng tư; kháng nghị xử lý.

**DoD:** ma trận trường dữ liệu–đối tượng xem được duyệt; không hiển thị thông tin liên hệ mặc định; kiểm duyệt và kháng nghị có trạng thái/thời hạn; thay đổi riêng tư phản ánh trong cửa sổ cam kết.

### SF05 — Định nghĩa xếp hạng

**Business flow:** quản trị chọn mục tiêu bảng xếp hạng; xác định nguồn chỉ số chính thức, công thức, phạm vi, điều kiện đủ, chu kỳ, quy tắc hòa điểm và phiên bản; xem trước phân bố/tác động; duyệt và kích hoạt; thay đổi lớn chỉ áp dụng kỳ mới; định nghĩa ngừng dùng vẫn giữ lịch sử.

**Edge case:** dùng điểm có thể tiêu; chỉ số âm; người mới; nguồn thiếu dữ liệu; công thức thiên vị trình độ/thời gian tham gia; đổi công thức giữa mùa; hai bảng giống nhau; nguồn chỉ số bị đính chính.

**DoD:** mỗi bảng có mục đích, chủ nguồn, công thức, phiên bản, điều kiện đủ, phạm vi, chu kỳ và hòa điểm; M09 không tự tạo chỉ số gốc; thay đổi có mô phỏng và hiệu lực rõ.

### SF06 — Chu kỳ, mùa và ảnh chụp

**Business flow:** trước mùa, M09 chốt định nghĩa và ranh giới thời gian; khởi tạo trạng thái đủ điều kiện; trong mùa nhận chỉ số hợp lệ; đến hạn khóa kỳ theo cửa sổ sự kiện muộn; tạo ảnh chụp kết quả; xử lý đính chính theo chính sách; công bố và lưu lịch sử; kỳ mới không ghi đè kỳ cũ.

**Edge case:** đổi múi giờ; mùa chồng lấn; sự kiện xảy ra trước nhưng đến sau; đóng kỳ chạy lại; nguồn rút kết quả; người dùng tham gia giữa mùa; nhóm giải thể; kỳ không đủ người; tạm dừng khẩn cấp.

**DoD:** ranh giới tuyệt đối, cửa sổ muộn và trạng thái mùa rõ; đóng kỳ chạy lại không đổi kết quả; ảnh chụp có phiên bản/bằng chứng; đính chính không sửa lịch sử âm thầm.

### SF07 — Tổng hợp và xác định thứ hạng

**Business flow:** M09 tiếp nhận ảnh chụp hoặc biến động chỉ số từ nguồn; xác minh người, kỳ, định danh và điều kiện đủ; cập nhật tổng hợp đúng một lần; áp công thức phiên bản; sắp xếp theo điểm và tiêu chí hòa; gán vị trí; xuất trạng thái cập nhật và cho phép đối soát.

**Edge case:** sự kiện lặp/sai thứ tự; cập nhật đồng thời; bằng điểm hoàn toàn; điểm vượt giới hạn; người bị loại sau khi có điểm; nguồn chậm; vị trí thay đổi khi phân trang; xử lý lại toàn kỳ; số lượng người rất lớn.

**DoD:** cùng dữ liệu/phiên bản cho cùng thứ hạng; gửi lại không cộng lặp; quy tắc hòa xác định; tổng điểm giải thích được từ nguồn; chạy lại đối chiếu được; mức trễ và thời điểm cập nhật được công bố.

### SF08 — Trải nghiệm bảng xếp hạng

**Business flow:** người học chọn bảng/phạm vi/kỳ; M09 kiểm tra quyền riêng tư và điều kiện; trả nhóm dẫn đầu, khu vực quanh vị trí người xem, vị trí cá nhân/nhóm, điểm, tiêu chí và thời điểm cập nhật; nếu chưa đủ điều kiện thì giải thích; lịch sử mùa hiển thị theo chính sách.

**Edge case:** ngoài nhóm dẫn đầu; không có điểm; không đủ điều kiện; hồ sơ ẩn; đồng hạng qua nhiều trang; dữ liệu đang đóng kỳ; người dùng bị khóa; nhóm riêng tư; vị trí thay đổi trong lúc xem; bảng trống.

**DoD:** người dùng luôn tìm được vị trí hoặc lý do không có; phân trang không trùng/bỏ khi cùng ảnh chụp; thông tin riêng tư được che; công thức, kỳ và độ mới minh bạch; không mặc định chỉ hiển thị top gây nản lòng.

### SF09 — Xếp hạng nhóm và ghi nhận xã hội

**Business flow:** M09 xác định thành viên hợp lệ trong kỳ; tổng hợp đóng góp theo công thức nhóm; chuẩn hóa theo quy mô/hoạt động nếu được duyệt; tính vị trí nhóm; công bố đóng góp ở mức riêng tư phù hợp; cột mốc hợp lệ có thể gửi sang M07 hoặc yêu cầu thưởng sang M06 theo quyền sở hữu rõ.

**Edge case:** nhóm tuyển đông để tăng điểm; thành viên chuyển nhóm giữa mùa; người rời sau đóng góp; tài khoản gian lận; nhóm giải thể; nhóm một người; đóng góp âm; thưởng tập thể cho người vào phút cuối; tên nhóm bị ẩn.

**DoD:** quy tắc tư cách/đóng góp theo thời điểm rõ; quy mô không tạo lợi thế vô hạn; chuyển nhóm không nhân đôi điểm/quyền lợi; M09 không cấp thưởng; lịch sử nhóm và thành viên đối soát được.

### SF10 — Công bằng, chống thao túng và vận hành

**Business flow:** M09 theo dõi phân bố điểm, tốc độ tăng, hành vi lặp, nhiều tài khoản, chênh lệch nhóm và tương quan với kết quả học; bất thường được gắn cờ; M11 xem bằng chứng và quyết định giữ, loại, đóng băng hoặc đính chính; người dùng có thể được thông báo/kháng nghị; thay đổi công thức được thử nghiệm và quay lại có kiểm soát.

**Edge case:** người học giỏi thật bị gắn cờ; tài khoản mới tăng nhanh; hành vi nhóm phối hợp thao túng; dữ liệu nguồn sai; kiểm duyệt viên xung đột lợi ích; loại sau trao thưởng; mô hình bất thường thiên vị nhóm nhỏ; thiếu dữ liệu chất lượng học.

**DoD:** tiêu chí đủ điều kiện và bất thường có lý do; không tự kết luận gian lận chỉ từ một tín hiệu; xử lý có quyền/kiểm toán/kháng nghị; chỉ số đánh giá bao gồm chất lượng học, duy trì và tác động tiêu cực.

## 5. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Xây dựng cộng đồng có vòng đời và quyền rõ | SF01, SF02 | M09-T001–M09-T010 |
| Quản lý tư cách, riêng tư và an toàn | SF03, SF04 | M09-T011–M09-T021 |
| Định nghĩa bảng và vận hành mùa | SF05, SF06 | M09-T022–M09-T030 |
| Tổng hợp và trình bày thứ hạng | SF07, SF08 | M09-T031–M09-T039 |
| Xếp hạng nhóm, công bằng và vận hành | SF09, SF10 | M09-T040–M09-T045 |
| Nghiệm thu và bàn giao | SF01–SF10 | M09-T046–M09-T047 |

## 6. Thứ tự thực hiện đề xuất

1. Chốt loại nhóm, vòng đời, vai trò, giới hạn và riêng tư.
2. Chốt gia nhập/rời nhóm, chuyển quyền, kiểm duyệt và lịch sử.
3. Chốt nguồn chỉ số, công thức, điều kiện đủ, hòa điểm và mùa.
4. Chốt tổng hợp, ảnh chụp, trải nghiệm cá nhân/nhóm và bàn giao M06/M07/M10.
5. Chốt chống thao túng, đo tác động, nghiệm thu và tài liệu bàn giao.

## 7. Cơ sở quyết định đã chốt

Tất cả 25 quyết định M09 đã được chốt. Hệ thống có nhóm cộng đồng tự quản và lớp học quản trị với quy tắc riêng. Người tạo nhóm cộng đồng phải có tài khoản đủ 7 ngày, cấp người dùng 10, xác minh liên hệ, không bị hạn chế và chỉ sở hữu một nhóm; mỗi người tham gia một nhóm cộng đồng chính và tối đa 10 lớp đang hoạt động. Nhóm cộng đồng tối đa 50 người, lớp học tối đa 100 người.

Bảng học tập dùng điểm tổng hợp 50% ghi nhớ, 30% chất lượng và 20% duy trì; cạnh tranh theo dải trình độ. Mùa kéo dài 4 tuần, có cửa sổ tạm chốt 24 giờ và cập nhật mục tiêu trong 5 phút. Điểm nhóm là trung bình chuẩn hóa, cần ít nhất 5 thành viên hoạt động, có trần đóng góp và chỉ xét tối đa 30 người cao nhất.

## 8. Rủi ro còn hiệu lực

- Quyền nhóm không đầy đủ dẫn đến chiếm quyền, loại thành viên tùy tiện hoặc nhóm không còn chủ.
- Hồ sơ, email hoặc thành tích bị hiển thị ngoài lựa chọn riêng tư.
- Điểm tích lũy suốt đời khiến người mới không có cơ hội cạnh tranh.
- Công thức nhóm thiên vị nhóm đông, tài khoản phụ hoặc thành viên chuyển nhóm.
- Cập nhật/đóng mùa không có ảnh chụp và định danh khiến thứ hạng thay đổi sau công bố.
- Tối ưu xếp hạng theo hoạt động có thể làm giảm ghi nhớ, tăng áp lực và tạo hành vi học đối phó.

## 9. Điều kiện sẵn sàng triển khai

- M01 chốt hồ sơ hiển thị, lựa chọn riêng tư, trạng thái tài khoản và múi giờ.
- M04/M08/M06 chốt nguồn chỉ số, định danh, đính chính và ý nghĩa của điểm gốc.
- M07/M06 thống nhất quyền sở hữu ghi nhận thành tích và phần thưởng.
- M10 thống nhất tín hiệu xã hội; M11 thống nhất quyền, kiểm duyệt, kiểm toán và chỉ số.
- Tất cả 25 quyết định đã chốt được truy vết; backlog có phụ thuộc và nghiệm thu xuyên module.

Không còn quyết định mở trong M09; `QUYET-DINH-MO.md` là nguồn chi tiết.
