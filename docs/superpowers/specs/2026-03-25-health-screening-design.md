# Health Screening — Design Spec

> Date: 2026-03-25
> Route: `/Sandbox/Conditions/HealthScreening`
> Vertical slice: fully isolated — own model, controller, views, partials, tests

## Domain

Resident admission health screening in a senior living facility. A nurse fills this
during move-in. The form adapts as answers reveal follow-up sections. Each section
saves independently. Final submit validates the complete assessment.

## Model

```csharp
public class HealthScreeningModel
{
    // Section A: Demographics (always visible)
    public string ResidentName { get; set; } = "";
    public decimal Age { get; set; }
    public bool IsVeteran { get; set; }
    public string VaId { get; set; } = "";
    public string ServiceBranch { get; set; } = "";

    // Section B: Primary Diagnosis (drives form shape)
    public string PrimaryDiagnosis { get; set; } = "";

    // Section B1: Cognitive (visible when Alzheimer's/Parkinson's)
    public decimal CognitiveScore { get; set; }     // 0-30 MMSE
    public bool Wanders { get; set; }
    public string WanderFrequency { get; set; } = "";

    // Section B2: Cardiac (visible when Heart Disease)
    public bool HasPacemaker { get; set; }
    public string PacemakerModel { get; set; } = "";
    public decimal SystolicBP { get; set; }

    // Section B3: Diabetes (visible when Diabetes)
    public string DiabetesType { get; set; } = "";
    public decimal A1cLevel { get; set; }
    public bool InsulinDependent { get; set; }
    public string InsulinSchedule { get; set; } = "";

    // Section C: Mobility & Falls (always visible)
    public string FallHistory { get; set; } = "";
    public bool CausedInjury { get; set; }
    public string InjuryType { get; set; } = "";
    public decimal FallRiskScore { get; set; }
    public string MobilityAid { get; set; } = "";

    // Section D: Medications (always visible)
    public decimal MedicationCount { get; set; }
    public bool TakesBloodThinners { get; set; }
    public bool TakesPainMedication { get; set; }
    public decimal PainLevel { get; set; }
    public string PainLocation { get; set; } = "";

    // Section E: Contacts (loaded via HTTP GET)
    public string EmergencyContact { get; set; } = "";
    public string PhysicianName { get; set; } = "";

    // Auto-populated (hidden fields, set by conditions + HTTP responses)
    public string RiskTier { get; set; } = "";
    public string CareUnit { get; set; } = "";
    public string MonitoringLevel { get; set; } = "";
    public string CognitiveAssessmentId { get; set; } = "";
    public string CardiacAssessmentId { get; set; } = "";
    public string DiabetesAssessmentId { get; set; } = "";
    public decimal SectionsCompleted { get; set; }
}
```

## Seed Data

```csharp
ViewBag.Diagnoses = new[] { "Alzheimer's", "Parkinson's", "Heart Disease",
                             "Diabetes", "Stroke", "Other" };
ViewBag.FallOptions = new[] { "None", "1-2 falls", "3+ falls" };
ViewBag.MobilityAids = new[] { "None", "Cane", "Walker", "Wheelchair" };
ViewBag.WanderFreqs = new[] { "Rarely", "Sometimes", "Frequently" };
ViewBag.InsulinSchedules = new[] { "Morning", "Evening", "Both", "As Needed" };
ViewBag.InjuryTypes = new[] { "Bruise", "Fracture", "Head Injury", "Other" };
ViewBag.DiabetesTypes = new[] { "Type 1", "Type 2" };
ViewBag.ServiceBranches = new[] { "Army", "Navy", "Air Force", "Marines", "Coast Guard" };
```

## Endpoints

| Method | Route | Delay | Request | Response (200) | Response (400) |
|--------|-------|-------|---------|----------------|----------------|
| GET | /Index | — | — | View | — |
| POST | /SaveCognitive | 800ms | CognitiveScore, Wanders, WanderFrequency | `{ id: "COG-xxx", message }` | `{ errors }` |
| POST | /SaveCardiac | 600ms | HasPacemaker, PacemakerModel, SystolicBP | `{ id: "CAR-xxx", message }` | `{ errors }` |
| POST | /SaveDiabetes | 700ms | DiabetesType, A1cLevel, InsulinDependent, InsulinSchedule | `{ id: "DIA-xxx", message }` | `{ errors }` |
| POST | /AlertElopement | 400ms | ResidentName, WanderFrequency | `{ message, gpsRequired }` | — |
| POST | /AlertHypertension | 400ms | SystolicBP, ResidentName | `{ message, referral }` | — |
| POST | /AlertPain | 400ms | PainLevel, PainLocation | `{ message, urgency }` | — |
| POST | /AlertNeuro | 400ms | InjuryType, ResidentName | `{ message }` | — |
| POST | /RequestRoomSetup | 500ms | MobilityAid, ResidentName | `{ message, roomType }` | — |
| GET | /Progress | 300ms | — | `{ completed, total, sections[] }` | — |
| POST | /Submit | 1000ms | All gathered fields + hidden IDs | `{ screeningId, careUnit, monitoring, alerts[] }` | `{ errors }` |

All delays are `Task.Delay()` in the controller — simulates real network.

## View Structure

```
Views/Conditions/HealthScreening/
├── Index.cshtml                        ← main layout, creates shared plan
├── _VeteranDetails.cshtml              ← partial, same model, shared plan
├── _CognitiveAssessment.cshtml         ← partial, same model, shared plan, own save
├── _CardiacAssessment.cshtml           ← partial, same model, shared plan, own save
├── _DiabetesManagement.cshtml          ← partial, same model, shared plan, own save
├── _FallDetails.cshtml                 ← partial, same model, shared plan
├── _PainAssessment.cshtml              ← partial, same model, shared plan
```

## Framework Primitive Mapping

### Section A: Demographics

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| Age → Risk Tier | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Gte(85m).Then(SetValue("High")).ElseIf(...).Gte(65m).Then(SetValue("Standard")).Else(SetValue("Low"))` |
| Veteran toggle → show section | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` on veteran section wrapper |
| Veteran toggle off → hide | same | `.Else(Hide)` |
| VA Verify button | `NativeButton.Reactive(Click)` | `Post("/VerifyVeteran", g.Include(VaId).Include(ServiceBranch)).WhileLoading(spinner).Response(OnSuccess(SetText))` |

### Section B: Diagnosis-Driven Sections

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| Diagnosis changed → show cognitive | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).In("Alzheimer's","Parkinson's").Then(Show("cognitive-section")).Else(Hide)` |
| Diagnosis changed → show cardiac | same pipeline | `When(comp.Value()).Eq("Heart Disease").Then(Show("cardiac-section")).Else(Hide)` |
| Diagnosis changed → show diabetes | same pipeline | `When(comp.Value()).Eq("Diabetes").Then(Show("diabetes-section")).Else(Hide)` |

### Section B1: Cognitive (partial)

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| Cognitive score → Care Unit | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Lt(15m).Then(SetValue("Memory Care")).ElseIf(...).Lt(25m).Then(SetValue("Assisted")).Else(SetValue("Standard"))` |
| Wanders toggle → show frequency | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` |
| Wander freq = "Frequently" → alert | Frequency dropdown Changed | `When(comp.Value()).Eq("Frequently").Then(Post("/AlertElopement").WhileLoading(...).Response(...))` |
| Save Cognitive button | `NativeButton.Reactive(Click)` | `Post("/SaveCognitive", g.Include(CognitiveScore).Include(Wanders).Include(WanderFrequency)).WhileLoading(spinner+disable).Response(OnSuccess(HiddenField.SetValue(id)))` |

### Section B2: Cardiac (partial)

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| HasPacemaker → show model/date | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` |
| SystolicBP > 140 → alert | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Gt(140m).Then(Post("/AlertHypertension").WhileLoading(...).Response(...)).Else(SetText("Normal"))` |
| Save Cardiac button | `NativeButton.Reactive(Click)` | `Post("/SaveCardiac", g.Include(...)).WhileLoading(...).Response(OnSuccess(HiddenField.SetValue(id)))` |

### Section B3: Diabetes (partial)

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| A1C > 9 → uncontrolled flag | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Gt(9m).Then(Show("uncontrolled-flag")).Else(Hide)` |
| InsulinDependent → show schedule | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` |
| Save Diabetes button | `NativeButton.Reactive(Click)` | `Post("/SaveDiabetes", g.Include(...)).WhileLoading(...).Response(OnSuccess(HiddenField.SetValue(id)))` |

### Section C: Mobility & Falls

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| FallHistory In("1-2","3+") → show details | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).In("1-2 falls","3+ falls").Then(Show).Else(Hide)` |
| CausedInjury → show injury type | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` |
| InjuryType = "Head Injury" → alert | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Eq("Head Injury").Then(Post("/AlertNeuro").WhileLoading(...).Response(...))` |
| FallRisk >= 7 AND Age >= 80 → continuous | `NativeButton.Reactive(Click)` | `When(fallRisk.Value()).Gte(7m).And(age.Value()).Gte(80m).Then(SetValue("Continuous")).Else(SetValue("Standard"))` |
| Wheelchair → room setup POST | Dropdown Changed | `When(comp.Value()).Eq("Wheelchair").Then(Post("/RequestRoomSetup").WhileLoading(...).Response(...))` |
| Walker/Wheelchair → escort indicator | same pipeline | `When(comp.Value()).In("Walker","Wheelchair").Then(Show("escort-indicator")).Else(Hide)` |

### Section D: Medications

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| MedCount > 10 → polypharmacy warning | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Gt(10m).Then(Show("polypharmacy-warning")).Else(Hide)` |
| BloodThinners → monitoring upgrade | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(SetValue("Enhanced")).Else(skip)` |
| TakesPainMed → show pain section | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Truthy().Then(Show).Else(Hide)` |
| PainLevel >= 7 → alert + location required | `.Reactive(plan, evt => evt.Changed, ...)` | `When(comp.Value()).Gte(7m).Then(Post("/AlertPain").WhileLoading(...).Response(...)).Else(Hide("pain-alert"))` |

### Section E: Contacts

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| EmergencyContact empty → warning | `.Reactive(plan, evt => evt.Changed, ...)` on NativeTextBox | `When(comp.Value()).IsEmpty().Then(Show("contact-warning")).Else(Hide)` |
| Physician contains "Dr." → verified | `.Reactive(plan, evt => evt.Changed, ...)` on NativeTextBox | `When(comp.Value()).Contains("Dr.").Then(Show("verified-badge")).Else(Hide)` |

### Progress & Submit

| Interaction | Trigger | DSL Primitive |
|-------------|---------|---------------|
| After each section save → update progress | OnSuccess of each save | `Get("/Progress").Response(OnSuccess(SetText(completed), SetText(total)))` chained after each section save |
| Submit button | `NativeButton.Reactive(Click)` | `Post("/Submit", g.IncludeAll()).WhileLoading(spinner+disable).Response(OnSuccess(SetText(screeningId)).OnError(400, ValidationErrors("screening-form")))` |

### WhileLoading (every HTTP call)

```csharp
.WhileLoading(l =>
{
    l.Element("save-btn-text").Hide();
    l.Element("save-spinner").Show();
    l.Element("save-btn").AddClass("disabled");
})
```

## Conditions Coverage (what this form exercises)

| Operator | Count | Component types |
|----------|-------|-----------------|
| Eq | 5 | DropDown, TextBox |
| In | 3 | DropDown |
| Gte | 4 | NumericTextBox |
| Gt | 2 | NumericTextBox |
| Lt | 1 | NumericTextBox |
| Between | 1 (via ElseIf) | NumericTextBox |
| Truthy | 5 | Switch, CheckBox |
| Falsy | implied by Else | — |
| IsEmpty | 1 | NativeTextBox |
| NotEmpty | implied | — |
| Contains | 1 | NativeTextBox |
| And (compound) | 1 | NumericTextBox × 2 |
| Source-vs-source | 0 (covered by NC slice) | — |

| Branch action | Count |
|---------------|-------|
| Component.SetValue | 4 (RiskTier, CareUnit, MonitoringLevel, Protocol) |
| Component.SetChecked | 0 (covered by CLC slice) |
| Element.Show/Hide | 12 sections |
| HTTP POST in Then | 6 (alerts + room setup) |
| HTTP POST + WhileLoading | 3 (section saves) |
| Chained GET after POST | 3 (progress update) |
| Hidden field SetValue from response | 3 |

| Pipeline pattern | Count |
|------------------|-------|
| Commands before condition | every section (echo + status) |
| Commands after condition | every section |
| ElseIf ladder | 2 (Age→RiskTier, CogScore→CareUnit) |
| Condition → HTTP in Then | 6 |
| WhileLoading | 9 HTTP calls |
| Partial (same model, shared plan) | 6 |
| Intentional server delay | all endpoints (300-1000ms) |

## Playwright Test Plan

### Happy path tests (200 responses)

| # | Test | Sections touched |
|---|------|------------------|
| 1 | `alzheimers_complete_path_saves_and_submits` | A → B1 (cognitive) → C → D → E → Submit |
| 2 | `heart_disease_complete_path_saves_and_submits` | A → B2 (cardiac) → C → D → E → Submit |
| 3 | `diabetes_insulin_path_saves_and_submits` | A → B3 (diabetes) → C → D → E → Submit |
| 4 | `minimal_other_diagnosis_submits_with_base_fields_only` | A (Other) → C (no falls) → D (no pain) → E → Submit |
| 5 | `veteran_with_alzheimers_verifies_va_and_saves_cognitive` | A (veteran) → verify VA → B1 → save → Submit |

### Conditional visibility tests

| # | Test | What appears/disappears |
|---|------|-------------------------|
| 6 | `selecting_alzheimers_shows_cognitive_section` | B1 visible, B2+B3 hidden |
| 7 | `selecting_heart_disease_shows_cardiac_section` | B2 visible, B1+B3 hidden |
| 8 | `switching_diagnosis_hides_previous_and_shows_new` | B1→B2 transition |
| 9 | `wanders_true_shows_frequency_field` | Level 3 nesting |
| 10 | `falls_history_shows_fall_details` | Nested partial |
| 11 | `injury_true_shows_injury_type` | Level 3 nesting |
| 12 | `pain_medication_shows_pain_section` | Nested partial |
| 13 | `veteran_toggle_shows_and_hides_details` | Same-model partial |

### HTTP-in-branch tests (with loaders)

| # | Test | Trigger | Server response verified |
|---|------|---------|--------------------------|
| 14 | `frequent_wandering_posts_elopement_alert` | WanderFreq = Frequently | gpsRequired = true |
| 15 | `systolic_above_140_posts_hypertension_flag` | SystolicBP = 160 | referral = cardiology |
| 16 | `severe_pain_posts_pain_management_alert` | PainLevel = 8 | urgency = immediate |
| 17 | `head_injury_posts_neuro_consult` | InjuryType = Head Injury | message contains "neuro" |
| 18 | `wheelchair_posts_room_setup_request` | MobilityAid = Wheelchair | roomType = accessible |

### Auto-population tests (component mutations)

| # | Test | Input | Auto-set field | Expected value |
|---|------|-------|----------------|----------------|
| 19 | `age_85_sets_risk_tier_high` | Age = 85 | RiskTier | High |
| 20 | `cognitive_12_sets_care_unit_memory_care` | CogScore = 12 | CareUnit | Memory Care |
| 21 | `fall_risk_7_and_age_80_sets_continuous_monitoring` | FallRisk=7, Age=80 | MonitoringLevel | Continuous |
| 22 | `polypharmacy_warning_shows_above_10_meds` | MedCount = 14 | Warning visible | — |

### Section save + hidden field tests

| # | Test | Save button | Hidden field | Final submit includes |
|---|------|-------------|--------------|------------------------|
| 23 | `save_cognitive_stores_assessment_id_in_hidden_field` | Save Cognitive | CognitiveAssessmentId | COG-xxx |
| 24 | `save_cardiac_stores_assessment_id_in_hidden_field` | Save Cardiac | CardiacAssessmentId | CAR-xxx |
| 25 | `progress_updates_after_each_section_save` | Any save | SectionsCompleted | increments |

### Validation failure tests (400 responses)

| # | Test | Missing field | Expected error |
|---|------|---------------|----------------|
| 26 | `submit_without_saving_cognitive_returns_error` | CognitiveAssessmentId | "must be saved" |
| 27 | `submit_without_emergency_contact_returns_error` | EmergencyContact | "required" |
| 28 | `submit_with_severe_pain_but_no_location_returns_error` | PainLocation | "required for severe pain" |

### Loader / WhileLoading tests

| # | Test | What to assert |
|---|------|----------------|
| 29 | `save_button_shows_spinner_during_http` | spinner visible, text hidden |
| 30 | `save_button_restores_after_http_completes` | spinner hidden, text visible |
