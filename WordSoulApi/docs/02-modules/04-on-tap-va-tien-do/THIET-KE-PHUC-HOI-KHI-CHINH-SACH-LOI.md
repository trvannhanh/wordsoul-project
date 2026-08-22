# Thiết kế phục hồi khi chính sách lỗi M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-POLICY-FAILURE-RECOVERY-1.0` |
| Task | M04-T019 |
| Đầu vào | M04-SRS-INTERVAL-CALCULATION-1.0 (M04-T016), M04-RETENTION-SCORE-CALCULATION-1.0 (M04-T018), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Chiến lược phát hiện và phục hồi khi phát hiện tham số chính sách SRS bị cài đặt sai (ví dụ: $Interval > 365$ ngày hoặc $EaseFactor < 1.30$), quy tắc Fallback về Safe Default và rollback |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế tự động phục hồi (`Policy Failure Recovery Engine`) trong M04 khi tham số chính sách SRS bị cấu hình lỗi hoặc gây xáo trộn lịch ôn tập.

- **Quy tắc Fallback về Cấu hình An toàn (`Safe Default Fallback Invariant`)**:
  - Nếu một bản ghi cấu hình chính sách SRS mới chứa giá trị vượt giới hạn an toàn $[Min, Max]$, hệ thống LẬP TỨC TỪ CHỐI áp dụng và rơi về bộ tham số an toàn mặc định (`SafeDefaultSrsParameters`):
    - $SrsMinEaseFactor = 1.30$
    - $SrsMaxEaseFactor = 2.50$
    - $SrsMaxIntervalDays = 365$
- **Không Biến đổi Tiến độ Âm thầm (`No Silent Corruption Invariant`)**:
  - Sự cố chính sách lỗi tuyệt đối CẤM làm hỏng hoặc xóa các bản ghi nhật ký ôn tập `UserSenseProgressLogs`.

## 2. Quy trình Phục hồi Cấu hình SRS Lỗi (Policy Recovery Flow)

```mermaid
graph TD
    ConfigLoad[Load SRS Policy Parameters] --> CheckValid{Params in [Min, Max] Range?}
    CheckValid -->|Yes| ApplyConfig[Apply Policy Parameters to Engine]
    CheckValid -->|No| Fallback[Fallback to SafeDefaultSrsParameters]
    Fallback --> AlertM11[Raise CRITICAL_POLICY_CONFIG_ERROR Alert to M11]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PF-G01`: 100% trường hợp tham số chính sách lỗi được tự động fallback về `SafeDefaultSrsParameters` trong $\le 100\text{ms}$.
- `PF-G02`: Sự cố chính sách phát cảnh báo `CRITICAL_POLICY_CONFIG_ERROR` sang M11 để Admin can thiệp.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PF19-01` | Admin cố tình nạp cấu hình `SrsMaxIntervalDays = 9999` ngày | Engine phát hiện vi phạm trần 365, tự động fallback về $365$ ngày và ghi log cảnh báo. |
| `PF19-02` | Admin nạp `SrsDefaultEaseFactor = 0.50` (dưới sàn 1.30) | Engine tự động fallback về $2.50$, gửi cảnh báo sang M11. |
| `PF19-03` | Kiểm thử hoàn tất luồng M04-POLICY-FAILURE-RECOVERY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PF-F01` | Đưa `SafeDefaultSrsParameters` làm hằng số bất biến trong Domain Model M04 | Đảm bảo tính sẵn sàng của cơ chế Fallback | M04-T039 |

## 5. Tự kiểm M04-T019
- Đã hoàn thành đặc tả `M04-POLICY-FAILURE-RECOVERY-1.0`.
- Chốt nguyên tắc Safe Default Fallback và cảnh báo sự cố chính sách M11.
- Ghi nhận 2 Regression Gates (`PF-G01`–`PF-G02`) và 3 Test Cases (`PF19-01`–`PF19-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế phục hồi khi chính sách lỗi M04-T019 | WSA-7K2 |
