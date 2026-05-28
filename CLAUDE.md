# Alis.Reactive Framework

C# fluent builders express reactive browser intent. `Html.RenderPlan(plan)` serializes that
intent as plan JSON from the rich C# domain model. Generated TypeScript plan
types mirror that domain model, and the TypeScript runtime executes plan
instructions without adding behavior the plan does not describe. C# never
executes browser behavior. TypeScript never invents information the plan does
not carry.

Core architecture rule: DSL -> Rich Plan Domain -> Generated Rich TS Contract ->
Runtime Executioner. Runtime code executes framework-generated plans; it does
not defend against impossible bad plans with preflight, rollback, fallback, or
speculative recovery. Invalid behavior belongs in the C# PlanModel where it can
be made unrepresentable. Runtime checks are for true external boundaries only:
DOM lookup, browser API failure, network, and malformed non-framework input.
Do not model normal execution bookkeeping as validation, claims, rejects,
lifecycle gates, or registries. If the server-generated plan says source A is
assigned to target B, the runtime reads A and writes B. Bookkeeping names must
describe what is remembered for execution or unload, not imply the plan is
suspicious.

Blueprint-first rule: plan-model and runtime refactors must be designed from
the frozen DSL source and the source blueprint/matrix, not from the current
helper code alone. A change is allowed only when it can be traced as
`DSL API -> developer intent -> domain concept -> JSON/TS term -> runtime
execution`. If that trace is missing, stop and complete the blueprint before
editing code.

Drift guard: source DSL is the requirement, not samples, XML docs, old unit
tests, the current runtime, or remembered clues. Before coding a
surface, update `docs/reactive-plan-source-blueprint.md` with the actual source
file, DSL input, rich domain output, JSON/TS term, and runtime output. If that
row cannot be written from source, read more source instead of making local
edits.

Doubt rule: whenever there is even slight doubt about a behavior, name, edge
case, or module boundary, stop inferring and go back to the actual DSL source.
Add or correct the input/output matrix row before changing code.

Rich domain model does not mean inventing names, wrappers, registries, lifecycles,
or abstractions so the code looks modeled. It means the smallest set of terms
that directly explain the DSL behavior and make the implementation simpler to
read, test, and change. If a type only carries parameters, renames a branch, or
requires explanation before its value is obvious, delete it or inline it.

Active rich-model vocabulary lives in `docs/reactive-plan-domain-language.md`.
Keep that glossary aligned with code changes; it is a navigation aid for the
DDD refactor, while C# plan types plus generated TypeScript types remain the
contract source.

Throughout this document, "the runtime" means the TypeScript code in
`Alis.Reactive.Assets/runtime/` that executes plans in the browser. It is
bundled by esbuild into `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js`
(IIFE), shipped inside the AlisReactive NuGet, and copied into the consumer's
`wwwroot/scripts/` by the shipped `AlisReactive.targets` file with the
consumer's package version baked into the filename.

## Architecture — 4 Layers, 3 Boundaries

Each boundary is guarded by behavior evidence. Tests are production code: they
must protect DSL behavior and domain language, not mirror implementation
indirection.

```
Layer 1  Frozen Public DSL in cshtml
         Quality: typed authoring, compile-time component/member APIs, no string magic except plugin compatibility
         Harness: complete facts grounded in actual DSL source plus Playwright slices that use the DSL
         ↓
         BOUNDARY: DSL intent must be representable without server-side browser execution
         ↓
Layer 2  Rich C# Plan Domain
         Quality: value objects, invariants, reaction graph, object contracts, lifecycle vocabulary
         Harness: domain behavior tests
         ↓
         BOUNDARY: generated TS plan contract
         ↓
Layer 3  Generated TypeScript Plan Types + Runtime Domain
         Quality: generated discriminated unions, immediate/async execution lanes, no fallback behavior
         Harness: npm run typecheck and focused runtime behavior tests
         ↓
         BOUNDARY: browser-visible behavior
         ↓
Layer 4  Browser Verification + Documentation
         Quality: real interactions, visible outcomes, no page.evaluate(), glossary aligned with code
         Harness: sandbox-verified examples
```

Detailed flows: `.claude/rules/process-pipeline.md`, `process-layers.md`, `process-task-types.md`

## Plan-Driven IDs — No DOM Scanning

`IdGenerator` (`Alis.Reactive/IdGenerator.cs`) generates every HTML element ID at C# render
time from the model type and property expression. Format: `{Namespace_TypeName}__{MemberPath}`.

```
Model:      Alis.Reactive.SandboxApp.Models.OrderModel
Expression: m => m.Address.City
ID:         Alis_Reactive_SandboxApp_Models_OrderModel__Address_City
```

All vendors (Syncfusion, Native) produce the same ID for the same expression. IDs are
deterministic and collision-free by construction. The plan carries every ID the runtime needs.

Non-input component IDs (buttons, elements, containers) are the developer's responsibility —
chosen explicitly via `p.Element("my-id")` or `Html.NativeButton("btn-id", ...)`. The framework
does not generate fallback IDs. If an ID collides, that is a developer error, not a framework
concern. No fallbacks, no auto-generated suffixes, no scanning to resolve ambiguity.

The runtime uses `getElementById` for all plan model class and element resolution. Wide DOM
queries exist in 3 justified locations only:
- `root.ts:25` — discovers `[data-reactive-plan]` script elements at boot
- `inject.ts:16` — discovers plans in dynamically injected HTML
- `retry-indicator.ts:53` — cleans up retry indicator elements by data attribute

Scoped `querySelector` calls exist in `error-display.ts` and `orchestrator.ts` for validation
summary element lookups (generated HTML, not plan components).

If you think you need `querySelectorAll` or DOM traversal for plan component resolution, the
plan is missing information. Fix the C# plan model class to carry it.

## The Plan Contract

C# `Render()` serializes the plan to JSON inside a `<script type="application/json"
data-reactive-plan>` element. The runtime discovers these elements, parses the JSON, merges
partials by `planId`, and boots each composed plan. Sandbox URL: `http://localhost:5220`.

`WriteOnlyPolymorphicConverter<T>` enables polymorphic serialization by delegating to the
concrete type via `JsonSerializer.Serialize(writer, value, value.GetType(), options)`. Each
concrete plan model class carries its own `kind` property (e.g., `public string Kind => "set"`)
which becomes the discriminator in the JSON, matched by TypeScript discriminated unions.

Generated TypeScript types come from the C# plan domain via `PlanTypeGenerator`.
The runtime trusts framework-produced plan JSON as domain output. If external or
corrupted JSON reaches the browser, runtime failures must expose the domain
drift with context rather than become normal control flow.

## Build & Run

Canonical build, run, and test reference. Every command runs from the repo
root. A fresh clone has no `node_modules` and no bundles — start with First run.

### First run (fresh clone)

```bash
npm ci                                          # JS deps, from package-lock.json
npm run build:all                               # build the JS/CSS bundles
dotnet run --project Alis.Reactive.SandboxApp    # → http://localhost:5220
```

Order is strict — `build:all` must finish before the sandbox starts.
`SandboxApp/Program.cs` serves the bundles directly and throws on startup if the
`Alis.Reactive.Assets/dist/` folder is missing.

### Daily dev loop — 3 terminals

```bash
npm run watch:runtime                            # framework JS → dist/ on every .ts edit
npm run watch:design-system                      # framework CSS → dist/ on every .css edit
dotnet watch --project Alis.Reactive.SandboxApp  # Razor + C# hot reload
```

Framework TS/CSS edits need **only a browser refresh** — no `dotnet build`, no
sandbox restart. `Program.cs` wires a `CompositeFileProvider` over the bundle
output; `asp-append-version` re-hashes each file per request so the browser
never serves stale bytes. The Fusion CSS watcher is `watch:fusion`; sandbox-only
bundles have their own watchers (`watch:sandbox-plugins`, `watch:sandbox-css`).

### The bundles — `npm run build:all`

`build:all` builds the two npm workspaces in order: `Alis.Reactive.Assets` (the
framework browser assets — runtime, design system, Fusion) then
`Alis.Reactive.SandboxApp` (sandbox-only assets). Every output path is
gitignored; `git status` stays clean after a build.

| npm script (root) | Output | Used by |
|-------------------|--------|---------|
| `build:runtime` | `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js` | sandbox, `AlisReactive` NuGet |
| `build:design-system` | `Alis.Reactive.Assets/dist/css/design-system.dev.css` | sandbox, `AlisReactive.DesignSystem` NuGet |
| `build:fusion` | `Alis.Reactive.Assets/dist/css/syncfusion.dev.css` | sandbox, `AlisReactive.Fusion` NuGet |
| `build:sandbox` | `Alis.Reactive.SandboxApp/wwwroot/{js,css}/` | sandbox only |

All three framework bundles build into one tree — `Alis.Reactive.Assets/dist/`.
They reach three places:

- **Sandbox** — `Program.cs` serves `Alis.Reactive.Assets/dist/` via a single
  `CompositeFileProvider`. No copy into `wwwroot/`.
- **NuGet** — each package ships its own bundle: `Alis.Reactive.csproj` packs the
  runtime JS into `AlisReactive`, `Alis.Reactive.DesignSystem.csproj` packs
  `design-system.dev.css` into `AlisReactive.DesignSystem`, and
  `Alis.Reactive.Fusion.csproj` packs `syncfusion.dev.css` into `AlisReactive.Fusion`
  — all from `$(AlisAssetsDist)`, defined once in `Directory.Build.props`.
  The copy-to-`wwwroot` mechanism is one shared `build/AlisReactiveAssets.targets`,
  packed into each. `dotnet pack` never runs npm; the `VerifyBundlesExistBeforePack`
  / `VerifyDesignSystemBundleExistsBeforePack` / `VerifyFusionBundleExistsBeforePack`
  targets fail fast if a bundle is missing.
- **Example app** (`examples/resident-intake/`) — consumes the *published*
  NuGet; `AlisReactive.targets` copies the bundles into its `wwwroot/` on build.
  Not driven by local `npm`. Rebuild via `scripts/rebuild-example-app.sh`.

### Static checks

```bash
npm run typecheck                                # both tsconfigs (framework + sandbox)
npm run lint
```

### Run the tests

Three layers. **All must pass before every push.**

**TypeScript runtime — vitest:**

```bash
npm test
```

Runs vitest (jsdom) across both npm workspaces. Each workspace has its own
`vitest.config.ts` — `Alis.Reactive.Assets` scans `runtime/__tests__/`,
`Alis.Reactive.SandboxApp` scans `Scripts/__tests__/`.
A branch with no such files (this branch has none) makes vitest print
`No test files found` and exit non-zero — that is the empty-suite signal, not a
failure in your code.

**C# build gate — compile the DSL, plan model, generators, runtime host, and Playwright harness:**

```bash
dotnet build
```

**Playwright — browser, end-to-end:**

```bash
dotnet build                                                 # pre-build so the fixture's sandbox starts fast
dotnet test tests/Alis.Reactive.PlaywrightTests --logger "console;verbosity=detailed"
```

The fixture starts and stops its **own** sandbox on a random free port — do not
pre-start the sandbox, and port 5220 does **not** need to be free for Playwright.
The `console;verbosity=detailed` logger prints every test as `Passed`/`Failed` and
ends with a `Total / Passed / Failed` summary, so a run always reports exactly
what failed. (`dotnet test` is cross-platform; no `.trx` file needed.)

First run only — build the test project, then install the browser (the
`playwright.ps1` script is a build output, so the build must come first):

```bash
dotnet build tests/Alis.Reactive.PlaywrightTests
pwsh tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
```

Re-run only the tests that failed — confirms a real failure vs. a load flake:

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "Name=test_one|Name=test_two"
```

A Playwright test that fails with a `TimeoutException` but passes on an isolated
re-run is a machine-load flake, not a product bug.

### Sandbox processes — kill stale, run fresh

Testing against a stale app is the top time-waster. Two failure modes:

**1. Stale process.** A leftover `dotnet run` keeps listening on 5220. The next
`dotnet run` cannot bind the port and crashes with `address already in use` — but
`localhost:5220` still answers, served by the **old** process, so you debug
against stale code. Stop the sandbox with `Ctrl+C` in its own terminal. To find
and kill a stray one:

```bash
# macOS / Linux
lsof -ti:5220 | xargs kill -9         # by port
pkill -f Alis.Reactive.SandboxApp     # by name
```

```bat
REM Windows
for /f "tokens=5" %p in ('netstat -ano ^| findstr :5220') do taskkill /F /PID %p
taskkill /F /IM Alis.Reactive.SandboxApp.exe
```

Playwright cleans up its own server; this applies only to a manually-run sandbox.

**2. Stale bundle.** Editing `.ts`/`.css` without rebuilding leaves the sandbox
serving old bytes. Rebuild with `npm run build:all`, or leave `npm run watch:runtime` /
`watch:design-system` running — `CompositeFileProvider` serves the new `dist/` output on the
next request and `asp-append-version` re-hashes the URL, so a browser refresh
always gets current bytes. No sandbox restart needed for TS/CSS changes; C#/Razor
changes need `dotnet watch` or a rebuild.

### Pack the NuGet

```bash
npm run build:all                                # required — pack does NOT invoke npm
dotnet build --configuration Release
dotnet pack Alis.Reactive/Alis.Reactive.csproj \
    --configuration Release --no-build --output ./nupkgs -p:Version=<version>
```

### Before every push

1. **All tests pass** — vitest, all seven C# unit projects, and Playwright (see
   Run the tests above). No exceptions.
2. **`git status` is clean** after a build. Every bundler output path is
   gitignored; tracked `wwwroot/` files are hand-written only
   (`disable-sf-animations.js`). A `dist/` or `wwwroot/` bundle showing in
   `git status` means the build wrote to a tracked path — that is a bug, do not
   `git add` it.
3. Regenerate API docs with `npm run build:api-docs` if XML docs changed.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| C# | .NET 10, C# 14. Use modern C# where it improves the domain model without weakening the DSL contract. |
| TS | TypeScript 5.8, esbuild ESM, Tailwind CSS v4 |
| Components | Syncfusion EJ2 32.x (Fusion) + Native HTML. Always through DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` |
| Validation | FluentValidation 12.x remains server authority; `ReactiveValidator<T>` records explicit browser validation metadata through DI |
| Tests | NUnit 4.3-4.5, Vitest 3.x + jsdom (configured, no tests yet), Playwright 1.52 |

## Skills

8 skills in `.claude/skills/`. Load applicable skills BEFORE reading code.

| Skill | Use for |
|-------|---------|
| `reactive-dsl` | Plan, triggers, Element, Dispatch, Component, InputField, .Reactive() |
| `http-pipeline` | Get/Post, Gather, Response, Chained, Parallel, WhileLoading, Into |
| `conditions-dsl` | When/Then/ElseIf/Else, operators, guard composition, source types |
| `validation-rules` | FluentValidation rules, Validate, ValidationErrors, WhenField |
| `onboard-fusion-component` | Adding Syncfusion components, 7-file vertical slice |
| `solid-ts-audit` | SOLID analysis of TypeScript runtime modules |
| `modern-csharp` | C# 14 patterns that clarify the rich plan domain model |
| `bdd-testing` | Playwright BDD tests, 5 rules, 7-behavior contract, blind reviewer |

10 hookify rules in `.claude/hookify.*.local.md` enforce quality gates automatically:
`enforce-csharp8`, `no-public-in-libraries`, `no-raw-inputs`, `bdd-test-enforcement`,
`bdd-public-api-only`, `xml-docs-quality`, `commit-requires-relevant-tests`,
`merge-requires-all-tests` (8 active). `no-js-in-views` and `protect-api-surface` exist
but are currently disabled (`enabled: false`).

## Rules

### 1. Plan Is the Only Contract

No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks — `root.ts` handles discovery and boot automatically.

### 2. New or Changed Primitive — All Layers

1. C# plan model class — sealed class, `internal` constructor
2. Polymorphic registration — `WriteOnlyPolymorphicConverter` delegates to concrete type
3. Builder method — PipelineBuilder, ElementBuilder, or TriggerBuilder
4. Generated TS plan contract — `PlanTypeGenerator` output stays aligned with C#
5. Runtime handler — new switch case + `assertNever`
6. C# domain behavior test — prove DSL intent becomes the right plan model
7. TS runtime behavior test — `Alis.Reactive.Assets/runtime/__tests__/*.test.ts`
8. Playwright test — browser behavior with sandbox view
9. Sandbox view — demonstrate the primitive

### 3. Vertical Slices — Duplication Over Abstraction

Each component module is self-contained. No shared base classes for behavior.
Duplication between slices is intentional.

### 4. Vendor Isolation

New component = C# vertical slice with `IInputComponent`. Zero TS runtime changes.
`resolver.ts` is the only module that maps vendor to DOM root (`resolveVendorRoot`) and wires
vendor-specific events (`wireEvent`). Adding a third vendor must only touch `resolver.ts` and
add a `resolution/event-{vendor}.ts` file. Vendor checks in other modules violate this rule.

### 5. Trust Generated Plans — Boundary Errors Only

The runtime and plan domain trust framework-generated plans. Do not add defensive
throws, validators, rejects, claims, or fallback paths for shapes the typed DSL
already controls. Errors belong at real boundaries: developer-authored DSL misuse,
DOM/component/plugin lookup, browser APIs, network, and external JSON. Inside the
generated plan graph, prefer direct domain state over proof-by-exception.

A fallback is a rare, deliberate, justified exception — never the default response
to uncertainty.

**Null escape hatches require justification.** Every NEW per-property `[JsonIgnore(WhenWritingNull)]`,
nullable property declaration, or `?? fallback` you add must be PROVEN necessary by answering
in writing: "Could this be a sentinel/empty/default instead? If yes, why am I taking the shortcut?
If no, what domain meaning would `Empty`/`None` collide with?" Mechanical addition of null markers
during a refactor is the failure pattern from `feedback_null_escape_hatch_blindness.md` — when
removing tech debt, if the count of null markers on ANY surface goes UP, stop and audit each
new marker for sentinel-replaceability before committing.

### 6. Plan-Driven IDs — No DOM Scanning

`IdGenerator` generates every element ID from the model expression at C# render time.
The runtime resolves plan components via `getElementById` only. Non-input IDs are the
developer's explicit choice. No fallback IDs. No scanning.

### 7. API Surface Is Frozen

No `public` constructors on plan model classes. All plan model class constructors are
`internal`. All plan model properties use `internal set`. Developers interact through builder
APIs and factory methods (`Html.On`, `Html.InputField`, `p.Get`, `p.When`) — never through
constructors.

### 8. Root Cause, Not Patch

Trace the full code path. Identify the exact line. Understand WHY before changing WHAT.
If stuck after 2 attempts: research online, save findings, dispatch agents with specific
input and evidence-based output criteria. Fix the root cause. Verify in browser.

A patch is a commit that fixes a symptom without understanding the cause. Ten patches is
ten mistakes. The scout rule applies: leave every file cleaner than you found it. If you
touch a file and see a code smell, fix it — do not walk past broken windows.

### 9. Code Hygiene

These small practices compound. They are not optional style preferences — they prevent
entire categories of bugs and keep the codebase readable under pressure.

**Revealing names over complex conditions.** Extract boolean expressions into named variables
that explain intent. The name is the documentation.

```csharp
// Wrong: condition is opaque
if (field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly)
    ExtractRule(field);

// Right: name reveals intent
var requiresClientValidation = field.Shape != null && field.Shape.Kind != "none" && !field.IsServerOnly;
if (requiresClientValidation)
    ExtractRule(field);
```

**Variables close to usage.** Declare a variable where it is first needed, not at the top of
the method. Long distances between declaration and usage hide bugs and make code harder to
follow. If a variable is used once, inline it.

**Avoid nesting.** Use early returns and guard clauses to flatten control flow. Deep nesting
hides logic and makes branches hard to trace.

```csharp
// Wrong: nested
if (component != null)
{
    if (component.Vendor == "fusion")
    {
        // 20 lines of logic
    }
}

// Right: guard clause, flat
if (component == null) return;
if (component.Vendor != "fusion") return;
// 20 lines of logic
```

**No dead code.** Delete unused variables, unreachable branches, commented-out code. Do not
keep code "for reference" — git has history. Commented-out code is a lie that rots.

**Small methods, single responsibility.** If a method needs a comment explaining what a
block does, extract that block into a named method. The method name replaces the comment.

### 10. Prefer BDD Vertical Slice Playwright Tests

Every Playwright test is an isolated vertical slice. It tests one user-visible behavior from
page load through interaction to visible outcome.

**Isolated:** Each test navigates to a fresh page. No shared state between tests. No test
ordering. If test B breaks when test A is skipped, test B is broken.

**Vertical:** The test exercises the full stack — C# view renders the plan, runtime boots it,
user interacts, browser reflects the outcome. No mocking. No `page.evaluate()`. No shortcuts.
Framework tests that verify the gather pipeline may assert on `request.PostData` — this is
the one justified exception where asserting on non-visible data is correct.

**Behavior-first:** The test name describes what the user sees, not what the code does.
`selecting_care_level_updates_billing_amount` not `domready_trigger_fires_sequential_reaction`.

Load the `bdd-testing` skill before writing any Playwright test. The 5 BDD rules and the
7-behavior contract per input component are defined there.

### 11. Quality Aspirations

Known weaknesses tracked for improvement:

- **DDD depth**: Domain model uses `null` where Value Objects with constructor invariants
  must enforce valid state. Association and aggregation boundaries are implicit.
  Screaming names (types that express domain intent) are underused.
- **Serialization**: `[JsonIgnore(Condition = WhenWritingNull)]` attributes scattered
  across plan model classes instead of explicit serialization contracts.
- **TS tracing**: `core/trace.ts` is 38 lines using `console.error`/`warn`/`log` dispatched
  by level. It must move toward OTel-style structured tracing — explicit data flowing through
  modules, correlation IDs, proper span context, actionable error messages.

### 12. Git Worktrees for Feature Work

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

## Process

### Prompt Clarity Gate
Prompt must be clear and specific. If vague — stop, ask, push back, propose a checklist.

### Plan — Identify Layers
1. Which layers does this touch?
2. Load applicable skills FIRST, before reading code.
3. Survey code at each layer. Read, don't guess.
4. Build a master task index (INVEST: Independent, Negotiable, Valuable, Estimable, Small, Testable).

### Thoughtful Editing
Before editing: understand the code path and blast radius. Design your strategy.
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

### Review Loop — Every Change

Each change cycles through three gates with team sign-off:

1. **Plan Review**: Post plan → reviewers verify against code → fix findings → sign-off
2. **Implementation Review**: Post diff → reviewers verify against plan + actual code → sign-off
3. **Post-Implementation**: All tests pass → browser verified → no broken cross-references
