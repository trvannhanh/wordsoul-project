# Phân tích chuyên sâu M08 — Đấu trường và phòng thử thách

## 1. Mục tiêu và phạm vi

M08 biến kiến thức từ vựng thành thử thách có luật rõ ràng, công bằng và có thể phục hồi. Module hỗ trợ cả hành trình đấu với hệ thống để kiểm chứng năng lực và đấu người chơi để tăng động lực, nhưng kết quả học thuật phải quan trọng hơn lợi thế sưu tập hay tốc độ thiết bị.

### Trong phạm vi

- Định nghĩa chế độ, luật trận, phiên bản cân bằng và vòng đời trận.
- Điều kiện mở khóa, tiến độ và thử lại phòng thử thách PvE.
- Xác minh đội hình, ảnh chụp thú cưng/hiệu ứng và điều kiện tham gia.
- Tạo phòng mời, tham gia phòng và ghép đối thủ phù hợp.
- Sẵn sàng, chọn câu hỏi, giới hạn thời gian, nhận câu trả lời và giải quyết vòng.
- Đồng bộ trạng thái, mất kết nối, quay lại, đầu hàng, hủy và kết thúc đúng một lần.
- Xác định kết quả trận, biến động điểm thi đấu gốc và điều kiện được thưởng.
- Lịch sử, xem lại, tranh chấp, chống gian lận, quan sát và cân bằng.

### Ngoài phạm vi

- Sở hữu hoặc biên soạn học liệu từ vựng.
- Quản lý quyền sở hữu, cấp độ hay kho thú cưng/vật phẩm.
- Trực tiếp thay đổi tài sản phần thưởng.
- Tổng hợp bảng xếp hạng theo mùa/phạm vi hoặc gửi thông báo ra thiết bị.

## 2. Từ điển trạng thái nghiệp vụ đề xuất

| Trạng thái | Ý nghĩa | Chuyển tiếp chính |
|---|---|---|
| Đã tạo | Trận/phòng có người khởi tạo nhưng chưa đủ điều kiện bắt đầu | Chờ đối thủ, Chờ sẵn sàng, Đã hủy |
| Chờ đối thủ | Phòng mời hoặc hàng chờ chưa có cặp hợp lệ | Chờ sẵn sàng, Hết hạn, Đã hủy |
| Chờ sẵn sàng | Đã xác định người chơi/đội hình, chờ xác nhận cuối | Đang diễn ra, Hết hạn, Đã hủy |
| Đang diễn ra | Luật và ảnh chụp đã khóa, trận nhận hành động hợp lệ | Tạm gián đoạn, Đang kết thúc |
| Tạm gián đoạn | Một bên mất kết nối trong thời gian ân hạn | Đang diễn ra, Đang kết thúc |
| Đang kết thúc | Đã có lý do kết thúc, đang chốt kết quả và các bàn giao | Hoàn thành, Cần đối soát |
| Hoàn thành | Kết quả cuối đã được chốt một lần | Không chuyển, trừ đính chính có phiên bản |
| Đã hủy/Hết hạn | Trận không bắt đầu hoặc bị hủy hợp lệ, không có kết quả thắng thua | Không chuyển |
| Bị bỏ dở | Trận PvE/PvP đã bắt đầu nhưng không thể hoàn tất theo chính sách | Cần đối soát hoặc kết quả đã quy định |
| Cần đối soát | Kết quả, điểm hoặc thưởng không nhất quán/có tranh chấp | Hoàn thành hoặc bị vô hiệu theo quyết định |

## 3. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Có tiến trình phòng thử thách với trạng thái khóa, mở, đã chinh phục, số lần thử, thời gian thử gần nhất và điểm tốt nhất.
- Phòng PvE có điều kiện kinh nghiệm, số từ đạt trạng thái nhớ, số câu hỏi, thời gian chờ sau thua, huy hiệu và đội đối thủ.
- Có đội hình ba thú, trạng thái máu, loại, sát thương, câu hỏi, điểm và kết quả từng vòng.
- PvP hỗ trợ tạo/tham gia bằng mã phòng, danh sách phòng chờ và hàng chờ ghép theo điểm thi đấu.
- Hai người chơi xác nhận sẵn sàng; câu trả lời được giữ đến khi đủ hai bên; có xử lý mất kết nối thành bỏ cuộc.
- Có biến động điểm thi đấu, phân hạng, lịch sử trận và chi tiết vòng.

### Khoảng trống và rủi ro

- Trạng thái hiện tại chưa biểu đạt chờ sẵn sàng, gián đoạn, đang kết thúc, hết hạn và cần đối soát.
- Thời gian trả lời do phía người chơi báo có thể bị thao túng; độ trễ mạng và hạn vòng chưa có chính sách đầy đủ.
- Chưa thấy định danh gửi câu trả lời/giải quyết vòng để chống gửi lặp và cạnh tranh đồng thời.
- Hàng chờ và câu trả lời đang chờ có nguy cơ mất khi tiến trình vận hành khởi động lại hoặc chạy nhiều bản sao.
- Mất kết nối có thể xử thua ngay, chưa có thời gian ân hạn, xác minh quay lại và ảnh chụp trạng thái.
- Luồng trì hoãn giữa các vòng phụ thuộc vào kết nối hiện hành, dễ gây trạng thái hai bên khác nhau.
- Mã phòng và thao tác rời hàng chờ cần ràng buộc chủ sở hữu, hết hạn và chống dò/quấy rối.
- Lựa chọn câu hỏi cần chứng minh cùng độ khó, cùng thời điểm, không lộ đáp án và phù hợp vốn học.
- Hiệu ứng thú cưng/sát thương có thể lấn át đúng sai học thuật; ảnh chụp cấu hình trận chưa rõ.
- Điểm thi đấu và thưởng cần chốt đúng một lần; M06 phải là nguồn tài sản, M09 là nơi tổng hợp bảng/mùa.

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Chế độ, luật và vòng đời trận | Tạo luật xác định, có phiên bản | PvE/PvP, trạng thái, điều kiện chuyển, ảnh chụp luật | Xếp hạng tổng hợp | M11 |
| SF02 | Mở khóa và tiến trình PvE | Xây dựng hành trình thử thách học thuật | Điều kiện, thứ tự, thử lại, cooldown, chinh phục | Tính tiến độ học gốc | M01, M04 |
| SF03 | Đội hình và hiệu ứng | Đảm bảo tài sản hợp lệ, công bằng trong cả trận | Quyền sở hữu, đội hình, ảnh chụp, giới hạn hiệu ứng | Quản lý kho/tài sản | M06 |
| SF04 | Phòng mời và ghép trận | Tạo cặp đấu phù hợp, an toàn | Phòng, mã mời, hàng chờ, mở rộng tiêu chí, hủy/hết hạn | Bảng xếp hạng mùa | M01, M09 |
| SF05 | Sẵn sàng và bộ câu hỏi | Bắt đầu hai bên với cùng điều kiện | Sẵn sàng, ảnh chụp, chọn câu, độ khó, thứ tự | Sở hữu học liệu | M02, M03 |
| SF06 | Vòng đấu, trả lời và chấm điểm | Giải quyết mỗi vòng đúng một lần | Hạn giờ, câu trả lời, đúng sai nguồn, điểm, sát thương | Biên soạn đáp án | M02, M03, M06 |
| SF07 | Đồng bộ, mất kết nối và phục hồi | Giữ một trạng thái chính thức qua gián đoạn | Sự kiện trạng thái, quay lại, ân hạn, bỏ cuộc, trận treo | Kênh thông báo thiết bị | M10, M11 |
| SF08 | Kết thúc, điểm và phần thưởng | Chốt kết quả/bàn giao đúng một lần | Lý do kết thúc, điểm thi đấu, tiến trình, yêu cầu thưởng | Cấp thưởng/bảng mùa | M06, M09 |
| SF09 | Lịch sử, xem lại và tranh chấp | Tạo bằng chứng đáng tin cậy | Tóm tắt, vòng, phiên bản, quyền xem, khiếu nại/đính chính | Hỗ trợ ngoài hệ thống | M01, M11 |
| SF10 | Chống gian lận, quan sát và cân bằng | Bảo vệ công bằng và giá trị học | Tín hiệu, cảnh báo, mô phỏng, chỉ số, kiểm toán | Điều tra danh tính bên ngoài | M04, M09, M11 |

## 5. Phân tích chi tiết

### SF01 — Chế độ, luật và vòng đời trận

**Business flow:** quản trị tạo bản nháp luật cho từng chế độ; khai báo điều kiện tham gia, số vòng, thời gian, cách tính, sát thương, thắng/hòa/bỏ cuộc và hiệu lực; hệ thống mô phỏng/kiểm tra; luật được duyệt; khi tạo trận, M08 lưu ảnh chụp phiên bản; trận chỉ chuyển trạng thái theo sơ đồ hợp lệ; thay đổi chỉ áp dụng trận mới trừ dừng khẩn cấp.

**Edge case:** hai phiên bản cùng hiệu lực; đổi luật khi đang chờ; trận cũ kéo dài qua hiệu lực mới; trạng thái chuyển lùi; kết thúc từ hai nguyên nhân; tắt khẩn cấp; cấu hình khiến trận không thể kết thúc.

**DoD:** từng chế độ có mục đích, luật, trạng thái và phiên bản; mọi chuyển trạng thái có tác nhân/điều kiện/thời điểm; trận đang chạy giữ ảnh chụp; kết thúc chỉ có một lý do chính thức.

### SF02 — Mở khóa và tiến trình PvE

**Business flow:** M08 nhận chỉ số đủ tin cậy từ M01/M04; đánh giá điều kiện và thứ tự phòng; mở khóa một lần; người học xem yêu cầu/đội đối thủ/phần thưởng; bắt đầu nếu không trong thời gian chờ; sau trận cập nhật số lần, điểm tốt nhất và trạng thái chinh phục; phát tín hiệu cho phòng tiếp theo.

**Edge case:** tiến độ giảm sau mở; phòng trước bị đính chính; mở nhiều phòng cùng lúc; điều kiện đổi; người dùng đã đủ trước khi phòng được thêm; thua/mất kết nối; bắt đầu đồng thời hai trận; cooldown qua múi giờ; phòng ngừng dùng.

**DoD:** điều kiện có nguồn, phiên bản và bằng chứng; mở khóa/chinh phục không lặp; mỗi người có giới hạn trận hoạt động; thời gian chờ dùng mốc tuyệt đối; lịch sử không bị xóa khi phòng đổi/ngừng.

### SF03 — Đội hình và hiệu ứng

**Business flow:** người học chọn đội hình; M08 hỏi M06 về quyền sở hữu/trạng thái và hiệu ứng hợp lệ; kiểm tra số lượng, trùng, cấp/độ hiếm và luật chế độ; khi hai bên sẵn sàng, lưu ảnh chụp đội hình/hiệu ứng; trong trận chỉ dùng ảnh chụp; thay đổi tài sản bên ngoài không sửa trận.

**Edge case:** thú không sở hữu; cùng thú nhiều vị trí; thú bị ngừng dùng; hai thiết bị đổi đội; tiến hóa sau khi tạo phòng; hiệu ứng vượt trần/cộng dồn; đội hình đối thủ bị lộ sớm; tài sản bị thu hồi giữa trận.

**DoD:** mọi vị trí có nguồn sở hữu và phiên bản; quy tắc chế độ/cân bằng được kiểm tra; ảnh chụp bất biến; hiệu ứng không đổi đúng/sai và có trần; lỗi xác minh không khóa tài sản vô hạn.

### SF04 — Phòng mời và ghép trận

**Business flow:** người chơi tạo phòng hoặc vào hàng chờ với đội hình hợp lệ; phòng có mã khó đoán, chủ, chế độ và hạn; người khác tham gia sau kiểm tra; hàng chờ tìm đối thủ theo điểm/trình độ/thời gian; tiêu chí mở rộng có kiểm soát; khi ghép thành công hai yêu cầu được loại khỏi chờ và tạo một trận; rời/hết hạn được xử lý an toàn.

**Edge case:** tự ghép chính mình; cùng người vào nhiều hàng; rời bằng mã không thuộc sở hữu; hai người cùng nhập mã; dò mã; phòng công khai lộ hồ sơ; đối thủ bị chặn; hàng chờ mất khi khởi động lại; ghép thành công nhưng thông báo mất; điểm đổi trong lúc chờ.

**DoD:** mỗi người tối đa một yêu cầu chờ hợp lệ theo chính sách; mã có hạn/chống dò; rời chỉ tác động yêu cầu của chính mình; một cặp tạo một trận; thời gian chờ và độ lệch được đo; phục hồi không ghép trùng.

### SF05 — Sẵn sàng và bộ câu hỏi

**Business flow:** người chơi vào đúng trận và xác nhận sẵn sàng; M08 xác thực danh tính/đội hình; khi đủ bên, khóa luật/đội; yêu cầu M02/M03 cung cấp tập câu hợp lệ; kiểm tra số lượng, độ khó, phạm vi và đáp án; tạo thứ tự/ảnh chụp; gửi cùng nội dung và mốc bắt đầu chính thức; trận chuyển sang diễn ra một lần.

**Edge case:** xác nhận lặp; đổi kết nối; một bên không sẵn sàng; câu thiếu đáp án; từ ngoài vốn học; hai bên khác trình độ; nội dung bị ngừng dùng; bộ câu trùng; thông điệp bắt đầu chỉ đến một bên; người xem đoán trước câu.

**DoD:** chỉ người tham gia được sẵn sàng; hai bên nhận cùng ảnh chụp/mốc thời gian PvP; độ khó có chính sách; đáp án không lộ trước vòng; bắt đầu lại trả cùng trạng thái, không tạo bộ câu mới.

### SF06 — Vòng đấu, trả lời và chấm điểm

**Business flow:** M08 mở vòng với thời hạn chính thức; nhận tối đa một câu trả lời cuối hợp lệ mỗi bên; kiểm tra người, trận, vòng, câu và định danh; xác định thời gian theo mốc tin cậy; hết hạn tạo kết quả thiếu câu trả lời; M03/M02 xác nhận đúng sai; áp công thức điểm/sát thương từ ảnh chụp; chốt vòng một lần; phát kết quả và vòng kế.

**Edge case:** gửi lặp/đổi đáp án; câu của vòng khác; thời gian âm/quá nhanh; hai câu đến đồng thời; một bên không trả lời; cả hai đúng cùng thời gian; mất kết nối khi gửi; chấm khác nhau do ký tự/locale; sát thương hạ nhiều thú; vòng đã giải quyết được gửi lại.

**DoD:** mỗi người/vòng có một kết quả hợp lệ theo chính sách; thời hạn phía vận hành là nguồn thật; cùng đầu vào cho cùng điểm/sát thương; giải quyết lặp không đổi trạng thái; mọi điểm truy vết đến đáp án/luật.

### SF07 — Đồng bộ, mất kết nối và phục hồi

**Business flow:** M08 lưu trạng thái chính thức trước khi phát tín hiệu; mỗi thay đổi có thứ tự/phiên bản; người chơi xác nhận hoặc yêu cầu ảnh chụp mới; khi mất kết nối, trận chuyển gián đoạn và bắt đầu ân hạn; người dùng xác thực quay lại nhận trạng thái mới nhất; hết ân hạn áp luật bỏ cuộc; tác vụ giám sát xử lý phòng/trận treo.

**Edge case:** thông điệp trùng/sai thứ tự; cả hai mất kết nối; đổi thiết bị/kết nối; tiến trình vận hành khởi động lại; nhiều bản sao xử lý cùng trận; quay lại đúng lúc hết hạn; phát kết quả thất bại; một bên cố tình ngắt mạng; PvE bị gián đoạn lâu.

**DoD:** trạng thái bền vững là nguồn thật, không phụ thuộc kết nối/bộ nhớ tạm; quay lại không mất lượt hợp lệ; ân hạn và bỏ cuộc xác định; sự kiện gửi lại an toàn; trận treo được phát hiện và kết thúc/phục hồi theo chính sách.

### SF08 — Kết thúc, điểm và phần thưởng

**Business flow:** khi đạt điều kiện thắng/hết vòng/bỏ cuộc/hủy, M08 tạo quyết định kết thúc duy nhất; chốt tỷ số, thắng/thua/hòa và bằng chứng; cập nhật tiến trình PvE; với PvP tính một biến động điểm gốc theo phiên bản; gửi biến động cho M09; tạo quyền nhận thưởng gửi M06; phát tín hiệu cho M07/M10; đối soát mọi bàn giao.

**Edge case:** cả hai kết thúc đồng thời; hòa; bỏ cuộc trước khi bắt đầu; trận vô hiệu; tính điểm thành công một bên; phản hồi M06/M09 mất; kết quả bị đính chính; người dùng khóa; thưởng nhiều loại; lặp yêu cầu kết thúc.

**DoD:** một trận có một kết quả chính thức và một bộ bàn giao định danh ổn định; xử lý lại không đổi điểm/cấp thêm; M06 sở hữu tài sản; M09 sở hữu tổng hợp mùa; lỗi bàn giao không làm mất kết quả.

### SF09 — Lịch sử, xem lại và tranh chấp

**Business flow:** sau khi chốt, M08 lưu tóm tắt, người tham gia, luật/đội/bộ câu, từng vòng, mốc thời gian, lý do kết thúc, điểm và trạng thái thưởng; người tham gia xem lịch sử theo quyền; quản trị xem bằng chứng mở rộng; người dùng gửi tranh chấp trong thời hạn; M11 điều tra và ra quyết định đính chính phiên bản hóa.

**Edge case:** câu hỏi/đáp án nhạy cảm bị tái sử dụng; đối thủ ẩn hồ sơ; trận đang đối soát; dữ liệu vòng thiếu; xóa tài khoản; tranh chấp lặp/quá hạn; thông tin chống gian lận bị lộ; đính chính sau thưởng/mùa công bố.

**DoD:** lịch sử giải thích được kết quả nhưng giới hạn lộ học liệu/dữ liệu cá nhân; người ngoài không xem; tranh chấp có trạng thái/thời hạn/bằng chứng/quyết định; đính chính không ghi đè bản cũ.

### SF10 — Chống gian lận, quan sát và cân bằng

**Business flow:** M08 thu tín hiệu thời gian, lặp đáp án, độ chính xác bất thường, mất kết nối, thông đồng, chênh lệch ghép và hiệu ứng; áp biện pháp phòng ngừa/giới hạn; gắn cờ trận; M11 xem bằng chứng và quyết định; theo dõi tỷ lệ hoàn thành, công bằng, chất lượng câu, lợi thế thiết bị/đội hình và tác động học; thay đổi luật được mô phỏng/thử nghiệm.

**Edge case:** người giỏi trả lời rất nhanh; độ trễ vùng địa lý; đối thủ quen nhau; câu lỗi tạo tín hiệu giả; nhiều tài khoản; quản trị xung đột lợi ích; hiệu ứng mới phá cân bằng; quy mô mẫu nhỏ; gian lận sau trao thưởng.

**DoD:** không tự kết luận chỉ từ một tín hiệu; bằng chứng có thời hạn và quyền xem; cảnh báo trận treo/độ lệch/điểm/thưởng có ngưỡng; chỉ số gồm chất lượng học và công bằng; có quy trình tắt/quay lại luật lỗi.

## 6. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Xây dựng luật và tiến trình PvE ổn định | SF01, SF02 | M08-T001–M08-T010 |
| Xác minh đội hình, phòng và ghép trận | SF03, SF04 | M08-T011–M08-T020 |
| Bắt đầu, hỏi đáp và giải quyết vòng công bằng | SF05, SF06 | M08-T021–M08-T030 |
| Phục hồi, kết thúc và bàn giao đúng một lần | SF07, SF08 | M08-T031–M08-T039 |
| Lịch sử, tranh chấp, gian lận và cân bằng | SF09, SF10 | M08-T040–M08-T046 |
| Nghiệm thu và bàn giao | SF01–SF10 | M08-T047–M08-T048 |

## 7. Thứ tự thực hiện đề xuất

1. Chốt chế độ, vòng đời, luật, ảnh chụp và tiến trình PvE.
2. Chốt đội hình/hiệu ứng với M06 và học liệu/câu hỏi với M02/M03.
3. Chốt phòng, hàng chờ, ghép cặp, sẵn sàng và giải quyết vòng.
4. Chốt đồng bộ, quay lại, kết thúc, điểm M09 và thưởng M06.
5. Chốt lịch sử, tranh chấp, chống gian lận, quan sát và nghiệm thu xuyên chức năng.

## 8. Cơ sở quyết định đã chốt

Tất cả 26 quyết định M08 đã được chốt. PvE và PvP là hai hệ thống nghiệp vụ độc lập. PvE có 10 câu và lộ trình tuyến tính; PvP có 7 vòng, cho hòa, chỉ nhận một đáp án cuối mỗi vòng và dùng thời gian phía vận hành có bù độ trễ giới hạn.

PvP bắt buộc ba thú, dùng bộ hiệu ứng riêng không can thiệp nội dung, đáp án hoặc thời gian. Ghép cặp kết hợp điểm thi đấu và năng lực từ vựng, mở rộng mỗi 15 giây trong trần; cùng đối thủ chỉ có hai trận tính điểm trong 24 giờ. Hai bên nhận cùng câu, thứ tự, thời hạn và tiêu chí chấm; chưa dùng phát âm trong PvP.

PvP cho 30 giây kết nối lại nhưng đồng hồ vòng vẫn chạy; cả hai không trở lại thì trận vô hiệu. PvE cho khôi phục trong 5 phút, tổng tạm dừng tối đa 10 phút. M08 sở hữu kết quả trận/điểm hiện tại, M09 sở hữu mùa và tổng hợp.

## 9. Rủi ro còn hiệu lực

- Tin thời gian phía người chơi hoặc nhận câu trả lời nhiều lần làm sai điểm và kết quả.
- Trạng thái chờ chỉ tồn tại tạm thời khiến trận mất khi gián đoạn vận hành.
- Mất kết nối bị xử thua ngay gây tranh chấp, nhưng ân hạn quá dài lại bị lợi dụng.
- Bộ câu khác độ khó hoặc ngoài vốn học làm PvP không còn công bằng học thuật.
- Hiệu ứng thú cưng lấn át đúng sai khiến lợi thế sưu tập quyết định trận.
- Kết thúc/điểm/thưởng không có định danh ổn định gây nhân đôi hoặc lệch M08–M09–M06.

## 10. Điều kiện sẵn sàng triển khai

- M01 chốt trạng thái tài khoản, hồ sơ hiển thị và quyền riêng tư đối thủ.
- M02/M03 chốt hợp đồng bộ câu, đáp án, độ khó và chấm đúng sai.
- M06 chốt ảnh chụp đội hình/hiệu ứng và yêu cầu thưởng đúng một lần.
- M09 chốt điểm thi đấu gốc, mùa và bàn giao biến động; M10 chốt tín hiệu trận.
- M11 chốt quyền cấu hình, kiểm toán, cảnh báo và quy trình tranh chấp.

Không còn quyết định mở trong M08; `QUYET-DINH-MO.md` là nguồn chi tiết.
