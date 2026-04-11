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

### Task 15: Vertical Slice — Array Manager Plugin

The sandbox proves ALL 4 DSL cases mixed with HTTP, conditions, SetText, gather, headers, route params, URL source, and component args. One plugin that does array manipulation, exercised through every framework feature.

#### Plugin TS

**File:** `Scripts/sandbox-plugins.ts` → builds to `wwwroot/js/sandbox-plugins.js`:

```typescript
interface Resident { id: number; name: string; status: string; }

const residents: Resident[] = [
  { id: 1, name: "John Doe", status: "active" },
  { id: 2, name: "Jane Smith", status: "active" },
  { id: 3, name: "Bob Johnson", status: "discharged" },
  { id: 4, name: "Alice Brown", status: "active" },
  { id: 5, name: "Charlie Wilson", status: "pending" },
];

((window as any).__alisPlugins ??= []).push({
  name: "arrayManager",
  instance: {
    // Case 4: returns value, no args
    getCount: () => residents.length,
    hasActive: () => residents.some(r => r.status === "active"),
    getFirstName: () => residents[0]?.name ?? "(empty)",

    // Case 3: returns value, with args
    getCountByStatus: (status: string) => residents.filter(r => r.status === status).length,
    getNameById: (id: number) => residents.find(r => r.id === id)?.name ?? "(not found)",
    contains: (name: string) => residents.some(r => r.name.includes(name)),

    // Case 1: void, no args
    shuffle: () => {
      for (let i = residents.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [residents[i], residents[j]] = [residents[j], residents[i]];
      }
    },

    // Case 2: void, with args
    addResident: (name: string) => { residents.push({ id: Date.now(), name, status: "active" }); },
  }
});
```

Load order in `_Layout.cshtml`:
```html
<script type="module" src="~/js/sandbox-plugins.js"></script>
<script type="module" src="~/js/alis-reactive.js"></script>
```

Add esbuild entry for sandbox-plugins in `package.json`.

#### Controller

**File:** `HttpController.cs` — add:

```csharp
[HttpGet("PluginArrayEcho")]
public IActionResult PluginArrayEcho(string? firstName, int? count, string? status) =>
    Json(new {
        receivedFirstName = firstName ?? "(none)",
        receivedCount = count,
        receivedStatus = status ?? "(none)",
        receivedHeader = Request.Headers["X-Array-Count"].FirstOrDefault() ?? "(none)"
    });

[HttpPost("PluginArrayAdd")]
public IActionResult PluginArrayAdd([FromBody] PluginAddRequest? req) =>
    Json(new { added = req?.Name ?? "(none)" });
```

**DTOs:**
```csharp
public class PluginArrayEchoResponse
{
    public string? ReceivedFirstName { get; set; }
    public int? ReceivedCount { get; set; }
    public string? ReceivedStatus { get; set; }
    public string? ReceivedHeader { get; set; }
}

public class PluginAddResponse { public string? Added { get; set; } }
public class PluginAddRequest { public string? Name { get; set; } }
```

#### View — Section 26: Plugin Array Vertical Slice

**Page URL:** `/Sandbox/HttpPipeline/Http?filterStatus=active&searchName=John`

```csharp
@{
    // ── DomReady: zero-arg reads → SetText + Conditions ──────
    Html.On(plan, t => t.DomReady(p =>
    {
        // Case 4: zero-arg read → SetText
        p.Element("arr-count").SetText(p.Plugin<int>("arrayManager", "getCount"));
        p.Element("arr-first").SetText(p.Plugin<string>("arrayManager", "getFirstName"));

        // Case 4: zero-arg read → Condition
        p.When(p.Plugin<bool>("arrayManager", "hasActive")).Truthy()
         .Then(t => t.Element("arr-has-active").Show());

        // Case 3: read with URL arg → SetText
        p.Element("arr-status-count").SetText(
            p.Plugin<int>("arrayManager", "getCountByStatus")
             .Arg(p.FromUrl("filterStatus")));

        // Case 3: read with URL arg → Condition
        p.When(p.Plugin<bool>("arrayManager", "contains")
             .Arg(p.FromUrl("searchName")))
         .Truthy()
         .Then(t => t.Element("arr-search-found").Show())
         .Else(e => e.Element("arr-search-not-found").Show());
    }));
}

<!-- Section 26a: DomReady Results -->
<native-card>
<native-card-body>
    <native-heading level="H2">26a. Plugin Reads on DomReady</native-heading>
    <div class="space-y-2 font-mono text-sm">
        <p>Count: <span id="arr-count">—</span></p>
        <p>First: <span id="arr-first">—</span></p>
        <p id="arr-has-active" hidden class="text-green-600">Has active residents ✓</p>
        <p>Active count: <span id="arr-status-count">—</span></p>
        <p id="arr-search-found" hidden class="text-green-600">Search name found ✓</p>
        <p id="arr-search-not-found" hidden class="text-amber-600">Not found</p>
    </div>
</native-card-body>
</native-card>
```

```csharp
@{
    // ── Button: GET with plugin → gather + header + URL ──────
    Html.NativeButton("arr-send-btn", "Send Array Data")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Get("/Sandbox/HttpPipeline/Http/PluginArrayEcho")
             .Gather(g => g
                 .Plugin<string>("arrayManager", "getFirstName", "firstName")
                 .Plugin<int>("arrayManager", "getCount", "count")
                 .Header("X-Array-Count", p.Plugin<int>("arrayManager", "getCount"))
                 .FromUrl("filterStatus", "status"))
             .Response(r => r
                .OnSuccess<PluginArrayEchoResponse>((json, s) =>
                {
                    s.Element("arr-echo-first").SetText(json, x => x.ReceivedFirstName);
                    s.Element("arr-echo-count").SetText(json, x => x.ReceivedCount);
                    s.Element("arr-echo-header").SetText(json, x => x.ReceivedHeader);
                    s.Element("arr-echo-status").SetText(json, x => x.ReceivedStatus);
                    s.Element("arr-echo-result").AddClass("text-green-600");
                    // Case 1: void call after success
                    s.Plugin("arrayManager", "shuffle");
                }));
        });
}

<!-- Section 26b: HTTP with Plugin Gather + Header + URL -->
<native-card>
<native-card-body>
    <native-heading level="H2">26b. Plugin → HTTP Gather + Header + URL</native-heading>
    @(Html.NativeButton("arr-send-btn", "Send Array Data")
        .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
    <div id="arr-echo-result" class="mt-3 space-y-2 font-mono text-sm text-text-muted">
        <p>First: <span id="arr-echo-first">—</span></p>
        <p>Count: <span id="arr-echo-count">—</span></p>
        <p>Header: <span id="arr-echo-header">—</span></p>
        <p>URL Status: <span id="arr-echo-status">—</span></p>
    </div>
</native-card-body>
</native-card>
```

```csharp
@{
    // ── Button: Plugin → Route Param ─────────────────────────
    Html.NativeButton("arr-route-btn", "Load Resident by Plugin Count")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Get("/Sandbox/HttpPipeline/Http/Residents/{id}")
             .Gather(g => g.RouteParam("id", p.Plugin<int>("arrayManager", "getCount")))
             .Response(r => r
                .OnSuccess<ResidentByIdResponse>((json, s) =>
                {
                    s.Element("arr-route-name").SetText(json, x => x.Name);
                    s.Element("arr-route-result").AddClass("text-green-600");
                }));
        });
}

<!-- Section 26c: Plugin → Route Param -->
<native-card>
<native-card-body>
    <native-heading level="H2">26c. Plugin → Route Param</native-heading>
    @(Html.NativeButton("arr-route-btn", "Load Resident by Plugin Count")
        .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
    <div id="arr-route-result" class="mt-3 font-mono text-sm text-text-muted">
        <p>Name: <span id="arr-route-name">—</span></p>
    </div>
</native-card-body>
</native-card>
```

```csharp
@{
    // ── Button: POST with void call + component arg ──────────
    Html.NativeButton("arr-add-btn", "Add Resident")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Post("/Sandbox/HttpPipeline/Http/PluginArrayAdd")
             .Gather(g => g.Static("name", "New Resident"))
             .Response(r => r
                .OnSuccess<PluginAddResponse>((json, s) =>
                {
                    s.Element("arr-add-result").SetText(json, x => x.Added);
                    s.Element("arr-add-section").AddClass("text-green-600");
                    // Case 2: void call with arg after HTTP success
                    s.Plugin("arrayManager", "addResident")
                     .Arg(p.Plugin<string>("arrayManager", "getFirstName"));
                }));
        });
}

<!-- Section 26d: POST + Void Call with Arg -->
<native-card>
<native-card-body>
    <native-heading level="H2">26d. HTTP POST + Void Plugin Call</native-heading>
    @(Html.NativeButton("arr-add-btn", "Add Resident")
        .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
    <div id="arr-add-section" class="mt-3 font-mono text-sm text-text-muted">
        <p>Added: <span id="arr-add-result">—</span></p>
    </div>
</native-card-body>
</native-card>
```

#### What This Proves

| Feature | Section | Element | Expected |
|---|---|---|---|
| Plugin zero-arg read → SetText | 26a | `#arr-count` | "5" |
| Plugin zero-arg read → SetText | 26a | `#arr-first` | "John Doe" |
| Plugin zero-arg read → Condition | 26a | `#arr-has-active` | visible |
| Plugin read + URL arg → SetText | 26a | `#arr-status-count` | "3" (active) |
| Plugin read + URL arg → Condition | 26a | `#arr-search-found` | visible |
| Plugin read → HTTP Gather param | 26b | `#arr-echo-first` | "John Doe" |
| Plugin read → HTTP Gather param | 26b | `#arr-echo-count` | "5" |
| Plugin read → HTTP Header | 26b | `#arr-echo-header` | "5" |
| URL source in same HTTP request | 26b | `#arr-echo-status` | "active" |
| Plugin void call (shuffle) | 26b | no errors | — |
| Plugin read → Route param | 26c | `#arr-route-name` | "Resident #5" |
| HTTP POST + void call with arg | 26d | `#arr-add-result` | "New Resident" |
| All 4 DSL cases | All | — | ✓ |

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
| `plugin_auto_registers_jstype` | plan.types["plugin.x"] created |
| `plan_without_plugins_clean` | no "plugin" in JSON |
| `empty_plugin_name_throws` | ArgumentException |
| `empty_member_throws` | ArgumentException |
| `null_arg_throws` | ArgumentNullException |
| `empty_gather_param_throws` | ArgumentException |

### Playwright Tests (10) — `WhenPluginArrayManipulates.cs`

Navigate to `/Sandbox/HttpPipeline/Http?filterStatus=active&searchName=John`.

| Test | Assert |
|---|---|
| `plugin_count_on_load` | `#arr-count` → "5" |
| `plugin_first_name_on_load` | `#arr-first` → "John Doe" |
| `plugin_has_active_condition` | `#arr-has-active` visible |
| `plugin_url_arg_status_count` | `#arr-status-count` → "3" |
| `plugin_url_arg_search_found` | `#arr-search-found` visible |
| `plugin_http_gather_echoes` | Click "Send Array Data" → `#arr-echo-first` → "John Doe", `#arr-echo-count` → "5" |
| `plugin_http_header_echoes` | Click "Send Array Data" → `#arr-echo-header` → "5" |
| `plugin_http_url_composes` | Click "Send Array Data" → `#arr-echo-status` → "active" |
| `plugin_route_param_resolves` | Click "Load Resident" → `#arr-route-name` → "Resident #5" |
| `plugin_post_then_void_call` | Click "Add Resident" → `#arr-add-result` → "New Resident" |

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
- [ ] `npm run build` — bundle builds (both alis-reactive.js and sandbox-plugins.js)
- [ ] Schema validates PluginSource
- [ ] JsType auto-created by DSL usage (methods only, WithMethod)
- [ ] `p.Plugin<T>()` read works in conditions, SetText, headers, gather, route params
- [ ] `p.Plugin<T>().Arg()` passes args via ReadProducer.args
- [ ] `p.Plugin()` void call emits CallReaction
- [ ] `p.Plugin().Arg()` void call with args
- [ ] `g.Plugin<T>()` gather carries shape from `<T>`
- [ ] Plugin read → HTTP gather → server echoes value
- [ ] Plugin read → HTTP header → server echoes value
- [ ] Plugin read → Route param → URL resolves
- [ ] Plugin read + URL source in same HTTP request
- [ ] Plugin void call fires after HTTP success
- [ ] Plugin void call with plugin-read arg
- [ ] Missing plugin throws at runtime
- [ ] Shape stays internal, ValueProducer stays internal
- [ ] execute.ts Set rejects plugin; Call allows plugin
- [ ] No inline JS in views
- [ ] All 18 C# unit tests pass
- [ ] All 8 vitest tests pass
- [ ] All 10 Playwright tests pass
- [ ] All existing 808+ Playwright tests pass
