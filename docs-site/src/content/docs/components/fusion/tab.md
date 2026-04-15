---
title: FusionTab
description: Tab strip for separating related content into browsable panels.
sidebar:
  order: 16
---

A tab strip for separating related content into browsable panels -- residents, staff, facilities, and reports inside a single facility management page. Non-input component: no `InputField` wrapper.

**Render as:** `@(Html.FusionTab(plan, "id", b => ...))` &nbsp; **Events:** `Selected`

## How do I render a tab strip?

Declare `TabItem` instances as a Razor-level local, then pass them to `.Items(...)`. Each item has a `Header` (with `Text`) and a `Content` string. `TabItem` and `TabHeader` come from `Syncfusion.EJ2.Navigations`, so add `@using Syncfusion.EJ2.Navigations` to the view.

```cshtml
@{
    var tabItems = new List<TabItem>
    {
        new TabItem { Header = new TabHeader { Text = "Residents" },
            Content = "<div class='p-4'><p class='text-sm'>Resident management content. Lists all residents in the facility with care level assignments.</p></div>" },
        new TabItem { Header = new TabHeader { Text = "Staff" },
            Content = "<div class='p-4'><p class='text-sm'>Staff scheduling and assignments. View shift rotations and caregiver-to-resident ratios.</p></div>" },
        new TabItem { Header = new TabHeader { Text = "Facilities" },
            Content = "<div class='p-4'><p class='text-sm'>Facility details and room availability.</p></div>" },
        new TabItem { Header = new TabHeader { Text = "Reports" },
            Content = "<div class='p-4'><p class='text-sm'>Monthly compliance and care reports.</p></div>" }
    };
}

@(Html.FusionTab(plan, "demo-tab", b => b
    .Items(tabItems)))
```

## How do I react when the user switches tabs?

`Selected.SelectedIndex` is zero-based. Chain `.When(...).Eq(0)` then `.ElseIf(...).Eq(1)` through each panel, close with `.Else(...)`. Each branch can do more than `SetText` -- drop a `Get(...)` with `Response(r => r.OnSuccess(s => s.Into("lazy-tab-content")))` inside a `Then` to lazy-load the panel's content from a partial view only when the user opens it.

```csharp
@(Html.FusionTab(plan, "demo-tab", b => b
    .Items(tabItems))
    .Reactive(evt => evt.Selected, (args, p) =>
    {
        p.Element("selected-index").SetText(args, x => x.SelectedIndex);
        p.When(args, x => x.SelectedIndex).Eq(0)
            .Then(t => t.Element("condition-result").SetText("Residents tab active"))
            .ElseIf(args, x => x.SelectedIndex).Eq(1)
            .Then(t => t.Element("condition-result").SetText("Staff tab active"))
            .ElseIf(args, x => x.SelectedIndex).Eq(2)
            .Then(t => t.Element("condition-result").SetText("Facilities tab active"))
            .Else(e => e.Element("condition-result").SetText("Reports tab active"));
    }))
```

## Reference

| Extension | Description |
|---|---|
| `Select(int index)` | Selects a tab by index |
| `HideTab(int index, bool isHidden = true)` | Shows or hides a tab by index |
| `SetSelectedItem(int index)` | Sets the selected tab index via the `selectedItem` property |
