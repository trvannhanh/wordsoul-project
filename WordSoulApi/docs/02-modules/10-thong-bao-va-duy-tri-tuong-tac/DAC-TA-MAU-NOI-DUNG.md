# Đặc tả mẫu nội dung M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-TEMPLATE-SPEC-1.0` |
| Task | M10-T011 |
| Đầu vào | M10-NOTIFICATION-TAXONOMY-1.0 (M10-T002) |
| Phạm vi | Cấu trúc bản mẫu nội dung thông báo (`NotificationTemplate`) hỗ trợ nội suy biến động (Dynamic Variable Interpolation) và fallback mặc định |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy chuẩn template tạo tiêu đề và nội dung cho thông báo In-App và PUSH.

- **Nội suy Biến An toàn (`Safe Variable Interpolation Invariant`)**: Template nội dung hỗ trợ placeholder dạng `{VariableName}`. Nếu biến nội suy bị thiếu dữ liệu khi render, hệ thống BẮT BUỘC sử dụng giá trị fallback an toàn (ví dụ: `{UserName}` $\implies$ "bạn học"). CẤM tuyệt đối việc render ra chuỗi `null` hoặc chuỗi rỗng trên giao diện người dùng.
- **Giới hạn Độ dài Tiêu đề & Nội dung Push (`Length Limit Invariant`)**: Tiêu đề Push CẤM vượt quá $50$ ký tự, nội dung Push CẤM vượt quá $150$ ký tự để không bị trích đoạn xấu trên màn hình khóa.

## 2. Bảng Danh mục Mẫu Nội dung Thông báo (Notification Template Catalog)

| Mã Template | Tiêu đề Mẫu | Nội dung Mẫu | Biến Nội suy | Fallback Mặc định |
|---|---|---|---|---|
| `TPL_DUE_REVIEW` | Đã đến giờ ôn tập! | Bạn có `{DueCount}` từ cần ôn hôm nay. | `DueCount` | "Nhiều từ vựng" |
| `TPL_STREAK_DANGER` | Cảnh báo mất Streak! | Bạn sắp mất chuỗi `{StreakDays}` ngày học. | `StreakDays` | "Streak" |
| `TPL_QUEST_CLAIM` | Nhiệm vụ hoàn thành! | Nhận ngay `{RewardAmount}` Gold thưởng. | `RewardAmount` | "phần thưởng" |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `NT-G01`: 100% nội dung Push render từ template có độ dài $\le 150$ ký tự.
- `NT-G02`: Render template thiếu biến nội suy không gây lỗi văng App và dùng đúng fallback.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `NT11-01` | Render template `TPL_DUE_REVIEW` với `DueCount = 15` | Tiêu đề: "Đã đến giờ ôn tập!", Nội dung: "Bạn có 15 từ cần ôn hôm nay." |
| `NT11-02` | Render template khi `DueCount` bị null | Content fallback: "Bạn có Nhiều từ vựng cần ôn hôm nay." |
| `NT11-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-TEMPLATE-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-NT-F01` | Cần bổ sung bảng `NotificationTemplates` trong CSDL Admin M11 | Cho phép Admin cập nhật mẫu thông báo | M10-T012 |

## 5. Tự kiểm M10-T011
- Đã đặc tả mẫu nội dung M10-T011.
- Ghi nhận 2 Regression Gates (`NT-G01`–`NT-G02`) và 3 Test Cases (`NT11-01`–`NT11-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả mẫu nội dung M10-T011 | WSA-7K2 |
