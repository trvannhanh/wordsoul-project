# REL-02 — Quyền và audit bù trừ

| Trường | Nội dung khởi tạo |
|---|---|
| Task mở hồ sơ | A0-T002 |
| Trạng thái | Đã mở — phạm vi quyền nhạy cảm đã chốt; đang chờ hợp đồng và bằng chứng audit runtime |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Hạn phản hồi | Trước khi nghiệm thu A-G02/A-G06 và đóng phạm vi phát hành Giai đoạn A |
| Phạm vi ban đầu | Quyền quản trị và đường audit thuộc M01/M11/M12 trong Giai đoạn A; chi tiết tiếp tục được chốt qua REL02-Q01–Q07 |
| Nơi lưu artifact | `docs/04-thuc-thi/lat-0/` và sổ bằng chứng Cổng A; không lưu secret, PII hoặc payload thô |
| Chặn | A-G02 và A-G06 |

## Phạm vi

Chứng minh rằng quyền tối thiểu, xác minh lại, từ chối mặc định và audit bền vững đủ kiểm soát thao tác quản trị nhạy cảm trong quyết định không dùng duyệt hai người và không có quyền tạm thời/khẩn cấp.

### Trong phạm vi

- Vai trò, quyền, phạm vi dữ liệu và mọi đường cấp/thu hồi/nâng quyền quản trị.
- Xác minh lại, lý do/vụ việc, giới hạn và kết quả từ chối cho thao tác nhạy cảm.
- Audit thành công, từ chối, thất bại và hành vi fail-closed khi không ghi được bằng chứng bắt buộc.
- Kiểm chứng không tồn tại quyền tạm thời/khẩn cấp và diễn tập phục hồi audit liên quan A-G06.

### Ngoài phạm vi task mở hồ sơ

- Chấp nhận rủi ro an toàn, miễn trừ audit, kết luận diễn tập hoặc quyết định phát hành; các việc này phải được xử lý bằng task/quyết định riêng.
- Thay đổi quyết định đã chốt để thêm duyệt hai người hoặc quyền khẩn cấp mà không cập nhật `DECISIONS.md`.

## Baseline quyết định và hiện trạng

| Nguồn | Baseline | Hệ quả bắt buộc |
|---|---|---|
| M11-D001–D006 | Quyền tối thiểu theo vai trò/phạm vi; không duyệt hai người; không quyền tạm thời/khẩn cấp | Xác minh lại, từ chối mặc định, lý do và audit là kiểm soát bù trừ bắt buộc |
| M11-D018–D021 | Audit không sửa/xóa thông thường; không lưu payload mặc định; không loại audit/bảo mật | Thao tác nhạy cảm phải dừng nếu bằng chứng bắt buộc không thể ghi |
| A-WP01/A-WP03 | Có đường cấp quyền quá rộng, sai tác nhân audit, payload chưa che và hàng đợi có thể loại bản ghi | Không dùng hiện trạng làm bằng chứng đạt; các finding nghiêm trọng phải được xử lý/re-test |

## Ma trận câu hỏi cần xác nhận

| Mã | Câu hỏi | Authority cần trả lời | Task/đầu ra ảnh hưởng | Trạng thái |
|---|---|---|---|---|
| REL02-Q01 | Danh mục vai trò, hành động và dữ liệu nhạy cảm trong phạm vi A là gì? | Chủ M11, chủ module và an toàn hệ thống | M11-T002–T004; A-G02 | Đã chốt design: M11-ACTION/ROLE/PERM-1.0; runtime evidence còn chờ |
| REL02-Q02 | Thao tác nào bắt buộc xác minh lại, lý do/vụ việc, hạn mức và audit trước–sau? | Chủ M11 và an toàn hệ thống | M11-T005–T007, M01-T028–T032 | Đã chốt design: M11-GRANT/ENHANCED-CONTROL-1.0; runtime evidence còn chờ |
| REL02-Q03 | Mọi đường cấp/nâng quyền nào phải được kiểm chứng không tạo quyền tạm thời/khẩn cấp? | Chủ M11 và an toàn hệ thống | M11-T006-A; G02-C05 | Đã chốt negative evidence M11-NO-EMERGENCY-A-1.0; runtime/deployment regression còn chờ |
| REL02-Q04 | Hợp đồng audit tối thiểu và ranh giới audit/activity/system log là gì? | Chủ audit M11, riêng tư và vận hành | M11-T031–T032 | Chờ xác nhận |
| REL02-Q05 | Metadata nào được phép lưu; trường/payload/bí mật nào bắt buộc loại hoặc che? | An toàn hệ thống, riêng tư và M12 | M11-T033; A-G02/A-G05 | Chờ xác nhận |
| REL02-Q06 | Khi audit store/queue lỗi hoặc đầy, thao tác nào fail-closed và cách cảnh báo/phục hồi ra sao? | An toàn hệ thống và vận hành | M11-T034–T035; G02-C07/C08; G06-E01 | Chờ xác nhận |
| REL02-Q07 | Ai được truy vấn/export log, theo vụ việc nào và retention/hold áp dụng thế nào? | Chủ M11, riêng tư và an toàn hệ thống | M11-T035; REL-07 | Chờ xác nhận |
| REL02-Q08 | Ai tự kiểm kiểm thử từ chối và diễn tập audit loss? | WSA-7K2 theo D-001 | A-G02/A-G06 | Đã chốt: WSA-7K2 tự nghiệm thu; không yêu cầu reviewer độc lập |

## Bằng chứng và tiêu chí đóng

| Evidence ID dự kiến | Bằng chứng | Chủ tạo | Người xác nhận | Kết quả yêu cầu | Trạng thái |
|---|---|---|---|---|---|
| A0-E005 | Ma trận vai trò–quyền–phạm vi và hành động nhạy cảm | WSA-7K2 | WSA-7K2 tự kiểm A-G02 | Mặc định từ chối; quyền xem/sửa và dữ liệu nhạy cảm tách rõ | Có bộ design M11-ROLE/PERM/ENHANCED-CONTROL-1.0; chưa phải runtime evidence |
| A0-E006 | Kết quả kiểm thử cho phép/từ chối, xác minh lại và không quyền khẩn cấp | WSA-7K2 | WSA-7K2 tự kiểm A-G02 | G02-C01–C06 đạt, bao gồm M11-T006-A | Chưa tạo |
| A0-E007 | Audit trước–sau, redaction và kiểm thử không mất audit | WSA-7K2 | WSA-7K2 tự kiểm A-G02/A-G05 | G02-C07–C09 đạt; không chứa payload/bí mật ngoài allowlist | Chưa tạo |
| A0-E008 | Biên bản diễn tập audit loss và phục hồi | WSA-7K2 | WSA-7K2 tự kiểm A-G06 | G06-E01 đạt hoặc finding có chủ đã re-test | Chưa tạo |

Các mã trên chỉ là định danh dự kiến; không đăng ký như bằng chứng thật trước khi có artifact và phiên bản kiểm tra được.

### Kết quả mở hồ sơ A0-T002

- WSA-7K2 là người thực hiện và tự nghiệm thu theo D-001.
- Phạm vi câu hỏi, task bị ảnh hưởng và đầu ra dự kiến A0-E005–A0-E008 đã được xác định.
- Hạn xử lý gắn với A-G02/A-G06 và phạm vi phát hành Giai đoạn A.
- Artifact được lưu trong cây tài liệu dự án với quy tắc không lưu secret, PII hoặc payload thô.

### Điều kiện đưa REL-02 sang tự kiểm kết luận và đóng

- REL02-Q01–Q08 có câu trả lời hoặc finding/task có chủ.
- A0-E005–A0-E008 có artifact thật trong sổ bằng chứng.
- Ma trận quyền, kiểm thử từ chối, xác minh lại, audit trước–sau, kiểm chứng không quyền khẩn và diễn tập đều đạt.
- Mọi finding nghiêm trọng/rất cao được xử lý và kiểm tra lại; WSA-7K2 tự kết luận A-G02/A-G06 theo D-001.

## Phụ thuộc và bàn giao

- BG-005 cung cấp đầu vào M12 cho redaction/log; REL-02 không tự kết luận thay REL-03.
- M11 Lát 4 dùng trực tiếp phạm vi quyền/audit sau khi REL-02 có kết luận tự kiểm; không cần bàn giao giữa bí danh.
- M11-T006-A giữ task M11-T006 cũ ở trạng thái không còn phù hợp; không mô tả quyền khẩn như năng lực đã triển khai.

## Lịch sử hồ sơ

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-15 | WSA-7K2 | Chuẩn bị baseline, REL02-Q01–Q08, kế hoạch A0-E005–A0-E008 và tiêu chí mở/đóng | Không có quyết định mới; chờ gán authority |
| 2026-08-18 | WSA-7K2 | Mở hồ sơ theo workflow một người; xác định chủ trì, hạn theo cổng, phạm vi ban đầu và nơi lưu artifact | D-001, D-008; chưa tạo Evidence ID |
| 2026-08-20 | WSA-7K2 | Chốt REL02-Q01–Q03 ở mức design/negative evidence; giữ hồ sơ mở cho audit và runtime evidence | D-034–D-038; M11-T002–T007 |
| 2026-08-20 | WSA-7K2 | Bổ sung immutable change-request schema, evidence/schedule/rollback gate cho 31 mutation action | D-039; M11-T008; chưa phải runtime evidence |
| 2026-08-20 | WSA-7K2 | Chốt state machine quyết định một actor theo revision/digest, TTL/invalidation/CAS/recovery | D-040; M11-T009; chưa phải runtime evidence |
| 2026-08-20 | WSA-7K2 | Chốt conflict set/key/mode, durable reservation/fencing và UTC/DST/cancel-reschedule semantics | D-041; M11-T010; chưa phải runtime evidence |
