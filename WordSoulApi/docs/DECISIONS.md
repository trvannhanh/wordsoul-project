# Decisions

Chỉ cập nhật file này khi thay đổi hành vi sản phẩm, kiến trúc, dữ liệu, bảo mật hoặc quy trình chung. Thay đổi triển khai nhỏ được ghi trực tiếp ở `TASKS.md` và Git.

## Quyết định đang có hiệu lực

| ID | Quyết định | Hệ quả |
|---|---|---|
| D-001 | WSA-7K2 là người thực hiện và tự nghiệm thu duy nhất | Không cần reviewer, authority, bàn giao hoặc chữ ký; REL/CT/gate là checklist tự kiểm |
| D-002 | Dùng checkout hiện tại của `vocamon-project` | Không tạo worktree hoặc nhánh theo bí danh; luôn bảo toàn thay đổi có sẵn |
| D-003 | `TASKS.md` là nguồn task và trạng thái duy nhất | Bảng import, tracker theo lát và nhật ký WSA cũ chỉ để tham chiếu |
| D-004 | Git commit là nhật ký công việc | Không duy trì `NHAT-KY.md`; kết quả ngắn gọn nằm trong task và chi tiết nằm trong diff/commit |
| D-005 | Chỉ dùng bốn trạng thái | `Chưa bắt đầu`, `Đang thực hiện`, `Bị chặn`, `Hoàn thành` |
| D-006 | Evidence ID là tùy chọn | Chỉ tạo khi kết quả cần truy vết lâu dài; không phải điều kiện hoàn thành mặc định |
| D-007 | Kiến trúc API theo lớp | Domain → Application → Infrastructure → API; client không cung cấp domain truth đáng tin cậy |
| D-008 | Dữ liệu và bí mật được bảo vệ tại ranh giới | SQL là durable store, Redis là cache/coordination; không ghi secret, token, PII hoặc payload thô |
| D-009 | Task hậu tố `-A` chỉ hoàn thành phạm vi lát A | Không tự đóng parent hoặc phạm vi hoãn còn lại |
| D-010 | Giữ năng lực AI sinh nội dung và xử lý giọng nói người dùng tắt trong Giai đoạn A/B | Không mở endpoint/UI/job/provider traffic hoặc thu dữ liệu để dùng sau; chỉ thay đổi bằng quyết định mới sau khi REL-01/REL-03 và các cổng liên quan đạt |
| D-011 | Đóng băng AP và chuẩn bị loại bỏ trong Giai đoạn B | Không cấp, tiêu, điều chỉnh hoặc tạo phụ thuộc AP mới; chỉ giữ lịch sử để phục vụ REL-05/B-G03; xóa số dư phải có kế hoạch dữ liệu, đối soát, truyền thông và rollback |
| D-012 | Dùng chính sách thông tin bảo mật M01 v1.0 cho đăng nhập trực tiếp | Mật khẩu dài 12–128 ký tự, không trim/cắt, không ép tổ hợp hay đổi định kỳ; chặn giá trị hiện tại/phổ biến/đã lộ; creation/change/reset fail-closed khi không kiểm tra được; bí mật không vào log/audit/support |
| D-013 | Dùng hợp đồng đăng ký trực tiếp M01-REG-1.0 | Email canonical là định danh đăng nhập; tên hiển thị không duy nhất; request bắt buộc idempotency key giữ tối thiểu 24 giờ; email trùng nhận phản hồi trung tính; tài khoản mới chờ xác minh và không cấp token/tài sản inline |
| D-014 | Dùng luồng xác minh email M01-VER-1.0 | Intent sống 30 phút, mã Base32 10 ký tự chỉ nhận trong body và tối đa 10 lần thử; resend cách 60 giây, tối đa 5 lần/24 giờ, intent mới thu hồi intent cũ; email adapter idempotent và `accepted` không đồng nghĩa `delivered` |
| D-015 | Dùng hợp đồng ghi nhận đồng ý M01-CONS-1.0 | Tách acknowledgement bắt buộc, consent tùy chọn và preference; policy đã publish bất biến, quyết định append-only/idempotent; im lặng không phải đồng ý; thiếu/stale/unknown policy phải fail-closed cho purpose cần consent |
| D-016 | Dùng điều phối onboarding M01-ONB-1.0 và hợp đồng M06-ONB-A-1.0 | Account commit trước side effect; starter được chọn sau khi đủ điều kiện và do M06 cấp bằng operation idempotent; timeout phải reconcile, không cấp bù mù; không cấp hoặc tạo phụ thuộc AP mới |
| D-017 | Dùng hợp đồng đăng nhập trực tiếp M01-LOGIN-1.0 | Chỉ email canonical là định danh đăng nhập; nonexistent/no-direct-credential/password sai có failure chung; credential đúng vẫn phải qua account/email/policy gate; login không chạy daily quest/reward và chỉ trả token sau session/audit commit |
| D-018 | Dùng từ điển tích hợp M12-DICT-1.0 | Module nghiệp vụ chỉ phụ thuộc capability/result chuẩn, không phụ thuộc provider; source module sở hữu business operation ID; `null`, timeout và exception không phải kết quả; unknown phải reconcile, cache/shared state không là durable truth |
| D-019 | Dùng M12-CAP-REG-1.0 làm registry năng lực tích hợp | Tách implementation, configuration và activation; DI/config/credential presence không chứng minh enabled hay healthy; capability chưa có runtime evidence giữ `unknown`/`unverified`; AI sinh nội dung và xử lý giọng nói người dùng vẫn tắt theo D-010 |
| D-020 | Dùng M12-CRIT-1.0 để phân loại criticality theo lát use case | C0 bảo vệ safety/quyền/durable truth phải fail-closed hoặc dừng lát khi không chắc chắn; C1 giữ hành trình lõi bằng pending/reconcile; C2 được suy giảm mà không đổi truth; C3 không có traffic/thu dữ liệu; severity sự cố được đánh giá riêng theo tác động thật |
| D-021 | Dùng M12-CONTRACT-1.0 làm envelope dữ liệu chuẩn qua ranh giới tích hợp | Source module sở hữu operation ID/fingerprint/purpose/deadline; M12 sở hữu attempt và provider mapping; consumer chỉ nhận result versioned, protected refs và metadata allowlist; secret/provider payload không được vượt boundary |
| D-022 | Dùng M12-RESULT-1.0 làm taxonomy kết quả/lỗi tích hợp | Chỉ tám status chuẩn đi qua consumer boundary; `unknown` buộc reconcile khi effect chưa biết, `noData` không là lỗi, chỉ temporary failure đã chứng minh retry-safe mới có thể retry và final result không bị late/duplicate outcome ghi đè |
| D-023 | Dùng M12-STATE-REG-1.0 làm registry trạng thái chia sẻ | Cache/queue/connection/limiter/lease không là durable truth; local-process không được mô tả là distributed; mọi use case phải có source of truth, namespace, lifetime, consistency/quota, criticality và failure mode trước khi được phụ thuộc |
| D-024 | Dùng M12-RATE-1.0 làm policy catalog giới hạn lưu lượng | Mọi public/user/admin/callback/workload entry point có policy theo identity server xác lập; nhiều bucket áp dụng đồng thời, C0/C1 cần aggregate xuyên instance, internal worker không được bypass và missing/zero config không được thành unlimited |
| D-025 | Dùng M12-FAIL-1.0 làm ma trận fail mode và suy giảm | Không có security fail-open được phép; C0 dùng fail-closed/hold hoặc conservative cap, cache chỉ bypass về durable truth, C2 degrade bằng output đã duyệt, C3 luôn off; recovery phải qua health/contract/reconcile/canary thay vì một probe thành công |
| D-026 | Dùng M01-ABUSE-1.0 để kiểm soát thử đăng nhập bất thường | Áp composite source/account/device/spray/distributed buckets; automated risk chỉ có trạng thái tạm thời và recovery, không dùng khóa quản trị/vĩnh viễn; correct credential trong risk state cần step-up, còn failed attempts đơn thuần không tự thu hồi phiên hiện có |
| D-027 | Dùng M01-INACTIVE-1.0 để xử lý đăng nhập theo trạng thái tài khoản | State-specific result chỉ sau credential/session proof; chờ xác minh được limited session học giới hạn + ticket 10 phút, pending state khác chỉ có action đúng purpose; locked/inactive không tự mở, pending deletion chỉ review/cancel khi còn reversible, deleted identity luôn generic failure và không thể phục hồi/liên kết lại |
| D-028 | Dùng M01-SESSION-1.0 làm chính sách vòng đời phiên | Mỗi login tạo session family riêng; learner access 15 phút/idle 7 ngày/absolute 30 ngày, limited 10 phút/24 giờ/7 ngày, admin 10 phút/30 phút/8 giờ; refresh one-time chỉ lưu digest, access luôn chịu current state/security/policy/restriction và revocation không chờ token hết hạn |
| D-029 | Dùng M01-REFRESH-1.0 cho rotation và reuse detection | Refresh request không nhận user ID; mỗi generation có một CAS successor; retry cùng operation được trả đúng response mã hóa trong 60 giây, còn reuse bằng operation khác revoke family và cảnh báo; không tái tạo token khi escrow hết hạn hoặc commit chưa chắc chắn |
| D-030 | Dùng M01-RECOVERY-1.0 cho khôi phục quyền truy cập | Request luôn trung tính; chỉ email đã xác minh đủ điều kiện nhận code 12 ký tự sống 15 phút/tối đa 5 lần thử; reset atomically đổi verifier, tăng security epoch, thu hồi mọi phiên và bắt đăng nhập lại; không tự mở khóa quản trị hoặc phục hồi identity đã xóa |
| D-031 | Dùng M01-SEC-CHANGE-1.0 cho thay đổi mật khẩu/email | Security mutation chỉ từ full current session + re-auth tối đa 5 phút; đổi mật khẩu reissue đúng current family và revoke family khác; đổi email cần proof cả kênh cũ/mới, revoke all và login lại; profile/admin endpoint không được kiêm security change |
| D-032 | Dùng M11-DICT-1.0 làm từ điển quản trị | Role không phải permission, permission phải có scope và enforcement current-state; M11 không chiếm source truth module; audit/activity/log và các namespace account/session/change/config/job/case/incident/capability không được dùng thay nhau |

## Khi nào cần thêm quyết định

Thêm một dòng mới khi lựa chọn làm thay đổi API/schema, kiến trúc, quyền, dữ liệu, bảo mật, hành vi người dùng hoặc phạm vi phát hành. Ghi rõ lựa chọn và hệ quả; không tạo tài liệu quyết định riêng nếu một dòng ở đây đã đủ.

## Câu hỏi chưa chốt

- Baseline môi trường/cấu hình nào là nguồn thật cho release candidate đầu tiên?
- Thị trường, tuổi và consent nào được dùng để tự đóng REL-01?
- Ngoài AI sinh nội dung và xử lý giọng nói người dùng đã tắt theo D-010, năng lực provider nào bật trong Giai đoạn A và năng lực nào tiếp tục tắt?
- Web, admin và mobile có cần hội tụ semantic token hay tiếp tục dùng theme riêng?

Chi tiết lịch sử vẫn có thể tra cứu trong các `QUYET-DINH-MO.md`, hồ sơ REL/CT và [quyết định chuyển đổi solo](./04-thuc-thi/phan-cong/QUYET-DINH-THUC-THI-SOLO.md).
