# Đánh giá hiện trạng A-WP02 — M02 Nội dung từ vựng và bộ từ

## 1. Phạm vi và phương pháp

Tài liệu đối chiếu 29 task được chọn cho A-WP02 với mã nguồn, cấu trúc dữ liệu và kiểm thử hiện có tại ngày 2026-08-14. Phạm vi gồm M02-T001–M02-T023 và M02-T029–M02-T034.

Đây là đánh giá tĩnh, chỉ đọc. Trạng thái phản ánh mức đáp ứng toàn bộ Definition of Done của task, không chỉ việc đã có màn hình hoặc thao tác tạo–sửa–xóa học liệu.

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
| Đáp ứng một phần | 20 | 69,0% |
| Chưa có | 9 | 31,0% |
| Không còn phù hợp | 0 | 0% |
| Tổng | 29 | 100% |

Hệ thống đã có nền học liệu vận hành được: mục từ với nghĩa, loại từ, CEFR, ví dụ và tài sản; bộ từ có chủ sở hữu, chủ đề, độ khó, công khai/riêng tư; thành phần có thứ tự và nội dung ghi đè; người dùng có thể thêm bộ vào thư viện. Tuy nhiên, mô hình hiện tại vẫn coi một bản ghi từ là một đơn vị nguyên khối, chưa tách nghĩa ổn định, không phiên bản hóa và chưa có vòng đời kiểm duyệt. Chủ sở hữu có thể công khai trực tiếp, còn cập nhật/xóa sửa ngay dữ liệu đang được module học sử dụng.

## 3. Bằng chứng hiện trạng nổi bật

- Mục từ hiện lưu mặt chữ, một nghĩa, loại từ, CEFR, mô tả, ví dụ và các URL tài sản trong cùng một bản ghi tại [Vocabulary.cs](../../../WordSoul.Domain/Entities/Vocabulary.cs:9). Không có định danh nghĩa hoặc phiên bản học liệu.
- Chỉ mục tìm kiếm hiện có cho mặt chữ và cặp loại từ–CEFR, nhưng không phải ràng buộc duy nhất theo mặt chữ/nghĩa tại [WordSoulDbContext.cs](../../../WordSoul.Infrastructure/Persistence/WordSoulDbContext.cs:308). Vì vậy có thể có nhiều bản ghi cùng từ nhưng không có quan hệ học thuật giữa các nghĩa.
- Luồng AI chuẩn hóa chữ thường, bỏ trùng theo mặt chữ và tái sử dụng bản ghi đầu tiên tìm thấy; khác nghĩa có nguy cơ bị hợp nhất tại [VocabularySetService.cs](../../../WordSoul.Application/Services/VocabularySetService.cs:154).
- Bộ từ có tiêu đề, chủ đề, mô tả, ảnh, độ khó, chủ sở hữu, cờ hoạt động và cờ công khai tại [VocabularySet.cs](../../../WordSoul.Domain/Entities/VocabularySet.cs:8), nhưng không có trạng thái chờ duyệt/cần sửa/thu hồi hoặc phiên bản.
- Người dùng thuộc ba vai trò chính đều có thể tạo bộ; quyền sửa/xóa chủ yếu dựa vào chủ sở hữu. Chủ sở hữu vai trò User có thể tự chuyển bộ sang công khai tại [VocabularySetController.cs](../../../WordSoul.Api/Controllers/VocabularySetController.cs:403) và [VocabularySetService.cs](../../../WordSoul.Application/Services/VocabularySetService.cs:564).
- Thành phần bộ chống thêm trùng ở tầng xử lý, lấy thứ tự lớn nhất rồi cộng một tại [SetVocabularyService.cs](../../../WordSoul.Application/Services/SetVocabularyService.cs:105). Chưa có ràng buộc duy nhất cho thứ tự trong một bộ hoặc xử lý hai thay đổi đồng thời.
- Nội dung ghi đè nghĩa, ví dụ, phát âm và mô tả được giữ trên quan hệ bộ–mục từ, không sửa nội dung chuẩn khi dùng đúng luồng tại [SetVocabulary.cs](../../../WordSoul.Domain/Entities/SetVocabulary.cs:16). Tuy nhiên, một luồng khác cho phép chủ bộ sửa trực tiếp nội dung lõi dùng chung.
- Phiên học chụp danh sách định danh và thứ tự mục từ, nhưng tiếp tục đọc nội dung từ bản ghi mục từ hiện hành; không chụp phiên bản nghĩa/tài sản/ghi đè tại [LearningSessionService.cs](../../../WordSoul.Application/Services/LearningSessionService.cs:229) và [SessionVocabulary.cs](../../../WordSoul.Domain/Entities/SessionVocabulary.cs:8).
- Thư viện kiểm tra bộ công khai hoặc thuộc chính người dùng và ngăn thêm trùng ở tầng xử lý tại [UserVocabularySetService.cs](../../../WordSoul.Application/Services/UserVocabularySetService.cs:31). Bỏ bộ hiện xóa quan hệ thư viện, chưa mô tả tác động phiên đang chạy và chính sách giữ tiến độ.
- Không tìm thấy mô hình gửi duyệt, checklist, quyết định cần sửa/từ chối, báo cáo nội dung, thu hồi theo mức hoặc khiếu nại. Cờ công khai/hoạt động và xóa trực tiếp hiện thay thế các vòng đời này.
- Kiểm thử hiện tập trung mạnh vào phiên học và tiến độ; chưa thấy bộ nghiệm thu riêng cho phiên bản học liệu, quyền xuất bản, kiểm duyệt, báo cáo/thu hồi và thay đổi bộ đang được học.

## 4. Ma trận đánh giá 29 task

### 4.1. Mô hình mục từ và chất lượng học liệu

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T001 | Đáp ứng một phần | Tài liệu M02 và mô hình hiện tại đã dùng các khái niệm mục từ, bộ từ, thành phần, công khai và ghi đè | Chưa có từ điển được duyệt với một nghĩa duy nhất cho “mục từ”, “nghĩa”, “biến thể”, “phiên bản”, “chuẩn”, “riêng tư” và chủ sở hữu thuật ngữ | Cao |
| M02-T002 | Đáp ứng một phần | Mỗi bản ghi có mặt chữ, nghĩa, loại từ và CEFR; dữ liệu cho phép tồn tại nhiều bản ghi cùng mặt chữ | Không có thực thể/định danh nghĩa ổn định, quan hệ giữa các nghĩa, biến thể cách dùng hoặc cơ chế để module tiêu thụ chọn đúng nghĩa | Nghiêm trọng |
| M02-T003 | Đáp ứng một phần | Một số luồng trim, chuyển chữ thường và loại giá trị lặp | Quy tắc không đồng nhất giữa tạo thủ công, tìm kiếm và AI; chưa xử lý khoảng trắng nội bộ, dấu câu, chính tả/giọng, cụm từ và trường hợp mơ hồ | Cao |
| M02-T004 | Đáp ứng một phần | Có tra cứu không phân biệt hoa thường và chống trùng định danh trong cùng bộ | Phát hiện trùng chỉ dựa nhiều vào mặt chữ; chưa phân loại trùng chắc chắn, gần giống, khác nghĩa và quyết định tái sử dụng/thêm nghĩa/tạo mới | Nghiêm trọng |
| M02-T005 | Đáp ứng một phần | Mục từ có CEFR và bộ có độ khó được chọn | Chưa có tiêu chí gán CEFR theo nghĩa, quy trình xác minh học thuật hoặc công thức suy ra độ khó bộ từ thành phần | Cao |
| M02-T006 | Đáp ứng một phần | Tạo mục từ yêu cầu mặt chữ, nghĩa và loại từ; có các trường phát âm, mô tả, ví dụ và tài sản | Thiếu checklist riêng cho bản nháp/công khai, giới hạn trường, kiểm tra ví dụ khớp nghĩa/trình độ, ngôn ngữ, phát âm và bằng chứng chất lượng | Rất cao |

### 4.2. Vòng đời, phiên bản và hợp đồng tiêu thụ

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T007 | Đáp ứng một phần | Có phân biệt nội dung tùy chỉnh; bộ có cờ công khai và hoạt động | Mục từ không có vòng đời; bộ không có nháp/chờ duyệt/cần sửa/thu hồi; quyền và tác động chuyển trạng thái chưa được cưỡng chế | Nghiêm trọng |
| M02-T008 | Chưa có | Nội dung có thời điểm tạo ở cấp bộ nhưng không có phiên bản học liệu | Sửa trực tiếp bản hiện hành; thiếu bản bất biến, thời điểm hiệu lực, lịch sử, bản nháp tách khỏi bản đang dùng và khả năng tra cứu phiên cũ | Nghiêm trọng |
| M02-T009 | Đáp ứng một phần | Các kết quả đọc cung cấp mặt chữ, nghĩa, loại từ, CEFR, ví dụ, ảnh và âm thanh cho module khác | Thiếu định danh nghĩa/phiên bản, quy tắc trường bắt buộc/tùy chọn và hành vi khi thiếu; hợp đồng không bảo đảm nội dung ổn định xuyên phiên | Rất cao |
| M02-T010 | Chưa có | Quan hệ dữ liệu bảo vệ một số mục từ đang có lịch sử khỏi xóa ngoài ý muốn | Không có trạng thái ngừng dùng, mục thay thế, hợp nhất nghĩa, bản đồ nơi sử dụng, thông báo tác động và truy vết thu hồi | Nghiêm trọng |

### 4.3. Tài sản học liệu

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T011 | Đáp ứng một phần | Có URL ảnh, âm thanh từ và âm thanh câu ví dụ; nguồn gồm tải lên, tìm ảnh và tổng hợp giọng | Chưa có catalog gắn mục đích, nguồn, quyền, giọng/ngôn ngữ, trạng thái, thời hạn và chủ sở hữu cho từng tài sản | Rất cao |
| M02-T012 | Chưa có | Quản trị viên có thể nhập hoặc thay tài sản | Không có checklist ảnh đúng nghĩa, audio đúng nội dung/giọng, kết quả đạt–sửa–từ chối, người duyệt và bằng chứng nguồn | Rất cao |
| M02-T013 | Đáp ứng một phần | AI/tìm ảnh/tổng hợp giọng có thể trả thiếu; luồng tạo theo lô ghi nhận từ thất bại và tiếp tục phần còn lại | Chưa phân biệt tài sản bắt buộc/tùy chọn, placeholder được duyệt, trạng thái chưa hoàn chỉnh, retry an toàn và quan sát lỗi theo tài sản | Cao |
| M02-T014 | Chưa có | Cập nhật có thể thay URL ảnh trong bản ghi | Không có xem trước, phiên bản tài sản, liên kết với phiên học lịch sử, giữ/xóa theo chính sách, tham chiếu và dọn tài sản cũ | Rất cao |

### 4.4. Bộ từ, quyền và chủ sở hữu

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T015 | Đáp ứng một phần | Bộ có tiêu đề, chủ đề, mô tả, độ khó, ảnh, cờ hoạt động/công khai và danh sách mục từ | Thiếu mục tiêu học, checklist nháp/công khai, giới hạn quy mô có lý do và quy tắc kiểm tra độ khó theo thành phần | Cao |
| M02-T016 | Đáp ứng một phần | Có phân quyền theo vai trò và kiểm tra chủ sở hữu cho nhiều thao tác sửa thành phần/bộ | Ma trận chưa bao phủ xem riêng tư, gửi duyệt, duyệt, thu hồi và xóa; quyền giữa Admin/SuperAdmin/User không nhất quán ở một số thao tác | Nghiêm trọng |
| M02-T017 | Đáp ứng một phần | Bộ có IsPublic/IsActive và thao tác chuyển sang công khai | Hai cờ không biểu diễn đầy đủ nháp, riêng tư, chờ duyệt, cần sửa, công khai, thu hồi; chủ bộ có thể vượt cổng duyệt | Nghiêm trọng |
| M02-T018 | Đáp ứng một phần | Bộ có CreatedBy; quan hệ chủ sở hữu dùng hạn chế xóa dây chuyền khi tài khoản bị xóa | Chưa có đóng băng/chuyển giao/chủ thay thế, xử lý riêng bộ private/public và bảo vệ dữ liệu khi chủ bị khóa/xóa | Rất cao |
| M02-T019 | Chưa có | Không tìm thấy luồng sao chép/phái sinh bộ từ | Thiếu nguồn gốc, quyền sửa bản sao, liên hệ phiên bản nguồn và hành vi khi nguồn bị thu hồi | Cao |

### 4.5. Thành phần bộ và tính ổn định khi học

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T020 | Đáp ứng một phần | Chủ bộ có thể thêm mục mới/có sẵn, chống thêm trùng ID và bỏ mục | Chưa chặn mục bị thu hồi, chưa lưu lịch sử thành phần hoặc xem trước tác động; một số đường tạo mục mới có thể tạo trùng nghĩa | Rất cao |
| M02-T021 | Đáp ứng một phần | Thành phần có Order; tạo/cập nhật danh sách gán thứ tự tuần tự | Chưa có ràng buộc duy nhất theo bộ–thứ tự, thao tác sắp xếp rõ ràng, xử lý xung đột hoặc kiểm thử thay đổi đồng thời; cách lấy max+1 có thể va chạm | Rất cao |
| M02-T022 | Đáp ứng một phần | Quan hệ bộ–mục từ có ghi đè nghĩa, ví dụ, phát âm và mô tả; kết quả đọc ưu tiên giá trị ghi đè | Thiếu nguồn, phiên bản, kiểm duyệt và danh sách trường được phép; luồng sửa nội dung lõi từ bộ có thể làm thay đổi mọi bộ dùng chung | Nghiêm trọng |
| M02-T023 | Đáp ứng một phần | Phiên đang chạy giữ danh sách ID và thứ tự mục từ; lịch sử học liên kết ID ổn định | Không chụp nghĩa/tài sản/ghi đè/phiên bản; sửa hoặc xóa bộ/mục có thể đổi nội dung giữa phiên; thiếu thời điểm hiệu lực và thông báo người học | Nghiêm trọng |

### 4.6. Gửi duyệt, xuất bản, hậu kiểm và khiếu nại

| Task | Trạng thái | Bằng chứng hiện có | Khoảng trống cần xử lý | Rủi ro |
|---|---|---|---|---|
| M02-T029 | Chưa có | Không tìm thấy yêu cầu gửi duyệt hoặc bản khóa để duyệt | Thiếu kiểm tra điều kiện, trạng thái gửi, phiên bản bất biến, hành vi sửa sau gửi và hiển thị tiến độ cho người tạo | Nghiêm trọng |
| M02-T030 | Chưa có | Không tìm thấy checklist hoặc hồ sơ kiểm duyệt | Thiếu tiêu chí học thuật, quyền, tài sản, an toàn, độ khó, lý do và xử lý xung đột lợi ích | Nghiêm trọng |
| M02-T031 | Chưa có | Không có quyết định cần sửa/từ chối gắn phiên bản | Thiếu lý do theo tiêu chí, lịch sử, sửa thành bản mới, gửi lại và bảo vệ bản công khai cũ | Rất cao |
| M02-T032 | Đáp ứng một phần | Có thao tác công khai và kiểm tra chủ sở hữu; gửi lặp không đổi thêm khi đã public | Chủ sở hữu tự công khai bản đang sửa; không cần bản đã duyệt, phiên bản, thời điểm hiệu lực hoặc thông báo phiên bản cho module tiêu thụ | Nghiêm trọng |
| M02-T033 | Đáp ứng một phần | Có thể đặt bộ không hoạt động, chuyển trạng thái hiển thị hoặc xóa trực tiếp | Không có báo cáo người dùng, mức khẩn cấp, điều tra độc lập, tạm ẩn/thu hồi có lịch sử, SLA và ma trận tác động M03/M04/M05/M08 | Nghiêm trọng |
| M02-T034 | Chưa có | Không tìm thấy hồ sơ khiếu nại quyết định nội dung | Thiếu thời hạn, người xét phù hợp, bằng chứng, kết quả cuối và bảo đảm không tự công khai khi chờ | Cao |

## 5. Sai lệch nghiêm trọng cần ưu tiên

| Thứ tự | Sai lệch | Task liên quan | Tác động |
|---:|---|---|---|
| 1 | Người tạo có thể tự công khai bộ mà không qua gửi duyệt, kiểm tra tự động hoặc người duyệt | M02-T017, M02-T029–M02-T032 | Nội dung sai, không an toàn hoặc không đủ quyền được phân phối trực tiếp tới người học |
| 2 | Không có định danh nghĩa và quy tắc đa nghĩa; tái sử dụng AI dựa trên mặt chữ | M02-T002–M02-T004 | Hợp nhất sai nghĩa, câu hỏi/chấm đáp án sai ngữ cảnh và làm hỏng tiến độ học |
| 3 | Mục từ và bộ không có phiên bản bất biến; cập nhật sửa trực tiếp bản đang dùng | M02-T007–M02-T010, M02-T023 | Phiên học/lịch sử không tái hiện được và nội dung có thể đổi giữa phiên |
| 4 | Phiên học chỉ giữ ID, không giữ ảnh chụp nghĩa/tài sản/ghi đè | M02-T009, M02-T023 | Người học thấy dữ liệu khác nhau trong cùng phiên; khó giải thích kết quả cũ |
| 5 | Không có báo cáo, tạm ẩn, thu hồi theo mức hoặc khiếu nại | M02-T031–M02-T034 | Không phản ứng có kiểm soát khi nội dung công khai sai hoặc bị tranh chấp |
| 6 | Tài sản chỉ là URL, không có catalog/kiểm duyệt/vòng đời | M02-T011–M02-T014 | Sai nghĩa, sai giọng, mất tài sản lịch sử, vi phạm quyền và orphan |
| 7 | Thứ tự max+1 và thiếu ràng buộc duy nhất/xung đột đồng thời | M02-T020–M02-T021 | Thứ tự trùng, mất cập nhật và phiên học nhận danh sách không xác định |
| 8 | Chủ bộ có đường sửa trực tiếp nội dung lõi dùng chung | M02-T022 | Một thay đổi cục bộ có thể làm biến đổi học liệu của bộ và người học khác |

## 6. Thứ tự nâng cấp đề xuất

### Nhóm 1 — Khóa đường công khai không kiểm duyệt

Ưu tiên M02-T007, M02-T016–M02-T017 và M02-T029–M02-T034. Trước khi có cổng đầy đủ, nội dung người dùng/AI không được tự trở thành học liệu công khai.

### Nhóm 2 — Tách nghĩa và phiên bản hóa học liệu

Thực hiện M02-T001–M02-T010. Chốt định danh nghĩa, chuẩn hóa/trùng, tiêu chí chất lượng, trạng thái và phiên bản bất biến trước khi mở rộng tạo nội dung.

### Nhóm 3 — Ổn định hợp đồng với phiên học

Thực hiện M02-T009–M02-T010 và M02-T020–M02-T023 cùng M03/M04. Phiên đang chạy phải dùng ảnh chụp học liệu rõ ràng; thay đổi tương lai có phiên bản và thời điểm hiệu lực.

### Nhóm 4 — Quản lý tài sản và chủ sở hữu

Thực hiện M02-T011–M02-T014, M02-T018–M02-T019 cùng A-WP01/A-WP04. Tài sản và bản phái sinh cần nguồn, trạng thái, quyền, vòng đời và chủ sở hữu hợp lệ.

### Nhóm 5 — Nghiệm thu dữ liệu bộ từ

Hoàn thiện M02-T015, M02-T020–M02-T022: giới hạn quy mô, độ khó, chống trùng theo nghĩa, thứ tự duy nhất, ghi đè có phạm vi và kiểm duyệt.

## 7. Cổng A-WP02

A-WP02 chưa đạt Cổng A. Điều kiện tối thiểu để chuyển trạng thái:

- Mỗi nghĩa có định danh ổn định, loại từ, CEFR, ví dụ và quy tắc chuẩn hóa/phát hiện trùng; không tái sử dụng chỉ vì trùng mặt chữ.
- Mục từ và bộ có vòng đời, phiên bản bất biến, thời điểm hiệu lực và bản cũ tra cứu được.
- Nội dung người dùng/AI không thể tự công khai; có kiểm tra tự động tối thiểu, gửi duyệt, checklist, cần sửa/từ chối và xuất bản đúng phiên bản.
- Có báo cáo, tạm ẩn, thu hồi theo mức, người xử lý độc lập, tác động xuyên module và khiếu nại có lịch sử.
- Phiên học chụp phiên bản học liệu cần thiết; cập nhật bộ không đổi nội dung của phiên đang chạy.
- Tài sản A/B có catalog, trạng thái, kiểm duyệt, placeholder và vòng đời theo A-WP04.
- Thành phần bộ chống trùng theo nghĩa, thứ tự duy nhất, xử lý đồng thời và ghi đè không làm đổi nội dung chuẩn.
- Chủ sở hữu bị khóa/xóa có hành vi xác định; không lộ bộ riêng tư và không tạo nội dung công khai vô chủ.

## 8. Giới hạn đánh giá

- Chưa chạy kiểm thử hoặc đánh giá dữ liệu thực tế, chất lượng học thuật, giao diện quản trị/web/mobile và hệ thống quan sát bên ngoài.
- Đánh giá tập trung WordSoulApi; kiểm thử được đọc để xác định phạm vi bao phủ, không dùng kết quả chạy làm bằng chứng.
- M02-T024–M02-T028 thuộc nhóm AI chưa chọn trong Giai đoạn A/B; M02-T035–M02-T044 thuộc B-WP01 nên không tính trong 29 task A-WP02.
- Việc chấp nhận rủi ro bản quyền không thay thế yêu cầu ghi nguồn/trạng thái/vòng đời vận hành trong phạm vi tài sản A/B.
