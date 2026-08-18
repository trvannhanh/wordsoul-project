# Đánh giá hiện trạng A-WP04 — M12 Tích hợp nền tảng và tài sản số

## 1. Phạm vi và phương pháp

Tài liệu đối chiếu 31 task được chọn cho A-WP04 với mã nguồn, cấu hình, dữ liệu và kiểm thử hiện có tại ngày 2026-08-14. Phạm vi gồm M12-T001–M12-T010, M12-T021–M12-T025, M12-T031–M12-T038 và M12-T040–M12-T047.

Đây là đánh giá tĩnh, chỉ đọc. Không đọc hoặc ghi lại giá trị bí mật. Trạng thái phản ánh mức đáp ứng toàn bộ Definition of Done của task, không chỉ việc đã kết nối được một nhà cung cấp.

| Trạng thái | Ý nghĩa |
|---|---|
| Đã đáp ứng | Có đủ hành vi, kiểm soát, bằng chứng và kiểm thử theo Definition of Done |
| Đáp ứng một phần | Có nền tảng nhưng thiếu một hoặc nhiều kiểm soát/nghiệm thu bắt buộc |
| Chưa có | Không tìm thấy bằng chứng đủ để xác nhận năng lực |
| Không còn phù hợp | Task mâu thuẫn với quyết định đã chốt và phải loại hoặc viết lại phạm vi có lý do |

## 2. Kết quả tổng hợp

| Kết quả | Số task | Tỷ lệ |
|---|---:|---:|
| Đã đáp ứng | 0 | 0% |
| Đáp ứng một phần | 19 | 61,3% |
| Chưa có | 12 | 38,7% |
| Không còn phù hợp | 0 | 0% |
| Tổng | 31 | 100% |

Hệ thống đã có các kết nối thực tế cho đăng nhập ngoài, tạo học liệu bằng AI, tìm ảnh, tổng hợp/chấm giọng nói, lưu ảnh, thư điện tử, thông báo đẩy, trạng thái thời gian thực, Redis và giới hạn lưu lượng. Tuy nhiên, các kết nối chủ yếu được quản lý riêng lẻ. Chưa có lớp quản trị chung về danh mục năng lực, hợp đồng trung lập nhà cung cấp, vòng đời tài sản, bí mật, dữ liệu rời hệ thống, bản quyền, mức dịch vụ, chi phí và thay thế nhà cung cấp.

## 3. Bằng chứng hiện trạng nổi bật

- Các tích hợp chính được đăng ký tập trung khi khởi động, nhưng chưa có sổ đăng ký nghiệp vụ ghi chủ sở hữu, mục đích, dữ liệu, mức quan trọng và phương án suy giảm tại [Program.cs](../../../WordSoul.Api/Program.cs:238).
- Luồng đăng nhập Google kiểm tra trạng thái xác minh email, nhưng tự ghép tài khoản theo email hiện có tại [AuthService.cs](../../../WordSoul.Application/Services/AuthService.cs:119), trái quyết định không tự liên kết chỉ dựa trên email.
- Tham số trạng thái đăng nhập ngoài được chuyển đi và tách khi quay về nhưng không được đối chiếu với trạng thái phiên đã phát hành; token nội bộ sau đó được đặt trong địa chỉ chuyển hướng tại [AuthController.cs](../../../WordSoul.Api/Controllers/AuthController.cs:68).
- Mã truy cập Google chỉ được dùng ngắn hạn để lấy thông tin tối thiểu, nhưng chưa thấy chính sách vòng đời token ngoài, thu hồi, xoay vòng và bằng chứng không rò rỉ tại [GoogleOAuthService.cs](../../../WordSoul.Infrastructure/ExternalServices/GoogleOAuthService.cs:32).
- Tải ảnh trả về URL và định danh của nhà cung cấp; kiểm tra kích thước/loại chỉ xuất hiện ở một số luồng, chưa có kiểm tra nội dung, mã kiểm tra, quota, chủ sở hữu hoặc trạng thái tải một phần tại [UploadAssetsService.cs](../../../WordSoul.Application/Services/UploadAssetsService.cs:21) và [VocabularySetController.cs](../../../WordSoul.Api/Controllers/VocabularySetController.cs:67).
- Một luồng tạo bộ từ đã để lại phần hoàn tác ảnh ở trạng thái vô hiệu hóa, cho thấy nguy cơ tài sản mồ côi khi nghiệp vụ sau tải lên thất bại tại [VocabularySetController.cs](../../../WordSoul.Api/Controllers/VocabularySetController.cs:103).
- Cache AI có khóa riêng, thời hạn bảy ngày và suy giảm khi Redis lỗi, nhưng chưa có danh mục trạng thái dùng chung, quy tắc môi trường/phiên bản, chống dồn tải hoặc kiểm thử vô hiệu hóa tại [VocabularyAiCacheService.cs](../../../WordSoul.Infrastructure/ExternalServices/VocabularyAiCacheService.cs:10).
- Bộ giới hạn Redis dùng thao tác nguyên tử và có thời hạn, nhưng mặc định cho qua mọi yêu cầu khi Redis lỗi tại [RedisRateLimiter.cs](../../../WordSoul.Infrastructure/RateLimiting/RedisRateLimiter.cs:130).
- Nhiều chính sách giới hạn lưu lượng tồn tại và trả thời gian thử lại, nhưng tiến trình nội bộ có thể bỏ qua toàn bộ giới hạn chỉ bằng dấu hiệu yêu cầu và địa chỉ vòng lặp tại [RateLimitingExtensions.cs](../../../WordSoul.Api/Extensions/RateLimitingExtensions.cs:104), trái M12-D023.
- Nhiều lời gọi ngoài nhận tín hiệu hủy, nhưng chưa thấy ngân sách thời gian nghiệp vụ, chính sách thử lại thống nhất, circuit breaker thật hoặc cô lập tài nguyên giữa các nhà cung cấp.
- Bí mật được lấy trực tiếp từ cấu hình ở nhiều tích hợp. Chưa thấy sổ kiểm kê, chủ sở hữu, hạn dùng, xoay vòng, thu hồi và kiểm toán truy cập; tài liệu này không kiểm tra giá trị bí mật.
- Middleware lưu request/response tối đa 4.000 ký tự mà không có danh sách metadata cho phép hoặc che dữ liệu tại [RequestResponseLoggingMiddleware.cs](../../../WordSoul.Api/Middlewares/RequestResponseLoggingMiddleware.cs:99). Một số tích hợp còn ghi nội dung lỗi hoặc phản hồi thô của nhà cung cấp.
- Hai điểm sức khỏe công khai luôn trả “healthy”, không kiểm tra nhà cung cấp, Redis, kho tài sản hoặc độ mới dữ liệu tại [Program.cs](../../../WordSoul.Api/Program.cs:427).

## 4. Ma trận đánh giá 31 task

### 4.1. Nền tảng tích hợp và danh tính ngoài

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M12-T001 | Đáp ứng một phần | Tài liệu M12 đã mô tả năng lực, kết quả, lỗi, suy giảm và fallback | Chưa có từ điển được duyệt dùng thống nhất giữa mọi module và chủ sở hữu từng quyết định | Trung bình |
| M12-T002 | Đáp ứng một phần | Có kết nối OAuth, AI, ảnh, speech, asset, email, push, realtime, cache và giới hạn lưu lượng | Chưa có sổ đăng ký gắn từng năng lực với nhà cung cấp, module dùng, chủ, mục đích, dữ liệu, môi trường và trạng thái | Rất cao |
| M12-T003 | Đáp ứng một phần | Một số dịch vụ tự suy giảm về rỗng/cache miss hoặc cho qua khi Redis lỗi | Chưa phân loại mức quan trọng, tác động hành trình, mức chịu gián đoạn, mục tiêu phục hồi và suy giảm được duyệt | Rất cao |
| M12-T004 | Đáp ứng một phần | Có ranh giới trao đổi dữ liệu riêng cho từng tích hợp | Hợp đồng còn gắn đặc thù nhà cung cấp; thiếu mục đích, phiên bản, định danh chống lặp, deadline và correlation thống nhất | Cao |
| M12-T005 | Đáp ứng một phần | Có xử lý một số trạng thái HTTP, rỗng, hủy và ngoại lệ | Nhiều nơi dùng giá trị rỗng hoặc ngoại lệ chung; chưa tách không dữ liệu, không chắc chắn, lỗi tạm/cuối, hết hạn và hủy | Rất cao |
| M12-T006 | Đáp ứng một phần | Google trả định danh, thư, tên, ảnh và trạng thái xác minh; luồng yêu cầu email đã xác minh | Chưa có hợp đồng trường được duyệt, mục đích/lưu giữ từng trường và hành vi thống nhất khi trường thiếu | Cao |
| M12-T007 | Đáp ứng một phần | Mã đăng nhập được đổi trực tiếp với provider | Trạng thái chống giả mạo không được ràng buộc với phiên; chưa chứng minh chống phát lại/redirect sai; token nội bộ xuất hiện trong địa chỉ chuyển hướng | Nghiêm trọng |
| M12-T008 | Đáp ứng một phần | Token ngoài được dùng ngắn hạn để lấy hồ sơ và không thấy lưu lâu dài | Chưa có chính sách nhận–hết hạn–thu hồi–xoay vòng, che log và bằng chứng lưu tối thiểu | Rất cao |
| M12-T009 | Chưa có | Có quan hệ tài khoản ngoài và ràng buộc định danh provider | Luồng hiện tại tự liên kết theo email; thiếu xác minh lại, ngắt liên kết an toàn, bảo vệ phương thức cuối và kịch bản takeover | Nghiêm trọng |
| M12-T010 | Đáp ứng một phần | Lỗi provider không làm mất các phiên nội bộ đã cấp | Chưa có playbook outage, fallback theo phương thức đã liên kết, trạng thái degraded, cảnh báo và chủ SLO; không được fail-open xác thực | Rất cao |

### 4.2. Kho tài sản số

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M12-T021 | Chưa có | Có các thư mục sử dụng cho avatar, từ vựng, bộ từ, pet và item; audio được lưu ở dịch vụ riêng | Chưa có asset catalog với chủ, định dạng, kích thước, độ nhạy, quyền, lưu giữ, bản quyền, placeholder và nơi lưu | Rất cao |
| M12-T022 | Đáp ứng một phần | Một số luồng ảnh kiểm tra có tệp, giới hạn 10 MB và loại bắt đầu bằng image | Kiểm tra không đồng nhất; thiếu kiểm tra nội dung thật, malware, quota, owner/purpose, request ID và xử lý tải một phần | Nghiêm trọng |
| M12-T023 | Chưa có | Lời gọi upload nhận URL và định danh nhà cung cấp | Nội dung nghiệp vụ chủ yếu giữ URL; chưa có metadata bất biến, checksum, owner, nguồn, license, phiên bản, trạng thái và chống trùng | Rất cao |
| M12-T024 | Đáp ứng một phần | Ảnh được phát qua URL bảo mật của nhà cung cấp | Chưa phân loại public/private, URL có hạn, thu hồi quyền, invalidation CDN và bảo vệ không chỉ dựa trên việc biết URL | Rất cao |
| M12-T025 | Chưa có | Có thay URL ảnh khi cập nhật nội dung | Chưa có phiên bản hóa, kiểm tra tham chiếu, xóa có grace/audit, dọn orphan và placeholder; hoàn tác upload đang bị vô hiệu hóa ở một luồng | Rất cao |

### 4.3. Trạng thái dùng chung, giới hạn và khả năng chống lỗi

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M12-T031 | Đáp ứng một phần | Redis được dùng cho cache AI và giới hạn lưu lượng; có trạng thái thời gian thực ở các luồng khác | Chưa có registry theo use case với nguồn sự thật, namespace, TTL, consistency, quota, mức quan trọng và failure mode | Rất cao |
| M12-T032 | Đáp ứng một phần | Cache AI có tiền tố khóa và TTL; lỗi Redis quay về nguồn ngoài | Chưa chuẩn hóa môi trường/module/phiên bản, invalidation, chống stampede, quy tắc không PII trong khóa và kiểm thử | Cao |
| M12-T033 | Chưa có | Không tìm thấy năng lực khóa phân tán được quản trị trong phạm vi A-WP04 | Thiếu owner, lease, renew, release, fencing và hành vi dừng tác dụng khi mất khóa | Nghiêm trọng |
| M12-T034 | Đáp ứng một phần | Có chính sách cho global, auth, AI, audio, ghép trận và gym; có Retry-After | Chưa bao phủ upload, notification/admin và chi phí; chưa chốt công bằng theo IP/user/device, proxy tin cậy và trusted caller | Rất cao |
| M12-T035 | Đáp ứng một phần | Có hành vi fail-open rõ khi Redis lỗi | Mọi lỗi limiter đều allow-all và tiến trình nội bộ bỏ qua giới hạn; chưa có ma trận fail-open/fail-closed, giới hạn cục bộ và cảnh báo theo năng lực | Nghiêm trọng |
| M12-T036 | Đáp ứng một phần | Nhiều tích hợp truyền tín hiệu hủy; tìm ảnh có giới hạn chờ cục bộ | Chưa có ngân sách thời gian cho từng năng lực, deadline xuyên luồng, chống kết quả muộn ghi đè và metric timeout | Rất cao |
| M12-T037 | Đáp ứng một phần | Có một số thử lại cục bộ ngoài phạm vi nhà cung cấp và một số nghiệp vụ có chống lặp | Chưa có taxonomy lỗi được thử lại, backoff/jitter, attempt cap, cùng request ID và đối soát khi không idempotent | Nghiêm trọng |
| M12-T038 | Chưa có | Chú thích “circuit-breaker fallback” chỉ mô tả cho qua khi Redis lỗi | Không có trạng thái đóng/mở/thăm dò, ngưỡng, bulkhead hoặc giới hạn đồng thời để cô lập lỗi và chi phí | Nghiêm trọng |

### 4.4. Bí mật, dữ liệu ngoài, bản quyền, mức dịch vụ và chi phí

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M12-T040 | Chưa có | Có nhiều khóa cấu hình cho provider và connection | Chưa có inventory ghi owner, vị trí, mức phơi lộ, hạn, xoay vòng và mức khẩn; chưa có playbook thu hồi bí mật nghi lộ | Nghiêm trọng |
| M12-T041 | Chưa có | Tích hợp lấy credential trực tiếp từ cấu hình | Chưa chứng minh kho bí mật tập trung, quyền tối thiểu theo workload, rotation không gián đoạn, revoke và audit truy cập | Nghiêm trọng |
| M12-T042 | Đáp ứng một phần | Có thể suy ra dữ liệu gửi cho Google, Gemini, Unsplash, Azure, Cloudinary, SendGrid và FCM từ từng luồng | Chưa có bản đồ được duyệt về trường, mục đích, nơi xử lý, lưu giữ, đồng ý, xóa và đơn vị phụ xử lý. Trong A/B chỉ lập bản đồ danh tính, tài sản và kênh đang hoạt động; AI/speech ở mức chính sách và mặc định tắt | Nghiêm trọng |
| M12-T043 | Chưa có | Log có trạng thái và một số correlation nghiệp vụ | Middleware lưu payload toàn phần; tích hợp AI có thể ghi nội dung lỗi/phản hồi thô; chưa có allowlist metadata, redaction và test secret/PII | Nghiêm trọng |
| M12-T044 | Chưa có | Ảnh ngoài có URL nguồn kỹ thuật; quyết định A/B chấp nhận rủi ro có kiểm soát khi thiếu bằng chứng quyền | Chưa có registry nguồn/quyền/attribution/thời điểm, owner xử lý thay đổi điều khoản hoặc takedown. Phạm vi A/B chỉ áp dụng cho tài sản đang dùng | Rất cao |
| M12-T045 | Đáp ứng một phần | Có điểm health công khai và health quản trị | Health luôn báo khỏe; chưa có SLI/SLO, độ đúng/độ mới/quota, error budget, chủ sở hữu và trạng thái degraded theo năng lực | Nghiêm trọng |
| M12-T046 | Chưa có | Có giới hạn AI/audio để giảm lạm dụng ở mức yêu cầu | Chưa đo usage/unit cost, phân bổ ngân sách, dự báo, đối soát hóa đơn, ngưỡng cảnh báo hoặc kill switch theo năng lực | Rất cao |
| M12-T047 | Chưa có | Có kiểm thử cho một số cấu hình/giới hạn nội bộ | Chưa có kiểm thử hợp đồng định kỳ, schema/error, canary, tiêu chí dừng và cảnh báo thay đổi phiên bản/điều khoản. Trong A/B chỉ áp dụng provider đang hoạt động | Rất cao |

## 5. Sai lệch nghiêm trọng cần ưu tiên

| Thứ tự | Sai lệch | Task liên quan | Tác động |
|---:|---|---|---|
| 1 | Đăng nhập ngoài tự ghép theo email; trạng thái chống giả mạo không được xác minh và token nội bộ nằm trong địa chỉ chuyển hướng | M12-T007–M12-T009 | Chiếm đoạt hoặc liên kết sai tài khoản; lộ token qua lịch sử, log hoặc trung gian |
| 2 | Request/response và phản hồi nhà cung cấp có thể được ghi thô | M12-T040–M12-T043 | Lộ token, thông tin cá nhân, prompt, nội dung học hoặc dữ liệu nhà cung cấp |
| 3 | Không có asset registry, metadata bất biến, quyền truy cập và vòng đời xóa/orphan | M12-T021–M12-T025 | Không truy nguồn, không thu hồi chính xác, rò tài sản riêng tư và tăng chi phí lưu trữ |
| 4 | Redis limiter fail-open toàn phần và có đường bỏ qua cho tiến trình nội bộ | M12-T034–M12-T035 | Lạm dụng auth/tài nguyên tốn phí hoặc gian lận khi Redis lỗi hay dấu hiệu trusted caller bị dùng sai |
| 5 | Thiếu timeout/deadline thống nhất, retry an toàn, circuit breaker và bulkhead | M12-T036–M12-T038 | Lỗi một provider lan rộng, cạn kết nối, retry storm và ghi kết quả muộn |
| 6 | Không có inventory/vòng đời bí mật và bản đồ dữ liệu rời hệ thống | M12-T040–M12-T043 | Không biết phạm vi ảnh hưởng, khó thu hồi/xóa và thiếu căn cứ riêng tư |
| 7 | Health giả định luôn khỏe; không có SLO, ngân sách chi phí hay kill switch | M12-T045–M12-T046 | Phát hiện chậm, quyết định vận hành sai và chi phí tăng không kiểm soát |
| 8 | Không có kiểm thử hợp đồng/canary hoặc quản trị bản quyền | M12-T044, M12-T047 | Thay đổi provider hoặc điều khoản gây lỗi diện rộng, vi phạm quyền sử dụng và thu hồi chậm |

## 6. Thứ tự nâng cấp đề xuất

### Nhóm 1 — Chặn rủi ro danh tính và rò dữ liệu

Ưu tiên M12-T006–M12-T009 và M12-T040–M12-T043. Không tự liên kết theo email, ràng buộc trạng thái đăng nhập với phiên, không chuyển token trong địa chỉ, dừng ghi payload thô và lập inventory bí mật/dữ liệu ngoài.

### Nhóm 2 — Tạo sổ đăng ký và hợp đồng nền

Thực hiện M12-T001–M12-T005, M12-T010 và M12-T031. Mọi năng lực phải có chủ, mục đích, module dùng, dữ liệu tối thiểu, mức quan trọng, trạng thái lỗi và suy giảm được duyệt trước khi mở rộng.

### Nhóm 3 — Quản lý tài sản số có vòng đời

Thực hiện M12-T021–M12-T025 cùng A-WP02. Tạo danh mục, metadata bất biến, kiểm tra upload thống nhất, phân loại quyền, thay thế/xóa có tham chiếu và dọn orphan có bằng chứng.

### Nhóm 4 — Giới hạn và cô lập lỗi

Thực hiện M12-T032–M12-T038. Loại đường bỏ qua không được phép, chọn fail-open/fail-closed theo năng lực, đặt deadline, retry có giới hạn/chống lặp và circuit/bulkhead thực sự.

### Nhóm 5 — Quản trị nhà cung cấp khi vận hành

Thực hiện M12-T044–M12-T047. Xác lập sổ quyền sử dụng trong phạm vi A/B, health thật, SLI/SLO, chi phí/ngân sách/kill switch và kiểm thử hợp đồng/canary cho provider đang hoạt động.

## 7. Cổng A-WP04

A-WP04 chưa đạt Cổng A. Điều kiện tối thiểu để chuyển trạng thái:

- Không tự liên kết tài khoản ngoài chỉ vì trùng email; trạng thái đăng nhập chống giả mạo/phát lại được xác minh; token không nằm trong địa chỉ hoặc log.
- Mọi tích hợp đang hoạt động có chủ, mục đích, dữ liệu tối thiểu, mức quan trọng, hợp đồng kết quả/lỗi, suy giảm và mục tiêu sức khỏe.
- Không lưu payload/response thô; bí mật có inventory, quyền tối thiểu, thu hồi/xoay vòng và audit phù hợp.
- Tài sản số A/B có catalog, upload an toàn, metadata bất biến, quyền truy cập, tham chiếu và vòng đời thay thế/xóa/orphan.
- Giới hạn lưu lượng không có đường bỏ qua theo dấu hiệu yêu cầu; failure mode được chốt theo auth, chi phí, gian lận và lõi học.
- Mỗi lời gọi ngoài có deadline; retry có giới hạn và chống lặp; lỗi/độ trễ được cô lập, không lan toàn hệ thống.
- Health phản ánh trạng thái thật; provider đang hoạt động có SLO, ngân sách chi phí, cảnh báo, công tắc dừng và kiểm thử hợp đồng/canary.
- Bản đồ dữ liệu ngoài bao phủ danh tính, tài sản và kênh đang dùng; AI/speech giữ ở mức chính sách và mặc định tắt trong A/B theo ngoại lệ đã chốt.

## 8. Giới hạn đánh giá

- Chưa chạy kiểm thử hoặc kiểm tra môi trường triển khai, tài khoản nhà cung cấp, dashboard quan sát và dữ liệu sản xuất.
- Không đọc giá trị bí mật; chỉ xác nhận cách hệ thống tham chiếu cấu hình.
- Chưa đánh giá ứng dụng web/mobile/admin ngoài những gì có thể suy ra từ hợp đồng phía WordSoulApi.
- M12-T026–M12-T030 và M12-T039 thuộc B-WP08; M12-T011–M12-T020, M12-T048–M12-T050 chưa được chọn cho Giai đoạn A/B hiện hành.
- M12-T042, M12-T044 và M12-T047 được đánh giá theo ngoại lệ phạm vi A/B đã ghi trong kế hoạch; không mở lại quyết định sản phẩm đã chốt.
