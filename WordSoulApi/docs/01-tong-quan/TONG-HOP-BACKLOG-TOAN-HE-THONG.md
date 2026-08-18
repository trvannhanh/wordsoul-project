# Tổng hợp backlog toàn hệ thống WordSoul

## 1. Mục đích và phạm vi

Tài liệu này hợp nhất kết quả phân tích chuyên sâu của 12 module thành một bản đồ công việc cấp hệ thống. Mục tiêu là:

- Cung cấp một điểm vào duy nhất để tra cứu toàn bộ backlog và sổ quyết định.
- Xác định rõ module sở hữu từng loại dữ liệu và kết quả nghiệp vụ dùng chung.
- Loại bỏ sự trùng lặp về trách nhiệm mà không xóa hoặc đổi mã task gốc.
- Chuẩn hóa hướng phụ thuộc, phân biệt phụ thuộc triển khai với vòng phản hồi nghiệp vụ hợp lệ.
- Xác định đường công việc trọng yếu, các luồng có thể thực hiện song song và phạm vi phát hành theo từng mức trưởng thành.

Tài liệu không thay thế backlog chi tiết của từng module. Mọi task vẫn được quản lý tại file `TASK-BACKLOG.md` tương ứng; tài liệu này chỉ tổ chức chúng thành một danh mục và lộ trình thống nhất.

## 2. Kết quả kiểm kê

| Chỉ số | Kết quả |
|---|---:|
| Module đã phân tích | 12/12 |
| Chức năng con | 120 |
| Task có mã duy nhất | 561 |
| Quyết định có mã duy nhất | 305 |
| Quyết định đã chốt | 305 |
| Quyết định còn mở | 0 |
| Quyết định chốt một phần | 0 |
| Task ưu tiên Cao | 480 |
| Task ưu tiên Trung bình | 78 |
| Task ưu tiên Thấp | 3 |
| Task độ phức tạp L | 300 |
| Task độ phức tạp M | 241 |
| Task độ phức tạp S | 20 |

### 2.1. Chỉ mục backlog và sổ quyết định

| Module | Phạm vi Task ID | Số task | Phạm vi quyết định | Số quyết định | Backlog chi tiết | Sổ quyết định |
|---|---|---:|---|---:|---|---|
| M01 — Danh tính và hồ sơ | M01-T001–M01-T043 | 43 | M01-D001–M01-D025 | 25 | [Mở backlog](../02-modules/01-danh-tinh-va-ho-so/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/01-danh-tinh-va-ho-so/QUYET-DINH-MO.md) |
| M02 — Nội dung từ vựng | M02-T001–M02-T046 | 46 | M02-D001–M02-D025 | 25 | [Mở backlog](../02-modules/02-noi-dung-tu-vung/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/02-noi-dung-tu-vung/QUYET-DINH-MO.md) |
| M03 — Phiên học và kiểm tra | M03-T001–M03-T047 | 47 | M03-D001–M03-D024 | 24 | [Mở backlog](../02-modules/03-phien-hoc-va-kiem-tra/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/03-phien-hoc-va-kiem-tra/QUYET-DINH-MO.md) |
| M04 — Ôn tập và tiến độ | M04-T001–M04-T046 | 46 | M04-D001–M04-D025 | 25 | [Mở backlog](../02-modules/04-on-tap-va-tien-do/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/04-on-tap-va-tien-do/QUYET-DINH-MO.md) |
| M05 — Luyện phát âm | M05-T001–M05-T043 | 43 | M05-D001–M05-D024 | 24 | [Mở backlog](../02-modules/05-luyen-phat-am/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/05-luyen-phat-am/QUYET-DINH-MO.md) |
| M06 — Thú cưng, vật phẩm và kinh tế | M06-T001–M06-T047 | 47 | M06-D001–M06-D025 | 25 | [Mở backlog](../02-modules/06-thu-cung-vat-pham-kinh-te/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/06-thu-cung-vat-pham-kinh-te/QUYET-DINH-MO.md) |
| M07 — Nhiệm vụ và thành tựu | M07-T001–M07-T046 | 46 | M07-D001–M07-D025 | 25 | [Mở backlog](../02-modules/07-nhiem-vu-va-thanh-tuu/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/07-nhiem-vu-va-thanh-tuu/QUYET-DINH-MO.md) |
| M08 — Đấu trường và phòng thử thách | M08-T001–M08-T048 | 48 | M08-D001–M08-D026 | 26 | [Mở backlog](../02-modules/08-dau-truong-va-phong-thu-thach/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/08-dau-truong-va-phong-thu-thach/QUYET-DINH-MO.md) |
| M09 — Nhóm và xếp hạng | M09-T001–M09-T047 | 47 | M09-D001–M09-D025 | 25 | [Mở backlog](../02-modules/09-nhom-va-xep-hang/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/09-nhom-va-xep-hang/QUYET-DINH-MO.md) |
| M10 — Thông báo và duy trì tương tác | M10-T001–M10-T048 | 48 | M10-D001–M10-D026 | 26 | [Mở backlog](../02-modules/10-thong-bao-va-duy-tri-tuong-tac/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/10-thong-bao-va-duy-tri-tuong-tac/QUYET-DINH-MO.md) |
| M11 — Quản trị và vận hành | M11-T001–M11-T050 | 50 | M11-D001–M11-D027 | 27 | [Mở backlog](../02-modules/11-quan-tri-va-van-hanh/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/11-quan-tri-va-van-hanh/QUYET-DINH-MO.md) |
| M12 — Tích hợp nền tảng và tài sản số | M12-T001–M12-T050 | 50 | M12-D001–M12-D028 | 28 | [Mở backlog](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/TASK-BACKLOG.md) | [Mở sổ quyết định](../02-modules/12-tich-hop-nen-tang-va-tai-san-so/QUYET-DINH-MO.md) |

## 3. Nguyên tắc hợp nhất

1. Không xóa, đổi mã hoặc gộp vật lý các task gốc. Mã task là định danh truy vết ổn định.
2. Khi nhiều task cùng hướng tới một đầu ra, chỉ định một module sở hữu. Task tại module khác trở thành công việc cung cấp đầu vào, tích hợp hoặc kiểm chứng chấp nhận.
3. Module phát sinh nghiệp vụ sở hữu ý nghĩa của sự kiện; module tiêu thụ sở hữu quy tắc chống xử lý lặp.
4. Module nghiệp vụ sở hữu quyết định cuối cùng; module dùng chung chỉ cung cấp năng lực vận hành hoặc tích hợp.
5. Trạng thái cốt lõi phải hoàn thành độc lập với thông báo, bảng xếp hạng, phân tích hoặc dịch vụ bên ngoài.
6. Vòng phản hồi học tập được phép tồn tại ở cấp nghiệp vụ, nhưng mỗi lần trao đổi phải có đầu ra hoàn chỉnh và không tạo phụ thuộc triển khai đồng thời.

## 4. Bản đồ sở hữu đầu ra dùng chung

| Nhóm đầu ra có dấu hiệu trùng | Module sở hữu | Module cung cấp hoặc tiêu thụ | Quy tắc hợp nhất |
|---|---|---|---|
| Danh tính, trạng thái tài khoản, hồ sơ, múi giờ và lựa chọn riêng tư | M01 | M03–M12 | M01 là nguồn sự thật. Module khác chỉ giữ tham chiếu hoặc bản chụp cần thiết cho nghiệp vụ của mình. |
| Thuật ngữ học thuật: đã học, trả lời đúng, hoàn thành, đến hạn, thành thạo | M03 và M04 theo ranh giới | M02, M05, M07, M09, M11 | M03 sở hữu kết quả phiên và câu trả lời; M04 sở hữu trạng thái ghi nhớ và lịch ôn. Không tạo một định nghĩa thứ ba. |
| Nội dung từ vựng và vòng đời xuất bản | M02 | M03, M04, M05, M08, M11, M12 | M02 quyết định nội dung có giá trị học thuật và được phép sử dụng; M11 cung cấp quy trình duyệt; M12 cung cấp năng lực tạo hoặc lưu tài sản. |
| Nội dung do AI hỗ trợ | M02 | M11, M12 | M02 sở hữu yêu cầu chất lượng và quyết định xuất bản; M12 sở hữu việc sử dụng nhà cung cấp; M11 sở hữu kiểm soát phê duyệt và truy vết. |
| Kết quả phiên học và bằng chứng trả lời | M03 | M04, M06, M07, M09, M11 | M03 phát hành kết quả đã hoàn tất; module tiêu thụ không sửa ngược lịch sử phiên. Sai sót được xử lý bằng bản điều chỉnh có truy vết. |
| Trạng thái ghi nhớ, lịch ôn và tiến độ học | M04 | M03, M05, M07, M09, M10, M11 | M04 là nguồn sự thật về người dùng–từ. M03 chỉ yêu cầu danh sách ôn và gửi kết quả mới. |
| Điểm và phản hồi phát âm | M05 | M04, M07, M11, M12 | M12 trả kết quả thô từ dịch vụ; M05 áp dụng chính sách đạt và phản hồi; tác động sang M04 là tùy chọn đã được phê duyệt. |
| Số dư, vật phẩm, thú cưng, hiệu ứng và lịch sử thưởng | M06 | M03, M05, M07, M08, M09, M11 | Chỉ M06 thay đổi tài sản. Module nguồn gửi điều kiện hoặc yêu cầu thưởng, không tự duy trì số dư. |
| Nhiệm vụ, thành tựu và trạng thái nhận thưởng | M07 | M03, M04, M05, M06, M10, M11 | M07 đánh giá điều kiện và trạng thái nhận; M06 thực hiện cấp tài sản đúng một lần. |
| Trận đấu, kết quả trận và điểm cạnh tranh hiện tại | M08 | M02, M06, M09, M10, M11, M12 | M08 quyết định kết quả hợp lệ và biến động điểm của trận; M09 chỉ tổng hợp theo mùa hoặc phạm vi bảng xếp hạng. |
| Nhóm, mùa và bảng xếp hạng tổng hợp | M09 | M01, M04, M08, M10, M11 | M09 sở hữu thành viên nhóm, kỳ xếp hạng và dữ liệu tổng hợp; không tính lại kết quả trận. |
| Nội dung thông báo, lựa chọn nhận tin, lịch và hộp thư | M10 | M01, M04, M07, M08, M09, M11, M12 | Module nguồn phát tín hiệu; M10 quyết định có gửi, gửi khi nào và qua kênh nào; M12 thực hiện chuyển phát. |
| Vai trò quản trị, phê duyệt, kiểm toán và cấu hình vận hành | M11 | M01–M10, M12 | M11 sở hữu quy trình quản trị chung; module nghiệp vụ vẫn sở hữu kiểm tra hợp lệ và trạng thái chuyên môn của mình. |
| Tài sản số, nhà cung cấp ngoài, hạn mức, bí mật và khả năng suy giảm | M12 | M01, M02, M05, M08, M10, M11 | M12 sở hữu vòng đời tích hợp và tài sản số; module gọi quyết định dữ liệu nào cần gửi và kết quả nào đủ điều kiện nghiệp vụ. |
| Hồ sơ hỗ trợ, khiếu nại và bồi hoàn | M11 | M01, M03, M06, M08, M09, M10 | M11 sở hữu hồ sơ xử lý; module nguồn cung cấp bằng chứng và thực hiện điều chỉnh trong phạm vi mình sở hữu. |
| Chỉ số, từ điển đo lường và bảng quan sát | M11 | M01–M10, M12 | Module nguồn định nghĩa dữ liệu nghiệp vụ gốc; M11 thống nhất tên, công thức tổng hợp, quyền xem và hiển thị. |
| Quyền xóa dữ liệu và thời hạn lưu giữ | M01 và M11 theo ranh giới | M02–M10, M12 | M01 tiếp nhận yêu cầu của người dùng; mỗi module xử lý dữ liệu mình sở hữu; M11 quản trị chính sách và bằng chứng; M12 xử lý bản sao bên ngoài. |

### 4.1. Kết luận xử lý task trùng

Không có task nào bị loại chỉ vì có cùng chủ đề. Các task tương tự được giữ lại nếu tạo ra một trong ba kết quả khác nhau:

- Đầu ra nghiệp vụ do module sở hữu tạo ra.
- Thỏa thuận trao đổi dữ liệu tại module nguồn hoặc module tiêu thụ.
- Tiêu chí chấp nhận và bằng chứng vận hành ở module quản trị hoặc tích hợp.

Nhờ vậy, backlog vẫn truy vết đủ 561 task nhưng không tạo nhiều nguồn sự thật cho cùng một loại dữ liệu.

## 5. Chuẩn hóa hướng phụ thuộc và xử lý vòng lặp

| Quan hệ có nguy cơ tạo vòng | Hướng phụ thuộc sau chuẩn hóa | Cách phá vòng |
|---|---|---|
| M01 ↔ M10 | M01 → M10; M10 chỉ phản hồi trạng thái gửi | Danh tính, thông tin liên hệ và lựa chọn riêng tư tồn tại độc lập. Thất bại thông báo không làm thay đổi trạng thái tài khoản. |
| M02 ↔ M12 | M02 → M12 cho năng lực tùy chọn | Nội dung thủ công phải hoạt động không cần AI. M12 không phê duyệt hoặc xuất bản học liệu. |
| M03 ↔ M04 | M03 → M04 sau phiên; M04 → M03 khi chuẩn bị phiên mới | Đây là vòng phản hồi theo thời gian, không phải vòng triển khai. Kết quả phiên được chốt trước khi cập nhật tiến độ; danh sách ôn là đầu vào của phiên kế tiếp. |
| M04 ↔ M05 | M05 → M04 theo chính sách tùy chọn | M05 luôn lưu được kết quả phát âm dù M04 không tiếp nhận; M04 lõi không phụ thuộc bắt buộc vào điểm phát âm. |
| M06 ↔ M07 | M07 → M06 để yêu cầu thưởng; M06 → M07 chỉ trả kết quả cấp | M06 sở hữu sổ tài sản và có thể hoàn thiện trước nguồn thưởng. M07 không tự sửa số dư khi yêu cầu thất bại. |
| M06 ↔ M08 | M08 → M06 để yêu cầu thưởng; M06 → M08 cung cấp ảnh chụp tài sản hợp lệ | Kết quả trận được chốt trước yêu cầu thưởng. Thưởng thất bại được xử lý lại, không đảo kết quả trận. |
| M08 ↔ M09 | M08 → M09 cho kết quả và điểm trận | M08 sở hữu kết quả/điểm cạnh tranh hiện tại; M09 sở hữu mùa và bảng tổng hợp. Ghép trận đọc ảnh chụp điểm do M08 quản lý, không yêu cầu M09 tính lại trận. |
| M08/M09 ↔ M10 | M08/M09 → M10 | Thông báo tiêu thụ tín hiệu sau khi nghiệp vụ nguồn hoàn tất. Gửi tin thất bại không hoàn tác trận, nhóm hay thứ hạng. |
| M11 ↔ các module nghiệp vụ | M11 cung cấp quy trình quản trị; module nghiệp vụ quyết định hợp lệ | Khung quyền, phê duyệt và kiểm toán được làm sớm; từng tích hợp được bổ sung dần. M11 không thay thế logic chuyên môn. |
| M12 ↔ module tiêu thụ | Module nghiệp vụ → M12; M12 trả kết quả kỹ thuật | Xác định trước mức quan trọng và phương án suy giảm. M12 không quyết định đăng nhập, xuất bản, đạt bài hay thắng trận. |

Sau khi áp dụng các quy tắc trên, không còn vòng phụ thuộc kiến trúc bắt buộc. Các vòng còn lại là vòng phản hồi nghiệp vụ qua các thời điểm khác nhau hoặc quan hệ yêu cầu–kết quả có trạng thái kết thúc rõ ràng.

## 6. Sơ đồ phụ thuộc đã chuẩn hóa

```text
M01 Danh tính ─────┬───────────────┬─────────────────────────────┐
                   v               v                             v
             M02 Học liệu      M11 Quản trị                 M12 Tích hợp
                   |               ^                             ^
                   v               |                             |
             M03 Phiên học ────────┼──────────────┐              |
                   |               |              v              |
                   v               |         M05 Phát âm ────────┘
             M04 Ôn tập ───────────┼──────────┐
                   |               |          |
                   ├───────────────┘          v
                   v                      M06 Kinh tế
             M07 Nhiệm vụ ────────────────┘
                   |
                   v
             M10 Thông báo

Nhánh xã hội và thi đấu:
M01 + M04 ──> M09 Nhóm/xếp hạng
M01 + M02 + M06 ──> M08 Đấu trường ──> M09
M07 + M08 + M09 ──> M10

M11 quản trị và M12 tích hợp hỗ trợ xuyên suốt, nhưng không sở hữu quyết định
nghiệp vụ của các module nằm trên luồng giá trị.
```

## 7. Đường công việc trọng yếu

### 7.1. Critical path cho phát hành ban đầu

```text
Quyết định nền tảng
        ↓
M01 Danh tính và hồ sơ
        ↓
M02 Nội dung thủ công đã duyệt
        ↓
M03 Phiên học và kết quả
        ↓
M04 Ôn tập và tiến độ
        ↓
M06 Sổ tài sản và cấp thưởng tối thiểu
        ↓
M07 Nhiệm vụ ngắn hạn
        ↓
M10 Hộp thư và nhắc học cơ bản
        ↓
Phát hành ban đầu
```

M11 và M12 không nằm cuối đường này. Các phần nền tảng của chúng phải chạy song song từ đầu: quyền quản trị tối thiểu, kiểm toán, cấu hình, quản lý tài sản số, chuyển phát và khả năng xử lý lỗi. Chỉ các năng lực nâng cao mới được triển khai sau.

### 7.2. Các nhánh có thể thực hiện song song

| Luồng công việc | Điều kiện vào | Đầu ra đóng góp | Có thể song song với |
|---|---|---|---|
| Quản trị và kiểm soát | Ranh giới M01 và danh mục dữ liệu sơ bộ | Vai trò quản trị, phê duyệt, kiểm toán, cấu hình và quan sát tối thiểu | Học liệu, phiên học, kinh tế |
| Tích hợp và tài sản số | Mức quan trọng của từng nhu cầu ngoài đã được phân loại | Năng lực dùng chung có phương án suy giảm | M01–M11 theo nhu cầu |
| Học thuật cốt lõi | M01 và nội dung mẫu đã rõ | Chuỗi M02 → M03 → M04 | Kinh tế nền tảng, quản trị |
| Kinh tế nền tảng | Danh tính và nguyên tắc thưởng đã rõ | Sổ tài sản, chống cấp lặp, lịch sử biến động | M03/M04; tích hợp M07 sau |
| Luyện phát âm | Nội dung và quy tắc riêng tư âm thanh đã rõ | Kết quả luyện độc lập, ảnh hưởng tiến độ tùy chọn | M04, M06, M07 |
| Nhiệm vụ và thành tựu | Sự kiện học ổn định và M06 sẵn sàng | Mục tiêu hành vi và yêu cầu thưởng | Phát âm, nhánh xã hội |
| Nhóm và xếp hạng | Danh tính cùng chỉ số tiến độ đã ổn định | Nhóm, mùa, bảng tổng hợp | Nhiệm vụ, phát âm |
| Đấu trường | Học liệu, tài sản và luật kết quả đã rõ | Thử thách cá nhân trước; đối kháng sau | M09 và M10, theo từng phần |
| Duy trì tương tác | Từng nguồn tín hiệu đã có trạng thái hoàn tất | Nhắc học, hộp thư, truyền thông | Tích hợp dần từng nguồn |

## 8. Phân kỳ backlog đề xuất

Phân kỳ dưới đây là phạm vi nghiệp vụ, không phải cam kết thời gian. Chỉ nên gán mốc sau khi có năng lực đội ngũ, mức chất lượng và giới hạn vận hành được thống nhất.

Phạm vi task cụ thể đã được chọn tại [Kế hoạch triển khai Giai đoạn A–B](../03-ke-hoach-giai-doan-a/KE-HOACH-TRIEN-KHAI-GIAI-DOAN-A-B.md): Giai đoạn A gồm 145 task, Giai đoạn B gồm 202 task và 214 task còn lại thuộc C–E.

### 8.1. Giai đoạn A — Chốt nền tảng và khả năng vận hành tối thiểu

| Nhóm | Phạm vi cần đạt | Module chính |
|---|---|---|
| Danh tính và an toàn tài khoản | Vòng đời tài khoản, hồ sơ tối thiểu, trạng thái truy cập, quyền riêng tư, múi giờ | M01 |
| Quản trị tối thiểu | Vai trò quản trị, thao tác nhạy cảm, phê duyệt học liệu, kiểm toán và cấu hình cơ bản | M11 |
| Tích hợp tối thiểu | Danh mục năng lực ngoài, mức quan trọng, giới hạn dữ liệu, tài sản số và phương án suy giảm | M12 |
| Quyết định xuyên module | Thuật ngữ học tập, đơn vị thưởng, ngày học, quyền dữ liệu và nguyên tắc xử lý lặp | M01–M12 |

Điều kiện ra: có thể quản lý người dùng và học liệu mẫu an toàn; mỗi phụ thuộc ngoài quan trọng có chủ sở hữu và cách xử lý khi gián đoạn.

### 8.2. Giai đoạn B — Phát hành ban đầu

| Khả năng người dùng | Phạm vi đưa vào | Module chính |
|---|---|---|
| Bắt đầu học | Duyệt nội dung thủ công, chọn bộ từ, tạo và hoàn thành phiên | M02, M03 |
| Ghi nhớ dài hạn | Ghi nhận trạng thái người dùng–từ, lịch ôn, hàng đợi ôn và tiến độ cơ bản | M04 |
| Động lực tối thiểu | Một số loại thưởng, sổ tài sản, nhiệm vụ ngày và nhận thưởng đúng một lần | M06, M07 |
| Quay lại học | Hộp thư, nhắc từ đến hạn, lựa chọn nhận tin, giờ yên lặng | M10, M12 |
| Vận hành | Quản lý nội dung, hỗ trợ tài khoản, chỉ số lõi và cảnh báo tối thiểu | M11 |

Không bắt buộc trong phát hành ban đầu: tạo học liệu bằng AI, phát âm chấm điểm sâu, thú cưng tiến hóa, bảng xếp hạng, đối kháng thời gian thực và chiến dịch truyền thông nâng cao.

### 8.3. Giai đoạn C — Nâng cao giá trị học thuật và gamification

| Nhóm nâng cấp | Phạm vi | Module chính |
|---|---|---|
| Phát âm | Thu âm, đánh giá, phản hồi, lịch sử và tác động tiến độ đã được kiểm chứng | M05, M04, M12 |
| Nội dung hỗ trợ thông minh | Gợi ý hoặc tạo nội dung có kiểm duyệt, theo dõi nguồn và chất lượng | M02, M11, M12 |
| Kinh tế mở rộng | Thú cưng, tiến hóa, vật phẩm, hiệu ứng và cân bằng kinh tế | M06, M11 |
| Thành tựu dài hạn | Chuỗi mục tiêu, thành tựu và cá nhân hóa phù hợp | M07, M10 |
| Thử thách cá nhân | Hoạt động thi đấu không cần đối thủ trực tiếp | M08, M02, M06 |

### 8.4. Giai đoạn D — Xã hội và cạnh tranh

| Nhóm nâng cấp | Phạm vi | Module chính |
|---|---|---|
| Cộng đồng | Nhóm, thành viên, quyền riêng tư và hoạt động chung | M09, M01 |
| Xếp hạng | Mùa, phạm vi xếp hạng, chống gian lận, khóa và chốt kết quả | M09, M11 |
| Đấu trường đối kháng | Ghép trận, trạng thái trận, công bằng, mất kết nối, kết quả, tranh chấp | M08, M09, M11, M12 |
| Tương tác theo sự kiện | Lời mời, kết quả, thay đổi thứ hạng và thông báo nhóm | M10, M12 |

### 8.5. Giai đoạn E — Tối ưu và mở rộng quy mô

| Nhóm nâng cấp | Phạm vi | Module chính |
|---|---|---|
| Quan sát nâng cao | Từ điển chỉ số hoàn chỉnh, phân tích hành vi, đối soát và quy trình ứng phó | M11 và module nguồn |
| Tối ưu tương tác | Phân nhóm, thử nghiệm, giới hạn tần suất và đánh giá hiệu quả dài hạn | M10, M11 |
| Độ bền tích hợp | Nhiều nhà cung cấp, chuyển đổi, kiểm soát chi phí và phục hồi sự cố lớn | M12, M11 |
| Cân bằng hệ thống | Điều chỉnh học thuật, kinh tế, nhiệm vụ và cạnh tranh dựa trên bằng chứng | M04, M06, M07, M08, M09, M11 |

## 9. Cổng quyết định toàn hệ thống

Các quyết định cấp hệ thống dưới đây đã được giải quyết. Khi phạm vi thay đổi, mọi đề xuất sửa quyết định phải ghi rõ lý do, người chịu trách nhiệm, tác động liên module và các task cần đánh giá lại.

| Mã | Quyết định cần chốt | Module dẫn dắt | Module bị ảnh hưởng | Hậu quả nếu để mở |
|---|---|---|---|---|
| G-D01 | Đối tượng tuổi, cơ sở đồng ý và yêu cầu bảo vệ người học | M01, M11 | Tất cả | Đã chốt theo lựa chọn sản phẩm ngày 2026-08-11; còn là điểm chặn rà soát pháp lý, xem mục 9.5. |
| G-D02 | Định nghĩa thống nhất cho đã học, hoàn thành, đến hạn và thành thạo | M03, M04 | M02, M05, M07, M09, M10, M11 | Đã chốt ngày 2026-08-11; xem mục 9.1. |
| G-D03 | Định danh hoạt động, chống xử lý lặp và cách điều chỉnh lịch sử | M11 | M03–M10, M12 | Đã chốt ngày 2026-08-11; xem mục 9.3. |
| G-D04 | Đơn vị giá trị, chính sách số dư, cấp thưởng và thu hồi | M06 | M03, M05, M07, M08, M11 | Đã chốt ngày 2026-08-11; xem mục 9.4. |
| G-D05 | Múi giờ, ranh giới ngày học, mùa và giờ yên lặng | M01 | M04, M07, M09, M10, M11 | Đã chốt ngày 2026-08-11; xem mục 9.2. |
| G-D06 | Tiêu chuẩn học liệu, quyền sử dụng nguồn, kiểm duyệt AI và xuất bản | M02, M11 | M03, M05, M08, M12 | Đã chốt theo lựa chọn sản phẩm ngày 2026-08-11; xem mục 9.6. |
| G-D07 | Ngưỡng đạt phát âm và mức tác động tới tiến độ | M05, M04 | M07, M11, M12 | Đã chốt ngày 2026-08-11; xem mục 9.7. |
| G-D08 | Chủ sở hữu điểm trận, mùa, xếp hạng và cách sửa kết quả | M08, M09 | M06, M10, M11 | Đã chốt ngày 2026-08-11; xem mục 9.8. |
| G-D09 | Đồng ý nhận tin, kênh được phép, mức khẩn và giới hạn tần suất | M10, M01 | M04, M07, M08, M09, M11, M12 | Đã chốt theo lựa chọn sản phẩm ngày 2026-08-11; xem mục 9.9. |
| G-D10 | Mô hình vai trò quản trị và phê duyệt thao tác nhạy cảm | M11, M01 | M02, M06–M10, M12 | Đã chốt theo lựa chọn sản phẩm ngày 2026-08-11; xem mục 9.10. |
| G-D11 | Thời hạn lưu, tải xuống, ẩn danh hóa và xóa dữ liệu | M01, M11 | M02–M10, M12 | Đã chốt ngày 2026-08-11; xem mục 9.11. |
| G-D12 | Dữ liệu được gửi ra ngoài, bí mật, hạn mức chi phí và mức dịch vụ | M12, M11 | M01, M02, M05, M08, M10 | Đã chốt ngày 2026-08-11; xem mục 9.12. |

### 9.1. Tiến độ xử lý quyết định G-D02

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Đã học | Một từ được xem là đã học khi người học hoàn thành ít nhất một hoạt động học hợp lệ do M03 xác nhận. Chỉ mở, xem từ hoặc xem thẻ không được tính. | Đã chốt | 2026-08-11 | M03, M04, M07; các chỉ số và module tiêu thụ phải dùng cùng định nghĩa |
| Hoàn thành | Một phiên học hoàn thành khi mọi nội dung bắt buộc đã có kết quả cuối cùng và M03 chốt phiên. Cấp thưởng, cập nhật nhiệm vụ hoặc gửi thông báo được xử lý sau; lỗi ở các bước này không hủy trạng thái hoàn thành. | Đã chốt | 2026-08-11 | M03 sở hữu trạng thái; M04, M06, M07, M10 và M11 tiêu thụ kết quả đã chốt |
| Đến hạn | Một từ đến hạn khi thời điểm hiện tại đạt hoặc vượt thời điểm ôn do M04 xác định. Hệ thống giữ thời điểm thống nhất; việc nhóm và hiển thị dùng ngày học theo múi giờ người dùng. | Đã chốt | 2026-08-11 | M04 sở hữu thời điểm ôn; M03, M07, M10 và M11 sử dụng cùng trạng thái đến hạn |
| Thành thạo | Một từ thành thạo khi có nhiều bằng chứng gợi nhớ đúng ở các lần ôn cách nhau theo thời gian và khoảng ôn đã đạt ngưỡng tối thiểu. Một lần đúng hoặc nhiều lần xem không đủ; trạng thái có thể mất khi có bằng chứng quên. | Đã chốt | 2026-08-11 | M04 sở hữu trạng thái; M07, M09 và M11 chỉ sử dụng kết quả đã xác định |

**Trạng thái G-D02:** Đã chốt toàn bộ ngày 2026-08-11. Mọi module phải sử dụng bốn định nghĩa trên và không tự tạo ngưỡng hoặc trạng thái tương đương khác.

### 9.2. Tiến độ xử lý quyết định G-D05

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Nguồn múi giờ | Múi giờ lưu trong hồ sơ người dùng là nguồn chính. Thiết bị chỉ đề xuất thay đổi; hệ thống không tự đổi nếu người dùng chưa xác nhận. | Đã chốt | 2026-08-11 | M01 sở hữu; M04, M07, M09, M10 và M11 tiêu thụ |
| Ranh giới ngày học | Ngày học trùng với ngày lịch tại múi giờ hồ sơ, bắt đầu lúc 00:00 và kết thúc ngay trước 00:00 ngày kế tiếp. | Đã chốt | 2026-08-11 | M03, M04, M07, M10 và báo cáo theo ngày |
| Hiệu lực khi đổi múi giờ | Hồ sơ và hiển thị đổi ngay; lịch ôn giữ nguyên thời điểm tuyệt đối. Nhiệm vụ và chuỗi ngày đang chạy dùng múi giờ cũ đến hết ngày học hiện tại, rồi áp dụng múi giờ mới từ ngày kế tiếp. | Đã chốt | 2026-08-11 | M01, M04, M07; M09 và M10 sử dụng hồ sơ đã xác nhận theo quy tắc riêng của chu kỳ |
| Ranh giới mùa | Mỗi mùa có thời điểm bắt đầu và kết thúc tuyệt đối dùng chung cho toàn hệ thống, được công bố trước. Múi giờ hồ sơ chỉ dùng để quy đổi cách hiển thị. | Đã chốt | 2026-08-11 | M08, M09, M10, M11 |
| Khung giờ yên lặng | Mặc định từ 22:00 đến 07:00 theo múi giờ hồ sơ. Người dùng có thể chỉnh hoặc tắt; khoảng thời gian đi qua nửa đêm được hỗ trợ. | Đã chốt | 2026-08-11 | M01, M10, M12 |
| Ngoại lệ giờ yên lặng | Chỉ thông báo bảo mật hoặc an toàn tài khoản cần hành động ngay được phép gửi. Nhắc học, gamification, nhóm, xếp hạng và thi đấu phải chờ hết giờ yên lặng hoặc hết hạn mà không gửi. | Đã chốt | 2026-08-11 | M01, M10, M11, M12 |

**Trạng thái G-D05:** Đã chốt toàn bộ ngày 2026-08-11. M01 sở hữu múi giờ hồ sơ; các module tiêu thụ không được tự suy đoán múi giờ từ thiết bị hoặc thay đổi ranh giới chu kỳ đang chạy.

### 9.3. Tiến độ xử lý quyết định G-D03

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Định danh và gửi lại | Mỗi hoạt động nghiệp vụ có một mã duy nhất, bất biến, do module nguồn tạo. Mọi lần gửi lại dùng cùng mã; module nhận trả cùng kết quả đã xử lý và không thực hiện hiệu ứng thêm lần nữa. | Đã chốt | 2026-08-11 | M03–M10, M12; M11 kiểm toán nguyên tắc chung |
| Cùng mã nhưng khác nội dung | Từ chối lần gửi sau, giữ nguyên nội dung và kết quả đầu tiên, ghi nhận sự cố và phát cảnh báo kiểm tra. Không ghi đè hoặc coi nội dung mới là hoạt động mới. | Đã chốt | 2026-08-11 | M03–M10, M12; M11 tiếp nhận dấu vết và cảnh báo |
| Điều chỉnh lịch sử | Không sửa hoặc xóa bản gốc. Tạo bản điều chỉnh mới có lý do, người thực hiện và tham chiếu bản gốc; mỗi module sở hữu tạo ảnh hưởng bù trừ tương ứng. | Đã chốt | 2026-08-11 | M03, M04, M06–M09, M11; chính sách số dư âm vẫn thuộc G-D04 |
| Hoạt động đến muộn hoặc sai thứ tự | Lưu thời điểm xảy ra và thời điểm tiếp nhận. Chấp nhận trong cửa sổ giới hạn theo loại hoạt động; nếu chu kỳ đã chốt thì tạo điều chỉnh có truy vết. Hoạt động quá cửa sổ được giữ để kiểm tra, không âm thầm bỏ hoặc tự động tính sang chu kỳ mới. | Đã chốt | 2026-08-11 | M04, M07–M10, M11; độ dài cửa sổ do module sở hữu cấu hình |

**Trạng thái G-D03:** Đã chốt toàn bộ ngày 2026-08-11. Thời lượng lưu mã đã xử lý và độ dài cửa sổ đến muộn là cấu hình vận hành của từng loại hoạt động, nhưng không được làm thay đổi bốn nguyên tắc trên.

### 9.4. Tiến độ xử lý quyết định G-D04

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Đơn vị giá trị hoạt động | XP người dùng biểu thị tiến trình; XP thú cưng là loại riêng; lượt gợi ý là tài sản tiêu dùng. AP ngừng được cấp và sử dụng. Phát triển cấp thú cưng dùng nhóm vật phẩm tăng trưởng riêng; tên và loại vật phẩm cụ thể chưa chốt. Không có tiền mềm, cửa hàng hoặc giao dịch giữa người dùng trong phạm vi hiện tại. | Đã chốt | 2026-08-11 | M06 sở hữu; M03, M07, M08 và M11 tiêu thụ |
| Xử lý AP hiện có | Ngừng AP và loại bỏ số dư khả dụng hiện có mà không chuyển đổi hoặc bồi hoàn. Lịch sử biến động AP vẫn được giữ để kiểm toán theo G-D03. | Đã chốt | 2026-08-11 | M06, M11 và người dùng đang có AP; cần truyền thông, thống kê ảnh hưởng và phương án phục hồi trước khi thực hiện |
| Nguồn sự thật số dư | Lịch sử biến động bất biến là nguồn sự thật. Số dư hiện tại là kết quả tổng hợp để sử dụng nhanh và phải được đối soát định kỳ với lịch sử. | Đã chốt | 2026-08-11 | M06 sở hữu; M11 kiểm toán và theo dõi đối soát |
| Cấp gói nhiều loại tài sản | Kiểm tra toàn bộ gói trước; chỉ ghi nhận khi mọi thành phần hợp lệ. Gói được cấp toàn bộ hoặc không cấp, và gửi lại cùng mã hoạt động trả đúng kết quả cũ. | Đã chốt | 2026-08-11 | M03, M05, M06–M08 |
| Chính sách số dư âm | Không cho số dư âm. Chỉ thu hồi phần còn khả dụng đến mức 0; phần chưa thu hồi chuyển thành khoản hoặc vụ việc xử lý riêng và không tự động trừ vào phần thưởng tương lai. | Đã chốt | 2026-08-11 | M06, M07, M08, M11 |
| Phạm vi thu hồi khi tài sản đã sử dụng | Tạo vụ việc có truy vết; chỉ tự động thu hồi phần còn khả dụng đến mức 0. Phần đã sử dụng không bị đảo ngược tự động; phần còn thiếu do quản trị xem xét, không trừ thưởng tương lai. | Đã chốt | 2026-08-11 | M06, M07, M08, M11 |

**Trạng thái G-D04:** Đã chốt toàn bộ ngày 2026-08-11. Tên, loại và quy tắc sử dụng vật phẩm tăng trưởng thú cưng được tiếp tục quản lý trong quyết định chuyên môn của M06, không làm mở lại các nguyên tắc sổ, cấp thưởng và thu hồi đã chốt.

> Lưu ý rủi ro: quyết định loại bỏ AP hiện có có thể ảnh hưởng niềm tin và quyền lợi kỳ vọng của người dùng. Trước khi thực hiện cần xác định số người bị ảnh hưởng, thông báo rõ thời điểm hiệu lực, giữ ảnh chụp đối soát và có phương án phục hồi khi phát hiện sai phạm vi. Tài liệu này không thực hiện thay đổi dữ liệu.

### 9.5. Tiến độ xử lý quyết định G-D01

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Nhóm tuổi phục vụ | Phục vụ mọi độ tuổi. Sản phẩm phải có cơ chế đồng ý, quản lý và bảo vệ dành cho phụ huynh/người giám hộ ngay trong phạm vi nền tảng. | Đã chốt | 2026-08-11 | Tất cả module; đặc biệt M01, M09–M12 |
| Cơ chế đồng ý của người giám hộ | Hệ thống không xác minh người giám hộ; chỉ yêu cầu người dùng tự xác nhận đã có sự đồng ý. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M01, M11, M12; rủi ro tuân thủ cao và phải rà soát theo thị trường trước phát hành |
| Khi người dùng khai chưa có đồng ý | Vẫn cho sử dụng toàn bộ hệ thống và chỉ hiển thị cảnh báo. Không khóa tài khoản hoặc chức năng dựa trên câu trả lời này. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | Tất cả module; xung đột rủi ro cao với mục tiêu bảo vệ người nhỏ tuổi |
| Quyền của người giám hộ | Không tạo tài khoản, quan hệ liên kết hoặc quyền quản lý cho người giám hộ. Hệ thống chỉ lưu câu trả lời tự xác nhận của người dùng. | Không áp dụng theo lựa chọn sản phẩm | 2026-08-11 | M01; các module khác không có vai trò người giám hộ |
| Hạn chế đối với tài khoản nhỏ tuổi | Không có mặc định bảo vệ hoặc giới hạn riêng; tài khoản nhỏ tuổi dùng cùng chức năng và thiết lập mặc định như tài khoản người lớn. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | Tất cả module; rủi ro bảo vệ trẻ em và tuân thủ rất cao |
| Dữ liệu tuổi cần thu thập | Chỉ thu thập nhóm tuổi và quốc gia/khu vực, không thu thập ngày sinh đầy đủ. Dữ liệu dùng để hiển thị nội dung đồng ý và phục vụ chính sách, không khóa chức năng theo phạm vi hiện tại. | Đã chốt | 2026-08-11 | M01 sở hữu; M11 và M12 chỉ sử dụng tối thiểu theo mục đích |
| Chuyển đổi khi đủ tuổi tự quyết | Không áp dụng về mặt chức năng vì tài khoản nhỏ tuổi và người lớn hiện không có khác biệt quyền hoặc mặc định. | Không áp dụng theo lựa chọn sản phẩm | 2026-08-11 | M01, M11 |

**Trạng thái G-D01:** Đã chốt theo lựa chọn sản phẩm ngày 2026-08-11. Đây chưa phải xác nhận tuân thủ. Các ngưỡng, nghĩa vụ và khả năng phát hành cho người nhỏ tuổi phải được rà soát theo từng thị trường; quyết định sản phẩm không thay thế tư vấn pháp lý.

> Lưu ý rủi ro: cơ chế tự xác nhận không chứng minh danh tính hoặc thẩm quyền của người giám hộ và có thể không đáp ứng yêu cầu tại một số thị trường. Trước khi phát hành cho người nhỏ tuổi cần đánh giá pháp lý theo khu vực, ghi nhận phiên bản nội dung đồng ý và có khả năng thay thế cơ chế này nếu bắt buộc.

> Điểm chặn phát hành cần xác nhận: việc cho phép sử dụng đầy đủ khi người dùng khai chưa có đồng ý làm cho bước hỏi đồng ý chỉ mang tính cảnh báo, không phải cổng bảo vệ. Phạm vi này không được mô tả là đã tuân thủ cho trẻ em nếu chưa có kết luận rà soát pháp lý theo thị trường.

> Làm rõ phạm vi: mặc dù lựa chọn nhóm tuổi ban đầu nêu cơ chế phụ huynh/người giám hộ, các quyết định chi tiết đã loại bỏ việc xác minh và quan hệ quản lý của người giám hộ. Phạm vi thực tế hiện tại chỉ có bước tự khai báo và cảnh báo.

> Điểm chặn bổ sung: tài khoản nhỏ tuổi hiện không có mặc định riêng tư, giới hạn xã hội, hạn chế âm thanh/AI hoặc kiểm soát thông báo riêng. Quyết định này phải được đánh giá lại trước khi mô tả sản phẩm là phù hợp cho mọi độ tuổi tại một thị trường cụ thể.

### 9.6. Tiến độ xử lý quyết định G-D06

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Quyền sử dụng và nguồn học liệu | Người quản trị tự chịu trách nhiệm kiểm tra quyền sử dụng. Hệ thống không bắt buộc lưu thông tin nguồn, giấy phép hoặc bằng chứng ghi công. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M02, M11, M12; rủi ro bản quyền và truy vết cao |
| Xuất bản nội dung do AI hỗ trợ | Nội dung AI được phép xuất bản ngay, không cần người duyệt trước; sai sót được xử lý sau khi người dùng báo cáo hoặc hệ thống phát hiện. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M02, M11, M12; rủi ro học thuật, an toàn và quyền sử dụng rất cao |
| Người tạo và người duyệt | Mọi người dùng được tự xuất bản nội dung công khai ngay; không có bước người khác duyệt trước. Nội dung được kiểm tra và xử lý sau khi phát hiện hoặc có báo cáo. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M02, M11; rủi ro chất lượng, nội dung xấu và lạm dụng rất cao |
| Tiêu chuẩn chất lượng tối thiểu | Trước xuất bản phải tự động chặn nội dung thiếu trường học thuật bắt buộc, không có đáp án hợp lệ, trùng rõ ràng, chứa nội dung bị cấm hoặc có tài sản không sử dụng được. Nội dung vượt cổng vẫn chịu hậu kiểm. | Đã chốt | 2026-08-11 | M02–M05, M08, M11, M12 |
| Báo cáo nội dung từ người học | Mọi người học được báo cáo theo nhóm lý do, thêm mô tả tùy chọn và theo dõi trạng thái. Nội dung chỉ tự ẩn khi có tín hiệu nguy hiểm hoặc đạt ngưỡng chống lạm dụng đã xác định. | Đã chốt | 2026-08-11 | M02, M10, M11 |
| Thu hồi và sửa nội dung đã phát hành | Phân theo mức độ: nội dung nguy hiểm hoặc sai đáp án nghiêm trọng bị thu hồi ngay và dừng phiên đang chạy; lỗi thường ngừng phiên mới, cho phiên đang chạy hoàn tất theo bản đã giữ và thông báo sửa sai sau đó. | Đã chốt | 2026-08-11 | M02–M05, M08, M10, M11 |

**Trạng thái G-D06:** Đã chốt toàn bộ ngày 2026-08-11. Vai trò và thời hạn hậu kiểm cụ thể vẫn là quyết định vận hành của M02/M11, nhưng không được làm suy yếu cổng tự động, cơ chế báo cáo và khả năng thu hồi đã chốt.

> Lưu ý rủi ro: khi hệ thống không lưu nguồn và quyền sử dụng, việc chứng minh học liệu được phép sử dụng hoặc xác định phạm vi cần gỡ bỏ sẽ phụ thuộc vào hồ sơ bên ngoài của quản trị. Đây có thể là điểm chặn vận hành hoặc pháp lý khi có khiếu nại.

> Điều kiện bắt buộc do xuất bản AI tự động: phải có khả năng dừng xuất bản, thu hồi nhanh, tiếp nhận báo cáo, xác định nội dung và phiên học bị ảnh hưởng, đồng thời thông báo sửa sai. Nếu các năng lực này chưa sẵn sàng thì luồng xuất bản AI tự động chưa đủ điều kiện kích hoạt.

> Điều kiện bắt buộc do người dùng tự xuất bản: phải có giới hạn chống lạm dụng, cơ chế báo cáo, kiểm duyệt sau xuất bản, khả năng khóa người tạo và thu hồi nội dung theo phạm vi. Nếu chưa có các năng lực này thì nội dung do người dùng tạo chỉ đủ điều kiện ở chế độ riêng tư.

### 9.7. Tiến độ xử lý quyết định G-D07

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Tác động tới trí nhớ nghĩa | Tiến độ phát âm được theo dõi riêng. Phát âm đúng hoặc sai không thay đổi trạng thái thành thạo và lịch ôn nghĩa của M04; kết quả vẫn có thể dùng cho nhiệm vụ phát âm riêng. | Đã chốt | 2026-08-11 | M04, M05, M07, M11 |
| Ngưỡng xác định đạt | Hiệu chuẩn theo biến thể phát âm mục tiêu, trình độ người học và độ khó từ; ngưỡng có phiên bản để giải thích lịch sử. Không dùng trực tiếp nhãn đạt của nhà cung cấp nếu chưa hiệu chuẩn. | Đã chốt | 2026-08-11 | M05 sở hữu chính sách; M07 tiêu thụ kết quả, M11 giám sát, M12 cung cấp kết quả thô |

**Trạng thái G-D07:** Đã chốt toàn bộ ngày 2026-08-11. Biến thể phát âm mục tiêu và giá trị ngưỡng cụ thể vẫn là quyết định chuyên môn của M05, nhưng phải tuân theo nguyên tắc hiệu chuẩn và phiên bản hóa trên.

### 9.8. Tiến độ xử lý quyết định G-D08

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Sở hữu kết quả và điểm | M08 sở hữu kết quả trận và điểm cạnh tranh hiện tại. M09 tổng hợp theo mùa, nhóm và bảng xếp hạng; không tính lại kết quả trận. | Đã chốt | 2026-08-11 | M08, M09; M06/M10/M11 tiêu thụ kết quả |
| Cách tính điểm cạnh tranh | Dùng mô hình điểm tương đối dựa trên kết quả và sức mạnh điểm của đối thủ. Người mới có giai đoạn điểm tạm thời; biến động có giới hạn và chính sách được phiên bản hóa. XP, tiến độ học và tài sản không cộng trực tiếp vào điểm cạnh tranh. | Đã chốt | 2026-08-11 | M08 sở hữu; M09 và M11 tiêu thụ/giám sát |
| Khởi tạo và chuyển mùa | Người mới có giai đoạn điểm tạm thời. Khi sang mùa, điểm cũ được điều chỉnh mềm về gần mức khởi đầu, không đặt lại hoàn toàn; giữ lịch sử/thành tích mùa trước và hiệu chỉnh qua các trận đầu mùa mới. | Đã chốt | 2026-08-11 | M08, M09, M10, M11 |
| Điều kiện xuất hiện | Chỉ tài khoản hợp lệ đã hoàn thành số trận tính điểm tối thiểu mới xuất hiện trên bảng chính thức. Người đang có điểm tạm thời chỉ thấy tiến trình cá nhân; số trận cụ thể là cấu hình vận hành. | Đã chốt | 2026-08-11 | M01, M08, M09, M11 |
| Xử lý hòa điểm | Người có cùng điểm chính được đồng hạng. Chỉ số phụ chỉ dùng để giải thích hoặc sắp xếp hiển thị, không thay đổi quyền lợi nếu chưa được công bố trước mùa. | Đã chốt | 2026-08-11 | M08, M09, M11 |
| Sửa kết quả và quyền lợi | Giữ lịch sử gốc, tạo kết quả điều chỉnh và tính lại chuỗi biến động điểm bị ảnh hưởng theo phiên bản chính sách. M09 nhận biến động sửa; tài sản xử lý theo G-D04. | Đã chốt | 2026-08-11 | M06, M08–M11 |

**Trạng thái G-D08:** Đã chốt toàn bộ ngày 2026-08-11. Giá trị hệ số, số trận tạm thời, số trận tối thiểu và độ dài mùa vẫn là cấu hình chuyên môn phải công bố/phiên bản hóa, không làm thay đổi các nguyên tắc sở hữu và công bằng đã chốt.

### 9.9. Tiến độ xử lý quyết định G-D09

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Thông báo bắt buộc | Chỉ thông báo bảo mật tài khoản và thông tin vận hành bắt buộc cần người dùng biết là không thể tắt. Học tập, gamification, xã hội, thi đấu và truyền thông đều cho phép tắt. | Đã chốt | 2026-08-11 | M01, M04, M07–M10, M12 |
| Trạng thái mặc định của thông báo tùy chọn | Bật mặc định tất cả thông báo tùy chọn; người dùng có thể tắt sau. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M01, M10, M12; phải rà soát đồng ý nhận tin theo từng kênh, độ tuổi và thị trường |
| Mức chi tiết của lựa chọn | Cho điều chỉnh theo cả nhóm nội dung và kênh. Có lựa chọn đơn giản để tắt toàn bộ thông báo tùy chọn và phần chi tiết cho học tập, gamification, xã hội, thi đấu, truyền thông trên từng kênh. | Đã chốt | 2026-08-11 | M10, M11 |
| Kênh và chuyển kênh khi lỗi | Hộp thư trong hệ thống giữ bản phù hợp. Chỉ chuyển sang đẩy hoặc email nếu đúng nhóm nội dung vẫn được người dùng cho phép trên kênh đó và thông báo còn giá trị; không chuyển sang kênh đã tắt. | Đã chốt | 2026-08-11 | M10, M12 |
| Hiệu lực khi người dùng tắt | Có hiệu lực ngay với mọi thông báo chưa giao. Nội dung đang chuyển phát được dừng trong khả năng có thể; mọi lần thử lại phải kiểm tra lựa chọn mới trước khi gửi. | Đã chốt | 2026-08-11 | M10, M12 |
| Giới hạn tần suất | Có trần theo nhóm nội dung, theo kênh và trần tổng mỗi ngày; tín hiệu tương tự được gộp. Thông báo bắt buộc không tính vào trần tùy chọn nhưng vẫn chống gửi lặp; giá trị cụ thể là cấu hình vận hành. | Đã chốt | 2026-08-11 | M04, M07–M10, M11 |

**Trạng thái G-D09:** Đã chốt toàn bộ theo lựa chọn sản phẩm ngày 2026-08-11. Giờ yên lặng và ngoại lệ tuân theo G-D05. Quyết định bật mặc định vẫn phải nhường cho yêu cầu bắt buộc của kênh, nền tảng hoặc thị trường triển khai.

### 9.10. Tiến độ xử lý quyết định G-D10

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Mô hình quyền | Tách quyền theo trách nhiệm nội dung, hỗ trợ người dùng, vận hành hệ thống, kinh tế/tài sản và bảo mật. Mỗi người chỉ có quyền tối thiểu cần thiết; quyền toàn hệ thống chỉ dùng trong trường hợp đặc biệt có kiểm soát. | Đã chốt | 2026-08-11 | M01, M11 và mọi module có thao tác quản trị |
| Vai trò xung đột | Cho kiêm nhiệm vai trò không xung đột; không cấp đồng thời các vai trò có khả năng tự cấp quyền–tự kiểm toán hoặc che giấu thao tác của chính mình. Không có bước phê duyệt hai người bắt buộc. | Đã chốt, đã điều chỉnh | 2026-08-11 | M01, M06, M10, M11 |
| Danh mục phê duyệt hai người | Không thao tác nào bắt buộc hai người phê duyệt. Người có quyền phù hợp được tự thực hiện; hệ thống kiểm soát bằng phạm vi quyền và nhật ký. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M01, M02, M06–M12; rủi ro nội bộ rất cao |
| Tự tạo và tự duyệt | Không áp dụng bước duyệt bắt buộc. Người có quyền được tự thực hiện thay đổi của mình và phải để lại nhật ký đầy đủ. | Đã chốt theo lựa chọn sản phẩm | 2026-08-11 | M02, M06, M10–M12 |
| Quyền tạm thời và khẩn cấp | Không hỗ trợ quyền tạm thời hoặc nâng quyền khẩn cấp; mọi thao tác chỉ dùng quyền cố định đã cấp trước. | Đã chốt | 2026-08-11 | M01, M11, M12 |

**Trạng thái G-D10:** Đã chốt toàn bộ theo lựa chọn sản phẩm ngày 2026-08-11. Không có phê duyệt hai người và không có quyền tạm thời; kiểm soát dựa trên phân vai cố định, quyền tối thiểu và nhật ký.

> Lưu ý rủi ro: không có kiểm soát hai người làm tăng hậu quả của tài khoản bị chiếm quyền, sai sót nội bộ và hành vi lạm dụng đối với quyền, dữ liệu, tài sản, cấu hình hoặc gửi diện rộng. Nhật ký chỉ hỗ trợ phát hiện và điều tra sau sự cố, không ngăn thao tác xảy ra.

### 9.11. Tiến độ xử lý quyết định G-D11

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Kênh yêu cầu xóa tài khoản | Chỉ tiếp nhận và xử lý qua bộ phận hỗ trợ; không có chức năng tự phục vụ. | Đã chốt | 2026-08-11 | M01, M11; tăng tải hỗ trợ và yêu cầu theo dõi thời hạn xử lý |
| Xác minh và thời gian chờ | Hỗ trợ xác minh lại quyền sở hữu, thông báo phạm vi dữ liệu và áp dụng thời gian chờ cho phép hủy trước khi thực hiện. Yêu cầu cùng từng bước xử lý có trạng thái và lịch sử; độ dài cụ thể là cấu hình phải công bố. | Đã chốt | 2026-08-11 | M01, M11 |
| Xóa hay ẩn danh hóa theo loại dữ liệu | Xóa dữ liệu nhận dạng trực tiếp, liên hệ, thiết bị, lựa chọn nhận tin và âm thanh. Lịch sử học, trận, phần thưởng và số liệu cần cho tính toàn vẹn được ẩn danh hóa không thể liên kết lại; ngoại lệ phải có căn cứ và thời hạn riêng. | Đã chốt | 2026-08-11 | M01–M12 |
| Thời hạn lưu dữ liệu | Âm thanh gốc xóa ngay sau xử lý hoặc tối đa 24 giờ nếu cần thử lại; thông báo hộp thư 90 ngày; nhật ký vận hành 30 ngày; nhật ký bảo mật/kiểm toán 12 tháng; dữ liệu học và tài sản có định danh giữ trong vòng đời tài khoản rồi ẩn danh hóa khi xóa. | Đã chốt làm mặc định sản phẩm | 2026-08-11 | M01–M12; yêu cầu pháp lý hoặc lưu giữ điều tra hợp lệ có thể ghi đè theo phạm vi/thời hạn |
| Xử lý bản sao và dữ liệu bên ngoài | Gửi yêu cầu xóa tới nhà cung cấp ngoài và theo dõi kết quả. Bản sao lưu tự hết hạn tối đa sau 30 ngày, không dùng để phục hồi riêng tài khoản đã xóa; nếu phục hồi toàn hệ thống từ bản cũ phải áp dụng lại yêu cầu xóa. | Đã chốt | 2026-08-11 | M01, M11, M12 |

**Trạng thái G-D11:** Đã chốt toàn bộ ngày 2026-08-11. Các thời hạn là mặc định sản phẩm và phải nhường cho nghĩa vụ bắt buộc hoặc lưu giữ điều tra hợp lệ có phạm vi, lý do và thời hạn rõ ràng.

### 9.12. Tiến độ xử lý quyết định G-D12

| Thành phần | Quyết định đã chốt | Trạng thái | Ngày chốt | Phạm vi áp dụng |
|---|---|---|---|---|
| Dữ liệu gửi ra ngoài | Chỉ gửi dữ liệu tối thiểu cần cho tác vụ. AI nhận học liệu cần xử lý, không nhận danh tính/toàn bộ tiến độ; dịch vụ giọng nói chỉ nhận âm thanh và ngữ cảnh cần thiết; ngoại lệ phải có mục đích và phê duyệt. | Đã chốt | 2026-08-11 | M01, M02, M05, M10–M12 |
| Quản lý bí mật tích hợp | Quản lý tập trung, tách theo môi trường/nhà cung cấp, cấp quyền tối thiểu, có thay đổi và thu hồi; không xuất hiện trong tài liệu, nhật ký hoặc ứng dụng người dùng. Bí mật nghi lộ phải được thu hồi/thay thế và điều tra. | Đã chốt | 2026-08-11 | M11, M12 |
| Hạn mức và chi phí | Có hạn mức theo người dùng, module, nhà cung cấp và chu kỳ ngày/tháng; cảnh báo trước ngưỡng, dừng hoặc suy giảm ở ngưỡng cứng. Danh tính, bảo mật và dữ liệu cốt lõi được ưu tiên; AI, ảnh và giọng nói suy giảm trước. | Đã chốt | 2026-08-11 | M02, M05, M10–M12 |
| Chính sách suy giảm | Phân theo mức quan trọng: danh tính/bảo mật từ chối thao tác không thể xác minh; dữ liệu cốt lõi chuyển trạng thái an toàn hoặc chờ xử lý; AI/ảnh/giọng nói dùng thay thế hoặc tạm ẩn; thông báo tùy chọn chờ/hết hạn và không hoàn tác nghiệp vụ nguồn. | Đã chốt | 2026-08-11 | M01, M02, M05, M08, M10–M12 |
| Mục tiêu mức dịch vụ | Phân tầng theo mức quan trọng: danh tính, bảo mật, tiến độ và tài sản chặt nhất; học tập tương tác/thi đấu ở mức tiếp theo; AI, ảnh, giọng nói và thông báo tùy chọn thấp hơn. Giá trị số chốt sau khi có số liệu nền và năng lực vận hành. | Đã chốt nguyên tắc | 2026-08-11 | M11, M12 và module tiêu thụ |
| Nhà cung cấp dự phòng | Danh tính, gửi thông tin bảo mật, lưu trữ dữ liệu và tương tác thời gian thực cần phương án thay thế hoặc phục hồi. AI, hình ảnh và giọng nói được phép suy giảm trước; nhiều nhà cung cấp chỉ bổ sung khi có nhu cầu chất lượng/chi phí. | Đã chốt | 2026-08-11 | M01, M05, M08, M10–M12 |

**Trạng thái G-D12:** Đã chốt toàn bộ ngày 2026-08-11. Giá trị số của mục tiêu mức dịch vụ và hạn mức chi phí là cấu hình vận hành cần được xác lập từ số liệu thực tế, không làm thay đổi thứ tự ưu tiên và chính sách suy giảm đã chốt.

> Lưu ý rủi ro: bật mặc định email, thông báo đẩy, xã hội hoặc truyền thông có thể không đáp ứng yêu cầu đồng ý của từng kênh/thị trường, đặc biệt khi sản phẩm phục vụ mọi độ tuổi. Cần rà soát pháp lý và quy định nền tảng trước phát hành; nếu không được phép, cấu hình thị trường phải ghi đè quyết định mặc định này.

## 10. Quy tắc đưa backlog vào công cụ quản lý công việc

| Trường quản lý | Cách sử dụng |
|---|---|
| Task ID | Giữ nguyên mã trong backlog module; không tạo lại mã mới khi nhập. |
| Nhóm công việc | Dùng M01–M12 làm nhóm sở hữu; dùng Giai đoạn A–E làm mốc phạm vi. |
| Phụ thuộc | Chỉ ghi task hoặc đầu ra phải hoàn tất trước; không dùng module thông báo, quản trị hay tích hợp như điều kiện để chốt trạng thái nghiệp vụ nguồn nếu không thật sự bắt buộc. |
| Quyết định | Liên kết mã quyết định module và mã G-D tương ứng; nếu cần thay đổi phải tạo dấu vết đánh giá lại thay vì sửa ngầm nội dung đã duyệt. |
| Tiêu chí hoàn thành | Giữ Definition of Done trong tài liệu phân tích; bổ sung bằng chứng kiểm chứng khi lập kế hoạch thực hiện. |
| Trạng thái | Phân biệt Chờ quyết định, Sẵn sàng, Đang thực hiện, Chờ phụ thuộc, Hoàn thành và Loại có lý do. |
| Truy vết | Task liên module phải liên kết cả task nguồn, task tiêu thụ và đầu ra do module sở hữu. |

## 11. Cổng kiểm tra trước khi khởi động một giai đoạn

Một giai đoạn chỉ nên được đưa vào thực hiện khi đáp ứng đủ các điều kiện sau:

- Các quyết định đã chốt có ảnh hưởng trực tiếp được truy vết tới task và tiêu chí nghiệm thu; thay đổi quyết định phải được đánh giá trước khi task phụ thuộc đi qua cổng phát hành.
- Module sở hữu dữ liệu và đầu ra đã rõ, không có hai nguồn sự thật.
- Phụ thuộc bắt buộc có đầu ra và tiêu chí chấp nhận cụ thể.
- Các trường hợp cấp lặp, xử lý lại, điều chỉnh và thất bại một phần đã có chủ sở hữu.
- Quyền riêng tư, lưu giữ, kiểm toán và dữ liệu gửi ra ngoài đã được đánh giá.
- Có phương án suy giảm khi thông báo, AI, giọng nói, tài sản số hoặc dịch vụ ngoài không sẵn sàng.
- Chỉ số xác nhận giá trị học thuật và động lực không bị thay thế hoàn toàn bằng chỉ số tương tác ngắn hạn.

## 12. Hành động tiếp theo đề xuất

1. G-D01–G-D12 đã được xử lý ngày 2026-08-11 và đồng bộ sang các sổ quyết định liên quan. Trước phát hành phải rà soát riêng các lựa chọn được đánh dấu rủi ro cao về người nhỏ tuổi, quyền học liệu, xuất bản tự động, thông báo mặc định và kiểm soát quản trị.
2. Duy trì truy vết 305 quyết định đã chốt và đánh giá lại khi phạm vi nghiệp vụ thay đổi.
3. Xử lý các cổng phát hành: rà soát pháp lý theo tuổi/thị trường, kiểm soát xuất bản tức thời, bằng chứng đồng ý cho giọng nói người chưa thành niên, loại bỏ AP, quyền quản trị không duyệt hai người và thông báo bật mặc định.
4. Phạm vi Giai đoạn A–B đã được chọn; thực hiện đánh giá hiện trạng theo [kế hoạch A–B](../03-ke-hoach-giai-doan-a/KE-HOACH-TRIEN-KHAI-GIAI-DOAN-A-B.md) và chưa gán thời hạn khi chưa có dữ liệu năng lực.
5. Nhập task của phạm vi đã chọn vào công cụ quản lý công việc, giữ nguyên Task ID, module sở hữu, điều kiện chặn và quan hệ phụ thuộc.
6. Chỉ mở Giai đoạn C–E khi chỉ số học thuật, độ ổn định vận hành, riêng tư và cân bằng thưởng của giai đoạn trước đạt ngưỡng được thống nhất.
