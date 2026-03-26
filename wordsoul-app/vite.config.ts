import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],

  base: '/',

  server: {
    proxy: {
      // Forward tất cả /api request sang backend (tránh CORS + self-signed cert)
      '/api': {
        target: 'https://localhost:7272',
        changeOrigin: true,
        secure: false, // bỏ qua self-signed certificate ở local
      },
      // Forward SignalR hub
      '/notificationHub': {
        target: 'https://localhost:7272',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
      '/battleHub': {
        target: 'https://localhost:7272',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
