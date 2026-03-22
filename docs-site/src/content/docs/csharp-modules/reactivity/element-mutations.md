---
title: Element Mutations
description: Targeting DOM elements for text, HTML, classes, and visibility changes.
sidebar:
  order: 2
---

Inside a `.Reactive()` or `Html.On()` pipeline, `pipeline` gives you access to mutations — ways to change what's on the page. You target an element or a component, then call methods that describe what should change.

From the [Grammar Tree](/alis-reactive/csharp-modules/mental-model/#the-grammar-tree) — Element mutations:

```
pipeline.Element("id")                               § target a DOM element by ID
├── .SetText("x") / .SetText(source, s => s.Prop)   static or source-bound text
├── .SetHtml("x") / .SetHtml(source, s => s.Prop)   static or source-bound HTML
├── .AddClass("x") / .RemoveClass("x") / .ToggleClass("x")
├── .Show() / .Hide()                                visibility (hidden attribute)
```

## How do I target a DOM element?

`pipeline.Element("id")` targets a plain DOM element by its `id` attribute. Here it is inside a `DomReady` trigger:

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("status-bar").AddClass("active");
    pipeline.Element("status-bar").SetText("Ready");
    pipeline.Element("welcome-section").Show();
}));
```

Or inside a `.Reactive()` on a component:

```csharp
.NativeCheckBox(b => b.Reactive(plan, evt => evt.Changed, (args, pipeline) =>
{
    pipeline.Element("details-section").Show();
}));
```

The `pipeline` is the same pipeline in both cases — same methods, same fluent chain.

## What class operations are available?

Three methods for CSS class manipulation:

```csharp
pipeline.Element("alert").AddClass("bg-red-100");     // Adds the class
pipeline.Element("alert").RemoveClass("bg-red-100");   // Removes it
pipeline.Element("sidebar").ToggleClass("collapsed");   // Adds if absent, removes if present
```

All three return the pipeline, so you continue with the next command through `Element()` again:

```csharp
pipeline.Element("card").RemoveClass("border-gray-200");
pipeline.Element("card").AddClass("border-green-500");
pipeline.Element("card").AddClass("shadow-lg");
```

## How do I set text and HTML content?

### Static values

```csharp
pipeline.Element("greeting").SetText("Welcome to Sunrise Manor");
```

Sets `textContent` -- HTML entities are escaped, so this is safe for user-provided strings.

```csharp
pipeline.Element("notice").SetHtml("<strong>Important:</strong> Review before submitting");
```

Sets `innerHTML`. Use this when you need rendered markup.

### Values from an event payload

The more powerful pattern -- bind the text to a property resolved at runtime:

```csharp
Html.On(plan, t => t.CustomEvent<ResidentPayload>("resident-selected", (payload, pipeline) =>
{
    pipeline.Element("name-display").SetText(payload, x => x.FullName);
    pipeline.Element("room-display").SetText(payload, x => x.RoomNumber);
    pipeline.Element("city-display").SetText(payload, x => x.Address.City);
}));
```

The expression `x => x.Address.City` compiles to `"evt.address.city"` in the plan JSON. At runtime, the event's `detail` object is walked at that path and the value is set as text.

`SetHtml` supports the same source-bound overload:

```csharp
pipeline.Element("bio-display").SetHtml(payload, x => x.FormattedBio);
```

### Values from an HTTP response

Inside a response handler, the same pattern works with the typed response body:

```csharp
pipeline.Get("/api/residents/42")
    .Response(r => r.OnSuccess<ResidentResponse>((json, pipeline) =>
    {
        pipeline.Element("name").SetText(json, x => x.FullName);
        pipeline.Element("facility").SetText(json, x => x.FacilityName);
    }));
```

The expression `x => x.FullName` compiles to `"responseBody.fullName"` -- a different prefix, but the same walk mechanism.

### Values from a component

You can also pass a component's value directly. This is useful when displaying what a user selected:

```csharp
var careLevel = pipeline.Component<FusionDropDownList>(m => m.CareLevel);
pipeline.Element("selected-level").SetText(careLevel.Value());
```

## How do I show and hide elements?

```csharp
pipeline.Element("success-message").Show();   // Removes the "hidden" attribute
pipeline.Element("loading-spinner").Hide();   // Sets the "hidden" attribute
```

A common pattern -- toggle visibility on events:

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("loading").Show();
    pipeline.Element("content").Hide();
    pipeline.Dispatch("data-ready");
}));

Html.On(plan, t => t.CustomEvent("data-ready", pipeline =>
{
    pipeline.Element("loading").Hide();
    pipeline.Element("content").Show();
}));
```

## How does source binding work?

When you write `pipeline.Element("city").SetText(payload, x => x.Address.City)`, the expression is walked at build time: `Address` → `address`, `City` → `city`, prefixed to `"evt.address.city"`. At runtime, the event payload is walked at that path and the value is set as text. The same mechanism works for response body paths (`"responseBody.fullName"`) and component sources.

**Next:** [Component API](/alis-reactive/csharp-modules/reactivity/component-api/) — targeting input components, app-level singletons, and reading values for conditions.
