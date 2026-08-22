# Xác định chỉ số nguồn vào và nguồn ra M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-INFLOW-OUTFLOW-METRICS-1.0` |
| Task | M06-T042 |
| Đầu vào | M06-ASSET-LEDGER-MODEL-1.0 (M06-T003), M06-DERIVED-BALANCE-RECONCILIATION-1.0 (M06-T004) |
| Phạm vi | Bộ chỉ số đo lường tổng dòng tiền vào (`Asset Inflow Aggregate`) và dòng tiền ra (`Asset Outflow Aggregate`) của toàn bộ hệ thống kinh tế M06 |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa bộ chỉ số đo lường dòng vào/ra (`Inflow & Outflow Aggregate Metrics`) của nền kinh tế M06.

- **Định luật Bảo toàn Dòng tiền Hệ thống (`Economic Flow Conservation Invariant`)**:
  - Đối với từng loại tài sản (Gold, Gems, Exp):
    $$\Delta NetSystemBalance = \sum Inflow - \sum Outflow$$
  - Tổng thay đổi số dư tài sản toàn hệ thống BẮT BUỘC khớp $100\%$ với hiệu số giữa tổng phát thưởng/cấp bù ($Inflow$) và tổng chi tiêu/thu hồi ($Outflow$).
- **Phân đoạn Nguồn phát sinh Chi tiết (`Categorized Source Inflow Invariant`)**: Bộ chỉ số BẮT BUỘC phân đoạn dòng tiền theo từng nguồn (Thưởng bài học M03, Thưởng nhiệm vụ M07, Mua vật phẩm M06).

## 2. Bảng Danh mục Chỉ số Nguồn vào & Nguồn ra (Economic Flow Metrics Catalog)

| Mã Chỉ số | Thuộc tính Dòng | Công thức Tính | Mục đích Báo cáo |
|---|---|---|---|
| `TOTAL_GOLD_INFLOW` | Inflow | $\sum \Delta Amount \text{ where } Type == CREDIT$ | Đo tốc độ bơm Gold vào game |
| `TOTAL_GOLD_OUTFLOW` | Outflow | $\sum |\Delta Amount| \text{ where } Type == DEBIT$ | Đo tốc độ tiêu thụ Gold |
| `NET_GOLD_CIRCULATION` | Net | $TOTAL\_GOLD\_INFLOW - TOTAL\_GOLD\_OUTFLOW$ | Lạm phát / Giảm phát Gold |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IO-G01`: 100% kết quả báo cáo dòng tiền khớp $100\%$ công thức `NetBalance = Inflow - Outflow`.
- `IO-G02`: Báo cáo phân đoạn đúng $100\%$ theo mã module nguồn (`SourceModule`).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IO42-01` | Trong ngày hệ thống phát 100,000 Gold thưởng, người dùng chi 40,000 Gold mua vật phẩm | Trả về `Inflow = 100,000`, `Outflow = 40,000`, `NetCirculation = +60,000 Gold`. |
| `IO42-02` | Admin xem báo cáo dòng tiền Gems 30 ngày | API trả về biểu đồ dòng nạp vs tiêu Gems chi tiết theo từng ngày. |
| `IO42-03` | Kiểm thử hoàn tất luồng M06-INFLOW-OUTFLOW-METRICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-IO-F01` | Đưa `EconomicFlowMetricsDto` vào Admin Dashboard M11 | Phục vụ giám sát chỉ số lạm phát kinh tế | M11-T012 |

## 5. Tự kiểm M06-T042
- Đã hoàn thành đặc tả `M06-INFLOW-OUTFLOW-METRICS-1.0`.
- Chốt công thức bảo toàn dòng tiền hệ thống và danh mục chỉ số bơm/rút tài sản.
- Ghi nhận 2 Regression Gates (`IO-G01`–`IO-G02`) và 3 Test Cases (`IO42-01`–`IO42-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định chỉ số nguồn vào và nguồn ra M06-T042 | WSA-7K2 |
