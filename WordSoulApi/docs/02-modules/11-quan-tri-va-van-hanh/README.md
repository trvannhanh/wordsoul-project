# M11 — Quản trị, cấu hình và quan sát hệ thống

> Đánh giá hiện trạng Giai đoạn A: [A-WP03 — M11 Quản trị và vận hành nền](DANH-GIA-HIEN-TRANG-A-WP03.md). Kết quả tĩnh ngày 2026-08-14: 0/42 task đáp ứng đầy đủ, 28 đáp ứng một phần, 13 chưa có và 1 không còn phù hợp với quyết định đã chốt.

## Tài liệu phân tích

- [Phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Từ điển quản trị M11](TU-DIEN-QUAN-TRI.md)
- [Danh mục hành động quản trị M11](DANH-MUC-HANH-DONG-QUAN-TRI.md)
- [Danh mục vai trò quản trị M11](DANH-MUC-VAI-TRO-QUAN-TRI.md)
- [Ma trận quyền tối thiểu M11](MA-TRAN-QUYEN-TOI-THIEU.md)
- [Sổ đăng ký cấu hình M11](SO-DANG-KY-CAU-HINH.md)
- [Từ điển chỉ số quản trị M11](TU-DIEN-CHI-SO-QUAN-TRI.md)
- [Ma trận nội dung quản trị chéo module M11](MA-TRAN-NOI-DUNG-QUAN-TRI-CHEO-MODULE.md)
- [Task backlog](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Quản trị, cấu hình và quan sát hệ thống |
| Mục đích | Cung cấp khả năng điều hành an toàn, theo dõi hiệu quả học tập và gamification, phát hiện bất thường và thay đổi chính sách mà không làm mất tính kiểm soát. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** bảng điều hành; quản lý người dùng và nhóm ở góc độ quản trị; cấu hình hệ thống; quản lý nội dung nhiệm vụ, thành tựu và phòng thử thách; số liệu phiên học, tiến độ, bảng xếp hạng và trận đấu; nhật ký hoạt động/hệ thống; kiểm tra sức khỏe; thông báo diện rộng; thao tác bảo trì được kiểm soát. **Không chịu trách nhiệm:** thay thế quy tắc nghiệp vụ nội tại của module học, ôn, tài sản hay thi đấu; tự tạo dữ liệu nghiệp vụ không có nguồn hợp lệ. |
| Đầu vào (Input) | Số liệu, lịch sử và tín hiệu lỗi từ tất cả module; yêu cầu quản trị có quyền; chính sách học, ôn, phần thưởng, nội dung và duy trì dữ liệu; trạng thái các năng lực tích hợp. |
| Đầu ra (Output) | Cấu hình đã kiểm tra và công bố cho module liên quan; nội dung/quy tắc được kích hoạt; báo cáo tổng quan; lịch sử học và trận phục vụ hỗ trợ; cảnh báo và nhật ký kiểm toán; kết quả thao tác quản trị; thông báo điều hành. |
| Phụ thuộc (Dependencies) | Tất cả module nghiệp vụ cung cấp dữ liệu và thực thi chính sách; danh tính và hồ sơ cung cấp phân quyền; tích hợp nền tảng hỗ trợ gửi tin và quan sát dịch vụ ngoài. |
| Người dùng/vai trò liên quan | Quản trị viên nội dung, quản trị viên vận hành, người hỗ trợ người dùng, chủ sản phẩm; người học chịu tác động gián tiếp. |
| Độ ưu tiên | Cao |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Quyền quản trị quá rộng; thay đổi cấu hình không kiểm tra; thiếu lịch sử ai thay đổi gì; thao tác bảo trì gây mất dữ liệu hoặc gián đoạn; nhật ký chứa dữ liệu nhạy cảm; số liệu tổng hợp sai; điều chỉnh số dư không có lý do; thông báo diện rộng không có bước kiểm tra. |

## Năng lực nghiệp vụ chính

- Quan sát chỉ số người dùng, học tập, ôn tập, phần thưởng và thi đấu.
- Quản lý cấu hình có phân loại công khai/nội bộ và kiểm tra giá trị hợp lệ.
- Kiểm soát vòng đời nội dung quản trị như nhiệm vụ, thành tựu và phòng thử thách.
- Tra cứu lịch sử học, ôn và trận để hỗ trợ hoặc điều tra sai lệch.
- Quản lý trạng thái tài khoản, vai trò, nhóm và điều chỉnh giá trị có lý do.
- Theo dõi sức khỏe, nhật ký, lưu lượng bất thường và các tác vụ nền.

## Điểm cần làm rõ

- Cần những vai trò quản trị tách biệt nào và quyền phê duyệt hai bước áp dụng cho thao tác nào?
- Thay đổi cấu hình có cần lên lịch, thử nghiệm giới hạn hoặc khả năng quay lại phiên bản trước không?
- Chỉ số nào phải gần thời gian thực và chỉ số nào có thể tổng hợp theo ngày?
- Nhật ký kiểm toán và nhật ký vận hành cần lưu bao lâu, ai được truy cập?
