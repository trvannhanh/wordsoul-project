# Xây dựng ma trận vai trò và quyền M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ROLE-MATRIX-1.0` |
| Task | M01-T028 |
| Đầu vào | M01-T001 (D-015), M11-T004 (D-004), REL-02 |
| Phạm vi | Ma trận phân quyền RBAC toàn hệ thống, danh mục 6 vai trò chính, quy tắc kiểm soát quyền tối thiểu và SLA thu hồi quyền $\le 30\text{s}$ |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Ma trận Vai trò và Phân quyền Tổng thể (`Role & Permission Matrix System`) thuộc M01, phục vụ công tác kiểm soát truy cập dựa trên vai trò (RBAC) cho toàn bộ các chức năng từ M01 đến M12.

- **Nguyên tắc Phân quyền Tối thiểu (`Principle of Least Privilege - REL-02`)**: Mỗi vai trò người dùng chỉ được cấp đúng và đủ các quyền (`Permissions`) tối thiểu cần thiết để thực hiện đúng trách nhiệm nghiệp vụ của mình.
- **Ràng buộc Vô hiệu hóa Quyền tức thời (`Role Claim Invalidation Invariant`)**: Khi vai trò của một tài khoản bị thay đổi hoặc thu hồi, tất cả các Token JWT active và Cache Session M01 tương ứng bắt buộc bị vô hiệu hóa hoặc làm mới claim trong thời gian $\le 30$ giây.
- **Định danh Vai trò Bất biến (`Immutable Role Enums`)**: Danh mục vai trò bao gồm 6 vai trò tiêu chuẩn. CẤM cấp quyền Quản trị viên cho tài khoản người học thông thường mà không có quyết định phê duyệt chính thức M11.
- **Cách ly Quyền Quản trị Cao nhất (`SuperAdmin Privilege Isolation`)**: Vai trò `SuperAdmin` sở hữu các quyền nhạy cảm nhất (như xoay key bảo mật, đổi quyền admin) và bắt buộc bảo vệ bằng giao thức Phê duyệt Kép (`Dual-Approval Gate` M01-T030).

## 2. Danh mục 6 Vai trò Hệ thống Tiêu chuẩn (System Roles Catalog)

| Mã Role | Tên vai trò | Mô tả phạm vi trách nhiệm | Mức độ Đặc quyền |
|---|---|---|---|
| `Learner` | Người học | Học từ vựng M03, ôn tập SRS M04, tham gia PvP M08, xem BXH M09, mua vật phẩm M06. | Cơ bản (`P0`) |
| `ContentCreator` | Tác giả biên soạn | Tạo Bộ từ cá nhân/công khai, gửi bài tập kiểm duyệt M02, quản lý bộ từ cá nhân. | Trung bình (`P1`) |
| `SupportAgent` | Nhân viên Hỗ trợ | Tra cứu người dùng M11-T027, xem lịch sử vụ việc REL-07, hỗ trợ mở khóa tài khoản. | Trung bình (`P1`) |
| `ContentAdmin` | Quản trị Biên tập | Phê duyệt/Từ chối mục từ & bộ từ M02, quản lý kiểm duyệt tài sản media M02-T012. | Cao (`P2`) |
| `SecurityAdmin` | Quản trị An ninh | Cấp/Thu hồi quyền M11-T005, xem Audit Log M11, quản lý xoay vòng bí mật M12. | Rất cao (`P3`) |
| `SuperAdmin` | Quản trị Tối cao | Phê duyệt các thay đổi quản trị cấp cao, bảo vệ bằng phê duyệt kép M01-T030. | Tối cao (`P4`) |

## 3. Ma trận Vai trò và Phân quyền Chi tiết (Role-Permission Matrix)

| Quyền hạn (`Permission Code`) | Nguồn Module | `Learner` | `ContentCreator` | `SupportAgent` | `ContentAdmin` | `SecurityAdmin` | `SuperAdmin` |
|---|---|---|---|---|---|---|---|
| `user:profile:write` | M01 | CÓ | CÓ | CÓ | CÓ | CÓ | CÓ |
| `set:custom:create` | M02 | CÓ | CÓ | CÓ | CÓ | CÓ | CÓ |
| `set:public:submit` | M02 | KHÔNG | CÓ | KHÔNG | CÓ | KHÔNG | CÓ |
| `headword:approve` | M02 | KHÔNG | KHÔNG | KHÔNG | CÓ | KHÔNG | CÓ |
| `asset:moderate` | M02 / M12 | KHÔNG | KHÔNG | KHÔNG | CÓ | KHÔNG | CÓ |
| `user:search:safe` | M11 | KHÔNG | KHÔNG | CÓ | CÓ | CÓ | CÓ |
| `support:ticket:manage` | M11 | KHÔNG | KHÔNG | CÓ | KHÔNG | KHÔNG | CÓ |
| `role:grant:assign` | M11 / M01 | KHÔNG | KHÔNG | KHÔNG | KHÔNG | CÓ | CÓ |
| `audit:log:read` | M11 | KHÔNG | KHÔNG | KHÔNG | KHÔNG | CÓ | CÓ |
| `secret:rotate` | M12 | KHÔNG | KHÔNG | KHÔNG | KHÔNG | CÓ | CÓ |
| `superadmin:override` | M01 / M11 | KHÔNG | KHÔNG | KHÔNG | KHÔNG | KHÔNG | CÓ |

## 4. Giao thức Kiểm tra Phân quyền và Thu hồi Session (Authorization & Invalidation Engine)

```
[Incoming API Request with JWT Token]
                  |
                  v
    [AuthorizeAttribute(Permission)]
                  |
                  v
     (Validate Role Claims in JWT)
                  |
         +--------+--------+
         | (Invalid Claim) | (Valid Claim)
         v                 v
  [Return 403 Forbidden] [Check Security Epoch in Redis]
                           |
                 +---------+---------+
                 | (Epoch Stale)     | (Epoch Fresh)
                 v                   v
          [Reject & Force   [Execute Controller Action]
           Re-authentication]
```

### 4.1. Cơ chế Thu hồi Quyền Tức thời qua Security Epoch (SLA $\le 30\text{s}$)
Khi `SecurityAdmin` thay đổi quyền của User `10024`:
1. Cập nhật `UserRoles` trong CSDL.
2. Tăng giá trị `SecurityEpoch` của User `10024` trong Redis: `user_epoch:10024 = Epoch + 1`.
3. Khi User `10024` gửi request tiếp theo, Middleware so sánh `Epoch` trong JWT với Redis:
   - Nếu `JWT.Epoch < Redis.Epoch` $\implies$ Tự động reject request với lỗi `TOKEN_SECURITY_EPOCH_STALE`, yêu cầu đăng nhập lại để làm mới JWT claims.

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RM-G01` | 100% người dùng thuộc đúng 1 trong 6 vai trò hệ thống tiêu chuẩn. |
| `RM-G02` | Mọi API endpoint yêu cầu quyền quản trị được bảo vệ bởi `[Authorize(Permissions = ...)]`. |
| `RM-G03` | Người học (`Learner`) tuyệt đối KHÔNG thể truy cập các API tìm kiếm người dùng hay duyệt bài. |
| `RM-G04` | Thay đổi vai trò người dùng tự động kích hoạt cập nhật `SecurityEpoch` trong Redis. |
| `RM-G05` | SLA thu hồi quyền active sau khi thay đổi vai trò đạt $\le 30$ giây. |
| `RM-G06` | Cấm gán vai trò `SuperAdmin` trực tiếp qua API thông thường (phải qua Phê duyệt Kép M01-T030). |
| `RM-G07` | Cấm tác giả tự phê duyệt nội dung của mình (`Self-Approval Guard` M02-T007). |
| `RM-G08` | Mọi thao tác cấp/thu hồi vai trò ghi vết Audit Event bất biến `ACT-M11-05` (M11-T005). |
| `RM-G09` | Quyền xem Audit Log và xoay bí mật giới hạn nghiêm ngặt cho `SecurityAdmin` và `SuperAdmin`. |
| `RM-G10` | 100% các test case tự kiểm RM28-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RM28-01` | Người học (`Learner`) truy cập API học từ vựng | Phân quyền thành công, trả về 200 OK |
| `RM28-02` | Người học (`Learner`) cố tình gọi API duyệt mục từ `headword:approve` | System reject 403 Forbidden |
| `RM28-03` | `ContentCreator` gửi Bộ từ cá nhân lên hàng chờ xuất bản | Cho phép gửi, phân quyền `set:public:submit` thành công |
| `RM28-04` | `SupportAgent` gọi API tìm kiếm người dùng an toàn M11-T027 | Cho phép tìm kiếm với `user:search:safe` |
| `RM28-05` | `SupportAgent` thử gọi API phê duyệt kiểm duyệt tài sản media | System reject 403 Forbidden |
| `RM28-06` | `ContentAdmin` phê duyệt kiểm duyệt tài sản media M02-T012 | Phân quyền `asset:moderate` hợp lệ, duyệt thành công |
| `RM28-07` | `SecurityAdmin` cấp vai trò `ContentAdmin` cho User B | Cấp quyền thành công, tăng `SecurityEpoch` cho B |
| `RM28-08` | User B gửi request ngay sau khi được cấp vai trò mới | JWT cũ bị từ chối do Epoch lệch, yêu cầu re-auth |
| `RM28-09` | User B đăng nhập lại nhận JWT claims mới | JWT mới chứa vai trò `ContentAdmin`, cho phép duyệt bài |
| `RM28-10` | Thử cấp vai trò `SuperAdmin` qua API cấp quyền thông thường | System reject với lỗi `REQUIRES_DUAL_APPROVAL` |
| `RM28-11` | `SecurityAdmin` thực hiện xoay bí mật JWT RSA Key | Phân quyền `secret:rotate` hợp lệ, xoay key thành công |
| `RM28-12` | `ContentAdmin` thử gọi API xoay bí mật hệ thống | System reject 403 Forbidden |
| `RM28-13` | Tải đồng thời 100 request kiểm tra phỏng quyền từ Middleware | Response latency overhead $< 2\text{ms}$ |
| `RM28-14` | Thu hồi vai trò `SupportAgent` của User C | Cập nhật `SecurityEpoch` trong Redis, vô hiệu hóa session |
| `RM28-15` | User C thử dùng Token cũ sau 10 giây | System reject 401 Unauthorized do Security Epoch stale |
| `RM28-16` | Phân tích tham chiếu trước khi thu hồi vai trò quản trị | Quét các ticket và công việc đang xử lý của admin (T020) |
| `RM28-17` | Kiểm tra tính bất biến của danh mục 6 vai trò tiêu chuẩn | Cấm tự tạo role ngẫu nhiên không thuộc enum |
| `RM28-18` | Xem vết Audit Log M11 sau khi cấp vai trò mới | Ghi nhận Audit Event `ACT-M11-05` với diff chi tiết |
| `RM28-19` | `SuperAdmin` thực hiện thao tác override toàn quyền hệ thống | Kiểm tra phê duyệt kép thành công, thực thi thao tác |
| `RM28-20` | Kiểm thử hoàn tất luồng ma trận vai trò và quyền M01-ROLE-MATRIX-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-RM-I01` | Trong `WordSoulApi`, phân quyền hiện tại chỉ dựa trên chuỗi Role cơ bản | Chưa chi tiết hóa theo danh mục Permission Code tiêu chuẩn | M01-T049 (Source task) |
| `M01-RM-I02` | Chưa có cơ chế `SecurityEpoch` trong Redis để thu hồi quyền ngay lập tức | Khi hạ vai trò Admin, JWT cũ vẫn dùng được cho đến khi hết TTL | M01-T049; REL-02 |
| `M01-RM-I03` | Thiếu validation chặn cấp trực tiếp vai trò `SuperAdmin` | Rủi ro bị lạm dụng quyền quản trị cao nhất | M01-T049; M01-T030 |
| `M01-RM-I04` | Chưa có ma trận phân quyền công khai minh bạch cho 6 vai trò | Khó khăn cho công tác kiểm toán an ninh | M01-T049 |
| `M01-RM-I05` | Chưa tích hợp kiểm tra `Self-Approval Guard` ở cấp độ Authorization Filter | Biên tập viên có thể tự phê duyệt bài của mình | M01-T049; M02-T007 |

- `M01-RM-F01`: Nâng cấp `PermissionAuthorizationHandler` kiểm tra Permission Code (tiếp nhận: M01-T049).
- `M01-RM-F02`: Triển khai `SecurityEpochMiddleware` với Redis Caching (tiếp nhận: M01-T049; REL-02).
- `M01-RM-F03`: Xây dựng `DualApprovalGuard` cho vai trò SuperAdmin (tiếp nhận: M01-T049; M01-T030).
- `M01-RM-F04`: Thiết lập bộ kiểm thử tự động RM-G01–G10 và RM28-01–20 (tiếp nhận: M01 tasks).
- `M01-RM-F05`: Thu thập bằng chứng runtime cho luồng ma trận vai trò M01 (tiếp nhận: M01 tasks; A-G02/REL-02).

## 8. Tự kiểm M01-T028

- Đã thiết kế hoàn chỉnh `M01-ROLE-MATRIX-1.0` với Ma trận Phân quyền 6 Vai trò Tiêu chuẩn.
- Đã chốt Ràng buộc Nguyên tắc Phân quyền Tối thiểu (`REL-02`).
- Đã xây dựng Cơ chế Vô hiệu hóa Quyền Tức thời qua `SecurityEpoch` SLA $\le 30\text{s}$.
- Đã lồng ghép bảo vệ Phê duyệt Kép cho vai trò `SuperAdmin` và Cấm tự duyệt (`Self-Approval Guard`).
- Đã xác lập 10 Regression Gates (`RM-G01`–`RM-G10`) và 20 Test Cases tự kiểm (`RM28-01`–`RM28-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả ma trận vai trò và quyền M01-T028 | WSA-7K2 |
