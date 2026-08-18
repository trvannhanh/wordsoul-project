# REL-03 — Bí mật và tích hợp

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T003 |
| Trạng thái | Đã mở — đang chờ kiểm kê tích hợp, bí mật và dữ liệu ngoài |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Hạn phản hồi | Trước khi nghiệm thu A-G04–A-G06 và đóng phạm vi phát hành Giai đoạn A |
| Phạm vi provider ban đầu | Chưa chốt; là câu hỏi mở trong `DECISIONS.md` và phải được xác định trước khi đóng REL-03 |
| Nơi lưu artifact | Tài liệu registry/bản đồ trong dự án và sổ bằng chứng Cổng A; tuyệt đối không lưu giá trị bí mật, PII hoặc payload thô |
| Chặn | A-G04, A-G05 và A-G06 |

## Phạm vi

Kiểm kê năng lực tích hợp và bí mật liên quan, xác định chủ sở hữu, dữ liệu đi ra ngoài, hạn mức, failure mode, phương án suy giảm và xử lý mọi bí mật nghi lộ. Hồ sơ tuyệt đối không lưu giá trị bí mật.

### Trong phạm vi

- Năng lực/provider đang hoạt động trong Giai đoạn A, module tiêu thụ, mục đích và mức quan trọng.
- Loại kho bí mật, workload được phép, vòng đời, mức phơi lộ và hành động thu hồi/xoay vòng; không ghi giá trị.
- Dữ liệu rời hệ thống, mục đích, tối thiểu hóa, lưu giữ/xóa và metadata log được phép.
- Timeout, quota, retry, limiter, circuit/bulkhead, kill switch, health thật và hành vi suy giảm.

### Ranh giới an toàn

- Không đưa secret, token, PII thật, response provider hoặc payload thô vào hồ sơ hay bằng chứng.
- Không coi provider là đang hoạt động, bí mật là an toàn hoặc kiểm thử là đạt khi chưa có artifact kiểm chứng.
- Không tự mở AI/giọng nói hoặc năng lực provider chưa được quyết định cho Giai đoạn A.

## Ma trận câu hỏi cần xác nhận

| Mã | Câu hỏi | Task/đầu ra ảnh hưởng | Trạng thái |
|---|---|---|---|
| REL03-Q01 | Những năng lực/provider nào thực sự hoạt động trong Giai đoạn A, với mục đích, module dùng và mức quan trọng nào? | M12-T002–T003; A-G04 | Chờ kiểm kê |
| REL03-Q02 | Mỗi loại bí mật nằm trong loại kho nào, workload nào được phép dùng, chủ và chu kỳ xoay vòng là gì? | M12-T040–T041; A-G05 | Chờ kiểm kê; không ghi giá trị |
| REL03-Q03 | Bí mật nào nghi lộ hoặc không còn cần thiết và phải thu hồi, xoay vòng hay thay thế? | M12-T041; A-G05/A-G06 | Chờ kiểm kê an toàn |
| REL03-Q04 | Dữ liệu nào rời hệ thống, căn cứ/mục đích, tối thiểu hóa, lưu giữ/xóa và log allowlist là gì? | M12-T042-A–T043; A-G05 | Chờ bản đồ dữ liệu |
| REL03-Q05 | Timeout, quota, outage, kết quả không chắc chắn và lỗi shared state phải fail/suy giảm thế nào? | M12-T034–T038, T045–T047-A; A-G04/A-G06 | Chờ hợp đồng năng lực |
| REL03-Q06 | Ai tự kiểm registry, redaction, xoay vòng và diễn tập suy giảm? | A-G04–A-G06 | Đã chốt: WSA-7K2 theo D-001 |

## Kế hoạch đầu ra và tự kiểm

| Artifact | Nội dung tối thiểu | Tự kiểm | Trạng thái |
|---|---|---|---|
| Registry năng lực tích hợp | Chủ, mục đích, module dùng, dữ liệu tối thiểu, criticality, hạn mức, failure mode, health và kill switch | A-G04 | Chưa tạo |
| Inventory bí mật không chứa giá trị | Secret ID nội bộ, loại kho, workload, môi trường, chu kỳ xoay, mức phơi lộ và hành động khẩn | A-G05 | Chưa tạo |
| Bản đồ dữ liệu rời hệ thống và log allowlist | Trường/nhóm dữ liệu, mục đích, provider, tối thiểu hóa, lưu giữ/xóa và metadata log | A-G05 | Chưa tạo |
| Báo cáo kiểm thử suy giảm và ứng phó | Timeout, quota, outage, limiter, shared state, circuit, kill switch, phục hồi và đối soát | A-G04/A-G06 | Chưa tạo |
| Kết quả thu hồi/xoay vòng khi cần | Chỉ lưu metadata kiểm chứng, thời điểm và kết quả; không lưu credential cũ/mới | A-G05/A-G06 | Chưa tạo |

## Bằng chứng và tiêu chí đóng

### Kết quả mở hồ sơ A0-T003

- WSA-7K2 là người thực hiện và tự nghiệm thu theo D-001.
- Phạm vi, câu hỏi REL03-Q01–Q06, đầu ra cần tạo và liên kết A-G04–A-G06 đã được xác định.
- Hạn xử lý gắn với ba cổng và phạm vi phát hành Giai đoạn A.
- Quy tắc không lưu bí mật, PII hoặc payload thô được áp dụng cho mọi artifact.

### Điều kiện đưa REL-03 sang tự kiểm kết luận và đóng

- REL03-Q01–Q06 có câu trả lời hoặc được chuyển thành finding/task có hành động rõ ràng.
- Registry tích hợp, inventory bí mật, bản đồ dữ liệu ngoài và các kết quả kiểm thử cần thiết có artifact truy vết được.
- Bí mật nghi lộ đã được thu hồi/xoay vòng hoặc năng lực liên quan bị tắt; không còn tích hợp không chủ.
- Kiểm thử timeout, quota, outage, limiter, shared state và kill switch đạt cho phạm vi trọng yếu.
- Không có đường bỏ qua giới hạn lưu lượng, thành công giả, payload thô hoặc giá trị bí mật trong tài liệu/log/bằng chứng.
- WSA-7K2 tự kết luận A-G04–A-G06 theo D-001; mọi finding nghiêm trọng/rất cao đã được xử lý và kiểm tra lại.

## Phụ thuộc

- A0-T008, A0-T010 và A0-T011 chỉ dùng REL-03 sau khi hồ sơ đã được mở; việc mở hồ sơ không đồng nghĩa các CT hoặc cổng đã đạt.
- REL-01 cung cấp căn cứ tuổi/đồng ý khi dữ liệu tương ứng rời hệ thống; REL-04 xử lý quyền tài sản, không được REL-03 tự kết luận thay.
- Các task M12/M11 tạo registry, policy và kiểm thử chi tiết; REL-03 tổng hợp kết quả để tự kiểm cổng.

## Lịch sử hồ sơ

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Mở hồ sơ theo workflow một người; xác định phạm vi, câu hỏi, kế hoạch đầu ra, hạn theo cổng và ranh giới dữ liệu nhạy cảm | D-001, D-008; chưa tạo Evidence ID |
