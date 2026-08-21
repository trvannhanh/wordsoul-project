# Thiết kế thay thế, xóa và orphan cleanup M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ASSET-REPLACEMENT-ORPHAN-CLEANUP-1.0` |
| Task | M12-T025 |
| Đầu vào | M12-ASSET-IMMUTABLE-METADATA-1.0 (D-126), M12-ASSET-ACCESS-DISTRIBUTION-1.0 (D-127), M11-REFERENCE-IMPACT-1.0 (D-052), REL-04, REL-07 |
| Phạm vi | Đặc tả Giao thức Thay thế, Xóa Mềm và Dọn dẹp Tài sản Mồ côi (`Asset Replacement, Soft-Deletion & Orphan Cleanup Protocol`), nguyên tắc cấm xóa vật lý tài sản đang được tham chiếu, luồng quét dọn tài sản rác `OrphanAssetCleanupWorker` và lưu vết kiểm toán |
| Tự kiểm | A-G03, A-G05; REL-04, REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu me đặc tả Giao thức Thay thế, Xóa Mềm và Dọn dẹp Tài sản Mồ côi (`Asset Replacement, Soft-Deletion & Orphan Cleanup Protocol`) thuộc M12, chuẩn hóa luồng thay thế tệp tin tài sản (âm thanh/hình ảnh), xử lý xóa mềm có lưu giữ lịch sử kiểm toán, và kích hoạt tiến trình tự động dọn dẹp các tài sản mồ côi (Orphan Assets) không còn liên kết để giải phóng dung lượng lưu trữ Cloud Storage (REL-04, REL-07).

- **Cấm Xóa Vật lý Tài sản Đang được Tham chiếu (`No Physical Delete on Active Reference Guard`)**: Yêu cầu xóa tài sản BẮT BUỘC trải qua bước Quét phân tích tác động tham chiếu đệ quy 5 tầng của M11 (M11-T020/D-052). Nếu tài sản còn số lượt tham chiếu active (`ActiveRefCount > 0`), thao tác Xóa Vật lý TUYỆT ĐỐI BỊ CẤM (`BLOCKING_HARD_DEPENDENCY`).
- **Máy Trạng thái Xóa Mềm 90 Ngày (`90-Day Soft-Deletion State Machine`)**: Tài sản yêu cầu xóa chuyển sang trạng thái `SOFT_DELETED`, cờ `IsDeleted = true`. Dữ liệu nhị phân trên Storage được chuyển sang Cold Glacier Storage và lưu giữ trong 90 ngày (`RetentionDays = 90`) để phục vụ khôi phục hoặc kiểm toán theo REL-07.
- **Quy trình Thay thế Tài sản Tạo Version Mới (`Asset Replacement Versioning`)**: Thao tác thay thế tệp tin tài sản (ví dụ: cập nhật lại âm thanh chuẩn cho mục từ) BẮT BUỘC tạo ra một `NewAssetId` GUID mới. Hệ thống cập nhật con trỏ tham chiếu ở M02 sang `NewAssetId` và giảm `ActiveRefCount` của `OldAssetId` đi 1.
- **Worker Tự động Dọn dẹp Tài sản Mồ côi (`OrphanAssetCleanupWorker`)**: Tiến trình chạy ngầm định kỳ hàng ngày (02:00 UTC) tự động tìm kiếm các tài sản có `ActiveRefCount == 0` kéo dài $> 30$ ngày (Tài sản mồ côi do thay thế hoặc Staging bỏ dở), tiến hành tiêu hủy tệp tin nhị phân trên Storage đệm và đánh dấu trạng thái `PURGED` (D-071).

## 2. Ma trận Quy trình Xóa và Dọn dẹp Tài sản (Cleanup Matrix)

| Hành động (`Action`) | Trạng thái Tham chiếu (`ActiveRefCount`) | Trạng thái Tài sản Mới (`NewState`) | Hành vi Tệp tin trên Storage | Thời hạn Lưu giữ (`Retention`) | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `REPLACE_ASSET` | N/A (Tạo Asset mới) | `ACTIVE` (Key Mới) | Tải tệp mới lên Prod Key | N/A (Tài sản mới) | `ACT-M11-25-REPLACE` |
| `SOFT_DELETE` | `ActiveRefCount == 0` | `SOFT_DELETED` | Chuyển sang Cold Glacier Storage | 90 ngày | `ACT-M11-25-SOFTDEL` |
| `HARD_DELETE_REJECT` | **`ActiveRefCount > 0`** | Giữ nguyên `ACTIVE` | **Giữ nguyên tệp tin (Refuse)** | N/A (Từ chối) | `ACT-M11-25-REJECT` |
| `ORPHAN_PURGE` | `ActiveRefCount == 0` (>30d) | `PURGED` | Tiêu hủy tệp tin trên Storage | 0 ngày (Xóa sạch) | `ACT-M11-25-PURGE` |

## 3. Kiến trúc Luồng Thay thế và Dọn dẹp Tài sản Mồ côi (Cleanup Engine)

```
[M02 / Client Requests Asset Deletion / Replacement]
                           |
                           v
        [Scan Active References Count (M11-T020)]
                           |
         +-----------------+-----------------+
         | (ActiveRefCount > 0)              | (ActiveRefCount == 0)
         v                                   v
[REJECT: BLOCKING_HARD_DEPENDENCY]    [Set State: SOFT_DELETED]
                                      - Move File to Glacier Cold Storage
                                      - Retention Window = 90 Days
                                      - Record Audit Log ACT-M11-25
                                                 |
                                                 v
                              [Daily OrphanAssetCleanupWorker (02:00 UTC)]
                              - Scan Assets with RefCount == 0 for > 30 Days
                              - Purge File from Storage SLA <= 5s
                              - Set State: PURGED
```

## 4. Giao thức Thực thi Dọn dẹp Tài sản CSDL (AssetCleanupService)

```csharp
public async Task<AssetCleanupResultDto> ProcessAssetDeletionOrPurgeAsync(
    string assetId, 
    DeletionType deletionType, 
    string actorUserId)
{
    var asset = await _db.AssetMetadatas.FirstOrDefaultAsync(a => a.AssetId == assetId);
    if (asset == null) throw new KeyNotFoundException("ASSET_NOT_FOUND");

    // 1. Reference Impact Scan M11-T020
    int activeRefCount = await _referenceImpactScanner.GetActiveReferenceCountAsync(assetId);

    if (deletionType == DeletionType.SOFT_DELETE)
    {
        if (activeRefCount > 0)
        {
            throw new InvalidOperationException($"BLOCKING_HARD_DEPENDENCY: Tài sản {assetId} đang được tham chiếu bởi {activeRefCount} đối tượng học liệu. Không thể xóa.");
        }

        // 2. Soft-Delete Branch: Move to Glacier & Retention 90d
        asset.IsDeleted = true;
        asset.DeletedAtUtc = DateTime.UtcNow;
        asset.RetentionExpiresAtUtc = DateTime.UtcNow.AddDays(90);
        asset.State = AssetState.SOFT_DELETED;

        await _db.SaveChangesAsync();

        // Evict CDN Cache SLA <= 60s
        await _cdnDistributionService.InvalidateCdnCacheAsync(asset.ObjectKey, actorUserId);

        // Move to Cold Storage
        await _storageClient.MoveToColdGlacierStorageAsync(asset.ObjectKey);

        // 3. Record Audit Event M11
        await _auditLog.RecordEventAsync("ACT-M11-25-SOFTDEL", actorUserId, new {
            AssetId = assetId,
            ObjectKey = asset.ObjectKey,
            ActiveRefCount = activeRefCount
        });

        return new AssetCleanupResultDto { AssetId = assetId, Status = "SOFT_DELETED" };
    }

    throw new ArgumentException("INVALID_DELETION_TYPE");
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `OC-G01` | Tuyệt đối CẤM xóa vật lý tài sản đang được bất kỳ bộ từ/mục từ nào tham chiếu (`ActiveRefCount > 0`). |
| `OC-G02` | Thao tác xóa mềm chuyển tài sản sang `SOFT_DELETED` và lưu giữ tệp tin trong 90 ngày (`RetentionDays = 90`). |
| `OC-G03` | Thay thế tài sản BẮT BUỘC sinh ra `NewAssetId` GUID mới, cập nhật con trỏ M02 và giảm RefCount tệp cũ. |
| `OC-G04` | Worker `OrphanAssetCleanupWorker` tự động chạy hàng ngày tiêu hủy tệp tin mồ côi không tham chiếu $> 30$ ngày. |
| `OC-G05` | Xóa mềm tài sản tự động phát lệnh dọn dẹp Cache CDN toàn cầu SLA $\le 60$ giây (D-051, D-127). |
| `OC-G06` | 100% các thao tác thay thế, xóa mềm hoặc tiêu hủy tài sản được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-25`). |
| `OC-G07` | Khôi phục tài sản bị xóa mềm (`RestoreSoftDeletedAsset`) chỉ thực hiện được trong vòng 90 ngày cửa sổ lưu giữ. |
| `OC-G08` | Phân quyền ban hành lệnh xóa mềm và dọn dẹp mồ côi chỉ dành cho `ContentAdmin`, `SecurityAdmin` và Auto Worker. |
| `OC-G09` | SLA thực thi API xóa mềm CSDL $< 20\text{ms}$; SLA lệnh di chuyển tệp sang Glacier $< 3\text{s}$. |
| `OC-G10` | 100% các test case tự kiểm OC25-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC25-01` | Xóa mềm tài sản MP3 phát âm không còn bài học nào tham chiếu (`ActiveRefCount == 0`) | Chuyển `SOFT_DELETED`, hẹn 90d, xóa cache CDN SLA $< 40\text{s}$ |
| `RC25-02` | Thử xóa mềm tài sản MP3 đang được 3 bộ từ công khai tham chiếu (`ActiveRefCount == 3`) | Reject 400 `BLOCKING_HARD_DEPENDENCY` |
| `RC25-03` | Thực hiện thay thế tệp âm thanh phát âm mới cho Mục từ A | Tạo `NewAssetId` mới, gán vào Mục từ A, RefCount tệp cũ giảm 1 |
| `RC25-04` | Worker `OrphanAssetCleanupWorker` quét thấy tệp đệm Staging mồ côi 35 ngày | Tiêu hủy tệp tin trên Storage đệm, chuyển trạng thái `PURGED` |
| `RC25-05` | Khôi phục tài sản bị xóa mềm ở ngày thứ 45 kể từ ngày xóa ($< 90$d) | Khôi phục tài sản về `ACTIVE`, chuyển tệp về Production Storage |
| `RC25-06` | Thử khôi phục tài sản bị xóa mềm ở ngày thứ 95 kể từ ngày xóa ($> 90$d) | Reject 400 `RETENTION_PERIOD_EXPIRED` |
| `RC25-07` | Tra cứu vết Audit Log M11 sau khi xóa mềm tài sản | Ghi nhận Audit Event `ACT-M11-25-SOFTDEL` đính kèm ObjectKey |
| `RC25-08` | Tải đồng thời 50 request xóa mềm từ 50 tác giả | Processing latency p95 $< 18\text{ms}$ |
| `RC25-09` | Dịch vụ Cloud Glacier Storage bị gián đoạn khi di chuyển tệp | Đổi cờ CSDL trước, retry chuyển tệp qua Outbox M12-T037 |
| `RC25-10` | Quét đồ thị tham chiếu 5 tầng M11-T020 phát hiện tham chiếu ẩn ở Module M05 | Chặn thao tác xóa mềm `BLOCKING_HARD_DEPENDENCY` |
| `RC25-11` | Thử xóa mềm tài sản khi cờ `IsDeleted` đã là `true` | Reject 400 `ASSET_ALREADY_DELETED` |
| `RC25-12` | Gửi request xóa mềm khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `RC25-13` | User không phải Admin thử gọi API tiêu hủy tệp tin mồ côi | Deny 403 Forbidden |
| `RC25-14` | User chưa đăng nhập gọi API xóa mềm tài sản | Deny 401 Unauthorized |
| `RC25-15` | Tải lên tệp thay thế nhưng tệp mới bị Antivirus quét thấy virus | Hủy tệp mới, giữ nguyên tệp tài sản cũ không bị thay thế |
| `RC25-16` | Kiểm tra độ trễ xóa sạch tệp tin mồ côi phía Cloud Storage | Purge SLA $< 3.5\text{s}$ |
| `RC25-17` | Phân tích tham chiếu các tài sản `SOFT_DELETED` trong CSDL | Quét schema `M12_SoftDeletedAssets` (T020) |
| `RC25-18` | Tiến trình Worker tự động dọn dẹp gặp sự cố sập đứt giữa chừng | Lock phân tán M12-T033 tự nhả, worker khác tiếp quản |
| `RC25-19` | Tra cứu danh sách các tài sản mồ côi đang chờ tiêu hủy | Trả về DTO danh sách AssetId có RefCount == 0 |
| `RC25-20` | Kiểm thử hoàn tất luồng thay thế và dọn dẹp M12-ASSET-REPLACEMENT-ORPHAN-CLEANUP-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-OC-I01` | M12 hiện tại chưa có `AssetCleanupService` quản lý xóa mềm | Risk xóa nhầm làm hỏng liên kết học liệu M02 | M12-T047-A (Source task) |
| `M12-OC-I02` | Thiếu cờ Quét tham chiếu 5 tầng chặn xóa tài sản active | Người học có thể bị văng sập ứng dụng M03 do thiếu tệp tin | M12-T047-A; M11-T020 |
| `M12-OC-I03` | Thiếu `OrphanAssetCleanupWorker` tự động dọn dẹp tệp rác | Storage bị phình to dữ liệu mồ côi từ các lần upload dở | M12-T047-A; M11-T038 |
| `M12-OC-I04` | Thiếu luồng chuyển tệp tin xóa mềm sang Glacier Cold Storage 90d | Chi phí lưu trữ CSDL phình cao do giữ tệp tin cũ ở Prod | M12-OC-F04; M12-T040 |
| `M12-OC-I05` | Chưa kết nối sự kiện xóa/dọn dẹp với Audit Log M11 (`ACT-M11-25`) | Không ghi vết được người xóa và danh sách tệp bị tiêu hủy | M12-T047-A; M11-T031 |

- `M12-OC-F01`: Triển khai `AssetCleanupService` với No Physical Delete Active Reference Guard (tiếp nhận: M12-T047-A).
- `M12-OC-F02`: Tích hợp Bắt buộc 90-Day Soft-Deletion State Machine & Cold Storage (tiếp nhận: M12-T047-A; REL-07).
- `M12-OC-F03`: Triển khai `OrphanAssetCleanupWorker` chạy ngầm 02:00 UTC (tiếp nhận: M12-T047-A; M11-T038).
- `M12-OC-F04`: Thiết lập bộ kiểm thử tự động OC-G01–G10 và OC25-01–20 (tiếp nhận: M12 tasks).
- `M12-OC-F05`: Thu thập bằng chứng runtime cho luồng dọn dẹp M12 (tiếp nhận: M12 tasks; A-G03/A-G05).

## 8. Tự kiểm M12-T025

- Đã thiết kế hoàn chỉnh `M12-ASSET-REPLACEMENT-ORPHAN-CLEANUP-1.0` với Ma trận Quy trình Xóa và Dọn dẹp Tài sản.
- Đã chốt Ràng buộc Cấm Xóa Vật lý Tài sản Đang được Tham chiếu (`No Physical Delete Active Reference Guard`).
- Đã chốt Máy Trạng thái Xóa Mềm 90 Ngày và di chuyển sang Cold Glacier Storage.
- Đã lồng ghép Quy trình Thay thế Tài sản Tạo Version Mới, Worker Tự động Dọn dẹp `OrphanAssetCleanupWorker` và Audit Log M11 (`ACT-M11-25`).
- Đã xác lập 10 Regression Gates (`OC-G01`–`OC-G10`) và 20 Test Cases tự kiểm (`OC25-01`–`OC25-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế thay thế, xóa và orphan cleanup M12-T025 | WSA-7K2 |
