# Chuẩn hóa nội dung tổng kết phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-SUMMARY-CONTENT-1.0` |
| Task | M03-T039 |
| Đầu vào | M03-ITEM-COMPLETION-RESULT-1.0 (M03-T035), M03-SINGLE-FINALIZATION-GUARANTEE-1.0 (M03-T038) |
| Phạm vi | Chuẩn hóa DTO dữ liệu màn hình tổng kết phiên (`SessionSummaryDto`), bao gồm số từ thuộc ngay, số từ cần ôn lại, tổng thời gian làm bài thực tế và phần thưởng tạm tính |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa cấu trúc DTO nội dung hiển thị trên màn hình tổng kết sau khi hoàn thành phiên học trong M03.

- **Phân biệt Rõ ràng Thuộc từ vs Hoàn thành Phiên (`Mastery vs Session Summary Invariant`)**:
  - Màn hình tổng kết phiên CHỈ ĐƯỢC DÙNG thuật ngữ "Hoàn thành phiên" (`Session Completed`) và "Độ chính xác lượt đầu" (`First-Try Accuracy`).
  - Tuyệt đối CẤM hiển thị thông điệp "Bạn đã thành thạo các từ này" trên màn hình tổng kết M03, vì việc đánh giá thành thạo thuộc thẩm quyền độc quyền của SRS M04.
- **Tính Bất biến của DTO Tổng kết (`Idempotent Summary Envelope Invariant`)**:
  - Request lấy kết quả tổng kết gọi lại nhiều lần BẮT BUỘC trả về cùng 1 đối tượng `SessionSummaryDto` duy nhất.

## 2. Cấu trúc DTO Màn hình Tổng kết Phiên (SessionSummaryDto Structure)

```csharp
public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public SessionType Type { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    
    public int TotalItemsCount { get; set; }
    public int FirstTryCorrectCount { get; set; }
    public double FirstTryAccuracyPercentage { get; set; }
    public int ActiveDurationSeconds { get; set; }
    
    public List<SessionSummaryItemDto> Items { get; set; }
    public PendingRewardSummaryDto PendingRewards { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SS-G01`: 100% `SessionSummaryDto` hiển thị chính xác chỉ số `FirstTryAccuracyPercentage` tách biệt với `CompletionRate`.
- `SS-G02`: Gọi API lấy summary 5 lần liên tiếp trả về cùng 1 kết quả `SessionSummaryDto` không thay đổi.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SS39-01` | Hoàn thành phiên 10 từ, đúng ngay lần 1 được 7 từ | UI hiển thị "Độ chính xác lượt đầu: 70%", không dùng từ "Thành thạo". |
| `SS39-02` | Người dùng vừa nhận màn tổng kết, bấm refresh lại trang web | System trả về đúng `SessionSummaryDto` đã lưu trong `FinalSummaryJson`. |
| `SS39-03` | Kiểm thử hoàn tất luồng M03-SESSION-SUMMARY-CONTENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SS-F01` | Lưu `FinalSummaryJson` vào bảng `LearningSessions` khi finalization | Cho phép trả về kết quả nhanh không cần tính lại | M03-T038 |

## 5. Tự kiểm M03-T039
- Đã hoàn thành đặc tả `M03-SESSION-SUMMARY-CONTENT-1.0`.
- Chốt DTO màn hình tổng kết phiên và thuật ngữ phân định với M04 SRS.
- Ghi nhận 2 Regression Gates (`SS-G01`–`SS-G02`) và 3 Test Cases (`SS39-01`–`SS39-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa nội dung tổng kết phiên M03-T039 | WSA-7K2 |
