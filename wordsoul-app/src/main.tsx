import ReactDOM from 'react-dom/client';
import { CookiesProvider } from 'react-cookie';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <CookiesProvider>
      <App />
  </CookiesProvider>
);