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
<script type="application/json" data-alis-plan data-trace="trace">@Html.Raw(plan.Render())</script>
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
| `Alis.Reactive/InputField/InputFieldSetup.cs` | Generic `InputFieldSetup<THelper, TModel, TProp>` — framework-agnostic base |
| `Alis.Reactive/InputField/InputFieldOptions.cs` | `Required()`, `Label()` — field wrapper options |
| `Alis.Reactive/InputField/InputFieldBuilder.cs` | Internal, BCL-only — renders label + validation slot wrapper HTML |
| `Alis.Reactive.Native/Extensions/InputFieldExtensions.cs` | Closes `THelper` to `IHtmlHelper<TModel>`, provides `Html.InputField()` factory + `Render(IHtmlContent)` |
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

**Two mutation kinds:** `set-prop` and `call`.

| C# DSL method | Mutation kind | Plan JSON | Runtime execution |
|----------------|--------------|-----------|-------------------|
| `SetText("x")` | `set-prop` | `{ kind: "set-prop", prop: "textContent" }, value: "x"` | `root.textContent = "x"` |
| `SetHtml("x")` | `set-prop` | `{ kind: "set-prop", prop: "innerHTML" }, value: "x"` | `root.innerHTML = "x"` |
| `AddClass("x")` | `call` | `{ kind: "call", method: "add", chain: "classList", args: [{ kind: "literal", value: "x" }] }` | `root.classList.add("x")` |
| `RemoveClass("x")` | `call` | `{ kind: "call", method: "remove", chain: "classList", args: [{ kind: "literal", value: "x" }] }` | `root.classList.remove("x")` |
| `ToggleClass("x")` | `call` | `{ kind: "call", method: "toggle", chain: "classList", args: [{ kind: "literal", value: "x" }] }` | `root.classList.toggle("x")` |
| `Show()` | `call` | `{ kind: "call", method: "removeAttribute", args: [{ kind: "literal", value: "hidden" }] }` | `root.removeAttribute("hidden")` |
| `Hide()` | `call` | `{ kind: "call", method: "setAttribute", args: [{ kind: "literal", value: "hidden" }, { kind: "literal", value: "" }] }` | `root.setAttribute("hidden", "")` |

**Component mutations use the same two kinds:**

| Pattern | Example | Mutation kind | Plan JSON |
|---------|---------|--------------|-----------|
| Prop set | `SetValue("x")` | `set-prop` | `{ kind: "set-prop", prop: "value" }, value: "x", vendor: "fusion"` |
| Prop set + coerce | `SetValue(42m)` | `set-prop` | `{ kind: "set-prop", prop: "value", coerce: "number" }, value: "42", vendor: "fusion"` |
| Void method | `Focus()` | `call` | `{ kind: "call", method: "focus" }, vendor: "fusion"` |
| Method + source | `SetItems(source)` | `call` | `{ kind: "call", method: "setItems", args: [{ kind: "source", source: {...} }] }, vendor: "fusion"` |
| Multi-arg | `AddItem(item, idx)` | `call` | `{ kind: "call", method: "addItem", args: [{ kind: "source", ... }, { kind: "source", ..., coerce: "number" }] }` |
| Mixed args | `SetProperty(name, val)` | `call` | `{ kind: "call", method: "setProperty", args: [{ kind: "literal", value: "name" }, { kind: "source", ... }] }` |

**MethodArg — per-arg resolution:** Each arg in `CallMutation.args` is a discriminated `MethodArg`:
- `LiteralArg` (`kind: "literal"`) — pass-through value
- `SourceArg` (`kind: "source"`) — resolved via `resolveSource()` + optional `coerce`

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
| `Scripts/auto-boot.ts` | esbuild entry point — auto-discovers `[data-alis-plan]`, reads `data-trace`, calls `boot()` |
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
<script type="application/json" data-alis-plan data-trace="trace">@Html.Raw(planJson)</script>
```

**Auto-boot architecture:**

| File | Role |
|------|------|
| `Scripts/auto-boot.ts` | esbuild entry point — auto-discovers `[data-alis-plan]`, reads `data-trace`, calls `boot()` |
| `Scripts/boot.ts` | Testable export — `boot()` and `trace` used by vitest, NOT the browser entry point |

`auto-boot.ts` runs on every page load. If `[data-alis-plan]` exists, it boots. If `data-trace`
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
| `when-using-unified-call-mutations.test.ts` | Unified call mutation: void, literal, source, multi-arg, mixed, component source, per-arg coerce |

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

### 1. Always Use Git Worktrees for Feature Work

Every feature branch MUST be created in a git worktree under `.worktrees/`. Never work
directly on the `main` branch. This ensures parallel sessions don't collide.

```bash
git worktree add .worktrees/<feature-name> -b feature/<feature-name>
cd .worktrees/<feature-name>
```

The `.worktrees/` directory is gitignored. Each session gets its own isolated copy of the repo
with its own branch. Run all builds and tests from within the worktree directory.

### 2. Plan Is the Only Contract

The C# DSL builds descriptors → `plan.Render()` serializes to JSON → JS runtime executes.
No shortcuts. No manual JS in views. No `document.addEventListener` in `.cshtml`. No `window.alis`.
No inline `<script>` blocks in views — `auto-boot.ts` handles discovery and boot automatically.

### 3. Every New Primitive Needs All Three Layers

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

### 4. Two-Phase Boot Is Inviolable

Custom-event listeners wire before dom-ready reactions execute. This ensures event chains
(`dom-ready → dispatch → custom-event → dispatch → ...`) work regardless of entry order in
the plan. Tests verify this at all three layers.

### 5. Trace Output Is Deterministic

`trace.ts` serializes to a single `JSON.stringify` string per message: `[alis:scope] msg {"key":"val"}`.
This makes trace output testable in Playwright via string matching. No separate `console.log`
arguments (browser-dependent formatting).

### 6. Verified Files Are Co-Located

`.verified.txt` snapshot files live in the same folder as their test class. Tests must call
`VerifyJson()` directly (not through a base method) so `[CallerFilePath]` resolves correctly.

### 7. Vertical Slices

Each runtime module (`element.ts`, `trigger.ts`, `execute.ts`) is a self-contained unit.
Each C# builder (`ElementBuilder`, `PipelineBuilder`, `TriggerBuilder`) is a self-contained unit.
No shared base classes for behavior. Duplication between slices is intentional.

### 8. Component Architecture (3 Foundational Modules — Never Violate)

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
each vertical slice's `Value()` extension using `IInputComponent.ReadExpr`, flows
through `When()` guard conditions with full type safety, and serializes to
`ComponentSource` at render time.

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

### 9. Validation Extraction — Design Rationale

The `FluentValidationAdapter` extracts a **subset** of FluentValidation rules for client-side
validation. This is intentional — not all server rules CAN be extracted.

**Why only simple rules:** FluentValidation supports arbitrary C# predicates (`.Must(x => ...)`),
database lookups, async validators, and dependency-injected services. These are opaque lambdas —
they cannot be serialized to JSON and sent to the browser. The adapter extracts ONLY rules that
map to a deterministic, stateless predicate the JS runtime can evaluate with zero server calls.

**Extracted rule types (18 client rules from FV interfaces):**

| FV Validator Interface | Client Rule | Example |
|------------------------|-------------|---------|
| `INotEmptyValidator` / `INotNullValidator` | `required` | `.NotEmpty()` |
| `ILengthValidator` | `minLength`, `maxLength` | `.MinimumLength(3).MaximumLength(100)` |
| `IEmailValidator` | `email` | `.EmailAddress()` |
| `IRegularExpressionValidator` | `regex` | `.Matches(@"^\d{5}$")` |
| `ICreditCardValidator` | `creditCard` | `.CreditCard()` |
| `IBetweenValidator` | `range` | `.InclusiveBetween(0, 120)` |
| `IExclusiveBetweenValidator` | `exclusiveRange` | `.IsExclusiveBetween(0, 100)` |
| `IComparisonValidator` | `min`, `max`, `gt`, `lt`, `equalTo`, `notEqualTo`, `notEqual` | `.GreaterThan(0)` |
| `IEmptyValidator` (custom) | `empty` | `.IsEmpty()` |

**Not extracted (server-only by design — cannot serialize to JSON):**

| Pattern | Why |
|---------|-----|
| `.Must(x => ...)` | Arbitrary C# lambda — cannot run in browser |
| `.MustAsync(...)` | Async + arbitrary logic |
| `.When(x => ...)` on rule | Arbitrary C# predicate for conditions |
| `.Unless(x => ...)` | Same as `.When()` — arbitrary predicate |
| Custom `IValidator` implementations | Unknown logic, may use DI, DB, etc. |
| `.IsInEnum()` | Requires enum values at extraction time (no reflection) |

**Conditional rules — two systems by design:**

| System | Where | How | Extractable? |
|--------|-------|-----|-------------|
| FV `.When(x => predicate)` | On rule or component | Arbitrary C# lambda | **No** — server-only |
| `ReactiveValidator.WhenField()` | On rule group | Constrained to field comparison (truthy/falsy/eq/neq) | **Yes** — serializes to `ValidationCondition` |

The framework provides `WhenField()` specifically because FV's `.When()` cannot be serialized.
`WhenField()` constrains conditions to simple, serializable field comparisons that the JS
runtime can evaluate. This is the ONLY way to get conditional validation on the client.

**Known limitations:**
- `IsInEnum()` silently dropped (no reflection = can't extract enum values)
- `TimeOnly` / `TimeSpan` comparisons extract with `null` coerceAs (no coercion type defined)
- Unsupported validator types produce zero client rules with no warning

**CoerceAs inference (for comparison rules):**

| C# Type | CoerceAs | Runtime comparison |
|---------|----------|-------------------|
| `int`, `long`, `decimal`, `double`, `float`, `byte`, `short` | `"number"` | Numeric |
| `DateTime`, `DateTimeOffset`, `DateOnly` | `"date"` | Timestamp (milliseconds) |
| `TimeOnly`, `TimeSpan` | `null` (not supported) | String (incorrect) |
| Everything else | `null` | String |

### 10. Fixing Bugs — Root Cause, Not Patch

When a behavior is wrong:
1. **STOP.** Do not apply the first fix that comes to mind.
2. **Trace the full code path** from trigger to outcome. Read every module involved.
3. **Identify the exact line** that produces the wrong result.
4. **Understand WHY** the current code does what it does — it may be correct for a
   different scenario. Changing it will break that scenario.
5. **Fix the root cause.** If the wrong validator is used, fix the validator scope —
   don't change the validation engine. If a condition fires incorrectly, fix the
   condition semantics — don't disable the validation pipeline.
6. **Verify in the actual browser** — fill the form, click submit, see the result
   with your own eyes. Playwright passing is necessary but NOT sufficient.
7. **Run ALL tests** — not just the module you touched.

**Never revert agreed-upon architecture to fix a symptom.** If the architecture says
"unenriched fields block with summary" and you see phantom errors, the problem is
upstream (wrong validator scope, wrong condition semantics) — not the blocking behavior.

### 10. No Fallbacks — Fail Fast

Never use fallback defaults for missing data. If a component is not registered in the plan,
if a vendor string is unknown, if a readExpr is missing — **throw immediately** with a clear
error message telling the developer what they forgot to register. Fallbacks hide bugs.
In UI code, a silent fallback means the wrong component gets read, the wrong vendor gets
resolved, or the wrong field gets validated — all silently. Throw, don't guess.

### 11. ESM Only + Cache Busting

The runtime is bundled as ESM (`--format=esm`). The layout loads it via
`<script type="module" src="~/js/alis-reactive.js" asp-append-version="true">`.
No IIFE, no `window.alis`, no import maps, no inline `<script type="module">` blocks.

**Cache busting:** `asp-append-version="true"` on both CSS and JS `<link>`/`<script>` tags
appends `?v=SHA256hash` — browser always gets the latest build. This works because the
layout uses `src=` (not inline `import from`), and ASP.NET's tag helpers compute the hash.

### 12. Playwright Test Workflow — Report First, Fix Methodically

**Always run Playwright with trx report so failures are captured in one pass:**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests \
  --logger "html;LogFileName=playwright-report.html" \
  --logger "trx;LogFileName=playwright-results.trx" \
  --results-directory TestResults
```

The HTML report at `TestResults/playwright-report.html` shows pass/fail with error
messages and stack traces. The trx file is machine-readable for CI pipelines.

**When tests fail, immediately open the HTML report in the user's browser:**
```bash
open TestResults/playwright-report.html
```
Do NOT re-run the full suite to find which tests failed — the report already has everything.

**After a run with failures, extract the failed test list from the report — never re-run
the full suite just to find which tests failed.**

**Fix one test at a time using this workflow:**

1. **Read the failed test** — understand what it asserts
2. **Check prerequisites** — was Sandbox rebuilt? (`npm run build:all && dotnet build`)
3. **Re-run only the failed test** — `dotnet test --filter "test_name"`
4. **If it still fails, open browser** — navigate to the page manually, verify behavior
5. **Compare test selectors vs actual DOM** — use browser DevTools to find correct selectors
6. **Fix the test** — update selectors or assertions to match current DOM
7. **Re-run only that test** — confirm it passes
8. **Move to next failed test** — repeat from step 1

**Never shotgun-fix multiple tests at once.** Each test gets its own diagnosis cycle.
After all individual tests pass, run the full suite once to confirm no regressions.

**Re-running only failed tests (with screenshots and sequential execution):**

```bash
# Extract failed test names from trx report and re-run sequentially
FAILED=$(grep 'outcome="Failed"' TestResults/playwright-results.trx \
  | sed 's/.*testName="//' | sed 's/".*//' | sort -u \
  | grep -v "ResultSummary" | paste -sd '|' -)

dotnet test tests/Alis.Reactive.PlaywrightTests \
  --filter "$FAILED" \
  --logger "html;LogFileName=rerun-report.html" \
  --logger "trx;LogFileName=rerun-results.trx" \
  --results-directory TestResults \
  -- NUnit.NumberOfTestWorkers=1
```

Screenshots are captured automatically on failure (`TestResults/screenshots/{testName}.png`)
via `PlaywrightTestBase.DumpLogsOnFailure()`.

**When re-run completes with failures, open the report:**
```bash
open TestResults/rerun-report.html
```

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

## Skills — DSL Reference & Onboarding

Framework behavior is documented in skills (grammar-shaped DSL references). **Skills are the
source of truth for how to USE the framework.** CLAUDE.md is the source of truth for architecture.

| Skill | When to use |
|-------|------------|
| `reactive-dsl` | Writing plan code: Html.On, triggers, Element/Dispatch/Component, .Reactive() |
| `http-pipeline` | HTTP requests: Get/Post, Gather, Response, Chained, Parallel |
| `conditions-dsl` | Conditions: When/Then/ElseIf/Else, operators, guard composition |
| `onboard-fusion-component` | Adding SF components, events, methods, or props |
| `validation-rules-alis-reactive` | Validation rules on TModel |

**Keeping skills accurate:**
- When adding a new builder method, operator, or pattern → update the relevant skill grammar
- When onboarding a new component reveals a missing pattern → update the onboard skill
- Skills must match current source — never document APIs that don't exist
- Keep skills general (grammar-shaped) to avoid duplication across components
- When in doubt, read the source before the skill — skills can drift

## Cross-Layer Changes (C# + TS + CSS + Tests)

**When a change touches both C# and TS (e.g., renaming a data attribute, changing plan shape):**

1. Make the change in ALL files (C#, TS, CSS, tests)
2. `npm run build:all` — rebuild JS + CSS bundles
3. `dotnet build` — rebuild C# (triggers esbuild via csproj target)
4. `npm test` — vitest must pass
5. **Restart the app** — `dotnet run --project Alis.Reactive.SandboxApp`
6. `dotnet test tests/Alis.Reactive.PlaywrightTests` — browser tests against running app

**Playwright tests run against the LIVE app.** If the app is serving stale bundles
(old JS/CSS), Playwright will fail even if the code is correct. Always rebuild AND
restart before running Playwright.

## Pre-Commit Verification (MANDATORY)

**Before every commit, ALL tests must pass. No exceptions.**

```bash
# Full test suite — run all from the repo root:
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive

# 1. TS unit tests (vitest + jsdom) — 894 tests
npm test

# 2. C# unit + schema tests — 155 tests
dotnet test tests/Alis.Reactive.UnitTests

# 3. Native component unit tests — 46 tests
dotnet test tests/Alis.Reactive.Native.UnitTests

# 4. Fusion component unit tests — 58 tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests

# 5. FluentValidator unit tests — 50 tests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests

# 6. Playwright browser tests (browser behavior) — 481 tests
dotnet test tests/Alis.Reactive.PlaywrightTests

# 7. SonarQube quality gate (requires Docker running with SonarQube)
./scripts/sonar-analyze.sh
# Exit code 1 = quality gate failed — fix reported issues before committing.
# Skip if Docker/SonarQube is not running, but run at least once per feature branch.
```

**Total: ~1,700 tests (894 TS + 326 C# unit + 481 Playwright)**

If any test fails, fix the issue and re-run ALL tests before committing.
Never commit with failing tests. Never skip Playwright.

**SonarQube:** If the quality gate fails (exit code 1), review violations at
`http://localhost:9000/dashboard?id=alis-reactive` and fix before committing.
SonarQube analyzes both C# and TypeScript for bugs, vulnerabilities, and code smells.
