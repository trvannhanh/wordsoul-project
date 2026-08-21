# Chuẩn hóa thay đổi vai trò M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ROLE-CHANGE-1.0` |
| Task | M01-T029 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M01-SESSION-POLICY-1.0 (D-028), M11-AUDIT-EVENT-1.0 (D-054), REL-02 |
| Phạm vi | Giao thức nâng/hạ vai trò người dùng trong hệ thống RBAC, quy tắc chống tự nâng quyền (`Self-Promotion Guard`), quy trình mở rộng vô hiệu hóa token tức thì SLA $\le 5\text{s}$ |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Chuẩn hóa Thay đổi Vai trò Người dùng (`Role Change Protocol Engine`) thuộc M01, điều phối các thao tác thăng cấp, gián cấp hoặc chuyển đổi vai trò hệ thống (`Learner`, `ContentCreator`, `SupportAgent`, `ContentAdmin`, `SecurityAdmin`, `SuperAdmin`), bảo đảm tuân thủ nguyên tắc phân quyền tối thiểu REL-02.

- **Ràng buộc Phân quyền Thực thi Đổi Vai trò (`Role Mutation Authorization Invariant`)**: CHỈ vai trò `SecurityAdmin` hoặc `SuperAdmin` mới có quyền thay đổi vai trò của tài khoản khác. Riêng vai trò `SuperAdmin` bắt buộc thực thi qua giao thức Phê duyệt Kép M01-T030.
- **Quy tắc Cấm Tự Nâng/Hạ Quyền Chính Mình (`Self-Role Mutation Guard`)**: Quản trị viên tuyệt đối CẤM tự thay đổi vai trò của chính tài khoản của mình (`ActorUserId != TargetUserId`).
- **Tự động Vô hiệu hóa Phiên và Token Tức thì SLA $\le 5\text{s}$ (`Instant Claim Invalidation`)**: Ngay khi vai trò người dùng bị thay đổi, hệ thống TỰ ĐỘNG tăng `SecurityEpoch` $+1$. Toàn bộ JWT Token cũ bị vô hiệu hóa trong CSDL Redis SLA $\le 5$ giây, buộc người dùng đăng nhập lại để nhận `role` claim mới.
- **Lưu vết Sổ Kiểm toán Bất biến M11 (`Role Change Audit Trail`)**: $100\%$ thao tác thay đổi vai trò bắt buộc lưu vết bất biến `ACT-M11-29` trong CSDL, đính kèm `ActorUserId`, `TargetUserId`, `OldRole`, `NewRole`, `TicketId` và `Reason` ($\ge 15$ ký tự).

## 2. Ma trận Cho phép Thay đổi Vai trò (Role Transition Matrix)

| Vai trò Hiện tại (`OldRole`) | Vai trò Đề xuất (`NewRole`) | Quyền Thực thi (`Allowed Actor`) | Yêu cầu Kèm theo (`Requirements`) |
|---|---|---|---|
| `Learner` | `ContentCreator` | `ContentAdmin`, `SecurityAdmin` | Phê duyệt đơn |
| `Learner` | `SupportAgent` | `SecurityAdmin`, `SuperAdmin` | Có Ticket nhân sự |
| `Learner` / `ContentCreator` | `ContentAdmin` | `SecurityAdmin`, `SuperAdmin` | Có Ticket quản trị M11 |
| Bất kỳ | `SecurityAdmin` | `SuperAdmin` | Phê duyệt Kép (M01-T030) |
| Bất kỳ | `SuperAdmin` | CẤM QUA API NÀY | Bắt buộc qua M01-T030 |
| `ContentAdmin` / `SupportAgent` | `Learner` (Hạ cấp) | `SecurityAdmin`, `SuperAdmin` | Lý do hạ cấp $\ge 15$ char |

## 3. Quy trình Thay đổi Vai trò CSDL (Role Change Engine)

```
[SecurityAdmin Submits Role Change Request (TargetUserId, NewRole, TicketId, Reason)]
                                   |
                                   v
             [Check Self-Role Mutation Guard (Actor != Target)]
                                   |
                         +---------+---------+
                         | (Actor == Target) | (Actor != Target)
                         v                   v
                  [Reject 400 Self]  [Validate Role Matrix & Permissions]
                                             |
                                     +-------+-------+
                                     | (Unauthorized) | (Authorized)
                                     v                v
                               [Deny 403]    [Update User.Role in DB]
                                                      |
                                                      v
                                        [Increment SecurityEpoch += 1]
                                        - Purge active JWT claims in Redis
                                                      |
                                                      v
                                        [Record Audit Event ACT-M11-29]
                                                      |
                                                      v
                                        [Publish UserRoleChangedEvent]
```

## 4. Giao thức Xử lý Thay đổi Vai trò (RoleChangeService)

```csharp
public async Task<UserRoleDto> ChangeUserRoleAsync(string targetUserId, string newRole, string ticketId, string reason, string actorUserId, string actorRole)
{
    // 1. Check Self-Role Mutation Guard
    if (targetUserId == actorUserId)
    {
        throw new InvalidOperationException("SELF_ROLE_MUTATION_FORBIDDEN: Quản trị viên không được phép tự sửa vai trò của chính mình.");
    }

    // 2. Validate Authority Matrix
    if (actorRole != "SecurityAdmin" && actorRole != "SuperAdmin")
    {
        throw new UnauthorizedAccessException("ROLE_MUTATION_FORBIDDEN");
    }

    if (newRole == "SuperAdmin")
    {
        throw new InvalidOperationException("SUPER_ADMIN_PROMOTION_REQUIRES_DUAL_APPROVAL");
    }

    if (string.IsNullOrEmpty(reason) || reason.Length < 15)
    {
        throw new ArgumentException("ROLE_CHANGE_REASON_MIN_LENGTH_15");
    }

    var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == targetUserId);
    if (user == null) throw new InvalidOperationException("TARGET_USER_NOT_FOUND");

    string oldRole = user.Role;
    if (oldRole == newRole) return MapToDto(user); // Giữ nguyên nếu không đổi

    // 3. Thực thi Đổi Vai trò và Tăng SecurityEpoch
    user.Role = newRole;
    user.SecurityEpoch += 1; // Vô hiệu hóa mọi JWT Token cũ SLA <= 5s
    user.LastRoleChangedAtUtc = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // 4. Ghi vết Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-29", actorUserId, new {
        TargetUserId = targetUserId,
        OldRole = oldRole,
        NewRole = newRole,
        TicketId = ticketId,
        Reason = reason,
        SecurityEpoch = user.SecurityEpoch
    });

    // 5. Phát sự kiện tích hợp
    await _eventPublisher.PublishAsync(new UserRoleChangedIntegrationEvent {
        UserId = targetUserId,
        OldRole = oldRole,
        NewRole = newRole,
        ChangedAtUtc = user.LastRoleChangedAtUtc.Value
    });

    return MapToDto(user);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RC-G01` | Cấm quản trị viên tự thay đổi vai trò của chính tài khoản của mình (`Self-Role Mutation Guard`). |
| `RC-G02` | Thăng cấp lên `SuperAdmin` CẤM thực hiện qua API đổi vai trò thông thường (yêu cầu M01-T030). |
| `RC-G03` | Thay đổi vai trò bắt buộc đi kèm lý do `reason >= 15` ký tự và mã `ticketId` hợp lệ. |
| `RC-G04` | Đổi vai trò thành công tự động tăng `SecurityEpoch` $+1$, vô hiệu hóa phiên JWT trong SLA $\le 5$ giây. |
| `RC-G05` | Người dùng bị đổi vai trò phải nộp bài đăng nhập lại để nhận `role` claim mới trong JWT token. |
| `RC-G06` | 100% thao tác thay đổi vai trò ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-29`). |
| `RC-G07` | Đổi vai trò tự động phát sự kiện `UserRoleChangedIntegrationEvent` đồng bộ sang M11 và M02. |
| `RC-G08` | Phân quyền thay đổi vai trò chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin` (REL-02). |
| `RC-G09` | SLA thực thi API đổi vai trò và vô hiệu hóa session $< 30\text{ms}$. |
| `RC-G10` | 100% các test case tự kiểm RC29-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC29-01` | `SecurityAdmin` thăng cấp User B từ `Learner` lên `ContentCreator` | Thăng cấp thành công, `SecurityEpoch += 1`, ghi log `ACT-M11-29` |
| `RC29-02` | `SecurityAdmin` tự gửi request đổi vai trò của chính mình | Reject 400 `SELF_ROLE_MUTATION_FORBIDDEN` |
| `RC29-03` | Thử thăng cấp User B lên `SuperAdmin` qua API đổi vai trò | Reject 400 `SUPER_ADMIN_PROMOTION_REQUIRES_DUAL_APPROVAL` |
| `RC29-04` | Thử đổi vai trò với lý do ngắn 10 ký tự ($< 15$) | Reject 400 `ROLE_CHANGE_REASON_MIN_LENGTH_15` |
| `RC29-05` | User B đang dùng JWT Token cũ với vai trò `Learner` gọi API biên tập | Deny 401 Unauthorized do `SecurityEpoch` đã bị tăng |
| `RC29-06` | User B đăng nhập lại nhận JWT mới có vai trò `ContentCreator` | Đăng nhập thành công, gọi API biên tập thành công |
| `RC29-07` | `SecurityAdmin` hạ cấp User B từ `ContentAdmin` về `Learner` | Hạ cấp thành công, vô hiệu hóa mọi quyền admin cũ |
| `RC29-08` | User vai trò `ContentAdmin` thử gọi API đổi vai trò người khác | Deny 403 Forbidden (REL-02) |
| `RC29-09` | Tra cứu vết Audit Log M11 sau khi đổi vai trò | Ghi nhận Audit Event `ACT-M11-29` với đính kèm `OldRole` & `NewRole` |
| `RC29-10` | Kiểm tra sự kiện tích hợp phát ra sau khi đổi vai trò | Module M11/M02 nhận `UserRoleChangedIntegrationEvent` |
| `RC29-11` | Thử truyền mã `ticketId` rỗng khi đổi vai trò | Reject 400 `MISSING_TICKET_ID` |
| `RC29-12` | Tải đồng thời 50 request kiểm tra quyền vai trò của User B | Response latency p95 $< 25\text{ms}$ |
| `RC29-13` | User chưa đăng nhập thử gọi API đổi vai trò | Deny 401 Unauthorized |
| `RC29-14` | Đổi vai trò cho tài khoản đang ở trạng thái `Locked` | Cập nhật vai trò trong DB, duy trì trạng thái `Locked` |
| `RC29-15` | Đổi vai trò trùng với vai trò hiện tại của người dùng | Tra về 200 OK giữ nguyên dữ liệu không tăng SecurityEpoch |
| `RC29-16` | Kiểm tra mốc `LastRoleChangedAtUtc` sau khi đổi | Trả về thời gian hiện tại UTC chuẩn xác |
| `RC29-17` | Phân tích tham chiếu vai trò trong ma trận RBAC M01-T028 | Quét claim roles trong CSDL (T020) |
| `RC29-18` | Thao tác đổi vai trò bị gián đoạn do lỗi DB | Rollback transaction, giữ nguyên vai trò cũ và SecurityEpoch |
| `RC29-19` | `SuperAdmin` thực hiện đổi vai trò cho 1 `SecurityAdmin` khác | Thực hiện đổi vai trò thành công |
| `RC29-20` | Kiểm thử hoàn tất luồng chuẩn hóa thay đổi vai trò M01-ROLE-CHANGE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-RC-I01` | Entity `User.cs` hiện chưa lưu mốc `LastRoleChangedAtUtc` | Thiếu vết thời gian đổi vai trò gần nhất | M01-T049 (Source task) |
| `M01-RC-I02` | Thiếu kiểm tra `Self-Role Mutation Guard` trong `UserService.cs` | Quản trị viên có thể tự đổi vai trò của mình | M01-T049; REL-02 |
| `M01-RC-I03` | Chưa tự động tăng `SecurityEpoch` $+1$ khi đổi vai trò | Người dùng tiếp tục dùng JWT cũ chứa vai trò cũ trong 24h | M01-T049; M01-T016 |
| `M01-RC-I04` | Thiếu validation độ dài lý do đổi vai trò tối thiểu 15 ký tự | Admin có thể nhập lý do quá ngắn gây khó đối soát | M01-T049 |
| `M01-RC-I05` | Chưa phát sự kiện `UserRoleChangedIntegrationEvent` sang M11 | Sổ Kiểm toán M11 không nhận được thông tin thay đổi quyền | M01-T049; M11-T031 |

- `M01-RC-F01`: Thêm `LastRoleChangedAtUtc` vào `User.cs` (tiếp nhận: M01-T049).
- `M01-RC-F02`: Triển khai `RoleChangeService` với Self-Role Mutation Guard & SecurityEpoch $+1$ (tiếp nhận: M01-T049; REL-02).
- `M01-RC-F03`: Tích hợp phát `UserRoleChangedIntegrationEvent` sang M11 (tiếp nhận: M01-T049; M11-T031).
- `M01-RC-F04`: Thiết lập bộ kiểm thử tự động RC-G01–G10 và RC29-01–20 (tiếp nhận: M01 tasks).
- `M01-RC-F05`: Thu thập bằng chứng runtime cho luồng thay đổi vai trò M01 (tiếp nhận: M01 tasks; A-G02).

## 8. Tự kiểm M01-T029

- Đã thiết kế hoàn chỉnh `M01-ROLE-CHANGE-1.0` với Ma trận Thay đổi Vai trò 6 Cấp.
- Đã chốt Ràng buộc Quy tắc Cấm Tự Nâng/Hạ Quyền (`Self-Role Mutation Guard`).
- Đã chốt Tự động Vô hiệu hóa Phiên JWT (`SecurityEpoch += 1`) SLA $\le 5\text{s}$.
- Đã lồng ghép Yêu cầu Lý do $\ge 15$ char, Mã `ticketId` và Lưu vết Audit Log M11 (`ACT-M11-29`).
- Đã xác lập 10 Regression Gates (`RC-G01`–`RC-G10`) và 20 Test Cases tự kiểm (`RC29-01`–`RC29-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chuẩn hóa thay đổi vai trò M01-T029 | WSA-7K2 |
