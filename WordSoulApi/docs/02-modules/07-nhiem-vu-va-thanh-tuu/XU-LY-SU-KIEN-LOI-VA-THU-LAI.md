# Xử lý sự kiện lỗi và thử lại M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-RETRY-DLQ-1.0` |
| Task | M07-T015 |
| Đầu vào | M07-QUEST-EVENT-CONTRACT-1.0 (M07-T012), M12-RETRY-IDEMPOTENCY-1.0 (M12-T037) |
| Phạm vi | Chính sách Retry (Exponential Backoff) và hàng chờ thư chết (Dead-Letter Queue - DLQ) khi xử lý sự kiện tiến độ nhiệm vụ thất bại |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chiến lược xử lý ngoại lệ khi tiêu thụ sự kiện tiến độ nhiệm vụ trong M07.

- **Chính sách Thử lại Lũy thừa (`Exponential Backoff Invariant`)**: Sự kiện bị lỗi kỹ thuật tạm thời (như mất kết nối CSDL) được thử lại tối đa 3 lần với khoảng chờ lũy thừa: $2^1 = 2$ giây, $2^2 = 4$ giây, $2^3 = 8$ giây.
- **Chuyển sang DLQ sau 3 lần Thất bại (`DLQ Escalation Invariant`)**: Nếu sau 3 lần thử lại vẫn thất bại, sự kiện BẮT BUỘC được đẩy vào `quest_progress_events_dlq` kèm theo lý do lỗi (`ExceptionStackTrace`).

## 2. Dynamic Retry & DLQ Consumer Configuration

```csharp
public class QuestEventConsumerDefinition : ConsumerDefinition<QuestProgressConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<QuestProgressConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(2), 
            TimeSpan.FromSeconds(4), 
            TimeSpan.FromSeconds(8)));
            
        endpointConfigurator.UseInMemoryOutbox();
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RD-G01`: 100% sự kiện thất bại 3 lần liên tiếp được đẩy thành công vào bảng `quest_progress_events_dlq`.
- `RD-G02`: Hệ thống phát cảnh báo PagerDuty khi DLQ có quá 50 sự kiện tồn đọng trong 1 giờ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RD15-01` | CSDL bị lấn chiếm connection pool khiến insert `QuestEventLog` thất bại | Retry 3 lần, lần 3 thành công khi DB khôi phục. |
| `RD15-02` | Payload event bị hỏng (Invalid Schema) | Không retry vô ích, đẩy ngay sang DLQ. |
| `RD15-03` | Kiểm thử hoàn tất luồng M07-QUEST-RETRY-DLQ-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-RD-F01` | Cần trang Admin UI hỗ trợ Replay message từ DLQ | Khôi phục sự kiện bị hỏng dữ liệu thủ công | M07-T016 |

## 5. Tự kiểm M07-T015
- Đã đặc tả xử lý sự kiện lỗi và thử lại M07-T015.
- Ghi nhận 2 Regression Gates (`RD-G01`–`RD-G02`) và 3 Test Cases (`RD15-01`–`RD15-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xử lý sự kiện lỗi và thử lại M07-T015 | WSA-7K2 |
