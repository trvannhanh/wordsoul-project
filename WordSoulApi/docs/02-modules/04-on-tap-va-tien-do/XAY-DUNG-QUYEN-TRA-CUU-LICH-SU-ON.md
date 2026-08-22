# Xây dựng quyền tra cứu lịch sử ôn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-REVIEW-LOG-AUTHORIZATION-1.0` |
| Task | M04-T027 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M04-REVIEW-LOG-SCHEMA-1.0 (M04-T024), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Ma trận phân quyền truy cập API tra cứu lịch sử ôn tập (`GetProgressHistory`), bảo đảm nguyên tắc dữ liệu tối thiểu và ghi log kiểm toán khi Admin truy vấn |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế phân quyền và bảo mật khi gọi các API tra cứu lịch sử ôn tập (`Review Log Authorization`) trong M04.

- **Chủ sở hữu Dữ liệu Ôn tập (`Data Owner Authorization Invariant`)**:
  - Người học CHỈ ĐƯỢC XEM nhật ký ôn tập của chính mình (`ProgressLog.UserId == CurrentUserId`). Thao tác truy xuất nhật ký của người khác bị chặn với lỗi HTTP 403 `FORBIDDEN_PROGRESS_LOG_ACCESS`.
  - Quản trị viên hệ thống có quyền xem lịch sử ôn của người dùng khác nhưng BẮT BUỘC ghi vết kiểm toán `AdminProgressLogLookupLogs`.
- **Che bớt Dữ liệu Không cần thiết (`Minimal Data Exposure Rule`)**: API trả về cho người học chỉ bao gồm các chỉ số tiến độ cơ bản, không lộ các cột chẩn đoán hệ thống thô.

## 2. Luồng Kiểm tra Quyền Tra cứu Lịch sử Ôn (Authorization Pipeline)

```mermaid
graph TD
    Req[GET /api/v1/progress/{senseId}/history] --> CheckAuth{UserId Matches or Admin Role?}
    CheckAuth -->|No| Err403[Reject HTTP 403 FORBIDDEN]
    CheckAuth -->|Yes - Admin| LogAudit[Record AdminProgressLogLookupLog]
    CheckAuth -->|Yes - Owner| FetchLogs[Fetch UserSenseProgressLogs]
    LogAudit --> FetchLogs
    FetchLogs --> MaskData[Mask Internal System Fields]
    MaskData --> Res200[Return ProgressHistoryDto]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LA-G01`: 100% request tra cứu nhật ký lịch sử ôn của tài khoản khác bị từ chối với HTTP 403.
- `LA-G02`: Admin xem lịch sử ôn người dùng tạo ra 1 dòng log kiểm toán trong M11.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LA27-01` | Learner A gọi API xem lịch sử tiến độ từ A của chính Learner A | API trả về danh sách `UserSenseProgressLog` đầy đủ. |
| `LA27-02` | Learner B cố tình gọi API xem lịch sử tiến độ từ A của Learner A | System chặn và ném lỗi HTTP 403 `FORBIDDEN_PROGRESS_LOG_ACCESS`. |
| `LA27-03` | Kiểm thử hoàn tất luồng M04-REVIEW-LOG-AUTHORIZATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-LA-F01` | Áp dụng `[Authorize(Policy = "CanReadOwnProgress")]` trên ProgressController | Phục vụ bảo mật endpoint API | M04-T024 |

## 5. Tự kiểm M04-T027
- Đã hoàn thành đặc tả `M04-REVIEW-LOG-AUTHORIZATION-1.0`.
- Chốt ma trận phân quyền truy cập lịch sử tiến độ và ghi log kiểm toán Admin.
- Ghi nhận 2 Regression Gates (`LA-G01`–`LA-G02`) và 3 Test Cases (`LA27-01`–`LA27-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xây dựng quyền tra cứu lịch sử ôn M04-T027 | WSA-7K2 |
