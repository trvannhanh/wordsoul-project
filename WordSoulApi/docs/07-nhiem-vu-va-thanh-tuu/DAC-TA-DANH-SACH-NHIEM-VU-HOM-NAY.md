# Đặc tả danh sách nhiệm vụ hôm nay M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-TODAY-QUEST-LIST-SPEC-1.0` |
| Task | M07-T038 |
| Đầu vào | M07-QUEST-UNIQUE-ASSIGNMENT-1.0 (M07-T010), M07-QUEST-CYCLE-RESET-SPEC-1.0 (M07-T025), M07-QUEST-COMPLETION-STATE-TRANSITION-1.0 (M07-T026) |
| Phạm vi | API truy vấn danh sách nhiệm vụ trong ngày `GetTodayQuestsQuery` cho App Mobile, phản ánh thời thực tiến độ và trạng thái nhận thưởng |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa API danh sách nhiệm vụ ngày (`Today Quest List API`) cho ứng dụng người học trong M07.

- **Tính Đầy đủ và Nhất quán của Tập Nhiệm vụ Ngày (`Complete 3-Quest Bundle Invariant`)**:
  - API BẮT BUỘC trả về chính xác $3$ nhiệm vụ được phân bổ cho ngày nghiệp vụ hiện tại (`BusinessDayKey`).
  - Nếu hệ thống chưa phân bổ (ví dụ: người dùng mới đăng nhập lần đầu trong ngày), API BẮT BUỘC tự động kích hoạt luồng phân bổ duy nhất `M07-T010` trước khi trả kết quả.
- **Tính Phản ánh Tiến độ Thời thực (`Real-Time Progress Accuracy Rule`)**: Trạng thái nhiệm vụ (`IN_PROGRESS`, `COMPLETED_UNCLAIMED`, `CLAIMED`) BẮT BUỘC đồng bộ thời thực theo sự kiện tích lũy mới nhất.

## 2. Cấu trúc Trả về API Danh sách Nhiệm vụ Hôm nay (TodayQuestListResponse)

```json
{
  "businessDayKey": "2026-08-22",
  "expiresAtUtc": "2026-08-22T23:59:59Z",
  "quests": [
    {
      "userQuestId": "uq_88123",
      "title": "Hoàn thành 1 phiên học mới",
      "currentProgress": 1,
      "targetCount": 1,
      "status": "COMPLETED_UNCLAIMED",
      "reward": { "gold": 100, "exp": 50 }
    }
  ]
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TQ-G01`: 100% phản hồi từ `GetTodayQuestsQuery` chứa đúng 3 phần tử nhiệm vụ hợp lệ.
- `TQ-G02`: Thao tác chuyển đổi múi giờ không làm thay đổi kết quả danh sách nhiệm vụ của ngày nghiệp vụ hiện tại.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TQ38-01` | Learner A vừa mở app lần đầu tiên trong ngày 2026-08-22 | System tự động phân bổ 3 nhiệm vụ ngày, API trả về 3 item trạng thái `IN_PROGRESS`. |
| `TQ38-02` | Learner A đã học xong 1 nhiệm vụ | API trả về 1 item `COMPLETED_UNCLAIMED`, 2 item `IN_PROGRESS`. |
| `TQ38-03` | Kiểm thử hoàn tất luồng M07-TODAY-QUEST-LIST-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-TQ-F01` | Tích hợp Redis Caching cho API `GetTodayQuestsQuery` với TTL 5 phút | Giảm tải DB khi client pull danh sách nhiều lần | M07-T010 |

## 5. Tự kiểm M07-T038
- Đã hoàn thành đặc tả `M07-TODAY-QUEST-LIST-SPEC-1.0`.
- Chốt cấu trúc Response 3 nhiệm vụ ngày và luồng tự động phân bổ duy nhất.
- Ghi nhận 2 Regression Gates (`TQ-G01`–`TQ-G02`) and 3 Test Cases (`TQ38-01`–`TQ38-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả danh sách nhiệm vụ hôm nay M07-T038 | WSA-7K2 |
