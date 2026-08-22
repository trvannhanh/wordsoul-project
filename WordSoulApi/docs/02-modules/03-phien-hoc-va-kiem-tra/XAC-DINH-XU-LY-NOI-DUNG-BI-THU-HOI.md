# Xác định xử lý nội dung bị thu hồi M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-RECALLED-CONTENT-HANDLING-1.0` |
| Task | M03-T008 |
| Đầu vào | M02-RECALLED-SET-HANDLING-1.0 (M02-T041), M03-SESSION-CONTENT-SNAPSHOT-1.0 (M03-T007) |
| Phạm vi | Xử lý các phiên học đang chạy hoặc chưa khởi tạo khi bộ từ vựng bị thu hồi (Thu hồi thường vs Thu hồi khẩn cấp) |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định phân loại hành vi xử lý phiên học trong M03 khi bộ từ vựng bị thu hồi từ M02.

- **Phân biệt Thu hồi Thường vs Thu hồi Khẩn cấp (`Normal vs Emergency Recall Invariant`)**:
  - *Thu hồi Thường (Normal Recall)*: Phiên học đang `IN_PROGRESS` vẫn được phép tiếp tục làm và hoàn thành bình thường bằng `SessionSnapshotJson` đã lưu. Chỉ chặn khởi tạo phiên học mới.
  - *Thu hồi Khẩn cấp (Emergency Recall - Ví dụ sai sót nội dung nghiêm trọng/vi phạm pháp luật)*: Tất cả các phiên học đang `IN_PROGRESS` của bộ từ đó bị hệ thống lập tức HỦY BỎ (chuyển `ABANDONED`) kèm lý do `EMERGENCY_RECALL_EVICTION`.
- **Cấm Khởi tạo Phiên mới (`Block New Sessions`)**: 100% request tạo phiên mới từ bộ bị thu hồi đều bị chặn.

## 2. Ma trận Xử lý Thu hồi Nội dung (Recall Action Matrix)

| Mức Thu hồi | Tác động Phiên New | Tác động Phiên đang Running | Phản hồi cho Người dùng |
|---|---|---|---|
| `NORMAL_RECALL` | Chặn khởi tạo (HTTP 403) | **Cho phép hoàn thành bình thường** | Thông báo: "Bộ từ dừng nhận học viên mới" |
| `EMERGENCY_RECALL` | Chặn khởi tạo (HTTP 403) | **Hủy phiên ngay lập tức** | Thông báo: "Phiên học bị dời do thu hồi nội dung khẩn" |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RC-G01`: 100% request tạo phiên mới cho bộ từ `RECALLED` bị chối bỏ.
- `RC-G02`: Thu hồi khẩn cấp chuyển trạng thái các phiên running của bộ đó sang `ABANDONED` trong $\le 5\text{s}$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC08-01` | Admin thực hiện Thu hồi thường bộ từ A1 | Learner đang học ở bước 3 vẫn làm tiếp và chốt phiên `COMPLETED` bình thường. |
| `RC08-02` | Admin thực hiện Thu hồi khẩn cấp bộ từ A1 | Phiên học đang ở bước 3 lập tức bị dừng, ném lỗi HTTP 410 `SESSION_CANCELLED_EMERGENCY_RECALL`. |
| `RC08-03` | Kiểm thử hoàn tất luồng M03-RECALLED-CONTENT-HANDLING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-RC-F01` | Lắng nghe event `EmergencyContentRecallEvent` từ Redis Pub/Sub | Thực hiện hủy phiên running tức thì | M03-T004 |

## 5. Tự kiểm M03-T008
- Đã hoàn thành đặc tả `M03-RECALLED-CONTENT-HANDLING-1.0`.
- Chốt nguyên tắc phân biệt thu hồi thường và thu hồi khẩn cấp.
- Ghi nhận 2 Regression Gates (`RC-G01`–`RC-G02`) và 3 Test Cases (`RC08-01`–`RC08-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định xử lý nội dung bị thu hồi M03-T008 | WSA-7K2 |
