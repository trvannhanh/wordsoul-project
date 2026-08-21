# Thiết kế xử lý thay đổi quản trị đồng thời M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-CONCURRENT-ADMIN-MUTATION-1.0` |
| Task | M01-T032 |
| Đầu vào | M01-ROLE-CHANGE-1.0 (D-094), M01-ACCOUNT-LOCK-UNLOCK-1.0 (D-092), M11-CONCURRENT-EDITING-1.0 (D-053), REL-02 |
| Phạm vi | Mô hình kiểm soát xung đột đồng thời khi nhiều Quản trị viên can thiệp cùng lúc vào trạng thái danh tính của một người dùng, cơ chế OCC `versionDigest` và thứ bậc ưu tiên xử lý tranh chấp |
| Tự kiểm | A-G02; REL-02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Xử lý Thay đổi Quản trị Đồng thời (`Concurrent Admin Mutation Engine`) thuộc M01, loại bỏ rủi ro Race Condition khi hai Quản trị viên (ví dụ: Admin A và Admin B) cùng thực hiện thao tác quản trị trên cùng một tài khoản người dùng tại cùng một thời điểm.

- **Kiểm soát Tương quan Lạc quan OCC `versionDigest` (`OCC Versioning Invariant`)**: Mọi request thay đổi quản trị (đổi vai trò M01-T029, khóa tài khoản M01-T031, thay đổi PII) bắt buộc gửi kèm mã băm phiên bản hiện tại `ExpectedVersionDigest`. Nếu `ExpectedVersionDigest` không khớp với giá trị CSDL tại thời điểm thực thi $\to$ Hệ thống REJECT ngay với mã lỗi `409 Conflict` (`CONCURRENT_ADMIN_MUTATION_CONFLICT`).
- **Thứ bậc Ưu tiên Xử lý Xung đột Quản trị (`Conflict Resolution Priority Hierarchy`)**:
  - *Cấp 1 (Tối cao)*: Khóa tài khoản (`ADMIN_PERMANENT_LOCK`).
  - *Cấp 2*: Hạ cấp vai trò (`Role Demotion`).
  - *Cấp 3*: Thăng cấp vai trò (`Role Promotion`).
  - *Cấp 4*: Cập nhật thông tin hồ sơ/PII (`Profile Edit`).
  - Khi hai request xung đột xảy ra đồng thời trong cùng một transaction CSDL, request có Cấp ưu tiên cao hơn sẽ thắng (`Locking beats Role Change`).
- **Ràng buộc Tăng `SecurityEpoch` Nguyên tử (`Atomic SecurityEpoch Increment`)**: Mọi thao tác quản trị ghi nhận thành công bắt buộc thực hiện tăng `SecurityEpoch` $+1$ nguyên tử trong transaction CSDL. Không thể xảy ra tình trạng đổi vai trò thành công nhưng quên vô hiệu hóa JWT session cũ.
- **Nhật ký Đánh dấu Xung đột Bất biến (`Conflict Audit Logging`)**: $100\%$ xung đột đồng thời bị chặn 409 đều được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-32`), bao gồm mã `ActorUserId` thắng/thua, mã `TargetUserId` và thời điểm xảy ra xung đột.

## 2. Thứ bậc Ưu tiên Xử lý Xung đột Quản trị (Priority Matrix)

| Thao tác A (`Operation A`) | Thao tác B (`Operation B`) | Kết quả Xử lý | Lý do Ưu tiên |
|---|---|---|---|
| Khóa tài khoản (`Lock`) | Đổi vai trò (`Role Change`) | **Lock THẮNG** | Bảo vệ an ninh cấp 1, vô hiệu hóa tài khoản lập tức |
| Hạ cấp vai trò (`Demote`) | Thăng cấp vai trò (`Promote`) | **Hạ cấp THẮNG** | Nguyên tắc an ninh phòng ngừa sự cố (REL-02) |
| Đổi vai trò A1 | Đổi vai trò A2 | **OCC First-Commit THẮNG**, Request 2 bị 409 | Tránh ghi đè ngầm vai trò không đồng nhất |
| Khóa tài khoản A1 | Mở khóa tài khoản A2 | **OCC First-Commit THẮNG**, Request 2 bị 409 | Yêu cầu xem lại trạng thái phiên bản CSDL mới nhất |

## 3. Kiến trúc Xử lý Xung đột Đồng thời (Concurrent Mutation Engine)

```
       [Admin A (Change Role)]                   [Admin B (Lock Account)]
   (Header: ExpectedVersion = V1)            (Header: ExpectedVersion = V1)
                 |                                         |
                 +-------------------+---------------------+
                                     |
                                     v
                       [DB Row-Level Lock Execution]
                       - Transaction Starts (Isolation: Repeatable Read)
                       - Fetch Target User CurrentVersionDigest (V1)
                                     |
                 +-------------------+-------------------+
                 | (First Commit: Admin B Lock)          | (Second Attempt: Admin A Role)
                 v                                       v
      [Validate Version V1 == V1]             [Validate Version V1 == V2]
      - Version Match PASS!                   - Version Mismatch FAIL! (Current = V2)
      - Execute Lock & SecurityEpoch += 1                |
      - Update UserVersionDigest = V2                    v
      - Transaction COMMIT SUCCESS!           [Rollback & Reject 409 Conflict]
                                              - Return CONCURRENT_MUTATION_CONFLICT
                                              - Audit Log ACT-M11-32 Conflict Event
```

## 4. Giao thức Thực thi Xử lý Xung đột CSDL (ConcurrentMutationService)

```csharp
public async Task<bool> ExecuteAdminMutationWithOccAsync(
    string targetUserId, 
    string expectedVersionDigest, 
    AdminMutationType mutationType, 
    Func<User, Task> mutationLogic, 
    string actorUserId)
{
    using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
    try
    {
        // 1. Fetch User row with Row-level Lock (FOR UPDATE)
        var user = await _db.Users
            .FromSqlInterpolated($"SELECT * FROM Users WITH (UPDLOCK, ROWLOCK) WHERE UserId = {targetUserId}")
            .FirstOrDefaultAsync();

        if (user == null) throw new InvalidOperationException("TARGET_USER_NOT_FOUND");

        // 2. Validate OCC Version Digest
        string currentDigest = ComputeUserVersionDigest(user);
        if (currentDigest != expectedVersionDigest)
        {
            await _auditLog.RecordEventAsync("ACT-M11-32", actorUserId, new {
                Status = "409_CONFLICT",
                TargetUserId = targetUserId,
                ExpectedVersion = expectedVersionDigest,
                CurrentVersion = currentDigest,
                AttemptedMutation = mutationType.ToString()
            });

            throw new DbUpdateConcurrencyException("CONCURRENT_ADMIN_MUTATION_CONFLICT: Dữ liệu tài khoản đã bị thay đổi bởi một Quản trị viên khác.");
        }

        // 3. Execute Mutation Logic & Update Version Digest
        await mutationLogic(user);
        user.SecurityEpoch += 1;
        user.LastMutationDigest = ComputeUserVersionDigest(user);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CM-G01` | Mọi request thay đổi quản trị bắt buộc truyền header `If-Match: {expectedVersionDigest}`. |
| `CM-G02` | Khác biệt `expectedVersionDigest` trả về HTTP 409 Conflict với payload mô tả lỗi chi tiết. |
| `CM-G03` | Hai thao tác quản trị đồng thời tuyệt đối CẤM tạo ra trạng thái CSDL bất nhất hoặc treo deadlock. |
| `CM-G04` | Thao tác Khóa tài khoản (`ADMIN_PERMANENT_LOCK`) luôn có ưu tiên tối cao trong mọi xung đột đồng thời. |
| `CM-G05` | Mọi thao tác quản trị thành công tự động cập nhật `LastMutationDigest` mới cho tài khoản target. |
| `CM-G06` | Mọi thao tác bị 409 Conflict tự động ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-32`). |
| `CM-G07` | Isolation Level của transaction quản trị đồng thời duy trì tối thiểu `RepeatableRead`. |
| `CM-G08` | Phân quyền xử lý xung đột quản trị tuân thủ ma trận vai trò M01-T028 (`SecurityAdmin` / `SuperAdmin`). |
| `CM-G09` | SLA phát hiện xung đột đồng thời và trả phản hồi HTTP 409 Conflict $< 15\text{ms}$. |
| `CM-G10` | 100% các test case tự kiểm CM32-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CM32-01` | Admin A và Admin B gửi request đổi vai trò User C cùng lúc với `Version V1` | 1 Admin thành công (200), 1 Admin bị chối (409 Conflict) |
| `CM32-02` | Admin A gửi đổi vai trò (`V1`), Admin B gửi Khóa tài khoản (`V1`) cùng lúc | Thao tác Khóa tài khoản của Admin B thắng, Admin A bị 409 |
| `CM32-03` | Admin A gửi request đổi vai trò nhưng không truyền `If-Match` header | Reject 400 `MISSING_OCC_VERSION_DIGEST` |
| `CM32-04` | Admin A gửi `expectedVersionDigest` rác không khớp với CSDL | Reject 409 `CONCURRENT_ADMIN_MUTATION_CONFLICT` |
| `CM32-05` | Kiểm tra vết Audit Log M11 sau khi xảy ra xung đột đồng thời | Ghi nhận Audit Event `ACT-M11-32` với mã `409_CONFLICT` |
| `CM32-06` | Admin A bị 409 Conflict nạp lại dữ liệu User C mới và gửi lại | Lần 2 gửi với `Version V2` thành công |
| `CM32-07` | Thử nghiệm 10 Admin đồng thời gửi request sửa trạng thái User C | Đúng 1 request thành công, 9 request bị 409 Conflict |
| `CM32-08` | Xung đột xảy ra giữa 1 request Khóa và 1 request Mở khóa | Thao tác COMMIT trước thắng, request sau bị 409 |
| `CM32-09` | Kiểm tra giá trị `SecurityEpoch` sau khi 1 trong 2 request thắng | `SecurityEpoch` chỉ tăng $+1$ duy nhất 1 lần |
| `CM32-10` | Tra cứu giá trị `LastMutationDigest` của User C sau khi đổi thành công | Cập nhật mã băm SHA-256 mới nguyên tử |
| `CM32-11` | Thao tác quản trị đồng thời bị Timeout do nghẽn CSDL | Rollback transaction, không đứt đứt dữ liệu |
| `CM32-12` | Tải đồng thời 100 request xung đột ngẫu nhiên trên 10 tài khoản | 0 lỗi deadlock, 100% xung đột được xử lý an toàn |
| `CM32-13` | User không phải Admin gọi API thay đổi quản trị đồng thời | Deny 403 Forbidden |
| `CM32-14` | User chưa đăng nhập gọi API thay đổi quản trị đồng thời | Deny 401 Unauthorized |
| `CM32-15` | Sửa thông tin PII và Đổi vai trò diễn ra đồng thời | Thao tác Đổi vai trò ưu tiên thực thi trước |
| `CM32-16` | Kiểm tra thời gian phản hồi khi xảy ra 409 Conflict | Response latency $< 12\text{ms}$ |
| `CM32-17` | Phân tích tham chiếu các khóa dòng CSDL trong SQL Server | Đảm bảo sử dụng `UPDLOCK, ROWLOCK` đúng chuẩn (T020) |
| `CM32-18` | Thao tác rollback khi request thắng bị lỗi ở bước ghi log Audit M11 | Rollback toàn bộ transaction CSDL, không lưu lửng lơ |
| `CM32-19` | `SuperAdmin` thực hiện can thiệp khẩn cấp ghi đè OCC | Phải truyền cờ `ForceOverride = true` và ghi log khẩn cấp |
| `CM32-20` | Kiểm thử hoàn tất luồng xử lý thay đổi quản trị đồng thời M01-CONCURRENT-ADMIN-MUTATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-CM-I01` | Entity `User.cs` chưa có thuộc tính `LastMutationDigest` | Chưa hỗ trợ kiểm tra OCC `versionDigest` cho tài khoản người dùng | M01-T049 (Source task) |
| `M01-CM-I02` | Các API quản trị M01 hiện tại chưa yêu cầu `If-Match` header | Nguy cơ bị Race Condition ghi đè ngầm dữ liệu quản trị | M01-T049; M11-T021 |
| `M01-CM-I03` | Thiếu thứ bậc ưu tiên xung đột (Lock beats Role Change) | Rủi ro thao tác đổi vai trò ghi đè làm mất hiệu lực lệnh khóa | M01-T049 |
| `M01-CM-I04` | Thiếu ghi log Audit M11 cho các sự kiện xung đột 409 Conflict | Không phát hiện được các hành vi cố tình spam thao tác quản trị | M01-T049; M11-T031 |
| `M01-CM-I05` | CSDL chưa cấu hình Row-level Lock (`UPDLOCK, ROWLOCK`) khi đọc user | Rủi ro bị deadlock khi hai Admin cùng đọc ghi tài khoản | M01-T049 |

- `M01-CM-F01`: Thêm `LastMutationDigest` vào `User.cs` và CSDL Migration (tiếp nhận: M01-T049).
- `M01-CM-F02`: Triển khai `ConcurrentMutationService` hỗ trợ OCC `versionDigest` và Priority Matrix (tiếp nhận: M01-T049; M11-T021).
- `M01-CM-F03`: Bắt buộc truyền `If-Match` header trên toàn bộ Admin API M01 (tiếp nhận: M01-T049).
- `M01-CM-F04`: Thiết lập bộ kiểm thử tự động CM-G01–G10 và CM32-01–20 (tiếp nhận: M01 tasks).
- `M01-CM-F05`: Thu thập bằng chứng runtime cho luồng quản trị đồng thời M01 (tiếp nhận: M01 tasks; A-G02).

## 8. Tự kiểm M01-T032

- Đã thiết kế hoàn chỉnh `M01-CONCURRENT-ADMIN-MUTATION-1.0` với Giao thức OCC `versionDigest`.
- Đã chốt Ràng buộc Thứ bậc Ưu tiên Xử lý Xung đột (`Lock` > `Demote` > `Promote` > `Profile`).
- Đã quy định Ràng buộc Tăng `SecurityEpoch` Nguyên tử và Trả lỗi HTTP 409 Conflict.
- Đã lồng ghép Nhật ký Đánh dấu Xung đột Bất biến vào Sổ Kiểm toán M11 (`ACT-M11-32`).
- Đã xác lập 10 Regression Gates (`CM-G01`–`CM-G10`) và 20 Test Cases tự kiểm (`CM32-01`–`CM32-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế xử lý thay đổi quản trị đồng thời M01-T032 | WSA-7K2 |
