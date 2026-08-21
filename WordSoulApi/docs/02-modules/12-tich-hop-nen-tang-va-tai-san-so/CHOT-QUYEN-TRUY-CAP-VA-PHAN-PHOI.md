# Chốt quyền truy cập và phân phối M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ASSET-ACCESS-DISTRIBUTION-1.0` |
| Task | M12-T024 |
| Đầu vào | M12-ASSET-1.0 (D-021), M12-ASSET-IMMUTABLE-METADATA-1.0 (D-126), REL-03, REL-04 |
| Phạm vi | Đặc tả Giao thức Phân quyền Truy cập và Phân phối CDN Tài sản Học liệu (`Asset Access & CDN Distribution Protocol`), phân loại tài sản Public/Private, cơ chế CDN Signed URL (TTL 60m) và quy trình hủy Cache CDN toàn cầu SLA $\le 60$ giây |
| Tự kiểm | A-G05; REL-03, REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Phân quyền Truy cập và Phân phối CDN (`Asset Access & CDN Distribution Protocol`) thuộc M12, xác lập chính sách phân phối tài sản học liệu qua mạng lưới CDN toàn cầu (CloudFront / Cloudflare), phân định quyền truy cập giữa tài sản công khai và riêng tư, đồng thời đảm bảo khả năng thu hồi CDN tức thì khi tài sản vi phạm bản quyền (REL-03, REL-04).

- **Phân loại Phân phối Tài sản Public vs Private (`Public vs Private Asset Distribution Invariant`)**:
  - *Tài sản PUBLIC (Học liệu bộ từ công khai, âm thanh từ vựng hệ thống)*: Phân phối trực tiếp qua CDN public domain (`cdn.wordsoul.com/prod/...`) với chính sách HTTP Caching tối đa (`Cache-Control: public, max-age=31536000, immutable`).
  - *Tài sản PRIVATE (Ảnh đại diện người dùng, bộ từ nháp cá nhân)*: TUYỆT ĐỐI CẤM phân phối công khai. BẮT BUỘC truy cập thông qua CDN Signed URL hoặc Signed Cookies có chữ ký số ngắn hạn (TTL đúng 60 phút).
- **Ràng buộc Hủy Cache CDN Toàn cầu SLA $\le 60\text{s}$ (`Global CDN Invalidation SLA`)**: Khi một tài sản bị thu hồi khẩn cấp do vi phạm bản quyền (M02-T033) hoặc bị gỡ bỏ, hệ thống BẮT BUỘC phát lệnh CDN Invalidation API tới toàn bộ các PoP Edge Nodes trên toàn thế giới trong SLA $\le 60$ giây (D-051, REL-04).
- **Cấm Truy cập Trực tiếp Thùng chứa Storage Origin (`Storage Origin Access Identity - OAI`)**: Thùng chứa Cloud Storage Origin (S3 Bucket) TUYỆT ĐỐI CẤM mở truy cập công khai (`Block Public Access = true`). 100% request từ client BẮT BUỘC phải đi qua CDN Edge với OAI / OAC credentials.
- **Lưu vết Sổ Kiểm toán Phân phối M11 (`Distribution Audit Trail`)**: $100\%$ các cuộc gọi xin cấp CDN Signed URL hoặc hủy Cache CDN được ghi vết bất biến `ACT-M11-24-DIST` trong Sổ Kiểm toán M11, bao gồm `AssetId`, `AccessType` (`PUBLIC`, `SIGNED_URL`), `RequesterUserId` và `InvalidationId`.

## 2. Ma trận Chính sách Phân phối CDN Tài sản (CDN Distribution Matrix)

| Cấp độ Bảo mật (`SecurityLevel`) | Loại Tài sản Mẫu (`Sample Assets`) | Phương thức Phân phối | HTTP Cache-Control | Thời hạn Signed URL (`TTL`) | Quy trình Thu hồi Cache |
|---|---|---|---|---|---|
| `PUBLIC_GLOBAL` | Âm thanh phát âm, Hình minh họa từ vựng công khai | Global Public CDN Domain | `public, max-age=31536000, immutable` | N/A (Truy cập tự do) | CDN Invalidation API SLA $\le 60\text{s}$ |
| `PRIVATE_USER` | Ảnh đại diện cá nhân, Bộ từ nháp chưa duyệt | CDN Signed URL / Signed Cookie | `private, no-store` | **TTL = 60 phút** | Hủy Signed URL tức thì |
| `SYSTEM_INTERNAL` | Tệp tin đệm Staging, Tệp quét Antivirus | S3 Internal API Only (Chặn CDN) | `no-cache` | N/A (Nội bộ Backend) | Xóa tệp Staging |

## 3. Kiến trúc Luồng Phân phối CDN và Hủy Cache (CDN Distribution Engine)

```
[Client Requests Asset Access (AssetId)]
                   |
                   v
   [M12-AssetAccessService: Check Asset SecurityLevel]
                   |
     +-------------+-------------+
     | (Level == PUBLIC)         | (Level == PRIVATE)
     v                           v
[Return Direct Public CDN URL] [Generate CDN Signed URL (TTL 60m)]
- cdn.wordsoul.com/prod/...    - cdn.wordsoul.com/private/...?Key-Pair-Id=...&Signature=...
                                 |
                                 +-----------------------+
                                                         |
                                                         v
                                      [Asset Emergency Recalled (M02-T033)]
                                                         |
                                                         v
                                      [Issue CDN Invalidation API SLA <= 60s]
                                      - Purge Edge Cache across Global PoPs
                                      - Record Audit Log ACT-M11-24-DIST
```

## 4. Giao thức Thực thi Cấp Signed URL và Hủy Cache CSDL (AssetDistributionService)

```csharp
public async Task<string> GetAssetAccessUrlAsync(string assetId, string requesterUserId)
{
    var asset = await _db.AssetMetadatas.FirstOrDefaultAsync(a => a.AssetId == assetId);
    if (asset == null) throw new KeyNotFoundException("ASSET_NOT_FOUND");

    // 1. Public Asset Branch: Return Direct Public CDN Domain
    if (asset.SecurityLevel == AssetSecurityLevel.PUBLIC_GLOBAL)
    {
        return $"{_cdnConfig.PublicCdnBaseUrl}/{asset.ObjectKey}";
    }

    // 2. Private Asset Branch: Check Ownership & Issue CDN Signed URL (TTL 60m)
    if (asset.CreatedByUserId != requesterUserId && !_permissionService.IsAdmin(requesterUserId))
    {
        throw new UnauthorizedAccessException("ASSET_ACCESS_DENIED: Bạn không có quyền truy cập tài sản riêng tư này.");
    }

    string resourceUrl = $"{_cdnConfig.PrivateCdnBaseUrl}/{asset.ObjectKey}";
    DateTime expiresAt = DateTime.UtcNow.AddMinutes(60);

    // Generate CloudFront / Cloudflare Signed URL via RSA Private Key
    string signedUrl = CdnSigner.GenerateSignedUrl(resourceUrl, expiresAt, _cdnConfig.PrivateKey);

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-24-DIST", requesterUserId, new {
        AssetId = assetId,
        AccessType = "SIGNED_URL",
        ExpiresAtUtc = expiresAt
    });

    return signedUrl;
}

public async Task<bool> InvalidateCdnCacheAsync(string objectKey, string actorUserId)
{
    // 4. Trigger CDN Invalidation API SLA <= 60s
    var invalidationRequest = new CreateInvalidationRequest
    {
        DistributionId = _cdnConfig.DistributionId,
        InvalidationBatch = new InvalidationBatch
        {
            Paths = new Items { Items = new List<string> { $"/{objectKey}" }, Quantity = 1 },
            CallerReference = Guid.NewGuid().ToString("N")
        }
    };

    var response = await _cloudfrontClient.CreateInvalidationAsync(invalidationRequest);

    await _auditLog.RecordEventAsync("ACT-M11-24-DIST", actorUserId, new {
        ObjectKey = objectKey,
        Action = "CDN_INVALIDATED",
        InvalidationId = response.Invalidation.Id
    });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AD-G01` | Tài sản PUBLIC được phân phối qua CDN domain công khai với HTTP Cache 1 năm (`max-age=31536000`). |
| `AD-G02` | Tài sản PRIVATE tuyệt đối CẤM phân phối công khai, BẮT BUỘC dùng CDN Signed URL với TTL $\le 60$ phút. |
| `AD-G03` | Thùng chứa Origin Cloud Storage BẮT BUỘC chặn $100\%$ truy cập công khai trực tiếp (`Block Public Access`). |
| `AD-G04` | Lệnh hủy Cache CDN (`InvalidateCdnCacheAsync`) BẮT BUỘC hoàn tất gửi API tới CDN Edge SLA $\le 60$ giây. |
| `AD-G05` | Cấp CDN Signed URL phải xác thực chính chủ tài khoản tạo tài sản hoặc người có vai trò Quản trị viên. |
| `AD-G06` | 100% các cuộc gọi xin Signed URL hoặc hủy Cache CDN được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-24-DIST`). |
| `AD-G07` | SLA cấp phát CDN Signed URL từ API Server $< 5\text{ms}$. |
| `AD-G08` | Phân quyền ban hành lệnh hủy Cache CDN toàn cầu chỉ dành cho `ContentAdmin`, `SecurityAdmin` và System Workers. |
| `AD-G09` | Hệ thống hỗ trợ xử lý tới 5,000 requests xin Signed URL/giây trên toàn bộ các node API Gateway. |
| `AD-G10` | 100% các test case tự kiểm AD24-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AD24-01` | Tra cứu URL tài sản công khai MP3 phát âm từ vựng | Trả về Direct Public CDN URL `cdn.wordsoul.com/prod/...` |
| `AD24-02` | Tra cứu URL tài sản riêng tư (Ảnh nháp bộ từ của User A) bởi User A | Trả về CDN Signed URL hợp lệ, TTL 60 phút |
| `AD24-03` | Thử dùng CDN Signed URL để truy cập tài sản sau 65 phút ($> 60\text{m}$) | CDN Edge từ chối truy cập, trả về HTTP 403 Expired |
| `AD24-04` | User B thử xin CDN Signed URL cho ảnh nháp riêng tư của User A | Deny 403 `ASSET_ACCESS_DENIED` |
| `AD24-05` | Admin xin CDN Signed URL cho ảnh nháp riêng tư của User A | Cấp Signed URL thành công (Admin Override) |
| `AD24-06` | Tra cứu vết Audit Log M11 sau khi sinh CDN Signed URL | Ghi nhận Audit Event `ACT-M11-24-DIST` đính kèm AccessType |
| `AD24-07` | Phát lệnh hủy Cache CDN cho bộ từ bị thu hồi bản quyền (M02-T033) | Phát API Invalidation sang CloudFront SLA $< 30\text{s}$ |
| `AD24-08` | Thử gọi trực tiếp URL S3 Origin `s3.amazonaws.com/wordsoul-bucket/...` | HTTP 403 Forbidden (Origin Access Control Blocked) |
| `AD24-09` | Tải đồng thời 500 yêu cầu xin CDN Signed URL từ 500 người dùng | Processing latency p95 $< 3.5\text{ms}$ |
| `AD24-10` | Khóa RSA Private Key sinh Signed URL bị thay đổi (Key Rotation) | Nạp key mới từ Secret Manager M12-T040 trơn tru |
| `AD24-11` | Thử xin Signed URL với TTL được truyền vào là 180 phút ($> 60\text{m}$) | Reject 400 `INVALID_SIGNED_URL_TTL_MAX_60M` |
| `AD24-12` | Gửi request xin Signed URL khi JWT Access Token đã bị hết hạn | Deny 401 Unauthorized |
| `AD24-13` | User không phải Admin thử gọi API Invalidation CDN | Deny 403 Forbidden |
| `AD24-14` | User chưa đăng nhập gọi API phân phối tài sản riêng tư | Deny 401 Unauthorized |
| `AD24-15` | Phân phối tài sản đại diện hệ thống với cờ `Cache-Control: immutable` | Browser cache tệp tin thành công 1 năm |
| `AD24-16` | Kiểm tra độ trễ hoàn tất Invalidation API phía CDN Provider | Total SLA $< 45\text{s}$ trên các Edge PoPs |
| `AD24-17` | Phân tích tham chiếu cấu hình CDN Distribution trong CSDL | Quét schema `M12_CdnDistributions` (T020) |
| `AD24-18` | API Invalidation CDN CloudFront bị gián đoạn mạng | Retry tự động theo Exponential Backoff M12-T037 |
| `AD24-19` | Tra cứu danh sách các lệnh Invalidation CDN đang xử lý | Trả về danh sách InvalidationId và tiến độ |
| `AD24-20` | Kiểm thử hoàn tất luồng chốt quyền truy cập và phân phối M12-ASSET-ACCESS-DISTRIBUTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-AD-I01` | M12 hiện tại chưa có `AssetDistributionService` sinh CDN Signed URL | Dữ liệu hình ảnh cá nhân bị hở truy cập công khai | M12-T047-A (Source task) |
| `M12-AD-I02` | Chưa cài đặt chính sách chặn truy cập S3 Origin Direct | S3 Bucket vẫn mở public access trực tiếp | M12-T047-A; REL-03 |
| `M12-AD-I03` | Thiếu luồng CDN Invalidation API SLA $\le 60\text{s}$ khi thu hồi tài sản | Tệp vi phạm bản quyền vẫn nằm trong bộ nhớ đệm CDN Edge | M12-T047-A; M02-T033 |
| `M12-AD-I04` | Thiếu cờ giới hạn TTL CDN Signed URL tối đa 60 phút | Signed URLs sinh ra bị vô hiệu muộn gây phơi nhiễm | M12-AD-F04; M12-T040 |
| `M12-AD-I05` | Chưa kết nối sự kiện phân phối với Audit Log M11 (`ACT-M11-24-DIST`) | Không ghi vết được các lượt cấp Signed URL tài sản | M12-T047-A; M11-T031 |

- `M12-AD-F01`: Triển khai `AssetDistributionService` với CDN Signed URL TTL 60m (tiếp nhận: M12-T047-A).
- `M12-AD-F02`: Tích hợp Bắt buộc S3 Origin Access Control Block Public Access (tiếp nhận: M12-T047-A; REL-03).
- `M12-AD-F03`: Triển khai CDN Invalidation API SLA $\le 60\text{s}$ (tiếp nhận: M12-T047-A; M02-T033).
- `M12-AD-F04`: Thiết lập bộ kiểm thử tự động AD-G01–G10 và AD24-01–20 (tiếp nhận: M12 tasks).
- `M12-AD-F05`: Thu thập bằng chứng runtime cho luồng phân phối CDN M12 (tiếp nhận: M12 tasks; A-G05).

## 8. Tự kiểm M12-T024

- Đã thiết kế hoàn chỉnh `M12-ASSET-ACCESS-DISTRIBUTION-1.0` với Ma trận Chính sách Phân phối CDN Tài sản.
- Đã chốt Ràng buộc Phân loại Phân phối Tài sản Public vs Private (`Public vs Private Asset Distribution`).
- Đã chốt Ràng buộc Hủy Cache CDN Toàn cầu SLA $\le 60$ giây.
- Đã lồng ghép Cấm Truy cập Trực tiếp Thùng chứa Storage Origin và Audit Log M11 (`ACT-M11-24-DIST`).
- Đã xác lập 10 Regression Gates (`AD-G01`–`AD-G10`) và 20 Test Cases tự kiểm (`AD24-01`–`AD24-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt quyền truy cập và phân phối M12-T024 | WSA-7K2 |
