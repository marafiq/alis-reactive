import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';

// Fusion (Syncfusion EJ2) CSS pipeline.
// Entry:  Alis.Reactive.Assets/fusion/syncfusion.entry.css
// Output: Alis.Reactive.Assets/dist/css/syncfusion.dev.css
//
// Bundles @syncfusion/ej2/tailwind3.css (from npm) with the framework's
// Syncfusion overrides. `root` is pinned to the repo root so the @syncfusion/ej2
// node_modules import resolves identically to the former repo-root build.
// No @tailwindcss/vite plugin: the input is plain vendor CSS + overrides.
const repoRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  root: repoRoot,
  build: {
    outDir: 'Alis.Reactive.Assets/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.Assets/fusion/syncfusion.entry.css',
      output: {
        assetFileNames: 'syncfusion.dev.css',
      },
    },
  },
});
