# Thiết kế kiểm tra và phê duyệt chính sách M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-POLICY-APPROVAL-WORKFLOW-1.0` |
| Task | M04-T040 |
| Đầu vào | M04-POLICY-PARAMETER-CATALOG-1.0 (M04-T039), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Vòng đời và quy trình phê duyệt 2 bước (`Four-Eyes Principle / Maker-Checker`) khi thay đổi tham số chính sách thuật toán SRS M04 |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả quy trình kiểm tra và phê duyệt (`Policy Governance & Approval Workflow`) khi thay đổi các cấu hình tham số thuật toán SRS trong M04.

- **Nguyên tắc Phê duyệt 2 Bước (`Four-Eyes Principle Invariant`)**:
  - Người đề xuất thay đổi tham số SRS (`Maker / Author`) KHÔNG ĐƯỢC PHÉP tự phê duyệt bản dự thảo chính sách của mình (`Checker != Maker`).
  - Mọi thay đổi tham số chính sách BẮT BUỘC trải qua 4 trạng thái: `DRAFT` $\to$ `PENDING_APPROVAL` $\to$ `APPROVED` $\to$ `ACTIVE`.
- **Ghi vết Kiểm toán Phê duyệt Bất biến (`Audit Log Preservation Invariant`)**: 100% quyết định phê duyệt hoặc từ chối BẮT BUỘC lưu trữ vào bảng `SrsPolicyGovernanceAuditLogs` kèm thời điểm và lý do.

## 2. Luồng Phê duyệt Tham số Chính sách SRS (Policy Approval Workflow)

```mermaid
graph TD
    AdminMaker[Admin 1 - Maker] -->|Submit Draft Change| Draft[State = DRAFT]
    Draft -->|Request Review| Pending[State = PENDING_APPROVAL]
    Pending --> AdminChecker{Admin 2 - Checker (Checker != Maker)?}
    AdminChecker -->|Reject| Rejected[State = REJECTED]
    AdminChecker -->|Approve| Approved[State = APPROVED & Scheduled Active]
    Approved --> Active[State = ACTIVE in SRS Engine]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `PA-G01`: 100% request tự phê duyệt chính sách của chính người tạo bị hệ thống từ chối với lỗi HTTP 403.
- `PA-G02`: Bảng `SrsPolicyGovernanceAuditLogs` lưu đầy đủ vết duyệt cho mọi lần kích hoạt chính sách mới.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `PA40-01` | Admin 1 gửi dự thảo thay đổi `SrsDefaultEaseFactor = 2.40`, sau đó tự bấm "Phê duyệt" | System chặn và ném lỗi HTTP 403 `MAKER_CANNOT_APPROVE_OWN_POLICY`. |
| `PA40-02` | Admin 2 bấm "Phê duyệt" chính sách do Admin 1 gửi | Trạng thái chuyển `APPROVED`, lưu 1 dòng log kiểm toán M11. |
| `PA40-03` | Kiểm thử hoàn tất luồng M04-POLICY-APPROVAL-WORKFLOW-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-PA-F01` | Tích hợp hệ thống phân quyền Maker-Checker với M01/M11 | Đảm bảo tính tuân thủ quản trị hệ thống | M11-T012 |

## 5. Tự kiểm M04-T040
- Đã hoàn thành đặc tả `M04-POLICY-APPROVAL-WORKFLOW-1.0`.
- Chốt nguyên tắc Four-Eyes Principle (Maker != Checker) và máy trạng thái phê duyệt chính sách.
- Ghi nhận 2 Regression Gates (`PA-G01`–`PA-G02`) và 3 Test Cases (`PA40-01`–`PA40-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế kiểm tra và phê duyệt chính sách M04-T040 | WSA-7K2 |
