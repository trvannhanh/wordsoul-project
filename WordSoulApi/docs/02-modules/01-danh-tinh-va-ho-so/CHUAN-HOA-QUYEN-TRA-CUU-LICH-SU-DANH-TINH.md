# Chuẩn hóa quyền tra cứu lịch sử danh tính M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-IDENTITY-HISTORY-AUTHORIZATION-1.0` |
| Task | M01-T041 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M11-LOG-TAXONOMY-1.0 (D-055), M11-SUPPORT-TIMELINE-1.0 (D-074), REL-02, REL-07 |
| Phạm vi | Mô hình Phân quyền Tra cứu Lịch sử Danh tính (`Identity History Access Control Engine`), 4 cấp độ quyền hạn, điều kiện ràng buộc Ticket Hỗ trợ active cho SupportAgent và Rate Limiter chống cào vét dữ liệu |
| Tự kiểm | A-G02; REL-02, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Phân quyền Tra cứu Lịch sử Danh tính (`Identity History Authorization Engine`) thuộc M01, xác lập ranh giới phân quyền nghiêm ngặt đối với việc xem nhật ký đăng nhập, lịch sử thay đổi thông tin bảo mật và vết sự kiện danh tính của người dùng, tuân thủ ma trận phân quyền tối thiểu REL-02 và bảo vệ quyền riêng tư người học REL-07.

- **4 Cấp độ Quyền hạn Tra cứu Lịch sử (`4 Access Control Levels Invariant`)**:
  - *Cấp 1 - SELF_USER (Chính chủ)*: Người học CHỈ ĐƯỢC XEM nhật ký đăng nhập và danh sách thiết bị active của CHÍNH MÌNH (`TargetUserId == ActorUserId`). CẤM tra cứu người khác.
  - *Cấp 2 - SUPPORT_AGENT (Chuyên viên Hỗ trợ)*: CHỈ ĐƯỢC XEM Dòng thời gian Hỗ trợ (`Support Timeline` M11-T028) của người dùng KHI VÀ CHỈ KHI tồn tại Ticket hỗ trợ active gán cho Agent đó (`HasActiveTicket == true` & `TicketId` hợp lệ). CẤM tra cứu tự do không có ticket.
  - *Cấp 3 - SECURITY_ADMIN (Quản trị An ninh)*: Đưa tra cứu toàn bộ lịch sử danh tính (Login, Security Changes, Epochs) kèm thông tin IP đã băm Salted SHA-256 (D-102).
  - *Cấp 4 - SUPER_ADMIN (Quản trị Tối cao)*: Toàn quyền tra cứu, nhưng yêu cầu Phê duyệt Kép 4 mắt (M01-T030) nếu muốn giải băm hoặc hiển thị PII thô cho vụ việc pháp lý.
- **Ràng buộc Ticket Hỗ trợ Active (`Active Ticket Requirement for SupportAgent`)**: SupportAgent gọi API tra cứu lịch sử danh tính BẮT BUỘC truyền header `X-Support-Ticket-Id`. API sẽ đối soát với CSDL M11; nếu Ticket không tồn tại hoặc đã đóng (`CLOSED`) $\to$ Trả về HTTP 403 Forbidden (`ACTIVE_TICKET_REQUIRED`).
- **Rate Limiting Chống Cào vét Dữ liệu (`Anti-Scraping Rate Limiter`)**: Mỗi tài khoản `SupportAgent` bị giới hạn tối đa 20 lượt tra cứu lịch sử danh tính / phút. Nếu vượt quá $\to$ Reject ngay với HTTP 429 Too Many Requests (`HISTORY_QUERY_RATE_EXCEEDED`).
- **Ghi vết Sổ Kiểm toán Tra cứu M11 (`History Access Audit Trail`)**: $100\%$ các lượt tra cứu lịch sử danh tính của Quản trị viên / SupportAgent bắt buộc được lưu vết bất biến `ACT-M11-41` trong Sổ Kiểm toán M11, bao gồm `ActorUserId`, `TargetUserId`, `TicketId`, `QueriedDateRange` và `ClientIPHash`.

## 2. Ma trận Quyền Tra cứu Lịch sử Danh tính (Authorization Matrix)

| Vai trò Yêu cầu (`Actor Role`) | Phạm vi Được phép Tra cứu | Điều kiện Ràng buộc (`Pre-conditions`) | Định dạng Dữ liệu Trả về | Rate Limit Tối đa |
|---|---|---|---|---|
| **Learner** (User) | Chỉ duy nhất tài khoản cá nhân | `TargetUserId == ActorUserId` | Public DTO (Email che mờ, Masked IP) | 60 requests / phút |
| **ContentCreator** | Chỉ duy nhất tài khoản cá nhân | `TargetUserId == ActorUserId` | Public DTO (Email che mờ, Masked IP) | 60 requests / phút |
| **SupportAgent** | Người học có Ticket hỗ trợ active | Có `X-Support-Ticket-Id` active M11 | Support Timeline DTO (Masked PII) | 20 requests / phút |
| **ContentAdmin** | KHÔNG CÓ QUYỀN TRA CỨU | CẤM QUYỀN (REL-02) | Reject 403 Forbidden | 0 requests |
| **SecurityAdmin** | Tất cả tài khoản trong hệ thống | Lý do an ninh `reason >= 15` char | Security Audit Log DTO (Salted IP) | 100 requests / phút |
| **SuperAdmin** | Tất cả tài khoản trong hệ thống | Phê duyệt Kép M01-T030 (nếu unmask) | Unmasked PII (chỉ khi có 2 chữ ký) | 100 requests / phút |

## 3. Kiến trúc Luồng Phân quyền và Tra cứu Lịch sử (Identity History Engine)

```
[Actor Requests GET /api/v1/users/{targetUserId}/identity-history]
                                |
                                v
               [Validate Role & Target User Match]
                                |
         +----------------------+----------------------+
         | (Actor == Target)    | (Actor != Target)    | (ContentAdmin)
         v                      v                      v
   [Allow SELF Access]  [Check Actor Role Matrix]  [Deny 403 Forbidden]
                                |
                     +----------+----------+
                     | (SupportAgent)      | (SecurityAdmin)
                     v                     v
            [Validate Active Ticket]  [Validate Reason >= 15]
            - Header: TicketId         - Query Security Audit Log
                     |                     |
                     +----------+----------+
                                |
                                v
                [Check Rate Limit (Max 20/min)]
                                |
                                v
            [Record Audit Event ACT-M11-41 in DB]
                                |
                                v
             [Return Filtered Identity History DTO]
```

## 4. Giao thức Thực thi Tra cứu Lịch sử CSDL (IdentityHistoryAuthorizationService)

```csharp
public async Task<IdentityHistoryResponseDto> GetIdentityHistoryAsync(
    string targetUserId, 
    string actorUserId, 
    string actorRole, 
    string ticketId, 
    string reason)
{
    // 1. Check Self Access
    if (targetUserId == actorUserId)
    {
        return await FetchSelfIdentityHistoryAsync(targetUserId);
    }

    // 2. Validate Authority Matrix
    if (actorRole == "ContentAdmin" || actorRole == "Learner" || actorRole == "ContentCreator")
    {
        throw new UnauthorizedAccessException("IDENTITY_HISTORY_ACCESS_DENIED: Bạn không có quyền xem lịch sử danh tính của người khác.");
    }

    // 3. SupportAgent Ticket Pre-condition Check
    if (actorRole == "SupportAgent")
    {
        if (string.IsNullOrEmpty(ticketId))
        {
            throw new ArgumentException("ACTIVE_TICKET_REQUIRED: Chuyên viên hỗ trợ bắt buộc truyền mã Ticket hỗ trợ.");
        }

        bool isTicketActive = await _m11SupportService.ValidateActiveTicketAsync(ticketId, targetUserId, actorUserId);
        if (!isTicketActive)
        {
            throw new UnauthorizedAccessException("INVALID_OR_CLOSED_SUPPORT_TICKET: Ticket không hợp lệ hoặc đã đóng.");
        }
    }

    // 4. Rate Limit Check (Max 20 queries/min for SupportAgent)
    string rateKey = $"wordsoul:history_query_rate:{actorUserId}:{DateTime.UtcNow:yyyyMMddHHmm}";
    long count = await _redisDb.StringIncrementAsync(rateKey);
    if (count == 1) await _redisDb.KeyExpireAsync(rateKey, TimeSpan.FromMinutes(1));
    if (count > 20 && actorRole == "SupportAgent")
    {
        throw new InvalidOperationException("HISTORY_QUERY_RATE_EXCEEDED: Đã vượt quá giới hạn 20 lượt tra cứu / phút.");
    }

    // 5. Ghi vết Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-41", actorUserId, new {
        TargetUserId = targetUserId,
        ActorRole = actorRole,
        TicketId = ticketId,
        Reason = reason
    });

    return await FetchAuditHistoryLogsAsync(targetUserId, actorRole);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IH-G01` | Người học (Learner) chỉ được phép tra cứu nhật ký đăng nhập và thiết bị của CHÍNH MÌNH. |
| `IH-G02` | SupportAgent tra cứu lịch sử người khác BẮT BUỘC có mã Ticket hỗ trợ active (`HasActiveTicket == true`). |
| `IH-G03` | Vai trò `ContentAdmin` tuyệt đối CẤM truy cập API tra cứu lịch sử danh tính của người dùng (REL-02). |
| `IH-G04` | Mỗi SupportAgent bị giới hạn tối đa 20 lượt tra cứu lịch sử danh tính / phút (Rate Limiter). |
| `IH-G05` | 100% các lượt tra cứu lịch sử danh tính từ Admin/SupportAgent ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-41`). |
| `IH-G06` | Địa chỉ IP trong dữ liệu lịch sử trả về cho SupportAgent BẮT BUỘC che mờ hoặc băm Salted SHA-256. |
| `IH-G07` | Yêu cầu xem PII thô chưa che mờ của SuperAdmin bắt buộc trải qua Phê duyệt Kép M01-T030. |
| `IH-G08` | Phân quyền truy cập API tra cứu lịch sử danh tính tuân thủ ma trận vai trò M01-T028. |
| `IH-G09` | SLA xử lý API tra cứu lịch sử danh tính $< 35\text{ms}$. |
| `IH-G10` | 100% các test case tự kiểm IH41-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IH41-01` | User A gọi API tra cứu lịch sử danh tính của chính mình (User A) | Trả về 200 OK với nhật ký đăng nhập cá nhân |
| `IH41-02` | User A gửi request tra cứu lịch sử danh tính của User B | Reject 403 `IDENTITY_HISTORY_ACCESS_DENIED` |
| `IH41-03` | SupportAgent gửi request tra cứu User B kèm TicketId active hợp lệ | Trả về Support Timeline DTO thành công |
| `IH41-04` | SupportAgent gửi request tra cứu User B nhưng truyền TicketId đã CLOSED | Reject 403 `INVALID_OR_CLOSED_SUPPORT_TICKET` |
| `IH41-05` | SupportAgent gửi request tra cứu User B nhưng KHÔNG truyền header TicketId | Reject 400 `ACTIVE_TICKET_REQUIRED` |
| `IH41-06` | ContentAdmin thử gọi API tra cứu lịch sử danh tính của User B | Deny 403 Forbidden (REL-02) |
| `IH41-07` | SupportAgent gửi 21 request tra cứu trong vòng 1 phút | Request thứ 21 bị chối với 429 `HISTORY_QUERY_RATE_EXCEEDED` |
| `IH41-08` | Tra cứu vết Audit Log M11 sau khi SupportAgent tra cứu lịch sử | Ghi nhận Audit Event `ACT-M11-41` đính kèm TicketId |
| `IH41-09` | SecurityAdmin tra cứu lịch sử danh tính của User B với lý do $\ge 15$ char | Trả về Security Audit Log DTO với IP đã băm Salted SHA-256 |
| `IH41-10` | SecurityAdmin tra cứu lịch sử nhưng nhập lý do quá ngắn ($< 15$ char) | Reject 400 `QUERY_REASON_MIN_LENGTH_15` |
| `IH41-11` | SuperAdmin yêu cầu giải băm PII thô của User B mà chưa có chữ ký 2 | Reject 400 `DUAL_APPROVAL_REQUIRED_FOR_UNMASKING` |
| `IH41-12` | Tải đồng thời 100 request tra cứu lịch sử từ 10 SupportAgent | 100% request được phân quyền chuẩn xác theo đúng ticket |
| `IH41-13` | User chưa đăng nhập gọi API tra cứu lịch sử danh tính | Deny 401 Unauthorized |
| `IH41-14` | Tra cứu lịch sử của một User không tồn tại trong CSDL | Reject 404 `TARGET_USER_NOT_FOUND` |
| `IH41-15` | SupportAgent tra cứu lịch sử của 1 tài khoản đã bị Xóa (Anonymized) | Chỉ hiển thị các bản ghi đã ẩn danh hóa GDPR |
| `IH41-16` | Kiểm tra thời gian phản hồi API tra cứu lịch sử danh tính | Response latency p95 $< 30\text{ms}$ |
| `IH41-17` | Phân tích tham chiếu các quyền tra cứu trong ma trận RBAC | Quét permissions `identity:history:read` M11-T004 (T020) |
| `IH41-18` | Thao tác ghi vết Audit M11 bị gián đoạn do lỗi CSDL | Rollback transaction, không trả về dữ liệu lịch sử |
| `IH41-19` | SupportAgent thử tra cứu lịch sử của 1 `SuperAdmin` | Deny 403 Forbidden (Bảo vệ cấp cao) |
| `IH41-20` | Kiểm thử hoàn tất luồng chuẩn hóa quyền tra cứu lịch sử M01-IDENTITY-HISTORY-AUTHORIZATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-IH-I01` | Chưa có bộ kiểm tra `ActiveTicket` khi SupportAgent gọi API tra cứu | Risk SupportAgent xem lén lịch sử người học mà không có vụ việc hỗ trợ | M01-T049 (Source task) |
| `M01-IH-I02` | Chưa cài đặt Rate Limiter 20 requests/phút cho SupportAgent | Risk bị cào vét toàn bộ danh sách thiết bị và thời gian học của user | M01-T049; M12-T034 |
| `M01-IH-I03` | CSDL M01 chưa có cờ cấm `ContentAdmin` tra cứu lịch sử danh tính | ContentAdmin có thể lợi dụng quyền để xem thông tin riêng tư | M01-T049; REL-02 |
| `M01-IH-I04` | Thiếu ghi log Audit Event `ACT-M11-41` khi có lệnh tra cứu lịch sử | Không phát hiện được các hành động lạm dụng quyền tra cứu của Admin | M01-T049; M11-T031 |
| `M01-IH-I05` | Chưa mã hóa/băm Salted SHA-256 các IP trả về trong DTO tra cứu | Rủi ro lộ vị trí địa lý chính xác của người học cho SupportAgent | M01-T049; M01-T033 |

- `M01-IH-F01`: Triển khai `IdentityHistoryAuthorizationService` với 4 Level Access Control (tiếp nhận: M01-T049).
- `M01-IH-F02`: Tích hợp Bắt buộc `Active Ticket Requirement` cho SupportAgent (tiếp nhận: M01-T049; M11-T028).
- `M01-IH-F03`: Thiết lập Rate Limiter 20 queries/min & Audit Log `ACT-M11-41` (tiếp nhận: M01-T049; M12-T034).
- `M01-IH-F04`: Thiết lập bộ kiểm thử tự động IH-G01–G10 và IH41-01–20 (tiếp nhận: M01 tasks).
- `M01-IH-F05`: Thu thập bằng chứng runtime cho luồng tra cứu lịch sử M01 (tiếp nhận: M01 tasks; A-G02).

## 8. Tự kiểm M01-T041

- Đã thiết kế hoàn chỉnh `M01-IDENTITY-HISTORY-AUTHORIZATION-1.0` với 4 Cấp độ Quyền hạn Tra cứu Lịch sử.
- Đã chốt Ràng buộc Ticket Hỗ trợ Active (`HasActiveTicket == true`) cho SupportAgent.
- Đã chốt Ràng buộc Cấm hoàn toàn vai trò `ContentAdmin` tra cứu lịch sử danh tính (REL-02).
- Đã lồng ghép Rate Limiting 20 requests/phút chống cào vét và Lưu vết Audit Log M11 (`ACT-M11-41`).
- Đã xác lập 10 Regression Gates (`IH-G01`–`IH-G10`) và 20 Test Cases tự kiểm (`IH41-01`–`IH41-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chuẩn hóa quyền tra cứu lịch sử danh tính M01-T041 | WSA-7K2 |
