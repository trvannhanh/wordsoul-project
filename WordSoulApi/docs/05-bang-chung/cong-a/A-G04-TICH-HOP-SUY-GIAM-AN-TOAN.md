# Mẫu bằng chứng A-G04 — Tích hợp suy giảm an toàn

## 1. Hồ sơ nghiệm thu

| Trường | Nội dung |
|---|---|
| Phạm vi tích hợp hoạt động | [Điền] |
| Chủ trì cổng | [Điền] |
| Người xác nhận | [Điền] |
| REL bắt buộc | REL-03 |
| Task trọng tâm | M12-T001–M12-T010; M12-T031–M12-T038; M12-T045–M12-T047; M11-T036–M11-T037 |
| Kết luận | [Chưa đánh giá/Đạt/Không đạt/Đạt có điều kiện] |

## 2. Registry năng lực tích hợp

| Năng lực | Mục đích | Chủ | Module dùng | Nhà cung cấp | Dữ liệu tối thiểu | Mức quan trọng | Mức chịu gián đoạn | Nguồn sự thật | Hạn mức | Failure mode | Suy giảm | SLO/health | Kill switch | Evidence ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Fail-open/Fail-closed/Bảo thủ] | [Điền] | [Điền] | [Điền] | [EV] |

## 3. Taxonomy kết quả và lỗi

| Trạng thái chuẩn | Dấu hiệu provider | Có retry | Có dùng cache/fallback | Module tiêu thụ phải làm gì | Có được ghi kết quả nghiệp vụ không | Metric/cảnh báo | Evidence ID |
|---|---|---|---|---|---|---|---|
| Thành công | [Điền] | Không | [Điền] | [Điền] | Có | [Điền] | [EV] |
| Không có dữ liệu | [Điền] | [Điền] | [Điền] | [Điền] | Theo nghiệp vụ | [Điền] | [EV] |
| Không chắc chắn | [Điền] | [Điền] | [Điền] | [Điền] | Không giả thành công | [Điền] | [EV] |
| Lỗi tạm thời | [Điền] | Có giới hạn | [Điền] | [Điền] | Không | [Điền] | [EV] |
| Lỗi cuối | [Điền] | Không | [Điền] | [Điền] | Không | [Điền] | [EV] |
| Hết hạn | [Điền] | Không hoặc tạo yêu cầu mới | [Điền] | [Điền] | Không | [Điền] | [EV] |
| Bị hủy/deadline | [Điền] | Theo chính sách | [Điền] | [Điền] | Không; kết quả muộn không ghi đè | [Điền] | [EV] |

## 4. Báo cáo kiểm thử suy giảm

| Test ID | Năng lực | Tình huống lỗi | Cách tạo lỗi an toàn | Deadline | Số lần thử tối đa | Kết quả lõi mong đợi | Trạng thái dữ liệu | Cảnh báo/metric | Phục hồi mong đợi | Evidence ID | Đạt/Không đạt |
|---|---|---|---|---|---|---|---|---|---|---|---|
| G04-R01 | [Điền] | Timeout/chậm | [Điền] | [Điền] | [Điền] | [Điền] | Không sai lệch | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R02 | [Điền] | 429/hết quota | [Điền] | [Điền] | [Điền] | [Điền] | Không ghi lặp | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R03 | [Điền] | 5xx/outage | [Điền] | [Điền] | [Điền] | [Điền] | Nhất quán | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R04 | [Điền] | Response sai/rỗng | [Điền] | [Điền] | [Điền] | Không giả thành công | Không đổi nguồn sự thật | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R05 | [Điền] | Redis/shared state lỗi | [Điền] | [Điền] | [Điền] | Failure mode đúng năng lực | Không mất khóa/ghi đôi | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R06 | [Điền] | Mất lock/lease | [Điền] | [Điền] | [Điền] | Tác dụng nguy hiểm dừng | Không tạo hai kết quả | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R07 | [Điền] | Circuit mở/half-open | [Điền] | [Điền] | [Điền] | Cô lập đúng phạm vi | Không lan lỗi | [Điền] | [Điền] | [EV] | [Điền] |
| G04-R08 | [Điền] | Phục hồi provider | [Điền] | [Điền] | [Điền] | Thăm dò và khôi phục có kiểm soát | Đối soát đạt | [Điền] | [Điền] | [EV] | [Điền] |

## 5. Điều kiện đạt

- Mọi năng lực đang hoạt động có chủ, mục đích, dữ liệu tối thiểu, mức quan trọng, hạn mức và suy giảm.
- Không dùng null/rỗng hoặc lỗi provider làm kết quả nghiệp vụ hợp lệ.
- Không có đường bỏ qua giới hạn lưu lượng theo dấu hiệu yêu cầu.
- Auth, chi phí và gian lận không mặc định allow-all khi limiter/shared state lỗi.
- Deadline, retry, idempotency, circuit và bulkhead có bằng chứng cho phạm vi cần thiết.
- REL-03 đạt và mọi test suy giảm trọng yếu đều đạt.
