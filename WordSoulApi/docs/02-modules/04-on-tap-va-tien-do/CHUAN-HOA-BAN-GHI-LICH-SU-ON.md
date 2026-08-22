# Chuẩn hóa bản ghi lịch sử ôn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-LOG-SCHEMA-1.0` |
| Task | M04-T024 |
| Đầu vào | M04-IDEMPOTENT-RESULT-CONSUMPTION-1.0 (M04-T007), M04-QUALITY-RATING-POLICY-1.0 (M04-T011), M04-SRS-INTERVAL-CALCULATION-1.0 (M04-T016) |
| Phạm vi | Schema bảng nhật ký lịch sử ôn tập bất biến (`UserSenseProgressLogs`), bảo đảm lưu trữ đủ dữ liệu để tái dựng toàn bộ quá trình học tập |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa schema cấu trúc bản ghi lịch sử ôn tập (`UserSenseProgressLog`) trong M04.

- **Tính Bất biến của Bản ghi Lịch sử (`Append-Only History Invariant`)**:
  - Bảng `UserSenseProgressLogs` là dạng nhật ký ghi nối tiếp (Append-Only).
  - Tuyệt đối CẤM sửa đổi (`UPDATE`) hoặc xóa (`DELETE`) các bản ghi lịch sử đã sinh ra, trừ trường hợp tuân thủ quy trình xóa dữ liệu cá nhân GDPA M10.
- **Đầy đủ Dữ liệu Tái dựng Lịch sử (`Full Audit Trail Invariant`)**:
  - Mỗi bản ghi lịch sử BẮT BUỘC chứa các thuộc tính trước và sau khi thay đổi: `PreviousInterval`, `NewInterval`, `PreviousEaseFactor`, `NewEaseFactor`, `QualityRating`, `ReferenceSessionId`, `ReviewedAtUtc`.

## 2. Cấu trúc Schema Bảng Lịch sử Ôn (UserSenseProgressLog Schema)

```csharp
public class UserSenseProgressLog
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }
    public Guid VocabularySenseId { get; set; }
    public Guid ReferenceSessionId { get; set; }
    
    public int QualityRating { get; set; } // Điểm q in [0, 5]
    public int PreviousIntervalDays { get; set; }
    public int NewIntervalDays { get; set; }
    
    public double PreviousEaseFactor { get; set; }
    public double NewEaseFactor { get; set; }
    
    public MemoryState PreviousState { get; set; }
    public MemoryState NewState { get; set; }
    
    public DateTime ReviewedAtUtc { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RL-G01`: 100% lần cập nhật trạng thái SRS tạo ra đúng 1 bản ghi `UserSenseProgressLog` tương ứng.
- `RL-G02`: Bảng `UserSenseProgressLogs` không cấp quyền UPDATE/DELETE cho các API nghiệp vụ thông thường.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RL24-01` | Người học hoàn thành phiên ôn 1 từ vựng A | Ghi 1 bản ghi `UserSenseProgressLog` lưu chính xác `PreviousInterval`, `NewInterval`, `QualityRating`. |
| `RL24-02` | Thử gọi hàm UpdateDirectly vào table `UserSenseProgressLogs` | System từ chối, DB ném exception `APPEND_ONLY_TABLE_VIOLATION`. |
| `RL24-03` | Kiểm thử hoàn tất luồng M04-REVIEW-LOG-SCHEMA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-RL-F01` | Thêm index duy nhất `(UserId, ReferenceSessionId, VocabularySenseId)` | Chống ghi lặp log cho cùng 1 phiên | M04-T007 |

## 5. Tự kiểm M04-T024
- Đã hoàn thành đặc tả `M04-REVIEW-LOG-SCHEMA-1.0`.
- Chốt schema nhật ký lịch sử ôn append-only và nguyên tắc bảo toàn dữ liệu tái dựng.
- Ghi nhận 2 Regression Gates (`RL-G01`–`RL-G02`) và 3 Test Cases (`RL24-01`–`RL24-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa bản ghi lịch sử ôn M04-T024 | WSA-7K2 |
