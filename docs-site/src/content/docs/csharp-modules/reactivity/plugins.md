---
title: Plugin System
description: Register JavaScript plugins, read their method results in the DSL, call void methods, chain plugin output as input, and use plugins in conditions and gather.
sidebar:
  order: 7
---

A plugin is a native JS object you expose to the C# DSL. You write the JavaScript implementation once, register it in C#, and the framework calls it at runtime. No `<script>` blocks in views.

```csharp
var count = pipeline.Plugin<int>("array", "count").Arg(json, x => x.Items);
s.Element("total").SetText(count);
```

That's the end result — two lines to call a JS function from C#. The rest of this page shows how to get there.

## How do I register a plugin?

Two sides: C# declares the type metadata, JS provides the implementation.

**C# — declare what the plugin offers:**

```csharp
plan.RegisterPlugin("array", p =>
{
    p.Method<int>("count");
    p.Command("track");
});
```

**JS — push the instance before the framework loads:**

```javascript
window.__alisPlugins ??= [];
window.__alisPlugins.push({
    name: "array",
    instance: { count: (arr) => arr?.length ?? 0 }
});
```

The runtime drains `window.__alisPlugins` at boot. Registration must happen before any `pipeline.Plugin()` reference in the plan.

### What if my plugin has multiple methods?

Add them in the same registration call:

```csharp
plan.RegisterPlugin("array", p =>
{
    p.Method<int>("count");
    p.Method<object>("filter");
    p.Method<int>("sum");
    p.Method<bool>("some");
    p.Command("track");
});
```

`Method<T>()` declares a method that returns a typed value. `Command()` declares a method with no return value. `Void()` remains available as the compatibility name.

## How do I read a plugin method's return value?

Call `pipeline.Plugin<T>()` with the plugin name and method name:

```csharp
var countSrc = pipeline.Plugin<int>("array", "count")
    .Arg(json, x => x.Items);
s.Element("total").SetText(countSrc);
```

The result converts to a typed source automatically — no `.Build()` needed.

### Where can I use the result?

Anywhere a typed source is accepted — `SetText`, `When`, `Header`, `Gather`:

```csharp
s.Element("total").SetText(countSrc);
```

### How do I pass multiple arguments?

Chain `.Arg()` calls:

```csharp
var pluckSrc = pipeline.Plugin<string>("array", "pluck")
    .Arg(json, x => x.Items).Arg(0).Arg("name");
s.Element("first-name").SetText(pluckSrc);
```

Each `.Arg()` accepts a response body expression, event args, a typed source, or a literal (string, int, bool, long).

## How do I call a plugin method that returns nothing?

Use the non-generic `pipeline.Plugin()` and end with `.Fire()`:

```csharp
s.Plugin("analytics", "track").Arg("array-sent").Fire();
```

`.Fire()` executes the call. Nothing chains after it — it's the last step.

## Can I pass one plugin's output to another?

Yes. Each plugin read returns a typed source that `.Arg()` accepts:

```csharp
var filtered = pipeline.Plugin<object>("array", "filter")
    .Arg(json, x => x.Items).Arg("status").Arg("active");
var activeCount = pipeline.Plugin<int>("array", "count")
    .Arg(filtered);
s.Element("active-count").SetText(activeCount);
```

This calls `filter(items, "status", "active")`, then passes the result to `count()`.

## Can I use a plugin result in a condition?

Yes — plugin reads are typed sources, so they work with `When()`:

```csharp
var someSrc = pipeline.Plugin<bool>("array", "some")
    .Arg(json, x => x.Items).Arg("status").Arg("critical");
s.When(someSrc).Truthy()
    .Then(then => then.Element("alert").Show())
    .Else(els => els.Element("no-alert").Show());
```

## How do I send a plugin result in a request?

Include it in the gather:

```csharp
.Gather(g => g.Plugin(countSrc, "count"))
```

### Can I use a plugin value as a header?

```csharp
.Gather(g => g.Header("X-Array-Count", countSrc))
```

### Can I combine both?

```csharp
pipeline.Get("/api/echo")
    .Gather(g => g
        .Header("X-Array-Count", countSrc)
        .Plugin(countSrc, "count"))
    .Response(r => r.OnSuccess<EchoResponse>((json, s) =>
    {
        s.Element("echo-count").SetText(json, x => x.ReceivedCount);
        s.Element("echo-header").SetText(json, x => x.ReceivedHeader);
        s.Plugin("analytics", "track").Arg("array-sent").Fire();
    }));
```

## What does a full plugin workflow look like?

Here is a complete pipeline from the ArrayManager sandbox — register plugins, fetch data, compute with plugin methods, branch on results, and send computed values to the server:

```csharp
@{
    var plan = Html.ReactivePlan<ArrayManagerModel>();

    plan.RegisterPlugin("array", p =>
    {
        p.Method<int>("count");
        p.Method<object>("filter");
        p.Method<bool>("some");
    });
    plan.RegisterPlugin("analytics", p => p.Command("track"));

    Html.On(plan, t => t.DomReady(pipeline =>
        pipeline.Get("/api/residents")
            .Response(r => r.OnSuccess<ResidentsResponse>((json, s) =>
            {
                // Count all items
                var count = pipeline.Plugin<int>("array", "count")
                    .Arg(json, x => x.Items);
                s.Element("total").SetText(count);

                // Filter → count active (nested composition)
                var active = pipeline.Plugin<object>("array", "filter")
                    .Arg(json, x => x.Items).Arg("status").Arg("active");
                var activeCount = pipeline.Plugin<int>("array", "count")
                    .Arg(active);
                s.Element("active-count").SetText(activeCount);

                // Branch on critical status
                var hasCritical = pipeline.Plugin<bool>("array", "some")
                    .Arg(json, x => x.Items).Arg("status").Arg("critical");
                s.When(hasCritical).Truthy()
                    .Then(then => then.Element("alert").Show())
                    .Else(els => els.Element("alert").Hide());
            }))));
}
```

This single pipeline handles data fetch, array computation, nested plugin composition, and conditional UI — all described in the plan, executed by the runtime.

**Previous:** [HTTP Pipeline Extensions](../http-pipeline-extensions/) — custom headers, route parameters, URL query parameters, and method arguments.

**Next:** [Validation](../validation/) — client-side validation with FluentValidation extraction.
