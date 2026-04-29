<h1 align="center">
  <br>
  <a href="https://github.com/trvannhanh/vocamon-project">
    <img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png" alt="Vocamon" width="120">
  </a>
  <br>
  Vocamon (WordSoul)
  <br>
</h1>

<h4 align="center">
  A gamified English vocabulary learning ecosystem powered by <b>SuperMemo-2 (SM-2)</b> algorithm and <b>Real-time PvP</b> combat.
</h4>

<p align="center">
  <a href="https://dotnet.microsoft.com/download/dotnet/8.0">
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 8">
  </a>
  <a href="https://reactjs.org/">
    <img src="https://img.shields.io/badge/React-18.x-61DAFB?style=flat-square&logo=react" alt="React">
  </a>
  <a href="https://www.typescriptlang.org/">
    <img src="https://img.shields.io/badge/TypeScript-5.x-3178C6?style=flat-square&logo=typescript" alt="TypeScript">
  </a>
  <a href="https://opensource.org/licenses/MIT">
    <img src="https://img.shields.io/badge/License-MIT-green.svg?style=flat-square" alt="License">
  </a>
</p>

---

## 📖 Table of Contents
- [Project Overview](#-project-overview)
- [Key Features](#-key-features)
- [Tech Stack](#-tech-stack)
- [System Architecture](#-system-architecture)
- [Core Algorithms](#-core-algorithms)
- [Installation](#-installation)
- [Usage](#-usage)
- [License](#-license)

---

## 🌟 Project Overview

**Vocamon** is an innovative web application designed to solve the challenge of long-term English vocabulary retention. [cite_start]By integrating **Spaced Repetition Systems (SRS)** with a **Pokemon-inspired Gamification** model, Vocamon transforms the tedious process of memorization into an engaging journey of growth and competition.

[cite_start]The project satisfies the three core psychological needs of **Self-Determination Theory (SDT)**: Competence, Autonomy, and Relatedness.

---

## ✨ Key Features

### 🧠 Cognitive Learning
- [cite_start]**SM-2 Algorithm Implementation**: Personalized review schedules based on memory strength and recall quality.
- [cite_start]**4-Stage Learning Session**: A word progresses through Flashcard → Fill-in-the-Blank → Multiple Choice → Listening stages to ensure multi-sensory mastery.
- [cite_start]**Active Recall Engine**: Forces the brain to retrieve information under time pressure for deeper memory traces.

### 🎮 Gamification & Interaction
- **Pet Ecosystem**: Catch, train, and evolve virtual pets. [cite_start]Your learning progress directly fuels your pet's evolution.
- **Real-time Combat (Battle Mode)**:
  - [cite_start]**PvE (Gym Circuit)**: Challenge AI leaders to test your vocabulary mastery.
  - [cite_start]**PvP (Arena)**: Duel other learners in real-time matches using SignalR technology.
- [cite_start]**Dynamic Reward System**: Earn Experience Points (XP) and Achievement Points (AP) with a 5x bonus for review sessions to encourage discipline.

---

## 🛠 Tech Stack

### Backend
- [cite_start]**Framework**: .NET 8 / ASP.NET Core Web API
- [cite_start]**Real-time**: SignalR for low-latency combat 
- [cite_start]**ORM**: Entity Framework Core (Code First)
- [cite_start]**Database**: SQL Server

### Frontend
- [cite_start]**Library**: React 18 (Vite + TypeScript)
- [cite_start]**Styling**: Tailwind CSS (Utility-first) 
- [cite_start]**State Management**: Redux 

### Infrastructure
- [cite_start]**Media Storage**: Cloudinary (for pronunciation and images)
- [cite_start]**Auth**: JWT (JSON Web Token) with Middleware Pipeline

---

## 🏗 System Architecture

[cite_start]The system follows **Onion Architecture** (Clean Architecture) to decouple business logic from infrastructure.

* [cite_start]**Domain Layer**: Core entities (User, Vocabulary, Pet) and SM-2 logic.
* [cite_start]**Application Layer**: MediatR commands, DTOs, and mapping logic.
* [cite_start]**Infrastructure Layer**: Data persistence, external service implementations.
* [cite_start]**Presentation Layer**: RESTful Controllers and SignalR Hubs.

---

## 🧬 Core Algorithms

### SuperMemo-2 (SM-2)
Calculates the next review interval based on feedback quality ($q$ from 0-5):
- [cite_start]**Easiness Factor (EF)**: $EF' = EF + (0.1 - (5-q) \times (0.08 + (5-q) \times 0.02)).
- [cite_start]**Interval ($I$)**: $I(n) = I(n-1) \times EF$.

### Battle Logic
- [cite_start]**Elemental Matrix**: Damage multipliers based on type effectiveness (Fire > Grass > Water > Fire).
- [cite_start]**Response Score**: Damage is calculated by combining answer accuracy with reaction time (ElapsedMs).

---

## 🚀 Installation

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### 1. Database Setup
Update the connection string in `appsettings.json` within the API project, then run:
```bash
dotnet ef database update
### 2. Backend Setup
```bash
cd Vocamon.Api
dotnet restore
dotnet run
### 3. Frontend Setup
```bash
cd Vocamon.UI
npm install
npm run dev

## 🖥 Usage
1. Sign Up: Create an account to start your journey.
2. Create Set: Build your personalized vocabulary set with specific topics.
3. Learn: Complete the 4-stage session to "catch" your first pet.
4. Battle: Enter the Arena to test your speed and accuracy against others.

## 📜 License
This project is licensed under the MIT License.
