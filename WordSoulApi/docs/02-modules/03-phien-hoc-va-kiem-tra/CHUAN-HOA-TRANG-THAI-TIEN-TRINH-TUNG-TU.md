# Chuẩn hóa trạng thái tiến trình từng từ M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-ITEM-PROGRESS-STATE-1.0` |
| Task | M03-T033 |
| Đầu vào | M03-QUESTION-FLOW-VERSIONING-1.0 (M03-T017), M03-SUBMIT-IDEMPOTENCY-1.0 (M03-T025), M03-CONCURRENT-SUBMISSION-HANDLING-1.0 (M03-T026) |
| Phạm vi | Máy trạng thái tiến trình của từng mục từ vựng trong phiên (`ItemProgressState`), quy tắc chuyển bước và khả năng tái dựng trạng thái từ nhật ký làm bài |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa máy trạng thái tiến trình (`ItemProgressState`) của từng mục từ vựng trong suốt quá trình người học thực hiện phiên học M03.

- **Khả năng Tái dựng từ Nhật ký (`Deterministic State Reconstruction Invariant`)**:
  - Trạng thái tiến trình của từng từ trong phiên BẮT BUỘC tái dựng lại được 100% từ danh sách các bản ghi lượt trả lời (`SessionAnswerLogs`) trong DB.
- **Tính Không nhảy Bước Trai phép (`Strict State Transition Invariant`)**:
  - Mỗi mục từ vựng trong phiên học chuyển qua các trạng thái: `UNTOUCHED` $\to$ `FLASHCARD_VIEWED` $\to$ `RECOGNITION_PASSED` $\to$ `RECALL_COMPLETED`. CẤM nhảy trực tiếp từ `UNTOUCHED` sang `RECALL_COMPLETED` mà không đi qua các bước trung gian.

## 2. Máy Trạng thái Tiến trình Mục từ (Item Progress State Machine)

```text
[UNTOUCHED] ──(View Flashcard)──> [FLASHCARD_VIEWED] ──(Pass Quiz 1)──> [RECOGNITION_PASSED]
                                                                                │
                                                                       (Pass Active Recall)
                                                                                ▼
                                                                       [RECALL_COMPLETED]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IP-G01`: 100% bản ghi `ItemProgressState` tái dựng lại từ log trả lời cho kết quả khớp hoàn toàn với state lưu trong DB.
- `IP-G02`: Request nhảy bước trái phép bị chặn với lỗi HTTP 400 `INVALID_STEP_TRANSITION`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IP33-01` | Người học vượt qua bước trắc nghiệm chọn nghĩa của từ A | `ItemProgressState` của từ A chuyển sang `RECOGNITION_PASSED`. |
| `IP33-02` | Thử gửi kết quả chốt từ khi chưa xem flashcard ở luồng học mới | System từ chối với lỗi HTTP 400 `INVALID_STEP_TRANSITION`. |
| `IP33-03` | Kiểm thử hoàn tất luồng M03-ITEM-PROGRESS-STATE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-IP-F01` | Bổ sung bảng `SessionItemProgresses` kẹp trong `LearningSession` aggregate | Lưu trữ tiến độ từng từ trong phiên | M03-T004 |

## 5. Tự kiểm M03-T033
- Đã hoàn thành đặc tả `M03-ITEM-PROGRESS-STATE-1.0`.
- Chốt máy trạng thái tiến trình mục từ 4 bước bất biến.
- Ghi nhận 2 Regression Gates (`IP-G01`–`IP-G02`) và 3 Test Cases (`IP33-01`–`IP33-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa trạng thái tiến trình từng từ M03-T033 | WSA-7K2 |
