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
// Typed source — any TypedSource<T> (component read, URL param, plugin read)
.Arg<TArg>(TypedSource<TArg> source)

// Event arg — expression path on event payload
.Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path)

// Response body — expression path on HTTP response
.Arg<TResponse, TProp>(TResponse json, Expression<Func<TResponse, TProp>> path)

// Literals — string, int, bool, long, decimal
.Arg(string value)
.Arg(int value)
.Arg(bool value)
```

All overloads create `ValueProducer` internally. No internal types leak.

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

    // Event/response arg via expression
    public PluginReadBuilder<TReturn, TModel> Arg<TPayload, TProp>(
        TPayload payload, Expression<Func<TPayload, TProp>> path)
    {
        var eventPath = ExpressionPathHelper.ToEventPath(path);
        var shape = Shape.FromClrType(typeof(TProp));
        // Determine scope from context — event args use "event", response uses "success"
        // This follows the existing pattern in ElementBuilder.SetText<T>(json, x => x.Prop)
        _args.Add(ValueProducer.Read(PayloadSource.Success(), eventPath, shape: shape));
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

**OWN page.** Not shared with HTTP pipeline. Own controller, own view, own index entry.

**Controller:** `Areas/Sandbox/Controllers/Plugins/ArrayManagerController.cs`

```csharp
[Area("Sandbox")]
[Route("Sandbox/Plugins/ArrayManager")]
public class ArrayManagerController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("~/Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml", new ArrayManagerModel());

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
    public IActionResult PluginEcho(int? count, string? firstName, string? status) => Json(new {
        receivedCount = count,
        receivedFirstName = firstName ?? "(none)",
        receivedStatus = status ?? "(none)",
        receivedHeader = Request.Headers["X-Array-Count"].FirstOrDefault() ?? "(none)"
    });
}
```

**View:** DomReady loads residents via GET, then uses array plugin on the response:

```csharp
@{
    var plan = Html.ReactivePlan<ArrayManagerModel>();

    // Load residents, then use array plugin on the response data
    Html.On(plan, t => t.DomReady(p =>
        p.Get("/Sandbox/Plugins/ArrayManager/Residents")
         .Response(r => r.OnSuccess<ResidentsListResponse>((json, s) =>
         {
             // array.count
             s.Element("total").SetText(
                 p.Plugin<int>("array", "count").Arg(json, x => x.Items));

             // array.first
             s.Element("first-name").SetText(
                 p.Plugin<string>("array", "first").Arg(json, x => x.Items));

             // array.filter + count (nested)
             s.Element("active-count").SetText(
                 p.Plugin<int>("array", "count")
                  .Arg(p.Plugin<object>("array", "filter")
                       .Arg(json, x => x.Items)
                       .Arg("status")
                       .Arg("active")));

             // array.sum
             s.Element("total-age").SetText(
                 p.Plugin<int>("array", "sum")
                  .Arg(json, x => x.Items)
                  .Arg("age"));

             // array.some → condition
             s.When(p.Plugin<bool>("array", "some")
                    .Arg(json, x => x.Items)
                    .Arg("status").Arg("critical"))
              .Truthy()
              .Then(t => t.Element("has-critical").Show())
              .Else(e => e.Element("no-critical").Show());

             s.Element("results").AddClass("text-green-600");
         }))));

    // Button: send plugin-computed values to server
    Html.On(plan, t => t.CustomEvent("send-plugin-data", p =>
        p.Get("/Sandbox/Plugins/ArrayManager/PluginEcho")
         .Gather(g => g
             .Plugin<int>("array", "count", "count")  // ← can't chain .Arg here
             // For gather with args, need to use Header or rethink
             .Header("X-Array-Count", p.Plugin<int>("array", "count"))
             .FromUrl("status"))
         .Response(r => r.OnSuccess<PluginEchoResponse>((json, s) =>
         {
             s.Element("echo-count").SetText(json, x => x.ReceivedCount);
             s.Element("echo-header").SetText(json, x => x.ReceivedHeader);
             s.Element("echo-status").SetText(json, x => x.ReceivedStatus);
             s.Element("echo-result").AddClass("text-green-600");
         }))));
}
```

**Element IDs:**
- `#total` → "5"
- `#first-name` → first resident object (or its name)
- `#active-count` → "3" (3 active)
- `#total-age` → "393" (82+75+68+91+77)
- `#has-critical` / `#no-critical` → no-critical visible (no critical status)
- `#echo-count`, `#echo-header`, `#echo-status` → server echoes

**Update sandbox index** at `/Sandbox/Index.cshtml` to include link to `/Sandbox/Plugins/ArrayManager`.

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

Navigate to `/Sandbox/Plugins/ArrayManager`.

| Test | Assert |
|---|---|
| `array_count_displayed` | `#total` → "5" |
| `array_filter_count_displayed` | `#active-count` → "3" |
| `array_sum_displayed` | `#total-age` → "393" |
| `array_some_condition_evaluates` | `#no-critical` visible |
| `array_results_success_class` | `#results` green |
| `plugin_echo_count_from_server` | Click send → `#echo-count` has value |
| `plugin_echo_header_from_server` | Click send → `#echo-header` has value |
| `plugin_echo_url_composes` | Click send → `#echo-status` has value |

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
