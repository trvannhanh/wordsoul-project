# Xây dựng tiêu chí đo hiệu quả khám phá M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-DISCOVERY-EFFECTIVENESS-METRICS-1.0` |
| Task | M02-T038 |
| Đầu vào | M02-SEARCH-HEADER-SET-1.0 (M02-T035), M02-CATEGORY-BROWSE-1.0 (M02-T036), M02-VOCAB-SET-RECOMMENDATION-1.0 (M02-T037), M11-REALTIME-FRESHNESS-1.0 (M11-T023) |
| Phạm vi | Chỉ số hiệu quả khám phá (`Discovery Metrics`), tỷ lệ thêm bộ vào thư viện (`Add-to-Library Rate`), tỷ lệ khởi tạo phiên học (`Session-Start Rate`) và phân đoạn nguồn khám phá |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này định nghĩa các chỉ số đo lường hiệu quả khám phá (`Discovery Effectiveness Metrics`) đối với nội dung học liệu M02.

- **Không Nhầm Lượt Xem với Chất lượng Khám phá (`Conversion Over Impression Invariant`)**:
  - Lượt xem danh sách bộ từ (`Impression`) KHÔNG ĐƯỢC COI LÀ thành công khám phá. Chỉ số khám phá hiệu quả BẮT BUỘC đo bằng hành vi chuyển đổi: Thêm bộ vào thư viện (`LibraryAddConversion`) và Khởi tạo phiên học đầu tiên từ bộ đó (`SessionStartConversion`).
- **Bắt buộc Phân đoạn theo Nguồn Khám phá (`Source Attribution Invariant`)**: 100% sự kiện tương tác khám phá BẮT BUỘC lưu trữ mã nguồn `DiscoverySource` (`SEARCH`, `CATEGORY_BROWSE`, `RECOMMENDED`, `DIRECT_LINK`).

## 2. Bảng Danh mục Chỉ số Đo hiệu quả Khám phá (Discovery Metrics Catalog)

| Mã Chỉ số | Tên Chỉ số | Công thức tính | Mục tiêu tối thiểu | Tác động quản trị M11 |
|---|---|---|---|---|
| `CTR_LIB_ADD` | Tỷ lệ Thêm Thư viện | $\frac{\text{Số lượt Thêm bộ vào Thư viện}}{\text{Số lượt Xem Chi tiết Bộ từ}} \times 100\%$ | $\ge 20\%$ | Đánh giá độ hấp dẫn bộ từ vựng |
| `CTR_SESS_START` | Tỷ lệ Khởi tạo Phiên | $\frac{\text{Số lượt Bắt đầu Học}}{\text{Số lượt Thêm bộ vào Thư viện}} \times 100\%$ | $\ge 60\%$ | Đánh giá chất lượng gợi ý |
| `DISCOVERY_BOUNCE` | Tỷ lệ Xem xong Bỏ đi | $\frac{\text{Số lượt Xem dưới 3 giây rồi thoát}}{\text{Tổng lượt Xem Chi tiết}} \times 100\%$ | $\le 15\%$ | Cảnh báo nội dung sai tiêu đề |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `DE-G01`: 100% sự kiện xem bộ từ gửi về M11 không được tính vào chỉ số chuyển đổi nếu người dùng không bấm Thêm thư viện hoặc Bắt đầu học.
- `DE-G02`: Sự kiện tương tác khám phá thiếu `DiscoverySource` bị gắn nhãn fallback `UNKNOWN_SOURCE` để đối soát.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DE38-01` | Người dùng tìm kiếm từ khóa `"TOEIC"`, xem chi tiết bộ 5s rồi bấm Thêm thư viện | Ghi nhận sự kiện `LIB_ADD` với `DiscoverySource = "SEARCH"`. |
| `DE38-02` | Người dùng lướt danh mục nhưng không bấm vào bộ từ nào | Ghi nhận 1 `CategoryImpression`, không tăng `CTR_LIB_ADD`. |
| `DE38-03` | Kiểm thử hoàn tất luồng M02-DISCOVERY-EFFECTIVENESS-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-DE-F01` | Đăng ký job tổng hợp chỉ số khám phá theo giờ trong M11 | Đảm bảo tính toán báo cáo chuyển đổi không làm chậm API M02 | M11-T023 |

## 5. Tự kiểm M02-T038
- Đã hoàn thành đặc tả `M02-DISCOVERY-EFFECTIVENESS-METRICS-1.0`.
- Chốt danh mục 3 chỉ số chuyển đổi chính và phân đoạn nguồn khám phá.
- Ghi nhận 2 Regression Gates (`DE-G01`–`DE-G02`) và 3 Test Cases (`DE38-01`–`DE38-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả tiêu chí đo hiệu quả khám phá M02-T038 | WSA-7K2 |
