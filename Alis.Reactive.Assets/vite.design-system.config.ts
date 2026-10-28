import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// Design-system CSS must build from the repo root so Tailwind sees the same C#
// sources as the shipped package. Keep emptyOutDir off; the CSS outputs share
// dist/ and a stale-file sweep can race the other asset build.
//
// Two stylesheets, built together so they are never out of step:
//   design-system.dev.css         the design system (app.css)
//   design-system-fusion.dev.css  its look for Syncfusion components (fusion.css),
//                                 linked after the Fusion package's stylesheet
const repoRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  root: repoRoot,
  plugins: [tailwindcss()],
  build: {
    outDir: 'Alis.Reactive.Assets/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: {
        'design-system': 'Alis.Reactive.Assets/design-system/app.css',
        'design-system-fusion': 'Alis.Reactive.Assets/design-system/fusion.css',
      },
      output: {
        assetFileNames: '[name].dev.css',
      },
    },
  },
});
