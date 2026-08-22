# Thiết kế mô phỏng tác động chính sách M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-POLICY-SIMULATION-ENGINE-1.0` |
| Task | M04-T041 |
| Đầu vào | M04-POLICY-APPROVAL-WORKFLOW-1.0 (M04-T040), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Công cụ chạy mô phỏng dự báo tác động (`SRS Policy Simulation Engine`) trên tập mẫu dữ liệu lịch sử anonymized trước khi duyệt kích hoạt chính sách SRS mới |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình và công cụ mô phỏng tác động (`Policy Simulation Engine`) trong M04.

- **Mô phỏng Độc lập Không Ảnh hưởng Dữ liệu Thật (`Isolated Dry-Run Invariant`)**:
  - Quá trình mô phỏng chính sách SRS mới BẮT BUỘC thực hiện trong môi trường Sandbox cách ly (`Simulation Context`).
  - Tuyệt đối CẤM ghi đè hoặc làm thay đổi chỉ số $Interval$, $EaseFactor$, $DueDateUtc$ của người học thật trong môi trường Production.
- **Bảo mật Anonymization Dữ liệu Mẫu (`Anonymized Sample Data Rule`)**: Tập dữ liệu lịch sử dùng để chạy mô phỏng BẮT BUỘC được ẩn danh hóa $100\%$ thông tin cá nhân (PII).

## 2. Quy trình Thực hiện Mô phỏng Chính sách (Policy Simulation Pipeline)

```mermaid
graph TD
    Maker[Submit New SRS Policy Draft] --> GenSample[Extract 1,000 Anonymized User Profiles]
    GenSample --> RunEngine[Run SRS Simulation Engine (30-day Projection)]
    RunEngine --> GenReport[Generate Simulation Impact Report]
    GenReport --> CheckSafety{Impact Metrics Within Safe Range?}
    CheckSafety -->|No - Workload Spike > 50%| FlagUnsafe[Flag UNSAFE Policy & Block Submission]
    CheckSafety -->|Yes| AttachReport[Attach Report to Checker Approval Request]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PS-G01`: 100% các phiên mô phỏng không tạo ra bất kỳ câu lệnh SQL `UPDATE` nào trên bảng `UserSenseProgresses` Production.
- `PS-G02`: Báo cáo mô phỏng trả về đủ 3 chỉ số dự báo: `ProjectedDailyOverdueVolume`, `ProjectedMasteryRate`, `ProjectedDropRate`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PS41-01` | Admin chạy mô phỏng dự thảo `DefaultEaseFactor = 1.80` cho 1,000 người dùng | Trả về báo cáo: Khối lượng ôn ngày tăng dự kiến $12\%$, tỷ lệ Mastered giảm $5\%$. Dữ liệu thật không đổi. |
| `PS41-02` | Dự thảo cấu hình gây bùng nổ khối lượng ôn $> 60\%$ trong 3 ngày | System tự động dán nhãn `UNSAFE_POLICY` và chặn gửi sang Checker duyệt. |
| `PS41-03` | Kiểm thử hoàn tất luồng M04-POLICY-SIMULATION-ENGINE-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PS-F01` | Tạo service `SrsPolicySimulationService` trong Infrastructure M04 | Cung cấp công cụ dự báo cho Admin Dashboard | M04-T040 |

## 5. Tự kiểm M04-T041
- Đã hoàn thành đặc tả `M04-POLICY-SIMULATION-ENGINE-1.0`.
- Chốt nguyên tắc dry-run cách ly $100\%$ và ngưỡng cảnh báo quá tải khối lượng ôn $> 50\%$.
- Ghi nhận 2 Regression Gates (`PS-G01`–`PS-G02`) và 3 Test Cases (`PS41-01`–`PS41-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế mô phỏng tác động chính sách M04-T041 | WSA-7K2 |
