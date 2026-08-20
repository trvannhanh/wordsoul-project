# Phân tách audit, activity và log vận hành M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-LOG-TAXONOMY-1.0` |
| Task | M11-T032 |
| Đầu vào | M11-AUDIT-EVENT-1.0, D-008, REL-02, REL-07 |
| Phạm vi | Phân định ranh giới 3 lớp log (Audit Log, Activity Log, Operational Log), quyền truy cập và chính sách lưu trữ |
| Tự kiểm | A-G02, A-G05; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập quy tắc phân định rõ ràng giữa 3 phân loại nhật ký hệ thống (Log Taxonomy): Audit Log (Nhật ký kiểm toán), Activity Log (Nhật ký hoạt động người dùng) và Operational/System Log (Nhật ký vận hành kỹ thuật).

- **Phân tách Ranh giới Tuyệt đối (Strict Log Boundary Separation)**:
  - *Audit Log*: Lưu vết các thao tác quản trị, thay đổi cấu hình, phân quyền, quyết định sự cố. Yêu cầu tính pháp lý, bất biến, chống giả mạo (`Tamper-Evident Hash Chain`), lưu trữ tối thiểu 12 tháng.
  - *Activity Log*: Lưu vết tương tác nghiệp vụ của người học (bắt đầu phiên học, hoàn thành bài tập, tham gia PvP, nhận phần thưởng). Phục vụ tính số liệu chỉ số, hỗ trợ người dùng và dòng thời gian.
  - *Operational Log*: Telemetry kỹ thuật (Serilog/ILogger debug, HTTP request/response, SQL query time, Redis cache hit/miss, worker execution logs). Phục vụ chẩn đoán sự cố kỹ thuật và giám sát hiệu năng.
- **Cấm Nhập nhằng Nguồn Truth**:
  - KHÔNG ĐƯỢC dùng Operational Log làm bằng chứng kiểm toán pháp lý hoặc làm nguồn tính toán số liệu nghiệp vụ.
  - KHÔNG ĐƯỢC ghi Audit Event ra stdout thô hoặc file log kỹ thuật không được bảo vệ.
- **Bảo vệ Secret & PII theo Cấp độ**: Operational log tuyệt đối KHÔNG chứa Secret/Token hay PII thô. Audit log băm PII/IP. Activity log chỉ chứa User ID và event metadata được cho phép.

## 2. Ma trận Phân loại 3 Lớp Log (Three-Tier Log Taxonomy Matrix)

| Tiêu chí | Audit Log (Nhật ký kiểm toán) | Activity Log (Nhật ký hoạt động) | Operational Log (Nhật ký vận hành) |
|---|---|---|---|
| **Mục đích** | Bằng chứng pháp lý, kiểm toán an ninh, truy vết thao tác quản trị | Dòng thời gian hỗ trợ, số liệu học tập, báo cáo sản phẩm | Giám sát hiệu năng, chẩn đoán lỗi ứng dụng, telemetry |
| **Đối tượng ghi** | 44 Admin Action ID (`M11-ACTION-1.0`), thay đổi cấu hình, grant quyền | Tương tác người học (M01–M10 events) | HTTP Requests, Exceptions, SQL execution, Cache logs |
| **Tính bất biến** | Bắt buộc Append-Only + **Tamper-Evident Hash Chain** | Append-Only (Hỗ trợ xóa theo GDPR/REL-07) | Ephemeral / Rotated files / Elastic index |
| **Thời hạn lưu giữ (`Retention`)** | Tối thiểu 12 tháng (SQL/Archive) | 3 – 6 tháng (Hot/Warm store) | 14 – 30 ngày (Hot store only) |
| **Quyền truy cập (`Access Role`)** | R10 Audit Admin, R12 Security Admin | R07 User Support, R09 Product Admin | R08 Operations Admin, Tech Lead / SRE |
| **Cơ chế lưu trữ** | Local Transaction SQL + Hash Chain | Event Stream / Partitioned Store | Serilog Sink / OpenTelemetry / Elasticsearch |
| **Quy tắc Redaction** | Bắt buộc `[REDACTED_SECRET]`, IP/PII Hash SHA-256 | Băm PII, chỉ lưu `userId` | Khóa Secret/Token/PII Filter tại Serilog Sink |

## 3. Quy tắc Phân định Ranh giới và Cấm Nhập nhằng

```
                                [APPLICATION EVENT SOURCE]
                                            |
         +----------------------------------+----------------------------------+
         |                                  |                                  |
         v                                  v                                  v
 [Governance / Admin Mutation]    [Learner Business Action]        [Technical System Telemetry]
         |                                  |                                  |
         v                                  v                                  v
  (AUDIT LOG ENGINE)              (ACTIVITY LOG ENGINE)            (OPERATIONAL LOG SINK)
  - M11-AUDIT-EVENT-1.0           - Activity Event Catalog         - Serilog / OpenTelemetry
  - Hash Chain Verification       - User Support Timeline          - Short Retention (14-30d)
  - Strict SQL Append-Only        - GDPR Delete Compliant          - Filter Secrets & PII
  - Long Retention (12m+)         - Mid Retention (3-6m)
```

### 3.1. Những hành vi BỊ CẤM (`Disallowed Practices`)
1. **Cấm dùng Log kỹ thuật làm Audit Log**: Không lấy log file `app-20260820.log` để giải trình kiểm toán an ninh.
2. **Cấm xuất Secret ra Operational Log**: Trình ghi log Serilog phải cài đặt bộ lọc `Destructing/Masking` tự động che giấu các trường `Password`, `Token`, `SecretKey`.
3. **Cấm xóa Activity Log mà không qua quy trình REL-07**: Việc xóa hoặc ẩn danh hóa Activity Log của người dùng bắt buộc tuân theo giao thức xuất/xóa dữ liệu cá nhân (REL-07).

## 4. Ma trận Quyền truy cập và Lưu giữ (Retention & Access Matrix)

| Loại Log | Quyền xem (`Read Access`) | Quyền Xuất (`Export Access`) | Thời gian lưu Hot | Thời gian lưu Cold / Archive |
|---|---|---|---|---|
| **Audit Log** | R10 Audit Admin, R12 Security Admin | R10 Audit Admin (Kèm lý do & Audit vụ việc) | 90 ngày (SQL) | 12 - 36 tháng (Encrypted Archive) |
| **Activity Log** | R07 Support, R09 Product, Admin sở hữu | R07 Support (Trừ PII theo REL-07) | 30 ngày (Redis/SQL) | 6 tháng (Parquet/Object Store) |
| **Operational Log** | R08 Ops, SRE, Developer | SRE / Ops (Chỉ log hạ tầng) | 14 ngày (Elastic) | 30 ngày (Purge tự động) |

## 5. Cài đặt Bộ lọc Bảo mật tại ranh giới Log (Log Boundary Security Filters)

Tất cả các thành phần ghi log trong `WordSoulApi` phải cấu hình Middleware/Enricher kiểm soát:
```csharp
// Ví dụ cấu hình Serilog Enrichment Masking tại ranh giới Operational Log
loggerConfiguration
    .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p.Contains("/api/auth/login")))
    .Enrich.WithMasking(options => {
        options.MaskProperties.Add("Password");
        options.MaskProperties.Add("Token");
        options.MaskProperties.Add("Secret");
        options.MaskFormat = "[REDACTED_SECRET]";
    });
```

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LT-G01` | Phân định 100% các loại log trong hệ thống thuộc đúng 1 trong 3 lớp: Audit, Activity, Operational. |
| `LT-G02` | Audit Log lưu trữ append-only tại SQL/Archive, không dùng file log kỹ thuật thay thế. |
| `LT-G03` | Operational Log cài đặt bộ lọc tự động che giấu `[REDACTED_SECRET]` và loại bỏ PII thô. |
| `LT-G04` | Thời hạn lưu trữ (`Retention Policy`) của từng lớp log được cấu hình và thực thi tự động. |
| `LT-G05` | Quyền truy cập tra cứu từng lớp log tuân thủ nghiêm ngặt ma trận vai trò phân quyền. |
| `LT-G06` | Activity Log của người học hỗ trợ luồng xóa/ẩn danh hóa tuân thủ giao thức REL-07. |
| `LT-G07` | Cấm dùng Operational Log làm nguồn truth để tính toán chỉ số nghiệp vụ hay số dư. |
| `LT-G08` | Mọi lỗi ghi log Operational không được làm crash tiến trình xử lý nghiệp vụ chính. |
| `LT-G09` | Hệ thống kiểm tra đối soát định kỳ tính toàn vẹn của Audit Log so với Activity Log. |
| `LT-G10` | 100% các test case tự kiểm LT32-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LT32-01` | Admin thực hiện Action `ACT-M11-01` ghi cấu hình | Log được định tuyến vào Audit Log Store với Hash Chain |
| `LT32-02` | Người học hoàn tất 1 phiên học M03 | Log được định tuyến vào Activity Log Store |
| `LT32-03` | Hệ thống ném ngoại lệ DB Timeout | Log được định tuyến vào Operational Log Store (Serilog) |
| `LT32-04` | Cố tình ghi Password người dùng vào Operational Log | Serilog enrichment mask thành `[REDACTED_SECRET]` |
| `LT32-05` | R07 Support Admin tra cứu Activity Log người học | Cho phép tra cứu dòng thời gian hoạt động học |
| `LT32-06` | R07 Support Admin cố tình tra cứu Audit Log quản trị | Deny 403 Forbidden |
| `LT32-07` | Chạy job dọn dẹp Operational Log quá 30 ngày | Xóa an toàn các index log kỹ thuật hết hạn |
| `LT32-08` | Chạy job dọn dẹp Audit Log quá 30 ngày | Deny operation; Audit Log lưu tối thiểu 12 tháng |
| `LT32-09` | Thực thi yêu cầu xóa dữ liệu cá nhân REL-07 cho người dùng | Xóa/Ẩn danh hóa Activity Log của người dùng đó |
| `LT32-10` | Thử dùng lệnh SQL DELETE trên Audit Log khi dọn Activity Log | DB deny operation vi phạm tính bất biến Audit |
| `LT32-11` | Kiểm tra log HTTP Request chứa header `Authorization: Bearer ...` | Header token bị mask thành `Bearer [REDACTED_TOKEN]` |
| `LT32-12` | Ghi log Activity khi Redis Cache offline | Activity log tự động chuyển hướng xuống SQL Table dự phòng |
| `LT32-13` | Developer truy cập Elasticsearch Operational Log | Xem được log chẩn đoán lỗi nhưng không thấy PII/Secret |
| `LT32-14` | Tra cứu log hỗ trợ sự cố theo `traceId` | Trả về vết Operational Log liên quan chính xác |
| `LT32-15` | Đẩy 10,000 Operational Logs/second | Serilog async sink xử lý không gây chậm ứng dụng |
| `LT32-16` | Kiểm tra quy tắc lưu trữ Cold Archive cho Audit Log | Log quá 90 ngày được đóng gói zip mã hóa chuyển sang Cold Store |
| `LT32-17` | Thử lấy Operational Log để chứng minh Admin A đã sửa đổi cấu hình | System reject bằng cảnh báo "Operational Log không phải bằng chứng kiểm toán" |
| `LT32-18` | Cấu hình mã hóa địa chỉ IP trong Activity Log | IP được băm `clientIpHash` an toàn |
| `LT32-19` | Tải đồng thời 50 request tra cứu 3 lớp log | Hệ thống phân quyền đúng role cho từng lớp log |
| `LT32-20` | Kiểm thử hoàn tất luồng phân loại 3 lớp log đầy đủ | Toàn bộ quy tắc phân định và bảo mật đạt 100% |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-LT-I01` | Trong `WordSoulApi`, toàn bộ log hiện tại đang sử dụng chung `ILogger` ghi ra Console/File | Chưa có sự phân tách giữa Audit Log, Activity Log và Operational Log | M11-T049 |
| `M11-LT-I02` | Chưa cài đặt bộ lọc tự động `Destructing/Masking` trên Serilog | Rủi ro ghi lỡ Secret/Token/Password ra file log thô | M11-T049 |
| `M11-LT-I03` | Thiếu chính sách tự động xóa/lưu trữ theo thời hạn (`Retention Policy`) cho các lớp log | CSDL và ỗ đĩa log có nguy cơ bị đầy dung lượng | M11-T049 |
| `M11-LT-I04` | Thiếu ma trận phân quyền truy cập tra cứu riêng biệt cho Audit Log vs Activity Log | Rủi ro rò rỉ vết kiểm toán an ninh cho các vai trò không phù hợp | M11-T049 |
| `M11-LT-I05` | Chưa có cơ chế xử lý xóa Activity Log tuân thủ giao thức REL-07 | Rủi ro vi phạm quy định xóa dữ liệu cá nhân | M01-T035; M11-T049 |

- `M11-LT-F01`: Cấu hình Serilog Enrichment Masking cho Operational Log (tiếp nhận: M11-T049).
- `M11-LT-F02`: Thiết lập hạ tầng lưu trữ riêng biệt cho Audit Log, Activity Log và Operational Log (tiếp nhận: M11-T049).
- `M11-LT-F03`: Xây dựng các Cron Job thực thi Retention Policy cho từng lớp log (tiếp nhận: M11-T049).
- `M11-LT-F04`: Thiết lập bộ kiểm thử tự động LT-G01–G10 và LT32-01–20 (tiếp nhận: M11-T049).
- `M11-LT-F05`: Thu thập bằng chứng runtime cho luồng phân tách 3 lớp log (tiếp nhận: M11-T049; A-G02/A-G05).

## 8. Tự kiểm M11-T032

- Đã thiết kế hoàn chỉnh `M11-LOG-TAXONOMY-1.0` phân định rõ 3 lớp log: Audit Log, Activity Log, Operational Log.
- Đã chốt ma trận lưu giữ (`Retention`) và ma trận phân quyền truy cập (`Access Role`) cho từng loại log.
- Đã xác lập các quy tắc bảo mật cấm Plaintext Secret và băm PII/IP tại ranh giới log.
- Đã thiết lập cấu hình mẫu Serilog Enrichment Masking cho ứng dụng.
- Đã xác lập 10 Regression Gates (`LT-G01`–`LT-G10`) và 20 Test Cases tự kiểm (`LT32-01`–`LT32-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả phân tách audit, activity và log vận hành M11-T032 | WSA-7K2 |
