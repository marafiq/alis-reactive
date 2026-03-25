# AdmissionAssessment — App Spec

> Vertical slice: AdmissionAssessment
> Route: `/Sandbox/Conditions/AdmissionAssessment`

---

## Part 1: The App (no framework, pure user perspective)

### What is this

When a new resident moves into Sunrise Senior Living, the admissions nurse sits down
with the resident (and often a family member) and completes this assessment on a tablet.
It takes 10-15 minutes. The form is smart — it only shows what's relevant based on answers.

### Who uses it

- **Admissions nurse** — fills the form during move-in
- **Charge nurse** — reviews the completed assessment
- **System** — auto-assigns care unit, monitoring level, triggers alerts

### Screen flow

The form has 4 steps with a step indicator at the top. The nurse taps Next/Previous
to move between steps. Some sections within a step appear/disappear based on answers.

```
┌─────────────────────────────────────────────────┐
│  Step 1        Step 2        Step 3      Step 4 │
│  ●━━━━━━━━━━━━━○━━━━━━━━━━━━━○━━━━━━━━━━━○     │
│  Demographics   Clinical      Functional  Review │
└─────────────────────────────────────────────────┘
```

### Step 1: Demographics

Always visible fields:
- **Resident Name** — text input, required
- **Age** — numeric input, required, 0-120
  - 85+ → red badge "High Risk" appears next to age
  - 65-84 → yellow badge "Standard Risk"
  - Under 65 → green badge "Low Risk"
- **Primary Diagnosis** — dropdown (Alzheimer's, Parkinson's, Heart Disease, Diabetes, Stroke, Other), required
- **Attending Physician** — autocomplete, searches server as nurse types (e.g., type "Sm" → "Dr. Smith, Dr. Smythe" appear)
  - When a physician is selected, a green "Verified" badge appears

Conditional section:
- **Is Veteran** — toggle switch
  - When ON → "Veteran Details" section slides in:
    - **VA ID** — text input, required when veteran
    - **Service Branch** — dropdown (Army, Navy, Air Force, Marines, Coast Guard)
    - **"Verify VA Benefits"** button → calls VA system, shows spinner while loading, shows "Eligible" or "Not eligible" result

[Next →]

### Step 2: Clinical Assessment

This step shows DIFFERENT content based on the diagnosis selected in Step 1.

**If Alzheimer's or Parkinson's → Cognitive Assessment:**
- **MMSE Score** — numeric input (0-30), required for this diagnosis
  - Score < 15 → "Severe Impairment" label + Care Unit auto-set to "Memory Care"
  - Score 15-24 → "Moderate Impairment" + Care Unit = "Assisted Living with Memory Support"
  - Score 25+ → "Mild/Normal" + Care Unit = "Standard Assisted Living"
- **Wanders** — toggle switch
  - When ON → Wander details appear:
    - **Frequency** — dropdown (Rarely, Sometimes, Frequently), required when wandering
    - If "Frequently" → system sends elopement risk alert (shows spinner, then confirmation "GPS tracker ordered")
- **"Save Cognitive Assessment"** button → saves this section independently
  - Shows spinner during save, then "Saved ✓" confirmation
  - Stores the returned assessment ID for final submission

**If Heart Disease → Cardiac Assessment:**
- **Systolic Blood Pressure** — numeric input (mmHg), required for this diagnosis
  - If > 140 → system flags hypertension (shows spinner, then "Cardiology referral created")
  - If ≤ 140 → green "Normal" label
- **Has Pacemaker** — toggle switch
  - When ON → Pacemaker details appear:
    - **Pacemaker Model** — text input, required when pacemaker
    - **Last Device Check** — date picker, required when pacemaker
- **"Save Cardiac Assessment"** button → same save pattern

**If Diabetes → Diabetes Management:**
- **Diabetes Type** — dropdown (Type 1, Type 2), required
- **A1C Level** — numeric input (0-20), required
  - If > 9.0 → system flags uncontrolled diabetes (spinner, then "Endocrinology referral created")
  - If ≤ 9.0 → green "Controlled" label
- **Insulin Dependent** — toggle switch
  - When ON → Insulin details appear:
    - **Insulin Schedule** — dropdown (Morning, Evening, Both, As Needed), required when insulin
- **"Save Diabetes Assessment"** button → same save pattern

**If Stroke or Other:**
- Text: "No specialized assessment required for this diagnosis. Proceed to Step 3."

[← Previous] [Save Section] [Next →]

### Step 3: Functional Assessment

Always visible:
- **Fall History** — dropdown (None, 1-2 falls in past year, 3+ falls in past year)
  - If "1-2 falls" or "3+ falls" → Fall Details section appears:
    - **Caused Injury** — toggle switch
      - When ON → Injury details appear:
        - **Injury Type** — dropdown (Bruise, Fracture, Head Injury, Other), required when injury
        - If "Head Injury" → system orders neuro consult (spinner, then confirmation)
- **Fall Risk Score** — numeric input (0-10)
  - If score ≥ 7 AND Age ≥ 80 → Monitoring Level auto-set to "Continuous"
  - Otherwise → "Standard"
- **Mobility Aid** — dropdown (None, Cane, Walker, Wheelchair)
  - If "Walker" or "Wheelchair" → "Escort Required" indicator appears
  - If "Wheelchair" → system requests accessible room setup (spinner, then confirmation)
- **Medication Count** — numeric input
  - If > 10 → "Polypharmacy Alert" warning banner appears: "14 medications — pharmacy review required"
- **Takes Blood Thinners** — toggle switch
  - When ON → Monitoring Level upgraded to at least "Enhanced"
- **Takes Pain Medication** — toggle switch
  - When ON → Pain Assessment section appears:
    - **Pain Level** — numeric input (0-10), required when takes pain meds
      - If ≥ 7 → system sends pain management alert (spinner, then "Pain plan required — immediate")
    - **Pain Location** — text input, required when pain level ≥ 7

[← Previous] [Next →]

### Step 4: Review & Submit

- **Emergency Contact** — text input, required
  - If empty → red "Required" warning visible
- **Summary panel** — read-only display of auto-populated fields:
  - Risk Tier: [High/Standard/Low]
  - Care Unit: [Memory Care / Assisted Living / Standard]
  - Monitoring Level: [Standard / Enhanced / Continuous]
  - Alerts Sent: [list of alerts that fired during assessment]
  - Sections Saved: [2 of 3] with status dots
- **"Submit Assessment"** button
  - Shows spinner + "Submitting assessment..." while loading
  - Server validates ALL conditionally-required fields
  - On success: "Assessment SCR-2026-001 saved. Care plan generated." with green banner
  - On validation failure: errors appear next to the relevant fields, step indicator shows which step has errors

[← Previous] [Submit Assessment]

### Business rules (what the server validates on Submit)

| Condition | Required fields |
|-----------|----------------|
| Always | ResidentName, Age, PrimaryDiagnosis, EmergencyContact |
| Diagnosis = Alzheimer's or Parkinson's | CognitiveScore, CognitiveAssessmentId (section must be saved) |
| Diagnosis = Alzheimer's/Parkinson's + Wanders | WanderFrequency |
| Diagnosis = Heart Disease | SystolicBP, CardiacAssessmentId |
| Diagnosis = Heart Disease + HasPacemaker | PacemakerModel, LastDeviceCheck |
| Diagnosis = Diabetes | DiabetesType, A1cLevel, DiabetesAssessmentId |
| Diagnosis = Diabetes + InsulinDependent | InsulinSchedule |
| FallHistory != None + CausedInjury | InjuryType |
| TakesPainMedication | PainLevel |
| TakesPainMedication + PainLevel >= 7 | PainLocation |
| IsVeteran | VaId |

### Client-side validation rules (same rules, enforced before POST)

The form validates on Submit click BEFORE sending to the server. If validation fails,
the nurse sees the errors immediately without waiting for a server round-trip.
Server validates again (defense in depth).

### What "saved" means

Each clinical section (Cognitive, Cardiac, Diabetes) has its own Save button. When saved:
1. Section data POSTs to its own endpoint
2. Server returns an assessment ID
3. The ID is stored in a hidden field on the form
4. The Save button changes to "Saved ✓" (green)
5. Final Submit checks that all VISIBLE clinical sections have been saved

This allows the nurse to save progress section-by-section, not lose work if they
navigate away, and ensures the final submission has all required assessments linked.

---

## Part 2: Developer Mapping (framework primitives)

### Component Inventory

| Field | Component | Vendor |
|-------|-----------|--------|
| ResidentName | NativeTextBox | native |
| Age | FusionNumericTextBox | fusion |
| PrimaryDiagnosis | FusionDropDownList | fusion |
| AttendingPhysician | FusionAutoComplete | fusion |
| IsVeteran | FusionSwitch | fusion |
| VaId | NativeTextBox | native |
| ServiceBranch | FusionDropDownList | fusion |
| CognitiveScore | FusionNumericTextBox | fusion |
| Wanders | FusionSwitch | fusion |
| WanderFrequency | FusionDropDownList | fusion |
| SystolicBP | FusionNumericTextBox | fusion |
| HasPacemaker | FusionSwitch | fusion |
| PacemakerModel | NativeTextBox | native |
| LastDeviceCheck | FusionDatePicker | fusion |
| DiabetesType | FusionDropDownList | fusion |
| A1cLevel | FusionNumericTextBox | fusion |
| InsulinDependent | FusionSwitch | fusion |
| InsulinSchedule | FusionDropDownList | fusion |
| FallHistory | FusionDropDownList | fusion |
| CausedInjury | FusionSwitch | fusion |
| InjuryType | FusionDropDownList | fusion |
| FallRiskScore | FusionNumericTextBox | fusion |
| MobilityAid | FusionDropDownList | fusion |
| MedicationCount | FusionNumericTextBox | fusion |
| TakesBloodThinners | FusionSwitch | fusion |
| TakesPainMedication | FusionSwitch | fusion |
| PainLevel | FusionNumericTextBox | fusion |
| PainLocation | NativeTextBox | native |
| EmergencyContact | NativeTextBox | native |
| RiskTier | FusionDropDownList (disabled) | fusion |
| CareUnit | FusionDropDownList (disabled) | fusion |
| MonitoringLevel | FusionDropDownList (disabled) | fusion |
| CognitiveAssessmentId | NativeHiddenField | native |
| CardiacAssessmentId | NativeHiddenField | native |
| DiabetesAssessmentId | NativeHiddenField | native |

**Total: 33 fields — 9 native, 24 fusion — 8 component types**

### Step Navigation (primitives)

Step navigation uses explicit Show/Hide — no state tracking needed:

```
Step 1 "Next" button:
  .Reactive(Click) → Element("step-1").Hide() + Element("step-2").Show()
                    + Element("indicator-1").RemoveClass("active")
                    + Element("indicator-2").AddClass("active")

Step 2 "Previous" button:
  .Reactive(Click) → Element("step-2").Hide() + Element("step-1").Show()
                    + Element("indicator-2").RemoveClass("active")
                    + Element("indicator-1").AddClass("active")
```

Each Next/Previous is a NativeButton with explicit element mutations. No hack,
no hidden counter — just direct Show/Hide per step.

### Condition → Primitive Mapping

**Level 1 conditions (driven by top-level fields):**

| User sees | Condition source | Operator | Branch action |
|-----------|-----------------|----------|---------------|
| Age → risk badge | comp.Value() NumericTB | Gte(85m) / Gte(65m) / Else | Element.Show + SetText per badge |
| Diagnosis → cognitive section | comp.Value() DDL | In("Alzheimer's","Parkinson's") | Element.Show/Hide("cognitive-section") |
| Diagnosis → cardiac section | comp.Value() DDL | Eq("Heart Disease") | Element.Show/Hide("cardiac-section") |
| Diagnosis → diabetes section | comp.Value() DDL | Eq("Diabetes") | Element.Show/Hide("diabetes-section") |
| IsVeteran → veteran details | comp.Value() Switch | Truthy() / Else | Element.Show/Hide("veteran-details") |
| FallHistory → fall details | comp.Value() DDL | In("1-2 falls","3+ falls") | Element.Show/Hide("fall-details") |
| TakesPainMed → pain section | comp.Value() Switch | Truthy() / Else | Element.Show/Hide("pain-section") |
| MedCount → polypharmacy | comp.Value() NumericTB | Gt(10m) / Else | Element.Show/Hide("polypharmacy-warning") |

**Level 2 conditions (driven by sub-section fields):**

| User sees | Condition source | Operator | Branch action |
|-----------|-----------------|----------|---------------|
| CogScore → care unit | comp.Value() NumericTB | Lt(15m) / Lt(25m) / Else | Component.SetValue on CareUnit DDL |
| Wanders → frequency section | comp.Value() Switch | Truthy() / Else | Element.Show/Hide |
| HasPacemaker → pacemaker details | comp.Value() Switch | Truthy() / Else | Element.Show/Hide |
| SystolicBP → hypertension | comp.Value() NumericTB | Gt(140m) / Else | POST /AlertHypertension in Then |
| A1C → uncontrolled flag | comp.Value() NumericTB | Gt(9m) / Else | POST /AlertUncontrolled in Then |
| InsulinDependent → schedule | comp.Value() Switch | Truthy() / Else | Element.Show/Hide |
| CausedInjury → injury type | comp.Value() Switch | Truthy() / Else | Element.Show/Hide |
| BloodThinners → monitoring | comp.Value() Switch | Truthy() | Component.SetValue MonitoringLevel |
| Wheelchair → escort | comp.Value() DDL | In("Walker","Wheelchair") | Element.Show + POST /RoomSetup in Eq("Wheelchair") |
| EmergencyContact → warning | comp.Value() TextBox | IsEmpty() / Else | Element.Show/Hide("contact-warning") |

**Level 3 conditions (nested 2 levels deep):**

| User sees | Condition source | Operator | Branch action |
|-----------|-----------------|----------|---------------|
| WanderFreq → elopement alert | comp.Value() DDL | Eq("Frequently") | POST /AlertElopement in Then |
| InjuryType → neuro consult | comp.Value() DDL | Eq("Head Injury") | POST /AlertNeuro in Then |
| PainLevel → pain alert | comp.Value() NumericTB | Gte(7m) | POST /AlertPain in Then |

**Compound conditions:**

| User sees | Sources | Operator | Branch action |
|-----------|---------|----------|---------------|
| FallRisk + Age → continuous | FallRiskScore + Age | Gte(7m).And(age.Value()).Gte(80m) | Component.SetValue MonitoringLevel = "Continuous" |

### HTTP → Primitive Mapping

| User sees | Trigger | HTTP | WhileLoading | OnSuccess |
|-----------|---------|------|-------------|-----------|
| Physician autocomplete | Typing in autocomplete | GET /SearchPhysicians?q=text | (built into AutoComplete) | UpdateData(items) |
| Verify VA | Button click | POST /VerifyVeteran | spinner + disable btn | SetText("Eligible") |
| Elopement alert | WanderFreq = Frequently | POST /AlertElopement | spinner | SetText("GPS tracker ordered") |
| Hypertension flag | SystolicBP > 140 | POST /AlertHypertension | spinner | SetText("Cardiology referral") |
| Uncontrolled diabetes | A1C > 9 | POST /AlertUncontrolled | spinner | SetText("Endocrinology referral") |
| Neuro consult | InjuryType = Head Injury | POST /AlertNeuro | spinner | SetText("Neuro consult ordered") |
| Room setup | MobilityAid = Wheelchair | POST /RequestRoomSetup | spinner | SetText("Accessible room") |
| Pain alert | PainLevel >= 7 | POST /AlertPain | spinner | SetText("Pain plan required") |
| Save Cognitive | Button click | POST /SaveCognitive | spinner + disable btn | HiddenField.SetValue(id) + SetText("Saved") |
| Save Cardiac | Button click | POST /SaveCardiac | spinner + disable btn | HiddenField.SetValue(id) + SetText("Saved") |
| Save Diabetes | Button click | POST /SaveDiabetes | spinner + disable btn | HiddenField.SetValue(id) + SetText("Saved") |
| Submit | Button click | POST /Submit | spinner + disable + "Submitting..." | SetText(screeningId) OR ValidationErrors |

**All HTTP calls use intentional server delays (400-1000ms) to test real loading states.**

### Client-Side Validation (FluentValidation → ReactiveValidator)

```csharp
public class AdmissionAssessmentValidator : ReactiveValidator<HealthScreeningModel>
{
    public AdmissionAssessmentValidator()
    {
        // Always required
        RuleFor(x => x.ResidentName).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0m);
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        RuleFor(x => x.EmergencyContact).NotEmpty();

        // Veteran conditional
        WhenField(x => x.IsVeteran, () => {
            RuleFor(x => x.VaId).NotEmpty();
        });

        // Diagnosis-conditional: Alzheimer's / Parkinson's
        WhenField(x => x.PrimaryDiagnosis, "Alzheimer's", () => {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
        });
        WhenField(x => x.PrimaryDiagnosis, "Parkinson's", () => {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
        });

        // Wandering conditional
        WhenField(x => x.Wanders, () => {
            RuleFor(x => x.WanderFrequency).NotEmpty();
        });

        // Heart Disease conditional
        WhenField(x => x.PrimaryDiagnosis, "Heart Disease", () => {
            RuleFor(x => x.SystolicBP).GreaterThan(0m);
        });
        WhenField(x => x.HasPacemaker, () => {
            RuleFor(x => x.PacemakerModel).NotEmpty();
        });

        // Diabetes conditional
        WhenField(x => x.PrimaryDiagnosis, "Diabetes", () => {
            RuleFor(x => x.DiabetesType).NotEmpty();
            RuleFor(x => x.A1cLevel).GreaterThan(0m);
        });
        WhenField(x => x.InsulinDependent, () => {
            RuleFor(x => x.InsulinSchedule).NotEmpty();
        });

        // Falls conditional
        WhenField(x => x.CausedInjury, () => {
            RuleFor(x => x.InjuryType).NotEmpty();
        });

        // Pain conditional
        WhenField(x => x.TakesPainMedication, () => {
            RuleFor(x => x.PainLevel).GreaterThan(0m);
        });
    }
}
```

Note: PainLocation required when PainLevel >= 7 needs a numeric WhenField condition.
Research needed: does WhenField support numeric thresholds, or only truthy/eq?
If not, server-side validation handles this case.

### Partial Structure

| Partial | Model | Shared plan | Own save |
|---------|-------|-------------|----------|
| _VeteranDetails.cshtml | same | yes | no (verify button only) |
| _CognitiveAssessment.cshtml | same | yes | yes → POST /SaveCognitive |
| _CardiacAssessment.cshtml | same | yes | yes → POST /SaveCardiac |
| _DiabetesManagement.cshtml | same | yes | yes → POST /SaveDiabetes |
| _FallDetails.cshtml | same | yes | no |
| _PainAssessment.cshtml | same | yes | no |

All partials receive the plan from the parent view and add their entries to it.
`@Html.RenderPlan(plan)` is called ONCE in the parent Index.cshtml, not in partials.

### Files to Create

```
Controllers/Conditions/AdmissionAssessmentController.cs
Models/Conditions/AdmissionAssessment/HealthScreeningModel.cs
Models/Conditions/AdmissionAssessment/AdmissionAssessmentValidator.cs
Models/Conditions/AdmissionAssessment/ResponseDtos.cs
Views/Conditions/AdmissionAssessment/Index.cshtml
Views/Conditions/AdmissionAssessment/_VeteranDetails.cshtml
Views/Conditions/AdmissionAssessment/_CognitiveAssessment.cshtml
Views/Conditions/AdmissionAssessment/_CardiacAssessment.cshtml
Views/Conditions/AdmissionAssessment/_DiabetesManagement.cshtml
Views/Conditions/AdmissionAssessment/_FallDetails.cshtml
Views/Conditions/AdmissionAssessment/_PainAssessment.cshtml
tests/.../Conditions/WithComponents/WhenAdmissionAssessmentFilled.cs
```

### Test Approach

Tests use **commented asserts + console warnings** for manual browser verification.
Record the session with Playwright video or GIF. Uncomment asserts as behavior
is verified in the browser.

Phase 1: Build app, verify manually in browser, record
Phase 2: Uncomment asserts one section at a time, make green
Phase 3: Full test suite runs with all asserts active

### Primitives Summary

| Primitive | Count | Examples |
|-----------|-------|---------|
| FusionNumericTextBox | 7 | Age, CognitiveScore, SystolicBP, A1C, FallRisk, MedCount, PainLevel |
| FusionDropDownList | 8 | Diagnosis, ServiceBranch, WanderFreq, DiabetesType, InsulinSchedule, FallHistory, MobilityAid, InjuryType |
| FusionSwitch | 7 | IsVeteran, Wanders, HasPacemaker, InsulinDependent, CausedInjury, BloodThinners, TakesPainMed |
| FusionAutoComplete | 1 | AttendingPhysician |
| FusionDatePicker | 1 | LastDeviceCheck |
| NativeTextBox | 4 | ResidentName, VaId, PacemakerModel, PainLocation, EmergencyContact |
| NativeHiddenField | 3 | CognitiveAssessmentId, CardiacAssessmentId, DiabetesAssessmentId |
| NativeButton | 8 | VerifyVA, SaveCognitive, SaveCardiac, SaveDiabetes, Prev×2, Next×3, Submit |
| When/Then/Else | 17 | All conditions listed above |
| ElseIf ladder | 2 | Age→RiskTier, CogScore→CareUnit |
| AND compound | 1 | FallRisk + Age |
| In() | 4 | Diagnosis, FallHistory, MobilityAid |
| HTTP POST | 8 | Alerts + saves |
| HTTP GET | 1 | Physician autocomplete |
| WhileLoading | 11 | Every HTTP call |
| Validate<T> | 1 | Submit button |
| Partial view | 6 | All sub-sections |
| Show/Hide (step nav) | 4 | Step containers |
