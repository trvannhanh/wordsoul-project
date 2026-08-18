# Luồng xác minh thư điện tử M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T006 |
| Phiên bản/trạng thái | 0.1-draft — chờ M01-T002/M01-T005 và hợp đồng gửi thư tối thiểu |
| Cơ sở | M01-D002: trước xác minh chỉ học/xem giới hạn; khóa xuất bản, xã hội, PvP, thông báo ngoài ứng dụng và thao tác nhạy cảm |

## Hợp đồng nghiệp vụ

| Thành phần | Yêu cầu |
|---|---|
| Evidence | Opaque, một lần, có hạn, gắn account/purpose/policy version; không lưu/log dạng thô |
| Gửi | Dùng request ID idempotent; retry không tạo nhiều intent hiệu lực ngoài chính sách |
| Gửi lại | Có limiter theo account/địa chỉ/nguồn phù hợp; lần mới xử lý hiệu lực lần cũ theo policy đã duyệt |
| Xác nhận | Kiểm tra purpose, account state, expiry, consumed/revoked và concurrency trước chuyển trạng thái |
| Quyền trước xác minh | Chỉ allowlist năng lực giới hạn; mọi consumer dùng trạng thái nguồn M01, không tin claim cũ để nâng quyền |
| Audit | Ghi requested/dispatched/failed/verified/expired/revoked bằng metadata, result/reason/correlation; không ghi email/evidence thô |

## Trình tự và nhánh lỗi

1. M01 tạo verification intent sau khi danh tính hợp lệ, rồi yêu cầu năng lực thư gửi template/version đã đăng ký.
2. Thất bại trước/giữa gửi giữ trạng thái `Chờ xác minh thư`, ghi kết quả và cho retry theo limiter; không tạo lại tài khoản.
3. Người dùng trình evidence; M01 consume nguyên tử. Hai yêu cầu đồng thời chỉ một kết quả chuyển trạng thái thành công.
4. Evidence hết hạn, sai purpose/account, đã dùng hoặc đã thu hồi bị từ chối thống nhất và không tiết lộ chi tiết nhạy cảm.
5. Sau xác minh, trạng thái chuyển `Hoạt động` hoặc `Chờ điều kiện tuổi/đồng ý`; phiên/claim cũ không tự mở quyền vượt trạng thái.

## Ca nghiệm thu tối thiểu

- Gửi và xác minh thành công; evidence hết hạn/dùng lại/sai account/sai purpose.
- Gửi lại, retry cùng request ID, hai callback đồng thời và timeout sau consume.
- Provider thư lỗi/chậm; limiter lỗi; callback giả mạo; log sink lỗi theo taxonomy audit.
- Tài khoản đã khóa/chờ xóa trước callback không được kích hoạt lại.
- Quyền trước/sau xác minh đúng allowlist trên mọi entry point.

## Điểm chờ và điều kiện duyệt

| Mã | Điểm chờ | Nguồn gỡ |
|---|---|---|
| M01-VER-O01 | TTL, cooldown, số lần và hành vi invalidation | Chủ M01/an toàn/sản phẩm; registry M11 |
| M01-VER-O02 | Hợp đồng request/result/template/provider thư | Chủ năng lực thư/M12 |
| M01-VER-O03 | Allowlist entry point trước xác minh | M01-T009 và chủ M03/M08/M09/M10 |

Chỉ duyệt khi O01–O03 được xác nhận, M01-T002/T005 đạt và bộ ca expiry/replay/concurrency/dependency/audit chạy được với Evidence ID hợp lệ.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo hợp đồng intent/evidence, quyền giới hạn và nhánh lỗi | Chưa gán |
