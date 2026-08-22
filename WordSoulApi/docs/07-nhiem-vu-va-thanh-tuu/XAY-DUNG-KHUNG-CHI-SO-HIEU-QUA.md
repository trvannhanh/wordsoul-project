# Xây dựng khung chỉ số hiệu quả M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-KPI-FRAMEWORK-1.0` |
| Task | M07-T043 |
| Đầu vào | M07-ANTI-GRIND-CEILING-POLICY-1.0 (M07-T021), M07-QUEST-REWARD-RECOVERY-RECONCILIATION-1.0 (M07-T034), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Khung chỉ số đo lường hiệu quả thiết kế nhiệm vụ (`Quest System Health KPI Framework`), đo tỷ lệ hoàn thành, tỷ lệ nhận thưởng và chỉ số chống cày gian lận |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa khung chỉ số đánh giá hiệu quả thiết kế nhiệm vụ (`Quest KPI & Health Framework`) trong M07.

- **Đồng nhất Chỉ số Học tập Chất lượng (`Pedagogical Alignment Invariant`)**:
  - Tỷ lệ hoàn thành nhiệm vụ BẮT BUỘC gắn liền với hiệu quả học tập thực tế (First-Try Accuracy $\ge 80\%$).
  - Không công nhận nhiệm vụ hiệu quả nếu tỷ lệ cày spam ngắn $< 3$ phút chiếm $> 30.0\%$.
- **Đo lường Tỷ lệ Nhận thưởng Bị bỏ quên (`Unclaimed Reward Waste Ratio Rule`)**:
  - Nếu `UnclaimedWasteRatio > 15.0\%` (nhiều người dùng hoàn thành nhưng quên bấm nhận thưởng), hệ thống BẮT BUỘC cảnh báo để M10 tối ưu giờ gửi nhắc.

## 2. Bảng Danh mục Chỉ số Hiệu quả Nhiệm vụ (Quest KPI Catalog)

| Mã Chỉ số KPI | Công thức Tính | Ngưỡng Khỏe mạnh | Hành động khi Vi phạm |
|---|---|---|---|
| `QUEST_COMPLETION_RATE` | $\frac{\text{Num Completed Quests}}{\text{Num Assigned Quests}}$ | $60.0\% - 85.0\%$ | Đội ngũ Content điều chỉnh độ khó |
| `UNCLAIMED_WASTE_RATIO` | $\frac{\text{Num EXPIRED\_UNCLAIMED}}{\text{Num COMPLETED\_UNCLAIMED}}$ | $< 10.0\%$ | M10 gửi thêm 1 Push nhắc nhận thưởng |
| `GRIND_EXPLOIT_INDEX` | $\frac{\text{Num Spam Frozen Sessions}}{\text{Total Sessions}}$ | $< 2.0\%$ | Khóa tài khoản gian lận nghi ngờ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QK-G01`: 100% chỉ số báo cáo KPI khớp công thức chuẩn hóa.
- `QK-G02`: `UnclaimedWasteRatio > 15%` tự động ném cảnh báo tới Dashboard Admin M11.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QK43-01` | Ngày 2026-08-22 có 1,000 nhiệm vụ giao, 750 nhiệm vụ hoàn thành | API trả về `QUEST_COMPLETION_RATE = 75.0%`. |
| `QK43-02` | Có 200 nhiệm vụ hoàn thành nhưng 40 nhiệm vụ bị hết hạn do không bấm nhận (20%) | API ném cảnh báo `HIGH_UNCLAIMED_WASTE_WARNING`. |
| `QK43-03` | Kiểm thử hoàn tất luồng M07-QUEST-KPI-FRAMEWORK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QK-F01` | Đưa `QuestKpiMetricsEnvelopeDto` vào Admin Dashboard M11 | Phục vụ đội ngũ vận hành theo dõi sức khỏe nhiệm vụ | M11-T012 |

## 5. Tự kiểm M07-T043
- Đã hoàn thành đặc tả `M07-QUEST-KPI-FRAMEWORK-1.0`.
- Chốt khung chỉ số KPI nhiệm vụ và ngưỡng cảnh báo tỷ lệ nhận thưởng bị bỏ quên.
- Ghi nhận 2 Regression Gates (`QK-G01`–`QK-G02`) và 3 Test Cases (`QK43-01`–`QK43-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng khung chỉ số hiệu quả M07-T043 | WSA-7K2 |
