# Chuẩn hóa bỏ bộ khỏi thư viện M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-LIBRARY-REMOVE-SET-1.0` |
| Task | M02-T040 |
| Đầu vào | M02-LIBRARY-ADD-SET-1.0 (M02-T039), M03-SESSION-LIFECYCLE-1.0 (M03-T003), M04-PROGRESS |
| Phạm vi | Quy trình loại bỏ bộ từ vựng khỏi thư viện cá nhân người dùng (`UserLibrarySets`), bảo lưu lịch sử tiến độ M04 và xử lý phiên học M03 đang chạy |
| Tự kiểm | B-G01 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định tác động và quy trình khi người học loại bỏ một bộ từ vựng ra khỏi thư viện cá nhân (`Remove Set from Library`) trong M02.

- **Bảo lưu Lịch sử Tiến độ Ghi nhớ (`Progress Preservation Invariant`)**:
  - Loại bỏ bộ từ khỏi thư viện CHỈ XÓA liên kết `UserLibrarySet`.
  - Tuyệt đối CẤM xóa hoặc reset các bản ghi tiến độ ghi nhớ SRS (`UserSenseProgress`) trong M04 đối với các từ vựng thuộc bộ đó. Nếu người dùng thêm lại bộ từ trong tương lai, tiến độ học cũ phải được giữ nguyên 100%.
- **Bảo toàn Phiên học Đang chạy (`Active Session Continuity Invariant`)**:
  - Nếu người dùng bấm bỏ bộ khỏi thư viện trong khi đang có một phiên học M03 đang `IN_PROGRESS` của bộ đó, phiên học hiện tại VẪN ĐƯỢC TIẾP TỤC và chốt bình thường. Tuy nhiên, hệ thống sẽ ngăn chặn khởi tạo các phiên học mới từ bộ đó.
- **Xác nhận Tác động (`User Confirmation Invariant`)**: Client BẮT BUỘC hiển thị hộp thoại xác nhận báo rõ tác động trước khi gửi API bỏ bộ khỏi thư viện.

## 2. Quy trình Xử lý Bỏ Bộ khỏi Thư viện (Remove Set Flow)

```mermaid
graph TD
    User[Learner] -->|DELETE /api/v1/library/sets/{setId}| API[Library API]
    API --> CheckLink{UserLibrarySet Exists?}
    CheckLink -->|No| Err404[HTTP 404 NOT_IN_LIBRARY]
    CheckLink -->|Yes| RemoveLink[Delete UserLibrarySet Record]
    RemoveLink --> KeepM04[Keep M04 Progress Intact]
    KeepM04 --> PubEvent[Publish UserLibrarySetRemoved Event]
    PubEvent --> Res200[HTTP 200 OK Success]
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `LR-G01`: 100% thao tác bỏ bộ khỏi thư viện không làm giảm hay mất bản ghi `UserSenseProgress` nào trong DB M04.
- `LR-G02`: Phiên học M03 đang dở không bị hủy bỏ hay đẩy văng ra ngoài khi bấm bỏ bộ khỏi thư viện.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LR40-01` | Người dùng đã thuộc 10 từ trong bộ A1 bấm "Xóa khỏi thư viện" | Xóa liên kết `UserLibrarySet`, 10 bản ghi SRS M04 vẫn còn nguyên. |
| `LR40-02` | Thêm lại bộ A1 vào thư viện sau khi đã xóa | 10 từ vựng hiển thị đúng mốc tiến độ đã thuộc trước đó, không bắt đầu lại từ 0. |
| `LR40-03` | Thử xóa bộ từ vựng chưa từng thêm vào thư viện | API trả về lỗi HTTP 404 `SET_NOT_FOUND_IN_LIBRARY`. |
| `LR40-04` | Kiểm thử hoàn tất luồng M02-LIBRARY-REMOVE-SET-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-LR-F01` | Thêm soft-delete `IsArchivedByUser` cho `UserLibrarySets` | Đảm bảo dễ dàng khôi phục và đối soát lịch sử | M02-T039 |

## 5. Tự kiểm M02-T040
- Đã hoàn thành đặc tả `M02-LIBRARY-REMOVE-SET-1.0`.
- Chốt nguyên tắc bảo lưu 100% tiến độ SRS M04 và giữ nguyên phiên học M03 đang chạy.
- Ghi nhận 2 Regression Gates (`LR-G01`–`LR-G02`) và 4 Test Cases (`LR40-01`–`LR40-04`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa bỏ bộ khỏi thư viện M02-T040 | WSA-7K2 |
