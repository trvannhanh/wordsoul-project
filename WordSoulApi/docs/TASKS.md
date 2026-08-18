# Tasks

Đây là nguồn trạng thái duy nhất của dự án. WSA-7K2 thực hiện toàn bộ 167 task Giai đoạn A; dữ liệu size/điểm và baseline cũ chỉ còn để tham chiếu trong `03-ke-hoach-giai-doan-a/`.

## Cách dùng

1. Chọn task có phụ thuộc đã đáp ứng và đổi `Chưa bắt đầu` thành `Đang thực hiện`.
2. Thực hiện, tự kiểm tra điều kiện hoàn thành và các REL/CT/cổng được ghi trong cột `Hoàn thành khi`.
3. Ghi kết quả kiểm tra ngắn gọn vào cột `Kết quả`, rồi đổi thành `Hoàn thành`; Git tự lưu commit tương ứng.
4. Chỉ dùng `Bị chặn` khi thiếu dữ liệu, công cụ hoặc phụ thuộc kỹ thuật thật; ghi lý do và hành động tiếp theo trong cột kết quả.

Trạng thái hợp lệ: `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành`. Git là nhật ký; không cập nhật tracker hoặc nhật ký cũ.

## Backlog

| ID | Module | Task | Phụ thuộc | Trạng thái | Hoàn thành khi | Kết quả |
|---|---|---|---|---|---|---|
| A0-T001 | Điều phối | Mở hồ sơ REL-01 tuổi và đồng ý | Không | Hoàn thành | Đầu ra đạt; tự kiểm A-G01; REL-01 | Đã mở REL-01 theo D-001; tự kiểm cấu trúc, phạm vi và liên kết A-G01 đạt; câu hỏi pháp lý/sản phẩm còn mở được giữ rõ. |
| A0-T002 | Điều phối | Mở hồ sơ REL-02 quyền và audit | Không | Hoàn thành | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-02 | Đã mở REL-02 theo D-001/D-008; tự kiểm cấu trúc, phạm vi và liên kết A-G02/A-G06 đạt; câu hỏi quyền/audit còn mở được giữ rõ. |
| A0-T003 | Điều phối | Mở hồ sơ REL-03 bí mật và tích hợp | Không | Hoàn thành | Đầu ra đạt; tự kiểm A-G04–A-G06; REL-03 | Đã mở REL-03 theo D-001/D-008; tự kiểm phạm vi, ranh giới dữ liệu, task tham chiếu và liên kết A-G04–A-G06 đạt; inventory còn mở được giữ rõ. |
| A0-T004 | Điều phối | Mở hồ sơ REL-04 quyền tài sản | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04 | — |
| A0-T005 | Điều phối | Mở hồ sơ REL-07 xuất và xóa dữ liệu | A0-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-07 | — |
| A0-T006 | Điều phối | Ghi nhận CT-01 đóng công khai nội dung chưa duyệt | A0-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| A0-T007 | Điều phối | Ghi nhận CT-02 không tự ghép tài khoản theo email | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| A0-T008 | Điều phối | Ghi nhận CT-03 không dùng payload thô làm bằng chứng hợp lệ | A0-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-02, REL-03 | — |
| A0-T009 | Điều phối | Ghi nhận CT-04 không dùng health giả định để phát hành | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| A0-T010 | Điều phối | Ghi nhận CT-05 cấm bỏ qua giới hạn lưu lượng | A0-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| A0-T011 | Điều phối | Ghi nhận CT-06 giữ AI/giọng nói tắt trong A/B | A0-T001, A0-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G05; REL-01, REL-03 | — |
| A0-T012 | Điều phối | Ghi nhận CT-07 không mở rộng AP | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm REL-05; chuẩn bị B-G03 | — |
| M01-T001 | M01 | Chuẩn hóa từ điển danh tính | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T002 | M01 | Xác định vòng đời tài khoản | M01-T001; A0-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-01 | — |
| M01-T003 | M01 | Lập bản đồ dữ liệu hồ sơ | M01-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-01, REL-07 | — |
| M01-T004 | M01 | Chốt chính sách thông tin bảo mật | M01-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-02 | — |
| M01-T005 | M01 | Chuẩn hóa dữ liệu đăng ký | M01-T003, M01-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-01 | — |
| M01-T006 | M01 | Thiết kế xác minh thư điện tử | M01-T002, M01-T005; hợp đồng thư tối thiểu | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T007 | M01 | Ghi nhận đồng ý và phiên bản chính sách | M01-T002, M01-T003; A0-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-01, REL-06 | — |
| M01-T008 | M01 | Điều phối khởi tạo người dùng mới | M01-T005–T007; hợp đồng tối thiểu M06 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; chuẩn bị B-G03 | — |
| M01-T009 | M01 | Xây dựng tiêu chí nghiệm thu đăng ký | M01-T005–T008 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-01 | — |
| M02-T001 | M02 | Chuẩn hóa từ điển học liệu | M01-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T002 | M02 | Thiết kế mô hình nhiều nghĩa và loại từ | M02-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T003 | M02 | Chuẩn hóa mặt chữ và biến thể | M02-T001, M02-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T004 | M02 | Thiết kế phát hiện nội dung trùng | M02-T002, M02-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T005 | M02 | Chuẩn hóa cấp độ và độ khó | M02-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T006 | M02 | Xác định chuẩn chất lượng mục từ | M02-T002, M02-T005 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 khi có tài sản | — |
| M11-T001 | M11 | Thống nhất từ điển quản trị | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T002 | M11 | Lập danh mục hành động quản trị | M11-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T012 | M11 | Lập sổ đăng ký cấu hình | M11-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T018 | M11 | Lập ma trận nội dung quản trị chéo module | M11-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G03; REL-04 | — |
| M11-T022 | M11 | Lập từ điển chỉ số quản trị | M11-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06 | — |
| M12-T001 | M12 | Thống nhất từ điển tích hợp | Không | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04 | — |
| M12-T002 | M12 | Lập sổ đăng ký năng lực tích hợp | M12-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G05; REL-03 | — |
| M12-T003 | M12 | Phân loại mức quan trọng và tác động | M12-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M12-T004 | M12 | Đặc tả hợp đồng dữ liệu chuẩn | M12-T002, M12-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G05; REL-03 | — |
| M12-T005 | M12 | Chuẩn hóa trạng thái kết quả và lỗi | M12-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| M12-T021 | M12 | Lập danh mục loại tài sản số | M12-T002; A0-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04 | — |
| M12-T031 | M12 | Lập danh mục use case trạng thái chia sẻ | M12-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| M12-T040 | M12 | Kiểm kê và phân loại bí mật | A0-T003; M12-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05; REL-03 | — |
| M12-T042-A | M12 | Lập bản đồ dữ liệu rời hệ thống — lát A | M12-T004; A0-T001, A0-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05; REL-01, REL-03 | — |
| M12-T044-A | M12 | Xây dựng sổ quyền tài sản — lát A | M12-T021; A0-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04 | — |
| M01-T010 | M01 | Chuẩn hóa luồng đăng nhập trực tiếp | M01-T002, T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T011 | M01 | Thiết kế kiểm soát thử đăng nhập bất thường | M01-T010; M12-T034 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G04; REL-03 | — |
| M01-T012 | M01 | Chuẩn hóa xử lý tài khoản không hoạt động | M01-T002, T010 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T013 | M01 | Chuẩn hóa đăng nhập bằng danh tính bên ngoài | M01-T002, T003; M12-T006–T010 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G04; REL-03 | — |
| M01-T014 | M01 | Bảo vệ liên kết tài khoản hiện có | M01-T006, T010, T013 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T015 | M01 | Xử lý xung đột và gỡ liên kết | M01-T014, T019 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T016 | M01 | Chốt chính sách vòng đời phiên | M01-T002, T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02 | — |
| M01-T017 | M01 | Chuẩn hóa gia hạn và phát hiện tái sử dụng | M01-T012, T016 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T018 | M01 | Thiết kế đăng xuất và quản lý phiên | M01-T016; hợp đồng M01-T025 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T019 | M01 | Thiết kế khôi phục quyền truy cập | M01-T004, T006, T016; M12 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T020 | M01 | Thiết kế thay đổi thông tin bảo mật | M01-T004, T016, T019 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02 | — |
| M01-T021 | M01 | Xác định đường hỗ trợ khi mất mọi kênh | M01-T003, T019; M11-T029 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-07 | — |
| M01-T028 | M01 | Xây dựng ma trận vai trò và quyền | M01-T001; M11-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M01-T029 | M01 | Chuẩn hóa thay đổi vai trò | M01-T016, T028; M11-T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M01-T030 | M01 | Bảo vệ vai trò quản trị cao nhất | M01-T028, T029; M11-T006-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M01-T031 | M01 | Chuẩn hóa khóa và mở tài khoản | M01-T012, T016, T028 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02 | — |
| M01-T032 | M01 | Thiết kế xử lý thay đổi quản trị đồng thời | M01-T029, T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M01-T038 | M01 | Xây dựng danh mục sự kiện danh tính | M01-T002, T028, T033; M11-T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02 | — |
| M01-T039 | M01 | Thiết kế cảnh báo hành vi danh tính bất thường | M01-T011, T019, T038 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G06 | — |
| M01-T040 | M01 | Xác định chỉ số sức khỏe M01 | M01-T038; M11-T022 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G06 | — |
| M01-T041 | M01 | Chuẩn hóa quyền tra cứu lịch sử danh tính | M01-T028, T038; M11-T027–T035 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02, REL-07 | — |
| M11-T003 | M11 | Xây dựng vai trò quản trị | M11-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T004 | M11 | Xây dựng ma trận quyền tối thiểu | M11-T002, T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T005 | M11 | Thiết kế cấp và thu hồi quyền | M11-T004; M01-T016 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T006-A | M11 | Kiểm chứng không có quyền tạm thời/khẩn cấp | M11-T005 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-02 | — |
| M11-T007 | M11 | Phân loại thao tác cần kiểm soát tăng cường | M11-T002, T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T027 | M11 | Đặc tả tìm kiếm người dùng an toàn | M11-T004; M01-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-07 | — |
| M11-T028 | M11 | Xây dựng dòng thời gian hỗ trợ | M11-T027; M01-T038 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-07 | — |
| M11-T029 | M11 | Thiết kế vòng đời vụ việc hỗ trợ | M11-T027, T028 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-07 | — |
| M11-T030 | M11 | Chốt thao tác hỗ trợ có kiểm soát | M11-T029; hợp đồng thay đổi tối thiểu | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02, REL-07 | — |
| M11-T031 | M11 | Đặc tả sự kiện kiểm toán chuẩn | M11-T002 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-02 | — |
| M11-T032 | M11 | Phân tách audit, activity và log vận hành | M11-T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-02 | — |
| M11-T033 | M11 | Xây dựng quy tắc che dữ liệu và bí mật | M11-T032; M12-T040–T043 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-03 | — |
| M11-T034 | M11 | Thiết kế thu nhận log bền vững | M11-T033; M12-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-02, REL-03 | — |
| M11-T035 | M11 | Chốt tìm kiếm, truy cập và lưu giữ log | M11-T004, T034 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-02, REL-07 | — |
| M12-T006 | M12 | Đặc tả dữ liệu danh tính tối thiểu | M12-T004; M01-T003 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-03 | — |
| M12-T007 | M12 | Thiết kế chống giả mạo và phát lại | M12-T006 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G04 | — |
| M12-T008 | M12 | Chốt vòng đời token ngoài | M12-T006, T007; M12-T040 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-03 | — |
| M12-T009 | M12 | Đặc tả liên kết và ngắt liên kết | M12-T008; M01-T014–T015 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M12-T010 | M12 | Thiết kế suy giảm khi danh tính ngoài lỗi | M12-T003, T009 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G04; REL-03 | — |
| M12-T034 | M12 | Xây dựng ma trận giới hạn lưu lượng | M12-T003, T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G04; REL-03 | — |
| M12-T035 | M12 | Chốt fail-open/fail-closed theo năng lực | M12-T003, T034 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| M12-T041 | M12 | Thiết kế vòng đời bí mật | M12-T040; M11-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05; REL-03 | — |
| M12-T043 | M12 | Chốt che dữ liệu và chính sách log | M12-T040–T042-A; M11-T031–T033 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G05; REL-03 | — |
| M11-T008 | M11 | Đặc tả yêu cầu thay đổi | M11-T007 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-02 | — |
| M11-T009 | M11 | Thiết kế vòng đời quyết định thay đổi | M11-T005, T008 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02 | — |
| M11-T010 | M11 | Chốt xung đột và lịch hiệu lực | M11-T009 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T011 | M11 | Thiết kế thực thi và rollback có kiểm chứng | M11-T009, T010 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-02 | — |
| M11-T013 | M11 | Đặc tả kiểm tra giá trị và phụ thuộc | M11-T012 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02 | — |
| M11-T014 | M11 | Thiết kế phiên bản cấu hình bất biến | M11-T008, T013 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T015 | M11 | Thiết kế xem trước và mô phỏng tác động | M11-T014, T022 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T016 | M11 | Chốt triển khai giới hạn và quan sát | M11-T011, T015 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T017 | M11 | Thiết kế quay lại và xử lý khóa ngừng dùng | M11-T014, T016 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06 | — |
| M11-T019 | M11 | Chuẩn hóa vòng đời nội dung | M11-T009, T018 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G03; REL-04 | — |
| M11-T020 | M11 | Thiết kế phân tích tham chiếu trước thay đổi | M11-T018, T019 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G06 | — |
| M11-T021 | M11 | Chốt xử lý chỉnh sửa đồng thời | M11-T019 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G03 | — |
| M11-T038 | M11 | Lập sổ đăng ký công việc nền | M11-T036 thuộc Lát 4 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-03, REL-07 | — |
| M11-T039 | M11 | Thiết kế lịch sử chạy và phục hồi công việc | M11-T004, T038 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G06; REL-07 | — |
| M11-T040-A | M11 | Thiết kế đối soát và cảnh báo sai lệch — lát A | M11-T039; M11-T024 ở B cho bản đầy đủ | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-07 | — |
| M02-T007 | M02 | Xác định vòng đời mục từ | M02-T001; M11-T019 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T008-A | M02 | Thiết kế phiên bản hóa mục từ — lát A | M02-T006, T007 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T009-A | M02 | Chuẩn hóa nội dung cung cấp module học — lát A | M02-T002, T006, T008-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T010 | M02 | Thiết kế ngừng dùng, hợp nhất và thay thế | M02-T004, T008-A, T009-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T015 | M02 | Chuẩn hóa dữ liệu và tiêu chí bộ từ | M02-T005, T006 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T016 | M02 | Xây dựng ma trận quyền bộ từ | M01-T028, M02-T007; M11-T004 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02, A-G03; REL-02 | — |
| M02-T017 | M02 | Thiết kế vòng đời bộ từ | M02-T015, T016 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T018 | M02 | Xử lý chủ sở hữu không còn hoạt động | M01-T031; M01-T036 thuộc Lát 5; M02-T016 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G03; REL-07 | — |
| M02-T019 | M02 | Thiết kế sao chép và nguồn gốc bộ từ | M02-T016, T017 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| M02-T020 | M02 | Chuẩn hóa thêm và bỏ mục từ trong bộ | M02-T004, T017 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T021 | M02 | Thiết kế sắp xếp thành phần bộ | M02-T020; M11-T021 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T022 | M02 | Chuẩn hóa nội dung ghi đè theo bộ | M02-T002, T006, T020 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T023-A | M02 | Xác định tác động thay đổi bộ đang học — lát A | M02-T017, T020–T022 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T029 | M02 | Thiết kế quy trình gửi duyệt | M02-T006, T012, T017, T022 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T030 | M02 | Xây dựng checklist kiểm duyệt công khai | M02-T006, T012, T015 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| M02-T031 | M02 | Thiết kế yêu cầu sửa và từ chối | M02-T029, T030 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T032 | M02 | Thiết kế xuất bản theo phiên bản | M02-T008-A, T017, T029, T030 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03 | — |
| M02-T033 | M02 | Thiết kế báo cáo và thu hồi nội dung | M02-T010, T032; M11-T019–T020 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G06; REL-04 | — |
| M02-T034 | M02 | Thiết kế khiếu nại quyết định nội dung | M02-T031, T033 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| M01-T024 | M01 | Thiết kế vòng đời ảnh đại diện | M01-T022-A; M12-T021–T025 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-04 | — |
| M02-T011 | M02 | Lập danh mục tài sản học liệu | M02-T006; M12-T021 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04 | — |
| M02-T012 | M02 | Chuẩn hóa kiểm duyệt tài sản | M02-T011 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| M02-T013 | M02 | Thiết kế xử lý tài sản lỗi hoặc thiếu | M02-T011, T012; M12-T003–T005 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G04 | — |
| M02-T014 | M02 | Thiết kế vòng đời thay thế tài sản | M02-T008-A, T011; M12-T025 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05 | — |
| M12-T022 | M12 | Đặc tả upload an toàn | M12-T021, T040–T043 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05; REL-03, REL-04 | — |
| M12-T023 | M12 | Thiết kế metadata và định danh bất biến | M12-T022 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04 | — |
| M12-T024 | M12 | Chốt quyền truy cập và phân phối | M12-T021, T023 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05 | — |
| M12-T025 | M12 | Thiết kế thay thế, xóa và orphan cleanup | M12-T023, T024; M11-T020 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03, A-G05; REL-04, REL-07 | — |
| M01-T022-A | M01 | Chuẩn hóa quyền xem và sửa hồ sơ — lát A | M01-T003, T028 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-01, REL-07 | — |
| M01-T023-A | M01 | Thiết kế thay đổi tên hiển thị — lát A | M01-T022-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01 | — |
| M01-T025-A | M01 | Chuẩn hóa múi giờ và giờ học mong muốn — lát A | M01-T022-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; chuẩn bị REL-06 | — |
| M01-T026-A | M01 | Thiết kế đăng ký nhiều thiết bị nhận tin — lát A | M01-T016, T025-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-06 | — |
| M01-T027-A | M01 | Thiết kế thu hồi thiết bị nhận tin — lát A | M01-T018, T026-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-06 | — |
| M11-T036 | M11 | Lập sổ sức khỏe năng lực và tích hợp | M11-T022; M12-T002–T005 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M11-T037 | M11 | Thiết kế cảnh báo và escalation | M11-T036 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-03 | — |
| M11-T043-A | M11 | Thiết kế chế độ bảo trì — lát A | M11-T036; hợp đồng M10 tối thiểu | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06 | — |
| M11-T044 | M11 | Thiết kế kill switch và dừng khẩn | M11-T006-A, T017, T036 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-02, REL-03 | — |
| M11-T045 | M11 | Xây dựng mô hình mức độ sự cố | M11-T036, T037 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06 | — |
| M11-T046 | M11 | Xây dựng playbook sự cố trọng yếu | M11-T037, T040-A, T043-A–T045 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-02, REL-03, REL-07 | — |
| M11-T047-A | M11 | Thiết kế truyền thông và hậu kiểm — lát A | M11-T031, T046 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06 | — |
| M11-T048 | M11 | Chốt mục tiêu phục hồi và diễn tập | M11-T045–T047-A; M12-T036–T047-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-02, REL-03 | — |
| M12-T032 | M12 | Thiết kế namespace, TTL và invalidation | M12-T031, T040 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G05; REL-03 | — |
| M12-T033 | M12 | Chốt khóa phân tán và ownership | M12-T031 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| M12-T036 | M12 | Chuẩn hóa timeout, deadline và hủy | M12-T003–T005 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M12-T037 | M12 | Chuẩn hóa retry và idempotency | M12-T005, T036 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| M12-T038 | M12 | Thiết kế circuit breaker và bulkhead | M12-T003, T036, T037 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M12-T045 | M12 | Định nghĩa SLO và health từng năng lực | M12-T003, T005; M11-T036 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M12-T046 | M12 | Thiết kế đo usage, chi phí và ngân sách | M12-T002, T034, T045 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M12-T047-A | M12 | Xây dựng kiểm thử hợp đồng và canary — lát A | M12-T004, T005; active-provider contracts | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04, A-G06; REL-03 | — |
| M01-T033 | M01 | Lập bản đồ dữ liệu cá nhân liên module | M01-T003; M11/M12 registries | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-01, REL-07 | — |
| M01-T034 | M01 | Thiết kế yêu cầu xuất dữ liệu | M01-T019, T033; M11-T029, T038–T040-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-07 | — |
| M01-T035 | M01 | Thiết kế yêu cầu xóa tài khoản | M01-T019, T033; M11-T029, T038–T040-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-07 | — |
| M01-T036 | M01 | Xây dựng ma trận xóa và ẩn danh hóa | M01-T033, T035 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G05; REL-07 | — |
| M01-T037 | M01 | Xác định quy tắc đăng ký lại sau xóa | M01-T005, T015, T036 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-07 | — |
| M01-T042-A | M01 | Xây dựng bộ nghiệm thu xuyên chức năng M01 — lát A | M01-T009, T015, T018, T021, T027-A, T032, T037, T041 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02; REL-01, REL-02, REL-07 | — |
| M01-T043-A | M01 | Hoàn thiện tài liệu bàn giao M01 — lát A | M01-T042-A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01, A-G02 | — |
| A5-T001 | Điều phối | Đóng băng phạm vi nghiệm thu A | Lát 0–4 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01–A-G06 | — |
| A5-T002 | Điều phối | Kiểm tra bao phủ 145 task | A5-T001 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01–A-G06 | — |
| A5-T003 | Điều phối | Nghiệm thu A-G01 | Lát 2, 3D, 5A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01; REL-01, REL-07 | — |
| A5-T004 | Điều phối | Nghiệm thu A-G02 | Lát 2, 3A, 5A | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G02; REL-02, REL-07 | — |
| A5-T005 | Điều phối | Nghiệm thu A-G03 | Lát 3A–3C | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G03; REL-04 | — |
| A5-T006 | Điều phối | Nghiệm thu A-G04 | Lát 2, Lát 4 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G04; REL-03 | — |
| A5-T007 | Điều phối | Nghiệm thu A-G05 | Lát 1–4 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G05; REL-01, REL-03, REL-04 | — |
| A5-T008 | Điều phối | Nghiệm thu A-G06 | Lát 4 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G06; REL-02, REL-03 | — |
| A5-T009 | Điều phối | Rà soát đóng REL ảnh hưởng A | A5-T003–T008 | Chưa bắt đầu | Đầu ra đạt; tự kiểm REL-01–REL-04, REL-07 | — |
| A5-T010 | Điều phối | Ra quyết định Cổng A | A5-T002–T009 | Chưa bắt đầu | Đầu ra đạt; tự kiểm A-G01–A-G06 | — |
