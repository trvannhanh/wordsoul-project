# Chính sách thông tin bảo mật M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T004 |
| Policy ID / phiên bản | M01-CRED-1.0 |
| Trạng thái | Có hiệu lực từ 2026-08-19 cho đăng nhập trực tiếp |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012; M01-D004, M01-D009, M01-D010, M01-D024–D025 |
| Phạm vi | Mật khẩu đăng nhập trực tiếp, verifier, tạo/đổi/đặt lại/khôi phục và xử lý hỗ trợ |
| Ngoài phạm vi | Access token, refresh credential, secret workload và credential provider ngoài có hợp đồng riêng nhưng cùng tuân thủ ranh giới không lộ bí mật |

## Yêu cầu mật khẩu đo được

| Chủ đề | Yêu cầu M01-CRED-1.0 | Hành vi từ chối/ngoại lệ |
|---|---|---|
| Độ dài | Từ 12 đến 128 Unicode scalar value, tính trên đúng chuỗi ứng dụng nhận | Dưới 12 hoặc trên 128 bị từ chối bằng lỗi policy chung; không cắt để hợp lệ |
| Chuỗi đầu vào | Cho phép khoảng trắng đầu/cuối, khoảng trắng bên trong và Unicode hợp lệ; giữ đúng chuỗi, không trim hoặc chuẩn hóa âm thầm | Dữ liệu Unicode không hợp lệ bị từ chối; UI không được biến đổi khác API |
| Tập ký tự | Không bắt buộc tổ hợp chữ hoa, chữ thường, số hoặc ký hiệu; cho phép cụm từ dài và trình quản lý mật khẩu | Không thêm composition rule ở client hoặc API ngoài policy version này |
| Giá trị phổ biến/đã lộ | So khớp với blocklist cục bộ và nguồn breached-password bằng cơ chế không gửi giá trị thô | Tạo/đổi/đặt lại fail-closed khi không hoàn tất kiểm tra; đăng nhập bằng verifier hiện có không gọi phụ thuộc này |
| Tái sử dụng | Từ chối giá trị trùng mật khẩu hiện tại; phiên bản 1.0 không lưu lịch sử mật khẩu cũ | Không suy ra hoặc lưu hash lịch sử ngoài quyết định phiên bản mới |
| Đổi định kỳ | Không buộc đổi chỉ vì thời gian | Buộc đổi/đặt lại khi có khôi phục, bằng chứng rủi ro, verifier yếu cần nâng cấp hoặc quyết định an toàn có audit |
| Lỗi phản hồi | Trả reason code theo policy và hướng dẫn có thể hành động, không phản chiếu chuỗi nhập | Không tiết lộ blocklist, trạng thái tài khoản, verifier hoặc kết quả kiểm tra nội bộ chi tiết |

## Lưu trữ, truyền và kiểm tra verifier

| Biên | Yêu cầu |
|---|---|
| Lưu trữ | Chỉ lưu verifier một chiều bằng implementation chuẩn được cấu hình/version hóa, salt riêng cho từng verifier và tham số đủ để kiểm tra/nâng cấp |
| Nâng cấp | Sau đăng nhập hợp lệ, nếu verifier dùng phiên bản/tham số cũ thì rehash bằng policy hiện hành trong mutation an toàn; không yêu cầu biết lại giá trị ngoài lần xác thực đó |
| So sánh | Dùng hàm kiểm tra của implementation chuẩn; không tự so sánh hash hoặc log nhánh chi tiết gây lộ thông tin |
| Truyền | Chỉ nhận qua kênh bảo vệ; không đặt trong URL, query, analytics, telemetry, message queue chung hoặc thông báo |
| Bộ nhớ/exception | Giữ chuỗi thô trong phạm vi ngắn nhất; không đưa vào exception, structured log, trace, dump chủ động hoặc payload audit |
| Provider kiểm tra đã lộ | Chỉ gửi biểu diễn bảo vệ/tối thiểu theo contract; không gửi mật khẩu, verifier, email hay định danh người dùng |
| Xóa | Xóa verifier khi phương thức đăng nhập bị gỡ hoặc danh tính bị xóa; audit chỉ giữ metadata sự kiện theo policy dữ liệu |

## Hành trình và tác động phiên

| Hành trình | Bằng chứng bắt buộc | Mutation | Tác động phiên | Thông báo/audit |
|---|---|---|---|---|
| Đăng ký trực tiếp | Dữ liệu đăng ký hợp lệ và mật khẩu đạt M01-CRED-1.0 | Tạo verifier mới; không lưu chuỗi thô | Chỉ cấp quyền theo trạng thái tài khoản, không tự coi là đã xác minh | Audit metadata tạo phương thức; không có mật khẩu/verifier |
| Đổi chủ động | Phiên an toàn và xác minh lại bằng mật khẩu hiện tại gần thời điểm thực hiện | Kiểm tra policy mới, ghi verifier mới atomically | Giữ thiết bị hiện tại; thu hồi các họ phiên khác theo M01-D009 | Cảnh báo bảo mật; audit trước/sau chỉ metadata/version |
| Đặt lại qua email | Bằng chứng một lần, có hạn, đúng mục đích, chống replay và tài khoản đủ điều kiện | Kiểm tra policy, tiêu thụ bằng chứng một lần và ghi verifier atomically | Thu hồi mọi phiên/họ phiên và yêu cầu đăng nhập lại | Phản hồi yêu cầu trung tính; cảnh báo khi hoàn tất; audit không có token |
| Nghi chiếm quyền | Rule/vụ việc có nguồn và xác minh phục hồi đạt | Đặt verifier mới hoặc vô hiệu phương thức theo playbook | Thu hồi mọi phiên ngay; không giữ thiết bị hiện tại | Cảnh báo qua kênh an toàn và audit vụ việc |
| Mất email nhưng còn đường hợp lệ | Bằng chứng theo M01-T019 và policy khôi phục | Không hạ chuẩn mật khẩu; thay đổi chỉ sau xác minh chủ thể | Thu hồi theo mức rủi ro, mặc định mọi phiên khi qua recovery | Audit và cảnh báo bắt buộc |
| Mất mọi kênh | Support case theo M01-T021, quyền tối thiểu, lý do và xác minh được duyệt | Support chỉ khởi tạo recovery; không xem, nhận hoặc đặt mật khẩu thay người dùng | Không cấp phiên trước khi recovery hoàn tất | Mọi lượt xem/mutation được audit; phản hồi không lộ dữ liệu |
| Admin/support yêu cầu reset | Actor có quyền, xác minh lại, reason/case và audit khả dụng | Chỉ phát hành hành trình reset cho chủ thể; không tạo mật khẩu tạm | Không cấp quyền cho actor; khi reset hoàn tất thu hồi mọi phiên chủ thể | Mutation fail-closed nếu audit bắt buộc không ghi được |

## Xác minh lại cho thao tác nhạy cảm

| Thao tác | Yêu cầu tối thiểu | Không đủ khi |
|---|---|---|
| Đổi mật khẩu | Mật khẩu hiện tại hoặc recovery đã hoàn tất; phiên hiện tại an toàn và còn mới theo M01-T016 | Chỉ có access token cũ hoặc thiết bị được nhận biết |
| Đổi email/phương thức đăng nhập | Xác minh phương thức hiện tại và phương thức mới; cảnh báo cả kênh phù hợp | Chỉ email mới tự khai hoặc provider trả email trùng |
| Liên kết/gỡ provider | Xác minh lại tài khoản WordSoul; không gỡ phương thức cuối nếu mất đường khôi phục | Chỉ có phiên provider hoặc trùng email |
| Đăng xuất mọi thiết bị/khôi phục | Xác minh lại hoặc bằng chứng recovery hợp lệ | Chỉ dựa tên hiển thị, email biết được hoặc câu hỏi bí mật |
| Admin/support khởi tạo recovery | Phiên quản trị đủ quyền, xác minh lại, case/reason và audit khả dụng | Actor chỉ có quyền xem, thiếu case hoặc audit lỗi |

## Phản hồi, log và audit

- Đăng nhập sai trả kết quả trung tính cho tài khoản không tồn tại, mật khẩu sai hoặc phương thức không phù hợp; không tạo khác biệt có thể dò tài khoản.
- Yêu cầu reset luôn trả phản hồi tiếp nhận trung tính; rate limit và trạng thái gửi không tiết lộ tài khoản tồn tại.
- Audit allowlist gồm event ID, thời điểm, actor/subject reference bảo vệ, hành động, policy version, result/reason code và correlation/case khi cần.
- Password, verifier, reset token, provider token, authorization code và payload hỗ trợ luôn bị loại khỏi log/audit/analytics/bằng chứng.
- Email chỉ được ghi ở dạng che khi mục đích vận hành thật sự cần; structured log không được nhận DTO/request body xác thực.
- Thay đổi credential, reset do quản trị/hỗ trợ và thay đổi kênh khôi phục fail-closed nếu audit bắt buộc không ghi được. Sự kiện đăng nhập thất bại vẫn từ chối truy cập và phải có cơ chế buffer/cảnh báo không chứa bí mật.
- Nhật ký bảo mật/kiểm toán giữ 12 tháng mặc định theo M01-D025; hold điều tra/nghĩa vụ phải có phạm vi và thời hạn riêng.

## Ngoại lệ và thay đổi policy

- Không có ngoại lệ cho lưu/ghi log bí mật thô, bỏ xác minh chủ thể, hỗ trợ đặt mật khẩu, tạo mật khẩu tạm hoặc tự cấp quyền quản trị.
- Ngoại lệ khác phải có owner, lý do, phạm vi, đánh giá rủi ro, kiểm soát bù trừ, thời hạn và audit; hết hạn tự động quay về policy chuẩn.
- Thay đổi độ dài, blocklist, hành vi outage, reuse hoặc verifier tạo `M01-CRED` version mới và kế hoạch tương thích/rehash; client không được áp policy mới trước API.

## Ma trận kiểm thử bắt buộc

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| SEC-C01 | 11 scalar value | Từ chối, không tạo verifier |
| SEC-C02 | Đúng 12 scalar value | Qua kiểm tra độ dài; còn phải qua blocklist/breached check |
| SEC-C03 | Đúng 128 scalar value | Được hỗ trợ đầy đủ, không cắt |
| SEC-C04 | 129 scalar value | Từ chối rõ theo policy, không cắt xuống 128 |
| SEC-C05 | Unicode và khoảng trắng đầu/cuối hợp lệ | Tạo/kiểm tra đúng cùng chuỗi; client/API không trim hoặc normalize khác nhau |
| SEC-C06 | Thiếu composition nhưng đủ dài và không nằm blocklist | Chấp nhận; không ép chữ hoa/số/ký hiệu |
| SEC-C07 | Giá trị phổ biến hoặc đã lộ | Từ chối mà không gửi/ghi giá trị thô |
| SEC-C08 | Nguồn breached-password lỗi khi tạo/đổi/reset | Fail-closed với reason code phụ thuộc; đăng nhập hiện hữu vẫn kiểm tra verifier nội bộ |
| SEC-C09 | Tái dùng mật khẩu hiện tại | Từ chối; không cần kho lịch sử cũ |
| SEC-C10 | Reset token hết hạn hoặc replay | Từ chối, không đổi verifier và không cấp phiên |
| SEC-C11 | Đổi chủ động thành công | Giữ phiên hiện tại an toàn, thu hồi các phiên khác và gửi cảnh báo |
| SEC-C12 | Reset/chiếm quyền thành công | Thu hồi mọi phiên và bắt đăng nhập lại |
| SEC-C13 | Support/admin thử xem, nhận hoặc đặt mật khẩu | Từ chối; chỉ cho khởi tạo recovery đúng quyền/case/audit |
| SEC-C14 | Log/audit/exception sau success, reject và dependency error | Không có mật khẩu, verifier, token, DTO body hoặc email đầy đủ |
| SEC-C15 | Audit bắt buộc không khả dụng khi admin khởi tạo reset | Mutation bị từ chối; không trả thành công giả |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-19; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-SEC-I01 | `RegisterDto.Password` không có validation và `RegisterAsync` hash trực tiếp giá trị nhận được | Chưa cưỡng chế 12–128, Unicode/space, blocklist hoặc breached check | M01-T005, M01-T009 |
| M01-SEC-I02 | Đăng nhập/đăng ký trực tiếp dùng `Username` thay vì email theo từ điển/M01-D003 | Policy định danh và phản hồi dò tài khoản chưa hội tụ | M01-T005, M01-T010 |
| M01-SEC-I03 | `AuthService` tạo `PasswordHasher<User>` trực tiếp, chưa có policy service/version hoặc đường rehash được quan sát | Khó bảo đảm cùng policy cho đăng ký/đổi/reset và nâng cấp verifier | M01-T005, M01-T019–T020 |
| M01-SEC-I04 | Không tìm thấy luồng đổi, quên hoặc đặt lại mật khẩu trong phạm vi mã M01 đã rà | Chưa có bằng chứng one-time/replay protection, xác minh lại, session revoke hoặc cảnh báo | M01-T019–T021 |
| M01-SEC-I05 | Không tìm thấy kiểm tra mật khẩu phổ biến/đã lộ | Không đáp ứng M01-D004/D-012 | M01-T005, M01-T020; M12 contract nếu dùng provider |
| M01-SEC-I06 | Không thấy rate limit/anti-enumeration được cưỡng chế trong `AuthService` | Chưa chứng minh chống dò/brute force tại ranh đăng nhập/reset | M01-T011; M12-T034–T035 |
| M01-SEC-I07 | `PasswordHash` và refresh credential cùng nằm trên thực thể `User` | Ranh credential/session và quyền truy cập kho chưa được chứng minh | M01-T016–T018; M01-T033 |

## Finding còn mở có trạng thái an toàn

| Mã | Phần cần cụ thể hóa | Baseline an toàn hiện hành | Nguồn/task xử lý |
|---|---|---|---|
| M01-SEC-F01 | Implementation/verifier algorithm và tham số triển khai theo môi trường | Dùng implementation chuẩn có version/salt/rehash; không tự viết crypto hoặc hạ tham số | M01-T005, M01-T020; security configuration |
| M01-SEC-F02 | Freshness và proof cho đổi chủ động credential/email | Recovery proof/TTL đã chốt ở M01-RECOVERY-1.0; không tái dùng recovery code như session hoặc change proof | M01-T020 |
| M01-SEC-F03 | Contract/khu vực/retention của nguồn breached-password ngoài | Không gửi giá trị thô/PII; không bật provider trước registry và contract | M12-T001–T005, M12-T040–T044; REL-03 |
| M01-SEC-F04 | Danh mục quyền quản trị/support và audit fail-closed đầy đủ | Support không xem/đặt mật khẩu; mutation nhạy cảm thiếu quyền/case/audit bị từ chối | M01-T028–T032, M11-T027–T035; REL-02 |

## Tự kiểm M01-T004, A-G01, A-G02 và REL-02

- M01-CRED-1.0 có ngưỡng 12–128, quy tắc Unicode/space, blocklist/breached, reuse, storage, outage và policy change đo được.
- Bảy hành trình nêu bằng chứng, mutation, tác động phiên và audit; năm thao tác nhạy cảm có yêu cầu xác minh lại/từ chối.
- Mười lăm case bao phủ boundary, Unicode, dependency outage, replay, session revoke, support/admin, redaction và audit loss.
- Bảy finding triển khai và bốn finding cấu hình/quyền có baseline an toàn cùng task tiếp nhận; không còn điểm chờ vô chủ trong M01-T004.
- REL-02 vẫn mở: ma trận quyền/audit đầy đủ và bằng chứng runtime A-G02 chưa hoàn thành. Tài liệu này không kết luận A-G01/A-G02/REL-02 đạt và không thay thế kiểm thử runtime.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Chuẩn hóa chính sách từ M01-D004 và khóa các giá trị cần reviewer kỹ thuật | Chưa gán |
| 2026-08-19 | 1.0 | Chốt policy M01-CRED-1.0, hành trình/xác minh lại, 15 case và finding hiện trạng | WSA-7K2 |
