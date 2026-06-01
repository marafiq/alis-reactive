# plugins

The plugin escape hatch lets a reactive plan reach a browser object the typed
component DSL does not model — `URL`, `localStorage`, a charting library, native
JS array helpers, a slugifier. A plugin is the *intentional* stringly boundary:
the plugin name and member names may be strings, but every argument and every
read stays typed through `TypedSource<T>` and the shared `ValueExpression` spine.

The flow has three parts:

1. **Declare** the plugin's member contract once — `plan.RegisterPlugin`, either
   inline with `PluginTypeBuilder` or as a reusable `Plugin` subclass exposing
   `PluginFunction`/`PluginCommand`/`PluginProperty` descriptors.
2. **Read** a plugin value anywhere a `TypedSource<T>` is allowed — conditions,
   reaction sets, gather payloads/headers/route-params, dispatch payloads, even
   as an `Arg` to another plugin call (nesting). `p.Plugin<T>(...)` returns a
   `PluginReadBuilder<TReturn,TModel>` that implicitly converts to
   `TypedPluginSource<TReturn>`; `p.PluginProperty<T>(...)` returns a
   `TypedPluginPropertySource<T>`.
3. **Call** a void plugin command for its side effect — `p.Plugin(...)` returns a
   `PluginCallBuilder<TModel>`; add args, then `.Fire()` emits the call.

Three member kinds, named by lane:

| Member kind | Returns | Read/Call lane | Descriptor | Source produced |
|-------------|---------|----------------|------------|-----------------|
| `Function` | a typed value | READ | `PluginFunction<TReturn>` | `TypedPluginSource<TReturn>` |
| `Property` | a typed value | READ | `PluginProperty<TValue>` | `TypedPluginPropertySource<TValue>` |
| `Command` | nothing (void) | CALL → `.Fire()` | `PluginCommand` | (none — side effect) |

`PluginMember` is the abstract supertype of `Function`/`Command`/`Property` —
they are all *members* of the plugin object, so the supertype reads as their
honest union. A member may also be the plugin's **root** call (no member name) —
the whole plugin is invoked as one function/command.

`Function`/`Command`/`Property` share one name per concept: value-op is always
`Function`, void-op is always `Command`, read-op is always `Property`. There are
no synonyms (`Method`/`Void` are retired).

Naming note: the read face is `PluginReadBuilder<TReturn,TModel>` (not
`PluginMemberBuilder`) — `Read` names the load-bearing lane, paired against the
void-CALL face `PluginCallBuilder<TModel>`. `Arg` glosses over literal/source/
payload value spines; `ArgValue<TValue>` is the generic-shaped literal variant.

---

## RegisterPlugin — declare the member contract

A plugin must be registered before any `p.Plugin(...)` reference. Three
registration entry points; pick by how the contract is authored.

### RegisterPlugin (inline PluginTypeBuilder)

Declare the members inline with a configure lambda — no separate class.

```csharp
plan.RegisterPlugin("storage", p => p
    .Function<string>("getItem", a => a
        .Arg<string>())
    .Command("setItem", a => a
        .Arg<string>()
        .Arg<string>())
    .Command("removeItem", a => a
        .Arg<string>()));
```

### RegisterPlugin (typed Plugin instance)

Build a `Plugin` subclass, then hand the instance to `RegisterPlugin`.

```csharp
var slugify = new SlugifyPlugin();
plan.RegisterPlugin(slugify);
```

### RegisterPlugin\<TPlugin\> — construct and register

Construct the `Plugin` subclass and register it in one call; the returned
instance exposes the typed descriptors for `p.Plugin(descriptor)` reads.

```csharp
var array = plan.RegisterPlugin<ArrayPlugin>();
var analytics = plan.RegisterPlugin<AnalyticsPlugin>();
var slugify = plan.RegisterPlugin<SlugifyPlugin>();
```

---

## PluginTypeBuilder — inline member declaration

The fluent declarer passed to `plan.RegisterPlugin("name", p => ...)`. Argument
arity is declared once through the args builder (`a => a.Arg<string>()`), never
through arity-specific overloads. Every call returns the builder for chaining.

### Function\<T\>(name) — value method, open args

A method returning a typed value; the shape is inferred from `T`. With no args
contract, arguments are open.

```csharp
plan.RegisterPlugin("careLevel", p => p
    .Function<string>("describe"));
```

### Function\<TReturn\>(name, arguments) — value method, exact args

Declare the exact JS argument contract with the args builder.

```csharp
plan.RegisterPlugin("billing", p => p
    .Function<decimal>("monthlyRate", a => a
        .Arg<string>()
        .Arg<int>()));
```

### Function\<T\>() — root value function, open args

The plugin object itself is callable as a value function (no member name).

```csharp
plan.RegisterPlugin("slugify", p => p
    .Function<string>());
```

### Function\<TReturn\>(arguments) — root value function, exact args

Root function with an exact argument contract.

```csharp
plan.RegisterPlugin("slugify", p => p
    .Function<string>(a => a
        .Arg<string>()));
```

### Property\<T\>(name) — readable object property

A readable property on the plugin object; shape inferred from `T`. Read via
`p.PluginProperty<T>(...)`.

```csharp
plan.RegisterPlugin("session", p => p
    .Property<string>("facilityId")
    .Property<bool>("isAdmin"));
```

### Command(name) — void method, open args

A side-effecting method that returns nothing. `Command` is the only name for
void-ops (the retired `Void` synonym is gone).

```csharp
plan.RegisterPlugin("analytics", p => p
    .Command("track"));
```

### Command(name, arguments) — void method, exact args

Void method with an exact argument contract.

```csharp
plan.RegisterPlugin("analytics", p => p
    .Command("track", a => a
        .Arg<string>()
        .Arg<int>()));
```

### Command() — root void command, open args

The plugin object itself is callable as a void command.

```csharp
plan.RegisterPlugin("refreshWidgets", p => p
    .Command());
```

### Command(arguments) — root void command, exact args

Root command with an exact argument contract.

```csharp
plan.RegisterPlugin("reloadPage", p => p
    .Command(a => a
        .Arg<bool>()));
```

### Mixed members — properties, functions, and commands together

A single builder declares every member kind. A name may not be declared as both
a property and a function on the same plugin.

```csharp
plan.RegisterPlugin("careAnalytics", p => p
    .Property<string>("facilityId")
    .Property<int>("residentCount")
    .Function<decimal>("averageBilling", a => a
        .Arg<string>())
    .Function<bool>("hasCritical", a => a
        .Arg<object>())
    .Command("track", a => a
        .Arg<string>()));
```

---

## Plugin subclass — reusable typed descriptors

Subclass `Plugin`, name it in the base constructor, and declare members in the
body. Each declaration returns a strongly-typed descriptor (`PluginFunction<T>`,
`PluginCommand`, `PluginProperty<T>`) you expose as a property so reads can pass
the descriptor directly (`p.Plugin(array.Count)`) instead of a string pair.

### Function\<TReturn\>(member) + .Arg\<TArg\>() — typed value member

Inside the body, declare a value function and chain `.Arg<T>()` per JS argument.

```csharp
private sealed class ArrayPlugin : Plugin
{
    public ArrayPlugin()
        : base("array")
    {
        Count = Function<int>("count")
            .Arg<object>();
        Pluck = Function<object>("pluck")
            .Arg<object>()
            .Arg<int>()
            .Arg<string>();
        Filter = Function<object>("filter")
            .Arg<object>()
            .Arg<string>()
            .Arg<string>();
        Sum = Function<int>("sum")
            .Arg<object>()
            .Arg<string>();
        Some = Function<bool>("some")
            .Arg<object>()
            .Arg<string>()
            .Arg<string>();
    }

    public PluginFunction<int> Count { get; }
    public PluginFunction<object> Pluck { get; }
    public PluginFunction<object> Filter { get; }
    public PluginFunction<int> Sum { get; }
    public PluginFunction<bool> Some { get; }
}
```

### Function\<TReturn\>(member, arguments) — value member, exact args block

Declare the whole argument contract in one `.Args(...)` block instead of chained
`.Arg<T>()` calls.

```csharp
private sealed class BillingPlugin : Plugin
{
    public BillingPlugin()
        : base("billing")
    {
        MonthlyRate = Function<decimal>("monthlyRate", a => a
            .Arg<string>()
            .Arg<int>());
    }

    public PluginFunction<decimal> MonthlyRate { get; }
}
```

### Function\<TReturn\>() — root value function

The plugin is invoked as a value function with no member name.

```csharp
private sealed class SlugifyPlugin : Plugin
{
    public SlugifyPlugin()
        : base("slugify")
    {
        Slug = Function<string>()
            .Arg<string>();
    }

    public PluginFunction<string> Slug { get; }
}
```

### Command(member) + .Arg\<TArg\>() — typed void member

Declare a side-effecting command; chain `.Arg<T>()` per JS argument.

```csharp
private sealed class AnalyticsPlugin : Plugin
{
    public AnalyticsPlugin()
        : base("analytics")
    {
        Track = Command("track")
            .Arg<string>();
    }

    public PluginCommand Track { get; }
}
```

### Command(member, arguments) — void member, exact args block

The command's argument contract declared in one `.Args(...)` block.

```csharp
private sealed class AuditPlugin : Plugin
{
    public AuditPlugin()
        : base("audit")
    {
        Record = Command("record", a => a
            .Arg<string>()
            .Arg<string>());
    }

    public PluginCommand Record { get; }
}
```

### Command() — root void command

The plugin is invoked as a void command with no member name.

```csharp
private sealed class RefreshPlugin : Plugin
{
    public RefreshPlugin()
        : base("refreshWidgets")
    {
        Refresh = Command();
    }

    public PluginCommand Refresh { get; }
}
```

### Property\<TValue\>(member) — readable object property

A readable property descriptor; pass to `p.Plugin(session.FacilityId)`.

```csharp
private sealed class SessionPlugin : Plugin
{
    public SessionPlugin()
        : base("session")
    {
        FacilityId = Property<string>("facilityId");
        IsAdmin = Property<bool>("isAdmin");
    }

    public PluginProperty<string> FacilityId { get; }
    public PluginProperty<bool> IsAdmin { get; }
}
```

### .Args(arguments) — append exact arg contract to a descriptor

`.Args(...)` (on `PluginFunction`/`PluginCommand`) appends an exact JS argument
contract without an arity-specific overload — the same as the `(member, args)`
form, available as a chained terminal.

```csharp
private sealed class ReportPlugin : Plugin
{
    public ReportPlugin()
        : base("report")
    {
        Generate = Function<string>("generate")
            .Args(a => a
                .Arg<string>()
                .Arg<DateTime>());
    }

    public PluginFunction<string> Generate { get; }
}
```

---

## PluginReadBuilder — read a plugin value (`p.Plugin<T>` / `.Plugin(descriptor)`)

`p.Plugin<T>(...)` returns `PluginReadBuilder<TReturn,TModel>`. The source *is*
the builder: it implicitly converts to `TypedPluginSource<TReturn>`, so there is
no explicit `Build()` — just assign it to a `TypedPluginSource<T>` or pass it
straight where a source is expected.

### Plugin\<T\>(pluginName, member) — read a named function by strings

The stringly entry: read a plugin function's return value by plugin + member
name. The plugin must be registered first.

```csharp
Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<int> total = p.Plugin<int>("array", "count")
             .Arg(json, x => x.Items);
         s.Element("arr-total").SetText(total);
     }))));
```

### Plugin\<T\>(pluginName) — read the root function by string

Read the plugin's root function (no member name).

```csharp
Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginSource<string> slug = p.Plugin<string>("slugify")
        .Arg("Sunrise Memory Care");
    p.Element("facility-slug").SetText(slug);
}));
```

### Plugin\<T\>(function) — read a declared function descriptor

Pass the typed `PluginFunction<T>` descriptor from a registered `Plugin`
subclass — fully typed, no strings at the call site.

```csharp
var array = plan.RegisterPlugin<ArrayPlugin>();

Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<int> total = p.Plugin(array.Count)
             .Arg(json, x => x.Items);
         s.Element("arr-total").SetText(total);
     }))));
```

### PluginProperty\<T\>(pluginName, member) — read a property by strings

Read a plugin object property by plugin + member name; returns
`TypedPluginPropertySource<T>` directly (no args, no builder).

```csharp
Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginPropertySource<string> facility =
        p.PluginProperty<string>("session", "facilityId");
    p.Element("active-facility").SetText(facility);
}));
```

### Plugin\<T\>(property) — read a declared property descriptor

Pass the typed `PluginProperty<T>` descriptor from a `Plugin` subclass.

```csharp
var session = plan.RegisterPlugin<SessionPlugin>();

Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginPropertySource<bool> isAdmin = p.Plugin(session.IsAdmin);
    p.When(isAdmin).Truthy()
        .Then(then => then
            .Element("admin-panel").Show());
}));
```

---

## PluginReadBuilder.Arg — typed arguments to a plugin read

Every `Arg` lowers to a `ValueExpression` over the shared value spine, and is
contract-checked against the declared argument shape. One `Arg` gloss spans
literals, typed sources, response bodies, and event payloads.

### .Arg\<TArg\>(source) — a typed source argument

Pass any `TypedSource<T>` — a component read, URL param, another plugin read.
This is how plugin reads **nest**: the result of one read feeds the next.

```csharp
Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<object> active = p.Plugin(array.Filter)
             .Arg(json, x => x.Items)
             .Arg("status")
             .Arg("active");

         TypedPluginSource<int> activeCount = p.Plugin(array.Count)
             .Arg(active);

         s.Element("arr-active-count").SetText(activeCount);
     }))));
```

### .Arg\<TResponse, TProp\>(body, path) — a response-body argument

Read a property off the success/error response body and pass it as an argument;
the source carries the response scope.

```csharp
Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<int> sum = p.Plugin(array.Sum)
             .Arg(json, x => x.Items)
             .Arg("age");
         s.Element("arr-total-age").SetText(sum);
     }))));
```

### .Arg\<TArgs, TProp\>(args, path) — an event-payload argument

Read a property off the event args and pass it as an argument.

```csharp
Html.SearchBox(plan, m => m.Query)
    .Reactive(plan, evt => evt.Input, (args, p) =>
    {
        TypedPluginSource<string> slug = p.Plugin(slugify.Slug)
            .Arg(args, e => e.Value);
        p.Element("query-slug").SetText(slug);
    });
```

### .Arg(string) — a string literal argument

A literal string argument.

```csharp
TypedPluginSource<string> slug = p.Plugin(slugify.Slug)
    .Arg("Sunrise Memory Care");
```

### .Arg(int) — an int literal argument

```csharp
TypedPluginSource<object> first = p.Plugin(array.Pluck)
    .Arg(json, x => x.Items)
    .Arg(0)
    .Arg("name");
```

### .Arg(bool) — a bool literal argument

```csharp
TypedPluginSource<string> report = p.Plugin(report.Generate)
    .Arg("billing")
    .Arg(true);
```

### .Arg(long) — a long literal argument

```csharp
TypedPluginSource<string> token = p.Plugin<string>("auth", "signFor")
    .Arg(9007199254740991L);
```

### .Arg(decimal) — a decimal literal argument

```csharp
TypedPluginSource<string> formatted = p.Plugin<string>("currency", "format")
    .Arg(2400.50m);
```

### .Arg(double) — a double literal argument

```csharp
TypedPluginSource<string> pct = p.Plugin<string>("format", "percent")
    .Arg(0.875);
```

### .Arg(DateTime) — a DateTime literal argument

A `DateTime` literal, formatted for browser date comparison.

```csharp
TypedPluginSource<string> report = p.Plugin(report.Generate)
    .Arg("monthly")
    .Arg(new DateTime(2026, 1, 1));
```

### .ArgValue\<TValue\>(value) — a generic-shaped literal argument

The generic literal variant: the plan shape is derived from `TValue`. Use when
the literal type is a type parameter, not one of the concrete overloads.

```csharp
TypedPluginSource<string> code = p.Plugin<string>("careLevel", "encode")
    .ArgValue(CareLevel.MemoryCare);
```

---

## PluginCallBuilder.Fire — call a void plugin command

`p.Plugin(...)` (no `<T>`) returns `PluginCallBuilder<TModel>` for void commands.
It shares the same `Arg`/`ArgValue` surface as the read face. `.Fire()` is the
terminal — it emits the call reaction into the pipeline. Nothing happens until
`.Fire()`.

### Plugin(pluginName, member).Fire() — call a named command by strings

```csharp
Html.NativeButton("track-btn", "Track")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Plugin("analytics", "track")
            .Arg("array-sent")
            .Fire();
    });
```

### Plugin(pluginName).Fire() — call the root command by string

```csharp
Html.NativeButton("reload-btn", "Reload")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Plugin("reloadPage")
            .Arg(true)
            .Fire();
    });
```

### Plugin(command).Fire() — call a declared command descriptor

Pass the typed `PluginCommand` descriptor from a `Plugin` subclass.

```csharp
var analytics = plan.RegisterPlugin<AnalyticsPlugin>();

Html.NativeButton("filter-btn", "Filter")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Plugin(analytics.Track)
            .Arg("filter-inspect")
            .Fire();
    });
```

### Command call inside a success scope

A void command fired after an HTTP success — the side effect runs once the
response lands.

```csharp
Html.NativeButton("send-btn", "Send")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Get("/Sandbox/Plugins/Echo")
         .Response(r => r.OnSuccess<EchoResponse>((json, s) =>
         {
             s.Element("echo-result").SetText(json, x => x.Received);
             s.Plugin(analytics.Track)
                 .Arg("array-sent")
                 .Fire();
         }));
    });
```

---

## Plugin source in conditions

A `TypedPluginSource<T>` / `TypedPluginPropertySource<T>` is a first-class
condition source — `When` / `ElseIf` accept it like any other typed source, and
every operator applies.

### When(plugin function) — branch on a plugin function result

```csharp
Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<bool> hasCritical = p.Plugin(array.Some)
             .Arg(json, x => x.Items)
             .Arg("status")
             .Arg("critical");

         s.When(hasCritical).Truthy()
             .Then(then => then
                 .Element("arr-has-critical").Show())
             .Else(els => els
                 .Element("arr-no-critical").Show());
     }))));
```

### When(plugin function).Gt — compare a plugin result to a literal

```csharp
TypedPluginSource<int> activeCount = p.Plugin(array.Count)
    .Arg(active);

s.When(activeCount).Gt(10)
    .Then(then => then
        .Element("capacity-warning").Show());
```

### When(plugin property) — branch on a plugin property

```csharp
var session = plan.RegisterPlugin<SessionPlugin>();

Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginPropertySource<bool> isAdmin = p.Plugin(session.IsAdmin);

    p.When(isAdmin).Truthy()
        .Then(then => then
            .Element("admin-panel").Show())
        .Else(els => els
            .Element("admin-panel").Hide());
}));
```

### ElseIf(plugin function) — chained plugin branches

```csharp
TypedPluginSource<int> count = p.Plugin(array.Count)
    .Arg(json, x => x.Items);

s.When(count).Eq(0)
    .Then(then => then
        .Element("empty-state").Show())
    .ElseIf(count).Gt(50)
        .Then(then => then
            .Element("overflow-state").Show())
    .Else(els => els
        .Element("normal-state").Show());
```

---

## Plugin source in gather

`Gather.Plugin<T>(source, paramName)` includes a plugin read's value in an HTTP
request payload under `paramName`. The plugin source may carry its own args.

### Gather.Plugin — plugin result into the request payload

```csharp
Html.NativeButton("send-btn", "Send")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        TypedPluginSource<int> count = p.Plugin(array.Count)
            .Arg("items");

        p.Get("/Sandbox/Plugins/PluginEcho")
         .Gather(g => g
             .Plugin(count, "count"))
         .Response(r => r.OnSuccess<PluginEchoResponse>((json, s) =>
         {
             s.Element("echo-count").SetText(json, x => x.ReceivedCount);
         }));
    });
```

### Gather.Header — plugin result into a request header

The same `TypedPluginSource<T>` flows into a header value (header is a value
target reading through the shared spine).

```csharp
Html.NativeButton("send-btn", "Send")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        TypedPluginSource<int> count = p.Plugin(array.Count)
            .Arg("items");

        p.Get("/Sandbox/Plugins/PluginEcho")
         .Gather(g => g
             .Header("X-Array-Count", count)
             .Plugin(count, "count"))
         .Response(r => r.OnSuccess<PluginEchoResponse>((json, s) =>
         {
             s.Element("echo-header").SetText(json, x => x.ReceivedHeader);
         }));
    });
```

---

## Plugin source in reactions

A plugin read is a `TypedSource<T>` everywhere a reaction takes a source —
`SetText`, `Set`, and `DispatchFrom`.

### SetText(plugin function) — write a plugin result into the DOM

```csharp
Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginSource<string> slug = p.Plugin(slugify.Slug)
        .Arg("John Doe");
    p.Element("resident-slug").SetText(slug);
}));
```

### Set(model, plugin function) — write a plugin result into a model field

```csharp
Html.On(plan, t => t.PageLoad(p =>
{
    TypedPluginSource<decimal> rate = p.Plugin(billing.MonthlyRate)
        .Arg("MemoryCare")
        .Arg(2);
    p.Set(m => m.EstimatedMonthly, rate);
}));
```

### DispatchFrom(name, plugin function) — dispatch a plugin-derived payload

```csharp
Html.On(plan, t => t.PageLoad(p =>
    p.Get("/Sandbox/Plugins/Residents")
     .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
     {
         TypedPluginSource<int> count = p.Plugin(array.Count)
             .Arg(json, x => x.Items);

         s.DispatchFrom<ResidentCountPayload>("residents-counted", b => b
             .From(d => d.Total, count));
     }))));
```

---

## Full vertical slice — declare, read, branch, gather, command

One end-to-end slice: a typed `Plugin` subclass registered with the plan, then
exercised across read, nested read, condition, gather, header, and a void
command fired after success.

```csharp
@functions {
    private sealed class ArrayPlugin : Plugin
    {
        public ArrayPlugin()
            : base("array")
        {
            Count = Function<int>("count")
                .Arg<object>();
            Filter = Function<object>("filter")
                .Arg<object>()
                .Arg<string>()
                .Arg<string>();
            Sum = Function<int>("sum")
                .Arg<object>()
                .Arg<string>();
            Some = Function<bool>("some")
                .Arg<object>()
                .Arg<string>()
                .Arg<string>();
        }

        public PluginFunction<int> Count { get; }
        public PluginFunction<object> Filter { get; }
        public PluginFunction<int> Sum { get; }
        public PluginFunction<bool> Some { get; }
    }

    private sealed class AnalyticsPlugin : Plugin
    {
        public AnalyticsPlugin()
            : base("analytics")
        {
            Track = Command("track")
                .Arg<string>();
        }

        public PluginCommand Track { get; }
    }
}

@{
    var array = plan.RegisterPlugin<ArrayPlugin>();
    var analytics = plan.RegisterPlugin<AnalyticsPlugin>();

    Html.NativeButton("inspect-btn", "Inspect Residents")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Get("/Sandbox/Plugins/Residents")
             .Gather(g => g
                 .Header("X-Facility", p.Plugin<string>("session", "facilityId")))
             .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
             {
                 TypedPluginSource<object> active = p.Plugin(array.Filter)
                     .Arg(json, x => x.Items)
                     .Arg("status")
                     .Arg("active");

                 TypedPluginSource<int> activeCount = p.Plugin(array.Count)
                     .Arg(active);
                 s.Element("active-count").SetText(activeCount);

                 TypedPluginSource<int> ageSum = p.Plugin(array.Sum)
                     .Arg(active)
                     .Arg("age");
                 s.Element("active-age-sum").SetText(ageSum);

                 TypedPluginSource<bool> hasCritical = p.Plugin(array.Some)
                     .Arg(json, x => x.Items)
                     .Arg("status")
                     .Arg("critical");

                 s.When(hasCritical).Truthy()
                     .Then(then => then
                         .Element("critical-banner").Show())
                     .Else(els => els
                         .Element("critical-banner").Hide());

                 s.Plugin(analytics.Track)
                     .Arg("residents-inspected")
                     .Fire();
             }));
        });
}
```
