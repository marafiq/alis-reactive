---
title: Plan Model
description: How a reactive plan is authored, rendered, merged, and executed in the V2 design.
sidebar:
  order: 1
---

A plan is the rendered V2 contract for one view or partial. It is not a bag of entries. It is one document with four domain objects:

- `contracts`
- `objects`
- `bindings`
- `workflows`

`@Html.RenderPlan(plan)` emits that document as JSON for the browser runtime.

## Standard view shape

```csharp
@model ResidentIntakeModel
@using Alis.Reactive.Native.Extensions
@{
    var plan = Html.ReactivePlan<ResidentIntakeModel>();

    Html.On(plan, t => t.DomReady(p =>
    {
        p.Element("status").SetText("ready");
    }));
}

<p id="status"></p>

@Html.RenderPlan(plan)
```

The server never executes browser behavior. It only authors the plan document.

## What the plan contains

### Contracts

Contracts describe capabilities. They hide vendor differences behind named members and events.

Examples:

- a native text input exposes `value`
- a Syncfusion dropdown exposes `value`, `text`, `showPopup`
- an event object exposes `text`, `preventDefaultAction`, `updateData`

### Objects

Objects name concrete runtime instances. They bind a contract to an element id or an app-level object.

Examples:

- `residentName` -> native text contract + `Resident_Name`
- `statusField` -> fusion dropdown contract + `Resident_Status`
- `toast` -> app-level toast contract

### Bindings

Bindings are canonical reads for model-facing data. Validation, request gather, and typed conditions should flow through bindings instead of inventing new access shapes.

### Workflows

A workflow connects:

- one subscription
- one action tree

Subscriptions cover dom-ready, custom events, object events, server push, and SignalR. Actions cover sequence, branch, set, call, request, inject, dispatch, and validation display.

## Partials and merge

`Html.ResolvePlan<TModel>()` creates a partial plan that merges into the parent by `planId`.

Merge behavior in V2 is explicit:

- `contracts` merge by name
- `objects`, `bindings`, and `workflows` are owned by `sourceId`
- removing a partial removes its owned slices cleanly

The runtime should never scan the DOM to rediscover what the plan already knows.

## Authoring rule

If a feature cannot be described as:

- contract member access
- binding read
- predicate evaluation
- action execution

then it does not belong in the framework yet.

Next: [Subscriptions and Workflows](./reactivity/subscriptions-and-workflows/) for the different ways to start behavior.
