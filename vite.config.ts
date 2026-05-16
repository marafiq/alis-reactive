import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

// Design-system CSS pipeline.
// Entry:  Alis.Reactive.DesignSystem/Styles/app.css
// Output: Alis.Reactive.DesignSystem/dist/css/design-system.dev.css
//
// The design-system CSS is owned and shipped by the AlisReactive.DesignSystem
// package, independently of the reactive core. emptyOutDir is false so a
// stale-file sweep never races other writers into the dist/ tree.
export default defineConfig({
  plugins: [tailwindcss()],
  build: {
    outDir: 'Alis.Reactive.DesignSystem/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.DesignSystem/Styles/app.css',
      output: {
        assetFileNames: 'design-system.dev.css',
      },
    },
  },
});
