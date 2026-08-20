# Thiết kế xuất bản theo phiên bản M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-VERSIONED-PUBLISHING-1.0` |
| Task | M02-T032 |
| Đầu vào | M02-HEADWORD-VERSIONING-A-1.0 (D-063), M02-SET-LIFECYCLE-1.0 (D-077), M02-SET-SUBMISSION-FLOW-1.0 (D-083), M02-PUBLIC-MODERATION-CHECKLIST-1.0 (D-084) |
| Phạm vi | Mô hình quản lý phiên bản xuất bản bất biến của Bộ từ vựng, quy tắc Semantic Versioning, giao thức Rollback phiên bản an toàn và đồng bộ cache Redis |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức Xuất bản Bộ từ vựng theo Phiên bản Bất biến (`Immutable Versioned Set Publishing Engine`) thuộc M02, cho phép lưu trữ và quản lý lịch sử các bản phát hành của Bộ từ vựng công khai, hỗ trợ truy xuất phiên bản cũ và rollback khẩn cấp khi phát hiện sự cố.

- **Bất biến Snapshots Phiên bản (`Immutable Set Version Snapshot Invariant`)**: Mỗi lần xuất bản thành công (`ACT_PUBLISH_SET`) tạo một bản ghi `VocabularySetVersion` mới chứa toàn bộ ảnh chụp JSON (`SetSnapshotJson`) và mã băm `RevisionDigest` (SHA-256). Bản ghi phiên bản đã tạo tuyệt đối BẤT BIẾN (CẤM UPDATE hoặc DELETE).
- **Quy tắc Đánh số Phiên bản Semantic Versioning (`SemVer Rule`)**:
  - *MAJOR (`vX.0.0`)*: Thay đổi lớn làm tái cấu trúc $> 50\%$ danh sách từ vựng trong bộ từ.
  - *MINOR (`v1.Y.0`)*: Bổ sung từ mới, bớt từ hoặc thay đổi thứ tự `DisplayOrder`.
  - *PATCH (`v1.0.Z`)*: Chỉnh sửa nội dung ghi đè ngữ nghĩa/ví dụ hoặc thay đổi metadata mô tả.
- **Ràng buộc Ghim Phiên bản Bài học M03 (`Version Pinning Invariant`)**: Phiên học M03 khi khởi tạo ghim chính xác `SetVersionId` và `RevisionDigest`. Việc phát hành phiên bản mới $v1.1.0$ không làm ảnh hưởng đến các phiên học M03 đang chạy trên $v1.0.0$.
- **Giao thức Rollback Phiên bản Khẩn cấp ($\le 30\text{s}$ Rollback SLA)**: Khi phiên bản mới gặp sự cố nặng, `ContentAdmin` phát lệnh Rollback về phiên bản cũ $v1.0.0$. Hệ thống tự động chuyển `ActiveVersionId` về $v1.0.0$ và xóa cache Redis `lesson_payload:{setId}` trong vòng $\le 30$ giây.

## 2. Mô hình Thực thể Phiên bản Bộ từ (VocabularySetVersion Schema)

```json
{
  "setVersionId": 5012,
  "vocabularySetId": 108,
  "versionNumber": "v1.1.0",
  "versionType": "MINOR",
  "revisionDigest": "a4f8e912b30491823908420e1839281a85e1927a",
  "publishedAtUtc": "2026-08-20T10:45:00Z",
  "publishedByActorId": "USR-ADM-007",
  "submissionTicketId": "TCK-SUB-2026-0820-0012",
  "itemCount": 16,
  "setDifficultyIndex": 5.35,
  "isCurrentActive": true,
  "changelogNotes": "Bổ sung thêm từ vựng 'accountant' và cập nhật câu ví dụ minh họa.",
  "setSnapshotJson": "{ \"vocabularySetId\": 108, \"title\": \"...\", \"items\": [...] }"
}
```

## 3. Kiến trúc Luồng Xuất bản Phiên bản Mới (Versioned Publishing Engine)

```
[ContentAdmin Approves & Publishes Set (M02-T030)]
                        |
                        v
     [Calculate New SemVer (Major/Minor/Patch)]
                        |
                        v
     [Generate SetSnapshotJson & RevisionDigest]
                        |
                        v
     [Insert Immutable VocabularySetVersion Record]
                        |
                        v
     [Update VocabularySet.ActiveVersionId]
                        |
                        v
     [Pre-generate & Update Redis Payload Cache]
                        |
                        v
     [Publish SetVersionPublishedEvent]
```

## 4. Giao thức Khôi phục Phiên bản Cũ (Rollback Protocol)

```csharp
public async Task<VocabularySetVersionDto> RollbackSetVersionAsync(int setId, int targetSetVersionId, string rollbackReason, string moderatorActorId)
{
    var set = await _db.VocabularySets.FirstOrDefaultAsync(s => s.VocabularySetId == setId);
    var targetVersion = await _db.VocabularySetVersions
        .FirstOrDefaultAsync(v => v.VocabularySetId == setId && v.SetVersionId == targetSetVersionId);

    if (set == null || targetVersion == null) throw new InvalidOperationException("TARGET_VERSION_NOT_FOUND");

    // 1. Mở Transaction CSDL
    set.ActiveVersionId = targetVersion.SetVersionId;
    set.ItemCount = targetVersion.ItemCount;
    set.SetDifficultyIndex = targetVersion.SetDifficultyIndex;

    // Unset IsCurrentActive trên tất cả các version khác, set true cho targetVersion
    await _db.VocabularySetVersions
        .Where(v => v.VocabularySetId == setId)
        .ExecuteUpdateAsync(s => s.SetProperty(v => v.IsCurrentActive, false));
    
    targetVersion.IsCurrentActive = true;

    // 2. Xóa lập tức Redis Cache Payload M03
    await _cacheService.RemoveAsync($"lesson_payload:{setId}");

    // 3. Ghi vết Audit Log M11
    await _auditLogService.RecordEventAsync("ACT-M11-04", moderatorActorId, new {
        Action = "SET_VERSION_ROLLBACK",
        SetId = setId,
        RollbackToVersion = targetVersion.VersionNumber,
        Reason = rollbackReason
    });

    await _db.SaveChangesAsync();
    return MapToDto(targetVersion);
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `VP-G01` | Mỗi lần xuất bản thành công tạo 1 bản ghi `VocabularySetVersion` bất biến trong CSDL. |
| `VP-G02` | Bản ghi `VocabularySetVersion` đã tạo tuyệt đối CẤM chỉnh sửa hoặc xóa vĩnh viễn. |
| `VP-G03` | Đánh số phiên bản tuân thủ chính xác quy tắc SemVer (`vX.Y.Z`). |
| `VP-G04` | Phiên học M03 đang chạy ghim chính xác `RevisionDigest` của phiên bản tại thời điểm bắt đầu. |
| `VP-G05` | Thao tác Rollback khôi phục trạng thái `ActiveVersionId` cũ trong thời gian $\le 30$ giây. |
| `VP-G06` | Thao tác Rollback xóa sạch Redis Cache `lesson_payload:{setId}` lập tức. |
| `VP-G07` | Phân quyền xuất bản và Rollback phiên bản chỉ dành riêng cho `ContentAdmin` và `SuperAdmin`. |
| `VP-G08` | 100% thao tác xuất bản và Rollback ghi vết Audit Event M11 (`ACT-M11-04`). |
| `VP-G09` | SLA nạp danh sách lịch sử phiên bản bộ từ $< 30\text{ms}$. |
| `VP-G10` | 100% các test case tự kiểm VP32-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `MC32-01` | Xuất bản lần đầu cho Bộ từ vựng 108 | Tạo bản ghi phiên bản `v1.0.0` với `isCurrentActive = true` |
| `MC32-02` | Bổ sung 2 từ mới vào Bộ từ 108 và xuất bản lại | Tạo phiên bản mới `v1.1.0` (MINOR), cập nhật `ActiveVersionId` |
| `MC32-03` | Sửa câu ví dụ ghi đè của 1 từ trong Bộ từ 108 | Tạo phiên bản mới `v1.1.1` (PATCH) |
| `MC32-04` | Thử gửi request UPDATE sửa dữ liệu trong `VocabularySetVersion` cũ | System reject field readonly, bảo vệ snapshots bất biến |
| `MC32-05` | `ContentAdmin` thực hiện Rollback Bộ từ 108 từ `v1.1.1` về `v1.0.0` | Cập nhật `ActiveVersionId` về `v1.0.0`, xóa Redis cache trong $\le 5\text{s}$ |
| `MC32-06` | Người học A khởi tạo phiên M03 ngay sau khi Rollback về `v1.0.0` | Phiên học của A nạp payload từ vựng của phiên bản `v1.0.0` |
| `MC32-07` | Tra cứu lịch sử phiên bản của Bộ từ 108 | Trả về danh sách `[v1.1.1, v1.1.0, v1.0.0]` với thông tin chi tiết |
| `MC32-08` | Người học B đang học phiên M03 ghim ở `v1.1.1` trong lúc Admin Rollback | Phiên học của B tiếp tục làm bài bình thường không bị đứt đoạn |
| `MC32-09` | Tải snapshot JSON của phiên bản `v1.0.0` | Trả về chuỗi `SetSnapshotJson` nguyên bản tại thời điểm xuất bản |
| `MC32-10` | Kiểm tra cache Redis `lesson_payload:108` sau khi xuất bản phiên bản mới | Cache cũ bị xóa, payload phiên bản mới được nạp |
| `MC32-11` | Thử Rollback về 1 `SetVersionId` không thuộc bộ từ | System reject với lỗi `TARGET_VERSION_NOT_FOUND` |
| `MC32-12` | Tải đồng thời 50 request truy vấn danh sách phiên bản bộ từ | Response latency p95 $< 25\text{ms}$ |
| `MC32-13` | User vai trò `Learner` thử gọi API Rollback phiên bản bộ từ | Deny 403 Forbidden (M01-ROLE-MATRIX-1.0) |
| `MC32-14` | User chưa đăng nhập thử gọi API xem lịch sử phiên bản | Deny 401 Unauthorized |
| `MC32-15` | Xem vết Audit Log M11 sau khi Rollback phiên bản | Ghi nhận Audit Event `ACT-M11-04` với mã phiên bản target |
| `MC32-16` | Xuất bản phiên bản mới cho Bộ từ có 50 mục từ tối đa | Snapshot JSON lưu trữ đầy đủ 50 mục từ trong $< 50\text{ms}$ |
| `MC32-17` | Phân tích tham chiếu trước khi Rollback phiên bản bộ từ | Quét các active session M03 đang mở bài học (T020) |
| `MC32-18` | Thao tác Rollback bị gián đoạn giữa chừng do lỗi DB | Rollback transaction, `ActiveVersionId` giữ nguyên version hiện tại |
| `MC32-19` | So sánh diff giữa 2 phiên bản `v1.0.0` và `v1.1.0` | Trả về danh sách từ vựng được bổ sung/thay đổi |
| `MC32-20` | Kiểm thử hoàn tất luồng xuất bản theo phiên bản M02-SET-VERSIONED-PUBLISHING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-VP-I01` | Entity `VocabularySet.cs` chưa có liên kết đến lịch sử `VocabularySetVersions` | Bộ từ vựng không lưu lại được lịch sử các lần xuất bản trước đó | M02-T049 (Source task) |
| `M02-VP-I02` | Chưa có tính năng Rollback phiên bản khẩn cấp trong `VocabularySetsController.cs` | Khi phiên bản mới có lỗi, Admin không thể quay lại phiên bản cũ | M02-T049 |
| `M02-VP-I03` | Thiếu quy tắc SemVer tự động tính toán số phiên bản (`vX.Y.Z`) | Việc đánh số phiên bản bị làm thủ công gây mất nhất quán | M02-T049 |
| `M02-VP-I04` | Chưa lưu `SetSnapshotJson` đầy đủ tại thời điểm xuất bản | Khi mục từ bị sửa/xóa trong từ điển gốc, phiên bản cũ bị đứt link | M02-T049 |
| `M02-VP-I05` | Chưa xóa Redis Cache `lesson_payload` lập tức khi Rollback | M03 vẫn nạp payload của phiên bản lỗi từ Redis cache | M02-T049 |

- `M02-VP-F01`: Tạo entity `VocabularySetVersion.cs` và CSDL Migration (tiếp nhận: M02-T049).
- `M02-VP-F02`: Triển khai `VersionedPublishingService` hỗ trợ Rollback SLA $\le 30\text{s}$ (tiếp nhận: M02-T049).
- `M02-VP-F03`: Tích hợp bộ tính toán SemVer tự động và ghim `RevisionDigest` (tiếp nhận: M02-T049).
- `M02-VP-F04`: Thiết lập bộ kiểm thử tự động VP-G01–G10 và VP32-01–20 (tiếp nhận: M02 tasks).
- `M02-VP-F05`: Thu thập bằng chứng runtime cho luồng xuất bản theo phiên bản M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T032

- Đã thiết kế hoàn chỉnh `M02-SET-VERSIONED-PUBLISHING-1.0` với Mô hình Snapshots Phiên bản Bất biến.
- Đã chốt Ràng buộc Quy tắc Đánh số Phiên bản SemVer (`vX.Y.Z`).
- Đã quy định Giao thức Rollback Phiên bản Khẩn cấp SLA $\le 30\text{s}$ và xóa cache Redis.
- Đã lồng ghép bảo vệ Ghim Phiên bản Bài học M03 (`Version Pinning Invariant`) và Lưu vết Audit M11 (`ACT-M11-04`).
- Đã xác lập 10 Regression Gates (`VP-G01`–`VP-G10`) và 20 Test Cases tự kiểm (`VP32-01`–`VP32-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế xuất bản theo phiên bản M02-T032 | WSA-7K2 |
