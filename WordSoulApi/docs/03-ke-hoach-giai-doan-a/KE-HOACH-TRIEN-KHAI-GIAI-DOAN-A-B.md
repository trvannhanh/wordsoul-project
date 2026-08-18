# Kế hoạch triển khai Giai đoạn A–B

## 1. Mục đích

Tài liệu này chuyển phân kỳ nghiệp vụ của WordSoul thành phạm vi task có thể lập kế hoạch thực hiện. Tài liệu chỉ tổ chức các task đã có, không thay thế nội dung chi tiết trong `TASK-BACKLOG.md` của từng module và không xác nhận rằng mã nguồn hiện tại đã đáp ứng các task.

Trạng thái “được chọn” trong tài liệu có nghĩa là task được đưa vào phạm vi lập kế hoạch. Task chỉ được coi là hoàn thành khi đáp ứng Definition of Done tại backlog nguồn và cung cấp đủ bằng chứng qua cổng giai đoạn.

## 2. Kết quả chọn phạm vi

| Chỉ số | Kết quả |
|---|---:|
| Tổng task toàn hệ thống | 561 |
| Task Giai đoạn A | 145 |
| Task Giai đoạn B | 202 |
| Tổng task A–B | 347 |
| Task dành cho Giai đoạn C–E | 214 |
| Quyết định nghiệp vụ còn mở | 0 |

Các năng lực không thuộc A–B gồm tạo học liệu bằng AI, chấm phát âm sâu, tiến hóa và hiệu ứng thú cưng, thành tựu dài hạn, chiến dịch truyền thông nâng cao, nhóm, xếp hạng, PvP và tối ưu đa nhà cung cấp. Những năng lực này phải được giữ ở trạng thái không hoạt động nếu phần bảo vệ tương ứng chưa hoàn tất.

## 3. Giai đoạn A — Nền tảng và vận hành tối thiểu

### 3.1. Mục tiêu đầu ra

- Quản lý được vòng đời tài khoản, quyền truy cập, hồ sơ tối thiểu và yêu cầu dữ liệu.
- Quản trị được học liệu mẫu theo phiên bản, quyền và quy trình duyệt.
- Có quyền quản trị tối thiểu, nhật ký kiểm toán, hỗ trợ tài khoản, cấu hình và ứng phó sự cố.
- Có danh mục tích hợp, tài sản số, bí mật, giới hạn lưu lượng, mục tiêu mức dịch vụ và phương án suy giảm.
- Không phụ thuộc AI, giọng nói, thông báo đẩy hoặc thời gian thực để sử dụng lõi học với nội dung đã có.

### 3.2. Danh mục work package

| Work package | Module | Task nguồn được chọn | Số task | Đầu ra chính | Phụ thuộc | Ưu tiên | Trạng thái |
|---|---|---|---:|---|---|---|---|
| A-WP01 — Danh tính và an toàn tài khoản | M01 | M01-T001–M01-T043 | 43 | Vòng đời tài khoản, phiên, hồ sơ, quyền riêng tư, quyền quản trị danh tính và hỗ trợ dữ liệu | Quyết định G-D01, G-D05, G-D10, G-D11 | Cao | Đã đánh giá: 0 đạt, 24 một phần, 19 chưa có; [xem kết quả](../02-modules/01-danh-tinh-va-ho-so/DANH-GIA-HIEN-TRANG-A-WP01.md) |
| A-WP02 — Học liệu mẫu có kiểm soát | M02 | M02-T001–M02-T023; M02-T029–M02-T034 | 29 | Mục từ, tài sản, bộ từ, phiên bản, gửi duyệt, xuất bản, báo cáo và thu hồi | A-WP01; A-WP03; A-WP04 | Cao | Đã đánh giá: 0 đạt, 20 một phần, 9 chưa có; [xem kết quả](../02-modules/02-noi-dung-tu-vung/DANH-GIA-HIEN-TRANG-A-WP02.md) |
| A-WP03 — Quản trị và vận hành nền | M11 | M11-T001–M11-T022; M11-T027–M11-T040; M11-T043–M11-T048 | 42 | Quyền tối thiểu, cấu hình, quản trị nội dung, hỗ trợ, kiểm toán, sức khỏe, bảo trì và ứng phó sự cố | A-WP01; đầu ra quản trị của A-WP02; A-WP04 | Cao | Đã đánh giá: 0 đạt, 28 một phần, 13 chưa có, 1 không còn phù hợp; [xem kết quả](../02-modules/11-quan-tri-va-van-hanh/DANH-GIA-HIEN-TRANG-A-WP03.md) |
| A-WP04 — Tích hợp và tài sản số nền | M12 | M12-T001–M12-T010; M12-T021–M12-T025; M12-T031–M12-T038; M12-T040–M12-T047 | 31 | Danh mục tích hợp, danh tính ngoài, kho tài sản, trạng thái dùng chung, giới hạn, bí mật, dữ liệu ngoài, bản quyền, mức dịch vụ và chi phí | A-WP01; A-WP02; A-WP03 | Cao | Đã đánh giá: 0 đạt, 19 một phần, 12 chưa có; [xem kết quả](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/DANH-GIA-HIEN-TRANG-A-WP04.md) |

### 3.3. Thứ tự thực hiện

```text
A-WP01 ranh giới danh tính ───────┐
                                  ├──> A-WP02 học liệu mẫu ──> Cổng A
A-WP04 danh mục tích hợp ─────────┤
                                  │
A-WP03 quyền và kiểm toán ─────────┘
```

Các work package chạy song song theo lát cắt. Từ điển, quyền sở hữu dữ liệu, danh mục tích hợp và ma trận quyền phải hoàn thành trước; vòng đời nghiệp vụ, suy giảm và kiểm toán hoàn thành tiếp theo; nghiệm thu xuyên luồng là bước cuối.

### 3.4. Cổng hoàn thành Giai đoạn A

| Cổng | Bằng chứng bắt buộc | Kết quả yêu cầu |
|---|---|---|
| A-G01 — Tài khoản an toàn | Ma trận trạng thái/quyền, kịch bản đăng ký–đăng nhập–khôi phục–khóa–xóa, bằng chứng thu hồi phiên | Không có đường truy cập bỏ qua trạng thái hoặc quyền |
| A-G02 — Quản trị có kiểm soát | Ma trận quyền tối thiểu, bằng chứng xác minh lại, nhật ký trước/sau và kịch bản từ chối | Thao tác nhạy cảm không thể thực hiện thiếu quyền hoặc thiếu nhật ký |
| A-G03 — Học liệu mẫu | Checklist chất lượng, phiên bản, quyền hiển thị, báo cáo và thu hồi | Có thể quản lý một tập học liệu mẫu mà không cần AI |
| A-G04 — Tích hợp suy giảm an toàn | Danh mục năng lực, chủ sở hữu, dữ liệu tối thiểu, hạn mức, trạng thái lỗi và phương án suy giảm | Lỗi dịch vụ ngoài không bị diễn giải thành kết quả nghiệp vụ hợp lệ |
| A-G05 — Bí mật và dữ liệu ngoài | Kiểm kê bí mật, bằng chứng thu hồi/thay thế bí mật nghi lộ, bản đồ dữ liệu rời hệ thống | Không còn bí mật không chủ sở hữu hoặc dữ liệu gửi ra ngoài không mục đích |
| A-G06 — Sẵn sàng ứng phó | Bốn mức sự cố, chủ xử lý, mục tiêu phản hồi/khôi phục, hướng dẫn và kết quả diễn tập | Sự cố trọng yếu có đường phát hiện, khống chế, khôi phục và đối soát |

## 4. Giai đoạn B — Phát hành ban đầu

### 4.1. Mục tiêu đầu ra

- Người dùng chọn được bộ từ thủ công đã duyệt, bắt đầu, tiếp tục và hoàn thành phiên học.
- Kết quả học cập nhật lịch ôn và tiến độ theo người dùng–nghĩa, chống ghi nhận lặp.
- Hệ thống có sổ tài sản, một số vật phẩm, cấp thưởng và thu hồi/điều chỉnh có truy vết.
- Có nhiệm vụ ngày, tiến độ nhiệm vụ và nhận thưởng đúng một lần.
- Có hộp thư, nhắc từ đến hạn, lựa chọn nhận tin và giờ yên lặng.
- Có chỉ số, đối soát và cảnh báo tối thiểu cho các luồng phát hành.

### 4.2. Danh mục work package

| Work package | Module | Task nguồn được chọn | Số task | Đầu ra chính | Phụ thuộc | Ưu tiên | Trạng thái |
|---|---|---|---:|---|---|---|---|
| B-WP01 — Hoàn thiện học liệu thủ công | M02 | M02-T035–M02-T044 | 10 | Khám phá, thư viện và liên kết cấu hình thưởng cho bộ từ đã duyệt | A-WP02; B-WP04 | Cao | Chờ Cổng A |
| B-WP02 — Phiên học và kiểm tra lõi | M03 | M03-T001–M03-T008; M03-T010–M03-T030; M03-T032–M03-T045 | 43 | Tạo/tiếp tục/chốt phiên, câu hỏi lõi, gợi ý, kết quả từ và sự kiện hoàn thành | B-WP01; B-WP03 | Cao | Chờ Cổng A |
| B-WP03 — Ôn tập và tiến độ | M04 | M04-T001–M04-T027; M04-T031–M04-T044 | 41 | Hồ sơ nhớ, chấm chất lượng, lịch ôn, hàng đợi, lịch sử, tiến độ và tín hiệu nhắc ôn | B-WP02; A-WP01 | Cao | Chờ Cổng A |
| B-WP04 — Tài sản và thưởng tối thiểu | M06 | M06-T001–M06-T008; M06-T011–M06-T017; M06-T034–M06-T042 | 24 | Bỏ AP, danh mục giá trị/vật phẩm, sổ biến động, cấp thưởng, sử dụng vật phẩm, điều chỉnh và đối soát | A-WP01; A-WP03; sự kiện B-WP02/B-WP05 | Cao | Chờ Cổng A |
| B-WP05 — Nhiệm vụ ngày | M07 | M07-T001–M07-T026; M07-T030–M07-T034; M07-T038; M07-T040–M07-T044 | 37 | Định nghĩa/phân bổ nhiệm vụ, đếm tiến độ, chu kỳ ngày, nhận thưởng và danh sách hôm nay | B-WP02; B-WP03; B-WP04 | Cao | Chờ B-WP02–B-WP04 |
| B-WP06 — Hộp thư và nhắc học | M10 | M10-T001–M10-T028; M10-T030–M10-T034; M10-T044; M10-T046 | 35 | Lựa chọn nhận tin, mẫu, hộp thư, chọn kênh, giới hạn tần suất, giờ yên lặng, nhắc ôn/nhiệm vụ và lưu giữ | A-WP01; B-WP03; B-WP05; B-WP08 | Cao | Chờ tín hiệu nguồn ổn định |
| B-WP07 — Quan sát và đối soát phát hành | M11 | M11-T023–M11-T026; M11-T041–M11-T042 | 6 | Độ mới chỉ số, bảng điều hành, xuất dữ liệu có kiểm soát, điều chỉnh và chạy bù | A-WP03; B-WP02–B-WP06 | Cao | Chờ các luồng nguồn |
| B-WP08 — Chuyển phát và suy giảm kênh | M12 | M12-T026–M12-T030; M12-T039 | 6 | Thư điện tử, đẩy, trạng thái chuyển phát, thử lại, chống lặp và kiểm thử suy giảm | A-WP04; B-WP06 | Cao | Chờ A-WP04 |

### 4.3. Thứ tự thực hiện

```text
B-WP01 học liệu ──> B-WP02 phiên học ──> B-WP03 ôn tập
                           │                    │
                           └──────┬─────────────┘
                                  ↓
B-WP04 tài sản/thưởng ──> B-WP05 nhiệm vụ ──> B-WP06 nhắc học
          │                                      │
          └────────> B-WP07 đối soát <───────────┤
B-WP08 chuyển phát ──────────────────────────────┘
```

### 4.4. Cổng hoàn thành Giai đoạn B

| Cổng | Bằng chứng bắt buộc | Kết quả yêu cầu |
|---|---|---|
| B-G01 — Hành trình học hoàn chỉnh | Kịch bản chọn bộ → học → trả lời → hoàn thành → sinh lịch ôn → ôn lại | Không mất trạng thái, không ghi kết quả lặp, học liệu trong phiên không đổi ngầm |
| B-G02 — Giá trị học thuật | Bộ ví dụ chấm chuẩn, quy tắc lịch ôn, chỉ số hoàn thành và duy trì | Chỉ số tương tác không thay thế tiêu chí ghi nhớ |
| B-G03 — Tài sản nhất quán | Sổ biến động, đối soát, kịch bản cấp lặp/thu hồi/chạy bù và kế hoạch loại AP | Mọi thay đổi tài sản giải thích được và không tạo số dư ngoài sổ |
| B-G04 — Nhiệm vụ không cấp thưởng lặp | Kịch bản sự kiện trùng/sai thứ tự/đến muộn, ảnh chụp gói thưởng và đối soát | Một điều kiện hoàn thành chỉ tạo một hiệu ứng thưởng hợp lệ |
| B-G05 — Thông báo phù hợp | Ma trận đồng ý theo thị trường/kênh/tuổi, giờ yên lặng, giới hạn tần suất, chống gửi lặp | Không gửi trái lựa chọn hiện tại hoặc ngoài ngoại lệ được duyệt |
| B-G06 — Vận hành phát hành | Dashboard lõi, cảnh báo, vụ việc, bảo trì, khôi phục và đối soát | Có thể phát hiện và xử lý lỗi trước khi sai lệch lan rộng |

## 5. Ngoại lệ cắt lát theo giai đoạn

Một số task nguồn bao phủ cả năng lực A–B và năng lực bị hoãn. Khi đưa vào công cụ quản lý công việc phải tạo lát cắt A–B có tiêu chí riêng, không được đánh dấu task nguồn hoàn thành toàn bộ nếu nhánh hoãn chưa được thực hiện.

| Task nguồn | Phạm vi được phép trong A–B | Nhánh tiếp tục hoãn |
|---|---|---|
| M06-T011 | Sự kiện thưởng từ phiên học và nhiệm vụ | Nguồn phát âm, PvP hoặc năng lực chưa phát hành |
| M07-T030 | Điều kiện nhận thưởng của nhiệm vụ ngày | Điều kiện thưởng thành tựu dài hạn |
| M10-T044 | Quyền vận hành hộp thư, mẫu và nhắc học | Quyền vận hành chiến dịch diện rộng |
| M12-T042 | Dữ liệu rời hệ thống cho danh tính, tài sản và kênh A–B; AI/giọng nói chỉ ghi chính sách “chưa hoạt động” | Luồng dữ liệu AI và giọng nói thực tế |
| M12-T044 | Tài sản ngoài sử dụng trong học liệu A–B, kèm cảnh báo rủi ro khi thiếu nguồn/quyền | Nguồn ảnh tự động hoặc danh mục tài sản nâng cao |
| M12-T047 | Kiểm chứng các nhà cung cấp đang hoạt động trong A–B | Kiểm chứng AI, giọng nói và thời gian thực chưa phát hành |

## 6. Điều kiện chặn và quyết định phát hành

Quyết định nghiệp vụ đã đầy đủ nhưng các bằng chứng sau chưa có trong tài liệu. Đây là cổng phát hành, không phải câu hỏi quyết định mới.

| Mã cổng | Điều kiện | Giai đoạn ảnh hưởng | Chủ trì đề xuất | Trạng thái hiện tại | Cách đóng cổng |
|---|---|---|---|---|---|
| REL-01 | Rà soát pháp lý cho sản phẩm mọi độ tuổi nhưng không xác minh/liên kết người giám hộ và vẫn cho truy cập đầy đủ khi thiếu đồng ý | A và B | Sản phẩm, pháp lý, M01, M11 | Chưa có bằng chứng; chặn phát hành | Văn bản chấp nhận hoặc yêu cầu điều chỉnh theo từng thị trường/nhóm tuổi |
| REL-02 | Chứng minh quyền tối thiểu, xác minh lại và nhật ký bất biến đủ bù cho việc không có duyệt hai người/quyền khẩn cấp | A | M11, an toàn hệ thống | Chưa nghiệm thu; chặn vận hành quản trị | Bộ kiểm thử quyền, bằng chứng từ chối, nhật ký và diễn tập khôi phục |
| REL-03 | Kiểm kê, thu hồi/thay thế bí mật nghi lộ và xác nhận mọi tích hợp có chủ, hạn mức, suy giảm | A | M12, M11 | Chưa có bằng chứng; chặn tích hợp liên quan | Sổ bí mật/tích hợp được duyệt và biên bản xoay vòng/kiểm thử suy giảm |
| REL-04 | Chấp nhận rủi ro bản quyền vì nguồn và giấy phép tài sản không bắt buộc | A và B | Sản phẩm, pháp lý, M02, M12 | Chưa có bằng chứng; chặn tài sản thiếu quyền rõ | Văn bản chấp nhận rủi ro, đầu mối xử lý khiếu nại và quy trình gỡ nhanh |
| REL-05 | Loại bỏ AP không chuyển đổi/bồi hoàn nhưng vẫn giữ lịch sử | B | Sản phẩm, M06, M11 | Chưa có kế hoạch dữ liệu/truyền thông; chặn chuyển đổi | Kiểm kê dữ liệu AP, kế hoạch ngừng dùng, đối soát, thông báo và phương án quay lui |
| REL-06 | Mặc định nhận thông báo phải được rà soát theo thị trường, kênh và nhóm tuổi | B | Sản phẩm, pháp lý, M01, M10 | Chưa có ma trận thị trường; chặn kênh tùy chọn | Ma trận đồng ý được duyệt; nơi pháp luật yêu cầu phải ghi đè mặc định sản phẩm |
| REL-07 | Tài khoản bị xóa qua hỗ trợ và xuất dữ liệu tự phục vụ phải có thời hạn, bằng chứng chủ thể và đối soát liên module | A và B | M01, M11 | Chưa nghiệm thu | Kịch bản xuyên module, thời hạn xử lý, bằng chứng xóa/ẩn danh và kết quả xuất |

### 6.1. Các cổng không chặn A–B nếu năng lực được giữ tắt

- Không gửi âm thanh người chưa thành niên ra ngoài; phát âm sâu thuộc Giai đoạn C.
- Không bật tạo/xuất bản học liệu AI; năng lực này thuộc Giai đoạn C.
- Không bật thú cưng tiến hóa, hiệu ứng học tập/chiến đấu hoặc cơ chế chuyển thú cưng trùng.
- Không bật nhóm, bảng xếp hạng, PvP, thời gian thực hoặc chiến dịch truyền thông diện rộng.

## 7. Quy tắc đưa vào công cụ quản lý công việc

| Trường | Giá trị sử dụng |
|---|---|
| Mã công việc | Giữ nguyên Task ID trong backlog module |
| Giai đoạn | `A` hoặc `B` theo work package trong tài liệu này |
| Nhóm sở hữu | M01–M12 |
| Work package | A-WP01–A-WP04 hoặc B-WP01–B-WP08 |
| Tên, mô tả, đầu vào/đầu ra, ưu tiên, độ phức tạp | Sao chép nguyên từ `TASK-BACKLOG.md` của module |
| Phụ thuộc | Giữ phụ thuộc nguồn; bổ sung work package/cổng phát hành liên quan |
| Trạng thái ban đầu | Chờ đánh giá hiện trạng |
| Điều kiện hoàn thành | Giữ Definition of Done nguồn và bổ sung bằng chứng cổng giai đoạn |
| Nhãn rủi ro | Gắn mã REL-01–REL-07 nếu task tạo bằng chứng đóng cổng tương ứng |

Không gán ngày hoàn thành hoặc số chu kỳ khi chưa có quy mô đội ngũ, năng lực thực tế và kết quả đánh giá mã nguồn. Không đánh dấu task “không cần làm” chỉ vì hệ thống hiện tại đã có màn hình hoặc dữ liệu tương tự; phải đối chiếu Definition of Done trước.

## 8. Trình tự làm việc tiếp theo

1. A-WP01–A-WP04 đã được đánh giá theo từng Task ID; baseline Giai đoạn A đã hoàn tất.
2. Ma trận sai lệch, hồ sơ REL và thứ tự triển khai đã được hợp nhất tại [Tổng hợp sai lệch và thứ tự triển khai Giai đoạn A](TONG-HOP-SAI-LECH-VA-THU-TU-TRIEN-KHAI-GIAI-DOAN-A.md).
3. Chỉ sau khi Cổng A đạt, đánh giá hiện trạng và lập thứ tự chi tiết cho các work package Giai đoạn B.
4. Hoàn thiện REL-05–REL-06 trước khi thử chuyển đổi AP hoặc gửi thông báo tùy chọn.
5. Nhập task đã đánh giá vào công cụ quản lý công việc; giữ nguyên mã, phụ thuộc, Definition of Done và liên kết tài liệu nguồn.
6. Toàn bộ gói task Lát 0–5, [Bảng import tổng 167 công việc](BANG-IMPORT-TONG-GIAI-DOAN-A.md), [Bộ mẫu bằng chứng Cổng A](../05-bang-chung/cong-a/README.md) và [Kiểm tra bao phủ 145 task/checklist quyết định](KIEM-TRA-BAO-PHU-VA-QUYET-DINH-CONG-A.md) đã sẵn sàng.
7. Sử dụng [Kế hoạch khởi động Lát 0](../04-thuc-thi/lat-0/KE-HOACH-KHOI-DONG.md) để gán người thực tế, mở hồ sơ bằng chứng và kích hoạt bảy task không có phụ thuộc trước khi mở năm task còn lại theo thứ tự đã chốt.
