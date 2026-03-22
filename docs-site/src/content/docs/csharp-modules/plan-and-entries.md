---
title: Plans and Rendering
description: How to create a reactive plan, add behavior to it, and render it for the browser runtime.
sidebar:
  order: 1
---

A plan is the container for all reactive behavior in a view. You create it at the top, wire triggers and components into it throughout the body, and render it at the bottom. The runtime discovers the rendered JSON on page load and boots it automatically.

From the [Grammar Tree](/alis-reactive/csharp-modules/mental-model/#the-grammar-tree) — the plan-related API:

```
Html.
├── ReactivePlan<TModel>()                          create a plan scoped to your model
├── ResolvePlan<TModel>()                           partial plan (merges into parent)
├── RenderPlan(plan)                                serialize and emit at bottom of view
│
├── On(plan, t => ...)                              add triggers to the plan
├── InputField(plan, m => m.Prop, o => ...)          add components to the plan
├── NativeButton("id", "text").Reactive(plan, ...)   add reactive behavior to the plan
└── NativeHiddenField(plan, m => m.Prop)              add hidden input to the plan
```

Every method takes `plan` as its first argument — the plan is the thread that connects them all.

## What is a plan?

Every view that uses Reactive must have a strongly-typed `@model` — no `dynamic`, no `ViewBag`. `Html.ReactivePlan<TModel>()` creates a plan scoped to that model. `TModel` is inferred from `@model`, so every expression — `m => m.Name`, `m => m.Country` — is type-checked at compile time. Partials use `Html.ResolvePlan<TModel>()` to merge into the parent. Both require `@Html.RenderPlan(plan)` at the bottom.

Every `Html.On`, `Html.InputField`, and `.Reactive()` call adds entries to the plan. Nothing executes at this point — it's all descriptors collected into a list.

`Html.RenderPlan(plan)` serializes the collected entries to JSON and emits a `<script type="application/json">` element into the page. The runtime reads it and executes the described behavior.

## The standard view shape

```csharp
@model ResidentIntakeModel
@using Alis.Reactive.Native.Extensions
@{
    var plan = Html.ReactivePlan<ResidentIntakeModel>();

    Html.On(plan, t => t.DomReady(pipeline =>
    {
        pipeline.Element("status").SetText("Form ready");
    }));
}

<h1>Resident Intake</h1>
<p id="status"></p>

@* InputField, NativeButton, .Reactive() — all pass plan *@

@Html.RenderPlan(plan)
```

1. `ReactivePlan` at the top — creates the plan
2. `Html.On` / `InputField` / `.Reactive()` in the body — adds behavior to the plan
3. HTML with element IDs and components
4. `RenderPlan` at the bottom — serializes everything to JSON

See the full [Grammar Tree](/alis-reactive/csharp-modules/mental-model/#the-grammar-tree) for what's available inside `pipeline`.

## Partial views

**Same model** — the partial is part of the parent form. `Html.ResolvePlan<TModel>()` creates a plan that the runtime merges into the parent by matching the model type:

```csharp
@* _AddressPartial.cshtml *@
@model ResidentIntakeModel
@{
    var plan = Html.ResolvePlan<ResidentIntakeModel>();
}

@* InputField, .Reactive() — same plan, same model *@

@Html.RenderPlan(plan)
```

**Different model** — the partial is independent. Use `Html.ReactivePlan<TModel>()` with its own model type. It gets its own plan, its own JSON, its own boot cycle.

## When does the plan execute?

Never on the server. `ReactivePlan` collects descriptors, `RenderPlan` serializes them. The JSON is inert until the runtime reads it in the browser. This separation is why there's no JavaScript in your views — the C# fluent builders describe intent, the runtime executes it.

Next: [Triggers](/alis-reactive/csharp-modules/reactivity/triggers-and-reactions/) — the different ways to say *when* something should happen.
