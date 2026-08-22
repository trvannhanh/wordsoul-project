# Đặc tả bảng điều hành theo vai trò M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-ROLE-BASED-DASHBOARD-SPEC-1.0` |
| Task | M11-T025 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M11-REALTIME-FRESHNESS-1.0 (M11-T023), M11-DATA-QUALITY-CONTROL-1.0 (M11-T024) |
| Phạm vi | Kiến trúc giao diện Dashboard Admin theo từng vai trò (`Role-Based Executive Dashboard`), ẩn danh dữ liệu cá nhân (PII) và giới hạn góc nhìn |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả giao diện và phân quyền truy cập Dashboard điều hành (`Role-Based Dashboard Architecture`) trong M11.

- **Bảo vệ PII và Giới hạn Quyền Xem theo Vai trò (`Dashboard RBAC Invariant`)**:
  - *Academic Content Admin*: CHỈ ĐƯỢC XEM chỉ số hoàn thành phiên học, chất lượng bộ từ (M02/M03/M04). Ẩn toàn bộ thông tin tài sản kinh tế và PII người dùng.
  - *Economy & Support Admin*: Được xem chỉ số dòng tiền, số dư tài sản (M06). Địa chỉ Email/Số điện thoại BẮT BUỘC bị che dạng `u***r@email.com`.
  - *Executive Board*: CHỈ ĐƯỢC XEM báo cáo tổng hợp cấp cao (DAU, MAU, Total Revenue), tuyệt đối CẤM truy xuất log giao dịch chi tiết của cá nhân.
- **Minh bạch SLA Độ mới Dữ liệu (`Data Freshness SLA Visibility Rule`)**: Tất cả widget trên Dashboard BẮT BUỘC hiển thị mốc `LastRefreshedAtUtc` và nhãn SLA tương ứng (Realtime 10s / Batch 15m).

## 2. Ma trận Hiển thị Widget Dashboard theo Vai trò (Widget Authorization Matrix)

| Tên Widget Metric | Role: Content Admin | Role: Economy Admin | Role: Executive Board |
|---|---|---|---|
| `LessonCompletionRate` | **Hiển thị** | Ẩn | **Hiển thị (Tổng hợp)** |
| `NetEconomicFlow` | Ẩn | **Hiển thị** | **Hiển thị (Tổng hợp)** |
| `UserSenseProgressLogs` | Ẩn | Ẩn | Ẩn |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RD-G01`: 100% request API lấy widget Dashboard của Content Admin từ chối dữ liệu kinh tế/PII.
- `RD-G02`: Mọi widget trên Dashboard hiển thị đúng $100\%$ thuộc tính `LastRefreshedAtUtc`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RD25-01` | Content Admin đăng nhập Dashboard | Giao diện chỉ hiển thị 2 tab "Chất lượng Học liệu" và "Tiến độ SRS", không có tab "Kinh tế". |
| `RD25-02` | Economy Admin tra cứu danh sách biến động tài sản | Cột Email người dùng hiển thị che PII: `j***n@gmail.com`. |
| `RD25-03` | Kiểm thử hoàn tất luồng M11-ROLE-BASED-DASHBOARD-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-RD-F01` | Thiết kế DTO Envelope `RoleFilteredDashboardResponse` | Phục vụ filter widget theo vai trò JWT | M11-T004 |

## 5. Tự kiểm M11-T025
- Đã hoàn thành đặc tả `M11-ROLE-BASED-DASHBOARD-SPEC-1.0`.
- Chốt ma trận hiển thị widget theo vai trò và bảo vệ PII triệt để.
- Ghi nhận 2 Regression Gates (`RD-G01`–`RD-G02`) và 3 Test Cases (`RD25-01`–`RD25-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả bảng điều hành theo vai trò M11-T025 | WSA-7K2 |
