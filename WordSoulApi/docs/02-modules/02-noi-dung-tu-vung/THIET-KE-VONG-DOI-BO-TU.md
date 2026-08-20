# Thiết kế vòng đời bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-LIFECYCLE-1.0` |
| Task | M02-T017 |
| Đầu vào | M02-VOCAB-SET-CRITERIA-1.0 (D-066), M02-SET-PERMISSIONS-1.0 (D-076), M11-T019, REL-04 |
| Phạm vi | Máy trạng thái vòng đời 7 bước của Bộ từ vựng, quy tắc chuyển trạng thái hợp lệ, điều kiện cứng trước khi xuất bản và giao thức thu hồi khẩn cấp |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Máy trạng thái Vòng đời Bộ từ vựng (`Vocabulary Set Lifecycle State Machine`) thuộc M02, quản lý toàn bộ quá trình từ khi soạn thảo bộ từ cho đến khi duyệt công khai, tạm ngưng hoặc lưu kho.

- **Đồng bộ Ma trận 7 Trạng thái Vòng đời (`7-State Set Lifecycle Alignment`)**: Vòng đời bộ từ bao gồm 7 trạng thái chuẩn: `Draft`, `Submitted`, `InReview`, `Approved`, `Published`, `Unpublished`, `Archived`.
- **Ràng buộc Chất lượng Bộ từ Xuất bản Cứng (`Set Quality Gate Invariant`)**: Bộ từ CHỈ ĐƯỢC PHÉP chuyển sang trạng thái `Published` khi $100\%$ mục từ thành phần có $5 \le ItemCount \le 50$, `QualityScore >= 80%` (M02-T006) và `rightsCleared == true` (REL-04 / CT-01). CẤM xuất bản bộ từ vi phạm bản quyền hoặc chứa từ Substandard.
- **Quy tắc Cấm Tác giả Tự duyệt (`Self-Approval Guard`)**: Người tạo bộ từ (`CreatorId`) tuyệt đối KHÔNG thể tự bấm phê duyệt xuất bản bộ từ của chính mình. Phê duyệt bắt buộc qua bước duyệt độc lập của `ContentAdmin`.
- **Giao thức Thu hồi Khẩn cấp ($\le 60\text{s}$ Emergency Recall SLA)**: Khi phát hiện bộ từ chứa học liệu sai sót nặng hoặc vi phạm pháp lý, `ContentAdmin` phát lệnh Thu hồi Khẩn cấp. Hệ thống tự động xóa cache Redis `lesson_payload:{setId}` và ẩn bộ từ khỏi catalog công khai trong vòng $\le 60$ giây.

## 2. Máy trạng thái Vòng đời Bộ từ (Set Lifecycle State Machine)

```
       [Draft] --(Submit: ItemCount 5-50)--> [Submitted]
          ^                                       |
          | (Request Changes)                     v
          +--------------------------------- [InReview]
                                                  |
                                                  v (Approve: 100% Quality & Rights)
                                             [Approved]
                                                  |
                                                  v (Publish & Cache Redis Payload)
                                             [Published]
                                              /       \
                        (Unpublish / Hide)   /         \  (Emergency Recall <= 60s)
                                            v           v
                                     [Unpublished]  [Archived]
```

## 3. Bảng Chuyển đổi Trạng thái Hợp lệ (Valid State Transitions Matrix)

| Trạng thái Nguồn | Trạng thái Đích | Tên Hành động (`Action Code`) | Quyền thực thi | Điều kiện ràng buộc |
|---|---|---|---|---|
| `Draft` | `Submitted` | `ACT_SUBMIT_SET` | Creator / ContentAdmin | $5 \le ItemCount \le 50$ |
| `Submitted` | `InReview` | `ACT_ASSIGN_SET_REVIEW` | ContentAdmin | Phân công người duyệt độc lập |
| `InReview` | `Draft` | `ACT_REJECT_SET_REVISION` | ContentAdmin | Nhập lý do yêu cầu chỉnh sửa |
| `InReview` | `Approved` | `ACT_APPROVE_SET` | ContentAdmin | Self-Approval Guard, 100% Quality $\ge 80\%$ & Rights |
| `Approved` | `Published` | `ACT_PUBLISH_SET` | ContentAdmin / System Job | Tự động tạo Redis Payload (M02-T009-A) |
| `Published` | `Unpublished` | `ACT_UNPUBLISH_SET` | ContentAdmin | Tạm ẩn khỏi catalog public, giữ active M03 session |
| `Unpublished`| `Published` | `ACT_REPUBLISH_SET` | ContentAdmin | Kiểm tra lại điều kiện Quality & Rights |
| `Published` | `Archived` | `ACT_RECALL_SET` | ContentAdmin / SecurityAdmin | Emergency Recall SLA $\le 60\text{s}$, xóa Redis cache |

## 4. Giao thức Thu hồi Khẩn cấp Bộ từ (Emergency Set Recall Protocol)

Khi nhận lệnh Thu hồi Khẩn cấp `ACT_RECALL_SET(setId, recallReason)`:

1. Mở CSDL Transaction: Chuyển `VocabularySet.Status = 'Archived'`.
2. Xóa lập tức Redis Cache Payload: `Redis.KeyDelete("lesson_payload:" + setId)`.
3. Gửi thông báo đến Module M03: Chặn mở thêm phiên học mới với `setId` này (các phiên học đang chạy được hoàn tất theo Session Pinning M02-T008-A).
4. Ghi vết Audit Log M11: Ghi nhận Audit Event `ACT-M11-04` với mã `recallReason` và `ActorId`.

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SL-G01` | 100% Bộ từ vựng chuyển trạng thái đúng theo Ma trận Chuyển đổi Vòng đời 7 bước. |
| `SL-G02` | Cấm nộp bộ từ `Draft` nếu $ItemCount < 5$ hoặc $ItemCount > 50$. |
| `SL-G03` | Cấm chuyển bộ từ sang `Approved` nếu chứa mục từ `QualityScore < 80%` hoặc `rightsCleared == false`. |
| `SL-G04` | Phê duyệt bộ từ bắt buộc qua bước duyệt độc lập của `ContentAdmin` (`Self-Approval Guard`). |
| `SL-G05` | Chuyển bộ từ sang `Published` tự động kích hoạt pre-generate Redis Lesson Payload M02-T009-A. |
| `SL-G06` | Lệnh Thu hồi Khẩn cấp (`ACT_RECALL_SET`) xóa sạch cache Redis `lesson_payload` trong $\le 60\text{s}$. |
| `SL-G07` | 100% thao tác thay đổi trạng thái vòng đời bộ từ ghi vết Audit Event bất biến M11. |
| `SL-G08` | Bộ từ ở trạng thái `Unpublished` hoặc `Archived` bị ẩn hoàn toàn khỏi API catalog public. |
| `SL-G09` | Phân quyền thực thi chuyển đổi trạng thái tuân thủ ma trận vai trò M02-T016. |
| `SL-G10` | 100% các test case tự kiểm SL17-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SL17-01` | Nộp bộ từ `Draft` có 15 mục từ đạt tiêu chuẩn | Chuyển trạng thái `Submitted` thành công |
| `SL17-02` | Thử nộp bộ từ `Draft` chỉ có 3 mục từ ($< 5$) | System reject với lỗi `SET_SIZE_BELOW_MINIMUM` |
| `SL17-03` | `ContentAdmin` nhận phân công duyệt bộ từ `Submitted` | Chuyển trạng thái `InReview` thành công |
| `SL17-04` | Thử phê duyệt bộ từ chứa 1 từ vựng `QualityScore = 60%` | Reject phê duyệt với lỗi `CONTAINS_SUBSTANDARD_HEADWORD` |
| `SL17-05` | Thử phê duyệt bộ từ chứa 1 từ vựng `rightsCleared = false` | Reject phê duyệt với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `SL17-06` | Tác giả bộ từ tự gửi request phê duyệt bộ từ của mình | Reject với lỗi `SELF_APPROVAL_FORBIDDEN` |
| `SL17-07` | `ContentAdmin` duyệt bộ từ đạt 100% tiêu chí quality & rights | Chuyển trạng thái `Approved` thành công |
| `SL17-08` | Xuất bản bộ từ `Approved` lên công khai | Chuyển `Published`, pre-generate Redis Payload |
| `SL17-09` | Kiểm tra thời gian phản hồi API nạp bài học sau khi xuất bản | Response latency $< 20\text{ms}$ từ Redis cache |
| `SL17-10` | Thực hiện Lệnh Thu hồi Khẩn cấp cho Bộ từ 108 | Chuyển `Archived`, xóa Redis cache trong $\le 60\text{s}$ |
| `SL17-11` | Người học thử nạp bài học cho bộ từ vừa bị `Archived` | Reject với lỗi `VOCABULARY_SET_NOT_PUBLISHED` |
| `SL17-12` | Tạm ẩn (`Unpublish`) bộ từ vựng công khai | Chuyển `Unpublished`, ẩn khỏi catalog công khai |
| `SL17-13` | Tái xuất bản (`Republish`) bộ từ vựng `Unpublished` | Chuyển lại `Published` sau khi xác minh điều kiện |
| `SL17-14` | Tải đồng thời 50 request chuyển đổi trạng thái bộ từ | Response latency p95 $< 30\text{ms}$ |
| `SL17-15` | User không phải Admin thực hiện xuất bản bộ từ hệ thống | Deny 403 Forbidden |
| `SL17-16` | Từ chối duyệt bộ từ `InReview` và yêu cầu tác giả sửa | Chuyển về `Draft` kèm lý do chỉnh sửa |
| `SL17-17` | Phân tích tham chiếu trước khi lưu kho (`Archived`) bộ từ | Quét các active session M03 đang mở bộ từ (T020) |
| `SL17-18` | Kiểm tra tính nhất quán giữa `ItemCount` và danh sách mục từ | `ItemCount` tự động khớp 100% số mục từ thực tế |
| `SL17-19` | Xem vết Audit Log M11 sau khi chuyển trạng thái bộ từ | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `SL17-20` | Kiểm thử hoàn tất luồng thiết kế vòng đời bộ từ M02-SET-LIFECYCLE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SL-I01` | Entity `VocabularySet.cs` hiện tại chưa có đầy đủ 7 trạng thái vòng đời | Thiếu các trạng thái `Submitted`, `InReview`, `Archived` | M02-T049 (Source task) |
| `M02-SL-I02` | API chuyển trạng thái bộ từ chưa lồng ghép `Self-Approval Guard` | Tác giả bộ từ có thể tự duyệt xuất bản bộ từ của mình | M02-T049; M02-T007 |
| `M02-SL-I03` | Thiếu luồng Lệnh Thu hồi Khẩn cấp xóa cache Redis $\le 60\text{s}$ | Bộ từ bị thu hồi vẫn tồn tại trong Redis cache của M03 | M02-T049 |
| `M02-SL-I04` | Chưa kiểm tra $100\%$ mục từ thành phần đạt quality & bản quyền trước khi Approve | Rủi ro lọt bộ từ kém chất lượng hoặc vi phạm bản quyền REL-04 | M02-T049; REL-04 |
| `M02-SL-I05` | Chưa tự động xóa cache Redis `lesson_payload` khi Unpublish bộ từ | Người học vẫn lấy được payload cũ từ Redis cache | M02-T049 |

- `M02-SL-F01`: Cập nhật Enum `VocabularySetStatus` đủ 7 trạng thái chuẩn (tiếp nhận: M02-T049).
- `M02-SL-F02`: Triển khai `SetLifecycleStateMachine` với chốt quality & bản quyền (tiếp nhận: M02-T049; REL-04).
- `M02-SL-F03`: Xây dựng `EmergencySetRecallHandler` gỡ cache Redis SLA $\le 60\text{s}$ (tiếp nhận: M02-T049).
- `M02-SL-F04`: Thiết lập bộ kiểm thử tự động SL-G01–G10 và SL17-01–20 (tiếp nhận: M02 tasks).
- `M02-SL-F05`: Thu thập bằng chứng runtime cho luồng vòng đời bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T017

- Đã thiết kế hoàn chỉnh `M02-SET-LIFECYCLE-1.0` với Máy trạng thái Vòng đời 7 bước.
- Đã chốt Ràng buộc Chất lượng Bộ từ Xuất bản Cứng (100% Quality $\ge 80\%$ & Rights).
- Đã lồng ghép bảo vệ Cấm tự duyệt (`Self-Approval Guard`) và Lệnh Thu hồi Khẩn cấp SLA $\le 60\text{s}$.
- Đã quy định quy trình gỡ và pre-generate Redis Lesson Payload Cache.
- Đã xác lập 10 Regression Gates (`SL-G01`–`SL-G10`) và 20 Test Cases tự kiểm (`SL17-01`–`SL17-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế vòng đời bộ từ M02-T017 | WSA-7K2 |
