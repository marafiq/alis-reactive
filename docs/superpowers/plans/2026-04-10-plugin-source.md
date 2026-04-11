# Plugin Source

**Goal:** User-defined JS objects as value sources and call targets. One DSL name: `Plugin`. `<T>` for reads, no `<T>` for void. Methods only. Enables native JS operations (array, math, string, date) that would be impossible to express declaratively in the plan model, plus domain-specific plugins (auth, permissions, business rules).

**Prerequisite:** ReadProducer.args (`cfdefa66`), URL Query Source (3-kind Source union).

---

## Registration

Plugins are TS modules bundled separately. Push-array, load before framework:

```typescript
// plugins/array.ts
((window as any).__alisPlugins ??= []).push({
  name: "array",
  instance: {
    count:    (arr: any[]) => arr.length,
    first:    (arr: any[]) => arr[0],
    last:     (arr: any[]) => arr[arr.length - 1],
    includes: (arr: any[], item: any) => arr.includes(item),
    find:     (arr: any[], key: string, val: any) => arr.find(i => i[key] === val),
    filter:   (arr: any[], key: string, val: any) => arr.filter(i => i[key] === val),
    sum:      (arr: any[], key: string) => arr.reduce((s, i) => s + (Number(i[key]) || 0), 0),
    sort:     (arr: any[], key: string) => [...arr].sort((a, b) => a[key] > b[key] ? 1 : -1),
    some:     (arr: any[], key: string, val: any) => arr.some(i => i[key] === val),
    every:    (arr: any[], key: string, val: any) => arr.every(i => i[key] === val),
    join:     (arr: any[], sep: string) => arr.join(sep),
    map:      (arr: any[], key: string) => arr.map(i => i[key]),
  }
});
```

```html
<script type="module" src="~/js/plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Framework drains at top of `root.ts` before boot. 5 lines. Zero timing change.

---

## DSL

One name. `<T>` = read. No `<T>` = void.

```csharp
// Read — zero args
p.Plugin<int>("array", "count")

// Read — with typed source args
p.Plugin<int>("array", "count").Arg(json, x => x.Items)

// Read — with literal args
p.Plugin<int>("array", "count")
 .Arg(p.Plugin<object>("array", "filter")
      .Arg(json, x => x.Items)
      .Arg("status")
      .Arg("active"))

// Void — zero args
p.Plugin("logger", "flush")

// Void — with args
p.Plugin("analytics", "track").Arg("pageView")
```

### Builder .Arg() Overloads

```csharp
// Typed source — component read, URL param, another plugin read
.Arg<TArg>(TypedSource<TArg> source)

// Response body — from OnSuccess<T>/OnError<T> handler (carries scope)
.Arg<TResponse, TProp>(ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)

// Event arg — from event handler args
.Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path)

// Literals
.Arg(string value)
.Arg(int value)
.Arg(bool value)
```

Response body overload accepts `ResponseBody<T>` (not raw type) — carries the correct payload scope (success/error). Matches `ElementBuilder.SetText` pattern (ElementBuilder.cs:66-74). All overloads create `ValueProducer` internally. No internal types leak.

### In Every Context

```csharp
// ── DomReady: read → SetText + Condition ────────────────
Html.On(plan, t => t.DomReady(p =>
{
    p.Element("theme").SetText(p.Plugin<string>("prefs", "getTheme"));

    p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
     .Then(t => t.Element("admin-panel").Show());
}));

// ── .Reactive: HTTP with plugin gather + header ─────────
Html.NativeButton("save-btn", "Save")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/api/save")
         .Gather(g => g
             .Header("Authorization", p.Plugin<string>("auth", "getToken"))
             .Plugin<string>("auth", "getSessionId", "session")
             .Include<FusionTextBox, Model>(m => m.Name))
         .Response(r => r.OnSuccess(s =>
         {
             s.Element("status").SetText("Saved");
             s.Plugin("analytics", "track").Arg("save-success");
         }));
    });

// ── Route param from plugin ─────────────────────────────
g.RouteParam("tenantId", p.Plugin<int>("auth", "getTenantId"))

// ── Array operations on HTTP response ───────────────────
p.Get("/api/residents")
 .Response(r => r.OnSuccess<ResidentsResponse>((json, s) =>
 {
     // count
     s.Element("total").SetText(
         p.Plugin<int>("array", "count").Arg(json, x => x.Items));

     // filter + count (nested plugin reads)
     s.Element("active-count").SetText(
         p.Plugin<int>("array", "count")
          .Arg(p.Plugin<object>("array", "filter")
               .Arg(json, x => x.Items)
               .Arg("status")
               .Arg("active")));

     // some → condition
     s.When(p.Plugin<bool>("array", "some")
            .Arg(json, x => x.Items)
            .Arg("status")
            .Arg("critical"))
      .Truthy()
      .Then(t => t.Element("alert").Show());

     // sum
     s.Element("total-age").SetText(
         p.Plugin<int>("array", "sum")
          .Arg(json, x => x.Items)
          .Arg("age"));
 }));
```

### Plan Model Mapping

| DSL | Plan Primitive | Existing? |
|---|---|---|
| `p.Plugin<T>(name, member)` | ReadProducer | ✓ |
| `p.Plugin<T>(name, member).Arg(...)` | ReadProducer + args | ✓ (cfdefa66) |
| `p.Plugin(name, member)` | CallReaction | ✓ |
| `p.Plugin(name, member).Arg(...)` | CallReaction + args | ✓ |

Zero new plan model kinds. Zero new ValueProducer kinds.

---

## Implementation

### Task 1: PluginSource

**File:** `Source.cs` — after UrlSource:

```csharp
public sealed class PluginSource : Source
{
    public string Kind => "plugin";
    public string Name { get; }
    private PluginSource(string name)
    {
        Name = name ?? throw new System.ArgumentNullException(nameof(name));
    }
    internal static PluginSource Of(string name) => new PluginSource(name);
}
```

Polymorphic serialization via `WriteOnlyPolymorphicConverter<Source>` — zero changes.

### Task 2: PlanBuildContext.EnsurePluginMethod

**File:** `PlanBuildContext.cs`:

```csharp
internal void EnsurePluginMethod(string pluginName, string member, Shape returns = null)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(member))
        throw new System.ArgumentException("Method name required.", nameof(member));
    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.ContainsKey(typeKey))
        _plan.MutableTypes[typeKey] = new JsType();
    _plan.MutableTypes[typeKey].WithMethod(member, Path.Parse(member), returns: returns);
}
```

Methods only. Auto-creates JsType on first use.

### Task 3: TypedPluginSource<T>

**File:** `Builders/Conditions/TypedPluginSource.cs`:

```csharp
public sealed class TypedPluginSource<TProp> : TypedSource<TProp>
{
    private readonly string _pluginName;
    private readonly string _member;
    private readonly List<ValueProducer> _args;
    internal TypedPluginSource(string pluginName, string member, List<ValueProducer> args = null)
    {
        _pluginName = pluginName;
        _member = member;
        _args = args;
    }
    internal override ValueProducer ToValueProducer() =>
        ValueProducer.Read(PluginSource.Of(_pluginName), _member, shape: Shape, args: _args);
}
```

### Task 4: PluginReadBuilder<T>

**File:** `Builders/PluginReadBuilder.cs`:

```csharp
public sealed class PluginReadBuilder<TReturn, TModel> where TModel : class
{
    private readonly string _pluginName;
    private readonly string _member;
    private readonly List<ValueProducer> _args = new List<ValueProducer>();

    internal PluginReadBuilder(string pluginName, string member)
    { _pluginName = pluginName; _member = member; }

    // Typed source arg
    public PluginReadBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
    { _args.Add(source.ToValueProducer()); return this; }

    // Response body — carries scope from OnSuccess/OnError handler
    public PluginReadBuilder<TReturn, TModel> Arg<TResponse, TProp>(
        ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
        where TResponse : class
    {
        var responsePath = ExpressionPathHelper.ToResponsePath(path);
        var shape = Shape.FromClrType(typeof(TProp));
        _args.Add(ValueProducer.Read(body.Scope, responsePath, shape: shape));
        return this;
    }

    // Event arg — uses PayloadSource.Event()
    public PluginReadBuilder<TReturn, TModel> Arg<TArgs, TProp>(
        TArgs args, Expression<Func<TArgs, TProp>> path)
    {
        var eventPath = ExpressionPathHelper.ToEventPath(path);
        var shape = Shape.FromClrType(typeof(TProp));
        _args.Add(ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape));
        return this;
    }

    // Literal overloads
    public PluginReadBuilder<TReturn, TModel> Arg(string value)
    { _args.Add(ValueProducer.Literal(value)); return this; }
    public PluginReadBuilder<TReturn, TModel> Arg(int value)
    { _args.Add(ValueProducer.Literal(value)); return this; }
    public PluginReadBuilder<TReturn, TModel> Arg(bool value)
    { _args.Add(ValueProducer.Literal(value)); return this; }
    public PluginReadBuilder<TReturn, TModel> Arg(long value)
    { _args.Add(ValueProducer.Literal(value)); return this; }

    // Implicit conversion — no .Build()
    public static implicit operator TypedPluginSource<TReturn>(
        PluginReadBuilder<TReturn, TModel> b) =>
        new TypedPluginSource<TReturn>(b._pluginName, b._member, b._args);
}
```

### Task 5: PluginCallBuilder

**File:** `Builders/PluginCallBuilder.cs`:

Same `.Arg()` overloads as PluginReadBuilder. Emits `CallReaction` via `IReactionEmitter.AddStep()`. The void `Plugin(name, member)` method on PipelineBuilder returns the builder. For zero-arg calls, the builder is created AND the reaction is emitted when the next pipeline statement starts (tracked by PipelineBuilder).

### Task 6: PipelineBuilder.Plugin overloads

**File:** `PipelineBuilder.cs`:

```csharp
public PluginReadBuilder<T, TModel> Plugin<T>(string pluginName, string member)
{
    if (string.IsNullOrWhiteSpace(pluginName)) throw new ArgumentException("Plugin name required.");
    if (string.IsNullOrWhiteSpace(member)) throw new ArgumentException("Member name required.");
    Context.EnsurePluginMethod(pluginName, member, returns: Shape.FromClrType(typeof(T)));
    return new PluginReadBuilder<T, TModel>(pluginName, member);
}

public PluginCallBuilder<TModel> Plugin(string pluginName, string member)
{
    if (string.IsNullOrWhiteSpace(pluginName)) throw new ArgumentException("Plugin name required.");
    if (string.IsNullOrWhiteSpace(member)) throw new ArgumentException("Member name required.");
    Context.EnsurePluginMethod(pluginName, member);
    return new PluginCallBuilder<TModel>(pluginName, member, this);
}
```

### Task 7: GatherBuilder.Plugin<T>

**File:** `GatherBuilder.cs`:

```csharp
public GatherBuilder<TModel> Plugin<T>(string pluginName, string member, string paramName)
{
    if (string.IsNullOrWhiteSpace(pluginName)) throw new ArgumentException("Plugin name required.");
    if (string.IsNullOrWhiteSpace(member)) throw new ArgumentException("Member name required.");
    if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentException("HTTP param name required.");
    var shape = Shape.FromClrType(typeof(T));
    _context.EnsurePluginMethod(pluginName, member, returns: shape);
    Fields.Add(GatherField.Of(paramName, ValueProducer.Read(PluginSource.Of(pluginName), member, shape: shape)));
    return this;
}
```

Generic `<T>`. Shape carried. No `Shape.Any`.

### Task 8: Schema + TS Types

PluginSource in Source oneOf. `additionalProperties: false`, name pattern `^\S+$`. TS `PluginSource { kind: "plugin"; name: string; }`.

### Task 9: TS — plugin-registry.ts

`registerPlugin(name, instance)` with validation (non-empty, non-null, object, no duplicates). `resolvePlugin(name)` with fail-fast.

### Task 10: TS — resolver.ts + evaluate.ts + execute.ts

- resolver: `case "plugin": resolvePlugin(source.name)` + `getJsTypeForSource` for "plugin"
- evaluate: widen component branch to include "plugin", fix error message
- execute: Call allows plugin, Set rejects, fix trace target

### Task 11: TS — root.ts drain

5 lines at top: drain `window.__alisPlugins`, delete after.

### Task 12: Vertical Slice — `/Sandbox/Plugins/ArrayManager`

**OWN page.** Own controller, own view, own model, own DTOs, own index entry. Not shared.

#### Plugin TS (already in sandbox-plugins.ts)

Methods return scalars OR objects/arrays — objects flow as payloads (same as event args and HTTP responses). The framework's `evaluateValue` handles recursive evaluation:

```typescript
// In sandbox-plugins.ts — array utility methods
// Scalars — for display and conditions
count:    (arr: any[]) => arr.length,
pluck:    (arr: any[], index: number, key: string) => arr[index]?.[key],
sum:      (arr: any[], key: string) => arr.reduce((s, i) => s + (Number(i[key]) || 0), 0),
some:     (arr: any[], key: string, val: any) => arr.some(i => i[key] === val),
every:    (arr: any[], key: string, val: any) => arr.every(i => i[key] === val),
includes: (arr: any[], item: any) => arr.includes(item),
join:     (arr: any[], sep: string) => arr.join(sep),

// Objects/arrays — for chaining between plugin calls
first:    (arr: any[]) => arr[0],
filter:   (arr: any[], key: string, val: any) => arr.filter(i => i[key] === val),
sort:     (arr: any[], key: string) => [...arr].sort((a, b) => a[key] > b[key] ? 1 : -1),
map:      (arr: any[], key: string) => arr.map(i => i[key]),
```

#### Model + DTOs

**File:** `Areas/Sandbox/Models/Plugins/ArrayManagerModel.cs`:

```csharp
public class ArrayManagerModel { }

public class ResidentsListResponse
{
    public object[] Items { get; set; } = Array.Empty<object>();
}

public class PluginEchoResponse
{
    public int? ReceivedCount { get; set; }
    public string? ReceivedHeader { get; set; }
}
```

#### Controller

**File:** `Areas/Sandbox/Controllers/Plugins/ArrayManagerController.cs`:

```csharp
[Area("Sandbox")]
[Route("Sandbox/Plugins/ArrayManager")]
public class ArrayManagerController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml", new ArrayManagerModel());

    [HttpGet("Residents")]
    public IActionResult Residents() => Json(new {
        items = new[] {
            new { id = 1, name = "John Doe", status = "active", age = 82 },
            new { id = 2, name = "Jane Smith", status = "active", age = 75 },
            new { id = 3, name = "Bob Johnson", status = "discharged", age = 68 },
            new { id = 4, name = "Alice Brown", status = "active", age = 91 },
            new { id = 5, name = "Charlie Wilson", status = "pending", age = 77 },
        }
    });

    [HttpGet("PluginEcho")]
    public IActionResult PluginEcho(int? count) => Json(new {
        receivedCount = count,
        receivedHeader = Request.Headers["X-Array-Count"].FirstOrDefault() ?? "(none)"
    });
}
```

#### View

**File:** `Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml`:

```html
@model ArrayManagerModel
@using Alis.Reactive
@using Alis.Reactive.Native.Extensions
@using Alis.Reactive.Native.Components
@{
    ViewData["Title"] = "Array Plugin";
    var plan = Html.ReactivePlan<ArrayManagerModel>();

    // DomReady → GET residents → use array plugin on response
    Html.On(plan, t => t.DomReady(p =>
        p.Get("/Sandbox/Plugins/ArrayManager/Residents")
         .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
         {
             // array.count → scalar int
             s.Element("arr-total").SetText(
                 p.Plugin<int>("array", "count").Arg(json, x => x.Items));

             // array.pluck(items, 0, "name") → scalar string
             s.Element("arr-first-name").SetText(
                 p.Plugin<string>("array", "pluck")
                  .Arg(json, x => x.Items)
                  .Arg(0)
                  .Arg("name"));

             // array.filter + count (NESTED plugin reads)
             s.Element("arr-active-count").SetText(
                 p.Plugin<int>("array", "count")
                  .Arg(p.Plugin<object>("array", "filter")
                       .Arg(json, x => x.Items)
                       .Arg("status")
                       .Arg("active")));

             // array.sum → scalar int
             s.Element("arr-total-age").SetText(
                 p.Plugin<int>("array", "sum")
                  .Arg(json, x => x.Items)
                  .Arg("age"));

             // array.some → condition
             s.When(p.Plugin<bool>("array", "some")
                    .Arg(json, x => x.Items)
                    .Arg("status")
                    .Arg("critical"))
              .Truthy()
              .Then(t => t.Element("arr-has-critical").Show())
              .Else(e => e.Element("arr-no-critical").Show());

             s.Element("arr-results").AddClass("text-green-600");
         }))));

    // Button → GET with plugin values in gather + header
    Html.NativeButton("arr-send-btn", "Send to Server")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Get("/Sandbox/Plugins/ArrayManager/PluginEcho")
             .Gather(g => g
                 .Header("X-Array-Count", p.Plugin<int>("array", "count"))
                 .Plugin<int>("array", "count", "count"))
             .Response(r => r
                .OnSuccess<PluginEchoResponse>((json, s) =>
                {
                    s.Element("arr-echo-count").SetText(json, x => x.ReceivedCount);
                    s.Element("arr-echo-header").SetText(json, x => x.ReceivedHeader);
                    s.Element("arr-echo-result").AddClass("text-green-600");
                    // Case 1: void call after HTTP success
                    s.Plugin("analytics", "track").Arg("array-sent");
                }));
        });
}

<native-vstack gap="Lg">
    <div>
        <native-heading level="H1">Array Plugin</native-heading>
        <native-text color="Secondary">
            Native JS array methods exposed to the DSL via plugin.
        </native-text>
    </div>

    <native-card>
    <native-card-body>
        <native-heading level="H2">DomReady: Array Operations on Server Data</native-heading>
        <div id="arr-results" class="space-y-2 font-mono text-sm text-text-muted">
            <p>Total: <span id="arr-total">—</span></p>
            <p>First Name: <span id="arr-first-name">—</span></p>
            <p>Active Count: <span id="arr-active-count">—</span></p>
            <p>Total Age: <span id="arr-total-age">—</span></p>
            <p id="arr-has-critical" hidden class="text-red-600">Critical residents found!</p>
            <p id="arr-no-critical" hidden class="text-green-600">No critical residents ✓</p>
        </div>
    </native-card-body>
    </native-card>

    <native-card>
    <native-card-body>
        <native-heading level="H2">HTTP: Plugin Values in Gather + Header</native-heading>
        @(Html.NativeButton("arr-send-btn", "Send to Server")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
        <div id="arr-echo-result" class="mt-3 space-y-2 font-mono text-sm text-text-muted">
            <p>Count: <span id="arr-echo-count">—</span></p>
            <p>Header: <span id="arr-echo-header">—</span></p>
        </div>
    </native-card-body>
    </native-card>
</native-vstack>

@Html.RenderPlan(plan)
```

#### Element Expectations

| Element | Expected Value | What It Proves |
|---|---|---|
| `#arr-total` | "5" | `array.count` — zero-arg on response array |
| `#arr-first-name` | "John Doe" | `array.pluck(items, 0, "name")` — index + key args |
| `#arr-active-count` | "3" | Nested: `array.count(array.filter(items, "status", "active"))` |
| `#arr-total-age` | "393" | `array.sum` — with literal key arg "age" |
| `#arr-no-critical` | visible | `array.some` → condition → Else branch (no critical status) |
| `#arr-results` | class `text-green-600` | DomReady success |
| `#arr-echo-count` | "5" | Plugin read → HTTP gather param |
| `#arr-echo-header` | "5" | Plugin read → HTTP header |
| `#arr-echo-result` | class `text-green-600` | HTTP success |

#### Sandbox Index Update

Add link to `/Sandbox/Plugins/ArrayManager` in `Areas/Sandbox/Views/Index.cshtml`.

---

## Tests

### C# Unit Tests (18) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `plugin_read_produces_plugin_source` | from: { kind: "plugin" } + AssertSchemaValid |
| `plugin_string_carries_shape` | shape: { kind: "string" } |
| `plugin_int_carries_shape` | shape: { kind: "number" } |
| `plugin_bool_carries_shape` | shape: { kind: "boolean" } |
| `plugin_in_condition` | CompareCondition with plugin read |
| `plugin_in_set_text` | SetReaction with plugin read |
| `plugin_in_header` | headers value is plugin read |
| `plugin_in_route_param` | route param is plugin read |
| `plugin_read_with_typed_source_arg` | ReadProducer.args from TypedSource |
| `plugin_read_with_literal_string_arg` | ReadProducer.args with literal "status" |
| `plugin_read_with_literal_int_arg` | ReadProducer.args with literal 42 |
| `plugin_void_call` | CallReaction with PluginSource |
| `plugin_void_call_with_literal_arg` | CallReaction.args with literal |
| `plugin_gather_carries_shape` | GatherField shape from `<T>` |
| `plugin_auto_registers_jstype` | plan.types["plugin.array"] created |
| `plan_without_plugins_clean` | no "plugin" in JSON |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_throws` | ArgumentException |

### Playwright Tests (8) — `WhenArrayPluginManipulates.cs`

**File:** `tests/Alis.Reactive.PlaywrightTests/Plugins/WhenArrayPluginManipulates.cs`

Navigate to `/Sandbox/Plugins/ArrayManager`. Wait for `#arr-total` to not be "—" (DomReady GET completes).

| Test | Action | Assert |
|---|---|---|
| `array_count_on_load` | DomReady | `#arr-total` → "5" |
| `array_pluck_first_name` | DomReady | `#arr-first-name` → "John Doe" |
| `array_nested_filter_count` | DomReady | `#arr-active-count` → "3" |
| `array_sum_total_age` | DomReady | `#arr-total-age` → "393" |
| `array_some_condition_no_critical` | DomReady | `#arr-no-critical` visible, `#arr-has-critical` hidden |
| `array_results_success_class` | DomReady | `#arr-results` has class `text-green-600` |
| `plugin_gather_sends_count` | Click "Send to Server" | `#arr-echo-count` → "5" |
| `plugin_header_sends_count` | Click "Send to Server" | `#arr-echo-header` → "5" |

### vitest Tests (8) — `plugin-registry.test.ts` + `evaluate-plugin.test.ts`

| Test | What It Proves |
|---|---|
| `registerPlugin round-trip` | store + resolve |
| `duplicate throws` | Error |
| `null instance throws` | Error |
| `whitespace name throws` | Error |
| `method zero-arg read` | evaluateValue → callMethod(root, method, []) |
| `method with args` | evaluateValue → callMethod(root, method, [evaluated args]) |
| `missing member throws` | Error from JsType lookup |
| `getJsTypeForSource finds plugin` | plan.types["plugin.name"] |

---

## Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — both bundles
- [ ] Schema validates PluginSource
- [ ] JsType auto-created (methods only)
- [ ] `p.Plugin<T>()` read in conditions, SetText, headers, gather, route params
- [ ] `p.Plugin<T>().Arg(source)` typed source args
- [ ] `p.Plugin<T>().Arg("literal")` literal args
- [ ] `p.Plugin<T>().Arg(json, x => x.Items)` response body args
- [ ] Nested plugin reads: `Plugin<int>("array","count").Arg(Plugin<object>("array","filter").Arg(...))`
- [ ] `p.Plugin()` void call
- [ ] `p.Plugin().Arg()` void with args
- [ ] `g.Plugin<T>()` gather with shape
- [ ] Plugin + URL source in same request
- [ ] Plugin + header in same request
- [ ] Plugin + route param composition
- [ ] execute.ts Set rejects, Call allows
- [ ] Shape/ValueProducer stay internal
- [ ] No inline JS
- [ ] Vertical slice on own page (`/Sandbox/Plugins/ArrayManager`)
- [ ] Sandbox index updated
- [ ] All 18 C# tests pass
- [ ] All 8 vitest tests pass
- [ ] All 8 Playwright tests pass
- [ ] All existing 808+ Playwright pass
