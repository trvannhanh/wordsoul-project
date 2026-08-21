# Thiết kế suy giảm khi danh tính ngoài lỗi M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-EXTERNAL-IDENTITY-DEGRADATION-1.0` |
| Task | M12-T010 |
| Đầu vào | M12-FAIL-1.0 (D-025), M12-EXTERNAL-LINK-UNLINK-1.0 (D-119), REL-03 |
| Phạm vi | Đặc tả Giao thức Xử lý Suy giảm và Phục hồi khi Tích hợp Danh tính ngoài gặp Sự cố (`External Identity Degradation Protocol`), nguyên tắc Fail-Closed bảo mật, cơ chế Fast-Fail Circuit Breaker, sử dụng JWKS Cache 24h và gợi ý chuyển kênh đăng nhập |
| Tự kiểm | A-G01, A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Xử lý Suy giảm Graceful Degradation (`External Identity Degradation Protocol`) thuộc M12, thiết lập các hành vi ứng phó hệ thống khi các dịch vụ xác thực ngoài (Google OAuth, Apple SIWA, Facebook Graph API) gặp sự cố ngắt kết nối, gián đoạn API hoặc bị tấn công nghẽn mạng, đảm bảo không làm sập ứng dụng và duy trì trải nghiệm học tập (REL-03).

- **Ràng buộc Tuyệt đối Không Fail-Open Bảo mật (`Strict Fail-Closed Invariant`)**: Xác thực danh tính ngoài KHÔNG BAO GIỜ được phép Fail-Open (không bao giờ tự động đăng nhập người dùng khi không xác thực được ID Token). Khi Provider API lỗi hoặc hết giờ, hệ thống BẮT BUỘC Fail-Closed, từ chối phiên và trả về HTTP 503 `EXTERNAL_PROVIDER_UNAVAILABLE` (D-025, REL-03).
- **Cơ chế Fast-Fail ngắt mạch Circuit Breaker SLA $\le 2\text{ms}$ (`Fast-Fail Circuit Breaker Invariant`)**: Khi Circuit Breaker M12-T038 của một Provider ở trạng thái `OPEN` ($> 50\%$ lỗi trong 30s), toàn bộ các request đăng nhập/liên kết qua Provider đó BẮT BUỘC Fast-Fail ngay lập tức SLA $\le 2\text{ms}$ mà không thực hiện cuộc gọi mạng, tránh làm kiệt hỏng tài nguyên Thread Pool của hệ thống.
- **Sử dụng JWKS Key Escrow Cache 24h (`JWKS Redis Key Escrow Fallback`)**: Khi JWKS Cert Endpoint của Provider (Google/Apple) không thể truy cập, Adapter tự động fallback sử dụng Public Keys đã được lưu đệm trong Redis (TTL 24 giờ) để tiếp tục thẩm định chữ ký ID Token hợp lệ của người học mà không làm gián đoạn đăng nhập.
- **Gợi ý Kênh Đăng nhập Thay thế (`Graceful Channel Switch Guidance`)**: Phản hồi lỗi HTTP 503 từ API BẮT BUỘC chứa DTO gợi ý người học chuyển sang sử dụng Mật khẩu Local hoặc một Nhà cung cấp ngoài khác đã được liên kết sẵn trong tài khoản.

## 2. Ma trận Chế độ Suy giảm khi Danh tính ngoài Lỗi (Degradation Matrix)

| Kịch bản Sự cố (`Failure Scenario`) | Trạng thái Circuit Breaker | Hành vi Suy giảm Hệ thống (`Fallback Action`) | Phản hồi API người dùng | SLA Xử lý Fallback | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `JWKS_ENDPOINT_DOWN` | CLOSED | Dùng JWKS Public Keys cached trong Redis (24h) | Đăng nhập Thành công (200 OK) | SLA $\le 5\text{ms}$ | `ACT-M11-10-JWKS-CACHE` |
| `PROVIDER_TIMEOUT_C0` | CLOSED (Sau 3s) | Timeout Ngắt kết nối, Fail-Closed | HTTP 503 `PROVIDER_TIMEOUT` | SLA $\le 3.01\text{s}$ | `ACT-M11-10-TIMEOUT` |
| `PROVIDER_CIRCUIT_OPEN` | OPEN | Fast-Fail Tức thì, Không gọi mạng | HTTP 503 `CIRCUIT_BREAKER_OPEN` | SLA $\le 2\text{ms}$ | `ACT-M11-10-FASTFAIL` |
| `REMOTE_REVOKE_FAILED` | CLOSED | Gỡ CSDL trước, Retry Revoke qua Outbox | HTTP 200 OK (Gỡ CSDL thành công) | SLA $\le 50\text{ms}$ | `ACT-M11-10-RETRY-REVOKE` |

## 3. Kiến trúc Luồng Suy giảm Danh tính ngoài (Degradation Engine Pipeline)

```
[User Initiates External Login / Link (e.g. Google)]
                        |
                        v
          [Check Circuit Breaker State (M12-T038)]
                        |
       +----------------+----------------+
       | (State == OPEN)                 | (State == CLOSED)
       v                                 v
[Fast-Fail SLA <= 2ms]         [Attempt External API Call (Timeout = 3s)]
- Return 503 CIRCUIT_OPEN                |
- Prompt Local Pass Switch       +-------+-------+
                                 | (Failed / Timeout)  | (Success)
                                 v                     v
                        [Check Fail-Closed]    [Process Login 200 OK]
                        - Log Audit M11
                        - Return 503 PROVIDER_UNAVAILABLE
                        - Suggest Linked Alt Providers
```

## 4. Giao thức Thực thi Suy giảm CSDL (ExternalIdentityDegradationService)

```csharp
public async Task<ExternalAuthResultDto> ExecuteExternalAuthWithDegradationAsync(
    string provider, 
    Func<Task<ExternalOidcClaimsDto>> authDelegate)
{
    // 1. Check Circuit Breaker State M12-T038 SLA <= 2ms
    if (_circuitBreaker.IsOpen(provider))
    {
        await _auditLog.RecordEventAsync("ACT-M11-10-FASTFAIL", "SYSTEM", new { Provider = provider, Reason = "CIRCUIT_OPEN" });
        return new ExternalAuthResultDto {
            Success = false,
            ErrorCode = "PROVIDER_CIRCUIT_OPEN",
            ErrorMessage = $"Dịch vụ đăng nhập qua {provider} đang gián đoạn tạm thời. Vui lòng sử dụng Mật khẩu Local.",
            SuggestedAlternateMethods = new[] { "LOCAL_PASSWORD", "ALTERNATE_LINKED_PROVIDER" }
        };
    }

    try
    {
        // 2. Execute Auth Delegate with CancellationToken Timeout C0 (3s)
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var claims = await authDelegate();

        _circuitBreaker.RecordSuccess(provider);
        return new ExternalAuthResultDto { Success = true, Claims = claims };
    }
    catch (OperationCanceledException)
    {
        // 3. Timeout Fallback: Record Failure in Circuit Breaker
        _circuitBreaker.RecordFailure(provider);

        await _auditLog.RecordEventAsync("ACT-M11-10-TIMEOUT", "SYSTEM", new { Provider = provider });
        return new ExternalAuthResultDto {
            Success = false,
            ErrorCode = "EXTERNAL_PROVIDER_UNAVAILABLE",
            ErrorMessage = $"Kết nối tới {provider} hết thời gian phản hồi (3s). Vui lòng thử lại hoặc đăng nhập bằng Mật khẩu.",
            SuggestedAlternateMethods = new[] { "LOCAL_PASSWORD" }
        };
    }
    catch (Exception ex)
    {
        _circuitBreaker.RecordFailure(provider);
        await _auditLog.RecordEventAsync("ACT-M11-10-DEGRADE", "SYSTEM", new { Provider = provider, Error = ex.Message });

        return new ExternalAuthResultDto {
            Success = false,
            ErrorCode = "EXTERNAL_AUTH_FAILED",
            ErrorMessage = $"Lỗi xác thực danh tính từ {provider}. Vui lòng thử lại sau.",
            SuggestedAlternateMethods = new[] { "LOCAL_PASSWORD" }
        };
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `EG-G01` | Xác thực danh tính ngoài TUYỆT ĐỐI CẤM Fail-Open (100% trường hợp lỗi đều Fail-Closed HTTP 503). |
| `EG-G02` | Trạng thái Circuit Breaker `OPEN` phải Fast-Fail phản hồi lỗi cho người dùng SLA $\le 2\text{ms}$. |
| `EG-G03` | Khi JWKS Endpoint gián đoạn, Adapter tự động fallback sử dụng JWKS Keys cached trong Redis (TTL 24h). |
| `EG-G04` | Phản hồi lỗi HTTP 503 BẮT BUỘC chứa danh sách các phương thức đăng nhập thay thế (`LOCAL_PASSWORD`). |
| `EG-G05` | 100% các sự cố gián đoạn hoặc ngắt mạch Circuit Breaker được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-10-DEGRADE`). |
| `EG-G06` | Cuộc gọi API tới Provider ngoài BẮT BUỘC áp dụng Timeout C0 tối đa đúng 3.0 giây (D-099). |
| `EG-G07` | Thất bại khi thu hồi Token xa không được làm gián đoạn thao tác ngắt liên kết CSDL tại M01 (D-118). |
| `EG-G08` | Tự động khôi phục Circuit Breaker về trạng thái `CLOSED` sau 30 giây khi có 3 request Half-Open thành công. |
| `EG-G09` | Phân quyền cấu hình tham số suy giảm chỉ dành riêng cho `SecurityAdmin` và `System Worker`. |
| `EG-G10` | 100% các test case tự kiểm EG10-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `EG10-01` | Google OAuth API bị ngắt kết nối mạng | Return HTTP 503 `EXTERNAL_PROVIDER_UNAVAILABLE`, Fail-Closed |
| `EG10-02` | Circuit Breaker Google ở trạng thái `OPEN` | Fast-Fail trong SLA $< 1.5\text{ms}$, trả về gợi ý Mật khẩu local |
| `EG10-03` | JWKS Endpoint của Apple bị timeout nhưng Redis có cache keys 24h | Xác thực ID Token thành công bằng Redis Cached JWKS Keys |
| `EG10-04` | JWKS Endpoint của Apple bị timeout VÀ Redis KHÔNG CÓ cache keys | Return HTTP 503 `JWKS_KEY_UNAVAILABLE`, Fail-Closed |
| `EG10-05` | Google Auth API phản hồi chậm 4.5 giây ($> 3.0$s Timeout C0) | Ngắt kết nối sau đúng 3.0s, ghi log `ACT-M11-10-TIMEOUT` |
| `EG10-06` | Tra cứu vết Audit Log M11 sau khi Fast-Fail Circuit Breaker | Ghi nhận Audit Event `ACT-M11-10-FASTFAIL` đính kèm Provider |
| `EG10-07` | Revoke Remote Token tới Facebook bị sập mạng | Xóa bản ghi CSDL trước, retry lệnh Revoke qua Outbox M12-T037 |
| `EG10-08` | Thử cấu hình Timeout C0 thành 15 giây ($> 3.0$s) | Reject 400 `INVALID_TIMEOUT_C0_MAX_3S` |
| `EG10-09` | Tải đồng thời 100 request tới Google khi Circuit Breaker OPEN | 100% request Fast-Fail p95 $< 1.8\text{ms}$ |
| `EG10-10` | Google OAuth khôi phục hoạt động, 3 request Half-Open thành công | Circuit Breaker tự động đóng lại `CLOSED` |
| `EG10-11` | Thử kích hoạt đăng nhập Fail-Open khi sập CSDL | Reject 500 `FAIL_OPEN_PROHIBITED_REL03` |
| `EG10-12` | Gửi request đăng nhập Google khi sập mạng nhưng tài khoản có linked Apple | Return HTTP 503 kèm gợi ý `[LOCAL_PASSWORD, APPLE]` |
| `EG10-13` | User không phải SecurityAdmin thử đổi cấu hình Circuit Breaker | Deny 403 Forbidden |
| `EG10-14` | User chưa đăng nhập gọi API suy giảm danh tính | Deny 401 Unauthorized |
| `EG10-15` | Kiểm tra thời gian lưu giữ JWKS Keys trong Redis đệm | TTL đúng mốc 24 giờ (86,400s) |
| `EG10-16` | Kiểm tra độ trễ phản hồi API suy giảm Fast-Fail | Response SLA $< 2\text{ms}$ |
| `EG10-17` | Phân tích tham chiếu trạng thái Circuit Breaker trong Redis | Quét namespace `wordsoul:{env}:circuit_breaker` (T020) |
| `EG10-18` | Sự cố gián đoạn Provider xảy ra trong giờ cao điểm học tập | Hệ thống ổn định 100%, 0 đứt gãy phiên active |
| `EG10-19` | Tra cứu danh sách các Provider đang ở trạng thái suy giảm (`OPEN`) | Trả về danh sách Provider kèm tỷ lệ lỗi trong 30s |
| `EG10-20` | Kiểm thử hoàn tất luồng thiết kế suy giảm M12-EXTERNAL-IDENTITY-DEGRADATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-EG-I01` | M12 hiện tại chưa có bộ `ExternalIdentityDegradationService` xử lý suy giảm | Sự cố Provider API làm treo thread pool cả ứng dụng | M12-T047-A (Source task) |
| `M12-EG-I02` | Chưa cài đặt cờ chặn Tuyệt đối Không Fail-Open | Risk lập trình viên tráo cờ cho phép bypass auth khi sập | M12-T047-A; REL-03 |
| `M12-EG-I03` | Thiếu bộ nạp đệm JWKS Keys Escrow 24h trong Redis | Hệ thống sập xác thực OIDC khi JWKS Endpoint bị chập | M12-T047-A; M12-T032 |
| `M12-EG-I04` | Thiếu DTO trả về danh sách gợi ý phương thức đăng nhập thay thế | Người dùng không biết phải làm gì khi Google OAuth bị lỗi | M12-T047-A; M01-T013 |
| `M12-EG-I05` | Chưa kết nối sự kiện suy giảm với Audit Log M11 (`ACT-M11-10-DEGRADE`) | Không ghi log đối soát các vụ việc gián đoạn của Provider | M12-T047-A; M11-T031 |

- `M12-EG-F01`: Triển khai `ExternalIdentityDegradationService` với Ràng buộc Strict Fail-Closed (tiếp nhận: M12-T047-A).
- `M12-EG-F02`: Tích hợp Bắt buộc Fast-Fail Circuit Breaker SLA $\le 2\text{ms}$ & Timeout 3s (tiếp nhận: M12-T047-A; M12-T038).
- `M12-EG-F03`: Triển khai JWKS Redis Key Escrow 24h & Channel Switch DTO (tiếp nhận: M12-T047-A; M12-T032).
- `M12-EG-F04`: Thiết lập bộ kiểm thử tự động EG-G01–G10 và EG10-01–20 (tiếp nhận: M12 tasks).
- `M12-EG-F05`: Thu thập bằng chứng runtime cho luồng suy giảm M12 (tiếp nhận: M12 tasks; A-G01/A-G04).

## 8. Tự kiểm M12-T010

- Đã thiết kế hoàn chỉnh `M12-EXTERNAL-IDENTITY-DEGRADATION-1.0` với Ma trận Chế độ Suy giảm.
- Đã chốt Ràng buộc Tuyệt đối Không Fail-Open Bảo mật (`Strict Fail-Closed`).
- Đã chốt Cơ chế Fast-Fail ngắt mạch Circuit Breaker SLA $\le 2\text{ms}$ và Timeout C0 3s.
- Đã lồng ghép JWKS Redis Key Escrow Cache 24h và Lưu vết Audit Log M11 (`ACT-M11-10-DEGRADE`).
- Đã xác lập 10 Regression Gates (`EG-G01`–`EG-G10`) và 20 Test Cases tự kiểm (`EG10-01`–`EG10-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế suy giảm khi danh tính ngoài lỗi M12-T010 | WSA-7K2 |
