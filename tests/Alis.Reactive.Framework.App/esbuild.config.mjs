import * as esbuild from 'esbuild';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

// The framework test app's components in one bundle, built the way an app builds its own. A component
// knows nothing of the framework (docs/design/framework-test-components.md): it imports only files of
// its own folder, and the entry imports only components. Anything else fails the build.
const appDir = path.dirname(fileURLToPath(import.meta.url));
const entry = path.join(appDir, 'components.ts');
const componentsDir = path.join(appDir, 'Components');
const watch = process.argv.includes('--watch');

const ownFolderOnly = {
  name: 'own-folder-only',
  setup(build) {
    build.onResolve({ filter: /.*/ }, args => {
      if (args.kind === 'entry-point') return undefined;
      const from = path.dirname(args.importer);
      const target = path.resolve(from, args.path);
      const within = args.importer === entry ? componentsDir : from;
      if (args.path.startsWith('.') && target.startsWith(within + path.sep)) return undefined;
      return {
        errors: [{
          text: `${path.relative(appDir, args.importer)} imports '${args.path}': a component imports only files of its own folder, and nothing of the framework.`,
        }],
      };
    });
  },
};

const options = {
  absWorkingDir: appDir,
  entryPoints: { 'framework-components': entry },
  bundle: true,
  outdir: path.join(appDir, 'wwwroot', 'js'),
  format: 'iife',
  plugins: [ownFolderOnly],
};

if (watch) {
  const context = await esbuild.context(options);
  await context.watch();
  console.log('esbuild: watching the framework test components');
} else {
  await esbuild.build(options);
}
