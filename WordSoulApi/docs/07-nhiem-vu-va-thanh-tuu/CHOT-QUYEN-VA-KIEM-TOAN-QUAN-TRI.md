# Chốt quyền và kiểm toán quản trị M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-GOVERNANCE-AUDIT-1.0` |
| Task | M07-T042 |
| Đầu vào | M01-ROLE-PERMISSION-MATRIX-1.0 (M01-T028), M07-QUEST-CHANGE-LIFECYCLE-1.0 (M07-T004), M11-CONFIG-REG-1.0 (M11-T012) |
| Phạm vi | Ma trận phân quyền thao tác cấu hình nhiệm vụ (`Quest Governance Authorization Matrix`) và cơ chế ghi log kiểm toán bất biến `QuestGovernanceAuditLogs` |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả cơ chế phân quyền và ghi log kiểm toán (`Quest Governance Audit & Authorization`) trong M07.

- **Phân quyền Thao tác Cấu hình Nhiệm vụ theo Vai trò (`Role-Based Governance Invariant`)**:
  - *Quest Content Author*: Được phép tạo mới bản dự thảo `DRAFT` và gửi duyệt `PENDING_APPROVAL`.
  - *Lead Quest Admin*: Được phép duyệt `APPROVED` và kích hoạt `ACTIVE` định nghĩa nhiệm vụ mới.
  - Tuyệt đối CẤM Content Author tự phê duyệt bản cấu hình nhiệm vụ do chính mình tạo ra (`Maker != Checker`).
- **Lưu trữ Vết Kiểm toán Bất biến (`Immutable Governance Audit Log`)**: 100% thay đổi cấu hình nhiệm vụ BẮT BUỘC lưu vết `QuestGovernanceAuditLogs` bao gồm `AdminUserId`, `ActionType`, `OldSnapshotJson`, `NewSnapshotJson` và `ReasonCode`.

## 2. Bảng Ma trận Phân quyền Quản trị Nhiệm vụ (Governance Matrix)

| Thao tác Quản trị | Vai trò Yêu cầu | Yêu cầu Maker != Checker | Ghi log Kiểm toán M11 |
|---|---|---|---|
| `CreateDraftQuest` | `QUEST_AUTHOR` | Không | Bắt buộc |
| `ApproveQuest` | `LEAD_QUEST_ADMIN` | **Bắt buộc (Maker != Checker)** | Bắt buộc |
| `DeprecateQuest` | `LEAD_QUEST_ADMIN` | Không | Bắt buộc |
| `ForceGrantQuest` | `SUPER_ADMIN` | Không | Bắt buộc |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QA-G01`: 100% request tự duyệt cấu hình nhiệm vụ của chính người tạo bị từ chối với HTTP 403.
- `QA-G02`: Bảng `QuestGovernanceAuditLogs` lưu vết $100\%$ các thao tác chuyển trạng thái định nghĩa nhiệm vụ.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QA42-01` | Author A tạo định nghĩa nhiệm vụ mới, sau đó tự bấm "Phê duyệt" | System từ chối, ném lỗi HTTP 403 `MAKER_CANNOT_APPROVE_OWN_QUEST`. |
| `QA42-02` | Lead Admin B bấm "Phê duyệt" nhiệm vụ do Author A tạo | Trạng thái chuyển `APPROVED`, lưu 1 dòng log kiểm toán M11. |
| `QA42-03` | Kiểm thử hoàn tất luồng M07-QUEST-GOVERNANCE-AUDIT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-QA-F01` | Áp dụng policy `[Authorize(Policy = "CanManageQuestDefinitions")]` trên AdminQuestController | Bảo đảm an toàn API quản trị nhiệm vụ | M07-T004 |

## 5. Tự kiểm M07-T042
- Đã hoàn thành đặc tả `M07-QUEST-GOVERNANCE-AUDIT-1.0`.
- Chốt ma trận phân quyền quản trị Maker-Checker và log kiểm toán bất biến.
- Ghi nhận 2 Regression Gates (`QA-G01`–`QA-G02`) và 3 Test Cases (`QA42-01`–`QA42-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chốt quyền và kiểm toán quản trị M07-T042 | WSA-7K2 |
