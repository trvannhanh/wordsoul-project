# Quy tắc làm việc solo

> Tài liệu lịch sử. Quy trình hiện hành nằm trong [PROJECT.md](../../PROJECT.md) và [TASKS.md](../../TASKS.md).

## Nơi làm việc duy nhất

| Người thực hiện | Repository | Phạm vi |
|---|---|---|
| WSA-7K2 | Checkout hiện tại của `vocamon-project` | 167 task / 412 điểm Giai đoạn A |

Không tạo worktree hoặc nhánh theo bí danh. Trước khi sửa file, chỉ cần kiểm tra thay đổi có sẵn để không ghi đè và xác nhận Task ID tồn tại trong danh sách solo.

## Chu kỳ tối giản

1. **Chọn:** lấy một task sẵn sàng hoặc một nhóm nhỏ cùng phụ thuộc.
2. **Đọc:** module, quyết định, backlog, lát/cổng và dependency trực tiếp.
3. **Làm:** cập nhật nguồn trước dẫn xuất; không mở rộng sang mã nguồn nếu task chỉ là tài liệu.
4. **Kiểm:** đầu ra, Task ID, trạng thái, liên kết, tổng 167/412, không bí mật/PII và không file ngoài phạm vi.
5. **Ghi:** cập nhật danh sách solo và nhật ký; evidence chỉ khi có kiểm chứng.
6. **Commit:** một commit cho một nhóm công việc liên quan; không cần bước chấp thuận riêng.

## Trạng thái và thẩm quyền

- Chỉ dùng: `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành`.
- `Đang thực hiện` khi bắt đầu làm; `Hoàn thành` khi người thực hiện tự kiểm tra đầu ra; `Bị chặn` khi thiếu dữ liệu/công cụ/phụ thuộc kỹ thuật thực tế.
- WSA-7K2 tự quyết định và tự nghiệm thu; không có reviewer hoặc authority bắt buộc.
- Phụ thuộc giữa module là phụ thuộc nội bộ, không phải bàn giao giữa người.

## Bảo vệ worktree

- Làm trong checkout hiện tại và không ghi đè thay đổi có sẵn của người dùng.
- Chỉ push, merge, rebase hoặc viết lại lịch sử khi chính người dùng yêu cầu thao tác Git đó.
- Task tài liệu không cho phép thay đổi mã nguồn; task triển khai chỉ thay đổi đúng Definition of Done.
