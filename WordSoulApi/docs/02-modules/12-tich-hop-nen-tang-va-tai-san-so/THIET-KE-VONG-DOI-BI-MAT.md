# Thiết kế vòng đời bí mật M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-SECRET-LIFECYCLE-1.0` |
| Task | M12-T041 |
| Đầu vào | M12-SECRET-INVENTORY-1.0 (D-069), M11-PERM-1.0 (D-035), REL-03 |
| Phạm vi | Máy trạng thái Vòng đời Bí mật 5 bước (`Secret Lifecycle State Machine`), cơ chế xoay khóa 2 phiên bản (Dual-Key Grace Period 7 ngày), quy trình thu hồi khẩn cấp SLA $\le 5$ phút và lưu vết kiểm toán |
| Tự kiểm | A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quản lý Vòng đời Bí mật (`Secret Lifecycle Protocol`) thuộc M12, xác lập quy trình khởi tạo, cập nhật, xoay khóa định kỳ (Rotation) và thu hồi khẩn cấp (Emergency Revocation) cho toàn bộ các bí mật hệ thống S0–S3 (JWT Signing Keys, DB Passwords, API Keys, Master Encryption Keys) mà không gây ngắt kết nối dịch vụ (Zero-Downtime Secret Rollover REL-03).

- **Máy Trạng thái Vòng đời Bí mật 5 Bước (`5-State Secret Lifecycle Invariant`)**:
  - `DRAFT`: Bí mật mới vừa được khởi tạo / staging trong Vault, chưa kích hoạt.
  - `ACTIVE`: Khóa chính hiện tại được ứng dụng sử dụng để ký / mã hóa / xác thực.
  - `ROTATING`: Trạng thái xoay khóa 2 phiên bản (Dual-Key Grace Period 7 ngày). Khóa mới dùng để ký mới, Khóa cũ vẫn được chấp nhận để giải mã / xác thực các token hiện hành.
  - `DEPRECATED`: Khóa cũ ngừng dùng để ký mới, chỉ giữ lại trong bộ nhớ tạm để giải mã dữ liệu lịch sử.
  - `REVOKED`: Khóa bị vô hiệu hóa hoàn toàn / tiêu hủy khỏi bộ nhớ SLA $\le 5$ phút khi có dấu hiệu bị rò rỉ.
- **Ràng buộc Xoay Khóa Không Gián đoạn Services (`Zero-Downtime Dual-Key Grace Period`)**: Khi kích hoạt xoay khóa (`ROTATING`), hệ thống BẮT BUỘC duy trì đồng thời 2 phiên bản (Primary Version & Secondary Version) trong cửa sổ gối đầu 7 ngày. $100\%$ các luồng xác thực JWT hoặc giải mã CSDL phải chấp nhận cả 2 khóa để đảm bảo không làm đứt gãy phiên của người dùng (REL-03).
- **Thu hồi Khẩn cấp khi Rò rỉ SLA $\le 5$ Phút (`Emergency Revocation SLA`)**: Khi cờ nghi ngờ rò rỉ bí mật được bật bởi `SecurityAdmin`, hệ thống TỰ ĐỘNG tiêu hủy phiên bản khóa tương ứng khỏi bộ nhớ RAM của tất cả các node dịch vụ trong thời gian $\le 5$ phút, đồng thời kích hoạt tăng `SecurityEpoch` M01 để ngắt sạch phiên JWT cũ (D-028).
- **Nhật ký Đánh dấu Vòng đời Bí mật M11 (`Secret Lifecycle Audit Trail`)**: $100\%$ các thay đổi trạng thái bí mật bắt buộc được ghi vết bất biến `ACT-M11-41-SEC` trong Sổ Kiểm toán M11, bao gồm `SecretId`, `OldVersion`, `NewVersion`, `ActorUserId`, `TransitionState` và `Reason`.

## 2. Ma trận Chuyển Trạng thái Vòng đời Bí mật (State Transition Matrix)

| Trạng thái Hiện tại (`OldState`) | Trạng thái Mới (`NewState`) | Kịch bản Kích hoạt (`Triggers`) | Hành vi Hệ thống (`System Behavior`) | Thẩm quyền Phê duyệt |
|---|---|---|---|---|
| `DRAFT` | `ACTIVE` | Khởi tạo bí mật mới | Kích hoạt khóa chính duy nhất | SecurityAdmin / System |
| `ACTIVE` | `ROTATING` | Đã đến hạn Xoay khóa (90d/365d) | Nạp khóa mới, giữ khóa cũ trong Dual-Key Pool (7 ngày) | SecurityAdmin / Auto Job |
| `ROTATING` | `DEPRECATED` | Tự động sau 7 ngày Grace Period | Ngừng ký khóa cũ, khóa mới lên `ACTIVE` chính | System Auto Worker |
| `DEPRECATED` | `REVOKED` | Hết hạn lưu vết giải mã (30 ngày) | Tiêu hủy hoàn toàn khóa cũ khỏi Memory / Storage | SecurityAdmin |
| `ACTIVE` / `ROTATING` | `REVOKED` | **Phát hiện rò rỉ khẩn cấp** | Tiêu hủy khóa tức thì SLA $\le 5\text{m}$, tăng SecurityEpoch $+1$ | SecurityAdmin / Dual Approval |

## 3. Máy Trạng thái Vòng đời Bí mật (State Machine)

```
        [DRAFT State] --(Activate)--> [ACTIVE State]
                                            |
                                            v (Scheduled Rotation - Every 90d/365d)
                                   [ROTATING State] (Dual-Key Grace Period 7 Days)
                                            |
                                            v (After 7 Days Grace Period)
                                  [DEPRECATED State]
                                            |
                                            v (After 30 Days)
                                   [REVOKED State] <---+
                                                       |
         (Emergency Revocation SLA <= 5m) -------------+
```

## 4. Giao thức Thực thi Chuyển Trạng thái Bí mật CSDL (SecretLifecycleManagementService)

```csharp
public async Task<SecretVersionDto> RotateOrRevokeSecretAsync(
    string secretId, 
    SecretLifecycleAction action, 
    string actorUserId, 
    string reason)
{
    var secret = await _vaultClient.GetSecretMetadataAsync(secretId);
    if (secret == null) throw new InvalidOperationException("SECRET_NOT_FOUND");

    if (action == SecretLifecycleAction.TRIGGER_ROTATION)
    {
        // 1. Generate New Version & Set State to ROTATING (Dual-Key Pool)
        var newVersion = await _vaultClient.GenerateNewSecretVersionAsync(secretId);
        await _vaultClient.UpdateStateAsync(secretId, newVersion.VersionId, SecretState.ROTATING);

        // 2. Broadcast Secret Rotation Signal to Redis Pub/Sub for In-Memory Reload SLA <= 1s
        await _redisDb.PublishAsync("wordsoul:system:secret_rotated", JsonSerializer.Serialize(new { SecretId = secretId, NewVersionId = newVersion.VersionId }));

        // 3. Audit Log M11
        await _auditLog.RecordEventAsync("ACT-M11-41-SEC", actorUserId, new { SecretId = secretId, Action = "ROTATION_STARTED", NewVersion = newVersion.VersionId });

        return newVersion;
    }
    else if (action == SecretLifecycleAction.EMERGENCY_REVOKE)
    {
        // 4. Emergency Revocation SLA <= 5m: Purge from Vault & Signal Nodes
        await _vaultClient.RevokeSecretVersionAsync(secretId, secret.ActiveVersionId);

        // Signal all nodes to clear in-memory key cache immediately
        await _redisDb.PublishAsync("wordsoul:system:secret_revoked", JsonSerializer.Serialize(new { SecretId = secretId, RevokedVersionId = secret.ActiveVersionId }));

        // Increment SecurityEpoch M01 to revoke active JWT sessions if signing key revoked
        if (secret.Type == SecretType.JWT_SIGNING_KEY)
        {
            await _identityService.IncrementGlobalSecurityEpochAsync();
        }

        await _auditLog.RecordEventAsync("ACT-M11-41-SEC", actorUserId, new { SecretId = secretId, Action = "EMERGENCY_REVOKED", Reason = reason });

        return new SecretVersionDto { Status = "REVOKED" };
    }

    throw new ArgumentException("INVALID_SECRET_ACTION");
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SL-G01` | Xoay khóa bí mật bắt buộc trải qua trạng thái `ROTATING` với cửa sổ gối đầu 7 ngày (`Dual-Key Grace Period`). |
| `SL-G02` | Trong thời gian Dual-Key Grace Period, $100\%$ các luồng xác thực/giải mã phải chấp nhận cả 2 phiên bản khóa. |
| `SL-G03` | Quy trình xoay khóa định kỳ tuyệt đối CẤM làm gián đoạn dịch vụ hoặc gây lỗi HTTP 401/500 cho người dùng (REL-03). |
| `SL-G04` | Thu hồi khẩn cấp (`EMERGENCY_REVOKE`) tiêu hủy khóa khỏi bộ nhớ RAM tất cả các node SLA $\le 5$ phút. |
| `SL-G05` | Thu hồi khẩn cấp khóa ký JWT Signing Key phải tự động tăng `SecurityEpoch` $+1$ ngắt sạch phiên cũ. |
| `SL-G06` | 100% các thao tác xoay khóa / thu hồi bí mật được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-41-SEC`). |
| `SL-G07` | Phân quyền thực thi xoay khóa và thu hồi bí mật chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `SL-G08` | Tín hiệu thông báo xoay khóa qua Redis Pub/Sub đến các node ứng dụng xử lý nạp lại key SLA $\le 1$ giây. |
| `SL-G09` | SLA thực thi API chuyển trạng thái vòng đời bí mật $< 20\text{ms}$. |
| `SL-G10` | 100% các test case tự kiểm SL41-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SL41-01` | Kích hoạt xoay khóa định kỳ cho JWT Signing Key S0 | Chuyển sang trạng thái `ROTATING`, nạp khóa v2, giữ v1 trong 7 ngày |
| `SL41-02` | Người dùng dùng JWT ký bằng v1 gửi request khi hệ thống ở `ROTATING` | Request xác thực thành công (chấp nhận cả v1 và v2) |
| `SL41-03` | Hệ thống phát hành JWT mới khi ở trạng thái `ROTATING` | JWT mới được ký bằng v2 chính xác |
| `SL41-04` | Hết thời gian 7 ngày Dual-Key Grace Period | Auto Job chuyển khóa v1 sang `DEPRECATED`, v2 chính thức thành `ACTIVE` |
| `SL41-05` | Phát hiện rò rỉ khóa v1, SecurityAdmin bấm `EMERGENCY_REVOKE` | Khóa v1 bị tiêu hủy khỏi RAM SLA $< 2\text{m}$, tăng SecurityEpoch $+1$ |
| `SL41-06` | Người dùng gửi JWT ký bằng v1 ngay sau khi v1 bị `REVOKED` | Reject 401 Unauthorized `REVOKED_SIGNING_KEY` |
| `SL41-07` | Tra cứu vết Audit Log M11 sau khi xoay khóa bí mật | Ghi nhận Audit Event `ACT-M11-41-SEC` đính kèm v1 & v2 |
| `SL41-08` | Thử kích hoạt xoay khóa cho một SecretId không tồn tại | Reject 404 `SECRET_NOT_FOUND` |
| `SL41-09` | Tải đồng thời 100 request xác thực JWT trong lúc đang Pub/Sub nạp khóa mới | 100% request xử lý trơn tru 0 sập phiên |
| `SL41-10` | Kích hoạt xoay khóa Master Encryption Key AES-256-GCM S1 | Kích hoạt luồng re-encrypt dữ liệu trong CSDL theo v2 |
| `SL41-11` | Thử thu hồi khẩn cấp bí mật mà không nhập lý do | Reject 400 `REVOCATION_REASON_REQUIRED` |
| `SL41-12` | Phản hồi tín hiệu Redis Pub/Sub `wordsoul:system:secret_rotated` | Các node ứng dụng cập nhật bộ nạp key trong SLA $< 800\text{ms}$ |
| `SL41-13` | User không phải SecurityAdmin thử gọi API xoay khóa | Deny 403 Forbidden |
| `SL41-14` | User chưa đăng nhập gọi API vòng đời bí mật | Deny 401 Unauthorized |
| `SL41-15` | Sửa thời gian Dual-Key Grace Period từ 7 ngày thành 14 ngày | Cập nhật cấu hình quản trị thành công |
| `SL41-16` | Kiểm tra thời gian tiêu hủy bí mật khỏi bộ nhớ đệm Vault | Expire TTL đúng mốc SLA $\le 5$ phút |
| `SL41-17` | Phân tích tham chiếu các trạng thái bí mật trong CSDL | Quét schema `M12_SecretVersions` (T020) |
| `SL41-18` | Tín hiệu Redis Pub/Sub bị ngắt kết nối giữa chừng | Node tự động gọi Polling Vault định kỳ 1 phút/lần làm fallback |
| `SL41-19` | Thử xoay khóa bí mật khi chưa hết mốc 90 ngày | Yêu cầu nhập lý do xoay khóa trước thời hạn (`Manual Early Rotation`) |
| `SL41-20` | Kiểm thử hoàn tất luồng thiết kế vòng đời bí mật M12-SECRET-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-SL-I01` | M12 hiện tại chưa có bộ `SecretLifecycleManagementService` quản lý 5 trạng thái | Xoay khóa hiện tại làm sập phiên làm việc của người học | M12-T047-A (Source task) |
| `M12-SL-I02` | Chưa có bộ nhớ đệm `Dual-Key Pool` trong JWT Validator | Không chấp nhận đồng thời 2 phiên bản khóa khi đang xoay | M12-T047-A; M01-T016 |
| `M12-SL-I03` | Thiếu tín hiệu Redis Pub/Sub `wordsoul:system:secret_rotated` đến các node | Các node ứng dụng không biết khi nào khóa được thay đổi | M12-T047-A; M12-T032 |
| `M12-SL-I04` | Thiếu luồng tự động tăng `SecurityEpoch` $+1$ khi thu hồi khẩn cấp khóa JWT | Phiên JWT cũ vẫn còn dùng được sau khi khóa bị thu hồi | M12-SL-F04; M01-T016 |
| `M12-SL-I05` | Chưa có `AutoDeprecateSecretWorker` chuyển trạng thái sau 7 ngày | Khóa cũ ở trạng thái ROTATING vĩnh viễn không đóng | M12-T047-A; M11-T038 |

- `M12-SL-F01`: Triển khai `SecretLifecycleManagementService` với Máy Trạng thái 5 Bước (tiếp nhận: M12-T047-A).
- `M12-SL-F02`: Tích hợp Bắt buộc `Dual-Key Pool` 7 ngày & Redis Pub/Sub SLA $\le 1\text{s}$ (tiếp nhận: M12-T047-A; M12-T032).
- `M12-SL-F03`: Triển khai Emergency Revocation SLA $\le 5\text{m}$ & Auto Epoch $+1$ (tiếp nhận: M12-T047-A; M01-T016).
- `M12-SL-F04`: Thiết lập bộ kiểm thử tự động SL-G01–G10 và SL41-01–20 (tiếp nhận: M12 tasks).
- `M12-SL-F05`: Thu thập bằng chứng runtime cho luồng vòng đời bí mật M12 (tiếp nhận: M12 tasks; A-G05).

## 8. Tự kiểm M12-T041

- Đã thiết kế hoàn chỉnh `M12-SECRET-LIFECYCLE-1.0` với Máy Trạng thái Vòng đời Bí mật 5 Bước.
- Đã chốt Ràng buộc Xoay Khóa Không Gián đoạn Services (`Dual-Key Grace Period` 7 ngày).
- Đã chốt Ràng buộc Thu hồi Khẩn cấp khi Rò rỉ SLA $\le 5$ phút và Tự động tăng `SecurityEpoch` $+1$.
- Đã lồng ghép Tín hiệu Redis Pub/Sub SLA $\le 1\text{s}$ và Lưu vết Audit Log M11 (`ACT-M11-41-SEC`).
- Đã xác lập 10 Regression Gates (`SL-G01`–`SL-G10`) và 20 Test Cases tự kiểm (`SL41-01`–`SL41-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế vòng đời bí mật M12-T041 | WSA-7K2 |
