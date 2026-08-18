# Decisions

Chỉ cập nhật file này khi thay đổi hành vi sản phẩm, kiến trúc, dữ liệu, bảo mật hoặc quy trình chung. Thay đổi triển khai nhỏ được ghi trực tiếp ở `TASKS.md` và Git.

## Quyết định đang có hiệu lực

| ID | Quyết định | Hệ quả |
|---|---|---|
| D-001 | WSA-7K2 là người thực hiện và tự nghiệm thu duy nhất | Không cần reviewer, authority, bàn giao hoặc chữ ký; REL/CT/gate là checklist tự kiểm |
| D-002 | Dùng checkout hiện tại của `vocamon-project` | Không tạo worktree hoặc nhánh theo bí danh; luôn bảo toàn thay đổi có sẵn |
| D-003 | `TASKS.md` là nguồn task và trạng thái duy nhất | Bảng import, tracker theo lát và nhật ký WSA cũ chỉ để tham chiếu |
| D-004 | Git commit là nhật ký công việc | Không duy trì `NHAT-KY.md`; kết quả ngắn gọn nằm trong task và chi tiết nằm trong diff/commit |
| D-005 | Chỉ dùng bốn trạng thái | `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành` |
| D-006 | Evidence ID là tùy chọn | Chỉ tạo khi kết quả cần truy vết lâu dài; không phải điều kiện hoàn thành mặc định |
| D-007 | Kiến trúc API theo lớp | Domain → Application → Infrastructure → API; client không cung cấp domain truth đáng tin cậy |
| D-008 | Dữ liệu và bí mật được bảo vệ tại ranh giới | SQL là durable store, Redis là cache/coordination; không ghi secret, token, PII hoặc payload thô |
| D-009 | Task hậu tố `-A` chỉ hoàn thành phạm vi lát A | Không tự đóng parent hoặc phạm vi hoãn còn lại |
| D-010 | Giữ năng lực AI sinh nội dung và xử lý giọng nói người dùng tắt trong Giai đoạn A/B | Không mở endpoint/UI/job/provider traffic hoặc thu dữ liệu để dùng sau; chỉ thay đổi bằng quyết định mới sau khi REL-01/REL-03 và các cổng liên quan đạt |

## Khi nào cần thêm quyết định

Thêm một dòng mới khi lựa chọn làm thay đổi API/schema, kiến trúc, quyền, dữ liệu, bảo mật, hành vi người dùng hoặc phạm vi phát hành. Ghi rõ lựa chọn và hệ quả; không tạo tài liệu quyết định riêng nếu một dòng ở đây đã đủ.

## Câu hỏi chưa chốt

- Baseline môi trường/cấu hình nào là nguồn thật cho release candidate đầu tiên?
- Thị trường, tuổi và consent nào được dùng để tự đóng REL-01?
- Ngoài AI sinh nội dung và xử lý giọng nói người dùng đã tắt theo D-010, năng lực provider nào bật trong Giai đoạn A và năng lực nào tiếp tục tắt?
- Web, admin và mobile có cần hội tụ semantic token hay tiếp tục dùng theme riêng?

Chi tiết lịch sử vẫn có thể tra cứu trong các `QUYET-DINH-MO.md`, hồ sơ REL/CT và [quyết định chuyển đổi solo](./04-thuc-thi/phan-cong/QUYET-DINH-THUC-THI-SOLO.md).
