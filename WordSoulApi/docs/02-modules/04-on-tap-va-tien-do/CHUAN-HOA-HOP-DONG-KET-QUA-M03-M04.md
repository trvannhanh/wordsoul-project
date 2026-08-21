# Chuẩn hóa hợp đồng kết quả M03→M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-SESSION-RESULT-CONTRACT-1.0` |
| Task | M04-T006 |
| Đầu vào | M03-SESSION-COMPLETED-EVENT-1.0 (M03-T040), M04-USER-SENSE-UNIT-1.0 (M04-T002) |
| Phạm vi | Hợp đồng giao tiếp dữ liệu sự kiện giữa M03 và M04, tiêu chuẩn xác thực thông tin và xử lý bất đồng bộ |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả giao thức hợp đồng nhận kết quả học tập từ M03 sang M04 để cập nhật trạng thái SRS.

- **Tính Đúng đắn của Thông tin Sự kiện (`Valid Event Contract Invariant`)**: M04 CHỈ chấp nhận các sự kiện hoàn thành phiên từ M03 có `SessionId`, `UserId` và danh sách `InitialRecallResults` hợp lệ. Sự kiện thiếu tham số bắt buộc sẽ bị đưa vào hàng đợi `DeadLetterQueue` (DLQ) để kiểm tra.
- **Tính Bất biến của Điểm Gợi nhớ Lần 1 (`First-Try Result Enforcement`)**: M04 CHỈ dùng kết quả trả lời LẦN 1 (`IsCorrectFirstTry`) để tính toán thuật toán lặp lại ngắt quãng SRS.

## 2. Dynamic Contract Message Consumer

```csharp
public class LearningSessionCompletedConsumer : IConsumer<LearningSessionCompletedEvent>
{
    private readonly ISrsEngine _srsEngine;
    
    public async Task Consume(ConsumeContext<LearningSessionCompletedEvent> context)
    {
        var msg = context.Message;
        
        // 1. Kiểm tra Idempotency
        if (await _srsEngine.IsEventProcessedAsync(msg.EventId))
        {
            return; // Đã xử lý trước đó, skip
        }
        
        // 2. Cập nhật hồ sơ SRS cho từng nét nghĩa
        foreach (var item in msg.InitialRecallResults)
        {
            await _srsEngine.ApplyRecallEvidenceAsync(
                msg.UserId, 
                item.VocabularySenseId, 
                item.IsCorrectFirstTry, 
                item.ResponseDurationMs
            );
        }
        
        // 3. Đánh dấu event đã xử lý thành công
        await _srsEngine.MarkEventProcessedAsync(msg.EventId);
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RC-G01`: 100% sự kiện tiêu thụ thành công tại M04 làm cập nhật `DueDateUtc` mới cho hồ sơ nhớ.
- `RC-G02`: Message lỗi cấu trúc tự động đẩy sang DLQ sau 3 lần retry thất bại.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC06-01` | M04 tiêu thụ `LearningSessionCompletedEvent` 10 từ | Cập nhật thành công 10 bản ghi `UserSenseProgress`. |
| `RC06-02` | Sự kiện gửi thiếu trường `UserId` | Consumer đẩy message sang DLQ `wordsoul-m04-dlq`. |
| `RC06-03` | Kiểm thử hoàn tất luồng M04-SESSION-RESULT-CONTRACT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-RC-F01` | Cần cấu hình `RabbitMQ` Exchange `wordsoul.learning.events` trong MassTransit | Đảm bảo routing message chính xác | M04-T007 |

## 5. Tự kiểm M04-T006
- Đã đặc tả chuẩn hóa hợp đồng kết quả M03->M04 M04-T006.
- Ghi nhận 2 Regression Gates (`RC-G01`–`RC-G02`) và 3 Test Cases (`RC06-01`–`RC06-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa hợp đồng kết quả M03->M04 M04-T006 | WSA-7K2 |
