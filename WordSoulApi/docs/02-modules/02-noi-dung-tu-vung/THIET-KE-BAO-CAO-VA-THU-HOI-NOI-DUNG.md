# Thiết kế báo cáo và thu hồi nội dung M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-REPORT-EMERGENCY-RECALL-1.0` |
| Task | M02-T033 |
| Đầu vào | M02-DEPRECATION-REPLACEMENT-1.0 (D-065), M02-SET-VERSIONED-PUBLISHING-1.0 (D-086), M11-CONTENT-LIFECYCLE-1.0 (D-051), REL-04 |
| Phạm vi | Đặc tả Giao thức Tiếp nhận Báo cáo Vi phạm và Thu hồi Nội dung Khẩn cấp (`Report & Emergency Content Recall Protocol`), 4 danh mục báo cáo, cờ tự động tạm ẩn khi đạt ngưỡng 5 báo cáo, SLA thu hồi khẩn cấp $\le 60$ giây và cơ chế cách ly phiên học M03 |
| Tự kiểm | A-G03, A-G06; REL-04 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Báo cáo và Thu hồi Nội dung Khẩn cấp (`Report & Emergency Content Recall Protocol`) thuộc M02, cho phép người học báo cáo các vi phạm bản quyền REL-04, lỗi sư phạm hoặc nội dung độc hại, đồng thời cung cấp giao thức thu hồi khẩn cấp cho Quản trị viên (ContentAdmin / SecurityAdmin) để gỡ bỏ tức thì các bộ từ / mục từ vi phạm khỏi toàn hệ thống mà không làm sập ứng dụng (REL-04, CT-01).

- **4 Danh mục Báo cáo Vi phạm Chuẩn hóa (`4 Violation Report Categories`)**:
  - `COPYRIGHT_VIOLATION_REL04`: Báo cáo vi phạm bản quyền hình ảnh, âm thanh hoặc tài sản trí tuệ (Ưu tiên cao nhất).
  - `PEDAGOGICAL_ERROR`: Báo cáo sai lệch nét nghĩa, từ loại, phiên âm hoặc dịch thuật.
  - `TOXIC_INAPPROPRIATE_CONTENT`: Báo cáo nội dung thô tục, bạo lực, thù ghét hoặc độc hại.
  - `SPAM_MISLEADING`: Báo cáo nội dung rác, đặt tên lừa đảo hoặc bộ từ rỗng.
- **Ràng buộc Tự động Tạm ẩn khi Đạt Ngưỡng 5 Báo cáo (`Auto-Quarantine Threshold Invariant`)**: Khi một bộ từ công khai nhận đủ $\ge 5$ báo cáo vi phạm bản quyền hoặc độc hại từ 5 người dùng khác nhau, hệ thống TỰ ĐỘNG chuyển trạng thái bộ từ sang `QUARANTINED` (Tạm ẩn công khai) để chờ Quản trị viên xử lý.
- **Quy trình Thu hồi Khẩn cấp SLA $\le 60\text{s}$ (`Emergency Recall SLA Invariant`)**: Lệnh thu hồi khẩn cấp do Admin ban hành BẮT BUỘC thực thi trong SLA $\le 60$ giây: Đổi trạng thái bộ từ / mục từ sang `RECALLED`, vô hiệu cờ `IsPublic`, và dọn dẹp sạch toàn bộ Cache Redis trên các node (D-051).
- **Cách ly Phiên Học Đang Chạy M03 (`Active Session Isolation`)**: Các phiên học M03 đang diễn ra được ghim phiên bản snapshot (`Session Version Pinning` D-082/D-086) để không bị ngắt kết nối đột ngột gây sập ứng dụng. Tuy nhiên, các phiên học MỚI được khởi tạo sẽ hoàn toàn không tìm thấy nội dung đã thu hồi.

## 2. Ma trận Loại Báo cáo và Quy trình Thu hồi Khẩn cấp (Recall Matrix)

| Danh mục Báo cáo (`ReportCategory`) | Mức Độ Ưu tiên | Ngưỡng Tự động Tạm ẩn | Hành động Thu hồi Khẩn cấp | SLA Dọn dẹp Cache Redis | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `COPYRIGHT_VIOLATION_REL04` | **RẤT CAO** | $\ge 5$ báo cáo độc lập | Chuyển `RECALLED`, Ngắt phân phối CDN | SLA $\le 60\text{s}$ | `ACT-M11-02-COPYRIGHT` |
| `TOXIC_INAPPROPRIATE` | **CAO** | $\ge 5$ báo cáo độc lập | Chuyển `QUARANTINED`, Ẩn khỏi tìm kiếm | SLA $\le 60\text{s}$ | `ACT-M11-02-TOXIC` |
| `PEDAGOGICAL_ERROR` | **TRUNG BÌNH** | $\ge 20$ báo cáo | Tạo Ticket yêu cầu sửa cho Tác giả | SLA $\le 5\text{m}$ | `ACT-M11-02-PEDAGOGY` |
| `SPAM_MISLEADING` | **THẤP** | $\ge 15$ báo cáo | Đánh dấu cờ nghi ngờ cho Moderation | SLA $\le 10\text{m}$ | `ACT-M11-02-SPAM` |

## 3. Kiến trúc Luồng Báo cáo và Thu hồi Khẩn cấp (Emergency Recall Engine)

```
[Learner Submits Content Report (Category, TargetSetId, Evidence)]
                               |
                               v
               [Validate Unique User Report Rate]
                               |
                               v
            [Store Report in M02_ContentReports DB]
                               |
                               v
             [Check Threshold: Reports >= 5?]
                               |
            +------------------+------------------+
            | (Reports < 5)                       | (Reports >= 5)
            v                                     v
   [Wait for Admin Review]             [Auto-Quarantine Content]
                                       - Set State = QUARANTINED
                                       - Evict Redis Cache
                                                  |
                                                  v
                              [Admin Issues Emergency Recall Command]
                                       - Set State = RECALLED
                                       - Clear CDN & Redis Cache SLA <= 60s
                                       - Pin Active M03 Sessions
                                       - Record Audit Log ACT-M11-02-RECALL
```

## 4. Giao thức Thực thi Thu hồi Khẩn cấp CSDL (ContentReportAndRecallService)

```csharp
public async Task<bool> ExecuteEmergencyContentRecallAsync(
    string targetSetId, 
    string recallReason, 
    string actorUserId, 
    string supportTicketId)
{
    // 1. Verify Authority (ContentAdmin or SecurityAdmin)
    var targetSet = await _db.VocabularySets.FirstOrDefaultAsync(s => s.Id == targetSetId);
    if (targetSet == null) throw new InvalidOperationException("SET_NOT_FOUND");

    if (string.IsNullOrEmpty(recallReason) || recallReason.Length < 15)
    {
        throw new ArgumentException("RECALL_REASON_MIN_LENGTH_15: Lý do thu hồi khẩn cấp phải tối thiểu 15 ký tự.");
    }

    // 2. Change Set State to RECALLED
    var oldState = targetSet.State;
    targetSet.State = VocabularySetState.RECALLED;
    targetSet.IsPublic = false;
    targetSet.RecalledAtUtc = DateTime.UtcNow;
    targetSet.RecalledByUserId = actorUserId;

    await _db.SaveChangesAsync();

    // 3. Purge Redis Cache SLA <= 60s
    string redisCacheKey = $"wordsoul:set:{targetSetId}";
    string redisListKey = $"wordsoul:public_sets:catalog";
    await _redisDb.KeyDeleteAsync(redisCacheKey);
    await _redisDb.KeyDeleteAsync(redisListKey);

    // 4. Notify M03 to isolate current active sessions (Pinning)
    await _eventPublisher.PublishAsync(new ContentEmergencyRecalledIntegrationEvent {
        TargetSetId = targetSetId,
        RecalledAtUtc = DateTime.UtcNow,
        Reason = recallReason
    });

    // 5. Record Audit Event M11
    await _auditLog.RecordEventAsync("ACT-M11-02-RECALL", actorUserId, new {
        TargetSetId = targetSetId,
        OldState = oldState.ToString(),
        NewState = "RECALLED",
        Reason = recallReason,
        TicketId = supportTicketId
    });

    return true;
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RC-G01` | Hệ thống hỗ trợ tiếp nhận 4 danh mục báo cáo vi phạm (`COPYRIGHT`, `PEDAGOGY`, `TOXIC`, `SPAM`). |
| `RC-G02` | Đạt ngưỡng $\ge 5$ báo cáo bản quyền/độc hại tự động chuyển bộ từ sang `QUARANTINED` ẩn công khai. |
| `RC-G03` | Lệnh thu hồi khẩn cấp gỡ bỏ bộ từ và dọn dẹp Redis Cache hoàn tất trong SLA $\le 60$ giây (D-051). |
| `RC-G04` | Các phiên học M03 đang diễn ra được ghim snapshot (`Session Version Pinning`), không bị sập đột ngột (D-082). |
| `RC-G05` | Bộ từ bị `RECALLED` tuyệt đối CẤM xuất hiện trong kết quả tìm kiếm public hoặc danh mục bộ từ hệ thống. |
| `RC-G06` | 100% các vụ việc thu hồi khẩn cấp được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-02-RECALL`). |
| `RC-G07` | Phân quyền ban hành lệnh thu hồi khẩn cấp chỉ dành riêng cho `ContentAdmin`, `SecurityAdmin` và `SuperAdmin`. |
| `RC-G08` | Mỗi người dùng chỉ được gửi tối đa 1 báo cáo cho cùng 1 bộ từ (Unique User Report Enforcement). |
| `RC-G09` | SLA thực thi API tiếp nhận báo cáo $< 15\text{ms}$; SLA lệnh thu hồi khẩn cấp $< 40\text{ms}$. |
| `RC-G10` | 100% các test case tự kiểm RC33-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC33-01` | Người học nộp báo cáo vi phạm bản quyền REL-04 cho Bộ từ A | Tiếp nhận báo cáo thành công, ghi log report |
| `RC33-02` | Bộ từ A nhận đủ 5 báo cáo vi phạm bản quyền từ 5 người dùng khác nhau | Tự động chuyển trạng thái bộ từ A sang `QUARANTINED` |
| `RC33-03` | Admin ban hành lệnh thu hồi khẩn cấp cho Bộ từ A | Chuyển bộ từ A sang `RECALLED`, xoá cache Redis SLA $< 40\text{s}$ |
| `RC33-04` | Người học mở bài học MỚI từ Bộ từ A sau khi đã bị `RECALLED` | Reject 404 `SET_RECALLED_NOT_AVAILABLE` |
| `RC33-05` | Người học ĐANG trong phiên học M03 tại thời điểm Bộ từ A bị thu hồi | Phiên học tiếp tục bình thường không crash (Session Pinning) |
| `RC33-06` | Thử ban hành lệnh thu hồi khẩn cấp với lý do 8 ký tự ($< 15$) | Reject 400 `RECALL_REASON_MIN_LENGTH_15` |
| `RC33-07` | Tra cứu vết Audit Log M11 sau khi Admin thu hồi bộ từ | Ghi nhận Audit Event `ACT-M11-02-RECALL` đính kèm TargetSetId |
| `RC33-08` | Cùng 1 người học gửi báo cáo vi phạm 2 lần cho cùng 1 bộ từ | Reject 400 `DUPLICATE_REPORT_FORBIDDEN` |
| `RC33-09` | Tải đồng thời 100 báo cáo vi phạm gửi về từ 100 người học | Report ingestion processing latency p95 $< 12\text{ms}$ |
| `RC33-10` | Admin bác bỏ báo cáo sai sự thật cho Bộ từ B đang bị `QUARANTINED` | Khôi phục Bộ từ B về trạng thái `APPROVED` / `PUBLISHED` |
| `RC33-11` | Kiểm tra độ trễ xóa Cache Redis bộ từ bị thu hồi | Eviction SLA $< 500\text{ms}$ |
| `RC33-12` | Tác giả bộ từ bị thu hồi thử gửi duyệt lại đúng bộ từ đó | Reject 400 `CANNOT_RESUBMIT_RECALLED_SET` |
| `RC33-13` | User không phải Admin gọi API thu hồi khẩn cấp | Deny 403 Forbidden |
| `RC33-14` | User chưa đăng nhập gọi API gửi báo cáo vi phạm | Deny 401 Unauthorized |
| `RC33-15` | Báo cáo vi phạm lỗi sư phạm `PEDAGOGICAL_ERROR` chạm mốc 20 lượt | Tự động mở Ticket yêu cầu tác giả sửa nội dung |
| `RC33-16` | Kiểm tra cờ `IsPublic` của bộ từ bị thu hồi | `IsPublic` cập nhật thành `false` chuẩn xác |
| `RC33-17` | Phân tích tham chiếu các báo cáo vi phạm trong CSDL M02 | Quét schema `M02_ContentReports` (T020) |
| `RC33-18` | Thao tác phát sự kiện `ContentEmergencyRecalled` sang M03 bị lỗi | Retry tự động theo Outbox Pattern M12-T037 |
| `RC33-19` | Tra cứu danh sách các bộ từ bị `QUARANTINED` trong trang quản trị M11 | Trả về danh sách bộ từ bị tạm ẩn kèm số lượt báo cáo |
| `RC33-20` | Kiểm thử hoàn tất luồng thiết kế báo cáo và thu hồi M02-REPORT-EMERGENCY-RECALL-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-RC-I01` | Codebase hiện tại chưa có bảng CSDL `M02_ContentReports` tiếp nhận báo cáo | Chưa có kênh báo cáo vi phạm bản quyền REL-04 cho học viên | M02-T049 (Source task) |
| `M02-RC-I02` | Chưa cài đặt cờ tự động tạm ẩn `QUARANTINED` khi đạt ngưỡng 5 báo cáo | Bộ từ vi phạm vẫn hiển thị công khai trên ứng dụng | M02-T049; REL-04 |
| `M02-RC-I03` | Thiếu luồng Thu hồi Khẩn cấp SLA $\le 60\text{s}$ dọn dẹp Redis Cache | Dữ liệu vi phạm bản quyền vẫn nằm trong bộ nhớ đệm CDN/Redis | M02-T049; M11-T019 |
| `M02-RC-I04` | Thiếu cờ `Session Pinning` cách ly phiên học M03 | Người học bị văng khỏi ứng dụng khi bộ từ đang học bị gỡ | M02-T049; M03 tasks |
| `M02-RC-I05` | Chưa kết nối sự kiện thu hồi với Sổ Kiểm toán M11 (`ACT-M11-02-RECALL`) | Không lưu vết được lý do và Admin ban hành lệnh thu hồi | M02-T049; M11-T031 |

- `M02-RC-F01`: Triển khai `ContentReportAndRecallService` tiếp nhận 4 loại báo cáo (tiếp nhận: M02-T049).
- `M02-RC-F02`: Tích hợp Bắt buộc Auto-Quarantine Threshold $\ge 5$ (tiếp nhận: M02-T049; REL-04).
- `M02-RC-F03`: Triển khai Emergency Recall SLA $\le 60\text{s}$ & Session Pinning M03 (tiếp nhận: M02-T049; M11-T019).
- `M02-RC-F04`: Thiết lập bộ kiểm thử tự động RC-G01–G10 và RC33-01–20 (tiếp nhận: M02 tasks).
- `M02-RC-F05`: Thu thập bằng chứng runtime cho luồng thu hồi M02 (tiếp nhận: M02 tasks; A-G03/A-G06).

## 8. Tự kiểm M02-T033

- Đã thiết kế hoàn chỉnh `M02-REPORT-EMERGENCY-RECALL-1.0` với 4 Danh mục Báo cáo Vi phạm Chuẩn hóa.
- Đã chốt Ràng buộc Tự động Tạm ẩn khi Đạt Ngưỡng 5 Báo cáo (`QUARANTINED`).
- Đã chốt Quy trình Thu hồi Khẩn cấp SLA $\le 60$ giây và dọn dẹp Redis Cache.
- Đã lồng ghép Cơ chế Cách ly Phiên Học Đang Chạy M03 (`Session Pinning`) và Audit Log M11 (`ACT-M11-02-RECALL`).
- Đã xác lập 10 Regression Gates (`RC-G01`–`RC-G10`) và 20 Test Cases tự kiểm (`RC33-01`–`RC33-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế báo cáo và thu hồi nội dung M02-T033 | WSA-7K2 |
