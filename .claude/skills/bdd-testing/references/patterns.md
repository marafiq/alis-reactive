# BDD Testing Patterns — Detailed Reference

## Table of Contents

1. [Parallel Fixture Design](#parallel-fixture-design)
2. [Full Journey Test Example](#full-journey-test-example)
3. [State Transition Testing](#state-transition-testing)
4. [AJAX Partial Lifecycle](#ajax-partial-lifecycle)
5. [Validator Scoping Rules](#validator-scoping-rules)
6. [TS Unit Test DOM Setup](#ts-unit-test-dom-setup)
7. [Orchestrator Routing Tests](#orchestrator-routing-tests)
8. [Root Cause Mappings](#root-cause-mappings)

---

## Parallel Fixture Design

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

Each fixture has:
- `private const string Path` — unique page URL
- Helper methods scoped to that page's fields
- Full journey tests, not fragments
- No shared state between fixtures

```csharp
[TestFixture]
public class WhenValidatingResidentAdmission : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator ErrorFor(string field) => Page.Locator($"#resident-form span[data-valmsg-for='{field}']");
    private ILocator SummaryDiv => Page.Locator("[data-alis-validation-summary]");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator Result => Page.Locator("#result");
}
```

---

## Full Journey Test Example

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

---

## State Transition Testing

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

---

## Validator Scoping Rules

Validators must match form scope — only rules for fields the page renders:

```csharp
// CORRECT — scoped to page's fields
public class ServerPartialValidator : ReactiveValidator<ResidentModel>
{
    public ServerPartialValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VeteranId).NotEmpty();
        });
        RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
    }
}

// WRONG — whole-model validator on partial page causes phantom errors
.Validate<ResidentValidator>("partial-form")
```

Nested properties must use SetValidator (adapter limitation):

```csharp
// CORRECT
RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());

// WRONG — adapter silently drops these
RuleFor(x => x.Address.Street).NotEmpty();
```

Conditional AJAX rules use WhenField:

```csharp
// CORRECT
WhenField(x => x.AddressType, "Custom Address", () =>
{
    RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
});
```

---

## TS Unit Test DOM Setup

DOM setup must match production exactly:

```typescript
// GOOD — production-accurate
const desc = {
    formId: "resident-form",
    planId: "App.Models.ResidentModel",
    fields: [...]
};

// BAD — no planId, wrong ID format
const desc = { formId: "form", fields: [...] };
```

Required DOM elements:
- Summary div: `data-alis-validation-summary="{planId}"`
- Form: `id` matches descriptor's `formId`
- Error spans: `data-valmsg-for="{fieldName}"`
- Hidden sections: `hidden` attribute
- Component IDs: `{TypeScope}__{PropertyPath}` from IdGenerator

---

## Orchestrator Routing Tests

Test WHERE the error goes, not just pass/fail:

```typescript
validate(desc);
expect(errorSpan("Name")!.textContent).toBe("required");      // inline
expect(summaryDiv().textContent).toBe("");                      // NOT in summary
expect(summaryDiv().hasAttribute("hidden")).toBe(true);         // summary hidden

// Hidden field → summary only:
validate(desc);
expect(errorSpan("HiddenField")!.textContent).toBe("");        // NOT inline
expect(summaryDiv().textContent).toContain("Hidden required");  // IN summary
```

---

## Root Cause Mappings

| Symptom | Wrong Fix | Right Fix |
|---------|-----------|-----------|
| Phantom summary errors | Revert unenriched→summary | Fix validator scope (too wide) |
| Address rules not extracted | Add direct `RuleFor(x.Address.Street)` | Use `SetValidator` (adapter limitation) |
| Condition fires on empty dropdown | Revert fail-closed | Fix eq/neq to return false on empty source |
| Hidden field error silently passes | Remove hidden field validation | Move `valid = false` outside `if (summaryEl)` |

When confused — **ASK the user**. Cost of asking: one message. Cost of guessing: hours of patch-fixing.
