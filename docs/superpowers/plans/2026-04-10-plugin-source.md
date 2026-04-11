# Plugin Source — User-Defined JS Objects as Value Sources

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to register arbitrary JavaScript objects as value sources in the plan. A plugin is a resolved root — just like a component (ej2 instance or DOM element) or a payload (event data). Once resolved, the same shared operations apply: read property, call method. The runtime doesn't care WHERE the root came from — it just resolves it and operates on it via JsType metadata.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver + registry

**Prerequisite:** URL Query Source plan must land first (it validates the Source union widening pattern). `formatForWire` extraction (from Headers plan Task 0) must also be done.

---

## Architecture

### The Insight

The runtime has TWO resolution paths today (Source.cs:7, resolver.ts:28-37):
- `ComponentSource` → `resolveComponent()` → DOM element → vendor root (ej2 instance or native element)
- `PayloadSource` → `resolvePayload()` → event data / response body / request payload

After URL Query Source lands, there's a third:
- `UrlSource` → `new URLSearchParams(window.location.search)`

A plugin is just ANOTHER resolution path:
- `PluginSource` → `resolvePlugin()` → user-registered JS object

Once resolved, the returned object is treated like a component root. `evaluateValue` (core/evaluate.ts:14) reads properties via JsType `properties[member]`, calls methods via JsType `methods[member]`, applies shape. Same as component reads. The only difference is HOW the root is resolved (registry instead of DOM).

### What a Plugin Can Be

```javascript
// 1. Simple object with properties
window.alisPlugins.register("userPrefs", {
  theme: "dark",
  locale: "en-US",
  timezone: "America/New_York"
});

// 2. Object with methods (zero-arg getters)
window.alisPlugins.register("auth", {
  getToken: () => localStorage.getItem("auth_token"),
  getUserId: () => JSON.parse(localStorage.getItem("user")).id,
  isAdmin: () => JSON.parse(localStorage.getItem("user")).role === "admin"
});

// 3. Side-effect only methods
window.alisPlugins.register("logger", {
  flush: () => navigator.sendBeacon("/logs", JSON.stringify(pendingLogs))
});
```

### Key Design Decisions

**1. Plugin = Named Root, Not a New Value Kind**

A plugin is NOT a new ValueProducer kind. It's a new SOURCE kind. `ValueProducer.Read(PluginSource.Of("auth"), "getToken")` uses the SAME ReadProducer (ValueProducer.cs:69-84) that component reads and URL reads use. The only difference is how the root is resolved.

**2. Plugins are NOT in plan.components**

Plugins don't have DOM elements. They're not native or fusion components. They should NOT pollute `plan.components` or widen the `Vendor` enum. Instead:
- Plugin JsTypes go directly into `plan.types` with key `"plugin." + pluginName`
- `getJsTypeForSource` (resolver.ts:101) is updated to look up `plan.types["plugin." + name]` for plugin sources
- Plugin resolution goes to the TS registry, not the DOM

This keeps components clean (DOM-only) and avoids widening the vendor enum.

**3. Plugin Registry — Global, Named, Immutable**

```typescript
// Scripts/core/plugin-registry.ts
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance) throw new Error(`[alis] plugin not found: "${name}"`);
  return instance;
}
```

Plugins are registered BEFORE boot (in a `<script>` tag). The plan references them by name. Immutable after registration prevents mid-lifecycle confusion.

**4. Properties vs Methods — Same as Components**

Plugin JsType has the same structure as component JsTypes (JsType.cs:6):
- **Properties** — `root.theme` → `readProperty(root, prop)` via `walkPath(root, prop.path)`
- **Methods (no args)** — `root.getToken()` → `callMethod(root, method, [])` via `resolveCallable(root, method.path)`

The runtime doesn't distinguish at the source level. `evaluateValue` dispatches on the member type (property vs method) found in the JsType. For plugins, the JsType is registered at C# build time via `PlanBuildContext.EnsureProperty/EnsureMethod`.

**5. C# Registration — PlanBuildContext.EnsurePluginType()**

```csharp
// In view code:
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);
    p.Method("getUserId", Shape.Number);
    p.Method("isAdmin", Shape.Boolean);
});
```

This registers a JsType at `plan.types["plugin.auth"]` with methods declared. No Component entry. No vendor. Just type metadata.

### DSL — Same Shared Concept Everywhere

```csharp
// GATHER: send plugin value as HTTP param
p.Get("/api/data")
 .Gather(g => g
     .Plugin("auth", "getToken", "Authorization")     // call auth.getToken(), send as Authorization
     .Plugin("userPrefs", "timezone", "tz")            // read userPrefs.timezone, send as tz
     .Include<FusionDropDownList, Model>(m => m.Filter))

// HEADERS: plugin value as header
p.Post("/api/save")
 .Gather(g => g
     .Header("Authorization", p.Plugin("auth", "getToken"))
     .Header("X-Session", p.Plugin("analytics", "getSessionId"))
     .Include<FusionTextBox, Model>(m => m.Name))

// CONDITIONS: branch on plugin value
p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
 .Then(t => t.Element("admin-panel").Show())

// PIPELINE: display plugin value
p.Element("timezone-display").SetText(p.Plugin("userPrefs", "timezone"))

// URL TEMPLATE: plugin value in route
p.Get("/tenants/{tenantId}/residents")
 .Gather(g => g
     .RouteParam("tenantId", p.Plugin("auth", "getUserId")))
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

The `PluginSource` carries only the plugin `name`. The `member` on ReadProducer (ValueProducer.cs:73) is the property or method to access on the resolved root. Shape flows as always.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `UrlSource` (or after `PayloadSource` if URL hasn't landed), add:

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

Unlike UrlSource (singleton), PluginSource carries a `name` — the plugin registry key. `WriteOnlyPolymorphicConverter<Source>` (Serialization/WriteOnlyPolymorphicConverter.cs:9) dispatches on runtime type — zero converter changes.

**Verification:** `dotnet build`. Serialize → `{ "kind": "plugin", "name": "auth" }`.

### Task 2: C# Plan Model — ValueProducer.ReadPlugin factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

Add after `ReadUrl` (or after `Read` at line 46):

```csharp
internal static ValueProducer ReadPlugin(string pluginName, string member, Shape shape = null) =>
    new ReadProducer(PluginSource.Of(pluginName), member, shape: shape);
```

### Task 3: C# Builder — PlanBuildContext.EnsurePluginType()

**File:** `Alis.Reactive/PlanModel/PlanBuildContext.cs`

Add after `EnsureEvent` (after line 182):

```csharp
/// <summary>
/// Ensures a plugin's JsType exists in the plan.
/// Plugin types are keyed as "plugin.{name}" in plan.types.
/// Unlike components, plugins have no DOM element and no entry in plan.components.
/// </summary>
internal string EnsurePluginType(string pluginName)
{
    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.ContainsKey(typeKey))
        _plan.MutableTypes[typeKey] = new JsType();
    return typeKey;
}

/// <summary>
/// Registers a readable property on a plugin's JsType.
/// </summary>
internal void EnsurePluginProperty(string pluginName, string memberName, Shape shape)
{
    EnsurePluginType(pluginName);
    var typeKey = "plugin." + pluginName;
    _plan.MutableTypes[typeKey].WithProperty(memberName, Path.Parse(memberName), shape, "read");
}

/// <summary>
/// Registers a callable method on a plugin's JsType.
/// </summary>
internal void EnsurePluginMethod(string pluginName, string memberName, Shape? returns = null, List<Shape>? args = null)
{
    EnsurePluginType(pluginName);
    var typeKey = "plugin." + pluginName;
    _plan.MutableTypes[typeKey].WithMethod(memberName, Path.Parse(memberName), args, returns);
}
```

**Verified references:**
- `_plan.MutableTypes` — PlanBuildContext.cs:36 (used in EnsureElement, EnsureComponent)
- `JsType.WithProperty(name, path, shape, access)` — JsType.cs:17
- `JsType.WithMethod(name, path, args, returns)` — JsType.cs:53
- `Path.Parse(string)` — used at PlanBuildContext.cs:162

### Task 4: C# Builder — TypedPluginSource<T>

**New file:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a member from a registered plugin.
    /// Returned by <c>PipelineBuilder.Plugin()</c> and <c>PipelineBuilder.Plugin&lt;T&gt;()</c>.
    /// </summary>
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

Extends `TypedSource<TProp>` (TypedSource.cs:11) — plugs into conditions, gather, pipeline, headers, route params with ZERO changes. The `Shape` property (TypedSource.cs:33) returns `Shape.FromClrType(typeof(TProp))`.

### Task 5: C# Builder — PipelineBuilder.Plugin()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

Add using (if not already present):
```csharp
using Alis.Reactive.Builders.Conditions;
```

Add after `FromUrl` methods (or after `Component` overloads at line 78):

```csharp
/// <summary>
/// Reads a named member from a registered plugin as a string.
/// </summary>
public TypedPluginSource<string> Plugin(string pluginName, string member)
{
    Context.EnsurePluginType(pluginName);
    return new TypedPluginSource<string>(pluginName, member);
}

/// <summary>
/// Reads a typed member from a registered plugin.
/// </summary>
public TypedPluginSource<T> Plugin<T>(string pluginName, string member)
{
    Context.EnsurePluginType(pluginName);
    return new TypedPluginSource<T>(pluginName, member);
}
```

Note: `Context` is accessible on PipelineBuilder (PipelineBuilder.cs:16 — `internal PlanBuildContext Context { get; }`). The `EnsurePluginType` call guarantees the JsType key `"plugin.{name}"` exists in `plan.types` even if no explicit `RegisterPlugin()` was called — the JsType starts empty and gets populated as members are used.

### Task 6: C# Builder — GatherBuilder.Plugin()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add after `FromUrl` methods (or after existing methods):

```csharp
/// <summary>
/// Includes a plugin value in the gather.
/// </summary>
public GatherBuilder<TModel> Plugin(string pluginName, string member, string paramName)
{
    var value = ValueProducer.ReadPlugin(pluginName, member);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}
```

**Verified references:**
- `ValueProducer.ReadPlugin(string, string)` — added in Task 2
- `GatherField.Of(string, ValueProducer)` — Request.cs:89-90
- `Fields` — GatherBuilder.cs:11

### Task 7: C# Builder — ReactivePlan.RegisterPlugin()

**File:** `Alis.Reactive/ReactivePlan.cs`

Add a public method for explicit plugin type registration:

```csharp
/// <summary>
/// Registers a plugin's type metadata in the plan.
/// Plugin members (properties and methods) are declared here so the runtime
/// knows how to read/call them via JsType member lookup.
/// </summary>
public void RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure)
{
    _context.EnsurePluginType(pluginName);
    var builder = new PluginTypeBuilder(_context, pluginName);
    configure(builder);
}
```

**New file:** `Alis.Reactive/Builders/PluginTypeBuilder.cs`

```csharp
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Configures a plugin's JsType members during plan construction.
    /// Used by <c>plan.RegisterPlugin("name", p => p.Property/Method(...))</c>.
    /// </summary>
    public sealed class PluginTypeBuilder
    {
        private readonly PlanBuildContext _context;
        private readonly string _pluginName;

        internal PluginTypeBuilder(PlanBuildContext context, string pluginName)
        {
            _context = context;
            _pluginName = pluginName;
        }

        /// <summary>
        /// Registers a readable property on the plugin.
        /// </summary>
        public PluginTypeBuilder Property(string name, Shape shape)
        {
            _context.EnsurePluginProperty(_pluginName, name, shape);
            return this;
        }

        /// <summary>
        /// Registers a callable method on the plugin.
        /// </summary>
        public PluginTypeBuilder Method(string name, Shape returns)
        {
            _context.EnsurePluginMethod(_pluginName, name, returns);
            return this;
        }

        /// <summary>
        /// Registers a callable method with arguments on the plugin.
        /// </summary>
        public PluginTypeBuilder Method(string name, Shape returns, List<Shape> args)
        {
            _context.EnsurePluginMethod(_pluginName, name, returns, args);
            return this;
        }
    }
}
```

### Task 8: JSON Schema — Source union gains PluginSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to Source oneOf (line 141-146, after UrlSource if it landed):

```json
"Source": {
  "oneOf": [
    { "$ref": "#/$defs/ComponentSource" },
    { "$ref": "#/$defs/PayloadSource" },
    { "$ref": "#/$defs/UrlSource" },
    { "$ref": "#/$defs/PluginSource" }
  ]
},
```

Add definition (after UrlSource definition):

```json
"PluginSource": {
  "type": "object",
  "required": ["kind", "name"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "plugin" },
    "name": { "type": "string", "minLength": 1 }
  }
},
```

### Task 9: TS Types — Source union + PluginSource

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Expand Source union (line 88, after UrlSource if it landed):

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;
```

Add interface (after UrlSource interface):

```typescript
export interface PluginSource {
  kind: "plugin";
  name: string;
}
```

### Task 10: TS Runtime — Plugin registry

**New file:** `Alis.Reactive.SandboxApp/Scripts/core/plugin-registry.ts`

```typescript
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (plugins.has(name))
    throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance)
    throw new Error(`[alis] plugin not found: "${name}"`);
  return instance;
}
```

### Task 11: TS Runtime — resolver.ts handles "plugin" kind

**File:** `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`

Add import:
```typescript
import { resolvePlugin } from "../core/plugin-registry";
```

Add to `resolveSource` switch (line 29-37):

```typescript
case "plugin":
  return resolvePlugin(source.name);
```

Update `getJsTypeForSource` (line 101-106) to handle plugin sources:

```typescript
export function getJsTypeForSource(plan: Plan, source: Source): JsType {
  switch (source.kind) {
    case "component":
      return getJsType(plan, source.component);
    case "plugin": {
      const typeKey = "plugin." + source.name;
      const jsType = plan.types[typeKey];
      if (!jsType) throw new Error(`[alis] type not found for plugin: "${source.name}"`);
      return jsType;
    }
    default:
      throw new Error(`[alis] getJsTypeForSource only supports component and plugin sources`);
  }
}
```

### Task 12: TS Runtime — evaluate.ts handles plugin reads

**File:** `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

In the `"read"` case (line 19), extend the component branch to include plugin. Change line 23:

```typescript
// Component or Plugin source: look up member in JsType
if (producer.from.kind === "component" || producer.from.kind === "plugin") {
  const jsType = getJsTypeForSource(plan, producer.from);
  const prop = jsType.properties?.[producer.member];
  if (prop) {
    const raw = resolverReadProperty(root, prop);
    return raw == null ? raw : applyShape(raw, producer.shape ?? prop.shape);
  }
  const method = jsType.methods?.[producer.member];
  if (method) {
    const raw = callMethod(root, method, []);
    return raw == null ? raw : applyShape(raw, producer.shape ?? method.returns);
  }
  const sourceName = producer.from.kind === "component"
    ? producer.from.component
    : (producer.from as PluginSource).name;
  throw new Error(`[alis] member "${producer.member}" not found on ${producer.from.kind} "${sourceName}"`);
}
```

Add PluginSource to imports:
```typescript
import type { Plan, ValueProducer, ExecContext, PluginSource } from "../types";
```

**NO new evaluate path needed.** The plugin root IS a resolved JS object. `readProperty(root, prop)` reads its properties via `walkPath(root, prop.path)` (resolver.ts:112). `callMethod(root, method, args)` calls its methods via `resolveCallable(root, method.path)` (resolver.ts:125). JsType metadata (registered at C# build time) declares which members are properties vs methods, with paths and shapes. Same as components.

### Task 13: Public API — expose registerPlugin

**File:** `Alis.Reactive.SandboxApp/Scripts/root.ts`

Add import and expose:

```typescript
import { registerPlugin } from "./core/plugin-registry";

// Expose to global scope for script-tag registration
(window as any).alisPlugins = { register: registerPlugin };
```

Usage in HTML:
```html
<script>
  window.alisPlugins.register("auth", {
    getToken: () => localStorage.getItem("auth_token"),
    getUserId: () => JSON.parse(localStorage.getItem("user")).id
  });
</script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Plugins must be registered BEFORE alis-reactive.js boots. The plan references them by name — if not registered, the runtime throws `plugin not found`.

---

## Composition Examples

### Plugin + Headers + URL Templates

```csharp
p.Get("/tenants/{tenantId}/residents")
 .Gather(g => g
     .RouteParam("tenantId", p.Plugin("auth", "getUserId"))
     .Header("Authorization", p.Plugin("auth", "getToken"))
     .FromUrl("facilityId")
     .Include<FusionDropDownList, Model>(m => m.Status))
 .Response(r => r.OnSuccess(...))
```

All four value sources in one request:
- Route param from plugin (`auth.getUserId`)
- Header from plugin (`auth.getToken`)
- Query param from URL (`?facilityId`)
- Body param from component (`Status` dropdown)

All expressed as ValueProducers. All evaluated via `evaluateValue`. All shaped.

### Plugin in Conditions

```csharp
p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
 .Then(t => {
     t.Element("admin-section").Show();
     t.Element("admin-badge").SetText("Administrator");
 })
 .Else(e => {
     t.Element("admin-section").Hide();
 });
```

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates plans with PluginSource
- [ ] Plugin registration before boot works (`window.alisPlugins.register`)
- [ ] Plugin property read works in gather, conditions, pipeline
- [ ] Plugin no-arg method call works (JsType method with path)
- [ ] Missing plugin throws clear error (`plugin not found: "name"`)
- [ ] Duplicate registration throws (`plugin "name" already registered`)
- [ ] Plugin composes with headers, URL templates, URL query source
- [ ] Shape flows correctly (plugin returning date → ISO conversion via formatForWire)
- [ ] Null return from plugin method propagates correctly
- [ ] All unit + Playwright tests pass
