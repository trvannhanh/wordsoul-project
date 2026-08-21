# Thiết kế chống áp dụng kết quả lặp M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-IDEMPOTENT-RESULT-CONSUMPTION-1.0` |
| Task | M04-T007 |
| Đầu vào | M04-SESSION-RESULT-CONTRACT-1.0 (M04-T006), M12-RETRY-IDEMPOTENCY-1.0 (M12-T037) |
| Phạm vi | Cơ chế Idempotency chống tính lặp thuật toán SRS khi Message Bus delivered lặp lại cùng 1 sự kiện `LearningSessionCompletedEvent` |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế bảo vệ tính Idempotency tại Consumer của M04.

- **Tính Idempotency của EventId (`EventId Idempotency Invariant`)**: Một `EventId` từ M03 CHỈ ĐƯỢC PHÁP áp dụng tính toán SRS đúng 1 lần. Nếu Message Bus gửi lại sự kiện trùng `EventId` (At-Least-Once Delivery), M04 BẮT BUỘC bỏ qua mà không làm tăng $Interval$ hay $RepetitionCount$ lần 2.
- **Bản ghi Nhật ký Tiêu thụ (`Processed Event Log Invariant`)**: Mỗi sự kiện tiêu thụ thành công được lưu vào bảng `ProcessedEvents` với khóa chính `EventId` dưới một Database Transaction.

## 2. Dynamic Processed Event Schema & Verification Logic

```csharp
public async Task<bool> ProcessSrsEventIdempotentAsync(LearningSessionCompletedEvent msg)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    
    // 1. Chèn khóa EventId vào ProcessedEvents table (Unique Constraint)
    var isInserted = await _dbContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO ProcessedEvents (EventId, ProcessedAtUtc) VALUES ({0}, {1}) ON CONFLICT DO NOTHING",
        msg.EventId, DateTime.UtcNow
    );
    
    if (isInserted == 0)
    {
        // Sự kiện đã được xử lý trước đó -> Rollback & Return Success (Idempotent Ack)
        await transaction.RollbackAsync();
        return true;
    }
    
    // 2. Áp dụng thuật toán SRS
    ApplySrsAlgorithm(msg);
    
    await _dbContext.SaveChangesAsync();
    await transaction.CommitAsync();
    return true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IC-G01`: Gửi trùng 3 lần sự kiện `EventId` chỉ làm thay đổi `IntervalDays` đúng 1 lần.
- `IC-G02`: Bảng `ProcessedEvents` ghi lại mốc thời gian tiêu thụ chính xác.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IC07-01` | Consumer nhận 2 message trùng `EventId` liên tiếp | Message 1 cập nhật SRS, Message 2 bị bỏ qua và Acknowledge ngay. |
| `IC07-02` | Kiểm tra $Interval$ sau khi nhận duplicate event | $Interval$ giữ nguyên giá trị đã tính ở lần 1. |
| `IC07-03` | Kiểm thử hoàn tất luồng M04-IDEMPOTENT-RESULT-CONSUMPTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-IC-F01` | Tạo bảng `ProcessedEvents` trong DbContext M04 | Lưu vết tiêu thụ sự kiện Idempotent | M04-T008 |

## 5. Tự kiểm M04-T007
- Đã đặc tả thiết kế chống áp dụng kết quả lặp M04-T007.
- Ghi nhận 2 Regression Gates (`IC-G01`–`IC-G02`) và 3 Test Cases (`IC07-01`–`IC07-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế chống áp dụng kết quả lặp M04-T007 | WSA-7K2 |
