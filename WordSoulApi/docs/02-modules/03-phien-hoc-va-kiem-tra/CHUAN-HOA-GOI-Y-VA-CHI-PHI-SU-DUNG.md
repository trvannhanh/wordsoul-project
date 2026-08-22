# Chuẩn hóa gợi ý và chi phí sử dụng M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-HINT-COST-POLICY-1.0` |
| Task | M03-T030 |
| Đầu vào | M03-NEW-LEARNING-FLOW-1.0 (M03-T015), M06-ASSET-ITEM-DICT-1.0 (M06-T001) |
| Phạm vi | Loại hình gợi ý trong phiên học (Gợi ý chữ cái đầu, Gợi ý dịch nghĩa, Loại 2 phương án sai), chi phí vật phẩm/Gold và tác động tới kết quả chấm điểm |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế sử dụng gợi ý (`Hint Usage`) và quy định chi phí trong phiên học M03.

- **Không dùng Gợi ý Sau khi Đã Gửi Đáp án (`No Post-Submission Hint Invariant`)**:
  - Người học CHỈ ĐƯỢC bấm xin gợi ý trước khi gửi câu trả lời. Sau khi đã bấm "Gửi đáp án", nút gợi ý bị khóa hoàn toàn.
- **Phân tách Trách nhiệm Tài sản với M06 (`M06 Asset Cost Delegation Invariant`)**:
  - M03 chịu trách nhiệm gửi request kiểm tra/tiêu tốn vật phẩm gợi ý (`ItemCode = "ITEM_HINT_TOKEN"`) sang M06. M03 CẤM tự ý trừ Gold/Gems trong DB.
- **Ghi nhận Tác động Chấm điểm (`Grading Impact Invariant`)**:
  - Việc sử dụng gợi ý trong câu hỏi làm giảm $50\%$ số điểm kinh nghiệm Exp nhận được cho câu đó và ghi dấu `UsedHintCount += 1` trong dữ liệu gửi M04.

## 2. Danh mục Các loại Gợi ý (Hint Types & Costs)

| Mã Gợi ý | Tên Gợi ý | Mô tả tác động | Chi phí | Tác động Điểm |
|---|---|---|---|---|
| `HINT_FIRST_LETTER` | Chữ cái đầu | Hiển thị chữ cái bắt đầu của từ vựng | 1 Thẻ Gợi ý (hoặc 10 Gold) | Trừ $50\%$ Exp |
| `HINT_REMOVE_TWO` | Loại 2 phương án sai | Loại bỏ 2 phương án nhiễu trong câu trắc nghiệm | 1 Thẻ Gợi ý (hoặc 15 Gold) | Trừ $50\%$ Exp |
| `HINT_TRANSLATION` | Gợi ý bổ sung | Hiển thị nghĩa tiếng Việt chi tiết hơn | Miễn phí (0 Gold) | Không trừ Exp |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `HC-G01`: 100% request gợi ý sau khi đã gửi câu trả lời bị chối bỏ với lỗi `HINT_NOT_ALLOWED_AFTER_SUBMISSION`.
- `HC-G02`: Sử dụng 1 thẻ gợi ý tạo lệnh trừ vật phẩm tương ứng tại M06.
- `HC-G03`: Câu hỏi có sử dụng gợi ý ghi nhận `UsedHintCount > 0` và trừ $50\%$ Exp thưởng.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HC30-01` | Bấm nút xin gợi ý chữ cái đầu trước khi trả lời | Hiển thị chữ cái đầu, M06 trừ 1 `ITEM_HINT_TOKEN`, ghi `UsedHintCount = 1`. |
| `HC30-02` | Thử bấm nút xin gợi ý sau khi câu trả lời đã chấm `INCORRECT` | System từ chối với lỗi `HINT_NOT_ALLOWED_AFTER_SUBMISSION`. |
| `HC30-03` | Trả lời đúng câu hỏi có dùng gợi ý | Nhận $50\%$ Exp tiêu chuẩn (ví dụ nhận 5 Exp thay vì 10 Exp). |
| `HC30-04` | Kiểm thử hoàn tất luồng M03-HINT-COST-POLICY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-HC-F01` | Thêm thuộc tính `UsedHintCount` vào `SubmitAnswerDto` | Lưu vết số lần dùng gợi ý của từng câu | M03-T024 |

## 5. Tự kiểm M03-T030
- Đã hoàn thành đặc tả `M03-HINT-COST-POLICY-1.0`.
- Chốt danh mục loại gợi ý, phân tách trừ tài sản M06 và 3 Regression Gates (`HC-G01`–`HC-G03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa gợi ý và chi phí sử dụng M03-T030 | WSA-7K2 |
