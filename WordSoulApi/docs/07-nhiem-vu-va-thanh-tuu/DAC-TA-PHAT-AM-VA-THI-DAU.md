# Đặc tả phát âm và thi đấu M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-PRONUNCIATION-PVP-QUEST-SPEC-1.0` |
| Task | M07-T020 |
| Đầu vào | M07-QUEST-TARGET-SPEC-1.0 (M07-T003), M07-QUEST-EVENT-CATALOG-1.0 (M07-T011) |
| Phạm vi | Đặc tả các loại mục tiêu nhiệm vụ phát âm (`Pronunciation Quests`) và thi đấu (`Arena/PvP Quests`), bao gồm tiêu chí xác nhận độ chính xác phát âm $\ge 80.0\%$ và tính hợp lệ trận thi đấu |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc tính tiến độ cho các loại nhiệm vụ phát âm (`Pronunciation Target`) và thi đấu (`Arena PvP Target`) trong M07.

- **Ngưỡng Độ chính xác Phát âm Bắt buộc (`Pronunciation Accuracy Rule`)**:
  - Sự kiện phát âm `PronunciationAttemptCompletedEvent` CHỈ ĐƯỢC TÍNH TIẾN ĐỘ cho nhiệm vụ khi điểm chấm phát âm `PronunciationScore >= 80.0`.
- **Trận Thi đấu Hợp lệ Không Đầu hàng Nhanh (`Valid PvP Match Invariant`)**:
  - Sự kiện thi đấu `ArenaMatchCompletedEvent` CHỈ ĐƯỢC TÍNH TIẾN ĐỘ khi trận đấu kéo dài trên $15$ giây và không bị đánh dấu bỏ cuộc/đầu hàng bất thường.

## 2. Bảng Ma trận Tiêu chí Nhiệm vụ Phát âm và Thi đấu (Pronunciation & PvP Matrix)

| Mã Loại Mục tiêu | Điều kiện Tiến độ | Nguồn Sự kiện Trigger | Chống Khai thác / Anti-Cheat |
|---|---|---|---|
| `PRONUNCIATION_PERFECT` | `PronunciationScore >= 85.0` | `PronunciationAttemptCompletedEvent` | Chặn audio thu âm sẵn $< 1.0\text{s}$ |
| `ARENA_WIN_MATCH` | `IsWinner == true` & Duration $> 15\text{s}$ | `ArenaMatchCompletedEvent` | Bỏ qua trận đấu với bot giả lập |
| `ARENA_PARTICIPATE` | Complete match & Duration $> 15\text{s}$ | `ArenaMatchCompletedEvent` | Bỏ qua trận đầu hàng $< 5\text{s}$ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PP-G01`: 100% lượt đọc phát âm có `PronunciationScore < 80.0` không được cộng tiến độ nhiệm vụ.
- `PP-G02`: Trận Arena PvP đầu hàng dưới 15 giây bị loại trừ $100\%$ khỏi tiến độ nhiệm vụ thi đấu.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PP20-01` | Learner phát âm từ A đạt `PronunciationScore = 88.0` | Cập nhật tăng `CurrentProgressCount` thêm 1 cho nhiệm vụ phát âm. |
| `PP20-02` | Learner tham gia trận Arena đấu bot, thắng trong 3 giây | System loại trừ trận đấu này, `CurrentProgressCount` giữ nguyên. |
| `PP20-03` | Kiểm thử hoàn tất luồng M07-PRONUNCIATION-PVP-QUEST-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-PP-F01` | Lắng nghe event `PronunciationAttemptCompletedEvent` từ M08/M03 | Cập nhật tiến độ nhiệm vụ phát âm bất đồng bộ | M07-T011 |

## 5. Tự kiểm M07-T020
- Đã hoàn thành đặc tả `M07-PRONUNCIATION-PVP-QUEST-SPEC-1.0`.
- Chốt ngưỡng phát âm $\ge 80.0\%$ và thời lượng trận đấu $> 15$ giây chống đầu hàng farm điểm.
- Ghi nhận 2 Regression Gates (`PP-G01`–`PP-G02`) và 3 Test Cases (`PP20-01`–`PP20-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả phát âm và thi đấu M07-T020 | WSA-7K2 |
