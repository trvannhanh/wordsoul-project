# Thiết kế chốt phiên đúng một lần M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SINGLE-FINALIZATION-GUARANTEE-1.0` |
| Task | M03-T038 |
| Đầu vào | M03-SUBMIT-IDEMPOTENCY-1.0 (M03-T025), M03-SESSION-COMPLETION-CONDITIONS-1.0 (M03-T037) |
| Phạm vi | Đảm bảo tính duy nhất bất biến của thao tác chốt phiên học (`Single Finalization Guarantee`), chống phát thưởng nhân bản M06/M07 khi retry API |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế khóa bất biến để đảm bảo 1 phiên học CHỈ ĐƯỢC CHỐT VÀ PHÁT SỰ KIỆN ĐÚNG 1 LẦN DUY NHẤT.

- **Tính Duy nhất của Bản ghi Chốt phiên (`Single Finalization Record Invariant`)**:
  - Thao tác chốt phiên `FinalizeSession` BẮT BUỘC thực hiện dưới một giao dịch CSDL nguyên tử (Atomic Database Transaction).
  - Khi phiên đã chốt (`IsFinalized == true`), mọi lệnh gọi `FinalizeSession` tiếp theo BẮT BUỘC trả về ngay DTO kết quả tổng kết cũ mà KHÔNG sinh thêm giao dịch phát thưởng hoặc phát lại sự kiện hoàn thành.

## 2. Dynamic Session Summary DTO Envelope

```csharp
public class SessionSummaryDto
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public SessionType Type { get; set; }
    
    public int TotalWords { get; set; }
    public int CorrectFirstTryCount { get; set; }
    public double AccuracyPercentage { get; set; }
    public long TotalDurationSeconds { get; set; }
    
    public int GoldEarned { get; set; }
    public int ExpEarned { get; set; }
    
    public DateTime FinalizedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SF-G01`: 100% lệnh gọi `FinalizeSession` lặp lại trả về HTTP 200 kèm `SessionSummaryDto` cũ, không làm tăng thêm Gold/Exp người dùng.
- `SF-G02`: Sự kiện `LearningSessionCompletedEvent` chỉ được phát ra Event Bus đúng 1 lần.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SF38-01` | Gọi API chốt phiên 3 lần liên tiếp do chập chờn mạng | Lần 1 chốt phiên & cấp thưởng, Lần 2-3 trả kết quả đã chốt, số dư Gold không đổi. |
| `SF38-02` | Kiểm tra Event Bus sau 3 lần gọi chốt | Chỉ có 1 message `LearningSessionCompletedEvent` xuất hiện trong queue. |
| `SF38-03` | Kiểm thử hoàn tất luồng M03-SINGLE-FINALIZATION-GUARANTEE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SF-F01` | Thêm thuộc tính `IsFinalized` và Unique Index `(SessionId)` trong DB | Đảm bảo ràng buộc chốt 1 lần ở cấp CSDL | M03-T040 |

## 5. Tự kiểm M03-T038
- Đã đặc tả thiết kế chốt phiên đúng một lần M03-T038.
- Ghi nhận 2 Regression Gates (`SF-G01`–`SF-G02`) và 3 Test Cases (`SF38-01`–`SF38-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế chốt phiên đúng một lần M03-T038 | WSA-7K2 |
