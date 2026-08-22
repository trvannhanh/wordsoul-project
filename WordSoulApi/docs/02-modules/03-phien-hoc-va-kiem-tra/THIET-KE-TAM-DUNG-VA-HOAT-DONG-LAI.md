# Thiết kế tạm dừng và hoạt động lại M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-PAUSE-RESUME-MECHANICS-1.0` |
| Task | M03-T011 |
| Đầu vào | M03-SESSION-LIFECYCLE-1.0 (M03-T003), M03-RESUME-SESSION-1.0 (M03-T010) |
| Phạm vi | Trạng thái `PAUSED` trong vòng đời phiên học M03, loại trừ khoảng thời gian tạm dừng khi tính thời gian trả lời câu hỏi và quy tắc khôi phục |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế tạm dừng (`Pause`) và kích hoạt lại (`Resume`) phiên học trong M03.

- **Loại trừ Thời gian Tạm dừng khi Tính Tốc độ (`Active Duration Calculation Invariant`)**:
  - Khi phiên học ở trạng thái `PAUSED`, đồng hồ đếm thời gian trả lời câu hỏi (`ActiveDurationSeconds`) BẮT BUỘC bị đóng băng.
  - Khoảng thời gian người học tạm dừng (ví dụ nghỉ 15 phút) KHÔNG ĐƯỢC TÍNH vào thời gian làm bài của câu hỏi hay tổng thời hạn làm bài của phiên học.
- **Tính Bất biến Trạng thái Bước (`Step Preservation Invariant`)**:
  - Tạm dừng phiên tại câu thứ $K$ không làm mất bất kỳ kết quả chấm hay dữ liệu câu trả lời nào từ câu $1$ đến câu $K-1$.

## 2. Máy Trạng thái Tạm dừng (Pause State Transitions)

```text
  [IN_PROGRESS] ──(User Pause / App Inactive)──> [PAUSED]
        ▲                                          │
        │                                          │
        └───(User Resume / Active API Call)────────┘
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PR-G01`: 100% khoảng thời gian phiên nằm ở trạng thái `PAUSED` bị trừ khỏi tổng `TotalDurationSeconds` của phiên.
- `PR-G02`: Tạm dừng và quay lại ở bước 4 không làm câu 4 bị biến đổi câu hỏi hay đổi vị trí đáp án.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PR11-01` | Người học bấm Pause ở phút thứ 2, nghỉ 10 phút rồi Resume làm tiếp | `TotalDurationSeconds` chốt phiên chỉ tính khoảng thời gian làm bài thực tế (loại bỏ 10 phút nghỉ). |
| `PR11-02` | Người học ẩn app xuống background (App Switch) | System tự chuyển trạng thái phiên sang `PAUSED`. |
| `PR11-03` | Kiểm thử hoàn tất luồng M03-PAUSE-RESUME-MECHANICS-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-PR-F01` | Thêm thuộc tính `TotalPausedDurationSeconds` vào Entity `LearningSession` | Đảm bảo trừ đúng bù thời gian tạm dừng | M03-T004 |

## 5. Tự kiểm M03-T011
- Đã hoàn thành đặc tả `M03-PAUSE-RESUME-MECHANICS-1.0`.
- Chốt nguyên tắc loại trừ thời gian tạm dừng khỏi duration làm bài.
- Ghi nhận 2 Regression Gates (`PR-G01`–`PR-G02`) và 3 Test Cases (`PR11-01`–`PR11-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế tạm dừng và hoạt động lại M03-T011 | WSA-7K2 |
