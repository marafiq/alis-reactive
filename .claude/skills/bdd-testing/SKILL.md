---
name: bdd-testing-alis-reactive
description: Write BDD tests for Alis.Reactive framework — full user journeys using framework primitives, parallel Playwright fixtures, senior living domain. Prevents tests passing while browser is broken.
---

# BDD Testing for Alis.Reactive

## When to Use This Skill

Use this skill when:
- Writing Playwright tests for any sandbox page
- Writing TS unit tests for validation, enrichment, merge-plan, or any runtime module
- Adding a new sandbox scenario (view + controller + tests)
- Fixing a bug — write the failing test FIRST, then fix

## The Core Problem This Skill Prevents

Tests that assert function return values but don't test the actual user journey. These tests pass while the browser is visibly broken because they test fragments, not workflows.

## Parallel Playwright Fixture Design

Each scenario gets its own test fixture class with its own URL path. All fixtures run in parallel — Playwright NUnit runs them concurrently on separate browser contexts.

```
tests/Alis.Reactive.PlaywrightTests/
├── ValidationContract/
│   ├── WhenValidatingAllFieldsOnOnePage.cs          → /Sandbox/ValidationContract
│   ├── WhenValidatingWithConditionalVisibility.cs   → /Sandbox/ValidationContract/ConditionalHide
│   ├── WhenValidatingWithServerPartials.cs           → /Sandbox/ValidationContract/ServerPartial
│   └── WhenValidatingWithAjaxPartials.cs             → /Sandbox/ValidationContract/AjaxPartial
├── Components/
│   ├── Native/
│   └── Fusion/
└── Events/
```

**Each fixture:**
- Has `private const string Path` — its unique page URL
- Has helper methods scoped to that page's fields
- Tests are full journeys, not fragments
- Runs independently — no shared state between fixtures

```csharp
[TestFixture]
public class WhenValidatingResidentAdmission : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract";
    // ID prefix from IdGenerator — typeof(ResidentModel).FullName with dots→underscores
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator ErrorFor(string field) => Page.Locator($"#resident-form span[data-valmsg-for='{field}']");
    private ILocator SummaryDiv => Page.Locator("[data-alis-validation-summary]");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator Result => Page.Locator("#result");
}
```

## Senior Living Domain — Use Real Models

All test scenarios use the senior living domain. Not generic "TestModel" or "User". Real domain entities:

| Model | Fields | Validation Pattern |
|-------|--------|-------------------|
| `ResidentModel` | Name, Email, CareLevel, IsVeteran, VeteranId, Address.* | Conditional: VeteranId when IsVeteran, PhysicianName when CareLevel ≠ Independent |
| `ResidentAddress` | Street, City, ZipCode | Nested via SetValidator, conditional on AddressType |
| `FacilityModel` | FacilityName, LicenseNumber | Standalone plan (different TModel) |

Field labels, placeholders, and error messages use senior living language:
- "Resident name", not "Username"
- "Care Level" with options: Independent, Assisted Living, Memory Care
- "Veteran ID" conditional on "Is Veteran" checkbox
- "Emergency Contact" with truthy/falsy flip

## Framework Primitives — Use Them, Don't Bypass

### Views must use Html.Field() + component builders

```csharp
// CORRECT — framework primitive
@{ Html.Field("Care Level", true, m => m.CareLevel, expr =>
    Html.NativeDropDownFor(plan, expr)
        .Items(careLevels)
        .Placeholder("-- Select --")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, a => a.Value).Eq("Memory Care")
                .Then(t => t.Element("memory-section").Show())
                .Else(e => e.Element("memory-section").Hide());
        })
); }

// WRONG — raw HTML, no component registration
<select id="CareLevel" name="CareLevel">...</select>
```

### Validators must match form scope

```csharp
// CORRECT — scoped validator for the page's fields
public class ServerPartialValidator : ReactiveValidator<ResidentModel>
{
    public ServerPartialValidator()
    {
        // Only fields that THIS page renders
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VeteranId).NotEmpty();
        });
        RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
    }
}

// WRONG — whole-model validator on a page with fewer fields
.Validate<ResidentValidator>("partial-form") // ResidentValidator has PhysicianName
                                              // but this page doesn't render it
```

### Nested properties use SetValidator, never direct chains

```csharp
// CORRECT — FluentValidation adapter extracts nested rules
RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());

// WRONG — adapter silently drops these (zero client validation, no error)
RuleFor(x => x.Address.Street).NotEmpty();
RuleFor(x => x.Address.City).NotEmpty();
```

### Conditional AJAX rules use WhenField

```csharp
// CORRECT — address rules only when Custom Address selected
WhenField(x => x.AddressType, "Custom Address", () =>
{
    RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
});

// WRONG — unconditional address rules cause phantom summary before partial loads
RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
```

## Rules — Non-Negotiable

### 1. Every Playwright test is a FULL USER JOURNEY

Not: "click submit, assert one error."
Yes: "submit empty → see all errors → fix one field → resubmit → that error gone, others remain → fix all → resubmit → success."

A test must include:
- **The starting state** (what the page looks like before interaction)
- **The action** (click, fill, select)
- **The visible result** (what errors appear WHERE — inline vs summary)
- **The ABSENCE of wrong results** (no phantom summary, no stale errors)
- **The recovery** (fix fields, resubmit, verify success)

### 2. Assert what the USER sees, not what the function returns

```csharp
// BAD: only checks console has no errors
AssertNoConsoleErrors();

// GOOD: checks exactly what the user sees
await Expect(ErrorFor("Name")).ToContainTextAsync("'Name' is required.");
await Expect(ErrorFor("Name")).ToBeVisibleAsync();
await Expect(Input("Name")).ToHaveClassAsync(new Regex("alis-has-error"));
await Expect(SummaryDiv).ToBeHiddenAsync(); // ← catches phantom summary
await Expect(Result).ToHaveTextAsync(""); // POST was blocked
```

### 3. Test state transitions, not just end states

```csharp
[Test]
public async Task changing_care_level_changes_which_fields_are_required()
{
    await NavigateTo(Path);
    await WaitForTraceMessage("booted", 5000);
    await FillAllRequired();

    // Independent → PhysicianName not required
    await Input("CareLevel").SelectOptionAsync("Independent");
    await SubmitBtn.ClickAsync();
    await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
    await Expect(SummaryDiv).ToBeHiddenAsync();

    // Assisted Living → PhysicianName required
    await Input("CareLevel").SelectOptionAsync("Assisted Living");
    await SubmitBtn.ClickAsync();
    await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");

    // Back to Independent → PhysicianName gone
    await Input("CareLevel").SelectOptionAsync("Independent");
    await SubmitBtn.ClickAsync();
    await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();

    AssertNoConsoleErrors();
}
```

### 4. ALWAYS assert summary state

Every submit assertion must check BOTH inline AND summary:
```csharp
await SubmitBtn.ClickAsync();
await Expect(ErrorFor("Name")).ToContainTextAsync("required");  // inline
await Expect(SummaryDiv).ToBeHiddenAsync();                     // no phantom summary
```

### 5. Test the AJAX partial lifecycle as ONE journey

```csharp
[Test]
public async Task full_ajax_partial_lifecycle()
{
    await NavigateTo(Path);
    await WaitForTraceMessage("booted", 5000);

    // Step 1: Submit with placeholder — parent errors only, no address in summary
    await SubmitBtn.ClickAsync();
    await Expect(ErrorFor("Name")).ToBeVisibleAsync();
    await Expect(SummaryDiv).ToBeHiddenAsync(); // NO phantom address errors

    // Step 2: Select Facility Address — still no address errors
    await Input("AddressType").SelectOptionAsync("Facility Address");
    await SubmitBtn.ClickAsync();
    await Expect(SummaryDiv).ToBeHiddenAsync();

    // Step 3: Select Custom Address → partial loads
    await Input("AddressType").SelectOptionAsync("Custom Address");
    await Expect(Input("Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

    // Step 4: Submit with empty address — inline errors
    await SubmitBtn.ClickAsync();
    await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
    await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();

    // Step 5: Fill all → success
    await FillParentFields();
    await Input("Address_Street").FillAsync("123 Sunrise Blvd");
    await Input("Address_City").FillAsync("Palm Springs");
    await Input("Address_ZipCode").FillAsync("92262");
    await SubmitBtn.ClickAsync();
    await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });

    AssertNoConsoleErrors();
}
```

### 6. TS unit tests for pure modules test BOUNDARIES

For `rule-engine.ts`, `condition.ts`:
```typescript
describe("required", () => {
  it("fails for empty string");
  it("fails for null");
  it("fails for undefined");
  it("fails for false");
  it("passes for 0 (zero is a value)");
});

describe("fail-closed defaults", () => {
  it("unknown rule type blocks (returns true)");
  it("broken regex blocks (returns true)");
  it("missing equalTo peer blocks (returns true)");
});

describe("condition: eq on empty source", () => {
  it("returns false (no intent expressed)");
});
```

### 7. Orchestrator tests — test ROUTING, not just pass/fail

```typescript
// Test WHERE the error goes, not just whether validate returns false
validate(desc);
expect(errorSpan("Name")!.textContent).toBe("required");      // inline
expect(summaryDiv().textContent).toBe("");                      // NOT in summary
expect(summaryDiv().hasAttribute("hidden")).toBe(true);         // summary hidden

// Hidden field:
validate(desc);
expect(errorSpan("HiddenField")!.textContent).toBe("");        // NOT inline
expect(summaryDiv().textContent).toContain("Hidden required");  // IN summary
```

### 8. Test setup must match production

- Summary div: `data-alis-validation-summary="{planId}"` — planId must match descriptor
- ValidationDescriptor: must include `planId`
- Form: `id` must match descriptor's `formId`
- Error spans: `data-valmsg-for="{fieldName}"`
- Hidden sections: `hidden` attribute
- Component IDs: `{TypeScope}__{PropertyPath}` matching IdGenerator output

```typescript
// BAD: no planId, no proper ID format
const desc = { formId: "form", fields: [...] };

// GOOD: production-accurate
const desc = { formId: "resident-form", planId: "App.Models.ResidentModel", fields: [...] };
```

## When a Test Fails — Root Cause Protocol

**NEVER patch-fix. NEVER revert architecture. Find the root cause.**

When a test fails after a code change:

### Step 1: STOP. Do not touch any code.

Read the test failure message. Understand what it expected vs what it got.

### Step 2: Trace the full code path.

Start from the trigger (button click, form submit) and follow every function call to the output (DOM change, error display, return value). Read the actual source — don't guess.

### Step 3: Identify the EXACT line that produces the wrong result.

Not "validation is broken." Rather: "orchestrator.ts line 105 sets `valid = false` inside the `if (summaryEl)` block, so when there's no summary div, `valid` stays `true`."

### Step 4: Ask WHY that line does what it does.

It may be correct for a different scenario. Changing it will break that scenario. Example from this session:
- Unenriched fields were skipped (seemed wrong → "fail-open")
- But skip was correct for AJAX partials (field enriches after merge)
- Changing skip to block broke AJAX partials
- The real bug was validator scope, not the skip behavior

### Step 5: Fix the ROOT CAUSE, not the symptom.

| Symptom | Wrong fix | Right fix |
|---------|-----------|-----------|
| Phantom summary errors | Revert unenriched→summary | Fix validator scope (too wide) |
| Address rules not extracted | Add direct `RuleFor(x.Address.Street)` | Use `SetValidator` (adapter limitation) |
| Condition fires on empty dropdown | Revert fail-closed | Fix eq/neq to return false on empty source |
| Hidden field error silently passes | Remove hidden field validation | Move `valid = false` outside `if (summaryEl)` |

### Step 6: When confused — ASK.

If you can't determine whether the test expectation is wrong or the code is wrong, **ask the user**. Do not guess. The cost of asking is one message. The cost of guessing is hours of patch-fixing.

### What "asking" looks like:

```
The test expects unenriched fields to block with summary,
but the AJAX partial page submits before loading the partial
and gets phantom summary errors.

Two possible fixes:
1. Skip unenriched fields (breaks fail-closed contract)
2. Make address rules conditional on AddressType (keeps fail-closed)

Which direction should I take?
```

### What "asking" does NOT look like:

```
Tests are failing. Let me try changing this... still failing...
let me try reverting... now other tests fail... let me patch...
```

## Checklist Before Committing Tests

- [ ] Every Playwright test is a full journey (error → fix → success)
- [ ] Every submit assertion checks BOTH inline errors AND summary state
- [ ] State transitions tested (toggle checkbox, change dropdown, reload partial)
- [ ] AJAX partial tests cover: before load, after load, facility vs custom, reload
- [ ] No test uses `AssertNoConsoleErrors()` as the ONLY assertion
- [ ] TS unit test DOM setup matches production (planId, summary div, error spans)
- [ ] Pure module tests cover every boundary value and fail-closed defaults
- [ ] Fixtures are parallel — each has its own URL, no shared state
- [ ] Senior living domain — ResidentModel, CareLevel, VeteranId, not generic test models
- [ ] Framework primitives used — Html.Field, NativeDropDownFor, SetValidator, WhenField
