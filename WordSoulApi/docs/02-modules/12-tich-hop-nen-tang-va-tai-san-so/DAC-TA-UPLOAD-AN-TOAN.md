# Đặc tả upload an toàn M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-SECURE-ASSET-UPLOAD-1.0` |
| Task | M12-T022 |
| Đầu vào | M12-ASSET-1.0 (D-021), M02-VOCAB-ASSET-CATALOG-1.0 (D-071), M12-SECRET-INVENTORY-1.0 (D-069), REL-03, REL-04 |
| Phạm vi | Đặc tả Giao thức Upload Tài sản Học liệu An toàn (`Secure Asset Upload Protocol`), mô hình Pre-Signed Upload URL (TTL 15m), quy tắc kiểm tra MIME-Type/Dung lượng/Magic Bytes, quét độc hại Antivirus và phân quyền nộp tài sản |
| Tự kiểm | A-G05; REL-03, REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Upload Tài sản An toàn (`Secure Asset Upload Protocol`) thuộc M12, quy định luồng đẩy tài sản nghe/nhìn (âm thanh phát âm MP3, hình ảnh WebP/PNG đại diện hoặc hình họa từ vựng) trực tiếp từ ứng dụng client tới Cloud Storage (AWS S3 / Cloudflare R2), đảm bảo an ninh hệ thống và ngăn ngừa tải lên các tệp tin độc hại (REL-03, REL-04).

- **Mô hình Pre-Signed Upload URL TTL 15m (`Pre-Signed Upload Pattern Invariant`)**: Client CẤM upload tệp tin qua API Server chính. Server CHỈ cấp phát một đường dẫn tạm thời Pre-Signed PUT Object URL có chữ ký số ngắn hạn (TTL đúng 15 phút). Sau 15 phút, URL tự động bị vô hiệu hóa (REL-03).
- **Ràng buộc Danh mục Loại Tệp & Dung lượng Cho phép (`Allowed MIME-Type & Size Limits`)**:
  - *Âm thanh*: CHỈ chấp nhận `audio/mpeg` (MP3), `audio/wav`, `audio/ogg`. Dung lượng tối đa $\le 5\text{MB}$.
  - *Hình ảnh*: CHỈ chấp nhận `image/webp`, `image/png`, `image/jpeg`. Dung lượng tối đa $\le 2\text{MB}$.
  - *Tệp bị cấm*: TUYỆT ĐỐI CẤM tất cả các tệp thực thi (`.exe`, `.sh`, `.php`, `.js`, `.html`, `.bat`).
- **Thẩm định Magic Bytes Nội dung Tệp (`Magic Byte Header Verification`)**: Khi xác nhận hoàn tất upload, Backend BẮT BUỘC đọc 16 bytes đầu tiên của tệp tin trên Storage đệm để kiểm tra Magic Bytes (ví dụ: `ID3` cho MP3, `RIFF...WEBP` cho WebP). Nếu đuôi tệp tin không khớp với Magic Bytes thực tế, lập tức hủy tài sản.
- **Cổng Quét Độc hại Antivirus Tự động (`Automated Antivirus Scan Gate`)**: Tất cả tệp tin mới upload nằm ở vùng cách ly đệm (`STAGING`). Tệp CHỈ ĐƯỢC chuyển sang vùng chính thức (`PRODUCTION`) sau khi hệ thống ClamAV / AWS GuardDuty trả về kết quả `CLEAN` (D-072).

## 2. Ma trận Quy tắc Upload Tài sản An toàn (Secure Upload Matrix)

| Loại Tài sản (`AssetCategory`) | Định dạng Cấu hình | Giới hạn Dung lượng | Magic Bytes Chấp nhận | Cổng Quét Antivirus | SLA Cấp Pre-Signed URL |
|---|---|---|---|---|---|
| `AUDIO_PRONUNCIATION` | `.mp3`, `.wav`, `.ogg` | Max $5\text{MB}$ | `ID3` (MP3), `RIFF` (WAV), `OggS` (OGG) | Bắt buộc ClamAV | SLA $\le 10\text{ms}$ |
| `IMAGE_VOCAB_ILLUST` | `.webp`, `.png`, `.jpg` | Max $2\text{MB}$ | `RIFF...WEBP`, `\x89PNG`, `\xFF\xD8\xFF` | Bắt buộc ClamAV | SLA $\le 10\text{ms}$ |
| `IMAGE_AVATAR` | `.webp`, `.png` | Max $1\text{MB}$ | `RIFF...WEBP`, `\x89PNG` | Bắt buộc ClamAV | SLA $\le 10\text{ms}$ |
| `FORBIDDEN_FILE` | `.exe`, `.php`, `.sh`, `.html` | ANY | Reject All | N/A | Refuse 400 |

## 3. Kiến trúc Luồng Upload Tài sản An toàn M12 (Secure Upload Engine)

```
[User / Author Requests Upload Intent (FileName, ContentType, FileSizeBytes)]
                                    |
                                    v
            [Validate Allowed MIME-Type & Size Limits Guard]
                                    |
                                    v
            [Generate Pre-Signed PUT URL (TTL 15m) in S3 Staging Bucket]
                                    |
                                    v
            [Client Uploads File Directly to Cloud Storage Staging]
                                    |
                                    v
            [Client Calls ConfirmUpload API (StagingAssetId)]
                                    |
                                    v
            [Verify File Magic Bytes + Trigger ClamAV Scan]
                                    |
            +-----------------------+-----------------------+
            | (Clean & Valid Bytes)                         | (Infected or Invalid)
            v                                               v
   [Move File to Production Bucket]               [Purge Staging File]
   [Register Asset Record in CSDL]                [Reject 400 MALICIOUS_FILE]
   [Record Audit Log ACT-M11-22]
```

## 4. Giao thức Thực thi Cấp Pre-Signed Upload URL CSDL (SecureAssetUploadService)

```csharp
public async Task<PreSignedUploadUrlDto> RequestPreSignedUploadUrlAsync(
    string userId, 
    string fileName, 
    string contentType, 
    long fileSizeBytes)
{
    // 1. Validate MIME-Type & File Extension Guard
    string extension = Path.GetExtension(fileName).ToLowerInvariant();
    if (!IsAllowedMimeType(contentType, extension, fileSizeBytes))
    {
        throw new ArgumentException("INVALID_FILE_TYPE_OR_SIZE: Loại tệp hoặc dung lượng không được hỗ trợ.");
    }

    // 2. Generate Unique Staging Object Key
    string stagingObjectKey = $"staging/{userId}/{Guid.NewGuid():N}{extension}";

    // 3. Create AWS S3 / Cloudflare R2 Pre-Signed URL with TTL = 15m
    var request = new GetPreSignedUrlRequest
    {
        BucketName = _storageConfig.StagingBucketName,
        Key = stagingObjectKey,
        Verb = HttpVerb.PUT,
        Expires = DateTime.UtcNow.AddMinutes(15),
        ContentType = contentType
    };

    string preSignedPutUrl = _s3Client.GetPreSignedURL(request);

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-22-UPLOAD", userId, new {
        StagingObjectKey = stagingObjectKey,
        ContentType = contentType,
        FileSizeBytes = fileSizeBytes
    });

    return new PreSignedUploadUrlDto {
        UploadUrl = preSignedPutUrl,
        StagingObjectKey = stagingObjectKey,
        ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
    };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SU-G01` | Client upload tệp tin BẮT BUỘC dùng Pre-Signed PUT URL trực tiếp tới Cloud Storage (TTL $\le 15$ phút). |
| `SU-G02` | Loại tệp và dung lượng BẮT BUỘC tuân thủ giới hạn cho phép (Âm thanh $\le 5\text{MB}$, Hình ảnh $\le 2\text{MB}$). |
| `SU-G03` | Tuyệt đối CẤM cấp Pre-Signed URL cho các định dạng thực thi nguy hiểm (`.exe`, `.php`, `.sh`, `.html`). |
| `SU-G04` | Xác nhận upload BẮT BUỘC thẩm định Magic Bytes đầu tệp tin trước khi đăng ký tài sản chính thức. |
| `SU-G05` | Tệp tin mới upload BẮT BUỘC nằm ở thùng chứa `STAGING` chờ Antivirus scan trả về `CLEAN` (D-072). |
| `SU-G06` | 100% các yêu cầu cấp Pre-Signed URL được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-22-UPLOAD`). |
| `SU-G07` | Pre-Signed Upload URL hết hạn sau đúng 15 phút, từ chối mọi yêu cầu PUT đến sau. |
| `SU-G08` | Phân quyền xin cấp Pre-Signed URL chỉ dành cho người dùng đã đăng nhập hoặc `ContentAdmin`. |
| `SU-G09` | SLA cấp phát Pre-Signed Upload URL từ API Server $< 10\text{ms}$. |
| `SU-G10` | 100% các test case tự kiểm SU22-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SU22-01` | Xin Pre-Signed URL cho tệp phát âm MP3 dung lượng $1.2\text{MB}$ hợp lệ | Cấp Pre-Signed URL thành công, TTL 15m, HTTP 200 OK |
| `SU22-02` | Xin Pre-Signed URL cho tệp hình ảnh WebP dung lượng $800\text{KB}$ hợp lệ | Cấp Pre-Signed URL thành công, TTL 15m, HTTP 200 OK |
| `SU22-03` | Thử xin Pre-Signed URL cho tệp âm thanh MP3 dung lượng $6.5\text{MB}$ ($> 5\text{MB}$) | Reject 400 `INVALID_FILE_TYPE_OR_SIZE` |
| `SU22-04` | Thử xin Pre-Signed URL cho tệp kịch bản `.php` hoặc `.exe` | Reject 400 `FORBIDDEN_FILE_EXTENSION` |
| `SU22-05` | Upload tệp `.exe` nhưng cố tình đổi tên đuôi thành `.mp3` rồi bấm confirm | Thẩm định Magic Bytes thất bại, Hủy tệp staging, Reject 400 |
| `SU22-06` | Tra cứu vết Audit Log M11 sau khi xin Pre-Signed URL | Ghi nhận Audit Event `ACT-M11-22-UPLOAD` đính kèm ObjectKey |
| `SU22-07` | Thử dùng Pre-Signed Upload URL để PUT tệp tin sau 16 phút ($> 15\text{m}$) | Cloud Storage từ chối cuộc gọi PUT (HTTP 403 Expired) |
| `SU22-08` | ClamAV phát hiện tệp tin upload ở `STAGING` chứa mã độc virus | Hủy tệp staging, đánh dấu cờ an ninh `VIRUS_DETECTED` |
| `SU22-09` | Tải đồng thời 100 yêu cầu xin cấp Pre-Signed URL từ 100 tác giả | Processing latency p95 $< 8\text{ms}$ |
| `SU22-10` | Pre-Signed URL được sinh ra với đúng Content-Type header bắt buộc | S3 chấp nhận PUT request với Content-Type chuẩn xác |
| `SU22-11` | Thử xin Pre-Signed URL cho tệp hình ảnh PNG dung lượng $2.5\text{MB}$ ($> 2\text{MB}$) | Reject 400 `INVALID_FILE_TYPE_OR_SIZE` |
| `SU22-12` | Thử truyền Content-Type rỗng khi xin Pre-Signed URL | Reject 400 `CONTENT_TYPE_REQUIRED` |
| `SU22-13` | User không phải tác giả/admin thử xin Pre-Signed URL cho bài học khác | Deny 403 Forbidden |
| `SU22-14` | User chưa đăng nhập gọi API xin Pre-Signed URL | Deny 401 Unauthorized |
| `SU22-15` | Xác nhận hoàn tất upload cho tệp MP3 có Magic Bytes `ID3` chuẩn | Chuyển tệp sang `PRODUCTION`, trả về AssetId |
| `SU22-16` | Kiểm tra thời gian vô hiệu Pre-Signed URL trên Cloud Storage | Invalidated SLA $< 1\text{s}$ sau khi hết TTL |
| `SU22-17` | Phân tích tham chiếu các tệp tin trong `M12_StagingAssets` | Quét schema `M12_StagingAssets` (T020) |
| `SU22-18` | Dịch vụ ClamAV bị tạm dừng kết nối | Giữ tệp ở `STAGING`, retry scan qua Outbox M12-T037 |
| `SU22-19` | Tra cứu danh sách các tệp tin đang nằm trong vùng `STAGING` | Trả về danh sách tệp đệm chờ quét virus |
| `SU22-20` | Kiểm thử hoàn tất luồng đặc tả upload an toàn M12-SECURE-ASSET-UPLOAD-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-SU-I01` | M12 hiện tại chưa có `SecureAssetUploadService` sinh Pre-Signed URL | Risk client upload tệp tin trực tiếp thông qua API Server | M12-T047-A (Source task) |
| `M12-SU-I02` | Chưa cài đặt cờ kiểm tra Magic Bytes đầu tệp tin | Risk kẻ tấn công đổi đuôi file `.exe` thành `.png` lừa hệ thống | M12-T047-A; REL-04 |
| `M12-SU-I03` | Thiếu luồng quét Antivirus cách ly ở vùng đệm `STAGING` | Tệp tin chứa mã độc có thể nhảy thẳng vào CDN công khai | M12-T047-A; M02-T012 |
| `M12-SU-I04` | Thiếu cờ giới hạn TTL Pre-Signed URL tối đa 15 phút | Pre-Signed URLs có thể bị rò rỉ sử dụng vô thời hạn | M12-SU-F04; M12-T040 |
| `M12-SU-I05` | Chưa kết nối sự kiện cấp URL upload với Audit Log M11 (`ACT-M11-22-UPLOAD`) | Không ghi vết được người học xin cấp quyền tải tệp tin | M12-T047-A; M11-T031 |

- `M12-SU-F01`: Triển khai `SecureAssetUploadService` với Pre-Signed PUT URL TTL 15m (tiếp nhận: M12-T047-A).
- `M12-SU-F02`: Tích hợp Bắt buộc Magic Byte Verification & MIME-Type Guard (tiếp nhận: M12-T047-A; REL-04).
- `M12-SU-F03`: Triển khai Staging Antivirus Scan Gate (ClamAV Integration) (tiếp nhận: M12-T047-A; M02-T012).
- `M12-SU-F04`: Thiết lập bộ kiểm thử tự động SU-G01–G10 và SU22-01–20 (tiếp nhận: M12 tasks).
- `M12-SU-F05`: Thu thập bằng chứng runtime cho luồng upload M12 (tiếp nhận: M12 tasks; A-G05).

## 8. Tự kiểm M12-T022

- Đã thiết kế hoàn chỉnh `M12-SECURE-ASSET-UPLOAD-1.0` với Ma trận Quy tắc Upload Tài sản An toàn.
- Đã chốt Ràng buộc Mô hình Pre-Signed Upload URL (TTL 15 phút).
- Đã chốt Ràng buộc Danh mục Loại Tệp & Dung lượng Cho phép (Âm thanh $\le 5\text{MB}$, Hình ảnh $\le 2\text{MB}$).
- Đã lồng ghép Thẩm định Magic Bytes Nội dung Tệp, Cổng Quét Antivirus Tự động và Audit Log M11 (`ACT-M11-22-UPLOAD`).
- Đã xác lập 10 Regression Gates (`SU-G01`–`SU-G10`) và 20 Test Cases tự kiểm (`SU22-01`–`SU22-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả upload an toàn M12-T022 | WSA-7K2 |
