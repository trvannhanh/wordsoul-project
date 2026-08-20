# Thiết kế mô hình nhiều nghĩa và loại từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-MULTI-SENSE-1.0` |
| Task | M02-T002 |
| Đầu vào | M02-VOCAB-DICT-1.0, M01-T001 |
| Phạm vi | Chuẩn hóa mô hình thực thể mục từ nhiều nghĩa (1-to-N VocabularySense), phân loại từ (POS) và cơ chế chọn/ghi đè nét nghĩa theo bộ từ |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả thiết kế chi tiết cho mô hình mục từ từ vựng nhiều nghĩa (Multi-Sense Domain Model) và phân loại ngữ pháp (Part of Speech - POS) trong hệ thống WordSoul M02.

- **Quan hệ 1-N giữa Mục từ Master và Nét nghĩa**: Một mục từ gốc `Vocabulary` sở hữu mối quan hệ 1-nhiều với danh sách thực thể `VocabularySense`. CẤM kết nối nhiều nghĩa thành một chuỗi văn bản thô phân tách bằng dấu phẩy hay chấm phẩy.
- **Tính độc lập của Nét nghĩa (`Sense Independence`)**: Mỗi `VocabularySense` sở hữu độc lập định nghĩa tiếng Việt (`definitionVi`), định nghĩa tiếng Anh (`definitionEn`), loại từ ngữ pháp (`partOfSpeech`), câu ví dụ (`exampleSentenceEn`/`Vi`) và file âm thanh câu ví dụ (`exampleAudioUrl`).
- **Chọn và Ghi đè Nét nghĩa theo Bộ từ (`Set-Level Sense Filtering`)**: Thực thể liên kết `SetVocabulary` cho phép cấu hình `selectedSenseIds` để chọn đúng nét nghĩa phù hợp với ngữ cảnh của bộ từ đó. Trường hợp không chỉ định, mặc định hiển thị nét nghĩa chính (`senseOrder == 1`).
- **Bảo toàn Lịch sử Tiến độ Học tập (`SRS Progress Integrity`)**: Việc bổ sung hoặc ngừng sử dụng một nét nghĩa (`VocabularySense`) KHÔNG làm hỏng hay xóa bản ghi tiến độ học tập (M04) của người dùng đối với mục từ `VocabularyId` tổng thể.

## 2. Mô hình Thực thể Quan hệ (Entity ERD & Relational Model)

```
+-----------------------------------+          +-----------------------------------+
|            Vocabulary             |          |          VocabularySense          |
+-----------------------------------+          +-----------------------------------+
| PK: VocabularyId (int)            | 1      N | PK: SenseId (int)                 |
| WordCanonical (string) [UQ]       |<-------->| FK: VocabularyId (int)            |
| DisplayWord (string)              |          | SenseOrder (int)                  |
| PronunciationIpa (string)         |          | PartOfSpeech (Enum: N, V, Adj...) |
| CEFRLevel (Enum: A1..C2)          |          | DefinitionVi (string)             |
| AudioUrl (string)                 |          | DefinitionEn (string)             |
| ImageUrl (string)                 |          | ExampleSentenceEn (string)        |
| Status (Enum: draft..published)   |          | ExampleSentenceVi (string)        |
| LicenseId (string)                |          | ExampleAudioUrl (string)          |
| RightsCleared (bool) [REL-04]     |          | Status (Enum: active/deprecated)  |
+-----------------------------------+          +-----------------------------------+
                  ^                                              ^
                  | 1                                            | N (Selected)
                  |                                              |
+----------------------------------------------------------------------------------+
|                                  SetVocabulary                                   |
+----------------------------------------------------------------------------------+
| PK/FK: VocabularySetId (int)                                                     |
| PK/FK: VocabularyId (int)                                                        |
| DisplayOrder (int)                                                               |
| SelectedSenseIds (int[] / JSON)  ==> Allowlist nét nghĩa được chọn theo bộ       |
+----------------------------------------------------------------------------------+
```

## 3. Phân loại Loại từ Ngữ pháp (Part of Speech Enum Standards)

| Mã Enum POS | Tên tiếng Anh | Tên tiếng Việt | Ví dụ định danh | Ràng buộc áp dụng |
|---|---|---|---|---|
| `Noun` | Noun | Danh từ | book, vocabulary, happiness | Bắt buộc có định nghĩa và câu ví dụ minh họa |
| `Verb` | Verb | Động từ | learn, run, memorize | Hỗ trợ cấu hình thì và biến thể động từ (M02-T003) |
| `Adjective` | Adjective | Tính từ | beautiful, rapid, smart | Hỗ trợ dạng so sánh |
| `Adverb` | Adverb | Trạng từ | quickly, fluently, well | Thường bổ nghĩa cho động từ/tính từ |
| `Preposition` | Preposition | Giới từ | in, on, at, under | Đi kèm collocation ngữ cảnh |
| `Conjunction` | Conjunction | Liên từ | and, but, although | Nối câu/mệnh đề |
| `Pronoun` | Pronoun | Đại từ | he, she, it, they | Thay thế danh từ |
| `Interjection` | Interjection | Thán từ | oh, wow, alas | Biểu cảm cảm xúc |
| `Idiom` | Idiom / Phrase | Thành ngữ / Cụm từ | break the ice, piece of cake | Gợi ý cả cụm từ cố định |

## 4. Cơ chế Ghi đè và Lọc Nét nghĩa theo Bộ từ (Set-Level Sense Selection Protocol)

### 4.1. Tình huống Ngữ cảnh
Một từ như `run` có nhiều nét nghĩa:
- Sense 1 (`Verb`): Chạy bộ (thể thao).
- Sense 2 (`Verb`): Vận hành / Quản lý (doanh nghiệp).
- Sense 3 (`Noun`): Chuyến đi / Cuộc chạy.

Khi đưa từ `run` vào Bộ từ "Business English":
- Người biên tập cấu hình `SetVocabulary.SelectedSenseIds = [5002]` (Chỉ chọn Sense 2: Vận hành doanh nghiệp).
- Khi người học mở Bộ từ "Business English", giao diện phiên học M03 và thẻ học tập chỉ hiển thị Sense 2 và câu ví dụ về quản lý doanh nghiệp.

### 4.2. Thuật toán Fallback Nét nghĩa
```csharp
public VocabularySenseDto ResolveSenseForSet(Vocabulary vocabulary, List<int>? selectedSenseIds)
{
    if (selectedSenseIds != null && selectedSenseIds.Any())
    {
        var matchedSense = vocabulary.Senses
            .FirstOrDefault(s => selectedSenseIds.Contains(s.SenseId) && s.Status == SenseStatus.Active);
        if (matchedSense != null) return matchedSense;
    }
    
    // Fallback: Lấy nét nghĩa chính (SenseOrder nhỏ nhất đang Active)
    return vocabulary.Senses
        .Where(s => s.Status == SenseStatus.Active)
        .OrderBy(s => s.SenseOrder)
        .First();
}
```

## 5. Quy trình Chuyển đổi Dữ liệu từ Entity Cũ (`Migration Strategy`)

Trong DB hiện tại của `WordSoulApi`, bảng `Vocabulary` đang lưu 2 trường chuỗi thô `Meaning` và `ExampleSentence`:

1. **Bước 1 (Materialize Sense Entity)**: Tạo bảng mới `VocabularySenses`.
2. **Bước 2 (Data Migration Job)**: Chạy job nạp dữ liệu cũ: Với mỗi record `Vocabulary`, tạo 1 `VocabularySense` tương ứng với `SenseOrder = 1`, `DefinitionVi = Meaning`, `ExampleSentenceEn = ExampleSentence`, `PartOfSpeech = Vocabulary.PartOfSpeech`.
3. **Bước 3 (Dual-Read Validation)**: Cấu hình API nạp dữ liệu ưu tiên đọc từ `VocabularySenses`, fallback sang trường `Meaning` cũ nếu danh sách Senses rỗng.
4. **Bước 4 (Deprecate Old Column)**: Đánh dấu `Meaning` và `ExampleSentence` trên entity `Vocabulary` là `[Obsolete]`, chuẩn bị gỡ bỏ ở lát refactor tiếp theo.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `MS-G01` | Thực thể `Vocabulary` sở hữu quan hệ 1-N với `VocabularySense`, không lưu nghĩa chuỗi thô. |
| `MS-G02` | Mỗi `VocabularySense` sở hữu độc lập POS, định nghĩa En/Vi, câu ví dụ En/Vi và audio ví dụ. |
| `MS-G03` | Enum `PartOfSpeech` hỗ trợ đủ 9 loại từ ngữ pháp tiêu chuẩn. |
| `MS-G04` | `SetVocabulary` hỗ trợ `SelectedSenseIds` cho phép lọc đúng nét nghĩa theo ngữ cảnh bộ từ. |
| `MS-G05` | Thuật toán fallback trả về nét nghĩa chính (`SenseOrder = 1`) khi bộ từ không chỉ định `SelectedSenseIds`. |
| `MS-G06` | Thao tác bổ sung/ngừng dùng một `VocabularySense` không làm hỏng tiến độ học tập M04 hiện có. |
| `MS-G07` | Dữ liệu cũ (`Meaning` / `ExampleSentence`) chuyển đổi thành công sang `VocabularySense` thứ nhất. |
| `MS-G08` | Cấm xóa cứng `VocabularySense` đã từng được người học làm bài kiểm tra trong M03. |
| `MS-G09` | Phân quyền biên tập nét nghĩa tuân thủ nghiêm ngặt ma trận vai trò M11. |
| `MS-G10` | 100% các test case tự kiểm MS02-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MS02-01` | Tạo mục từ "run" với 3 nét nghĩa `VocabularySense` riêng biệt | Lưu thành công 1 `Vocabulary` và 3 `VocabularySense` |
| `MS02-02` | Nạp từ "run" vào Bộ từ "Sports" chọn `SelectedSenseIds = [101]` | Bộ từ "Sports" chỉ hiển thị nét nghĩa Chạy bộ |
| `MS02-03` | Nạp từ "run" vào Bộ từ "Business" chọn `SelectedSenseIds = [102]` | Bộ từ "Business" chỉ hiển thị nét nghĩa Vận hành doanh nghiệp |
| `MS02-04` | Nạp từ "run" vào Bộ từ tổng hợp không chọn `SelectedSenseIds` | Fallback hiển thị nét nghĩa chính (`SenseOrder = 1`) |
| `MS02-05` | Thêm nét nghĩa mới cho từ vựng đã phát hành | Thêm nét nghĩa thành công, `VocabularyId` và progress M04 an toàn |
| `MS02-06` | Đánh dấu `deprecated` nét nghĩa thứ 2 của một từ | Nét nghĩa thứ 2 ẩn khỏi các bộ từ mới, session cũ giữ snapshot |
| `MS02-07` | Chạy job migration dữ liệu cũ từ trường `Meaning` | Tạo 1 `VocabularySense` với `SenseOrder = 1` từ chuỗi `Meaning` |
| `MS02-08` | Tạo `VocabularySense` với loại từ `Idiom` | Lưu loại từ `Idiom` thành công kèm câu ví dụ cụm từ |
| `MS02-09` | Thử tạo `VocabularySense` thiếu `DefinitionVi` | System reject với lỗi `MISSING_DEFINITION_VI` |
| `MS02-10` | Thử xóa cứng `VocabularySense` đang được tham chiếu trong M03 | Deny operation với lỗi `SENSE_REFERENCED_IN_LESSONS` |
| `MS02-11` | Cập nhật câu ví dụ `ExampleSentenceEn` của một nét nghĩa | Cập nhật thành công, phát tín hiệu tạo audio ví dụ mới M12 |
| `MS02-12` | Gán `SenseOrder` trùng nhau cho 2 nét nghĩa cùng 1 mục từ | System reject với lỗi `DUPLICATE_SENSE_ORDER` |
| `MS02-13` | Tra cứu chi tiết từ vựng qua API DTO | DTO trả về đầy đủ danh sách `Senses` theo đúng thứ tự `SenseOrder` |
| `MS02-14` | Biên tập viên đổi loại từ `PartOfSpeech` của một nét nghĩa | Cập nhật POS thành công, ghi vết audit M11 |
| `MS02-15` | Nạp một mục từ có 5 nét nghĩa khác nhau | Hiển thị mượt mà danh sách 5 nét nghĩa trên UI |
| `MS02-16` | Thử chọn `SelectedSenseIds` chứa SenseId không thuộc VocabularyId đó | System reject với lỗi `INVALID_SET_SENSE_SELECTION` |
| `MS02-17` | Lấy danh sách câu hỏi phiên học M03 cho bộ từ có nét nghĩa đã chọn | Câu hỏi phiên học sử dụng đúng câu ví dụ của nét nghĩa đó |
| `MS02-18` | Tải đồng thời 50 request lấy chi tiết từ vựng nhiều nghĩa | Response p95 $< 50\text{ms}$ từ Redis cache |
| `MS02-19` | Phân tích phụ thuộc trước khi ngắt 1 nét nghĩa | Quét đúng các bộ từ đang sử dụng nét nghĩa đó (M11-T020) |
| `MS02-20` | Kiểm thử hoàn tất luồng tạo và chọn nét nghĩa M02-MULTI-SENSE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-MS-I01` | Entity `Vocabulary.cs` hiện tại chưa có bảng/collection `VocabularySenses` | Thiếu mô hình 1-N cho nhiều nét nghĩa | M02-T049 (Source task) |
| `M02-MS-I02` | `SetVocabulary.cs` chưa có trường `SelectedSenseIds` | Không thể lọc nét nghĩa theo bối cảnh bộ từ vựng | M02-T049 |
| `M02-MS-I03` | Trường `Meaning` cũ trong DB là chuỗi thô không phân tách | Rủi ro trộn lẫn nhiều nghĩa gây khó khăn cho SRS M04 | M02-T049 |
| `M02-MS-I04` | DTO `VocabularyDto.cs` trả về thuộc tính `Meaning` đơn lẻ | DTO chưa phản ánh danh sách `VocabularySenseDto` | M02-T049 |
| `M02-MS-I05` | Chưa có Migration Script chuyển đổi trường `Meaning` sang `VocabularySenses` | Thiếu công cụ chuyển đổi dữ liệu an toàn cho DB hiện có | M02-T049 |

- `M02-MS-F01`: Tạo Entity `VocabularySense.cs` và thiết lập quan hệ 1-N với `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-MS-F02`: Thêm thuộc tính `SelectedSenseIds` vào `SetVocabulary.cs` (tiếp nhận: M02-T049).
- `M02-MS-F03`: Xây dựng Migration Job chuyển đổi dữ liệu từ `Meaning` sang `VocabularySenses` (tiếp nhận: M02-T049).
- `M02-MS-F04`: Thiết lập bộ kiểm thử tự động MS-G01–G10 và MS02-01–20 (tiếp nhận: M02 tasks).
- `M02-MS-F05`: Thu thập bằng chứng runtime cho luồng mô hình nhiều nghĩa M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T002

- Đã thiết kế hoàn chỉnh `M02-MULTI-SENSE-1.0` với mô hình 1-N giữa `Vocabulary` và `VocabularySense`.
- Đã chốt danh mục 9 loại từ ngữ pháp (`PartOfSpeech`) chuẩn hóa.
- Đã xây dựng giao thức và thuật toán lọc/ghi đè nét nghĩa theo bộ từ (`Set-Level Sense Selection Protocol`).
- Đã đề xuất chiến lược chuyển đổi dữ liệu an toàn 4 bước từ trường `Meaning` cũ sang bảng `VocabularySenses`.
- Đã xác lập 10 Regression Gates (`MS-G01`–`MS-G10`) và 20 Test Cases tự kiểm (`MS02-01`–`MS02-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế mô hình nhiều nghĩa và loại từ M02-T002 | WSA-7K2 |
