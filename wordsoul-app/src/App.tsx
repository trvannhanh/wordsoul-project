import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Header from './layouts/Header';
import LearningSession from './features/learningSession/LearningSession';
import Home from './pages/Home';
import VocabularySetDetail from './features/vocabularySet/VocabularySetDetail';
import Login from './features/auth/Login';
import { AuthProvider } from './store/AuthContext';
import Register from './features/auth/Register';
import VocabularySet from './features/vocabularySet/VocabularySet';
import Footer from './layouts/Footer';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Header />
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<Home />} />
          <Route path="/learningSession/:id" element={<LearningSession />} />
          <Route path="/vocabularySet/:id" element={<VocabularySetDetail />} />
          <Route path="/vocabularySet" element={<VocabularySet />} />
        </Routes>
        <Footer />
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;