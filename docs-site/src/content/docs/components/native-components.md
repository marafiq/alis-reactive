---
title: Native Components
description: Every native HTML component in Reactive -- text inputs, checkboxes, dropdowns, textareas, checkbox lists, radio groups, buttons, and hidden fields.
sidebar:
  order: 2
---

Native components wrap standard HTML elements. They use vendor `"native"` -- the runtime reads values directly from the DOM element (`el.value`, `el.checked`) without any vendor-specific root resolution.

**Package:** `Alis.Reactive.Native`

---

## NativeTextBox

Renders an `<input type="text">` element.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` (DOM `"change"`) |
| EventArgs | `NativeTextBoxChangeArgs` -- `Value: string?` |
| Extensions | `SetValue(string)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a text input?

```csharp
Html.InputField(plan, m => m.ResidentName, o => o.Label("Resident Name"))
    .NativeTextBox(b => b
        .Placeholder("Enter resident name")
        .CssClass("rounded-md border border-border px-3 py-1.5 text-sm"));
```

### How do I set its value from a pipeline?

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Component<NativeTextBox>(m => m.ResidentName).SetValue("Jane Doe");
}));
```

### How do I react to changes?

Wire the `Changed` event with `.Reactive()`. The event args give you typed access to `Value`:

```csharp
Html.InputField(plan, m => m.EmergencyContact, o => o.Label("Emergency Contact"))
    .NativeTextBox(b => b
        .Placeholder("Enter emergency contact")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, x => x.Value).IsEmpty()
                .Then(t =>
                {
                    t.Element("contact-status").SetText("contact required");
                    t.Element("contact-status").AddClass("text-red-600");
                })
                .Else(e =>
                {
                    e.Element("contact-status").SetText("contact provided");
                    e.Element("contact-status").AddClass("text-green-600");
                });
        }));
```

### How do I read its value from a button click?

Use `comp.Value()` as a condition source:

```csharp
var comp = p.Component<NativeTextBox>(m => m.ResidentName);
p.When(comp.Value()).IsEmpty()
    .Then(t => t.Element("warning").SetText("name is required"))
    .Else(e => e.Element("warning").SetText("name set"));
```

### Builder methods

| Method | Description |
|--------|-------------|
| `.Placeholder(string)` | Sets the placeholder text |
| `.CssClass(string)` | Sets the CSS class attribute |
| `.Type(string)` | Sets the input type (default: `"text"`, also `"email"`, `"password"`, `"number"`) |

---

## NativeCheckBox

Renders an `<input type="checkbox">` element.

| Property | Value |
|----------|-------|
| ReadExpr | `"checked"` |
| Events | `Changed` (DOM `"change"`) |
| EventArgs | `NativeCheckBoxChangeArgs` -- `Checked: bool?` |
| Extensions | `SetChecked(bool)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<bool>` |

### How do I render a checkbox?

```csharp
Html.InputField(plan, m => m.ReceivesMedication, o => o.Label("Receives Medication"))
    .NativeCheckBox(b => b.CssClass("h-4 w-4 rounded border-border text-accent"));
```

### How do I set its checked state?

```csharp
p.Component<NativeCheckBox>(m => m.ReceivesMedication).SetChecked(false);
```

### How do I react to changes?

The event args expose `Checked` as a boolean:

```csharp
Html.InputField(plan, m => m.HasDietaryRestrictions, o => o.Label("Has Dietary Restrictions"))
    .NativeCheckBox(b => b
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, a => a.Checked).Truthy()
                .Then(t =>
                {
                    t.Element("restrictions-panel").Show();
                    t.Element("restrictions-status").SetText("checked");
                })
                .Else(e =>
                {
                    e.Element("restrictions-panel").Hide();
                    e.Element("restrictions-status").SetText("unchecked");
                });
        }));
```

### How do I check its value from a button click?

```csharp
var comp = p.Component<NativeCheckBox>(m => m.ReceivesMedication);
p.When(comp.Value()).Truthy()
    .Then(t => t.Element("status").SetText("resident receives medication"))
    .Else(e => e.Element("status").SetText("no medication on record"));
```

---

## NativeDropDown

Renders a `<select>` element with `<option>` children.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` (DOM `"change"`) |
| EventArgs | `NativeDropDownChangeArgs` -- `Value: string?` |
| Extensions | `SetValue(string)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a dropdown?

Pass a `SelectListItem[]` from your controller:

```csharp
Html.InputField(plan, m => m.CareLevel, o => o.Label("Care Level"))
    .NativeDropDown(b => b
        .Items(careLevelItems)
        .Placeholder("-- Select Care Level --")
        .CssClass("rounded-md border border-border px-3 py-1.5 text-sm"));
```

### How do I set the selected value?

```csharp
p.Component<NativeDropDown>(m => m.CareLevel).SetValue("Memory Care");
```

### How do I react to changes?

```csharp
Html.InputField(plan, m => m.FacilityType, o => o.Label("Facility Type"))
    .NativeDropDown(b => b
        .Items(facilityTypeItems)
        .Placeholder("-- Select Facility Type --")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, x => x.Value).Eq("Medical")
                .Then(t => t.Element("medical-notice").SetText("medical facility selected"))
                .Else(e => e.Element("medical-notice").SetText("not a medical facility"));
        }));
```

### Builder methods

| Method | Description |
|--------|-------------|
| `.Items(SelectListItem[])` | Sets the dropdown options |
| `.Placeholder(string)` | Sets the empty-state placeholder text |
| `.CssClass(string)` | Sets the CSS class attribute |

---

## NativeTextArea

Renders a `<textarea>` element. Same API shape as NativeTextBox, with multi-line support.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` (DOM `"change"`) |
| EventArgs | `NativeTextAreaChangeArgs` -- `Value: string?` |
| Extensions | `SetValue(string)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a textarea?

```csharp
Html.InputField(plan, m => m.CareNotes, o => o.Label("Care Notes"))
    .NativeTextArea(b => b
        .Rows(4)
        .Placeholder("Enter care notes")
        .CssClass("rounded-md border border-border px-3 py-1.5 text-sm w-full"));
```

### How do I set its value?

```csharp
p.Component<NativeTextArea>(m => m.CareNotes)
    .SetValue("Resident stable. Vitals within normal range.");
```

### How do I react to changes?

```csharp
Html.InputField(plan, m => m.IncidentDescription, o => o.Label("Incident Description"))
    .NativeTextArea(b => b
        .Rows(4)
        .Placeholder("Describe the incident")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.When(args, x => x.Value).IsEmpty()
                .Then(t => t.Element("incident-status").SetText("no incident"))
                .Else(e => e.Element("incident-status").SetText("incident logged"));
        }));
```

### Builder methods

| Method | Description |
|--------|-------------|
| `.Rows(int)` | Sets the visible row count |
| `.Placeholder(string)` | Sets the placeholder text |
| `.CssClass(string)` | Sets the CSS class attribute |

---

## NativeCheckList

Renders a group of checkboxes for multi-select. The canonical element is a container `<div>` whose `.value` (managed by the framework's `checklist.ts`) holds a `string[]`.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| EventArgs | `NativeCheckListChangeArgs` -- `Value: string[]?` |
| Extensions | `SetValue(string[])`, `SetValue(source, path)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<string[]>` |

### How do I render a checkbox list?

Two variations: rich items with text + description, or simple text-only items.

```csharp
// Rich items with description
Html.InputField(plan, m => m.Allergies, o => o.Label("Allergies"))
    .NativeCheckList(b => b
        .Items(allergyItems)
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("allergy-echo").SetText(args, a => a.Value);
        }));

// Simple text-only items
Html.InputField(plan, m => m.Amenities, o => o.Label("Amenities"))
    .NativeCheckList(b => b.Items(amenityItems));
```

`Items` takes a `RadioButtonItem[]` with `Value`, `Text`, and optional `Description` properties.

### How do I set the selected values?

```csharp
p.Component<NativeCheckList>(m => m.Allergies)
    .SetValue(new[] { "Peanuts", "Dairy" });
```

### How do I check if anything is selected?

```csharp
var comp = p.Component<NativeCheckList>(m => m.Allergies);
p.When(comp.Value()).NotEmpty()
    .Then(t => t.Element("status").SetText("allergies recorded"))
    .Else(e => e.Element("status").SetText("no allergies selected"));
```

---

## NativeRadioGroup

Renders a group of radio buttons for single-select. The canonical element is a hidden `<input>` whose `.value` holds the selected radio's value.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| EventArgs | `NativeRadioGroupChangeArgs` -- `Value: string?` |
| Extensions | `SetValue(string)`, `SetValue(source, path)`, `FocusIn()`, `.Value()` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a radio group?

Two approaches: inline `.Option()` calls, or a `RadioButtonItem[]` array.

```csharp
// Inline options with descriptions
Html.InputField(plan, m => m.CareLevel, o => o.Required().Label("Care Level"))
    .NativeRadioGroup(b => b
        .Option("Assisted Living", "Assisted Living",
            "Daily assistance with meals, bathing, and medication management")
        .Option("Memory Care", "Memory Care",
            "Specialized care for Alzheimer's and other cognitive conditions")
        .Option("Independent Living", "Independent Living",
            "Self-sufficient lifestyle with optional community services"));

// From RadioButtonItem[] array
Html.InputField(plan, m => m.RoomType, o => o.Required().Label("Room Type"))
    .NativeRadioGroup(b => b.Items(roomTypeItems));
```

### How do I react to selection?

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.When(args, x => x.Value).Eq("Memory Care")
        .Then(t => t.Element("notice").SetText("assessment score required"))
        .Else(e => e.Element("notice").SetText("standard admission process"));
})
```

### How do I echo the selected value?

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.Element("meal-echo").SetText(args, a => a.Value);
})
```

---

## NativeButton

Renders a `<button>` element. **Not an input component** -- buttons have no form value, no `ReadExpr`, no `.Value()` source. They exist to trigger pipelines.

| Property | Value |
|----------|-------|
| Implements | `IComponent` (not `IInputComponent`) |
| Events | `Click` (DOM `"click"`) |
| EventArgs | `NativeButtonClickArgs` (empty -- no payload) |
| Extensions | `SetText(string)`, `FocusIn()` |

### How do I render a button with a click handler?

Buttons are created via `Html.NativeButton`, not `Html.InputField`:

```csharp
@(Html.NativeButton("btn-admit", "Admit Resident")
    .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Element("admit-status").SetText("Admit Resident clicked");
        p.Element("admit-status").AddClass("text-green-600");
    }))
```

Note: the `@(...)` Razor expression is needed because the button returns `IHtmlContent` directly.

### How do I change a button's text at runtime?

Reference the button by its string ID (buttons are not bound to model properties):

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Component<NativeButton>("btn-admit-text").SetText("Admit Resident");
}));
```

### How do I dispatch an event from a button?

```csharp
@(Html.NativeButton("btn-transfer", "Transfer Resident")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Dispatch("resident-transferred");
    }))
```

### How do I focus another component from a button click?

```csharp
@(Html.NativeButton("btn-focus-trigger", "Focus Discharge")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Component<NativeButton>("btn-discharge").FocusIn();
    }))
```

---

## NativeActionLink

Renders an `<a>` element that triggers an HTTP pipeline on click. **Not an input component** -- action links have no form value, no `ReadExpr`, no `.Value()` source. They exist to make links that execute server requests declaratively.

| Property | Value |
|----------|-------|
| Implements | Standalone (not `IComponent` or `IInputComponent`) |
| Rendered | `<a>` with `data-reactive-link` attribute |

### How do I render an action link?

Action links are created via `Html.NativeActionLink`, not `Html.InputField`. The pipeline must contain exactly one HTTP request:

```csharp
@Html.NativeActionLink("Delete Resident", "/api/residents/delete/42", pipeline =>
{
    pipeline.Post("/api/residents/delete/42", g => g.Static("id", 42))
        .WhileLoading(l => l.Element("status").SetText("Deleting..."))
        .Response(r => r
            .OnSuccess(p => p.Element("status").SetText("Deleted"))
            .OnError(400, e => e.Element("status").SetText("Delete failed")));
})
    .CssClass("text-red-600 hover:text-red-700")
```

The `url` parameter must match the request URL in the pipeline.

### How do I add a confirmation dialog?

Wrap the HTTP request in a `Confirm` guard:

```csharp
@Html.NativeActionLink("Discharge", "/api/residents/discharge/42", pipeline =>
{
    pipeline.Confirm("Are you sure you want to discharge this resident?")
        .Then(then =>
        {
            then.Post("/api/residents/discharge/42", g => g.Static("id", 42))
                .Response(r => r.OnSuccess(p => p.Dispatch("resident-discharged")));
        });
})
    .CssClass("text-amber-600 hover:text-amber-700")
```

### Builder methods

| Method | Description |
|--------|-------------|
| `.CssClass(string)` | Sets CSS classes on the anchor element |
| `.Attr(string name, string value)` | Adds a custom HTML attribute (cannot override `id`, `href`, or `data-reactive-link`; `class` is redirected to `.CssClass()`) |

---

## NativeHiddenField

Renders an `<input type="hidden">` element. No label, no validation slot, no visible presence. Participates in `ComponentsMap` so `IncludeAll()` gathers its value.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Extensions | `SetValue(string)`, `.Value()` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a hidden field?

Use `Html.HiddenFieldFor` directly -- no `InputField` wrapper:

```csharp
@Html.HiddenFieldFor(plan, m => m.ResidentId)
@Html.HiddenFieldFor(plan, m => m.FormToken)
```

### How do I read a hidden field's value?

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Element("resident-id-echo").SetText(
        p.Component<NativeHiddenField>(m => m.ResidentId).Value());
}));
```

### How do hidden fields participate in form submission?

They are gathered automatically by `IncludeAll()`:

```csharp
p.Post("/Sandbox/NativeHiddenField/Echo", g => g.IncludeAll())
    .Response(r => r.OnSuccess<EchoResponse>((json, s) =>
    {
        s.Element("echo-resident-id").SetText(json, x => x.ResidentId);
        s.Element("echo-form-token").SetText(json, x => x.FormToken);
    }));
```

---

## App-Level Native Components

App-level components are singletons -- one instance per page with a well-known element ID. Rendered once in the layout, referenced from any pipeline without a model expression.

### NativeDrawer

A slide-out panel for secondary content. Rendered in the layout via `@Html.NativeDrawer()`.

| Extension | Description |
|-----------|-------------|
| `Open()` | Shows the drawer (adds visible class, removes `aria-hidden`) |
| `Close()` | Hides the drawer |
| `SetSize(DrawerSize)` | Sets width: `DrawerSize.Sm` (28rem), `DrawerSize.Md` (36rem), `DrawerSize.Lg` (48rem) |

```csharp
p.Element("alis-drawer-title").SetText("Resident Details");
p.Component<NativeDrawer>().SetSize(DrawerSize.Sm);
p.Get("/Sandbox/Drawer/ResidentDetails")
    .Response(r => r.OnSuccess(s => s.Into("alis-drawer-content")));
p.Component<NativeDrawer>().Open();
```

### NativeLoader

A loading overlay. Rendered in the layout via `@Html.NativeLoader()`. Covers its target element, or the viewport if no target is specified.

| Extension | Description |
|-----------|-------------|
| `Show()` | Shows the loader overlay |
| `Hide()` | Hides the loader overlay |
| `SetTarget(string)` | Sets which element to cover (by ID) |
| `SetTimeout(int)` | Auto-hide after N milliseconds |

```csharp
p.Component<NativeLoader>().Show();
p.Post("/api/residents/save", g => g.IncludeAll())
    .Response(r =>
    {
        r.OnSuccess(s => s.Component<NativeLoader>().Hide());
        r.OnError(500, e => e.Component<NativeLoader>().Hide());
    });
```
