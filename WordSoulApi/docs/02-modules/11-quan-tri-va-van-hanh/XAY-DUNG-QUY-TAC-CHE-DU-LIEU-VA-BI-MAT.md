# Xây dựng quy tắc che dữ liệu và bí mật M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-DATA-REDACTION-LOG-POLICY-1.0` |
| Task | M11-T033 |
| Đầu vào | M11-LOG-TAXONOMY-1.0 (D-055), M12-SECRET-INVENTORY-1.0 (D-069), M12-SECRET-LIFECYCLE-1.0 (D-113), REL-02, REL-03 |
| Phạm vi | Đặc tả Giao thức Che Dữ liệu và Bảo vệ Bí mật trong Nhật ký (`Data Redaction & Secret Logging Policy Protocol`), 4 danh mục trường bí mật bắt buộc che mờ, bộ lọc `SerilogRedactionEnricher` tự động, mã băm SHA-256 kèm Salt cho IP/PII và quy định kiểm toán |
| Tự kiểm | A-G02, A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Che Dữ liệu và Bảo vệ Bí mật trong Nhật ký (`Data Redaction & Secret Logging Policy Protocol`) thuộc M11, xác lập bộ quy tắc bắt buộc áp dụng cho toàn bộ các lớp log (Serilog Operational Logs, Activity Logs, Audit Logs), ngăn chặn triệt để nguy cơ rò rỉ bí mật hệ thống S0–S3 và dữ liệu cá nhân PII ra các hệ thống ghi log tập trung (ELK / Seq / CloudWatch) (REL-02, REL-03).

- **Nguyên tắc Tuyệt đối Không Ghi Vết Bí mật (`Zero-Secret Logging Invariant`)**: $100\%$ các trường chứa mật khẩu local (`password`, `passwordHash`), khóa bí mật (`clientSecret`, `privateKey`), JWT Tokens (`accessToken`, `refreshToken`), Master Key và AES Ciphertexts TUYỆT ĐỐI CẤM xuất hiện dưới dạng văn bản thô trong bất kỳ dòng log nào. Bộ lọc BẮT BUỘC thay thế bằng chuỗi cố định `***REDACTED***` (REL-03).
- **Quy tắc Che mờ Dữ liệu Cá nhân PII (`PII Masking & Anonymization Rules`)**:
  - *Email*: Giữ 1 ký tự đầu và tên miền (`j***n@domain.com`).
  - *Số điện thoại*: Giữ 2 số đầu và 4 số cuối (`09****1234`).
  - *Địa chỉ IP*: BẮT BUỘC chuyển đổi thành salted SHA-256 hash (`IP_HASH = SHA256(IP + SystemSalt)`).
  - *Họ tên người học*: Che mờ 50% độ dài ký tự (`Tr**n V** A`).
- **Bộ lọc Che mờ Tự động ở Cấp độ Middleware Serilog (`Serilog Redaction Enricher`)**: Mọi log payload xuất ra từ ứng dụng BẮT BUỘC trải qua bộ lọc `RedactionDestructuringPolicy` của Serilog trước khi ghi vào đĩa hoặc đẩy qua mạng. CẤM lập trình viên ghi log thủ công các đối tượng DTO chứa trường nhạy cảm mà không qua policy (REL-03).
- **Lưu vết Vi phạm Che mờ Nhật ký M11 (`Redaction Violation Audit Trail`)**: Khi phát hiện có cố gắng ghi log chứa từ khóa bí mật bị cấm, hệ thống TỰ ĐỘNG ngắt dòng log đó, thay bằng cảnh báo an ninh `ACT-M11-33-REDACT_VIOLATION` gửi về `SecurityAdmin`.

## 2. Ma trận Quy tắc Che mờ Dữ liệu và Bí mật (Redaction Policy Matrix)

| Danh mục Nhạy cảm (`Category`) | Danh sách Trường Mẫu (`Target Fields`) | Quy tắc Che mờ (`Redaction Rule`) | Giá trị Xuất Nhật ký (`Logged Output`) | Vết Kiểm toán M11 |
|---|---|---|---|---|
| `SYSTEM_SECRETS` | `password`, `secret`, `token`, `masterKey`, `privateKey` | **FULL EVICTION** | `***REDACTED***` | `ACT-M11-33-SECRET` |
| `PII_EMAIL` | `email`, `userEmail`, `contactEmail` | Canonical Email Mask | `a***n@example.com` | `ACT-M11-33-PII` |
| `PII_PHONE` | `phoneNumber`, `mobile`, `phone` | Phone Number Mask | `09****5678` | `ACT-M11-33-PII` |
| `NETWORK_IP` | `ipAddress`, `clientIp`, `remoteIp` | Salted SHA-256 Hash | `hash:e3b0c44298fc...` | `ACT-M11-33-IP` |
| `PAYMENT_CREDIT` | `cardNumber`, `cvv`, `accountNo` | PCI-DSS Standard Mask | `**** **** **** 4321` | `ACT-M11-33-PCI` |

## 3. Kiến trúc Bộ lọc Che mờ Log Tự động (Redaction Pipeline)

```
[Application Trace / Serilog Log Event]
                   |
                   v
  [Serilog RedactionDestructuringPolicy]
                   |
     +-------------+-------------+
     | (Contains Forbidden Key) | (Normal Field)
     v                           v
[Replace Value with ***REDACTED***] [Pass Original Value]
     |                           |
     +-------------+-------------+
                   |
                   v
  [Apply PII Masking & IP Salted SHA-256 Hash]
                   |
                   v
  [Write Cleaned Event to Log Sink (ELK / Disk / Seq)]
```

## 4. Giao thức Thực thi Che mờ Serilog (SerilogRedactionEnricher Implementation)

```csharp
public class SerilogRedactionEnricher : ILogEventEnricher
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordhash", "secret", "clientsecret", "accesstoken", 
        "refreshtoken", "privatekey", "masterkey", "encryptedrefreshtoken"
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            if (SensitiveKeys.Contains(property.Key))
            {
                // Force overwrite sensitive key value with ***REDACTED***
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, "***REDACTED***"));
            }
            else if (property.Key.Equals("ipAddress", StringComparison.OrdinalIgnoreCase))
            {
                // Hash IP with salted SHA-256
                string rawIp = property.Value.ToString().Trim('"');
                string hashedIp = SaltedIpHasher.HashIp(rawIp);
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, $"hash:{hashedIp}"));
            }
            else if (property.Key.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                // Mask Email
                string rawEmail = property.Value.ToString().Trim('"');
                string maskedEmail = MaskEmail(rawEmail);
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Key, maskedEmail));
            }
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return "***@domain.com";
        var parts = email.Split('@');
        string name = parts[0];
        string maskedName = name.Length <= 2 ? name[0] + "***" : name[0] + "***" + name[^1];
        return $"{maskedName}@{parts[1]}";
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DR-G01` | 100% bí mật hệ thống (`password`, `token`, `secret`, `masterKey`) bị che mờ thành `***REDACTED***` trong log. |
| `DR-G02` | Địa chỉ IP người dùng xuất ra log BẮT BUỘC mã hóa dạng Salted SHA-256 Hash (`hash:xxx`). |
| `DR-G03` | Email và Số điện thoại cá nhân trong log bắt buộc phải được che mờ 70% độ dài ký tự theo chuẩn PII. |
| `DR-G04` | Bộ lọc che mờ `SerilogRedactionEnricher` bắt buộc được tích hợp ở cấp độ Middleware Serilog gốc. |
| `DR-G05` | Tuyệt đối CẤM lập trình viên ghi log thủ công bằng `Console.WriteLine` bypass bộ lọc Serilog (REL-03). |
| `DR-G06` | 100% các vi phạm thử ghi log chứa từ khóa bí mật được cảnh báo về Sổ Kiểm toán M11 (`ACT-M11-33-SECRET`). |
| `DR-G07` | SLA xử lý thực thi bộ lọc che mờ log không làm tăng độ trễ ghi log $> 0.5\text{ms}$ per log event. |
| `DR-G08` | Phân quyền thay đổi danh mục từ khóa nhạy cảm `SensitiveKeys` chỉ dành riêng cho `SecurityAdmin`. |
| `DR-G09` | Kiểm tra tự động bằng Unit Test quét 100% log payloads trong bộ build CI/CD pipeline. |
| `DR-G10` | 100% các test case tự kiểm DR33-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DR33-01` | Ghi log câu lệnh DTO chứa thuộc tính `password` | Log Output hiển thị `"password": "***REDACTED***"` |
| `DR33-02` | Ghi log câu lệnh DTO chứa thuộc tính `refreshToken` | Log Output hiển thị `"refreshToken": "***REDACTED***"` |
| `DR33-03` | Ghi log sự kiện có chứa `ipAddress = "192.168.1.50"` | Log Output hiển thị `"ipAddress": "hash:e3b0c442..."` |
| `DR33-04` | Ghi log chứa email `john.doe@gmail.com` | Log Output hiển thị `"email": "j***e@gmail.com"` |
| `DR33-05` | Ghi log chứa số điện thoại `0912345678` | Log Output hiển thị `"phoneNumber": "09****5678"` |
| `DR33-06` | Tra cứu vết Audit Log M11 sau khi phát hiện vi phạm thử ghi secret | Ghi nhận Audit Event `ACT-M11-33-SECRET` |
| `DR33-07` | Ghi log DTO lồng nhau 3 cấp chứa thuộc tính `clientSecret` | Bộ lọc đệ quy che mờ `clientSecret` thành `***REDACTED***` |
| `DR33-08` | Thử ghi log bằng từ khóa hoa thường hỗn hợp `PassWord` | Serilog Enricher Case-Insensitive che mờ chính xác |
| `DR33-09` | Tải đồng thời 10,000 log events trải qua bộ lọc che mờ Serilog | Redaction processing latency p95 $< 0.3\text{ms}$ per event |
| `DR33-10` | Ghi log số thẻ tín dụng 16 chữ số trong giao dịch thanh toán M06 | Log Output hiển thị `"cardNumber": "**** **** **** 4321"` |
| `DR33-11` | Kiểm tra tính bất biến của Salt dùng cho mã băm IP Hash | Salt lưu giữ trong Secret Manager M12-T040, 0 hardcode |
| `DR33-12` | Ghi log biến cấu hình `MasterEncryptionKey` | Log Output hiển thị `"masterKey": "***REDACTED***"` |
| `DR33-13` | User không phải SecurityAdmin thử thêm từ khóa vào `SensitiveKeys` | Deny 403 Forbidden |
| `DR33-14` | Ghi log lỗi ngoại lệ Exception StackTrace chứa chuỗi kết nối DB | StackTrace được làm sạch, che mờ DB Password |
| `DR33-15` | Quét mã nguồn ứng dụng kiểm tra gọi `Console.WriteLine` | 0 kết quả (bị chặn hoàn toàn bởi Linter CI/CD) |
| `DR33-16` | Kiểm tra thời gian che mờ 1,000 log events liên tục | Total SLA $< 200\text{ms}$ |
| `DR33-17` | Phân tích tham chiếu các quy tắc che mờ trong CSDL | Quét schema `M11_LogRedactionRules` (T020) |
| `DR33-18` | Thao tác ghi log sang Seq/ELK bị ngắt kết nối đứt mạng | Log đệm local file, tiếp tục che mờ khi ghi lại |
| `DR33-19` | Tra cứu danh sách các từ khóa đang được bảo vệ trong `SensitiveKeys` | Trả về danh sách 15 từ khóa bảo mật tiêu chuẩn |
| `DR33-20` | Kiểm thử hoàn tất luồng quy tắc che dữ liệu M11-DATA-REDACTION-LOG-POLICY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-DR-I01` | M11 hiện tại chưa có `SerilogRedactionEnricher` cài đặt ở Serilog Pipeline | Risk ghi thô password và token vào log ứng dụng | M11-T049 (Source task) |
| `M11-DR-I02` | Chưa cài đặt mã băm Salted SHA-256 Hash cho IP Address trong log | Trực tiếp ghi thô IP người dùng vi phạm quyền riêng tư PII | M11-T049; REL-03 |
| `M11-DR-I03` | Thiếu bộ lọc Mask Email và Phone Number trong Serilog Destructuring | Phơi nhiễm thông tin cá nhân của người học ra Seq/ELK | M11-T049; REL-02 |
| `M11-DR-I04` | Thiếu Linter Rule cấm dùng `Console.WriteLine` trong dự án | Lập trình viên có thể log thô dữ liệu nhạy cảm | M11-T049 |
| `M11-DR-I05` | Chưa kết nối sự kiện vi phạm che mờ với Audit Log M11 (`ACT-M11-33-SECRET`) | Không ghi vết được khi có dòng code thử log secret | M11-T049; M11-T031 |

- `M11-DR-F01`: Triển khai `SerilogRedactionEnricher` với Zero-Secret Logging Invariant (tiếp nhận: M11-T049).
- `M11-DR-F02`: Tích hợp Bắt buộc Salted SHA-256 IP Hash & PII Masking (tiếp nhận: M11-T049; REL-03).
- `M11-DR-F03`: Triển khai Linter Rule cấm Console.WriteLine & Audit Alert M11 (tiếp nhận: M11-T049; M11-T031).
- `M11-DR-F04`: Thiết lập bộ kiểm thử tự động DR-G01–G10 và DR33-01–20 (tiếp nhận: M11 tasks).
- `M11-DR-F05`: Thu thập bằng chứng runtime cho luồng che mờ log M11 (tiếp nhận: M11 tasks; A-G02/A-G05).

## 8. Tự kiểm M11-T033

- Đã thiết kế hoàn chỉnh `M11-DATA-REDACTION-LOG-POLICY-1.0` với Ma trận Quy tắc Che mờ Dữ liệu và Bí mật.
- Đã chốt Ràng buộc Nguyên tắc Tuyệt đối Không Ghi Vết Bí mật (`Zero-Secret Logging`).
- Đã chốt Quy tắc Che mờ Dữ liệu Cá nhân PII và Salted SHA-256 Hash cho Địa chỉ IP.
- Đã lồng ghép Bộ lọc Che mờ Serilog Tự động (`SerilogRedactionEnricher`) và Audit Log M11 (`ACT-M11-33-SECRET`).
- Đã xác lập 10 Regression Gates (`DR-G01`–`DR-G10`) và 20 Test Cases tự kiểm (`DR33-01`–`DR33-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả quy tắc che dữ liệu và bí mật M11-T033 | WSA-7K2 |
