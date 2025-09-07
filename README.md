
<h1 align="center">
  <br>
  <a href="https://github.com/your-username/wordsoul"><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756452551/coin-logo_ysauhp.png" alt="WordSoul" width="100"></a>
  <br>
  WordSoul
  <br>
</h1>

<h4 align="center">
  Một ứng dụng web hỗ trợ học và ôn tập từ vựng tiếng Anh kết hợp <i>gamification</i>: thu thập thú cưng, nhận điểm XP, vừa học vừa chơi đầy hứng thú.
</h4>

<p align="center">
  <a href="#tính-năng-chính">Tính năng chính</a> •
  <a href="#-công-nghệ-sử-dụng">Công nghệ sử dụng</a> •
  <a href="#-kiến-trúc--nguyên-tắc">Kiến trúc & Nguyên tắc</a> •
  <a href="#-yêu-cầu-cài-đặt">Yêu cầu cài đặt</a> •
  <a href="#how-to-use">Cách chạy project</a> •
  <a href="#credits">Credits</a>
</p>

---

## Tính năng chính

- 📚 Học Từ Vựng: luyện tập từ mới theo từng chủ đề.
- 🔄 Ôn tập: hệ thống nhắc lại để ghi nhớ lâu dài.
- 🎁 Nhận thưởng: nhận XP, quà tặng khi hoàn thành nhiệm vụ.
- 📂 Quản lý bộ từ vựng: tự tạo, chỉnh sửa và theo dõi.
- 📊 Báo cáo thống kê: theo dõi tiến trình học tập.
- 🐾 Bộ sưu tập: thu thập thú cưng, tạo động lực học.

---

## 🛠 Công nghệ sử dụng

- **Backend**: ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT Authentication  
- **Frontend**: React, Vite, TypeScript, TailwindCSS  
- **Khác**: Cloudinary (lưu trữ hình ảnh), REST API

---

## 🔑 Kiến trúc & Nguyên tắc

- Sử dụng **Dependency Injection (DI)** để tách biệt các thành phần, giảm sự phụ thuộc lẫn nhau và tăng khả năng mở rộng, kiểm thử.  
- Tuân theo các nguyên tắc **SOLID** nhằm đảm bảo mã nguồn rõ ràng, dễ bảo trì:  
  - **S**: Single Responsibility Principle – Mỗi lớp chỉ đảm nhận một trách nhiệm duy nhất.  
  - **O**: Open/Closed Principle – Dễ mở rộng tính năng, hạn chế chỉnh sửa trực tiếp mã gốc.  
  - **L**: Liskov Substitution Principle – Có thể thay thế đối tượng bằng lớp con mà không phá vỡ tính đúng đắn.  
  - **I**: Interface Segregation Principle – Chia nhỏ interface, tránh tạo interface quá lớn.  
  - **D**: Dependency Inversion Principle – Lớp cấp cao không phụ thuộc trực tiếp vào lớp cấp thấp, mà thông qua abstraction (interface).  
- Thiết kế theo mô hình **Modular Monolith** với các tầng rõ ràng: Controller, Service, Repository.  

---

## ⚙️ Yêu cầu cài đặt

Trước khi chạy dự án, cần cài đặt:

- [Node.js](https://nodejs.org/) >= 18  
- [npm](https://www.npmjs.com/) hoặc [yarn](https://yarnpkg.com/)  
- [.NET SDK](https://dotnet.microsoft.com/download) >= 8.0  
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  

## How To Use

Từ command line:

```bash
# Clone this repository
$ git clone https://github.com/your-username/wordsoul

# Go into the repository
$ cd wordsoul

# Backend
$ cd wordsoulapi
$ dotnet restore
$ dotnet ef database update   # tạo database
$ dotnet run

# Frontend
$ cd wordsoul-app
$ npm install
$ npm run dev
```
## Credits
Dự án này sử dụng:

- ASP.NET Core • EF Core • SQL Server
- React • Vite • TailwindCSS • TypeScript
- Cloudinary API



---

> GitHub [@trvannhanh](https://github.com/trvannhanh) &nbsp;&middot;&nbsp;

