# Mẫu bằng chứng A-G01 — Tài khoản an toàn

## 1. Hồ sơ nghiệm thu

| Trường | Nội dung |
|---|---|
| Phạm vi phát hành | [Điền] |
| Chủ trì cổng | [Điền] |
| Người xác nhận | [Điền] |
| Ngày đánh giá | [Điền] |
| REL bắt buộc | REL-01; REL-07 |
| Task trọng tâm | M01-T002–M01-T021; M01-T031; M01-T033–M01-T037; M12-T006–M12-T010 |
| Kết luận | [Chưa đánh giá/Đạt/Không đạt/Đạt có điều kiện] |

## 2. Ma trận trạng thái và quyền tài khoản

| Trạng thái | Điều kiện vào | Hành động người dùng được phép | Hành động bị từ chối | Hiệu lực phiên hiện có | Hiển thị dữ liệu/hồ sơ | Điều kiện thoát | Bằng chứng ID |
|---|---|---|---|---|---|---|---|
| Chờ xác minh thư | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Hoạt động | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Tạm khóa do rủi ro | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Khóa quản trị | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Chờ xóa | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Đã xóa/ẩn danh | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |

## 3. Ma trận tuổi, thị trường và đồng ý

| Thị trường | Nhóm tuổi | Dữ liệu tuổi được dùng | Loại đồng ý | Có cần người giám hộ | Chức năng được phép khi thiếu đồng ý | Cách thu hồi đồng ý | Căn cứ REL-01 | Bằng chứng ID |
|---|---|---|---|---|---|---|---|---|
| [Điền] | [Điền] | [Điền] | [Điền] | [Có/Không] | [Điền] | [Điền] | [Liên kết] | [EV] |

## 4. Ma trận vòng đời phiên

| Tình huống | Access session | Refresh session | Thiết bị khác | Yêu cầu xác minh lại | Kết quả bắt buộc | Thời hạn hiệu lực | Bằng chứng ID |
|---|---|---|---|---|---|---|---|
| Đăng nhập mới | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Đăng xuất một thiết bị | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Đăng xuất tất cả | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Đổi thông tin bảo mật | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |
| Khóa tài khoản | [Điền] | [Điền] | [Điền] | [Điền] | Mọi đường truy cập bị chặn theo chính sách | [Điền] | [EV] |
| Phát hiện refresh dùng lại | [Điền] | [Điền] | [Điền] | [Điền] | Thu hồi phạm vi phiên liên quan và cảnh báo | [Điền] | [EV] |
| Yêu cầu xóa | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [EV] |

## 5. Kịch bản nghiệm thu bắt buộc

| Case ID | Kịch bản | Dữ liệu thử | Kết quả mong đợi | Evidence ID | Kết quả thực tế | Đạt/Không đạt |
|---|---|---|---|---|---|---|
| G01-C01 | Đăng ký hợp lệ và xác minh thư | Dữ liệu giả | Chỉ chuyển hoạt động sau điều kiện đã duyệt | [EV] | [Điền] | [Điền] |
| G01-C02 | Thư trùng hoặc định danh xung đột | Dữ liệu giả | Không lộ sự tồn tại tài khoản; không tự ghép | [EV] | [Điền] | [Điền] |
| G01-C03 | Mã xác minh hết hạn/dùng lại | Dữ liệu giả | Bị từ chối, có giới hạn thử lại | [EV] | [Điền] | [Điền] |
| G01-C04 | Đăng nhập ngoài với state sai/redirect sai | Dữ liệu giả | Bị từ chối; không tạo phiên | [EV] | [Điền] | [Điền] |
| G01-C05 | Provider trả email trùng tài khoản nội bộ | Dữ liệu giả | Không tự liên kết; yêu cầu bằng chứng mạnh hơn | [EV] | [Điền] | [Điền] |
| G01-C06 | Khóa trong khi nhiều thiết bị đang hoạt động | Dữ liệu giả | Mọi phiên thuộc phạm vi bị thu hồi/chặn | [EV] | [Điền] | [Điền] |
| G01-C07 | Refresh token bị phát lại | Dữ liệu giả | Phát hiện, thu hồi và cảnh báo theo chính sách | [EV] | [Điền] | [Điền] |
| G01-C08 | Khôi phục khi mất một hoặc mọi kênh | Dữ liệu giả | Không hạ chuẩn xác minh; có đường hỗ trợ kiểm soát | [EV] | [Điền] | [Điền] |
| G01-C09 | Xuất dữ liệu | Dữ liệu giả đa module | Xác minh chủ thể; manifest đầy đủ; phần lỗi rõ | [EV] | [Điền] | [Điền] |
| G01-C10 | Xóa và đăng ký lại | Dữ liệu giả đa module | Đúng thời gian chờ, ẩn danh/xóa, thu hồi phiên và chính sách tái đăng ký | [EV] | [Điền] | [Điền] |

## 6. Điều kiện đạt

- Không có trạng thái hoặc đường truy cập bỏ qua quyền đã chốt.
- Không tự liên kết tài khoản chỉ vì email trùng.
- Token không xuất hiện trong địa chỉ, log hoặc bằng chứng.
- Khóa/xóa/thay đổi bảo mật có hiệu lực trên mọi phiên thuộc phạm vi.
- REL-01 và REL-07 có kết luận phù hợp với phạm vi phát hành.
- Tất cả case bắt buộc đạt; sai lệch nghiêm trọng/rất cao bằng không.
