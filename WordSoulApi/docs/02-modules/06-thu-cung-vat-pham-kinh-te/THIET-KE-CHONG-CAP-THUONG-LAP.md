# Thiết kế chống cấp thưởng lặp M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-REWARD-IDEMPOTENCY-1.0` |
| Task | M06-T012 |
| Đầu vào | M06-ASSET-LEDGER-MODEL-1.0 (M06-T003), M06-REWARD-EVENT-CONTRACT-1.0 (M06-T011) |
| Phạm vi | Cơ chế Idempotency chống cấp trùng phần thưởng khi nhận lại sự kiện `ReferenceEventId` từ M03/M07 |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế chống cấp thưởng trùng lặp trong M06.

- **Tính Duy nhất của ReferenceEventId (`Reference Event Idempotency Invariant`)**: Một mã sự kiện tham chiếu (`ReferenceEventId`) BẮT BUỘC chỉ được phép tạo đúng 1 giao dịch cộng thưởng trong `AssetLedgerEntries`. Sự kiện trùng `ReferenceEventId` gửi lại CẤM tuyệt đối việc cộng thêm tiền vào tài khoản.
- **Ràng buộc Duy nhất ở CSDL (`Database Unique Constraint Invariant`)**: Bảng `AssetLedgerEntries` bổ sung chỉ mục duy nhất (Unique Index) trên bộ khóa `(ReferenceEventId, SourceModule, CurrencyCode)`.

## 2. Dynamic Idempotent Reward Credit Logic

```csharp
public async Task<bool> CreditRewardIdempotentAsync(Guid userId, string currencyCode, int amount, string refEventId, string sourceModule)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    // 1. Kiểm tra tồn tại trong sổ biến động theo ReferenceEventId
    bool exists = await _dbContext.AssetLedgerEntries.AnyAsync(e => 
        e.ReferenceEventId == refEventId && 
        e.SourceModule == sourceModule && 
        e.CurrencyCode == currencyCode);
        
    if (exists)
    {
        await transaction.RollbackAsync();
        return true; // Idempotent Ack (đã cộng thưởng từ trước)
    }
    
    // 2. Lấy số dư hiện tại & kẹp trần daily cap
    int currentBalance = await GetCurrentBalanceAsync(userId, currencyCode);
    int allowedAmount = CalculateCapAllowedAmount(userId, currencyCode, amount);
    
    if (allowedAmount <= 0)
    {
        await transaction.RollbackAsync();
        return true; // Đã chạm trần ngày
    }
    
    // 3. Chèn bản ghi sổ biến động
    var entry = new AssetLedgerEntry {
        EntryId = Guid.NewGuid(),
        UserId = userId,
        CurrencyCode = currencyCode,
        Type = TransactionType.CREDIT,
        Amount = allowedAmount,
        BalanceBefore = currentBalance,
        BalanceAfter = currentBalance + allowedAmount,
        SourceModule = sourceModule,
        ReferenceEventId = refEventId,
        CreatedAtUtc = DateTime.UtcNow
    };
    
    _dbContext.AssetLedgerEntries.Add(entry);
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
    return true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RI-G01`: Gửi 5 sự kiện cộng 100 Gold trùng `ReferenceEventId` chỉ có 1 giao dịch cộng Gold được thực ghi.
- `RI-G02`: Vi phạm Unique Constraint tại CSDL được bắt lỗi và trả lời Idempotent Ack an toàn.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RI12-01` | Sự kiện chốt phiên `ses_123` được gửi lại 3 lần do retry bus | Chỉ 1 lần duy nhất cộng 50 Gold, 2 lần sau bị bỏ qua. |
| `RI12-02` | Kiểm tra tổng số dư Gold sau khi nhận 3 sự kiện trùng | Số dư Gold tăng đúng 50 Gold, không tăng 150 Gold. |
| `RI12-03` | Kiểm thử hoàn tất luồng M06-REWARD-IDEMPOTENCY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-RI-F01` | Tạo Unique Index `UX_AssetLedger_RefEvent` trong CSDL | Đảm bảo tính chống trùng ở cấp DB | M06-T013 |

## 5. Tự kiểm M06-T012
- Đã đặc tả thiết kế chống cấp thưởng lặp M06-T012.
- Ghi nhận 2 Regression Gates (`RI-G01`–`RI-G02`) và 3 Test Cases (`RI12-01`–`RI12-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế chống cấp thưởng lặp M06-T012 | WSA-7K2 |
