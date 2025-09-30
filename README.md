<h1 align="center">
  <br>
  <a href="https://github.com/your-username/wordsoul"><img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png" alt="WordSoul" width="100"></a>
  <br>
  WordSoul
  <br>
</h1>

<h4 align="center">
  WordSoul — A web application for learning and reviewing English vocabulary with gamification (collecting pets, earning XP), incorporating scientific memory principles (spaced repetition + retrieval practice) for engaging and long-lasting learning.
</h4>

<p align="center">
  <a href="#project-overview">Project Overview</a> •
  <a href="#key-features">Key Features</a> •
  <a href="#architecture--design-principles">Architecture & Design Principles</a> •
  <a href="#main-interface">Main Interface</a> •
  <a href="#installation-requirements">Installation Requirements</a> •
  <a href="#how-to-use">How to Run the Project</a> •
  <a href="#credits">Credits</a>
</p>

---

## Project Overview

WordSoul is a web application aimed at:
- Combining scientific memory principles (SRS — spaced repetition; retrieval practice) with gamification to boost learning motivation.
- Users learn through vocabulary sets, with each word progressing through a sequence of question types (Flashcard → Fill-in-Blank → MultipleChoice → Listening) and is considered mastered upon completing the sequence in a single session.
- The system includes a pet mechanism: collect, upgrade, and evolve pets using XP/AP to enhance user engagement.

---

## Key Features

- 📚 Smart Learning & Review: SRS-based scheduling, automatic review calendar.
- 🔁 4 Question Types in a Session: Flashcard, Fill-in-Blank, MultipleChoice, Listening.
- 🧠 State Machine for Each Word: Words transition between states based on correct/incorrect answers.
- 🎮 Gamification: XP, pet collection and upgrades, reward animations.
- 🗂 Vocabulary Set Management: Create, edit, add/remove words, permissions (owned/public).
- 📊 Dashboard & Statistics: Proficiency charts, words due for review, learning progress.
- 🏆 Leaderboard: Player ranking based on points.
- 🔔 Realtime Notifications: SignalR for pushing event notifications (learning sessions, pet upgrades, etc.).
- 🔒 Authentication & Authorization: JWT-based authentication, Admin/User roles.

---

## Architecture & Design Principles
- Architecture: Modular Monolith (modules by function: User, Vocabulary, LearningSession, Pet, Reward, Notification).
- Backend: ASP.NET Core (.NET 9 / .NET 8 compatible), Entity Framework Core (Code First), SQL Server.
- Frontend: React (Vite + TypeScript), TailwindCSS.
- Realtime: SignalR.
- Storage Media: Cloudinary (images / audio pronunciation).
- Patterns & Principles: Repository + Service, DTO, Dependency Injection, Unit of Work (DbContext), SOLID.

---

## Main Interface
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

## Installation Requirements

Before running the project, install:

- [Node.js](https://nodejs.org/) >= 18  
- [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/)  
- [.NET SDK](https://dotnet.microsoft.com/download) >= 8.0  
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  

## How To Use

From the command line:

```bash
# Clone this repository
$ git clone https://github.com/your-username/wordsoul

# Go into the repository
$ cd wordsoul

# Backend
$ cd wordsoulapi
$ dotnet restore
$ dotnet ef database update   # create database
$ dotnet run

# Frontend
$ cd wordsoul-app
$ npm install
$ npm run dev
```

## Credit
The project was built with the following technologies and libraries:
- Backend: ASP.NET Core, Entity Framework Core, SQL Server, SignalR, Cloudinary
- Frontend: React, Vite, TypeScript, TailwindCSS
- Tools: Git, Visual Studio, VS Code
