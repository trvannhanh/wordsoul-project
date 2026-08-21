# Thiết kế metadata và định danh bất biến M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ASSET-IMMUTABLE-METADATA-1.0` |
| Task | M12-T023 |
| Đầu vào | M12-ASSET-1.0 (D-021), M12-SECURE-ASSET-UPLOAD-1.0 (D-125), REL-04 |
| Phạm vi | Đặc tả Giao thức Định danh Bất biến và Metadata Tài sản Học liệu (`Asset Immutable Identity & Metadata Protocol`), cấu trúc mã băm SHA-256 (`AssetHash`), mô hình đường dẫn Object Key bất biến và quản lý bản quyền REL-04 |
| Tự kiểm | A-G03, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Định danh Bất biến và Metadata Tài sản Học liệu (`Asset Immutable Identity & Metadata Protocol`) thuộc M12, quy định nguyên tắc gán định danh duy nhất không thể thay đổi, tính toán mã băm nội dung SHA-256 để chống giả mạo và khử trùng lắp (Deduplication), đồng thời lưu giữ đầy đủ dữ liệu bản quyền REL-04 phục vụ Module M02 và M11.

- **Định danh Bất biến `AssetId` GUID (`Immutable AssetId Invariant`)**: Mỗi tài sản học liệu khi được đăng ký chính thức BẮT BUỘC sở hữu một `AssetId` dạng GUID duy nhất. `AssetId` này KHÔNG BAO GIỜ THAY ĐỔI trong suốt vòng đời của tệp tin. Mọi thao tác chỉnh sửa nội dung BẮT BUỘC tạo một `AssetId` mới.
- **Mã Băm Nội dung SHA-256 `AssetHash` (`Content-Addressed Hash Invariant`)**: Hệ thống TỰ ĐỘNG tính toán mã băm SHA-256 cho toàn bộ dữ liệu nhị phân của tệp tin ngay khi chuyển từ Staging sang Production. Mã `AssetHash` dùng để xác thực tính nguyên vẹn tệp tin và phát hiện tài sản trùng lặp (Deduplication SLA $\le 10\text{ms}$).
- **Cấu trúc Đường dẫn Object Key Bất biến (`Immutable Object Key Structure`)**: Tệp tin lưu trữ trên Cloud Storage BẮT BUỘC tuân thủ cấu trúc đường dẫn cố định: `prod/{AssetCategory}/{AssetHash[..2]}/{AssetId}{Extension}`. TUYỆT ĐỐI CẤM ghi đè tệp tin sẵn có tại Object Key này.
- **Envelope Metadata Bản quyền Bắt buộc REL-04 (`Mandatory Rights Metadata Envelope`)**: 100% tài sản BẮT BUỘC chứa các thuộc tính metadata bản quyền: `RightsLedgerId`, `LicenseType` (`CREATIVE_COMMONS`, `COMMERCIAL_PURCHASED`, `PUBLIC_DOMAIN`, `PROPRIETARY`), `OriginalAuthor`, và cờ `RightsCleared == true` trước khi được sử dụng ở M02 (REL-04, CT-01).

## 2. Ma trận Thuộc tính Metadata Tài sản Chuẩn hóa (Asset Metadata Matrix)

| Nhóm Metadata (`Group`) | Thuộc tính (`Property`) | Kiểu Dữ liệu | Quy tắc Ràng buộc | Mục đích Sử dụng |
|---|---|---|---|---|
| **IDENTITY** | `AssetId` | GUID (String) | Primary Key Bất biến | Định danh duy nhất hệ thống |
| **IDENTITY** | `AssetHash` | SHA-256 Hex | 64 ký tự Hex Bất biến | Khử trùng lắp & Kiểm tra toàn vẹn |
| **TECHNICAL** | `MimeType` | Enum String | Allowed Whitelist | Thẩm định loại tệp (D-125) |
| **TECHNICAL** | `FileSizeBytes` | Long | $>0$, Max Size Limit | Kiểm soát dung lượng |
| **TECHNICAL** | `AudioDurationSec` | Double? | $\le 120.0\text{s}$ (Cho Audio) | Tính toán thời lượng phát âm M05 |
| **TECHNICAL** | `ImageDimensions` | String? | Format `WIDTHxHEIGHT` | Tối ưu hiển thị UI M02/M06 |
| **RIGHTS_REL04** | `RightsCleared` | Boolean | Must be `true` for Public | Rào chắn pháp lý REL-04 / CT-01 |
| **RIGHTS_REL04** | `LicenseType` | Enum String | Allowed License Enum | Kiểm soát bản quyền tác giả |

## 3. Kiến trúc Đăng ký Metadata và Mã băm Asset (Metadata Registry Pipeline)

```
[File Uploaded to Production Storage]
                 |
                 v
  [Compute SHA-256 Binary Content Hash (AssetHash)]
                 |
                 v
   [Check Existing AssetHash in CSDL (Deduplication)]
                 |
        +--------+--------+
        | (Hash Exists)   | (Hash Unique)
        v                 v
[Reuse Existing AssetId] [Extract Technical Metadata (Duration/Dimensions)]
                         [Attach Rights Metadata Envelope (REL-04)]
                         [Save Asset Metadata Record to CSDL M12]
                         [Record Audit Event ACT-M11-23-META]
```

## 4. Giao thức Thực thi Đăng ký Metadata CSDL (AssetMetadataService)

```csharp
public async Task<AssetMetadataDto> RegisterAssetMetadataAsync(
    string stagingObjectKey, 
    AssetRightsEnvelopeDto rightsEnvelope, 
    string creatorUserId)
{
    // 1. Verify Rights Cleared Invariant REL-04 / CT-01
    if (!rightsEnvelope.RightsCleared || string.IsNullOrEmpty(rightsEnvelope.LicenseType))
    {
        throw new ArgumentException("RIGHTS_NOT_CLEARED_REL04: Tài sản bắt buộc phải có cờ RightsCleared = true và LicenseType hợp lệ.");
    }

    // 2. Download Binary Stream & Compute SHA-256 AssetHash
    using var fileStream = await _storageClient.GetStagingFileStreamAsync(stagingObjectKey);
    string assetHash = CryptoUtils.ComputeSha256Hash(fileStream);

    // 3. Deduplication Check: Reuse existing AssetId if binary identical
    var duplicateAsset = await _db.AssetMetadatas.FirstOrDefaultAsync(a => a.AssetHash == assetHash);
    if (duplicateAsset != null)
    {
        await _storageClient.DeleteStagingFileAsync(stagingObjectKey);
        return duplicateAsset.ToDto();
    }

    // 4. Extract Technical Metadata
    var techMetadata = await TechnicalMetadataExtractor.ExtractAsync(fileStream, rightsEnvelope.MimeType);

    // 5. Generate Immutable AssetId & Production Object Key
    string assetId = Guid.NewGuid().ToString("N");
    string extension = Path.GetExtension(stagingObjectKey);
    string productionObjectKey = $"prod/{rightsEnvelope.Category}/{assetHash[..2]}/{assetId}{extension}";

    // Move file to Production Bucket
    await _storageClient.MoveToProductionAsync(stagingObjectKey, productionObjectKey);

    // 6. Save Asset Record
    var assetRecord = new AssetMetadata {
        AssetId = assetId,
        AssetHash = assetHash,
        ObjectKey = productionObjectKey,
        MimeType = rightsEnvelope.MimeType,
        FileSizeBytes = fileStream.Length,
        AudioDurationSec = techMetadata.DurationSec,
        ImageDimensions = techMetadata.Dimensions,
        RightsCleared = true,
        LicenseType = rightsEnvelope.LicenseType,
        OriginalAuthor = rightsEnvelope.OriginalAuthor,
        CreatedByUserId = creatorUserId,
        CreatedAtUtc = DateTime.UtcNow
    };

    _db.AssetMetadatas.Add(assetRecord);
    await _db.SaveChangesAsync();

    // 7. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-23-META", creatorUserId, new {
        AssetId = assetId,
        AssetHash = assetHash,
        ObjectKey = productionObjectKey,
        LicenseType = rightsEnvelope.LicenseType
    });

    return assetRecord.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IM-G01` | Định danh `AssetId` GUID là bất biến, tuyệt đối CẤM thay đổi hoặc dùng lại cho tệp tin khác. |
| `IM-G02` | Tệp tin đăng ký BẮT BUỘC được tính toán mã băm SHA-256 `AssetHash` để xác thực tính toàn vẹn. |
| `IM-G03` | Hai tệp tin trùng khớp 100% binary hash tự động kích hoạt cơ chế Khử trùng lắp (`Deduplication`). |
| `IM-G04` | 100% tài sản công khai BẮT BUỘC chứa `RightsCleared == true` và `LicenseType` hợp lệ (REL-04 / CT-01). |
| `IM-G05` | Đường dẫn Object Key trên Cloud Storage tuân thủ cấu trúc bất biến `prod/{Category}/{Hash[..2]}/{AssetId}{Ext}`. |
| `IM-G06` | 100% các thao tác đăng ký metadata được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-23-META`). |
| `IM-G07` | SLA trích xuất technical metadata (độ dài âm thanh / kích thước ảnh) $< 20\text{ms}$. |
| `IM-G08` | Phân quyền cập nhật metadata bản quyền chỉ dành cho `ContentAdmin` và `SecurityAdmin`. |
| `IM-G09` | SLA API tra cứu metadata tài sản theo `AssetId` từ Redis Cache $< 2\text{ms}$. |
| `IM-G10` | 100% các test case tự kiểm IM23-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IM23-01` | Đăng ký tài sản MP3 phát âm với `RightsCleared = true` | Cấp `AssetId` GUID mới, tính mã `AssetHash` 64 ký tự hex |
| `IM23-02` | Đăng ký một tệp tin MP3 có binary hash trùng hệt tệp tin đã có | Deduplication thành công, trả về `AssetId` cũ, không tốn storage |
| `IM23-03` | Thử đăng ký tài sản với `RightsCleared = false` để dùng công khai | Reject 400 `RIGHTS_NOT_CLEARED_REL04` |
| `IM23-04` | Tra cứu metadata của `AssetId` hợp lệ qua API M12 | Trả về DTO chứa đầy đủ Technical & Rights metadata |
| `IM23-05` | Kiểm tra cấu hình đường dẫn Object Key tạo ra trên Production Bucket | Khớp với định dạng `prod/{Category}/{Hash[..2]}/{AssetId}{Ext}` |
| `IM23-06` | Tra cứu vết Audit Log M11 sau khi đăng ký metadata tài sản | Ghi nhận Audit Event `ACT-M11-23-META` đính kèm AssetId |
| `IM23-07` | Đăng ký tài sản hình ảnh WebP kích thước $500\times 500$ px | Trích xuất thành công `ImageDimensions = "500x500"` |
| `IM23-08` | Thử sửa thuộc tính `AssetHash` của một bản ghi metadata sẵn có | Reject 400 `IMMUTABLE_PROPERTY_CANNOT_BE_MUTATED` |
| `IM23-09` | Tải đồng thời 100 yêu cầu đăng ký metadata từ 100 tác giả | Processing latency p95 $< 18\text{ms}$ |
| `IM23-10` | Trích xuất độ dài tệp âm thanh MP3 phát âm $3.2$ giây | Trích xuất thành công `AudioDurationSec = 3.2` |
| `IM23-11` | Đăng ký tài sản với `LicenseType` rỗng hoặc không hợp lệ | Reject 400 `LICENSE_TYPE_REQUIRED` |
| `IM23-12` | Thử cập nhật `AssetId` của bản ghi trong CSDL SQL | Trigger SQL throw exception ngắt cập nhật |
| `IM23-13` | User không phải Admin thử sửa thông tin `OriginalAuthor` | Deny 403 Forbidden |
| `IM23-14` | User chưa đăng nhập gọi API đăng ký metadata | Deny 401 Unauthorized |
| `IM23-15` | Đăng ký tài sản thuộc bản quyền mua thương mại `COMMERCIAL_PURCHASED` | Lưu vết `RightsLedgerId` chứng minh mua bản quyền |
| `IM23-16` | Kiểm tra độ trễ tra cứu metadata từ Redis Cache đệm | Cache hit SLA $< 1.2\text{ms}$ |
| `IM23-17` | Phân tích tham chiếu danh sách `AssetMetadatas` trong CSDL | Quét schema `M12_AssetMetadatas` (T020) |
| `IM23-18` | Dịch vụ trích xuất metadata bị lỗi khi đọc tệp âm thanh bị hỏng | Fallback set `AudioDurationSec = null`, ghi log WARN |
| `IM23-19` | Tra cứu danh sách các tài sản được tạo bởi User A | Trả về DTO danh sách AssetId do User A tải lên |
| `IM23-20` | Kiểm thử hoàn tất luồng thiết kế metadata bất biến M12-ASSET-IMMUTABLE-METADATA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-IM-I01` | M12 hiện tại chưa có `AssetMetadataService` quản lý metadata bất biến | Thiếu cơ chế kiểm soát tính nguyên vẹn tệp tin | M12-T047-A (Source task) |
| `M12-IM-I02` | Chưa cài đặt mã băm `AssetHash` SHA-256 chống trùng lặp | Storage bị tốn bộ nhớ lưu trữ các tệp tin giống hệt nhau | M12-T047-A; M12-T025 |
| `M12-IM-I03` | Thiếu cờ bắt buộc `RightsCleared == true` khi đăng ký metadata | Risk phát hành tài sản chưa qua kiểm duyệt bản quyền REL-04 | M12-T047-A; REL-04 |
| `M12-IM-I04` | Thiếu bộ trích xuất Technical Metadata (Audio duration / Image dimensions) | Không đo được thời lượng phát âm phục vụ Module M05 | M12-T047-A |
| `M12-IM-I05` | Chưa kết nối sự kiện đăng ký metadata với Audit Log M11 (`ACT-M11-23-META`) | Không ghi vết được lịch sử khởi tạo tài sản | M12-T047-A; M11-T031 |

- `M12-IM-F01`: Triển khai `AssetMetadataService` với Immutable AssetId & SHA-256 Hash (tiếp nhận: M12-T047-A).
- `M12-IM-F02`: Tích hợp Bắt buộc Rights Metadata Envelope REL-04 / CT-01 (tiếp nhận: M12-T047-A; REL-04).
- `M12-IM-F03`: Triển khai Automatic Deduplication & Technical Metadata Extractor (tiếp nhận: M12-T047-A; M12-T025).
- `M12-IM-F04`: Thiết lập bộ kiểm thử tự động IM-G01–G10 và IM23-01–20 (tiếp nhận: M12 tasks).
- `M12-IM-F05`: Thu thập bằng chứng runtime cho luồng metadata M12 (tiếp nhận: M12 tasks; A-G03/A-G05).

## 8. Tự kiểm M12-T023

- Đã thiết kế hoàn chỉnh `M12-ASSET-IMMUTABLE-METADATA-1.0` với Ma trận Thuộc tính Metadata Tài sản Chuẩn hóa.
- Đã chốt Ràng buộc Định danh Bất biến `AssetId` GUID.
- Đã chốt Ràng buộc Mã Băm Nội dung SHA-256 `AssetHash` và Khử trùng lắp (`Deduplication`).
- Đã lồng ghép Cấu trúc Đường dẫn Object Key Bất biến, Envelope Metadata Bản quyền REL-04 và Audit Log M11 (`ACT-M11-23-META`).
- Đã xác lập 10 Regression Gates (`IM-G01`–`IM-G10`) và 20 Test Cases tự kiểm (`IM23-01`–`IM23-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế metadata và định danh bất biến M12-T023 | WSA-7K2 |
