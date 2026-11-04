import * as esbuild from 'esbuild';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('..', import.meta.url));
const watch = process.argv.includes('--watch');

const options = {
  absWorkingDir: repoRoot,
  entryPoints: {
    'sandbox-plugins': 'Alis.Reactive.SandboxApp/Scripts/sandbox-plugins.ts',
    'sandbox-components': 'Alis.Reactive.SandboxApp/Scripts/sandbox-components.ts',
  },
  bundle: true,
  outdir: 'Alis.Reactive.SandboxApp/wwwroot/js',
  format: 'iife',
};

if (watch) {
  const ctx = await esbuild.context(options);
  await ctx.watch();
  console.log('esbuild: watching Scripts/ for the sandbox bundles');
} else {
  await esbuild.build(options);
}
