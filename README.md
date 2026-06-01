<h1 align="center">
  <br>
  <a href="https://github.com/trvannhanh/vocamon-project">
    <img src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1780302586/Gemini_Generated_Image_hnp6j3hnp6j3hnp6-removebg-preview_eiarqy.png" alt="Vocamon" width="120">
  </a>
  <br>
  Vocamon
  <br>
</h1>

<h4 align="center">
  A gamified English vocabulary learning ecosystem powered by <b>SuperMemo-2 (SM-2)</b> SRS algorithm and <b>Real-time PvP</b> combat.
</h4>

<p align="center">
  <a href="https://dotnet.microsoft.com/download/dotnet/9.0">
    <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 9">
  </a>
  <a href="https://reactjs.org/">
    <img src="https://img.shields.io/badge/React-19.x-61DAFB?style=flat-square&logo=react" alt="React">
  </a>
  <a href="https://www.typescriptlang.org/">
    <img src="https://img.shields.io/badge/TypeScript-5.x-3178C6?style=flat-square&logo=typescript" alt="TypeScript">
  </a>
  <a href="https://expo.dev/">
    <img src="https://img.shields.io/badge/Expo-SDK%2056-000020?style=flat-square&logo=expo" alt="Expo SDK 56">
  </a>
  <a href="https://opensource.org/licenses/MIT">
    <img src="https://img.shields.io/badge/License-MIT-green.svg?style=flat-square" alt="License">
  </a>
</p>

---

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Core Algorithms](#core-algorithms)
- [Getting Started](#getting-started)
- [Database Seeding](#database-seeding)
- [Project Structure](#project-structure)
- [License](#license)

---

## Project Overview

**WordSoul** is a full-stack, cross-platform vocabulary learning ecosystem. It combines a clinically-proven **Spaced Repetition System (SM-2)** with a **Pokémon-inspired gamification** layer — pets, gym leaders, PvP duels, and achievements — to turn vocabulary memorization into an engaging long-term habit.

The design is grounded in **Self-Determination Theory (SDT)**: the platform nurtures *Competence* (SRS mastery), *Autonomy* (custom vocabulary sets), and *Relatedness* (groups and real-time battles).

The platform ships as four coordinated sub-projects:

| Sub-project | Description |
|---|---|
| `WordSoulApi` | ASP.NET Core 9 REST API + SignalR back-end |
| `wordsoul-app` | React 19 + Vite learner web application |
| `wordsoul-admin` | Next.js 16 back-office admin panel |
| `wordsoul-mobile` | React Native + Expo 56 mobile app |

---

## Architecture

```
vocamon-project/
├── WordSoulApi/        ← ASP.NET Core 9  (port 63982)
├── wordsoul-app/       ← React 19 + Vite  (port 5173)
├── wordsoul-admin/     ← Next.js 16        (port 3000)
├── wordsoul-mobile/    ← React Native / Expo
└── Report/             ← LaTeX thesis document
```

The back-end follows **Clean / Onion Architecture**:

```
WordSoulApi/
├── WordSoul.Domain/          ← Entities, enums, domain services
├── WordSoul.Application/     ← Use cases, interfaces, SRS logic
├── WordSoul.Infrastructure/  ← EF Core, repos, background jobs, external services
└── WordSoul.Api/             ← Controllers, SignalR Hubs, middleware
```

---

## Key Features

### Learning Engine
- **SM-2 Spaced Repetition** — adaptive review intervals per word based on recall quality
- **4-Stage Learning Sessions** — Flashcard → Fill-in-the-Blank → Multiple Choice → Listening
- **Active Recall** — timed answer pressure for deeper memory consolidation
- **Vocabulary Sets** — curated collections or user-created sets with reward pets

### Gamification
- **Pet Ecosystem** — collect & evolve Pokémon-style pets by completing learning milestones
- **Gym Circuit (PvE)** — defeat AI Gym Leaders to progress through difficulty tiers
- **Real-time PvP Arena** — duel other learners via SignalR `BattleHub`; damage calculated from answer accuracy × response speed
- **Achievement System** — milestone badges with AP rewards
- **Daily Quests** — recurring challenges driving daily retention
- **Item Shop** — purchasable power-ups and boosters

### Social & Admin
- **User Groups** — create or join study communities
- **Push Notifications** — Firebase-powered alerts (web + mobile)
- **Full Admin Panel** — manage users, content, battles, system health, logs, and configurations

---

## Tech Stack

### Backend — `WordSoulApi`

| Concern | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core |
| Database | Azure SQL / SQL Server via EF Core (Code First) |
| Real-time | SignalR (`BattleHub`, `NotificationHub`) |
| Auth | JWT Bearer + Google OAuth |
| Push Notifications | Firebase Admin SDK |
| Media Storage | Cloudinary |
| Logging | Serilog |
| API Docs | Scalar (OpenAPI) |
| Background Jobs | ASP.NET Core Hosted Services |

### Web App — `wordsoul-app`

| Concern | Technology |
|---|---|
| Framework | React 19 + TypeScript + Vite |
| Styling | Tailwind CSS v4 |
| Routing | React Router v7 |
| State | Redux Toolkit |
| HTTP | Axios |
| Real-time | @microsoft/signalr |
| Animations | Framer Motion |
| Charts | Recharts + Chart.js |
| Notifications | Firebase Web SDK |

### Admin Panel — `wordsoul-admin`

| Concern | Technology |
|---|---|
| Framework | Next.js 16 (App Router) |
| UI Library | Ant Design v6 |
| Styling | Tailwind CSS v4 |
| State | Zustand |
| Charts | Recharts |

### Mobile App — `wordsoul-mobile`

| Concern | Technology |
|---|---|
| Framework | React Native 0.85 + Expo SDK 56 |
| Navigation | React Navigation v7 |
| Styling | NativeWind (Tailwind CSS) |
| State | Zustand |
| Real-time | @microsoft/signalr |
| Auth | expo-auth-session |
| Secure Storage | expo-secure-store |
| Notifications | expo-notifications |

---

## Core Algorithms

### SuperMemo-2 (SM-2)

Calculates the next review interval from answer quality $q \in [0,5]$:

$$EF' = EF + \bigl(0.1 - (5-q)(0.08 + (5-q) \times 0.02)\bigr)$$

$$I(n) = I(n-1) \times EF', \quad I(1)=1,\ I(2)=6$$

Words with $q < 3$ are reset to interval 1 (lapse).

### Battle Damage

```
damage = basePower × typeEffectiveness × (accuracy_score / maxScore)
       × speedBonus(elapsedMs)
```

- **Type matrix** — Fire / Water / Grass / Electric elemental counters
- **Speed bonus** — diminishing multiplier as `elapsedMs` increases

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- SQL Server or Azure SQL (configure connection string in `WordSoulApi/WordSoul.Api/Appsettings/appsettings.Development.json`)
- Android Studio + AVD **or** Expo Go app (for mobile)

### 1. Backend

```powershell
cd WordSoulApi/WordSoul.Api
dotnet run
```

| Endpoint | URL |
|---|---|
| REST API | `http://localhost:63982` |
| Scalar (API docs) | `http://localhost:63982/scalar` |

### 2. Web App

```powershell
cd wordsoul-app
npm install
npm run dev       # http://localhost:5173
```

### 3. Admin Panel

```powershell
cd wordsoul-admin
npm install
npm run dev       # http://localhost:3000
```

### 4. Mobile App

```powershell
cd wordsoul-mobile
npm install
```

Create `wordsoul-mobile/.env`:

```env
# Android Emulator
EXPO_PUBLIC_API_URL=http://10.0.2.2:63982/api

# iOS Simulator
# EXPO_PUBLIC_API_URL=http://localhost:63982/api

# Physical device (same WiFi — replace with your machine's local IP)
# EXPO_PUBLIC_API_URL=http://192.168.1.xxx:63982/api
```

```powershell
npx expo start --android   # Android emulator
npx expo start --ios       # iOS simulator (macOS only)
npx expo start             # Expo Go / QR code
```

---

## Database Seeding

Python seed scripts live in `WordSoulApi/Scripts/`:

```powershell
cd WordSoulApi/Scripts
pip install -r requirements.txt

python seed_system_data.py      # Base config & system data
python seed_vocabularies.py     # ~200 vocabulary items
python seed_pokemon_gen1.py     # Generation 1 pets
python seed_pokemon_gen2.py     # Generation 2 pets
```

Each script writes a companion `.sql` file that can be applied via SSMS or Azure Data Studio.

---

## Project Structure

```
WordSoulApi/
├── WordSoul.Api/
│   ├── Controllers/        AuthController, BattleController, GymController, …
│   ├── Hubs/               BattleHub.cs, NotificationHub.cs
│   ├── Middlewares/
│   └── Services/
├── WordSoul.Application/   Use cases, SRS logic, service interfaces
├── WordSoul.Domain/        Entities (User, Vocabulary, Pet, BattleSession, …)
├── WordSoul.Infrastructure/EF Core DbContext, repositories, background jobs
└── Scripts/                Seed scripts

wordsoul-app/src/
├── features/               auth, battle, gym, learningSession, pets, vocabularySet, …
├── components/             Battle/, Pet/, VocabularySet/, UserDashboard/, …
├── store/                  Redux slices
└── services/               Axios API clients

wordsoul-admin/src/app/
├── (admin)/
│   ├── dashboard/
│   ├── users/, groups/, roles/
│   ├── vocabularies/, words/
│   ├── pets/, items/
│   ├── gyms/, battles/, pvp/
│   ├── quests/, notifications/
│   ├── analytics/
│   ├── system-logs/, system-health/
│   └── settings/
└── login/

wordsoul-mobile/src/
├── screens/                auth, home, learn, battle, pets, profile
├── navigation/             RootNavigator, AuthStack, MainTabs
├── contexts/               AuthContext
└── services/               API clients
```

---

## License

This project is licensed under the [MIT License](LICENSE).
