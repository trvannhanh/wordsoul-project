# CT-01 — Đóng công khai nội dung chưa duyệt

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T006 |
| Trạng thái | Có hiệu lực từ 2026-08-18; triển khai kỹ thuật và bằng chứng runtime chờ các task liên quan |
| Dependency | A0-T004 đã hoàn thành; REL-04 đã mở và chưa đóng |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Phạm vi áp dụng | Nội dung học liệu/tài sản do người dùng, AI, import, batch hoặc quản trị tạo trong Giai đoạn A |
| Rủi ro kiểm soát | Nội dung người dùng hoặc AI tạo được công khai khi chưa có duyệt, quyền tài sản và khả năng thu hồi |

## Quy tắc tạm thời

- Nội dung người dùng hoặc AI tạo không được chuyển sang trạng thái công khai.
- Không có ngoại lệ theo vai trò, nguồn nội dung hoặc môi trường phát hành.
- Nội dung có thể tồn tại ở phạm vi riêng tư phục vụ soạn thảo nếu quyền truy cập được giới hạn.
- Mọi nỗ lực thay đổi trạng thái phải có thể truy vết và được kiểm tra định kỳ.
- Chỉ phiên bản đã qua luồng gửi duyệt, checklist chất lượng và kiểm tra quyền tài sản mới đủ điều kiện được xem xét công khai; CT-01 không tự cấp quyền công khai.
- Khi trạng thái duyệt hoặc quyền tài sản không chắc chắn, hành vi mặc định là từ chối công khai và giữ trạng thái an toàn hiện tại.

## Ma trận áp dụng

| Đường tạo/thay đổi | Trạng thái được phép trước duyệt | Yêu cầu khi thử công khai | Kết quả bắt buộc |
|---|---|---|---|
| Người dùng tạo/sửa | Nháp hoặc riêng tư | Phiên bản gửi duyệt, checklist và quyền tài sản phù hợp | Từ chối nếu thiếu bất kỳ điều kiện nào |
| AI sinh hoặc hỗ trợ sinh | Nháp hoặc riêng tư | Cùng điều kiện như nội dung người dùng; không tin nguồn AI | Không tự công khai hoặc tự duyệt |
| Import/batch/job nền | Nháp, tạm hoặc chờ duyệt | Không được bỏ qua state machine hay quyền | Từ chối/giữ an toàn và ghi kết quả theo lô |
| Quản trị/API nội bộ | Theo quyền tối thiểu và trạng thái vòng đời | Không có bypass theo vai trò; thao tác phải truy vết | Từ chối mặc định khi thiếu quyền, duyệt hoặc lý do |
| Tài sản thiếu/lỗi/bị thu hồi | Tạm, từ chối, thu hồi hoặc placeholder | Phải theo kết luận REL-04 và vòng đời tài sản | Không công khai tài sản thiếu/không rõ quyền |

## Tự kiểm A-G03

| Case | Nội dung tự kiểm CT-01 | Kết quả yêu cầu | Trạng thái |
|---|---|---|---|
| G03-C02 | Nội dung thiếu trường/checklist bắt buộc | Giữ nháp hoặc từ chối; không công khai | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G03-C03 | Người tạo hoặc vai trò bất kỳ thử tự công khai | Bị chặn; phải qua phiên bản gửi duyệt | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G03-C04 | Sửa sau khi gửi duyệt | Tạo phiên bản mới; bản đang duyệt không đổi | Tiêu chí đã ghi; chờ thiết kế phiên bản |
| G03-C07 | Tài sản thiếu/lỗi/bị thu hồi | Không báo hoàn chỉnh giả; placeholder hoặc suy giảm đã duyệt | Tiêu chí đã ghi; chờ REL-04 và bằng chứng runtime |
| G03-C08 | Nội dung công khai bị báo cáo nghiêm trọng | Tạm ẩn, điều tra và thu hồi theo SLA | Tiêu chí đã ghi; chờ quy trình và diễn tập |

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh mục nội dung công khai theo nguồn; kết quả kiểm tra đường thay đổi trạng thái; sai lệch và xử lý |
| Tần suất/điểm kiểm tra | Khi thay đổi state machine/quyền/import; trước A-G03; và sau sự cố công khai sai |
| Khi vi phạm | Tạm ẩn ngay, giữ dấu vết, đánh giá phạm vi ảnh hưởng và chuyển cấp chủ sản phẩm |
| Điều kiện gỡ | A-G03 đạt; gửi duyệt, checklist, báo cáo, thu hồi và quyền tài sản đã được nghiệm thu; không còn finding nghiêm trọng/rất cao liên quan |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng cập nhật task/quyết định phù hợp; đầu vào pháp lý vẫn bắt buộc khi liên quan quyền tài sản |

## Lịch sử

| Ngày | Người cập nhật | Thay đổi | Bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-01 theo workflow một người; chốt phạm vi, deny-by-default, ma trận áp dụng và tự kiểm A-G03 | A0-T004 hoàn thành; REL-04 đã mở; chưa có bằng chứng runtime |
