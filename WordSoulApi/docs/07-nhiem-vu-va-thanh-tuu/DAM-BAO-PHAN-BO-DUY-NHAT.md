# Đảm bảo phân bổ duy nhất M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-QUEST-UNIQUE-ASSIGNMENT-1.0` |
| Task | M07-T010 |
| Đầu vào | M07-QUEST-SNAPSHOT-VERSIONING-1.0 (M07-T005), M07-QUEST-SELECTION-STRATEGY-1.0 (M07-T008), M07-NEW-USER-MISSING-DATA-HANDLING-1.0 (M07-T009) |
| Phạm vi | Cơ chế bảo đảm phân bổ nhiệm vụ ngày duy nhất (`Single Daily Assignment Guarantee`), chống tạo trùng lặp nhiệm vụ khi gọi trigger nhiều lần trong cùng một ngày nghiệp vụ |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định cơ chế đảm bảo tính duy nhất khi phân bổ nhiệm vụ ngày (`Unique Daily Assignment Invariant`) trong M07.

- **Ràng buộc Khóa Duy nhất theo Ngày Nghiệp vụ (`Daily Unique Constraint Invariant`)**:
  - Thực thể `UserQuest` BẮT BUỘC có chỉ mục duy nhất Unique Index trong DB trên bộ 3 thuộc tính: `(UserId, AssignmentDateLocal, QuestDefinitionId)`.
  - Việc gọi lại trigger phân bổ nhiệm vụ ngày nhiều lần trong cùng 1 ngày (ví dụ do cron job chạy lặp hoặc người dùng mở app nhiều thiết bị) BẮT BUỘC trả về danh sách nhiệm vụ đã gán trước đó mà KHÔNG ĐƯỢC CHÈN THÊM bản ghi mới.
- **Tính Bất biến của Danh sách Nhiệm vụ Ngày (`Assigned Quest List Immutability`)**: Bộ 3 nhiệm vụ ngày sau khi đã gán thành công cho người dùng BẮT BUỘC giữ nguyên trong suốt 24h của ngày đó.

## 2. Quy trình Phân bổ Nhiệm vụ Ngày An toàn Idempotency (Unique Assignment Engine)

```mermaid
graph TD
    Req[Trigger Assignment for Today] --> QueryExisting{UserQuests Exist for (UserId, Today)?}
    QueryExisting -->|Yes| ReturnExisting[Return Existing Assigned Quests]
    QueryExisting -->|No| AcquireLock{Acquire Redlock lock_quest_assign_{userId}_{today}}
    AcquireLock -->|Wins| RunStrategy[Run Selection Strategy & Save DB]
    AcquireLock -->|Loses| WaitAndFetch[Wait 500ms & Fetch Existing Quests]
    RunStrategy --> ReleaseLock[Release Lock & Return Quests]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `UA-G01`: 100% trường hợp gọi trigger phân bổ 100 lần trong 1 ngày cho 1 người dùng chỉ sinh ra đúng 3 bản ghi `UserQuest`.
- `UA-G02`: DB throw `UniqueConstraintViolationException` nếu cố tình chèn trùng `(UserId, AssignmentDateLocal, QuestDefinitionId)`.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `UA10-01` | Chạy 10 thread đồng thời gọi API `GetDailyQuests` cho người dùng A lúc 00:00:01 UTC | Chỉ có đúng 1 thread tạo nhiệm vụ, 9 thread còn lại nhận lại cùng bộ 3 nhiệm vụ đó. |
| `UA10-02` | Người dùng đổi thiết bị điện thoại sang máy tính trong cùng 1 ngày | Cả hai thiết bị hiển thị khớp $100\%$ cùng bộ 3 nhiệm vụ ngày. |
| `UA10-03` | Kiểm thử hoàn tất luồng M07-QUEST-UNIQUE-ASSIGNMENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-UA-F01` | Cấu hình Unique Index `IX_UserQuests_UserId_AssignmentDateLocal_QuestDefinitionId` trong EF Core | Đảm bảo tính toàn vẹn dữ liệu DB | M07-T003 |

## 5. Tự kiểm M07-T010
- Đã hoàn thành đặc tả `M07-QUEST-UNIQUE-ASSIGNMENT-1.0`.
- Chốt Unique Index DB và cơ chế Redlock `lock_quest_assign_{userId}_{today}` chống phân bổ trùng.
- Ghi nhận 2 Regression Gates (`UA-G01`–`UA-G02`) và 3 Test Cases (`UA10-01`–`UA10-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đảm bảo phân bổ duy nhất M07-T010 | WSA-7K2 |
