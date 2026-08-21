# Lập danh mục tín hiệu nguồn M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-SIGNAL-CATALOG-1.0` |
| Task | M10-T003 |
| Đầu vào | M10-NOTIFICATION-TAXONOMY-1.0 (M10-T002), REL-06 |
| Phạm vi | Danh mục các tín hiệu nguồn từ các module khác (M01, M04, M06, M07) được phép khởi tạo thông báo trong M10 |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định danh mục tín hiệu sự kiện nguồn được kích hoạt tạo thông báo trong M10.

- **Tiêu chuẩn Tín hiệu Nguồn Chấp nhận (`Signal Qualification Invariant`)**: Một tín hiệu sự kiện gửi về M10 BẮT BUỘC chứa: `SourceModule`, `TriggerEventId`, `TargetUserId`, `CategoryCode` (`SECURITY`, `STUDY`, `REWARD`, `SYSTEM`), `Priority` và `TimeToLiveSeconds`.
- **Ràng buộc Giờ Yên tĩnh (REL-06 Quiet Hours Invariant)**: Tín hiệu thuộc nhóm `STUDY` và `REWARD` phát ra trong khung giờ 22:00 - 07:00 (Giờ địa phương của người dùng) BẮT BUỘC bị hoãn phát tin PUSH sang 07:01 sáng hôm sau. Tín hiệu `SECURITY` được phép miễn trừ khỏi Giờ yên tĩnh.

## 2. Bảng Danh mục Tín hiệu Nguồn (Notification Signal Catalog)

| Mã Tín hiệu | Module Nguồn | Nhóm | Mức Ưu tiên | TTL mặc định | Miễn Quiet Hours |
|---|---|---|---|---|---|
| `NEW_DEVICE_LOGIN` | M01 Auth | `SECURITY` | `HIGH` | 24 giờ | **CÓ** |
| `DUE_REVIEWS_REMINDER` | M04 SRS | `STUDY` | `MEDIUM` | 12 giờ | KHÔNG |
| `STREAK_DANGER_WARNING` | M07 Quest | `STUDY` | `HIGH` | 6 giờ | KHÔNG |
| `QUEST_REWARD_CLAIMABLE` | M07 Quest | `REWARD` | `MEDIUM` | 48 giờ | KHÔNG |
| `SYSTEM_MAINTENANCE` | M11 Admin | `SYSTEM` | `LOW` | 7 ngày | KHÔNG |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SC-G01`: 100% tín hiệu `STUDY` phát ra vào 23:00 được giữ lại trong hàng đợi hoãn, phát tin PUSH sau 07:00 sáng.
- `SC-G02`: Tín hiệu `SECURITY` phát lúc 02:00 sáng được gửi tin PUSH ngay lập tức.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SC03-01` | M04 gửi tín hiệu `DUE_REVIEWS_REMINDER` lúc 20:00 | Khởi tạo thông báo và gửi PUSH ngay lập tức. |
| `SC03-02` | M04 gửi tín hiệu `DUE_REVIEWS_REMINDER` lúc 23:30 | Tạo In-App notification, hoãn Push sang 07:01 sáng. |
| `SC03-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-SIGNAL-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-SC-F01` | Bổ sung helper `QuietHoursEvaluator` trong Domain M10 | Đánh giá múi giờ và thời gian đẩy PUSH | M10-T004 |

## 5. Tự kiểm M10-T003
- Đã đặc tả lập danh mục tín hiệu nguồn M10-T003.
- Ghi nhận 2 Regression Gates (`SC-G01`–`SC-G02`) và 3 Test Cases (`SC03-01`–`SC03-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả lập danh mục tín hiệu nguồn M10-T003 | WSA-7K2 |
