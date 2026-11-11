# Alis.Reactive.Assets

The core's browser code: `runtime/` is the TypeScript runtime that executes a plan, one folder for each module, and
`runtime/SliceTree.txt` is its slice table. Root CLAUDE.md states what the runtime never does (invent behavior a plan
does not carry, default a plan's field).

- The compiler holds every strictness option `spec/REWRITE.md` section 2.7 lists, in `tsconfig.json`; an enum is
  refused there.
- The lint is the root `eslint.config.js`: two typed presets, each rule an error, no inline configuration, and the
  rules section 2.7 lists beyond them, each for one line of the spec. A rule is never turned off or down for a file;
  code that a rule refuses is rewritten.
- A rule of the lint is proven by a refused and an accepted file in `tests/Alis.Reactive.ArchitectureTests/LintCases`,
  which `WhenTheLintIsConfigured` lints with this configuration.

```bash
npx tsc -b Alis.Reactive.Assets
npx eslint Alis.Reactive.Assets --max-warnings 0
scripts/check.sh --no-spec
```
