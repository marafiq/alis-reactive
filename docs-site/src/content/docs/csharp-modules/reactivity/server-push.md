---
title: Server-Sent Events (SSE)
description: Real-time server-to-client data push using the browser's native EventSource API — zero library, automatic reconnection, connection pooling.
sidebar:
  order: 7
---

ServerPush triggers fire when the server pushes data via Server-Sent Events. The browser's native `EventSource` API handles the connection — zero JS library needed, automatic reconnection built into the spec.

## How do I listen for SSE messages?

Use `t.ServerPush()` in a trigger builder. The simplest form listens for all messages from a stream:

```csharp
Html.On(plan, t => t.ServerPush("/api/alerts/stream", pipeline =>
{
    pipeline.Element("alert-badge").Show();
    pipeline.Dispatch("alert-received");
}));
```

## How do I filter by event type?

SSE supports named event types via the `event:` field in the protocol. Pass an event type to only handle messages of that type:

```csharp
Html.On(plan, t => t.ServerPush("/api/facility-alerts", "facility-alert", pipeline =>
{
    pipeline.Element("alert-status").SetText("Alert received");
}));
```

This only fires when the server sends `event: facility-alert` in the SSE stream. Other event types on the same stream are ignored by this trigger.

## How do I access the payload?

Use `ServerPush<T>` with a typed payload class for compile-time access to properties:

```csharp
public class FacilityAlert
{
    public string Message { get; set; } = "";
    public string Level { get; set; } = "";
}
```

```csharp
Html.On(plan, t => t.ServerPush<FacilityAlert>("/api/facility-alerts", "facility-alert",
    (alert, pipeline) =>
{
    pipeline.Element("alert-message").SetText(alert, x => x.Message);
    pipeline.Element("alert-level").SetText(alert, x => x.Level);
}));
```

The expression `x => x.Message` compiles to `"evt.message"` in the plan. At runtime, the parsed SSE JSON data is walked at that path. The payload class must have a parameterless constructor.

## How do I write the server endpoint?

A standard ASP.NET Core HTTP endpoint. Nothing framework-specific:

```csharp
[HttpGet("/api/facility-alerts")]
public async Task FacilityAlertStream(CancellationToken ct)
{
    Response.ContentType = "text/event-stream";
    Response.Headers["Cache-Control"] = "no-cache";
    Response.Headers.Connection = "keep-alive";

    var alert = JsonSerializer.Serialize(
        new { message = "Facility check complete", level = "info" },
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    await Response.WriteAsync($"event: facility-alert\ndata: {alert}\n\n", ct);
    await Response.Body.FlushAsync(ct);

    // Keep connection open until client disconnects
    try { await Task.Delay(Timeout.Infinite, ct); }
    catch (OperationCanceledException) { }
}
```

**SSE protocol format:** Each message is `event: {type}\ndata: {json}\n\n`. The `event:` line is optional — omit it for unnamed messages (handled by `ServerPush` without an event type parameter). **Data must be valid JSON** — the runtime calls `JSON.parse()` and throws immediately on non-JSON data. Use `camelCase` properties — the runtime resolves `evt.message`, not `evt.Message`.

## How does connection management work?

### Lazy connections

Writing `t.ServerPush(...)` in C# does **not** open an `EventSource`. It produces a JSON descriptor. The actual connection only opens when the browser loads the page and the runtime processes the plan during boot.

### Connection pooling

Multiple triggers on the same URL share one `EventSource`. The runtime maintains a `Map<string, ManagedSource>` — one entry per unique URL. If three triggers all listen to `/api/alerts/stream` (even for different event types), only one HTTP connection is created.

### Automatic reconnection

The browser's `EventSource` spec handles transient errors automatically — if the connection drops temporarily, the browser reconnects without any framework intervention.

### Permanent close and retry

When the browser determines the connection is permanently lost (`readyState === CLOSED`), the runtime shows a retry indicator near the first mutated element. The retry indicator is a small clickable button that:

- Appears as an absolutely-positioned element on the mutation target's parent
- Creates a fresh `EventSource` on click and re-wires all handlers for that URL
- Is removed automatically when the new connection opens successfully

### Intentional close

When a page navigates away or an `AbortSignal` fires, the connection closes without showing a retry indicator. This prevents false "connection lost" indicators during normal navigation.

## Can I listen to multiple event types on the same stream?

Yes. Each trigger registers its own event listener on the shared `EventSource`:

```csharp
Html.On(plan, t => t
    .ServerPush<FacilityAlert>("/api/alerts/stream", "facility-alert", (alert, p) =>
    {
        p.Element("facility-message").SetText(alert, x => x.Message);
    })
    .ServerPush<MaintenanceAlert>("/api/alerts/stream", "maintenance", (alert, p) =>
    {
        p.Element("maintenance-message").SetText(alert, x => x.Description);
    }));
```

Both use the same URL — one `EventSource`, two event listeners.

## When should I use SSE vs SignalR?

| | SSE (`ServerPush`) | SignalR |
|---|---|---|
| **Direction** | Server to client only | Bidirectional (DSL exposes server to client) |
| **Library** | Native `EventSource` (0 bytes) | `@microsoft/signalr` (~50kb bundled) |
| **Auto-reconnect** | Browser built-in | Library built-in |
| **Server side** | Raw HTTP endpoint | Hub class + `MapHub<T>()` |
| **Best for** | Simple streams, alerts, status feeds | Rich notifications, multi-hub, groups |

**Choose SSE when** you need a lightweight one-way stream with zero client-side library overhead. **Choose SignalR when** you need multiple hubs, group-based routing, or your infrastructure already uses SignalR.

**Previous:** [Triggers & Real-Time](../triggers-and-reactions/) — DomReady, CustomEvent, component events, and the trigger builder API.

**Next:** [SignalR](../signalr/) — real-time hub method triggers with WebSocket transport.
