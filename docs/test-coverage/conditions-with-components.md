# Conditions with Components — Test Coverage Matrix

> Last updated: 2026-03-25
> Branch: feature/coerce-first-class
> Status: P1+P2 complete, P3-P5 pending

## Vertical Slices — Sorted by Priority

| # | Slice | Route | What it proves | Real user story | Status |
|---|---|---|---|---|---|
| **P1** | Vitals Alert | `/Sandbox/Conditions/VitalsAlert` | Condition → HTTP inside Then/Else, ElseIf → different POST per tier, command sandwich | Nurse enters HR > 140 → system POSTs alert, shows confirmation with server timestamp | **PASS (17 tests)** |
| **P2** | Care Level Cascade | `/Sandbox/Conditions/CareLevelCascade` | Condition → Component.SetValue (cascade) + Component.SetChecked (cross-type), In() operator | Select Memory Care → protocol dropdown auto-populates + escort switch enables | **PASS (9 tests)** |
| **P3** | Veteran Toggle | `/Sandbox/Conditions/VeteranToggle` | Condition → Show section with real inputs + commands before/after condition | Toggle Is Veteran → veteran fields appear, toggle off → fields hide | TODO |
| **P4** | Priority Escalation | `/Sandbox/Conditions/PriorityEscalation` | ElseIf → different Dispatch per branch → chained listeners do different HTTP | Urgent → dispatch alert → POST nurse. Routine → dispatch log → POST audit | TODO |
| **P5** | Compound Gate | `/Sandbox/Conditions/CompoundGate` | AND compound with comp sources → HTTP only if both pass | HR AND BP both in range → POST vitals normal. Either out → show warning, no POST | TODO |
| **done** | Numeric Conditions | `/Sandbox/Conditions/NumericCondition` | Simple Gt, ElseIf ladder, AND compound, cross-component source-vs-source | Vital sign thresholds with NumericTextBox | **PASS (20 tests)** |

## Matrix 1: What's INSIDE Condition Branches (Then/ElseIf/Else)

| Action inside branch | Simple When | ElseIf chain | AND/OR compound | Source-vs-source |
|---|---|---|---|---|
| **Element.SetText(static)** | Guards, NC, 18 comp pages | Guards, NC | Guards, NC | NC |
| **Element.Show()** | Guards, NC, 18 comp pages | — | — | — |
| **Element.Hide()** | Guards, NC, 18 comp pages | — | — | — |
| **Element.SetText(source)** | **VA** (comp.Value before condition) | — | — | — |
| **Component.SetValue()** | **CLC** (SetValue on DDL) | **CLC** (ElseIf per care level) | — | — |
| **Component.SetChecked()** | **CLC** (SetChecked on Switch) | — | — | — |
| **Component.Method()** | — | — | — | — |
| **Dispatch()** | — | — | — | — |
| **Post/Get (HTTP)** | **VA** (POST /Alert in Then) | **VA** (POST /Critical, /Warning per tier) | — | — |
| **Load partial** | — | — | — | — |
| **Multiple mixed actions** | partial (Show+SetText) | — | — | — |

> **P1 breakthrough:** Vitals Alert (VA) is the first test where HTTP lives INSIDE a condition branch.
> Still needed: Component mutations, Dispatch, and Load partial inside branches.

## Matrix 2: Commands BEFORE / AFTER Condition Blocks

| Pipeline shape | Tested? | Where? |
|---|---|---|
| `[cmd] → When → Then/Else` (cmd before) | **VA** S1+S3 | NTB change: SetText(args) before When, VA: SetText(comp.Value) before |
| `When → Then/Else → [cmd]` (cmd after) | **VA** S1+S3 | VA: SetText("checked") / SetText("after-ran") after condition |
| `[cmd] → When → Then/Else → [cmd]` (both) | **VA** S3 | VA sandwich: before-ran → condition → after-ran |
| `HTTP → When → Then/Else` (HTTP before) | Event args only | HttpMixing |
| `When → Then/Else → HTTP` (HTTP after) | — | **Never** |
| `Component.SetValue → When → Then/Else` | — | **Never** |
| `When → Then/Else → Component.SetValue` | — | **Never** |
| `Dispatch → When → Then/Else` | — | **Never** |
| `When → Then/Else → Dispatch` | — | **Never** |

## Matrix 3: Real User Scenarios — Condition Source x Branch Action

| Real user scenario | Condition source | Branch action | Tested? | Slice |
|---|---|---|---|---|
| Enter heart rate > 140 → POST alert to nurse station | NumericTB comp.Value().Gt(140) | HTTP Post inside Then | **PASS** | P1 |
| Heart rate <= 140 → show "normal" confirmation | NumericTB comp.Value().Gt(140) | Element.SetText in Else | **PASS** | P1 |
| Select "Memory Care" → set protocol dropdown value | DropDown comp.Value().Eq("Memory Care") | Component.SetValue() | **PASS** | P2 |
| Select "Independent" → clear protocol dropdown | DropDown comp.Value().Eq("Independent") | Component.SetValue("") | **PASS** | P2 |
| Toggle "Is Veteran" → show veteran ID section | Switch comp.Value().Truthy() | Show section + components | — | P3 |
| Toggle off → hide veteran section, commands still run around condition | Switch comp.Value().Truthy() | Hide + before/after cmds | — | P3 |
| Select "Urgent" → dispatch alert → chained POST | DropDown comp.Value().Eq("Urgent") | Dispatch in Then | — | P4 |
| Select "Routine" → dispatch log → different POST | DropDown ElseIf Eq("Routine") | Dispatch in ElseIf | — | P4 |
| HR AND BP both in range → POST vitals normal | NumericTB AND compound | HTTP in Then | — | P5 |
| Either out of range → show warning, no POST | NumericTB AND compound | Element in Else (no HTTP) | — | P5 |

## Matrix 4: ElseIf with Non-Condition Work Before/After

| Pattern | Tested? | Slice |
|---|---|---|
| `SetText → ElseIf chain → SetText` (work sandwiching condition) | **VA** S3 | VA: before-ran → When/Then/Else → after-ran |
| `HTTP → ElseIf chain` (server data drives branches) | — | P4 |
| `ElseIf chain → HTTP` (each branch POSTs differently) | **VA** S2 | VA: Gte(180)→/Critical, Gte(140)→/Warning, Else→text |
| `ElseIf chain → Dispatch` (branch dispatches different events) | — | P4 |
| `Component.SetValue → ElseIf chain → Component.SetValue` | — | P2 |
| `Dispatch → ElseIf chain → HTTP` (full pipeline around condition) | — | P5 |

## Matrix 5: Component Type x Operator (comp.Value() source)

| Component | Gt | Gte | Lt | Lte | Eq | NotEq | Truthy | Falsy | NotNull | IsNull | IsEmpty | NotEmpty | Contains | StartsWith | Matches | MinLength | Between | In |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| FusionNumericTextBox | NTB+NC | NC | — | NC | — | — | — | — | — | — | — | — | — | — | — | — | — | — |
| FusionSwitch | — | — | — | — | — | — | SW | — | — | — | — | — | — | — | — | — | — | — |
| FusionDropDownList | — | — | — | — | — | — | — | — | DDL | — | — | — | — | — | — | — | — | — |
| FusionDatePicker | — | — | — | — | — | — | — | — | DP | — | DP | — | — | — | — | — | — | — |
| FusionDateTimePicker | — | — | — | — | — | — | — | — | DTP | — | DTP | — | — | — | — | — | — | — |
| FusionDateRangePicker | — | — | — | — | — | — | — | — | DRP | — | DRP | — | — | — | — | — | — | — |
| FusionTimePicker | — | — | — | — | — | — | — | — | — | — | TP | — | — | — | — | — | — | — |
| FusionAutoComplete | — | — | — | — | — | — | — | — | AC | — | — | — | — | — | — | — | — | — |
| FusionMultiSelect | — | — | — | — | — | — | — | — | MS | — | — | — | — | — | — | — | — | — |
| FusionMultiColumnCB | — | — | — | — | — | — | — | — | MCCB | — | — | — | — | — | — | — | — | — |
| FusionInputMask | — | — | — | — | — | — | — | — | — | — | IM | IM | — | — | — | — | — | — |
| FusionRichTextEditor | — | — | — | — | — | — | — | — | — | — | RTE | RTE | — | — | — | — | — | — |
| NativeTextBox | — | — | — | — | — | — | — | — | — | — | TB | — | — | — | — | — | — | — |
| NativeTextArea | — | — | — | — | — | — | — | — | — | — | TA | — | — | — | — | — | — | — |
| NativeCheckBox | — | — | — | — | — | — | CB | — | — | — | — | — | — | — | — | — | — | — |
| NativeDropDown | — | — | — | — | — | — | — | — | — | — | — | NDD | — | — | — | — | — | — |
| NativeRadioGroup | — | — | — | — | — | — | — | — | — | — | — | RG | — | — | — | — | — | — |
| NativeCheckList | — | — | — | — | — | — | — | — | — | — | — | CL | — | — | — | — | — | — |

Legend: NTB=NumericTextBox page, NC=NumericCondition slice, SW=Switch page, DDL=DropDownList page, DP=DatePicker page, DTP=DateTimePicker page, DRP=DateRangePicker page, TP=TimePicker page, AC=AutoComplete page, MS=MultiSelect page, MCCB=MultiColumnComboBox page, IM=InputMask page, RTE=RichTextEditor page, TB=NativeTextBox page, TA=NativeTextArea page, CB=NativeCheckBox page, NDD=NativeDropDown page, RG=NativeRadioGroup page, CL=NativeCheckList page

## Matrix 6: Composition Patterns x Source Type

| Pattern | Event Args (CustomEvent) | Component Read (comp.Value()) |
|---|---|---|
| Simple (single guard) | Guards (38 tests) | 18 component pages + NC |
| ElseIf chain | Guards (grade ladder) | NC (heart rate zones) |
| AND (direct) | Guards | NC (BP range) |
| OR (direct) | Guards | — |
| NOT (invert) | Guards | — |
| AND (lambda) | Guards | — |
| OR (lambda) | Guards | — |
| Source-vs-source | — | NC (HR vs threshold) |
| Per-action When | Guards | — |
