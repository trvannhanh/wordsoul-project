# Tài liệu phân rã module hệ thống WordSoul

## 1. Thông tin tài liệu

| Trường | Nội dung |
|---|---|
| Tên hệ thống | Hỗ trợ học từ vựng tiếng Anh kết hợp gamification trong học tập |
| Mục tiêu | Tạo động lực học lâu dài, làm cho việc học thú vị nhưng vẫn bảo đảm khả năng ghi nhớ và áp dụng từ vựng |
| Đối tượng chính | Người học tiếng Anh từ cơ bản đến trung cấp |
| Ngôn ngữ tài liệu | Tiếng Việt |
| Mục đích sử dụng | Làm bản đồ ngữ cảnh hệ thống, quản lý thông tin theo module và hỗ trợ phân tích, lập task nâng cấp |
| Cơ sở phân tích | Nhu cầu đầu vào và các năng lực nghiệp vụ đang hiện diện trong hệ thống tại thời điểm lập tài liệu |

## 2. Tổng quan hệ thống

WordSoul là hệ thống hỗ trợ người dùng xây dựng vốn từ vựng tiếng Anh thông qua các bộ từ, phiên học có nhiều dạng câu hỏi, luyện phát âm và cơ chế ôn tập theo thời điểm phù hợp. Kết quả học được chuyển hóa thành tiến độ, phần thưởng, thú cưng, nhiệm vụ, thành tựu và quyền tham gia các hoạt động thi đấu.

Giá trị cốt lõi của hệ thống gồm:

- Tăng khả năng ghi nhớ dài hạn bằng việc theo dõi từng từ và nhắc ôn đúng thời điểm.
- Tạo trải nghiệm học đa dạng qua nhận biết, gợi nhớ, nghe và phát âm.
- Duy trì động lực bằng mục tiêu ngắn hạn, thành tựu dài hạn, phần thưởng và thi đấu.
- Cho phép người quản trị kiểm soát nội dung, cân bằng phần thưởng và quan sát sức khỏe vận hành.

### 2.1. Sơ đồ khối tổng thể

```text
                         +-------------------------+
                         | Quản trị và vận hành     |
                         | nội dung, cấu hình, số liệu|
                         +------------+------------+
                                      |
                                      v
+----------------+        +-----------+-----------+        +-------------------+
| Người học      |------->| Danh tính và hồ sơ    |------->| Nhóm và xếp hạng  |
+-------+--------+        +-----------+-----------+        +-------------------+
        |                             |
        v                             v
+-------+----------------+   +--------+-------------------+
| Nội dung từ vựng       |-->| Phiên học và kiểm tra     |
| từ, bộ từ, học liệu    |   | câu hỏi, câu trả lời      |
+-------+----------------+   +-----+----------------------+
        |                          |
        |                          +----------+------------------+
        |                                     |                  |
        v                                     v                  v
+-------+----------------+          +---------+---------+  +-----+--------------+
| Luyện phát âm          |--------->| Ôn tập và tiến độ |  | Nhiệm vụ, thành tựu|
+------------------------+          +---------+---------+  +-----+--------------+
                                             |                  |
                                             +---------+--------+
                                                       v
                                            +----------+-----------+
                                            | Thú cưng, vật phẩm   |
                                            | và kinh tế phần thưởng|
                                            +----------+-----------+
                                                       |
                                                       v
                                            +----------+-----------+
                                            | Đấu trường và phòng  |
                                            | thử thách             |
                                            +----------+-----------+
                                                       |
                                                       v
                                            +----------+-----------+
                                            | Thông báo và duy trì |
                                            | tương tác             |
                                            +----------------------+

Các module nghiệp vụ sử dụng chung năng lực tích hợp nền tảng và tài sản số.
```

### 2.2. Luồng giá trị chính

1. Người dùng tạo hoặc truy cập tài khoản và thiết lập hồ sơ học tập.
2. Người dùng chọn bộ từ phù hợp với trình độ hoặc mục tiêu.
3. Hệ thống tạo phiên học, ghi nhận câu trả lời và kết quả từng từ.
4. Module ôn tập cập nhật mức độ ghi nhớ và xác định thời điểm cần ôn lại.
5. Hoạt động học đóng góp vào nhiệm vụ, thành tựu và phần thưởng.
6. Phần thưởng giúp người dùng phát triển bộ sưu tập, năng lực hỗ trợ và đội hình thi đấu.
7. Thông báo, bảng xếp hạng và thử thách kéo người dùng quay lại chu kỳ học tiếp theo.

## 3. Danh sách module

Kế hoạch điều phối việc chạy từng prompt được trình bày tại [Kế hoạch chạy prompt phân tích chuyên sâu](../90-luu-tru/KE-HOACH-CHAY-PROMPT.md).

Kết quả hợp nhất 561 task, quyền sở hữu liên module, phụ thuộc và phân kỳ phát hành được trình bày tại [Tổng hợp backlog toàn hệ thống](TONG-HOP-BACKLOG-TOAN-HE-THONG.md).

| Mã | Module | Vai trò chính | Ưu tiên | Tài liệu | Prompt phân tích |
|---|---|---|---|---|---|
| M01 | Danh tính và hồ sơ người dùng | Tạo danh tính tin cậy, quyền truy cập và hồ sơ nền tảng | Cao | [Xem mô tả](../02-modules/01-danh-tinh-va-ho-so/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M02 | Nội dung từ vựng và bộ từ | Quản lý học liệu từ vựng và cấu trúc bộ từ | Cao | [Xem mô tả](../02-modules/02-noi-dung-tu-vung/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M03 | Phiên học và kiểm tra | Điều phối trải nghiệm học, câu hỏi, câu trả lời và hoàn thành phiên | Cao | [Xem mô tả](../02-modules/03-phien-hoc-va-kiem-tra/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M04 | Ôn tập ngắt quãng và tiến độ | Đánh giá mức ghi nhớ, lập lịch ôn và tổng hợp tiến bộ | Cao | [Xem mô tả](../02-modules/04-on-tap-va-tien-do/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M05 | Luyện phát âm | Cung cấp bài luyện, đánh giá và lịch sử phát âm | Trung bình | [Xem mô tả](../02-modules/05-luyen-phat-am/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M06 | Thú cưng, vật phẩm và kinh tế phần thưởng | Quản lý tài sản gamification và hiệu ứng hỗ trợ | Cao | [Xem mô tả](../02-modules/06-thu-cung-vat-pham-kinh-te/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M07 | Nhiệm vụ và thành tựu | Biến hành vi học thành mục tiêu ngắn và dài hạn | Cao | [Xem mô tả](../02-modules/07-nhiem-vu-va-thanh-tuu/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M08 | Đấu trường và phòng thử thách | Tạo hoạt động đối kháng dựa trên kiến thức và tài sản gamification | Trung bình | [Xem mô tả](../02-modules/08-dau-truong-va-phong-thu-thach/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M09 | Nhóm cộng đồng và xếp hạng | Tạo động lực xã hội, quản lý nhóm và so sánh tiến bộ | Trung bình | [Xem mô tả](../02-modules/09-nhom-va-xep-hang/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M10 | Thông báo và duy trì tương tác | Đưa thông tin đúng lúc và hỗ trợ người học quay lại | Trung bình | [Xem mô tả](../02-modules/10-thong-bao-va-duy-tri-tuong-tac/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M11 | Quản trị, cấu hình và quan sát hệ thống | Điều hành nội dung, người dùng, cân bằng và chất lượng vận hành | Cao | [Xem mô tả](../02-modules/11-quan-tri-va-van-hanh/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |
| M12 | Tích hợp nền tảng và tài sản số | Cung cấp AI, giọng nói, hình ảnh, gửi tin và năng lực hỗ trợ dùng chung | Trung bình | [Xem mô tả](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/README.md) | [Mở prompt](../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md) |

## 4. Ma trận quan hệ giữa các module

Quy ước: `A → B` nghĩa là A gửi dữ liệu hoặc kích hoạt nghiệp vụ tại B.

| Nguồn | Đích | Dữ liệu hoặc luồng nghiệp vụ chính |
|---|---|---|
| M01 Danh tính và hồ sơ | M03, M04, M06, M07, M08, M09, M10 | Danh tính người dùng, vai trò, trạng thái tài khoản và thông tin hồ sơ cần thiết |
| M02 Nội dung từ vựng | M03 | Từ, nghĩa, ví dụ, âm thanh, hình ảnh và cấu trúc bộ từ để tạo hoạt động học |
| M02 Nội dung từ vựng | M05 | Danh sách từ và dữ liệu tham chiếu phục vụ luyện phát âm |
| M02 Nội dung từ vựng | M08 | Kho câu hỏi từ vựng dùng trong các vòng thi đấu |
| M03 Phiên học | M04 | Kết quả trả lời, độ chính xác, số lần thử và trạng thái hoàn thành từng từ |
| M03 Phiên học | M07 | Sự kiện học mới, ôn tập, độ chính xác và hoàn thành phiên |
| M03 Phiên học | M06 | Yêu cầu cấp kinh nghiệm hoặc phần thưởng khi đủ điều kiện |
| M04 Ôn tập và tiến độ | M03 | Danh sách từ đến hạn ôn và mức ưu tiên của chúng |
| M04 Ôn tập và tiến độ | M10 | Thời điểm ôn sắp tới, từ quá hạn và tín hiệu nhắc học |
| M04 Ôn tập và tiến độ | M09, M11 | Chỉ số tiến bộ, mức duy trì kiến thức và dữ liệu tổng hợp |
| M05 Luyện phát âm | M04 | Kết quả phát âm có thể điều chỉnh đánh giá mức thành thạo từ |
| M05 Luyện phát âm | M07 | Sự kiện luyện tập và kết quả đạt yêu cầu |
| M06 Kinh tế phần thưởng | M03, M05, M08 | Hiệu ứng hỗ trợ đang có hiệu lực và tài sản người dùng được phép sử dụng |
| M06 Kinh tế phần thưởng | M09, M11 | Số dư, bộ sưu tập và biến động tài sản cần hiển thị hoặc kiểm soát |
| M07 Nhiệm vụ và thành tựu | M06 | Lệnh cấp thưởng sau khi người dùng đạt và nhận thưởng hợp lệ |
| M08 Đấu trường | M06 | Kết quả thi đấu và yêu cầu trao thưởng hoặc cập nhật tài sản |
| M08 Đấu trường | M09 | Điểm thi đấu và dữ liệu bảng xếp hạng |
| M08 Đấu trường | M10 | Trạng thái ghép trận, lời mời, kết quả trận và sự kiện cần báo ngay |
| M09 Nhóm và xếp hạng | M10 | Hoạt động nhóm hoặc thay đổi thứ hạng đáng chú ý |
| M11 Quản trị và vận hành | M02, M06, M07, M08 | Nội dung đã duyệt, cấu hình cân bằng, trạng thái kích hoạt và chính sách vận hành |
| M11 Quản trị và vận hành | M03, M04, M10 | Tham số điều hành phiên học, ôn tập, phần thưởng và truyền thông |
| M12 Tích hợp nền tảng | M01, M02, M05, M10 | Xác thực bên ngoài, nội dung hỗ trợ, xử lý giọng nói, hình ảnh, thư và thông báo đẩy |
| M01–M11 | M11 Quản trị và vận hành | Nhật ký hoạt động, số liệu sử dụng, lỗi nghiệp vụ và tín hiệu sức khỏe |

### 4.1. Quan hệ cần kiểm soát chặt

- M03, M04 và M02 tạo thành lõi học thuật; thay đổi ở một module có thể làm sai lịch ôn hoặc kết quả học nếu không đồng bộ định nghĩa “đã học”, “đã trả lời” và “đã thành thạo”.
- M06 là nguồn sự thật về tài sản và phần thưởng; M03, M07 và M08 chỉ phát sinh điều kiện nhận thưởng, không tự duy trì số dư riêng.
- M11 thiết lập chính sách nhưng không nên trực tiếp thay thế trách nhiệm nghiệp vụ của các module được cấu hình.
- M12 chỉ cung cấp năng lực tích hợp; quyết định nghiệp vụ cuối cùng vẫn thuộc module gọi nó.

## 5. Thứ tự phân tích và triển khai đề xuất

| Giai đoạn | Module | Lý do và điều kiện phụ thuộc |
|---|---|---|
| 1. Nền tảng người dùng | M01 Danh tính và hồ sơ | Mọi tiến độ, tài sản và hoạt động đều phải gắn với một người dùng hợp lệ |
| 2. Nền tảng học liệu | M02 Nội dung từ vựng | Cần một mô hình học liệu ổn định trước khi xây luồng học và đánh giá |
| 3. Lõi trải nghiệm học | M03 Phiên học và kiểm tra | Tạo ra tương tác học và dữ liệu kết quả ban đầu |
| 4. Lõi ghi nhớ | M04 Ôn tập ngắt quãng và tiến độ | Phụ thuộc dữ liệu từ M02 và M03; quyết định giá trị học thuật dài hạn |
| 5. Quản trị tối thiểu | M11 Quản trị, cấu hình và quan sát | Cần sớm để quản lý học liệu, chính sách và theo dõi chất lượng lõi |
| 6. Động lực nền tảng | M06 Thú cưng, vật phẩm và kinh tế phần thưởng | Cần quy tắc hoàn thành từ M03, M04 và kiểm soát từ M11 |
| 7. Mục tiêu hành vi | M07 Nhiệm vụ và thành tựu | Dựa trên sự kiện ổn định từ học tập và nguồn thưởng thống nhất từ M06 |
| 8. Mở rộng kỹ năng | M05 Luyện phát âm | Dựa trên học liệu; có thể phản hồi về tiến độ khi tiêu chí đánh giá đã rõ |
| 9. Duy trì tương tác | M10 Thông báo và duy trì tương tác | Chỉ nên tự động hóa mạnh sau khi thời điểm ôn, nhiệm vụ và trạng thái người dùng đáng tin cậy |
| 10. Động lực xã hội | M09 Nhóm cộng đồng và xếp hạng | Cần hồ sơ và chỉ số thành tích ổn định để so sánh công bằng |
| 11. Thử thách nâng cao | M08 Đấu trường và phòng thử thách | Phụ thuộc nhiều module nhất: người dùng, học liệu, thú cưng, phần thưởng, xếp hạng và thông báo |
| Xuyên suốt | M12 Tích hợp nền tảng và tài sản số | Thực hiện theo nhu cầu từng giai đoạn; phải luôn có phương án khi dịch vụ ngoài gián đoạn |

Thứ tự trên là thứ tự ưu tiên kiến trúc và giảm phụ thuộc, không bắt buộc là lịch phát hành duy nhất. M11 và M12 có thể được triển khai từng phần song song với module đang cần chúng.

## 6. Trạng thái quyết định và điểm cần quản trị

Hệ thống có 305 quyết định và toàn bộ đã được chốt; không còn mục mở hoặc chốt một phần. Các quyết định được truy vết trong các sổ `QUYET-DINH-MO.md`. Dù quyết định nghiệp vụ đã đầy đủ, những điểm sau vẫn là điều kiện phải xử lý trước phát hành:

- Rà soát pháp lý theo tuổi và thị trường vì sản phẩm phục vụ mọi độ tuổi nhưng chưa xác minh hoặc liên kết người giám hộ.
- Không gửi giọng nói người chưa thành niên ra ngoài khi chưa có bằng chứng đồng ý người giám hộ phù hợp.
- Hoàn thiện kiểm tra tự động, báo cáo, hậu kiểm và thu hồi khẩn cấp trước khi mở công khai nội dung người dùng hoặc AI tạo.
- Xác nhận chấp nhận rủi ro bản quyền khi tài sản không bắt buộc lưu nguồn, giấy phép hoặc ghi công.
- Lập kế hoạch thông báo, đối soát và lưu lịch sử trước khi loại bỏ AP không chuyển đổi hoặc bồi hoàn.
- Rà soát mặc định nhận thông báo theo từng thị trường/kênh/nhóm tuổi; nơi yêu cầu đồng ý chủ động phải ghi đè cấu hình sản phẩm.
- Vì không có duyệt hai người hay quyền khẩn cấp, quyền tối thiểu, xác minh lại và nhật ký bất biến là kiểm soát bắt buộc.
- Mọi tích hợp ngoài phải có chủ sở hữu, dữ liệu tối thiểu, hạn mức, phương án suy giảm và kế hoạch thay thế.

## 7. Nguyên tắc duy trì tài liệu

Kế hoạch thực thi đã chọn phạm vi 145 task cho Giai đoạn A và 202 task cho Giai đoạn B tại [Kế hoạch triển khai Giai đoạn A–B](../03-ke-hoach-giai-doan-a/KE-HOACH-TRIEN-KHAI-GIAI-DOAN-A-B.md). Kết quả baseline, hồ sơ REL và thứ tự đóng Cổng A được hợp nhất tại [Tổng hợp sai lệch và thứ tự triển khai Giai đoạn A](../03-ke-hoach-giai-doan-a/TONG-HOP-SAI-LECH-VA-THU-TU-TRIEN-KHAI-GIAI-DOAN-A.md). Các gói công việc gồm [Lát 0–Lát 1](../03-ke-hoach-giai-doan-a/GOI-TASK-THUC-THI-LAT-0-1-GIAI-DOAN-A.md), [Lát 2](../03-ke-hoach-giai-doan-a/GOI-TASK-THUC-THI-LAT-2-GIAI-DOAN-A.md), [Lát 3A–3D](../03-ke-hoach-giai-doan-a/GOI-TASK-THUC-THI-LAT-3A-3D-GIAI-DOAN-A.md) và [Lát 4–Lát 5](../03-ke-hoach-giai-doan-a/GOI-TASK-THUC-THI-LAT-4-5-GIAI-DOAN-A.md). [Bảng import tổng Giai đoạn A](../03-ke-hoach-giai-doan-a/BANG-IMPORT-TONG-GIAI-DOAN-A.md) hợp nhất 145 task nguồn và 22 task điều phối để gán người và nhập công cụ quản lý công việc. [Kế hoạch khởi động Lát 0](../04-thuc-thi/lat-0/KE-HOACH-KHOI-DONG.md) quy định thứ tự mở 12 công việc A0, vai trò, điều kiện bắt đầu và đầu ra bắt buộc. [Bộ hồ sơ thực thi Lát 0](../04-thuc-thi/lat-0/HO-SO-THUC-THI.md) chứa năm hồ sơ REL và bảy biên bản CT đã mở sẵn. [Kiểm tra bao phủ và checklist quyết định](../03-ke-hoach-giai-doan-a/KIEM-TRA-BAO-PHU-VA-QUYET-DINH-CONG-A.md) quản lý đủ 145 task nguồn. Bộ biểu mẫu bằng chứng nằm tại [Bộ mẫu Cổng A](../05-bang-chung/cong-a/README.md).

- Mỗi thay đổi nghiệp vụ cần cập nhật module sở hữu trách nhiệm và các quan hệ đầu vào, đầu ra liên quan.
- Một quy tắc chỉ nên có một module làm nguồn sự thật; module khác tham chiếu kết quả thay vì duy trì bản sao logic.
- Task nâng cấp nên ghi rõ module chính, module bị ảnh hưởng, luồng dữ liệu thay đổi và tiêu chí đo kết quả.
- Các giả định chưa xác nhận phải được chuyển vào phần “Điểm cần làm rõ” của module tương ứng.
