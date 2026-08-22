# Thiết kế tiếp tục phiên chưa hoàn thành M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-RESUME-SESSION-1.0` |
| Task | M03-T010 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-SESSION-CONTENT-SNAPSHOT-1.0 (M03-T007), M01-SESSION-1.0 (M01-T016) |
| Phạm vi | Quy trình tiếp tục phiên học dở (`Resume Session`), khôi phục đúng bước hiện tại (`CurrentStepIndex`), bảo đảm đúng chủ sở hữu và xử lý hết hạn |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình tiếp tục một phiên học chưa hoàn thành (`Resume Session`) trong M03.

- **Chỉ Đúng Chủ sở hữu Tiếp tục (`Session Owner Authorization Invariant`)**:
  - CHỈ người học tạo phiên (`Session.UserId == CurrentUserId`) mới có quyền truy cập tiếp tục phiên. Yêu cầu tiếp tục từ tài khoản khác bị chặn ngay lập tức (HTTP 403 `FORBIDDEN_SESSION_ACCESS`).
- **Khôi phục Chính xác Bước dở (`Exact Step Recovery Invariant`)**:
  - API Resume trả về đúng `CurrentStepIndex`, giữ nguyên các câu đã làm đúng/sai trước đó và tiếp tục sử dụng `SessionSnapshotJson` ban đầu. CẤM sinh lại danh sách câu hỏi hoặc reset về bước 1.
- **Tự động Hủy nếu Hết hạn (`Expiration Eviction Rule`)**:
  - Nếu phiên dở có `CreatedAtUtc` quá 24 giờ, hệ thống tự động chuyển trạng thái phiên thành `ABANDONED` và từ chối tiếp tục (HTTP 410 `SESSION_EXPIRED_ABANDONED`).

## 2. DTO Tiếp tục Phiên học (Resume Session DTO Envelope)

```csharp
public class ResumeSessionResponseDto
{
    public Guid SessionId { get; set; }
    public SessionType SessionType { get; set; }
    public int CurrentStepIndex { get; set; }
    public int TotalSteps { get; set; }
    public SessionSnapshotDto Snapshot { get; set; }
    public List<StepProgressSummaryDto> CompletedSteps { get; set; } = new();
    public DateTime ExpiresAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RS-G01`: 100% request tiếp tục phiên của người dùng khác bị chối bỏ với lỗi HTTP 403.
- `RS-G02`: Phiên dở khôi phục đúng `CurrentStepIndex` và giữ nguyên lịch sử làm bài trước đó.
- `RS-G03`: Phiên quá hạn 24h tự động chuyển `ABANDONED` khi người dùng bấm tiếp tục.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RS10-01` | Learner A thoát ứng dụng ở bước 5/10, sau đó mở lại app bấm tiếp tục | Trả về DTO phiên dở với `CurrentStepIndex = 5`, 4 bước trước đã ghi nhận. |
| `RS10-02` | Learner B thử gọi API resume phiên của Learner A | System từ chối với lỗi HTTP 403 `FORBIDDEN_SESSION_ACCESS`. |
| `RS10-03` | Bấm tiếp tục phiên đã tạo cách đây 25 giờ ($> 24\text{h}$) | Trả lỗi HTTP 410 `SESSION_EXPIRED_ABANDONED`, chuyển trạng thái `ABANDONED`. |
| `RS10-04` | Kiểm thử hoàn tất luồng M03-RESUME-SESSION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-RS-F01` | Cần lưu `LastInteractedAtUtc` khi tiếp tục phiên | Đảm bảo tính toán chính xác thời gian tạm dừng | M03-T011 |

## 5. Tự kiểm M03-T010
- Đã hoàn thành đặc tả `M03-RESUME-SESSION-1.0`.
- Chốt nguyên tắc phân quyền chủ sở hữu và khôi phục bước dở chính xác.
- Ghi nhận 3 Regression Gates (`RS-G01`–`RS-G03`) và 4 Test Cases (`RS10-01`–`RS10-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế tiếp tục phiên chưa hoàn thành M03-T010 | WSA-7K2 |
