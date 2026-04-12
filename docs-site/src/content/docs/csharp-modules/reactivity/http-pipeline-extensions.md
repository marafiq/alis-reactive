---
title: HTTP Pipeline Extensions
description: Custom headers, URL templates with route parameters, URL query parameters, and method arguments for advanced HTTP requests.
sidebar:
  order: 6
---

These features extend the [HTTP Pipeline](../http-pipeline/). If you haven't read that page yet, start there — it covers GET/POST/PUT/DELETE, gather, response handlers, and parallel requests.

## How do I add custom headers to a request?

Pass a literal string value:

```csharp
.Gather(g => g.Header("X-Api-Key", "my-key"))
```

The header is added to the HTTP request alongside any gather items.

### What if the header value comes from a component?

Pass any typed source — a component read, a URL param, or a plugin result:

```csharp
var tenant = pipeline.Component<FusionDropDownList>(m => m.TenantId);
.Gather(g => g.Header("X-Tenant", tenant.Value()))
```

### What if it comes from the event that triggered this pipeline?

Use the event args expression:

```csharp
.Gather(g => g.Header("X-Correlation", args, x => x.CorrelationId))
```

### What types can I use for header values?

Headers must be scalar — string, int, or bool. Arrays and objects are rejected at build time with a clear error message.

## How do I use route parameters in a URL?

Put placeholders in the URL with `{name}`, then supply values via `RouteParam`:

```csharp
pipeline.Put("/api/residents/{id}", g => g.RouteParam("id", 42))
    .Response(r => r.OnSuccess(s => s.Element("status").SetText("Updated")));
```

### What if the route value comes from a form field?

Pass a typed source:

```csharp
var residentId = pipeline.Component<FusionDropDownList>(m => m.ResidentId);
g.RouteParam("id", residentId.Value());
```

### What if it comes from event args?

```csharp
g.RouteParam("id", args, x => x.ResidentId)
```

### Can I have multiple route parameters?

Yes — each placeholder gets its own `RouteParam`:

```csharp
pipeline.Get("/api/facilities/{facilityId}/residents/{residentId}")
    .Gather(g =>
    {
        g.RouteParam("facilityId", 7);
        g.RouteParam("residentId", 99);
    })
    .Response(r => r.OnSuccess(s => s.Element("result").SetText("Loaded")));
```

### What happens if I forget a route parameter?

Every `{placeholder}` must have a matching `RouteParam`, and every `RouteParam` must match a `{placeholder}`. Both directions are validated at build time — mismatches throw immediately.

## How do I read a URL query parameter?

The simplest case — forward the browser's `?tab=medications` into a gather:

```csharp
g.FromUrl("tab")
```

This reads the `tab` query parameter and sends it as the request key `"tab"`.

### What if I need a typed value?

```csharp
g.FromUrl<int>("page")
```

Type conversion is automatic — the runtime coerces the URL string to the target type.

### What if I want a different request parameter name?

```csharp
g.FromUrl("q", "searchTerm")
```

Reads `?q=smith` from the URL, sends it as `"searchTerm"` in the request.

### Can I branch on a URL parameter?

Yes — use `pipeline.FromUrl()` in a condition:

```csharp
pipeline.When(pipeline.FromUrl("tab")).Eq("medications")
    .Then(then => then.Element("med-panel").Show())
    .Else(else_ => else_.Element("med-panel").Hide());
```

For typed comparisons:

```csharp
pipeline.When(pipeline.FromUrl<int>("page")).Gt(1)
    .Then(then => then.Element("prev-btn").Show())
    .Else(else_ => else_.Element("prev-btn").Hide());
```

### Can I display a URL parameter?

```csharp
pipeline.Element("current-tab").SetText(pipeline.FromUrl("tab"));
pipeline.Element("current-facility").SetText(pipeline.FromUrl("facilityId"));
```

### Can I combine URL params with headers and route params?

Yes — all gather strategies compose freely:

```csharp
pipeline.Put("/api/residents/{id}", g =>
    {
        g.RouteParam("id", 42);
        g.Header("X-Tab", pipeline.FromUrl("tab"));
        g.FromUrl("facilityId", "facility");
    })
    .Response(r => r.OnSuccess(s => s.Element("status").SetText("Saved")));
```

## How do I pass arguments to a method read?

The most common case — pass a response body property to a plugin method:

```csharp
var count = pipeline.Plugin<int>("array", "count").Arg(json, x => x.Items);
s.Element("total").SetText(count);
```

The result converts automatically — no `.Build()` needed.

### What if the argument comes from event args?

```csharp
pipeline.Plugin<int>("array", "count").Arg(args, x => x.ResidentId)
```

### What about a component value or a literal?

```csharp
.Arg(tenant.Value())    // from a component
.Arg("active")          // string literal
.Arg(42)                // int literal
```

Arguments chain — call `.Arg()` multiple times to pass multiple values to the JS method.

### Where can I use the result?

Anywhere a typed source is accepted:

```csharp
// In SetText
s.Element("count").SetText(count);

// In a condition
pipeline.When(count).Gt(0)
    .Then(then => then.Element("results").Show());

// In a header
g.Header("X-Count", count);

// In a gather
g.Plugin(count, "totalCount");
```

**Previous:** [HTTP Pipeline](../http-pipeline/) — GET/POST/PUT/DELETE, gather, loading states, typed responses, and chained/parallel requests.

**Next:** [Plugin System](../plugins/) — register JS plugins, read method results, call void methods, and compose plugin chains.
