# Thiết kế mô hình sổ biến động M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ASSET-LEDGER-MODEL-1.0` |
| Task | M06-T003 |
| Đầu vào | M06-VALUE-UNIT-CATALOG-1.0 (M06-T002), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Mô hình Sổ biến động tài sản (`AssetLedgerEntry`), cấu trúc nhật ký giao dịch chỉ ghi (Append-Only), tính toán số dư tức thời từ lịch sử giao dịch |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả kiến trúc bảng sổ biến động tài sản kinh tế trong WordSoul M06.

- **Chỉ ghi Không Sửa Xóa (`Append-Only Ledger Invariant`)**: Bảng `AssetLedgerEntries` BẮT BUỘC chỉ tiếp nhận lệnh `INSERT`. CẤM tuyệt đối lệnh `UPDATE` hoặc `DELETE` trên bất kỳ bản ghi biến động tài sản nào để đảm bảo tính toàn vẹn kiểm toán (Audit Trail).
- **Tính Cân bằng Số dư (`Balance Reconciliation Invariant`)**: Số dư khả dụng của một người học tại bất kỳ thời điểm nào BẮT BUỘC bằng tổng lượng biến động ($Balance = \sum Amount$). Số dư trước và sau biến động (`BalanceBefore`, `BalanceAfter`) BẮT BUỘC được ghi lại bất biến tại thời điểm phát sinh giao dịch.

## 2. Dynamic Asset Ledger Entry Schema

```csharp
public class AssetLedgerEntry
{
    public Guid EntryId { get; set; }
    public Guid UserId { get; set; }
    
    public string CurrencyCode { get; set; } // GOLD, GEMS, EXP
    public TransactionType Type { get; set; } // CREDIT (Cộng), DEBIT (Trừ)
    public int Amount { get; set; }
    
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    
    public string SourceModule { get; set; } // M03_SESSION, M07_QUEST, M06_SHOP
    public string ReferenceEventId { get; set; } // EventId duy nhất từ nguồn
    
    public DateTime CreatedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LM-G01`: 100% bản ghi biến động trong `AssetLedgerEntries` có `BalanceAfter == BalanceBefore + Amount` (hoặc `- Amount`).
- `LM-G02`: Thử gọi `UPDATE` hoặc `DELETE` trên `AssetLedgerEntries` bị CSDL chặn hoàn toàn qua Trigger/Role.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LM03-01` | Cộng 50 Gold thưởng hoàn thành phiên M03 | Tạo bản ghi `AssetLedgerEntry` loại `CREDIT`, `BalanceAfter = BalanceBefore + 50`. |
| `LM03-02` | Thử xóa 1 bản ghi sổ biến động | CSDL trả lỗi `PERMISSION_DENIED_APPEND_ONLY_TABLE`. |
| `LM03-03` | Kiểm thử hoàn tất luồng M06-ASSET-LEDGER-MODEL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-LM-F01` | Thêm DB Trigger chặn UPDATE/DELETE trên bảng `AssetLedgerEntries` | Bảo đảm tính bất biến ở cấp CSDL | M06-T004 |

## 5. Tự kiểm M06-T003
- Đã đặc tả thiết kế mô hình sổ biến động M06-T003.
- Ghi nhận 2 Regression Gates (`LM-G01`–`LM-G02`) và 3 Test Cases (`LM03-01`–`LM03-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế mô hình sổ biến động M06-T003 | WSA-7K2 |
