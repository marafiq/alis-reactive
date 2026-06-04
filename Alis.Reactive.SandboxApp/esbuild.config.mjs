import * as esbuild from 'esbuild';
import { fileURLToPath } from 'node:url';

// Sandbox-only plugin bundle; the framework package does not ship these demo plugins.
// absWorkingDir stays at repo root so entry and output paths match build wrappers.
const repoRoot = fileURLToPath(new URL('..', import.meta.url));
const watch = process.argv.includes('--watch');

const options = {
  absWorkingDir: repoRoot,
  entryPoints: ['Alis.Reactive.SandboxApp/Scripts/sandbox-plugins.ts'],
  bundle: true,
  outfile: 'Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js',
  format: 'iife',
};

if (watch) {
  const ctx = await esbuild.context(options);
  await ctx.watch();
  console.log('esbuild: watching Scripts/ for the sandbox plugins bundle');
} else {
  await esbuild.build(options);
}
