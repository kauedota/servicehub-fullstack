import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Em desenvolvimento, o Vite repassa (proxy) tudo que começa com /api para a API .NET.
// Assim o frontend chama "/api/clientes" sem se preocupar com CORS ou porta.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5000',
      '/health': 'http://localhost:5000',
    },
  },
});
