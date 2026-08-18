# Sổ theo dõi thực thi Lát 0

> Tracker cũ, chỉ để truy vết. Trạng thái hiện hành nằm trong [TASKS.md](../../TASKS.md).

## Trạng thái

| Chỉ số | Giá trị |
|---|---:|
| Tổng task | 12 |
| Chưa bắt đầu | 12 |
| Đang thực hiện | 0 |
| Bị chặn | 0 |
| Hoàn thành | 0 |

## Danh sách tự kiểm

| Task | Hồ sơ | Phụ thuộc | Trạng thái | Điều kiện tự kiểm chính |
|---|---|---|---|---|
| A0-T001 | [REL-01](./REL-01-TUOI-THI-TRUONG-VA-DONG-Y.md) | Không | Chưa bắt đầu | Phạm vi thị trường, tuổi và đồng ý rõ ràng |
| A0-T002 | [REL-02](./REL-02-QUYEN-VA-AUDIT-BU-TRU.md) | Không | Chưa bắt đầu | Ma trận quyền, từ chối và audit kiểm tra được |
| A0-T003 | [REL-03](./REL-03-BI-MAT-VA-TICH-HOP.md) | Không | Chưa bắt đầu | Registry tích hợp và cách xử lý bí mật rõ ràng |
| A0-T004 | [REL-04](./REL-04-QUYEN-SU-DUNG-TAI-SAN.md) | Không | Chưa bắt đầu | Quyền tài sản và quy trình gỡ/khiếu nại rõ ràng |
| A0-T005 | [REL-07](./REL-07-XUAT-VA-XOA-DU-LIEU.md) | A0-T001 | Chưa bắt đầu | Bản đồ dữ liệu, xuất/xóa và lỗi một phần rõ ràng |
| A0-T006 | [CT-01](./CT-01-DONG-CONG-KHAI-NOI-DUNG-CHUA-DUYET.md) | A0-T004 | Chưa bắt đầu | Nội dung chưa duyệt không có đường công khai |
| A0-T007 | [CT-02](./CT-02-KHONG-TU-GHEP-TAI-KHOAN.md) | Không | Chưa bắt đầu | Không tự ghép tài khoản theo email |
| A0-T008 | [CT-03](./CT-03-KHONG-DUNG-PAYLOAD-THO.md) | A0-T003 | Chưa bắt đầu | Không dùng payload thô làm bằng chứng |
| A0-T009 | [CT-04](./CT-04-KHONG-DUNG-HEALTH-GIA-DINH.md) | Không | Chưa bắt đầu | Không dùng health giả định làm kết luận |
| A0-T010 | [CT-05](./CT-05-CAM-BO-QUA-GIOI-HAN-LUU-LUONG.md) | A0-T003 | Chưa bắt đầu | Không có đường bỏ qua giới hạn lưu lượng |
| A0-T011 | [CT-06](./CT-06-GIU-AI-VA-GIONG-NOI-TAT.md) | A0-T001, A0-T003 | Chưa bắt đầu | AI và giọng nói vẫn tắt đúng phạm vi |
| A0-T012 | [CT-07](./CT-07-KHONG-MO-RONG-AP.md) | Không | Chưa bắt đầu | Không mở rộng AP ngoài phạm vi |

WSA-7K2 tự chuyển trạng thái sau khi làm và tự kiểm tra. Evidence ID chỉ được tạo khi kết quả cần truy vết lâu dài; không có bước bàn giao hoặc chờ phê duyệt.

## Nhật ký

| Ngày | Thay đổi | Kết quả tự kiểm |
|---|---|---|
| 2026-08-18 | Chuẩn hóa Lát 0 theo workflow một người | 12 task khớp danh sách solo; chưa task nào được tự động bắt đầu hoặc hoàn thành |
