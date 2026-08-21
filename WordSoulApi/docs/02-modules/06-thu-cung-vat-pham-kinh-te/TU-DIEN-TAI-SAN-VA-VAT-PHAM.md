# Chuẩn hóa từ điển tài sản và vật phẩm M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-ASSET-ITEM-DICT-1.0` |
| Task | M06-T001 |
| Đầu vào | M01-T001 (Từ điển danh tính), CT-07 / D-011 (Loại bỏ AP), REL-04 (Hạn mức tài sản) |
| Phạm vi | Chuẩn hóa từ điển tài sản, tiền tệ (Gold/Gems), vật phẩm tiêu hao, sổ biến động (Asset Ledger) và nguyên tắc cấm AP (Action Points) |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa toàn bộ thuật ngữ, đơn vị tài sản kinh tế và nguyên tắc quản lý sổ cái tài sản (Asset Ledger) trong M06.

- **Nguyên tắc Sổ cái Biến động Append-Only (`Append-Only Asset Ledger Invariant`)**:
  - 100% thay đổi số dư tài sản (Gold, Gems, Exp, Item Quantity) BẮT BUỘC phải tạo ra một bản ghi giao dịch bất biến (`AssetLedgerEntry`) với lý do (`ReasonCode`), mã tham chiếu (`ReferenceId`) và số dư trước/sau (`BalanceBefore`, `BalanceAfter`).
  - CẤM gọi lệnh `UPDATE` trực tiếp làm thay đổi số dư mà không có bản ghi sổ cái tương ứng.
- **Bảo đảm Loại bỏ AP (`Zero Action Points Invariant - CT-07 / D-011`)**:
  - Tuyệt đối CẤM khái niệm Thể lực / Năng lượng học / Action Points (AP) làm giới hạn lượt học. Người học có quyền thực hiện phiên học không giới hạn mà không bị chặn bởi bất kỳ cơ chế AP nào.
- **Tính Chống Cấp Thưởng Trùng (`Reward Idempotency Invariant`)**:
  - Mọi giao dịch cấp thưởng (từ M03 Session, M07 Quest) bắt buộc phải kiểm tra `IdempotencyKey` / `ReferenceId`. Cấp lặp lại với cùng `ReferenceId` bắt buộc trả về kết quả thành công mà không cộng thêm tiền/điểm.

## 2. Bảng Từ điển Thuật ngữ Kinh tế M06 (Asset Lexicon)

| Thuật ngữ | Tên tiếng Việt | Mô tả & Quy tắc trong WordSoul | Đơn vị / Giới hạn |
|---|---|---|---|
| `Gold` | Vàng học tập | Đơn vị tiền tệ chính nhận được qua việc hoàn thành phiên học và nhiệm vụ. Dùng mua vật phẩm học tập. | Số nguyên $\ge 0$. |
| `Gems` | Kim cương | Đơn vị tiền tệ cao cấp/thưởng đặc biệt khi đạt mốc thành tựu hoặc chuỗi học (Streak). | Số nguyên $\ge 0$. |
| `Exp` | Điểm kinh nghiệm | Điểm tích lũy nâng cấp độ người học (User Level). Không thể tiêu dùng. | Số nguyên $\ge 0$. |
| `AssetLedgerEntry` | Bản ghi sổ cái | Đơn vị giao dịch bất biến ghi nhận tăng/giảm tài sản kèm hash truy vết. | Immutable Record |
| `ConsumableItem` | Vật phẩm tiêu hao | Các vật phẩm hỗ trợ học tập (Thẻ đông băng Streak, Thẻ gợi ý, Thẻ nhân đôi Exp). | Số lượng $\ge 0$. |
| `IdempotencyKey` | Mã chống trùng | Chuỗi định danh duy nhất cho giao dịch (ví dụ: `REWARD-M03-{SessionId}`). | Unique String |
| `CompensationAdjustment` | Điều chỉnh bồi hoàn | Thao tác cộng/trừ tài sản thủ công do ContentAdmin/SupportAgent thực hiện kèm TicketId. | M11 Controlled Op |

## 3. Cấu trúc Sổ cái Tài sản (Asset Ledger Schema)

```csharp
public class AssetLedgerEntry
{
    public Guid LedgerEntryId { get; set; }
    public Guid UserId { get; set; }
    public AssetType AssetType { get; set; } // GOLD, GEMS, EXP, ITEM
    public string? ItemId { get; set; }
    public long AmountDelta { get; set; } // +100 hoặc -50
    public long BalanceBefore { get; set; }
    public long BalanceAfter { get; set; }
    public string ReasonCode { get; set; } // REWARD_SESSION_COMPLETE, REWARD_DAILY_QUEST, PURCHASE_ITEM
    public string ReferenceId { get; set; } // SessionId, QuestId, OrderId
    public DateTime CreatedAtUtc { get; set; }
}
```

## 4. Kiểm soát Hạn mức Ngày và An toàn Kinh tế (Daily Economy Caps - REL-04)

- **Hạn mức Vàng tối đa trong ngày (`Max Daily Gold Cap`)**: Tối đa $5,000$ Gold / ngày từ mọi nguồn thưởng học tập.
- **Hạn mức Exp tối đa trong ngày (`Max Daily Exp Cap`)**: Tối đa $5,000$ Exp / ngày.
- Khi đạt hạn mức ngày, các giao dịch cấp thưởng tiếp theo vẫn trả về thành công nhưng `AmountDelta = 0` kèm `ReasonCode = DAILY_CAP_REACHED`.

## 5. Regression Gates và Test Cases

### 5.1. Regression Gates
- `AI-G01`: 100% biến động tài sản tạo bản ghi `AssetLedgerEntry` bất biến, balance tính toán khớp tuyệt đối `BalanceBefore + AmountDelta == BalanceAfter`.
- `AI-G02`: Không tồn tại bất kỳ API hay logic nào yêu cầu hoặc trừ điểm AP (CT-07 compliance).
- `AI-G03`: Gửi trùng request cấp thưởng với cùng `ReferenceId` không làm tăng thêm số dư tài sản (Idempotent).
- `AI-G04`: Hạn mức Gold/Exp hàng ngày được kiểm soát không vượt quá $5,000$ / ngày.

### 5.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AI01-01` | Cấp 100 Gold từ phiên học M03 | Sổ cái ghi nhận transaction `AmountDelta = +100`, `BalanceAfter = BalanceBefore + 100`. |
| `AI01-02` | Gửi lại event cấp thưởng M03 hai lần liên tiếp với cùng `SessionId` | Lần 1 cộng 100 Gold; lần 2 trả kết quả cũ, không cộng thêm Gold. |
| `AI01-03` | Người học học liên tục đạt $5,000$ Gold trong ngày | Các phiên tiếp theo ghi nhận `AmountDelta = 0`, `ReasonCode = DAILY_CAP_REACHED`. |
| `AI01-04` | Kiểm tra mã nguồn xem có thuộc tính AP hay Energy | Đạt 100% không chứa AP hay Energy limit. |
| `AI01-05` | SupportAgent thực hiện bồi hoàn 50 Gold cho người học | Tạo bản ghi ledger với `ReasonCode = SUPPORT_COMPENSATION` kèm `TicketId`. |
| `AI01-06` | Kiểm thử hoàn tất luồng M06-ASSET-ITEM-DICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 6. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-AI-F01` | Cần tạo bảng `AssetLedgerEntries` trong CSDL `WordSoul.Infrastructure` | Chưa có CSDL lưu vết sổ cái tài sản | M06-T002 |

## 7. Tự kiểm M06-T001
- Đã chuẩn hóa từ điển tài sản M06, cam kết sổ cái append-only và tuân thủ CT-07 cấm AP.
- Chốt hạn mức kinh tế an toàn $5,000$ Gold/Exp daily cap.
- Xác lập 4 Regression Gates (`AI-G01`–`AI-G04`) và 6 Test Cases (`AI01-01`–`AI01-06`).

## 8. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa từ điển tài sản và vật phẩm M06-T001 | WSA-7K2 |
