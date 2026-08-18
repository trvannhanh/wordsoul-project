# Task backlog M09 — Nhóm cộng đồng và xếp hạng

> Tất cả 25 quyết định M09 đã được chốt. `QUYET-DINH-MO.md` là đầu vào bắt buộc; S/M/L là độ phức tạp tương đối.

> Bảng dùng ngôn ngữ nghiệp vụ, có thể sao chép sang Jira, Trello hoặc Notion. Độ phức tạp: S (nhỏ), M (vừa), L (lớn).

| Task ID | Chức năng con | Tên task | Mô tả ngắn | Input mong đợi | Output mong đợi | Ưu tiên | Độ phức tạp | Task phụ thuộc | Definition of Done |
|---|---|---|---|---|---|---|---|---|---|
| M09-T001 | SF01 | Thống nhất từ điển cộng đồng | Chốt nghĩa nhóm, chủ sở hữu, quản lý, thành viên, mùa, điểm và thứ hạng | README M09; thuật ngữ M01/M04/M08 | Từ điển và ranh giới sở hữu | Cao | S | Không | Không còn khái niệm cốt lõi có hai nghĩa; module sở hữu dữ liệu gốc/tổng hợp được chỉ rõ |
| M09-T002 | SF01 | Phân loại nhóm | Xác định nhóm cộng đồng, lớp quản trị hoặc loại được hỗ trợ | Mục tiêu sản phẩm; đối tượng người dùng | Danh mục loại nhóm | Cao | M | M09-T001 | Mỗi loại có chủ thể tạo, mục đích, cách tham gia, quyền riêng tư và giới hạn |
| M09-T003 | SF01 | Đặc tả thông tin nhóm | Chốt dữ liệu nghiệp vụ cần có của nhóm | M09-T002; chính sách hồ sơ | Mẫu thông tin nhóm | Cao | M | M09-T002 | Bao phủ tên, mô tả, hình ảnh, loại, chế độ, giới hạn, trạng thái, người tạo và thời điểm |
| M09-T004 | SF01 | Thiết kế vòng đời nhóm | Chốt nháp, chờ duyệt, hoạt động, hạn chế, lưu trữ và đóng | Chính sách quản trị M11 | Sơ đồ trạng thái nhóm | Cao | M | M09-T003 | Mỗi chuyển trạng thái có tác nhân, điều kiện, tác động thành viên/xếp hạng và kiểm toán |
| M09-T005 | SF01 | Chốt giới hạn và quy tắc sở hữu | Xác định số nhóm mỗi người, quy mô nhóm và người tạo có tự là chủ/thành viên | Cấu hình hiện có; mục tiêu loại nhóm | Chính sách giới hạn | Cao | M | M09-T002, M09-T004 | Giới hạn có mặc định, ngoại lệ, thời điểm kiểm tra và hành vi khi giảm cấu hình |
| M09-T006 | SF01 | Chốt lưu trữ và đóng nhóm | Bảo toàn lịch sử thay cho xóa cứng | Vòng đời; nghĩa vụ lưu giữ | Quy trình ngừng hoạt động | Cao | M | M09-T004, M09-T005 | Nhóm có lịch sử không bị xóa mất; người dùng biết tác động; mùa và quyền lợi đang chờ được xử lý |
| M09-T007 | SF02 | Xây dựng ma trận vai trò nhóm | Xác định chủ, quản lý, thành viên và vai trò chỉ đọc nếu cần | Loại nhóm; hành động nghiệp vụ | Ma trận vai trò–quyền | Cao | M | M09-T002 | Bao phủ xem, sửa, mời, duyệt, loại, gán quyền, đóng và xem dữ liệu nhạy cảm |
| M09-T008 | SF02 | Chốt gán và thu hồi vai trò | Quy định ai được thay đổi vai trò của ai | M09-T007; chính sách M11 | Luồng quản lý vai trò | Cao | M | M09-T007 | Không tự nâng quyền hoặc tác động vai trò cao hơn; thay đổi đồng thời có một kết quả |
| M09-T009 | SF02 | Thiết kế chuyển quyền sở hữu | Xử lý chuyển chủ có xác nhận và thất bại an toàn | M09-T007; tư cách thành viên | Luồng chuyển quyền | Cao | L | M09-T007, M09-T008 | Bên nhận hợp lệ và xác nhận; nhóm không có hai/không có chủ; hết hạn/hủy được bao phủ |
| M09-T010 | SF02 | Chốt kế nhiệm và can thiệp quản trị | Xử lý chủ rời, bị khóa, bị xóa hoặc mất khả năng quản lý | Trạng thái M01; quyền M11 | Chính sách kế nhiệm | Cao | L | M09-T009 | Mỗi trường hợp có chủ mới hoặc trạng thái quản trị; can thiệp có lý do, phê duyệt và lịch sử |
| M09-T011 | SF03 | Đặc tả khám phá và điều kiện tham gia | Quy định nhóm nào được tìm thấy và ai đủ điều kiện | Chế độ nhóm; hồ sơ M01 | Quy tắc khám phá/đủ điều kiện | Cao | M | M09-T003, M09-T005 | Nhóm riêng tư không lộ ngoài chính sách; lý do không đủ điều kiện được xác định |
| M09-T012 | SF03 | Thiết kế yêu cầu tham gia | Chốt gửi, hủy, duyệt, từ chối và hết hạn yêu cầu | M09-T011; quyền M09-T007 | Vòng đời yêu cầu tham gia | Cao | M | M09-T007, M09-T011 | Một người/nhóm chỉ có một yêu cầu hiện hành; gửi lại an toàn; quyết định có tác nhân/thời điểm |
| M09-T013 | SF03 | Thiết kế lời mời thành viên | Chốt tạo, gửi, chấp nhận, từ chối, thu hồi và hết hạn lời mời | Vai trò; lựa chọn nhận tin M10 | Vòng đời lời mời | Cao | M | M09-T007, M09-T011 | Lời mời không chuyển tiếp trái phép; không lộ nhóm riêng; chấp nhận lặp không tạo thành viên trùng |
| M09-T014 | SF03 | Giải quyết lời mời và yêu cầu đồng thời | Xác định kết quả khi nhiều đường tham gia cùng tồn tại | M09-T012, M09-T013 | Ma trận hợp nhất trạng thái | Cao | L | M09-T012, M09-T013 | Mọi thứ tự thao tác cho một tư cách thành viên; yêu cầu còn lại được đóng có lý do |
| M09-T015 | SF03 | Đảm bảo giới hạn khi gia nhập đồng thời | Ngăn nhóm vượt chỗ hoặc người vượt số nhóm khi nhiều phê duyệt cùng lúc | M09-T005; luồng tham gia | Quy tắc giữ chỗ/quyết định | Cao | L | M09-T005, M09-T014 | Tại giới hạn chỉ số yêu cầu hợp lệ được nhận; không vượt trần; thất bại giải thích được |
| M09-T016 | SF03 | Thiết kế rời và loại thành viên | Chốt tự rời, bị loại, lý do và tác động quyền/điểm | Vai trò; mùa đang chạy | Luồng kết thúc tư cách | Cao | L | M09-T009, M09-T015 | Chủ không rời khi chưa kế nhiệm; lịch sử giữ nguyên; điểm/quyền lợi theo mùa có chính sách rõ |
| M09-T017 | SF03 | Chốt cấm và tái gia nhập | Xử lý thời hạn cấm, kháng nghị và điều kiện quay lại | Báo cáo; lịch sử thành viên | Chính sách hạn chế thành viên | Trung bình | M | M09-T016 | Cấm có lý do/thời hạn/người duyệt; hết hạn không tự vượt điều kiện; kháng nghị truy vết được |
| M09-T018 | SF04 | Lập ma trận dữ liệu hiển thị | Xác định trường hồ sơ nào được xem theo vai trò/phạm vi | Lựa chọn riêng tư M01; nhu cầu nhóm/xếp hạng | Ma trận dữ liệu–đối tượng xem | Cao | L | M09-T007, M09-T011, M01 | Thông tin liên hệ không công khai mặc định; mỗi trường có nguồn, phạm vi và hành vi khi ẩn |
| M09-T019 | SF04 | Chốt kiểm duyệt tên và hình ảnh | Quy định kiểm tra trước/sau công bố và xử lý sửa nội dung | Chính sách cộng đồng M11 | Quy trình kiểm duyệt nội dung nhóm | Cao | M | M09-T003, M09-T004 | Trạng thái chờ/duyệt/từ chối/ẩn rõ; đổi nội dung phải kiểm tra lại theo mức rủi ro |
| M09-T020 | SF04 | Thiết kế báo cáo và kháng nghị | Cho phép báo cáo nhóm/nội dung/quản lý và theo dõi kết quả | M09-T019; quyền M11 | Vòng đời vụ việc cộng đồng | Trung bình | L | M09-T019 | Chống báo cáo lặp; bằng chứng được giới hạn; có thời hạn, quyết định, thông báo và kháng nghị |
| M09-T021 | SF04 | Chốt tìm kiếm và danh bạ nhóm | Xác định lọc, sắp xếp, dữ liệu trả về và trạng thái không hiển thị | M09-T011, M09-T018, M09-T019 | Yêu cầu danh bạ an toàn | Trung bình | M | M09-T011, M09-T018, M09-T019 | Không rò nhóm/nội dung/hồ sơ bị ẩn; kết quả có phân trang ổn định và lý do loại trừ |
| M09-T022 | SF05 | Lập danh mục bảng xếp hạng | Xác định bảng học tập, thi đấu, cá nhân và nhóm thực sự cần | Mục tiêu sản phẩm; nguồn M04/M08 | Danh mục bảng và mục đích | Cao | M | M09-T001 | Mỗi bảng có đối tượng, hành vi muốn khuyến khích, nguy cơ và tiêu chí loại bỏ |
| M09-T023 | SF05 | Chốt nguồn chỉ số chính thức | Xác định module nguồn và bằng chứng cho từng thành phần điểm | M04, M06, M08; danh mục M09-T022 | Ma trận chỉ số–nguồn | Cao | L | M09-T022 | Mỗi chỉ số có chủ, đơn vị, thời điểm, định danh, đính chính và giới hạn sử dụng |
| M09-T024 | SF05 | Đặc tả công thức xếp hạng học tập | Tạo phép đo cân bằng khối lượng, chất lượng và duy trì | Chỉ số M04/M06; mục tiêu giáo dục | Công thức nghiệp vụ và giải thích | Cao | L | M09-T023 | Không chỉ dựa vào thao tác/điểm có thể tiêu; có trần, mô phỏng theo trình độ và ví dụ đối soát |
| M09-T025 | SF05 | Đặc tả xếp hạng thi đấu | Chốt M09 sử dụng điểm/kết quả chính thức từ M08 ra sao | Xếp hạng và kết quả M08 | Hợp đồng nguồn thi đấu | Cao | L | M09-T023, M08 | M08 sở hữu kết quả/điểm gốc; M09 sở hữu phạm vi/mùa/trình bày; bỏ cuộc và hủy trận rõ |
| M09-T026 | SF05 | Chốt điều kiện đủ và phân hạng | Xác định số hoạt động tối thiểu, trạng thái tài khoản và nhóm so sánh | M09-T023–M09-T025; hồ sơ M01 | Quy tắc đủ điều kiện/phân hạng | Cao | L | M09-T024, M09-T025 | Người mới/không hoạt động có trạng thái rõ; phân hạng không suy diễn sai năng lực; loại trừ có lý do |
| M09-T027 | SF05 | Chốt phiên bản và hiệu lực công thức | Quy định thay đổi nào chỉ áp dụng mùa mới và cách xem trước tác động | Công thức; vòng đời quản trị | Chính sách phiên bản | Cao | M | M09-T024–M09-T026 | Mỗi kết quả gắn phiên bản; không đổi công thức âm thầm giữa mùa; có phê duyệt và quay lại |
| M09-T028 | SF06 | Thiết kế vòng đời mùa | Chốt chuẩn bị, hoạt động, tạm dừng, đóng, chờ đính chính và công bố | Định nghĩa bảng; lịch vận hành | Sơ đồ trạng thái mùa | Cao | L | M09-T027 | Mỗi trạng thái có thời gian, quyền, hành động hợp lệ và tác động hiển thị/phần thưởng |
| M09-T029 | SF06 | Chốt ranh giới và sự kiện đến muộn | Xác định múi giờ, thời điểm xảy ra, cửa sổ chấp nhận và đổi múi giờ | M01; hợp đồng M04/M08 | Chính sách thời gian mùa | Cao | L | M09-T023, M09-T028 | Sự kiện biên kỳ có kết quả xác định; thời gian thiết bị không được tin tuyệt đối; cửa sổ được công bố |
| M09-T030 | SF06 | Thiết kế đóng kỳ và ảnh chụp | Khóa dữ liệu, chạy đối soát, tạo kết quả bất biến và công bố | M09-T028, M09-T029 | Quy trình đóng mùa | Cao | L | M09-T028, M09-T029 | Chạy lại không tạo ảnh chụp khác nếu đầu vào không đổi; đính chính có phiên bản/lý do; lịch sử được giữ |
| M09-T031 | SF07 | Đặc tả tiếp nhận biến động chỉ số | Chốt dữ liệu, định danh và phản hồi khi nguồn gửi chỉ số | M09-T023; nguồn M04/M08/M06 | Hợp đồng tổng hợp | Cao | L | M09-T023, M09-T029 | Gửi lại không cộng lặp; sai kỳ/nguồn bị từ chối có lý do; theo dõi từ chỉ số đến tổng điểm |
| M09-T032 | SF07 | Thiết kế tổng hợp và chạy lại | Quy định cập nhật tăng dần, tái tính và đối chiếu với nguồn | M09-T024–M09-T027, M09-T031 | Quy trình tổng hợp/đối soát | Cao | L | M09-T027, M09-T031 | Cùng đầu vào cho cùng tổng điểm; không mất cập nhật đồng thời; chênh lệch có trạng thái xử lý |
| M09-T033 | SF07 | Chốt quy tắc hòa điểm | Xác định đồng hạng hoặc tiêu chí phụ công bằng cho từng bảng | Mục đích bảng; chỉ số nguồn | Ma trận hòa điểm | Cao | M | M09-T022, M09-T024, M09-T025 | Tiêu chí phụ có thứ tự, không dùng dữ liệu riêng tư; trường hợp bằng hoàn toàn và thưởng được xử lý |
| M09-T034 | SF07 | Chốt cách gán vị trí | Quy định vị trí khi đồng hạng, phân trang và thay đổi dữ liệu | M09-T033; yêu cầu trải nghiệm | Quy tắc vị trí ổn định | Cao | M | M09-T033 | Các trang không trùng/bỏ trong cùng ảnh chụp; cùng điểm hiển thị đúng quy ước; cá nhân đối soát được |
| M09-T035 | SF07 | Chốt độ mới và cam kết cập nhật | Xác định cập nhật tức thời, theo lô hoặc lai và thời điểm hiển thị | Quy mô; nhu cầu trải nghiệm/vận hành | Cam kết độ trễ | Trung bình | L | M09-T031, M09-T032, M09-T034 | Người xem biết dữ liệu cập nhật lúc nào; lỗi/chậm có trạng thái; đóng kỳ ưu tiên tính nhất quán |
| M09-T036 | SF08 | Đặc tả bảng dẫn đầu và phân trang | Mô tả dữ liệu bảng, bộ lọc, kỳ và phạm vi | M09-T018, M09-T030, M09-T034 | Yêu cầu trải nghiệm bảng | Cao | M | M09-T018, M09-T030, M09-T034 | Hiển thị điểm, vị trí, kỳ, phiên bản giải thích và độ mới; quyền riêng tư áp dụng trước khi trả dữ liệu |
| M09-T037 | SF08 | Thiết kế vị trí cá nhân và lân cận | Cho người ngoài nhóm dẫn đầu vẫn thấy vị trí có ý nghĩa | M09-T034–M09-T036 | Yêu cầu vùng lân cận | Cao | M | M09-T034, M09-T036 | Trả vị trí hoặc lý do không đủ; vùng quanh người dùng ổn định; không suy ra hồ sơ ẩn |
| M09-T038 | SF08 | Thiết kế lịch sử mùa và thành tích | Hiển thị kết quả kỳ trước mà không ghi đè | Ảnh chụp M09-T030; riêng tư | Yêu cầu lịch sử xếp hạng | Trung bình | M | M09-T030, M09-T036 | Mỗi mùa có trạng thái/phiên bản; đính chính được nhận biết; người bị ẩn xử lý theo chính sách hiện hành |
| M09-T039 | SF08 | Đặc tả tín hiệu xã hội cho M10 | Chốt tín hiệu thay đổi hạng, lời mời, duyệt và kết quả mùa | Trạng thái nhóm/xếp hạng; lựa chọn nhận tin | Hợp đồng tín hiệu M09–M10 | Trung bình | M | M09-T012–M09-T016, M09-T030, M09-T035 | Tín hiệu có định danh chống lặp, mức ưu tiên và phạm vi; M09 không tự quyết kênh/giờ gửi |
| M09-T040 | SF09 | Đặc tả công thức điểm nhóm | Chốt tổng, trung bình hoặc chuẩn hóa theo quy mô và hoạt động | M09-T024–M09-T026; tư cách thành viên | Công thức nhóm | Cao | L | M09-T016, M09-T024, M09-T026 | Nhóm đông không có lợi thế vô hạn; mô phỏng nhóm nhỏ/lớn; giải thích được đóng góp hợp lệ |
| M09-T041 | SF09 | Chốt đóng góp khi chuyển nhóm | Quy định điểm trước/sau rời, gia nhập giữa mùa và khóa chuyển | Lịch sử thành viên; mùa | Chính sách tư cách theo kỳ | Cao | L | M09-T016, M09-T028, M09-T040 | Một đóng góp không thuộc hai nhóm; chuyển không tạo thưởng lặp; nhóm giải thể và thành viên mới rõ |
| M09-T042 | SF09 | Đặc tả ghi nhận và bàn giao quyền lợi | Chuyển cột mốc hợp lệ sang M07/M06 mà không tự cấp thưởng | Kết quả mùa; tư cách đủ điều kiện | Hợp đồng M09–M07/M06 | Trung bình | L | M09-T030, M09-T040, M09-T041 | Quyền lợi có định danh và ảnh chụp người đủ điều kiện; gửi lại an toàn; M09 không sửa tài sản |
| M09-T043 | SF10 | Xây dựng khung chống thao túng | Xác định tín hiệu cày điểm, tài khoản phụ, thông đồng và tuyển nhóm bất thường | Dữ liệu M04/M08; lịch sử nhóm | Danh mục tín hiệu/ngưỡng | Cao | L | M09-T024–M09-T026, M09-T040, M09-T041 | Mỗi tín hiệu có bằng chứng/giới hạn; không tự kết luận từ một dấu hiệu; có nhóm ngoại lệ hợp lệ |
| M09-T044 | SF10 | Thiết kế xử lý vi phạm và kháng nghị | Chốt đóng băng, loại điểm, đính chính và quyền xem xét | M09-T020, M09-T043; M11 | Quy trình vụ việc xếp hạng | Cao | L | M09-T020, M09-T043 | Quyết định có quyền, lý do, bằng chứng, thời hạn và kháng nghị; ảnh hưởng thưởng được bàn giao đối soát |
| M09-T045 | SF10 | Xây dựng khung chỉ số tác động | Đo cộng đồng, duy trì, chất lượng học, công bằng và tác động tiêu cực | Dữ liệu M04/M09; mục tiêu sản phẩm | Từ điển chỉ số và cảnh báo | Cao | L | M09-T024, M09-T035, M09-T043 | Có công thức, nguồn, phân nhóm và ngưỡng; không đồng nhất thứ hạng cao với học tốt; theo dõi áp lực/rời bỏ |
| M09-T046 | Toàn module | Xây dựng bộ nghiệm thu xuyên chức năng M09 | Bao phủ vòng đời nhóm, quyền, thành viên, riêng tư, mùa, hòa điểm và gian lận | Đầu ra M09-T001–M09-T045 | Bộ kịch bản nghiệm thu M09 | Cao | L | M09-T006, M09-T010, M09-T017, M09-T021, M09-T030, M09-T035, M09-T042, M09-T045 | Mỗi rủi ro có ca thành công, lỗi, lặp, đồng thời, quyền, riêng tư, biên mùa, đính chính và truy vết |
| M09-T047 | Toàn module | Hoàn thiện tài liệu bàn giao M09 | Hợp nhất quyết định, luồng, backlog, chỉ số và hợp đồng liên module | Toàn bộ đầu ra M09 | Gói tài liệu M09 được duyệt | Cao | M | M09-T046 | Không mâu thuẫn; 25 quyết định đã chốt được truy vết; M01/M04/M06/M07/M08/M10/M11 nhận đủ đầu vào |

## Các mốc backlog

| Mốc | Task | Kết quả |
|---|---|---|
| A — Nền tảng nhóm | M09-T001–M09-T010 | Có loại, vòng đời, vai trò và chuyển quyền |
| B — Thành viên và an toàn | M09-T011–M09-T021 | Có gia nhập/rời, riêng tư và kiểm duyệt |
| C — Định nghĩa và mùa | M09-T022–M09-T030 | Có chỉ số, công thức, điều kiện đủ và ảnh chụp |
| D — Tổng hợp và trải nghiệm | M09-T031–M09-T039 | Có thứ hạng xác định, phân trang và tín hiệu xã hội |
| E — Nhóm và công bằng | M09-T040–M09-T045 | Có điểm nhóm, chống thao túng và đo tác động |
| F — Sẵn sàng bàn giao | M09-T046–M09-T047 | M09 vượt cổng chất lượng |

## Ma trận quyết định → nhóm task

| Nhóm quyết định | Task chính | Điều kiện nghiệm thu bổ sung |
|---|---|---|
| M09-D001–M09-D010 | M09-T001–M09-T017, M09-T040–M09-T042 | Hai loại nhóm có quyền riêng; điều kiện tạo/tham gia/giới hạn thành viên; kế nhiệm và đóng nhóm không làm mất lịch sử |
| M09-D011–M09-D016 | M09-T018–M09-T027, M09-T036–M09-T039 | Hồ sơ tối thiểu; kiểm duyệt tự động; điểm học 50/30/20; phân dải trình độ; điều kiện xuất hiện trên bảng |
| M09-D017–M09-D021 | M09-T028–M09-T037 | Mùa 4 tuần; cửa sổ 24 giờ; công thức chỉ đổi mùa mới; đồng hạng đúng quyền lợi; cập nhật mục tiêu trong 5 phút |
| M09-D022–M09-D025 | M09-T040–M09-T047 | Điểm nhóm chuẩn hóa với 5–30 người; chuyển nhóm chờ 7 ngày; thưởng huy hiệu/vật phẩm giới hạn; đo cả tác động tích cực và tiêu cực |

## Điều kiện chặn

- Chưa có nguồn điểm M04/M08 có phiên bản và xử lý điều chỉnh thì chưa mở bảng chính thức.
- Chưa có ảnh kết quả sau cửa sổ 24 giờ và cấp thưởng đúng một lần từ M06 thì chưa công bố quyền lợi cuối mùa.
- Cơ chế nhóm chưa có kế nhiệm, khiếu nại và kiểm duyệt hậu kiểm thì chưa mở tạo nhóm cộng đồng.
