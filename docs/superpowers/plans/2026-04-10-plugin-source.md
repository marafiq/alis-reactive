# Plugin Source

**Goal:** User-defined JS objects as value sources and call targets. One DSL name: `Plugin`. `<T>` for reads, no `<T>` for void. Methods only. Scalar returns display directly. Object returns chain as args to other plugin calls — not directly walkable by DSL.

**Prerequisite:** ReadProducer.args (`cfdefa66`), URL Query Source (3-kind Source union), `ExpressionPathHelper.ToResponsePath<TSource, TProp>` typed overload (must add — only `object?` overload exists today at line 82).

---

## Registration

Push-array. Plugins load before framework:

```typescript
((window as any).__alisPlugins ??= []).push({
  name: "array",
  instance: {
    count:    (arr: any[]) => arr.length,
    first:    (arr: any[]) => arr[0],
    filter:   (arr: any[], key: string, val: any) => arr.filter(i => i[key] === val),
    sum:      (arr: any[], key: string) => arr.reduce((s, i) => s + (Number(i[key]) || 0), 0),
    some:     (arr: any[], key: string, val: any) => arr.some(i => i[key] === val),
    pluck:    (arr: any[], index: number, key: string) => arr[index]?.[key],
  }
});
```

```html
<script type="module" src="~/js/plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Framework drains `window.__alisPlugins` at top of `root.ts` before boot.

---

## DSL

One name. `<T>` = read. No `<T>` = void.

```csharp
// Read — zero args, scalar result
p.Plugin<int>("array", "count")

// Read — with args, scalar result
p.Plugin<int>("array", "count").Arg(json, x => x.Items)

// Read — returns object (for chaining between plugin calls)
p.Plugin<object>("array", "first").Arg(json, x => x.Items)
// Object results flow as args to other plugin calls:
p.Plugin<string>("array", "pluck").Arg(p.Plugin<object>("array", "first").Arg(json, x => x.Items)).Arg("name")
// For direct display, use a plugin method that returns the scalar:
p.Plugin<string>("array", "pluck").Arg(json, x => x.Items).Arg(0).Arg("name")

// Read — nested (filter returns array, count takes it)
p.Plugin<int>("array", "count")
 .Arg(p.Plugin<object>("array", "filter")
      .Arg(json, x => x.Items).Arg("status").Arg("active"))

// Void — zero args
p.Plugin("logger", "flush").Fire()

// Void — with args
p.Plugin("analytics", "track").Arg("pageView").Fire()
```

### .Arg() Overloads

Two separate overloads for response body vs event args — matching `ElementBuilder.SetText` pattern:

```csharp
// Response body — ResponseBody<T> carries scope (success/error)
.Arg<TResponse, TProp>(ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
// Uses ExpressionPathHelper.ToResponsePath + body.Scope

// Event arg — uses PayloadSource.Event()
.Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path)
// Uses ExpressionPathHelper.ToEventPath + PayloadSource.Event()

// Typed source — component read, URL param, other plugin read
.Arg<TArg>(TypedSource<TArg> source)

// Literals
.Arg(string value)
.Arg(int value)
.Arg(bool value)
.Arg(long value)
```

C# overload resolution: `ResponseBody<T>` overload matches first for response payloads (has `where TResponse : class` constraint). Generic `TArgs` overload matches for event args. No ambiguity.

### Plan Model Mapping

| DSL | Plan Primitive | Existing? |
|---|---|---|
| `p.Plugin<T>(name, member)` | ReadProducer | ✓ |
| `p.Plugin<T>(name, member).Arg(...)` | ReadProducer + args | ✓ (cfdefa66) |
| `p.Plugin(name, member).Fire()` | CallReaction | ✓ |
| `p.Plugin(name, member).Arg(...).Fire()` | CallReaction + args | ✓ |

---

## Implementation

### Task 0: ExpressionPathHelper.ToResponsePath typed overload

**File:** `ExpressionPathHelper.cs` — add after line 82:

```csharp
public static string ToResponsePath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
{
    // Same implementation as ToEventPath<TSource, TProp> but for response paths
    return ToResponsePath((Expression<Func<TSource, object?>>)
        Expression.Lambda(Expression.Convert(expression.Body, typeof(object)), expression.Parameters));
}
```

Matches `ToEventPath<TSource, TProp>` at line 70. Required for `.Arg(json, x => x.Count)` where `Count` is a value type.

### Task 1: PluginSource

**File:** `Source.cs`:

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

Methods only. Auto-creates JsType. `WithMethod` at JsType.cs:53 replaces on re-registration (last-write-wins for same member).

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

    // Response body — carries scope from OnSuccess/OnError
    public PluginReadBuilder<TReturn, TModel> Arg<TResponse, TProp>(
        ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path)
        where TResponse : class
    {
        var responsePath = ExpressionPathHelper.ToResponsePath(path);
        _args.Add(ValueProducer.Read(body.Scope, responsePath, shape: Shape.FromClrType(typeof(TProp))));
        return this;
    }

    // Event arg — PayloadSource.Event()
    public PluginReadBuilder<TReturn, TModel> Arg<TArgs, TProp>(
        TArgs args, Expression<Func<TArgs, TProp>> path)
    {
        var eventPath = ExpressionPathHelper.ToEventPath(path);
        _args.Add(ValueProducer.Read(PayloadSource.Event(), eventPath, shape: Shape.FromClrType(typeof(TProp))));
        return this;
    }

    // Typed source
    public PluginReadBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
    { _args.Add(source.ToValueProducer()); return this; }

    // Literals
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

Same `.Arg()` overloads. `.Fire()` emits the CallReaction:

```csharp
public sealed class PluginCallBuilder<TModel> where TModel : class
{
    private readonly string _pluginName;
    private readonly string _method;
    private readonly IReactionEmitter _emitter;
    private readonly List<ValueProducer> _args = new List<ValueProducer>();

    internal PluginCallBuilder(string pluginName, string method, IReactionEmitter emitter)
    { _pluginName = pluginName; _method = method; _emitter = emitter; }

    // Same .Arg() overloads as PluginReadBuilder (response, event, typed, literals)

    public void Fire()
    {
        _emitter.AddStep(Reaction.Call(
            PluginSource.Of(_pluginName), _method,
            _args.Count > 0 ? _args : null));
    }
}
```

`.Fire()` is explicit — matches how HTTP chains end with `.Response()`. No deferred emission, no hidden state.

### Task 6: PipelineBuilder.Plugin

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

### Task 7: GatherBuilder.Plugin

**File:** `GatherBuilder.cs`:

Accepts `TypedPluginSource<T>` (which may carry args from a builder):

```csharp
public GatherBuilder<TModel> Plugin<T>(TypedPluginSource<T> source, string paramName)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentException("HTTP param name required.");
    Fields.Add(GatherField.Of(paramName, source.ToValueProducer()));
    return this;
}
```

DSL usage — args from any source available at request time:
```csharp
// Plugin read with component arg → gather param
g.Plugin(p.Plugin<int>("array", "count")
          .Arg(p.Component<FusionGrid>(m => m.Data).Value()), "itemCount")

// Plugin read with URL arg → gather param
g.Plugin(p.Plugin<int>("array", "getCountByStatus")
          .Arg(p.FromUrl("status")), "statusCount")

// Zero-arg plugin read → gather param
g.Plugin(p.Plugin<string>("auth", "getToken"), "token")

// In response handler — response body as arg
g.Plugin(p.Plugin<int>("array", "count").Arg(json, x => x.Items), "count")
```

The `p.Plugin<T>(...)` builder implicitly converts to `TypedPluginSource<T>` which carries the args. `GatherBuilder.Plugin` calls `.ToValueProducer()`. Shape, args, everything flows through. Args can be component reads, URL params, other plugin reads, literals — same as headers and route params.

### Task 8: Schema + TS Types

PluginSource: `{ kind: "plugin", name: string }`, `additionalProperties: false`, `pattern: "^\S+$"`.

Source union: 4 members. TS: `Source = ComponentSource | PayloadSource | UrlSource | PluginSource`.

### Task 9: TS — plugin-registry.ts

`registerPlugin(name, instance)` with validation. `resolvePlugin(name)` with fail-fast.

### Task 10: TS — resolver.ts

`resolveSource`: add `case "plugin": return resolvePlugin(source.name);`

`getJsTypeForSource`: add `case "plugin"`:
```typescript
case "plugin": {
  const typeKey = "plugin." + source.name;
  const jsType = plan.types[typeKey];
  if (!jsType) throw new Error(`[alis] type not found for plugin: "${source.name}"`);
  return jsType;
}
```

### Task 11: TS — evaluate.ts

Widen: `if (producer.from.kind === "component" || producer.from.kind === "plugin")`.

ReadProducer.args already handled by `cfdefa66` — `evaluateValue` passes args to `callMethod`.

Fix error message: use `source.name` for plugin kind.

### Task 12: TS — execute.ts

`executeCall` guard: allow `"plugin"` alongside `"component"` and `"payload"`.

After guard, plugin follows the component path:
```typescript
const jsType = getJsTypeForSource(plan, reaction.on);
const method = jsType.methods?.[reaction.method];
if (!method) throw new Error(`...`);
callMethod(root, method, args);
```

`getJsTypeForSource` already handles `"plugin"` (Task 10). `callMethod` already works. Zero new code in the call body — just the guard change.

`executeSet`: unchanged — rejects plugin (not component or payload).

Trace target: `reaction.on.kind === "plugin" ? (reaction.on as PluginSource).name : ...`

### Task 13: TS — root.ts drain

5 lines at top. Drain `window.__alisPlugins`. Delete after.

### Task 14: Vertical Slice — `/Sandbox/Plugins/ArrayManager`

Own page. Own controller. Own model. Own DTOs. Own index entry.

**sandbox-plugins.ts**: Array utility plugin + analytics void plugin.

**Controller**: GET `/Residents` returns 5 residents. GET `/PluginEcho` echoes count + header.

**View**: DomReady loads residents via GET, then:
- `array.count(items)` → `#arr-total` → "5"
- `array.pluck(items, 0, "name")` → `#arr-first-name` → "John Doe"
- `array.count(array.filter(items, "status", "active"))` → `#arr-active-count` → "3"
- `array.sum(items, "age")` → `#arr-total-age` → "393"
- `array.some(items, "status", "critical")` → condition → `#arr-no-critical` visible
- Button: gather + header with plugin values → server echoes
- Void call after HTTP success

**HTML**: Exact element IDs, `@Html.RenderPlan(plan)` at bottom.

---

## Tests

### C# Unit Tests (18) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `plugin_read_produces_plugin_source` | `from: { kind: "plugin" }` + AssertSchemaValid |
| `plugin_string_carries_shape` | `shape: { kind: "string" }` |
| `plugin_int_carries_shape` | `shape: { kind: "number" }` |
| `plugin_bool_carries_shape` | `shape: { kind: "boolean" }` |
| `plugin_in_condition` | CompareCondition with plugin read |
| `plugin_in_set_text` | SetReaction with plugin read |
| `plugin_in_header` | headers value is plugin read |
| `plugin_in_route_param` | route param is plugin read |
| `plugin_read_with_typed_source_arg` | ReadProducer.args from TypedSource |
| `plugin_read_with_literal_string_arg` | ReadProducer.args with literal |
| `plugin_read_with_literal_int_arg` | ReadProducer.args with literal int |
| `plugin_void_call_fire` | CallReaction with PluginSource |
| `plugin_void_call_with_arg_fire` | CallReaction.args with literal |
| `plugin_gather_from_typed_source` | GatherField via TypedPluginSource |
| `plugin_auto_registers_jstype` | plan.types["plugin.array"] |
| `plan_without_plugins_clean` | no "plugin" in JSON |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_throws` | ArgumentException |

### Playwright Tests (8) — `Plugins/WhenArrayPluginManipulates.cs`

Navigate to `/Sandbox/Plugins/ArrayManager`. Wait for `#arr-total` ≠ "—".

| Test | Action | Assert |
|---|---|---|
| `array_count_on_load` | DomReady | `#arr-total` → "5" |
| `array_pluck_first_name` | DomReady | `#arr-first-name` → "John Doe" |
| `array_nested_filter_count` | DomReady | `#arr-active-count` → "3" |
| `array_sum_age` | DomReady | `#arr-total-age` → "393" |
| `array_some_no_critical` | DomReady | `#arr-no-critical` visible |
| `array_results_class` | DomReady | `#arr-results` has `text-green-600` |
| `plugin_gather_echoes` | Click "Send to Server" | `#arr-echo-count` → "5" |
| `plugin_header_echoes` | Click "Send to Server" | `#arr-echo-header` → "5" |

### vitest Tests (8)

| Test | What It Proves |
|---|---|
| `registerPlugin round-trip` | store + resolve |
| `duplicate throws` | Error |
| `null instance throws` | Error |
| `whitespace name throws` | Error |
| `method zero-arg read` | evaluateValue → callMethod(root, method, []) |
| `method with args` | evaluateValue → callMethod(root, method, evaluatedArgs) |
| `missing member throws` | Error |
| `getJsTypeForSource finds plugin` | plan.types["plugin.name"] |

---

## Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — both bundles
- [ ] `ToResponsePath<TSource, TProp>` overload added
- [ ] Schema validates PluginSource
- [ ] JsType auto-created (methods only, WithMethod)
- [ ] `p.Plugin<T>()` read in conditions, SetText, headers, route params
- [ ] `p.Plugin<T>().Arg(json, x => x.Items)` response body args (ResponseBody scope)
- [ ] `p.Plugin<T>().Arg(args, a => a.Id)` event args (Event scope)
- [ ] `p.Plugin<T>().Arg("literal")` literal args
- [ ] `p.Plugin<T>().Arg(source)` typed source args
- [ ] Nested: `Plugin<int>("array","count").Arg(Plugin<object>("array","filter").Arg(...))`
- [ ] Object return: flows as arg to other plugin calls (chaining), not directly walkable by DSL
- [ ] `p.Plugin().Fire()` void call emits CallReaction
- [ ] `p.Plugin().Arg().Fire()` void with args
- [ ] `g.Plugin(typedPluginSource, "paramName")` gather with plugin source
- [ ] `executeCall` allows plugin, resolves via getJsTypeForSource
- [ ] `executeSet` rejects plugin
- [ ] Shape/ValueProducer stay internal
- [ ] `.Fire()` explicit — no deferred emission
- [ ] No inline JS
- [ ] Vertical slice on own page
- [ ] All 18 C# tests pass
- [ ] All 8 vitest tests pass
- [ ] All 8 Playwright tests pass
- [ ] All existing 808+ Playwright pass
