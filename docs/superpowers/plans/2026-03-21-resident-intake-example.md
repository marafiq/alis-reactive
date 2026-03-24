# Resident Intake Example App — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a standalone ASP.NET MVC app demonstrating 22+ Alis.Reactive features in one cohesive resident intake form. Ships as a downloadable ZIP from the docs site with a walkthrough page.

**Architecture:** Single-page intake form (no areas, no database). Lookup data served from in-memory controller endpoints. Framework DLLs referenced from local `lib/` folder. Runtime JS/CSS copied into `wwwroot/`. Syncfusion + FluentValidation come from NuGet.

**Tech Stack:** .NET 10, ASP.NET MVC, Alis.Reactive (DLLs), Syncfusion EJ2 32.2.8 (NuGet + CDN), FluentValidation 12.x, Starlight docs (Astro)

---

## File Structure

```
examples/resident-intake/
├── ResidentIntake.csproj          ← DLL refs + NuGet deps
├── Program.cs                     ← Minimal MVC startup
├── Models/
│   ├── ResidentIntakeModel.cs     ← 13 properties
│   └── LookupData.cs             ← DTOs: LookupItem, response types
├── Validators/
│   └── IntakeValidator.cs         ← 10 rules, 2 WhenField conditionals
├── Controllers/
│   └── IntakeController.cs        ← Index + 5 API endpoints
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml         ← SF CDN + runtime module
│   └── Intake/
│       ├── Index.cshtml           ← THE view — full reactive plan
│       └── _Summary.cshtml        ← Partial for drawer
├── wwwroot/
│   ├── js/alis-reactive.js        ← copied from framework build
│   └── css/design-system.css      ← copied from framework build
└── lib/
    ├── Alis.Reactive.dll
    ├── Alis.Reactive.Native.dll
    ├── Alis.Reactive.Fusion.dll
    ├── Alis.Reactive.FluentValidator.dll
    └── Alis.Reactive.NativeTagHelpers.dll

docs-site/
├── public/downloads/
│   └── resident-intake.zip        ← packaged example
└── src/content/docs/examples/
    └── resident-intake.mdx        ← walkthrough page
```

---

## Task 1: Create Project Skeleton

**Files:**
- Create: `examples/resident-intake/ResidentIntake.csproj`
- Create: `examples/resident-intake/Program.cs`
- Create: `examples/resident-intake/Views/_ViewImports.cshtml`
- Create: `examples/resident-intake/Views/_ViewStart.cshtml`
- Create: `examples/resident-intake/Views/Shared/_Layout.cshtml`

- [ ] **Step 1: Create the .csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Syncfusion.EJ2.AspNet.Core" Version="32.2.8" />
    <PackageReference Include="FluentValidation" Version="12.*" />
    <PackageReference Include="Newtonsoft.Json" Version="13.*" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="Alis.Reactive">
      <HintPath>lib/Alis.Reactive.dll</HintPath>
    </Reference>
    <Reference Include="Alis.Reactive.Native">
      <HintPath>lib/Alis.Reactive.Native.dll</HintPath>
    </Reference>
    <Reference Include="Alis.Reactive.Fusion">
      <HintPath>lib/Alis.Reactive.Fusion.dll</HintPath>
    </Reference>
    <Reference Include="Alis.Reactive.FluentValidator">
      <HintPath>lib/Alis.Reactive.FluentValidator.dll</HintPath>
    </Reference>
    <Reference Include="Alis.Reactive.NativeTagHelpers">
      <HintPath>lib/Alis.Reactive.NativeTagHelpers.dll</HintPath>
    </Reference>
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create Program.cs**

```csharp
using Alis.Reactive;
using Alis.Reactive.FluentValidator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// SF EJ2 uses Newtonsoft internally — camelCase for DataSource rendering
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register FluentValidation extraction for client-side validation
ReactivePlanConfig.UseValidationExtractor(
    new FluentValidationAdapter(type =>
        (FluentValidation.IValidator?)Activator.CreateInstance(type)));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Intake}/{action=Index}/{id?}");

app.Run();
```

- [ ] **Step 3: Create _ViewImports.cshtml**

```cshtml
@using ResidentIntake.Models
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@using Alis.Reactive.Native.AppLevel
@using Alis.Reactive.Builders.Requests
@using Alis.Reactive.Fusion.Components
@using Alis.Reactive.Fusion.AppLevel
@using Syncfusion.EJ2
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Alis.Reactive.NativeTagHelpers
```

- [ ] **Step 4: Create _ViewStart.cshtml**

```cshtml
@{
    Layout = "_Layout";
}
```

- [ ] **Step 5: Create _Layout.cshtml**

Simplified layout — same structure as sandbox but with minimal nav:

```html
<!DOCTYPE html>
<html lang="en" class="h-full">
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>@ViewData["Title"] — Resident Intake</title>
    <link rel="stylesheet" href="https://cdn.syncfusion.com/ej2/32.2.8/tailwind3.css"/>
    <link rel="stylesheet" href="~/css/design-system.css" asp-append-version="true"/>
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap"
          rel="stylesheet" media="print" onload="this.media='all'"/>
</head>
<body class="alis-root h-full">
<div class="min-h-full flex flex-col">
    <div class="h-1 bg-gradient-to-r from-[#7A2E3B] via-[#9B4A57] to-[#D4A053]"></div>

    <header class="border-b border-border bg-white">
        <native-container>
            <div class="flex items-center h-16 gap-4">
                <span class="w-8 h-8 rounded-lg bg-gradient-to-br from-[#7A2E3B] to-[#9B4A57]
                             flex items-center justify-center">
                    <span class="text-white font-bold text-sm">A</span>
                </span>
                <span class="text-lg font-bold tracking-tight text-[#7A2E3B]">
                    Resident Intake
                </span>
                <span class="text-sm text-text-muted ml-auto">Powered by Alis.Reactive</span>
            </div>
        </native-container>
    </header>

    <main class="flex-1">
        <native-container class="py-10">
            @RenderBody()
        </native-container>
    </main>

    <footer class="border-t border-border bg-white mt-auto">
        <native-container class="py-6">
            <p class="text-xs text-text-muted">&copy; 2026 Alis.Reactive Example</p>
        </native-container>
    </footer>
</div>

<script src="https://cdn.syncfusion.com/ej2/32.2.8/dist/ej2.min.js"></script>
@Html.FusionToast()
@Html.EJS().ScriptManager()
@Html.FusionConfirmDialog()
@Html.NativeLoader()
@Html.NativeDrawer()
<script type="module" src="~/js/alis-reactive.js" asp-append-version="true"></script>
</body>
</html>
```

---

## Task 2: Create Model & DTOs

**Files:**
- Create: `examples/resident-intake/Models/ResidentIntakeModel.cs`
- Create: `examples/resident-intake/Models/LookupData.cs`

- [ ] **Step 1: Create ResidentIntakeModel**

```csharp
namespace ResidentIntake.Models;

public class ResidentIntakeModel
{
    // Personal Info
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Placement
    public string? FacilityId { get; set; }
    public string? UnitId { get; set; }
    public string? CareLevel { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public decimal? MonthlyRate { get; set; }

    // Medical
    public bool RequiresMedicationManagement { get; set; }
    public string? PrimaryPhysician { get; set; }
    public DateTime? CognitiveAssessmentDate { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
```

- [ ] **Step 2: Create LookupData DTOs**

```csharp
namespace ResidentIntake.Models;

public record LookupItem(string Id, string Name);

// Response shapes for API endpoints
public class FacilitiesResponse
{
    public List<LookupItem> Facilities { get; set; } = [];
}

public class CareLevelsResponse
{
    public List<LookupItem> Levels { get; set; } = [];
}

public class UnitsResponse
{
    public List<LookupItem> Units { get; set; } = [];
}

public class ConfirmationResponse
{
    public string Number { get; set; } = "";
}
```

---

## Task 3: Create Validator

**Files:**
- Create: `examples/resident-intake/Validators/IntakeValidator.cs`

- [ ] **Step 1: Create IntakeValidator with unconditional + conditional rules**

```csharp
using Alis.Reactive.FluentValidator;
using FluentValidation;
using ResidentIntake.Models;

namespace ResidentIntake.Validators;

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
        RuleFor(x => x.MonthlyRate).NotEmpty()
            .GreaterThan(0).WithMessage("Monthly rate must be greater than zero");

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
    }
}
```

---

## Task 4: Create Controller

**Files:**
- Create: `examples/resident-intake/Controllers/IntakeController.cs`

- [ ] **Step 1: Create IntakeController with all endpoints**

```csharp
using Microsoft.AspNetCore.Mvc;
using ResidentIntake.Models;
using ResidentIntake.Validators;

namespace ResidentIntake.Controllers;

public class IntakeController : Controller
{
    public IActionResult Index() => View(new ResidentIntakeModel());

    [HttpGet]
    public IActionResult Facilities()
    {
        var facilities = new FacilitiesResponse
        {
            Facilities =
            [
                new("sunrise", "Sunrise Senior Living"),
                new("oakwood", "Oakwood Care Center"),
                new("maple", "Maple Grove Residence")
            ]
        };
        return Ok(facilities);
    }

    [HttpGet]
    public IActionResult CareLevels()
    {
        var levels = new CareLevelsResponse
        {
            Levels =
            [
                new("independent", "Independent Living"),
                new("assisted", "Assisted Living"),
                new("memory-care", "Memory Care")
            ]
        };
        return Ok(levels);
    }

    [HttpGet]
    public IActionResult Units([FromQuery] string? facilityId)
    {
        var units = (facilityId ?? "").ToLowerInvariant() switch
        {
            "sunrise" =>
            [
                new LookupItem("s-101", "Suite 101 — Garden View"),
                new LookupItem("s-102", "Suite 102 — Garden View"),
                new LookupItem("s-201", "Suite 201 — Courtyard"),
                new LookupItem("s-202", "Suite 202 — Courtyard")
            ],
            "oakwood" =>
            [
                new LookupItem("o-a1", "Wing A — Room 1"),
                new LookupItem("o-a2", "Wing A — Room 2"),
                new LookupItem("o-b1", "Wing B — Room 1")
            ],
            "maple" =>
            [
                new LookupItem("m-10", "Cottage 10"),
                new LookupItem("m-11", "Cottage 11"),
                new LookupItem("m-12", "Cottage 12"),
                new LookupItem("m-13", "Cottage 13"),
                new LookupItem("m-14", "Cottage 14")
            ],
            _ => new List<LookupItem>()
        };

        return Ok(new UnitsResponse { Units = units });
    }

    [HttpPost]
    public IActionResult Save([FromBody] ResidentIntakeModel? model)
    {
        if (model == null)
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    ["FirstName"] = ["Request body is required."]
                }
            });

        var validator = new IntakeValidator();
        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Intake saved successfully" });
    }

    [HttpGet]
    public IActionResult ConfirmationNumber()
    {
        // Simulate generating a confirmation number
        var number = $"RES-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        return Ok(new ConfirmationResponse { Number = number });
    }

    [HttpGet]
    public IActionResult Summary([FromQuery] string? firstName, [FromQuery] string? lastName,
        [FromQuery] string? facilityId, [FromQuery] string? careLevel,
        [FromQuery] string? admissionDate, [FromQuery] decimal? monthlyRate)
    {
        ViewBag.FirstName = firstName ?? "—";
        ViewBag.LastName = lastName ?? "—";
        ViewBag.FacilityId = facilityId ?? "—";
        ViewBag.CareLevel = careLevel ?? "—";
        ViewBag.AdmissionDate = admissionDate ?? "—";
        ViewBag.MonthlyRate = monthlyRate?.ToString("C") ?? "—";
        return PartialView("_Summary");
    }
}
```

---

## Task 5: Create the View — Index.cshtml

**Files:**
- Create: `examples/resident-intake/Views/Intake/Index.cshtml`

This is the star of the show. Every section demonstrates framework features naturally.

- [ ] **Step 1: Create the full reactive plan view**

```cshtml
@model ResidentIntakeModel
@using ResidentIntake.Validators
@{
    ViewData["Title"] = "New Resident Intake";
    var plan = Html.ReactivePlan<ResidentIntakeModel>();

    // ── DomReady: Load facility + care level lookups via parallel HTTP ──
    Html.On(plan, t => t.DomReady(pipeline =>
    {
        pipeline.Parallel(
            a => a.Get("/Intake/Facilities")
                  .Response(r => r.OnSuccess<FacilitiesResponse>((json, s) =>
                  {
                      s.Component<FusionDropDownList>(m => m.FacilityId)
                          .SetDataSource(json, j => j.Facilities)
                          .DataBind();
                  })),
            b => b.Get("/Intake/CareLevels")
                  .Response(r => r.OnSuccess<CareLevelsResponse>((json, s) =>
                  {
                      s.Component<FusionDropDownList>(m => m.CareLevel)
                          .SetDataSource(json, j => j.Levels)
                          .DataBind();
                  }))
        ).OnAllSettled(s =>
        {
            s.Element("loading-message").Hide();
            s.Element("intake-form").Show();
            s.Element("form-actions").Show();
        });
    }));

    // ── CustomEvent: "do-save" — decouples button click from save logic ──
    Html.On(plan, t => t.CustomEvent("intake-saved", pipeline =>
    {
        pipeline.Get("/Intake/Summary")
            .Gather(g => g.IncludeAll())
            .Response(r => r.OnSuccess(s =>
            {
                s.Into("alis-drawer-content");
            }));
        pipeline.Element("alis-drawer-title").SetText("Intake Summary");
        pipeline.Component<NativeDrawer>().SetSize(DrawerSize.Md);
        pipeline.Component<NativeDrawer>().Open();
    }));
}

<native-vstack gap="Lg">
    <div>
        <native-heading level="H1">New Resident Intake</native-heading>
        <native-text color="Secondary">
            Complete all sections below. Fields marked with * are required.
        </native-text>
    </div>

    <p id="loading-message" class="text-sm text-text-muted">Loading form data...</p>

    <native-card>
    <native-card-body>
        <form id="intake-form" class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4" hidden>

            @* ═══════════════════════════════════════════════════════ *@
            @* PERSONAL INFORMATION                                    *@
            @* ═══════════════════════════════════════════════════════ *@
            <div class="col-span-full">
                <native-heading level="H3">Personal Information</native-heading>
            </div>

            @{ Html.InputField(plan, m => m.FirstName, o => o.Required().Label("First Name"))
                .NativeTextBox(b => b
                    .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                    .Placeholder("Jane")); }

            @{ Html.InputField(plan, m => m.LastName, o => o.Required().Label("Last Name"))
                .NativeTextBox(b => b
                    .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                    .Placeholder("Doe")); }

            @{ Html.InputField(plan, m => m.DateOfBirth, o => o.Required().Label("Date of Birth"))
                .DatePicker(b => b
                    .Placeholder("Select date of birth")); }

            @* ═══════════════════════════════════════════════════════ *@
            @* PLACEMENT                                               *@
            @* ═══════════════════════════════════════════════════════ *@
            <div class="col-span-full mt-2">
                <native-heading level="H3">Placement</native-heading>
            </div>

            @{ Html.InputField(plan, m => m.FacilityId, o => o.Required().Label("Facility"))
                .DropDownList(b => b
                    .Placeholder("Select facility")
                    .Fields<LookupItem>(t => t.Name, v => v.Id)
                    .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
                    {
                        // ── Cascading: Facility → GET → populate Unit dropdown ──
                        pipeline.Get("/Intake/Units")
                            .Gather(g => g
                                .Include<FusionDropDownList, ResidentIntakeModel>(m => m.FacilityId))
                            .Response(r => r
                                .OnSuccess<UnitsResponse>((json, s) =>
                                {
                                    s.Component<FusionDropDownList>(m => m.UnitId)
                                        .SetDataSource(json, j => j.Units)
                                        .DataBind();
                                }));
                    })); }

            @{ Html.InputField(plan, m => m.UnitId, o => o.Label("Unit"))
                .DropDownList(b => b
                    .Placeholder("Select unit — pick a facility first")
                    .Fields<LookupItem>(t => t.Name, v => v.Id)); }

            @{ Html.InputField(plan, m => m.CareLevel, o => o.Required().Label("Care Level"))
                .DropDownList(b => b
                    .Placeholder("Select care level")
                    .Fields<LookupItem>(t => t.Name, v => v.Id)
                    .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
                    {
                        // ── Condition: "Memory Care" → show cognitive assessment ──
                        pipeline.When(args, a => a.Value).Eq("memory-care")
                            .Then(t => t.Element("cognitive-section").Show())
                            .Else(e => e.Element("cognitive-section").Hide());
                    })); }

            @{ Html.InputField(plan, m => m.AdmissionDate, o => o.Required().Label("Admission Date"))
                .DatePicker(b => b
                    .Placeholder("Select admission date")); }

            @{ Html.InputField(plan, m => m.MonthlyRate, o => o.Required().Label("Monthly Rate ($)"))
                .NumericTextBox(b => b
                    .Min(0).Step(100)); }

            <div id="cognitive-section" class="col-span-full" hidden>
                @{ Html.InputField(plan, m => m.CognitiveAssessmentDate,
                    o => o.Label("Last Cognitive Assessment"))
                    .DatePicker(b => b
                        .Placeholder("Assessment date")); }
            </div>

            @* ═══════════════════════════════════════════════════════ *@
            @* MEDICAL                                                 *@
            @* ═══════════════════════════════════════════════════════ *@
            <div class="col-span-full mt-2">
                <native-heading level="H3">Medical</native-heading>
            </div>

            <div class="col-span-full">
                @{ Html.InputField(plan, m => m.RequiresMedicationManagement,
                    o => o.Label("Requires Medication Management"))
                    .NativeCheckBox(b => b
                        .CssClass("h-4 w-4 rounded border-border text-accent")
                        .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
                        {
                            // ── Condition: checkbox → show/hide physician field ──
                            pipeline.When(args, a => a.Checked).Truthy()
                                .Then(t => t.Element("physician-section").Show())
                                .Else(e => e.Element("physician-section").Hide());
                        })); }
            </div>

            <div id="physician-section" class="col-span-full" hidden>
                @{ Html.InputField(plan, m => m.PrimaryPhysician,
                    o => o.Label("Primary Physician"))
                    .NativeTextBox(b => b
                        .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                        .Placeholder("Dr. Smith")); }
            </div>

            @* ═══════════════════════════════════════════════════════ *@
            @* EMERGENCY CONTACT                                       *@
            @* ═══════════════════════════════════════════════════════ *@
            <div class="col-span-full mt-2">
                <native-heading level="H3">Emergency Contact</native-heading>
            </div>

            @{ Html.InputField(plan, m => m.EmergencyContactName,
                o => o.Required().Label("Contact Name"))
                .NativeTextBox(b => b
                    .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                    .Placeholder("John Doe")); }

            @{ Html.InputField(plan, m => m.EmergencyContactPhone,
                o => o.Required().Label("Contact Phone"))
                .NativeTextBox(b => b
                    .CssClass("rounded-md border border-border px-3 py-1.5 text-sm")
                    .Placeholder("(555) 123-4567")); }

        </form>

        @* ═══════════════════════════════════════════════════════ *@
        @* FORM ACTIONS                                            *@
        @* ═══════════════════════════════════════════════════════ *@
        <div id="form-actions" class="mt-6 flex items-center gap-3" hidden>
            @(Html.NativeButton("submit-intake", "Submit Intake")
                .CssClass("rounded-md bg-accent px-6 py-2 text-sm font-medium text-white hover:bg-accent/90")
                .Reactive(plan, evt => evt.Click, (args, pipeline) =>
                {
                    // ── Confirm → POST → Validate → Toast → Chained confirmation # ──
                    pipeline.Confirm("Submit this resident intake? Please verify all information is correct.")
                        .Then(t =>
                        {
                            t.Post("/Intake/Save", g => g.IncludeAll())
                             .Validate<IntakeValidator>("intake-form")
                             .WhileLoading(l =>
                             {
                                 l.Component<NativeLoader>().Show();
                             })
                             .Response(r => r
                                .OnSuccess(s =>
                                {
                                    s.Component<FusionToast>()
                                        .SetTitle("Resident Intake")
                                        .SetContent("Intake submitted successfully")
                                        .Success()
                                        .Show();
                                    s.Element("post-save-section").Show();
                                    s.Element("submit-intake").Hide();
                                })
                                .OnError(400, e =>
                                {
                                    e.ValidationErrors("intake-form");
                                })
                                .Chained(c => c
                                    .Get("/Intake/ConfirmationNumber")
                                    .Response(r2 => r2
                                        .OnSuccess<ConfirmationResponse>((json, s2) =>
                                        {
                                            s2.Element("confirmation-number")
                                                .SetText(json, j => j.Number);
                                        }))));
                        });
                }))
        </div>

        @* ═══════════════════════════════════════════════════════ *@
        @* POST-SAVE SECTION (hidden until save succeeds)          *@
        @* ═══════════════════════════════════════════════════════ *@
        <div id="post-save-section" class="mt-6 p-4 rounded-md border border-border bg-surface-alt" hidden>
            <div class="flex items-center gap-2 mb-2">
                <span class="w-2 h-2 rounded-full bg-green-500"></span>
                <span class="text-sm font-medium text-text-primary">Intake Submitted</span>
            </div>
            <p class="text-sm text-text-secondary">
                Confirmation: <strong id="confirmation-number" class="text-text-primary"></strong>
            </p>
            <div class="mt-3">
                @(Html.NativeButton("view-summary", "View Summary")
                    .CssClass("rounded-md border border-border px-4 py-1.5 text-sm font-medium hover:bg-surface-alt")
                    .Reactive(plan, evt => evt.Click, (args, pipeline) =>
                    {
                        // ── Dispatch event → CustomEvent handler loads drawer ──
                        pipeline.Dispatch("intake-saved");
                    }))
            </div>
        </div>

    </native-card-body>
    </native-card>

    @* ═══════════════════════════════════════════════════════ *@
    @* PLAN JSON (developer reference)                         *@
    @* ═══════════════════════════════════════════════════════ *@
    <native-card>
    <native-card-body>
        <native-heading level="H3">Plan JSON</native-heading>
        <native-text color="Secondary" class="mb-3">
            The rendered JSON plan — this is the only contract between C# and the browser.
        </native-text>
        <pre class="text-xs overflow-auto max-h-96 p-3 bg-surface-alt rounded-md">
            <code>@Html.Raw(plan.RenderFormatted())</code>
        </pre>
    </native-card-body>
    </native-card>
</native-vstack>

@Html.RenderPlan(plan)
```

**Features demonstrated in this view:**

| Line range | Feature |
|------------|---------|
| DomReady block | DomReady trigger, Parallel HTTP, SetDataSource, DataBind |
| CustomEvent block | CustomEvent trigger, Into, NativeDrawer, Dispatch |
| Facility dropdown | Cascading dropdowns, Gather Include, GET, SetDataSource |
| Care Level dropdown | Condition (Eq), Show/Hide element |
| Checkbox | Condition (Truthy/Falsy), Show/Hide element |
| Submit button | Confirm guard, POST, IncludeAll, Validate, WhileLoading, NativeLoader |
| OnSuccess | FusionToast, Element Show/Hide, SetText |
| OnError 400 | ValidationErrors routing |
| Chained | Chained HTTP, ResponseBody typed access |
| View Summary | Dispatch, NativeDrawer, Into, Gather IncludeAll |

---

## Task 6: Create Summary Partial

**Files:**
- Create: `examples/resident-intake/Views/Intake/_Summary.cshtml`

- [ ] **Step 1: Create the drawer summary partial**

```html
<div class="space-y-4 p-2">
    <div class="border-b border-border pb-3">
        <h4 class="text-sm font-semibold text-text-primary mb-2">Resident</h4>
        <p class="text-sm">@ViewBag.FirstName @ViewBag.LastName</p>
    </div>
    <div class="border-b border-border pb-3">
        <h4 class="text-sm font-semibold text-text-primary mb-2">Placement</h4>
        <dl class="grid grid-cols-2 gap-2 text-sm">
            <dt class="text-text-muted">Facility</dt>
            <dd>@ViewBag.FacilityId</dd>
            <dt class="text-text-muted">Care Level</dt>
            <dd>@ViewBag.CareLevel</dd>
            <dt class="text-text-muted">Admission</dt>
            <dd>@ViewBag.AdmissionDate</dd>
            <dt class="text-text-muted">Monthly Rate</dt>
            <dd>@ViewBag.MonthlyRate</dd>
        </dl>
    </div>
    <div>
        <p class="text-xs text-text-muted">
            This summary was loaded into the drawer via <code>Into()</code> —
            an HTTP GET fetched this partial and injected it as HTML.
        </p>
    </div>
</div>
```

---

## Task 7: Build Framework & Copy Assets

**Files:**
- Copy: framework DLLs → `examples/resident-intake/lib/`
- Copy: `Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js` → `examples/resident-intake/wwwroot/js/`
- Copy: `Alis.Reactive.SandboxApp/wwwroot/css/design-system.css` → `examples/resident-intake/wwwroot/css/`

- [ ] **Step 1: Build the framework from repo root**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive
dotnet build -c Release
npm run build:all
```

- [ ] **Step 2: Create lib/ and wwwroot/ directories**

```bash
cd examples/resident-intake
mkdir -p lib wwwroot/js wwwroot/css
```

- [ ] **Step 3: Copy framework DLLs**

Copy from `bin/Release/net10.0/` of each project. The exact DLLs needed:

```bash
RELEASE=bin/Release/net10.0

cp ../../Alis.Reactive/$RELEASE/Alis.Reactive.dll lib/
cp ../../Alis.Reactive.Native/$RELEASE/Alis.Reactive.Native.dll lib/
cp ../../Alis.Reactive.Fusion/$RELEASE/Alis.Reactive.Fusion.dll lib/
cp ../../Alis.Reactive.FluentValidator/$RELEASE/Alis.Reactive.FluentValidator.dll lib/
cp ../../Alis.Reactive.NativeTagHelpers/$RELEASE/Alis.Reactive.NativeTagHelpers.dll lib/
```

**Note:** If any DLL has transitive dependencies beyond NuGet packages (SF, FV, Newtonsoft), those must be copied too. Check with `dotnet publish` output if needed.

- [ ] **Step 4: Copy runtime JS and CSS**

```bash
cp ../../Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js wwwroot/js/
cp ../../Alis.Reactive.SandboxApp/wwwroot/css/design-system.css wwwroot/css/
```

---

## Task 8: Build & Verify the Example App

- [ ] **Step 1: Restore NuGet packages and build**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/examples/resident-intake
dotnet restore
dotnet build
```

Expected: Build succeeds with no errors.

- [ ] **Step 2: Run the app**

```bash
dotnet run
```

Expected: App starts on `http://localhost:5000` (or similar).

- [ ] **Step 3: Verify in browser**

Open the app and test each feature:
1. Page loads → dropdowns populate (Parallel GET)
2. Select facility → units dropdown populates (Cascading)
3. Select "Memory Care" → cognitive assessment field appears (Condition)
4. Check "Requires Medication Management" → physician field appears (Condition)
5. Submit empty form → validation errors show on required fields
6. Fill all required fields → Submit → confirmation dialog appears (Confirm)
7. Click OK → save executes → toast shows → confirmation number appears (POST + Chained)
8. Click "View Summary" → drawer opens with summary partial (Dispatch + Drawer + Into)

- [ ] **Step 4: Fix any compilation or runtime issues**

If builder methods don't match (e.g., SF API differences), read the actual builder source and adjust.

---

## Task 9: Create the Docs Walkthrough Page

**Files:**
- Create: `docs-site/src/content/docs/examples/resident-intake.mdx`
- Modify: `docs-site/astro.config.mjs` (add Examples section to sidebar)

- [ ] **Step 1: Create the MDX page**

Structure — question → answer, progressive disclosure:

```
---
title: "Worked Example: Resident Intake Form"
description: "End-to-end example — 6 component types, cascading dropdowns, conditions, validation, HTTP, toast, drawer — in one cohesive form."
---

import { Tabs, TabItem } from '@astrojs/starlight/components';

## What are we building?

[Screenshot / description of the resident intake form]
[Feature coverage table — 22 features in one page]
[Download link to ZIP]

## The Model

<Tabs>
  <TabItem label="ResidentIntakeModel.cs">[model code]</TabItem>
  <TabItem label="LookupData.cs">[DTO code]</TabItem>
</Tabs>

## How does the form load its data?

DomReady → Parallel GET → populate dropdowns

[3-5 line code snippet from the DomReady block]
[Explain: two independent lookups, OnAllSettled shows the form]

## What happens when you pick a facility?

Cascading: Facility Changed → GET → Unit dropdown

[Code snippet from the Facility .Reactive block]
[Explain: Gather Include sends selected facility, response populates child]

## How do conditional sections work?

Two conditions — checkbox and dropdown

[Code snippets for both When/Then/Else blocks]
[Explain: Truthy for boolean, Eq for string comparison]

## What about validation?

Client-side extraction + server-side enforcement

<Tabs>
  <TabItem label="IntakeValidator.cs">[validator code]</TabItem>
  <TabItem label="View (validation wiring)">[Validate + OnError snippet]</TabItem>
</Tabs>

[Explain: WhenField conditional rules, ValidationErrors routing]

## The save flow

Confirm → POST → Toast → Chained confirmation number

[Code snippet from the Submit button .Reactive block]
[Explain: Confirm guard, WhileLoading, OnSuccess/OnError, Chained]

## The summary drawer

Dispatch → CustomEvent → GET → Into → Open

[Code snippets for Dispatch + CustomEvent handler]
[Explain: decoupled events, Into injects HTML partial, drawer opens]

## The Controller

<Tabs>
  <TabItem label="IntakeController.cs">[controller code]</TabItem>
  <TabItem label="_Summary.cshtml">[partial code]</TabItem>
</Tabs>

## The rendered plan

[Collapsible JSON output showing the complete plan]
[Brief note: this JSON is the ONLY thing the browser receives]

## Download & Run

:::note[Prerequisites]
- .NET 10 SDK
- Syncfusion EJ2 license key (trial available)
:::

[Download resident-intake.zip](/downloads/resident-intake.zip)

\```bash
unzip resident-intake.zip
cd resident-intake
dotnet run
# Open http://localhost:5000
\```
```

- [ ] **Step 2: Add Examples section to sidebar**

In `docs-site/astro.config.mjs`, add a new sidebar section:

```javascript
{
  label: 'Examples',
  items: [
    { label: 'Resident Intake Form', slug: 'examples/resident-intake' },
  ],
},
```

Place it after "Getting Started" and before "Features" in the sidebar array.

---

## Task 10: Package & Final Verification

**Files:**
- Create: `docs-site/public/downloads/resident-intake.zip`

- [ ] **Step 1: Create the ZIP**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/examples
zip -r ../docs-site/public/downloads/resident-intake.zip resident-intake/ \
    -x "resident-intake/bin/*" \
    -x "resident-intake/obj/*"
```

- [ ] **Step 2: Build the docs site**

```bash
cd /Users/muhammadadnanrafiq/Documents/alis-reactive-framework-1-0/Alis.Reactive/docs-site
npm run build
```

Expected: Build succeeds, ZIP is served at `/downloads/resident-intake.zip`.

- [ ] **Step 3: Preview and verify**

```bash
npm run preview
```

Navigate to the Examples → Resident Intake Form page. Verify:
- All code tabs render correctly
- Download link works
- Page reads naturally with question → answer flow

---

## Feature Coverage Checklist

After implementation, verify each feature works:

- [ ] DomReady trigger
- [ ] Parallel HTTP (two GET requests)
- [ ] FusionDropDownList (3 instances)
- [ ] SetDataSource + DataBind
- [ ] Cascading dropdowns (Facility → Units)
- [ ] Gather Include (specific component)
- [ ] NativeTextBox (5 instances)
- [ ] NativeCheckBox (1 instance)
- [ ] FusionDatePicker (3 instances)
- [ ] FusionNumericTextBox (1 instance)
- [ ] Condition — When/Eq (dropdown value)
- [ ] Condition — When/Truthy (checkbox)
- [ ] Condition — WhenField in validator (2 conditional rules)
- [ ] Element Show/Hide
- [ ] HTTP POST with IncludeAll
- [ ] Validate + ValidationErrors (400 routing)
- [ ] FusionConfirm (confirmation dialog)
- [ ] WhileLoading + NativeLoader
- [ ] FusionToast (success notification)
- [ ] Chained HTTP (save → get confirmation #)
- [ ] Element SetText (confirmation number from response)
- [ ] Dispatch + CustomEvent
- [ ] NativeDrawer (open, set size, set title)
- [ ] Into (load partial HTML)
