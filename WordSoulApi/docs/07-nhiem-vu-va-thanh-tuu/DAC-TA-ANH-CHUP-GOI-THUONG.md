# Đặc tả ảnh chụp gói thưởng M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-REWARD-PACKAGE-SNAPSHOT-1.0` |
| Task | M07-T031 |
| Đầu vào | M07-QUEST-TARGET-SPEC-1.0 (M07-T003), M07-QUEST-REWARD-CLAIM-CONDITIONS-1.0 (M07-T030) |
| Phạm vi | Cấu trúc thuộc tính ảnh chụp gói thưởng (`Reward Package Snapshot JSON`), bảo lưu chính xác lượng Gold/Exp đã cam kết tại thời điểm phân bổ nhiệm vụ |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả thuộc tính lưu trữ ảnh chụp gói thưởng (`Reward Package Snapshot Schema`) trong M07.

- **Bảo lưu Cam kết Phần thưởng Bất biến (`Immutable Reward Snapshot Invariant`)**:
  - Thuộc tính `RewardPackageSnapshotJson` BẮT BUỘC được tạo và đóng băng ngay tại thời điểm phân bổ `UserQuest`.
  - Mọi thay đổi về cấu hình phần thưởng của định nghĩa nhiệm vụ sau thời điểm phân bổ KHÔNG ĐƯỢC PHÉP thay đổi nội dung gói thưởng trong `UserQuest`.
- **Phương án Thay thế khi Vật phẩm Ngừng dùng (`Discontinued Reward Fallback Rule`)**: Nếu vật phẩm nằm trong gói thưởng bị ngừng dùng (`DISCONTINUED`) trước khi bấm nhận, M07 tự động quy đổi vật phẩm đó sang giá trị Gold tương đương.

## 2. Cấu trúc JSON Ảnh chụp Gói thưởng (RewardPackageSnapshotJson Schema)

```json
{
  "snapshotCreatedAtUtc": "2026-08-22T00:00:00Z",
  "goldAmount": 100,
  "expAmount": 50,
  "items": [
    {
      "itemDefinitionId": "item_streak_saver_01",
      "quantity": 1,
      "goldFallbackValue": 50
    }
  ]
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RS-G01`: 100% bản ghi `UserQuest` được khởi tạo có chuỗi `RewardPackageSnapshotJson` hợp lệ.
- `RS-G02`: Thay đổi giá trị thưởng trong `QuestDefinition` không làm lệch $100\%$ giá trị trong `RewardPackageSnapshotJson`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RS31-01` | Learner được giao nhiệm vụ A thưởng 100 Gold. Admin sửa nhiệm vụ A thành 50 Gold | Learner bấm nhận thưởng vẫn nhận đủ 100 Gold theo snapshot. |
| `RS31-02` | Gói thưởng chứa vật phẩm X đã bị ngừng dùng | M07 đọc cờ fallback, cấp 50 Gold thay cho vật phẩm X. |
| `RS31-03` | Kiểm thử hoàn tất luồng M07-REWARD-PACKAGE-SNAPSHOT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-RS-F01` | Thêm thuộc tính `RewardPackageSnapshotJson` vào `UserQuest` Entity | Bảo đảm cam kết phần thưởng bất biến | M07-T003 |

## 5. Tự kiểm M07-T031
- Đã hoàn thành đặc tả `M07-REWARD-PACKAGE-SNAPSHOT-1.0`.
- Chốt schema ảnh chụp gói thưởng JSON và quy tắc fallback vật phẩm bị ngừng dùng.
- Ghi nhận 2 Regression Gates (`RS-G01`–`RS-G02`) và 3 Test Cases (`RS31-01`–`RS31-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả ảnh chụp gói thưởng M07-T031 | WSA-7K2 |
