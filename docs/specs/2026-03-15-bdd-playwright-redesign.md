# BDD Playwright Test Redesign — Senior Living Domain

> Each test file = one view = one controller = one behavior surface.
> No mocks. No raw HTML. No shared endpoints. Framework primitives only.

## Problem

Current Playwright tests are organized by framework concept (Events, Conditions, Requests)
rather than by vertical slice. Tests share controller endpoints (`Echo`, `ValidateClient`),
use raw `<button onclick>` inline JS, and the Validation controller is a 27-action monolith.
The Home page only links 6 of 13 sandbox routes.

## Goals

1. **BDD tests** — test behaviors, not implementation. Names describe what the system does.
2. **Vertical slice isolation** — each test class has its own view + controller. No shared actions.
3. **Parallel execution** — all test classes run concurrently. Zero shared state.
4. **Framework primitives only** — every input uses `Html.Field()` + builder. No raw HTML.
5. **Senior living domain** — realistic models instead of abstract test data.
6. **Onboard NativeDatePicker** — first new component, value + change event only.
7. **Sandbox index** — cards for every route, grouped logically with descriptions.

## Domain Models

Senior living gives us natural field diversity (strings, dates, numbers, booleans, enums,
nested addresses) without inventing abstract test models.

```csharp
// Core models — each view picks the fields it needs
ResidentModel        // Name, DOB, RoomNumber, CareLevel, Status, IsActive, AdmissionDate
FacilityModel        // Name, Address (Street/City/Zip), Capacity, Type
CareNoteModel        // ResidentName, Date, Notes, Severity
AdmissionModel       // ResidentName, FacilityName, AdmissionDate, CareLevel, Notes, IsEmergency
```

## Component Roadmap

All components: value + change event only. Onboarded one at a time. Duplication is intentional —
each vertical slice is self-contained. Each follows the canonical file pattern established in the
codebase (sealed phantom type, internal-ctor builder, separate HtmlExtensions/Extensions/Events/
ReactiveExtensions files).

### Native Components (HTML elements)

| Component | HTML Element | ReadExpr | Value Type | Status |
|-----------|-------------|----------|------------|--------|
| `NativeTextBox` | `<input type="text">` | `"value"` | `string` | Exists |
| `NativeDropDown` | `<select>` | `"value"` | `string` | Exists |
| `NativeCheckBox` | `<input type="checkbox">` | `"checked"` | `bool` | Exists |
| `NativeButton` | `<button>` | N/A (non-input) | N/A | Exists |
| `NativeDatePicker` | `<input type="date">` | `"value"` | `string` | New |
| `NativeTimePicker` | `<input type="time">` | `"value"` | `string` | New |
| `NativeNumericInput` | `<input type="number">` | `"value"` | `string` | New |
| `NativeTextArea` | `<textarea>` | `"value"` | `string` | New |
| `NativeRadioButton` | `<input type="radio">` | `"value"` | `string` | New |

### Fusion Components (Syncfusion EJ2)

| Component | SF Builder | ReadExpr | Value Type | Status |
|-----------|-----------|----------|------------|--------|
| `FusionNumericTextBox` | `NumericTextBoxBuilder` | `"value"` | `decimal` | Exists |
| `FusionDropDownList` | `DropDownListBuilder` | `"value"` | `string` | Exists |
| `FusionDatePicker` | `DatePickerBuilder` | `"value"` | `string` | New |
| `FusionDateRangePicker` | `DateRangePickerBuilder` | `"value"` | `string` | New |
| `FusionDateTimePicker` | `DateTimePickerBuilder` | `"value"` | `string` | New |
| `FusionTimePicker` | `TimePickerBuilder` | `"value"` | `string` | New |
| `FusionMultiSelectDropdown` | `MultiSelectBuilder` | `"value"` | `string` | New |
| `FusionComboBox` | `ComboBoxBuilder` | `"value"` | `string` | New |
| `FusionInputMask` | `MaskedTextBoxBuilder` | `"value"` | `string` | New |
| `FusionColorPicker` | `ColorPickerBuilder` | `"value"` | `string` | New |
| `FusionRichTextEditor` | `RichTextEditorBuilder` | `"value"` | `string` | New |

### Vertical Slice File Pattern (per component)

Each new component produces exactly these files:

```
{Component}/
  {Component}.cs                    — sealed : {Base}Component, IInputComponent (ReadExpr)
  {Component}Builder.cs             — internal ctor, IHtmlContent (native) or returns SF builder (fusion)
  {Component}HtmlExtensions.cs      — Html.{Component}For(plan, expr) — registers in ComponentsMap
  {Component}Extensions.cs          — SetValue(...), Value() — mutations on ComponentRef<T>
  {Component}Events.cs              — Singleton: Changed event
  {Component}ReactiveExtensions.cs  — Single .Reactive<TModel, TProp, TArgs>()
  Events/
    {Component}OnChanged.cs         — {Component}ChangeArgs { Value }
```

### Onboarding Order

One component per implementation cycle. Native first, then Fusion counterpart.

1. NativeDatePicker
2. NativeTimePicker
3. NativeNumericInput
4. NativeTextArea
5. NativeRadioButton
6. FusionDatePicker
7. FusionTimePicker
8. FusionDateTimePicker
9. FusionDateRangePicker
10. FusionComboBox
11. FusionMultiSelectDropdown
12. FusionInputMask
13. FusionColorPicker
14. FusionRichTextEditor

## Route Map

### Group 1: Core Primitives

| Route | View | Behavior Tested |
|-------|------|-----------------|
| `/Sandbox/Events` | Event dispatch chain | DomReady fires, CustomEvent chains, payload flows between hops |
| `/Sandbox/Payload` | Payload resolution | All primitive types (int, long, double, string, bool) + nested dot-paths resolve to DOM |
| `/Sandbox/Conditions` | Guard evaluation | All 20+ operators, AND/OR/NOT, nested paths, nullable, confirm dialog |

**Conditions page fix:** Replace all raw `<button onclick="document.dispatchEvent(...)">` with
`NativeButton` + `.Reactive()` using `p.Dispatch("event-name", payload)`. Each button dispatches
a typed payload through the plan — zero inline JS.

### Group 2: Native Components

Each page exercises the mandatory surface: property write, property read, event,
event-args condition (typed), component-read condition (typed), reactive wiring.

| Route | View | Slice Surface Tested |
|-------|------|---------------------|
| `/Sandbox/NativeButton` | Button full API | Click event, SetText, FocusIn |
| `/Sandbox/NativeCheckBox` | Checkbox full API | SetChecked, Changed, When(args, x => x.Checked).Truthy(), When(comp.Value()) |
| `/Sandbox/NativeTextBox` | TextBox full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).IsEmpty(), FocusIn |
| `/Sandbox/NativeDropDown` | DropDown full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull(), FocusIn |
| `/Sandbox/NativeDatePicker` | DatePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/NativeTimePicker` | TimePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/NativeNumericInput` | NumericInput full API | SetValue, Changed, When(args, x => x.Value).Gt(), When(comp.Value()).Gt() |
| `/Sandbox/NativeTextArea` | TextArea full API | SetValue, Changed, When(args, x => x.Value).IsEmpty(), When(comp.Value()).MinLength() |
| `/Sandbox/NativeRadioButton` | RadioButton full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).Eq() |

### Group 3: Fusion Components

Same mandatory surface: property write, property read, event, typed conditions, reactive wiring.

| Route | View | Slice Surface Tested |
|-------|------|---------------------|
| `/Sandbox/FusionNumericTextBox` | NumericTextBox full API | SetValue, SetMin, Increment, Decrement, Changed/Focus/Blur, When(args, x => x.Value).Gt(), When(comp.Value()).Gt() |
| `/Sandbox/FusionDropDownList` | DropDownList full API | SetValue, SetText, ShowPopup, HidePopup, Changed/Focus/Blur, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull() |
| `/Sandbox/FusionDatePicker` | DatePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionTimePicker` | TimePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionDateTimePicker` | DateTimePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionDateRangePicker` | DateRangePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionComboBox` | ComboBox full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull() |
| `/Sandbox/FusionMultiSelectDropdown` | MultiSelect full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionInputMask` | InputMask full API | SetValue, Changed, When(args, x => x.Value).Matches(), When(comp.Value()).IsEmpty() |
| `/Sandbox/FusionColorPicker` | ColorPicker full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull() |
| `/Sandbox/FusionRichTextEditor` | RichTextEditor full API | SetValue, Changed, When(args, x => x.Value).IsEmpty(), When(comp.Value()).MinLength() |

### Group 4: HTTP & Data

Each HTTP test verifies both the **request payload sent to server** and the **response handling
in DOM**. The server controller echoes back the received payload so the test can assert the
exact fields, types, and structure that arrived. Both JSON (`application/json`) and form data
(`application/x-www-form-urlencoded`) content types are tested.

| Route | View | Behavior Tested |
|-------|------|-----------------|
| `/Sandbox/Http` | HTTP verbs | GET/POST/PUT/DELETE, chained, parallel, response routing |
| `/Sandbox/ContentType` | Response handling | JSON flat/nested, HTML partial via Into() |
| `/Sandbox/Gather` | Form gathering | Include per component, IncludeAll, static params |

**HTTP test surface (BDD):**

| Behavior | Request Assertion | Response Assertion |
|----------|-------------------|-------------------|
| POST JSON sends gathered values | Server echoes `{ "field": "value" }` — test verifies exact payload | DOM shows success message |
| POST form data sends gathered values | Server echoes received form fields — test verifies exact field names and values | DOM shows confirmation |
| GET loads data on DomReady | N/A (no payload) | DOM populated with response data |
| PUT sends updated payload | Server echoes updated fields — test verifies changed values | DOM shows update confirmation |
| DELETE sends identifier | Server echoes deleted ID — test verifies correct ID sent | DOM removes element |
| Chained requests pass data forward | Second request receives first response's data | DOM shows final chain result |
| Parallel requests fire concurrently | Both endpoints receive correct payloads independently | Both DOM targets populated |
| JSON content type | Server receives `application/json` body with correct structure | Echo confirms field types preserved |
| Form data content type | Server receives `application/x-www-form-urlencoded` with correct key=value pairs | Echo confirms field names match model binding |
| Gather Include sends component value | Server echoes `{ "PropertyName": "componentValue" }` — matches component's current value | DOM confirms gathered |
| Gather IncludeAll sends all registered | Server echoes all registered component values by binding path | DOM confirms all gathered |
| Gather Static sends literal param | Server echoes `{ "paramName": "literalValue" }` | DOM confirms static value |

**Server echo pattern:** Each controller action deserializes the request body, wraps it in a
response that includes the raw received fields, and returns it. The Playwright test reads the
echoed payload from the DOM (rendered by the response handler) and asserts exact field match.
No shared `Echo()` endpoint — each view's controller has its own echo action.

### Group 5: Validation

Each scenario gets its own view + own controller actions. No shared `ValidateClient()`.

| Route | View | Behavior Tested |
|-------|------|-----------------|
| `/Sandbox/Validation/ClientRules` | Client-side rules | Required, email, regex, range, minlength — resident intake form |
| `/Sandbox/Validation/ServerErrors` | Server-side errors | ProblemDetails 400 display at fields — duplicate resident check |
| `/Sandbox/Validation/Conditional` | Conditional rules | Checkbox toggle makes field required — emergency contact |
| `/Sandbox/Validation/HiddenFields` | Hidden field skip | Toggle visibility → validation engine skips hidden fields |
| `/Sandbox/Validation/LiveClearing` | Live error clearing | Typing in field clears its error message |
| `/Sandbox/Validation/PartialMerge` | Partial merge | Same-model partials merge into one validation surface |
| `/Sandbox/Validation/Isolation` | Standalone isolation | Independent partial plan doesn't pollute parent validation |
| `/Sandbox/Validation/Workflow` | Multi-partial workflow | Root + same-model + standalone plans coexist correctly |

### Group 6: Framework Infrastructure

| Route | View | Behavior Tested |
|-------|------|-----------------|
| `/Sandbox/IdGenerator` | Collision-free IDs | Format, uniqueness, nested properties, JSON + form POST |
| `/Sandbox/Architecture` | Vendor-agnostic regression | Native + Fusion side-by-side, cross-vendor gather, events |
| `/Sandbox/ReactiveWiring` | .Reactive() syntax | Component-event triggers both vendors, nested property IDs, cross-vendor custom event |
| `/Sandbox/ReactiveConditions` | Conditions inside .Reactive() | ElseIf chains, auto-fill patterns, cross-vendor mutations |

## Test Architecture

### BDD Naming

Test classes: `When{Verb}{Subject}` (e.g., `WhenUsingNativeDatePicker`, `WhenValidatingClientRules`)
Test methods: `{Behavior_in_snake_case}` (e.g., `SetValue_updates_the_date_field`)

### Parallel Execution

```csharp
// AssemblyInfo.cs
[assembly: Parallelizable(ParallelScope.All)]
```

- Each test class navigates its own independent route
- Each controller is self-contained — no shared action methods
- WebServerFixture starts Kestrel once (assembly-level), serves all routes concurrently
- No database, no shared state between routes

### Test Shape

Each test class tests exactly what the vertical slice exposes. Every input component
MUST exercise the conditions module with typed access — this is how we know the
full DSL chain (`When(args, x => x.Value).Eq(...)` and `When(comp.Value()).Gt(...)`)
isn't broken.

**Mandatory test surface per input component:**

| Surface | What it proves | Example |
|---------|---------------|---------|
| Property write | `SetValue()` works end-to-end | DomReady sets initial value |
| Property read | `Value()` returns `TypedComponentSource<T>` | Value echoed to text element |
| Event | `Changed` event fires and carries payload | Changed event updates status text |
| Condition (event args) | `When(args, x => x.Value).Eq(...)` typed access works | Changed + condition → show/hide |
| Condition (component read) | `When(comp.Value()).Gt(...)` typed access works | Component value drives conditional UI |
| Reactive wiring | `.Reactive(plan, evt => evt.Changed, ...)` wires correctly | Changed event triggers reaction chain |

```csharp
[TestFixture]
public class WhenUsingNativeDatePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeDatePicker";

    [Test] public async Task Page_loads_without_errors() { }

    // Property write
    [Test] public async Task DomReady_sets_initial_date_value() { }

    // Property read + component condition
    [Test] public async Task Value_drives_conditional_visibility() { }

    // Event + event args condition (typed access)
    [Test] public async Task Changed_event_with_condition_shows_status() { }

    // Reactive wiring
    [Test] public async Task Reactive_changed_updates_dependent_element() { }
}
```

### What Tests Do NOT Test

- Internal descriptor shape (covered by C# unit tests + schema validation)
- JSON plan structure (covered by C# snapshot tests)
- Runtime module internals (covered by vitest)
- Implementation details (bracket notation, resolveRoot, walk.ts)

Tests ONLY assert: "when a user does X in the browser, the DOM shows Y."

## Sandbox Home Page

Cards grouped with headers matching the 6 groups above. Each card links to its route
with a short description of what behavior it exercises.

```
Core Primitives           | Native Components        | Fusion Components
- Events & Dispatch       | - NativeButton           | - FusionNumericTextBox
- Payload Resolution      | - NativeCheckBox         | - FusionDropDownList
- Conditions & Guards     | - NativeTextBox          | - FusionDatePicker
                          | - NativeDropDown         | - FusionTimePicker
                          | - NativeDatePicker       | - FusionDateTimePicker
                          | - NativeTimePicker       | - FusionDateRangePicker
                          | - NativeNumericInput     | - FusionComboBox
                          | - NativeTextArea         | - FusionMultiSelectDropdown
                          | - NativeRadioButton      | - FusionInputMask
                          |                          | - FusionColorPicker
                          |                          | - FusionRichTextEditor

HTTP & Data               | Validation               | Framework Infrastructure
- HTTP Verbs              | - Client Rules           | - IdGenerator
- Content Types           | - Server Errors          | - Architecture Regression
- Gather                  | - Conditional Rules      | - Reactive Wiring
                          | - Hidden Fields          | - Reactive Conditions
                          | - Live Clearing          |
                          | - Partial Merge          |
                          | - Isolation              |
                          | - Multi-Partial          |
```

## Implementation Order

1. Onboard NativeDatePicker vertical slice (component + builder + extensions + events + reactive)
2. Create NativeDatePicker sandbox view + controller + Playwright test
3. Restructure existing Playwright tests to BDD naming
4. Split Validation monolith into 8 views + controllers
5. Fix Conditions page (replace raw buttons with NativeButton + Dispatch)
6. Split PlaygroundSyntax into ReactiveWiring + ReactiveConditions
7. Create missing views (NativeButton, NativeTextBox standalone pages)
8. Update Sandbox Home page with grouped cards
9. Enable parallel execution
10. Run full suite — all tests green
11. Post-task: verify vertical slice pattern compliance

## Constraints

- **Never change the DSL shape** — public API stays identical
- **Duplication over abstraction** — each slice is self-contained, no shared base
- **No mocks** — real browser, real server, real components
- **No raw HTML** — every `<input>`, `<select>`, `<button>` uses framework builders
- **Schema tests stay** — C# unit tests with JSON schema validation unchanged
- **One component at a time** — NativeDatePicker first, FusionDatePicker later
