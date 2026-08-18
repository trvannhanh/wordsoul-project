# Mẫu prompt phân tích chuyên sâu module

## Trạng thái

Mẫu này thay thế 12 prompt theo module đã hoàn thành. Khi cần phân tích lại một module, sao chép bảng mô tả module hiện hành và các quyết định mới nhất vào phần đầu vào; không dùng bản mô tả cũ nếu phạm vi đã thay đổi.

## Nội dung yêu cầu

Phân tích chuyên sâu module từ bảng mô tả được cung cấp với mục tiêu tạo danh sách công việc nghiệp vụ cụ thể. Không viết code, không đặt tên lớp, hàm hoặc điểm tích hợp kỹ thuật.

Kết quả phải:

1. Làm rõ mục tiêu, phạm vi chịu trách nhiệm và phần không thuộc module.
2. Liệt kê các chức năng/nghiệp vụ con, quan hệ phụ thuộc và vai trò liên quan.
3. Với từng chức năng con, mô tả luồng nghiệp vụ chính theo bước, trường hợp đặc biệt và Definition of Done có thể kiểm chứng.
4. Chuyển chức năng con thành task có mã truy vết, tên, mô tả, input, output, ưu tiên, độ phức tạp S/M/L và task phụ thuộc.
5. Tách rõ quyết định đã chốt, giả định tạm thời, câu hỏi còn mở và rủi ro chặn.
6. Xuất bảng task có thể chuyển vào Jira, Trello hoặc Notion nhưng không gắn ngày khi chưa có năng lực đội ngũ.
7. Đối chiếu task mới với backlog hiện hành để tránh tạo nguồn sự thật trùng lặp.

## Đầu vào bắt buộc

- Toàn bộ bảng mô tả module hiện hành.
- Quyết định đã chốt của module.
- Module phụ thuộc và hợp đồng nghiệp vụ liên quan.
- Backlog hiện hành nếu đây là lần phân tích lại.
- Phạm vi giai đoạn và các năng lực được giữ tắt.
