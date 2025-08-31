import ReactDOM from 'react-dom/client';
import { CookiesProvider } from 'react-cookie';
import { AuthProvider } from './store/AuthContext';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <CookiesProvider>
    <AuthProvider>
      <App />
    </AuthProvider>
  </CookiesProvider>
);