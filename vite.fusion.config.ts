import { defineConfig } from 'vite';

// Fusion (Syncfusion EJ2) CSS pipeline.
// Entry:  Alis.Reactive.Fusion/Styles/syncfusion.entry.css
// Output: Alis.Reactive.Fusion/dist/css/syncfusion.dev.css
//
// Bundles @syncfusion/ej2/tailwind3.css (sourced from npm) with the
// framework's Syncfusion overrides. Lives in the Fusion project tree so
// AlisReactive.Fusion packs and ships it independently of the core package —
// Native-only consumers of AlisReactive do not pull in Syncfusion CSS.
//
// No @tailwindcss/vite plugin here: the input is plain vendor CSS + overrides,
// not a Tailwind v4 authoring entry.
export default defineConfig({
  build: {
    outDir: 'Alis.Reactive.Fusion/dist/css',
    emptyOutDir: false,
    cssMinify: true,
    rollupOptions: {
      input: 'Alis.Reactive.Fusion/Styles/syncfusion.entry.css',
      output: {
        assetFileNames: 'syncfusion.dev.css',
      },
    },
  },
});
