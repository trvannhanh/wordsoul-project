# CT-04 — Không dùng health giả định

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T009 |
| Trạng thái | Sẵn sàng kích hoạt |
| Chủ trì / xác nhận | Chủ vận hành / Chủ M11 và M12 |
| Rủi ro kiểm soát | Phát hành hoặc tiếp tục vận hành dựa trên tín hiệu khỏe không kiểm tra phụ thuộc thật |

## Quy tắc tạm thời

- Điểm health hiện tại không đủ làm bằng chứng dịch vụ khỏe hoặc sẵn sàng phát hành.
- Năng lực trọng yếu phải có bước xác minh thủ công tạm thời và người ghi nhận kết quả.
- Khi chưa xác minh được phụ thuộc, trạng thái phải là chưa xác định hoặc suy giảm, không mặc định khỏe.
- Kết luận phát hành phải tham chiếu bằng chứng bổ sung ngoài điểm health hiện tại.

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / xác nhận | Chưa gán / Chưa gán |
| Bằng chứng định kỳ | Danh sách năng lực, phụ thuộc, cách kiểm tra thủ công, thời điểm và người xác nhận |
| Khi vi phạm | Thu hồi kết luận phát hành, đánh giá lại phạm vi và ghi nhận sự cố nếu gây ảnh hưởng |
| Điều kiện gỡ | M11-T036 và M12-T045 đạt |
| Thẩm quyền gỡ | Chủ vận hành với xác nhận của M11/M12 |
