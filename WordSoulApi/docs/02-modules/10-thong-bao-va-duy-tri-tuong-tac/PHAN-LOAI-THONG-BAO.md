# Phân loại thông báo M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-TAXONOMY-1.0` |
| Task | M10-T002 |
| Đầu vào | M10-NOTIF-INBOX-DICT-1.0 (M10-T001), REL-06 |
| Phạm vi | Phân loại 4 nhóm thông báo, định rõ mức độ ưu tiên (`Priority`), thời hạn hết hiệu lực (`TTL`) và kênh cho phép cho từng nhóm |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định phân loại và các thuộc tính vận hành của 4 nhóm thông báo trong M10.

- **Bắt buộc Gán Mức độ Ưu tiên và TTL (`Mandatory Priority & TTL Invariant`)**: 100% thông báo khởi tạo BẮT BUỘC được gán thuộc tính `Priority` (`HIGH`, `MEDIUM`, `LOW`) và `TimeToLive` (TTL). Thông báo quá TTL sẽ bị hệ thống tự động loại bỏ (Evict), không tiếp tục gửi ra ngoài.
- **Tuân thủ Quyền Hủy Nhận Tin theo Phân loại (`Category Opt-Out Invariant`)**: Mọi nhóm thông báo (trừ `SECURITY_ALERT`) bắt buộc tôn trọng ma trận Opt-Out của người dùng.

## 2. Bảng Phân loại Nhóm Thông báo (Notification Classification Matrix)

| Mã Nhóm | Tên Nhóm | Kênh cho phép | Ưu tiên | TTL mặc định | Cho phép Opt-Out |
|---|---|---|---|---|---|
| `SECURITY` | Cảnh báo an ninh | Push, Email, In-App | `HIGH` | 24 giờ | **KHÔNG** |
| `STUDY` | Nhắc học tập & Ôn tập | Push, In-App | `MEDIUM` | 12 giờ | **CÓ** |
| `REWARD` | Thưởng & Nhiệm vụ | In-App, Push | `MEDIUM` | 48 giờ | **CÓ** |
| `SYSTEM` | Cập nhật hệ thống | In-App, Email | `LOW` | 7 ngày | **CÓ** |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `NT-G01`: Thông báo quá hạn TTL không được hệ thống gửi PUSH/Email sang M12.
- `NT-G02`: 100% thông báo loại `SECURITY` luôn mang `Priority = HIGH`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `NT02-01` | Tạo thông báo nhắc học `STUDY` bị hoãn quá 12h (quá TTL) | Worker tự động hủy thông báo, không phát tin PUSH. |
| `NT02-02` | Khởi tạo thông báo `SECURITY` khi đăng nhập thiết bị mới | Gán `Priority = HIGH`, `TTL = 24h`, gửi ngay lập tức. |
| `NT02-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-TAXONOMY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-NT-F01` | Cần bổ sung Enum `NotificationPriority` trong Domain | Đảm bảo xử lý thứ tự hàng đợi gửi | M10-T004 |

## 5. Tự kiểm M10-T002
- Đã phân loại thông báo M10-T002 với 4 nhóm chuẩn.
- Ghi nhận 2 Regression Gates (`NT-G01`–`NT-G02`) and 3 Test Cases (`NT02-01`–`NT02-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả phân loại thông báo M10-T002 | WSA-7K2 |
