# Xây dựng ma trận xóa và ẩn danh hóa M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ANONYMIZATION-DELETION-MATRIX-1.0` |
| Task | M01-T036 |
| Đầu vào | M01-ACCOUNT-DELETION-REQUEST-1.0 (D-148), M01-CROSS-MODULE-PII-MAP-1.0 (D-102), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Ma trận Xóa Dữ liệu và Ẩn danh hóa (`Data Deletion & Anonymization Matrix Protocol`), quy tắc phân loại xử lý thực thể (Hard Delete, Anonymize, Preserve), thuật toán băm SHA-256 mã hóa vết kiểm toán và lưu vết M11 |
| Tự kiểm | A-G01, A-G05; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Ma trận Xóa Dữ liệu và Ẩn danh hóa (`Data Deletion & Anonymization Matrix Protocol`) thuộc M01, chuẩn hóa cách thức xử lý chi tiết đối với từng loại dữ liệu cá nhân (PII) trên toàn bộ hệ thống WordSoul khi người học hoàn tất quy trình xóa tài khoản (hết 30 ngày Grace Period), đảm bảo tuân thủ triệt để quyền được quên (GDPR) nhưng vẫn duy trì tính toàn vẹn kiểm toán tài chính và báo cáo thống kê (REL-01, REL-07).

- **Quy tắc Phân loại 3 Nhóm Xử lý Thực thể (`3 Entity Handling Classes Invariant`)**:
  - `HARD_DELETE`: Tiêu hủy vĩnh viễn $100\%$ dữ liệu khỏi CSDL SQL và S3. Áp dụng cho: Mật khẩu băm, Tokens xác thực, External OAuth claims (`sub`, `provider`), Avatar Upload Files (M12-T025), Push Notification Tokens (M01-T027-A).
  - `ANONYMIZE`: Xóa thông tin định danh trực tiếp, thay thế bằng mã băm hoặc biệt danh ẩn danh. Áp dụng cho: Sổ kiểm toán Audit Logs (`UserId` $\to$ Salted SHA-256 `UserIdHash`), Bình luận công cộng (`DisplayName` $\to$ `"Learner #ANON"`), Lịch sử giao dịch M06 (Xóa địa chỉ thanh toán PII, giữ lại số tiền và mã hóa đơn tài chính theo REL-07).
  - `PRESERVE_AGGREGATE`: Giữ lại các chỉ số tổng hợp phi định danh. Áp dụng cho: Thống kê tổng số từ vựng đã học M04, Tổng điểm kinh nghiệm Exp/QualityScore (Không gắn với PII).
- **Thuật toán Băm Ẩn danh Bất biến (`Salted SHA-256 Anonymization Hash`)**: Toàn bộ liên kết `UserId` trong các bảng kiểm toán lịch sử BẮT BUỘC được chuyển đổi bằng thuật toán `Salted SHA-256` với khóa Salt cố định của hệ thống (`UserIdHash = SHA256(UserId + SystemSalt)`). CẤM giữ lại con trỏ tham chiếu GUID trực tiếp.
- **Ràng buộc Xóa Tài sản Media S3 M12-T025 (`S3 Asset RefCount Reduction`)**: Khi ẩn danh hóa tài khoản, tất cả các tệp media cá nhân (Ảnh đại diện avatar M01-T024) BẮT BUỘC giảm `ActiveRefCount -= 1` ở Module M12 để kích hoạt luồng dọn dẹp tệp mồ côi `OrphanAssetCleanupWorker`.
- **Lưu vết Sổ Kiểm toán Ẩn danh M11 (`Anonymization Audit Trail`)**: $100\%$ các đợt chạy ẩn danh hóa dữ liệu cá nhân được ghi vết bất biến `ACT-M01-36-ANONYMIZE` trong Sổ Kiểm toán M11.

## 2. Ma trận Xóa và Ẩn danh hóa Dữ liệu Hệ thống (Data Anonymization Matrix)

| Loại Thực thể Dữ liệu (`DataEntity`) | Module Sở hữu | Phân loại Xử lý (`HandlingClass`) | Hành vi Xử lý Chi tiết | Thời hạn Lưu trữ Pháp lý | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| **Identity & Passwords** | M01 | `HARD_DELETE` | Xóa sạch bản ghi `M01_Users` & Credentials | **Xóa ngay (0d)** | `ACT-M01-36-HARDDEL` |
| **External OAuth Tokens** | M12 | `HARD_DELETE` | Xóa Refresh Tokens & Nonce Redis | Xóa ngay (0d) | `ACT-M01-36-OAUTHDEL` |
| **User Avatar Media** | M01 / M12 | `HARD_DELETE` | Giảm RefCount M12, Xóa S3 File | Xóa sau 30d (M12-T025) | `ACT-M01-36-AVATARDEL` |
| **Audit Log Traces** | M11 | `ANONYMIZE` | Sửa `UserId` $\to$ Salted SHA-256 Hash | **12 Tháng (REL-07)** | `ACT-M01-36-AUDITHASH` |
| **Billing / Transactions** | M06 | `ANONYMIZE` | Redact PII Address, Keep Amount | 10 Năm (Luật Thuế) | `ACT-M01-36-BILLANON` |
| **Public Comments** | M08 | `ANONYMIZE` | Sửa `DisplayName` $\to$ `"Learner #ANON"` | Bất biến | `ACT-M01-36-COMMANON` |
| **SRS Learning Stats** | M04 | `PRESERVE_AGGREGATE` | Giữ tổng số lượt ôn tập phi định danh | Bất biến | `ACT-M01-36-STAT` |

## 3. Kiến trúc Luồng Xử lý Ẩn danh hóa Dữ liệu M01 (Anonymization Pipeline)

```
[AccountDeletionWorker (M11-T038) Triggers Anonymization Job (Day 31)]
                                   |
                                   v
    [Query User PII Mapping Metadata from M01-CROSS-MODULE-PII-MAP-1.0]
                                   |
        +--------------------------+--------------------------+
        |                          |                          |
        v                          v                          v
[HARD_DELETE Class]       [ANONYMIZE Class]          [PRESERVE_AGGREGATE Class]
- Delete Identity Record  - Salted SHA-256 UserIdHash   - Detach UserId Reference
- Delete OAuth Tokens     - Redact PII Address M06   - Keep Overall Cards Stats
- Decrement Avatar Ref    - Mask Public DisplayName  - Evict PII Metadata
        |                          |                          |
        +--------------------------+--------------------------+
                                   |
                                   v
                [Verify Zero PII Residue Guard]
                [Record Audit Log ACT-M01-36-ANONYMIZE]
```

## 4. Giao thức Thực thi Ẩn danh hóa CSDL (DataAnonymizationService)

```csharp
public async Task AnonymizeUserDataAsync(string userId)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null || user.AccountStatus != AccountStatus.PENDING_DELETION) return;

    string userIdHash = ComputeSaltedSha256(userId, _config["SystemAuditSalt"]);

    // 1. HARD_DELETE Identity Credentials & Avatar M12
    if (!string.IsNullOrEmpty(user.AvatarAssetId))
    {
        await _assetService.DecrementRefCountAsync(user.AvatarAssetId); // M12-T025
    }

    var authTokens = _db.ExternalAuthTokens.Where(t => t.UserId == userId);
    _db.ExternalAuthTokens.RemoveRange(authTokens);

    // 2. ANONYMIZE Audit Logs M11 & Transactions M06
    await _db.Database.ExecuteSqlRawAsync(
        "UPDATE M11_AuditLogs SET UserId = {0} WHERE UserId = {1}", userIdHash, userId);

    await _db.Database.ExecuteSqlRawAsync(
        "UPDATE M06_Transactions SET BillingAddress = '***REDACTED***', PiiName = '***REDACTED***' WHERE UserId = {0}", userId);

    // 3. ANONYMIZE Public DisplayName
    await _db.Database.ExecuteSqlRawAsync(
        "UPDATE M08_Comments SET DisplayName = 'Learner #ANON', AuthorUserId = NULL WHERE AuthorUserId = {0}", userId);

    // 4. HARD_DELETE User Record
    _db.Users.Remove(user);
    await _db.SaveChangesAsync();

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M01-36-ANONYMIZE", "SYSTEM_WORKER", new {
        UserIdHash = userIdHash,
        Action = "USER_DATA_ANONYMIZED_AND_HARD_DELETED"
    });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AM-G01` | Ma trận ẩn danh hóa BẮT BUỘC phân loại rõ 3 nhóm xử lý (`HARD_DELETE`, `ANONYMIZE`, `PRESERVE_AGGREGATE`). |
| `AM-G02` | Danh tính cá nhân, mật khẩu băm và Refresh Tokens BẮT BUỘC bị tiêu hủy vĩnh viễn (`HARD_DELETE`). |
| `AM-G03` | Liên kết `UserId` trong Sổ Kiểm toán M11 BẮT BUỘC được chuyển đổi bằng thuật toán `Salted SHA-256`. |
| `AM-G04` | Lịch sử giao dịch tài chính M06 BẮT BUỘC được làm sạch PII (Địa chỉ/Họ tên) nhưng giữ lại số tiền để phục vụ thuế (REL-07). |
| `AM-G05` | Ảnh đại diện avatar BẮT BUỘC giảm `ActiveRefCount -= 1` ở Module M12 để kích hoạt dọn dẹp S3 (M12-T025). |
| `AM-G06` | 100% các đợt thực thi ẩn danh hóa dữ liệu được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M01-36-ANONYMIZE`). |
| `AM-G07` | SLA thực thi hoàn tất luồng ẩn danh hóa tài khoản có 50,000 bản ghi $< 15$ giây. |
| `AM-G08` | Phân quyền khởi chạy ẩn danh hóa dữ liệu chỉ dành riêng cho tiến trình ngầm `AccountDeletionWorker`. |
| `AM-G09` | Kiểm tra Zero PII Residue Guard đảm bảo không còn dữ liệu PII trực tiếp sót lại trong CSDL chính. |
| `AM-G10` | 100% các test case tự kiểm AM36-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AM36-01` | Chạy ẩn danh hóa cho tài khoản `PENDING_DELETION` quá 30 ngày | Xóa record `M01_Users`, chuyển Audit Log sang Salted SHA-256 |
| `AM36-02` | Tra cứu bản ghi giao dịch M06 sau khi ẩn danh hóa | `BillingAddress` bị sửa thành `"***REDACTED***"`, giữ nguyên số tiền |
| `AM36-03` | Tra cứu bình luận công cộng M08 sau khi ẩn danh hóa | `DisplayName` đổi thành `"Learner #ANON"`, `AuthorUserId` null |
| `AM36-04` | Kiểm tra RefCount ảnh đại diện Avatar trên M12 sau khi ẩn danh | `ActiveRefCount` giảm 1, đủ điều kiện tiêu hủy S3 |
| `AM36-05` | Kiểm tra danh sách Refresh Tokens trên Redis sau khi ẩn danh | Xóa sạch $100\%$ tokens xác thực của người dùng |
| `AM36-06` | Tra cứu vết Audit Log M11 sau khi hoàn tất ẩn danh hóa | Ghi nhận Audit Event `ACT-M01-36-ANONYMIZE` đính kèm UserIdHash |
| `AM36-07` | Thử giải mã ngược chuỗi `UserIdHash` về `UserId` ban đầu | Không thể giải mã ngược (Thuật toán SHA-256 1 chiều) |
| `AM36-08` | Chạy bộ lọc Zero PII Residue Guard quét CSDL SQL sau ẩn danh | Trả về 0 bản ghi PII trùng khớp với Email/Phone người dùng |
| `AM36-09` | Tải đồng thời 20 worker threads ẩn danh hóa 20 tài khoản cùng lúc | Execution SLA $< 12\text{s}$ |
| `AM36-10` | Thử chạy ẩn danh hóa cho tài khoản đang ở trạng thái `ACTIVE` | Reject `INVALID_ACCOUNT_STATUS_FOR_ANONYMIZATION` |
| `AM36-11` | Thử truyền chuỗi `SystemAuditSalt` rỗng trong hàm tính hash | Reject 400 `SYSTEM_SALT_REQUIRED` |
| `AM36-12` | Gửi request yêu cầu ẩn danh hóa trực tiếp từ API bên ngoài | Deny 403 Forbidden (Chỉ cho Worker ngầm M11-T038) |
| `AM36-13` | User không phải System Worker thử gọi API ẩn danh hóa | Deny 403 Forbidden |
| `AM36-14` | User chưa đăng nhập gọi API ẩn danh hóa M01 | Deny 401 Unauthorized |
| `AM36-15` | Kiểm tra dữ liệu tổng hợp bài học M04 sau khi ẩn danh hóa | Giữ nguyên tổng số bài đã hoàn thành phi định danh |
| `AM36-16` | Kiểm tra độ trễ ngắt mạch ẩn danh hóa khi CSDL bị khóa bảng | Abort transaction, rollback an toàn, phát alert |
| `AM36-17` | Phân tích tham chiếu các bản ghi `AnonymizationAuditLogs` trong CSDL | Quét schema `M11_AnonymizationAudits` (T020) |
| `AM36-18` | Tiến trình ẩn danh hóa bị sập giữa chừng khi đang sửa M06 | Rollback SQL Transaction 100%, giữ nguyên PENDING_DELETION |
| `AM36-19` | Tra cứu danh sách các tài khoản đã được ẩn danh hóa thành công | Trả về DTO danh sách AnonymizedUserHashes |
| `AM36-20` | Kiểm thử hoàn tất luồng ma trận xóa và ẩn danh hóa M01-ANONYMIZATION-DELETION-MATRIX-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-AM-I01` | M01 hiện tại chưa có `DataAnonymizationService` xử lý 3 nhóm thực thể | Risk không xóa sạch dữ liệu PII khi người học yêu cầu | M01-T049 (Source task) |
| `M01-AM-I02` | Thiếu luồng Băm Salted SHA-256 cho `UserId` trong Sổ Audit Logs M11 | Vẫn còn giữ con trỏ GUID PII lộ vết kiểm toán | M01-T049; REL-07 |
| `M01-AM-I03` | Thiếu cờ Giữ lại Lịch sử Giao dịch M06 phi định danh phục vụ luật thuế | Xóa nhầm dữ liệu hóa đơn thuế vi phạm pháp luật | M01-T049; M06-T001 |
| `M01-AM-I04` | Thiếu luồng giảm RefCount Avatar M12-T025 khi ẩn danh hóa | Rác media S3 đọng lại vĩnh viễn tốn chi phí lưu trữ | M01-AM-F04; M12-T025 |
| `M01-AM-I05` | Chưa kết nối sự kiện ẩn danh hóa với Audit Log M11 (`ACT-M01-36-ANONYMIZE`) | Không ghi vết được bằng chứng đã thực hiện xóa GDPR | M01-T049; M11-T031 |

- `M01-AM-F01`: Triển khai `DataAnonymizationService` với 3 Entity Handling Classes (tiếp nhận: M01-T049).
- `M01-AM-F02`: Tích hợp Bắt buộc Salted SHA-256 UserIdHash & Tax Retention M06 (tiếp nhận: M01-T049; REL-07).
- `M01-AM-F03`: Triển khai S3 Avatar RefCount Reduction & Zero PII Residue Guard (tiếp nhận: M01-T049; M12-T025).
- `M01-AM-F04`: Thiết lập bộ kiểm thử tự động AM-G01–G10 và AM36-01–20 (tiếp nhận: M01 tasks).
- `M01-AM-F05`: Thu thập bằng chứng runtime cho luồng ẩn danh hóa M01 (tiếp nhận: M01 tasks; A-G01/A-G05).

## 8. Tự kiểm M01-T036

- Đã thiết kế hoàn chỉnh `M01-ANONYMIZATION-DELETION-MATRIX-1.0` với Ma trận Xóa và Ẩn danh hóa Dữ liệu Hệ thống.
- Đã chốt Ràng buộc Quy tắc Phân loại 3 Nhóm Xử lý Thực thể (`HARD_DELETE`, `ANONYMIZE`, `PRESERVE_AGGREGATE`).
- Đã chốt Ràng buộc Thuật toán Băm Ẩn danh Bất biến (`Salted SHA-256 Anonymization Hash`).
- Đã lồng ghép Ràng buộc Xóa Tài sản Media S3 M12-T025 (`S3 Asset RefCount Reduction`), Làm sạch PII Giao dịch M06 (REL-07) và Audit Log M11 (`ACT-M01-36-ANONYMIZE`).
- Đã xác lập 10 Regression Gates (`AM-G01`–`AM-G10`) và 20 Test Cases tự kiểm (`AM36-01`–`AM36-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả ma trận xóa và ẩn danh hóa M01-T036 | WSA-7K2 |
