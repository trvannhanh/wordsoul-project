# WordSoul Docs

Chỉ có ba tài liệu vận hành cần đọc và cập nhật thường xuyên:

1. [PROJECT.md](./PROJECT.md): mục tiêu, kiến trúc, quy tắc và Definition of Done.
2. [DECISIONS.md](./DECISIONS.md): quyết định đang có hiệu lực và câu hỏi chưa chốt.
3. [TASKS.md](./TASKS.md): backlog, dependency, trạng thái và kết quả/commit.

## Quy trình

```text
Chọn task → Đang thực hiện → Làm → Kiểm tra → Ghi kết quả/commit → Hoàn thành
```

Nếu thiếu dữ liệu, công cụ hoặc dependency kỹ thuật thật, dùng `Bị chặn` và ghi hành động tiếp theo. WSA-7K2 tự quyết định và tự nghiệm thu; Git là nhật ký công việc.

## Tài liệu tham khảo

Các thư mục `01-tong-quan` đến `05-bang-chung` chứa phân tích module, kế hoạch cũ, REL/CT và checklist chi tiết. Chỉ đọc khi task liên quan; không đồng bộ trạng thái vào các tracker, bảng import, sổ bàn giao hoặc nhật ký cũ. `90-luu-tru` chỉ dùng để truy vết.

Codex dùng [`$wordsoul-doc-workflow`](../.agents/skills/wordsoul-doc-workflow/SKILL.md) cho mọi thay đổi task hoặc tài liệu WordSoul.
