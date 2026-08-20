# Kiểm soát thử đăng nhập bất thường M01

## Quản trị tài liệu

| Trường | Giá trị |
|---|---|
| Task nguồn | M01-T011 |
| Policy ID / phiên bản | M01-ABUSE-1.0 |
| Trạng thái | Baseline thiết kế có hiệu lực từ 2026-08-20 |
| Chủ tài liệu / xác nhận | WSA-7K2 / WSA-7K2 |
| Cơ sở | D-012, D-017, D-024–D-026; M01-LOGIN-1.0; M12-RATE-1.0 RL-004; M12-FAIL-1.0 |
| Phạm vi | Direct password login: brute force, credential stuffing, password spraying, distributed attempts, lockout/recovery/alert |
| Ngoài phạm vi | External identity abuse; session/token lifecycle T016–T018; recovery contract T019; alert operations T038–T039 |

## 1. Mục tiêu và bất biến

- Chặn thử sai tập trung và phân tán mà không tạo khóa vĩnh viễn hoặc cho kẻ tấn công dễ dàng khóa tài khoản của người khác.
- Nonexistent, no-direct-credential và wrong password có cùng public envelope/timing class; counter cho account candidate hoạt động giống nhau dù account có tồn tại hay không.
- Chỉ identity server xác lập partition. Email dùng HMAC của canonical form, IP qua trusted-proxy algorithm, device dùng protected verified ref; không dùng raw email/IP/user-agent làm key log/metric.
- Failed attempts đơn thuần **không** thu hồi phiên đang hợp lệ, khóa quản trị hoặc đổi account lifecycle state. Chỉ bằng chứng compromise/recovery/admin policy riêng mới làm việc đó.
- Risk state không thay password verification, state/policy gate, audit hoặc session commit. Correct password trong risk state có thể cần step-up và không tự cấp full session.
- Không CAPTCHA/device fingerprinting/provider mới trong baseline. Thêm signal/provider phải qua data-purpose/privacy/contract/REL review.
- Limiter/risk/audit state không chắc chắn thì fail-closed/conservative theo C0; không `GetNoLimiter`, allow-all hoặc silent reset counter.

## 2. Threat và signal allowlist

| Signal ID | Signal | Cách tính | Không được suy |
|---|---|---|---|
| SIG-01 | Failure velocity theo account candidate | Số credential failure trên HMAC(email canonical), gồm account không tồn tại | Account tồn tại, user gian lận hoặc compromise đã xảy ra |
| SIG-02 | Failure velocity theo source | Failure trên protected client-IP bucket sau trusted proxy | Một IP là một người; shared NAT tự là malicious |
| SIG-03 | Failure velocity theo verified device | Failure theo device ref đã bound server-side | Device ID tự khai là thiết bị tin cậy |
| SIG-04 | Password spray breadth | Số account-candidate HMAC khác nhau thất bại từ một source trong cửa sổ | Email/account thật hoặc nội dung password |
| SIG-05 | Distributed account attack | Số source bucket khác nhau thất bại trên cùng account candidate | Vị trí địa lý chính xác hoặc danh tính attacker |
| SIG-06 | Successful credential trong risk episode | Verifier đúng khi account candidate đang Elevated/Protected | Đủ điều kiện full session; vẫn cần state/policy/step-up |
| SIG-07 | Known session/device context | Session/device ref hợp lệ, security epoch và recent activity | Request body/header tự khai là trusted device |
| SIG-08 | Control/telemetry integrity | Policy version, counter freshness, audit commit, proxy/risk resolver health | Missing event nghĩa là không có rủi ro |

Không lưu hoặc so sánh password để phát hiện spraying. Hash verifier chỉ trả category đúng/sai/lỗi; password/hash/credential payload không vào risk event.

## 3. Bucket nền bắt buộc

Các bucket được áp dụng đồng thời; queue bằng 0. `Retry-After` không cho biết bucket nào đã hết.

| Bucket | Ngưỡng | Phạm vi | Mục đích |
|---|---|---|---|
| B-01 Source/IP | 10 attempt / 15 phút | Protected IP + login route | Coarse brute-force/NAT guard trước lookup |
| B-02 Account candidate failures | 5 failure / 15 phút | HMAC canonical account candidate | Chặn một account qua nhiều source |
| B-03 Verified device failures | 10 failure / 15 phút | Device ref + login route | Chặn source đổi IP trên cùng device đã biết |
| B-04 Spray breadth | 10 account candidate khác nhau / 15 phút | Protected source | Phát hiện một source thử nhiều account; cô lập source 60 phút, không đổi risk state của mọi account đã thử |
| B-05 Distributed breadth | 5 source bucket khác nhau / 15 phút | Account candidate | Phát hiện nhiều source đánh cùng account |
| B-06 Protected credential probe | 1 attempt / 5 phút, tối đa 3 / 60 phút | Account candidate trong Elevated | Cho chủ hợp lệ chứng minh credential để vào step-up mà không mở brute force |

- B-01/B-03 là attempt budget; B-02/B-04/B-05 chỉ tăng sau generic credential failure. Schema/limiter reject trước verifier không được tính là password failure.
- B-04 tạo source-isolation episode có hạn; nó không khóa account candidate. Shared NAT được theo dõi false-positive và tự hết hạn, không có allowlist/bypass thủ công.
- Success không xóa audit/history hoặc reset ngay source breadth; counter decay theo cửa sổ. Account risk episode chỉ đóng sau recovery/step-up hoặc expiry an toàn.
- Counter update phải atomic và aggregate xuyên instance. Key có environment + policy/version + HMAC key-version; rotation không tạo cửa sổ quota gấp đôi.
- Nonexistent candidate có cùng B-02/B-05 nhưng không tạo fake account row; state ở risk store có TTL và protected key.

## 4. Risk state và ngưỡng

| State | Điều kiện vào (một điều kiện đủ) | Thời hạn tự động tối đa | Login behavior | Thoát state |
|---|---|---|---|---|
| Normal | Không điều kiện Elevated/Protected/Recovery | — | Pipeline M01-LOGIN-1.0 + B-01–B-05 | Khi threshold khác đạt |
| Elevated | B-02 đạt 5/15 phút hoặc B-05 đạt 5 source/15 phút | 30 phút từ failure cuối, không kéo dài quá 24 giờ chỉ bởi traffic | Normal verifier bị giới hạn; B-06 cho correct credential nhưng chỉ phát action ticket step-up, chưa full session | Step-up hợp lệ; hoặc hết hạn và counter dưới threshold |
| Protected | 10 failure/60 phút/account hoặc 10 source/60 phút/account | 2 giờ từ trigger cuối; tổng tự động không quá 24 giờ | Không password login bình thường; chỉ recovery/verified existing-session approval/controlled step-up | Recovery hoặc verified approval; nếu tự hết hạn quay Elevated trước Normal |
| Recovery required | 20 failure/24 giờ/account; hoặc compromise đã được xác nhận bởi recovery/security process | 24 giờ nếu chỉ do threshold; confirmed compromise cần recovery đóng episode | Không full session bằng password; dùng M01-T019/support; confirmed compromise áp session action theo T016–T020 | Recovery credential + step-up/audit; security/admin decision có reason |

Ngưỡng là baseline v1.0 và chỉ được siết/nới bằng policy version có audit/canary. Risk episode không dùng trạng thái `Khóa quản trị` hoặc `Ngừng hoạt động`; đó là lifecycle decision khác với owner/reason/appeal riêng.

## 5. Quyết định theo từng attempt

1. Validate schema/size/version và trusted client context; reject trước lookup nếu sai.
2. Tính protected source/account/device keys, đọc policy/risk state. Missing/invalid C0 state → 503 hoặc local conservative deny; không normal login.
3. Áp B-01/B-03 và source spray guard. Nếu hết → generic 429; không lookup/verifier.
4. Với Normal, thực hiện lookup/dummy verify theo M01-LOGIN-1.0. Failure tăng B-02/B-04/B-05 atomic, đánh giá state và ghi audit đúng một lần.
5. Với Elevated, chỉ B-06 cho phép verifier thưa. Failure giữ generic result; correct credential không cấp session mà tạo action ticket purpose-bound cho step-up.
6. Với Protected/Recovery required, không chạy normal verifier; trả generic unavailable/rate-limited và chỉ mở recovery/approval journey đã xác minh.
7. Correct credential ở Normal vẫn qua account/email/policy/security epoch/audit/session gate. Risk evaluator không thể cấp token.
8. Mọi transition dùng compare-and-set/version; concurrent attempt không tạo hai episode, hai alert hoặc hạ state.

## 6. Step-up và phục hồi không gây khóa vĩnh viễn

| Đường | Được dùng khi | Bằng chứng | Tác động |
|---|---|---|---|
| Verified email step-up | Elevated/Protected và email/kênh còn hợp lệ | One-time intent, 30 phút, resend/rate theo D-014/RL-006–RL-007 | Đóng/giảm episode, sau đó tạo login operation mới hoặc session theo contract |
| Existing trusted session approval | Session/device còn hợp lệ, không thuộc compromise scope | Re-auth/recent session + security epoch + action ticket | Cho xác nhận attempt/device; không tự trust source khác |
| Password recovery | Protected/Recovery required hoặc user chủ động | M01-T019 one-time evidence; response không lộ account | Đổi credential; confirmed compromise thu hồi session theo D-009 |
| Controlled support | Mất mọi kênh | M01-T021 evidence/dual control/audit | Không hạ chuẩn chỉ vì rate state hoặc urgency |
| Automatic decay | Chỉ threshold-based, không confirmed compromise | Server time + counter/episode version | Recovery required → Elevated → Normal; không nhảy thẳng khi signal còn cao |

- Failed attempts không gửi vô hạn email: một security notification cho mỗi transition/episode class, tối đa một bản lặp/24 giờ nếu chưa có transition mới.
- Notification failure giữ durable alert intent/HOLD nhưng không xóa risk state. Audit/risk commit bắt buộc lỗi thì không cấp session/action ticket.
- Step-up/recovery public response chỉ được đưa sau credential/session evidence phù hợp; nonexistent/wrong-password path không lộ rằng kênh tồn tại.

## 7. Alert và metric

| Alert ID | Trigger | Mức/đối tượng | Hành động |
|---|---|---|---|
| ALT-01 | Account candidate chuyển Elevated | User security inbox/email dedup + operational info | Hướng dẫn kiểm tra/step-up; không hiển thị raw IP/location |
| ALT-02 | Chuyển Protected/Recovery required | User security alert + operational high | Giữ episode, kiểm tra source breadth, recovery path; không auto-revoke chỉ bởi failures |
| ALT-03 | Source đạt 10 candidate/15 phút hoặc tiếp tục sau reject | Operational high | Cô lập source bucket, kiểm proxy spoof/NAT, theo dõi distributed spread |
| ALT-04 | ≥20 account candidate chuyển Elevated từ cùng source class trong 5 phút | Operational high | Điều tra spray/credential stuffing; siết versioned edge/source policy nếu cần |
| ALT-05 | Invalid-credential rate >5× median cùng khung giờ 7 ngày và ≥100 failure/5 phút | Operational high | Kiểm telemetry/control integrity, campaign scope và false positive |
| ALT-06 | Limiter/risk/audit health hoặc event freshness không đạt | Operational critical cho login C0 | Fail-closed/conservative, incident/playbook; không coi thiếu event là normal |

Metric tối thiểu (không label raw identity): attempt/result/risk-state-transition theo channel/app version/coarse source class; limiter reject theo bucket class; dummy/real verifier latency distribution; step-up success/failure; recovery age; alert delivery; risk-store/audit health; false-positive proxy như successful recovery sau threshold.

M01-T039 chốt owner/escalation/response target và tuning cadence. Khi chưa đủ median 7 ngày cho ALT-05, dùng ngưỡng tĩnh ≥100 failure/5 phút và đánh dấu baseline chưa trưởng thành. Tuning chỉ dựa aggregate/redacted data; không hạ threshold thủ công cho một account/source.

## 8. Dữ liệu, retention và privacy

| Dữ liệu | Lưu tối đa / owner | Quy tắc |
|---|---|---|
| Hot rate counters | Cửa sổ + grace, không quá 24 giờ / M12 | Protected key, atomic TTL; không raw email/IP/password |
| Risk episode/state | Đến khi đóng + 30 ngày metadata tối thiểu / M01 security | Protected account-candidate/subject ref, transition/reason/policy version |
| Security/audit event | 12 tháng mặc định theo M01-D025 / M01+M11 | Outcome/category/coarse source; legal hold có scope/time riêng |
| Device/source linkage | Theo session/device policy / M01 | Verified protected ref; không fingerprint mới hoặc raw user-agent history |
| Nonexistent candidate | Hot counter/audit category theo TTL | Không fake account, không contact/profile row, không notification |

HMAC key nằm trong secret store, có version/rotation/access audit; digest không dùng làm metric label. Raw network logs nếu có thuộc ingress security policy riêng, không được sao chép vào login risk store/evidence.

## 9. Failure mode

| Failure | Hành vi bắt buộc |
|---|---|
| Distributed counter unavailable | Trusted local source cap bảo thủ + deny account-sensitive path nếu risk state không xác định; không allow-all |
| Risk policy/config missing/invalid | 503 generic trước verifier/session; không dùng primitive default 100/60 |
| Audit durable write unavailable | Không token/action ticket; 503 generic, giữ correlation protected |
| Notification unavailable | Risk/audit state vẫn commit; durable alert intent pending, không gửi lặp mù |
| Trusted proxy chain/config unknown | Dùng socket peer nếu policy hợp lệ hoặc deny; không nhận caller XFF |
| Verifier lỗi/cost metadata unsupported | Fail-closed theo M01-LOGIN-1.0; không tăng wrong-password counter như credential failure chắc chắn |
| State conflict/concurrent transition | Retry CAS có trần cùng attempt; nếu vẫn unknown thì không session và reconcile |

## 10. Đối chiếu hiện trạng tĩnh

| Finding ID | Quan sát | Sai lệch / rủi ro | Task tiếp nhận |
|---|---|---|---|
| M01-ABUSE-I01 | Auth limiter hiện là local fixed-window theo IP; không thấy account/device/distributed breadth/risk state | Distributed stuffing/spray chưa được kiểm soát; NAT fairness chưa có | M01-T038–T039; M12-T047-A |
| M01-ABUSE-I02 | `GetClientIp` tin leftmost XFF parse được | Caller có thể chọn/bypass source partition nếu ingress không strip | M12-T047-A; A-G01/A-G04 |
| M01-ABUSE-I03 | Internal loopback header dùng no-limiter và Redis limiter fail-open | Có đường bypass/allow-all trái CT-05 | M12-T047-A; REL-03 |
| M01-ABUSE-I04 | Không thấy failed-login audit/risk episode; login chỉ ghi activity success | Không điều tra/tuning/alert được campaign | M01-T038–T039, T041; M11-T031–T035 |
| M01-ABUSE-I05 | Không thấy dummy verifier/timing-equalized nonexistent path | Enumeration timing có thể tồn tại | M01-T042-A |
| M01-ABUSE-I06 | Test limiter chỉ dùng minimal in-process auth endpoint và tin XFF trong fixture | Chưa chứng minh host thật, composite/multi-instance/failure/recovery | M01-T042-A; M12-T047-A; REL-03 |

Các sai lệch I01–I06 là release blocker cho direct login theo M01-ABUSE-1.0 đến khi source/runtime evidence tương ứng đạt; tài liệu không tự chứng minh enforcement.

## 11. Ma trận nghiệm thu

| Case ID | Kịch bản | Kết quả bắt buộc |
|---|---|---|
| AB11-01 | 5 wrong password cùng account/15 phút | Elevated đúng một lần; generic failure; alert dedup |
| AB11-02 | Nonexistent candidate cùng pattern | Public behavior/counter/timing class tương đương; không fake account/notification |
| AB11-03 | 10 account khác nhau từ một source/15 phút | Spray/source control + ALT-03; không lộ account nào tồn tại |
| AB11-04 | Một account từ 5 source/15 phút | Elevated/distributed signal; source spoof không tạo trusted identity |
| AB11-05 | Correct password khi Elevated | Chỉ B-06 và step-up ticket; chưa full session |
| AB11-06 | Wrong password khi B-06 hết | 429/generic; không verifier/session/downstream |
| AB11-07 | 10 failures/60 phút hoặc 10 source/60 phút | Protected có hạn, recovery paths rõ; không admin/permanent lock |
| AB11-08 | 20 failures/24 giờ | Recovery required tối đa 24 giờ nếu chưa confirmed compromise |
| AB11-09 | Failed attack khi user có session hợp lệ | Không auto-revoke session chỉ vì failures; sensitive action vẫn risk-aware |
| AB11-10 | Verified step-up/recovery | CAS đóng/giảm episode, audit; operation mới rõ, không reset source evidence |
| AB11-11 | Nhiều user hợp lệ sau NAT | Composite buckets/fairness telemetry; không manual bypass hoặc permanent IP ban |
| AB11-12 | Giả XFF/internal/device header | Không đổi trusted partition/bucket/quota |
| AB11-13 | Hai instance xử lý đồng thời threshold | Tổng counter/state/alert atomic, không nhân quota/transition |
| AB11-14 | Counter/risk/audit store lỗi | FC/CM bảo thủ, 503/429 generic; không token |
| AB11-15 | Verifier lỗi | Không tính wrong password chắc chắn, không session, audit dependency category |
| AB11-16 | Alert channel lỗi | Risk state giữ nguyên, durable alert pending, không spam retry |
| AB11-17 | Episode tự hết hạn | Recovery → Elevated → Normal theo signal; không khóa vĩnh viễn/nhảy sai |
| AB11-18 | Timing/enumeration/redaction sweep | Wrong/nonexistent/no-direct không phân biệt thực tế; không PII/secret raw |

## 12. Finding còn mở

| Finding ID | Khoảng trống | Baseline an toàn | Task tiếp nhận |
|---|---|---|---|
| M01-ABUSE-F01 | Session encoding và recovery proof phía sau risk action ticket | Ticket/state interaction đã chốt ở M01-INACTIVE-1.0; không full session; risk state khác admin lock | M01-T016, M01-T019 |
| M01-ABUSE-F02 | Event schema, operational alert owner/response/tuning | Metadata allowlist, no raw identity, threshold chỉ versioned | M01-T038–T041; M11-T031–T037 |
| M01-ABUSE-F03 | Distributed store, proxy, composite bucket và failure runtime | Không allow-all; direct login chưa đạt release | M12-T047-A; REL-03 |
| M01-ABUSE-F04 | Session action khi compromise được xác nhận | Failed attempts không revoke; confirmed compromise theo D-009 | M01-T016–T020 |

## 13. Tự kiểm M01-T011, A-G01, A-G04 và REL-03

- Sáu bucket, bốn risk state và năm recovery path có ngưỡng/thời hạn rõ; không có khóa vĩnh viễn hoặc dùng admin lock cho automated risk.
- Composite account/source/device/spray/distributed controls giữ public behavior cho nonexistent/wrong và giảm lockout DoS bằng protected credential probe + step-up.
- Existing session không bị thu hồi chỉ bởi failures; correct password trong risk state không tự cấp full session; audit/session control vẫn fail-closed.
- Sáu alert, metric tối thiểu, retention/redaction/HMAC key rules đáp ứng thiết kế điều tra mà không lưu raw password/email/IP.
- 18 case bao phủ stuffing/spraying/distributed/NAT/spoof/concurrency/failure/recovery/timing; A-G01/A-G04 có baseline nhưng runtime evidence chưa có.
- 6 sai lệch và 4 finding mở có task tiếp nhận; REL-03 vẫn mở đến khi source/host/multi-instance/failure/alert evidence đạt.

## Lịch sử

| Ngày | Phiên bản | Thay đổi | Người xác nhận |
|---|---|---|---|
| 2026-08-20 | 1.0 | Chốt 6 bucket, 4 risk state, recovery/alert/privacy và 18 case | WSA-7K2 |
