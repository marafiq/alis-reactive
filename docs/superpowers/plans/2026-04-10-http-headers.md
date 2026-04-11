# HTTP Headers in Gather Pipeline

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to set HTTP request headers from the same value sources used for body/query params — component reads, event args, literals. Headers flow through the shared ValueProducer concept with shape-aware serialization.

**Tech Stack:** C# plan model, JSON schema, TypeScript types + runtime

---

## Architecture

Headers are `Dictionary<string, ValueProducer>` on the `Request` model (not GatherInput) because headers apply to any request regardless of input type. The GatherBuilder collects them via `.Header()` overloads. At runtime, each header's ValueProducer is evaluated via `evaluateValue()`, the result is stringified with shape awareness (dates become ISO strings), and set on the fetch `RequestInit.headers`.

Shape flows from C# → plan JSON → TS runtime:
- `TypedComponentSource<DateTime>.ToValueProducer()` carries `Shape.Date`
- `evaluateValue()` applies shape via `applyShape()`
- Header value stringified via `String(value)` — dates already ISO from applyShape + toDate

### DSL

```csharp
p.Post("/api/orders")
 .Gather(g => g
     .Include<FusionTextBox, Model>(m => m.Name)
     .Header("X-Api-Version", "2024-01-15")                    // literal
     .Header("X-Tenant-Id", tenantDDL.Value())                 // component read
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

### Task 1: C# Plan Model — Add Headers to Request

**File:** `Alis.Reactive/PlanModel/Request.cs`

Add after line 17 (after `Next` property):

```csharp
public Dictionary<string, ValueProducer>? Headers { get; internal set; }
```

No serialization attributes needed — global `CamelCase` policy serializes as `"headers"`, `WhenWritingNull` omits when null. `ValueProducer` already has `WriteOnlyPolymorphicConverter`.

**Verification:** `dotnet build` — 0 errors. Render a plan with no headers — JSON should NOT contain `"headers"`.

### Task 2: C# Builder — GatherBuilder.Header() overloads

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add field after line 13:

```csharp
internal Dictionary<string, ValueProducer> HeaderFields { get; } = new Dictionary<string, ValueProducer>();
```

Add `using Alis.Reactive.Builders.Conditions;` at top.

Add 3 overloads after the `FromEvent` method:

```csharp
/// <summary>Adds a literal string header.</summary>
public GatherBuilder<TModel> Header(string name, string value)
{
    HeaderFields[name] = ValueProducer.Literal(value);
    return this;
}

/// <summary>Adds a header from a typed component source (e.g., ddl.Value()).</summary>
public GatherBuilder<TModel> Header<TProp>(string name, TypedComponentSource<TProp> source)
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

**Verification:** `dotnet build`. Write a test that calls `.Header("X-Test", "value")` and verify plan JSON contains `"headers": { "X-Test": { "kind": "literal" } }`.

### Task 3: C# Builder — HttpRequestBuilder wires headers to Request

**File:** `Alis.Reactive/Builders/Requests/HttpRequestBuilder.cs`

After the validator wiring (before `return request;`), add:

```csharp
if (_gatherBuilder != null && _gatherBuilder.HeaderFields.Count > 0)
    request.Headers = new Dictionary<string, ValueProducer>(_gatherBuilder.HeaderFields);
```

**Verification:** Full build + unit test with header → plan JSON shows headers on Request.

### Task 4: JSON Schema — Request gains headers

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

In the Request definition properties, add:

```json
"headers": {
  "type": "object",
  "additionalProperties": { "$ref": "#/$defs/ValueProducer" }
}
```

**Verification:** `AssertSchemaValid` passes with headers in plan JSON.

### Task 5: TS Types — Request gains headers

**File:** `Scripts/types/plan.ts`

Add to Request interface:

```typescript
headers?: Record<string, ValueProducer>;
```

**Verification:** `npm run typecheck` — clean.

### Task 6: TS Runtime — http.ts evaluates headers

**File:** `Scripts/execution/http.ts`

Import `evaluateValue` from `../core/evaluate`.

Update `buildFetch` signature to accept `plan` and `ctx`.

After the Content-Type header setup, add:

```typescript
if (req.headers) {
  const existing = (init.headers as Record<string, string>) ?? {};
  for (const [name, producer] of Object.entries(req.headers)) {
    const value = evaluateValue(producer, plan, ctx);
    if (value != null) {
      existing[name] = String(value);
    }
  }
  init.headers = existing;
}
```

Update the `buildFetch` call site to pass `plan` and `ctx`.

**Verification:** `npm run typecheck` + `npm run build`. Sandbox test: add a header, verify in browser DevTools Network tab.

### Task 7: Playwright test

Write a test that sends a request with custom headers and verifies the server receives them.

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Plan JSON with headers validates against schema
- [ ] Plan JSON without headers omits the field
- [ ] All 258 C# unit tests pass
- [ ] All 779 Playwright tests pass
- [ ] Browser: header visible in Network tab
- [ ] Literal, component read, and event arg headers all work
- [ ] Null header value is suppressed (not sent)
- [ ] User header overrides auto-set Content-Type when specified
