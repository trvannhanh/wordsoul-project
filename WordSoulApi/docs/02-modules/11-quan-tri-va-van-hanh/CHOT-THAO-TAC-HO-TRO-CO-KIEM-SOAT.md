# Chốt thao tác hỗ trợ có kiểm soát M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-CONTROLLED-SUPPORT-MUTATION-1.0` |
| Task | M11-T030 |
| Đầu vào | M11-ENHANCED-CONTROL-1.0 (D-038), M11-SUPPORT-TICKET-LIFECYCLE-1.0 (D-108), REL-02, REL-07 |
| Phạm vi | Đặc tả Giao thức Thao tác Hỗ trợ có Kiểm soát (`Controlled Support Mutation Engine`), danh mục 5 thao tác hỗ trợ hợp lệ, quy tắc xác thực lại (`Re-Authentication Guard`) và hạn mức đền bù điểm thưởng |
| Tự kiểm | A-G02; REL-02, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thao tác Hỗ trợ có Kiểm soát (`Controlled Support Mutation Engine`) thuộc M11, xác lập ranh giới các thao tác biến đổi dữ liệu được phép thực hiện bởi Chuyên viên Hỗ trợ (SupportAgent), loại bỏ nguy cơ lạm dụng quyền thay đổi mật khẩu, email hoặc vai trò người dùng bất hợp pháp (REL-02, REL-07).

- **Nguyên tắc Biến đổi Dữ liệu Tối thiểu (`Minimal Data Mutation Invariant`)**: SupportAgent tuyệt đối CẤM sửa trực tiếp Mật khẩu, Email, SĐT hoặc Vai trò của người dùng. Chỉ 5 thao tác hỗ trợ chuẩn hóa sau đây được phép thực hiện:
  - *OP_01_RESEND_VERIFICATION_EMAIL*: Gửi lại thư xác minh email.
  - *OP_02_UNBLOCK_FAILED_LOGINS*: Xóa bộ đếm thử sai mật khẩu, mở khóa tạm thời (TEMPORARY_AUTO_LOCK).
  - *OP_03_FORCE_DEVICE_LOGOUT*: Đăng xuất cưỡng chế toàn bộ thiết bị (Tăng SecurityEpoch $+1$).
  - *OP_04_CANCEL_PENDING_ORDER*: Hủy giao dịch mua vật phẩm chưa hoàn tất.
  - *OP_05_REWARD_COMPENSATION*: Đền bù Gold/Exp cho sự cố game (Hạn mức $\le 100$ Gold/Exp).
- **Ràng buộc Xác thực Lại Mật khẩu Quản trị (`Re-Authentication Guard`)**: Trước khi thực thi bất kỳ thao tác hỗ trợ có kiểm soát nào (OP_01–05), SupportAgent BẮT BUỘC xác thực lại mật khẩu hoặc TOTP MFA của chính mình trong thời gian $\le 5$ phút trước đó (`ReAuthVerified == true`).
- **Hạn mức Đền bù và Duyệt 2 Chữ ký (`Compensation Cap & Dual-Signature`)**: Thao tác đền bù `OP_05_REWARD_COMPENSATION` cho SupportAgent bị giới hạn tối đa 100 Gold/Exp mỗi lần và không quá 300 Gold/Exp mỗi ngày cho cùng 1 học viên. Mọi đền bù vượt hạn mức BẮT BUỘC chuyển sang phê duyệt của `SecurityAdmin`.
- **Nhật ký Đánh dấu Thao tác Hỗ trợ M11 (`Controlled Support Audit Trail`)**: $100\%$ các thao tác hỗ trợ có kiểm soát bắt buộc được ghi vết bất biến `ACT-M11-30` trong Sổ Kiểm toán M11, đính kèm `ActorUserId`, `TargetUserId`, `TicketId`, `OperationType`, `MutationValue` và `Reason` ($\ge 15$ ký tự).

## 2. Danh mục 5 Thao tác Hỗ trợ Được phép (Allowed Support Operations)

| Mã Thao tác (`OpCode`) | Tên Thao tác Hỗ trợ | Thẩm quyền Thực thi | Ràng buộc Hạn mức | Yêu cầu Mã Ticket | Nhật ký Audit M11 |
|---|---|---|---|---|---|
| `OP_01` | Resend Verification Email | SupportAgent | Max 3 lần / 24h | Bắt buộc `X-Support-Ticket-Id` | `ACT-M11-30-OP01` |
| `OP_02` | Unblock Failed Logins | SupportAgent | Chỉ mở khóa TEMPORARY | Bắt buộc `X-Support-Ticket-Id` | `ACT-M11-30-OP02` |
| `OP_03` | Force Device Logout | SupportAgent | Tăng Epoch $+1$ | Bắt buộc `X-Support-Ticket-Id` | `ACT-M11-30-OP03` |
| `OP_04` | Cancel Pending Transaction| SupportAgent | Giao dịch PENDING | Bắt buộc `X-Support-Ticket-Id` | `ACT-M11-30-OP04` |
| `OP_05` | Reward Compensation | SupportAgent | $\le 100$ Gold/Exp ($>100$ duyệt Admin) | Bắt buộc `X-Support-Ticket-Id` | `ACT-M11-30-OP05` |

## 3. Quy trình Thực thi Thao tác Hỗ trợ có Kiểm soát (Controlled Support Engine)

```
[SupportAgent Submits Support Action Request (OpCode, TargetUserId, TicketId, Reason)]
                                    |
                                    v
            [Validate Active Support Ticket (State: IN_PROGRESS)]
            - Verify TicketId active in Redis SLA <= 1s
                                    |
                                    v
            [Validate Re-Authentication Guard (ReAuth <= 5m)]
                                    |
                          +---------+---------+
                          | (Not Re-Authed)   | (Re-Authed OK)
                          v                   v
                   [Reject 401 Re-Auth] [Check Operation Matrix & Compensation Cap]
                                              |
                                     +--------+--------+
                                     | (Exceed Cap)    | (Within Cap)
                                     v                 v
                              [Require Admin    [Execute Controlled
                               Dual-Signature]   Mutation in DB]
                                                       |
                                                       v
                                                [Record Audit ACT-M11-30]
                                                       |
                                                       v
                                                [Return 200 OK Result]
```

## 4. Giao thức Thực thi Thao tác Hỗ trợ CSDL (ControlledSupportMutationService)

```csharp
public async Task<bool> ExecuteControlledSupportMutationAsync(
    string targetUserId, 
    string ticketId, 
    string opCode, 
    int compensationValue, 
    string reason, 
    string actorUserId, 
    string actorPassword)
{
    // 1. Validate Active Support Ticket State
    bool isTicketActive = await _supportTicketService.ValidateTicketActiveAsync(ticketId, targetUserId, actorUserId);
    if (!isTicketActive)
    {
        throw new UnauthorizedAccessException("INVALID_OR_CLOSED_SUPPORT_TICKET: Yêu cầu Ticket hỗ trợ đang ở trạng thái IN_PROGRESS.");
    }

    // 2. Re-authentication Guard
    bool isPasswordValid = await _identityService.VerifyActorPasswordAsync(actorUserId, actorPassword);
    if (!isPasswordValid)
    {
        throw new UnauthorizedAccessException("RE_AUTH_FAILED: Mật khẩu xác thực lại của Chuyên viên hỗ trợ không chính xác.");
    }

    if (string.IsNullOrEmpty(reason) || reason.Length < 15)
    {
        throw new ArgumentException("SUPPORT_MUTATION_REASON_MIN_LENGTH_15: Bắt buộc nhập lý do thao tác tối thiểu 15 ký tự.");
    }

    // 3. Execute Operation & Check Caps
    switch (opCode)
    {
        case "OP_02_UNBLOCK_FAILED_LOGINS":
            await _identityService.ResetFailedLoginAttemptsAsync(targetUserId);
            break;

        case "OP_03_FORCE_DEVICE_LOGOUT":
            await _identityService.IncrementSecurityEpochAsync(targetUserId); // Revoke all sessions SLA <= 5s
            break;

        case "OP_05_REWARD_COMPENSATION":
            if (compensationValue > 100)
            {
                throw new InvalidOperationException("COMPENSATION_CAP_EXCEEDED: Đền bù vượt quá 100 Gold/Exp yêu cầu SecurityAdmin phê duyệt.");
            }
            await _gamificationService.AddCompensationRewardAsync(targetUserId, compensationValue, reason);
            break;

        default:
            throw new ArgumentException("INVALID_SUPPORT_OPCODE");
    }

    // 4. Record Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-30", actorUserId, new {
        OpCode = opCode,
        TargetUserId = targetUserId,
        TicketId = ticketId,
        CompensationValue = compensationValue,
        Reason = reason
    });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CS-G01` | SupportAgent tuyệt đối CẤM sửa trực tiếp Mật khẩu, Email, SĐT hoặc Vai trò của người học. |
| `CS-G02` | Thao tác hỗ trợ chỉ được thực hiện khi có Ticket hỗ trợ active ở trạng thái `IN_PROGRESS` (D-108). |
| `CS-G03` | Thực thi thao tác BẮT BUỘC vượt qua xác thực lại mật khẩu SupportAgent (`ReAuthVerified == true`). |
| `CS-G04` | Đền bù `OP_05_REWARD_COMPENSATION` cho SupportAgent bị giới hạn tối đa 100 Gold/Exp mỗi lần. |
| `CS-G05` | Mọi thao tác đền bù $> 100$ Gold/Exp bắt buộc chuyển sang Phê duyệt Kép của `SecurityAdmin`. |
| `CS-G06` | Thao tác `OP_03_FORCE_DEVICE_LOGOUT` tự động tăng `SecurityEpoch` $+1$, hủy phiên JWT SLA $\le 5$ giây. |
| `CS-G07` | 100% các thao tác hỗ trợ có kiểm soát được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-30`). |
| `CS-G08` | Phân quyền thực thi 5 thao tác hỗ trợ chuẩn hóa dành riêng cho `SupportAgent`, `SecurityAdmin` và `SuperAdmin`. |
| `CS-G09` | SLA thực thi API thao tác hỗ trợ có kiểm soát $< 30\text{ms}$. |
| `CS-G10` | 100% các test case tự kiểm CS30-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CS30-01` | SupportAgent thực thi `OP_02_UNBLOCK_FAILED_LOGINS` cho User B kèm TicketId active hợp lệ | Mở khóa tạm thời thành công, ghi log `ACT-M11-30-OP02` |
| `CS30-02` | SupportAgent thực thi `OP_03_FORCE_DEVICE_LOGOUT` cho User B | Tăng `SecurityEpoch` $+1$, hủy mọi JWT session cũ của User B |
| `CS30-03` | SupportAgent thử sửa trực tiếp Email của User B qua API hỗ trợ | Reject 403 `DIRECT_PII_MUTATION_FORBIDDEN` |
| `CS30-04` | SupportAgent thực thi thao tác hỗ trợ nhưng nhập sai mật khẩu xác thực lại | Reject 401 `RE_AUTH_FAILED` |
| `CS30-05` | SupportAgent đền bù 50 Gold (`OP_05`) cho User B | Đền bù thành công ($\le 100$), cộng 50 Gold vào ví M06 |
| `CS30-06` | SupportAgent thử đền bù 150 Gold ($> 100$) cho User B | Reject 400 `COMPENSATION_CAP_EXCEEDED` (Yêu cầu Admin duyệt) |
| `CS30-07` | SupportAgent thực thi thao tác khi ticket hỗ trợ đã `CLOSED` | Reject 403 `INVALID_OR_CLOSED_SUPPORT_TICKET` |
| `CS30-08` | Tra cứu vết Audit Log M11 sau khi thực hiện `OP_03` | Ghi nhận Audit Event `ACT-M11-30` đính kèm mã OpCode |
| `CS30-09` | Thử thực hiện thao tác hỗ trợ với lý do 10 ký tự ($< 15$) | Reject 400 `SUPPORT_MUTATION_REASON_MIN_LENGTH_15` |
| `CS30-10` | SecurityAdmin phê duyệt đền bù 500 Gold cho vụ việc đặc biệt | Đền bù thành công sau khi có 2 chữ ký SecurityAdmin |
| `CS30-11` | Kiểm tra thời gian phản hồi API thao tác hỗ trợ có kiểm soát | Response latency p95 $< 25\text{ms}$ |
| `CS30-12` | Tải đồng thời 100 request thao tác hỗ trợ có kiểm soát | 100% request được xử lý nhất quán không race condition |
| `CS30-13` | User không phải SupportAgent/Admin gọi API thao tác hỗ trợ | Deny 403 Forbidden |
| `CS30-14` | User chưa đăng nhập gọi API thao tác hỗ trợ | Deny 401 Unauthorized |
| `CS30-15` | SupportAgent đền bù 300 Gold tổng cộng trong 1 ngày cho User B | Đạt trần hạn mức ngày, request đền bù tiếp theo bị chặn |
| `CS30-16` | Thực hiện gửi lại email xác minh `OP_01_RESEND_VERIFICATION_EMAIL` | Email xác minh mới được gửi tới M10 thành công |
| `CS30-17` | Phân tích tham chiếu các OpCode hỗ trợ trong CSDL M11 | Quét enum `M11_SupportOpCodes` (T020) |
| `CS30-18` | Thao tác đền bù M06 bị gián đoạn do lỗi CSDL | Rollback transaction, không lưu log Audit giả |
| `CS30-19` | SupportAgent thực hiện hủy đơn hàng PENDING (`OP_04`) | Đơn hàng PENDING được cập nhật trạng thái `CANCELLED` |
| `CS30-20` | Kiểm thử hoàn tất luồng thao tác hỗ trợ có kiểm soát M11-CONTROLLED-SUPPORT-MUTATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-CS-I01` | M11 hiện tại chưa có bộ `ControlledSupportMutationService` kiểm soát 5 OpCode | Risk SupportAgent tự ý sửa dữ liệu người dùng không qua kiểm soát | M11-T049 (Source task) |
| `M11-CS-I02` | Thiếu cờ xác thực lại mật khẩu (`ReAuthRequired`) khi thực hiện thao tác hỗ trợ | Risk bị người khác lợi dụng máy SupportAgent đang bật để thao tác | M11-T049; REL-02 |
| `M11-CS-I03` | Thiếu hạn mức đền bù tối đa 100 Gold/Exp cho SupportAgent | Risk gây thất thoát điểm thưởng Gamification M06 | M11-T049; M06 tasks |
| `M11-CS-I04` | Thiếu cờ validation lý do thao tác hỗ trợ tối thiểu 15 ký tự | SupportAgent nhập thông tin sơ sài không đủ vết đối soát | M11-T049 |
| `M11-CS-I05` | Chưa kết nối luồng `OP_03_FORCE_DEVICE_LOGOUT` với tăng `SecurityEpoch` M01 | Không ngắt được phiên JWT tức thì của người dùng | M11-T049; M01-T016 |

- `M11-CS-F01`: Triển khai `ControlledSupportMutationService` cho 5 OpCode (tiếp nhận: M11-T049).
- `M11-CS-F02`: Tích hợp Bắt buộc `Re-Authentication Guard` & Re-Auth TTL 5m (tiếp nhận: M11-T049; REL-02).
- `M11-CS-F03`: Thiết lập Hạn mức Đền bù 100 Gold/Exp & Duyệt 2 Chữ ký (tiếp nhận: M11-T049; M06 tasks).
- `M11-CS-F04`: Thiết lập bộ kiểm thử tự động CS-G01–G10 và CS30-01–20 (tiếp nhận: M11 tasks).
- `M11-CS-F05`: Thu thập bằng chứng runtime cho luồng thao tác hỗ trợ M11 (tiếp nhận: M11 tasks; A-G02).

## 8. Tự kiểm M11-T030

- Đã thiết kế hoàn chỉnh `M11-CONTROLLED-SUPPORT-MUTATION-1.0` với 5 Thao tác Hỗ trợ Được phép Chuẩn hóa.
- Đã chốt Ràng buộc Nguyên tắc Biến đổi Dữ liệu Tối thiểu (CẤM sửa trực tiếp Password, Email, Role).
- Đã chốt Ràng buộc Xác thực Lại Mật khẩu Quản trị (`ReAuthVerified == true`).
- Đã lồng ghép Hạn mức Đền bù 100 Gold/Exp và Lưu vết Audit Log M11 (`ACT-M11-30`).
- Đã xác lập 10 Regression Gates (`CS-G01`–`CS-G10`) và 20 Test Cases tự kiểm (`CS30-01`–`CS30-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt thao tác hỗ trợ có kiểm soát M11-T030 | WSA-7K2 |
