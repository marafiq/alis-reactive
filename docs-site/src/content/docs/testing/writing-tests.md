---
title: Writing Tests
description: Practical patterns for writing tests in the active test layers.
sidebar:
  order: 2
---

This page covers the conventions, base classes, and patterns for the active test layers. Read [Testing Strategy](../strategy/) first for the overall approach.

---

## Current Test Surfaces

The current repo keeps runtime behavior tests in Vitest and browser behavior
tests in Playwright. `scripts/test.sh` also discovers non-Playwright .NET test
projects under `tests/` and runs them when present.

### Generated contract verification

The plan contract is generated from the C# plan domain into `runtime/types/plan.ts`.
Run `npm run typecheck` after plan-domain changes so TypeScript compilation proves
the runtime still matches the generated contract.

### Test naming

BDD style. Name the test around the behavior under proof:

```
WhenPlanBoots.events_page_renders_plan_json
WhenMultipleItemsSelected.gather_posts_selected_values
executeReaction.calls_a_plugin_command_through_the_declared_js_object_contract
```

### Running

```bash
scripts/test.sh --no-e2e
```

---

## TypeScript Unit Tests

### Configuration

Tests live in `Alis.Reactive.Assets/runtime/__tests__/`. Vitest is configured in
`Alis.Reactive.Assets/vitest.config.ts`:

- **Environment:** jsdom
- **Include pattern:** `runtime/__tests__/**/*.test.ts`

Most runtime tests reset boot state and `document.body.innerHTML` in `afterEach`
so Active Plan state does not leak between tests.

### Integration tests (boot pattern)

For end-to-end runtime behavior, construct a plan and call `boot()`:

```typescript
import { afterEach, describe, it, expect } from "vitest";
import { boot } from "../lifecycle/boot";
import { resetBootStateForTests } from "../lifecycle/boot";
import type { PlanDocument } from "../types/index";

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("when triggering on page-ready", () => {
  it("executes a dispatch reaction when document is ready", () => {
    let executed = false;
    document.addEventListener("ready-evt", () => { executed = true; });

    const plan: PlanDocument = {
      version: 3,
      planId: "Test.Model",
      scope: { kind: "root" },
      types: {},
      components: {},
      behaviors: [{
        startsWhen: { kind: "page-ready" },
        reaction: { kind: "dispatch", event: "ready-evt", payload: { kind: "none" } },
      }],
    };

    boot(plan);

    expect(executed).toBe(true);
  });
});
```

This exercises the full path: trigger wiring, reaction execution, and event dispatch.

### DOM setup with JSDOM

When tests need specific HTML elements, create a JSDOM instance in `beforeEach`:

```typescript
let boot: (plan: PlanDocument) => void;

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
import { toJavaScriptString } from "../shared/javascript-string";

it("formats thrown values for diagnostics", () => {
  expect(toJavaScriptString(new Error("boom"))).toContain("boom");
});
```

### Document event tests

For `document-event` triggers, boot the plan (which wires the listener), then
fire the event:

```typescript
it("dispatches a document event payload", () => {
  let handled = false;
  document.addEventListener("loaded:handled", () => { handled = true; });

  const plan: PlanDocument = {
    version: 3,
    planId: "Test.DocumentEvent",
    scope: { kind: "root" },
    types: {},
    components: {},
    behaviors: [{
      startsWhen: {
        kind: "document-event",
        event: "loaded",
        payloadType: { kind: "untyped" },
      },
      reaction: {
        kind: "dispatch",
        event: "loaded:handled",
        payload: { kind: "none" },
      },
    }],
  };

  boot(plan);
  document.dispatchEvent(new CustomEvent("loaded"));

  expect(handled).toBe(true);
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
        await NavigateTo("/Sandbox/CoreBehaviors/Events");
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
scripts/playwright.sh --no-build
```

---

## Running the full suite

Before push or release work:

```bash
scripts/test.sh
```

For focused commits, run the narrow proof that matches the touched surface.
Use `scripts/playwright.sh --filter "..."` for browser behavior and
`scripts/test.sh --no-e2e` when browser behavior is intentionally out of scope.

---

## Checklist for new primitives

When adding a new reaction kind, trigger kind, component, or validation rule:

1. C# intent class with `[JsonDerivedType]`
2. Builder method on the appropriate builder
3. C# plan-domain update
4. Runtime handler in the appropriate execution module
5. TS types in `types/`
6. C# or plan-domain proof when it adds useful signal
7. TS unit test -- runtime behavior in jsdom
8. Playwright test -- browser behavior verification
9. Sandbox view -- usage demonstration in the SandboxApp
