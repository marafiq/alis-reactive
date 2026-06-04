# Agent Guidance

## Goal

Build the framework around this fixed flow:

```text
Frozen public DSL -> Rich C# Plan Domain -> Generated TS Contract -> Dumb Runtime Executor
```

The DSL source is the requirement. Existing PlanModel, runtime code, tests, docs,
and memory are evidence only after they are checked against the DSL.
Use complete facts grounded in actual DSL source; partial inventories, inferred
facts, and approximate coverage are not enough for plan/runtime design.

## Required Process

For any shared plan/runtime module change, follow this order:

1. Read the actual public DSL source files for that module.
2. Build or update the DSL graph.
3. Build or update the input/output matrix.
4. Design the domain model from the graph and matrix.
5. Delete code and tests that do not map to the design.
6. Implement the C# domain, generated TS contract, and runtime executor.
7. Prove behavior with tests that exercise DSL intent.
8. Commit the closed module.

Do not edit implementation first.

## Pass Protocol

Start every pass by writing one clear pass goal in this shape:

```text
Close matrix row: <DSL source call> -> <domain term> -> <runtime behavior>
```

Each pass must name:

- DSL source files being used as requirements
- sync/async lane expectation for the row
- code to delete or simplify
- behavior proof to run before commit
- exact commit boundary

Approaches that worked and must be reused:

- source-grounded input/output matrix before implementation
- one closed behavior row per commit
- deletion-first cleanup of wrappers, stale names, and impossible-plan checks
- focused runtime tests before broader build gates
- generated TS typecheck after C# domain or plan JSON changes
- runtime asset build before Playwright
- observable Playwright runs through `scripts/playwright.sh`, not raw `dotnet test`
- glossary/process updates in the same commit when vocabulary changes

Approaches that failed and must not be repeated:

- local edits before reading the DSL source
- progress claims from uncommitted work
- broad “module improvement” passes without a named matrix row
- preserving old helper code because tests reference it
- adding fallback, registry, validation, or lifecycle concepts without a DSL graph node
- treating docs/current runtime as requirements before checking DSL source

## Commit Discipline

Every commit must move the primary goal forward in a way that can be audited
from history. Before committing, name the matrix row being closed and verify the
patch does all of this:

- deletes stale indirection instead of preserving it under new names
- keeps C# domain terms, generated TS terms, runtime names, and glossary terms aligned
- proves behavior at the DSL boundary or page-visible runtime boundary
- leaves no half-renamed vocabulary or dead helper path behind

If a commit cannot be described as a closed matrix row, keep cutting scope until
it can. Do not report module progress from uncommitted local edits.

## DSL Graph

Use diagrams as design tools, not decoration.

Graph nodes:

- DSL entry points and builder contexts
- value scopes: URL, event payload, success body, error body, request snapshot
- domain concepts
- JSON and generated TS terms
- runtime executors

Graph edges:

- trigger -> pipeline
- pipeline -> reaction
- condition -> branch
- request -> gather
- gather target <- value source
- response -> success/error scope
- response -> chained request
- component event/callback -> payload scope
- partial slot load -> merged plan state
- partial slot unload -> remove loaded state

If a behavior is not in the graph, do not implement it from inference.

## Input/Output Matrix

Every module needs a matrix row shaped like this:

```text
source file + DSL call
  -> developer intent
  -> C# domain term
  -> JSON/generated TS term
  -> runtime executor behavior
  -> behavior proof
```

The matrix must cover cross-module cases such as:

- conditions mixed with requests and other reactions
- gather from URL, event, response, component, plugin, and literals
- chained and parallel requests
- validation field binding and partial load/unload
- component events/callbacks with sync payload mutation
- app-level components and action links

## Domain Rules

Rich domain model means the smallest clear set of concepts that names real DSL
behavior. It does not mean wrappers, registries, fallback paths, claims,
validators for generated plans, or impressive names around ordinary execution.

C# implementation uses the repository's current language level: C# 14. Prefer
small value objects, discriminated unions/pattern matching where they simplify
the actual DSL graph, collection expressions, and primary constructors when
they improve clarity. Do not use modern syntax to hide weak concepts.

Use these core concepts unless the DSL graph proves a better name:

- `PlanDocument`
- `Trigger`
- `ReactionGraph`
- `ValueExpression`
- `ConditionGraph`
- `RequestPlan`
- `RequestInput`
- `ResponseRoute`
- `ClientValidationRule`
- `BrowserObjectContract`
- `ComponentObject`
- `PluginContract`
- `SlotLoad`

## Runtime Rules

Runtime executes framework-generated plans. It does not re-validate generated
plan shape or infer missing behavior.

Runtime checks are allowed only at real browser boundaries:

- missing DOM object
- missing component/plugin object
- browser API or network failure
- malformed external JSON not generated by the framework

Sync behavior stays sync unless the DSL concept is async by nature. Async
concepts are HTTP, parallel HTTP, remote triggers, partial injection, and
confirm/user decision.

## Component Rules

Component vertical slices stay isolated and compile-time typed. Do not add
stringly component APIs to make a test pass. Components expose browser object
properties, methods, events/callbacks, and typed value sources.

Controlled component IDs are absolute join keys for markup, plan, validation,
gather, partial load/unload, and runtime lookup.

## Test Rules

Tests are production code.

Keep tests that prove behavior from DSL intent. Delete or rewrite tests that
only pin helper classes, old JSON shape, stale vocabulary, or internal syntax.

A module is closed only when tests prove the matrix rows for that module.

Use `docs/developer-cli.md` as the canonical build, test, Playwright, and pack
command guide. Use root wrappers instead of ad hoc command sequences:

```text
scripts/doctor.sh
scripts/build.sh
scripts/run.sh
scripts/test.sh
scripts/playwright.sh --filter "..."
scripts/pack.sh <version>
```

Playwright must run through `scripts/playwright.sh`, not raw `dotnet test`. Full
gate order is `npm run typecheck` -> `npm run build:all` -> `npm test` ->
`dotnet build` -> non-Playwright dotnet tests -> `scripts/playwright.sh --no-build`.
The wrapper prints active test markers, writes live log/TRX/diag artifacts, and
rejects stale browser assets or stale `--no-build` binaries.

For UI work, use `docs/developer-cli.md#ui-developer-workflows` to choose the
right watcher, distinguish framework-shipped assets from sandbox-only assets,
and pick a narrow Playwright proof before the full gate.

## Deletion Rule

When code is confusing, first ask whether it maps to a DSL graph node or edge.
If not, delete it. Do not preserve previous agent work as sunk cost.
