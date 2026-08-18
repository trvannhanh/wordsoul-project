# CT-03 — Không dùng payload thô làm bằng chứng

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T008 |
| Trạng thái | Có hiệu lực từ 2026-08-18; triển khai redaction và bằng chứng runtime chờ các task liên quan |
| Dependency | A0-T003 đã hoàn thành; REL-03 đã mở và chưa đóng |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Phạm vi áp dụng | Log, audit, trace, ảnh chụp, export chẩn đoán, ticket và artifact bằng chứng của API, client, job nền và provider ngoài |
| Rủi ro kiểm soát | Token, bí mật hoặc dữ liệu cá nhân xuất hiện trong log và hồ sơ bằng chứng |

## Quy tắc tạm thời

- Payload hoặc phản hồi thô không được coi là bằng chứng vận hành hợp lệ.
- Không sao chép dữ liệu thô vào tài liệu, công cụ quản lý công việc hoặc kênh trao đổi chung.
- Chỉ dùng dữ liệu giả, trường đã cho phép và giá trị đã che khi cần minh họa.
- Nguồn log hiện hữu có rủi ro phải được giới hạn quyền xem và có kế hoạch thay thế.
- Không dùng việc băm/mã hóa giá trị nhạy cảm như cách mặc định để biến payload thành bằng chứng; chỉ giữ metadata tối thiểu có mục đích rõ.
- Khi cần xem dữ liệu thô để xử lý sự cố, việc xem chỉ diễn ra trong công cụ runtime được kiểm soát quyền/lưu giữ; artifact kết quả vẫn phải được rút gọn và che trước khi dùng làm bằng chứng.

## Allowlist bằng chứng

| Nguồn | Metadata được phép ở mức tối thiểu | Bắt buộc loại/che |
|---|---|---|
| Đăng ký/đăng nhập/OAuth | Correlation ID nội bộ, provider, loại sự kiện, kết quả, error category và thời điểm | Mật khẩu, token, code, state nhạy cảm, email đầy đủ, header/cookie và redirect payload |
| Quản trị/audit | Event ID, actor ID nội bộ tối thiểu, vai trò/phạm vi, action, object ID tối thiểu, kết quả, lý do/vụ việc và thời điểm | Request/response body, secret, PII ngoài allowlist và trạng thái trước–sau chưa redaction |
| Tích hợp/provider | Năng lực/provider, status/error category, latency, retry count, schema/version và correlation | Body/response thô, auth header, credential, prompt/nội dung người dùng và PII |
| Upload/tài sản | Asset ID nội bộ, loại, kích thước phân loại, kết quả scan/xử lý và correlation | Nội dung tệp, URL riêng tư có hạn, tên tệp/metadata nhạy cảm và binary dump |
| Xuất/xóa dữ liệu | Request ID, trạng thái, số lượng tổng hợp, phần thành công/lỗi và thời điểm | Nội dung export, dữ liệu chủ thể, link tải, khóa/checksum nhạy cảm và payload module |

## Tự kiểm A-G02/A-G05

| Phạm vi kiểm tra | Kết quả yêu cầu | Trạng thái |
|---|---|---|
| Audit trước–sau A-G02 | Chỉ metadata allowlist; tác nhân/kết quả truy vết được mà không chứa payload/bí mật ngoài phạm vi | Tiêu chí đã ghi; chờ M11-T031–T035 và bằng chứng runtime |
| G05-L01 đăng ký/đăng nhập | Mật khẩu, token, mã xác minh và email đầy đủ bị loại/che | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G05-L02 OAuth callback | Code, state nhạy cảm, token và redirect payload bị loại/che | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G05-L03 lỗi provider | Body/response thô, PII và secret bị loại; chỉ giữ status/error category | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G05-L04 upload/tài sản | Nội dung tệp, URL riêng tư và metadata nhạy cảm bị loại/che | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G05-L05 xuất/xóa | Nội dung xuất và dữ liệu chủ thể bị loại; chỉ giữ trạng thái/số lượng tổng hợp | Tiêu chí đã ghi; chờ bằng chứng runtime |

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh sách nguồn log, mức nhạy cảm, quyền truy cập, mẫu đã che và kết quả rà soát |
| Nhịp kiểm tra | Khi thêm/sửa log, audit hoặc provider; trước A-G02/A-G05; và sau sự cố lộ dữ liệu/bí mật |
| Khi vi phạm | Hạn chế truy cập, loại bằng chứng, xử lý dữ liệu lộ và đánh giá nhu cầu xoay vòng bí mật |
| Điều kiện gỡ | M11-T031–M11-T035, M12-T040–T041, M12-T042-A và M12-T043 đạt |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng cập nhật task/quyết định phù hợp sau khi điều kiện gỡ và A-G02/A-G05 đạt |

## Kết quả kích hoạt và điều kiện gỡ

- WSA-7K2 là người duy trì và tự nghiệm thu theo D-001; phạm vi nguồn, allowlist và dữ liệu bắt buộc loại/che đã được ghi nhận.
- CT-03 có hiệu lực ở mức guardrail dự án; không diễn giải điều này thành bằng chứng log/redaction hiện tại đã đạt.
- Chỉ gỡ sau khi M11-T031–T035, M12-T040–T041, M12-T042-A, M12-T043 và các kiểm thử A-G02/G05-L01–L05 đạt, không còn finding nghiêm trọng/rất cao liên quan.
- Nếu phát hiện payload/bí mật/PII trong artifact, artifact đó không hợp lệ và phải được hạn chế truy cập, loại bỏ an toàn, đánh giá phạm vi và xử lý credential/dữ liệu liên quan.

## Lịch sử

| Ngày | Người cập nhật | Thay đổi | Bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-03 theo workflow một người; chốt phạm vi, allowlist metadata, tự kiểm A-G02/A-G05 và điều kiện xử lý vi phạm | A0-T003 hoàn thành; REL-02/REL-03 đã mở; chưa có bằng chứng runtime |
