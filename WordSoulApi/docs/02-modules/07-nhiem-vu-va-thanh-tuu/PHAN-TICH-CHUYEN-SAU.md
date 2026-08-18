# Phân tích chuyên sâu M07 — Nhiệm vụ và thành tựu

## 1. Mục tiêu và phạm vi

M07 chuyển các kết quả học tập hợp lệ thành mục tiêu ngắn hạn và cột mốc dài hạn. Module giúp người học biết nên làm gì tiếp theo, thấy tiến bộ rõ ràng và nhận phần thưởng công bằng mà không làm suy giảm chất lượng học.

### Trong phạm vi

- Định nghĩa, phiên bản và vòng đời nhiệm vụ/thành tựu.
- Phân bổ nhiệm vụ theo chu kỳ, điều kiện và mức phù hợp với người học.
- Tiếp nhận sự kiện từ các hoạt động học, ôn tập, phát âm và thi đấu đã được module nguồn xác nhận.
- Đếm tiến độ đúng một lần, hoàn thành, mở khóa và lưu lịch sử.
- Xác định quyền nhận thưởng và chuyển yêu cầu hợp lệ sang M06.
- Hiển thị trạng thái, phát tín hiệu thông báo và đo hiệu quả của mục tiêu.

### Ngoài phạm vi

- Chấm đúng/sai, chấm phát âm hoặc xác định kết quả trận đấu.
- Trực tiếp thay đổi kinh nghiệm, điểm, vật phẩm hoặc thú cưng của người dùng.
- Tự gửi thông báo ra thiết bị.
- Quản lý nội dung từ vựng, lịch ôn tập hay luật thi đấu.

## 2. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Đã có định nghĩa nhiệm vụ ngày với loại điều kiện, mục tiêu, phần thưởng và trạng thái hoạt động.
- Có bản ghi nhiệm vụ theo người dùng/ngày, tiến độ, trạng thái hoàn thành và trạng thái đã nhận thưởng.
- Đã có các nguồn đếm học mới, ôn tập, bắt thú, phát âm và độ chính xác.
- Đã có định nghĩa thành tựu, tiến độ dài hạn, thời điểm hoàn thành và nhận thưởng.
- Người học có thể xem nhiệm vụ hôm nay, thành tựu và yêu cầu nhận thưởng; quản trị có thể quản lý định nghĩa.

### Khoảng trống và rủi ro

- Chưa thấy định danh sự kiện nguồn dùng để chống đếm lặp và xử lý gửi lại.
- Ranh giới ngày, múi giờ, đổi múi giờ và sự kiện đến muộn chưa có chính sách rõ.
- Nhiệm vụ hiện có xu hướng gán toàn bộ định nghĩa đang hoạt động, chưa thể hiện chọn lọc/cá nhân hóa.
- Sửa định nghĩa có thể làm thay đổi mục tiêu hoặc phần thưởng của nhiệm vụ đang thực hiện nếu không có ảnh chụp phiên bản.
- Việc cấp phần thưởng trực tiếp từ M07 tạo nguy cơ trùng trách nhiệm với M06, lỗi một phần và nhận nhiều lần khi đồng thời.
- Thành tựu mới có thể không được bổ sung đầy đủ cho người dùng cũ; chính sách tính hồi tố chưa rõ.
- Xóa định nghĩa đang được tham chiếu có thể làm mất lịch sử hoặc quyền lợi.
- Chưa có chỉ số chứng minh nhiệm vụ cải thiện kết quả học thay vì chỉ tăng hoạt động hời hợt.

## 3. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Định nghĩa và vòng đời mục tiêu | Tạo mục tiêu ổn định, có kiểm soát | Loại, điều kiện, chu kỳ, phiên bản, kích hoạt/ngừng dùng | Chấm hoạt động nguồn | M11 |
| SF02 | Phân bổ nhiệm vụ ngày | Giao mục tiêu phù hợp và khả thi | Điều kiện đủ, lựa chọn, giới hạn, ảnh chụp khi giao | Lập lịch học chi tiết | M01, M04 |
| SF03 | Tiếp nhận sự kiện tiến độ | Nhận sự kiện nguồn tin cậy đúng một lần | Hợp đồng dữ liệu, xác thực nguồn, chống lặp, thứ tự | Tạo kết quả học gốc | M03, M04, M05, M08 |
| SF04 | Quy tắc đếm tiến độ | Chuyển sự kiện thành tiến độ đúng nghĩa | Ánh xạ, điều kiện, trần, tổ hợp, chống cày | Chấm chất lượng nguồn | M03, M04, M05, M08 |
| SF05 | Chu kỳ, ngày và múi giờ | Giữ ranh giới nhiệm vụ nhất quán | Ngày hiệu lực, đổi múi giờ, đến muộn, hết hạn | Quản lý múi giờ hồ sơ | M01 |
| SF06 | Hoàn thành và mở khóa | Ghi nhận cột mốc chính xác, bất biến | Chuyển trạng thái, thời điểm, hồi tố, thành tựu ẩn | Cấp tài sản | M04 |
| SF07 | Nhận và bàn giao phần thưởng | Đảm bảo mỗi quyền lợi được xử lý một lần | Điều kiện nhận, ảnh chụp gói thưởng, yêu cầu M06, thử lại | Ghi số dư/kho | M06 |
| SF08 | Thành tựu dài hạn | Theo dõi mục tiêu tích lũy bền vững | Khởi tạo, nhiều bậc, ẩn/hiện, lịch sử | Xếp hạng cộng đồng | M01, M04, M09 |
| SF09 | Trải nghiệm và lịch sử người học | Giúp hiểu mục tiêu và tiến độ | Danh sách, trạng thái, giải thích, lịch sử, tín hiệu | Gửi thông báo thiết bị | M10 |
| SF10 | Quản trị và đo hiệu quả | Vận hành an toàn và tối ưu giá trị học | Quyền, duyệt, kiểm toán, chỉ số, thử nghiệm, cảnh báo | Quyết định sản phẩm cuối cùng | M11 |

## 4. Phân tích chi tiết

### SF01 — Định nghĩa và vòng đời mục tiêu

**Business flow:** quản trị tạo bản nháp; chọn loại mục tiêu, nguồn sự kiện, điều kiện, chu kỳ, ngưỡng và gói thưởng; hệ thống kiểm tra tính hợp lệ và tác động; người có quyền duyệt; định nghĩa được kích hoạt theo thời điểm; thay đổi đáng kể tạo phiên bản mới; định nghĩa đã phát sinh tiến độ chỉ được ngừng dùng, không xóa lịch sử.

**Edge case:** ngưỡng bằng 0 hoặc không thể đạt; điều kiện không có nguồn sự kiện; thời gian hiệu lực chồng lấn; phần thưởng ngừng dùng; sửa khi người dùng đang thực hiện; xóa định nghĩa đã tham chiếu; hai phiên bản cùng hiệu lực.

**DoD:** mỗi định nghĩa có mã ổn định, trạng thái, hiệu lực, phiên bản và chủ sở hữu; quy tắc hợp lệ được tài liệu hóa; thay đổi có xem trước tác động và kiểm toán; bản đã giao không đổi ngoài chính sách được duyệt.

### SF02 — Phân bổ nhiệm vụ ngày

**Business flow:** khi bắt đầu chu kỳ, M07 xác định người dùng đủ điều kiện; lấy trình độ, lịch sử và khả năng hoạt động; loại mục tiêu không phù hợp; chọn số nhiệm vụ theo chính sách; lưu ảnh chụp điều kiện/phần thưởng và ranh giới thời gian; trả danh sách nhất quán trên mọi thiết bị.

**Edge case:** người dùng mới; không đủ mục tiêu hợp lệ; mục tiêu yêu cầu tính năng chưa mở; người dùng không có dữ liệu gần đây; mở ứng dụng đồng thời; tạo muộn trong ngày; nhiệm vụ trùng hành vi; mục tiêu vượt khả năng.

**DoD:** mỗi người chỉ có một tập nhiệm vụ hợp lệ cho một chu kỳ; việc tạo lại không sinh bản trùng; lý do đủ điều kiện/chọn lựa truy vết được; nhiệm vụ có thể hoàn thành bằng hoạt động sẵn có; ảnh chụp không bị sửa ngầm.

### SF03 — Tiếp nhận sự kiện tiến độ

**Business flow:** module nguồn gửi sự kiện đã xác nhận với người dùng, loại hoạt động, kết quả, thời điểm, định danh duy nhất và phiên bản ngữ nghĩa; M07 kiểm tra nguồn/phạm vi; ghi nhận sự kiện một lần; xác định các mục tiêu liên quan; chuyển sang tính tiến độ; trả trạng thái đã xử lý hoặc đã tồn tại.

**Edge case:** gửi lặp; đến sai thứ tự; đến muộn; thiếu trường bắt buộc; người dùng không tồn tại/bị khóa; nguồn rút lại kết quả; nhiều mục tiêu cùng nhận một sự kiện; sự kiện trước khi nhiệm vụ được giao; gửi đồng thời.

**DoD:** danh mục sự kiện và chủ sở hữu rõ; mỗi sự kiện nguồn chỉ được xử lý một lần; gửi lại an toàn; lỗi có lý do và khả năng xử lý lại; có chính sách cho rút lại/đính chính; theo dõi được từ nguồn đến tiến độ.

### SF04 — Quy tắc đếm tiến độ

**Business flow:** M07 đối chiếu sự kiện với điều kiện của phiên bản mục tiêu; kiểm tra thời gian, chất lượng và giới hạn; tính phần tăng; cập nhật tiến độ; chặn tại ngưỡng; nếu đạt thì chuyển sang hoàn thành. Một sự kiện có thể đóng góp cho nhiều mục tiêu độc lập nếu chính sách cho phép.

**Edge case:** học lại cùng từ; trả lời nhiều lần; phiên bị hủy; độ chính xác theo câu hay phiên; phát âm dưới ngưỡng; hoạt động luyện dễ bất thường; tiến độ âm do đính chính; đạt nhiều bậc cùng lúc; số đếm vượt trần.

**DoD:** mỗi loại mục tiêu có định nghĩa “một đơn vị tiến độ”; quy tắc chất lượng và giới hạn được duyệt; kết quả xác định với cùng đầu vào; không vượt mục tiêu; có kịch bản cho lặp, đồng thời, đính chính và chống cày.

### SF05 — Chu kỳ, ngày và múi giờ

**Business flow:** M07 lấy múi giờ hiệu lực của người dùng tại đầu chu kỳ; xác định thời điểm bắt đầu/kết thúc tuyệt đối; gán sự kiện theo thời điểm xảy ra và chính sách đến muộn; khi hết hạn khóa tiến độ, lưu kết quả; chu kỳ tiếp theo tạo tập mới.

**Edge case:** đổi múi giờ trong ngày; giờ mùa hè; đi qua đường đổi ngày; thiết bị sai giờ; sự kiện xảy ra trước nhưng đến sau; mất kết nối qua nửa đêm; tài khoản tạo gần cuối ngày; chạy bù nhiều ngày.

**DoD:** ranh giới chu kỳ được lưu và hiển thị rõ; đổi múi giờ không tạo hai ngày hoặc kéo dài vô hạn; thời gian nguồn và thời gian tiếp nhận được phân biệt; sự kiện đến muộn có chính sách nhất quán; kết thúc chu kỳ có thể chạy lại an toàn.

### SF06 — Hoàn thành và mở khóa

**Business flow:** sau mỗi cập nhật, M07 kiểm tra ngưỡng; khi đạt, ghi trạng thái và thời điểm hoàn thành duy nhất; đánh giá các thành tựu liên quan; mở khóa các cột mốc đủ điều kiện; phát tín hiệu cho hiển thị, phần thưởng và thông báo; lịch sử không bị mất khi định nghĩa ngừng dùng.

**Edge case:** hai sự kiện cùng đạt ngưỡng; đạt nhiều thành tựu/bậc cùng lúc; đính chính sau hoàn thành; tài khoản bị khóa; thành tựu tạo sau khi người dùng đã đủ điều kiện; điều kiện bị giảm; mở khóa trùng.

**DoD:** hoàn thành/mở khóa chỉ xảy ra một lần mỗi phiên bản và người dùng; thời điểm cùng bằng chứng được lưu; chính sách hồi tố/đính chính rõ; tín hiệu gửi lại không tạo mở khóa mới; lịch sử vẫn đọc được.

### SF07 — Nhận và bàn giao phần thưởng

**Business flow:** người dùng yêu cầu nhận hoặc chính sách tự nhận được kích hoạt; M07 kiểm tra quyền, trạng thái, thời hạn và chưa gửi thành công; tạo yêu cầu thưởng duy nhất từ ảnh chụp; M06 xử lý và trả kết quả; M07 cập nhật đang xử lý, thành công hoặc cần thử lại; người dùng xem được kết quả.

**Edge case:** nhấn nhận nhiều lần; hai thiết bị; M06 chậm/không phản hồi; gói thưởng có nhiều loại và lỗi một phần; tài sản đã ngừng dùng; người dùng bị khóa; phần thưởng hết hạn; M06 xử lý thành công nhưng phản hồi bị mất; yêu cầu thu hồi.

**DoD:** M07 không trực tiếp sửa tài sản; một quyền nhận có một định danh yêu cầu ổn định; gửi lại không cấp thêm; trạng thái giữa M07 và M06 đối soát được; lỗi không làm mất quyền; chính sách hết hạn/thu hồi được duyệt.

### SF08 — Thành tựu dài hạn

**Business flow:** M07 duy trì danh mục thành tựu theo chủ đề/bậc; người dùng được liên kết khi đủ điều kiện hoặc khi cần theo dõi; sự kiện tích lũy cập nhật cột mốc; đạt bậc mở khóa và có thể tiếp tục sang bậc kế; thành tựu ẩn chỉ tiết lộ theo chính sách; lịch sử phản ánh thành tích đã đạt.

**Edge case:** thêm thành tựu cho người dùng cũ; nhiều bậc cùng ngưỡng; đổi ngưỡng; thành tựu mùa vụ; thành tựu ẩn bị lộ qua mô tả; dữ liệu lịch sử không đủ để tính hồi tố; reset tiến độ; thành tựu ngừng dùng sau mở khóa.

**DoD:** quy tắc khởi tạo và hồi tố rõ; mỗi bậc có tiến độ/trạng thái riêng; ngừng dùng không xóa thành tích; nội dung ẩn không rò rỉ; tiến độ dài hạn có nguồn bằng chứng và có thể đối soát.

### SF09 — Trải nghiệm và lịch sử người học

**Business flow:** người học xem nhiệm vụ hôm nay và thành tựu; M07 trả mục tiêu, tiến độ, thời hạn, trạng thái, phần thưởng mô tả và giải thích cách đóng góp; khi thay đổi, giao diện nhận trạng thái nhất quán; người học xem lịch sử hoàn thành/nhận thưởng và xử lý trường hợp đang chờ.

**Edge case:** không có nhiệm vụ; ngoại tuyến; dữ liệu chậm; phần thưởng đang xử lý; định nghĩa ngừng dùng; thành tựu ẩn; ngôn ngữ thiếu nội dung; nhiều thiết bị hiển thị khác nhau; người dùng tranh chấp tiến độ.

**DoD:** trạng thái và hành động hợp lệ được định nghĩa; thời hạn theo múi giờ người dùng; lý do không được tính có thể hỗ trợ tra cứu; lịch sử phân biệt hoàn thành với nhận thưởng; tín hiệu cho M10 không chứa nội dung nhạy cảm thừa.

### SF10 — Quản trị và đo hiệu quả

**Business flow:** quản trị có quyền tạo/duyệt/lên lịch/ngừng mục tiêu; xem trước số người đủ điều kiện và chi phí thưởng; hệ thống ghi kiểm toán; sau phát hành, M11 theo dõi tỷ lệ giao, bắt đầu, hoàn thành, nhận thưởng, chất lượng học, chi phí và bất thường; thay đổi dựa trên thử nghiệm có kiểm soát.

**Edge case:** tự duyệt thay đổi nhạy cảm; mục tiêu hoàn thành quá cao/thấp; một nhóm bị loại không chủ ý; khuyến khích lặp hoạt động dễ; chi phí thưởng tăng đột biến; dữ liệu thiếu; thay đổi giữa thử nghiệm; quyền xem dữ liệu cá nhân.

**DoD:** quyền và phê duyệt theo mức ảnh hưởng; mọi thay đổi có lịch sử; chỉ số có định nghĩa và nhóm so sánh; có cảnh báo gian lận/chi phí/chất lượng; đánh giá hiệu quả học không chỉ dựa vào số lần tương tác.

## 5. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Định nghĩa và phân bổ mục tiêu ổn định | SF01, SF02 | M07-T001–M07-T010 |
| Nhận sự kiện và đếm tiến độ tin cậy | SF03, SF04 | M07-T011–M07-T021 |
| Quản lý chu kỳ, hoàn thành và mở khóa | SF05, SF06 | M07-T022–M07-T029 |
| Nhận thưởng và thành tựu dài hạn | SF07, SF08 | M07-T030–M07-T037 |
| Trải nghiệm, vận hành và đo hiệu quả | SF09, SF10 | M07-T038–M07-T044 |
| Nghiệm thu và bàn giao | SF01–SF10 | M07-T045–M07-T046 |

## 6. Thứ tự thực hiện đề xuất

1. Chốt thuật ngữ, vòng đời, phiên bản và ảnh chụp định nghĩa.
2. Chốt hợp đồng sự kiện, chống lặp và quy tắc đếm với từng module nguồn.
3. Chốt phân bổ, chu kỳ, múi giờ, hoàn thành và hồi tố.
4. Chốt bàn giao phần thưởng M06, trải nghiệm người học và thành tựu dài hạn.
5. Chốt quản trị, đo hiệu quả, nghiệm thu xuyên module và tài liệu bàn giao.

## 7. Cơ sở quyết định đã chốt

Tất cả 25 quyết định M07 đã được chốt. Mỗi ngày có ba nhiệm vụ, ít nhất một nhiệm vụ học/ôn cốt lõi và phải khả thi trong một phiên ngắn. Ngày học dùng múi giờ hồ sơ từ 00:00 đến trước 00:00; đổi múi giờ chỉ áp dụng từ ngày học tiếp theo. Bản đầu không cho đổi nhiệm vụ.

Chỉ bằng chứng đã xác nhận từ module sở hữu mới tăng tiến độ; một hoạt động có thể tăng nhiều mục tiêu độc lập nhưng không tăng các mục tiêu trùng ý nghĩa. Định nghĩa và phần thưởng được chụp khi giao. Người dùng chủ động nhận thưởng; nhiệm vụ ngày có ân hạn 7 ngày và quyền lợi đã đạt không bị mất.

## 8. Rủi ro còn hiệu lực

- Một hoạt động bị đếm nhiều lần làm sai tiến độ và tạo thêm phần thưởng.
- Ranh giới ngày không nhất quán gây mất nhiệm vụ hoặc tạo cơ hội nhận lặp.
- Sửa định nghĩa đang chạy làm thay đổi quyền lợi đã hứa với người học.
- Nhiệm vụ tối ưu số lượng thao tác nhưng làm giảm độ khó, khả năng nhớ và động lực nội tại.
- Bàn giao thưởng không có định danh ổn định gây cấp trùng hoặc trạng thái “đã nhận” giả.
- Thành tựu hồi tố từ dữ liệu thiếu có thể thiếu công bằng giữa người dùng cũ và mới.

## 9. Điều kiện sẵn sàng triển khai

- Tất cả 25 quyết định đã chốt được truy vết tới task và kịch bản nghiệm thu.
- M03, M04, M05 và M08 thống nhất danh mục sự kiện, ý nghĩa và định danh duy nhất.
- M01 thống nhất múi giờ hiệu lực; M06 thống nhất yêu cầu phần thưởng và trạng thái đối soát.
- M10 thống nhất tín hiệu thông báo; M11 thống nhất quyền, kiểm toán và chỉ số.
- Backlog đã được ưu tiên, ước lượng, gắn phụ thuộc và có tiêu chí nghiệm thu nghiệp vụ.

Không còn quyết định mở trong M07; `QUYET-DINH-MO.md` là nguồn chi tiết.
