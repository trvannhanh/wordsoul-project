# Chuẩn hóa danh mục tham số chính sách M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-POLICY-PARAMETER-CATALOG-1.0` |
| Task | M04-T039 |
| Đầu vào | M04-MEMORY-DICT-1.0 (M04-T001), M04-SRS-INTERVAL-CALCULATION-1.0 (M04-T016), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Kiểm kê toàn bộ tham số cấu hình thuật toán SRS, giá trị mặc định, giới hạn an toàn và phân quyền chỉnh sửa M11 |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này kiểm kê và đóng băng danh mục toàn bộ tham số điều phối chính sách ghi nhớ SRS trong M04.

- **Giới hạn An toàn Tham số (`Parameter Boundary Invariant`)**:
  - Mọi thay đổi cấu hình tham số SRS từ trang quản trị M11 BẮT BUỘC nằm trong khoảng giới hạn an toàn (`MinAllowed` $\le$ `Value` $\le$ `MaxAllowed`). Cấu hình nằm ngoài khoảng an toàn sẽ bị hệ thống từ chối áp dụng.
- **Tính Bất biến Tham số Hồi tố (`No Retroactive Recalculation Invariant`)**:
  - Thay đổi giá trị tham số chính sách CHỈ ÁP DỤNG cho các lần ôn tập phát sinh từ thời điểm thay đổi về sau. Tuyệt đối CẤM tự động tính toán lại lịch sử $Interval$ của các bản ghi cũ.

## 2. Bảng Danh mục Tham số Chính sách SRS (Policy Parameter Catalog)

| Mã Tham số | Tên Tham số | Đơn vị | Giá trị Mặc định | Khoảng Cho phép [Min, Max] | Mô tả tác động |
|---|---|---|---|---|---|
| `SrsInitialIntervalDays` | Khoảng ôn ban đầu | Ngày | 1 | [1, 3] | Khoảng thời gian từ lúc học mới tới lần ôn 1 |
| `SrsSecondIntervalDays` | Khoảng ôn lần 2 | Ngày | 6 | [3, 10] | Khoảng thời gian từ lần ôn 1 tới lần ôn 2 |
| `SrsDefaultEaseFactor` | Hệ số EF mặc định | Số thực | 2.50 | [1.50, 2.50] | Hệ số dễ nhớ ban đầu cho từ mới |
| `SrsMinEaseFactor` | Hệ số EF tối thiểu | Số thực | 1.30 | [1.10, 1.50] | Sàn kẹp $EF$ không cho giảm quá sâu |
| `SrsMaxIntervalDays` | Khoảng ôn tối đa | Ngày | 365 | [90, 730] | Trần kẹp khoảng ôn tập dài hạn |
| `SrsMasteredIntervalThreshold` | Ngưỡng đạt Mastered | Ngày | 21 | [14, 60] | Số ngày kẹp để chuyển trạng thái `MASTERED` |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PP-G01`: 100% request thay đổi cấu hình SRS vi phạm kẹp [Min, Max] bị chối bỏ với lỗi `PARAMETER_OUT_OF_BOUNDS`.
- `PP-G02`: Thay đổi tham số `SrsDefaultEaseFactor` không làm thay đổi $EF$ của các từ vựng đã khởi tạo từ trước.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PP39-01` | Admin thử đổi `SrsMinEaseFactor = 0.50` (dưới min 1.10) | System ném lỗi HTTP 400 `PARAMETER_OUT_OF_BOUNDS`. |
| `PP39-02` | Admin đổi `SrsInitialIntervalDays = 2` | Các từ vựng mới học sau thời điểm này nhận $Interval = 2$ ngày. Các từ cũ giữ nguyên. |
| `PP39-03` | Kiểm thử hoàn tất luồng M04-POLICY-PARAMETER-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PP-F01` | Đăng ký các key tham số `Srs.*` trong `SystemConfigs` M11 | Cho phép quản trị viên điều chỉnh qua UI | M11-T012 |

## 5. Tự kiểm M04-T039
- Đã hoàn thành đặc tả `M04-POLICY-PARAMETER-CATALOG-1.0`.
- Kiểm kê 6 tham số cố định và kẹp khoảng an toàn.
- Ghi nhận 2 Regression Gates (`PP-G01`–`PP-G02`) và 3 Test Cases (`PP39-01`–`PP39-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa danh mục tham số chính sách M04-T039 | WSA-7K2 |
