# Mẫu bằng chứng A-G05 — Bí mật và dữ liệu ngoài

## 1. Hồ sơ nghiệm thu

| Trường | Nội dung |
|---|---|
| Phạm vi phát hành | [Điền] |
| Chủ trì cổng | [Điền] |
| Người xác nhận | [Điền] |
| REL bắt buộc | REL-03; REL-04; REL-01 khi có dữ liệu tuổi/đồng ý |
| Task trọng tâm | M12-T040–M12-T044; M11-T031–M11-T035; M02-T011–M02-T014 |
| Kết luận | [Chưa đánh giá/Đạt/Không đạt/Đạt có điều kiện] |

## 2. Inventory bí mật — không ghi giá trị

| Secret ID | Năng lực/provider | Mục đích | Chủ | Vị trí lưu loại nào | Workload được phép | Môi trường | Hạn/chu kỳ xoay | Mức phơi lộ | Hành động khẩn | Bằng chứng thu hồi/xoay | Audit truy cập | Trạng thái |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [SEC-A-001] | [Điền] | [Điền] | [Điền] | [Chỉ mô tả loại kho, không ghi bí mật] | [Điền] | [Điền] | [Điền] | [Không rõ/Nghi lộ/Được kiểm soát] | [Điền] | [EV] | [EV] | [Điền] |

## 3. Bản đồ dữ liệu rời hệ thống

| Flow ID | Module nguồn | Trường/nhóm dữ liệu | Mục đích | Provider/nơi xử lý | Căn cứ/đồng ý | Tối thiểu hóa | Vị trí/khu vực | Lưu giữ | Xóa/thu hồi | Subprocessor | Log được phép | Chủ phê duyệt | Evidence ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [DF-A-001] | [Điền] | [Không dùng dữ liệu thật] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Điền] | [Metadata allowlist] | [Điền] | [EV] |

## 4. Registry quyền và nguồn tài sản

| Asset ID/loại | Module sở hữu | Nguồn | Chủ thể quyền | License/điều khoản | Attribution | Phạm vi dùng | Trạng thái bằng chứng | Mức rủi ro chấp nhận | Đầu mối khiếu nại | SLA tạm ẩn/gỡ | Evidence ID |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [Điền] | [Điền] | [Điền] | [Điền] | [Điền hoặc Chưa có] | [Điền] | [Điền] | [Đủ/Thiếu/Không rõ] | [Theo REL-04] | [Điền] | [Điền] | [EV] |

## 5. Kiểm thử che dữ liệu và log

| Case ID | Nguồn log | Dữ liệu giả được đưa vào | Metadata được phép còn lại | Dữ liệu bắt buộc bị che/loại | Kết quả thực tế | Evidence ID | Đạt/Không đạt |
|---|---|---|---|---|---|---|---|
| G05-L01 | Đăng ký/đăng nhập | Email/token giả | Correlation, loại sự kiện, kết quả | Mật khẩu, token, mã xác minh, email đầy đủ | [Điền] | [EV] | [Điền] |
| G05-L02 | OAuth callback | Code/state/token giả | Provider, status, correlation | Code, state nhạy cảm, token, redirect payload | [Điền] | [EV] | [Điền] |
| G05-L03 | Tích hợp ngoài lỗi | Response giả chứa PII/secret | Provider, status, error category | Body/response thô và secret | [Điền] | [EV] | [Điền] |
| G05-L04 | Upload/tài sản | Tên tệp/dữ liệu giả | Asset ID, loại, kết quả | Nội dung tệp, URL riêng tư có hạn, metadata nhạy cảm | [Điền] | [EV] | [Điền] |
| G05-L05 | Xuất/xóa dữ liệu | Hồ sơ giả | Request ID, trạng thái, số lượng tổng hợp | Nội dung xuất và dữ liệu chủ thể | [Điền] | [EV] | [Điền] |

## 6. Điều kiện đạt

- Mọi bí mật có chủ, vị trí lưu kiểm soát, quyền tối thiểu, hạn và bằng chứng xoay vòng/thu hồi khi cần.
- Không có giá trị bí mật trong tài liệu, source artifact, log hoặc bằng chứng.
- Mọi dữ liệu rời hệ thống có mục đích, tối thiểu hóa, lưu giữ/xóa và chủ phê duyệt.
- AI/giọng nói được ghi rõ “tắt” trong phạm vi A/B nếu chưa được phép hoạt động.
- Log dùng allowlist metadata và vượt toàn bộ kiểm thử redaction.
- Tài sản thiếu quyền chỉ được dùng đúng phạm vi đã được REL-04 chấp nhận và có quy trình gỡ nhanh.
