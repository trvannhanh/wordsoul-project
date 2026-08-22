# Chốt tiêu chí đủ điều kiện M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-ELIGIBILITY-CRITERIA-1.0` |
| Task | M07-T007 |
| Đầu vào | M07-QUEST-TARGET-SPEC-1.0 (M07-T003), M01-USER-LEVEL-DICT-1.0 (M01-T003) |
| Phạm vi | Bộ tiêu chí lọc và kiểm tra tính đủ điều kiện (`Eligibility Evaluator`) trước khi phân bổ nhiệm vụ ngày cho người học (Level tối thiểu, tài khoản hoạt động, loại trừ bộ từ thu hồi) |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định bộ tiêu chí xác định tính đủ điều kiện (`Quest Eligibility Criteria`) khi chọn nhiệm vụ ngày cho người dùng trong M07.

- **Đánh giá Đủ Điều kiện An toàn (`Safe Eligibility Evaluation Invariant`)**:
  - Một định nghĩa nhiệm vụ `QuestDefinition` CHỈ ĐƯỢC PHÂN BỔ cho người dùng nếu thỏa mãn đồng thời 3 điều kiện:
    1. Trạng thái tài khoản `UserAccountStatus == ACTIVE`.
    2. Cấp độ người dùng `UserLevel >= QuestDefinition.MinUserLevel`.
    3. Người dùng có ít nhất 1 bộ từ vựng hợp lệ (không bị `RECALLED`) trong thư viện M02.
- **Xử lý Thiếu Dữ liệu Mặc định An toàn (`Safe Default on Missing Data Invariant`)**:
  - Nếu không thể truy vấn được dữ liệu cấp độ (ví dụ M01 bị gián đoạn), Evaluator mặc định áp dụng cấp độ 1 (`Level = 1`) để phân bổ các nhiệm vụ dễ nhất thay vì ném lỗi crash ứng dụng.

## 2. Ma trận Tiêu chí Đủ Điều kiện Phân bổ Nhiệm vụ (Eligibility Criteria Matrix)

| Tiêu chí | Điều kiện Đạt | Hành vi khi Không Đạt | Fallback khi Thiếu Dữ liệu |
|---|---|---|---|
| Trạng thái Tài khoản | `ACTIVE` | Không phân bổ nhiệm vụ | Coi như `INACTIVE` (Chặn) |
| Cấp độ Người dùng | `UserLevel >= MinUserLevel` | Bỏ qua nhiệm vụ đó, chọn nhiệm vụ khác | Coi `UserLevel = 1` |
| Thư viện Từ vựng | Số bộ `PUBLISHED > 0` | Gán bộ từ hệ thống mặc định | Dùng bộ từ hệ thống mặc định |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QE-G01`: 100% người dùng cấp độ 1 không bị phân bổ các nhiệm vụ nâng cao yêu cầu `MinUserLevel >= 5`.
- `QE-G02`: Sự cố gián đoạn dữ liệu M01 tự động fallback về `Level 1` mà không làm hỏng tiến trình gán nhiệm vụ ngày.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QE07-01` | Learner mới tạo tài khoản (Level 1) đăng nhập nhận nhiệm vụ ngày | Chỉ nhận các nhiệm vụ có `MinUserLevel = 1`, các nhiệm vụ Level 5 bị loại trừ. |
| `QE07-02` | M01 bị gián đoạn kết nối không trả về Level | Evaluator tự động gán `Level = 1`, phân bổ thành công 3 nhiệm vụ ngày cơ bản. |
| `QE07-03` | Kiểm thử hoàn tất luồng M07-QUEST-ELIGIBILITY-CRITERIA-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QE-F01` | Tạo service `QuestEligibilityEvaluator` trong Domain M07 | Đánh giá điều kiện gán nhiệm vụ | M07-T008 |

## 5. Tự kiểm M07-T007
- Đã hoàn thành đặc tả `M07-QUEST-ELIGIBILITY-CRITERIA-1.0`.
- Chốt ma trận 3 tiêu chí đủ điều kiện và cơ chế Safe Fallback khi thiếu dữ liệu.
- Ghi nhận 2 Regression Gates (`QE-G01`–`QE-G02`) và 3 Test Cases (`QE07-01`–`QE07-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chốt tiêu chí đủ điều kiện M07-T007 | WSA-7K2 |
