# Component Gestures, Surfaces, Assertions

The locator grammar for Playwright tests. The codebase is mid-migration: both
`PagePlan<TModel>` and `ComponentScope`/direct locators coexist. New tests
prefer `PagePlan<TModel>` for compile-time safety.

## Locator Patterns

**1. `PagePlan<TModel>` — expression-based, compile-time safety (preferred)**

```csharp
_plan = await PagePlan<TModel>.FromPage(Page);
var physician = _plan.AutoComplete(m => m.Physician);
await _plan.ErrorFor(m => m.Physician);  // validation error
```

Reads the plan JSON from the page. Model renames break tests at compile time.

**2. `ComponentScope` — string-based, when model expressions aren't available**

```csharp
var scope = new ComponentScope(Page, "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__");
var score = scope.NumericTextBox("MemoryAssessmentScore");
```

Uses the IdGenerator pattern (`{TypeScope}__{PropertyName}`): the string is the
prefix including its `__`; `new ComponentScope(Page, typeof(TModel))` builds it
from the model type. For tests whose project does not reference the model type,
and for multi-scope pages.

**3. Direct `Page.Locator` — for explicit-ID elements not model-bound**

```csharp
private ILocator SubmitBtn => Page.Locator("#submit-btn");
```

## Grammar

```
COMPONENT (via PagePlan or ComponentScope):
  | .AutoComplete("Prop")     -- Type, SelectItem, TypeAndSelect, Clear, Focus, Blur
  | .DropDownList("Prop")     -- Select("text"), Choose, Pick, Open, Focus
  | .NumericTextBox("Prop")   -- Fill, FillAndBlur, Clear, Focus, Blur
  | .Switch("Prop")           -- Toggle
  | .TextBox("Prop")          -- Fill, FillAndBlur, Clear, Focus, Blur
  | .DatePicker("Prop")       -- Fill, FillAndBlur, SelectDate, Clear, Focus, Blur
  | .TimePicker("Prop")       -- Fill, FillAndBlur, SelectTime, Clear, Focus, Blur
  | .DateTimePicker("Prop")   -- Fill, FillAndBlur, Select, SelectDate, SelectTime, Clear, Focus, Blur
  NOTE: DatePicker/TimePicker/DateTimePicker — the popup gestures (SelectDate, SelectTime) are reliable.
        FillAndBlur (text gesture) may not set ej2.value. Prefer the popup gesture when available.
        See DatePickerLocator.cs: "typed text does not always update Syncfusion ej2.value."
  | .DateRangePicker("Prop")  -- Fill, FillAndBlur, SelectRange, Clear, Focus, Blur
  | .MultiColumnComboBox("Prop") -- Select, Choose, Pick, Open, Focus
  | .InputMask("Prop")        -- Fill, FillAndBlur, Type, Clear, Focus, Blur
  | .RichTextEditor("Prop")   -- Fill, FillAndBlur, Clear, Focus, Blur
  | .MultiSelect("Prop")      -- Open, SelectItem, SelectItems, RemoveItem, Blur

SURFACE :=
  | _plan.Element("explicit-id")        -- explicit-ID page elements
  | _plan.ErrorFor(m => m.Prop)         -- validation error for a model property
  | component.Input                     -- the input element (for value assertions)
  | component.PopupItems                -- popup suggestions (AutoComplete)
  | component.PopupItem("Dr. Smith")    -- specific popup suggestion by text (AutoComplete)
  | component.CalendarIcon              -- calendar icon button (DatePicker family, opens popup)
  | component.Popup                     -- calendar popup container (DatePicker family)

ASSERTION :=
  | Expect(SURFACE).ToContainTextAsync(...)
  | Expect(SURFACE).ToBeVisibleAsync()
  | Expect(SURFACE).ToHaveValueAsync(...)
  | Assert.That(request.PostData, Does.Contain(...))   -- framework gather tests only
```

## Always

- `_plan.ComponentType(m => m.Prop)` or `scope.ComponentType("Prop")` — never
  `Page.Locator("#hardcoded-id")` for model-bound elements.
- `_plan.ErrorFor(m => m.Prop)` — never raw `span[data-valmsg-for]` selectors.
- Gestures — never `EvaluateAsync` or `ej2_instances`.
- When a popup click does not work: use the proven gesture for that component
  (see `patterns.md`), never hack the selector.
