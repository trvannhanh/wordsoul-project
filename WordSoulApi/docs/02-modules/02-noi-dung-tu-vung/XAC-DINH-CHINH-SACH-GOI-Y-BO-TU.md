# Xác định chính sách gợi ý bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-VOCAB-SET-RECOMMENDATION-1.0` |
| Task | M02-T037 |
| Đầu vào | M01-USER-PROFILE-1.0 (M01-T003), M02-SET-STATUS-1.0 (M02-T032), M04-PROGRESS, M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Tiêu chí gợi ý bộ từ vựng xuất bản (`PUBLISHED`), minh bạch lý do gợi ý (`RecommendationReason`), loại trừ bộ không hợp lệ và phương án Cold Start cho người dùng mới |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chính sách và thuật toán gợi ý bộ từ vựng (`Set Recommendation Policy`) cho người học trên trang khám phá M02.

- **Tôn trọng Quyền và Trạng thái Xuất bản (`Eligibility & Status Invariant`)**:
  - CHỈ gợi ý các bộ từ ở trạng thái `PUBLISHED` và `IsPublic == true` (hoặc bộ được cấp quyền riêng).
  - Tuyệt đối CẤM gợi ý các bộ từ ở trạng thái `DRAFT`, `ARCHIVED`, `RECALLED` hoặc các bộ đã có trong thư viện cá nhân của người học (`UserLibrarySets`).
- **Minh bạch Lý do Gợi ý (`Recommendation Transparency Invariant`)**:
  - 100% item gợi ý trả về BẮT BUỘC đi kèm mã và văn bản `RecommendationReason` (ví dụ: `BASED_ON_GOAL`, `POPULAR_IN_LEVEL`, `NEW_RELEASE`).
- **Phương án Cold Start Cho Người dùng Mới (`New User Fallback`)**: Người dùng mới tạo tài khoản chưa có lịch sử học sẽ mặc định nhận tập gợi ý bộ từ phổ biến nhất theo trình độ mục tiêu chọn khi đăng ký (`TargetCefrLevel`).

## 2. Ma trận Loại Lý do Gợi ý (Recommendation Reason Matrix)

| Mã Lý do | Tiêu chí gợi ý | Ví dụ hiển thị |
|---|---|---|
| `MATCH_CEFR` | Phù hợp trình độ CEFR của người dùng | "Phù hợp với trình độ B1 của bạn" |
| `BASED_ON_GOAL` | Trùng với mục đích học tập chọn khi đăng ký | "Dành cho mục tiêu Luyện thi TOEIC" |
| `POPULAR_IN_CATEGORY` | Top lượt thêm thư viện trong danh mục | "Bộ từ hot nhất tuần này" |
| `NEW_RELEASE` | Bộ từ mới phát hành trong 14 ngày | "Bộ từ vựng mới ra mắt" |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `VR-G01`: 100% bộ từ đã nằm trong thư viện cá nhân của người dùng bị loại khỏi danh sách gợi ý.
- `VR-G02`: API trả về kết quả gợi ý luôn chứa trường `RecommendationReason` hợp lệ.
- `VR-G03`: Người dùng mới chưa có tiến độ học tập nhận danh sách gợi ý mặc định theo trình độ mục tiêu mà không bị lỗi.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VR37-01` | Người dùng trình độ B1 gọi API lấy gợi ý | Trả về 5 bộ từ B1 ở trạng thái `PUBLISHED` chưa có trong thư viện. |
| `VR37-02` | Người dùng đã thêm bộ từ "IELTS Core" vào thư viện | Bộ "IELTS Core" biến mất khỏi danh sách gợi ý. |
| `VR37-03` | Người dùng mới tạo tài khoản gọi API gợi ý | Trả về danh sách bộ từ phổ biến kèm `ReasonCode = "NEW_USER_COLD_START"`. |
| `VR37-04` | Kiểm thử hoàn tất luồng M02-VOCAB-SET-RECOMMENDATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-VR-F01` | Tích hợp cache Redis 15 phút cho danh sách `PopularSets` | Đảm bảo hiệu năng cho Cold Start fallback | M02-T038 |

## 5. Tự kiểm M02-T037
- Đã hoàn thành đặc tả `M02-VOCAB-SET-RECOMMENDATION-1.0`.
- Chốt nguyên tắc loại trừ bộ đã có trong thư viện và minh bạch lý do gợi ý.
- Ghi nhận 3 Regression Gates (`VR-G01`–`VR-G03`) và 4 Test Cases (`VR37-01`–`VR37-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định chính sách gợi ý bộ từ M02-T037 | WSA-7K2 |
