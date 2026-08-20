# Xử lý chủ sở hữu không còn hoạt động M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-INACTIVE-OWNER-HANDLING-1.0` |
| Task | M02-T018 |
| Đầu vào | M01-ACCOUNT-LOCK-UNLOCK-1.0 (D-092), M02-SET-PERMISSIONS-1.0 (D-076), REL-07 |
| Phạm vi | Giao thức quản lý và bảo toàn bộ từ vựng khi tài khoản tác giả bị khóa, bị xóa hoặc không hoạt động $> 180$ ngày (`Inactive Owner`), phân định giữa Bộ từ Cá nhân Riêng tư vs Bộ từ Công khai |
| Tự kiểm | A-G01, A-G03; REL-07 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Xử lý Bộ từ vựng khi Chủ sở hữu Không còn Hoạt động (`Inactive Set Owner Handling Engine`) thuộc M02, đảm bảo trải nghiệm học tập của cộng đồng không bị gián đoạn khi tác giả tạo bộ từ bị khóa tài khoản hoặc ngừng hoạt động.

- **3 Dấu hiệu Tác giả Không còn Hoạt động (`3-Condition Inactivity Invariant`)**:
  - *Tài khoản bị Khóa (`AccountStatus == Locked`)*: Do vi phạm chính sách hoặc bị khóa an ninh (M01-T031 / D-092).
  - *Tài khoản đã Xóa (`IsDeleted == true`)*: Người dùng chủ động xóa tài khoản (M01-T020).
  - *Ngừng Hoạt động Không Đăng nhập $> 180$ Ngày (`LastLoginAtUtc <= Now - 180 days`)*: Không có tương tác trong 6 tháng.
- **Ràng buộc Bảo toàn Bộ từ Công khai (`Public Set Preservation Invariant`)**: Bộ từ vựng công khai (`IsPublic == true`) của tác giả không còn hoạt động VẪN ĐƯỢC GIỮ NGUYÊN trên catalog công khai để người học khác tiếp tục học tập và làm bài M03. CẤM tự động xóa hoặc ẩn các bộ từ công khai hợp lệ.
- **Khóa Chỉnh sửa và Chuyển giao Quản trị (`Mutation Lock & Admin Takeover Protocol`)**:
  - Tự động bật `IsMutationLocked = true` cho các Bộ từ thuộc tác giả không còn hoạt động để ngăn chặn sửa đổi trái phép.
  - `ContentAdmin` có quyền chuyển giao quyền quản trị hoặc fork bộ từ công khai này thành Bộ từ Hệ thống (`IsSystemSet = true`), giữ nguyên ghi nhận tác giả gốc (`OriginalCreatorId`) theo REL-04.
- **Ẩn Bộ từ Cá nhân Riêng tư (`Private Custom Set Hiding`)**: Các bộ từ cá nhân riêng tư (`IsPublic == false`) của tác giả bị khóa/xóa sẽ tự động ẩn khỏi mọi API truy vấn và tìm kiếm.

## 2. Ma trận Xử lý Bộ từ theo Trạng thái Tác giả (Owner Inactivity Matrix)

| Trạng thái Tác giả (`Owner Status`) | Loại Bộ từ (`Set Type`) | Trạng thái Bộ từ (`Set Status`) | Quyền Học tập M03 | Quyền Chỉnh sửa (`Mutation`) | Quyền Quản trị Admin |
|---|---|---|---|---|---|
| **Locked** (Khóa) | Bộ từ Công khai (`IsPublic=true`) | `Published` (Khóa Mutation) | CHO PHÉP | BỊ KHÓA (`IsMutationLocked=true`) | `ContentAdmin` có thể Fork sang System Set |
| **Locked** (Khóa) | Bộ từ Riêng tư (`IsPublic=false`) | `Draft` (Hidden) | BỊ KHÓA | BỊ KHÓA | Không can thiệp trừ khi có ticket |
| **Deleted** (Đã xóa) | Bộ từ Công khai (`IsPublic=true`) | `Published` (Ghi nhận Tác giả gốc) | CHO PHÉP | BỊ KHÓA (`IsMutationLocked=true`) | Tự động gán `OrphanedPublicSet` |
| **Deleted** (Đã xóa) | Bộ từ Riêng tư (`IsPublic=false`) | `Archived` (Soft-Deleted) | BỊ KHÓA | BỊ KHÓA | Lưu kho bất biến 30 ngày rồi purge |
| **Inactive > 180d** | Bộ từ Công khai (`IsPublic=true`) | `Published` | CHO PHÉP | BỊ KHÓA tạm thời | Admin có thể gửi cảnh báo qua Email |

## 3. Kiến trúc Quét và Xử lý Tác giả Không Hoạt động (Inactive Owner Engine)

```
[Background Job / User Lock Event Triggers Inactive Check]
                           |
                           v
           [Query Sets Owned by Target UserId]
                           |
            +--------------+--------------+
            | (IsPublic == false)         | (IsPublic == true)
            v                             v
   [Hide / Archive Private Sets]   [Preserve Public Sets for Learners]
   - Set Status = Archived         - Set IsMutationLocked = true
   - Purge from Search API         - Keep Active in Catalog (M03)
   - Preserve SRS History          - Set InactiveOwnerFlag = true
                                          |
                                          v
                              [Optional Admin Action: Fork to System Set]
                              - Copy to IsSystemSet = true
                              - Retain OriginalCreatorId (REL-04)
```

## 4. Giao thức Thực thi CSDL (InactiveOwnerHandlingService)

```csharp
public async Task ProcessInactiveOwnerSetsAsync(string inactiveUserId, string reasonCode, string moderatorActorId = null)
{
    var userSets = await _db.VocabularySets
        .Where(s => s.CreatorId == inactiveUserId)
        .ToListAsync();

    foreach (var set in userSets)
    {
        if (!set.IsPublic)
        {
            // 1. Bộ từ cá nhân riêng tư -> Ẩn và Soft-Archive
            set.Status = VocabularySetStatus.Archived;
            set.IsMutationLocked = true;
            await _cacheService.RemoveAsync($"lesson_payload:{set.VocabularySetId}");
        }
        else
        {
            // 2. Bộ từ công khai -> Khóa chỉnh sửa nhưng giữ nguyên trên Catalog công khai
            set.IsMutationLocked = true;
            set.InactiveOwnerFlag = true;
            set.InactiveReason = reasonCode;
            
            // Xóa cache để làm mới trạng thái IsMutationLocked
            await _cacheService.RemoveAsync($"lesson_payload:{set.VocabularySetId}");
        }
    }

    await _db.SaveChangesAsync();

    // 3. Ghi vết Audit Log M11
    if (!string.IsNullOrEmpty(moderatorActorId))
    {
        await _auditLog.RecordEventAsync("ACT-M11-04", moderatorActorId, new { Action = "PROCESS_INACTIVE_OWNER_SETS", TargetUserId = inactiveUserId, Reason = reasonCode, ProcessedCount = userSets.Count });
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `IO-G01` | Khóa hoặc xóa tài khoản tác giả KHÔNG làm mất hoặc ẩn các Bộ từ vựng công khai (`IsPublic == true`). |
| `IO-G02` | Người học khác tiếp tục học bài M03 trên Bộ từ công khai của tác giả đã không còn hoạt động. |
| `IO-G03` | Bộ từ công khai của tác giả không còn hoạt động tự động bật `IsMutationLocked = true`. |
| `IO-G04` | Bộ từ cá nhân riêng tư (`IsPublic == false`) của tác giả không hoạt động tự động ẩn khỏi Search API. |
| `IO-G05` | Tiến độ SRS M04 của người học trên bộ từ của tác giả không còn hoạt động tuyệt đối KHÔNG bị ảnh hưởng. |
| `IO-G06` | `ContentAdmin` có quyền Fork bộ từ công khai của tác giả không còn hoạt động thành Bộ từ Hệ thống (`IsSystemSet = true`). |
| `IO-G07` | Khi Fork thành Bộ từ Hệ thống, thuộc tính `OriginalCreatorId` giữ nguyên mã tác giả ban đầu (REL-04). |
| `IO-G08` | 100% thao tác xử lý bộ từ của tác giả không hoạt động ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-04`). |
| `IO-G09` | SLA xử lý chuyển trạng thái toàn bộ bộ từ của 1 tác giả $< 50\text{ms}$. |
| `IO-G10` | 100% các test case tự kiểm IO18-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IO18-01` | Tài khoản tác giả A bị `SecurityAdmin` khóa vĩnh viễn | Bộ từ công khai 108 của A vẫn hiển thị trên catalog công khai |
| `IO18-02` | Người học B bấm học bài M03 trên Bộ từ 108 của tác giả A bị khóa | Nạp bài học M03 thành công, bài học chạy bình thường |
| `IO18-03` | Tác giả A bị khóa cố gắng đăng nhập và gọi API sửa Bộ từ 108 | Deny 401 Unauthorized do tài khoản A bị khóa |
| `IO18-04` | Tác giả A có Bộ từ riêng tư 109 (`IsPublic = false`) khi bị khóa | Bộ từ 109 chuyển `Archived`, ẩn khỏi mọi API tìm kiếm |
| `IO18-05` | Tác giả A chủ động thực hiện Xóa Tài khoản | Bộ từ công khai 108 bật `IsMutationLocked = true`, `OriginalCreatorId = "USR-A"` |
| `IO18-06` | `ContentAdmin` thực hiện Fork Bộ từ 108 thành Bộ từ Hệ thống 201 | Tạo bộ từ hệ thống mới, gán `IsSystemSet = true`, giữ `OriginalCreatorId` |
| `IO18-07` | Kiểm tra tiến độ SRS M04 của Người học B trên Bộ từ 108 sau khi A bị khóa | Khung giờ ôn tập SRS của B giữ nguyên không bị gián đoạn |
| `IO18-08` | Tác giả C không đăng nhập hệ thống trong 181 ngày ($> 180$d) | Bộ từ công khai của C bật cờ `InactiveOwnerFlag = true` |
| `IO18-09` | Tác giả C đăng nhập lại hệ thống ở ngày thứ 185 | Hệ thống làm mới `LastLoginAtUtc`, gỡ cờ `InactiveOwnerFlag` |
| `IO18-10` | Tra cứu vết Audit Log M11 sau khi xử lý bộ từ của tác giả bị khóa | Ghi nhận Audit Event `ACT-M11-04` kèm mã tác giả bị khóa |
| `IO18-11` | Tra cứu danh sách các bộ từ công khai thuộc sở hữu của tác giả bị khóa | Trả về danh sách bộ từ kèm cờ `IsMutationLocked = true` |
| `IO18-12` | Tải đồng thời 50 request học bài M03 trên bộ từ của tác giả bị khóa | Response latency p95 $< 30\text{ms}$ |
| `IO18-13` | User không phải Admin thử gọi API Fork bộ từ của tác giả không hoạt động | Deny 403 Forbidden |
| `IO18-14` | User chưa đăng nhập thử gọi API xử lý bộ từ tác giả không hoạt động | Deny 401 Unauthorized |
| `IO18-15` | Người học B sao chép (Clone) Bộ từ 108 của tác giả A bị khóa | Sao chép thành công, `ParentSetId = 108`, `OriginalCreatorId = "USR-A"` |
| `IO18-16` | Xóa cache Redis `lesson_payload:108` sau khi chuyển trạng thái khóa | Cache Redis tự động bị xoá và tái tạo dữ liệu mới |
| `IO18-17` | Phân tích tham chiếu các liên kết bộ từ của tác giả không hoạt động | Quét bảng `UserStudyProgress` M04 (T020) |
| `IO18-18` | Thao tác xử lý bộ từ bị gián đoạn giữa chừng do lỗi CSDL | Rollback transaction, trạng thái bộ từ giữ nguyên |
| `IO18-19` | Khôi phục tài khoản A từ trạng thái `Locked` về `Active` | Gỡ cờ `IsMutationLocked` nếu bộ từ chưa từng bị Admin can thiệp |
| `IO18-20` | Kiểm thử hoàn tất luồng xử lý chủ sở hữu không còn hoạt động M02-INACTIVE-OWNER-HANDLING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-IO-I01` | Entity `VocabularySet.cs` chưa có thuộc tính `InactiveOwnerFlag` | Không đánh dấu được các bộ từ thuộc tác giả ngừng hoạt động | M02-T049 (Source task) |
| `M02-IO-I02` | Chưa tự động bật `IsMutationLocked = true` khi tài khoản tác giả bị khóa | Rủi ro bị can thiệp sửa đổi trái phép khi tài khoản tác giả bị xâm nhập | M02-T049; M01-T031 |
| `M02-IO-I03` | Thiếu tính năng Fork bộ từ công khai của tác giả bị khóa sang System Set | Ban quản trị khó khăn khi muốn tiếp quản nội dung chất lượng | M02-T049 |
| `M02-IO-I04` | Chưa tự động ẩn các bộ từ riêng tư (`IsPublic = false`) khi tác giả bị xóa | Bộ từ riêng tư vẫn xuất hiện trong các API liệt kê dữ liệu rác | M02-T049 |
| `M02-IO-I05` | Thiếu background job quét các tài khoản inactive $> 180$ ngày | Không phát hiện được các bộ từ công khai thiếu tác giả duy trì | M02-T049; M11-T038 |

- `M02-IO-F01`: Thêm `InactiveOwnerFlag` và `InactiveReason` vào `VocabularySet.cs` (tiếp nhận: M02-T049).
- `M02-IO-F02`: Triển khai `InactiveOwnerHandlingService` liên kết với `AccountLockService` M01 (tiếp nhận: M02-T049; M01-T031).
- `M02-IO-F03`: Tích hợp tính năng Fork sang System Set duy trì `OriginalCreatorId` REL-04 (tiếp nhận: M02-T049).
- `M02-IO-F04`: Thiết lập bộ kiểm thử tự động IO-G01–G10 và IO18-01–20 (tiếp nhận: M02 tasks).
- `M02-IO-F05`: Thu thập bằng chứng runtime cho luồng xử lý tác giả không hoạt động M02 (tiếp nhận: M02 tasks; A-G01/A-G03).

## 8. Tự kiểm M02-T018

- Đã thiết kế hoàn chỉnh `M02-INACTIVE-OWNER-HANDLING-1.0` với Giao thức Phân định 3 Dấu hiệu Tác giả Không hoạt động.
- Đã chốt Ràng buộc Bảo toàn Bộ từ Công khai (`Public Set Preservation Invariant`).
- Đã chốt Giao thức Khóa Chỉnh sửa (`IsMutationLocked = true`) và Chuyển giao Quản trị Fork sang System Set.
- Đã lồng ghép bảo tồn Ghi nhận Tác giả Gốc (`OriginalCreatorId` REL-04) và Bảo toàn Tiến độ SRS M04.
- Đã xác lập 10 Regression Gates (`IO-G01`–`IO-G10`) và 20 Test Cases tự kiểm (`IO18-01`–`IO18-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả xử lý chủ sở hữu không còn hoạt động M02-T018 | WSA-7K2 |
