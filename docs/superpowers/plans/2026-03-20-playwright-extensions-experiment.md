# Playwright Extensions Experiment — ComponentType + BDD Test

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `componentType` to plan JSON, build a real sandbox page with diverse components + validation, write BDD tests using plan-driven locators, and evaluate: does this help or hurt?

**Architecture:** Add one field (`componentType`) to `ComponentRegistration`. Each component's HtmlExtensions passes its type name during registration. Plan JSON carries it. `PagePlan<TModel>` reads it and validates locator calls. New sandbox page exercises: AutoComplete, DropDownList, NumericTextBox, Switch, NativeTextBox with FluentValidation.

**Tech Stack:** C# (.NET 10), Playwright.NUnit, FluentValidation, Syncfusion EJ2

**Evaluation criteria (decide at the end):**
1. Does the typed locator catch a simulated component-type change?
2. Does the typed locator catch a simulated property rename?
3. Is the BDD test shorter and more readable than the raw Playwright equivalent?
4. Does validation (submit → errors → fix → success) work as a full journey?
5. Would a new developer understand the test without reading framework source?

---

## File Map

| File | Action | Purpose |
|------|--------|---------|
| `Alis.Reactive/ComponentRegistration.cs` | Modify | Add `ComponentType` field |
| `Alis.Reactive/ReactivePlan.cs` | Modify | Serialize `componentType` in plan JSON |
| `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteHtmlExtensions.cs` | Modify | Pass `"autocomplete"` to registration |
| `Alis.Reactive.Fusion/Components/FusionDropDownList/FusionDropDownListHtmlExtensions.cs` | Modify | Pass `"dropdownlist"` |
| `Alis.Reactive.Fusion/Components/FusionNumericTextBox/FusionNumericTextBoxHtmlExtensions.cs` | Modify | Pass `"numerictextbox"` |
| `Alis.Reactive.Fusion/Components/FusionSwitch/FusionSwitchHtmlExtensions.cs` | Modify | Pass `"switch"` |
| `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxHtmlExtensions.cs` | Modify | Pass `"textbox"` |
| `Alis.Reactive.Native/Components/NativeDropDown/NativeDropDownHtmlExtensions.cs` | Modify | Pass `"dropdown"` |
| `Alis.Reactive.SandboxApp/.../Models/BddExperimentModel.cs` | Create | TModel + validator |
| `Alis.Reactive.SandboxApp/.../Controllers/BddExperimentController.cs` | Create | Endpoints |
| `Alis.Reactive.SandboxApp/.../Views/BddExperiment/Index.cshtml` | Create | View with all component types |
| `tests/Alis.Reactive.Playwright.Extensions/PagePlan.cs` | Modify | Add validation against `componentType` |
| `tests/Alis.Reactive.Playwright.Extensions/DropDownListLocator.cs` | Create | Surfaces + gestures |
| `tests/Alis.Reactive.Playwright.Extensions/NumericTextBoxLocator.cs` | Create | Surfaces + gestures |
| `tests/Alis.Reactive.Playwright.Extensions/SwitchLocator.cs` | Create | Surfaces + gestures |
| `tests/Alis.Reactive.Playwright.Extensions/NativeTextBoxLocator.cs` | Create | Surfaces + gestures |
| `tests/Alis.Reactive.PlaywrightTests/.../WhenAdmittingResident.cs` | Create | The BDD test |

---

### Task 1: Add `componentType` to ComponentRegistration

**Files:**
- Modify: `Alis.Reactive/ComponentRegistration.cs`
- Modify: `Alis.Reactive/ReactivePlan.cs:86-91`

- [ ] **Step 1: Add ComponentType to ComponentRegistration**

```csharp
public sealed class ComponentRegistration
{
    public string ComponentId { get; }
    public string Vendor { get; }
    public string BindingPath { get; }
    public string ReadExpr { get; }
    public string ComponentType { get; }

    public ComponentRegistration(string componentId, string vendor, string bindingPath, string readExpr, string componentType)
    {
        ComponentId = componentId;
        Vendor = vendor;
        BindingPath = bindingPath;
        ReadExpr = readExpr;
        ComponentType = componentType;
    }
}
```

- [ ] **Step 2: Serialize componentType in plan JSON**

In `ReactivePlan.cs` `SerializeComponentsMap()`:
```csharp
result[kvp.Key] = new
{
    id = kvp.Value.ComponentId,
    vendor = kvp.Value.Vendor,
    readExpr = kvp.Value.ReadExpr,
    componentType = kvp.Value.ComponentType
};
```

- [ ] **Step 3: Fix all compilation errors**

Every `new ComponentRegistration(...)` call gains a 5th argument. Find all with:
```bash
grep -rn "new ComponentRegistration(" --include="*.cs" | grep -v "obj/"
```

Each HtmlExtensions file changes from:
```csharp
new ComponentRegistration(setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr)
```
to:
```csharp
new ComponentRegistration(setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "autocomplete")
```

Component type strings (kebab-case matching SF convention):

| Component | Type String |
|-----------|-------------|
| FusionAutoComplete | `"autocomplete"` |
| FusionDropDownList | `"dropdownlist"` |
| FusionMultiSelect | `"multiselect"` |
| FusionDatePicker | `"datepicker"` |
| FusionDateTimePicker | `"datetimepicker"` |
| FusionDateRangePicker | `"daterangepicker"` |
| FusionTimePicker | `"timepicker"` |
| FusionNumericTextBox | `"numerictextbox"` |
| FusionSwitch | `"switch"` |
| FusionInputMask | `"inputmask"` |
| FusionRichTextEditor | `"richtexteditor"` |
| FusionMultiColumnComboBox | `"multicolumncombobox"` |
| FusionFileUpload | `"fileupload"` |
| NativeTextBox | `"textbox"` |
| NativeCheckBox | `"checkbox"` |
| NativeDropDown | `"dropdown"` |
| NativeTextArea | `"textarea"` |
| NativeCheckList | `"checklist"` |
| NativeRadioGroup | `"radiogroup"` |
| NativeHiddenField | `"hiddenfield"` |

- [ ] **Step 4: Build all projects**

```bash
dotnet build
```
Expected: Build succeeded. Zero errors.

- [ ] **Step 5: Run existing tests — nothing should break**

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
```

Snapshot tests will need `.verified.txt` updates (plan JSON now has `componentType`).
Accept all snapshot changes — the new field is additive.

- [ ] **Step 6: Verify plan JSON has componentType**

Start the sandbox app, navigate to `/Sandbox/AutoComplete`, check plan JSON in page source:
```json
"Physician": { "id": "...", "vendor": "fusion", "readExpr": "value", "componentType": "autocomplete" }
```

---

### Task 2: Build sandbox page — BddExperiment

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/BddExperimentModel.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/BddExperimentController.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/BddExperiment/Index.cshtml`

- [ ] **Step 1: Create TModel + Validator**

```csharp
// BddExperimentModel.cs
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class BddExperimentModel
    {
        public string? ResidentName { get; set; }
        public string? Physician { get; set; }
        public string? CareLevel { get; set; }
        public decimal? MonthlyRate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class CareLevelOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class BddExperimentValidator : FluentValidation.AbstractValidator<BddExperimentModel>
    {
        public BddExperimentValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Physician is required.");
            RuleFor(x => x.MonthlyRate).NotNull().WithMessage("Monthly rate is required.");
        }
    }

    public class BddExperimentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
```

- [ ] **Step 2: Create Controller**

```csharp
// BddExperimentController.cs
[Area("Sandbox")]
public class BddExperimentController : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult Submit([FromBody] object? body)
        => Ok(new BddExperimentResponse { Success = true, Message = "Resident admitted" });
}
```

- [ ] **Step 3: Create View**

Uses: AutoComplete (Physician), DropDownList (CareLevel), NumericTextBox (MonthlyRate),
Switch (IsActive), NativeTextBox (ResidentName, Notes). POST with Validate + IncludeAll.

```cshtml
@model BddExperimentModel
@{
    ViewData["Title"] = "BDD Experiment";
    var plan = Html.ReactivePlan<BddExperimentModel>();
    var careLevels = new List<CareLevelOption>
    {
        new() { Value = "independent", Text = "Independent Living" },
        new() { Value = "assisted", Text = "Assisted Living" },
        new() { Value = "memory", Text = "Memory Care" }
    };

    Html.On(plan, t => t.CustomEvent("do-submit", p =>
    {
        p.Post("/Sandbox/BddExperiment/Submit")
         .Gather(g => g.IncludeAll())
         .Validate<BddExperimentValidator>("admission-form")
         .WhileLoading(l => l.Element("submit-status").SetText("Saving..."))
         .Response(r => r
            .OnSuccess<BddExperimentResponse>((json, s) =>
            {
                s.Element("submit-status").SetText(json, x => x.Message);
                s.Element("submit-status").AddClass("text-green-600");
            })
            .OnError(400, e =>
            {
                e.Element("submit-status").SetText("Validation failed");
                e.ValidationErrors("admission-form");
            }));
    }));
}

<form id="admission-form">
<native-vstack gap="Lg">
    <native-heading level="H1">BDD Experiment — Resident Admission</native-heading>

    <native-card><native-card-body>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            @{ Html.InputField(plan, m => m.ResidentName, o => o.Required().Label("Resident Name"))
                .NativeTextBox(b => b.Placeholder("Full name").CssClass("rounded-md border border-border px-3 py-1.5 text-sm")); }

            @{ Html.InputField(plan, m => m.Physician, o => o.Required().Label("Physician"))
                .AutoComplete(b => b.Placeholder("Search physician...")); }

            @{ Html.InputField(plan, m => m.CareLevel, o => o.Label("Care Level"))
                .DropDownList(b => b.DataSource(careLevels).Fields<CareLevelOption>(t => t.Text, v => v.Value).Placeholder("Select")); }

            @{ Html.InputField(plan, m => m.MonthlyRate, o => o.Required().Label("Monthly Rate"))
                .NumericTextBox(b => b.Min(0).Max(99999).Step(100)); }

            @{ Html.InputField(plan, m => m.IsActive, o => o.Label("Active"))
                .Switch(b => b); }

            @{ Html.InputField(plan, m => m.Notes, o => o.Label("Notes"))
                .NativeTextBox(b => b.Placeholder("Additional notes").CssClass("rounded-md border border-border px-3 py-1.5 text-sm")); }
        </div>
    </native-card-body></native-card>

    <div class="flex gap-4 items-center">
        @(Html.NativeButton("submit-btn", "Admit Resident")
            .CssClass("px-4 py-2 bg-[#7A2E3B] text-white rounded-md text-sm font-medium")
            .Reactive(plan, evt => evt.Click, (args, p) => { p.Dispatch("do-submit"); }))
        <span id="submit-status" class="text-sm text-text-muted">Ready</span>
    </div>
</native-vstack>
</form>

@Html.RenderPlan(plan)
```

- [ ] **Step 4: Build and verify page loads**

```bash
dotnet build
dotnet run --project Alis.Reactive.SandboxApp --urls "http://localhost:5220"
```

Navigate to `/Sandbox/BddExperiment`. Verify: components render, plan JSON has `componentType` for all 6 fields.

---

### Task 3: Add component locators to Playwright.Extensions

**Files:**
- Create: `tests/Alis.Reactive.Playwright.Extensions/DropDownListLocator.cs`
- Create: `tests/Alis.Reactive.Playwright.Extensions/NumericTextBoxLocator.cs`
- Create: `tests/Alis.Reactive.Playwright.Extensions/SwitchLocator.cs`
- Create: `tests/Alis.Reactive.Playwright.Extensions/NativeTextBoxLocator.cs`
- Modify: `tests/Alis.Reactive.Playwright.Extensions/PagePlan.cs`

- [ ] **Step 1: DropDownListLocator — surfaces + gestures**

```csharp
public sealed class DropDownListLocator
{
    // Surfaces: Input (wrapper span), Popup, PopupItems, PopupItem(text)
    // Gestures: Open (click wrapper), SelectItem(text), Focus, Blur
    // SF quirk: wrapper span intercepts clicks, not the inner input
}
```

- [ ] **Step 2: NumericTextBoxLocator — surfaces + gestures**

```csharp
public sealed class NumericTextBoxLocator
{
    // Surfaces: Input
    // Gestures: Fill(value), Clear, Focus, Blur
    // SF quirk: FillAsync works, but must Tab/Blur to trigger change event
}
```

- [ ] **Step 3: SwitchLocator — surfaces + gestures**

```csharp
public sealed class SwitchLocator
{
    // Surfaces: Wrapper (the clickable switch element)
    // Gestures: Toggle (click the wrapper)
    // SF quirk: clicking the wrapper span toggles the switch
}
```

- [ ] **Step 4: NativeTextBoxLocator — surfaces + gestures**

```csharp
public sealed class NativeTextBoxLocator
{
    // Surfaces: Input
    // Gestures: Type(text), Fill(text), Clear, Focus, Blur
    // Native: FillAsync works, BlurAsync triggers change
}
```

- [ ] **Step 5: Update PagePlan to resolve all component types**

`PagePlan<TModel>` gains: `DropDownList()`, `NumericTextBox()`, `Switch()`, `TextBox()`.
Each method validates `componentType` from the plan matches the expected type.

- [ ] **Step 6: Build**

```bash
dotnet build tests/Alis.Reactive.Playwright.Extensions
```

---

### Task 4: Write the BDD test — full user journey

**Files:**
- Create: `tests/Alis.Reactive.PlaywrightTests/Components/BddExperiment/WhenAdmittingResident.cs`

- [ ] **Step 1: Write the test**

```csharp
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

[TestFixture]
public class WhenAdmittingResident : PlaywrightTestBase
{
    private const string Path = "/Sandbox/BddExperiment";
    private PagePlan<BddExperimentModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<BddExperimentModel>.FromPage(Page);
    }

    [Test]
    public async Task submitting_empty_form_shows_validation_errors_then_fixing_leads_to_success()
    {
        await NavigateAndBoot();

        var name = _plan.TextBox(m => m.ResidentName);
        var physician = _plan.AutoComplete(m => m.Physician);
        var rate = _plan.NumericTextBox(m => m.MonthlyRate);
        var submitBtn = Page.Locator("#submit-btn");
        var status = _plan.Element("submit-status");

        // Given: empty form
        // When: click submit
        await submitBtn.ClickAsync();

        // Then: validation errors appear
        await Expect(status).ToContainTextAsync("Validation failed", new() { Timeout = 5000 });
        // Inline errors visible for required fields
        // (exact selectors depend on validation error rendering)

        // When: fill required fields
        await name.Fill("Margaret Thompson");
        await name.Blur();
        await physician.TypeAndSelect("smi", "Dr. Smith");
        await rate.Fill("4500");
        await rate.Blur();

        // When: resubmit
        await submitBtn.ClickAsync();

        // Then: success
        await Expect(status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
```

- [ ] **Step 2: Build and run**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "WhenAdmittingResident"
```

- [ ] **Step 3: Verify all existing tests still pass**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests
```

---

### Task 5: Evaluate — honest assessment

- [ ] **Step 1: Simulate component type change**

Temporarily change `BddExperimentModel.Physician` from `AutoComplete` to `DropDownList` in the view.
Run the test. Expected: `PagePlan` throws: *"Physician is dropdownlist, expected autocomplete"*.

- [ ] **Step 2: Simulate property rename**

Temporarily rename `Physician` → `PrimaryPhysician` on the model.
Expected: compiler error in BOTH view AND test.

- [ ] **Step 3: Compare line counts**

```bash
wc -l WhenAdmittingResident.cs          # with extensions
# vs estimated raw Playwright equivalent
```

- [ ] **Step 4: Ask a colleague to read the test**

Can they understand what it does without reading framework source?

- [ ] **Step 5: Write verdict**

Answer each evaluation criterion from the top of this plan:
1. ComponentType drift detection: YES/NO
2. Rename detection: YES/NO
3. Shorter + more readable: YES/NO (with line count)
4. Full journey works: YES/NO
5. New developer understands: YES/NO

If 4+ YES → adopt. If 3 or fewer → reconsider approach.
