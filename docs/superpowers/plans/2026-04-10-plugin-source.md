# Plugin Source — User-Defined JS Objects as Value Sources

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to register arbitrary JavaScript objects as value sources in the plan. A plugin is a resolved root — just like a component (ej2 instance or DOM element). Once resolved, the same shared operations apply: read property, call zero-arg method. The runtime doesn't care WHERE the root came from — it resolves it via the registry and operates on it via JsType metadata.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver + registry

**Prerequisite:** URL Query Source must be landed (Source union already widened to 3 kinds, execute.ts Set/Call guards in place).

---

## Architecture

### The Insight

After URL Query Source, the runtime has THREE resolution paths:
- `ComponentSource` → DOM element → vendor root (ej2 instance or HTMLElement)
- `PayloadSource` → execution context (event, response, request)
- `UrlSource` → `new URLSearchParams(window.location.search)`

A plugin is a FOURTH resolution path:
- `PluginSource` → registry lookup → user-registered JS object

Once resolved, the returned object is treated like a component root. `evaluateValue` reads properties via `jsType.properties[member]` and calls zero-arg methods via `jsType.methods[member]`. Same JsType member lookup as components. The only difference is HOW the root is resolved.

### Two-Part Registration

Plugins require registration on BOTH sides:

**C# side (plan JSON):** Declares the plugin's capabilities — which members exist, what shapes they have. Goes into `plan.types["plugin.{name}"]` as a JsType. This is compile-time metadata.

```csharp
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);      // zero-arg getter → returns string
    p.Method("getUserId", Shape.Number);     // zero-arg getter → returns number
    p.Property("theme", Shape.String);       // direct property read
});
```

**JS side (runtime):** Registers the actual JS object with functions. This CANNOT be in plan JSON (functions aren't serializable). Devs create their own module — NO inline JS, NO globals.

```typescript
// site-plugins.ts (developer's own file — NOT inline, NOT in views)
import { registerPlugin } from "./alis-reactive";

registerPlugin("auth", {
  getToken: () => localStorage.getItem("auth_token"),
  getUserId: () => JSON.parse(localStorage.getItem("user")!).id,
  theme: "dark"
});
```

```html
<!-- In _Layout.cshtml or the view — NO inline JavaScript -->
<script type="module" src="~/js/site-plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
@Html.RenderPlan(plan)
```

**Why two parts?** The plan is the contract (Rule 1). The plan declares WHAT the plugin has (JsType metadata). The runtime provides the actual object. If the JS object doesn't match the declared JsType, the runtime fails fast (member not found → throw).

### RegisterPlugin Is Mandatory

`RegisterPlugin` must be called before any `p.Plugin(...)` reference. The builder validates that the requested member exists on the registered JsType. If not:
- Plugin not registered → throw at build time
- Member not declared → throw at build time

This follows the framework's principle: the plan carries ALL information. No runtime discovery.

### Zero-Arg Methods Only

`evaluateValue` calls methods with `[]` (zero args):
```typescript
const raw = callMethod(root, method, []);
```

Plugin methods are zero-arg getters: `getToken()`, `getUserId()`, `isAdmin()`. No arg-bearing methods. This matches the evaluateValue contract. The `PluginTypeBuilder.Method` overload accepts only `(name, returns)` — no args parameter.

### Plugins Are NOT Mutation Targets

`execute.ts` already guards Set/Call reactions against non-component/non-payload sources (from URL Query Source work). `PluginSource` is rejected with an explicit throw. Plugins are read-only value sources.

### Null Semantics

Plugin properties/methods CAN return null. This flows through the existing `raw == null ? raw : applyShape(...)` pattern. Conditions `.IsNull()` and `.NotNull()` work correctly. This is the same as component reads — null is data, not an error.

### Source Resolution Summary

| Source | Resolution | Member Access | JsType? |
|---|---|---|---|
| ComponentSource | DOM → vendor root | JsType walkPath | Yes |
| PayloadSource | ExecContext scope | walk() dot-path | No |
| UrlSource | URLSearchParams | params.get(member) | No |
| **PluginSource** | **Registry lookup** | **JsType walkPath** (same as component) | **Yes** |

### DSL

```csharp
// Register plugin capabilities at plan construction time
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);
    p.Method("getUserId", Shape.Number);
    p.Method("isAdmin", Shape.Boolean);
});

plan.RegisterPlugin("userPrefs", p => {
    p.Property("theme", Shape.String);
    p.Property("locale", Shape.String);
});

// USE in headers
p.Post("/api/save")
 .Gather(g => g
     .Header("Authorization", p.Plugin<string>("auth", "getToken"))
     .Include<FusionTextBox, Model>(m => m.Name))

// USE in conditions
p.When(p.Plugin<bool>("auth", "isAdmin")).Truthy()
 .Then(t => t.Element("admin-panel").Show())

// USE in pipeline
p.Element("theme-display").SetText(p.Plugin<string>("userPrefs", "theme"))

// USE in gather
p.Get("/api/data")
 .Gather(g => g.Plugin("auth", "getToken", "token"))

// USE in route params
p.Get("/tenants/{tenantId}/residents")
 .Gather(g => g.RouteParam("tenantId", p.Plugin<int>("auth", "getUserId")))
```

### Plan JSON

Plugin read:
```json
{
  "kind": "read",
  "from": { "kind": "plugin", "name": "auth" },
  "member": "getToken",
  "shape": { "kind": "string" }
}
```

Plugin JsType in plan.types:
```json
{
  "plugin.auth": {
    "methods": {
      "getToken": { "path": [{ "kind": "property", "name": "getToken" }], "returns": { "kind": "string" } },
      "getUserId": { "path": [{ "kind": "property", "name": "getUserId" }], "returns": { "kind": "number" } }
    }
  }
}
```

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add PluginSource

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `UrlSource`, add:

```csharp
/// <summary>Reads a value from a user-registered JS plugin object.
/// Carries the plugin name — the registry key for resolution.</summary>
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

Unlike UrlSource (singleton), PluginSource carries a `name` — the registry key.

### Task 2: C# Plan Model — ValueProducer.ReadPlugin factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

```csharp
/// <summary>Creates a ReadProducer for a plugin member. Shape comes from JsType registration.</summary>
internal static ValueProducer ReadPlugin(string pluginName, string member, Shape shape = null) =>
    Read(PluginSource.Of(pluginName), member, shape: shape);
```

### Task 3: C# Builder — PlanBuildContext plugin methods

**File:** `Alis.Reactive/PlanModel/PlanBuildContext.cs`

Add after existing `EnsureEvent`:

```csharp
private readonly HashSet<string> _registeredPlugins = new HashSet<string>();

/// <summary>Registers a plugin's JsType. Throws on duplicate registration.</summary>
internal void RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure)
{
    if (!_registeredPlugins.Add(pluginName))
        throw new InvalidOperationException(
            $"Plugin '{pluginName}' is already registered. Each plugin can only be registered once.");
    var typeKey = "plugin." + pluginName;
    _plan.MutableTypes[typeKey] = new JsType();
    var builder = new PluginTypeBuilder(this, pluginName, typeKey);
    configure(builder);
}

/// <summary>Validates a plugin member exists on its JsType. Throws if not registered.</summary>
internal void ValidatePluginMember(string pluginName, string member)
{
    var typeKey = "plugin." + pluginName;
    if (!_plan.MutableTypes.TryGetValue(typeKey, out var jsType))
        throw new InvalidOperationException(
            $"Plugin '{pluginName}' is not registered. Call plan.RegisterPlugin(\"{pluginName}\", ...) first.");
    var hasProp = jsType.Properties?.ContainsKey(member) == true;
    var hasMethod = jsType.Methods?.ContainsKey(member) == true;
    if (!hasProp && !hasMethod)
        throw new InvalidOperationException(
            $"Member '{member}' is not declared on plugin '{pluginName}'. " +
            $"Register it via .Property(\"{member}\", shape) or .Method(\"{member}\", shape) in RegisterPlugin.");
}

/// <summary>Gets the shape of a plugin member from its JsType.</summary>
internal Shape GetPluginMemberShape(string pluginName, string member)
{
    var typeKey = "plugin." + pluginName;
    var jsType = _plan.MutableTypes[typeKey];
    var prop = jsType.Properties?.GetValueOrDefault(member);
    if (prop != null) return prop.Shape;
    var method = jsType.Methods?.GetValueOrDefault(member);
    if (method?.Returns != null) return method.Returns;
    return Shape.Any;
}
```

### Task 4: C# Builder — PluginTypeBuilder

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
        private readonly string _typeKey;

        internal PluginTypeBuilder(PlanBuildContext context, string pluginName, string typeKey)
        {
            _context = context;
            _pluginName = pluginName;
            _typeKey = typeKey;
        }

        /// <summary>Declares a readable property on the plugin.</summary>
        public PluginTypeBuilder Property(string name, Shape shape)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Plugin property name must not be null or whitespace.", nameof(name));
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            _context.Plan.MutableTypes[_typeKey]
                .WithProperty(name, Path.Parse(name), shape, "read");
            return this;
        }

        /// <summary>Declares a zero-arg method on the plugin. The method is called with no arguments
        /// and its return value is used as the read result.</summary>
        public PluginTypeBuilder Method(string name, Shape returns)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Plugin method name must not be null or whitespace.", nameof(name));
            if (returns == null) throw new System.ArgumentNullException(nameof(returns));
            _context.Plan.MutableTypes[_typeKey]
                .WithMethod(name, Path.Parse(name), returns: returns);
            return this;
        }
    }
}
```

**No args overload.** Zero-arg methods only — matches evaluateValue contract.

### Task 5: C# Builder — TypedPluginSource<T>

**New file:** `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a member from a registered plugin.
    /// Returned by <c>PipelineBuilder.Plugin()</c>.
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

### Task 6: C# Builder — PipelineBuilder.Plugin()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

```csharp
/// <summary>
/// Reads a member from a registered plugin as a string.
/// The plugin must be registered via <c>plan.RegisterPlugin()</c> first.
/// </summary>
public Conditions.TypedPluginSource<string> Plugin(string pluginName, string member)
{
    Context.ValidatePluginMember(pluginName, member);
    return new Conditions.TypedPluginSource<string>(pluginName, member);
}

/// <summary>
/// Reads a typed member from a registered plugin.
/// Shape is inferred from T and must be scalar.
/// </summary>
public Conditions.TypedPluginSource<T> Plugin<T>(string pluginName, string member)
{
    Context.ValidatePluginMember(pluginName, member);
    return new Conditions.TypedPluginSource<T>(pluginName, member);
}
```

**Build-time validation:** `ValidatePluginMember` checks the JsType exists AND the member is declared. Missing plugin → throw. Missing member → throw.

### Task 7: C# Builder — GatherBuilder.Plugin()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

```csharp
/// <summary>Includes a plugin value in the gather. Shape comes from JsType registration.</summary>
public GatherBuilder<TModel> Plugin(string pluginName, string member, string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException("HTTP parameter name must not be null or whitespace.", nameof(paramName));
    // Shape from JsType — not dropped, not guessed
    var shape = _context.GetPluginMemberShape(pluginName, member);
    var value = ValueProducer.ReadPlugin(pluginName, member, shape);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}
```

**Shape preserved:** `GetPluginMemberShape` reads shape from the JsType registered at build time. No shape drop.

### Task 8: C# Builder — ReactivePlan.RegisterPlugin()

**File:** `Alis.Reactive/ReactivePlan.cs`

```csharp
/// <summary>
/// Registers a plugin's type metadata in the plan.
/// Plugin members (properties and methods) are declared here so the runtime
/// knows how to read/call them via JsType member lookup.
/// Must be called before any <c>p.Plugin()</c> reference to the same plugin.
/// </summary>
public void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
{
    if (string.IsNullOrWhiteSpace(pluginName))
        throw new ArgumentException("Plugin name must not be null or whitespace.", nameof(pluginName));
    _context.RegisterPlugin(pluginName, configure);
}
```

### Task 9: JSON Schema — Source union gains PluginSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to Source oneOf (after UrlSource):
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
},
```

### Task 10: TS Types — Source union + PluginSource

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Expand Source union:
```typescript
export type Source = ComponentSource | PayloadSource | UrlSource | PluginSource;
```

Add interface:
```typescript
export interface PluginSource {
  kind: "plugin";
  name: string;
}
```

### Task 11: TS Runtime — Plugin registry

**New file:** `Alis.Reactive.SandboxApp/Scripts/core/plugin-registry.ts`

```typescript
const plugins = new Map<string, unknown>();

export function registerPlugin(name: string, instance: unknown): void {
  if (!name) throw new Error("[alis] plugin name must not be empty");
  if (instance == null) throw new Error(`[alis] plugin "${name}" instance must not be null`);
  if (typeof instance !== "object") throw new Error(`[alis] plugin "${name}" must be an object, got ${typeof instance}`);
  if (plugins.has(name)) throw new Error(`[alis] plugin "${name}" already registered`);
  plugins.set(name, instance);
}

export function resolvePlugin(name: string): unknown {
  const instance = plugins.get(name);
  if (!instance) throw new Error(`[alis] plugin not found: "${name}"`);
  return instance;
}
```

Validates: non-empty name, non-null instance, object type, no duplicates.

### Task 12: TS Runtime — resolver.ts handles "plugin" kind

**File:** `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`

Add to `resolveSource` switch:
```typescript
case "plugin":
  return resolvePlugin(source.name);
```

Import: `import { resolvePlugin } from "../core/plugin-registry";`

Update `getJsTypeForSource`:
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

### Task 13: TS Runtime — evaluate.ts handles plugin reads

**File:** `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

Extend the component branch:
```typescript
if (producer.from.kind === "component" || producer.from.kind === "plugin") {
  const jsType = getJsTypeForSource(plan, producer.from);
  // ... existing property/method lookup — NO CHANGES
}
```

The plugin root IS a resolved JS object. Same walkPath, same callMethod. JsType metadata declares which members are properties vs methods. Zero change to the read logic.

### Task 14: TS Runtime — Export registerPlugin from root.ts

**File:** `Alis.Reactive.SandboxApp/Scripts/root.ts`

```typescript
export { registerPlugin } from "./core/plugin-registry";
```

Devs import this in their own module. NO `window.alisPlugins` global. NO inline script.

### Task 15: Sandbox — Controller + DTOs

**File:** `HttpController.cs` — add:
```csharp
[HttpGet("PluginEcho")]
public IActionResult PluginEcho(string? token, string? theme) =>
    Json(new { receivedToken = token ?? "(none)", receivedTheme = theme ?? "(none)" });
```

**File:** `HttpShowcaseModel.cs` — add:
```csharp
public class PluginEchoResponse
{
    public string? ReceivedToken { get; set; }
    public string? ReceivedTheme { get; set; }
}
```

### Task 15b: Sandbox View — Sections 23-25

**Note:** The sandbox page must include a plugin registration script. Since this is a SANDBOX (testing tool, not production), a small inline bootstrap is acceptable for demonstration. Production apps use a separate module.

#### Section 23: Plugin Read in SetText + Condition

**DomReady (no click):**
```csharp
plan.RegisterPlugin("userPrefs", p => {
    p.Property("theme", Shape.String);
});

Html.On(plan, t => t.DomReady(p =>
{
    p.Element("plugin-theme").SetText(p.Plugin<string>("userPrefs", "theme"));

    p.When(p.Plugin<string>("userPrefs", "theme")).Eq("dark")
     .Then(then => then.Element("plugin-dark-mode").Show());
}));
```

**Element IDs:**
- `plugin-theme` — expect: "dark"
- `plugin-dark-mode` — expect: visible (theme is "dark")

#### Section 24: Plugin Read in Gather + Header

**Button:** "Send with Plugin Values"
```csharp
plan.RegisterPlugin("auth", p => {
    p.Method("getToken", Shape.String);
});

// On button click:
p.Get("/Sandbox/HttpPipeline/Http/PluginEcho")
 .Gather(g => g
     .Plugin("auth", "getToken", "token")
     .Header("X-Plugin-Theme", p.Plugin<string>("userPrefs", "theme")))
 .Response(r => r.OnSuccess<PluginEchoResponse>((json, s) =>
 {
     s.Element("plugin-echo-token").SetText(json, x => x.ReceivedToken);
     s.Element("plugin-echo-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `plugin-echo-token` — expect: the token value from the plugin
- `plugin-echo-result` — success class

#### Section 25: Plugin in Condition (auth gate)

**DomReady:**
```csharp
plan.RegisterPlugin("authGate", p => {
    p.Method("isAdmin", Shape.Boolean);
});

Html.On(plan, t => t.DomReady(p =>
{
    p.When(p.Plugin<bool>("authGate", "isAdmin")).Truthy()
     .Then(then => then.Element("plugin-admin-panel").Show())
     .Else(els => els.Element("plugin-admin-panel").Hide());
}));
```

**Element IDs:**
- `plugin-admin-panel` — visible if isAdmin returns true, hidden otherwise

### Task 16: C# Unit Tests (18 tests) — `WhenUsingPlugins.cs`

| Test | What It Proves |
|---|---|
| `register_plugin_creates_jstype_in_plan` | RegisterPlugin → plan.types["plugin.auth"] exists + AssertSchemaValid |
| `plugin_property_produces_read_with_url_source_kind` | `p.Plugin<string>("prefs", "theme")` → `{ kind: "read", from: { kind: "plugin", name: "prefs" }, member: "theme" }` |
| `plugin_method_produces_read_with_shape` | `p.Plugin<string>("auth", "getToken")` → shape: { kind: "string" } |
| `plugin_typed_int_carries_number_shape` | `p.Plugin<int>("auth", "getUserId")` → shape: { kind: "number" } |
| `plugin_typed_bool_carries_boolean_shape` | `p.Plugin<bool>("auth", "isAdmin")` → shape: { kind: "boolean" } |
| `plugin_in_condition_produces_compare` | `When(p.Plugin<bool>("auth", "isAdmin")).Truthy()` → CompareCondition |
| `plugin_in_set_text_produces_set_reaction` | `Element("x").SetText(p.Plugin<string>("prefs", "theme"))` → SetReaction |
| `plugin_in_header_produces_header_value` | `Header("X-Token", p.Plugin<string>("auth", "getToken"))` → headers value is plugin read |
| `plugin_in_gather_carries_shape` | `.Plugin("auth", "getToken", "token")` → GatherField with shape from JsType |
| `plugin_in_route_param_composes` | `RouteParam("id", p.Plugin<int>("auth", "getUserId"))` → route param value is plugin read |
| `plan_without_plugins_has_no_plugin_kind` | Normal plan → JSON does NOT contain `"kind": "plugin"` |
| `unregistered_plugin_throws_at_build_time` | `p.Plugin<string>("unknown", "member")` → InvalidOperationException |
| `undeclared_member_throws_at_build_time` | RegisterPlugin("auth", p => p.Method("getToken", Shape.String)); then `p.Plugin<string>("auth", "missingMember")` → throws |
| `duplicate_register_plugin_throws` | RegisterPlugin("auth", ...) twice → InvalidOperationException |
| `empty_plugin_name_throws` | RegisterPlugin("", ...) → ArgumentException |
| `empty_member_name_throws` | p.Method("", Shape.String) → ArgumentException |
| `null_shape_throws` | p.Method("getToken", null) → ArgumentNullException |
| `empty_gather_param_name_throws` | g.Plugin("auth", "getToken", "") → ArgumentException |

### Task 17: Playwright Tests (6 tests) — `WhenPluginsProvideValues.cs`

Navigate to `/Sandbox/HttpPipeline/Http` (sandbox must include plugin registration script).

| Test | Assert |
|---|---|
| `plugin_property_displayed_in_element` | `#plugin-theme` → "dark" |
| `plugin_condition_shows_panel_for_dark_theme` | `#plugin-dark-mode` is visible |
| `plugin_gather_sends_token_to_server` | Click "Send with Plugin Values" → `#plugin-echo-token` has value |
| `plugin_gather_applies_success_class` | `#plugin-echo-result` has class `text-green-600` |
| `plugin_boolean_condition_shows_admin_panel` | `#plugin-admin-panel` is visible (if isAdmin=true) |
| `plugin_boolean_condition_hides_when_false` | Navigate with isAdmin=false plugin → `#plugin-admin-panel` hidden |

### Task 18: vitest Tests (4 tests) — `core/plugin-registry.test.ts`

| Test | What It Proves |
|---|---|
| `registerPlugin stores and resolvePlugin retrieves` | Basic round-trip |
| `duplicate registration throws` | Second call with same name → Error |
| `null instance throws` | registerPlugin("x", null) → Error |
| `missing plugin throws` | resolvePlugin("unknown") → Error |

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates plans with PluginSource (`additionalProperties: false`)
- [ ] RegisterPlugin creates JsType in plan.types with correct key
- [ ] Plugin property read works in conditions, SetText, gather, headers, route params
- [ ] Plugin zero-arg method call works (JsType method with path)
- [ ] Missing plugin throws at C# build time (not runtime)
- [ ] Undeclared member throws at C# build time (not runtime)
- [ ] Duplicate C# registration throws
- [ ] Duplicate JS registration throws
- [ ] Null/empty plugin name/member throws
- [ ] Shape flows from JsType through plan JSON through TS runtime
- [ ] No inline JavaScript in views (module import only)
- [ ] execute.ts Set/Call guards reject PluginSource (from URL Query Source)
- [ ] All 18 C# unit tests pass (Task 16)
- [ ] All 4 vitest tests pass (Task 18)
- [ ] All 6 Playwright tests pass (Task 17)
- [ ] All existing 808+ Playwright tests pass (no regressions)
