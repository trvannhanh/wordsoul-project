# Chuẩn hóa mặt chữ và biến thể M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-WORD-VARIANTS-1.0` |
| Task | M02-T003 |
| Đầu vào | M02-VOCAB-DICT-1.0, M02-MULTI-SENSE-1.0 |
| Phạm vi | Quy tắc chuẩn hóa mặt chữ gốc (Word Canonicalization), mô hình lưu trữ biến thể hình thái/chính tả (Morphological Variants & Inflections) và bộ khớp biến thể khi học |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập bộ quy tắc chuẩn hóa mặt chữ gốc (`WordCanonical`) và mô hình quản lý biến thể từ vựng (`VocabularyVariant`) nhằm đảm bảo tính duy nhất của từ điển Master M02, đồng thời hỗ trợ người học tra cứu và trả lời chính xác khi nhập các dạng biến thể hình thái (chia động từ, số nhiều, biến thể chính tả Anh-Anh/Anh-Mỹ).

- **Quy tắc Chuẩn hóa Mặt chữ Gốc (`Word Canonicalization Rules`)**:
  - `wordCanonical = Normalize(rawInput)`: Chuyển toàn bộ về chữ viết thường (`lowercase`), cắt bỏ khoảng trắng thừa ở hai đầu (`trim`), chuẩn hóa mã UTF-8 theo chuẩn Unicode NFC, loại bỏ ký tự zero-width.
  - Giữ lại các dấu gạch nối (`-`) và dấu nháy đơn (`'`) hợp lệ trong cụm từ (vd: `well-known`, `rock 'n' roll`). Loại bỏ các dấu chấm, phẩy, chấm cảm không thuộc mặt chữ gốc.
  - CẤM tạo 2 mục từ Master phân biệt chỉ bằng chữ hoa/viết thường (vd: `Apple` và `apple` trùng 1 `WordCanonical`).
- **Mô hình Quản lý Biến thể Hình thái (1-N Variant Sub-schema)**:
  - Một `Vocabulary` có thể liên kết với danh sách các `VocabularyVariant`. Mỗi biến thể ghi rõ chuỗi biến thể (`variantText`), loại biến thể (`variantType`) và quy tắc khớp (`matchingRule`).
- **Khớp Biến thể Thời gian thực (Variant Matcher Protocol)**: Khi người học gõ dạng biến thể (vd: `running`, `ran`, `children`) trong phiên học M03 hoặc thanh tìm kiếm, Động cơ Khớp Biến thể (`Variant Matcher`) tự động ánh xạ về `VocabularyId` gốc để chấp nhận câu trả lời hoặc hiển thị đúng từ vựng Master.

## 2. Mô hình Thực thể Biến thể Từ vựng (Vocabulary Variant Schema)

```json
{
  "variantId": 3012,
  "vocabularyId": 1024,
  "variantTextCanonical": "vocabularies",
  "displayVariant": "vocabularies",
  "variantType": "Plural",
  "languageConvention": "Universal",
  "isAcceptedInQuiz": true,
  "createdAtUtc": "2026-08-20T10:00:00Z"
}
```

## 3. Danh mục Loại Biến thể Từ vựng (Variant Types Enum)

| Mã VariantType | Tên biến thể | Mô tả & Ví dụ | Áp dụng Loại từ | Chấp nhận trong Bài kiểm tra? |
|---|---|---|---|---|
| `Plural` | Dạng số nhiều | `vocabularies` (từ `vocabulary`), `children` (từ `child`) | Noun | Có (`isAcceptedInQuiz = true`) |
| `PastTense` | Thì quá khứ đơn | `ran` (từ `run`), `learned` (từ `learn`) | Verb | Có |
| `PastParticiple` | Qúa khứ phân từ | `run` / `ridden` / `written` | Verb | Có |
| `PresentParticiple` | V-ing / Hiện tại phân từ | `running`, `learning`, `studying` | Verb | Có |
| `ThirdPersonSingular` | Động từ ngôi thứ 3 | `runs`, `studies`, `goes` | Verb | Có |
| `Comparative` | Dạng so sánh hơn | `faster`, `better`, `more beautiful` | Adj / Adv | Có |
| `Superlative` | Dạng so sánh nhất | `fastest`, `best`, `most beautiful` | Adj / Adv | Có |
| `SpellingUSUK` | Chính tả Anh-Mỹ / Anh-Anh | `color` (US) vs `colour` (UK), `organize` vs `organise` | Tất cả | Có (Ánh xạ đồng đẳng) |
| `Contraction` | Dạng viết tắt | `don't` (từ `do not`), `it's` (từ `it is`) | Phrase | Có |
| `HyphenatedAlias` | Biến thể dấu gạch nối | `email` vs `e-mail`, `online` vs `on-line` | Tất cả | Có |

## 4. Thuật toán Chuẩn hóa Mặt chữ Gốc (Canonicalization Pipeline)

```csharp
public static class WordCanonicalizer
{
    public static string ToCanonical(string rawWord)
    {
        if (string.IsNullOrWhiteSpace(rawWord))
            return string.Empty;

        // 1. Unicode NFC Normalization
        string normalized = rawWord.Normalize(NormalizationForm.FormC);

        // 2. Lowercase conversion & Trim
        string lower = normalized.ToLowerInvariant().Trim();

        // 3. Remove Invisible / Zero-width characters
        string clean = Regex.Replace(lower, @"[\u200B-\u200D\uFEFF]", "");

        // 4. Collapse multiple spaces into single space
        string singleSpace = Regex.Replace(clean, @"\s+", " ");

        // 5. Strip illegal punctuation (Retain letters, digits, spaces, hyphens, apostrophes)
        string canonical = Regex.Replace(singleSpace, @"[^\p{L}\p{N}\s\-']", "");

        return canonical.Trim();
    }
}
```

Ví dụ kiểm thử chuẩn hóa:
- `"  APPLE  "` $\to$ `"apple"`
- `"e-mail "` $\to$ `"e-mail"`
- `"Color (US)"` $\to$ `"color us"` (nếu loại bỏ ngoặc) hoặc `"color"` (nếu bỏ annotation).

## 5. Động cơ Khớp Biến thể trong Phiên học (Variant Matcher Engine)

Khi người học nhập một từ trong câu hỏi viết/gõ của Phiên học M03:

1. **Khớp trực tiếp Master Canonical**: So sánh `ToCanonical(userInput)` với `Vocabulary.WordCanonical`. Nếu khớp $\to$ Trả về `EXACT_MATCH` (100% điểm).
2. **Khớp qua Bảng Biến thể (`VocabularyVariant`)**: So sánh `ToCanonical(userInput)` với `VocabularyVariant.VariantTextCanonical`. Nếu khớp và `isAcceptedInQuiz == true` $\to$ Trả về `VARIANT_MATCH` (100% điểm kèm ghi chú: *"Đúng! Đây là dạng biến thể của từ..."*).
3. **Khớp biến thể US/UK**: Nếu nhập `colour` cho từ master `color` $\to$ Trả về `US_UK_MATCH` (100% điểm).
4. **Không khớp**: Trả về `NO_MATCH` (0 điểm).

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `WV-G01` | `WordCanonical` được tạo ra bằng thuật toán chuẩn hóa NFC, lowercase, trim 100% nhất quán. |
| `WV-G02` | Cấm tạo 2 mục từ Master phân biệt chỉ bằng chữ hoa/chữ thường hoặc khoảng trắng thừa. |
| `WV-G03` | Thực thể `Vocabulary` sở hữu danh sách 1-N biến thể `VocabularyVariant`. |
| `WV-G04` | Enum `VariantType` bao phủ đầy đủ 10 loại biến thể ngữ pháp và chính tả tiêu chuẩn. |
| `WV-G05` | Động cơ Variant Matcher ánh xạ chính xác biến thể nhập vào về `VocabularyId` Master. |
| `WV-G06` | Biến thể chính tả US/UK được coi là khớp đồng đẳng (100% điểm bài kiểm tra). |
| `WV-G07` | Giữ nguyên dấu gạch nối `-` và dấu nháy đơn `'` trong các từ phức/thành ngữ hợp lệ. |
| `WV-G08` | Thao tác bổ sung biến thể không làm thay đổi ID mục từ Master hay làm hỏng dữ liệu M04. |
| `WV-G09` | Phân quyền quản lý biến thể tuân thủ nghiêm ngặt ma trận vai trò M11. |
| `WV-G10` | 100% các test case tự kiểm WV03-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `WV03-01` | Chuẩn hóa chuỗi `"  Well-Known  "` | Trả về `wordCanonical = "well-known"` |
| `WV03-02` | Thử thêm từ `"Apple"` khi từ `"apple"` đã tồn tại trong Master DB | System reject với lỗi `CANONICAL_WORD_EXISTS` |
| `WV03-03` | Thêm biến thể số nhiều `"children"` cho từ master `"child"` | Lưu `VocabularyVariant` loại `Plural` thành công |
| `WV03-04` | Người học gõ `"running"` cho câu hỏi từ master `"run"` | Variant Matcher trả về `VARIANT_MATCH` (100% điểm) |
| `WV03-05` | Người học gõ `"colour"` cho câu hỏi từ master `"color"` | Variant Matcher trả về `US_UK_MATCH` (100% điểm) |
| `WV03-06` | Chuẩn hóa chuỗi chứa ký tự unicode FormD (dấu phân rã) | Chuyển thành FormC NFC chuẩn hóa chính xác |
| `WV03-07` | Chuẩn hóa chuỗi chứa ký tự ẩn Zero-width space `\u200B` | Loại bỏ ký tự ẩn, trả về chuỗi sạch |
| `WV03-08` | Thêm biến thể thì quá khứ `"ran"` cho động từ `"run"` | Lưu `VocabularyVariant` loại `PastTense` thành công |
| `WV03-09` | Thử tạo biến thể trùng `variantTextCanonical` trong cùng 1 mục từ | System reject với lỗi `DUPLICATE_VARIANT_TEXT` |
| `WV03-10` | Tra cứu từ điển qua từ khóa dạng biến thể `"vocabularies"` | Động cơ tìm thấy đúng mục từ Master `"vocabulary"` |
| `WV03-11` | Thêm biến thể so sánh hơn `"better"` cho tính từ `"good"` | Lưu `VocabularyVariant` loại `Comparative` thành công |
| `WV03-12` | Thử gõ một từ hoàn toàn không liên quan `"cat"` cho từ master `"dog"` | Matcher trả về `NO_MATCH` |
| `WV03-13` | Chuẩn hóa cụm từ có dấu nháy đơn `"rock 'n' roll"` | Giữ nguyên dấu nháy đơn, trả về `"rock 'n' roll"` |
| `WV03-14` | Đánh dấu `isAcceptedInQuiz = false` cho một biến thể hiếm | Matcher không cộng điểm bài kiểm tra cho biến thể đó |
| `WV03-15` | Xóa một biến thể từ vựng | Xóa biến thể thành công, mục từ Master giữ nguyên |
| `WV03-16` | Thao tác nhập từ có ký tự chấm cảm `"Hello!"` | Loại bỏ dấu chấm cảm, trả về `wordCanonical = "hello"` |
| `WV03-17` | Tải đồng thời 50 request khớp biến thể trong bài kiểm tra | Response p95 $< 30\text{ms}$ từ Redis Cache |
| `WV03-18` | Thêm biến thể viết tắt `"don't"` cho cụm từ `"do not"` | Lưu `VocabularyVariant` loại `Contraction` thành công |
| `WV03-19` | Phân tích phụ thuộc khi xóa một biến thể | Kiểm tra an toàn không ảnh hưởng tới Master Headword (T020) |
| `WV03-20` | Kiểm thử hoàn tất luồng chuẩn hóa mặt chữ và biến thể M02-WORD-VARIANTS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-WV-I01` | Entity `Vocabulary.cs` chưa có thuộc tính `WordCanonical` đã băm/trim chuẩn | Rủi ro tạo từ trùng lặp do hoa/thường hoặc khoảng trắng | M02-T049 (Source task) |
| `M02-WV-I02` | Chưa có entity `VocabularyVariant.cs` cho các dạng biến thể ngữ pháp | Không hỗ trợ người học gõ dạng số nhiều, thì quá khứ trong bài test | M02-T049 |
| `M02-WV-I03` | `VocabularyService.cs` nạp từ bằng `ToLowerInvariant().Trim()` đơn giản | Chưa có pipeline chuẩn hóa Unicode NFC và loại ký tự ẩn | M02-T049 |
| `M02-WV-I04` | Chưa có Động cơ Khớp Biến thể (`Variant Matcher Engine`) | Người học nhập `running` bị tính sai cho từ gốc `run` | M02-T049 |
| `M02-WV-I05` | Chưa có bảng tra cứu đồng đẳng chính tả US/UK (color/colour) | Người học nhập chính tả Anh-Anh bị tính sai đối với từ Anh-Mỹ | M02-T049 |

- `M02-WV-F01`: Thêm trường `WordCanonical` vào `Vocabulary.cs` và cài đặt `WordCanonicalizer` (tiếp nhận: M02-T049).
- `M02-WV-F02`: Tạo Entity `VocabularyVariant.cs` và cài đặt quan hệ 1-N với `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-WV-F03`: Triển khai `VariantMatcherEngine` phục vụ chấm điểm bài test M03 (tiếp nhận: M02-T049).
- `M02-WV-F04`: Thiết lập bộ kiểm thử tự động WV-G01–G10 và WV03-01–20 (tiếp nhận: M02 tasks).
- `M02-WV-F05`: Thu thập bằng chứng runtime cho luồng chuẩn hóa mặt chữ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T003

- Đã thiết kế hoàn chỉnh `M02-WORD-VARIANTS-1.0` với pipeline chuẩn hóa `WordCanonical` 5 bước.
- Đã chốt mô hình thực thể `VocabularyVariant` và danh mục 10 loại biến thể ngữ pháp/chính tả tiêu chuẩn.
- Đã xây dựng Động cơ Khớp Biến thể Thời gian thực (`Variant Matcher Engine`) cho các bài kiểm tra M03.
- Đã quy định rõ cơ chế khớp đồng đẳng US/UK (color/colour).
- Đã xác lập 10 Regression Gates (`WV-G01`–`WV-G10`) và 20 Test Cases tự kiểm (`WV03-01`–`WV03-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa mặt chữ và biến thể M02-T003 | WSA-7K2 |
