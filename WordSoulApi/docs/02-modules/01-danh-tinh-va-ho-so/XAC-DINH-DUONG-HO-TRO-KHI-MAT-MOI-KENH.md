# Xác định đường hỗ trợ khi mất mọi kênh M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-LOST-CHANNEL-RECOVERY-1.0` |
| Task | M01-T021 |
| Đầu vào | M01-RECOVERY-1.0 (D-030), M11-SUPPORT-TICKET-LIFECYCLE-1.0 (D-108), REL-01, REL-07 |
| Phạm vi | Đặc tả Giao thức Hỗ trợ Khôi phục Tài khoản khi Mất Mọi Kênh (`Lost-Channel Account Recovery Protocol`), 3 tiêu chí đối soát minh chứng lịch sử, quy trình duyệt 2 người (`SupportAgent` + `SecurityAdmin`) và tạo mã phục hồi tạm thời 24h |
| Tự kiểm | A-G01, A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Hỗ trợ Khôi phục Tài khoản khi Mất Mọi Kênh (`Lost-Channel Account Recovery Protocol`) thuộc M01, cung cấp quy trình phân xử thủ công cho trường hợp người học bị mất đồng thời mật khẩu và quyền truy cập hòm thư email khôi phục, đảm bảo đối soát chính xác chủ sở hữu hợp pháp và ngăn ngừa nguy cơ bị mạo danh chiếm đoạt tài khoản (REL-01, REL-07).

- **3 Tiêu chí Đối soát Minh chứng Lịch sử Bắt buộc (`3 Historical Identity Evidences Invariant`)**: Để khiếu nại khôi phục tài khoản mất mọi kênh, người học BẮT BUỘC cung cấp tối thiểu 2/3 nhóm minh chứng lịch sử sau:
  - *Evid_01 - Registered Device History*: Mã UUID thiết bị quen thuộc đã từng đăng nhập tài khoản trong 90 ngày.
  - *Evid_02 - Learning Milestones (M03)*: Thông tin chính xác về 2 bộ từ đã hoàn thành hoặc ngày đạt mốc học gần nhất.
  - *Evid_03 - Transaction/Item Proof (M06)*: Mã đơn hàng mua vật phẩm/Gold hoặc thông tin giao dịch thành công.
- **Ràng buộc Phê duyệt Kép 2 Người (`Dual-Approval Recovery Guard`)**: Yêu cầu đổi email khôi phục do mất mọi kênh BẮT BUỘC trải qua 2 bước phê duyệt: Bước 1 do `SupportAgent` đối soát minh chứng lịch sử; Bước 2 do `SecurityAdmin` phê duyệt cuối cùng (`Initiator != Approver`).
- **Cập nhật Email và Tăng SecurityEpoch $+1$ (`Email Reset & Instant Revocation`)**: Ngay khi Ticket khôi phục mất mọi kênh được duyệt (`APPROVED`), hệ thống TỰ ĐỘNG: Cập nhật địa chỉ email mới, tăng `SecurityEpoch` $+1$ hủy toàn bộ phiên cũ, và tạo một Mã khôi phục mật khẩu tạm thời TTL 24 giờ gửi tới email mới.
- **Lưu vết Sổ Kiểm toán Khôi phục M11 (`Lost Channel Recovery Audit Trail`)**: $100\%$ các thương vụ khôi phục tài khoản mất mọi kênh bắt buộc được ghi vết bất biến `ACT-M11-21` trong Sổ Kiểm toán M11, bao gồm `TargetUserId`, `NewEmail`, `AgentUserId`, `SecurityAdminUserId`, `ApprovedTicketId` và `EvidenceScores`.

## 2. Quy trình 4 Bước Phân xử Khôi phục Tài khoản Mất Mọi Kênh (Workflow Pipeline)

```
[Learner Lost All Channels -> Submits Recovery Claim Form]
                          |
                          v
        [Verify 3 Historical Evidences (2/3 Passed?)]
        - Evid_01: Device UUID match
        - Evid_02: M03 Learning milestones match
        - Evid_03: M06 Transaction IDs match
                          |
                          v
         [Create Ticket M11 (State: IN_PROGRESS)]
                          |
                          v
  [Step 1: SupportAgent Verifies & Signs Approval]
                          |
                          v
  [Step 2: SecurityAdmin Reviews & Signs Dual-Approval]
                          |
                          v
      [System Executes Automatic Account Recovery]
      - Update User Email to New Verified Email
      - Increment SecurityEpoch +1 (Revoke All Sessions SLA <= 5s)
      - Generate 24h Temp Password Code to New Email
      - Record Audit Event ACT-M11-21
```

## 3. Giao thức Thực thi Khôi phục CSDL (LostChannelAccountRecoveryService)

```csharp
public async Task<bool> ExecuteLostChannelAccountRecoveryAsync(
    string targetUserId, 
    string newEmail, 
    string ticketId, 
    string supportAgentId, 
    string securityAdminId)
{
    // 1. Dual-Control Approval Guard
    if (supportAgentId == securityAdminId)
    {
        throw new UnauthorizedAccessException("DUAL_APPROVAL_REQUIRED: SecurityAdmin phải độc lập với SupportAgent khởi tạo.");
    }

    var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId && t.State == SupportTicketState.IN_PROGRESS);
    if (ticket == null) throw new InvalidOperationException("ACTIVE_SUPPORT_TICKET_REQUIRED");

    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
    if (user == null) throw new InvalidOperationException("USER_NOT_FOUND");

    // 2. Atomic Update Email & Increment SecurityEpoch +1
    user.Email = newEmail;
    user.IsEmailVerified = true;
    user.SecurityEpoch += 1; // Revoke all old JWT sessions SLA <= 5s
    user.UpdatedAtUtc = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // 3. Update Redis SecurityEpoch
    await _redisDb.StringSetAsync($"wordsoul:security_epoch:{targetUserId}", user.SecurityEpoch);

    // 4. Generate 24-Hour Temporary Recovery Code
    string tempRecoveryCode = $"{Guid.NewGuid():N}[..12]".ToUpperInvariant();
    await _redisDb.StringSetAsync($"wordsoul:lost_channel_code:{targetUserId}", tempRecoveryCode, TimeSpan.FromHours(24));

    // Send email via M10
    await _eventPublisher.PublishAsync(new LostChannelRecoveryApprovedIntegrationEvent {
        UserId = targetUserId,
        NewEmail = newEmail,
        TempCode = tempRecoveryCode
    });

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-21", securityAdminId, new {
        TargetUserId = targetUserId,
        NewEmail = newEmail,
        TicketId = ticketId,
        SupportAgentId = supportAgentId,
        SecurityAdminId = securityAdminId,
        NewSecurityEpoch = user.SecurityEpoch
    });

    return true;
}
```

## 4. Regression Gate và Case tự kiểm

### 4.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LC-G01` | Khôi phục tài khoản mất mọi kênh BẮT BUỘC đạt tối thiểu 2/3 nhóm minh chứng lịch sử (Device, M03, M06). |
| `LC-G02` | Quy trình đổi email do mất mọi kênh BẮT BUỘC có 2 chữ ký phê duyệt kép (`SupportAgent` + `SecurityAdmin`). |
| `LC-G03` | Cập nhật email mới tự động tăng `SecurityEpoch` $+1$, ngắt sạch $100\%$ phiên làm việc cũ SLA $\le 5$ giây. |
| `LC-G04` | Mã khôi phục tạm thời 12 ký tự có thời hạn hiệu lực tối đa đúng 24 giờ (`TTL = 24h`). |
| `LC-G05` | Tuyệt đối CẤM SupportAgent tự mình vừa khởi tạo vừa phê duyệt đổi email tài khoản mà không có SecurityAdmin. |
| `LC-G06` | 100% các vụ việc khôi phục tài khoản mất mọi kênh được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-21`). |
| `LC-G07` | Email mới nhận mã khôi phục tạm thời phải kiểm tra cờ chưa tồn tại ở bất kỳ tài khoản local nào khác (CT-02). |
| `LC-G08` | Phân quyền phê duyệt cuối cùng cho luồng mất mọi kênh chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `LC-G09` | SLA thực thi API phê duyệt khôi phục mất mọi kênh $< 30\text{ms}$. |
| `LC-G10` | 100% các test case tự kiểm LC21-01–20 đạt thành công trong bộ suite kiểm thử. |

### 4.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LC21-01` | Đơn khiếu nại đạt 2/3 minh chứng (Device UUID + M03 Milestone) | Đủ điều kiện khởi tạo Ticket khôi phục mất mọi kênh |
| `LC21-02` | Đơn khiếu nại chỉ đạt 1/3 minh chứng (Chỉ có M06 Purchase ID) | Reject 400 `INSUFFICIENT_IDENTITY_EVIDENCE` |
| `LC21-03` | SupportAgent A khởi tạo đơn, SecurityAdmin B phê duyệt cuối | Cập nhật email thành công, phát mã tạm 24h sang email mới |
| `LC21-04` | SupportAgent A thử tự mình phê duyệt luôn đơn khôi phục do mình tạo | Reject 403 `DUAL_APPROVAL_REQUIRED` |
| `LC21-05` | Đơn khôi phục được duyệt thành công cho User B | SecurityEpoch $+1$, ngắt toàn bộ phiên JWT cũ của User B |
| `LC21-06` | User B nhập mã khôi phục tạm 12 ký tự gửi sang email mới trong vòng 24h | Xác thực thành công, yêu cầu thiết lập mật khẩu mới |
| `LC21-07` | User B nhập mã khôi phục tạm sau 25 giờ ($> 24$h TTL) | Reject 400 `TEMPORARY_CODE_EXPIRED` |
| `LC21-08` | Tra cứu vết Audit Log M11 sau khi phê duyệt khôi phục mất mọi kênh | Ghi nhận Audit Event `ACT-M11-21` đính kèm 2 Admin ID |
| `LC21-09` | Email mới xin cập nhật lại trùng với email của User C đang hoạt động | Reject 400 `EMAIL_ALREADY_EXISTS_CT02` |
| `LC21-10` | Tải đồng thời 50 request phê duyệt khôi phục mất mọi kênh | Processing latency p95 $< 25\text{ms}$ |
| `LC21-11` | Thử nhập mã khôi phục tạm thời sai 5 lần liên tiếp | Vô hiệu mã tạm thời, yêu cầu làm lại quy trình |
| `LC21-12` | Kích hoạt gửi email khôi phục chứa mã tạm thời sang M10 | Email được gửi tức thì trong SLA $< 10\text{s}$ |
| `LC21-13` | User không phải SecurityAdmin thử gọi API phê duyệt bước 2 | Deny 403 Forbidden |
| `LC21-14` | User chưa đăng nhập gọi API phê duyệt khôi phục | Deny 401 Unauthorized |
| `LC21-15` | Đơn khiếu nại bị SecurityAdmin từ chối ở bước 2 | Chuyển Ticket sang `RESOLVED`, không thay đổi email user |
| `LC21-16` | Kiểm tra thời gian vô hiệu toàn bộ phiên JWT sau khi duyệt | Invalidation SLA $< 50\text{ms}$ |
| `LC21-17` | Phân tích tham chiếu các khóa mã tạm thời trong Redis | Quét namespace `wordsoul:{env}:lost_channel_code` (T020) |
| `LC21-18` | Thao tác gửi email M10 bị gián đoạn do ngắt kết nối SMTP | Retry tự động theo Outbox Pattern M12-T037 |
| `LC21-19` | Tra cứu danh sách các Ticket khôi phục mất mọi kênh đang chờ duyệt | Trả về danh sách Ticket M11 kèm điểm số minh chứng |
| `LC21-20` | Kiểm thử hoàn tất luồng xác định đường hỗ trợ mất mọi kênh M01-LOST-CHANNEL-RECOVERY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-LC-I01` | M01 hiện tại chưa có bộ `LostChannelAccountRecoveryService` đối soát 3 minh chứng | Người dùng mất mọi kênh không thể khôi phục tài khoản | M01-T049 (Source task) |
| `M01-LC-I02` | Thiếu cờ Phê duyệt Kép 2 Người (`SupportAgent` + `SecurityAdmin`) | Risk SupportAgent tự ý đổi email người dùng chiếm đoạt tài khoản | M01-T049; REL-02 |
| `M01-LC-I03` | Thiếu luồng tạo mã khôi phục tạm thời 12 ký tự TTL 24h trong Redis | Người dùng không thiết lập lại mật khẩu sau khi đổi email | M01-T049; M01-T019 |
| `M01-LC-I04` | Thiếu cờ tự động tăng `SecurityEpoch` $+1$ khi duyệt đổi email mất mọi kênh | Kẻ chiếm đoạt vẫn duy trì phiên JWT cũ sau khi email đã bị đổi | M01-LC-F04; M01-T016 |
| `M01-LC-I05` | Chưa kết nối sự kiện khôi phục mất mọi kênh với Audit Log M11 (`ACT-M11-21`) | Không ghi vết được 2 Admin phê duyệt vụ việc | M01-T049; M11-T031 |

- `M01-LC-F01`: Triển khai `LostChannelAccountRecoveryService` đối soát 3 minh chứng (tiếp nhận: M01-T049).
- `M01-LC-F02`: Tích hợp Bắt buộc `Dual-Approval Recovery Guard` 2 người (tiếp nhận: M01-T049; REL-02).
- `M01-LC-F03`: Triển khai Mã khôi phục tạm thời TTL 24h & Epoch $+1$ (tiếp nhận: M01-T049; M01-T016).
- `M01-LC-F04`: Thiết lập bộ kiểm thử tự động LC-G01–G10 và LC21-01–20 (tiếp nhận: M01 tasks).
- `M01-LC-F05`: Thu thập bằng chứng runtime cho luồng khôi phục mất mọi kênh M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T021

- Đã thiết kế hoàn chỉnh `M01-LOST-CHANNEL-RECOVERY-1.0` với 3 Tiêu chí Đối soát Minh chứng Lịch sử Bắt buộc.
- Đã chốt Ràng buộc Phê duyệt Kép 2 Người (`SupportAgent` + `SecurityAdmin`).
- Đã chốt Cập nhật Email và Tăng `SecurityEpoch` $+1$ ngắt sạch phiên cũ.
- Đã lồng ghép Mã khôi phục tạm thời 12 ký tự TTL 24h và Lưu vết Audit Log M11 (`ACT-M11-21`).
- Đã xác lập 10 Regression Gates (`LC-G01`–`LC-G10`) và 20 Test Cases tự kiểm (`LC21-01`–`LC21-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả xác định đường hỗ trợ khi mất mọi kênh M01-T021 | WSA-7K2 |
