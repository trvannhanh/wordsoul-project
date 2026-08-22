# Xây dựng ma trận quyền điều chỉnh kinh tế M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ECONOMIC-MUTATION-AUTHORIZATION-1.0` |
| Task | M06-T038 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M06-VALUE-UNIT-CATALOG-1.0 (M06-T002), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Ma trận phân quyền điều chỉnh tài sản kinh tế (`Economic Mutation Matrix`), hạn mức cộng/trừ tối đa theo vai trò Admin và quy trình phê duyệt cấp cao |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả ma trận phân quyền điều chỉnh tài sản kinh tế (`Economic Mutation Authorization Matrix`) trong M06.

- **Giới hạn Hạn mức Điều chỉnh theo Vai trò (`Role-Based Asset Limit Invariant`)**:
  - *Customer Support Admin*: Được phép cộng/bù tối đa $500$ Gold / $50$ Exp cho người dùng. Thao tác vượt quá trần này BẮT BUỘC gửi yêu cầu phê duyệt cấp cao.
  - *Lead Economic Admin*: Được phép điều chỉnh tối đa $5,000$ Gold / $1,000$ Gems.
  - *System Owner*: Được quyền duyệt các giao dịch điều chỉnh đặc biệt vượt trần.
- **Ghi log Kiểm toán Điều chỉnh Bất biến (`Immutable Economic Audit Invariant`)**: 100% lệnh cộng/trừ thủ công BẮT BUỘC ghi vết `ManualAssetAdjustmentLogs` bao gồm `AdminUserId`, `TargetUserId`, `Amount`, `ReasonCode` và `ProofTicketId`.

## 2. Bảng Ma trận Phân quyền Điều chỉnh Tài sản (Authorization Matrix)

| Vai trò Admin | Hạn mức Gold / Lần | Hạn mức Gems / Lần | Yêu cầu Phê duyệt Cấp cao | Ghi log Kiểm toán M11 |
|---|---|---|---|---|
| `SUPPORT_ADMIN` | $\le 500$ Gold | $0$ Gems (Không được cấp) | Không (Duyệt tự động) | Bắt buộc |
| `LEAD_ADMIN` | $\le 5,000$ Gold | $\le 1,000$ Gems | Không (Duyệt tự động) | Bắt buộc |
| `SUPER_ADMIN` | $> 5,000$ Gold | $> 1,000$ Gems | Cần 2 Super Admin | Bắt buộc |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `EA-G01`: 100% request cộng Gold từ `SUPPORT_ADMIN` vượt mức 500 Gold bị chặn với HTTP 403.
- `EA-G02`: Mọi lệnh điều chỉnh tài sản thủ công tạo ra 1 dòng log kiểm toán trong `ManualAssetAdjustmentLogs`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EA38-01` | Support Admin bấm bù 1,000 Gold cho người dùng bị sự cố sập phiên | System từ chối, ném lỗi HTTP 403 `EXCEEDS_SUPPORT_ROLE_LIMIT`. |
| `EA38-02` | Lead Admin bù 2,000 Gold kèm mã vé hỗ trợ `TICKET_999` | Giao dịch thành công, số dư người dùng tăng 2,000 Gold, ghi log kiểm toán M11. |
| `EA38-03` | Kiểm thử hoàn tất luồng M06-ECONOMIC-MUTATION-AUTHORIZATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-EA-F01` | Áp dụng policy `[Authorize(Policy = "CanMutateAssetBalances")]` trên AdminAssetController | Kiểm soát an toàn API điều chỉnh | M06-T039 |

## 5. Tự kiểm M06-T038
- Đã hoàn thành đặc tả `M06-ECONOMIC-MUTATION-AUTHORIZATION-1.0`.
- Chốt ma trận 3 cấp hạn mức điều chỉnh kinh tế và log kiểm toán bất biến.
- Ghi nhận 2 Regression Gates (`EA-G01`–`EA-G02`) và 3 Test Cases (`EA38-01`–`EA38-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng ma trận quyền điều chỉnh kinh tế M06-T038 | WSA-7K2 |
