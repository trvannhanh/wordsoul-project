# CT-04 — Không dùng health giả định

| Trường | Nội dung khởi tạo |
|---|---|
| Task | A0-T009 |
| Trạng thái | Có hiệu lực từ 2026-08-18; health registry và bằng chứng runtime chờ các task liên quan |
| Chủ trì / tự xác nhận | WSA-7K2 |
| Cá nhân thực tế | WSA-7K2 |
| Phạm vi áp dụng | Mọi quyết định phát hành/tiếp tục vận hành và mọi năng lực/phụ thuộc trọng yếu trong Giai đoạn A |
| Rủi ro kiểm soát | Phát hành hoặc tiếp tục vận hành dựa trên tín hiệu khỏe không kiểm tra phụ thuộc thật |

## Quy tắc tạm thời

- Điểm health hiện tại không đủ làm bằng chứng dịch vụ khỏe hoặc sẵn sàng phát hành.
- Năng lực trọng yếu phải có bước xác minh thủ công tạm thời và người ghi nhận kết quả.
- Khi chưa xác minh được phụ thuộc, trạng thái phải là chưa xác định hoặc suy giảm, không mặc định khỏe.
- Kết luận phát hành phải tham chiếu bằng chứng bổ sung ngoài điểm health hiện tại.
- Liveness chỉ chứng minh tiến trình phản hồi; không tự chứng minh readiness, correctness, freshness, quota hay khả năng phục vụ hành trình người dùng.
- Tín hiệu quá hạn, thiếu nguồn, bị che lỗi hoặc dùng giá trị mặc định phải được coi là `unknown/degraded`, không được chuyển thành `healthy`.

## Bằng chứng health tối thiểu

| Thành phần | Yêu cầu để dùng trong kết luận phát hành | Khi thiếu/không chắc chắn |
|---|---|---|
| Năng lực và chủ sở hữu | Tên năng lực, module/chủ, mức quan trọng và hành trình người dùng bị ảnh hưởng | Không kết luận phạm vi đó khỏe |
| SLI/SLO | Availability, latency, correctness, freshness hoặc quota phù hợp với năng lực; có cửa sổ đo | Ghi `unknown` hoặc `degraded` |
| Nguồn dữ liệu thật | Metric/probe/đối soát từ workload và phụ thuộc thực, không phải hằng số hay endpoint giả | Không dùng làm bằng chứng phát hành |
| Độ mới | Thời điểm đo, timezone/cửa sổ và ngưỡng stale rõ ràng | Tín hiệu stale bị loại |
| Phụ thuộc | Trạng thái SQL/Redis/provider/job/asset store liên quan và failure mode đã chốt | Suy giảm hoặc dừng theo năng lực |
| Cảnh báo và playbook | Ngưỡng, chủ xử lý, đường escalation, hành động suy giảm/kill switch và cách phục hồi | Không chấp nhận phát hành năng lực trọng yếu |
| Kiểm thử suy giảm | Timeout, quota, outage, shared state, circuit và phục hồi có kết quả gần nhất | Ghi finding; không suy diễn từ happy path |

## Tự kiểm A-G04/A-G06

| Phạm vi kiểm tra | Kết quả yêu cầu | Trạng thái |
|---|---|---|
| Registry A-G04 | Mỗi năng lực có chủ, criticality, phụ thuộc, SLO/health thật, suy giảm và kill switch | Tiêu chí đã ghi; chờ registry/runtime |
| G04-R01, G04-R02 và G04-R03 | Timeout, quota và outage không trả khỏe/thành công giả; dữ liệu vẫn nhất quán | Tiêu chí đã ghi; chờ bằng chứng runtime |
| G04-R05, G04-R06, G04-R07 và G04-R08 | Shared state, lock, circuit và phục hồi phản ánh đúng trạng thái degraded/recovery | Tiêu chí đã ghi; chờ bằng chứng runtime |
| Sổ sức khỏe A-G06 | Có SLI/SLO, error budget, độ mới, degraded, cảnh báo, mục tiêu phản hồi/khôi phục và playbook | Tiêu chí đã ghi; chờ M11-T036/M12-T045 |
| G06-E02 và G06-E03 | Gián đoạn danh tính và tích hợp chậm được phát hiện bằng tín hiệu thật, không fail-open | Tiêu chí đã ghi; chờ diễn tập |

## Duy trì, kiểm tra và gỡ bỏ

| Mục | Yêu cầu |
|---|---|
| Cá nhân duy trì / tự xác nhận | WSA-7K2 / WSA-7K2 theo D-001 |
| Bằng chứng định kỳ | Danh sách năng lực, phụ thuộc, cách kiểm tra thủ công, thời điểm và người xác nhận |
| Nhịp kiểm tra | Trước mỗi kết luận phát hành; khi đổi health/probe/phụ thuộc; và sau sự cố hoặc tín hiệu stale/sai |
| Khi vi phạm | Thu hồi kết luận phát hành, đánh giá lại phạm vi và ghi nhận sự cố nếu gây ảnh hưởng |
| Điều kiện gỡ | M11-T036 và M12-T045 đạt |
| Thẩm quyền gỡ | WSA-7K2 ghi nhận bằng cập nhật task/quyết định phù hợp sau khi điều kiện gỡ và A-G04/A-G06 đạt |

## Kết quả kích hoạt và điều kiện gỡ

- WSA-7K2 là người duy trì và tự nghiệm thu theo D-001; phạm vi, bằng chứng tối thiểu và hành vi `unknown/degraded` đã được ghi nhận.
- CT-04 có hiệu lực ở mức guardrail phát hành; không diễn giải điều này thành health registry hoặc runtime hiện tại đã đạt.
- Chỉ gỡ sau khi M11-T036, M12-T045 và các kiểm thử A-G04/A-G06 liên quan đạt, không còn finding nghiêm trọng/rất cao về health giả định hoặc phụ thuộc không được quan sát.

## Lịch sử

| Ngày | Người cập nhật | Thay đổi | Bằng chứng |
|---|---|---|---|
| 2026-08-18 | WSA-7K2 | Kích hoạt CT-04 theo workflow một người; chốt bằng chứng health tối thiểu, trạng thái unknown/degraded và tự kiểm A-G04/A-G06 | D-001; REL-03 đã mở; chưa có bằng chứng runtime |
