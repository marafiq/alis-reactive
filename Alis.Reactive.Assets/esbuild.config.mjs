import * as esbuild from 'esbuild';

// Builds the Alis.Reactive runtime bundle.
//   input  : runtime/root.ts
//   output : dist/scripts/alis-reactive.dev.js   (IIFE, global name __alisReactive)
//
//   node esbuild.config.mjs            one-shot, minified  (CI / build:all)
//   node esbuild.config.mjs --watch    rebuild on change, unminified  (dev loop)
//
// Option parity with the former inline package.json CLI: --bundle --format=iife
// --global-name=__alisReactive, --minify on build only.

const watch = process.argv.includes('--watch');

const options = {
  entryPoints: ['runtime/root.ts'],
  bundle: true,
  outfile: 'dist/scripts/alis-reactive.dev.js',
  format: 'iife',
  globalName: '__alisReactive',
  minify: !watch,
};

if (watch) {
  const ctx = await esbuild.context(options);
  await ctx.watch();
  console.log('esbuild: watching runtime/ for the alis-reactive runtime bundle');
} else {
  await esbuild.build(options);
}
