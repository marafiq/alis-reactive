---
title: Writing Tests
description: Practical patterns for writing tests at all three layers.
sidebar:
  order: 2
---

This page covers the conventions, base classes, and patterns for each test layer. Read [Testing Strategy](../strategy/) first for the overall approach.

---

## C# Unit Tests

### PlanTestBase

All core unit tests extend `PlanTestBase`, which provides:

| Method | Purpose |
|--------|---------|
| `CreatePlan()` | Creates a `ReactivePlan<TestModel>` |
| `Trigger(plan)` | Returns a `TriggerBuilder` to wire triggers on the plan |
| `AssertSchemaValid(json)` | Validates rendered JSON against `reactive-plan.schema.json` |

A typical test class defines a private `Build` helper that wires a dom-ready trigger:

```csharp
[TestFixture]
public class WhenDispatchingAnEvent : PlanTestBase
{
    [Test]
    public Task Event_name_flows_to_plan() =>
        VerifyJson(Build(p => p.Dispatch("user-saved")).Render());

    [Test]
    public Task Payload_flows_when_provided() =>
        VerifyJson(Build(p => p.Dispatch("saved", new TestModel { Id = "abc" })).Render());

    private static IReactivePlan<TestModel> Build(
        Action<Builders.PipelineBuilder<TestModel>> configure)
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(configure);
        return plan;
    }
}
```

### Snapshot verification

`VerifyJson()` captures exact JSON output in a `.verified.txt` file. On subsequent runs, any diff causes a test failure.

**Critical rule:** Call `VerifyJson()` directly in the test method, never through a base class helper. Verify uses `[CallerFilePath]` to locate the snapshot file. If the call originates from a base class, the snapshot lands in the wrong folder.

**Co-location:** `.verified.txt` files live next to their test class. Move both together.

### Schema validation

`AssertSchemaValid()` loads `reactive-plan.schema.json`, evaluates the JSON, and fails with structured errors if any constraint is violated:

```csharp
[Test]
public void rendered_plan_validates_against_schema()
{
    var json = Build(p =>
    {
        p.Element("status").AddClass("active");
        p.Dispatch("ready");
    }).Render();

    AssertSchemaValid(json);
}
```

### Test naming

BDD style. The class is `When{Scenario}`, the method is the expected behavior:

```
WhenMutatingAnElement.AddClass_produces_mutate_element_command
WhenMutatingAnElement.Show_and_hide_produce_correct_actions
WhenDispatchingAnEvent.Multiple_commands_in_sequence
```

### Running

```bash
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
```

---

## TypeScript Unit Tests

### Configuration

Tests live in `Scripts/__tests__/`. Vitest is configured in `vitest.config.ts`:

- **Environment:** jsdom
- **Include pattern:** `Scripts/__tests__/**/*.test.ts`
- **Setup file:** `Scripts/__tests__/vitest.setup.ts` -- runs `resetBootStateForTests()`, restores mocks, and clears `document.body.innerHTML` after every test.

Because the setup file handles cleanup, individual test files do not need `afterEach` blocks unless they have extra teardown.

### Integration tests (boot pattern)

For end-to-end runtime behavior, construct a plan and call `boot()`:

```typescript
import { describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when triggering on dom-ready", () => {
  it("executes commands immediately when document is ready", () => {
    let executed = false;
    document.addEventListener("ready-evt", () => { executed = true; });

    boot({
      planId: "Test.Model",
      components: {},
      entries: [{
        trigger: { kind: "dom-ready" },
        reaction: {
          kind: "sequential",
          commands: [{ kind: "dispatch", event: "ready-evt" }],
        },
      }],
    });

    expect(executed).toBe(true);
  });
});
```

This exercises the full path: trigger wiring, reaction execution, command dispatch.

### DOM setup with JSDOM

When tests need specific HTML elements, create a JSDOM instance in `beforeEach`:

```typescript
let boot: (plan: Plan) => void;

beforeEach(async () => {
  const dom = new JSDOM(`<!DOCTYPE html><html><body>
    <p id="status" class="text-muted">waiting</p>
    <div id="panel">initial</div>
  </body></html>`);

  (globalThis as any).document = dom.window.document;
  (globalThis as any).CustomEvent = dom.window.CustomEvent;

  const mod = await import("../lifecycle/boot");
  boot = mod.boot;
});
```

Re-importing `boot` after setting up the DOM ensures the module binds to the new `document`.

### Unit tests (direct imports)

When testing a specific module in isolation, import the function directly:

```typescript
import { resolveEventPath, coerce } from "../resolution/resolver";
import type { ExecContext } from "../types";

it("resolves a nested property", () => {
  const ctx: ExecContext = { evt: { address: { city: "Seattle" } } };
  expect(resolveEventPath("evt.address.city", ctx)).toBe("Seattle");
});

it("coerces 'false' string to boolean false", () => {
  expect(coerce("false", "boolean")).toBe(false);
});
```

### Custom event tests

For custom-event triggers, boot the plan (which wires the listener), then fire the event:

```typescript
it("resolves payload source to element text", () => {
  document.body.innerHTML = '<span id="city">unknown</span>';

  boot({
    planId: "Test", components: {},
    entries: [{
      trigger: { kind: "custom-event", event: "loaded" },
      reaction: { kind: "sequential", commands: [{
        kind: "mutate-element", target: "city",
        mutation: { kind: "set-prop", prop: "textContent" },
        source: { kind: "event", path: "evt.address.city" }
      }]}
    }]
  });

  document.dispatchEvent(
    new CustomEvent("loaded", { detail: { address: { city: "Seattle" } } })
  );

  expect(document.getElementById("city")!.textContent).toBe("Seattle");
});
```

### Running

```bash
npm test          # vitest run (all tests)
npm run test:watch # vitest in watch mode
```

---

## Playwright Tests

### Infrastructure

Two classes power the Playwright layer:

**WebServerFixture** (assembly-level `[SetUpFixture]`) starts the SandboxApp on port 5220 before any tests run and kills it when the suite finishes. Tests do not need to manage the server.

**PlaywrightTestBase** extends Playwright's `PageTest` and provides:

| Method | Purpose |
|--------|---------|
| `NavigateTo(path)` | Navigates to `BaseUrl + path` |
| `WaitForTraceMessage(msg, timeoutMs)` | Polls captured console messages for a string match |
| `AssertTraceContains(scope, text)` | Asserts a trace message with `[alis:scope]` contains the text |
| `AssertNoConsoleErrors()` | Fails if any `console.error` was captured |

Console messages are captured automatically. On test failure, the full console log is dumped to test output.

### Writing a Playwright test

```csharp
[TestFixture]
public class WhenEventChainFires : PlaywrightTestBase
{
    [Test]
    public async Task three_hop_chain_completes_in_order()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#step-1")).ToContainTextAsync("dom-ready fired");
        await Expect(Page.Locator("#step-2")).ToContainTextAsync("\"test\" received");
        await Expect(Page.Locator("#step-3")).ToContainTextAsync("\"test-received\" received");

        AssertNoConsoleErrors();
    }
}
```

The pattern: navigate, wait for boot, assert DOM state, check for errors.

### PagePlan and typed locators

`PagePlan<TModel>` reads the plan JSON from the page and provides expression-based component locators. No hardcoded element IDs in tests.

```csharp
// Initialize from the page after boot
var plan = await PagePlan<AutoCompleteModel>.FromPage(Page);

// Expression-based lookup -- same expression as the view
var physician = plan.AutoComplete(m => m.Physician);
await physician.Type("smith");
await physician.SelectItem("Dr. Smith");

// Element lookup for non-component elements
await Expect(plan.Element("change-value"))
    .ToContainTextAsync("smith", new() { Timeout = 5000 });

// Validation error lookup by model expression
await Expect(plan.ErrorFor(m => m.Physician))
    .ToContainTextAsync("required");
```

`PagePlan<TModel>` provides typed locators for each component type:

| Method | Returns |
|--------|---------|
| `AutoComplete(m => m.Prop)` | `AutoCompleteLocator` |
| `DropDownList(m => m.Prop)` | `DropDownListLocator` |
| `NumericTextBox(m => m.Prop)` | `NumericTextBoxLocator` |
| `Switch(m => m.Prop)` | `SwitchLocator` |
| `TextBox(m => m.Prop)` | `NativeTextBoxLocator` |
| `Element(id)` | `ILocator` |
| `ErrorFor(m => m.Prop)` | `ILocator` |
| `FindComponent(m => m.Prop)` | `ComponentEntry?` |

If a model property is renamed, both the view and the test break at compile time.

### Rebuild before running

Playwright tests run against the live application. If you changed TypeScript or CSS and did not rebuild, Playwright tests the stale code.

```bash
npm run build:all          # Rebuild JS + CSS
dotnet build               # Rebuild C# (picks up new bundle hash)
dotnet test tests/Alis.Reactive.PlaywrightTests
```

---

## Running the full suite

Before every commit:

```bash
npm test                                                    # TS unit tests
dotnet test tests/Alis.Reactive.UnitTests                   # Core C# tests
dotnet test tests/Alis.Reactive.Native.UnitTests            # Native tests
dotnet test tests/Alis.Reactive.Fusion.UnitTests            # Fusion tests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests   # Validation tests
dotnet test tests/Alis.Reactive.PlaywrightTests             # Browser tests
```

All must pass. No exceptions.

---

## Checklist for new primitives

When adding a new command kind, trigger kind, component, or validation rule:

1. C# intent class with `[JsonDerivedType]`
2. Builder method on the appropriate builder
3. JSON schema update in `reactive-plan.schema.json`
4. Runtime handler in the appropriate execution module
5. TS types in `types/`
6. C# unit test -- snapshot + schema validation
7. TS unit test -- runtime behavior in jsdom
8. Playwright test -- browser behavior verification
9. Sandbox view -- usage demonstration in the SandboxApp
