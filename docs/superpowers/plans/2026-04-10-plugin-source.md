# Plugin Source — User-Defined JS Objects as Value Sources

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to register arbitrary JavaScript objects/functions as value sources in the plan. A plugin is a resolved root — just like a component (ej2 instance), an element (DOM node), or the URL (URLSearchParams). Once resolved, the same shared operations apply: read property, call method, walk path. The runtime doesn't care WHERE the root came from — it just resolves it and operates on it.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver + registry

---

## Architecture

### The Insight

The runtime has THREE resolution paths today:
- `ComponentSource` → `resolveComponent()` → DOM element → vendor root (ej2 instance or native element)
- `PayloadSource` → `resolvePayload()` → event data / response body / request payload
- `UrlSource` → `resolveUrl()` → URLSearchParams

A plugin is just ANOTHER resolution path:
- `PluginSource` → `resolvePlugin()` → user-registered JS object

Once resolved, the returned object is treated like any other root. `evaluateValue` reads properties via `jsType.properties[member]`, calls methods via `jsType.methods[member]`, applies shape. No special handling, no walking, no auto-detection. The plugin IS a component without DOM — same JsType, same resolution path, different source.

### What a Plugin Can Be

```javascript
// 1. Simple object with properties
window.alisPlugins.register("userPrefs", {
  theme: "dark",
  locale: "en-US",
  timezone: "America/New_York"
});

// 2. Object with methods (with or without args)
window.alisPlugins.register("analytics", {
  getSessionId: () => crypto.randomUUID(),
  getPageLoadTime: () => performance.now(),
  trackEvent: (name, data) => { /* fire and forget */ }
});

// 3. Factory that returns a value
window.alisPlugins.register("auth", {
  getToken: () => localStorage.getItem("auth_token"),
  getUserId: () => JSON.parse(localStorage.getItem("user")).id,
  isAdmin: () => JSON.parse(localStorage.getItem("user")).role === "admin"
});

// 4. No-arg, no-return (side effect only)
window.alisPlugins.register("logger", {
  flush: () => navigator.sendBeacon("/logs", JSON.stringify(pendingLogs))
});
```

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

// PIPELINE: call plugin method (fire and forget, no return)
p.Plugin("analytics").Call("trackEvent", ValueProducer.Literal("form-submit"))

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

The `PluginSource` carries only the plugin `name`. The `member` on ReadProducer is the property or method to access on the resolved root. Shape flows as always.

For method calls with arguments:

```json
{
  "kind": "call",
  "on": { "kind": "plugin", "name": "analytics" },
  "method": "trackEvent",
  "args": [{ "kind": "literal", "value": "form-submit" }]
}
```

This uses the existing `CallReaction` shape — same as component method calls. The only change is the source kind.

---

## Key Design Decisions

### 1. Plugin = Named Root, Not a New Value Kind

A plugin is NOT a new ValueProducer kind. It's a new SOURCE kind. `ValueProducer.Read(PluginSource.Of("auth"), "getToken")` uses the SAME ReadProducer that component reads and URL reads use. The only difference is how the root is resolved.

### 2. Plugin Registry — Global, Named, Immutable After Registration

```typescript
// Runtime registry
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance) throw new Error(`[alis] plugin not found: ${name}`);
  return instance;
}
```

Plugins are registered BEFORE boot (in a `<script>` tag or module). The plan references them by name. The runtime resolves them from the registry. Immutable after registration prevents mid-lifecycle confusion.

### 3. Properties AND Methods — Same as Components

A plugin root is a JS object. It can have:
- **Properties** — `root.theme` → read via `walk(root, "theme")`
- **Methods (no args)** — `root.getToken()` → call via `callMethod(root, method, [])`
- **Methods (with args)** — `root.trackEvent("click", data)` → call via `callMethod(root, method, args)`
- **Nested objects** — `root.config.api.baseUrl` → walk via path segments

The runtime doesn't distinguish. `evaluateValue` dispatches on the member type (property vs method) found on the JsType. For plugins, the JsType is registered at C# build time via `EnsureProperty` / `EnsureMethod`.

### 4. JsType Registration for Plugins

```csharp
// C# DSL — register plugin's readable properties and callable methods
Html.Plugin<AuthPlugin>("auth", plugin => {
    plugin.Property("getToken", Shape.String);      // method that returns string
    plugin.Property("getUserId", Shape.Number);      // method that returns number
    plugin.Property("isAdmin", Shape.Boolean);       // method that returns boolean
});
```

This registers the plugin's JsType in the plan — same as component onboarding. The plan carries the type metadata. The runtime uses it to resolve properties and methods.

### 5. No Vendor — Plugins Are Vendor-Agnostic

Plugins don't have a vendor ("native" or "fusion"). They're pure JS objects. The resolver dispatches on `source.kind === "plugin"` and returns the registered object directly — no vendor root resolution.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `UrlSource`, add:

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

Unlike UrlSource (singleton), PluginSource carries a `name` — the plugin registry key.

### Task 2: C# Plan Model — ValueProducer.ReadPlugin factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

```csharp
internal static ValueProducer ReadPlugin(string pluginName, string member, Shape shape = null) =>
    new ReadProducer(PluginSource.Of(pluginName), member, shape: shape);
```

### Task 3: C# Builder — TypedPluginSource<T>

**New file:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`

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

Extends `TypedSource<TProp>` — plugs into conditions, gather, pipeline with ZERO changes.

### Task 4: C# Builder — PipelineBuilder.Plugin()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

```csharp
/// <summary>
/// Reads a named member from a registered plugin.
/// </summary>
public TypedPluginSource<string> Plugin(string pluginName, string member)
{
    return new TypedPluginSource<string>(pluginName, member);
}

/// <summary>
/// Reads a typed member from a registered plugin.
/// </summary>
public TypedPluginSource<T> Plugin<T>(string pluginName, string member)
{
    return new TypedPluginSource<T>(pluginName, member);
}
```

### Task 5: C# Builder — GatherBuilder.Plugin()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

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

### Task 6: JSON Schema — Source union gains PluginSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

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
    "name": { "type": "string", "minLength": 1 }
  }
}
```

### Task 7: TS Types — Source union + PluginSource

**File:** `Scripts/types/plan.ts`

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;

export interface PluginSource {
  kind: "plugin";
  name: string;
}
```

### Task 8: TS Runtime — Plugin registry

**New file:** `Scripts/core/plugin-registry.ts`

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

Export `registerPlugin` from root.ts so devs can call `window.alisPlugins.register()` or import it.

### Task 9: TS Runtime — resolver.ts handles "plugin" kind

**File:** `Scripts/resolution/resolver.ts`

Add to `resolveSource` switch:

```typescript
case "plugin":
  return resolvePlugin(source.name);
```

Import `resolvePlugin` from `../core/plugin-registry`.

### Task 10: TS Runtime — evaluate.ts handles plugin reads

**File:** `Scripts/core/evaluate.ts`

**NO new evaluate path needed.** A plugin is a component without DOM. After `resolveSource` returns the plugin object, the EXISTING component read path handles everything:

```typescript
if (producer.from.kind === "component" || producer.from.kind === "plugin") {
  const jsType = getJsTypeForSource(plan, producer.from);
  const prop = jsType.properties?.[producer.member];
  if (prop) {
    const raw = readProperty(root, prop);
    return raw == null ? raw : applyShape(raw, producer.shape ?? prop.shape);
  }
  const method = jsType.methods?.[producer.member];
  if (method) {
    const raw = callMethod(root, method, []);
    return raw == null ? raw : applyShape(raw, producer.shape ?? method.returns);
  }
  throw new Error(`[alis] member "${producer.member}" not found on ${producer.from.kind} "${(producer.from as any).name ?? (producer.from as any).component}"`);
}
```

The plugin root IS the resolved object. `readProperty(root, prop)` reads its properties. `callMethod(root, method, args)` calls its methods. JsType metadata (registered at C# build time) declares which members are properties vs methods, with paths and shapes. Same as components. No walking, no auto-detection, no guessing.

**`getJsTypeForSource` needs updating** to handle plugin sources — look up the JsType by `"plugin." + source.name` (same pattern as `vendor + "." + componentId` for components).

### Task 11: Public API — expose registerPlugin

**File:** `Scripts/root.ts`

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
     e.Element("admin-section").Hide();
 });
```

### Plugin as Event Side Effect

```csharp
// After form save, track analytics
p.Post("/api/save", g => g.IncludeAll())
 .Response(r => r.OnSuccess(s => {
     s.Element("status").SetText("Saved");
     s.Dispatch("form-saved");
 }));

// Listen for save event, call analytics plugin
Html.On(plan, t => t.CustomEvent("form-saved", p => {
    p.Plugin("analytics").Call("trackEvent",
        ValueProducer.Literal("form-saved"));
}));
```

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates plans with PluginSource
- [ ] Plugin registration before boot works (`window.alisPlugins.register`)
- [ ] Plugin property read works in gather, conditions, pipeline
- [ ] Plugin no-arg method call works (auto-detected as function)
- [ ] Plugin method with args works via CallReaction
- [ ] Missing plugin throws clear error (`plugin not found: "name"`)
- [ ] Duplicate registration throws (`plugin "name" already registered`)
- [ ] Plugin composes with headers, URL templates, URL query source
- [ ] Shape flows correctly (plugin returning date → ISO conversion)
- [ ] Null return from plugin method propagates correctly (null check works)
- [ ] All unit + Playwright tests pass
