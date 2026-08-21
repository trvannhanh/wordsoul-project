# Đặc tả danh sách và số chưa đọc M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-INBOX-LIST-UNREAD-COUNT-1.0` |
| Task | M10-T017 |
| Đầu vào | M10-INBOX-MODEL-SPEC-1.0 (M10-T016) |
| Phạm vi | API truy vấn danh sách thông báo Hộp thư phân trang (`GET /api/v1/notifications/inbox`) và đếm số lượng chưa đọc (`GET /api/v1/notifications/unread-count`) |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy chuẩn API trả về danh sách Hộp thư và badge chưa đọc cho người học.

- **Phân trang Đột biến Thống nhất (`Cursor-Based Pagination Invariant`)**: API danh sách Hộp thư BẮT BUỘC hỗ trợ phân trang theo con trỏ (`Cursor` = `CreatedAtUtc`) để đảm bảo không bị bỏ sót hoặc trùng lặp bản ghi khi có thông báo mới chèn vào giữa luồng.
- **Tốc độ Phản hồi Badge Chưa đọc (`Unread Count Cache Invariant`)**: API `GET /unread-count` BẮT BUỘC lấy giá trị từ Redis Cache key `user_unread_count_{userId}` để đảm bảo thời gian phản hồi $< 20$ ms cho icon chuông thông báo trên App.

## 2. API Contract Schema - Inbox List Response

```json
{
  "unreadCount": 3,
  "items": [
    {
      "notificationId": "8f3b2a1c-9d4e-4f7b-9c2a-1e3f5b7c9d1a",
      "categoryCode": "STUDY",
      "title": "Đã đến giờ ôn tập!",
      "body": "Bạn có 15 từ cần ôn hôm nay.",
      "actionDeepLink": "wordsoul://learning/review",
      "isRead": false,
      "createdAtUtc": "2026-08-21T10:00:00Z"
    }
  ],
  "nextCursor": "2026-08-21T09:30:00Z",
  "hasMore": true
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IL-G01`: 100% response `GET /unread-count` có thời gian phản hồi $< 20$ ms.
- `IL-G02`: Danh sách Hộp thư không chứa các thông báo đã hết hạn TTL.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IL17-01` | Gọi `GET /api/v1/notifications/unread-count` khi có 3 tin chưa đọc | Response `{ "unreadCount": 3 }`, HTTP 200 OK. |
| `IL17-02` | Lấy danh sách Hộp thư trang 1 (`pageSize = 10`) | Trả 10 thông báo mới nhất kèm `nextCursor`. |
| `IL17-03` | Kiểm thử hoàn tất luồng M10-INBOX-LIST-UNREAD-COUNT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-IL-F01` | Bổ sung Redis Key `user_unread_count_{userId}` trong Cache Service | Phục vụ đọc badge siêu tốc | M10-T018 |

## 5. Tự kiểm M10-T017
- Đã đặc tả danh sách và số chưa đọc M10-T017.
- Ghi nhận 2 Regression Gates (`IL-G01`–`IL-G02`) và 3 Test Cases (`IL17-01`–`IL17-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả danh sách và số chưa đọc M10-T017 | WSA-7K2 |
