---
title: FusionInPlaceEditor
description: Click-to-edit single-field UX that wraps another Syncfusion input. Commit via the reactive HTTP pipeline.
sidebar:
  order: 15
---

A click-to-edit editor. Displays a value as plain text; user clicks the pencil; an inner editor (Text, DropDownList, NumericTextBox, DatePicker, ...) opens with OK/Cancel; on OK, the reactive pipeline commits to your server. Use it for quick-edit UX on detail pages — "change this resident's DOB without leaving the profile view".

**Model type:** `string` (the outer `value` surfaces every inner type as string) &nbsp; **ReadExpr:** `"value"` &nbsp; **Commit event:** `ActionSuccess`

## How do I render a quick-edit card?

Configure the inner editor on the builder (`Type`, `Mode`, `Model`). Hook `ActionSuccess` — it fires only after the user's edit passes Syncfusion's internal validation. `SubmitClick` fires on every save click, even when validation blocks the commit, so it is **not** a commit-success signal.

```csharp
Html.InputField(plan, m => m.Value, o => o.Label("Date of Birth"))
    .FusionInPlaceEditor(b => b
        .Type(InputType.Date)
        .Mode(RenderMode.Inline)
        .EditableOn(EditableType.Click)
        .Reactive(plan, evt => evt.ActionSuccess, (args, p) =>
        {
            p.Post("/residents/dob")
             .Gather(g => g
                 .Include<FusionInPlaceEditor, DobEdit>(m => m.Value)
                 .Include<NativeHiddenField, DobEdit>(m => m.ResidentId))
             .Validate<DobEditValidator>("card-dob")
             .Response(r => r
                .OnSuccess<Commit>((json, s) =>
                    s.Element("dob-display").SetText(json, x => x.Display))
                .OnError<CommitError>((err, e) =>
                    e.Element("dob-error").SetText(err, x => x.Message)));
        }));
```

## How do I send the entity identity with the commit?

The standalone quick-edit model carries both the edited value and any identity fields. Render the identity as a `NativeHiddenField` and include it in the gather — no duplication, no new primitives.

```csharp
@model DobEdit  // { ResidentId, Value }

@Html.HiddenFieldFor(plan, m => m.ResidentId)

// ... then in the Reactive handler:
.Gather(g => g
    .Include<FusionInPlaceEditor, DobEdit>(m => m.Value)
    .Include<NativeHiddenField, DobEdit>(m => m.ResidentId))
```

## How does validation work?

Validation is the framework's single path: define rules **once** in a `FluentValidator<T>`, call `.Validate<TValidator>(formId)` on the `HttpRequestBuilder`, and the framework:

- runs the extracted client-side rules before the POST (aborting if invalid),
- renders errors in the per-field validation slot (`{elementId}_error`),
- routes server-side 400 responses through the same slot via `e.ValidationErrors(formId)`.

```csharp
b.Reactive(plan, evt => evt.ActionSuccess, (args, p) =>
{
    p.Post(url)
     .Gather(g => g
        .Include<FusionInPlaceEditor, M>(m => m.Value)
        .Include<NativeHiddenField, M>(m => m.ResidentId))
     .Validate<MonthlyRateQuickEditValidator>("card-monthly-rate")
     .Response(r => r
        .OnSuccess<Ok>((json, s) => s.Element("display").SetText(json, x => x.Display))
        .OnError(400, e => e.ValidationErrors("card-monthly-rate"))
        .OnError<CommitError>((err, e) => e.Element("error").SetText(err, x => x.Message)));
});
```

Do **not** configure Syncfusion's `ValidationRules` dictionary in parallel. That duplicates the validator declaration (one in the view, one in the `FluentValidator<T>`) and creates two independent enforcement paths that can drift out of sync. The framework's single-source-of-truth rule applies: one validator, one plan, one enforcement path.

## How do I react to user cancel?

```csharp
b.Reactive(plan, evt => evt.CancelClick, (args, p) =>
{
    p.Element("card-status").SetText("User cancelled the edit");
});
```

## Reference

### Mutations

| Extension | Description |
|---|---|
| `SetValue(string?)` | Sets the committed value |
| `Enable()` / `Disable()` | Toggles edit-mode entry (calls SF `disable(bool)`) |
| `Save()` | Programmatic commit (fires `ActionSuccess`, not `SubmitClick`) |
| `Focus()` | Focuses the inner editor input |

### Reads

| Extension | Returns | Purpose |
|---|---|---|
| `Value()` | `TypedComponentSource<string>` | Read the committed value for conditions or gather |

### Events

| Event | When it fires | Args highlights |
|---|---|---|
| `BeginEdit` | User opens the editor | `cancel`, `cancelFocus`, `mode` |
| `Change` | Inner value changes while editing | `value`, `previousValue` |
| `EndEdit` | Editor leaves edit mode | `cancel`, `action` (`"submit"` or `"cancel"`) |
| `ActionBegin` | Before SF's submit lifecycle | `cancel`, `data` (`{ name, primaryKey, value }`) |
| `ActionSuccess` | After successful commit &mdash; **use as your reactive commit hook** | `value`, `data` |
| `SubmitClick` | User clicked Save or pressed Enter | `name` only; fires even when validation blocked the commit |
| `CancelClick` | User clicked Cancel | `name` only |

### Args extensions

| Extension | On which args | Purpose |
|---|---|---|
| `PreventDefault(pipeline)` | `BeginEditArgs`, `EndEditArgs`, `ActionBeginArgs` | Sets `args.cancel = true` so SF honors the block |
