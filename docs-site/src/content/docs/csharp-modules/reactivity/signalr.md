---
title: SignalR
description: Real-time hub method triggers using WebSocket transport — automatic reconnection, connection pooling, strict payload validation.
sidebar:
  order: 8
---

SignalR triggers fire when the server invokes a hub method via `IHubContext`. The runtime uses `@microsoft/signalr` with WebSocket transport, automatic reconnection, and connection pooling across triggers.

## How do I listen for a hub method?

Use `t.SignalR()` in a trigger builder:

```csharp
Html.On(plan, t => t.SignalR("/hubs/notifications", "ReceiveNotification", pipeline =>
{
    pipeline.Dispatch("notification-arrived");
}));
```

This wires a handler for the `ReceiveNotification` method on the `/hubs/notifications` hub.

## How do I access the payload?

Use `SignalR<T>` with a typed payload class:

```csharp
public class NotificationPayload
{
    public int Count { get; set; }
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "normal";
}
```

```csharp
Html.On(plan, t => t.SignalR<NotificationPayload>("/hubs/notifications", "ReceiveNotification",
    (payload, pipeline) =>
{
    pipeline.Element("notif-count").SetText(payload, x => x.Count);
    pipeline.Element("notif-message").SetText(payload, x => x.Message);
    pipeline.Element("notif-priority").SetText(payload, x => x.Priority);
}));
```

The expression `x => x.Count` compiles to `"evt.count"`. At runtime, the SignalR method argument is walked at that path.

**Strict validation:** The server must send a single object argument. The runtime throws immediately if:
- Zero arguments are sent
- Multiple arguments are sent
- The argument is null or not an object

The error message includes the hub URL and method name to help diagnose the issue.

## How do I write the server side?

Standard ASP.NET Core SignalR. Nothing framework-specific.

### Define a hub

```csharp
public class NotificationHub : Hub { }
```

Hubs can be empty — the server broadcasts via `IHubContext<T>` from controllers or services.

### Register in Program.cs

```csharp
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notifications");
```

### Broadcast from a controller or service

```csharp
public class AlertService(IHubContext<NotificationHub> hub)
{
    public async Task Broadcast(string message, int count)
    {
        // Single object argument — the runtime expects one deserialized object
        await hub.Clients.All.SendAsync("ReceiveNotification",
            new NotificationPayload { Message = message, Count = count });
    }
}
```

**Single object argument rule:** `SendAsync` must pass exactly one argument that is a non-null object. Primitives, nulls, and multi-argument sends are rejected by the runtime. This is enforced because the runtime deserializes the argument and walks it as an object — primitives have no properties to walk.

## How do I listen to multiple methods on the same hub?

Chain triggers on the same hub URL. They share one WebSocket:

```csharp
Html.On(plan, t => t
    .SignalR<NotificationPayload>("/hubs/notifications", "ReceiveAlert", (payload, p) =>
    {
        p.Element("alert-text").SetText(payload, x => x.Message);
    })
    .SignalR<NotificationPayload>("/hubs/notifications", "ReceiveUpdate", (payload, p) =>
    {
        p.Element("update-count").SetText(payload, x => x.Count);
    }));
```

## How do I use multiple hubs?

Different hub URLs create separate WebSocket connections:

```csharp
Html.On(plan, t => t
    .SignalR<NotificationPayload>("/hubs/notifications", "ReceiveNotification",
        (payload, p) =>
    {
        p.Element("notif-message").SetText(payload, x => x.Message);
    })
    .SignalR<ResidentStatusPayload>("/hubs/resident-status", "StatusChanged",
        (payload, p) =>
    {
        p.Element("resident-name").SetText(payload, x => x.ResidentName);
        p.Element("resident-status").SetText(payload, x => x.Status);
    }));
```

Two hub URLs, two WebSockets. Each is managed independently.

## How does connection management work?

### Lazy connections

Writing `t.SignalR(...)` in C# does **not** open a WebSocket. It produces a JSON descriptor. The connection only opens when the browser boots and processes the plan.

### Connection pooling

The runtime maintains a `Map<string, ManagedConnection>` — one connection per unique hub URL. All triggers on the same hub share one WebSocket, regardless of which methods they listen to.

### Automatic reconnection

The runtime configures `withAutomaticReconnect()` with the default backoff: 0s, 2s, 10s, 30s. This handles reconnection **after a successful initial connection**. If the WebSocket drops temporarily, the library reconnects automatically.

### Initial connection retry

If the **first connection** fails (e.g., the server is starting up), the runtime retries with the same backoff delays (0s, 2s, 10s, 30s) for a total of 4 attempts. This is separate from the library's automatic reconnect, which only kicks in after a successful start.

### Retry indicator

When all retries are exhausted (both initial and reconnection), a retry indicator appears near the first mutated element — the same clickable button used by SSE. Clicking it creates a fresh connection attempt.

### Handler persistence

`.on()` handlers survive across reconnections. When the library reconnects after a temporary drop, all registered method handlers continue to work without re-registration.

### Partial view support

Partials that declare their own plan with the same hub URL reuse the parent's connection via plan merging. The runtime sees the same hub URL and joins the existing managed connection.

### Cookie authentication

Connections use `withCredentials: true` by default — cookies are sent automatically, so session-based authentication works without extra configuration.

### Intentional close

When an `AbortSignal` fires (page navigation, cleanup), the connection closes gracefully without showing a retry indicator. The runtime sets an internal `stopping` flag to distinguish intentional closes from connection loss.

## Can I use conditions and HTTP in SignalR pipelines?

Yes. The pipeline inside a SignalR trigger is the same full pipeline builder used everywhere else:

```csharp
Html.On(plan, t => t.SignalR<NotificationPayload>("/hubs/notifications", "ReceiveNotification",
    (payload, p) =>
{
    p.When(payload, x => x.Priority).Eq("urgent")
        .Then(then =>
        {
            then.Component<FusionToast>()
                .SetTitle("Urgent Alert")
                .SetContent("Immediate attention required")
                .Danger()
                .Show();
        })
        .Else(else_ =>
        {
            else_.Element("notif-message").SetText(payload, x => x.Message);
        });
}));
```

You can branch on payload values, make HTTP requests, show toasts, update components — the full DSL is available.

**Previous:** [Server-Sent Events](../server-push/) — SSE with native EventSource, zero library overhead.

**Next:** [Element Mutations](../element-mutations/) — targeting DOM elements for text, HTML, classes, and visibility.
