import ReactDOM from 'react-dom/client';
import { CookiesProvider } from 'react-cookie';
import App from './App';
import { AppErrorBoundary } from './shared/components/ErrorBoundary';
import { ToastProvider } from './shared/toast';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <AppErrorBoundary>
    <ToastProvider>
      <CookiesProvider>
        <App />
      </CookiesProvider>
    </ToastProvider>
  </AppErrorBoundary>
);
