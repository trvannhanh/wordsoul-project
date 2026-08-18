# Tổng hợp sai lệch và thứ tự triển khai Giai đoạn A

## 1. Mục đích và phạm vi

Tài liệu hợp nhất kết quả đánh giá A-WP01–A-WP04 thành một thứ tự thực hiện xuyên module. Mục tiêu là đóng sáu cổng A-G01–A-G06 và năm hồ sơ phát hành ảnh hưởng trực tiếp đến Giai đoạn A, thay vì triển khai tuần tự từng module.

Nguồn đánh giá:

- [A-WP01 — Danh tính và hồ sơ](../02-modules/01-danh-tinh-va-ho-so/DANH-GIA-HIEN-TRANG-A-WP01.md)
- [A-WP02 — Nội dung từ vựng](../02-modules/02-noi-dung-tu-vung/DANH-GIA-HIEN-TRANG-A-WP02.md)
- [A-WP03 — Quản trị và vận hành](../02-modules/11-quan-tri-va-van-hanh/DANH-GIA-HIEN-TRANG-A-WP03.md)
- [A-WP04 — Tích hợp và tài sản số](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/DANH-GIA-HIEN-TRANG-A-WP04.md)

Đây là kế hoạch tài liệu và task breakdown. Tài liệu không xác nhận bất kỳ thay đổi mã nguồn nào đã được thực hiện.

## 2. Baseline Giai đoạn A

| Work package | Tổng task | Đã đáp ứng | Một phần | Chưa có | Không còn phù hợp |
|---|---:|---:|---:|---:|---:|
| A-WP01 — Danh tính và an toàn tài khoản | 43 | 0 | 24 | 19 | 0 |
| A-WP02 — Học liệu mẫu có kiểm soát | 29 | 0 | 20 | 9 | 0 |
| A-WP03 — Quản trị và vận hành nền | 42 | 0 | 28 | 13 | 1 |
| A-WP04 — Tích hợp và tài sản số nền | 31 | 0 | 19 | 12 | 0 |
| **Tổng** | **145** | **0** | **91** | **53** | **1** |

Task M11-T006 không còn phù hợp vì quyết định đã chốt không cho phép quyền tạm thời hoặc quyền khẩn cấp. Task này phải được loại có lý do hoặc đổi thành kiểm chứng rằng không tồn tại đường nâng quyền khẩn cấp; không được triển khai theo Definition of Done cũ.

## 3. Trạng thái Cổng A

| Cổng | Trạng thái | Sai lệch chính | Task tạo bằng chứng chính |
|---|---|---|---|
| A-G01 — Tài khoản an toàn | Chưa đạt | Thiếu xác minh thư/đồng ý/tuổi; khóa chưa thu hồi mọi quyền; OAuth tự ghép theo email, thiếu chống giả mạo; phiên không tách theo thiết bị | M01-T002–M01-T021; M01-T031; M12-T006–M12-T010 |
| A-G02 — Quản trị có kiểm soát | Chưa đạt | Vai trò quá rộng, có thể cấp quyền cao nhất, thiếu xác minh lại, lý do, trước/sau và audit bền vững | M01-T028–M01-T032; M01-T038–M01-T041; M11-T002–M11-T007; M11-T027–M11-T035 |
| A-G03 — Học liệu mẫu | Chưa đạt | Không có định danh nghĩa/phiên bản; người tạo tự công khai; thiếu gửi duyệt, checklist, báo cáo, thu hồi và khiếu nại | M02-T001–M02-T023; M02-T029–M02-T034; M11-T018–M11-T021; M12-T021–M12-T025 |
| A-G04 — Tích hợp suy giảm an toàn | Chưa đạt | Thiếu registry/criticality/hợp đồng lỗi; limiter fail-open và có đường bỏ qua; chưa có deadline, retry, circuit và bulkhead | M12-T001–M12-T010; M12-T031–M12-T038; M12-T045–M12-T047; M11-T036–M11-T037 |
| A-G05 — Bí mật và dữ liệu ngoài | Chưa đạt | Thiếu inventory/vòng đời bí mật và bản đồ dữ liệu; payload/response thô có thể vào log; tài sản thiếu nguồn/quyền/vòng đời | M12-T040–M12-T044; M11-T031–M11-T035; M02-T011–M02-T014 |
| A-G06 — Sẵn sàng ứng phó | Chưa đạt | Health luôn khỏe; thiếu cảnh báo, bảo trì thật, kill switch, bốn mức sự cố, playbook và diễn tập | M11-T036–M11-T037; M11-T043–M11-T048; M12-T045–M12-T047 |

Kết luận: chưa cổng nào đủ bằng chứng để mở Giai đoạn B. Một task “đáp ứng một phần” không được dùng làm bằng chứng đóng cổng cho đến khi toàn bộ Definition of Done và kịch bản xuyên module liên quan đạt.

## 4. Hồ sơ REL-01–REL-07

### 4.1. Hồ sơ ảnh hưởng trực tiếp Giai đoạn A

| Hồ sơ | Phạm vi cần xử lý | Task liên quan | Bằng chứng đóng hồ sơ | Chủ trì đề xuất | Quan hệ Cổng A |
|---|---|---|---|---|---|
| REL-01 — Tuổi, thị trường và đồng ý | Xác định điều kiện cung cấp đầy đủ chức năng cho mọi độ tuổi khi không xác minh/liên kết người giám hộ | M01-T002–M01-T009; M01-T033; M11-T001–M11-T004 | Văn bản pháp lý/sản phẩm theo thị trường và nhóm tuổi; ma trận dữ liệu/đồng ý; kịch bản từ chối hoặc hạn chế phù hợp | Sản phẩm và pháp lý; M01 cung cấp dữ liệu, M11 cung cấp bằng chứng | Chặn A-G01 và phát hành A/B |
| REL-02 — Quyền và audit bù trừ | Chứng minh quyền tối thiểu, xác minh lại và audit bất biến đủ bù cho việc không duyệt hai người/không quyền khẩn cấp | M01-T028–M01-T032; M01-T038–M01-T041; M11-T002–M11-T005; M11-T007; M11-T027–M11-T035; M11-T048 | Ma trận quyền, kiểm thử từ chối, bằng chứng xác minh lại, audit trước/sau không mất và diễn tập khôi phục | An toàn hệ thống và M11 | Chặn A-G02, A-G06 |
| REL-03 — Bí mật và tích hợp | Kiểm kê bí mật/tích hợp, xử lý bí mật nghi lộ, chốt chủ, hạn mức, failure mode và suy giảm | M12-T001–M12-T005; M12-T010; M12-T031–M12-T038; M12-T040–M12-T043; M12-T045–M12-T047; M11-T033–M11-T037; M11-T043–M11-T048 | Registry được duyệt; inventory không chứa giá trị bí mật; biên bản thu hồi/xoay vòng; kiểm thử timeout, quota, outage và kill switch | M12, an toàn hệ thống và vận hành | Chặn A-G04, A-G05, A-G06 |
| REL-04 — Quyền sử dụng tài sản | Chốt mức chấp nhận rủi ro khi không bắt buộc bằng chứng nguồn/quyền và thiết lập cơ chế gỡ nhanh | M02-T011–M02-T014; M02-T029–M02-T034; M12-T021–M12-T025; M12-T042–M12-T044 | Văn bản chấp nhận rủi ro; phạm vi tài sản được phép; đầu mối khiếu nại; quy trình tạm ẩn/gỡ; bằng chứng diễn tập | Sản phẩm và pháp lý; M02/M12 vận hành | Chặn A-G03, A-G05 đối với tài sản thiếu quyền rõ |
| REL-07 — Xuất và xóa dữ liệu | Thực hiện yêu cầu tự phục vụ/hỗ trợ có xác minh chủ thể, thời hạn và đối soát liên module | M01-T033–M01-T037; M01-T041–M01-T043; M11-T027–M11-T035; M11-T038–M11-T040 | Kịch bản yêu cầu–xác minh–xuất/xóa–đối soát; manifest kết quả; bằng chứng xóa/ẩn danh; báo cáo phần thất bại và chạy lại | M01 và M11; mọi module dữ liệu xác nhận | Chặn A-G01/A-G02; bắt buộc trước phát hành đầy đủ |

### 4.2. Hồ sơ thuộc Giai đoạn B

| Hồ sơ | Xử lý trong Giai đoạn A | Thời điểm đóng chính |
|---|---|---|
| REL-05 — Loại bỏ AP | Ngăn mở rộng phụ thuộc mới vào AP; ghi nhận thao tác điều chỉnh AP hiện tại là sai lệch; chưa thực hiện chuyển đổi | Trước B-G03 và trước mọi chuyển đổi dữ liệu M06 |
| REL-06 — Mặc định nhận thông báo | Bảo đảm mô hình tuổi/thị trường/đồng ý của REL-01 có thể làm đầu vào; chưa bật kênh tùy chọn | Trước B-G05 và trước phát hành email/push tùy chọn |

REL-05 và REL-06 không được dùng để trì hoãn việc đóng Cổng A, nhưng cũng không được đánh dấu hoàn thành từ đầu ra Giai đoạn A.

## 5. Kiểm soát tạm thời trước khi triển khai

Các kiểm soát dưới đây là điều kiện vận hành tạm thời, không thay thế task nguồn:

| Mã | Kiểm soát tạm thời | Lý do | Điều kiện gỡ bỏ |
|---|---|---|---|
| CT-01 | Không mở công khai nội dung người dùng hoặc AI tạo | Chưa có gửi duyệt, checklist, báo cáo và thu hồi | A-G03 đạt |
| CT-02 | Không coi email trùng là đủ bằng chứng liên kết tài khoản ngoài | Luồng hiện tại có nguy cơ ghép sai/chiếm tài khoản | M01-T014–M01-T015 và M12-T007–M12-T009 đạt |
| CT-03 | Không dùng log payload/response thô làm bằng chứng vận hành hợp lệ | Có nguy cơ chứa token và dữ liệu cá nhân | M11-T031–M11-T035 và M12-T040–M12-T043 đạt |
| CT-04 | Không coi health hiện tại là bằng chứng dịch vụ khỏe | Điểm health không kiểm tra phụ thuộc thật | M11-T036, M12-T045 đạt |
| CT-05 | Không cho đường “trusted/internal” bỏ qua giới hạn lưu lượng | Trái quyết định đã chốt và có thể mở đường lạm dụng | M12-T034–M12-T035 đạt |
| CT-06 | Không mở tích hợp AI/giọng nói trong phạm vi A/B | Chưa có đầy đủ dữ liệu ngoài, consent, quota, suy giảm và kiểm thử | Chỉ mở ở giai đoạn đã định sau khi cổng tương ứng đạt |
| CT-07 | Không thêm hành vi mới dựa trên AP | AP đã được quyết định loại bỏ nhưng vẫn còn đường điều chỉnh trực tiếp | REL-05 đạt trong Giai đoạn B |

## 6. Thứ tự triển khai xuyên module

### 6.1. Sơ đồ phụ thuộc

```text
Lát 0: kiểm soát tạm thời + chủ hồ sơ REL
                         |
                         v
Lát 1: từ điển + registry + nguồn sự thật + phân loại dữ liệu
                         |
              +----------+----------+
              v                     v
Lát 2: danh tính/quyền/audit   Lát 3: học liệu/tài sản/phiên bản
              +----------+----------+
                         v
Lát 4: resilience + health + sự cố + phục hồi
                         |
                         v
Lát 5: xuất/xóa/đối soát + nghiệm thu A-G01–A-G06
```

Lát 2 và Lát 3 có thể chạy song song sau khi các hợp đồng ở Lát 1 được duyệt. Task được nhắc ở nhiều hồ sơ chỉ thực hiện một lần; mỗi hồ sơ REL dùng chung cùng bộ bằng chứng.

### 6.2. Danh sách lát triển khai

| Thứ tự | Lát triển khai | Task nguồn chính | Đầu ra bắt buộc | Phụ thuộc | Cổng/REL |
|---:|---|---|---|---|---|
| 0 | Chặn mở rộng rủi ro và giao chủ hồ sơ | CT-01–CT-07; mở hồ sơ REL-01–REL-04, REL-07 | Phạm vi tắt/giới hạn được ghi nhận; chủ, người xác nhận, bằng chứng cần nộp và điều kiện hết hiệu lực | Không | Toàn bộ Cổng A |
| 1 | Từ điển và sổ đăng ký nền | M01-T001–M01-T009; M02-T001–M02-T006; M11-T001–M11-T002; M11-T012; M11-T018; M11-T022; M12-T001–M12-T005; M12-T021; M12-T031; M12-T040; M12-T042; M12-T044 | Thuật ngữ, chủ sở hữu dữ liệu, định danh nghĩa, registry tích hợp/tài sản/trạng thái dùng chung/bí mật/dữ liệu ngoài và mức quan trọng | Lát 0; đầu vào pháp lý REL-01/REL-04 có thể chạy song song | REL-01, REL-03, REL-04 |
| 2 | Biên an toàn danh tính và quản trị | M01-T010–M01-T021; M01-T028–M01-T032; M01-T038–M01-T041; M11-T003–M11-T007 (loại/viết lại T006); M11-T027–M11-T035; M12-T006–M12-T010; M12-T034–M12-T035; M12-T041; M12-T043 | Trạng thái tài khoản cưỡng chế, phiên theo thiết bị, OAuth an toàn, quyền tối thiểu, xác minh lại, support case, audit bền vững, log được che và limiter không bypass | Registry/thuật ngữ Lát 1 | A-G01, A-G02, A-G05; REL-02, REL-03 |
| 3A | Phiên bản hóa cấu hình và thay đổi quản trị | M11-T008–M11-T017; M11-T020–M11-T021; M11-T038–M11-T040 | Yêu cầu thay đổi, phiên bản bất biến, lịch hiệu lực, xem trước, tham chiếu, rollback, đồng thời và đối soát | Quyền/audit Lát 2 | A-G02, A-G06; REL-02, REL-07 |
| 3B | Học liệu mẫu có kiểm duyệt | M02-T007–M02-T010; M02-T015–M02-T023; M02-T029–M02-T034; M11-T019–M11-T021 | Vòng đời/phiên bản mục từ và bộ, snapshot cho phiên học, quyền, gửi duyệt, checklist, xuất bản, báo cáo, thu hồi và khiếu nại | Định danh nghĩa Lát 1; quyền/audit Lát 2; quy tắc thay đổi Lát 3A | A-G03; REL-02, REL-04 |
| 3C | Tài sản số có vòng đời | M01-T024; M02-T011–M02-T014; M12-T022–M12-T025 | Upload an toàn, metadata bất biến, nguồn/trạng thái/quyền truy cập, thay thế/xóa/orphan và liên kết phiên bản học liệu | Asset catalog Lát 1; quyền/audit Lát 2; học liệu Lát 3B | A-G03, A-G05; REL-04 |
| 3D | Hồ sơ và thiết bị người dùng | M01-T022–M01-T027 | Quyền xem/sửa hồ sơ, tên/ảnh, múi giờ, giờ học và nhiều thiết bị nhận tin có thu hồi | Trạng thái/phiên Lát 2; tài sản Lát 3C cho ảnh đại diện | A-G01; đầu vào REL-06 |
| 4 | Chống lỗi và sẵn sàng ứng phó | M11-T036–M11-T037; M11-T043–M11-T048; M12-T032–M12-T033; M12-T036–M12-T038; M12-T045–M12-T047 | Cache/lock an toàn, deadline, retry, circuit/bulkhead, SLO/health thật, cảnh báo, bảo trì, kill switch, bốn mức sự cố, playbook và diễn tập | Registry Lát 1; quyền/audit Lát 2; thay đổi kiểm soát Lát 3A | A-G04, A-G06; REL-02, REL-03 |
| 5A | Xuất, xóa và đối soát dữ liệu | M01-T033–M01-T037; M11-T027–M11-T035; M11-T038–M11-T040 | Bản đồ dữ liệu, yêu cầu tự phục vụ/hỗ trợ, xác minh chủ thể, manifest xuất, xóa/ẩn danh, retry phần lỗi và đối soát liên module | Mô hình dữ liệu Lát 1; quyền/audit Lát 2; công việc nền Lát 3A; mọi module xác nhận ma trận | A-G01, A-G02; REL-07 |
| 5B | Nghiệm thu và bàn giao Cổng A | M01-T042–M01-T043; bằng chứng cổng từ A-WP02/A-WP03/A-WP04 | Bộ kịch bản A-G01–A-G06, kết quả đạt/không đạt, liên kết bằng chứng, sai lệch còn lại, người chấp nhận và quyết định mở/không mở B | Lát 1–5A; hồ sơ REL ảnh hưởng cổng phải đóng | A-G01–A-G06; REL-01–REL-04, REL-07 |

## 7. Task chặn Cổng A ưu tiên cao nhất

Các cụm dưới đây phải được đưa lên đầu hàng đợi trong lát tương ứng vì hiện trạng có đường gây rủi ro trực tiếp:

| Ưu tiên | Cụm task | Rủi ro cần đóng | Điều kiện tối thiểu |
|---:|---|---|---|
| 1 | M11-T031–M11-T035; M12-T040–M12-T043 | Lưu/lộ token, dữ liệu cá nhân và phản hồi thô; mất audit | Log theo allowlist và che dữ liệu; audit không mất; inventory/vòng đời bí mật và bản đồ dữ liệu được duyệt |
| 2 | M01-T010–M01-T018; M01-T031; M12-T006–M12-T010 | Tài khoản khóa vẫn truy cập; OAuth giả mạo/tự ghép; token/phiên không thu hồi đúng | Trạng thái được cưỡng chế ở mọi đường; OAuth chống giả mạo/phát lại; không tự ghép theo email; phiên theo thiết bị |
| 3 | M01-T028–M01-T030; M11-T002–M11-T007 | Cấp quyền quá mức và thiếu kiểm soát thao tác nhạy cảm | Ma trận quyền tối thiểu; không tự cấp quyền cao nhất; xác minh lại; T006 bị loại/viết lại đúng quyết định |
| 4 | M02-T007–M02-T010; M02-T017; M02-T029–M02-T034 | Nội dung không phiên bản và tự công khai | Phiên bản bất biến; chủ không tự xuất bản; gửi duyệt/checklist/báo cáo/thu hồi/khiếu nại hoạt động |
| 5 | M02-T002–M02-T004; M02-T022–M02-T023 | Hợp nhất sai nghĩa và thay nội dung giữa phiên | Định danh nghĩa; trùng theo nghĩa; ghi đè có phạm vi; snapshot phiên học |
| 6 | M12-T021–M12-T025; M02-T011–M02-T014 | Tài sản không nguồn, không quyền và không vòng đời | Catalog/metadata/quyền/trạng thái; thay thế/xóa/orphan có tham chiếu và audit |
| 7 | M12-T034–M12-T038 | Limiter fail-open/bypass; lỗi provider lan rộng | Failure mode theo năng lực; không bypass; deadline/retry/circuit/bulkhead có kiểm thử |
| 8 | M11-T036–M11-T037; M11-T043–M11-T048; M12-T045–M12-T047 | Health sai và không thể phát hiện/khống chế/khôi phục | Health thật, SLO, cảnh báo, kill switch, playbook và diễn tập có kết quả |
| 9 | M01-T033–M01-T037; M11-T027–M11-T040 | Xóa vật lý thiếu yêu cầu/đối soát; tra cứu hỗ trợ quá rộng | Vụ việc và xác minh chủ thể; xuất/xóa có manifest, thời hạn, ẩn danh, retry và đối soát |

## 8. Quy tắc tạo task triển khai

Khi đưa task vào Jira, Trello hoặc Notion:

- Giữ nguyên Task ID nguồn; thêm trường `Lát Giai đoạn A`, `Cổng A`, `REL`, `Chủ trì`, `Bằng chứng`, `Trạng thái baseline` và `Điều kiện chặn`.
- Một task liên quan nhiều REL chỉ có một bản triển khai; các REL cùng tham chiếu bộ bằng chứng đó.
- Không đánh dấu hoàn thành chỉ vì đã có một phần hành vi. Phải đáp ứng Definition of Done, kiểm thử từ chối/lỗi/đồng thời và bằng chứng vận hành liên quan.
- Task có thay đổi ranh giới dữ liệu phải được module nguồn và module tiêu thụ cùng xác nhận.
- Task thuộc ngoại lệ A/B chỉ được hoàn thành cho lát đã nêu; nhánh hoãn vẫn giữ trạng thái chưa làm.
- Mọi task chạm dữ liệu, quyền, bí mật, audit hoặc thu hồi phải có kịch bản quay lui/khôi phục và người xác nhận kết quả.

## 9. Điều kiện ra quyết định mở Giai đoạn B

Chỉ đề xuất mở Giai đoạn B khi đồng thời thỏa mãn:

1. A-G01–A-G06 đều có kết quả đạt và liên kết bằng chứng.
2. REL-01–REL-04 và REL-07 đã đóng hoặc có văn bản chấp nhận rõ ràng đúng thẩm quyền; không dùng chấp nhận chung chung thay cho kiểm soát bắt buộc.
3. Không còn đường tự công khai nội dung, tự ghép tài khoản theo email, bỏ qua giới hạn, ghi payload thô, cấp quyền cao nhất trái chính sách hoặc báo health giả.
4. Kịch bản xuyên module cho khóa tài khoản, thu hồi phiên, thay đổi/thu hồi học liệu, outage tích hợp, mất audit, xuất/xóa dữ liệu và sự cố trọng yếu đều đạt.
5. Sai lệch còn lại không nằm trên đường phát hành B, có chủ, hạn xử lý, mức rủi ro và quyết định chấp nhận được ghi nhận.

Nếu một điều kiện trên chưa đạt, quyết định mặc định là chưa mở Giai đoạn B.
