# Từ điển chỉ số quản trị M11

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M11-T022 |
| Phiên bản | 0.1-draft |
| Trạng thái | Bản nháp khung — chờ M11-T001 và từng chủ nguồn xác nhận công thức |
| Cơ sở quyết định | M11-D014–D016: health/sự cố/bảo mật/nguy cơ lệch tài sản mục tiêu trong 1 phút; chỉ số khác theo giờ/ngày; lưu/đối soát UTC, hiển thị một múi giờ được công bố; xuất mặc định tổng hợp/đã che |
| Chủ catalog | M11; module nguồn sở hữu sự kiện, công thức miền và chất lượng nguồn |

## Schema bắt buộc

| Trường | Yêu cầu |
|---|---|
| Metric ID/tên hiển thị | ID ổn định; không tái dùng cùng tên cho công thức khác |
| Câu hỏi quyết định | Nêu quyết định/cảnh báo mà chỉ số phục vụ; không thu thập chỉ vì có dữ liệu |
| Công thức | Tử số, mẫu số, tập đủ điều kiện, loại trừ và quy tắc dữ liệu đến muộn |
| Nguồn/chủ | Event/table/aggregate có phiên bản và một module chịu trách nhiệm |
| Cửa sổ/thời gian | Event time hay processing time; lưu theo UTC; timezone hiển thị công bố |
| Chiều phân tích | Allowlist chiều và cardinality; cấm chiều làm lộ cá nhân ngoài quyền |
| Freshness/SLO | Mục tiêu độ mới, mốc đo và hành vi khi stale |
| Quality/giới hạn | Missing/duplicate/outlier/reconciliation và điều không được suy luận |
| Quyền/xuất | Vai trò xem, mức tổng hợp/che, purpose/case khi cần chi tiết |
| Version | Phiên bản công thức và ngày hiệu lực; dashboard ghi đúng version |

## Seed dictionary cần chủ nguồn xác nhận

| Metric ID | Tên/câu hỏi | Công thức draft | Nguồn/chủ dự kiến | Cửa sổ | Freshness mục tiêu | Giới hạn/quyền | Owner status |
|---|---|---|---|---|---|---|---|
| MET-OPS-HEALTHY-CAP | Tỷ lệ năng lực khỏe | Số năng lực trọng yếu đạt toàn bộ điều kiện health / số năng lực trọng yếu được kỳ vọng | M11/M12 + chủ năng lực | Ảnh hiện tại theo UTC | ≤1 phút | Không thay thế kiểm tra dependency chi tiết; vận hành | Chưa xác nhận |
| MET-SEC-AUTH-ANOMALY | Lượt xác thực bất thường | Số sự kiện xác thực được rule version phân loại bất thường; không đếm retry trùng correlation | M01/M12 | Trượt ngắn, UTC | ≤1 phút | Không hiển thị email/IP thô; an toàn/vụ việc | Chưa xác nhận |
| MET-M06-RECON-DIFF | Nguy cơ sai lệch tài sản | Tổng số mutation/đối tượng lệch theo reconciliation version, tách chưa xử lý/đã xử lý | M06/M11 | Ảnh + trượt, UTC | ≤1 phút | Không dùng tổng tiền không cùng đơn vị; kinh tế/vận hành | Chưa xác nhận |
| MET-INC-OPEN | Sự cố đang mở theo cấp | Số incident chưa ở trạng thái đóng, nhóm theo severity version | M11 | Ảnh hiện tại | ≤1 phút | Công bố severity/timezone; vận hành | Chưa xác nhận |
| MET-M01-REG-COMPLETE | Tỷ lệ đăng ký hoàn tất | Số registration attempt đủ điều kiện đạt trạng thái hoàn tất / số attempt đủ điều kiện bắt đầu; dedupe theo attempt ID | M01 | Giờ/ngày UTC | Theo giờ | Loại traffic test/bot theo rule version; sản phẩm | Chưa xác nhận |
| MET-M01-VERIFY-LAG | Thời gian xác minh thư | Phân phối thời gian từ bằng chứng gửi thành công đến xác minh hợp lệ cho cohort đủ điều kiện | M01/M10 | Cohort ngày UTC | Theo giờ/ngày | Không suy ra thời gian khi gửi phụ thuộc lỗi; tổng hợp | Chưa xác nhận |
| MET-M02-CONTENT-READY | Nội dung sẵn sàng | Số content version đạt toàn bộ quality gate / số version gửi rà soát trong kỳ | M02 | Ngày UTC | Theo ngày | Gắn quality/version; nội dung/sản phẩm | Chưa xác nhận |
| MET-M03-LEARNING-COMPLETE | Tỷ lệ phiên học hoàn tất | Số phiên đủ điều kiện hoàn tất / số phiên đủ điều kiện bắt đầu; định nghĩa timeout rõ | M03 | Giờ/ngày UTC | Theo giờ | Không đồng nhất hoàn tất với hiệu quả học; sản phẩm/học thuật | Chưa xác nhận |
| MET-M04-REVIEW-DUE | Khối lượng ôn đến hạn | Số mục ôn đến hạn chưa hoàn tất tại snapshot, theo algorithm version | M04 | Snapshot UTC | Theo giờ | Phải công bố timezone hiển thị/độ trễ; học thuật | Chưa xác nhận |
| MET-M08-MATCH-COMPLETE | Tỷ lệ trận kết thúc hợp lệ | Số trận có kết quả hợp lệ / số trận đủ điều kiện bắt đầu, theo match version | M08 | Giờ/ngày UTC | Theo giờ | Tách hủy/lỗi/timeout; vận hành/sản phẩm | Chưa xác nhận |
| MET-M10-DELIVERY | Tỷ lệ giao thông báo | Số delivery terminal thành công / số delivery attempt đủ điều kiện, tách provider/channel | M10/M12 | Giờ/ngày UTC | Theo giờ | Không coi accepted là delivered nếu provider không xác nhận; vận hành | Chưa xác nhận |
| MET-JOB-SUCCESS | Tỷ lệ run công việc đạt | Số run terminal thành công / số run terminal trong kỳ, tách job/version và retry | M11 + module owner | Giờ/ngày UTC | Theo giờ | Run đang chạy không vào mẫu số; vận hành | Chưa xác nhận |

## Quy tắc phiên bản, chất lượng và riêng tư

- Thay đổi tử số, mẫu số, tập đủ điều kiện, nguồn hoặc cửa sổ tạo metric version mới; không ghi đè lịch sử âm thầm.
- Dashboard hiển thị `as of`, freshness target, timezone, metric version và trạng thái chất lượng; stale/partial không hiển thị như chắc chắn.
- Backfill/đính chính ghi processing time, affected window, reason và phiên bản; báo cáo đã chốt phải có chính sách mở lại.
- Chiều có nguy cơ nhận diện dùng ngưỡng tổng hợp/che và quyền; xuất chi tiết chỉ theo vai trò, vụ việc, mục đích, phạm vi, thời hạn và audit.
- Không dùng metric tổng hợp làm nguồn quyết định tài khoản/tài sản nếu không có đường truy vết về nguồn sự thật.

## Khoảng trống phải đóng

| Mã | Khoảng trống | Điều kiện đóng |
|---|---|---|
| M11-MET-O01 | Công thức seed chưa có event/table/version vật lý | Mỗi chủ nguồn xác nhận data contract |
| M11-MET-O02 | Freshness theo giờ/ngày chưa có SLO số cho từng metric | M11-T023 và năng lực nguồn xác nhận |
| M11-MET-O03 | Chưa có ngưỡng quality/cảnh báo | M11-T024; chủ nguồn/vận hành xác nhận |
| M11-MET-O04 | Chưa có ma trận vai trò/chiều/xuất chi tiết | M11-T004/M11-T025; REL-02/07 |

## Điều kiện duyệt M11-T022

- Mỗi metric có tử/mẫu, nguồn, cửa sổ, timezone, freshness, giới hạn, owner và version.
- Không có hai công thức dùng cùng ID/tên; metric đổi nghĩa tạo version mới.
- Chủ M01–M10/M12 xác nhận seed thuộc mình; M11 không tự diễn giải luật miền.
- M11-MET-O01–O04 được đóng hoặc chuyển thành finding/task có chủ.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Reviewer |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo schema metric và seed 12 chỉ số danh tính, học liệu, học tập, vận hành, tài sản, trận, thông báo và job | Chưa gán |
