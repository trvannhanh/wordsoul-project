# Thiết kế bỏ dở và làm lại phiên M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-ABANDON-RETRY-SESSION-1.0` |
| Task | M03-T013 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-SESSION-EXPIRATION-EXTENSION-1.0 (M03-T012), M06-ASSET-ITEM-DICT-1.0 (M06-T001) |
| Phạm vi | Hành vi bỏ dở phiên (`Abandon Session`) và làm lại phiên (`Retry Session`), bảo đảm không cấp thưởng cho phiên bỏ dở và xử lý lịch sử |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định quy trình xử lý khi người học chủ động bỏ dở phiên (`Abandon`) hoặc chọn làm lại phiên (`Retry`) trong M03.

- **Cấm Cấp Thưởng khi Bỏ dở (`No Reward on Abandon Invariant`)**:
  - Phiên học ở trạng thái `ABANDONED` tuyệt đối CẤM phát sự kiện cấp Gold/Exp hay tính tiến độ nhiệm vụ M07.
- **Tính Bảo toàn Lịch sử Thử nghiệm (`Audit History Preservation Invariant`)**:
  - Làm lại phiên (`Retry`) sẽ hủy phiên cũ (chuyển `ABANDONED`) và sinh một phiên học mới độc lập với `SessionId` mới.
  - CẤM xóa cứng hay ghi đè bản ghi phiên cũ trong CSDL.

## 2. Quy trình Làm lại Phiên học (Retry Session Workflow)

```mermaid
graph TD
    User[Learner] -->|POST /api/v1/sessions/{id}/retry| API[Session API]
    API --> CheckStatus{Session Active?}
    CheckStatus -->|Yes| AbandonOld[Set Old Session = ABANDONED]
    AbandonOld --> CreateNew[Create New Session with New SessionId]
    CreateNew --> Res200[Return New Session DTO]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `AR-G01`: 100% phiên `ABANDONED` không tạo ra bất kỳ biến động tài sản Gold/Exp nào trong M06.
- `AR-G02`: Hành vi Retry giữ nguyên bản ghi phiên cũ trong DB với trạng thái `ABANDONED`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AR13-01` | Người học bấm "Bỏ dở phiên học" ở bước 7/10 | Chuyển phiên sang `ABANDONED`, không gửi event M06/M07. |
| `AR13-02` | Người học bấm "Làm lại phiên" ở bước 5/10 | Phiên cũ chuyển `ABANDONED`, khởi tạo phiên mới từ bước 1 với `SessionId` mới. |
| `AR13-03` | Kiểm thử hoàn tất luồng M03-ABANDON-RETRY-SESSION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-AR-F01` | Ghi vết `AbandonReason` trong `LearningSessions` | Phục vụ phân tích lý do người dùng bỏ dở | M03-T004 |

## 5. Tự kiểm M03-T013
- Đã hoàn thành đặc tả `M03-ABANDON-RETRY-SESSION-1.0`.
- Chốt nguyên tắc cấm cấp thưởng khi bỏ dở và giữ lịch sử khi làm lại.
- Ghi nhận 2 Regression Gates (`AR-G01`–`AR-G02`) và 3 Test Cases (`AR13-01`–`AR13-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế bỏ dở và làm lại phiên M03-T013 | WSA-7K2 |
