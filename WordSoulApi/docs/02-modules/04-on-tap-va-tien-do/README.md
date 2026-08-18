# M04 — Ôn tập ngắt quãng và tiến độ

## Tài liệu phân tích và lập kế hoạch

- [Prompt phân tích chuyên sâu](../../90-luu-tru/PROMPT-PHAN-TICH-MODULE-MAU.md)
- [Kết quả phân tích chuyên sâu](PHAN-TICH-CHUYEN-SAU.md)
- [Backlog task](TASK-BACKLOG.md)
- [Quyết định mở](QUYET-DINH-MO.md)

## Mô tả module

| Trường | Nội dung |
|---|---|
| Tên module | Ôn tập ngắt quãng và tiến độ |
| Mục đích | Duy trì khả năng ghi nhớ dài hạn bằng cách lượng hóa trạng thái từng từ, lựa chọn thời điểm ôn phù hợp và cung cấp bức tranh tiến bộ có ý nghĩa. |
| Phạm vi trách nhiệm | **Chịu trách nhiệm:** hồ sơ tiến độ theo người dùng–từ; tiếp nhận kết quả học và ôn; phân loại chất lượng nhớ; tính thời điểm ôn tiếp theo; chọn từ đến hạn; lưu lịch sử ôn; ước lượng khả năng duy trì và tổng hợp tiến độ bộ từ/người dùng. **Không chịu trách nhiệm:** tạo nội dung câu hỏi, điều phối giao diện phiên học, tự trao thưởng hoặc gửi thông báo trực tiếp. |
| Đầu vào (Input) | Kết quả câu trả lời và hoàn thành từ module phiên học; dữ liệu phát âm đủ điều kiện từ module luyện phát âm; cấu hình chính sách ôn từ quản trị; học liệu và quan hệ bộ từ từ module nội dung; danh tính người học. |
| Đầu ra (Output) | Danh sách từ đến hạn và thứ tự ưu tiên cho phiên ôn; thời điểm ôn tiếp theo; trạng thái thành thạo và chỉ số duy trì; tiến độ theo từ/bộ/người dùng; lịch sử ôn cho người học và quản trị; tín hiệu nhắc học cho module thông báo. |
| Phụ thuộc (Dependencies) | Danh tính và hồ sơ; nội dung từ vựng; phiên học và kiểm tra; luyện phát âm nếu phát âm tác động tới tiến độ; quản trị, cấu hình và quan sát. |
| Người dùng/vai trò liên quan | Người học xem tiến độ và nhận danh sách ôn; quản trị viên theo dõi hiệu quả; module phiên học và thông báo sử dụng kết quả. |
| Độ ưu tiên | Cao |
| Độ phức tạp ước tính | Cao |
| Rủi ro/Điểm cần lưu ý | Lịch ôn quá dày hoặc quá thưa; thay đổi chính sách làm mất tính so sánh; dữ liệu trùng khiến tiến độ nhảy sai; múi giờ làm sai ngày đến hạn; phát âm tác động quá mạnh; chỉ số “thành thạo” gây hiểu lầm; khó giải thích vì sao một từ được chọn ôn. |

## Năng lực nghiệp vụ chính

- Tạo và duy trì trạng thái học riêng cho mỗi cặp người dùng–từ.
- Cập nhật lịch ôn dựa trên chất lượng nhớ, lịch sử và chính sách đang áp dụng.
- Chọn và sắp xếp các từ cần ôn theo mức cấp thiết.
- Ghi lịch sử để giải thích thay đổi và phục vụ phân tích hiệu quả học.
- Tổng hợp tiến độ bộ từ và khả năng duy trì kiến thức chung.
- Tiếp nhận tác động bổ sung từ phát âm khi có quy tắc nghiệp vụ rõ ràng.

## Điểm cần làm rõ

- Chính sách ôn mong muốn tối ưu cho tỷ lệ nhớ mục tiêu bao nhiêu?
- Định nghĩa chính thức của trạng thái mới, đang học, đến hạn và thành thạo là gì?
- Kết quả phát âm thay đổi lịch ôn hay chỉ bổ sung một chiều kỹ năng độc lập?
- Người dùng có được tự đặt lại tiến độ hoặc đánh dấu một từ là đã biết không?
