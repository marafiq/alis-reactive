---
title: Triggers
description: DomReady, CustomEvent, typed payloads, component events, SSE, and SignalR.
sidebar:
  order: 1
---

A trigger defines *when* a pipeline executes. Every reactive behavior starts with one — page load, a named event, a component interaction, or a real-time server push.

### Placement — anywhere in the view

`Html.On()` builds a **descriptor** and adds it to the plan. It does not execute anything. You can call it anywhere in the `.cshtml` — top, middle, bottom, inside conditionals, in loops. Order doesn't matter. The calls just accumulate entries in the plan object, and `@Html.RenderPlan(plan)` serializes them all to JSON at the end.

```csharp
@{
    // These three calls can appear in any order — they all just add entries to `plan`
    Html.On(plan, t => t.DomReady(p => p.Dispatch("init")));
    Html.On(plan, t => t.SignalR("/hubs/alerts", "Receive", p => p.Element("x").Show()));
    Html.On(plan, t => t.CustomEvent("init", p => p.Element("status").SetText("ready")));
}

<!-- HTML here — the triggers above don't care about their position relative to markup -->

@Html.RenderPlan(plan)  @* Serializes all entries to JSON — nothing connects until the browser boots *@
```

### Lazy connections — no connection until the browser boots

Writing `t.SignalR(...)` or `t.ServerPush(...)` in C# does **not** open a WebSocket or EventSource. It produces a JSON descriptor — data, not execution. The actual connection only happens when the browser loads the page and the JS runtime processes the plan during boot.

- **No page load = no connection.** If the view is never rendered, no resources are consumed.
- **Partial views are lazy too.** A partial loaded via `.Into()` only connects when the partial arrives and its plan merges.
- **Connection pooling is automatic.** Multiple triggers on the same URL share one connection — the runtime deduplicates.

This means real-time triggers are safe to declare unconditionally. They cost nothing until the page actually loads in a browser.

From the [Grammar Tree](../../mental-model/#the-grammar-tree) — the trigger-related API:

```
Html.On(plan, t => ...)                              § Triggers (what fires the pipeline)
├── t.DomReady(pipeline => { ... })                  on page load — fires once
├── t.CustomEvent("name", pipeline => { ... })       on named event — fires each time dispatched
├── t.CustomEvent<T>("name", (payload, pipeline) => { ... })
│                                                    on named event with typed payload
├── t.ServerPush(url, pipeline => { ... })            on SSE message — native EventSource
├── t.ServerPush<T>(url, eventType, (payload, p) => { ... })
│                                                    on named SSE event with typed payload
├── t.SignalR(hubUrl, method, pipeline => { ... })    on SignalR hub method invocation
└── t.SignalR<T>(hubUrl, method, (payload, p) => { ... })
                                                     on hub method with typed payload

.Reactive(plan, evt => evt.{EventName}, (args, pipeline) => { ... })
                                                     on component event — wired on the builder
```

## DomReady

Fires once when the page finishes loading. Use it to set initial UI state or dispatch events that kick off other behavior.

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("status").SetText("System online");
    pipeline.Dispatch("page-ready");
}));
```

## CustomEvent

Fires when something dispatches the matching event name. Events are dispatched with `pipeline.Dispatch("name")` from any pipeline — this is how different parts of the page communicate.

```csharp
Html.On(plan, t => t.CustomEvent("page-ready", pipeline =>
{
    pipeline.Element("form-section").Show();
}));
```

## CustomEvent with typed payload

When the event carries data, use `CustomEvent<TPayload>`. The `payload` parameter gives you compile-time access to the event's properties — `x => x.Name` becomes `"evt.name"` in the plan, resolved from the actual event data at runtime.

```csharp
Html.On(plan, t => t.CustomEvent<ResidentCreatedPayload>("resident-created", (payload, pipeline) =>
{
    pipeline.Element("name-display").SetText(payload, x => x.Name);
}));
```

Dispatching with data:

```csharp
pipeline.Dispatch("resident-created", new ResidentCreatedPayload
{
    Name = "Jane Doe",
    Facility = "Sunrise Manor"
});
```

## .Reactive() — component events

Components fire their own events — `Changed`, `Click`, `Filtering`. Wire them with `.Reactive()` directly on the component builder. `evt` selects the event, `args` gives you the typed payload.

```csharp
Html.InputField(plan, m => m.Country, o => o.Required().Label("Country"))
    .FusionDropDownList(b => b
        .Fields<CountryItem>(t => t.Text, v => v.Value)
        .Reactive(plan, evt => evt.Changed, (args, pipeline) =>
        {
            pipeline.Element("selected").SetText(args, a => a.Value);
        }));
```

`.Reactive()` is for a specific component's event — the trigger lives with the component. `CustomEvent` is for named events that any part of the page can dispatch or listen to.

## ServerPush — Server-Sent Events (SSE)

Fires when the server pushes a message via an SSE endpoint. Uses the browser's native `EventSource` API — zero JS library, automatic reconnection built into the spec.

```csharp
// Listen for all messages from an SSE stream
Html.On(plan, t => t.ServerPush("/api/alerts/stream", pipeline =>
{
    pipeline.Dispatch("alert-received");
}));
```

### Named event types

SSE supports named event types via the `event:` field. Filter by type to handle different message kinds from the same stream:

```csharp
Html.On(plan, t => t.ServerPush("/api/stream", "notification", pipeline =>
{
    pipeline.Element("alert-badge").Show();
}));
```

### Typed payload

When the SSE message carries structured JSON data, use `ServerPush<T>` for compile-time expression access:

```csharp
Html.On(plan, t => t.ServerPush<FacilityAlert>("/api/alerts/stream", "facility-alert",
    (alert, pipeline) =>
{
    pipeline.Element("alert-message").SetText(alert, x => x.Message);
    pipeline.Element("alert-level").SetText(alert, x => x.Level);
}));
```

The SSE endpoint sends JSON in the `data:` field:

```csharp
// ASP.NET Core SSE endpoint — standard, nothing framework-specific
[HttpGet("/api/alerts/stream")]
public async Task AlertStream(CancellationToken ct)
{
    Response.ContentType = "text/event-stream";
    Response.Headers.CacheControl = "no-cache";

    var json = JsonSerializer.Serialize(new { message = "Fire drill complete", level = "info" });
    await Response.WriteAsync($"event: facility-alert\ndata: {json}\n\n", ct);
    await Response.Body.FlushAsync(ct);

    await Task.Delay(Timeout.Infinite, ct); // Keep connection open
}
```

## SignalR — Hub method triggers

Fires when the server invokes a Hub method via `IHubContext`. Uses `@microsoft/signalr` — WebSocket with automatic reconnection, connection pooling, and retry indicator on disconnection.

```csharp
Html.On(plan, t => t.SignalR("/hubs/notifications", "ReceiveNotification", pipeline =>
{
    pipeline.Dispatch("notification-arrived");
}));
```

### Typed payload

Use `SignalR<T>` for compile-time access to the Hub method's payload:

```csharp
Html.On(plan, t => t.SignalR<NotificationPayload>("/hubs/notifications", "ReceiveNotification",
    (payload, pipeline) =>
{
    pipeline.Element("count").SetText(payload, x => x.Count);
    pipeline.Element("message").SetText(payload, x => x.Message);
}));
```

### Multiple hubs

Each `hubUrl` gets one WebSocket connection. Multiple triggers on the same hub share the connection:

```csharp
Html.On(plan, t => t
    // Hub 1: notifications
    .SignalR<NotificationPayload>("/hubs/notifications", "ReceiveAlert", (payload, p) =>
    {
        p.Element("alert-text").SetText(payload, x => x.Message);
    })
    // Hub 2: resident status (separate connection)
    .SignalR<ResidentStatusPayload>("/hubs/resident-status", "StatusChanged", (payload, p) =>
    {
        p.Element("resident-name").SetText(payload, x => x.ResidentName);
    }));
```

### Server side — unchanged

Hubs are standard ASP.NET Core SignalR. Nothing about how you write or register hubs changes:

```csharp
// Standard Hub — no framework dependency
public class NotificationHub : Hub { }

// Push via IHubContext from any service or controller
public class AlertService(IHubContext<NotificationHub> hub)
{
    public async Task Broadcast(string message, int count)
    {
        await hub.Clients.All.SendAsync("ReceiveNotification",
            new NotificationPayload { Message = message, Count = count });
    }
}

// Register in Program.cs
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notifications");
```

### Connection lifecycle

The runtime handles everything automatically:

- **Auto-reconnect**: `withAutomaticReconnect()` retries at 0s, 2s, 10s, 30s
- **Initial retry**: If the first connection fails, retries 5 times with backoff
- **Connection pooling**: Multiple triggers on the same `hubUrl` share one WebSocket
- **Retry indicator**: When all retries are exhausted, a subtle retry icon appears on the target element — click to reconnect
- **Handler persistence**: `.on()` handlers survive across reconnections — no re-registration needed
- **Partial view support**: Partials with their own plan reuse the parent's connection via plan merging

### SSE vs SignalR — when to use which

| | SSE (`ServerPush`) | SignalR |
|---|---|---|
| **Direction** | Server → Client only | Bidirectional (but DSL exposes server → client) |
| **Library** | Native `EventSource` (0 bytes) | `@microsoft/signalr` (~50kb bundled) |
| **Auto-reconnect** | Browser built-in | Library built-in |
| **Server side** | Raw HTTP endpoint | Hub class + `MapHub<T>()` |
| **Best for** | Simple streams, alerts | Rich notifications, multi-hub, groups |

## Event chaining

DomReady dispatches → CustomEvent catches → dispatches again. The runtime guarantees all CustomEvent listeners are wired before any DomReady reaction fires — order in the view doesn't matter.

```csharp
Html.On(plan, t => t.DomReady(pipeline =>
{
    pipeline.Element("step-1").SetText("Loaded");
    pipeline.Dispatch("loaded");
}));

Html.On(plan, t => t.CustomEvent("loaded", pipeline =>
{
    pipeline.Element("step-2").SetText("Ready");
}));
```

Next: [Element Mutations](../element-mutations/) — what you can do inside a pipeline.
