# Thiết kế xử lý kết quả đến sai thứ tự M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-OUT-OF-ORDER-RESULT-HANDLING-1.0` |
| Task | M04-T008 |
| Đầu vào | M04-SESSION-RESULT-CONTRACT-1.0 (M04-T006), M04-IDEMPOTENT-RESULT-CONSUMPTION-1.0 (M04-T007) |
| Phạm vi | Xử lý các sự kiện kết quả phiên học từ M03 chuyển sang M04 bị đến sai thứ tự thời gian (`Out-of-order Events`), bảo đảm timestamp bất biến và không ghi đè hồ sơ nhớ SRS mới hơn bằng dữ liệu cũ |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chiến lược xử lý khi M04 tiếp nhận các sự kiện hoàn thành phiên học bị đến sai thứ tự thời gian.

- **Bảo vệ Hồ sơ Nhớ Mới hơn (`Monotonic Timestamp Guard Invariant`)**:
  - Mỗi bản ghi `UserSenseProgress` lưu trữ thuộc tính `LastReviewedAtUtc`.
  - Nếu một sự kiện kết quả phiên học nhận được có `CompletedAtUtc < LastReviewedAtUtc` (sự kiện cũ đến sau do trễ mạng/retry), hệ thống CHỈ GHI LỊCH SỬ (`ProgressLogs`), KHÔNG ĐƯỢC TÍNH LẠI hay ghi đè các chỉ số `Interval`, `EaseFactor`, `DueDateUtc` của trạng thái SRS hiện tại.
- **Tính Bất biến Lịch sử (`Immutable Log Audit Invariant`)**: Mọi sự kiện kết quả đến sai thứ tự đều được lưu thông tin vào nhật ký `OutofOrderEventLogs` để phục vụ đối soát mà không làm hỏng tiến độ học tập hiện tại.

## 2. Quy trình Xử lý Sự kiện Đến sai Thứ tự (Out-of-order Pipeline)

```mermaid
graph TD
    Event[LearningSessionCompleted Event] --> Fetch[Fetch UserSenseProgress]
    Fetch --> CheckTime{Event.CompletedAtUtc > Progress.LastReviewedAtUtc?}
    CheckTime -->|Yes (Normal Order)| UpdateSRS[Update SRS Interval & DueDate]
    CheckTime -->|No (Out of Order)| SkipSRS[Skip SRS State Update & Audit Log]
    UpdateSRS --> Save[Save to DB]
    SkipSRS --> Save
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `OO-G01`: 100% sự kiện có `CompletedAtUtc` cũ hơn `LastReviewedAtUtc` hiện tại không làm thay đổi `IntervalDays` hay `EaseFactor` trong `UserSenseProgress`.
- `OO-G02`: Tất cả sự kiện trễ thứ tự đều được ghi vết trong `OutofOrderEventLogs`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `OO08-01` | Người học ôn tập phiên B lúc 10:00 (CompletedAt = 10:00). Do mạng chậm, sự kiện phiên A (làm lúc 09:00) tới M04 sau phiên B | M04 ghi nhận lịch sử phiên A, giữ nguyên trạng thái SRS của phiên B. |
| `OO08-02` | Hai sự kiện hoàn thành có cùng timestamp | Xử lý theo thứ tự ID sự kiện, không tạo xung đột DB. |
| `OO08-03` | Kiểm thử hoàn tất luồng M04-OUT-OF-ORDER-RESULT-HANDLING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-OO-F01` | Cần tạo bảng `OutofOrderEventLogs` trong Entity Framework Core | Đảm bảo lưu vết các sự kiện đến muộn | M04-T007 |

## 5. Tự kiểm M04-T008
- Đã hoàn thành đặc tả `M04-OUT-OF-ORDER-RESULT-HANDLING-1.0`.
- Chốt nguyên tắc Monotonic Timestamp Guard bảo vệ dữ liệu SRS mới hơn.
- Ghi nhận 2 Regression Gates (`OO-G01`–`OO-G02`) và 3 Test Cases (`OO08-01`–`OO08-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế xử lý kết quả đến sai thứ tự M04-T008 | WSA-7K2 |
