# Phân tích chuyên sâu M12 — Tích hợp nền tảng và tài sản số

## 1. Mục tiêu và phạm vi

M12 là ranh giới chống phụ thuộc nhà cung cấp cho toàn hệ thống. Module nhận yêu cầu có mục đích từ nghiệp vụ, chỉ gửi dữ liệu tối thiểu ra ngoài, chuẩn hóa kết quả/lỗi/độ chắc chắn, kiểm soát chi phí và cung cấp phương án suy giảm để hoạt động học cốt lõi vẫn tiếp tục khi năng lực phụ trợ gián đoạn.

### Trong phạm vi

- Danh mục năng lực ngoài, chủ sở hữu, module tiêu thụ và mức quan trọng.
- Hợp đồng dữ liệu khái niệm, trạng thái, phiên bản và định danh yêu cầu.
- Liên kết danh tính bên ngoài; AI hỗ trợ học liệu; tìm ảnh; giọng nói và phát âm.
- Lưu trữ/vòng đời hình ảnh, âm thanh và tài sản số.
- Thư điện tử, thông báo đẩy và truyền dữ liệu thời gian thực.
- Bộ nhớ chia sẻ, giới hạn lưu lượng và trạng thái phối hợp dùng chung.
- Thời hạn, thử lại, chống lặp, ngắt mạch, suy giảm và dự phòng.
- Bí mật, quyền riêng tư, đồng ý, vị trí dữ liệu, bản quyền và lưu giữ.
- Chất lượng, mức dùng, chi phí, sức khỏe, thay đổi/chuyển nhà cung cấp và playbook.

### Ngoài phạm vi

- Quyết định đăng nhập thành công, học liệu được xuất bản, phát âm đạt, thời điểm nhắc hoặc kết quả trận.
- Tự biến kết quả không chắc chắn thành quyết định nghiệp vụ cuối cùng.
- Sở hữu hồ sơ, học liệu, tiến độ, tài sản hay thông báo logic.
- Thay thế trách nhiệm quyền/phê duyệt/quan sát tổng thể của M11.

## 2. Danh mục năng lực và ảnh hưởng gián đoạn

| Năng lực | Module tiêu thụ | Mức quan trọng | Ảnh hưởng khi gián đoạn | Suy giảm tạm thời đề xuất |
|---|---|---|---|---|
| Danh tính liên kết ngoài | M01 | Cao | Người dùng phụ thuộc nhà cung cấp không đăng nhập/đăng ký được | Giữ phiên hợp lệ; cho đăng nhập nội bộ nếu tài khoản đã liên kết và chính sách cho phép |
| AI hỗ trợ tạo học liệu | M02 | Trung bình | Không tự sinh nghĩa, ví dụ và metadata | Cho nhập/biên tập thủ công; không chặn học liệu đã có |
| Tìm ảnh ngoài | M02 | Thấp | Thiếu ảnh minh họa mới | Dùng ảnh mặc định hoặc bỏ ảnh; không chặn tạo nội dung |
| Tổng hợp giọng đọc | M02, M05 | Trung bình | Không tạo âm thanh mới | Dùng âm thanh đã lưu/nguồn được duyệt hoặc hiển thị trạng thái chờ |
| Đánh giá phát âm | M05 | Cao đối với module, thấp đối với lõi học | Không chấm lượt mới | Cho luyện không chấm hoặc thử lại sau; không ảnh hưởng học/ôn cơ bản |
| Lưu trữ tài sản số | M01, M02, M05, M06, M08 | Cao | Không tải lên/đọc ảnh hoặc âm thanh mới | Giữ tài sản đã cache; dùng placeholder; chặn xuất bản nếu tài sản bắt buộc |
| Email | M01, M10 | Cao cho khôi phục tài khoản, trung bình cho nhắc | Không gửi xác minh/khôi phục/nhắc | Hộp thư trong ứng dụng cho thông tin không bảo mật; hàng chờ có hạn cho thư bắt buộc |
| Thông báo đẩy | M10 | Trung bình | Không báo ngoài ứng dụng | Hộp thư và thời gian thực khi đang trực tuyến; không gửi bù quá hạn |
| Thời gian thực | M08, M10 | Cao cho PvP, trung bình cho hộp thư | Trận/thông báo trực tiếp mất đồng bộ | PvP không bắt đầu hoặc tạm gián đoạn; hộp thư vẫn dùng cơ chế tải lại |
| Bộ nhớ/chia sẻ trạng thái | M02, M08, M11 | Cao tùy luồng | Cache miss, giới hạn phân tán/khóa phối hợp suy giảm | Về nguồn thật; áp chính sách fail-open/fail-closed riêng từng năng lực |
| Giới hạn lưu lượng | M01, M02, M05, M08 | Cao | Lạm dụng/chi phí hoặc từ chối nhầm | Giới hạn cục bộ bảo thủ; ưu tiên bảo vệ thao tác tốn phí và xác thực |

## 3. Đánh giá ngữ cảnh hiện tại

### Năng lực đã quan sát thấy

- Có liên kết đăng nhập với nhà cung cấp danh tính ngoài và lấy hồ sơ cơ bản.
- Có AI sinh metadata từ vựng theo lô, lưu tạm kết quả và giới hạn tần suất cho hoạt động tốn phí.
- Có tìm ảnh ngoài, tải ảnh lên kho tài sản và trả vị trí tham chiếu.
- Có tổng hợp giọng đọc, tải âm thanh lên kho và đánh giá phát âm qua dịch vụ giọng nói.
- Có gửi email, thông báo đẩy, truyền thông báo/trận theo thời gian thực.
- Có bộ nhớ chia sẻ và giới hạn lưu lượng phân tán với phương án cục bộ ở một số luồng.
- Có cấu hình nhà cung cấp, log lỗi và một số xử lý trả kết quả rỗng/fallback khi dịch vụ không sẵn sàng.

### Khoảng trống và rủi ro

- Chưa có sổ tích hợp thống nhất về chủ, mức quan trọng, dữ liệu, thời hạn, quota, chi phí, SLA và suy giảm.
- Kết quả rỗng thường đại diện đồng thời cho không có dữ liệu, lỗi nhà cung cấp, hết hạn hoặc hủy, khiến nghiệp vụ khó xử lý đúng.
- Chưa thấy định danh yêu cầu/đầu ra chuẩn cho thử lại; các hoạt động tạo và tải tài sản có thể phát sinh trùng hoặc ghi đè.
- Chưa có chính sách thời hạn, thử lại, ngắt mạch và giới hạn đồng thời thống nhất theo loại tích hợp.
- Bí mật/credential của nhà cung cấp có dấu hiệu tồn tại trong tệp thuộc cây dự án/đầu ra; cần kiểm kê, thu hồi nếu lộ và chuyển sang kho bí mật có vòng đời.
- Log có thể chứa authorization code, token, nội dung AI, văn bản giọng nói, URL tài sản hoặc phản hồi thô.
- Tài sản âm thanh có thể được công khai; quyền truy cập, tên tệp, ghi đè, checksum, bản quyền, lưu giữ và xóa mồ côi chưa thống nhất.
- AI trả kết quả không ổn định; thiếu độ chắc chắn, kiểm tra cấu trúc/đủ mục, phiên bản prompt/model và bắt buộc duyệt con người trước xuất bản.
- Ảnh từ nguồn ngoài cần lưu attribution/quyền sử dụng và xử lý khi nguồn gỡ hoặc điều khoản thay đổi.
- Giới hạn phân tán có xu hướng fail-open khi bộ nhớ chia sẻ lỗi; phù hợp hay không phải quyết định theo rủi ro từng luồng.
- Chưa có ngân sách theo người dùng/năng lực, dự báo chi phí, ngưỡng dừng và đối soát hóa đơn.
- Chưa có kiểm thử hợp đồng định kỳ, cảnh báo thay đổi nhà cung cấp hoặc kế hoạch chuyển đổi/dự phòng có diễn tập.

## 4. Danh sách chức năng con

| Mã | Chức năng con | Mục tiêu | Trong phạm vi | Ngoài phạm vi | Phụ thuộc |
|---|---|---|---|---|---|
| SF01 | Danh mục và hợp đồng tích hợp | Chuẩn hóa ranh giới, trạng thái và mức quan trọng | Registry, chủ, dữ liệu, SLA, lỗi, định danh, version | Quyết định nghiệp vụ cuối | M01–M11 |
| SF02 | Danh tính liên kết ngoài | Liên kết tài khoản an toàn, ít dữ liệu | Trao đổi danh tính, xác minh, lỗi, unlink, outage | Tạo tài khoản/quyền | M01, M11 |
| SF03 | AI hỗ trợ học liệu và tìm ảnh | Tạo gợi ý có nguồn, kiểm duyệt và giới hạn | Yêu cầu, kết quả, chất lượng, cache, attribution | Xuất bản học liệu | M02, M11 |
| SF04 | Giọng nói và phát âm | Chuẩn hóa TTS/chấm phát âm có độ chắc chắn | Audio/text tối thiểu, trạng thái, chất lượng, timeout | Quyết định đạt/không đạt | M02, M05 |
| SF05 | Vòng đời tài sản số | Lưu tài sản an toàn, truy vết, không mồ côi | Upload, kiểm tra, quyền, metadata, version, xóa, CDN | Nội dung nghiệp vụ sở hữu | M01, M02, M05, M06, M08 |
| SF06 | Kênh gửi và thời gian thực | Cung cấp giao nhận chuẩn hóa, phục hồi được | Email, push, realtime, trạng thái, kết nối, phản hồi | Quyết định ai/điều gì cần gửi | M01, M08, M10 |
| SF07 | Bộ nhớ chia sẻ và bảo vệ lưu lượng | Hỗ trợ mở rộng, chi phí và chống lạm dụng | Cache, khóa, quota, rate limit, fallback, isolation | Nguồn sự thật nghiệp vụ | M01, M02, M05, M08, M11 |
| SF08 | Khả năng chịu lỗi và suy giảm | Giữ hệ thống xác định khi nhà cung cấp lỗi | Timeout, retry, idempotency, circuit, fallback, replay | Sửa kết quả nghiệp vụ nguồn | M01–M11 |
| SF09 | Bí mật, riêng tư và bản quyền | Giảm rủi ro dữ liệu/credential/quyền sử dụng | Secret lifecycle, consent, minimization, residency, retention, license | Quyết định pháp lý cuối | M01, M02, M05, M10, M11 |
| SF10 | Chất lượng, chi phí và vòng đời nhà cung cấp | Quản lý SLA, ngân sách và thay đổi | Metrics, cost, contract test, version, migration, DR | Thương lượng thương mại cuối | M11 |

## 5. Ma trận module tiêu thụ

| Module | Năng lực M12 sử dụng | Yêu cầu suy giảm chính |
|---|---|---|
| M01 | Danh tính ngoài, email, ảnh đại diện, giới hạn xác thực | Giữ phiên hợp lệ; không lộ tài khoản; khôi phục qua kênh đáng tin |
| M02 | AI, ảnh ngoài, upload ảnh, TTS, cache | Cho biên tập thủ công; nội dung đã duyệt vẫn học được |
| M03 | Giới hạn lưu lượng, thời gian/quan sát dùng chung | Phiên học cốt lõi không phụ thuộc dịch vụ AI ngoài lúc học |
| M04 | Cache/quan sát/công việc nền dùng chung | Tính tiến độ từ nguồn thật khi cache lỗi |
| M05 | Đánh giá phát âm, audio, giới hạn chi phí | Luyện không chấm hoặc thử lại; không tự coi lỗi là phát âm kém |
| M06 | Tài sản hình ảnh và quan sát dùng chung | Dùng placeholder; không mất quyền sở hữu khi CDN lỗi |
| M07 | Quan sát và tín hiệu dùng chung | Tiến độ nhiệm vụ không phụ thuộc nhà cung cấp ngoài |
| M08 | Thời gian thực, cache/khóa, giới hạn, tài sản | Không bắt đầu PvP khi không đảm bảo đồng bộ; phục hồi trạng thái bền vững |
| M09 | Cache và quan sát | Bảng xếp hạng có thể chậm nhưng không tính sai |
| M10 | Email, push, realtime, thiết bị | Hộp thư là fallback; không gửi bù quá hạn/trùng |
| M11 | Health, log, cost, secret status, provider status | Quan sát độc lập; có playbook, kill switch và đối soát |

## 6. Phân tích chi tiết

### SF01 — Danh mục và hợp đồng tích hợp

**Business flow:** M12 đăng ký từng năng lực với module tiêu thụ, chủ nghiệp vụ/kỹ thuật, nhà cung cấp, dữ liệu vào/ra, mức quan trọng, thời hạn, giới hạn, chi phí, trạng thái chuẩn, suy giảm và playbook; yêu cầu từ module có mục đích/định danh; M12 kiểm tra hợp đồng; gọi nhà cung cấp; chuẩn hóa kết quả kèm phiên bản/độ chắc chắn; ghi usage/truy vết.

**Edge case:** cùng năng lực nhiều nhà cung cấp; hợp đồng đổi; module gửi dữ liệu thừa; thiếu định danh; nhà cung cấp trả thành công nhưng thiếu trường; phản hồi không rõ; yêu cầu bị hủy; kết quả đến muộn; chủ sở hữu vắng.

**DoD:** mọi tích hợp có registry đầy đủ; trạng thái phân biệt thành công/không dữ liệu/không chắc chắn/lỗi tạm/lỗi cuối/hết hạn/hủy; version và idempotency rõ; module không phụ thuộc thuật ngữ nhà cung cấp.

### SF02 — Danh tính liên kết ngoài

**Business flow:** M01 tạo yêu cầu đăng nhập có trạng thái chống giả mạo; M12 trao đổi mã một lần qua kênh bảo mật; xác minh phản hồi/nhà phát hành/đối tượng/thời hạn; chỉ lấy thuộc tính được phép; trả danh tính ngoài chuẩn hóa cho M01 quyết định liên kết/đăng nhập; token tạm không lưu/log ngoài nhu cầu; unlink/thu hồi theo chính sách.

**Edge case:** mã dùng lại/hết hạn; email chưa xác minh; cùng email nhiều danh tính; nhà cung cấp chậm; redirect không khớp; tài khoản ngoài bị thu hồi; đổi email; linking takeover; lỗi sau trao đổi token; người dùng mất phương thức duy nhất.

**DoD:** chống giả mạo/replay; thuộc tính tối thiểu và trạng thái xác minh rõ; lỗi không tiết lộ tồn tại tài khoản; bí mật/token không vào log; outage có trải nghiệm/fallback được duyệt; unlink không khóa nhầm người dùng.

### SF03 — AI hỗ trợ học liệu và tìm ảnh

**Business flow:** M02 gửi từ/ngôn ngữ/mục đích và yêu cầu đầu ra; M12 kiểm tra quota/cache; loại dữ liệu không cần; gọi AI với phiên bản model/prompt; kiểm tra cấu trúc, số lượng, ngôn ngữ, nội dung an toàn và độ chắc chắn; trả bản gợi ý; M02 bắt buộc duyệt trước xuất bản. Tìm ảnh trả kết quả cùng nguồn, attribution và quyền sử dụng.

**Edge case:** AI bịa nghĩa/ví dụ; thiếu/thừa mục; JSON sai; prompt injection trong từ; nội dung độc hại; model đổi; cache cũ; cùng từ khác nghĩa; ảnh không liên quan/nhạy cảm; nguồn gỡ ảnh; quota/chi phí hết; kết quả từng phần.

**DoD:** đầu ra gắn provider/model/prompt/version/thời điểm; kiểm tra cấu trúc/nội dung; không tự xuất bản; cache có khóa ngữ nghĩa/TTL; ảnh có attribution/license; lỗi từng phần không làm mất bản thủ công.

### SF04 — Giọng nói và phát âm

**Business flow:** module gọi gửi văn bản hoặc audio tối thiểu, ngôn ngữ/giọng và mục đích; M12 kiểm tra loại/kích thước/thời lượng/đồng ý/quota; gọi dịch vụ; chuẩn hóa âm thanh hoặc chỉ số phát âm, độ chắc chắn, cảnh báo chất lượng và phiên bản; lưu tài sản nếu cần; M02/M05 quyết định dùng/chấm; audio tạm được xóa theo hạn.

**Edge case:** audio rỗng/hỏng/quá dài; tiếng ồn; sai ngôn ngữ; trẻ em; nhà cung cấp trả điểm thiếu; timeout sau đã xử lý; cùng audio gửi lại; tên file trùng; TTS nội dung không phù hợp; giọng bị ngừng; khu vực dịch vụ lỗi.

**DoD:** loại/kích thước/thời lượng/consent kiểm tra; lỗi nhà cung cấp không thành điểm thấp; độ chắc chắn/cảnh báo trả rõ; định danh chống tính phí lặp; audio có retention; suy giảm không chặn học cơ bản.

### SF05 — Vòng đời tài sản số

**Business flow:** module nguồn yêu cầu tạo/upload với chủ sở hữu, loại, mục đích, phạm vi; M12 kiểm tra tệp, kích thước, nội dung nguy hiểm và quota; tạo tên/định danh bất biến; lưu ở vùng quyền phù hợp; ghi checksum, nguồn, license, phiên bản và tham chiếu; công bố; thay thế tạo phiên bản; xóa chỉ khi hết tham chiếu/retention; quét mồ côi.

**Edge case:** tệp giả định dạng; malware; ảnh quá lớn; tên traversal; ghi đè; upload một phần; tài sản công khai nhạy cảm; URL ký hết hạn; CDN cũ; hai module cùng tham chiếu; nguồn ngoài gỡ; xóa tài khoản; tài sản orphan.

**DoD:** whitelist loại/kích thước; quyền truy cập theo nhạy cảm; checksum/owner/source/license/version/ref count; upload retry không nhân bản; không xóa tài sản còn dùng; có placeholder/khôi phục metadata.

### SF06 — Kênh gửi và thời gian thực

**Business flow:** M01/M10/M08 gửi yêu cầu đã quyết định với người nhận, nội dung/định danh/hạn; M12 kiểm tra hợp đồng kênh/điểm nhận; giao tới email/push/realtime; ghi tiếp nhận, lỗi và phản hồi chuẩn; retry cùng định danh khi hợp lệ; điểm nhận lỗi được báo M10/M01; realtime xác thực kết nối và phục hồi qua trạng thái nghiệp vụ bền vững.

**Edge case:** gửi thành công nhưng phản hồi mất; email bounce; token thiết bị tái gán; URL không tương thích; nhiều thiết bị; kết nối cũ; broadcast nhầm group; client nhận sai thứ tự; nhà cung cấp rate limit; nội dung hết hạn; một kênh lỗi.

**DoD:** trạng thái từng kênh rõ; idempotency/retry/hạn; không log nội dung/điểm nhận thừa; xác thực người/nhóm realtime; M12 không sở hữu thông báo logic/trận; fallback do M10/M08 quyết định.

### SF07 — Bộ nhớ chia sẻ và bảo vệ lưu lượng

**Business flow:** module đăng ký mục đích cache/khóa/rate limit, key scope, TTL, độ nhất quán và chính sách lỗi; M12 thực hiện cô lập namespace/quota; cache miss về nguồn thật; khóa/rate limit dùng thời gian/quota chuẩn; khi dịch vụ chia sẻ lỗi áp fail-open/fail-closed theo rủi ro; phát health/usage/cảnh báo; phục hồi không dùng dữ liệu quá hạn.

**Edge case:** key collision; dữ liệu cá nhân trong key; cache stampede; stale data; khóa hết hạn giữa việc; split brain; nhiều instance; Redis lỗi; fail-open làm tăng chi phí/tấn công; fail-closed chặn học; clock skew; quota không công bằng.

**DoD:** mỗi use case có namespace/TTL/nguồn thật/consistency/failure mode; dữ liệu nhạy cảm không ở key; chống stampede; rate limit phân tán/cục bộ có hành vi rõ; lỗi không làm cache thành nguồn thật.

### SF08 — Khả năng chịu lỗi và suy giảm

**Business flow:** yêu cầu có timeout/hạn/idempotency; M12 giới hạn đồng thời; lỗi được phân loại; retry lỗi tạm với cùng định danh; ngắt mạch khi vượt ngưỡng; dùng cache/dự phòng/suy giảm nếu hợp lệ; hàng chờ bền vững cho công việc không đồng bộ; hồi phục thử nghiệm dần; đối soát tác dụng bên ngoài sau phản hồi mất.

**Edge case:** retry storm; provider chậm nhưng không lỗi; timeout sau tác dụng; fallback trả chất lượng khác; cả hai provider lỗi; circuit flap; queue backlog; yêu cầu hết hạn; hủy nhưng kết quả về; chuyển provider giữa yêu cầu; không idempotency phía ngoài.

**DoD:** mỗi năng lực có timeout/retry/circuit/concurrency/degradation; không retry lỗi cuối; kết quả muộn không ghi đè; tác dụng có đối soát; thử nghiệm chaos/giới hạn/chậm đạt; lõi học tiếp tục theo ma trận.

### SF09 — Bí mật, riêng tư và bản quyền

**Business flow:** M12 kiểm kê bí mật/dữ liệu/nhà cung cấp/mục đích/vị trí; bí mật lưu trong kho được kiểm soát, cấp theo danh tính workload, xoay vòng và audit; trước gửi ngoài áp data minimization/consent; hợp đồng retention/deletion; dữ liệu/log được che; nguồn AI/ảnh/audio lưu quyền sử dụng; phát hiện lộ bí mật kích hoạt thu hồi/thay và điều tra.

**Edge case:** credential trong repo/build/log; bí mật hết hạn; xoay vòng gây outage; nhà cung cấp dùng dữ liệu huấn luyện; dữ liệu qua vùng khác; xóa nhưng backup còn; ảnh thiếu attribution; người dùng rút consent khi đang xử lý; subprocessor mới; tệp công khai vô tình.

**DoD:** không bí mật trong mã/tệp phát hành/log; inventory/owner/rotation/expiry; data flow/consent/residency/retention được duyệt; quyền/license truy vết; playbook lộ bí mật được diễn tập; xóa có bằng chứng trong khả năng nhà cung cấp.

### SF10 — Chất lượng, chi phí và vòng đời nhà cung cấp

**Business flow:** M12 thu latency, availability, error, quota, chất lượng, usage và chi phí theo năng lực/module/người dùng ở mức phù hợp; so với SLO/ngân sách; cảnh báo/kill switch; chạy contract test với phiên bản provider; đánh giá thay đổi điều khoản/model/API; canary; khi thay nhà cung cấp, ánh xạ hợp đồng chuẩn, chạy song song/so sánh, chuyển dần, đối soát và đóng provider cũ.

**Edge case:** hóa đơn trễ; usage thiếu; chất lượng giảm không lỗi; provider ngừng model; breaking change; giá tăng; hai provider cho kết quả khác; lock-in tài sản; migration dở; quota chung giữa môi trường; dashboard cost chứa PII.

**DoD:** SLO/ngân sách/chủ/ngưỡng cho từng năng lực; cost allocation và dự báo; contract test/canary; thay đổi provider không buộc module nghiệp vụ đổi hợp đồng; migration có rollback, đối soát và xóa bí mật cũ.

## 7. Ma trận truy vết

| Mục tiêu | Chức năng con | Nhóm Task ID |
|---|---|---|
| Chuẩn hóa danh mục, hợp đồng và danh tính ngoài | SF01, SF02 | M12-T001–M12-T010 |
| Kiểm soát AI, ảnh, giọng nói và phát âm | SF03, SF04 | M12-T011–M12-T020 |
| Quản lý tài sản, kênh gửi và thời gian thực | SF05, SF06 | M12-T021–M12-T030 |
| Bảo vệ trạng thái dùng chung và chịu lỗi | SF07, SF08 | M12-T031–M12-T039 |
| Bảo mật, bản quyền, chất lượng và chi phí | SF09, SF10 | M12-T040–M12-T048 |
| Nghiệm thu và bàn giao | SF01–SF10 | M12-T049–M12-T050 |

## 8. Thứ tự thực hiện đề xuất

1. Chốt inventory, criticality, hợp đồng chuẩn, trạng thái lỗi và danh tính ngoài.
2. Chốt data minimization/secret trước khi mở rộng AI, ảnh, speech và tài sản.
3. Chốt idempotency/trạng thái cho asset, email, push và realtime.
4. Chốt cache/rate limit, timeout/retry/circuit/degradation và kiểm thử lỗi.
5. Chốt SLO, cost, contract testing, vendor lifecycle, nghiệm thu và playbook.

## 9. Cơ sở quyết định đã chốt

M12 có 28/28 quyết định đã chốt. Các nguyên tắc yêu cầu giảm thiểu dữ liệu gửi ra ngoài, quản lý bí mật tập trung, giới hạn chi phí và phương án suy giảm theo mức quan trọng. Lõi học với nội dung đã có phải tiếp tục hoạt động khi AI, ảnh ngoài, thông báo đẩy hoặc chấm giọng nói lỗi; module nghiệp vụ không phụ thuộc tên nhà cung cấp. Danh tính ngoài không được bỏ qua kiểm soát hay tự liên kết chỉ bằng email. Nội dung AI được xuất bản sau kiểm tra tự động tối thiểu theo quyết định cấp hệ thống, có hậu kiểm, báo cáo và gỡ nhanh. Nguồn/giấy phép ảnh được lưu khi có nhưng không bắt buộc; quản trị viên chịu trách nhiệm và hệ thống phải cảnh báo rủi ro bản quyền. Âm thanh thô được xóa sớm và không quá 24 giờ, không được nhà cung cấp dùng để huấn luyện. Tài sản có phân loại truy cập, phiên bản và vòng đời; tài sản mồ côi có thời gian chờ mặc định 30 ngày trước khi xóa có kiểm soát. Các luồng gửi chống hiệu ứng lặp, PvP suy giảm công bằng, giới hạn truy cập xử lý theo mức rủi ro, ngân sách được quản lý theo năng lực và mọi thay đổi nhà cung cấp phải kiểm chứng chất lượng, chi phí cùng khả năng quay lui.

Lỗi tạm thời chỉ được thử lại có giới hạn trên cùng định danh hoạt động; lỗi kỹ thuật không được biến thành kết quả nghiệp vụ hợp lệ. Tích hợp trọng yếu cần phương án dự phòng hoặc chế độ suy giảm đã công bố. Tài sản số phải có phân loại quyền truy cập, vòng đời và khả năng xóa; dữ liệu giọng nói thô tuân thủ giới hạn tối đa 24 giờ của M05.

## 10. Rủi ro còn hiệu lực

- Bí mật nhà cung cấp trong cây dự án, artifact hoặc log dẫn tới truy cập trái phép và chi phí/rò dữ liệu.
- Trả `không có kết quả` cho mọi loại lỗi khiến nghiệp vụ hiểu sai và ghi dữ liệu thiếu như hợp lệ.
- Retry không có định danh làm gửi thư/đẩy, tạo audio hoặc upload tài sản lặp và tăng chi phí.
- AI/ảnh ngoài thiếu kiểm duyệt, nguồn, bản quyền hoặc phiên bản làm giảm chất lượng học liệu.
- Fail-open của rate limit khi hệ thống chia sẻ lỗi có thể mở đường lạm dụng đúng lúc quan sát suy giảm.
- Tài sản công khai/không có vòng đời làm lộ audio, tồn file mồ côi và không thực hiện được xóa dữ liệu.
- Không có ngân sách/SLO/contract test khiến thay đổi giá, model hoặc API chỉ được phát hiện sau khi ảnh hưởng người dùng.

## 11. Điều kiện sẵn sàng triển khai

- M01–M11 xác nhận module nào dùng năng lực nào, dữ liệu tối thiểu, mức quan trọng và suy giảm chấp nhận được.
- M11 chốt quyền, cấu hình, audit, health, cost, kill switch và playbook.
- M01/M05/M10 chốt consent/retention cho danh tính, audio và kênh gửi; M02 chốt kiểm duyệt/bản quyền.
- Toàn bộ bí mật có inventory, chủ, trạng thái lộ, vòng xoay và nơi lưu được duyệt.
- Mỗi tích hợp trọng yếu có kiểm thử lỗi/chậm/quota, dashboard SLO/cost và kế hoạch thay thế.

Toàn bộ quyết định M12 đã được xác nhận. `QUYET-DINH-MO.md` là nguồn trạng thái và nội dung quyết định chi tiết.
