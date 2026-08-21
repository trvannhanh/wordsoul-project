# Thiết kế thu nhận log bền vững M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-SUSTAINABLE-LOG-INGESTION-1.0` |
| Task | M11-T034 |
| Đầu vào | M11-LOG-TAXONOMY-1.0 (D-055), M11-DATA-REDACTION-LOG-POLICY-1.0 (D-123), M12-CAPABILITY-1.0 (D-020), REL-02, REL-03 |
| Phạm vi | Đặc tả Giao thức Thu nhận Log Bền vững (`Sustainable Log Ingestion Protocol`), cơ chế Async Disk Buffer Queue, chính sách Backpressure & Fallback sang đệm Local Disk Spooling, tiến trình `LogIngestionShipper` khôi phục đẩy log khi hết ngắt kết nối và lưu vết kiểm toán M11 |
| Tự kiểm | A-G02, A-G06; REL-02, REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Thu nhận Log Bền vững (`Sustainable Log Ingestion Protocol`) thuộc M11, xác lập kiến trúc đẩy và đệm log tin cậy cao từ toàn bộ các ứng dụng node API Server về các hệ thống thu nhận tập trung (Seq / ELK / CloudWatch), đảm bảo ứng dụng không bao giờ bị nghẽn (Backpressure) hoặc mất log khi hệ thống ghi log đối tác bị sự cố gián đoạn (REL-02, REL-03).

- **Hàng chờ Đệm Đĩa Bất đồng bộ local (`Async Disk Buffer Queue Invariant`)**: 100% dòng log phát sinh từ ứng dụng BẮT BUỘC được đẩy qua hàng chờ bất đồng bộ không chặn thread (`Serilog.Sinks.File` với buffer đệm đĩa local tối đa 500MB per node). Tuyệt đối CẤM ghi log đồng bộ trực tiếp ra mạng làm tăng độ trễ HTTP request (REL-03).
- **Chính sách Khống chế Áp lực và Dự phòng Đệm Đĩa local (`Backpressure & Local Spooling Fallback`)**: Khi hệ thống thu nhận log tập trung (ELK/Seq) bị đứt kết nối hoặc quá tải, ứng dụng TỰ ĐỘNG chuyển sang chế độ `LOCAL_DISK_SPOOLING`. Dữ liệu log được nén và ghi đệm an toàn vào đĩa local. Thời gian lưu giữ đệm local tối đa 7 ngày (`MaxDiskSpoolDays = 7`).
- **Worker Đẩy Log Khôi phục Tự động (`LogIngestionShipper Worker`)**: Tiến trình chạy ngầm `LogIngestionShipper` liên tục kiểm tra trạng thái kết nối tới Log Sink. Khi kết nối khôi phục, worker tự động đẩy lại các gói log đệm local theo thuật toán Exponential Backoff Retry ($\le 60\text{s}$) với giới hạn lưu lượng (max 10,000 log events/giây).
- **Lưu vết Sổ Kiểm toán Thu nhận Log M11 (`Log Ingestion Audit Trail`)**: $100\%$ các sự kiện kích hoạt chế độ đệm đĩa local hoặc đứt kết nối Log Sink được ghi vết bất biến `ACT-M11-34-INGEST` trong Sổ Kiểm toán M11.

## 2. Ma trận Quy trình Thu nhận Log và Fallback (Log Ingestion Matrix)

| Trạng thái Log Sink (`SinkState`) | Kênh Ghi Log | Hành vi Hàng chờ (`Queue Action`) | Giới hạn Lưu giữ Đệm (`Spool Limit`) | SLA Khôi phục Đẩy Log | Vết Kiểm toán M11 |
|---|---|---|---|---|---|
| `ONLINE` | Central Aggregator (Seq/ELK) | Flush Async qua Network | Dynamic Memory Buffer | N/A (Trực tiếp) | N/A (Chuẩn) |
| `SINK_DEGRADED` | Network + Local Spooling | Batch Size Nén 500 events | Local Disk 500MB | Retry SLA $\le 10\text{s}$ | `ACT-M11-34-DEGRADED` |
| `SINK_OFFLINE` | **Local Disk Spooling Only** | Write to Local Encrypted Log Files | Local Disk 5GB / **7 Ngày** | Auto Retry Exponential | `ACT-M11-34-OFFLINE` |
| `DISK_FULL_EMERGENCY` | Local Disk + Alert | Drop P4_INFO, Keep P1 Audit | Ring Buffer Overwrite P4 | Alert SLA $\le 60\text{s}$ | `ACT-M11-34-EMERGENCY` |

## 3. Kiến trúc Luồng Thu nhận Log Bền vững M11 (Ingestion Engine Pipeline)

```
[Application Logger Trace Event]
                |
                v
 [Apply Serilog Redaction Enricher (D-123)]
                |
                v
 [Async Disk Buffer Queue (Local Memory/Disk 500MB)]
                |
                v
 [LogIngestionShipper Worker: Check Central Sink Status]
                |
       +--------+--------+
       | (Sink ONLINE)   | (Sink OFFLINE)
       v                 v
[Flush Stream to ELK] [Switch to LOCAL_DISK_SPOOLING Mode]
                      - Write to Encrypted Spool Files
                      - Set Retention Window = 7 Days
                      - Trigger Retry Exponential Backoff (<=60s)
                      - Record Audit Log ACT-M11-34-OFFLINE
```

## 4. Giao thức Thực thi Worker Đẩy Log CSDL (LogIngestionShipperWorker)

```csharp
public class LogIngestionShipperWorker : BackgroundService
{
    private readonly ISpoolFileRepository _spoolRepository;
    private readonly ICentralLogSinkClient _centralSinkClient;
    private readonly IAuditLogger _auditLog;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingSpoolFiles = await _spoolRepository.GetUnsentSpoolFilesAsync();
            if (pendingSpoolFiles.Any())
            {
                bool isSinkHealthy = await _centralSinkClient.CheckHealthAsync(stoppingToken);
                if (isSinkHealthy)
                {
                    foreach (var spoolFile in pendingSpoolFiles)
                    {
                        var eventsBatch = await _spoolRepository.ReadBatchAsync(spoolFile, maxEvents: 1000);
                        bool isSuccess = await _centralSinkClient.SendLogBatchAsync(eventsBatch, stoppingToken);

                        if (isSuccess)
                        {
                            await _spoolRepository.MarkFileProcessedAsync(spoolFile);
                        }
                        else
                        {
                            // Trigger Exponential Backoff Retry (<= 60s)
                            await Task.Delay(TimeSpan.FromSeconds(Math.Min(60, 5 * spoolFile.RetryCount)), stoppingToken);
                            break;
                        }
                    }
                }
                else
                {
                    await _auditLog.RecordEventAsync("ACT-M11-34-OFFLINE", "LOG_WORKER", new {
                        PendingFilesCount = pendingSpoolFiles.Count,
                        Action = "SPOOLING_TO_LOCAL_DISK"
                    });
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `LI-G01` | Ghi log từ ứng dụng BẮT BUỘC dùng đệm bất đồng bộ local, tuyệt đối CẤM chặn luồng xử lý HTTP request chính. |
| `LI-G02` | Khi Log Sink tập trung bị gián đoạn, ứng dụng tự động chuyển sang chế độ đệm đĩa local (`LOCAL_DISK_SPOOLING`). |
| `LI-G03` | Thời hạn lưu giữ log đệm đĩa local duy trì tối đa 7 ngày (`MaxDiskSpoolDays = 7`). |
| `LI-G04` | Worker `LogIngestionShipper` tự động đẩy khôi phục log khi kết nối trở lại với tốc độ giới hạn $\le 10,000$ events/s. |
| `LI-G05` | Trường hợp đĩa local bị đầy khẩn cấp, hệ thống tự động ưu tiên giữ lại Log Audit/P1 và loại bỏ Log P4_INFO. |
| `LI-G06` | 100% các sự kiện đứt kết nối hoặc kích hoạt chế độ đệm đĩa local được ghi vết bất biến vào Sổ Kiểm toán M11 (`ACT-M11-34-OFFLINE`). |
| `LI-G07` | SLA thực thi đẩy một gói 1,000 log events từ đệm local sang Central Sink $< 200\text{ms}$. |
| `LI-G08` | Phân quyền cấu hình đường dẫn đệm local `SpoolDirectoryPath` chỉ dành cho `SystemAdmin` và `SecurityAdmin`. |
| `LI-G09` | Hệ thống chịu tải đệm đĩa local lên tới 5GB dữ liệu log mà không bị tràn bộ nhớ RAM. |
| `LI-G10` | 100% các test case tự kiểm LI34-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `LI34-01` | Ghi 10,000 log events liên tục trong khi Log Sink đang Online | Log được đẩy bất đồng bộ, HTTP latency p95 tăng $< 0.1\text{ms}$ |
| `LI34-02` | Ngắt kết nối mạng tới Log Sink Seq trong 10 phút | Tự động chuyển `LOCAL_DISK_SPOOLING`, ghi log vào đĩa local an toàn |
| `LI34-03` | Khôi phục kết nối mạng tới Log Sink Seq sau 10 phút đứt | Worker `LogIngestionShipper` đẩy cạn tệp đệm local SLA $< 15\text{s}$ |
| `LI34-04` | Dung lượng đệm đĩa local đạt mốc 90% ngưỡng tối đa (4.5GB / 5GB) | Kích hoạt cảnh báo WARN `DISK_SPOOL_NEAR_CAPACITY` |
| `LI34-05` | Dung lượng đệm đĩa local bị tràn 100% (5GB / 5GB) | Giữ lại Audit Logs, đè dọn dẹp bớt Log P4_INFO cũ nhất |
| `LI34-06` | Tra cứu vết Audit Log M11 sau khi chuyển sang chế độ đệm đĩa local | Ghi nhận Audit Event `ACT-M11-34-OFFLINE` |
| `LI34-07` | Đọc tệp đệm log local bị lỗi hỏng do ngắt nguồn bất ngờ | Bỏ qua phần tệp lỗi, đẩy tiếp phần log nguyên vẹn |
| `LI34-08` | Gửi gói 1,000 log events có chứa 1 event bị lỗi định dạng JSON | Central Sink nhận 999 events, log riêng event lỗi |
| `LI34-09` | Tải đồng thời 5,000 log events/giây qua Worker LogIngestionShipper | Flush processing latency p95 $< 150\text{ms}$ |
| `LI34-10` | Kiểm tra thời gian vô hiệu tệp đệm local sau khi đẩy thành công | Xóa tệp đệm local SLA $< 1\text{s}$ sau khi mark processed |
| `LI34-11` | Thử cấu hình đường dẫn `SpoolDirectoryPath` sang thư mục không có quyền write | Fallback ghi vào `%TEMP%/wordsoul-logs`, phát alert |
| `LI34-12` | Gửi request cấu hình tham số thu nhận log khi JWT hết hạn | Deny 401 Unauthorized |
| `LI34-13` | User không phải SystemAdmin thử thay đổi dung lượng `MaxDiskSpoolMB` | Deny 403 Forbidden |
| `LI34-14` | User chưa đăng nhập gọi API tra cứu trạng thái đệm log M11 | Deny 401 Unauthorized |
| `LI34-15` | Kiểm tra tính bảo mật nén mã hóa của các tệp đệm log local | Tệp đệm đĩa local được mã hóa AES-256 an toàn |
| `LI34-16` | Kiểm tra độ trễ ngắt mạch đẩy log khi Retry thất bại 5 lần | Exponential delay đúng mốc 60 giây |
| `LI34-17` | Phân tích tham chiếu các bản ghi `SpoolFiles` trong CSDL | Quét schema `M11_SpoolFiles` (T020) |
| `LI34-18` | Tiến trình `LogIngestionShipperWorker` gặp sự cố sập giữa chừng | Worker dự phòng tự khởi động tiếp quản đệm đĩa |
| `LI34-19` | Tra cứu danh sách các tệp đệm log local đang chờ đẩy | Trả về DTO danh sách tệp kèm dung lượng |
| `LI34-20` | Kiểm thử hoàn tất luồng thu nhận log bền vững M11-SUSTAINABLE-LOG-INGESTION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-LI-I01` | M11 hiện tại chưa có `LogIngestionShipperWorker` xử lý đệm đĩa local | Risk mất log khi hệ thống ghi log Seq bị gián đoạn | M11-T049 (Source task) |
| `M11-LI-I02` | Thiếu cờ `LOCAL_DISK_SPOOLING` tự động khi đứt kết nối Log Sink | Gây phơi nhiễm lỗi hoặc treo luồng ứng dụng chính | M11-T049; REL-02 |
| `M11-LI-I03` | Thiếu luồng nén mã hóa AES-256 cho tệp đệm log local | Risk rò rỉ dữ liệu nhạy cảm nếu ổ đĩa đệm bị đọc trộm | M11-T049; REL-03 |
| `M11-LI-I04` | Thiếu chính sách ưu tiên giữ Audit Log khi đĩa đệm bị tràn | Audit proof quan trọng có thể bị xóa nhầm | M11-LI-F04; M11-T031 |
| `M11-LI-I05` | Chưa kết nối sự kiện đứt kết nối log sink với Audit Log M11 (`ACT-M11-34-OFFLINE`) | Không ghi vết được khoảng thời gian log sink bị offline | M11-T049; M11-T031 |

- `M11-LI-F01`: Triển khai `LogIngestionShipperWorker` với Async Disk Buffer Queue (tiếp nhận: M11-T049).
- `M11-LI-F02`: Tích hợp Bắt buộc `LOCAL_DISK_SPOOLING` Fallback Mode (tiếp nhận: M11-T049; REL-02).
- `M11-LI-F03`: Triển khai Encrypted Local Spool Files & Exponential Retry (tiếp nhận: M11-T049; REL-03).
- `M11-LI-F04`: Thiết lập bộ kiểm thử tự động LI-G01–G10 và LI34-01–20 (tiếp nhận: M11 tasks).
- `M11-LI-F05`: Thu thập bằng chứng runtime cho luồng thu nhận log M11 (tiếp nhận: M11 tasks; A-G02/A-G06).

## 8. Tự kiểm M11-T034

- Đã thiết kế hoàn chỉnh `M11-SUSTAINABLE-LOG-INGESTION-1.0` với Ma trận Quy trình Thu nhận Log và Fallback.
- Đã chốt Ràng buộc Hàng chờ Đệm Đĩa Bất đồng bộ local (`Async Disk Buffer Queue`).
- Đã chốt Chính sách Khống chế Áp lực và Dự phòng Đệm Đĩa local (`LOCAL_DISK_SPOOLING` Fallback 7 ngày).
- Đã lồng ghép Worker Đẩy Log Khôi phục Tự động (`LogIngestionShipperWorker`), Mã hóa AES-256 tệp đệm đĩa và Audit Log M11 (`ACT-M11-34-OFFLINE`).
- Đã xác lập 10 Regression Gates (`LI-G01`–`LI-G10`) and 20 Test Cases tự kiểm (`LI34-01`–`LI34-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế thu nhận log bền vững M11-T034 | WSA-7K2 |
