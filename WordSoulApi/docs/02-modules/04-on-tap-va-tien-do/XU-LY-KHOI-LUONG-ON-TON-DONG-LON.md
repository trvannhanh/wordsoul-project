# Xử lý khối lượng ôn tồn đọng lớn M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-BACKLOG-MANAGEMENT-1.0` |
| Task | M04-T022 |
| Đầu vào | M04-REVIEW-PRIORITY-SCORE-1.0 (M04-T021), M10-NOTIFICATION-DICT-1.0 (M10-T001) |
| Phạm vi | Chiến lược phân luồng chia nhỏ khối lượng tồn đọng (`Review Backlog Slicing Strategy`) khi người học nghỉ dài ngày có $> 100$ từ đến hạn ôn tập cùng lúc |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định chiến lược xử lý khi người học bị tích tụ khối lượng từ vựng cần ôn tập tồn đọng quá lớn (`Overwhelming Review Backlog`) trong M04.

- **Chiến lược Chia nhỏ Khối lượng Ôn (`Backlog Slicing Invariant`)**:
  - Khi tổng số từ đến hạn ôn $OverdueTotal > 50$ từ:
    - Hệ thống KHÔNG ĐƯỢC ép người học ôn toàn bộ trong 1 phiên.
    - Tự động phân mảng thành các "Gói ôn khôi phục" (`Recovery Review Batches`) với kích thước tối đa $20$ từ/phiên, sắp xếp ưu tiên theo `ReviewPriorityScore`.
- **Cấm Bỏ rơi Từ Quá hạn Lâu (`No Starvation Invariant`)**:
  - Mỗi Gói ôn khôi phục BẮT BUỘC dành ít nhất $20\%$ dung lượng (4 từ) cho nhóm từ quá hạn lâu nhất ($OverdueDays > 30$) để tránh hiện tượng bỏ quên từ cũ.

## 2. Quy trình Cấu trúc Gói Ôn Khôi phục Tồn đọng (Backlog Slicing Engine)

```text
[Overdue Total: 150 Items]
       │
       ▼
[Sort by Priority Score P] ──> Select Top 16 High Priority Items (80%)
       │
       └────────────────────> Select Top 4 Longest Overdue Items (20%)
                                        │
                                        ▼
                         [Recovery Batch 1: 20 Items Queue]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `BM-G01`: 100% người dùng có $OverdueTotal > 50$ nhận được danh sách phiên ôn gợi ý kẹp trần tối đa $20$ từ/phiên.
- `BM-G02`: Mỗi gói ôn khôi phục luôn chứa ít nhất $20\%$ các từ có $OverdueDays > 30$ (nếu có).

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `BM22-01` | Người dùng bỏ học 2 tháng, mở ứng dụng có 200 từ đến hạn ôn | API trả về gợi ý "Phiên ôn khôi phục 1" gồm 20 từ (16 từ ưu tiên + 4 từ quá hạn sâu). |
| `BM22-02` | Người dùng hoàn thành xong Gói ôn khôi phục 1 | Hàng đợi tự động chuẩn bị Gói ôn khôi phục 2 với 20 từ tiếp theo. |
| `BM22-03` | Kiểm thử hoàn tất luồng M04-BACKLOG-MANAGEMENT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-BM-F01` | Truyền cờ `IsBacklogRecoverySession = true` trong response M04$\to$M03 | Giao diện UI M03 hiển thị badge "Khôi phục phong độ" | M04-T023 |

## 5. Tự kiểm M04-T022
- Đã hoàn thành đặc tả `M04-BACKLOG-MANAGEMENT-1.0`.
- Chốt thuật toán chia gói 20 từ và tỷ lệ $80/20$ bảo đảm không bỏ rơi từ quá hạn sâu.
- Ghi nhận 2 Regression Gates (`BM-G01`–`BM-G02`) và 3 Test Cases (`BM22-01`–`BM22-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xử lý khối lượng ôn tồn đọng lớn M04-T022 | WSA-7K2 |
