# Kiểm tra bao phủ 145 task và checklist quyết định Cổng A

## 1. Kết quả bao phủ

| Work package | Phạm vi nguồn | Lát 0–1 | Lát 2 | Lát 3 | Lát 4–5 | Tổng duy nhất | Kết quả |
|---|---|---:|---:|---:|---:|---:|---|
| A-WP01 / M01 | M01-T001–M01-T043 | 9 | 21 | 6 | 7 | 43 | Đủ, không trùng nguồn |
| A-WP02 / M02 | M02-T001–M02-T023; M02-T029–M02-T034 | 6 | 0 | 23 | 0 | 29 | Đủ, không trùng nguồn |
| A-WP03 / M11 | M11-T001–M11-T022; M11-T027–M11-T040; M11-T043–M11-T048 | 5 | 14 | 15 | 8 | 42 | Đủ; M11-T006 dùng task kiểm chứng thay thế |
| A-WP04 / M12 | M12-T001–M12-T010; M12-T021–M12-T025; M12-T031–M12-T038; M12-T040–M12-T047 | 10 | 9 | 4 | 8 | 31 | Đủ, không trùng nguồn |
| **Tổng** |  | **30** | **44** | **48** | **23** | **145** | **Bao phủ 100%** |

Mười hai task A0 và mười task A5 là task điều phối bổ sung, không được tính vào 145 task nguồn.

## 2. Ma trận quyền sở hữu task theo gói

| Gói | Task nguồn sở hữu |
|---|---|
| Lát 0–1 | M01-T001–T009; M02-T001–T006; M11-T001, T002, T012, T018, T022; M12-T001–T005, T021, T031, T040, T042, T044 |
| Lát 2 | M01-T010–T021, T028–T032, T038–T041; M11-T003–T007, T027–T035; M12-T006–T010, T034–T035, T041, T043 |
| Lát 3A–3D | M01-T022–T027; M02-T007–T023, T029–T034; M11-T008–T011, T013–T017, T019–T021, T038–T040; M12-T022–T025 |
| Lát 4–5 | M01-T033–T037, T042–T043; M11-T036–T037, T043–T048; M12-T032–T033, T036–T038, T045–T047 |

## 3. Registry task thay thế và lát A

| Mã thực thi | Parent nguồn | Quy tắc trạng thái parent |
|---|---|---|
| M11-T006-A | M11-T006 | Parent giữ “không còn phù hợp”; task A chứng minh không có quyền khẩn cấp |
| M12-T042-A | M12-T042 | Chỉ hoàn tất data-flow danh tính/tài sản/kênh A/B; AI/speech giữ tắt |
| M12-T044-A | M12-T044 | Chỉ phạm vi tài sản A/B và REL-04 |
| M02-T008-A | M02-T008 | Parent chờ integration đầy đủ M03/M04 |
| M02-T009-A | M02-T009 | Parent chờ tiếp nhận M03/M05/M08 |
| M02-T023-A | M02-T023 | Parent chờ nghiệm thu phiên/tiến độ M03/M04 |
| M01-T022-A | M01-T022 | Parent chờ visibility cộng đồng M09 |
| M01-T023-A | M01-T023 | Parent chờ propagation M09 |
| M01-T025-A | M01-T025 | Parent chờ tiêu thụ M10 |
| M01-T026-A | M01-T026 | Parent chờ push M10/B-WP08 |
| M01-T027-A | M01-T027 | Parent chờ delivery feedback M10/B-WP08 |
| M11-T040-A | M11-T040 | Parent chờ M11-T024/B-WP07 |
| M11-T043-A | M11-T043 | Parent chờ tích hợp truyền thông M10 đầy đủ |
| M11-T047-A | M11-T047 | Parent chờ kênh người dùng M10 đầy đủ |
| M12-T047-A | M12-T047 | Parent chờ provider AI/speech/realtime chưa phát hành |
| M01-T042-A | M01-T042 | Parent chờ nghiệm thu nhánh hoãn |
| M01-T043-A | M01-T043 | Parent được cập nhật lại khi nhánh hoãn mở |

## 4. Checklist trước nghiệm thu cổng

| Câu hỏi | Kết quả | Bằng chứng/ghi chú |
|---|---|---|
| Release scope và danh sách năng lực tắt đã freeze? | [Có/Không] | [Điền] |
| 145 task đều có owner, baseline, gói, Cổng/REL và evidence requirement? | [Có/Không] | [Điền] |
| Không task nguồn nào xuất hiện ở hai gói với hai owner? | [Có/Không] | [Điền] |
| Mọi lát A giữ parent mở đúng registry? | [Có/Không] | [Điền] |
| M11-T006 không bị đánh dấu đã triển khai theo DoD cũ? | [Có/Không] | [Điền] |
| CT-01–CT-07 còn hiệu lực hoặc có evidence gỡ bỏ? | [Có/Không] | [Điền] |
| Sổ bằng chứng không chứa secret/PII thật? | [Có/Không] | [Điền] |
| Mọi finding nghiêm trọng/rất cao đã đóng và re-test? | [Có/Không] | [Điền] |

## 5. Checklist kết luận từng cổng

| Cổng | Hồ sơ mẫu | Điều kiện bắt buộc | Người xác nhận | Kết quả |
|---|---|---|---|---|
| A-G01 | A-G01 Tài khoản an toàn | State/rights, registration/login/recovery/lock/delete/session; REL-01/07 | [Điền] | [Đạt/Không đạt] |
| A-G02 | A-G02 Quản trị có kiểm soát | Least privilege, re-auth, denial, before/after audit; REL-02/07 | [Điền] | [Đạt/Không đạt] |
| A-G03 | A-G03 Học liệu mẫu | Sense/version/quality/review/publish/report/withdraw; REL-04 | [Điền] | [Đạt/Không đạt] |
| A-G04 | A-G04 Tích hợp suy giảm | Registry/failure/degradation/limiter/deadline/circuit; REL-03 | [Điền] | [Đạt/Không đạt] |
| A-G05 | A-G05 Bí mật và dữ liệu ngoài | Secret inventory/data map/redaction/asset rights; REL-01/03/04 | [Điền] | [Đạt/Không đạt] |
| A-G06 | A-G06 Sẵn sàng ứng phó | Health/SLO/alerts/maintenance/kill/playbook/exercise; REL-02/03 | [Điền] | [Đạt/Không đạt] |

## 6. Checklist REL ảnh hưởng A

| REL | Authority xác nhận | Evidence tối thiểu | Kết quả | Hạn/điều kiện |
|---|---|---|---|---|
| REL-01 | Sản phẩm và pháp lý | Thị trường/tuổi/đồng ý và hành vi thiếu consent | [Đóng/Mở] | [Điền] |
| REL-02 | An toàn hệ thống và M11 | Permission denial, re-auth, immutable audit, recovery exercise | [Đóng/Mở] | [Điền] |
| REL-03 | M12/an toàn/vận hành | Integration/secret registry, rotation, degradation tests | [Đóng/Mở] | [Điền] |
| REL-04 | Sản phẩm và pháp lý | Risk acceptance, asset scope, complaint/takedown | [Đóng/Mở] | [Điền] |
| REL-07 | M01/M11/riêng tư | Subject verification, export/delete manifest and reconciliation | [Đóng/Mở] | [Điền] |

## 7. Quy tắc quyết định

| Điều kiện | Quyết định bắt buộc |
|---|---|
| Cả sáu cổng đạt; năm REL ảnh hưởng A đóng; không finding chặn; coverage đủ | Có thể xem xét mở đúng phạm vi B đã freeze |
| Có cổng không đạt hoặc REL chưa đóng | Không mở Giai đoạn B |
| Chỉ lát A đạt nhưng parent còn nhánh hoãn | Chỉ mở phạm vi cho phép; nhánh hoãn tiếp tục tắt |
| Có ngoại lệ nằm trong danh sách không được chấp nhận | Không mở bất kể các cổng khác |
| Evidence hết hạn/không tái hiện/không có reviewer | Coi như thiếu evidence; không mở |

## 8. Liên kết làm việc

- [Gói Lát 0–Lát 1](GOI-TASK-THUC-THI-LAT-0-1-GIAI-DOAN-A.md)
- [Gói Lát 2](GOI-TASK-THUC-THI-LAT-2-GIAI-DOAN-A.md)
- [Gói Lát 3A–3D](GOI-TASK-THUC-THI-LAT-3A-3D-GIAI-DOAN-A.md)
- [Gói Lát 4–Lát 5](GOI-TASK-THUC-THI-LAT-4-5-GIAI-DOAN-A.md)
- [Bộ mẫu bằng chứng Cổng A](../05-bang-chung/cong-a/README.md)
- [Biên bản quyết định Cổng A](../05-bang-chung/cong-a/BIEN-BAN-QUYET-DINH-CONG-A.md)
