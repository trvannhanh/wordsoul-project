# Chuẩn hóa câu hỏi loại từ và loại thẻ M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-QUESTION-CARD-TAXONOMY-1.0` |
| Task | M03-T031 |
| Đầu vào | M03-NEW-LEARNING-FLOW-1.0 (M03-T015), M03-QUESTION-DATA-TYPES-1.0 (M03-T019) |
| Phạm vi | Phân loại 4 dạng câu hỏi/thẻ bài tập chính (`MultipleChoice`, `TextRecall`, `ClozeSentence`, `AudioListening`), quy tắc chấm và lựa chọn dạng câu hỏi phù hợp |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này phân loại và đóng băng quy tắc cho 4 dạng thẻ bài tập/câu hỏi trong M03.

- **Ranh giới Dạng Thẻ theo Giai đoạn Luồng (`Flow Stage Card Mapping Invariant`)**:
  - *Giai đoạn 1 (Xem từ)*: Thẻ Flashcard Giới thiệu (`FLASHCARD_INTRO`).
  - *Giai đoạn 2 (Nhận diện)*: Thẻ Trắc nghiệm chọn nghĩa (`MULTIPLE_CHOICE`).
  - *Giai đoạn 3 (Gợi nhớ chủ động)*: Thẻ Gõ từ (`TEXT_RECALL`), Thẻ Điền từ vào câu (`CLOZE_SENTENCE`), Thẻ Nghe chép từ (`AUDIO_LISTENING`).
- **Chính xác Quy tắc Chấm theo Dạng Thẻ (`Card-Specific Grading Invariant`)**: Mỗi dạng thẻ có Engine chấm riêng biệt tuân thủ tuyệt đối chuẩn `M03-TEXT-ANSWER-NORMALIZATION-1.0`.

## 2. Bảng Phân loại Dạng Thẻ Bài tập (Question Card Taxonomy Matrix)

| Loại Thẻ | Mã Dạng | Yêu cầu Input | Engine chấm | Sử dụng trong Phiên |
|---|---|---|---|---|
| Flashcard Xem từ | `FLASHCARD_INTRO` | Không chấm (User Next) | N/A | Chỉ Phiên Học mới |
| Trắc nghiệm chọn nghĩa | `MULTIPLE_CHOICE` | Chọn 1 trong 4 ID | Exact ID Match | Học mới & Ôn tập |
| Gõ từ tự do | `TEXT_RECALL` | Chuỗi văn bản | Normalizer + Levenshtein | Học mới & Ôn tập |
| Điền từ vào câu | `CLOZE_SENTENCE` | Chuỗi văn bản kẹp ngữ cảnh | Normalizer + Levenshtein | Học mới & Ôn tập |
| Nghe phát âm chép từ | `AUDIO_LISTENING` | Phím Audio + Text Input | Normalizer + Levenshtein | Học mới & Ôn tập |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QT-G01`: 100% câu hỏi trong phiên được gán `CardType` thuộc 5 dạng mã hóa chuẩn ở trên.
- `QT-G02`: Thẻ `FLASHCARD_INTRO` không bao giờ xuất hiện trong phiên ôn tập `ReviewSession`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QT31-01` | Tạo phiên ôn tập từ hàng đợi 10 từ | Danh sách câu hỏi chỉ gồm `MULTIPLE_CHOICE`, `TEXT_RECALL`, `CLOZE_SENTENCE`, `AUDIO_LISTENING`, không chứa `FLASHCARD_INTRO`. |
| `QT31-02` | Làm bài thẻ `CLOZE_SENTENCE` điền từ *"apple"* vào câu `"She ate an ____."` | System chấm bằng `ClozeSentenceGradingEngine`, so sánh với nghĩa từ phiên. |
| `QT31-03` | Kiểm thử hoàn tất luồng M03-QUESTION-CARD-TAXONOMY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-QT-F01` | Cần bổ sung Enum `QuestionCardType` trong Domain | Đảm bảo phân nhánh xử lý câu hỏi đúng loại | M03-T019 |

## 5. Tự kiểm M03-T031
- Đã hoàn thành đặc tả `M03-QUESTION-CARD-TAXONOMY-1.0`.
- Chốt danh mục 5 loại thẻ bài tập và ma trận phân bổ luồng.
- Ghi nhận 2 Regression Gates (`QT-G01`–`QT-G02`) và 3 Test Cases (`QT31-01`–`QT31-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa câu hỏi loại từ và loại thẻ M03-T031 | WSA-7K2 |
