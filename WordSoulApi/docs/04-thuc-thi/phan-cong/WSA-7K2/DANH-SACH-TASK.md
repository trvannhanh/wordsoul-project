# Danh sách task solo — WSA-7K2

- Vai trò công việc: Người thực hiện duy nhất Giai đoạn A
- Miền: M01, M02, M11, M12 và toàn bộ task điều phối
- Tổng: **167 task / 412 điểm trọng số**.
- Trọng số: S=1, M=2, L=3; không phải ước lượng thời gian.

| Task | Lát | Module | Tên | Ưu tiên | Size | Phụ thuộc | Cổng/REL | Baseline | Trạng thái làm việc |
|---|---|---|---|---|---|---|---|---|---|
| A0-T001 | 0 | Điều phối | Mở hồ sơ REL-01 tuổi và đồng ý | P0 | M | Không | A-G01; REL-01 | Task điều phối mới | Chưa bắt đầu |
| A0-T002 | 0 | Điều phối | Mở hồ sơ REL-02 quyền và audit | P0 | M | Không | A-G02, A-G06; REL-02 | Task điều phối mới | Chưa bắt đầu |
| A0-T003 | 0 | Điều phối | Mở hồ sơ REL-03 bí mật và tích hợp | P0 | M | Không | A-G04–A-G06; REL-03 | Task điều phối mới | Chưa bắt đầu |
| A0-T004 | 0 | Điều phối | Mở hồ sơ REL-04 quyền tài sản | P0 | M | Không | A-G03, A-G05; REL-04 | Task điều phối mới | Chưa bắt đầu |
| A0-T005 | 0 | Điều phối | Mở hồ sơ REL-07 xuất và xóa dữ liệu | P0 | L | A0-T001 | A-G01, A-G02; REL-07 | Task điều phối mới | Chưa bắt đầu |
| A0-T006 | 0 | Điều phối | Ghi nhận CT-01 đóng công khai nội dung chưa duyệt | P0 | S | A0-T004 | A-G03; REL-04 | Task điều phối mới | Chưa bắt đầu |
| A0-T007 | 0 | Điều phối | Ghi nhận CT-02 không tự ghép tài khoản theo email | P0 | S | Không | A-G01 | Task điều phối mới | Chưa bắt đầu |
| A0-T008 | 0 | Điều phối | Ghi nhận CT-03 không dùng payload thô làm bằng chứng hợp lệ | P0 | M | A0-T003 | A-G02, A-G05; REL-02, REL-03 | Task điều phối mới | Chưa bắt đầu |
| A0-T009 | 0 | Điều phối | Ghi nhận CT-04 không dùng health giả định để phát hành | P0 | S | Không | A-G04, A-G06; REL-03 | Task điều phối mới | Chưa bắt đầu |
| A0-T010 | 0 | Điều phối | Ghi nhận CT-05 cấm bỏ qua giới hạn lưu lượng | P0 | S | A0-T003 | A-G04; REL-03 | Task điều phối mới | Chưa bắt đầu |
| A0-T011 | 0 | Điều phối | Ghi nhận CT-06 giữ AI/giọng nói tắt trong A/B | P0 | S | A0-T001, A0-T003 | A-G04, A-G05; REL-01, REL-03 | Task điều phối mới | Chưa bắt đầu |
| A0-T012 | 0 | Điều phối | Ghi nhận CT-07 không mở rộng AP | P1 | M | Không | REL-05; chuẩn bị B-G03 | Task điều phối mới | Chưa bắt đầu |
| M01-T001 | 1 | M01 | Chuẩn hóa từ điển danh tính | P0 | S | Không | A-G01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T002 | 1 | M01 | Xác định vòng đời tài khoản | P0 | M | M01-T001; A0-T001 | A-G01; REL-01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T003 | 1 | M01 | Lập bản đồ dữ liệu hồ sơ | P0 | M | M01-T001 | A-G01, A-G05; REL-01, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T004 | 1 | M01 | Chốt chính sách thông tin bảo mật | P0 | M | M01-T001 | A-G01, A-G02; REL-02 | Chưa có | Chưa bắt đầu |
| M01-T005 | 1 | M01 | Chuẩn hóa dữ liệu đăng ký | P0 | M | M01-T003, M01-T004 | A-G01; REL-01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T006 | 1 | M01 | Thiết kế xác minh thư điện tử | P0 | M | M01-T002, M01-T005; hợp đồng thư tối thiểu | A-G01 | Chưa có | Chưa bắt đầu |
| M01-T007 | 1 | M01 | Ghi nhận đồng ý và phiên bản chính sách | P0 | S | M01-T002, M01-T003; A0-T001 | A-G01; REL-01, REL-06 | Chưa có | Chưa bắt đầu |
| M01-T008 | 1 | M01 | Điều phối khởi tạo người dùng mới | P0 | L | M01-T005–T007; hợp đồng tối thiểu M06 | A-G01; chuẩn bị B-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T009 | 1 | M01 | Xây dựng tiêu chí nghiệm thu đăng ký | P0 | M | M01-T005–T008 | A-G01; REL-01 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T001 | 1 | M02 | Chuẩn hóa từ điển học liệu | P0 | S | M01-T001 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T002 | 1 | M02 | Thiết kế mô hình nhiều nghĩa và loại từ | P0 | L | M02-T001 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T003 | 1 | M02 | Chuẩn hóa mặt chữ và biến thể | P0 | M | M02-T001, M02-T002 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T004 | 1 | M02 | Thiết kế phát hiện nội dung trùng | P0 | L | M02-T002, M02-T003 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T005 | 1 | M02 | Chuẩn hóa cấp độ và độ khó | P0 | M | M02-T002 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T006 | 1 | M02 | Xác định chuẩn chất lượng mục từ | P0 | M | M02-T002, M02-T005 | A-G03; REL-04 khi có tài sản | Đáp ứng một phần | Chưa bắt đầu |
| M11-T001 | 1 | M11 | Thống nhất từ điển quản trị | P0 | S | Không | A-G02, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T002 | 1 | M11 | Lập danh mục hành động quản trị | P0 | L | M11-T001 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T012 | 1 | M11 | Lập sổ đăng ký cấu hình | P0 | L | M11-T001 | A-G02, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T018 | 1 | M11 | Lập ma trận nội dung quản trị chéo module | P0 | M | M11-T002 | A-G02, A-G03; REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T022 | 1 | M11 | Lập từ điển chỉ số quản trị | P1 | L | M11-T001 | A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T001 | 1 | M12 | Thống nhất từ điển tích hợp | P0 | S | Không | A-G04 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T002 | 1 | M12 | Lập sổ đăng ký năng lực tích hợp | P0 | L | M12-T001 | A-G04, A-G05; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T003 | 1 | M12 | Phân loại mức quan trọng và tác động | P0 | L | M12-T002 | A-G04, A-G06; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T004 | 1 | M12 | Đặc tả hợp đồng dữ liệu chuẩn | P0 | L | M12-T002, M12-T003 | A-G04, A-G05; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T005 | 1 | M12 | Chuẩn hóa trạng thái kết quả và lỗi | P0 | L | M12-T004 | A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T021 | 1 | M12 | Lập danh mục loại tài sản số | P0 | M | M12-T002; A0-T004 | A-G03, A-G05; REL-04 | Chưa có | Chưa bắt đầu |
| M12-T031 | 1 | M12 | Lập danh mục use case trạng thái chia sẻ | P0 | L | M12-T002 | A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T040 | 1 | M12 | Kiểm kê và phân loại bí mật | P0 | L | A0-T003; M12-T002 | A-G05; REL-03 | Chưa có | Chưa bắt đầu |
| M12-T042-A | 1 | M12 | Lập bản đồ dữ liệu rời hệ thống — lát A | P0 | L | M12-T004; A0-T001, A0-T003 | A-G05; REL-01, REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T044-A | 1 | M12 | Xây dựng sổ quyền tài sản — lát A | P0 | L | M12-T021; A0-T004 | A-G03, A-G05; REL-04 | Chưa có | Chưa bắt đầu |
| M01-T010 | 2 | M01 | Chuẩn hóa luồng đăng nhập trực tiếp | P0 | M | M01-T002, T004 | A-G01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T011 | 2 | M01 | Thiết kế kiểm soát thử đăng nhập bất thường | P0 | M | M01-T010; M12-T034 | A-G01, A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T012 | 2 | M01 | Chuẩn hóa xử lý tài khoản không hoạt động | P0 | S | M01-T002, T010 | A-G01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T013 | 2 | M01 | Chuẩn hóa đăng nhập bằng danh tính bên ngoài | P0 | M | M01-T002, T003; M12-T006–T010 | A-G01, A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T014 | 2 | M01 | Bảo vệ liên kết tài khoản hiện có | P0 | L | M01-T006, T010, T013 | A-G01 | Chưa có | Chưa bắt đầu |
| M01-T015 | 2 | M01 | Xử lý xung đột và gỡ liên kết | P0 | M | M01-T014, T019 | A-G01 | Chưa có | Chưa bắt đầu |
| M01-T016 | 2 | M01 | Chốt chính sách vòng đời phiên | P0 | L | M01-T002, T004 | A-G01, A-G02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T017 | 2 | M01 | Chuẩn hóa gia hạn và phát hiện tái sử dụng | P0 | L | M01-T012, T016 | A-G01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T018 | 2 | M01 | Thiết kế đăng xuất và quản lý phiên | P0 | M | M01-T016; hợp đồng M01-T025 | A-G01 | Chưa có | Chưa bắt đầu |
| M01-T019 | 2 | M01 | Thiết kế khôi phục quyền truy cập | P0 | L | M01-T004, T006, T016; M12 | A-G01 | Chưa có | Chưa bắt đầu |
| M01-T020 | 2 | M01 | Thiết kế thay đổi thông tin bảo mật | P0 | M | M01-T004, T016, T019 | A-G01, A-G02 | Chưa có | Chưa bắt đầu |
| M01-T021 | 2 | M01 | Xác định đường hỗ trợ khi mất mọi kênh | P1 | L | M01-T003, T019; M11-T029 | A-G01, A-G02; REL-07 | Chưa có | Chưa bắt đầu |
| M01-T028 | 2 | M01 | Xây dựng ma trận vai trò và quyền | P0 | L | M01-T001; M11-T004 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T029 | 2 | M01 | Chuẩn hóa thay đổi vai trò | P0 | M | M01-T016, T028; M11-T031 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T030 | 2 | M01 | Bảo vệ vai trò quản trị cao nhất | P0 | M | M01-T028, T029; M11-T006-A | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T031 | 2 | M01 | Chuẩn hóa khóa và mở tài khoản | P0 | M | M01-T012, T016, T028 | A-G01, A-G02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T032 | 2 | M01 | Thiết kế xử lý thay đổi quản trị đồng thời | P1 | M | M01-T029, T031 | A-G02; REL-02 | Chưa có | Chưa bắt đầu |
| M01-T038 | 2 | M01 | Xây dựng danh mục sự kiện danh tính | P0 | M | M01-T002, T028, T033; M11-T031 | A-G01, A-G02 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T039 | 2 | M01 | Thiết kế cảnh báo hành vi danh tính bất thường | P0 | L | M01-T011, T019, T038 | A-G01, A-G06 | Chưa có | Chưa bắt đầu |
| M01-T040 | 2 | M01 | Xác định chỉ số sức khỏe M01 | P1 | M | M01-T038; M11-T022 | A-G01, A-G06 | Chưa có | Chưa bắt đầu |
| M01-T041 | 2 | M01 | Chuẩn hóa quyền tra cứu lịch sử danh tính | P0 | M | M01-T028, T038; M11-T027–T035 | A-G02; REL-02, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T003 | 2 | M11 | Xây dựng vai trò quản trị | P0 | L | M11-T002 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T004 | 2 | M11 | Xây dựng ma trận quyền tối thiểu | P0 | L | M11-T002, T003 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T005 | 2 | M11 | Thiết kế cấp và thu hồi quyền | P0 | L | M11-T004; M01-T016 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T006-A | 2 | M11 | Kiểm chứng không có quyền tạm thời/khẩn cấp | P0 | M | M11-T005 | A-G02, A-G06; REL-02 | Không còn phù hợp | Chưa bắt đầu |
| M11-T007 | 2 | M11 | Phân loại thao tác cần kiểm soát tăng cường | P0 | L | M11-T002, T004 | A-G02; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T027 | 2 | M11 | Đặc tả tìm kiếm người dùng an toàn | P0 | M | M11-T004; M01-T003 | A-G02; REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T028 | 2 | M11 | Xây dựng dòng thời gian hỗ trợ | P0 | L | M11-T027; M01-T038 | A-G02; REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T029 | 2 | M11 | Thiết kế vòng đời vụ việc hỗ trợ | P0 | L | M11-T027, T028 | A-G02; REL-07 | Chưa có | Chưa bắt đầu |
| M11-T030 | 2 | M11 | Chốt thao tác hỗ trợ có kiểm soát | P0 | L | M11-T029; hợp đồng thay đổi tối thiểu | A-G02; REL-02, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T031 | 2 | M11 | Đặc tả sự kiện kiểm toán chuẩn | P0 | L | M11-T002 | A-G02, A-G05; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T032 | 2 | M11 | Phân tách audit, activity và log vận hành | P0 | M | M11-T031 | A-G02, A-G05; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T033 | 2 | M11 | Xây dựng quy tắc che dữ liệu và bí mật | P0 | L | M11-T032; M12-T040–T043 | A-G02, A-G05; REL-03 | Chưa có | Chưa bắt đầu |
| M11-T034 | 2 | M11 | Thiết kế thu nhận log bền vững | P0 | L | M11-T033; M12-T003 | A-G02, A-G06; REL-02, REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T035 | 2 | M11 | Chốt tìm kiếm, truy cập và lưu giữ log | P0 | L | M11-T004, T034 | A-G02, A-G05; REL-02, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T006 | 2 | M12 | Đặc tả dữ liệu danh tính tối thiểu | P0 | M | M12-T004; M01-T003 | A-G01, A-G05; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T007 | 2 | M12 | Thiết kế chống giả mạo và phát lại | P0 | L | M12-T006 | A-G01, A-G04 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T008 | 2 | M12 | Chốt vòng đời token ngoài | P0 | L | M12-T006, T007; M12-T040 | A-G01, A-G05; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T009 | 2 | M12 | Đặc tả liên kết và ngắt liên kết | P0 | L | M12-T008; M01-T014–T015 | A-G01 | Chưa có | Chưa bắt đầu |
| M12-T010 | 2 | M12 | Thiết kế suy giảm khi danh tính ngoài lỗi | P0 | L | M12-T003, T009 | A-G01, A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T034 | 2 | M12 | Xây dựng ma trận giới hạn lưu lượng | P0 | L | M12-T003, T031 | A-G01, A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T035 | 2 | M12 | Chốt fail-open/fail-closed theo năng lực | P0 | L | M12-T003, T034 | A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T041 | 2 | M12 | Thiết kế vòng đời bí mật | P0 | L | M12-T040; M11-T004 | A-G05; REL-03 | Chưa có | Chưa bắt đầu |
| M12-T043 | 2 | M12 | Chốt che dữ liệu và chính sách log | P0 | L | M12-T040–T042-A; M11-T031–T033 | A-G02, A-G05; REL-03 | Chưa có | Chưa bắt đầu |
| M11-T008 | 3A | M11 | Đặc tả yêu cầu thay đổi | P0 | M | M11-T007 | A-G02, A-G06; REL-02 | Chưa có | Chưa bắt đầu |
| M11-T009 | 3A | M11 | Thiết kế vòng đời quyết định thay đổi | P0 | L | M11-T005, T008 | A-G02; REL-02 | Chưa có | Chưa bắt đầu |
| M11-T010 | 3A | M11 | Chốt xung đột và lịch hiệu lực | P0 | L | M11-T009 | A-G02, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T011 | 3A | M11 | Thiết kế thực thi và rollback có kiểm chứng | P0 | L | M11-T009, T010 | A-G02, A-G06; REL-02 | Chưa có | Chưa bắt đầu |
| M11-T013 | 3A | M11 | Đặc tả kiểm tra giá trị và phụ thuộc | P0 | L | M11-T012 | A-G02 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T014 | 3A | M11 | Thiết kế phiên bản cấu hình bất biến | P0 | L | M11-T008, T013 | A-G02, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T015 | 3A | M11 | Thiết kế xem trước và mô phỏng tác động | P0 | L | M11-T014, T022 | A-G02, A-G06 | Chưa có | Chưa bắt đầu |
| M11-T016 | 3A | M11 | Chốt triển khai giới hạn và quan sát | P0 | L | M11-T011, T015 | A-G02, A-G06 | Chưa có | Chưa bắt đầu |
| M11-T017 | 3A | M11 | Thiết kế quay lại và xử lý khóa ngừng dùng | P0 | L | M11-T014, T016 | A-G02, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T019 | 3A | M11 | Chuẩn hóa vòng đời nội dung | P0 | L | M11-T009, T018 | A-G02, A-G03; REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T020 | 3A | M11 | Thiết kế phân tích tham chiếu trước thay đổi | P0 | L | M11-T018, T019 | A-G03, A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T021 | 3A | M11 | Chốt xử lý chỉnh sửa đồng thời | P1 | M | M11-T019 | A-G02, A-G03 | Chưa có | Chưa bắt đầu |
| M11-T038 | 3A | M11 | Lập sổ đăng ký công việc nền | P0 | L | M11-T036 thuộc Lát 4 | A-G06; REL-03, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T039 | 3A | M11 | Thiết kế lịch sử chạy và phục hồi công việc | P0 | L | M11-T004, T038 | A-G02, A-G06; REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T040-A | 3A | M11 | Thiết kế đối soát và cảnh báo sai lệch — lát A | P0 | L | M11-T039; M11-T024 ở B cho bản đầy đủ | A-G06; REL-07 | Chưa có | Chưa bắt đầu |
| M02-T007 | 3B | M02 | Xác định vòng đời mục từ | P0 | M | M02-T001; M11-T019 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T008-A | 3B | M02 | Thiết kế phiên bản hóa mục từ — lát A | P0 | L | M02-T006, T007 | A-G03 | Chưa có | Chưa bắt đầu |
| M02-T009-A | 3B | M02 | Chuẩn hóa nội dung cung cấp module học — lát A | P0 | M | M02-T002, T006, T008-A | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T010 | 3B | M02 | Thiết kế ngừng dùng, hợp nhất và thay thế | P0 | L | M02-T004, T008-A, T009-A | A-G03 | Chưa có | Chưa bắt đầu |
| M02-T015 | 3B | M02 | Chuẩn hóa dữ liệu và tiêu chí bộ từ | P0 | M | M02-T005, T006 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T016 | 3B | M02 | Xây dựng ma trận quyền bộ từ | P0 | M | M01-T028, M02-T007; M11-T004 | A-G02, A-G03; REL-02 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T017 | 3B | M02 | Thiết kế vòng đời bộ từ | P0 | L | M02-T015, T016 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T018 | 3B | M02 | Xử lý chủ sở hữu không còn hoạt động | P1 | M | M01-T031; M01-T036 thuộc Lát 5; M02-T016 | A-G01, A-G03; REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T019 | 3B | M02 | Thiết kế sao chép và nguồn gốc bộ từ | P2 | M | M02-T016, T017 | A-G03; REL-04 | Chưa có | Chưa bắt đầu |
| M02-T020 | 3B | M02 | Chuẩn hóa thêm và bỏ mục từ trong bộ | P0 | M | M02-T004, T017 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T021 | 3B | M02 | Thiết kế sắp xếp thành phần bộ | P0 | M | M02-T020; M11-T021 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T022 | 3B | M02 | Chuẩn hóa nội dung ghi đè theo bộ | P0 | L | M02-T002, T006, T020 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T023-A | 3B | M02 | Xác định tác động thay đổi bộ đang học — lát A | P0 | L | M02-T017, T020–T022 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T029 | 3B | M02 | Thiết kế quy trình gửi duyệt | P0 | M | M02-T006, T012, T017, T022 | A-G03 | Chưa có | Chưa bắt đầu |
| M02-T030 | 3B | M02 | Xây dựng checklist kiểm duyệt công khai | P0 | M | M02-T006, T012, T015 | A-G03; REL-04 | Chưa có | Chưa bắt đầu |
| M02-T031 | 3B | M02 | Thiết kế yêu cầu sửa và từ chối | P0 | M | M02-T029, T030 | A-G03 | Chưa có | Chưa bắt đầu |
| M02-T032 | 3B | M02 | Thiết kế xuất bản theo phiên bản | P0 | L | M02-T008-A, T017, T029, T030 | A-G03 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T033 | 3B | M02 | Thiết kế báo cáo và thu hồi nội dung | P0 | L | M02-T010, T032; M11-T019–T020 | A-G03, A-G06; REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T034 | 3B | M02 | Thiết kế khiếu nại quyết định nội dung | P2 | M | M02-T031, T033 | A-G03; REL-04 | Chưa có | Chưa bắt đầu |
| M01-T024 | 3C | M01 | Thiết kế vòng đời ảnh đại diện | P1 | M | M01-T022-A; M12-T021–T025 | A-G01, A-G05; REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T011 | 3C | M02 | Lập danh mục tài sản học liệu | P0 | M | M02-T006; M12-T021 | A-G03, A-G05; REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T012 | 3C | M02 | Chuẩn hóa kiểm duyệt tài sản | P0 | M | M02-T011 | A-G03; REL-04 | Chưa có | Chưa bắt đầu |
| M02-T013 | 3C | M02 | Thiết kế xử lý tài sản lỗi hoặc thiếu | P1 | M | M02-T011, T012; M12-T003–T005 | A-G03, A-G04 | Đáp ứng một phần | Chưa bắt đầu |
| M02-T014 | 3C | M02 | Thiết kế vòng đời thay thế tài sản | P1 | M | M02-T008-A, T011; M12-T025 | A-G03, A-G05 | Chưa có | Chưa bắt đầu |
| M12-T022 | 3C | M12 | Đặc tả upload an toàn | P0 | L | M12-T021, T040–T043 | A-G05; REL-03, REL-04 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T023 | 3C | M12 | Thiết kế metadata và định danh bất biến | P0 | L | M12-T022 | A-G03, A-G05; REL-04 | Chưa có | Chưa bắt đầu |
| M12-T024 | 3C | M12 | Chốt quyền truy cập và phân phối | P0 | L | M12-T021, T023 | A-G05 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T025 | 3C | M12 | Thiết kế thay thế, xóa và orphan cleanup | P0 | L | M12-T023, T024; M11-T020 | A-G03, A-G05; REL-04, REL-07 | Chưa có | Chưa bắt đầu |
| M01-T022-A | 3D | M01 | Chuẩn hóa quyền xem và sửa hồ sơ — lát A | P0 | M | M01-T003, T028 | A-G01, A-G02; REL-01, REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T023-A | 3D | M01 | Thiết kế thay đổi tên hiển thị — lát A | P1 | S | M01-T022-A | A-G01 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T025-A | 3D | M01 | Chuẩn hóa múi giờ và giờ học mong muốn — lát A | P0 | M | M01-T022-A | A-G01; chuẩn bị REL-06 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T026-A | 3D | M01 | Thiết kế đăng ký nhiều thiết bị nhận tin — lát A | P0 | M | M01-T016, T025-A | A-G01, A-G05; REL-06 | Chưa có | Chưa bắt đầu |
| M01-T027-A | 3D | M01 | Thiết kế thu hồi thiết bị nhận tin — lát A | P0 | M | M01-T018, T026-A | A-G01, A-G05; REL-06 | Chưa có | Chưa bắt đầu |
| M11-T036 | 4 | M11 | Lập sổ sức khỏe năng lực và tích hợp | P0 | L | M11-T022; M12-T002–T005 | A-G04, A-G06; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T037 | 4 | M11 | Thiết kế cảnh báo và escalation | P0 | L | M11-T036 | A-G06; REL-03 | Chưa có | Chưa bắt đầu |
| M11-T043-A | 4 | M11 | Thiết kế chế độ bảo trì — lát A | P0 | L | M11-T036; hợp đồng M10 tối thiểu | A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T044 | 4 | M11 | Thiết kế kill switch và dừng khẩn | P0 | L | M11-T006-A, T017, T036 | A-G04, A-G06; REL-02, REL-03 | Chưa có | Chưa bắt đầu |
| M11-T045 | 4 | M11 | Xây dựng mô hình mức độ sự cố | P0 | M | M11-T036, T037 | A-G06 | Đáp ứng một phần | Chưa bắt đầu |
| M11-T046 | 4 | M11 | Xây dựng playbook sự cố trọng yếu | P0 | L | M11-T037, T040-A, T043-A–T045 | A-G06; REL-02, REL-03, REL-07 | Chưa có | Chưa bắt đầu |
| M11-T047-A | 4 | M11 | Thiết kế truyền thông và hậu kiểm — lát A | P0 | L | M11-T031, T046 | A-G06 | Chưa có | Chưa bắt đầu |
| M11-T048 | 4 | M11 | Chốt mục tiêu phục hồi và diễn tập | P0 | L | M11-T045–T047-A; M12-T036–T047-A | A-G06; REL-02, REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T032 | 4 | M12 | Thiết kế namespace, TTL và invalidation | P0 | L | M12-T031, T040 | A-G04, A-G05; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T033 | 4 | M12 | Chốt khóa phân tán và ownership | P0 | L | M12-T031 | A-G04; REL-03 | Chưa có | Chưa bắt đầu |
| M12-T036 | 4 | M12 | Chuẩn hóa timeout, deadline và hủy | P0 | L | M12-T003–T005 | A-G04, A-G06; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T037 | 4 | M12 | Chuẩn hóa retry và idempotency | P0 | L | M12-T005, T036 | A-G04; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T038 | 4 | M12 | Thiết kế circuit breaker và bulkhead | P0 | L | M12-T003, T036, T037 | A-G04, A-G06; REL-03 | Chưa có | Chưa bắt đầu |
| M12-T045 | 4 | M12 | Định nghĩa SLO và health từng năng lực | P0 | L | M12-T003, T005; M11-T036 | A-G04, A-G06; REL-03 | Đáp ứng một phần | Chưa bắt đầu |
| M12-T046 | 4 | M12 | Thiết kế đo usage, chi phí và ngân sách | P0 | L | M12-T002, T034, T045 | A-G04, A-G06; REL-03 | Chưa có | Chưa bắt đầu |
| M12-T047-A | 4 | M12 | Xây dựng kiểm thử hợp đồng và canary — lát A | P0 | L | M12-T004, T005; active-provider contracts | A-G04, A-G06; REL-03 | Chưa có | Chưa bắt đầu |
| M01-T033 | 5A | M01 | Lập bản đồ dữ liệu cá nhân liên module | P0 | L | M01-T003; M11/M12 registries | A-G01, A-G05; REL-01, REL-07 | Chưa có | Chưa bắt đầu |
| M01-T034 | 5A | M01 | Thiết kế yêu cầu xuất dữ liệu | P1 | L | M01-T019, T033; M11-T029, T038–T040-A | A-G01, A-G02; REL-07 | Chưa có | Chưa bắt đầu |
| M01-T035 | 5A | M01 | Thiết kế yêu cầu xóa tài khoản | P0 | L | M01-T019, T033; M11-T029, T038–T040-A | A-G01, A-G02; REL-07 | Đáp ứng một phần | Chưa bắt đầu |
| M01-T036 | 5A | M01 | Xây dựng ma trận xóa và ẩn danh hóa | P0 | L | M01-T033, T035 | A-G01, A-G05; REL-07 | Chưa có | Chưa bắt đầu |
| M01-T037 | 5A | M01 | Xác định quy tắc đăng ký lại sau xóa | P1 | M | M01-T005, T015, T036 | A-G01; REL-07 | Chưa có | Chưa bắt đầu |
| M01-T042-A | 5B | M01 | Xây dựng bộ nghiệm thu xuyên chức năng M01 — lát A | P0 | L | M01-T009, T015, T018, T021, T027-A, T032, T037, T041 | A-G01, A-G02; REL-01, REL-02, REL-07 | Chưa có | Chưa bắt đầu |
| M01-T043-A | 5B | M01 | Hoàn thiện tài liệu bàn giao M01 — lát A | P0 | M | M01-T042-A | A-G01, A-G02 | Đáp ứng một phần | Chưa bắt đầu |
| A5-T001 | 5 | Điều phối | Đóng băng phạm vi nghiệm thu A | P0 | S | Lát 0–4 | A-G01–A-G06 | Task điều phối mới | Chưa bắt đầu |
| A5-T002 | 5 | Điều phối | Kiểm tra bao phủ 145 task | P0 | M | A5-T001 | A-G01–A-G06 | Task điều phối mới | Chưa bắt đầu |
| A5-T003 | 5 | Điều phối | Nghiệm thu A-G01 | P0 | L | Lát 2, 3D, 5A | A-G01; REL-01, REL-07 | Task điều phối mới | Chưa bắt đầu |
| A5-T004 | 5 | Điều phối | Nghiệm thu A-G02 | P0 | L | Lát 2, 3A, 5A | A-G02; REL-02, REL-07 | Task điều phối mới | Chưa bắt đầu |
| A5-T005 | 5 | Điều phối | Nghiệm thu A-G03 | P0 | L | Lát 3A–3C | A-G03; REL-04 | Task điều phối mới | Chưa bắt đầu |
| A5-T006 | 5 | Điều phối | Nghiệm thu A-G04 | P0 | L | Lát 2, Lát 4 | A-G04; REL-03 | Task điều phối mới | Chưa bắt đầu |
| A5-T007 | 5 | Điều phối | Nghiệm thu A-G05 | P0 | L | Lát 1–4 | A-G05; REL-01, REL-03, REL-04 | Task điều phối mới | Chưa bắt đầu |
| A5-T008 | 5 | Điều phối | Nghiệm thu A-G06 | P0 | L | Lát 4 | A-G06; REL-02, REL-03 | Task điều phối mới | Chưa bắt đầu |
| A5-T009 | 5 | Điều phối | Rà soát đóng REL ảnh hưởng A | P0 | M | A5-T003–T008 | REL-01–REL-04, REL-07 | Task điều phối mới | Chưa bắt đầu |
| A5-T010 | 5 | Điều phối | Ra quyết định Cổng A | P0 | M | A5-T002–T009 | A-G01–A-G06 | Task điều phối mới | Chưa bắt đầu |
