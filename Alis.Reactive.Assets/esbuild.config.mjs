import * as esbuild from 'esbuild';

// Sandbox and NuGet consumers load the runtime as a plain script, so the bundle
// stays an IIFE at dist/scripts/alis-reactive.dev.js.

const watch = process.argv.includes('--watch');

const options = {
  entryPoints: ['bundle.ts'],
  bundle: true,
  outfile: 'dist/scripts/alis-reactive.dev.js',
  format: 'iife',
  globalName: '__alisReactive',
  minify: !watch,
  // The map is written beside the bundle with no reference inside it, so no page ever asks for it and
  // the package ships the bundle alone; the framework's coverage report reads it.
  sourcemap: 'external',
};

if (watch) {
  const buildContext = await esbuild.context(options);
  await buildContext.watch();
  console.log('esbuild: watching runtime/ and components/ for the alis-reactive bundle');
} else {
  await esbuild.build(options);
}
