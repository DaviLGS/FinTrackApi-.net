import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// O front chama /api/... e o Vite repassa para a API (docker compose, porta 8080).
// Assim não é preciso configurar CORS na API para testar.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': process.env.API_URL ?? 'http://localhost:8080',
    },
  },
})
