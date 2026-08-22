# Thiết kế tra cứu và xem lại phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-LOOKUP-REVIEW-1.0` |
| Task | M03-T044 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M03-SESSION-EVENT-CATALOG-1.0 (M03-T042), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | API tra cứu danh sách lịch sử phiên học và chi tiết tổng kết phiên (`GetSessionHistory`, `GetSessionDetail`), phân quyền người dùng và kiểm toán xem lại |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình tra cứu và xem lại chi tiết lịch sử phiên học (`Session Lookup & Review`) trong M03.

- **Tối thiểu Quyền Xem lại (`Strict Session Lookup Authorization Invariant`)**:
  - Người học CHỈ ĐƯỢC XEM lịch sử phiên học do chính mình tạo ra (`Session.UserId == CurrentUserId`). Truy cập phiên của người khác bị chặn với lỗi HTTP 403 `FORBIDDEN_SESSION_LOOKUP`.
  - Quản trị viên M11 có quyền xem lại phiên bất kỳ nhưng thao tác xem BẮT BUỘC ghi vết kiểm toán `AdminSessionLookupLogs`.
- **Che Dữ liệu Nhạy cảm khi Xem lại (`Mask Sensitive Content Invariant`)**:
  - API xem lại phiên học chỉ trả về danh sách các câu đã làm, đáp án người dùng đã chọn và đáp án đúng. Tuyệt đối CẤM trả về token xác thực hoặc submission key bí mật.

## 2. Quy trình Tra cứu Chi tiết Phiên học (Session Lookup Flow)

```mermaid
graph TD
    Req[GET /api/v1/sessions/{sessionId}] --> AuthCheck{UserId Match or Admin Role?}
    AuthCheck -->|No| Err403[Reject HTTP 403 FORBIDDEN]
    AuthCheck -->|Yes - Admin| LogAdmin[Log AdminSessionLookupLog]
    AuthCheck -->|Yes - Owner| ReturnData[Return SessionDetailDto]
    LogAdmin --> ReturnData
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SL-G01`: 100% request tra cứu chi tiết phiên của người dùng khác bị chối bỏ với lỗi HTTP 403.
- `SL-G02`: Quản trị viên truy vấn chi tiết phiên tạo 1 bản ghi kiểm toán trong `AdminSessionLookupLogs`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SL44-01` | Learner A gọi API xem chi tiết phiên học `Session_123` do chính Learner A tạo | Trả về `SessionDetailDto` đầy đủ danh sách kết quả các từ. |
| `SL44-02` | Learner B thử gọi API xem chi tiết phiên `Session_123` của Learner A | System ném lỗi HTTP 403 `FORBIDDEN_SESSION_LOOKUP`. |
| `SL44-03` | Admin truy vấn xem lại phiên `Session_123` | API trả về dữ liệu thành công và ghi 1 dòng log kiểm toán M11. |
| `SL44-04` | Kiểm thử hoàn tất luồng M03-SESSION-LOOKUP-REVIEW-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SL-F01` | Tạo DTO `SessionDetailDto` và `SessionSummaryItemDto` | Phục vụ màn hình Tổng kết và Xem lại phiên | M03-T039 |

## 5. Tự kiểm M03-T044
- Đã hoàn thành đặc tả `M03-SESSION-LOOKUP-REVIEW-1.0`.
- Chốt phân quyền truy cập phiên cá nhân và log kiểm toán quản trị.
- Ghi nhận 2 Regression Gates (`SL-G01`–`SL-G02`) và 4 Test Cases (`SL44-01`–`SL44-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế tra cứu và xem lại phiên M03-T044 | WSA-7K2 |
