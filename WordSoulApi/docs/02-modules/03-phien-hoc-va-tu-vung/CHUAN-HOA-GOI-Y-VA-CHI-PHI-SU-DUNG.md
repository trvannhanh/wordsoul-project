# Chuẩn hóa gợi ý và chi phí sử dụng M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-HINT-USAGE-COST-SPEC-1.0` |
| Task | M03-T030 |
| Đầu vào | M03-HINT-COST-BOUNDS-1.0 (M03-T030), M06-ASSET-LEDGER-MODEL-1.0 (M06-T003) |
| Phạm vi | Loại gợi ý trợ giúp trong phiên học (`Hint Types`), chi phí tiêu tốn tài sản kinh tế (Gold/Gems) và tác động giảm điểm chấm SRS |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa cơ chế gợi ý và chi phí sử dụng (`Hint System & Economic Cost Specification`) trong M03.

- **Khấu trừ Tài sản Kinh tế thuộc Quyền M06 (`M06 Deduction Invariant`)**:
  - Việc trừ Gold khi dùng gợi ý BẮT BUỘC thực hiện qua lệnh giao dịch sổ cái M06 `DEBIT_HINT_PURCHASE`.
  - Nếu số dư Gold không đủ ($Balance < Cost$), hệ thống BẮT BUỘC từ chối cung cấp gợi ý và trả lỗi HTTP 400 `INSUFFICIENT_FUNDS`.
- **Tác động Giảm Điểm Chấm bài SRS (`SRS Scoring Penalty Invariant`)**:
  - Dùng Gợi ý 1 (Hiển thị ký tự đầu / Loại 2 phương án sai): Điểm SRS bị trừ $1$ cấp ($QualityScore = \max(1, OriginalScore - 1)$).
  - Dùng Gợi ý 2 (Hiển thị đáp án): Chấm `QUALITY_0` (Quên hoàn toàn).

## 2. Bảng Phân loại Gợi ý và Chi phí Kinh tế (Hint Cost Matrix)

| Loại Gợi ý | Tác dụng Trợ giúp | Chi phí Gold | Tác động Điểm SRS |
|---|---|---|---|
| `HINT_ELIMINATE_50` | Loại bỏ 2 phương án trắc nghiệm sai | 10 Gold | Trừ 1 cấp QualityScore |
| `HINT_FIRST_LETTER` | Hiển thị chữ cái đầu tiên của từ | 15 Gold | Trừ 1 cấp QualityScore |
| `HINT_REVEAL_ANSWER` | Hiển thị toàn bộ đáp án đúng | 50 Gold | Gán cố định QualityScore = 0 |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `HC-G01`: 100% yêu cầu dùng gợi ý khi không đủ số dư Gold bị chặn với HTTP 400.
- `HC-G02`: Sử dụng gợi ý `HINT_REVEAL_ANSWER` gán cố định $100\%$ `QualityScore = 0`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `HC30-01` | Learner có 5 Gold, bấm gợi ý `HINT_ELIMINATE_50` giá 10 Gold | System từ chối, hiển thị pop-up "Không đủ Gold". |
| `HC30-02` | Learner có 100 Gold, bấm `HINT_ELIMINATE_50`, trả lời đúng câu hỏi | Trừ 10 Gold (gửi M06), điểm SRS giảm từ 5 xuống 4. |
| `HC30-03` | Kiểm thử hoàn tất luồng M03-HINT-USAGE-COST-SPEC-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-HC-F01` | Phát `HintUsedIntegrationEvent` gửi cho M06 để trừ Gold | Phục vụ đồng bộ sổ cái kinh tế | M06-T003 |

## 5. Tự kiểm M03-T030
- Đã hoàn thành đặc tả `M03-HINT-USAGE-COST-SPEC-1.0`.
- Chốt ma trận chi phí Gold và quy tắc phạt điểm SRS khi dùng gợi ý.
- Ghi nhận 2 Regression Gates (`HC-G01`–`HC-G02`) và 3 Test Cases (`HC30-01`–`HC30-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa gợi ý và chi phí sử dụng M03-T030 | WSA-7K2 |
