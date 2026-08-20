# Chuẩn hóa thêm và bỏ mục từ trong bộ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-ITEM-MUTATION-1.0` |
| Task | M02-T020 |
| Đầu vào | M02-DUPLICATE-DETECTION-1.0 (D-059), M02-VOCAB-SET-CRITERIA-1.0 (D-066), M02-SET-LIFECYCLE-1.0 (D-077), REL-04 |
| Phạm vi | Quy trình kiểm soát thao tác Thêm (`Add`) và Bỏ (`Remove`) mục từ trong Bộ từ vựng, bảo toàn giới hạn kích thước $5 \le N \le 50$, tự động tính chỉ số độ khó và xóa cache Redis |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức Kiểm soát Thao tác Biến đổi Thành phần Bộ từ vựng (`Set Item Addition & Removal Engine`) thuộc M02, bảo đảm tính toàn vẹn dữ liệu, không tạo mục từ trùng lặp nội bộ bộ từ và duy trì các chỉ số độ khó thực tế.

- **Bảo toàn Ràng buộc Giới hạn Kích thước ($5 \le ItemCount \le 50$)**:
  - *Khi Thêm*: CẤM thêm mục từ mới nếu `ItemCount` hiện tại của bộ từ đã đạt tối đa 50 từ (`ItemCount >= 50`).
  - *Khi Bỏ*: Nếu bỏ mục từ làm `ItemCount` giảm xuống dưới 5 từ đối với Bộ từ `Published`, hệ thống TỰ ĐỘNG hạ trạng thái bộ từ về `InReview` hoặc `Draft` và phát cảnh báo.
- **Ràng buộc Chống Trùng lặp Nội bộ Bộ từ (`Internal Set Uniqueness Invariant`)**: Một mục từ vựng `VocabularyId` chỉ được xuất hiện tối đa 1 lần trong 1 Bộ từ vựng. CẤM thêm cùng một từ vựng 2 lần vào cùng 1 bộ từ.
- **Ràng buộc Chất lượng Mục từ cho Bộ từ Hệ thống (`System Set Quality Invariant`)**: Đối với Bộ từ Hệ thống (`IsCustom == false`), mục từ được thêm vào BẮT BUỘC có trạng thái `Published`, `QualityScore >= 80%` và `rightsCleared == true` (REL-04 / CT-01).
- **Tự động Tính toán lại Độ khó & Xóa Cache Redis**: Mỗi thao tác thêm/bớt mục từ tự động cập nhật `SetDifficultyIndex` $D_{set}$ (M02-T005) và xóa cache Redis `lesson_payload:{setId}` (M02-T009-A).

## 2. Quy trình Thao tác Thêm Mục từ vào Bộ (Add Item Workflow)

```
[Add Item Request (SetId, VocabularyId, SelectedSenseIds)]
                           |
                           v
        [Check Set Existence & Mutation Rights] (M02-T016)
                           |
                           v
     [Check Max Limit & Internal Uniqueness Gate]
     - ItemCount < 50
     - VocabularyId NOT IN SetVocabulary(SetId)
                           |
                           v
   [Check Quality & Rights Gate for System Set] (REL-04 / CT-01)
   - Status == 'Published'
   - QualityScore >= 80%
   - rightsCleared == true
                           |
         +-----------------+-----------------+
         | (Pass)                            | (Fail)
         v                                   v
  [Insert into SetVocabulary]          [REJECT MUTATION]
  - ItemCount = ItemCount + 1          - Return Error DTO
  - Recalculate SetDifficultyIndex
  - Purge Redis Cache lesson_payload
```

## 3. Quy trình Thao tác Bỏ Mục từ khỏi Bộ (Remove Item Workflow)

```
[Remove Item Request (SetId, VocabularyId)]
                           |
                           v
        [Check Set Existence & Mutation Rights] (M02-T016)
                           |
                           v
        [Check Minimum Limit Bound (ItemCount >= 5)]
                           |
         +-----------------+-----------------+
         | (ItemCount > 5)                   | (ItemCount == 5)
         v                                   v
  [Delete from SetVocabulary]          [Delete & Downgrade Set]
  - ItemCount = ItemCount - 1          - ItemCount = 4
  - Recalculate SetDifficultyIndex     - Set Status = 'InReview'
  - Purge Redis Cache lesson_payload   - Warn "Chuyển về InReview do < 5 từ"
```

## 4. Cấu trúc Response DTO Biến đổi Thành phần Bộ từ (SetMutationResponseDto)

```json
{
  "vocabularySetId": 108,
  "title": "Từ vựng Giao tiếp Tiếng Anh B1 - Chủ đề Du lịch",
  "action": "ITEM_ADDED",
  "affectedVocabularyId": 1024,
  "itemCount": 16,
  "setDifficultyIndex": 5.35,
  "suggestedSetLevel": "B1-B2",
  "setLifecycleStatus": "Published",
  "cachePurged": true,
  "updatedAtUtc": "2026-08-20T10:00:00Z"
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SI-G01` | Cấm thêm mục từ vào bộ từ nếu `ItemCount` hiện tại đã đạt 50 từ. |
| `SI-G02` | Cấm thêm cùng 1 mục từ (`VocabularyId`) hai lần vào cùng một Bộ từ. |
| `SI-G03` | Thao tác bỏ từ làm `ItemCount < 5` trên bộ từ `Published` tự động chuyển trạng thái bộ từ về `InReview`. |
| `SI-G04` | Cấm thêm mục từ `QualityScore < 80%` hoặc `rightsCleared == false` vào Bộ từ vựng Hệ thống. |
| `SI-G05` | Mỗi thao tác thêm/bớt từ tự động tính toán lại `SetDifficultyIndex` $D_{set}$ chuẩn xác. |
| `SI-G06` | Thao tác thêm/bớt từ xóa sạch Redis Cache `lesson_payload:{setId}` trong vòng $\le 10$ giây. |
| `SI-G07` | Phân quyền thao tác thêm/bớt từ tuân thủ ma trận vai trò M02-T016 (`CreatorId` hoặc `ContentAdmin`). |
| `SI-G08` | Nét nghĩa ngữ cảnh `SelectedSenseIds` được đính kèm chuẩn xác khi thêm từ vào bộ. |
| `SI-G09` | SLA xử lý API thêm/bớt từ vựng $< 50\text{ms}$. |
| `SI-G10` | 100% các test case tự kiểm SI20-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SI20-01` | Thêm từ vựng 1024 vào Bộ từ 108 có 15 mục từ | Thêm thành công, `ItemCount = 16`, tính lại $D_{set}$ |
| `SI20-02` | Thử thêm từ vựng 1024 lần thứ 2 vào Bộ từ 108 | Reject với lỗi `ITEM_ALREADY_EXISTS_IN_SET` |
| `SI20-03` | Thử thêm từ vựng vào Bộ từ đã chứa đủ 50 mục từ | Reject với lỗi `SET_SIZE_EXCEEDS_MAXIMUM` |
| `SI20-04` | Thử thêm từ vựng Substandard ($60\%$) vào Bộ từ Hệ thống | Reject với lỗi `CONTAINS_SUBSTANDARD_HEADWORD` |
| `SI20-05` | Thử thêm từ vựng `rightsCleared = false` vào Bộ từ Hệ thống | Reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `SI20-06` | Bỏ 1 mục từ khỏi Bộ từ 108 đang có 10 từ | Xóa thành công, `ItemCount = 9`, cập nhật $D_{set}$ |
| `SI20-07` | Bỏ 1 mục từ khỏi Bộ từ `Published` đang có đúng 5 từ | Xóa thành công, `ItemCount = 4`, hạ trạng thái về `InReview` |
| `SI20-08` | Người học B thử thêm từ vào Bộ từ Cá nhân của Người học A | System reject 403 Forbidden (M02-T016) |
| `SI20-09` | Kiểm tra cache Redis `lesson_payload:108` sau khi thêm từ | Cache cũ bị xóa, request tiếp theo nạp payload mới |
| `SI20-10` | Thêm từ vựng trình độ C2 vào Bộ từ A1 | $D_{set}$ tăng lên, tự động cập nhật `suggestedSetLevel` |
| `SI20-11` | Thêm từ vựng kèm chỉ định `SelectedSenseIds = [5012]` | Ghi nhận `SelectedSenseIds` chuẩn xác vào `SetVocabulary` |
| `SI20-12` | Bỏ 1 từ vựng không có trong bộ từ | Reject với lỗi `ITEM_NOT_FOUND_IN_SET` |
| `SI20-13` | Tải đồng thời 50 request thêm/bớt từ vựng trong các bộ từ | Response latency p95 $< 45\text{ms}$ |
| `SI20-14` | User chưa đăng nhập thử gọi API thêm từ vào bộ | Deny 401 Unauthorized |
| `SI20-15` | Xem vết Audit Log M11 sau khi biến đổi thành phần bộ từ | Ghi nhận Audit Event `ACT-M11-04` với diff chi tiết |
| `SI20-16` | Thêm từ vựng vào Bộ từ Cá nhân `IsCustom = true` | Cho phép thêm từ `Draft` của chính tác giả |
| `SI20-17` | Phân tích tham chiếu trước khi bỏ 1 mục từ khỏi bộ | Quét các active session M03 đang tham chiếu từ đó (T020) |
| `SI20-18` | Thao tác thêm từ bị gián đoạn giữa chừng do lỗi CSDL | Rollback transaction toàn bộ, số lượng từ giữ nguyên |
| `SI20-19` | Thêm đồng loạt 5 từ vựng vào bộ từ (Batch Add) | Thêm thành công cả 5 từ, tăng `ItemCount` đúng 5 |
| `SI20-20` | Kiểm thử hoàn tất luồng chuẩn hóa thêm và bỏ mục từ M02-SET-ITEM-MUTATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SI-I01` | API `VocabularySetsController.cs` hiện tại chưa check giới hạn $5 \le N \le 50$ khi bớt từ | Có thể làm số từ trong bộ giảm xuống 0 từ mà bộ từ vẫn ở trạng thái `Published` | M02-T049 (Source task) |
| `M02-SI-I02` | Thiếu validation kiểm tra trùng lặp `VocabularyId` nội bộ bộ từ | Rủi ro chèn 1 từ vựng 2 lần gây lỗi hiển thị M03 | M02-T049 |
| `M02-SI-I03` | Thiếu kiểm tra quality & bản quyền khi thêm mục từ vào bộ từ hệ thống | Rủi ro nạp nhầm từ kém chất lượng vào bộ từ public | M02-T049; REL-04 |
| `M02-SI-I04` | Chưa tự động tính toán lại `SetDifficultyIndex` sau khi thêm/bớt từ | Chỉ số độ khó bộ từ bị sai lệch sau khi sửa nội dung | M02-T049 |
| `M02-SI-I05` | Chưa xóa Redis Cache `lesson_payload` sau khi thay đổi thành phần bộ từ | M03 lấy payload cũ từ Redis cache trong 24h tiếp theo | M02-T049 |

- `M02-SI-F01`: Tích hợp `SetItemMutationService` với validation giới hạn $5 \le N \le 50$ (tiếp nhận: M02-T049).
- `M02-SI-F02`: Triển khai `InternalUniquenessGuard` chống trùng từ trong bộ (tiếp nhận: M02-T049).
- `M02-SI-F03`: Tích hợp tính lại $D_{set}$ và xóa cache Redis `lesson_payload` (tiếp nhận: M02-T049).
- `M02-SI-F04`: Thiết lập bộ kiểm thử tự động SI-G01–G10 và SI20-01–20 (tiếp nhận: M02 tasks).
- `M02-SI-F05`: Thu thập bằng chứng runtime cho luồng biến đổi thành phần bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T020

- Đã thiết kế hoàn chỉnh `M02-SET-ITEM-MUTATION-1.0` với Quy trình Thêm và Bỏ Mục từ chuẩn hóa.
- Đã chốt Ràng buộc Giới hạn Kích thước ($5 \le ItemCount \le 50$) và Chống Trùng lặp Nội bộ.
- Đã quy định cơ chế tự động chuyển trạng thái về `InReview` khi số từ $< 5$.
- Đã lồng ghép tính toán lại `SetDifficultyIndex` $D_{set}$ và xóa cache Redis `lesson_payload`.
- Đã xác lập 10 Regression Gates (`SI-G01`–`SI-G10`) và 20 Test Cases tự kiểm (`SI20-01`–`SI20-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa thêm và bỏ mục từ trong bộ M02-T020 | WSA-7K2 |
