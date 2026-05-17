import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// Sandbox CSS pipeline.
// Entry:  Alis.Reactive.SandboxApp/Styles/sandbox.css
// Output: Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css
//
// `root` is pinned to the repo root: Tailwind's content scan is rooted there,
// so moving this config into the workspace does not change which files are
// scanned and the built CSS is identical to the repo-root build. `input` and
// `outDir` stay repo-root-relative for the same reason. emptyOutDir is false
// because wwwroot/css/ also holds hand-written assets.
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
