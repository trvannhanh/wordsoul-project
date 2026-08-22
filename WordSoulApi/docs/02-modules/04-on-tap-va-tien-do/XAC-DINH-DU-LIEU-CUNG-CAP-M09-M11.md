# Xác định dữ liệu cung cấp M09/M11 M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-DATA-EXPORT-M09-M11-1.0` |
| Task | M04-T035 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M04-USER-PROGRESS-METRICS-1.0 (M04-T032), M04-ACTIVITY-RETENTION-TRENDS-1.0 (M04-T034) |
| Phạm vi | Ranh giới chia sẻ dữ liệu tiến độ từ M04 sang M09 (Leaderboards/Social) và M11 (Admin Analytics), bảo đảm nguyên tắc bảo mật thông tin cá nhân và dữ liệu tối thiểu |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc xuất dữ liệu tiến độ (`Data Export Contract`) từ M04 sang các module M09 và M11.

- **Dữ liệu Tối thiểu cho Module Xã hội M09 (`Minimal Exposure for M09 Invariant`)**:
  - M09 CHỈ ĐƯỢC PHÉP tiếp nhận các chỉ số tổng hợp công khai (`TotalMasteredCount`, `StreakDays`).
  - Tuyệt đối CẤM truyền chi tiết nhật ký ôn tập cá nhân hay danh sách các nét nghĩa từ vựng cá nhân sang M09.
- **Quyền Truy vấn Quản trị M11 (`Admin Audit Query Invariant`)**: M11 tiếp nhận bộ chỉ số chẩn đoán hệ thống thô để phục vụ giám sát thuật toán SRS, nhưng BẮT BUỘC có cờ kiểm toán `IsAdminAuditExport = true`.

## 2. Bảng Ranh giới Chia sẻ Dữ liệu (Data Sharing Boundary Matrix)

| Module Đích | Tập Dữ liệu Cung cấp | Định dạng Transfer | Mục đích Sử dụng |
|---|---|---|---|
| **M09 (Social)** | `MasteredSensesCount`, `StreakDays` | `UserProgressPublicSummaryEvent` | Bảng xếp hạng, Badge xã hội |
| **M11 (Analytics)** | `RetentionScoreStats`, `EaseFactorDistribution` | `SrsSystemHealthReportEvent` | Giám sát hiệu quả SRS toàn hệ thống |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `DE-G01`: 100% payload gửi sang M09 không chứa thuộc tính `UserSenseProgressLog` chi tiết.
- `DE-G02`: Request truy xuất từ M11 chứa đầy đủ mã cờ kiểm toán `IsAdminAuditExport`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DE35-01` | M04 phát sự kiện đồng bộ dữ liệu sang M09 cho Learner A | Event chỉ chứa `UserId`, `MasteredSensesCount`, `StreakDays`. Không có nhật ký ôn chi tiết. |
| `DE35-02` | M11 gọi API lấy báo cáo phân bố EaseFactor toàn hệ thống | API trả về dữ liệu phân bố $EF$ được ẩn danh hóa. |
| `DE35-03` | Kiểm thử hoàn tất luồng M04-DATA-EXPORT-M09-M11-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-DE-F01` | Phát `UserProgressPublicSummaryEvent` khi người dùng thay đổi số từ thuộc | Phục vụ cập nhật Bảng xếp hạng M09 | M09-T001 |

## 5. Tự kiểm M04-T035
- Đã hoàn thành đặc tả `M04-DATA-EXPORT-M09-M11-1.0`.
- Chốt ranh giới dữ liệu tối thiểu cho M09 và kiểm toán cho M11.
- Ghi nhận 2 Regression Gates (`DE-G01`–`DE-G02`) và 3 Test Cases (`DE35-01`–`DE35-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định dữ liệu cung cấp M09/M11 M04-T035 | WSA-7K2 |
