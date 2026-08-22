# Thiết kế số dư dẫn xuất và đối soát M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-DERIVED-BALANCE-RECONCILIATION-1.0` |
| Task | M06-T004 |
| Đầu vào | M06-ASSET-LEDGER-MODEL-1.0 (M06-T003), M11-REALTIME-FRESHNESS-1.0 (M11-T023) |
| Phạm vi | Mô hình số dư dẫn xuất (`Derived Balance`), thuật toán đối soát sổ cái (`Ledger Reconciliation Engine`) và phát hiện chênh lệch tài sản |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định thuật toán đối soát (`Reconciliation Engine`) giữa số dư dẫn xuất (`UserAssetBalance`) và tổng các dòng trong sổ cái (`AssetLedgerEntries`) M06.

- **Số dư Khớp 100% với Tổng Sổ cái (`Ledger Reconciliation Invariant`)**:
  - Số dư tài sản hiện tại $Balance_{current}$ của người dùng BẮT BUỘC bằng tổng tất cả các biến động cộng/trừ trong sổ cái:
    $$Balance_{current} = Balance_{initial} + \sum \Delta Amount$$
  - Nếu xảy ra chênh lệch ($Balance_{current} \neq \sum \Delta Amount$), hệ thống lập tức phát cảnh báo `ASSET_RECONCILIATION_MISMATCH` và tạm thời khóa tính năng tiêu dùng tài sản của tài khoản đó để bảo vệ hệ thống.
- **Không bao giờ Âm ngoài Chính sách (`Non-Negative Balance Rule`)**: Số dư dẫn xuất và tổng sổ cái tuyệt đối CẤM $< 0$.

## 2. Quy trình Đối soát Sổ cái Tài sản (Ledger Reconciliation Flow)

```mermaid
graph TD
    Cron[Daily / Triggered Audit Job] --> ReadBal[Read UserAssetBalance]
    ReadBal --> SumLedger[Calculate Sum(AssetLedgerEntries.Amount)]
    SumLedger --> CheckMatch{Balance == Sum?}
    CheckMatch -->|Yes| PassAudit[Log Audit PASSED]
    CheckMatch -->|No| RaiseAlert[Raise ASSET_RECONCILIATION_MISMATCH Alert & Lock Spending]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `BR-G01`: 100% tài khoản có sai lệch giữa số dư dẫn xuất và tổng sổ cái bị khóa giao dịch tiêu dùng trong $\le 1\text{s}$.
- `BR-G02`: Báo cáo đối soát ghi nhận chính xác giá trị chênh lệch và danh sách các giao dịch nghi vấn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `BR04-01` | Chạy job đối soát cho tài khoản có 100 giao dịch sổ cái khớp số dư 500 Gold | Trả về kết quả đối soát `PASSED`, `DiscrepancyAmount = 0`. |
| `BR04-02` | Giả lập hacker sửa trực tiếp cột `GoldBalance` trong DB từ 100 lên 1,000 | Job đối soát phát hiện chênh lệch 900 Gold, ném cảnh báo `MISMATCH` và khóa tài khoản. |
| `BR04-03` | Kiểm thử hoàn tất luồng M06-DERIVED-BALANCE-RECONCILIATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-BR-F01` | Đăng ký job đối soát tài sản định kỳ 02:00 AM UTC trong M11 | Tự động phát hiện chênh lệch tài sản | M11-T038 |

## 5. Tự kiểm M06-T004
- Đã hoàn thành đặc tả `M06-DERIVED-BALANCE-RECONCILIATION-1.0`.
- Chốt công thức đối soát sổ cái $Balance = \sum \Delta Amount$ và cơ chế tự động khóa khi mismatch.
- Ghi nhận 2 Regression Gates (`BR-G01`–`BR-G02`) và 3 Test Cases (`BR04-01`–`BR04-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế số dư dẫn xuất và đối soát M06-T004 | WSA-7K2 |
