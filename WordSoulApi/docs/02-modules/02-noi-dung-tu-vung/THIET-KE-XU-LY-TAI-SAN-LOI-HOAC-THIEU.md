# Thiết kế xử lý tài sản lỗi hoặc thiếu M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-ASSET-DEGRADATION-1.0` |
| Task | M02-T013 |
| Đầu vào | M02-VOCAB-ASSET-CATALOG-1.0 (D-071), M02-ASSET-MODERATION-1.0 (D-072), M12-RESULT-1.0 (D-022), M12-FAIL-MODE-1.0 (D-025) |
| Phạm vi | Chiến lược suy giảm chức năng mềm (`Graceful Degradation`) khi tài sản phương tiện bị thiếu, lỗi tải hoặc bị thu hồi; hạ tầng fallback 3 cấp và cơ chế cảnh báo tự động |
| Tự kiểm | A-G03, A-G04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Chiến lược Suy giảm Chức năng Mềm (`Graceful Degradation Protocol`) thuộc M02 khi đối mặt với các tình huống tài sản phương tiện (âm thanh, hình ảnh) bị thiếu trong CSDL, bị lỗi truyền tải CDN hoặc bị thu hồi khẩn cấp.

- **Không Gây Crash Phiên học M03 (`No Session Crash Invariant`)**: Sự cố thiếu file âm thanh hoặc hình ảnh của mục từ TUYỆT ĐỐI KHÔNG làm sập hoặc treo phiên học/kiểm tra M03. Hệ thống bắt buộc tự động kích hoạt cơ chế Fallback thích ứng trong thời gian $< 50\text{ms}$.
- **Hạ tầng Fallback 3 Cấp độ (`3-Tier Fallback Hierarchy`)**:
  1. *Cấp 1 (Client-side Web Speech Synthesis)*: Nếu thiếu file âm thanh MP3 lưu trữ, client tự động phát âm bằng Web Speech API trên trình duyệt/thiết bị di động.
  2. *Cấp 2 (Placeholder Media Render)*: Nếu thiếu file hình ảnh minh họa, client hiển thị ảnh đại diện chủ đề hoặc biểu tượng SVG phân loại loại từ (Noun, Verb, Adjective).
  3. *Cấp 3 (Text-Only Mode)*: Nếu cả audio và image đều thiếu, giao diện tự động chuyển sang chế độ hiển thị thuần văn bản (Text-Only) và gỡ biểu tượng nghe phát âm.
- **Trừ điểm Chất lượng & Tự động Đặt Cảnh báo (`Asset Health Alerting`)**: Mục từ bị thiếu âm thanh phát âm bị tự động trừ $25\%$ `QualityScore` (M02-T006). Nếu tình trạng thiếu kéo dài $> 48$ giờ, hệ thống phát cảnh báo `WARN_VOCAB_MISSING_AUDIO` tới Admin.
- **Metric Giám sát Sức khỏe Tài sản (`Prometheus Asset Metric`)**: Xuất chỉ số `wordsoul_vocab_missing_audio_total` và `wordsoul_vocab_missing_image_total` theo dõi thời gian thực.

## 2. Ma trận Chiến lược Fallback khi Tài sản Lỗi/Thiếu (Asset Fallback Matrix)

| Loại Tài sản | Tình huống Lỗi | Trạng thái Lỗi (`Error Status`) | Chiến lược Fallback | Trải nghiệm Người học | Tác động QualityScore |
|---|---|---|---|---|---|
| `AUDIO_HEADWORD` | Nạp file MP3 thất bại (HTTP 404/500) | `MEDIA_NOT_FOUND` | Fallback Cấp 1: Web Speech Synthesis | Nghe phát âm bằng giọng đọc thiết bị | $-25\%$ QualityScore |
| `AUDIO_HEADWORD` | Chưa có audio (`AudioAssetId == null`) | `UNASSIGNED_ASSET` | Fallback Cấp 1: Web Speech Synthesis | Nghe phát âm bằng giọng đọc thiết bị | $-25\%$ QualityScore |
| `AUDIO_EXAMPLE_SENTENCE` | Thiếu file âm thanh câu ví dụ | `UNASSIGNED_ASSET` | Fallback Cấp 3: Ẩn nút loa phát âm ví dụ | Chỉ hiển thị văn bản câu ví dụ | $-10\%$ QualityScore |
| `IMAGE_HEADWORD` | Thiếu hoặc hỏng ảnh minh họa | `MEDIA_CORRUPTED` | Fallback Cấp 2: SVG POS Icon Placeholder | Hiển thị biểu tượng Loại từ (Noun/Verb) | $-15\%$ QualityScore |
| `IMAGE_SET_COVER` | Thiếu ảnh bìa Bộ từ vựng | `MEDIA_NOT_FOUND` | Fallback Cấp 2: Theme Gradient Cover | Hiển thị ảnh bìa dải màu chủ đề mặc định | Chặn xuất bản bộ từ |

## 3. Quy trình Xử lý Fallback và Phản hồi API (API Graceful Fallback Flow)

```
[M03 Quiz Engine Request Lesson Content Payload]
                        |
                        v
  [M02 LessonContentBuilderService]
  - Fetch Vocabulary & Media Asset Status
                        |
        +---------------+---------------+
        | (Media Available)             | (Media Missing / Corrupted)
        v                               v
  [Return Asset CDN URL]      [Apply Graceful Degradation]
                              - Set fallbackMode = 'WEB_SPEECH_SYNTHESIS'
                              - Set placeholderSvg = 'pos_noun.svg'
                              - Increment Prometheus Missing Metric
                              - Return Lesson Payload safely (< 50ms)
```

## 4. Giao thức Tự động Đặt Cảnh báo và Thu hồi Xuất bản (Health Escalation Protocol)

```csharp
public async Task ProcessMissingMediaHealthScanAsync()
{
    var missingAudioVocabs = await _db.Vocabularies
        .Where(v => v.Status == "Published" && v.AudioAssetId == null)
        .ToListAsync();

    foreach (var vocab in missingAudioVocabs)
    {
        // 1. Tự động tính toán lại QualityScore
        vocab.QualityScore = CalculateQualityScore(vocab);

        // 2. Nếu QualityScore < 80% -> Đổi trạng thái về InReview (M02-T007)
        if (vocab.QualityScore < 80)
        {
            vocab.Status = "InReview";
            _logger.LogWarning("Tự động chuyển từ {Word} về InReview do QualityScore = {Score}% < 80%", vocab.WordCanonical, vocab.QualityScore);
        }

        // 3. Đặt cảnh báo nếu thiếu > 48 giờ
        if (vocab.UpdatedAtUtc <= DateTime.UtcNow.AddHours(-48))
        {
            await _alertService.RaiseAlertAsync("WARN_VOCAB_MISSING_AUDIO", vocab.VocabularyId);
        }
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AD-G01` | Sự cố thiếu/lỗi tài sản phương tiện TUYỆT ĐỐI KHÔNG làm sập hoặc crash phiên học M03. |
| `AD-G02` | Khi thiếu `AUDIO_HEADWORD`, DTO tự động trả về `fallbackMode = 'WEB_SPEECH_SYNTHESIS'`. |
| `AD-G03` | Khi thiếu `IMAGE_HEADWORD`, DTO trả về đường dẫn `placeholderSvg` phân loại theo loại từ POS. |
| `AD-G04` | Phản hồi API nạp bài học M03 chứa thông tin fallback vẫn đạt SLA latency $< 50\text{ms}$. |
| `AD-G05` | Mục từ thiếu file phát âm bị tự động trừ $25\%$ điểm `QualityScore` trong CSDL. |
| `AD-G06` | Từ vựng `Published` bị tụt `QualityScore < 80%` do mất tài sản tự động chuyển về trạng thái `InReview`. |
| `AD-G07` | Hệ thống phát cảnh báo `WARN_VOCAB_MISSING_AUDIO` khi mục từ thiếu audio quá 48 giờ. |
| `AD-G08` | Chỉ số Prometheus `wordsoul_vocab_missing_audio_total` cập nhật chính xác theo thời gian thực. |
| `AD-G09` | Phân quyền xác nhận xử lý tài sản thiếu tuân thủ ma trận vai trò M11 (`R03 Content Admin`). |
| `AD-G10` | 100% các test case tự kiểm AD13-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AD13-01` | Nạp bài học cho từ vựng thiếu file phát âm MP3 | Trả về payload an toàn, `fallbackMode = 'WEB_SPEECH_SYNTHESIS'`, không crash M03 |
| `AD13-02` | Client nhận payload có `fallbackMode = 'WEB_SPEECH_SYNTHESIS'` | Trình duyệt kích hoạt Web Speech API phát âm từ vựng |
| `AD13-03` | Nạp bài học cho từ vựng thiếu file ảnh minh họa Noun | Trả về `placeholderSvg = "assets/placeholders/pos-noun.svg"` |
| `AD13-04` | Nạp bài học cho từ vựng thiếu cả audio và image | Trả về payload chế độ Text-Only an toàn |
| `AD13-05` | Tải file MP3 từ CDN bị timeout sau 2 giây | Client tự động chuyển sang Fallback Cấp 1 trong $< 20\text{ms}$ |
| `AD13-06` | Mục từ `"apple"` bị mất file phát âm MP3 | `QualityScore` giảm $25\%$, từ $90\%$ xuống $65\%$ |
| `AD13-07` | `QualityScore` từ `"apple"` giảm xuống $65\%$ ($< 80\%$) | Tự động chuyển trạng thái từ `"apple"` từ `Published` về `InReview` |
| `AD13-08` | Mục từ thiếu audio trong $50$ giờ ($> 48$ giờ) | Health Scanner phát cảnh báo `WARN_VOCAB_MISSING_AUDIO` |
| `AD13-09` | Khai thác metric Prometheus `wordsoul_vocab_missing_audio_total` | Trả về đúng số lượng mục từ đang thiếu audio |
| `AD13-10` | Bổ sung file audio MP3 mới cho từ vựng đang thiếu | `QualityScore` khôi phục $+25\%$, tự động cho phép Publish lại |
| `AD13-11` | Thiếu ảnh bìa cho Bộ từ vựng công khai | Trả về `placeholderCover = "assets/covers/gradient-theme-b1.png"` |
| `AD13-12` | Tải đồng thời 100 request nạp bài học chứa 20% mục từ lỗi media | Response p95 $< 40\text{ms}$ từ Redis cache |
| `AD13-13` | Thiếu âm thanh câu ví dụ cho nét nghĩa 5012 | Payload ẩn biểu tượng loa câu ví dụ, giữ nguyên text câu ví dụ |
| `AD13-14` | Tra cứu danh sách mục từ đang hoạt động ở chế độ Fallback | Trả về báo cáo danh sách từ vựng kèm loại fallback đang dùng |
| `AD13-15` | Khôi phục lại từ vựng bị chuyển `InReview` sau khi nạp đủ media | Chuyển lại `Published` sau khi duyệt thành công |
| `AD13-16` | User không phải Admin yêu cầu chạy Job quét sức khỏe media | Deny 403 Forbidden |
| `AD13-17` | Phân tích phụ thuộc trước khi tạm ẩn 1 mục từ thiếu media | Quét các bộ từ vựng M02 đang chứa từ vựng đó (T020) |
| `AD13-18` | Thử nạp bài học khi Redis Cache bị nghẽn | Fallback truy vấn DB trực tiếp, không làm crash M03 |
| `AD13-19` | Xem vết Audit Log M11 sau khi tự động chuyển trạng thái `InReview` | Ghi nhận Audit Event `ACT-M11-04` với lý do suy giảm media |
| `AD13-20` | Kiểm thử hoàn tất luồng xử lý tài sản lỗi hoặc thiếu M02-ASSET-DEGRADATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-AD-I01` | Trong DTO `VocabularySetFullDetailDto.cs`, chưa có thuộc tính `fallbackMode` | M03 chưa biết khi nào cần kích hoạt Client TTS | M02-T049 (Source task) |
| `M02-AD-I02` | Client ứng dụng chưa có bộ icon SVG Placeholder theo loại từ POS | Giao diện bị vỡ hình hoặc hiển thị ảnh rỗng 404 | M02-T049; FE-Task |
| `M02-AD-I03` | Thiếu Cron Job `MissingMediaHealthScannerJob` quét tự động | Từ vựng bị thiếu media vẫn nằm ở trạng thái `Published` bất hợp lệ | M02-T049 |
| `M02-AD-I04` | Thiếu metric Prometheus `wordsoul_vocab_missing_audio_total` | Chưa theo dõi được tỷ lệ bao phủ âm thanh trên tổng số từ vựng | M02-T049 |
| `M02-AD-I05` | Chưa lồng ghép việc trừ $25\%$ QualityScore khi thiếu audio vào `CalculateQualityScore` | Điểm chất lượng mục từ bị sai lệch so với thực tế | M02-T049 |

- `M02-AD-F01`: Bổ sung `fallbackMode` và `placeholderSvg` vào DTO bài học (tiếp nhận: M02-T049).
- `M02-AD-F02`: Xây dựng `MissingMediaHealthScannerJob` quét tự động hàng ngày (tiếp nhận: M02-T049).
- `M02-AD-F03`: Tích hợp Prometheus Metric theo dõi media health (tiếp nhận: M02-T049).
- `M02-AD-F04`: Thiết lập bộ kiểm thử tự động AD-G01–G10 và AD13-01–20 (tiếp nhận: M02 tasks).
- `M02-AD-F05`: Thu thập bằng chứng runtime cho luồng suy giảm tài sản M02 (tiếp nhận: M02 tasks; A-G03/A-G04).

## 8. Tự kiểm M02-T013

- Đã thiết kế hoàn chỉnh `M02-ASSET-DEGRADATION-1.0` với Hạ tầng Fallback 3 Cấp độ.
- Đã chốt Ràng buộc Không Gây Crash Phiên học M03 (`No Session Crash Invariant`) SLA $< 50\text{ms}$.
- Đã lồng ghép cơ chế trừ $25\%$ `QualityScore` và tự động chuyển về `InReview` khi thiếu media.
- Đã xây dựng Giao thức Cảnh báo `WARN_VOCAB_MISSING_AUDIO` sau 48 giờ và metric Prometheus.
- Đã xác lập 10 Regression Gates (`AD-G01`–`AD-G10`) và 20 Test Cases tự kiểm (`AD13-01`–`AD13-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế xử lý tài sản lỗi hoặc thiếu M02-T013 | WSA-7K2 |
