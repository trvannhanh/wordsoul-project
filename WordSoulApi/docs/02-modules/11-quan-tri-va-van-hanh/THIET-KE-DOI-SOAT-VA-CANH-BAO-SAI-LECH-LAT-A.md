# Thiết kế đối soát và cảnh báo sai lệch — lát A M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-RECONCILIATION-DISCREPANCY-ALERT-A-1.0` |
| Task | M11-T040-A |
| Đầu vào | M11-BACKGROUND-JOB-EXECUTION-RECOVERY-1.0 (D-136), REL-07 |
| Phạm vi | Đặc tả Giao thức Đối soát Dữ liệu và Cảnh báo Sai lệch Lát A (`Data Reconciliation & Discrepancy Alerting Protocol - Slice A`), quy trình worker `DataReconciliationWorker` quét đếm bộ đếm CSDL vs thực tế, chính sách tự động khôi phục Auto-Healing $<1\%$ và lưu vết kiểm toán M11 |
| Tự kiểm | A-G06; REL-07 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Đối soát Dữ liệu và Cảnh báo Sai lệch Lát A (`Data Reconciliation & Discrepancy Alerting Protocol - Slice A`) thuộc M11, chuẩn hóa cơ chế quét đối soát tự động hàng ngày giữa các biến bộ đếm CSDL (Counters) và số lượng bản ghi thực tế trong CSDL SQL, phát hiện và tự động sửa chữa các sai lệch nhỏ hoặc phát cảnh báo tới đội ngũ vận hành khi có bất thường lớn (REL-07).

- **Phạm vi Đối soát Lát A 3 Nhóm Dữ liệu Key (`Slice A 3 Target Reconciliation Categories`)**:
  - *Nhóm 1 (Push Devices)*: Đối soát biến đếm số thiết bị PUSH active `MaxActivePushDevices` vs tổng số bản ghi active thực tế trong `M01_UserPushDevices`.
  - *Nhóm 2 (Asset References)*: Đối soát biến `ActiveRefCount` của tài sản media M12 vs số lượng liên kết thực tế ở các mục từ M02.
  - *Nhóm 3 (Learning Cards)*: Đối soát biến `TotalCardsCount` bộ từ M02 vs số bản ghi mục từ active thuộc bộ từ đó.
- **Chính sách Tự động Sửa chữa Auto-Healing $< 1\%$ (`Auto-Healing Policy for Minor Discrepancy`)**: Nếu tỷ lệ sai lệch bộ đếm $< 1\%$, hệ thống TỰ ĐỘNG thực hiện tính toán lại (Recount & Repair) và cập nhật giá trị đúng vào CSDL (`AUTO_HEALED`), đồng thời ghi vết log WARN.
- **Chính sách Phát Cảnh báo Sự cố $\ge 1\%$ (`Alerting Policy for Major Discrepancy`)**: Nếu tỷ lệ sai lệch $\ge 1\%$, hệ thống CẤM tự động ghi đè, LẬP TỨC phát cảnh báo an toàn `P2_HIGH` (D-132) sang Slack `#alerts-warning` yêu cầu Kỹ sư CSDL đối soát thủ công (REL-07).
- **Lưu vết Sổ Kiểm toán Đối soát M11 (`Reconciliation Audit Trail`)**: $100\%$ các đợt chạy đối soát, bao gồm các bản ghi tự động sửa chữa hay phát cảnh báo sai lệch, được ghi vết bất biến `ACT-M11-40-RECON` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Đối soát Dữ liệu (Reconciliation Matrix)

| Tỷ lệ Sai lệch (`DiscrepancyRate`) | Phân loại Sai lệch | Hành vi Tự động (`Auto Action`) | Kênh Cảnh báo M11 | Trạng thái Đợt Đối soát | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| $0\%$ (Không sai lệch) | `CONSISTENT` | Không thao tác | N/A (Bình thường) | `PASSED` | `ACT-M11-40-PASSED` |
| **$< 1\%$** | `MINOR_DISCREPANCY` | **Tự động Cập nhật Giá trị Đúng (Auto-Heal)** | Slack Log `#alerts-digest` | `AUTO_HEALED` | `ACT-M11-40-HEALED` |
| **$\ge 1\%$** | `MAJOR_DISCREPANCY` | **CẤM Tự động Sửa (Block Auto-Fix)** | **Alert `P2_HIGH` Slack `#alerts-warning`** | `REQUIRES_MANUAL_FIX` | `ACT-M11-40-MAJOR` |

## 3. Kiến trúc Luồng Đối soát Dữ liệu Lát A M11 (Reconciliation Engine Pipeline)

```
[Daily DataReconciliationWorker Triggers (03:00 UTC)]
                          |
                          v
    [Execute Aggregate SQL Queries for Target 3 Categories]
                          |
                          v
    [Calculate Discrepancy Rate: |Counter - ActualCount| / ActualCount]
                          |
        +-----------------+-----------------+
        | (Rate == 0)                       | (Rate > 0)
        v                                   v
[Set State: PASSED]        +----------------+----------------+
                           | (Rate < 1%)                     | (Rate >= 1%)
                           v                                 v
                  [Auto-Heal Counter DB]            [BLOCK Auto-Fix]
                  [Set State: AUTO_HEALED]          [Trigger Alert P2_HIGH (D-132)]
                  [Record Audit Log ACT-M11-40]     [Set State: REQUIRES_MANUAL_FIX]
                                                    [Record Audit Log ACT-M11-40-MAJOR]
```

## 4. Giao thức Thực thi Worker Đối soát CSDL (DataReconciliationService)

```csharp
public async Task<ReconciliationBatchResultDto> RunDataReconciliationAsync(string actorUserId)
{
    var result = new ReconciliationBatchResultDto { BatchId = Guid.NewGuid().ToString("N"), ExecutedAtUtc = DateTime.UtcNow };

    // Category 1: Asset Reference Count Reconciliation M12
    var assetDiscrepancies = await _db.AssetMetadatas
        .Select(a => new {
            a.AssetId,
            a.ActiveRefCount,
            ActualCount = _db.Headwords.Count(h => h.AudioAssetId == a.AssetId || h.ImageAssetId == a.AssetId)
        })
        .Where(x => x.ActiveRefCount != x.ActualCount)
        .ToListAsync();

    foreach (var item in assetDiscrepancies)
    {
        double diffRate = Math.Abs(item.ActiveRefCount - item.ActualCount) / (double)Math.Max(1, item.ActualCount);
        if (diffRate < 0.01) // < 1% Auto-Heal
        {
            var asset = await _db.AssetMetadatas.FindAsync(item.AssetId);
            asset.ActiveRefCount = item.ActualCount;
            await _db.SaveChangesAsync();

            await _auditLog.RecordEventAsync("ACT-M11-40-HEALED", actorUserId, new {
                Target = "AssetActiveRefCount",
                AssetId = item.AssetId,
                OldVal = item.ActiveRefCount,
                NewVal = item.ActualCount
            });
        }
        else // >= 1% Major Discrepancy Alert P2_HIGH
        {
            await _alertService.DispatchIncidentAlertAsync("M12_ASSET_COUNTER", SeverityLevel.P2_HIGH, 
                $"Major discrepancy in Asset {item.AssetId}: DB Counter = {item.ActiveRefCount}, Actual = {item.ActualCount} ({diffRate:P2})");

            await _auditLog.RecordEventAsync("ACT-M11-40-MAJOR", actorUserId, new {
                Target = "AssetActiveRefCount",
                AssetId = item.AssetId,
                DiscrepancyRate = diffRate
            });
        }
    }

    return result;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RC-G01` | Worker đối soát BẮT BUỘC tự động quét 3 nhóm dữ liệu đối soát chính Lát A hàng ngày lúc 03:00 UTC. |
| `RC-G02` | Tỷ lệ sai lệch bộ đếm $< 1\%$ BẮT BUỘC tự động sửa chữa (`AUTO_HEALED`) và lưu vết log. |
| `RC-G03` | Tỷ lệ sai lệch bộ đếm $\ge 1\%$ TUYỆT ĐỐI CẤM tự động sửa, phải phát cảnh báo `P2_HIGH` (D-132, REL-07). |
| `RC-G04` | 100% các đợt đối soát hoàn tất phải tổng hợp báo cáo tỷ lệ khớp dữ liệu chung (Overall Consistency Rate). |
| `RC-G05` | Phục hồi đối soát thủ công từ Admin BẮT BUỘC xác thực lại mật khẩu local trong vòng 5 phút (`ReAuthMinutes <= 5m`). |
| `RC-G06` | 100% các thao tác đối soát và Auto-Heal được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-40-RECON`). |
| `RC-G07` | SLA thực thi đợt quét đối soát 100,000 bản ghi $< 45$ giây. |
| `RC-G08` | Phân quyền kích hoạt chạy đối soát thủ công chỉ dành cho `DatabaseAdmin` và `SystemAdmin`. |
| `RC-G09` | Tiến trình đối soát chạy với Isolation Level `READ COMMITTED` không làm khóa bảng CSDL production. |
| `RC-G10` | 100% các test case tự kiểm RC40-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC40-01` | Quét đối soát `ActiveRefCount` tài sản thấy biến đếm = 10, thực tế = 10 | Đánh dấu `PASSED`, không thao tác sửa CSDL |
| `RC40-02` | Quét đối soát `ActiveRefCount` thấy biến đếm = 100, thực tế = 100 (Sai lệch 0.99% $< 1\%$) | Tự động sửa biến đếm về 100 (`AUTO_HEALED`), ghi vết Audit Log |
| `RC40-03` | Quét đối soát `ActiveRefCount` thấy biến đếm = 100, thực tế = 85 (Sai lệch 15% $\ge 1\%$) | Chặn Auto-Fix, phát cảnh báo P2_HIGH sang Slack, ghi Audit Log |
| `RC40-04` | Quét đối soát `MaxActivePushDevices` M01 thấy sai lệch 0.5% | Tự động sửa biến đếm thiết bị PUSH active về giá trị đúng |
| `RC40-05` | Admin xác thực lại 2 phút trước bấm nút "Chạy Đối soát Thủ công" | Khởi chạy đợt đối soát thành công, trả về Report DTO |
| `RC40-06` | Admin thử bấm nút "Chạy Đối soát Thủ công" khi lần Re-Auth cuối là 9 phút trước ($> 5\text{m}$) | Reject 401 `REAUTH_REQUIRED` |
| `RC40-07` | Tra cứu vết Audit Log M11 sau khi Auto-Heal sai lệch nhỏ | Ghi nhận Audit Event `ACT-M11-40-HEALED` đính kèm Old/NewVal |
| `RC40-08` | Tra cứu vết Audit Log M11 sau khi phát hiện sai lệch lớn $\ge 1\%$ | Ghi nhận Audit Event `ACT-M11-40-MAJOR` |
| `RC40-09` | Tải quét đối soát trên CSDL 500,000 bản ghi mục từ M02 | Quét hoàn tất trong SLA $< 32\text{s}$ |
| `RC40-10` | Cập nhật cấu hình ngưỡng Auto-Heal từ 1% xuống 0.5% | Cấu hình mới áp dụng chính xác cho các đợt đối soát sau |
| `RC40-11` | Thử truyền danh mục đối soát không hợp lệ | Reject 400 `INVALID_RECONCILIATION_CATEGORY` |
| `RC40-12` | Gửi request chạy đối soát khi JWT Access Token đã hết hạn | Deny 401 Unauthorized |
| `RC40-13` | User không phải DatabaseAdmin thử gọi API chạy đối soát | Deny 403 Forbidden |
| `RC40-14` | User chưa đăng nhập gọi API tra cứu báo cáo đối soát M11 | Deny 401 Unauthorized |
| `RC40-15` | Chạy đối soát nhóm 3 (Learning Cards M04) phát hiện sai lệch 0.8% | Tự động sửa `TotalCardsCount` về giá trị thực tế |
| `RC40-16` | Kiểm tra thời gian phát cảnh báo P2_HIGH khi sai lệch lớn | Dispatch SLA $< 5\text{s}$ |
| `RC40-17` | Phân tích tham chiếu các bản ghi `ReconciliationBatches` trong CSDL | Quét schema `M11_ReconciliationBatches` (T020) |
| `RC40-18` | CSDL bị ngắt kết nối giữa đợt quét đối soát | Cancel đợt quét, rollback transaction, ghi log ERROR |
| `RC40-19` | Tra cứu danh sách các sai lệch lớn chưa được xử lý thủ công | Trả về DTO danh sách Discrepancies có cờ Major |
| `RC40-20` | Kiểm thử hoàn tất luồng đối soát dữ liệu Lát A M11-RECONCILIATION-DISCREPANCY-ALERT-A-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-RC-I01` | M11 hiện tại chưa có `DataReconciliationWorker` đối soát tự động | Risk sai lệch bộ đếm CSDL tích tụ theo thời gian | M11-T049 (Source task) |
| `M11-RC-I02` | Thiếu cờ Auto-Healing $< 1\%$ tự động khôi phục bộ đếm | Bắt Kỹ sư CSDL phải sửa tay các sai lệch vô hại nhỏ | M11-T049; REL-07 |
| `M11-RC-I03` | Thiếu cờ Chặn Auto-Fix & Phát Alert `P2_HIGH` khi sai lệch $\ge 1\%$ | Risk tự động sửa nhầm khi CSDL gặp sự cố lớn | M11-T049; M11-T037 |
| `M11-RC-I04` | Thiếu Re-Auth Guard $\le 5\text{m}$ khi kích hoạt chạy đối soát thủ công | Người mượn máy có thể tự ý chạy đối soát gây tải DB | M11-RC-F04; REL-01 |
| `M11-RC-I05` | Chưa kết nối sự kiện đối soát với Audit Log M11 (`ACT-M11-40-RECON`) | Không ghi vết được lịch sử Auto-Heal bộ đếm | M11-T049; M11-T031 |

- `M11-RC-F01`: Triển khai `DataReconciliationWorker` quét 3 nhóm Lát A (tiếp nhận: M11-T049).
- `M11-RC-F02`: Tích hợp Bắt buộc Auto-Healing $<1\%$ & Block Auto-Fix $\ge 1\%$ (tiếp nhận: M11-T049; REL-07).
- `M11-RC-F03`: Triển khai Discrepancy Alert P2_HIGH & Re-Auth Guard $\le 5\text{m}$ (tiếp nhận: M11-T049; M11-T037).
- `M11-RC-F04`: Thiết lập bộ kiểm thử tự động RC-G01–G10 và RC40-01–20 (tiếp nhận: M11 tasks).
- `M11-RC-F05`: Thu thập bằng chứng runtime cho luồng đối soát M11 (tiếp nhận: M11 tasks; A-G06).

## 8. Tự kiểm M11-T040-A

- Đã thiết kế hoàn chỉnh `M11-RECONCILIATION-DISCREPANCY-ALERT-A-1.0` với Ma trận Quy trình Đối soát Dữ liệu.
- Đã chốt Ràng buộc Phạm vi Đối soát Lát A 3 Nhóm Dữ liệu Key (Push Devices, Asset References, Learning Cards).
- Đã chốt Ràng buộc Chính sách Tự động Sửa chữa Auto-Healing $< 1\%$.
- Đã lồng ghép Chính sách Phát Cảnh báo Sự cố $\ge 1\%$ (`P2_HIGH`), Re-Auth Guard $\le 5\text{m}$ và Audit Log M11 (`ACT-M11-40-RECON`).
- Đã xác lập 10 Regression Gates (`RC-G01`–`RC-G10`) và 20 Test Cases tự kiểm (`RC40-01`–`RC40-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả đối soát dữ liệu Lát A M11-T040-A | WSA-7K2 |
