# Thiết kế khiếu nại quyết định nội dung M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-CONTENT-DECISION-APPEAL-1.0` |
| Task | M02-T034 |
| Đầu vào | M02-REJECTION-REVISION-1.0 (D-085), M02-REPORT-EMERGENCY-RECALL-1.0 (D-114), REL-04 |
| Phạm vi | Đặc tả Giao thức Phân xử Khiếu nại Quyết định Nội dung (`Content Decision Appeal Protocol`), máy trạng thái 5 bước, cửa sổ khiếu nại 14 ngày, quy tắc phân công kiểm duyệt viên độc lập cấp 2 và minh chứng bản quyền REL-04 |
| Tự kiểm | A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Khiếu nại Quyết định Nội dung (`Content Decision Appeal Protocol`) thuộc M02, cung cấp cơ chế chính thức cho Tác giả bộ từ (ContentCreator) gửi đơn khiếu nại đối với các quyết định Từ chối duyệt (D-085) hoặc Thu hồi khẩn cấp (D-114), đảm bảo tính minh bạch, công bằng và bảo vệ quyền tác giả REL-04.

- **Cửa sổ Thời hạn Khiếu nại 14 Ngày (`14-Day Appeal Window Invariant`)**: Tác giả chỉ được phép nộp khiếu nại trong vòng 14 ngày kể từ thời điểm có quyết định Từ chối / Thu hồi (`AppealWindowDays = 14`). Sau 14 ngày, quyết định ban đầu trở thành vĩnh viễn và không thể khiếu nại.
- **Yêu cầu Bắt buộc Minh chứng Bản quyền REL-04 (`Mandatory Copyright Evidence Invariant`)**: Đơn khiếu nại vi phạm bản quyền BẮT BUỘC đính kèm tài liệu chứng minh quyền sở hữu hoặc giấy phép sử dụng hợp pháp (`AssetRightsLedgerId` / URL minh chứng REL-04 / CT-01) và lý do chi tiết `appealReason` $\ge 30$ ký tự.
- **Ràng buộc Phân công Kiểm duyệt viên Độc lập Cấp 2 (`Independent Second-Level Reviewer Invariant`)**: Đơn khiếu nại BẮT BUỘC được phân công cho một Chuyên viên Kiểm duyệt hoặc Admin CẤP CAO HƠN và ĐỘC LẬP (`ReviewerId != OriginalModeratorId`). CẤM kiểm duyệt viên cũ tự thẩm định lại khiếu nại của chính mình.
- **Máy Trạng thái Khiếu nại 5 Bước (`5-State Appeal Lifecycle Invariant`)**:
  - `SUBMITTED`: Đơn khiếu nại vừa được gửi bởi tác giả.
  - `UNDER_REVIEW`: Đã phân công kiểm duyệt viên độc lập cấp 2 xử lý.
  - `EVIDENCE_REQUESTED`: Yêu cầu tác giả bổ sung thêm bằng chứng bản quyền trong 7 ngày.
  - `APPEAL_APPROVED`: Khiếu nại được chấp thuận $\to$ Khôi phục trạng thái bộ từ (`APPROVED` / `PUBLISHED`).
  - `APPEAL_REJECTED`: Khiếu nại bị bác bỏ $\to$ Giữ nguyên quyết định từ chối/thu hồi ban đầu (Cuối cùng).

## 2. Ma trận Kết quả Khiếu nại Quyết định Nội dung (Appeal Outcome Matrix)

| Trạng thái Khiếu nại (`AppealState`) | Điều kiện Kích hoạt | Trạng thái Bộ từ Sau Phân xử | Thẩm quyền Quyết định | Hành động Hệ thống | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `SUBMITTED` | Tác giả nộp đơn trong 14d | Giữ nguyên (`REJECTED`/`RECALLED`) | Hệ thống Tự động | Ghi log `M02_ContentAppeals` | `ACT-M11-02-APP-SUBMIT` |
| `UNDER_REVIEW` | Gán cho Moderator cấp 2 | Giữ nguyên | Admin cấp cao | Phân công `ReviewerId != OriginalId` | `ACT-M11-02-APP-ASSIGN` |
| `EVIDENCE_REQUESTED` | Thiếu minh chứng REL-04 | Giữ nguyên (Hạn 7 ngày) | Moderator cấp 2 | Gửi PUSH/Email thông báo tác giả | `ACT-M11-02-APP-EVID` |
| `APPEAL_APPROVED` | Minh chứng bản quyền hợp lệ | Khôi phục `APPROVED` / `PUBLISHED` | ContentAdmin / SecurityAdmin | Xóa cờ Quarantined, Re-cache Redis | `ACT-M11-02-APP-APPROVE` |
| `APPEAL_REJECTED` | Minh chứng không đạt / Lỗi nặng | Giữ nguyên `REJECTED` / `RECALLED` | ContentAdmin / SecurityAdmin | Khóa khiếu nại vĩnh viễn cho version này | `ACT-M11-02-APP-REJECT` |

## 3. Kiến trúc Máy Trạng thái Khiếu nại (Appeal Engine Pipeline)

```
[ContentCreator Submits Appeal (SetId, Reason >= 30, RightsEvidenceId)]
                                 |
                                 v
          [Validate 14-Day Appeal Window & One-Appeal Constraint]
                                 |
                                 v
                 [Create Appeal Record: SUBMITTED]
                                 |
                                 v
           [Assign Independent Reviewer (ReviewerId != OriginalId)]
                                 |
                                 v
                        [State: UNDER_REVIEW]
                                 |
          +----------------------+----------------------+
          | (Evidence Missing)   | (Evidence Clear)     | (Evidence Invalid)
          v                      v                      v
[EVIDENCE_REQUESTED]    [APPEAL_APPROVED]      [APPEAL_REJECTED]
(7-day Upload Window)   - Restore Set State    - Final Rejection
                        - Evict Redis Cache    - Log Audit M11
                        - Log Audit M11
```

## 4. Giao thức Thực thi Phân xử Khiếu nại CSDL (ContentDecisionAppealService)

```csharp
public async Task<AppealResultDto> ProcessContentAppealAsync(
    string appealId, 
    AppealDecision decision, 
    string reviewerId, 
    string reviewerNotes)
{
    var appeal = await _db.ContentAppeals.FirstOrDefaultAsync(a => a.Id == appealId);
    if (appeal == null) throw new InvalidOperationException("APPEAL_NOT_FOUND");

    // 1. Independent Reviewer Guard
    if (reviewerId == appeal.OriginalModeratorId)
    {
        throw new UnauthorizedAccessException("INDEPENDENT_REVIEWER_REQUIRED: Kiểm duyệt viên xử lý khiếu nại phải độc lập với người đưa ra quyết định ban đầu.");
    }

    if (string.IsNullOrEmpty(reviewerNotes) || reviewerNotes.Length < 20)
    {
        throw new ArgumentException("REVIEWER_NOTES_MIN_LENGTH_20: Ghi chú phân xử khiếu nại phải tối thiểu 20 ký tự.");
    }

    var targetSet = await _db.VocabularySets.FirstOrDefaultAsync(s => s.Id == appeal.TargetSetId);

    // 2. Execute Appeal Decision Branch
    if (decision == AppealDecision.APPROVE)
    {
        appeal.State = AppealState.APPEAL_APPROVED;
        targetSet.State = VocabularySetState.PUBLISHED;
        targetSet.IsPublic = true;

        // Purge Cache to republish
        await _redisDb.KeyDeleteAsync($"wordsoul:set:{targetSet.Id}");
        await _redisDb.KeyDeleteAsync($"wordsoul:public_sets:catalog");
    }
    else if (decision == AppealDecision.REJECT)
    {
        appeal.State = AppealState.APPEAL_REJECTED;
        // Keep current REJECTED / RECALLED state
    }

    appeal.ReviewedByUserId = reviewerId;
    appeal.ReviewedAtUtc = DateTime.UtcNow;
    appeal.ReviewerNotes = reviewerNotes;

    await _db.SaveChangesAsync();

    // 3. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-02-APPEAL", reviewerId, new {
        AppealId = appealId,
        TargetSetId = targetSet.Id,
        Decision = decision.ToString(),
        Notes = reviewerNotes
    });

    return new AppealResultDto { AppealId = appealId, FinalState = appeal.State.ToString() };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AP-G01` | Đơn khiếu nại chỉ được chấp nhận trong vòng 14 ngày kể từ ngày ban hành quyết định từ chối / thu hồi. |
| `AP-G02` | Mỗi phiên bản bộ từ / mục từ chỉ được phép nộp duy nhất 1 đơn khiếu nại (Single Appeal Limit). |
| `AP-G03` | Người xử lý khiếu nại BẮT BUỘC là kiểm duyệt viên độc lập cấp 2 (`ReviewerId != OriginalModeratorId`). |
| `AP-G04` | Đơn khiếu nại vi phạm bản quyền BẮT BUỘC đính kèm minh chứng bản quyền hợp lệ (REL-04 / CT-01). |
| `AP-G05` | Lý do khiếu nại (`appealReason`) BẮT BUỘC tối thiểu 30 ký tự; ghi chú phân xử tối thiểu 20 ký tự. |
| `AP-G06` | Khiếu nại được duyệt (`APPEAL_APPROVED`) tự động khôi phục cờ `IsPublic` và xóa Cache Redis trong SLA $\le 30\text{s}$. |
| `AP-G07` | 100% các quyết định phân xử khiếu nại được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-02-APPEAL`). |
| `AP-G08` | Phân quyền phê duyệt đơn khiếu nại chỉ dành riêng cho `ContentAdmin`, `SecurityAdmin` và `SuperAdmin`. |
| `AP-G09` | SLA thực thi API tiếp nhận khiếu nại $< 20\text{ms}$; SLA phân xử khiếu nại $< 35\text{ms}$. |
| `AP-G10` | 100% các test case tự kiểm AP34-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AP34-01` | Tác giả nộp đơn khiếu nại hợp lệ cho Bộ từ A trong vòng 5 ngày sau từ chối | Nộp đơn khiếu nại thành công, chuyển `SUBMITTED` |
| `AP34-02` | Tác giả thử nộp đơn khiếu nại sau 16 ngày kể từ quyết định ($> 14$ ngày) | Reject 400 `APPEAL_WINDOW_EXPIRED_14D` |
| `AP34-03` | Tác giả nộp đơn khiếu nại nhưng nhập lý do 15 ký tự ($< 30$) | Reject 400 `APPEAL_REASON_MIN_LENGTH_30` |
| `AP34-04` | Tác giả khiếu nại vi phạm bản quyền nhưng không gửi đính kèm minh chứng REL-04 | Reject 400 `RIGHTS_EVIDENCE_REQUIRED_REL04` |
| `AP34-05` | OriginalModerator (người từ chối cũ) thử tự phê duyệt đơn khiếu nại của tác giả | Reject 403 `INDEPENDENT_REVIEWER_REQUIRED` |
| `AP34-06` | Moderator cấp 2 phê duyệt đơn khiếu nại `APPEAL_APPROVED` | Khôi phục bộ từ về `PUBLISHED`, dọn cache Redis SLA $< 30\text{s}$ |
| `AP34-07` | Moderator cấp 2 bác bỏ đơn khiếu nại `APPEAL_REJECTED` với ghi chú 25 ký tự | Giữ nguyên trạng thái `REJECTED`, khóa khiếu nại tiếp theo |
| `AP34-08` | Tra cứu vết Audit Log M11 sau khi phân xử khiếu nại | Ghi nhận Audit Event `ACT-M11-02-APPEAL` đính kèm ReviewerId |
| `AP34-09` | Tác giả thử nộp đơn khiếu nại thứ 2 cho cùng 1 phiên bản bộ từ bị bác bỏ | Reject 400 `MAX_APPEALS_PER_VERSION_REACHED` |
| `AP34-10` | Moderator cấp 2 yêu cầu bổ sung bằng chứng `EVIDENCE_REQUESTED` | Chuyển trạng thái khiếu nại, hẹn hạn 7 ngày cho tác giả |
| `AP34-11` | Tác giả không nộp bổ sung bằng chứng sau 7 ngày hẹn | Tự động chuyển đơn khiếu nại sang `APPEAL_REJECTED` |
| `AP34-12` | Tải đồng thời 50 request nộp đơn khiếu nại từ 50 tác giả | Processing latency p95 $< 18\text{ms}$ |
| `AP34-13` | User không phải Admin/Moderator thử gọi API phân xử khiếu nại | Deny 403 Forbidden |
| `AP34-14` | User chưa đăng nhập gọi API nộp đơn khiếu nại | Deny 401 Unauthorized |
| `AP34-15` | Tác giả tự rút đơn khiếu nại khi đang ở trạng thái `SUBMITTED` | Chuyển đơn khiếu nại sang `WITHDRAWN` |
| `AP34-16` | Kiểm tra thời gian cập nhật kết quả khiếu nại tới tác giả | Gửi PUSH/Email thông báo qua M10 trong SLA $< 10\text{s}$ |
| `AP34-17` | Phân tích tham chiếu các đơn khiếu nại trong CSDL M02 | Quét schema `M02_ContentAppeals` (T020) |
| `AP34-18` | Thao tác xóa cờ Quarantined trong Redis bị chập chập ngắt mạng | Retry tự động theo Outbox Pattern M12-T037 |
| `AP34-19` | Tra cứu danh sách các đơn khiếu nại đang chờ xử lý (`SUBMITTED`) | Trả về danh sách đơn khiếu nại xếp theo thứ tự thời gian nộp |
| `AP34-20` | Kiểm thử hoàn tất luồng thiết kế khiếu nại M02-CONTENT-DECISION-APPEAL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-AP-I01` | Codebase hiện tại chưa có bảng CSDL `M02_ContentAppeals` nộp khiếu nại | Tác giả không có kênh chính thức để phản hồi khi bị từ chối/thu hồi | M02-T049 (Source task) |
| `M02-AP-I02` | Thiếu cờ `ReviewerId != OriginalModeratorId` kiểm tra người phân xử độc lập | Risk người từ chối cũ tự duyệt lại đơn khiếu nại của chính mình | M02-T049; REL-02 |
| `M02-AP-I03` | Thiếu cờ validation cửa sổ thời gian 14 ngày (`APPEAL_WINDOW_EXPIRED_14D`) | Tác giả có thể khiếu nại các bộ từ đã bị từ chối từ nhiều tháng trước | M02-T049 |
| `M02-AP-I04` | Thiếu luồng bắt buộc minh chứng bản quyền REL-04 khi khiếu nại vi phạm | Không có căn cứ pháp lý để Admin khôi phục bộ từ | M02-T049; REL-04 |
| `M02-AP-I05` | Chưa kết nối sự kiện phân xử khiếu nại với Audit Log M11 (`ACT-M11-02-APPEAL`) | Không ghi vết được kết quả phân xử của Moderator cấp 2 | M02-T049; M11-T031 |

- `M02-AP-F01`: Triển khai `ContentDecisionAppealService` với Máy Trạng thái 5 Bước (tiếp nhận: M02-T049).
- `M02-AP-F02`: Tích hợp Bắt buộc Independent Reviewer Guard `ReviewerId != OriginalId` (tiếp nhận: M02-T049; REL-02).
- `M02-AP-F03`: Thiết lập Cửa sổ khiếu nại 14 ngày & Bắt buộc minh chứng bản quyền REL-04 (tiếp nhận: M02-T049; REL-04).
- `M02-AP-F04`: Thiết lập bộ kiểm thử tự động AP-G01–G10 và AP34-01–20 (tiếp nhận: M02 tasks).
- `M02-AP-F05`: Thu thập bằng chứng runtime cho luồng khiếu nại M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T034

- Đã thiết kế hoàn chỉnh `M02-CONTENT-DECISION-APPEAL-1.0` với Máy Trạng thái Khiếu nại 5 Bước.
- Đã chốt Ràng buộc Cửa sổ Thời hạn Khiếu nại 14 Ngày kể từ ngày từ chối/thu hồi.
- Đã chốt Ràng buộc Phân công Kiểm duyệt viên Độc lập Cấp 2 (`ReviewerId != OriginalModeratorId`).
- Đã lồng ghép Yêu cầu Minh chứng Bản quyền REL-04 và Lưu vết Audit Log M11 (`ACT-M11-02-APPEAL`).
- Đã xác lập 10 Regression Gates (`AP-G01`–`AP-G10`) và 20 Test Cases tự kiểm (`AP34-01`–`AP34-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế khiếu nại quyết định nội dung M02-T034 | WSA-7K2 |
