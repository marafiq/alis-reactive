# Plugin Source

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** User-defined JS objects as value sources and call targets. One DSL name: `Plugin`. Generic `<T>` for reads, non-generic for void calls. Methods only — no property reads. All four JS method patterns supported via existing plan primitives.

**Prerequisite:** ReadProducer.args (`cfdefa66`), URL Query Source (3-kind Source union, execute.ts guards).

---

## Registration

Plugin TS bundles load before framework. Push-array:

```typescript
((window as any).__alisPlugins ??= []).push({
  name: "auth",
  instance: {
    getToken: () => localStorage.getItem("token"),
    getUserId: () => 42,
    isAdmin: () => true,
    getSessionId: () => crypto.randomUUID(),
    track: (event: string) => navigator.sendBeacon("/analytics", event),
    format: (first: string, last: string) => `${last}, ${first}`
  }
});
```

```html
<script type="module" src="~/js/plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Framework drains at top of `root.ts` before boot — 5 lines, zero timing change.

---

## DSL

One name. `<T>` means read. No `<T>` means void.

```csharp
// READ — zero args
p.Plugin<string>("auth", "getToken")

// READ — with args
p.Plugin<string>("auth", "format").Arg(firstNameSource).Arg(lastNameSource)

// VOID — zero args
p.Plugin("logger", "flush")

// VOID — with args
p.Plugin("analytics", "track").Arg(source)
```

### In Every Context

```csharp
// DomReady — SetText + Condition
Html.On(plan, t => t.DomReady(p =>
{
    p.Element("theme").SetText(p.Plugin<string>("prefs", "getTheme"));
    p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
     .Then(t => t.Element("admin-panel").Show());
    p.Plugin("analytics", "pageView");
}));

// .Reactive event — Header + Gather + void in success
Html.NativeButton("save-btn", "Save")
    .Reactive(plan, evt => evt.Click, (args, p) =>
    {
        p.Post("/api/save")
         .Gather(g => g
             .Header("Authorization", p.Plugin<string>("auth", "getToken"))
             .Plugin("auth", "getSessionId", "session")
             .Include<FusionTextBox, Model>(m => m.Name))
         .Response(r => r.OnSuccess(s =>
         {
             s.Element("status").SetText("Saved");
             s.Plugin("analytics", "track").Arg(p.Plugin<string>("auth", "getUserId"));
         }));
    });

// Route params
g.RouteParam("tenantId", p.Plugin<int>("auth", "getTenantId"))

// Conditions with args
p.When(p.Plugin<bool>("permissions", "canAccess").Arg(args, a => a.ResourceId))
 .Truthy()
 .Then(t => t.Element("panel").Show());
```

### Plan Model Mapping

| DSL | Plan Primitive | Existing? |
|---|---|---|
| `p.Plugin<T>(name, member)` | ReadProducer (zero args) | ✓ |
| `p.Plugin<T>(name, member).Arg(...)` | ReadProducer + args | ✓ (cfdefa66) |
| `p.Plugin(name, member)` | CallReaction (zero args) | ✓ |
| `p.Plugin(name, member).Arg(...)` | CallReaction + args | ✓ |

Zero new plan model kinds.

---

## Implementation

### Task 1: PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs`

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

**File:** `Alis.Reactive/PlanModel/PlanBuildContext.cs`

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

Methods only. `returns` is null for void calls. JsType auto-created on first use. `WithMethod` (JsType.cs:53) handles re-registration.

### Task 3: TypedPluginSource<T>

**File:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`

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

Extends `TypedSource<T>` — plugs into conditions, SetText, headers, route params. Args from ReadProducer.args (cfdefa66).

### Task 4: PluginReadBuilder<T> (for .Arg on reads)

**File:** `Alis.Reactive/Builders/PluginReadBuilder.cs`

```csharp
public sealed class PluginReadBuilder<TReturn, TModel> where TModel : class
{
    private readonly string _pluginName;
    private readonly string _member;
    private readonly List<ValueProducer> _args = new List<ValueProducer>();

    internal PluginReadBuilder(string pluginName, string member)
    {
        _pluginName = pluginName;
        _member = member;
    }

    public PluginReadBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
    {
        if (source == null) throw new System.ArgumentNullException(nameof(source));
        _args.Add(source.ToValueProducer());
        return this;
    }

    public static implicit operator TypedPluginSource<TReturn>(
        PluginReadBuilder<TReturn, TModel> b) =>
        new TypedPluginSource<TReturn>(b._pluginName, b._member, b._args);
}
```

Implicit conversion — no `.Build()` needed. Each `.Arg<T>()` independently generic, calls internal `.ToValueProducer()`.

### Task 5: PluginCallBuilder (for void calls)

**File:** `Alis.Reactive/Builders/PluginCallBuilder.cs`

```csharp
public sealed class PluginCallBuilder<TModel> where TModel : class
{
    private readonly string _pluginName;
    private readonly string _method;
    private readonly IReactionEmitter _emitter;
    private readonly List<ValueProducer> _args = new List<ValueProducer>();
    private bool _emitted;

    internal PluginCallBuilder(string pluginName, string method, IReactionEmitter emitter)
    {
        _pluginName = pluginName;
        _method = method;
        _emitter = emitter;
    }

    public PluginCallBuilder<TModel> Arg<TArg>(TypedSource<TArg> source)
    {
        if (source == null) throw new System.ArgumentNullException(nameof(source));
        _args.Add(source.ToValueProducer());
        return this;
    }

    internal void Emit()
    {
        if (_emitted) return;
        _emitted = true;
        _emitter.AddStep(Reaction.Call(
            PluginSource.Of(_pluginName), _method,
            _args.Count > 0 ? _args : null));
    }
}
```

`PipelineBuilder.Plugin(name, member)` creates the builder AND calls `Emit()` for zero-arg case. For `.Arg()` chains, `PipelineBuilder` defers emit — the builder is tracked and emitted when the next pipeline step starts or at build time.

### Task 6: PipelineBuilder.Plugin overloads

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

```csharp
/// <summary>Reads a plugin method return value. Use .Arg() for args.</summary>
public PluginReadBuilder<T, TModel> Plugin<T>(string pluginName, string member)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(member))
        throw new System.ArgumentException("Member name required.", nameof(member));
    var shape = PlanModel.Shape.FromClrType(typeof(T));
    Context.EnsurePluginMethod(pluginName, member, returns: shape);
    return new PluginReadBuilder<T, TModel>(pluginName, member);
}

/// <summary>Calls a plugin method (void). Use .Arg() for args.</summary>
public PluginCallBuilder<TModel> Plugin(string pluginName, string member)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(member))
        throw new System.ArgumentException("Member name required.", nameof(member));
    Context.EnsurePluginMethod(pluginName, member);
    return new PluginCallBuilder<TModel>(pluginName, member, this);
}
```

### Task 7: GatherBuilder.Plugin

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

```csharp
public GatherBuilder<TModel> Plugin<T>(string pluginName, string member, string paramName)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(member))
        throw new System.ArgumentException("Member name required.", nameof(member));
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException("HTTP parameter name required.", nameof(paramName));
    var shape = Shape.FromClrType(typeof(T));
    _context.EnsurePluginMethod(pluginName, member, returns: shape);
    var value = ValueProducer.Read(PluginSource.Of(pluginName), member, shape: shape);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}
```

Generic `<T>` — shape carried. No `Shape.Any`.

### Task 8: Schema — PluginSource

Add to Source oneOf + definition:

```json
"PluginSource": {
  "type": "object",
  "required": ["kind", "name"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "plugin" },
    "name": { "type": "string", "minLength": 1, "pattern": "^\\S+$" }
  }
}
```

### Task 9: TS Types — PluginSource

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;
export interface PluginSource { kind: "plugin"; name: string; }
```

### Task 10: TS — Plugin registry

**New file:** `Scripts/core/plugin-registry.ts`

```typescript
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (!name || !name.trim()) throw new Error("[alis] plugin name required");
  if (instance == null) throw new Error(`[alis] plugin "${name}" must not be null`);
  if (typeof instance !== "object") throw new Error(`[alis] plugin "${name}" must be an object`);
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance) throw new Error(`[alis] plugin not found: "${name}"`);
  return instance;
}
```

### Task 11: TS — resolver.ts

Add `case "plugin": return resolvePlugin(source.name);` to resolveSource.

Update getJsTypeForSource for `"plugin"` kind: `plan.types["plugin." + source.name]`.

### Task 12: TS — evaluate.ts

Widen: `if (producer.from.kind === "component" || producer.from.kind === "plugin")`.

Fix error message for plugin source kind. ReadProducer.args already handled by `cfdefa66`.

### Task 13: TS — execute.ts

`executeCall` guard allows `"plugin"`. `executeSet` unchanged — rejects plugin.

Fix trace target for plugin source kind.

### Task 14: TS — root.ts drain

```typescript
import { registerPlugin } from "./core/plugin-registry";
const pending = (window as any).__alisPlugins as Array<{ name: string; instance: unknown }> | undefined;
if (pending) {
  for (const entry of pending) registerPlugin(entry.name, entry.instance);
  delete (window as any).__alisPlugins;
}
```

### Task 15: Sandbox

**sandbox-plugins.ts** → `wwwroot/js/sandbox-plugins.js`:

```typescript
((window as any).__alisPlugins ??= []).push({
  name: "auth",
  instance: { getToken: () => "sandbox-token", getUserId: () => 42, isAdmin: () => true }
});
((window as any).__alisPlugins ??= []).push({
  name: "userPrefs",
  instance: { getTheme: () => "dark" }
});
```

**Sections 23-25:** Read in SetText/condition, read in gather+header, void call.

---

## Tests

### C# Unit Tests (18) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `plugin_read_produces_plugin_source` | from: { kind: "plugin" } + AssertSchemaValid |
| `plugin_string_read_carries_shape` | shape: { kind: "string" } |
| `plugin_int_read_carries_shape` | shape: { kind: "number" } |
| `plugin_bool_read_carries_shape` | shape: { kind: "boolean" } |
| `plugin_read_in_condition` | CompareCondition with plugin read |
| `plugin_read_in_set_text` | SetReaction with plugin read |
| `plugin_read_in_header` | headers value is plugin read |
| `plugin_read_in_route_param` | route param is plugin read |
| `plugin_read_with_args` | ReadProducer.args present |
| `plugin_void_call_produces_call_reaction` | CallReaction with PluginSource |
| `plugin_void_call_with_args` | CallReaction.args present |
| `plugin_gather_carries_shape` | GatherField with shape from `<T>` |
| `plugin_auto_registers_jstype` | plan.types["plugin.auth"] created |
| `plan_without_plugins_clean` | no "plugin" in JSON |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_throws` | ArgumentException |
| `null_arg_throws` | ArgumentNullException |
| `empty_gather_param_throws` | ArgumentException |

### Playwright Tests (6) — `WhenPluginsProvideValues.cs`

| Test | Assert |
|---|---|
| `plugin_read_displayed_in_element` | `#plugin-theme` → "dark" |
| `plugin_condition_shows_admin_panel` | `#plugin-admin-panel` visible |
| `plugin_gather_sends_token` | `#plugin-echo-token` has value |
| `plugin_header_reaches_server` | `#plugin-echo-header` → value |
| `plugin_void_call_no_errors` | no console errors |
| `plugin_gather_success_class` | `#plugin-result` green |

### vitest Tests (8) — `plugin-registry.test.ts` + `evaluate-plugin.test.ts`

| Test | What It Proves |
|---|---|
| `registerPlugin round-trip` | store + resolve |
| `duplicate throws` | Error |
| `null instance throws` | Error |
| `whitespace name throws` | Error |
| `plugin method zero-arg read` | evaluateValue calls method, returns value |
| `plugin method with args` | evaluateValue passes args to callMethod |
| `plugin missing member throws` | Error |
| `getJsTypeForSource finds plugin type` | plan.types["plugin.name"] |

---

## Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates PluginSource
- [ ] JsType auto-created by DSL usage (methods only)
- [ ] `p.Plugin<T>()` read works in conditions, SetText, headers, gather, route params
- [ ] `p.Plugin<T>().Arg()` passes args via ReadProducer.args
- [ ] `p.Plugin()` void call emits CallReaction
- [ ] `p.Plugin().Arg()` void call with args
- [ ] `g.Plugin<T>()` gather carries shape from `<T>`
- [ ] Missing plugin throws at runtime
- [ ] Shape stays internal, ValueProducer stays internal
- [ ] execute.ts Set rejects plugin; Call allows plugin
- [ ] No inline JS in views
- [ ] All 18 C# unit tests pass
- [ ] All 8 vitest tests pass
- [ ] All 6 Playwright tests pass
- [ ] All existing 808+ Playwright tests pass
