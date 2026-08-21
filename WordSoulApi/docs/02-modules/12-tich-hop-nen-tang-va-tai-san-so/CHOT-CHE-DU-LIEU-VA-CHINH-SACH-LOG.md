# Chốt che dữ liệu và chính sách log M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-DATA-REDACTION-LOG-POLICY-1.0` |
| Task | M12-T043 |
| Đầu vào | M12-SECRET-INVENTORY-1.0 (D-069), M12-SECRET-LIFECYCLE-1.0 (D-113), M11-DATA-REDACTION-LOG-POLICY-1.0 (D-123), REL-03 |
| Phạm vi | Đặc tả Giao thức Che Dữ liệu và Chính sách Log Tích hợp Nền tảng (`Integration Secret & Payload Redaction Policy Protocol`), quy tắc lọc HTTP Headers & Body nhạy cảm, bộ lọc ẩn danh hóa prompt AI Gemini và lưu vết kiểm toán M11 |
| Tự kiểm | A-G02, A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Che Dữ liệu và Chính sách Log Tích hợp Nền tảng (`Integration Secret & Payload Redaction Policy Protocol`) thuộc M12, thiết lập rào chắn bảo vệ cho toàn bộ các cuộc gọi HTTP/gRPC đối thoại với các nền tảng bên ngoài (Google OAuth, Apple SIWA, Facebook Graph, AI Gemini, Cloud Storage), đảm bảo bí mật tích hợp S0–S3 và dữ liệu nhạy cảm không bị phơi nhiễm ra nhật ký ứng dụng (REL-03).

- **Nguyên tắc Che mờ HTTP Authorization Headers (`Header Redaction Invariant`)**: $100\%$ các HTTP Request/Response Headers chứa thông tin xác thực (`Authorization`, `X-Api-Key`, `Cookie`, `Set-Cookie`, `X-Apple-Client-Secret`) BẮT BUỘC phải được thay thế giá trị thành `***REDACTED***` bởi Middleware `IntegrationHttpRedactionHandler` trước khi ghi vết Log (REL-03).
- **Bộ lọc An danh hóa Prompt AI Gemini (`AI Prompt Anonymization Filter Invariant`)**: Trước khi gửi Prompt sang dịch vụ AI Gemini M12, bộ lọc `PromptAnonymizerFilter` BẮT BUỘC quét và ẩn danh hóa toàn bộ các thông tin PII (như email, số điện thoại, tên riêng người học) thành các token giả định (`[USER_EMAIL]`, `[USER_NAME]`), ngăn ngừa rò rỉ dữ liệu cá nhân ra mô hình AI ngoài (D-102, REL-01).
- **Tuyệt đối CẤM Log Body HTTP Chứa Master Keys hoặc Ciphertexts (`Zero-Ciphertext Logging Invariant`)**: Request/Response JSON Payloads chứa Refresh Token Ciphertexts, Pre-Signed Storage URLs hoặc Key Material BẮT BUỘC bị che mờ từng trường nhạy cảm (`encryptedRefreshToken: "***REDACTED***"`).
- **Lưu vết Vi phạm Log Tích hợp M11 (`Integration Logging Audit Trail`)**: $100\%$ các lần kích hoạt bộ lọc che mờ log HTTP tích hợp được ghi vết bất biến `ACT-M11-43-LOG` trong Sổ Kiểm toán M11, ghi nhận `Provider`, `EndpointPath`, `RedactedFieldsCount` và `StatusCode`.

## 2. Ma trận Quy tắc Che mờ Dữ liệu Tích hợp M12 (Integration Redaction Matrix)

| Loại Tích hợp (`IntegrationType`) | Đối tượng Nhạy cảm (`Sensitive Target`) | Quy tắc Che mờ (`Redaction Policy`) | Vị trí Thực thi Handler | Vết Kiểm toán M11 |
|---|---|---|---|---|
| `OAUTH_PROVIDERS` | `code`, `client_secret`, `access_token`, `refresh_token` | Mask Body JSON & Authorization Header | `IntegrationHttpRedactionHandler` | `ACT-M11-43-OAUTH` |
| `AI_GEMINI` | `apiKey`, Prompt User PII (email, name, phone) | Mask Header `x-goog-api-key` & Anonymize Prompt | `PromptAnonymizerFilter` | `ACT-M11-43-GEMINI` |
| `ASSET_STORAGE` | `X-Amz-Signature`, Pre-Signed Upload URLs | Redact Signature Query String & SAS Tokens | `StorageClientLogFilter` | `ACT-M11-43-STORAGE` |
| `DATABASE_MASTER` | `masterEncryptionKey`, AES-256 Ciphertexts | Mask DB Parameter Values in Serilog | `DbCommandRedactionInterceptor` | `ACT-M11-43-DB` |

## 3. Kiến trúc Bộ lọc Che mờ HTTP & AI Prompt M12 (Redaction Pipeline)

```
[M12 Integration Service Initiates External Call (OAuth / AI Gemini / Storage)]
                                 |
                                 v
            [IntegrationHttpRedactionHandler Middleware]
                                 |
         +-----------------------+-----------------------+
         | (Is AI Gemini Prompt?)                        | (Standard HTTP Request)
         v                                               v
[PromptAnonymizerFilter]                        [Redact Sensitive Headers]
- Replace PII with [USER_EMAIL]                 - Authorization: Bearer ***REDACTED***
- Replace Name with [USER_NAME]                 - x-goog-api-key: ***REDACTED***
         |                                               |
         +-----------------------+-----------------------+
                                 |
                                 v
            [Redact JSON Payload Body (Tokens & Secrets)]
                                 |
                                 v
            [Pass Cleaned HttpRequestMessage to HttpClient]
                                 |
                                 v
            [Record Audit Event ACT-M11-43-LOG in M11]
```

## 4. Giao thức Thực thi Handler Che mờ HTTP CSDL (IntegrationHttpRedactionHandler)

```csharp
public class IntegrationHttpRedactionHandler : DelegatingHandler
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "X-Api-Key", "x-goog-api-key", "Cookie", "Set-Cookie", "X-Apple-Client-Secret"
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Sanitize Request Headers
        var sanitizedHeaders = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            if (SensitiveHeaders.Contains(header.Key))
            {
                sanitizedHeaders[header.Key] = "***REDACTED***";
            }
            else
            {
                sanitizedHeaders[header.Key] = string.Join(", ", header.Value);
            }
        }

        // 2. Sanitize Request Body if JSON
        string sanitizedBody = "[EMPTY_OR_NON_JSON]";
        if (request.Content != null)
        {
            string rawBody = await request.Content.ReadAsStringAsync(cancellationToken);
            sanitizedBody = RedactJsonBody(rawBody);
        }

        // 3. Execute HTTP Call
        var response = await base.SendAsync(request, cancellationToken);

        // 4. Log Cleaned Telemetry via Serilog
        Log.Information("M12 External Call: {Method} {Uri} | Status: {Status} | Headers: {@Headers} | Body: {Body}",
            request.Method, request.RequestUri?.GetLeftPart(UriPartial.Path), response.StatusCode, sanitizedHeaders, sanitizedBody);

        return response;
    }

    private static string RedactJsonBody(string json)
    {
        if (string.IsNullOrEmpty(json) || !json.TrimStart().StartsWith('{')) return json;
        // Regex replace token, secret, password fields with ***REDACTED***
        return Regex.Replace(json, @"(?i)""(client_secret|access_token|refresh_token|password|code|apiKey)""\s*:\s*""[^""]+""", @"""$1"":""***REDACTED***""");
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IR-G01` | 100% HTTP Authorization Headers (`Authorization`, `x-goog-api-key`) xuất ra log bị thay bằng `***REDACTED***`. |
| `IR-G02` | Prompt AI Gemini M12 BẮT BUỘC trải qua bộ lọc `PromptAnonymizerFilter` ẩn danh hóa PII trước khi phát đi (D-102). |
| `IR-G03` | Request/Response JSON Body chứa Refresh Tokens hoặc Client Secrets bị che mờ từng trường nhạy cảm. |
| `IR-G04` | Pre-Signed URLs của Cloud Storage bị xóa bỏ tham số chữ ký nhạy cảm (`X-Amz-Signature`) trong dòng log. |
| `IR-G05` | 100% các thao tác lọc log tích hợp được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-43-LOG`). |
| `IR-G06` | Middleware `IntegrationHttpRedactionHandler` được tích hợp tự động vào $100\%$ các instance `HttpClient` M12. |
| `IR-G07` | SLA xử lý bộ lọc che mờ HTTP Request/Response $< 1.0\text{ms}$ per request. |
| `IR-G08` | Phân quyền cấu hình danh sách `SensitiveHeaders` chỉ dành riêng cho `SecurityAdmin` và `System Worker`. |
| `IR-G09` | Kiểm tra tự động bằng Unit Test quét 100% HTTP trace logs trong bộ build CI/CD pipeline. |
| `IR-G10` | 100% các test case tự kiểm IR43-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IR43-01` | Gửi HTTP Request có `Authorization: Bearer eyJhbGci...` tới Google OAuth API | Log Output hiển thị `"Authorization": "***REDACTED***"` |
| `IR43-02` | Gửi HTTP Request tới AI Gemini với header `x-goog-api-key: AIzaSy...` | Log Output hiển thị `"x-goog-api-key": "***REDACTED***"` |
| `IR43-03` | Gửi Prompt AI chứa email `learner@example.com` sang Gemini | Prompt biến đổi thành `[USER_EMAIL]` trước khi phát đi |
| `IR43-04` | Gửi OAuth Code Exchange JSON Body `{"client_secret": "sec_123"}` | Log Output hiển thị `"client_secret": "***REDACTED***"` |
| `IR43-05` | Gửi Pre-Signed Upload URL chứa `X-Amz-Signature=abc123xyz` | Log Output che mờ signature thành `X-Amz-Signature=***REDACTED***` |
| `IR43-06` | Tra cứu vết Audit Log M11 sau khi thực thi Handler che mờ | Ghi nhận Audit Event `ACT-M11-43-LOG` đính kèm Provider |
| `IR43-07` | Gửi HTTP Request có header `Cookie: session_id=xyz` | Log Output hiển thị `"Cookie": "***REDACTED***"` |
| `IR43-08` | Thử disable middleware `IntegrationHttpRedactionHandler` trong config | Reject 500 `REDACTION_HANDLER_MANDATORY_REL03` |
| `IR43-09` | Tải đồng thời 1,000 HTTP Requests tích hợp M12 qua Handler | Redaction processing latency p95 $< 0.8\text{ms}$ per request |
| `IR43-10` | Response JSON từ Apple OAuth chứa `id_token` nhạy cảm | Log Output che mờ `id_token` thành `***REDACTED***` |
| `IR43-11` | Gửi Prompt AI chứa tên riêng `Trần Văn A` sang Gemini | Prompt biến đổi thành `[USER_NAME]` trước khi phát đi |
| `IR43-12` | Ghi log tham số DB Query chứa `masterEncryptionKey` | Interceptor che mờ tham số CSDL thành `***REDACTED***` |
| `IR43-13` | User không phải SecurityAdmin thử chỉnh sửa danh sách `SensitiveHeaders` | Deny 403 Forbidden |
| `IR43-14` | User chưa đăng nhập gọi API cấu hình chính sách log M12 | Deny 401 Unauthorized |
| `IR43-15` | Phản hồi lỗi HTTP 400 Bad Request từ Google chứa OAuth error payload | Log Output hiển thị lỗi đã che mờ secret |
| `IR43-16` | Kiểm tra thời gian che mờ 500 HTTP Requests tích hợp liên tục | Total SLA $< 300\text{ms}$ |
| `IR43-17` | Phân tích tham chiếu các quy tắc che mờ tích hợp trong CSDL | Quét schema `M12_IntegrationRedactionRules` (T020) |
| `IR43-18` | Thao tác ghi log telemetry M12 sang Seq bị gián đoạn do đứt mạng | Log đệm file local, tiếp tục che mờ khi ghi lại |
| `IR43-19` | Tra cứu danh sách các HTTP Headers đang được che mờ | Trả về danh sách 6 headers bảo mật tiêu chuẩn |
| `IR43-20` | Kiểm thử hoàn tất luồng chốt che dữ liệu M12-DATA-REDACTION-LOG-POLICY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-IR-I01` | M12 hiện tại chưa có `IntegrationHttpRedactionHandler` cho HttpClient | Risk ghi thô Bearer Tokens và API Keys vào log Seq | M12-T047-A (Source task) |
| `M12-IR-I02` | Chưa cài đặt `PromptAnonymizerFilter` ẩn danh hóa PII cho AI Gemini | Risk phơi nhiễm email và tên học viên sang Google Gemini LLM | M12-T047-A; M01-T033 |
| `M12-IR-I03` | Thiếu bộ lọc Signature Query String cho Cloud Storage Pre-Signed URLs | Pre-Signed Upload URLs có thể bị đánh cắp qua log file | M12-T047-A; M12-T024 |
| `M12-IR-I04` | Thiếu cờ bắt buộc tích hợp Handler vào 100% `HttpClient` M12 | Lập trình viên có thể tạo HttpClient mới bypass bộ lọc | M12-T047-A; REL-03 |
| `M12-IR-I05` | Chưa kết nối sự kiện lọc log với Audit Log M11 (`ACT-M11-43-LOG`) | Không ghi vết được số lượng headers và fields được che mờ | M12-T047-A; M11-T031 |

- `M12-IR-F01`: Triển khai `IntegrationHttpRedactionHandler` bảo vệ 100% HttpClient M12 (tiếp nhận: M12-T047-A).
- `M12-IR-F02`: Tích hợp Bắt buộc `PromptAnonymizerFilter` cho AI Gemini (tiếp nhận: M12-T047-A; M01-T033).
- `M12-IR-F03`: Triển khai Storage Pre-Signed URL Signature Redaction (tiếp nhận: M12-T047-A; M12-T024).
- `M12-IR-F04`: Thiết lập bộ kiểm thử tự động IR-G01–G10 và IR43-01–20 (tiếp nhận: M12 tasks).
- `M12-IR-F05`: Thu thập bằng chứng runtime cho luồng che mờ log M12 (tiếp nhận: M12 tasks; A-G02/A-G05).

## 8. Tự kiểm M12-T043

- Đã thiết kế hoàn chỉnh `M12-DATA-REDACTION-LOG-POLICY-1.0` với Ma trận Quy tắc Che mờ Dữ liệu Tích hợp.
- Đã chốt Ràng buộc Che mờ HTTP Authorization Headers (`IntegrationHttpRedactionHandler`).
- Đã chốt Bộ lọc Ẩn danh hóa Prompt AI Gemini (`PromptAnonymizerFilter`).
- Đã lồng ghép Nguyên tắc Tuyệt đối CẤM Log Body HTTP Chứa Master Keys/Ciphertexts và Audit Log M11 (`ACT-M11-43-LOG`).
- Đã xác lập 10 Regression Gates (`IR-G01`–`IR-G10`) và 20 Test Cases tự kiểm (`IR43-01`–`IR43-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt che dữ liệu và chính sách log M12-T043 | WSA-7K2 |
