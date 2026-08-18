# Mẫu bằng chứng A-G06 — Sẵn sàng ứng phó

## 1. Hồ sơ nghiệm thu

| Trường | Nội dung |
|---|---|
| Phạm vi phát hành | [Điền] |
| Chủ trì cổng | [Điền] |
| Người chỉ huy sự cố dự phòng | [Điền] |
| Người xác nhận | [Điền] |
| REL bắt buộc | REL-02; REL-03 |
| Task trọng tâm | M11-T036–M11-T037; M11-T043–M11-T048; M12-T045–M12-T047 |
| Kết luận | [Chưa đánh giá/Đạt/Không đạt/Đạt có điều kiện] |

## 2. Sổ sức khỏe và SLO

| Năng lực | Chủ | SLI | SLO | Error budget | Dữ liệu health thật | Độ mới | Trạng thái degraded | Ngưỡng cảnh báo | Mục tiêu phản hồi | Mục tiêu khôi phục | Playbook | Evidence ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [Điền] | [Điền] | [Availability/Latency/Correctness/Freshness/Quota] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Liên kết] | [EV] |

## 3. Mô hình mức độ sự cố

| Mức | Tiêu chí | Ví dụ phạm vi | Người chỉ huy | Thời gian phản hồi | Nhịp cập nhật | Quyền bảo trì/kill switch | Đối tượng thông báo | Điều kiện hạ mức/đóng |
|---|---|---|---|---|---|---|---|---|
| SEV-1 | [Điền] | [Rò dữ liệu/mất toàn vẹn/gián đoạn lõi diện rộng] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] |
| SEV-2 | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] |
| SEV-3 | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] |
| SEV-4 | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] |

## 4. Mẫu playbook sự cố

| Mục | Nội dung |
|---|---|
| Tên/kịch bản sự cố | [Điền] |
| Tín hiệu phát hiện | [Điền cảnh báo/metric/báo cáo] |
| Phạm vi và mức ban đầu | [Điền] |
| Người chỉ huy và người thay thế | [Điền] |
| Hành động 15 phút đầu | [Điền theo thứ tự] |
| Cách khống chế lan rộng | [Bảo trì/kill switch/cô lập/fail-closed phù hợp] |
| Dữ liệu cần bảo toàn | [Điền] |
| Cách khôi phục | [Điền] |
| Cách xác minh sau khôi phục | [Health thật, đối soát, kiểm tra người dùng] |
| Kế hoạch truyền thông | [Đối tượng, nhịp, người duyệt nội dung] |
| Điều kiện rollback | [Điền] |
| Điều kiện đóng sự cố | [Điền] |
| Hành động hậu kiểm | [Điền] |

## 5. Biên bản diễn tập

| Trường | Nội dung |
|---|---|
| Exercise ID | [EX-A-001] |
| Kịch bản | [Điền] |
| Ngày/giờ và múi giờ | [Điền] |
| Phạm vi/môi trường | [Điền] |
| Người tham gia và vai trò | [Điền] |
| Điều kiện an toàn/dừng diễn tập | [Điền] |
| Tín hiệu đầu tiên | [Điền] |
| Thời điểm phát hiện | [Điền] |
| Thời điểm phân mức và giao chỉ huy | [Điền] |
| Thời điểm khống chế | [Điền] |
| Thời điểm khôi phục | [Điền] |
| Kết quả đối soát | [Điền] |
| Mục tiêu đạt/không đạt | [Điền] |
| Sai lệch và task xử lý | [Điền Finding ID/Task ID] |
| Người xác nhận kết quả | [Điền] |

## 6. Kịch bản diễn tập tối thiểu

| Case ID | Kịch bản | Năng lực cần chứng minh | Evidence ID | Đạt/Không đạt |
|---|---|---|---|---|
| G06-E01 | Audit không ghi được khi có thao tác nhạy cảm | Dừng an toàn, cảnh báo, bảo toàn bằng chứng và khôi phục | [EV] | [Điền] |
| G06-E02 | Provider danh tính gián đoạn | Không fail-open xác thực; phiên hợp lệ xử lý đúng; truyền thông phù hợp | [EV] | [Điền] |
| G06-E03 | Tích hợp ngoài chậm gây cạn tài nguyên | Deadline/circuit/bulkhead và suy giảm bảo vệ lõi | [EV] | [Điền] |
| G06-E04 | Nội dung công khai bị báo cáo khẩn | Tạm ẩn, điều tra, thu hồi và tác động xuyên module | [EV] | [Điền] |
| G06-E05 | Bí mật nghi lộ | Khoanh vùng, thu hồi/xoay vòng, xác minh và đối soát | [EV] | [Điền] |
| G06-E06 | Công việc nền chạy lặp hoặc bỏ sót | Dừng, chạy lại idempotent và đối soát | [EV] | [Điền] |
| G06-E07 | Khóa/xóa tài khoản thất bại một phần | Giữ trạng thái an toàn, retry và báo phần chưa hoàn tất | [EV] | [Điền] |

## 7. Điều kiện đạt

- Health phản ánh dữ liệu/phụ thuộc thật và có trạng thái degraded rõ.
- Mỗi cảnh báo có mức, chủ, ngưỡng, đường escalation và playbook.
- Bảo trì/kill switch thực hiện đúng phạm vi và không trả thành công giả.
- Bốn mức sự cố, mục tiêu phản hồi/khôi phục và truyền thông đã được duyệt.
- Các diễn tập tối thiểu đạt hoặc sai lệch đã được xử lý và xác nhận lại.
- REL-02 và REL-03 có đủ bằng chứng phục hồi liên quan.
