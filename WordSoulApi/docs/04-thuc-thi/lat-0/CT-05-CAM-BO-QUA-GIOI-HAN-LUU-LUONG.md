# CT-05 — Cấm bỏ qua giới hạn lưu lượng

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T010 |
| Trạng thái | Có hiệu lực từ 2026-08-18; M12-T034/T035 đã hoàn thành baseline thiết kế ngày 2026-08-20, bằng chứng runtime vẫn chờ |
| Dependency | A0-T003 đã hoàn thành; REL-03 đã mở và chưa đóng |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Phạm vi áp dụng | API công khai/nội bộ, callback provider, tác vụ quản trị, batch/job và mọi năng lực có quota/chi phí/rủi ro lạm dụng trong Giai đoạn A |
| Rủi ro kiểm soát | Nguồn gọi tự nhận trusted/internal để bỏ qua giới hạn và gây lạm dụng hoặc quá tải |

## Quy tắc tạm thời

- Không nguồn gọi nào được miễn giới hạn chỉ dựa trên dấu hiệu do yêu cầu cung cấp.
- Hành trình nội bộ và bên ngoài đều phải có chính sách giới hạn được xác định rõ.
- Khi không xác minh được trạng thái giới hạn, hành vi phải suy giảm an toàn theo mức quan trọng.
- Mọi thay đổi chính sách phải có chủ, lý do, phạm vi, thời hạn và audit.
- Header, query, body, claim chưa được trust boundary xác minh hoặc nhãn `trusted/internal` do caller cung cấp không được tạo bypass hay bucket đặc quyền.
- Auth, chống gian lận, thao tác chi phí cao và mutation nhạy cảm không được mặc định `allow-all` khi Redis/limiter/shared state lỗi.

## Ma trận áp dụng

| Nguồn gọi | Căn cứ định danh/phạm vi đáng tin | Chính sách bắt buộc | Khi limiter/shared state lỗi |
|---|---|---|---|
| Chưa xác thực/công khai | Dữ liệu kết nối và định danh do server xác lập; không tin nhãn caller | Bucket/giới hạn theo endpoint và rủi ro; phản hồi không làm lộ thông tin nhạy cảm | Từ chối hoặc fallback cục bộ bảo thủ có cap rõ |
| Người dùng đã xác thực | Subject/session đã xác minh cùng account state; không dùng email/raw token làm key log | Giới hạn theo subject, hành trình và tài nguyên; chống chia nhỏ qua nhiều entry point | Auth/mutation nhạy cảm fail-closed hoặc giảm mức an toàn đã chốt |
| Service nội bộ/job/batch | Workload identity/mTLS hoặc cơ chế server-side tương đương đã xác minh | Có quota/budget riêng; không miễn hoàn toàn; retry phải có cap/backoff | Dừng, trì hoãn hoặc chạy giới hạn; không tạo bão retry |
| Quản trị/hỗ trợ | Phiên quản trị, quyền/phạm vi, re-auth và vụ việc hợp lệ | Giới hạn theo hành động nhạy cảm, actor và đối tượng; thay đổi chính sách có audit | Từ chối thao tác nhạy cảm nếu không bảo đảm kiểm soát |
| Callback/provider | Chữ ký/credential và contract provider được xác minh tại biên | Giới hạn theo provider/năng lực và chống replay; không tin source header đơn lẻ | Cô lập/buffer có giới hạn hoặc từ chối theo failure mode đã chốt |

## Tự kiểm A-G04

| Phạm vi kiểm tra | Kết quả yêu cầu | Trạng thái |
|---|---|---|
| Registry năng lực A-G04 | Mỗi năng lực có chủ, nguồn gọi, hạn mức, budget, failure mode, suy giảm và metric/cảnh báo | Baseline M12-RATE-1.0/M12-FAIL-1.0 đã có; chờ enforcement, metric/cảnh báo và registry runtime |
| Giả mạo dấu hiệu trusted/internal | Header/claim/nhãn do caller cung cấp không làm đổi bucket hoặc bỏ qua giới hạn | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G04-R02 | 429/hết quota không gây retry vô hạn, ghi lặp hoặc chuyển sang đường miễn | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G04-R05 | Redis/shared state lỗi không mặc định allow-all; failure mode đúng criticality và không ghi đôi | Tiêu chí đã ghi; chờ bằng chứng runtime |
| Thay đổi chính sách | Có chủ, lý do, scope, version/hiệu lực, rollback và audit; không có bypass vô thời hạn | Tiêu chí đã ghi; chờ hợp đồng cấu hình/audit |

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh mục nguồn gọi, chính sách áp dụng, kịch bản giả mạo dấu hiệu và kết quả từ chối |
| Nhịp kiểm tra | Khi thêm/sửa entry point, workload identity, limiter hoặc policy; trước A-G04; và sau sự cố quota/lạm dụng/quá tải |
| Khi vi phạm | Dừng đường miễn, giới hạn nguồn ảnh hưởng, điều tra lưu lượng và đánh giá dữ liệu bị tác động |
| Điều kiện gỡ | M12-T034–M12-T035 đạt |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng cập nhật task/quyết định phù hợp sau khi điều kiện gỡ và A-G04 đạt |

## Kết quả kích hoạt và điều kiện gỡ

- WSA-7K2 là người duy trì và tự nghiệm thu theo D-001; nguồn gọi, trust boundary, failure mode và tự kiểm A-G04 đã được ghi nhận.
- CT-05 có hiệu lực ở mức guardrail dự án; không diễn giải điều này thành limiter/runtime hiện tại đã đạt.
- Chỉ gỡ sau khi M12-T034, M12-T035, kiểm thử giả mạo nguồn gọi, G04-R02 và G04-R05 đạt; không còn finding nghiêm trọng/rất cao về bypass hoặc `allow-all` khi lỗi.

## Lịch sử

| Ngày | Người cập nhật | Thay đổi | Bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-05 theo workflow một người; chốt trust boundary, failure mode, ma trận nguồn gọi và tự kiểm A-G04 | A0-T003 hoàn thành; REL-03 đã mở; chưa có bằng chứng runtime |
| 2026-08-20 | WSA-7K2 | Ghi nhận M12-T034/T035 hoàn thành baseline rate/fail-mode; tiếp tục giữ CT-05 vì G04-R02/R05 và runtime coverage chưa đạt | M12-RATE-1.0; M12-FAIL-1.0; D-024–D-025 |
