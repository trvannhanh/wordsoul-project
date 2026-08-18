# CT-06 — Giữ AI và giọng nói tắt

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T011 |
| Trạng thái | Có hiệu lực từ 2026-08-18 cho toàn bộ Giai đoạn A/B |
| Dependency | A0-T001 và A0-T003 đã hoàn thành; REL-01/REL-03 đã mở và chưa đóng |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Quyết định | D-010 |
| Phạm vi áp dụng | API, web, admin, mobile, job nền, feature flag, provider và mọi luồng thu/gửi dữ liệu của Giai đoạn A/B |
| Rủi ro kiểm soát | Dữ liệu, đặc biệt âm thanh hoặc dữ liệu người chưa thành niên, được gửi ra ngoài khi chưa đủ đồng ý và kiểm soát tích hợp |

## Quy tắc tạm thời

- Không bật tạo hoặc xuất bản học liệu bằng AI trong Giai đoạn A/B.
- Không bật xử lý giọng nói sâu hoặc gửi âm thanh người dùng ra nhà cung cấp ngoài.
- Không thu thập dữ liệu “để dùng sau” cho các năng lực đang tắt.
- Không có thử nghiệm ngầm, bật theo nhóm nhỏ hoặc ngoại lệ vận hành ngoài phạm vi đã duyệt.
- Không để endpoint, UI, job, feature flag hay fallback có thể tạo provider traffic cho các năng lực bị tắt trong release A/B.
- Mã nguồn hoặc thiết kế thử nghiệm không được coi là năng lực hoạt động; không dùng dữ liệu người dùng thật và không nối vào hành trình phát hành.

## Ma trận phạm vi tắt

| Năng lực | Trạng thái A/B | Hành vi bắt buộc | Ghi chú ranh giới |
|---|---|---|---|
| AI sinh/sửa/xuất bản học liệu | Tắt | Không endpoint/UI/job/provider traffic; nội dung không được tạo hoặc công khai bằng AI | CT-01 vẫn cấm nội dung chưa duyệt nếu phạm vi tương lai được mở |
| AI chat, phản hồi hoặc cá nhân hóa sinh | Tắt | Không gửi prompt, hồ sơ, tiến độ hay nội dung người dùng ra provider | Không thu dữ liệu “để dùng sau” |
| Speech-to-text/xử lý giọng nói sâu | Tắt | Không thu/gửi audio người dùng và không gọi provider | Bao gồm transcription/phân tích giọng nói ngoài phạm vi đã duyệt |
| Chấm phát âm dùng audio người dùng/provider ngoài | Tắt | Không upload, lưu tạm hoặc truyền audio cho năng lực này | Chỉ mở bằng quyết định mới sau A/B |
| Voice cloning/tổng hợp từ mẫu người dùng | Tắt | Không thu mẫu, huấn luyện, tạo giọng hoặc lưu embedding | Không có thử nghiệm nhóm nhỏ |
| Phát audio học liệu tĩnh đã có quyền | Không thuộc CT-06 | Có thể tồn tại nếu không thu/gửi audio người dùng và đáp ứng REL-04 | Không được diễn giải thành xử lý giọng nói đang bật |

## Tự kiểm A-G04/A-G05

| Phạm vi kiểm tra | Kết quả yêu cầu | Trạng thái |
|---|---|---|
| Registry A-G04 | Năng lực AI/giọng nói bị tắt được ghi rõ trạng thái, không có active provider traffic, fallback hoặc dependency phát hành | Tiêu chí đã ghi; chờ registry/runtime |
| Cấu hình và entry point | API/UI/job/feature flag mặc định và deployed state đều không thể kích hoạt ngoài quyết định mới | Tiêu chí đã ghi; chờ kiểm tra cấu hình/runtime |
| Bản đồ dữ liệu A-G05 | Không có flow prompt/audio/hồ sơ/tiến độ tới provider cho năng lực bị tắt | Tiêu chí đã ghi; chờ M12-T042-A |
| Inventory bí mật A-G05 | Không có credential đang được workload A/B sử dụng cho năng lực bị tắt; secret còn tồn tại phải có chủ và trạng thái kiểm soát | Tiêu chí đã ghi; chờ M12-T040/M12-T041 |
| Bằng chứng không lưu lượng | Chỉ dùng metadata tổng hợp/allowlist chứng minh không có lời gọi; không lưu payload, PII, prompt hoặc audio | Tiêu chí đã ghi; chờ bằng chứng runtime |

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh sách năng lực tắt, đường dữ liệu liên quan, người kiểm tra và kết quả không có lưu lượng ngoài |
| Nhịp kiểm tra | Khi thêm/sửa provider, endpoint, UI, job hoặc feature flag; trước A-G04/A-G05; và trước mọi kết luận phát hành A/B |
| Khi vi phạm | Tắt năng lực, dừng gửi dữ liệu, đánh giá nghĩa vụ thông báo và xử lý dữ liệu đã truyền |
| Điều kiện gỡ | Không gỡ trong A/B; giai đoạn sau phải có quyết định mới thay thế D-010, REL-01/REL-03 và cổng pháp lý, riêng tư, dữ liệu ngoài, quota, suy giảm tương ứng đạt |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng quyết định mới; đầu vào pháp lý/riêng tư bắt buộc khi có dữ liệu tuổi, đồng ý, cá nhân hoặc audio |

## Kết quả kích hoạt

- Hai dependency đã hoàn thành và REL-01/REL-03 đã được mở; các câu hỏi chưa chốt không được dùng để bật năng lực.
- WSA-7K2 là người duy trì và tự nghiệm thu theo D-001; D-010 là nguồn quyết định sản phẩm/phát hành.
- CT-06 có hiệu lực trong toàn bộ A/B; không diễn giải điều này thành bằng chứng runtime đã kiểm tra hoặc hai REL đã đóng.

## Lịch sử

| Ngày | Người cập nhật | Thay đổi | Quyết định/bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-06; chốt phạm vi AI/giọng nói tắt, ranh giới audio tĩnh, tự kiểm A-G04/A-G05 và điều kiện thay đổi sau A/B | D-010; REL-01/REL-03 đã mở; chưa có bằng chứng runtime |
