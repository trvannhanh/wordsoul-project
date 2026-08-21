# Thiết kế chống gửi lặp M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SUBMIT-IDEMPOTENCY-1.0` |
| Task | M03-T025 |
| Đầu vào | M03-SUBMIT-ANSWER-DATA-1.0 (M03-T024), M12-RETRY-IDEMPOTENCY-1.0 (M12-T037) |
| Phạm vi | Cơ chế Idempotency chống gửi lặp request đáp án do nghẽn mạng hoặc người dùng nhấp đúp |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế đảm bảo tính Idempotency tuyệt đối cho API gửi đáp án trong M03.

- **Tính Idempotency của Submission Token (`Submission Token Idempotency Invariant`)**:
  - Gửi lặp lại cùng 1 request (cùng `ClientSubmissionToken` và cùng `StepId`) BẮT BUỘC trả về kết quả đáp án đã lưu trước đó mà KHÔNG làm tăng số lần thử (`AttemptCount`) và KHÔNG phát lại event sang M04.
  - Gửi request có cùng `StepId` nhưng `ClientSubmissionToken` khác nhau trong khi bước đó đã hoàn thành sẽ bị từ chối với lỗi HTTP 409 `STEP_ALREADY_ANSWERED`.

## 2. Quy trình Xử lý Chống Gửi lặp (Idempotency Pipeline)

```csharp
public async Task<SubmitAnswerResultDto> ProcessSubmitAnswerAsync(SubmitAnswerRequestDto request)
{
    string cacheKey = $"wordsoul:submit:{request.SessionId}:{request.StepId}";
    
    // 1. Kiểm tra cache kết quả đã chấm cho Token này
    var existingResult = await _cache.GetAsync<SubmitAnswerResultDto>(cacheKey);
    if (existingResult != null)
    {
        return existingResult; // Trả về kết quả cũ, không chấm lại
    }
    
    // 2. Thực hiện khóa phân tán Redlock chống race-condition
    using (var lockHandle = await _lock.AcquireLockAsync($"lock:step:{request.StepId}", TimeSpan.FromSeconds(5)))
    {
        // 3. Chấm điểm & lưu kết quả
        var result = ExecuteGrading(request);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24));
        return result;
    }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SI-G01`: Gửi 5 request trùng `ClientSubmissionToken` cùng lúc chỉ có 1 request thực thi chấm điểm, 4 request còn lại nhận kết quả cũ cached.
- `SI-G02`: Số lần thử `AttemptCount` không bị tăng khi người học gửi lặp do mất mạng.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SI25-01` | Người dùng nhấp đúp nút "Gửi đáp án" (2 request song song) | Cả 2 request nhận cùng 1 kết quả `SubmitAnswerResultDto`, không bị tính 2 lần thử. |
| `SI25-02` | Mạng lag khiến Client retry request đã gửi thành công trước đó | Trả về ngay kết quả từ Redis cache trong $\le 5\text{ms}$. |
| `SI25-03` | Kiểm thử hoàn tất luồng M03-SUBMIT-IDEMPOTENCY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SI-F01` | Tích hợp Redis Redlock từ M12-T033 cho endpoint submit | Chống race condition trên distributed node | M03-T026 |

## 5. Tự kiểm M03-T025
- Đã đặc tả cơ chế chống gửi lặp M03-T025.
- Ghi nhận 2 Regression Gates (`SI-G01`–`SI-G02`) và 3 Test Cases (`SI25-01`–`SI25-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế chống gửi lặp M03-T025 | WSA-7K2 |
