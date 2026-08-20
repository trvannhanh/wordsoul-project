# Đặc tả tìm kiếm người dùng an toàn M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-SAFE-USER-SEARCH-1.0` |
| Task | M11-T027 |
| Đầu vào | M11-T004 (D-004), M01-T003 (D-016), REL-07 |
| Phạm vi | Đặc tả API tìm kiếm tài khoản người dùng cho Quản trị viên & Đội hỗ trợ (CS/Support), quy tắc che giấu dữ liệu PII mặc định, giới hạn tần suất chống cào dữ liệu và ghi vết audit bất biến |
| Tự kiểm | A-G02; REL-07 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Công cụ và Quy trình Tìm kiếm Người dùng An toàn (`Safe Admin User Search Engine`) thuộc M11, phục vụ công tác hỗ trợ khách hàng, giải quyết vụ việc (Ticket REL-07) và quản trị tài khoản người học mà không làm lộ thông tin cá nhân PII ra màn hình ngoài phạm vi cần thiết.

- **Cấm Tìm kiếm Wildcard Toàn bộ CSDL (`No Unbounded Wildcard Search Invariant`)**: API tìm kiếm CẤM các câu lệnh query không có điều kiện hoặc wildcard quá ngắn ($< 3$ ký tự) gây quá tải DB và rủi ro rò rỉ dữ liệu danh sách hàng loạt.
- **Che mờ Dữ liệu Căn cước Mặc định (`PII Masking by Default`)**: Kết quả tìm kiếm trả về màn hình Admin mặc định được che mờ email (`t***v@gmail.com`) và số điện thoại (`09****1234`). Chỉ hiển thị đầy đủ PII khi có mã ngữ cảnh Vụ việc Hỗ trợ hợp lệ (`ticketId`) được xác minh.
- **Giới hạn Tần suất Tra cứu Chống Cào Dữ liệu (`Anti-Scraping Rate Limiter`)**: Mỗi Nhân viên Hỗ trợ/Admin chỉ được phép thực hiện tối đa 10 lượt tìm kiếm/phút. Khi vượt ngưỡng, hệ thống tự động khóa tính năng tra cứu trong 15 phút và phát cảnh báo an ninh.
- **Lưu vết Kiểm toán Tìm kiếm Bất biến (`Search Audit Trail`)**: 100% lệnh tìm kiếm do Admin/Support thực hiện được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-27`), bao gồm `ActorId`, `SearchTerm` (đã mã hóa hash), `TicketId` và `ResultsCount`.

## 2. Các Phương thức Tìm kiếm Hợp lệ (Valid Search Criteria)

| Loại Tìm kiếm | Thuộc tính Khảo sát | Kiểu Khớp | Chi tiết Validation | Rủi ro PII |
|---|---|---|---|---|
| theo User ID | `UserId` (int) / `UserPublicId` (GUID) | Khớp chính xác (Exact) | Số nguyên dương hoặc GUID | Thấp |
| theo Email | `CanonicalEmail` (string) | Khớp chính xác (Exact) | Chuẩn hóa lowercase trim email | Cao (Che mờ mặc định) |
| theo Số điện thoại | `PhoneNumber` (string) | Khớp chính xác (Exact) | Định dạng E.164 (`+84...`) | Cao (Che mờ mặc định) |
| theo Tên hiển thị | `DisplayName` (string) | Khớp chuỗi (Contains) | Tối thiểu $\ge 3$ ký tự | Trung bình |
| theo Mã Vụ việc | `TicketId` (string) | Khớp chính xác (Exact) | Mã Ticket CS hợp lệ (M11-T029) | Thấp (Liên kết vụ việc) |

## 3. Cấu trúc Response DTO Tìm kiếm An toàn (SafeUserSearchResultDto Schema)

```json
{
  "searchContext": {
    "ticketId": "TCK-2026-0820-001",
    "searchTimestampUtc": "2026-08-20T10:00:00Z",
    "searchedByActorId": "USR-SUP-003",
    "unmaskReason": "Khôi phục tài khoản theo yêu cầu khách hàng"
  },
  "totalMatches": 1,
  "users": [
    {
      "userId": 10024,
      "userPublicId": "01J5XA00000000000000000099",
      "displayName": "Tran Van Nhanh",
      "maskedEmail": "t***v@gmail.com",
      "unmaskedEmail": "tranv@gmail.com", 
      "maskedPhone": "09****1234",
      "accountStatus": "Active",
      "role": "Learner",
      "registeredAtUtc": "2026-01-15T08:30:00Z",
      "identityHash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ]
}
```

*Lưu ý*: Trường `unmaskedEmail` CHỈ XUẤT HIỆN trong DTO nếu request có chứa `ticketId` hợp lệ và Nhân viên có vai trò `R07 Support Agent` hoặc `R12 Security Admin`.

## 4. Kiến trúc Bộ lọc Tìm kiếm và Ghi vết Kiểm toán (Search Pipeline & Audit)

```
[Support Agent Search Request]
               |
               v
  [Check Anti-Scraping Rate Limiter] (Max 10 req/min)
               |
         +-----+-----+
         | (Exceeded) | (Valid)
         v            v
  [Block 15m & Alert] [Validate Search Term Length >= 3]
                              |
                              v
                   [Execute DB Query & Unmask Policy]
                   - If valid ticketId -> Reveal PII
                   - Else -> Mask Email & Phone
                              |
                              v
                   [Record Audit Event ACT-M11-27]
                   - ActorId, TicketId, SearchTermHash
                              |
                              v
                   [Return SafeUserSearchResultDto]
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `US-G01` | Cấm tìm kiếm chuỗi tên hiển thị có độ dài $< 3$ ký tự hoặc tìm kiếm rỗng. |
| `US-G02` | Mặc định 100% email và số điện thoại trả về màn hình Admin được che mờ (`t***v@gmail.com`). |
| `US-G03` | Chỉ bỏ che mờ (`unmaskedEmail`) khi request đi kèm mã `ticketId` hợp lệ thuộc vụ việc mở. |
| `US-G04` | Giới hạn tần suất tra cứu tối đa 10 request/phút cho mỗi Nhân viên Hỗ trợ. |
| `US-G05` | Vượt quá 10 request/phút tự động khóa tính năng tra cứu trong 15 phút và cảnh báo an ninh. |
| `US-G06` | 100% thao tác tìm kiếm người dùng ghi vết bất biến vào Audit Log M11 (`ACT-M11-27`). |
| `US-G07` | Chuỗi từ khóa tìm kiếm (`SearchTerm`) trong Audit Log được mã hóa băm SHA-256 (REL-03). |
| `US-G08` | Tìm kiếm theo Email chỉ chấp nhận khớp chính xác (`Exact Match`), cấm wildcard `%email%`. |
| `US-G09` | Phân quyền tra cứu tuân thủ ma trận vai trò M11 (`R07 Support Agent`, `R03 Content Admin`, `R12 Security Admin`). |
| `US-G10` | 100% các test case tự kiểm US27-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `US27-01` | Tìm kiếm người dùng theo `UserId = 10024` | Trả về thông tin người học với email che mờ `t***v@gmail.com` |
| `US27-02` | Tìm kiếm theo email chính xác `"tranv@gmail.com"` không kèm `ticketId` | Trả về người học, `maskedEmail = "t***v@gmail.com"`, `unmaskedEmail = null` |
| `US27-03` | Tìm kiếm theo email kèm mã vụ việc hợp lệ `ticketId = "TCK-108"` | Trả về người học kèm cả `maskedEmail` và `unmaskedEmail` |
| `US27-04` | Thử tìm kiếm với mã `ticketId` đã đóng hoặc không tồn tại | System reject bỏ che mờ, trả về kết quả che mờ mặc định |
| `US27-05` | Thử tìm kiếm tên hiển thị chỉ có 2 ký tự `"an"` | Reject với lỗi `SEARCH_TERM_TOO_SHORT` |
| `US27-06` | Thử tìm kiếm email bằng cú pháp wildcard `"%@gmail.com%"` | Reject với lỗi `WILDCARD_NOT_ALLOWED_FOR_EMAIL` |
| `US27-07` | Nhân viên Hỗ trợ A thực hiện 11 lượt tìm kiếm trong 30 giây | Request thứ 11 bị chặn với lỗi `RATE_LIMIT_EXCEEDED` |
| `US27-08` | Nhân viên A bị vi phạm rate limit thử tìm kiếm lượt 12 | Hệ thống khóa tính năng tra cứu của A trong 15 phút |
| `US27-09` | Tra cứu vết Audit Log M11 sau 1 lượt tìm kiếm thành công | Ghi nhận Audit Event `ACT-M11-27` với `SearchTermHash` |
| `US27-10` | Tra cứu thông tin PII từ Audit Log M11 | Audit Log không chứa email/phone thô (REL-03) |
| `US27-11` | Tìm kiếm người dùng đang ở trạng thái `Locked` | Trả về kết quả với `accountStatus = "Locked"` |
| `US27-12` | Tìm kiếm tên hiển thị `"Tran Van Nhanh"` | Trả về danh sách kết quả phân trang (PageSize max 20) |
| `US27-13` | Tải đồng thời 50 request tìm kiếm từ 50 Support Agent khác nhau | Response p95 $< 30\text{ms}$ |
| `US27-14` | User người học thường thử gọi API tìm kiếm người dùng Admin | Deny 403 Forbidden |
| `US27-15` | Tìm kiếm theo số điện thoại `"+84912345678"` không kèm ticket | Trả về kết quả với `maskedPhone = "09****5678"` |
| `US27-16` | Hết thời gian khóa 15 phút do vi phạm rate limit | Mở lại quyền tra cứu cho Support Agent |
| `US27-17` | Phân tích tham chiếu trước khi khóa 1 tài khoản từ kết quả tìm kiếm | Chuyển tiếp sang luồng khóa tài khoản M01-T031 (T020) |
| `US27-18` | Tìm kiếm người dùng không tồn tại trong hệ thống | Trả về kết quả rỗng `totalMatches = 0`, không crash |
| `US27-19` | Xem vết Audit Log M11 khi phát hiện hành vi cào dữ liệu | Ghi nhận cảnh báo an ninh `ACT-M11-13` |
| `US27-20` | Kiểm thử hoàn tất luồng đặc tả tìm kiếm người dùng an toàn M11-SAFE-USER-SEARCH-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-US-I01` | API tìm kiếm người dùng hiện tại chưa che mờ Email và Phone mặc định | Rủi ro rò rỉ dữ liệu cá nhân PII khi nhân viên mở màn hình | M11-T049 (Source task) |
| `M11-US-I02` | Chưa lồng ghép kiểm tra `ticketId` để quyết định bỏ che mờ PII | Nhân viên Hỗ trợ có thể xem PII mà không cần ngữ cảnh vụ việc | M11-T049; REL-07 |
| `M11-US-I03` | Thiếu Anti-Scraping Rate Limiter (tối đa 10 req/min/agent) | Rủi ro bị tài khoản Admin/Support lạm dụng để cào dữ liệu user | M11-T049 |
| `M11-US-I04` | Lệnh tìm kiếm chưa được ghi vết Audit Event `ACT-M11-27` | Chưa đáp ứng 100% tiêu chí kiểm toán vết tra cứu REL-07 | M11-T049; REL-07 |
| `M11-US-I05` | Chưa có validation chặn wildcard trên email | Rủi ro làm chậm CSDL khi tìm kiếm dạng `%string%` | M11-T049 |

- `M11-US-F01`: Triển khai `SafeUserSearchService` với PII Masking mặc định (tiếp nhận: M11-T049).
- `M11-US-F02`: Tích hợp `TicketContextVerificationFilter` cho luồng unmask PII (tiếp nhận: M11-T049; REL-07).
- `M11-US-F03`: Thiết lập Anti-Scraping Rate Limiter 10 req/min (tiếp nhận: M11-T049).
- `M11-US-F04`: Thiết lập bộ kiểm thử tự động US-G01–G10 và US27-01–20 (tiếp nhận: M11 tasks).
- `M11-US-F05`: Thu thập bằng chứng runtime cho luồng tìm kiếm an toàn M11 (tiếp nhận: M11 tasks; A-G02/REL-07).

## 8. Tự kiểm M11-T027

- Đã thiết kế hoàn chỉnh `M11-SAFE-USER-SEARCH-1.0` với Các Phương thức Tìm kiếm Hợp lệ và JSON Schema DTO.
- Đã chốt Ràng buộc Che mờ PII Mặc định (`maskedEmail`, `maskedPhone`) và chỉ Unmask khi có `ticketId` hợp lệ.
- Đã xây dựng Anti-Scraping Rate Limiter tối đa 10 req/min và khóa 15 phút khi vi phạm.
- Đã lồng ghép Sổ Kiểm toán Tìm kiếm Bất biến `ACT-M11-27` mã hóa hash từ khóa.
- Đã xác lập 10 Regression Gates (`US-G01`–`US-G10`) và 20 Test Cases tự kiểm (`US27-01`–`US27-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả tìm kiếm người dùng an toàn M11-T027 | WSA-7K2 |
