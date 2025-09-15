
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

- 📚 Học từ vựng: Luyện tập từ vựng theo chủ đề, hỗ trợ lọc theo từ, nghĩa, loại từ (PartOfSpeech), hoặc cấp độ CEFR.
- 🔄 Ôn tập thông minh: Hệ thống theo dõi tiến độ học và nhắc nhở ôn tập từ vựng dựa trên thời gian cần ôn (spaced repetition).
- 🎁 Gamification: Nhận điểm XP, thu thập thú cưng, và nhận thưởng khi hoàn thành nhiệm vụ học tập.
- 📂 Quản lý bộ từ vựng: Người dùng có thể tạo, chỉnh sửa, thêm/xóa từ vựng vào các bộ từ vựng, với phân quyền rõ ràng (Admin/User).
- 📊 Thống kê tiến trình: Theo dõi tiến độ học tập qua báo cáo và phân trang dữ liệu.
- 🐾 Bộ sưu tập thú cưng: Thu thập và quản lý thú cưng, tạo động lực học tập.
- 🔔 Thông báo thời gian thực: Sử dụng SignalR để gửi thông báo về tiến độ học hoặc các sự kiện trong ứng dụng.
- 🔒 Xác thực và phân quyền: Sử dụng JWT để xác thực người dùng và phân quyền (Admin/User).

---

## 🛠 Công nghệ sử dụng

- **Backend**: ASP.NET Core Web API (.NET 9), Entity Framework Core (ORM cho SQL Server), SQL Server (Cơ sở dữ liệu quan hệ), JWT Authentication (Xác thực người dùng), SignalR (Thông báo thời gian thực), Cloudinary (Lưu trữ và quản lý hình ảnh/pronunciation)
- **Frontend**: ReactJS (Vite, TypeScript), TailwindCSS (Giao diện)
- **Khác**: REST API (Giao tiếp client-server), CORS (Hỗ trợ truy cập từ localhost), Dependency Injection (Quản lý phụ thuộc trong ASP.NET Core)

---

## 🔑 Kiến trúc & Nguyên tắc
Dự án được thiết kế theo kiến trúc Modular Monolith, với các module chức năng riêng biệt (từ vựng, người dùng, thú cưng, phiên học, thông báo, v.v.), nhưng chạy trong một tiến trình duy nhất. Các Design Pattern và nguyên tắc được áp dụng:
- Design Patterns:
  - Repository Pattern: Tách biệt logic truy cập dữ liệu (VocabularyRepository, UserRepository) khỏi logic nghiệp vụ.
  - Service Pattern: Xử lý logic nghiệp vụ trong các service (VocabularyService, UserService).
  - Dependency Injection (DI): Quản lý phụ thuộc qua ASP.NET Core DI, sử dụng Scoped và Singleton lifetime.
  - DTO Pattern: Sử dụng các DTO (VocabularyDto, AdminVocabularyDto) để truyền dữ liệu an toàn và hiệu quả.
  - Unit of Work: Ngầm định qua Entity Framework Core với WordSoulDbContext.
- Nguyên tắc SOLID:
  - S (Single Responsibility): Mỗi lớp chỉ đảm nhận một nhiệm vụ (controller xử lý API, service xử lý logic, repository truy cập dữ liệu).
  - O (Open/Closed): Dễ dàng mở rộng thông qua interface (IVocabularyService, IVocabularyRepository) mà không cần sửa mã hiện có.
  - L (Liskov Substitution): Các lớp triển khai interface có thể thay thế lẫn nhau mà không làm hỏng hệ thống.
  - I (Interface Segregation): Interface được chia nhỏ, cụ thể cho từng chức năng.
  - D (Dependency Inversion): Các lớp phụ thuộc vào abstraction (interface) thay vì triển khai cụ thể.
- Modular Monolith:
  - Mã nguồn được tổ chức theo các chức năng (từ vựng, người dùng, thú cưng, v.v.) với các service/repository riêng biệt.
  - Mỗi module giao tiếp qua interface, đảm bảo tính độc lập và dễ bảo trì.
  - Dự án vẫn là một ứng dụng đơn khối, phù hợp cho quy mô nhỏ/vừa, dễ triển khai hơn microservices.
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
Dự án được xây dựng với các công nghệ và thư viện:
  - Backend: ASP.NET Core, Entity Framework Core, SQL Server, SignalR, Cloudinary
  - Frontend: React, Vite, TypeScript, TailwindCSS
  - Công cụ: Git, Visual Studio, VS Code

---

> GitHub [@trvannhanh](https://github.com/trvannhanh) &nbsp;&middot;&nbsp;

