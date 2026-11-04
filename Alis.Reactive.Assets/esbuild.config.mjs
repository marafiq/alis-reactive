import * as esbuild from 'esbuild';

const watch = process.argv.includes('--watch');

const options = {
  entryPoints: ['bundle.ts'],
  bundle: true,
  outfile: 'dist/scripts/alis-reactive.dev.js',
  format: 'iife',
  globalName: '__alisReactive',
  minify: !watch,
  sourcemap: 'external',
};

if (watch) {
  const buildContext = await esbuild.context(options);
  await buildContext.watch();
  console.log('esbuild: watching runtime/ and components/ for the alis-reactive bundle');
} else {
  await esbuild.build(options);
}
