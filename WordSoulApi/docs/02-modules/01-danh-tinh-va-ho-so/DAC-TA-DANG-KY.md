# Đặc tả dữ liệu đăng ký M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T005 |
| Phiên bản/trạng thái | 0.1-draft — chờ M01-T003/M01-T004 và REL-01 |
| Cơ sở | M01-D002–D004, M01-D011; bản đồ dữ liệu và chính sách bảo mật draft |

## Trường và chuẩn hóa

| Trường | Bắt buộc | Chuẩn hóa/validation | Tính duy nhất | Lỗi an toàn |
|---|---|---|---|---|
| Email | Có cho đăng nhập trực tiếp | Kiểm tra cấu trúc/độ dài; lưu dạng chuẩn theo chính sách; giữ giá trị hiển thị phù hợp | Duy nhất theo canonicalization đã phê duyệt; không tự ghép danh tính ngoài | Phản hồi trung tính ở luồng có nguy cơ dò tài khoản |
| Thông tin bảo mật | Có cho đăng nhập trực tiếp | Áp dụng đúng policy version M01-T004; không trim/biến đổi âm thầm | Không áp dụng | Chỉ báo quy tắc chưa đạt; không phản chiếu hoặc log giá trị |
| Tên hiển thị | Có | Unicode/độ dài/nội dung theo policy version; trim theo quy tắc công bố | Không bắt buộc duy nhất | Không tiết lộ hồ sơ khác có tên trùng |
| Nhóm tuổi/khu vực tự khai | Theo REL-01 | Chỉ tập giá trị/policy version được duyệt; không suy diễn ngày sinh | Không áp dụng | Không mở năng lực nhạy cảm khi không xác định được policy áp dụng |
| Đồng ý | Theo policy áp dụng | Loại, phiên bản, thời điểm, trạng thái; không gộp các mục đích độc lập | Idempotent theo account/consent/version | Thiếu đồng ý bắt buộc không được báo đăng ký hoàn tất đầy đủ |
| Correlation/attempt ID | Có ở hệ thống | Opaque, không chứa PII; ổn định cho một attempt/retry | Duy nhất cho attempt | Retry cùng intent không tạo tài khoản trùng |

## Luồng quyết định

1. Tiếp nhận attempt và khóa policy/schema version dùng để đánh giá.
2. Chuẩn hóa/kiểm tra từng trường; trả lỗi trường phù hợp nhưng không tiết lộ tài khoản tồn tại ở ngữ cảnh nhạy cảm.
3. Kiểm tra xung đột email trong transaction/idempotency boundary; không dùng email để tự liên kết provider.
4. Ghi danh tính và bản ghi đồng ý nhất quán; trạng thái đầu theo M01-T002.
5. Gửi yêu cầu xác minh qua hợp đồng tối thiểu; lỗi phụ thuộc không làm mất danh tính và có thể tiếp tục an toàn.
6. Khởi tạo quyền lợi module khác chỉ qua hợp đồng có idempotency; M01 không tự tạo tài sản.

## Ca nghiệm thu tối thiểu

- Thành công với dữ liệu biên hợp lệ; Unicode/khoảng trắng; email khác hoa thường theo policy.
- Email trùng, retry đồng thời và timeout sau ghi không tạo hai danh tính.
- Thông tin bảo mật yếu/đã lộ bị từ chối mà không rò vào log.
- Thiếu/thu hồi đồng ý và policy version đổi giữa attempt có kết quả xác định.
- Provider thư hoặc module khởi tạo lỗi tạo trạng thái tiếp tục được, không báo thành công giả.

## Điểm chờ và điều kiện duyệt

| Mã | Điểm chờ | Nguồn gỡ |
|---|---|---|
| M01-REG-O01 | Canonicalization/độ dài email và tên cụ thể | Chủ M01 + chuẩn kỹ thuật |
| M01-REG-O02 | Trường tuổi/khu vực và điều kiện đồng ý | REL-01/pháp lý |
| M01-REG-O03 | Transaction/idempotency boundary với M06 và thư | Hợp đồng M06/M10/M12 |

Chỉ duyệt khi M01-T003/T004 được xác nhận, O01–O03 có chủ/kết luận và mỗi quy tắc có ca cho phép/từ chối/retry/lỗi phụ thuộc.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema, luồng quyết định và ca nghiệm thu đăng ký | Chưa gán |
