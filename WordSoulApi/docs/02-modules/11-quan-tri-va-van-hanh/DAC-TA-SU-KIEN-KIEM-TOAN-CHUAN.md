# Đặc tả sự kiện kiểm toán chuẩn M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-AUDIT-EVENT-1.0` |
| Task | M11-T031 |
| Đầu vào | M11-ACTION-1.0, M11-PERM-1.0, D-008, REL-02 |
| Phạm vi | Cấu trúc Envelope sự kiện kiểm toán bất biến, bảo vệ tính chống giả mạo và che giấu bí mật/PII |
| Tự kiểm | A-G02, A-G05; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Đặc tả sự kiện kiểm toán chuẩn (Standard Audit Event Envelope Specification) đặt ra khuôn dạng chuẩn hóa, nguyên tắc bất biến và cơ chế bảo vệ chống giả mạo đối với toàn bộ các sự kiện kiểm toán phát sinh từ các thao tác quản trị hệ thống (44 Action ID thuộc `M11-ACTION-1.0`).

- **Bất biến Append-Only**: Sự kiện kiểm toán chỉ được phép thêm mới (`INSERT`), TUYỆT ĐỐI CẤM chỉnh sửa (`UPDATE`) hoặc xóa (`DELETE`) bản ghi đã ghi nhận dưới mọi hình thức ở cấp độ CSDL SQL và Storage.
- **Bảo vệ tính chống giả mạo (`Tamper-Evident Cryptographic Chain`)**: Mỗi sự kiện kiểm toán chứa `previousEventHash` và `eventHash` tạo thành một chuỗi Hash mã hóa nối tiếp. Mọi sự thay đổi trái phép trên bất kỳ bản ghi nào trong quá khứ sẽ làm hỏng chuỗi Hash và bị phát hiện lập tức khi chạy đối soát.
- **Loại bỏ Secret, Token và Payload thô (D-008 / A-G05)**: Sự kiện kiểm toán TUYỆT ĐỐI KHÔNG chứa mật khẩu, secret key, token, API key, payload thô chưa mã hóa hoặc PII cá nhân.
- **Mã hóa và Che giấu PII (REL-02 / Privacy)**: Địa chỉ IP client, User-Agent và dữ liệu định danh người học được băm mã hóa SHA-256 (`clientIpHash`, `actorIdHash`) hoặc che giấu (`[REDACTED_PII]`).
- **Liên kết ngữ cảnh Yêu cầu Thay đổi (`Traceability Envelope`)**: Mỗi sự kiện kiểm toán phải liên kết chính xác với `actionId`, `actorId`, `adminSessionId`, `changeRevisionId`, `decisionId` và `executionId`.

## 2. Ephemeral & Durable Audit Model

| Field / Component | Kiểu dữ liệu | Quy tắc & Ràng buộc | Mục đích kiểm toán |
|---|---|---|---|
| `eventId` | `UUID / ULID` | Khóa chính bất biến, sắp xếp theo thời gian monotonic | Định danh duy nhất sự kiện |
| `eventTimeUtc` | `DateTime (UTC)` | ISO 8601 UTC timestamp từ server DB truth | Thời điểm phát sinh thao tác |
| `actionId` | `String` | Ánh xạ 1 trong 44 Action ID thuộc `M11-ACTION-1.0` | Thao tác quản trị thực hiện |
| `actor` | `JSON Object` | `actorId`, `actorRole`, `adminSessionId`, `authVersion`, `clientIpHash`, `userAgentHash` | Đối tượng thực thi thao tác |
| `target` | `JSON Object` | `targetModule`, `resourceType`, `resourceId`, `tenantId` | Tài nguyên chịu tác động |
| `changeRevisionRef` | `JSON Object` | `changeRevisionId`, `revisionDigest`, `decisionId`, `executionId` | Hồ sơ ý định & quyết định |
| `diff` | `JSON Object` | `beforeDigest`, `afterDigest`, `changeSummaryJson` (Redacted) | Vi sản trước/sau thao tác |
| `outcome` | `JSON Object` | `status` (`SUCCESS`/`FAILED`/`REJECTED`), `errorCode`, `reason` | Kết quả thực thi |
| `tamperProtection` | `JSON Object` | `sequenceNo`, `previousEventHash`, `eventHash` (SHA-256) | Bảo vệ chống sửa đổi log |

Sơ đồ Chuỗi Hash Mã hóa (`Tamper-Evident Hash Chain`):
```
[Audit Event N-1]
  |-- eventId: ...
  |-- eventHash: Hash_A -----------------------+
                                                |
                                                v
[Audit Event N]                                 |
  |-- eventId: ...                              |
  |-- previousEventHash: Hash_A <---------------+
  |-- payload: (actor, action, diff, outcome)
  |-- eventHash: SHA256(previousEventHash + payload) = Hash_B ------------------+
                                                                                 |
                                                                                 v
[Audit Event N+1]                                                                |
  |-- previousEventHash: Hash_B <------------------------------------------------+
```

## 3. Cấu trúc Audit Event Envelope mẫu (JSON Format)

```json
{
  "eventId": "01J5X98Z7P0000000000000001",
  "eventTimeUtc": "2026-08-20T14:30:00.1234567Z",
  "actionId": "ACT-M11-03",
  "actor": {
    "actorId": "USR-ADMIN-007",
    "actorRole": "R02_OPERATIONS_ADMIN",
    "adminSessionId": "SES-ADM-99812",
    "authVersion": 4,
    "clientIpHash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    "userAgentHash": "5f4dcc3b5aa765d61d8327deb882cf99"
  },
  "target": {
    "targetModule": "M11",
    "resourceType": "SystemConfiguration",
    "resourceId": "WordsPerSession",
    "tenantId": "GLOBAL"
  },
  "changeRevisionRef": {
    "changeRevisionId": "REV-20260820-0012",
    "revisionDigest": "a8f5f167f44f4964e6c998dee827110c",
    "decisionId": "DEC-20260820-0005",
    "executionId": "EX-20260820-0001"
  },
  "diff": {
    "beforeDigest": "c4ca4238a0b923820dcc509a6f75849b",
    "afterDigest": "c81e728d9d4c2f636f067f89cc14862c",
    "changeSummaryJson": "[{\"key\":\"WordsPerSession\",\"before\":10,\"after\":15}]"
  },
  "outcome": {
    "status": "SUCCESS",
    "errorCode": null,
    "reason": "Routine SRS parameter adjustment via Canary Rollout"
  },
  "tamperProtection": {
    "sequenceNo": 10452,
    "previousEventHash": "893973d81b4d8d1e3d09a2b53a0678d46a6f0d922a969b925fb635d8e78f9f01",
    "eventHash": "b10a8db164e0754105b7a99be72e3fe5... (SHA256 of entire event payload)"
  }
}
```

## 4. Nguyên tắc Che Dữ liệu và Bảo mật (D-008 / REL-02 / A-G05)

1. **Tuyệt đối cấm Plaintext Secret**: Các trường chứa bí mật như `Password`, `PrivateKey`, `ApiKey`, `ConnectionToken` khi xuất hiện trong diff bắt buộc hiển thị `[REDACTED_SECRET]`.
2. **Loại bỏ PII nhạy cảm**: Email người dùng, Số điện thoại, Tên thật khi ghi vào log kiểm toán công khai bắt buộc chuyển thành Hash `[REDACTED_PII]` hoặc băm SHA-256.
3. **Mã hóa địa chỉ IP**: `clientIp` của Admin không được lưu dạng chuỗi thô (vd: `192.168.1.1`), bắt buộc băm `clientIpHash = SHA256(IP + Salt)`.

## 5. Quy trình Ghi nhận và Lưu giữ Log Bền vững

1. **Ghi đồng bộ trong Giao dịch (`In-Transaction Audit Enlistment`)**:
   Khi thực thi Action M11, bản ghi Audit Event được tạo và thêm vào `DbContext.AuditEvents` trong cùng một Local Transaction với thao tác đổi cấu hình / pointer. Nếu giao dịch thất bại, Audit Event tự động rollback.
2. **Outbox Pattern phát sự kiện**: Sau khi Local Transaction commit thành công, một Outbox Event đẩy Audit Envelope tới Redis Stream / Elastic/ClickHouse phục vụ tìm kiếm nhanh.
3. **Bảo vệ CSDL SQL**: Đặt quyền hạn ngạch CSDL (`DB Role Permissions`): Tài khoản ứng dụng chỉ có quyền `INSERT` và `SELECT` trên bảng `AuditEvents`; cấm quyền `UPDATE` và `DELETE`.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AE-G01` | 100% thao tác thuộc 44 Action ID M11 khi thực thi thành công/thất bại đều sinh Audit Event Envelope. |
| `AE-G02` | Bảng `AuditEvents` ở CSDL SQL cài đặt Trigger/Policy cấm hoàn toàn lệnh UPDATE và DELETE. |
| `AE-G03` | Chuỗi mã hóa Hash Chain (`previousEventHash` $\to$ `eventHash`) được tính toán và kiểm tra liên tục. |
| `AE-G04` | 100% secret, token, API key, password được che giấu thành `[REDACTED_SECRET]` trong diff envelope. |
| `AE-G05` | IP address và PII người dùng được băm mã hóa SHA-256 trước khi lưu vào audit envelope. |
| `AE-G06` | Audit Event Envelope lưu vết đầy đủ `changeRevisionId`, `decisionId` và `executionId`. |
| `AE-G07` | Lỗi phát sinh trong quá trình ghi Audit Event làm fail-closed toàn bộ thao tác mutation (D-008). |
| `AE-G08` | Cấu trúc Audit Event Envelope chuẩn hóa theo đúng JSON Schema v1.0. |
| `AE-G09` | Phân quyền truy cập tra cứu Audit log tuân thủ nghiêm ngặt ma trận `M11-PERM-1.0`. |
| `AE-G10` | 100% các test case tự kiểm AE31-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AE31-01` | Thực thi thành công Action `ACT-M11-03` | Sinh `AuditEventEnvelope` chuẩn đầy đủ 9 nhóm trường |
| `AE31-02` | Thử thực hiện lệnh SQL `UPDATE AuditEvents SET actionId = ...` | DB reject với lỗi `UPDATE_FORBIDDEN_ON_AUDIT_LOG` |
| `AE31-03` | Thử thực hiện lệnh SQL `DELETE FROM AuditEvents` | DB reject với lỗi `DELETE_FORBIDDEN_ON_AUDIT_LOG` |
| `AE31-04` | Sửa đổi 1 file log kiểm toán cũ và chạy tool đối soát Hash Chain | Hệ thống phát hiện đứt chuỗi Hash tại `sequenceNo` bị sửa |
| `AE31-05` | Thực thi Action đổi mật khẩu / secret key | Trường diff hiển thị `[REDACTED_SECRET]` cho secret payload |
| `AE31-06` | Thực thi Action có truyền địa chỉ IP client `10.0.0.1` | `clientIpHash` được lưu dưới dạng chuỗi SHA-256 |
| `AE31-07` | Thực thi Action bị thất bại do lỗi phân quyền 403 | Sinh Audit Event với `outcome.status = REJECTED` |
| `AE31-08` | Ghi Audit Event khi CSDL Audit bị ngắt kết nối | Mutation giao dịch chính bị rollback theo quy tắc Fail-Closed |
| `AE31-09` | Tra cứu Audit Event theo `changeRevisionId` | Trả về chính xác các sự kiện kiểm toán thuộc revision đó |
| `AE31-10` | Chèn 1000 Audit Events liên tiếp | Chuỗi Hash Chain được tính toán chính xác không đứt đoạn |
| `AE31-11` | Thực thi Action không qua Change Request (Action EC-1 read-only) | Sinh Audit Event loại Activity / Read với `changeRevisionRef = null` |
| `AE31-12` | Thử gửi Payload Audit Event thiếu `actorId` | Reject event creation với lỗi `INVALID_AUDIT_ENVELOPE_SCHEMA` |
| `AE31-13` | User không có quyền `R10 Audit Admin` gọi API tra cứu Audit Log | Deny 403 Forbidden |
| `AE31-14` | Kiểm tra tính nguyên tử giữa Audit Event và Mutation DB | Cả 2 cùng commit hoặc cùng rollback trong cùng 1 transaction |
| `AE31-15` | Đẩy Audit Event qua Redis Stream Outbox | Outbox worker đẩy event thành công tới Search Index |
| `AE31-16` | Thực thi Action điều chỉnh số dư tài sản M06 | Audit Event lưu vết `beforeDigest`, `afterDigest` và Mutation ID |
| `AE31-17` | Thực thi Action khóa tài khoản người dùng M01 | Audit Event lưu vết `actorId`, `targetResourceId` và `reason` |
| `AE31-18` | Tải đồng thời 100 giao dịch sinh Audit Event | p95 latency ghi log $< 50\text{ms}$, không tranh chấp CSDL |
| `AE31-19` | Xuất file Audit Log để phục vụ kiểm toán bên ngoài | File log xuất ra giữ nguyên chuỗi Hash mã hóa verification |
| `AE31-20` | Kiểm thử hoàn tất luồng: Action Execute $\to$ Enlist Audit $\to$ Hash Chain $\to$ Verify | Toàn bộ luồng thực thi đạt 100% tiêu chí an toàn |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-AE-I01` | Trong source `WordSoulApi`, chưa có bảng `AuditEvents` hay Envelope DTO chuẩn | Chưa có hạ tầng ghi nhận sự kiện kiểm toán bất biến | M11-T049 |
| `M11-AE-I02` | Chưa có cơ chế bảo vệ chống sửa đổi log bằng Chuỗi Hash Mã hóa (`Tamper-Evident Hash Chain`) | Log kiểm toán có thể bị chỉnh sửa hoặc xóa mà không để lại vết | M11-T049 |
| `M11-AE-I03` | Thiếu quy định cấm SQL UPDATE/DELETE trên bảng log kiểm toán | Rủi ro quản trị viên CSDL có thể can thiệp xóa vết | M11-T049 |
| `M11-AE-I04` | Thiếu cơ chế tự động che giấu secret (`[REDACTED_SECRET]`) và băm IP/PII | Rủi ro rò rỉ bí mật và PII người dùng vào log kiểm toán | M11-T049 |
| `M11-AE-I05` | Chưa có sự liên kết giữa Audit Log với `changeRevisionId` và `executionId` | Khó khăn khi điều tra truy vết các quyết định thay đổi | M11-T049 |

- `M11-AE-F01`: Xây dựng `AuditEventService` và bảng lưu trữ `AuditEvents` bất biến (tiếp nhận: M11-T049).
- `M11-AE-F02`: Triển khai engine tính toán Chuỗi Hash Mã hóa (`HashChainCalculator`) (tiếp nhận: M11-T049).
- `M11-AE-F03`: Thiết lập CSDL SQL Policy cấm UPDATE/DELETE trên bảng `AuditEvents` (tiếp nhận: M11-T049).
- `M11-AE-F04`: Thiết lập bộ kiểm thử tự động AE-G01–G10 và AE31-01–20 (tiếp nhận: M11-T049).
- `M11-AE-F05`: Thu thập bằng chứng runtime cho luồng ghi nhận và kiểm tra tính toàn vẹn của Audit Log (tiếp nhận: M11-T049; A-G02/A-G05).

## 8. Tự kiểm M11-T031

- Đã thiết kế hoàn chỉnh `M11-AUDIT-EVENT-1.0` với cấu trúc Audit Event Envelope 9 nhóm trường chuẩn.
- Đã chốt cơ chế bảo vệ chống giả mạo bằng Chuỗi Hash Mã hóa (`Tamper-Evident Hash Chain`).
- Đã xác lập nguyên tắc cấm SQL UPDATE/DELETE và ghi đồng bộ trong giao dịch local transaction.
- Đã quy định rõ các tiêu chuẩn che giấu bí mật (`[REDACTED_SECRET]`) và băm IP/PII bằng SHA-256.
- Đã xác lập 10 Regression Gates (`AE-G01`–`AE-G10`) và 20 Test Cases tự kiểm (`AE31-01`–`AE31-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả sự kiện kiểm toán chuẩn M11-T031 | WSA-7K2 |
