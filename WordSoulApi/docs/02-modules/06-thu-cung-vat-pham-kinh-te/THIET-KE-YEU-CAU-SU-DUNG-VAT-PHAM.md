# Thiết kế yêu cầu sử dụng vật phẩm M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ITEM-USE-REQUEST-1.0` |
| Task | M06-T035 |
| Đầu vào | M06-ITEM-INVENTORY-OWNERSHIP-1.0 (M06-T034), M03-SESSION-POLICY-1.0 (M03-T002) |
| Phạm vi | Quy trình xử lý yêu cầu kích hoạt sử dụng vật phẩm (`POST /api/v1/inventory/use`), kiểm tra số lượng và hiệu ứng áp dụng |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả logic xác thực và trừ số lượng vật phẩm khi người học sử dụng.

- **Ràng buộc Số lượng Đủ (`Sufficient Inventory Invariant`)**: Yêu cầu sử dụng vật phẩm CHỈ ĐƯỢC CHẤP NHẬN khi $Quantity \ge 1$ trong `UserInventory`. Ngược lại, hệ thống từ chối với lỗi HTTP 400 `INSUFFICIENT_ITEM_QUANTITY`.
- **Trừ Kho Nguyên tử (`Atomic Inventory Deduction Invariant`)**: Việc trừ $Quantity$ trong kho và áp dụng hiệu ứng BẮT BUỘC thực thi trong cùng một Database Transaction.

## 2. Dynamic Item Use Logic

```csharp
public async Task<bool> UseItemAsync(Guid userId, Guid itemId, string contextSessionId)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    var slot = await _dbContext.UserInventory
        .FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == itemId);
        
    if (slot == null || slot.Quantity <= 0)
    {
        throw new InvalidOperationException("INSUFFICIENT_ITEM_QUANTITY");
    }
    
    // Trừ kho 1 đơn vị
    slot.Quantity -= 1;
    slot.UpdatedAtUtc = DateTime.UtcNow;
    
    // Ghi nhận log tiêu thụ vật phẩm
    _dbContext.ItemUsageLogs.Add(new ItemUsageLog {
        LogId = Guid.NewGuid(),
        UserId = userId,
        ItemId = itemId,
        ContextSessionId = contextSessionId,
        UsedAtUtc = DateTime.UtcNow
    });
    
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
    return true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IU-G01`: 100% request dùng vật phẩm khi `Quantity == 0` bị từ chối với lỗi HTTP 400.
- `IU-G02`: `ItemUsageLogs` lưu vết chính xác `ContextSessionId` nơi vật phẩm được kích hoạt.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IU35-01` | Người dùng có 2 `STREAK_FREEZE`, gọi API kích hoạt | `Quantity` giảm xuống 1, trả kết quả kích hoạt thành công. |
| `IU35-02` | Gọi dùng vật phẩm khi chưa mua (`Quantity == 0`) | System reject với lỗi `INSUFFICIENT_ITEM_QUANTITY`. |
| `IU35-03` | Kiểm thử hoàn tất luồng M06-ITEM-USE-REQUEST-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-IU-F01` | Cần Consumer `ItemConsumedConsumer` lắng nghe sự kiện hoàn trả | Xử lý hoàn trả vật phẩm khi phiên lỗi | M06-T036 |

## 5. Tự kiểm M06-T035
- Đã đặc tả thiết kế yêu cầu sử dụng vật phẩm M06-T035.
- Ghi nhận 2 Regression Gates (`IU-G01`–`IU-G02`) và 3 Test Cases (`IU35-01`–`IU35-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế yêu cầu sử dụng vật phẩm M06-T035 | WSA-7K2 |
