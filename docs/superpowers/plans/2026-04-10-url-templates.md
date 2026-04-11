# URL Template Parameters in HTTP Pipeline

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Support parameterized URLs like `/residents/{residentId}/medications` where route values come from the same shared ValueProducer concept — component reads, event args, literals. At runtime, `{param}` placeholders are replaced with evaluated values before the fetch call.

**Tech Stack:** C# plan model, JSON schema, TypeScript types + runtime

---

## Architecture

Route parameters are `Dictionary<string, ValueProducer>` on the `Request` model, alongside `url`. The URL field becomes a template string with `{param}` placeholders (matching ASP.NET/OpenAPI convention). The GatherBuilder collects route params via `.RouteParam()` overloads. At runtime, each param's ValueProducer is evaluated, stringified with shape awareness, URI-encoded, and substituted into the URL template before fetch.

Shape flows end-to-end:
- `TypedComponentSource<int>.ToValueProducer()` carries `Shape.Number`
- `evaluateValue()` applies shape
- Route param value is `toString()` + `encodeURIComponent()` for URI safety
- Dates become ISO strings via `formatForWire` pattern

### DSL

```csharp
p.Get("/residents/{residentId}/medications")
 .Gather(g => g
     .RouteParam("residentId", args, x => x.Id)           // from event arg
     .RouteParam("residentId", residentDDL.Value())        // from component read
     .RouteParam("residentId", 44)                         // static literal
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
routeParams:  { residentId: ValueProducer.Literal(44) }
Evaluated:    { residentId: 44 }
Resolved URL: /residents/44/medications
```

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add RouteParams to Request

**File:** `Alis.Reactive/PlanModel/Request.cs`

Add after `Url` property (after line 10):

```csharp
[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
public Dictionary<string, ValueProducer>? RouteParams { get; internal set; }
```

**Verification:** `dotnet build`. Plan without route params omits the field.

### Task 2: C# Builder — GatherBuilder.RouteParam() overloads

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add field:

```csharp
internal Dictionary<string, ValueProducer> RouteParamFields { get; } = new Dictionary<string, ValueProducer>();
```

Add overloads:

```csharp
/// <summary>Adds a route param from an event arg.</summary>
public GatherBuilder<TModel> RouteParam<TArgs, TProp>(
    string paramName, TArgs args, Expression<Func<TArgs, TProp>> path)
{
    var eventPath = ExpressionPathHelper.ToEventPath(path);
    var shape = Shape.FromClrType(typeof(TProp));
    RouteParamFields[paramName] = ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape);
    return this;
}

/// <summary>Adds a route param from a typed component source.</summary>
public GatherBuilder<TModel> RouteParam<TProp>(
    string paramName, TypedComponentSource<TProp> source)
{
    RouteParamFields[paramName] = source.ToValueProducer();
    return this;
}

/// <summary>Adds a route param from a static int.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, int value)
{
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a route param from a static string.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, string value)
{
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a route param from a static long.</summary>
public GatherBuilder<TModel> RouteParam(string paramName, long value)
{
    RouteParamFields[paramName] = ValueProducer.Literal(value);
    return this;
}
```

**Verification:** `dotnet build`. Test: `.RouteParam("id", 42)` produces correct plan JSON.

### Task 3: C# Builder — HttpRequestBuilder wires route params

**File:** `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`

After validator wiring, before `return request;`:

```csharp
if (_gatherBuilder != null && _gatherBuilder.RouteParamFields.Count > 0)
    request.RouteParams = new Dictionary<string, ValueProducer>(_gatherBuilder.RouteParamFields);
```

**Note:** Route params can exist even with no body fields. A GET with only route params is valid:
```csharp
p.Get("/residents/{id}").Gather(g => g.RouteParam("id", 44))
```

### Task 4: JSON Schema — Request gains routeParams

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

In Request properties, after `url`:

```json
"routeParams": {
  "type": "object",
  "additionalProperties": { "$ref": "#/$defs/ValueProducer" }
}
```

### Task 5: TS Types

**File:** `Scripts/types/plan.ts`

Add to Request interface after `url`:

```typescript
routeParams?: Record<string, ValueProducer>;
```

### Task 6: TS Runtime — URL template resolution

**File:** `Scripts/execution/http.ts`

Add imports: `evaluateValue` from `../core/evaluate`, `toString` from `../core/shape-convert`.

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
      return match;
    }
    const raw = evaluateValue(producer, plan, ctx);
    if (raw == null) {
      log.warn("route param evaluated to null", { param: paramName });
      return "";
    }
    const result = toString(raw);
    return encodeURIComponent(result.ok ? result.value : String(raw));
  });
}
```

Update `buildFetch` to resolve route params:

```typescript
let url = req.routeParams
  ? resolveRouteParams(req.url, req.routeParams, plan, ctx)
  : req.url;
```

Update `buildFetch` signature to accept `plan` and `ctx`. Update call site.

### Task 7: Playwright test

Test: GET `/api/test/{id}` with RouteParam("id", 42) → verify server receives `/api/test/42`.

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Plan JSON with routeParams validates against schema
- [ ] Plan JSON without routeParams omits the field
- [ ] URL template `{param}` replaced correctly at runtime
- [ ] Route param values are URI-encoded
- [ ] Date route params become ISO strings
- [ ] Unresolved `{param}` (no matching routeParam) logs warning, keeps placeholder
- [ ] Null route param value logs warning, produces empty string
- [ ] Route params compose with body/query gather (both work simultaneously)
- [ ] All unit + Playwright tests pass
