# Thiết kế ghi kết quả gợi nhớ đầu M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-INITIAL-RECALL-CAPTURE-1.0` |
| Task | M03-T034 |
| Đầu vào | M03-REVIEW-FLOW-1.0 (M03-T016), M04-USER-SENSE-UNIT-1.0 (M04-T002) |
| Phạm vi | Cơ chế chụp và ghi nhận kết quả phản hồi ĐẦU TIÊN (`InitialRecallResult`) của mỗi từ vựng trong phiên để làm dữ liệu đầu vào cho M04 SRS |
| Tự kiểm | B-G01, B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định logic capture kết quả gợi nhớ đầu tiên của mỗi mục từ vựng trong phiên học/ôn.

- **Một Lần Ghi Nhất quán (`Single Capture Invariant`)**: Mỗi mục từ vựng trong 1 phiên học CHỈ có duy nhất 1 bản ghi `InitialRecallResult`. Lần làm lại thứ 2, 3 sau khi trả lời sai KHÔNG ĐƯỢC làm ghi đè hay thay đổi bản ghi này.
- **Tính Đầy đủ Dữ liệu Ghi nhớ (`Recall Data Completeness Invariant`)**: Bản ghi `InitialRecallResult` BẮT BUỘC chứa các trường: `VocabularySenseId`, `IsCorrect` (lần 1), `ResponseDurationMs` (lần 1), `UsedHint` (có dùng gợi ý không).

## 2. Dynamic Initial Recall Record Envelope

```csharp
public class InitialRecallResultRecord
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid VocabularySenseId { get; set; }
    
    public bool IsCorrectFirstTry { get; set; }
    public long ResponseDurationMs { get; set; }
    public bool UsedHint { get; set; }
    
    public DateTime CapturedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IR-G01`: Lần làm lại câu hỏi sau khi làm sai KHÔNG tạo thêm bản ghi `InitialRecallResultRecord` mới.
- `IR-G02`: 100% bản ghi gợi nhớ đầu tiên chứa `ResponseDurationMs >= 200ms`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IR34-01` | Người học trả lời sai lần 1, sau đó thử lại đúng ở lần 2 | `InitialRecallResultRecord` lưu `IsCorrectFirstTry = false`. |
| `IR34-02` | Người học trả lời đúng ngay lần 1 trong 1.5s | `InitialRecallResultRecord` lưu `IsCorrectFirstTry = true`, `ResponseDurationMs = 1500`. |
| `IR34-03` | Kiểm thử hoàn tất luồng M03-INITIAL-RECALL-CAPTURE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-IR-F01` | Cần bảng tạm `SessionInitialRecallRecords` trong Redis cache phiên | Lưu trữ trước khi chốt phiên và phát event | M03-T035 |

## 5. Tự kiểm M03-T034
- Đã đặc tả thiết kế ghi kết quả gợi nhớ đầu M03-T034.
- Ghi nhận 2 Regression Gates (`IR-G01`–`IR-G02`) và 3 Test Cases (`IR34-01`–`IR34-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế ghi kết quả gợi nhớ đầu M03-T034 | WSA-7K2 |
