# Chuẩn hóa quyền sở hữu vật phẩm M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ITEM-INVENTORY-OWNERSHIP-1.0` |
| Task | M06-T034 |
| Đầu vào | M06-ITEM-CATALOG-MODEL-1.0 (M06-T007), M06-REWARD-IDEMPOTENCY-1.0 (M06-T012) |
| Phạm vi | Mô hình Kho kho chứa vật phẩm của người dùng (`UserInventory`), quản lý quyền sở hữu, số lượng sở hữu (`Quantity`) và trạng thái hiệu lực |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc kiểm soát quyền sở hữu và kho vật phẩm cá nhân trong M06.

- **Tính Duy nhất của Bản ghi Kho (`Unique Inventory Slot Invariant`)**: Mỗi bộ đôi `(UserId, ItemId)` BẮT BUỘC có tối đa 1 bản ghi duy nhất trong `UserInventory`. CẤM xuất hiện 2 bản ghi kho trùng nhau cho cùng một vật phẩm của 1 người học.
- **Ràng buộc Số lượng Không Âm (`Non-Negative Inventory Quantity Invariant`)**: Số lượng sở hữu `Quantity` BẮT BUỘC thỏa mãn $Quantity \ge 0$. Việc cộng thêm khi mua hoặc thưởng phải tuân thủ trần `MaxStackSize`.

## 2. Dynamic User Inventory Schema

```csharp
public class UserInventory
{
    public Guid InventoryId { get; set; }
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
    
    public int Quantity { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    
    public DateTime UpdatedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IO-G01`: 100% bản ghi trong `UserInventory` có `Quantity >= 0`.
- `IO-G02`: Bảng `UserInventory` bổ sung Unique Index `UX_UserInventory_User_Item` chặn tạo trùng slot kho.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IO34-01` | Người dùng chưa có vật phẩm, nhận thưởng 2 `STREAK_FREEZE` | Thêm 1 bản ghi `UserInventory` mới với `Quantity = 2`. |
| `IO34-02` | Người dùng đã có 2 `STREAK_FREEZE`, nhận tiếp 1 | Cập nhật bản ghi hiện tại `Quantity` lên 3. |
| `IO34-03` | Kiểm thử hoàn tất luồng M06-ITEM-INVENTORY-OWNERSHIP-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-IO-F01` | Cần thuộc tính `MaxStackSize` kiểm tra trước khi chèn bản ghi kho | Chống mua tràn kho | M06-T035 |

## 5. Tự kiểm M06-T034
- Đã đặc tả chuẩn hóa quyền sở hữu vật phẩm M06-T034.
- Ghi nhận 2 Regression Gates (`IO-G01`–`IO-G02`) và 3 Test Cases (`IO34-01`–`IO34-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa quyền sở hữu vật phẩm M06-T034 | WSA-7K2 |
