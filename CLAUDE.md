# Alis.Reactive Framework

C# fluent builders express reactive browser intent. `Html.RenderPlan(plan)` serializes that
intent to JSON validated against `Alis.Reactive/Schemas/reactive-plan.schema.json` (61
definitions). The TypeScript runtime executes plan instructions without adding behavior the
plan does not describe. C# never executes browser behavior. TypeScript never invents
information the plan does not carry.

Throughout this document, "the runtime" means the TypeScript code in
`Alis.Reactive.Assets/Scripts/` that executes plans in the browser. It is
bundled by esbuild into `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js`
(IIFE), shipped inside the AlisReactive NuGet, and copied into the consumer's
`wwwroot/scripts/` (net10) or `Content/alisreactive/` (net48) by the shipped
`AlisReactive.targets` file with the consumer's package version baked into
the filename.

## Architecture — 5 Layers, 4 Boundaries

Each boundary is guarded by a test harness. A failing test is the only reason to cross one.

```
Layer 1  C# Domain Model + Builders
         Quality: DDD value objects, internal constructors, C# 8.0, SOLID
         Harness: AssertSchemaValid() — 70 calls across Core, Fusion, Native test bases
         ↓
         BOUNDARY: failing AssertSchemaValid() drives schema update
         ↓
Layer 2  JSON Schema (Alis.Reactive/Schemas/reactive-plan.schema.json)
         Quality: additionalProperties: false on every object, 61 $defs
         Harness: AssertSchemaValid() in PlanTestBase, FusionTestBase, NativeTestBase
         ↓
         BOUNDARY: schema change → failing vitest drives TS type update
         ↓
Layer 3  TypeScript Types + Runtime
         Quality: discriminated unions match schema, fail-fast, no fallbacks
         Harness: npm run typecheck (vitest configured but no tests on this branch yet)
         ↓
         BOUNDARY: browser first, then Playwright — eyes before automation
         ↓
Layer 4  Browser Verification
         Quality: real interactions, visible outcomes, no page.evaluate()
         Harness: Playwright BDD (5 rules), 69 test fixtures
         ↓
         BOUNDARY: working sandbox example before writing docs
         ↓
Layer 5  Documentation + Skills
         Quality: dev-facing language, verified code examples, no internals vocabulary
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

Top-level JSON shape: `version` (3), `planId`, `partId`?, `types`, `components`, `behaviors`.

`WriteOnlyPolymorphicConverter<T>` enables polymorphic serialization by delegating to the
concrete type via `JsonSerializer.Serialize(writer, value, value.GetType(), options)`. Each
concrete plan model class carries its own `kind` property (e.g., `public string Kind => "set"`)
which becomes the discriminator in the JSON, matched by TypeScript discriminated unions.

Schema validation happens in C# tests via `AssertSchemaValid()`. The runtime trusts the
JSON — it does not re-validate. If the JSON is malformed, `JSON.parse` throws at boot time.

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
`Alis.Reactive.Assets/dist/` or `Alis.Reactive.Fusion/dist/` folder is missing.

### Daily dev loop — 3 terminals

```bash
npm run watch                                    # framework JS → dist/ on every .ts edit
npm run watch:css                                # framework CSS → dist/ on every .css edit
dotnet watch --project Alis.Reactive.SandboxApp  # Razor + C# hot reload
```

Framework TS/CSS edits need **only a browser refresh** — no `dotnet build`, no
sandbox restart. `Program.cs` wires a `CompositeFileProvider` over the bundle
output; `asp-append-version` re-hashes each file per request so the browser
never serves stale bytes. Sandbox-only bundles have their own watchers
(`watch:sandbox-plugins`, `watch:sandbox-css`) — edited rarely.

### The bundles — `npm run build:all` runs 5 steps

Every output path is gitignored; `git status` stays clean after a build.

| npm script | Output | Used by |
|------------|--------|---------|
| `build` | `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js` | sandbox, NuGet |
| `build:css` | `Alis.Reactive.Assets/dist/css/design-system.dev.css` | sandbox, NuGet |
| `build:fusion-css` | `Alis.Reactive.Fusion/dist/css/syncfusion.dev.css` | sandbox, `AlisReactive.Fusion` NuGet |
| `build:sandbox-plugins` | `Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js` | sandbox only |
| `build:sandbox-css` | `Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css` | sandbox only |

The bundles reach three places:

- **Sandbox** — `Program.cs` serves `Alis.Reactive.Assets/dist/` and
  `Alis.Reactive.Fusion/dist/` via `CompositeFileProvider`. No copy into `wwwroot/`.
- **NuGet** — `Alis.Reactive.csproj` packs the core `dist/` bundles into the
  `AlisReactive` package; `Alis.Reactive.Fusion.csproj` packs `syncfusion.dev.css`
  into `AlisReactive.Fusion` the same way. `dotnet pack` never runs npm; the
  `VerifyBundlesExistBeforePack` / `VerifyFusionBundleExistsBeforePack` targets
  fail fast if a bundle is missing.
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

Runs the vitest suite (jsdom). `vitest.config.ts` looks for `*.test.ts` under
`Alis.Reactive.Assets/Scripts/__tests__/` and `Alis.Reactive.SandboxApp/Scripts/__tests__/`.
A branch with no such files (this branch has none) makes vitest print
`No test files found` and exit non-zero — that is the empty-suite signal, not a
failure in your code.

**C# unit tests — fast, no server needed:**

```bash
dotnet build
dotnet test tests/Alis.Reactive.UnitTests                    # Core + schema
dotnet test tests/Alis.Reactive.Native.UnitTests             # Native
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests    # Validation
dotnet test tests/Alis.Reactive.Analyzers.Tests              # Analyzers
dotnet test tests/Alis.Reactive.DesignSystem.Tests           # Design system
dotnet test tests/Alis.Reactive.NativeTagHelpers.Tests       # Tag helpers
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
serving old bytes. Rebuild with `npm run build:all`, or leave `npm run watch` /
`watch:css` running — `CompositeFileProvider` serves the new `dist/` output on the
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
| C# | .NET 10. **C# 8.0 enforced** in 4 library projects (Core, Fusion, Native, FluentValidator). Analyzers uses `latest`. Apps and tests use `latest`. |
| TS | TypeScript 5.8, esbuild ESM, Tailwind CSS v4 |
| Components | Syncfusion EJ2 32.x (Fusion) + Native HTML. Always through DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` |
| Validation | FluentValidation 12.x, extracted to client rules via `FluentValidationAdapter` |
| Tests | NUnit 4.3-4.5, Vitest 3.x + jsdom (configured, no tests yet), Playwright 1.52 |
| Schema | JSON Schema with 61 `$defs`, validated by `JsonSchema.Net` |

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
| `modern-csharp` | C# patterns (needs rewrite — currently promotes C# 12+, repo uses C# 8.0) |
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
4. JSON schema — failing `AssertSchemaValid()` test drives the update
5. TS types — new interface in `Alis.Reactive.Assets/Scripts/types/`, discriminated union with `kind`
6. Runtime handler — new switch case + `assertNever`
7. C# unit test — `AssertSchemaValid()` validates rendered JSON against schema
8. TS unit test — `Alis.Reactive.Assets/Scripts/__tests__/*.test.ts`, runtime behavior via `boot()`
9. Playwright test — browser behavior with sandbox view
10. Sandbox view — demonstrate the primitive

### 3. Vertical Slices — Duplication Over Abstraction

Each component module is self-contained. No shared base classes for behavior.
Duplication between slices is intentional.

### 4. Vendor Isolation

New component = C# vertical slice with `IInputComponent`. Zero TS runtime changes.
`resolver.ts` is the only module that maps vendor to DOM root (`resolveVendorRoot`) and wires
vendor-specific events (`wireEvent`). Adding a third vendor must only touch `resolver.ts` and
add a `resolution/event-{vendor}.ts` file. Vendor checks in other modules violate this rule.

### 5. Fail Fast — Fallbacks Are Exceptions

Default thinking is throw, not fallback. When something is missing or unknown, surface
the error immediately. Fallbacks hide bugs for hours because wrong values propagate silently.
A fallback is a rare, deliberate, justified exception — never the default response to uncertainty.

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
  should enforce valid state. Association and aggregation boundaries are implicit.
  Screaming names (types that express domain intent) are underused.
- **Serialization**: `[JsonIgnore(Condition = WhenWritingNull)]` attributes scattered
  across plan model classes instead of explicit serialization contracts.
- **TS tracing**: `core/trace.ts` is 38 lines using `console.error`/`warn`/`log` dispatched
  by level. Should aspire to OTel-style structured tracing — explicit data flowing through
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
