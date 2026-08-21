# Thiết kế đo usage, chi phí và ngân sách M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-USAGE-COST-BUDGET-TRACKING-1.0` |
| Task | M12-T046 |
| Đầu vào | M12-CAPABILITY-SLO-HEALTH-DEFINITIONS-1.0 (D-142), M12-RATE-LIMITING-1.0 (D-024), REL-03 |
| Phạm vi | Đặc tả Giao thức Đo lường Mức dùng, Chi phí và Ngân sách Tích hợp (`Integration Usage, Cost & Budget Tracking Protocol`), đo lường Token AI Gemini / S3 Storage / FCM Push, hạn ngạch ngân sách tháng $500, cơ chế tự động ngắt khi vượt budget $100\%$ và lưu vết kiểm toán M12 |
| Tự kiểm | A-G04, A-G06; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đo lường Mức dùng, Chi phí và Ngân sách Tích hợp (`Integration Usage, Cost & Budget Tracking Protocol`) thuộc M12, xác lập khả năng đo lường chính xác dung lượng tiêu thụ tài nguyên của từng đối tác bên ngoài (Tokens Gemini AI, Dung lượng lưu trữ S3, Số lượng tin nhắn Push FCM), khống chế ngân sách hàng tháng để tránh chi phí tăng bùng nổ ngoài vượt kiểm soát (REL-03).

- **Đo lường Chi tiết Lưu lượng Tiêu thụ Tài nguyên (`Provider Usage Metering Invariant`)**: 100% các cuộc gọi tích hợp bên ngoài BẮT BUỘC ghi vết lượng tiêu thụ:
  - *Gemini AI*: Số lượng `InputTokens` và `OutputTokens` per request.
  - *S3 / CDN Storage*: Số lượng `UploadedBytes` và `BandwidthBytes` per request.
  - *Firebase FCM*: Số lượng `PushDispatchedCount` per request.
- **Hạn ngạch Ngân sách Tháng và Ngưỡng Cảnh báo (`Monthly Budget Cap & Alert Thresholds`)**:
  - *Gemini AI Monthly Budget Cap*: Tối đa $\$500.00$ / tháng. Khi đạt $80\%$ ($\$400.00$), tự động phát cảnh báo WARN sang Slack `#billing-alerts`. Khi đạt $100\%$ ($\$500.00$), hệ thống TỰ ĐỘNG kích hoạt `KILL_AI_GEMINI` (D-139) chuyển sang từ điển tĩnh fallback (REL-03).
  - *S3 Storage Monthly Cap*: Tối đa $\$200.00$ / tháng.
  - *Firebase Push Monthly Cap*: Tối đa $\$100.00$ / tháng.
- **Giới hạn Hạn ngạch Người dùng Hàng ngày SLA $\le 1\text{ms}$ (`Per-User Daily Quota Guard`)**: Mỗi người học được cấp hạn ngạch tối đa `UserDailyTokenQuota = 50,000 tokens/ngày` cho tính năng AI Gemini. Khi vượt hạn ngạch, API Gateway từ chối cuộc gọi với lỗi HTTP 429 `DAILY_USAGE_QUOTA_EXCEEDED` trong SLA $\le 1\text{ms}$ (D-024).
- **Lưu vết Sổ Kiểm toán Ngân sách M12 (`Integration Budget Audit Trail`)**: $100\%$ các đợt chạm ngưỡng $80\%$ cảnh báo hoặc vượt $100\%$ ngân sách tháng được ghi vết bất biến `ACT-M12-46-BUDGET` trong Sổ Kiểm toán M11.

## 2. Ma trận Ngân sách và Hạn ngạch Tích hợp (Budget & Quota Matrix)

| Dịch vụ Đối tác (`ProviderService`) | Đơn vị Đo lường (`Metric Unit`) | Hạn ngạch Người dùng (`Daily User Quota`) | Ngân sách Tháng (`Monthly Budget Cap`) | Cảnh báo 80% (`Alert 80% Threshold`) | Ngắt 100% (`Auto-Kill 100% Threshold`) | Vết Kiểm toán M11 |
|---|---|---|---|---|---|---|
| **`GOOGLE_GEMINI_AI`** | Tokens (In/Out) | **50,000 Tokens/Ngày** | **$\$500.00$ / Tháng** | **$\$400.00$ (Slack Alert)** | **Auto-Kill sang Static Cache** | `ACT-M12-46-GEMINI` |
| `S3_CDN_STORAGE` | Storage Bytes / Bandwidth | 100MB Upload/Ngày | $\$200.00$ / Tháng | $\$160.00$ (Slack Alert) | Chặn Direct Upload URL | `ACT-M12-46-S3` |
| `FIREBASE_PUSH` | Push Messages Count | 50 Messages/Ngày | $\$100.00$ / Tháng | $\$80.00$ (Slack Alert) | In-App Banner Only | `ACT-M12-46-PUSH` |

## 3. Kiến trúc Luồng Đo lường và Khống chế Ngân sách M12 (Budget Engine Pipeline)

```
[Integration Call Completed (Gemini / S3 / Push)]
                        |
                        v
 [IntegrationUsageMeter: Meter Tokens / Bytes / Message Count]
                        |
                        v
 [Atomic Increment Redis Tenant & Monthly Cost Counter (SLA <= 1ms)]
                        |
        +---------------+---------------+
        | (Cost < 80% Cap)              | (Cost >= 80% Cap)
        v                               v
[Normal Operation]      +---------------+---------------+
                        | (Cost 80% - 99%)              | (Cost >= 100% Cap)
                        v                               v
               [Send Slack #billing-alerts]     [Enable Feature Kill Switch (D-139)]
               [Record Audit ACT-M12-46]        [Switch to Static Fallback Cache]
                                                [Record Audit Log ACT-M12-46-KILL]
```

## 4. Giao thức Thực thi Quản lý Ngân sách CSDL (IntegrationUsageCostService)

```csharp
public async Task RecordProviderUsageAsync(string providerKey, string userId, int tokensUsed, double estimatedCostUsd)
{
    var db = _redis.GetDatabase();
    string currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
    string monthCostKey = $"wordsoul:cost:monthly:{currentMonth}:{providerKey}";
    string userDailyKey = $"wordsoul:usage:daily:{DateTime.UtcNow:yyyy-MM-dd}:{userId}";

    // 1. Atomic Increment User Daily Usage SLA <= 1ms
    long userTotalTokens = await db.StringIncrementAsync(userDailyKey, tokensUsed);
    if (userTotalTokens == tokensUsed) await db.KeyExpireAsync(userDailyKey, TimeSpan.FromDays(2));

    if (userTotalTokens > 50000) // 50,000 Daily User Limit
    {
        throw new InvalidOperationException("DAILY_USAGE_QUOTA_EXCEEDED: Bạn đã vượt quá hạn ngạch sử dụng AI trong ngày.");
    }

    // 2. Atomic Increment Provider Monthly Cost
    double totalMonthCost = await db.HashIncrementAsync(monthCostKey, "CostUsd", estimatedCostUsd);

    // 3. Check Monthly Budget Cap ($500.00)
    double budgetCap = 500.00;
    if (totalMonthCost >= budgetCap)
    {
        // 100% Budget Reached: Enable Kill Switch (D-139)
        await _killSwitchService.SetFeatureKillSwitchAsync("KILL_AI_GEMINI", true, "SYSTEM_BUDGET_CAP_EXCEEDED");
        
        await _auditLog.RecordEventAsync("ACT-M12-46-KILL", "BUDGET_SYSTEM", new {
            ProviderKey = providerKey,
            TotalMonthCost = totalMonthCost,
            Action = "KILL_SWITCH_ENABLED_BUDGET_100_PERCENT"
        });
    }
    else if (totalMonthCost >= budgetCap * 0.8 && !await db.KeyExistsAsync($"{monthCostKey}:alerted_80"))
    {
        // 80% Budget Reached: Trigger Slack Warning Alert
        await db.StringSetAsync($"{monthCostKey}:alerted_80", "1", TimeSpan.FromDays(30));
        await _alertService.SendBillingAlertAsync(providerKey, totalMonthCost, budgetCap);

        await _auditLog.RecordEventAsync("ACT-M12-46-BUDGET", "BUDGET_SYSTEM", new {
            ProviderKey = providerKey,
            TotalMonthCost = totalMonthCost,
            Action = "BILLING_WARN_80_PERCENT_REACHED"
        });
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `UC-G01` | 100% các cuộc gọi tích hợp bên ngoài BẮT BUỘC đo lường lượng tài nguyên tiêu thụ (Tokens/Bytes/Messages). |
| `UC-G02` | Hạn ngạch người dùng hàng ngày Gemini AI BẮT BUỘC giới hạn $\le 50,000$ Tokens/ngày per user. |
| `UC-G03` | Chi phí tháng Gemini AI BẮT BUỘC khống chế mốc tối đa $\$500.00$ / tháng. |
| `UC-G04` | Khi chi phí đạt mốc $80\%$ ($\$400.00$), hệ thống BẮT BUỘC phát tin cảnh báo WARN sang Slack `#billing-alerts`. |
| `UC-G05` | Khi chi phí đạt mốc $100\%$ ($\$500.00$), hệ thống BẮT BUỘC bật `KILL_AI_GEMINI` chuyển từ điển tĩnh fallback (REL-03). |
| `UC-G06` | 100% các lần phát cảnh báo 80% hoặc ngắt 100% ngân sách được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M12-46-BUDGET`). |
| `UC-G07` | SLA thực thi ghi nhận đo lường lưu lượng và cộng dồn chi phí Redis $< 1.0\text{ms}$ per call. |
| `UC-G08` | Phân quyền điều chỉnh hạn ngạch ngân sách tháng chỉ dành riêng cho `FinanceAdmin` và `SystemAdmin`. |
| `UC-G09` | Hệ thống hỗ trợ xử lý đo lường đồng thời 5,000 API requests/giây mà không bị ngắt quãng luồng chính. |
| `UC-G10` | 100% các test case tự kiểm UC46-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `UC46-01` | Người học dùng 4,000 Tokens AI Gemini trong ngày ($< 50,000$) | Ghi nhận token usage thành công, 200 OK |
| `UC46-02` | Người học dùng đến mốc 50,001 Tokens AI Gemini trong ngày ($> 50,000$) | Reject HTTP 429 `DAILY_USAGE_QUOTA_EXCEEDED` SLA $< 1\text{ms}$ |
| `UC46-03` | Tổng chi phí Gemini AI tháng đạt mốc $\$400.00$ ($80\%$ budget cap) | Phát cảnh báo WARN sang Slack `#billing-alerts`, ghi vết Audit |
| `UC46-04` | Tổng chi phí Gemini AI tháng đạt mốc $\$500.00$ ($100\%$ budget cap) | Tự động kích hoạt `KILL_AI_GEMINI` (D-139), chuyển static cache |
| `UC46-05` | Cảnh báo 80% ngân sách chỉ phát DUY NHẤT 1 lần trong tháng | Redis de-duplicate key khóa thông báo lặp |
| `UC46-06` | Tra cứu vết Audit Log M11 sau khi bật Kill Switch do chạm $100\%$ budget | Ghi nhận Audit Event `ACT-M12-46-KILL` |
| `UC46-07` | Sang tháng mới (Ngày 1 hàng tháng), bộ đếm chi phí tháng tự động reset | Khôi phục cờ `KILL_AI_GEMINI` về false, mở lại dịch vụ AI |
| `UC46-08` | S3 Storage Upload đạt chi phí $\$160.00$ / tháng ($80\%$ budget cap) | Phát cảnh báo WARN sang Slack `#billing-alerts` |
| `UC46-09` | Tải đồng thời 1,000 request đo lường lưu lượng token vào Redis | Processing latency p95 $< 0.8\text{ms}$ per call |
| `UC46-10` | FinanceAdmin cập nhật ngân sách tháng Gemini từ $\$500$ lên $\$800$ | Cập nhật cấu hình thành công, tắt Kill Switch nếu cost $< \$800$ |
| `UC46-11` | Thử nạp tham số ngân sách tháng âm ($< \$0$) | Reject 400 `INVALID_BUDGET_AMOUNT` |
| `UC46-12` | Gửi request điều chỉnh ngân sách khi JWT Access Token hết hạn | Deny 401 Unauthorized |
| `UC46-13` | User không phải FinanceAdmin thử điều chỉnh ngân sách Gemini AI | Deny 403 Forbidden |
| `UC46-14` | User chưa đăng nhập gọi API tra cứu báo cáo chi phí M12 | Deny 401 Unauthorized |
| `UC46-15` | Người học hết hạn ngạch ngày thử gọi API AI Gemini | Từ chối 429 kèm thông điệp khôi phục lúc 00:00 UTC |
| `UC46-16` | Kiểm tra thời gian kích hoạt Kill Switch khi chạm 100% budget | Activation SLA $< 100\text{ms}$ |
| `UC46-17` | Phân tích tham chiếu các bản ghi `IntegrationProviderUsages` trong CSDL | Quét schema `M12_ProviderUsages` (T020) |
| `UC46-18` | Dịch vụ Redis bị gián đoạn khi ghi nhận đo lường token | Fallback nạp metric vào đệm bộ nhớ local memory |
| `UC46-19` | Tra cứu bảng tổng hợp chi phí dự kiến tháng của 4 dịch vụ đối tác | Trả về DTO danh sách CostBreakdown |
| `UC46-20` | Kiểm thử hoàn tất luồng đo usage, chi phí M12-USAGE-COST-BUDGET-TRACKING-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-UC-I01` | M12 hiện tại chưa có `IntegrationUsageCostService` đo lường chi phí | Risk chi phí API Gemini tăng vọt không kiểm soát | M12-T049 (Source task) |
| `M12-UC-I02` | Thiếu cờ Ngân sách Tháng $\$500.00$ & Auto-Kill 100% Budget | Không bảo vệ được quỹ vận hành của dự án | M12-T049; REL-03 |
| `M12-UC-I03` | Thiếu cờ Hạn ngạch Người dùng Hàng ngày 50,000 Tokens/ngày | Kẻ xấu có thể dùng tool spam làm cạn ngân sách Gemini | M12-T049; M12-T034 |
| `M12-UC-I04` | Thiếu luồng phát tin Slack Alert 80% de-duplication | Phát tràn lan thông báo gây nhiễu cho đội tài chính | M12-UC-F04; M11-T037 |
| `M12-UC-I05` | Chưa kết nối sự kiện khống chế ngân sách với Audit Log M11 (`ACT-M12-46-BUDGET`) | Không ghi vết được thời điểm dịch vụ bị ngắt do hết tiền | M12-T049; M11-T031 |

- `M12-UC-F01`: Triển khai `IntegrationUsageCostService` với Metering Engine (tiếp nhận: M12-T049).
- `M12-UC-F02`: Tích hợp Bắt buộc Monthly Budget Cap $\$500$ & Auto-Kill 100% (tiếp nhận: M12-T049; REL-03).
- `M12-UC-F03`: Triển khai Per-User Daily Quota 50k Tokens SLA $\le 1\text{ms}$ (tiếp nhận: M12-T049; M12-T034).
- `M12-UC-F04`: Thiết lập bộ kiểm thử tự động UC-G01–G10 và UC46-01–20 (tiếp nhận: M12 tasks).
- `M12-UC-F05`: Thu thập bằng chứng runtime cho luồng ngân sách M12 (tiếp nhận: M12 tasks; A-G04/A-G06).

## 8. Tự kiểm M12-T046

- Đã thiết kế hoàn chỉnh `M12-USAGE-COST-BUDGET-TRACKING-1.0` với Ma trận Ngân sách và Hạn ngạch Tích hợp.
- Đã chốt Ràng buộc Đo lường Chi tiết Lưu lượng Tiêu thụ Tài nguyên (Tokens, Storage Bytes, Push Messages).
- Đã chốt Ràng buộc Hạn ngạch Ngân sách Tháng và Ngưỡng Cảnh báo (Gemini $\$500$, Alert $80\%$, Auto-Kill $100\%$ REL-03).
- Đã lồng ghép Giới hạn Hạn ngạch Người dùng Hàng ngày SLA $\le 1\text{ms}$ (50,000 Tokens/ngày) và Audit Log M11 (`ACT-M12-46-BUDGET`).
- Đã xác lập 10 Regression Gates (`UC-G01`–`UC-G10`) và 20 Test Cases tự kiểm (`UC46-01`–`UC46-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế đo usage, chi phí và ngân sách M12-T046 | WSA-7K2 |
