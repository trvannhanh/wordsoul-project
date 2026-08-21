# Thiết kế vòng đời vụ việc hỗ trợ M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-SUPPORT-TICKET-LIFECYCLE-1.0` |
| Task | M11-T029 |
| Đầu vào | M11-SAFE-USER-SEARCH-1.0 (D-074), M11-SUPPORT-TIMELINE-BUILDER-1.0 (D-107), REL-07 |
| Phạm vi | Máy trạng thái Vòng đời Vụ việc Hỗ trợ 6 bước (`Support Ticket Lifecycle State Machine`), quy trình cấp/thu hồi quyền truy cập dữ liệu người dùng thời gian thực, cam kết SLA phản hồi và tự động đóng ticket |
| Tự kiểm | A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quản lý Vòng đời Vụ việc Hỗ trợ (`Support Ticket Lifecycle Engine`) thuộc M11, xác lập máy trạng thái chuẩn cho các ticket yêu cầu hỗ trợ từ người học, điều phối quyền hạn tra cứu dữ liệu cá nhân của chuyên viên (SupportAgent) và đảm bảo nguyên tắc thu hồi quyền truy cập ngay khi vụ việc kết thúc (REL-07).

- **Máy Trạng thái Vụ việc Hỗ trợ 6 Bước (`6-State Ticket Lifecycle Invariant`)**:
  - `NEW`: Ticket được tạo bởi Người học hoặc Chuyên viên Hỗ trợ. Chưa gán agent.
  - `ASSIGNED`: Ticket được gán cho một SupportAgent cụ thể.
  - `IN_PROGRESS`: Agent đang tích cực xử lý. MỞ QUYỀN tra cứu Dòng thời gian Hỗ trợ M11-T028 & Tìm kiếm an toàn M11-T027 (D-106 / D-107).
  - `ESCALATED`: Chuyển cấp lên SecurityAdmin hoặc ContentAdmin khi vượt thẩm quyền.
  - `RESOLVED`: Vụ việc đã xử lý xong kèm Tóm tắt giải pháp (`ResolutionSummary` $\ge 15$ char).
  - `CLOSED`: Ticket đóng hoàn toàn. TỰ ĐỘNG THU HỒI TỨC THÌ mọi quyền tra cứu lịch sử danh tính và PII của SupportAgent đối với tài khoản target.
- **Ràng buộc Thu hồi Quyền Tức thì SLA $\le 1\text{s}$ (`Instant Permission Revocation on Ticket Close`)**: Ngay khi ticket chuyển trạng thái `CLOSED` hoặc `RESOLVED`, cờ `HasActiveTicket` trong CSDL Redis bị hủy bỏ SLA $\le 1$ giây. SupportAgent gọi API tra cứu lịch sử sau mốc này sẽ bị chặn HTTP 403 Forbidden.
- **Cam kết SLA Phản hồi và Xử lý (`Ticket SLA Targets`)**: SLA Phản hồi đầu tiên (`First Response SLA`) $\le 4$ giờ. SLA Giải quyết vụ việc (`Resolution SLA`) $\le 24$ giờ. Tự động đóng ticket (`Auto-Close`) sau 72 giờ ở trạng thái `RESOLVED` nếu không có phản hồi mới từ người học.
- **Nhật ký Đánh dấu Chuyển Trạng thái M11 (`Ticket Transition Audit Trail`)**: $100\%$ các lần chuyển trạng thái ticket bắt buộc được ghi vết bất biến `ACT-M11-29` trong Sổ Kiểm toán M11, đính kèm `ActorUserId`, `TicketId`, `OldState`, `NewState` và `Reason`.

## 2. Ma trận Chuyển Trạng thái Vụ việc Hỗ trợ (State Transition Matrix)

| Trạng thái Hiện tại (`OldState`) | Trạng thái Mới (`NewState`) | Điều kiện Chuyển (`Transition Triggers`) | Quyền Tra cứu PII / Timeline | Yêu cầu Kèm theo |
|---|---|---|---|---|
| `NEW` | `ASSIGNED` | Gán cho SupportAgent | **TẮT** (Chưa mở) | Gán `AssignedAgentId` |
| `ASSIGNED` | `IN_PROGRESS` | SupportAgent bấm bắt đầu xử lý | **BẬT** (Active Ticket) | Mở cờ `HasActiveTicket` |
| `IN_PROGRESS` | `ESCALATED` | Cần quyền SecurityAdmin/ContentAdmin | **BẬT** (Chuyển cấp) | Ghi rõ `EscalationReason` $\ge 15$ char |
| `IN_PROGRESS` / `ESCALATED` | `RESOLVED` | Xử lý xong sự cố | **TẮT** (Tạm khóa) | Nhập `ResolutionSummary` $\ge 15$ char |
| `RESOLVED` | `CLOSED` | Khách bần đồng ý / Tự động sau 72h | **TẮT** (Xóa hoàn toàn) | Hủy key Redis active ticket SLA $\le 1\text{s}$ |
| `RESOLVED` | `IN_PROGRESS` | Khách hàng phản hồi mở lại | **BẬT** (Tái mở) | Người học gửi phản hồi mới |

## 3. Máy Trạng thái Vòng đời Ticket Hỗ trợ (State Machine)

```
        [NEW State] --(Assign Agent)--> [ASSIGNED State]
                                              |
                                              v (Start Work)
                                    +-> [IN_PROGRESS State] <---+ (Re-open)
                                    |         |                 |
                (Escalate Special)  |         +---(Resolve)-----+
                                    v                           |
                            [ESCALATED State]                   |
                                    |                           v
                                    +-------(Resolve)-----> [RESOLVED State]
                                                                |
                                                                v (Close / 72h Timeout)
                                                            [CLOSED State]
                                                            (Revoke PII Access SLA <= 1s)
```

## 4. Giao thức Thực thi Chuyển Trạng thái CSDL (SupportTicketLifecycleService)

```csharp
public async Task<SupportTicketDto> TransitionTicketStateAsync(
    string ticketId, 
    SupportTicketState newState, 
    string actorUserId, 
    string reason)
{
    var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
    if (ticket == null) throw new InvalidOperationException("SUPPORT_TICKET_NOT_FOUND");

    var oldState = ticket.State;
    if (!IsValidTransition(oldState, newState))
    {
        throw new InvalidOperationException($"INVALID_STATE_TRANSITION: Không thể chuyển từ {oldState} sang {newState}.");
    }

    if (newState == SupportTicketState.RESOLVED && (string.IsNullOrEmpty(reason) || reason.Length < 15))
    {
        throw new ArgumentException("RESOLUTION_SUMMARY_MIN_LENGTH_15: Bắt buộc nhập tóm tắt giải pháp tối thiểu 15 ký tự.");
    }

    // 1. Update DB State
    ticket.State = newState;
    ticket.UpdatedAtUtc = DateTime.UtcNow;
    if (newState == SupportTicketState.RESOLVED) ticket.ResolvedAtUtc = DateTime.UtcNow;
    if (newState == SupportTicketState.CLOSED) ticket.ClosedAtUtc = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // 2. Manage Redis Active Ticket Access Flag SLA <= 1s
    string activeTicketKey = $"wordsoul:active_ticket:{ticket.TargetUserId}:{ticket.AssignedAgentId}";
    if (newState == SupportTicketState.IN_PROGRESS || newState == SupportTicketState.ESCALATED)
    {
        await _redisDb.StringSetAsync(activeTicketKey, ticketId, TimeSpan.FromHours(24));
    }
    else if (newState == SupportTicketState.RESOLVED || newState == SupportTicketState.CLOSED)
    {
        await _redisDb.KeyDeleteAsync(activeTicketKey); // Revoke PII access immediately
    }

    // 3. Record Audit Log M11
    await _auditLog.RecordEventAsync("ACT-M11-29", actorUserId, new {
        TicketId = ticketId,
        TargetUserId = ticket.TargetUserId,
        OldState = oldState.ToString(),
        NewState = newState.ToString(),
        Reason = reason
    });

    return MapToDto(ticket);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `TL-G01` | Máy trạng thái ticket hỗ trợ tuân thủ nghiêm ngặt 6 trạng thái (`NEW` $\to$ `CLOSED`). |
| `TL-G02` | Quyền tra cứu Dòng thời gian / PII chỉ mở khi ticket ở trạng thái `IN_PROGRESS` hoặc `ESCALATED`. |
| `TL-G03` | Chuyển ticket sang `CLOSED` hoặc `RESOLVED` tự động thu hồi cờ active ticket trong Redis SLA $\le 1$ giây. |
| `TL-G04` | Chuyển ticket sang `RESOLVED` BẮT BUỘC nhập `ResolutionSummary` $\ge 15$ ký tự. |
| `TL-G05` | Ticket ở trạng thái `RESOLVED` tự động chuyển `CLOSED` sau 72 giờ không có phản hồi mới từ người dùng. |
| `TL-G06` | SLA phản hồi lần đầu (`First Response SLA`) của ticket không được vượt quá 4 giờ. |
| `TL-G07` | 100% các lần chuyển trạng thái ticket được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-29`). |
| `TL-G08` | Phân quyền chuyển trạng thái ticket chỉ dành riêng cho `SupportAgent`, `SecurityAdmin` và `SuperAdmin`. |
| `TL-G09` | SLA thực thi API chuyển trạng thái ticket $< 25\text{ms}$. |
| `TL-G10` | 100% các test case tự kiểm TL29-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TL29-01` | SupportAgent chuyển ticket từ `ASSIGNED` sang `IN_PROGRESS` | Chuyển thành công, cờ Redis active ticket được bật |
| `TL29-02` | SupportAgent xem Dòng thời gian M11-T028 khi ticket ở `IN_PROGRESS` | Xem Dòng thời gian thành công |
| `TL29-03` | SupportAgent chuyển ticket từ `IN_PROGRESS` sang `RESOLVED` | Chuyển thành công, cờ Redis active ticket bị xóa SLA $< 1\text{s}$ |
| `TL29-04` | SupportAgent thử xem Dòng thời gian M11-T028 ngay sau khi ticket sang `RESOLVED` | Reject 403 `INVALID_OR_CLOSED_SUPPORT_TICKET` |
| `TL29-05` | Thử chuyển ticket sang `RESOLVED` với tóm tắt giải pháp 10 ký tự ($< 15$) | Reject 400 `RESOLUTION_SUMMARY_MIN_LENGTH_15` |
| `TL29-06` | Ticket ở `RESOLVED` quá 72 giờ không có phản hồi | Auto-Close Job chuyển ticket sang `CLOSED` tự động |
| `TL29-07` | Người học phản hồi ý kiến mới khi ticket đang ở `RESOLVED` | Ticket tự động quay lại trạng thái `IN_PROGRESS` |
| `TL29-08` | Tra cứu vết Audit Log M11 sau khi chuyển trạng thái ticket | Ghi nhận Audit Event `ACT-M11-29` đính kèm OldState & NewState |
| `TL29-09` | Thử nhảy cóc chuyển ticket từ `NEW` trực tiếp sang `RESOLVED` | Reject 400 `INVALID_STATE_TRANSITION` |
| `TL29-10` | Chuyển ticket sang `ESCALATED` cho SecurityAdmin | Ticket ghi nhận lý do Escalation và thông báo cho SecurityAdmin |
| `TL29-11` | Kiểm tra thời gian phản hồi chuyển trạng thái ticket | Response latency p95 $< 20\text{ms}$ |
| `TL29-12` | Tải đồng thời 100 request chuyển trạng thái ticket | 100% state transitions nhất quán không race condition |
| `TL29-13` | User không phải SupportAgent/Admin chuyển trạng thái ticket | Deny 403 Forbidden |
| `TL29-14` | User chưa đăng nhập gọi API chuyển trạng thái ticket | Deny 401 Unauthorized |
| `TL29-15` | Chuyển ticket từ `CLOSED` quay lại `IN_PROGRESS` trực tiếp | Reject 400 `CANNOT_REOPEN_CLOSED_TICKET` (Phải tạo ticket mới) |
| `TL29-16` | Kiểm tra mốc `FirstResponseAtUtc` sau lần phản hồi đầu | Mốc thời gian lưu chuẩn xác $< 4$ giờ SLA |
| `TL29-17` | Phân tích tham chiếu các trạng thái ticket trong CSDL | Quét schema `M11_SupportTickets` (T020) |
| `TL29-18` | Thao tác xóa cờ Redis active ticket bị ngắt kết nối giữa chừng | Lock fallback dọn dẹp cờ active ticket trong job nền |
| `TL29-19` | Hủy bỏ ticket bởi người học (Learner Cancel) | Ticket chuyển sang trạng thái `CANCELLED` |
| `TL29-20` | Kiểm thử hoàn tất luồng thiết kế vòng đời vụ việc hỗ trợ M11-SUPPORT-TICKET-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-TL-I01` | M11 hiện tại chưa có bảng CSDL `M11_SupportTickets` đầy đủ | Chưa quản lý được máy trạng thái 6 bước vụ việc hỗ trợ | M11-T049 (Source task) |
| `M11-TL-I02` | Chưa có bộ `AutoCloseTicketJob` tự động đóng ticket sau 72h | Ticket ở trạng thái RESOLVED bị đồn tích tụ không đóng | M11-T049; M11-T038 |
| `M11-TL-I03` | Thiếu cờ Redis `active_ticket` thu hồi quyền SLA $\le 1\text{s}$ | SupportAgent vẫn có thể xem PII người dùng sau khi giải quyết vụ việc | M11-TL-F03; REL-07 |
| `M11-TL-I04` | Thiếu validation tóm tắt giải pháp tối thiểu 15 ký tự | SupportAgent nhập nội dung qua loa gây khó đối soát | M11-T049 |
| `M11-TL-I05` | Chưa đo đạc chỉ số First Response SLA và Resolution SLA | Không phát hiện được các vụ việc hỗ trợ bị ngâm lâu | M11-T049; M11-T022 |

- `M11-TL-F01`: Triển khai `SupportTicketLifecycleService` với Máy Trạng thái 6 Bước (tiếp nhận: M11-T049).
- `M11-TL-F02`: Tích hợp Bắt buộc `Redis Active Ticket Flag` SLA $\le 1\text{s}$ (tiếp nhận: M11-T049; REL-07).
- `M11-TL-F03`: Triển khai `AutoCloseTicketJob` đóng ticket sau 72h (tiếp nhận: M11-T049; M11-T038).
- `M11-TL-F04`: Thiết lập bộ kiểm thử tự động TL-G01–G10 và TL29-01–20 (tiếp nhận: M11 tasks).
- `M11-TL-F05`: Thu thập bằng chứng runtime cho luồng vòng đời vụ việc M11 (tiếp nhận: M11 tasks; A-G02).

## 8. Tự kiểm M11-T029

- Đã thiết kế hoàn chỉnh `M11-SUPPORT-TICKET-LIFECYCLE-1.0` với Máy Trạng thái Vụ việc Hỗ trợ 6 Bước.
- Đã chốt Ràng buộc Thu hồi Quyền Tức thì SLA $\le 1\text{s}$ trong Redis khi ticket `RESOLVED` / `CLOSED`.
- Đã chốt Cam kết SLA Phản hồi ($\le 4\text{h}$), SLA Giải quyết ($\le 24\text{h}$) và Tự động Đóng Ticket sau 72h.
- Đã lồng ghép Yêu cầu Tóm tắt Giải pháp $\ge 15$ char và Lưu vết Audit Log M11 (`ACT-M11-29`).
- Đã xác lập 10 Regression Gates (`TL-G01`–`TL-G10`) và 20 Test Cases tự kiểm (`TL29-01`–`TL29-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế vòng đời vụ việc hỗ trợ M11-T029 | WSA-7K2 |
