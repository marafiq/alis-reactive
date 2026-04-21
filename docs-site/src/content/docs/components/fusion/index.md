---
title: Syncfusion Components
description: Every Syncfusion EJ2 control wrapped as a Reactive component -- autocomplete, pickers, dropdowns, surfaces, dialogs, and more.
sidebar:
  order: 0
---

Fusion components wrap [Syncfusion EJ2](https://ej2.syncfusion.com/) controls. The C# vertical slice declares property and method names; the plan carries them as data; the runtime executes on `el.ej2_instances[0]`. You never write Syncfusion-specific JavaScript.

**Package:** `Alis.Reactive.Fusion`

Each component has its own page. Pick the one you need.

## Form inputs

Bind a model property to a Syncfusion input. Rendered through `Html.InputField(plan, m => m.X).FusionX(b => ...)`.

- [AutoComplete](./auto-complete/) -- text input with server-side filtering
- [DropDownList](./dropdown-list/) -- single-select with search and cascade
- [MultiSelect](./multi-select/) -- multi-select with `string[]` value
- [MultiColumnComboBox](./multi-column-combobox/) -- combo with multi-column dropdown
- [NumericTextBox](./numeric-textbox/) -- numeric with spin buttons
- [DatePicker](./date-picker/) -- date only
- [DateTimePicker](./date-time-picker/) -- date and time
- [TimePicker](./time-picker/) -- time only
- [DateRangePicker](./date-range-picker/) -- start and end dates
- [ColorPicker](./color-picker/) -- hex color input
- [Switch](./switch/) -- toggle
- [InputMask](./input-mask/) -- format-enforced text input
- [RichTextEditor](./rich-text-editor/) -- WYSIWYG editor
- [FileUpload](./file-upload/) -- multi-file picker
- [InPlaceEditor](./in-place-editor/) -- click-to-edit a single field that wraps another inner input; commits via reactive pipeline

## Surfaces and containers

Components that organize content on the page. Rendered directly; no `InputField` wrapper.

- [Accordion](./accordion/) -- collapsible panels
- [Tab](./tab/) -- tab strip with per-panel content
- [Grid](./grid/) -- data grid with server-side sort, page, filter
- [Schedule](./schedule/) -- calendar, resource planning, CRUD
- [Dialog](./dialog/) -- modal popup for confirmations and forms
- [Tooltip](./tooltip/) -- hover hint on a target element

## App-level singletons

Rendered once in the layout. Called from any plan without a model expression.

- [Toast](./toast/) -- transient notifications (success, warn, danger, info)
- [Confirm](./confirm/) -- confirmation dialog
