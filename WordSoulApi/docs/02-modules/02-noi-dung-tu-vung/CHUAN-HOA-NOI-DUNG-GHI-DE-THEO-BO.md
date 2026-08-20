# Chuẩn hóa nội dung ghi đè theo bộ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-VOCAB-OVERRIDE-1.0` |
| Task | M02-T022 |
| Đầu vào | M02-MULTI-SENSE-1.0 (D-057), M02-HEADWORD-QUALITY-1.0 (D-061), M02-SET-ITEM-MUTATION-1.0 (D-079) |
| Phạm vi | Giao thức ghi đè ngữ nghĩa và ví dụ ngữ cảnh của Từ vựng theo từng Bộ từ, quy tắc Fallback về từ điển gốc và kiểm duyệt an toàn nội dung ghi đè |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức Ghi đè Nội dung Từ vựng theo Ngữ cảnh Bộ từ (`Set-Level Vocabulary Contextual Override Protocol`) thuộc M02, cho phép tùy chỉnh nét nghĩa và ví dụ minh họa của một từ vựng cho phù hợp với chủ đề của Bộ từ vựng mà KHÔNG làm ảnh hưởng đến dữ liệu từ điển gốc.

- **Bảo toàn Tính Bất biến Từ điển Gốc (`Headword Dictionary Immutability Invariant`)**: Thao tác ghi đè ngữ nghĩa (`CustomMeaningOverride`) hoặc câu ví dụ (`CustomExampleSentence`) trong Bộ từ CHỈ lưu tại thực thể liên kết `SetVocabulary`. CẤM sửa đổi dữ liệu từ điển gốc `Headwords` / `VocabularySenses`.
- **Ràng buộc Fallback Tự động (`Automatic Fallback Priority Chain`)**:
  1. Nếu `CustomMeaningOverride != null` $\implies$ Hiển thị nghĩa ghi đè theo bộ.
  2. Ngược lại, nếu `SelectedSenseIds` chứa danh sách ID $\implies$ Hiển thị nét nghĩa được chọn trong từ điển.
  3. Ngược lại $\implies$ Hiển thị nét nghĩa chính mặc định (`IsPrimary == true`) của Từ vựng gốc (M02-T001).
- **Ràng buộc Độ dài và Kiểm duyệt An toàn (`Safety Screening & Length Limits`)**:
  - `CustomMeaningOverride`: Tối đa 300 ký tự.
  - `CustomExampleSentence`: Tối đa 500 ký tự.
  - Nội dung ghi đè bắt buộc trải qua Bộ lọc AI Safety Screening (M02-T012). CẤM chứa ngôn từ thù hận, vi phạm thuần phong mỹ tục hoặc thông tin nhạy cảm.

## 2. Mô hình Ghi đè Dữ liệu trong SetVocabulary (Override Attributes Schema)

```json
{
  "vocabularySetId": 108,
  "vocabularyId": 1024,
  "wordCanonical": "bank",
  "displayOrder": 3,
  "selectedSenseIds": [5012],
  "customMeaningOverride": "Ngân hàng thương mại cung cấp dịch vụ tài chính",
  "customExampleSentence": "The company secured a business loan from the central bank.",
  "fallbackMeaning": "Bờ sông, đê",
  "isOverridden": true
}
```

## 3. Thứ tự Ưu tiên Nạp Dữ liệu bài học DTO (Lesson DTO Override Chain)

```
[Fetch SetVocabulary Item for M03 Lesson]
                   |
                   v
    <Is CustomMeaningOverride Present?>
         /                   \
    (Yes)/                     \(No)
        v                       v
 [Use CustomMeaningOverride] <Is SelectedSenseIds Present?>
                                  /                 \
                             (Yes)/                   \(No)
                                 v                     v
                         [Use SelectedSense]   [Use Primary Sense]
```

## 4. Quy trình Cập nhật Nội dung Ghi đè (Update Override Workflow)

```csharp
public async Task<SetVocabularyDto> UpdateItemOverrideAsync(int setId, int vocabularyId, UpdateOverrideRequestDto dto, string currentUserId)
{
    // 1. Kiểm tra quyền thao tác trên Bộ từ (M02-T016)
    var set = await _db.VocabularySets.FirstOrDefaultAsync(s => s.VocabularySetId == setId);
    await _permissionGuard.ValidateMutationRightsAsync(set, currentUserId);

    var item = await _db.SetVocabularies.FirstOrDefaultAsync(sv => sv.VocabularySetId == setId && sv.VocabularyId == vocabularyId);
    if (item == null) throw new InvalidOperationException("ITEM_NOT_FOUND_IN_SET");

    // 2. Kiềm duyệt nội dung AI Safety (M02-T012)
    if (!string.IsNullOrEmpty(dto.CustomMeaningOverride))
    {
        if (dto.CustomMeaningOverride.Length > 300) throw new ArgumentException("OVERRIDE_MEANING_EXCEEDS_300_CHARS");
        await _aiSafetyFilter.ValidateContentAsync(dto.CustomMeaningOverride);
        item.CustomMeaningOverride = dto.CustomMeaningOverride.Trim();
    }

    if (!string.IsNullOrEmpty(dto.CustomExampleSentence))
    {
        if (dto.CustomExampleSentence.Length > 500) throw new ArgumentException("OVERRIDE_EXAMPLE_EXCEEDS_500_CHARS");
        await _aiSafetyFilter.ValidateContentAsync(dto.CustomExampleSentence);
        item.CustomExampleSentence = dto.CustomExampleSentence.Trim();
    }

    await _db.SaveChangesAsync();

    // 3. Xóa Cache Redis Lesson Payload M03
    await _cacheService.RemoveAsync($"lesson_payload:{setId}");

    return MapToDto(item);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `VO-G01` | Nội dung ghi đè chỉ lưu tại `SetVocabulary`, tuyệt đối KHÔNG làm thay đổi dữ liệu từ điển gốc `Headwords`. |
| `VO-G02` | Chuỗi `CustomMeaningOverride` vượt quá 300 ký tự bị từ chối với lỗi `OVERRIDE_MEANING_EXCEEDS_300_CHARS`. |
| `VO-G03` | Chuỗi `CustomExampleSentence` vượt quá 500 ký tự bị từ chối với lỗi `OVERRIDE_EXAMPLE_EXCEEDS_500_CHARS`. |
| `VO-G04` | Nội dung ghi đè vi phạm AI Safety Screening bị chặn lập tức và ghi log cảnh báo M11. |
| `VO-G05` | Khi `CustomMeaningOverride == null`, hệ thống tự động fallback nạp nét nghĩa từ điển gốc chuẩn xác. |
| `VO-G06` | Cập nhật nội dung ghi đè tự động xóa sạch Redis Cache `lesson_payload:{setId}` trong vòng $\le 10$ giây. |
| `VO-G07` | Phân quyền cập nhật nội dung ghi đè tuân thủ ma trận vai trò M02-T016 (`CreatorId` hoặc `ContentAdmin`). |
| `VO-G08` | DTO bài học M03 hiển thị chính xác nghĩa ghi đè theo ngữ cảnh của bộ từ đang học. |
| `VO-G09` | SLA xử lý cập nhật ghi đè ngữ nghĩa cho mục từ $< 40\text{ms}$. |
| `VO-G10` | 100% các test case tự kiểm VO22-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `VO22-01` | Ghi đè nghĩa cho từ "bank" trong Bộ từ Tiếng Anh Tài chính | Cập nhật `CustomMeaningOverride` thành công, giữ nguyên từ điển gốc |
| `VO22-02` | Nạp bài học M03 cho Bộ từ Tiếng Anh Tài chính | DTO hiển thị nghĩa ghi đè "Ngân hàng thương mại" |
| `VO22-03` | Nạp bài học M03 cho từ "bank" trong Bộ từ Địa lý không có ghi đè | DTO hiển thị fallback nét nghĩa gốc "Bờ sông, đê" |
| `VO22-04` | Thử ghi đè chuỗi nghĩa dài 350 ký tự ($> 300$) | Reject với lỗi `OVERRIDE_MEANING_EXCEEDS_300_CHARS` |
| `VO22-05` | Thử ghi đè ví dụ dài 600 ký tự ($> 500$) | Reject với lỗi `OVERRIDE_EXAMPLE_EXCEEDS_500_CHARS` |
| `VO22-06` | Thử ghi đè chuỗi chứa từ ngữ vi phạm AI Safety | Reject bởi AI Safety Filter, ghi log an ninh M11 |
| `VO22-07` | Xóa bỏ nội dung ghi đè (`CustomMeaningOverride = null`) | Tự động chuyển về hiển thị nghĩa mặc định từ điển |
| `VO22-08` | Người học B thử ghi đè từ vựng trong Bộ từ Cá nhân của Người học A | System reject 403 Forbidden (M02-T016) |
| `VO22-09` | Kiểm tra cache Redis `lesson_payload:108` sau khi sửa ghi đè | Cache cũ bị xóa, payload mới được cập nhật |
| `VO22-10` | Ghi đè cả nghĩa và ví dụ minh họa cùng một lúc | Cập nhật thành công cả 2 thuộc tính trong 1 transaction |
| `VO22-11` | Sao chép (Clone) Bộ từ có chứa nội dung ghi đè | Bộ từ con giữ nguyên $100\%$ các chuỗi ghi đè từ bộ từ gốc |
| `VO22-12` | Từ điển gốc cập nhật nét nghĩa mặc định | Bộ từ không ghi đè tự động nhận nghĩa mới, bộ từ ghi đè giữ nguyên nghĩa ghi đè |
| `VO22-13` | Tải đồng thời 50 request cập nhật ghi đè trên các bộ từ khác nhau | Response latency p95 $< 45\text{ms}$ |
| `VO22-14` | User chưa đăng nhập thử gọi API ghi đè nội dung | Deny 401 Unauthorized |
| `VO22-15` | Xem vết Audit Log M11 sau khi ghi đè nội dung từ vựng | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `VO22-16` | Ghi đè nghĩa cho mục từ trong Bộ từ Hệ thống bằng `ContentAdmin` | Cập nhật thành công, pre-generate Redis Payload |
| `VO22-17` | Phân tích tham chiếu trước khi ghi đè nội dung bộ từ | Quét các active session M03 đang mở bài học (T020) |
| `VO22-18` | Thao tác ghi đè bị gián đoạn do lỗi mạng | Rollback transaction, giữ nguyên nội dung trước đó |
| `VO22-19` | Chỉ định `SelectedSenseIds` kèm theo `CustomMeaningOverride` | Nghĩa ghi đè có ưu tiên cao nhất, `SelectedSenseIds` làm phụ |
| `VO22-20` | Kiểm thử hoàn tất luồng chuẩn hóa nội dung ghi đè M02-SET-VOCAB-OVERRIDE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-VO-I01` | Entity `SetVocabulary.cs` chưa có thuộc tính `CustomMeaningOverride` và `CustomExampleSentence` | Người dùng không thể tùy chỉnh nghĩa từ theo ngữ cảnh chủ đề bộ từ | M02-T049 (Source task) |
| `M02-VO-I02` | API nạp bài học M03 chưa triển khai chuỗi ưu tiên Fallback | Bài học luôn lấy nghĩa mặc định, bỏ qua ngữ cảnh của bộ từ | M02-T049 |
| `M02-VO-I03` | Thiếu validation giới hạn độ dài (300 char cho meaning, 500 char cho example) | Rủi ro lưu chuỗi quá dài gây tràn vỡ UI ứng dụng | M02-T049 |
| `M02-VO-I04` | Thiếu kiểm duyệt AI Safety Screening đối với nội dung ghi đè thủ công | Rủi ro người học cố tình chèn ngôn từ độc hại vào bộ từ công khai | M02-T049; M02-T012 |
| `M02-VO-I05` | Chưa xóa Redis Cache `lesson_payload` sau khi cập nhật ghi đè | M03 vẫn nạp payload nghĩa cũ từ Redis cache | M02-T049 |

- `M02-VO-F01`: Thêm `CustomMeaningOverride` và `CustomExampleSentence` vào `SetVocabulary.cs` (tiếp nhận: M02-T049).
- `M02-VO-F02`: Triển khai Fallback Priority Chain trong `LessonContentBuilderService` (tiếp nhận: M02-T049).
- `M02-VO-F03`: Tích hợp AI Safety Screening Filter cho luồng override (tiếp nhận: M02-T049; M02-T012).
- `M02-VO-F04`: Thiết lập bộ kiểm thử tự động VO-G01–G10 và VO22-01–20 (tiếp nhận: M02 tasks).
- `M02-VO-F05`: Thu thập bằng chứng runtime cho luồng ghi đè bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T022

- Đã thiết kế hoàn chỉnh `M02-SET-VOCAB-OVERRIDE-1.0` với Chuỗi Ưu tiên Fallback Tự động 3 Cấp.
- Đã chốt Ràng buộc Bảo toàn Tính Bất biến Từ điển Gốc (`Headwords`).
- Đã quy định giới hạn độ dài (300/500 char) và Bộ lọc AI Safety Screening.
- Đã lồng ghép xóa Redis Cache `lesson_payload` khi cập nhật nội dung ghi đè.
- Đã xác lập 10 Regression Gates (`VO-G01`–`VO-G10`) và 20 Test Cases tự kiểm (`VO22-01`–`VO22-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa nội dung ghi đè theo bộ M02-T022 | WSA-7K2 |
