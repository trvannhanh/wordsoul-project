# Chuẩn hóa xử lý phiên bị khóa hoặc kẹt M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-LOCKED-STUCK-SESSION-RECOVERY-1.0` |
| Task | M03-T014 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M11-BACKGROUND-JOB-REGISTRY-1.0 (M11-T038) |
| Phạm vi | Cơ chế mở khóa tự động đối với các phiên học bị kẹt ở trạng thái trung gian, xử lý phiên kẹt khóa phân tán (`Redlock Timeout`) và cấm tự sinh kết quả hoàn thành giả tạo |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy trình xử lý và khôi phục khi phiên học bị kẹt hoặc khóa dở dang trong M03.

- **Cấm Tự sinh Kết quả Hoàn thành Giả tạo (`No Synthetic Completion Invariant`)**:
  - Khi một phiên học bị kẹt quá thời hạn (quá 24h hoặc bị lỗi hệ thống), Worker khôi phục CHỈ ĐƯỢC CHUYỂN trạng thái phiên sang `ABANDONED`.
  - Tuyệt đối CẤM tự động đánh dấu phiên kẹt thành `COMPLETED` hoặc tự tạo kết quả hoàn thành giả để tránh sai lệch báo cáo SRS và cấp thưởng lừa đảo.
- **Tự động Giải phóng Khóa Phân tán (`Distributed Lock Release SLA <= 30s`)**:
  - Các thao tác chốt phiên/gửi đáp án bị kẹt khóa phân tán Redis (`Redlock`) phải được tự động giải phóng sau tối đa $30\text{s}$ (Lock TTL expired).

## 2. Worker Khôi phục Phiên kẹt (Stuck Session Recovery Worker)

```csharp
public class StuckSessionRecoveryWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Quét các phiên IN_PROGRESS/PAUSED không có tương tác > 24 giờ
            var expiredSessions = await _sessionRepo.GetStuckSessionsAsync(TimeSpan.FromHours(24));
            
            foreach (var session in expiredSessions)
            {
                session.Status = SessionStatus.ABANDONED;
                session.AbandonedAtUtc = DateTime.UtcNow;
                session.AbandonReason = "AUTO_EVICTED_STUCK_24H";
                await _sessionRepo.UpdateAsync(session);
                
                _logger.LogInformation("Evicted stuck session {SessionId} for User {UserId}", session.SessionId, session.UserId);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LS-G01`: 100% phiên bị kẹt quá 24h chuyển sang trạng thái `ABANDONED`, không bao giờ thành `COMPLETED`.
- `LS-G02`: Khóa phân tán Redis hết hạn tự động giải phóng trong $\le 30\text{s}$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LS14-01` | Chạy `StuckSessionRecoveryWorker` quét phiên bị ngắt kết nối 25 giờ | Chuyển trạng thái sang `ABANDONED`, không phát event cấp thưởng. |
| `LS14-02` | Khai thác lỗi ngắt kết nối mạng đúng thời điểm bấm chốt phiên | Khóa Redis tự giải phóng sau 30s, người dùng có thể gửi lại an toàn. |
| `LS14-03` | Kiểm thử hoàn tất luồng M03-LOCKED-STUCK-SESSION-RECOVERY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-LS-F01` | Cần đăng ký `StuckSessionRecoveryWorker` vào sổ công việc nền M11 | Đảm bảo vận hành tự động theo dõi job | M11-T038 |

## 5. Tự kiểm M03-T014
- Đã hoàn thành đặc tả `M03-LOCKED-STUCK-SESSION-RECOVERY-1.0`.
- Chốt nguyên tắc cấm tự tạo kết quả hoàn thành giả và SLA giải phóng khóa 30s.
- Ghi nhận 2 Regression Gates (`LS-G01`–`LS-G02`) và 3 Test Cases (`LS14-01`–`LS14-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa xử lý phiên bị khóa hoặc kẹt M03-T014 | WSA-7K2 |
