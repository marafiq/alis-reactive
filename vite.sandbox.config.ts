import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

// Sandbox CSS pipeline.
// Entry:  Alis.Reactive.SandboxApp/Styles/sandbox.css
// Output: Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css
//
// emptyOutDir is false because wwwroot/css/ also holds hand-written assets.
export default defineConfig({
  plugins: [tailwindcss()],
  build: {
    outDir: 'Alis.Reactive.SandboxApp/wwwroot/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.SandboxApp/Styles/sandbox.css',
      output: {
        assetFileNames: 'sandbox.css',
      },
    },
  },
});
