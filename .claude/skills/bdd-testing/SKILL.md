---
name: bdd-testing
description: >
  This skill should be used when writing Playwright browser tests, TS unit tests,
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

### Step 4: Write using PagePlan\<TModel\>

```
PLAN := await PagePlan<TModel>.FromPage(Page)

COMPONENT :=
  | _plan.AutoComplete(m => m.Prop)     -- Type, SelectItem, TypeAndSelect, Clear, Focus, Blur
  | _plan.DropDownList(m => m.Prop)     -- Select("text"), Focus
  | _plan.NumericTextBox(m => m.Prop)   -- Fill, FillAndBlur, Clear, Focus, Blur
  | _plan.Switch(m => m.Prop)           -- Toggle
  | _plan.TextBox(m => m.Prop)          -- Fill, FillAndBlur, Clear, Focus, Blur

SURFACE :=
  | _plan.Element("explicit-id")        -- status spans, echo divs, results
  | _plan.ErrorFor(m => m.Prop)         -- validation error for a model property
  | component.Input                     -- the input element (for value assertions)
  | component.PopupItems                -- popup suggestions (AutoComplete only)

ASSERTION :=
  | Expect(SURFACE).ToContainTextAsync(...)
  | Expect(SURFACE).ToBeVisibleAsync()
  | Expect(SURFACE).ToHaveValueAsync(...)
  | Assert.That(request.PostData, Does.Contain(...))   -- framework tests only
```

**Always** `_plan.ComponentType(m => m.Prop)` — never `Page.Locator("#hardcoded-id")`.
**Always** `_plan.ErrorFor(m => m.Prop)` — never raw `span[data-valmsg-for]` selectors.
**Always** gestures — never `EvaluateAsync` or `ej2_instances`.

### Step 5: Verify outcomes

```
FRAMEWORK TESTS (testing the gather pipeline):
  Verify POST body — Assert.That(request.PostData, Does.Contain(...))

APP TESTS (testing real application behavior):
  Verify server response on screen — the round-trip proves data reached server
```

Happy path without verifying what was sent/received is INCOMPLETE.

### Step 6: Validate

```
- [ ] Traces to a criterion from the story
- [ ] Name is ONE sentence the [role] would say
- [ ] Uses PagePlan<TModel> — no hardcoded IDs
- [ ] Uses gestures — no ej2, no EvaluateAsync
- [ ] Framework: verifies POST body. App: verifies screen after round-trip
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
→ STOP. Use the proven gesture for that component. See `references/locators.md`.

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
public class WhenDoing{Feature} : PlaywrightTestBase
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
