# M12 — Tích hợp nền tảng và tài sản số

## Tài liệu phân tích

- [Phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Task backlog](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)
- [Đánh giá hiện trạng A-WP04](DANH-GIA-HIEN-TRANG-A-WP04.md)
- [Từ điển tích hợp M12](TU-DIEN-TICH-HOP.md)
- [Sổ đăng ký năng lực tích hợp M12](SO-DANG-KY-NANG-LUC-TICH-HOP.md)
- [Phân loại mức quan trọng và tác động tích hợp M12](PHAN-LOAI-MUC-QUAN-TRONG-VA-TAC-DONG.md)
- [Hợp đồng dữ liệu chuẩn tích hợp M12](HOP-DONG-DU-LIEU-CHUAN.md)
- [Trạng thái kết quả và lỗi chuẩn M12](TRANG-THAI-KET-QUA-VA-LOI.md)
- [Sổ đăng ký use case trạng thái chia sẻ M12](SO-DANG-KY-TRANG-THAI-CHIA-SE.md)
- [Ma trận giới hạn lưu lượng M12](MA-TRAN-GIOI-HAN-LUU-LUONG.md)
- [Ma trận fail mode và suy giảm M12](MA-TRAN-FAIL-MODE-VA-SUY-GIAM.md)
- [Danh mục loại tài sản số M12](DANH-MUC-LOAI-TAI-SAN-SO.md)
- [Xây dựng sổ quyền tài sản — Lát A M12](SO-QUYEN-TAI-SAN-A.md)
- [Kiểm kê và phân loại bí mật M12](KIEM-KE-VA-PHAN-LOAI-BI-MAT.md)
- [Lập bản đồ dữ liệu rời hệ thống — Lát A M12](BAN-DO-DU-LIEU-ROI-HE-THONG-A.md)
- [Thiết kế namespace, TTL và invalidation M12](THIET-KE-NAMESPACE-TTL-VA-INVALIDATION.md)
- [Chốt khóa phân tán và ownership M12](CHOT-KHOA-PHAN-TAN-VA-OWNERSHIP.md)
- [Chuẩn hóa timeout, deadline và hủy M12](CHUAN-HOA-TIMEOUT-DEADLINE-VA-HUY.md)
- [Chuẩn hóa retry và idempotency M12](CHUAN-HOA-RETRY-VA-IDEMPOTENCY.md)
- [Thiết kế circuit breaker và bulkhead M12](THIET-KE-CIRCUIT-BREAKER-VA-BULKHEAD.md)
- [Đặc tả dữ liệu danh tính tối thiểu M12](DAC-TA-DU-LIEU-DANH-TINH-TOI-THIEU.md)
- [Thiết kế chống giả mạo và phát lại M12](THIET-KE-CHONG-GIA-MAO-VA-PHAT-LAI.md)
- [Chốt vòng đời token ngoài M12](CHOT-VONG-DOI-TOKEN-NGOAI.md)
- [Thiết kế vòng đời bí mật M12](THIET-KE-VONG-DOI-BI-MAT.md)
- [Đặc tả liên kết và ngắt liên kết M12](DAC-TA-LIEN-KET-VA-NGAT-LIEN-KET.md)
- [Thiết kế suy giảm khi danh tính ngoài lỗi M12](THIET-KE-SUY-GIAM-KHI-DANH-TINH-NGOAI-LOI.md)
- [Chốt che dữ liệu và chính sách log M12](CHOT-CHE-DU-LIEU-VA-CHINH-SACH-LOG.md)
- [Đặc tả upload an toàn M12](DAC-TA-UPLOAD-AN-TOAN.md)
- [Thiết kế metadata và định danh bất biến M12](THIET-KE-METADATA-VA-DINH-DANH-BAT-BIEN.md)
- [Chốt quyền truy cập và phân phối M12](CHOT-QUYEN-TRUY-CAP-VA-PHAN-PHOI.md)
- [Thiết kế thay thế, xóa và orphan cleanup M12](THIET-KE-THAY-THE-XOA-VA-ORPHAN-CLEANUP.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Tích hợp nền tảng và tài sản số |
| Mục đích | Cung cấp một ranh giới thống nhất giữa nghiệp vụ WordSoul với các năng lực bên ngoài và năng lực hạ tầng dùng chung, giúp các module nghiệp vụ không phụ thuộc trực tiếp vào đặc thù từng nhà cung cấp. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** kết nối nhà cung cấp danh tính; hỗ trợ tạo nội dung bằng trí tuệ nhân tạo; tìm và lưu tài sản hình ảnh; tổng hợp/đánh giá giọng nói; gửi thư và thông báo đẩy; truyền dữ liệu tức thời; lưu tạm dữ liệu phù hợp; kiểm soát tần suất; báo cáo trạng thái và lỗi tích hợp. **Không chịu trách nhiệm:** quyết định nội dung nào được xuất bản, người dùng phát âm đạt hay không, thời điểm cần nhắc học, ai thắng trận hoặc ai được nhận thưởng. |
| Đầu vào (Input) | Yêu cầu có mục đích rõ ràng từ module nghiệp vụ; dữ liệu cần xử lý ở mức tối thiểu; thông tin cấu hình và quyền truy cập dịch vụ từ vận hành; phản hồi hoặc sự kiện từ nhà cung cấp bên ngoài. |
| Đầu ra (Output) | Kết quả đã chuẩn hóa cho module gọi; tài sản số và vị trí tham chiếu; trạng thái gửi; kết quả xử lý giọng nói; nội dung gợi ý; lỗi và chỉ số sử dụng cho quản trị; phương án suy giảm chức năng khi dịch vụ ngoài không sẵn sàng. |
| Phụ thuộc (Dependencies) | Các nhà cung cấp danh tính, trí tuệ nhân tạo, giọng nói, hình ảnh, thư và thông báo đẩy; quản trị, cấu hình và quan sát hệ thống; kho lưu trữ và năng lực vận hành dùng chung. |
| Người dùng/vai trò liên quan | Không có người dùng nghiệp vụ trực tiếp; người học và quản trị viên tương tác gián tiếp qua các module sử dụng; đội vận hành theo dõi chất lượng và chi phí. |
| Độ ưu tiên | Trung bình |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Phụ thuộc nhà cung cấp; chi phí tăng không kiểm soát; giới hạn lưu lượng; lộ thông tin bí mật hoặc dữ liệu cá nhân; điều khoản bản quyền; độ trễ; kết quả không ổn định; thay đổi dịch vụ ngoài; thiếu phương án dự phòng; lưu tạm dữ liệu cũ gây kết quả sai. |

## Năng lực nghiệp vụ chính

- Chuẩn hóa trao đổi với các dịch vụ bên ngoài để module nghiệp vụ chỉ nhận kết quả cần thiết.
- Quản lý vòng đời tải lên, lưu trữ và loại bỏ tài sản số.
- Theo dõi mức dùng, chi phí, lỗi và chất lượng theo từng năng lực.
- Giới hạn tần suất và ngăn lạm dụng các hoạt động tốn chi phí.
- Cho phép suy giảm có kiểm soát: vẫn học được khi chức năng phụ trợ tạm gián đoạn, nếu nghiệp vụ cho phép.
- Bảo vệ thông tin kết nối và giảm tối đa dữ liệu cá nhân gửi ra ngoài.

## Điểm cần làm rõ

- Dịch vụ bên ngoài nào là lựa chọn chính thức và cam kết chất lượng của từng dịch vụ là gì?
- Dữ liệu nào được phép rời khỏi hệ thống, đặc biệt là âm thanh và thông tin cá nhân?
- Mức chi phí tối đa theo người dùng/tháng và ngưỡng cảnh báo là bao nhiêu?
- Năng lực nào cần nhà cung cấp dự phòng và tiêu chí chuyển đổi là gì?
