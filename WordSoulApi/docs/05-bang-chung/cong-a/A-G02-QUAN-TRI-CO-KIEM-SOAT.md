# Mẫu bằng chứng A-G02 — Quản trị có kiểm soát

## 1. Hồ sơ nghiệm thu

| Trường | Nội dung |
|---|---|
| Phạm vi phát hành | [Điền] |
| Chủ trì cổng | [Điền] |
| Người xác nhận | [Điền] |
| REL bắt buộc | REL-02; REL-07 khi có thao tác hỗ trợ dữ liệu |
| Task trọng tâm | M01-T028–M01-T032; M01-T038–M01-T041; M11-T002–M11-T007; M11-T027–M11-T035 |
| Kết luận | [Chưa đánh giá/Đạt/Không đạt/Đạt có điều kiện] |

## 2. Ma trận quyền tối thiểu

| Hành động | Module sở hữu | Đối tượng/phạm vi dữ liệu | Vai trò được phép | Vai trò bị từ chối | Xác minh lại | Lý do/vụ việc bắt buộc | Hạn mức | Audit bắt buộc | Hành vi khi không ghi được audit | Evidence ID |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Có/Không] | [Điền] | [Điền] | [Điền] | [Dừng/Từ chối] | [EV] |

## 3. Mẫu audit trước–sau

| Trường bằng chứng | Giá trị yêu cầu |
|---|---|
| Định danh sự kiện bất biến | [Điền] |
| Thời điểm chuẩn | [Điền] |
| Tác nhân thực hiện | [Định danh nội bộ; không dùng tên hiển thị làm nguồn duy nhất] |
| Vai trò và phạm vi quyền tại thời điểm thực hiện | [Điền] |
| Mục đích/lý do/vụ việc | [Điền] |
| Hành động và module sở hữu | [Điền] |
| Đối tượng bị tác động | [Định danh tối thiểu] |
| Trạng thái trước | [Metadata cho phép; đã che dữ liệu] |
| Trạng thái sau | [Metadata cho phép; đã che dữ liệu] |
| Kết quả | [Thành công/Từ chối/Thất bại/Một phần] |
| Lý do kết quả | [Điền] |
| Correlation và task thay đổi | [Điền] |
| Chính sách lưu giữ | [Điền] |

## 4. Kịch bản nghiệm thu bắt buộc

| Case ID | Kịch bản | Kết quả mong đợi | Evidence ID | Kết quả thực tế | Đạt/Không đạt |
|---|---|---|---|---|---|
| G02-C01 | Vai trò không đủ thử xem dữ liệu nhạy cảm | Từ chối mặc định và có audit tối thiểu | [EV] | [Điền] | [Điền] |
| G02-C02 | Vai trò có quyền xem thử sửa | Từ chối quyền sửa; không suy diễn quyền | [EV] | [Điền] | [Điền] |
| G02-C03 | Thao tác nhạy cảm thiếu xác minh lại | Từ chối, không thay đổi dữ liệu | [EV] | [Điền] | [Điền] |
| G02-C04 | Admin thử tự cấp vai trò cao nhất | Từ chối theo quyết định | [EV] | [Điền] | [Điền] |
| G02-C05 | Dùng đường quyền tạm thời/khẩn cấp | Không tồn tại hoặc bị từ chối; M11-T006 cũ không được triển khai | [EV] | [Điền] | [Điền] |
| G02-C06 | Thao tác thiếu lý do/vụ việc | Từ chối khi trường bắt buộc thiếu | [EV] | [Điền] | [Điền] |
| G02-C07 | Kho audit không sẵn sàng | Thao tác nhạy cảm dừng, không trả thành công giả | [EV] | [Điền] | [Điền] |
| G02-C08 | Hàng đợi audit đầy | Không loại bằng chứng audit/bảo mật | [EV] | [Điền] | [Điền] |
| G02-C09 | Tra cứu người dùng/log ngoài vụ việc | Từ chối hoặc che/giới hạn đúng chính sách; lượt xem được audit | [EV] | [Điền] | [Điền] |
| G02-C10 | Hai thay đổi quản trị đồng thời | Phát hiện xung đột, không ghi đè âm thầm | [EV] | [Điền] | [Điền] |

## 5. Điều kiện đạt

- Mọi hành động quản trị trong phạm vi có chủ, quyền tối thiểu và trường hợp từ chối.
- Không có đường cấp quyền cao nhất trái chính sách hoặc quyền khẩn cấp/tạm thời.
- Thao tác nhạy cảm yêu cầu xác minh lại, lý do và audit trước–sau.
- Audit/bảo mật không bị loại khi quá tải và không chứa payload/bí mật ngoài allowlist.
- REL-02 đạt; các thao tác hỗ trợ xuất/xóa liên quan phải đáp ứng REL-07.
