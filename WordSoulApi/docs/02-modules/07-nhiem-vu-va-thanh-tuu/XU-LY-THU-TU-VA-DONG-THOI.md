# Xử lý thứ tự và đồng thời M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-CONCURRENCY-ORDERING-1.0` |
| Task | M07-T014 |
| Đầu vào | M07-QUEST-EVENT-CONTRACT-1.0 (M07-T012), M07-QUEST-IDEMPOTENT-PROGRESS-1.0 (M07-T013) |
| Phạm vi | Cơ chế khóa bi quan/lạc quan và quản lý thứ tự sự kiện khi có nhiều request cập nhật tiến độ nhiệm vụ đồng thời từ 2 thiết bị |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc đảm bảo tính nhất quán dữ liệu tiến độ nhiệm vụ khi xuất hiện tranh chấp đồng thời (Concurrency Contention).

- **Tính Khóa Hàng chờ Phân tán (`Distributed Lock Invariant`)**: Mọi thao tác cập nhật tiến độ nhiệm vụ của 1 người dùng (`UserId`) BẮT BUỘC phải chiếm giữ khóa phân tán (Distributed Redlock) theo bộ `lock_quest_{UserId}`. Thời gian giữ khóa tối đa $3,000$ ms.
- **Ràng buộc Không Tràn Ngưỡng (`Counter Bound Invariant`)**: Giá trị tiến độ $CurrentCount$ CẤM vượt quá $TargetCount$. Khi đạt $TargetCount$, tiến độ kẹp dừng ở $TargetCount$ và trạng thái chuyển `CLAIMABLE`.

## 2. Dynamic Concurrent Progress Update Logic

```csharp
public async Task UpdateProgressConcurrentSafeAsync(Guid userId, Guid questId, int amount, string eventId)
{
    string lockKey = $"lock_quest_{userId}";
    using var redLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(3));
    
    if (!redLock.IsAcquired)
    {
        throw new InvalidOperationException("QUEST_PROGRESS_MUTATION_BUSY");
    }
    
    await IncrementQuestProgressIdempotentAsync(userId, questId, amount, eventId);
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `CO-G01`: 10 request cập nhật tiến độ gửi song song trong cùng 1 ms không làm thất thoát tiến độ hoặc gây dead-lock CSDL.
- `CO-G02`: $CurrentCount$ không bao giờ lớn hơn $TargetCount$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CO14-01` | Gửi đồng thời 2 event hoàn thành phiên từ Web và Mobile | Khóa Redlock đảm bảo 2 event được xử lý nối tiếp, tiến độ cộng đúng $+2$. |
| `CO14-02` | Tiến độ đang 2/3, nhận đồng thời 2 event $+1$ | Tiến độ kẹp dừng ở $3/3$, trạng thái `CLAIMABLE`. |
| `CO14-03` | Kiểm thử hoàn tất luồng M07-CONCURRENCY-ORDERING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-CO-F01` | Cần cấu hình Redlock Redis Provider trong Infrastructure | Đảm bảo khóa phân tán ổn định | M07-T015 |

## 5. Tự kiểm M07-T014
- Đã đặc tả xử lý thứ tự và đồng thời M07-T014.
- Ghi nhận 2 Regression Gates (`CO-G01`–`CO-G02`) và 3 Test Cases (`CO14-01`–`CO14-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả xử lý thứ tự và đồng thời M07-T014 | WSA-7K2 |
