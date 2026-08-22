# Thiết kế thu hồi hoặc bù phần thưởng M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-REWARD-CLAWBACK-COMPENSATION-1.0` |
| Task | M06-T017 |
| Đầu vào | M06-ASSET-LEDGER-MODEL-1.0 (M06-T003), M06-MULTI-COMPONENT-REWARD-1.0 (M06-T015), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Quy trình thu hồi tài sản thưởng sai (`Reward Clawback`) hoặc cấp bù phần thưởng thiếu (`Reward Compensation`), bảo đảm nguyên tắc ghi sổ bù liên kết giao dịch gốc |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy trình thu hồi (`Clawback`) hoặc cấp bù (`Compensation`) tài sản kinh tế trong M06.

- **Liên kết Sổ cái Gốc Bất biến (`Original Transaction Linkage Invariant`)**:
  - Giao dịch thu hồi hoặc bù phần thưởng BẮT BUỘC chứa thuộc tính `ReferenceLedgerId` tham chiếu đến dòng sổ cái gốc bị sai.
  - Tuyệt đối CẤM sửa trực tiếp số dư cũ hay xóa dòng sổ cái nguyên bản.
- **Quy tắc Xử lý Số dư khi Thu hồi (`Clawback Balance Constraint`)**:
  - Nếu người dùng đã lỡ tiêu hết Gold thưởng sai (số dư hiện tại $<$ lượng Gold cần thu hồi):
    - Hệ thống thu hồi tối đa đến $0$ Gold (`GoldBalance = 0`).
    - Ghi nhận khoản nợ tài sản `PendingDebtAmount` để tự động trừ vào các lần nhận thưởng tương lai. CẤM để số dư tài sản mang giá trị âm ($< 0$).

## 2. Quy trình Thực hiện Thu hồi / Cấp bù Tài sản (Clawback Flow)

```mermaid
graph TD
    Trigger[Clawback / Compensation Trigger] --> FetchOrig[Fetch Original Ledger Entry]
    FetchOrig --> CheckType{Action Type?}
    CheckType -->|Compensation| AddCredit[Insert CREDIT Entry with ReferenceLedgerId]
    CheckType -->|Clawback| CalcDebit[Calculate Debit Amount = Min(CurrentBal, ClawbackAmt)]
    CalcDebit --> AddDebit[Insert DEBIT Entry with ReferenceLedgerId]
    AddDebit --> RecordDebt{ClawbackAmt > CurrentBal?}
    RecordDebt -->|Yes| SetPendingDebt[Record PendingDebtAmount]
    RecordDebt -->|No| Finish[Finish Clawback]
    AddCredit --> Finish
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RC-G01`: 100% giao dịch thu hồi/bù sinh bản ghi sổ cái liên kết `ReferenceLedgerId` hợp lệ.
- `RC-G02`: Thu hồi tài sản vượt quá số dư hiện tại không làm số dư bị âm ($< 0$).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC17-01` | Thu hồi 500 Gold thưởng lầm, tài khoản người dùng có 300 Gold | Trừ 300 Gold (số dư về 0), ghi nhận `PendingDebtAmount = 200 Gold`. |
| `RC17-02` | Người dùng có nợ 200 Gold vừa hoàn thành phiên học nhận 100 Gold | Hệ thống tự động khấu trừ 100 Gold vào nợ, số dư nhận thực tế = 0, nợ còn 100 Gold. |
| `RC17-03` | Kiểm thử hoàn tất luồng M06-REWARD-CLAWBACK-COMPENSATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-RC-F01` | Thêm thuộc tính `PendingDebtAmount` trong thực thể `UserAssetBalance` | Phục vụ khấu trừ nợ thưởng tự động | M06-T004 |

## 5. Tự kiểm M06-T017
- Đã hoàn thành đặc tả `M06-REWARD-CLAWBACK-COMPENSATION-1.0`.
- Chốt nguyên tắc nợ tài sản `PendingDebtAmount` chống âm số dư và liên kết `ReferenceLedgerId`.
- Ghi nhận 2 Regression Gates (`RC-G01`–`RC-G02`) và 3 Test Cases (`RC17-01`–`RC17-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế thu hồi hoặc bù phần thưởng M06-T017 | WSA-7K2 |
