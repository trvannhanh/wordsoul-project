
<h1 align="center">
  <br>
  <a href="https://github.com/your-username/wordsoul"><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756452551/coin-logo_ysauhp.png" alt="WordSoul" width="100"></a>
  <br>
  WordSoul
  <br>
</h1>

<h4 align="center">
  WordSoul — Ứng dụng web học & ôn từ vựng tiếng Anh có gamification (thu thập thú cưng, nhận XP), kết hợp các nguyên lý khoa học về ghi nhớ (spaced repetition + retrieval practice) để vừa học vừa chơi — học mà vẫn nhớ lâu.
</h4>

<p align="center">
  <a href="#tổng-quan-dự-án">Tổng quan dự án</a> •
  <a href="#tính-năng-chính">Tính năng chính</a> •
  <a href="#kiến-trúc--nguyên-tắc-thiết-kế">Kiến trúc & Nguyên tắc thiết kế</a> •
  <a href="#giao-diện-chính">Giao diện chính</a> •
  <a href="#-yêu-cầu-cài-đặt">Yêu cầu cài đặt</a> •
  <a href="#how-to-use">Cách chạy project</a> •
  <a href="#credits">Credits</a>
</p>

---

## Tổng quan dự án

WordSoul là một ứng dụng web hướng tới mục tiêu:
- Kết hợp các nguyên lý khoa học về ghi nhớ (SRS — spaced repetition; retrieval practice) với cơ chế gamification để tăng động lực học.
- Người dùng học theo bộ từ, mỗi từ trải qua chuỗi dạng bài (Flashcard → Fill-in-Blank → MultipleChoice → Listening) và được coi là hoàn thành khi vượt qua toàn bộ chuỗi này trong một phiên học.
- Hệ thống còn có cơ chế pet (thú cưng): thu thập, nâng cấp, tiến hóa — dùng XP/AP để tăng tính gắn kết.
---

## Tính năng chính

- 📚 Học & ôn tập thông minh: SRS-based scheduling, lịch ôn tự động.
- 🔁 4 loại câu hỏi trong phiên học: Flashcard, Fill-in-Blank, MultipleChoice, Listening.
- 🧠 State machine cho mỗi từ: từ di chuyển giữa các trạng thái theo kết quả đúng/sai.
- 🎮 Gamification: XP, thu thập & nâng cấp thú cưng, animation phần thưởng.
- 🗂 Quản lý bộ từ: tạo, chỉnh sửa, thêm/xóa từ, phân quyền (sở hữu/công khai).
- 📊 Dashboard & thống kê: biểu đồ trình độ, số từ cần ôn, tiến trình học.
- 🏆 Bảng xếp hạng: xếp hạng người chơi theo điểm.
- 🔔 Realtime notifications: SignalR để push thông báo sự kiện (phiên học, nâng cấp pet...).
- 🔒 Xác thực & phân quyền: JWT-based authentication, role Admin/User.

---

## Kiến trúc & Nguyên tắc thiết kế
- Kiến trúc: Modular Monolith (module theo chức năng: User, Vocabulary, LearningSession, Pet, Reward, Notification).
- Backend: ASP.NET Core (.NET 9 / .NET 8 tương thích), Entity Framework Core (Code First), SQL Server.
- Frontend: React (Vite + TypeScript), TailwindCSS.
- Realtime: SignalR.
- Storage media: Cloudinary (ảnh / audio pronunciation).
- Patterns & Principles: Repository + Service, DTO, Dependency Injection, Unit of Work (DbContext), SOLID.
---

## Giao diện chính
<p align="center">
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452852/Final_IF_Session_tjkrvk.png" alt="Session" width="700" style="margin:6px; border-radius:8px;"></a>
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452853/Final_IF_Static_ofkoge.png" alt="Dashboard" width="700" style="margin:6px; border-radius:8px;"></a>

  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452855/IF_SetList_jzmxrs.png" alt="Set List" width="700" style="margin:6px; border-radius:8px;"></a>
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452854/IF_SetDetail_fa3dgl.png" alt="Set Detail" width="700" style="margin:6px; border-radius:8px;"></a>
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452851/Final_IF_Set1_vnlpw6.png" alt="Create Set - Step 1" width="700" style="margin:6px; border-radius:8px;"></a>
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452851/Final_IF_Set2_cwmmdw.png" alt="Create Set - Step 2" width="700" style="margin:6px; border-radius:8px;"></a>


  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452837/IF_PetList_can1tg.png" alt="Pet List" width="500" style="margin:6px; border-radius:8px;"></a>
  <a href=""><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1758452853/Final_IF_Pet_yqsneg.png" alt="Pet Detail" width="500" style="margin:6px; border-radius:8px;"></a>
</p>
  

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

