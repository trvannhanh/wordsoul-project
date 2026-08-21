# Thiết kế tính gói thưởng theo phiên bản M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-VERSIONED-REWARD-CALCULATION-1.0` |
| Task | M06-T014 |
| Đầu vào | M02-SET-REWARD-LINK-1.0 (M02-T042), M06-REWARD-EVENT-CONTRACT-1.0 (M06-T011) |
| Phạm vi | Thuật toán tính toán lượng thưởng tài sản dựa trên mã cấu hình phiên bản (`RewardConfigVersion`) tại thời điểm tạo phiên học/nhiệm vụ |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy tắc áp dụng cấu hình gói phần thưởng theo phiên bản thời gian.

- **Tính Bất biến của Cấu hình Gói thưởng (`Immutable Reward Package Invariant`)**: Gói thưởng gắn liền với một sự kiện BẮT BUỘC áp dụng phiên bản cấu hình thưởng (`RewardConfigCode` & `Version`) tại thời điểm khởi tạo sự kiện đó. Thay đổi cấu hình thưởng của Admin về sau CẤM áp dụng hồi tố cho các phiên/sự kiện đã tạo trước đó.
- **Trường hợp Cấu hình Ngừng hoạt động (`Deprecated Package Fallback`)**: Nếu mã cấu hình thưởng bị dừng (`DEPRECATED`), hệ thống tự động suy giảm về gói thưởng mặc định an toàn (`DEFAULT_BASE_REWARD`).

## 2. Bảng Phân loại Phiên bản Cấu hình Thưởng (Reward Package Matrix)

| Mã Gói thưởng | Phiên bản | Cấu hình Thưởng | Ngữ cảnh áp dụng |
|---|---|---|---|
| `PKG_LESSON_BASE` | v1 | 10 Gold + 20 Exp / phiên | Hoàn thành phiên học tiêu chuẩn |
| `PKG_LESSON_PERFECT` | v1 | 25 Gold + 50 Exp / phiên | Hoàn thành phiên học đạt 100% độ chính xác |
| `PKG_DAILY_QUEST_1` | v1 | 50 Gold + 1 Gem / quest | Hoàn thành nhiệm vụ ngày 1 |
| `PKG_BASE_FALLBACK` | v1 | 5 Gold + 10 Exp / phiên | Fallback khi cấu hình chính bị deprecate |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `VR-G01`: 100% giao dịch thưởng lưu trữ đúng `RewardConfigVersion` được áp dụng.
- `VR-G02`: Thay đổi giá trị Gold trong cấu hình mới không làm thay đổi số Gold đã cấp cho các sự kiện cũ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VR14-01` | Chốt phiên học tạo lúc 10:00 (khi gói v1 đang hiệu lực), Admin đổi gói v2 lúc 10:05 | Áp dụng đúng định mức thưởng của gói v1. |
| `VR14-02` | Gọi tính thưởng cho gói bị deprecate | Fallback về `PKG_BASE_FALLBACK`. |
| `VR14-03` | Kiểm thử hoàn tất luồng M06-VERSIONED-REWARD-CALCULATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-VR-F01` | Cần cache bảng `RewardConfigVersions` trong Redis | Giảm latency khi tính toán gói thưởng | M06-T015 |

## 5. Tự kiểm M06-T014
- Đã đặc tả thiết kế tính gói thưởng theo phiên bản M06-T014.
- Ghi nhận 2 Regression Gates (`VR-G01`–`VR-G02`) và 3 Test Cases (`VR14-01`–`VR14-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế tính gói thưởng theo phiên bản M06-T014 | WSA-7K2 |
