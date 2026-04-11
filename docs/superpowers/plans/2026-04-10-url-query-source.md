# URL Query Parameter Source (UrlSource)

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to READ values from the browser's current URL query string (`window.location.search`) and use them anywhere ValueProducer is accepted — gather, conditions, headers, route params, pipeline commands. This is a new value SOURCE, not a new value kind.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver

**Prerequisite:** HTTP Headers and URL Templates must be landed. They provide Shape.IsScalar, RequireScalarShape, formatForWire, buildFetch(plan, ctx), and the test patterns this plan follows.

---

## Architecture

A new `UrlSource` kind is added to the `Source` union (alongside `ComponentSource` and `PayloadSource`). The `member` field on `ReadProducer` serves as the query parameter name. At runtime, `resolveSource` dispatches to `new URLSearchParams(window.location.search)`, and `evaluateValue` reads the specific param via `.get(member)`.

### Why Shape Matters Here

URL query params are inherently strings. Without shape:
- `"42" > "1"` → string comparison → WRONG (string "42" > "1" is true, but "9" > "10" is also true)
- `"true" === true` → false → condition breaks

With shape:
- `FromUrl<int>("page")` → `Shape.Number` → `applyShape("42", numberShape)` → `42` → numeric `Gt(1)` works
- `FromUrl<bool>("active")` → `Shape.Boolean` → `applyShape("true", boolShape)` → `true` → `Truthy()` works
- `FromUrl("tab")` → `Shape.String` (default) → passthrough → `Eq("meds")` works

### Null Semantics

`URLSearchParams.get(name)` returns `null` for absent params. This is NOT an error — it's a normal condition (the URL might not have `?page=2`). The null flows through the existing `raw == null ? raw : applyShape(...)` pattern in evaluateValue. Conditions like `.IsNull()` and `.NotNull()` work correctly. This is different from route params where null is always a bug (route params are required path segments).

### Source Union Widening

Adding UrlSource to the Source union means `SetReaction.on` and `CallReaction.on` could theoretically target a URL. This is semantically invalid but harmless: the C# builders won't generate it (no builder method creates Set/Call with UrlSource), and the runtime would throw via getJsTypeForSource which rejects non-component/non-plugin sources.

### DSL — Five Usage Contexts

```csharp
// 1. GATHER: pass URL param as HTTP request param
p.Get("/api/data")
 .Gather(g => g
     .FromUrl("facilityId")                          // reads ?facilityId, sends as facilityId
     .FromUrl("unitId", "unit"))                     // reads ?unitId, sends as "unit"

// 2. CONDITIONS: branch based on URL param
p.When(p.FromUrl("tab")).Eq("medications")
 .Then(t => t.Element("meds-panel").Show())

// 3. PIPELINE: display URL param value
p.Element("facility-name").SetText(p.FromUrl("facilityId"))

// 4. TYPED: numeric URL param with comparison
p.When(p.FromUrl<int>("page")).Gt(1).Then(t => {
    t.Element("prev-button").Show();
})

// 5. COMPOSE: URL param flows into route param, header, and gather
p.Get("/api/facilities/{facilityId}/residents")
 .Gather(g => g
     .RouteParam("facilityId", p.FromUrl<int>("facilityId"))
     .Header("X-Tab", p.FromUrl("tab"))
     .FromUrl("status", "filterStatus"))
```

### Plan JSON

```json
{
  "kind": "read",
  "from": { "kind": "url" },
  "member": "facilityId",
  "shape": { "kind": "string" }
}
```

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add UrlSource to Source union

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `PayloadSource` class, add:

```csharp
/// <summary>Reads a value from the browser's current URL query string.
/// Singleton — no per-instance state. The query param name is the member on ReadProducer.</summary>
public sealed class UrlSource : Source
{
    public string Kind => "url";
    private UrlSource() { }
    internal static UrlSource Instance { get; } = new UrlSource();
}
```

Singleton — one instance, no state. `WriteOnlyPolymorphicConverter<Source>` dispatches on `value.GetType()` — handles new subclass automatically, zero converter changes.

**Verification:** `dotnet build`. Serialize a UrlSource — produces `{ "kind": "url" }`.

### Task 2: C# Plan Model — ValueProducer.ReadUrl factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

Add after the `Read` factory:

```csharp
/// <summary>Creates a ReadProducer that reads a URL query parameter by name.
/// Default shape is String because URL params are inherently strings.</summary>
internal static ValueProducer ReadUrl(string paramName, Shape shape = null) =>
    new ReadProducer(UrlSource.Instance, paramName, shape: shape ?? Shape.String);
```

Default shape `Shape.String` — callers override for typed reads (`FromUrl<int>` → `Shape.Number`).

### Task 3: C# Builder — TypedUrlSource<T> class

**New file:** `Alis.Reactive/Builders/Conditions/TypedUrlSource.cs`

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a URL query parameter from the browser's current location.
    /// Returned by <c>PipelineBuilder.FromUrl()</c> and <c>PipelineBuilder.FromUrl&lt;T&gt;()</c>.
    /// Plugs into all TypedSource&lt;T&gt; consumers: conditions, guards, branches, element ops, gather, headers, route params.
    /// </summary>
    public sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException(
                    "URL param name must not be null or whitespace.", nameof(paramName));
            _paramName = paramName;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.ReadUrl(_paramName, shape: Shape);
    }
}
```

**Design notes:**
- Param name validated in constructor — catches errors at the earliest point, before any builder uses it.
- `ToComponentSource()` and `ReadMember` are NOT overridden — they throw (TypedSource base). UrlSource is not a component.
- `Shape` property (TypedSource.cs) returns `Shape.FromClrType(typeof(TProp))` — shape flows automatically.

### Task 4: C# Builder — PipelineBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

Add using (if not already present):
```csharp
using Alis.Reactive.Builders.Conditions;
```

Add after existing methods:

```csharp
/// <summary>
/// Reads a query parameter from the browser's current URL as a string.
/// Use in conditions (<c>When(p.FromUrl("tab")).Eq("meds")</c>),
/// element ops (<c>Element("x").SetText(p.FromUrl("name"))</c>),
/// or as input to headers/route params.
/// </summary>
public TypedUrlSource<string> FromUrl(string paramName)
{
    return new TypedUrlSource<string>(paramName);
}

/// <summary>
/// Reads a query parameter with typed shape coercion.
/// Use <c>FromUrl&lt;int&gt;("page")</c> for numeric comparison,
/// <c>FromUrl&lt;bool&gt;("active")</c> for boolean checks.
/// </summary>
public TypedUrlSource<T> FromUrl<T>(string paramName)
{
    return new TypedUrlSource<T>(paramName);
}
```

### Task 5: C# Builder — GatherBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add after RouteParam methods:

```csharp
/// <summary>
/// Includes a URL query parameter value in the gather.
/// The parameter name is used as both the URL param to read and the HTTP request key.
/// </summary>
public GatherBuilder<TModel> FromUrl(string paramName)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException(
            "URL param name must not be null or whitespace.", nameof(paramName));
    var value = ValueProducer.ReadUrl(paramName);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}

/// <summary>
/// Includes a URL query parameter with an explicit HTTP request parameter name.
/// </summary>
public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
{
    if (string.IsNullOrWhiteSpace(paramName))
        throw new System.ArgumentException(
            "URL param name must not be null or whitespace.", nameof(paramName));
    if (string.IsNullOrWhiteSpace(asParam))
        throw new System.ArgumentException(
            "HTTP parameter name must not be null or whitespace.", nameof(asParam));
    var value = ValueProducer.ReadUrl(paramName);
    Fields.Add(GatherField.Of(asParam, value));
    return this;
}
```

**Verified references:**
- `ValueProducer.ReadUrl(string)` — added in Task 2
- `GatherField.Of(string, ValueProducer)` — Request.cs

### Task 6: JSON Schema — Source union gains UrlSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to Source oneOf:

```json
"Source": {
  "oneOf": [
    { "$ref": "#/$defs/ComponentSource" },
    { "$ref": "#/$defs/PayloadSource" },
    { "$ref": "#/$defs/UrlSource" }
  ]
},
```

Add new definition (after PayloadSource definition):

```json
"UrlSource": {
  "type": "object",
  "required": ["kind"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "url" }
  }
},
```

**Note:** `additionalProperties: false` — UrlSource has ONLY `kind`, no extra properties.

### Task 7: TS Types — Source union + UrlSource

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Expand Source union:

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource;
```

Add interface (after PayloadSource interface):

```typescript
export interface UrlSource {
  kind: "url";
}
```

### Task 8: TS Runtime — resolver.ts handles "url" kind

**File:** `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`

Add `"url"` case to `resolveSource` switch:

```typescript
case "url":
  return new URLSearchParams(window.location.search);
```

The returned `URLSearchParams` object is the "root." The `member` on ReadProducer navigates to the specific value via `.get(member)` in evaluate.ts.

### Task 9: TS Runtime — evaluate.ts handles URL source reads

**File:** `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

In the `"read"` case, add URL source branch between component and payload handling:

```typescript
// URL source: read query parameter by name
if (producer.from.kind === "url") {
  const params = root as URLSearchParams;
  const raw = params.get(producer.member);
  return raw == null ? raw : applyShape(raw, producer.shape);
}
```

**Why this is correct:**
- `URLSearchParams.get(name)` returns `string | null`
- Null for absent params → propagates as null (conditions `.IsNull()` work)
- String value → `applyShape` converts to target type based on shape
- No JsType lookup needed — URL source is not a component, has no properties/methods

### Task 10: Sandbox — 4 sections with exact element IDs

**Response DTOs** (`HttpShowcaseModel.cs`):
```csharp
public class UrlParamEchoResponse
{
    public string? Tab { get; set; }
    public string? FacilityId { get; set; }
    public string? Page { get; set; }
}
```

**Controller endpoint** (`HttpController.cs`):
```csharp
[HttpGet("UrlParamEcho")]
public IActionResult UrlParamEcho(string? tab, string? facilityId, string? page) =>
    Json(new { tab, facilityId, page });
```

**Page URL:** `/Sandbox/HttpPipeline/Http?tab=medications&facilityId=7&page=3`

The page must be navigated to WITH query params. The sandbox view reads them via `FromUrl()`.

#### Section 19: FromUrl in Gather

**Button:** "Send URL Params"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/UrlParamEcho")
 .Gather(g => g
     .FromUrl("tab")
     .FromUrl("facilityId"))
 .Response(r => r.OnSuccess<UrlParamEchoResponse>((json, s) =>
 {
     s.Element("url-gather-tab").SetText(json, x => x.Tab);
     s.Element("url-gather-facility").SetText(json, x => x.FacilityId);
     s.Element("url-gather-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `url-gather-tab` — expect: "medications"
- `url-gather-facility` — expect: "7"

#### Section 20: FromUrl in Conditions

**Auto-evaluates on page load (DomReady):**
```csharp
// Tab condition: show the correct panel based on ?tab
p.When(p.FromUrl("tab")).Eq("medications")
 .Then(t => t.Element("url-cond-meds").Show())
 .Else(e => e.Element("url-cond-meds").Hide());

// Numeric condition: show prev button when ?page > 1
p.When(p.FromUrl<int>("page")).Gt(1)
 .Then(t => t.Element("url-cond-prev").Show())
 .Else(e => e.Element("url-cond-prev").Hide());
```

**Element IDs:**
- `url-cond-meds` — starts hidden, shows when `?tab=medications` (expect: visible)
- `url-cond-prev` — starts hidden, shows when `?page > 1` (expect: visible since page=3)

#### Section 21: FromUrl in Pipeline (SetText)

**Auto-evaluates on DomReady:**
```csharp
p.Element("url-display-tab").SetText(p.FromUrl("tab"));
p.Element("url-display-facility").SetText(p.FromUrl("facilityId"));
```

**Element IDs:**
- `url-display-tab` — expect: "medications"
- `url-display-facility` — expect: "7"

#### Section 22: FromUrl Composed with Route Params + Headers

**Button:** "Compose All Sources"
**DSL:**
```csharp
p.Get("/Sandbox/HttpPipeline/Http/Residents/{id}")
 .Gather(g => g
     .RouteParam("id", 42)
     .Header("X-Tab", p.FromUrl("tab"))
     .FromUrl("facilityId", "facility"))
 .Response(r => r.OnSuccess<ResidentByIdResponse>((json, s) =>
 {
     s.Element("url-compose-name").SetText(json, x => x.Name);
     s.Element("url-compose-result").AddClass("text-green-600");
 }))
```

**Element IDs:**
- `url-compose-name` — expect: "Resident #42"

### Task 11: C# Unit Tests (16 tests) — `WhenReadingUrlParams.cs`

**File:** `tests/Alis.Reactive.UnitTests/Http/WhenReadingUrlParams.cs`

| Test | What It Proves |
|---|---|
| `from_url_string_produces_url_source_read` | `FromUrl("tab")` → `{ kind: "read", from: { kind: "url" }, member: "tab", shape: { kind: "string" } }` + AssertSchemaValid |
| `from_url_typed_int_carries_number_shape` | `FromUrl<int>("page")` → shape: { kind: "number" } + AssertSchemaValid |
| `from_url_typed_bool_carries_boolean_shape` | `FromUrl<bool>("active")` → shape: { kind: "boolean" } + AssertSchemaValid |
| `from_url_typed_datetime_carries_date_shape` | `FromUrl<DateTime>("since")` → shape: { kind: "date" } + AssertSchemaValid |
| `from_url_gather_produces_gather_field` | `.FromUrl("facilityId")` in Gather → GatherField with key "facilityId" + AssertSchemaValid |
| `from_url_gather_with_alias_uses_alias_as_key` | `.FromUrl("unitId", "unit")` → GatherField key is "unit" + AssertSchemaValid |
| `from_url_in_condition_produces_compare` | `When(p.FromUrl("tab")).Eq("meds")` → CompareCondition with left: { from: { kind: "url" } } + AssertSchemaValid |
| `from_url_typed_in_condition_produces_shaped_compare` | `When(p.FromUrl<int>("page")).Gt(1)` → CompareCondition with shape: { kind: "number" } + AssertSchemaValid |
| `from_url_in_set_text_produces_set_reaction` | `Element("x").SetText(p.FromUrl("tab"))` → SetReaction with value: { from: { kind: "url" } } + AssertSchemaValid |
| `plan_without_url_source_has_no_url_kind` | Normal plan → JSON does NOT contain `"kind": "url"` |
| `from_url_composes_with_route_params_and_headers` | Same request: RouteParam + Header + FromUrl → all three in JSON |
| `from_url_as_route_param_value` | `RouteParam("facilityId", p.FromUrl<int>("facilityId"))` → routeParams value is url-source read |
| `from_url_as_header_value` | `Header("X-Tab", p.FromUrl("tab"))` → headers value is url-source read |
| `empty_param_name_throws_in_pipeline` | `p.FromUrl("")` → ArgumentException |
| `empty_param_name_throws_in_gather` | `.FromUrl("")` → ArgumentException |
| `empty_alias_throws_in_gather` | `.FromUrl("tab", "")` → ArgumentException |

### Task 12: vitest Tests (4 tests) — `core/evaluate-url.test.ts`

**File:** `Alis.Reactive.SandboxApp/Scripts/__tests__/core/evaluate-url.test.ts`

| Test | What It Proves |
|---|---|
| `url source returns string value from URLSearchParams` | Basic read: `params.get("tab")` → "meds" |
| `url source returns null for absent param` | Missing param → null propagation |
| `url source applies number shape to string value` | `applyShape("42", numberShape)` → 42 |
| `url source applies boolean shape to string value` | `applyShape("true", boolShape)` → true |

Note: Mock `resolveSource` to return a pre-built `URLSearchParams`. Mock `applyShape` to verify it's called with correct shape.

### Task 13: Playwright Tests (8 tests) — `WhenUrlParamsRead.cs`

**File:** `tests/Alis.Reactive.PlaywrightTests/HttpPipeline/WhenUrlParamsRead.cs`

Navigate to `/Sandbox/HttpPipeline/Http?tab=medications&facilityId=7&page=3`.

**Section 19 — FromUrl in Gather:**

| Test | Click | Assert |
|---|---|---|
| `url_param_sent_as_gather_field` | "Send URL Params" | `#url-gather-tab` → "medications", `#url-gather-facility` → "7" |
| `url_gather_applies_success_class` | "Send URL Params" | `#url-gather-result` has class `text-green-600` |

**Section 20 — FromUrl in Conditions (DomReady — no click):**

| Test | Assert (after boot) |
|---|---|
| `url_condition_string_eq_shows_correct_panel` | `#url-cond-meds` is visible |
| `url_condition_numeric_gt_shows_prev_button` | `#url-cond-prev` is visible |

**Section 21 — FromUrl in SetText (DomReady — no click):**

| Test | Assert (after boot) |
|---|---|
| `url_param_displayed_in_element_text` | `#url-display-tab` → "medications", `#url-display-facility` → "7" |

**Section 22 — Composition:**

| Test | Click | Assert |
|---|---|---|
| `url_param_composes_with_route_param_and_header` | "Compose All Sources" | `#url-compose-name` → "Resident #42" |
| `url_compose_applies_success_class` | "Compose All Sources" | `#url-compose-result` has class `text-green-600` |

**Missing param test:**

| Test | Navigate to | Assert |
|---|---|---|
| `missing_url_param_returns_null` | `/Sandbox/HttpPipeline/Http` (NO query params) | `#url-cond-meds` is hidden, `#url-display-tab` → "—" (placeholder stays) |

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates plans with UrlSource (`additionalProperties: false` on UrlSource definition)
- [ ] `FromUrl("param")` in gather sends URL param value as HTTP param
- [ ] `FromUrl("param", "alias")` uses alias as HTTP param name
- [ ] `When(p.FromUrl("tab")).Eq("meds")` condition evaluates correctly
- [ ] `When(p.FromUrl<int>("page")).Gt(1)` does numeric comparison (not string)
- [ ] `p.Element("x").SetText(p.FromUrl("name"))` displays URL param value
- [ ] Missing URL param returns null — `.IsNull()` works, conditions don't show panel
- [ ] Shape.String is default — no explicit shape needed for string comparisons
- [ ] Shape.Number on `FromUrl<int>` enables correct numeric comparison
- [ ] Shape.Boolean on `FromUrl<bool>` enables correct truthiness
- [ ] Composes with Headers (`Header("X-Tab", p.FromUrl("tab"))`)
- [ ] Composes with Route Params (`RouteParam("id", p.FromUrl<int>("id"))`)
- [ ] Empty param name throws ArgumentException in both PipelineBuilder and GatherBuilder
- [ ] All 16 C# unit tests pass (Task 11)
- [ ] All 4 vitest tests pass (Task 12)
- [ ] All 8 Playwright tests pass (Task 13)
- [ ] All existing 799+ Playwright tests pass (no regressions)
