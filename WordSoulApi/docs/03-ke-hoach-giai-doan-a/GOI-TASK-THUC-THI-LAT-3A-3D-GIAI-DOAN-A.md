# Gói task thực thi Lát 3A–3D Giai đoạn A

## 1. Mục đích và phạm vi

Gói này chuyển các nền tảng Lát 1 và biên an toàn Lát 2 thành tài nguyên có phiên bản, học liệu được kiểm duyệt, tài sản có vòng đời và hồ sơ người dùng có hợp đồng rõ. Gói có 48 task/lát con duy nhất:

| Lát | Phạm vi | Số task duy nhất |
|---|---|---:|
| 3A | Thay đổi, cấu hình, nội dung quản trị và công việc nền | 15 |
| 3B | Học liệu có phiên bản và kiểm duyệt | 19 |
| 3C | Tài sản số có vòng đời | 9 |
| 3D | Hồ sơ và thiết bị người dùng | 5 |
| **Tổng** |  | **48** |

M11-T012 đã nằm trong Lát 1 và chỉ là đầu vào cho Lát 3A. M11-T019–M11-T021 do Lát 3A sở hữu nhưng đồng thời là điều kiện của Lát 3B. M01-T024 do Lát 3C sở hữu và được Lát 3D sử dụng.

## 2. Quy tắc cắt lát phụ thuộc Giai đoạn B

Một số Definition of Done tham chiếu module chưa được triển khai ở Giai đoạn A. Các task sau dùng mã lát A và không đóng task nguồn toàn phần:

| Lát A | Task nguồn | Phạm vi được đóng trong A | Phần giữ mở |
|---|---|---|---|
| M02-T008-A | M02-T008 | Phiên bản mục từ bất biến, hiệu lực và hợp đồng snapshot | Nghiệm thu tích hợp đầy đủ với M03/M04 |
| M02-T009-A | M02-T009 | Hợp đồng gói học liệu và hành vi thiếu trường | Tiếp nhận thực tế ở M03/M05/M08 |
| M02-T023-A | M02-T023 | Ma trận tác động, snapshot và thời điểm hiệu lực | Nghiệm thu phiên/tiến độ thực tế ở M03/M04 |
| M01-T022-A | M01-T022 | Quyền xem/sửa hồ sơ trong phạm vi A/B | Hiển thị cộng đồng M09 |
| M01-T023-A | M01-T023 | Quy tắc tên hiển thị và lịch sử trong phạm vi hồ sơ | Lan truyền/xung đột tên ở M09 |
| M01-T025-A | M01-T025 | Lưu múi giờ/giờ học và quy tắc đổi múi giờ | Tiêu thụ nhắc học thực tế ở M10 |
| M01-T026-A | M01-T026 | Registry thiết bị, chủ sở hữu và trạng thái endpoint | Gửi push đa thiết bị ở M10/B-WP08 |
| M01-T027-A | M01-T027 | Thu hồi endpoint và tác động quyền riêng tư | Xác nhận chuyển phát ở M10/B-WP08 |
| M11-T040-A | M11-T040 | Hợp đồng đối soát, sai lệch và cảnh báo cho năng lực A | Dashboard/đối soát phát hành đầy đủ phụ thuộc M11-T024 ở B-WP07 |

## 3. Điều kiện vào chung

- Mọi task Lát 2 liên quan quyền, audit, redaction và bí mật đã đạt hoặc có phụ thuộc rõ không chặn đường thực hiện.
- M11-T012, M11-T018, M12-T021 và các từ điển M01/M02/M11/M12 đã được duyệt.
- REL-02, REL-03, REL-04 và REL-07 đã có chủ; CT-01–CT-07 còn hiệu lực trong khi cổng chưa đạt.
- Mẫu A-G01, A-G02, A-G03 và A-G05 đã được sao chép thành hồ sơ nghiệm thu của phạm vi phát hành.

## 4. Lát 3A — Thay đổi, cấu hình và công việc nền

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M11-T008 | Đặc tả yêu cầu thay đổi | Phân loại thao tác M11-T007 | Hồ sơ mục đích, phạm vi, tác động, bằng chứng, lịch và rollback | Chủ M11 | Chủ module/an toàn hệ thống | P0 | M | M11-T007 | A-G02, A-G06; REL-02 | Chờ Lát 2 |
| M11-T009 | Thiết kế vòng đời quyết định thay đổi | Quyền M11-T005; yêu cầu thay đổi | Trạng thái một người thực hiện có kiểm soát, hết hạn, vô hiệu khi nội dung đổi và truy vết quyết định | Chủ M11 | An toàn hệ thống | P0 | L | M11-T005, T008 | A-G02; REL-02 | Không bổ sung duyệt hai người trái quyết định |
| M11-T010 | Chốt xung đột và lịch hiệu lực | M11-T009; timezone policy | Quy tắc version conflict, thời điểm/múi giờ, hủy/đổi lịch và hai thay đổi cùng phạm vi | Chủ vận hành thay đổi | Chủ module | P0 | L | M11-T009 | A-G02, A-G06 | Chờ T009 |
| M11-T011 | Thiết kế thực thi và rollback có kiểm chứng | M11-T009–T010 | Idempotency, trạng thái từng phần, xác minh kết quả, rollback và audit | Chủ vận hành M11 | An toàn hệ thống/chủ module | P0 | L | M11-T009, T010 | A-G02, A-G06; REL-02 | Chờ T010 |
| M11-T013 | Đặc tả kiểm tra giá trị và phụ thuộc | Registry cấu hình M11-T012 | Quy tắc kiểu/range/chéo module, lỗi thống nhất và chủ module xác nhận | Chủ cấu hình M11 | Các chủ module | P0 | L | M11-T012 | A-G02 | Sẵn sàng sau Lát 1 |
| M11-T014 | Thiết kế phiên bản cấu hình bất biến | M11-T008, T013 | Bộ cấu hình nguyên tử, version, lịch sử hiệu lực và không sửa tại chỗ | Chủ cấu hình M11 | An toàn hệ thống/vận hành | P0 | L | M11-T008, T013 | A-G02, A-G06 | Chờ T008/T013 |
| M11-T015 | Thiết kế xem trước và mô phỏng tác động | M11-T014; từ điển chỉ số M11-T022 | Baseline, đối tượng ảnh hưởng, chi phí/rủi ro, giới hạn và kết quả gắn draft version | Chủ M11/dữ liệu | Chủ module/sản phẩm | P0 | L | M11-T014, T022 | A-G02, A-G06 | Chờ T014 |
| M11-T016 | Chốt triển khai giới hạn và quan sát | M11-T011, T015 | Phạm vi ổn định, chỉ số dừng, nhóm nhạy cảm bị loại và bằng chứng quan sát | Chủ vận hành | An toàn hệ thống/sản phẩm | P0 | L | M11-T011, T015 | A-G02, A-G06 | Chờ T011/T015 |
| M11-T017 | Thiết kế quay lại và xử lý khóa ngừng dùng | M11-T014, T016 | Rollback version, tương thích, deprecate, tham chiếu và cảnh báo drift | Chủ cấu hình M11 | Chủ module/vận hành | P0 | L | M11-T014, T016 | A-G02, A-G06 | Chờ T016 |
| M11-T019 | Chuẩn hóa vòng đời nội dung | Ma trận nội dung M11-T018; M11-T009 | Vòng đời chuẩn, quyền, version, hiệu lực, thu hồi và audit xuyên module | Chủ nội dung M11 | Chủ M02 và module nội dung | P0 | L | M11-T009, T018 | A-G02, A-G03; REL-04 | Chờ T009 |
| M11-T020 | Thiết kế phân tích tham chiếu trước thay đổi | M11-T018–T019 | Báo cáo nơi dùng, bản đang chạy/lịch sử, tác động và phương án thay thế | Chủ M11/dữ liệu | Các module tiêu thụ | P0 | L | M11-T018, T019 | A-G03, A-G06 | Chờ T019 |
| M11-T021 | Chốt xử lý chỉnh sửa đồng thời | M11-T019 | Version token nghiệp vụ, diff, từ chối ghi đè và yêu cầu đánh giá lại | Chủ M11 | An toàn hệ thống/chủ nội dung | P1 | M | M11-T019 | A-G02, A-G03 | Chờ T019 |
| M11-T038 | Lập sổ đăng ký công việc nền | Sổ sức khỏe M11-T036 | Registry chủ, lịch/trigger, idempotency, source of truth, retry, criticality và health | Chủ vận hành nền | Các chủ module | P0 | L | M11-T036 thuộc Lát 4 | A-G06; REL-03, REL-07 | Chưa bắt đầu cho tới M11-T036 |
| M11-T039 | Thiết kế lịch sử chạy và phục hồi công việc | Quyền M11-T004; M11-T038 | Run history, checkpoint, lease, replay/rerun, chống lặp và audit | Chủ vận hành nền | An toàn hệ thống/chủ module | P0 | L | M11-T004, T038 | A-G02, A-G06; REL-07 | Chờ T038 |
| M11-T040-A | Thiết kế đối soát và cảnh báo sai lệch — lát A | M11-T039; hợp đồng dữ liệu A | Hợp đồng expected/actual, tolerance, finding, cảnh báo, chủ và hành động sửa cho năng lực A | Chủ dữ liệu vận hành | Chủ module/an toàn hệ thống | P0 | L | M11-T039; M11-T024 ở B cho bản đầy đủ | A-G06; REL-07 | Lát A; không đóng M11-T040 toàn phần |

## 5. Lát 3B — Học liệu có phiên bản và kiểm duyệt

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M02-T007 | Xác định vòng đời mục từ | Từ điển/quality M02; M11-T019 | Trạng thái, quyền, điều kiện vào/ra, hiển thị và tác động module | Chủ M02 | Học thuật/M11 | P0 | M | M02-T001; M11-T019 | A-G03 | Chờ 3A |
| M02-T008-A | Thiết kế phiên bản hóa mục từ — lát A | M02-T006–T007; consumer contract | Version bất biến, hiệu lực, lịch sử, draft tách published và snapshot contract | Chủ M02 | M03/M04 ở mức hợp đồng; M11 | P0 | L | M02-T006, T007 | A-G03 | Lát A; giữ integration M03/M04 mở |
| M02-T009-A | Chuẩn hóa nội dung cung cấp module học — lát A | Mô hình nghĩa; M02-T008-A | Gói dữ liệu có sense/version, trường bắt buộc/tùy chọn và hành vi thiếu | Chủ M02/kiến trúc | Chủ M03/M05/M08 ở mức hợp đồng | P0 | M | M02-T002, T006, T008-A | A-G03 | Lát A; chưa đóng tiếp nhận module hoãn |
| M02-T010 | Thiết kế ngừng dùng, hợp nhất và thay thế | Trùng/phiên bản/consumer contract | Redirect thay thế, giữ lịch sử, tác động module và audit | Chủ M02 | Học thuật/M11 | P0 | L | M02-T004, T008-A, T009-A | A-G03 | Chờ contract/version |
| M02-T015 | Chuẩn hóa dữ liệu và tiêu chí bộ từ | CEFR/quality M02 | Mục tiêu, theme, difficulty, size, checklist draft/public và lý do giới hạn | Chủ nội dung M02 | Học thuật/sản phẩm | P0 | M | M02-T005, T006 | A-G03 | Sẵn sàng sau Lát 1 |
| M02-T016 | Xây dựng ma trận quyền bộ từ | Quyền M01/M11; vòng đời mục từ | Quyền tạo/sửa/xem/gửi/duyệt/thu hồi/xóa theo chủ và trạng thái | Chủ M02/M11 | An toàn hệ thống | P0 | M | M01-T028, M02-T007; M11-T004 | A-G02, A-G03; REL-02 | Chờ quyền/vòng đời |
| M02-T017 | Thiết kế vòng đời bộ từ | M02-T015–T016 | Nháp/private/chờ duyệt/cần sửa/public/thu hồi, chuyển hợp lệ và visibility | Chủ M02 | M11/sản phẩm | P0 | L | M02-T015, T016 | A-G03 | Chờ T016 |
| M02-T018 | Xử lý chủ sở hữu không còn hoạt động | Khóa/xóa M01; quyền bộ | Freeze/transfer/owner thay thế cho private/public, không lộ dữ liệu và audit | Chủ M02/M01 | Riêng tư/M11 | P1 | M | M01-T031; M01-T036 thuộc Lát 5; M02-T016 | A-G01, A-G03; REL-07 | Thiết kế được; đóng sau ma trận xóa M01-T036 |
| M02-T019 | Thiết kế sao chép và nguồn gốc bộ từ | Quyền/vòng đời bộ | Provenance, quyền sửa bản sao, quan hệ version nguồn và xử lý nguồn thu hồi | Chủ M02 | Học thuật/pháp lý | P2 | M | M02-T016, T017 | A-G03; REL-04 | Chờ T017/REL-04 |
| M02-T020 | Chuẩn hóa thêm và bỏ mục từ trong bộ | Trùng/vòng đời bộ | Quyền, chống trùng theo nghĩa, chặn mục thu hồi, lịch sử và tác động | Chủ M02 | Học thuật/M11 | P0 | M | M02-T004, T017 | A-G03 | Chờ T017 |
| M02-T021 | Thiết kế sắp xếp thành phần bộ | M02-T020; concurrency M11-T021 | Thứ tự duy nhất, thao tác reorder, xung đột và version list | Chủ M02 | M11/M03 contract | P0 | M | M02-T020; M11-T021 | A-G03 | Chờ T020/concurrency |
| M02-T022 | Chuẩn hóa nội dung ghi đè theo bộ | Mô hình nghĩa/quality/thành phần | Trường cho phép, scope chỉ trong bộ, source/version/review và ưu tiên đọc | Chủ M02 | Học thuật/M11 | P0 | L | M02-T002, T006, T020 | A-G03 | Chờ T020 |
| M02-T023-A | Xác định tác động thay đổi bộ đang học — lát A | Vòng đời/thành phần/override | Snapshot contract, hiệu lực tương lai, ma trận session/library/progress và thông báo | Chủ M02/kiến trúc | M03/M04 ở mức hợp đồng | P0 | L | M02-T017, T020–T022 | A-G03 | Lát A; integration M03/M04 giữ mở |
| M02-T029 | Thiết kế quy trình gửi duyệt | Quality/assets/lifecycle/override | Eligibility, locked version, trạng thái gửi và sửa sau gửi tạo bản mới | Chủ M02/M11 | Học thuật/an toàn hệ thống | P0 | M | M02-T006, T012, T017, T022 | A-G03 | Chờ 3C asset review |
| M02-T030 | Xây dựng checklist kiểm duyệt công khai | Quality/assets/set criteria/M11 | Đạt–cần sửa–từ chối cho học thuật/quyền/an toàn/độ khó, lý do và conflict | Chủ chất lượng nội dung | Học thuật/pháp lý/M11 | P0 | M | M02-T006, T012, T015 | A-G03; REL-04 | Chờ asset checklist |
| M02-T031 | Thiết kế yêu cầu sửa và từ chối | M02-T029–T030 | Lý do theo tiêu chí, version mới, resubmit và lịch sử đầy đủ | Chủ M02 | M11/người tạo đại diện | P0 | M | M02-T029, T030 | A-G03 | Chờ review flow |
| M02-T032 | Thiết kế xuất bản theo phiên bản | Version/lifecycle/submission/checklist | Chỉ version approved public, hiệu lực, idempotency và consumer version | Chủ M02 | M11/M03 contract | P0 | L | M02-T008-A, T017, T029, T030 | A-G03 | Chờ các cổng duyệt |
| M02-T033 | Thiết kế báo cáo và thu hồi nội dung | Replacement/publish/M11 | Severity, owner, SLA, temporary hide, investigation, withdrawal và tác động module | Chủ M02/M11 | Học thuật/an toàn hệ thống | P0 | L | M02-T010, T032; M11-T019–T020 | A-G03, A-G06; REL-04 | Chờ publish/reference analysis |
| M02-T034 | Thiết kế khiếu nại quyết định nội dung | Request changes/withdrawal/M11 | Deadline, người xét phù hợp, bằng chứng, kết quả cuối, lịch sử và không auto-public | Chủ M11/nội dung | Sản phẩm/pháp lý | P2 | M | M02-T031, T033 | A-G03; REL-04 | Chờ T031/T033 |

## 6. Lát 3C — Tài sản số có vòng đời

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T024 | Thiết kế vòng đời ảnh đại diện | Quyền hồ sơ; asset catalog | Upload/review/access/replace/delete/placeholder và privacy cho avatar | Chủ M01/M12 | Riêng tư/M11 | P1 | M | M01-T022-A; M12-T021–T025 | A-G01, A-G05; REL-04 | Chờ asset foundation |
| M02-T011 | Lập danh mục tài sản học liệu | Quality M02; asset catalog M12 | Loại ảnh/audio, mục đích, source/right, locale/voice, trạng thái và retention | Chủ M02/M12 | Học thuật/pháp lý | P0 | M | M02-T006; M12-T021 | A-G03, A-G05; REL-04 | Chờ catalog M12 |
| M02-T012 | Chuẩn hóa kiểm duyệt tài sản | M02-T011 | Checklist đúng nghĩa/nội dung/giọng, đạt–sửa–từ chối, reviewer/evidence | Chủ chất lượng nội dung | Học thuật/pháp lý | P0 | M | M02-T011 | A-G03; REL-04 | Chờ T011 |
| M02-T013 | Thiết kế xử lý tài sản lỗi hoặc thiếu | Catalog/checklist/degradation | Required/optional, placeholder, incomplete state, retry safe và observability | Chủ M02/M12 | Chủ M03/M05 ở mức contract | P1 | M | M02-T011, T012; M12-T003–T005 | A-G03, A-G04 | Chờ review/error taxonomy |
| M02-T014 | Thiết kế vòng đời thay thế tài sản | Version content; catalog | Preview, asset version, giữ/xóa đúng retention và không hỏng lịch sử | Chủ M02/M12 | M11/riêng tư | P1 | M | M02-T008-A, T011; M12-T025 | A-G03, A-G05 | Chờ asset lifecycle |
| M12-T022 | Đặc tả upload an toàn | Asset catalog; secret policy | Type/size/content/malware/quota/owner/purpose/request ID và partial failure | Chủ M12 | An toàn hệ thống/riêng tư | P0 | L | M12-T021, T040–T043 | A-G05; REL-03, REL-04 | Chờ Lát 2 secret/log |
| M12-T023 | Thiết kế metadata và định danh bất biến | M12-T022 | Asset ID, checksum, owner, source, license, version, state, references và dedupe | Chủ M12/dữ liệu | Chủ M01/M02 | P0 | L | M12-T022 | A-G03, A-G05; REL-04 | Chờ upload contract |
| M12-T024 | Chốt quyền truy cập và phân phối | Asset sensitivity; M12-T023 | Public/private, URL có hạn, CDN/cache invalidation, revoke và auth không dựa URL | Chủ M12/an toàn hệ thống | Riêng tư/các chủ module | P0 | L | M12-T021, T023 | A-G05 | Chờ metadata |
| M12-T025 | Thiết kế thay thế, xóa và orphan cleanup | Metadata/access; M11 change/audit | Version replace, reference check, grace, orphan audit, deletion proof và placeholder | Chủ M12 | Chủ M01/M02/M11 | P0 | L | M12-T023, T024; M11-T020 | A-G03, A-G05; REL-04, REL-07 | Chờ reference analysis |

## 7. Lát 3D — Hồ sơ và thiết bị người dùng

| Task ID | Tên task | Input chính | Output/bằng chứng để đóng | Chủ trì đề xuất | Người xác nhận | Ưu tiên | Size | Phụ thuộc thực thi | Cổng/REL | Trạng thái khởi tạo |
|---|---|---|---|---|---|---|---|---|---|---|
| M01-T022-A | Chuẩn hóa quyền xem và sửa hồ sơ — lát A | Data map; role matrix | Ma trận field/view/edit/scope/redaction/re-auth cho self/admin trong A/B | Chủ M01 | Riêng tư/M11 | P0 | M | M01-T003, T028 | A-G01, A-G02; REL-01, REL-07 | Lát A; M09 giữ mở |
| M01-T023-A | Thiết kế thay đổi tên hiển thị — lát A | M01-T022-A | Normalize/moderation/cooldown/history/conflict trong hồ sơ A/B | Chủ M01 | M02/M11 | P1 | S | M01-T022-A | A-G01 | Lát A; propagation M09 giữ mở |
| M01-T025-A | Chuẩn hóa múi giờ và giờ học mong muốn — lát A | M01-T022-A; timezone policy | Source, validation, change history, DST behavior và contract cho M10 | Chủ M01 | Chủ M10 ở mức contract | P0 | M | M01-T022-A | A-G01; chuẩn bị REL-06 | Lát A; consumption M10 giữ mở |
| M01-T026-A | Thiết kế đăng ký nhiều thiết bị nhận tin — lát A | Session policy; timezone contract | Device endpoint registry, owner, consent state, shared-device risk, dedupe và expiry | Chủ M01/M12 | Riêng tư/M10 contract | P0 | M | M01-T016, T025-A | A-G01, A-G05; REL-06 | Lát A; delivery M10/B-WP08 giữ mở |
| M01-T027-A | Thiết kế thu hồi thiết bị nhận tin — lát A | Session logout; device registry | Revoke on logout/account/security change, invalid endpoint lifecycle và audit | Chủ M01/M12 | Chủ M10 ở mức contract | P0 | M | M01-T018, T026-A | A-G01, A-G05; REL-06 | Lát A; delivery feedback giữ mở |

## 8. Thứ tự kéo task khuyến nghị

| Đợt | Task chính | Kết quả mở khóa |
|---:|---|---|
| 1 | M11-T008–T010, T013, T019, T021; M02-T007, T015–T017; M12-T022; M01-T022-A | Vòng đời, quyền, thay đổi, validation và upload contract |
| 2 | M11-T011, T014, T020; M02-T008-A–T010, T020–T022; M12-T023–T024; M01-T023-A, T025-A | Version, reference analysis, content/asset metadata và profile contract |
| 3 | M11-T015–T017; M02-T011–T014, T023-A; M12-T025; M01-T024, T026-A–T027-A | Preview/rollback, snapshot, asset lifecycle và device lifecycle |
| 4 | M02-T029–T032 | Gửi duyệt, checklist, request changes và xuất bản version |
| 5 | M02-T018–T019, T033–T034 | Owner inactive, provenance/copy, report/withdrawal và appeal |
| 6 | M11-T038–T040-A | Chỉ bắt đầu sau M11-T036 ở Lát 4; tạo registry job, run history và reconciliation lát A |

## 9. Definition of Done chung

- Mọi thay đổi cấu hình/nội dung có request, version bất biến, hiệu lực, preview/reference impact, concurrency và rollback phù hợp.
- Học liệu công khai chỉ từ version đã duyệt; người tạo không tự duyệt; report/withdrawal/appeal có audit.
- Phiên học tương lai nhận sense/version contract; phiên đang chạy không đổi học liệu ngầm.
- Tài sản có ID bất biến, metadata, source/right state, access, reference, replace/delete/orphan lifecycle.
- Hồ sơ và device endpoint có owner, quyền tối thiểu, consent/revoke và không làm lộ dữ liệu trên thiết bị dùng chung.
- Mọi lát A giữ parent task mở cho phần phụ thuộc B/C; công cụ quản lý phải thể hiện parent–child và nhánh hoãn.
- Bằng chứng không chứa bí mật/dữ liệu cá nhân thật và được đăng ký trong sổ Cổng A.

## 10. Tiêu chí hoàn thành gói Lát 3

1. 48 task/lát con được nhập một lần, có chủ thực tế và phụ thuộc.
2. Chín lát A trong bảng cắt lát không bị dùng để đóng parent toàn phần.
3. Không còn đường tự công khai học liệu, sửa trực tiếp version đang dùng, xóa tài sản còn tham chiếu hoặc ghi đè nội dung chuẩn từ phạm vi bộ.
4. Mẫu A-G03 hoàn tất cho một tập học liệu mẫu không cần AI; phần tài sản liên quan của A-G05 có bằng chứng.
5. Các task M11-T038–T040-A không bị đóng trước M11-T036 và không giả định M11-T024 đã tồn tại.

## 11. Bước tiếp theo

Lập gói Lát 4–Lát 5: resilience/health/sự cố, xuất–xóa/đối soát, nghiệm thu A-G01–A-G06 và quyết định mở hoặc không mở Giai đoạn B.
