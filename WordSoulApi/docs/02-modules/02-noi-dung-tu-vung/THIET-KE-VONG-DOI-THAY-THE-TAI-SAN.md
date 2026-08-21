# Thiết kế vòng đời thay thế tài sản M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-ASSET-REPLACEMENT-LIFECYCLE-1.0` |
| Task | M02-T014 |
| Đầu vào | M02-VOCAB-ASSET-CATALOG-1.0 (D-071), M02-HEADWORD-VERSIONING-1.0 (D-063), M12-ASSET-REPLACEMENT-ORPHAN-CLEANUP-1.0 (D-128), REL-04 |
| Phạm vi | Đặc tả Giao thức Vòng đời Thay thế Tài sản Học liệu (`Learning Asset Replacement Lifecycle Protocol`), cơ chế tạo phiên bản mục từ mới khi thay thế media, ghim phiên học M03, tự động tính lại điểm chất lượng QualityScore và dọn dẹp Cache CDN SLA $\le 60$ giây |
| Tự kiểm | A-G03, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Vòng đời Thay thế Tài sản Học liệu (`Learning Asset Replacement Lifecycle Protocol`) thuộc M02, chuẩn hóa luồng thay thế tệp âm thanh phát âm hoặc hình ảnh minh họa cho các mục từ và bộ từ đã xuất bản, đảm bảo không gây xáo trộn phiên học đang diễn ra của người học và tuân thủ chặt chẽ bản quyền REL-04 (D-063, D-128).

- **Tạo Phiên bản Mục từ Mới khi Thay thế Media (`Asset Replacement Versioning Invariant`)**: Khi thay thế tài sản media (`OldAssetId` $\to$ `NewAssetId`) trên một mục từ đã xuất bản, hệ thống TỰ ĐỘNG tạo một phiên bản `VocabularyRevision` mới (M02-T008-A/D-063) và tính toán lại `RevisionDigest`. CẤM ghi đè trực tiếp tài sản trên phiên bản cũ.
- **Ràng buộc Ghim Phiên bản Phiên Học M03 (`Session Revision Pinning`)**: Các phiên học M03 đang diễn ra được ghim phiên bản snapshot cũ (`revisionDigest` D-082) để người học hoàn thành bài học bình thường mà không bị crash ứng dụng. Phiên học MỚI khởi tạo sau thời điểm thay thế sẽ nhận tài sản media mới.
- **Tự động Tính lại Điểm Chất lượng QualityScore (`QualityScore Recalculation`)**: Ngay khi thay thế tệp âm thanh chuẩn (MP3 chất lượng cao) cho một mục từ trước đó bị thiếu âm thanh, hệ thống TỰ ĐỘNG xóa bỏ điểm phạt $-25\%$ và khôi phục điểm `QualityScore` đạt mốc $\ge 80\%$.
- **Ràng buộc Kiểm duyệt Bản quyền Tài sản Mới REL-04 (`Rights Verification Gate`)**: Tài sản thay thế mới (`NewAssetId`) BẮT BUỘC có cờ `RightsCleared == true` và `LicenseType` hợp lệ trước khi được duyệt thay thế (REL-04, CT-01).

## 2. Ma trận Vòng đời Thay thế Tài sản (Asset Replacement Matrix)

| Kịch bản Thay thế (`Scenario`) | Trạng thái Tài sản Mới (`NewAsset`) | Tạo Version Mới M02 | Tác động Phiên Học M03 | Tác động Điểm QualityScore | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `REPLACE_AUDIO_CORRECT` | `RightsCleared == true` | Tạo `VocabularyRevision` N+1 | Ghim phiên cũ (No crash) | Xóa phạt $-25\%$, Tăng điểm | `ACT-M11-02-REPLACE_AUDIO` |
| `REPLACE_IMAGE_IMPROVED` | `RightsCleared == true` | Tạo `VocabularyRevision` N+1 | Ghim phiên cũ (No crash) | Tăng điểm hiển thị | `ACT-M11-02-REPLACE_IMAGE` |
| `REPLACE_UNCLEARED_REJECT` | `RightsCleared == false` | **REJECT (Không tạo)** | Giữ nguyên phiên cũ | Giữ nguyên | `ACT-M11-02-REJECT_RIGHTS` |
| `REPLACE_INVALID_MIME` | Sai định dạng tệp | **REJECT (Không tạo)** | Giữ nguyên phiên cũ | Giữ nguyên | `ACT-M11-02-REJECT_MIME` |

## 3. Kiến trúc Luồng Thay thế Tài sản Học liệu M02 (Replacement Engine Pipeline)

```
[Author / ContentAdmin Initiates Media Asset Replacement (VocabularyId, NewAssetId)]
                                    |
                                    v
            [Verify New Asset RightsCleared == true & Valid License REL-04]
                                    |
            +-----------------------+-----------------------+
            | (RightsCleared == false)                      | (RightsCleared == true)
            v                                               v
   [REJECT: 400 RIGHTS_NOT_CLEARED]             [Create New VocabularyRevision (N+1)]
                                                - Link NewAssetId to Revision
                                                - Recalculate RevisionDigest
                                                - Decrement RefCount on OldAssetId
                                                            |
                                                            v
                                                [Recalculate QualityScore M02]
                                                - Remove -25% penalty if audio added
                                                            |
                                                            v
                                                [Evict Redis Cache SLA <= 60s]
                                                - Active M03 sessions stay pinned
                                                - Record Audit Log ACT-M11-02-REPLACE
```

## 4. Giao thức Thực thi Thay thế Tài sản CSDL (VocabularyAssetReplacementService)

```csharp
public async Task<VocabularyRevisionDto> ReplaceVocabularyAssetAsync(
    string vocabularyId, 
    AssetType assetType, 
    string newAssetId, 
    string actorUserId)
{
    var vocab = await _db.Headwords
        .Include(h => h.Revisions)
        .FirstOrDefaultAsync(h => h.Id == vocabularyId);

    if (vocab == null) throw new KeyNotFoundException("VOCABULARY_NOT_FOUND");

    // 1. Verify New Asset Rights Cleared Gate REL-04 / CT-01
    var newAsset = await _assetMetadataService.GetAssetMetadataAsync(newAssetId);
    if (newAsset == null || !newAsset.RightsCleared)
    {
        throw new InvalidOperationException("RIGHTS_NOT_CLEARED_REL04: Tài sản thay thế mới phải được xác minh bản quyền RightsCleared = true.");
    }

    string oldAssetId = assetType == AssetType.AUDIO ? vocab.AudioAssetId : vocab.ImageAssetId;

    // 2. Create New VocabularyRevision (SemVer Versioning D-063)
    var latestRevision = vocab.Revisions.OrderByDescending(r => r.VersionNumber).FirstOrDefault();
    int newVersionNumber = (latestRevision?.VersionNumber ?? 0) + 1;

    var newRevision = new VocabularyRevision {
        VocabularyId = vocabularyId,
        VersionNumber = newVersionNumber,
        AudioAssetId = assetType == AssetType.AUDIO ? newAssetId : vocab.AudioAssetId,
        ImageAssetId = assetType == AssetType.IMAGE ? newAssetId : vocab.ImageAssetId,
        CreatedByUserId = actorUserId,
        CreatedAtUtc = DateTime.UtcNow
    };

    newRevision.RevisionDigest = CryptoUtils.ComputeRevisionDigest(newRevision);
    _db.VocabularyRevisions.Add(newRevision);

    // Update active pointer on Headword
    if (assetType == AssetType.AUDIO) vocab.AudioAssetId = newAssetId;
    if (assetType == AssetType.IMAGE) vocab.ImageAssetId = newAssetId;

    // 3. Recalculate QualityScore (Remove -25% penalty if audio provided)
    vocab.QualityScore = QualityScoreCalculator.Calculate(vocab);
    await _db.SaveChangesAsync();

    // 4. Update Active Reference Counts in M12-T025
    await _assetCleanupService.IncrementRefCountAsync(newAssetId);
    if (!string.IsNullOrEmpty(oldAssetId))
    {
        await _assetCleanupService.DecrementRefCountAsync(oldAssetId);
    }

    // 5. Evict Redis Cache SLA <= 60s (Session Pinning preserves active M03 sessions)
    await _redisDb.KeyDeleteAsync($"wordsoul:vocab:{vocabularyId}");

    // 6. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-02-REPLACE", actorUserId, new {
        VocabularyId = vocabularyId,
        AssetType = assetType.ToString(),
        OldAssetId = oldAssetId,
        NewAssetId = newAssetId,
        NewVersionNumber = newVersionNumber
    });

    return newRevision.ToDto();
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AR-G01` | Thay thế tài sản media BẮT BUỘC tạo `VocabularyRevision` mới (D-063) và tính lại `RevisionDigest`. |
| `AR-G02` | Tài sản thay thế mới BẮT BUỘC có cờ `RightsCleared == true` và `LicenseType` hợp lệ (REL-04 / CT-01). |
| `AR-G03` | Các phiên học M03 đang diễn ra được ghim phiên bản snapshot cũ (`Session Pinning`), không bị crash ứng dụng. |
| `AR-G04` | Thay thế tệp phát âm mới tự động tính lại `QualityScore`, xóa bỏ điểm phạt $-25\%$ khi bổ sung audio. |
| `AR-G05` | Thao tác thay thế tự động cập nhật giảm RefCount của `OldAssetId` và tăng RefCount của `NewAssetId` M12-T025. |
| `AR-G06` | 100% các vụ việc thay thế tài sản học liệu được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-02-REPLACE`). |
| `AR-G07` | Lệnh xóa Cache Redis bộ từ/mục từ hoàn tất trong SLA $\le 60$ giây sau khi thay thế tài sản thành công. |
| `AR-G08` | Phân quyền thay thế tài sản học liệu công khai chỉ dành cho `ContentAdmin` và `SuperAdmin`. |
| `AR-G09` | SLA thực thi API thay thế tài sản CSDL $< 30\text{ms}$. |
| `AR-G10` | 100% các test case tự kiểm AR14-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AR14-01` | Thay thế tệp MP3 phát âm mới cho Mục từ A với `RightsCleared = true` | Tạo Revision N+1, cập nhật con trỏ, RefCount old $-1$, new $+1$ |
| `AR14-02` | Thử thay thế tệp MP3 mới nhưng tài sản mới có `RightsCleared = false` | Reject 400 `RIGHTS_NOT_CLEARED_REL04` |
| `AR14-03` | Thay thế tệp phát âm cho mục từ đang bị phạt $-25\%$ do thiếu audio | Xóa phạt $-25\%$, QualityScore tăng từ $65\%$ lên $90\%$ |
| `AR14-04` | Người học ĐANG trong phiên học M03 tại thời điểm tài sản MP3 bị thay thế | Phiên học tiếp tục dùng MP3 cũ không bị crash (Session Pinning) |
| `AR14-05` | Người học mở bài học MỚI từ Mục từ A sau khi tài sản MP3 bị thay thế | Nhận tệp MP3 mới chính xác 200 OK |
| `AR14-06` | Tra cứu vết Audit Log M11 sau khi thay thế tài sản phát âm | Ghi nhận Audit Event `ACT-M11-02-REPLACE` đính kèm Old/NewAssetId |
| `AR14-07` | Thay thế hình ảnh WebP mới cho Mục từ B | Tạo Revision N+1, cập nhật `ImageAssetId` mới |
| `AR14-08` | Thử thay thế tài sản cho một `VocabularyId` không tồn tại | Reject 404 `VOCABULARY_NOT_FOUND` |
| `AR14-09` | Tải đồng thời 50 request thay thế tài sản từ 50 biên tập viên | Processing latency p95 $< 25\text{ms}$ |
| `AR14-10` | Kiểm tra thời gian dọn dẹp Cache Redis sau khi thay thế tài sản | Eviction SLA $< 300\text{ms}$ |
| `AR14-11` | Thử thay thế tệp âm thanh bằng một tệp video `.mp4` | Reject 400 `INVALID_ASSET_TYPE_FOR_AUDIO` |
| `AR14-12` | Gửi request thay thế tài sản khi JWT Access Token đã bị hết hạn | Deny 401 Unauthorized |
| `AR14-13` | User không phải ContentAdmin thử gọi API thay thế tài sản hệ thống | Deny 403 Forbidden |
| `AR14-14` | User chưa đăng nhập gọi API thay thế tài sản học liệu | Deny 401 Unauthorized |
| `AR14-15` | Thay thế tài sản trên bộ từ nháp (`DRAFT` state) | Cập nhật trực tiếp không cần sinh Revision mới |
| `AR14-16` | Kiểm tra độ trễ tính toán lại `RevisionDigest` cho phiên bản mới | Digest computation SLA $< 2\text{ms}$ |
| `AR14-17` | Phân tích tham chiếu các phiên bản `VocabularyRevisions` trong CSDL | Quét schema `M02_VocabularyRevisions` (T020) |
| `AR14-18` | Thao tác cập nhật RefCount M12-T025 bị gián đoạn do đứt kết nối | Retry tự động theo Outbox Pattern M12-T037 |
| `AR14-19` | Tra cứu lịch sử các lần thay thế tài sản của Mục từ A | Trả về DTO danh sách các Revision kèm AssetId qua từng thời kỳ |
| `AR14-20` | Kiểm thử hoàn tất luồng thiết kế vòng đời thay thế M02-ASSET-REPLACEMENT-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-AR-I01` | M02 hiện tại chưa có `VocabularyAssetReplacementService` xử lý thay thế | Thay thế media trực tiếp làm hỏng lịch sử học liệu M03 | M02-T049 (Source task) |
| `M02-AR-I02` | Thiếu cờ sinh `VocabularyRevision` mới khi thay thế tài sản | Vi phạm nguyên tắc SemVer phiên bản hóa mục từ D-063 | M02-T049; M02-T008-A |
| `M02-AR-I03` | Thiếu cờ kiểm duyệt `RightsCleared == true` cho tài sản thay thế mới | Risk đưa tài sản vi phạm bản quyền REL-04 vào hệ thống | M02-T049; REL-04 |
| `M02-AR-I04` | Thiếu luồng tự động tính lại `QualityScore` khi bổ sung audio | Mục từ đã bổ sung âm thanh vẫn bị giữ điểm phạt $-25\%$ | M02-T049; M02-T006 |
| `M02-AR-I05` | Chưa kết nối sự kiện thay thế tài sản với Audit Log M11 (`ACT-M11-02-REPLACE`) | Không ghi vết được lịch sử thay đổi media của mục từ | M02-T049; M11-T031 |

- `M02-AR-F01`: Triển khai `VocabularyAssetReplacementService` với SemVer Versioning (tiếp nhận: M02-T049).
- `M02-AR-F02`: Tích hợp Bắt buộc Rights Verification Gate REL-04 / CT-01 (tiếp nhận: M02-T049; REL-04).
- `M02-AR-F03`: Triển khai QualityScore Recalculation & RefCount Update M12-T025 (tiếp nhận: M02-T049; M12-T025).
- `M02-AR-F04`: Thiết lập bộ kiểm thử tự động AR-G01–G10 và AR14-01–20 (tiếp nhận: M02 tasks).
- `M02-AR-F05`: Thu thập bằng chứng runtime cho luồng thay thế M02 (tiếp nhận: M02 tasks; A-G03/A-G05).

## 8. Tự kiểm M02-T014

- Đã thiết kế hoàn chỉnh `M02-ASSET-REPLACEMENT-LIFECYCLE-1.0` với Ma trận Vòng đời Thay thế Tài sản.
- Đã chốt Ràng buộc Tạo Phiên bản Mục từ Mới khi Thay thế Media (`VocabularyRevision`).
- Đã chốt Ràng buộc Ghim Phiên bản Phiên Học M03 (`Session Revision Pinning`).
- Đã lồng ghép Tự động Tính lại Điểm Chất lượng QualityScore, Ràng buộc Kiểm duyệt Bản quyền REL-04 và Audit Log M11 (`ACT-M11-02-REPLACE`).
- Đã xác lập 10 Regression Gates (`AR-G01`–`AR-G10`) và 20 Test Cases tự kiểm (`AR14-01`–`AR14-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế vòng đời thay thế tài sản M02-T014 | WSA-7K2 |
