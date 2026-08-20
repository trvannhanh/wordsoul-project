# Xác định chuẩn chất lượng mục từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-ITEM-QUALITY-1.0` |
| Task | M02-T006 |
| Đầu vào | M02-MULTI-SENSE-1.0, M02-CEFR-DIFFICULTY-1.0, REL-04, CT-01 |
| Phạm vi | Tiêu chí đánh giá chuẩn chất lượng mục từ (Quality Score 7 tiêu chí), điều kiện phê duyệt xuất bản và tích hợp xác minh bản quyền phương tiện |
| Tự kiểm | A-G03; REL-04 khi có tài sản |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Bộ tiêu chí đánh giá chuẩn chất lượng mục từ (Headword Quality Score Engine) và điều kiện cứng để phê duyệt xuất bản (`published`) mục từ từ vựng trong hệ thống WordSoul M02.

- **Điểm Chất lượng Mục từ (`QualityScore` $\in [0, 100\%]$)**: Mỗi mục từ `Vocabulary` được đánh giá tự động dựa trên 7 tiêu chí thành phần. Mục từ đạt chất lượng phải có `QualityScore >= 80%`.
- **Ràng buộc Bản quyền Phương tiện Cứng (`REL-04 / CT-01 Mandatory Gate`)**: Trạng thái `rightsCleared == true` là ĐIỀU KIỆN BẮT BUỘC. Dù mục từ đạt $100\%$ điểm chất lượng nhưng nếu có phương tiện (Audio/Image) chưa clear bản quyền (`rightsCleared == false`), hệ thống CẤM TUYỆT ĐỐI việc chuyển trạng thái sang `published` (CT-01).
- **Phân cấp Danh hiệu Chất lượng (`Quality Badges`)**:
  - *Gold Quality ($100\%$)*: Đầy đủ định nghĩa 2 ngôn ngữ, IPA, 2 câu ví dụ, audio phát âm từ + câu, hình ảnh minh họa và đã clear bản quyền REL-04.
  - *Standard Quality ($80\% \to 99\%$)*: Đủ điều kiện xuất bản cơ bản (IPA, CEFR, định nghĩa, câu ví dụ, đã clear REL-04).
  - *Substandard ($< 80\%$)*: Thiếu thông tin quan trọng $\implies$ Giữ trạng thái `draft` / `in_review`, cấm xuất bản công khai.

## 2. Thang điểm 7 Tiêu chí Thành phần Đánh giá Chất lượng

| STT | Tiêu chí đánh giá | Điều kiện đạt | Trọng số điểm | Ràng buộc áp dụng |
|---|---|---|---|---|
| 1 | **Chuẩn hóa Mặt chữ & IPA** | `wordCanonical` hợp lệ và `pronunciationIpa` không rỗng (vd: `/vəˈkæbjələri/`) | $+15\%$ | Bắt buộc phải có |
| 2 | **Cấp độ CEFR Chuẩn** | `CEFRLevel` hợp lệ thuộc A1-C2 | $+15\%$ | Bắt buộc (M02-T005) |
| 3 | **Định nghĩa Đa ngôn ngữ** | Thực thể `VocabularySense` có cả `definitionVi` và `definitionEn` | $+20\%$ | Bắt buộc $\ge 1$ sense |
| 4 | **Câu Ví dụ Ngữ cảnh** | `exampleSentenceEn` và `exampleSentenceVi` đầy đủ, chính xác | $+20\%$ | Bắt buộc $\ge 1$ câu |
| 5 | **Âm thanh Phát âm (Audio)** | `audioUrl` hoặc `exampleAudioUrl` phát âm chuẩn giọng đọc | $+15\%$ | Cần có audio |
| 6 | **Hình ảnh Minh họa (Image)** | `imageUrl` đính kèm hình ảnh minh họa chất lượng cao | $+10\%$ | Khuyến khích |
| 7 | **Bản quyền Phương tiện (REL-04)** | `licenseId` hợp lệ và `rightsCleared == true` | $+5\%$ | **Mandatory Gate** (CT-01) |

## 3. Động cơ Tính Điểm Chất lượng Tự động (Quality Score Engine)

```csharp
public int CalculateQualityScore(Vocabulary vocab)
{
    int score = 0;

    // 1. Word & IPA (+15%)
    if (!string.IsNullOrWhiteSpace(vocab.WordCanonical) && !string.IsNullOrWhiteSpace(vocab.PronunciationIpa))
        score += 15;

    // 2. CEFR Level (+15%)
    if (vocab.CEFRLevel.HasValue)
        score += 15;

    // 3. Multi-Sense Definition (+20%)
    if (vocab.Senses != null && vocab.Senses.Any(s => !string.IsNullOrWhiteSpace(s.DefinitionVi) && !string.IsNullOrWhiteSpace(s.DefinitionEn)))
        score += 20;

    // 4. Context Example Sentence (+20%)
    if (vocab.Senses != null && vocab.Senses.Any(s => !string.IsNullOrWhiteSpace(s.ExampleSentenceEn) && !string.IsNullOrWhiteSpace(s.ExampleSentenceVi)))
        score += 20;

    // 5. Audio Asset (+15%)
    if (!string.IsNullOrWhiteSpace(vocab.AudioUrl) || (vocab.Senses != null && vocab.Senses.Any(s => !string.IsNullOrWhiteSpace(s.ExampleAudioUrl))))
        score += 15;

    // 6. Image Asset (+10%)
    if (!string.IsNullOrWhiteSpace(vocab.ImageUrl))
        score += 10;

    // 7. License Clearance (+5%)
    if (!string.IsNullOrWhiteSpace(vocab.LicenseId) && vocab.RightsCleared)
        score += 5;

    return score;
}
```

## 4. Điều kiện Phê duyệt Xuất bản Mục từ (Publish Approval Rules)

Khi Biên tập viên thực hiện Action `ACT-M11-04` (Approve/Publish Content):

```
[Request Publish Headword (VocabularyId)]
                   |
                   v
    (Calculate QualityScore & Check Rights)
                   |
     +-------------+-------------+
     |                           |
     v                           v
(QualityScore < 80%)      (rightsCleared == false)
     |                           |
     +-------------+-------------+
                   | (Matches either condition)
                   v
    [REJECT PUBLISH OPERATION]
    - ErrorCode: SUBSTANDARD_QUALITY / ASSET_RIGHTS_NOT_CLEARED
    - Log Audit Event: M11 REJECTED (M11-T031)
                   |
                   | (QualityScore >= 80% AND rightsCleared == true)
                   v
    [APPROVE & PUBLISH HEADWORD]
    - Status = 'published'
    - Purge Public Dictionary Cache M12
```

## 5. Quy trình Kiểm duyệt và Huy hiệu Chất lượng (Editor Checklist & Badges)

Giao diện Quản trị Nội dung M11 (Content Administration Dashboard) hiển thị danh sách mục từ kèm huy hiệu chất lượng:

- 🥇 **Gold Badge ($100\%$)**: Đạt tối đa toàn bộ 7 tiêu chí. Ưu tiên xuất bản vào các Bộ từ vựng Tiêu chuẩn.
- 🥈 **Standard Badge ($80\% \to 99\%$)**: Đủ tiêu chuẩn xuất bản công khai.
- ⚠️ **Draft / Substandard ($< 80\%$)**: Cần bổ sung thêm thông tin trước khi gửi duyệt.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IQ-G01` | `QualityScore` được tính toán tự động chính xác dựa trên 7 tiêu chí thành phần ($0 \to 100\%$). |
| `IQ-G02` | Cấm xuất bản mục từ công khai nếu `QualityScore < 80%`. |
| `IQ-G03` | Cấm xuất bản mục từ có tài sản phương tiện nếu `rightsCleared == false` (REL-04 / CT-01). |
| `IQ-G04` | Mục từ xuất bản bắt buộc có `WordCanonical`, `PronunciationIpa`, `CEFRLevel` và $\ge 1$ Nét nghĩa. |
| `IQ-G05` | Huy hiệu chất lượng (Gold, Standard, Substandard) được cập nhật thời gian thực khi sửa mục từ. |
| `IQ-G06` | Thao tác từ chối xuất bản vi phạm chất lượng/bản quyền sinh vết Audit Event M11 (`REJECTED`). |
| `IQ-G07` | Khi bổ sung file audio/hình ảnh đã clear bản quyền, `QualityScore` tự động tăng điểm tương ứng. |
| `IQ-G08` | Bộ từ vựng chỉ được phép xuất bản khi $100\%$ các từ bên trong đạt tiêu chuẩn chất lượng xuất bản. |
| `IQ-G09` | Phân quyền phê duyệt xuất bản tuân thủ nghiêm ngặt ma trận vai trò M11 (`R03 Content Admin`). |
| `IQ-G10` | 100% các test case tự kiểm IQ06-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IQ06-01` | Đánh giá mục từ có đầy đủ 7 tiêu chí thành phần | `QualityScore = 100%`, gắn huy hiệu Gold Badge |
| `IQ06-02` | Đánh giá mục từ có IPA, CEFR, định nghĩa, ví dụ, audio nhưng thiếu hình ảnh ($90\%$) | `QualityScore = 90%`, gắn huy hiệu Standard Badge |
| `IQ06-03` | Đánh giá mục từ thiếu câu ví dụ và audio ($60\%$) | `QualityScore = 60%`, gắn huy hiệu Substandard |
| `IQ06-04` | Thử xuất bản mục từ có `QualityScore = 60%` | System reject với lỗi `SUBSTANDARD_QUALITY_SCORE` |
| `IQ06-05` | Thử xuất bản mục từ có `QualityScore = 95%` nhưng `rightsCleared = false` | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `IQ06-06` | Xuất bản mục từ có `QualityScore = 90%` và `rightsCleared = true` | Xuất bản thành công, đổi trạng thái `published` |
| `IQ06-07` | Bổ sung file audio đã clear REL-04 cho mục từ $80\%$ | `QualityScore` tự động tăng từ $80\%$ lên $95\%$ |
| `IQ06-08` | Xóa bớt nét nghĩa duy nhất của một mục từ đang `published` | `QualityScore` giảm xuống $40\%$, cảnh báo chất lượng |
| `IQ06-09` | Thử xuất bản Bộ từ vựng chứa 1 mục từ Substandard ($60\%$) | Reject xuất bản bộ từ với lỗi `CONTAINS_SUBSTANDARD_HEADWORD` |
| `IQ06-10` | Tra cứu danh sách mục từ `draft` theo bộ lọc `QualityScore < 80%` | Trả về chính xác các mục từ cần hoàn thiện |
| `IQ06-11` | Thử xuất bản mục từ thiếu `CEFRLevel` | System reject với lỗi `MISSING_CEFR_LEVEL` |
| `IQ06-12` | Thử xuất bản mục từ thiếu `PronunciationIpa` | System reject với lỗi `MISSING_PRONUNCIATION_IPA` |
| `IQ06-13` | Kiểm tra tính nguyên tử giữa kiểm tra chất lượng và chuyển trạng thái | Đạt đồng thời 2 điều kiện mới commit status |
| `IQ06-14` | Biên tập viên xem báo cáo tỷ lệ mục từ đạt Gold Badge | Trả về thống kê chính xác theo từng bộ từ |
| `IQ06-15` | Đính kèm giấy phép `CC-BY-4.0` đã xác minh vào mục từ | Cập nhật `LicenseId`, `rightsCleared = true`, cộng điểm |
| `IQ06-16` | Thu hồi giấy phép bản quyền của file audio đính kèm | `rightsCleared` chuyển `false`, mục từ bị thu hồi về `in_review` |
| `IQ06-17` | Tải đồng thời 50 request tính điểm chất lượng mục từ | Response p95 $< 10\text{ms}$ từ in-memory score engine |
| `IQ06-18` | Người học truy cập từ điển công khai M02 | Chỉ thấy các mục từ `published` có `QualityScore >= 80%` |
| `IQ06-19` | Phân tích tham chiếu trước khi thu hồi mục từ bị vi phạm bản quyền | Quét các bộ từ ảnh hưởng để tạm ẩn (M11-T020) |
| `IQ06-20` | Kiểm thử hoàn tất luồng xác định chuẩn chất lượng M02-ITEM-QUALITY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-IQ-I01` | Entity `Vocabulary.cs` chưa có trường `QualityScore` tự động | Không có chỉ số đánh giá mức độ hoàn thiện mục từ | M02-T049 (Source task) |
| `M02-IQ-I02` | Chưa có Động cơ Đánh giá Chất lượng Mục từ (`QualityScoreEngine`) | Biên tập viên duyệt nội dung bằng cảm quan thủ công | M02-T049 |
| `M02-IQ-I03` | Thiếu bộ lọc ngăn chặn xuất bản khi `rightsCleared == false` (REL-04 / CT-01) | Rủi ro xuất bản tài sản âm thanh/hình ảnh vi phạm bản quyền | M02-T049; REL-04 |
| `M02-IQ-I04` | API xuất bản bộ từ vựng chưa kiểm tra chất lượng của các mục từ con | Có thể xuất bản bộ từ chứa các từ thiếu IPA hay định nghĩa | M02-T049 |
| `M02-IQ-I05` | Chưa hiển thị huy hiệu chất lượng (Gold, Standard, Substandard) trên DTO | DTO chưa truyền tải thông tin chất lượng cho admin | M02-T049 |

- `M02-IQ-F01`: Thêm trường `QualityScore` vào `Vocabulary.cs` và cài đặt `QualityScoreEngine` (tiếp nhận: M02-T049).
- `M02-IQ-F02`: Tích hợp bộ kiểm tra `QualityScore >= 80%` và `rightsCleared == true` vào Action duyệt M11 (tiếp nhận: M02-T049; REL-04).
- `M02-IQ-F03`: Thêm huy hiệu chất lượng vào DTO quản trị `VocabularyDetailDto.cs` (tiếp nhận: M02-T049).
- `M02-IQ-F04`: Thiết lập bộ kiểm thử tự động IQ-G01–G10 và IQ06-01–20 (tiếp nhận: M02 tasks).
- `M02-IQ-F05`: Thu thập bằng chứng runtime cho luồng chuẩn chất lượng M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T006

- Đã thiết kế hoàn chỉnh `M02-ITEM-QUALITY-1.0` với Động cơ Tính Điểm Chất lượng 7 tiêu chí thành phần ($0 \to 100\%$).
- Đã chốt điều kiện cứng phê duyệt xuất bản: `QualityScore >= 80%` AND `rightsCleared == true` (REL-04 / CT-01).
- Đã phân cấp 3 huy hiệu chất lượng (Gold Badge $100\%$, Standard $80-99\%$, Substandard $<80\%$).
- Đã lồng ghép quy định cấm xuất bản bộ từ chứa mục từ Substandard và ghi vết audit log M11.
- Đã xác lập 10 Regression Gates (`IQ-G01`–`IQ-G10`) và 20 Test Cases tự kiểm (`IQ06-01`–`IQ06-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả xác định chuẩn chất lượng mục từ M02-T006 | WSA-7K2 |
