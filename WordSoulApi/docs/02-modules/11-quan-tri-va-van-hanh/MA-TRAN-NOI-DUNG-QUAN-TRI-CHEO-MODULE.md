# Ma trận nội dung quản trị chéo module M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CROSS-CONTENT-MATRIX-1.0` |
| Task | M11-T018 |
| Đầu vào | M11-ACTION-1.0, M11-ROLE-1.0, M11-PERM-1.0, M11-ENHANCED-CONTROL-1.0, M11-CHANGE-REQUEST-1.0, REL-04 |
| Phạm vi | Ranh giới trách nhiệm, thẩm quyền tác động và kiểm soát quản trị chéo 12 module M01–M12 |
| Tự kiểm | A-G02, A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Nguyên tắc ranh giới và sở hữu dữ liệu

Ma trận nội dung quản trị chéo module (Cross-Module Content Matrix) xác lập ranh giới trách nhiệm và thẩm quyền giữa Module Quản trị M11 và 11 Module nghiệp vụ chuyên biệt (M01–M10, M12).

- **Module nguồn sở hữu Truth**: Module nguồn sở hữu domain model, quy tắc kiểm tra tính hợp lệ (`business validation`), trạng thái bền vững (`durable state`) và quy tắc chuyển trạng thái hợp lệ. M11 KHÔNG tự ý ghi đè DB entity ngoài ranh giới API/Contract đã chốt.
- **M11 sở hữu Workflow & Control**: M11 sở hữu catalog hành động quản trị (44 Action ID thuộc `M11-ACTION-1.0`), ma trận quyền tối thiểu (`M11-PERM-1.0`), quy trình duyệt yêu cầu thay đổi (`M11-CHANGE-REQUEST/DECISION-1.0`), xem trước/mô phỏng (`M11-PREVIEW-1.0`), triển khai giới hạn (`M11-CANARY-ROLLOUT-1.0`) và nhật ký kiểm toán bất biến.
- **Ranh giới giao dịch chéo module (`Change Set`)**: Thay đổi ảnh hưởng nhiều module bắt buộc được đóng gói thành một `Change Set` có thứ tự thực thi, kiểm tra phụ thuộc và kịch bản đền bù (`Compensating Operation`) nếu xảy ra lỗi một phần.
- **Tuân thủ Bản quyền & Quyền tài sản số (`REL-04 / A-G03 / A-G05`)**: Mọi thao tác quản trị xuất bản hoặc sửa đổi tài sản số (âm thanh, hình ảnh mục từ, bài học, nhiệm vụ) bắt buộc phải kiểm tra trạng thái xác minh quyền tài sản số (REL-04). M11 KHÔNG ĐƯỢC phép tự ý công khai tài sản chưa duyệt bản quyền (CT-01).

## 2. Ma trận trách nhiệm chéo 12 Module (M01 – M12)

| Module | Miền nội dung / Thực thể nguồn | Action ID M11 ánh xạ | M11 được làm (`Allowed Operations`) | M11 không được làm (`Disallowed Operations`) | Hợp đồng / Bằng chứng bắt buộc | Trạng thái ranh giới |
|---|---|---|---|---|---|---|
| **M01** | Danh tính, Hồ sơ, Tài khoản, Quyền người dùng | `ACT-M01-01` .. `ACT-M01-05` | Khóa/Mở tài khoản, điều chỉnh vai trò quản trị, thu hồi phiên, xem lịch sử danh tính có quyền | Tự tạo/sửa password, bypass verification email, tự mở tài khoản vi phạm chính sách abuse | `M01-SESSION-1.0`, `M01-ABUSE-1.0`, `M11-GRANT-1.0` | Đã chốt hợp đồng |
| **M02** | Mục từ, Từ điển, Bộ từ vựng, Hình ảnh/Âm thanh | `ACT-M02-01` .. `ACT-M02-06` | Phê duyệt/từ chối bản thảo mục từ, ẩn/hiện bộ từ, gán nhãn độ khó, thay thế tài sản lỗi | Tự sửa nội dung mục từ đang phát hành trực tiếp ngoài workflow duyệt, phát hành tài sản thiếu REL-04 | `M02-QUALITY-1.0`, `REL-04`, `M11-CHANGE-REQUEST-1.0` | Đã chốt hợp đồng |
| **M03** | Loại phiên học, Quy tắc thời lượng, Cấu hình lượt học | `ACT-M03-01` .. `ACT-M03-03` | Điều chỉnh `WordsPerSession`, cấu hình loại phiên học, lên lịch áp dụng policy phiên học | Thay đổi dữ liệu lịch sử phiên học đã hoàn thành của người dùng, sửa kết quả học tập | `M03-SESSION-POLICY-1.0`, `M11-CONFIG-VERSION-1.0` | Đã chốt hợp đồng |
| **M04** | Tiến độ ôn tập, Thuật toán Lặp lại ngắt quãng (SRS) | `ACT-M04-01` .. `ACT-M04-03` | Cấu hình tham số thuật toán SRS (`SrsPolicyVersion`), điều phối rollout phiên bản thuật toán | Tự sửa trực tiếp điểm `nextReviewAt` hoặc master level của từng cá nhân ngoài quy trình reconcile | `M04-SRS-POLICY-1.0`, `M11-PREVIEW-1.0` | Đã chốt hợp đồng |
| **M05** | Ngưỡng đánh giá phát âm, Nhận dạng giọng nói | `ACT-M05-01` .. `ACT-M05-02` | Cấu hình ngưỡng điểm đạt phát âm, tắt/bật provider phát âm theo chính sách D-010 | Lưu trữ âm thanh người dùng thô, tự bật AI giọng nói khi chưa đạt REL-01/REL-03 | `D-010`, `M12-CAP-REG-1.0`, `M11-ENHANCED-CONTROL-1.0` | Đã chốt hợp đồng |
| **M06** | Sổ cái tài sản, Vật phẩm, Số dư XP/AP, Giao dịch | `ACT-M06-01` .. `ACT-M06-05` | Điều chỉnh số dư có vụ việc/lý do, cấu hình giá vật phẩm, đối soát sai lệch | Tự sửa số dư ngoài bút toán sổ cái, cấp/tiêu AP mới (D-011), sửa lịch sử giao dịch | `D-011`, `M06-LEDGER-1.0`, `M11-ENHANCED-CONTROL-1.0` | Đã chốt hợp đồng |
| **M07** | Nhiệm vụ, Thành tựu, Phòng thử thách, Phần thưởng | `ACT-M07-01` .. `ACT-M07-04` | Quản lý vòng đời nhiệm vụ, lên lịch chiến dịch thành tựu, vô hiệu hóa thử thách vi phạm | Sửa điều kiện nhận thưởng của nhiệm vụ đã kết thúc, cấp bù phần thưởng không qua reconcile | `M07-QUEST-POLICY-1.0`, `M11-CHANGE-SCHEDULE-1.0` | Đã chốt hợp đồng |
| **M08** | Quy tắc thi đấu PvP, Phòng đấu, Elo/Rating | `ACT-M08-01` .. `ACT-M08-03` | Cấu hình matchmaking timeout, tạm dừng phòng đấu sự cố, điều phối Elo rating policy | Sửa tỷ số trận đấu đã kết thúc, can thiệp kết quả trận đấu đang diễn ra | `M08-BATTLE-POLICY-1.0`, `M11-CANARY-ROLLOUT-1.0` | Đã chốt hợp đồng |
| **M09** | Bảng xếp hạng, Nhóm học tập, Cuộc thi cộng đồng | `ACT-M09-01` .. `ACT-M09-03` | Ẩn/Hiện nhóm vi phạm chuẩn mực, cấu hình chu kỳ bảng xếp hạng, ẩn thông tin PII | Công khai danh sách PII người dùng (Top Active/XP), sửa điểm xếp hạng cá nhân | `M09-COMMUNITY-1.0`, `M11-METRIC-DICT-1.0` (MP-03) | Đã chốt hợp đồng |
| **M10** | Mẫu thông báo, Chiến dịch Push/Email, Lịch gửi tin | `ACT-M10-01` .. `ACT-M10-04` | Phê duyệt nội dung mẫu thông báo, dừng chiến dịch gửi tin khẩn cấp (Kill Switch) | Bỏ qua consent nhận tin của người dùng, gửi tin nhắn rác không qua rate limiter | `M10-NOTIF-POLICY-1.0`, `M01-CONS-1.0`, `M12-RATE-1.0` | Đã chốt hợp đồng |
| **M11** | Cấu hình hệ thống, Vai trò Quản trị, Job, Audit Log | `ACT-M11-01` .. `ACT-M11-08` | Quản lý registry cấu hình, phân quyền vai trò, quản lý job nền, tra cứu audit log | Tự cấp đặc quyền khẩn cấp/tạm thời (D-037), xóa cứng audit log, sửa log history | `D-037`, `M11-PERM-1.0`, `M11-CONFIG-VERSION-1.0` | Đã chốt hợp đồng |
| **M12** | Capability tích hợp, Provider, Secret, Rate Limiter | `ACT-M12-01` .. `ACT-M12-04` | Cấu hình rate limit policies, điều phối fail-open/fail-closed, xoay vòng secret ref | Hiển thị/Export secret thô ra log/UI, bypass rate limit cho workload nội bộ trái CT-05 | `CT-05`, `M12-RATE-1.0`, `M12-FAIL-1.0`, `D-008` | Đã chốt hợp đồng |

## 3. Quy tắc Quản lý Thay đổi Chéo Module (Cross-Module Change Sets)

Khi một yêu cầu thay đổi quản trị ảnh hưởng đến từ 2 module trở lên (vd: Thêm một Bài học mới M02 đòi hỏi cập nhật Nhiệm vụ M07 và Thông báo M10):

1. **Đóng gói Change Set bất biến**: Toàn bộ các thay đổi thành phần được gán chung một `changeSetId` và `changeSetDigest`.
2. **Kiểm tra phụ thuộc thứ tự (`Dependency Ordering`)**:
   - Bước 1: Tạo và kiểm duyệt nội dung M02 (đạt REL-04).
   - Bước 2: Tạo cấu hình Nhiệm vụ M07 trỏ tới Version ID của M02.
   - Bước 3: Tạo Mẫu thông báo M10 trỏ tới Nhiệm vụ M07.
3. **Thực thi Giao dịch và Xử lý Lỗi một phần (`Partial Failure Protocol`)**:
   - Thực thi theo cơ chế Pipeline có kiểm soát (T011).
   - Nếu Bước 1 & 2 thành công nhưng Bước 3 thất bại: Lập tức kích hoạt quy trình Rollback/Compensate cho Bước 1 & 2. TUYỆT ĐỐI KHÔNG đánh dấu Change Set là `SUCCESS` khi có bất kỳ bước thành phần nào thất bại.

## 4. Kiểm soát Quyền Tài sản số và Bản quyền (REL-04 / A-G03 / A-G05)

Mọi nội dung liên quan đến tài sản số (hình ảnh minh họa mục từ, file âm thanh phát âm, banner bài học, Quest icon) khi được điều chỉnh qua M11 phải qua 3 cổng kiểm soát:
1. **G03-C01 / CT-01**: Cấm công khai nội dung chưa duyệt bản quyền.
2. **G05-L01**: Không lưu trữ file phương tiện thô chứa bí mật hoặc metadata nhạy cảm.
3. **REL-04 License Check**: Bản ghi tài sản số phải gắn `licenseId` và `rightsCleared = true` trước khi M11 Action `ACT-M02-PUBLISH` được phép phê duyệt.

## 5. Quy tắc Giải quyết Xung đột (Conflict Resolution Protocol)

- **Owner Validation Superiority**: Nếu M11 Action bị Module nguồn reject do vi phạm quy tắc nghiệp vụ nội tại (vd: M06 reject điều chỉnh số dư do tài khoản đang bị khóa tranh chấp), M11 phải tuân thủ và chuyển trạng thái Change Request thành `REJECTED_BY_SOURCE_OWNER`.
- **Concurrent Change Locking**: Hai Admin Action cùng thao tác trên 1 tài nguyên chéo module phải thông qua cơ chế khóa phân tán CAS (T010). Lệnh đến sau nhận phản hồi `CONCURRENCY_CONFLICT` và bắt buộc rebase dữ liệu.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `XC-G01` | Ma trận bao phủ 100% 12 module M01–M12 với phân định rõ Allowed/Disallowed Operations. |
| `XC-G02` | M11 không sở hữu hoặc tự ý sửa đổi durable entity state của các module nguồn M01–M10, M12. |
| `XC-G03` | Change Set chéo module bắt buộc thực thi giao dịch có kiểm soát và rollback toàn bộ nếu lỗi một phần. |
| `XC-G04` | 100% thao tác xuất bản tài sản số M02/M05/M07 qua M11 phải verify bản quyền REL-04 và CT-01. |
| `XC-G05` | Thao tác điều chỉnh số dư M06 qua M11 bắt buộc có lý do vụ việc, hồ sơ audit và không chạm vào AP (D-011). |
| `XC-G06` | Thao tác M01 qua M11 không được phép tự tạo/sửa password hoặc bypass email verification (D-012/D-014). |
| `XC-G07` | Thao tác M10 qua M11 tuyệt đối tuân thủ consent người dùng M01-CONS-1.0 và rate limit CT-05. |
| `XC-G08` | Thao tác M12 qua M11 không được lộ secret thô ra UI/Log (D-008). |
| `XC-G09` | Phân quyền và kiểm soát tăng cường tuân thủ nghiêm ngặt M11-PERM-1.0 và M11-ENHANCED-CONTROL-1.0. |
| `XC-G10` | 100% các test case tự kiểm XC18-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `XC18-01` | M11 phê duyệt phát hành bộ từ M02 có đầy đủ bản quyền REL-04 | Phê duyệt thành công, chuyển trạng thái `published` |
| `XC18-02` | Thử dùng M11 xuất bản hình ảnh mục từ M02 chưa xác minh bản quyền (CT-01) | Reject request với lỗi `ASSET_RIGHTS_NOT_CLEARED_REL04` |
| `XC18-03` | M11 điều chỉnh số dư XP cho người dùng M06 có lý do hỗ trợ | Tạo bút toán điều chỉnh trong M06, ghi log audit M11 |
| `XC18-04` | Cố tình dùng M11 Action cấp thêm điểm AP cho người dùng (D-011) | Reject request với lỗi `AP_FROZEN_D011` |
| `XC18-05` | M11 khóa tài khoản người dùng M01 do vi phạm abuse | Gọi M01 Service thu hồi toàn bộ phiên đăng nhập lập tức |
| `XC18-06` | Thử dùng M11 xem mật khẩu thô hoặc đổi mật khẩu người dùng M01 | Reject request với lỗi `DIRECT_CREDENTIAL_MUTATION_FORBIDDEN` |
| `XC18-07` | M11 khởi tạo Change Set chéo module M02-M07-M10 hợp lệ | Đóng gói Change Set, thực thi theo đúng thứ tự |
| `XC18-08` | Change Set M02-M07-M10 bị lỗi ở bước M10 | Tự động kích hoạt rollback đền bù cho bước M02 & M07 |
| `XC18-09` | M11 điều chỉnh tham số thuật toán SRS M04 `WordsPerSession` | Tạo version mới `SrsPolicyVersion`, không sửa history phiên học |
| `XC18-10` | Cố tình bật AI phát âm M05 khi tính năng đang bị tắt theo D-010 | Reject request với lỗi `AI_VOICE_DISABLED_D010` |
| `XC18-11` | M11 dừng khẩn cấp chiến dịch thông báo M10 | Kích hoạt Kill Switch, dừng lập tức các worker đang gửi tin |
| `XC18-12` | Thử dùng M11 gửi thông báo M10 tới người dùng đã Withdraw Consent | Reject request với lỗi `USER_CONSENT_WITHDRAWN` |
| `XC18-13` | M11 xoay vòng secret reference trong M12 | Cập nhật secret reference, secret thô không xuất hiện trong log |
| `XC18-14` | Hai Admin cùng chỉnh sửa 1 phòng đấu M08 chéo module | Giao dịch CAS khóa xung đột, Admin đến sau nhận 409 Conflict |
| `XC18-15` | M11 ẩn nhóm học tập M09 vi phạm | Ẩn nhóm thành công, danh sách thành viên không rò rỉ PII |
| `XC18-16` | Module nguồn M06 reject lệnh điều chỉnh do tài khoản bị phong tỏa | M11 cập nhật Change Request state là `REJECTED_BY_SOURCE_OWNER` |
| `XC18-17` | M11 cấu hình tỷ lệ thắng/thua cho trận đấu PvP M08 | Deny operation với lỗi `BATTLE_RESULT_MUTATION_FORBIDDEN` |
| `XC18-18` | Kiểm tra tính bất biến của hợp đồng Ma trận Chéo module v1.0 | API deny mọi thao tác chỉnh sửa ma trận ngoài workflow |
| `XC18-19` | Thực thi 50 Change Set chéo module đồng thời | Hệ thống xử lý song song, các giao dịch đền bù chạy chính xác |
| `XC18-20` | Tra cứu log audit cho thao tác chéo module | Log hiển thị đầy đủ traceId, sourceModule, targetModule và changeSetId |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-XC-I01` | Trong source `WordSoulApi`, các Controller/Service đang gọi trực tiếp DbContext để chỉnh sửa entity của module khác | Vi phạm ranh giới Module Owner và không qua Cross-Module Matrix | M11-T049 |
| `M11-XC-I02` | Chưa có cơ chế đóng gói `Change Set` chéo module và xử lý rollback đền bù một phần | Rủi ro dữ liệu bị lệch trạng thái giữa các module khi có lỗi | M11-T049 |
| `M11-XC-I03` | Thao tác quản trị hiện tại chưa kiểm tra trạng thái xác minh bản quyền REL-04 trước khi public | Rủi ro vi phạm bản quyền nội dung (CT-01 / REL-04) | M02 tasks; M11-T049 |
| `M11-XC-I04` | Thiếu sự kết nối giữa M11 Change Execution với quy tắc từ chối của Module nguồn | M11 có thể áp đặt thay đổi trái với business logic nguồn | M11-T049 |
| `M11-XC-I05` | Chưa có hệ thống log audit ghi nhận thông tin `changeSetId` và `sourceModule` | Khó khăn khi điều tra sự cố giao dịch chéo module | M11-T049 |

- `M11-XC-F01`: Triển khai `CrossModuleChangeOrchestrator` quản lý Change Set (tiếp nhận: M11-T049).
- `M11-XC-F02`: Thiết lập API Contract ranh giới giữa M11 và 11 Module nguồn M01–M10, M12 (tiếp nhận: M11-T049).
- `M11-XC-F03`: Tích hợp bộ kiểm tra bản quyền REL-04 cho các thao tác xuất bản M02/M05/M07 (tiếp nhận: M02 tasks; M11-T049).
- `M11-XC-F04`: Thiết lập bộ kiểm thử tự động XC-G01–G10 và XC18-01–20 (tiếp nhận: M11-T049).
- `M11-XC-F05`: Thu thập bằng chứng runtime cho luồng giao dịch chéo module (tiếp nhận: M11-T049; A-G02/A-G03/REL-04).

## 8. Tự kiểm M11-T018

- Đã nâng cấp `M11-CROSS-CONTENT-MATRIX-1.0` phủ đủ 12 module M01–M12 với quy định rõ Allowed/Disallowed Operations.
- Đã chốt cơ chế đóng gói `Change Set` chéo module và giao thức xử lý lỗi một phần / rollback đền bù.
- Đã lồng ghép chặt chẽ các cổng kiểm soát bản quyền tài sản số REL-04, CT-01 và A-G03/A-G05.
- Đã xác lập 10 Regression Gates (`XC-G01`–`XC-G10`) và 20 Test Cases tự kiểm (`XC18-01`–`XC18-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo ma trận ranh giới M01–M12 và quy tắc xung đột dự thảo | Chưa gán |
| 2026-08-20 | 1.0 | Khởi tạo hoàn chỉnh đặc tả ma trận nội dung quản trị chéo module M11-T018 | WSA-7K2 |
