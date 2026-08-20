# Kiểm kê và phân loại bí mật M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-SECRET-INVENTORY-1.0` |
| Task | M12-T040 |
| Đầu vào | A0-T003 (REL-03), M12-T002 |
| Phạm vi | Danh mục kiểm kê toàn bộ bí mật hệ thống, phân loại 4 cấp độ rủi ro (S0-S3), quy định nạp/lưu trữ/xoay vòng bí mật và cơ chế chống rò rỉ |
| Tự kiểm | A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Danh mục Kiểm kê Bí mật Hệ thống (`Secret Inventory & Classification Catalog`) thuộc M12, quy định các tiêu chuẩn bảo mật, nạp bí mật từ Secret Manager và chính sách xoay vòng (`Secret Rotation Policy`) tuân thủ nguyên tắc an toàn dữ liệu REL-03 và A-G05.

- **Cấm Lưu Bí mật trong Source Code & File Log (`Zero Secret Ingestion Invariant`)**: Tuyệt đối KHÔNG lưu mật khẩu, private key, API token thô trong source code C#, config file JSON commit lên Git hay xuất ra file log.
- **Lưu trữ An toàn tập trung (`Vault / Secret Manager Integration`)**: Toàn bộ bí mật hệ thống được quản lý tập trung trên Azure Key Vault hoặc HashiCorp Vault. Ứng dụng nạp bí mật vào bộ nhớ RAM thời gian thực qua Environment Variables / Secret Provider Interface.
- **Phân loại 4 Cấp độ Rủi ro Bí mật (`S0` đến `S3`)**:
  - `S0_CRITICAL`: Private Key mã hóa, JWT Signing Key, CSDL Admin Password.
  - `S1_HIGH`: Third-Party API Keys (Azure TTS Key, OpenAI/Gemini Key, S3 Credentials, SMTP Password).
  - `S2_MEDIUM`: Mật khẩu Redis Cache, API Token trao đổi nội bộ microservices.
  - `S3_LOW`: Public OAuth Client IDs, App Configuration Hashes.
- **Bộ lọc Che giấu Tự động (`Automated Masking & Redaction`)**: Serilog / OpenTelemetry / Console sinks được cấu hình bộ lọc tự động thay thế mọi giá trị thuộc tính có tên chứa `Secret`, `Key`, `Password`, `Token` thành `[REDACTED_SECRET]`.

## 2. Bảng Kiểm kê và Phân loại Bí mật Hệ thống (Secret Classification Matrix)

| Mã Bí mật | Loại bí mật | Phân loại Rủi ro | Nguồn Nạp (`Secret Source`) | Chu kỳ Xoay vòng (`Rotation`) | Hành vi khi lọt lộ |
|---|---|---|---|---|---|
| `SEC-JWT-RSA-PRIV` | RSA Private Key ký JWT Token (M01) | `S0_CRITICAL` | Azure Key Vault / RAM | 90 ngày | Revoke toàn bộ token active, thu hồi session |
| `SEC-DB-CONN-STRING` | Chuỗi kết nối CSDL SQL Admin | `S0_CRITICAL` | Secret Manager / Env Var | 90 ngày | Khóa tài khoản DB, xoay vòng chuỗi kết nối |
| `SEC-AZURE-TTS-KEY` | API Key dịch vụ Azure Speech TTS (M05/M12) | `S1_HIGH` | Key Vault / Env Var | 180 ngày | Xoay key mới, vô hiệu hóa key cũ |
| `SEC-AI-PROVIDER-KEY` | API Key dịch vụ OpenAI / Gemini AI (M02/M12) | `S1_HIGH` | Key Vault / Env Var | 180 ngày | Hủy key bị lộ, phát key mới |
| `SEC-S3-SECRET-KEY` | S3 / Blob Storage Secret Access Key (M12) | `S1_HIGH` | Key Vault / Env Var | 180 ngày | Hủy IAM Access Key, cấp lại Key mới |
| `SEC-SMTP-PASS` | Mật khẩu tài khoản Email SMTP gửi mail | `S1_HIGH` | Key Vault / Env Var | 180 ngày | Đổi mật khẩu tài khoản gửi mail |
| `SEC-REDIS-PASS` | Mật khẩu kết nối CSDL Redis Cache | `S2_MEDIUM` | Key Vault / Env Var | 365 ngày | Đổi mật khẩu Redis Cluster |
| `SEC-INTERNAL-API-TOKEN` | Secret Token trao đổi API nội bộ M11/M12 | `S2_MEDIUM` | Key Vault / Env Var | 180 ngày | Đổi secret token giao tiếp service |
| `SEC-OAUTH-CLIENT-SECRET` | Client Secret cho Google/Facebook OAuth (M01) | `S1_HIGH` | Key Vault / Env Var | 180 ngày | Cập nhật OAuth App credentials |
| `SEC-PUSH-FCM-KEY` | Key gửi thông báo Firebase FCM (M10/M12) | `S1_HIGH` | Key Vault / Env Var | 365 ngày | Tạo Server Key mới trên Firebase Console |

## 3. Kiến trúc Nạp và Bảo vệ Bí mật tại Runtime (Secret Runtime Ingestion Engine)

```
       [Azure Key Vault / Secret Manager]
                       |
                       v (Secure TLS 1.3 / Managed Identity)
     [WordSoul Secret Ingestion Provider]
                       |
                       v (In-Memory Safe Injection)
     +-----------------+-----------------+
     |                                   |
     v                                   v
[Application Business Logic]    [Serilog / OpenTelemetry Sink]
(Use Secret in Memory Only)     (AUTOMATED REDACTION FILTER)
                                - Property: Password, Token, Key
                                - Output: [REDACTED_SECRET]
```

### 3.1. Cấu hình Serilog Enrichment Filter Chống Rò rỉ Bí mật
```csharp
public class SecretRedactionEnricher : ILogEventEnricher
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "Secret", "ApiKey", "Token", "PrivateKey", "ConnectionString", "Authorization"
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            if (SensitiveKeys.Any(k => property.Key.Contains(k)))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, "[REDACTED_SECRET]"));
            }
        }
    }
}
```

## 4. Giao thức Xoay vòng Bí mật (Secret Rotation Protocol)

Khi thực hiện xoay vòng bí mật (ví dụ `SEC-JWT-RSA-PRIV` hoặc `SEC-AZURE-TTS-KEY`):

1. **Giai đoạn 1 (Dual-Key Support Window)**: Đăng ký Key Mới vào Secret Vault, cấu hình hệ thống chấp nhận cả Key Cũ và Key Mới trong cửa sổ chuyển tiếp (30 phút cho Dual Key).
2. **Giai đoạn 2 (Switch Primary Key)**: Chuyển Key Mới thành Key Chính dùng để ký/gửi request.
3. **Giai đoạn 3 (Revoke Old Key)**: Hủy bỏ Key Cũ trên Secret Vault và thu hồi hoàn toàn sau khi xác nhận không còn request active dùng Key Cũ.
4. **Ghi vết Audit Event M11**: Mọi thao tác xoay vòng bí mật phải ghi vết sự kiện kiểm toán bất biến `ACT-M11-12` (M11-T031).

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SI-G01` | 100% bí mật hệ thống thuộc đúng 1 trong 4 cấp độ rủi ro `S0`, `S1`, `S2`, `S3`. |
| `SI-G02` | Cấm lưu trữ bí mật thô dưới dạng chuỗi hardcode trong source code hoặc file config Git. |
| `SI-G03` | Nạp 100% bí mật hệ thống từ Secret Manager / Key Vault vào bộ nhớ RAM thời gian thực. |
| `SI-G04` | Cấu hình bộ lọc Redaction tự động che giấu `[REDACTED_SECRET]` trên 100% các sink ghi log. |
| `SI-G05` | Tuân thủ chính sách xoay vòng bí mật định kỳ ($90 \to 365$ ngày) theo từng cấp độ rủi ro. |
| `SI-G06` | Thao tác xoay vòng bí mật `S0` / `S1` hỗ trợ cửa sổ Dual-Key để không làm đứt đoạn dịch vụ. |
| `SI-G07` | Khi phát hiện lọt lộ bí mật, hệ thống hỗ trợ lệnh thu hồi khẩn cấp (`Emergency Secret Revocation`). |
| `SI-G08` | API responses và Audit Event Envelopes tuyệt đối KHÔNG chứa bí mật thô. |
| `SI-G09` | Phân quyền truy cập Secret Manager tuân thủ nghiêm ngặt ma trận vai trò M11 (`R08 Operations Admin`). |
| `SI-G10` | 100% các test case tự kiểm SI40-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SI40-01` | Quét toàn bộ C# codebase trong `WordSoulApi` tìm secret hardcode | Không phát hiện bất kỳ chuỗi secret/password hardcode nào |
| `SI40-02` | Nạp `SEC-AZURE-TTS-KEY` từ Environment Variable vào RAM | Nạp bí mật thành công vào service |
| `SI40-03` | Cố tình ghi log đối tượng DTO chứa thuộc tính `Password` | Serilog enrichment mask thành `[REDACTED_SECRET]` |
| `SI40-04` | Cố tình ghi log chuỗi kết nối CSDL `SEC-DB-CONN-STRING` | Serilog enrichment mask thành `[REDACTED_SECRET]` |
| `SI40-05` | Thực hiện xoay vòng `SEC-JWT-RSA-PRIV` với cửa sổ Dual-Key 30 phút | Token phát bằng Key Mới và Key Cũ đều xác thực hợp lệ |
| `SI40-06` | Kết thúc cửa sổ 30 phút xoay key JWT | Key Cũ bị revoke, chỉ Key Mới được chấp nhận |
| `SI40-07` | Thử nạp bí mật từ Key Vault bị lỗi kết nối | System ném ngoại lệ fail-closed, không dùng fallback thô |
| `SI40-08` | Kiểm tra thời hạn xoay vòng đối với bí mật rủi ro `S0` | Đặt lịch cảnh báo xoay vòng sau 90 ngày |
| `SI40-09` | Kiểm tra thời hạn xoay vòng đối với bí mật rủi ro `S1` | Đặt lịch cảnh báo xoay vòng sau 180 ngày |
| `SI40-10` | Khai thác API DTO trả về thông tin cấu hình tích hợp M12 | DTO không chứa secret key, chỉ chứa metadata công khai |
| `SI40-11` | Thực thi lệnh Thu hồi Bí mật Khẩn cấp khi phát hiện lọt lộ | Hủy bí mật cũ trên Key Vault và nạp bí mật mới trong $\le 5$ phút |
| `SI40-12` | Tra cứu vết Audit Log M11 sau khi xoay vòng bí mật | Ghi nhận Audit Event `ACT-M11-12` với thông tin actor an toàn |
| `SI40-13` | User không phải Operations Admin yêu cầu đọc cấu hình Key Vault | Deny 403 Forbidden |
| `SI40-14` | Tải đồng thời 50 request truy cập dịch vụ cần nạp secret từ RAM | Response p95 $< 10\text{ms}$ từ in-memory cache |
| `SI40-15` | Kiểm tra tính mã hóa khi truyền secret qua đường truyền mạng | 100% truyền qua TLS 1.3 mã hóa an toàn |
| `SI40-16` | Thử commit file `.env` chứa secret key lên Git | Git pre-commit hook reject commit vi phạm |
| `SI40-17` | Kiểm tra sự che giấu secret trong Audit Event Diff Envelope | Diff hiển thị `[REDACTED_SECRET]` cho secret payload |
| `SI40-18` | Xoay vòng bí mật `SEC-S3-SECRET-KEY` của hạ tầng Blob Storage | Cập nhật Storage Provider credentials thành công |
| `SI40-19` | Phân tích phụ thuộc trước khi thu hồi 1 bí mật hệ thống | Quét các dịch vụ M12 đang sử dụng bí mật đó (T020) |
| `SI40-20` | Kiểm thử hoàn tất luồng kiểm kê và phân loại bí mật M12-SECRET-INVENTORY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-SI-I01` | Trong `appsettings.json` hiện tại có chứa một số chuỗi connection string mẫu | Cần đảm bảo khi deploy staging/production 100% nạp từ Secret Manager | M12-T049 (Source task) |
| `M12-SI-I02` | Chưa cài đặt `SecretRedactionEnricher` trên Serilog | Rủi ro ghi lỡ secret/token vào file log Operational | M12-T049 |
| `M12-SI-I03` | Thiếu danh mục kiểm kê 4 cấp độ rủi ro bí mật (S0-S3) trong codebase | Khó khăn cho đội vận hành khi theo dõi chu kỳ xoay vòng key | M12-T049 |
| `M12-SI-I04` | Chưa triển khai giao thức xoay vòng Dual-Key cho JWT RSA Key | Xoay key JWT có thể làm ngắt đột ngột session của người dùng | M12-T049 |
| `M12-SI-I05` | Chưa có Git Pre-commit Hook tự động quét lọt lộ secret | Rủi ro lập trình viên vô tình commit file `.env` chứa key thô | M12-T049 |

- `M12-SI-F01`: Tích hợp Azure Key Vault / Secret Manager Provider vào `Program.cs` (tiếp nhận: M12-T049).
- `M12-SI-F02`: Triển khai `SecretRedactionEnricher` cho Serilog và OpenTelemetry (tiếp nhận: M12-T049).
- `M12-SI-F03`: Xây dựng `DualKeyRotationHandler` cho JWT và API keys (tiếp nhận: M12-T049).
- `M12-SI-F04`: Thiết lập bộ kiểm thử tự động SI-G01–G10 và SI40-01–20 (tiếp nhận: M12 tasks).
- `M12-SI-F05`: Thu thập bằng chứng runtime cho luồng kiểm kê bí mật M12 (tiếp nhận: M12 tasks; A-G05/REL-03).

## 8. Tự kiểm M12-T040

- Đã thiết kế hoàn chỉnh `M12-SECRET-INVENTORY-1.0` với Bảng Kiểm kê và Phân loại Bí mật 4 cấp độ rủi ro (S0-S3).
- Đã chốt quy tắc cấm hardcode secret và nạp an toàn từ Vault/Secret Manager vào RAM.
- Đã xây dựng bộ lọc Serilog Enrichment Redaction tự động che giấu `[REDACTED_SECRET]`.
- Đã chốt Giao thức Xoay vòng Bí mật Dual-Key 3 giai đoạn và lịch xoay vòng định kỳ ($90 \to 365$ ngày).
- Đã xác lập 10 Regression Gates (`SI-G01`–`SI-G10`) và 20 Test Cases tự kiểm (`SI40-01`–`SI40-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả kiểm kê và phân loại bí mật M12-T040 | WSA-7K2 |
