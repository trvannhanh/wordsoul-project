# Thiết kế phiên bản hóa mục từ — Lát A M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-HEADWORD-VERSIONING-1.0` |
| Task | M02-T008-A |
| Đầu vào | M02-ITEM-QUALITY-1.0, M02-HEADWORD-LIFECYCLE-1.0 |
| Phạm vi | Mô hình theo dõi phiên bản chỉnh sửa mục từ (`VocabularyRevision`), mã băm nội dung `revisionDigest`, giao thức khôi phục bản cũ và bảo toàn tiến độ M04 |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả thiết kế hệ thống Phiên bản hóa Mục từ vựng (`Headword Versioning & Revision Model`) thuộc M02 cho Lát A, nhằm lưu vết toàn bộ lịch sử chỉnh sửa định nghĩa, ví dụ, âm thanh/hình ảnh của từng từ vựng, đồng thời bảo vệ tính ổn định của tiến độ học tập người học (M04).

- **Bất biến Append-Only Lịch sử Phiên bản**: Mỗi lần chỉnh sửa mục từ `Vocabulary` đã phê duyệt/xuất bản, hệ thống sinh một bản ghi mới `VocabularyRevision` với số phiên bản tăng dần nguyên tử (`versionNumber = N + 1`). TUYỆT ĐỐI CẤM ghi đè hoặc xóa các bản ghi revision cũ.
- **Mã băm Nội dung Bất biến (`revisionDigest`)**: Mỗi revision sở hữu một mã băm MD5/SHA-256 (`revisionDigest`) đại diện cho toàn bộ payload dữ liệu của mục từ tại thời điểm đó (mặt chữ, IPA, định nghĩa, ví dụ, URL media).
- **Ghim Phiên bản trong Phiên học M03 (`Session Revision Pinning`)**: Khi phiên học M03 khởi tạo, danh sách từ vựng trong phiên học được ghim cố định theo `revisionDigest` tại thời điểm tạo phiên. Việc chỉnh sửa mục từ trong CSDL giữa chừng KHÔNG làm thay đổi nội dung câu hỏi của phiên học đang diễn ra.
- **Bảo toàn Tiến độ Ôn tập SRS M04**: Tiến độ học tập của người dùng (`UserVocabularyProgress`) liên kết với định danh `VocabularyId` tổng thể. Việc nâng cấp phiên bản định nghĩa KHÔNG làm mất số lần ôn tập hay điểm ghi nhớ của người học.

## 2. Mô hình Thực thể Lịch sử Phiên bản (VocabularyRevision Schema)

```json
{
  "revisionId": 9012,
  "vocabularyId": 1024,
  "versionNumber": 3,
  "revisionDigest": "e4d909c290d0fb1ca068ffaddf22cbd0",
  "payloadJson": "{\"wordCanonical\":\"vocabulary\",\"displayWord\":\"Vocabulary\",\"pronunciationIpa\":\"/vəˈkæbjələri/\",\"cefrLevel\":\"B1\",\"senses\":[{\"senseOrder\":1,\"definitionVi\":\"Vốn từ vựng...\",\"exampleSentenceEn\":\"Expand your vocabulary.\"}]}",
  "changeSummary": "Sửa câu ví dụ tiếng Anh và bổ sung âm thanh phát âm",
  "authorId": "USR-ADMIN-007",
  "createdAtUtc": "2026-08-20T11:00:00Z"
}
```

## 3. Giao thức So sánh và Khôi phục Phiên bản (Revision Diff & Rollback Protocol)

### 3.1. So sánh Khác biệt giữa 2 Phiên bản (Revision Diff)
Giao diện quản trị M11 cung cấp tính năng xem diff giữa 2 phiên bản bất kỳ (`Rev_A` vs `Rev_B`):

```csharp
public RevisionDiffDto CompareRevisions(VocabularyRevision revA, VocabularyRevision revB)
{
    var payloadA = JsonSerializer.Deserialize<VocabularyPayload>(revA.PayloadJson);
    var payloadB = JsonSerializer.Deserialize<VocabularyPayload>(revB.PayloadJson);

    return new RevisionDiffDto
    {
        VocabularyId = revA.VocabularyId,
        FromVersion = revA.VersionNumber,
        ToVersion = revB.VersionNumber,
        FieldChanges = GetFieldDiffs(payloadA, payloadB)
    };
}
```

### 3.2. Khôi phục Phiên bản Cũ (Rollback Protocol)
Khi Quản trị viên muốn khôi phục mục từ về nội dung của Phiên bản 1 (`Rev_1`):

1. Hệ thống nạp `payloadJson` của `Rev_1`.
2. Tạo một bản ghi `VocabularyRevision` mới với `versionNumber = CurrentMaxVersion + 1`.
3. Ghi `changeSummary = "Rollback về nội dung của phiên bản #1"`.
4. Cập nhật thực thể `Vocabulary` hiện tại theo `payloadJson` đó.
5. Phát sự kiện `VocabularyUpdatedEvent` để làm mới Redis Cache.

## 4. Giao thức Tương thích với Tiến độ M04 và Phiên học M03

| Module tương tác | Cơ chế bảo toàn phiên bản | Hành vi khi có sửa đổi mục từ |
|---|---|---|
| **M03 Lesson Session** | Ghim `revisionDigest` khi tạo Session DTO | Session giữ nguyên nội dung ghim, không bị ảnh hưởng giữa chừng. |
| **M04 SRS Progress** | Liên kết theo `VocabularyId` bất biến | Tiến độ ôn tập giữ nguyên, chỉ hiển thị định nghĩa phiên bản mới nhất. |
| **M11 Content Admin** | Hiển thị full danh sách `Revisions` | Cho phép Admin xem diff, restore bản cũ và audit vết ai đã sửa. |
| **M12 Cache Store** | Cache theo `VocabularyId` kèm `revisionDigest` | Purge cache key cũ khi có revision mới được Approve/Publish. |

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `HV-G01` | Mọi lần chỉnh sửa mục từ `Vocabulary` đã xuất bản tự động tạo `VocabularyRevision` mới. |
| `HV-G02` | `versionNumber` tăng dần nguyên tử, `revisionDigest` băm chính xác payload dữ liệu. |
| `HV-G03` | Cấm xóa cứng hoặc chỉnh sửa các bản ghi `VocabularyRevision` cũ trong CSDL. |
| `HV-G04` | Phiên học M03 được ghim ghim cố định `revisionDigest` tại thời điểm tạo phiên học. |
| `HV-G05` | Tiến độ ôn tập M04 của người học giữ nguyên khi mục từ nâng cấp phiên bản mới. |
| `HV-G06` | Giao thức Rollback tạo một `versionNumber` mới (N+1), bảo toàn nguyên tắc Append-Only. |
| `HV-G07` | Admin xem được diff chi tiết giữa 2 phiên bản bất kỳ của mục từ. |
| `HV-G08` | Mọi thao tác tạo revision hoặc rollback ghi vết Audit Event bất biến trong M11. |
| `HV-G09` | Phân quyền khôi phục phiên bản tuân thủ nghiêm ngặt ma trận vai trò M11 (`R03 Content Admin`). |
| `HV-G10` | 100% các test case tự kiểm HV08-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HV08-01` | Chỉnh sửa định nghĩa của mục từ `VocabularyId_10` lần đầu | Sinh `VocabularyRevision` với `versionNumber = 1` |
| `HV08-02` | Chỉnh sửa tiếp câu ví dụ của mục từ trên lần 2 | Sinh `VocabularyRevision` với `versionNumber = 2` |
| `HV08-03` | Thử dùng lệnh SQL `UPDATE VocabularyRevisions SET payloadJson = ...` | DB deny operation, bảng lưu lịch sử bất biến |
| `HV08-04` | Khởi tạo phiên học M03 khi từ ở `versionNumber = 2` | Session ghim `revisionDigest` của version 2 |
| `HV08-05` | Admin sửa từ lên `versionNumber = 3` khi người học đang làm bài M03 | Session của người học tiếp tục dùng câu hỏi version 2 an toàn |
| `HV08-06` | Người học hoàn thành bài M03 trên | Cập nhật tiến độ M04 trỏ về `VocabularyId_10` an toàn |
| `HV08-07` | Admin xem diff giữa Version 1 và Version 3 | Trả về bảng diff chi tiết các trường bị thay đổi |
| `HV08-08` | Admin thực hiện Rollback từ Version 3 về Version 1 | Sinh `versionNumber = 4` chứa nội dung Version 1 |
| `HV08-09` | Tra cứu danh sách 4 Revisions của `VocabularyId_10` | Trả về đủ 4 bản ghi revision theo đúng thứ tự |
| `HV08-10` | Thử thực hiện Rollback về versionId không thuộc VocabularyId đó | System reject với lỗi `INVALID_REVISION_ID` |
| `HV08-11` | Kiểm tra mã băm `revisionDigest` cho 2 revision có nội dung y hệt | Trả về 2 chuỗi băm MD5/SHA-256 hoàn toàn trùng nhau |
| `HV08-12` | Chỉnh sửa mục từ đang ở trạng thái `Draft` | Không sinh `VocabularyRevision` mới, chỉ update bản nháp |
| `HV08-13` | Phê duyệt bản nháp `Draft` chuyển sang `Published` | Tạo `VocabularyRevision` chính thức đầu tiên |
| `HV08-14` | Tải đồng thời 50 request xem diff lịch sử phiên bản | Response p95 $< 20\text{ms}$ từ Redis cache |
| `HV08-15` | Xóa một bộ từ vựng chứa mục từ có 5 revisions | Các `VocabularyRevision` của mục từ đó giữ nguyên |
| `HV08-16` | User không phải Content Admin yêu cầu Rollback mục từ | Deny 403 Forbidden |
| `HV08-17` | Purge cache Redis khi sinh revision mới | Cache cũ bị xóa, nạp revision mới nhất vào cache |
| `HV08-18` | Kiểm tra tính nhất quán giữa `authorId` trong revision và Audit Log M11 | `authorId` khớp 100% với Admin thực hiện |
| `HV08-19` | Phân tích phụ thuộc trước khi Rollback mục từ | Quét kiểm tra các bộ từ bị ảnh hưởng bởi rollback (T020) |
| `HV08-20` | Kiểm thử hoàn tất luồng phiên bản hóa mục từ M02-HEADWORD-VERSIONING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-HV-I01` | Entity `Vocabulary.cs` chưa có bảng/collection `VocabularyRevisions` | Chưa lưu vết lịch sử các phiên bản chỉnh sửa | M02-T049 (Source task) |
| `M02-HV-I02` | Chưa có trường `revisionDigest` băm payload nội dung | Không thể so sánh băm nhanh giữa các phiên bản | M02-T049 |
| `M02-HV-I03` | DTO phiên học M03 chưa ghim `revisionDigest` | Rủi ro câu hỏi bị thay đổi giữa chừng khi admin sửa DB | M02-T049 |
| `M02-HV-I04` | Chưa có Service tính diff và thực thi Rollback phiên bản | Admin không thể khôi phục lại nội dung cũ khi sửa sai | M02-T049 |
| `M02-HV-I05` | Chưa lồng ghép sự kiện `VocabularyUpdatedEvent` để purge Redis Cache | Cache có thể trả về thông tin từ vựng lỗi thời | M02-T049 |

- `M02-HV-F01`: Tạo Entity `VocabularyRevision.cs` và thiết lập quan hệ 1-N với `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-HV-F02`: Triển khai `HeadwordVersioningService` hỗ trợ tính diff và Rollback (tiếp nhận: M02-T049).
- `M02-HV-F03`: Cập nhật DTO phiên học M03 hỗ trợ ghim `revisionDigest` (tiếp nhận: M02-T049).
- `M02-HV-F04`: Thiết lập bộ kiểm thử tự động HV-G01–G10 và HV08-01–20 (tiếp nhận: M02 tasks).
- `M02-HV-F05`: Thu thập bằng chứng runtime cho luồng phiên bản hóa M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T008-A

- Đã thiết kế hoàn chỉnh `M02-HEADWORD-VERSIONING-1.0` với Mô hình Thực thể `VocabularyRevision` bất biến.
- Đã chốt cơ chế băm mã `revisionDigest` và ghim phiên bản cho Phiên học M03 (`Session Revision Pinning`).
- Đã xây dựng Giao thức Xem Diff và Khôi phục Phiên bản (`Revision Diff & Rollback Protocol`).
- Đã bảo tồn nguyên tắc an toàn dữ liệu lịch sử cho tiến độ học tập SRS M04.
- Đã xác lập 10 Regression Gates (`HV-G01`–`HV-G10`) và 20 Test Cases tự kiểm (`HV08-01`–`HV08-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế phiên bản hóa mục từ M02-T008-A | WSA-7K2 |
