# Thiết kế sắp xếp thành phần bộ M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-ITEM-ORDERING-1.0` |
| Task | M02-T021 |
| Đầu vào | M02-SET-ITEM-MUTATION-1.0 (D-079), M11-CONCURRENT-EDITING-1.0 (D-053), M02-LESSON-CONTENT-1.0 (D-064) |
| Phạm vi | Thuật toán và giao thức sắp xếp thứ tự hiển thị của các mục từ vựng trong Bộ từ, quy tắc tự động đánh lại chỉ số `DisplayOrder` và xử lý xung đột đồng thời |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập Giao thức và Thuật toán Sắp xếp Thứ tự Thành phần Bộ từ vựng (`Set Item Ordering & Reindexing Engine`) thuộc M02, quản lý vị trí xuất hiện của các từ vựng trong bài học M03 và trên giao diện người dùng.

- **Bảo toàn Thứ tự Liên tục Không Đứt đoạn (`Contiguous DisplayOrder Invariant`)**: Thuộc tính `DisplayOrder` trên thực thể liên kết `SetVocabulary` bắt buộc nhận giá trị số nguyên liên tục từ $1 \to N$ (với $N = ItemCount$). CẤM xuất hiện khoảng trống (`Gap`) hoặc giá trị trùng lặp thứ tự trong cùng 1 Bộ từ.
- **Ràng buộc Nhất quán trong Payload Bài học M03 (`Payload Ordering Consistency`)**: DTO bài học `LessonContentPayloadDto` cung cấp cho Module Học M03 bắt buộc trả về danh sách từ vựng được sắp xếp chính xác 100% theo thứ tự `DisplayOrder`.
- **4 Chế độ Sắp xếp Tiêu chuẩn (`4-Ordering Modes Matrix`)**: Hỗ trợ 4 chế độ: Soạn thảo thủ công (`Manual`), Theo bảng chữ cái (`Alphabetical`), Theo độ khó tăng/giảm (`Difficulty`), Theo cấp độ CEFR (`CEFR`).
- **Xử lý Sắp xếp Đồng thời bằng Optimistic Concurrency Control (`OCC`)**: Thao tác thay đổi thứ tự hàng loạt được bảo vệ bởi `expectedVersionDigest` (M11-T021 / D-053). Nếu 2 biên tập viên cùng kéo thả sắp xếp một bộ từ, hệ thống phát hiện xung đột 409 Conflict và yêu cầu Rebase.

## 2. Các Chế độ Sắp xếp Bộ từ vựng (Ordering Modes Matrix)

| Mã Mode | Tên Chế độ | Thuật toán Sắp xếp (`Sorting Key`) | Hành vi khi thêm từ mới | Quyền thực thi |
|---|---|---|---|---|
| `Manual` | Thủ công (Kéo thả) | Theo thứ tự mảng ID do người dùng kéo thả | Thêm từ mới vào cuối danh sách ($DisplayOrder = N + 1$) | Creator / ContentAdmin |
| `Alphabetical` | Bảng chữ cái | Sắp xếp A-Z theo `WordCanonical` | Tự động chèn từ mới vào đúng vị trí A-Z, re-index | Creator / ContentAdmin |
| `Difficulty` | Độ khó thực tế | Sắp xếp tăng dần theo `ItemDifficultyScore` | Tự động chèn từ mới theo $D_{item}$, re-index | Creator / ContentAdmin |
| `CEFR` | Trình độ khung Châu Âu | Sắp xếp tăng dần theo `CefrLevel` (A1 $\to$ C2) | Tự động xếp từ mới vào nhóm cấp độ tương ứng | Creator / ContentAdmin |

## 3. Giao thức Re-indexing Thứ tự (Auto Re-indexing Protocol)

Khi có thao tác Thêm từ mới, Bỏ từ hoặc Sắp xếp lại thứ tự (Drag & Drop):

```csharp
public async Task ReindexSetItemsAsync(int setId, List<int> orderedVocabularyIds, string expectedVersionDigest)
{
    var set = await _db.VocabularySets
        .Include(s => s.SetVocabularies)
        .FirstOrDefaultAsync(s => s.VocabularySetId == setId);

    if (set == null) throw new InvalidOperationException("SET_NOT_FOUND");

    // 1. Kiểm tra OCC Version Digest (M11-T021)
    if (set.VersionDigest != expectedVersionDigest)
    {
        throw new OptimisticConcurrencyException("CONCURRENT_SET_ORDERING_CONFLICT", "Bộ từ đã bị thay đổi thứ tự bởi người dùng khác.");
    }

    // 2. Cập nhật DisplayOrder số nguyên liên tục 1..N
    for (int i = 0; i < orderedVocabularyIds.Count; i++)
    {
        var vocabId = orderedVocabularyIds[i];
        var item = set.SetVocabularies.FirstOrDefault(sv => sv.VocabularyId == vocabId);
        if (item != null)
        {
            item.DisplayOrder = i + 1;
        }
    }

    // 3. Cập nhật versionDigest mới cho bộ từ
    set.VersionDigest = GenerateNewVersionDigest(set);
    
    await _db.SaveChangesAsync();

    // 4. Xóa cache Redis Payload M03
    await _cacheService.RemoveAsync($"lesson_payload:{setId}");
}
```

## 4. Quy trình Xử lý re-index khi Bỏ mục từ khỏi bộ

Khi một mục từ có `DisplayOrder = K` bị gỡ khỏi bộ từ có $N$ phần tử:

1. Xóa bản ghi `SetVocabulary` của mục từ bị gỡ.
2. Với tất cả các mục từ còn lại có `DisplayOrder > K`, giảm thứ tự `DisplayOrder = DisplayOrder - 1`.
3. Bảo toàn dải thứ tự liên tục từ $1 \to N-1$.

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `SO-G01` | Dải chỉ số `DisplayOrder` trong 1 bộ từ bắt buộc là dãy số nguyên liên tục $1 \to N$. |
| `SO-G02` | Cấm tồn tại 2 mục từ có cùng `DisplayOrder` trong cùng 1 Bộ từ vựng. |
| `SO-G03` | Payload DTO `LessonContentPayloadDto` cung cấp cho M03 trả về danh sách từ vựng được xếp 100% theo `DisplayOrder`. |
| `SO-G04` | Thao tác sắp xếp lại thứ tự hàng loạt được bảo vệ bởi OCC `versionDigest` (M11-T021 / D-053). |
| `SO-G05` | Thêm từ mới ở chế độ `Manual` tự động gán `DisplayOrder = ItemCount + 1`. |
| `SO-G06` | Thao tác gỡ từ tự động re-index lại các mục từ phía sau để bảo toàn dải số liên tục. |
| `SO-G07` | Thao tác sắp xếp lại thứ tự tự động xóa cache Redis `lesson_payload:{setId}` trong vòng $\le 10$ giây. |
| `SO-G08` | Phân quyền thay đổi thứ tự bộ từ tuân thủ ma trận vai trò M02-T016 (`CreatorId` hoặc `ContentAdmin`). |
| `SO-G09` | SLA xử lý re-index thứ tự cho bộ từ 50 mục từ $< 30\text{ms}$. |
| `SO-G10` | 100% các test case tự kiểm SO21-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `SO21-01` | Sắp xếp lại danh sách 15 từ vựng trong Bộ từ 108 bằng Kéo thả | `DisplayOrder` cập nhật chính xác từ 1 đến 15 |
| `SO21-02` | Thêm từ vựng mới vào cuối Bộ từ ở chế độ `Manual` | Từ mới nhận `DisplayOrder = 16` |
| `SO21-03` | Gỡ từ vựng ở vị trí `DisplayOrder = 5` trong Bộ từ 15 từ | Từ vị trí 6 đến 15 tự động giảm xuống 5 đến 14 |
| `SO21-04` | Chuyển chế độ sắp xếp sang `Alphabetical` (A-Z) | Tự động sắp xếp lại 100% danh sách từ theo `WordCanonical` |
| `SO21-05` | Chuyển chế độ sắp xếp sang `Difficulty` | Tự động sắp xếp lại danh sách từ theo `ItemDifficultyScore` |
| `SO21-06` | Sắp xếp đồng thời: Biên tập viên A và B cùng kéo thả Bộ từ 108 | B nhận lỗi 409 Conflict `CONCURRENT_SET_ORDERING_CONFLICT` |
| `SO21-07` | Biên tập viên B thực hiện Rebase và sắp xếp lại | Cập nhật thứ tự thành công với `versionDigest` mới |
| `SO21-08` | Nạp DTO bài học M03 sau khi sắp xếp lại thứ tự | Danh sách từ vựng trong DTO khớp 100% với `DisplayOrder` |
| `SO21-09` | Kiểm tra cache Redis `lesson_payload:108` sau khi re-index | Cache cũ bị xóa, payload mới được pre-generate |
| `SO21-10` | Thử gửi mảng danh sách ID thiếu 1 mục từ khi re-index | System reject với lỗi `INVALID_ORDERING_ARRAY_LENGTH` |
| `SO21-11` | Thử gửi mảng danh sách ID chứa 1 mục từ không thuộc bộ từ | System reject với lỗi `ITEM_NOT_FOUND_IN_SET` |
| `SO21-12` | Người học B thử gửi request sắp xếp Bộ từ Cá nhân của A | System reject 403 Forbidden (M02-T016) |
| `SO21-13` | Tải đồng thời 50 request re-index thứ tự trên 50 bộ từ | Response latency p95 $< 35\text{ms}$ |
| `SO21-14` | User chưa đăng nhập thử gọi API sắp xếp thứ tự | Deny 401 Unauthorized |
| `SO21-15` | Xem vết Audit Log M11 sau khi sắp xếp lại bộ từ | Ghi nhận Audit Event `ACT-M11-04` với chi tiết thứ tự |
| `SO21-16` | Sắp xếp bộ từ chứa 50 mục từ tối đa | Re-index thành công 50 phần tử trong thời gian $< 25\text{ms}$ |
| `SO21-17` | Phân tích tham chiếu trước khi đổi thứ tự từ trong bộ | Quét các active session M03 đang tham chiếu thứ tự (T020) |
| `SO21-18` | Thao tác re-index bị gián đoạn giữa chừng do lỗi DB | Rollback transaction toàn bộ, giữ nguyên thứ tự cũ |
| `SO21-19` | Chuyển chế độ sắp xếp sang `CEFR` (A1 $\to$ C2) | Danh sách từ vựng tự động gom theo nhóm trình độ |
| `SO21-20` | Kiểm thử hoàn tất luồng thiết kế sắp xếp thành phần bộ M02-SET-ITEM-ORDERING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-SO-I01` | Entity `SetVocabulary.cs` chưa có thuộc tính `DisplayOrder` | Dữ liệu từ vựng trong bộ bị xáo trộn ngẫu nhiên khi query | M02-T049 (Source task) |
| `M02-SO-I02` | API nạp bài học M03 hiện tại chưa `OrderBy(sv => sv.DisplayOrder)` | Phiên học M03 hiển thị thứ tự từ không đúng ý đồ thiết kế | M02-T049 |
| `M02-SO-I03` | Thiếu cơ chế OCC `versionDigest` bảo vệ khi sắp xếp đồng thời | Kéo thả đồng thời có thể gây ghi đè thứ tự xáo trộn | M02-T049; M11-T021 |
| `M02-SO-I04` | Thiếu thuật toán tự động re-index liên tục $1 \to N$ khi gỡ từ | Xuất hiện khoảng trống `DisplayOrder` gây lỗi sắp xếp | M02-T049 |
| `M02-SO-I05` | Chưa xóa Redis Cache `lesson_payload` sau khi thay đổi thứ tự | M03 vẫn lấy thứ tự cũ từ Redis cache | M02-T049 |

- `M02-SO-F01`: Thêm `DisplayOrder` vào `SetVocabulary.cs` và index CSDL (tiếp nhận: M02-T049).
- `M02-SO-F02`: Triển khai `ReindexSetItemsService` bảo vệ bằng OCC `versionDigest` (tiếp nhận: M02-T049; M11-T021).
- `M02-SO-F03`: Cập nhật `LessonContentBuilderService` sắp xếp theo `DisplayOrder` (tiếp nhận: M02-T049).
- `M02-SO-F04`: Thiết lập bộ kiểm thử tự động SO-G01–G10 và SO21-01–20 (tiếp nhận: M02 tasks).
- `M02-SO-F05`: Thu thập bằng chứng runtime cho luồng sắp xếp bộ từ M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T021

- Đã thiết kế hoàn chỉnh `M02-SET-ITEM-ORDERING-1.0` với Ma trận 4 Chế độ Sắp xếp Bộ từ vựng.
- Đã chốt Ràng buộc Bảo toàn Thứ tự Liên tục Không Đứt đoạn ($1 \to N$).
- Đã xây dựng `ReindexSetItemsProtocol` bảo vệ bằng OCC `versionDigest` (D-053).
- Đã lồng ghép xóa Redis Cache `lesson_payload` khi re-index và quy tắc tự động dồn chỉ số khi gỡ từ.
- Đã xác lập 10 Regression Gates (`SO-G01`–`SO-G10`) và 20 Test Cases tự kiểm (`SO21-01`–`SO21-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế sắp xếp thành phần bộ M02-T021 | WSA-7K2 |
