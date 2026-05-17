import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// Design-system CSS pipeline.
// Entry:  Alis.Reactive.Assets/design-system/app.css
// Output: Alis.Reactive.Assets/dist/css/design-system.dev.css
//
// `root` is pinned to the repo root so app.css's `@source` C# scan and Tailwind
// content detection resolve identically to the former repo-root build — moving
// this config into the workspace does not change the built CSS. emptyOutDir is
// false so a stale-file sweep never races other writers into the dist/ tree.
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
