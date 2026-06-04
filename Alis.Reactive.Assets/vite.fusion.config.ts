import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';

// Fusion CSS bundles Syncfusion EJ2 Tailwind CSS with the framework overrides.
// Build from the repo root so the @syncfusion/ej2 import resolves through the
// same node_modules tree as the package build. No Tailwind plugin is needed here;
// this input is vendor CSS plus overrides.
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
