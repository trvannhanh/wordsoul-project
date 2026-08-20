# Chuẩn hóa nội dung cung cấp module học — Lát A M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-LESSON-CONTENT-1.0` |
| Task | M02-T009-A |
| Đầu vào | M02-MULTI-SENSE-1.0, M02-ITEM-QUALITY-1.0, M02-HEADWORD-VERSIONING-1.0 |
| Phạm vi | Hợp đồng Payload DTO cung cấp học liệu từ vựng cho Module Học M03, quy tắc kiểm tra chất lượng trước khi nạp và chiến lược cache sub-50ms |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Hợp đồng Giao diện DTO Cung cấp Nội dung Học liệu (`Lesson Content Payload Contract`) do M02 xuất bản cho Module Phiên học & Kiểm tra M03 (Lesson & Quiz Engine), bảo đảm tính chính xác, nhất quán về nét nghĩa, âm thanh, hình ảnh và hiệu năng nạp dữ liệu.

- **Chỉ Cung cấp Học liệu Đã Xuất bản (`Published Content Only`)**: M02 CHỈ CUNG CẤP cho M03 các mục từ vựng đang ở trạng thái `Published`, có `QualityScore >= 80%` (M02-T006) và `rightsCleared == true` (REL-04 / CT-01). CẤM đưa các mục từ `Draft`, `InReview` hoặc vi phạm bản quyền vào phiên học public.
- **Lọc Nét nghĩa Ngữ cảnh theo Bộ từ (`Contextual Sense Filtering`)**: Payload trả về cho M03 chỉ chứa nét nghĩa `VocabularySense` được chọn trong `SetVocabulary.SelectedSenseIds`. Nếu không chỉ định, fallback trả về nét nghĩa chính (`SenseOrder == 1`).
- **Ghim Mã băm Phiên bản Nội dung (`Revision Digest Pinning`)**: Payload phiên học chứa `revisionDigest` bất biến của mục từ (M02-T008-A). Khi bài học đang chạy, thông tin nét nghĩa và ví dụ không bị thay đổi bất ngờ.
- **Đạt SLA Latency Sub-50ms (`Sub-50ms Payload Caching`)**: Toàn bộ DTO gói bài học cho một Bộ từ vựng được pre-generate và lưu trong Redis Cache (`lesson_payload:{setId}:{revisionDigest}`) với latency phản hồi $p95 < 50\text{ms}$.

## 2. Chuẩn DTO Payload Bài học M02 cung cấp cho M03 (JSON Schema)

```json
{
  "setId": 108,
  "setTitle": "Từ vựng Giao tiếp Tiếng Anh B1 - Chủ đề Du lịch",
  "setDifficultyIndex": 5.2,
  "totalItems": 15,
  "items": [
    {
      "vocabularyId": 1024,
      "wordCanonical": "vocabulary",
      "displayWord": "Vocabulary",
      "pronunciationIpa": "/vəˈkæbjələri/",
      "cefrLevel": "B1",
      "audioUrl": "https://assets.wordsoul.app/audio/vocab/vocabulary.mp3",
      "imageUrl": "https://assets.wordsoul.app/images/vocab/vocabulary.jpg",
      "revisionDigest": "e4d909c290d0fb1ca068ffaddf22cbd0",
      "activeSense": {
        "senseId": 5012,
        "senseOrder": 1,
        "partOfSpeech": "Noun",
        "definitionVi": "Tập hợp các từ ngữ trong một ngôn ngữ.",
        "definitionEn": "All the words used in a particular language.",
        "exampleSentenceEn": "Reading books expands your vocabulary.",
        "exampleSentenceVi": "Đọc sách mở rộng vốn từ vựng của bạn.",
        "exampleAudioUrl": "https://assets.wordsoul.app/audio/examples/ex-5012.mp3"
      },
      "acceptedQuizVariants": ["vocabularies"]
    }
  ]
}
```

## 3. Quy trình Kiểm soát Chất lượng trước khi Tạo Payload

```
[Request Session Content for SetId]
                 |
                 v
   (Query Published Items in Set)
                 |
                 v
  [Check Item Quality & Rights Gate]
  - Status == 'Published'
  - QualityScore >= 80%
  - rightsCleared == true (REL-04)
                 |
     +-----------+-----------+
     | (Fail)                | (Pass)
     v                       v
 [Filter / Exclude Item]  [Map to LessonVocabularyItemDto]
                             |
                             v
                  [Build Redis Cache Key]
                  `lesson_payload:{setId}:{revisionDigest}`
                             |
                             v
                  [Return Sub-50ms Payload to M03]
```

## 4. Chiến lược Caching và Xóa Cache Dữ liệu Bài học (Caching Strategy)

1. **Cấu trúc Key Redis**: `lesson_payload:{setId}:{setRevisionDigest}`.
2. **Thời gian lưu Cache (TTL)**: 24 giờ.
3. **Cơ chế Vô hiệu hóa Cache (Invalidation Events)**:
   - Khi có 1 từ trong bộ chuyển trạng thái `Deprecated` / `Recalled` / `Archived`: Phát sự kiện `SetContentInvalidatedEvent` lập tức gỡ cache key tương ứng.
   - Khi Admin cập nhật hoặc rollback một revision trong bộ: Tự động xóa cache key cũ và pre-generate cache key mới.

## 5. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LC-G01` | Payload M02 cung cấp cho M03 tuân thủ 100% JSON Schema `LessonVocabularyItemDto`. |
| `LC-G02` | 100% các từ vựng trong payload có trạng thái `Published`, `QualityScore >= 80%` và `rightsCleared == true`. |
| `LC-G03` | Nét nghĩa `activeSense` được lọc chính xác theo `SetVocabulary.SelectedSenseIds` (fallback `SenseOrder = 1`). |
| `LC-G04` | Payload chứa đầy đủ danh sách `acceptedQuizVariants` phục vụ Động cơ Chấm điểm M03. |
| `LC-G05` | Payload ghim ghim `revisionDigest` bất biến của mục từ tại thời điểm nạp bài. |
| `LC-G06` | Latency truy xuất payload bài học qua API từ Redis Cache đạt $p95 < 50\text{ms}$. |
| `LC-G07` | Khi từ trong bộ bị `Recalled` hoặc `Deprecated`, cache payload tự động bị gỡ bỏ trong $\le 60\text{s}$. |
| `LC-G08` | Cấm đưa mục từ `Draft`, `InReview` hoặc vi phạm bản quyền vào payload cung cấp cho M03. |
| `LC-G09` | Phân quyền yêu cầu khởi tạo payload bài học tuân thủ ma trận vai trò M01/M03. |
| `LC-G10` | 100% các test case tự kiểm LC09-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LC09-01` | Yêu cầu nạp payload cho Bộ từ "Travel B1" | Trả về `LessonContentPayloadDto` chuẩn với đầy đủ từ `Published` |
| `LC09-02` | Bộ từ chứa 1 mục từ có `rightsCleared = false` | Mục từ chưa clear bị tự động loại khỏi payload bài học |
| `LC09-03` | Bộ từ chứa 1 mục từ có `QualityScore = 60%` | Mục từ Substandard bị loại khỏi payload bài học |
| `LC09-04` | Bộ từ chỉ định `SelectedSenseIds = [5012]` cho từ "run" | `activeSense` trong DTO trả về đúng nét nghĩa 5012 |
| `LC09-05` | Bộ từ không chỉ định `SelectedSenseIds` cho từ "run" | `activeSense` fallback trả về nét nghĩa chính `SenseOrder = 1` |
| `LC09-06` | Kiểm tra trường `acceptedQuizVariants` cho từ "run" | Trả về danh sách biến thể `["running", "ran", "runs"]` |
| `LC09-07` | Lấy payload bài học lần 2 từ Redis Cache | Latency phản hồi $< 15\text{ms}$ ($< 50\text{ms}$) |
| `LC09-08` | Thu hồi khẩn cấp (`Recalled`) 1 từ vựng trong bộ | Cache `lesson_payload` bị xóa lập tức, tái sinh payload sạch |
| `LC09-09` | Thử nạp payload cho Bộ từ vựng đang ở trạng thái `Draft` | System reject với lỗi `VOCABULARY_SET_NOT_PUBLISHED` |
| `LC09-10` | Nạp payload chứa từ vựng có audio câu ví dụ | `activeSense.exampleAudioUrl` trả về HTTPS signed URL hợp lệ |
| `LC09-11` | Nạp payload cho bộ từ 100 từ vựng | Trả về danh sách DTO đủ 100 từ trong thời gian $< 40\text{ms}$ |
| `LC09-12` | Admin cập nhật revision mới cho 1 từ vựng trong bộ | Cache cũ bị xóa, payload mới cập nhật `revisionDigest` mới |
| `LC09-13` | Người học chưa đăng nhập yêu cầu lấy bài học bộ từ công khai | Cho phép lấy payload bài học công khai M02 |
| `LC09-14` | Người học yêu cầu lấy payload bộ từ cá nhân của người khác | Deny 403 Forbidden |
| `LC09-15` | Kiểm tra sự khớp nối dữ liệu giữa `displayWord` và `wordCanonical` | `wordCanonical` là dạng lowercase trim chuẩn hóa |
| `LC09-16` | Sửa nét nghĩa tiếng Việt `definitionVi` của một nét nghĩa đang active | Payload mới phản ánh nét nghĩa tiếng Việt vừa sửa |
| `LC09-17` | Tải đồng thời 100 request nạp payload bài học từ M03 | Response p95 $< 35\text{ms}$ từ Redis Cluster |
| `LC09-18` | Bộ từ rỗng không chứa mục từ nào | Return payload với `totalItems = 0`, không crash system |
| `LC09-19` | Phân tích phụ thuộc trước khi ngắt 1 bài học | Quét các active session M03 đang tham chiếu DTO này (T020) |
| `LC09-20` | Kiểm thử hoàn tất luồng chuẩn hóa cung cấp nội dung học M02-LESSON-CONTENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-LC-I01` | DTO `VocabularySetFullDetailDto.cs` hiện tại trả về danh sách từ thô không có `activeSense` | Chưa lọc nét nghĩa ngữ cảnh theo `SelectedSenseIds` | M02-T049 (Source task) |
| `M02-LC-I02` | API lấy dữ liệu bài học chưa lồng ghép kiểm tra `QualityScore >= 80%` và `rightsCleared == true` | Rủi ro nạp nhầm học liệu kém chất lượng/vi phạm bản quyền vào M03 | M02-T049; REL-04 |
| `M02-LC-I03` | Chưa có trường `revisionDigest` ghim phiên bản trong Payload DTO | Rủi ro câu hỏi phiên học bị trôi nội dung khi Admin sửa DB | M02-T049 |
| `M02-LC-I04` | DTO bài học chưa trả về danh sách biến thể chấp nhận `acceptedQuizVariants` | Module M03 phải tự truy vấn biến thể gây chậm hiệu năng | M02-T049 |
| `M02-LC-I05` | Chưa cài đặt Redis Caching cho Payload DTO bộ từ vựng (`lesson_payload:{setId}`) | Phải query DB đệ quy mỗi lần khởi tạo phiên học M03 | M02-T049 |

- `M02-LC-F01`: Tạo `LessonVocabularyItemDto.cs` và `LessonContentPayloadDto.cs` chuẩn (tiếp nhận: M02-T049).
- `M02-LC-F02`: Triển khai `LessonContentBuilderService` với các chốt kiểm tra chất lượng/bản quyền (tiếp nhận: M02-T049; REL-04).
- `M02-LC-F03`: Thiết lập Caching Redis sub-50ms cho Lesson Payload (tiếp nhận: M02-T049).
- `M02-LC-F04`: Thiết lập bộ kiểm thử tự động LC-G01–G10 và LC09-01–20 (tiếp nhận: M02 tasks).
- `M02-LC-F05`: Thu thập bằng chứng runtime cho luồng cung cấp nội dung học M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T009-A

- Đã thiết kế hoàn chỉnh `M02-LESSON-CONTENT-1.0` với JSON Schema `LessonContentPayloadDto` và `LessonVocabularyItemDto`.
- Đã chốt quy tắc kiểm tra chất lượng & bản quyền cứng trước khi cung cấp cho M03 (`QualityScore >= 80%` & `rightsCleared == true`).
- Đã xây dựng cơ chế lọc nét nghĩa ngữ cảnh (`activeSense`) và ghim `revisionDigest` bất biến.
- Đã chốt chiến lược Redis Caching đạt SLA sub-50ms latency cho M03.
- Đã xác lập 10 Regression Gates (`LC-G01`–`LC-G10`) và 20 Test Cases tự kiểm (`LC09-01`–`LC09-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ DTO hiện có và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả nội dung cung cấp module học M02-T009-A | WSA-7K2 |
