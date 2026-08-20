# Thiết kế phát hiện nội dung trùng M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-DUPLICATE-DETECTION-1.0` |
| Task | M02-T004 |
| Đầu vào | M02-MULTI-SENSE-1.0, M02-WORD-VARIANTS-1.0 |
| Phạm vi | Động cơ phát hiện trùng lặp mục từ/bộ từ 4 cấp độ, giao thức lọc trùng lặp khi nạp học liệu và quy trình hợp nhất từ trùng |
| Tự kiểm | A-G03 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này xác lập quy trình và thuật toán tự động phát hiện nội dung trùng lặp (Duplicate Detection Engine) áp dụng đối với Mục từ vựng (`Vocabulary`), Nét nghĩa (`VocabularySense`) và Bộ từ vựng (`VocabularySet`) trong M02.

- **Chống Trùng lặp Từ điển Master (`Zero Duplicate Master Headwords`)**: Hệ thống TUYỆT ĐỐI KHÔNG tạo 2 mục từ Master phân biệt nếu có cùng `WordCanonical`. Mọi yêu cầu nạp từ trùng mặt chữ gốc phải được ánh xạ về `VocabularyId` đã tồn tại.
- **Phân định 4 Cấp độ Trùng lặp (`4-Level Duplicate Taxonomy`)**:
  - *Cấp 1 (Exact Canonical)*: Trùng tuyệt đối `WordCanonical` $\to$ Tự động gộp vào `VocabularyId` hiện có.
  - *Cấp 2 (Phonetic / Variant)*: Trùng dạng biến thể chính tả US/UK hoặc hình thái $\to$ Tự động liên kết dưới dạng `VocabularyVariant`.
  - *Cấp 3 (Semantic Sense)*: Độ tương đồng chuỗi định nghĩa $\ge 85\%$ (Levenshtein / Cosine similarity) $\to$ Đánh dấu cảnh báo cho biên tập viên rà soát.
  - *Cấp 4 (Vocabulary Set Similarity)*: Độ tương đồng tập hợp mục từ trong Bộ từ $\ge 80\%$ (Chỉ số Jaccard Similarity) $\to$ Cảnh báo trùng bộ từ công khai.
- **Quy trình Hợp nhất Mục từ An toàn (`Safe Headword Merge Protocol`)**: Khi biên tập viên thực thi lệnh hợp nhất 2 mục từ trùng lặp (`Merge Headwords`), toàn bộ liên kết bộ từ (`SetVocabulary`), lịch sử phiên học (`SessionVocabulary`) và tiến độ người dùng (`UserVocabularyProgress`) được chuyển dịch an toàn sang mục từ đích trước khi lưu trữ mục từ bị gộp.

## 2. Phân loại 4 Cấp độ Trùng lặp và Thuật toán Xử lý

| Cấp độ trùng lặp | Phương pháp phát hiện | Ngưỡng đánh giá (`Threshold`) | Hành vi xử lý tự động |
|---|---|---|---|
| **Cấp 1: Exact Canonical** | So sánh khớp chuỗi `WordCanonical` | Khớp $100\%$ | **Hard Deduplication**: Tự động liên kết `VocabularyId` hiện có, không tạo entity mới. |
| **Cấp 2: Variant Match** | So sánh bảng `VocabularyVariant` hoặc bộ từ điển US/UK | Khớp $100\%$ chuỗi biến thể | **Variant Binding**: Tạo liên kết `VocabularyVariant`, giữ mục từ Master chính. |
| **Cấp 3: Semantic Sense** | Thuật toán Levenshtein Distance & Cosine Similarity trên `DefinitionVi` | Độ tương đồng $\ge 85\%$ | **Editor Warning**: Cảnh báo *"Nét nghĩa này tương tự Sense ID #X"* để biên tập viên gộp nghĩa. |
| **Cấp 4: Set Similarity** | Chỉ số Jaccard Similarity $J(A,B) = \frac{\|A \cap B\|}{\|A \cup B\|}$ trên danh sách `VocabularyId` | $J(A,B) \ge 80\%$ | **Set Warning**: Cảnh báo người dùng *"Bộ từ này trùng $80\%$ với Bộ từ công khai #Y"*. |

## 3. Giao thức Lọc Trùng lạp khi Nạp Học liệu (Ingestion Guard Pipeline)

```
[Incoming Content Stream (AI / Import / Custom)]
                      |
                      v
       (ToCanonical String Normalization)
                      |
                      v
       [Check Level 1: Exact WordCanonical] ---> (Found) ---> [Reuse Existing VocabularyId]
                      | (Not Found)
                      v
       [Check Level 2: Variant Text Lookup] ---> (Found) ---> [Add as VocabularyVariant]
                      | (Not Found)
                      v
       [Check Level 3: Definition Similarity] -> (>= 85%) --> [Flag Editor Warning]
                      | (< 85%)
                      v
       [Create New Master Vocabulary & Sense]
```

## 4. Giao thức Hợp nhất Mục từ An toàn (Safe Headword Merge Protocol)

Khi 2 mục từ Master (vd: `VocabularyId_A` và `VocabularyId_B`) vô tình bị tạo trùng lặp trong quá khứ, Biên tập viên gọi lệnh `MergeHeadwords(sourceId: A, targetId: B)`:

```
[Start Merge Operation (Source: A -> Target: B)]
                       |
                       v
       +-----------------------------------------------+
       | Step 1: Open Local Transaction                |
       +-----------------------------------------------+
                       |
                       v
       +-----------------------------------------------+
       | Step 2: Migrate SetVocabulary Links           |
       | (Reassign SetVocabulary records from A to B)  |
       +-----------------------------------------------+
                       |
                       v
       +-----------------------------------------------+
       | Step 3: Migrate User Progress & Sessions      |
       | (Merge UserVocabularyProgress from A to B)    |
       +-----------------------------------------------+
                       |
                       v
       +-----------------------------------------------+
       | Step 4: Add Source A Word as Variant for B    |
       +-----------------------------------------------+
                       |
                       v
       +-----------------------------------------------+
       | Step 5: Update Source A Status to 'Archived'  |
       +-----------------------------------------------+
                       |
                       v
       +-----------------------------------------------+
       | Step 6: Commit Transaction & Audit Log M11    |
       +-----------------------------------------------+
```

## 5. Cảnh báo Trùng lặp Bộ từ (Vocabulary Set Similarity Guard)

- Khi người học hoặc biên tập viên tạo một Bộ từ vựng mới $S_{new}$:
  1. Động cơ tính chỉ số Jaccard $J(S_{new}, S_{public})$ đối với tất cả các Bộ từ Hệ thống $S_{public}$ công khai đang có hiệu lực.
  2. Nếu $J(S_{new}, S_{public}) \ge 0.80$:
     - Trả về thông báo cảnh báo: *"Bộ từ bạn đang tạo có 8/10 từ trùng với Bộ từ công khai 'Từ vựng Tiếng Anh Giao tiếp B1'. Bạn có muốn bookmark Bộ từ công khai thay vì tạo mới không?"*
  3. Giúp giảm thiểu rác dữ liệu và tiết kiệm dung lượng lưu trữ hệ thống.

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DD-G01` | Cấm tạo mục từ Master trùng `WordCanonical` đã tồn tại trong CSDL. |
| `DD-G02` | Động cơ lọc trùng lặp Cấp 1 (Exact Canonical) chạy tự động trong mọi pipeline nạp dữ liệu. |
| `DD-G03` | Biến thể từ vựng Cấp 2 được liên kết tự động vào mục từ Master thay vì tạo từ mới. |
| `DD-G04` | Độ tương đồng định nghĩa $\ge 85\%$ tự động đưa cảnh báo Cấp 3 cho biên tập viên rà soát. |
| `DD-G05` | Chỉ số Jaccard Similarity $\ge 80\%$ tự động phát cảnh báo trùng bộ từ Cấp 4. |
| `DD-G06` | Giao thức Hợp nhất Mục từ (`Merge Protocol`) chuyển dịch an toàn toàn bộ tiến độ người học M04. |
| `DD-G07` | Mục từ nguồn sau khi gộp được chuyển trạng thái `archived`, cấm xóa cứng (Physical Delete). |
| `DD-G08` | Mọi thao tác Hợp nhất Mục từ ghi vết bản ghi Audit bất biến trong M11. |
| `DD-G09` | Phân quyền thực thi lệnh gộp từ tuân thủ nghiêm ngặt ma trận vai trò M11. |
| `DD-G10` | 100% các test case tự kiểm DD04-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DD04-01` | Nạp từ `"  vocabulary  "` khi từ `"vocabulary"` đã có trong Master DB | Ánh xạ về `VocabularyId` hiện có, không tạo entity mới |
| `DD04-02` | Nạp từ dạng biến thể chính tả `"colour"` khi từ `"color"` đã có | Tạo `VocabularyVariant` loại US/UK trỏ về `color` |
| `DD04-03` | Nạp nét nghĩa mới có độ tương đồng định nghĩa $90\%$ với nét nghĩa cũ | Trả về cảnh báo Cấp 3 `SEMANTIC_DUPLICATE_WARNING` |
| `DD04-04` | Tạo bộ từ mới có $9$ trên $10$ từ trùng bộ từ công khai hiện có | Trả về cảnh báo Cấp 4 `SET_SIMILARITY_JACCARD_HIGH` |
| `DD04-05` | Biên tập viên gọi lệnh Hợp nhất `VocabularyId_10` vào `VocabularyId_20` | Toàn bộ tiến độ M04 được chuyển sang ID 20 an toàn |
| `DD04-06` | Kiểm tra trạng thái `VocabularyId_10` sau khi gộp | Chuyển trạng thái `archived`, không bị xóa cứng |
| `DD04-07` | Thử gộp 2 mục từ không tồn tại | System reject với lỗi `VOCABULARY_NOT_FOUND` |
| `DD04-08` | Thử gộp 1 mục từ vào chính nó | System reject với lỗi `CANNOT_MERGE_SAME_VOCABULARY` |
| `DD04-09` | AI nạp 20 từ vựng gợi ý có 5 từ đã có trong Master DB | 5 từ trùng được reuse, 15 từ mới được tạo draft |
| `DD04-10` | Nạp từ có độ tương đồng định nghĩa $60\%$ ($< 85\%$) | Tạo nét nghĩa mới an toàn không phát cảnh báo |
| `DD04-11` | Tạo bộ từ có tỷ lệ trùng $50\%$ ($< 80\%$) với bộ từ công khai | Tạo bộ từ mới thành công không phát cảnh báo |
| `DD04-12` | Chạy job quét trùng lặp toàn bộ từ điển Master định kỳ | Trả về danh sách các cặp từ nghi ngờ trùng lặp |
| `DD04-13` | User không phải `R03 Content Admin` thực hiện lệnh gộp từ | Deny 403 Forbidden |
| `DD04-14` | Lệnh gộp từ bị ngắt kết nối DB giữa chừng | Local transaction rollback toàn bộ, dữ liệu không bị hỏng |
| `DD04-15` | Tra cứu lịch sử hợp đơn của một mục từ bị gộp | Trả về log audit M11 trỏ tới `targetVocabularyId` |
| `DD04-16` | Thao tác nhập từ có ký tự unicode biến thể FormD | Động cơ chuyển thành FormC và phát hiện trùng Cấp 1 |
| `DD04-17` | Tải đồng thời 50 request quét trùng lặp khi nạp bộ từ | Response p95 $< 100\text{ms}$, không tranh chấp CSDL |
| `DD04-18` | Khôi phục một mục từ đã bị gộp nhầm (`Unmerge Operation`) | Khôi phục liên kết ban đầu từ bản ghi audit |
| `DD04-19` | Phân tích tham chiếu trước khi thực thi gộp từ | Quét đủ các active lesson/battle đang trỏ từ cũ (T020) |
| `DD04-20` | Kiểm thử hoàn tất luồng phát hiện nội dung trùng M02-DUPLICATE-DETECTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-DD-I01` | Ingest service trong `VocabularySetService.cs` nạp từ chưa kiểm tra `WordCanonical` trùng | Có nguy cơ tạo trùng lặp mục từ trong Master DB | M02-T049 (Source task) |
| `M02-DD-I02` | Chưa có Động cơ Lọc Trùng lặp 4 Cấp độ (`Duplicate Detection Engine`) | Thiếu thuật toán Levenshtein và Jaccard similarity | M02-T049 |
| `M02-DD-I03` | Chưa có API hay Service thực thi lệnh Hợp nhất Mục từ (`Merge Headwords`) | Không thể dọn dẹp các từ trùng lặp đã tạo trong quá khứ | M02-T049 |
| `M02-DD-I04` | Thiếu cảnh báo trùng bộ từ Cấp 4 khi người dùng tạo custom set | Người dùng tạo nhiều bộ từ cá nhân trùng lặp nội dung | M02-T049 |
| `M02-DD-I05` | Thiếu cơ chế ghi log audit M11 cho các thao tác hợp nhất từ vựng | Khó khăn khi truy vết lịch sử gộp từ vựng | M02-T049 |

- `M02-DD-F01`: Triển khai `DuplicateDetectionEngine` với 4 cấp độ lọc trùng (tiếp nhận: M02-T049).
- `M02-DD-F02`: Xây dựng `HeadwordMergeService` thực thi giao thức gộp từ an toàn (tiếp nhận: M02-T049).
- `M02-DD-F03`: Tích hợp Jaccard Similarity Guard vào API tạo bộ từ vựng (tiếp nhận: M02-T049).
- `M02-DD-F04`: Thiết lập bộ kiểm thử tự động DD-G01–G10 và DD04-01–20 (tiếp nhận: M02 tasks).
- `M02-DD-F05`: Thu thập bằng chứng runtime cho luồng phát hiện nội dung trùng M02 (tiếp nhận: M02 tasks; A-G03).

## 8. Tự kiểm M02-T004

- Đã thiết kế hoàn chỉnh `M02-DUPLICATE-DETECTION-1.0` với Động cơ Lọc Trùng 4 Cấp độ (Exact Canonical, Variant, Semantic, Set Similarity).
- Đã chốt Giao thức Hợp nhất Mục từ An toàn 6 bước (`Safe Headword Merge Protocol`).
- Đã xây dựng thuật toán phát hiện trùng bộ từ bằng chỉ số Jaccard Similarity $\ge 80\%$.
- Đã lồng ghép cơ chế bảo vệ tiến độ học tập M04 và lưu vết audit log M11.
- Đã xác lập 10 Regression Gates (`DD-G01`–`DD-G10`) và 20 Test Cases tự kiểm (`DD04-01`–`DD04-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ code `VocabularySetService.cs` và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế phát hiện nội dung trùng M02-T004 | WSA-7K2 |
