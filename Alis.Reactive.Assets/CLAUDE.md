# Alis.Reactive.Assets

The core's browser code: `runtime/` is the TypeScript runtime that executes a plan, one folder for each module, named
as the C# module whose nodes it executes, and `runtime/SliceTree.txt` is its slice table.

## The standard

- The runtime executes the plan and invents no behavior the plan does not carry. No field of the plan is optional
  or nullable, so plan data takes no default. It trusts a framework-generated plan: no validator, fallback, registry
  or defensive throw for a shape the C# controls. It fails loud only at real boundaries: DOM lookup, browser APIs,
  the network, component or plugin lookup, and JSON the framework did not produce.
- `runtime/Schema/plan.ts` and `runtime/Schema/plan.schema.json` are the plan's contract, generated from the C#
  grammar (`tools/PlanContractGenerator`): the runtime declares no type of the plan's own, a hook refuses a hand
  edit of either file, and `scripts/check.sh` fails when the grammar generates another.
- It never guesses: it reads no intent from the DOM, takes an element by its id alone, and retries nothing another
  way. What it needs to know, the plan carries. One file finds a root's plan scripts, `runtime/Execution/plan-scripts.ts`,
  and the lint refuses a query wider than an id in every other.
- A union of the plan has one switch, with a case for every kind, ending in `assertNever` (`runtime/Schema/kinds.ts`).
  A kind whose module's unit has not built it throws `notBuilt` there, as a door that is not built throws in C#: it
  runs or it fails loud. The evaluator's two switches, `runtime/Execution/statements.ts` and
  `runtime/Execution/expressions.ts`, are whole; a nested union's switch lands whole with the handler that reaches it.
- The compiler holds every strictness option `spec/REWRITE.md` section 2.7 lists, in `tsconfig.json`; an enum is
  refused there.
- The lint is the root `eslint.config.js`: two typed presets, each rule an error, no inline configuration, and the
  rules section 2.7 lists beyond them, each for one line of the spec. A rule is never turned off or down for a file;
  code that a rule refuses is rewritten.
- A rule of the lint is proven by a refused and an accepted file in `tests/Alis.Reactive.ArchitectureTests/LintCases`,
  which `WhenTheLintIsConfigured` lints with this configuration. A rule's refused and accepted cases are listed, one
  fact each, before the rule is written (the lead's slip, 2026-11-12).
- `runtime/SliceTree.txt` orders the module folders. A slice's file imports the files of its own folder and of the
  rows before its own, each by its file's name, and the table's last row is the dot, the bundle's entry
  `runtime/index.ts` beside it. A folder enters the table with its first file: the lint refuses every relative
  import in a file whose folder has no row.

## A module's pure tests

A runtime module's pure tests sit beside its code, in files named for it that end in .test.ts, and vitest runs
them (the root's `vitest.config.js`). A test imports its own module's index and nothing else of the runtime, so it reads the module's public
surface and never its internals: the lint refuses any other import of the runtime there (`no-restricted-imports`). It runs in node with no DOM and expects literals written from the schema's lines,
never a recorded output. What touches the DOM has no pure test: a Playwright test proves it.

## Commands

```bash
npx tsc -b Alis.Reactive.Assets
npx eslint Alis.Reactive.Assets --max-warnings 0
npx vitest run
scripts/check.sh --no-spec
```
