# AdmissionAssessment Wizard Refactor — Next Session

## How to Start

1. Open a new Claude Code session
2. Copy everything below the `---` line
3. Paste as your first message

---

Continue work on Alis.Reactive feature/coerce-first-class branch.
Worktree: .worktrees/coerce-first-class

Task: Refactor AdmissionAssessment from single flat model to per-step models with draft persistence.

Read these files BEFORE doing anything:
- docs/superpowers/specs/2026-03-25-admission-assessment.md (original spec)
- docs/superpowers/specs/2026-03-25-admission-wizard-refactor-prompt.md (this file — rules + asserts)
- docs/test-coverage/conditions-with-components.md (coverage matrix)
- Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/AdmissionAssessment/Index.cshtml (current view)
- tests/Alis.Reactive.PlaywrightTests/Conditions/WithComponents/WhenAdmissionAssessmentFilled.cs (26 passing tests)

Also study HOW EDIT SCENARIOS WORK in existing sandbox pages:
- Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml
- Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/DropDownList/Index.cshtml
- Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Switch/Index.cshtml

These pages pass model values → SF components render with those values already set.
The SAME pattern applies here: each step's model carries the saved draft state,
SF components bind to it, user sees their previously entered values.

## RULES — Read Before Writing Any Code

1. **NO FRAMEWORK CHANGES.** Zero changes to Alis.Reactive, Alis.Reactive.Native,
   Alis.Reactive.Fusion, or Alis.Reactive.FluentValidator projects. Use only existing primitives.

2. **EACH STEP = OWN MODEL + OWN VALIDATOR.** No shared model. No monolithic validator.
   Step1DemographicsModel has Step1Validator. Step2ClinicalModel has Step2Validator. Etc.

3. **EACH VALIDATOR = ReactiveValidator<StepModel>.** Uses WhenField for client-side
   conditional rules. Validators only reference fields on their own model — never cross-step.

4. **DRAFT PERSISTENCE = IN-MEMORY.** Use a static ConcurrentDictionary keyed by screeningId.
   Each step save stores the step model. Step load reads it back. No database.

5. **EDIT SCENARIO = MODEL BINDING.** When the controller loads a step, it passes the saved
   draft as the model. ASP.NET MVC renders the view with model values. SF components pick up
   the values automatically via property expressions. DataSource items come from ViewBag.
   The user sees their saved data. No custom JS hydration.

6. **NAVIGATION = SAVE THEN LOAD.** Next button: POST save current step → OnSuccess → navigate
   to next step (which loads from server with saved state). Previous: just load previous step
   from server (already saved).

7. **CONDITIONS STILL FIRE ON LOADED STATE.** When a step loads with saved data, the conditions
   DSL doesn't auto-fire (it fires on CHANGE events, not on initial load). For conditional
   sections that should be visible based on saved state, the controller must set initial
   visibility in the model or the DomReady handler must evaluate conditions.

8. **ALL 26 EXISTING TESTS MUST PASS.** Run after each task. If a test breaks, fix before continuing.
   Never skip a broken test.

9. **PLAN FIRST, CODE SECOND.** Write the implementation plan. Get approval. Then execute task by task.

10. **ONE TASK AT A TIME.** Build → test → commit → next task. Never batch multiple tasks.

## The Problem (Why Refactor)

Current: One HealthScreeningModel with one AdmissionAssessmentValidator across 4 wizard steps.
- Submit fires ALL WhenField rules including rules for hidden steps → phantom validation errors
- No draft persistence → refresh loses everything
- Next/Previous doesn't reload saved state → user re-enters data

## Per-Step Models

```
Step1DemographicsModel:
  ResidentName (string, required)
  Age (decimal, required > 0)
  PrimaryDiagnosis (string, required)
  AttendingPhysician (string)
  IsVeteran (bool)
  VaId (string, required when IsVeteran)
  ServiceBranch (string)
  RiskTier (string, auto-set by Age condition)

Step2ClinicalModel:
  PrimaryDiagnosis (string, read-only — copied from Step 1 draft)
  CognitiveScore (decimal, required when Alzheimer's/Parkinson's)
  Wanders (bool)
  WanderFrequency (string, required when Wanders)
  HasPacemaker (bool)
  PacemakerModel (string, required when HasPacemaker)
  LastDeviceCheck (DateTime?)
  SystolicBP (decimal, required when Heart Disease)
  DiabetesType (string, required when Diabetes)
  A1cLevel (decimal, required when Diabetes)
  InsulinDependent (bool)
  InsulinSchedule (string, required when InsulinDependent)
  CareUnit (string, auto-set by CognitiveScore condition)
  CognitiveAssessmentId (string, set by section save response)
  CardiacAssessmentId (string)
  DiabetesAssessmentId (string)

Step3FunctionalModel:
  Age (decimal, read-only — copied from Step 1 draft, needed for compound condition)
  FallHistory (string)
  CausedInjury (bool)
  InjuryType (string, required when CausedInjury)
  FallRiskScore (decimal)
  MobilityAid (string)
  MedicationCount (decimal)
  TakesBloodThinners (bool)
  TakesPainMedication (bool)
  PainLevel (decimal, required when TakesPainMedication)
  PainLocation (string, required server-side when PainLevel > 7)
  MonitoringLevel (string, auto-set)

Step4ReviewModel:
  EmergencyContact (string, required)
  ScreeningId (string, set by server on successful submit)
  — Plus read-only summary fields loaded from all step drafts:
  RiskTier, CareUnit, MonitoringLevel, AlertsSent (List<string>)
  Step1Saved, Step2Saved, Step3Saved (bool — for status display)
```

## Per-Step Validators

```csharp
public class Step1Validator : ReactiveValidator<Step1DemographicsModel>
{
    public Step1Validator()
    {
        RuleFor(x => x.ResidentName).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0m);
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        WhenField(x => x.IsVeteran, () => { RuleFor(x => x.VaId).NotEmpty(); });
    }
}

public class Step2Validator : ReactiveValidator<Step2ClinicalModel>
{
    public Step2Validator()
    {
        // These fire ONLY when Step 2 is submitted — never on Step 1 or 3
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
    }
}

public class Step3Validator : ReactiveValidator<Step3FunctionalModel>
{
    public Step3Validator()
    {
        WhenField(x => x.CausedInjury, () => { RuleFor(x => x.InjuryType).NotEmpty(); });
        WhenField(x => x.TakesPainMedication, () => { RuleFor(x => x.PainLevel).GreaterThan(0m); });
        // PainLocation when PainLevel > 7 → server-side only (WhenField doesn't support numeric thresholds)
    }
}

public class Step4Validator : ReactiveValidator<Step4ReviewModel>
{
    public Step4Validator()
    {
        RuleFor(x => x.EmergencyContact).NotEmpty();
    }
}
```

## Controller Endpoints

```
GET  /Index                      → main page shell (step indicator + empty containers)
GET  /Step1?screeningId=         → Step1 partial (fresh or from draft)
POST /SaveStep1                  → validate Step1Validator → store draft → return {screeningId}
GET  /Step2?screeningId=X        → Step2 partial (from draft, PrimaryDiagnosis pre-set)
POST /SaveStep2                  → validate Step2Validator → store draft
GET  /Step3?screeningId=X        → Step3 partial (from draft, Age pre-set for compound)
POST /SaveStep3                  → validate Step3Validator → store draft
GET  /Step4?screeningId=X        → Step4 partial (read-only summary from all drafts)
POST /Submit                     → check all drafts saved → return care plan or errors

Existing (unchanged):
GET  /SearchPhysicians?q=        → autocomplete filter
POST /VerifyVeteran              → VA check
POST /AlertElopement, AlertHypertension, AlertUncontrolled, AlertNeuro, AlertPain
POST /RequestRoomSetup
POST /SaveCognitive, SaveCardiac, SaveDiabetes  (section saves within Step 2)
```

## Assert Criteria — What Each Test Must Verify

### Existing 26 tests (must still pass):

Group 1 — Step Navigation:
  ASSERT: step-1 visible on load, step-2/3/4 hidden
  ASSERT: click Next → current step hides, next step shows
  ASSERT: click Previous → current step hides, previous step shows

Group 2 — Age → Risk Tier:
  ASSERT: enter 85 → risk badge contains "High"
  ASSERT: enter 70 → risk badge contains "Standard"
  ASSERT: enter 50 → risk badge contains "Low"

Group 3 — Diagnosis → Section Visibility:
  ASSERT: select "Alzheimer's" → cognitive-section visible, cardiac/diabetes hidden
  ASSERT: select "Heart Disease" → cardiac-section visible, cognitive/diabetes hidden
  ASSERT: select "Diabetes" → diabetes-section visible, cognitive/cardiac hidden

Group 4 — Veteran Toggle:
  ASSERT: toggle on → veteran-section visible
  ASSERT: toggle off → veteran-section hidden

Group 5 — Cognitive Assessment:
  ASSERT: score 12 → cognitive-status contains "Memory Care"
  ASSERT: score 20 → cognitive-status contains "Assisted"
  ASSERT: wanders on → wander-details visible
  ASSERT: frequency "Frequently" → elopement-result shows alert message

Group 6 — Cardiac Assessment:
  ASSERT: BP 160 → hypertension-result shows alert message
  ASSERT: BP 120 → hypertension-result shows "Normal"
  ASSERT: pacemaker on → pacemaker-details visible

Group 7 — Diabetes Assessment:
  ASSERT: A1C 10.5 → uncontrolled-result shows alert message
  ASSERT: A1C 7 → uncontrolled-result shows "Controlled"
  ASSERT: insulin on → insulin-details visible

Group 8 — Falls & Mobility:
  ASSERT: "3+ falls" → fall-details visible
  ASSERT: "Wheelchair" → escort-indicator visible + room-result shows message
  ASSERT: fallRisk 8 + age 82 → fallrisk-result contains "Continuous"

Group 9 — Medications:
  ASSERT: medCount 14 → polypharmacy-warning visible
  ASSERT: painMed on + pain 9 → pain-result shows alert + location-required visible

### NEW tests to add (10 tests):

Test: save_step1_and_reload_preserves_resident_name
  ACTION: fill name "Margaret Johnson", age 82, diagnosis "Alzheimer's"
  ACTION: click Save Step 1
  ASSERT: server returns 200 with screeningId
  ACTION: navigate away and back to Step 1
  ASSERT: ResidentName field shows "Margaret Johnson"
  ASSERT: Age field shows 82
  ASSERT: Diagnosis dropdown shows "Alzheimer's"

Test: save_step2_cognitive_and_reload_preserves_score
  ACTION: complete Step 1 with Alzheimer's, save, go to Step 2
  ACTION: fill CognitiveScore 12, toggle Wanders on, select "Frequently"
  ACTION: click Save Step 2
  ASSERT: server returns 200
  ACTION: navigate away and back to Step 2
  ASSERT: CognitiveScore shows 12
  ASSERT: Wanders switch is on
  ASSERT: WanderFrequency shows "Frequently"
  ASSERT: cognitive-section is visible (because diagnosis = Alzheimer's)

Test: step1_validation_error_shows_on_step1_only
  ACTION: leave ResidentName empty, click Save Step 1
  ASSERT: validation error appears next to ResidentName
  ASSERT: NO errors appear on Step 2/3/4 fields

Test: step2_validation_error_for_missing_cognitive_score
  ACTION: save Step 1 with Alzheimer's
  ACTION: go to Step 2, leave CognitiveScore empty, click Save Step 2
  ASSERT: validation error for CognitiveScore
  ASSERT: NO error for ResidentName or EmergencyContact

Test: submit_with_all_drafts_saved_returns_care_plan
  ACTION: complete and save all 4 steps
  ACTION: click Submit
  ASSERT: server returns 200 with screeningId, careUnit, monitoringLevel

Test: submit_with_missing_step2_draft_returns_error
  ACTION: save Step 1 only, skip Step 2, go to Step 4
  ACTION: click Submit
  ASSERT: server returns 400 with error about missing Step 2

Test: per_step_validation_does_not_leak_across_steps
  ACTION: save Step 1 (valid)
  ACTION: go to Step 3, leave fields empty, click Save Step 3
  ASSERT: Step 3 validates only Step 3 fields (CausedInjury, TakesPainMedication, etc.)
  ASSERT: Step 1 fields (ResidentName, Age) are NOT referenced in errors

Test: veteran_section_visible_on_reload_when_saved_as_veteran
  ACTION: toggle IsVeteran on, fill VaId, save Step 1
  ACTION: reload Step 1
  ASSERT: IsVeteran switch is on
  ASSERT: veteran-section is visible
  ASSERT: VaId field shows saved value

Test: conditions_fire_on_reloaded_step2_when_user_changes_value
  ACTION: save Step 1 with Alzheimer's, save Step 2 with CognitiveScore 12
  ACTION: reload Step 2
  ASSERT: CognitiveScore shows 12, cognitive-status shows "Memory Care"
  ACTION: change CognitiveScore to 20
  ASSERT: cognitive-status updates to "Assisted" (condition fires on change)

Test: previous_button_loads_step_from_server_with_saved_state
  ACTION: fill and save Step 1, go to Step 2
  ACTION: click Previous
  ASSERT: Step 1 loads with all saved values visible

## Framework Primitives Reference — Exact Syntax From Sandbox

Study these files for the EXACT syntax. Do not invent new patterns.

### Creating plans (parent vs partial)

```csharp
// Parent view (Index.cshtml) — creates the root plan
var plan = Html.ReactivePlan<Step1DemographicsModel>();

// Partial view (_VeteranDetails.cshtml) — resolves into parent's plan
var plan = Html.ResolvePlan<Step1DemographicsModel>();

// Both render their own plan element — runtime merges by planId
@Html.RenderPlan(plan)
```
Source: `Alis.Reactive.Native/Extensions/PlanExtensions.cs`

### Input components with InputField

```csharp
// NativeTextBox
@{ Html.InputField(plan, m => m.ResidentName, o => o.Required().Label("Resident Name"))
    .NativeTextBox(b => b.Placeholder("Full name")); }

// FusionNumericTextBox with .Reactive
@{ Html.InputField(plan, m => m.Age, o => o.Required().Label("Age"))
    .NumericTextBox(b => b.Min(0).Max(120).Step(1)
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            var comp = p.Component<FusionNumericTextBox>(m => m.Age);
            p.When(comp.Value()).Gte(85m)
                .Then(t => t.Element("risk-badge").SetText("High Risk"))
                .Else(e => e.Element("risk-badge").SetText("Low Risk"));
        })); }

// FusionDropDownList with .DataSource from ViewBag
@{ Html.InputField(plan, m => m.PrimaryDiagnosis, o => o.Required().Label("Diagnosis"))
    .DropDownList(b => b
        .DataSource(diagnoses)
        .Placeholder("Select diagnosis")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            var comp = p.Component<FusionDropDownList>(m => m.PrimaryDiagnosis);
            p.When(comp.Value()).Eq("Heart Disease")
                .Then(t => t.Element("cardiac-section").Show())
                .Else(e => e.Element("cardiac-section").Hide());
        })); }

// FusionSwitch
@{ Html.InputField(plan, m => m.IsVeteran, o => o.Label("Is Veteran"))
    .Switch(b => b.Reactive(plan, evt => evt.Changed, (args, p) =>
    {
        var comp = p.Component<FusionSwitch>(m => m.IsVeteran);
        p.When(comp.Value()).Truthy()
            .Then(t => t.Element("veteran-section").Show())
            .Else(e => e.Element("veteran-section").Hide());
    })); }

// FusionAutoComplete with server-side filtering
@{ Html.InputField(plan, m => m.AttendingPhysician, o => o.Label("Physician"))
    .AutoComplete(b => b
        .Placeholder("Type to search...")
        .DebounceDelay(300)
        .Reactive(plan, evt => evt.Filtering, (args, p) =>
        {
            args.PreventDefault(p);
            p.Get("/SearchPhysicians")
             .Gather(g => g.FromEvent(args, x => x.Text, "q"))
             .Response(r => r.OnSuccess<PhysicianSearchResponse>((json, s) =>
             {
                 args.UpdateData(s, json, j => j.Physicians);
             }));
        })); }

// FusionDatePicker
@{ Html.InputField(plan, m => m.LastDeviceCheck, o => o.Label("Last Device Check"))
    .DatePicker(b => b.Placeholder("Select date")); }

// NativeHiddenField
@Html.HiddenFieldFor(plan, m => m.CareUnit)
```
Source: existing sandbox component pages under Views/Components/

### HTTP pipeline in condition branches

```csharp
// POST inside Then with WhileLoading
p.When(comp.Value()).Gt(140m)
    .Then(then =>
    {
        then.Element("spinner").Show();
        then.Post("/AlertHypertension",
            g => g.Include<FusionNumericTextBox, StepModel>(m => m.SystolicBP))
         .Response(r => r.OnSuccess<ScreeningAlertResponse>((json, s) =>
         {
             s.Element("result").SetText(json, x => x.Message);
             s.Element("spinner").Hide();
         }));
    })
    .Else(e => e.Element("result").SetText("Normal"));
```
Source: `Views/Conditions/VitalsAlert/Index.cshtml`

### Component mutation inside condition branches

```csharp
// SetValue on another component
p.When(careLevel.Value()).Eq("Memory Care")
    .Then(t => t.Component<FusionDropDownList>(m => m.Protocol).SetValue("Enhanced Monitoring"))
    .Else(e => e.Component<FusionDropDownList>(m => m.Protocol).SetValue(""));

// SetChecked on switch
p.When(careLevel.Value()).In("Memory Care", "Skilled Nursing")
    .Then(t => t.Component<FusionSwitch>(m => m.RequiresEscort).SetChecked(true))
    .Else(e => e.Component<FusionSwitch>(m => m.RequiresEscort).SetChecked(false));

// SetValue on hidden field from HTTP response
s.Component<NativeHiddenField>(m => m.CognitiveAssessmentId).SetValue(json, x => x.Id);
```
Source: `Views/Conditions/CareLevelCascade/Index.cshtml`

### Save button with Validate + POST

```csharp
@(Html.NativeButton("save-step1-btn", "Save Step 1")
    .CssClass("rounded-md bg-accent px-6 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Element("save-spinner").Show();
        p.Post("/SaveStep1", g => g.IncludeAll())
         .Validate<Step1Validator>("step1-form")
         .Response(r => r
            .OnSuccess<SaveStepResponse>((json, s) =>
            {
                s.Element("save-result").SetText(json, x => x.Message);
                s.Element("save-spinner").Hide();
            })
            .OnError(400, e =>
            {
                e.ValidationErrors("step1-form");
                e.Element("save-spinner").Hide();
            }));
    }))
```
Source: `Views/Conditions/VitalsAlert/Index.cshtml` + validation skill

### NativeButton for step navigation

```csharp
@(Html.NativeButton("next-1", "Next: Clinical →")
    .CssClass("rounded-md bg-accent px-6 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Element("step-1").Hide();
        p.Element("step-2").Show();
        p.Element("step-ind-1").RemoveClass("active");
        p.Element("step-ind-2").AddClass("active");
    }))
```

### Partial includes with full path (required in Areas)

```csharp
@await Html.PartialAsync("~/Areas/Sandbox/Views/Conditions/AdmissionAssessment/_VeteranDetails.cshtml", Model)
```

### Conditions DSL — all operators used in this form

```csharp
comp.Value()).Gte(85m)          // numeric >=
comp.Value()).Gt(140m)          // numeric >
comp.Value()).Lt(15m)           // numeric <
comp.Value()).Gt(10m)           // numeric >
comp.Value()).Eq("Heart Disease")  // string ==
comp.Value()).In("Alzheimer's", "Parkinson's")  // membership
comp.Value()).Truthy()          // bool/presence
comp.Value()).IsEmpty()         // empty string
comp.Value()).Gte(7m).And(age.Value()).Gte(80m)  // compound AND cross-component
```

### Playwright test helpers — DDL selection via ej2 API

```csharp
// SF DDL keyboard navigation doesn't wrap reliably for re-selections.
// Use ej2 API for deterministic selection:
private async Task SelectDropDown(string componentId, string text)
{
    await Page.EvaluateAsync("(args) => { " +
        "const el = document.getElementById(args.id); " +
        "const ej2 = el.ej2_instances[0]; " +
        "ej2.value = args.text; ej2.dataBind(); }",
        new { id = componentId, text });
}
```

## What NOT to Do

- Do NOT modify any file in Alis.Reactive/, Alis.Reactive.Native/, Alis.Reactive.Fusion/,
  or Alis.Reactive.FluentValidator/ — framework is frozen
- Do NOT use a single shared model across steps
- Do NOT use a single shared validator
- Do NOT add custom JS hydration — model binding handles edit state
- Do NOT break existing 26 tests — run after every task
- Do NOT batch multiple tasks — one at a time, build, test, commit
- Do NOT write manual JS or hacks — every behavior must use the DSL
- Do NOT guess syntax — search the sandbox views for the exact pattern before writing
- Subagents MUST read existing sandbox component pages for correct syntax before coding
- If a primitive doesn't exist, STOP and ask — never invent workarounds
