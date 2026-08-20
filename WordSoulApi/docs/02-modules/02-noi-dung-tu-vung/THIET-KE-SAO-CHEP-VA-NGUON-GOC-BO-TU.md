# Thiết kế sao chép và nguồn gốc bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-CLONE-LINEAGE-1.0` |
| Task | M02-T019 |
| Đầu vào | M02-SET-PERMISSIONS-1.0 (D-076), M02-SET-LIFECYCLE-1.0 (D-077), REL-04 |
| Phạm vi | Giao thức sao chép Bộ từ vựng, quản lý cây nguồn gốc (`Lineage Tree`), bảo toàn thông tin tác giả gốc và lan truyền thông báo cập nhật/thu hồi |
| Tự kiểm | A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Sao chép và Quản lý Nguồn gốc Bộ từ vựng (`Set Cloning & Lineage Origin Engine`) thuộc M02, cho phép người học sao chép các bộ từ công khai về bộ từ cá nhân của mình mà vẫn bảo toàn thông tin tác quyền bản quyền REL-04.

- **Bảo toàn Cây Nguồn gốc Bất biến (`Lineage Tree Invariant`)**: Mọi bộ từ được sao chép bắt buộc lưu vết `ParentSetId` (ID bộ từ trực tiếp bị sao chép) và `OriginalSetId` (ID bộ từ gốc ban đầu). CẤM làm đứt đoạn cây nguồn gốc nhằm đảm bảo minh bạch tác quyền (REL-04).
- **Ràng buộc Quyền Tác giả Gốc (`Author Attribution Preservation`)**: DTO hiển thị của bộ từ được sao chép bắt buộc giữ thuộc tính `OriginalCreatorId` và `OriginalCreatorName` để ghi nhận công sức của tác giả tạo ra bộ từ ban đầu.
- **Tự động Tăng Chỉ số Sao chép (`Clone Counter Auto-Increment`)**: Mỗi lượt sao chép thành công tự động tăng chỉ số `CloneCount` của bộ từ gốc lên $+1$, dùng để xếp hạng độ phổ biến của bộ từ (M09).
- **Lan truyền Thông báo Cập nhật và Thu hồi (`Update & Recall Propagation`)**: Khi bộ từ gốc có sự thay đổi từ vựng hoặc bị thu hồi bản quyền, hệ thống phát sự kiện `SetLineageUpdatedEvent` thông báo cho các bộ từ con được sao chép.

## 2. Mô hình Thực thể Nguồn gốc Bộ từ (Lineage Attributes Schema)

```json
{
  "vocabularySetId": 205,
  "title": "Từ vựng Giao tiếp Du lịch (Bản cá nhân của Nhanh)",
  "isCustom": true,
  "creatorId": "USR-10024",
  "parentSetId": 108,
  "originalSetId": 108,
  "cloneDepth": 1,
  "originalCreatorId": "USR-ADMIN-007",
  "originalCreatorName": "Ban Biên Tập WordSoul",
  "hasPendingOriginUpdates": true,
  "hasCopyrightNotice": false,
  "createdAtUtc": "2026-08-20T10:00:00Z"
}
```

## 3. Kiến trúc Cây Nguồn gốc Bộ từ (Lineage Tree Architecture)

```
       [System Original Set: SetId 108] (OriginalCreator: Admin 007)
                      |
        +-------------+-------------+
        | (Clone 1)                 | (Clone 2)
        v                           v
  [User A Custom Set: Set 205]  [User B Custom Set: Set 309]
  - ParentSetId = 108           - ParentSetId = 108
  - OriginalSetId = 108         - OriginalSetId = 108
  - CloneDepth = 1              - CloneDepth = 1
        |
        v (Clone 3 from User A)
  [User C Custom Set: Set 412]
  - ParentSetId = 205
  - OriginalSetId = 108
  - CloneDepth = 2
```

## 4. Giao thức Thực thi Sao chép Bộ từ (Set Cloning Protocol)

```csharp
public async Task<VocabularySetDto> CloneVocabularySetAsync(int sourceSetId, string currentUserId)
{
    var sourceSet = await _db.VocabularySets
        .Include(s => s.SetVocabularies)
        .FirstOrDefaultAsync(s => s.VocabularySetId == sourceSetId);

    if (sourceSet == null || (!sourceSet.IsPublished && sourceSet.CreatorId != currentUserId))
    {
        throw new InvalidOperationException("CANNOT_CLONE_UNAVAILABLE_SET");
    }

    // 1. Khởi tạo Bộ từ Cá nhân mới
    var clonedSet = new VocabularySet
    {
        Title = $"{sourceSet.Title} (Bản sao)",
        Description = sourceSet.Description,
        CategoryTag = sourceSet.CategoryTag,
        CefrLevel = sourceSet.CefrLevel,
        SetDifficultyIndex = sourceSet.SetDifficultyIndex,
        IsCustom = true,
        CreatorId = currentUserId,
        IsPublished = false,
        ParentSetId = sourceSet.VocabularySetId,
        OriginalSetId = sourceSet.OriginalSetId ?? sourceSet.VocabularySetId,
        CloneDepth = (sourceSet.CloneDepth ?? 0) + 1,
        OriginalCreatorId = sourceSet.OriginalCreatorId ?? sourceSet.CreatorId,
        ItemCount = sourceSet.ItemCount
    };

    _db.VocabularySets.Add(clonedSet);
    await _db.SaveChangesAsync();

    // 2. Sao chép danh sách mục từ thành phần
    foreach (var item in sourceSet.SetVocabularies)
    {
        _db.SetVocabularies.Add(new SetVocabulary
        {
            VocabularySetId = clonedSet.VocabularySetId,
            VocabularyId = item.VocabularyId,
            SelectedSenseIds = item.SelectedSenseIds
        });
    }

    // 3. Tăng CloneCount của bộ từ nguồn
    sourceSet.CloneCount = (sourceSet.CloneCount ?? 0) + 1;

    await _db.SaveChangesAsync();
    return MapToDto(clonedSet);
}
```

## 5. Giao thức Lan truyền Cảnh báo Thu hồi Bản quyền (Copyright Recall Propagation)

Khi bộ từ gốc `SetId_108` bị phát hiện vi phạm bản quyền và bị Thu hồi Khẩn cấp (`ACT_RECALL_SET` M02-T017):

1. Hệ thống phát sự kiện `SetCopyrightRevokedEvent(OriginalSetId = 108)`.
2. Subscriber trong M02 tìm tất cả bộ từ con có `OriginalSetId == 108`.
3. Đặt `hasCopyrightNotice = true` trên các bộ từ con.
4. Hiển thị thông báo trên ứng dụng của người học: *"Bộ từ gốc của bản sao này đã bị thu hồi do vi phạm bản quyền. Một số từ vựng có thể không còn phát được âm thanh."*

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CL-G01` | 100% Bộ từ được sao chép lưu vết chính xác `ParentSetId`, `OriginalSetId` và `CloneDepth`. |
| `CL-G02` | Bộ từ được sao chép tự động gắn `IsCustom = true` và `CreatorId` của người thực hiện sao chép. |
| `CL-G03` | DTO công khai của bộ từ sao chép giữ nguyên thông tin tác giả gốc `OriginalCreatorId`. |
| `CL-G04` | Sao chép bộ từ thành công tự động tăng `CloneCount` của bộ từ nguồn lên $+1$. |
| `CL-G05` | Cấm sao chép các bộ từ chưa được xuất bản (`IsPublished == false`) ngoại trừ chính tác giả. |
| `CL-G06` | Khi bộ từ gốc bị thu hồi bản quyền, 100% bộ từ con tự động gắn `hasCopyrightNotice = true`. |
| `CL-G07` | Khi bộ từ gốc cập nhật nội dung, bộ từ con tự động nhận cờ `hasPendingOriginUpdates = true`. |
| `CL-G08` | Phân quyền thực hiện sao chép tuân thủ ma trận vai trò M02-T016 (`Learner` & `ContentCreator`). |
| `CL-G09` | SLA thực thi thao tác sao chép bộ từ (bao gồm nạp 50 từ) $< 100\text{ms}$. |
| `CL-G10` | 100% các test case tự kiểm CL19-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CL19-01` | Người học A sao chép Bộ từ Hệ thống 108 công khai | Tạo bộ từ mới `SetId_205`, `ParentSetId = 108`, `CloneDepth = 1` |
| `CL19-02` | Người học B sao chép Bộ từ 205 của Người học A | Tạo bộ từ mới `SetId_309`, `ParentSetId = 205`, `OriginalSetId = 108`, `CloneDepth = 2` |
| `CL19-03` | Kiểm tra thông tin tác giả gốc trên Bộ từ 309 | `OriginalCreatorId` trỏ về `Admin007` (tác giả gốc 108) |
| `CL19-04` | Kiểm tra `CloneCount` của Bộ từ 108 sau 2 lượt sao chép | `CloneCount` tăng $+2$ so với ban đầu |
| `CL19-05` | Thử sao chép một Bộ từ cá nhân chưa xuất bản của người khác | System reject với lỗi `CANNOT_CLONE_UNAVAILABLE_SET` |
| `CL19-06` | Tác giả sao chép chính Bộ từ cá nhân `Draft` của mình | Cho phép sao chép nhân bản bộ từ draft thành công |
| `CL19-07` | Bộ từ gốc 108 bị thu hồi khẩn cấp do vi phạm bản quyền | Bộ từ 205 và 309 tự động nhận `hasCopyrightNotice = true` |
| `CL19-08` | Bộ từ gốc 108 bổ sung thêm 2 mục từ mới | Bộ từ con nhận cờ `hasPendingOriginUpdates = true` |
| `CL19-09` | Người học A bấm "Đồng bộ cập nhật từ bộ từ gốc" | Bổ sung 2 từ mới vào Bộ từ 205, xóa cờ notification |
| `CL19-10` | Xóa Bộ từ con 205 | Không làm ảnh hưởng đến Bộ từ gốc 108 hay Bộ từ cháu 309 |
| `CL19-11` | Sao chép Bộ từ chứa 50 mục từ tối đa | Sao chép thành công toàn bộ 50 mục từ trong $< 80\text{ms}$ |
| `CL19-12` | Tải đồng thời 50 request sao chép các bộ từ khác nhau | Response latency p95 $< 90\text{ms}$ |
| `CL19-13` | User chưa đăng nhập thử gọi API sao chép bộ từ | Deny 401 Unauthorized |
| `CL19-14` | Kiểm tra tên hiển thị mặc định của bộ từ sao chép | Tên có dạng `"{OriginalTitle} (Bản sao)"` |
| `CL19-15` | Xem vết Audit Log M11 sau khi sao chép bộ từ | Ghi nhận Audit Event `ACT-M11-04` với nguồn gốc rõ ràng |
| `CL19-16` | Thử sửa `OriginalSetId` của bộ từ con thủ công qua API | System reject field readonly, bảo vệ cây nguồn gốc |
| `CL19-17` | Phân tích tham chiếu trước khi thu hồi 1 bộ từ gốc | Quét đồ thị các bộ từ con trong cây nguồn gốc (T020) |
| `CL19-18` | Khôi phục bộ từ gốc sau khi giải quyết xong khiếu nại bản quyền | Xóa cờ `hasCopyrightNotice` trên các bộ từ con |
| `CL19-19` | Tra cứu danh sách các bản sao của 1 bộ từ vựng | Trả về danh sách bộ từ con trực tiếp và gián tiếp |
| `CL19-20` | Kiểm thử hoàn tất luồng sao chép và nguồn gốc bộ từ M02-SET-CLONE-LINEAGE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-CL-I01` | Entity `VocabularySet.cs` chưa có các trường `ParentSetId`, `OriginalSetId`, `CloneDepth` | Chưa lưu vết được cây nguồn gốc sao chép bộ từ | M02-T049 (Source task) |
| `M02-CL-I02` | API Clone Set hiện tại chưa tự động tăng `CloneCount` | Không đo lường được độ phổ biến của bộ từ để xếp hạng M09 | M02-CL-F02 |
| `M02-CL-I03` | Thiếu luồng phát sự kiện lan truyền cảnh báo khi bộ từ gốc bị thu hồi bản quyền | Bộ từ con không biết bộ từ gốc đã bị vi phạm bản quyền REL-04 | M02-T049; REL-04 |
| `M02-CL-I04` | Chưa giữ lại thông tin `OriginalCreatorId` khi sao chép đệ quy ($Depth \ge 2$) | Bị mất thông tin tác giả gốc ban đầu | M02-T049 |
| `M02-CL-I05` | Chưa có API "Đồng bộ cập nhật từ bộ từ gốc" cho người học | Người học phải tự thêm thủ công các từ mới từ bộ gốc | M02-T049 |

- `M02-CL-F01`: Thêm `ParentSetId`, `OriginalSetId`, `CloneDepth`, `CloneCount` vào `VocabularySet.cs` (tiếp nhận: M02-T049).
- `M02-CL-F02`: Triển khai `SetCloningService` bảo toàn tác giả gốc (tiếp nhận: M02-T049; REL-04).
- `M02-CL-F03`: Xây dựng `SetLineageEventSubscriber` xử lý lan truyền thông báo (tiếp nhận: M02-T049).
- `M02-CL-F04`: Thiết lập bộ kiểm thử tự động CL-G01–G10 và CL19-01–20 (tiếp nhận: M02 tasks).
- `M02-CL-F05`: Thu thập bằng chứng runtime cho luồng nguồn gốc bộ từ M02 (tiếp nhận: M02 tasks; A-G03/REL-04).

## 8. Tự kiểm M02-T019

- Đã thiết kế hoàn chỉnh `M02-SET-CLONE-LINEAGE-1.0` với Cấu trúc Cây Nguồn gốc Bất biến (`Lineage Tree`).
- Đã chốt Ràng buộc Bảo toàn Quyền Tác giả Gốc (`OriginalCreatorId`) tuân thủ REL-04.
- Đã xây dựng `SetCloningProtocol` và tính năng tự động tăng `CloneCount`.
- Đã lồng ghép Giao thức Lan truyền Cảnh báo Thu hồi Bản quyền và Cập nhật Nội dung.
- Đã xác lập 10 Regression Gates (`CL-G01`–`CL-G10`) and 20 Test Cases tự kiểm (`CL19-01`–`CL19-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế sao chép và nguồn gốc bộ từ M02-T019 | WSA-7K2 |
