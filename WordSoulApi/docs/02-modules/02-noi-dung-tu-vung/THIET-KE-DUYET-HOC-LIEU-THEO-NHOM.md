# Thiết kế duyệt học liệu theo nhóm M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-CATEGORY-BROWSE-1.0` |
| Task | M02-T036 |
| Đầu vào | M02-VOCAB-SET-CRITERIA-1.0 (M02-T015), M02-SET-VERSIONED-PUBLISHING-1.0 (M02-T032) |
| Phạm vi | Cấu trúc phân loại học liệu theo chủ đề/nhóm, chống lặp bộ từ ngoài chủ đích và xử lý biến mất tức thì của nội dung bị thu hồi |
| Tự kiểm | A-G03, B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế duyệt bộ từ vựng và mục từ theo nhóm/chủ đề danh mục (Catalog Categorization & Discovery Groups) cho M02.

- **Duyệt theo Nhóm Tiêu chí (`Criteria-based Group Browsing`)**: Bộ từ được sắp xếp vào 5 nhóm tiêu chuẩn: `CEFR Level`, `Topic/Theme`, `Exam Prep (IELTS/TOEIC)`, `Skill Level`, `Curated Special`.
- **Chống Lặp Bộ từ Ngoài Chủ đích (`Unintended Set Duplication Prevention`)**: CẤM hiển thị trùng lặp 1 bộ từ vựng nhiều lần trong cùng một nhóm danh mục/màn hình danh sách (trừ khi bộ từ thuộc nhiều danh mục được gán nhãn rõ ràng).
- **Xử lý Thu hồi Nhanh chóng (`Instant Recall Eviction SLA <= 60s`)**: Khi một bộ từ bị thu hồi (Recall / Quarantine theo M02-T033), bộ từ đó phải lập tức bị loại khỏi mọi danh mục hiển thị public trong vòng $\le 60\text{s}$ (thông qua cơ chế Xóa Cache Redis Pub/Sub).

## 2. Phân loại Nhóm Danh mục (Category Hierarchy)

| Mã Nhóm | Tên Nhóm | Mô tả | Tiêu chí Gán |
|---|---|---|---|
| `CAT_CEFR` | Cấp độ CEFR | Phân loại theo trình độ chuẩn A1-C2 | $D_{set}$ và CEFR tĩnh của các từ thành viên |
| `CAT_TOPIC` | Chủ đề Cuộc sống | Giao tiếp, Du lịch, Công sở, Y tế, Công nghệ | Tag danh mục `CategoryId` |
| `CAT_EXAM` | Luyện thi | IELTS, TOEIC, TOEFL, SAT | Bộ từ được gán nhãn chuyên biệt cho kỳ thi |
| `CAT_FEATURED` | Bộ từ Nổi bật | Do Biên tập viên chọn lọc (Curated) | `IsFeatured = true` và `QualityScore >= 90%` |
| `CAT_NEW` | Mới phát hành | Mới xuất bản trong 30 ngày | `PublishedAtUtc >= Now - 30 days` |

## 3. Thuật toán Duyệt và Chống Lặp (Browsing & Deduplication Engine)

```csharp
public async Task<CategoryGroupContentDto> GetCategoryGroupContentAsync(Guid categoryId, int page, int pageSize)
{
    // 1. Kiểm tra cache Redis
    string cacheKey = $"wordsoul:live:m02:category:{categoryId}:p{page}";
    var cached = await _cache.GetAsync<CategoryGroupContentDto>(cacheKey);
    if (cached != null) return cached;

    // 2. Truy vấn DB các bộ từ PUBLISHED thuộc categoryId
    var sets = await _setRepo.GetPublishedSetsByCategoryAsync(categoryId, page, pageSize);

    // 3. Khử trùng lặp ID bộ từ trong danh sách trả về
    var deduplicatedSets = sets.DistinctBy(s => s.VocabularySetId).ToList();

    var result = new CategoryGroupContentDto {
        CategoryId = categoryId,
        Sets = deduplicatedSets,
        Page = page,
        PageSize = pageSize
    };

    // 4. Cache kết quả 15 phút
    await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
    return result;
}
```

## 4. Xử lý Nội dung Bị Thu hồi hoặc Ẩn (Recalled Content Eviction)

Khi sự kiện `VocabularySetRecalledIntegrationEvent` phát ra:
1. `CategoryCacheManager` nhận event qua Redis Pub/Sub.
2. Thực hiện xóa toàn bộ cache keys thuộc prefix `wordsoul:live:m02:category:*`.
3. Trong lượt truy vấn tiếp theo, DB loại bỏ bộ từ có `Status == RECALLED` hoặc `IsQuarantined == true`.
4. Tổng thời gian hoàn tất loại bỏ trên toàn hệ thống $\le 60\text{s}$.

## 5. Regression Gates và Test Cases

### 5.1. Regression Gates
- `CB-G01`: 100% bộ từ bị thu hồi (`RECALLED`) không xuất hiện trong bất kỳ nhóm danh mục public nào.
- `CB-G02`: Không trùng lặp bộ từ vựng trong cùng 1 trang kết quả duyệt danh mục.
- `CB-G03`: Thời gian xóa cache danh mục khi có sự kiện thu hồi khẩn cấp $\le 60\text{s}$.
- `CB-G04`: 100% nhóm danh mục trả về `CategoryTitle` và `CategoryCode` hợp lệ.

### 5.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CB36-01` | Người học duyệt danh mục `"Luyện thi IELTS"` | Trả về danh sách các bộ từ xuất bản thuộc IELTS, không lặp ID. |
| `CB36-02` | Admin phát lệnh Thu hồi khẩn cấp 1 bộ từ trong danh mục | Bộ từ biến mất khỏi danh mục public trong $\le 60\text{s}$. |
| `CB36-03` | Duyệt danh mục không có bộ từ nào | Trả về danh sách rỗng `Sets = []`, không ném exception. |
| `CB36-04` | Duyệt danh mục với phân trang trang 1 và trang 2 | Danh sách trang 2 không chứa bất kỳ bộ từ nào của trang 1. |
| `CB36-05` | Thử truy vấn danh mục không tồn tại | Trả HTTP 404 `CATEGORY_NOT_FOUND`. |
| `CB36-06` | Kiểm thử hoàn tất luồng M02-CATEGORY-BROWSE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 6. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-CB-F01` | Cần bổ sung bảng `VocabularyCategories` trong DB schema | Chưa có bảng lưu thông tin danh mục động | M02-T049 |

## 7. Tự kiểm M02-T036
- Đã hoàn thiện đặc tả `M02-CATEGORY-BROWSE-1.0`.
- Đã chốt 5 nhóm danh mục chuẩn và SLA thu hồi $\le 60\text{s}$.
- Ghi nhận 4 Regression Gates (`CB-G01`–`CB-G04`) và 6 Test Cases (`CB36-01`–`CB36-06`).

## 8. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế duyệt học liệu theo nhóm M02-T036 | WSA-7K2 |
