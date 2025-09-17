import { BrowserRouter, Routes, Route } from 'react-router-dom';
import LearningSession from './features/learningSession/LearningSession';
import Home from './pages/Home';
import VocabularySetDetail from './features/vocabularySet/VocabularySetDetail';
import Login from './features/auth/Login';
import { AuthProvider } from './store/AuthContext';
import Register from './features/auth/Register';
import VocabularySet from './features/vocabularySet/VocabularySet';
import Pets from './features/pets/Pets';
import Community from './features/community/Community';
import UserDashboard from './pages/UserDashboard';
import DashboardLayout from './components/AdminDashboard/DashboardLayout';
import UserList from './components/AdminDashboard/UserList';
import UserDetail from './components/AdminDashboard/UserDetail';
import ActivityLog from './components/AdminDashboard/ActivityLog';
import VocabularySetList from './components/AdminDashboard/VocabularySetList';
import MainLayout from './layouts/MainLayout';
import PetList from './components/AdminDashboard/PetList';
import PetDetailPage from './features/pets/PetDetailPage';
import NoFooterLayout from './layouts/NoFooterLayout';
import CreateVocabularySet from './features/vocabularySet/CreateVocabularySet';


const App: React.FC = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/" element={<Home />} />
            
            <Route path="/vocabularySet/:id" element={<VocabularySetDetail />} />
            <Route path="/vocabularySet" element={<VocabularySet />} />
            <Route path="/vocabulary-sets/create" element={<CreateVocabularySet />} />
            <Route path='/community' element={<Community />} />
            <Route path='/home' element={<UserDashboard />} />
          </Route>

          <Route path="/learningSession/:id" element={<LearningSession />} />

          <Route element={<NoFooterLayout />}>
            <Route path='/pets' element={<Pets />} />
            <Route path='/pets/:id' element={<PetDetailPage />} />
          </Route>
          
          

          <Route path="/admin" element={<DashboardLayout />}>
            <Route index element={<UserList />} />
            <Route path="users/:userId" element={<UserDetail />} />
            <Route path="activities" element={<ActivityLog />} />
            <Route path='vocabulary-sets' element={<VocabularySetList />} />
            <Route path='pets' element={<PetList />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;