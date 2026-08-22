# Chuẩn hóa yêu cầu trợ năng cho câu hỏi M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-QUESTION-ACCESSIBILITY-1.0` |
| Task | M03-T023 |
| Đầu vào | M03-QUESTION-DATA-TYPES-1.0 (M03-T019), M03-ASSET-DEGRADATION-FALLBACK-1.0 (M03-T022), WCAG 2.1 AA Standards |
| Phạm vi | Tiêu chuẩn trợ năng (Accessibility - Accessibility Standards), Alt-text cho hình ảnh, nhãn đọc Screen Reader (Aria-Label) và hỗ trợ điều khiển bàn phím |
| Tự kiểm | B-G01, A-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định các chuẩn trợ năng (`Accessibility Standards`) bắt buộc cho tất cả các giao diện và DTO câu hỏi trong M03.

- **Văn bản Thay thế Bắt buộc cho Phương tiện (`Mandatory Alt-Text Invariant`)**:
  - 100% tài sản hình ảnh và âm thanh trong DTO câu hỏi BẮT BUỘC cung cấp trường `AltText` hoặc `AriaLabel`.
  - Tuyệt đối CẤM truyền tài sản phương tiện mà không có mô tả văn bản cho Screen Reader.
- **Không Dùng Một Kênh Duy nhất truyền Ý nghĩa (`Multi-Sensory Redundancy Invariant`)**:
  - Phản hồi Đúng/Sai BẮT BUỘC kết hợp cả 3 yếu tố: Màu sắc + Biểu tượng Icon + Âm thanh/Văn bản. Không dùng duy nhất màu đỏ/xanh để phân biệt đúng sai nhằm hỗ trợ người mù màu.

## 2. Cấu trúc DTO Trợ năng Câu hỏi (Accessible Question DTO)

```csharp
public class AccessibleQuestionPromptDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string AriaLabel { get; set; } // Nhãn đọc đầy đủ cho Screen Reader
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; } // Alt text bắt buộc khi có hình ảnh
    public string? AudioUrl { get; set; }
    public string? AudioTransmittedText { get; set; } // Văn bản đọc thay thế audio
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `QA-G01`: 100% DTO câu hỏi có hình ảnh/âm thanh chứa trường `AltText`/`AriaLabel` không rỗng.
- `QA-G02`: Tất cả các nút bấm tương tác (Option Button, Submit, Hint) có thuộc tính `AriaLabel` hỗ trợ đọc màn hình.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `QA23-01` | Khởi tạo câu hỏi có hình ảnh minh họa | JSON DTO chứa `ImageAltText = "Hình ảnh minh họa từ vựng: Con mèo"`. |
| `QA23-02` | Trả lời câu hỏi sai đối với người dùng sử dụng Screen Reader | Trình đọc màn hình phát âm đúng chuỗi nhãn: `Lỗi: Đáp án sai. Đáp án đúng là...`. |
| `QA23-03` | Kiểm thử hoàn tất luồng M03-QUESTION-ACCESSIBILITY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-QA-F01` | Cần tự động kiểm tra `AltText` trong pipeline biên tập nội dung M02 | Tránh lọt câu hỏi thiếu trợ năng lên production | M02-T009 |

## 5. Tự kiểm M03-T023
- Đã hoàn thành đặc tả `M03-QUESTION-ACCESSIBILITY-1.0`.
- Chốt nguyên tắc Alt-Text bắt buộc và đa kênh truyền tín hiệu (WCAG 2.1 AA).
- Ghi nhận 2 Regression Gates (`QA-G01`–`QA-G02`) và 3 Test Cases (`QA23-01`–`QA23-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả chuẩn hóa yêu cầu trợ năng cho câu hỏi M03-T023 | WSA-7K2 |
