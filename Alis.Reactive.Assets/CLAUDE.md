# Alis.Reactive.Assets

The core's browser code: `runtime/` is the TypeScript runtime that executes a plan, one folder for each module, named
as the C# module whose nodes it executes, and `runtime/SliceTree.txt` is its slice table.

## The standard

- The runtime executes the plan and invents no behavior the plan does not carry. No field of the plan is optional
  or nullable, so plan data takes no default. It trusts a framework-generated plan: no validator, fallback, registry
  or defensive throw for a shape the C# controls. It fails loud only at real boundaries: DOM lookup, browser APIs,
  the network, component or plugin lookup, and JSON the framework did not produce.
- It never guesses: it reads no intent from the DOM, takes an element by its id alone, and retries nothing another
  way. What it needs to know, the plan carries.
- The compiler holds every strictness option `spec/REWRITE.md` section 2.7 lists, in `tsconfig.json`; an enum is
  refused there.
- The lint is the root `eslint.config.js`: two typed presets, each rule an error, no inline configuration, and the
  rules section 2.7 lists beyond them, each for one line of the spec. A rule is never turned off or down for a file;
  code that a rule refuses is rewritten.
- A rule of the lint is proven by a refused and an accepted file in `tests/Alis.Reactive.ArchitectureTests/LintCases`,
  which `WhenTheLintIsConfigured` lints with this configuration.

## A module's pure tests

A runtime module's pure tests sit beside its code, in files named for it that end in .test.ts, and U3 lands the
first with vitest. A test imports its own module's index and nothing else of the runtime, so it reads the module's public
surface and never its internals. It runs in node with no DOM and expects literals written from the schema's lines,
never a recorded output. What touches the DOM has no pure test: a Playwright test proves it.

## Commands

```bash
npx tsc -b Alis.Reactive.Assets
npx eslint Alis.Reactive.Assets --max-warnings 0
scripts/check.sh --no-spec
```
