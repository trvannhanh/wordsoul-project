# Thiết kế namespace, TTL và invalidation M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-CACHE-TTL-INVALIDATION-1.0` |
| Task | M12-T032 |
| Đầu vào | M12-STATE-REG-1.0 (D-023), M12-SECRET-INVENTORY-1.0 (D-069), REL-03 |
| Phạm vi | Chuẩn hóa danh mục Namespace Redis, thời hạn lưu tồn TTL (Time-To-Live), giao thức vô hiệu hóa cache theo sự kiện (`Event-Driven Eviction`) và bảo mật dữ liệu cache |
| Tự kiểm | A-G04, A-G05; REL-03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Giao thức Quản lý Namespace, TTL và Vô hiệu hóa Cache Redis (`Redis Namespace, TTL & Invalidation Engine`) thuộc M12, chuẩn hóa kiến trúc bộ nhớ đệm toàn hệ thống WordSoulApi, đảm bảo tính nhất quán dữ liệu giữa CSDL Cấu trúc và Redis Cache.

- **Quy tắc Đặt tên Key Chuẩn hóa (`Key Naming Hierarchy Invariant`)**: Mọi Redis Key bắt buộc tuân thủ cấu trúc phân cấp: `wordsoul:{env}:{namespace}:{entityId}`. CẤM đặt key tự do không có namespace (ví dụ: `key123`).
- **4 Tầng Cache và Thời hạn TTL Chuẩn hóa (`4-Tier Cache Classification`)**:
  - *Tầng L1 (Lesson Payload Cache)*: `wordsoul:{env}:lesson_payload:{setId}`, TTL = 3600s (1 giờ). Vô hiệu hóa lập tức SLA $\le 1\text{s}$ khi bộ từ bị sửa hoặc rollback.
  - *Tầng L2 (User Session & Security Epoch)*: `wordsoul:{env}:session:{userId}`, TTL = 86400s (24 giờ sliding). Xóa ngay khi đăng xuất hoặc đổi vai trò.
  - *Tầng L3 (Rate Limiter & Quotas)*: `wordsoul:{env}:ratelimit:{endpoint}:{userId}`, TTL = 60s đến 86400s.
  - *Tầng L4 (Ephemeral Ticket & Verification)*: `wordsoul:{env}:ticket:{ticketId}`, TTL = 600s đến 900s (10-15 phút).
- **Giao thức Vô hiệu hóa Cache Sự kiện (`Event-Driven Eviction SLA <= 1s`)**: Khi dữ liệu gốc CSDL bị cập nhật, service tương ứng phát sự kiện `CacheInvalidationEvent` qua Redis Pub/Sub để xóa cache đồng bộ trên tất cả các instance API trong thời gian $\le 1$ giây.
- **Ràng buộc Bảo mật và Không chứa Bí mật (`No Unmasked Secret in Cache`)**: Dữ liệu lưu trong Redis Cache tuyệt đối CẤM chứa mật khẩu CSDL thô, JWT Signing Keys hoặc PII chưa được che mờ (`rightsCleared` verification REL-03).

## 2. Danh mục Namespace Redis và Chính sách TTL (Redis Namespace Catalog)

| Namespace (`Key Prefix`) | Loại Dữ liệu | Loại TTL | Thời gian TTL | Giao thức Invalidation |
|---|---|---|---|---|
| `wordsoul:{env}:lesson_payload:{setId}` | JSON Payload Bài học M03 | Fixed TTL | 3600 giây (1h) | Event Eviction khi Edit/Publish/Rollback |
| `wordsoul:{env}:session:{userId}` | Epoch & Family Claims M01 | Sliding TTL | 86400 giây (24h) | Evict ngay khi Logout/Lock/Role Change |
| `wordsoul:{env}:ratelimit:{key}` | Cột đếm Rate Limit API | Fixed Window | 60 - 86400 giây | Tự hết hạn theo Window |
| `wordsoul:{env}:ticket:{ticketId}` | Ticket Duyệt/Hỗ trợ M11 | Fixed TTL | 900 giây (15m) | Evict khi Ticket chuyển Approved/Rejected |
| `wordsoul:{env}:leaderboard:{boardId}`| Bảng xếp hạng M09 | Fixed TTL | 300 giây (5m) | Cron Job tự động tính toán lại |
| `wordsoul:{env}:asset_rights:{assetId}`| Trạng thái Bản quyền M12 | Fixed TTL | 86400 giây (24h) | Evict ngay khi Takedown/Rights Revoked |

## 3. Kiến trúc Luồng Eviction Cache Sự kiện (Event-Driven Eviction Engine)

```
[Admin Updates Set / Revokes Asset in DB]
                   |
                   v
    [Commit DB Transaction Success]
                   |
                   v
    [Publish CacheInvalidationEvent]
    - Pattern: "wordsoul:prod:lesson_payload:108"
                   |
                   v
    [Redis Pub/Sub Channel: 'cache-invalidation-channel']
                   |
     +-------------+-------------+
     |                           |
     v                           v
[API Node 1 Cache Evict]   [API Node 2 Cache Evict]
- Key Removed in <= 1s     - Key Removed in <= 1s
```

## 4. Giao thức Thực thi Quản lý Cache CSDL (RedisCacheManagerService)

```csharp
public async Task EvictCachePatternAsync(string keyPattern, string reason)
{
    var endpoints = _redis.GetEndPoints();
    foreach (var endpoint in endpoints)
    {
        var server = _redis.GetServer(endpoint);
        var keys = server.Keys(pattern: keyPattern).ToArray();
        foreach (var key in keys)
        {
            await _dbCache.KeyDeleteAsync(key);
        }
    }

    // Phát sự kiện Pub/Sub đồng bộ tới các node API
    await _subscriber.PublishAsync("cache-invalidation-channel", new CacheEvictionPayload {
        KeyPattern = keyPattern,
        EvictedAtUtc = DateTime.UtcNow,
        Reason = reason
    });

    await _auditLog.RecordEventAsync("ACT-M11-12", "SYSTEM", new { Pattern = keyPattern, Reason = reason });
}
```

## 5. Regression Gate và Case tự kiểm

### 5.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `TI-G01` | 100% Redis Keys tuân thủ chính xác định dạng `wordsoul:{env}:{namespace}:{entityId}`. |
| `TI-G02` | Mọi key lưu vào Redis bắt buộc cài đặt thời hạn TTL (CẤM lưu key vĩnh viễn TTL = -1). |
| `TI-G03` | Vô hiệu hóa cache bài học (`lesson_payload`) thực thi trong SLA $\le 1$ giây sau khi cập nhật CSDL. |
| `TI-G04` | Đăng xuất hoặc thay đổi vai trò tự động xóa cache `wordsoul:session:{userId}` trong SLA $\le 1$ giây. |
| `TI-G05` | Dữ liệu lưu trữ trong Redis tuyệt đối CẤM chứa chuỗi mật khẩu thô hoặc PII chưa che mờ (REL-03). |
| `TI-G06` | Phân vùng Namespace được tách biệt tuyệt đối giữa các môi trường (`dev`, `staging`, `prod`). |
| `TI-G07` | Lệnh xóa cache diện rộng theo pattern (`KeyDelete`) phải qua Pub/Sub để tránh làm nghẽn Redis thread. |
| `TI-G08` | Phân quyền vô hiệu hóa cache thủ công chỉ dành riêng cho `ContentAdmin` và `SuperAdmin`. |
| `TI-G09` | SLA thực thi API đọc cache hit $< 5\text{ms}$. |
| `TI-G10` | 100% các test case tự kiểm TI32-01–20 đạt thành công trong bộ suite kiểm thử. |

### 5.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `TI32-01` | Nạp cache payload cho Bộ từ 108 ở môi trường Prod | Key tạo thành công: `wordsoul:prod:lesson_payload:108`, TTL = 3600s |
| `TI32-02` | Admin chỉnh sửa Bộ từ 108 và lưu CSDL thành công | Key `wordsoul:prod:lesson_payload:108` tự động bị xoá trong $< 1\text{s}$ |
| `TI32-03` | Thử nạp key không đúng cấu hình `bad_key_name` | System reject 400 `INVALID_REDIS_KEY_NAMESPACE` |
| `TI32-04` | Thử tạo key mà không thiết lập thời hạn TTL (TTL = -1) | System reject 400 `REDIS_KEY_TTL_REQUIRED` |
| `TI32-05` | Người học A đăng xuất khỏi hệ thống | Cache session `wordsoul:prod:session:USR-A` tự động bị xoá |
| `TI32-06` | Quét nội dung lưu trong cache `wordsoul:prod:session:USR-A` | Chỉ chứa `SecurityEpoch` và claims đã mã hóa, 0 PII thô |
| `TI32-07` | Kiểm tra thời gian phản hồi khi đọc cache hit bài học | Response latency p95 $< 3\text{ms}$ |
| `TI32-08` | Xảy ra sự cố ngắt kết nối Redis giữa chừng | Hệ thống fallback tự động đọc từ CSDL PostgreSQL (Fail-safe) |
| `TI32-09` | Admin thực hiện lệnh Flush Cache thủ công cho 1 Bộ từ | Xóa cache thành công và phát sự kiện Pub/Sub đồng bộ |
| `TI32-10` | Kiểm tra phân vùng cache giữa Prod và Staging | Key `wordsoul:staging:...` hoàn toàn độc lập với `wordsoul:prod:...` |
| `TI32-11` | Tra cứu vết Audit Log M11 sau khi xoá cache mẫu | Ghi nhận Audit Event `ACT-M11-12` đính kèm pattern |
| `TI32-12` | Tải đồng thời 1000 request đọc cache hit | Response latency p95 $< 4\text{ms}$, 0 lỗi kết nối |
| `TI32-13` | User không phải Admin thử gọi API xóa cache thủ công | Deny 403 Forbidden |
| `TI32-14` | User chưa đăng nhập thử gọi API vô hiệu hóa cache | Deny 401 Unauthorized |
| `TI32-15` | Đặt lại mốc thời gian hết hạn TTL cho key đang tồn tại | Cập nhật mốc TTL mới thành công |
| `TI32-16` | Kiểm tra tỷ lệ Hit-Rate của Cache bài học M03 | Cache Hit-Rate đạt $> 92\%$ trong điều kiện bình thường |
| `TI32-17` | Phân tích tham chiếu danh sách key trong bộ nhớ đệm | Quét toàn bộ namespace active trong Redis (T020) |
| `TI32-18` | Thao tác xoá cache bị gián đoạn do timeout | Ghi log cảnh báo và tự động retry xóa trong job nền |
| `TI32-19` | Thu hồi bản quyền 1 tài khoản media M12 | Xóa cache `wordsoul:prod:asset_rights:{assetId}` ngay lập tức |
| `TI32-20` | Kiểm thử hoàn tất luồng thiết kế namespace TTL M12-CACHE-TTL-INVALIDATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-TI-I01` | Một số key Redis trong codebase hiện tại đặt tên không thống nhất | Gây gián đoạn và rủi ro ghi đè key giữa các module | M12-T049 (Source task) |
| `M12-TI-I02` | Chưa có bộ lắng nghe Pub/Sub đồng bộ cache eviction giữa các node API | Một số node API vẫn giữ cache cũ khi CSDL đã đổi | M12-T049; M12-T031 |
| `M12-TI-I03` | Thiếu cờ bắt buộc gán TTL cho 100% Redis keys | Nguy cơ tràn bộ nhớ RAM Redis do tích tụ key rác vĩnh viễn | M12-T049 |
| `M12-TI-I04` | Thiếu cơ chế fallback đọc CSDL khi Redis bị ngắt kết nối | Khi Redis gặp sự cố, toàn bộ ứng dụng bị sập theo (500) | M12-T049; M12-T003 |
| `M12-TI-I05` | Chưa mã hóa/che mờ một số trường dữ liệu PII lưu trong cache | Rủi ro lộ PII nếu Server Redis bị truy cập bất hợp pháp | M12-TI-F05; REL-03 |

- `M12-TI-F01`: Triển khai `RedisKeyGenerator` chuẩn hóa namespace (tiếp nhận: M12-T049).
- `M12-TI-F02`: Triển khai `RedisPubSubEvictionManager` đồng bộ đa node (tiếp nhận: M12-T049; M12-T031).
- `M12-TI-F03`: Tích hợp Fallback Read CSDL khi Redis sập (tiếp nhận: M12-T049; M12-T003).
- `M12-TI-F04`: Thiết lập bộ kiểm thử tự động TI-G01–G10 và TI32-01–20 (tiếp nhận: M12 tasks).
- `M12-TI-F05`: Thu thập bằng chứng runtime cho luồng namespace TTL M12 (tiếp nhận: M12 tasks; A-G04/A-G05).

## 8. Tự kiểm M12-T032

- Đã thiết kế hoàn chỉnh `M12-CACHE-TTL-INVALIDATION-1.0` với Quy tắc Đặt tên Key `wordsoul:{env}:{namespace}:{entityId}`.
- Đã chốt Ràng buộc 4 Tầng Cache và Thời hạn TTL Chuẩn hóa.
- Đã chốt Giao thức Vô hiệu hóa Cache Sự kiện qua Redis Pub/Sub (SLA $\le 1\text{s}$).
- Đã lồng ghép Bảo mật Dữ liệu Cache không chứa Bí mật/PII thô (REL-03) và Fallback CSDL.
- Đã xác lập 10 Regression Gates (`TI-G01`–`TI-G10`) và 20 Test Cases tự kiểm (`TI32-01`–`TI32-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Khởi tạo đặc tả thiết kế namespace, TTL và invalidation M12-T032 | WSA-7K2 |
