---
title: Syncfusion Components
description: Every Syncfusion EJ2 component in Reactive -- autocomplete, numeric inputs, date/time pickers, dropdowns, multi-selects, switches, and more.
sidebar:
  order: 3
---

Fusion components wrap [Syncfusion EJ2](https://ej2.syncfusion.com/) controls. They use vendor `"fusion"` -- the runtime resolves the root as `el.ej2_instances[0]` (the Syncfusion component instance attached to the DOM element). You never write Syncfusion-specific JavaScript. The C# vertical slice declares property and method names, the plan carries them as data, and the runtime executes via bracket notation.

**Package:** `Alis.Reactive.Fusion`

---

## FusionAutoComplete

A text input with server-side or client-side filtering and autocomplete suggestions. The most feature-rich input component.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed`, `Filtering` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render an autocomplete?

```csharp
Html.InputField(plan, m => m.Physician, o => o.Label("Physician"))
    .AutoComplete(b => b
        .DataSource(physicians)
        .Fields<PhysicianItem>(t => t.Text, v => v.Value)
        .Placeholder("Select a physician"));
```

### How do I set its value?

```csharp
p.Component<FusionAutoComplete>(m => m.Physician).SetValue("smith");
```

### How do I react to selection changes?

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.Element("change-value").SetText(args, x => x.Value);
    p.When(args, x => x.Value).Eq("smith")
        .Then(t => t.Element("condition").SetText("dr smith selected"))
        .Else(e => e.Element("condition").SetText("other physician"));
})
```

### How do I wire server-side filtering?

The `Filtering` event fires as the user types. Use it to fetch results from an API and feed them back:

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/api/physicians?q=", g => g.Include(m => m.Physician))
     .Response(r => r.OnSuccess<PhysicianResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Physicians);
     }));
})
```

Key points:
- `args.PreventDefault(p)` stops Syncfusion's built-in client-side filtering
- `args.UpdateData(s, json, path)` calls the SF `updateData()` API to replace suggestions
- No `DataBind()` needed -- `updateData` handles the refresh internally

### How do I cascade from another component?

When one component's selection should reload this AutoComplete's data, use `SetDataSource` + `DataBind`:

```csharp
// In the parent component's Changed event:
p.Get("/api/physicians?dept=", g => g.Include(m => m.Department))
 .Response(r => r.OnSuccess<PhysicianResponse>((json, s) =>
 {
     s.Component<FusionAutoComplete>(m => m.Physician)
         .SetDataSource(json, j => j.Physicians)
         .DataBind();
 }));
```

`SetDataSource` + `DataBind()` is required in the cascade pattern because you are setting a property and need to flush the change.

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending property changes to the SF instance |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the suggestion dropdown |
| `Enable()` / `Disable()` | Enable or disable the component |

---

## FusionNumericTextBox

A numeric input with spin buttons, formatting, and min/max constraints.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed`, `Focus`, `Blur` |
| Typed Source | `TypedComponentSource<decimal>` |

### How do I render a numeric input?

```csharp
Html.InputField(plan, m => m.Amount, o => o.Label("Amount"))
    .NumericTextBox(b => b
        .Min(-100).Max(99999).Step(1));
```

### How do I set its value?

Note the `decimal` type -- the framework handles coercion to `"number"` in the plan:

```csharp
p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42m);
p.Component<FusionNumericTextBox>(m => m.Amount).SetMin(0m);
```

### How do I read its value as a source?

```csharp
var comp = p.Component<FusionNumericTextBox>(m => m.Amount);
p.Element("value-echo").SetText(comp.Value());
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(decimal)` | Sets the value (coerced to number in the plan) |
| `SetMin(decimal)` | Sets the minimum allowed value |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `Increment()` | Increases value by the step amount |
| `Decrement()` | Decreases value by the step amount |

---

## FusionDatePicker

A date-only picker with calendar popup.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<DateTime>` |

### How do I render a date picker?

```csharp
Html.InputField(plan, m => m.AdmissionDate, o => o.Label("Admission Date"))
    .DatePicker(b => b
        .Placeholder("Select admission date")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Value);
            p.When(args, x => x.Value).NotNull()
                .Then(t => t.Element("condition").SetText("date selected"))
                .Else(e => e.Element("condition").SetText("no date"));
        }));
```

### How do I set its value?

```csharp
p.Component<FusionDatePicker>(m => m.AdmissionDate)
    .SetValue(new DateTime(2026, 6, 15));
```

The `DateTime` is serialized as `"yyyy-MM-dd"` in the plan.

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(DateTime)` | Sets the date |
| `FocusIn()` / `FocusOut()` | Manage focus |

---

## FusionDateTimePicker

A combined date and time picker.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<DateTime>` |

### How do I render a datetime picker?

```csharp
Html.InputField(plan, m => m.AppointmentTime, o => o.Label("Appointment Time"))
    .DateTimePicker(b => b
        .Placeholder("Select date and time"));
```

### How do I set its value?

The `DateTime` is serialized as `"yyyy-MM-ddTHH:mm"` in the plan:

```csharp
p.Component<FusionDateTimePicker>(m => m.AppointmentTime)
    .SetValue(new DateTime(2026, 4, 1, 14, 30, 0));
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(DateTime)` | Sets the date and time |
| `FocusIn()` / `FocusOut()` | Manage focus |

---

## FusionTimePicker

A time-only picker.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<DateTime>` |

### How do I render a time picker?

```csharp
Html.InputField(plan, m => m.MedicationTime, o => o.Label("Medication Time"))
    .TimePicker(b => b
        .Placeholder("Select time"));
```

### How do I set its value?

The `DateTime` is serialized as `"HH:mm"` in the plan:

```csharp
p.Component<FusionTimePicker>(m => m.MedicationTime)
    .SetValue(new DateTime(2026, 1, 1, 8, 30, 0));
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(DateTime)` | Sets the time |
| `FocusIn()` / `FocusOut()` | Manage focus |

---

## FusionDateRangePicker

A picker for selecting a start and end date. Unique among components because the Syncfusion ej2 instance's `.value` returns `[Date, Date]` — an array of two Date objects (start + end).

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` — returns `[Date, Date]` from ej2 instance |
| Model type | `DateTime[]?` — array matches the Syncfusion value shape |
| CoerceAs | `"array"` (inferred from `DateTime[]`) with element type `"date"` |
| Events | `Changed` |
| Typed Sources | `StartDate()` / `EndDate()` → `TypedComponentSource<DateTime>` (individual dates), `Value()` → `TypedComponentSource<DateTime[]>` (full array) |

### How do I render a date range picker?

```csharp
// Model property is DateTime[] — matches Syncfusion [Date, Date] value
public DateTime[]? StayPeriod { get; set; }

// View
Html.InputField(plan, m => m.StayPeriod, o => o.Required().Label("Stay Period"))
    .DateRangePicker(b => b
        .Placeholder("Select date range"));
```

### How do I read individual dates in conditions?

`StartDate()` and `EndDate()` use hardcoded readExpr `"startDate"` / `"endDate"` — independent of the component's ReadExpr. They return individual `DateTime` values for typed condition comparison.

```csharp
var stay = p.Component<FusionDateRangePicker>(m => m.StayPeriod);

p.When(stay.StartDate()).NotNull()
    .Then(t => t.Element("start-echo").SetText(stay.StartDate()));

p.When(stay.EndDate()).NotNull()
    .Then(t => t.Element("end-echo").SetText(stay.EndDate()));
```

### How does gather work?

`IncludeAll()` reads `ej2.value` → `[Date, Date]`. Gather's `emitArray` iterates each Date and serializes via `toString()` → ISO 8601.

- **JSON POST:** `{ "StayPeriod": ["2026-07-01T...", "2026-07-15T..."] }` — ASP.NET binds `DateTime[]` natively
- **FormData:** repeated key `StayPeriod=ISO&StayPeriod=ISO` — ASP.NET binds arrays from repeated keys
- **GET:** repeated params — same pattern

### Source extensions

| Extension | Returns | ReadExpr | Use case |
|-----------|---------|----------|----------|
| `StartDate()` | `TypedComponentSource<DateTime>` | `"startDate"` | Individual date for conditions/mutations |
| `EndDate()` | `TypedComponentSource<DateTime>` | `"endDate"` | Individual date for conditions/mutations |
| `Value()` | `TypedComponentSource<DateTime[]>` | `"value"` | Full array for gather/validation |

No `SetValue()` is provided — the DateRangePicker is set by user interaction only.

---

## FusionDropDownList

A single-select dropdown with search and filtering.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed`, `Focus`, `Blur` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a dropdown?

```csharp
Html.InputField(plan, m => m.Category, o => o.Label("Category"))
    .DropDownList(b => b
        .DataSource(categories)
        .Placeholder("Select a category")
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Value);
            p.When(args, x => x.Value).Eq("Electronics")
                .Then(t => t.Element("condition").SetText("electronics selected"))
                .Else(e => e.Element("condition").SetText("other category"));
        }));
```

### How do I use typed fields?

When your data source has complex objects, use `.Fields<T>()` to bind text and value:

```csharp
.DropDownList(b => b
    .DataSource(facilityRecords)
    .Fields<FacilityRecord>(x => x.Name, x => x.Id)
    .Placeholder("Select facility"))
```

### How do I cascade dropdowns?

When one dropdown's selection should reload another:

```csharp
.Reactive(plan, evt => evt.Changed, (args, p) =>
{
    p.Get("/api/wings?facility=", g => g.Include(m => m.FacilityId))
     .Response(r => r.OnSuccess<WingResponse>((json, s) =>
     {
         s.Component<FusionDropDownList>(m => m.WingId)
             .SetDataSource(json, j => j.Wings)
             .DataBind();
     }));
})
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending property changes |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the dropdown |

---

## FusionMultiSelect

A multi-select dropdown. Selected values are a `string[]`.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed`, `Filtering` |
| Typed Source | `TypedComponentSource<string[]>` |

### How do I render a multi-select?

```csharp
Html.InputField(plan, m => m.Allergies, o => o.Label("Allergies"))
    .MultiSelect(b => b
        .DataSource(allergies)
        .Fields<AllergyItem>(x => x.Text, x => x.Value));
```

### How do I read the selected values?

```csharp
var comp = p.Component<FusionMultiSelect>(m => m.Allergies);
p.Element("value-echo").SetText(comp.Value());
```

### How do I wire server-side filtering?

Same pattern as AutoComplete -- the `Filtering` event fires as the user types:

```csharp
.Reactive(plan, evt => evt.Filtering, (args, p) =>
{
    args.PreventDefault(p);
    p.Get("/api/supplies?q=", g => g.Include(m => m.Supplies))
     .Response(r => r.OnSuccess<SuppliesResponse>((json, s) =>
     {
         args.UpdateData(s, json, j => j.Items);
     }));
})
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string[]?)` | Sets the selected values |
| `SetDataSource(source, path)` | Sets the data source |
| `DataBind()` | Flushes pending changes |
| `ShowPopup()` / `HidePopup()` | Open or close the popup |

---

## FusionMultiColumnComboBox

A combo box that displays multiple columns in its dropdown -- useful for showing structured data (name + ID + department).

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<string>` |

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string?)` | Sets the selected value |
| `SetText(string)` | Sets the display text |
| `SetDataSource(source, path)` | Sets data source from event payload or response body |
| `DataBind()` | Flushes pending changes |
| `FocusIn()` / `FocusOut()` | Manage focus |
| `ShowPopup()` / `HidePopup()` | Open or close the dropdown |

---

## FusionSwitch

A toggle switch. Uses `ReadExpr => "checked"`, same concept as NativeCheckBox but rendered as a Syncfusion Switch control.

| Property | Value |
|----------|-------|
| ReadExpr | `"checked"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<bool>` |

### How do I render a switch?

```csharp
Html.InputField(plan, m => m.ReceiveNotifications, o => o.Label("Receive Notifications"))
    .Switch(b => b
        .Reactive(plan, evt => evt.Changed, (args, p) =>
        {
            p.Element("change-value").SetText(args, x => x.Checked);
            p.When(args, x => x.Checked).Truthy()
                .Then(t => t.Element("condition").SetText("notifications enabled"))
                .Else(e => e.Element("condition").SetText("notifications disabled"));
        }));
```

### How do I set its checked state?

```csharp
p.Component<FusionSwitch>(m => m.ReceiveNotifications).SetChecked(false);
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetChecked(bool)` | Sets the checked state (coerced to boolean in the plan) |

---

## FusionInputMask

A masked text input that enforces a specific format (phone numbers, SSNs, zip codes).

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a masked input?

```csharp
Html.InputField(plan, m => m.PhoneNumber, o => o.Label("Phone Number"))
    .InputMask(b => b
        .Mask("(000) 000-0000"));
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string)` | Sets the unmasked value |
| `FocusIn()` | Moves focus into the input |

---

## FusionRichTextEditor

A WYSIWYG rich text editor.

| Property | Value |
|----------|-------|
| ReadExpr | `"value"` |
| Events | `Changed` |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a rich text editor?

```csharp
Html.InputField(plan, m => m.CarePlanNotes, o => o.Label("Care Plan"))
    .RichTextEditor(b => b);
```

### Mutation extensions

| Extension | Description |
|-----------|-------------|
| `SetValue(string)` | Sets the HTML content |
| `FocusIn()` | Moves focus into the editor |

---

## FusionFileUpload

A file upload component in form mode (no auto-upload). Read-only -- the framework can read `filesData` but does not provide a `SetValue` method. Files are set by user interaction.

| Property | Value |
|----------|-------|
| ReadExpr | `"filesData"` |
| Events | `Selected` (fires when files are chosen) |
| Typed Source | `TypedComponentSource<string>` |

### How do I render a file upload?

```csharp
Html.InputField(plan, m => m.Documents, o => o.Label("Supporting Documents"))
    .FileUpload(b => b);
```

---

## App-Level Fusion Components

App-level Fusion components are singletons rendered once in the layout. Referenced from any pipeline without a model expression.

### FusionToast

Toast notifications. Rendered in the layout via `@Html.FusionToast()`.

### How do I show a toast?

Configure properties, set a type, then call `Show()`:

```csharp
p.Component<FusionToast>()
    .SetTitle("Resident Saved")
    .SetContent("Jane Doe has been admitted to Assisted Living")
    .Success()
    .Show();
```

### What toast types are available?

| Method | CSS Class |
|--------|-----------|
| `Success()` | `e-toast-success` (green) |
| `Warning()` | `e-toast-warning` (yellow) |
| `Danger()` | `e-toast-danger` (red) |
| `Info()` | `e-toast-info` (blue) |

### All toast extensions

| Extension | Description |
|-----------|-------------|
| `SetTitle(string)` | Sets the toast title |
| `SetContent(string)` | Sets the toast body text |
| `SetTimeout(int)` | Auto-dismiss after N milliseconds |
| `ShowCloseButton()` | Shows the close button |
| `ShowProgressBar()` | Shows the auto-dismiss progress bar |
| `Success()` / `Warning()` / `Danger()` / `Info()` | Sets the toast type styling |
| `Show()` | Displays the toast (calls `dataBind` + `show` on the SF instance) |
| `Hide()` | Hides the toast |

### FusionConfirm

A confirmation dialog. Rendered in the layout via `@Html.FusionConfirmDialog()`.

### How do I use it?

```csharp
p.Component<FusionConfirm>()
    .SetContent("Are you sure you want to discharge this resident?")
    .Show();
```

| Extension | Description |
|-----------|-------------|
| `SetContent(string)` | Sets the dialog message (calls `dataBind` after setting) |
| `Show()` | Opens the dialog |
| `Hide()` | Closes the dialog |
