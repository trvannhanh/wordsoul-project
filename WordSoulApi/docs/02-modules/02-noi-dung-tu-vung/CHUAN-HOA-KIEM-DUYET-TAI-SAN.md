# Chuẩn hóa kiểm duyệt tài sản M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-ASSET-MODERATION-1.0` |
| Task | M02-T012 |
| Đầu vào | M02-VOCAB-ASSET-CATALOG-1.0 (D-071), M12-ASSET-RIGHTS-LEDGER-1.0 (D-068), REL-04 |
| Phạm vi | Quy trình 4 bước kiểm duyệt tài sản phương tiện học liệu, ma trận trạng thái kiểm duyệt và giao thức từ chối/gỡ bỏ khi vi phạm bản quyền hoặc nội dung độc hại |
| Tự kiểm | A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Quy trình Kiểm duyệt và Phê duyệt Tài sản Phương tiện Học liệu (`Media Asset Moderation Pipeline`) thuộc M02, bảo đảm 100% hình ảnh và âm thanh gán cho mục từ vựng hoặc bộ từ vựng đạt tiêu chuẩn kỹ thuật, sạch bản quyền và không chứa nội dung độc hại.

- **Quy trình Kiểm duyệt 4 Bước Bắt buộc (`4-Stage Moderation Gate`)**: Mọi tài sản phương tiện trước khi được đính kèm vào mục từ `Published` phải trải qua 4 bước: (1) Quét kỹ thuật & format thô, (2) Kiểm tra bản quyền REL-04 / CT-01, (3) Quét nội dung độc hại tự động bằng AI, (4) Phê duyệt cuối cùng của Quản trị viên Biên tập.
- **Ràng buộc Chống Vi phạm Bản quyền Cứng (`REL-04 / CT-01 Non-Bypass Invariant`)**: Nếu bước 2 phát hiện `rightsCleared == false` hoặc giấy phép không hợp lệ, hệ thống TỰ ĐỘNG CHẶN kiểm duyệt và chuyển tài sản sang trạng thái `REJECTED_COPYRIGHT`. CẤM phê duyệt thủ công đè lên vi phạm bản quyền.
- **Phân tách Trạng thái Phê duyệt (`Moderation State Machine Isolation`)**: Chỉ tài sản có trạng thái kiểm duyệt chính thức `APPROVED_FOR_PUBLISH` mới được phép đính kèm vào DTO công khai bài học M03.
- **Giao thức Thu hồi Tự động khi Phát hiện Vi phạm sau Xuất bản (`Post-Publish Takedown Protocol`)**: Nếu tài sản đã xuất bản bị báo cáo hoặc bị hủy bản quyền từ M12, hệ thống tự động khóa tài sản (`Status = REJECTED_VIOLATION`) và kích hoạt Lệnh Thu hồi Khẩn cấp $\le 60\text{s}$ Emergency Recall M11-T019.

## 2. Quy trình Kiểm duyệt 4 Bước (4-Stage Moderation Pipeline)

```
[Ingest / Upload Asset Candidate]
               |
               v
  [Stage 1: Technical & Malware Scan]
  - Valid MIME (MP3/WebP) & MaxSize
  - Antivirus & Shellcode Scan
               |
               v
  [Stage 2: Copyright & License Gate] (REL-04 / CT-01)
  - Call AssetRightsService.VerifyRights(assetId)
  - Must have rightsCleared == true
               |
               v
  [Stage 3: AI Safety & Content Screening]
  - Vision AI / Audio Screening for NSFW / Hate Speech
  - Auto score SafetyConfidence >= 95%
               |
               v
  [Stage 4: Human Admin Approval]
  - R03 Content Admin Review
  - Approve -> Status = APPROVED_FOR_PUBLISH
```

## 3. Ma trận Trạng thái Kiểm duyệt Tài sản (Asset Moderation States)

| Trạng thái | Mã State | Cho phép đính kèm Pub? | Mô tả chi tiết | Hành động tiếp theo |
|---|---|---|---|---|
| Chờ kiểm duyệt | `PENDING_REVIEW` | KHÔNG | Tài sản mới nạp, đang ở hàng chờ kiểm duyệt 4 bước | Chờ AI & Admin duyệt |
| Tự động Đạt | `AUTOMATED_CLEARED` | KHÔNG | Đã vượt qua Bước 1, 2, 3 tự động, chờ Admin duyệt bước 4 | Phân công Content Admin |
| Đã Duyệt Xuất bản | `APPROVED_FOR_PUBLISH` | CÓ | Đã vượt qua cả 4 bước, sẵn sàng đính kèm mục từ public | Cho phép nạp DTO M03 |
| Từ chối - Kỹ thuật | `REJECTED_TECHNICAL_FAIL` | KHÔNG | Lỗi dung lượng quá lớn, sai MIME type hoặc nén hỏng | Yêu cầu nạp lại file |
| Từ chối - Bản quyền | `REJECTED_COPYRIGHT` | KHÔNG | Vi phạm bản quyền REL-04 hoặc giấy phép không rõ nguồn gốc | Khóa vĩnh viễn file |
| Từ chối - Độc hại | `REJECTED_OFFENSIVE` | KHÔNG | Chứa nội dung NSFW, bạo lực, ngôn từ kích động thù hận | Khóa & phát cảnh báo an ninh |
| Bị Thu hồi Khẩn | `REJECTED_VIOLATION` | KHÔNG | Tài sản bị phát hiện vi phạm sau khi xuất bản | Thu hồi khẩn $\le 60\text{s}$ |

## 4. Giao thức Xử lý Từ chối và Báo cáo Vi phạm (Rejection Protocol)

```csharp
public async Task RejectAssetAsync(string assetId, ModerationRejectReason reason, string reviewerNote, string reviewerActorId)
{
    var asset = await _db.DigitalAssets.FirstOrDefaultAsync(a => a.AssetId == assetId);
    if (asset == null) return;

    // 1. Cập nhật trạng thái từ chối
    asset.ModerationState = reason switch
    {
        ModerationRejectReason.CopyrightViolation => "REJECTED_COPYRIGHT",
        ModerationRejectReason.OffensiveContent => "REJECTED_OFFENSIVE",
        _ => "REJECTED_TECHNICAL_FAIL"
    };

    // 2. Gỡ liên kết khỏi tất cả các mục từ/bộ từ vựng đang tham chiếu
    await _vocabAssetService.UnlinkAssetFromAllVocabulariesAsync(assetId);

    // 3. Ghi vết Audit Event M11
    await _auditService.LogEventAsync("ACT-M11-04", reviewerActorId, new { assetId, reason, reviewerNote });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AM-G01` | 100% tài sản phương tiện học liệu trải qua đầy đủ 4 bước kiểm duyệt trước khi `APPROVED_FOR_PUBLISH`. |
| `AM-G02` | Cấm xuất bản mục từ chứa tài sản chưa đạt trạng thái `APPROVED_FOR_PUBLISH`. |
| `AM-G03` | Cấm đè duyệt thủ công (Manual Override) nếu Bước 2 kiểm tra bản quyền trả về `rightsCleared == false`. |
| `AM-G04` | Quét AI tự động chặn 100% tài sản bị phát hiện NSFW hoặc ngôn từ đe dọa/kích động thù hận. |
| `AM-G05` | Thao tác từ chối tài sản tự động gỡ liên kết khỏi tất cả các mục từ vựng liên quan trong $\le 60$ giây. |
| `AM-G06` | Mọi quyết định duyệt/từ chối tài sản ghi nhận vết Audit Event M11 với lý do và ActorId người duyệt. |
| `AM-G07` | Tài sản bị `REJECTED_COPYRIGHT` hoặc `REJECTED_OFFENSIVE` tự động bị purge khỏi CDN Cache public. |
| `AM-G08` | Phân quyền phê duyệt bước 4 tuân thủ nghiêm ngặt ma trận vai trò M11 (`R03 Content Admin`). |
| `AM-G09` | SLA xử lý kiểm duyệt tài sản tự động Bước 1-3 $< 3\text{s}$, bước duyệt Admin $< 24\text{h}$. |
| `AM-G10` | 100% các test case tự kiểm AM12-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AM12-01` | Nạp file phát âm MP3 đạt 100% tiêu chí 4 bước kiểm duyệt | Chuyển trạng thái `APPROVED_FOR_PUBLISH`, cho phép gán mục từ |
| `AM12-02` | Thử phê duyệt tài sản chưa qua Bước 2 kiểm tra bản quyền | System reject phê duyệt với lỗi `COPYRIGHT_SCAN_PENDING` |
| `AM12-03` | Nạp file ảnh chứa giấy phép không rõ nguồn gốc (`rightsCleared = false`) | Bước 2 tự động gắn trạng thái `REJECTED_COPYRIGHT` |
| `AM12-04` | Admin cố tình bấm "Duyệt đè" cho file bị `REJECTED_COPYRIGHT` | System deny operation, báo lỗi `CANNOT_OVERRIDE_COPYRIGHT_GATE` |
| `AM12-05` | Nạp file ảnh có chứa hình ảnh nhạy cảm NSFW | Bước 3 AI Screening tự động từ chối, chuyển `REJECTED_OFFENSIVE` |
| `AM12-06` | Từ chối một tài sản âm thanh đang được đính kèm vào 5 mục từ | Tự động gỡ liên kết khỏi 5 mục từ, tính lại `QualityScore` |
| `AM12-07` | Phát hiện tài sản đã xuất bản bị khiếu nại bản quyền | Chuyển `REJECTED_VIOLATION`, kích hoạt Emergency Recall $\le 60\text{s}$ |
| `AM12-08` | Tra cứu hàng chờ tài sản chờ Admin duyệt (`AUTOMATED_CLEARED`) | Trả về danh sách tài sản đã qua 3 bước tự động |
| `AM12-09` | Phê duyệt đồng loạt 20 tài sản âm thanh hợp lệ | Chuyển `APPROVED_FOR_PUBLISH` cho cả 20 tài sản |
| `AM12-10` | Nạp file âm thanh bị nén lỗi không thể playback | Bước 1 Technical Scan tự động gắn `REJECTED_TECHNICAL_FAIL` |
| `AM12-11` | User không có quyền Content Admin bấm phê duyệt tài sản | Deny 403 Forbidden |
| `AM12-12` | Tải đồng thời 50 request quét kiểm duyệt tự động Bước 1-3 | Response latency p95 $< 1.5\text{s}$ |
| `AM12-13` | Purge CDN Cache cho file bị từ chối do vi phạm | File bị xóa khỏi CDN edge server trong $\le 60$ giây |
| `AM12-14` | Xem vết Audit Log M11 sau khi từ chối 1 tài sản | Ghi nhận Audit Event `ACT-M11-04` kèm `reviewerNote` |
| `AM12-15` | Khôi phục lại tài sản bị từ chối kỹ thuật sau khi tác giả upload lại file chuẩn | Nạp lại pipeline 4 bước cho file mới |
| `AM12-16` | Kiểm tra thời gian SLA kiểm duyệt tự động cho 100 file | 100% file qua Bước 1-3 trong thời gian $< 3\text{s}$ |
| `AM12-17` | Phân tích phụ thuộc trước khi gỡ 1 tài sản bị vi phạm | Quét các bộ từ vựng M02 bị ảnh hưởng (T020) |
| `AM12-18` | Nạp file ảnh bìa bộ từ WebP đã qua kiểm duyệt AI | Cho phép đính kèm `CoverImageAssetId` cho bộ từ |
| `AM12-19` | Báo cáo thống kê tỷ lệ từ chối tài sản theo lý do hàng tháng | Export báo cáo với các chỉ số vi phạm bản quyền/kỹ thuật |
| `AM12-20` | Kiểm thử hoàn tất luồng chuẩn hóa kiểm duyệt tài sản M02-ASSET-MODERATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-AM-I01` | Trong `WordSoulApi`, chưa có workflow kiểm duyệt 4 bước cho tài sản media | Chưa có hạ tầng duyệt media riêng biệt trước khi gán từ vựng | M02-T049 (Source task) |
| `M02-AM-I02` | Thiếu thuộc tính `ModerationState` trên thực thể tài sản | Chưa phân định được tài sản đã duyệt hay đang chờ duyệt | M02-T049 |
| `M02-AM-I03` | Thiếu tích hợp AI Vision/Audio Content Screening cho Bước 3 | Phụ thuộc 100% vào duyệt tay gây chậm tiến độ kiểm duyệt | M02-T049 |
| `M02-AM-I04` | Thiếu cơ chế Cấm đè duyệt thủ công khi vi phạm bản quyền | Rủi ro Admin duyệt nhầm file vi phạm bản quyền tác giả | M02-T049; REL-04 |
| `M02-AM-I05` | Chưa có API purge CDN cache tự động khi từ chối tài sản vi phạm | File bị xóa trong DB nhưng vẫn truy cập được qua link CDN | M02-T049 |

- `M02-AM-F01`: Triển khai `AssetModerationPipeline` 4 bước chuẩn (tiếp nhận: M02-T049).
- `M02-AM-F02`: Thêm trường `ModerationState` vào bảng `DigitalAssets` (tiếp nhận: M02-T049).
- `M02-AM-F03`: Tích hợp AI Vision/Audio Screening Service cho Bước 3 (tiếp nhận: M02-T049).
- `M02-AM-F04`: Thiết lập bộ kiểm thử tự động AM-G01–G10 và AM12-01–20 (tiếp nhận: M02 tasks).
- `M02-AM-F05`: Thu thập bằng chứng runtime cho luồng kiểm duyệt tài sản M02 (tiếp nhận: M02 tasks; A-G03/REL-04).

## 8. Tự kiểm M02-T012

- Đã thiết kế hoàn chỉnh `M02-ASSET-MODERATION-1.0` với Quy trình Kiểm duyệt 4 Bước chuẩn hóa.
- Đã chốt Ràng buộc Chống Vi phạm Bản quyền Cứng (`REL-04 / CT-01`): Cấm đè duyệt thủ công khi `rightsCleared == false`.
- Đã xác lập Ma trận 7 Trạng thái Kiểm duyệt Tài sản (`APPROVED_FOR_PUBLISH`, `REJECTED_COPYRIGHT`, v.v.).
- Đã xây dựng Giao thức Thu hồi Tự động và Purge CDN Cache SLA $\le 60\text{s}$ khi có vi phạm.
- Đã xác lập 10 Regression Gates (`AM-G01`–`AM-G10`) và 20 Test Cases tự kiểm (`AM12-01`–`AM12-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa kiểm duyệt tài sản M02-T012 | WSA-7K2 |
