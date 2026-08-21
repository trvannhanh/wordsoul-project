# Xây dựng danh mục sự kiện danh tính M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-IDENTITY-EVENT-CATALOG-1.0` |
| Task | M01-T038 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M01-CROSS-MODULE-PII-MAP-1.0 (D-102), M11-AUDIT-EVENT-1.0 (D-054), M12-CONTRACT-1.0 (D-021) |
| Phạm vi | Danh mục 12 Sự kiện Danh tính Chuẩn hóa (`Identity Integration Event Catalog`), cấu trúc Envelope bất biến, cam kết At-Least-Once Delivery qua RabbitMQ/Outbox và quy tắc che mờ PII trong Event Payload |
| Tự kiểm | A-G01, A-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Danh mục Sự kiện Danh tính Chuẩn hóa (`Identity Integration Event Catalog Engine`) thuộc M01, chuẩn hóa toàn bộ các sự kiện phát sinh trong vòng đời tài khoản người dùng và truyền phát an toàn sang các module tiêu thụ (M02, M03, M06, M10, M11, M12).

- **Khung Envelope Sự kiện Bất biến M12 (`Mandatory Event Envelope Invariant`)**: $100\%$ sự kiện danh tính phát ra từ M01 BẮT BUỘC đóng gói theo cấu trúc Envelope chuẩn `IntegrationEventEnvelope<T>` (`EventId`, `EventType`, `ProducerModule = "M01"`, `TimestampUtc`, `AggregateId`, `SecurityEpoch`, `PayloadJson`).
- **Ràng buộc Mẫu thiết kế Transactional Outbox Pattern (`Outbox Pattern Invariant`)**: Phát sự kiện danh tính BẮT BUỘC lưu vào bảng `M01_OutboxEvents` trong cùng một CSDL Transaction với nghiệp vụ chính, bảo đảm cam kết `At-Least-Once Delivery` qua Message Broker (RabbitMQ / EventBus).
- **Ràng buộc Khóa Đẳng phản cho Consumer (`Consumer Idempotency Invariant`)**: Mọi Consumer nhận sự kiện M01 BẮT BUỘC kiểm tra `EventId` và `SecurityEpoch` trong Redis để từ chối xử lý các sự kiện phát lại hoặc đã cũ (`Idempotent Event Handler`).
- **Tuyệt đối Không Lộ PII_DIRECT trong Event Payload (`Zero Direct PII Egress Invariant`)**: Payload của sự kiện danh tính CHỈ chứa `UserId` (GUID), `SecurityEpoch`, `Role` và các thuộc tính trạng thái. CẤM chứa mật khẩu, Email thô hoặc SĐT thô trong payload công khai (REL-01).

## 2. Danh mục 12 Sự kiện Danh tính Chuẩn hóa (Identity Event Catalog)

| Tên Sự kiện (`EventType`) | Trigger Phát sinh | Module Tiêu thụ (`Consumers`) | Mục đích Xử lý |
|---|---|---|---|
| `UserRegisteredIntegrationEvent` | Người dùng đăng ký tài khoản mới thành công | M06, M10, M11 | Khởi tạo ví Exp/Gold M06, gửi email chào mừng M10 |
| `UserLoggedInIntegrationEvent` | Đăng nhập thành công | M11 | Ghi vết Sổ Kiểm toán M11, phân tích hành vi bất thường |
| `UserRoleChangedIntegrationEvent` | Đổi vai trò (M01-T029) | M02, M03, M11 | Cập nhật quyền biên tập M02/M03, vô hiệu phiên JWT |
| `UserAccountLockedIntegrationEvent` | Khóa tài khoản (M01-T031) | M10, M11 | Gửi thông báo PUSH/Email M10, ngắt kết nối lập tức |
| `UserAccountUnlockedIntegrationEvent` | Mở khóa tài khoản | M10, M11 | Gửi thông báo khôi phục tài khoản M10 |
| `SecurityEpochIncrementedIntegrationEvent` | Tăng SecurityEpoch | M01, M12 | Xóa Redis Session cache toàn hệ thống SLA $\le 5\text{s}$ |
| `PushDeviceRegisteredIntegrationEvent` | Đăng ký thiết bị nhận PUSH (M01-T025-A) | M10 | Đăng ký FCM Device Token |
| `PushDeviceRevokedIntegrationEvent` | Thu hồi thiết bị nhận PUSH (M01-T027-A) | M10 | Ngắt gửi PUSH cho thiết bị đã thu hồi (D-091) |
| `SecurityCredentialsChangedIntegrationEvent` | Đổi Password / Email | M01, M11 | Vô hiệu hóa phiên cũ, ghi log đổi mật khẩu |
| `UserDataExportRequestedIntegrationEvent` | Yêu cầu xuất dữ liệu (M01-T034) | M11 | Tạo ticket xử lý xuất dữ liệu GDPR |
| `UserAccountDeletionRequestedIntegrationEvent` | Yêu cầu xóa tài khoản (M01-T035) | M02–M12 | Kích hoạt luồng ẩn danh hóa / xóa dữ liệu liên module |
| `UserAccountAnonymizedIntegrationEvent` | Hoàn tất ẩn danh hóa (M01-T036) | M11 | Lưu vết hoàn thành nghĩa vụ xóa dữ liệu GDPR |

## 3. Cấu trúc JSON Envelope Chuẩn hóa (IntegrationEventEnvelope Schema)

```json
{
  "eventId": "EVT-M01-20260821-99401284",
  "eventType": "UserRoleChangedIntegrationEvent",
  "producerModule": "M01",
  "aggregateId": "USR-10024",
  "securityEpoch": 4,
  "timestampUtc": "2026-08-21T09:30:00Z",
  "payload": {
    "userId": "USR-10024",
    "oldRole": "Learner",
    "newRole": "ContentCreator",
    "actorUserId": "USR-SEC-001",
    "ticketId": "TCK-ROLE-2026-001"
  }
}
```

## 4. Giao thức Thực thi Phát Sự kiện CSDL Outbox (IdentityEventPublisherService)

```csharp
public async Task PublishIdentityEventAsync<TPayload>(string eventType, string userId, int securityEpoch, TPayload payload)
{
    var envelope = new IntegrationEventEnvelope<TPayload> {
        EventId = $"EVT-M01-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}[..8]",
        EventType = eventType,
        ProducerModule = "M01",
        AggregateId = userId,
        SecurityEpoch = securityEpoch,
        TimestampUtc = DateTime.UtcNow,
        Payload = payload
    };

    // 1. Save to Outbox Table in DB Transaction
    var outboxEntry = new OutboxEvent {
        EventId = envelope.EventId,
        EventType = envelope.EventType,
        PayloadJson = JsonSerializer.Serialize(envelope),
        CreatedAtUtc = DateTime.UtcNow,
        IsProcessed = false
    };

    _db.OutboxEvents.Add(outboxEntry);
    await _db.SaveChangesAsync();

    // 2. Outbox Background Processor reads and publishes to MessageBus (RabbitMQ)
    await _outboxSignal.NotifyNewEventAsync();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IE-G01` | 100% sự kiện danh tính từ M01 đóng gói đúng cấu trúc `IntegrationEventEnvelope<T>`. |
| `IE-G02` | Mọi sự kiện phát ra BẮT BUỘC lưu qua bảng Outbox CSDL cùng transaction nghiệp vụ (`Outbox Pattern`). |
| `IE-G03` | Event Payload tuyệt đối CẤM chứa mật khẩu thô, Email thô hoặc SĐT thô của người dùng (REL-01). |
| `IE-G04` | Cấu trúc `EventId` là mã băm ngẫu nhiên duy nhất, đảm bảo tính đẳng phản cho các Consumer. |
| `IE-G05` | SLA lưu sự kiện vào Outbox CSDL trong cùng transaction nghiệp vụ $< 5\text{ms}$. |
| `IE-G06` | SLA Background Worker đẩy sự kiện từ Outbox lên MessageBus (RabbitMQ) $< 500\text{ms}$. |
| `IE-G07` | 100% các lần đẩy sự kiện Outbox thất bại phải được retry tự động với Exponential Backoff. |
| `IE-G08` | Phân quyền truy cập cấu hình Outbox Event Worker chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `IE-G09` | Consumer nhận sự kiện cũ hơn `SecurityEpoch` hiện tại trong Redis bắt buộc bỏ qua an toàn. |
| `IE-G10` | 100% các test case tự kiểm IE38-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IE38-01` | Đổi vai trò người dùng M01-T029 | Phát sự kiện `UserRoleChangedIntegrationEvent` vào Outbox |
| `IE38-02` | Khóa tài khoản người dùng M01-T031 | Phát sự kiện `UserAccountLockedIntegrationEvent` vào Outbox |
| `IE38-03` | Quét payload sự kiện `UserRegisteredIntegrationEvent` | Payload chứa `UserId` GUID, 0 chứa mật khẩu thô |
| `IE38-04` | Consumer Module M10 nhận sự kiện `PushDeviceRevokedIntegrationEvent` | M10 ngắt gửi PUSH cho thiết bị tương ứng |
| `IE38-05` | Gián đoạn kết nối RabbitMQ khi Outbox Worker đang chạy | Sự kiện giữ lại trong `M01_OutboxEvents` cho đến khi mạng khôi phục |
| `IE38-06` | Consumer M06 nhận 2 lần trùng `EventId` từ RabbitMQ | Lần 2 phát hiện `EventId` trong Redis $\to$ Bỏ qua an toàn |
| `IE38-07` | Thu hồi thiết bị nhận PUSH (M01-T027-A) | Phát sự kiện `PushDeviceRevokedIntegrationEvent` sang M10 (D-091) |
| `IE38-08` | Tra cứu vết Audit Log M11 sau khi phát sự kiện danh tính | Ghi nhận Audit Event `ACT-M11-38` đính kèm EventId |
| `IE38-09` | Kiểm tra thời gian ghi Outbox CSDL khi đổi vai trò | Transaction latency $< 4\text{ms}$ |
| `IE38-10` | Đổi mật khẩu người dùng M01-T020 | Phát sự kiện `SecurityCredentialsChangedIntegrationEvent` |
| `IE38-11` | Yêu cầu xóa tài khoản người dùng M01-T035 | Phát sự kiện `UserAccountDeletionRequestedIntegrationEvent` |
| `IE38-12` | Tải đồng thời 1000 sự kiện danh tính đẩy vào Outbox | 100% sự kiện được xử lý tuần tự không mất mát dữ liệu |
| `IE38-13` | User không phải Admin thử can thiệpOutbox Worker | Deny 403 Forbidden |
| `IE38-14` | User chưa đăng nhập gọi API phát sự kiện thủ công | Deny 401 Unauthorized |
| `IE38-15` | Kiểm tra cờ `IsProcessed = true` trong bảng Outbox sau khi đẩy | Cập nhật cờ thành công cùng thời gian `ProcessedAtUtc` |
| `IE38-16` | Dọn dẹp các Outbox Event đã xử lý quá 7 ngày | Outbox Cleanup Job tự động xóa các record cũ |
| `IE38-17` | Phân tích tham chiếu danh sách Outbox Events trong CSDL | Quét schema `M01_OutboxEvents` (T020) |
| `IE38-18` | Thao tác ghi Outbox bị gián đoạn do lỗi DB | Rollback toàn bộ transaction nghiệp vụ chính |
| `IE38-19` | Phát sự kiện tăng `SecurityEpoch` | EventEnvelope chứa giá trị `SecurityEpoch` mới chính xác |
| `IE38-20` | Kiểm thử hoàn tất luồng danh mục sự kiện danh tính M01-IDENTITY-EVENT-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-IE-I01` | Một số sự kiện danh tính hiện tại phát trực tiếp qua In-Memory EventBus | Nguy cơ mất sự kiện khi Server bị crash đột ngột trước khi phát | M01-T049 (Source task) |
| `M01-IE-I02` | Bảng `M01_OutboxEvents` chưa được khởi tạo trong CSDL Migration | Chưa hỗ trợ Transactional Outbox Pattern cho M01 | M01-T049; M12-T004 |
| `M01-IE-I03` | Thiếu cờ `SecurityEpoch` trong một số payload sự kiện cũ | Consumer không nhận biết được thứ tự ưu tiên phiên làm việc | M01-T049; M01-T016 |
| `M01-IE-I04` | Thiếu bộ dọn dẹp `OutboxCleanupJob` định kỳ | Bảng Outbox bị phình to làm chậm truy vấn CSDL | M01-T049; M11-T038 |
| `M01-IE-I05` | Chưa có bộ kiểm thử Idempotency cho các Event Consumer | Risk xử lý trùng lặp giao dịch thưởng Exp/Gold M06 | M01-T049; M06 tasks |

- `M01-IE-F01`: Triển khai `IdentityEventPublisherService` với Transactional Outbox (tiếp nhận: M01-T049).
- `M01-IE-F02`: Khởi tạo CSDL Migration cho bảng `M01_OutboxEvents` (tiếp nhận: M01-T049; M12-T004).
- `M01-IE-F03`: Chuẩn hóa 100% Event Envelope schema kèm `SecurityEpoch` (tiếp nhận: M01-T049).
- `M01-IE-F04`: Thiết lập bộ kiểm thử tự động IE-G01–G10 và IE38-01–20 (tiếp nhận: M01 tasks).
- `M01-IE-F05`: Thu thập bằng chứng runtime cho luồng danh mục sự kiện M01 (tiếp nhận: M01 tasks; A-G01/A-G02).

## 8. Tự kiểm M01-T038

- Đã thiết kế hoàn chỉnh `M01-IDENTITY-EVENT-CATALOG-1.0` với Danh mục 12 Sự kiện Danh tính Chuẩn hóa.
- Đã chốt Ràng buộc Khung Envelope Sự kiện Bất biến M12 và Transactional Outbox Pattern (`At-Least-Once`).
- Đã chốt Ràng buộc Khóa Đẳng phản cho Consumer và 0 Lộ PII_DIRECT trong Event Payload.
- Đã lồng ghép Tự động Dọn dẹp Outbox Events 7 ngày và Lưu vết Audit Log M11 (`ACT-M11-38`).
- Đã xác lập 10 Regression Gates (`IE-G01`–`IE-G10`) và 20 Test Cases tự kiểm (`IE38-01`–`IE38-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả xây dựng danh mục sự kiện danh tính M01-T038 | WSA-7K2 |
