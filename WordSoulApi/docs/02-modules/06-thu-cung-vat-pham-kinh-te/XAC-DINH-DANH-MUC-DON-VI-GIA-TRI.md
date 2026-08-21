# Xác định danh mục đơn vị giá trị M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-VALUE-UNIT-CATALOG-1.0` |
| Task | M06-T002 |
| Đầu vào | M06-ASSET-ITEM-DICT-1.0 (M06-T001), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Danh mục phân loại các đơn vị tiền tệ/giá trị trong WordSoul (`Gold`, `Gems`, `Exp`), trần/sàn giá trị và chủ sở hữu module |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này xác định chi tiết các tham số quản lý đơn vị giá trị tài sản trong M06.

- **Tính Không Âm (`Non-Negative Balance Invariant`)**: Số dư của `Gold` và `Gems` tuyệt đối CẤM nhỏ hơn 0 ($Balance \ge 0$). Giao dịch tiêu dùng khiến số dư $< 0$ sẽ bị hệ thống từ chối ngay lập tục với lỗi `INSUFFICIENT_FUNDS`.
- **Trần Tích lũy An toàn (`Maximum Balance Ceiling Invariant`)**:
  - `Gold`: Maximum $99,999,999$
  - `Gems`: Maximum $9,999,999$
  - `Exp`: Maximum $999,999,999$
- **Ranh giới Chủ sở hữu (`Owner Boundary`)**: M06 là chủ sở hữu duy nhất giữ quyền ghi (Write Access) đối với số dư các đơn vị giá trị. Mọi module khác (M03, M07, M08) chỉ có quyền đọc hoặc phát sự kiện yêu cầu.

## 2. Bảng Danh mục Đơn vị Giá trị (Value Unit Catalog)

| Mã Đơn vị | Tên Đơn vị | Tiêu dùng | Nguồn vào chính | Nguồn ra chính | Hạn mức Ngày (REL-04) |
|---|---|---|---|---|---|
| `GOLD` | Vàng | Có | Hoàn thành phiên M03, Nhiệm vụ M07 | Mua vật phẩm M06 | $5,000$ Gold / ngày |
| `GEMS` | Kim cương | Có | Chuỗi học Streak, Thành tựu | Mua vật phẩm cao cấp | Không áp dụng |
| `EXP` | Điểm kinh nghiệm | Không | Học tập M03, Ôn tập M04 | Tăng level người dùng | $5,000$ Exp / ngày |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `VU-G01`: 100% giao dịch làm số dư Gold/Gems $< 0$ bị chối bỏ với lỗi `INSUFFICIENT_FUNDS`.
- `VU-G02`: Số dư vượt trần tối đa được kẹp tại giá trị ceiling an toàn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VU02-01` | Mua vật phẩm 100 Gold khi có số dư 50 Gold | System ném exception `INSUFFICIENT_FUNDS`, không trừ tiền. |
| `VU02-02` | Cấp thưởng Exp khiến tổng điểm vượt 999,999,999 | Kẹp số dư ở mức maximum 999,999,999. |
| `VU02-03` | Kiểm thử hoàn tất luồng M06-VALUE-UNIT-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-VU-F01` | Cần cấu hình tham số `MaxGoldCeiling` trong `appsettings.json` | Đảm bảo cấu hình quản trị M11 tập trung | M06-T003 |

## 5. Tự kiểm M06-T002
- Đã xác định danh mục đơn vị giá trị M06-T002.
- Ghi nhận 2 Regression Gates (`VU-G01`–`VU-G02`) và 3 Test Cases (`VU02-01`–`VU02-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xác định danh mục đơn vị giá trị M06-T002 | WSA-7K2 |
