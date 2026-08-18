# Bộ mẫu bằng chứng Cổng A

## 1. Mục đích

Bộ mẫu này chuẩn hóa cách thu thập, rà soát và phê duyệt bằng chứng cho A-G01–A-G06. Mẫu không tự tạo bằng chứng và không thay thế Definition of Done của task nguồn.

## 2. Danh mục tài liệu

| Tài liệu | Mục đích |
|---|---|
| [Sổ đăng ký bằng chứng](SO-DANG-KY-BANG-CHUNG.md) | Quản lý một danh sách chung cho mọi bằng chứng, task, REL và kết quả rà soát |
| [A-G01 — Tài khoản an toàn](A-G01-TAI-KHOAN-AN-TOAN.md) | Ma trận trạng thái/quyền tài khoản, phiên và hành trình danh tính |
| [A-G02 — Quản trị có kiểm soát](A-G02-QUAN-TRI-CO-KIEM-SOAT.md) | Ma trận quyền tối thiểu, xác minh lại, audit trước–sau và kiểm thử từ chối |
| [A-G03 — Học liệu mẫu](A-G03-HOC-LIEU-MAU.md) | Vòng đời, phiên bản, checklist chất lượng, xuất bản, báo cáo và thu hồi |
| [A-G04 — Tích hợp suy giảm an toàn](A-G04-TICH-HOP-SUY-GIAM-AN-TOAN.md) | Registry năng lực, trạng thái lỗi, suy giảm và báo cáo kiểm thử resilience |
| [A-G05 — Bí mật và dữ liệu ngoài](A-G05-BI-MAT-VA-DU-LIEU-NGOAI.md) | Inventory bí mật không chứa giá trị, bản đồ dữ liệu ngoài, redaction và quyền tài sản |
| [A-G06 — Sẵn sàng ứng phó](A-G06-SAN-SANG-UNG-PHO.md) | Health/SLO, cảnh báo, mức sự cố, playbook, diễn tập và hậu kiểm |
| [Biên bản quyết định Cổng A](BIEN-BAN-QUYET-DINH-CONG-A.md) | Ghi quyết định đạt, không đạt hoặc đạt có điều kiện và các ngoại lệ còn lại |

## 3. Quy trình sử dụng

1. Gán người chủ trì và người xác nhận cho cổng.
2. Sao chép mẫu cổng thành hồ sơ của lần nghiệm thu; không ghi đè mẫu gốc.
3. Đăng ký từng bằng chứng trong sổ chung trước khi dùng để kết luận.
4. Điền đầy đủ task nguồn, REL, phạm vi, môi trường, thời điểm và mức nhạy cảm.
5. Thực hiện cả kịch bản thành công, từ chối, lỗi, gửi lặp, đồng thời và phục hồi phù hợp.
6. Người xác nhận độc lập rà soát bằng chứng và ghi đạt/không đạt.
7. Dùng biên bản quyết định để kết luận từng cổng và toàn Giai đoạn A.

## 4. Nguyên tắc bằng chứng

- Không đưa mật khẩu, token, khóa, connection string, dữ liệu cá nhân thật hoặc payload nhạy cảm vào tài liệu.
- Bằng chứng phải truy ngược được tới task, yêu cầu, lần thực hiện và người xác nhận.
- Ảnh chụp màn hình đơn lẻ không đủ chứng minh hành vi từ chối, đồng thời, thu hồi hoặc phục hồi.
- Bằng chứng từ môi trường thử nghiệm phải ghi rõ dữ liệu giả và khác biệt so với môi trường phát hành.
- Một bằng chứng có thể phục vụ nhiều cổng/REL, nhưng phải có một định danh duy nhất và không sao chép thành nhiều bản mâu thuẫn.
- Bằng chứng hết hạn, không tái hiện được hoặc chứa dữ liệu không được phép phải bị loại và thay thế.

## 5. Trạng thái chuẩn

| Trạng thái | Ý nghĩa |
|---|---|
| Chưa chuẩn bị | Chưa có người thực hiện hoặc chưa có artifact |
| Đang chuẩn bị | Đang thu thập nhưng chưa sẵn sàng rà soát |
| Chờ xác nhận | Đã nộp đủ theo người thực hiện, chờ người xác nhận |
| Đạt | Người xác nhận chấp nhận và không còn sai lệch chặn cổng |
| Không đạt | Bằng chứng cho thấy tiêu chí chưa đáp ứng |
| Hết hiệu lực | Bằng chứng không còn đại diện cho phiên bản/phạm vi hiện tại |
| Bị loại | Sai phạm vi, không truy vết được hoặc chứa dữ liệu không được phép |
