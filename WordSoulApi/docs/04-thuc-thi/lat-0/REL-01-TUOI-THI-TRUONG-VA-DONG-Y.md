# REL-01 — Tuổi, thị trường và đồng ý

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T001 |
| Trạng thái | Đã mở — đang chờ chốt thị trường, tuổi và mô hình đồng ý |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Hạn phản hồi | Trước khi nghiệm thu A-G01 và đóng phạm vi phát hành Giai đoạn A |
| Phạm vi phát hành dự kiến | Chưa chốt; là câu hỏi mở trong `DECISIONS.md` |
| Nơi lưu artifact | `docs/04-thuc-thi/lat-0/` và sổ bằng chứng Cổng A; không lưu PII hoặc payload thô |
| Chặn | A-G01; phát hành đầy đủ Giai đoạn A/B |

## Phạm vi

Xác định điều kiện cung cấp sản phẩm cho người học từ cơ bản đến trung cấp ở các nhóm tuổi và thị trường dự kiến, bao gồm cách ghi nhận đồng ý, hành vi khi thiếu hoặc rút đồng ý và giới hạn cần áp dụng cho người chưa thành niên.

### Trong phạm vi

- Đăng ký trực tiếp và đăng nhập bằng danh tính ngoài khi có thể tạo tài khoản mới.
- Dữ liệu dùng để xác định thị trường/nhóm tuổi và bằng chứng đồng ý tương ứng.
- Trạng thái tài khoản, quyền truy cập và tác động khi thiếu, từ chối hoặc rút đồng ý.
- Phạm vi dữ liệu cần chuyển sang REL-07 và dữ liệu rời hệ thống cần phối hợp với REL-03.

### Ngoài phạm vi kết luận của WSA-7K2

- Danh sách thị trường phát hành chính thức, cách diễn giải luật hoặc ngưỡng tuổi hợp pháp.
- Chấp nhận rủi ro pháp lý/riêng tư, miễn trừ kiểm soát hoặc quyết định phát hành.
- Giá trị cấu hình cuối cùng khi chưa có văn bản xác nhận của đúng authority.

## Baseline sản phẩm cần thẩm định

| Nguồn | Baseline hiện hành | Cách sử dụng trong hồ sơ |
|---|---|---|
| M01-D001 | Sản phẩm dự kiến phục vụ mọi độ tuổi, thu nhóm tuổi/khu vực bằng tự khai và chưa xác minh/liên kết người giám hộ | Chỉ là đầu vào để pháp lý/sản phẩm thẩm định; không dùng làm kết luận phát hành |
| M01-D002 | Người chưa xác minh thư chỉ được truy cập phạm vi học giới hạn; các năng lực nhạy cảm bị khóa | Dùng khi xác định quyền theo trạng thái, không thay thế quyết định tuổi/đồng ý |
| M01-D018–D022 | Xuất dữ liệu tự phục vụ có xác minh lại; xóa qua hỗ trợ với thời gian chờ; xử lý dữ liệu theo loại | Dùng để nối REL-01 với REL-07 và yêu cầu lưu bằng chứng đồng ý |
| A-WP01 | Chưa có bằng chứng triển khai đầy đủ cho tuổi, quốc gia, đồng ý và vòng đời dữ liệu | Không coi hành vi hiện tại là bằng chứng chính sách đã đạt |

## Ma trận câu hỏi cần xác nhận

| Mã | Câu hỏi cần quyết định | Authority cần trả lời | Task/đầu ra bị ảnh hưởng | Trạng thái |
|---|---|---|---|---|
| REL01-Q01 | Thị trường phát hành đầu tiên và ngưỡng tuổi áp dụng cho từng thị trường là gì? | Chủ sản phẩm và pháp lý | M01-T002, M01-T007; A-G01 | Chờ xác nhận |
| REL01-Q02 | Cần xác minh tuổi hay chỉ tự khai nhóm tuổi/khu vực; dữ liệu nào được phép dùng? | Pháp lý, riêng tư và chủ M01 | M01-T003, M01-T005, M01-T033 | Chờ xác nhận |
| REL01-Q03 | Trường hợp nào cần đồng ý của người giám hộ và bằng chứng quan hệ/xác nhận ở mức nào? | Pháp lý và chủ sản phẩm | M01-T007; A-G01 | Chờ xác nhận |
| REL01-Q04 | Khi thiếu/từ chối/rút đồng ý, chức năng nào được phép, bị khóa hoặc phải dừng ngay? | Chủ sản phẩm, pháp lý và chủ M01 | M01-T002, M01-T006–T009; A-G01 | Chờ xác nhận |
| REL01-Q05 | Bản ghi đồng ý phải gồm trường nào, lưu bao lâu và ai được xem/xuất? | Riêng tư, pháp lý và chủ M01 | M01-T003, M01-T007, M01-T033–T034 | Chờ xác nhận |
| REL01-Q06 | Khi chính sách đổi phiên bản, đối tượng nào phải đồng ý lại và quyền thay đổi tại thời điểm nào? | Chủ sản phẩm, pháp lý và chủ M01 | M01-T002, M01-T007 | Chờ xác nhận |
| REL01-Q07 | Khi yêu cầu xóa/rút đồng ý, dữ liệu nào xóa, ẩn danh hoặc giữ có thời hạn? | Riêng tư, pháp lý và các chủ dữ liệu | A0-T005, M01-T035–T036; REL-07 | Chờ xác nhận |
| REL01-Q08 | Ai kết luận đạt, đạt có điều kiện hoặc không đạt cho từng phạm vi phát hành? | WSA-7K2 theo D-001 | A0-T001; biên bản Cổng A | Đã chốt: WSA-7K2 tự nghiệm thu; ý kiến pháp lý là đầu vào khi cần |

## Bằng chứng và tiêu chí đóng

| Evidence ID dự kiến | Bằng chứng | Chủ tạo | Người xác nhận | Kết quả yêu cầu | Trạng thái |
|---|---|---|---|---|---|
| A0-E001 | Ma trận thị trường–nhóm tuổi–loại đồng ý | WSA-7K2 | WSA-7K2 tự kiểm với đầu vào pháp lý phù hợp | Có kết luận cho mọi phạm vi phát hành | Chưa tạo |
| A0-E002 | Ma trận dữ liệu tuổi/khu vực/đồng ý và mục đích | WSA-7K2 | WSA-7K2 tự kiểm với đầu vào riêng tư/pháp lý phù hợp | Dữ liệu tối thiểu, quyền, nguồn và lưu giữ rõ ràng | Chưa tạo |
| A0-E003 | Kịch bản thiếu, từ chối, rút và đồng ý lại | WSA-7K2 | WSA-7K2 tự kiểm A-G01 | Trạng thái tài khoản, quyền và tác động phiên xác định | Chưa tạo |
| A0-E004 | Văn bản kết luận REL-01 | WSA-7K2 | WSA-7K2 | Kết luận, điều kiện áp dụng và task khắc phục rõ ràng | Chưa tạo |

Các mã trên chỉ là định danh dự kiến. Chỉ đăng ký vào sổ bằng chứng khi có artifact thật, phiên bản, người tạo và người xác nhận.

## Điều kiện mở và đóng hồ sơ

### Kết quả mở hồ sơ A0-T001

- WSA-7K2 là người thực hiện và tự nghiệm thu theo D-001.
- Phạm vi câu hỏi, task bị ảnh hưởng và đầu ra dự kiến A0-E001–A0-E004 đã được xác định.
- Hạn xử lý gắn với A-G01 và phạm vi phát hành Giai đoạn A; chưa áp đặt ngày giả định khi thị trường chưa chốt.
- Artifact được lưu trong cây tài liệu dự án với quy tắc không lưu PII, bí mật hoặc payload thô.

### Đủ điều kiện đưa REL-01 sang tự kiểm kết luận

- A0-E001–A0-E004 có artifact thật và đã đăng ký trong sổ bằng chứng.
- Tất cả REL01-Q01–Q08 có câu trả lời hoặc được chuyển thành finding/task có chủ.
- Các thay đổi bắt buộc đã đồng bộ sang quyết định/backlog/task phụ thuộc theo đúng thứ tự nguồn.

### Đủ điều kiện đóng REL-01

- Kết luận pháp lý/sản phẩm không còn mơ hồ cho phạm vi phát hành đã freeze.
- A-G01 tham chiếu đúng phiên bản kết luận và không còn finding nghiêm trọng/rất cao liên quan.
- WSA-7K2 tự kiểm và chấp nhận bằng chứng theo D-001; đầu vào pháp lý/sản phẩm bắt buộc phải có khi kết luận phụ thuộc vào luật hoặc phạm vi phát hành.

Hồ sơ chỉ đóng khi kết luận pháp lý/sản phẩm không còn mơ hồ và các điều chỉnh bắt buộc đã được chuyển thành task có chủ. Không dùng giả định mặc định “mọi độ tuổi đều được truy cập đầy đủ”.

## Phụ thuộc và bàn giao

- A0-T005 giữ `Chờ phụ thuộc` đến khi REL-01 có phạm vi dữ liệu/đồng ý sơ bộ được xác nhận.
- A0-T011 và M12-T042-A dùng đầu ra REL-01 như phụ thuộc nội bộ; không dựa vào bản nháp chưa được authority xác nhận.
- M01-T002 có thể soạn song song sau M01-T001, nhưng phần tuổi/đồng ý không được đóng trước REL-01.
- M01-T007 và M01-T033–M01-T036 giữ mở cho đến khi câu hỏi pháp lý/riêng tư liên quan được giải quyết.

## Lịch sử hồ sơ

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-15 | WSA-7K2 | Chuẩn bị baseline, ma trận REL01-Q01–Q08, kế hoạch A0-E001–A0-E004 và tiêu chí mở/đóng | Không có quyết định mới; chờ gán authority |
| 2026-08-18 | WSA-7K2 | Mở hồ sơ theo workflow một người; xác định chủ trì, hạn theo cổng, nơi lưu artifact và giữ rõ các câu hỏi chưa chốt | D-001; chưa tạo Evidence ID |
