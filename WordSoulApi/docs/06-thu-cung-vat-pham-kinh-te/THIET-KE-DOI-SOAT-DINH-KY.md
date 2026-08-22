# Thiết kế đối soát định kỳ M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-PERIODIC-ECONOMIC-RECONCILIATION-1.0` |
| Task | M06-T040 |
| Đầu vào | M06-DERIVED-BALANCE-RECONCILIATION-1.0 (M06-T004), M06-ITEM-INVENTORY-OWNERSHIP-1.0 (M06-T034), M06-PREVIEWABLE-ECONOMIC-ADJUSTMENT-1.0 (M06-T039), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Tiến trình chạy ngầm đối soát định kỳ toàn bộ sổ cái và số dư tài sản (`Periodic Reconciliation Engine`), tự động phát hiện sai lệch và khóa giao dịch nghi vấn |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình và tiến trình ngầm đối soát định kỳ tài sản kinh tế (`Periodic Reconciliation Engine`) trong M06.

- **Đối soát Số dư Dẫn xuất Toàn diện (`Complete Ledger Balance Equivalence Invariant`)**:
  - Tiến trình ngầm `AssetReconciliationWorker` chạy vào 02:00 UTC hàng ngày:
    - Kiểm tra công thức $Balance_{current} == \sum \Delta Amount_{ledger}$ cho $100\%$ tài khoản active.
    - So sánh tổng kho vật phẩm `UserInventories` với nhật ký biến động vật phẩm `ItemUsageLogs`.
- **Khóa Giao dịch Tạm thời khi Phát hiện Chênh lệch (`Mismatch Freeze Rule`)**:
  - Nếu phát hiện chênh lệch $Balance_{current} \ne \sum \Delta Amount$:
    - Tự động gắn cờ `IsFrozenForAudit = true` cho tài khoản bị lệch.
    - Bắn cảnh báo `CRITICAL_ASSET_MISMATCH_ALERT` sang M11. Tuyệt đối CẤM tự ý sửa số dư thô khi chưa có lệnh điều chỉnh đã duyệt.

## 2. Quy trình Thực hiện Đối soát Định kỳ (Periodic Reconciliation Workflow)

```mermaid
graph TD
    Cron[Daily 02:00 UTC Worker] --> ScanUsers[Scan Active User Accounts]
    ScanUsers --> CheckGold{Gold Balance == Sum(Ledger Gold)?}
    CheckGold -->|Mismatch| Freeze[Set IsFrozenForAudit = true & Raise Alert]
    CheckGold -->|Match| CheckGems{Gems Balance == Sum(Ledger Gems)?}
    CheckGems -->|Mismatch| Freeze
    CheckGems -->|Match| Pass[Pass Reconciliation & Write Log]
    Freeze --> NotifyAdmin[Send Alert to M11 Audit Queue]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PR-G01`: 100% tài khoản có chênh lệch số dư tự động bị gắn cờ `IsFrozenForAudit = true`.
- `PR-G02`: Tiến trình đối soát không làm gián đoạn các giao dịch đọc số dư của người dùng hợp lệ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PR40-01` | Giả lập sửa tay số dư Gold của Learner A từ 1,000 lên 1,500 (sổ cái chỉ ghi tổng 1,000) | Worker quét lúc 02:00 UTC phát hiện chênh 500 Gold, khóa tài khoản `IsFrozenForAudit = true`, bắn cảnh báo M11. |
| `PR40-02` | Tài khoản Learner B có số dư và sổ cái khớp $100\%$ | Worker ghi nhận `Status = PASSED` trong `ReconciliationLogs`. |
| `PR40-03` | Kiểm thử hoàn tất luồng M06-PERIODIC-ECONOMIC-RECONCILIATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-PR-F01` | Tạo cờ `IsFrozenForAudit` trong thực thể `UserAssetBalance` | Chặn các giao dịch chi tiêu Gold/Gems khi đang bị khóa đối soát | M06-T004 |

## 5. Tự kiểm M06-T040
- Đã hoàn thành đặc tả `M06-PERIODIC-ECONOMIC-RECONCILIATION-1.0`.
- Chốt tiến trình ngầm 02:00 UTC, cờ khóa `IsFrozenForAudit` và cấm tự sửa số dư không qua duyệt.
- Ghi nhận 2 Regression Gates (`PR-G01`–`PR-G02`) và 3 Test Cases (`PR40-01`–`PR40-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế đối soát định kỳ M06-T040 | WSA-7K2 |
