---
title: Components Overview
description: Every input component available in Reactive -- native HTML and Syncfusion EJ2 -- with their key properties and methods.
sidebar:
  order: 1
---

## What does `Html.InputField` do?

It creates a labeled, validation-ready wrapper around any input component. You pick the model property, set options, then chain the component builder:

```csharp
Html.InputField(plan, m => m.ResidentName, o => o.Required().Label("Resident Name"))
    .NativeTextBox(b => b.Placeholder("Enter name"));
```

That single call does three things: registers the component in the plan's `ComponentsMap`, renders the HTML with a label and validation slot, and makes the component available for `.Reactive()` event wiring, gather, and `When()` conditions.

## How do I interact with a component after it renders?

Through `p.Component<T>(m => m.Property)` inside any pipeline. This gives you a `ComponentRef` with typed extension methods:

```csharp
// Write a value
p.Component<NativeTextBox>(m => m.ResidentName).SetValue("Jane Doe");

// Read a value (as a typed source for conditions or SetText)
var comp = p.Component<NativeTextBox>(m => m.ResidentName);
p.Element("echo").SetText(comp.Value());
```

## Native Components

Standard HTML elements. Vendor is `"native"` -- the runtime reads directly from the DOM element (`el.value`, `el.checked`).

| Component | HTML Element | ReadExpr | Value Type | Key Methods |
|-----------|-------------|----------|------------|-------------|
| `NativeTextBox` | `<input type="text">` | `"value"` | `string` | `SetValue`, `FocusIn`, `Value()` |
| `NativeCheckBox` | `<input type="checkbox">` | `"checked"` | `bool` | `SetChecked`, `FocusIn`, `Value()` |
| `NativeDropDown` | `<select>` | `"value"` | `string` | `SetValue`, `FocusIn`, `Value()` |
| `NativeTextArea` | `<textarea>` | `"value"` | `string` | `SetValue`, `FocusIn`, `Value()` |
| `NativeCheckList` | checkbox group | `"value"` | `string[]` | `SetValue`, `FocusIn`, `Value()` |
| `NativeRadioGroup` | radio group | `"value"` | `string` | `SetValue`, `FocusIn`, `Value()` |
| `NativeButton` | `<button>` | -- | -- | `SetText`, `FocusIn` |
| `NativeHiddenField` | `<input type="hidden">` | `"value"` | `string` | `SetValue`, `Value()` |

All input components fire a `Changed` event. NativeButton fires `Click` instead.

## Syncfusion Components

Syncfusion EJ2 components. Vendor is `"fusion"` -- the runtime reads via `el.ej2_instances[0]`.

| Component | SF Control | ReadExpr | Value Type | Key Methods |
|-----------|-----------|----------|------------|-------------|
| `FusionAutoComplete` | AutoComplete | `"value"` | `string` | `SetValue`, `SetText`, `SetDataSource`, `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup`, `Enable`, `Disable` |
| `FusionNumericTextBox` | NumericTextBox | `"value"` | `decimal` | `SetValue`, `SetMin`, `FocusIn`, `FocusOut`, `Increment`, `Decrement` |
| `FusionDatePicker` | DatePicker | `"value"` | `DateTime` | `SetValue`, `FocusIn`, `FocusOut` |
| `FusionDateTimePicker` | DateTimePicker | `"value"` | `DateTime` | `SetValue`, `FocusIn`, `FocusOut` |
| `FusionTimePicker` | TimePicker | `"value"` | `DateTime` | `SetValue`, `FocusIn`, `FocusOut` |
| `FusionDateRangePicker` | DateRangePicker | `"startDate"` | `DateTime` | `StartDate()`, `EndDate()`, `Value()` |
| `FusionDropDownList` | DropDownList | `"value"` | `string` | `SetValue`, `SetText`, `SetDataSource`, `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup` |
| `FusionMultiSelect` | MultiSelect | `"value"` | `string[]` | `SetValue`, `SetDataSource`, `DataBind`, `ShowPopup`, `HidePopup` |
| `FusionMultiColumnComboBox` | MultiColumnComboBox | `"value"` | `string` | `SetValue`, `SetText`, `SetDataSource`, `DataBind`, `FocusIn`, `FocusOut`, `ShowPopup`, `HidePopup` |
| `FusionSwitch` | Switch | `"checked"` | `bool` | `SetChecked` |
| `FusionInputMask` | MaskedTextBox | `"value"` | `string` | `SetValue`, `FocusIn` |
| `FusionRichTextEditor` | RichTextEditor | `"value"` | `string` | `SetValue`, `FocusIn` |
| `FusionFileUpload` | Uploader | `"filesData"` | -- | read-only, no `SetValue` |

All Fusion input components fire `Changed`. Some also fire `Filtering` (AutoComplete, MultiSelect), `Focus`/`Blur` (NumericTextBox, DropDownList), or `Selected` (FileUpload).

## App-Level Components

Singletons that exist once per page. Referenced without a model expression -- no `Html.InputField`, no binding path.

| Component | Vendor | Default ID | Key Methods |
|-----------|--------|-----------|-------------|
| `NativeDrawer` | native | `"alis-drawer"` | `Open`, `Close`, `SetSize` |
| `NativeLoader` | native | `"alis-loader"` | `Show`, `Hide`, `SetTarget`, `SetTimeout` |
| `FusionToast` | fusion | `"alisFusionToast"` | `SetTitle`, `SetContent`, `SetTimeout`, `Success`, `Warning`, `Danger`, `Info`, `Show`, `Hide`, `ShowCloseButton`, `ShowProgressBar` |
| `FusionConfirm` | fusion | `"alisConfirmDialog"` | `SetContent`, `Show`, `Hide` |

### How do I use them?

```csharp
// Drawer -- set title, size, load content, open
p.Element("alis-drawer-title").SetText("Resident Details");
p.Component<NativeDrawer>().SetSize(DrawerSize.Md);
p.Component<NativeDrawer>().Open();

// Toast -- configure and show
p.Component<FusionToast>().SetContent("Saved").Success().Show();

// Loader -- show/hide around an HTTP request
p.Component<NativeLoader>().Show();
```

They must be rendered once in your layout:

```csharp
@Html.NativeDrawer()
@Html.NativeLoader()
@Html.FusionToast()
@Html.FusionConfirmDialog()
```

## Where is the architecture behind all this?

Every component -- native or Syncfusion -- follows the same pattern: a sealed C# class declares `Vendor` and `ReadExpr`, the plan carries that metadata as JSON, and the runtime executes via vendor-neutral bracket notation. Adding a new component requires zero runtime changes.

For the full architecture -- `walk.ts`, `component.ts`, `resolver.ts`, the vertical slice shape, and `ComponentsMap` -- see [Component Model](/architecture/component-model/).
