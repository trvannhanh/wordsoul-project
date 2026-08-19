# Bản đồ dữ liệu hồ sơ M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T003 |
| Phiên bản | 1.0 |
| Trạng thái | Baseline dữ liệu hồ sơ có hiệu lực từ 2026-08-19; retention tuổi/đồng ý/xóa tiếp tục bị điều kiện bởi REL-01/REL-07 |
| Thuật ngữ nguồn | Từ điển danh tính M01 v1.0 |
| Phạm vi | Dữ liệu M01 sở hữu hoặc tham chiếu trực tiếp trong hồ sơ; bản đồ liên module đầy đủ thuộc M01-T033 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Nguồn quyết định | M01-D011–D015, M01-D018–D025; D-008, D-011 |

## Quy ước phân loại và quyền

| Nhãn | Ý nghĩa | Quy tắc mặc định |
|---|---|---|
| Công khai có lựa chọn | Trường hồ sơ có thể hiển thị cho người khác sau lựa chọn rõ ràng | Riêng tư cho tới khi người dùng bật và chính sách cộng đồng cho phép |
| Riêng tư | Dữ liệu phục vụ trải nghiệm cá nhân | Chủ tài khoản xem; hỗ trợ chỉ xem theo vụ việc, mục đích và phạm vi |
| Nhạy cảm | Dữ liệu định danh, trạng thái, quyền, an toàn hoặc hành vi có thể gây hại nếu lộ | Che theo góc nhìn; truy cập đặc quyền có quyền tối thiểu và audit |
| Bí mật | Credential/token/bằng chứng không được hiển thị hoặc ghi lại | Chỉ workload xác thực dùng qua cơ chế bảo vệ chuyên biệt |
| Dẫn xuất/tham chiếu | M01 chỉ hiển thị dữ liệu do module khác sở hữu | Không sửa, không sao chép thành nguồn sự thật; kèm nguồn và độ mới |

Các góc nhìn hồ sơ phải dùng DTO/allowlist riêng: `self`, `public`, `support`, `security-admin` và `service`. Không tuần tự hóa thực thể lưu trữ rồi loại trường sau.

## Bản đồ trường logic

| Nhóm/trường logic | Hiện trạng vật lý quan sát | Mục đích | Phân loại | Nguồn sự thật | Xem | Sửa | Xuất | Xóa/ẩn danh/lưu giữ |
|---|---|---|---|---|---|---|---|---|
| Định danh người dùng nội bộ | `User.Id` | Liên kết danh tính và ownership ổn định | Nhạy cảm | M01 | Service và vai trò đúng quyền; self chỉ khi contract thật sự cần | Hệ thống tạo, bất biến, không tái sử dụng | Dùng định danh tương quan trong manifest, không biến thành định danh công khai | Gỡ liên kết nhận dạng; lịch sử cần toàn vẹn dùng surrogate không thể liên kết lại theo M01-T036/REL-07 |
| Email đăng nhập chuẩn hóa | `User.Email`; DTO đăng ký/đăng nhập/hồ sơ | Đăng nhập trực tiếp, xác minh, phục hồi và cảnh báo | Nhạy cảm | M01 | Self; support đúng vụ việc ở dạng che; không public | Self qua luồng đổi/xác minh lại; support không sửa trực tiếp | Có trong phần dữ liệu cá nhân của chủ thể sau xác minh lại | Xóa nhận dạng trực tiếp khi hoàn tất xóa; ngoại lệ/hold phải có căn cứ REL-01/REL-07 |
| Tên hiển thị | `User.Username` đang đồng thời được dùng cho đăng nhập và hiển thị | Nhận diện trong trải nghiệm/cộng đồng | Công khai có lựa chọn | M01 | Self; public theo lựa chọn; support theo quyền | Self theo kiểm duyệt/cooldown; không dùng để chứng minh danh tính | Có | Xóa/ẩn danh giá trị hiện tại; lịch sử chống lạm dụng theo retention được duyệt |
| Bằng chứng mật khẩu | `User.PasswordHash` | Xác thực đăng nhập trực tiếp | Bí mật | M01 credential store | Không người dùng/quản trị nào xem | Chỉ luồng đặt/đổi/khôi phục ghi giá trị bảo vệ mới | Không bao giờ | Xóa khi phương thức hoặc danh tính bị xóa; không ghi payload vào audit/log |
| Liên kết danh tính ngoài | `ExternalLoginProvider`, `ExternalLoginProviderKey`, `ExternalLoginEmail` trên `User` | Đăng nhập bằng provider và quản lý liên kết; contract provider tham chiếu M12 | Nhạy cảm | M01 | Self xem tên provider; support/security đúng quyền; không public | Luồng liên kết/gỡ có xác minh; không tự ghép từ email | Chỉ metadata liên kết cần thiết, không gồm token provider | Gỡ/xóa định danh provider khi xóa; không tự nối lại tài khoản mới |
| Trạng thái xác minh email | Chưa có trường bền vững riêng được quan sát | Cưỡng chế quyền trước/sau xác minh | Nhạy cảm | M01 | Self và service quyết định; support đúng quyền | Chỉ luồng xác minh M01 | Có trạng thái/thời điểm cần thiết, không có mã xác minh | Xóa/ẩn danh cùng danh tính; audit lưu theo chính sách |
| Nhóm tuổi/khu vực tự khai | Chưa có trường được quan sát | Chọn policy áp dụng, không suy diễn ngày sinh | Nhạy cảm | M01 | Self; service policy; vai trò chuyên trách | Self qua luồng có kiểm soát | Chỉ trường đã được REL-01 cho phép | Chờ REL01-Q02/Q07 và ma trận REL-07; không thu thập trước khi có mục đích/căn cứ |
| Bản ghi đồng ý và policy version | Chưa có entity/trường được quan sát | Chứng minh lựa chọn, phạm vi và phiên bản chính sách | Nhạy cảm | M01 | Self dạng dễ hiểu; vai trò riêng tư/pháp lý đúng quyền | Append/thu hồi qua luồng; không ghi đè lịch sử | Có bản ghi của chủ thể theo policy | Lưu/xóa/hold theo REL01-Q05/Q07 và REL-07; chưa được tự đặt thời hạn |
| Trạng thái tài khoản và hạn chế | `User.IsActive` chỉ là cờ hiện trạng | Cưỡng chế vòng đời/quyền truy cập | Nhạy cảm | M01 | Self ở mức giải thích phù hợp; service/support/admin đúng quyền | Hệ thống hoặc actor có quyền, lý do và audit | Có lịch sử/quyết định ở dạng phù hợp | Trạng thái nhận dạng xóa/ẩn danh; audit giữ theo M01-D025/hold được duyệt |
| Vai trò và quyền | `User.Role`; claim vai trò trong token | Phân quyền nghiệp vụ/quản trị | Nhạy cảm | M01 | Self ở mức phù hợp; admin/security đúng quyền; service kiểm tra | Luồng cấp/thu hồi có kiểm soát; không tự sửa | Có lịch sử cấp/thu hồi của chủ thể theo quyền | Thu hồi khi đóng/xóa; audit mặc định 12 tháng theo M01-D025, hold có phạm vi |
| Mốc tạo và hoạt động | `CreatedAt`, `LastActiveAt`, `LastReminderEmailSentAt` | Vận hành vòng đời, an toàn và nhắc tương tác | Riêng tư | M01 | Self khi có giá trị trải nghiệm; service/support đúng mục đích | Hệ thống ghi; không cho client đặt trực tiếp | Có nếu thuộc phạm vi yêu cầu | Xóa/ẩn danh cùng danh tính; retention audit/an toàn tách riêng |
| Phiên và refresh credential | `RefreshToken`, `RefreshTokenExpiryTime` trên `User` | Duy trì và thu hồi truy cập | Bí mật | M01 session store | Self chỉ xem metadata phiên, không token; security workload dùng bí mật | Hệ thống rotation/revoke; self chỉ yêu cầu thu hồi | Chỉ metadata phiên an toàn, không token | Thu hồi/hết hạn; xóa bí mật khi đóng/xóa; metadata an toàn theo audit |
| Múi giờ đã xác nhận | Chưa có trường được quan sát | Ranh giới ngày và lập lịch theo M01-D014 | Riêng tư | M01 | Self; module lập lịch cần thiết | Self xác nhận; thiết bị chỉ đề xuất | Có | Xóa cùng lựa chọn hồ sơ |
| Giờ học mong muốn | `PreferredStudyHour` | Cá nhân hóa nhắc học | Riêng tư | M01 | Self; M10 theo consent/phạm vi | Self; kiểm tra miền giá trị | Có | Xóa cùng lựa chọn hồ sơ |
| Thiết bị nhận tin | `FcmToken` đơn trên `User` | Gửi thông báo đúng thiết bị; M10 chỉ tiêu thụ endpoint hợp lệ | Nhạy cảm | M01 | Self xem metadata thiết bị, không token; workload gửi dùng token | Luồng đăng ký/thu hồi; không nhập trực tiếp trong hồ sơ chung | Metadata thiết bị, không endpoint/token thô | Xóa token/liên kết khi logout, revoke, invalid endpoint hoặc xóa tài khoản |
| Tham chiếu ảnh đại diện | `AvatarUrl` trên `User` | Chọn asset M12 để hiển thị trong hồ sơ | Công khai có lựa chọn | M01 | Theo góc nhìn hồ sơ và lựa chọn công khai | Self qua luồng asset có kiểm tra quyền | Có asset ID/metadata được phép, không URL riêng tư có hạn | Gỡ liên kết; asset nguồn xử lý tại M12 theo REL-04/REL-07 và reference check |
| Tùy chọn hiển thị hồ sơ | Chưa có trường/registry được quan sát | Ghi lựa chọn public/private theo từng trường | Riêng tư | M01 | Self; service render hồ sơ | Self; thay đổi có audit khi ảnh hưởng riêng tư | Có | Xóa cùng hồ sơ; lịch sử tối thiểu theo policy |
| Thiết bị đăng nhập nhận biết | Chưa có registry; refresh token nằm trực tiếp trên `User` | Nhận diện thiết bị, nhiều phiên và thu hồi theo phạm vi | Nhạy cảm | M01 | Self xem metadata; security/support đúng quyền | Hệ thống; self chỉ đặt tên/thu hồi trong phạm vi | Metadata an toàn, không fingerprint/token thô | Hết hạn/thu hồi; xóa liên kết khi xóa, giữ audit tối thiểu |
| Số dư/tài sản hiển thị | `XP`, `AP`, `HintBalance` trên `User`; quan hệ vật phẩm/thú cưng | Tổng quan gamification | Dẫn xuất/tham chiếu | M06; AP bị đóng băng theo D-011 | Self; vai trò đúng quyền theo M06 | Không sửa qua hồ sơ M01 | Theo manifest M06 | Không sao chép nguồn; xử lý theo M06/REL-07, AP theo REL-05 |
| Chỉ số PvP hiển thị | `PvpRating`, `PvpWins`, `PvpLosses` trên `User` | Hiển thị thành tích thi đấu | Dẫn xuất/tham chiếu | M08 | Self; public theo lựa chọn/chính sách thi đấu | Không sửa qua hồ sơ M01 | Theo manifest M08 | Ẩn danh/xóa theo M08/REL-07; không sao chép thành nguồn M01 |
| Xếp hạng/nhóm cộng đồng hiển thị | Dữ liệu nhóm/xếp hạng ở module liên quan | Hiển thị quan hệ và vị trí cộng đồng | Dẫn xuất/tham chiếu | M09 | Self; public theo lựa chọn/chính sách cộng đồng | Không sửa qua hồ sơ M01 | Theo manifest M09 | Ẩn danh/xóa theo M09/REL-07; không sao chép thành nguồn M01 |
| Nhật ký danh tính/audit | Activity log và log ứng dụng tham chiếu người dùng | Điều tra, truy vết thay đổi nhạy cảm | Nhạy cảm | M11; M01 phát sự kiện | Security/support đúng quyền; self theo hợp đồng xuất | Append-only qua service; không sửa/xóa tùy ý | Sự kiện thuộc chủ thể sau che dữ liệu và xác minh | Mặc định 12 tháng theo M01-D025; hold riêng có phạm vi/thời hạn |

## Biên dữ liệu rời hệ thống

Đây là allowlist tối thiểu cho hồ sơ M01. Provider, khu vực xử lý, retention và subprocessor cụ thể vẫn phải được đăng ký bởi M12/REL-03 trước khi bật.

| Flow | Dữ liệu tối thiểu được phép | Dữ liệu cấm gửi | Điều kiện |
|---|---|---|---|
| Provider danh tính ngoài | Authorization artifact sống ngắn theo protocol; provider subject; email/trạng thái xác minh chỉ khi contract đã duyệt | Hồ sơ đầy đủ, tiến độ, tài sản, mật khẩu, refresh credential WordSoul | M12 contract/allowlist; state/redirect hợp lệ; không log payload/token |
| Kênh email | Email đích, template ID, locale và dữ liệu thay thế tối thiểu | Mật khẩu, token thô trong log, hồ sơ/tiến độ không cần thiết | Mục đích đã duyệt, chống lạm dụng, retention và xóa theo REL-03/REL-07 |
| Provider push | Endpoint thiết bị và payload thông báo tối thiểu | Email, tuổi/khu vực, credential, hồ sơ đầy đủ | Consent/phạm vi gửi hợp lệ; endpoint được thu hồi/xóa đúng vòng đời |
| Media/avatar | Asset nhị phân/metadata cần cho upload và tham chiếu asset | Email, credential, trạng thái tài khoản hoặc dữ liệu học | Catalog/quyền REL-04; URL riêng tư có hạn không vào log/bằng chứng |

## Quy tắc truy cập, xuất và xóa

- API/giao diện hồ sơ dùng allowlist theo góc nhìn; quyền xem không kéo theo quyền sửa, xuất hoặc tra cứu lịch sử.
- Email và trường nhạy cảm phải che theo ngữ cảnh; không dùng email làm định danh công khai hoặc tiêu chí tự liên kết.
- Support chỉ tra cứu khi có vụ việc, mục đích, phạm vi, xác minh phù hợp và audit; không bao giờ xem credential/token.
- Trường dẫn xuất luôn kèm module nguồn và độ mới; M01 không điều chỉnh số dư, tiến độ, xếp hạng hoặc tài sản thay module sở hữu.
- Export dùng manifest phiên bản hóa, chỉ gồm trường của chủ thể được phép; credential/token và bí mật luôn bị loại.
- Xóa dùng manifest từng module, idempotent và đối soát; phần lỗi không được báo thành công toàn phần. Cache, asset, provider và dữ liệu dẫn xuất phải được xử lý theo ma trận REL-07.
- Tài liệu, log và bằng chứng chỉ dùng schema/metadata/dữ liệu thử giả; không lưu PII thật, token, nội dung export hoặc payload provider.

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-19; chưa phải bằng chứng runtime.

| Mã | Finding | Rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-DATA-I01 | `User` chưa có trạng thái xác minh email, nhóm tuổi/khu vực, consent/policy version, timezone hoặc lựa chọn hiển thị | Không cưỡng chế/giải thích được policy và quyền riêng tư theo trường | M01-T005–T007, M01-T022-A, M01-T025-A; REL-01 |
| M01-DATA-I02 | `Username` vừa là dữ liệu đăng nhập vừa là tên hiển thị và có kiểm tra duy nhất | Mâu thuẫn từ điển v1.0/M01-D011; có thể lộ hoặc khóa sai định danh | M01-T005, M01-T010, M01-T023-A |
| M01-DATA-I03 | `PasswordHash`, một `RefreshToken` và thời hạn được đặt trực tiếp trên `User` | Ranh credential/session chưa tách; khó rotation, nhiều thiết bị và thu hồi theo phạm vi | M01-T004, M01-T016–T018 |
| M01-DATA-I04 | Ba trường external login trên `User` chỉ biểu diễn một liên kết và code tự ghép theo email | Không hỗ trợ nhiều provider an toàn; nguy cơ chiếm quyền/liên kết sai | M01-T013–T015; M12-T006–T010 |
| M01-DATA-I05 | `FcmToken` chỉ lưu một endpoint trên người dùng | Không đáp ứng nhiều thiết bị, trạng thái và thu hồi theo M01-D015 | M01-T026-A, M01-T027-A |
| M01-DATA-I06 | `AvatarUrl` nhận trực tiếp từ DTO cập nhật hồ sơ | Chưa chứng minh ownership, asset ID, lifecycle hoặc quyền phân phối | M01-T024; M12-T021–T025; REL-04 |
| M01-DATA-I07 | DTO thành viên nhóm chứa `Email` cùng tên/role | Cần chứng minh allowlist/quyền để tránh lộ email trong bối cảnh cộng đồng | M01-T022-A, M01-T041; M09 |
| M01-DATA-I08 | Auth service ghi email đầy đủ trong một số log OAuth | Không đạt yêu cầu redaction G05-L01/G05-L02 | M01-T038; M11-T031–T035; M12-T041 |
| M01-DATA-I09 | `XP`, `AP`, `HintBalance` và chỉ số PvP nằm trên `User` dù chủ ngữ nghĩa thuộc module khác | Dễ biến M01 thành nguồn sự thật kép; AP còn là dữ liệu legacy | M01-T022-A; M06/M08/M09; D-011/REL-05 |

## Finding chính sách và task tiếp nhận

| Mã | Phần chưa chốt | Trạng thái an toàn trong baseline | Nguồn/task xử lý |
|---|---|---|---|
| M01-DATA-F01 | Trường tuổi/khu vực/đồng ý tối thiểu, quyền xem và retention | Không thu thập hoặc suy diễn thêm; không mở nhánh phụ thuộc khi thiếu kết luận | REL01-Q02/Q05/Q07; M01-T007, M01-T033 |
| M01-DATA-F02 | Retention/hold và hành động xóa theo từng dataset | Chỉ áp dụng quy tắc đã chốt; không xóa bằng cách chỉ xóa `User` hoặc tự đặt thời hạn | REL07-Q01/Q04/Q06; M01-T033, M01-T036 |
| M01-DATA-F03 | Quyền public/self/support/admin chi tiết cho từng trường | Riêng tư mặc định, support theo vụ việc, không public email/credential | M01-T022-A, M01-T028, M01-T041 |
| M01-DATA-F04 | Registry provider/khu vực/retention cho dữ liệu rời hệ thống | Không bật flow mới ngoài contract/allowlist đã duyệt | M12-T006, M12-T040–T044; REL-03 |

## Tự kiểm M01-T003, A-G01, A-G05, REL-01 và REL-07

- 22 nhóm dữ liệu logic có mục đích, phân loại, nguồn sự thật, quyền xem/sửa, xuất và hành động xóa/lưu giữ sơ bộ.
- Mỗi nhóm có đúng một nguồn sự thật ngữ nghĩa; trường dẫn xuất M06/M08/M09 không được sửa hoặc sao chép thành nguồn M01.
- Năm góc nhìn hồ sơ dùng allowlist; bí mật/token bị loại khỏi UI, export, log và bằng chứng theo baseline.
- Bốn biên dữ liệu ngoài có allowlist tối thiểu và giữ điều kiện M12/REL-03; không kết luận provider cụ thể đã đạt A-G05.
- Chín finding triển khai và bốn finding chính sách có trạng thái an toàn cùng task/REL tiếp nhận; không còn khoảng trống vô chủ trong phạm vi M01-T003.
- REL-01 và REL-07 vẫn mở: retention, hold, tuổi/đồng ý và ma trận xuyên module chưa được tự chốt. Tài liệu này không kết luận A-G01/A-G05 hoặc các REL đã đạt và không thay thế kiểm thử runtime.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo bản đồ trường hồ sơ nền, phân loại và quyền xem/sửa | Chưa gán |
| 2026-08-19 | 1.0 | Chốt 22 nhóm dữ liệu, nguồn sự thật, quyền, export/xóa, biên dữ liệu ngoài và finding hiện trạng | WSA-7K2 |
