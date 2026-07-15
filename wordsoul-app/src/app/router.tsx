import { lazy, Suspense } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../store/AuthProvider';
import MainLayout from '../layouts/MainLayout';
import NoFooterLayout from '../layouts/NoFooterLayout';

const ActivityLog = lazy(() => import('../features/adminDashboard/components/ActivityLog'));
const PetList = lazy(() => import('../features/adminDashboard/components/PetList'));
const UserDetail = lazy(() => import('../features/adminDashboard/components/UserDetail'));
const UserList = lazy(() => import('../features/adminDashboard/components/UserList'));
const VocabularySetList = lazy(() => import('../features/adminDashboard/components/VocabularySetList'));
const DashboardLayout = lazy(() => import('../features/adminDashboard/AdminDashboardLayout'));
const Home = lazy(() => import('../pages/Home'));
const UserDashboard = lazy(() => import('../pages/UserDashboard'));
const GoogleCallback = lazy(() => import('../features/auth/GoogleCallback'));
const Login = lazy(() => import('../features/auth/Login'));
const Register = lazy(() => import('../features/auth/Register'));
const BattleArena = lazy(() => import('../features/battle/BattleArena'));
const BattleArenaResult = lazy(() => import('../features/battle/BattleArenaResult'));
const PetSelector = lazy(() => import('../features/battle/PetSelector'));
const PvpBattleArena = lazy(() => import('../features/battle/PvpBattleArena'));
const PvpBattleResult = lazy(() => import('../features/battle/PvpBattleResult'));
const PvpLobby = lazy(() => import('../features/battle/PvpLobby'));
const PvpMatchmaking = lazy(() => import('../features/battle/PvpMatchmaking'));
const PvpPetSelector = lazy(() => import('../features/battle/PvpPetSelector'));
const Community = lazy(() => import('../features/community/Community'));
const GymDetail = lazy(() => import('../features/gymLeader/GymDetail'));
const GymMap = lazy(() => import('../features/gymLeader/GymMap'));
const GymResult = lazy(() => import('../features/gymLeader/GymResult'));
const LearningSession = lazy(() => import('../features/learningSession/LearningSession'));
const PetDetailPage = lazy(() => import('../features/pets/PetDetailPage'));
const Pets = lazy(() => import('../features/pets/Pets'));
const PronunciationPetSelect = lazy(() => import('../features/pronunciation/PronunciationPetSelect'));
const PronunciationSession = lazy(() => import('../features/pronunciation/PronunciationSession'));
const ProfilePage = lazy(() => import('../features/userProfile/ProfilePage'));
const CreateVocabularySet = lazy(() => import('../features/vocabularySet/CreateVocabularySet'));
const VocabularySet = lazy(() => import('../features/vocabularySet/VocabularySet'));
const VocabularySetDetail = lazy(() => import('../features/vocabularySet/VocabularySetDetail'));

const routeFallback = (
  <div className="min-h-screen flex items-center justify-center background-color text-color font-pixel text-xs">
    Loading...
  </div>
);

export function AppRouter() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Suspense fallback={routeFallback}>
          <Routes>
            <Route element={<MainLayout />}>
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/auth/callback" element={<GoogleCallback />} />
              <Route path="/" element={<Home />} />

              <Route path="/vocabularySet/:id" element={<VocabularySetDetail />} />
              <Route path="/vocabularySet" element={<VocabularySet />} />
              <Route path="/vocabulary-sets/create" element={<CreateVocabularySet />} />

              <Route path="/home" element={<UserDashboard />} />
              <Route path="/profile" element={<ProfilePage />} />

              <Route path="/gym" element={<GymMap />} />
              <Route path="/gym/:gymId" element={<GymDetail />} />
            </Route>

            <Route path="/learningSession/:id" element={<LearningSession />} />
            <Route path="/gym/battle/:sessionId/result" element={<GymResult />} />
            <Route path="/gym/:gymId/pets" element={<PetSelector />} />
            <Route path="/arena/:sessionId" element={<BattleArena />} />
            <Route path="/arena/:sessionId/result" element={<BattleArenaResult />} />
            <Route path="/pvp" element={<PvpLobby />} />
            <Route path="/pvp/pets" element={<PvpPetSelector />} />
            <Route path="/pvp/matchmaking" element={<PvpMatchmaking />} />
            <Route path="/pvp/arena/:sessionId" element={<PvpBattleArena />} />
            <Route path="/pvp/arena/:sessionId/result" element={<PvpBattleResult />} />

            <Route path="/pronunciation" element={<PronunciationPetSelect />} />
            <Route path="/pronunciation/session" element={<PronunciationSession />} />

            <Route element={<NoFooterLayout />}>
              <Route path="/pets" element={<Pets />} />
              <Route path="/pets/:id" element={<PetDetailPage />} />
              <Route path="/community" element={<Community />} />
            </Route>

            <Route path="/admin" element={<DashboardLayout />}>
              <Route index element={<UserList />} />
              <Route path="users/:userId" element={<UserDetail />} />
              <Route path="activities" element={<ActivityLog />} />
              <Route path="vocabulary-sets" element={<VocabularySetList />} />
              <Route path="pets" element={<PetList />} />
            </Route>
          </Routes>
        </Suspense>
      </AuthProvider>
    </BrowserRouter>
  );
}