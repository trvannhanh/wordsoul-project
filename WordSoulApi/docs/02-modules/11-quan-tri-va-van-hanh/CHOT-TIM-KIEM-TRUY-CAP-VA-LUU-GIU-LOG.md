# Chốt tìm kiếm, truy cập và lưu giữ log M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-LOG-SEARCH-ACCESS-RETENTION-1.0` |
| Task | M11-T035 |
| Đầu vào | M11-LOG-TAXONOMY-1.0 (D-055), M11-SUSTAINABLE-LOG-INGESTION-1.0 (D-133), REL-02, REL-07 |
| Phạm vi | Đặc tả Giao thức Tìm kiếm, Phân quyền Truy cập và Vòng đời Lưu giữ Log (`Log Search, Access Control & Retention Protocol`), ma trận phân quyền truy cập RBAC theo lớp log, chỉ mục tìm kiếm ElasticSearch/Seq SLA $\le 500\text{ms}$, quy trình lưu trữ phân tầng Hot/Warm/Cold 12 tháng theo REL-07 và lưu vết kiểm toán M11 |
| Tự kiểm | A-G02, A-G05; REL-02, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Tìm kiếm, Phân quyền Truy cập và Vòng đời Lưu giữ Log (`Log Search, Access Control & Retention Protocol`) thuộc M11, xác lập khung quản trị tìm kiếm nhanh nhật ký theo `CorrelationId` / `UserId` / `TraceId`, phân quyền truy cập nghiêm ngặt theo vai trò RBAC, và thực thi chính sách lưu trữ phân tầng 3 lớp (Hot / Warm / Cold Glacier Lock 12 tháng) tuân thủ quy định pháp lý REL-02 và REL-07.

- **Ma trận Phân quyền Truy cập RBAC Theo Lớp Log (`RBAC Log Access Control Invariant`)**:
  - *Audit Logs*: CHỈ DÀNH RIÊNG cho `AuditAdmin` và `ComplianceOfficer`. Mọi thao tác giải mờ (Unmask PII) BẮT BUỘC có `TicketId` hỗ trợ hợp lệ (D-087, REL-01).
  - *Activity Logs*: Cấp quyền cho `SupportAgent` (yêu cầu ticket) và `ContentAdmin`.
  - *Operational Logs*: Cấp quyền cho `DevOpsEngineers` và `SecurityEngineers`.
- **Chỉ mục Tìm kiếm Nhanh SLA $\le 500\text{ms}$ (`Fast Index Search Engine SLA`)**: Hệ thống index ElasticSearch / Seq BẮT BUỘC hỗ trợ truy vấn tìm kiếm chuỗi log theo các trường khóa (`CorrelationId`, `UserId`, `TraceId`, `StatusCode`) với thời gian phản hồi SLA $\le 500\text{ms}$ đối với khoảng dữ liệu Hot Index 30 ngày.
- **Chính sách Lưu trữ Phân tầng Hot / Warm / Cold 12 Tháng (`Tiered Retention & Cold Archival Protocol REL-07`)**:
  - *Hot Index (ElasticSearch / Seq)*: Lưu giữ 30 ngày (Truy vấn tức thì SLA $< 500\text{ms}$).
  - *Warm Storage (S3 Standard-IA)*: Lưu giữ từ ngày 31 đến ngày 90 (Nén nạp lại khi cần).
  - *Cold Archive (S3 Glacier Vault Lock / Immutable WORM Storage)*: Lưu giữ từ tháng thứ 4 đến tháng thứ 12 (12 tháng bất biến theo REL-07).
- **Lưu vết Sổ Kiểm toán Truy cập Log M11 (`Log Access Audit Trail`)**: $100\%$ các cuộc gọi tra cứu, tìm kiếm hoặc xuất file nhật ký (Log Export) được ghi vết bất biến `ACT-M11-35-LOGACCESS` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy định Truy cập và Lưu giữ Log (Log Retention & Access Matrix)

| Lớp Log (`LogClass`) | Đối tượng Phân quyền (`Allowed Roles`) | Thời gian Hot Index | Thời gian Warm Storage | Thời gian Cold Glacier Archive | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `AUDIT_LOGS` | `AuditAdmin`, `ComplianceOfficer` | 30 ngày | 90 ngày | **12 Tháng (WORM Vault Lock)** | `ACT-M11-35-AUDIT` |
| `ACTIVITY_LOGS` | `SupportAgent` (Ticket), `ContentAdmin` | 30 ngày | 60 ngày | 180 ngày | `ACT-M11-35-ACTIVITY` |
| `OPERATIONAL_LOGS` | `DevOps`, `SecurityEngineer` | 14 ngày | 30 ngày | Xóa sạch sau 30d | `ACT-M11-35-OPS` |

## 3. Kiến trúc Luồng Tra cứu và Lưu giữ Phân tầng (Log Retention Engine)

```
[Log Event Ingested into ElasticSearch / Seq Hot Index]
                         |
                         v
[Hot Index Search API (SLA <= 500ms for CorrelationId / UserId)]
                         |
                         v
       [Daily Retention Lifecycle Worker (01:00 UTC)]
                         |
        +----------------+----------------+
        | (Age > 30 Days)                 | (Age > 90 Days)
        v                                 v
[Move to Warm S3 Standard-IA]    [Move to Cold S3 Glacier WORM Lock]
- Compressed JSON Parquet        - Immutable Vault Lock (12m Retention)
- Evict from Hot Index           - Record Audit Log ACT-M11-35-ARCHIVE
```

## 4. Giao thức Thực thi Tra cứu và Phân tầng CSDL (LogSearchAndRetentionService)

```csharp
public async Task<LogSearchResultDto> SearchLogsAsync(LogSearchQueryDto query, string actorUserId)
{
    // 1. RBAC Access Control Guard
    if (query.TargetLogClass == LogClass.AUDIT_LOGS)
    {
        if (!_permissionService.HasRole(actorUserId, "AuditAdmin") && !_permissionService.HasRole(actorUserId, "ComplianceOfficer"))
        {
            throw new UnauthorizedAccessException("LOG_ACCESS_DENIED: Bạn không có quyền truy cập Audit Logs.");
        }
    }

    // 2. Unmasking Ticket Verification Guard REL-01 / REL-07
    if (query.IsUnmaskRequested && string.IsNullOrEmpty(query.SupportTicketId))
    {
        throw new ArgumentException("UNMASK_TICKET_REQUIRED: Thao tác giải mờ thông tin log bắt buộc đính kèm SupportTicketId hợp lệ.");
    }

    // 3. Execute Fast Index Search SLA <= 500ms
    var searchResponse = await _elasticClient.SearchAsync<OperationalLogDocument>(s => s
        .Index(query.TargetLogClass.ToString().ToLowerInvariant())
        .Query(q => q.Term(t => t.CorrelationId, query.CorrelationId))
        .Size(query.PageSize)
    );

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-35-LOGACCESS", actorUserId, new {
        LogClass = query.TargetLogClass.ToString(),
        CorrelationId = query.CorrelationId,
        IsUnmaskRequested = query.IsUnmaskRequested,
        ResultCount = searchResponse.Documents.Count
    });

    return new LogSearchResultDto {
        Logs = searchResponse.Documents.ToList(),
        TotalHits = searchResponse.Total
    };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LS-G01` | Phân quyền truy cập log BẮT BUỘC tuân thủ ma trận RBAC (Audit Logs chỉ cho `AuditAdmin` & `ComplianceOfficer`). |
| `LS-G02` | Thao tác giải mờ log (Unmask PII) BẮT BUỘC có `SupportTicketId` hợp lệ được xác minh (REL-01, REL-07). |
| `LS-G03` | Chỉ mục Hot Index BẮT BUỘC phản hồi kết quả tìm kiếm theo `CorrelationId` / `UserId` SLA $\le 500\text{ms}$. |
| `LS-G04` | Audit Logs BẮT BUỘC được lưu giữ tối thiểu 12 tháng tại kho lưu trữ bất biến Cold Glacier WORM Vault Lock. |
| `LS-G05` | Operational Logs tự động được dọn dẹp và tiêu hủy hoàn toàn sau 30 ngày lưu trữ. |
| `LS-G06` | 100% các cuộc gọi tra cứu hoặc xuất log được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-35-LOGACCESS`). |
| `LS-G07` | Lệnh lưu trữ phân tầng tự động di chuyển log sang S3 Glacier chạy ngầm hàng ngày lúc 01:00 UTC. |
| `LS-G08` | Phân quyền xuất file log số lượng lớn (Log Export $> 10,000$ rows) chỉ dành riêng cho `SecurityAdmin`. |
| `LS-G09` | Chịu tải đến 500 yêu cầu tìm kiếm log đồng thời trên hệ thống ElasticSearch/Seq Cluster. |
| `LS-G10` | 100% các test case tự kiểm LS35-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LS35-01` | AuditAdmin tìm kiếm Audit Logs theo `CorrelationId = "corr_123"` | Trả về log chuẩn xác trong Hot Index SLA $< 120\text{ms}$ |
| `LS35-02` | SupportAgent thử tra cứu Audit Logs khi không thuộc vai trò AuditAdmin | Reject 403 `LOG_ACCESS_DENIED` |
| `LS35-03` | SupportAgent xin giải mờ Email trong Activity Log kèm `SupportTicketId` hợp lệ | Giải mờ thành công, ghi vết Audit Log `ACT-M11-35-UNMASK` |
| `LS35-04` | SupportAgent xin giải mờ Email trong Activity Log nhưng KHÔNG truyền `SupportTicketId` | Reject 400 `UNMASK_TICKET_REQUIRED` |
| `LS35-05` | Worker phân tầng di chuyển Audit Logs 35 ngày tuổi sang Warm Storage S3-IA | Di chuyển tệp nén Parquet thành công, xóa khỏi Hot Index |
| `LS35-06` | Tra cứu vết Audit Log M11 sau khi thực hiện tìm kiếm log | Ghi nhận Audit Event `ACT-M11-35-LOGACCESS` |
| `LS35-07` | Thử xóa hoặc sửa tệp Audit Log nằm trong S3 Glacier WORM Vault Lock | S3 Object Lock từ chối lệnh DELETE/PUT (HTTP 403 AccessDenied) |
| `LS35-08` | Tìm kiếm Operational Logs đã quá 35 ngày tuổi (quá hạn 30 ngày) | Trả về 0 kết quả (Đã dọn dẹp sạch theo chính sách 30d) |
| `LS35-09` | Tải đồng thời 100 truy vấn tìm kiếm log theo `UserId` | Search latency p95 $< 380\text{ms}$ |
| `LS35-10` | SecurityAdmin xuất 50,000 dòng Operational Log ra file CSV | Xuất file thành công, ghi nhận Audit Event `ACT-M11-35-EXPORT` |
| `LS35-11` | Thử tìm kiếm log với chuỗi truy vấn `CorrelationId` rỗng | Reject 400 `CORRELATION_ID_REQUIRED` |
| `LS35-12` | Gửi request tìm kiếm log khi JWT Access Token đã bị hết hạn | Deny 401 Unauthorized |
| `LS35-13` | User không phải SecurityAdmin thử xuất 50,000 dòng log | Deny 403 Forbidden |
| `LS35-14` | User chưa đăng nhập gọi API tìm kiếm log M11 | Deny 401 Unauthorized |
| `LS35-15` | Tra cứu kho Cold Glacier Archive đòi hỏi khôi phục tệp log cũ | Trả về yêu cầu 402 `GLACIER_RESTORE_INITIATED` (Chờ 3-5h) |
| `LS35-16` | Kiểm tra thời gian tạo chỉ mục Hot Index cho 1,000 log events mới | Indexing SLA $< 300\text{ms}$ |
| `LS35-17` | Phân tích tham chiếu các bản ghi `LogAccessAudits` trong CSDL | Quét schema `M11_LogAccessAudits` (T020) |
| `LS35-18` | Dịch vụ ElasticSearch Cluster bị ngắt kết nối tạm thời | Fallback tra cứu qua Log File đệm đĩa M11-T034 |
| `LS35-19` | Tra cứu danh sách các yêu cầu khôi phục tệp log từ Glacier | Trả về DTO danh sách RestoreRequests và trạng thái |
| `LS35-20` | Kiểm thử hoàn tất luồng chốt tìm kiếm và lưu giữ log M11-LOG-SEARCH-ACCESS-RETENTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-LS-I01` | M11 hiện tại chưa có `LogSearchAndRetentionService` phân quyền RBAC | Risk rò rỉ Audit Logs cho người không đúng thẩm quyền | M11-T049 (Source task) |
| `M11-LS-I02` | Thiếu cờ bắt buộc `SupportTicketId` khi giải mờ log cá nhân | Vi phạm nghiêm trọng nguyên tắc bảo vệ dữ liệu REL-01/REL-07 | M11-T049; REL-01 |
| `M11-LS-I03` | Thiếu luồng tự động nén di chuyển log sang Cold S3 Glacier WORM Lock 12m | Không đáp ứng tiêu chuẩn lưu giữ kiểm toán 12 tháng | M11-T049; REL-07 |
| `M11-LS-I04` | Thiếu cờ giới hạn xuất file log số lượng lớn cho SecurityAdmin | Risk bị tải trộm toàn bộ log ứng dụng ra ngoài | M11-LS-F04; M11-T004 |
| `M11-LS-I05` | Chưa kết nối sự kiện tra cứu log với Audit Log M11 (`ACT-M11-35-LOGACCESS`) | Không ghi vết được người đã đọc và xuất dữ liệu log | M11-T049; M11-T031 |

- `M11-LS-F01`: Triển khai `LogSearchAndRetentionService` với RBAC Access Control Matrix (tiếp nhận: M11-T049).
- `M11-LS-F02`: Tích hợp Bắt buộc SupportTicketId Guard cho Unmasking (tiếp nhận: M11-T049; REL-01).
- `M11-LS-F03`: Triển khai Tiered Retention & S3 Glacier WORM Lock 12m (tiếp nhận: M11-T049; REL-07).
- `M11-LS-F04`: Thiết lập bộ kiểm thử tự động LS-G01–G10 và LS35-01–20 (tiếp nhận: M11 tasks).
- `M11-LS-F05`: Thu thập bằng chứng runtime cho luồng tìm kiếm log M11 (tiếp nhận: M11 tasks; A-G02/A-G05).

## 8. Tự kiểm M11-T035

- Đã thiết kế hoàn chỉnh `M11-LOG-SEARCH-ACCESS-RETENTION-1.0` với Ma trận Quy định Truy cập và Lưu giữ Log.
- Đã chốt Ràng buộc Ma trận Phân quyền Truy cập RBAC Theo Lớp Log.
- Đã chốt Ràng buộc Bắt buộc Ticket khi Giải mờ Log (`SupportTicketId`).
- Đã lồng ghép Chỉ mục Tìm kiếm Nhanh SLA $\le 500\text{ms}$, Chính sách Lưu trữ Phân tầng Hot/Warm/Cold 12 Tháng (WORM Glacier Lock REL-07) và Audit Log M11 (`ACT-M11-35-LOGACCESS`).
- Đã xác lập 10 Regression Gates (`LS-G01`–`LS-G10`) và 20 Test Cases tự kiểm (`LS35-01`–`LS35-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt tìm kiếm, truy cập và lưu giữ log M11-T035 | WSA-7K2 |
