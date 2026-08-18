# Gói task thực thi Lát 2 Giai đoạn A

## 1. Mục đích và phạm vi

Lát 2 xây biên an toàn trước khi triển khai phiên bản học liệu, tài sản và vận hành nâng cao. Gói gồm 44 task/lát con:

- 21 task M01 về đăng nhập, phiên, khôi phục, vai trò và quan sát danh tính.
- 14 task/lát con M11 về quyền quản trị, hỗ trợ, audit và log.
- 9 task M12 về danh tính ngoài, giới hạn lưu lượng, bí mật và redaction.

M11-T006 theo backlog cũ không được triển khai. Gói dùng **M11-T006-A** để kiểm chứng rằng không tồn tại quyền tạm thời hoặc quyền khẩn cấp, phù hợp quyết định đã chốt.

## 2. Điều kiện vào Lát 2

| Điều kiện | Bằng chứng yêu cầu | Nếu chưa đạt |
|---|---|---|
| Hồ sơ REL-02 và REL-03 đã có chủ | A0-T002, A0-T003 | Chưa khởi động task quyền/audit/bí mật |
| Từ điển M01, M11, M12 đã duyệt | M01-T001, M11-T001, M12-T001 | Chỉ được soạn nháp, không chốt hợp đồng |
| Registry tích hợp và criticality đã duyệt | M12-T002–M12-T003 | Chưa chốt failure mode/giới hạn |
| Hợp đồng dữ liệu/lỗi tích hợp đã duyệt | M12-T004–M12-T005 | Chưa chốt OAuth và suy giảm |
| Inventory bí mật và bản đồ dữ liệu lát A có khung | M12-T040, M12-T042-A | Chưa nghiệm thu vòng đời bí mật/redaction |
| Danh mục hành động quản trị đã duyệt | M11-T002 | Chưa lập vai trò/ma trận quyền |
| Mẫu bằng chứng A-G01/A-G02/A-G04/A-G05 sẵn sàng | Bộ mẫu Cổng A | Chưa thực hiện nghiệm thu chính thức |

## 3. Trình tự thực hiện trong Lát 2

```text
2A Audit, log và bí mật ──────────────┐
                                      ├──> 2D Hỗ trợ và khôi phục ──> 2F Nghiệm thu Lát 2
2B Vai trò và quyền ───────┐          │
                           ├──> 2C Đăng nhập và phiên
2E Danh tính ngoài/limiter ┘
```

2A phải hoàn thành phần taxonomy/allowlist trước khi các lát khác tạo bằng chứng. 2B và phần hợp đồng của 2E có thể chạy song song. 2D chỉ được đóng sau khi 2B–2C có kiểm soát xác minh lại và phiên.

## 4. M01 — Danh tính, phiên và quyền

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T010 | Chuẩn hóa luồng đăng nhập trực tiếp | Vòng đời tài khoản; chính sách bảo mật | Luồng kết quả đồng nhất, không lộ tài khoản, cưỡng chế trạng thái trước cấp phiên | Chủ M01 | An toàn hệ thống | P0 | M | M01-T002, T004 | A-G01 | Chờ Lát 1 |
| M01-T011 | Thiết kế kiểm soát thử đăng nhập bất thường | M01-T010; registry giới hạn | Ma trận theo IP/tài khoản/thiết bị, ngưỡng, cảnh báo và chống khóa nhầm | Chủ an toàn danh tính | M11/M12 | P0 | M | M01-T010; M12-T034 | A-G01, A-G04; REL-03 | Chờ limiter policy |
| M01-T012 | Chuẩn hóa xử lý tài khoản không hoạt động | Vòng đời tài khoản; M01-T010 | Quy tắc từ chối cho chờ xác minh/khóa/chờ xóa ở mọi đường đăng nhập và refresh | Chủ M01 | An toàn hệ thống | P0 | S | M01-T002, T010 | A-G01 | Chờ T010 |
| M01-T013 | Chuẩn hóa đăng nhập bằng danh tính bên ngoài | Hợp đồng M12-T006–T010; vòng đời tài khoản | Luồng thành công/lỗi/xung đột/suy giảm, không tạo tài khoản hoặc phiên ngoài chính sách | Chủ M01 | Chủ M12/an toàn hệ thống | P0 | M | M01-T002, T003; M12-T006–T010 | A-G01, A-G04; REL-03 | Chờ hợp đồng M12 |
| M01-T014 | Bảo vệ liên kết tài khoản hiện có | M01-T006, T010, T013 | Quy tắc không tự ghép theo email, xác minh lại và bằng chứng chống takeover | Chủ M01 | An toàn hệ thống | P0 | L | M01-T006, T010, T013 | A-G01 | Chờ T013 |
| M01-T015 | Xử lý xung đột và gỡ liên kết | M01-T014; chính sách khôi phục | Ma trận xung đột, ngắt liên kết an toàn, bảo vệ phương thức cuối và lịch sử quyết định | Chủ M01 | An toàn hệ thống/hỗ trợ | P0 | M | M01-T014, T019 | A-G01 | Chờ T014/T019 |
| M01-T016 | Chốt chính sách vòng đời phiên | Vòng đời tài khoản; chính sách bảo mật | Mô hình phiên theo thiết bị, hạn, phạm vi, thu hồi và sự kiện nhạy cảm | Chủ an toàn danh tính | Chủ M01/M11 | P0 | L | M01-T002, T004 | A-G01, A-G02 | Chờ Lát 1 |
| M01-T017 | Chuẩn hóa gia hạn và phát hiện tái sử dụng | M01-T012, T016 | Luồng rotation, phát hiện phát lại, thu hồi phạm vi, đồng thời và cảnh báo | Chủ M01 | An toàn hệ thống | P0 | L | M01-T012, T016 | A-G01 | Chờ T012/T016 |
| M01-T018 | Thiết kế đăng xuất và quản lý phiên | M01-T016; chính sách thiết bị | Đăng xuất một/tất cả thiết bị, danh sách phiên, thu hồi và bảo vệ quyền riêng tư | Chủ M01 | An toàn hệ thống | P0 | M | M01-T016; hợp đồng M01-T025 | A-G01 | Có thể thiết kế; đóng sau hợp đồng múi giờ/thiết bị cần thiết |
| M01-T019 | Thiết kế khôi phục quyền truy cập | Chính sách bảo mật; xác minh thư; vòng đời phiên | Luồng khôi phục có hạn, chống dò, xác minh lại, thu hồi phiên và cảnh báo | Chủ M01 | An toàn hệ thống/hỗ trợ | P0 | L | M01-T004, T006, T016; M12 | A-G01 | Chờ T016 và hợp đồng gửi tối thiểu |
| M01-T020 | Thiết kế thay đổi thông tin bảo mật | M01-T004, T016, T019 | Luồng đổi thư/mật khẩu/phương thức, xác minh hai phía, thu hồi và audit | Chủ M01 | An toàn hệ thống/M11 | P0 | M | M01-T004, T016, T019 | A-G01, A-G02 | Chờ T019 |
| M01-T021 | Xác định đường hỗ trợ khi mất mọi kênh | Bản đồ dữ liệu; khôi phục; support case | Quy trình không hạ chuẩn, bằng chứng chủ thể, hạn mức, lý do và audit | Chủ hỗ trợ danh tính | An toàn hệ thống/M11 | P1 | L | M01-T003, T019; M11-T029 | A-G01, A-G02; REL-07 | Chờ support case |
| M01-T028 | Xây dựng ma trận vai trò và quyền | Từ điển M01; M11-T004 | Ma trận hành động–vai trò–phạm vi–từ chối, tách xem/sửa và quyền tối thiểu | Chủ M01/M11 | An toàn hệ thống | P0 | L | M01-T001; M11-T004 | A-G02; REL-02 | Chờ M11-T004 |
| M01-T029 | Chuẩn hóa thay đổi vai trò | Phiên; ma trận quyền; audit | Luồng xác minh lại, lý do, hiệu lực phiên, trước–sau và thu hồi quyền | Chủ M01 | Chủ M11/an toàn hệ thống | P0 | M | M01-T016, T028; M11-T031 | A-G02; REL-02 | Chờ quyền/audit |
| M01-T030 | Bảo vệ vai trò quản trị cao nhất | M01-T028–T029; quyết định không quyền khẩn | Quy tắc không tự cấp quyền cao nhất, chống khóa quản trị cuối và kiểm thử từ chối | Chủ an toàn hệ thống | Chủ sản phẩm/M11 | P0 | M | M01-T028, T029; M11-T006-A | A-G02; REL-02 | Chờ T029/T006-A |
| M01-T031 | Chuẩn hóa khóa và mở tài khoản | Trạng thái; phiên; quyền; thông báo tối thiểu | Luồng lý do/xác minh lại/thu hồi mọi phiên, tác động module, mở khóa và audit | Chủ M01 | M11/an toàn hệ thống | P0 | M | M01-T012, T016, T028 | A-G01, A-G02 | Chờ phiên/quyền |
| M01-T032 | Thiết kế xử lý thay đổi quản trị đồng thời | M01-T029, T031 | Quy tắc version/xung đột, không ghi đè, kết quả xác định và audit | Chủ M01/M11 | An toàn hệ thống | P1 | M | M01-T029, T031 | A-G02; REL-02 | Chờ T029/T031 |
| M01-T038 | Xây dựng danh mục sự kiện danh tính | Vòng đời tài khoản; quyền; bản đồ dữ liệu | Catalog sự kiện actor/object/result/reason/correlation và mức audit/security | Chủ M01/M11 | An toàn hệ thống/riêng tư | P0 | M | M01-T002, T028, T033; M11-T031 | A-G01, A-G02 | Có thể khởi tạo; đóng sau bản đồ dữ liệu và audit chuẩn |
| M01-T039 | Thiết kế cảnh báo hành vi danh tính bất thường | Login controls; khôi phục; event catalog | Tín hiệu, ngưỡng, chống nhiễu, mức cảnh báo, chủ và playbook | Chủ an toàn danh tính | Chủ M11/vận hành | P0 | L | M01-T011, T019, T038 | A-G01, A-G06 | Chờ event catalog |
| M01-T040 | Xác định chỉ số sức khỏe M01 | Event catalog; từ điển chỉ số M11 | Công thức, mẫu số, nguồn, độ mới, ngưỡng và chủ cho sức khỏe danh tính | Chủ M01/dữ liệu | Chủ M11/vận hành | P1 | M | M01-T038; M11-T022 | A-G01, A-G06 | Chờ từ điển chỉ số |
| M01-T041 | Chuẩn hóa quyền tra cứu lịch sử danh tính | Ma trận quyền; event catalog; support case | Quyền tìm/xem, che dữ liệu, mục đích/vụ việc, hạn mức, audit lượt xem và lưu giữ | Chủ M01/M11 | Riêng tư/an toàn hệ thống | P0 | M | M01-T028, T038; M11-T027–T035 | A-G02; REL-02, REL-07 | Chờ hỗ trợ/audit |

## 5. M11 — Quyền quản trị, hỗ trợ và audit

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M11-T003 | Xây dựng vai trò quản trị | Danh mục hành động M11-T002 | Vai trò tách hỗ trợ/nội dung/vận hành/an toàn/dữ liệu và phạm vi rõ | Chủ M11 | An toàn hệ thống/các chủ module | P0 | L | M11-T002 | A-G02; REL-02 | Chờ Lát 1 |
| M11-T004 | Xây dựng ma trận quyền tối thiểu | M11-T002–T003 | Ma trận xem/sửa/phạm vi/mục đích/từ chối cho mọi hành động quản trị | Chủ M11 | An toàn hệ thống | P0 | L | M11-T002, T003 | A-G02; REL-02 | Chờ T003 |
| M11-T005 | Thiết kế cấp và thu hồi quyền | M11-T004; vòng đời phiên M01 | Quy trình xác minh lại, lý do, hiệu lực, thu hồi phiên, rà soát định kỳ và audit | Chủ M11 | Chủ M01/an toàn hệ thống | P0 | L | M11-T004; M01-T016 | A-G02; REL-02 | Chờ quyền/phiên |
| M11-T006-A | Kiểm chứng không có quyền tạm thời/khẩn cấp | Quyết định đã chốt; danh mục đường cấp quyền | Ma trận kiểm chứng mọi đường nâng quyền đều không tạo phiên/quyền khẩn; bằng chứng từ chối và lý do loại T006 cũ | Chủ an toàn hệ thống | Chủ sản phẩm/M11 | P0 | M | M11-T005 | A-G02, A-G06; REL-02 | Task thay thế; không đóng M11-T006 cũ như đã triển khai |
| M11-T007 | Phân loại thao tác cần kiểm soát tăng cường | Danh mục hành động; ma trận quyền | Phân loại rủi ro, xác minh lại, hạn mức, bằng chứng và hành vi từ chối; không yêu cầu duyệt hai người | Chủ M11 | An toàn hệ thống/sản phẩm | P0 | L | M11-T002, T004 | A-G02; REL-02 | Chờ T004 |
| M11-T027 | Đặc tả tìm kiếm người dùng an toàn | M11-T004; bản đồ dữ liệu M01 | Tìm theo định danh tối thiểu, vụ việc/mục đích, che kết quả, hạn mức và audit lượt tìm | Chủ hỗ trợ M11 | Riêng tư/an toàn hệ thống | P0 | M | M11-T004; M01-T003 | A-G02; REL-07 | Chờ quyền/dữ liệu |
| M11-T028 | Xây dựng dòng thời gian hỗ trợ | M11-T027; event catalog M01 | Timeline nguồn, độ mới, mâu thuẫn, quyền xem và bảo vệ nguồn khỏi sửa | Chủ hỗ trợ M11 | Chủ M01/dữ liệu | P0 | L | M11-T027; M01-T038 | A-G02; REL-07 | Chờ tìm kiếm/event catalog |
| M11-T029 | Thiết kế vòng đời vụ việc hỗ trợ | M11-T027–T028 | Case có chủ, SLA, lý do, bằng chứng, ghi chú, trạng thái, liên kết trùng và audit | Chủ hỗ trợ M11 | Vận hành/riêng tư | P0 | L | M11-T027, T028 | A-G02; REL-07 | Chờ timeline |
| M11-T030 | Chốt thao tác hỗ trợ có kiểm soát | Support case; thay đổi/rollback M11-T011 | Ma trận thao tác, xem trước, xác minh lại, reason, hạn mức, bù/đối soát; không sửa nguồn sự thật trực tiếp | Chủ M11 | Các chủ module/an toàn hệ thống | P0 | L | M11-T029; hợp đồng thay đổi tối thiểu | A-G02; REL-02, REL-07 | Chờ case; phần rollback đầy đủ ở Lát 3A |
| M11-T031 | Đặc tả sự kiện kiểm toán chuẩn | Danh mục hành động M11-T002 | Schema khái niệm actor/object/before/after/result/reason/correlation, bao phủ từ chối/thất bại | Chủ audit M11 | An toàn hệ thống/riêng tư | P0 | L | M11-T002 | A-G02, A-G05; REL-02 | Chờ Lát 1 |
| M11-T032 | Phân tách audit, activity và log vận hành | M11-T031 | Ranh giới mục đích, nguồn sự thật, lưu giữ, quyền và điều kiện không thay thế nhau | Chủ audit M11 | Vận hành/an toàn hệ thống | P0 | M | M11-T031 | A-G02, A-G05; REL-02 | Chờ T031 |
| M11-T033 | Xây dựng quy tắc che dữ liệu và bí mật | M11-T032; inventory/data map M12 | Allowlist metadata, quy tắc redaction, dữ liệu cấm và bộ case kiểm thử | Chủ an toàn log | Riêng tư/M12 | P0 | L | M11-T032; M12-T040–T043 | A-G02, A-G05; REL-03 | Chờ taxonomy/data map |
| M11-T034 | Thiết kế thu nhận log bền vững | M11-T033; criticality M12 | Phân tầng audit/security/ops, backpressure, không loại audit, cảnh báo tồn và phục hồi ghi | Chủ nền tảng M11 | An toàn hệ thống/vận hành | P0 | L | M11-T033; M12-T003 | A-G02, A-G06; REL-02, REL-03 | Chờ redaction/criticality |
| M11-T035 | Chốt tìm kiếm, truy cập và lưu giữ log | M11-T004, T034; chính sách lưu giữ | Quyền xem, vụ việc/mục đích, che dữ liệu, audit lượt xem, retention/legal hold và xóa đúng hạn | Chủ M11/riêng tư | An toàn hệ thống | P0 | L | M11-T004, T034 | A-G02, A-G05; REL-02, REL-07 | Chờ quyền/log bền vững |

## 6. M12 — Danh tính ngoài, limiter, bí mật và log

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M12-T006 | Đặc tả dữ liệu danh tính tối thiểu | Hợp đồng M12-T004; bản đồ M01 | Danh sách claim được duyệt, mục đích, xác minh, thiếu trường, lưu giữ và xóa | Chủ M12/M01 | Riêng tư/an toàn hệ thống | P0 | M | M12-T004; M01-T003 | A-G01, A-G05; REL-03 | Chờ Lát 1 |
| M12-T007 | Thiết kế chống giả mạo và phát lại | M12-T006; luồng M01 | Quy tắc state/redirect/code một lần/hạn, từ chối và audit không chứa bí mật | Chủ an toàn tích hợp | Chủ M01 | P0 | L | M12-T006 | A-G01, A-G04 | Chờ T006 |
| M12-T008 | Chốt vòng đời token ngoài | M12-T006–T007; inventory bí mật | Chính sách nhận/dùng tối thiểu/hết hạn/thu hồi/xoay/không log và bằng chứng lưu trữ | Chủ M12 | An toàn hệ thống | P0 | L | M12-T006, T007; M12-T040 | A-G01, A-G05; REL-03 | Chờ T007/inventory |
| M12-T009 | Đặc tả liên kết và ngắt liên kết | M12-T008; chính sách M01 | Hợp đồng bằng chứng link/unlink, xác minh lại, phương thức cuối và takeover | Chủ M12/M01 | An toàn hệ thống | P0 | L | M12-T008; M01-T014–T015 | A-G01 | Chờ phối hợp M01 |
| M12-T010 | Thiết kế suy giảm khi danh tính ngoài lỗi | Criticality; M12-T009; phiên M01 | Playbook outage, không fail-open, phiên hợp lệ/fallback, cảnh báo và SLO owner | Chủ M12/vận hành | Chủ M01/an toàn hệ thống | P0 | L | M12-T003, T009 | A-G01, A-G04; REL-03 | Chờ T009 |
| M12-T034 | Xây dựng ma trận giới hạn lưu lượng | Criticality; shared-state registry | Chính sách theo IP/user/device/cost cho auth, AI, speech, upload, battle, notification/admin và Retry-After | Chủ nền tảng M12 | An toàn hệ thống/các module | P0 | L | M12-T003, T031 | A-G01, A-G04; REL-03 | Chờ Lát 1 |
| M12-T035 | Chốt fail-open/fail-closed theo năng lực | M12-T003, T034 | Failure-mode matrix; không bypass; auth/chi phí/gian lận không allow-all; fallback cục bộ/cảnh báo | Chủ M12 | An toàn hệ thống/vận hành | P0 | L | M12-T003, T034 | A-G04; REL-03 | Chờ T034 |
| M12-T041 | Thiết kế vòng đời bí mật | Inventory M12-T040; quyền M11 | Kho lưu, workload access, rotate/revoke, audit, break-glass theo quyết định và không downtime mục tiêu | Chủ an toàn nền tảng | Chủ M11/M12 | P0 | L | M12-T040; M11-T004 | A-G05; REL-03 | Chờ inventory/quyền |
| M12-T043 | Chốt che dữ liệu và chính sách log | Inventory, data map, audit taxonomy | Allowlist metadata, redaction cho token/code/email/prompt/audio/response và test PII/secret giả | Chủ M12/M11 | Riêng tư/an toàn hệ thống | P0 | L | M12-T040–T042-A; M11-T031–T033 | A-G02, A-G05; REL-03 | Chờ data map/audit taxonomy |

## 7. Thứ tự kéo task khuyến nghị

| Đợt | Task | Kết quả mở khóa |
|---:|---|---|
| 1 | M11-T031–T033; M12-T041, T043; M01-T038 | Taxonomy audit, ranh giới log, redaction, vòng đời bí mật và event catalog |
| 2 | M11-T003–T004; M11-T006-A, T007; M01-T016, T028; M12-T006–T008; M12-T034–T035 | Vai trò/quyền nền, phiên, claim/token ngoài và failure mode limiter |
| 3 | M11-T005; M01-T010–T014, T017–T018, T029–T031; M12-T009–T010 | Cấp/thu hồi quyền, đăng nhập/phiên/OAuth/linking/khóa an toàn |
| 4 | M01-T015, T019–T021, T032; M11-T027–T030; M11-T034–T035 | Gỡ liên kết, khôi phục, hỗ trợ có vụ việc, log bền vững và tra cứu kiểm soát |
| 5 | M01-T011, T039–T041 | Cảnh báo, chỉ số sức khỏe và quyền tra cứu lịch sử danh tính |

Không kéo Đợt 3 trước khi taxonomy audit/redaction của Đợt 1 được duyệt; nếu không, các luồng mới sẽ tiếp tục tạo bằng chứng không an toàn hoặc không đủ truy vết.

## 8. Definition of Done chung cho Lát 2

- Mỗi task giữ liên kết với baseline, Cổng A, REL và Evidence ID trong sổ chung.
- Có kịch bản thành công, từ chối, hết hạn, gửi lặp, đồng thời, lỗi phụ thuộc và thu hồi phù hợp.
- Không có token, mã xác minh, payload, dữ liệu cá nhân thật hoặc bí mật trong tài liệu/bằng chứng.
- Trạng thái tài khoản/quyền được cưỡng chế trên mọi đường liên quan, không chỉ giao diện quản trị.
- Audit ghi đúng tác nhân, đối tượng, lý do, trước–sau và kết quả; thao tác nhạy cảm dừng nếu không ghi được bằng chứng bắt buộc.
- Không có quyền tạm thời/khẩn cấp, không tự cấp vai trò cao nhất và không tự liên kết tài khoản theo email.
- Không có đường trusted/internal bỏ qua giới hạn lưu lượng; failure mode đúng từng năng lực.
- Người xác nhận độc lập đánh dấu đạt trong mẫu A-G01/A-G02/A-G04/A-G05 tương ứng.

## 9. Tiêu chí hoàn thành gói Lát 2

1. Cả 44 task/lát con đã được nhập với chủ thực tế, phụ thuộc và bằng chứng cần nộp.
2. M11-T006 cũ giữ trạng thái “không còn phù hợp”; M11-T006-A có bằng chứng không tồn tại đường quyền khẩn cấp.
3. Các sai lệch trực tiếp sau không còn: tự ghép theo email, state OAuth không xác minh, token trong địa chỉ/log, tài khoản khóa vẫn truy cập, Admin tự cấp quyền cao nhất, payload thô, audit bị loại, limiter bypass/fail-open không phân loại.
4. Kịch bản liên quan trong A-G01, A-G02, A-G04 và A-G05 đã chạy và không còn finding nghiêm trọng/rất cao.
5. REL-02 và phần kỹ thuật của REL-03 có bộ bằng chứng sẵn sàng xác nhận; phần phụ thuộc pháp lý/ngoài hệ thống được ghi rõ, không đánh dấu đạt giả.

## 10. Bước tiếp theo

Sau Lát 2, triển khai Lát 3A–3D: phiên bản hóa cấu hình, học liệu có kiểm duyệt, tài sản có vòng đời và hồ sơ/thiết bị người dùng.
