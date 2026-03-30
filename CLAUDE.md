# Alis.Reactive Framework

C# fluent builders capture reactive browser intent as descriptors. `Html.RenderPlan(plan)`
serializes to JSON validated against `reactive-plan.schema.json`. The JS runtime executes
the plan — the only contract. C# never executes behavior, JS never invents it.

## Skills

| Skill | Status | Use for |
|-------|--------|---------|
| `reactive-dsl` | WIP | Plan, triggers, Element, Dispatch, Component, InputField, .Reactive() |
| `http-pipeline` | OK | Get/Post, Gather, Response, Chained, Parallel, WhileLoading, Into |
| `conditions-dsl` | OK | When/Then/ElseIf/Else, operators, guard composition, source types |
| `validation-rules-alis-reactive` | WIP | FluentValidation rules, Validate, ValidationErrors, WhenField |
| `onboard-fusion-component` | WIP | Adding SF components, 7-file vertical slice |
| `dotnet-xml-docs` | OK | XML documentation on public types |

## Build & Run

```bash
npm run build:all                # JS bundles + CSS
dotnet build                     # All C# projects
npm run build:api-docs           # API reference from XML docs

npm run watch                    # esbuild watch
npm run watch:css                # Tailwind watch
lsof -ti:5220 | xargs kill -9 2>/dev/null; dotnet run --project Alis.Reactive.SandboxApp

npm run typecheck                # TS type checking
npm run lint                     # ESLint

npm test                                                     # TS vitest
dotnet test tests/Alis.Reactive.UnitTests                    # Core + schema
dotnet test tests/Alis.Reactive.Native.UnitTests             # Native
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests    # Validation
dotnet test tests/Alis.Reactive.Analyzers.Tests              # Analyzers
dotnet test tests/Alis.Reactive.DesignSystem.Tests           # Design system
dotnet test tests/Alis.Reactive.NativeTagHelpers.Tests       # Tag helpers
dotnet test tests/Alis.Reactive.PlaywrightTests \
  --logger "trx;LogFileName=playwright-results.trx" \
  --results-directory TestResults
./scripts/sonar-analyze.sh                                   # SonarQube (Docker)
```

After TS/CSS changes: `npm run build:all && dotnet build`, restart SandboxApp.
All tests pass before every commit.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| C# | .NET 10, **C# 8.0 enforced** in library projects. Apps/tests use latest. |
| TS | TypeScript 5.8, esbuild ESM, Tailwind CSS v4 |
| Components | Fusion (SF EJ2 32.x) + Native. Always through DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` |
| Validation | FluentValidation 12.x |
| Tests | NUnit 4.5 + Verify, Vitest 3.x + jsdom, Playwright 1.52 |

## Process

Pipeline: **C# → Schema → TS Types → TS Runtime → Browser → Docs**.
Each layer has skills, thinking, and a test harness. A failing test drives every boundary crossing.

Detailed flows in `.claude/rules/`: `process-pipeline.md`, `process-layers.md`, `process-task-types.md`

### Prompt Clarity Gate
Prompt must be clear and specific. If vague — stop, ask, push back, propose a checklist.

### Plan — Identify Layers
1. Which layers does this touch?
2. Load applicable skills FIRST, before reading code.
3. Survey code at each layer. Read, don't guess.
4. Build a master task index (INVEST: Independent, Negotiable, Valuable, Estimable, Small, Testable).

### Thoughtful Editing
Before editing: understand the code path and blast radius. Design your strategy.
Confirm right skills and processes are loaded and you are following the plan.
If editing the same file multiple times, rethink your approach and design choices.

### Wrong Plan Protocol
If touching an unexpected layer, the plan or task is wrong. Stop, save learnings, return to planning.

### Pre-flight
- [ ] Loaded skills? Read source code at each layer touched?
- [ ] Understood WHY the current code works the way it does?
- [ ] Visibility (`internal`/`public`) chosen deliberately? API surface unchanged?
- [ ] Input evidence: what proves this change is needed?

### Post-flight
- [ ] All tests pass? Verified in actual browser?
- [ ] Each boundary crossing driven by a failing test?
- [ ] Root cause fixed, not a patch? No code smells?
- [ ] Output evidence: what proves this change is correct?
- [ ] Coverage matrix: every item in scope mapped to a test or justified as untestable?

## Rules

### 1. Git Worktrees for Feature Work

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

### 2. Plan Is the Only Contract

No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks — `root.ts` handles discovery and boot automatically.

### 3. New or Changed Primitive — 10 Steps, All Layers

1. C# descriptor — sealed class, `internal` constructor
2. Polymorphic registration — `WriteOnlyPolymorphicConverter` switch
3. Builder method — PipelineBuilder, ElementBuilder, or TriggerBuilder
4. JSON schema — failing `AssertSchemaValid()` test drives the update
5. TS types — new interface in `Scripts/types/`, discriminated union
6. Runtime handler — new switch case + `assertNever`
7. C# unit test — `VerifyJson` snapshot + `AssertSchemaValid`
8. TS unit test — runtime behavior via `boot()`
9. Playwright test — browser behavior with sandbox view
10. Sandbox view — demonstrate the primitive


### 5. Vertical Slices — Duplication Over Abstraction

Each module is self-contained. No shared base classes for behavior.
Duplication between slices is intentional.

### 6. Vendor Isolation

New component = C# vertical slice with `IInputComponent`. Zero TS runtime changes.
`component.ts` is the ONLY module that maps vendor to root (`resolveRoot`, `evalRead`).
Adding a third vendor must only touch `component.ts`. Vendor checks (`if vendor === "x"`)
in other modules violate this rule — add exports to `component.ts` instead.

### 7. Fail Fast — Fallbacks Are Exceptions

Default thinking is throw, not fallback. When something is missing or unknown, surface
the error immediately. Fallbacks hide bugs for hours because wrong values propagate silently.
A fallback is a rare, deliberate, justified exception — never the default response to uncertainty.

### 8. Plan-Driven IDs — No DOM Scanning

`IdGenerator` generates every element ID from the model expression at C# render time.
The plan carries IDs. Runtime uses `getElementById` only — direct lookup, zero scanning.
If you think you need `querySelectorAll` or DOM traversal, the plan is missing information.
Fix the C# descriptor to carry it.

### 9. API Surface Is Frozen

Enforced by hookify rule `.claude/hookify.protect-api-surface.local.md`.

### 10. Root Cause, Not Patch

Trace the full code path. Identify the exact line. Understand WHY before changing WHAT.
If stuck after 2 attempts: research online, save findings to a temp file, dispatch agents
with specific input and evidence-based output criteria. Fix the root cause. Verify in browser.
Run all tests.


