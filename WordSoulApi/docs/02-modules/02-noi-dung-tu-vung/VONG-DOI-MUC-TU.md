# Xác định vòng đời mục từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-HEADWORD-LIFECYCLE-1.0` |
| Task | M02-T007 |
| Đầu vào | M02-VOCAB-DICT-1.0, M11-CONTENT-LIFECYCLE-1.0 (D-051), REL-04 |
| Phạm vi | Ma trận 8 trạng thái vòng đời mục từ, điều kiện chuyển trạng thái và tích hợp các chốt kiểm tra bản quyền/chất lượng |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập quy trình quản lý vòng đời chuyển trạng thái của Mục từ vựng Master (`Vocabulary Headword Lifecycle`) trong M02, tương thích hoàn toàn với Mô hình Vòng đời Nội dung Quản trị M11 (`M11-CONTENT-LIFECYCLE-1.0`).

- **Đồng bộ 8 Trạng thái Vòng đời chuẩn M11**:
  `Draft` $\to$ `Submitted` $\to$ `InReview` $\to$ `Approved` / `Rejected` $\to$ `Scheduled` $\to$ `Published` $\to$ `Deprecated` $\to$ `Archived` / `Recalled`.
- **Cấm Tự Phê duyệt (`Self-Approval Guard`)**: Tác giả khởi tạo bản thảo mục từ (`CreatorId`) KHÔNG ĐƯỢC PHÉP tự phê duyệt (`Approved`) hoặc tự xuất bản (`Published`) mục từ của mình (Quy tắc EC-2/3/4 của M11).
- **Ràng buộc Chất lượng & Bản quyền trước xuất bản**: Mục từ chỉ được phép chuyển trạng thái `Approved` / `Published` khi đáp ứng đồng thời `QualityScore >= 80%` (M02-T006) và `rightsCleared == true` (REL-04 / CT-01).
- **Giao thức Thu hồi Khẩn cấp ($\le 60\text{s}$ Emergency Recall)**: Khi có lệnh thu hồi khẩn cấp từ Admin (`Recalled`), mục từ lập tức được gỡ khỏi Cache công khai và vô hiệu hóa khỏi các phiên học mới trong vòng tối đa 60 giây.

## 2. Ma trận 8 Trạng thái Vòng đời Mục từ vựng

| Trạng thái | Mô tả ý nghĩa | Thao tác cho phép | Ai có quyền chuyển? | Điều kiện kiểm tra bắt buộc |
|---|---|---|---|---|
| `Draft` | Bản thảo đang biên soạn | Chỉnh sửa nội dung, xóa bản thảo | Tác giả biên soạn (R03 Content Admin) | Không kiểm tra |
| `Submitted` | Đã gửi chờ phân công duyệt | Xem bản thảo, rút lại bản thảo | Tác giả biên soạn | Kiểm tra tính đầy đủ trường bắt buộc |
| `InReview` | Đang trong quá trình kiểm duyệt | Đánh giá chất lượng, ghi chú sửa đổi | Biên tập viên được phân công (R03) | `CreatorId != EditorId` (Cấm tự duyệt) |
| `Approved` | Đã phê duyệt, sẵn sàng xuất bản | Đặt lịch xuất bản, đăng ngay | Quản trị viên nội dung (R03) | `QualityScore >= 80%` & `rightsCleared == true` |
| `Scheduled` | Đã lên lịch hẹn ngày xuất bản | Hủy lịch, điều chỉnh ngày xuất bản | Quản trị viên nội dung | Ngày xuất bản phải trong tương lai |
| `Published` | Đã công khai trên ứng dụng | Người học tra cứu, học tập trong M03 | Hệ thống (Cron) / Admin xuất bản | Phân tích tham chiếu M11-T020 an toàn |
| `Deprecated` | Ngừng sử dụng cho bộ từ mới | Duy trì cho tiến độ M04 cũ | Quản trị viên nội dung | Quét tham chiếu 0 active ref mới |
| `Archived` / `Recalled` | Đã lưu trữ / Thu hồi khẩn cấp | Ẩn hoàn toàn khỏi cache/API public | Security Admin (R12) / System | Thực thi Emergency Recall $\le 60\text{s}$ |

## 3. Sơ đồ Chuyển dịch Trạng thái Vòng đời (Lifecycle State Machine)

```
 [Draft] ----(Submit)----> [Submitted] ----(Assign Editor)----> [InReview]
    |                          |                                    |
    +----(Delete Draft)        +----(Withdraw)                      +----(Reject)----> [Rejected] (Về Draft)
                                                                    |
                                                               (Approve) [QualityScore >= 80% & REL-04 OK]
                                                                    |
                                                                    v
                                                                [Approved]
                                                                    |
                                           +------------------------+------------------------+
                                           | (Publish Now)                                   | (Schedule)
                                           v                                                 v
                                      [Published] <--------(Publish Trigger)------------ [Scheduled]
                                           |
                   +-----------------------+-----------------------+
                   | (Deprecate)                                   | (Emergency Recall)
                   v                                               v
              [Deprecated]                                    [Recalled]
                   |                                               |
                   +----------------(Archive)----------------------+
                                           |
                                           v
                                      [Archived]
```

## 4. Giao thức Thu hồi Khẩn cấp Mục từ (Emergency Recall Protocol)

Khi mục từ `VocabularyId` đang `Published` bị phát hiện sai sót nghiêm trọng hoặc vi phạm bản quyền:

1. **Kích hoạt Lệnh Thu hồi**: Quản trị viên gọi API `EmergencyRecallHeadword(VocabularyId, Reason)`.
2. **Cập nhật CSDL trong Transaction**: Trạng thái đổi thành `Recalled`, `updatedAtUtc` được ghi nhận.
3. **Phát tín hiệu Xóa Cache Redis ($\le 60\text{s}$ SLA)**:
   - Phát sự kiện Pub/Sub `VocabularyRecalledEvent`.
   - Các API gateway / CDN node purge cache mục từ đó trong vòng $\le 60$ giây.
4. **Cô lập Phiên học M03**: Các phiên học đang chạy nếu chứa từ bị `Recalled` sẽ tự động thay thế bằng từ dự phòng trong cùng Bộ từ.

## 5. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `HL-G01` | Thực thể `Vocabulary` triển khai chuẩn 8 trạng thái vòng đời tương thích M11. |
| `HL-G02` | Cấm tác giả khởi tạo (`CreatorId`) tự phê duyệt hoặc xuất bản mục từ của chính mình. |
| `HL-G03` | Cấm chuyển sang `Approved` / `Published` nếu `QualityScore < 80%` hoặc `rightsCleared == false`. |
| `HL-G04` | Lệnh Thu hồi Khẩn cấp (`Recalled`) gỡ bỏ mục từ khỏi cache công khai trong vòng $\le 60$ giây. |
| `HL-G05` | Mục từ `Deprecated` giữ nguyên dữ liệu cho tiến độ M04 cũ nhưng ẩn khỏi việc tạo bộ từ mới. |
| `HL-G06` | Mọi thao tác chuyển trạng thái vòng đời đều được ghi nhận Audit Event bất biến trong M11. |
| `HL-G07` | Mục từ `Archived` bị khóa hoàn toàn thao tác mutation, chỉ cho phép tra cứu lưu trữ. |
| `HL-G08` | Chuyển trạng thái `Scheduled` tự động kích hoạt Job xuất bản đúng thời điểm đặt lịch. |
| `HL-G09` | Phân quyền thực hiện chuyển trạng thái tuân thủ nghiêm ngặt ma trận vai trò M11. |
| `HL-G10` | 100% các test case tự kiểm HL07-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HL07-01` | Tạo mục từ mới | Khởi tạo trạng thái `Draft` thành công |
| `HL07-02` | Tác giả gửi bản thảo mục từ | Chuyển trạng thái sang `Submitted` thành công |
| `HL07-03` | Admin phân công người kiểm duyệt | Chuyển trạng thái sang `InReview` thành công |
| `HL07-04` | Tác giả (Editor) cố tình tự bấm Approve mục từ của mình | System reject với lỗi `SELF_APPROVAL_DISALLOWED` |
| `HL07-05` | Editor khác bấm Approve mục từ đạt `QualityScore = 90%` và `rightsCleared = true` | Chuyển trạng thái sang `Approved` thành công |
| `HL07-06` | Editor bấm Approve mục từ có `rightsCleared = false` | Reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `HL07-07` | Đăng ngay mục từ `Approved` lên ứng dụng | Chuyển trạng thái sang `Published` và phát cache invalidation |
| `HL07-08` | Lên lịch xuất bản mục từ vào 10:00 ngày mai | Chuyển trạng thái sang `Scheduled`, job xuất bản được tạo |
| `HL07-09` | Đến 10:00 ngày mai, Cron Job kích hoạt xuất bản | Trạng thái chuyển thành `Published` tự động |
| `HL07-10` | Phát lệnh Thu hồi Khẩn cấp cho mục từ vi phạm bản quyền | Trạng thái chuyển `Recalled`, ẩn khỏi API công khai trong $\le 60\text{s}$ |
| `HL07-11` | Chuyển mục từ cũ sang trạng thái `Deprecated` | Từ ẩn khỏi công cụ tạo bộ từ mới, M04 progress giữ nguyên |
| `HL07-12` | Thử sửa nội dung mục từ đang ở trạng thái `Archived` | System reject với lỗi `ARCHIVED_CONTENT_IMMUTABLE` |
| `HL07-13` | Từ chối bản thảo mục từ (`Reject`) | Trạng thái quay trở lại `Draft` kèm lý do từ chối |
| `HL07-14` | Tác giả rút lại bản thảo đang `Submitted` | Trạng thái quay lại `Draft` thành công |
| `HL07-15` | Tra cứu danh sách mục từ `InReview` theo vai trò Editor | Trả về danh sách mục từ được phân công duyệt |
| `HL07-16` | Kiểm tra thời gian phản hồi API Emergency Recall | Xóa cache hoàn tất trong $1.2\text{s}$ ($< 60\text{s}$) |
| `HL07-17` | Tải đồng thời 50 request chuyển trạng thái vòng đời | Response p95 $< 50\text{ms}$, ghi audit log M11 |
| `HL07-18` | Thử chuyển trạng thái từ `Draft` thẳng lên `Published` | Reject với lỗi `INVALID_LIFECYCLE_TRANSITION` |
| `HL07-19` | Phân tích tham chiếu trước khi Deprecate mục từ | Quét đệ quy các bộ từ bị ảnh hưởng (M11-T020) |
| `HL07-20` | Kiểm thử hoàn tất luồng vòng đời mục từ M02-HEADWORD-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-HL-I01` | Entity `Vocabulary.cs` hiện tại chưa có thuộc tính `Status` vòng đời | Thiếu trạng thái quản lý vòng đời mục từ | M02-T049 (Source task) |
| `M02-HL-I02` | Chưa cài đặt quy tắc Cấm tự phê duyệt (`Self-Approval Guard`) | Rủi ro biên tập viên tự tạo và tự duyệt mục từ sai | M02-T049 |
| `M02-HL-I03` | Thiếu luồng Lên lịch xuất bản (`Scheduled`) tự động | Phải bấm xuất bản thủ công từng từ vựng | M02-T049 |
| `M02-HL-I04` | Thiếu cơ chế Lệnh Thu hồi Khẩn cấp ($\le 60\text{s}$ Emergency Recall) | Không thể gỡ bỏ nhanh học liệu vi phạm | M02-T049 |
| `M02-HL-I05` | Chưa lồng ghép các chốt kiểm tra `QualityScore >= 80%` và `rightsCleared == true` vào API Approve | Rủi ro xuất bản học liệu kém chất lượng/vi phạm bản quyền | M02-T049; REL-04 |

- `M02-HL-F01`: Thêm trường `Status` (Enum 8 trạng thái) vào `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-HL-F02`: Triển khai `HeadwordLifecycleService` xử lý state machine và Self-Approval Guard (tiếp nhận: M02-T049).
- `M02-HL-F03`: Xây dựng `EmergencyRecallHandler` gỡ cache trong $\le 60\text{s}$ (tiếp nhận: M02-T049).
- `M02-HL-F04`: Thiết lập bộ kiểm thử tự động HL-G01–G10 và HL07-01–20 (tiếp nhận: M02 tasks).
- `M02-HL-F05`: Thu thập bằng chứng runtime cho luồng vòng đời mục từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T007

- Đã thiết kế hoàn chỉnh `M02-HEADWORD-LIFECYCLE-1.0` với Ma trận 8 Trạng thái Vòng đời mục từ chuẩn M11.
- Đã chốt quy tắc Cấm Tự Phê duyệt (`Self-Approval Guard`) cho tác giả nội dung.
- Đã lồng ghép các ràng buộc cứng trước khi xuất bản: `QualityScore >= 80%` AND `rightsCleared == true` (REL-04 / CT-01).
- Đã xây dựng Giao thức Thu hồi Khẩn cấp SLA $\le 60\text{s}$ (`Emergency Recall Protocol`).
- Đã xác lập 10 Regression Gates (`HL-G01`–`HL-G10`) và 20 Test Cases tự kiểm (`HL07-01`–`HL07-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả xác định vòng đời mục từ M02-T007 | WSA-7K2 |
