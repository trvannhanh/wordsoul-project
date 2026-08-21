# Thiết kế cảnh báo hành vi danh tính bất thường M01

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M01-ANOMALOUS-IDENTITY-ALERT-1.0` |
| Task | M01-T039 |
| Đầu vào | M01-ABUSE-1.0 (D-026), M01-RECOVERY-1.0 (D-030), M01-IDENTITY-EVENT-CATALOG-1.0 (D-103), REL-01, REL-06 |
| Phạm vi | Đặc tả Giao thức Cảnh báo và Phát hiện Hành vi Danh tính Bất thường (`Anomalous Identity Alert Engine`), 5 quy tắc nhận diện rủi ro, tự động yêu cầu xác thực bổ sung (`Step-up Auth`) và phát cảnh báo PUSH/Email an ninh SLA $\le 10\text{s}$ |
| Tự kiểm | A-G01, A-G06 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Cảnh báo Hành vi Danh tính Bất thường (`Anomalous Identity Alert Engine`) thuộc M01, chủ động phát hiện các bất thường trong hành vi đăng nhập và quản lý danh tính của người dùng nhằm ngăn ngừa nguy cơ bị chiếm quyền tài khoản (Account Takeover - ATO).

- **5 Quy tắc Nhận diện Rủi ro Danh tính Cứng (`5 Hard Risk Rules Invariant`)**:
  - *RULE_01_IMPOSSIBLE_TRAVEL*: Phát hiện đăng nhập thành công từ 2 quốc gia/vùng địa lý khác nhau trong khoảng thời gian $< 15$ phút ($\text{Tốc dịch chuyển tính toán} > 900\text{ km/h}$).
  - *RULE_02_BRUTE_FORCE_BURST*: Phát hiện $\ge 5$ lần thử đăng nhập thất bại trong cửa sổ 60 giây $\to$ Kích hoạt Tự động Khóa Tạm thời 30 phút (`TEMPORARY_AUTO_LOCK` D-092) và cảnh báo.
  - *RULE_03_DEVICE_TOKEN_FLAPPING*: Phát hiện hành vi đăng ký / thu hồi thiết bị nhận PUSH lặp lại $\ge 3$ lần trong 5 phút.
  - *RULE_04_CONCURRENT_DISTRIBUTED_SESSIONS*: Đăng nhập đồng thời từ $\ge 5$ dải IP Subnet khác nhau tại cùng một thời điểm.
  - *RULE_05_HIGH_PRIVILEGE_PROMOTION_SPIKE*: Phát hiện có $\ge 2$ yêu cầu thăng cấp vai trò quản trị trong 1 giờ.
- **Tự động Yêu cầu Xác thực Bổ sung (`Step-up Auth Invariant`)**: Khi cờ Rủi ro Cao (`RiskScore >= 80%`) được bật, hệ thống TỰ ĐỘNG yêu cầu xác thực bổ sung qua OTP Email hoặc Hardware MFA trước khi cho phép thực hiện các thao tác quan trọng.
- **Phát Cảnh báo An ninh Tức thì SLA $\le 10\text{s}$ (`Realtime Security Alert SLA`)**: Ngay khi phát hiện rủi ro bất thường, hệ thống TỰ ĐỘNG phát cảnh báo Email và PUSH Notification qua M10 tới thiết bị người dùng trong thời gian $\le 10$ giây (REL-06).
- **Lưu vết Sổ Kiểm toán Bất biến M11 (`Anomalous Audit Trail`)**: $100\%$ các hành vi danh tính bất thường phát hiện được bắt buộc ghi vết bất biến `ACT-M11-39` trong CSDL, bao gồm `RiskRuleCode`, `CalculatedRiskScore`, `ClientIPHash` và `ActionTaken`.

## 2. Ma trận Quy tắc Rủi ro và Hành động Ứng phó (Risk Rule Matrix)

| Mã Quy tắc (`RuleCode`) | Tên Quy tắc Rủi ro | Điều kiện Kích hoạt | Mức Rủi ro | Hành động Tự động Ứng phó | SLA Cảnh báo |
|---|---|---|---|---|---|
| `RULE_01` | Impossible Travel | 2 IP khác quốc gia trong $< 15$m | **CAO (85%)** | Yêu cầu Step-up Auth OTP + Gửi Email | SLA $\le 10\text{s}$ |
| `RULE_02` | Brute Force Burst | 5 thất bại / 60s | **RẤT CAO (95%)** | Khóa tạm thời 30m + Gửi PUSH/Email | SLA $\le 5\text{s}$ |
| `RULE_03` | Device Flapping | 3 lần reg/unreg / 5m | **TRUNG BÌNH (60%)**| Hủy toàn bộ Push Token cũ + Log Audit | SLA $\le 10\text{s}$ |
| `RULE_04` | Distributed Sessions | 5 Subnet IPs / cùng lúc | **CAO (80%)** | Tăng SecurityEpoch $+1$ (Revoke All) | SLA $\le 5\text{s}$ |
| `RULE_05` | Role Spike | 2 Yêu cầu Admin / 1h | **RẤT CAO (90%)** | Chuyển sang Phê duyệt Kép M01-T030 | SLA $\le 10\text{s}$ |

## 3. Kiến trúc Luồng Phát hiện và Cảnh báo Bất thường (Anomalous Alert Engine)

```
[User Login / Identity Mutation Action]
                   |
                   v
   [Identity Risk Engine Evaluator]
   - Evaluate 5 Risk Rules
   - Calculate RiskScore [0-100%]
                   |
         +---------+---------+
         | (RiskScore < 60%) | (RiskScore >= 60%)
         v                   v
   [Allow Action]    [Trigger Automated Response & Alert]
                     - Require Step-up Auth / Revoke Sessions
                     - Publish IdentityAnomalyDetectedIntegrationEvent
                     - Send PUSH / Email via M10 SLA <= 10s
                     - Record Audit Log ACT-M11-39
```

## 4. Giao thức Thực thi Đánh giá Rủi ro CSDL (IdentityRiskEvaluationService)

```csharp
public async Task<RiskEvaluationResultDto> EvaluateIdentityActionRiskAsync(string userId, string clientIp, string userAgent, IdentityActionType actionType)
{
    int calculatedScore = 0;
    List<string> triggeredRules = new();

    // 1. Check RULE_01 Impossible Travel
    var lastLogin = await _db.UserLogins.Where(l => l.UserId == userId).OrderByDescending(l => l.TimestampUtc).FirstOrDefaultAsync();
    if (lastLogin != null && (DateTime.UtcNow - lastLogin.TimestampUtc).TotalMinutes < 15)
    {
        bool isImpossible = CheckGeoLocationDistance(lastLogin.IpAddress, clientIp, maxSpeedKmH: 900);
        if (isImpossible)
        {
            calculatedScore += 85;
            triggeredRules.Add("RULE_01_IMPOSSIBLE_TRAVEL");
        }
    }

    // 2. Check RULE_02 Brute Force Burst
    var recentFailures = await _cache.GetAsync<int>($"wordsoul:auth_fails:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}");
    if (recentFailures >= 5)
    {
        calculatedScore += 95;
        triggeredRules.Add("RULE_02_BRUTE_FORCE_BURST");
    }

    // 3. Execute Automated Action if High Risk
    if (calculatedScore >= 80)
    {
        await _eventPublisher.PublishAsync(new IdentityAnomalyDetectedIntegrationEvent {
            UserId = userId,
            RiskScore = calculatedScore,
            TriggeredRules = triggeredRules,
            DetectedAtUtc = DateTime.UtcNow
        });

        await _auditLog.RecordEventAsync("ACT-M11-39", "SYSTEM", new {
            UserId = userId,
            RiskScore = calculatedScore,
            Rules = string.Join(",", triggeredRules)
        });
    }

    return new RiskEvaluationResultDto { IsHighRisk = calculatedScore >= 80, RiskScore = calculatedScore, TriggeredRules = triggeredRules };
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AA-G01` | Hệ thống tự động đánh giá 5 quy tắc rủi ro hành vi danh tính đối với $100\%$ request đăng nhập. |
| `AA-G02` | Đăng nhập bất thường Impossible Travel (RULE_01) tự động kích hoạt Step-up Auth OTP Email. |
| `AA-G03` | Cảnh báo an ninh PUSH/Email gửi tới người dùng trong thời gian SLA $\le 10$ giây sau khi phát hiện. |
| `AA-G04` | Phát hiện 5 dải IP Subnet đồng thời (RULE_04) tự động tăng `SecurityEpoch` $+1$ ngắt toàn bộ phiên. |
| `AA-G05` | 100% các vụ việc danh tính bất thường được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-39`). |
| `AA-G06` | Phân quyền điều chỉnh ngưỡng điểm rủi ro chỉ dành riêng cho `SecurityAdmin` và `SuperAdmin`. |
| `AA-G07` | Đánh giá rủi ro danh tính thực thi song song, không làm tăng độ trễ đăng nhập quá $15\text{ms}$. |
| `AA-G08` | Cảnh báo bất thường tự động phát sự kiện `IdentityAnomalyDetectedIntegrationEvent` sang M10/M11. |
| `AA-G09` | Tọa độ địa lý IP tra cứu qua Local Cache, không được gọi API ngoài quá 3 giây Timeout. |
| `AA-G10` | 100% các test case tự kiểm AA39-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AA39-01` | Đăng nhập thành công tại Việt Nam, 10 phút sau đăng nhập thành công tại Mỹ | Phát hiện `RULE_01_IMPOSSIBLE_TRAVEL`, kích hoạt Step-up Auth |
| `AA39-02` | Thử sai mật khẩu 5 lần trong 45 giây cho User A | Phát hiện `RULE_02_BRUTE_FORCE_BURST`, khóa tài khoản 30m |
| `AA39-03` | Đăng ký và thu hồi Device PUSH 4 lần trong 3 phút | Phát hiện `RULE_03_DEVICE_TOKEN_FLAPPING`, hủy sạch token cũ |
| `AA39-04` | Tài khoản User B duy trì phiên chạy trên 6 IP khác Subnet | Phát hiện `RULE_04_CONCURRENT_DISTRIBUTED_SESSIONS`, tăng SecurityEpoch $+1$ |
| `AA39-05` | Gửi 2 yêu cầu thăng cấp vai trò Admin trong 30 phút | Phát hiện `RULE_05_HIGH_PRIVILEGE_PROMOTION_SPIKE`, yêu cầu Dual-Approval |
| `AA39-06` | Kiểm tra thời gian gửi Email/PUSH cảnh báo an ninh | Cảnh báo gửi tới người dùng trong SLA $< 7\text{s}$ |
| `AA39-07` | Tra cứu vết Audit Log M11 sau khi kích hoạt RULE_01 | Ghi nhận Audit Event `ACT-M11-39` đính kèm RiskScore 85% |
| `AA39-08` | User A nhập đúng mã OTP Step-up Auth gửi qua Email | Xác thực thành công, nâng cờ tin cậy thiết bị |
| `AA39-09` | User A nhập sai mã OTP Step-up Auth 3 lần | Hủy phiên đăng nhập, khóa tài khoản tạm thời |
| `AA39-10` | Tải đồng thời 500 request đánh giá rủi ro đăng nhập | Evaluation latency p95 $< 12\text{ms}$ |
| `AA39-11` | Đăng nhập bình thường từ thiết bị quen thuộc tại cùng 1 vị trí | RiskScore = 0%, cho phép truy cập bình thường |
| `AA39-12` | Quét sự kiện `IdentityAnomalyDetectedIntegrationEvent` phát ra | M10 nhận sự kiện và tiến hành gửi email tức thì |
| `AA39-13` | User không phải Admin xin thay đổi ngưỡng RiskScore | Deny 403 Forbidden |
| `AA39-14` | User chưa đăng nhập gọi API đánh giá rủi ro thủ công | Deny 401 Unauthorized |
| `AA39-15` | Tra cứu danh sách các địa chỉ IP bị gắn cờ rủi ro trong 24h | Trả về danh sách IP Salted Hash đã xử lý |
| `AA39-16` | Kiểm tra độ trễ tra cứu GeoIP từ bộ nhớ đệm Redis | Latency $< 2\text{ms}$ |
| `AA39-17` | Phân tích tham chiếu các quy tắc rủi ro trong CSDL | Quét schema `M01_RiskRules` (T020) |
| `AA39-18` | Dịch vụ GeoIP ngoài bị sập ngắt kết nối | Fallback bỏ qua check Impossible Travel không treo đăng nhập |
| `AA39-19` | Khôi phục điểm RiskScore của người dùng về 0% sau 24h không có lỗi | Tự động reset điểm rủi ro về bình thường |
| `AA39-20` | Kiểm thử hoàn tất luồng cảnh báo hành vi bất thường M01-ANOMALOUS-IDENTITY-ALERT-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M01-AA-I01` | Chưa có bộ đánh giá `IdentityRiskEvaluationService` trong codebase M01 | Đăng nhập bất thường chưa được cảnh báo tự động | M01-T049 (Source task) |
| `M01-AA-I02` | CSDL chưa lưu lịch sử `UserLogins` kèm IP/GeoLocation | Không có dữ liệu tính toán Impossible Travel (RULE_01) | M01-T049; M01-T003 |
| `M01-AA-I03` | Thiếu luồng yêu cầu xác thực bổ sung `Step-up Auth` khi có rủi ro cao | Người dùng vẫn vào được hệ thống khi bị lộ password | M01-T049; M01-T010 |
| `M01-AA-I04` | Thiếu sự kiện `IdentityAnomalyDetectedIntegrationEvent` phát sang M10 | Bộ phận gửi tin nhắn M10 không biết để gửi email cảnh báo | M01-T049; M10 tasks |
| `M01-AA-I05` | Chưa có bộ nhớ đệm GeoIP Cache trong Redis | Gọi API ngoài GeoIP trực tiếp làm tăng latency đăng nhập | M01-T049; M12-T032 |

- `M01-AA-F01`: Triển khai `IdentityRiskEvaluationService` đánh giá 5 quy tắc rủi ro (tiếp nhận: M01-T049).
- `M01-AA-F02`: Khởi tạo bảng `UserLogins` lưu lịch sử vị trí đăng nhập (tiếp nhận: M01-T049; M01-T003).
- `M01-AA-F03`: Tích hợp luồng `Step-up Auth` và cảnh báo M10 SLA $\le 10\text{s}$ (tiếp nhận: M01-T049; REL-06).
- `M01-AA-F04`: Thiết lập bộ kiểm thử tự động AA-G01–G10 và AA39-01–20 (tiếp nhận: M01 tasks).
- `M01-AA-F05`: Thu thập bằng chứng runtime cho luồng cảnh báo danh tính M01 (tiếp nhận: M01 tasks; A-G01/A-G06).

## 8. Tự kiểm M01-T039

- Đã thiết kế hoàn chỉnh `M01-ANOMALOUS-IDENTITY-ALERT-1.0` với 5 Quy tắc Nhận diện Rủi ro Danh tính Cứng.
- Đã chốt Ràng buộc Tự động Yêu cầu Xác thực Bổ sung (`Step-up Auth`).
- Đã chốt Giao thức Phát Cảnh báo An ninh PUSH/Email qua M10 SLA $\le 10\text{s}$ (REL-06).
- Đã lồng ghép Tự động Khóa Tạm thời 30m, Revoke Session và Lưu vết Audit Log M11 (`ACT-M11-39`).
- Đã xác lập 10 Regression Gates (`AA-G01`–`AA-G10`) và 20 Test Cases tự kiểm (`AA39-01`–`AA39-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế cảnh báo hành vi danh tính bất thường M01-T039 | WSA-7K2 |
