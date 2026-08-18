# Gói task thực thi Lát 0–Lát 1 Giai đoạn A

## 1. Mục đích

Tài liệu chuyển thứ tự triển khai Giai đoạn A thành danh sách công việc có thể nhập vào Jira, Trello hoặc Notion. Phạm vi gồm:

- **Lát 0:** mở năm hồ sơ REL ảnh hưởng Giai đoạn A và ghi nhận bảy kiểm soát tạm thời.
- **Lát 1:** khởi tạo 30 task nguồn về từ điển, mô hình nền và các sổ đăng ký.

Các chủ trì dưới đây là **vai trò đề xuất**, chưa phải tên người được giao thực tế. Khi nhập công cụ quản lý công việc phải thay bằng cá nhân chịu trách nhiệm và người xác nhận.

## 2. Quy ước import

| Trường | Cách sử dụng |
|---|---|
| Mã task | Giữ nguyên mã nguồn đối với task module; task điều phối mới dùng tiền tố A0 |
| Lát | 0 hoặc 1 |
| Loại | Hồ sơ REL, kiểm soát tạm thời hoặc task nguồn |
| Chủ trì đề xuất | Vai trò chịu trách nhiệm tạo đầu ra; không đồng nghĩa người duyệt cuối |
| Người xác nhận | Vai trò xác nhận bằng chứng và cho phép đóng task |
| Trạng thái khởi tạo | Sẵn sàng; chờ task phụ thuộc; hoặc chờ quyết định/bằng chứng ngoài hệ thống |
| Cổng/REL | Liên kết task với điều kiện phát hành |
| Bằng chứng | Tài liệu, ma trận, biên bản, mẫu dữ liệu hoặc kết quả rà soát bắt buộc để đóng task |

Độ phức tạp S/M/L giữ theo backlog nguồn. Với task A0, độ phức tạp phản ánh công việc điều phối và tài liệu, không phải toàn bộ công việc đóng REL.

## 3. Lát 0 — Hồ sơ phát hành và kiểm soát tạm thời

### 3.1. Hồ sơ REL

| Mã task | Tên task | Mô tả ngắn | Input | Output mong đợi | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Độ phức tạp | Phụ thuộc | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|---|
| A0-T001 | Mở hồ sơ REL-01 tuổi và đồng ý | Ghi phạm vi thị trường/độ tuổi, câu hỏi pháp lý, lựa chọn sản phẩm và bằng chứng cần nộp | Quyết định M01; đánh giá A-WP01; chính sách dự kiến | Hồ sơ có chủ, phạm vi, hạn phản hồi, ma trận câu hỏi và tiêu chí đóng | Chủ sản phẩm | Pháp lý và chủ M01 | P0 | M | Không | A-G01; REL-01 | Sẵn sàng mở; đóng hồ sơ chờ ý kiến pháp lý |
| A0-T002 | Mở hồ sơ REL-02 quyền và audit | Chốt các kiểm soát bù trừ bắt buộc khi không duyệt hai người và không quyền khẩn cấp | Quyết định M11; A-WP01/A-WP03 | Danh sách bằng chứng quyền, xác minh lại, từ chối, audit và diễn tập | Chủ M11/an toàn hệ thống | Chủ sản phẩm và vận hành | P0 | M | Không | A-G02, A-G06; REL-02 | Sẵn sàng |
| A0-T003 | Mở hồ sơ REL-03 bí mật và tích hợp | Xác định phạm vi kiểm kê, bí mật nghi lộ, tích hợp hoạt động và bài kiểm thử suy giảm | A-WP04; danh sách tích hợp hiện có | Hồ sơ có chủ từng tích hợp/bí mật, mức khẩn và tiêu chí thu hồi/xoay vòng | Chủ M12 | An toàn hệ thống và vận hành | P0 | M | Không | A-G04–A-G06; REL-03 | Sẵn sàng; không ghi giá trị bí mật |
| A0-T004 | Mở hồ sơ REL-04 quyền tài sản | Ghi loại tài sản A/B, mức bằng chứng quyền hiện có, rủi ro được chấp nhận và đường gỡ nhanh | A-WP02/A-WP04; quyết định bản quyền | Hồ sơ tài sản trong phạm vi, đầu mối khiếu nại và tiêu chí tạm ẩn/gỡ | Chủ sản phẩm/nội dung | Pháp lý | P0 | M | Không | A-G03, A-G05; REL-04 | Sẵn sàng mở; đóng hồ sơ chờ xác nhận pháp lý/sản phẩm |
| A0-T005 | Mở hồ sơ REL-07 xuất và xóa dữ liệu | Chốt hành trình tự phục vụ/hỗ trợ, xác minh chủ thể, thời hạn và module phải đối soát | A-WP01/A-WP03; bản đồ dữ liệu hiện có | Danh sách module, chủ dữ liệu, SLA, manifest và kịch bản phần lỗi | Chủ M01 | Chủ M11, riêng tư và các chủ module dữ liệu | P0 | L | A0-T001 | A-G01, A-G02; REL-07 | Chờ phạm vi dữ liệu/đồng ý sơ bộ từ A0-T001 |

### 3.2. Kiểm soát tạm thời

| Mã task | Tên task | Mô tả ngắn | Input | Output mong đợi | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Độ phức tạp | Phụ thuộc | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|---|
| A0-T006 | Ghi nhận CT-01 đóng công khai nội dung chưa duyệt | Xác nhận phạm vi nội dung người dùng/AI không được công khai và người có quyền thay đổi trạng thái | A-WP02; M02-T017, T029–T034 | Biên bản phạm vi bị khóa, ngoại lệ bằng không và điều kiện mở lại | Chủ M02 | Chủ sản phẩm và M11 | P0 | S | A0-T004 | A-G03; REL-04 | Chờ hồ sơ REL-04 được mở |
| A0-T007 | Ghi nhận CT-02 không tự ghép tài khoản theo email | Xác nhận email trùng không phải bằng chứng đủ và mọi ngoại lệ phải bị từ chối | A-WP01/A-WP04 | Chính sách tạm thời, phạm vi luồng và tiêu chí gỡ bỏ | Chủ M01 | An toàn hệ thống | P0 | S | Không | A-G01 | Sẵn sàng |
| A0-T008 | Ghi nhận CT-03 không dùng payload thô làm bằng chứng hợp lệ | Phân loại log hiện tại là nguồn có rủi ro và hạn chế người/phạm vi sử dụng trong thời gian xử lý | A-WP03/A-WP04 | Danh sách nguồn log rủi ro, chủ xử lý, giới hạn truy cập và kế hoạch thay thế | Chủ M11 | An toàn hệ thống và riêng tư | P0 | M | A0-T003 | A-G02, A-G05; REL-02, REL-03 | Chờ hồ sơ REL-03 được mở |
| A0-T009 | Ghi nhận CT-04 không dùng health giả định để phát hành | Xác định điểm health hiện tại không đủ làm bằng chứng và thống nhất kiểm tra thủ công tạm thời | A-WP03/A-WP04 | Danh sách năng lực cần xác minh, người xác nhận và điều kiện gỡ bỏ | Chủ vận hành | Chủ M11/M12 | P0 | S | Không | A-G04, A-G06; REL-03 | Sẵn sàng |
| A0-T010 | Ghi nhận CT-05 cấm bỏ qua giới hạn lưu lượng | Chốt không có trusted/internal caller được miễn giới hạn chỉ bằng dấu hiệu yêu cầu | A-WP04; M12-D023 | Biên bản phạm vi cấm, danh sách năng lực bị ảnh hưởng và tiêu chí kiểm chứng | Chủ M12 | An toàn hệ thống | P0 | S | A0-T003 | A-G04; REL-03 | Chờ hồ sơ REL-03 được mở |
| A0-T011 | Ghi nhận CT-06 giữ AI/giọng nói tắt trong A/B | Ghi rõ năng lực nào không hoạt động và dữ liệu nào tuyệt đối không gửi ra ngoài | Ngoại lệ kế hoạch A/B; REL-01/REL-03 | Danh sách năng lực tắt, chủ kiểm tra và điều kiện mở ở giai đoạn sau | Chủ sản phẩm/M12 | Pháp lý, riêng tư và vận hành | P0 | S | A0-T001, A0-T003 | A-G04, A-G05; REL-01, REL-03 | Chờ hai hồ sơ được mở |
| A0-T012 | Ghi nhận CT-07 không mở rộng AP | Đóng băng phụ thuộc mới vào AP và ghi rõ các điểm hiện hữu phải chuyển sang REL-05 | Quyết định loại AP; A-WP03 | Danh sách điểm dùng AP hiện có, chủ sở hữu và nguyên tắc không mở rộng | Chủ M06 | Chủ sản phẩm và M11 | P1 | M | Không | REL-05; chuẩn bị B-G03 | Sẵn sàng |

### 3.3. Definition of Done chung cho Lát 0

Một task A0 chỉ hoàn thành khi:

- Có một cá nhân chủ trì và một cá nhân xác nhận được ghi rõ.
- Phạm vi áp dụng và không áp dụng không mơ hồ.
- Có liên kết đến quyết định, task nguồn, cổng và REL liên quan.
- Có danh sách bằng chứng cần nộp, tiêu chí đạt/không đạt và điều kiện hết hiệu lực.
- Kiểm soát tạm thời có cách kiểm tra định kỳ; không chỉ là tuyên bố trong tài liệu.
- Không đưa giá trị bí mật hoặc dữ liệu cá nhân thật vào hồ sơ.

## 4. Lát 1 — Từ điển và sổ đăng ký nền

### 4.1. M01 — Danh tính và hồ sơ

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T001 | Chuẩn hóa từ điển danh tính | Quyết định M01; thuật ngữ toàn hệ thống | Từ điển được duyệt, chủ thuật ngữ và danh sách mâu thuẫn đã giải quyết | Chủ M01 | Chủ M02/M11/M12 | P0 | S | Không | A-G01 | Sẵn sàng |
| M01-T002 | Xác định vòng đời tài khoản | M01-T001; REL-01 | Sơ đồ trạng thái, điều kiện chuyển, quyền theo trạng thái và tác động phiên | Chủ M01 | An toàn hệ thống, sản phẩm | P0 | M | M01-T001; A0-T001 | A-G01; REL-01 | Chờ M01-T001; có thể soạn song song với rà soát pháp lý |
| M01-T003 | Lập bản đồ dữ liệu hồ sơ | M01-T001; hiện trạng dữ liệu | Danh mục trường, mục đích, nguồn sự thật, độ nhạy, lưu giữ và module tiêu thụ | Chủ M01/riêng tư | Các chủ module dữ liệu | P0 | M | M01-T001 | A-G01, A-G05; REL-01, REL-07 | Chờ M01-T001 |
| M01-T004 | Chốt chính sách thông tin bảo mật | M01-T001; rủi ro xác thực/khôi phục | Danh sách trường bảo mật, quyền xem/sửa, che dữ liệu và xác minh lại | Chủ an toàn danh tính | Chủ M01/M11 | P0 | M | M01-T001 | A-G01, A-G02; REL-02 | Chờ M01-T001 |
| M01-T005 | Chuẩn hóa dữ liệu đăng ký | M01-T003, M01-T004; REL-01 | Hợp đồng dữ liệu đăng ký, kiểm tra xung đột, lỗi và dữ liệu tối thiểu | Chủ M01 | Sản phẩm, riêng tư | P0 | M | M01-T003, M01-T004 | A-G01; REL-01 | Chờ phụ thuộc nội bộ |
| M01-T006 | Thiết kế xác minh thư điện tử | M01-T002, M01-T005; năng lực thư hiện có | Luồng phát hành/hết hạn/gửi lại/chống lạm dụng và bằng chứng không lộ tài khoản | Chủ M01 | An toàn hệ thống, M12 | P0 | M | M01-T002, M01-T005; hợp đồng thư tối thiểu | A-G01 | Chờ phụ thuộc; M12-T026 đầy đủ thuộc B nên chỉ dùng hợp đồng tối thiểu A |
| M01-T007 | Ghi nhận đồng ý và phiên bản chính sách | M01-T002, M01-T003; REL-01 | Bản ghi đồng ý/từ chối có phiên bản, thời điểm, phạm vi và nguồn | Chủ riêng tư/M01 | Pháp lý, sản phẩm | P0 | S | M01-T002, M01-T003; A0-T001 | A-G01; REL-01, REL-06 | Chờ phụ thuộc và đầu vào pháp lý |
| M01-T008 | Điều phối khởi tạo người dùng mới | M01-T005–T007; hợp đồng cấp giá trị M06 | Luồng khởi tạo chống lặp, trạng thái phần lỗi, retry/đối soát và không tạo AP mới | Chủ M01 | Chủ M06/M11 | P0 | L | M01-T005–T007; hợp đồng tối thiểu M06 | A-G01; chuẩn bị B-G03 | Bị chặn một phần bởi hợp đồng M06; không đóng toàn bộ trong Lát 1 |
| M01-T009 | Xây dựng tiêu chí nghiệm thu đăng ký | M01-T005–T008; REL-01 | Bộ kịch bản tuổi/đồng ý/xác minh/trùng/gửi lặp/phần lỗi và kết quả mong đợi | Chủ kiểm thử M01 | Sản phẩm, an toàn hệ thống | P0 | M | M01-T005–T008 | A-G01; REL-01 | Chờ các task trước |

### 4.2. M02 — Nội dung từ vựng

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M02-T001 | Chuẩn hóa từ điển học liệu | M01-T001; quyết định M02 | Từ điển mục từ–nghĩa–biến thể–bộ–phiên bản–xuất bản được duyệt | Chủ M02 | Chuyên gia học thuật, M03/M04/M05 | P0 | S | M01-T001 | A-G03 | Chờ M01-T001 |
| M02-T002 | Thiết kế mô hình nhiều nghĩa và loại từ | M02-T001; mẫu từ đa nghĩa | Mô hình khái niệm có định danh nghĩa ổn định và quy tắc module tiêu thụ chọn nghĩa | Chủ M02 | Chuyên gia học thuật, M03/M04 | P0 | L | M02-T001 | A-G03 | Chờ M02-T001 |
| M02-T003 | Chuẩn hóa mặt chữ và biến thể | M02-T001, M02-T002; mẫu dữ liệu | Bộ quy tắc có ví dụ đạt/không đạt, không tự hợp nhất trường hợp mơ hồ | Chủ nội dung M02 | Chuyên gia học thuật | P0 | M | M02-T001, M02-T002 | A-G03 | Chờ M02-T002 |
| M02-T004 | Thiết kế phát hiện nội dung trùng | M02-T002, M02-T003; dữ liệu hiện tại | Ma trận trùng chắc chắn/gần giống/khác nghĩa và quyết định tái sử dụng–thêm nghĩa–tạo mới | Chủ M02 | Chuyên gia học thuật, kiểm duyệt | P0 | L | M02-T002, M02-T003 | A-G03 | Chờ phụ thuộc nội bộ |
| M02-T005 | Chuẩn hóa cấp độ và độ khó | M02-T002; khung CEFR | Tiêu chí cấp độ theo nghĩa và quy tắc suy ra/xác nhận độ khó bộ | Chuyên gia học thuật | Chủ M02/sản phẩm học tập | P0 | M | M02-T002 | A-G03 | Chờ M02-T002 |
| M02-T006 | Xác định chuẩn chất lượng mục từ | M02-T002, M02-T005; mục tiêu người học | Checklist bản nháp/công khai, trường bắt buộc và tiêu chí nghĩa–ví dụ–phát âm | Chủ chất lượng nội dung | Chuyên gia học thuật, M11 | P0 | M | M02-T002, M02-T005 | A-G03; REL-04 khi có tài sản | Chờ phụ thuộc nội bộ |

### 4.3. M11 — Quản trị và vận hành

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M11-T001 | Thống nhất từ điển quản trị | Quyết định M11; thuật ngữ M01/M02/M12 | Từ điển quyền, trạng thái, thay đổi, audit, log, sự cố và hỗ trợ | Chủ M11 | Chủ M01/M02/M12 | P0 | S | Không | A-G02, A-G06 | Sẵn sàng |
| M11-T002 | Lập danh mục hành động quản trị | M11-T001; hiện trạng thao tác | Danh mục hành động có module sở hữu, độ nhạy, điều kiện từ chối và bằng chứng | Chủ M11 | An toàn hệ thống và các chủ module | P0 | L | M11-T001 | A-G02; REL-02 | Chờ M11-T001 |
| M11-T012 | Lập sổ đăng ký cấu hình | M11-T001; cấu hình hiện có | Sổ khóa cấu hình, chủ, module dùng, độ nhạy, mặc định, lưu giữ và quyền sửa | Chủ vận hành cấu hình | Các chủ module nguồn | P0 | L | M11-T001 | A-G02, A-G06 | Chờ M11-T001 |
| M11-T018 | Lập ma trận nội dung quản trị chéo module | M11-T002; danh mục nội dung | Ma trận loại nội dung–chủ–vòng đời–quyền–rủi ro và hành động được phép | Chủ M11/nội dung | Chủ M02 và các chủ nội dung | P0 | M | M11-T002 | A-G02, A-G03; REL-04 | Chờ M11-T002 |
| M11-T022 | Lập từ điển chỉ số quản trị | M11-T001; chỉ số hiện có | Từ điển công thức, mẫu số, nguồn, độ mới, chủ và giới hạn sử dụng | Chủ dữ liệu vận hành | Chủ sản phẩm và các chủ module | P1 | L | M11-T001 | A-G06 | Chờ M11-T001 |

### 4.4. M12 — Tích hợp và tài sản số

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M12-T001 | Thống nhất từ điển tích hợp | Quyết định M12; thuật ngữ module tiêu thụ | Từ điển năng lực, provider, request, attempt, lỗi, suy giảm và fallback | Chủ M12 | Chủ M01/M02/M11 | P0 | S | Không | A-G04 | Sẵn sàng |
| M12-T002 | Lập sổ đăng ký năng lực tích hợp | M12-T001; hiện trạng tích hợp | Registry có chủ, mục đích, module dùng, dữ liệu, môi trường và trạng thái | Chủ M12 | Vận hành, an toàn hệ thống | P0 | L | M12-T001 | A-G04, A-G05; REL-03 | Chờ M12-T001 |
| M12-T003 | Phân loại mức quan trọng và tác động | M12-T002; hành trình A/B | Ma trận criticality, tác động, mức chịu gián đoạn, phục hồi và suy giảm | Chủ kiến trúc/vận hành | Chủ sản phẩm và module tiêu thụ | P0 | L | M12-T002 | A-G04, A-G06; REL-03 | Chờ M12-T002 |
| M12-T004 | Đặc tả hợp đồng dữ liệu chuẩn | M12-T002, M12-T003; nhu cầu module | Hợp đồng trung lập provider có mục đích, version, deadline, idempotency và correlation | Chủ M12 | Các chủ module tiêu thụ | P0 | L | M12-T002, M12-T003 | A-G04, A-G05; REL-03 | Chờ phụ thuộc nội bộ |
| M12-T005 | Chuẩn hóa trạng thái kết quả và lỗi | M12-T004; lỗi provider hiện có | Taxonomy thành công/no-data/uncertain/lỗi tạm-cuối/hết hạn/hủy và hành vi tiêu thụ | Chủ M12 | Vận hành và các chủ module tiêu thụ | P0 | L | M12-T004 | A-G04; REL-03 | Chờ M12-T004 |
| M12-T021 | Lập danh mục loại tài sản số | M12-T002; nhu cầu M01/M02 | Catalog loại tài sản có chủ, định dạng, kích thước, access, retention, license và placeholder | Chủ tài sản M12 | Chủ M01/M02, riêng tư | P0 | M | M12-T002; A0-T004 | A-G03, A-G05; REL-04 | Chờ registry và hồ sơ REL-04 |
| M12-T031 | Lập danh mục use case trạng thái chia sẻ | M12-T002; cache/limiter/trạng thái hiện có | Registry nguồn sự thật, namespace, TTL, consistency, quota, criticality và failure mode | Chủ nền tảng M12 | Chủ M11 và module tiêu thụ | P0 | L | M12-T002 | A-G04; REL-03 | Chờ M12-T002 |
| M12-T040 | Kiểm kê và phân loại bí mật | M12-T002; cấu hình/artifact/log trong phạm vi | Inventory không chứa giá trị bí mật, có owner, vị trí, mức phơi lộ, hạn và hành động xoay vòng | Chủ an toàn nền tảng | Chủ M12/M11 | P0 | L | A0-T003; M12-T002 | A-G05; REL-03 | Có thể bắt đầu khung; chờ registry để hoàn tất |
| M12-T042-A | Lập bản đồ dữ liệu rời hệ thống — lát A | M12-T004; dữ liệu danh tính/tài sản/kênh A/B | Bản đồ trường–mục đích–provider–nơi xử lý–lưu giữ–đồng ý–xóa cho phạm vi A/B; AI/speech ghi “tắt” | Chủ riêng tư M12 | Chủ M01/M02/M10 và pháp lý | P0 | L | M12-T004; A0-T001, A0-T003 | A-G05; REL-01, REL-03 | Lát A của M12-T042; không đóng toàn bộ task nguồn |
| M12-T044-A | Xây dựng sổ quyền tài sản — lát A | Asset catalog; tài sản A/B; REL-04 | Sổ nguồn/quyền/attribution/trạng thái rủi ro và đầu mối takedown cho tài sản A/B | Chủ tài sản/nội dung | Sản phẩm và pháp lý | P0 | L | M12-T021; A0-T004 | A-G03, A-G05; REL-04 | Lát A của M12-T044; hoàn tất provenance sâu sau M12-T023 |

## 5. Thứ tự kéo task vào thực hiện

Không khởi động đồng thời cả 30 task Lát 1. Thứ tự kéo khuyến nghị:

| Đợt | Task được kéo | Điều kiện vào | Kết quả mở khóa |
|---:|---|---|---|
| 1 | A0-T001–A0-T012; M01-T001; M11-T001; M12-T001 | Có người nhận vai trò chủ trì/xác nhận | Hồ sơ cổng, kiểm soát tạm thời và ba từ điển nền |
| 2 | M02-T001; M01-T002–T004; M11-T002, T012, T022; M12-T002 | Các từ điển liên quan đã duyệt | Vòng đời sơ bộ và registry gốc |
| 3 | M01-T003–T007; M02-T002, T005; M11-T018; M12-T003, T021, T031, T040 | Registry/chủ dữ liệu rõ; hồ sơ REL đã mở | Mô hình dữ liệu/criticality/catalog và chính sách nền |
| 4 | M02-T003–T004, T006; M12-T004–T005; M12-T042-A, T044-A | Mô hình nghĩa và criticality được duyệt | Quy tắc chất lượng/trùng, hợp đồng lỗi, bản đồ dữ liệu/quyền |
| 5 | M01-T008–T009 | Hợp đồng tối thiểu M06 và M12; REL-01 có hướng xử lý | Điều phối khởi tạo và bộ nghiệm thu đăng ký |

Mỗi đợt chỉ chuyển sang đợt tiếp theo khi đầu ra nền đã được xác nhận. Việc bắt đầu soạn trước được phép, nhưng không được đóng task phụ thuộc trước task nguồn.

## 6. Bảng kiểm sẵn sàng Lát 0–Lát 1

| Điều kiện | Trạng thái ban đầu | Cách xác nhận |
|---|---|---|
| Có chủ thực tế cho REL-01–REL-04 và REL-07 | Chưa gán | Tên cá nhân, vai trò, người thay thế và ngày nhận trách nhiệm trong công cụ quản lý |
| Có người xác nhận cho từng từ điển/registry | Chưa gán | Người xác nhận chấp nhận trách nhiệm và tiêu chí duyệt |
| CT-01–CT-07 có phạm vi kiểm tra được | Chưa có bằng chứng | Biên bản, danh sách năng lực/luồng và lịch rà soát |
| Phạm vi thị trường/độ tuổi A/B | Chờ REL-01 | Tài liệu sản phẩm/pháp lý đã duyệt |
| Phạm vi tài sản A/B và mức rủi ro quyền | Chờ REL-04 | Danh mục tài sản và văn bản chấp nhận/xử lý |
| Inventory không chứa giá trị bí mật | Chưa lập | Rà soát mẫu biểu trước khi thu thập |
| Hợp đồng tối thiểu M06 cho khởi tạo người dùng | Chưa có trong Lát 1 | Đầu ra ranh giới, chống cấp lặp và không tạo AP mới |
| Hai lát M12-T042-A/T044-A không đóng task nguồn toàn phần | Đã xác định trong kế hoạch | Trạng thái parent vẫn mở và nhánh hoãn được ghi rõ |

## 7. Tiêu chí hoàn thành gói Lát 0–Lát 1

Gói chỉ được coi là hoàn thành khi:

1. Mười hai task A0 có chủ và bằng chứng theo Definition of Done chung.
2. Ba mươi task nguồn/lát con được nhập với đầy đủ phụ thuộc, Cổng A, REL và trạng thái baseline.
3. Các task từ điển và registry đã được duyệt bởi cả module nguồn lẫn module tiêu thụ liên quan.
4. Không có task bị đóng khi vẫn chờ đầu vào pháp lý, M06 hoặc task nguồn.
5. M12-T042-A và M12-T044-A chỉ đóng lát A; task nguồn toàn phần vẫn mở.
6. Danh sách sai lệch mới phát hiện được liên kết lại với Cổng A, REL và lát triển khai tiếp theo.

## 8. Bước tiếp theo sau gói này

Sau khi tạo/import task, chuẩn bị bộ mẫu bằng chứng dùng chung cho A-G01–A-G06: ma trận trạng thái/quyền, mẫu registry, hồ sơ thay đổi, audit trước/sau, bản đồ dữ liệu, báo cáo kiểm thử suy giảm, playbook sự cố và biên bản diễn tập.
