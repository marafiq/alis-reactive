import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// Design-system CSS must build from the repo root so Tailwind sees the same C#
// sources as the shipped package. Keep emptyOutDir off; the CSS outputs share
// dist/ and a stale-file sweep can race the other asset build.
const repoRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  root: repoRoot,
  plugins: [tailwindcss()],
  build: {
    outDir: 'Alis.Reactive.Assets/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.Assets/design-system/app.css',
      output: {
        assetFileNames: 'design-system.dev.css',
      },
    },
  },
});
