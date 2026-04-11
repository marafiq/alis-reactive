# HTTP Headers in Gather Pipeline

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to set HTTP request headers from the same value sources used for body/query params — component reads, event args, literals. Headers flow through the shared ValueProducer concept with shape-aware serialization.

**Tech Stack:** C# plan model, JSON schema, TypeScript types + runtime

**Prerequisite:** Task 0 (extract `formatForWire` to shared module) must land first.

---

## Architecture

Headers are `Dictionary<string, ValueProducer>` on the `Request` model (not GatherInput) because headers apply to any request regardless of input type. The GatherBuilder collects them via `.Header()` overloads. At runtime, each header's ValueProducer is evaluated via `evaluateValue()`, the result is stringified with shape awareness (dates become ISO strings), and set on the fetch `RequestInit.headers`.

Shape flows from C# → plan JSON → TS runtime:
- `TypedComponentSource<DateTime>.ToValueProducer()` carries `Shape.Date` (via `Shape.FromClrType(typeof(DateTime))`)
- `evaluateValue()` applies shape via `applyShape()` (core/shape-convert.ts)
- Header value is stringified via `String(value)` — dates already ISO from `formatForWire`

### DSL

```csharp
p.Post("/api/orders")
 .Gather(g => g
     .Include<FusionTextBox, Model>(m => m.Name)
     .Header("X-Api-Version", "2024-01-15")                    // literal
     .Header("X-Tenant-Id", tenantDDL.Value())                 // component read (TypedSource<T>)
     .Header("X-Correlation-Id", args, a => a.CorrelationId))  // event arg
 .Response(r => r.OnSuccess(...))
```

### Plan JSON

```json
{
  "method": "POST",
  "url": "/api/orders",
  "headers": {
    "X-Api-Version": { "kind": "literal", "value": "2024-01-15", "shape": { "kind": "string" } },
    "X-Tenant-Id": { "kind": "read", "from": { "kind": "component", "component": "tenant-ddl" }, "member": "value", "shape": { "kind": "string" } },
    "X-Correlation-Id": { "kind": "read", "from": { "kind": "payload", "scope": "event" }, "member": "correlationId" }
  },
  "input": { ... }
}
```

---

## Step-by-Step Implementation

### Task 0: Extract formatForWire to shared module (PREREQUISITE)

**Current:** `formatForWire` is local to `Alis.Reactive.SandboxApp/Scripts/execution/gather.ts:43-49`.

**New file:** `Alis.Reactive.SandboxApp/Scripts/core/wire-format.ts`

```typescript
import type { Shape } from "../types";

/** Shape-aware wire formatting. Date timestamps -> ISO strings for HTTP transport. */
export function formatForWire(value: unknown, shape?: Shape): unknown {
  if (!shape) return value;
  if (shape.kind === "date" && typeof value === "number" && !isNaN(value))
    return new Date(value).toISOString();
  if (shape.kind === "nullable" && shape.inner?.kind === "date" && typeof value === "number" && !isNaN(value))
    return new Date(value).toISOString();
  return value;
}
```

**Update gather.ts:43-49:** Delete local `formatForWire`. Add import:
```typescript
import { formatForWire } from "../core/wire-format";
```

**Verification:** `npm run typecheck` + `npm run build`. All 779 Playwright tests still pass (no behavior change).

### Task 1: C# Plan Model — Add Headers to Request

**File:** `Alis.Reactive/PlanModel/Request.cs`

Add after `Next` property (after line 17):

```csharp
[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
public Dictionary<string, ValueProducer>? Headers { get; internal set; }
```

Note: `WhenWritingNull` ensures plans without headers omit the field. The global `CamelCase` policy serializes as `"headers"`. `ValueProducer` already has `WriteOnlyPolymorphicConverter`.

**Verification:** `dotnet build`. Render a plan with no headers — JSON should NOT contain `"headers"`.

### Task 2: C# Builder — GatherBuilder.Header() overloads

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add using at top (line 4):
```csharp
using Alis.Reactive.Builders.Conditions;
```

Add field after `EventFields` (after line 13):
```csharp
internal Dictionary<string, ValueProducer> HeaderFields { get; } = new Dictionary<string, ValueProducer>();
```

Add 3 overloads after `FromEvent` method (after line 47):

```csharp
/// <summary>Adds a literal string header.</summary>
public GatherBuilder<TModel> Header(string name, string value)
{
    HeaderFields[name] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a header from any typed source (component, URL, plugin).</summary>
public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source)
{
    HeaderFields[name] = source.ToValueProducer();
    return this;
}

/// <summary>Adds a header from an event arg expression.</summary>
public GatherBuilder<TModel> Header<TArgs, TProp>(string name, TArgs args, Expression<Func<TArgs, TProp>> path)
{
    var eventPath = ExpressionPathHelper.ToEventPath(path);
    HeaderFields[name] = ValueProducer.Read(PayloadSource.Event(), eventPath);
    return this;
}
```

**Verified references:**
- `ValueProducer.Literal(string)` — ValueProducer.cs:16
- `TypedSource<TProp>.ToValueProducer()` — TypedSource.cs:16
- `ExpressionPathHelper.ToEventPath<TArgs,TProp>` — ExpressionPathHelper.cs:70
- `PayloadSource.Event()` — Source.cs:37

**Verification:** `dotnet build`. Test: `.Header("X-Test", "value")` produces plan JSON with `"headers": { "X-Test": { "kind": "literal" } }`.

### Task 3: C# Builder — HttpRequestBuilder wires headers to Request

**File:** `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`

In `BuildRequest()` method, after validator wiring (after line 145 `request.ValidatorType = _validatorType;`), before the implicit `return request;` at end of method — add:

```csharp
if (_gatherBuilder != null && _gatherBuilder.HeaderFields.Count > 0)
    request.Headers = new Dictionary<string, ValueProducer>(_gatherBuilder.HeaderFields);
```

Add `using System.Collections.Generic;` if not already present (it IS present at line 2).

**Verification:** Full build + unit test with header → plan JSON shows headers on Request.

### Task 4: JSON Schema — Request gains headers

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

In the Request definition (line 342-371), add after `"url"` property (after line 350):

```json
"headers": {
  "type": "object",
  "additionalProperties": { "$ref": "#/$defs/ValueProducer" }
},
```

**Verification:** `AssertSchemaValid` passes with headers in plan JSON. Plans without headers still validate.

### Task 5: TS Types — Request gains headers

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Add to Request interface (after `url: string;` at line 226):

```typescript
headers?: Record<string, ValueProducer>;
```

**Verification:** `npm run typecheck` — clean.

### Task 6: TS Runtime — http.ts evaluates headers

**File:** `Alis.Reactive.SandboxApp/Scripts/execution/http.ts`

Add imports (at top, after existing imports):
```typescript
import { evaluateValue } from "../core/evaluate";
import { formatForWire } from "../core/wire-format";
```

Change `buildFetch` signature (line 19) to accept `plan` and `ctx`:

```typescript
function buildFetch(req: Request, gatherResult: GatherResult, plan: Plan, ctx?: ExecContext): ResolvedFetch {
```

Add type imports:
```typescript
import type { Request, ResponseHandler, Plan, ExecContext } from "../types";
```

After the Content-Type/body setup block (after line 35 `}`), before the `return`:

```typescript
// Evaluate and set custom headers from plan
if (req.headers) {
  const existing = (init.headers as Record<string, string>) ?? {};
  for (const [name, producer] of Object.entries(req.headers)) {
    const value = evaluateValue(producer, plan, ctx);
    if (value != null) {
      const wire = formatForWire(value, producer.shape);
      existing[name] = String(wire);
    }
  }
  init.headers = existing;
}
```

Update `buildFetch` call site in `executeRequest` (line 61):

```typescript
const resolved = buildFetch(req, gatherResult, plan, ctx);
```

**Verification:** `npm run typecheck` + `npm run build`. Sandbox test: add a header, verify in browser DevTools Network tab.

### Task 7: Playwright test

Test: POST with `.Header("X-Custom", "test-value")` → verify server receives the header.

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Plan JSON with headers validates against schema
- [ ] Plan JSON without headers omits the field
- [ ] All C# unit tests pass
- [ ] All Playwright tests pass
- [ ] Browser: header visible in Network tab
- [ ] Literal, component read, and event arg headers all work
- [ ] Null header value is suppressed (not sent)
- [ ] User headers merge with auto-set Content-Type. Explicit Content-Type header overrides the default.
