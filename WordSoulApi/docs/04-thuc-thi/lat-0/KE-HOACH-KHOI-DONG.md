# Kế hoạch khởi động Lát 0 — Giai đoạn A

## 1. Mục đích

Tài liệu này chuyển 12 công việc điều phối A0 thành kế hoạch giao việc có thể bắt đầu ngay. Lát 0 không triển khai chức năng sản phẩm; mục tiêu là mở các hồ sơ rủi ro phát hành, xác lập kiểm soát tạm thời và tạo điều kiện an toàn để thực hiện Lát 1–Lát 5.

Tên cá nhân và thời hạn chưa được điền vì chưa có thông tin đội ngũ. Các vai trò dưới đây là phương án khuyến nghị và phải được thay bằng người thực tế trước khi công việc chuyển sang thực hiện.

## 2. Kết quả bắt buộc của Lát 0

Lát 0 hoàn thành khi đồng thời đáp ứng các điều kiện sau:

- Năm hồ sơ REL-01, REL-02, REL-03, REL-04 và REL-07 đã được mở, có chủ trì, người xác nhận, phạm vi, câu hỏi cần giải quyết và tiêu chí đóng.
- Bảy kiểm soát tạm thời CT-01–CT-07 đã được ghi nhận, có phạm vi áp dụng, cách kiểm tra và điều kiện hết hiệu lực.
- Mỗi công việc A0 có một cá nhân chủ trì và ít nhất một cá nhân hoặc vai trò xác nhận độc lập.
- Không hồ sơ nào chứa bí mật, dữ liệu cá nhân thật hoặc tài sản chưa rõ quyền sử dụng.
- Các task Lát 1 biết rõ hồ sơ hoặc kiểm soát nào đang chặn chúng.
- Mọi thay đổi quyết định đều được ghi vào biên bản thay vì chỉ trao đổi miệng.

Hoàn thành Lát 0 chỉ có nghĩa nền quản trị đã sẵn sàng. Điều này không đồng nghĩa các REL đã được đóng hoặc hệ thống đã đủ điều kiện phát hành.

## 3. Danh sách vai trò cần gán

| Vai trò | Trách nhiệm trong Lát 0 | Cá nhân thực tế | Trạng thái gán |
|---|---|---|---|
| Người điều phối Giai đoạn A | Duy trì bảng công việc, phụ thuộc, biên bản và báo cáo sai lệch | Chưa gán | Bắt buộc trước buổi khởi động |
| Chủ sản phẩm | Xác nhận phạm vi, năng lực tắt, chấp nhận hoặc chuyển cấp rủi ro sản phẩm | Chưa gán | Chưa gán |
| Chủ M01 | Chủ dữ liệu danh tính, đồng ý, xuất và xóa dữ liệu | Chưa gán | Chưa gán |
| Chủ M02 | Chủ nội dung từ vựng và trạng thái công khai | Chưa gán | Chưa gán |
| Chủ M06 | Chủ lịch sử AP và nguyên tắc không mở rộng AP | Chưa gán | Chưa gán |
| Chủ M11 | Chủ quyền quản trị, audit, log và vận hành | Chưa gán | Chưa gán |
| Chủ M12 | Chủ tích hợp, bí mật, giới hạn lưu lượng và tài sản số | Chưa gán | Chưa gán |
| Đại diện pháp lý | Kết luận vấn đề tuổi, đồng ý, quyền tài sản và phạm vi thị trường | Chưa gán | Chưa gán; là phụ thuộc bên ngoài quan trọng |
| Đại diện riêng tư | Xác nhận xử lý dữ liệu cá nhân, log, xuất/xóa và dữ liệu gửi ra ngoài | Chưa gán | Chưa gán |
| Đại diện an toàn hệ thống | Xác nhận kiểm soát quyền, bí mật, xác thực và giới hạn lưu lượng | Chưa gán | Chưa gán |
| Đại diện vận hành | Xác nhận health, suy giảm, sự cố và khả năng kiểm tra định kỳ | Chưa gán | Chưa gán |
| Đại diện học thuật/nội dung | Xác nhận nội dung được phép công khai và tiêu chuẩn kiểm duyệt | Chưa gán | Chưa gán |

Một người có thể giữ nhiều vai trò nếu quy mô đội nhỏ, nhưng người xác nhận không nên tự xác nhận đầu ra do chính mình tạo đối với REL-02, REL-03 và các kiểm soát an toàn liên quan.

## 4. Thứ tự mở công việc

### Đợt 0A — Mở ngay, không có phụ thuộc task

| Thứ tự | Task | Kết quả cần tạo đầu tiên | Chủ trì đề xuất | Người xác nhận | Điều kiện chuyển sang đang thực hiện |
|---|---|---|---|---|---|
| 1 | A0-T001 | Hồ sơ REL-01 có phạm vi thị trường, nhóm tuổi, câu hỏi pháp lý và hạn phản hồi | Chủ sản phẩm | Pháp lý và chủ M01 | Đã gán người và xác định người có thẩm quyền kết luận |
| 2 | A0-T002 | Hồ sơ REL-02 có danh mục quyền nhạy cảm và bằng chứng bù trừ cần thu thập | Chủ M11/an toàn hệ thống | Chủ sản phẩm và vận hành | Đã gán người xác nhận độc lập |
| 3 | A0-T003 | Hồ sơ REL-03 có danh mục tích hợp/bí mật cần kiểm kê mà không ghi giá trị bí mật | Chủ M12 | An toàn hệ thống và vận hành | Có nơi lưu hồ sơ hạn chế quyền xem |
| 4 | A0-T004 | Hồ sơ REL-04 có phạm vi tài sản, tình trạng quyền và đường tạm ẩn/gỡ | Chủ sản phẩm/nội dung | Pháp lý | Có đầu mối nhận khiếu nại và người quyết định tạm ẩn |
| 5 | A0-T007 | Biên bản CT-02 cấm tự ghép tài khoản chỉ dựa trên thư điện tử trùng | Chủ M01 | An toàn hệ thống | Phạm vi luồng danh tính đã được liệt kê |
| 6 | A0-T009 | Biên bản CT-04 xác định health hiện tại không đủ làm bằng chứng phát hành | Chủ vận hành | Chủ M11/M12 | Có danh sách năng lực cần kiểm tra thủ công |
| 7 | A0-T012 | Biên bản CT-07 đóng băng mọi phụ thuộc mới vào AP | Chủ M06 | Chủ sản phẩm và M11 | Có danh sách nơi AP còn xuất hiện để đối soát |

### Đợt 0B — Mở sau khi hồ sơ nền tương ứng được tạo

| Thứ tự | Task | Phụ thuộc mở việc | Kết quả cần tạo | Chủ trì đề xuất | Người xác nhận |
|---|---|---|---|---|---|
| 8 | A0-T005 | A0-T001 | Hồ sơ REL-07 có phạm vi dữ liệu, chủ dữ liệu, thời hạn xử lý và hành trình đối soát liên module | Chủ M01 | Chủ M11, riêng tư và các chủ module dữ liệu |
| 9 | A0-T006 | A0-T004 | Biên bản CT-01 khóa công khai nội dung chưa duyệt và xác định điều kiện mở lại | Chủ M02 | Chủ sản phẩm và M11 |
| 10 | A0-T008 | A0-T003 | Biên bản CT-03 phân loại log/payload thô có rủi ro, giới hạn sử dụng và kế hoạch thay thế | Chủ M11 | An toàn hệ thống và riêng tư |
| 11 | A0-T010 | A0-T003 | Biên bản CT-05 cấm miễn giới hạn lưu lượng chỉ dựa trên dấu hiệu yêu cầu | Chủ M12 | An toàn hệ thống |

### Đợt 0C — Chốt phạm vi năng lực giữ tắt

| Thứ tự | Task | Phụ thuộc mở việc | Kết quả cần tạo | Chủ trì đề xuất | Người xác nhận |
|---|---|---|---|---|---|
| 12 | A0-T011 | A0-T001 và A0-T003 | Biên bản CT-06 liệt kê AI/giọng nói được giữ tắt, dữ liệu không được gửi ra ngoài và điều kiện xem xét lại | Chủ sản phẩm/M12 | Pháp lý, riêng tư và vận hành |

Đợt 0B có thể bắt đầu ngay khi hồ sơ phụ thuộc đã được **mở và xác nhận phạm vi sơ bộ**; không cần chờ REL được đóng. Đợt 0C chỉ bắt đầu sau khi cả phạm vi tuổi/đồng ý và phạm vi tích hợp đã được ghi nhận.

## 5. Bảng giao việc Lát 0

| Task | Cá nhân chủ trì | Cá nhân xác nhận | Ngày bắt đầu | Hạn rà soát | Trạng thái | Liên kết hồ sơ/bằng chứng |
|---|---|---|---|---|---|---|
| A0-T001 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng | [REL-01 — bản nháp đã chuẩn bị](./REL-01-TUOI-THI-TRUONG-VA-DONG-Y.md) |
| A0-T002 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng | [REL-02 — bản nháp đã chuẩn bị](./REL-02-QUYEN-VA-AUDIT-BU-TRU.md) |
| A0-T003 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng gán | Chưa tạo |
| A0-T004 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng gán | Chưa tạo |
| A0-T005 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Chờ A0-T001 | Chưa tạo |
| A0-T006 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Chờ A0-T004 | Chưa tạo |
| A0-T007 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng | [CT-02 — bản nháp đã chuẩn bị](./CT-02-KHONG-TU-GHEP-TAI-KHOAN.md) |
| A0-T008 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Chờ A0-T003 | Chưa tạo |
| A0-T009 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng gán | Chưa tạo |
| A0-T010 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Chờ A0-T003 | Chưa tạo |
| A0-T011 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Chờ A0-T001 và A0-T003 | Chưa tạo |
| A0-T012 | Chưa gán | Chưa gán | Chưa xác định | Chưa xác định | Sẵn sàng gán | Chưa tạo |

Không tự điền ngày dự kiến khi chưa biết năng lực đội ngũ và thời gian phản hồi của pháp lý. Sau khi gán đủ người, người điều phối mới lập lịch theo sức chứa thực tế.

## 6. Cấu trúc tối thiểu của mỗi hồ sơ REL

Mỗi hồ sơ REL phải có:

1. Mã và tên điều kiện phát hành.
2. Phạm vi áp dụng, không áp dụng và năng lực đang bị chặn.
3. Chủ trì, người xác nhận và người có thẩm quyền kết luận.
4. Quyết định sản phẩm liên quan và các giả định đang sử dụng.
5. Danh sách câu hỏi chưa có bằng chứng hoặc cần ý kiến chuyên môn.
6. Danh sách bằng chứng phải thu thập, nguồn, người tạo và người kiểm tra.
7. Tiêu chí đạt, không đạt và đạt có điều kiện.
8. Sai lệch còn lại, biện pháp tạm thời, ngày hết hiệu lực và điều kiện chuyển cấp.
9. Lịch sử thay đổi và kết luận cuối.

Hồ sơ được xem là đã mở khi các mục 1–6 có nội dung và đã có người chịu trách nhiệm. Việc mở hồ sơ không phải là chấp nhận rủi ro.

## 7. Cấu trúc tối thiểu của mỗi kiểm soát tạm thời

Mỗi biên bản CT phải có:

1. Rủi ro mà kiểm soát đang hạn chế.
2. Phạm vi hành trình, dữ liệu, nội dung hoặc tích hợp chịu ảnh hưởng.
3. Hành vi bị cấm và hành vi vẫn được phép.
4. Người chịu trách nhiệm duy trì và người kiểm tra độc lập.
5. Cách kiểm tra định kỳ và bằng chứng kết quả.
6. Cách xử lý khi phát hiện vi phạm.
7. Task hoặc REL thay thế kiểm soát tạm thời.
8. Điều kiện và thẩm quyền cho phép gỡ bỏ.

Một tuyên bố chính sách không kèm cách kiểm tra không đủ để đóng task CT.

## 8. Chương trình buổi khởi động

| Nội dung | Kết quả phải đạt |
|---|---|
| Xác nhận người điều phối và chủ từng vai trò | Bảng vai trò có tên cá nhân, không còn vai trò bắt buộc chưa có chủ |
| Xác nhận phạm vi Giai đoạn A | Danh sách năng lực trong phạm vi và năng lực giữ tắt không mâu thuẫn với gói task |
| Mở bốn hồ sơ độc lập ban đầu | REL-01, REL-02, REL-03 và REL-04 có người chịu trách nhiệm và tiêu chí mở hồ sơ |
| Kích hoạt ba kiểm soát độc lập | CT-02, CT-04 và CT-07 có phạm vi, người duy trì và người kiểm tra |
| Chốt nơi lưu bằng chứng | Mọi người dùng cùng một sổ đăng ký, quy ước định danh và mức quyền xem |
| Chốt nhịp rà soát | Có lịch rà soát phụ thuộc, sai lệch và bằng chứng; không tự đặt ngày phát hành |

## 9. Tiêu chí cho phép chuyển sang Lát 1

Lát 1 được phép bắt đầu khi:

- A0-T001–A0-T004 đã có hồ sơ được mở đúng cấu trúc.
- A0-T007, A0-T009 và A0-T012 đã có hiệu lực và có người kiểm tra.
- A0-T005–A0-T011 đã có chủ, người xác nhận và trạng thái phụ thuộc rõ ràng.
- Không có bất đồng chưa ghi nhận về phạm vi nội dung chưa duyệt, tự ghép tài khoản, sử dụng payload thô, health, giới hạn lưu lượng, AI/giọng nói hoặc AP.
- Sổ đăng ký bằng chứng đã có quy ước sử dụng và người duy trì.

Lát 1 có thể bắt đầu khi các REL còn đang xử lý, nhưng task phụ thuộc vào kết luận pháp lý, quyền tài sản hoặc tích hợp vẫn phải giữ trạng thái chờ phù hợp.

## 10. Đầu vào liên quan

- [Gói task Lát 0–Lát 1](../../03-ke-hoach-giai-doan-a/GOI-TASK-THUC-THI-LAT-0-1-GIAI-DOAN-A.md)
- [Bảng import tổng Giai đoạn A](../../03-ke-hoach-giai-doan-a/BANG-IMPORT-TONG-GIAI-DOAN-A.md)
- [Tổng hợp sai lệch và thứ tự triển khai](../../03-ke-hoach-giai-doan-a/TONG-HOP-SAI-LECH-VA-THU-TU-TRIEN-KHAI-GIAI-DOAN-A.md)
- [Bộ mẫu bằng chứng Cổng A](../../05-bang-chung/cong-a/README.md)
- [Kiểm tra bao phủ và checklist quyết định](../../03-ke-hoach-giai-doan-a/KIEM-TRA-BAO-PHU-VA-QUYET-DINH-CONG-A.md)
