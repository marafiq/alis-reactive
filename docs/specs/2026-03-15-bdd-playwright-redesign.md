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

## New Component: NativeDatePicker

Onboard as a complete vertical slice following the canonical pattern:

```
NativeDatePicker/
  NativeDatePicker.cs                  — sealed : NativeComponent, IInputComponent (ReadExpr => "value")
  NativeDatePickerBuilder.cs           — internal ctor, wraps <input type="date">
  NativeDatePickerHtmlExtensions.cs    — Html.NativeDatePickerFor(plan, expr)
  NativeDatePickerExtensions.cs        — SetValue(string), FocusIn(), Value()
  NativeDatePickerEvents.cs            — Singleton: Changed event ("change")
  NativeDatePickerReactiveExtensions.cs — Single .Reactive<TModel, TProp, TArgs>()
  Events/
    NativeDatePickerOnChanged.cs       — NativeDatePickerChangeArgs { Value }
```

- `ReadExpr => "value"` (HTML date input stores ISO string in `.value`)
- `SetValue(string)` — ISO date string (e.g., "2026-03-15")
- `Value()` — `TypedComponentSource<string>` for condition guards
- One `Changed` event, one `Reactive` overload. Duplication from NativeTextBox is intentional.

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

Each page exercises: property write, property read, event, event-args condition (typed), component-read condition (typed), reactive wiring.

| Route | View | Slice Surface Tested |
|-------|------|---------------------|
| `/Sandbox/NativeButton` | Button full API | Click event, SetText, FocusIn |
| `/Sandbox/NativeCheckBox` | Checkbox full API | SetChecked, Changed, When(args, x => x.Checked).Truthy(), When(comp.Value()) |
| `/Sandbox/NativeTextBox` | TextBox full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).IsEmpty(), FocusIn |
| `/Sandbox/NativeDropDown` | DropDown full API | SetValue, Changed, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull(), FocusIn |
| `/Sandbox/NativeDatePicker` | DatePicker full API | SetValue, Changed, When(args, x => x.Value).NotNull(), When(comp.Value()).IsEmpty() |

### Group 3: Fusion Components

Same mandatory surface: property write, property read, event, typed conditions, reactive wiring.

| Route | View | Slice Surface Tested |
|-------|------|---------------------|
| `/Sandbox/FusionNumericTextBox` | NumericTextBox full API | SetValue, SetMin, Increment, Decrement, Changed/Focus/Blur, When(args, x => x.Value).Gt(), When(comp.Value()).Gt() |
| `/Sandbox/FusionDropDownList` | DropDownList full API | SetValue, SetText, ShowPopup, HidePopup, Changed/Focus/Blur, When(args, x => x.Value).Eq(), When(comp.Value()).NotNull() |

### Group 4: HTTP & Data

| Route | View | Behavior Tested |
|-------|------|-----------------|
| `/Sandbox/Http` | HTTP verbs | GET/POST/PUT/DELETE, chained, parallel, response routing |
| `/Sandbox/ContentType` | Response handling | JSON flat/nested, HTML partial via Into() |
| `/Sandbox/Gather` | Form gathering | Include per component, IncludeAll, static params |

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
Core Primitives          | Native Components       | Fusion Components
- Events & Dispatch      | - NativeButton          | - FusionNumericTextBox
- Payload Resolution     | - NativeCheckBox        | - FusionDropDownList
- Conditions & Guards    | - NativeTextBox         |
                         | - NativeDropDown        |
                         | - NativeDatePicker      |

HTTP & Data              | Validation              | Framework Infrastructure
- HTTP Verbs             | - Client Rules          | - IdGenerator
- Content Types          | - Server Errors         | - Architecture Regression
- Gather                 | - Conditional Rules     | - Reactive Wiring
                         | - Hidden Fields         | - Reactive Conditions
                         | - Live Clearing         |
                         | - Partial Merge         |
                         | - Isolation             |
                         | - Multi-Partial         |
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
