# Thiết kế phiên bản hóa luồng câu hỏi M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-QUESTION-FLOW-VERSIONING-1.0` |
| Task | M03-T017 |
| Đầu vào | M03-NEW-LEARNING-FLOW-1.0 (M03-T015), M03-REVIEW-FLOW-1.0 (M03-T016), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Đóng băng mã phiên bản luồng câu hỏi (`FlowVersionCode`), hỗ trợ nâng cấp luồng không làm gãy các phiên đang chạy và quy tắc tương thích ngược |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định việc gắn mã phiên bản luồng câu hỏi (`FlowVersionCode`) cho từng phiên học trong M03.

- **Đóng băng Phiên bản Luồng theo Phiên (`Session Flow Version Pinning Invariant`)**:
  - Mỗi phiên học khi khởi tạo BẮT BUỘC lưu trữ mã `FlowVersionCode` (ví dụ: `FLOW_NEW_v1.0`, `FLOW_REVIEW_v1.0`).
  - Khi hệ thống nâng cấp hoặc phát hành phiên bản luồng mới trong M11, các phiên học đang `IN_PROGRESS` vẫn duy trì 100% quy tắc chuyển bước và loại câu hỏi của phiên bản luồng ban đầu.
- **Tương thích Ngược Đầu ra (`Backward Compatible Outcome Invariant`)**:
  - Dù chạy phiên bản luồng câu hỏi nào (`v1.0`, `v2.0`), định dạng sự kiện kết quả trả về M04 (`LearningSessionCompletedIntegrationEvent`) và M06 phải giữ nguyên 100% envelope chuẩn.

## 2. Danh mục Phiên bản Luồng Câu hỏi (Flow Version Registry)

| Mã Phiên bản | Loại Phiên | Cấu trúc Luồng | Trạng thái |
|---|---|---|---|
| `FLOW_NEW_v1.0` | Học mới | Flashcard Intro -> Recognition Quiz -> Active Recall | **ACTIVE** |
| `FLOW_REVIEW_v1.0` | Ôn tập | Direct Multiple Choice / Cloze / Listening Quiz | **ACTIVE** |
| `FLOW_NEW_v1.1` | Học mới | Flashcard Intro -> Pronunciation Listen -> Quiz | **DRAFT** |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `FV-G01`: 100% phiên học có thuộc tính `FlowVersionCode` hợp lệ trong danh mục M11.
- `FV-G02`: Nâng cấp luồng trên server không làm gián đoạn hay đổi hành vi của phiên đang chạy.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `FV17-01` | Tạo phiên học mới với cấu hình luồng mặc định | Gán `FlowVersionCode = "FLOW_NEW_v1.0"`. |
| `FV17-02` | Admin phát hành `FLOW_NEW_v2.0` khi người học đang ở bước 2 | Phiên hiện tại tiếp tục chạy tới hết bằng `FLOW_NEW_v1.0`. |
| `FV17-03` | Kiểm thử hoàn tất luồng M03-QUESTION-FLOW-VERSIONING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-FV-F01` | Cần thêm trường `FlowVersionCode` vào Entity `LearningSession.cs` | Đảm bảo lưu thông tin phiên bản luồng | M03-T004 |

## 5. Tự kiểm M03-T017
- Đã hoàn thành đặc tả `M03-QUESTION-FLOW-VERSIONING-1.0`.
- Chốt nguyên tắc pinning phiên bản luồng và 2 Regression Gates (`FV-G01`–`FV-G02`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế phiên bản hóa luồng câu hỏi M03-T017 | WSA-7K2 |
