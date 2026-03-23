import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    port: 4500,
    proxy: {
      '/api': 'http://localhost:4501',
      '/ws': { target: 'ws://localhost:4501', ws: true },
    },
  },
  build: {
    outDir: 'dist',
  },
});
