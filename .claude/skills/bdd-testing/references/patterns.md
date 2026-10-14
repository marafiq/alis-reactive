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
├── Components/
│   ├── AppLevel/
│   ├── Brought/
│   ├── Fusion/          (one folder per component: DatePicker/, Grid/, ...)
│   └── Native/
├── Conditions/          (one folder per journey: MedicationRound/, PharmacyReview/, ...)
├── CoreBehaviors/
├── HttpPipeline/        (ResidentRegister/, ShiftEmailLink/, RealTime/, ...)
├── Patterns/            (BedAssignment/, MoveInPacket/, Workflows/, ...)
├── Plugins/
└── Validation/          (Contract/, Rules/, MoveInReview/, ...)
```

Each fixture has:
- `private const string Path` — unique page URL
- Helper methods scoped to that page's fields
- Full journey tests, not fragments
- No shared state between fixtures

This fixture uses hardcoded IDs and string-based locators (legacy). New tests should prefer
`PagePlan<TModel>` for compile-time safety; the codebase is mid-migration and both patterns coexist.
The opening of `Validation/Contract/WhenAjaxPartialsLoadWithValidation.cs` (lines 5-18):

```csharp
[TestFixture]
public class WhenAjaxPartialsLoadWithValidation : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/AjaxPartial";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{ModelIdPrefix}{suffix}");
```

---

## Full Journey Test Example

From `Validation/Contract/WhenAjaxPartialsLoadWithValidation.cs:53-92`:

```csharp
[Test]
public async Task full_ajax_partial_lifecycle()
{
    await NavigateTo(Path);
    await WaitForTraceMessage("booted", 5000);

    await ClickWhenStable(SubmitBtn);
    await Expect(ErrorFor("Name")).ToContainTextAsync("required");
    await Expect(ErrorFor("Name")).ToBeVisibleAsync();
    await Expect(ErrorFor("Email")).ToContainTextAsync("required");
    await Expect(SummaryDiv).ToBeHiddenAsync();

    await Input("AddressType").SelectOptionAsync("Facility Address");
    await ClickWhenStable(SubmitBtn);
    await Expect(ErrorFor("Name")).ToContainTextAsync("required");
    await Expect(SummaryDiv).ToBeHiddenAsync();

    await FillParentFields();
    await SelectCustomAddress();

    await ClickWhenStable(SubmitBtn);
    await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
    await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
    await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
    await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");
    await Expect(SummaryDiv).ToBeHiddenAsync();

    await FillParentFields();
    await Input("Address_Street").FillAsync("123 Sunrise Blvd");
    await Input("Address_City").FillAsync("Palm Springs");
    await Input("Address_ZipCode").FillAsync("92262");
    await ClickWhenStable(SubmitBtn);

    await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
    await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
    await Expect(ErrorFor("Address.Street")).Not.ToBeVisibleAsync();
    await Expect(SummaryDiv).ToBeHiddenAsync();

    AssertNoConsoleErrors();
}
```

---

## State Transition Testing

Choosing a care level changes which fields are required. From
`Validation/Contract/WhenHiddenFieldsSkipValidation.cs:106-123` (sibling tests cover Independent and
Memory Care):

```csharp
[Test]
public async Task assisted_living_shows_physician_section_and_requires_physician()
{
    await NavigateTo(Path);
    await WaitForTraceMessage("booted", 5000);

    await FillAllRequired();
    await Input("CareLevel").SelectOptionAsync("Assisted Living");

    await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

    await ClickWhenStable(SubmitBtn);
    await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
    await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
    await Expect(SummaryDiv).ToBeHiddenAsync();

    AssertNoConsoleErrors();
}
```

---

## Validator Scoping Rules

Validators must match form scope — only rules for fields the page renders.
From `Models/Validation/Contract/ResidentValidator.cs:50-86`, the validator of the page whose
form renders no physician or memory fields:

```csharp
// CORRECT — scoped to page's fields

// ServerPartial omits rules for fields that page does not render.
public class ServerPartialValidator : ReactiveValidator<ResidentModel>
{
    public ServerPartialValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("'Name' is required.")
            .MinimumLength(2).WithMessage("'Name' must have a minimum length of 2.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("'Email' is required.")
            .EmailAddress().WithMessage("'Email' must be a valid email address.");
        RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

        RuleFor(x => x.ConfirmEmail)
            .NotEmpty().WithMessage("'Confirm Email' is required.")
            .Equal(x => x.Email).WithMessage("'Confirm Email' must match 'Email'.");

        When(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VeteranId).NotEmpty().WithMessage("'Veteran ID' is required when veteran.");
        });

        When(x => x.HasEmergencyContact, () =>
        {
            RuleFor(x => x.EmergencyName).NotEmpty().WithMessage("'Emergency Name' is required.");
            RuleFor(x => x.EmergencyPhone).NotEmpty().WithMessage("'Emergency Phone' is required.");
        });

        When(x => !x.HasEmergencyContact, () =>
        {
            RuleFor(x => x.ReasonForNoContact).NotEmpty().WithMessage("'Reason' is required when no emergency contact.");
        });

        RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
    }
}

// WRONG — whole-model validator on partial page causes phantom errors
.Validate<ResidentValidator>("partial-form")
```

Nested properties take the child's validator, whose rules reach the browser with the prefix:

```csharp
RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator());
```

Conditional rules are a `When` lambda over the model; the browser applies the same condition:

```csharp
When(x => x.AddressType == "Custom Address", () =>
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
- Summary div: `data-reactive-validation-summary="{planId}"`
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
| Address rules missing client-side | Rules under a method-group `When` | Write the condition as a lambda: `When(x => ..., () => ...)` |
| Condition fires on empty dropdown | Revert fail-closed | Fix eq/neq to return false on empty source |
| Hidden field error silently passes | Remove hidden field validation | Move `valid = false` outside `if (summaryEl)` |

When confused — **ASK the user**. Cost of asking: one message. Cost of guessing: hours of patch-fixing.
