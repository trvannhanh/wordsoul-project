# Phân tích chuyên sâu M05 — Luyện phát âm

## 1. Mục tiêu và phạm vi

M05 giúp người học luyện phát âm một từ theo mẫu, nhận phản hồi dễ hiểu và theo dõi cải thiện mà không biến một điểm số từ nhà cung cấp thành phán xét tuyệt đối về năng lực. Module phải bảo vệ dữ liệu giọng nói, xử lý khác biệt giọng vùng miền/thiết bị và chỉ tác động sang M04/M07 theo chính sách đã xác nhận.

### Trong phạm vi

- Chọn từ phù hợp để luyện và cung cấp mẫu nghe/cách đọc.
- Xin quyền, tiếp nhận, kiểm tra và gửi bản ghi giọng nói để đánh giá.
- Chuẩn hóa kết quả độ chính xác, trôi chảy, đầy đủ, tổng thể và chi tiết âm.
- Phân loại kết quả, tạo phản hồi, cho thử lại và theo dõi chuỗi kết quả.
- Lưu lịch sử/thống kê ở mức được phép.
- Phát hành kết quả đủ điều kiện cho M04, M06 và M07.
- Quản lý đồng ý, thời hạn lưu, truy vết, chi phí và lỗi đánh giá.

### Ngoài phạm vi

- Sở hữu nội dung từ/cách đọc chuẩn; M02 là nguồn học liệu.
- Tự tính lịch ôn hoặc trạng thái thành thạo dài hạn; M04 sở hữu.
- Tự duy trì số dư kinh nghiệm/phần thưởng; M06 sở hữu.
- Vận hành nhà cung cấp xử lý giọng nói; M12 sở hữu ranh giới tích hợp.

## 2. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Chọn danh sách từ luyện dựa trên trạng thái nhớ, số lần phát âm sai và lần luyện gần nhất.
- Nhận bản ghi, chuyển đổi định dạng khi cần và gửi dịch vụ ngoài đánh giá theo từ tham chiếu.
- Nhận điểm chính xác, trôi chảy, đầy đủ, tổng thể và chi tiết theo âm.
- Phân loại kết quả theo các ngưỡng, lưu lần thử, chuỗi kết quả hoàn hảo và kinh nghiệm đã trao.
- Áp tác động trực tiếp sang tiến độ ôn và có lịch sử/thống kê phát âm.

### Khoảng trống và rủi ro

- Chưa thấy mã chống gửi lặp; thử lại do mất mạng có thể ghi nhiều lần và trao thưởng lặp.
- Không thấy bước kiểm tra tiếng ồn, im lặng, cắt âm hoặc sai từ trước khi gửi đánh giá.
- Ngưỡng chung có thể thiên lệch theo giọng vùng miền, tuổi, từ khó hoặc chất lượng thiết bị.
- Dữ liệu phản hồi thô từ nhà cung cấp được lưu nhưng mục đích, thời hạn và mức nhạy cảm chưa rõ.
- Chưa rõ bản ghi âm gốc có được lưu hay chỉ xử lý tạm thời.
- Tác động M04 và cấp kinh nghiệm đang gắn trong cùng luồng; lỗi từng phần có thể tạo kết quả không nhất quán.
- Chưa có bằng chứng kiểm thử nghiệp vụ riêng cho phát âm.

## 3. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Chọn từ luyện | Đề xuất từ có giá trị luyện cao | Tiêu chí chọn, ưu tiên, giới hạn và loại trừ | Tính trạng thái nhớ | M02, M04 |
| SF02 | Trình bày mẫu phát âm | Cho người học hiểu mục tiêu trước khi thu | Từ, nghĩa, phiên âm, mẫu nghe, biến thể | Tạo học liệu gốc | M02, M12 |
| SF03 | Đồng ý và thu âm | Thu dữ liệu hợp lệ, minh bạch và tối thiểu | Xin quyền, hướng dẫn, ghi/dừng/nghe lại, đồng ý | Quyền hệ điều hành | M01, M12 |
| SF04 | Kiểm tra chất lượng bản ghi | Loại đầu vào không thể đánh giá | Định dạng, dung lượng, thời lượng, im lặng, nhiễu, cắt âm | Đánh giá phát âm học thuật | M12 |
| SF05 | Đánh giá và chuẩn hóa kết quả | Nhận kết quả ngoài và chuyển thành dữ liệu ổn định | Gửi đánh giá, thời hạn, lỗi, điểm và phiên bản | Vận hành nhà cung cấp | M02, M12 |
| SF06 | Phân loại và phản hồi | Đưa phản hồi có thể hành động, không gây nản | Ngưỡng, nhãn, chi tiết âm, lời khuyên | Nội dung khóa học phát âm dài hạn | M11 |
| SF07 | Lần thử, thử lại và chống lặp | Ghi đúng một lần và hỗ trợ luyện lặp có kiểm soát | Mã lần thử, trạng thái, gửi lại, chuỗi thành tích | Sở hữu phần thưởng | M06 |
| SF08 | Lịch sử và thống kê | Cho người học thấy xu hướng cải thiện | Lịch sử theo từ, tổng hợp, quyền xem | Phân tích học thuật dài hạn ngoài M05 | M01, M11 |
| SF09 | Bàn giao tiến độ, nhiệm vụ và thưởng | Phát kết quả nhất quán cho module sở hữu | Sự kiện kết quả, điều kiện tác động, xử lý lại | Tự cập nhật M04/M06/M07 | M04, M06, M07 |
| SF10 | Quyền riêng tư và vận hành | Bảo vệ giọng nói, chi phí và độ tin cậy | Lưu giữ, xóa, chỉ số, cảnh báo, suy giảm | Chính sách nền tảng ngoài M05 | M01, M11, M12 |

## 4. Phân tích chi tiết

### SF01 — Chọn từ luyện

**Business flow:** M05 nhận danh sách từ người dùng có quyền học; kết hợp tín hiệu từ M04 và lịch sử phát âm; loại nội dung thiếu chuẩn đọc; sắp xếp theo ưu tiên; trả danh sách có lý do gợi ý.

**Edge case:** người dùng mới; không có tiến độ; từ chưa từng luyện; tất cả từ thiếu âm thanh; từ bị thu hồi; cùng từ nhiều nghĩa/biến thể; danh sách lặp một nhóm khó.

**DoD:** tiêu chí chọn và loại trừ rõ; có phương án người dùng mới; mỗi gợi ý có lý do; chỉ dùng dữ liệu được phép; nội dung bị thu hồi không tiếp tục xuất hiện.

### SF02 — Trình bày mẫu phát âm

**Business flow:** người dùng chọn từ; M05 hiển thị mặt chữ, nghĩa/cách dùng, phiên âm, biến thể mục tiêu và mẫu nghe; người dùng có thể nghe lại trước khi thu.

**Edge case:** nhiều giọng chuẩn; âm thanh lỗi; phiên âm thiếu; từ đồng hình khác nghĩa; mẫu nghe không khớp văn bản; người dùng nghe mẫu quá nhiều rồi ghi âm phát lại.

**DoD:** mẫu khớp đúng phiên bản/biến thể; có phương án thiếu tài sản; người dùng biết giọng mục tiêu; nghe mẫu không bị tính là lần thử.

### SF03 — Đồng ý và thu âm

**Business flow:** M05 giải thích mục đích xử lý; xin quyền thiết bị và đồng ý cần thiết; hướng dẫn môi trường; người dùng ghi, dừng, nghe lại và chọn gửi hoặc hủy; chỉ dữ liệu đã chọn gửi mới bước vào đánh giá.

**Edge case:** từ chối/rút quyền; quyền bị tắt giữa lần thu; cuộc gọi làm gián đoạn; ghi nhầm; trẻ em; bản ghi chứa lời nói ngoài từ mục tiêu; người dùng hủy sau khi gửi.

**DoD:** đồng ý gắn phiên bản/mục đích; từ chối vẫn dùng được phần khác; hủy trước gửi không tạo lần thử; trạng thái thu rõ và có hướng dẫn quyền riêng tư.

### SF04 — Kiểm tra chất lượng bản ghi

**Business flow:** M05 kiểm tra loại, kích thước, thời lượng và khả năng đọc; phát hiện im lặng/quá nhỏ/cắt âm/nhiễu ở mức phù hợp; yêu cầu thu lại hoặc gửi đánh giá.

**Edge case:** định dạng giả; tệp quá lớn; bản ghi cực ngắn/dài; âm lượng cao gây vỡ; nhiều người nói; phát lại âm mẫu; chuyển đổi thất bại.

**DoD:** dữ liệu không hợp lệ không gửi ra ngoài/không tính lần thử; lỗi có hướng khắc phục; giới hạn đo được; kiểm tra không lưu dữ liệu lâu hơn cần thiết.

### SF05 — Đánh giá và chuẩn hóa kết quả

**Business flow:** M05 tạo yêu cầu với văn bản/biến thể tham chiếu và bản ghi; M12 chuyển tới nhà cung cấp; kết quả được kiểm tra đầy đủ, chuẩn hóa điểm/chi tiết âm/phiên bản; lỗi hoặc chậm trả trạng thái thử lại phù hợp.

**Edge case:** quá thời gian; nhà cung cấp trả thành công nhưng thiếu điểm; điểm ngoài khoảng; nhiều kết quả cho một lần thử; dịch vụ không hỗ trợ biến thể; kết quả “0” do lỗi bị hiểu là phát âm sai.

**DoD:** lỗi kỹ thuật không phân loại người học là sai; kết quả có phiên bản nhà cung cấp/chính sách; gửi lại cùng lần thử không tạo đánh giá nghiệp vụ mới; có phương án suy giảm.

### SF06 — Phân loại và phản hồi

**Business flow:** M05 áp chính sách ngưỡng theo ngữ cảnh; tạo nhãn kết quả; chọn tối đa vài điểm cần cải thiện; đưa phản hồi tích cực, cụ thể và gợi ý nghe/thử lại.

**Edge case:** điểm tổng cao nhưng một âm rất thấp; độ chính xác cao nhưng thiếu âm; từ một âm; khác biệt giọng hợp lệ; nhiều lỗi gây phản hồi quá tải; thay đổi ngưỡng làm lịch sử khó so sánh.

**DoD:** phản hồi phân biệt điểm nhà cung cấp và diễn giải WordSoul; không dùng ngôn ngữ phán xét; có phiên bản ngưỡng; lịch sử giải thích được; phản hồi ưu tiên hành động cụ thể.

### SF07 — Lần thử, thử lại và chống lặp

**Business flow:** mỗi lần gửi có mã duy nhất và trạng thái; hệ thống xử lý đúng một kết quả; người dùng xem phản hồi rồi thử lại; chuỗi thành tích được cập nhật từ các lần hợp lệ; giới hạn/tần suất được áp dụng.

**Edge case:** gửi hai lần do mất mạng; cùng mã khác bản ghi; hai thiết bị; thử lại quá nhanh; lỗi kỹ thuật; xóa lịch sử; chuỗi thành tích bị tăng bởi gửi lặp.

**DoD:** cùng mã trả cùng kết quả; lỗi kỹ thuật không tính thất bại học thuật; chuỗi/phần thưởng không tăng lặp; giới hạn rõ; mọi chuyển trạng thái truy vết được.

### SF08 — Lịch sử và thống kê

**Business flow:** người dùng xem lịch sử theo từ và thống kê tổng; hệ thống hiển thị xu hướng theo cùng chính sách hoặc đánh dấu thay đổi phiên bản; người dùng lọc theo thời gian/kết quả.

**Edge case:** ít dữ liệu; chính sách điểm thay đổi; mục từ bị thu hồi; xóa tài khoản; dữ liệu thô thiếu; nhiều biến thể phát âm cùng từ.

**DoD:** chỉ chủ tài khoản/quản trị đúng quyền xem; thống kê có công thức; không so sánh trực tiếp điểm không tương thích; dữ liệu thiếu không tạo kết luận sai.

### SF09 — Bàn giao tiến độ, nhiệm vụ và thưởng

**Business flow:** sau khi kết quả hợp lệ được chốt, M05 tạo một sự kiện duy nhất; M04 quyết định tác động ghi nhớ, M07 cập nhật mục tiêu, M06 quyết định/cấp kinh nghiệm; lỗi module nhận được xử lý lại.

**Edge case:** M04 thành công nhưng M06 thất bại; gửi sự kiện lặp; chính sách đổi giữa đánh giá và xử lý; tài khoản bị khóa; kết quả bị hủy do lỗi nhà cung cấp.

**DoD:** M05 không trực tiếp sở hữu tiến độ/số dư/nhiệm vụ; sự kiện có mã và phiên bản; nhận lại an toàn; kết quả kỹ thuật lỗi không phát tác động; trạng thái bàn giao quan sát được.

### SF10 — Quyền riêng tư và vận hành

**Business flow:** dữ liệu âm thanh được xử lý theo đồng ý và mục đích; chỉ dữ liệu tối thiểu gửi ngoài; bản ghi/thô được xóa hoặc giữ theo chính sách; chỉ số theo dõi độ trễ, lỗi, chi phí và phân bố điểm; bất thường tạo cảnh báo.

**Edge case:** yêu cầu xóa trong khi đang đánh giá; nhà cung cấp lưu dữ liệu; dữ liệu thô chứa thông tin ngoài mục tiêu; chi phí tăng đột biến; nhà cung cấp ngừng hoạt động; cảnh báo thiên lệch theo nhóm người dùng.

**DoD:** có bản đồ dữ liệu và thời hạn; quyền xem/xóa rõ; không ghi bản ghi hoặc phản hồi thô quá mục đích; có ngưỡng chi phí/chất lượng và phương án khi dịch vụ gián đoạn.

## 5. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Chọn và chuẩn bị bài luyện phù hợp | SF01, SF02 | M05-T001–M05-T008 |
| Thu và kiểm tra dữ liệu an toàn | SF03, SF04 | M05-T009–M05-T016 |
| Đánh giá và phản hồi đáng tin | SF05, SF06 | M05-T017–M05-T025 |
| Thử lại, lịch sử và thống kê đúng | SF07, SF08 | M05-T026–M05-T033 |
| Bàn giao và vận hành có kiểm soát | SF09, SF10 | M05-T034–M05-T041 |
| Nghiệm thu và bàn giao | SF01–SF10 | M05-T042–M05-T043 |

## 6. Thứ tự thực hiện đề xuất

1. Chốt từ điển, biến thể mục tiêu, tiêu chí chọn từ và dữ liệu tham chiếu.
2. Chốt đồng ý, thu âm, kiểm tra chất lượng và vòng đời dữ liệu.
3. Chốt hợp đồng đánh giá, xử lý lỗi, ngưỡng và phản hồi.
4. Chốt chống lặp, thử lại, lịch sử và thống kê.
5. Chốt ranh giới M04/M06/M07, chỉ số vận hành và bộ nghiệm thu.

## 7. Cơ sở quyết định đã chốt

Tất cả 24 quyết định M05 đã được chốt. Hệ thống hỗ trợ Anh–Mỹ và Anh–Anh; người học chọn mặc định và có thể đổi trước hoạt động. Tiến độ phát âm tách khỏi nhớ nghĩa. Chỉ kết quả đạt mới tính nhiệm vụ; thưởng chỉ cấp cho lần đạt hợp lệ đầu tiên của mỗi nghĩa trong ngày.

Bản ghi âm và phản hồi thô của nhà cung cấp phải xóa ngay hoặc chậm nhất 24 giờ; kết quả chuẩn hóa giữ theo vòng đời tài khoản. Người dùng có thể tự xóa toàn bộ lịch sử phát âm chuẩn hóa sau xác nhận. Nhân sự hỗ trợ chỉ được truy cập dữ liệu thô theo từng vụ, sau khi người dùng đồng ý, có lý do và nhật ký.

Mỗi người tối đa 50 lượt chấm hợp lệ/ngày và 10 lượt cho mỗi nghĩa/ngày; lỗi kỹ thuật không bị tính. Lỗi tạm thời được thử lại tối đa hai lần trên cùng lượt. Khi dịch vụ ngừng hoạt động, vẫn cho nghe và tự luyện nhưng không tạo điểm, tiến độ, nhiệm vụ hay phần thưởng.

## 8. Rủi ro còn hiệu lực và điều kiện phát hành

- Điểm nhà cung cấp có thể bị hiểu sai là thước đo tuyệt đối và làm người học nản.
- Thiếu chống gửi lặp có thể tăng lịch sử, chuỗi thành tích và phần thưởng nhiều lần.
- Ghi dữ liệu phản hồi thô lâu dài làm tăng rủi ro riêng tư mà chưa chắc có giá trị học thuật.
- Kết quả kỹ thuật thất bại không được phân biệt có thể làm giảm tiến độ M04 sai.
- Một ngưỡng chung cho mọi giọng/từ/thiết bị có nguy cơ thiên lệch.

- Người chưa thành niên chỉ được gửi giọng nói ra bên ngoài sau xác nhận đồng ý người giám hộ; M01 hiện chưa có bằng chứng hoặc quan hệ này, vì vậy đây là điều kiện chặn phát hành chấm bên ngoài cho nhóm tuổi đó.
- Ngưỡng đạt phải được phiên bản hóa theo biến thể giọng, trình độ và độ khó; thống kê qua thay đổi nhà cung cấp/ngưỡng chỉ được nối khi chứng minh tương đương.

Không còn quyết định mở trong M05; `QUYET-DINH-MO.md` là nguồn chi tiết.
