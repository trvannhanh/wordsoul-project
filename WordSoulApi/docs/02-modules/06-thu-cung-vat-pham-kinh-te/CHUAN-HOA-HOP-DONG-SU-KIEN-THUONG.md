# Chuẩn hóa hợp đồng sự kiện thưởng M06

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M06-REWARD-EVENT-CONTRACT-1.0` |
| Task | M06-T011 |
| Đầu vào | M03-SESSION-COMPLETED-EVENT-1.0 (M03-T040), M07-QUEST-DICT-1.0 (M07-T001) |
| Phạm vi | Hợp đồng giao tiếp sự kiện yêu cầu cấp thưởng từ M03 (Phiên học) và M07 (Nhiệm vụ) sang M06 |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cấu trúc và logic xác thực cho các sự kiện yêu cầu cấp phần thưởng kinh tế gửi về M06.

- **Nguồn Phát hành Được chứng nhận (`Authorized Reward Source Invariant`)**: M06 CHỈ nhận và xử lý sự kiện phát thưởng phát ra từ các module được ủy quyền (`M03_LEARNING_SESSION`, `M07_DAILY_QUEST`). Các sự kiện không rõ nguồn gốc bị chối bỏ với lỗi `UNAUTHORIZED_REWARD_SOURCE`.
- **Hạn mức Tích lũy Ngày (REL-04 / CT-07 Invariant)**: Tổng lượng `Gold` hoặc `Exp` cộng vào tài khoản trong 1 ngày nghiệp vụ (00:00 - 23:59 UTC) CẤM vượt quá $5,000$ Gold/Exp. Lượng thưởng vượt hạn mức ngày sẽ tự động bị kẹp kịch trần $5,000$.

## 2. Dynamic Reward Event Consumer Code

```csharp
public class RewardEventConsumer : IConsumer<RewardClaimEvent>
{
    private readonly IAssetLedgerService _ledger;
    
    public async Task Consume(ConsumeContext<RewardClaimEvent> context)
    {
        var msg = context.Message;
        
        // 1. Kiểm tra nguồn ủy quyền
        if (!IsValidSource(msg.SourceModule))
        {
            throw new InvalidOperationException("UNAUTHORIZED_REWARD_SOURCE");
        }
        
        // 2. Cấp thưởng có bảo vệ Cap Ngày (REL-04)
        await _ledger.CreditWithDailyCapAsync(
            msg.UserId,
            msg.CurrencyCode,
            msg.Amount,
            msg.ReferenceEventId,
            msg.SourceModule
        );
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RC-G01`: 100% phần thưởng Gold/Exp tích lũy trong ngày vượt $5,000$ được kẹp đúng mức trần $5,000$.
- `RC-G02`: Nhận sự kiện thưởng từ module lạ không nằm trong danh sách được cấp phép trả lỗi `UNAUTHORIZED_REWARD_SOURCE`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC11-01` | Hoàn thành phiên M03 cộng 100 Gold khi tổng ngày đã là 4,950 Gold | Chỉ cộng 50 Gold để đạt đúng trần 5,000 Gold/ngày. |
| `RC11-02` | Thử gửi request cộng tiền từ Client trực tiếp | M06 reject với lỗi `UNAUTHORIZED_REWARD_SOURCE`. |
| `RC11-03` | Kiểm thử hoàn tất luồng M06-REWARD-EVENT-CONTRACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M06-RC-F01` | Cần thuộc tính `DailyAccumulatedGold` trong bảng `UserAssetSummary` | Theo dõi hạn mức ngày REL-04 hiệu quả | M06-T012 |

## 5. Tự kiểm M06-T011
- Đã đặc tả chuẩn hóa hợp đồng sự kiện thưởng M06-T011.
- Ghi nhận 2 Regression Gates (`RC-G01`–`RC-G02`) và 3 Test Cases (`RC11-01`–`RC11-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa hợp đồng sự kiện thưởng M06-T011 | WSA-7K2 |
