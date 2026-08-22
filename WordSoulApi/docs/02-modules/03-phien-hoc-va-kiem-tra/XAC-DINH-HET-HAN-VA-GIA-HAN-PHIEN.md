# Xác định hết hạn và gia hạn phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-EXPIRATION-EXTENSION-1.0` |
| Task | M03-T012 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-PAUSE-RESUME-MECHANICS-1.0 (M03-T011), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Quy định thời hạn hết hiệu lực phiên học (Session Expiration - 24 giờ), quy tắc gia hạn phiên và từ chối nhận câu trả lời sau khi hết hạn |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định logic quản lý hết hạn (`Expiration`) và chính sách gia hạn (`Extension`) cho phiên học M03.

- **Thời hạn Cố định 24 Giờ (`Fixed 24-Hour Expiration Invariant`)**:
  - Mọi phiên học (`NewLearningSession` và `ReviewSession`) có thời hạn sống tối đa là $24$ giờ tính từ thời điểm tạo (`ExpiresAtUtc = CreatedAtUtc + 24 Hours`).
  - Quá mốc `ExpiresAtUtc`, API gửi đáp án `SubmitAnswer` BẮT BUỘC bị chối bỏ (HTTP 410 `SESSION_EXPIRED`).
- **Giới hạn Gia hạn Nghiêm ngặt (`Strict Extension Limit Invariant`)**:
  - CHỈ cho phép gia hạn tối đa 1 lần ($+12$ giờ) nếu có sự cố đường truyền mạng hoặc được phê duyệt theo chính sách M11.

## 2. Quy trình Xử lý Hết hạn Phiên (Session Expiration Timeline)

```text
[CREATED] (T = 0h) ───────> [IN_PROGRESS / PAUSED] ───────> [EXPIRED / ABANDONED] (T >= 24h)
                                                                 │
                                                    (Rejected HTTP 410 SESSION_EXPIRED)
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SE-G01`: 100% request gửi đáp án khi `NowUtc > ExpiresAtUtc` bị chối bỏ với lỗi HTTP 410.
- `SE-G02`: Phiên học hết hạn tự động chuyển trạng thái sang `ABANDONED` khi có tương tác mới.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SE12-01` | Gửi đáp án cho phiên học tạo cách đây 25 giờ | API trả về lỗi HTTP 410 `SESSION_EXPIRED`, không ghi nhận đáp án. |
| `SE12-02` | Khởi tạo phiên học mới | Trường `ExpiresAtUtc` được gán chính xác `CreatedAtUtc + 24h`. |
| `SE12-03` | Kiểm thử hoàn tất luồng M03-SESSION-EXPIRATION-EXTENSION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SE-F01` | Cần bổ sung CronJob quét và dọn dẹp các phiên hết hạn trong DB | Giảm bớt kích thước bảng phiên active | M11-T038 |

## 5. Tự kiểm M03-T012
- Đã hoàn thành đặc tả `M03-SESSION-EXPIRATION-EXTENSION-1.0`.
- Chốt mốc 24h hết hạn và quy tắc từ chối HTTP 410.
- Ghi nhận 2 Regression Gates (`SE-G01`–`SE-G02`) và 3 Test Cases (`SE12-01`–`SE12-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định hết hạn và gia hạn phiên M03-T012 | WSA-7K2 |
