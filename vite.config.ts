import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

// Framework CSS pipeline.
// Entry:  Alis.Reactive.Assets/Styles/app.css
// Output: Alis.Reactive.Assets/dist/css/design-system.dev.css
//
// emptyOutDir is false because Alis.Reactive.Assets/dist/ also holds the
// esbuild JS bundle (dist/scripts/alis-reactive.dev.js); Vite must not wipe it.
export default defineConfig({
  plugins: [tailwindcss()],
  build: {
    outDir: 'Alis.Reactive.Assets/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.Assets/Styles/app.css',
      output: {
        assetFileNames: 'design-system.dev.css',
      },
    },
  },
});
