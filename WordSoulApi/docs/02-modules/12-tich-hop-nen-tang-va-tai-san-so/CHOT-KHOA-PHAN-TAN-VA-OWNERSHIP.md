# Chốt khóa phân tán và ownership M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-DISTRIBUTED-LOCK-1.0` |
| Task | M12-T033 |
| Đầu vào | M12-STATE-REG-1.0 (D-023), M12-CACHE-TTL-INVALIDATION-1.0 (D-097), REL-03 |
| Phạm vi | Đặc tả Giao thức Quản lý Khóa Phân tán Redis Redlock (`Redis Distributed Lock`), định danh sở hữu khóa duy nhất (`LockOwnershipValue`), giải phóng khóa an toàn bằng Lua Script và tự động gia hạn TTL |
| Tự kiểm | A-G04; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quản lý Khóa Phân tán và Quyền Sở hữu Khóa (`Redis Distributed Lock & Lock Ownership Engine`) thuộc M12, loại bỏ triệt để rủi ro Race Condition khi nhiều Instance API cùng tranh chấp xử lý các thao tác bất biến (ví dụ: Nộp bài M03, Khởi tạo giao dịch Gamification M06, Đổi tên hiển thị M01).

- **Thuật toán Khóa Phân tán Redlock (`Redlock Consensus Invariant`)**: Sử dụng thuật toán Redlock qua cụm Redis Cluster. Khóa chỉ được coi là ĐÃ CHIẾM HỮU THÀNH CÔNG khi đạt đồng thuận trên $N/2 + 1$ node Redis trong thời gian nhỏ hơn thời hạn TTL của khóa.
- **Định danh Quyền Sở hữu Khóa Duy nhất (`Lock Ownership Value Invariant`)**: Mọi thao tác xin khóa bắt buộc sinh một giá trị GUID ngẫu nhiên duy nhất đại diện cho Node và Thread chiếm giữ (`LockValue = $"{NodeId}:{Guid.NewGuid()}"`). CẤM dùng giá trị cố định hoặc chỉ dùng NodeId.
- **Giải phóng Khóa Nguyên tử bằng Lua Script (`Atomic Release via Lua Script Invariant`)**: Giải phóng khóa BẮT BUỘC thực thi qua đoạn mã Lua Script kiểm tra chính xác `LockValue` trước khi xóa (`DEL`). CẤM xóa trực tiếp key mà không kiểm tra sở hữu, tránh trường hợp Node A bị trễ xóa mất khóa của Node B.
- **Tự động Gia hạn Thời hạn Khóa (`Lock Extension Heartbeat`)**: Đối với các tác vụ xử lý kéo dài hơn thời gian lease mặc định (5000ms), một timer chạy ngầm (`Heartbeat Timer` mỗi 1500ms) sẽ tự động gia hạn TTL khóa nếu Node chiếm giữ vẫn đang hoạt động.

## 2. Lua Script Giải phóng Khóa An toàn (Atomic Release Lua Script)

```lua
-- Lua Script giải phóng khóa an toàn
-- KEYS[1]: Lock Key (vd: wordsoul:prod:lock:set_publish:108)
-- ARGV[1]: Lock Ownership Value (vd: NODE_01:a8f9c2d1-...)

if redis.call("get", KEYS[1]) == ARGV[1] then
    return redis.call("del", KEYS[1])
else
    return 0
end
```

## 3. Quy trình Chiếm giữ và Giải phóng Khóa Phân tán (Distributed Lock Lifecycle)

```
[Service Request Acquire Distributed Lock (LockKey, LeaseTime=5s)]
                                |
                                v
               [Generate Unique LockValue GUID]
               - LockValue = $"{NodeId}:{Guid.NewGuid()}"
                                |
                                v
               [Execute SET LockKey LockValue NX PX 5000]
                                |
                      +---------+---------+
                      | (Acquire Failed)  | (Acquire Success)
                      v                   v
               [Retry / Reject    [Start Heartbeat Extension Timer]
                Busy 429]         - Extend TTL by 5s every 1500ms
                                          |
                                          v
                              [Execute Critical Business Logic]
                                          |
                                          v
                              [Stop Extension Timer]
                                          |
                                          v
                              [Execute Atomic Release Lua Script]
                              - Verify KEYS[1] == ARGV[1] before DEL
```

## 4. Giao thức Thực thi Khóa Phân tán CSDL (DistributedLockManagerService)

```csharp
public async Task<IDistributedLockHandle> AcquireLockAsync(string resourceKey, TimeSpan leaseTime, TimeSpan acquireTimeout)
{
    string fullKey = $"wordsoul:{_env}:lock:{resourceKey}";
    string lockValue = $"{_nodeId}:{Guid.NewGuid():N}";
    var cancellationTokenSource = new CancellationTokenSource(acquireTimeout);

    while (!cancellationTokenSource.IsCancellationRequested)
    {
        bool acquired = await _redisDb.StringSetAsync(fullKey, lockValue, leaseTime, When.NotExists);
        if (acquired)
        {
            // Tự động bật Heartbeat Timer gia hạn lock nếu xử lý lâu
            var timer = new Timer(async _ => {
                await ExtendLockAsync(fullKey, lockValue, leaseTime);
            }, null, TimeSpan.FromMilliseconds(1500), TimeSpan.FromMilliseconds(1500));

            return new DistributedLockHandle(fullKey, lockValue, _redisDb, timer);
        }

        await Task.Delay(50, cancellationTokenSource.Token); // Retry sau 50ms
    }

    throw new TimeoutException($"DISTRIBUTED_LOCK_ACQUIRE_TIMEOUT: Không thể chiếm khóa tài nguyên '{resourceKey}' trong {acquireTimeout.TotalMilliseconds}ms.");
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `DL-G01` | Mọi khóa phân tán bắt buộc dùng `LockValue` GUID duy nhất đại diện cho thread/node sở hữu. |
| `DL-G02` | Giải phóng khóa phân tán bắt buộc chạy qua Lua Script kiểm tra giá trị sở hữu trước khi xóa. |
| `DL-G03` | Cấm Node A giải phóng khóa của Node B khi tác vụ của Node A bị chậm trễ quá TTL. |
| `DL-G04` | Tác vụ kéo dài quá 5000ms tự động được Heartbeat Timer gia hạn TTL mỗi 1500ms. |
| `DL-G05` | Chiếm khóa không thành công trong thời gian `acquireTimeout` phải ném lỗi Timeout 429 rõ ràng. |
| `DL-G06` | Phân vùng Key khóa phân tán phải có tiền tố `wordsoul:{env}:lock:{resourceKey}`. |
| `DL-G07` | Khi tác vụ hoàn thành, Heartbeat Timer bắt buộc được dọn dẹp (Dispose) ngay lập tức. |
| `DL-G08` | 100% các thao tác chiếm/giải phóng khóa thất bại hoặc bị tranh chấp được ghi vết log chẩn đoán. |
| `DL-G09` | SLA chiếm khóa thành công trong điều kiện không tranh chấp $< 10\text{ms}$. |
| `DL-G10` | 100% các test case tự kiểm DL33-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `DL33-01` | Node 1 chiếm khóa tài nguyên `set_publish:108` | Chiếm khóa thành công, `SET NX PX 5000` trả về OK |
| `DL33-02` | Node 2 thử chiếm khóa `set_publish:108` trong khi Node 1 đang giữ | Node 2 bị chặn, phải chờ hoặc nhận lỗi Timeout |
| `DL33-03` | Node 1 giải phóng khóa sau khi làm xong nghiệp vụ | Chạy Lua Script kiểm tra `LockValue` match và xóa thành công |
| `DL33-04` | Node 1 bị tạm dừng GC 6 giây (quá TTL 5s), Node 2 chiếm được khóa mới, sau đó Node 1 tỉnh lại và thử xóa khóa | Lua Script phát hiện `LockValue` không khớp, CẤM xóa khóa của Node 2 |
| `DL33-05` | Tác vụ của Node 1 chạy kéo dài 10 giây (quá 5s lease) | Heartbeat Timer tự động gia hạn lock thành công ở s thứ 3 và 6 |
| `DL33-06` | Thử giải phóng khóa bằng lệnh `KeyDelete` trực tiếp không qua Lua | System reject / Code review Fail |
| `DL33-07` | Kiểm tra thời gian phản hồi chiếm khóa thành công | Latency p95 $< 8\text{ms}$ |
| `DL33-08` | Thử nghiệm 100 request tranh chấp 1 tài nguyên duy nhất cùng lúc | Đúng 1 request chiếm được khóa tại 1 thời điểm, 99 request xếp hàng |
| `DL33-09` | Node 1 sập đột ngột khi đang giữ khóa | Khóa tự động giải phóng sau 5000ms khi TTL hết hạn |
| `DL33-10` | Đặt `acquireTimeout = 500ms` và tranh chấp khóa | Hết 500ms không lấy được lock ném `DISTRIBUTED_LOCK_ACQUIRE_TIMEOUT` |
| `DL33-11` | Tra cứu vết Audit Log M11 khi xảy ra tranh chấp khóa kéo dài | Ghi nhận Audit Event `ACT-M11-12` chẩn đoán |
| `DL33-12` | Tải đồng thời 500 request xin khóa trên 50 tài nguyên khác nhau | 100% tài nguyên được xử lý song song độc lập |
| `DL33-13` | User không phải Admin xin khóa các tác vụ hệ thống | Deny 403 Forbidden |
| `DL33-14` | User chưa đăng nhập xin khóa tác vụ | Deny 401 Unauthorized |
| `DL33-15` | Giải phóng khóa 2 lần liên tiếp từ cùng 1 handle | Lần 1 xóa thành công, lần 2 trả về 0 an toàn không lỗi |
| `DL33-16` | Kiểm tra tên khóa sinh ra cho tài nguyên `user_role:10024` | Key: `wordsoul:prod:lock:user_role:10024` |
| `DL33-17` | Phân tích tham chiếu danh sách khóa đang active trên Redis | Quét các key có tiền tố `wordsoul:*:lock:*` (T020) |
| `DL33-18` | Thao tác chiếm khóa bị ngắt do mất kết nối mạng CSDL | Handle ném exception và dọn dẹp timer |
| `DL33-19` | Xin khóa với `leaseTime = 10000ms` (10s) cho tác vụ nặng | Chiếm khóa với TTL 10 giây thành công |
| `DL33-20` | Kiểm thử hoàn tất luồng chốt khóa phân tán M12-DISTRIBUTED-LOCK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-DL-I01` | Một số vị trí xử lý đồng thời hiện đang dùng `lock(object)` C# in-memory | Không chặn được race condition giữa các instance API chạy trên nhiều container | M12-T049 (Source task) |
| `M12-DL-I02` | Chưa có đoạn mã Lua Script chuẩn hóa giải phóng khóa an toàn | Rủi ro xóa nhầm khóa của node khác khi bị trễ execution | M12-T049; REL-03 |
| `M12-DL-I03` | Thiếu Heartbeat Timer gia hạn lock tự động cho tác vụ nặng | Tác vụ chạy quá 5s bị mất lock giữa chừng gây race condition | M12-T049 |
| `M12-DL-I04` | Thiếu validation cấm xin lock với `LockValue` cố định | Dễ mắc sai lầm dùng chung GUID cho toàn bộ instance | M12-T049 |
| `M12-DL-I05` | Chưa đo đạc thời gian chờ lock (`AcquireLockLatency`) | Không phát hiện được các điểm nghẽn cổ chai trong hệ thống | M12-T049; M12-T045 |

- `M12-DL-F01`: Triển khai `DistributedLockManagerService` với Redlock & Lua script (tiếp nhận: M12-T049).
- `M12-DL-F02`: Tích hợp Heartbeat Extension Timer gia hạn lock (tiếp nhận: M12-T049).
- `M12-DL-F03`: Thay thế toàn bộ in-memory lock bằng Redis Distributed Lock (tiếp nhận: M12-T049; REL-03).
- `M12-DL-F04`: Thiết lập bộ kiểm thử tự động DL-G01–G10 và DL33-01–20 (tiếp nhận: M12 tasks).
- `M12-DL-F05`: Thu thập bằng chứng runtime cho luồng khóa phân tán M12 (tiếp nhận: M12 tasks; A-G04).

## 8. Tự kiểm M12-T033

- Đã thiết kế hoàn chỉnh `M12-DISTRIBUTED-LOCK-1.0` với Giao thức Khóa Phân tán Redis Redlock.
- Đã chốt Ràng buộc Thuật toán Khóa Phân tán Redlock và Định danh Sở hữu `LockValue` GUID duy nhất.
- Đã chốt Giao thức Giải phóng Khóa Nguyên tử qua Lua Script và Heartbeat Timer gia hạn TTL tự động.
- Đã lồng ghép Tự động Dọn dẹp Timer và fallback ném lỗi Timeout 429 (REL-03).
- Đã xác lập 10 Regression Gates (`DL-G01`–`DL-G10`) và 20 Test Cases tự kiểm (`DL33-01`–`DL33-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả chốt khóa phân tán và ownership M12-T033 | WSA-7K2 |
