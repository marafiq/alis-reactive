# Plugin Source — User-Defined JS Objects as Value Sources

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to register arbitrary JavaScript objects as value sources and call targets in the plan. A plugin is a resolved root — same as a component. Four operations: read property, call zero-arg method (get return), call method with args (get return), call method with args (void). All use existing framework primitives.

**Prerequisite:** ReadProducer.args must be landed (commit `cfdefa66`). URL Query Source must be landed (Source union at 3 kinds, execute.ts guards).

---

## Registration (TS Infrastructure — Not DSL)

Plugin TS bundles load BEFORE the framework. Push-array pattern — no framework imports, no load-order dependency between plugin scripts:

```typescript
// plugins/auth.ts → bundled to plugins.js
((window as any).__alisPlugins ??= []).push({
  name: "auth",
  instance: {
    getToken: () => localStorage.getItem("auth_token"),
    getUserId: () => 42,
    isAdmin: () => true,
    flush: () => navigator.sendBeacon("/log", ""),
    trackEvent: (name: string) => console.log("track:", name),
    format: (first: string, last: string) => `${last}, ${first}`
  }
});
```

```html
<!-- _Layout.cshtml -->
<script type="module" src="~/js/plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Framework drains the queue in `root.ts` at module-level before boot:

```typescript
import { registerPlugin } from "./core/plugin-registry";
const pending = (window as any).__alisPlugins as Array<{ name: string; instance: unknown }> | undefined;
if (pending) {
  for (const entry of pending) registerPlugin(entry.name, entry.instance);
  delete (window as any).__alisPlugins;
}
```

---

## DSL — Four Cases

Every case maps to an existing plan model primitive. Zero new kinds.

### Case 4: Returns value, no args → ReadProducer

```csharp
p.Plugin<string>("auth", "getToken")
// Plan: { kind: "read", from: { kind: "plugin", name: "auth" }, member: "getToken", shape: { kind: "string" } }
```

Used in: conditions, SetText, headers, gather, route params — anywhere `TypedSource<T>` is accepted.

### Case 3: Returns value, with args → ReadProducer + args

```csharp
p.PluginRead<string>("auth", "format")
 .Arg(p.Component<FusionTextBox>(m => m.FirstName).Value())
 .Arg(p.Component<FusionTextBox>(m => m.LastName).Value())
// Plan: { kind: "read", from: { kind: "plugin" }, member: "format", shape: { kind: "string" },
//         args: [{ kind: "read", from: { kind: "component" }, member: "value" }, ...] }
```

Uses ReadProducer.args (landed in `cfdefa66`). Builder collects args via `.Arg<T>(TypedSource<T>)` — each independently generic, calls `.ToValueProducer()` internally. Implicit conversion to `TypedSource<TReturn>`.

### Case 1: No return, no args → CallReaction

```csharp
p.PluginCall("logger", "flush")
// Plan: { kind: "call", on: { kind: "plugin", name: "logger" }, method: "flush" }
```

### Case 2: No return, with args → CallReaction + args

```csharp
p.PluginCall("analytics", "trackEvent")
 .Arg(p.Plugin<string>("auth", "getUserId"))
// Plan: { kind: "call", on: { kind: "plugin" }, method: "trackEvent",
//         args: [{ kind: "read", from: { kind: "plugin" }, member: "getUserId" }] }
```

### Plan Model Mapping

| Case | Returns | Args | Plan Primitive | Existing? |
|---|---|---|---|---|
| 4 | Yes | No | ReadProducer | ✓ |
| 3 | Yes | Yes | ReadProducer + args | ✓ (cfdefa66) |
| 1 | No | No | CallReaction | ✓ |
| 2 | No | Yes | CallReaction + args | ✓ |

---

## DSL In Every Context

```csharp
// CONDITIONS
p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
 .Then(t => t.Element("admin-panel").Show());

// SETTEXT
p.Element("theme").SetText(p.Plugin<string>("userPrefs", "theme"));

// HEADERS
p.Header("Authorization", p.Plugin<string>("auth", "getToken"))

// GATHER
g.Plugin("auth", "getToken", "token")

// ROUTE PARAMS
g.RouteParam("tenantId", p.Plugin<int>("auth", "getTenantId"))

// READ WITH ARGS
p.Element("name").SetText(
    p.PluginRead<string>("auth", "format")
     .Arg(firstNameTextBox.Value())
     .Arg(lastNameTextBox.Value()));

// VOID CALL
p.PluginCall("logger", "flush");
p.PluginCall("analytics", "trackEvent")
 .Arg(p.Plugin<string>("auth", "getUserId"));

// COMPOSITION — all sources in one request
p.Get("/api/facilities/{id}/residents")
 .Gather(g => g
     .RouteParam("id", p.Plugin<int>("auth", "getTenantId"))
     .Header("Authorization", p.Plugin<string>("auth", "getToken"))
     .FromUrl("status")
     .Include<FusionDropDownList, Model>(m => m.Filter));
```

---

## Architecture

### Source Resolution

| Source | Resolution | Member Access | JsType? |
|---|---|---|---|
| ComponentSource | DOM → vendor root | JsType walkPath | Yes |
| PayloadSource | ExecContext scope | walk() dot-path | No |
| UrlSource | URLSearchParams | params.get(member) | No |
| **PluginSource** | **Registry lookup** | **JsType walkPath** | **Yes** |

### JsType Auto-Registration

`p.Plugin<T>("auth", "getToken")` auto-registers the member on `plan.types["plugin.auth"]`. Shape from `<T>` via `Shape.FromClrType(typeof(T))` — internal. No separate `RegisterPlugin` call needed. The DSL usage IS the type declaration.

First call creates the JsType. Subsequent calls for the same plugin add members. Shape conflicts detected via `ShapeCompat.Resolve` (JsType.cs:73).

### Encapsulation

- `PluginSource`: public sealed class, private constructor, internal factory `Of(name)`
- `TypedPluginSource<T>`: public sealed, extends `TypedSource<T>`, internal constructor
- `PluginReadBuilder<T>`: public, `.Arg<TArg>(TypedSource<TArg>)` calls internal `.ToValueProducer()`
- `PluginCallBuilder`: public, `.Arg<TArg>(TypedSource<TArg>)` calls internal `.ToValueProducer()`
- Shape: stays internal — `<T>` generics carry type info
- ValueProducer: stays internal — builders create them
- Plugin registry: internal module, `registerPlugin` internal function

### Mutation Rules

- **Read** (evaluateValue): property read or method call (zero-arg or with-args via ReadProducer.args) → returns value
- **Call** (executeCall): method with args → void. `execute.ts` Call guard ALLOWS PluginSource
- **Set** (executeSet): NOT supported. `execute.ts` Set guard REJECTS PluginSource

### Polymorphic Serialization

`WriteOnlyPolymorphicConverter<Source>` dispatches on `value.GetType()`. Zero converter changes for PluginSource.

---

## Step-by-Step Implementation

### Task 1: C# — PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs` — after UrlSource:

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

### Task 2: C# — PlanBuildContext.EnsurePluginMember

**File:** `Alis.Reactive/PlanModel/PlanBuildContext.cs`:

```csharp
internal string EnsurePluginMember(string pluginName, string member, Shape shape, bool isMethod)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(member))
        throw new System.ArgumentException("Member name required.", nameof(member));

    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.ContainsKey(typeKey))
        _plan.MutableTypes[typeKey] = new JsType();

    var jsType = _plan.MutableTypes[typeKey];
    if (isMethod)
        jsType.WithMethod(member, Path.Parse(member), returns: shape);
    else
        jsType.WithProperty(member, Path.Parse(member), shape, "read");

    return typeKey;
}

internal void EnsurePluginMethod(string pluginName, string method)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new System.ArgumentException("Plugin name required.", nameof(pluginName));
    if (string.IsNullOrWhiteSpace(method))
        throw new System.ArgumentException("Method name required.", nameof(method));

    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.ContainsKey(typeKey))
        _plan.MutableTypes[typeKey] = new JsType();

    // Void method — no returns shape, path only
    _plan.MutableTypes[typeKey].WithMethod(method, Path.Parse(method));
}
```

Auto-creates JsType on first use. `ShapeCompat.Resolve` in `JsType.WithProperty/WithMethod` handles re-registration with compatible shapes.

### Task 3: C# — TypedPluginSource<T>

**New file:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`:

```csharp
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
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
}
```

### Task 4: C# — PluginReadBuilder<T> and PluginCallBuilder

**New file:** `Alis.Reactive/Builders/PluginReadBuilder.cs`:

```csharp
using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
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

        public static implicit operator TypedPluginSource<TReturn>(PluginReadBuilder<TReturn, TModel> builder) =>
            new TypedPluginSource<TReturn>(builder._pluginName, builder._member, builder._args);
    }
}
```

**New file:** `Alis.Reactive/Builders/PluginCallBuilder.cs`:

```csharp
using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public sealed class PluginCallBuilder<TModel> where TModel : class
    {
        private readonly string _pluginName;
        private readonly string _method;
        private readonly IReactionEmitter _emitter;
        private readonly List<ValueProducer> _args = new List<ValueProducer>();

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

        /// <summary>Emits the CallReaction. Called automatically when chaining ends.</summary>
        internal void Emit()
        {
            _emitter.AddStep(Reaction.Call(
                PluginSource.Of(_pluginName), _method,
                _args.Count > 0 ? _args : null));
        }
    }
}
```

Note: `PluginCallBuilder.Emit()` needs to be called. Options: (a) explicit `.Fire()`, (b) called by PipelineBuilder after the builder chain. Simplest: the `PluginCall` method on PipelineBuilder returns the builder AND emits immediately for zero-arg case, or the caller chains `.Arg()` then builder emits on dispose/finalize. **For clarity, use explicit `.Fire()`:**

```csharp
p.PluginCall("analytics", "trackEvent").Arg(source).Fire();
p.PluginCall("logger", "flush").Fire();
```

Update PluginCallBuilder:
```csharp
public void Fire()
{
    _emitter.AddStep(Reaction.Call(
        PluginSource.Of(_pluginName), _method,
        _args.Count > 0 ? _args : null));
}
```

### Task 5: C# — PipelineBuilder.Plugin / PluginRead / PluginCall

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`:

```csharp
/// <summary>Reads a plugin member value. Zero-arg method or property.</summary>
public Conditions.TypedPluginSource<T> Plugin<T>(string pluginName, string member)
{
    var shape = PlanModel.Shape.FromClrType(typeof(T));
    Context.EnsurePluginMember(pluginName, member, shape, isMethod: true);
    return new Conditions.TypedPluginSource<T>(pluginName, member);
}

/// <summary>Reads a plugin method with args. Use .Arg() to pass values.</summary>
public PluginReadBuilder<T, TModel> PluginRead<T>(string pluginName, string member)
{
    var shape = PlanModel.Shape.FromClrType(typeof(T));
    Context.EnsurePluginMember(pluginName, member, shape, isMethod: true);
    return new PluginReadBuilder<T, TModel>(pluginName, member);
}

/// <summary>Calls a plugin method (void). Use .Arg() then .Fire().</summary>
public PluginCallBuilder<TModel> PluginCall(string pluginName, string method)
{
    Context.EnsurePluginMethod(pluginName, method);
    return new PluginCallBuilder<TModel>(pluginName, method, this);
}
```

### Task 6: C# — GatherBuilder.Plugin

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`:

```csharp
public GatherBuilder<TModel> Plugin(string pluginName, string member, string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException("HTTP parameter name required.", nameof(paramName));
    _context.EnsurePluginMember(pluginName, member, Shape.Any, isMethod: true);
    var value = ValueProducer.Read(PluginSource.Of(pluginName), member);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}
```

Note: `Shape.Any` here because gather serializes via `evaluateValue` → `formatForWire` using the JsType's shape. The gather field doesn't need its own shape.

### Task 7: Schema — PluginSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`:

Add to Source oneOf:
```json
{ "$ref": "#/$defs/PluginSource" }
```

Add definition:
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

### Task 8: TS Types — PluginSource

**File:** `Scripts/types/plan.ts`:

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;

export interface PluginSource {
  kind: "plugin";
  name: string;
}
```

### Task 9: TS Runtime — Plugin registry

**New file:** `Scripts/core/plugin-registry.ts`:

```typescript
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (!name || !name.trim()) throw new Error("[alis] plugin name required");
  if (instance == null) throw new Error(`[alis] plugin "${name}" instance must not be null`);
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

### Task 10: TS Runtime — resolver.ts

Add `case "plugin": return resolvePlugin(source.name);` to resolveSource.

Update getJsTypeForSource:
```typescript
case "plugin": {
  const typeKey = "plugin." + source.name;
  const jsType = plan.types[typeKey];
  if (!jsType) throw new Error(`[alis] type not found for plugin: "${source.name}"`);
  return jsType;
}
```

### Task 11: TS Runtime — evaluate.ts

Widen component branch: `if (producer.from.kind === "component" || producer.from.kind === "plugin")`.

Fix error message for plugin source kind.

Note: ReadProducer.args already handled by `cfdefa66` — evaluateValue passes `producer.args` to callMethod.

### Task 12: TS Runtime — execute.ts

Update `executeCall` guard to allow `"plugin"`. `executeSet` remains unchanged — rejects plugins.

Fix trace target for plugin source kind.

### Task 13: TS Runtime — root.ts queue drain

Add ~5 lines at top of root.ts to drain `window.__alisPlugins`.

### Task 14: Sandbox

**sandbox-plugins.ts** → builds to `wwwroot/js/sandbox-plugins.js`:

```typescript
((window as any).__alisPlugins ??= []).push({
  name: "auth",
  instance: { getToken: () => "sandbox-token", getUserId: () => 42, isAdmin: () => true }
});
((window as any).__alisPlugins ??= []).push({
  name: "userPrefs",
  instance: { theme: "dark", locale: "en-US" }
});
```

**View sections 23-25** use `p.Plugin<T>()`, `p.PluginCall()`, conditions, SetText, gather+header.

**Controller** echoes plugin values back for Playwright verification.

---

## Tests

### C# Unit Tests (20) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `plugin_read_produces_read_with_plugin_source` | from: { kind: "plugin" } + AssertSchemaValid |
| `plugin_string_carries_string_shape` | shape: { kind: "string" } |
| `plugin_int_carries_number_shape` | shape: { kind: "number" } |
| `plugin_bool_carries_boolean_shape` | shape: { kind: "boolean" } |
| `plugin_in_condition_produces_compare` | CompareCondition with plugin read |
| `plugin_in_set_text_produces_set_reaction` | SetReaction with plugin read |
| `plugin_in_header_produces_header_value` | headers value is plugin read |
| `plugin_in_gather_produces_gather_field` | GatherField with plugin source |
| `plugin_in_route_param_composes` | route param value is plugin read |
| `plugin_read_with_args_includes_args` | ReadProducer.args has ValueProducers |
| `plugin_call_produces_call_reaction` | CallReaction with plugin source |
| `plugin_call_with_args_includes_args` | CallReaction.args has ValueProducers |
| `plugin_auto_registers_jstype` | plan.types["plugin.auth"] created |
| `plugin_second_member_adds_to_jstype` | same JsType gets two members |
| `plan_without_plugins_has_no_plugin_kind` | JSON does NOT contain "plugin" |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_name_throws` | ArgumentException |
| `null_arg_source_throws` | ArgumentNullException |
| `empty_gather_param_throws` | ArgumentException |
| `plugin_read_composes_with_all_sources` | route param + header + gather + plugin in one request |

### Playwright Tests (6) — `WhenPluginsProvideValues.cs`

| Test | Assert |
|---|---|
| `plugin_property_displayed_in_element` | `#plugin-theme` → "dark" |
| `plugin_condition_shows_admin_panel` | `#plugin-admin-panel` visible |
| `plugin_gather_sends_token_to_server` | `#plugin-echo-token` has value |
| `plugin_header_reaches_server` | `#plugin-echo-theme` → "dark" |
| `plugin_call_fires_without_error` | No console errors after void call |
| `plugin_gather_applies_success_class` | `#plugin-echo-result` green |

### vitest Tests (8) — `plugin-registry.test.ts` + `evaluate-plugin.test.ts`

| Test | What It Proves |
|---|---|
| `registerPlugin stores and resolvePlugin retrieves` | Round-trip |
| `duplicate registration throws` | Error |
| `null instance throws` | Error |
| `whitespace name throws` | Error |
| `plugin property read via JsType walkPath` | evaluateValue reads root.theme |
| `plugin method read calls zero-arg function` | evaluateValue calls root.getToken() |
| `plugin method read with args passes evaluated args` | evaluateValue passes args to callMethod |
| `plugin missing member throws` | Error |

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates PluginSource (`additionalProperties: false`, name pattern `^\S+$`)
- [ ] JsType auto-created by `p.Plugin<T>()` usage
- [ ] Plugin property read works (conditions, SetText, gather, headers, route params)
- [ ] Plugin zero-arg method read works
- [ ] Plugin read with args passes args to callMethod
- [ ] Plugin void call works (CallReaction)
- [ ] Plugin void call with args works
- [ ] Missing plugin throws at runtime (fail-fast)
- [ ] Shape flows from `<T>` through plan JSON through TS runtime
- [ ] No inline JavaScript in views
- [ ] No internal leaks (Shape, ValueProducer stay internal)
- [ ] execute.ts Set guard rejects PluginSource; Call guard allows PluginSource
- [ ] All 20 C# unit tests pass
- [ ] All 8 vitest tests pass
- [ ] All 6 Playwright tests pass
- [ ] All existing 808+ Playwright tests pass (no regressions)
