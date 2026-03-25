# AdmissionAssessment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a 4-step admission assessment form that exercises every condition pattern with real input components, HTTP alerts, partials, validation, and wizard navigation.

**Architecture:** 4-step wizard (Show/Hide per step), 6 same-model partials (ResolvePlan + RenderPlan merging), 12 server endpoints with intentional delays, 3-layer validation (conditions DSL → client FluentValidation → server), NativeLoader for all HTTP calls.

**Tech Stack:** C# DSL (Alis.Reactive), Syncfusion EJ2 components, FluentValidation, ASP.NET MVC partials, Playwright + NUnit

**Spec:** `docs/superpowers/specs/2026-03-25-admission-assessment.md`

---

## Complete .Reactive Wiring Map

Every field → event → condition → action, in one table:

| Field | Component | .Reactive Event | Condition | Action in Then | Action in Else | Before cmd | After cmd |
|-------|-----------|----------------|-----------|----------------|----------------|------------|-----------|
| Age | FusionNumericTextBox | evt.Changed | Gte(85m) | SetValue(RiskTier, "High") | — | SetText(age-echo) | SetText("assessed") |
| Age | — | — | ElseIf Gte(65m) | SetValue(RiskTier, "Standard") | SetValue(RiskTier, "Low") | — | — |
| PrimaryDiagnosis | FusionDropDownList | evt.Changed | In("Alzheimer's","Parkinson's") | Show("cognitive-section") | Hide("cognitive-section") | — | — |
| PrimaryDiagnosis | — | — | Eq("Heart Disease") | Show("cardiac-section") | Hide("cardiac-section") | — | — |
| PrimaryDiagnosis | — | — | Eq("Diabetes") | Show("diabetes-section") | Hide("diabetes-section") | — | — |
| AttendingPhysician | FusionAutoComplete | evt.Filtering | (no condition — always filters) | PreventDefault + GET /SearchPhysicians + UpdateData | — | — | — |
| IsVeteran | FusionSwitch | evt.Changed | Truthy() | Show("veteran-section") | Hide("veteran-section") | — | — |
| CognitiveScore | FusionNumericTextBox | evt.Changed | Lt(15m) | SetValue(CareUnit, "Memory Care") | — | SetText(score-echo) | SetText("scored") |
| CognitiveScore | — | — | ElseIf Lt(25m) | SetValue(CareUnit, "Assisted + Memory") | SetValue(CareUnit, "Standard") | — | — |
| Wanders | FusionSwitch | evt.Changed | Truthy() | Show("wander-details") | Hide("wander-details") | — | — |
| WanderFrequency | FusionDropDownList | evt.Changed | Eq("Frequently") | POST /AlertElopement + WhileLoading | SetText("no alert") | — | — |
| SystolicBP | FusionNumericTextBox | evt.Changed | Gt(140m) | POST /AlertHypertension + WhileLoading | SetText("Normal") | SetText(bp-echo) | — |
| HasPacemaker | FusionSwitch | evt.Changed | Truthy() | Show("pacemaker-details") | Hide("pacemaker-details") | — | — |
| A1cLevel | FusionNumericTextBox | evt.Changed | Gt(9m) | POST /AlertUncontrolled + WhileLoading | SetText("Controlled") | SetText(a1c-echo) | — |
| InsulinDependent | FusionSwitch | evt.Changed | Truthy() | Show("insulin-details") | Hide("insulin-details") | — | — |
| FallHistory | FusionDropDownList | evt.Changed | In("1-2 falls","3+ falls") | Show("fall-details") | Hide("fall-details") | — | — |
| CausedInjury | FusionSwitch | evt.Changed | Truthy() | Show("injury-details") | Hide("injury-details") | — | — |
| InjuryType | FusionDropDownList | evt.Changed | Eq("Head Injury") | POST /AlertNeuro + WhileLoading | SetText("noted") | — | — |
| MobilityAid | FusionDropDownList | evt.Changed | Eq("Wheelchair") | POST /RequestRoomSetup + WhileLoading + Show("escort") | — | — | — |
| MobilityAid | — | — | In("Walker","Wheelchair") | Show("escort-indicator") | Hide("escort-indicator") | — | — |
| MedicationCount | FusionNumericTextBox | evt.Changed | Gt(10m) | Show("polypharmacy-warning") | Hide("polypharmacy-warning") | — | — |
| TakesBloodThinners | FusionSwitch | evt.Changed | Truthy() | SetValue(MonitoringLevel, "Enhanced") | — | — | — |
| TakesPainMedication | FusionSwitch | evt.Changed | Truthy() | Show("pain-section") | Hide("pain-section") | — | — |
| PainLevel | FusionNumericTextBox | evt.Changed | Gt(7m) | POST /AlertPain + WhileLoading + Show("location-required") | Hide("location-required") | SetText(pain-echo) | — |
| EmergencyContact | NativeTextBox | evt.Changed | IsEmpty() | Show("contact-warning") | Hide("contact-warning") | — | — |
| PhysicianName (display) | (read from AutoComplete) | (on AutoComplete select) | Contains("Dr.") | Show("verified-badge") | Hide("verified-badge") | — | — |

**Button-triggered pipelines:**

| Button | .Reactive Event | Pipeline |
|--------|----------------|----------|
| Verify VA | evt.Click | POST /VerifyVeteran (Include VaId, ServiceBranch) + WhileLoading + OnSuccess(SetText) |
| Save Cognitive | evt.Click | POST /SaveCognitive (Include CognitiveScore, Wanders, WanderFrequency) + WhileLoading + OnSuccess(HiddenField.SetValue(id)) |
| Save Cardiac | evt.Click | POST /SaveCardiac (Include SystolicBP, HasPacemaker, PacemakerModel, LastDeviceCheck) + WhileLoading + OnSuccess(HiddenField.SetValue(id)) |
| Save Diabetes | evt.Click | POST /SaveDiabetes (Include DiabetesType, A1cLevel, InsulinDependent, InsulinSchedule) + WhileLoading + OnSuccess(HiddenField.SetValue(id)) |
| FallRisk Check | evt.Click | When(fallRisk.Value()).Gte(7m).And(age.Value()).Gte(80m).Then(SetValue(Monitoring,"Continuous")).Else(SetValue(Monitoring,"Standard")) |
| Step Next (×3) | evt.Click | Element(current-step).Hide() + Element(next-step).Show() + AddClass/RemoveClass on indicators |
| Step Prev (×3) | evt.Click | Element(current-step).Hide() + Element(prev-step).Show() + AddClass/RemoveClass on indicators |
| Submit | evt.Click | Validate\<AdmissionAssessmentValidator\>("screening-form") + POST /Submit (IncludeAll) + WhileLoading(NativeLoader) + OnSuccess(SetText) + OnError(400, ValidationErrors) |

---

## Task 1: Model + DTOs + Validator

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Conditions/AdmissionAssessment/HealthScreeningModel.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Conditions/AdmissionAssessment/ResponseDtos.cs`
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Conditions/AdmissionAssessment/AdmissionAssessmentValidator.cs`

- [ ] **Step 1: Create HealthScreeningModel**

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class HealthScreeningModel
{
    // Section A: Demographics
    public string ResidentName { get; set; } = "";
    public decimal Age { get; set; }
    public string PrimaryDiagnosis { get; set; } = "";
    public string AttendingPhysician { get; set; } = "";
    public bool IsVeteran { get; set; }
    public string VaId { get; set; } = "";
    public string ServiceBranch { get; set; } = "";

    // Section B1: Cognitive
    public decimal CognitiveScore { get; set; }
    public bool Wanders { get; set; }
    public string WanderFrequency { get; set; } = "";

    // Section B2: Cardiac
    public bool HasPacemaker { get; set; }
    public string PacemakerModel { get; set; } = "";
    public DateTime? LastDeviceCheck { get; set; }
    public decimal SystolicBP { get; set; }

    // Section B3: Diabetes
    public string DiabetesType { get; set; } = "";
    public decimal A1cLevel { get; set; }
    public bool InsulinDependent { get; set; }
    public string InsulinSchedule { get; set; } = "";

    // Section C: Mobility & Falls
    public string FallHistory { get; set; } = "";
    public bool CausedInjury { get; set; }
    public string InjuryType { get; set; } = "";
    public decimal FallRiskScore { get; set; }
    public string MobilityAid { get; set; } = "";

    // Section D: Medications
    public decimal MedicationCount { get; set; }
    public bool TakesBloodThinners { get; set; }
    public bool TakesPainMedication { get; set; }
    public decimal PainLevel { get; set; }
    public string PainLocation { get; set; } = "";

    // Section E: Contacts
    public string EmergencyContact { get; set; } = "";

    // Auto-populated (hidden fields)
    public string RiskTier { get; set; } = "";
    public string CareUnit { get; set; } = "";
    public string MonitoringLevel { get; set; } = "";
    public string CognitiveAssessmentId { get; set; } = "";
    public string CardiacAssessmentId { get; set; } = "";
    public string DiabetesAssessmentId { get; set; } = "";
}
```

- [ ] **Step 2: Create ResponseDtos**

```csharp
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class AlertResponse { public string Message { get; set; } = ""; public string Urgency { get; set; } = ""; }
public class SaveSectionResponse { public string Id { get; set; } = ""; public string Message { get; set; } = ""; }
public class VerifyVaResponse { public string Message { get; set; } = ""; public bool Eligible { get; set; } }
public class SubmitResponse { public string ScreeningId { get; set; } = ""; public string CareUnit { get; set; } = ""; public string MonitoringLevel { get; set; } = ""; public string Message { get; set; } = ""; public List<string> Alerts { get; set; } = new(); }
public class PhysicianSearchResponse { public List<PhysicianItem> Physicians { get; set; } = new(); }
public class PhysicianItem { public string Text { get; set; } = ""; public string Value { get; set; } = ""; }
```

- [ ] **Step 3: Create AdmissionAssessmentValidator**

Use `ReactiveValidator<HealthScreeningModel>` for client-extractable conditions.
Server-side catches PainLocation when PainLevel > 7 (numeric threshold not in WhenField).

```csharp
using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class AdmissionAssessmentValidator : ReactiveValidator<HealthScreeningModel>
{
    public AdmissionAssessmentValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0m);
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        RuleFor(x => x.EmergencyContact).NotEmpty();

        WhenField(x => x.IsVeteran, () => { RuleFor(x => x.VaId).NotEmpty(); });

        WhenField(x => x.PrimaryDiagnosis, "Alzheimer's", () => { RuleFor(x => x.CognitiveScore).GreaterThan(0m); });
        WhenField(x => x.PrimaryDiagnosis, "Parkinson's", () => { RuleFor(x => x.CognitiveScore).GreaterThan(0m); });
        WhenField(x => x.Wanders, () => { RuleFor(x => x.WanderFrequency).NotEmpty(); });

        WhenField(x => x.PrimaryDiagnosis, "Heart Disease", () => { RuleFor(x => x.SystolicBP).GreaterThan(0m); });
        WhenField(x => x.HasPacemaker, () => { RuleFor(x => x.PacemakerModel).NotEmpty(); });

        WhenField(x => x.PrimaryDiagnosis, "Diabetes", () => {
            RuleFor(x => x.DiabetesType).NotEmpty();
            RuleFor(x => x.A1cLevel).GreaterThan(0m);
        });
        WhenField(x => x.InsulinDependent, () => { RuleFor(x => x.InsulinSchedule).NotEmpty(); });

        WhenField(x => x.CausedInjury, () => { RuleFor(x => x.InjuryType).NotEmpty(); });
        WhenField(x => x.TakesPainMedication, () => { RuleFor(x => x.PainLevel).GreaterThan(0m); });
    }
}
```

- [ ] **Step 4: Build and verify compilation**

```bash
dotnet build Alis.Reactive.SandboxApp --verbosity quiet
```

- [ ] **Step 5: Commit**

```bash
git add Models/Conditions/AdmissionAssessment/
git commit -m "feat(AdmissionAssessment): model, DTOs, validator — Task 1"
```

---

## Task 2: Controller with All Endpoints

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Conditions/AdmissionAssessmentController.cs`

- [ ] **Step 1: Create controller with all 12 endpoints**

Each endpoint has `Task.Delay()` for realistic loading. Group:
- GET Index (returns view + seed data)
- GET SearchPhysicians (autocomplete filter)
- POST VerifyVeteran
- POST AlertElopement, AlertHypertension, AlertUncontrolled, AlertNeuro, AlertPain, RequestRoomSetup
- POST SaveCognitive, SaveCardiac, SaveDiabetes
- POST Submit (full validation + care plan computation)

The Submit endpoint validates ALL conditionally-required fields server-side, including
PainLocation when PainLevel > 7 (the one gap in client-side validation).

Seed data in ViewBag:
- Diagnoses, FallOptions, MobilityAids, WanderFreqs, InsulinSchedules, InjuryTypes, DiabetesTypes, ServiceBranches

- [ ] **Step 2: Build and verify**

```bash
dotnet build Alis.Reactive.SandboxApp --verbosity quiet
```

- [ ] **Step 3: Commit**

```bash
git add Controllers/Conditions/AdmissionAssessmentController.cs
git commit -m "feat(AdmissionAssessment): controller with 12 endpoints — Task 2"
```

---

## Task 3: Main Index.cshtml — Step Wizard Shell

**Files:**
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/AdmissionAssessment/Index.cshtml`

- [ ] **Step 1: Create Index with step wizard shell**

The parent view:
1. Creates `Html.ReactivePlan<HealthScreeningModel>()`
2. Renders step indicator bar (4 dots with labels)
3. Renders 4 step containers (`<div id="step-1">` through `<div id="step-4">`)
4. Step 2, 3, 4 start `hidden`
5. Wires Next/Previous buttons — each is a NativeButton.Reactive(Click) that:
   - Hides current step, shows next/prev step
   - Updates step indicator classes
6. Renders NativeHiddenField components for assessment IDs
7. Calls `@Html.RenderPlan(plan)` at the bottom (ONCE — partials use ResolvePlan)

Step 1 contains:
- Demographics fields inline (ResidentName, Age, PrimaryDiagnosis, AttendingPhysician)
- `@await Html.PartialAsync("_VeteranDetails", Model)` inside veteran-section div
- IsVeteran switch wired to Show/Hide veteran-section
- Age wired to ElseIf ladder → SetValue on RiskTier
- PrimaryDiagnosis wired to Show/Hide cognitive/cardiac/diabetes sections
- AttendingPhysician wired as AutoComplete with Filtering event

Step 2 contains:
- `@await Html.PartialAsync("_CognitiveAssessment", Model)` inside cognitive-section div (hidden)
- `@await Html.PartialAsync("_CardiacAssessment", Model)` inside cardiac-section div (hidden)
- `@await Html.PartialAsync("_DiabetesManagement", Model)` inside diabetes-section div (hidden)
- "No specialized assessment" text for Stroke/Other

Step 3 contains:
- Mobility & Falls fields inline
- `@await Html.PartialAsync("_FallDetails", Model)` inside fall-details div (hidden)
- `@await Html.PartialAsync("_PainAssessment", Model)` inside pain-section div (hidden)
- MedicationCount, BloodThinners, TakesPainMedication switches

Step 4 contains:
- EmergencyContact field
- Summary panel (read-only spans for RiskTier, CareUnit, MonitoringLevel)
- Submit button with Validate + POST /Submit + WhileLoading(NativeLoader)

- [ ] **Step 2: Build and verify**
- [ ] **Step 3: Commit**

---

## Task 4: _VeteranDetails Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_VeteranDetails.cshtml`

- [ ] **Step 1: Create partial**

```csharp
@model HealthScreeningModel
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@using Alis.Reactive.Fusion.Components
@{
    var plan = Html.ResolvePlan<HealthScreeningModel>();
}
// VaId (NativeTextBox), ServiceBranch (FusionDropDownList)
// Verify VA button → POST /VerifyVeteran + WhileLoading + OnSuccess(SetText)
@Html.RenderPlan(plan)
```

- [ ] **Step 2: Build, commit**

---

## Task 5: _CognitiveAssessment Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_CognitiveAssessment.cshtml`

- [ ] **Step 1: Create partial**

Fields: CognitiveScore (FusionNumericTextBox), Wanders (FusionSwitch), WanderFrequency (FusionDropDownList)

Wiring:
- CognitiveScore.Reactive(Changed) →
  - Before: SetText(score-echo)
  - When(comp.Value()).Lt(15m).Then(SetValue(CareUnit,"Memory Care")).ElseIf(...).Lt(25m).Then(SetValue(CareUnit,"Assisted + Memory")).Else(SetValue(CareUnit,"Standard"))
  - After: SetText("scored")
- Wanders.Reactive(Changed) → When(comp.Value()).Truthy().Then(Show("wander-details")).Else(Hide)
- WanderFrequency.Reactive(Changed) → When(comp.Value()).Eq("Frequently").Then(POST /AlertElopement + WhileLoading).Else(SetText("no alert"))
- Save Cognitive button → POST /SaveCognitive + WhileLoading + OnSuccess(HiddenField.SetValue(id))

- [ ] **Step 2: Build, commit**

---

## Task 6: _CardiacAssessment Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_CardiacAssessment.cshtml`

- [ ] **Step 1: Create partial**

Fields: SystolicBP (FusionNumericTextBox), HasPacemaker (FusionSwitch), PacemakerModel (NativeTextBox), LastDeviceCheck (FusionDatePicker)

Wiring:
- SystolicBP.Reactive(Changed) → Before: SetText(bp-echo) → When(comp.Value()).Gt(140m).Then(POST /AlertHypertension + WhileLoading).Else(SetText("Normal"))
- HasPacemaker.Reactive(Changed) → When(comp.Value()).Truthy().Then(Show("pacemaker-details")).Else(Hide)
- Save Cardiac button → POST /SaveCardiac + WhileLoading + OnSuccess(HiddenField.SetValue(id))

- [ ] **Step 2: Build, commit**

---

## Task 7: _DiabetesManagement Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_DiabetesManagement.cshtml`

- [ ] **Step 1: Create partial**

Fields: DiabetesType (FusionDropDownList), A1cLevel (FusionNumericTextBox), InsulinDependent (FusionSwitch), InsulinSchedule (FusionDropDownList)

Wiring:
- A1cLevel.Reactive(Changed) → Before: SetText(a1c-echo) → When(comp.Value()).Gt(9m).Then(POST /AlertUncontrolled + WhileLoading).Else(SetText("Controlled"))
- InsulinDependent.Reactive(Changed) → When(comp.Value()).Truthy().Then(Show("insulin-details")).Else(Hide)
- Save Diabetes button → POST /SaveDiabetes + WhileLoading + OnSuccess(HiddenField.SetValue(id))

- [ ] **Step 2: Build, commit**

---

## Task 8: _FallDetails Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_FallDetails.cshtml`

- [ ] **Step 1: Create partial**

Fields: CausedInjury (FusionSwitch), InjuryType (FusionDropDownList)

Wiring:
- CausedInjury.Reactive(Changed) → When(comp.Value()).Truthy().Then(Show("injury-details")).Else(Hide)
- InjuryType.Reactive(Changed) → When(comp.Value()).Eq("Head Injury").Then(POST /AlertNeuro + WhileLoading).Else(SetText("noted"))

- [ ] **Step 2: Build, commit**

---

## Task 9: _PainAssessment Partial

**Files:**
- Create: `Views/Conditions/AdmissionAssessment/_PainAssessment.cshtml`

- [ ] **Step 1: Create partial**

Fields: PainLevel (FusionNumericTextBox), PainLocation (NativeTextBox)

Wiring:
- PainLevel.Reactive(Changed) → Before: SetText(pain-echo) → When(comp.Value()).Gt(7m).Then(POST /AlertPain + WhileLoading + Show("location-required")).Else(Hide("location-required"))

- [ ] **Step 2: Build, commit**

---

## Task 10: Wire Remaining Index Conditions

**Files:**
- Modify: `Views/Conditions/AdmissionAssessment/Index.cshtml`

- [ ] **Step 1: Wire Step 3 conditions in Index**

Inline fields in Step 3 (not in partials):
- FallHistory.Reactive(Changed) → When(comp.Value()).In("1-2 falls","3+ falls").Then(Show("fall-details")).Else(Hide)
- FallRiskScore + Age compound: button.Reactive(Click) → When(fallRisk.Value()).Gte(7m).And(age.Value()).Gte(80m).Then(SetValue(Monitoring,"Continuous")).Else(SetValue(Monitoring,"Standard"))
- MobilityAid.Reactive(Changed) →
  - When(comp.Value()).In("Walker","Wheelchair").Then(Show("escort-indicator")).Else(Hide)
  - When(comp.Value()).Eq("Wheelchair").Then(POST /RequestRoomSetup + WhileLoading)
- MedicationCount.Reactive(Changed) → When(comp.Value()).Gt(10m).Then(Show("polypharmacy-warning")).Else(Hide)
- TakesBloodThinners.Reactive(Changed) → When(comp.Value()).Truthy().Then(SetValue(MonitoringLevel,"Enhanced"))
- TakesPainMedication.Reactive(Changed) → When(comp.Value()).Truthy().Then(Show("pain-section")).Else(Hide)

- [ ] **Step 2: Wire Step 4 in Index**

- EmergencyContact.Reactive(Changed) → When(comp.Value()).IsEmpty().Then(Show("contact-warning")).Else(Hide)
- Submit button → Validate + POST /Submit + WhileLoading(NativeLoader) + OnSuccess + OnError(400, ValidationErrors)

- [ ] **Step 3: Update Conditions index page with card**
- [ ] **Step 4: Full build: `npm run build:all && dotnet build`**
- [ ] **Step 5: Commit**

---

## Task 11: Manual Browser Verification

- [ ] **Step 1: Restart app**

```bash
pkill -f "dotnet.*SandboxApp"; sleep 1
dotnet run --project Alis.Reactive.SandboxApp &
```

- [ ] **Step 2: Navigate to http://localhost:5220/Sandbox/Conditions/AdmissionAssessment**

Verify manually in browser — walk through each step:
1. Step 1: Enter age → risk badge. Select diagnosis → section appears. Toggle veteran → details show. Type physician → autocomplete searches.
2. Step 2: Fill cognitive/cardiac/diabetes fields. Conditions fire. Save sections. Hidden fields populated.
3. Step 3: Fall history → details. Mobility → escort. Medications → polypharmacy. Pain → alert.
4. Step 4: Emergency contact → warning. Submit → validation or success.

- [ ] **Step 3: Record browser session (Playwright video or GIF)**
- [ ] **Step 4: Note any issues for test adjustments**

---

## Task 12: Playwright Tests (Commented Asserts + Warnings)

**Files:**
- Create: `tests/Alis.Reactive.PlaywrightTests/Conditions/WithComponents/WhenAdmissionAssessmentFilled.cs`

- [ ] **Step 1: Create test file with warning approach**

Tests use `TestContext.Out.WriteLine("WARNING: ...")` for manual verification items.
Asserts that are verified in browser are active. Asserts that need manual confirmation
are commented with `// MANUAL: uncomment after browser verification`.

Test groups:
1. **Step navigation** (3 tests): next/prev/step indicators
2. **Alzheimer's happy path** (1 test): full flow through cognitive → save → submit
3. **Heart Disease happy path** (1 test): cardiac → save → submit
4. **Diabetes happy path** (1 test): diabetes → save → submit
5. **Conditional visibility** (6 tests): each toggle/dropdown shows/hides correctly
6. **HTTP in branches** (4 tests): alerts fire at correct thresholds
7. **Component mutations** (3 tests): SetValue on auto-populated fields
8. **Validation failures** (3 tests): missing required fields → 400
9. **Compound condition** (1 test): FallRisk + Age → Continuous

- [ ] **Step 2: Build and run tests**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "WhenAdmissionAssessmentFilled" --verbosity quiet
```

- [ ] **Step 3: Fix any failures, re-run**
- [ ] **Step 4: Update coverage matrix**
- [ ] **Step 5: Commit and push**

```bash
git add .
git commit -m "feat(AdmissionAssessment): flagship conditions vertical slice — complete"
git push
```

---

## Task Summary

| Task | Files | What it produces |
|------|-------|-----------------|
| 1 | Model + DTOs + Validator | Data layer — compiles independently |
| 2 | Controller (12 endpoints) | Server layer — endpoints return correct responses |
| 3 | Index.cshtml (wizard shell) | Page structure — steps navigate, partials render |
| 4 | _VeteranDetails | Veteran section with VA verify |
| 5 | _CognitiveAssessment | Cognitive with 3-level nesting + section save |
| 6 | _CardiacAssessment | Cardiac with pacemaker details + section save |
| 7 | _DiabetesManagement | Diabetes with insulin details + section save |
| 8 | _FallDetails | Falls with injury details |
| 9 | _PainAssessment | Pain with threshold alert |
| 10 | Wire remaining conditions | Step 3 + Step 4 inline conditions + submit |
| 11 | Manual browser verification | Record session, note issues |
| 12 | Playwright tests | 22+ tests with warning approach |

**Total: ~15 files, 33 fields, 8 component types, 17 conditions, 12 HTTP endpoints, 6 partials**
