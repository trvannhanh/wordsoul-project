# Chuẩn hóa tìm kiếm mục từ và bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SEARCH-HEADER-SET-1.0` |
| Task | M02-T035 |
| Đầu vào | M02-WORD-VARIANTS-1.0 (M02-T003), M02-SET-PERMISSIONS-1.0 (M02-T016), M02-SET-VERSIONED-PUBLISHING-1.0 (M02-T032) |
| Phạm vi | Tìm kiếm từ khóa không dấu/có dấu, tìm theo biến thể, bộ lọc CEFR/POS/trạng thái xuất bản, tôn trọng quyền xem và phân trang nhất quán |
| Tự kiểm | A-G03, B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định kiến trúc tìm kiếm từ vựng (`Headword Search`) và tìm kiếm bộ từ (`Vocabulary Set Search`) trong M02, đảm bảo hiệu năng cao, phân trang chuẩn và tuân thủ tuyệt đối ma trận quyền sở hữu/trạng thái xuất bản.

- **Bắt buộc Tôn trọng Quyền và Trạng thái (`Strict Access & Status Boundary`)**:
  - Đối với `Headword`: Chỉ tìm thấy các mục từ ở trạng thái `PUBLISHED` (trừ ContentAdmin/SecurityAdmin xem được mọi trạng thái kèm vé hỗ trợ/quyền quản trị).
  - Đối với `VocabularySet`: Bộ từ hệ thống (`IsSystemSet = true`) phải ở trạng thái `PUBLISHED`. Bộ từ cá nhân (`IsPublic = false`) CHỈ được trả về cho đúng chủ sở hữu (`CreatorId == CurrentUserId`). Bộ từ cá nhân công khai (`IsPublic = true`) phải ở trạng thái `PUBLISHED`.
- **Chuẩn hóa Từ khóa và Tìm kiếm Biến thể (`Keyword Normalization & Variant Matching`)**:
  - Tự động bỏ dấu tiếng Việt (Unaccent), hạ chữ thường (Lowercase), xóa khoảng trắng thừa.
  - Tự động tra cứu biến thể từ (`VocabularyVariant` theo M02-T003) để đưa kết quả Master Headword phù hợp.
- **Phân trang và Sắp xếp Nhất quán (`Deterministic Pagination & Sorting`)**:
  - 100% API tìm kiếm sử dụng Offset/Limit hoặc Cursor-based pagination.
  - Sắp xếp mặc định ưu tiên MatchScore (Chính xác > Biến thể > Bắt đầu bằng > Chứa từ) + `DisplayOrder` / `CreatedAtUtc` làm tie-breaker để chống nhảy trang.

## 2. Tiêu chí Tìm kiếm và Bộ lọc (Search Parameters & Filters)

| Tham số | Loại | Giá trị / Định dạng | Mô tả & Quy tắc |
|---|---|---|---|
| `Keyword` | String | Max 100 ký tự | Từ khóa tìm kiếm. Tự động normalize (Unaccent, Lowercase). |
| `CEFRLevels` | Array[Enum] | `A1, A2, B1, B2, C1, C2` | Bộ lọc cấp độ CEFR. Null = Tất cả. |
| `POS` | Array[Enum] | `Noun, Verb, Adjective, ...` | Bộ lọc loại từ POS. Null = Tất cả. |
| `CategoryIds` | Array[Guid] | Danh sách ID danh mục | Bộ lọc theo danh mục học liệu M02-T015. |
| `IncludePrivate` | Boolean | `true / false` | Đòi hỏi token xác thực. Trả về bộ từ cá nhân của người dùng hiện tại. |
| `PageNumber` | Int | `PageNumber >= 1` | Số trang (Default = 1). |
| `PageSize` | Int | `1 <= PageSize <= 50` | Kích thước trang (Default = 20, Max = 50). |

## 3. Thuật toán Xử lý Tìm kiếm và Chống Trùng (Search Matching Engine)

```csharp
public async Task<PagedResultDto<VocabularySearchResultDto>> SearchHeadwordsAsync(SearchHeadwordQuery query, UserId currentUserId)
{
    string normalizedKeyword = StringUtils.NormalizeCanonical(query.Keyword);
    
    // 1. Quét exact match từ master canonical name
    var exactMatches = await _vocabRepo.GetCanonicalMatchesAsync(normalizedKeyword);
    
    // 2. Quét variant matches qua M02-T003 Variant Matcher
    var variantMatches = await _variantRepo.GetHeadwordsByVariantAsync(normalizedKeyword);
    
    // 3. Quét prefix/contains matches
    var fuzzyMatches = await _vocabRepo.GetFuzzyMatchesAsync(normalizedKeyword, query.CEFRLevels, query.POS);
    
    // 4. Tổng hợp, xếp hạng MatchScore & kẹp phân trang
    var combinedResults = CombineAndRank(exactMatches, variantMatches, fuzzyMatches, query.PageNumber, query.PageSize);
    
    return combinedResults;
}
```

## 4. Quyền Truy cập và Tôn trọng Trạng thái (Access Control & State Privacy)

| Đối tượng | Trạng thái cho phép | Điều kiện hiển thị |
|---|---|---|
| Master Headword | `PUBLISHED` | Công khai cho mọi Learner / Guest. |
| Master Headword | `DRAFT, IN_REVIEW, ARCHIVED` | Chỉ ContentAdmin / SuperAdmin có quyền `M11_READ_INTERNAL_CONTENT`. |
| System Set | `PUBLISHED` | Công khai cho mọi Learner. |
| Custom Set (Private) | Mọi trạng thái | CHỈ trả về cho chính `CreatorId`. Trả HTTP 403 / Omit nếu người khác tìm kiếm. |
| Custom Set (Public) | `PUBLISHED` | Hiển thị trong kết quả tìm kiếm cộng đồng. |

## 5. Regression Gates và Test Cases

### 5.1. Regression Gates
- `SR-G01`: 100% kết quả tìm kiếm bộ từ cá nhân riêng tư (`IsPublic = false`) bị ẩn khỏi người dùng khác.
- `SR-G02`: Tìm kiếm từ khóa không dấu (ví dụ: `hoc sinh`) khớp đúng từ có dấu (`học sinh`).
- `SR-G03`: Kết quả tìm kiếm từ vựng qua biến thể (ví dụ: `running`) tự động map về Headword gốc (`run`).
- `SR-G04`: Phân trang phân định rõ `TotalCount`, `PageNumber`, `PageSize` và không trùng lắp mục giữa các trang.
- `SR-G05`: `PageSize > 50` tự động bị kẹp về `50`.

### 5.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SR35-01` | Learner A tìm kiếm từ khóa `"hello"` | Trả về các mục từ `PUBLISHED` khớp với `"hello"`. |
| `SR35-02` | Learner A tìm bộ từ riêng tư của Learner B | Kết quả không chứa bộ từ của Learner B. |
| `SR35-03` | Learner B tìm kiếm bộ từ riêng tư của chính mình với `IncludePrivate = true` | Trả về đúng các bộ từ của Learner B. |
| `SR35-04` | Truy vấn phân trang `PageNumber = 2, PageSize = 10` | Trả về đúng các mục từ 11 đến 20, không lặp lại trang 1. |
| `SR35-05` | Tìm kiếm từ vựng bằng biến thể quá khứ `"bought"` | Trả về Headword gốc `"buy"` (MatchScore Variant). |
| `SR35-06` | Truy vấn tham số `PageSize = 500` | System kẹp `PageSize = 50`. |
| `SR35-07` | Lọc từ vựng theo CEFR `[A1, A2]` và POS `[Verb]` | Trả về đúng các động từ cấp độ A1 hoặc A2. |
| `SR35-08` | Tìm kiếm từ khóa không có dữ liệu khớp | Trả về `TotalCount = 0`, `Items = []` (Empty State nhất quán, không null). |
| `SR35-09` | Khách (Guest / Unauthenticated) tìm kiếm bộ từ | Chỉ trả về System Set & Public Set ở trạng thái `PUBLISHED`. |
| `SR35-10` | Kiểm thử hoàn tất luồng M02-SEARCH-HEADER-SET-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 6. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SR-F01` | Chưa có Redis Cache cho kết quả tìm kiếm từ khóa phổ biến | Tải DB tăng cao khi có nhiều request lặp | M02-T049 |
| `M02-SR-F02` | Rút gọn kết quả DTO tìm kiếm để tránh lộ thông tin nội bộ | Cần tách `SearchHeadwordSummaryDto` khỏi `VocabularyDetailDto` | M02-T049 |

## 7. Tự kiểm M02-T035
- Đã hoàn thiện đặc tả `M02-SEARCH-HEADER-SET-1.0`.
- Đã chốt cơ chế tìm kiếm không dấu, biến thể và tôn trọng ranh giới riêng tư.
- Xác lập 5 Regression Gates (`SR-G01`–`SR-G05`) và 10 Test Cases (`SR35-01`–`SR35-10`).

## 8. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa tìm kiếm mục từ và bộ từ M02-T035 | WSA-7K2 |
