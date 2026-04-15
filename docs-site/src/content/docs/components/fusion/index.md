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

- [AutoComplete](/alis-reactive/components/fusion/auto-complete/) -- text input with server-side filtering
- [DropDownList](/alis-reactive/components/fusion/dropdown-list/) -- single-select with search and cascade
- [MultiSelect](/alis-reactive/components/fusion/multi-select/) -- multi-select with `string[]` value
- [MultiColumnComboBox](/alis-reactive/components/fusion/multi-column-combobox/) -- combo with multi-column dropdown
- [NumericTextBox](/alis-reactive/components/fusion/numeric-textbox/) -- numeric with spin buttons
- [DatePicker](/alis-reactive/components/fusion/date-picker/) -- date only
- [DateTimePicker](/alis-reactive/components/fusion/date-time-picker/) -- date and time
- [TimePicker](/alis-reactive/components/fusion/time-picker/) -- time only
- [DateRangePicker](/alis-reactive/components/fusion/date-range-picker/) -- start and end dates
- [ColorPicker](/alis-reactive/components/fusion/color-picker/) -- hex color input
- [Switch](/alis-reactive/components/fusion/switch/) -- toggle
- [InputMask](/alis-reactive/components/fusion/input-mask/) -- format-enforced text input
- [RichTextEditor](/alis-reactive/components/fusion/rich-text-editor/) -- WYSIWYG editor
- [FileUpload](/alis-reactive/components/fusion/file-upload/) -- multi-file picker

## Surfaces and containers

Components that organize content on the page. Rendered directly; no `InputField` wrapper.

- [Accordion](/alis-reactive/components/fusion/accordion/) -- collapsible panels
- [Tab](/alis-reactive/components/fusion/tab/) -- tab strip with per-panel content
- [Grid](/alis-reactive/components/fusion/grid/) -- data grid with server-side sort, page, filter
- [Schedule](/alis-reactive/components/fusion/schedule/) -- calendar, resource planning, CRUD
- [Dialog](/alis-reactive/components/fusion/dialog/) -- modal popup for confirmations and forms
- [Tooltip](/alis-reactive/components/fusion/tooltip/) -- hover hint on a target element

## App-level singletons

Rendered once in the layout. Called from any plan without a model expression.

- [Toast](/alis-reactive/components/fusion/toast/) -- transient notifications (success, warn, danger, info)
- [Confirm](/alis-reactive/components/fusion/confirm/) -- confirmation dialog
