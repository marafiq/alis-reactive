# URL Template Parameters in HTTP Pipeline

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Support parameterized URLs like `/residents/{residentId}/medications` where route values come from the same shared ValueProducer concept — component reads, event args, literals. At runtime, `{param}` placeholders are replaced with evaluated values before the fetch call.

**Tech Stack:** C# plan model, JSON schema, TypeScript types + runtime

**Prerequisite:** HTTP Headers plan must be landed — it provides `formatForWire` in `Scripts/core/wire-format.ts`, the `buildFetch(req, gatherResult, plan, ctx)` signature, and the `RequireScalarShape` / `Shape.IsScalar` infrastructure.

---

## Architecture

Route parameters are `Dictionary<string, ValueProducer>` on the `Request` model, alongside `url`. The URL field becomes a template string with `{param}` placeholders (matching ASP.NET/OpenAPI convention). The GatherBuilder collects route params via `.RouteParam()` overloads. At runtime, each param's ValueProducer is evaluated, wire-formatted with shape awareness, stringified, URI-encoded, and substituted into the URL template before fetch.

### Shared Guards (from Headers plan)

Route params are string-destination values — same as headers. They reuse:
- `Shape.IsScalar` (Shape.cs) — semantic check for stringifiable types
- `GatherBuilder.RequireScalarShape<TProp>(name, context)` — build-time rejection of arrays/objects
- `ValidateParamName(name)` — rejects null/whitespace param names

### Shape Flow End-to-End

- `TypedComponentSource<int>.ToValueProducer()` carries `Shape.Number` (via `Shape.FromClrType(typeof(int))` — Shape.cs)
- `evaluateValue()` applies shape (core/evaluate.ts:14)
- `formatForWire()` handles date→ISO (core/wire-format.ts — shared with gather and headers)
- `toString()` converts to string (core/shape-convert.ts:72)
- `encodeURIComponent()` ensures URI safety

### Build-Time Validation

1. **Scalar guard:** `RequireScalarShape<TProp>` rejects arrays/objects (reuses header infrastructure)
2. **Param name validation:** Empty/whitespace param names throw `ArgumentException`
3. **Placeholder match:** `HttpRequestBuilder.BuildRequest()` validates that every route param name matches a `{placeholder}` in the URL template — catches typos at build time, not at runtime

### DSL

```csharp
p.Get("/residents/{residentId}/medications")
 .Gather(g => g
     .RouteParam("residentId", args, x => x.Id)           // from event arg (shape: typeof(int))
     .RouteParam("residentId", residentDDL.Value())        // from component read (TypedSource<T>)
     .RouteParam("residentId", 44)                         // static literal (int)
     .RouteParam("residentId", "custom-id")                // static literal (string)
     .Include<FusionDropDownList, Model>(m => m.Filter))   // body/query as usual
 .Response(r => r.OnSuccess(...))
```

### Plan JSON

```json
{
  "method": "GET",
  "url": "/residents/{residentId}/medications",
  "routeParams": {
    "residentId": { "kind": "literal", "value": 44, "shape": { "kind": "number" } }
  },
  "input": { ... }
}
```

### Runtime Resolution

```
URL template: /residents/{residentId}/medications
routeParams:  { residentId: { kind: "literal", value: 44, shape: { kind: "number" } } }
Evaluated:    { residentId: 44 }
Resolved URL: /residents/44/medications
```

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add RouteParams to Request

**File:** `Alis.Reactive/PlanModel/Request.cs`

Add after `Headers` property (currently at line ~19):

```csharp
/// <summary>URL template parameters. Each value is a ValueProducer evaluated at request time.
/// Placeholders like {id} in the URL are replaced with the evaluated, URI-encoded values.</summary>
public Dictionary<string, ValueProducer>? RouteParams { get; internal set; }
```

No `[JsonIgnore(WhenWritingNull)]` attribute — global config at ReactivePlan.cs:20 handles null suppression (same pattern as Headers, Container, Input, etc.).

**Verification:** `dotnet build`. Plan without route params omits the field in JSON.

### Task 2: C# Builder — GatherBuilder.RouteParam() overloads

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add field after `HeaderFields` (after line 15):
```csharp
internal Dictionary<string, ValueProducer> RouteParamFields { get; } = new Dictionary<string, ValueProducer>();
```

Add 5 overloads after Header methods. Every typed overload calls `RequireScalarShape` (already `internal static` from headers work) and `ValidateRouteParamName`:

```csharp
/// <summary>Adds a route param from a static int.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, int value)
{
    ValidateRouteParamName(paramName);
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a route param from a static string. Value must not be null.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, string value)
{
    ValidateRouteParamName(paramName);
    if (value == null)
        throw new System.ArgumentNullException(nameof(value),
            $"Route param '{paramName}' value must not be null. Literal route params require a concrete value. " +
            "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a route param from a static long.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, long value)
{
    ValidateRouteParamName(paramName);
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a route param from a typed source. Route params are scalar — arrays and objects are rejected at build time.</summary>
public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source)
{
    ValidateRouteParamName(paramName);
    if (source == null) throw new System.ArgumentNullException(nameof(source));
    RequireScalarShape<TProp>(paramName, "route param");
    RouteParamFields[paramName] = source.ToValueProducer();
    return this;
}

/// <summary>Adds a route param from an event arg expression. Route params are scalar — arrays and objects are rejected at build time.</summary>
public GatherBuilder<TModel> RouteParam<TArgs, TProp>(
    string paramName, TArgs args, Expression<Func<TArgs, TProp>> path)
{
    ValidateRouteParamName(paramName);
    RequireScalarShape<TProp>(paramName, "route param");
    var eventPath = ExpressionPathHelper.ToEventPath(path);
    var shape = Shape.FromClrType(typeof(TProp));
    RouteParamFields[paramName] = ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape);
    return this;
}

private static readonly System.Text.RegularExpressions.Regex RouteParamNameRe =
    new System.Text.RegularExpressions.Regex(@"^\w+$", System.Text.RegularExpressions.RegexOptions.Compiled);

private static void ValidateRouteParamName(string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException("Route param name must not be null or whitespace.", nameof(paramName));
    if (!RouteParamNameRe.IsMatch(paramName))
        throw new System.ArgumentException(
            $"Route param name '{paramName}' contains invalid characters. " +
            "Names must match [a-zA-Z0-9_] to align with the runtime {{placeholder}} regex.",
            nameof(paramName));
}
```

**Design notes:**
- `RequireScalarShape<TProp>(paramName, "route param")` — same guard as headers, different context string
- Shape propagation on event-arg overload: `Shape.FromClrType(typeof(TProp))` → passes to `ValueProducer.Read`
- Literal overloads (int, string, long) don't need scalar guard — they're scalar by construction
- Literal overloads DO need name validation — typos are the common mistake

**Verified references:**
- `RequireScalarShape<TProp>` — GatherBuilder.cs:80 (from headers implementation)
- `ExpressionPathHelper.ToEventPath<TArgs,TProp>` — ExpressionPathHelper.cs:70
- `Shape.FromClrType(typeof(TProp))` — Shape.cs:47
- `PayloadSource.Event()` — Source.cs:37
- `ValueProducer.Literal(int/string/long)` — ValueProducer.cs:16-22

### Task 3: C# Builder — HttpRequestBuilder wires + validates route params

**File:** `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`

After headers wiring (after line 148), before `return request;`:

```csharp
if (_gatherBuilder != null && _gatherBuilder.RouteParamFields.Count > 0)
{
    // Validate route param names match URL placeholders — catches typos at build time
    foreach (var paramName in _gatherBuilder.RouteParamFields.Keys)
    {
        if (!_url.Contains("{" + paramName + "}"))
            throw new InvalidOperationException(
                $"Route param '{paramName}' does not match any placeholder in URL '{_url}'. " +
                $"Expected '{{" + paramName + "}}' in the URL template.");
    }
    request.RouteParams = new Dictionary<string, ValueProducer>(_gatherBuilder.RouteParamFields);
}
```

**Design note:** This validation catches the single most likely developer mistake — a typo between `.RouteParam("residentId", ...)` and `"/residents/{residnetId}/..."`. Without this, the typo silently produces a URL with the unreplaced `{residnetId}` placeholder, which the server returns 404 for. With this, the developer gets a clear exception at plan construction time.

### Task 4: JSON Schema — Request gains routeParams

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

In Request properties, add after `headers`:

```json
"routeParams": {
  "type": "object",
  "additionalProperties": { "$ref": "#/$defs/ValueProducer" }
},
```

### Task 5: TS Types

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Add to Request interface after `headers`:

```typescript
routeParams?: Record<string, ValueProducer>;
```

### Task 6: TS Runtime — URL template resolution

**File:** `Alis.Reactive.SandboxApp/Scripts/execution/http.ts`

Add import (toString not yet imported from headers work):
```typescript
import { toString } from "../core/shape-convert";
```

Add function before `buildFetch`:

```typescript
const ROUTE_PARAM_RE = /\{(\w+)\}/g;

function resolveRouteParams(
  urlTemplate: string,
  routeParams: Record<string, ValueProducer>,
  plan: Plan,
  ctx?: ExecContext,
): string {
  return urlTemplate.replace(ROUTE_PARAM_RE, (match, paramName: string) => {
    const producer = routeParams[paramName];
    if (!producer) {
      log.warn("unresolved route param", { param: paramName });
      return match; // keep placeholder — fail visible, not silent
    }
    const raw = evaluateValue(producer, plan, ctx);
    if (raw == null) {
      log.warn("route param evaluated to null", { param: paramName });
      return "";
    }
    const wire = formatForWire(raw, producer.shape);
    const result = toString(wire);
    return encodeURIComponent(result.ok ? result.value : String(wire));
  });
}
```

**Imports already present from headers:** `evaluateValue` from `../core/evaluate`, `formatForWire` from `../core/wire-format`.

Update `buildFetch` — resolve route params at the top:

```typescript
let url = req.routeParams
  ? resolveRouteParams(req.url, req.routeParams, plan, ctx)
  : req.url;
```

This replaces `let url = req.url;` at the start of buildFetch. The rest of buildFetch is unchanged.

### Task 6b: TS Unit Tests (vitest) — resolveRouteParams

**New file:** `Alis.Reactive.SandboxApp/Scripts/__tests__/execution/resolveRouteParams.test.ts`

The process pipeline requires vitest coverage for new TS runtime functions. `resolveRouteParams` has non-trivial logic (regex, null handling, URI encoding, shape formatting) that must be unit-tested independently of the browser.

Export `resolveRouteParams` for testing (can be a named export used only by tests — the function is `function`, not `export function` in the plan. Either export it or extract to a testable module).

| Test | What It Proves |
|---|---|
| `replaces single {param} with evaluated literal value` | Basic replacement works |
| `replaces multiple {params} in same URL` | All placeholders resolved |
| `unresolved placeholder keeps original {param} text` | Missing producer → visible failure, not silent |
| `null value produces empty string` | Null handling documented behavior |
| `URI-encodes special characters in values` | `encodeURIComponent("John Doe")` → `"John%20Doe"` |
| `date value formatted as ISO string via formatForWire` | Shape.Date → `formatForWire` → ISO string in URL |

Note: These tests require mocking `evaluateValue` and `log` since they depend on plan resolution. Use vitest's `vi.mock` for the imports.

### Design Notes

**Gather() is required for route params.** Without `.Gather(g => g.RouteParam(...))`, `_gatherBuilder` is null in HttpRequestBuilder and route params are silently dropped. This is natural DSL enforcement — route params live on GatherBuilder alongside headers, static fields, and component fields. The DSL makes this obvious: `p.Get("/path/{id}").Gather(g => g.RouteParam("id", 42))`.

**URL resolution ordering in buildFetch:**
1. Route params resolved FIRST: `{id}` → `42` → URL becomes `/residents/42/meds`
2. GET query params appended AFTER: `/residents/42/meds?filter=active`

Route params are path segments. Query params are query string. They compose correctly because route params transform the URL template before query params are appended.

**Chained requests resolve independently.** Each chained request has its own `url`, `routeParams`, `headers`. `executeRequest` calls `buildFetch` for each — route params from the parent don't leak into the child.

### Task 7: Sandbox — 4 sections with exact element IDs

**Response DTOs** (`HttpShowcaseModel.cs`):
```csharp
public class ResidentByIdResponse
{
    public int ResidentId { get; set; }
    public string? Name { get; set; }
}

public class FacilityResidentResponse
{
    public int FacilityId { get; set; }
    public int ResidentId { get; set; }
    public string? Name { get; set; }
}
```

**Controller endpoints** (`HttpController.cs`):
```csharp
[HttpGet("Residents/{id}")]
public IActionResult ResidentById(int id) =>
    Json(new { residentId = id, name = $"Resident #{id}" });

[HttpGet("Facilities/{facilityId}/Residents/{residentId}")]
public IActionResult FacilityResident(int facilityId, int residentId) =>
    Json(new { facilityId, residentId, name = $"Resident #{residentId} at Facility #{facilityId}" });

[HttpGet("ResidentByName/{name}")]
public IActionResult ResidentByName(string name) =>
    Json(new { receivedName = name, decoded = System.Net.WebUtility.UrlDecode(name) });
```

#### Section 15: Single Route Param

**Button:** "Load Resident #42"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/Residents/{id}")
 .Gather(g => g.RouteParam("id", 42))
 .WhileLoading(l => l.Element("route-single-spinner").Show())
 .Response(r => r.OnSuccess<ResidentByIdResponse>((json, s) =>
 {
     s.Element("route-single-spinner").Hide();
     s.Element("route-single-id").SetText(json, x => x.ResidentId);
     s.Element("route-single-name").SetText(json, x => x.Name);
     s.Element("route-single-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `route-single-spinner` — loading indicator
- `route-single-id` — displays echoed residentId (expect: "42")
- `route-single-name` — displays echoed name (expect: "Resident #42")
- `route-single-result` — success class container

#### Section 16: Multiple Route Params

**Button:** "Load Facility Resident"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/Facilities/{facilityId}/Residents/{residentId}")
 .Gather(g => g
     .RouteParam("facilityId", 7)
     .RouteParam("residentId", 99))
 .WhileLoading(l => l.Element("route-multi-spinner").Show())
 .Response(r => r.OnSuccess<FacilityResidentResponse>((json, s) =>
 {
     s.Element("route-multi-spinner").Hide();
     s.Element("route-multi-facility").SetText(json, x => x.FacilityId);
     s.Element("route-multi-resident").SetText(json, x => x.ResidentId);
     s.Element("route-multi-name").SetText(json, x => x.Name);
     s.Element("route-multi-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `route-multi-facility` — expect: "7"
- `route-multi-resident` — expect: "99"
- `route-multi-name` — expect: "Resident #99 at Facility #7"

#### Section 17: Chained with Route Params + Headers

**Button:** "Chain with Route Params"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/Residents/{id}")
 .Gather(g => g
     .RouteParam("id", 42)
     .Header("X-Step", "first"))
 .WhileLoading(l => l.Element("route-chain-spinner").Show())
 .Response(r => r
    .OnSuccess<ResidentByIdResponse>((json, s) =>
    {
        s.Element("route-chain-first-id").SetText(json, x => x.ResidentId);
        s.Element("route-chain-first").AddClass("text-green-600");
    })
    .Chained(c => c
        .Get("/Sandbox/HttpPipeline/Http/Facilities/{facilityId}/Residents/{residentId}")
        .Gather(g2 => g2
            .RouteParam("facilityId", 3)
            .RouteParam("residentId", 77)
            .Header("X-Step", "second"))
        .Response(r2 => r2.OnSuccess<FacilityResidentResponse>((json2, s2) =>
        {
            s2.Element("route-chain-spinner").Hide();
            s2.Element("route-chain-second-name").SetText(json2, x => x.Name);
            s2.Element("route-chain-second").AddClass("text-green-600");
        }))
    ))
```

**Element IDs:**
- `route-chain-first-id` — expect: "42" (first hop)
- `route-chain-second-name` — expect: "Resident #77 at Facility #3" (second hop)

#### Section 18: URI-Encoded Route Param

**Button:** "Load Resident by Name"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/ResidentByName/{name}")
 .Gather(g => g.RouteParam("name", "John Doe"))
 .Response(r => r.OnSuccess<ResidentByNameResponse>((json, s) =>
 {
     s.Element("route-encoded-name").SetText(json, x => x.ReceivedName);
     s.Element("route-encoded-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `route-encoded-name` — expect: "John Doe" (server URL-decodes the `John%20Doe` path segment)

**Response DTO:**
```csharp
public class ResidentByNameResponse
{
    public string? ReceivedName { get; set; }
}
```

### Task 8: Unit tests — one per overload + guards + composition

Every overload has a dedicated test. Every guard path has a test. Every composition pattern has a test.

| Test | What It Proves |
|---|---|
| `literal_int_route_param_produces_correct_json` | int literal → `{ kind: "literal", value: 42, shape: { kind: "number" } }` + AssertSchemaValid |
| `literal_string_route_param_produces_correct_json` | string literal → `{ kind: "literal", value: "abc", shape: { kind: "string" } }` + AssertSchemaValid |
| `literal_long_route_param_produces_correct_json` | long literal → number shape + AssertSchemaValid |
| `typed_source_route_param_produces_component_read` | TypedComponentSource → `{ kind: "read", from: { kind: "component" }, member: "..." }` + AssertSchemaValid |
| `event_arg_route_param_carries_shape` | event arg → shape flows from TProp (int → number) + AssertSchemaValid |
| `plan_without_route_params_omits_field` | null suppression — JSON does NOT contain `"routeParams"` |
| `multiple_route_params_all_appear` | two params on one URL template |
| `route_params_and_headers_coexist` | same request has both routeParams AND headers |
| `route_params_with_body_gather_coexist` | same Gather has `.RouteParam()` AND `.Include<>()` — both appear in JSON |
| `route_params_on_chained_requests_independent` | each chain hop carries own URL + routeParams |
| `array_route_param_throws_at_build_time` | scalar guard rejects string[] |
| `null_string_route_param_throws_at_build_time` | null literal string rejected |
| `empty_param_name_throws_at_build_time` | name validation rejects empty |
| `mismatched_param_name_throws_at_build_time` | placeholder validation in BuildRequest — param "residentId" with URL containing `{residnetId}` (typo) throws |
| `nullable_int_route_param_accepted` | Nullable<int> passes scalar guard |
| `whitespace_param_name_throws_at_build_time` | name validation rejects whitespace |
| `hyphenated_param_name_throws_at_build_time` | name validation rejects `resident-id` — grammar must match runtime `\w+` regex |
| `datetime_typed_source_carries_date_shape` | TypedComponentSource<DateTime> → shape: { kind: "date" } — proves formatForWire will produce ISO |

### Task 9: Playwright tests — `WhenRouteParamsResolve.cs`

**File:** `tests/Alis.Reactive.PlaywrightTests/HttpPipeline/WhenRouteParamsResolve.cs`

Navigate to `/Sandbox/HttpPipeline/Http`, wait for boot + DomReady GET.

**Section 15 — Single Route Param:**

| Test | Click | Assert |
|---|---|---|
| `single_route_param_resolves_to_correct_id` | "Load Resident #42" | `#route-single-id` → "42" |
| `single_route_param_server_echoes_name` | "Load Resident #42" | `#route-single-name` → "Resident #42" |
| `single_route_param_applies_success_class` | "Load Resident #42" | `#route-single-result` has class `text-green-600` |
| `single_route_param_hides_spinner` | "Load Resident #42" | `#route-single-spinner` is hidden |

**Section 16 — Multiple Route Params:**

| Test | Click | Assert |
|---|---|---|
| `multiple_route_params_resolve_both_values` | "Load Facility Resident" | `#route-multi-facility` → "7", `#route-multi-resident` → "99" |
| `multiple_route_params_server_echoes_compound_name` | "Load Facility Resident" | `#route-multi-name` → "Resident #99 at Facility #7" |

**Section 17 — Chained with Route Params + Headers:**

| Test | Click | Assert |
|---|---|---|
| `chained_first_hop_resolves_route_param` | "Chain with Route Params" | `#route-chain-first-id` → "42" |
| `chained_second_hop_resolves_different_route_params` | "Chain with Route Params" | `#route-chain-second-name` → "Resident #77 at Facility #3" |
| `chained_route_params_spinner_hides_after_both` | "Chain with Route Params" | `#route-chain-spinner` is hidden |

**Section 18 — URI-Encoded Route Param:**

| Test | Click | Assert |
|---|---|---|
| `route_param_with_space_is_uri_encoded` | "Load Resident by Name" | `#route-encoded-name` → "John Doe" (server URL-decoded the `John%20Doe` in path) |

**Total: 10 Playwright tests** across 4 sections. Each test follows the BDD pattern: click button → assert element text/class/visibility. No `page.evaluate()`. Real interactions only.

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Plan JSON with routeParams validates against schema (AssertSchemaValid)
- [ ] Plan JSON without routeParams omits the field
- [ ] URL template `{param}` replaced correctly at runtime
- [ ] Route param values are URI-encoded
- [ ] Date route params become ISO strings (via formatForWire — shape MUST be present)
- [ ] Unresolved `{param}` (no matching routeParam) logs warning, keeps placeholder
- [ ] Null route param value logs warning, produces empty string
- [ ] Route params compose with body/query gather (both work simultaneously)
- [ ] Route params compose with custom headers (all three features on one request)
- [ ] Non-scalar TypedSource (e.g., string[]) throws at build time with clear message
- [ ] Null literal string value throws ArgumentNullException at build time
- [ ] Null TypedSource throws ArgumentNullException at build time
- [ ] Empty/whitespace param name throws at build time
- [ ] Param name with non-word chars (e.g., `resident-id`) throws at build time — grammar matches runtime `\w+` regex
- [ ] Param name not matching URL placeholder throws at build time
- [ ] Route params compose with Include<> body fields in same Gather
- [ ] Gather() required — no Gather = no route params (natural DSL enforcement)
- [ ] All C# unit tests pass (15 per-overload + guard + composition tests)
- [ ] All Playwright tests pass (4 end-to-end scenarios)
- [ ] All existing 789+ Playwright tests pass (no regressions)
