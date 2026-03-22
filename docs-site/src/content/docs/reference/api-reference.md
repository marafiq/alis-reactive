---
title: API Reference
description: Complete public API — every method, every overload, every signature.
sidebar:
  order: 0
---

Complete public API for Alis.Reactive. Every method listed here exists in the source code. For usage and examples, see the [Grammar Tree](/alis-reactive/csharp-modules/mental-model/#the-grammar-tree) and the [Reactivity](/alis-reactive/csharp-modules/reactivity/triggers-and-reactions/) pages.

## Html Extensions

### Plan Lifecycle

```csharp
IReactivePlan<TModel> Html.ReactivePlan<TModel>()
IReactivePlan<TModel> Html.ResolvePlan<TModel>()
IHtmlContent Html.RenderPlan<TModel>(IReactivePlan<TModel> plan)
```

### Triggers

```csharp
void Html.On<TModel>(IReactivePlan<TModel> plan, Action<TriggerBuilder<TModel>> triggerBuilder)
```

### TriggerBuilder

```csharp
TriggerBuilder<TModel> t.DomReady(Action<PipelineBuilder<TModel>> configure)
TriggerBuilder<TModel> t.CustomEvent(string eventName, Action<PipelineBuilder<TModel>> configure)
TriggerBuilder<TModel> t.CustomEvent<TPayload>(string eventName, Action<TPayload, PipelineBuilder<TModel>> configure)
```

### InputField

```csharp
InputFieldSetup<TModel, TProp> Html.InputField<TModel, TProp>(
    IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression,
    Action<InputFieldOptions>? options = null)
```

#### InputFieldOptions

```csharp
InputFieldOptions Required()
InputFieldOptions Label(string label)
```

### Standalone Components

```csharp
NativeButtonBuilder<TModel> Html.NativeButton<TModel>(string elementId, string text)
NativeHiddenFieldBuilder<TModel, TProp> Html.HiddenFieldFor<TModel, TProp>(
    IReactivePlan<TModel> plan, Expression<Func<TModel, TProp>> expression)
NativeActionLinkBuilder<TModel> Html.NativeActionLink<TModel>(
    string linkText, string url, Action<PipelineBuilder<TModel>> configure)
```

### Layout Helpers

```csharp
IHtmlContent Html.NativeDrawer()
IHtmlContent Html.NativeLoader()
IHtmlContent Html.FusionConfirmDialog()
IHtmlContent Html.FusionToast()
```

---

## Native Components

### NativeTextBox

```csharp
// Factory
void InputFieldSetup.NativeTextBox(Action<NativeTextBoxBuilder<TModel, TProp>> configure)

// Builder
NativeTextBoxBuilder Type(string type)
NativeTextBoxBuilder Placeholder(string placeholder)
NativeTextBoxBuilder CssClass(string css)
NativeTextBoxBuilder Reactive<TArgs>(IReactivePlan<TModel> plan,
    Func<NativeTextBoxEvents, TypedEventDescriptor<TArgs>> eventSelector,
    Action<TArgs, PipelineBuilder<TModel>> pipeline)

// Events
evt.Changed → NativeTextBoxChangeArgs { string? Value }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### NativeCheckBox

```csharp
// Factory
void InputFieldSetup.NativeCheckBox(Action<NativeCheckBoxBuilder<TModel, bool>> configure)

// Builder
NativeCheckBoxBuilder CssClass(string css)
NativeCheckBoxBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeCheckBoxChangeArgs { bool? Checked }

// Pipeline Extensions
ComponentRef SetChecked(bool isChecked)
ComponentRef FocusIn()
TypedComponentSource<bool> Value()
```

### NativeDropDown

```csharp
// Factory
void InputFieldSetup.NativeDropDown(Action<NativeDropDownBuilder<TModel, TProp>> configure)

// Builder
NativeDropDownBuilder Items(IEnumerable<SelectListItem> items)
NativeDropDownBuilder Placeholder(string optionLabel)
NativeDropDownBuilder Enabled(bool enabled)
NativeDropDownBuilder CssClass(string css)
NativeDropDownBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeDropDownChangeArgs { string? Value }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### NativeTextArea

```csharp
// Factory
void InputFieldSetup.NativeTextArea(Action<NativeTextAreaBuilder<TModel, TProp>> configure)

// Builder
NativeTextAreaBuilder Rows(int rows)
NativeTextAreaBuilder Placeholder(string placeholder)
NativeTextAreaBuilder CssClass(string css)
NativeTextAreaBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeTextAreaChangeArgs { string? Value }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### NativeCheckList

```csharp
// Factory
void InputFieldSetup.NativeCheckList(Action<NativeCheckListBuilder<TModel, TProp>> configure)

// Builder
NativeCheckListBuilder Items(IEnumerable<RadioButtonItem> items)
NativeCheckListBuilder Option(string value)
NativeCheckListBuilder Option(string value, string text)
NativeCheckListBuilder Option(string value, string text, string description)
NativeCheckListBuilder CssClass(string css)
NativeCheckListBuilder OptionCssClass(string css)
NativeCheckListBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeCheckListChangeArgs { string[]? Value }

// Pipeline Extensions
ComponentRef SetValue(string[] value)
ComponentRef SetValue<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef FocusIn()
TypedComponentSource<string[]> Value()
```

### NativeRadioGroup

```csharp
// Factory
void InputFieldSetup.NativeRadioGroup(Action<NativeRadioGroupBuilder<TModel, TProp>> configure)

// Builder
NativeRadioGroupBuilder Items(IEnumerable<RadioButtonItem> items)
NativeRadioGroupBuilder Option(string value)
NativeRadioGroupBuilder Option(string value, string text)
NativeRadioGroupBuilder Option(string value, string text, string description)
NativeRadioGroupBuilder CssClass(string css)
NativeRadioGroupBuilder OptionCssClass(string css)
NativeRadioGroupBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeRadioGroupChangeArgs { string? Value }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef SetValue<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### NativeButton

```csharp
// Factory
NativeButtonBuilder Html.NativeButton(string elementId, string text)

// Builder
NativeButtonBuilder Type(string type)
NativeButtonBuilder CssClass(string css)
NativeButtonBuilder Reactive<TArgs>(...)

// Events
evt.Click → NativeButtonClickArgs { }

// Pipeline Extensions
ComponentRef SetText(string text)
ComponentRef FocusIn()
```

### NativeHiddenField

```csharp
// Factory
NativeHiddenFieldBuilder Html.HiddenFieldFor(IReactivePlan<TModel> plan,
    Expression<Func<TModel, TProp>> expression)

// Builder
NativeHiddenFieldBuilder Reactive<TArgs>(...)

// Events
evt.Changed → NativeHiddenFieldChangeArgs { string? Value }

// Pipeline Extensions
ComponentRef SetValue(string value)
TypedComponentSource<string> Value()
```

### NativeDrawer (App-Level)

```csharp
// Pipeline Extensions
ComponentRef SetSize(DrawerSize size)        // DrawerSize.Sm | .Md | .Lg
ComponentRef Open()
ComponentRef Close()
```

### NativeLoader (App-Level)

```csharp
// Pipeline Extensions
ComponentRef SetTarget(string targetId)
ComponentRef SetTimeout(int ms)
ComponentRef Show()
ComponentRef Hide()
```

---

## Fusion Components

### FusionDropDownList

```csharp
// Factory
void InputFieldSetup.DropDownList(Action<DropDownListBuilder> configure)

// Builder (Syncfusion EJ2 + Reactive Extensions)
DropDownListBuilder Fields<TItem>(Expression text, Expression value)
DropDownListBuilder Fields<TItem>(Expression text, Expression value, Expression groupBy)
DropDownListBuilder Reactive<TArgs>(...)

// Events
evt.Changed → FusionDropDownListChangeArgs { string? Value, bool IsInteracted }
evt.Focus → FusionDropDownListFocusArgs { }
evt.Blur → FusionDropDownListBlurArgs { }

// Pipeline Extensions
ComponentRef SetValue(string? value)
ComponentRef SetText(string text)
ComponentRef SetDataSource<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef SetDataSource<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
ComponentRef DataBind()
ComponentRef FocusIn()
ComponentRef FocusOut()
ComponentRef ShowPopup()
ComponentRef HidePopup()
TypedComponentSource<string> Value()
```

### FusionAutoComplete

```csharp
// Factory
void InputFieldSetup.AutoComplete(Action<AutoCompleteBuilder> configure)

// Builder
AutoCompleteBuilder Fields<TItem>(Expression text, Expression value)
AutoCompleteBuilder Fields<TItem>(Expression text, Expression value, Expression groupBy)
AutoCompleteBuilder Reactive<TArgs>(...)

// Events
evt.Changed → FusionAutoCompleteChangeArgs { string? Value, bool IsInteracted }
evt.Filtering → FusionAutoCompleteFilteringArgs { string Text }

// Filtering Args Extensions
void args.PreventDefault<TModel>(PipelineBuilder<TModel> pipeline)
void args.UpdateData<TModel, TResponse>(PipelineBuilder<TModel> pipeline,
    ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)

// Pipeline Extensions
ComponentRef SetValue(string? value)
ComponentRef SetText(string text)
ComponentRef SetDataSource<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef SetDataSource<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
ComponentRef DataBind()
ComponentRef FocusIn()
ComponentRef FocusOut()
ComponentRef ShowPopup()
ComponentRef HidePopup()
ComponentRef Enable()
ComponentRef Disable()
TypedComponentSource<string> Value()
```

### FusionMultiSelect

```csharp
// Factory
void InputFieldSetup.MultiSelect(Action<MultiSelectBuilder> configure)

// Builder
MultiSelectBuilder Fields<TItem>(Expression text, Expression value)
MultiSelectBuilder Fields<TItem>(Expression text, Expression value, Expression groupBy)
MultiSelectBuilder Reactive<TArgs>(...)

// Events
evt.Changed → FusionMultiSelectChangeArgs { string[]? Value, bool IsInteracted }
evt.Filtering → FusionMultiSelectFilteringArgs { string Text }

// Filtering Args Extensions
void args.PreventDefault<TModel>(PipelineBuilder<TModel> pipeline)
void args.UpdateData<TModel, TResponse>(PipelineBuilder<TModel> pipeline,
    ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)

// Pipeline Extensions
ComponentRef SetValue(string[]? value)
ComponentRef SetDataSource<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef SetDataSource<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
ComponentRef DataBind()
ComponentRef ShowPopup()
ComponentRef HidePopup()
TypedComponentSource<string[]> Value()
```

### FusionMultiColumnComboBox

```csharp
// Factory
void InputFieldSetup.MultiColumnComboBox(Action<MultiColumnComboBoxBuilder> configure)

// Builder
MultiColumnComboBoxBuilder Fields<TItem>(Expression text, Expression value)
MultiColumnComboBoxBuilder Fields<TItem>(Expression text, Expression value, Expression groupBy)
MultiColumnComboBoxBuilder Reactive<TArgs>(...)

// Events
evt.Changed → FusionMultiColumnComboBoxChangeArgs { string? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(string? value)
ComponentRef SetText(string text)
ComponentRef SetDataSource<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ComponentRef SetDataSource<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
ComponentRef DataBind()
ComponentRef FocusIn()
ComponentRef FocusOut()
ComponentRef ShowPopup()
ComponentRef HidePopup()
TypedComponentSource<string> Value()
```

### FusionNumericTextBox

```csharp
// Factory
void InputFieldSetup.NumericTextBox(Action<NumericTextBoxBuilder> configure)

// Builder (Syncfusion EJ2 + Reactive Extensions)
NumericTextBoxBuilder Reactive<TArgs>(...)

// Events
evt.Changed → FusionNumericTextBoxChangeArgs { decimal Value, decimal PreviousValue, bool IsInteracted }
evt.Focus → FusionNumericTextBoxFocusArgs { }
evt.Blur → FusionNumericTextBoxBlurArgs { }

// Pipeline Extensions
ComponentRef SetValue(decimal value)
ComponentRef SetMin(decimal min)
ComponentRef FocusIn()
ComponentRef FocusOut()
ComponentRef Increment()
ComponentRef Decrement()
TypedComponentSource<decimal> Value()
```

### FusionDatePicker

```csharp
// Factory
void InputFieldSetup.DatePicker(Action<DatePickerBuilder> configure)

// Events
evt.Changed → FusionDatePickerChangeArgs { DateTime? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(DateTime value)
ComponentRef FocusIn()
ComponentRef FocusOut()
TypedComponentSource<DateTime> Value()
```

### FusionDateTimePicker

```csharp
// Factory
void InputFieldSetup.DateTimePicker(Action<DateTimePickerBuilder> configure)

// Events
evt.Changed → FusionDateTimePickerChangeArgs { DateTime? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(DateTime value)
ComponentRef FocusIn()
ComponentRef FocusOut()
TypedComponentSource<DateTime> Value()
```

### FusionDateRangePicker

```csharp
// Factory
void InputFieldSetup.DateRangePicker(Action<DateRangePickerBuilder> configure)

// Events
evt.Changed → FusionDateRangePickerChangeArgs { DateTime? StartDate, DateTime? EndDate, bool IsInteracted }

// Pipeline Extensions (reads only — no SetValue)
TypedComponentSource<DateTime> StartDate()
TypedComponentSource<DateTime> EndDate()
TypedComponentSource<DateTime> Value()        // alias for StartDate()
```

### FusionTimePicker

```csharp
// Factory
void InputFieldSetup.TimePicker(Action<TimePickerBuilder> configure)

// Events
evt.Changed → FusionTimePickerChangeArgs { DateTime? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(DateTime value)
ComponentRef FocusIn()
ComponentRef FocusOut()
TypedComponentSource<DateTime> Value()
```

### FusionSwitch

```csharp
// Factory
void InputFieldSetup.Switch(Action<SwitchBuilder> configure)

// Events
evt.Changed → FusionSwitchChangeArgs { bool Checked, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetChecked(bool isChecked)
TypedComponentSource<bool> Value()
```

### FusionInputMask

```csharp
// Factory
void InputFieldSetup.InputMask(Action<MaskedTextBoxBuilder> configure)

// Events
evt.Changed → FusionInputMaskChangeArgs { string? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### FusionRichTextEditor

```csharp
// Factory
void InputFieldSetup.RichTextEditor(Action<RichTextEditorBuilder> configure)

// Events
evt.Changed → FusionRichTextEditorChangeArgs { string? Value, bool IsInteracted }

// Pipeline Extensions
ComponentRef SetValue(string value)
ComponentRef FocusIn()
TypedComponentSource<string> Value()
```

### FusionFileUpload

```csharp
// Factory
void InputFieldSetup.FileUpload(Action<UploaderBuilder> configure)

// Events
evt.Selected → FusionFileUploadSelectedArgs { int FilesCount, bool IsInteracted }

// Pipeline Extensions (read only — no SetValue)
TypedComponentSource<string> Value()
```

### FusionToast (App-Level)

```csharp
// Pipeline Extensions
ComponentRef SetTitle(string title)
ComponentRef SetContent(string content)
ComponentRef SetTimeout(int ms)
ComponentRef ShowCloseButton()
ComponentRef ShowProgressBar()
ComponentRef Success()
ComponentRef Warning()
ComponentRef Danger()
ComponentRef Info()
ComponentRef Show()
ComponentRef Hide()
```

### FusionConfirm (App-Level)

```csharp
// Pipeline Extensions
ComponentRef SetContent(string message)
ComponentRef Show()
ComponentRef Hide()
```

---

## Pipeline

### PipelineBuilder

```csharp
ElementBuilder<TModel> Element(string elementId)
ComponentRef<TComponent, TModel> Component<TComponent>(Expression<Func<TModel, object?>> expr)
ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
ComponentRef<TComponent, TModel> Component<TComponent>()    // IAppLevelComponent only
PipelineBuilder<TModel> Dispatch(string eventName)
PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
PipelineBuilder<TModel> ValidationErrors(string formId)
PipelineBuilder<TModel> Into(string elementId)
```

### ElementBuilder

```csharp
PipelineBuilder AddClass(string className)
PipelineBuilder RemoveClass(string className)
PipelineBuilder ToggleClass(string className)
PipelineBuilder SetText(string text)
PipelineBuilder SetText<TSource>(TSource source, Expression<Func<TSource, object?>> path)
PipelineBuilder SetText<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
ElementBuilder SetText(BindSource source)
ElementBuilder SetText<TProp>(TypedSource<TProp> source)
PipelineBuilder SetHtml(string html)
PipelineBuilder SetHtml<TSource>(TSource source, Expression<Func<TSource, object?>> path)
ElementBuilder SetHtml(BindSource source)
ElementBuilder SetHtml<TProp>(TypedSource<TProp> source)
PipelineBuilder Show()
PipelineBuilder Hide()
ElementBuilder When<TPayload, TProp>(TPayload payload,
    Expression<Func<TPayload, TProp>> path,
    Func<ConditionSourceBuilder<TModel, TProp>, GuardBuilder<TModel>> configure)
```

### HTTP — PipelineBuilder

```csharp
HttpRequestBuilder<TModel> Get(string url)
HttpRequestBuilder<TModel> Post(string url)
HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
HttpRequestBuilder<TModel> Delete(string url)
ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
```

### HttpRequestBuilder

```csharp
HttpRequestBuilder Gather(Action<GatherBuilder<TModel>> configure)
HttpRequestBuilder AsJson()
HttpRequestBuilder AsFormData()
HttpRequestBuilder Validate<TValidator>(string formId)
HttpRequestBuilder Validate(ValidationDescriptor validation)
HttpRequestBuilder WhileLoading(Action<PipelineBuilder<TModel>> configure)
HttpRequestBuilder Response(Action<ResponseBuilder<TModel>> configure)
// Convenience verbs (used in Chained/Parallel):
HttpRequestBuilder Get(string url)
HttpRequestBuilder Post(string url)
HttpRequestBuilder Put(string url)
HttpRequestBuilder Delete(string url)
```

### GatherBuilder

```csharp
GatherBuilder IncludeAll()
GatherBuilder Static(string param, object value)
GatherBuilder FromEvent<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path, string param)
GatherBuilder AddItem(GatherItem item)
// Vendor extensions:
GatherBuilder Include<TComponent, TModel>(Expression<Func<TModel, object?>> expr)
GatherBuilder Include<TComponent, TModel>(string refId, string name)
```

### ResponseBuilder

```csharp
ResponseBuilder OnSuccess(Action<PipelineBuilder<TModel>> configure)
ResponseBuilder OnSuccess<TResponse>(Action<ResponseBody<TResponse>, PipelineBuilder<TModel>> configure)
ResponseBuilder OnError(int statusCode, Action<PipelineBuilder<TModel>> configure)
ResponseBuilder Chained(Action<HttpRequestBuilder<TModel>> configure)
```

### ParallelBuilder

```csharp
ParallelBuilder OnAllSettled(Action<PipelineBuilder<TModel>> configure)
```

### Conditions — PipelineBuilder

```csharp
ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
    TPayload payload, Expression<Func<TPayload, TProp>> path)
ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
GuardBuilder<TModel> Confirm(string message)
```

### ConditionSourceBuilder — Operators

```csharp
// Comparison (typed operand)
GuardBuilder Eq(TProp operand)
GuardBuilder NotEq(TProp operand)
GuardBuilder Gt(TProp operand)
GuardBuilder Gte(TProp operand)
GuardBuilder Lt(TProp operand)
GuardBuilder Lte(TProp operand)

// Presence (no operand)
GuardBuilder Truthy()
GuardBuilder Falsy()
GuardBuilder IsNull()
GuardBuilder NotNull()
GuardBuilder IsEmpty()
GuardBuilder NotEmpty()

// Membership
GuardBuilder In(params TProp[] values)
GuardBuilder NotIn(params TProp[] values)

// Range
GuardBuilder Between(TProp low, TProp high)

// Text
GuardBuilder Contains(string substring)
GuardBuilder StartsWith(string prefix)
GuardBuilder EndsWith(string suffix)
GuardBuilder Matches(string pattern)
GuardBuilder MinLength(int length)

// Array
GuardBuilder ArrayContains(object item)

// Source-vs-source (right side is TypedSource, not literal)
GuardBuilder Eq(TypedSource<TProp> right)
GuardBuilder NotEq(TypedSource<TProp> right)
GuardBuilder Gt(TypedSource<TProp> right)
GuardBuilder Gte(TypedSource<TProp> right)
GuardBuilder Lt(TypedSource<TProp> right)
GuardBuilder Lte(TypedSource<TProp> right)
```

### GuardBuilder — Composition

```csharp
// Direct And/Or
ConditionSourceBuilder And<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path)
ConditionSourceBuilder Or<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path)
ConditionSourceBuilder And<TProp>(TypedSource<TProp> source)
ConditionSourceBuilder Or<TProp>(TypedSource<TProp> source)

// Lambda And/Or (for nesting)
GuardBuilder And(Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
GuardBuilder Or(Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)

// Inversion
GuardBuilder Not()

// Branching
BranchBuilder Then(Action<PipelineBuilder<TModel>> configure)
```

### BranchBuilder

```csharp
ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
    TPayload payload, Expression<Func<TPayload, TProp>> path)
ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source)
void Else(Action<PipelineBuilder<TModel>> configure)
```

### ConditionStart (inside And/Or lambdas)

```csharp
ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
    TPayload payload, Expression<Func<TPayload, TProp>> path)
ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
GuardBuilder<TModel> Confirm(string message)
```
