# Task backlog M07 — Nhiệm vụ và thành tựu

> Tất cả 25 quyết định M07 đã được chốt. `QUYET-DINH-MO.md` là đầu vào bắt buộc; S/M/L là độ phức tạp tương đối.

> Bảng dùng ngôn ngữ nghiệp vụ, có thể sao chép sang Jira, Trello hoặc Notion. Độ phức tạp: S (nhỏ), M (vừa), L (lớn).

| Task ID | Chức năng con | Tên task | Mô tả ngắn | Input mong đợi | Output mong đợi | Ưu tiên | Độ phức tạp | Task phụ thuộc | Definition of Done |
|---|---|---|---|---|---|---|---|---|---|
| M07-T001 | SF01 | Thống nhất từ điển mục tiêu | Chốt nghĩa của nhiệm vụ, thành tựu, tiến độ, hoàn thành, mở khóa, nhận thưởng và chu kỳ | README M07; thuật ngữ M03–M06 | Từ điển và ranh giới sở hữu | Cao | S | Không | Không còn thuật ngữ cốt lõi có hai nghĩa; module sở hữu từng khái niệm được chỉ rõ |
| M07-T002 | SF01 | Phân loại nhiệm vụ và thành tựu | Xác định các loại mục tiêu được hỗ trợ và mục đích học tập của từng loại | Nhu cầu sản phẩm; nguồn sự kiện | Danh mục loại mục tiêu | Cao | M | M07-T001 | Mỗi loại có nguồn, đơn vị đếm, chu kỳ, giới hạn và ví dụ hợp lệ/không hợp lệ |
| M07-T003 | SF01 | Đặc tả định nghĩa mục tiêu | Mô tả thông tin bắt buộc của một định nghĩa nghiệp vụ | M07-T002; chính sách thưởng | Mẫu định nghĩa chuẩn | Cao | M | M07-T002 | Bao phủ điều kiện, ngưỡng, hiệu lực, đối tượng, nội dung, phần thưởng và chủ sở hữu |
| M07-T004 | SF01 | Thiết kế vòng đời quyết định thay đổi | Chốt trạng thái nháp, duyệt, lên lịch, hoạt động và ngừng dùng | Quyền M11; mức ảnh hưởng | Sơ đồ vòng đời và quyền chuyển trạng thái | Cao | M | M07-T003 | Mỗi chuyển trạng thái có vai trò, điều kiện, kiểm toán và xử lý sai rõ ràng |
| M07-T005 | SF01 | Chốt phiên bản và ảnh chụp | Quy định thay đổi nào tạo phiên bản mới và dữ liệu nào được đóng băng khi giao | Vòng đời; quyền lợi người dùng | Chính sách phiên bản/ảnh chụp | Cao | L | M07-T003, M07-T004 | Nhiệm vụ đang chạy không đổi ngầm; có quy tắc hiệu lực và truy vết phiên bản |
| M07-T006 | SF01 | Xử lý ngừng dùng và xóa | Bảo toàn tiến độ/lịch sử khi định nghĩa không còn hoạt động | Chính sách lưu giữ; quan hệ tham chiếu | Quy tắc ngừng dùng | Cao | M | M07-T004, M07-T005 | Không xóa mất bằng chứng hoặc quyền đã phát sinh; hành vi với bản đang chạy được duyệt |
| M07-T007 | SF02 | Chốt tiêu chí đủ điều kiện | Xác định ai có thể nhận từng mục tiêu theo trình độ, tính năng và lịch sử | Hồ sơ M01; tiến độ M04; định nghĩa | Bộ quy tắc đủ điều kiện | Cao | M | M07-T003 | Mỗi loại điều kiện có nguồn dữ liệu, mặc định khi thiếu và lý do loại trừ |
| M07-T008 | SF02 | Thiết kế chiến lược lựa chọn | Chốt số lượng và cách chọn nhiệm vụ chung/cá nhân hóa | M07-T002, M07-T007; mục tiêu sản phẩm | Chính sách lựa chọn có thể đánh giá | Cao | L | M07-T007 | Không chọn mục tiêu bất khả thi/trùng quá mức; có tiêu chí đa dạng và công bằng |
| M07-T009 | SF02 | Xử lý người dùng mới và dữ liệu thiếu | Tạo tập nhiệm vụ an toàn khi chưa có lịch sử | Hồ sơ tối thiểu; danh mục mục tiêu | Luồng khởi đầu và phương án dự phòng | Cao | M | M07-T007, M07-T008 | Người mới luôn nhận tập khả thi hoặc trạng thái giải thích được; không suy diễn trình độ sai |
| M07-T010 | SF02 | Đảm bảo phân bổ duy nhất | Chốt quy tắc tạo một tập nhiệm vụ cho mỗi người/chu kỳ kể cả khi đồng thời hoặc chạy lại | Chu kỳ; chiến lược lựa chọn | Quy tắc nhận diện và chạy bù | Cao | L | M07-T005, M07-T008, M07-T009 | Tạo lại cho cùng chu kỳ không sinh bản trùng; ảnh chụp và lý do chọn được lưu |
| M07-T011 | SF03 | Lập danh mục sự kiện nguồn | Liệt kê sự kiện từ học, ôn, phát âm và thi đấu có thể đóng góp tiến độ | Tài liệu M03, M04, M05, M08 | Ma trận sự kiện–chủ sở hữu–mục tiêu | Cao | L | M07-T002 | Mỗi sự kiện có chủ nguồn, thời điểm, trạng thái xác nhận và phạm vi sử dụng |
| M07-T012 | SF03 | Đặc tả hợp đồng sự kiện nghiệp vụ | Chốt dữ liệu tối thiểu và ý nghĩa nhất quán của sự kiện | M07-T011; danh tính M01 | Hợp đồng dữ liệu liên module | Cao | L | M07-T011 | Có định danh duy nhất, người dùng, loại, thời điểm xảy ra/tiếp nhận, kết quả và phiên bản ngữ nghĩa |
| M07-T013 | SF03 | Thiết kế chống đếm lặp | Quy định cách nhận biết, lưu và trả kết quả cho sự kiện gửi lại | M07-T012 | Chính sách xử lý đúng một lần | Cao | L | M07-T012 | Cùng sự kiện gửi bao nhiêu lần cũng chỉ tạo một kết quả tiến độ; có bằng chứng đối soát |
| M07-T014 | SF03 | Xử lý thứ tự và đồng thời | Chốt hành vi khi sự kiện đến sai thứ tự hoặc nhiều sự kiện cập nhật cùng mục tiêu | Hợp đồng sự kiện; quy tắc tiến độ | Ma trận cạnh tranh/thứ tự | Cao | L | M07-T012, M07-T013 | Không mất tiến độ, vượt ngưỡng hoặc mở khóa lặp trong các kịch bản đồng thời |
| M07-T015 | SF03 | Xử lý sự kiện lỗi và thử lại | Định nghĩa trạng thái từ chối, chờ, thử lại và khu cách ly nghiệp vụ | Quy tắc xác thực; vận hành M11 | Quy trình phục hồi sự kiện | Trung bình | M | M07-T012, M07-T013 | Lỗi có lý do; sửa được dữ liệu phù hợp và xử lý lại mà không đếm lặp |
| M07-T016 | SF03 | Chốt đính chính và rút lại | Quy định nguồn sửa/hủy kết quả đã gửi và tác động đến tiến độ/hoàn thành | Chính sách M03/M05/M08 | Quy trình đính chính có kiểm toán | Cao | L | M07-T013, M07-T014 | Phân biệt lỗi kỹ thuật/gian lận; không sửa lịch sử âm thầm; phần thưởng đã cấp được chuyển sang xử lý M06 |
| M07-T017 | SF04 | Đặc tả đơn vị đếm học mới | Chốt khi nào một từ hoặc hoạt động học mới được tính | Kết quả M03; học liệu M02 | Quy tắc đếm học mới | Cao | M | M07-T011, M07-T012 | Nêu rõ học lại, bỏ dở, từ trùng, phiên hủy và giới hạn theo ngày |
| M07-T018 | SF04 | Đặc tả đơn vị đếm ôn tập | Chốt điều kiện một lượt ôn có giá trị và chống lặp nội dung dễ | Lịch/kết quả M04, M03 | Quy tắc đếm ôn tập | Cao | M | M07-T011, M07-T012 | Chỉ hoạt động ôn hợp lệ được tính; có quy tắc cho quá hạn, làm lại và phiên không hoàn thành |
| M07-T019 | SF04 | Đặc tả mục tiêu chất lượng | Chốt cách tính độ chính xác, chuỗi đúng hoặc chất lượng phiên | Kết quả đã chấm M03 | Quy tắc mục tiêu chất lượng | Cao | L | M07-T011, M07-T012 | Mẫu số, ngưỡng tối thiểu, làm tròn, câu bỏ qua và phiên quá ngắn được xác định |
| M07-T020 | SF04 | Đặc tả phát âm và thi đấu | Chốt điều kiện đếm phát âm, bắt thú hoặc kết quả thi đấu nếu áp dụng | M05, M08; chính sách công bằng | Quy tắc nguồn tùy chọn | Trung bình | L | M07-T011, M07-T012 | Chỉ kết quả cuối cùng hợp lệ được tính; có ngưỡng chất lượng và chống khai thác |
| M07-T021 | SF04 | Thiết kế trần và chống cày | Giới hạn đóng góp từ lặp hoạt động dễ nhưng không cản học thật | Các quy tắc đếm; dữ liệu hành vi | Chính sách trần và cảnh báo | Cao | L | M07-T017–M07-T020 | Có trần theo loại/chu kỳ, lý do giáo dục, ngoại lệ hợp lệ và chỉ số phát hiện bất thường |
| M07-T022 | SF05 | Chốt múi giờ và ranh giới ngày | Xác định múi giờ hiệu lực cùng thời điểm bắt đầu/kết thúc chu kỳ | Hồ sơ M01; yêu cầu trải nghiệm | Chính sách ngày nghiệp vụ | Cao | L | M07-T001 | Mỗi nhiệm vụ lưu ranh giới tuyệt đối; hiển thị theo người dùng; giờ mùa hè được bao phủ |
| M07-T023 | SF05 | Chốt chính sách đổi múi giờ | Ngăn đổi múi giờ tạo thêm chu kỳ hoặc kéo dài nhiệm vụ | M07-T022; lịch sử hồ sơ | Quy tắc chuyển múi giờ | Cao | M | M07-T022 | Có giới hạn, thời điểm áp dụng và xử lý đi lại; không nhận hai tập cho cùng ngày nghiệp vụ |
| M07-T024 | SF05 | Xử lý sự kiện đến muộn | Chốt dùng thời điểm xảy ra hay tiếp nhận và cửa sổ chấp nhận | M07-T012, M07-T022 | Chính sách sự kiện muộn | Cao | L | M07-T012, M07-T022 | Kịch bản mất mạng qua nửa đêm, sai giờ thiết bị và chạy bù có kết quả xác định |
| M07-T025 | SF05 | Đặc tả kết thúc và tạo chu kỳ | Chốt khóa tiến độ, lưu kết quả, tạo kỳ mới và chạy lại an toàn | M07-T010, M07-T022–M07-T024 | Luồng đóng/mở chu kỳ | Cao | M | M07-T010, M07-T024 | Không mất/chồng kỳ; xử lý lại không đổi kết quả; nhiệm vụ chưa nhận có trạng thái rõ |
| M07-T026 | SF06 | Chốt chuyển trạng thái hoàn thành | Định nghĩa điều kiện và bằng chứng khi nhiệm vụ đạt ngưỡng | Quy tắc đếm; chu kỳ | Sơ đồ trạng thái tiến độ | Cao | M | M07-T014, M07-T021, M07-T025 | Hoàn thành chỉ một lần; có thời điểm/bằng chứng; không vượt ngưỡng; đồng thời được bao phủ |
| M07-T027 | SF06 | Chốt mở khóa thành tựu và nhiều bậc | Đánh giá cột mốc dài hạn sau sự kiện hoặc hoàn thành | Danh mục thành tựu; tiến độ | Luồng mở khóa/bậc | Cao | L | M07-T026 | Đạt nhiều bậc có thứ tự rõ; mỗi bậc chỉ mở một lần; lịch sử không bị ghi đè |
| M07-T028 | SF06 | Chốt hồi tố thành tựu | Quy định dùng dữ liệu lịch sử khi tạo thành tựu hoặc đổi điều kiện | Dữ liệu M03–M05; chất lượng lịch sử | Chính sách hồi tố | Trung bình | L | M07-T005, M07-T027 | Nêu rõ nguồn đủ tin cậy, cửa sổ, trường hợp không thể tính và cách truyền thông công bằng |
| M07-T029 | SF06 | Xử lý đính chính sau hoàn thành | Chốt giữ, thu hồi hay đánh dấu tranh chấp khi kết quả nguồn bị sửa | M07-T016, M07-T026; M06 | Ma trận điều chỉnh sau hoàn thành | Cao | L | M07-T016, M07-T026 | Không xóa dấu vết; quyền lợi và yêu cầu thu hồi được đối soát; có quyền phê duyệt |
| M07-T030 | SF07 | Chốt điều kiện nhận thưởng | Xác định nhận thủ công/tự động, thời hạn, tài khoản hợp lệ và trạng thái | Chính sách sản phẩm; M01, M06 | Quy tắc quyền nhận | Cao | M | M07-T026, M07-T027 | Mỗi trạng thái cho biết hành động hợp lệ; người dùng không mất quyền do lỗi hệ thống |
| M07-T031 | SF07 | Đặc tả ảnh chụp gói thưởng | Đóng băng nội dung thưởng gắn với nhiệm vụ/thành tựu đã giao | M07-T005; danh mục M06 | Mẫu gói thưởng phiên bản hóa | Cao | M | M07-T005, M07-T030 | Thay đổi sau đó không làm đổi quyền đã hứa; tài sản ngừng dùng có phương án xử lý |
| M07-T032 | SF07 | Đặc tả bàn giao phần thưởng cho M06 | Chốt yêu cầu, phản hồi và quyền sở hữu trạng thái giữa hai module | M06-T011–M06-T017; M07-T031 | Hợp đồng nghiệp vụ M07–M06 | Cao | L | M07-T031, M06-T017 | M07 chỉ gửi quyền nhận; M06 là nguồn sự thật tài sản; trạng thái và lý do lỗi ánh xạ đầy đủ |
| M07-T033 | SF07 | Đảm bảo nhận thưởng đúng một lần | Dùng một định danh ổn định cho mỗi quyền nhận và mọi lần thử lại | M07-T032 | Chính sách chống cấp lặp | Cao | L | M07-T032 | Nhấn lặp, hai thiết bị, mất phản hồi và gửi lại đều không cấp thêm hoặc đánh dấu sai |
| M07-T034 | SF07 | Thiết kế phục hồi và đối soát thưởng | Xử lý chờ lâu, lỗi một phần, phản hồi mất và sai lệch trạng thái | Trạng thái M06; nhật ký M07 | Quy trình thử lại/đối soát | Cao | L | M07-T032, M07-T033 | Có trạng thái chờ/thành công/thất bại cần xử lý; tự động và thủ công không xung đột; không mất quyền |
| M07-T035 | SF08 | Chốt mô hình thành tựu dài hạn | Xác định chủ đề, bậc, chu kỳ, tiến độ và quan hệ giữa các cột mốc | M07-T002, M07-T003 | Khung thành tựu | Cao | M | M07-T002, M07-T003 | Mỗi thành tựu có giá trị học tập, nguồn, đơn vị, bậc, hiệu lực và phần thưởng rõ |
| M07-T036 | SF08 | Chốt khởi tạo cho người dùng cũ/mới | Bảo đảm thành tựu mới được theo dõi đúng đối tượng | M07-T007, M07-T028, M07-T035 | Chính sách liên kết và chạy bù | Cao | L | M07-T028, M07-T035 | Không bỏ sót người dùng cũ; chạy lại không tạo bản trùng; trường hợp thiếu lịch sử được ghi nhận |
| M07-T037 | SF08 | Chốt thành tựu ẩn và mùa vụ | Quy định mức tiết lộ, thời hạn và bảo toàn thành tích sau mùa | Nội dung sản phẩm; chính sách riêng tư | Quy tắc ẩn/mùa vụ | Trung bình | M | M07-T035 | Không rò điều kiện ẩn ngoài chủ ý; hết mùa không xóa thành tích; phần thưởng quá hạn rõ |
| M07-T038 | SF09 | Đặc tả danh sách nhiệm vụ hôm nay | Mô tả dữ liệu và trạng thái cần cho người học hiểu và hành động | Ảnh chụp nhiệm vụ; chu kỳ; tiến độ | Yêu cầu trải nghiệm nhiệm vụ | Cao | M | M07-T010, M07-T025, M07-T026 | Có mục tiêu, tiến độ, thời hạn, cách tính, phần thưởng và trạng thái nhận; không có dữ liệu mâu thuẫn |
| M07-T039 | SF09 | Đặc tả danh sách thành tựu | Mô tả đang theo dõi, đã mở, đã nhận và nội dung ẩn | M07-T027, M07-T035–M07-T037 | Yêu cầu trải nghiệm thành tựu | Trung bình | M | M07-T027, M07-T037 | Tiến độ/bậc/trạng thái nhất quán; nội dung ẩn đúng chính sách; ngừng dùng vẫn có lịch sử |
| M07-T040 | SF09 | Thiết kế lịch sử và giải thích tiến độ | Cho người học và hỗ trợ tra cứu vì sao được/không được tính | Sự kiện, tiến độ, thưởng | Mô hình lịch sử và thông điệp giải thích | Trung bình | L | M07-T013, M07-T026, M07-T034 | Phân biệt sự kiện, tiến độ, hoàn thành và nhận thưởng; dữ liệu nhạy cảm được giới hạn |
| M07-T041 | SF09 | Đặc tả tín hiệu cho M10 | Chốt các thời điểm M07 phát tín hiệu hoàn thành, sắp hết hạn và thưởng | Trạng thái M07; lựa chọn nhận tin | Hợp đồng tín hiệu M07–M10 | Trung bình | M | M07-T025, M07-T026, M07-T034 | Tín hiệu có định danh chống lặp, thời điểm, mức ưu tiên; M07 không tự quyết kênh gửi |
| M07-T042 | SF10 | Chốt quyền và kiểm toán quản trị | Phân tách tạo, duyệt, phát hành, ngừng và xem dữ liệu | Vai trò M11; vòng đời M07 | Ma trận quyền và nhật ký bắt buộc | Cao | M | M07-T004 | Thao tác nhạy cảm có phê duyệt phù hợp; ghi người, thời điểm, lý do và trước/sau |
| M07-T043 | SF10 | Xây dựng khung chỉ số hiệu quả | Đo giao, hoàn thành, chất lượng học, duy trì và chi phí thưởng | Mục tiêu sản phẩm; dữ liệu M04, M06 | Từ điển chỉ số và chiều phân tích | Cao | L | M07-T008, M07-T021, M07-T034 | Mỗi chỉ số có công thức, cửa sổ, nguồn và giới hạn; không đồng nhất tương tác với học tốt |
| M07-T044 | SF10 | Thiết kế giám sát và thử nghiệm | Theo dõi bất thường, công bằng và tác động khi đổi mục tiêu | M07-T042, M07-T043 | Quy trình cảnh báo/thử nghiệm/quay lại | Trung bình | L | M07-T042, M07-T043 | Có ngưỡng chi phí, gian lận, hoàn thành, chất lượng; nhóm so sánh và tiêu chí dừng rõ |
| M07-T045 | Toàn module | Xây dựng bộ nghiệm thu xuyên chức năng M07 | Bao phủ định nghĩa, phân bổ, sự kiện, chu kỳ, hoàn thành, thưởng và vận hành | Đầu ra M07-T001–M07-T044 | Bộ kịch bản nghiệm thu M07 | Cao | L | M07-T006, M07-T010, M07-T016, M07-T021, M07-T025, M07-T029, M07-T034, M07-T044 | Mỗi rủi ro có ca thành công, lỗi, lặp, đồng thời, biên thời gian, quyền, phục hồi và truy vết |
| M07-T046 | Toàn module | Hoàn thiện tài liệu bàn giao M07 | Hợp nhất quyết định, luồng, backlog, chỉ số và hợp đồng liên module | Toàn bộ đầu ra M07 | Gói tài liệu M07 được duyệt | Cao | M | M07-T045 | Không mâu thuẫn; 25 quyết định đã chốt được truy vết; M01/M03/M04/M05/M06/M08/M10/M11 nhận đủ đầu vào |

## Các mốc backlog

| Mốc | Task | Kết quả |
|---|---|---|
| A — Nền tảng mục tiêu | M07-T001–M07-T010 | Có định nghĩa, phiên bản và phân bổ ổn định |
| B — Sự kiện và tiến độ | M07-T011–M07-T021 | Có hợp đồng nguồn, chống lặp và quy tắc đếm |
| C — Chu kỳ và hoàn thành | M07-T022–M07-T029 | Có ranh giới thời gian, mở khóa và đính chính |
| D — Thưởng và thành tựu | M07-T030–M07-T037 | Có bàn giao M06 đúng một lần và tiến độ dài hạn |
| E — Trải nghiệm và vận hành | M07-T038–M07-T044 | Có lịch sử, thông báo, chỉ số và cảnh báo |
| F — Sẵn sàng bàn giao | M07-T045–M07-T046 | M07 vượt cổng chất lượng |

## Ma trận quyết định → nhóm task

| Nhóm quyết định | Task chính | Điều kiện nghiệm thu bổ sung |
|---|---|---|
| M07-D001–M07-D004 | M07-T001–M07-T010 | Ba nhiệm vụ/ngày, ít nhất một mục tiêu học/ôn, phân nhóm phù hợp và không giao nhiệm vụ bất khả thi; chưa cho đổi nhiệm vụ |
| M07-D005–M07-D009 | M07-T011–M07-T025 | Ngày theo múi giờ hồ sơ; đổi múi giờ áp dụng ngày sau; sự kiện muộn có cửa sổ/điều chỉnh; không đếm trùng ý nghĩa |
| M07-D010–M07-D019 | M07-T014–M07-T037 | Chỉ sự kiện hợp lệ từ module sở hữu; ảnh chụp định nghĩa; thành tựu hồi tố chỉ khi dữ liệu đủ; mùa vụ tách tiến độ |
| M07-D020–M07-D025 | M07-T030–M07-T046 | Nhận thưởng chủ động, ân hạn 7 ngày; gửi lại cùng kết quả; điều chỉnh không tạo âm; đo cả ghi nhớ và tín hiệu mệt mỏi |

## Điều kiện chặn

- Chưa thống nhất định danh sự kiện với M03/M04/M05/M08 và xử lý đúng một lần thì chưa mở tiến độ nhiệm vụ.
- Chưa có xác nhận/đối soát từ M06 thì không hiển thị trạng thái thưởng đã nhận.
- Nhiệm vụ tối ưu thao tác nhưng làm giảm chất lượng hoặc tăng mệt mỏi phải được dừng/điều chỉnh ở lượt giao mới.
