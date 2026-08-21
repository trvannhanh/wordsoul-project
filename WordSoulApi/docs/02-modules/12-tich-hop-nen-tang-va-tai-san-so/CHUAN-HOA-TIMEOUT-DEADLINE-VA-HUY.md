# Chuẩn hóa timeout, deadline và hủy M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-TIMEOUT-DEADLINE-CANCELLATION-1.0` |
| Task | M12-T036 |
| Đầu vào | M12-CRIT-1.0 (D-020), M12-CONTRACT-1.0 (D-021), M12-RESULT-1.0 (D-022), REL-03 |
| Phạm vi | Đặc tả Giao thức Quản lý Ngưỡng Timeout, Lan truyền Hạn chót Deadline (`Deadline Propagation`) và Hủy Tác vụ Tích hợp (`CancellationToken Pipeline`), dọn dẹp tài nguyên và phản hồi mã lỗi chuẩn |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chuẩn hóa Timeout, Deadline và Hủy Tác vụ Tích hợp (`Integration Timeout, Deadline & Cancellation Engine`) thuộc M12, ngăn ngừa tình trạng nghẽn ThreadPool hoặc treo kết nối HTTP khi gọi các dịch vụ bên ngoài (AI Gemini, FCM Push, SMTP Mail, Cloud Storage).

- **4 Cấp Ngưỡng Timeout Bắt buộc theo Lát Năng lực (`4-Tier Timeout Threshold Invariant`)**:
  - *Cấp C0 (Realtime Sync / DB Auth)*: Timeout = 3000ms.
  - *Cấp C1 (Fast External / FCM Push / TTS Cache)*: Timeout = 5000ms.
  - *Cấp C2 (AI Generation / LLM Synthesis)*: Timeout = 15000ms.
  - *Cấp C3 (Async Batch / Report Export)*: Timeout = 60000ms.
- **Lan truyền CancellationToken Bắt buộc (`Mandatory CancellationToken Propagation Invariant`)**: $100\%$ phương thức bất đồng bộ gọi HTTP/CSDL/Redis BẮT BUỘC truyền tham số `CancellationToken`. CẤM gọi API không hỗ trợ cancellation token.
- **Quy trình Lan truyền Hạn chót Deadline (`Deadline Propagation Invariant`)**: Khi request gọi xuyên nhiều microservice/module, mốc Deadline UTC tổng thể (`RequestDeadlineUtc`) được lan truyền qua HTTP Header `X-Request-Deadline-Utc`. Thời gian timeout của dịch vụ con tự động rút ngắn bằng `Min(LocalTimeout, RemainingDeadline)`.
- **Dọn dẹp Tài nguyên khi Hủy Tác vụ (`Resource Cleanup on Abort`)**: Khi tác vụ bị Timeout hoặc bị client ngắt kết nối (`TaskCanceledException`), hệ thống BẮT BUỘC hủy kết nối socket HTTP, giải phóng Redis Distributed Lock và trả về HTTP 504 Gateway Timeout hoặc 499 Client Closed Request.

## 2. Ngưỡng Timeout theo Phân loại Lát Năng lực (Timeout Matrix)

| Phân loại Lát (`Capability Class`) | Dịch vụ Áp dụng | Ngưỡng Timeout Cứng (`Hard Timeout`) | Mã Lỗi khi Quá Hạn | Phản hồi HTTP Status |
|---|---|---|---|---|
| **C0_REALTIME_SYNC** | OAuth Provider, Session DB | 3,000 ms (3s) | `ERR_TIMEOUT_C0_AUTH` | 504 Gateway Timeout |
| **C1_FAST_EXTERNAL** | FCM Push, Speech TTS Cache | 5,000 ms (5s) | `ERR_TIMEOUT_C1_FAST` | 504 Gateway Timeout |
| **C2_AI_GENERATION** | AI Gemini LLM, Audio Synthesis | 15,000 ms (15s) | `ERR_TIMEOUT_C2_AI` | 504 Gateway Timeout |
| **C3_ASYNC_BATCH** | Export Report, Orphan Cleanup | 60,000 ms (60s) | `ERR_TIMEOUT_C3_BATCH` | 504 Gateway Timeout |

## 3. Kiến trúc Luồng Lan truyền Deadline và Hủy Tác vụ (Deadline & Cancellation Engine)

```
[Client Sends Request (Header: X-Request-Deadline-Utc: 10:00:15Z)]
                                 |
                                 v
         [Calculate Remaining Deadline (e.g. 12,000 ms remaining)]
                                 |
                                 v
         [Create CancellationTokenSource(Min(15000ms, 12000ms))]
                                 |
                                 v
         [Execute Async Integration Call (Pass CancellationToken)]
                                 |
                 +---------------+---------------+
                 | (Completed in Time)           | (Timeout / Aborted)
                 v                               v
         [Return 200 OK Result]         [Catch TaskCanceledException]
                                         - Abort Socket Connection
                                         - Release Redis Lock
                                         - Record Log M12_TIMEOUT
                                         - Return HTTP 504 / 499
```

## 4. Giao thức Thực thi Quản lý Timeout CSDL (IntegrationTimeoutService)

```csharp
public async Task<TResult> ExecuteWithTimeoutAsync<TResult>(
    Func<CancellationToken, Task<TResult>> integrationCall, 
    CapabilityClass capabilityClass, 
    HttpContext httpContext)
{
    // 1. Xác định Timeout theo Capability Class
    int defaultTimeoutMs = capabilityClass switch
    {
        CapabilityClass.C0_REALTIME_SYNC => 3000,
        CapabilityClass.C1_FAST_EXTERNAL => 5000,
        CapabilityClass.C2_AI_GENERATION => 15000,
        CapabilityClass.C3_ASYNC_BATCH => 60000,
        _ => 5000
    };

    // 2. Lan truyền Deadline từ Header X-Request-Deadline-Utc
    var remainingMs = defaultTimeoutMs;
    if (httpContext.Request.Headers.TryGetValue("X-Request-Deadline-Utc", out var deadlineHeader) &&
        DateTime.TryParse(deadlineHeader, out var deadlineUtc))
    {
        var diffMs = (int)(deadlineUtc - DateTime.UtcNow).TotalMilliseconds;
        if (diffMs > 0 && diffMs < defaultTimeoutMs) remainingMs = diffMs;
    }

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(
        httpContext.RequestAborted, 
        new CancellationTokenSource(remainingMs).Token
    );

    try
    {
        return await integrationCall(cts.Token);
    }
    catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
    {
        await _auditLog.RecordEventAsync("ACT-M11-12", "SYSTEM", new { Status = "499_CLIENT_CLOSED" });
        throw new InvalidOperationException("M12_CLIENT_CANCELLED_REQUEST");
    }
    catch (OperationCanceledException)
    {
        await _auditLog.RecordEventAsync("ACT-M11-12", "SYSTEM", new { Status = "504_TIMEOUT", Capability = capabilityClass.ToString() });
        throw new TimeoutException($"M12_INTEGRATION_TIMEOUT_EXCEEDED: Dịch vụ {capabilityClass} quá hạn {remainingMs}ms.");
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `TC-G01` | 100% phương thức gọi dịch vụ ngoài bắt buộc truyền `CancellationToken` và cài đặt Timeout. |
| `TC-G02` | Quá hạn Timeout C0/C1/C2/C3 tự động hủy kết nối HTTP và trả về HTTP 504 Gateway Timeout. |
| `TC-G03` | Người học ngắt kết nối (`RequestAborted`) tự động hủy tác vụ đang chạy rỗng trên Server (HTTP 499). |
| `TC-G04` | Header `X-Request-Deadline-Utc` được lan truyền chuẩn xác qua tất cả các lớp gọi dịch vụ con. |
| `TC-G05` | Ngưỡng Timeout dịch vụ AI Gemini C2 tuyệt đối không được vượt quá 15,000ms (15s). |
| `TC-G06` | Khi Timeout xảy ra, toàn bộ Redis Distributed Lock liên quan bắt buộc phải được giải phóng an toàn. |
| `TC-G07` | 100% các sự kiện Timeout hoặc Client Cancel được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-12`). |
| `TC-G08` | Phân quyền điều chỉnh ngưỡng Timeout hệ thống chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `TC-G09` | SLA phát hiện Timeout và hủy socket connection $< 10\text{ms}$. |
| `TC-G10` | 100% các test case tự kiểm TC36-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TC36-01` | Gọi dịch vụ OAuth C0 bị treo quá 3000ms | Hủy socket, trả về HTTP 504 `ERR_TIMEOUT_C0_AUTH` |
| `TC36-02` | Gọi dịch vụ AI Gemini C2 bị treo quá 15000ms | Hủy socket, trả về HTTP 504 `ERR_TIMEOUT_C2_AI` |
| `TC36-03` | Gọi dịch vụ AI Gemini phản hồi thành công ở giây thứ 3 | Trả về 200 OK với kết quả gợi ý |
| `TC36-04` | Người học đóng trình duyệt ở giây thứ 2 khi AI Gemini đang chạy | Nhận cờ `RequestAborted`, hủy tác vụ AI lập tức (HTTP 499) |
| `TC36-05` | Truyền header `X-Request-Deadline-Utc` còn lại 2000ms cho tác vụ C2 (15s) | Tác vụ C2 tự động rút ngắn Timeout còn đúng 2000ms |
| `TC36-06` | Tác vụ bị Timeout khi đang giữ Redis Distributed Lock | Lock tự động được giải phóng qua Lua Script |
| `TC36-07` | Tra cứu vết Audit Log M11 sau khi xảy ra sự cố Timeout C2 | Ghi nhận Audit Event `ACT-M11-12` đính kèm mã lỗi 504 |
| `TC36-08` | Thử gọi API integration mà không truyền `CancellationToken` | Code review Reject / Build fail |
| `TC36-09` | Tải đồng thời 50 request bị Timeout cùng lúc | 50 socket connection được dọn dẹp sạch sẽ không rò rỉ RAM |
| `TC36-10` | Kiểm tra thời gian hủy socket sau khi chạm mốc Timeout | Socket ngắt trong $< 5\text{ms}$ |
| `TC36-11` | Gọi dịch vụ FCM Push C1 bị quá hạn 5000ms | Hủy tác vụ push, trả về `ERR_TIMEOUT_C1_FAST` |
| `TC36-12` | Tải đồng thời 100 request thành công trong hạn chót Deadline | 100% request trả về 200 OK |
| `TC36-13` | User không phải Admin thử điều chỉnh cấu hình Timeout | Deny 403 Forbidden |
| `TC36-14` | User chưa đăng nhập thử gọi API integration | Deny 401 Unauthorized |
| `TC36-15` | Gọi tác vụ Batch C3 xử lý trong 45 giây ($< 60\text{s}$) | Xử lý hoàn tất thành công |
| `TC36-16` | Gọi tác vụ Batch C3 bị treo quá 60 giây | Hủy tác vụ batch, trả về `ERR_TIMEOUT_C3_BATCH` |
| `TC36-17` | Phân tích tham chiếu các điểm ngắt CancellationToken | Quét toàn bộ async controllers M12 (T020) |
| `TC36-18` | Thao tác dọn dẹp sau Timeout bị gián đoạn do lỗi | Retry dọn dẹp ngầm không làm đứt response 504 |
| `TC36-19` | Truyền header Deadline đã hết hạn từ trước (`Deadline < Now`) | Reject ngay 504 Gateway Timeout không gọi dịch vụ ngoài |
| `TC36-20` | Kiểm thử hoàn tất luồng chuẩn hóa timeout deadline M12-TIMEOUT-DEADLINE-CANCELLATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-TC-I01` | Một số HttpClient calls trong codebase chưa truyền `CancellationToken` | Nguy cơ rò rỉ socket và nghẽn ThreadPool khi dịch vụ ngoài chậm | M12-T049 (Source task) |
| `M12-TC-I02` | Chưa có bộ lan truyền header `X-Request-Deadline-Utc` giữa các modules | Các dịch vụ con vẫn chạy đủ 15s dù client đã hết hạn | M12-T049; M12-T004 |
| `M12-TC-I03` | Thiếu phân loại 4 cấp Timeout (C0 3s, C1 5s, C2 15s, C3 60s) | Áp dụng timeout cào bằng gây lỗi rải rác | M12-T049 |
| `M12-TC-I04` | Thiếu luồng tự động giải phóng Redis Lock khi gặp `TaskCanceledException` | Lock bị giữ lại 5s vô ích khi tác vụ đã bị cancel | M12-T049; M12-T033 |
| `M12-TC-I05` | Chưa phân biệt log giữa 504 Timeout và 499 Client Cancelled | Khó khăn cho bộ phận vận hành khi phân tích nguyên nhân lỗi | M12-T049; M11-T031 |

- `M12-TC-F01`: Triển khai `IntegrationTimeoutService` với 4 cấp Timeout C0-C3 (tiếp nhận: M12-T049).
- `M12-TC-F02`: Tích hợp Bắt buộc `CancellationToken` trên 100% async methods (tiếp nhận: M12-T049).
- `M12-TC-F03`: Triển khai `DeadlinePropagationMiddleware` đọc `X-Request-Deadline-Utc` (tiếp nhận: M12-T049; M12-T004).
- `M12-TC-F04`: Thiết lập bộ kiểm thử tự động TC-G01–G10 và TC36-01–20 (tiếp nhận: M12 tasks).
- `M12-TC-F05`: Thu thập bằng chứng runtime cho luồng timeout deadline M12 (tiếp nhận: M12 tasks; A-G04/A-G06).

## 8. Tự kiểm M12-T036

- Đã thiết kế hoàn chỉnh `M12-TIMEOUT-DEADLINE-CANCELLATION-1.0` với Khung 4 Cấp Ngưỡng Timeout C0-C3.
- Đã chốt Ràng buộc Lan truyền CancellationToken Bắt buộc và Lan truyền Deadline `X-Request-Deadline-Utc`.
- Đã chốt Giao thức Dọn dẹp Tài nguyên socket/lock khi bị Abort và Phản hồi HTTP 504 / 499.
- Đã lồng ghép Phân biệt Log Chẩn đoán M11 (`ACT-M11-12`) cho sự cố Timeout và Client Cancel.
- Đã xác lập 10 Regression Gates (`TC-G01`–`TC-G10`) và 20 Test Cases tự kiểm (`TC36-01`–`TC36-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chuẩn hóa timeout, deadline và hủy M12-T036 | WSA-7K2 |
