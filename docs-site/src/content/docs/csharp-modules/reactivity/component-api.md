---
title: Component API
description: Targeting input components, non-input components, and app-level singletons in the pipeline — write properties, call methods, read values.
sidebar:
  order: 2
---

Components are targeted through `pipeline.Component<TComponent>()` inside any pipeline. Unlike `pipeline.Element("id")` which targets raw DOM elements, `Component<TComponent>` gives you typed access — IntelliSense constrains exactly which operations are available for each component type.

From the [Grammar Tree](/csharp-modules/mental-model/#the-grammar-tree) — the component API:

```
pipeline.Component<TComponent>(m => m.Prop)             § input components (TModel inferred)
├── .Set{Prop}(value)                                   write a property
├── .{Method}()                                         call a method
├── .Value()                                            read for conditions

pipeline.Component<NativeButton>("id")                  § non-input (by string ID)
├── .SetText("...") / .FocusIn()

pipeline.Component<TComponent>()                        § app-level (no model binding)
├── FusionToast, FusionConfirm, NativeDrawer, NativeLoader
```

## Input components — by model expression

You target the same model expression you used to render it with `InputField`. The binding is natural — `m => m.Country` in the pipeline finds the `InputField(plan, m => m.Country)` component on the page.

```csharp
pipeline.Component<FusionDropDownList>(m => m.FacilityId).SetValue("123");
pipeline.Component<FusionNumericTextBox>(m => m.RoomNumber).SetValue(204m);
pipeline.Component<NativeCheckBox>(m => m.IsActive).SetChecked(true);
```

`TModel` is inferred from the plan — the expression `m => m.FacilityId` resolves to the DOM element ID using the same convention as ASP.NET's `Html.IdFor()`: `"FacilityId"` for simple properties, `"Address_City"` for nested ones.

### Property writes — .Set{Prop}(value)

Each component type defines which properties can be written. The value type is enforced by the compiler.

```csharp
// String values — text inputs, dropdowns, autocomplete
pipeline.Component<NativeTextBox>(m => m.Name).SetValue("Jane Doe");
pipeline.Component<FusionDropDownList>(m => m.Country).SetValue("US");

// Decimal — numeric inputs
pipeline.Component<FusionNumericTextBox>(m => m.Temperature).SetValue(98.6m);

// DateTime — date/time pickers
pipeline.Component<FusionDatePicker>(m => m.AdmissionDate).SetValue(DateTime.Today);

// Boolean — checkboxes, switches
pipeline.Component<NativeCheckBox>(m => m.IsActive).SetChecked(true);
pipeline.Component<FusionSwitch>(m => m.HasInsurance).SetChecked(false);
```

Source-bound writes — set a value from an event payload:

```csharp
pipeline.Component<NativeCheckList>(m => m.Services)
    .SetValue(args, a => a.Value);
```

### Data source operations — list components

For dropdown, autocomplete, multi-select, and multi-column combo box components, you can replace the data source and refresh:

```csharp
pipeline.Component<FusionDropDownList>(m => m.City)
    .SetDataSource(json, j => j.Cities)
    .DataBind();
```

`SetDataSource` accepts data from an event payload or an HTTP response body. `DataBind()` must be called after `SetDataSource` to refresh the component's rendering. Both chain fluently.

Display text can also be set directly:

```csharp
pipeline.Component<FusionDropDownList>(m => m.Country).SetText("United States");
pipeline.Component<FusionAutoComplete>(m => m.Medication).SetText("Aspirin");
```

### Method calls — .{Method}()

Common operations across components:

```csharp
// Focus management
pipeline.Component<NativeTextBox>(m => m.Name).FocusIn();
pipeline.Component<FusionDropDownList>(m => m.Country).FocusOut();

// Popup control — list components
pipeline.Component<FusionDropDownList>(m => m.Country).ShowPopup();
pipeline.Component<FusionAutoComplete>(m => m.Search).HidePopup();

// Numeric stepping
pipeline.Component<FusionNumericTextBox>(m => m.Quantity).Increment();
pipeline.Component<FusionNumericTextBox>(m => m.Quantity).Decrement();

// Numeric range
pipeline.Component<FusionNumericTextBox>(m => m.Temperature).SetMin(0m);

// Enable/Disable — AutoComplete
pipeline.Component<FusionAutoComplete>(m => m.Search).Enable();
pipeline.Component<FusionAutoComplete>(m => m.Search).Disable();
```

### Reads — .Value()

`.Value()` returns a `TypedComponentSource<TProp>` — a typed reference to the component's current value on the page. Use it in conditions or pass it to `SetText`:

```csharp
var country = pipeline.Component<FusionDropDownList>(m => m.Country);

// In conditions
pipeline.When(country.Value()).Eq("US")
    .Then(then => then.Element("country-label").SetText("United States"));

// In SetText
pipeline.Element("selected-country").SetText(country.Value());
```

The return type matches the component's semantic type:

| Component | .Value() type |
|-----------|--------------|
| NativeTextBox, FusionDropDownList, FusionAutoComplete | `TypedComponentSource<string>` |
| NativeCheckList, FusionMultiSelect | `TypedComponentSource<string[]>` |
| NativeCheckBox, FusionSwitch | `TypedComponentSource<bool>` |
| FusionNumericTextBox | `TypedComponentSource<decimal>` |
| FusionDatePicker, FusionTimePicker | `TypedComponentSource<DateTime>` |

FusionDateRangePicker exposes two reads:

```csharp
var range = pipeline.Component<FusionDateRangePicker>(m => m.StayPeriod);
pipeline.When(range.StartDate()).NotNull()
    .Then(then => then.Element("start").SetText("Start date selected"));
pipeline.When(range.EndDate()).NotNull()
    .Then(then => then.Element("end").SetText("End date selected"));
```

## Non-input components — by string ID

Components that don't bind to a model property — buttons and hidden fields. Buttons use a string ID, hidden fields use the model expression.

```csharp
// NativeButton — by string ID (matches Html.NativeButton("save-btn", "Save"))
pipeline.Component<NativeButton>("save-btn").SetText("Saving...");
pipeline.Component<NativeButton>("save-btn").FocusIn();

// NativeHiddenField — model-bound (matches Html.NativeHiddenField(plan, m => m.Id))
pipeline.Component<NativeHiddenField>(m => m.Id).SetValue("42");
var hiddenId = pipeline.Component<NativeHiddenField>(m => m.Id).Value();
```

## App-level components — by well-known ID

Singletons that live in `_Layout.cshtml` — shared and accessible across all views. Target them with no argument — the framework resolves the well-known ID automatically.

### FusionToast — notifications

```csharp
pipeline.Component<FusionToast>()
    .SetTitle("Intake Complete")
    .SetContent("Resident Jane Doe has been admitted")
    .SetTimeout(5000)
    .ShowCloseButton()
    .ShowProgressBar()
    .Success()
    .Show();
```

Severity methods: `.Success()`, `.Warning()`, `.Danger()`, `.Info()`. Call `.Show()` to display, `.Hide()` to dismiss.

### FusionConfirm — confirmation dialog

```csharp
pipeline.Component<FusionConfirm>()
    .SetContent("Are you sure you want to discharge this resident?")
    .Show();
```

### NativeDrawer — slide-out panel

```csharp
pipeline.Component<NativeDrawer>()
    .SetSize(DrawerSize.Lg)
    .Open();

// Later — close it
pipeline.Component<NativeDrawer>().Close();
```

Sizes: `DrawerSize.Sm`, `DrawerSize.Md`, `DrawerSize.Lg`.

### NativeLoader — loading overlay

```csharp
pipeline.Component<NativeLoader>()
    .SetTarget("form-section")
    .SetTimeout(10000)
    .Show();

// Later — hide it
pipeline.Component<NativeLoader>().Hide();
```

`.SetTarget("elementId")` scopes the overlay to a specific section instead of the full page.

**Next:** [Conditions](/csharp-modules/reactivity/conditions/) — runtime branching with When/Then/ElseIf/Else and guard composition.
