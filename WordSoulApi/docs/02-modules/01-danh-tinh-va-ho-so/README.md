# M01 — Danh tính và hồ sơ người dùng

> Đánh giá hiện trạng Giai đoạn A: [A-WP01 — M01 Danh tính và hồ sơ](DANH-GIA-HIEN-TRANG-A-WP01.md). Kết quả tĩnh ngày 2026-08-14: 0/43 task đáp ứng đầy đủ, 24 đáp ứng một phần và 19 chưa có bằng chứng triển khai.

## Tài liệu phân tích và lập kế hoạch

- [Prompt phân tích chuyên sâu](../../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md)
- [Kết quả phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Từ điển danh tính M01](TU-DIEN-DANH-TINH.md)
- [Vòng đời tài khoản M01](VONG-DOI-TAI-KHOAN.md)
- [Bản đồ dữ liệu hồ sơ M01](BAN-DO-DU-LIEU-HO-SO.md)
- [Chính sách thông tin bảo mật M01](CHINH-SACH-THONG-TIN-BAO-MAT.md)
- [Đặc tả dữ liệu đăng ký M01](DAC-TA-DANG-KY.md)
- [Luồng xác minh thư điện tử M01](LUONG-XAC-MINH-THU.md)
- [Quy tắc ghi nhận đồng ý M01](QUY-TAC-GHI-NHAN-DONG-Y.md)
- [Điều phối khởi tạo người dùng mới M01](DIEU-PHOI-KHOI-TAO-NGUOI-DUNG.md)
- [Tiêu chí nghiệm thu đăng ký M01](TIEU-CHI-NGHIEM-THU-DANG-KY.md)
- [Đặc tả đăng nhập trực tiếp M01](DAC-TA-DANG-NHAP-TRUC-TIEP.md)
- [Kiểm soát thử đăng nhập bất thường M01](KIEM-SOAT-THU-DANG-NHAP-BAT-THUONG.md)
- [Xử lý tài khoản không hoạt động M01](XU-LY-TAI-KHOAN-KHONG-HOAT-DONG.md)
- [Backlog task](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Danh tính và hồ sơ người dùng |
| Mục đích | Bảo đảm mỗi người dùng có danh tính tin cậy, truy cập đúng quyền và có hồ sơ nền tảng để các hoạt động học tập, gamification và cộng đồng được ghi nhận nhất quán. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** đăng ký, đăng nhập, duy trì phiên truy cập, đăng nhập qua nhà cung cấp danh tính bên ngoài, hồ sơ cá nhân, trạng thái tài khoản, vai trò, thông tin thiết bị nhận thông báo và số dư tổng quan gắn với người dùng. **Không chịu trách nhiệm:** nội dung học, tính toán tiến độ từng từ, quy tắc trao thưởng, nội dung thông báo hoặc xếp hạng. |
| Đầu vào (Input) | Thông tin đăng ký và đăng nhập từ người dùng; bằng chứng xác thực từ nhà cung cấp bên ngoài; yêu cầu cập nhật hồ sơ; quyết định vai trò, khóa/mở tài khoản hoặc điều chỉnh hợp lệ từ quản trị; thông tin thiết bị từ ứng dụng người dùng. |
| Đầu ra (Output) | Danh tính và trạng thái truy cập cho tất cả module cần xác định người dùng; hồ sơ hiển thị cho người dùng và cộng đồng; trạng thái vai trò cho quản trị; thông tin liên hệ và thiết bị hợp lệ cho module thông báo. |
| Phụ thuộc (Dependencies) | Tích hợp nền tảng và tài sản số cho đăng nhập bên ngoài; quản trị, cấu hình và quan sát hệ thống cho chính sách tài khoản và kiểm soát vận hành. |
| Người dùng/vai trò liên quan | Khách chưa đăng nhập, người học, quản trị viên; gián tiếp gồm mọi module cần nhận biết người dùng. |
| Độ ưu tiên | Cao |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Chiếm quyền tài khoản; lộ dữ liệu cá nhân; phân quyền sai; phiên truy cập không được vô hiệu hóa đúng lúc; trùng danh tính khi dùng nhiều hình thức đăng nhập; điều chỉnh số dư thiếu truy vết; xóa tài khoản làm mất liên kết lịch sử. |

## Năng lực nghiệp vụ chính

- Tạo và xác thực tài khoản bằng thông tin trực tiếp hoặc nhà cung cấp danh tính bên ngoài.
- Duy trì trạng thái truy cập an toàn và gia hạn hợp lệ.
- Xem, cập nhật và quản lý trạng thái hồ sơ.
- Phân biệt quyền người học và quyền quản trị.
- Quản lý trạng thái hoạt động của tài khoản và thông tin thiết bị nhận tin.
- Cung cấp danh tính thống nhất cho tiến độ, tài sản, nhóm và lịch sử hoạt động.

## Điểm cần làm rõ

- Có yêu cầu xác minh thư điện tử, khôi phục mật khẩu và xác thực tăng cường hay không?
- Khi liên kết đăng nhập bên ngoài với tài khoản đã tồn tại, tiêu chí hợp nhất là gì?
- Người dùng có được đổi tên hiển thị tự do và tên đó có cần kiểm duyệt không?
- Quy trình xuất, ẩn danh hóa và xóa toàn bộ dữ liệu người dùng cần tuân theo chính sách nào?
