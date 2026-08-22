# Chuẩn hóa giá trị ban đầu M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-INITIAL-PROGRESS-VALUES-1.0` |
| Task | M04-T004 |
| Đầu vào | M04-INIT-PROFILE-CONDITIONS-1.0 (M04-T003), M04-POLICY-PARAMETER-CATALOG-1.0 (M04-T039) |
| Phạm vi | Quy định giá trị mặc định ban đầu khi tạo mới bản ghi `UserSenseProgress` và phân biệt giữa "Học mới tự nhiên" vs "Đánh dấu đã biết" |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa bộ giá trị tham số mặc định khi khởi tạo một hồ sơ ghi nhớ mới (`UserSenseProgress`) trong M04.

- **Giá trị Ban đầu Nằm trong Khoảng An toàn (`Initial Bounds Invariant`)**:
  - Khi một mục từ vựng được học mới lần đầu:
    - `State` = `LEARNING`
    - `IntervalDays` = $1$ ngày (hoặc tham số `SrsInitialIntervalDays`)
    - `EaseFactor` = $2.50$ (hoặc tham số `SrsDefaultEaseFactor`)
    - `RepetitionCount` = $0$
    - `DueDateUtc` = `LastReviewedAtUtc + 1 Day`
- **Khác biệt giữa "Học mới" và "Đánh dấu đã biết" (`Known vs Learned Invariant`)**:
  - Hành vi bấm "Đánh dấu đã biết" (`Mark as Known`) khởi tạo hồ sơ với `State = MASTERED`, `IntervalDays = 21` ngày. Tuyệt đối CẤM coi hành vi đánh dấu đã biết như một lần học tập thông thường ($Interval = 1$).

## 2. Bảng Ma trận Giá trị Khởi tạo Ban đầu (Initial Values Matrix)

| Thuộc tính | Học mới tự nhiên (M03 New Learning) | Đánh dấu đã biết (Mark as Known) |
|---|---|---|
| `MemoryState` | `LEARNING` | `MASTERED` |
| `IntervalDays` | $1$ ngày | $21$ ngày |
| `EaseFactor` | $2.50$ | $2.50$ |
| `RepetitionCount` | $0$ | $3$ |
| `MasteryScore` | $10.0\%$ | $85.0\%$ |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IV-G01`: 100% hồ sơ ghi nhớ học mới có $EaseFactor = 2.50$ và $IntervalDays = 1$.
- `IV-G02`: Đánh dấu đã biết tạo hồ sơ ở trạng thái `MASTERED` với $IntervalDays = 21$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IV04-01` | Người học bấm học từ vựng mới lần đầu tiên | Tạo `UserSenseProgress` với `Interval = 1`, `EF = 2.50`, `State = LEARNING`. |
| `IV04-02` | Người học bấm "Đánh dấu đã biết" từ vựng A | Tạo `UserSenseProgress` với `Interval = 21`, `EF = 2.50`, `State = MASTERED`. |
| `IV04-03` | Kiểm thử hoàn tất luồng M04-INITIAL-PROGRESS-VALUES-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-IV-F01` | Đọc cấu hình từ `SrsInitialIntervalDays` trong M11 khi tạo bản ghi | Cho phép thay đổi tham số khởi tạo linh hoạt | M04-T039 |

## 5. Tự kiểm M04-T004
- Đã hoàn thành đặc tả `M04-INITIAL-PROGRESS-VALUES-1.0`.
- Chốt bộ giá trị mặc định cho Học mới và Đánh dấu đã biết.
- Ghi nhận 2 Regression Gates (`IV-G01`–`IV-G02`) và 3 Test Cases (`IV04-01`–`IV04-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa giá trị ban đầu M04-T004 | WSA-7K2 |
