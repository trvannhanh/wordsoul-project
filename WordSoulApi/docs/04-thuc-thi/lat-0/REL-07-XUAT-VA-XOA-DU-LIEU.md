# REL-07 — Xuất và xóa dữ liệu

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T005 |
| Trạng thái | Đã mở — đang chờ bản đồ dữ liệu, chính sách lưu giữ và hợp đồng xuất/xóa |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Hạn phản hồi | Trước khi nghiệm thu A-G01/A-G02 và phát hành đầy đủ Giai đoạn A |
| Đầu vào REL-01 | Hồ sơ REL-01 đã mở; câu hỏi tuổi, thị trường, đồng ý và lưu giữ liên quan vẫn chưa chốt |
| Nơi lưu artifact | Chỉ lưu schema, metadata tổng hợp và dữ liệu thử giả trong tài liệu/sổ bằng chứng; không lưu PII, token, nội dung export hoặc payload thô |
| Chặn | A-G01 và A-G02; phát hành đầy đủ |

## Phạm vi

Xác định hành trình yêu cầu, xác minh chủ thể, xuất, xóa hoặc ẩn danh hóa và đối soát dữ liệu trên toàn bộ module. Bao gồm yêu cầu tự phục vụ, yêu cầu qua hỗ trợ, chạy lại phần lỗi và bằng chứng hoàn tất.

### Trong phạm vi

- Yêu cầu tự phục vụ và yêu cầu qua hỗ trợ, gồm xác minh chủ thể và trường hợp mất kênh truy cập.
- Bản đồ dữ liệu liên module, chủ dữ liệu, nguồn thật, lưu giữ, xuất, xóa và ẩn danh hóa.
- Trạng thái yêu cầu, thời gian chờ, hủy, chạy nền, retry/idempotency, phần lỗi và đối soát.
- Manifest xuất, chuyển giao an toàn, bằng chứng xóa/ẩn danh và thông báo kết quả.

### Ranh giới an toàn

- Không dùng PII thật, nội dung export thật, token, liên kết tải có hạn hoặc payload module làm bằng chứng tài liệu.
- Không coi xóa bản ghi tài khoản chính là hoàn tất nếu dữ liệu dẫn xuất, cache, tài sản, log hoặc provider còn nhận diện được ngoài chính sách.
- Task mở hồ sơ không tự chốt căn cứ giữ dữ liệu, thời hạn pháp lý, thị trường hoặc mức xác minh chủ thể khi đầu vào chưa có.

## Ma trận câu hỏi cần xác nhận

| Mã | Câu hỏi | Task/đầu ra ảnh hưởng | Trạng thái |
|---|---|---|---|
| REL07-Q01 | Module/loại dữ liệu nào thuộc phạm vi, nguồn thật và chủ dữ liệu là gì? | M01-T033; A-G01 | Chờ bản đồ dữ liệu |
| REL07-Q02 | Mức xác minh chủ thể nào áp dụng cho tự phục vụ, hỗ trợ và trường hợp mất mọi kênh? | M01-T019, T021, T034–T035; A-G01/A-G02 | Chờ chính sách |
| REL07-Q03 | SLA, trạng thái yêu cầu, thời gian chờ/hủy, định dạng manifest và chuyển giao an toàn là gì? | M01-T034–T035; M11-T029, T038–T040-A | Chờ hợp đồng hành trình |
| REL07-Q04 | Với từng dataset, dữ liệu nào xóa, ẩn danh, giữ có thời hạn hoặc chịu hold; căn cứ là gì? | M01-T036; REL-01; A-G01 | Chờ ma trận lưu giữ |
| REL07-Q05 | Phần lỗi được retry/idempotent, đối soát và thông báo thế nào để không báo hoàn tất giả? | M11-T038–T040-A; A-G01/A-G02 | Chờ hợp đồng công việc nền |
| REL07-Q06 | Sau xóa, phiên, cache, tài sản, provider ngoài và quy tắc đăng ký lại được xử lý thế nào? | M01-T018, T025-A–T027-A, T037; A-G01 | Chờ hợp đồng xuyên module |
| REL07-Q07 | Ai tự kiểm kịch bản xuất/xóa, quyền hỗ trợ, audit và đối soát? | A-G01/A-G02 | Đã chốt: WSA-7K2 theo D-001 |

## Kế hoạch đầu ra và tự kiểm

| Artifact | Nội dung tối thiểu | Tự kiểm | Trạng thái |
|---|---|---|---|
| Bản đồ dữ liệu liên module | Dataset, nguồn thật, chủ, mục đích, lưu giữ, export, delete/anonymize và phụ thuộc | A-G01 | Chưa tạo |
| Hợp đồng yêu cầu xuất/xóa | Kênh, xác minh, trạng thái, SLA, thời gian chờ/hủy, idempotency và audit | A-G01/A-G02 | Chưa tạo |
| Manifest xuất và chuyển giao an toàn | Schema/version, phạm vi, phần thành công/lỗi, checksum/expiry ở mức thiết kế; không chứa dữ liệu thật | A-G01 | Chưa tạo |
| Ma trận xóa/ẩn danh/giữ | Quy tắc từng dataset, cache/tài sản/provider, hold, bằng chứng và đối soát | A-G01/A-G02 | Chưa tạo |
| Kết quả kịch bản xuyên module | Xác minh, xuất, xóa, phần lỗi, chạy lại, đăng ký lại và quyền hỗ trợ bằng dữ liệu giả | A-G01/A-G02 | Chưa tạo |

## Bằng chứng và tiêu chí đóng

### Kết quả mở hồ sơ A0-T005

- Dependency A0-T001 đã hoàn thành; các câu hỏi REL-01 chưa chốt được giữ như đầu vào mở.
- WSA-7K2 là người thực hiện và tự nghiệm thu theo D-001.
- Phạm vi, câu hỏi REL07-Q01–Q07, đầu ra cần tạo và liên kết A-G01/A-G02 đã được xác định.
- Hạn xử lý gắn với hai cổng và phát hành đầy đủ Giai đoạn A; ranh giới không lưu dữ liệu người dùng trong bằng chứng đã được ghi rõ.

### Điều kiện đưa REL-07 sang tự kiểm kết luận và đóng

- REL07-Q01–Q07 có câu trả lời hoặc được chuyển thành finding/task có hành động rõ ràng.
- Bản đồ dữ liệu bao phủ mọi module trong phạm vi và có quy tắc export/delete/anonymize/retain truy vết được.
- Kịch bản xuyên module chứng minh xác minh chủ thể, manifest xuất, chuyển giao, xóa/ẩn danh, phần lỗi, chạy lại và đối soát bằng dữ liệu giả.
- Xóa/khóa thu hồi phiên phù hợp; dữ liệu dẫn xuất, cache, tài sản và provider ngoài được xử lý đúng ma trận.
- Thao tác hỗ trợ tuân thủ quyền tối thiểu, xác minh lại, lý do/vụ việc và audit; không báo hoàn tất giả khi còn phần lỗi.
- WSA-7K2 tự kết luận A-G01/A-G02 theo D-001; mọi finding nghiêm trọng/rất cao đã được xử lý và kiểm tra lại.

## Phụ thuộc

- REL-01 cung cấp đầu vào tuổi/thị trường/đồng ý và lưu giữ; hồ sơ đã mở nhưng kết luận liên quan vẫn phải chờ câu trả lời thực tế.
- REL-02 cung cấp quyền và audit cho thao tác hỗ trợ; REL-03 cung cấp bản đồ dữ liệu/provider ngoài khi có dữ liệu rời hệ thống.
- M01-T033–T037 tạo bản đồ và hành trình nghiệp vụ; M11-T027–T035, T038–T040-A tạo support case, audit, công việc nền và đối soát.

## Lịch sử hồ sơ

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Mở hồ sơ theo workflow một người; xác định phạm vi, câu hỏi, kế hoạch đầu ra, hạn theo cổng và ranh giới dữ liệu cá nhân | D-001, D-008; chưa tạo Evidence ID |
