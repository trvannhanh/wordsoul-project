# Chốt múi giờ và ranh giới ngày M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-TIMEZONE-DAY-BOUNDARY-1.0` |
| Task | M07-T022 |
| Đầu vào | M07-QUEST-DICT-1.0 (M07-T001), M01-TIMEZONE-1.0 (M01-T025) |
| Phạm vi | Quy định ranh giới ngày nghiệp vụ (Business Day Boundary) và cơ chế chuyển đổi múi giờ chuẩn 00:00 UTC để reset Nhiệm vụ ngày |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này chốt chuẩn ranh giới thời gian reset chu kỳ Nhiệm vụ ngày trong M07.

- **Chu kỳ Reset Ngày Chuẩn UTC (`00:00 UTC Reset Invariant`)**: Chu kỳ Nhiệm vụ ngày (`Daily Quest Cycle`) BẮT BUỘC được tính theo mốc thời gian tuyệt đối 00:00:00 UTC đến 23:59:59 UTC mỗi ngày. CẤM dùng giờ local thiết bị người dùng làm mốc reset DB để tránh gian lận chỉnh giờ thiết bị (Clock Manipulation).
- **Quy đổi Hiển thị Múi giờ Người dùng (`User TimeZone Display Invariant`)**: Client ứng dụng quy đổi mốc `ResetAtUtc` sang múi giờ địa phương của người dùng (ví dụ: UTC+7 $\implies$ 07:00 sáng hôm sau) để hiển thị đồng hồ đếm ngược.

## 2. Dynamic Business Day Evaluation Logic

```csharp
public string GetCurrentBusinessDayKey(DateTime timestampUtc)
{
    // Luôn format ngày nghiệp vụ theo UTC format YYYY-MM-DD
    return timestampUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `TB-G01`: 100% bản ghi nhiệm vụ ngày được gán nhãn `BusinessDayKey` chuẩn UTC format `yyyy-MM-DD`.
- `TB-G02`: Thay đổi giờ thiết bị di động không làm thay đổi ranh giới reset nhiệm vụ trên Server.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TB22-01` | Người dùng tại UTC+7 truy vấn nhiệm vụ lúc 23:30 (tức 16:30 UTC) | Nhận danh sách nhiệm vụ của ngày UTC hiện tại `yyyy-MM-DD`. |
| `TB22-02` | Đổi giờ thiết bị di động tiến lên 2 ngày | Server giữ nguyên danh sách nhiệm vụ theo đúng giờ UTC chuẩn. |
| `TB22-03` | Kiểm thử hoàn tất luồng M07-TIMEZONE-DAY-BOUNDARY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-TB-F01` | Cần Cronjob `DailyQuestResetJob` chạy lúc 00:00 UTC mỗi ngày | Phân bổ tập nhiệm vụ ngày mới cho người dùng | M07-T025 |

## 5. Tự kiểm M07-T022
- Đã đặc tả chốt múi giờ và ranh giới ngày M07-T022.
- Ghi nhận 2 Regression Gates (`TB-G01`–`TB-G02`) và 3 Test Cases (`TB22-01`–`TB22-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chốt múi giờ và ranh giới ngày M07-T022 | WSA-7K2 |
