# Lập bản đồ dữ liệu rời hệ thống — Lát A M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-OUTBOUND-DATA-MAP-1.0` |
| Task | M12-T042-A |
| Đầu vào | M12-CONTRACT-1.0 (D-021), A0-T001 (REL-01), A0-T003 (REL-03) |
| Phạm vi | Bản đồ 6 luồng dữ liệu truyền ra bên ngoài hệ thống (`Data Egress Matrix`), quy tắc giảm thiểu PII (`Zero PII Egress to AI`) và bộ lọc làm sạch dữ liệu trước khi gửi |
| Tự kiểm | A-G05; REL-01, REL-03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Bản đồ Kiểm soát Dữ liệu Rời Hệ thống (`Outbound Data Egress Registry & Mapping Engine`) thuộc M12 cho Lát A, quản lý toàn bộ các luồng dữ liệu truyền ra bên ngoài ranh giới WordSoul sang các nhà cung cấp bên thứ ba (AI, TTS, STT, FCM, Email, OAuth).

- **Nguyên tắc Cấm Rò rỉ Dữ liệu Nhạy cảm sang AI (`Zero PII Egress to AI Invariant`)**: Tuyệt đối KHÔNG truyền họ tên, email, số điện thoại, IP hoặc `UserId` cá nhân của người học vào các prompt gửi cho nhà cung cấp AI (OpenAI / Gemini). Các prompt CHỈ ĐƯỢC PHÉP chứa ngữ cảnh từ vựng vô danh.
- **Tối thiểu hóa Dữ liệu Truyền ra (`Data Minimization Protocol`)**: Dữ liệu gửi ra ngoài chỉ chứa các trường thuộc tính tối thiểu cần thiết để dịch vụ bên ngoài thực thi chức năng (REL-01).
- **Mã hóa và Ẩn danh hóa Thông tin Căn cước (`Anonymization & Pseudonymization`)**: Địa chỉ email gửi qua SMTP/FCM được mã hóa trên đường truyền (TLS 1.3). Mã định danh thiết bị Push Notification được ánh xạ qua `InstallationId` ẩn danh.
- **Giám sát và Vinh danh Lưu lượng Chuyển ra (`Egress Monitoring & Audit`)**: 100% các luồng dữ liệu gửi ra dịch vụ bên ngoài phải qua Dịch vụ Chuyển đổi Adaptor M12 và ghi nhận chỉ số lưu lượng (`Egress Byte Counter`).

## 2. Bảng Bản đồ 6 Luồng Dữ liệu Rời Hệ thống (Outbound Data Egress Matrix)

| Mã Luồng | Nhà cung cấp Bên ngoài | Mục đích Nghiệp vụ | Payload Dữ liệu Truyền ra | Dữ liệu PII/Cá nhân? | Cơ chế Bảo vệ & Làm sạch |
|---|---|---|---|---|---|
| `EG-01-TTS` | Azure Speech TTS | Tổng hợp âm thanh từ vựng / câu ví dụ | Text từ vựng, SSML voice tag, IPA | KHÔNG | Kiểm tra không chứa PII người dùng |
| `EG-02-AI` | OpenAI / Gemini API | Gợi ý biên soạn bộ từ & ví dụ ngữ cảnh | Topic name, CEFR level, prompt template | KHÔNG | Bộ lọc `PromptAnonymizerFilter` gỡ toàn bộ PII |
| `EG-03-STT` | Azure / Google Speech-to-Text | Đánh giá phát âm người học M05 | Raw Audio PCM Wave bytes, reference text | KHÔNG (Anonymous Audio) | Gỡ toàn bộ ID người học khỏi stream audio |
| `EG-04-FCM` | Firebase Cloud Messaging | Gửi thông báo đẩy nhắc học / thách đấu | FCM Device Token, Notification Title/Body | Có (Device Token) | Token ánh xạ ẩn danh với `InstallationId` |
| `EG-05-MAIL` | SMTP Mail Provider | Gửi email kích hoạt / khôi phục tài khoản | Recipient Email, Template Vars (OTP, Name) | CÓ (Email Address) | Mã hóa TLS 1.3, dùng kết nối riêng an toàn |
| `EG-06-AUTH` | Google / Facebook OAuth | Xác thực danh tính người dùng M01 | Authorization Code, Client ID, Secret | KHÔNG | Truyền qua đường truyền HTTPS mã hóa |

## 3. Kiến trúc Bộ lọc Làm sạch Dữ liệu trước khi Gửi ra (Outbound Sanitization Pipeline)

```
[Business Module Request Outbound Data]
                   |
                   v
     [M12 Outbound Adapter Service]
                   |
                   v
   [Outbound Sanitization Middleware]
   - Check Zero PII for AI (EG-02)
   - Strip UserId from Audio Stream (EG-03)
   - Pseudonymize Device Tokens (EG-04)
                   |
         +---------+---------+
         | (Pass)            | (PII Detected!)
         v                   v
  [Send to External API]  [REJECT OUTBOUND REQUEST]
  - Record Egress Metrics - Log Security Alert REL-03
```

### 3.1. Cấu hình Bộ lọc PromptAnonymizerFilter cho AI Provider (EG-02)
```csharp
public class PromptAnonymizerFilter
{
    private static readonly Regex EmailRegex = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\b\d{10,11}\b", RegexOptions.Compiled);

    public string SanitizePrompt(string rawPrompt)
    {
        // 1. Gỡ bỏ địa chỉ Email nếu vô tình lọt vào prompt
        var sanitized = EmailRegex.Replace(rawPrompt, "[REDACTED_EMAIL]");
        
        // 2. Gỡ bỏ số điện thoại
        sanitized = PhoneRegex.Replace(sanitized, "[REDACTED_PHONE]");

        // 3. Đảm bảo không chứa UserId hoặc Token
        if (sanitized.Contains("UserId") || sanitized.Contains("Bearer "))
        {
            throw new InvalidOperationException("OUTBOUND_PII_VIOLATION: Prompt chứa thuộc tính nhạy cảm.");
        }

        return sanitized;
    }
}
```

## 4. Giao thức Giám sát và Kiểm soát Lưu lượng Dữ liệu Rời Hệ thống (Egress Monitoring)

1. **Ghi nhận Chỉ số Egress Byte Counter**: Mỗi request chuyển ra ngoài ghi nhận `EgressBytesSent` và `EgressBytesReceived` trong Prometheus Metric `wordsoul_outbound_egress_bytes_total`.
2. **Ngưỡng Cảnh báo Bất thường (Egress Anomaly Alert)**: Nếu lưu lượng truyền ra cho 1 nhà cung cấp tăng đột biến $> 300\%$ trong 15 phút, hệ thống phát cảnh báo `WARN_EGRESS_SPIKE`.
3. **Tuân thủ Quyền Riêng tư REL-01 & REL-03**: Người học có thể tra cứu danh sách các dữ liệu cá nhân bị truyền sang bên thứ ba (chỉ bao gồm Email cho SMTP và Device Token cho FCM).

## 5. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DM-G01` | 100% dữ liệu truyền ra ngoài ranh giới hệ thống thuộc 1 trong 6 luồng dữ liệu Egress tiêu chuẩn. |
| `DM-G02` | Tuyệt đối KHÔNG truyền PII (họ tên, email, phone, IP) vào các prompt gửi nhà cung cấp AI (EG-02). |
| `DM-G03` | Luồng âm thanh đánh giá phát âm (EG-03) chỉ chứa file audio thô, không đính kèm ID người dùng. |
| `DM-G04` | Dữ liệu truyền qua SMTP (EG-05) và OAuth (EG-06) bắt buộc mã hóa TLS 1.3 trên đường truyền. |
| `DM-G05` | Thiết bị FCM (EG-04) được ánh xạ ẩn danh qua `InstallationId`, cấm truyền thông tin cá nhân trong push payload. |
| `DM-G06` | Bộ lọc `PromptAnonymizerFilter` hoạt động tự động trên 100% các request phát sinh gửi sang AI. |
| `DM-G07` | Ghi nhận đầy đủ chỉ số lưu lượng `EgressBytes` cho 100% request rời hệ thống. |
| `DM-G08` | Cảnh báo tự động kích hoạt khi phát hiện lưu lượng Egress tăng bất thường $> 300\%$. |
| `DM-G09` | Phân quyền cấu hình luồng Egress tuân thủ ma trận vai trò M11 (`R08 Operations Admin`). |
| `DM-G10` | 100% các test case tự kiểm DM42-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DM42-01` | Gửi prompt sinh câu ví dụ từ vựng sang OpenAI | Prompt được làm sạch qua `PromptAnonymizerFilter`, không có PII |
| `DM42-02` | Thử nạp prompt chứa Email `user@example.com` sang AI | `PromptAnonymizerFilter` thay thế thành `[REDACTED_EMAIL]` |
| `DM42-03` | Thử nạp prompt chứa thuộc tính `UserId = 10024` sang AI | System throw `OUTBOUND_PII_VIOLATION`, chặn request |
| `DM42-04` | Gửi stream âm thanh người học thử phát âm sang Azure Speech | Stream audio được tách sạch ID người dùng, gửi an toàn |
| `DM42-05` | Gửi email khôi phục mật khẩu qua SMTP Provider | Kết nối sử dụng mã hóa TLS 1.3 an toàn |
| `DM42-06` | Gửi thông báo đẩy nhắc học qua FCM | Payload chỉ chứa `InstallationId` ẩn danh và tiêu đề bài học |
| `DM42-07` | Kiểm tra metric `wordsoul_outbound_egress_bytes_total` sau request | Metric ghi nhận chính xác số byte vừa truyền ra |
| `DM42-08` | Mô phỏng lưu lượng Egress tăng đột biến $400\%$ trong 10 phút | Hệ thống tự động kích hoạt cảnh báo `WARN_EGRESS_SPIKE` |
| `DM42-09` | Gửi text từ vựng sang Azure Speech TTS để tổng hợp audio | Payload chỉ chứa text bài học, không có dữ liệu cá nhân |
| `DM42-10` | Người học yêu cầu xuất dữ liệu cá nhân rời hệ thống (REL-01) | Hệ thống xuất đúng 2 mục: Email (SMTP) và Device Token (FCM) |
| `DM42-11` | Thử gửi request Egress qua đường truyền HTTP không mã hóa | Adaptor ném ngoại lệ reject request không an toàn |
| `DM42-12` | Gửi authorization code sang Google OAuth để đổi token | Request thực thi qua giao thức HTTPS chuẩn mã hóa |
| `DM42-13` | Tải đồng thời 100 request Egress qua Adaptor M12 | Response latency overhead của Middleware $< 5\text{ms}$ |
| `DM42-14` | Xem vết Audit Log M11 sau khi phát hiện vi phạm Egress PII | Ghi nhận Audit Event `ACT-M11-13` báo cáo sự cố an ninh |
| `DM42-15` | User không phải Admin thực hiện thay đổi cấu hình luồng Egress | Deny 403 Forbidden |
| `DM42-16` | Gửi notification push chứa thông báo vinh danh người dùng | Tên hiển thị người học trong push payload được làm sạch |
| `DM42-17` | Ngắt kết nối dịch vụ bên ngoài (OpenAI bị outage) | Adaptor M12 kích hoạt cơ chế suy giảm (Circuit Breaker) |
| `DM42-18` | Kiểm tra log hệ thống sau khi gửi request Egress | Log không chứa chuỗi payload PII thô đã gửi ra ngoài |
| `DM42-19` | Phân tích tham chiếu trước khi chặn 1 luồng Egress | Quét các module nghiệp vụ M01-M10 đang phụ thuộc (T020) |
| `DM42-20` | Kiểm thử hoàn tất luồng bản đồ dữ liệu rời hệ thống M12-OUTBOUND-DATA-MAP-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-DM-I01` | Trong `WordSoulApi`, các API service gửi request trực tiếp ra ngoài chưa qua `PromptAnonymizerFilter` | Rủi ro để lọt dữ liệu nhạy cảm hoặc PII sang AI provider | M12-T049 (Source task) |
| `M12-DM-I02` | Stream audio gửi đi đánh giá phát âm hiện tại còn đính kèm metadata `UserId` | Rủi ro vi phạm quyền riêng tư dữ liệu sinh trắc học | M12-T049; REL-03 |
| `M12-DM-I03` | Thiếu metric Prometheus `wordsoul_outbound_egress_bytes_total` | Đội vận hành chưa theo dõi được chi phí dữ liệu truyền ra | M12-T049 |
| `M12-DM-I04` | Thiếu cơ chế phát cảnh báo tự động khi lưu lượng Egress tăng đột biến | Rủi ro bị lạm dụng API gây tốn chi phí nhà cung cấp | M12-T049 |
| `M12-DM-I05` | Chưa có bộ tài liệu công khai minh bạch các dữ liệu truyền ra bên ngoài cho người dùng | Chưa đáp ứng 100% tiêu chí REL-01 về minh bạch dữ liệu | M12-T049; REL-01 |

- `M12-DM-F01`: Triển khai `PromptAnonymizerFilter` cho 100% AI outbound requests (tiếp nhận: M12-T049).
- `M12-DM-F02`: Xây dựng `OutboundSanitizationMiddleware` làm sạch audio stream và FCM payload (tiếp nhận: M12-T049; REL-03).
- `M12-DM-F03`: Tích hợp Prometheus Egress Byte Counter & Alerting (tiếp nhận: M12-T049).
- `M12-DM-F04`: Thiết lập bộ kiểm thử tự động DM-G01–G10 và DM42-01–20 (tiếp nhận: M12 tasks).
- `M12-DM-F05`: Thu thập bằng chứng runtime cho luồng dữ liệu rời hệ thống M12 (tiếp nhận: M12 tasks; A-G05/REL-01).

## 8. Tự kiểm M12-T042-A

- Đã thiết kế hoàn chỉnh `M12-OUTBOUND-DATA-MAP-1.0` với Bảng Bản đồ 6 Luồng Dữ liệu Rời Hệ thống.
- Đã chốt quy tắc Cấm Rò rỉ Dữ liệu Nhạy cảm sang AI (`Zero PII Egress to AI`) với bộ lọc `PromptAnonymizerFilter`.
- Đã quy định cơ chế làm sạch audio stream và mã hóa TLS 1.3 cho SMTP/OAuth.
- Đã lồng ghép Prometheus Egress Byte Counter và cảnh báo tăng đột biến $> 300\%$.
- Đã xác lập 10 Regression Gates (`DM-G01`–`DM-G10`) và 20 Test Cases tự kiểm (`DM42-01`–`DM42-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả bản đồ dữ liệu rời hệ thống M12-T042-A | WSA-7K2 |
