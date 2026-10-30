import { defineConfig } from 'vite';
import { fileURLToPath } from 'node:url';

// Fusion CSS bundles the Syncfusion EJ2 Tailwind3 theme with the styling of Fusion's own markup.
// Build from the repo root so the @syncfusion/ej2 import resolves through the
// same node_modules tree as the package build. No Tailwind plugin is needed here;
// this input is vendor CSS plus plain CSS. The output keeps its handoff name, syncfusion.dev.css.
const repoRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  root: repoRoot,
  build: {
    outDir: 'Alis.Reactive.Assets/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.Assets/fusion/fusion.entry.css',
      output: {
        assetFileNames: 'syncfusion.dev.css',
      },
    },
  },
});
