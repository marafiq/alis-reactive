# Alis.Reactive Framework

ASP.NET MVC developers express reactive browser behavior in C# without writing JavaScript.
Fluent builders (`Html.On`, `PipelineBuilder`, `ElementBuilder`) capture intent as descriptors.
`Html.RenderPlan(plan)` serializes them to JSON validated against `reactive-plan.schema.json`.
The JS runtime discovers the plan on page load and executes it — sets properties, calls methods,
wires listeners, makes HTTP requests. It does not know what a checkbox is or what cascading
dropdowns means. The plan is the only contract — C# never executes behavior, JS never invents it.

## Skills

### Writing .cshtml Views

| Skill | Status | Use for |
|-------|--------|---------|
| `reactive-dsl` | WIP | Plan, triggers, Element, Dispatch, Component, InputField, .Reactive(), SSE/SignalR |
| `http-pipeline` | OK | Get/Post, Gather, Response, Chained, Parallel, WhileLoading, Into |
| `conditions-dsl` | OK | When/Then/ElseIf/Else, operators, guard composition, source types |
| `validation-rules-alis-reactive` | WIP | FluentValidation rules, Validate, ValidationErrors, WhenField |
| `design-system` | Missing | Layout: vstack, hstack, card, grid, heading, text, divider |

### Writing Core C# Framework

Projects: `Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.Fusion`, `Alis.Reactive.FluentValidator`, `Alis.Reactive.Analyzers`, `Alis.Reactive.NativeTagHelpers`

| Skill | Status | Use for |
|-------|--------|---------|
| `onboard-fusion-component` | WIP | Adding SF components, events, methods, props |
| `dotnet-xml-docs` | OK | XML documentation on public types |
| `technical-documentation-writing` | WIP | Writing docs-site pages, architecture guides |

API reference auto-generated from XML docs: `npm run build:api-docs` → `tools/ApiDocGenerator`
Documentation site: `docs-site/` (Starlight + astro-d2)

Skills issues tracked in `docs/todo-skill-updates.md`.

## Build & Run Commands

```bash
# ── Build ──
npm run build:all                # Two JS bundles + CSS, all loaded in _Layout.cshtml
dotnet build                     # All C# projects
npm run build:api-docs           # Regenerate API reference from XML docs

# ── Development ──
npm run watch                    # esbuild watch — rebuilds JS on save
npm run watch:css                # Tailwind watch — rebuilds CSS on save
# Mac/Linux: kill stale Kestrel on port 5220, then run
lsof -ti:5220 | xargs kill -9 2>/dev/null; dotnet run --project Alis.Reactive.SandboxApp
# Windows: netstat -ano | findstr :5220 | findstr LISTENING → taskkill /PID <pid> /F
# then: dotnet run --project Alis.Reactive.SandboxApp

# ── Quality ──
npm run typecheck                # TypeScript type checking
npm run lint                     # ESLint
npm run lint:fix                 # ESLint auto-fix

# ── Tests ──
npm test                                                     # TS vitest
dotnet test tests/Alis.Reactive.UnitTests                    # Core C# unit + schema
dotnet test tests/Alis.Reactive.Native.UnitTests             # Native components
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion components
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests    # FluentValidation
dotnet test tests/Alis.Reactive.Analyzers.Tests              # Roslyn analyzers
dotnet test tests/Alis.Reactive.DesignSystem.Tests           # Design system
dotnet test tests/Alis.Reactive.NativeTagHelpers.Tests       # Tag helpers
# Playwright — always use trx logger so you can re-run only failed tests
dotnet test tests/Alis.Reactive.PlaywrightTests \
  --logger "trx;LogFileName=playwright-results.trx" \
  --results-directory TestResults

# Re-run only failed Playwright tests — Mac/Linux (saves hours)
FAILED=$(grep 'outcome="Failed"' TestResults/playwright-results.trx \
  | sed 's/.*testName="//' | sed 's/".*//' | sort -u \
  | grep -v "ResultSummary" | paste -sd '|' -)
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "$FAILED" \
  -- NUnit.NumberOfTestWorkers=1
# Windows: open playwright-results.trx, find Failed test names, use --filter "Name1|Name2"

# Re-run a single test
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "TestName"

# ── SonarQube (optional, requires Docker) ──
./scripts/sonar-analyze.sh
```

**After any TS or CSS change:** `npm run build:all && dotnet build`, restart SandboxApp, then run Playwright.

ALL tests must pass before every commit. Hooks enforce this.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| C# | .NET 10, **C# 8.0 enforced** in `Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.Fusion`, `Alis.Reactive.FluentValidator`. Apps/tests use latest. |
| TS | TypeScript 5.8, esbuild (ESM bundle), Tailwind CSS v4 — no raw JS, all `.ts` files |
| Components | `Alis.Reactive.Fusion` (wraps SF EJ2 32.x), `Alis.Reactive.Native` (wraps native HTML inputs). Never use raw `<input>` or SF builders directly — always through the DSL: `Html.InputField(plan, m => m.Name).NativeTextBox(build: b => ...)` or `Html.InputField(plan, m => m.Country).FusionDropDownList(build: b => ...)` |
| Validation | FluentValidation 12.x |
| Tests | NUnit 4.5 + Verify, Vitest 3.x + jsdom, Playwright 1.52 |

## SOLID Design Principles — Applied to This Codebase

**Single Responsibility:** Each vertical slice owns one component end-to-end (C# descriptor,
builder, extensions, events, reactive wiring, gather, tests). Each TS module does one job
(`component.ts` = vendor roots, `resolver.ts` = source binding, `element.ts` = mutations).
When adding behavior, put it in the module that owns that concern — don't spread it.

**Open/Closed:** New components and command kinds extend the framework without modifying
existing modules. A new Fusion component = new C# vertical slice, zero TS changes.
A new command kind = new handler in `commands.ts`, existing handlers untouched.

**Liskov Substitution:** All components implement `IComponent`/`IInputComponent` — the runtime
treats Native and Fusion identically through `resolveRoot()`. Adding a third vendor must
work without `if (vendor === "newVendor")` hacks in existing modules.

**Interface Segregation:** `IComponent` (vendor only), `IInputComponent` (vendor + readExpr),
`IAppLevelComponent` (vendor + defaultId). Components implement only what they need.
`ICommandEmitter` is the narrow interface for adding commands — not the full `PipelineBuilder`.

**Dependency Inversion:** Builders depend on descriptor interfaces, not concrete types.
Runtime depends on plan JSON shape, not C# types. The schema is the shared contract —
neither side references the other's implementation.

**Recurring violations — these cost hours every session:**

Code quality:
- Repeated `switch`/`if` chains instead of polymorphism or lookup tables
- Nested `if` blocks instead of early returns (guard clauses)
- Long methods doing 3+ things — extract, name the sub-operation
- High cognitive complexity — flatten, simplify, break apart
- Poor naming — types and methods must describe domain role, not implementation
- Calling something "v1" or "good enough for now" — this is a production system, ship it right

Visibility discipline:
- Making `internal` → `public` to "fix" a compilation error (breaks encapsulation)
- No sense of `private`/`internal`/`public` — everything defaults to public out of laziness
- Tests treat internal API as if it were public — use the DSL entry points only

Architecture:
- Adding `if (type === "checkbox")` heuristics in runtime (plan should carry the info)
- Creating duplicate abstractions to "get by" instead of fixing the existing one
- String-matching on type names instead of using proper interfaces
- Silent fallbacks that hide misconfiguration for hours
- Vertical slice isolation not enforced in tests and sandbox views
- Shallow module design — trace module in TS is too basic, lacks deep thought
- Forgetting sandbox index cards when adding new pages

Testing & debugging:
- Root cause analysis skipped — patch-fix cycles that create 10+ commits fixing symptoms
- When Playwright tests fail: forgetting to open traces, inspector, or real browser to debug
- Guessing for 2 days instead of 5 minutes of research (SF DropDownList ArrowDown incident)
- Claiming "all tests pass" without verifying in actual browser — tests pass ≠ working software
- Writing surface BDD tests that assert true/false, not full user journeys

Process:
- Rubber-stamping audits — saying PASS without tracing runtime paths
- Agreeing blindly instead of pushing back with evidence
- Dispatching agents without roles, context, or evidence demands
- Patching after user said "stop patching" — 5+ times in one session

## Rules

### 1. Git Worktrees for Feature Work

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

### 2. Plan Is the Only Contract

No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks — `root.ts` handles discovery and boot automatically.

### 3. Every New Primitive Needs All Three Layers

Adding a new command kind, trigger kind, or reaction kind touches these files in order:

1. **C# descriptor** — `Alis.Reactive/Descriptors/` (new sealed class, `internal` constructor)
2. **Polymorphic registration** — add to `WriteOnlyPolymorphicConverter` switch in parent type
3. **Builder method** — on `PipelineBuilder`, `ElementBuilder`, or `TriggerBuilder`
4. **JSON schema** — new `$ref` in `reactive-plan.schema.json` (schema MUST match descriptor output)
5. **TS types** — new interface in `Scripts/types/`, add to discriminated union
6. **Runtime handler** — new case in `commands.ts` or `execute.ts`
7. **C# unit test** — snapshot (`VerifyJson`) + schema validation (`AssertSchemaValid`)
8. **TS unit test** — runtime behavior in jsdom via `boot()`
9. **Playwright test** — browser behavior with sandbox view
10. **Sandbox view** — demonstrate the new primitive in a page

**Schema drift is a known risk.** Descriptors and schema can silently diverge.
TODO: Build a hook/script that validates descriptor JSON output against schema on every build.

### 4. Two-Phase Boot Is Inviolable

Custom-event listeners wire before dom-ready reactions execute. This ensures
`dom-ready → dispatch("x")` fires after someone listens for `"x"`.

### 5. Vertical Slices — Duplication Over Abstraction

Each module is self-contained. No shared base classes for behavior.
Duplication between slices is intentional.

### 6. Component Architecture — Vendor Isolation

Adding a new component = new C# vertical slice with `IInputComponent`. Zero TS changes.
`component.ts` is the ONLY module that maps vendor → root. All other modules call it:

```typescript
// CORRECT — component.ts owns vendor knowledge
const root = resolveRoot(el, vendor);   // native → el, fusion → el.ej2_instances[0]
const value = evalRead(id, vendor, readExpr); // resolveRoot + walk(readExpr)

// WRONG — vendor check leaked into trigger.ts:45
if (trigger.vendor === "native") { detail = { [expr]: walk(el, expr) }; }
else { detail = e ?? {}; }

// WRONG — vendor check leaked into live-clear.ts:44
if (field.vendor === "native") { ... }
```

Adding a third vendor must only touch `component.ts`. If you need vendor behavior elsewhere,
add an export to `component.ts` — never `if (vendor === "x")` in other modules.
Use the `onboard-fusion-component` skill for the full vertical slice pattern.

### 7. No Fallbacks — Fail Fast

When writing framework and core abstractions, there is only one way to build the right thing.
Fallbacks and escape hatches are rare exceptions, not defaults.

**Throw immediately when:**
- Component not in `ComponentsMap` → `throw` — dev forgot `Html.InputField()`
- Unknown vendor string → `throw` — typo or missing `IComponent.Vendor`
- Missing `readExpr` → `throw` — component doesn't implement `IInputComponent`
- Gather resolves to empty → `throw` — dev forgot to register component in plan
- Unknown command kind in runtime → `assertNever()` — schema/descriptor drift

**Never:**
- Return `null` or empty string as "safe" default — hides the bug for hours
- Silently skip an unregistered component — wrong field gets validated/gathered
- Fall back to `"value"` when `readExpr` is missing — checkbox reads wrong property
- Catch and swallow exceptions in builders — dev never sees the misconfiguration

### 8. No DOM Scanning — IDs Are Plan-Driven

The runtime never scans the DOM to discover components. Every element ID is generated by
`IdGenerator` from the model expression at C# render time and carried in the plan JSON.

```
C#: Html.InputField(plan, m => m.Address.City)
  → IdGenerator.For<OrderModel>(m => m.Address.City)
  → "Alis_Reactive_SandboxApp_Models_OrderModel__Address_City"

Format: {Namespace_TypeName}__{MemberPath}  (double underscore separates scope from property)

Plan JSON: { "target": "Alis_Reactive_SandboxApp_Models_OrderModel__Address_City", "vendor": "fusion" }
Runtime: document.getElementById("Alis_Reactive_...") — direct lookup, zero scanning
```

**Never write `querySelectorAll`, `getElementsByClassName`, or DOM traversal in runtime TS.**
If you think you need to scan the DOM, the plan is missing information — fix the C# descriptor
to carry it. Ask explicit user permission before adding any DOM scanning code.

### 9. API Surface Is Frozen

The public API surface is **frozen**. A hookify rule (`.claude/hookify.protect-api-surface.local.md`)
enforces this at the tool level.

**To change ANY public API, you must provide:**
1. What is changing and why (specific problem, not "cleanup")
2. Grep of all affected call sites
3. Impact on views, tests, docs, examples, and skills
4. Explicit user approval

**Locked conventions:**
- Descriptor constructors: `internal`
- Changing `internal` to `public`: **strictly forbidden**
- Fusion methods use Fusion prefix: `.FusionDropDownList()`, `.FusionNumericTextBox()`, etc.
- Parameter names: `pipeline`, `build`, `trigger`, `gather`, `response`, `request`, `guard`, `inner`

### 10. Fixing Bugs — Root Cause, Not Patch

Trace the full code path. Identify the exact line. Understand WHY the current code does
what it does. Fix the root cause. Verify in the actual browser. Run ALL tests.
Never revert agreed-upon architecture to fix a symptom.
Never change `internal` to `public` to get around a compilation error.
Never duplicate a core abstraction to "get by" — fix the existing one.

### 11. ESM Only + Cache Busting

ESM bundle via esbuild. `asp-append-version="true"` on CSS/JS tags for cache busting.
No IIFE, no `window.alis`, no import maps.

### 12. Verified Snapshots Are Co-Located

`.verified.txt` files live next to their test class. Call `VerifyJson()` directly.

### 13. BDD Tests — Public API Only

Test classes: `When{Scenario}`. Test methods describe expected behavior.
Tests verify what the system does for the user, not implementation details.
Arrange tests using the public DSL (`Html.On`, `CreatePlan()`, `Trigger()`) only.

**Known violation:** 30 instances across 10 test files directly construct internal types
(`new SequentialReaction`, `new DispatchCommand`, `new Entry`, etc.). These need refactoring
to use the builder API. See `docs/todo-skill-updates.md`.

