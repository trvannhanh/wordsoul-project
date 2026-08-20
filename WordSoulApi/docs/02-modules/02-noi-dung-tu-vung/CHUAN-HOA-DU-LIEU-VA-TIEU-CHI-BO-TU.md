# Chuẩn hóa dữ liệu và tiêu chí bộ từ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-VOCAB-SET-CRITERIA-1.0` |
| Task | M02-T015 |
| Đầu vào | M02-CEFR-DIFFICULTY-1.0 (D-060), M02-ITEM-QUALITY-1.0 (D-061), REL-04 |
| Phạm vi | Chuẩn hóa cấu trúc thực thể Bộ từ vựng (`VocabularySet`), tiêu chí chất lượng bộ từ công khai, giới hạn số lượng mục từ và phân loại danh mục |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập tiêu chuẩn cấu trúc dữ liệu, phân loại danh mục và chốt kiểm tra chất lượng đối với Bộ từ vựng (`VocabularySet`) trong M02.

- **Giới hạn Kích thước Bộ từ (`Set Size Bounds Invariant`)**: Một Bộ từ vựng phải chứa tối thiểu 5 mục từ và tối đa 50 mục từ. Kích thước tối ưu được khuyến nghị là $10 \to 20$ mục từ/bộ nhằm đạt hiệu quả học tập và ghi nhớ SRS cao nhất.
- **Ràng buộc Chất lượng Bộ từ Công khai (`Public Set Quality Gate`)**: Một Bộ từ vựng Hệ thống (`IsCustom == false`) CHỈ ĐƯỢC PHÉP chuyển trạng thái `published` khi $100\%$ các mục từ thành phần đạt `QualityScore >= 80%` (M02-T006) và `rightsCleared == true` (REL-04 / CT-01). CẤM xuất bản bộ từ chứa dù chỉ 1 từ Substandard hoặc chưa clear bản quyền.
- **Phân tách Rõ ràng Bộ từ Cá nhân và Bộ từ Hệ thống**:
  - *Bộ từ Hệ thống (`System Set`)*: Do ban biên tập quản lý (`IsCustom = false`), công khai cho toàn bộ người học, có `SetDifficultyIndex` và cấp độ CEFR gợi ý.
  - *Bộ từ Cá nhân (`Custom Set`)*: Do người học tự tạo (`IsCustom = true`), gắn `CreatorId`, chỉ người tạo mới có quyền truy cập mặc định.
- **Chỉ số Độ khó Tổng thể của Bộ từ (`SetDifficultyIndex`)**: Được tự động tính toán dựa trên trung bình trọng số chỉ số độ khó thực tế của tất cả các mục từ thành phần (M02-T005 / D-060).

## 2. Mô hình Thực thể Bộ từ vựng (VocabularySet Schema)

```json
{
  "vocabularySetId": 108,
  "title": "Từ vựng Giao tiếp Tiếng Anh B1 - Chủ đề Du lịch",
  "description": "Tập hợp 15 từ vựng phổ biến nhất khi đi du lịch và đặt phòng khách sạn.",
  "categoryTag": "Travel",
  "cefrLevel": "B1",
  "setDifficultyIndex": 5.2,
  "suggestedSetLevel": "B1-B2",
  "isCustom": false,
  "creatorId": null,
  "isPublished": true,
  "itemCount": 15,
  "rewardId": "RWD-SET-108",
  "createdAtUtc": "2026-08-20T10:00:00Z",
  "updatedAtUtc": "2026-08-20T10:00:00Z"
}
```

## 3. Phân loại Danh mục Bộ từ vựng (Set Taxonomy & Categories)

| Mã Category | Tên danh mục | Mô tả & Đối tượng | Ví dụ tiêu biểu | Ràng buộc áp dụng |
|---|---|---|---|---|
| `Curriculum` | Giáo trình / Chủ đề | Bộ từ theo các chủ đề bài học cơ bản | Du lịch, Gia đình, Công việc, Thể thao | Bắt buộc có `CEFRLevel` |
| `ExamPrep` | Luyện thi chứng chỉ | Bộ từ chuyên dụng cho các kỳ thi chuẩn hóa | IELTS Academic, TOEIC 750+, JLPT N3 | Chuẩn hóa theo cấu trúc đề thi |
| `Specialized` | Chuyên ngành | Bộ từ chuyên sâu cho các lĩnh vực công việc | Công nghệ thông tin, Y khoa, Tài chính | Cấp độ thường từ B2 $\to$ C2 |
| `Starter` | Nhập môn Onboarding | Bộ từ đề xuất cho người dùng mới | 50 Từ vựng Tiếng Anh Thông dụng nhất | Gắn với luồng Onboarding M01-T008 |
| `Custom` | Cá nhân tự tạo | Bộ từ do cá nhân người học biên soạn | Từ cần ôn tập tuần này, Từ khó | `IsCustom = true`, gắn `CreatorId` |

## 4. Giao thức Kiểm tra Chất lượng trước Xuất bản Bộ từ (Set Quality Gate Protocol)

Khi Quản trị viên gọi Action `ACT-M11-04` để xuất bản một Bộ từ vựng Hệ thống:

```
[Publish VocabularySet Request]
               |
               v
  (Fetch Member Headwords)
               |
               v
  [Check 100% Member Headwords]
  - ItemCount >= 5 AND ItemCount <= 50
  - 100% Status == 'Published'
  - 100% QualityScore >= 80% (M02-T006)
  - 100% rightsCleared == true (REL-04 / CT-01)
               |
     +---------+---------+
     | (Fail)            | (Pass)
     v                   v
 [REJECT PUBLISH]     [PUBLISH VOCABULARY SET]
 - Return Error DTO   - Set IsPublished = true
                      - Recalculate SetDifficultyIndex
                      - Generate Redis Cache Payload (M02-T009-A)
```

## 5. Quy tắc Tính toán lại Chỉ số Độ khó Bộ từ (Set Difficulty Recalculation)

Mỗi khi có thao tác thêm/bớt từ vựng trong bộ từ hoặc khi Job cập nhật `ItemDifficultyScore` động chạy hàng ngày:

1. Hệ thống tính trung bình cộng $D_{set} = \frac{1}{N} \sum_{i=1}^{N} D_{item, i}$.
2. Cập nhật trường `SetDifficultyIndex = Math.Round(D_set, 2)`.
3. Tự động điều chỉnh `suggestedSetLevel` theo ngưỡng:
   - $D_{set} < 3.0 \implies$ `A1-A2`
   - $3.0 \le D_{set} < 6.5 \implies$ `B1-B2`
   - $D_{set} \ge 6.5 \implies$ `C1-C2`

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SC-G01` | Một Bộ từ vựng bắt buộc có $5 \le ItemCount \le 50$. |
| `SC-G02` | Cấm xuất bản Bộ từ Hệ thống (`IsCustom = false`) nếu chứa mục từ `QualityScore < 80%`. |
| `SC-G03` | Cấm xuất bản Bộ từ Hệ thống nếu chứa mục từ `rightsCleared == false` (REL-04 / CT-01). |
| `SC-G04` | Bộ từ cá nhân (`IsCustom = true`) được gắn `CreatorId` và cách ly quyền truy cập mặc định. |
| `SC-G05` | Chỉ số độ khó bộ từ `SetDifficultyIndex` tự động cập nhật khi thay đổi từ vựng bên trong. |
| `SC-G06` | Phân loại danh mục `categoryTag` thuộc 1 trong 5 nhóm tiêu chuẩn. |
| `SC-G07` | Bộ từ vựng xuất bản thành công tự động tạo Redis Cache Payload cho M03 (M02-T009-A). |
| `SC-G08` | Cấm gán phần thưởng bộ từ (`rewardId`) không tồn tại trong CSDL M06/M07. |
| `SC-G09` | Phân quyền quản lý bộ từ hệ thống tuân thủ nghiêm ngặt ma trận vai trò M11 (`R03 Content Admin`). |
| `SC-G10` | 100% các test case tự kiểm SC15-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SC15-01` | Tạo Bộ từ hệ thống mới với 15 mục từ chuẩn | Tạo bộ từ thành công, `IsCustom = false`, `ItemCount = 15` |
| `SC15-02` | Thử xuất bản Bộ từ chỉ có 3 mục từ ($< 5$) | System reject với lỗi `SET_SIZE_BELOW_MINIMUM` |
| `SC15-03` | Thử xuất bản Bộ từ có 60 mục từ ($> 50$) | System reject với lỗi `SET_SIZE_EXCEEDS_MAXIMUM` |
| `SC15-04` | Thử xuất bản Bộ từ chứa 1 mục từ Substandard ($60\%$) | System reject với lỗi `CONTAINS_SUBSTANDARD_HEADWORD` |
| `SC15-05` | Thử xuất bản Bộ từ chứa 1 mục từ `rightsCleared = false` | System reject với lỗi `ASSET_RIGHTS_NOT_CLEARED` (CT-01) |
| `SC15-06` | Xuất bản Bộ từ 10 từ đạt $100\%$ chất lượng và clear bản quyền | Xuất bản `isPublished = true` thành công |
| `SC15-07` | User tạo Bộ từ cá nhân 10 từ | Tạo bộ từ thành công, `IsCustom = true`, `CreatorId = UserId` |
| `SC15-08` | User khác truy cập Bộ từ cá nhân trên | System deny 403 Forbidden |
| `SC15-09` | Thêm 5 từ trình độ C1 vào Bộ từ B1 | `SetDifficultyIndex` tự động cập nhật và điều chỉnh `suggestedSetLevel` |
| `SC15-10` | Đính kèm phần thưởng `rewardId = "RWD-108"` vào Bộ từ | Đính kèm thành công, xác minh tồn tại từ M06/M07 |
| `SC15-11` | Thử đính kèm `rewardId` không tồn tại | System reject với lỗi `INVALID_REWARD_ID` |
| `SC15-12` | Tra cứu danh sách Bộ từ hệ thống theo danh mục `ExamPrep` | Trả về các bộ từ IELTS, TOEIC thuộc danh mục đó |
| `SC15-13` | Cập nhật tên bài học `title` của một Bộ từ | Cập nhật thành công, xóa Redis Cache cũ để nạp mới |
| `SC15-14` | Ngừng xuất bản (`Unpublish`) một Bộ từ vựng | `isPublished = false`, xóa cache payload M03 |
| `SC15-15` | Tải đồng thời 50 request lấy danh sách Bộ từ công khai | Response p95 $< 30\text{ms}$ từ Redis cache |
| `SC15-16` | Thử gán `categoryTag = "Unknown"` không hợp lệ | System reject với lỗi `INVALID_CATEGORY_TAG` |
| `SC15-17` | Lấy chi tiết DTO Bộ từ vựng qua API | DTO trả về đủ `itemCount`, `setDifficultyIndex`, `suggestedSetLevel` |
| `SC15-18` | Xóa một mục từ khỏi Bộ từ làm số từ giảm xuống 4 từ | Hệ thống tự động chuyển bộ từ về `isPublished = false` |
| `SC15-19` | Phân tích tham chiếu trước khi ngừng xuất bản Bộ từ | Quét các active session M03 đang mở bộ từ đó (T020) |
| `SC15-20` | Kiểm thử hoàn tất luồng chuẩn hóa dữ liệu bộ từ M02-VOCAB-SET-CRITERIA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SC-I01` | Entity `VocabularySet.cs` chưa có ràng buộc số từ tối thiểu/tối đa ($5 \to 50$) | Có nguy cơ tạo bộ từ rỗng hoặc bộ từ quá lớn 500 từ | M02-T049 (Source task) |
| `M02-SC-I02` | API xuất bản bộ từ chưa lồng ghép kiểm tra $100\%$ mục từ thành phần đạt quality & bản quyền | Rủi ro xuất bản bộ từ chứa từ vi phạm bản quyền REL-04 | M02-T049; REL-04 |
| `M02-SC-I03` | `VocabularySet.cs` chưa có trường `SetDifficultyIndex` và `suggestedSetLevel` | Bộ từ vựng chưa hiển thị độ khó tổng thể cho người học | M02-T049 |
| `M02-SC-I04` | Thiếu thuộc tính `categoryTag` phân loại 5 nhóm danh mục tiêu chuẩn | Bộ từ vựng chưa được phân nhóm khoa học | M02-T049 |
| `M02-SC-I05` | Thiếu cơ chế kiểm tra `rewardId` với Module M06/M07 khi đính kèm phần thưởng bộ từ | Rủi ro liên kết phần thưởng không tồn tại | M02-T049; M06 |

- `M02-SC-F01`: Nâng cấp Entity `VocabularySet.cs` bổ sung `SetDifficultyIndex`, `categoryTag`, `suggestedSetLevel` (tiếp nhận: M02-T049).
- `M02-SC-F02`: Tích hợp bộ kiểm tra `QualityScore >= 80%` và `rightsCleared == true` cho 100% mục từ trong bộ (tiếp nhận: M02-T049; REL-04).
- `M02-SC-F03`: Đặt validation giới hạn $5 \le ItemCount \le 50$ (tiếp nhận: M02-T049).
- `M02-SC-F04`: Thiết lập bộ kiểm thử tự động SC-G01–G10 và SC15-01–20 (tiếp nhận: M02 tasks).
- `M02-SC-F05`: Thu thập bằng chứng runtime cho luồng chuẩn hóa bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T015

- Đã thiết kế hoàn chỉnh `M02-VOCAB-SET-CRITERIA-1.0` với Schema `VocabularySet` chuẩn hóa.
- Đã chốt giới hạn kích thước bộ từ $5 \le ItemCount \le 50$.
- Đã xây dựng Giao thức Kiểm tra Chất lượng Bộ từ Công khai cứng: $100\%$ từ thành phần phải đạt `QualityScore >= 80%` & `rightsCleared == true` (REL-04 / CT-01).
- Đã chốt Ma trận 5 Danh mục Phân loại Bộ từ vựng (`Curriculum`, `ExamPrep`, `Specialized`, `Starter`, `Custom`).
- Đã xác lập 10 Regression Gates (`SC-G01`–`SC-G10`) và 20 Test Cases tự kiểm (`SC15-01`–`SC15-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `VocabularySet.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa dữ liệu và tiêu chí bộ từ M02-T015 | WSA-7K2 |
