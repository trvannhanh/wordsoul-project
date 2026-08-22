# Chốt quyền và kiểm toán vận hành M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-NOTIFICATION-OPS-GOVERNANCE-1.0` |
| Task | M10-T044 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M10-INBOX-HIDE-DELETE-RETENTION-1.0 (M10-T019), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Quy định quyền phát thông báo hệ thống hàng loạt (`Broadcast Push Authorization`) và lưu log kiểm toán vận hành `NotificationOpsAuditLogs` |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa quy trình phân quyền và ghi log kiểm toán vận hành thông báo (`Notification Ops Governance`) trong M10.

- **Duyệt 2 Bước cho Lệnh Phát Thông báo Hàng loạt (`Broadcast Push Dual Approval Invariant`)**:
  - Lệnh gửi Push Broadcast toàn hệ thống (cho $> 1,000$ người học):
    - BẮT BUỘC có phê duyệt của Lead Admin (`Maker != Checker`).
    - Tuyệt đối CẤM Admin tự ý phát Push Broadcast trực tiếp mà không qua phê duyệt.
- **Vết Kiểm toán Vận hành Bất biến (`Notification Ops Audit Log Rule`)**: 100% thao tác thay đổi cấu hình template thông báo, gửi Broadcast hoặc can thiệp Inbox BẮT BUỘC lưu vết `NotificationOpsAuditLogs` trong M11.

## 2. Bảng Ma trận Phân quyền Vận hành Thông báo (Ops Governance Matrix)

| Thao tác Vận hành | Vai trò Cho phép | Yêu cầu Maker != Checker | Ghi log Kiểm toán M11 |
|---|---|---|---|
| `CreateNotifTemplate` | `NOTIF_AUTHOR` | Không | Bắt buộc |
| `ApproveNotifTemplate` | `LEAD_NOTIF_ADMIN` | **Bắt buộc** | Bắt buộc |
| `DispatchBroadcastPush` | `SUPER_ADMIN` | **Bắt buộc (>1k users)** | Bắt buộc |
| `PurgeInboxHistory` | `SYSTEM_CRON` | Không | Bắt buộc |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `OG-G01`: 100% lệnh phát Broadcast Push $> 1,000$ người dùng thiếu phê duyệt bị chặn với HTTP 403.
- `OG-G02`: Bảng `NotificationOpsAuditLogs` lưu vết $100\%$ thao tác thay đổi template thông báo.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `OG44-01` | Admin A tạo chiến dịch Broadcast Push cho 50,000 users và tự bấm phát | System từ chối, ném lỗi HTTP 403 `BROADCAST_REQUIRES_DUAL_APPROVAL`. |
| `OG44-02` | Lead Admin B phê duyệt chiến dịch của Admin A | Trạng thái chuyển `APPROVED`, hệ thống đưa chiến dịch vào hàng đợi phát broadcast. |
| `OG44-03` | Kiểm thử hoàn tất luồng M10-NOTIFICATION-OPS-GOVERNANCE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-OG-F01` | Áp dụng policy `[Authorize(Policy = "CanManageNotificationTemplates")]` trên AdminNotifController | Phục vụ bảo mật API quản trị thông báo | M10-T002 |

## 5. Tự kiểm M10-T044
- Đã hoàn thành đặc tả `M10-NOTIFICATION-OPS-GOVERNANCE-1.0`.
- Chốt cơ chế dual-approval cho Push Broadcast $> 1,000$ users và log kiểm toán vận hành bất biến.
- Ghi nhận 2 Regression Gates (`OG-G01`–`OG-G02`) và 3 Test Cases (`OG44-01`–`OG44-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chốt quyền và kiểm toán vận hành M10-T044 | WSA-7K2 |
