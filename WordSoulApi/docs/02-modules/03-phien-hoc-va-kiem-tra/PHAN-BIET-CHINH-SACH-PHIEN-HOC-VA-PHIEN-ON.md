# Phân biệt chính sách phiên học và phiên ôn M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-SESSION-POLICY-1.0` |
| Task | M03-T002 |
| Đầu vào | M03-SESSION-DICT-1.0 (M03-T001), M02-LESSON-CONTENT-1.0 (M02-T009-A), M04-PROGRESS |
| Phạm vi | Quy định ranh giới điều kiện tạo, luồng bài tập, kết quả đầu ra và module tiếp nhận cho Phiên học mới (`NewLearningSession`) vs Phiên ôn tập (`ReviewSession`) |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy tắc phân tách chính sách giữa Phiên học mới (`NewLearningSession`) và Phiên ôn tập (`ReviewSession`).

- **Tính Riêng biệt của Nguồn Đầu vào (`Input Source Invariant`)**:
  - *NewLearningSession*: Khởi tạo dựa trên Bộ từ vựng (`VocabularySetId`) trong M02. Chỉ chứa các mục từ người học CHƯA bắt đầu tiến độ.
  - *ReviewSession*: Khởi tạo dựa trên Hàng đợi Ôn tập đến hạn (`DueReviewItemsQueue`) từ M04. Không phụ thuộc vào một bộ từ duy nhất.
- **Khác biệt Luồng Bài tập (`Workflow Differentiation Invariant`)**:
  - *NewLearningSession*: Bắt buộc có bước "Giới thiệu thẻ học liệu" (Card Introduction / Flashcard Display) trước khi kiểm tra.
  - *ReviewSession*: Đi thẳng vào các câu hỏi kiểm tra gợi nhớ (Recall Quiz), KHÔNG hiển thị thẻ xem trước để đảm bảo tính khách quan khi đo khả năng nhớ.

## 2. Ma trận So sánh Chính sách Phiên (Session Policy Comparison Matrix)

| Tiêu chí | Phiên học mới (`NewLearningSession`) | Phiên ôn tập (`ReviewSession`) |
|---|---|---|
| Nguồn dữ liệu | Bộ từ vựng M02 | Hàng đợi ôn tập đến hạn M04 |
| Bước hiển thị thẻ | BẮT BUỘC có bước flashcard xem từ | CẤM hiển thị flashcard trước câu hỏi |
| Số từ trong phiên | $5 \le N \le 20$ từ | $10 \le N \le 30$ từ |
| Kết quả gửi M04 | Khởi tạo hồ sơ nhớ mới (`InitializeRecord`) | Cập nhật khoảng cách SRS (`UpdateSRSInterval`) |
| Phát thưởng M06 | Cấp Gold + Exp + M07 Quest event | Cấp Exp + M07 Quest event |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `SP-G01`: 100% `ReviewSession` không hiển thị màn hình xem thẻ flashcard trước khi trả lời câu hỏi.
- `SP-G02`: `NewLearningSession` chỉ khởi tạo từ bộ từ vựng hợp lệ trong thư viện cá nhân M02.
- `SP-G03`: `ReviewSession` gửi kết quả gợi nhớ đầu tiên cho M04 để tính toán thuật toán SRS.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SP02-01` | Tạo phiên học mới cho bộ từ A1 | Khởi tạo `NewLearningSession`, hiển thị flashcard xem từ trước. |
| `SP02-02` | Tạo phiên ôn tập từ hàng đợi 15 từ đến hạn M04 | Khởi tạo `ReviewSession`, nhảy trực tiếp vào câu hỏi trắc nghiệm/điền từ. |
| `SP02-03` | Kiểm thử hoàn tất luồng M03-SESSION-POLICY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-SP-F01` | Cần tách DTO `CreateSessionRequest` theo `SessionType` | Đảm bảo truyền tham số đầu vào chính xác | M03-T004 |

## 5. Tự kiểm M03-T002
- Đã hoàn thành đặc tả `M03-SESSION-POLICY-1.0`.
- Chốt ma trận so sánh chính sách phiên và 3 Regression Gates (`SP-G01`–`SP-G03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả phân biệt chính sách phiên học và phiên ôn M03-T002 | WSA-7K2 |
