# Đặc tả đơn vị đếm ôn tập M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-REVIEW-UNIT-SPEC-1.0` |
| Task | M07-T018 |
| Đầu vào | M07-QUEST-EVENT-CATALOG-1.0 (M07-T011), M03-REVIEW-FLOW-1.0 (M03-T016) |
| Phạm vi | Quy tắc đếm tiến độ cho các nhiệm vụ ngày yêu cầu ôn tập số lượng mục từ vựng đến hạn (`ITEMS_REVIEWED`) |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định logic đếm tiến độ nhiệm vụ cho các hoạt động ôn tập từ vựng.

- **Chỉ Đếm Bằng chứng Gợi nhớ Đúng Lần 1 (`First-Try Correct Review Invariant`)**: Tiến độ `ITEMS_REVIEWED` CHỈ ĐƯỢC CỘNG cho các từ vựng mà người học trả lời ĐÚNG ở lần thử đầu tiên (`IsCorrectFirstTry == true`). Thử lại nhiều lần trong cùng phiên chỉ đếm là 1 từ.
- **Tính Phù hợp với Hàng đợi Ôn (`Queue Alignment Invariant`)**: Chỉ các mục từ đến hạn ôn từ M04 được tính vào tiến độ nhiệm vụ ôn tập ngày.

## 2. Dynamic Review Unit Counter Rule

```csharp
public int CountValidReviewItems(LearningSessionCompletedEvent evt)
{
    if (evt.SessionType != SessionType.REVIEW) return 0;
    
    // Đếm số từ trả lời đúng ngay từ lần 1
    int validCount = evt.InitialRecallResults
        .Count(r => r.IsCorrectFirstTry);
        
    return validCount;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RU-G01`: 100% mục từ trả lời sai ở lần 1 không làm tăng tiến độ nhiệm vụ `ITEMS_REVIEWED`.
- `RU-G02`: Nhiệm vụ ôn tập được đếm chính xác số từ trả lời đúng lần 1.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RU18-01` | Phiên ôn 15 từ: 12 từ đúng lần 1, 3 từ sai lần 1 | Tiến độ nhiệm vụ "Ôn tập từ vựng" tăng đúng $+12$. |
| `RU18-02` | Làm bài ôn lại tự do không qua M04 queue | Tiến độ không tăng. |
| `RU18-03` | Kiểm thử hoàn tất luồng M07-REVIEW-UNIT-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-RU-F01` | Cần thuộc tính `IsCorrectFirstTry` trong DTO event phát ra từ M03 | Đảm bảo M07 lọc đúng từ hợp lệ | M07-T022 |

## 5. Tự kiểm M07-T018
- Đã đặc tả đơn vị đếm ôn tập M07-T018.
- Ghi nhận 2 Regression Gates (`RU-G01`–`RU-G02`) và 3 Test Cases (`RU18-01`–`RU18-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả đặc tả đơn vị đếm ôn tập M07-T018 | WSA-7K2 |
