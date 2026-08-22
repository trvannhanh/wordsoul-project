# Xây dựng danh mục sự kiện phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-EVENT-CATALOG-1.0` |
| Task | M03-T042 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-SUBMIT-ANSWER-DATA-1.0 (M03-T024), M03-SINGLE-FINALIZATION-GUARANTEE-1.0 (M03-T038) |
| Phạm vi | Kiểm kê và chuẩn hóa schema cho toàn bộ các sự kiện domain và tích hợp của M03 |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này kiểm kê và đóng băng hợp đồng cho 4 sự kiện chính thuộc vòng đời phiên học M03.

- **Bảo mật Nội dung Đáp án (`Zero Answer Leakage Invariant`)**: Payload sự kiện phát đi tuyệt đối CẤM chứa thông tin chuỗi đáp án đúng, trừ khi sự kiện đó dùng cho mục đích kiểm toán nội bộ M11 có phân quyền.
- **Tính Bất biến Payload (`Immutable Event Payload Invariant`)**: Sự kiện đã được xuất ra khỏi M03 có schema bất biến theo phiên bản (`EventVersion = "1.0"`).

## 2. Bảng Danh mục Sự kiện Phiên (Session Event Catalog)

| Mã Sự kiện | Loại | Mô tả | Trọng yếu | Module tiêu thụ |
|---|---|---|---|---|
| `LearningSessionCreatedEvent` | Domain | Phát ra khi người học vừa tạo phiên học mới | Low | M11 |
| `SessionAnswerSubmittedEvent` | Domain | Phát ra sau từng lượt trả lời đúng/sai của người học | Medium | M04 (Initial Recall), M11 |
| `LearningSessionCompletedIntegrationEvent` | Integration | Event tích hợp chính khi phiên chốt hoàn thành | **HIGH** | M04 (SRS), M06 (Reward), M07 (Quest), M11 |
| `LearningSessionAbandonedEvent` | Domain | Phát ra khi phiên bị quá hạn 24h hoặc bị hủy | Low | M11 (Analytics) |

## 3. Cấu trúc Payload Sự kiện Tích hợp Hoàn thành (LearningSessionCompleted Payload)

```csharp
public class LearningSessionCompletedIntegrationEvent
{
    public Guid EventId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public SessionType SessionType { get; set; } // NEW_LEARNING vs REVIEW
    public Guid VocabularySetId { get; set; }
    public string SetRevisionDigest { get; set; }
    
    public int TotalItems { get; set; }
    public int CorrectFirstTryCount { get; set; }
    public double FirstTryAccuracyPercentage { get; set; }
    public int TotalDurationSeconds { get; set; }
    
    public List<CompletedItemResultDto> ItemResults { get; set; } = new();
    public DateTime CompletedAtUtc { get; set; }
    public string EventVersion { get; set; } = "1.0";
}
```

## 4. Regression Gates và Test Cases

### 4.1. Regression Gates
- `SE-G01`: 100% `LearningSessionCompletedIntegrationEvent` chứa `EventId` GUID duy nhất và `EventVersion = "1.0"`.
- `SE-G02`: Payload sự kiện không chứa bất kỳ đáp án đúng thô nào.

### 4.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SE42-01` | Người học hoàn thành phiên học 10 từ | Phát event tích hợp với `TotalItems = 10`, `ItemResults` chứa 10 phần tử. |
| `SE42-02` | Kiểm tra payload JSON của sự kiện | Không chứa trường `CorrectAnswerText`. |
| `SE42-03` | Kiểm thử hoàn tất luồng M03-SESSION-EVENT-CATALOG-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 5. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SE-F01` | Thêm class `LearningSessionCompletedIntegrationEvent` vào `WordSoul.Contracts` | Tách biệt hợp đồng tích hợp liên module | M03-T040 |

## 6. Tự kiểm M03-T042
- Đã hoàn thành đặc tả `M03-SESSION-EVENT-CATALOG-1.0`.
- Kiểm kê 4 sự kiện chính và chốt schema `LearningSessionCompletedIntegrationEvent`.
- Ghi nhận 2 Regression Gates (`SE-G01`–`SE-G02`) và 3 Test Cases (`SE42-01`–`SE42-03`).

## 7. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng danh mục sự kiện phiên M03-T042 | WSA-7K2 |
