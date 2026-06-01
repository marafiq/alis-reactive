# components

How inputs and display elements are authored, bound to a model, wired to browser
events, mutated by reactions, and read as typed value sources. This is the
components area of the DSL: `Html.InputField`, the `Component<T>` pipeline
references, the `.Reactive()` event-wiring extensions, the `Fusion*` / `Native*`
component builders (textbox, dropdown, datepicker, numeric, checkbox, grid),
`TypedEvent<TArgs>`, `.SetValue` / `.Value`, `ElementBuilder`
(`Element` / `AddClass` / `RemoveClass` / `ToggleClass` / `Show` / `Hide` /
`SetText` / `SetHtml`), and component focus.

Every sample is a **tall** vertical fluent chain — one call per line, read
top-to-bottom. Domain: senior-living (residents, care levels, facilities,
billing, assessments).

Mental model:

- **`Html.InputField(plan, m => m.Prop)`** opens a model-bound field wrapper
  (label + validation slot). You finish it with **one** component factory —
  `.NativeTextBox(...)`, `.FusionDropDownList(...)`, etc. The factory registers
  the input contract, so gather and validation can find it.
- **Non-input components** (grid, accordion, chart) are created directly off the
  helper — `Html.FusionGrid<TModel, TRow>(plan, "grid-id", ...)` — no field
  wrapper, no validation slot.
- **`.Reactive(plan, e => e.SomeEvent, (args, p) => ...)`** wires a *browser
  event* on a rendered component into a reactive pipeline. `args` is typed.
- Inside a pipeline `p`, **`p.Component<TComponent>(m => m.Prop)`** returns a
  typed `ComponentRef` you mutate (`.SetValue(...)`, `.Focus()`) or read
  (`.Value()` -> a `TypedSource`).
- **`p.Element("id")`** targets any plain DOM element for text / HTML / class /
  visibility mutations.

---

## Html.InputField — open a model-bound field

`Html.InputField(plan, expr)` returns an `InputBoundField<TModel, TProp>`. It is
the only correct way to start an input: it allocates the deterministic element
id, captures the binding path, and reserves the registration slot the component
factory fills. You always chain exactly one component factory onto it.

### InputField(plan, expression)
Bind a typed model property to a field; chain the component factory to choose the control.

```csharp
Html.InputField(plan, m => m.Resident.FullName)
    .NativeTextBox(b => b
        .Placeholder("Resident full name"));
```

### InputField(plan, expression, configure) — label + required marker
The second overload configures the field wrapper: label text and the required `*` indicator.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name")
        .Required())
    .NativeTextBox(b => b
        .Placeholder("Resident full name"));
```

### InputFieldOptions.Label(text)
Sets the label rendered above the input.

```csharp
Html.InputField(plan, m => m.Resident.RoomNumber, o => o
        .Label("Room number"))
    .NativeTextBox(b => b
        .Type("number"));
```

### InputFieldOptions.Required()
Adds the required marker next to the label (display only; server validation stays authoritative).

```csharp
Html.InputField(plan, m => m.Assessment.CareLevel, o => o
        .Label("Care level")
        .Required())
    .FusionDropDownList(b => b
        .DataSource(careLevels)
        .Fields<CareLevelItem>(t => t.Name, v => v.Code));
```

---

## Native component builders — plain HTML inputs

Native builders implement the framework HTML-content type directly, so the
factory both renders and registers in one call. The pipeline finishes with
`.Reactive(...)` (no separate `.Render()`).

### NativeTextBox — `<input>` bound to a property
Renders a native text input; defaults to `type="text"`.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name")
        .Required())
    .NativeTextBox(b => b
        .Placeholder("Enter full name"));
```

### NativeTextBox.Type(type) — email / password / number / etc.
Overrides the HTML input `type` attribute.

```csharp
Html.InputField(plan, m => m.Resident.ContactEmail, o => o
        .Label("Contact email"))
    .NativeTextBox(b => b
        .Type("email")
        .Placeholder("name@example.com"));
```

### NativeTextBox.CssClass(css)
Adds CSS classes to the input element.

```csharp
Html.InputField(plan, m => m.Resident.RoomNumber, o => o
        .Label("Room number"))
    .NativeTextBox(b => b
        .Type("number")
        .CssClass("form-control room-input"));
```

### NativeTextBox.Placeholder(text)
Sets the placeholder shown when empty.

```csharp
Html.InputField(plan, m => m.Facility.Name, o => o
        .Label("Facility"))
    .NativeTextBox(b => b
        .Placeholder("Sunrise Senior Living"));
```

### NativeCheckBox — `<input type="checkbox">` bound to a bool
Factory is constrained to a `bool` property.

```csharp
Html.InputField(plan, m => m.Resident.IsVeteran, o => o
        .Label("Veteran"))
    .NativeCheckBox(b => b
        .CssClass("form-check-input"));
```

### NativeCheckBox.CssClass(css)
Adds CSS classes to the checkbox element.

```csharp
Html.InputField(plan, m => m.Consent.MediaRelease, o => o
        .Label("Media release on file"))
    .NativeCheckBox(b => b
        .CssClass("consent-toggle"));
```

---

## Fusion component builders — Syncfusion EJ2 inputs

Fusion input factories wrap a Syncfusion EJ2 MVC builder and render inside the
field wrapper. The `build` callback configures the Syncfusion control; the
factory wires the controlled id and binding name and registers the input
contract automatically.

### FusionTextBox — Syncfusion TextBox bound to a property
The `build` callback receives the raw `TextBoxBuilder`.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name")
        .Required())
    .FusionTextBox(b => b
        .Placeholder("Enter full name")
        .FloatLabelType(FloatLabelType.Auto));
```

### FusionDropDownList — Syncfusion DropDownList bound to a property
Configure data and field mappings inside `build`.

```csharp
Html.InputField(plan, m => m.Assessment.CareLevel, o => o
        .Label("Care level")
        .Required())
    .FusionDropDownList(b => b
        .DataSource(careLevels)
        .Fields<CareLevelItem>(t => t.Name, v => v.Code)
        .Placeholder("Select care level"));
```

### FusionDropDownList.Fields(text, value) — typed field mapping
Derives the camelCase text/value field names from the item type expressions.

```csharp
Html.InputField(plan, m => m.Resident.PrimaryFacilityId, o => o
        .Label("Facility"))
    .FusionDropDownList(b => b
        .DataSource(facilities)
        .Fields<FacilityItem>(t => t.Name, v => v.Id));
```

### FusionDropDownList.Fields(text, value, groupBy) — grouped popup
Adds a group-by expression so the popup groups items.

```csharp
Html.InputField(plan, m => m.Resident.PrimaryFacilityId, o => o
        .Label("Facility"))
    .FusionDropDownList(b => b
        .DataSource(facilities)
        .Fields<FacilityItem>(t => t.Name, v => v.Id, g => g.Region));
```

### FusionDatePicker — Syncfusion DatePicker bound to a DateTime
Binds a date property; value reads/writes use ISO `yyyy-MM-dd`.

```csharp
Html.InputField(plan, m => m.Resident.MoveInDate, o => o
        .Label("Move-in date")
        .Required())
    .FusionDatePicker(b => b
        .Format("MM/dd/yyyy")
        .Placeholder("Select date"));
```

### FusionNumericTextBox — Syncfusion NumericTextBox bound to a number
Binds a numeric property (decimal-shaped value).

```csharp
Html.InputField(plan, m => m.Billing.MonthlyRate, o => o
        .Label("Monthly rate")
        .Required())
    .FusionNumericTextBox(b => b
        .Format("c2")
        .Min(0)
        .Step(50));
```

---

## Non-input component builders — grid

Non-input components are created directly on the helper with an explicit element
id. There is **no** field wrapper and **no** input registration. The factory
returns a `FusionGridBuilder<TModel>` you finish with `.Reactive(...)`.

### Html.FusionGrid(plan, elementId, build) — bound to a row DTO
`TRow` is the row type; `build` configures columns, paging, sorting, editing.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .Columns(cols => cols
            .Add(c => c.Field("fullName").HeaderText("Resident"))
            .Add(c => c.Field("careLevel").HeaderText("Care level"))
            .Add(c => c.Field("monthlyRate").HeaderText("Rate").Format("C2")))
        .AllowPaging()
        .AllowSorting())
    .Reactive(e => e.RowSelected<ResidentRow>(), (args, p) => p
        .Set(m => m.SelectedResidentId, args.Data.Id)))
```

---

## .Reactive — wire a browser event into a pipeline

`.Reactive(plan, e => e.Event, (args, p) => ...)` is the last call in a builder
chain. The event selector picks a `TypedEvent<TArgs>` off the component's events
class; `args` is the typed event payload, `p` is the pipeline. Native inputs and
the grid omit the `plan` argument only when the builder already carries it (grid);
input builders take `plan` explicitly.

### Reactive — native input change event
`evt => evt.Changed` selects the change event; `args` is the typed change payload.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name"))
    .NativeTextBox(b => b
        .Placeholder("Enter full name")
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Element("name-echo")
            .SetText(args, x => x.Value)));
```

### Reactive — native checkbox change event
Branch on the checked state to drive other UI.

```csharp
Html.InputField(plan, m => m.Resident.IsVeteran, o => o
        .Label("Veteran"))
    .NativeCheckBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .When(args, x => x.Checked)
            .Then(p => p
                .Element("veteran-benefits")
                .Show())
            .Else(p => p
                .Element("veteran-benefits")
                .Hide())));
```

### Reactive — Fusion textbox input event (every keystroke)
`Input` fires on every keystroke; `Changed` fires on commit.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name"))
    .FusionTextBox(b => b
        .Reactive(plan, e => e.Input, (args, p) => p
            .Element("char-count")
            .SetText(args, x => x.Value)));
```

### Reactive — Fusion textbox change / focus / blur events
Four event lanes: `Input`, `Changed`, `Focus`, `Blur`.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name"))
    .FusionTextBox(b => b
        .Reactive(plan, e => e.Blur, (args, p) => p
            .Element("name-field")
            .RemoveClass("editing")));
```

### Reactive — Fusion dropdown change drives a billing recalculation
A change on care level dispatches a domain event other behaviors react to.

```csharp
Html.InputField(plan, m => m.Assessment.CareLevel, o => o
        .Label("Care level")
        .Required())
    .FusionDropDownList(b => b
        .DataSource(careLevels)
        .Fields<CareLevelItem>(t => t.Name, v => v.Code)
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Dispatch("care-level-changed")));
```

### Reactive — Fusion datepicker change event
Read the new date off `args` and echo it.

```csharp
Html.InputField(plan, m => m.Resident.MoveInDate, o => o
        .Label("Move-in date"))
    .FusionDatePicker(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Element("move-in-echo")
            .SetText(args, x => x.Value)));
```

### Reactive — Fusion numeric change event
React when the monthly rate is edited.

```csharp
Html.InputField(plan, m => m.Billing.MonthlyRate, o => o
        .Label("Monthly rate"))
    .FusionNumericTextBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Dispatch("rate-changed")));
```

### Reactive — grid DataStateChange (server-side data)
Fires on sort/page/filter in custom-binding mode; trigger a fetch.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .Columns(cols => cols
            .Add(c => c.Field("fullName").HeaderText("Resident"))))
    .Reactive(e => e.DataStateChange, (args, p) => p
        .Get("/api/residents")
        .Into("residents-grid")))
```

### Reactive — grid ToolbarClick, branch on the clicked button id
One toolbar event; branch on `args.Item.Id` to route per button.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .Toolbar(new[] { "Add", "ExportCsv" }))
    .Reactive(e => e.ToolbarClick, (args, p) => p
        .When(args, x => x.Item.Id)
        .Eq("ExportCsv")
        .Then(p => p
            .Get("/api/residents/export"))))
```

### Reactive — grid RecordClick with a typed row payload
`RecordClick<TRow>()` carries `args.RowData` typed to the row DTO.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .Columns(cols => cols
            .Add(c => c.Field("fullName").HeaderText("Resident"))))
    .Reactive(e => e.RecordClick<ResidentRow>(), (args, p) => p
        .Set(m => m.SelectedResidentId, args.RowData.Id)))
```

### Reactive — grid RowSelected
`RowSelected<TRow>()` carries the selected row's typed data.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .AllowSelection())
    .Reactive(e => e.RowSelected<ResidentRow>(), (args, p) => p
        .Element("selected-name")
        .SetText(args, x => x.Data.FullName)))
```

### Reactive — grid ActionBegin / ActionComplete edit lifecycle
Before/after edit, save, delete, sort, page, filter, group.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .EditSettings(s => s.AllowEditing()))
    .Reactive(e => e.ActionComplete<ResidentRow>(), (args, p) => p
        .Dispatch("roster-changed")))
```

---

## TypedEvent — the typed event payload

`TypedEvent<TArgs>` carries the JS event name plus a strongly-typed `args`
instance. The `.Reactive()` selector returns one; the pipeline reads payload
properties through `args` (used as a payload source by `Set`, `SetText`, `When`,
gather, and dispatch).

### args as a payload source in Set
Read a property off the event payload and assign it to the model.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name"))
    .NativeTextBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Set(m => m.Resident.FullNameUpper, args, x => x.Value)));
```

### args as a payload source in a condition
Guard the pipeline on a payload value.

```csharp
Html.InputField(plan, m => m.Billing.MonthlyRate, o => o
        .Label("Monthly rate"))
    .FusionNumericTextBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .When(args, x => x.Value)
            .Gt(10000m)
            .Then(p => p
                .Element("rate-warning")
                .Show())));
```

### args as a payload source in a dispatch
Carry payload fields into a domain event for other behaviors.

```csharp
@(Html.FusionGrid<RosterModel, ResidentRow>(plan, "residents-grid", b => b
        .Columns(cols => cols
            .Add(c => c.Field("fullName").HeaderText("Resident"))))
    .Reactive(e => e.RecordClick<ResidentRow>(), (args, p) => p
        .DispatchFrom<ResidentSelected>("resident-selected", d => d
            .Set(x => x.ResidentId, args.RowData.Id)
            .Set(x => x.Name, args.RowData.FullName))))
```

---

## Component<T> — typed component reference inside a pipeline

Inside a pipeline `p`, `p.Component<TComponent>(...)` returns a
`ComponentRef<TComponent, TModel>` you mutate or read. Four overloads identify
the component four ways.

### Component(m => m.Prop) — by model expression (same model)
The common case: the component is bound to a property on this view's model.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionTextBox>(m => m.Resident.FullName)
        .SetValue("New resident"));
```

### Component<TComponent, TOtherModel>(expr) — cross-model (partials)
Reference a component bound to a *different* model rendered by a partial.

```csharp
Html.On(plan, t => t.Event("facility-loaded"))
    .Then(p => p
        .Component<FusionDropDownList, FacilityModel>(m => m.SelectedFacilityId)
        .SetValue("FAC-001"));
```

### Component<TComponent>(refId) — by explicit id
Reference a component by a literal element id (non-model-bound id).

```csharp
Html.On(plan, t => t.Event("refresh"))
    .Then(p => p
        .Component<FusionGrid>("residents-grid")
        .Refresh());
```

### Component<TComponent>() — app-level singleton by default id
Reference a layout-owned app component (Toast, Confirm, Loader) by its fixed id.

```csharp
Html.On(plan, t => t.Event("saved"))
    .Then(p => p
        .Component<FusionToast>()
        .Show("Saved"));
```

---

## .SetValue / .Value — component mutation and reading

Every input `ComponentRef` exposes `.SetValue(...)` (write the control's value)
and `.Value()` (read it as a `TypedSource` for conditions, gather, or as a value
source in other mutations). The value type matches the control's semantic type.

### FusionTextBox.SetValue(string) — write + flush to the visible input
Sets the value and calls `dataBind` so the DOM updates immediately.

```csharp
Html.On(plan, t => t.Event("prefill"))
    .Then(p => p
        .Component<FusionTextBox>(m => m.Resident.FullName)
        .SetValue("Jane Doe"));
```

### FusionTextBox.Value() — read as a typed string source
Use the live value in a condition or as a gather source.

```csharp
Html.On(plan, t => t.Event("validate-name"))
    .When(p => p.Component<FusionTextBox>(m => m.Resident.FullName).Value())
    .IsEmpty()
    .Then(p => p
        .Element("name-error")
        .Show());
```

### FusionDropDownList.SetValue(string)
Selects an option by its value key.

```csharp
Html.On(plan, t => t.Event("reset-care-level"))
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Assessment.CareLevel)
        .SetValue("ASSISTED"));
```

### FusionDropDownList.SetText(string)
Sets the displayed text directly.

```csharp
Html.On(plan, t => t.Event("show-placeholder"))
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Assessment.CareLevel)
        .SetText("Choose a care level"));
```

### FusionDropDownList.SetDataSource — from an event payload
Rebind the popup from an event payload array.

```csharp
Html.On(plan, t => t.Event<FacilitiesLoaded>("facilities-loaded"))
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Resident.PrimaryFacilityId)
        .SetDataSource(args, x => x.Facilities)
        .DataBind());
```

### FusionDropDownList.SetDataSource — from an HTTP response body
Rebind from a success response body array.

```csharp
Html.On(plan, t => t.Event("load-facilities"))
    .Get("/api/facilities")
    .OnSuccess<FacilityListResponse>((body, p) => p
        .Component<FusionDropDownList>(m => m.Resident.PrimaryFacilityId)
        .SetDataSource(body, x => x.Items)
        .DataBind());
```

### FusionDropDownList.Value() — read as a typed string source
Read the selected value key for a condition.

```csharp
Html.On(plan, t => t.Event("check-care-level"))
    .When(p => p.Component<FusionDropDownList>(m => m.Assessment.CareLevel).Value())
    .Eq("MEMORY")
    .Then(p => p
        .Element("memory-care-notice")
        .Show());
```

### FusionDatePicker.SetValue(DateTime)
Sets the selected date (serialized ISO).

```csharp
Html.On(plan, t => t.Event("default-move-in"))
    .Then(p => p
        .Component<FusionDatePicker>(m => m.Resident.MoveInDate)
        .SetValue(new DateTime(2026, 1, 1)));
```

### FusionDatePicker.Value() — read as a typed DateTime source
Read the live date for a condition guard.

```csharp
Html.On(plan, t => t.Event("require-move-in"))
    .When(p => p.Component<FusionDatePicker>(m => m.Resident.MoveInDate).Value())
    .IsNull()
    .Then(p => p
        .Element("move-in-error")
        .Show());
```

### FusionNumericTextBox.SetValue(decimal)
Sets the numeric value.

```csharp
Html.On(plan, t => t.Event("apply-base-rate"))
    .Then(p => p
        .Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate)
        .SetValue(4200m));
```

### FusionNumericTextBox.SetMin(decimal)
Sets the control's minimum allowed value.

```csharp
Html.On(plan, t => t.Event("clamp-rate"))
    .Then(p => p
        .Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate)
        .SetMin(1000m));
```

### FusionNumericTextBox.Value() — read as a typed decimal source
Use the live numeric value in a comparison.

```csharp
Html.On(plan, t => t.Event("check-rate"))
    .When(p => p.Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate).Value())
    .Gt(0m)
    .Then(p => p
        .Element("rate-ok")
        .Show());
```

---

## Component methods — call control APIs

Beyond value get/set, each `ComponentRef` exposes the control's methods as typed
calls that emit `Call` reactions.

### FusionDropDownList.ShowPopup() / HidePopup()
Open or close the dropdown popup programmatically.

```csharp
Html.On(plan, t => t.Event("open-care-level"))
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Assessment.CareLevel)
        .ShowPopup());
```

### FusionDropDownList.DataBind()
Flush pending property changes into the rendered control.

```csharp
Html.On(plan, t => t.Event("rebind-care-level"))
    .Then(p => p
        .Component<FusionDropDownList>(m => m.Assessment.CareLevel)
        .SetValue("ASSISTED")
        .DataBind());
```

### FusionNumericTextBox.Increment() / Decrement()
Step the numeric value up or down by one step.

```csharp
Html.On(plan, t => t.Event("bump-rate"))
    .Then(p => p
        .Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate)
        .Increment());
```

### FusionTextBox.AddAppendIcon(cssClass)
Adds an append icon span via Syncfusion's `addIcon` API.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionTextBox>(m => m.Resident.ContactEmail)
        .AddAppendIcon("e-icons e-mail"));
```

---

## Component focus — FocusIn / FocusOut

Inputs expose focus as method calls on their `ComponentRef`. `FocusIn()` moves
focus into the control; `FocusOut()` removes it. (These are the finalized focus
verbs; they emit `Call` reactions like any other method.)

### FusionTextBox.Focus()
Move focus into the textbox after a reaction.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Component<FusionTextBox>(m => m.Resident.FullName)
        .Focus());
```

### FusionTextBox.Blur()
Remove focus from the textbox.

```csharp
Html.On(plan, t => t.Event("dismiss"))
    .Then(p => p
        .Component<FusionTextBox>(m => m.Resident.FullName)
        .Blur());
```

### FusionDropDownList.Focus() — focus a different control after change
Chain focus onto the next field when care level changes.

```csharp
Html.InputField(plan, m => m.Assessment.CareLevel, o => o
        .Label("Care level"))
    .FusionDropDownList(b => b
        .DataSource(careLevels)
        .Fields<CareLevelItem>(t => t.Name, v => v.Code)
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate)
            .Focus()));
```

### FusionDatePicker.Focus() / FocusOut()
Move focus into or out of the date picker.

```csharp
Html.On(plan, t => t.Event("edit-move-in"))
    .Then(p => p
        .Component<FusionDatePicker>(m => m.Resident.MoveInDate)
        .Focus());
```

### FusionNumericTextBox.Focus() / FocusOut()
Move focus into or out of the numeric input.

```csharp
Html.On(plan, t => t.Event("edit-rate"))
    .Then(p => p
        .Component<FusionNumericTextBox>(m => m.Billing.MonthlyRate)
        .Focus());
```

---

## ElementBuilder — mutate any DOM element

`p.Element("id")` returns an `ElementBuilder<TModel>` for non-input display
elements (banners, status spans, panels). It mutates text, inner HTML, CSS
classes, and visibility. Most methods return the pipeline for continued
chaining; the `TypedSource` overloads of `SetText` / `SetHtml` return the element
builder so element mutations can chain.

### Element(id) — target a DOM element by id
Open an element target; chain a mutation.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("welcome-banner")
        .SetText("Welcome to Sunrise"));
```

### SetText(literal)
Set the element's text content to a literal string.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("status")
        .SetText("Ready"));
```

### SetText(args, path) — from an event payload
Set text from a property on the event payload.

```csharp
Html.InputField(plan, m => m.Resident.FullName, o => o
        .Label("Full name"))
    .NativeTextBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Element("name-echo")
            .SetText(args, x => x.Value)));
```

### SetText(body, path) — from an HTTP response body
Set text from a property on a success/error response body.

```csharp
Html.On(plan, t => t.Event("load-summary"))
    .Get("/api/residents/summary")
    .OnSuccess<SummaryResponse>((body, p) => p
        .Element("resident-count")
        .SetText(body, x => x.TotalResidents));
```

### SetText(typedSource) — from a typed source
Set text from a component / plugin / URL value (returns the element builder).

```csharp
Html.On(plan, t => t.Event("echo-care-level"))
    .Then(p => p
        .Element("care-level-echo")
        .SetText(p.Component<FusionDropDownList>(m => m.Assessment.CareLevel).Value()));
```

### SetHtml(literal)
Set the element's inner HTML to a literal string.

```csharp
Html.On(plan, t => t.PageLoad())
    .Then(p => p
        .Element("notice")
        .SetHtml("<strong>Annual review due</strong>"));
```

### SetHtml(args, path) — from an event payload
Set inner HTML from a property on the event payload.

```csharp
Html.On(plan, t => t.Event<NoticePushed>("notice-pushed"))
    .Then(p => p
        .Element("notice")
        .SetHtml(args, x => x.HtmlBody));
```

### SetHtml(typedSource) — from a typed source
Set inner HTML from a typed source (returns the element builder).

```csharp
Html.On(plan, t => t.Event("render-template"))
    .Then(p => p
        .Element("panel")
        .SetHtml(p.Plugin<string>("templates", "render")));
```

### AddClass(className)
Add a CSS class to the element.

```csharp
Html.On(plan, t => t.Event("mark-overdue"))
    .Then(p => p
        .Element("assessment-row")
        .AddClass("overdue"));
```

### RemoveClass(className)
Remove a CSS class from the element.

```csharp
Html.On(plan, t => t.Event("clear-overdue"))
    .Then(p => p
        .Element("assessment-row")
        .RemoveClass("overdue"));
```

### ToggleClass(className)
Toggle a CSS class on the element.

```csharp
Html.InputField(plan, m => m.Resident.IsVeteran, o => o
        .Label("Veteran"))
    .NativeCheckBox(b => b
        .Reactive(plan, e => e.Changed, (args, p) => p
            .Element("veteran-card")
            .ToggleClass("highlighted")));
```

### Show()
Show the element (removes the `hidden` attribute).

```csharp
Html.On(plan, t => t.Event("show-billing"))
    .Then(p => p
        .Element("billing-panel")
        .Show());
```

### Hide()
Hide the element (sets the `hidden` attribute).

```csharp
Html.On(plan, t => t.Event("hide-billing"))
    .Then(p => p
        .Element("billing-panel")
        .Hide());
```

### Chained element mutations across many elements
Each pipeline-returning mutation lets you open another `Element` target.

```csharp
Html.On(plan, t => t.Event("care-level-changed"))
    .Then(p => p
        .Element("care-level-badge")
        .AddClass("changed")
        .Element("billing-panel")
        .Show()
        .Element("save-hint")
        .SetText("Remember to save"));
```

---

## IComponent / IInputComponent / IAppLevelComponent — the component contracts

The marker interfaces every component slice implements. They are not authored
directly in views, but they shape what `Component<T>` accepts and what value
member gather and validation read.

### IComponent — vendor identity
Every component carries a `Vendor` used for DOM resolution (`"native"`, the
Fusion vendor token, etc.). `Component<TComponent>(...)` requires
`TComponent : IComponent, new()`.

```csharp
// A component slice declares its vendor:
internal sealed class FusionTextBox : IComponent
{
    public string Vendor => "fusion";
}
```

### IInputComponent — readable value member
Model-bound inputs add `ValueMember` — the JS member gather and validation read
and that `.Value()` / `.SetValue(...)` target.

```csharp
internal sealed class FusionDropDownList : IInputComponent
{
    public string Vendor => "fusion";
    public string ValueMember => "value";
}
```

### IAppLevelComponent — fixed default id
Layout-owned singletons (Toast, Confirm, Loader) add `DefaultId`; that is what the
parameterless `Component<TComponent>()` resolves.

```csharp
internal sealed class FusionToast : IAppLevelComponent
{
    public string Vendor => "fusion";
    public string DefaultId => "alis-toast";
}
```
