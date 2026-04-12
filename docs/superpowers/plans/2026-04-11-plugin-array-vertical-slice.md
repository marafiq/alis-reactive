# Plugin Vertical Slice — Array Manipulation

> Proves Plugin DSL works with every framework feature: conditions, SetText, gather, headers, route params, URL source, chained, parallel.

---

## The Plugin: `arrayManager`

Manages a client-side array of residents. Exposes scalar results to the DSL — all methods, no properties.

```typescript
// plugins/array-manager.ts → bundled to sandbox-plugins.js
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
    formatSummary: (active: number, total: number) => `${active} of ${total} residents active`,
    contains: (name: string) => residents.some(r => r.name.includes(name)),
    
    // Case 1: void, no args
    shuffle: () => { for (let i = residents.length - 1; i > 0; i--) { const j = Math.floor(Math.random() * (i + 1)); [residents[i], residents[j]] = [residents[j], residents[i]]; } },
    
    // Case 2: void, with args
    addResident: (name: string, status: string) => { residents.push({ id: Date.now(), name, status }); },
    removeById: (id: number) => { const idx = residents.findIndex(r => r.id === id); if (idx >= 0) residents.splice(idx, 1); },
  }
});
```

---

## Controller

**File:** `Areas/Sandbox/Controllers/HttpPipeline/HttpController.cs` — add:

```csharp
[HttpGet("PluginArrayEcho")]
public IActionResult PluginArrayEcho(string? firstName, int? count, string? summary) =>
    Json(new {
        receivedFirstName = firstName ?? "(none)",
        receivedCount = count,
        receivedSummary = summary ?? "(none)",
        receivedHeader = Request.Headers["X-Array-Count"].FirstOrDefault() ?? "(none)"
    });
```

**DTO:**
```csharp
public class PluginArrayEchoResponse
{
    public string? ReceivedFirstName { get; set; }
    public int? ReceivedCount { get; set; }
    public string? ReceivedSummary { get; set; }
    public string? ReceivedHeader { get; set; }
}
```

---

## View — Section 26: Plugin Array Manipulation

All 4 cases + composition with every DSL feature:

```csharp
@{
    // ── DomReady: read plugin values into DOM ──────────────
    Html.On(plan, t => t.DomReady(p =>
    {
        // Case 4: zero-arg read → SetText
        p.Element("arr-count").SetText(p.Plugin<int>("arrayManager", "getCount"));
        p.Element("arr-first").SetText(p.Plugin<string>("arrayManager", "getFirstName"));

        // Case 4: zero-arg read → Condition
        p.When(p.Plugin<bool>("arrayManager", "hasActive")).Truthy()
         .Then(t => t.Element("arr-has-active").Show());

        // Case 3: read with args → SetText (uses URL param as arg!)
        p.Element("arr-status-count").SetText(
            p.Plugin<int>("arrayManager", "getCountByStatus")
             .Arg(p.FromUrl("filterStatus")));

        // Case 3: read with args → Condition
        p.When(p.Plugin<bool>("arrayManager", "contains")
             .Arg(p.FromUrl("searchName")))
         .Truthy()
         .Then(t => t.Element("arr-search-found").Show())
         .Else(e => e.Element("arr-search-not-found").Show());
    }));

    // ── Button: Gather + Header with plugin reads ─────────
    // Sends plugin values as HTTP params AND headers
    Html.NativeButton("arr-send-btn", "Send Array Data")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            p.Get("/Sandbox/HttpPipeline/Http/PluginArrayEcho")
             .Gather(g => g
                 // Plugin read → gather param
                 .Plugin<string>("arrayManager", "getFirstName", "firstName")
                 .Plugin<int>("arrayManager", "getCount", "count")
                 // Plugin read with args → gather (formatSummary needs active + total)
                 // Can't chain .Arg on gather — use separate approach
                 // Header from plugin
                 .Header("X-Array-Count", p.Plugin<string>("arrayManager", "getCount"))
                 // URL source composing with plugin
                 .FromUrl("filterStatus", "status"))
             .Response(r => r
                .OnSuccess<PluginArrayEchoResponse>((json, s) =>
                {
                    s.Element("arr-echo-first").SetText(json, x => x.ReceivedFirstName);
                    s.Element("arr-echo-count").SetText(json, x => x.ReceivedCount);
                    s.Element("arr-echo-header").SetText(json, x => x.ReceivedHeader);
                    s.Element("arr-echo-result").AddClass("text-green-600");
                    // Case 1: void call after success — no args
                    s.Plugin("arrayManager", "shuffle");
                }));
        });

    // ── Button: Plugin read → Route param ─────────────────
    Html.NativeButton("arr-route-btn", "Load Resident by Plugin ID")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            // Plugin int read → route param (plugin provides the ID)
            p.Get("/Sandbox/HttpPipeline/Http/Residents/{id}")
             .Gather(g => g.RouteParam("id", p.Plugin<int>("arrayManager", "getCount")))
             .Response(r => r
                .OnSuccess<ResidentByIdResponse>((json, s) =>
                {
                    s.Element("arr-route-name").SetText(json, x => x.Name);
                    s.Element("arr-route-result").AddClass("text-green-600");
                }));
        });

    // ── Button: Read with args from component ─────────────
    Html.NativeButton("arr-lookup-btn", "Lookup by Name")
        .Reactive(plan, evt => evt.Click, (args, p) =>
        {
            // Case 3: read with arg from component input
            p.Element("arr-lookup-result").SetText(
                p.Plugin<string>("arrayManager", "getNameById")
                 .Arg(p.Component<FusionNumericTextBox>(m => m.LookupId).Value()));
        });
}

<native-vstack gap="Lg">
    <!-- DomReady results -->
    <native-card>
    <native-card-body>
        <native-heading level="H2">26. Plugin Array — All 4 Cases</native-heading>
        <div class="space-y-2 font-mono text-sm">
            <p>Count: <span id="arr-count">—</span></p>
            <p>First: <span id="arr-first">—</span></p>
            <p id="arr-has-active" hidden class="text-green-600">Has active residents ✓</p>
            <p>Status filter count: <span id="arr-status-count">—</span></p>
            <p id="arr-search-found" hidden class="text-green-600">Search name found ✓</p>
            <p id="arr-search-not-found" hidden class="text-amber-600">Search name not found</p>
        </div>
    </native-card-body>
    </native-card>

    <!-- Gather + Header -->
    <native-card>
    <native-card-body>
        <native-heading level="H2">26b. Plugin → Gather + Header + URL</native-heading>
        @(Html.NativeButton("arr-send-btn", "Send Array Data")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
        <div id="arr-echo-result" class="mt-3 space-y-2 font-mono text-sm text-text-muted">
            <p>First: <span id="arr-echo-first">—</span></p>
            <p>Count: <span id="arr-echo-count">—</span></p>
            <p>Header: <span id="arr-echo-header">—</span></p>
        </div>
    </native-card-body>
    </native-card>

    <!-- Route param from plugin -->
    <native-card>
    <native-card-body>
        <native-heading level="H2">26c. Plugin → Route Param</native-heading>
        @(Html.NativeButton("arr-route-btn", "Load Resident by Plugin ID")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
        <div id="arr-route-result" class="mt-3 font-mono text-sm text-text-muted">
            <p>Name: <span id="arr-route-name">—</span></p>
        </div>
    </native-card-body>
    </native-card>

    <!-- Lookup with component arg -->
    <native-card>
    <native-card-body>
        <native-heading level="H2">26d. Plugin Read with Component Arg</native-heading>
        @{ Html.InputField(plan, m => m.LookupId, o => o.Label("Resident ID"))
           .FusionNumericTextBox(b => b.Min(1).Max(100)); }
        @(Html.NativeButton("arr-lookup-btn", "Lookup by Name")
            .CssClass("rounded-md bg-accent px-4 py-2 text-sm font-medium text-white"))
        <div class="mt-3 font-mono text-sm text-text-muted">
            <p>Result: <span id="arr-lookup-result">—</span></p>
        </div>
    </native-card-body>
    </native-card>
</native-vstack>
```

**Page URL:** `/Sandbox/HttpPipeline/Http?filterStatus=active&searchName=John`

---

## What This Proves

| Feature Combination | Section | Evidence |
|---|---|---|
| Plugin read → SetText | 26 | `#arr-count` → "5", `#arr-first` → "John Doe" |
| Plugin read → Condition | 26 | `#arr-has-active` visible |
| Plugin read + URL arg → SetText | 26 | `#arr-status-count` → "3" (3 active) |
| Plugin read + URL arg → Condition | 26 | `#arr-search-found` visible (John found) |
| Plugin read → Gather param | 26b | Server echoes firstName + count |
| Plugin read → Header | 26b | Server echoes X-Array-Count |
| URL source + Plugin in same request | 26b | Both fromUrl and plugin values sent |
| Plugin void call (shuffle) | 26b | No errors after success handler |
| Plugin read → Route param | 26c | `/Residents/5` → "Resident #5" |
| Plugin read + Component arg | 26d | Enter ID → lookup returns name |
| All 4 DSL cases | All | read/void × args/no-args |
| Composition: plugin + URL + component + header + route param | 26b+26c+26d | All sources in same page |

---

## Playwright Tests (8) — `WhenPluginArrayManipulates.cs`

| Test | Assert |
|---|---|
| `plugin_count_displayed_on_load` | `#arr-count` → "5" |
| `plugin_first_name_displayed` | `#arr-first` → "John Doe" |
| `plugin_has_active_shows` | `#arr-has-active` visible |
| `plugin_url_arg_filters_status` | `#arr-status-count` → "3" (navigate with ?filterStatus=active) |
| `plugin_gather_sends_values` | Click "Send Array Data" → `#arr-echo-first` → "John Doe" |
| `plugin_header_reaches_server` | Click "Send Array Data" → `#arr-echo-header` → "5" |
| `plugin_route_param_resolves` | Click "Load Resident by Plugin ID" → `#arr-route-name` has value |
| `plugin_component_arg_lookup` | Enter 2 in numeric, click "Lookup" → `#arr-lookup-result` → "Jane Smith" |
