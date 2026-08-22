# Lập kế hoạch chuyển dữ liệu kinh tế hiện có M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-LEGACY-ECONOMIC-DATA-MIGRATION-1.0` |
| Task | M06-T005 |
| Đầu vào | M06-ASSET-LEDGER-MODEL-1.0 (M06-T003), M06-DERIVED-BALANCE-RECONCILIATION-1.0 (M06-T004), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Kế hoạch di chuyển dữ liệu số dư kinh tế cũ (`Legacy Asset Migration Plan`), sinh bản ghi mở đầu sổ cái (`INITIAL_MIGRATION`) và kịch bản khôi phục (Rollback Plan) |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chiến lược di chuyển dữ liệu số dư tài sản từ hệ thống cũ (`Legacy Asset Balances`) sang mô hình sổ cái mới M06.

- **Bản ghi Mở đầu Sổ cái Bất biến (`Initial Ledger Migration Entry Invariant`)**:
  - Với mỗi tài khoản người dùng có số dư cũ $> 0$, script di chuyển BẮT BUỘC sinh đúng 1 bản ghi mở đầu sổ cái:
    - `TransactionType = INITIAL_MIGRATION`
    - `Amount = LegacyBalance`
    - `ReferenceEventId = MIGRATION_{UserId}_{AssetCode}`
- **Bảo toàn Tổng Tài sản trước và sau Di chuyển (`Zero Net Loss Migration Invariant`)**:
  - Tổng số dư tài sản trong toàn bộ hệ thống sau di chuyển BẮT BUỘC bằng chính xác tổng số dư cũ:
    $$\sum Balance_{new} = \sum Balance_{legacy}$$
  - Nếu phát sinh bất kỳ chênh lệch nào ($\sum Balance_{new} \neq \sum Balance_{legacy}$), script BẮT BUỘC tự động rollback toàn bộ transaction di chuyển.

## 2. Quy trình Di chuyển Dữ liệu Kinh tế (Migration Execution Flow)

```mermaid
graph TD
    Start[Start Migration Script] --> ReadLegacy[Read Legacy Balances Table]
    ReadLegacy --> BeginTx[Begin Master DB Transaction]
    BeginTx --> GenLedger[Insert INITIAL_MIGRATION Ledger Entries]
    GenLedger --> GenBalance[Populate UserAssetBalance Summary Rows]
    GenBalance --> AuditSum{Check Sum(Legacy) == Sum(New)?}
    AuditSum -->|Matches 100%| CommitTx[Commit Migration Transaction]
    AuditSum -->|Mismatch| RollbackTx[Rollback & Output Exception Report]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `EM-G01`: 100% tài khoản có số dư cũ nhận được bản ghi `INITIAL_MIGRATION` khớp chính xác giá trị trong sổ cái mới.
- `EM-G02`: Báo cáo đối soát sau di chuyển ghi nhận `TotalDiscrepancy = 0`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EM05-01` | Chạy migration cho 10,000 tài khoản người dùng cũ | 10,000 bản ghi `INITIAL_MIGRATION` được chèn vào DB, tổng số dư hệ thống khớp 100%. |
| `EM05-02` | Thử nạp dữ liệu lỗi có 1 tài khoản mang số dư âm | Script ngắt tiến trình, ném exception `INVALID_LEGACY_NEGATIVE_BALANCE` và rollback. |
| `EM05-03` | Kiểm thử hoàn tất luồng M06-LEGACY-ECONOMIC-DATA-MIGRATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-EM-F01` | Tạo SQL Migration script `V6.0__MigrateLegacyAssetBalances.sql` trong DbUp/EF Migrations | Thực thi nhất quán khi triển khai hệ thống | M06-T003 |

## 5. Tự kiểm M06-T005
- Đã hoàn thành đặc tả `M06-LEGACY-ECONOMIC-DATA-MIGRATION-1.0`.
- Chốt nguyên tắc khởi tạo bản ghi `INITIAL_MIGRATION` và kiểm soát chênh lệch tổng hệ thống bằng 0.
- Ghi nhận 2 Regression Gates (`EM-G01`–`EM-G02`) và 3 Test Cases (`EM05-01`–`EM05-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả lập kế hoạch chuyển dữ liệu kinh tế hiện có M06-T005 | WSA-7K2 |
