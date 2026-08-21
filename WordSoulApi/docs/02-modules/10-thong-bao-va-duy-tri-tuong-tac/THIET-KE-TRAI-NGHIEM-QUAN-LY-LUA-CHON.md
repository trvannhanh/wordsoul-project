# Thiết kế trải nghiệm quản lý lựa chọn M10

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M10-PREFERENCE-MANAGEMENT-UX-1.0` |
| Task | M10-T009 |
| Đầu vào | M10-NOTIFICATION-PREFERENCE-MATRIX-1.0 (M10-T007), M10-CONSENT-EVIDENCE-AUDIT-1.0 (M10-T008) |
| Phạm vi | Quy chuẩn giao diện và API quản lý tùy chọn nhận thông báo (`GET /api/v1/notifications/preferences`, `PUT /api/v1/notifications/preferences`), đồng bộ đa thiết bị |
| Tự kiểm | B-G05 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy chuẩn API và UX cho màn hình cài đặt thông báo cá nhân.

- **Đồng bộ Đa Thiết bị Tức thời (`Cross-Device Sync Invariant`)**: Thay đổi tùy chọn cài đặt Push/Email trên 1 thiết bị BẮT BUỘC có hiệu lực tức thì trên toàn bộ các phiên làm việc của người dùng trong vòng $500$ ms qua Redis Distributed Cache.
- **Ràng buộc Khóa Công tắc An ninh (`Disabled Security Toggle Invariant`)**: Công tắc (Toggle) tùy chọn nhóm `SECURITY` trên giao diện người dùng BẮT BUỘC hiển thị ở trạng thái khóa (`Disabled = true, Checked = true`) kèm nhãn giải thích "Thông báo an toàn bắt buộc".

## 2. API Contract Schema - Notification Preferences

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "preferences": [
    {
      "categoryCode": "SECURITY",
      "categoryName": "Cảnh báo An ninh",
      "isEditable": false,
      "isInAppEnabled": true,
      "isPushEnabled": true,
      "isEmailEnabled": true
    },
    {
      "categoryCode": "STUDY",
      "categoryName": "Nhắc nhở Học tập & Ôn tập",
      "isEditable": true,
      "isInAppEnabled": true,
      "isPushEnabled": true,
      "isEmailEnabled": false
    }
  ]
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PM-G01`: 100% response API `GET /preferences` trả về thuộc tính `isEditable = false` cho nhóm `SECURITY`.
- `PM-G02`: Thay đổi settings cập nhật cache Redis trong vòng $< 100$ ms.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PM09-01` | Gọi `GET /api/v1/notifications/preferences` | Trả danh sách tùy chọn 4 nhóm, nhóm `SECURITY` có `isEditable: false`. |
| `PM09-02` | Gọi `PUT /preferences` cập nhật tắt Push `STUDY` | HTTP 200 OK, xóa cache Redis cũ, tạo audit log. |
| `PM09-03` | Kiểm thử hoàn tất luồng M10-PREFERENCE-MANAGEMENT-UX-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M10-PM-F01` | Cần bổ sung Cache Key `user_notif_pref_{userId}` trong Redis | Tối ưu tốc độ đọc preference tại Consumer | M10-T010 |

## 5. Tự kiểm M10-T009
- Đã đặc tả thiết kế trải nghiệm quản lý lựa chọn M10-T009.
- Ghi nhận 2 Regression Gates (`PM-G01`–`PM-G02`) và 3 Test Cases (`PM09-01`–`PM09-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả thiết kế trải nghiệm quản lý lựa chọn M10-T009 | WSA-7K2 |
