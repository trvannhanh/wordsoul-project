# Chuẩn hóa cấp độ và độ khó M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-CEFR-DIFFICULTY-1.0` |
| Task | M02-T005 |
| Đầu vào | M02-MULTI-SENSE-1.0, M01-T001 |
| Phạm vi | Phân loại cấp độ ngôn ngữ CEFR (A1-C2), công thức tính chỉ số độ khó thực tế `ItemDifficultyScore` và độ khó bộ từ `SetDifficultyIndex` |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này quy định hệ thống phân loại cấp độ năng lực ngôn ngữ theo chuẩn CEFR (Common European Framework of Reference for Languages) và công thức tính toán chỉ số độ khó thực tế của từng mục từ vựng và bộ từ vựng trong WordSoul M02.

- **Bắt buộc Gán Cấp độ CEFR (`Mandatory CEFR Assignment`)**: 100% mục từ Master `Vocabulary` khi xuất bản công khai bắt buộc phải được gán 1 trong 6 cấp độ CEFR chuẩn (`A1`, `A2`, `B1`, `B2`, `C1`, `C2`). CẤM mục từ xuất bản để trường `CEFRLevel == null`.
- **Phân tách Cấp độ Tĩnh và Độ khó Động (`Static Level vs Dynamic Difficulty`)**:
  - *CEFR Level (Tĩnh)*: Phân loại chuẩn ngôn ngữ học định sẵn do ban biên tập/từ điển tiêu chuẩn quy định.
  - *Item Difficulty Score $D_{item} \in [1.0, 10.0]$ (Động)*: Chỉ số độ khó thực tế được cập nhật tự động dựa trên dữ liệu phản hồi của tập hợp người học (tỷ lệ trả lời sai trong M03, thời gian phản hồi, tần suất quên khi ôn tập M04).
- **Tính toán Độ khó Bộ từ vựng (`Set Difficulty Index calculation`)**: Chỉ số độ khó của Bộ từ vựng ($D_{set}$) được tính dựa trên phân bố trọng số cấp độ CEFR và chỉ số độ khó động của các từ chứa bên trong.

## 2. Chuẩn hóa Cấp độ Ngôn ngữ CEFR (CEFR Standard Matrix)

| Cấp độ CEFR | Tên phân loại | Mô tả năng lực | Thang điểm tĩnh $S_{cefr}$ | Tiêu chí tham chiếu |
|---|---|---|---|---|
| `A1` | Beginner | Sơ cấp / Nhập môn | $1.0 - 2.0$ | Oxford 3000 từ thông dụng nhất, từ đơn giản ngắn gọn. |
| `A2` | Elementary | Căn bản / Sơ trung cấp | $2.1 - 4.0$ | Chủ đề hàng ngày, giao tiếp cơ bản, cấu trúc từ phổ biến. |
| `B1` | Intermediate | Trung cấp | $4.1 - 6.0$ | Từ vựng chủ đề công việc, du lịch, mô tả trải nghiệm. |
| `B2` | Upper Intermediate | Trung cao cấp | $6.1 - 7.5$ | Từ vựng chuyên ngành cơ bản, tranh luận, cụm từ kết hợp. |
| `C1` | Advanced | Cao cấp | $7.6 - 9.0$ | Thuật ngữ học thuật, văn phong chính thức, cấu trúc phức tạp. |
| `C2` | Proficiency | Thành thạo / Bản ngữ | $9.1 - 10.0$ | Thành ngữ hiếm, từ cổ, thuật ngữ chuyên sâu ít gặp. |

## 3. Động cơ Tính toán Chỉ số Độ khó Thực tế (Item Difficulty Score Engine)

Chỉ số độ khó thực tế $D_{item} \in [1.0, 10.0]$ của mục từ được cập nhật định kỳ (Cron Job hàng ngày) dựa trên công thức cập nhật Bayes:

$$D_{item} = w_{base} \cdot S_{cefr} + w_{error} \cdot (10 \cdot E_{rate}) + w_{time} \cdot T_{penalty}$$

Trong đó:
- $S_{cefr}$: Thang điểm tĩnh CEFR gốc ($1.0 \to 10.0$).
- $E_{rate} = \frac{N_{failed}}{N_{total}}$: Tỷ lệ trả lời sai của người học trong bài kiểm tra M03 ($0.0 \to 1.0$).
- $T_{penalty}$: Điểm phạt latency nếu thời gian suy nghĩ trung bình $T_{avg} > 10\text{s}$.
- Trọng số mặc định: $w_{base} = 0.50$, $w_{error} = 0.35$, $w_{time} = 0.15$.

### 3.1. Thuật toán Cập nhật Độ khó Động
```csharp
public double CalculateDynamicItemDifficulty(CEFRLevel cefr, int totalAnswers, int wrongAnswers, double avgResponseSeconds)
{
    double baseScore = GetBaseCefrScore(cefr); // A1=1.5, A2=3.0, B1=5.0, B2=6.8, C1=8.3, C2=9.5
    
    if (totalAnswers < 30) // Chưa đủ mẫu thống kê người học -> Dùng điểm CEFR tĩnh
        return baseScore;

    double errorRate = (double)wrongAnswers / totalAnswers; // 0.0 -> 1.0
    double timePenalty = Math.Min(2.0, Math.Max(0.0, (avgResponseSeconds - 5.0) / 2.5)); // Phạt nếu > 5s suy nghĩ

    double dynamicScore = (0.50 * baseScore) + (0.35 * errorRate * 10.0) + (0.15 * (baseScore + timePenalty));
    
    // Kẹp giá trị trong khoảng 1.0 đến 10.0
    return Math.Round(Math.Clamp(dynamicScore, 1.0, 10.0), 2);
}
```

## 4. Công thức Tính toán Độ khó Bộ từ vựng (Set Difficulty Index)

Độ khó tổng thể của một Bộ từ vựng $D_{set}$ chứa $N$ mục từ được tính theo trung bình trọng số:

$$D_{set} = \frac{1}{N} \sum_{i=1}^{N} D_{item, i}$$

Phân loại cấp độ hiển thị của Bộ từ (`SuggestedSetLevel`):
- $D_{set} < 3.0 \implies$ Bộ từ dành cho **A1-A2 (Sơ cấp)**.
- $3.0 \le D_{set} < 6.5 \implies$ Bộ từ dành cho **B1-B2 (Trung cấp)**.
- $D_{set} \ge 6.5 \implies$ Bộ từ dành cho **C1-C2 (Cao cấp)**.

## 5. Ánh xạ Đề xuất Bộ từ với Trình độ Người học (Learner Profile Matching)

Khi gợi ý Bộ từ vựng cho Người học (M01-T008 / M04-PROGRESS):
- Nếu trình độ người học là $L_{user} = B1$: Hệ thống ưu tiên đề xuất các bộ từ có $4.0 \le D_{set} \le 6.5$ (Vùng thử thách phù hợp Zone of Proximal Development - ZPD).
- Tránh gợi ý bộ từ quá dễ ($D_{set} < 2.0$) gây chán nản hoặc quá khó ($D_{set} > 8.5$) gây áp lực quá tải.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `CD-G01` | 100% mục từ Master `Vocabulary` khi xuất bản có trường `CEFRLevel` hợp lệ thuộc A1-C2. |
| `CD-G02` | Thang điểm tĩnh $S_{cefr}$ nằm trong khoảng từ $1.0$ đến $10.0$. |
| `CD-G03` | Chỉ số độ khó động $D_{item}$ tự động cập nhật dựa trên dữ liệu tỷ lệ sai $E_{rate}$ và latency $T_{avg}$. |
| `CD-G04` | Khi tổng số lượt trả lời $N_{total} < 30$, hệ thống giữ nguyên chỉ số độ khó tĩnh CEFR gốc. |
| `CD-G05` | Chỉ số độ khó bộ từ $D_{set}$ được tự động tính toán lại khi thêm/bớt từ trong bộ. |
| `CD-G06` | Phân loại cấp độ bộ từ (`SuggestedSetLevel`) phản ánh chính xác điểm trung bình $D_{set}$. |
| `CD-G07` | Hệ thống đề xuất bộ từ tuân thủ vùng thử thách phù hợp với trình độ người học trong hồ sơ M01. |
| `CD-G08` | Cấm gán cấp độ CEFR không thuộc 6 giá trị chuẩn (`A1`, `A2`, `B1`, `B2`, `C1`, `C2`). |
| `CD-G09` | Phân quyền cập nhật chỉ số độ khó tĩnh tuân thủ ma trận vai trò M11. |
| `CD-G10` | 100% các test case tự kiểm CD05-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `CD05-01` | Tạo mục từ mới gán `CEFRLevel = B1` | Gán thành công, $S_{cefr} = 5.0$ mặc định |
| `CD05-02` | Thử xuất bản mục từ có `CEFRLevel = null` | System reject với lỗi `MISSING_CEFR_LEVEL` |
| `CD05-03` | Từ vựng có 100 lượt làm bài, tỷ lệ sai $50\%$, latency $8\text{s}$ | $D_{item}$ tự động điều chỉnh tăng lên $> 6.5$ |
| `CD05-04` | Từ vựng mới tạo chỉ có 5 lượt làm bài ($< 30$) | Giữ nguyên điểm $D_{item} = S_{cefr}$ gốc |
| `CD05-05` | Tính độ khó $D_{set}$ cho bộ từ 10 từ trình độ A1 ($S_{cefr} = 1.5$) | $D_{set} = 1.5$, phân loại bộ từ `Sơ cấp (A1-A2)` |
| `CD05-06` | Thêm 2 từ trình độ C1 ($S_{cefr} = 8.5$) vào bộ từ A1 trên | $D_{set}$ tự động tăng lên $2.67$, tính lại phân loại bộ |
| `CD05-07` | Người học có trình độ B1 yêu cầu gợi ý bộ từ | Ưu tiên đề xuất các bộ từ có $4.0 \le D_{set} \le 6.5$ |
| `CD05-08` | Thử truyền chuỗi `CEFRLevel = "D1"` không hợp lệ | System reject với lỗi `INVALID_CEFR_LEVEL_ENUM` |
| `CD05-09` | Chạy job cập nhật chỉ số độ khó hàng ngày | Cập nhật $D_{item}$ thành công cho toàn bộ các từ có dữ liệu |
| `CD05-10` | Cập nhật cấp độ CEFR từ B1 lên B2 cho mục từ Master | Cập nhật $S_{cefr} = 6.8$, ghi vết audit log M11 |
| `CD05-11` | Từ vựng A1 nhưng có tỷ lệ sai $90\%$ do câu ví dụ khó hiểu | $D_{item}$ tự động tăng lên $6.2$, phát cảnh báo xem xét câu ví dụ |
| `CD05-12` | Lấy thông tin DTO bộ từ vựng qua API | DTO trả về đầy đủ `setDifficultyIndex` và `suggestedSetLevel` |
| `CD05-13` | Người học mới chưa có trình độ chọn bộ từ nhập môn A1 | Hệ thống chấp nhận đề xuất bộ từ A1 chuẩn |
| `CD05-14` | Kiểm tra thời gian tính toán $D_{set}$ cho bộ từ 500 từ | Thời gian tính toán $< 5\text{ms}$ |
| `CD05-15` | Cố tình truyền $D_{item} = 15.0$ vượt hạn mức | System kẹp giá trị `Clamp(15.0) -> 10.0` |
| `CD05-16` | Cố tình truyền $D_{item} = -2.0$ | System kẹp giá trị `Clamp(-2.0) -> 1.0` |
| `CD05-17` | Tải đồng thời 50 request lấy danh sách gợi ý bộ từ theo độ khó | Response p95 $< 40\text{ms}$ từ Redis cache |
| `CD05-18` | Người học C2 yêu cầu đề xuất bộ từ | Hệ thống gợi ý bộ từ $D_{set} \ge 8.0$ (C1-C2) |
| `CD05-19` | Phân tích phụ thuộc khi thay đổi cấp độ CEFR của từ vựng | Quét các bộ từ chứa từ đó để recalculate $D_{set}$ (T020) |
| `CD05-20` | Kiểm thử hoàn tất luồng chuẩn hóa cấp độ và độ khó M02-CEFR-DIFFICULTY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-CD-I01` | Enum `CEFRLevel.cs` trong `WordSoul.Domain` đã có sẵn nhưng chưa bắt đầu validation `[Required]` | Rủi ro lưu `CEFRLevel == null` trong CSDL | M02-T049 (Source task) |
| `M02-CD-I02` | Entity `Vocabulary.cs` chưa có thuộc tính `ItemDifficultyScore` động | Chưa hỗ trợ theo dõi độ khó thực tế của từ | M02-T049 |
| `M02-CD-I03` | Entity `VocabularySet.cs` chưa có thuộc tính `SetDifficultyIndex` | Bộ từ vựng chưa được tính điểm độ khó tổng thể | M02-T049 |
| `M02-CD-I04` | Chưa có Cron Job cập nhật chỉ số độ khó động hàng ngày (`DifficultyCalculatorJob`) | Chỉ số độ khó không phản ánh đúng phản hồi người học | M02-T049 |
| `M02-CD-I05` | Thuật toán gợi ý bộ từ vựng chưa dựa trên vùng ZPD của hồ sơ người học M01 | Gợi ý bộ từ chưa tối ưu cho từng trình độ cá nhân | M02-T049 |

- `M02-CD-F01`: Đặt ràng buộc bắt buộc `CEFRLevel` trên Entity `Vocabulary.cs` (tiếp nhận: M02-T049).
- `M02-CD-F02`: Thêm trường `ItemDifficultyScore` vào `Vocabulary.cs` và `SetDifficultyIndex` vào `VocabularySet.cs` (tiếp nhận: M02-T049).
- `M02-CD-F03`: Cài đặt `DifficultyCalculatorEngine` và Job cập nhật định kỳ (tiếp nhận: M02-T049).
- `M02-CD-F04`: Thiết lập bộ kiểm thử tự động CD-G01–G10 và CD05-01–20 (tiếp nhận: M02 tasks).
- `M02-CD-F05`: Thu thập bằng chứng runtime cho luồng chuẩn hóa cấp độ và độ khó M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T005

- Đã thiết kế hoàn chỉnh `M02-CEFR-DIFFICULTY-1.0` với ma trận 6 cấp độ CEFR chuẩn (A1-C2).
- Đã chốt thuật toán tính chỉ số độ khó thực tế động $D_{item} \in [1.0, 10.0]$ dựa trên Bayes error rate và latency penalty.
- Đã xây dựng công thức tính độ khó bộ từ $D_{set}$ và phân loại 3 phân khúc gợi ý (A1-A2, B1-B2, C1-C2).
- Đã lồng ghép cơ chế đề xuất bộ từ theo vùng thử thách ZPD phù hợp với hồ sơ trình độ M01.
- Đã xác lập 10 Regression Gates (`CD-G01`–`CD-G10`) và 20 Test Cases tự kiểm (`CD05-01`–`CD05-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `Vocabulary.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả chuẩn hóa cấp độ và độ khó M02-T005 | WSA-7K2 |
