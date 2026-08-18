# Thực thi solo Giai đoạn A

> Quy trình này đã được thay bằng [PROJECT.md](../../PROJECT.md), [DECISIONS.md](../../DECISIONS.md) và [TASKS.md](../../TASKS.md). Nội dung dưới đây chỉ để truy vết.

## Mô hình hiện hành

| Người thực hiện | Phạm vi | Số task | Điểm S/M/L | Khu vực làm việc |
|---|---|---:|---:|---|
| WSA-7K2 | M01, M02, M11, M12 và toàn bộ task điều phối Giai đoạn A | 167 | 412 | [Mở khu vực solo](./WSA-7K2/README.md) |

Quyết định chuyển đổi được ghi tại [Quyết định thực thi solo](./QUYET-DINH-THUC-THI-SOLO.md). `WSA-9M4` không còn là owner đang hoạt động; khu vực cũ chỉ được giữ để truy vết.

## Một luồng làm việc

1. Đọc [bảng import tổng](../../03-ke-hoach-giai-doan-a/BANG-IMPORT-TONG-GIAI-DOAN-A.md) để xác định phạm vi và phụ thuộc.
2. Chọn một task `Chưa bắt đầu` trong [danh sách solo](./WSA-7K2/DANH-SACH-TASK.md).
3. Đọc nguồn module/quyết định/backlog và lát liên quan; không bắt đầu từ riêng một dòng task.
4. Thực hiện một nhóm nhỏ cùng chuỗi phụ thuộc; cập nhật task và [nhật ký](./WSA-7K2/NHAT-KY.md).
5. Tự kiểm tra đầu ra, ghi kết quả ngắn gọn và chuyển task sang `Hoàn thành`.

## Những gì được bỏ

- Không chia task theo hai người, không dùng nhánh WSA-9M4.
- Không mở bàn giao chéo khi chuyển giữa module; dùng trực tiếp quan hệ phụ thuộc trong danh sách solo.
- Không duy trì hai tracker, hai nhật ký hoặc hai báo cáo trạng thái.
- Không tạo Evidence ID cho bản nháp chưa được kiểm chứng.
- Không có bước chờ reviewer, authority, người xác nhận hoặc chữ ký.

## Những gì vẫn bắt buộc

- Người thực hiện tự chốt quyết định sản phẩm, kỹ thuật, dữ liệu và mức rủi ro cần thiết cho task.
- Task hoàn thành sau khi người thực hiện tự kiểm tra đầu ra; Evidence ID chỉ dùng khi thấy hữu ích.
- Không đóng parent khi task hậu tố `-A` còn phạm vi hoãn thật sự.
- Không ghi token, bí mật, PII thật hoặc payload thô.
- Không sửa mã nguồn trong task chỉ về tài liệu.

## Tài liệu vận hành

- [Prompt khởi động solo](./WSA-7K2/PROMPT-KHOI-DONG.md)
- [Kế hoạch thực hiện solo](./WSA-7K2/KE-HOACH-THUC-HIEN-GIAI-DOAN-A.md)
- [Quy tắc nhánh và cập nhật](./QUY-TAC-NHANH-LAM-VIEC.md)
- [Sổ bàn giao cũ — chỉ lưu trữ](./SO-BAN-GIAO-CHEO.md)
- [Khu vực WSA-9M4 — chỉ lưu trữ](./WSA-9M4/README.md)
