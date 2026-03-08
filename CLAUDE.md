# CLAUDE.md — Alis.Reactive Framework

> **This document is law.** The architecture is a SOLID loop: C# DSL builds descriptors,
> descriptors serialize to a JSON plan, the JS runtime executes the plan. Nothing else.

## The SOLID Loop

```
C# DSL (Descriptors)  →  JSON Plan  →  JS Runtime (Executes Plan)
         │                    │                    │
    Compile-time          Schema-validated      Browser-verified
    type safety           structure             behavior
         │                    │                    │
    Unit Tests            Schema Tests          Playwright Tests
    (Verify snapshots)    (JSON Schema)         (DOM assertions)
```

**The plan is the only contract between C# and JS.** The C# DSL never executes behavior —
it builds descriptors. The JS runtime never invents behavior — it executes plan primitives.
No manual JS in views. Ever.

## Architecture

### Layer 1: C# DSL (Descriptor Builders)

The developer writes C# in `.cshtml` views. Every call builds a descriptor and adds it to
the `ReactivePlan`. Nothing executes at this point.

```csharp
@{
    IReactivePlan<MyModel> plan = new ReactivePlan<MyModel>();

    Html.On(plan, t => t.DomReady(p =>
    {
        p.Element("status").AddClass("active");
        p.Element("status").SetText("loaded");
        p.Dispatch("ready");
    }));
}
<script type="application/json" id="alis-plan">@Html.Raw(plan.Render())</script>
```

**Key files:**

| File | Purpose |
|------|---------|
| `Alis.Reactive/IReactivePlan.cs` | Plan interface + `ReactivePlan<T>` (collects entries, renders JSON) |
| `Alis.Reactive/Builders/TriggerBuilder.cs` | `DomReady()`, `CustomEvent()`, `CustomEvent<T>()` |
| `Alis.Reactive/Builders/PipelineBuilder.cs` | `Dispatch()`, `Element()` — the reaction command builder |
| `Alis.Reactive/Builders/ElementBuilder.cs` | `AddClass()`, `RemoveClass()`, `ToggleClass()`, `SetText()`, `SetHtml()`, `Show()`, `Hide()` |
| `Alis.Reactive/Descriptors/Commands/Command.cs` | `DispatchCommand`, `MutateElementCommand` (polymorphic JSON) |
| `Alis.Reactive/Descriptors/Triggers/Trigger.cs` | `DomReadyTrigger`, `CustomEventTrigger` (polymorphic JSON) |
| `Alis.Reactive/Descriptors/Reactions/Reaction.cs` | `SequentialReaction` (list of commands) |
| `Alis.Reactive/Descriptors/Entry.cs` | One entry = trigger + reaction |
| `Alis.Reactive/ExpressionPathHelper.cs` | Converts `x => x.Address.City` to `"evt.address.city"` dot-path for source binding |
| `Alis.Reactive.Native/Extensions/HtmlExtensions.cs` | `Html.On()` extension — the view entry point |

### Layer 2: JSON Plan (Schema-Validated Contract)

`plan.Render()` serializes all entries to JSON using `System.Text.Json` with `camelCase`
naming and polymorphic type discriminators (`"kind"` property).

**Schema:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

**Plan structure:**
```json
{
  "entries": [
    {
      "trigger": { "kind": "dom-ready" | "custom-event", "event?": "name" },
      "reaction": {
        "kind": "sequential",
        "commands": [
          { "kind": "dispatch", "event": "name", "payload?": {} },
          { "kind": "mutate-element", "target": "elementId", "action": "add-class", "value?": "className" }
        ]
      }
    }
  ]
}
```

**Command kinds:**

| Kind | Fields | Runtime behavior |
|------|--------|-----------------|
| `dispatch` | `event`, `payload?` | `document.dispatchEvent(new CustomEvent(event, { detail: payload }))` |
| `mutate-element` | `target`, `action`, `value?` | `document.getElementById(target)` → execute action |

**Mutate actions:** `add-class`, `remove-class`, `toggle-class`, `set-text`, `set-html`, `show`, `hide`

**Source binding (BindExpr):** `set-text` and `set-html` support a `source` field — a `BindExpr`
(dot-notation path into execution context) instead of a static `value`. The runtime's `resolver.ts`
module walks `"evt.address.city"` against the event's `detail` object at execution time.

C# DSL: `p.Element("x").SetText(payload, x => x.Address.City)` →
`ExpressionPathHelper` converts the expression to `"evt.address.city"`.

**Resolver (reusable module):** `resolver.ts` exports `resolve()`, `resolveAs()`, `resolveToString()`,
and `coerce()`. Coercion types: `string` (null→""), `number` (NaN→0), `boolean` ("false"→false), `raw`.
Schema defines `BindExpr` and `CoercionType` as reusable `$defs`. This module will be used by
the Conditions module for guard evaluation, not just DOM mutations.

### Layer 3: JS Runtime (Plan Executor)

ESM module bundled by esbuild. The runtime reads the plan JSON and executes it. Nothing more.

**Key files:**

| File | Purpose |
|------|---------|
| `Scripts/auto-boot.ts` | esbuild entry point — auto-discovers `#alis-plan`, reads `data-trace`, calls `boot()` |
| `Scripts/boot.ts` | Two-phase boot: wire custom-event listeners first, then execute dom-ready (testable export) |
| `Scripts/trigger.ts` | `wireTrigger()` — wires dom-ready and custom-event listeners |
| `Scripts/execute.ts` | `executeReaction()` → `executeCommand()` — dispatch to command handlers |
| `Scripts/element.ts` | `mutateElement()` — resolves element by ID, executes mutation action |
| `Scripts/resolver.ts` | `resolve()`, `resolveAs()`, `resolveToString()`, `coerce()` — BindExpr dot-path resolution with type coercion |
| `Scripts/trace.ts` | `scope()`, `setLevel()` — deterministic single-string trace output |
| `Scripts/types.ts` | TypeScript interfaces mirroring the JSON plan schema |

**Two-phase boot (critical invariant):**
Custom-event listeners MUST be wired before dom-ready reactions execute. Otherwise
`dom-ready → dispatch("x")` fires before anyone listens for `"x"`.

```typescript
// Phase 1: wire all non-dom-ready triggers
// Phase 2: execute dom-ready triggers (which may dispatch into phase 1 listeners)
```

**View bootstrap (zero inline scripts):**

The layout loads the runtime via `<script type="module" src>` with cache busting.
Views emit only the plan JSON element — no inline scripts at all.

```html
<!-- _Layout.cshtml — loads once, cache-busted via SHA256 hash -->
<link rel="stylesheet" href="~/css/alis-modern-tailwind.css" asp-append-version="true"/>
<script type="module" src="~/js/alis-reactive.js" asp-append-version="true"></script>

<!-- View — plan element only, auto-boot discovers it -->
<script type="application/json" id="alis-plan" data-trace="trace">@Html.Raw(planJson)</script>
```

**Auto-boot architecture:**

| File | Role |
|------|------|
| `Scripts/auto-boot.ts` | esbuild entry point — auto-discovers `#alis-plan`, reads `data-trace`, calls `boot()` |
| `Scripts/boot.ts` | Testable export — `boot()` and `trace` used by vitest, NOT the browser entry point |

`auto-boot.ts` runs on every page load. If `#alis-plan` exists, it boots. If `data-trace`
is set, it enables tracing. This eliminates per-view inline scripts entirely.

## Projects

| Project | Purpose | Dependencies |
|---------|---------|-------------|
| `Alis.Reactive` | Core framework — descriptors, builders, plan, schema | None (zero deps) |
| `Alis.Reactive.Native` | MVC extensions — `Html.On()` | Core + ASP.NET |
| `Alis.Reactive.Fusion` | Syncfusion component builders (future) | Core + SF EJ2 |
| `Alis.Reactive.SandboxApp` | MVC app that uses the DSL + hosts the runtime | Native + ASP.NET |

## Test Coverage — Three Layers, 100% (BDD)

All tests are **behavior-driven** — named after what the system does, not what the code does.
Test classes follow `When{Scenario}`, test methods describe the expected behavior.

### Layer 1: C# Unit Tests (Verify + Schema)

`tests/Alis.Reactive.UnitTests/` — NUnit + Verify.NUnit + JsonSchema.Net

**Pattern:** Each test calls the C# DSL, renders the plan, and either:
- **Snapshot verifies** (`VerifyJson(plan.Render())`) — captures exact JSON output
- **Schema validates** (`AssertSchemaValid(plan.Render())`) — validates against `reactive-plan.schema.json`

**Test files:**

| File | What it tests |
|------|--------------|
| `Commands/WhenDispatchingAnEvent.cs` | Dispatch with/without payload, multiple commands |
| `Commands/WhenMutatingAnElement.cs` | All 7 actions: AddClass, RemoveClass, ToggleClass, SetText, SetHtml, Show, Hide + mixed chains |
| `Commands/WhenResolvingPayloadSource.cs` | Source-based SetText/SetHtml from event payload: flat primitives (int, long, double, string, bool) + nested dot-paths (address.city, address.zip) |
| `Triggers/WhenTriggeringOnDomReady.cs` | DomReady trigger serialization |
| `Triggers/WhenTriggeringOnCustomEvent.cs` | CustomEvent trigger, typed payload |
| `Triggers/WhenTriggeringOnCustomEventWithAllSupportedTypes.cs` | All primitive types in payload |
| `Plan/WhenComposingAPlan.cs` | Empty plan, multiple entries, empty pipeline |
| `Schema/AllPlansConformToSchema.cs` | Every command kind × every trigger kind validated against JSON schema |

**Conventions:**
- `.verified.txt` files live in the same folder as the test class
- Tests call `VerifyJson()` directly (not through base class) for correct `[CallerFilePath]`
- `PlanTestBase` provides `CreatePlan()`, `Trigger()`, `AssertSchemaValid()`

**Run:** `dotnet test tests/Alis.Reactive.UnitTests`

### Layer 2: TS Unit Tests (Runtime Logic)

`Scripts/__tests__/` — Vitest + jsdom

**Pattern:** Each test creates a plan object, calls `boot()`, and asserts DOM state or event dispatches.

| File | What it tests |
|------|--------------|
| `when-composing-a-plan.test.ts` | Plan structure, entry count |
| `when-triggering-on-dom-ready.test.ts` | Dom-ready fires reaction |
| `when-triggering-on-custom-event.test.ts` | Custom event listener → reaction |
| `when-dom-ready-chains-into-custom-event.test.ts` | Two-phase boot regression test |
| `when-dispatching-an-event.test.ts` | Dispatch fires CustomEvent on document |
| `when-dispatching-a-custom-event-with-payload.test.ts` | All primitive types survive serialization |
| `when-mutating-an-element.test.ts` | All 7 mutate actions + mixed chains |
| `when-resolving-payload-source.test.ts` | Source dot-path resolution: flat int/string/bool, nested address.city/street/zip, missing path fallback |

**Run:** `npm test`

### Layer 3: Playwright Tests (Browser Behavior)

`tests/Alis.Reactive.PlaywrightTests/` — Playwright.NUnit

**Pattern:** Tests navigate to the sandbox app, wait for trace messages in console, and assert
DOM state and trace output. `PlaywrightTestBase` captures all console messages and dumps them
on test failure.

| File | What it tests |
|------|--------------|
| `Events/WhenPageLoads.cs` | Page title, content sections, plan JSON rendered, nav links |
| `Events/WhenEventChainFires.cs` | 3-hop dispatch chain, chronological order, payload, DOM mutations |
| `Events/WhenTraceIsEnabled.cs` | Boot trace in console, trace format |
| `Payload/WhenPayloadPropertiesResolve.cs` | All primitives (int, long, double, string, bool) + nested (address.street, city, zip) resolved from event payload to DOM text |

**Infrastructure:**
- `WebServerFixture.cs` — Assembly-level setup, starts Kestrel on port 5220
- `PlaywrightTestBase.cs` — Console capture, `WaitForTraceMessage()`, `AssertTraceContains()`, `AssertNoConsoleErrors()`

**Run:** `dotnet test tests/Alis.Reactive.PlaywrightTests`

## Build Commands

```bash
# JS runtime (ESM bundle — entry: auto-boot.ts)
npm run build                    # → wwwroot/js/alis-reactive.js (2kb)

# Tailwind CSS (v4)
npm run build:css                # → wwwroot/css/alis-modern-tailwind.css

# Both JS + CSS
npm run build:all

# TypeScript typecheck
npm run typecheck

# All tests
npm test                                                    # TS unit tests (vitest)
dotnet test tests/Alis.Reactive.UnitTests                   # C# unit + schema tests
dotnet test tests/Alis.Reactive.PlaywrightTests              # Browser tests

# Full build
dotnet build                                                 # All C# projects
```

**After any TS or CSS change:** run `npm run build:all` then `dotnet build` to ensure
`asp-append-version` picks up the new SHA256 hash. Stale bundles = stale browser cache.

## Rules

### 1. Plan Is the Only Contract

The C# DSL builds descriptors → `plan.Render()` serializes to JSON → JS runtime executes.
No shortcuts. No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks in views — `auto-boot.ts` handles discovery and boot automatically.

### 2. Every New Primitive Needs All Three Layers

Adding a new command, trigger, or action requires:
1. **C# descriptor** — new `Command` subclass with `[JsonDerivedType]`
2. **Builder method** — on `PipelineBuilder`, `ElementBuilder`, or `TriggerBuilder`
3. **JSON schema update** — new definition in `reactive-plan.schema.json`
4. **Runtime handler** — in `execute.ts` or its own module
5. **TS types** — in `types.ts`
6. **C# unit test** — snapshot + schema validation
7. **TS unit test** — runtime behavior in jsdom
8. **Playwright test** — browser behavior verification
9. **Sandbox view usage** — demonstrate in Events page (or new page)

### 3. Two-Phase Boot Is Inviolable

Custom-event listeners wire before dom-ready reactions execute. This ensures event chains
(`dom-ready → dispatch → custom-event → dispatch → ...`) work regardless of entry order in
the plan. Tests verify this at all three layers.

### 4. Trace Output Is Deterministic

`trace.ts` serializes to a single `JSON.stringify` string per message: `[alis:scope] msg {"key":"val"}`.
This makes trace output testable in Playwright via string matching. No separate `console.log`
arguments (browser-dependent formatting).

### 5. Verified Files Are Co-Located

`.verified.txt` snapshot files live in the same folder as their test class. Tests must call
`VerifyJson()` directly (not through a base method) so `[CallerFilePath]` resolves correctly.

### 6. Vertical Slices

Each runtime module (`element.ts`, `trigger.ts`, `execute.ts`) is a self-contained unit.
Each C# builder (`ElementBuilder`, `PipelineBuilder`, `TriggerBuilder`) is a self-contained unit.
No shared base classes for behavior. Duplication between slices is intentional.

### 7. ESM Only + Cache Busting

The runtime is bundled as ESM (`--format=esm`). The layout loads it via
`<script type="module" src="~/js/alis-reactive.js" asp-append-version="true">`.
No IIFE, no `window.alis`, no import maps, no inline `<script type="module">` blocks.

**Cache busting:** `asp-append-version="true"` on both CSS and JS `<link>`/`<script>` tags
appends `?v=SHA256hash` — browser always gets the latest build. This works because the
layout uses `src=` (not inline `import from`), and ASP.NET's tag helpers compute the hash.

## Feedback Loop

```
1. Write C# descriptor + builder method
2. Update JSON schema
3. Write runtime handler (TS module)
4. Update TS types
5. npm run build                          # Bundle runtime
6. npm test                               # TS unit tests pass
7. dotnet test UnitTests                  # C# unit + schema tests pass
8. npm run build:css                      # Rebuild Tailwind (if view changed)
9. dotnet build                           # All C# compiles
10. dotnet test PlaywrightTests           # Browser tests pass
11. If anything fails → fix and re-run from step 5
```

Every change goes through all three test layers before it's done. No exceptions.

## Pre-Commit Verification (MANDATORY)

**Before every commit, ALL tests must pass. No exceptions.**

```bash
# Full test suite — run all three from the repo root:
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive

# 1. TS unit tests (vitest + jsdom) — 25 tests
npm test

# 2. C# unit + schema tests (NUnit + Verify + JsonSchema.Net) — 35 tests
dotnet test tests/Alis.Reactive.UnitTests

# 3. Playwright browser tests (browser behavior) — 10 tests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

If any test fails, fix the issue and re-run ALL tests before committing.
Never commit with failing tests. Never skip Playwright.
