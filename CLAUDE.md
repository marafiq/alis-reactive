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
          { "kind": "mutate-element", "target": "elementId", "method": "add", "chain": "classList", "value": "className" }
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
| `mutate-element` | `target`, `prop?`/`method?`, `value?`, `source?` | resolveRoot → bracket notation: `root[prop]=val` or `root[method](val)` |

**Structured mutations (prop/method):** The plan carries structured `prop` or `method` fields.
The runtime resolves the vendor root via `resolveRoot(domEl, vendor)`, then uses bracket notation:
`root[prop] = val` for property sets, `root[method](val)` for method calls. Zero `new Function()`,
zero `eval`, CSP-compatible. No switch statements, no action enums.

**Six mutation patterns:**

| C# DSL method | Plan JSON | Runtime execution |
|----------------|-----------|-------------------|
| `AddClass("x")` | `{ method: "add", chain: "classList", value: "x" }` | `root.classList.add("x")` |
| `RemoveClass("x")` | `{ method: "remove", chain: "classList", value: "x" }` | `root.classList.remove("x")` |
| `ToggleClass("x")` | `{ method: "toggle", chain: "classList", value: "x" }` | `root.classList.toggle("x")` |
| `SetText("x")` | `{ prop: "textContent", value: "x" }` | `root.textContent = "x"` |
| `SetHtml("x")` | `{ prop: "innerHTML", value: "x" }` | `root.innerHTML = "x"` |
| `Show()` | `{ method: "removeAttribute", args: ["hidden"] }` | `root.removeAttribute("hidden")` |
| `Hide()` | `{ method: "setAttribute", args: ["hidden", ""] }` | `root.setAttribute("hidden", "")` |

**Component mutations use the same patterns:**

| Pattern | Example | Plan JSON |
|---------|---------|-----------|
| Prop set | `SetValue("x")` | `{ prop: "value", value: "x", vendor: "fusion" }` |
| Prop set + coerce | `SetValue(42m)` | `{ prop: "value", value: "42", coerce: "number", vendor: "fusion" }` |
| Void method | `Focus()` | `{ method: "focus", vendor: "fusion" }` |
| Method + arg | `SetItems(source)` | `{ method: "setItems", source: {...}, vendor: "fusion" }` |

Adding a new component = just a `prop` name or `method` name in the C# extension.
Zero runtime changes. `resolveRoot` is the vendor-neutral execution layer.

**Source binding (BindExpr):** `value` provides a static val. `source` provides a BindExpr
(dot-notation path into execution context) — resolved at runtime via `resolver.ts`.
The runtime resolves: `source` present → `resolveToString(source, ctx)` → val. Otherwise → `value`.

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
| `Alis.Reactive.Native` | Native DOM component vertical slices + `Html.On()` | Core + ASP.NET |
| `Alis.Reactive.Fusion` | Syncfusion component vertical slices | Core + SF EJ2 |
| `Alis.Reactive.FluentValidator` | FluentValidation integration — `IValidationExtractor` | Core + FluentValidation |
| `Alis.Reactive.SandboxApp` | MVC app that uses the DSL + hosts the runtime | Native + Fusion + ASP.NET |

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

**Pattern:** Integration tests call `boot()` and assert DOM state. Direct unit tests import
resolver functions and test every edge case in isolation.

| File | What it tests |
|------|--------------|
| `when-composing-a-plan.test.ts` | Plan structure, entry count |
| `when-triggering-on-dom-ready.test.ts` | Dom-ready fires reaction |
| `when-triggering-on-custom-event.test.ts` | Custom event listener → reaction |
| `when-dom-ready-chains-into-custom-event.test.ts` | Two-phase boot regression test |
| `when-dispatching-an-event.test.ts` | Dispatch fires CustomEvent on document |
| `when-dispatching-a-custom-event-with-payload.test.ts` | All primitive types survive serialization |
| `when-mutating-an-element.test.ts` | All 7 mutate actions + mixed chains |
| `when-resolving-payload-source.test.ts` | Source dot-path resolution via boot(): flat int/string/bool, nested, missing path fallback |
| `when-resolving-bind-expr.test.ts` | Direct resolve()/resolveAs()/resolveToString() tests: flat/nested/edge cases, condition-ready patterns (numeric, presence, truthiness, emptiness, text, range) |
| `when-coercing-resolved-values.test.ts` | Direct coerce() tests: all 4 types (string, number, boolean, raw) with every boundary value |

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
dotnet test tests/Alis.Reactive.Native.UnitTests             # Native component tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests             # Fusion component tests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests    # FluentValidator tests
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

### 7. Component Architecture (3 Foundational Modules — Never Violate)

Every component interaction flows through three TS runtime modules. No module reinvents
vendor-specific behavior. No module writes `ej2_instances` inline. Adding a new component
type requires ZERO runtime changes — only a new C# vertical slice with `IInputComponent`.

#### Component Interfaces (Instance Properties — No Reflection)

Component metadata is declared via C# 8.0 interface properties, not attributes or reflection:

| Interface | Properties | Purpose |
|-----------|------------|---------|
| `IComponent` | `Vendor` | Base — declares vendor ("native" or "fusion") |
| `IInputComponent` | `Vendor`, `ReadExpr` | Input components — declares property path for reading |
| `IAppLevelComponent` | `Vendor`, `DefaultId` | App-level singletons with well-known element IDs |

Every component is a `sealed class` with `new()` constraint. `ComponentRef<TComponent>` caches
a static `TComponent` instance — vendor and readExpr are resolved once at builder creation time,
not at runtime via reflection.

#### ComponentsMap — Single Source of Truth

`IReactivePlan.ComponentsMap` is populated when component builders register their components.
It maps `bindingPath → ComponentRegistration(componentId, vendor, bindingPath, readExpr)`.
Used by:
- **GatherResolver** — resolves gather placeholders to structured `ComponentSource` at render time
- **ValidationResolver** — resolves validation rules to the correct component read expressions
- Never queried at JS runtime — all resolution happens at C# render time

#### BindSource — Structured Source Binding (No Raw Strings)

`BindSource` is a polymorphic type discriminated by `"kind"`:

| Kind | Class | Fields | Use case |
|------|-------|--------|----------|
| `"event"` | `EventSource` | `path` | Event payload binding: `evt.address.city` |
| `"component"` | `ComponentSource` | `componentId`, `vendor`, `readExpr` | Component value reading |

`TypedComponentSource<TProp>` preserves the typed condition pipeline — created by
`ComponentRef.ReadProperty<TProp>()`, flows through `When()` guard conditions with
full type safety, and serializes to `ComponentSource` at render time.

#### MutateElementCommand — Vendor-Aware Mutations

`MutateElementCommand` carries an optional `vendor` field. When present, the runtime
resolves the vendor root (e.g., `ej2_instances[0]` for Fusion) via `resolveRoot()`.
When absent (plain DOM mutations), `root` is the raw DOM element. Bracket notation
(`root[prop]=val` or `root[method](val)`) works identically for both vendors.

#### walk.ts — Dot-Path Walking (Framework Fundamental)

Pure utility. Zero side effects, zero DOM, zero vendor knowledge.

`walk(root, "a.b.c")` → `root.a.b.c`

| C# Expression | ExpressionPathHelper | Runtime |
|----------------|---------------------|---------|
| `x => x.Address.Street` | `"evt.address.street"` | `walk(ctx, "evt.address.street")` |
| `IInputComponent.ReadExpr` | `"value"` / `"checked"` | `walk(root, "value")` |

**Right side vs Left side:**
- **Right side (source):** walk ANY object (event, response, etc.) — vendor-agnostic → `walk.ts`
- **Left side (target):** component interaction — vendor-AWARE (resolveRoot) → `component.ts`
- walk feeds `val` into bracket notation — works for property assignment AND method calls with arguments

#### component.ts — Vendor Root Resolution

Single source of truth for vendor → root. The ONLY module that knows about `ej2_instances`.

| Vendor | Root | readExpr example |
|--------|------|------------------|
| `native` | `el` (DOM element) | "value" → el.value, "checked" → el.checked |
| `fusion` | `el.ej2_instances[0]` (SF instance) | "value" → ej2.value |

Exports: `resolveRoot(el, vendor)`, `evalRead(id, vendor, readExpr)`
Used by: `gather.ts`, `validation.ts`, `trigger.ts`

#### resolver.ts — BindExpr / Source Resolution

Walks event payload paths (`evt.address.city`) against ExecContext using `walk()`.
Separate from component.ts — different root (ExecContext vs DOM element).

#### How all interactions flow through the modules

| Interaction | C# vertical slice owns | Runtime module | Pattern |
|-------------|----------------------|----------------|---------|
| Property read | vendor + readExpr | component.ts | resolveRoot + walk(readExpr) |
| Property write | prop field | element.ts | root[prop] = val (bracket notation) |
| Method call (void) | method field | element.ts | root[method]() (bracket notation) |
| Method call (args) | method + source | element.ts + resolver.ts | walk(ctx, source) → val → root[method](val) |
| Event wiring | vendor + jsEvent | trigger.ts | resolveRoot + .addEventListener() |
| Source binding | BindSource (structured) | resolver.ts | EventSource or ComponentSource → val |

#### "Value" is a singular concept

Read and write are two sides of the same property:
- NativeCheckBox: reads `el.checked` (readExpr), writes `{ prop: "checked", coerce: "boolean" }`
- FusionNumericTextBox: reads `ej2.value` (readExpr), writes `{ prop: "value", coerce: "number" }`

#### The Cardinal Rule: Plan carries ALL behavior. Runtime NEVER invents.

- No `if (el.type === "checkbox")` heuristics in runtime
- No `readExpr.startsWith("comp.")` prefix conventions
- No fallback defaults — every component explicitly declares readExpr via `IInputComponent`
- New component = new C# vertical slice with `IInputComponent`. Zero TS changes.
- `component.ts` is the ONLY module with `ej2_instances` — zero vendor logic elsewhere

#### Adding a New Component Type (Zero Runtime Changes)

1. C# sealed class implementing `IComponent` + `IInputComponent` (declares Vendor + ReadExpr as instance properties)
2. Event args class (typed payload properties)
3. Events singleton (TypedEventDescriptor registry)
4. Extensions (structured prop/method fields for property writes, method calls, read expressions)
5. Builder (IHtmlContent, renders HTML, registers component in ComponentsMap)
6. Reactive extension (.Reactive() creates ComponentEventTrigger)
7. Gather extension (uses TComponent.ReadExpr via `new()` constraint)
8. Tests at all 3 layers

The runtime does NOT change. The plan JSON carries vendor, readExpr, prop/method, jsEvent.
The runtime resolves root, walks paths, and executes via bracket notation. That's it.

#### Architecture Regression Test

`/Sandbox/Architecture` page uses TestWidget (real JS component with ej2_instances pattern)
to exercise ALL interaction types end-to-end. Playwright tests verify every module path.
If any module breaks vendor-agnostic architecture, these tests catch it immediately.

### 8. No Fallbacks — Fail Fast

Never use fallback defaults for missing data. If a component is not registered in the plan,
if a vendor string is unknown, if a readExpr is missing — **throw immediately** with a clear
error message telling the developer what they forgot to register. Fallbacks hide bugs.
In UI code, a silent fallback means the wrong component gets read, the wrong vendor gets
resolved, or the wrong field gets validated — all silently. Throw, don't guess.

### 9. ESM Only + Cache Busting

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
# Full test suite — run all from the repo root:
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive

# 1. TS unit tests (vitest + jsdom) — 409 tests
npm test

# 2. C# unit + schema tests — 150 tests
dotnet test tests/Alis.Reactive.UnitTests

# 3. Native component unit tests — 35 tests
dotnet test tests/Alis.Reactive.Native.UnitTests

# 4. Fusion component unit tests — 61 tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests

# 5. FluentValidator unit tests — 43 tests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests

# 6. Playwright browser tests (browser behavior) — 186 tests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

**Total: 884 tests (409 TS + 289 C# unit + 186 Playwright)**

If any test fails, fix the issue and re-run ALL tests before committing.
Never commit with failing tests. Never skip Playwright.
