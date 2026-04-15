---
title: FusionDialog
description: Modal popup for confirmations, forms, and detail views.
sidebar:
  order: 19
---

A modal dialog for confirmations, forms, and detail views -- confirming a resident discharge, capturing a care incident, showing medication details. Non-input component: no `InputField` wrapper.

**Render as:** `@(Html.FusionDialog(plan, "id", b => ...))` &nbsp; **Events:** `BeforeOpen`, `Opened`, `BeforeClose`, `Closed`, `OverlayClick`

## How do I render a dialog?

Declare the dialog at the top level of the view (not nested inside display containers -- Syncfusion needs to portal the modal overlay to `document.body`). Start hidden with `b.Visible(false)`; it becomes visible when something calls `Show()`.

```csharp
@(Html.FusionDialog(plan, "new-assignment-dialog", b =>
{
    b.Header("New Assignment");
    b.Width("480px");
    b.IsModal(true);
    b.ShowCloseIcon(true);
    b.CloseOnEscape(true);
    b.Visible(false);
    b.Content("<div id='new-assignment-content'></div>");
})
.Reactive(evt => evt.OverlayClick, (args, p) =>
{
    p.Component<FusionDialog>("new-assignment-dialog").Hide();
}))
```

## How do I show it from another event?

Resolve the dialog through the pipeline with `p.Component<FusionDialog>("new-assignment-dialog")` and call `.Show()`. The same ID you passed to `Html.FusionDialog(...)` is the handle from any reactive callback -- a button click, a schedule cell click, an HTTP response handler.

```csharp
// From a schedule cell click:
p.Get("/Sandbox/Components/Schedule/NewAssignmentForm")
 .Gather(g => g.FromEvent(args, x => x.StartTime, "startTime"))
 .Response(r => r.OnSuccess(s => s.Into("new-assignment-content")));
p.Component<FusionDialog>("new-assignment-dialog").Show();
```

Loading remote content into the dialog: `Response(r => r.OnSuccess(s => s.Into("new-assignment-content")))` writes the fetched HTML into the `<div id="new-assignment-content">` you declared inside `b.Content(...)`. The form renders inside the modal on open.

## Reference

| Extension | Description |
|---|---|
| `Show()` | Shows the dialog |
| `Hide()` | Hides the dialog |
| `RefreshPosition()` | Refreshes the dialog position and dimensions |
