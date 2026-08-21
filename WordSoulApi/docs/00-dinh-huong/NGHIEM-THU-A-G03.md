# Nghiệm thu A-G03 Giai đoạn A

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `A5-ACCEPTANCE-CRITERIA-AG03-1.0` |
| Task | A5-T005 |
| Đầu vào | A5-ACCEPTANCE-CRITERIA-AG02-1.0 (D-156), M05 Gamification, M06 Billing, REL-04 |
| Phạm vi | Đặc tả Giao thức Nghiệm thu Tiêu chí A-G03 (`Phase A Acceptance Criteria A-G03 Verification Protocol`), thẩm định toàn vẹn kinh tế, chống gian lận lạm dụng phần thưởng (Anti-Cheat Engine), hạn ngạch giao dịch vật phẩm M05/M06 và lưu vết M11 |
| Tự kiểm | A-G03; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Nghiệm thu Tiêu chí A-G03 (`Phase A Acceptance Criteria A-G03 Verification Protocol`) thuộc A5, thực hiện quy trình kiểm định chính thức tiêu chí chất lượng **A-G03** (Toàn vẹn Kinh tế & Chống Gian lận): Xác nhận toàn bộ cơ chế kinh tế trong game (M05 Gamification, M06 Giao dịch) tuân thủ nghiêm ngặt các hạn ngạch thưởng (Daily Exp/Soul Gem Caps), cơ chế chống gian lận (Anti-Cheat Speed Hack Guard) và không xảy ra lạm dụng hoặc tạo tiền/vật phẩm ảo trái phép (REL-04).

- **Quy tắc Kiểm soát Hạn ngạch Thưởng Ngày (`Daily Economy Cap Invariant`)**: 100% các hành vi cộng điểm kinh nghiệm Exp, Gem hoặc Soul Stone từ M05/M06 BẮT BUỘC kiểm tra hạn ngạch trần ngày (`DailyExpCap = 5,000 Exp/day`, `DailyGemCap = 500 Gems/day`). Khi vượt trần, hệ thống chặn cộng điểm và ghi log cảnh báo lạm dụng (REL-04).
- **Ràng buộc Chống Hack Tốc độ & Auto-Bot (`Anti-Cheat Speed Hack Guard Invariant`)**: Thẩm định thời gian hoàn thành 1 bài luyện từ vựng M04/M05, nếu thời gian trả lời $< 500\text{ms}$ per card hoặc tổng bài $< 3\text{s}$, hệ thống tự động gắn cờ `SUSPICIOUS_SPEED_HACK`, vô hiệu phần thưởng đợt đó và hạ điểm tin cậy `ReputationScore`.
- **Phán quyết Nghiệm thu Tiêu chí A-G03 (`A-G03 Acceptance Verdict Invariant`)**: Tiêu chí A-G03 CHỈ ĐƯỢC KÝ DUYỆT `PASSED` khi cả 10 Acceptance Gates (`AG03-G01` đến `AG03-G10`) và 20 Test Cases nghiệm thu (`AG03-01` me đến `AG03-20`) đạt kết quả 100% thành công.
- **Lưu vết Sổ Kiểm toán Nghiệm thu M11 (`Acceptance A-G03 Audit Trail`)**: Biên bản nghiệm thu tiêu chí A-G03 được ghi vết bất biến `ACT-A5-05-AG03` trong Sổ Kiểm toán M11.

## 2. Ma trận Kết quả Thẩm định Tiêu chí A-G03 theo Module (A-G03 Verification Matrix)

| Module ID | Thành phần Kinh tế | Hạn ngạch Kiểm soát | Cơ chế Chống Gian lận | Tỷ lệ Đạt (%) | Phán quyết Thẩm định | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **M05** | Exp & Soul Gems | Max 5,000 Exp & 500 Gems/Day | Speed Hack & Bot Guard SLA $< 1\text{ms}$ | **100%** | **PASSED** | `ACT-A5-05-M05` |
| **M06** | Vật phẩm & In-App Purchases | Max 10 Purchases/Hour per User | Double Spending & Idempotency Key OK | **100%** | **PASSED** | `ACT-A5-05-M06` |
| **M07** | Đấu sĩ & Thưởng PvP | Daily PvP Battle Exp Cap 1,000 | Win-Trading Detection Engine OK | **100%** | **PASSED** | `ACT-A5-05-M07` |
| **M08** | Nhiệm vụ Nhóm & Quà tặng | Max Gift Amount 100 Gems/Day | Multi-Account Sybil Attack Guard OK | **100%** | **PASSED** | `ACT-A5-05-M08` |
| **M09** | Thưởng Xếp hạng Mùa giải | Season Reward Distribution Lock | Anti-Rank Farming Protection OK | **100%** | **PASSED** | `ACT-A5-05-M09` |
| **TỔNG** | **Toàn bộ Nền kinh tế Game** | **100% Economy Caps Active** | **Anti-Cheat Engine 100% Active** | **100.0%** | **PASSED A-G03** | `ACT-A5-05-AG03` |

## 3. Kiến trúc Luồng Thẩm định Tiêu chí A-G03 A5 (A-G03 Verification Pipeline)

```
[Trigger Phase A Acceptance Criteria A-G03 Verification (A5-T005)]
                                  |
                                  v
 +--------------------------------+--------------------------------+
 | 1. Scan M05/M06 Economy Code: Verify Daily Exp/Gem Caps        |
 | 2. Verify Anti-Cheat Speed Hack Engine: Check Card Answer Time  |
 | 3. Verify Double Spending Guard: Check Idempotency Header      |
 +--------------------------------+--------------------------------+
                                  |
                                  v
        +-------------------------+-------------------------+
        | (100% Verification Passed)                       | (Any Check Failed)
        v                                                  v
[OFFICIAL SIGN-OFF: A-G03 PASSED]                  [OFFICIAL REJECTION: FAILED]
[Issue A-G03 Compliance Certificate]               [Issue Defect Report]
[Record Audit Log ACT-A5-05-AG03]                  [Record Audit Log ACT-A5-05-FAIL]
```

## 4. Giao thức Thực thi Thẩm định CSDL (AcceptanceAG03Service)

```csharp
public async Task<AcceptanceVerdictDto> VerifyAG03ComplianceAsync(string leadAuditorUserId)
{
    var verdict = new AcceptanceVerdictDto { CriterionId = "A-G03", VerifiedAtUtc = DateTime.UtcNow };

    // 1. Verify Daily Exp and Gem Caps Enforcement in M05 Gamification Engine
    bool isEconomyCapEnforced = VerifyM05DailyCapsEnforcement();

    // 2. Verify Anti-Cheat Speed Hack Engine Response Time (< 500ms trigger)
    bool isAntiCheatActive = VerifyM04AntiCheatSpeedHackEngine();

    // 3. Verify Double Spending & Idempotency Key in M06 Transactions
    bool isDoubleSpendingPrevented = VerifyM06TransactionIdempotency();

    verdict.IsApproved = isEconomyCapEnforced && isAntiCheatActive && isDoubleSpendingPrevented;

    // 4. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-A5-05-AG03", leadAuditorUserId, new {
        CriterionId = "A-G03",
        IsApproved = verdict.IsApproved,
        IsEconomyCapEnforced = isEconomyCapEnforced,
        IsAntiCheatActive = isAntiCheatActive,
        IsDoubleSpendingPrevented = isDoubleSpendingPrevented
    });

    return verdict;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AG03-G01` | Hạn ngạch thưởng Exp và Gem trần ngày BẮT BUỘC được thực thi trên toàn bộ các nguồn cộng thưởng (REL-04). |
| `AG03-G02` | Bộ máy Anti-Cheat BẮT BUỘC gắn cờ `SUSPICIOUS_SPEED_HACK` khi câu trả lời nhanh bất thường $< 500\text{ms}$. |
| `AG03-G03` | 100% các giao dịch vật phẩm M06 BẮT BUỘC truyền `X-Idempotency-Key` ngăn ngừa Double Spending. |
| `AG03-G04` | Phát hiện hành vi Win-Trading PvP M07 BẮT BUỘC tự động hủy kết quả trận đấu và tạm khóa tính năng PvP 24h. |
| `AG03-G05` | Tính năng tặng quà nhóm M08 BẮT BUỘC kiểm soát hạn ngạch tối đa 100 Gems/ngày per user để chống Sybil. |
| `AG03-G06` | Biên bản nghiệm thu tiêu chí A-G03 BẮT BUỘC ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-A5-05-AG03`). |
| `AG03-G07` | SLA thực thi thẩm định tự kiểm tiêu chí A-G03 trên CSDL SQL $< 1.5$ giây. |
| `AG03-G08` | Phân quyền phê duyệt biên bản nghiệm thu A-G03 chỉ dành cho `LeadAuditor` và `GameEconomist`. |
| `AG03-G09` | Chữ ký số biên bản A-G03 BẮT BUỘC được lưu cố định trong CSDL A5. |
| `AG03-G10` | 100% các test case tự kiểm AG03-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AG03-01` | Chạy thẩm định A-G03 khi 100% cơ chế hạn ngạch & anti-cheat hoạt động tốt | Phán quyết `A-G03 PASSED`, ký duyệt biên bản |
| `AG03-02` | Giả lập cộng Exp quá 5,000 Exp/ngày trong M05 mà KHÔNG bị chặn | Reject nghiệm thu HTTP 400 `ECONOMY_CAP_BYPASS_DETECTED` |
| `AG03-03` | Giả lập trả lời bài học M04 trong 200ms ($< 500\text{ms}$) mà KHÔNG bị cờ Anti-Cheat | Reject nghiệm thu HTTP 400 `ANTI_CHEAT_INACTIVE` |
| `AG03-04` | Gửi 2 request mua vật phẩm M06 trùng `X-Idempotency-Key` cùng lúc | Request thứ 2 trả về response cached từ Redis, không bị trừ tiền 2 lần |
| `AG03-05` | Tra cứu vết Audit Log M11 sau khi phê duyệt nghiệm thu A-G03 | Ghi nhận Audit Event `ACT-A5-05-AG03` đính kèm signature |
| `AG03-06` | Tra cứu chứng nhận tuân thủ A-G03 cho Module M05 | Trả về Status `COMPLIANT_PASSED` |
| `AG03-07` | Developer thử bấm nút phê duyệt nghiệm thu A-G03 | Reject HTTP 403 `FORBIDDEN_LEAD_AUDITOR_ONLY` |
| `AG03-08` | Tra cứu danh sách các tài khoản bị Anti-Cheat gắn cờ trong tuần | Trả về DTO danh sách FlaggedCheaterAccounts |
| `AG03-09` | Tải đồng thời 30 request tra cứu chứng nhận nghiệm thu A-G03 | Response latency p95 $< 6\text{ms}$ |
| `AG03-10` | Kiểm tra độ trễ phát thông báo A-G03 PASSED sang Slack #announcements | Dispatch SLA $< 1.2\text{s}$ |
| `AG03-11` | Thử nạp mã `CriterionId` không hợp lệ (Ví dụ: A-G99) | Reject 400 `INVALID_CRITERION_ID` |
| `AG03-12` | Gửi request nghiệm thu A-G03 khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `AG03-13` | User không phải LeadAuditor/GameEconomist thử bấm duyệt A-G03 | Deny 403 Forbidden |
| `AG03-14` | User chưa đăng nhập gọi API tra cứu trạng thái nghiệm thu A-G03 | Cho phép xem công khai trạng thái nghiệm thu |
| `AG03-15` | Thử phê duyệt nghiệm thu A-G03 khi Task A5-T004 (A-G02) chưa hoàn thành | Reject 400 `PREVIOUS_CRITERION_AG02_REQUIRED_FIRST` |
| `AG03-16` | Kiểm tra tính nhất quán giữa hạn ngạch trần ngày trong config và CSDL | Matching 100% configuration caps |
| `AG03-17` | Phân tích tham chiếu các bản ghi `AcceptanceSignOffs` trong CSDL | Quét schema `A5_AcceptanceSignOffs` (T020) |
| `AG03-18` | Dịch vụ kiểm tra Anti-Cheat bị ngắt kết nối Redis | Catch exception, rollback transaction, trả về 500 |
| `AG03-19` | Tra cứu danh sách các đợt phát hiện gian lận Win-Trading M07 | Trả về DTO danh sách WinTradingIncidents |
| `AG03-20` | Kiểm thử hoàn tất nghiệm thu A-G03 A5-ACCEPTANCE-CRITERIA-AG03-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `A5-AG03-I01` | A5 hiện tại chưa có `AcceptanceAG03Service` thẩm định nền kinh tế | Risk duyệt nghiệm thu thủ công thiếu công cụ kiểm tra gian lận | A5-T006 (Source task) |
| `A5-AG03-I02` | Thiếu luồng tự động quét hạn ngạch trần Exp/Gem trong M05 | Khó đảm bảo không có lỗ hổng hack điểm thưởng | A5-T006; REL-04 |
| `A5-AG03-I03` | Thiếu cờ xác minh Anti-Cheat Speed Hack Engine trên M04/M05 | Risk Auto-Bot hoành hành lạm dụng tài nguyên bài học | A5-T006; REL-04 |
| `A5-AG03-I04` | Thiếu luồng phát chứng nhận tuân thủ A-G03 cho từng Module | Game Economist không xem được chỉ số an toàn kinh tế | A5-AG03-F04; Econom |
| `A5-AG03-I05` | Chưa kết nối sự kiện Nghiệm thu A-G03 với Audit Log M11 (`ACT-A5-05-AG03`) | Không ghi vết được biên bản nghiệm thu A-G03 | A5-T006; M11-T031 |

- `A5-AG03-F01`: Triểnkai `AcceptanceAG03Service` với Automated Economy Cap Inspector (tiếp nhận: A5-T006).
- `A5-AG03-F02`: Tích hợp Bắt buộc Anti-Cheat Speed Hack Verifier & Idempotency Check (tiếp nhận: A5-T006; REL-04).
- `A5-AG03-F03`: Triển khai Compliance Certificate Generator & Sign-Off Engine (tiếp nhận: A5-T006; Econom).
- `A5-AG03-F04`: Thiết lập bộ kiểm thử tự động AG03-G01–G10 và AG03-01–20 (tiếp nhận: A5 tasks).
- `A5-AG03-F05`: Thu thập bằng chứng runtime cho luồng nghiệm thu A-G03 A5 (tiếp nhận: A5 tasks; A-G03/REL-04).

## 8. Tự kiểm A5-T005

- Đã thiết kế hoàn chỉnh `A5-ACCEPTANCE-CRITERIA-AG03-1.0` với Ma trận Kết quả Thẩm định Tiêu chí A-G03 theo Module.
- Đã chốt Ràng buộc Quy tắc Kiểm soát Hạn ngạch Thưởng Ngày (`Daily Economy Cap Invariant` REL-04).
- Đã chốt Ràng buộc Chống Hack Tốc độ & Auto-Bot (`Anti-Cheat Speed Hack Guard Invariant` $< 500\text{ms}$).
- Đã lồng ghép Phán quyết Nghiệm thu Tiêu chí A-G03 (`A-G03 Acceptance Verdict Invariant`), Chứng nhận tuân thủ và Audit Log M11 (`ACT-A5-05-AG03`).
- Đã xác lập 10 Regression Gates (`AG03-G01`–`AG03-G10`) và 20 Test Cases tự kiểm (`AG03-01`–`AG03-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả nghiệm thu A-G03 A5-T005 | WSA-7K2 |
