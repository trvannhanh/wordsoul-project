# Từ điển học liệu M02

| Thuộc tính | Giá trị |
|---|---|
| Dictionary ID | `M02-VOCAB-DICT-1.0` |
| Task | M02-T001 |
| Đầu vào | M01-T001, PROJECT.md |
| Phạm vi | 35 thuật ngữ chuẩn hóa, ranh giới trách nhiệm module, 8 quy tắc giải quyết xung đột miền và chuẩn schema cho M02 |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Từ điển học liệu M02 (Learning Vocabulary Dictionary) xác lập hệ thuật ngữ chuẩn hóa, mô hình thực thể mục từ, ranh giới trách nhiệm giữa Module M02 với các module liên quan (M01, M03, M04, M05, M06, M08, M11, M12) và quy tắc nhất quán dữ liệu học liệu trong toàn bộ hệ thống WordSoul.

- **Định danh Từ vựng Chuẩn hóa (`Headword Canonicalization`)**: Mặt chữ gốc (`WordCanonical`) là dạng chuẩn viết thường, loại bỏ khoảng trắng thừa/dấu phụ không cần thiết, đóng vai trò khóa định danh chuẩn hóa.
- **Tách biệt Mục từ Tổng thể và Bộ từ vựng**: Mục từ tổng thể (`Master Headword`) thuộc Từ điển Học liệu dùng chung hệ thống. Một Mục từ có thể xuất hiện trong nhiều Bộ từ vựng (`Vocabulary Set`) mà không nhân bản dữ liệu định nghĩa.
- **Tách biệt Nghĩa và Ví dụ (Multi-Sense & Context Isolation)**: Một mục từ có thể có nhiều nét nghĩa (`VocabularySense`) và nhiều ví dụ ngữ cảnh tương ứng. Không trộn lẫn nhiều nghĩa vào một chuỗi thô đơn lẻ.
- **Xác minh Bản quyền Phương tiện (`REL-04 / CT-01 / A-G03`)**: File âm thanh phát âm và hình ảnh minh họa đính kèm mục từ phải có `LicenseId` và trạng thái xác minh bản quyền (`rightsCleared == true`). Cấm xuất bản học liệu có phương tiện chưa clear bản quyền ra ứng dụng công khai (CT-01).

## 2. Danh mục 35 thuật ngữ chuẩn hóa M02

| STT | Thuật ngữ tiếng Việt | Thuật ngữ tiếng Anh | Định nghĩa & Ràng buộc chuẩn |
|---|---|---|---|
| 1 | Mục từ | Headword / Vocabulary Item | Đơn vị từ vựng cơ sở trong từ điển học liệu, chứa thông tin mặt chữ gốc, phát âm và các nét nghĩa. |
| 2 | Mặt chữ gốc | Word Canonical | Dạng mặt chữ chuẩn hóa của từ (loại bỏ ký tự lạ, chuẩn hóa unicode) làm định danh tra cứu. |
| 3 | Nét nghĩa | Vocabulary Sense | Một nét nghĩa cụ thể của mục từ gắn liền với định nghĩa tiếng Việt, loại từ và ví dụ. |
| 4 | Loại từ | Part of Speech (POS) | Phân loại ngữ pháp của từ (Noun, Verb, Adjective, Adverb, Preposition, Conjunction, Pronoun, Interjection). |
| 5 | Cấp độ khung Châu Âu | CEFR Level | Cấp độ năng lực ngôn ngữ theo chuẩn CEFR (A1, A2, B1, B2, C1, C2). |
| 6 | Phiên âm quốc tế | Pronunciation IPA | Chuỗi ký tự phiên âm ngữ âm quốc tế (vd: `/wɜːrd/`). |
| 7 | Bộ từ vựng | Vocabulary Set | Tập hợp gồm nhiều mục từ được nhóm theo chủ đề, cấp độ hoặc mục tiêu học tập. |
| 8 | Bộ từ hệ thống | System / Public Set | Bộ từ chính thức do ban biên tập công bố công khai cho toàn bộ người học. |
| 9 | Bộ từ cá nhân | Custom / User Set | Bộ từ do cá nhân người học tự tạo phục vụ mục tiêu học tập riêng. |
| 10 | Bộ từ nhập môn | Starter Set | Bộ từ mặc định được đề xuất cho người dùng mới trong luồng Onboarding (M01-T008 / M06-ONB-A-1.0). |
| 11 | Xem trước AI | AI Vocabulary Preview | Bản nạp nội dung gợi ý mục từ do AI tạo ra trước khi người biên soạn xác nhận lưu (M02-T015). |
| 12 | Âm thanh phát âm | Pronunciation Audio | File âm thanh chuẩn giọng đọc (TTS hoặc thu âm) phát âm cho từ gốc. |
| 13 | Âm thanh câu ví dụ | Sentence Audio | File âm thanh đọc mẫu cho câu ví dụ ngữ cảnh. |
| 14 | Hình ảnh minh họa | Vocabulary Image | File hình ảnh trực quan minh họa nghĩa của mục từ. |
| 15 | Giấy phép bản quyền | Content License / REL-04 | Bản ghi xác nhận quyền sở hữu tác giả hoặc giấy phép sử dụng hợp pháp của tài sản học liệu. |
| 16 | Chuẩn chất lượng | Quality Standard | Bộ tiêu chí đánh giá mục từ đạt điều kiện phê duyệt xuất bản (M02-T006). |
| 17 | Trạng thái vòng đời | Content Lifecycle State | Trạng thái của mục từ/bộ từ: `draft`, `submitted`, `approved`, `published`, `deprecated`, `archived`. |
| 18 | Thu hồi học liệu | Content Recall | Lệnh gỡ bỏ khẩn cấp học liệu công khai khi phát hiện sai sót hoặc vi phạm bản quyền. |
| 19 | Lịch sử biên tập | Vocabulary Revision History | Nhật ký ghi nhận các phiên bản chỉnh sửa của mục từ qua thời gian. |
| 20 | Tỷ lệ hoàn thành bộ | Set Completion Rate | Tỷ lệ % số từ người học đã đạt trạng thái ghi nhớ trong một bộ từ cụ thể. |
| 21 | Biến thể hình thái | Morphological Variant | Các dạng biến thể ngữ pháp của từ (số nhiều, thì quá khứ, dạng so sánh). |
| 22 | Từ đồng nghĩa | Synonym | Các từ có nghĩa tương tự nhau trong ngữ cảnh cụ thể. |
| 23 | Từ trái nghĩa | Antonym | Các từ có nghĩa đối lập nhau. |
| 24 | Cụm từ cố định | Collocation | Nhóm các từ thường đi kèm với nhau theo thói quen bản ngữ. |
| 25 | Độ khó mục từ | Item Difficulty Score | Chỉ số đánh giá độ khó thực tế của từ dựa trên dữ liệu phản hồi của người học. |
| 26 | Ghi đè nghĩa theo bộ | Set-Level Sense Override | Việc chọn hoặc tùy chỉnh nét nghĩa ưu tiên của từ khi đưa vào một bộ từ cụ thể. |
| 27 | Từ điển dùng chung | Master Vocabulary Catalog | Kho lưu trữ toàn bộ các mục từ chính thức của hệ thống WordSoul. |
| 28 | Phân tích phụ thuộc | Dependency Analysis | Quét kiểm tra tác động đến các bộ từ và phiên học khi một mục từ bị thay đổi/xóa. |
| 29 | Thẻ chủ đề | Topic Tag | Nhãn phân loại chủ đề (vd: Business, Travel, Technology, Academic). |
| 30 | Điểm chất lượng | Content Quality Score | Điểm đánh giá tổng hợp mức độ hoàn thiện của mục từ (0-100%). |
| 31 | Định danh tài sản | Asset Reference ID | Định danh bất biến của file âm thanh/hình ảnh đính kèm theo chuẩn M12. |
| 32 | Thu thập tự động | Auto-Ingested Content | Học liệu được nạp từ các nguồn dữ liệu bên ngoài qua pipeline M12. |
| 33 | Gắn phần thưởng bộ | Set Mastery Reward | Phần thưởng (vật phẩm, danh hiệu) cấp cho người học khi hoàn thành bộ từ (M06/M07). |
| 34 | Khóa sửa đổi | Content Edit Lease | Khóa mềm ngắn hạn ngăn 2 người cùng sửa một bản thảo mục từ (M11-T021). |
| 35 | Bản ghi đối soát | Vocabulary Integrity Record | Bản ghi kiểm tra sự khớp nối dữ liệu từ vựng giữa M02, M03 và M04. |

## 3. Ranh giới sở hữu giữa M02 và các Module liên quan

| Module | M02 sở hữu (`M02 Owns`) | Module khác sở hữu (`Other Module Owns`) | Hợp đồng ranh giới |
|---|---|---|---|
| **M01 Identity** | Gắn `CreatorId` cho bộ từ cá nhân, phân quyền sửa bộ từ | Thông tin tài khoản, vai trò, phiên làm việc người dùng | `M01-SESSION-1.0`, `M01-PERM-1.0` |
| **M03 Session** | Cung cấp định nghĩa, âm thanh, hình ảnh mục từ cho phiên học | Quyết định logic điều phối câu hỏi, chấm điểm trả lời phiên học | `M02-LESSON-CONTENT-1.0` |
| **M04 Progress** | Cung cấp danh mục mục từ chuẩn và thông tin độ khó | Tiến độ ôn tập từng cá nhân (`nextReviewAt`, `interval`, `masteryLevel`) | `M04-SRS-PROGRESS-1.0` |
| **M05 Voice** | Cung cấp chuỗi IPA và văn bản mẫu câu ví dụ | Đánh giá điểm phát âm âm thanh thô của người dùng | `M05-PRONUNCIATION-1.0` |
| **M06 Economy** | Định nghĩa metadata phần thưởng bộ từ | Thực thi cộng điểm XP, cấp vật phẩm vào túi đồ người dùng | `M06-LEDGER-1.0`, `D-011` |
| **M08 Battle** | Cung cấp danh sách từ vựng phục vụ thi đấu PvP | Quản lý phòng đấu, matchmaking, tính điểm trận đấu | `M08-BATTLE-1.0` |
| **M11 Admin** | Thực thi workflow duyệt bản thảo, định nghĩa nội dung | Cấu hình hệ thống, cấp quyền admin, ghi audit event | `M11-CROSS-CONTENT-MATRIX-1.0` |
| **M12 Infra** | Truyền metadata tài sản và yêu cầu AI generate | Quản lý lưu trữ blob, provider AI, TTS engine, cấp URL signed | `M12-CAP-REG-1.0`, `M12-CONTRACT-1.0` |

## 4. Quy tắc Giải quyết 8 Xung đột Miền M02

1. **Xung đột Chuẩn hóa Mặt chữ (Canonical vs Case/Accent Variants)**:
   - *Quy tắc*: `WordCanonical` luôn lưu dưới dạng viết thường (lowercase), trim khoảng trắng. Các dạng viết hoa (Proper Nouns) hoặc biến thể giữ trong metadata `DisplayWord`. Cấm tạo 2 mục từ phân biệt chỉ bằng chữ hoa/thường (vd: `apple` và `Apple`).
2. **Xung đột Một từ nhiều Nghĩa (Multi-Sense Structure)**:
   - *Quy tắc*: Một mục từ `Vocabulary` chứa một danh sách các nét nghĩa `VocabularySense`. Mỗi `VocabularySense` có loại từ (`POS`), định nghĩa tiếng Việt và câu ví dụ riêng. Không nối các nghĩa thành chuỗi duy nhất bằng dấu phẩy.
3. **Xung đột Bộ từ Cá nhân vs Bộ từ Hệ thống**:
   - *Quy tắc*: Bộ từ cá nhân (`IsCustom == true`) chỉ hiển thị cho người tạo. Khi bộ từ cá nhân được biên tập viên chọn công khai, hệ thống copy dữ liệu thành Bộ từ Hệ thống mới với `IsCustom = false` và thực hiện kiểm duyệt đầy đủ.
4. **Xung đột Ghi đè Nghĩa theo Bộ từ (Set-Level Override)**:
   - *Quy tắc*: Mục từ gốc trong Từ điển Master giữ đầy đủ các nét nghĩa. Khi gán từ vào một Bộ từ cụ thể, có thể chỉ định `SelectedSenseIds` để chọn nét nghĩa phù hợp với chủ đề của bộ từ đó.
5. **Xung đột Thay đổi Từ đã được người dùng học (Historical Progress Integrity)**:
   - *Quy tắc*: Khi sửa định nghĩa của một `VocabularyId` đã có người dùng học (M04), KHÔNG xóa hoặc đổi ID mục từ. Nếu thay đổi làm đổi hẳn bản chất từ, tạo `VocabularyId` mới và đánh dấu từ cũ là `Deprecated`.
6. **Xung đột Tài sản Phương tiện thiếu Bản quyền (REL-04 / CT-01)**:
   - *Quy tắc*: Nếu một mục từ chứa file âm thanh hoặc hình ảnh chưa có `rightsCleared == true`, mục từ đó KHÔNG được phép chuyển sang trạng thái `published` (CT-01).
7. **Xung đột Tạo nội dung bằng AI (AI Ingestion vs Human Approval)**:
   - *Quy tắc*: Kết quả AI sinh ra (`VocabularyPreviewDto`) chỉ là bản tạm thời (`transient preview`). Cấm tự động chèn trực tiếp dữ liệu AI vào Master Dictionary mà không qua bước biên tập viên xác nhận.
8. **Xung đột Trùng lặp Mục từ khi tạo bộ (Duplicate Ingestion Guard)**:
   - *Quy tắc*: Khi nạp bộ từ mới, hệ thống tự động kiểm tra `WordCanonical` với Master Dictionary. Nếu từ đã tồn tại, liên kết tới `VocabularyId` hiện có thay vì tạo từ trùng lặp.

## 5. Chuẩn Schema Thực thể Cơ sở M02

### 5.1. Master Vocabulary Record Schema
```json
{
  "vocabularyId": 1024,
  "wordCanonical": "vocabulary",
  "displayWord": "Vocabulary",
  "pronunciationIpa": "/vəˈkæbjələri/",
  "partOfSpeech": "Noun",
  "cefrLevel": "B1",
  "status": "published",
  "isCustom": false,
  "creatorId": null,
  "audioUrl": "https://assets.wordsoul.app/audio/vocab/vocabulary.mp3",
  "imageUrl": "https://assets.wordsoul.app/images/vocab/vocabulary.jpg",
  "licenseId": "LIC-2026-CC-BY-001",
  "rightsCleared": true,
  "createdAtUtc": "2026-08-20T10:00:00Z",
  "updatedAtUtc": "2026-08-20T10:00:00Z"
}
```

### 5.2. Vocabulary Sense Schema
```json
{
  "senseId": 5012,
  "vocabularyId": 1024,
  "senseOrder": 1,
  "partOfSpeech": "Noun",
  "definitionVi": "Tập hợp các từ ngữ trong một ngôn ngữ hoặc thuộc một lĩnh vực cụ thể.",
  "definitionEn": "All the words used in a particular language or subject.",
  "exampleSentenceEn": "Reading books is a great way to expand your vocabulary.",
  "exampleSentenceVi": "Đọc sách là một cách tuyệt vời để mở rộng vốn từ vựng của bạn.",
  "exampleSentenceAudioUrl": "https://assets.wordsoul.app/audio/examples/ex-5012.mp3"
}
```

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `VD-G01` | Đủ 35 thuật ngữ chuẩn hóa M02 với định nghĩa và ràng buộc không nhập nhằng. |
| `VD-G02` | `WordCanonical` được chuẩn hóa viết thường, trim khoảng trắng, không trùng lặp mục từ Master. |
| `VD-G03` | Cấu trúc một mục từ tách biệt các nét nghĩa `VocabularySense`, không nối chuỗi thô. |
| `VD-G04` | 100% tài sản phương tiện (Audio/Image) xuất bản có `rightsCleared == true` tuân thủ REL-04 và CT-01. |
| `VD-G05` | Ranh giới sở hữu dữ liệu giữa M02 với M01, M03, M04, M05, M06, M08, M11, M12 minh bạch. |
| `VD-G06` | Bộ từ cá nhân (`IsCustom == true`) được cách ly hoàn toàn với Bộ từ Hệ thống công khai. |
| `VD-G07` | Kết quả AI nạp từ (`AI Preview`) KHÔNG được tự động chèn vào Master DB mà không qua duyệt. |
| `VD-G08` | Thay đổi định nghĩa từ vựng không làm hỏng dữ liệu tiến độ lịch sử học tập M04. |
| `VD-G09` | Phân quyền biên tập và kiểm duyệt nội dung tuân thủ nghiêm ngặt ma trận vai trò M11. |
| `VD-G10` | 100% các test case tự kiểm VD01-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VD01-01` | Tạo mục từ mới với mặt chữ `  Vocabulary  ` | Tự động trim và chuyển thành `wordCanonical = "vocabulary"` |
| `VD01-02` | Thử tạo 2 mục từ master trùng `wordCanonical = "apple"` | System reject với lỗi `DUPLICATE_MASTER_HEADWORD` |
| `VD01-03` | Nạp một mục từ có 2 nét nghĩa (Danh từ và Động từ) | Lưu thành công 1 `Vocabulary` và 2 `VocabularySense` |
| `VD01-04` | Xuất bản mục từ có file âm thanh chưa xác minh bản quyền (CT-01) | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` |
| `VD01-05` | User tạo bộ từ cá nhân với 10 mục từ custom | Tạo bộ từ thành công với `IsCustom = true`, `CreatorId = UserId` |
| `VD01-06` | User khác truy cập bộ từ cá nhân của người dùng trên | System deny 403 Forbidden |
| `VD01-07` | Gán 1 mục từ master vào 2 bộ từ khác nhau | Cả 2 bộ từ trỏ chung `VocabularyId`, không nhân bản từ |
| `VD01-08` | AI nạp gợi ý danh sách từ vựng cho chủ đề "Travel" | Trả về `VocabularyPreviewDto`, chưa ghi DB master |
| `VD01-09` | Biên tập viên duyệt bản nạp AI và lưu vào hệ thống | Lưu vào Master DB thành công sau khi kiểm duyệt |
| `VD01-10` | Đổi định nghĩa của một từ đã có 500 người dùng học ở M04 | Cập nhật định nghĩa, `VocabularyId` giữ nguyên, M04 progress an toàn |
| `VD01-11` | Thử gán giá trị `CEFRLevel` không hợp lệ | System reject với lỗi `INVALID_CEFR_LEVEL` |
| `VD01-12` | Ghi đè nét nghĩa ưu tiên của từ khi gán vào Bộ từ | Bộ từ chỉ hiển thị nét nghĩa được chọn `SelectedSenseIds` |
| `VD01-13` | Tra cứu mục từ theo từ khóa không phân biệt hoa thường | Tìm thấy mục từ chính xác |
| `VD01-14` | Xóa một bộ từ hệ thống | Bộ từ bị đánh dấu `archived`, các mục từ Master bên trong vẫn an toàn |
| `VD01-15` | Thử tạo mục từ thiếu thông tin `PartOfSpeech` | System reject với lỗi `MISSING_PART_OF_SPEECH` |
| `VD01-16` | Kiểm tra tính tương thích giữa `PronunciationUrl` và chuẩn M12 | URL là HTTPS signed URL hợp lệ từ M12 |
| `VD01-17` | Biên tập viên công khai một bộ từ cá nhân của người dùng | Copy thành Bộ từ Hệ thống mới với `IsCustom = false` |
| `VD01-18` | Tải đồng thời 50 request tra cứu từ điển master | p95 latency $< 50\text{ms}$ từ Redis cache |
| `VD01-19` | Xuất bản mục từ có đầy đủ âm thanh và hình ảnh đã clear REL-04 | Chuyển trạng thái `published` thành công |
| `VD01-20` | Kiểm thử hoàn toàn bộ quy tắc từ điển học liệu M02-VOCAB-DICT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-VD-I01` | Entity `Vocabulary.cs` hiện tại lưu `Meaning` và `ExampleSentence` dạng chuỗi đơn | Chưa tách biệt entity `VocabularySense` cho mô hình nhiều nghĩa | M02-T002 |
| `M02-VD-I02` | `Vocabulary.cs` chưa có trường `WordCanonical` chuẩn hóa | Rủi ro tạo từ trùng lặp do hoa/thường hoặc khoảng trắng | M02-T003 |
| `M02-VD-I03` | Chưa có các trường `Status`, `LicenseId`, `RightsCleared` trong `Vocabulary.cs` | Chưa đáp ứng hợp đồng bản quyền REL-04 và CT-01 | M02-T006; REL-04 |
| `M02-VD-I04` | `VocabularyService.cs` nạp AI preview gán trực tiếp vào DTO mà không qua kiểm tra trùng Master DB | Có thể nhân bản mục từ đã tồn tại trong từ điển hệ thống | M02-T004 |
| `M02-VD-I05` | Chưa có cơ chế ghi đè nét nghĩa theo bộ từ (`Set-Level Sense Override`) | Bộ từ hiển thị toàn bộ nét nghĩa gây nhiễu ngữ cảnh | M02-T015 |

- `M02-VD-F01`: Nâng cấp Entity `Vocabulary.cs` và tạo mới `VocabularySense.cs` (tiếp nhận: M02-T002).
- `M02-VD-F02`: Xây dựng service chuẩn hóa `WordCanonical` và phát hiện trùng lặp (tiếp nhận: M02-T003, M02-T004).
- `M02-VD-F03`: Thêm metadata bản quyền REL-04 vào schema từ vựng (tiếp nhận: M02-T006; REL-04).
- `M02-VD-F04`: Thiết lập bộ kiểm thử tự động VD-G01–G10 và VD01-01–20 (tiếp nhận: M02 tasks).
- `M02-VD-F05`: Thu thập bằng chứng runtime cho luồng từ điển học liệu M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T001

- Đã thiết kế hoàn chỉnh `M02-VOCAB-DICT-1.0` bao phủ đủ 35 thuật ngữ chuẩn hóa học liệu.
- Đã chốt ranh giới sở hữu giữa M02 và các module liên quan M01–M12.
- Đã đưa ra quy tắc giải quyết 8 xung đột miền M02 (chuẩn hóa mặt chữ, multi-sense, custom vs system sets, REL-04 license check).
- Đã xác lập chuẩn Schema cho Master Vocabulary và Vocabulary Sense.
- Đã xác lập 10 Regression Gates (`VD-G01`–`VD-G10`) và 20 Test Cases tự kiểm (`VD01-01`–`VD01-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task M02 tiếp theo.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo từ điển học liệu M02-VOCAB-DICT-1.0 | WSA-7K2 |
