/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    open: true,
    proxy: {
      '/api/Eventos': {
        target: 'https://contenedor-evento.jollybay-7b230335.brazilsouth.azurecontainerapps.io',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path,
      },
      '/api/asientos': {
        target: 'https://contenedor-asiento.jollybay-7b230335.brazilsouth.azurecontainerapps.io',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
});
