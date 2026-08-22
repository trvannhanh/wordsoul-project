# Đánh giá xuất bản tài liệu lên repository public

| Thuộc tính | Giá trị |
|---|---|
| Artifact ID | `PUBLIC-REPO-DOC-PUBLICATION-ASSESSMENT-1.0` |
| Phạm vi | 197 commit local chưa push so với `origin/feat/feature2` |
| Quyết định tạm thời | D-370 — `HOLD_PUBLIC_PUSH` |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Kết luận khuyến nghị

**Không push nguyên trạng 197 commit lên repository public.** Phương án khuyến nghị nếu code vẫn cần public là chuyển full tài liệu hệ thống sang repository private và chỉ xuất bản một tập tài liệu public bằng allowlist. Nếu không có nhu cầu giữ code public, chuyển toàn repository sang private là phương án đơn giản hơn.

Lợi thế hiện tại là các commit docs chưa lên public origin, nên có thể tách nhánh sạch mà chưa cần rewrite lịch sử public hay force-push.

## 2. Hiện trạng kiểm tra local

| Hạng mục | Kết quả |
|---|---|
| Remote | `origin` trỏ tới GitHub; repository được chủ dự án xác nhận là public |
| Nhánh | `feat/feature2` đi trước upstream 197 commit, không đi sau |
| Phạm vi khác biệt | 478 path: 470 dưới `WordSoulApi/docs`, 8 path workflow/context khác |
| Tên file rủi ro cao | Không thấy `.env`, private key, PFX/PEM, credential/appsettings production trong diff chưa push |
| Quét regex giá trị | Không xác nhận private key, GitHub/AWS/JWT/OpenAI credential thật; các hit ban đầu là tên/đoạn code giả dương |
| Scanner chuyên dụng | `gitleaks` và `trufflehog` chưa có trong môi trường; kết quả hiện tại không đủ cấp phép publish |
| File docs untracked | 3 file; một bản sao M02 giống byte và hai artifact M07/M10 không khớp task tracker |

## 3. Vì sao docs vẫn nhạy cảm dù không có secret

- Ma trận role/permission, luồng re-auth, abuse/rate-limit và finding bảo mật giúp đối tượng ngoài lập bản đồ kiểm soát.
- Secret inventory, provider topology, fail mode, SLO, budget và kill switch làm lộ cấu trúc vận hành.
- Incident/DR playbook, health/job/reconcile contract và khoảng trống runtime cho biết điểm yếu hoặc năng lực chưa tồn tại.
- Data map, retention, export/delete, consent/age policy làm tăng rủi ro riêng tư và pháp lý nếu thông tin chưa được rà soát.
- Roadmap, task tracker và quyết định sản phẩm tiết lộ chiến lược chưa phát hành.

## 4. Phân loại xuất bản

| Lớp | Ví dụ | Đích |
|---|---|---|
| `PUBLIC` | README sản phẩm, kiến trúc mức cao, hướng dẫn đóng góp, API đã phát hành, ADR đã làm sạch | Public repo |
| `INTERNAL` | TASKS, roadmap, schema chi tiết, dependency map, metric/SLO/cost target, backlog chưa phát hành | Private docs repo |
| `RESTRICTED` | Threat/finding, admin/security matrix, secret inventory metadata, data egress, incident/DR/runbook | Private repo với quyền tối thiểu |
| `NEVER-GIT` | Secret/token/key thật, PII, raw payload, dump/log/evidence nhạy cảm | Secret/evidence system phù hợp; không commit |

Phân loại dùng allowlist: tài liệu không được gắn `PUBLIC` mặc định không đi ra public origin.

## 5. Các phương án

| Phương án | Khi phù hợp | Đánh giá |
|---|---|---|
| A — Chuyển toàn repository thành private | Không cần mã nguồn public | An toàn và ít vận hành nhất; khuyến nghị nếu chấp nhận được |
| B — Code public, full docs ở repository private | Cần open-source/showcase code | **Khuyến nghị chính**; tách quyền và lịch sử rõ, public chỉ nhận bản sanitized |
| C — Giữ full docs trong repo public sau redaction | Chỉ khi mọi nội dung thực sự được phép công khai | Không khuyến nghị; chi phí audit cao và dễ lộ chi tiết qua lịch sử |
| D — Mã hóa docs trong public Git | Khi có hạ tầng quản lý khóa nghiêm ngặt | Không ưu tiên; metadata/lịch sử còn lộ và vận hành khóa phức tạp |

## 6. Quy trình đề xuất cho phương án B

1. Giữ `HOLD_PUBLIC_PUSH`; không dùng `git add .`, `git push` hoặc force-push.
2. Tạo repository private riêng, giới hạn thành viên và bật branch protection/audit phù hợp.
3. Chạy scanner lịch sử chuyên dụng trên toàn bộ 197 commit và working tree. Nếu thấy credential thật, xoay/thu hồi trước khi sửa Git.
4. Push/mirror full nhánh tài liệu sang private remote sau khi chủ repository duyệt quyền truy cập.
5. Tạo nhánh public sạch từ `origin/feat/feature2`, không từ HEAD hiện tại; đưa sang chỉ các thay đổi code/context được phép và một snapshot docs `PUBLIC` đã sanitize.
6. Review diff public theo allowlist, kiểm tra link, generated output, metadata, author email và nội dung xóa vẫn còn trong history hay không.
7. Push nhánh public bình thường. Chỉ rewrite/force-push nếu nội dung nhạy cảm đã từng lên remote và có kế hoạch phối hợp rõ.
8. Thiết lập CI kiểm tra secret và policy path để ngăn `INTERNAL/RESTRICTED` quay lại public repo.

`.gitignore`, Git LFS hoặc xóa file ở commit mới không làm dữ liệu biến mất khỏi lịch sử đã push.

## 7. Điều kiện bỏ giữ push

- Chủ dự án chọn A hoặc B và xác nhận remote đích/quyền truy cập.
- Scanner chuyên dụng hoàn tất không có secret chưa xử lý.
- 470 path docs được phân loại; public allowlist được review.
- Ba file untracked được giữ, di chuyển hoặc xóa bằng quyết định riêng; không vô tình stage.
- Diff public cuối cùng không có INTERNAL/RESTRICTED/NEVER-GIT.
- Có bản sao/private baseline xác nhận trước mọi thao tác rewrite lịch sử.

## 8. Tự kiểm

- Chỉ dùng lệnh Git read-only; không fetch, push, đổi remote hoặc rewrite history.
- Không in giá trị nghi secret; kết quả regex được phân loại bằng đầu ra che giá trị.
- Không stage hoặc xóa ba file docs untracked.
- Khuyến nghị dựa trên lịch sử local hiện có; chưa thay thế secret scan chuyên dụng.
