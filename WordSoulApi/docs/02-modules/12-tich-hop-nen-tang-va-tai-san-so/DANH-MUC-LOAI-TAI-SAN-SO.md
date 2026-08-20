# Danh mục loại tài sản số M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ASSET-CATALOG-1.0` |
| Task | M12-T021 |
| Đầu vào | M12-CONTRACT-1.0 (D-021), A0-T004 (REL-04), CT-01 |
| Phạm vi | Danh mục 8 loại tài sản số, quy tắc định dạng/dung lượng, cấu trúc đường dẫn lưu trữ, quy trình xác minh bản quyền và giao thức Presigned URL |
| Tự kiểm | A-G03, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Danh mục Phân loại và Hạ tầng Quản lý Vòng đời Tài sản Số (`Digital Asset Catalog Engine`) trong M12, phục vụ lưu trữ, cung cấp và bảo vệ bản quyền cho toàn bộ các file âm thanh, hình ảnh và tài liệu của WordSoul.

- **Định danh Bất biến Tài sản Số (`Asset Unique Identity`)**: Mỗi tài sản số sở hữu một mã định danh duy nhất `assetId` (UUID/ULID) và đường dẫn lưu trữ bất biến (`storagePath`).
- **Tuân thủ Bản quyền Cứng (`REL-04 / CT-01 Compliance`)**: Mọi tài sản số được sử dụng trong các mục từ/bộ từ vựng công khai bắt buộc phải có thông tin bản quyền `licenseId`, `licenseType` và trạng thái xác minh `rightsCleared == true`. CẤM lưu trữ hoặc phân phối tài sản vi phạm bản quyền.
- **Phân tách Tài sản Công khai và Riêng tư (`Public vs Private Bucket Isolation`)**:
  - *Public Assets (Âm thanh/Hình ảnh học liệu)*: Lưu trữ tại Bucket Công khai (`wordsoul-public-assets`), phân phối qua CDN với HTTPS URL cố định.
  - *Private / Transient Assets (Ghi âm phát âm người học M05, file tạm AI)*: Lưu trữ tại Bucket Riêng tư (`wordsoul-private-assets`), chỉ cho phép truy cập qua Signed Presigned URL ngắn hạn (TTL 15 phút).
- **Vòng đời và Tự động Dọn dẹp (`Asset Lifecycle & Retention Job`)**: Các tài sản tạm thời (User Audio Attempt) có thời gian lưu trữ tối đa 14 ngày trước khi bị dọn dẹp tự động nhằm tối ưu chi phí lưu trữ.

## 2. Danh mục 8 Loại Tài sản Số tiêu chuẩn (Digital Asset Catalog Matrix)

| Loại tài sản | Mã AssetType | Định dạng MIME | Dung lượng max | Bucket / Path Pattern | Loại Bucket | Thời hạn lưu trữ (`Retention`) |
|---|---|---|---|---|---|---|
| Âm thanh Từ vựng | `AUDIO_HEADWORD` | `audio/mpeg` (MP3), `audio/ogg` | 500 KB | `audio/vocab/{vocabId}.mp3` | Public | Vĩnh viễn (CDN Cache) |
| Âm thanh Câu ví dụ | `AUDIO_EXAMPLE_SENTENCE` | `audio/mpeg` (MP3) | 1.5 MB | `audio/examples/{senseId}.mp3` | Public | Vĩnh viễn (CDN Cache) |
| Âm thanh Người học thử | `AUDIO_PRONUNCIATION_ATTEMPT` | `audio/wav`, `audio/webm` | 2.0 MB | `private/attempts/{userId}/{attemptId}.wav` | Private | 14 ngày (Auto Purge) |
| Hình ảnh Mục từ | `IMAGE_HEADWORD` | `image/jpeg`, `image/webp`, `image/png` | 2.0 MB | `images/vocab/{vocabId}.webp` | Public | Vĩnh viễn (CDN Cache) |
| Hình ảnh Avatar | `IMAGE_AVATAR` | `image/jpeg`, `image/webp` | 1.0 MB | `images/avatars/{userId}.webp` | Public | Vĩnh viễn (Ghi đè) |
| Hình ảnh Ảnh bìa Bộ từ | `IMAGE_SET_COVER` | `image/jpeg`, `image/webp` | 3.0 MB | `images/sets/{setId}.webp` | Public | Vĩnh viễn (CDN Cache) |
| Hình ảnh Trang phục Thú cưng | `IMAGE_PET_SKIN` | `image/png`, `image/svg+xml` | 4.0 MB | `images/pets/{petId}/{skinId}.png` | Public | Vĩnh viễn (CDN Cache) |
| Tài liệu PDF Điều khoản | `DOC_TERMS_PDF` | `application/pdf` | 10.0 MB | `docs/legal/{docId}_v{ver}.pdf` | Public | 36 tháng (REL-06) |

## 3. Metadata Bản quyền và Cấu trúc Schema (Asset Metadata Schema)

```json
{
  "assetId": "01J5XA00000000000000000001",
  "assetType": "AUDIO_HEADWORD",
  "mimeType": "audio/mpeg",
  "fileSizeBytes": 245800,
  "storageBucket": "wordsoul-public-assets",
  "storagePath": "audio/vocab/vocab-1024.mp3",
  "publicUrl": "https://cdn.wordsoul.app/audio/vocab/vocab-1024.mp3",
  "checksumSha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "licenseId": "LIC-2026-CC-BY-4.0-001",
  "licenseType": "CC-BY-4.0",
  "rightsOwner": "Oxford Learner Data / WordSoul Publishing",
  "rightsCleared": true,
  "createdAtUtc": "2026-08-20T10:00:00Z"
}
```

## 4. Giao thức Tạo URL Truy cập (Presigned URL Protocol)

```csharp
public class AssetAccessService
{
    public string ResolveAssetUrl(DigitalAsset asset)
    {
        // 1. Nếu là Public Asset -> Trả về CDN Public URL cố định
        if (asset.StorageBucket == "wordsoul-public-assets")
        {
            return asset.PublicUrl;
        }

        // 2. Nếu là Private Asset -> Generates Signed Presigned URL với TTL = 15 phút
        return _blobStorageProvider.GeneratePresignedUrl(
            bucket: asset.StorageBucket,
            path: asset.StoragePath,
            expiration: TimeSpan.FromMinutes(15)
        );
    }
}
```

## 5. Quy trình Kiểm tra Bản quyền trước Nạp Tài sản (REL-04 Ingestion Gate)

```
[Upload / Ingest New Digital Asset Request]
                     |
                     v
      [Validate MIME Type & FileSize]
                     |
         +-----------+-----------+
         | (Invalid)             | (Valid)
         v                       v
    [Reject Upload]    [Check License Metadata (REL-04)]
    - M12 Result 400   - Must have licenseId & licenseType
                       - Set rightsCleared == true
                                 |
                     +-----------+-----------+
                     | (Fail)                | (Pass)
                     v                       v
            [Mark rightsCleared=false]  [Mark rightsCleared=true]
            - Block Publish (CT-01)     - Allow Public CDN
```

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AC-G01` | 100% tài sản số trong hệ thống thuộc đúng 1 trong 8 loại AssetType tiêu chuẩn. |
| `AC-G02` | Mọi tài sản số có đầy đủ `assetId`, `mimeType`, `fileSizeBytes` và `checksumSha256`. |
| `AC-G03` | Kiểm tra kích thước file (`fileSizeBytes`) và định dạng `mimeType` nghiêm ngặt khi nạp tài sản. |
| `AC-G04` | Tài sản thuộc Bucket Riêng tư (`wordsoul-private-assets`) chỉ cung cấp qua Presigned URL (TTL $\le 15\text{m}$). |
| `AC-G05` | 100% tài sản phương tiện xuất bản công khai có `rightsCleared == true` tuân thủ REL-04 và CT-01. |
| `AC-G06` | Tài sản tạm thời (`AUDIO_PRONUNCIATION_ATTEMPT`) được tự động dọn dẹp sau 14 ngày. |
| `AC-G07` | Cấm tải lên các file thực thi nguy hại (`.exe`, `.sh`, `.php`, `.js`) vào các bucket lưu trữ. |
| `AC-G08` | Mọi thao tác nạp, xóa, cập nhật tài sản ghi vết Audit Event bất biến trong M11. |
| `AC-G09` | Phân quyền nạp/xóa tài sản tuân thủ nghiêm ngặt ma trận vai trò M11 (`R08 Operations Admin`). |
| `AC-G10` | 100% các test case tự kiểm AC21-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AC21-01` | Nạp file âm thanh từ vựng MP3 dung lượng $300\text{KB}$ | Nạp thành công vào `AUDIO_HEADWORD`, trả về `publicUrl` |
| `AC21-02` | Thử nạp file âm thanh MP3 dung lượng $1.2\text{MB}$ ($> 500\text{KB}$) | Reject với lỗi `FILE_SIZE_EXCEEDS_LIMIT` |
| `AC21-03` | Thử nạp file script độc hại `malicious.sh` | Reject với lỗi `UNSUPPORTED_MIME_TYPE` |
| `AC21-04` | Yêu cầu lấy URL cho file ghi âm phát âm của người học | Trả về Signed Presigned URL có thời hạn Hết hạn = 15 phút |
| `AC21-05` | Thử dùng Presigned URL đã hết hạn 20 phút | Storage Provider deny 403 Forbidden |
| `AC21-06` | Nạp tài sản hình ảnh có thông tin bản quyền `CC-BY-4.0` | Đặt `rightsCleared = true` thành công |
| `AC21-07` | Nạp tài sản hình ảnh thiếu thông tin bản quyền | Đặt `rightsCleared = false`, cảnh báo vi phạm bản quyền (REL-04) |
| `AC21-08` | Thử đính kèm tài sản `rightsCleared = false` vào từ vựng `published` | Reject xuất bản từ vựng theo CT-01 |
| `AC21-09` | Chạy job dọn dẹp tài sản tạm thời quá 14 ngày | Xóa an toàn các file `AUDIO_PRONUNCIATION_ATTEMPT` hết hạn |
| `AC21-10` | Chạy job dọn dẹp tài sản `AUDIO_HEADWORD` vĩnh viễn | Deny operation; tài sản học liệu chính giữ vĩnh viễn |
| `AC21-11` | Nạp file ảnh bìa bộ từ WebP dung lượng $2.5\text{MB}$ | Nạp thành công vào `IMAGE_SET_COVER` |
| `AC21-12` | Nạp file PDF điều khoản dung lượng $8.0\text{MB}$ | Nạp thành công vào `DOC_TERMS_PDF` |
| `AC21-13` | Tính toán mã băm SHA-256 cho file tải lên | Mã băm khớp 100% với `checksumSha256` trong CSDL |
| `AC21-14` | Tải đồng thời 50 request sinh Presigned URL | Response p95 $< 25\text{ms}$ |
| `AC21-15` | Xóa một tài sản số không còn tham chiếu | Xóa file trên Storage Bucket thành công, ghi audit log |
| `AC21-16` | User không phải Admin thực hiện nạp tài sản hệ thống | Deny 403 Forbidden |
| `AC21-17` | Kiểm tra tính tương thích CDN cho Public URL | URL hỗ trợ HTTP/2, HTTPS và CDN edge caching |
| `AC21-18` | Nạp tài sản trang phục thú cưng SVG | Nạp thành công vào `IMAGE_PET_SKIN` |
| `AC21-19` | Phân tích tham chiếu trước khi xóa một tài sản số | Quét các từ vựng M02 đang trỏ tới tài sản đó (T020) |
| `AC21-20` | Kiểm thử hoàn tất luồng danh mục loại tài sản số M12-ASSET-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-AC-I01` | Trong `WordSoulApi`, chưa có bảng `DigitalAssets` lưu metadata tài sản số | URL âm thanh/hình ảnh đang lưu chuỗi trực tiếp trên entity `Vocabulary` | M12-T049 (Source task) |
| `M12-AC-I02` | Thiếu bộ kiểm tra dung lượng `fileSizeBytes` và định dạng `mimeType` chuẩn | Rủi ro nạp file quá lớn hoặc file script nguy hại | M12-T049 |
| `M12-AC-I03` | Chưa phân tách giữa Public Bucket và Private Bucket với Presigned URL | File ghi âm phát âm của người dùng có nguy cơ bị lộ công khai | M12-T049 |
| `M12-AC-I04` | Thiếu metadata bản quyền (`licenseId`, `rightsCleared`) trên tài sản phương tiện | Rủi ro vi phạm bản quyền tác giả theo REL-04 | M12-T049; REL-04 |
| `M12-AC-I05` | Chưa có Cron Job tự động dọn dẹp các tài sản tạm thời (`AUDIO_PRONUNCIATION_ATTEMPT`) | Dung lượng lưu trữ bị lãng phí do tích tụ file tạm | M12-T049 |

- `M12-AC-F01`: Tạo bảng CSDL `DigitalAssets` và DTO quản lý tài sản số chuẩn (tiếp nhận: M12-T049).
- `M12-AC-F02`: Triển khai `AssetStorageProvider` hỗ trợ Public CDN & Signed Presigned URL (tiếp nhận: M12-T049).
- `M12-AC-F03`: Cài đặt `AssetValidationMiddleware` kiểm tra MIME & MaxSize (tiếp nhận: M12-T049).
- `M12-AC-F04`: Thiết lập bộ kiểm thử tự động AC-G01–G10 và AC21-01–20 (tiếp nhận: M12 tasks).
- `M12-AC-F05`: Thu thập bằng chứng runtime cho luồng tài sản số M12 (tiếp nhận: M12 tasks; A-G03/A-G05).

## 8. Tự kiểm M12-T021

- Đã thiết kế hoàn chỉnh `M12-ASSET-CATALOG-1.0` với Danh mục 8 loại tài sản số tiêu chuẩn.
- Đã chốt quy tắc phân tách Public Bucket (CDN) và Private Bucket (Signed Presigned URL TTL $\le 15\text{m}$).
- Đã lồng ghép cơ chế kiểm tra bản quyền REL-04 & CT-01 (`rightsCleared == true`).
- Đã quy định chính sách dọn dẹp tự động đối với tài sản tạm thời.
- Đã xác lập 10 Regression Gates (`AC-G01`–`AC-G10`) và 20 Test Cases tự kiểm (`AC21-01`–`AC21-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả danh mục loại tài sản số M12-T021 | WSA-7K2 |
