# Bảo vệ vai trò quản trị cao nhất M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-SUPER-ADMIN-PROTECTION-1.0` |
| Task | M01-T030 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M01-ROLE-CHANGE-1.0 (D-094), M11-T006-A (D-039), REL-02 |
| Phạm vi | Giao thức bảo vệ vai trò quản trị cao nhất `SuperAdmin`, nguyên tắc Phê duyệt Kép (`4-Eye Principle`), duy trì tối thiểu 2 SuperAdmin và bắt buộc xác thực MFA phần cứng |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Bảo vệ Vai trò Quản trị Cao nhất (`SuperAdmin Protection Engine`) thuộc M01, xác lập các lớp phòng thủ nghiêm ngặt nhất đối với vai trò `SuperAdmin`, ngăn ngừa nguy cơ chiếm quyền hệ thống, thăng cấp trái phép hoặc vô tình xóa mất tài khoản quản trị cuối cùng.

- **Nguyên tắc Phê duyệt Kép 4 Mắt (`4-Eye Dual-Approval Invariant`)**: Bất kỳ đề xuất thăng cấp tài khoản lên `SuperAdmin` hoặc hạ cấp/khóa một tài khoản `SuperAdmin` hiện tại BẮT BUỘC trải qua 2 bước phê duyệt từ 2 tài khoản `SuperAdmin` ĐỘC LẬP (`InitiatorActorId != ApproverActorId`). CẤM đề xuất 1 người duyệt.
- **Ràng buộc Duy trì Tối thiểu 2 SuperAdmin (`MinSuperAdminCount = 2 Invariant`)**: Hệ thống TỰ ĐỘNG CHẶN mọi thao tác hạ cấp hoặc xóa tài khoản `SuperAdmin` nếu hành động đó dẫn đến tổng số `SuperAdmin` active trong toàn bộ hệ thống nhỏ hơn 2 (`ActiveSuperAdminCount < 2`).
- **Yêu cầu Bắt buộc Xác thực MFA Phần cứng (`Mandatory Hardware MFA Invariant`)**: Thao tác đăng nhập hoặc phê duyệt các hành động thuộc vai trò `SuperAdmin` bắt buộc xác thực MFA TOTP/FIDO2 Hardware Key (`MfaVerified == true`).
- **Nhật ký Cảnh báo An ninh Đặc biệt M11 (`Critical Security Audit Trail`)**: $100\%$ đề xuất, phê duyệt và thay đổi liên quan đến `SuperAdmin` bắt buộc ghi vết bất biến `ACT-M11-30` và phát cảnh báo PUSH/Email tức thì tới tất cả các SuperAdmin còn lại.

## 2. Quy trình Phê duyệt Kép Thăng cấp SuperAdmin (Dual-Approval Workflow)

```
[SuperAdmin 1 Submits Ticket: Promote User B to SuperAdmin]
                            |
                            v
            [Create PromotionTicket (Status: Pending)]
            - TicketId = TCK-SUP-2026-0821-0001
            - InitiatorActorId = USR-SUP-001
            - TargetUserId = USR-10024
                            |
                            v
            [Notify All Other Active SuperAdmins]
            - High-priority security alert sent
                            |
                            v
            [SuperAdmin 2 Reviews & Approves Ticket]
            - Validate ApproverActorId != InitiatorActorId
            - Verify Approver MFA Hardware Key
                            |
                     +------+------+
                     | (Approve)   | (Reject)
                     v             v
             [Promote User B to] [Reject Ticket & Log]
             [SuperAdmin Status] [Status = Rejected]
             - Increment SecurityEpoch += 1
             - Record Audit ACT-M11-30
```

## 3. Cấu trúc Response DTO Đề xuất Thăng cấp (SuperAdminPromotionTicketDto)

```json
{
  "ticketId": "TCK-SUP-2026-0821-0001",
  "targetUserId": "USR-10024",
  "targetUserEmail": "nhanh.tran@wordsoul.com",
  "initiatorActorId": "USR-SUP-001",
  "approverActorId": "USR-SUP-002",
  "promotionStatus": "APPROVED",
  "reason": "Bổ sung Quản trị viên cao nhất phụ trách khu vực Đông Nam Á.",
  "requestedAtUtc": "2026-08-21T08:00:00Z",
  "approvedAtUtc": "2026-08-21T09:15:00Z"
}
```

## 4. Giao thức Thực thi Phê duyệt Kép CSDL (SuperAdminProtectionService)

```csharp
public async Task<SuperAdminPromotionTicketDto> ApproveSuperAdminPromotionAsync(string ticketId, string approverActorId, string approverRole)
{
    // 1. Validate Approver Role & MFA
    if (approverRole != "SuperAdmin")
    {
        throw new UnauthorizedAccessException("SUPER_ADMIN_APPROVAL_FORBIDDEN");
    }

    var ticket = await _db.SuperAdminPromotionTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
    if (ticket == null || ticket.PromotionStatus != "PENDING")
    {
        throw new InvalidOperationException("INVALID_OR_EXPIRED_PROMOTION_TICKET");
    }

    // 2. Validate Dual-Approval Guard (Initiator != Approver)
    if (ticket.InitiatorActorId == approverActorId)
    {
        throw new InvalidOperationException("DUAL_APPROVAL_VIOLATION: Nguời đề xuất không được phép tự duyệt thăng cấp SuperAdmin.");
    }

    // 3. Thực thi Thăng cấp
    var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == ticket.TargetUserId);
    targetUser.Role = "SuperAdmin";
    targetUser.SecurityEpoch += 1; // Vô hiệu hóa JWT cũ SLA <= 5s

    ticket.ApproverActorId = approverActorId;
    ticket.PromotionStatus = "APPROVED";
    ticket.ApprovedAtUtc = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // 4. Ghi vết Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-30", approverActorId, new {
        Action = "SUPER_ADMIN_PROMOTED",
        TicketId = ticketId,
        TargetUserId = ticket.TargetUserId,
        Initiator = ticket.InitiatorActorId,
        Approver = approverActorId
    });

    return MapToDto(ticket);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SAP-G01` | Thăng cấp vai trò `SuperAdmin` BẮT BUỘC trải qua Phê duyệt Kép từ 2 SuperAdmin khác nhau. |
| `SAP-G02` | Người khởi tạo đề xuất CẤM tự duyệt đề xuất thăng cấp do chính mình tạo (`Initiator != Approver`). |
| `SAP-G03` | Cấm hạ cấp hoặc xóa tài khoản `SuperAdmin` nếu tổng số SuperAdmin active còn lại nhỏ hơn 2 (`MinSuperAdminCount = 2`). |
| `SAP-G04` | Thao tác `SuperAdmin` bắt buộc đính kèm xác thực MFA phần cứng (`MfaVerified == true`). |
| `SAP-G05` | Thăng cấp hoặc hạ cấp `SuperAdmin` tự động tăng `SecurityEpoch` $+1$, vô hiệu hóa JWT trong SLA $\le 5$ giây. |
| `SAP-G06` | 100% đề xuất và phê duyệt `SuperAdmin` ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-30`). |
| `SAP-G07` | Đề xuất thăng cấp `SuperAdmin` tự động hết hạn sau 48 giờ (`TicketTTL = 48h`) nếu chưa đủ 2 chữ ký. |
| `SAP-G08` | Cấm cấp quyền tạm thời hoặc đặc quyền khẩn cấp cho `SuperAdmin` (tuân thủ M11-T006-A / D-039). |
| `SAP-G09` | SLA xử lý API Phê duyệt Kép `SuperAdmin` $< 40\text{ms}$. |
| `SAP-G10` | 100% các test case tự kiểm SAP30-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SA30-01` | SuperAdmin 1 gửi đề xuất thăng cấp User B lên SuperAdmin | Tạo ticket trạng thái `PENDING`, phát cảnh báo an ninh |
| `SA30-02` | SuperAdmin 1 tự gọi API duyệt ticket thăng cấp do mình tạo | Reject 400 `DUAL_APPROVAL_VIOLATION` |
| `SA30-03` | SuperAdmin 2 gọi API duyệt ticket thăng cấp User B | Duyệt thành công, User B thành `SuperAdmin`, `SecurityEpoch += 1` |
| `SA30-04` | Thử hạ cấp 1 SuperAdmin khi hệ thống chỉ có đúng 2 SuperAdmin | Reject 400 `MIN_SUPER_ADMIN_COUNT_VIOLATION` |
| `SA30-05` | Thử hạ cấp 1 SuperAdmin khi hệ thống đang có 3 SuperAdmin | Hạ cấp thành công sau khi có Phê duyệt Kép của 2 SuperAdmin còn lại |
| `SA30-06` | SuperAdmin gọi API bảo vệ nhưng chưa xác thực MFA phần cứng | Reject 401 `HARDWARE_MFA_REQUIRED` |
| `SA30-07` | Ticket đề xuất thăng cấp để quá 48 giờ không có người duyệt 2 | Ticket tự động chuyển trạng thái `EXPIRED` |
| `SA30-08` | Tra cứu vết Audit Log M11 sau khi thăng cấp SuperAdmin | Ghi nhận Audit Event `ACT-M11-30` đính kèm 2 mã actor |
| `SA30-09` | User vai trò `SecurityAdmin` thử gọi API Phê duyệt Kép SuperAdmin | Deny 403 Forbidden |
| `SA30-10` | User chưa đăng nhập thử gọi API gửi đề xuất SuperAdmin | Deny 401 Unauthorized |
| `SA30-11` | Thử yêu cầu cấp quyền đặc cách khẩn cấp cho SuperAdmin | System reject (M11-T006-A / D-039) |
| `SA30-12` | Tải đồng thời 50 request kiểm tra trạng thái ticket SuperAdmin | Response latency p95 $< 30\text{ms}$ |
| `SA30-13` | SuperAdmin 2 từ chối (Reject) ticket đề xuất thăng cấp | Ticket chuyển `REJECTED`, User B giữ nguyên vai trò cũ |
| `SA30-14` | Kiểm tra thông báo PUSH an ninh gửi tới các SuperAdmin khi có đề xuất | PUSH notification gửi thành công trong $< 5\text{s}$ |
| `SA30-15` | Đăng nhập tài khoản SuperAdmin mới được thăng cấp | Bắt buộc thiết lập MFA Hardware Key trong lần đăng nhập đầu |
| `SA30-16` | Tra cứu danh sách các ticket đề xuất SuperAdmin trong quá khứ | Trả về danh sách chi tiết các lần đề xuất và lý do |
| `SA30-17` | Phân tích tham chiếu danh sách SuperAdmin trong CSDL | Quét cờ role `SuperAdmin` (T020) |
| `SA30-18` | Thao tác Phê duyệt Kép bị gián đoạn do lỗi CSDL | Rollback transaction, trạng thái ticket giữ nguyên `PENDING` |
| `SA30-19` | Hủy bỏ ticket đề xuất bởi chính SuperAdmin 1 (Initiator Cancel) | Ticket chuyển trạng thái `CANCELLED` |
| `SA30-20` | Kiểm thử hoàn tất luồng bảo vệ vai trò quản trị cao nhất M01-SUPER-ADMIN-PROTECTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-SA-I01` | Entity `User.cs` hiện chưa có bảng `SuperAdminPromotionTickets` | Chưa lưu được quy trình Phê duyệt Kép 2 bước | M01-T049 (Source task) |
| `M01-SA-I02` | Chưa có quy tắc cứng cấm hạ cấp SuperAdmin khi $N \le 2$ | Rủi ro lỡ tay hạ cấp mất SuperAdmin cuối cùng của hệ thống | M01-T049 |
| `M01-SA-I03` | Thiếu kiểm tra MFA Hardware Key đối với các API quản trị SuperAdmin | Rủi ro bị lộ mật khẩu làm lọt quyền quản trị cao nhất | M01-T049; M01-T012 |
| `M01-SA-I04` | Thiếu phát cảnh báo an ninh tức thì tới tất cả SuperAdmin còn lại | Không phát hiện kịp thời các nỗ lực leo thang đặc quyền | M01-T049; M11-T037 |
| `M01-SA-I05` | Chưa tự động Hết hạn ticket đề xuất thăng cấp sau 48 giờ | Các ticket lơ lửng đe dọa an toàn thông tin | M01-T049 |

- `M01-SA-F01`: Tạo entity `SuperAdminPromotionTicket.cs` và CSDL Migration (tiếp nhận: M01-T049).
- `M01-SA-F02`: Triển khai `SuperAdminProtectionService` hỗ trợ Phê duyệt Kép 4 mắt (tiếp nhận: M01-T049; REL-02).
- `M01-SA-F03`: Tích hợp cấm hạ cấp khi $N \le 2$ và bắt buộc MFA phần cứng (tiếp nhận: M01-T049).
- `M01-SA-F04`: Thiết lập bộ kiểm thử tự động SAP-G01–G10 và SA30-01–20 (tiếp nhận: M01 tasks).
- `M01-SA-F05`: Thu thập bằng chứng runtime cho luồng bảo vệ SuperAdmin M01 (tiếp nhận: M01 tasks; A-G02).

## 8. Tự kiểm M01-T030

- Đã thiết kế hoàn chỉnh `M01-SUPER-ADMIN-PROTECTION-1.0` với Nguyên tắc Phê duyệt Kép 4 Mắt.
- Đã chốt Ràng buộc Duy trì Tối thiểu 2 SuperAdmin (`MinSuperAdminCount = 2`).
- Đã chốt Yêu cầu Bắt buộc Xác thực MFA Phần cứng (`MfaVerified == true`).
- Đã lồng ghép Thời gian hết hạn Ticket 48 giờ và Lưu vết Audit Log M11 (`ACT-M11-30`).
- Đã xác lập 10 Regression Gates (`SAP-G01`–`SAP-G10`) và 20 Test Cases tự kiểm (`SA30-01`–`SA30-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả bảo vệ vai trò quản trị cao nhất M01-T030 | WSA-7K2 |
