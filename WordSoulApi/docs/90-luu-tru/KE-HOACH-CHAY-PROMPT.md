# Kế hoạch chạy prompt phân tích chuyên sâu theo module

> Trạng thái: đã hoàn thành và được lưu để truy vết phương pháp. Mười hai prompt đầu vào theo module đã được hợp nhất thành [mẫu prompt dùng lại](./PROMPT-PHAN-TICH-MODULE-MAU.md); đầu ra chính thức nằm trong thư mục từng module.

## 1. Mục tiêu kế hoạch

Kế hoạch này hướng dẫn chạy lần lượt 12 prompt phân tích module để tạo ra:

- Phân tích nghiệp vụ chi tiết và có thể truy vết về mục tiêu module.
- Danh sách chức năng con, luồng chính, trường hợp biên và Definition of Done.
- Backlog task có thứ tự, ưu tiên, độ phức tạp và phụ thuộc rõ ràng.
- Danh sách quyết định còn mở trước khi bắt đầu nâng cấp hệ thống.
- Backlog tổng hợp không trùng lặp giữa các module.

Kế hoạch chỉ điều phối hoạt động phân tích và lập task. Việc triển khai task không thuộc phạm vi của tài liệu này.

## 2. Nguyên tắc thực hiện

1. Chạy một prompt chính cho một module tại một thời điểm để dễ kiểm soát ngữ cảnh và chất lượng.
2. Không coi giả định do AI tạo ra là yêu cầu đã được xác nhận.
3. Không chuyển sang module phụ thuộc nếu đầu ra nền tảng còn thiếu định nghĩa quan trọng.
4. Mỗi task phải thuộc đúng một module chính; module khác chỉ được ghi là bên phụ thuộc hoặc bị ảnh hưởng.
5. Task giao thoa phải được phát hiện và hợp nhất trong bước rà soát chéo, không sao chép thành nhiều nguồn sự thật.
6. Mọi kết quả vẫn là tài liệu nghiệp vụ; không sinh mã, tên thành phần lập trình hoặc thiết kế triển khai chi tiết.

## 3. Bộ đầu vào cho mỗi lần chạy

Trước khi chạy prompt của một module, cung cấp cho AI:

- [Mẫu prompt phân tích module](./PROMPT-PHAN-TICH-MODULE-MAU.md), được điền bằng mô tả module hiện hành.
- File `README.md` của module.
- Phần quan hệ liên quan trong tài liệu tổng quan.
- Các quyết định đã được xác nhận từ những module chạy trước.
- Các đầu ra hoặc Task ID của module phụ thuộc trực tiếp.
- Những giới hạn sản phẩm mới được người yêu cầu bổ sung, nếu có.

Nếu thiếu thông tin bắt buộc, AI vẫn phải hoàn thành phần có thể phân tích và đưa phần còn thiếu vào nhóm task chờ quyết định, thay vì tự xác nhận giả định.

## 4. Chuẩn đầu ra cho mỗi module

Sau mỗi lần chạy, lưu kết quả trong chính thư mục module:

| File đầu ra | Nội dung |
|---|---|
| `PHAN-TICH-CHUYEN-SAU.md` | Mục tiêu, phạm vi, câu hỏi mở, giả định, chức năng con, business flow, edge case, quy tắc và Definition of Done |
| `TASK-BACKLOG.md` | Bảng task sạch, một task mỗi hàng, phù hợp chuyển sang công cụ quản lý công việc |
| `QUYET-DINH-MO.md` | Những quyết định chưa được xác nhận, phương án lựa chọn, ảnh hưởng và module liên quan |

### 4.1. Quy ước mã task

| Module | Tiền tố task | Ví dụ hình thức |
|---|---|---|
| M01 | `M01-T` | `M01-T001` |
| M02 | `M02-T` | `M02-T001` |
| M03 | `M03-T` | `M03-T001` |
| M04 | `M04-T` | `M04-T001` |
| M05 | `M05-T` | `M05-T001` |
| M06 | `M06-T` | `M06-T001` |
| M07 | `M07-T` | `M07-T001` |
| M08 | `M08-T` | `M08-T001` |
| M09 | `M09-T` | `M09-T001` |
| M10 | `M10-T` | `M10-T001` |
| M11 | `M11-T` | `M11-T001` |
| M12 | `M12-T` | `M12-T001` |

Mã không được tái sử dụng sau khi một task đã bị loại bỏ. Task bị loại bỏ cần giữ trạng thái và lý do trong lịch sử backlog.

## 5. Thứ tự chạy đề xuất

### Giai đoạn 0 — Chuẩn bị ngữ cảnh chung

| Thứ tự | Công việc | Đầu ra cần có | Điều kiện hoàn thành |
|---|---|---|---|
| 0.1 | Rà soát thuật ngữ chung | Danh sách định nghĩa sơ bộ về người dùng, từ, bộ từ, phiên học, tiến độ, phần thưởng, nhiệm vụ và trận đấu | Không còn một thuật ngữ cốt lõi có hai nghĩa mâu thuẫn giữa các module |
| 0.2 | Lập sổ quyết định chung | Danh sách câu hỏi xuyên module và người có trách nhiệm xác nhận | Mỗi câu hỏi có mã, mức ảnh hưởng và module liên quan |
| 0.3 | Chốt chuẩn task | Quy ước mã, cột dữ liệu, mức ưu tiên và S/M/L | Tất cả module dùng cùng một cấu trúc backlog |

### Giai đoạn 1 — Nền tảng người dùng và học liệu

| Thứ tự | Prompt cần chạy | Lý do | Đầu vào từ trước | Cổng hoàn thành |
|---|---|---|---|---|
| 1 | M01 — Danh tính và hồ sơ | Xác định người dùng, vai trò, quyền và vòng đời tài khoản cho toàn hệ thống | Ngữ cảnh chung | Vai trò, trạng thái tài khoản, quyền riêng tư và danh tính dùng chung đã có Definition of Done hoặc được đánh dấu chờ quyết định |
| 2 | M02 — Nội dung từ vựng và bộ từ | Xác định nguồn học liệu chuẩn cho học, ôn, phát âm và thi đấu | Quyết định hồ sơ/trình độ từ M01 | Vòng đời nội dung, chuẩn chất lượng và quy tắc thay đổi học liệu đã rõ |

### Giai đoạn 2 — Lõi học thuật

| Thứ tự | Prompt cần chạy | Lý do | Đầu vào từ trước | Cổng hoàn thành |
|---|---|---|---|---|
| 3 | M03 — Phiên học và kiểm tra | Tạo nguồn kết quả học chính thức | M01, M02 | Phân biệt phiên học/ôn, trạng thái phiên, quy tắc câu trả lời và hoàn thành đúng một lần đã rõ |
| 4 | M05 — Luyện phát âm | Làm rõ một nguồn đánh giá kỹ năng có thể tác động tiến độ | M01, M02 và khái niệm kết quả học từ M03 | Ngưỡng đạt, quyền riêng tư âm thanh và phạm vi tác động sang tiến độ đã rõ hoặc được treo quyết định |
| 5 | M04 — Ôn tập ngắt quãng và tiến độ | Tổng hợp kết quả học và phát âm thành lịch ôn, mức ghi nhớ | M01, M02, M03, M05 | Trạng thái người dùng–từ, lịch ôn, múi giờ, lịch sử và chỉ số thành thạo đã có quy tắc nhất quán |

M04 chỉ sử dụng tác động phát âm đã được xác nhận. Nếu M05 chưa chốt được tác động, M04 phải thiết kế tiến độ phát âm như một nhánh tùy chọn, không mặc định trộn điểm.

### Giai đoạn 3 — Động lực và kinh tế gamification

| Thứ tự | Prompt cần chạy | Lý do | Đầu vào từ trước | Cổng hoàn thành |
|---|---|---|---|---|
| 6 | M06 — Thú cưng, vật phẩm và kinh tế phần thưởng | Thiết lập nguồn sự thật cho tài sản và phần thưởng trước khi nhiều module phát thưởng | M01, M02, kết quả hoàn thành từ M03/M04 | Đơn vị giá trị, nguồn vào/ra, chống cấp lặp, lịch sử biến động và hiệu ứng đã rõ |
| 7 | M07 — Nhiệm vụ và thành tựu | Chuyển sự kiện học ổn định thành mục tiêu và yêu cầu nhận thưởng | M03, M04, M05, M06 | Nguồn sự kiện, cách đếm, chu kỳ, múi giờ và nhận thưởng đúng một lần đã rõ |

### Giai đoạn 4 — Xã hội, thi đấu và duy trì tương tác

| Thứ tự | Prompt cần chạy | Lý do | Đầu vào từ trước | Cổng hoàn thành |
|---|---|---|---|---|
| 8 | M09 — Nhóm cộng đồng và xếp hạng | Xác định quyền nhóm và nguồn điểm trước khi dùng cho ghép trận/hiển thị thành tích | M01, M04 và khái niệm điểm thi đấu dự kiến | Vai trò nhóm, quyền riêng tư, công thức/phạm vi/chu kỳ xếp hạng đã rõ |
| 9 | M08 — Đấu trường và phòng thử thách | Phụ thuộc học liệu, tài sản và dữ liệu xếp hạng | M01, M02, M06, M09 | Luật trận, công bằng, trạng thái, mất kết nối, kết thúc, điểm và thưởng đã có Definition of Done |
| 10 | M10 — Thông báo và duy trì tương tác | Tổng hợp tín hiệu từ tiến độ, nhiệm vụ, nhóm và thi đấu sau khi nguồn sự kiện ổn định | M01, M04, M07, M08, M09 | Phân loại, kênh, lựa chọn nhận tin, chống lặp, múi giờ và giờ yên lặng đã rõ |

### Giai đoạn 5 — Điều hành và tích hợp dùng chung

| Thứ tự | Prompt cần chạy | Lý do | Đầu vào từ trước | Cổng hoàn thành |
|---|---|---|---|---|
| 11 | M11 — Quản trị, cấu hình và quan sát | Tổng hợp đầy đủ nhu cầu quản trị, cấu hình và đo lường từ các module nghiệp vụ | M01–M10 | Vai trò quản trị, thao tác nhạy cảm, cấu hình, chỉ số, kiểm toán và vận hành đã được bao phủ |
| 12 | M12 — Tích hợp nền tảng và tài sản số | Chốt danh mục năng lực ngoài sau khi biết rõ module nào tiêu thụ và mức quan trọng | M01, M02, M05, M08, M10, M11 | Mỗi tích hợp có chủ sở hữu nghiệp vụ, dữ liệu tối thiểu, giới hạn chi phí, xử lý lỗi và phương án suy giảm |

M11 và M12 được phân tích ở cuối để có đủ yêu cầu từ module tiêu thụ. Các yêu cầu quản trị hoặc tích hợp cấp thiết phát hiện ở giai đoạn trước vẫn phải được ghi nhận ngay dưới dạng phụ thuộc, không chờ đến lúc chạy hai prompt này.

## 6. Quy trình cho một lần chạy prompt

### Bước A — Chuẩn bị

1. Đọc prompt và mô tả module.
2. Thu thập quyết định từ các module chạy trước.
3. Liệt kê phụ thuộc đã có Task ID và phụ thuộc chưa được phân tích.
4. Đánh dấu thông tin nào là xác nhận, giả định hoặc câu hỏi mở.

### Bước B — Chạy prompt

1. Gửi nguyên nội dung prompt cho AI.
2. Bổ sung các quyết định đã xác nhận ngay sau phần thông tin module.
3. Yêu cầu AI không dừng ở câu hỏi làm rõ; phải tiếp tục phần phân tích có thể làm với giả định được gắn nhãn.
4. Yêu cầu một task trên một hàng và không xuống dòng trong ô bảng task.

### Bước C — Kiểm duyệt đầu ra

1. Đối chiếu lại phạm vi module.
2. Kiểm tra mọi chức năng con đều có business flow, edge case và Definition of Done.
3. Kiểm tra mọi chức năng con có ít nhất một task.
4. Loại task trùng hoặc chia task có nhiều kết quả lớn.
5. Chuẩn hóa Task ID, ưu tiên, S/M/L và phụ thuộc.
6. Chuyển giả định quan trọng vào file quyết định mở.

### Bước D — Đóng lần chạy

1. Lưu ba file đầu ra theo chuẩn.
2. Cập nhật phụ thuộc cho các module chưa chạy.
3. Đánh dấu trạng thái module là `Đã phân tích`, `Chờ quyết định` hoặc `Cần chạy lại`.
4. Chỉ mở giai đoạn tiếp theo khi cổng hoàn thành đã đạt hoặc có quyết định chấp nhận rủi ro.

## 7. Cổng kiểm tra chất lượng

| Cổng | Nội dung kiểm tra | Điều kiện đạt |
|---|---|---|
| Q1 — Phạm vi | Task có bám mục tiêu và ranh giới module không? | Không có task ngoài phạm vi trong backlog chính |
| Q2 — Độ phủ | Chức năng, luồng, ngoại lệ và vai trò có đầy đủ không? | Mỗi mục tiêu có chức năng và mỗi chức năng có task |
| Q3 — Chất lượng task | Task có một kết quả, input/output và DoD kiểm chứng được không? | Không còn task mơ hồ hoặc quá lớn chưa được tách |
| Q4 — Phụ thuộc | Phụ thuộc có Task ID/module và có vòng lặp không? | Không có phụ thuộc vòng; điểm chặn được đánh dấu |
| Q5 — Rủi ro | Bảo mật, riêng tư, dữ liệu, công bằng và vận hành đã được xét chưa? | Rủi ro liên quan có task xử lý hoặc quyết định chấp nhận |
| Q6 — Khả năng nhập | Bảng có một task mỗi hàng và giá trị chuẩn không? | Có thể sao chép sang công cụ quản lý mà không phải tách lại nội dung |

## 8. Rà soát chéo sau mỗi giai đoạn

| Sau giai đoạn | Nội dung rà soát |
|---|---|
| Giai đoạn 1 | Thống nhất người dùng, vai trò, trình độ và quyền xem/chỉnh sửa học liệu |
| Giai đoạn 2 | Thống nhất định nghĩa câu trả lời, hoàn thành, đã học, đến hạn và thành thạo |
| Giai đoạn 3 | Thống nhất sự kiện tạo thưởng, nguồn sự thật số dư và chống nhận lặp |
| Giai đoạn 4 | Thống nhất điểm, xếp hạng, sự kiện xã hội và loại thông báo |
| Giai đoạn 5 | Hợp nhất cấu hình, nhật ký, chỉ số, dữ liệu gửi ra ngoài và phương án suy giảm |

Khi phát hiện mâu thuẫn, không tự sửa âm thầm trong một module. Tạo một quyết định mở, liệt kê các module bị ảnh hưởng và chỉ cập nhật backlog sau khi chọn phương án.

## 9. Hợp nhất backlog toàn hệ thống

**Trạng thái:** Đã hoàn thành. Xem [Tổng hợp backlog toàn hệ thống](../01-tong-quan/TONG-HOP-BACKLOG-TOAN-HE-THONG.md).

Sau khi hoàn thành M12:

1. Gộp task từ 12 module theo Task ID.
2. Phát hiện task trùng mục tiêu hoặc cùng tạo một đầu ra.
3. Chọn một module sở hữu; module còn lại ghi quan hệ phụ thuộc.
4. Kiểm tra phụ thuộc vòng ở cấp toàn hệ thống.
5. Nhóm task theo các mốc: nền tảng, lõi học thuật, gamification, xã hội/thi đấu, vận hành/tích hợp.
6. Xác định đường công việc dài nhất và các task có thể làm song song.
7. Tạo backlog phát hành ban đầu và backlog nâng cấp sau, nhưng không tự gán thời hạn nếu chưa có năng lực đội ngũ.

## 10. Bảng theo dõi trạng thái chạy prompt

| Thứ tự | Module | Prompt | Trạng thái | Phụ thuộc đầu vào | Ghi chú |
|---|---|---|---|---|---|
| 1 | M01 Danh tính và hồ sơ | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | Ngữ cảnh chung | 10 chức năng con, 43 task, 25 quyết định mở; [xem kết quả](../02-modules/01-danh-tinh-va-ho-so/PHAN-TICH-CHUYEN-SAU.md) |
| 2 | M02 Nội dung từ vựng | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01 | 10 chức năng con, 46 task, 25 quyết định mở; [xem kết quả](../02-modules/02-noi-dung-tu-vung/PHAN-TICH-CHUYEN-SAU.md) |
| 3 | M03 Phiên học và kiểm tra | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M02 | 10 chức năng con, 47 task, 24 quyết định mở; [xem kết quả](../02-modules/03-phien-hoc-va-kiem-tra/PHAN-TICH-CHUYEN-SAU.md) |
| 4 | M05 Luyện phát âm | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M02, M03 | 10 chức năng con, 43 task, 24 quyết định mở; [xem kết quả](../02-modules/05-luyen-phat-am/PHAN-TICH-CHUYEN-SAU.md) |
| 5 | M04 Ôn tập và tiến độ | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M02, M03, M05 | 10 chức năng con, 46 task, 25 quyết định mở; tác động phát âm được tách thành nhánh tùy chọn; [xem kết quả](../02-modules/04-on-tap-va-tien-do/PHAN-TICH-CHUYEN-SAU.md) |
| 6 | M06 Thú cưng, vật phẩm và kinh tế | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M02, M03, M04 | 10 chức năng con, 47 task, 25 quyết định mở; [xem kết quả](../02-modules/06-thu-cung-vat-pham-kinh-te/PHAN-TICH-CHUYEN-SAU.md) |
| 7 | M07 Nhiệm vụ và thành tựu | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M03, M04, M05, M06 | 10 chức năng con, 46 task, 25 quyết định mở; [xem kết quả](../02-modules/07-nhiem-vu-va-thanh-tuu/PHAN-TICH-CHUYEN-SAU.md) |
| 8 | M09 Nhóm và xếp hạng | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M04 | 10 chức năng con, 47 task, 25 quyết định mở; [xem kết quả](../02-modules/09-nhom-va-xep-hang/PHAN-TICH-CHUYEN-SAU.md) |
| 9 | M08 Đấu trường và phòng thử thách | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M02, M06, M09 | 10 chức năng con, 48 task, 26 quyết định mở; [xem kết quả](../02-modules/08-dau-truong-va-phong-thu-thach/PHAN-TICH-CHUYEN-SAU.md) |
| 10 | M10 Thông báo và duy trì tương tác | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích | M01, M04, M07, M08, M09 | 10 chức năng con, 48 task, 26 quyết định mở; [xem kết quả](../02-modules/10-thong-bao-va-duy-tri-tuong-tac/PHAN-TICH-CHUYEN-SAU.md) |
| 11 | M11 Quản trị và vận hành | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích, quyết định đầy đủ | M01–M10 | 10 chức năng con, 50 task, 27/27 quyết định đã chốt; [xem kết quả](../02-modules/11-quan-tri-va-van-hanh/PHAN-TICH-CHUYEN-SAU.md) |
| 12 | M12 Tích hợp nền tảng và tài sản số | [Mở prompt](PROMPT-PHAN-TICH-MODULE-MAU.md) | Đã phân tích, quyết định đầy đủ | M01, M02, M05, M08, M10, M11 | 10 chức năng con, 50 task, 28/28 quyết định đã chốt; [xem kết quả](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/PHAN-TICH-CHUYEN-SAU.md) |

## 11. Hành động tiếp theo

Đã hoàn thành prompt và cổng kiểm tra cấu trúc cho cả 12 module. Bộ tài liệu hiện có 120 chức năng con, 561 task và 305 quyết định; toàn bộ 305 quyết định đã chốt, không có mục mở hoặc chốt một phần và mã task/quyết định không trùng giữa các module.

Bước tiếp theo là xử lý các điều kiện chặn phát hành, xác nhận phạm vi Giai đoạn A–B và chuyển các nhóm task đã đủ điều kiện sang công cụ quản lý công việc.
