# Thiết kế ngừng dùng, hợp nhất và thay thế M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-DEPRECATION-REPLACEMENT-1.0` |
| Task | M02-T010 |
| Đầu vào | M02-DUPLICATE-DETECTION-1.0 (D-059), M02-HEADWORD-VERSIONING-1.0 (D-063), M11-ROLLBACK-DEPRECATION-1.0 (D-049), M11-REFERENCE-IMPACT-1.0 (D-052) |
| Phạm vi | Quy trình 3 thao tác: Ngừng dùng (Deprecate), Hợp nhất (Merge) và Thay thế (Replace) mục từ vựng; phân tích tác động tham chiếu 5 tầng và bảo toàn lịch sử tiến độ M04 |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả giao thức xử lý 3 thao tác quản trị vòng đời nâng cao đối với Mục từ vựng Master (`Vocabulary`): Ngừng dùng (`Deprecate`), Hợp nhất (`Merge`) và Thay thế (`Replace`), nhằm bảo đảm tính toàn vẹn học liệu, không làm đứt đoạn tiến độ học tập người dùng (M04) và tuân thủ các chốt kiểm tra tác động tham chiếu của M11.

- **Bảo toàn Tiến độ Lịch sử SRS M04**: Ngừng dùng hay gộp mục từ TUYỆT ĐỐI KHÔNG làm xóa hoặc giảm bớt số lần ôn tập/điểm ghi nhớ (`UserVocabularyProgress`) của người học đối với từ vựng đó.
- **Phân tích Tác động Tham chiếu 5 Tầng trước Thay thế (`Pre-Replacement Reference Scan`)**: Trước khi thực hiện Ngừng dùng hay Thay thế một mục từ, hệ thống bắt buộc chạy quét đồ thị phụ thuộc đệ quy 5 tầng (M11-T020 / D-052) để phát hiện các bộ từ, phiên học M03, đấu trường M08 bị ảnh hưởng.
- **Cấm Xóa Cửng CSDL (`No Physical Delete Invariant`)**: Mọi mục từ khi bị ngừng dùng hoặc gộp trùng lặp đều được chuyển trạng thái `Deprecated` / `Archived`, cấm thực hiện lệnh SQL `DELETE` thô trên bảng `Vocabularies`.
- **Ánh xạ Mục từ Thay thế (`Replacement Pointer`)**: Mục từ bị `Deprecated` hoặc `Archived` sở hữu thuộc tính `ReplacementVocabularyId` trỏ đến mục từ mới tương đương để tự động gợi ý chuyển tiếp cho các bộ từ vựng liên quan.

## 2. Mô tả 3 Thao tác Quản trị Nâng cao

```
                                [GOVERNANCE OPERATION]
                                          |
         +--------------------------------+--------------------------------+
         |                                |                                |
         v                                v                                v
 [1. DEPRECATE HEADWORD]          [2. MERGE HEADWORDS]          [3. REPLACE IN SET]
 - Status = 'Deprecated'          - Source (A) -> Target (B)      - Replace Old -> New in Set X
 - ReplacementVocabId = B         - Reassign SetVocabulary        - Re-evaluate Set Difficulty
 - Block in New Sets              - Migrate User Progress         - Purge Lesson Payload Cache
 - Keep Historical SRS M04        - Archive Source (A)            - Audit Event M11
```

### 2.1. Thao tác 1: Ngừng dùng Mục từ (Deprecate Headword)
- **Mục đích**: Đánh dấu một mục từ lỗi thời, không còn phù hợp với chương trình học hiện tại.
- **Quy trình**:
  1. Đặt `Vocabulary.Status = Deprecated`.
  2. Gán `Vocabulary.ReplacementVocabularyId = NewVocabId` (nếu có).
  3. Cấm chèn mục từ này vào các Bộ từ vựng mới tạo.
  4. Các Bộ từ vựng công khai hiện tại chứa từ này hiển thị cảnh báo *"Từ vựng này đã ngừng dùng, đề xuất thay thế bằng..."*.

### 2.2. Thao tác 2: Hợp nhất Mục từ (Merge Headwords)
- **Mục đích**: Gộp 2 mục từ vô tình bị tạo trùng lặp (`SourceId_A` $\to$ `TargetId_B`).
- **Quy trình**:
  1. Mở CSDL Transaction.
  2. Chuyển dịch toàn bộ bản ghi `SetVocabulary` từ A sang B (loại bỏ nếu B đã có trong bộ đó).
  3. Hợp nhất `UserVocabularyProgress` của A sang B (giữ điểm ghi nhớ cao hơn và `nextReviewAt` sớm hơn).
  4. Đánh dấu `SourceId_A` là `Archived`, gán `ReplacementVocabularyId = B`.
  5. Đăng ký `WordCanonical` của A vào danh sách `VocabularyVariant` của B.
  6. Commit Transaction và xóa Cache Payload M03.

### 2.3. Thao tác 3: Thay thế Mục từ trong Bộ (Replace Headword in Set)
- **Mục đích**: Đổi một từ vựng `OldId` bằng `NewId` trong một Bộ từ cụ thể `SetId_X` mà không làm thay đổi các bộ từ khác.
- **Quy trình**:
  1. Kiểm tra điều kiện `NewId` có trạng thái `Published` và `QualityScore >= 80%` (M02-T006).
  2. Cập nhật `SetVocabulary` thay thế `OldId` bằng `NewId`.
  3. Tính toán lại chỉ số độ khó của bộ từ `SetDifficultyIndex` (M02-T005).
  4. Xóa cache payload bài học `lesson_payload:{SetId}` trong Redis.

## 3. Quy trình Phân tích Tác động Tham chiếu 5 Tầng (Pre-Replacement Impact Scan)

Trước khi thực thi 1 trong 3 thao tác trên, Quản trị viên gọi API `ScanHeadwordImpact(VocabularyId)`:

```
[Start ScanHeadwordImpact(VocabularyId)]
                   |
                   v
       +-----------------------------------------------+
       | Level 1: Quét các Bộ từ vựng (VocabularySets)  |
       +-----------------------------------------------+
                   |
                   v
       +-----------------------------------------------+
       | Level 2: Quét các Phiên học M03 đang mở       |
       +-----------------------------------------------+
                   |
                   v
       +-----------------------------------------------+
       | Level 3: Quét Thẻ ôn tập SRS M04 người dùng    |
       +-----------------------------------------------+
                   |
                   v
       +-----------------------------------------------+
       | Level 4: Quét Phòng đấu PvP M08 active        |
       +-----------------------------------------------+
                   |
                   v
       +-----------------------------------------------+
       | Level 5: Quét Bảng xếp hạng / Thách đấu M09    |
       +-----------------------------------------------+
                   |
                   v
  [Categorize Impact Level & Return Impact Summary DTO]
  - SAFE_NO_ACTIVE_REF (0 active ref)
  - WARN_CASCADE_UPDATE (Chỉ có bộ từ static)
  - BLOCKING_HARD_DEPENDENCY (Đang có phiên đấu PvP live)
```

## 4. Giao thức Bảo toàn Lịch sử Tiến độ Ôn tập M04 khi Gộp từ

```csharp
public async Task MergeUserProgressAsync(int sourceVocabId, int targetVocabId)
{
    var sourceProgresses = await _db.UserVocabularyProgresses
        .Where(p => p.VocabularyId == sourceVocabId).ToListAsync();

    foreach (var src in sourceProgresses)
    {
        var tgt = await _db.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == src.UserId && p.VocabularyId == targetVocabId);

        if (tgt == null)
        {
            // Chuyển dịch thẳng bản ghi progress sang Target ID
            src.VocabularyId = targetVocabId;
        }
        else
        {
            // Hợp nhất điểm số: Lấy SRS interval lớn hơn & memory score cao hơn
            tgt.IntervalDays = Math.Max(tgt.IntervalDays, src.IntervalDays);
            tgt.MasteryScore = Math.Max(tgt.MasteryScore, src.MasteryScore);
            tgt.NextReviewAtUtc = src.NextReviewAtUtc < tgt.NextReviewAtUtc ? src.NextReviewAtUtc : tgt.NextReviewAtUtc;
            
            // Xóa bản ghi source thừa
            _db.UserVocabularyProgresses.Remove(src);
        }
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DR-G01` | Thao tác Ngừng dùng (`Deprecate`) gán chính xác `ReplacementVocabularyId` và ngăn chèn vào bộ mới. |
| `DR-G02` | Thao tác Hợp nhất (`Merge`) chuyển dịch an toàn toàn bộ tiến độ M04 sang mục từ đích `TargetId`. |
| `DR-G03` | Cấm xóa cứng (Physical DELETE) các mục từ bị `Deprecated` hay `Archived` trong CSDL. |
| `DR-G04` | Phân tích tác động tham chiếu quét đủ 5 tầng phụ thuộc trước khi thực thi thay thế. |
| `DR-G05` | Nếu phát hiện `BLOCKING_HARD_DEPENDENCY` (phòng đấu PvP M08 đang chạy), hệ thống chặn thao tác thay thế. |
| `DR-G06` | Thao tác Thay thế trong bộ từ tự động kích hoạt tính toán lại `SetDifficultyIndex` (M02-T005). |
| `DR-G07` | Toàn bộ cache Redis `lesson_payload` liên quan bị xóa hoàn tất trong vòng $\le 60$ giây. |
| `DR-G08` | Thao tác gộp từ chèn `WordCanonical` của từ nguồn vào danh sách `VocabularyVariant` của từ đích. |
| `DR-G09` | Phân quyền thực thi 3 thao tác tuân thủ nghiêm ngặt ma trận vai trò M11 (`R03 Content Admin`). |
| `DR-G10` | 100% các test case tự kiểm DR10-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DR10-01` | Ngừng dùng từ `"e-mail"` chỉ định từ thay thế `"email"` | `Status = Deprecated`, `ReplacementVocabularyId` trỏ tới `"email"` |
| `DR10-02` | Thử thêm từ `"e-mail"` đã `Deprecated` vào một bộ từ vựng mới | System reject với lỗi `CANNOT_ADD_DEPRECATED_VOCABULARY` |
| `DR10-03` | Gộp từ `VocabularyId_10` (nguồn) vào `VocabularyId_20` (đích) | `VocabularyId_10` chuyển `Archived`, `ReplacementVocabularyId = 20` |
| `DR10-04` | Người học A có tiến độ SRS cho cả từ 10 và từ 20 | Hợp nhất tiến độ A, giữ `MasteryScore` cao hơn |
| `DR10-05` | Thay thế từ 10 bằng từ 20 trong Bộ từ 108 | `SetVocabulary` của bộ 108 đổi từ 10 thành 20, tính lại $D_{set}$ |
| `DR10-06` | Quét tác động tham chiếu cho từ vựng đang có phòng đấu PvP M08 | Trả về `BLOCKING_HARD_DEPENDENCY`, chặn thao tác thay thế |
| `DR10-07` | Quét tác động tham chiếu cho từ vựng không có active ref nào | Trả về `SAFE_NO_ACTIVE_REF`, cho phép thực thi ngay |
| `DR10-08` | Thử thay thế bằng một từ mới có `QualityScore = 60%` | System reject với lỗi `SUBSTANDARD_REPLACEMENT_HEADWORD` |
| `DR10-09` | Thử thay thế bằng từ chưa clear bản quyền (`rightsCleared = false`) | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `DR10-10` | Kiểm tra bảng `Vocabularies` sau khi gộp từ | Từ nguồn giữ nguyên ID trong CSDL, không bị xoá cứng |
| `DR10-11` | Khai thác từ khóa `"e-mail"` trong thanh tìm kiếm | Động cơ Variant Matcher tìm thấy từ đích `"email"` |
| `DR10-12` | Xóa cache Redis sau khi thực hiện Hợp nhất từ vựng | Cache `lesson_payload` bị purge lập tức |
| `DR10-13` | Thao tác gộp từ bị gián đoạn giữa chừng do lỗi DB | Rollback transaction toàn bộ, tiến độ M04 không bị hỏng |
| `DR10-14` | Xem vết Audit Log M11 sau thao tác Ngừng dùng mục từ | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `DR10-15` | User không có quyền Content Admin yêu cầu gộp từ | Deny 403 Forbidden |
| `DR10-16` | Thao tác thay thế từ vựng trong Bộ từ Tiêu chuẩn A1 | Trí tuệ nhân tạo nạp gợi ý từ thay thế cùng trình độ A1 |
| `DR10-17` | Tải đồng thời 50 request quét tác động tham chiếu | Response p95 $< 30\text{ms}$ |
| `DR10-18` | Khôi phục lại mục từ đã `Deprecated` về `Published` | Chuyển trạng thái `Published` thành công sau khi kiểm tra |
| `DR10-19` | Gộp từ vựng có 10 câu ví dụ ngữ cảnh | Giữ lại các câu ví dụ chất lượng nhất cho từ đích |
| `DR20-20` | Kiểm thử hoàn tất luồng ngừng dùng, hợp nhất và thay thế M02-DEPRECATION-REPLACEMENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-DR-I01` | Entity `Vocabulary.cs` chưa có thuộc tính `ReplacementVocabularyId` | Chưa hỗ trợ ánh xạ từ thay thế khi ngừng dùng | M02-T049 (Source task) |
| `M02-DR-I02` | Chưa có Service thực thi Phân tích Tác động Tham chiếu 5 Tầng | Rủi ro gây lỗi runtime cho M03/M08 khi thay thế từ vựng | M02-T049 |
| `M02-DR-I03` | Chưa có thuật toán gộp hợp nhất tiến độ học tập `UserVocabularyProgress` | Gộp từ có nguy cơ làm mất điểm ghi nhớ SRS của người học | M02-T049 |
| `M02-DR-I04` | API cập nhật bộ từ vựng chưa tính toán lại `SetDifficultyIndex` sau thay thế | Chỉ số độ khó bộ từ bị sai lệch sau khi đổi từ | M02-T049 |
| `M02-DR-I05` | Chưa lồng ghép sự kiện vinh danh gộp từ vào Động cơ Variant Matcher | Người học gõ từ cũ bị tính sai điểm sau khi gộp từ | M02-T049 |

- `M02-DR-F01`: Thêm trường `ReplacementVocabularyId` vào `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-DR-F02`: Triểnkai `HeadwordDeprecationService` với 3 giao thức Deprecate, Merge, Replace (tiếp nhận: M02-T049).
- `M02-DR-F03`: Tích hợp Phân tích Tác động Tham chiếu 5 Tầng trước khi thay thế (tiếp nhận: M02-T049).
- `M02-DR-F04`: Thiết lập bộ kiểm thử tự động DR-G01–G10 và DR10-01–20 (tiếp nhận: M02 tasks).
- `M02-DR-F05`: Thu thập bằng chứng runtime cho luồng ngừng dùng và hợp nhất M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T010

- Đã thiết kế hoàn chỉnh `M02-DEPRECATION-REPLACEMENT-1.0` với 3 giao thức: Deprecate, Merge, Replace.
- Đã chốt quy tắc Phân tích Tác động Tham chiếu 5 Tầng (SAFE, WARN, BLOCKING) trước khi thực hiện.
- Đã bảo tồn 100% lịch sử tiến độ ôn tập SRS M04 và nguyên tắc Cấm Xóa Cứng CSDL.
- Đã lồng ghép cơ chế tự động chuyển `WordCanonical` cũ sang `VocabularyVariant` của mục từ mới.
- Đã xác lập 10 Regression Gates (`DR-G01`–`DR-G10`) và 20 Test Cases tự kiểm (`DR10-01`–`DR10-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế ngừng dùng, hợp nhất và thay thế M02-T010 | WSA-7K2 |
