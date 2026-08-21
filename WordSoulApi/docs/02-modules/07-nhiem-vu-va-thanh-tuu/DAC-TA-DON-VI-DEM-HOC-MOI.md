# Đặc tả đơn vị đếm học mới M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-NEW-LEARNING-UNIT-SPEC-1.0` |
| Task | M07-T017 |
| Đầu vào | M07-QUEST-EVENT-CATALOG-1.0 (M07-T011), M03-NEW-LEARNING-FLOW-1.0 (M03-T015) |
| Phạm vi | Quy tắc chi tiết ghi nhận tiến độ cho các nhiệm vụ đếm theo phiên học mới (`NewLearningSession`) |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định các tiêu chí chấp nhận khi đếm tiến độ nhiệm vụ liên quan đến hoạt động học từ vựng mới.

- **Chỉ Tính Phiên Hoàn thành (`Completed Session Requirement Invariant`)**: CHỈ các phiên học mới có `SessionState == COMPLETED` mới được đếm `+1` vào tiến độ nhiệm vụ `SESSIONS_COMPLETED`. Phiên bị tạm dừng (`PAUSED`), bỏ dở (`ABANDONED`) hoặc quá hạn tuyệt đối CẤM đếm.
- **Không Đếm Lặp từ Học lại (`Unique Sense Counter Invariant`)**: Học lại một từ đã có hồ sơ SRS không được tính là "Từ học mới". Tiến độ chỉ tăng khi `FirstLearnedAtUtc` được khởi tạo mới trong ngày.

## 2. Dynamic New Learning Unit Counter Rule

```csharp
public bool ShouldCountNewLearningSession(LearningSessionCompletedEvent evt)
{
    // 1. Kiểm tra đúng loại phiên học mới
    if (evt.SessionType != SessionType.NEW_LEARNING) return false;
    
    // 2. Kiểm tra độ bao phủ phiên đạt 100%
    if (evt.Summary.AccuracyPercentage < 0.0 || evt.Summary.TotalWords <= 0) return false;
    
    return true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `NL-G01`: 100% phiên học mới bị `ABANDONED` không được tính vào tiến độ nhiệm vụ đếm phiên.
- `NL-G02`: Chỉ phiên có `SessionType == NEW_LEARNING` mới làm tăng chỉ số nhiệm vụ học mới.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `NL17-01` | Hoàn thành 1 phiên học mới 10 từ | Nhiệm vụ "Hoàn thành 1 phiên học mới" tăng tiến độ $+1$. |
| `NL17-02` | Bỏ dở phiên học mới giữa chừng | Tiến độ nhiệm vụ giữ nguyên, không tăng. |
| `NL17-03` | Kiểm thử hoàn tất luồng M07-NEW-LEARNING-UNIT-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-NL-F01` | Bổ sung check `SessionType` trong Quest Consumer | Phân loại đúng loại phiên khi tăng counter | M07-T018 |

## 5. Tự kiểm M07-T017
- Đã đặc tả đơn vị đếm học mới M07-T017.
- Ghi nhận 2 Regression Gates (`NL-G01`–`NL-G02`) và 3 Test Cases (`NL17-01`–`NL17-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả đơn vị đếm học mới M07-T017 | WSA-7K2 |
