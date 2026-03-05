# WordSoul - Vocabulary Seed Script

Script Python để tự động seed ~200 từ vựng vào Azure SQL database.

## Cách chạy (Windows)

### Bước 1: Cài Python (nếu chưa có)
Tải tại https://www.python.org/downloads/ — chọn "Add to PATH" khi cài.

### Bước 2: Mở Terminal/CMD trong thư mục Scripts
```cmd
cd d:\University\DoAnNganh\WordSoul\WordSoulApi\Scripts
```

### Bước 3: Cài thư viện
```cmd
pip install -r requirements.txt
```

### Bước 4: Chạy script
```cmd
python seed_vocabularies.py
```

Script sẽ chạy trong khoảng **3-5 phút** (có delay để tránh bị rate-limit).
Sau khi xong, file `seed_vocabularies.sql` sẽ xuất hiện trong thư mục `Scripts/`.

---

## Import vào Azure SQL qua SSMS

1. Mở **SQL Server Management Studio (SSMS)**
2. Connect tới Azure SQL Server của bạn
3. **File → Open → File** → chọn file `seed_vocabularies.sql`
4. Tìm dòng đầu: `USE [your_database_name];` → **đổi** `your_database_name` thành tên DB thật
5. Nhấn **F5** (Execute)
6. Kiểm tra: `SELECT COUNT(*) FROM Vocabularies` — kết quả phải là ~200

---

## Lưu ý
- Script sử dụng Google Translate miễn phí — đôi khi độ chính xác không 100%.
- URL ảnh là từ Unsplash (stable, không cần CDN riêng).
- Nếu `dictionaryapi.dev` không tìm thấy từ nào, từ đó vẫn được thêm với dữ liệu tối thiểu.
