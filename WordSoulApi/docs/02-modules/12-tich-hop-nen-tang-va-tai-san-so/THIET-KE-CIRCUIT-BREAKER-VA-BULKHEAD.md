# Thiết kế circuit breaker và bulkhead M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-CIRCUIT-BREAKER-BULKHEAD-1.0` |
| Task | M12-T038 |
| Đầu vào | M12-CRIT-1.0 (D-020), M12-TIMEOUT-DEADLINE-CANCELLATION-1.0 (D-099), M12-RETRY-IDEMPOTENCY-1.0 (D-100), REL-03 |
| Phạm vi | Đặc tả Giao thức Ngắt mạch (`Circuit Breaker Engine`) 3 trạng thái (`Closed`, `Open`, `Half-Open`) và Vách ngăn Cách ly Tải (`Bulkhead Isolation Engine`) theo từng nhà cung cấp hạ tầng tích hợp |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Ngắt mạch và Vách ngăn Cách ly Tải (`Circuit Breaker & Bulkhead Isolation Engine`) thuộc M12, ngăn chặn hiện tượng Sập đổ Dây chuyền (`Cascading Failures`) khi một nhà cung cấp bên ngoài (ví dụ: AI Gemini LLM hoặc FCM Push) gặp sự cố ngừng hoạt động kéo dài.

- **3 Trạng thái Ngắt mạch Chuẩn hóa (`3-State Circuit Breaker Invariant`)**:
  - *Trạng thái Mở (`Closed` - Bình thường)*: Phân tích tỷ lệ lỗi trong cửa sổ trượt 30 giây (tối thiểu 10 request). Nếu tỷ lệ thất bại $> 50\%$ $\to$ Tự động chuyển sang `Open`.
  - *Trạng thái Ngắt (`Open` - Sự cố)*: Ngắt toàn bộ luồng kết nối trong 30 giây (`BreakDuration = 30s`). $100\%$ request đến ngay lập tức bị chối từ hoặc chuyển sang luồng suy giảm (`Degraded Fallback`).
  - *Trạng thái Thử nghiệm (`Half-Open` - Thử nghiệm)*: Cho phép 3 request thử nghiệm (`HalfOpenTrialCount = 3`). Nếu cả 3 thành công $\to$ Chuyển về `Closed`. Nếu có 1 request lỗi $\to$ Quay lại `Open` 30s.
- **Phân vùng Cách ly Tải Bulkhead theo Provider (`Bulkhead Isolation Invariant`)**: Mỗi nhà cung cấp bên ngoài được cấp một pool tài nguyên riêng biệt (ví dụ: AI Gemini max 20 concurrent execution slots, FCM Push max 50 slots, Speech TTS max 30 slots).
- **Quy tắc Chặn Tràn Hàng chờ Bulkhead (`Bulkhead Queue Overflow Rule`)**: Hàng chờ tối đa của từng bulkhead pool là 10 request. Nếu hàng chờ đầy $\to$ Reject ngay lập tức với HTTP 429 Too Many Requests (`BULKHEAD_CAPACITY_EXCEEDED`).
- **Nhật ký Đánh dấu Trạng thái Ngắt mạch M11 (`Circuit Breaker Audit Trail`)**: $100\%$ sự kiện chuyển trạng thái ngắt mạch (`Closed` $\to$ `Open` $\to$ `Half-Open`) bắt buộc được ghi vết bất biến `ACT-M11-12` và phát cảnh báo Telegram/Email tới đội vận hành.

## 2. Ma trận Phân vùng Bulkhead và Ngưỡng Ngắt mạch (Bulkhead & Circuit Matrix)

| Nhà cung cấp (`Provider`) | Slot Đồng thời (`Execution Slots`) | Hàng chờ Tối đa (`Max Queue`) | Ngưỡng Lỗi Ngắt (`Failure Threshold`) | Thời gian Ngắt (`Break Duration`) | Phương án Suy giảm (`Fallback Action`) |
|---|---|---|---|---|---|
| **AI Gemini LLM** | 20 slots | 10 requests | $> 50\%$ trong 30s | 30 giây | Trả về bộ từ gợi ý mẫu sẵn có (`Static Fallback`) |
| **FCM Push** | 50 slots | 20 requests | $> 50\%$ trong 30s | 30 giây | Lưu notification vào DB queue gửi sau (`In-DB Deferred`) |
| **Speech TTS** | 30 slots | 10 requests | $> 50\%$ trong 30s | 30 giây | Tắt phát âm tự động trong bài học (`Mute Audio`) |
| **OAuth Providers** | 40 slots | 15 requests | $> 50\%$ trong 30s | 30 giây | Chuyển hướng người dùng sang Đăng nhập Email |

## 3. Máy Trạng thái Ngắt mạch Circuit Breaker (State Machine)

```
       +---------------------------------------------+
       |                                             |
       v                                             | (3 Trial Requests
  [CLOSED State] --(Failure Rate > 50% / 30s)--> [OPEN State]
       ^                                             |
       |                                             | (After 30s Break Duration)
       |                                             v
       +---(All 3 Trials Succeed)--- [HALF-OPEN State]
                                             |
                                             +---(Any Trial Fails)---+
```

## 4. Giao thức Thực thi Ngắt mạch và Bulkhead CSDL (CircuitBreakerService)

```csharp
public async Task<TResult> ExecuteWithCircuitBreakerAndBulkheadAsync<TResult>(
    string providerName, 
    Func<Task<TResult>> primaryCall, 
    Func<Task<TResult>> fallbackCall)
{
    var circuitPolicy = GetCircuitBreakerPolicy(providerName); // Polly CircuitBreakerPolicy
    var bulkheadPolicy = GetBulkheadPolicy(providerName);       // Polly BulkheadPolicy

    try
    {
        return await bulkheadPolicy.ExecuteAsync(async () =>
            await circuitPolicy.ExecuteAsync(primaryCall)
        );
    }
    catch (BrokenCircuitException)
    {
        await _auditLog.RecordEventAsync("ACT-M11-12", "SYSTEM", new { Action = "CIRCUIT_OPEN", Provider = providerName });
        return await fallbackCall(); // Chuyển sang luồng suy giảm
    }
    catch (BulkheadRejectedException)
    {
        await _auditLog.RecordEventAsync("ACT-M11-12", "SYSTEM", new { Action = "BULKHEAD_FULL", Provider = providerName });
        throw new InvalidOperationException($"BULKHEAD_CAPACITY_EXCEEDED: Hạ tầng tích hợp {providerName} đang quá tải.");
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CB-G01` | 100% kết nối tới nhà cung cấp ngoài phải qua Circuit Breaker và Bulkhead Isolation Policy. |
| `CB-G02` | Tỷ lệ lỗi $> 50\%$ trong 30s tự động chuyển cờ Circuit Breaker sang `Open` trong thời gian 30s. |
| `CB-G03` | Khi Circuit Breaker `Open`, request tự động chuyển sang luồng suy giảm (`Fallback`) trong SLA $\le 2\text{ms}$. |
| `CB-G04` | Sau 30s ngắt, Circuit Breaker tự động chuyển sang `Half-Open` để thử nghiệm 3 request mẫu. |
| `CB-G05` | Bulkhead pool cách ly hoàn toàn giữa các nhà cung cấp (AI sập không làm ảnh hưởng đến FCM Push). |
| `CB-G06` | Tràn hàng chờ Bulkhead tự động chối từ với mã lỗi `BULKHEAD_CAPACITY_EXCEEDED` (HTTP 429). |
| `CB-G07` | 100% các lần chuyển trạng thái Circuit Breaker được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-12`). |
| `CB-G08` | Phân quyền thay đổi thông số Circuit Breaker / Bulkhead chỉ dành riêng cho `SuperAdmin`. |
| `CB-G09` | SLA kiểm tra trạng thái Circuit Breaker $< 1\text{ms}$. |
| `CB-G10` | 100% các test case tự kiểm CB38-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CB38-01` | Dịch vụ AI Gemini bị lỗi 10/10 request trong 30s | Circuit Breaker chuyển `Open`, 100% request sau nhận fallback |
| `CB38-02` | Request đến dịch vụ AI Gemini khi Circuit Breaker đang `Open` | Nhận kết quả fallback mẫu ngay lập tức ($< 2\text{ms}$) |
| `CB38-03` | Hết 30s ngắt, gửi 3 request thử nghiệm khi trạng thái `Half-Open` | 3 request thành công $\to$ Circuit Breaker quay lại `Closed` |
| `CB38-04` | Trong 3 request thử nghiệm `Half-Open`, có 1 request bị thất bại | Circuit Breaker quay lại trạng thái `Open` thêm 30 giây |
| `CB38-05` | Tải đồng thời 30 request gọi AI Gemini (chỉ cấp 20 slots, max 10 queue) | 20 request chạy, 10 request chờ trong queue |
| `CB38-06` | Gửi request thứ 31 đến AI Gemini khi bulkhead queue đã đầy | Reject 429 `BULKHEAD_CAPACITY_EXCEEDED` |
| `CB38-07` | Dịch vụ AI Gemini sập hoàn toàn (`Open`) | Dịch vụ FCM Push vẫn hoạt động bình thường ở trạng thái `Closed` |
| `CB38-08` | Tra cứu vết Audit Log M11 sau khi Circuit Breaker chuyển `Open` | Ghi nhận Audit Event `ACT-M11-12` đính kèm tên Provider |
| `CB38-09` | Phân tích thời gian phản hồi kiểm tra Circuit Breaker | Checked latency $< 1\text{ms}$ |
| `CB38-10` | Khôi phục hoạt động nhà cung cấp OAuth sau khi sửa sự cố | Trạng thái tự động chuyển `Closed` sau khi test thành công |
| `CB38-11` | Dịch vụ TTS bị ngắt mạch | Bài học M03 tự động bật chế độ Mute Audio không gây lỗi ứng dụng |
| `CB38-12` | Tải đồng thời 1000 request qua 4 bulkhead pool | 100% bulkhead pool cách ly độc lập theo đúng quota |
| `CB38-13` | User không phải Admin xin thay đổi cấu hình bulkhead | Deny 403 Forbidden |
| `CB38-14` | User chưa đăng nhập gọi API qua Circuit Breaker | Deny 401 Unauthorized |
| `CB38-15` | Cấu hình cờ `ForceOpen` thủ công bởi SuperAdmin | Circuit Breaker chuyển `Open` lập tức cho đến khi bỏ cờ |
| `CB38-16` | Kiểm tra tỷ lệ phục hồi sau sự cố chập chờn | Hệ thống tự động phục hồi về 100% capacity khi hết lỗi |
| `CB38-17` | Phân tích tham chiếu các cờ trạng thái ngắt mạch | Quét metrics Prometheus `circuit_breaker_state` (T020) |
| `CB38-18` | Thao tác ghi vết ngắt mạch bị ném exception | Vẫn trả kết quả Fallback an toàn cho người dùng |
| `CB38-19` | Dịch vụ FCM Push bị tràn Bulkhead Queue | Đưa notification vào CSDL local chờ phát lại (In-DB Deferred) |
| `CB38-20` | Kiểm thử hoàn tất luồng ngắt mạch bulkhead M12-CIRCUIT-BREAKER-BULKHEAD-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-CB-I01` | Một số HttpClient gọi dịch vụ ngoài hiện chưa bọc Polly CircuitBreakerPolicy | Nguy cơ bị treo chuỗi dịch vụ khi nhà cung cấp ngoài gặp sự cố sập kéo dài | M12-T049 (Source task) |
| `M12-CB-I02` | Chưa có cấu hình Bulkhead Isolation cách ly slots theo từng Provider | Một dịch vụ AI quá tải có thể vắt kiệt ThreadPool của toàn bộ API Server | M12-T049; M12-T003 |
| `M12-CB-I03` | Thiếu luồng Fallback Action tiêu chuẩn khi Circuit Breaker bị `Open` | Khi bị ngắt mạch, API trả về lỗi 500 thay vì kết quả suy giảm | M12-T049 |
| `M12-CB-I04` | Thiếu metric Prometheus đo đếm trạng thái Circuit Breaker | Đội vận hành không phát hiện được dịch vụ ngoài đang bị ngắt mạch | M12-T049; M11-T036 |
| `M12-CB-I05` | Chưa có cờ `ForceOpen` cho phép SuperAdmin can thiệp khẩn cấp | Không thể chủ động ngắt kết nối với 1 nhà cung cấp bị sự cố bảo mật | M12-T049; REL-03 |

- `M12-CB-F01`: Triển khai `CircuitBreakerService` với 3 trạng thái và Fallback Actions (tiếp nhận: M12-T049).
- `M12-CB-F02`: Triển khai `BulkheadIsolationManager` cách ly pool theo Provider (tiếp nhận: M12-T049; M12-T003).
- `M12-CB-F03`: Tích hợp cờ `ForceOpen` và metric Prometheus (tiếp nhận: M12-T049; M11-T036).
- `M12-CB-F04`: Thiết lập bộ kiểm thử tự động CB-G01–G10 và CB38-01–20 (tiếp nhận: M12 tasks).
- `M12-CB-F05`: Thu thập bằng chứng runtime cho luồng ngắt mạch M12 (tiếp nhận: M12 tasks; A-G04/A-G06).

## 8. Tự kiểm M12-T038

- Đã thiết kế hoàn chỉnh `M12-CIRCUIT-BREAKER-BULKHEAD-1.0` với Máy Trạng thái Ngắt mạch 3 bước (`Closed`, `Open`, `Half-Open`).
- Đã chốt Ràng buộc Phân vùng Cách ly Tải Bulkhead theo Provider và Quy tắc Chặn Tràn Hàng chờ (HTTP 429).
- Đã chốt Luồng Suy giảm (`Degraded Fallback`) khi bị ngắt mạch SLA $\le 2\text{ms}$.
- Đã lồng ghép Nhật ký Đánh dấu Trạng thái M11 (`ACT-M11-12`) và Cờ `ForceOpen` cho SuperAdmin.
- Đã xác lập 10 Regression Gates (`CB-G01`–`CB-G10`) và 20 Test Cases tự kiểm (`CB38-01`–`CB38-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế circuit breaker và bulkhead M12-T038 | WSA-7K2 |
