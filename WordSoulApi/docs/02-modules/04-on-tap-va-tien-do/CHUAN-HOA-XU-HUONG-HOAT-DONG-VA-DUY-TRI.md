# Chuẩn hóa xu hướng hoạt động và duy trì M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-ACTIVITY-RETENTION-TRENDS-1.0` |
| Task | M04-T034 |
| Đầu vào | M04-SCHEDULE-TIMEZONE-1.0 (M04-T017), M04-REVIEW-LOG-SCHEMA-1.0 (M04-T024), M04-USER-PROGRESS-METRICS-1.0 (M04-T032) |
| Phạm vi | Chuỗi thời gian biểu đồ xu hướng học tập (`Activity & Retention Trends DTO`), phân tích xu hướng biến thiên chỉ số thuộc từ theo tuần/tháng theo múi giờ cá nhân |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa cấu trúc dữ liệu và quy tắc nhóm chuỗi thời gian biểu đồ xu hướng (`ActivityRetentionTrendsDto`) trong M04.

- **Nhóm Dữ liệu Chuẩn theo Ranh giới Múi giờ (`Timezone Aggregation Invariant`)**:
  - Dữ liệu xu hướng học tập từng ngày BẮT BUỘC nhóm theo ranh giới `00:00:00 - 23:59:59` của múi giờ địa phương `UserTimeZoneId`.
  - Tuyệt đối CẤM dùng ranh giới ngày UTC thô để vẽ biểu đồ cho người dùng ở múi giờ khác UTC+0 (gây sai lệch số từ ôn trong ngày).
- **Gắn Nhãn Phiên bản Chính sách khi So sánh (`Policy Version Labeling Rule`)**:
  - Nếu trong khoảng thời gian biểu đồ có sự thay đổi phiên bản chính sách SRS M04, biểu đồ BẮT BUỘC gắn cờ `PolicyVersionChanged = true` để giải thích lý do chỉ số biến động đột ngột.

## 2. Cấu trúc DTO Biểu đồ Xu hướng Hoạt động và Duy trì (Trends DTO Structure)

```csharp
public class ActivityRetentionTrendsDto
{
    public Guid UserId { get; set; }
    public string TimeZoneId { get; set; }
    public List<DailyRetentionPointDto> DailyPoints { get; set; }
}

public class DailyRetentionPointDto
{
    public string DateKey { get; set; } // "yyyy-MM-dd"
    public int ReviewedCount { get; set; }
    public double AverageRetentionScore { get; set; }
    public bool IsPolicyChangeBoundary { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `AT-G01`: 100% điểm dữ liệu trên biểu đồ `DateKey` quy đổi chính xác theo múi giờ địa phương `UserTimeZoneId`.
- `AT-G02`: Sự kiện đổi chính sách SRS sinh điểm đánh dấu `IsPolicyChangeBoundary = true`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AT34-01` | Learner ở múi giờ `Asia/Ho_Chi_Minh` học từ lúc 11:45 PM local | Điểm dữ liệu được ghi nhận vào `DateKey` của ngày hôm đó theo giờ UTC+7. |
| `AT34-02` | Yêu cầu xem biểu đồ 30 ngày gần nhất | Trả về đủ 30 điểm dữ liệu liên tục không bị nhảy cốc/thiếu ngày. |
| `AT34-03` | Kiểm thử hoàn tất luồng M04-ACTIVITY-RETENTION-TRENDS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-AT-F01` | Tạo API GET `/api/v1/progress/trends` trong ProgressController | Phục vụ vẽ biểu đồ tiến độ trên web/mobile | M04-T032 |

## 5. Tự kiểm M04-T034
- Đã hoàn thành đặc tả `M04-ACTIVITY-RETENTION-TRENDS-1.0`.
- Chốt DTO biểu đồ xu hướng nhóm theo múi giờ địa phương và điểm mốc đổi chính sách.
- Ghi nhận 2 Regression Gates (`AT-G01`–`AT-G02`) và 3 Test Cases (`AT34-01`–`AT34-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa xu hướng hoạt động và duy trì M04-T034 | WSA-7K2 |
