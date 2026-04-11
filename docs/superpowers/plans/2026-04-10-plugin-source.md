# Plugin Source — User-Defined JS Objects as Value Sources

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to register arbitrary JavaScript objects as value sources in the plan. A plugin is a resolved root — just like a component. Once resolved, the same shared operations apply: read property, call method (with or without args). The runtime doesn't care WHERE the root came from — it resolves it via the registry and operates on it via JsType metadata.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver + registry

**Prerequisite:** URL Query Source must be landed (Source union already widened to 3 kinds, execute.ts guards in place).

---

## Architecture

### Resolution

After URL Query Source, the runtime has THREE resolution paths. A plugin is a FOURTH:

| Source | Resolution | Member Access | JsType? |
|---|---|---|---|
| ComponentSource | DOM → vendor root | JsType walkPath | Yes |
| PayloadSource | ExecContext scope | walk() dot-path | No |
| UrlSource | URLSearchParams | params.get(member) | No |
| **PluginSource** | **Registry lookup** | **JsType walkPath** (same as component) | **Yes** |

### Two-Part Registration

**C# side (plan JSON):** Declares capabilities — which members exist, shapes, property vs method.

```csharp
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);       // getter: zero-arg, returns string
    p.Method("getUserId", Shape.Number);      // getter: zero-arg, returns number
    p.Method("isAdmin", Shape.Boolean);       // getter: zero-arg, returns boolean
    p.Method("trackEvent", new List<Shape> { Shape.String }); // action: one arg, fire-and-forget
    p.Property("theme", Shape.String);        // direct property read
});
```

**TS side (runtime) — Push-Array Pattern:**

Plugins bundle SEPARATELY. They push to `window.__alisPlugins` — a passive queue. Plugin scripts MUST load before the framework script.

```typescript
// plugins/auth-plugin.ts (developer's own TS, bundled separately)
((window as any).__alisPlugins ??= []).push({
  name: "auth",
  instance: {
    getToken: () => localStorage.getItem("auth_token"),
    getUserId: () => JSON.parse(localStorage.getItem("user")!).id,
    isAdmin: () => JSON.parse(localStorage.getItem("user")!).role === "admin",
    trackEvent: (name: string) => navigator.sendBeacon("/analytics", name),
    theme: "dark"
  }
});
```

**Script order — REQUIRED:**
```html
<!-- Plugins FIRST — push to passive queue -->
<script type="module" src="~/js/plugins.js"></script>
<!-- Framework SECOND — drains queue, then boots -->
<script type="module" src="~/js/alis-reactive.js"></script>
@Html.RenderPlan(plan)
```

**Framework drains the queue** in `root.ts` at module-level, before `initConfirm()` and plan parsing:
```typescript
import { registerPlugin } from "./core/plugin-registry";
const pending = (window as any).__alisPlugins as Array<{ name: string; instance: unknown }> | undefined;
if (pending) {
  for (const entry of pending) registerPlugin(entry.name, entry.instance);
  delete (window as any).__alisPlugins;
}
```

No boot timing change. No permanent globals. Pages without plugins: queue is undefined, zero cost.

### RegisterPlugin Is Mandatory

C# `RegisterPlugin` must be called before any `p.Plugin(...)` reference. The builder validates:
- Plugin not registered → throw at build time
- Member not declared → throw at build time
- Shape incompatible with `<T>` → throw at build time

### Two Method Patterns

1. **Getter** `Method(name, returns)` — zero-arg, returns value. Used by `evaluateValue` for reads.
2. **Action** `Method(name, args)` — with args, no return. Used by `CallReaction` for side effects.

Both use existing `JsType.WithMethod` (JsType.cs:53) and `callMethod` (resolver.ts:125).

### Mutation Rules

- **Read** (evaluateValue) — property or zero-arg method → returns value. Primary use case.
- **Call** (CallReaction) — method with args → side effect. `execute.ts` Call guard ALLOWS PluginSource.
- **Set** (SetReaction) — NOT supported. `execute.ts` Set guard REJECTS PluginSource.

### Null Semantics

Plugin members CAN return null. Flows through existing `raw == null ? raw : applyShape(...)`. Conditions `.IsNull()` and `.NotNull()` work.

### DSL

```csharp
// Register ALL plugins at plan construction time
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);
    p.Method("getUserId", Shape.Number);
    p.Method("isAdmin", Shape.Boolean);
    p.Method("trackEvent", new List<Shape> { Shape.String });
});
plan.RegisterPlugin("userPrefs", p => {
    p.Property("theme", Shape.String);
});

// READ in headers
p.Header("Authorization", p.Plugin<string>("auth", "getToken"))

// READ in conditions
p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
 .Then(t => t.Element("admin-panel").Show())

// READ in SetText
p.Element("theme-display").SetText(p.Plugin<string>("userPrefs", "theme"))

// READ in gather
g.Plugin("auth", "getToken", "token")

// READ in route params
g.RouteParam("tenantId", p.Plugin<int>("auth", "getUserId"))

// CALL with args (fire-and-forget via CallReaction)
// Requires ComponentRef-like API — see Task 6b
```

### Plan JSON

```json
{
  "kind": "read",
  "from": { "kind": "plugin", "name": "auth" },
  "member": "getToken",
  "shape": { "kind": "string" }
}
```

**Polymorphic serialization:** `WriteOnlyPolymorphicConverter<Source>` dispatches on `value.GetType()` (Serialization/WriteOnlyPolymorphicConverter.cs:10). Zero converter changes needed.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs` — after UrlSource:

```csharp
/// <summary>Reads a value from a user-registered JS plugin object.</summary>
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

### Task 2: C# Plan Model — ValueProducer.ReadPlugin

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`:

```csharp
internal static ValueProducer ReadPlugin(string pluginName, string member, Shape shape = null) =>
    Read(PluginSource.Of(pluginName), member, shape: shape);
```

### Task 3: C# Builder — PlanBuildContext plugin methods

**File:** `Alis.Reactive/PlanModel/PlanBuildContext.cs`:

```csharp
private readonly HashSet<string> _registeredPlugins = new HashSet<string>();

internal void RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure)
{
    if (!_registeredPlugins.Add(pluginName))
        throw new InvalidOperationException($"Plugin '{pluginName}' is already registered.");
    var typeKey = "plugin." + pluginName;
    _plan.MutableTypes[typeKey] = new JsType();
    configure(new PluginTypeBuilder(this, pluginName, typeKey));
}

internal void ValidatePluginMember(string pluginName, string member)
{
    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.TryGetValue(typeKey, out var jsType))
        throw new InvalidOperationException($"Plugin '{pluginName}' is not registered.");
    var hasProp = jsType.Properties != null && jsType.Properties.ContainsKey(member);
    var hasMethod = jsType.Methods != null && jsType.Methods.ContainsKey(member);
    if (!hasProp && !hasMethod)
        throw new InvalidOperationException($"Member '{member}' not declared on plugin '{pluginName}'.");
}

internal Shape GetPluginMemberShape(string pluginName, string member)
{
    ValidatePluginMember(pluginName, member);
    var typeKey = "plugin." + pluginName;
    var jsType = _plan.MutableTypes[typeKey];
    if (jsType.Properties != null && jsType.Properties.TryGetValue(member, out var prop))
        return prop.Shape;
    if (jsType.Methods != null && jsType.Methods.TryGetValue(member, out var method) && method.Returns != null)
        return method.Returns;
    throw new InvalidOperationException($"Plugin '{pluginName}' member '{member}' has no shape.");
}
```

### Task 4: C# Builder — PluginTypeBuilder

**New file:** `Alis.Reactive/Builders/PluginTypeBuilder.cs`:

```csharp
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public sealed class PluginTypeBuilder
    {
        private readonly PlanBuildContext _context;
        private readonly string _pluginName;
        private readonly string _typeKey;

        internal PluginTypeBuilder(PlanBuildContext context, string pluginName, string typeKey)
        {
            _context = context; _pluginName = pluginName; _typeKey = typeKey;
        }

        /// <summary>Declares a readable property.</summary>
        public PluginTypeBuilder Property(string name, Shape shape)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new System.ArgumentException("Name required.", nameof(name));
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            _context.Plan.MutableTypes[_typeKey].WithProperty(name, Path.Parse(name), shape, "read");
            return this;
        }

        /// <summary>Declares a zero-arg getter method (returns a value).</summary>
        public PluginTypeBuilder Method(string name, Shape returns)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new System.ArgumentException("Name required.", nameof(name));
            if (returns == null) throw new System.ArgumentNullException(nameof(returns));
            _context.Plan.MutableTypes[_typeKey].WithMethod(name, Path.Parse(name), returns: returns);
            return this;
        }

        /// <summary>Declares an action method with args (fire-and-forget via CallReaction).</summary>
        public PluginTypeBuilder Method(string name, List<Shape> args)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new System.ArgumentException("Name required.", nameof(name));
            _context.Plan.MutableTypes[_typeKey].WithMethod(name, Path.Parse(name), args: args);
            return this;
        }
    }
}
```

### Task 5: C# Builder — TypedPluginSource<T>

**New file:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`:

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    public sealed class TypedPluginSource<TProp> : TypedSource<TProp>
    {
        private readonly string _pluginName;
        private readonly string _member;

        internal TypedPluginSource(string pluginName, string member)
        {
            _pluginName = pluginName;
            _member = member;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.Read(PluginSource.Of(_pluginName), _member, shape: Shape);
    }
}
```

### Task 6: C# Builder — PipelineBuilder.Plugin() (READ)

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`:

```csharp
public Conditions.TypedPluginSource<string> Plugin(string pluginName, string member)
{
    Context.ValidatePluginMember(pluginName, member);
    return new Conditions.TypedPluginSource<string>(pluginName, member);
}

public Conditions.TypedPluginSource<T> Plugin<T>(string pluginName, string member)
{
    Context.ValidatePluginMember(pluginName, member);
    var declaredShape = Context.GetPluginMemberShape(pluginName, member);
    var requestedShape = PlanModel.Shape.FromClrType(typeof(T));
    if (PlanModel.ShapeCompat.Resolve(declaredShape, requestedShape) == null)
        throw new System.InvalidOperationException(
            $"Plugin '{pluginName}' member '{member}' declared as '{declaredShape.Kind}' " +
            $"but Plugin<{typeof(T).Name}>() requests '{requestedShape.Kind}'.");
    return new Conditions.TypedPluginSource<T>(pluginName, member);
}
```

### Task 6b: C# Builder — Plugin Call (ACTION)

Plugin action calls use the existing `ComponentRef` pattern. Add a `PluginRef` that emits `CallReaction`:

```csharp
// In PipelineBuilder:
public PluginRef<TModel> PluginRef(string pluginName)
{
    if (!Context.Plan.MutableTypes.ContainsKey("plugin." + pluginName))
        throw new InvalidOperationException($"Plugin '{pluginName}' is not registered.");
    return new PluginRef<TModel>(pluginName, this);
}
```

**New file:** `Alis.Reactive/Builders/PluginRef.cs`:

```csharp
namespace Alis.Reactive.Builders
{
    public sealed class PluginRef<TModel> where TModel : class
    {
        private readonly string _pluginName;
        private readonly IReactionEmitter _emitter;

        internal PluginRef(string pluginName, IReactionEmitter emitter)
        {
            _pluginName = pluginName;
            _emitter = emitter;
        }

        public void Call(string method, params ValueProducer[] args)
        {
            _emitter.BuildContext.ValidatePluginMember(_pluginName, method);
            _emitter.AddStep(Reaction.Call(
                PlanModel.PluginSource.Of(_pluginName), method,
                args.Length > 0 ? new List<ValueProducer>(args) : null));
        }
    }
}
```

**DSL usage:**
```csharp
p.PluginRef("analytics").Call("trackEvent", ValueProducer.Literal("form-submit"));
```

### Task 7: C# Builder — GatherBuilder.Plugin()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`:

```csharp
public GatherBuilder<TModel> Plugin(string pluginName, string member, string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException("HTTP parameter name required.", nameof(paramName));
    _context.ValidatePluginMember(pluginName, member);
    var shape = _context.GetPluginMemberShape(pluginName, member);
    var value = ValueProducer.ReadPlugin(pluginName, member, shape);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}
```

### Task 8: C# Builder — ReactivePlan.RegisterPlugin()

**File:** `Alis.Reactive/ReactivePlan.cs`:

```csharp
public void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new ArgumentException("Plugin name required.", nameof(pluginName));
    if (configure == null) throw new ArgumentNullException(nameof(configure));
    _context.RegisterPlugin(pluginName, configure);
}
```

### Task 9: JSON Schema — PluginSource

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

### Task 10: TS Types — PluginSource

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;

export interface PluginSource {
  kind: "plugin";
  name: string;
}
```

### Task 11: TS Runtime — Plugin registry

**New file:** `Scripts/core/plugin-registry.ts`:

```typescript
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (!name || !name.trim()) throw new Error("[alis] plugin name must not be empty or whitespace");
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

### Task 12: TS Runtime — resolver.ts

Add `case "plugin": return resolvePlugin(source.name);` to resolveSource.

Update getJsTypeForSource to handle `"plugin"` kind with `plan.types["plugin." + source.name]`.

### Task 13: TS Runtime — evaluate.ts

Widen component branch: `if (producer.from.kind === "component" || producer.from.kind === "plugin")`.

Fix error message: use source-kind-aware name (`producer.from.component` vs `(producer.from as PluginSource).name`).

### Task 13b: TS Runtime — execute.ts Call guard

Update `executeCall` guard to allow `"plugin"` alongside `"component"` and `"payload"`. `executeSet` remains unchanged — rejects plugins.

### Task 14: TS Runtime — root.ts queue drain

Add ~5 lines at top of root.ts to drain `window.__alisPlugins` before `initConfirm()`.

### Task 15: Sandbox

**sandbox-plugins.ts** (builds separately to `wwwroot/js/sandbox-plugins.js`):

```typescript
((window as any).__alisPlugins ??= []).push(
  { name: "auth", instance: { getToken: () => "sandbox-token-abc123", getUserId: () => 42, isAdmin: () => true, trackEvent: (n: string) => console.log("track:", n) } },
);
((window as any).__alisPlugins ??= []).push(
  { name: "userPrefs", instance: { theme: "dark", locale: "en-US" } },
);
```

**View:** Register ALL plugins ONCE at the top of the Razor code block:

```csharp
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);
    p.Method("getUserId", Shape.Number);
    p.Method("isAdmin", Shape.Boolean);
    p.Method("trackEvent", new List<Shape> { Shape.String });
});
plan.RegisterPlugin("userPrefs", p => {
    p.Property("theme", Shape.String);
});
```

**Section 23:** DomReady — SetText + condition with "userPrefs.theme"
- `#plugin-theme` → "dark"
- `#plugin-dark-mode` → visible

**Section 24:** Button "Send with Plugin Values" — gather token + header theme
- `#plugin-echo-token` → "sandbox-token-abc123"
- `#plugin-echo-theme` → "dark"

**Section 25:** DomReady — condition with "auth.isAdmin"
- `#plugin-admin-panel` → visible (isAdmin=true in sandbox)

**Controller:** `PluginEcho` reads token from query + theme from `X-Plugin-Theme` header.

### Task 16: C# Unit Tests (22 tests) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `register_plugin_creates_jstype_in_plan` | plan.types["plugin.auth"] exists + AssertSchemaValid |
| `plugin_property_produces_read_with_plugin_source` | from: { kind: "plugin", name: "prefs" } |
| `plugin_method_produces_read_with_shape` | shape: { kind: "string" } |
| `plugin_typed_int_carries_number_shape` | shape: { kind: "number" } |
| `plugin_typed_bool_carries_boolean_shape` | shape: { kind: "boolean" } |
| `plugin_in_condition_produces_compare` | CompareCondition with plugin read |
| `plugin_in_set_text_produces_set_reaction` | SetReaction with plugin read |
| `plugin_in_header_produces_header_value` | headers value is plugin read |
| `plugin_in_gather_carries_shape` | GatherField with shape from JsType |
| `plugin_in_route_param_composes` | route param value is plugin read |
| `plan_without_plugins_has_no_plugin_kind` | JSON does NOT contain "plugin" |
| `unregistered_plugin_throws_at_build_time` | InvalidOperationException |
| `undeclared_member_throws_at_build_time` | InvalidOperationException |
| `duplicate_register_plugin_throws` | InvalidOperationException |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_name_throws` | ArgumentException |
| `null_shape_throws` | ArgumentNullException |
| `empty_gather_param_name_throws` | ArgumentException |
| `incompatible_shape_throws_at_build_time` | String declared, int requested → throws |
| `null_configure_delegate_throws` | ArgumentNullException |
| `plugin_null_return_propagates` | null → condition .IsNull() works |
| `gather_plugin_unregistered_throws` | build-time InvalidOperationException |

### Task 17: Playwright Tests (6 tests) — `WhenPluginsProvideValues.cs`

| Test | Assert |
|---|---|
| `plugin_property_displayed_in_element` | `#plugin-theme` → "dark" |
| `plugin_condition_shows_panel_for_dark_theme` | `#plugin-dark-mode` visible |
| `plugin_gather_sends_token_to_server` | `#plugin-echo-token` has value |
| `plugin_header_reaches_server` | `#plugin-echo-theme` → "dark" |
| `plugin_gather_applies_success_class` | `#plugin-echo-result` green |
| `plugin_boolean_condition_shows_admin_panel` | `#plugin-admin-panel` visible |

### Task 18: vitest Tests (10 tests)

**`plugin-registry.test.ts` (6 tests):**

| Test | What It Proves |
|---|---|
| `registerPlugin stores and resolvePlugin retrieves` | Round-trip |
| `duplicate registration throws` | Error |
| `null instance throws` | Error |
| `whitespace name throws` | Error |
| `non-object instance throws` | Error |
| `missing plugin resolution throws` | Error |

**`evaluate-plugin.test.ts` (4 tests):**

| Test | What It Proves |
|---|---|
| `plugin property read returns value via JsType walkPath` | root.theme |
| `plugin method read calls zero-arg function` | root.getToken() |
| `plugin missing member throws` | Error |
| `getJsTypeForSource returns plugin JsType` | plan.types["plugin.name"] |

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates PluginSource (`additionalProperties: false`, name pattern `^\S+$`)
- [ ] RegisterPlugin creates JsType in plan.types
- [ ] Plugin property read works (conditions, SetText, gather, headers, route params)
- [ ] Plugin zero-arg method call works (getter pattern)
- [ ] Plugin action method call works (CallReaction via PluginRef)
- [ ] Missing plugin throws at C# build time
- [ ] Undeclared member throws at C# build time
- [ ] Incompatible shape throws at C# build time
- [ ] Duplicate C# registration throws
- [ ] Duplicate JS registration throws
- [ ] Null/empty validation on all inputs
- [ ] Shape flows from JsType through plan JSON through TS runtime
- [ ] No inline JavaScript in views
- [ ] execute.ts Set guard rejects PluginSource; Call guard allows PluginSource
- [ ] All 22 C# unit tests pass (Task 16)
- [ ] All 10 vitest tests pass (Task 18)
- [ ] All 6 Playwright tests pass (Task 17)
- [ ] All existing 808+ Playwright tests pass (no regressions)
