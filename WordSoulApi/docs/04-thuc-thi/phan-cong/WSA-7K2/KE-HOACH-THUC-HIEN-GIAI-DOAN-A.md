# Kế hoạch thực hiện solo Giai đoạn A

## Mục tiêu

Hoàn thành 167 task / 412 điểm bằng một backlog, một nhánh và một nhật ký. Trình tự được quyết định bởi phụ thuộc và cổng, không bởi ranh giới người thực hiện cũ.

## Luồng ưu tiên

| Đợt | Phạm vi | Điều kiện ra |
|---|---|---|
| 0 — Checklist nền | A0-T001–T012 | REL/CT có phạm vi, quyết định và tiêu chí tự kiểm rõ ràng |
| 1 — Registry nền | M01/M02/M11/M12 Lát 1 | Từ điển, vòng đời, data/config/capability/asset registry có phiên bản và owner |
| 2 — Luồng cốt lõi | M01/M02/M11/M12 Lát 2 | Danh tính, quyền, OAuth, audit, log, limiter và nội dung có ca cho phép/từ chối/lỗi phụ thuộc |
| 3 — Thay đổi có kiểm soát | Lát 3A–3D | Version, preview, rollout, rollback, reference và asset/profile/device đạt |
| 4 — Vận hành | M11 Lát 4 + M12 liên quan | Health, alert, job, incident, backup/recovery có diễn tập và reconciliation |
| 5 — Tự nghiệm thu | Lát 5/5A/5B | Dữ liệu, bộ test, sáu checklist cổng và quyết định phát hành do WSA-7K2 chốt |

## WIP và batching

- Tối đa một chuỗi phụ thuộc chính ở `Đang thực hiện`; task hỗ trợ chỉ mở khi trực tiếp gỡ chuỗi đó.
- Một batch nên gồm 1–4 task cùng module/lát hoặc cùng hợp đồng; dùng một commit khi thay đổi gắn kết.
- Task thiếu dữ liệu/công cụ/phụ thuộc thật sự giữ `Bị chặn`; không có trạng thái chờ người khác duyệt.
- Mọi phụ thuộc từng được ghi BG-001–BG-016 được đọc trực tiếp như dependency nội bộ, không cần xác nhận bàn giao giữa bí danh.

## Điểm kiểm tra bắt buộc

1. Sau mỗi batch: ID, owner, dependency, gate, links, secret/PII scan, diff scope và trạng thái.
2. Sau mỗi lát: đủ 167 task / 412 điểm, không trùng ID/owner, parent `-A` còn mở đúng.
3. Trước `Hoàn thành`: đầu ra tồn tại và người thực hiện đã tự chạy kiểm tra phù hợp.
4. Evidence ID chỉ tạo khi hữu ích cho truy vết, không phải cổng bắt buộc.
5. Trước Cổng A: WSA-7K2 tự rà finding, REL/CT và phạm vi phát hành rồi ghi quyết định.

## Trạng thái chuyển đổi

Việc chuyển sang solo chỉ đổi owner và cách điều phối. Mọi trạng thái task được bảo toàn; không task nào tự động đạt hoặc bắt đầu chỉ vì cùng một người sở hữu.
