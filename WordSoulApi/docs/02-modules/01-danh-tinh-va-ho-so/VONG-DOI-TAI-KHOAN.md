# Vòng đời tài khoản M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T002 |
| Phiên bản | 1.0 |
| Trạng thái | Baseline vòng đời có hiệu lực từ 2026-08-19; nhánh tuổi/đồng ý tiếp tục bị điều kiện bởi REL-01 |
| Thuật ngữ nguồn | Từ điển danh tính M01 v1.0 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Nguồn quyết định | M01-D002, M01-D007–D010, M01-D017–D021; REL-01 |

## Nguyên tắc bất biến

- API lưu và kiểm tra trạng thái tài khoản hiện tại. Client, token còn hạn hoặc provider ngoài không được bỏ qua trạng thái.
- Mỗi tài khoản có đúng một trạng thái chính. “Hạn chế theo chức năng” là lớp phủ có phạm vi, không phải trạng thái chính thứ hai.
- Mọi chuyển trạng thái có tác nhân, lý do, thời điểm, nguồn yêu cầu, trạng thái trước/sau và audit phù hợp. Nếu không ghi được audit bắt buộc, chuyển trạng thái nhạy cảm phải thất bại an toàn.
- Thay đổi trạng thái phải xử lý đồng bộ quyền cấp mới, access token, refresh credential, họ phiên và thiết bị; token tự chứa không phải nguồn sự thật của trạng thái hiện tại.
- Khóa, ngừng hoạt động, chờ xóa và đã xóa không được fail-open. Thao tác lặp phải idempotent và không tự mở lại tài khoản.
- Nhánh tuổi/đồng ý chỉ được cấu hình sau kết luận REL-01. Khi không xác định được chính sách áp dụng, không mở năng lực nhạy cảm hoặc tự chuyển sang `Hoạt động`.
- Không đặt giả số ngày cho phiên quản trị, khóa hoặc chờ xóa; giá trị cụ thể thuộc task/cấu hình có chủ.

## Mô hình trạng thái

```text
[Khởi tạo]
     |
     v
[Chờ xác minh thư] -----> [Chờ điều kiện tuổi/đồng ý]*
     |                                  |
     +----------------------------------+
                        v
                   [Hoạt động] <------ [Tạm khóa do rủi ro]
                    |   ^  |              |
                    |   |  +----------> [Khóa quản trị]
                    |   |                    |
                    v   +--------------------+
           [Ngừng hoạt động lâu dài]
                    |
                    v
                 [Chờ xóa] -----> [Đã xóa/ẩn danh]

* Chỉ kích hoạt theo kết luận REL-01.
* Lớp phủ “Hạn chế theo chức năng” có thể áp dụng cho trạng thái Hoạt động.
```

## Ma trận trạng thái và quyền

| Trạng thái | Điều kiện vào | Hành động được phép | Hành động bị từ chối | Tác động phiên | Hiển thị hồ sơ/dữ liệu | Điều kiện thoát |
|---|---|---|---|---|---|---|
| Chờ xác minh thư | Đăng ký hợp lệ nhưng chưa hoàn tất bằng chứng thư bắt buộc | Xem học liệu và học giới hạn; quản lý tối thiểu cho xác minh/đăng xuất | Xuất bản, xã hội, PvP, thông báo ngoài ứng dụng, thay đổi nhạy cảm và quyền đầy đủ | Chỉ cấp phạm vi giới hạn; refresh không nâng quyền; hết hạn theo chính sách phiên | Chỉ chủ tài khoản thấy hồ sơ riêng tối thiểu; không công khai ngoài phạm vi đã duyệt | Xác minh hợp lệ và điều kiện bắt buộc khác đạt; hoặc chuyển khóa/chờ xóa |
| Chờ điều kiện tuổi/đồng ý | REL-01 yêu cầu điều kiện chưa có, bị từ chối, hết hiệu lực hoặc đã rút | Chỉ phạm vi an toàn được kết luận rõ trong REL-01; đường cập nhật/thu hồi đồng ý | Mọi năng lực chưa được REL-01 cho phép và mọi thao tác nhạy cảm | Không cấp/gia hạn vượt phạm vi đã duyệt; giảm quyền có hiệu lực ở lần kiểm tra kế tiếp hoặc ngay khi chính sách yêu cầu | Tối thiểu, riêng tư mặc định; không công khai do giả định tuổi | Điều kiện REL-01 đạt; hoặc chuyển trạng thái khóa/chờ xóa phù hợp |
| Hoạt động | Xác minh và mọi điều kiện bắt buộc hiện hành đã đạt | Theo vai trò, quyền, ownership và lớp phủ hạn chế | Mọi hành động không có quyền, sai ownership hoặc cần xác minh lại chưa đạt | Phiên hợp lệ được duy trì; thao tác nhạy cảm kiểm tra lại trạng thái và xác minh gần thời điểm thực hiện | Theo ma trận hồ sơ/quyền riêng tư; không mặc định công khai | Hạn chế, khóa, ngừng hoạt động hoặc chờ xóa |
| Tạm khóa do rủi ro | Tín hiệu nghi chiếm quyền hoặc rủi ro đạt rule đã duyệt | Chỉ khôi phục/hỗ trợ an toàn và xem thông báo cần thiết | Đăng nhập mới, gia hạn và hành động nghiệp vụ | Chặn cấp mới; thu hồi mọi phiên khi nghi chiếm quyền, ghi kết quả thu hồi | Không công khai thay đổi mới; hỗ trợ chỉ xem phần đã che theo quyền | Xác minh phục hồi đạt; chuyển khóa quản trị hoặc chờ xóa |
| Khóa quản trị | Người có quyền áp dụng khóa với lý do, phạm vi/thời hạn và bảo vệ quản trị cao nhất | Chỉ khiếu nại/hỗ trợ được chính sách cho phép | Mọi truy cập nghiệp vụ và cấp/gia hạn phiên | Chặn cấp/gia hạn; thu hồi hoặc chặn mọi phiên hiện có | Không thay đổi hồ sơ; chỉ vai trò hỗ trợ đúng quyền xem dữ liệu đã che | Quyết định mở lại có audit; chuyển ngừng hoạt động hoặc chờ xóa |
| Ngừng hoạt động lâu dài | Quyết định có thẩm quyền xác định tài khoản không còn được sử dụng | Chỉ hỗ trợ, khiếu nại hoặc xử lý dữ liệu được phép | Đăng nhập và mọi hành động nghiệp vụ | Thu hồi/chặn mọi phiên và phương thức cấp mới | Không công khai; giữ/xem theo chính sách dữ liệu và quyền hỗ trợ | Mở lại có thẩm quyền sau tái kiểm tra; hoặc chuyển chờ xóa |
| Chờ xóa | Yêu cầu qua hỗ trợ đã xác minh lại, có policy version và thời gian chờ | Xem trạng thái yêu cầu; hủy trong thời gian cho phép qua kênh đã xác minh | Hành động nghiệp vụ mới, đăng nhập thông thường và thay đổi nhằm né xóa | Thu hồi phiên thông thường; chỉ kênh ngắn hạn, phạm vi hẹp cho trạng thái/hủy nếu chính sách cho phép | Ngừng công khai; dữ liệu giữ nguyên trong thời gian chờ trừ xử lý an toàn bắt buộc | Hủy hợp lệ về trạng thái an toàn trước đó; hoặc xóa/ẩn danh sau đối soát |
| Đã xóa/ẩn danh | Manifest từng module hoàn tất và đối soát không còn lỗi chặn | Không có quyền truy cập; chỉ lịch sử đã ẩn danh theo chính sách được hệ thống sử dụng | Đăng nhập, khôi phục, liên kết lại hoặc mở lại tài khoản cũ | Không còn phiên, refresh credential hoặc phương thức đăng nhập hợp lệ | Không còn hồ sơ nhận dạng; dữ liệu giữ lại không thể liên kết lại | Trạng thái cuối; đăng ký lại tạo định danh và tài khoản mới |

## Lớp phủ hạn chế theo chức năng

| Thuộc tính bắt buộc | Quy tắc |
|---|---|
| Phạm vi | Nêu chính xác năng lực/hành động bị hạn chế; không dùng cờ chung mơ hồ |
| Lý do và nguồn | Có reason code, quyết định/rule nguồn và correlation tới vụ việc nếu có |
| Thời hạn | Có hiệu lực và hết hạn rõ; vô thời hạn phải là quyết định có thẩm quyền |
| Quyền còn lại | Chỉ quyền của năng lực không bị hạn chế; session không tự mở rộng khi lớp phủ hết hạn |
| Tác động phiên | Chặn/revoke quyền liên quan ngay; không bắt buộc thu hồi phiên không liên quan trừ khi rủi ro yêu cầu |
| Gỡ hạn chế | Tái kiểm tra trạng thái tài khoản, quyền và điều kiện an toàn; ghi audit trước/sau |
| Khiếu nại | Có đường xem lý do phù hợp và gửi khiếu nại mà không lộ tín hiệu bảo mật nhạy cảm |

## Chuyển trạng thái hợp lệ

| Từ | Sang | Trigger/bằng chứng tối thiểu | Từ chối khi | Hiệu lực bắt buộc |
|---|---|---|---|---|
| Khởi tạo | Chờ xác minh thư | Dữ liệu đăng ký hợp lệ, định danh mới duy nhất và tạo tài khoản idempotent | Dữ liệu/xác minh ban đầu không hợp lệ, trùng hoặc không xác định chính sách áp dụng | Chỉ phiên giới hạn; không phát sinh quyền đầy đủ |
| Chờ xác minh thư | Hoạt động | Bằng chứng một lần còn hạn và mọi điều kiện bắt buộc hiện hành đã đạt | Bằng chứng hết hạn/dùng lại hoặc REL-01 yêu cầu điều kiện còn thiếu | Cấp quyền theo trạng thái mới ở phiên được phát hành lại; không nâng quyền token cũ ngầm |
| Chờ xác minh thư | Chờ điều kiện tuổi/đồng ý | Xác minh thư đạt nhưng kết luận REL-01 yêu cầu điều kiện chưa đạt | Chưa có kết luận REL-01 cho phạm vi phát hành | Duy trì fail-closed với năng lực nhạy cảm |
| Chờ điều kiện tuổi/đồng ý | Hoạt động | Bằng chứng đồng ý/điều kiện đúng loại, phiên bản, phạm vi và còn hiệu lực | Thiếu/rút/hết hiệu lực hoặc policy version không khớp | Phát hành lại phiên theo quyền mới; ghi phiên bản chính sách |
| Hoạt động | Tạm khóa do rủi ro | Rule/tín hiệu có nguồn và correlation đạt ngưỡng đã duyệt | Không có nguồn, rule version hoặc không ghi được audit | Chặn cấp/gia hạn và thu hồi theo phạm vi sự cố ngay |
| Hoạt động | Khóa quản trị | Actor có quyền, lý do, phạm vi/thời hạn và kiểm tra chống tự khóa trái chính sách | Thiếu quyền/lý do, xung đột phiên bản hoặc audit lỗi | Thu hồi/chặn mọi phiên; từ chối nghiệp vụ ngay |
| Hoạt động | Ngừng hoạt động lâu dài | Quyết định có thẩm quyền và lý do theo M01-D017 | Thiếu quyền, thiếu đường khiếu nại hoặc còn mutation xung đột | Thu hồi mọi phiên; dừng công khai hồ sơ |
| Tạm khóa do rủi ro | Hoạt động | Khôi phục/xác minh lại đạt và nguyên nhân đã xử lý | Chưa giải quyết tín hiệu hoặc bằng chứng không đủ | Thu hồi họ phiên cũ; đăng nhập lại bằng bằng chứng mới |
| Tạm khóa do rủi ro | Khóa quản trị | Điều tra xác nhận cần khóa có thẩm quyền | Chưa có actor/quyết định hợp lệ | Giữ chặn; ghi chuyển nguồn rủi ro sang quản trị |
| Khóa quản trị hoặc Ngừng hoạt động lâu dài | Hoạt động | Actor có quyền, điều kiện mở lại đạt và trạng thái dữ liệu còn phục hồi được | Yêu cầu xóa đã qua điểm không thể hủy hoặc nguyên nhân chưa giải quyết | Không tái dùng phiên cũ; yêu cầu đăng nhập/xác minh lại |
| Mọi trạng thái chưa xóa | Chờ xóa | Vụ việc hỗ trợ, xác minh lại, policy version và thông báo tác động | Thiếu chủ thể, ma trận dữ liệu hoặc thời gian chờ cấu hình | Thu hồi phiên thông thường; đóng băng mutation nghiệp vụ |
| Chờ xóa | Trạng thái an toàn trước đó | Hủy trong thời gian cho phép và tái kiểm tra điều kiện hiện hành | Đã qua điểm không thể hủy hoặc có phần xóa không thể phục hồi | Không phục hồi token; phát hành phiên mới sau xác minh |
| Chờ xóa | Đã xóa/ẩn danh | Job/manifest từng module và đối soát đạt | Còn phần lỗi, liên kết nhận dạng hoặc phiên/phương thức đăng nhập | Trạng thái cuối, không thể mở lại |

## Chuyển trạng thái bị cấm

| Trường hợp | Lý do |
|---|---|
| Bất kỳ trạng thái nào → Hoạt động chỉ vì token/provider còn hợp lệ | Bỏ qua nguồn sự thật trạng thái và điều kiện hiện hành |
| Chờ xác minh thư → Hoạt động khi bằng chứng hết hạn/dùng lại | Hạ chuẩn xác minh và vi phạm G01-C03 |
| Chờ điều kiện tuổi/đồng ý → Hoạt động khi REL-01 chưa kết luận | Tự suy diễn pháp lý/phạm vi phát hành |
| Khóa/Ngừng hoạt động → Hoạt động bằng thao tác client hoặc retry | Mở lại phải có thẩm quyền, điều kiện và audit |
| Chờ xóa → Hoạt động sau điểm không thể hủy | Có thể nối nhầm dữ liệu đã xóa/ẩn danh một phần |
| Đã xóa/ẩn danh → bất kỳ trạng thái cũ nào | Danh tính cũ là trạng thái cuối; đăng ký lại phải tạo định danh mới |

## Đối chiếu tĩnh hiện trạng triển khai

Kết quả dưới đây được quan sát từ mã nguồn ngày 2026-08-19. Đây là finding thiết kế/triển khai, không phải bằng chứng runtime.

| Mã | Hiện trạng quan sát | Sai lệch so với baseline | Task tiếp nhận |
|---|---|---|---|
| M01-LC-I01 | `User` chỉ lưu cờ `IsActive` | Không biểu diễn được tám trạng thái, lý do, thời hạn, phiên bản hoặc lớp phủ hạn chế | M01-T003, M01-T012, M01-T031 |
| M01-LC-I02 | Đăng ký tạo `User` với `IsActive = true` mặc định và không lưu trạng thái chờ xác minh | Có thể coi tài khoản mới là hoạt động trước khi điều kiện bắt buộc đạt | M01-T005, M01-T006, M01-T009 |
| M01-LC-I03 | `LoginAsync` và `RefreshTokenAsync` chưa thể hiện kiểm tra trạng thái tài khoản trước khi cấp token | Token có thể được cấp mà không cưỡng chế trạng thái hiện tại | M01-T010, M01-T016, M01-T017 |
| M01-LC-I04 | `GoogleLoginAsync` tự liên kết tài khoản khi email provider trùng | Vi phạm chuyển bị cấm và kỳ vọng G01-C05; email không đủ chứng minh quyền sở hữu | M01-T014, M01-T015 |
| M01-LC-I05 | `User` giữ một `RefreshToken` và thời hạn trực tiếp | Không biểu diễn họ phiên, nhiều thiết bị, rotation/reuse detection hoặc thu hồi theo phạm vi | M01-T016–T018 |
| M01-LC-I06 | Đăng nhập trực tiếp và đăng ký hiện dùng trường `Username` | Không khớp định danh đăng nhập trực tiếp bằng email trong từ điển v1.0/M01-D003 | M01-T005, M01-T010 |

## Finding và task tiếp nhận phần chưa chốt

| Mã | Phần chưa chốt | Trạng thái an toàn trong baseline này | Nguồn/task xử lý |
|---|---|---|---|
| M01-LC-F01 | Thị trường, nhóm tuổi, bằng chứng và quyền khi thiếu/rút đồng ý | Không kích hoạt nhánh hoặc mở năng lực nhạy cảm khi chưa có kết luận | REL01-Q01–Q07; M01-T007 |
| M01-LC-F03 | Thời hạn phiên quản trị, khóa, thời gian chờ xóa và điểm không thể hủy | Không đặt số giả; dùng cấu hình đã duyệt, thiếu cấu hình thì không mở hành trình liên quan | M01-T016, M01-T031, M01-T035–T036; M11 cấu hình |
| M01-LC-F04 | Cơ chế rotation, phát hiện reuse và thứ tự thu hồi access/refresh | Baseline yêu cầu chặn/revoke; không coi một refresh token là toàn bộ phiên | M01-T016–T018 |

## Tự kiểm M01-T002, A-G01 và REL-01

- M01-T001 v1.0 và A0-T001 đã hoàn thành; dependency của task được đáp ứng.
- Tám trạng thái chính có điều kiện vào/ra, hành động cho phép/từ chối, tác động phiên và hiển thị dữ liệu rõ; lớp phủ hạn chế được tách riêng.
- Mười ba chuyển hợp lệ và sáu chuyển bị cấm bao phủ đăng ký, xác minh, điều kiện tuổi/đồng ý, khóa, mở lại, ngừng hoạt động và xóa.
- Tên sáu trạng thái trọng tâm khớp ma trận A-G01; khóa/xóa không dựa vào token còn hạn và không tự mở lại.
- Ba phần chưa chốt còn lại có trạng thái an toàn và task/REL tiếp nhận; M01-LC-F02 đã đóng bằng M01-ABUSE-1.0/M01-INACTIVE-1.0, không còn điểm chờ vô chủ.
- Sáu sai lệch triển khai tĩnh có task tiếp nhận; vì chưa có sửa mã hoặc kiểm thử runtime, chúng vẫn là finding mở cho A-G01.
- REL-01 vẫn mở: nhánh tuổi/đồng ý là điều kiện, không phải kết luận pháp lý hoặc bằng chứng phát hành. Tài liệu này không kết luận A-G01 hay REL-01 đạt và không thay thế G01-C01–G01-C10.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-15 | 0.1-draft | Tạo ma trận trạng thái/chuyển trạng thái từ quyết định M01 và REL-01 draft | Chưa gán |
| 2026-08-19 | 1.0 | Chốt tám trạng thái, lớp phủ hạn chế, chuyển hợp lệ/bị cấm và chuyển phần chưa chốt sang finding có chủ | WSA-7K2 |
