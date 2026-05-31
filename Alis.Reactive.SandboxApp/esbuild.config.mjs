import * as esbuild from 'esbuild';
import { fileURLToPath } from 'node:url';

// Builds the sandbox-only plugins bundle (not shipped — sandbox app asset).
//   input  : Alis.Reactive.SandboxApp/Scripts/sandbox-plugins.ts
//   output : Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js
//
//   node esbuild.config.mjs            one-shot build
//   node esbuild.config.mjs --watch    rebuild on change
//
// absWorkingDir is pinned to the repo root so esbuild's module-path comments
// in the (unminified) bundle match the former repo-root build byte-for-byte.
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
