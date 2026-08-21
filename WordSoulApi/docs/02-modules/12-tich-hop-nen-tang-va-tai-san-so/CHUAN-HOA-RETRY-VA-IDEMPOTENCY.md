# Chuẩn hóa retry và idempotency M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-RETRY-IDEMPOTENCY-1.0` |
| Task | M12-T037 |
| Đầu vào | M12-RESULT-1.0 (D-022), M12-TIMEOUT-DEADLINE-CANCELLATION-1.0 (D-099), REL-03 |
| Phạm vi | Đặc tả Giao thức Thử lại (`Exponential Backoff with Jitter Policy`) và Tính Đẳng phản (`Idempotency Engine`) cho toàn bộ tác vụ tích hợp bên ngoài và biến đổi dữ liệu hệ thống |
| Tự kiểm | A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chuẩn hóa Thử lại và Tính Đẳng phản (`Integration Retry & Idempotency Engine`) thuộc M12, bảo đảm tính kiên cường (resilience) khi tích hợp với dịch vụ ngoài bị chập chờn đồng thời loại bỏ nguy cơ xử lý trùng lặp giao dịch (duplicate execution).

- **Chiến lược Thử lại Lùi lũy thừa kèm Nhiễu ngẫu nhiên (`Exponential Backoff with Jitter Invariant`)**:
  - *Công thức tính thời gian chờ*: $\text{Delay} = \operatorname{Min}\left(\text{MaxDelay}, \text{InitialDelay} \times 2^{\text{attempt}} + \text{RandomJitter}\right)$.
  - Max Retries = 3 lần cho các lỗi tạm thời (HTTP 502 Bad Gateway, 503 Service Unavailable, 504 Gateway Timeout, SocketException).
  - CẤM thử lại đối với các lỗi vĩnh viễn (HTTP 400, 401, 403, 404, 409, 422) $\to$ Thất bại ngay lập tức (`Immediate Fail`).
- **Giao thức Khóa Đẳng phản Bắt buộc (`Mandatory Idempotency Key Invariant`)**: Mọi HTTP API có tác động thay đổi trạng thái CSDL (`POST`, `PUT`, `DELETE`) BẮT BUỘC gửi kèm mã băm `X-Idempotency-Key` (UUIDv4).
- **Lưu trữ Kết quả Đẳng phản Redis (`Redis Idempotency Storage TTL = 24h`)**: Kết quả xử lý ứng với `X-Idempotency-Key` được lưu vào Redis Key `wordsoul:{env}:idempotency:{key}` trong thời hạn 24 giờ. Nếu request bị lặp lại với cùng Idempotency Key $\to$ Trả về ngay kết quả cache mà CẤM thực thi lại nghiệp vụ CSDL.
- **Xử lý Request Đẳng phản Trùng lặp Đang thực thi (`In-Flight Concurrent Request Guard`)**: Nếu một request thứ hai gửi cùng `X-Idempotency-Key` khi request thứ nhất chưa làm xong $\to$ Trả về HTTP 409 Conflict hoặc 429 Too Many Requests (`IDEMPOTENT_OPERATION_IN_PROGRESS`).

## 2. Ma trận Phân loại Lỗi Thử lại (Retry Eligibility Matrix)

| Mã HTTP Phản hồi / Loại Lỗi | Phân loại Lỗi | Hành động Thử lại (`Retry Strategy`) | Số Lần Retry Tối đa |
|---|---|---|---|
| HTTP 502 / 503 / 504 | Transient Error | **RETRY** với Exponential Backoff | Tối đa 3 lần |
| SocketException / HttpRequestException | Network Error | **RETRY** với Exponential Backoff | Tối đa 3 lần |
| HTTP 429 Too Many Requests | Rate Limited | **RETRY** sau mốc `Retry-After` Header | Tối đa 2 lần |
| HTTP 400 / 422 | Client Error | **NO RETRY** (Báo lỗi ngay) | 0 lần |
| HTTP 401 / 403 | Auth Error | **NO RETRY** (Báo lỗi ngay) | 0 lần |
| HTTP 409 Conflict | State Conflict | **NO RETRY** (Yêu cầu client nạp lại) | 0 lần |

## 3. Quy trình Xử lý Đẳng phản Idempotency Pipeline (Idempotency Engine)

```
[Client POST /api/v1/word-sets (Header: X-Idempotency-Key: IDEM-884920)]
                                   |
                                   v
             [Check Redis Key: wordsoul:prod:idempotency:IDEM-884920]
                                   |
                +------------------+------------------+
                | (Key Exists - COMPLETED)            | (Key Not Found)
                v                                     v
   [Return Cached HTTP Response]           [Acquire Lock & Set Key State: IN_PROGRESS]
   - Status 200/201                        - Lease TTL = 60s
   - Zero DB execution                                |
                                                      v
                                           [Execute Business Logic in DB]
                                                      |
                                                      v
                                           [Save Final Response to Redis]
                                           - Update Key State: COMPLETED
                                           - Set Key TTL = 86400s (24h)
                                           - Return Response to Client
```

## 4. Giao thức Thực thi Thử lại và Đẳng phản CSDL (RetryIdempotencyService)

```csharp
public async Task<TResult> ExecuteWithRetryAndIdempotencyAsync<TResult>(
    string idempotencyKey, 
    Func<Task<TResult>> businessOperation, 
    int maxRetries = 3)
{
    string redisKey = $"wordsoul:{_env}:idempotency:{idempotencyKey}";

    // 1. Kiểm tra cache Idempotency
    var cachedJson = await _redisDb.StringGetAsync(redisKey);
    if (cachedJson.HasValue)
    {
        var cachedResponse = JsonSerializer.Deserialize<IdempotencyEntry<TResult>>(cachedJson!);
        if (cachedResponse.State == IdempotencyState.IN_PROGRESS)
        {
            throw new DbUpdateConcurrencyException("IDEMPOTENT_OPERATION_IN_PROGRESS: Yêu cầu cùng mã đẳng phản đang được xử lý.");
        }
        return cachedResponse.Result; // Trả về kết quả cũ
    }

    // 2. Đánh dấu trạng thái IN_PROGRESS
    await _redisDb.StringSetAsync(redisKey, JsonSerializer.Serialize(new IdempotencyEntry<TResult> { State = IdempotencyState.IN_PROGRESS }), TimeSpan.FromSeconds(60));

    // 3. Thực thi retry với Polly / Exponential Backoff Policy
    var policy = Policy<TResult>
        .Handle<HttpRequestException>()
        .Or<SocketException>()
        .OrResult(r => false) // Retry chỉ áp dụng lỗi mạng/transient
        .WaitAndRetryAsync(maxRetries, retryAttempt => 
            TimeSpan.FromMilliseconds(Math.Min(3000, Math.Pow(2, retryAttempt) * 200 + Random.Shared.Next(0, 100)))
        );

    try
    {
        var result = await policy.ExecuteAsync(businessOperation);

        // 4. Lưu kết quả COMPLETED vào Redis (24h TTL)
        var entry = new IdempotencyEntry<TResult> { State = IdempotencyState.COMPLETED, Result = result };
        await _redisDb.StringSetAsync(redisKey, JsonSerializer.Serialize(entry), TimeSpan.FromHours(24));

        return result;
    }
    catch (Exception)
    {
        await _redisDb.KeyDeleteAsync(redisKey); // Xóa key nếu thất bại để cho phép thử lại
        throw;
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RI-G01` | Mọi API biến đổi dữ liệu (`POST`/`PUT`/`DELETE`) BẮT BUỘC truyền `X-Idempotency-Key`. |
| `RI-G02` | Request gửi trùng `X-Idempotency-Key` trong 24h BẮT BUỘC trả về ngay cached response mà không gọi CSDL. |
| `RI-G03` | Thử lại API chỉ áp dụng cho lỗi mạng transient (502, 503, 504, SocketException) với tối đa 3 lần. |
| `RI-G04` | Các lỗi vĩnh viễn (400, 401, 403, 404, 409, 422) CẤM retry, phải báo thất bại ngay lập tức. |
| `RI-G05` | Thuật toán retry bắt buộc dùng Exponential Backoff kèm Random Jitter để tránh hiện tượng Thundering Herd. |
| `RI-G06` | Request gửi trùng Idempotency Key khi request trước đang `IN_PROGRESS` phải trả về HTTP 409 Conflict. |
| `RI-G07` | 100% các thao tác retry hoặc đụng độ đẳng phản được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-12`). |
| `RI-G08` | Phân quyền vô hiệu hóa khóa đẳng phản thủ công chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `RI-G09` | SLA trả phản hồi từ cache đẳng phản $< 5\text{ms}$. |
| `RI-G10` | 100% các test case tự kiểm RI37-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RI37-01` | Gửi request `POST /api/v1/word-sets` với `X-Idempotency-Key: IDEM-001` | Tạo thành công 201 Created, lưu cache 24h |
| `RI37-02` | Gửi lại chính xác request với `X-Idempotency-Key: IDEM-001` sau 5 phút | Trả về ngay 201 Created từ Redis cache, CSDL không có record trùng |
| `RI37-03` | Dịch vụ FCM Push bị lỗi 503 ở 2 lần thử đầu, thành công ở lần 3 | Retry thành công ở lần 3, log ghi lại 2 lần retry |
| `RI37-04` | Gọi API AI Gemini bị lỗi 401 Unauthorized | Báo lỗi ngay lập tức, 0 lần retry |
| `RI37-05` | Gửi request POST không có header `X-Idempotency-Key` | System reject 400 `MISSING_IDEMPOTENCY_KEY_HEADER` |
| `RI37-06` | Hai request trùng `X-Idempotency-Key` gửi đến cùng 1 milisecond | Request 1 thực thi, Request 2 bị chặn 409 `IDEMPOTENT_OPERATION_IN_PROGRESS` |
| `RI37-07` | Kiểm tra thời gian delay giữa các lần retry (attempt 1, 2, 3) | Delay tăng lũy thừa: ~200ms, ~400ms, ~800ms kèm jitter |
| `RI37-08` | Dịch vụ ngoài bị lỗi 504 liên tục cả 3 lần retry | Ném exception thất bại cuối cùng sau 3 lần retry |
| `RI37-09` | Xử lý nghiệp vụ bị ném exception trong quá trình thực thi | Tự động xóa cờ `IN_PROGRESS` trong Redis để client có thể gửi lại |
| `RI37-10` | Tra cứu vết Audit Log M11 sau khi retry 3 lần thành công | Ghi nhận Audit Event `ACT-M11-12` đính kèm chi tiết retry |
| `RI37-11` | Thử lại request quá mốc `Retry-After: 5` của phản hồi HTTP 429 | Đợi đủ 5 giây rồi mới retry lần thứ 2 |
| `RI37-12` | Tải đồng thời 500 request gửi trùng 50 Idempotency Keys | Đúng 50 request được xử lý CSDL, 450 request nhận kết quả cache |
| `RI37-13` | User không phải Admin xin xóa key đẳng phản | Deny 403 Forbidden |
| `RI37-14` | User chưa đăng nhập gọi API đẳng phản | Deny 401 Unauthorized |
| `RI37-15` | Kiểm tra TTL của key `wordsoul:prod:idempotency:IDEM-001` | TTL duy trì đúng 86,400 giây (24 giờ) |
| `RI37-16` | Kiểm tra thời gian phản hồi cache hit đẳng phản | Response latency p95 $< 4\text{ms}$ |
| `RI37-17` | Phân tích tham chiếu danh sách key đẳng phản trong Redis | Quét các key có tiền tố `wordsoul:*:idempotency:*` (T020) |
| `RI37-18` | Thao tác ghi cache đẳng phản bị lỗi do Redis ngắt kết nối | Bỏ qua ghi cache ngầm, trả kết quả CSDL bình thường (Fallback) |
| `RI37-19` | Client gửi header Idempotency với định dạng không hợp lệ | Reject 400 `INVALID_IDEMPOTENCY_KEY_FORMAT` |
| `RI37-20` | Kiểm thử hoàn tất luồng chuẩn hóa retry idempotency M12-RETRY-IDEMPOTENCY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-RI-I01` | Một số API POST hiện tại chưa bắt buộc kiểm tra `X-Idempotency-Key` | Nguy cơ bị gửi trùng dữ liệu khi client tự động retry | M12-T049 (Source task) |
| `M12-RI-I02` | Chưa có bộ Polly Retry Policy cấu hình lũy thừa Exponential Backoff | Lượng retry dồn dập không có jitter làm ngập dịch vụ ngoài | M12-T049; M12-T005 |
| `M12-RI-I03` | Chưa xử lý chặn request đồng thời trùng Idempotency Key (`IN_PROGRESS`) | Hai request gửi cùng lúc vẫn bị lọt vào CSDL xử lý song song | M12-T049 |
| `M12-RI-I04` | Thiếu cờ tự xóa key Redis khi nghiệp vụ bị ném exception | Key bị kẹt ở trạng thái `IN_PROGRESS` khiến client không thể thử lại | M12-T049 |
| `M12-RI-I05` | Chưa lưu đủ thông tin HTTP Header của cached response trong Redis | Khi trả cache đẳng phản bị thiếu các custom header cần thiết | M12-T049; REL-03 |

- `M12-RI-F01`: Triểnkai `RetryIdempotencyService` với Polly Policy & Exponential Backoff (tiếp nhận: M12-T049).
- `M12-RI-F02`: Triển khai `IdempotencyMiddleware` bắt buộc `X-Idempotency-Key` (tiếp nhận: M12-T049; REL-03).
- `M12-RI-F03`: Tích hợp cờ `IN_PROGRESS` và tự động cleanup khi exception (tiếp nhận: M12-T049).
- `M12-RI-F04`: Thiết lập bộ kiểm thử tự động RI-G01–G10 và RI37-01–20 (tiếp nhận: M12 tasks).
- `M12-RI-F05`: Thu thập bằng chứng runtime cho luồng retry idempotency M12 (tiếp nhận: M12 tasks; A-G04).

## 8. Tự kiểm M12-T037

- Đã thiết kế hoàn chỉnh `M12-RETRY-IDEMPOTENCY-1.0` với Công thức Thử lại Exponential Backoff kèm Random Jitter.
- Đã chốt Ràng buộc Giao thức Khóa Đẳng phản Bắt buộc (`X-Idempotency-Key`) và Redis Storage TTL 24h.
- Đã chốt Ràng buộc Chặn Request Đẳng phản Trùng lặp Đang thực thi (`IN_PROGRESS` $\to$ 409 Conflict).
- Đã lồng ghép Tự động Xóa Key khi Exception và Phân loại Lỗi Được phép Retry (502, 503, 504).
- Đã xác lập 10 Regression Gates (`RI-G01`–`RI-G10`) và 20 Test Cases tự kiểm (`RI37-01`–`RI37-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chuẩn hóa retry và idempotency M12-T037 | WSA-7K2 |
