# WordSoul – Hướng dẫn Kiểm thử Hiệu năng (k6)

## Mục tiêu
Chứng minh hệ thống chịu tải **100 người dùng đồng thời** liên tục gọi `SubmitAnswer` trong **5 phút**, 
đo được: Response Time · Throughput · Error Rate.

---

## Yêu cầu cài đặt

### 1. Cài k6

```powershell
# Windows (winget)
winget install k6 --source winget

# Hoặc download trực tiếp
# https://github.com/grafana/k6/releases/latest
# → k6-v0.x.x-windows-amd64.msi
```

Kiểm tra:
```powershell
k6 version
```

---

## Thiết lập trước khi chạy

### Bước 1 – Seed 100 tài khoản test

Chạy script SQL trong SQL Server Management Studio:
```
Scripts/performance-test/seed_test_accounts.sql
```

> **Lưu ý:** Thay `$passwordHash` trong file SQL bằng hash thực tế.  
> Tạo hash bằng C#:
> ```csharp
> string hash = BCrypt.Net.BCrypt.HashPassword("Test@123456");
> Console.WriteLine(hash);
> ```

### Bước 2 – Gán VocabularySet cho 100 test accounts

Mỗi test account cần sở hữu ít nhất 1 `VocabularySet` (bảng `UserVocabularySets`).  
Chạy script sau trong SSMS (chạy **sau** `seed_test_accounts.sql`):

```
Scripts/performance-test/seed_user_vocabulary_sets.sql
```

Script này tự động **CROSS JOIN** toàn bộ `VocabularySets` đang active với 100 test accounts —  
mỗi account sẽ sở hữu tất cả bộ từ của hệ thống, dùng để test `SubmitAnswer` đầy đủ.

Kiểm tra sau khi chạy:
```sql
SELECT u.Email, uvs.VocabularySetId, uvs.IsCompleted
FROM Users u
JOIN UserVocabularySets uvs ON uvs.UserId = u.Id
WHERE u.Email LIKE '%@wordsoul.test'
```

> **Lưu ý:** Nếu DB chưa có `VocabularySet` nào → tạo ít nhất 1 set có from vựng trước (`SetVocabularies`).  
> Chỉnh `VOCAB_SET_ID` trong lệnh `k6 run` thành đúng `Id` của set đó.

### Bước 3 – Kiểm tra endpoint login

Đảm bảo `POST /api/auth/login` trả về `{ token: "..." }` hoặc `{ accessToken: "..." }`.  
Nếu route khác, chỉnh trong file `wordsoul_submit_answer_test.js`:
```javascript
const loginRes = http.post(`${BASE_URL}/api/auth/login`, ...)
// Đổi thành: /api/auth/signin hoặc /api/account/token v.v.
```

---

## Chạy test

### Chạy cơ bản
```powershell
cd d:\University\DoAnNganh\WordSoul\WordSoulApi\Scripts\performance-test

k6 run `
  -e BASE_URL=https://localhost:7272 `
  -e VOCAB_SET_ID=1 `
  wordsoul_submit_answer_test.js
```

### Chạy + xuất kết quả JSON để tạo biểu đồ
```powershell
k6 run `
  -e BASE_URL=https://localhost:7001 `
  -e VOCAB_SET_ID=1 `
  wordsoul_submit_answer_test.js
```
> File `result_summary.json` được xuất tự động bởi hàm `handleSummary()` trong script.

### Chạy với dashboard real-time (k6 v0.49+)
```powershell
$env:K6_WEB_DASHBOARD=1
$env:K6_WEB_DASHBOARD_OPEN=1
k6 run -e BASE_URL=https://localhost:7001 wordsoul_submit_answer_test.js
```
Mở trình duyệt → `http://localhost:5665` để xem biểu đồ real-time.

---

## Xem kết quả – Biểu đồ HTML

1. Sau khi test xong, mở file:
   ```
   Scripts/performance-test/generate_chart.html
   ```
   bằng trình duyệt (Chrome/Edge)

2. Click **"Chọn file JSON"** → chọn `result_summary.json`

3. Hoặc click **"Xem Demo"** để xem mẫu kết quả

---

## Giải thích kịch bản test

| Giai đoạn | Thời gian | VU |
|-----------|-----------|-----|
| Ramp-up   | 0s – 30s  | 0 → 100 |
| Steady    | 30s – 4m30s | 100 (ổn định) |
| Ramp-down | 4m30s – 5m | 100 → 0 |

Mỗi VU thực hiện:
1. **Login** → lấy JWT token (1 lần, cache lại)
2. **Tạo LearningSession** → `POST /api/learning-sessions/{setId}`
3. **Liên tục SubmitAnswer** → `POST /api/learning-sessions/{sessionId}/answers`
4. Sau khi hết từ vựng trong session → tạo session mới, lặp lại

---

## Ngưỡng pass/fail (Thresholds)

| Metric | Điều kiện | Lý do |
|--------|-----------|-------|
| `submit_answer_response_time` | p95 < 2000ms | Trải nghiệm người dùng |
| `submit_answer_error_rate` | rate < 1% | Độ tin cậy hệ thống |
| `http_req_failed` | rate < 5% | HTTP-level failures |
| `login_error_rate` | rate < 5% | Auth stability |

---

## Kết quả mong đợi (hệ thống khỏe mạnh)

```
✅ submit_answer_response_time p95 < 2000ms  → ~300-600ms
✅ submit_answer_error_rate rate < 1%        → ~0.1-0.3%
✅ Throughput                                → ~15-40 req/s
```

---

## Troubleshooting

### HTTPS certificate error
```powershell
# Bỏ qua TLS check khi test local
k6 run --insecure-skip-tls-verify wordsoul_submit_answer_test.js
```

### 401 Unauthorized liên tục
- Kiểm tra JWT route: file script dùng `/api/auth/login`
- Đảm bảo accounts đã được seed đúng password hash
- Kiểm tra token field name: `token` hay `accessToken`

### 404 Not Found
- Kiểm tra `VOCAB_SET_ID` tồn tại và thuộc user test
- Chạy SQL: `SELECT * FROM VocabularySets WHERE Id = 1`
