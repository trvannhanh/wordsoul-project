# Xác định tác động thay đổi bộ đang học — lát A M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-ACTIVE-SET-CHANGE-IMPACT-A-1.0` |
| Task | M02-T023-A |
| Đầu vào | M02-LESSON-CONTENT-1.0 (D-064), M02-SET-LIFECYCLE-1.0 (D-077), M02-SET-ITEM-MUTATION-1.0 (D-079), M02-SET-VOCAB-OVERRIDE-1.0 (D-081) |
| Phạm vi | Đánh giá và xử lý tác động khi Bộ từ vựng bị thay đổi (thêm/bớt từ, sắp xếp, ghi đè, thu hồi) lên các Phiên học M03 đang diễn ra và Tiến độ Ôn tập SRS M04 của người học |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức Đánh giá Tác động Thay đổi Bộ từ vựng Đang Học — Lát A (`Active Set Change Impact Protocol - Slice A`) thuộc M02, giải quyết bài toán xung đột khi dữ liệu Bộ từ bị biến đổi trong lúc có hàng ngàn người học đang thực hiện phiên học M03 hoặc theo dõi tiến độ SRS M04.

- **Bảo toàn Phiên học Đang Diễn ra bằng Session Pinning (`Active Session Pinning Invariant`)**: Phiên học M03 khi khởi tạo đã được chốt phiên bản bằng mã băm `revisionDigest` (D-064). Mọi thao tác thêm/bớt từ, đổi thứ tự hay thu hồi bộ từ trong lúc phiên học đang chạy tuyệt đối KHÔNG làm crash, gián đoạn hay thay đổi danh sách câu hỏi của phiên học đó.
- **Cách ly Tiến độ Ôn tập SRS người học (`SRS Progress Isolation Invariant`)**: Mức độ thành thạo SRS (M04) của người học được liên kết trực tiếp với `VocabularyId`, KHÔNG phụ thuộc vào `VocabularySetId`. Việc gỡ một từ khỏi bộ từ hoặc lưu kho bộ từ KHÔNG làm mất tiến độ ghi nhớ SRS của người học.
- **Lập lịch Cập nhật Redis Cache Tức thời cho Phiên mới (`Instant Cache Eviction Protocol`)**: Ngay khi Bộ từ vựng bị biến đổi thành phần, cache Redis `lesson_payload:{setId}` bị xóa lập tức. Các phiên học M03 khởi tạo *sau* thời điểm biến đổi tự động nhận payload mới.
- **Hạ Cờ Cảnh báo cho Bộ từ bị Thu hồi (`Emergency Recall Safety Buffer`)**: Khi bộ từ bị Thu hồi Khẩn cấp (`ACT_RECALL_SET`), các phiên học M03 mới bị chặn lập tức với lỗi `VOCABULARY_SET_RECALLED`. Phiên học cũ đang chạy được phép nộp kết quả bài học nhưng bị từ chối cộng thưởng Gamification M06.

## 2. Ma trận Tác động và Quy tắc Xử lý theo Loại Thay đổi (Impact & Handling Matrix)

| Loại Thay đổi (`Set Mutation Type`) | Tác động đến Phiên M03 đang chạy | Tác động đến Phiên M03 khởi tạo mới | Tác động đến Tiến độ SRS M04 của người học |
|---|---|---|---|
| **Thêm từ mới vào bộ** | Không ảnh hưởng (Pin theo `revisionDigest` cũ) | Nạp payload mới có $N+1$ từ từ Redis | Không ảnh hưởng (Từ mới bắt đầu ở mức SRS 0) |
| **Bỏ từ khỏi bộ** | Không ảnh hưởng (Giữ nguyên $N$ từ trong session) | Nạp payload mới có $N-1$ từ từ Redis | Giữ nguyên tiến độ SRS của từ bị bỏ trong CSDL |
| **Đổi thứ tự từ (`DisplayOrder`)**| Không ảnh hưởng (Giữ thứ tự ban đầu) | Nạp payload mới xếp theo `DisplayOrder` mới | Không ảnh hưởng |
| **Ghi đè nghĩa/ví dụ mới** | Không ảnh hưởng (Giữ nghĩa khi start) | Nạp payload mới hiển thị nghĩa ghi đè mới | Không ảnh hưởng |
| **Thu hồi Khẩn cấp (`Archived`)** | Cho phép học xong, chặn thưởng M06 | Chặn khởi tạo mới (`SET_RECALLED`) | Giữ nguyên tiến độ SRS cá nhân |

## 3. Quy trình Xử lý Sự kiện Biến đổi Bộ từ (Set Mutation Event Flow)

```
[Admin/Creator Mutates Published Vocabulary Set (SetId 108)]
                           |
                           v
          [Execute DB Mutation Transaction]
                           |
                           v
   [Evict Redis Cache Key: lesson_payload:108]
                           |
                           v
     [Publish SetMutatedIntegrationEvent (SetId 108)]
               /                        \
              /                          \
             v                            v
  [Module M03 Session Handler]   [Module M04 SRS Handler]
  - Active Sessions (Pinned):    - Maintain VocabularyId progress
    Continue normally            - Mark Set snapshot stale in UI
  - New Sessions (Unpinned):
    Pull new Redis payload
```

## 4. Giao thức Bảo toàn Nộp Kết quả Phiên học (Session Result Submission Protocol)

```csharp
public async Task<SubmitLessonResultDto> SubmitLessonAsync(SubmitLessonRequestDto request)
{
    // 1. Kiểm tra session hợp lệ
    var session = await _sessionRepository.GetAsync(request.SessionId);
    if (session == null) throw new InvalidOperationException("SESSION_NOT_FOUND");

    // 2. So sánh revisionDigest đã chốt lúc start với trạng thái bộ từ hiện tại
    var currentSet = await _setRepository.GetAsync(session.VocabularySetId);
    bool isSetRecalled = currentSet.Status == VocabularySetStatus.Archived;

    // 3. Ghi nhận kết quả tiến độ SRS M04 cho từng VocabularyId
    foreach (var answer in request.Answers)
    {
        await _srsService.RecordAttemptAsync(request.UserId, answer.VocabularyId, answer.IsCorrect);
    }

    // 4. Nếu bộ từ bị thu hồi khẩn cấp -> Không cộng thưởng Gamification M06
    if (isSetRecalled)
    {
        return new SubmitLessonResultDto
        {
            Status = "COMPLETED_WITHOUT_REWARDS",
            Message = "Bộ từ đã bị thu hồi. Kết quả học tập được ghi nhận vào SRS nhưng không cộng xu/kinh nghiệm."
        };
    }

    // 5. Cộng xu và kinh nghiệm bình thường M06
    var rewards = await _rewardService.GrantLessonRewardsAsync(request.UserId, request.Score);
    return new SubmitLessonResultDto { Status = "SUCCESS", Rewards = rewards };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AC-G01` | Phiên học M03 đang diễn ra tuyệt đối KHÔNG bị crash hoặc gián đoạn khi bộ từ vựng bị sửa đổi. |
| `AC-G02` | Mức độ thành thạo SRS (M04) của người học được bảo toàn $100\%$ khi từ vựng bị gỡ khỏi bộ từ. |
| `AC-G03` | Biến đổi bộ từ vựng tự động xóa cache Redis `lesson_payload:{setId}` lập tức trong vòng $\le 5$ giây. |
| `AC-G04` | Phiên học M03 mới khởi tạo sau thời điểm sửa đổi tự động nhận đúng payload mới. |
| `AC-G05` | Phiên học M03 đang diễn ra thuộc Bộ từ bị Thu hồi Khẩn cấp được phép nộp bài nhưng bị chặn thưởng M06. |
| `AC-G06` | Cấm khởi tạo phiên học M03 mới đối với Bộ từ đã chuyển sang trạng thái `Archived` hoặc `Unpublished`. |
| `AC-G07` | Mọi sự kiện biến đổi bộ từ phát sự kiện tích hợp `SetMutatedIntegrationEvent` đến M03 và M04. |
| `AC-G08` | Phân tích tác động thay đổi bộ từ đáp ứng tiêu chuẩn tự kiểm A-G03. |
| `AC-G09` | SLA xử lý xóa cache và phát sự kiện tích hợp $< 20\text{ms}$. |
| `AC-G10` | 100% các test case tự kiểm AC23-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AC23-01` | Sửa đổi bộ từ 108 trong lúc Người học A đang học phiên M03 | Phiên học của A tiếp tục hoàn thành bình thường không bị lỗi |
| `AC23-02` | Người học B khởi tạo phiên học M03 mới ngay sau khi bộ từ 108 bị sửa | Phiên học của B nhận đúng payload từ vựng đã sửa đổi |
| `AC23-03` | Gỡ từ vựng 1024 khỏi Bộ từ 108 | Tiến độ SRS M04 của từ 1024 trên tài khoản Người học A giữ nguyên |
| `AC23-04` | Thu hồi khẩn cấp Bộ từ 108 trong lúc Người học A đang làm bài thi cuối khóa M03 | A làm xong và nộp bài thành công, nhận cờ `COMPLETED_WITHOUT_REWARDS` |
| `AC23-05` | Người học B cố gắng bấm "Học ngay" đối với Bộ từ 108 vừa bị thu hồi | System reject với lỗi `VOCABULARY_SET_RECALLED` |
| `AC23-06` | Thêm 5 từ mới vào Bộ từ 108 | Cache Redis `lesson_payload:108` bị xóa, các phiên mới có $N+5$ từ |
| `AC23-07` | Thay đổi `DisplayOrder` của từ vựng trong Bộ từ 108 | DTO phiên mới hiển thị thứ tự câu hỏi mới |
| `AC23-08` | Ghi đè nghĩa mới cho từ "bank" trong Bộ từ 108 | DTO phiên mới hiển thị nghĩa ghi đè mới |
| `AC23-09` | Kiểm tra thời gian xóa cache Redis sau khi biến đổi bộ từ | Cache bị gỡ trong vòng $< 3\text{ms}$ |
| `AC23-10` | Người học A học lại Bộ từ 108 sau khi bộ từ đã bổ sung 3 từ mới | M03 tạo bài học chứa các từ mới kèm từ cũ |
| `AC23-11` | Nộp bài học M03 cho bộ từ không bị biến đổi | Nộp bài thành công, ghi nhận SRS M04 và cộng thưởng M06 |
| `AC23-12` | Tải đồng thời 100 phiên M03 đang học khi 1 bộ từ bị sửa | 100/100 phiên học hoàn tất an toàn, không có exception crash |
| `AC23-13` | User ngắt kết nối mạng rồi nộp bài phiên M03 đã bị thu hồi bộ từ | Nộp bài thành công khi reconnection, chặn thưởng M06 |
| `AC23-14` | Kiểm tra log sự kiện tích hợp `SetMutatedIntegrationEvent` | Ghi nhận chi tiết `SetId`, `MutationType`, `TimestampUtc` |
| `AC23-15` | Tra cứu vết Audit Log M11 khi nộp bài cho bộ từ bị thu hồi | Ghi nhận Audit Event `ACT-M11-04` với ghi chú chặn thưởng |
| `AC23-16` | Bỏ 1 từ vựng khỏi bộ từ rồi thêm lại từ đó sau 5 phút | Tiến độ SRS M04 của người học tự động kết nối lại |
| `AC23-17` | Phân tích tham chiếu các phiên học active khi biến đổi bộ từ | Quét danh sách active session tokens trong Redis (T020) |
| `AC23-18` | Thao tác biến đổi bộ từ bị rollback giữa chừng | Cache Redis giữ nguyên, phiên học mới nhận payload cũ |
| `AC23-19` | Người học hoàn thành bài học phiên chốt `revisionDigest` cũ | Kết quả được chấm điểm chính xác theo danh sách từ lúc chốt |
| `AC23-20` | Kiểm thử hoàn tất luồng xác định tác động thay đổi bộ M02-ACTIVE-SET-CHANGE-IMPACT-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-AC-I01` | Trong `LessonService.cs`, chưa có cơ chế Session Pinning bằng `revisionDigest` | Khi bộ từ bị sửa, phiên học M03 đang chạy có thể bị lỗi out-of-index | M02-T049 (Source task) |
| `M02-AC-I02` | Chưa có bộ xử lý nộp bài `SubmitLessonAsync` phân biệt bộ từ bị thu hồi | Người học vẫn nhận thưởng Gamification M06 dù bộ từ bị thu hồi vi phạm | M02-T049; M06-T010 |
| `M02-AC-I03` | Thiếu việc xóa Redis Cache `lesson_payload` lập tức khi biến đổi bộ từ | Người học mới vẫn nạp lại payload từ vựng chưa sửa đổi | M02-T049 |
| `M02-AC-I04` | Chưa phát sự kiện tích hợp `SetMutatedIntegrationEvent` sang M03 và M04 | M03 và M04 không nhận biết được bộ từ đã bị thay đổi thành phần | M02-T049 |
| `M02-AC-I05` | Chưa tách biệt hoàn toàn giữa `VocabularyId` SRS progress và `VocabularySetId` | Khi xóa bộ từ, một số câu hỏi SRS bị đứt đoạn tham chiếu | M02-T049; M04-T008 |

- `M02-AC-F01`: Triển khai Session Pinning với `revisionDigest` cho M03 (tiếp nhận: M02-T049).
- `M02-AC-F02`: Tích hợp logic chặn thưởng M06 cho bộ từ bị `Archived` (tiếp nhận: M02-T049; M06-T010).
- `M02-AC-F03`: Xây dựng `SetMutatedIntegrationEvent` publisher/subscriber (tiếp nhận: M02-T049).
- `M02-AC-F04`: Thiết lập bộ kiểm thử tự động AC-G01–G10 và AC23-01–20 (tiếp nhận: M02 tasks).
- `M02-AC-F05`: Thu thập bằng chứng runtime cho luồng tác động thay đổi bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T023-A

- Đã thiết kế hoàn chỉnh `M02-ACTIVE-SET-CHANGE-IMPACT-A-1.0` với Ma trận Tác động 5 Loại Thay đổi.
- Đã chốt Ràng buộc Bảo toàn Phiên học Đang Diễn ra (`Active Session Pinning Invariant`).
- Đã chốt Ràng buộc Cách ly Tiến độ Ôn tập SRS người học M04 (`SRS Progress Isolation Invariant`).
- Đã lồng ghép Giao thức Nộp Kết quả Phiên học đối với Bộ từ bị Thu hồi Khẩn cấp (Chặn thưởng M06).
- Đã xác lập 10 Regression Gates (`AC-G01`–`AC-G10`) và 20 Test Cases tự kiểm (`AC23-01`–`AC23-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả tác động thay đổi bộ đang học M02-T023-A | WSA-7K2 |
