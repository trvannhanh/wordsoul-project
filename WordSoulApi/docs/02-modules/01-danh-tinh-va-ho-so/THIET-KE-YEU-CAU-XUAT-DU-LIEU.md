# Thiết kế yêu cầu xuất dữ liệu M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-DATA-EXPORT-REQUEST-1.0` |
| Task | M01-T034 |
| Đầu vào | M01-CROSS-MODULE-PII-MAP-1.0 (D-102), M11-BACKGROUND-JOB-REGISTRY-1.0 (D-135), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Yêu cầu Xuất Dữ liệu Cá nhân (`Personal Data Export Protocol - GDPR/Data Portability`), luồng tạo gói dữ liệu JSON/ZIP nén mã hóa AES-256, thời hạn tải 7 ngày via S3 Private Signed URL, giới hạn 1 lần/24h kèm Re-Auth $\le 5\text{m}$ và lưu vết kiểm toán M11 |
| Tự kiểm | A-G01, A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Yêu cầu Xuất Dữ liệu Cá nhân (`Personal Data Export Protocol`) thuộc M01, thực thi quyền di chuyển dữ liệu cá nhân (GDPR Data Portability Right) cho người học WordSoul, cho phép xuất toàn bộ lịch sử hồ sơ, tiến trình học tập (SRS M04), lịch sử giao dịch (M06) và thành tích xã hội ra tệp nén an toàn được bảo vệ bằng mật khẩu mã hóa (REL-01, REL-07).

- **Quyền Di chuyển Dữ liệu Cá nhân GDPR (`GDPR Data Portability Invariant`)**: Người học có quyền tạo yêu cầu xuất toàn bộ dữ liệu cá nhân liên module dưới dạng tệp nén ZIP chứa các tệp JSON chuẩn hóa.
- **Worker Gom Dữ liệu Bất đồng bộ (`Async DataExportWorker Invariant`)**: Việc gom dữ liệu cá nhân liên module BẮT BUỘC thực hiện qua Worker ngầm `DataExportWorker` đăng ký tại M11-T038 để tránh gây nghẽn API Gateway. Gói ZIP xuất ra BẮT BUỘC mã hóa bằng `AES-256` với mật khẩu ngẫu nhiên và lưu tại S3 Private Bucket.
- **Thời hạn Tải Tệp Xuất 7 Ngày & Phân quyền Re-Auth $\le 5\text{m}$ (`Export Expiry & Re-Auth Guard`)**:
  - *Re-Auth Guard*: Yêu cầu xuất dữ liệu BẮT BUỘC xác thực lại mật khẩu local trong 5 phút gần nhất (`ReAuthMinutes <= 5m`).
  - *Rate Limit*: Tối đa 1 yêu cầu xuất tệp trong 24 giờ (`MaxExportRequestsPerDay = 1`).
  - *Tệp tải về*: Được truy cập qua S3 Private Signed URL với thời hạn vô hiệu 7 ngày (`ExportDownloadUrlTtl = 7d`). Sau 7 ngày, tệp ZIP tự động bị tiêu hủy (REL-07).
- **Lưu vết Sổ Kiểm toán Xuất Dữ liệu M11 (`Data Export Audit Trail`)**: $100\%$ các yêu cầu tạo, hoàn tất nén tệp hoặc tải gói dữ liệu cá nhân được ghi vết bất biến `ACT-M01-34-EXPORT` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Xuất Dữ liệu Cá nhân (Data Export Matrix)

| Trạng thái Yêu cầu (`ExportStatus`) | Điều kiện Chuyển | Định dạng Tệp Đơn | Thời hạn Tải Tệp (`TTL`) | Giới hạn Tần suất (`Rate Limit`) | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `REQUESTED` | Re-Auth $\le 5\text{m}$ OK | N/A (Đang hàng chờ) | N/A | **1 Yêu cầu / 24 Giờ** | `ACT-M01-34-REQUEST` |
| `PROCESSING` | Worker tiếp nhận | Memory Buffer JSON | N/A | N/A | `ACT-M01-34-PROCESS` |
| **`READY_FOR_DOWNLOAD`** | **Nén ZIP AES-256 OK** | **Encrypted ZIP File** | **7 Ngày (S3 Signed URL)** | N/A | `ACT-M01-34-READY` |
| `EXPIRED_DELETED` | Quá hạn 7 ngày | Tiêu hủy tệp S3 | N/A | N/A | `ACT-M01-34-EXPIRED` |

## 3. Kiến trúc Luồng Xuất Dữ liệu Cá nhân M01 (Data Export Pipeline)

```
[Learner Submits Export Request (Re-Auth <= 5m Guard)]
                           |
                           v
    [Create ExportRequest Record (Status: REQUESTED)]
                           |
                           v
      [DataExportWorker (M11-T038) Pickups Job]
                           |
      +--------------------+--------------------+
      | Aggregate M01 Identity & Profile Data    |
      | Aggregate M04 SRS Learning Cards Data   |
      | Aggregate M06 Transaction History Data  |
      +--------------------+--------------------+
                           |
                           v
    [Compress to Password-Protected AES-256 ZIP Archive]
                           |
                           v
    [Upload to S3 Private Bucket & Generate Signed URL (TTL 7d)]
    [Update Status: READY_FOR_DOWNLOAD]
    [Send Push / Email Notification via M10]
    [Record Audit Log ACT-M01-34-EXPORT]
```

## 4. Giao thức Thực thi Xuất Dữ liệu CSDL (PersonalDataExportService)

```csharp
public async Task<ExportRequestDto> RequestDataExportAsync(string userId)
{
    // 1. Re-Auth Guard <= 5m
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null || user.LastReAuthenticatedAtUtc == null || 
        (DateTime.UtcNow - user.LastReAuthenticatedAtUtc.Value).TotalMinutes > 5)
    {
        throw new UnauthorizedAccessException("REAUTH_REQUIRED: Vui lòng xác thực lại mật khẩu trước khi yêu cầu xuất dữ liệu cá nhân.");
    }

    // 2. Rate Limit Guard: 1 request / 24 hours
    var recentRequest = await _db.DataExportRequests
        .FirstOrDefaultAsync(r => r.UserId == userId && r.CreatedAtUtc > DateTime.UtcNow.AddHours(-24));
    
    if (recentRequest != null)
    {
        throw new InvalidOperationException("EXPORT_RATE_LIMIT_EXCEEDED: Bạn chỉ được gửi tối đa 1 yêu cầu xuất dữ liệu trong vòng 24 giờ.");
    }

    // 3. Create Export Request Record
    var exportReq = new DataExportRequest {
        ExportRequestId = Guid.NewGuid().ToString("N"),
        UserId = userId,
        Status = DataExportStatus.REQUESTED,
        CreatedAtUtc = DateTime.UtcNow,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
    };

    _db.DataExportRequests.Add(exportReq);
    await _db.SaveChangesAsync();

    // 4. Dispatch Background Export Worker Job (M11-T038)
    await _jobDispatcher.DispatchJobAsync("JOB_DATA_EXPORT", exportReq.ExportRequestId);

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M01-34-EXPORT", userId, new {
        ExportRequestId = exportReq.ExportRequestId,
        Status = "REQUESTED"
    });

    return exportReq.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DE-G01` | Tính năng xuất dữ liệu BẮT BUỘC bao phủ toàn bộ dữ liệu cá nhân thuộc M01, M04, M06 và thành tích. |
| `DE-G02` | Yêu cầu xuất dữ liệu BẮT BUỘC xác thực lại mật khẩu local trong 5 phút gần nhất (`ReAuthMinutes <= 5m`). |
| `DE-G03` | Giới hạn tần suất yêu cầu xuất dữ liệu BẮT BUỘC duy trì tối đa 1 lần / 24 giờ per user. |
| `DE-G04` | Tệp xuất nén ZIP BẮT BUỘC được mã hóa bằng chuẩn `AES-256` với mật khẩu ngẫu nhiên an toàn. |
| `DE-G05` | Tệp nén xuất dữ liệu trên S3 BẮT BUỘC tự động tiêu hủy sau 7 ngày (`ExportDownloadUrlTtl = 7d`, REL-07). |
| `DE-G06` | 100% các đợt yêu cầu xuất hoặc tải tệp dữ liệu cá nhân được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M01-34-EXPORT`). |
| `DE-G07` | SLA thực thi tổng hợp gói dữ liệu cho người học có 100,000 bản ghi $< 60$ giây. |
| `DE-G08` | Phân quyền yêu cầu xuất dữ liệu cá nhân chỉ dành riêng cho chính chủ tài khoản sở hữu dữ liệu đó. |
| `DE-G09` | Không được xuất dữ liệu nhạy cảm của người dùng khác hoặc thông tin bí mật hệ thống trong gói ZIP. |
| `DE-G10` | 100% các test case tự kiểm DE34-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DE34-01` | Người học xác thực lại 2 phút trước gửi yêu cầu xuất dữ liệu cá nhân | Tạo `ExportRequestId`, chuyển trạng thái `REQUESTED`, 200 OK |
| `DE34-02` | Người học thử gửi yêu cầu xuất dữ liệu lần thứ 2 trong cùng 24h | Reject HTTP 400 `EXPORT_RATE_LIMIT_EXCEEDED` |
| `DE34-03` | Người học thử gửi yêu cầu xuất dữ liệu khi lần Re-Auth cuối là 8 phút trước ($> 5\text{m}$) | Reject HTTP 401 `REAUTH_REQUIRED` |
| `DE34-04` | Worker `DataExportWorker` tổng hợp xong tệp ZIP nén mã hóa AES-256 | Cập nhật `READY_FOR_DOWNLOAD`, phát notification qua M10 |
| `DE34-05` | Tra cứu vết Audit Log M11 sau khi gửi yêu cầu xuất dữ liệu thành công | Ghi nhận Audit Event `ACT-M01-34-REQUEST` |
| `DE34-06` | Tải tệp ZIP sau 8 ngày kể từ lúc tạo ($> 7$d quá hạn TTL) | S3 Signed URL trả về 403 Expired, trạng thái `EXPIRED_DELETED` |
| `DE34-07` | Đơn giản hóa giải nén tệp ZIP với mật khẩu ngẫu nhiên được cấp | Giải nén thành công các tệp `profile.json`, `learning_cards.json` |
| `DE34-08` | Kiểm tra nội dung tệp JSON xuất ra không chứa mật khẩu băm hoặc Refresh Tokens | Không chứa PII nhạy cảm cấp hệ thống |
| `DE34-09` | Tải đồng thời 50 request kiểm tra trạng thái tệp xuất dữ liệu | Query latency p95 $< 10\text{ms}$ |
| `DE34-10` | Người học bấm nút "Tải Gói Dữ liệu" từ trang bảo mật | Trả về S3 Private Signed URL (TTL 15m) |
| `DE34-11` | Thử gửi yêu cầu xuất dữ liệu cho `UserId` của người học khác | Reject 403 `FORBIDDEN_USER_CROSS_ACCESS` |
| `DE34-12` | Gửi request yêu cầu xuất dữ liệu khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `DE34-13` | User bị khóa tài khoản `AccountStatus = BANNED` thử xuất dữ liệu | Reject 403 `ACCOUNT_BANNED` |
| `DE34-14` | User chưa đăng nhập gọi API yêu cầu xuất dữ liệu | Deny 401 Unauthorized |
| `DE34-15` | Quá trình gom dữ liệu bị gián đoạn do lỗi bộ nhớ | Retry worker ngầm, chuyển trạng thái `FAILED` nếu quá 3 lần |
| `DE34-16` | Kiểm tra độ trễ phát sự kiện Push Notification qua M10 khi tệp xuất sẵn sàng | Dispatch SLA $< 500\text{ms}$ |
| `DE34-17` | Phân tích tham chiếu các bản ghi `DataExportRequests` trong CSDL | Quét schema `M01_DataExportRequests` (T020) |
| `DE34-18` | Dịch vụ S3 bị ngắt kết nối khi lưu tệp ZIP nén | Fallback nạp đệm tệp nén vào đĩa local M11 |
| `DE34-19` | Tra cứu danh sách các yêu cầu xuất dữ liệu chưa quá hạn của người dùng | Trả về DTO danh sách ActiveExportRequests |
| `DE34-20` | Kiểm thử hoàn tất luồng yêu cầu xuất dữ liệu M01-DATA-EXPORT-REQUEST-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-DE-I01` | M01 hiện tại chưa có `PersonalDataExportService` xử lý GDPR | Không đáp ứng quyền di chuyển dữ liệu cá nhân của người học | M01-T049 (Source task) |
| `M01-DE-I02` | Thiếu cờ Mã hóa Tệp nén ZIP AES-256 & TTL 7 ngày trên S3 | Risk rò rỉ dữ liệu cá nhân nếu link tải bị lộ | M01-T049; REL-07 |
| `M01-DE-I03` | Thiếu cờ Re-Auth Guard $\le 5\text{m}$ & Rate Limit 1 lần/24h | Kẻ xấu mượn máy có thể tạo vô số gói xuất gây quá tải S3 | M01-T049; REL-01 |
| `M01-DE-I04` | Thiếu luồng phát thông báo PUSH/Email qua M10 khi tệp xuất sẵn sàng | Người học không biết khi nào tệp nén ZIP đã tạo xong | M01-DE-F04; M10-T001 |
| `M01-DE-I05` | Chưa kết nối sự kiện xuất dữ liệu với Audit Log M11 (`ACT-M01-34-EXPORT`) | Không ghi vết được lịch sử xuất và tải tệp dữ liệu cá nhân | M01-T049; M11-T031 |

- `M01-DE-F01`: Triển khai `PersonalDataExportService` với GDPR Data Portability (tiếp nhận: M01-T049).
- `M01-DE-F02`: Tích hợp Bắt buộc Encrypted AES-256 ZIP & 7-Day S3 TTL (tiếp nhận: M01-T049; REL-07).
- `M01-DE-F03`: Triển khai Re-Auth Guard $\le 5\text{m}$ & Rate Limit 1 Req/24h (tiếp nhận: M01-T049; REL-01).
- `M01-DE-F04`: Thiết lập bộ kiểm thử tự động DE-G01–G10 và DE34-01–20 (tiếp nhận: M01 tasks).
- `M01-DE-F05`: Thu thập bằng chứng runtime cho luồng xuất dữ liệu M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T034

- Đã thiết kế hoàn chỉnh `M01-DATA-EXPORT-REQUEST-1.0` với Ma trận Quy trình Xuất Dữ liệu Cá nhân.
- Đã chốt Ràng buộc Quyền Di chuyển Dữ liệu Cá nhân GDPR (`GDPR Data Portability`).
- Đã chốt Ràng buộc Worker Gom Dữ liệu Bất đồng bộ (`Async DataExportWorker` mã hóa AES-256).
- Đã lồng ghép Thời hạn Tải Tệp Xuất 7 Ngày (S3 Signed URL TTL 7d REL-07), Re-Auth Guard $\le 5\text{m}$, Rate Limit 1 lần/24h và Audit Log M11 (`ACT-M01-34-EXPORT`).
- Đã xác lập 10 Regression Gates (`DE-G01`–`DE-G10`) và 20 Test Cases tự kiểm (`DE34-01`–`DE34-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế yêu cầu xuất dữ liệu M01-T034 | WSA-7K2 |
