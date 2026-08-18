# Phân tích chuyên sâu M10 — Thông báo và duy trì tương tác

## 1. Mục tiêu và phạm vi

M10 chuyển các tín hiệu nghiệp vụ đã được module nguồn xác nhận thành thông tin đúng người, đúng lúc và đúng kênh. Mục tiêu là hỗ trợ người học quay lại hoạt động có giá trị mà vẫn tôn trọng sự đồng ý, riêng tư, giờ nghỉ và giới hạn chú ý của họ.

### Trong phạm vi

- Phân loại thông báo, mức ưu tiên, nguồn sự kiện và vòng đời thông báo.
- Lựa chọn nhận tin theo loại/kênh, trạng thái đồng ý, múi giờ và giờ yên lặng.
- Mẫu nội dung, bản địa hóa, nội dung màn hình khóa và liên kết hành động.
- Hộp thư trong hệ thống, trạng thái chưa đọc/đã đọc/đã ẩn và lịch sử.
- Điều phối kênh trong hệ thống, thời gian thực, đẩy và thư điện tử.
- Lập lịch nhắc học, chống gửi lặp, giới hạn tần suất và gom thông báo.
- Quản lý điểm nhận trên thiết bị/địa chỉ ở mức cần cho việc gửi.
- Gửi diện rộng có phân khúc, xem trước, phê duyệt, lên lịch và hủy.
- Theo dõi gửi, lỗi, tương tác, quay lại học, kiểm toán và lưu giữ.

### Ngoài phạm vi

- Xác định nội dung nào đến hạn ôn, nhiệm vụ nào hoàn thành hoặc ai thắng trận.
- Sở hữu email, hồ sơ, múi giờ hay danh tính thiết bị ngoài thông tin đồng bộ cần cho việc gửi.
- Tự thay đổi tiến độ học, nhiệm vụ, thứ hạng hoặc tài sản.
- Bảo đảm nhà cung cấp bên ngoài thực sự hiển thị thông báo trên thiết bị.

## 2. Từ điển trạng thái nghiệp vụ đề xuất

| Nhóm trạng thái | Trạng thái | Ý nghĩa |
|---|---|---|
| Thông báo | Đã tiếp nhận | Tín hiệu nguồn hợp lệ đã được ghi nhận đúng một lần |
| Thông báo | Đã chặn | Không tạo/gửi vì lựa chọn nhận tin, tần suất, hết hạn hoặc chính sách |
| Thông báo | Đã lên lịch | Có thời điểm gửi phù hợp múi giờ/giờ yên lặng |
| Thông báo | Đang điều phối | Đã chọn kênh và đang tạo các lần gửi |
| Thông báo | Hoàn tất | Các kênh bắt buộc đã có kết quả cuối hoặc không cần gửi |
| Lần gửi | Chờ gửi | Đủ điều kiện và chưa giao cho kênh |
| Lần gửi | Đã giao nhà cung cấp | Kênh tiếp nhận nhưng chưa đồng nghĩa người dùng đã thấy |
| Lần gửi | Gửi thất bại tạm thời | Có thể thử lại theo chính sách |
| Lần gửi | Gửi thất bại cuối | Không thử lại; có lý do như điểm nhận hết hiệu lực |
| Tương tác | Chưa đọc/Đã đọc/Đã mở hành động/Đã ẩn | Trạng thái trải nghiệm của người dùng, tách biệt kết quả gửi |

## 3. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Có hộp thông báo cá nhân với tiêu đề, nội dung, loại, liên kết hành động, trạng thái đọc và thời điểm tạo.
- Người học có thể xem, đánh dấu một/tất cả đã đọc và xóa thông báo.
- Thông báo mới được lưu rồi truyền theo thời gian thực; có gửi đẩy nếu người dùng có điểm nhận.
- Có các loại ôn tập, phần thưởng, sự kiện, cảnh báo chuỗi học, thành tựu và hệ thống.
- Có tác vụ nhắc ôn theo giờ học ưu tiên và chống lặp sơ bộ trong một giờ.
- Có email gọi lại người dùng không hoạt động theo khoảng cách ngày; có gửi diện rộng tới danh sách hoặc toàn bộ người dùng và ghi hoạt động quản trị.

### Khoảng trống và rủi ro

- Chưa có định danh sự kiện nguồn nên gửi lại có thể tạo nhiều bản ghi và nhiều lần đẩy.
- Chưa có lựa chọn nhận tin theo loại/kênh, bằng chứng đồng ý, giờ yên lặng hoặc ngoại lệ khẩn cấp.
- Giờ học ưu tiên được so theo giờ chung, chưa thể hiện chuyển đổi múi giờ và giờ mùa hè.
- Trạng thái đọc một thông báo cần kiểm tra chủ sở hữu; xóa cứng có thể mất bằng chứng kiểm toán.
- Chưa có trạng thái gửi độc lập từng kênh, mã phản hồi, số lần thử, hết hạn, đối soát hoặc phục hồi sau lỗi.
- Lưu thông báo và gửi kênh được thực hiện liền nhau; lỗi kênh có thể tạo kết quả một phần khó giải thích.
- Một điểm nhận đẩy cho mỗi người không đủ cho nhiều thiết bị; vòng đời đăng ký/thu hồi/hết hiệu lực chưa rõ.
- Chống lặp nhắc ôn dựa trên loại, trạng thái chưa đọc và cửa sổ ngắn; đọc sớm có thể cho phép nhắc lại cùng tín hiệu.
- Mẫu nội dung nằm rải rác, thiếu phiên bản, bản địa hóa, kiểm tra liên kết và chính sách màn hình khóa.
- Gửi diện rộng xử lý trực tiếp từng người, chưa có xem trước đối tượng, phê duyệt, lịch gửi, hủy, tiến độ hay phục hồi từng phần.
- “Đã gửi” chưa được tách khỏi “đã nhận/đã đọc/đã quay lại học”, dễ đánh giá sai hiệu quả.

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Phân loại và tiếp nhận tín hiệu | Chuẩn hóa ý nghĩa, ưu tiên và chống lặp | Danh mục, hợp đồng nguồn, định danh, hiệu lực | Xác định sự kiện gốc | M04, M07, M08, M09 |
| SF02 | Đồng ý và lựa chọn nhận tin | Tôn trọng quyền lựa chọn từng người | Loại, kênh, opt-in/out, bằng chứng, ngoại lệ | Sở hữu hồ sơ gốc | M01, M11 |
| SF03 | Mẫu nội dung và riêng tư | Tạo nội dung nhất quán, an toàn | Mẫu, phiên bản, ngôn ngữ, biến, liên kết, màn hình khóa | Sáng tạo chiến dịch ngoài quy trình | M01, M11 |
| SF04 | Hộp thư trong hệ thống | Cung cấp lịch sử thông báo đúng quyền | Danh sách, chưa đọc, đọc, mở, ẩn, lưu giữ | Hộp thư bên ngoài | M01 |
| SF05 | Điều phối kênh và chính sách chú ý | Chọn đúng kênh/thời điểm, không spam | Ưu tiên, chống lặp, tần suất, gom, hết hạn, fallback | Gửi vật lý của nhà cung cấp | M01, M11 |
| SF06 | Lập lịch và nhắc học | Nhắc hoạt động có giá trị theo thời điểm cá nhân | Múi giờ, giờ yên lặng, lịch học, đến hạn, không hoạt động | Tính lịch ôn gốc | M01, M04 |
| SF07 | Giao nhận và phục hồi kênh | Theo dõi từng lần gửi và xử lý lỗi an toàn | Điểm nhận, kết quả kênh, thử lại, hủy đăng ký, đối soát | Hạ tầng nhà cung cấp | M01, M11 |
| SF08 | Thông báo diện rộng | Ngăn gửi nhầm và vận hành chiến dịch an toàn | Phân khúc, xem trước, duyệt, lịch, hủy, tiến độ | Quản lý chiến dịch marketing đầy đủ | M01, M11 |
| SF09 | Đo tương tác và quay lại học | Đo tác động thực thay vì chỉ số gửi | Mở, hành động, phiên quay lại, attribution, thử nghiệm | Tính chất lượng học gốc | M03, M04, M11 |
| SF10 | Vận hành, kiểm toán và lưu giữ | Bảo đảm khả năng quan sát và tuân thủ | Cảnh báo, nhật ký, quyền, lưu giữ, xóa/ẩn danh | Chính sách pháp lý cuối cùng | M01, M11 |

## 5. Phân tích chi tiết

### SF01 — Phân loại và tiếp nhận tín hiệu

**Business flow:** module nguồn gửi tín hiệu đã xác nhận gồm người nhận, loại, thời điểm, định danh, mức khẩn, thời hạn và dữ liệu tối thiểu; M10 xác thực nguồn/phiên bản; ghi nhận đúng một lần; ánh xạ sang loại thông báo/chính sách; tín hiệu trùng trả cùng kết quả; tín hiệu không còn hiệu lực bị chặn có lý do.

**Edge case:** gửi lặp/sai thứ tự; thiếu người dùng; nguồn rút lại; tín hiệu đến muộn; một sự kiện có nhiều người nhận; người dùng bị khóa/xóa; loại chưa biết; cùng sự kiện thay đổi nội dung; tín hiệu khẩn giả.

**DoD:** mỗi loại có chủ nguồn, mục đích, ưu tiên, thời hạn và dữ liệu; định danh chống lặp xuyên lần thử; lỗi có lý do; có chính sách đính chính/hủy; truy vết từ sự kiện đến thông báo/lần gửi.

### SF02 — Đồng ý và lựa chọn nhận tin

**Business flow:** người dùng xem lựa chọn theo loại và kênh; hệ thống hiển thị mặc định/minh bạch; thay đổi được xác nhận và ghi thời điểm/nguồn/phiên bản; trước mỗi lần gửi M10 dùng lựa chọn hiệu lực mới nhất; opt-out có hiệu lực trong thời hạn cam kết; loại bắt buộc chỉ dùng cho thông tin vận hành cần thiết.

**Edge case:** người dùng mới; chưa có lựa chọn; rút đồng ý khi thông báo đã xếp hàng; nhiều thiết bị; tài khoản trẻ vị thành niên nếu áp dụng; loại khẩn cấp; kênh email xác minh chưa hoàn tất; thay đổi đồng thời; nhập lại tài khoản.

**DoD:** ma trận loại–kênh–mặc định được duyệt; opt-out chặn các lần gửi chưa giao; bằng chứng đồng ý truy vết được; không gộp thông báo bắt buộc với tương tác/marketing; trải nghiệm cho phép thay đổi dễ dàng.

### SF03 — Mẫu nội dung và riêng tư

**Business flow:** quản trị tạo bản nháp mẫu theo loại/kênh/ngôn ngữ; khai báo biến và mức nhạy cảm; kiểm tra độ dài, biến, liên kết và nội dung khóa màn hình; xem trước với dữ liệu mẫu; duyệt/phiên bản hóa; khi điều phối, M10 dựng nội dung từ ảnh chụp mẫu và dữ liệu nguồn đã cho phép.

**Edge case:** thiếu bản dịch/biến; ký tự đặc biệt; liên kết không hợp lệ cho kênh; tên người dùng chứa nội dung xấu; thông tin thắng/thua/phần thưởng lộ trên khóa; mẫu đổi sau lên lịch; hành động đích không còn; nội dung quá dài.

**DoD:** mỗi mẫu có phiên bản, ngôn ngữ, biến được phép, fallback, mức nhạy cảm và liên kết hợp lệ; không đưa dữ liệu nhạy cảm vào bề mặt khóa mặc định; nội dung đã lên lịch không đổi ngầm.

### SF04 — Hộp thư trong hệ thống

**Business flow:** khi chính sách cho phép, M10 tạo một bản thông báo hộp thư; người dùng xem danh sách phân trang và số chưa đọc; mở/đọc/đọc tất cả/ẩn theo chính chủ; thao tác lặp an toàn; liên kết hành động được kiểm tra; dữ liệu hết hạn được lưu giữ hoặc loại bỏ theo chính sách mà không mất nhật ký cần thiết.

**Edge case:** đánh dấu thông báo của người khác; nhiều thiết bị đọc đồng thời; bản đã ẩn; đọc tất cả khi có bản mới; liên kết hết hạn; thông báo bị rút; tài khoản xóa; hàng nghìn bản; cùng thông báo xuất hiện trùng.

**DoD:** mọi thao tác ràng buộc người sở hữu; có thời điểm đọc/mở/ẩn; phân trang ổn định; số chưa đọc nhất quán; xóa trải nghiệm không đồng nghĩa xóa bằng chứng; dữ liệu hết hạn có trạng thái rõ.

### SF05 — Điều phối kênh và chính sách chú ý

**Business flow:** M10 lấy loại, ưu tiên, lựa chọn, trạng thái hoạt động và thiết bị; kiểm tra chống lặp, tần suất, giờ yên lặng và thời hạn; chọn kênh theo chính sách; gom tín hiệu có thể gộp; tạo lần gửi riêng từng kênh; nếu kênh chính không khả dụng thì dùng fallback đã được đồng ý; chốt trạng thái điều phối.

**Edge case:** nhiều tín hiệu cùng lúc; ưu tiên cao vượt giới hạn; thông báo đã đọc trong hộp thư trước khi đẩy; người dùng đang trực tuyến; không có kênh hợp lệ; fallback gây gửi hai lần; hết hạn khi đang chờ; sự kiện đính chính sau điều phối.

**DoD:** một tín hiệu không tạo gửi trùng ngoài chính sách; giới hạn theo loại/kênh/toàn cục; ưu tiên và fallback rõ; gom không làm mất ý nghĩa/thời hạn; quyết định gửi/chặn giải thích được.

### SF06 — Lập lịch và nhắc học

**Business flow:** M10 nhận lịch ôn/đến hạn từ M04, nhiệm vụ từ M07 và trạng thái hoạt động từ M01; tính thời điểm phù hợp theo múi giờ, giờ học và giờ yên lặng; gom số lượng hiện hành gần lúc gửi; kiểm tra người dùng đã hoàn thành/đã hoạt động; gửi hoặc bỏ; email gọi lại chỉ kích hoạt theo ngưỡng không hoạt động và giới hạn riêng.

**Edge case:** đổi múi giờ; giờ mùa hè; thiết bị sai giờ; từ đã ôn trước lúc gửi; số lượng thay đổi; nhiệm vụ sắp hết hạn trong giờ yên lặng; người dùng đang học; không có giờ ưu tiên; chạy lịch lặp/khởi động lại; không hoạt động do tài khoản khóa.

**DoD:** thời điểm lưu bằng mốc tuyệt đối và hiển thị theo người dùng; chạy lại không tạo nhắc trùng; tái kiểm tra tính còn liên quan trước gửi; giờ yên lặng được tôn trọng; nhắc học có trần và mục tiêu hành động rõ.

### SF07 — Giao nhận và phục hồi kênh

**Business flow:** mỗi kênh nhận một lần gửi có định danh, nội dung và hạn; M10 giao tới kênh; ghi kết quả tiếp nhận; phản hồi tạm thời được thử lại với cùng định danh theo lịch; lỗi cuối làm vô hiệu điểm nhận nếu phù hợp; nhiều thiết bị được xử lý độc lập; đối soát trạng thái và cảnh báo sai lệch.

**Edge case:** nhà cung cấp chậm/mất phản hồi; đã nhận nhưng M10 nghĩ lỗi; điểm nhận cũ thuộc người khác; nhiều thiết bị; email trả lại; liên kết không hợp lệ; giới hạn nhà cung cấp; thử lại sau hết hạn; một kênh thành công kênh khác lỗi.

**DoD:** trạng thái/số lần thử/mã lý do theo từng kênh; lỗi tạm và cuối được phân biệt; gửi lại không tạo bản logic mới; điểm nhận hết hiệu lực bị ngừng; lỗi một kênh không làm mất hộp thư/kết quả kênh khác.

### SF08 — Thông báo diện rộng

**Business flow:** quản trị chọn mục tiêu, phân khúc, mẫu, kênh và lịch; hệ thống ước lượng đối tượng/chi phí và loại theo lựa chọn nhận tin; gửi bản thử; người có quyền khác duyệt nếu cần; chiến dịch được lên lịch; trước phát hành tái tính đối tượng theo chính sách; xử lý theo lô có tiến độ; có thể tạm dừng/hủy phần chưa gửi; xuất báo cáo.

**Edge case:** chọn nhầm tất cả; danh sách chứa người không tồn tại; tự duyệt; mẫu/đối tượng đổi sau duyệt; hủy khi một phần đã gửi; nhà cung cấp lỗi giữa lô; gửi hai lần do chạy lại; nội dung khẩn cấp; múi giờ nhiều vùng; chi phí vượt ngưỡng.

**DoD:** có xem trước số lượng/mẫu/kênh/chi phí; quyền và phê duyệt theo mức ảnh hưởng; mỗi người tối đa một bản logic của chiến dịch; tiến độ/chặn/lỗi truy vết được; hủy không ảnh hưởng phần đã gửi.

### SF09 — Đo tương tác và quay lại học

**Business flow:** M10 ghi giao nhà cung cấp, đọc, mở hành động và phiên học tiếp theo với định danh phù hợp; áp cửa sổ ghi nhận; so sánh nhóm nhận/không nhận hoặc biến thể; tổng hợp hiệu quả theo loại/kênh/giờ/phân khúc; theo dõi opt-out, bỏ ứng dụng và chất lượng học; chỉ tối ưu khi lợi ích vượt tác động tiêu cực.

**Edge case:** mở trên thiết bị khác; nhiều thông báo cùng dẫn tới một phiên; trình chặn theo dõi; mở không đăng nhập; người dùng tự quay lại; thông báo giao nhưng không hiển thị; mẫu nhỏ; chiến dịch chọn nhóm thiên lệch; tương tác giả.

**DoD:** phân biệt tạo/gửi/giao/đọc/mở/quay lại/học; cửa sổ và quy tắc attribution rõ; không tuyên bố quan hệ nhân quả từ tương quan; có chỉ số opt-out/mệt mỏi/chất lượng; dữ liệu đo tuân thủ lựa chọn riêng tư.

### SF10 — Vận hành, kiểm toán và lưu giữ

**Business flow:** M11 theo dõi hàng chờ, độ trễ, lỗi kênh, tỷ lệ chặn, chi phí và bất thường; cảnh báo có chủ; quản trị tra từ tín hiệu đến quyết định/lần gửi; thao tác nhạy cảm có kiểm toán; chính sách lưu giữ xóa nội dung/điểm nhận nhưng giữ số liệu tổng hợp cần thiết; yêu cầu xóa tài khoản được lan truyền.

**Edge case:** tồn đọng lớn; một mẫu gây lỗi hàng loạt; dữ liệu nhà cung cấp thiếu; nhật ký chứa nội dung cá nhân; xóa khi đang gửi; đổi thời hạn lưu giữ; quản trị truy cập trái phép; gửi khẩn cấp khi kênh lỗi; chi phí tăng đột biến.

**DoD:** có chỉ số/ngưỡng/chủ xử lý; truy vết không ghi bí mật/PII thừa; quyền xem theo vai trò; lưu giữ theo loại dữ liệu; xóa/ẩn danh không làm hỏng báo cáo tổng hợp; có quy trình dừng khẩn cấp.

## 6. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Tiếp nhận tín hiệu và tôn trọng đồng ý | SF01, SF02 | M10-T001–M10-T010 |
| Nội dung an toàn và hộp thư đúng quyền | SF03, SF04 | M10-T011–M10-T019 |
| Điều phối, lịch nhắc và giới hạn chú ý | SF05, SF06 | M10-T020–M10-T029 |
| Giao nhận đa kênh và gửi diện rộng | SF07, SF08 | M10-T030–M10-T039 |
| Đo tác động và vận hành | SF09, SF10 | M10-T040–M10-T046 |
| Nghiệm thu và bàn giao | SF01–SF10 | M10-T047–M10-T048 |

## 7. Thứ tự thực hiện đề xuất

1. Chốt phân loại, hợp đồng tín hiệu, chống lặp và lựa chọn nhận tin.
2. Chốt mẫu nội dung, riêng tư, liên kết và hộp thư đúng quyền.
3. Chốt điều phối kênh, giới hạn tần suất, múi giờ và lịch nhắc.
4. Chốt trạng thái gửi, nhiều thiết bị, thử lại và gửi diện rộng có phê duyệt.
5. Chốt attribution, chỉ số mệt mỏi, quan sát, lưu giữ và nghiệm thu xuyên module.

## 8. Cơ sở quyết định đã chốt

Tất cả 26 quyết định M10 đã được chốt. Các nguyên tắc đã chốt gồm: thông báo chia thành năm nhóm bảo mật/vận hành bắt buộc, học tập, phần thưởng/thành tích, xã hội/thi đấu và quảng bá; mỗi thông báo chỉ có một nhóm chính. Chỉ bảo mật tài khoản và vận hành bắt buộc không thể tắt. Màn hình khóa chỉ hiển thị tiêu đề chung và hành động phù hợp; dữ liệu học tập, thi đấu, phần thưởng, nhóm và bảo mật nhạy cảm được che mặc định. Khi thiếu bản dịch, chỉ nội dung không nhạy cảm được dùng ngôn ngữ mặc định đã duyệt; nội dung có nguy cơ hiểu sai phải bị chặn. Liên kết hành động chỉ dẫn tới màn hình nội bộ hoặc miền web được phê duyệt, có kiểm tra quyền và điểm đến thay thế an toàn. Xóa thông báo làm ẩn ngay khỏi hộp thư; chỉ bằng chứng tối thiểu không nhạy cảm được giữ có thời hạn rồi xóa hoặc ẩn danh hóa. Khi ứng dụng đang hoạt động, thông báo thường chỉ dùng hộp thư/kênh trong ứng dụng; đẩy chỉ giữ cho bảo mật hoặc vận hành khẩn cấp đã duyệt. Nhắc ôn chỉ gửi khi có nội dung đến hạn, gần giờ học ưu tiên và tối đa một lần/ngày. Email gọi lại bắt đầu sau 7 ngày không hoạt động học hợp lệ, tối đa một email mỗi 7 ngày và dừng khi quay lại hoặc từ chối nhận. Điểm nhận lỗi vĩnh viễn bị vô hiệu ngay; lỗi tạm thử tối đa ba lần rồi tạm ngưng 24 giờ mà không ảnh hưởng kênh khác. Thử gửi lại tối đa ba lần với khoảng chờ tăng dần, không quá 24 giờ và dừng theo hết hạn, opt-out hoặc điểm nhận vô hiệu. Chiến dịch lưu bản xem trước khi phê duyệt nhưng tính lại điều kiện và lựa chọn nhận ngay trước gửi, đồng thời ghi chênh lệch. Chiến dịch đang gửi chỉ dừng được phần chưa giao; phần đã gửi phải sửa bằng thông báo đính chính liên kết với chiến dịch gốc. Cửa sổ ghi nhận là 24 giờ cho nhắc học/nhiệm vụ/thi đấu và 7 ngày cho email gọi lại, dùng báo cáo nhiều điểm chạm. Chỉ số tối ưu kết hợp chất lượng quay lại học, duy trì, hoàn thành và tín hiệu tiêu cực; không tối ưu riêng tỷ lệ mở.

Thông báo tùy chọn được bật mặc định theo lựa chọn sản phẩm, nhưng người dùng có thể tắt ngay theo cả nhóm nội dung và kênh; cấu hình thị trường phải ghi đè khi pháp luật hoặc nhóm tuổi yêu cầu đồng ý chủ động. Giờ yên lặng mặc định 22:00–07:00 theo múi giờ hồ sơ. Mỗi sự kiện nguồn phải chống tạo/gửi lặp theo từng kênh.

## 9. Rủi ro còn hiệu lực

- Không có định danh nguồn khiến một sự kiện tạo nhiều thông báo trên nhiều kênh.
- Không tôn trọng opt-out/giờ yên lặng làm mất niềm tin và tăng rời bỏ.
- Lỗi quyền sở hữu thao tác hộp thư làm lộ hoặc thay đổi thông báo của người khác.
- Trạng thái gửi không tách theo kênh khiến thử lại tạo trùng hoặc báo thành công sai.
- Gửi diện rộng không xem trước/phê duyệt có thể gửi sai toàn bộ người dùng.
- Tối ưu tỷ lệ mở thay vì quay lại học chất lượng tạo nội dung gây áp lực, giật gân hoặc spam.

## 10. Điều kiện sẵn sàng triển khai

- M01 chốt múi giờ, ngôn ngữ, lựa chọn nhận tin, email và vòng đời điểm nhận thiết bị.
- M04/M07/M08/M09 chốt danh mục tín hiệu, định danh, thời hạn và dữ liệu tối thiểu.
- M11 chốt vai trò, phê duyệt gửi diện rộng, lưu giữ, chỉ số và cảnh báo.
- Nhà cung cấp từng kênh có bảng kết quả/lỗi và chính sách thử lại được tài liệu hóa.
- Tất cả 26 quyết định đã chốt được truy vết; backlog và bộ nghiệm thu xuyên kênh được duyệt.

Không còn quyết định mở trong M10; `QUYET-DINH-MO.md` là nguồn chi tiết.
