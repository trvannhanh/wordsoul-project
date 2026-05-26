# 🚀 Hướng dẫn khởi chạy dự án WordSoul

## Tổng quan kiến trúc

```
vocamon-project/
├── WordSoulApi/          ← Backend ASP.NET Core (port 63982)
├── wordsoul-app/         ← Web App React + Vite (port 5173)
├── wordsoul-admin/       ← Admin Panel Next.js (port 3000)
└── wordsoul-mobile/      ← Mobile App React Native + Expo ← [MỚI]
```

---

## 1. Backend — WordSoulApi

### Yêu cầu
- .NET 9 SDK
- SQL Server hoặc kết nối Azure SQL đã có sẵn trong `appsettings.Development.json`

### Khởi chạy
```powershell
cd WordSoulApi/WordSoul.Api
dotnet run
```
API chạy tại: **`http://localhost:63982`**

> Swagger UI: `http://localhost:63982/swagger`

---

## 2. Web App — wordsoul-app

### Yêu cầu
- Node.js 18+

### Khởi chạy
```powershell
cd wordsoul-app
npm install
npm run dev
```
Chạy tại: **`http://localhost:5173`**

---

## 3. Admin Panel — wordsoul-admin

### Yêu cầu
- Node.js 18+

### Khởi chạy
```powershell
cd wordsoul-admin
npm install
npm run dev
```
Chạy tại: **`http://localhost:3000`**

---

## 4. Mobile App — wordsoul-mobile ⭐

### Yêu cầu
- Node.js 18+
- Expo CLI: `npm install -g expo-cli` (hoặc dùng `npx expo`)
- Android: Android Studio + Android Emulator (API 33+) **HOẶC** Expo Go app trên điện thoại thật
- iOS: Xcode + iOS Simulator (macOS only) **HOẶC** Expo Go app

### Cài đặt dependencies (đã thực hiện)
```powershell
cd wordsoul-mobile
npm install
```

### Cấu hình môi trường

File `.env` đã được tạo sẵn. Chỉnh `EXPO_PUBLIC_API_URL` tùy môi trường:

| Môi trường | URL |
|---|---|
| Android Emulator | `http://10.0.2.2:63982/api` ✅ (default) |
| iOS Simulator | `http://localhost:63982/api` |
| Điện thoại thật (cùng WiFi) | `http://<IP-máy-tính>:63982/api` |
| Điện thoại thật (tunneling) | Dùng Expo tunnel (xem bên dưới) |

```env
# wordsoul-mobile/.env
EXPO_PUBLIC_API_URL=http://10.0.2.2:63982/api
```

### Khởi chạy

#### Android Emulator
```powershell
cd wordsoul-mobile
npx expo start --android
```
> Yêu cầu Android Studio đang chạy với một AVD đã tạo sẵn.

#### iOS Simulator (chỉ macOS)
```powershell
npx expo start --ios
```

#### Điện thoại thật qua Expo Go (cần cùng WiFi)
```powershell
# Bước 1: Lấy IP máy tính
ipconfig  # Windows → IPv4 Address

# Bước 2: Cập nhật .env
# EXPO_PUBLIC_API_URL=http://192.168.1.xxx:63982/api

# Bước 3: Đảm bảo backend bind trên tất cả interfaces
# Trong launchSettings.json: "applicationUrl": "http://0.0.0.0:63982"

# Bước 4: Chạy Expo
npx expo start
# Scan QR code bằng Expo Go app
```

#### Điện thoại thật qua Tunnel (không cần cùng WiFi)
```powershell
npx expo start --tunnel
# Cần cài: npm install -g @expo/ngrok
```

### Lệnh hữu ích
```powershell
# Xóa cache Metro và khởi động lại
npx expo start --clear

# Build APK Android (dev)
npx expo run:android

# Build IPA iOS (dev, chỉ macOS)
npx expo run:ios

# Kiểm tra TypeScript
npx tsc --noEmit

# Xem log thiết bị
npx expo start --android 2>&1
```

---

## 5. Chạy toàn bộ stack cùng lúc (Windows PowerShell)

Mở **4 cửa sổ terminal** riêng biệt:

```powershell
# Terminal 1 — Backend
cd WordSoulApi/WordSoul.Api ; dotnet run

# Terminal 2 — Web App
cd wordsoul-app ; npm run dev

# Terminal 3 — Admin Panel
cd wordsoul-admin ; npm run dev

# Terminal 4 — Mobile
cd wordsoul-mobile ; npx expo start --android
```

---

## 6. Cấu trúc Mobile App

```
wordsoul-mobile/
├── App.tsx                    ← Entry point
├── global.css                 ← NativeWind base styles
├── babel.config.js            ← NativeWind v4 config
├── metro.config.js            ← Metro bundler config
├── tailwind.config.js         ← Tailwind theme (primary/secondary/accent)
├── .env                       ← API URL
└── src/
    ├── contexts/
    │   ├── AuthContext.ts     ← useAuth() hook
    │   └── AuthProvider.tsx   ← JWT + SecureStore
    ├── helpers/
    │   └── authHelpers.ts     ← Token cache + SecureStore utils
    ├── navigation/
    │   ├── RootNavigator.tsx  ← Auth gate
    │   ├── AuthStack.tsx      ← Login / Register / Onboarding
    │   └── MainTabs.tsx       ← 5 bottom tabs
    ├── services/
    │   ├── api.ts             ← axios instances + endpoints
    │   ├── auth.ts            ← login / register / refresh
    │   ├── user.ts            ← profile / progress / leaderboard
    │   ├── vocabularySet.ts   ← vocab sets CRUD
    │   ├── learningSession.ts ← sessions / quiz / answers
    │   ├── pet.ts             ← pets management
    │   ├── gym.ts             ← gym leaders / battle
    │   ├── dailyQuest.ts      ← daily quests
    │   ├── achievement.ts     ← achievements
    │   └── notification.ts    ← notifications
    ├── types/                 ← TypeScript DTOs (10 files)
    ├── components/
    │   ├── ui/
    │   │   ├── Button.tsx     ← variants: primary/secondary/outline/danger/ghost
    │   │   ├── Card.tsx
    │   │   └── ProgressBar.tsx← animated with Reanimated v4
    │   └── layout/
    │       └── ScreenContainer.tsx
    └── screens/
        ├── auth/              ← Login / Register / Onboarding
        ├── home/              ← HomeScreen / LeaderboardScreen
        ├── learn/             ← VocabSetList / VocabSetDetail / LearningSession
        ├── pets/              ← PetsScreen / PetDetailScreen
        ├── battle/            ← GymMapScreen / ArenaBattle / PvpLobby / PvpBattle
        └── profile/           ← Profile / Achievement / DailyQuest / Settings
```

---

## 7. Luồng Authentication

```
App khởi động
    ↓
initTokenCache() — đọc SecureStore → memory cache
    ↓
Có token? → getCurrentUser() → vào MainTabs
Không token? → AuthStack (Login)
    ↓ (Login)
POST /auth/login → {accessToken, refreshToken}
    ↓
Lưu vào SecureStore + memory cache
    ↓
GET /users/me → UserDto → vào MainTabs
    ↓ (Token hết hạn)
401 response → auto refresh via /auth/refresh-token
```

---

## 8. Lưu ý quan trọng

| Vấn đề | Giải pháp |
|---|---|
| Android Emulator không kết nối được API | Dùng `10.0.2.2` thay vì `localhost` trong `.env` |
| iOS Simulator không kết nối | Dùng `localhost` hoặc IP WiFi |
| Điện thoại thật không kết nối | Dùng IP WiFi của máy tính, đảm bảo cùng mạng |
| Backend không nhận requests từ thiết bị | Thêm `http://0.0.0.0:63982` vào `applicationUrl` trong `launchSettings.json` |
| NativeWind classes không áp dụng | Chạy `npx expo start --clear` để xóa Metro cache |
| Expo cache cũ gây lỗi | Xóa `.expo/` folder và `node_modules/.cache/` |
| `@expo/vector-icons` lỗi | Đã được cài trong `package.json` |

---

## 9. Kiểm tra sức khỏe project

```powershell
cd wordsoul-mobile

# TypeScript (0 errors)
npx tsc --noEmit

# Kiểm tra packages
npm ls --depth=0

# Audit security
npm audit
```

---

*Cập nhật lần cuối: May 24, 2026 — Phase 8 hoàn thành (0 TypeScript errors)*
