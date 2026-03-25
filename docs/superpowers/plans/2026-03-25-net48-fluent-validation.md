# Net48 FluentValidation Integration Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add FluentValidation support to the net48 smoke test so the Intake form validates client-side and server-side identically to the net10 example.

**Architecture:** Add a ProjectReference to `Alis.Reactive.FluentValidator` (which already multi-targets net48 with FluentValidation 11.x). Register the adapter in `Global.asax.cs`. Port `IntakeValidator`. Update the view to use `.Validate<>()` and `ValidationErrors()`.

**Tech Stack:** Alis.Reactive.FluentValidator, FluentValidation 11.x (transitive via ProjectReference), MVC 5

---

## File Map

| Action | File | Responsibility |
|--------|------|----------------|
| Modify | `tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj` | Add ProjectReference to FluentValidator, add Compile for validator |
| Modify | `tests/Alis.Reactive.Net48.SmokeTest/Global.asax.cs` | Register `ReactivePlanConfig.UseValidationExtractor()` at startup |
| Create | `tests/Alis.Reactive.Net48.SmokeTest/Validators/IntakeValidator.cs` | Validation rules (same as example) |
| Modify | `tests/Alis.Reactive.Net48.SmokeTest/Views/Intake/Index.cshtml` | Add `.Validate<IntakeValidator>()` + `ValidationErrors()` |
| Modify | `tests/Alis.Reactive.Net48.SmokeTest/Controllers/IntakeController.cs` | Replace inline validation with FluentValidation |

---

### Task 1: Add ProjectReference to FluentValidator

**Files:**
- Modify: `tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`

- [ ] **Step 1: Add the ProjectReference**

In the `<ItemGroup>` that already has Alis.Reactive and Alis.Reactive.Native ProjectReferences, add:

```xml
<ProjectReference Include="..\..\Alis.Reactive.FluentValidator\Alis.Reactive.FluentValidator.csproj">
  <Name>Alis.Reactive.FluentValidator</Name>
</ProjectReference>
```

- [ ] **Step 2: Build to verify transitive FluentValidation resolves**

Run: `dotnet build tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`
Expected: 0 errors. FluentValidation.dll should appear in `bin/` via transitive dependency.

- [ ] **Step 3: Verify FluentValidation.dll landed in bin**

Run: `ls tests/Alis.Reactive.Net48.SmokeTest/bin/FluentValidation.dll`
Expected: File exists.

- [ ] **Step 4: Commit**

```bash
git add tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj
git commit -m "feat: add FluentValidator ProjectReference to net48 smoke test"
```

---

### Task 2: Register validation extractor at startup

**Files:**
- Modify: `tests/Alis.Reactive.Net48.SmokeTest/Global.asax.cs`

- [ ] **Step 1: Update Global.asax.cs**

Add the validation extractor registration in `Application_Start()`:

```csharp
using System;
using System.Web.Mvc;
using System.Web.Routing;
using Alis.Reactive;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Net48.SmokeTest
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Register FluentValidation extraction for client-side validation
            ReactivePlanConfig.UseValidationExtractor(
                new FluentValidationAdapter(type =>
                    (FluentValidation.IValidator)Activator.CreateInstance(type)));
        }
    }
}
```

Note: net48 uses `FluentValidation.IValidator` (not nullable — FV 11 doesn't have nullable annotations).

- [ ] **Step 2: Build to verify registration compiles**

Run: `dotnet build tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add tests/Alis.Reactive.Net48.SmokeTest/Global.asax.cs
git commit -m "feat: register FluentValidation extractor in Global.asax"
```

---

### Task 3: Create IntakeValidator

**Files:**
- Create: `tests/Alis.Reactive.Net48.SmokeTest/Validators/IntakeValidator.cs`
- Modify: `tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj` (add Compile item)

- [ ] **Step 1: Create the validator**

```csharp
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Net48.SmokeTest.Models;
using FluentValidation;

namespace Alis.Reactive.Net48.SmokeTest.Validators
{
    public class IntakeValidator : ReactiveValidator<ResidentIntakeModel>
    {
        public IntakeValidator()
        {
            // Personal Info — always required
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DateOfBirth).NotEmpty();

            // Placement — always required
            RuleFor(x => x.FacilityId).NotEmpty().WithMessage("Please select a facility");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Please select a care level");
            RuleFor(x => x.AdmissionDate).NotEmpty();
            RuleFor(x => x.MonthlyRate).NotEmpty();

            // Emergency Contact — always required
            RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(20);

            // Conditional: medication management → physician required
            WhenField(x => x.RequiresMedicationManagement, () =>
            {
                RuleFor(x => x.PrimaryPhysician).NotEmpty()
                    .WithMessage("Physician is required when medication management is needed");
            });

            // Conditional: care level = memory-care → cognitive assessment required
            WhenField(x => x.CareLevel, "memory-care", () =>
            {
                RuleFor(x => x.CognitiveAssessmentDate).NotEmpty()
                    .WithMessage("Cognitive assessment date is required for memory care residents");
            });

            // Facility-specific fields
            RuleFor(x => x.RoomPreference).NotEmpty()
                .WithMessage("Room preference is required for this facility")
                .MaximumLength(200);
            RuleFor(x => x.DepositAmount).NotEmpty()
                .WithMessage("Move-in deposit is required");
        }
    }
}
```

- [ ] **Step 2: Add Compile item to .csproj**

Add to the `<ItemGroup>` with other `<Compile>` entries:

```xml
<Compile Include="Validators\IntakeValidator.cs" />
```

- [ ] **Step 3: Build**

Run: `dotnet build tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`
Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add tests/Alis.Reactive.Net48.SmokeTest/Validators/IntakeValidator.cs \
       tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj
git commit -m "feat: add IntakeValidator with conditional validation rules"
```

---

### Task 4: Update view to use `.Validate<>()` + `ValidationErrors()`

**Files:**
- Modify: `tests/Alis.Reactive.Net48.SmokeTest/Views/Intake/Index.cshtml`

- [ ] **Step 1: Add using directive**

Add at the top of the file (after existing `@using` lines):

```csharp
@using Alis.Reactive.Net48.SmokeTest.Validators
```

- [ ] **Step 2: Add `.Validate<IntakeValidator>()` to the POST call**

In the submit button's `.Reactive()` handler, change:

```csharp
t.Post("/Intake/Save", g => g.IncludeAll())
 .WhileLoading(l =>
```

to:

```csharp
t.Post("/Intake/Save", g => g.IncludeAll())
 .Validate<IntakeValidator>("intake-form")
 .WhileLoading(l =>
```

- [ ] **Step 3: Add `ValidationErrors()` to the OnError(400) handler**

In the `OnError(400)` handler, change:

```csharp
.OnError(400, e =>
{
    e.Component<NativeLoader>().Hide();
    e.Element("error-toast").Show();
})
```

to:

```csharp
.OnError(400, e =>
{
    e.Component<NativeLoader>().Hide();
    e.ValidationErrors("intake-form");
})
```

Remove the static `error-toast` div since `ValidationErrors` now handles error display.

- [ ] **Step 4: Build**

Run: `dotnet build tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`
Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add tests/Alis.Reactive.Net48.SmokeTest/Views/Intake/Index.cshtml
git commit -m "feat: wire IntakeValidator into form submission pipeline"
```

---

### Task 5: Replace inline controller validation with FluentValidation

**Files:**
- Modify: `tests/Alis.Reactive.Net48.SmokeTest/Controllers/IntakeController.cs`

- [ ] **Step 1: Replace the Save action's inline validation**

Replace the manual `errors` dictionary with the FluentValidation validator:

```csharp
using Alis.Reactive.Net48.SmokeTest.Validators;
// ... (existing usings)

[HttpPost]
public ActionResult Save(ResidentIntakeModel model)
{
    if (model == null)
    {
        Response.StatusCode = 400;
        return Json(new { errors = new { FirstName = new[] { "Request body is required." } } },
            JsonRequestBehavior.AllowGet);
    }

    var validator = new IntakeValidator();
    var result = validator.Validate(model);

    if (!result.IsValid)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
        Response.StatusCode = 400;
        return Json(new { errors }, JsonRequestBehavior.AllowGet);
    }

    return Json(new { message = "Intake saved successfully" }, JsonRequestBehavior.AllowGet);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build tests/Alis.Reactive.Net48.SmokeTest/Alis.Reactive.Net48.SmokeTest.csproj`
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add tests/Alis.Reactive.Net48.SmokeTest/Controllers/IntakeController.cs
git commit -m "feat: use FluentValidation in IntakeController.Save"
```

---

### Task 6: Verify end-to-end (manual)

- [ ] **Step 1: Build the full solution**

Run: `dotnet build`
Expected: 0 errors.

- [ ] **Step 2: Run the net48 smoke test in IIS Express**

Open in VS → Set as StartUp Project → F5 → Navigate to `/Intake`.

- [ ] **Step 3: Verify validation fires**

1. Click "Submit Intake" without filling fields → confirm dialog → Accept
2. Expected: validation errors appear inline on required fields
3. Fill in all required fields → Submit → Should succeed with confirmation number

- [ ] **Step 4: Test conditional validation**

1. Check "Requires Medication Management" → physician section appears
2. Submit without physician → validation error on PrimaryPhysician
3. Select "Memory Care" → cognitive section appears
4. Submit without assessment date → validation error on CognitiveAssessmentDate

---

## Known Risks

1. **Transitive FluentValidation resolution** — if MSBuild doesn't bring FluentValidation.dll transitively, fall back to adding `FluentValidation 11.11.0` to `packages.config` with a `<Reference>` HintPath.

2. **Binding redirect** — FluentValidation 11.x may need a binding redirect in `Web.config` if the resolved version doesn't match at runtime. Watch for `FileLoadException` on `FluentValidation`.

3. **Razor compilation on net48** — the `@using` for the Validators namespace must be present. If Razor can't find `IntakeValidator`, add it to `Views/Web.config` under `<system.web.webPages.razor><pages><namespaces>`.
