# Quyết định thực thi solo Giai đoạn A

> Quyết định này được giữ để truy vết. Các quyết định hiện hành đã được hợp nhất tại [DECISIONS.md](../../DECISIONS.md).

## Quyết định

Từ ngày 2026-08-18, Giai đoạn A được thực hiện bởi một người với bí danh duy nhất `WSA-7K2`.

| Nội dung | Trước đây | Áp dụng từ quyết định này |
|---|---|---|
| Người thực hiện | WSA-7K2 và WSA-9M4 | Chỉ WSA-7K2 |
| Phạm vi | 85 + 82 task | 167 task / 412 điểm trong một backlog |
| Nơi làm việc | Hai worktree/nhánh cá nhân | Checkout hiện tại của `vocamon-project` |
| Theo dõi trạng thái | Hai danh sách và hai nhật ký | Một danh sách và một nhật ký tại WSA-7K2 |
| Bàn giao giữa người thực hiện | Sổ bàn giao chéo | Không áp dụng; chuyển thành phụ thuộc nội bộ trong backlog |
| Quyết định và nghiệm thu | Nhiều vai trò chấp thuận | WSA-7K2 tự quyết định, tự kiểm tra và tự nghiệm thu |

## Nguyên tắc đơn giản hóa

1. `BANG-IMPORT-TONG-GIAI-DOAN-A.md` là nguồn phạm vi và owner; `WSA-7K2/DANH-SACH-TASK.md` là nguồn trạng thái thực thi.
2. Mỗi thời điểm chỉ ưu tiên một lát/chuỗi phụ thuộc chính; được gom các task liên quan trong một commit hoặc một lần cập nhật nhật ký.
3. Không tạo bàn giao khi chuyển giữa M01, M02, M11 và M12; chỉ ghi task/phụ thuộc đầu vào trong backlog thống nhất.
4. Evidence ID là tùy chọn để truy vết kiểm thử quan trọng; không phải điều kiện bắt buộc để hoàn thành task.
5. Không nhân đôi tracker, checklist hoặc báo cáo nếu thông tin đã có trong nguồn hiện hành.
6. Parent của task lát `-A` và các phụ thuộc kỹ thuật vẫn được giữ; CT/REL/cổng chỉ là checklist tự kiểm của người thực hiện.

## Quyền quyết định và hoàn thành

- WSA-7K2 tự chốt yêu cầu, giải pháp, ngoại lệ, mức rủi ro và kết quả task.
- Không cần reviewer, authority, người xác nhận hoặc chữ ký riêng để bắt đầu hay hoàn thành.
- Chỉ dùng bốn trạng thái: `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành`.
- Task được `Hoàn thành` khi đầu ra tồn tại, các kiểm tra do người thực hiện chọn đã đạt và kết quả được ghi ngắn gọn trong nhật ký.
- `Bị chặn` chỉ dùng khi thiếu dữ liệu, công cụ hoặc phụ thuộc kỹ thuật thực tế; không dùng vì đang chờ người khác chấp thuận.
- Trong tài liệu cũ, các trường `Reviewer`, `Authority`, `Chủ xác nhận`, `Người xác nhận`, `Chờ xác nhận` hoặc nội dung yêu cầu phê duyệt chỉ là ghi chú lịch sử/tham khảo và không tạo điều kiện chặn.

## Chuyển đổi hồ sơ cũ

- 82 task của WSA-9M4 được chuyển nguyên ID, phụ thuộc, baseline, cổng và trạng thái sang WSA-7K2.
- Khu vực WSA-9M4 và sổ bàn giao chéo được giữ chỉ để truy vết lịch sử, không còn là nguồn điều hành.
- Các dòng bàn giao cũ trở thành ghi chú phụ thuộc nội bộ; việc lưu trữ không có nghĩa đầu ra hoặc điều kiện phụ thuộc đã đạt.
- Quyết định chuyển đổi không tự đánh dấu task hoàn thành; trạng thái được thay đổi khi người thực hiện bắt đầu hoặc tự kiểm tra xong.

## Tiêu chí kiểm tra chuyển đổi

- Bảng import có đúng 167 task / 412 điểm và mọi dòng chỉ ghi người thực hiện `WSA-7K2`.
- Danh sách solo có đủ 167 Task ID, không trùng, giữ nguyên tổng điểm và dùng đúng bốn trạng thái solo.
- Không còn tài liệu đang hoạt động yêu cầu nhánh WSA-9M4 hoặc bàn giao chéo.
- Không có file mã nguồn bị thay đổi.

## Lịch sử

| Ngày | Thay đổi | Người quyết định |
|---|---|---|
| 2026-08-18 | Chuyển từ mô hình hai người sang một người thực hiện | Người dùng/chủ dự án |
| 2026-08-18 | Bỏ worktree riêng, reviewer bắt buộc và các bước chờ chấp thuận | Người dùng/chủ dự án |
