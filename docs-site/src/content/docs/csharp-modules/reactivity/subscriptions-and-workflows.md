---
title: Subscriptions and Workflows
description: DomReady, document events, component events, server push, and SignalR in the V2 workflow model.
sidebar:
  order: 1
---

Reactive behavior starts with a subscription and runs through a workflow.

- A subscription says when execution starts.
- A workflow says what action tree runs.

No browser work happens while Razor is rendering. C# only authors the workflow.

## Subscription surface

`Html.On(plan, t => ...)` adds document-level subscriptions.

```csharp
Html.On(plan, t => t.DomReady(p =>
{
    p.Dispatch("resident-editor.ready");
}));
```

Component builders add object-event subscriptions through `.Reactive(...)`.

```csharp
Html.InputField(plan, m => m.Country, o => o.Required())
    .FusionDropDownList()
    .Reactive(evt => evt.Changed, (args, p) =>
    {
        p.Element("status").SetText("changed");
    });
```

## Supported subscriptions

- `DomReady`
- `CustomEvent`
- typed `CustomEvent<TPayload>`
- `ServerPush`
- typed `ServerPush<TPayload>`
- `SignalR`
- typed `SignalR<TPayload>`
- component `.Reactive(...)` event subscriptions

All of them compile to V2 workflow subscriptions. None of them create legacy trigger objects.

## Typed payloads

Typed payloads stay compile-time safe in C# and serialize as value expressions in the rendered plan.

```csharp
public sealed class ResidentCreatedPayload
{
    public string Name { get; set; } = "";
}

Html.On(plan, t => t.CustomEvent<ResidentCreatedPayload>("resident-created", (payload, p) =>
{
    p.Element("name").SetText(payload, x => x.Name);
}));
```

The `payload` argument is an authoring proxy. Runtime values come from the actual browser event payload.

## Object events

Component `.Reactive(...)` wiring is just another subscription form. The difference is that the plan already knows:

- which object is involved
- which contract it uses
- which event name maps to the browser event

That is why the runtime can stay dumb.

## Workflow actions

Inside a workflow you compose actions:

- `Element(...).SetText(...)`
- `Component<T>(...).SetValue(...)`
- `Dispatch(...)`
- `When(...).Then(...)`
- `Get/Post/Put/Delete`
- `Parallel(...)`
- validation and partial injection flows

Nested actions become branches, sequences, requests, and child workflows in the final JSON.

## Lazy connections

SignalR and SSE subscriptions are lazy:

- no page load means no connection
- no partial merge means no connection
- no workflow means no listener

The runtime wires only what the plan declares.
