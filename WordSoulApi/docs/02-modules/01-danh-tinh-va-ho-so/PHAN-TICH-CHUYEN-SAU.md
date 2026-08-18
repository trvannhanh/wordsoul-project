# Phân tích chuyên sâu M01 — Danh tính và hồ sơ người dùng

## 1. Mục tiêu và phạm vi

### 1.1. Mục tiêu nghiệp vụ

M01 tạo danh tính thống nhất và đáng tin cậy cho mỗi người dùng. Module phải giúp người dùng bắt đầu học với ít ma sát, quay lại tài khoản an toàn, kiểm soát hồ sơ và dữ liệu cá nhân; đồng thời giúp hệ thống xác định đúng ai được thực hiện hành động nào.

Kết quả thành công của M01 không chỉ là “đăng nhập được”, mà còn gồm:

- Một người thật không vô tình tạo nhiều danh tính xung đột.
- Tài khoản bị hạn chế không tiếp tục truy cập bằng phiên cũ.
- Người dùng có thể phục hồi quyền truy cập bằng quy trình có kiểm soát.
- Hồ sơ công khai và dữ liệu riêng tư được tách rõ.
- Mọi thay đổi nhạy cảm đều có thể truy vết.
- Module khác nhận được danh tính, vai trò và trạng thái nhất quán.

### 1.2. Trong phạm vi

- Đăng ký bằng thông tin trực tiếp và khởi tạo hồ sơ nền tảng.
- Đăng nhập trực tiếp và đăng nhập qua nhà cung cấp danh tính bên ngoài.
- Liên kết hoặc xử lý xung đột giữa các hình thức đăng nhập.
- Duy trì, gia hạn, thu hồi và quan sát phiên truy cập.
- Khôi phục quyền truy cập và thay đổi thông tin bảo mật.
- Xem, cập nhật hồ sơ, ảnh đại diện và tùy chọn cá nhân thuộc M01.
- Ghi nhận thiết bị nhận thông báo ở mức danh tính thiết bị.
- Quản lý vai trò và trạng thái hoạt động của tài khoản.
- Xuất, xóa hoặc ẩn danh hóa dữ liệu cá nhân theo chính sách.
- Ghi nhận và quan sát sự kiện danh tính quan trọng.

### 1.3. Ngoài phạm vi

- Tính tiến độ học, chuỗi ngày học, điểm thi đấu hoặc cấp độ.
- Xác định và trao thú cưng, vật phẩm, kinh nghiệm hay điểm thành tựu.
- Xây nội dung và lịch gửi thông báo.
- Tính bảng xếp hạng hoặc quản lý thành viên nhóm.
- Quyết định học liệu, phiên học hoặc lịch ôn.

M01 có thể hiển thị số liệu tổng quan do module khác cung cấp, nhưng không trở thành nguồn sự thật của các số liệu đó.

## 2. Đánh giá ngữ cảnh hiện tại

### 2.1. Năng lực đã quan sát thấy

- Đăng ký bằng tên người dùng, thư điện tử và mật khẩu.
- Đăng nhập trực tiếp và gia hạn quyền truy cập.
- Đăng nhập qua một nhà cung cấp danh tính bên ngoài, gồm trường hợp tạo mới và liên kết theo thư điện tử.
- Khởi tạo một số quyền lợi ban đầu cho người dùng mới.
- Xem hồ sơ cá nhân; quản trị viên có thể tra cứu danh sách và chi tiết tài khoản.
- Cập nhật tên hiển thị và ảnh đại diện.
- Quản lý vai trò và khóa/mở tài khoản; khóa tài khoản có tác động đến phiên gia hạn.
- Ghi nhận thời điểm hoạt động gần nhất và thiết bị nhận thông báo.
- Ghi một phần lịch sử đăng ký, đăng nhập và thao tác quản trị.

### 2.2. Khoảng trống cần xác nhận hoặc bổ sung

- Chưa thấy quy trình xác minh thư điện tử cho đăng ký trực tiếp.
- Chưa thấy quy trình quên, đặt lại hoặc đổi mật khẩu.
- Chưa thấy luồng đăng xuất một thiết bị, đăng xuất mọi thiết bị hoặc danh sách phiên đang hoạt động.
- Chưa thấy lựa chọn nhận tin, quyền riêng tư hồ sơ và quản lý nhiều thiết bị đầy đủ.
- Chưa thấy quy trình người dùng tự yêu cầu xuất, xóa hoặc ẩn danh hóa dữ liệu.
- Việc liên kết tài khoản theo thư điện tử cần thêm bước chứng minh quyền sở hữu để tránh liên kết nhầm.
- Quy tắc đăng nhập của tài khoản bị khóa và hiệu lực của các phiên đang tồn tại cần được xác nhận toàn diện.
- Ranh giới sở hữu số dư và chỉ số gamification đang xuất hiện trong hồ sơ cần thống nhất với M06 và M09.

Đây là quan sát phục vụ phân tích, không phải kết luận rằng hệ thống chắc chắn thiếu ở mọi kênh hoặc môi trường.

## 3. Từ điển nghiệp vụ sơ bộ

> Bản nháp được quản trị theo phiên bản cho M01-T001 nằm tại [Từ điển danh tính M01](TU-DIEN-DANH-TINH.md). Bảng dưới đây là baseline phân tích và không tự thay thế bản được các module tiêu thụ xác nhận.

| Thuật ngữ | Định nghĩa đề xuất |
|---|---|
| Danh tính người dùng | Bản ghi duy nhất đại diện cho một người dùng trong WordSoul |
| Thông tin đăng nhập | Bằng chứng cho phép chứng minh quyền sử dụng danh tính, như mật khẩu hoặc tài khoản bên ngoài |
| Hồ sơ | Thông tin người dùng được phép xem hoặc chỉnh sửa, gồm phần riêng tư và phần có thể công khai |
| Phiên truy cập | Khoảng thời gian một thiết bị được phép hành động dưới danh tính đã xác thực |
| Thiết bị tin cậy | Thiết bị đã đăng nhập và được người dùng nhận biết; không mặc định là thiết bị được miễn mọi kiểm tra bổ sung |
| Tài khoản hoạt động | Tài khoản được phép đăng nhập và sử dụng chức năng theo vai trò |
| Tài khoản bị hạn chế | Tài khoản tạm thời hoặc lâu dài không được phép thực hiện một số hay toàn bộ hành động |
| Xóa tài khoản | Kết thúc quyền sử dụng và xử lý dữ liệu theo chính sách lưu giữ, không mặc định là xóa vật lý ngay mọi bản ghi |
| Ẩn danh hóa | Loại bỏ khả năng liên kết dữ liệu lịch sử với danh tính cá nhân trong khi giữ số liệu cần thiết |

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Đăng ký và khởi tạo tài khoản | Tạo danh tính duy nhất, hợp lệ và sẵn sàng học | Thu thập thông tin, kiểm tra trùng, xác minh, chấp thuận chính sách, khởi tạo hồ sơ | Tự quyết định phần thưởng khởi đầu | M06, M10, M12 |
| SF02 | Đăng nhập trực tiếp | Cho chủ tài khoản truy cập an toàn | Kiểm tra thông tin đăng nhập, trạng thái tài khoản, ghi nhận kết quả | Chính sách giới hạn lưu lượng nền tảng | M11, M12 |
| SF03 | Đăng nhập và liên kết danh tính bên ngoài | Giảm ma sát nhưng không tạo danh tính trùng hoặc chiếm quyền | Đăng nhập ngoài, tạo mới, liên kết, xử lý xung đột | Vận hành nhà cung cấp bên ngoài | M12 |
| SF04 | Vòng đời phiên truy cập | Duy trì và chấm dứt quyền truy cập đúng lúc | Gia hạn, xoay vòng, đăng xuất, thu hồi, nhiều thiết bị | Truyền thông báo đến thiết bị | M10, M11 |
| SF05 | Khôi phục và bảo vệ quyền truy cập | Cho chủ tài khoản lấy lại quyền mà không mở đường cho kẻ giả mạo | Quên/đặt lại/đổi mật khẩu, xác minh hành động nhạy cảm | Hỗ trợ thủ công ngoài quy trình đã duyệt | M10, M11, M12 |
| SF06 | Hồ sơ và tùy chọn cá nhân | Cho người dùng kiểm soát thông tin hiển thị và sở thích thuộc M01 | Xem/sửa hồ sơ, ảnh, múi giờ, giờ học mong muốn, quyền hiển thị | Tính chỉ số học hoặc kinh tế | M09, M10, M12 |
| SF07 | Danh tính thiết bị nhận tin | Duy trì danh sách thiết bị hợp lệ để module thông báo sử dụng | Đăng ký, thay thế, thu hồi thiết bị và gắn với phiên | Chọn nội dung/kênh/thời điểm gửi | M10, M12 |
| SF08 | Vai trò và trạng thái tài khoản | Bảo đảm quyền tối thiểu và xử lý hạn chế có kiểm soát | Gán vai trò, khóa/mở, lý do, hiệu lực và bảo vệ vai trò cao nhất | Định nghĩa thao tác nghiệp vụ của module khác | M11 |
| SF09 | Quyền riêng tư và vòng đời dữ liệu | Cho người dùng thực hiện quyền dữ liệu và giảm rủi ro lưu giữ | Đồng ý, xuất dữ liệu, yêu cầu xóa, ẩn danh hóa, thời hạn lưu | Chính sách pháp lý chưa được phê duyệt | Tất cả module lưu dữ liệu người dùng, M11, M12 |
| SF10 | Kiểm toán và quan sát danh tính | Phát hiện bất thường và giải thích hành động nhạy cảm | Danh mục sự kiện, lịch sử, cảnh báo và chỉ số | Điều tra an ninh chuyên sâu ngoài phạm vi vận hành | M11, M12 |

## 5. Phân tích chi tiết chức năng con

### SF01 — Đăng ký và khởi tạo tài khoản

**Luồng nghiệp vụ chính**

1. Khách chọn hình thức đăng ký trực tiếp.
2. Hệ thống trình bày thông tin bắt buộc, điều khoản và lựa chọn đồng ý cần thiết.
3. Khách cung cấp tên người dùng, thư điện tử và thông tin bảo mật.
4. Hệ thống kiểm tra định dạng, độ phù hợp, tính duy nhất và dấu hiệu lạm dụng.
5. Hệ thống yêu cầu chứng minh quyền sở hữu thư điện tử nếu chính sách bắt buộc.
6. Sau khi xác minh hợp lệ, hệ thống tạo một danh tính và hồ sơ nền tảng duy nhất.
7. Hệ thống yêu cầu module liên quan cấp quyền lợi khởi đầu theo chính sách, nhưng không tự ghi số dư.
8. Hệ thống ghi nhận đăng ký và đưa người dùng vào bước giới thiệu ban đầu.

**Edge case và ngoại lệ**

- Tên hoặc thư điện tử đã tồn tại, khác biệt chỉ ở chữ hoa/thường hoặc khoảng trắng.
- Người dùng gửi lại yêu cầu nhiều lần do mất kết nối.
- Thư xác minh hết hạn, được dùng lại hoặc gửi quá nhiều lần.
- Người chưa đủ độ tuổi tự khai nếu sản phẩm có giới hạn tuổi.
- Tạo tài khoản thành công nhưng cấp quyền lợi khởi đầu thất bại.
- Người dùng đăng ký trực tiếp bằng thư đã gắn với danh tính bên ngoài.
- Tên hiển thị chứa nội dung không phù hợp hoặc giả mạo vai trò.

**Definition of Done**

- Có quy tắc trường bắt buộc, duy nhất, chuẩn hóa và thông báo lỗi dễ hiểu.
- Một yêu cầu gửi lặp không tạo nhiều tài khoản hoặc nhiều quyền lợi khởi đầu.
- Trạng thái trước/sau xác minh được xác định rõ và tài khoản chưa xác minh bị giới hạn đúng chính sách.
- Đồng ý điều khoản được ghi nhận theo phiên bản và thời điểm.
- Đăng ký, xác minh, thất bại và dấu hiệu lạm dụng đều có thể truy vết mà không ghi lộ thông tin bí mật.

### SF02 — Đăng nhập trực tiếp

**Luồng nghiệp vụ chính**

1. Người dùng cung cấp thông tin đăng nhập.
2. Hệ thống kiểm tra thông tin theo cách không tiết lộ tài khoản có tồn tại hay không.
3. Hệ thống kiểm tra trạng thái hoạt động và các hạn chế hiện hành.
4. Nếu hợp lệ, hệ thống tạo quyền truy cập, ghi nhận thiết bị và thời điểm hoạt động.
5. Người dùng được chuyển đến ngữ cảnh học phù hợp.

**Edge case và ngoại lệ**

- Sai thông tin liên tiếp hoặc thử nhiều tài khoản từ cùng nguồn.
- Tài khoản chỉ có hình thức đăng nhập bên ngoài nhưng người dùng thử mật khẩu.
- Tài khoản bị khóa sau khi thông tin đăng nhập đã được xác nhận.
- Hai lần đăng nhập đồng thời trên nhiều thiết bị.
- Đồng hồ hệ thống hoặc múi giờ thiết bị sai.
- Tài khoản đang chờ xóa hoặc chưa hoàn thành xác minh.

**Definition of Done**

- Thông báo thất bại không làm lộ sự tồn tại của tài khoản.
- Tài khoản không hoạt động không nhận được phiên mới.
- Có chính sách xử lý thử sai liên tiếp và phục hồi sau giới hạn.
- Đăng nhập thành công/thất bại quan trọng được ghi nhận với dữ liệu tối thiểu.
- Kịch bản nhiều thiết bị và trạng thái tài khoản thay đổi được kiểm thử nghiệp vụ.

### SF03 — Đăng nhập và liên kết danh tính bên ngoài

**Luồng nghiệp vụ chính**

1. Người dùng chọn nhà cung cấp danh tính.
2. Nhà cung cấp yêu cầu người dùng đồng ý chia sẻ dữ liệu cần thiết.
3. Hệ thống chỉ chấp nhận kết quả còn hiệu lực và thư điện tử đã được nhà cung cấp xác minh.
4. Nếu danh tính ngoài đã liên kết, hệ thống kiểm tra trạng thái tài khoản và cho đăng nhập.
5. Nếu chưa liên kết nhưng thư trùng tài khoản hiện có, hệ thống yêu cầu người dùng chứng minh quyền sở hữu tài khoản hiện có trước khi liên kết.
6. Nếu không có xung đột, hệ thống tạo tài khoản mới và hoàn tất khởi tạo.
7. Hệ thống ghi nhận nguồn đăng nhập và thay đổi liên kết.

**Edge case và ngoại lệ**

- Người dùng từ chối đồng ý hoặc nhà cung cấp trả thiếu dữ liệu.
- Thư điện tử chưa xác minh, thay đổi hoặc không còn được cung cấp.
- Một danh tính ngoài đã gắn với tài khoản khác.
- Tài khoản hiện có bị khóa hoặc chờ xóa.
- Liên kết tự động theo thư dẫn đến chiếm tài khoản.
- Người dùng mất quyền truy cập nhà cung cấp duy nhất.
- Nhà cung cấp gián đoạn giữa bước đồng ý và hoàn tất.

**Definition of Done**

- Không liên kết chỉ dựa trên thư trùng nếu chưa có bằng chứng sở hữu phù hợp.
- Một danh tính ngoài chỉ thuộc một tài khoản WordSoul.
- Có luồng thất bại và thử lại không tạo tài khoản trùng.
- Người dùng có ít nhất một phương thức phục hồi trước khi gỡ hình thức đăng nhập duy nhất.
- Việc liên kết/gỡ liên kết được thông báo và truy vết.

### SF04 — Vòng đời phiên truy cập

**Luồng nghiệp vụ chính**

1. Sau xác thực, hệ thống tạo phiên có thời hạn và gắn với ngữ cảnh thiết bị.
2. Khi quyền truy cập ngắn hạn hết hiệu lực, thiết bị gửi bằng chứng gia hạn còn hợp lệ.
3. Hệ thống kiểm tra phiên, trạng thái tài khoản và dấu hiệu tái sử dụng bất thường.
4. Hệ thống cấp quyền mới và làm mất hiệu lực bằng chứng gia hạn cũ theo chính sách.
5. Người dùng có thể đăng xuất thiết bị hiện tại hoặc tất cả thiết bị.
6. Thay đổi bảo mật, khóa tài khoản hoặc xóa tài khoản thu hồi các phiên liên quan.

**Edge case và ngoại lệ**

- Hai yêu cầu gia hạn đồng thời dùng cùng bằng chứng.
- Bằng chứng cũ bị dùng lại sau khi đã xoay vòng.
- Người dùng xóa ứng dụng mà không đăng xuất.
- Quản trị viên khóa tài khoản khi người dùng đang hoạt động.
- Người dùng đổi mật khẩu trên một thiết bị trong khi thiết bị khác còn phiên.
- Không thể xác định tên thiết bị hoặc vị trí gần đúng.

**Definition of Done**

- Chính sách thời hạn, gia hạn, xoay vòng và thu hồi được ghi thành quy tắc nghiệp vụ.
- Bằng chứng hết hạn, đã thu hồi hoặc bị dùng lại không tạo quyền mới.
- Đăng xuất hiện tại và mọi thiết bị có kết quả dễ hiểu và có thể truy vết.
- Khóa/xóa tài khoản làm mất hiệu lực quyền truy cập theo thời gian mục tiêu đã xác định.
- Người dùng xem được thông tin phiên ở mức hữu ích nhưng không lộ dữ liệu nhạy cảm.

### SF05 — Khôi phục và bảo vệ quyền truy cập

**Luồng nghiệp vụ chính**

1. Người dùng yêu cầu khôi phục bằng thông tin liên hệ đã xác minh.
2. Hệ thống phản hồi trung tính, không xác nhận tài khoản có tồn tại.
3. Hệ thống gửi bằng chứng khôi phục có thời hạn, dùng một lần và giới hạn tần suất.
4. Người dùng chứng minh quyền sở hữu và đặt thông tin bảo mật mới đáp ứng chính sách.
5. Hệ thống thu hồi bằng chứng khôi phục và các phiên theo chính sách.
6. Hệ thống thông báo thay đổi qua kênh độc lập phù hợp và ghi lịch sử.

**Edge case và ngoại lệ**

- Yêu cầu hàng loạt cho cùng tài khoản hoặc cùng nguồn.
- Bằng chứng hết hạn, dùng lại hoặc bị thay thế bởi yêu cầu mới.
- Tài khoản chỉ dùng danh tính bên ngoài.
- Người dùng không còn quyền truy cập thư điện tử.
- Tài khoản bị khóa, chờ xóa hoặc đang bị điều tra.
- Thay đổi mật khẩu nhưng phiên đáng ngờ vẫn hoạt động.

**Definition of Done**

- Luồng khôi phục không làm lộ sự tồn tại tài khoản.
- Bằng chứng có thời hạn, dùng một lần và bị vô hiệu hóa đúng quy tắc.
- Có giới hạn chống lạm dụng và đường hỗ trợ cho trường hợp mất kênh khôi phục.
- Thay đổi bảo mật nhạy cảm tạo cảnh báo và lịch sử kiểm toán.
- Quy tắc thu hồi phiên sau khôi phục/đổi mật khẩu được xác nhận và kiểm thử.

### SF06 — Hồ sơ và tùy chọn cá nhân

**Luồng nghiệp vụ chính**

1. Người dùng xem hồ sơ của chính mình với dữ liệu cá nhân và số liệu tổng quan được phép.
2. Người dùng chọn chỉnh sửa trường được phép như tên hiển thị, ảnh, múi giờ hoặc giờ học mong muốn.
3. Hệ thống kiểm tra quyền, định dạng, nội dung và tính duy nhất nếu áp dụng.
4. Tài sản cũ được thay thế hoặc lưu giữ theo chính sách.
5. Hệ thống lưu thay đổi, trả hồ sơ mới và thông báo module liên quan khi cần.

**Edge case và ngoại lệ**

- Người dùng cố sửa hồ sơ người khác hoặc trường chỉ quản trị được sửa.
- Tên mới trùng, không phù hợp hoặc thay đổi quá thường xuyên.
- Ảnh sai loại, quá lớn, vi phạm nội dung hoặc tải lên dang dở.
- Múi giờ thay đổi làm ảnh hưởng giờ nhắc học.
- Số liệu từ module khác tạm thời không sẵn sàng.
- Hai thiết bị cùng sửa hồ sơ.

**Definition of Done**

- Có danh sách rõ trường riêng tư, công khai, chỉ đọc và ai được sửa.
- Thay đổi không hợp lệ bị từ chối với lý do có thể hành động.
- Ảnh cũ và ảnh lỗi được xử lý theo chính sách vòng đời tài sản.
- Cập nhật đồng thời không âm thầm ghi đè thay đổi quan trọng.
- M01 không tự tính hoặc điều chỉnh số liệu thuộc module khác.

### SF07 — Danh tính thiết bị nhận tin

**Luồng nghiệp vụ chính**

1. Sau khi người dùng đồng ý nhận thông báo, ứng dụng cung cấp danh tính thiết bị hợp lệ.
2. Hệ thống gắn thiết bị với tài khoản và phiên tương ứng.
3. Khi danh tính thiết bị thay đổi, hệ thống cập nhật mà không tạo bản ghi trùng.
4. Khi đăng xuất, thu hồi quyền hoặc nhận lỗi thiết bị vĩnh viễn, hệ thống ngừng dùng thiết bị đó.
5. M10 chỉ nhận danh sách thiết bị còn hợp lệ và được phép nhận tin.

**Edge case và ngoại lệ**

- Một thiết bị lần lượt được nhiều tài khoản sử dụng.
- Một tài khoản có nhiều thiết bị.
- Danh tính thiết bị bị thay đổi, trùng hoặc hết hiệu lực.
- Người dùng tắt quyền thông báo ở hệ điều hành nhưng hệ thống chưa biết.
- Đăng xuất không thành công sau khi thiết bị đã được gắn.

**Definition of Done**

- Hỗ trợ nhiều thiết bị và không gửi cho tài khoản cũ trên thiết bị dùng chung.
- Có trạng thái hợp lệ, thu hồi và thời điểm hoạt động gần nhất.
- Việc gắn/gỡ thiết bị tuân theo phiên và lựa chọn nhận tin.
- M10 nhận dữ liệu thiết bị tối thiểu, không nhận thông tin bảo mật phiên.

### SF08 — Vai trò và trạng thái tài khoản

**Luồng nghiệp vụ chính**

1. Quản trị viên có quyền chọn tài khoản cần thay đổi.
2. Hệ thống hiển thị trạng thái, vai trò hiện tại và tác động dự kiến.
3. Quản trị viên chọn thay đổi, cung cấp lý do và xác nhận nếu là thao tác nhạy cảm.
4. Hệ thống kiểm tra người thực hiện có quyền cao hơn hoặc phù hợp.
5. Hệ thống áp dụng thay đổi, thu hồi phiên nếu cần và ghi trước/sau.
6. Người dùng bị ảnh hưởng nhận thông báo phù hợp và có đường khiếu nại nếu chính sách yêu cầu.

**Edge case và ngoại lệ**

- Quản trị viên tự hạ vai trò khiến hệ thống mất người quản trị cao nhất.
- Một quản trị viên sửa người có quyền bằng hoặc cao hơn.
- Hai quản trị viên thay đổi cùng lúc.
- Khóa tài khoản đã bị khóa hoặc mở tài khoản đang chờ xóa.
- Phiên đang hoạt động tiếp tục mang quyền cũ.
- Không có lý do hoặc bằng chứng cho điều chỉnh.

**Definition of Done**

- Có ma trận vai trò–quyền được phê duyệt và nguyên tắc quyền tối thiểu.
- Không thể vô tình loại bỏ người quản trị cao nhất cuối cùng.
- Thay đổi vai trò/trạng thái có hiệu lực với phiên hiện có theo thời gian mục tiêu.
- Mọi thay đổi ghi người thực hiện, lý do, giá trị trước/sau và thời điểm.
- Quy trình khôi phục thao tác sai được xác định.

### SF09 — Quyền riêng tư và vòng đời dữ liệu

**Luồng nghiệp vụ chính**

1. Người dùng xem các đồng ý, lựa chọn riêng tư và loại dữ liệu hệ thống lưu.
2. Người dùng yêu cầu xuất hoặc xóa tài khoản.
3. Hệ thống xác minh lại danh tính và trình bày tác động, thời gian xử lý, dữ liệu phải giữ.
4. Yêu cầu bước vào thời gian chờ nếu chính sách cho phép hủy.
5. Hệ thống thu hồi quyền truy cập và điều phối các module xuất, xóa hoặc ẩn danh hóa dữ liệu.
6. Người dùng nhận xác nhận hoàn thành; hệ thống giữ bằng chứng xử lý tối thiểu.

**Edge case và ngoại lệ**

- Tài khoản đang bị điều tra, có tranh chấp hoặc nghĩa vụ lưu dữ liệu.
- Người dùng đổi ý trong thời gian chờ.
- Một module phụ thuộc không thể xóa dữ liệu ngay.
- Dữ liệu lịch sử cần giữ cho tính toàn vẹn bảng xếp hạng hoặc trận đấu.
- Yêu cầu xuất quá lớn hoặc chứa dữ liệu của người khác.
- Tài khoản đã xóa đăng ký lại bằng cùng thư điện tử.

**Definition of Done**

- Có bản đồ dữ liệu cá nhân, chủ sở hữu, mục đích và thời hạn lưu theo module.
- Yêu cầu xuất chỉ chứa dữ liệu người yêu cầu được phép nhận.
- Xóa/ẩn danh hóa có trạng thái, thời hạn, bằng chứng và xử lý thất bại từng phần.
- Phiên truy cập bị thu hồi khi yêu cầu xóa có hiệu lực.
- Quy tắc đăng ký lại và dữ liệu cần giữ được xác nhận.

### SF10 — Kiểm toán và quan sát danh tính

**Luồng nghiệp vụ chính**

1. Các hành động danh tính quan trọng tạo sự kiện nghiệp vụ chuẩn hóa.
2. Hệ thống ghi dữ liệu tối thiểu để trả lời ai, làm gì, khi nào, kết quả và lý do.
3. Quy tắc phát hiện bất thường tổng hợp thất bại đăng nhập, khôi phục, liên kết hoặc thay đổi quyền.
4. Sự kiện vượt ngưỡng tạo cảnh báo cho người dùng hoặc vận hành theo mức độ.
5. Người có quyền tra cứu lịch sử mà không thấy thông tin bí mật.

**Edge case và ngoại lệ**

- Nhật ký vô tình chứa mật khẩu hoặc bằng chứng phiên.
- Cảnh báo quá nhiều tạo nhiễu hoặc quá ít bỏ sót tấn công.
- Đồng hồ, vị trí hoặc nhận diện thiết bị không chính xác.
- Người quản trị cố sửa/xóa dấu vết của chính mình.
- Dữ liệu kiểm toán hết thời hạn lưu trong khi còn điều tra.

**Definition of Done**

- Có danh mục sự kiện, mức độ, dữ liệu tối thiểu và thời hạn lưu được phê duyệt.
- Không ghi thông tin đăng nhập bí mật hoặc dữ liệu cá nhân vượt mục đích.
- Hành động nhạy cảm và thất bại bất thường có thể tra cứu và cảnh báo.
- Chỉ vai trò được phép mới xem được dữ liệu kiểm toán; lượt xem cũng được ghi nhận.
- Có chỉ số tối thiểu về đăng ký, đăng nhập, thất bại, khôi phục, thu hồi và khóa tài khoản.

## 6. Ma trận truy vết mục tiêu → chức năng → nhóm task

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Tạo danh tính duy nhất, ít ma sát | SF01, SF03 | M01-T001–M01-T009 |
| Cho chủ tài khoản truy cập và phục hồi an toàn | SF02, SF04, SF05 | M01-T010–M01-T021 |
| Cho người dùng kiểm soát hồ sơ và thiết bị | SF06, SF07 | M01-T022–M01-T027 |
| Bảo đảm quyền đúng và trạng thái có hiệu lực | SF08 | M01-T028–M01-T032 |
| Bảo vệ quyền riêng tư và vòng đời dữ liệu | SF09 | M01-T033–M01-T037 |
| Truy vết và phát hiện bất thường | SF10 | M01-T038–M01-T041 |
| Xác nhận chất lượng toàn module | SF01–SF10 | M01-T042–M01-T043 |

## 7. Thứ tự thực hiện đề xuất

1. **Nền tảng quyết định:** M01-T001–M01-T004 và M01-T028 xác định vòng đời, dữ liệu, vai trò và chính sách bảo mật.
2. **Danh tính ban đầu:** M01-T005–M01-T012 hoàn thiện đăng ký, đăng nhập và trạng thái không hoạt động.
3. **Danh tính bên ngoài và phiên:** M01-T013–M01-T018 bảo vệ liên kết và vòng đời phiên.
4. **Khôi phục:** M01-T019–M01-T021 hoàn thiện đường lấy lại quyền truy cập.
5. **Hồ sơ và thiết bị:** M01-T022–M01-T027 chuẩn hóa dữ liệu người dùng và quan hệ với M09/M10.
6. **Quản trị tài khoản:** M01-T029–M01-T032 áp dụng quyền, khóa/mở và bảo vệ vai trò cao nhất.
7. **Quyền dữ liệu và kiểm toán:** M01-T033–M01-T041 hoàn thiện quyền riêng tư, vòng đời và quan sát.
8. **Xác nhận toàn module:** M01-T042–M01-T043 kiểm thử liên chức năng và bàn giao tài liệu.

Các task về hồ sơ, thiết bị và danh mục sự kiện có thể làm song song sau khi M01-T001–M01-T004 hoàn thành. Task tích hợp gửi thư, ảnh và danh tính bên ngoài chỉ chốt đầu ra nghiệp vụ tại M01; phần năng lực ngoài được giao cho M12.

## 8. Cơ sở quyết định đã chốt

Tất cả 25 quyết định của M01 đã được chốt. `QUYET-DINH-MO.md` là nguồn chi tiết; các nguyên tắc dưới đây là đầu vào bắt buộc cho luồng nghiệp vụ, nghiệm thu và bàn giao task:

- Sản phẩm phục vụ mọi nhóm tuổi, chỉ thu nhóm tuổi và quốc gia. Hiện chưa xác minh, liên kết hoặc quản lý quan hệ người giám hộ; không tự hạn chế quyền truy cập khi thiếu xác nhận đồng ý.
- Người chưa xác minh thư điện tử chỉ được xem học liệu và học giới hạn; không được xuất bản, dùng tính năng xã hội, PvP, nhận tin ngoài ứng dụng hoặc thực hiện thao tác tài khoản nhạy cảm.
- Thư điện tử là thông tin đăng nhập trực tiếp. Tên hiển thị có thể trùng, được đổi tối đa một lần trong 30 ngày và phải qua kiểm tra nội dung.
- Thông tin bảo mật tối thiểu 12 ký tự, chấp nhận cụm từ dài, chặn giá trị phổ biến hoặc đã lộ và không bắt đổi định kỳ nếu không có rủi ro.
- Không tự liên kết danh tính bên ngoài chỉ vì trùng thư. Chủ tài khoản phải chứng minh quyền sở hữu tài khoản hiện có; không được gỡ phương thức truy cập cuối cùng.
- Cho phép nhiều thiết bị trong giới hạn hợp lý. Phiên người học có thể kéo dài tối đa 30 ngày khi còn hoạt động; thao tác nhạy cảm phải xác minh lại gần thời điểm thực hiện.
- Đặt lại thông tin bảo mật hoặc nghi ngờ chiếm quyền thu hồi toàn bộ phiên; thay đổi chủ động trong phiên an toàn giữ phiên hiện tại và thu hồi các phiên còn lại.
- Hồ sơ riêng tư mặc định. M01 chỉ hiển thị tài sản và thứ hạng từ module sở hữu, không tự điều chỉnh các giá trị này.
- Yêu cầu xóa tài khoản chỉ được tiếp nhận qua hỗ trợ, có xác minh lại và thời gian chờ được công bố. Thông tin nhận dạng trực tiếp bị xóa; lịch sử cần cho tính toàn vẹn chỉ được giữ dưới dạng không thể liên kết lại.
- Xuất dữ liệu là luồng tự phục vụ có xác minh lại; nhật ký bảo mật và kiểm toán giữ mặc định 12 tháng, trừ phạm vi điều tra hoặc nghĩa vụ lưu giữ riêng.

## 9. Rủi ro còn hiệu lực và điều kiện phát hành

- Chính sách phục vụ mọi độ tuổi nhưng không xác minh người giám hộ là rủi ro tuân thủ mức rất cao; phải được rà soát theo từng thị trường trước khi phát hành.
- M05 yêu cầu xác nhận của người giám hộ trước khi gửi giọng nói người chưa thành niên cho bên ngoài, nhưng M01 chưa có bằng chứng hay quan hệ người giám hộ. Chấm phát âm bên ngoài cho nhóm tuổi này phải bị chặn cho đến khi có cơ chế bằng chứng phù hợp hoặc một quyết định thay thế được phê duyệt.
- Tự động liên kết tài khoản theo thư điện tử bị cấm; mọi luồng hiện hữu làm như vậy phải được xem là khoảng cách cần xử lý vì có nguy cơ chiếm quyền.
- Xóa vật lý toàn bộ lịch sử có thể phá vỡ tính toàn vẹn của học tập, tài sản, nhóm và trận; ma trận xóa và ẩn danh hóa liên module phải hoàn tất trước khi vận hành yêu cầu xóa.
- Lựa chọn giờ học và thiết bị thuộc M01, nhưng quyền quyết định gửi thuộc M10; hai module phải dùng cùng nhóm thông báo, tùy chọn nhận và trạng thái thiết bị.
- Quyền quản trị được tách theo trách nhiệm và không có cơ chế phê duyệt hai người; quyền tối thiểu, xác minh lại và nhật ký bất biến là kiểm soát bắt buộc.

Không còn quyết định mở trong M01. Mọi thay đổi so với cơ sở trên phải tạo quyết định mới, nêu module bị ảnh hưởng và cập nhật task liên quan.
