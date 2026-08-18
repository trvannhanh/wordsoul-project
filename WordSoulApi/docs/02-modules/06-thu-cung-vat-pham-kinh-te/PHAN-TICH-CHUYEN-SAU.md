# Phân tích chuyên sâu M06 — Thú cưng, vật phẩm và kinh tế phần thưởng

## 1. Mục tiêu và phạm vi

M06 là nguồn sự thật duy nhất về tài sản gamification của người dùng: đơn vị giá trị, thú cưng, vật phẩm, số lượng, cấp độ, trạng thái kích hoạt và mọi biến động. Module nhận “điều kiện nhận thưởng” từ module khác, kiểm tra chính sách rồi ghi một biến động đúng một lần; module nguồn không được tự thay đổi số dư.

### Trong phạm vi

- Danh mục và vòng đời thú cưng, vật phẩm, độ hiếm, loại, hình ảnh và quan hệ tiến hóa.
- Định nghĩa đơn vị giá trị như kinh nghiệm, điểm thành tựu và lượt gợi ý.
- Quyền sở hữu, kho đồ, số lượng, thú cưng trùng, nâng cấp và tiến hóa.
- Chọn thú cưng hoạt động/yêu thích và tính hiệu ứng hợp lệ.
- Nhận/cấp/trừ/thu hồi thưởng đúng một lần từ M03, M05, M07 và M08.
- Điều chỉnh quản trị, sổ biến động, đối soát, chỉ số kinh tế và cân bằng.

### Ngoài phạm vi

- Xác định người dùng đã hoàn thành phiên, nhiệm vụ hay thắng trận.
- Sở hữu liên kết nội dung bộ từ–phần thưởng; M02 sở hữu cấu hình liên kết.
- Điều hành trận đấu hoặc chấm câu trả lời.
- Tự quyết định chiến lược cân bằng không qua M11.

## 2. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Danh mục thú cưng có độ hiếm, loại chính/phụ, thứ tự, dạng gốc, dạng tiến hóa, cấp yêu cầu và trạng thái hoạt động.
- Quyền sở hữu thú cưng có cấp độ, kinh nghiệm, yêu thích, hoạt động và thời điểm nhận.
- Nếu nhận thú cưng đã sở hữu, hệ thống chuyển thành kinh nghiệm; nếu chưa có, có thể nhận theo tỷ lệ.
- Có nâng cấp/tiến hóa thú cưng, chọn thú cưng hoạt động và mô tả hiệu ứng theo loại/độ hiếm.
- Danh mục vật phẩm và kho vật phẩm theo số lượng đã tồn tại; có thêm/trừ kho.
- Kinh nghiệm người dùng, điểm thành tựu và lượt gợi ý tồn tại dưới dạng số dư; quản trị có thể điều chỉnh có lý do.

### Khoảng trống và rủi ro

- Chưa thấy sổ biến động thống nhất; nhiều luồng thay đổi trực tiếp số dư nên khó đối soát và chống lặp.
- Mã sự kiện nguồn chưa được dùng như khóa cấp thưởng duy nhất trên toàn bộ loại tài sản.
- Danh mục vật phẩm có nhưng vòng đời sử dụng, hiệu lực, hết hạn và hoàn trả chưa rõ.
- Hiệu ứng hiện chủ yếu phục vụ hiển thị; quy tắc áp dụng, cộng dồn và ảnh chụp theo phiên chưa được quản trị thống nhất.
- Thú cưng trùng tự đổi sang kinh nghiệm nhưng tỷ lệ, trần và tính minh bạch chưa được xác nhận.
- Xóa danh mục thú cưng/vật phẩm đang được sở hữu có thể làm mất toàn vẹn.
- Điểm kinh nghiệm/điểm thành tựu/lượt gợi ý nằm cùng hồ sơ người dùng nhưng trách nhiệm nguồn sự thật cần chuyển rõ về M06.

## 3. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Danh mục tài sản | Quản lý thú cưng/vật phẩm ổn định, có vòng đời | Tạo, sửa, kích hoạt, ngừng dùng, tài sản số | Vận hành kho ảnh | M11, M12 |
| SF02 | Mô hình đơn vị giá trị và sổ biến động | Tạo nguồn sự thật kiểm toán được | Loại giá trị, số dư, biến động, nguồn, điều chỉnh | Xếp hạng | M01, M11 |
| SF03 | Tiếp nhận và xử lý thưởng | Cấp mỗi sự kiện đúng một lần | Kiểm tra nguồn, chính sách, chống lặp, lỗi, thu hồi | Xác định hoàn thành nguồn | M03, M05, M07, M08 |
| SF04 | Sở hữu và nhận thú cưng | Quản lý bộ sưu tập và thú cưng trùng | Nhận, tỷ lệ, trùng, chuyển đổi, lịch sử | Cấu hình bộ–thưởng | M02 |
| SF05 | Phát triển và tiến hóa thú cưng | Tạo tiến trình dài hạn có kiểm soát | Kinh nghiệm pet, cấp, điều kiện, tiến hóa | Kết quả học/trận | M03, M08, M11 |
| SF06 | Trạng thái sử dụng thú cưng | Chọn đúng thú cưng hoạt động/yêu thích | Kích hoạt, thay thế, yêu thích, đội hình tham chiếu | Điều hành trận | M08 |
| SF07 | Hiệu ứng gamification | Cung cấp hiệu ứng phiên bản hóa, công bằng | Loại, giá trị, trần, cộng dồn, ảnh chụp | Tự áp luật phiên/trận | M03, M05, M08, M11 |
| SF08 | Kho và sử dụng vật phẩm | Quản lý số lượng, tiêu thụ và hiệu lực | Thêm, trừ, dùng, hết hạn, hoàn trả | Nội dung hành động học | M03, M11 |
| SF09 | Điều chỉnh và đối soát | Sửa sai có truy vết, không viết lại lịch sử | Điều chỉnh quản trị, thu hồi, đối soát, chênh lệch | Sửa kết quả nguồn | M11 |
| SF10 | Phân tích và cân bằng kinh tế | Kiểm soát lạm phát, chi phí và công bằng | Nguồn vào/ra, tích lũy, cảnh báo, mô phỏng | Quyết định sản phẩm cuối cùng | M11 |

## 4. Phân tích chi tiết

### SF01 — Danh mục tài sản

**Business flow:** quản trị tạo bản nháp tài sản; khai báo loại, độ hiếm, mô tả, hình ảnh và quan hệ; hệ thống kiểm tra trùng/quan hệ vòng; duyệt và kích hoạt; thay đổi lớn tạo phiên bản; tài sản đang sở hữu chỉ được ngừng dùng, không xóa mất lịch sử.

**Edge case:** tên trùng; tiến hóa vòng; dạng tiếp theo không hoạt động; tài sản đang là thú cưng hoạt động/phần thưởng; ảnh lỗi; sửa độ hiếm làm thay đổi hiệu ứng.

**DoD:** có trạng thái/phiên bản/quyền; quan hệ hợp lệ; không xóa cứng tài sản đã tham chiếu; mọi thay đổi có tác động và lịch sử.

### SF02 — Mô hình đơn vị giá trị và sổ biến động

**Business flow:** mỗi thay đổi số dư tạo một biến động gồm người dùng, loại giá trị, lượng, nguồn, mã sự kiện, lý do và số dư trước/sau; hệ thống kiểm tra quy tắc không âm/trần; cập nhật số dư và sổ như một kết quả nhất quán.

**Edge case:** hai thay đổi đồng thời; gửi lại cùng sự kiện; số lượng 0/âm sai dấu; tràn giới hạn; dữ liệu cũ không có sổ; xóa tài khoản.

**DoD:** mọi số dư giải thích được bằng sổ; mã nguồn không áp hai lần; không âm ngoài chính sách; điều chỉnh không sửa bản ghi cũ; đối soát được.

### SF03 — Tiếp nhận và xử lý thưởng

**Business flow:** M06 nhận sự kiện đủ điều kiện; kiểm tra nguồn, người dùng, chính sách/phiên bản và việc đã xử lý; tính gói thưởng; tạo biến động/tài sản; trả kết quả; module nguồn có thể gửi lại an toàn.

**Edge case:** nhiều module gửi cùng thành tích; chính sách đổi; tài khoản khóa; tài sản ngừng dùng; xử lý một phần; thu hồi sự kiện nguồn; phần thưởng ngẫu nhiên.

**DoD:** mỗi sự kiện một kết quả bất biến; lỗi một phần không làm mất cân bằng; có trạng thái chờ/thành công/thất bại/thu hồi; module nguồn không trực tiếp ghi số dư.

### SF04 — Sở hữu và nhận thú cưng

**Business flow:** M06 xác định thú cưng/tỷ lệ theo cấu hình hiệu lực; ghi kết quả ngẫu nhiên có thể kiểm tra; nếu chưa có thì tạo quyền sở hữu; nếu trùng áp chính sách chuyển đổi; thú đầu tiên có thể được kích hoạt theo lựa chọn đã duyệt.

**Edge case:** tỷ lệ ngoài phạm vi; thú bị ngừng dùng; nhận lặp; trùng ở hai yêu cầu đồng thời; chuyển đổi vượt trần; người dùng đã có nhiều thú hoạt động.

**DoD:** kết quả ngẫu nhiên có cấu hình/nguồn/thời điểm; quyền sở hữu không trùng ngoài chính sách; chuyển đổi minh bạch; gửi lại không nhận thêm.

### SF05 — Phát triển và tiến hóa thú cưng

**Business flow:** sự kiện hợp lệ cấp kinh nghiệm pet; M06 cập nhật sổ/cấp; khi đủ điều kiện người dùng yêu cầu tiến hóa; hệ thống kiểm tra chuỗi, chi phí và trạng thái; chốt dạng mới và bảo toàn lịch sử.

**Edge case:** tăng nhiều cấp một lần; tiến hóa cuối; thiếu điểm; hai yêu cầu đồng thời; dạng đích ngừng dùng; hoàn trả khi lỗi; thú đang dùng trong phiên/trận.

**DoD:** công thức cấp/chi phí có phiên bản; không trừ hai lần; tiến hóa không mất quyền/lịch sử; phiên/trận đang chạy giữ ảnh chụp cũ.

### SF06 — Trạng thái sử dụng thú cưng

**Business flow:** người dùng xem bộ sưu tập; chọn yêu thích hoặc thú hoạt động; M06 kiểm tra quyền sở hữu/trạng thái; tắt lựa chọn cũ theo quy tắc và bật mới; cung cấp ảnh chụp cho M03/M08.

**Edge case:** chọn thú không sở hữu; hai thiết bị; thú bị ngừng dùng; xóa tài khoản; đội hình trận dùng thú khác thú hoạt động; không có thú nào.

**DoD:** tối đa số thú hoạt động theo chính sách; cập nhật đồng thời xác định; module khác nhận phiên bản/trạng thái; không tự sửa ảnh chụp phiên đang chạy.

### SF07 — Hiệu ứng gamification

**Business flow:** quản trị định nghĩa hiệu ứng theo loại/độ hiếm/cấp; mô phỏng và duyệt; M06 tính hiệu ứng của tài sản hợp lệ; module tiêu thụ yêu cầu ảnh chụp; áp dụng theo trách nhiệm riêng và trả kết quả sử dụng.

**Edge case:** loại chính/phụ trùng; cộng dồn vượt trần; chính sách đổi giữa phiên; hiệu ứng biến sai thành đúng; tài sản hết hiệu lực; nhiều nguồn hiệu ứng.

**DoD:** mỗi hiệu ứng có phạm vi, trần, thứ tự và phiên bản; không tác động đúng/sai học thuật; ảnh chụp bất biến trong phiên; có thể tắt khẩn cấp.

### SF08 — Kho và sử dụng vật phẩm

**Business flow:** M06 nhận cấp vật phẩm và ghi kho; người dùng yêu cầu dùng; hệ thống kiểm tra quyền, số lượng, trạng thái, thời hạn và ngữ cảnh; giữ/trừ đúng một lần; module đích xác nhận; lỗi theo chính sách hoàn trả.

**Edge case:** số lượng không đủ; vật phẩm hết hạn; dùng đồng thời; module đích thất bại; vật phẩm ngừng dùng; kho về 0; sử dụng sai ngữ cảnh.

**DoD:** thêm/trừ có sổ; không âm; dùng lặp không trừ hai lần; hiệu lực/hoàn trả rõ; bản ghi kho 0 có chính sách nhất quán.

### SF09 — Điều chỉnh và đối soát

**Business flow:** quản trị có quyền chọn người/tài sản; xem số dư và lịch sử; nhập thay đổi/lý do/bằng chứng; hệ thống xem trước tác động, yêu cầu phê duyệt nếu nhạy cảm; tạo biến động điều chỉnh; chạy đối soát phát hiện chênh lệch.

**Edge case:** điều chỉnh làm âm; tự phê duyệt; hai quản trị cùng sửa; thu hồi phần thưởng đã tiêu; chênh lệch dữ liệu cũ; tài khoản bị xóa.

**DoD:** không sửa lịch sử; có quyền/phê duyệt/lý do/trước-sau; chênh lệch có trạng thái xử lý; thu hồi không tạo số dư âm ngoài chính sách.

### SF10 — Phân tích và cân bằng kinh tế

**Business flow:** M06 tổng hợp nguồn vào/ra/tích lũy theo loại; M11 theo dõi phân bố, lạm phát, tỷ lệ trùng, hiệu ứng và chi phí; thay đổi chính sách được mô phỏng, duyệt, đặt hiệu lực và theo dõi.

**Edge case:** người dùng cũ tích lũy quá lớn; thay đổi làm giảm giá trị tài sản; sự kiện bất thường; nhóm người dùng nhận thưởng chênh lệch; dữ liệu trước sổ biến động.

**DoD:** chỉ số có công thức; mô phỏng trước thay đổi lớn; có ngưỡng/cảnh báo/kế hoạch quay lại; không giảm tài sản người dùng âm thầm.

## 5. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Danh mục và nguồn sự thật kinh tế | SF01, SF02 | M06-T001–M06-T010 |
| Cấp thưởng và sở hữu đúng một lần | SF03, SF04 | M06-T011–M06-T020 |
| Phát triển, trạng thái và hiệu ứng | SF05, SF06, SF07 | M06-T021–M06-T033 |
| Kho, điều chỉnh và cân bằng | SF08, SF09, SF10 | M06-T034–M06-T045 |
| Nghiệm thu và bàn giao | SF01–SF10 | M06-T046–M06-T047 |

## 6. Thứ tự thực hiện đề xuất

1. Chốt đơn vị giá trị, danh mục và sổ biến động.
2. Chốt hợp đồng sự kiện thưởng và chống cấp lặp.
3. Chốt sở hữu, thú trùng, nâng cấp/tiến hóa và kích hoạt.
4. Chốt hiệu ứng, kho vật phẩm và sử dụng/hoàn trả.
5. Chốt điều chỉnh, đối soát, cân bằng, nghiệm thu và bàn giao.

## 7. Cơ sở quyết định đã chốt

Tất cả 25 quyết định M06 đã được chốt. XP người dùng và XP thú cưng là hai giá trị riêng; AP ngừng cấp/sử dụng, số AP khả dụng cũ bị loại bỏ không chuyển đổi hoặc bồi hoàn nhưng lịch sử được giữ. Phạm vi không có tiền mềm, cửa hàng hay giao dịch giữa người dùng. Lịch sử biến động bất biến là nguồn sự thật và không cho số dư âm.

Thú cưng trùng có ba lựa chọn: nâng sao, giữ cá thể riêng với hướng chỉ số khác hoặc đổi thành vật phẩm tăng trưởng. Mỗi thú có một kỹ năng học tập hoặc chiến đấu; người dùng tự chọn tối đa ba thú hoạt động trong học tập, còn đội hình thi đấu theo M08. Hiệu ứng không được thay đổi đúng/sai hay trạng thái thành thạo và luôn có trần/phiên bản.

Nhóm vật phẩm lõi gồm lượt gợi ý, thức ăn tăng XP thú cưng, vật phẩm tiến hóa và vật phẩm sự kiện; các loại mở rộng chỉ được kích hoạt sau khi xác định mục đích, nguồn cấp/tiêu, hiệu lực và hoàn trả.

## 8. Rủi ro còn hiệu lực và điều kiện phát hành

- Không có sổ biến động khiến số dư không thể giải thích hoặc phục hồi đáng tin cậy.
- Nhiều module tự cộng thưởng tạo cấp lặp và lệch nguồn sự thật.
- Hiệu ứng không có trần có thể phá công bằng học/thi đấu.
- Xóa tài sản danh mục đang được sở hữu phá bộ sưu tập và lịch sử.
- Thay đổi tỷ lệ ngẫu nhiên không phiên bản hóa tạo tranh chấp phần thưởng.

- Việc loại bỏ AP không bồi hoàn là quyết định sản phẩm có nguy cơ khiếu nại cao; phải thông báo rõ và bảo toàn lịch sử trước khi thực hiện.
- Không được kích hoạt loại vật phẩm mới nếu chưa có vòng đời, nguồn cấp/tiêu, chính sách hết hạn và hoàn trả.

Không còn quyết định mở trong M06; `QUYET-DINH-MO.md` là nguồn chi tiết.
