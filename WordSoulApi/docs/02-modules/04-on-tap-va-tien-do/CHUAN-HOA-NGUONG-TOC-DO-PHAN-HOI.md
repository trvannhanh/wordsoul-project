# Chuẩn hóa ngưỡng tốc độ phản hồi M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-RESPONSE-SPEED-THRESHOLDS-1.0` |
| Task | M04-T012 |
| Đầu vào | M03-SUBMIT-ANSWER-DATA-1.0 (M03-T024), M04-QUALITY-RATING-POLICY-1.0 (M04-T011) |
| Phạm vi | Các ngưỡng thời gian phản hồi (`Response Duration Thresholds`), kẹp kẹp thời gian siêu nhanh bất thường ($< 200\text{ms}$) và xử lý quá hạn phản hồi ($> 60\text{s}$) |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định các ngưỡng thời gian làm bài (`Response Speed Thresholds`) để phục vụ tính điểm chất lượng SRS trong M04.

- **Kẹp Thời gian Phản hồi Bất thường (`Duration Clamping Invariant`)**:
  - *Thời gian siêu ngắn ($Duration < 200\text{ms}$)*: Hệ thống coi là bot/auto-clicker bất thường, kẹp thời gian về $200\text{ms}$ và gắn nhãn nghi vấn `SUSPICIOUS_SPEED`.
  - *Thời gian siêu dài ($Duration > 60\text{s}$)*: Hệ thống kẹp thời gian về mức trần $60\text{s}$ để tránh làm méo mó các chỉ số trung bình do người học treo máy.
- **Phân loại Ngưỡng theo Dạng Thẻ (`Card-Specific Speed Thresholds`)**: Mỗi loại thẻ có ngưỡng phản hồi "Nhanh" khác nhau (ví dụ: Trắc nghiệm $< 5\text{s}$, Gõ từ $< 10\text{s}$).

## 2. Bảng Phân loại Ngưỡng Tốc độ theo Dạng Thẻ (Speed Threshold Matrix)

| Dạng Thẻ | Mức Nhanh (Fast: $q=5$) | Mức Trung bình (Good: $q=4$) | Mức Chậm (Slow: $q=3$) | Mức Timeout ($q=0$) |
|---|---|---|---|---|
| `MULTIPLE_CHOICE` | $< 5.0\text{s}$ | $5.0\text{s} \to 15.0\text{s}$ | $15.0\text{s} \to 30.0\text{s}$ | $> 60.0\text{s}$ |
| `TEXT_RECALL` | $< 8.0\text{s}$ | $8.0\text{s} \to 20.0\text{s}$ | $20.0\text{s} \to 40.0\text{s}$ | $> 60.0\text{s}$ |
| `CLOZE_SENTENCE` | $< 10.0\text{s}$ | $10.0\text{s} \to 25.0\text{s}$ | $25.0\text{s} \to 45.0\text{s}$ | $> 60.0\text{s}$ |
| `AUDIO_LISTENING` | $< 12.0\text{s}$ | $12.0\text{s} \to 30.0\text{s}$ | $30.0\text{s} \to 50.0\text{s}$ | $> 60.0\text{s}$ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `ST-G01`: 100% thời gian trả lời $Duration < 200\text{ms}$ bị kẹp về $200\text{ms}$ trước khi tính điểm $q$.
- `ST-G02`: Thời gian $Duration > 60\text{s}$ kẹp về $60\text{s}$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `ST12-01` | Bot gửi đáp án với `Duration = 50ms` | Clamper kẹp thời gian thành $200\text{ms}$, gắn nhãn `SUSPICIOUS_SPEED`. |
| `ST12-02` | Người học treo máy làm bài 120s rồi mới bấm câu trả lời đúng | Clamper kẹp thời gian thành $60\text{s}$, tính điểm $q = 3$. |
| `ST12-03` | Kiểm thử hoàn tất luồng M04-RESPONSE-SPEED-THRESHOLDS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-ST-F01` | Tạo helper `ResponseDurationClamper` trong Domain M04 | Đảm bảo kẹp thời gian nhất quán | M04-T011 |

## 5. Tự kiểm M04-T012
- Đã hoàn thành đặc tả `M04-RESPONSE-SPEED-THRESHOLDS-1.0`.
- Chốt kẹp kẹp thời gian 200ms - 60s và ma trận ngưỡng tốc độ theo dạng thẻ.
- Ghi nhận 2 Regression Gates (`ST-G01`–`ST-G02`) và 3 Test Cases (`ST12-01`–`ST12-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa ngưỡng tốc độ phản hồi M04-T012 | WSA-7K2 |
