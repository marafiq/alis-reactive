import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// Keep the sandbox CSS build rooted at the repo so Tailwind scans the same files as CI.
// emptyOutDir stays false because wwwroot/css also holds hand-written sandbox assets.
const repoRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  root: repoRoot,
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
