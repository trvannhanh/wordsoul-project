# Phân tích chuyên sâu M04 — Ôn tập ngắt quãng và tiến độ

## 1. Mục tiêu và phạm vi

M04 duy trì một hồ sơ ghi nhớ riêng cho từng cặp người dùng–nghĩa từ vựng, chuyển kết quả học/ôn thành lịch ôn tiếp theo và cung cấp chỉ số tiến bộ có thể giải thích. Mục tiêu cuối không phải kéo dài khoảng ôn bằng mọi giá, mà là cân bằng khả năng duy trì kiến thức, khối lượng ôn và động lực người học.

### Trong phạm vi

- Khởi tạo và duy trì tiến độ theo người dùng–nghĩa từ.
- Tiếp nhận kết quả chính thức từ M03 và tín hiệu kỹ năng phát âm từ M05.
- Chuyển bằng chứng nhớ thành mức chất lượng có lý do/phiên bản.
- Tính trạng thái nhớ, tham số, thời điểm ôn tiếp theo và điểm duy trì.
- Chọn/sắp xếp từ đến hạn và bàn giao cho M03.
- Lưu lịch sử trước/sau, tổng hợp theo từ/bộ/người dùng và phát tín hiệu M10/M11.
- Quản lý phiên bản chính sách, thay đổi cấu hình và tính giải thích.

### Ngoài phạm vi

- Tạo/chấm câu hỏi và điều phối phiên ôn.
- Sở hữu học liệu hoặc tự thay đổi nội dung từ.
- Đánh giá bản ghi phát âm.
- Gửi thông báo hoặc trao thưởng trực tiếp.

## 2. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Hồ sơ theo người dùng–từ lưu số lần đúng/sai, lần lặp, khoảng ôn, hệ số dễ, điểm chất lượng, trạng thái nhớ, thời điểm học/thuộc và ngày ôn tiếp.
- Ghi riêng kết quả gợi nhớ đầu, luyện trong phiên và luyện sửa.
- Lịch sử ôn lưu kết quả, tốc độ, gợi ý, điểm, lý do, phiên bản chính sách và giá trị trước/sau.
- Chọn từ đến hạn theo thời điểm rồi ưu tiên theo độ quá hạn và điểm duy trì.
- Có chính sách tham số hóa, kiểm tra giá trị và phiên bản.
- Tổng hợp số từ đến hạn, lần ôn gần nhất, trạng thái nhớ, tỷ lệ duy trì, từ khó, chủ đề và hoạt động tuần.
- Kết quả phát âm hiện có thể tăng hệ số dễ hoặc kéo lịch ôn gần hơn.

### Khoảng trống và rủi ro

- Chưa thấy mã kết quả nguồn được lưu như khóa chống áp dụng lại; gửi lại từ M03/M05 có thể thay đổi tiến độ nhiều lần.
- Hồ sơ theo “từ” cần điều chỉnh theo quyết định nhiều nghĩa của M02 để tránh trộn bằng chứng của các nghĩa khác nhau.
- Các trạng thái mới/đang học/ôn/đã thuộc đang dựa vào tham số nhưng định nghĩa sản phẩm và khả năng rời trạng thái đã thuộc chưa rõ.
- Thời điểm đến hạn dùng thời gian tuyệt đối; cách trình bày theo múi giờ, đổi vùng và ngày học chưa được chuẩn hóa.
- Điểm duy trì là ước lượng nhưng dễ bị hiểu thành phần trăm chắc chắn.
- Thay đổi chính sách có phiên bản nhưng chưa rõ có tính lại dữ liệu cũ, chỉ áp dụng lần tới hay chạy thử trước.
- Tác động phát âm đang trộn kỹ năng phát âm với nhớ nghĩa; cần quyết định học thuật trước khi giữ.

## 3. Từ điển nghiệp vụ sơ bộ

| Thuật ngữ | Định nghĩa đề xuất |
|---|---|
| Hồ sơ ghi nhớ | Trạng thái hiện tại của một người dùng đối với một nghĩa/cách dùng cụ thể |
| Bằng chứng nhớ | Kết quả gợi nhớ hợp lệ từ M03, kèm tốc độ, gợi ý và số lần thử |
| Điểm chất lượng | Giá trị chuẩn hóa diễn giải chất lượng của một bằng chứng nhớ, không phải điểm thưởng |
| Khoảng ôn | Thời lượng dự kiến từ một lần cập nhật hợp lệ đến lần ôn kế tiếp |
| Đến hạn | Thời điểm hiện tại bằng hoặc sau thời điểm ôn tiếp theo theo đồng hồ chuẩn |
| Quá hạn | Mức thời gian đã trôi qua sau thời điểm đến hạn |
| Điểm duy trì | Ước lượng tương đối về khả năng còn nhớ, không phải xác suất được bảo đảm |
| Đã thuộc | Trạng thái đạt điều kiện khoảng ôn/lần lặp theo chính sách; vẫn có thể quên và trở lại ôn |

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Khởi tạo hồ sơ ghi nhớ | Tạo đúng một hồ sơ khi người dùng bắt đầu học nghĩa từ | Điều kiện tạo, giá trị ban đầu, chống trùng | Tạo nội dung | M01, M02, M03 |
| SF02 | Tiếp nhận bằng chứng học/ôn | Áp dụng mỗi kết quả hợp lệ đúng một lần | Mã nguồn, xác thực, thứ tự, gửi lại, đồng thời | Chấm câu trả lời | M03 |
| SF03 | Phân loại chất lượng nhớ | Chuyển kết quả M03 thành mức chất lượng giải thích được | Đúng/sai, tốc độ, gợi ý, lần thử, lý do | Tạo câu hỏi | M03, M11 |
| SF04 | Tính lịch ôn và trạng thái nhớ | Cập nhật tham số, trạng thái và ngày ôn | Chính sách tính, giới hạn và hồi quy trạng thái | Nhắc người dùng | M11 |
| SF05 | Chọn và ưu tiên từ đến hạn | Tạo đầu vào ôn phù hợp với khối lượng người học | Đến hạn, quá hạn, ưu tiên, giới hạn, loại trừ | Tạo phiên ôn | M02, M03 |
| SF06 | Lịch sử và khả năng giải thích | Giải thích mọi thay đổi tiến độ | Trước/sau, nguồn, chính sách, lý do, truy vết | Lịch sử câu trả lời chi tiết | M03, M11 |
| SF07 | Kỹ năng phát âm trong tiến độ | Theo dõi phát âm mà không làm sai nhớ nghĩa | Tín hiệu M05, hồ sơ kỹ năng, tác động tùy chính sách | Đánh giá âm thanh | M05 |
| SF08 | Tiến độ bộ từ và người dùng | Cung cấp bức tranh tổng hợp không gây hiểu lầm | Theo từ/bộ/trạng thái, từ khó, duy trì, hoạt động | Xếp hạng xã hội | M02, M09, M11 |
| SF09 | Tín hiệu nhắc ôn và module tiêu thụ | Cung cấp thời điểm/khối lượng cho M03/M10/M11 | Danh sách ôn, thời điểm kế, tín hiệu thay đổi | Gửi thông báo | M03, M10, M11 |
| SF10 | Quản trị chính sách và chất lượng | Thay đổi chính sách an toàn, đo hiệu quả và phục hồi | Phiên bản, kiểm tra, thử nghiệm, chuyển đổi, cảnh báo | Tự quyết định chiến lược sản phẩm | M11 |

## 5. Phân tích chi tiết

### SF01 — Khởi tạo hồ sơ ghi nhớ

**Business flow:** khi M03 ghi bằng chứng học đầu tiên, M04 xác định đúng người và nghĩa từ; kiểm tra hồ sơ hiện có; tạo một hồ sơ với trạng thái/giá trị ban đầu theo phiên bản chính sách; lưu nguồn khởi tạo.

**Edge case:** hai phiên tạo đồng thời; cùng mặt chữ khác nghĩa; từ bị thu hồi; người dùng đánh dấu đã biết; hồ sơ từng bị đặt lại; dữ liệu cũ thiếu phiên bản.

**DoD:** mỗi người–nghĩa có tối đa một hồ sơ hoạt động; tạo lặp trả cùng hồ sơ; giá trị ban đầu có phiên bản/lý do; không tạo cho nội dung không hợp lệ.

### SF02 — Tiếp nhận bằng chứng học/ôn

**Business flow:** M04 nhận kết quả có mã duy nhất từ M03; kiểm tra nguồn, người, nghĩa, loại phiên và phiên bản; phát hiện đã xử lý; áp dụng theo thứ tự hợp lệ; ghi trạng thái xử lý và trả kết quả có thể gửi lại.

**Edge case:** gửi lặp; hai kết quả cùng lúc; kết quả cũ đến sau mới; cùng mã khác nội dung; phiên bị hủy; hồ sơ không tồn tại; lỗi sau khi cập nhật một phần.

**DoD:** một mã nguồn chỉ thay đổi tiến độ một lần; gửi lại trả cùng kết quả; thứ tự sai có xử lý rõ; cập nhật tiến độ và lịch sử là một kết quả nhất quán.

### SF03 — Phân loại chất lượng nhớ

**Business flow:** M04 lấy kết quả gợi nhớ đầu, đúng/sai, tốc độ, gợi ý và số lần thử; áp chính sách chấm; tạo điểm chất lượng và lý do; lưu phiên bản; chuyển cho chính sách lịch ôn.

**Edge case:** đúng nhưng rất chậm/nhiều gợi ý; sai rồi sửa đúng; thẻ xem không phải bằng chứng nhớ; thời gian bất thường; dữ liệu luồng cũ thiếu trường; kết quả gần đúng.

**DoD:** cùng bằng chứng/chính sách cho cùng điểm; lý do giải thích được; chỉ bằng chứng được duyệt làm thay đổi lịch; không dùng bước học thụ động như bằng chứng nhớ hoàn hảo.

### SF04 — Tính lịch ôn và trạng thái nhớ

**Business flow:** M04 đọc hồ sơ và chính sách phiên bản; tính tham số mới, khoảng ôn, điểm duy trì, trạng thái và ngày ôn; áp giới hạn; lưu trước/sau; đánh dấu thời điểm đạt/rời trạng thái đã thuộc.

**Edge case:** điểm ngoài phạm vi; khoảng âm/quá dài; đồng hồ lùi; trạng thái đã thuộc nhưng quên; chính sách lỗi; ngày ôn rơi qua đổi múi giờ; cập nhật đồng thời.

**DoD:** tham số luôn trong giới hạn; trạng thái có điều kiện vào/ra; ngày ôn dùng đồng hồ chuẩn; mọi thay đổi có phiên bản/lý do; chính sách lỗi dùng phương án an toàn và cảnh báo.

### SF05 — Chọn và ưu tiên từ đến hạn

**Business flow:** M04 lấy hồ sơ hoạt động có ngày ôn không sau hiện tại; loại nội dung không hợp lệ/đang nằm trong phiên ôn mở theo chính sách; tính ưu tiên từ độ quá hạn, điểm duy trì và độ khó; áp giới hạn; trả danh sách cho M03.

**Edge case:** hàng nghìn từ quá hạn; nhiều nghĩa cùng mặt chữ; từ bị thu hồi; người dùng mới; đổi múi giờ; cùng từ đã có phiên; điểm duy trì bằng nhau; dữ liệu thiếu.

**DoD:** danh sách duy nhất, ổn định và giải thích được; không chứa nội dung bị chặn; có quy tắc khi quá nhiều/không có từ; cùng thời điểm cho cùng thứ tự nếu dữ liệu không đổi.

### SF06 — Lịch sử và khả năng giải thích

**Business flow:** mỗi cập nhật ghi mã nguồn, bằng chứng, điểm/lý do, chính sách, tham số/ngày/trạng thái trước–sau; người dùng xem diễn giải đơn giản; quản trị xem chi tiết theo quyền.

**Edge case:** lịch sử bị trùng; dữ liệu cũ thiếu phiên bản; nội dung bị thu hồi; xóa tài khoản; sửa thủ công; lượng lịch sử lớn.

**DoD:** tái dựng được vì sao ngày ôn thay đổi; lịch sử không bị sửa âm thầm; quyền/che dữ liệu rõ; điều chỉnh tạo bản ghi bù thay vì viết lại.

### SF07 — Kỹ năng phát âm trong tiến độ

**Business flow:** M04 nhận kết quả phát âm hợp lệ có mã/phiên bản; cập nhật chỉ số phát âm riêng; nếu chính sách đã duyệt cho phép, tạo tác động nhỏ có lý do lên lịch nhớ nghĩa; gửi lại an toàn.

**Edge case:** lỗi kỹ thuật; gửi lặp; kết quả khác biến thể; phát âm tốt nhưng không nhớ nghĩa; phát âm sai nhiều lần; chính sách tác động bị tắt; kết quả cũ đến muộn.

**DoD:** phát âm có hồ sơ/chỉ số tách biệt; lỗi kỹ thuật không tác động; cùng mã áp một lần; tác động lên nhớ nghĩa chỉ tồn tại khi có quyết định, giới hạn và khả năng giải thích.

### SF08 — Tiến độ bộ từ và người dùng

**Business flow:** M04 tổng hợp hồ sơ theo trạng thái, cấp độ, chủ đề/bộ; tính số đến hạn, thời điểm tiếp theo, điểm duy trì tổng hợp, từ khó và xu hướng; hiển thị định nghĩa/cảnh báo phù hợp.

**Edge case:** một từ thuộc nhiều bộ; bộ đổi thành phần; ít dữ liệu; dữ liệu rất cũ; từ thu hồi; người dùng bỏ bộ; nhiều chính sách trong cùng kỳ.

**DoD:** không đếm trùng ngoài chủ đích; công thức công khai trong tài liệu; dữ liệu thiếu/không tương thích có nhãn; không gọi hoàn thành bộ là nhớ lâu nếu chưa đủ bằng chứng.

### SF09 — Tín hiệu nhắc ôn và module tiêu thụ

**Business flow:** M04 công bố số từ đến hạn, thời điểm ôn gần nhất và thay đổi đáng chú ý; M03 yêu cầu danh sách khi tạo phiên; M10 quyết định có gửi nhắc; M11 theo dõi khối lượng/chất lượng.

**Edge case:** lịch đổi sau khi đã lên lịch nhắc; người dùng tắt nhắc; không có từ; nhiều cập nhật liên tiếp; múi giờ đổi; tài khoản bị khóa.

**DoD:** M04 chỉ cung cấp tín hiệu, không gửi tin; sự kiện có mã/phiên bản; M10 nhận lại an toàn; trạng thái tài khoản/quyền nhận tin do module sở hữu kiểm soát.

### SF10 — Quản trị chính sách và chất lượng

**Business flow:** người có quyền đề xuất bộ tham số mới; hệ thống kiểm tra giá trị; chạy mô phỏng trên dữ liệu đại diện; phê duyệt và đặt thời điểm hiệu lực; áp dụng cho cập nhật mới hoặc chuyển đổi theo quyết định; theo dõi chỉ số và quay lại khi bất thường.

**Edge case:** tham số không hợp lệ; thay đổi làm bùng nổ số từ đến hạn; nhiều phiên bản đồng thời; quay lại nhưng dữ liệu đã cập nhật; người dùng trong nhóm thử nghiệm; cấu hình thiếu.

**DoD:** không kích hoạt cấu hình sai; mỗi chính sách có phiên bản/người duyệt/lý do; chiến lược dữ liệu cũ rõ; có mô phỏng, chỉ số, ngưỡng dừng và kế hoạch phục hồi.

## 6. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Hồ sơ và bằng chứng đúng một lần | SF01, SF02 | M04-T001–M04-T009 |
| Chấm và lập lịch giải thích được | SF03, SF04 | M04-T010–M04-T019 |
| Danh sách ôn và lịch sử đáng tin | SF05, SF06 | M04-T020–M04-T027 |
| Phát âm và tổng hợp tiến độ đúng ranh giới | SF07, SF08 | M04-T028–M04-T035 |
| Tín hiệu và chính sách vận hành an toàn | SF09, SF10 | M04-T036–M04-T044 |
| Nghiệm thu và bàn giao | SF01–SF10 | M04-T045–M04-T046 |

## 7. Thứ tự thực hiện đề xuất

1. Chốt đơn vị tiến độ người dùng–nghĩa, mã nguồn và chống xử lý lặp.
2. Chốt chấm chất lượng, trạng thái nhớ, lịch ôn và múi giờ.
3. Chốt danh sách đến hạn, lịch sử và giải thích.
4. Chốt ranh giới phát âm, công thức tổng hợp và tín hiệu M10.
5. Chốt quản trị phiên bản, mô phỏng, nghiệm thu và bàn giao.

## 8. Cơ sở quyết định đã chốt

Tất cả 25 quyết định M04 đã được chốt. Tiến độ gắn riêng với từng nghĩa/cách dùng và chỉ được tạo từ bằng chứng học hợp lệ hoặc bài kiểm tra nhanh của thao tác “đã biết”. Thêm bộ hoặc xem thẻ không tạo tiến độ. Người dùng có thể đặt lại từng nghĩa hoặc toàn bộ bộ sau xác nhận; lịch sử cũ vẫn được giữ.

Mỗi bằng chứng chỉ được áp dụng một lần. Chất lượng nhớ, trạng thái, ngày đến hạn và điểm duy trì phải giải thích được, dùng múi giờ hồ sơ và không được trình bày như xác suất chắc chắn. Phát âm là chiều tiến độ riêng; kết quả phát âm không tự thay đổi khả năng nhớ nghĩa. Bỏ bộ khỏi thư viện vẫn giữ tiến độ toàn cục.

Chính sách mới mặc định chỉ áp dụng từ bằng chứng tiếp theo; không tính lại hàng loạt trừ sửa lỗi nghiêm trọng đã mô phỏng và có điều chỉnh truy vết. Thử nghiệm chính sách phải phân nhóm ổn định, lưu phiên bản và đo cả ghi nhớ lẫn tác động tiêu cực.

## 9. Rủi ro còn hiệu lực và điều kiện phát hành

- Áp dụng cùng kết quả hai lần làm lịch ôn trôi xa và số liệu sai khó phát hiện.
- Trộn nhiều nghĩa vào một hồ sơ làm bằng chứng nhớ không còn đúng đối tượng.
- Thay đổi tham số không mô phỏng có thể tạo “bão ôn tập” và làm người dùng bỏ cuộc.
- Điểm duy trì hiển thị như xác suất chắc chắn gây hiểu lầm.
- Trộn phát âm với nhớ nghĩa thiếu bằng chứng có thể phạt người biết nghĩa nhưng khác giọng.

Không còn quyết định mở trong M04. `QUYET-DINH-MO.md` là nguồn chi tiết; thay đổi công thức, trạng thái hoặc cách chuyển đổi dữ liệu phải tạo quyết định và phiên bản chính sách mới.
