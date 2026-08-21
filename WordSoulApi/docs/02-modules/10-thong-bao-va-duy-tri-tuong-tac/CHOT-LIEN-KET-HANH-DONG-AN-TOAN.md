# Chốt liên kết hành động an toàn M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-SAFE-ACTION-DEEPLINK-1.0` |
| Task | M10-T015 |
| Đầu vào | M10-NOTIFICATION-TEMPLATE-SPEC-1.0 (M10-T011), M01-SECURITY-1.0 (M01-T028) |
| Phạm vi | Quy chuẩn định dạng Deeplink điều hướng an toàn (`ActionDeepLink`) đính kèm trong thông báo, chống lỗ hổng Open Redirect |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định tiêu chuẩn bảo mật cho các liên kết điều hướng khi người học tương tác với thông báo.

- **White-list Schema Deeplink Nội bộ (`Internal Deeplink Whitelist Invariant`)**: Thuộc tính `ActionDeepLink` BẮT BUỘC bắt đầu bằng URI scheme chính thức của ứng dụng `wordsoul://` hoặc đường dẫn tuyệt đối nội bộ. CẤM tuyệt đối việc chứa các URL bên ngoài (`http://`, `https://`) để chặn tấn công phishing/open-redirect.
- **Xử lý Fallback khi Deeplink Không Tồn tại (`Invalid Deeplink Fallback Invariant`)**: Nếu màn hình đích trong Deeplink bị gỡ bỏ hoặc không hợp lệ, ứng dụng di động BẮT BUỘC fallback chuyển hướng về Màn hình Chính (`HomeScreen`).

## 2. Dynamic Safe Deeplink Whitelist Table

| Schema / Route | Màn hình Đích trên App | Ngữ cảnh sử dụng |
|---|---|---|
| `wordsoul://learning/review` | Màn hình Hàng đợi Ôn tập SRS (M04) | Nhắc nhở từ vựng đến hạn |
| `wordsoul://quests/daily` | Màn hình Nhiệm vụ Ngày (M07) | Cảnh báo nhiệm vụ sắp hết hạn |
| `wordsoul://inventory/shop` | Màn hình Cửa hàng Vật phẩm (M06) | Thưởng vật phẩm/Gems |
| `wordsoul://home` | Màn hình Trang chủ (HomeScreen) | Fallback mặc định an toàn |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SD-G01`: 100% request chèn thông báo có `ActionDeepLink` chứa `http://` hoặc `https://` bị từ chối với lỗi HTTP 400 `EXTERNAL_URL_NOT_ALLOWED_IN_DEEPLINK`.
- `SD-G02`: Nhấn vào thông báo có Deeplink bị lỗi được chuyển hướng an toàn về HomeScreen.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SD15-01` | Tạo thông báo chứa Deeplink `wordsoul://learning/review` | Thành công, Deeplink hợp lệ. |
| `SD15-02` | Thử chèn thông báo chứa `https://phishing-site.com` | System reject với lỗi `EXTERNAL_URL_NOT_ALLOWED_IN_DEEPLINK`. |
| `SD15-03` | Kiểm thử hoàn tất luồng M10-SAFE-ACTION-DEEPLINK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-SD-F01` | Cần Validator `DeeplinkSecurityValidator` trong Domain M10 | Xác thực độ an toàn của Deeplink trước khi lưu | M10-T016 |

## 5. Tự kiểm M10-T015
- Đã đặc tả chốt liên kết hành động an toàn M10-T015.
- Ghi nhận 2 Regression Gates (`SD-G01`–`SD-G02`) và 3 Test Cases (`SD15-01`–`SD15-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt liên kết hành động an toàn M10-T015 | WSA-7K2 |
