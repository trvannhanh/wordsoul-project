# Thiết kế lịch sử và giải thích tiến độ M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-HISTORY-EXPLANATION-1.0` |
| Task | M07-T040 |
| Đầu vào | M07-QUEST-COMPLETION-STATE-TRANSITION-1.0 (M07-T026), M07-QUEST-REWARD-RECOVERY-RECONCILIATION-1.0 (M07-T034) |
| Phạm vi | API truy vấn lịch sử nhiệm vụ ngày (`GetQuestHistoryQuery`) và diễn giải minh bạch các mốc tăng tiến độ cho người học |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa API xem lại lịch sử và giải thích tiến độ nhiệm vụ ngày (`Quest History & Audit Trail Engine`) trong M07.

- **Bảo toàn Nhật ký Tiến độ Bất biến (`Immutable Quest Audit Log Invariant`)**:
  - 100% các mốc tăng tiến độ nhiệm vụ BẮT BUỘC lưu vết trong `UserQuestProgressLogs` kèm `TriggerEventId`, `IncrementValue`, `OccurredAtUtc` và `ExplanationMessage`.
- **Minh bạch Diễn giải Lý do Tăng Tiến độ (`Progress Explanation Rule`)**:
  - API trả về chuỗi diễn giải dễ hiểu cho người học (ví dụ: *"Tăng 1 tiến độ từ Phiên học lúc 08:30"*).

## 2. Cấu trúc Trả về Nhật ký Tiến độ (QuestHistoryItemDto)

```json
{
  "userQuestId": "uq_88123",
  "businessDayKey": "2026-08-22",
  "title": "Hoàn thành 10 từ vựng",
  "status": "CLAIMED",
  "claimedAtUtc": "2026-08-22T09:00:00Z",
  "progressLogs": [
    {
      "logId": "log_991",
      "occurredAtUtc": "2026-08-22T08:30:00Z",
      "increment": 5,
      "explanation": "Hoàn thành 5 từ vựng đúng lần đầu trong Phiên #1293"
    }
  ]
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QH-G01`: 100% bản ghi nhật ký tiến độ chứa chuỗi diễn giải `ExplanationMessage` hợp lệ.
- `QH-G02`: Nhật ký tiến độ nhiệm vụ từ chối $100\%$ các thao tác sửa đổi hoặc xóa nhật ký lịch sử.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QH40-01` | Learner xem lại nhật ký nhiệm vụ ngày hôm qua | API trả về 3 nhiệm vụ ngày hôm qua cùng chi tiết các mốc tăng tiến độ. |
| `QH40-02` | Learner thắc mắc tại sao phiên học không cộng điểm nhiệm vụ | API tra cứu log giải thích: "Phiên học bị hủy do dưới 60 giây". |
| `QH40-03` | Kiểm thử hoàn tất luồng M07-QUEST-HISTORY-EXPLANATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QH-F01` | Tạo API `GET /api/v1/quests/history` | Cho phép người học tra cứu lịch sử nhiệm vụ 30 ngày | M07-T026 |

## 5. Tự kiểm M07-T040
- Đã hoàn thành đặc tả `M07-QUEST-HISTORY-EXPLANATION-1.0`.
- Chốt schema nhật ký tiến độ bất biến và diễn giải lý do cho người học.
- Ghi nhận 2 Regression Gates (`QH-G01`–`QH-G02`) và 3 Test Cases (`QH40-01`–`QH40-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế lịch sử và giải thích tiến độ M07-T040 | WSA-7K2 |
