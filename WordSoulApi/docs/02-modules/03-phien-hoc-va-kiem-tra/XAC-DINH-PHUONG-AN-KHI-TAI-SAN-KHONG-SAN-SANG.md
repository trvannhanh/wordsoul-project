# Xác định phương án khi tài sản không sẵn sàng M03

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M03-ASSET-DEGRADATION-FALLBACK-1.0` |
| Task | M03-T022 |
| Đầu vào | M02-ASSET-DEGRADATION-1.0 (M02-T013), M03-QUESTION-DATA-TYPES-1.0 (M03-T019), M12-CAPABILITY-SLO-HEALTH-DEFINITIONS-1.0 (M12-T045) |
| Phạm vi | Phương án suy giảm 3 tầng khi tài sản âm thanh/hình ảnh bị lỗi (S3/CDN outage), không chấm sai người học do lỗi media và chuyển dạng câu hỏi mượt mà |
| Tự kiểm | B-G01, A-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định phương án suy giảm mượt (`Graceful Fallback`) khi tài sản phương tiện (Audio MP3, Image WebP) bị lỗi hoặc CDN không sẵn sàng trong M03.

- **Cấm Chấm Sai do Lỗi Tài sản (`No Fault Penalty Invariant`)**:
  - Tuyệt đối CẤM chấm người học trả lời sai nếu lỗi phát sinh do tài sản audio không tải được (HTTP 404/500 CDN) hoặc file âm thanh bị hỏng.
- **Phương án Suy giảm 3 Tầng (`3-Tier Asset Degradation Fallback Invariant`)**:
  - *Tier 1 (Tải Audio CDN gốc)*: Phát file MP3 lưu trên CDN private/public M12.
  - *Tier 2 (Client Web Speech API Fallback)*: Nếu CDN timeout $\ge 500\text{ms}$ hoặc trả lỗi, client tự động dùng Web Speech API (TTS trình duyệt) để phát phát âm.
  - *Tier 3 (Chuyển dạng Text Fallback)*: Nếu Web Speech API không khả dụng, hệ thống tự động đổi câu hỏi nghe sang câu hỏi hiển thị phiên âm IPA + Text mà KHÔNG làm gián đoạn phiên học.

## 2. Ma trận Phương án Suy giảm Tài sản (Asset Fallback Matrix)

| Loại Tài sản | Trạng thái Lỗi | Phương án Xử lý | Tác động Kết quả Chấm |
|---|---|---|---|
| Audio MP3 | CDN Timeout $> 500\text{ms}$ | Chuyển sang Web Speech API (TTS client) | Giữ nguyên chấm bình thường |
| Audio MP3 | CDN 404 / Web Speech Fail | Hiển thị phiên âm IPA + Nút "Bỏ qua câu nghe" | Không phạt sai, cho phép skip |
| Image WebP | Image Load Error | Hiển thị SVG Icon mặc định theo loại từ (POS) | Giữ nguyên chấm bình thường |

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `ADF-G01`: 100% câu hỏi nghe bị lỗi audio CDN không bị chấm `INCORRECT` khi người học chọn bỏ qua câu bị lỗi.
- `ADF-G02`: Thời gian phát hiện lỗi CDN và chuyển sang Web Speech API $\le 500\text{ms}$.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `ADF22-01` | Giả lập CDN S3 sập (HTTP 500) khi người học làm câu nghe | App tự chuyển sang dùng Web Speech API phát âm thanh. |
| `ADF22-02` | File audio bị xóa trên server (404 Not Found) | App hiển thị phiên âm IPA và nút "Bỏ qua do lỗi âm thanh", không tính điểm phạt. |
| `ADF22-03` | Kiểm thử hoàn tất luồng M03-ASSET-DEGRADATION-FALLBACK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M03-ADF-F01` | Thêm thuộc tính `PhoneticIpa` vào `SnapshotVocabularyItemDto` | Đảm bảo có phiên âm chữ để fallback khi mất audio | M03-T007 |

## 5. Tự kiểm M03-T022
- Đã hoàn thành đặc tả `M03-ASSET-DEGRADATION-FALLBACK-1.0`.
- Chốt suy giảm 3 tầng (CDN -> Web Speech -> IPA Text) và nguyên tắc cấm phạt lỗi media.
- Ghi nhận 2 Regression Gates (`ADF-G01`–`ADF-G02`) và 3 Test Cases (`ADF22-01`–`ADF22-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả xác định phương án khi tài sản không sẵn sàng M03-T022 | WSA-7K2 |
