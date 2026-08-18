# REL-04 — Quyền sử dụng tài sản

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T004 |
| Trạng thái | Đã mở — đang chờ kiểm kê nguồn, quyền và phạm vi sử dụng tài sản |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Hạn phản hồi | Trước khi nghiệm thu A-G03/A-G05 và công khai tài sản thuộc phạm vi Giai đoạn A |
| Phạm vi tài sản ban đầu | Tài sản học liệu, hồ sơ và gamification thuộc Giai đoạn A; từng loại/nguồn phải được kiểm kê trước khi kết luận |
| Nơi lưu artifact | Registry và tài liệu kết luận trong dự án, chỉ dùng metadata/định danh an toàn; không sao chép tài sản hoặc dữ liệu cấp phép nhạy cảm vào bằng chứng |
| Chặn | A-G03 và A-G05 đối với tài sản thiếu quyền rõ |

## Phạm vi

Xác định loại tài sản được phép sử dụng cho nội dung học tập, hồ sơ và trải nghiệm gamification; bằng chứng nguồn/quyền cần có; cách xử lý tài sản chưa rõ quyền, khiếu nại, tạm ẩn và gỡ nhanh.

### Trong phạm vi

- Ảnh, âm thanh, phát âm, tệp tải lên và tài sản hiển thị trong học liệu, hồ sơ hoặc gamification.
- Nguồn, chủ thể quyền, giấy phép/điều khoản, attribution, phạm vi sử dụng và phân phối lại.
- Trạng thái đủ/thiếu/không rõ bằng chứng quyền và hành vi hiển thị tương ứng.
- Khiếu nại, tạm ẩn, gỡ, thay thế, tham chiếu phiên bản và lịch sử liên quan.

### Ranh giới an toàn

- “Đã tìm thấy trên Internet”, có URL công khai hoặc đã tải được không phải bằng chứng quyền sử dụng.
- Tài sản thiếu/không rõ quyền không được tự chuyển thành đủ điều kiện công khai; phải giữ ẩn, dùng placeholder hoặc xử lý theo kết luận REL-04.
- Task mở hồ sơ không tự tạo kết luận pháp lý, chấp nhận rủi ro hoặc quyền sử dụng cho bất kỳ tài sản cụ thể nào.

## Ma trận câu hỏi cần xác nhận

| Mã | Câu hỏi | Task/đầu ra ảnh hưởng | Trạng thái |
|---|---|---|---|
| REL04-Q01 | Những loại tài sản và nguồn nào nằm trong phạm vi Giai đoạn A? | M02-T011; M12-T021; A-G03/A-G05 | Chờ kiểm kê |
| REL04-Q02 | Bằng chứng nguồn, chủ thể quyền, license/điều khoản và attribution tối thiểu cho từng loại là gì? | M02-T011–T012; M12-T023 | Chờ kết luận sản phẩm/pháp lý |
| REL04-Q03 | Tài sản thiếu hoặc không rõ quyền phải giữ ẩn, dùng placeholder hay bị loại trong trường hợp nào? | M02-T013–T014; G03-C07 | Chờ ma trận xử lý |
| REL04-Q04 | Ai tiếp nhận khiếu nại, ai được tạm ẩn/gỡ và SLA theo mức độ là gì? | M02-T033–T034; A-G03 | Chờ quy trình |
| REL04-Q05 | Khi thay thế/gỡ tài sản, phiên bản học liệu, phiên đang chạy, cache/CDN và lịch sử phải được xử lý thế nào? | M02-T014; M12-T024–T025; A-G03/A-G05 | Chờ hợp đồng vòng đời |
| REL04-Q06 | Ai tự kiểm registry quyền, quy trình gỡ và diễn tập? | A-G03/A-G05 | Đã chốt: WSA-7K2 theo D-001 |

## Kế hoạch đầu ra và tự kiểm

| Artifact | Nội dung tối thiểu | Tự kiểm | Trạng thái |
|---|---|---|---|
| Danh mục tài sản trong phạm vi | Loại, module sở hữu, mục đích, nguồn và trạng thái sử dụng | A-G03/A-G05 | Chưa tạo |
| Ma trận quyền và nguồn | Chủ thể quyền, license/điều khoản, attribution, phạm vi dùng/phân phối và trạng thái bằng chứng | A-G05 | Chưa tạo |
| Kết luận sản phẩm/pháp lý theo phạm vi | Phạm vi được phép, điều kiện, ngoại lệ bị cấm và task khắc phục | REL-04/A-G03 | Chưa tạo |
| Quy trình khiếu nại–tạm ẩn–gỡ | Kênh tiếp nhận, phân loại, quyền thao tác, SLA, audit, thông báo và phục hồi | A-G03 | Chưa tạo |
| Kết quả diễn tập gỡ/thay thế | Tác động version, tham chiếu, cache/phân phối, phiên đang chạy và đối soát | A-G03/A-G05 | Chưa tạo |

## Bằng chứng và tiêu chí đóng

### Kết quả mở hồ sơ A0-T004

- WSA-7K2 là người thực hiện và tự nghiệm thu theo D-001.
- Phạm vi, câu hỏi REL04-Q01–Q06, đầu ra cần tạo và liên kết A-G03/A-G05 đã được xác định.
- Hạn xử lý gắn với hai cổng và việc công khai tài sản trong Giai đoạn A.
- Ranh giới không suy diễn quyền từ khả năng truy cập hoặc nguồn Internet đã được ghi rõ.

### Điều kiện đưa REL-04 sang tự kiểm kết luận và đóng

- REL04-Q01–Q06 có câu trả lời hoặc được chuyển thành finding/task có hành động rõ ràng.
- Danh mục tài sản, ma trận quyền, kết luận sản phẩm/pháp lý, quy trình khiếu nại–gỡ và kết quả diễn tập có artifact truy vết được.
- Mọi tài sản công khai trong phạm vi có bằng chứng quyền phù hợp hoặc kết luận chấp nhận rủi ro rõ cho đúng phạm vi.
- Tài sản thiếu/không rõ quyền bị giữ ẩn, thay thế hoặc gỡ; không có đường tự công khai.
- Diễn tập chứng minh tạm ẩn/gỡ/thay thế không làm mất lịch sử, phá tham chiếu hoặc để cache/phân phối tiếp tục ngoài chính sách.
- WSA-7K2 tự kết luận A-G03/A-G05 theo D-001; mọi finding nghiêm trọng/rất cao đã được xử lý và kiểm tra lại.

## Phụ thuộc

- A0-T006 chỉ dùng REL-04 sau khi hồ sơ đã được mở; việc mở hồ sơ không đồng nghĩa CT-01 hoặc các cổng đã đạt.
- M02-T011–T014 và M12-T021–T025 tạo catalog/vòng đời chi tiết; M02-T029–T034 tạo luồng duyệt, báo cáo, thu hồi và khiếu nại.
- REL-03 xử lý bí mật/dữ liệu ngoài và không được dùng để suy diễn quyền tài sản; REL-04 cung cấp trạng thái quyền cho A-G05.

## Lịch sử hồ sơ

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Mở hồ sơ theo workflow một người; xác định phạm vi, câu hỏi, kế hoạch đầu ra, hạn theo cổng và ranh giới quyền tài sản | D-001; chưa tạo Evidence ID |
