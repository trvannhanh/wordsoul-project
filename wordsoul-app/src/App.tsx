import { BrowserRouter, Routes, Route } from 'react-router-dom';
import LearningSession from './features/learningSession/LearningSession';
import Home from './pages/Home';
import VocabularySetDetail from './features/vocabularySet/VocabularySetDetail';
import Login from './features/auth/Login';
import Register from './features/auth/Register';
import VocabularySet from './features/vocabularySet/VocabularySet';
import Pets from './features/pets/Pets';
import Community from './features/community/Community';
import UserDashboard from './pages/UserDashboard';
import DashboardLayout from './pages/AdminDashboardLayout';
import UserList from './components/AdminDashboard/UserList';
import UserDetail from './components/AdminDashboard/UserDetail';
import ActivityLog from './components/AdminDashboard/ActivityLog';
import VocabularySetList from './components/AdminDashboard/VocabularySetList';
import MainLayout from './layouts/MainLayout';
import PetList from './components/AdminDashboard/PetList';
import PetDetailPage from './features/pets/PetDetailPage';
import NoFooterLayout from './layouts/NoFooterLayout';
import CreateVocabularySet from './features/vocabularySet/CreateVocabularySet';
import ProfilePage from './features/userProfile/ProfilePage';
import GymMap from './features/gymLeader/GymMap';
import GymDetail from './features/gymLeader/GymDetail';
import GymResult from './features/gymLeader/GymResult';
import PetSelector from './features/battle/PetSelector';
import BattleArena from './features/battle/BattleArena';
import BattleArenaResult from './features/battle/BattleArenaResult';
import PvpLobby from './features/battle/PvpLobby';
import PvpPetSelector from './features/battle/PvpPetSelector';
import PvpBattleArena from './features/battle/PvpBattleArena';
import PvpBattleResult from './features/battle/PvpBattleResult';
import { AuthProvider } from './store/AuthProvider';


const App: React.FC = () => {
  return (

    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/" element={<Home />} />

            <Route path="/vocabularySet/:id" element={<VocabularySetDetail />} />
            <Route path="/vocabularySet" element={<VocabularySet />} />
            <Route path="/vocabulary-sets/create" element={<CreateVocabularySet />} />

            <Route path='/home' element={<UserDashboard />} />
            <Route path='/profile' element={<ProfilePage />} />

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
          <Route path="/pvp/arena/:sessionId" element={<PvpBattleArena />} />
          <Route path="/pvp/arena/:sessionId/result" element={<PvpBattleResult />} />

          <Route element={<NoFooterLayout />}>
            <Route path='/pets' element={<Pets />} />
            <Route path='/pets/:id' element={<PetDetailPage />} />
            <Route path='/community' element={<Community />} />
          </Route>



          <Route path="/admin" element={<DashboardLayout />}>
            <Route index element={<UserList />} />
            <Route path="users/:userId" element={<UserDetail />} />
            <Route path="activities" element={<ActivityLog />} />
            <Route path='vocabulary-sets' element={<VocabularySetList />} />
            <Route path='pets' element={<PetList />} />
          </Route>
        </Routes>

      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;