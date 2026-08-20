# Lập danh mục tài sản học liệu M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-VOCAB-ASSET-CATALOG-1.0` |
| Task | M02-T011 |
| Đầu vào | M02-ITEM-QUALITY-1.0 (D-061), M12-ASSET-CATALOG-1.0 (D-067), M12-ASSET-RIGHTS-LEDGER-1.0 (D-068), REL-04 |
| Phạm vi | Ánh xạ danh mục tài sản phương tiện học liệu M02 với hạ tầng M12, quy định chuẩn hóa chất lượng file và chốt xác minh bản quyền trước khi gán cho mục từ/bộ từ |
| Tự kiểm | A-G03, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Danh mục và Quy tắc Quản lý Tài sản Phương tiện Học liệu (`Vocabulary Learning Asset Registry`) thuộc M02, liên kết các phương tiện âm thanh và hình ảnh của mục từ/bộ từ vựng với Hạ tầng Tài sản Số M12.

- **Ràng buộc Ánh xạ Định danh Tài sản M12 (`AssetId Foreign Reference Invariant`)**: Mọi tài sản phương tiện gắn với `Vocabulary`, `VocabularySense` hoặc `VocabularySet` bắt buộc trỏ tới một `assetId` hợp lệ được đăng ký trong Sổ Quyền Tài sản M12 (`M12-ASSET-RIGHTS-LEDGER-1.0`).
- **Tuân thủ Bản quyền Cứng trước khi Đính kèm (`REL-04 / CT-01 Verification Gate`)**: Một tài sản phương tiện CHỈ ĐƯỢC PHÉP gán cho một mục từ/bộ từ khi API `VerifyAssetRights(assetId)` trả về `rightsCleared == true`. CẤM đính kèm tài sản chưa được xác minh hoặc đã hết hạn bản quyền.
- **Tiêu chuẩn Kỹ thuật Phương tiện Học liệu (`Media Technical Standards`)**:
  - *Âm thanh Phát âm*: MP3 Mono $64\text{kbps}$, $44.1\text{kHz}$, dung lượng $\le 500\text{KB}$, thời lượng $1 \to 3$ giây.
  - *Âm thanh Câu ví dụ*: MP3 Mono $96\text{kbps}$, dung lượng $\le 1.5\text{MB}$, thời lượng $3 \to 10$ giây.
  - *Hình ảnh Minh họa*: WebP $80\%$ quality, kích thước max $1024 \times 1024\text{px}$, dung lượng $\le 2.0\text{MB}$.
  - *Ảnh bìa Bộ từ*: WebP $85\%$ quality, tỷ lệ $16:9$, kích thước $1280 \times 720\text{px}$, dung lượng $\le 3.0\text{MB}$.
- **Trừ điểm Chất lượng khi Thiếu Tài sản Phương tiện (`QualityScore Impact`)**: Mục từ thiếu phát âm âm thanh bị trừ $25\%$ điểm `QualityScore`, thiếu hình ảnh minh họa bị trừ $15\%$ điểm (M02-T006).

## 2. Bảng Ánh xạ Danh mục Tài sản Phương tiện M02 với M12 (Vocab Asset Mapping Matrix)

| Thuộc tính Entity M02 | Mã M12 AssetType | Định dạng Chuẩn | Dung lượng Max | Đường dẫn CDN Storage | Yêu cầu Bản quyền (REL-04) | Trừ điểm khi thiếu |
|---|---|---|---|---|---|---|
| `Vocabulary.AudioAssetId` | `AUDIO_HEADWORD` | MP3 / OGG | 500 KB | `audio/vocab/{vocabId}.mp3` | `rightsCleared == true` | $-25\%$ QualityScore |
| `VocabularySense.ExampleAudioAssetId` | `AUDIO_EXAMPLE_SENTENCE` | MP3 | 1.5 MB | `audio/examples/{senseId}.mp3` | `rightsCleared == true` | $-10\%$ QualityScore |
| `VocabularySense.ImageAssetId` | `IMAGE_HEADWORD` | WebP / PNG / JPG | 2.0 MB | `images/vocab/{vocabId}.webp` | `rightsCleared == true` | $-15\%$ QualityScore |
| `VocabularySet.CoverImageAssetId` | `IMAGE_SET_COVER` | WebP / JPG | 3.0 MB | `images/sets/{setId}.webp` | `rightsCleared == true` | Chặn xuất bản bộ từ |

## 3. Schema Ánh xạ Tài sản Phương tiện Mục từ (Headword Asset Link Schema)

```json
{
  "vocabularyId": 1024,
  "wordCanonical": "vocabulary",
  "audioAsset": {
    "assetId": "01J5XA00000000000000000001",
    "assetType": "AUDIO_HEADWORD",
    "cdnUrl": "https://cdn.wordsoul.app/audio/vocab/vocab-1024.mp3",
    "licenseId": "LIC-2026-CC-BY-4.0-001",
    "rightsCleared": true
  },
  "senses": [
    {
      "senseId": 5012,
      "imageAsset": {
        "assetId": "01J5XA00000000000000000002",
        "assetType": "IMAGE_HEADWORD",
        "cdnUrl": "https://cdn.wordsoul.app/images/vocab/vocab-1024.webp",
        "licenseId": "LIC-2026-RF-002",
        "rightsCleared": true
      },
      "exampleAudioAsset": {
        "assetId": "01J5XA00000000000000000003",
        "assetType": "AUDIO_EXAMPLE_SENTENCE",
        "cdnUrl": "https://cdn.wordsoul.app/audio/examples/ex-5012.mp3",
        "licenseId": "LIC-2026-CC-BY-4.0-001",
        "rightsCleared": true
      }
    }
  ]
}
```

## 4. Giao thức Đính kèm và Phê duyệt Tài sản Phương tiện (Asset Attachment Protocol)

```
[Attach Media Asset Request (VocabularyId, AssetId)]
                        |
                        v
     [Verify M12 Asset Existence & Status]
                        |
                        v
   [Call AssetRightsService.VerifyRights(assetId)]
                        |
        +---------------+---------------+
        | (rightsCleared == false)      | (rightsCleared == true)
        v                               v
 [REJECT ATTACHMENT]          [LINK ASSET TO VOCABULARY]
 - Return Error DTO           - Set AudioAssetId / ImageAssetId
 - Log REL-04 Violation       - Recalculate QualityScore (M02-T006)
                              - Invalidate Redis Lesson Payload (M02-T009-A)
```

## 5. Quy trình Xử lý khi Tài sản bị Thu hồi hoặc Hết hạn (Asset Revocation Protocol)

Khi M12 phát thông báo Thu hồi Khẩn cấp ($\le 60\text{s}$ Emergency Recall M12-T044-A) cho 1 `assetId`:

1. M02 nhận sự kiện `DigitalAssetRevokedEvent(assetId)`.
2. Tìm tất cả `Vocabulary` và `VocabularySet` đang tham chiếu `assetId` này.
3. Đổi thuộc tính `AudioAssetId` / `ImageAssetId` tương ứng về `null`.
4. Tính toán lại `QualityScore` của mục từ. Nếu `QualityScore < 80%`, tự động chuyển mục từ về trạng thái `InReview` hoặc `Recalled` (M02-T007).
5. Xóa cache payload bài học `lesson_payload:{setId}` trong Redis.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `VA-G01` | 100% tài sản đính kèm mục từ/bộ từ có `assetId` hợp lệ thuộc M12. |
| `VA-G02` | Cấm đính kèm tài sản phương tiện nếu `VerifyAssetRights(assetId)` trả về `rightsCleared == false` (REL-04 / CT-01). |
| `VA-G03` | Âm thanh phát âm mục từ bắt buộc đạt chuẩn MP3/OGG $\le 500\text{KB}$. |
| `VA-G04` | Hình ảnh minh họa bắt buộc đạt chuẩn WebP/PNG/JPG $\le 2.0\text{MB}$. |
| `VA-G05` | Đính kèm hoặc gỡ tài sản phương tiện tự động kích hoạt tính toán lại `QualityScore` (M02-T006). |
| `VA-G06` | Cấm xuất bản mục từ nếu `QualityScore < 80%` do thiếu tài sản phương tiện cốt lõi. |
| `VA-G07` | Khi tài sản M12 bị thu hồi, hệ thống tự động gỡ liên kết trong vòng $\le 60$ giây. |
| `VA-G08` | DTO học liệu cung cấp cho M03 chứa đầy đủ CDN HTTPS Public URL của tài sản. |
| `VA-G09` | Phân quyền đính kèm/thay đổi tài sản học liệu tuân thủ ma trận vai trò M11 (`R03 Content Admin`). |
| `VA-G10` | 100% các test case tự kiểm VA11-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VA11-01` | Đính kèm file phát âm MP3 $300\text{KB}$ đã clear bản quyền cho từ `"run"` | Đính kèm thành công, `QualityScore` tăng $+25\%$ |
| `VA11-02` | Thử đính kèm file phát âm có `rightsCleared = false` | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `VA11-03` | Thử đính kèm file phát âm có dung lượng $800\text{KB}$ ($> 500\text{KB}$) | System reject với lỗi `AUDIO_FILE_SIZE_EXCEEDED` |
| `VA11-04` | Đính kèm hình ảnh WebP $1.2\text{MB}$ cho nét nghĩa 5012 | Đính kèm thành công, `QualityScore` tăng $+15\%$ |
| `VA11-05` | Thử đính kèm file ảnh BMP $5.0\text{MB}$ | System reject với lỗi `UNSUPPORTED_IMAGE_FORMAT` |
| `VA11-06` | Đính kèm ảnh bìa WebP $2.5\text{MB}$ cho Bộ từ vựng 108 | Đính kèm `CoverImageAssetId` cho bộ từ thành công |
| `VA11-07` | Gỡ file phát âm của từ vựng đang ở trạng thái `Published` | `QualityScore` giảm xuống $70\%$, tự động chuyển mục từ về `InReview` |
| `VA11-08` | Nhận sự kiện M12 thu hồi bản quyền của 1 file ảnh minh họa | Tự động đặt `ImageAssetId = null` trong $\le 60\text{s}$ |
| `VA11-09` | Thử gán `assetId` không tồn tại trong CSDL M12 | System reject với lỗi `INVALID_ASSET_ID` |
| `VA11-10` | Nạp payload bài học M03 sau khi cập nhật file âm thanh mới | DTO trả về CDN URL mới, cache Redis được làm sạch |
| `VA11-11` | Thử đính kèm tài sản loại `IMAGE_AVATAR` cho mục từ | Reject với lỗi `INVALID_ASSET_TYPE_FOR_HEADWORD` |
| `VA11-12` | Đính kèm file audio câu ví dụ MP3 $1.0\text{MB}$ | Đính kèm thành công vào `ExampleAudioAssetId` |
| `VA11-13` | Tra cứu danh sách từ vựng đang bị thiếu file âm thanh phát âm | Trả về danh sách từ vựng có `AudioAssetId == null` |
| `VA11-14` | Tải đồng thời 50 request đính kèm tài sản học liệu | Response p95 $< 35\text{ms}$ |
| `VA11-15` | Admin cập nhật lại file âm thanh phát âm tốt hơn | Tự động tăng `VocabularyRevision` mới (M02-T008-A) |
| `VA11-16` | User không có quyền Biên tập viên thực hiện đính kèm tài sản | Deny 403 Forbidden |
| `VA11-17` | Phân tích phụ thuộc trước khi gỡ một file âm thanh học liệu | Quét các bộ từ vựng M02 đang chứa từ vựng đó (T020) |
| `VA11-18` | Đính kèm hình ảnh SVG cho nét nghĩa mục từ | Đính kèm thành công, hiển thị chuẩn trên ứng dụng |
| `VA11-19` | Xem vết Audit Log M11 sau khi đính kèm tài sản phương tiện | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `VA11-20` | Kiểm thử hoàn tất luồng danh mục tài sản học liệu M02-VOCAB-ASSET-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-VA-I01` | In `WordSoulApi`, entity `Vocabulary.cs` chứa chuỗi `AudioUrl` và `ImageUrl` thô | Chưa chuyển sang tham chiếu `AudioAssetId` và `imageAssetId` M12 | M02-T049 (Source task) |
| `M02-VA-I02` | API gắn media chưa gọi `VerifyAssetRights` kiểm tra bản quyền | Rủi ro gắn nhầm tài sản chưa clear bản quyền vào bài học | M02-T049; REL-04 |
| `M02-VA-I03` | Chưa có handler lắng nghe sự kiện `DigitalAssetRevokedEvent` từ M12 | Khi tài sản bị thu hồi bản quyền, M02 không tự động gỡ liên kết | M02-T049 |
| `M02-VA-I04` | Thiếu bộ kiểm tra chuẩn hóa định dạng (MP3/WebP) và dung lượng max khi đính kèm | Rủi ro làm phình dung lượng CSDL và băng thông CDN | M02-T049 |
| `M02-VA-I05` | Chưa lồng ghép việc tính toán lại `QualityScore` khi gỡ media | Từ vựng bị mất media vẫn ở trạng thái `Published` bất hợp lệ | M02-T049 |

- `M02-VA-F01`: Chuyển đổi thuộc tính URL thô sang `AudioAssetId` và `imageAssetId` (tiếp nhận: M02-T049).
- `M02-VA-F02`: Tích hợp `VerifyAssetRights` kiểm tra bản quyền trước khi đính kèm (tiếp nhận: M02-T049; REL-04).
- `M02-VA-F03`: Triển khai `DigitalAssetRevokedEventHandler` gỡ media tự động SLA $\le 60\text{s}$ (tiếp nhận: M02-T049).
- `M02-VA-F04`: Thiết lập bộ kiểm thử tự động VA-G01–G10 và VA11-01–20 (tiếp nhận: M02 tasks).
- `M02-VA-F05`: Thu thập bằng chứng runtime cho luồng tài sản học liệu M02 (tiếp nhận: M02 tasks; A-G03/A-G05).

## 8. Tự kiểm M02-T011

- Đã thiết kế hoàn chỉnh `M02-VOCAB-ASSET-CATALOG-1.0` với Bảng Ánh xạ Tài sản Phương tiện M02 sang M12.
- Đã chốt quy tắc Ràng buộc Ánh xạ Định danh `AssetId` và xác minh `rightsCleared == true` (REL-04 / CT-01).
- Đã quy định chuẩn hóa kỹ thuật định dạng MP3/WebP và dung lượng max cho từng loại phương tiện.
- Đã xây dựng Giao thức Thu hồi Tự động SLA $\le 60\text{s}$ khi M12 thu hồi bản quyền tài sản.
- Đã xác lập 10 Regression Gates (`VA-G01`–`VA-G10`) và 20 Test Cases tự kiểm (`VA11-01`–`VA11-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả danh mục tài sản học liệu M02-T011 | WSA-7K2 |
