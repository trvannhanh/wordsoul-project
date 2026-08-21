# Chuẩn hóa thêm bộ vào thư viện M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-LIBRARY-ADD-SET-1.0` |
| Task | M02-T039 |
| Đầu vào | M01-T001, M02-SET-PERMISSIONS-1.0 (M02-T016), M02-SET-VERSIONED-PUBLISHING-1.0 (M02-T032) |
| Phạm vi | Quy trình thêm bộ từ vựng đã duyệt vào thư viện cá nhân, xử lý idempotency chống trùng lặp và đồng bộ trạng thái sang M03/M04 |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình người học thêm một bộ từ vựng (System Set hoặc Public Custom Set) vào Thư viện cá nhân (`UserLibrary`).

- **Chỉ Thêm Bộ từ Hợp lệ (`Valid Set Invariant`)**: CHỈ cho phép thêm các bộ từ ở trạng thái `PUBLISHED` và có `IsPublic = true` (hoặc System Set). Tuyệt đối CẤM thêm các bộ từ ở trạng thái `DRAFT`, `IN_REVIEW`, `RECALLED` hoặc bộ từ riêng tư của người khác.
- **Tính Chống Trùng lặp (`Idempotency Invariant`)**: Gửi yêu cầu thêm cùng một bộ từ nhiều lần CHỈ tạo ra 1 liên kết duy nhất trong thư viện cá nhân (`UserLibrarySet`), không làm tăng bản ghi lặp.
- **Đồng bộ Trạng thái Thống nhất (`Unified State Synchronization Invariant`)**: Khi thêm thành công bộ từ vào thư viện, M02 phát ra sự kiện `UserLibrarySetAddedIntegrationEvent` để M03 có thể cho phép tạo phiên học và M04 sẵn sàng khởi tạo tiến độ SRS.

## 2. Quy trình Thêm Bộ từ vào Thư viện (Workflow)

```mermaid
graph TD
    User[Learner] -->|POST /api/v1/library/sets/{setId}| Gateway[API Gateway]
    Gateway --> Guard[Set Eligibility Guard]
    Guard -->|Check Published & Public| DB[(UserLibrary DB)]
    DB -->|UPSERT UserLibrarySet| Event[UserLibrarySetAdded Event]
    Event --> M03[M03 Session Ready]
    Event --> M04[M04 SRS Ready]
```

## 3. Cấu trúc Bản ghi Thư viện Cá nhân (UserLibrarySet Schema)

```csharp
public class UserLibrarySet
{
    public Guid UserLibrarySetId { get; set; }
    public Guid UserId { get; set; }
    public Guid VocabularySetId { get; set; }
    public string PinnedRevisionDigest { get; set; } // Ghim phiên bản bộ từ tại thời điểm thêm
    public DateTime AddedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## 4. Regression Gates và Test Cases

### 4.1. Regression Gates
- `LA-G01`: 100% bộ từ không ở trạng thái `PUBLISHED` bị từ chối khi thêm vào thư viện (HTTP 400 `SET_NOT_ELIGIBLE_FOR_LIBRARY`).
- `LA-G02`: Gọi API thêm cùng bộ từ 5 lần liên tiếp chỉ sinh 1 bản ghi `UserLibrarySet` duy nhất.
- `LA-G03`: Bộ từ cá nhân riêng tư của người khác (`IsPublic = false`) bị từ chối thêm (HTTP 403 `PRIVATE_SET_ACCESS_DENIED`).

### 4.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LA39-01` | Learner A thêm 1 System Set `PUBLISHED` vào thư viện | Thêm thành công, phát event `UserLibrarySetAdded`. |
| `LA39-02` | Learner A bấm thêm lại bộ từ đã có trong thư viện | Trả về HTTP 200 OK với thông báo bộ từ đã có trong thư viện, không tạo bản ghi mới. |
| `LA39-03` | Thử thêm bộ từ đang ở trạng thái `IN_REVIEW` | System reject với lỗi `SET_NOT_PUBLISHED`. |
| `LA39-04` | Thử thêm bộ từ riêng tư của Learner B | System reject với lỗi `FORBIDDEN_PRIVATE_SET`. |
| `LA39-05` | Kiểm thử hoàn tất luồng M02-LIBRARY-ADD-SET-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 5. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-LA-F01` | Cần tạo bảng `UserLibrarySets` trong CSDL | Chưa có schema lưu thư viện bộ từ cá nhân | M02-T049 |

## 6. Tự kiểm M02-T039
- Đã hoàn thành đặc tả `M02-LIBRARY-ADD-SET-1.0`.
- Chốt 3 Regression Gates (`LA-G01`–`LA-G03`) và 5 Test Cases (`LA39-01`–`LA39-05`).

## 7. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa thêm bộ vào thư viện M02-T039 | WSA-7K2 |
