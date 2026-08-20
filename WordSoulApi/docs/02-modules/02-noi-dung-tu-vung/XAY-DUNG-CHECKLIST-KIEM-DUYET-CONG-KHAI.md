# Xây dựng checklist kiểm duyệt công khai M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-PUBLIC-MODERATION-CHECKLIST-1.0` |
| Task | M02-T030 |
| Đầu vào | M02-HEADWORD-QUALITY-1.0 (D-061), M02-ASSET-MODERATION-1.0 (D-072), M02-VOCAB-SET-CRITERIA-1.0 (D-066), REL-04 |
| Phạm vi | Checklist 4 Trụ cột Kiểm duyệt cho Quản trị viên Biên tập (`ContentAdmin`), tiêu chuẩn đánh giá chất lượng sư phạm, xác minh bản quyền và ghi vết Sổ Kiểm toán M11 |
| Tự kiểm | A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Checklist và Giao thức Kiểm duyệt Bộ từ vựng Công khai (`Public Content Moderation Checklist Engine`) thuộc M02, chuẩn hóa các tiêu chuẩn mà Quản trị viên Biên tập (`ContentAdmin`) phải thực hiện đánh giá trước khi Phê duyệt (`Approve`), Từ chối (`Reject`) hoặc Yêu cầu Chỉnh sửa (`Request Changes`) một Bộ từ vựng.

- **4 Trụ cột Kiểm duyệt Bắt buộc (`4-Pillar Moderation Framework`)**: Mọi Bộ từ vựng gửi duyệt bắt buộc được kiểm tra độc lập qua 4 trụ cột: Kỹ thuật & Định dạng, Bản quyền & Tác quyền (REL-04 / CT-01), Chất lượng Sư phạm & Ngôn ngữ, An toàn AI & Chính sách.
- **Ràng buộc Quyết định Phê duyệt Tuyệt đối (`Zero-Violation Approval Invariant`)**: Bộ từ vựng CHỈ ĐƯỢC PHÉP phê duyệt xuất bản công khai khi $100\%$ các mục kiểm tra trong Checklist đạt kết quả PASS. CẤM Phê duyệt ngoại lệ hoặc bỏ qua vi phạm bản quyền (`No Override for Copyright Failures`).
- **Quy tắc Cấm Tác giả Tự duyệt (`Self-Approval Guard`)**: Tác giả nộp bài tuyệt đối KHÔNG thể đóng vai trò `ContentAdmin` để đánh giá Checklist của chính bộ từ do mình khởi tạo.
- **Lưu vết Sổ Kiểm toán Bất biến M11 (`Moderation Audit Trail`)**: $100\%$ kết quả đánh giá Checklist (Scorecard) được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-04`), đính kèm `ModeratorActorId`, `SetId`, `ScoreCardJson` và `DecisionResult`.

## 2. Danh mục 4 Trụ cột Kiểm duyệt Chi tiết (4-Pillar Checklist)

| Trụ cột (`Pillar`) | Mã Mục (`Item Code`) | Nội dung Kiểm tra | Tiêu chuẩn Bắt buộc (PASS Condition) | Loại Kiểm tra |
|---|---|---|---|---|
| **1. Kỹ thuật & Định dạng** | `CHK_TECH_01` | Kích thước Bộ từ | $5 \le ItemCount \le 50$ mục từ | Tự động |
| | `CHK_TECH_02` | Định dạng Media | $100\%$ Audio MP3 $\ge 128\text{kbps}$, Ảnh WebP $\le 2\text{MB}$ | Tự động |
| | `CHK_TECH_03` | Tính Toàn vẹn Link | $100\%$ đường dẫn CDN tải thành công $< 200\text{ms}$ | Tự động |
| **2. Bản quyền REL-04** | `CHK_COPYRIGHT_01` | Bản quyền Audio/Ảnh | $100\%$ tài sản media có `rightsCleared == true` (CT-01) | Tự động & Thủ công |
| | `CHK_COPYRIGHT_02` | Nguồn gốc Tác giả | Có giấy phép Creative Commons / Sở hữu trí tuệ hợp lệ | Thủ công |
| **3. Chất lượng Sư phạm** | `CHK_PEDAGOGY_01` | Điểm Chất lượng Từ | $100\%$ mục từ thành phần có `QualityScore >= 80%` | Tự động |
| | `CHK_PEDAGOGY_02` | Đồng nhất Trình độ | Dịch nghĩa & Ví dụ khớp với khung CEFR được khai báo | Thủ công |
| | `CHK_PEDAGOGY_03` | Chính xác Ngữ pháp | Không có lỗi chính tả, sai Part of Speech hoặc sai ngữ cảnh | Thủ công |
| **4. An toàn AI & Policy** | `CHK_SAFETY_01` | AI Safety Screening | AI Toxicity Score $< 0.05$, không lộ PII | Tự động |
| | `CHK_SAFETY_02` | Phù hợp Độ tuổi | Không chứa nội dung khiêu dâm, bạo lực hoặc kích động | Thủ công |

## 3. Cấu trúc Scorecard Kiểm duyệt (ModerationScorecard Schema)

```json
{
  "submissionTicketId": "TCK-SUB-2026-0820-0012",
  "vocabularySetId": 108,
  "moderatorActorId": "USR-ADM-007",
  "evaluatedAtUtc": "2026-08-20T10:15:00Z",
  "pillarResults": {
    "technicalCompliance": { "status": "PASS", "score": 100 },
    "copyrightGate": { "status": "PASS", "score": 100, "rightsCleared": true },
    "pedagogicalQuality": { "status": "PASS", "score": 92 },
    "aiSafetyScreening": { "status": "PASS", "score": 100 }
  },
  "overallResult": "APPROVED",
  "moderatorNotes": "Bộ từ biên soạn công phu, âm thanh chuẩn native, hình ảnh minh họa hợp lệ bản quyền."
}
```

## 4. Giao thức Đánh giá Checklist và Xử lý Quyết định (Moderation Evaluation Engine)

```csharp
public async Task<ModerationResultDto> EvaluateSubmissionAsync(string ticketId, ModerationEvaluationRequestDto dto, string moderatorActorId)
{
    var ticket = await _db.SubmissionTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
    var set = await _db.VocabularySets.FirstOrDefaultAsync(s => s.VocabularySetId == ticket.VocabularySetId);

    // 1. Check Self-Approval Guard
    if (set.CreatorId == moderatorActorId)
    {
        throw new InvalidOperationException("SELF_APPROVAL_FORBIDDEN", "Quản trị viên không được phép tự duyệt bộ từ do chính mình tạo.");
    }

    // 2. Chạy tự động Pillar 1, 2, 4
    var autoResult = await RunAutomatedPillarScansAsync(set);
    if (autoResult.HasFailures)
    {
        dto.OverallResult = ModerationDecision.Rejected;
    }

    // 3. Ghi nhận Scorecard và Cập nhật trạng thái Vòng đời (M02-T017)
    var scorecard = BuildScorecard(set, autoResult, dto, moderatorActorId);
    _db.ModerationScorecards.Add(scorecard);

    if (dto.OverallResult == ModerationDecision.Approved)
    {
        set.Status = VocabularySetStatus.Approved;
        set.IsMutationLocked = false;
    }
    else if (dto.OverallResult == ModerationDecision.RequestChanges)
    {
        set.Status = VocabularySetStatus.Draft;
        set.IsMutationLocked = false;
    }

    // 4. Ghi vết Audit Event M11
    await _auditLogService.RecordEventAsync("ACT-M11-04", moderatorActorId, scorecard);

    await _db.SaveChangesAsync();
    return MapToDto(scorecard);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `MC-G01` | Cấm Phê duyệt xuất bản nếu bất kỳ mục nào trong 4 Trụ cột Checklist bị FAIL. |
| `MC-G02` | Cấm phê duyệt nếu có tài sản media bị `rightsCleared == false` (`Zero-Violation Invariant` REL-04). |
| `MC-G03` | Cấm tác giả nộp bài thực hiện duyệt Checklist cho bộ từ của chính mình (`Self-Approval Guard`). |
| `MC-G04` | 100% đánh giá Checklist sinh ra `ModerationScorecard` lưu vết bất biến trong CSDL. |
| `MC-G05` | 100% thao tác Phê duyệt/Từ chối ghi vết Audit Event M11 (`ACT-M11-04`). |
| `MC-G06` | Kết quả Phê duyệt thành công tự động chuyển bộ từ sang `Approved` và mở khóa `IsMutationLocked = false`. |
| `MC-G07` | Kết quả Yêu cầu Chỉnh sửa chuyển bộ từ về `Draft` kèm Scorecard chi tiết các lỗi cần sửa. |
| `MC-G08` | Phân quyền thực thi duyệt Checklist chỉ dành riêng cho vai trò `ContentAdmin` và `SuperAdmin`. |
| `MC-G09` | SLA thực thi API đánh giá Checklist và chốt kết quả duyệt $< 80\text{ms}$. |
| `MC-G10` | 100% các test case tự kiểm MC30-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MC30-01` | `ContentAdmin` đánh giá PASS cả 4 Trụ cột cho Bộ từ 108 | Duyệt thành công, `overallResult = APPROVED`, chuyển `Approved` |
| `MC30-02` | Thử Phê duyệt Bộ từ có 1 ảnh bìa `rightsCleared = false` | Reject với lỗi `ZERO_VIOLATION_COPYRIGHT_FAILURE` (REL-04) |
| `MC30-03` | Thử Phê duyệt Bộ từ chứa 1 mục từ `QualityScore = 75%` ($< 80\%$) | Reject với lỗi `PEDAGOGICAL_QUALITY_BELOW_THRESHOLD` |
| `MC30-04` | Tác giả tự gửi request đánh giá Checklist cho Bộ từ của mình | System reject với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `MC30-05` | `ContentAdmin` chọn "Yêu cầu Chỉnh sửa" đối với lỗi chính tả | Chuyển bộ từ về `Draft`, mở khóa `IsMutationLocked = false` |
| `MC30-06` | `ContentAdmin` chọn "Từ chối" đối với vi phạm chính sách nặng | Chuyển bộ từ về `Archived`, thông báo từ chối đến tác giả |
| `MC30-07` | Tra cứu vết Audit Log M11 sau khi Phê duyệt bộ từ | Ghi nhận Audit Event `ACT-M11-04` kèm `ScorecardJson` |
| `MC30-08` | Tự động chạy quét AI Safety Screening phát hiện toxicity $> 0.1$ | Tự động dán nhãn FAIL cho Trụ cột 4, không cho phép Approve |
| `MC30-09` | Kiểm tra Scorecard lưu trong CSDL sau khi duyệt | Lưu trữ đầy đủ chi tiết kết quả 4 trụ cột và ghi chú của Admin |
| `MC30-10` | Duyệt Bộ từ chứa 50 mục từ tối đa | Chạy tự động quét 50 mục từ trong $< 40\text{ms}$, trả kết quả PASS |
| `MC30-11` | Thử truyền `moderatorNotes` rỗng khi Yêu cầu Chỉnh sửa | System reject với lỗi `MISSING_REJECTION_REASON` |
| `MC30-12` | Tải đồng thời 50 request duyệt Checklist từ 5 ContentAdmin khác nhau | Response latency p95 $< 70\text{ms}$ |
| `MC30-13` | User vai trò `Learner` hoặc `SupportAgent` thử gọi API duyệt Checklist | Deny 403 Forbidden (M01-ROLE-MATRIX-1.0) |
| `MC30-14` | User chưa đăng nhập thử gọi API duyệt Checklist | Deny 401 Unauthorized |
| `MC30-15` | Sửa lại bộ từ theo Yêu cầu Chỉnh sửa rồi gửi duyệt lại | Admin mới đánh giá lại Scorecard mới độc lập |
| `MC30-16` | Phê duyệt Bộ từ Cá nhân gửi lên công khai | Bộ từ được phép xuất bản lên catalog công khai |
| `MC30-17` | Phân tích tham chiếu trước khi chốt kết quả kiểm duyệt | Quét danh sách các mục từ thành phần M02 (T020) |
| `MC30-18` | Thao tác đánh giá Checklist bị gián đoạn giữa chừng do lỗi DB | Rollback transaction, bộ từ giữ nguyên `Submitted` |
| `MC30-19` | `SuperAdmin` thực hiện duyệt đè trong trường hợp khẩn cấp | Kiểm tra điều kiện copyright vẫn bắt buộc PASS 100% |
| `MC30-20` | Kiểm thử hoàn tất luồng checklist kiểm duyệt công khai M02-PUBLIC-MODERATION-CHECKLIST-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-MC-I01` | Hệ thống hiện chưa có bảng `ModerationScorecards` lưu chi tiết 4 trụ cột | Đánh giá duyệt bài của Admin chưa có bằng chứng chi tiết lưu vết | M02-T049 (Source task) |
| `M02-MC-I02` | Chưa có quy tắc cứng cấm Approve khi vi phạm bản quyền REL-04 | Admin vẫn có thể lỡ tay phê duyệt bộ từ vi phạm bản quyền | M02-T049; REL-04 |
| `M02-MC-I03` | Thiếu kiểm tra `Self-Approval Guard` trong API đánh giá duyệt | Tác giả có quyền Admin có thể tự duyệt bài của mình | M02-T049 |
| `M02-MC-I04` | Thiếu validation bắt buộc nhập `moderatorNotes` khi từ chối bài | Tác giả không biết lý do tại sao bộ từ bị từ chối | M02-T049 |
| `M02-MC-I05` | Chưa tự động chạy quét AI Safety Screening cho 100% câu ví dụ ghi đè | Rủi ro lọt ngôn từ độc hại trong câu ví dụ theo bộ | M02-T049; M02-T012 |

- `M02-MC-F01`: Tạo entity `ModerationScorecard.cs` và CSDL Migration (tiếp nhận: M02-T049).
- `M02-MC-F02`: Triển khai `PublicModerationService` với 4 Trụ cột Checklist (tiếp nhận: M02-T049; REL-04).
- `M02-MC-F03`: Tích hợp Zero-Violation Copyright Guard & Self-Approval Guard (tiếp nhận: M02-T049).
- `M02-MC-F04`: Thiết lập bộ kiểm thử tự động MC-G01–G10 và MC30-01–20 (tiếp nhận: M02 tasks).
- `M02-MC-F05`: Thu thập bằng chứng runtime cho luồng checklist kiểm duyệt M02 (tiếp nhận: M02 tasks; A-G03/REL-04).

## 8. Tự kiểm M02-T030

- Đã thiết kế hoàn chỉnh `M02-PUBLIC-MODERATION-CHECKLIST-1.0` với Khung 4 Trụ cột Kiểm duyệt Chi tiết.
- Đã chốt Ràng buộc Quyết định Phê duyệt Tuyệt đối (`Zero-Violation Invariant` REL-04).
- Đã quy định cấu trúc `ModerationScorecard` và Giao thức Đánh giá Tự động/Thủ công.
- Đã lồng ghép bảo vệ Cấm tự duyệt (`Self-Approval Guard`) và Lưu vết Sổ Kiểm toán M11 (`ACT-M11-04`).
- Đã xác lập 10 Regression Gates (`MC-G01`–`MC-G10`) và 20 Test Cases tự kiểm (`MC30-01`–`MC30-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả checklist kiểm duyệt công khai M02-T030 | WSA-7K2 |
