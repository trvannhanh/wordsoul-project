# Chuẩn hóa mô hình danh mục vật phẩm M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ITEM-CATALOG-MODEL-1.0` |
| Task | M06-T007 |
| Đầu vào | M06-ASSET-ITEM-DICT-1.0 (M06-T001), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Mô hình đối tượng vật phẩm cửa hàng (`ItemCatalog`) quy định khả năng mua, tiêu thụ, thời hạn sử dụng và cộng dồn |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cấu trúc bảng danh mục vật phẩm mua sắm và sử dụng trong M06.

- **Ràng buộc Sử dụng Theo Ngữ cảnh (`Usage Context Invariant`)**: Mỗi vật phẩm BẮT BUỘC có một thuộc tính `UsageContext` (`LEARNING_SESSION`, `STREAK_PROTECTION`, `AVATAR_FRAME`). Vật phẩm chỉ được kích hoạt sử dụng trong đúng ngữ cảnh được quy định.
- **Ràng buộc Vật phẩm Không Cộng dồn (`Non-Stackable Duration Invariant`)**: Các vật phẩm đóng vai trò Bảo vệ Streak (`STREAK_FREEZE`) hoặc Thẻ Nhân đôi EXP (`EXP_BOOSTER`) CẤM cộng dồn thời gian hiệu lực quá $7$ ngày liên tiếp.

## 2. Dynamic Item Catalog Schema

```csharp
public class ItemCatalog
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public ItemType Type { get; set; } // CONSUMABLE, PERMANENT, TIME_BOUND
    public UsageContext Context { get; set; } // LEARNING_SESSION, STREAK_PROTECTION
    
    public string PriceCurrency { get; set; } // GOLD, GEMS
    public int PriceAmount { get; set; }
    
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; }
    
    public bool IsActive { get; set; } = true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IC-G01`: 100% vật phẩm loại `CONSUMABLE` có `MaxStackSize > 0`.
- `IC-G02`: Thử dùng vật phẩm `STREAK_FREEZE` ngoài màn hình Streak trả lỗi `INVALID_USAGE_CONTEXT`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IC07-01` | Mua 1 vật phẩm `STREAK_FREEZE` bằng 200 Gold | Trừ 200 Gold, thêm 1 item vào kho `UserInventory`. |
| `IC07-02` | Thử kích hoạt vật phẩm `STREAK_FREEZE` trong khi đang làm bài thi | System reject với lỗi `INVALID_USAGE_CONTEXT`. |
| `IC07-03` | Kiểm thử hoàn tất luồng M06-ITEM-CATALOG-MODEL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-IC-F01` | Cần Seeder tạo 3 vật phẩm mặc định (Streak Freeze, Exp Boost 2x, Hint Token) | Đảm bảo Shop có sẵn mặt hàng khi khởi chạy | M06-T008 |

## 5. Tự kiểm M06-T007
- Đã đặc tả chuẩn hóa mô hình danh mục vật phẩm M06-T007.
- Ghi nhận 2 Regression Gates (`IC-G01`–`IC-G02`) and 3 Test Cases (`IC07-01`–`IC07-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa mô hình danh mục vật phẩm M06-T007 | WSA-7K2 |
