# Xây dựng bộ kiểm thử lỗi và suy giảm M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-FAULT-DEGRADATION-TEST-SUITE-1.0` |
| Task | M12-T039 |
| Đầu vào | M12-CHANNEL-DEGRADATION-POLICY-1.0 (M12-T030), M12-CHANNEL-RESPONSE-RETRY-1.0 (M12-T029) |
| Phạm vi | Ma trận kịch bản thử nghiệm giả lập sự cố hạ tầng (`Chaos Testing Matrix`), kiểm tra khả năng phục hồi của hệ thống khi mất kết nối Redis, DB, FCM, SendGrid hoặc SignalR |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa ma trận kịch bản kiểm thử chịu lỗi và suy giảm hạ tầng (`Fault Injection & Chaos Testing Suite`) trong M12.

- **Đảm bảo Trải nghiệm Học tập Cốt lõi Không bị Gián đoạn (`Core Learning Resilience Invariant`)**:
  - Khi $100\%$ dịch vụ Push Notification (FCM/SendGrid) hoặc Analytics M11 sập hoàn toàn:
    - Trải nghiệm học bài M03, tính toán SRS M04 và số dư tài sản M06 BẮT BUỘC tiếp tục hoạt động bình thường $100\%$ không bị treo đứt.
- **Tiêu chuẩn Khôi phục Tự động Chaos Test (`Automated Chaos Recovery Rule`)**: Hệ thống BẮT BUỘC tự động khôi phục về trạng thái khỏe mạnh `HEALTHY` trong vòng $< 30$ giây sau khi sự cố hạ tầng giả lập được giải tỏa.

## 2. Ma trận Kịch bản Thử nghiệm Chịu lỗi Hạ tầng (Fault Test Matrix)

| Mã Case | Sự cố Giả lập (Fault Injection) | Mô tả Tình huống | Kết quả Kỳ vọng Bắt buộc |
|---|---|---|---|
| `FT-01` | FCM Push Gateway Sập | FCM trả về 500/Timeout 100% | Push chuyển sang `DeferredPushQueue`, M03/M04 hoạt động mượt mà |
| `FT-02` | Redis Cache Sập | Redis Cluster Unreachable | System fallback đọc DB SQL direct, latency tăng nhẹ nhưng không crash |
| `FT-03` | SignalR Realtime Sập | Ngắt kết nối WebSocket toàn bộ | M08 tạm ngắt thi đấu PvP, App fallback sang HTTP Polling nhẹ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `FG-G01`: 100% bài kiểm thử Chaos Test không làm gián đoạn API nộp câu trả lời bài học M03.
- `FG-G02`: Thời gian tự động khôi phục dịch vụ sau sự cố giả lập $< 30$ giây.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `FT39-01` | Giả lập sập toàn bộ dịch vụ SendGrid Email trong khi 1,000 học sinh đang làm bài | API nộp bài M03 phản hồi 40ms bình thường, email đưa vào queue chờ. |
| `FT39-02` | Mất kết nối Redis Cluster 10 giây trong Chaos Test | System tự động fallback đọc SQL DB, khôi phục ngay khi Redis sống lại. |
| `FT39-03` | Kiểm thử hoàn tất luồng M12-FAULT-DEGRADATION-TEST-SUITE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-FG-F01` | Xây dựng bộ test suite `WordSoul.Infrastructure.ChaosTests` | Tự động hóa kiểm thử chịu lỗi CI/CD | M12-T030 |

## 5. Tự kiểm M12-T039
- Đã hoàn thành đặc tả `M12-FAULT-DEGRADATION-TEST-SUITE-1.0`.
- Chốt ma trận Chaos Test 3 kịch bản chịu lỗi hạ tầng và SLA tự khôi phục $< 30$ giây.
- Ghi nhận 2 Regression Gates (`FG-G01`–`FG-G02`) và 3 Test Cases (`FT39-01`–`FT39-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng bộ kiểm thử lỗi và suy giảm M12-T039 | WSA-7K2 |
