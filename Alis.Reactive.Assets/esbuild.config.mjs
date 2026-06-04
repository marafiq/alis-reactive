import * as esbuild from 'esbuild';

// Sandbox and NuGet consumers load the runtime as a plain script, so the bundle
// stays an IIFE at dist/scripts/alis-reactive.dev.js.

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
  const buildContext = await esbuild.context(options);
  await buildContext.watch();
  console.log('esbuild: watching runtime/ for the alis-reactive runtime bundle');
} else {
  await esbuild.build(options);
}
