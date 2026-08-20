# Xây dựng ma trận quyền bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-PERMISSIONS-1.0` |
| Task | M02-T016 |
| Đầu vào | M01-ROLE-MATRIX-1.0 (D-075), M02-HEADWORD-LIFECYCLE-1.0 (D-062), M11-T004 (D-004), REL-02 |
| Phạm vi | Ma trận phân quyền sở hữu và thao tác đối với Bộ từ vựng (`VocabularySet`), quy tắc cách ly bộ từ cá nhân và kiểm soát thao tác xuất bản/chỉnh sửa |
| Tự kiểm | A-G02, A-G03; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Ma trận Phân quyền và Sở hữu Bộ từ vựng (`Vocabulary Set Access & Ownership Matrix`) thuộc M02, phân định rõ quyền hạn giữa Bộ từ Hệ thống (`IsCustom == false`) và Bộ từ Cá nhân (`IsCustom == true`).

- **Quyền Sở hữu Tuyệt đối Bộ từ Cá nhân (`Custom Set Ownership Invariant`)**: Bộ từ cá nhân do `CreatorId` khởi tạo. Chỉ có tác giả tạo ra bộ từ hoặc `ContentAdmin` mới có quyền xem/sửa/xóa bộ từ đó. Các người học khác tuyệt đối KHÔNG thể xem hoặc sửa bộ từ cá nhân của người khác nếu chưa được chia sẻ.
- **Ràng buộc Quyền Kiểm soát Bộ từ Hệ thống (`System Set Governance Gate`)**: Bộ từ vựng Hệ thống (`IsCustom == false`) do Ban biên tập quản lý. Chỉ người dùng có vai trò `ContentAdmin` hoặc `SuperAdmin` mới có quyền thêm/bớt từ, sửa thông tin hoặc xuất bản bộ từ hệ thống.
- **Quy tắc Kiểm soát Phân quyền Tối thiểu (`REL-02 Compliance`)**: Mọi request thao tác trên Bộ từ vựng bắt buộc qua Bộ kiểm tra Phân quyền (`SetAuthorizationGuard`). CẤM cho phép người học thường sửa nội dung bộ từ hệ thống.
- **Chính sách Cách ly Dữ liệu Công khai và Riêng tư (`Visibility Isolation Protocol`)**:
  - *Public Sets*: Hiển thị công khai cho $100\%$ người học trên catalog bộ từ hệ thống.
  - *Private Custom Sets*: Ẩn khỏi catalog công khai, chỉ hiển thị trong danh sách "Bộ từ của tôi" của chính `CreatorId`.

## 2. Ma trận Quyền Thao tác theo Vai trò và Loại Bộ từ (Set Operation Matrix)

| Thao tác trên Bộ từ (`Operation`) | Nguồn API | Bộ từ Cá nhân (`IsCustom=true`, Chủ sở hữu) | Bộ từ Cá nhân (`IsCustom=true`, Người khác) | Bộ từ Hệ thống (`IsCustom=false`, Learner) | Bộ từ Hệ thống (`IsCustom=false`, ContentAdmin) |
|---|---|---|---|---|---|
| Xem chi tiết & Học bài (`READ`) | GET `/sets/{id}` | CÓ | KHÔNG (Deny 403) | CÓ | CÓ |
| Sửa thông tin Bộ từ (`UPDATE`) | PUT `/sets/{id}` | CÓ | KHÔNG (Deny 403) | KHÔNG (Deny 403) | CÓ |
| Thêm / Bớt Mục từ (`MODIFY_ITEMS`)| POST/DELETE `/sets/{id}/items` | CÓ | KHÔNG (Deny 403) | KHÔNG (Deny 403) | CÓ |
| Xóa Bộ từ (`DELETE`) | DELETE `/sets/{id}` | CÓ | KHÔNG (Deny 403) | KHÔNG (Deny 403) | CÓ (Soft-Archive) |
| Gửi duyệt Xuất bản Công khai (`SUBMIT`)| POST `/sets/{id}/submit` | CÓ | KHÔNG (Deny 403) | KHÔNG (N/A) | CÓ |
| Phê duyệt / Từ chối Duyệt (`APPROVE`)| POST `/sets/{id}/approve` | KHÔNG (Cấm tự duyệt) | KHÔNG | KHÔNG | CÓ |

## 3. Kiến trúc Bộ kiểm tra Phân quyền Bộ từ (SetAuthorizationGuard Engine)

```csharp
public async Task<AuthorizationResult> ValidateSetAccessAsync(int setId, string userId, string userRole, SetOperation operation)
{
    var set = await _db.VocabularySets.FirstOrDefaultAsync(s => s.VocabularySetId == setId);
    if (set == null) return AuthorizationResult.Failed("VOCABULARY_SET_NOT_FOUND");

    // 1. Phê duyệt cho ContentAdmin & SuperAdmin
    if (userRole == "ContentAdmin" || userRole == "SuperAdmin")
    {
        return AuthorizationResult.Success();
    }

    // 2. Kiểm tra thao tác trên Bộ từ Cá nhân
    if (set.IsCustom)
    {
        if (set.CreatorId != userId)
        {
            return AuthorizationResult.Failed("FORBIDDEN_CUSTOM_SET_ACCESS", "Bạn không có quyền truy cập bộ từ cá nhân của người khác.");
        }
        return AuthorizationResult.Success();
    }

    // 3. Kiểm tra thao tác trên Bộ từ Hệ thống
    if (!set.IsCustom)
    {
        if (operation == SetOperation.Read)
        {
            return set.IsPublished 
                ? AuthorizationResult.Success() 
                : AuthorizationResult.Failed("UNPUBLISHED_SYSTEM_SET", "Bộ từ hệ thống chưa được xuất bản.");
        }

        // Mọi thao tác WRITE/DELETE/MODIFY trên bộ từ hệ thống yêu cầu ContentAdmin
        return AuthorizationResult.Failed("FORBIDDEN_SYSTEM_SET_MUTATION", "Chỉ ContentAdmin mới có quyền thay đổi bộ từ hệ thống.");
    }

    return AuthorizationResult.Failed("INVALID_SET_OPERATION");
}
```

## 4. Giao thức Chuyển đổi Quyền Sở hữu và Sao chép Bộ từ (Copy & Transfer Protocol)

- **Sao chép Bộ từ Cá nhân (Clone Set)**: Khi Người học A sao chép một Bộ từ Hệ thống hoặc Bộ từ công khai B, hệ thống tạo ra một Bộ từ Cá nhân mới `Set_New` có `IsCustom = true`, `CreatorId = UserId_A`, sao chép nguyên trạng danh sách từ vựng.
- **Chuyển giao Quyền Sở hữu (Owner Transfer)**: Khi một tài khoản bị vô hiệu hóa hoặc đổi vai trò (M02-T018), các Bộ từ Cá nhân công khai được gán lại `CreatorId = SystemAdminId` để không làm mất bộ từ.

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SP-G01` | Người học A tuyệt đối KHÔNG thể xem hoặc sửa Bộ từ Cá nhân riêng tư của Người học B (Deny 403). |
| `SP-G02` | Người học thông thường tuyệt đối KHÔNG thể thêm/bớt từ hoặc sửa Bộ từ Hệ thống (`IsCustom = false`). |
| `SP-G03` | Chỉ `ContentAdmin` và `SuperAdmin` mới có quyền phê duyệt xuất bản Bộ từ vựng Hệ thống. |
| `SP-G04` | Cấm tác giả tự duyệt xuất bản Bộ từ cá nhân do chính mình tạo ra (`Self-Approval Guard`). |
| `SP-G05` | Thao tác xóa Bộ từ vựng Hệ thống chỉ thực hiện Soft-Archive, cấm xóa cứng (Physical DELETE) trong CSDL. |
| `SP-G06` | Bộ từ Cá nhân được sao chép (Clone) gắn `CreatorId` của người sao chép và thiết lập `IsCustom = true`. |
| `SP-G07` | Mọi thao tác thay đổi quyền sở hữu bộ từ ghi vết Audit Event M11 (`ACT-M11-04`). |
| `SP-G08` | Phân quyền thao tác bộ từ tuân thủ nghiêm ngặt ma trận vai trò M01 (`M01-ROLE-MATRIX-1.0`). |
| `SP-G09` | SLA kiểm tra phân quyền bộ từ qua `SetAuthorizationGuard` $< 5\text{ms}$. |
| `SP-G10` | 100% các test case tự kiểm SP16-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SP16-01` | Người học A xem chi tiết Bộ từ Cá nhân do chính A tạo | Phân quyền thành công, trả về 200 OK |
| `SP16-02` | Người học B thử truy cập Bộ từ Cá nhân của Người học A | System reject 403 Forbidden |
| `SP16-03` | Người học A xem chi tiết Bộ từ Hệ thống B1 đã xuất bản | Phân quyền READ thành công, trả về 200 OK |
| `SP16-04` | Người học A thử gửi request PUT sửa tên Bộ từ Hệ thống B1 | System reject 403 Forbidden |
| `SP16-05` | Người học A thử gửi request DELETE xóa Bộ từ Hệ thống B1 | System reject 403 Forbidden |
| `SP16-06` | `ContentAdmin` gửi request PUT sửa tên Bộ từ Hệ thống B1 | Phân quyền hợp lệ, cập nhật tên bộ từ thành công |
| `SP16-07` | `ContentAdmin` xóa Bộ từ Hệ thống B1 không còn dùng | Chuyển `IsArchived = true`, không xóa cứng CSDL |
| `SP16-08` | `ContentCreator` A gửi duyệt Bộ từ cá nhân lên công khai | Tạo Yêu cầu Duyệt `Submitted` thành công |
| `SP16-09` | `ContentCreator` A thử tự bấm nút Phê duyệt cho Bộ từ của mình | System reject với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `SP16-10` | `ContentAdmin` B thực hiện Phê duyệt Bộ từ của `ContentCreator` A | Phê duyệt thành công, chuyển `IsPublished = true` |
| `SP16-11` | Người học A thực hiện Sao chép (Clone) Bộ từ B1 | Tạo bộ từ mới `IsCustom = true`, `CreatorId = UserId_A` |
| `SP16-12` | Tải đồng thời 100 request kiểm tra phân quyền bộ từ | Response latency overhead $< 3\text{ms}$ |
| `SP16-13` | User không đăng nhập thử truy cập Bộ từ Cá nhân riêng tư | Deny 401 Unauthorized |
| `SP16-14` | User không đăng nhập truy cập Bộ từ Hệ thống công khai | Cho phép READ bài học công khai |
| `SP16-15` | Xem vết Audit Log M11 sau khi `ContentAdmin` sửa bộ từ | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `SP16-16` | Thử thêm 1 mục từ Substandard vào bộ từ hệ thống | Reject theo chốt kiểm soát chất lượng M02-T015 |
| `SP16-17` | Phân tích tham chiếu trước khi lưu kho 1 bộ từ vựng | Quét các active session M03 đang mở bộ từ (T020) |
| `SP16-18` | `SuperAdmin` thực hiện gán lại quyền sở hữu bộ từ cho Admin mới | Đổi `CreatorId` thành công, ghi audit log |
| `SP16-19` | Thử truy cập Bộ từ Hệ thống đang ở trạng thái `Draft` bằng tài khoản Learner | System reject 403 Forbidden |
| `SP16-20` | Kiểm thử hoàn tất luồng ma trận quyền bộ từ M02-SET-PERMISSIONS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SP-I01` | Trong `VocabularySetsController.cs`, chưa có `SetAuthorizationGuard` kiểm tra quyền sở hữu | Người dùng B có thể đoán ID và sửa bộ từ cá nhân của A | M02-T049 (Source task) |
| `M02-SP-I02` | Chưa có cơ chế phân biệt giữa bộ từ công khai và bộ từ cá nhân riêng tư | Tất cả bộ từ cá nhân đang bị lộ công khai qua API search | M02-T049; REL-02 |
| `M02-SP-I03` | Thiếu kiểm tra `Self-Approval Guard` khi duyệt xuất bản bộ từ | Tác giả bộ từ có thể tự xuất bản bộ từ của mình | M02-T049; M02-T007 |
| `M02-SP-I04` | API xóa bộ từ hệ thống hiện chưa dùng Soft-Archive | Rủi ro xóa cứng gây đứt đoạn dữ liệu lịch sử SRS M04 | M02-T049 |
| `M02-SP-I05` | Chưa triển khai tính năng Clone Set tự động gán `IsCustom = true` | Người học sao chép bộ từ làm hỏng dữ liệu bộ từ gốc | M02-T049 |

- `M02-SP-F01`: Triển khai `SetAuthorizationGuard` cho 100% API bộ từ vựng (tiếp nhận: M02-T049).
- `M02-SP-F02`: Tích hợp bộ lọc Visibility cho bộ từ cá nhân riêng tư (tiếp nhận: M02-T049; REL-02).
- `M02-SP-F03`: Cài đặt `SelfApprovalGuard` cho luồng duyệt bộ từ (tiếp nhận: M02-T049).
- `M02-SP-F04`: Thiết lập bộ kiểm thử tự động SP-G01–G10 và SP16-01–20 (tiếp nhận: M02 tasks).
- `M02-SP-F05`: Thu thập bằng chứng runtime cho luồng ma trận quyền bộ từ M02 (tiếp nhận: M02 tasks; A-G02/A-G03).

## 8. Tự kiểm M02-T016

- Đã thiết kế hoàn chỉnh `M02-SET-PERMISSIONS-1.0` với Ma trận Quyền Thao tác theo Vai trò và Loại Bộ từ.
- Đã chốt Ràng buộc Quyền Sở hữu Tuyệt đối Bộ từ Cá nhân và Cách ly Dữ liệu Riêng tư.
- Đã xây dựng `SetAuthorizationGuard` và Giao thức Sao chép/Chuyển giao bộ từ.
- Đã lồng ghép bảo vệ Cấm tự duyệt (`Self-Approval Guard`) và Soft-Archive khi xóa bộ từ hệ thống.
- Đã xác lập 10 Regression Gates (`SP-G01`–`SP-G10`) và 20 Test Cases tự kiểm (`SP16-01`–`SP16-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả ma trận quyền bộ từ M02-T016 | WSA-7K2 |
