---
title: FusionAccordion
description: Collapsible panel container for grouping related content.
sidebar:
  order: 15
---

Reach for Accordion when a page has several related sections the user will only read one at a time -- facility overviews, care-level breakdowns, resident profile tabs. Non-input component: no `InputField` wrapper, no model property.

**Render as:** `@(Html.FusionAccordion(plan, "id", b => ...))` &nbsp; **Events:** `Expanded`

## How do I render an accordion with multiple panels?

Pass `AccordionItem` instances to `.Items(...)` inside the builder. Each item has a `Header` and a `Content` string (raw HTML). The element ID you pass as the second argument is the handle you use from any reactive callback.

```csharp
@(Html.FusionAccordion(plan, "demo-accordion", b => b
    .Items(new List<AccordionItem>
    {
        new AccordionItem { Header = "Facility Overview", Content = "<p>Sunrise Senior Living provides assisted living, memory care, and independent living communities across 30 states.</p>" },
        new AccordionItem { Header = "Care Levels", Content = "<p>Level 1: Independent Living. Level 2: Assisted Living. Level 3: Memory Care. Level 4: Skilled Nursing.</p>" },
        new AccordionItem { Header = "Contact Information", Content = "<p>Main Office: (555) 123-4567. Emergency: (555) 987-6543. Admissions: admissions@sunrise.example.com</p>" }
    })))
```

The whole expression returns `IHtmlContent`, so it is wrapped in `@(...)`. `Items(...)` accepts any `List<AccordionItem>` you build -- inline literals, partial views, database lookups.

## How do I react when a panel opens or closes?

The `Expanded` event args expose `Index` (zero-based) and `IsExpanded` (true when opening, false when collapsing). Use `p.When(args, x => x.IsExpanded).Truthy()` to branch -- chain `.And(args, x => x.Index).Eq(N)` to react only to a specific panel. That's the right shape for lazy-loading each panel's content with a `Get(...)` inside the `Then` branch.

```csharp
@(Html.FusionAccordion(plan, "demo-accordion", b => b
    .Items(new List<AccordionItem>
    {
        new AccordionItem { Header = "Facility Overview", Content = "<p>...</p>" },
        new AccordionItem { Header = "Care Levels", Content = "<p>...</p>" },
        new AccordionItem { Header = "Contact Information", Content = "<p>...</p>" }
    }))
    .Reactive(evt => evt.Expanded, (args, p) =>
    {
        p.Element("expanded-index").SetText(args, x => x.Index);
        p.Element("expanded-state").SetText(args, x => x.IsExpanded);

        p.When(args, x => x.IsExpanded).Truthy()
            .Then(t => t.Element("condition-result").SetText("Panel expanded"))
            .Else(e => e.Element("condition-result").SetText("Panel collapsed"));
    }))
```

## Reference

| Extension | Description |
|---|---|
| `ExpandItem(bool isExpand, int index)` | Expands or collapses a panel by index |
| `EnableItem(int index, bool isEnable = true)` | Enables or disables a panel by index |
