---
title: FusionTooltip
description: Hover tooltip anchored to a target element.
sidebar:
  order: 20
---

A hover tooltip for surfacing contextual hints -- care-level differences on a resident card, severity legends on an incident grid, medication schedule details on a timeline. Non-input component: no `InputField` wrapper.

**Render as:** `@(Html.FusionTooltip(plan, "id", b => ...))` &nbsp; **Events:** `BeforeRender`, `BeforeOpen`, `Opened`, `BeforeClose`, `Closed`

## How do I attach a tooltip to an element?

Point the tooltip at the DOM element it describes with `b.Target("#care-level-target")` and pick a trigger with `b.OpensOn("Hover")`. The `Position` argument uses Syncfusion's strongly typed `Position` enum, so write it fully qualified. Chain `.Reactive(...)` callbacks for the lifecycle events you care about -- `Opened` and `Closed` are the common pair.

```csharp
<p id="care-level-target" class="inline-block rounded bg-surface-muted px-3 py-2">
    Memory Care
</p>

@(Html.FusionTooltip(plan, "care-level-tooltip", b =>
{
    b.Target("#care-level-target");
    b.Content("24/7 staff, secure environment, assistance with activities of daily living.");
    b.Position(Syncfusion.EJ2.Popups.Position.TopCenter);
    b.OpensOn("Hover");
    b.Width("260px");
    b.ShowTipPointer(true);
})
.Reactive(evt => evt.Opened, (_, p) =>
{
    p.Element("tooltip-opened").SetText("opened");
}))
```

## How do I open or close it programmatically?

Resolve the tooltip through the pipeline with `p.Component<FusionTooltip>("care-level-tooltip")` and call `.Open()` or `.Close()`. The ID you passed to `Html.FusionTooltip(...)` is the handle.

```csharp
@(Html.NativeButton("open-tooltip-btn", "Open Tooltip")
    .Reactive(plan, evt => evt.Click, (_, p) =>
    {
        p.Component<FusionTooltip>("care-level-tooltip").Open();
    }))
```

## Reference

| Extension | Description |
|---|---|
| `Open()` | Opens the tooltip programmatically on the target element |
| `Close()` | Closes the tooltip |
| `Refresh()` | Refreshes the tooltip position and content |
