# Trung tâm tài liệu WordSoul

Tài liệu được tổ chức theo mục đích sử dụng để người phụ trách và AI agent có thể đi từ bối cảnh hệ thống đến công việc đang thực hiện mà không phải đọc toàn bộ kho tài liệu.

## Lộ trình đọc khuyến nghị

1. Đọc [Tổng quan hệ thống](./01-tong-quan/TONG-QUAN-HE-THONG.md).
2. Chọn module trong [Danh mục module](./02-modules/README.md).
3. Với Giai đoạn A, đọc [Kế hoạch triển khai](./03-ke-hoach-giai-doan-a/KE-HOACH-TRIEN-KHAI-GIAI-DOAN-A-B.md) và [Bảng import tổng](./03-ke-hoach-giai-doan-a/BANG-IMPORT-TONG-GIAI-DOAN-A.md).
4. Theo dõi công việc hiện tại tại [Thực thi Lát 0](./04-thuc-thi/lat-0/README.md).
5. Mở [Quy trình thực thi solo](./04-thuc-thi/phan-cong/README.md) để xem backlog và quy tắc làm việc hiện hành.
6. Dùng [Bộ bằng chứng Cổng A](./05-bang-chung/cong-a/README.md) khi nghiệm thu.

## Cấu trúc thư mục

| Khu vực | Nội dung | Trạng thái sử dụng |
|---|---|---|
| [01-tong-quan](./01-tong-quan/) | Bản đồ hệ thống và backlog tổng hợp | Nguồn nhập môn |
| [02-modules](./02-modules/) | 12 module; phân tích, quyết định và backlog nguồn | Nguồn sự thật theo module |
| [03-ke-hoach-giai-doan-a](./03-ke-hoach-giai-doan-a/) | Baseline, lát triển khai, task import và cổng phát hành | Kế hoạch đang dùng |
| [04-thuc-thi](./04-thuc-thi/) | Hồ sơ và sổ theo dõi công việc đang thực hiện | Cập nhật thường xuyên |
| [05-bang-chung](./05-bang-chung/) | Mẫu và sổ bằng chứng nghiệm thu | Dùng khi tạo/duyệt bằng chứng |
| [90-luu-tru](./90-luu-tru/) | Quy trình phân tích đã hoàn thành và mẫu có thể tái sử dụng | Chỉ đọc khi cần truy vết |

## Nguồn sự thật và quy tắc cập nhật

- Phạm vi module nằm trong `README.md` của module.
- Chi tiết nghiệp vụ nằm trong `PHAN-TICH-CHUYEN-SAU.md`.
- Quyết định sản phẩm nằm trong `QUYET-DINH-MO.md`; tên file được giữ để truy vết lịch sử dù nhiều quyết định đã chốt.
- Danh sách công việc nguồn nằm trong `TASK-BACKLOG.md`.
- Trạng thái triển khai thực tế nằm trong khu vực `04-thuc-thi`, không ghi ngược trạng thái vận hành vào phân tích nguồn.
- Bằng chứng quan trọng được ghi trong khu vực `05-bang-chung` và liên kết về task/hồ sơ liên quan; Evidence ID không bắt buộc cho mọi task.
- Tài liệu lưu trữ không được dùng làm trạng thái hiện hành nếu đã có tài liệu thay thế trong khu vực đang hoạt động.
- [Quyết định thực thi solo](./04-thuc-thi/phan-cong/QUYET-DINH-THUC-THI-SOLO.md) có hiệu lực ưu tiên hơn mọi yêu cầu reviewer, authority, người xác nhận hoặc phê duyệt còn sót lại trong tài liệu cũ.

## Quy trình dành cho Codex

Khi giao Codex đọc, cập nhật task, bằng chứng hoặc bàn giao, sử dụng skill dự án [`$wordsoul-doc-workflow`](../.agents/skills/wordsoul-doc-workflow/SKILL.md). Skill bắt buộc xác định đúng mã phụ trách, đọc nguồn theo thứ tự và đồng bộ nhật ký trước khi kết luận công việc hoàn thành.

## Phạm vi thay đổi hiện tại

Kho tài liệu mô tả và lập kế hoạch nghiệp vụ. Trạng thái task không tự chứng minh mã nguồn đã thay đổi; người thực hiện phải tự kiểm tra theo Definition of Done và checklist liên quan.
