# Đặc tả mục tiêu chất lượng M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUALITY-TARGET-SPEC-1.0` |
| Task | M07-T019 |
| Đầu vào | M07-QUEST-TARGET-SPEC-1.0 (M07-T003), M07-QUEST-EVENT-CATALOG-1.0 (M07-T011) |
| Phạm vi | Quy tắc tính toán tiến độ cho các loại nhiệm vụ yêu cầu chất lượng học tập (`Quality Quest Target Spec`), ngưỡng độ chính xác tối thiểu (ví dụ: `FirstTryAccuracy >= 80%`) và loại trừ phiên học bất thường |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc tính toán và kiểm tra cho các loại nhiệm vụ yêu cầu đạt ngưỡng chất lượng học tập (`Quality Quest Targets`) trong M07.

- **Ngưỡng Chất lượng Bắt buộc (`Quality Threshold Invariant`)**:
  - Tín hiệu sự kiện `LearningSessionCompletedIntegrationEvent` CHỈ ĐƯỢC TÍNH TIẾN ĐỘ cho nhiệm vụ chất lượng nếu thỏa mãn đồng thời 2 điều kiện:
    1. Độ chính xác gợi nhớ lần 1 `FirstTryAccuracy` $\ge \text{TargetMinAccuracy}$ (ví dụ $\ge 80.0\%$).
    2. Thời gian làm bài thực tế `ActiveDurationSeconds` $\ge \text{TargetMinDurationSeconds}$ (chặn các phiên làm siêu nhanh/gian lận).
- **Loại trừ Phiên Bị Cảnh báo Bất thường (`Abnormal Session Exclusion Rule`)**:
  - 100% phiên học bị gắn cờ `SUSPICIOUS_SPEED_SESSION` hoặc `ABNORMAL_BOT_SPEED` bị loại trừ khỏi tiến độ nhiệm vụ chất lượng.

## 2. Bảng Ma trận Tiêu chí Nhiệm vụ Chất lượng (Quality Target Matrix)

| Loại Mục tiêu Nhiệm vụ | Ngưỡng Chính xác Lần 1 | Ngưỡng Thời gian Tối thiểu | Tác động Phiên Gian lận |
|---|---|---|---|
| `PERFECT_SESSION` | $100.0\%$ | $\ge 30$ giây | **Loại trừ (Không tính)** |
| `HIGH_ACCURACY_SESSION` | $\ge 80.0\%$ | $\ge 45$ giây | **Loại trừ (Không tính)** |
| `SPEED_RECALL_SESSION` | $\ge 90.0\%$ | $[15\text{s}, 60\text{s}]$ | **Loại trừ (Không tính)** |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QT-G01`: 100% phiên học có `FirstTryAccuracy < 80%` không được tính tiến độ cho nhiệm vụ `HIGH_ACCURACY_SESSION`.
- `QT-G02`: Phiên học có cờ `SUSPICIOUS_SPEED_SESSION = true` bị chặn 100% không tăng tiến độ nhiệm vụ chất lượng.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QT19-01` | Learner hoàn thành phiên học với `FirstTryAccuracy = 85.0%` trong 60s | Cập nhật tăng `CurrentProgressCount` thêm 1 cho nhiệm vụ `HIGH_ACCURACY_SESSION`. |
| `QT19-02` | Learner đạt `FirstTryAccuracy = 100%` nhưng làm trong 4s (bị cờ `SUSPICIOUS_SPEED`) | System loại trừ phiên này, `CurrentProgressCount` giữ nguyên 0. |
| `QT19-03` | Kiểm thử hoàn tất luồng M07-QUALITY-TARGET-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QT-F01` | Kiểm tra thuộc tính `IsSuspiciousSpeed` từ payload sự kiện M03 | Đảm bảo tính trung thực của tiến độ nhiệm vụ | M07-T012 |

## 5. Tự kiểm M07-T019
- Đã hoàn thành đặc tả `M07-QUALITY-TARGET-SPEC-1.0`.
- Chốt nguyên tắc điều kiện kép (Chính xác + Thời gian) và loại trừ phiên bất thường.
- Ghi nhận 2 Regression Gates (`QT-G01`–`QT-G02`) và 3 Test Cases (`QT19-01`–`QT19-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả mục tiêu chất lượng M07-T019 | WSA-7K2 |
