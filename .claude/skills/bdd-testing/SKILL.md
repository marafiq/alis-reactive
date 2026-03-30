---
name: bdd-testing
description: >
  Writes BDD Playwright browser tests and TS unit tests for Alis.Reactive using PagePlan<TModel> typed locators and user-story-driven criteria. Use when asked to "write a test", "add Playwright tests", "test this component", "fix a failing test", "add a test for this view", or "why is this test failing".
  or adding new test scenarios for Alis.Reactive. Also use when the user asks to
  "write a test", "add Playwright tests", "test this component", "fix a failing test",
  "add a test for this view", or "why is this test failing". Derives tests from
  user stories using PagePlan<TModel> typed locators.
---

# BDD Testing — Alis.Reactive

## Principle

> Sensitive to behavior, insensitive to structure. — Kent Beck

Refactoring internals must NEVER break a Playwright test. If it does, the test
is coupled to implementation — rewrite the test.

## Process — Follow in Order

### Step 1: Write the Story BEFORE looking at the view

```
As a [role]
I want [feature]
So that [business value]
```

The story comes from the DOMAIN, not from the code.

### Step 2: List criteria the [role] would confirm

Each criterion = ONE sentence the role would say:

```
✓ "I can search for a physician by name and select them"
✓ "The system tells me which information is missing"
✓ "My complete admission reaches the server with correct data"

✗ "echo span updates"               — no role says this
✗ "componentType validates"          — infrastructure
✗ "ej2 value equals expected"        — implementation
```

Ask: would [role] say this sentence? If not, it's not a criterion.

### Step 3: Each criterion = one test

One When. One Then. Multiple cycles = multiple tests.

Exception: State-cycle tests (fill -> clear -> refill) may stay as one test when they
verify the FULL lifecycle of a single behavior. Split when the steps test DIFFERENT behaviors.

### Step 4: Choose the Right Locator Pattern

**Note:** The codebase is mid-migration. Both `PagePlan<TModel>` and `ComponentScope`/direct
locators coexist. New tests should prefer `PagePlan<TModel>` for compile-time safety.

Three locator patterns exist. Choose the right one for the element:

**1. `PagePlan<TModel>` — expression-based, compile-time safety (preferred for new tests)**

```csharp
_plan = await PagePlan<TModel>.FromPage(Page);
var physician = _plan.AutoComplete(m => m.Physician);
await _plan.ErrorFor(m => m.Physician);  // validation error
```

Reads the plan JSON from the page. Model renames break tests at compile time.

**2. `ComponentScope` — string-based, when model expressions aren't available**

```csharp
var scope = new ComponentScope(Page, "Alis_Reactive_SandboxApp_Models_ResidentModel");
var rate = scope.NumericTextBox("MonthlyRate");
await scope.Element("echo");  // any element by raw ID
```

Uses IdGenerator pattern (`{TypeScope}__{PropertyName}`). Used when the test
project doesn't reference the model type or for multi-scope pages.

**3. Direct `Page.Locator` — for explicit-ID elements not model-bound**

```csharp
private ILocator SubmitBtn => Page.Locator("#submit-btn");
private ILocator Result => Page.Locator("#result");
```

For buttons, echo spans, and other elements with explicit IDs set in the view.

### Step 4b: Component Types and Surfaces

```
COMPONENT (via PagePlan or ComponentScope):
  | .AutoComplete("Prop")     -- Type, SelectItem, TypeAndSelect, Clear, Focus, Blur
  | .DropDownList("Prop")     -- Select("text"), Open, Focus
  | .NumericTextBox("Prop")   -- Fill, FillAndBlur, Clear, Focus, Blur
  | .Switch("Prop")           -- Toggle
  | .TextBox("Prop")          -- Fill, FillAndBlur, Clear, Focus, Blur
  | .DatePicker("Prop")       -- FillAndBlur, SelectDate, Clear, Focus, Blur
  | .TimePicker("Prop")       -- FillAndBlur, Clear, Focus, Blur
  | .DateTimePicker("Prop")   -- FillAndBlur, Clear, Focus, Blur
  NOTE: DatePicker/TimePicker/DateTimePicker — SelectDate (popup gesture) is reliable.
        FillAndBlur (text gesture) may not set ej2.value. Prefer SelectDate when available.
        See DatePickerLocator.cs: "typed input does NOT always update the instance."
  | .DateRangePicker("Prop")  -- FillAndBlur, Clear, Focus, Blur
  | .MultiColumnComboBox("Prop") -- Select, Focus
  | .InputMask("Prop")        -- Fill, FillAndBlur, Clear, Focus, Blur
  | .RichTextEditor("Prop")   -- Fill, Clear, Focus, Blur
  | .MultiSelect("Prop")      -- Open, Select, Clear, Focus, Blur

SURFACE :=
  | _plan.Element("explicit-id")        -- status spans, echo divs, results
  | _plan.ErrorFor(m => m.Prop)         -- validation error for a model property
  | component.Input                     -- the input element (for value assertions)
  | component.PopupItems                -- popup suggestions (AutoComplete)
  | component.PopupItem("Dr. Smith")    -- specific popup suggestion by text (AutoComplete)
  | component.CalendarIcon              -- calendar icon button (DatePicker family, opens popup)
  | component.Popup                     -- calendar popup container (DatePicker family)

ASSERTION :=
  | Expect(SURFACE).ToContainTextAsync(...)
  | Expect(SURFACE).ToBeVisibleAsync()
  | Expect(SURFACE).ToHaveValueAsync(...)
  | Assert.That(request.PostData, Does.Contain(...))   -- framework tests only
```

**Always** `_plan.ComponentType(m => m.Prop)` or `scope.ComponentType("Prop")` — never `Page.Locator("#hardcoded-id")` for model-bound elements.
**Always** `_plan.ErrorFor(m => m.Prop)` — never raw `span[data-valmsg-for]` selectors.
**Always** gestures — never `EvaluateAsync` or `ej2_instances`.

### Step 5: Verify outcomes

```
FRAMEWORK TESTS (testing the gather pipeline):
  Verify POST body — Assert.That(request.PostData, Does.Contain(...))

APP TESTS (testing real application behavior):
  Verify server response on screen — the round-trip proves data reached server

COMPONENT EXERCISE PAGES (no HTTP, testing reactive state changes):
  Verify element text and visibility — Expect(echo).ToContainTextAsync(...)
  Verify conditional show/hide — Expect(panel).ToBeVisibleAsync() / ToBeHiddenAsync()
```

Happy path without verifying what was sent/received (or what changed on screen) is INCOMPLETE.

### Step 6: Validate

```
- [ ] Traces to a criterion from the story
- [ ] Name is ONE sentence the [role] would say
- [ ] Uses PagePlan<TModel> — no hardcoded IDs
- [ ] Uses gestures — no ej2, no EvaluateAsync
- [ ] Framework: verifies POST body. App: verifies screen after round-trip. Exercise: verifies reactive state via element text/visibility
- [ ] Survives refactoring of internals
```

## Stop and Check

**"I'll write one test that fills everything and submits"**
→ Multiple behaviors. Split.

**"I'll test that the echo span updates"**
→ No role cares. Test the behavior the echo serves.

**"I'll use Page.Locator('#some-id')"**
→ Use `_plan.TextBox(m => m.Field)`. Hardcoded IDs break on rename.

**"I'll check ej2_instances[0].value"**
→ Implementation. Assert what the user sees or what the server received.

**"The test name describes the framework action"**
→ Name it as the role would. "incomplete_admission_tells_user_which_fields_are_missing."

**"I'll assert raw POST body in an app test"**
→ POST format is implementation. Assert the screen after round-trip.

**"The popup click doesn't work, let me hack the selector"**
→ STOP. Use the proven gesture for that component. See `references/patterns.md`.

## When a Test Fails — Triage in Order

```
1. Is the test testing the correct thing?
   → Does the criterion match a real user need?

2. Is the test arranged correctly? (Arrange-Act-Assert)
   → Right state? Right action? Right outcome?

3. Is the test using the right tools?
   → PagePlan locators? Correct gestures? No hardcoded selectors?

4. ALL YES → the test is correct. Do NOT hack it.

5. Verify manually in browser.
   → Open the page, do what the test does, see what happens.
   → This determines: locator bug vs app bug.

6. Classify:
   LOCATOR BUG: gesture doesn't work reliably
     → Fix the locator in Playwright.Extensions.
     → Verify with isolated test + browser experiment first.

   APP BUG: behavior genuinely broken
     → Fix the app. Use systematic-debugging skill.
     → The test caught a real bug. Celebrate.

7. NEVER hack the test to make it pass.
```

## Fixture Shape

```csharp
[TestFixture]
public class When{BehaviorHappens} : PlaywrightTestBase
{
    private PagePlan<TModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<TModel>.FromPage(Page);
    }
}
```

## Test Layers

```
"Does the role see the right thing?"    → Playwright + PagePlan<TModel>
"Does this function compute correctly?" → Vitest (boundary values)
"Does the DSL produce correct JSON?"    → NUnit + Verify (snapshots)
```

## References

- **`references/patterns.md`** — Fixture code, validation errors, POST interception
- **`references/first-principles.md`** — Dan North, Kent Beck, Ian Cooper research
