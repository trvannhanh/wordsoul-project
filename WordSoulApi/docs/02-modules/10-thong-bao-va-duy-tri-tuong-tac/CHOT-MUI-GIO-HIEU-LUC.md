# Chốt múi giờ hiệu lực M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-USER-TIMEZONE-EVALUATION-1.0` |
| Task | M10-T025 |
| Đầu vào | M01-TIMEZONE-1.0 (M01-T025), M10-CHANNEL-SELECTION-MATRIX-1.0 (M10-T020) |
| Phạm vi | Quy trình xác định múi giờ địa phương (`UserTimeZoneId`, ví dụ `Asia/Ho_Chi_Minh`) để tính toán chính xác khung Giờ yên tĩnh (Quiet Hours) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định việc sử dụng múi giờ địa phương người dùng khi lập lịch thông báo.

- **Tính Chính xác của Múi giờ Địa phương (`Local TimeZone Evaluation Invariant`)**: Thời gian phát thông báo PUSH BẮT BUỘC được đánh giá dựa trên múi giờ địa phương hiện tại của người dùng (`UserTimeZoneId`). Mặc định khi thiếu dữ liệu là `Asia/Ho_Chi_Minh` (UTC+7).
- **Ràng buộc Xử lý Đổi Múi giờ (`TimeZone Shift Invariant`)**: Khi người dùng di chuyển sang múi giờ mới, hệ thống tự động cập nhật `UserTimeZoneId` từ Client mà CẤM gây phát đúp hoặc mất lịch nhắc ôn tập.

## 2. Dynamic Local Time Conversion Formula

$$\text{LocalTime} = \text{UtcNow} + \text{TimeZoneOffset}(\text{UserTimeZoneId})$$

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TZ-G01`: 100% đánh giá Giờ yên tĩnh sử dụng đúng `UserTimeZoneId` của hồ sơ người dùng.
- `TZ-G02`: Người dùng thiếu thông tin múi giờ tự động gán fallback `Asia/Ho_Chi_Minh`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TZ25-01` | Người dùng ở múi giờ `America/New_York` (UTC-5) | Giờ yên tĩnh 22:00-07:00 được tính theo giờ New York. |
| `TZ25-02` | Người dùng chưa bao giờ cập nhật múi giờ | System dùng fallback `Asia/Ho_Chi_Minh` (UTC+7). |
| `TZ25-03` | Kiểm thử hoàn tất luồng M10-USER-TIMEZONE-EVALUATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-TZ-F01` | Cần Sync `UserTimeZoneId` từ ứng dụng di động mỗi khi đăng nhập | Đảm bảo múi giờ luôn mới nhất | M10-T026 |

## 5. Tự kiểm M10-T025
- Đã đặc tả chốt múi giờ hiệu lực M10-T025.
- Ghi nhận 2 Regression Gates (`TZ-G01`–`TZ-G02`) và 3 Test Cases (`TZ25-01`–`TZ25-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt múi giờ hiệu lực M10-T025 | WSA-7K2 |
