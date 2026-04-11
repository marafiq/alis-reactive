# URL Query Parameter Source (UrlSource)

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to READ values from the browser's current URL query string (`window.location.search`) and use them anywhere ValueProducer is accepted — gather, conditions, pipeline commands. This is a new value SOURCE, not a new value kind.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver

---

## Architecture

A new `UrlSource` kind is added to the `Source` union (alongside `ComponentSource` and `PayloadSource`). The `member` field on `ReadProducer` serves as the query parameter name. At runtime, `resolveSource` dispatches to `new URLSearchParams(window.location.search)`, and `evaluateValue` reads the specific param via `.get(member)`.

Shape is critical: URL query params are inherently strings. Without shape, `"42" > "1"` is string comparison (wrong). With `Shape.Number`, `applyShape("42", Shape.Number)` converts to `42`, enabling correct numeric comparison.

### DSL — Three Usage Contexts

```csharp
// 1. GATHER: pass URL param as HTTP request param
p.Get("/api/data")
 .Gather(g => g
     .FromUrl("facilityId")                          // reads ?facilityId, sends as facilityId
     .FromUrl("unitId", "unit"))                     // reads ?unitId, sends as "unit"

// 2. CONDITIONS: branch based on URL param
p.When(p.FromUrl("tab")).Eq("medications").Then(t => { ... })

// 3. PIPELINE: display URL param value
p.Element("facility-name").SetText(p.FromUrl("facilityId"))

// 4. TYPED: numeric URL param with comparison
p.When(p.FromUrl<int>("page")).Gt(1).Then(t => {
    t.Element("prev-button").Show();
})
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

### Key Design Decisions

1. **UrlSource is a singleton** — no per-instance state. Unlike ComponentSource (carries component ID) or PayloadSource (carries scope), the URL source is always `window.location.search`. The query param name is the `member` on ReadProducer.

2. **TypedUrlSource<T> extends TypedSource<T>** — plugs into ALL existing typed infrastructure with ZERO changes: conditions (`When<T>(TypedSource<T>)`), guards (`And/Or<T>(TypedSource<T>)`), branches (`ElseIf<T>(TypedSource<T>)`), element operations (`SetText<T>(TypedSource<T>)`).

3. **FromUrl<int>("page")** uses `Shape.FromClrType(typeof(int))` = `Shape.Number` — applyShape converts the URL string "42" to number 42 at runtime.

4. **Null semantics** — `URLSearchParams.get(name)` returns `null` for absent params. This flows through the existing `raw == null ? raw : applyShape(...)` pattern in evaluateValue. Conditions like `.IsNull()` and `.NotNull()` work correctly.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add UrlSource to Source union

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `PayloadSource` class, add:

```csharp
public sealed class UrlSource : Source
{
    public string Kind => "url";
    private UrlSource() { }
    internal static UrlSource Instance { get; } = new UrlSource();
}
```

Singleton — one instance, no state. The `WriteOnlyPolymorphicConverter<Source>` handles serialization automatically (dispatches on runtime type).

**Verification:** `dotnet build`. Serialize a UrlSource — produces `{ "kind": "url" }`.

### Task 2: C# Plan Model — ValueProducer.ReadUrl factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

Add after the `Read` factory:

```csharp
internal static ValueProducer ReadUrl(string paramName, Shape shape = null) =>
    new ReadProducer(UrlSource.Instance, paramName, shape: shape ?? Shape.String);
```

Default shape is `Shape.String` because URL params are inherently strings. Callers override for typed reads.

### Task 3: C# Builder — TypedUrlSource<T> class

**New file:** `Alis.Reactive/Builders/Conditions/TypedUrlSource.cs`

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a URL query parameter from the browser's current location.
    /// </summary>
    public sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            _paramName = paramName;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.Read(UrlSource.Instance, _paramName, shape: Shape);

        internal string ParamName => _paramName;
    }
}
```

Extends `TypedSource<TProp>` — automatically plugs into:
- `ConditionSourceBuilder` via `When<TProp>(TypedSource<TProp>)`
- `ElementBuilder.SetText<TProp>(TypedSource<TProp>)`
- `GuardBuilder.And/Or<TProp>(TypedSource<TProp>)`
- `BranchBuilder.ElseIf<TProp>(TypedSource<TProp>)`

**Zero changes needed** in any of these files.

### Task 4: C# Builder — PipelineBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

Add after `Component<T>()` overloads:

```csharp
/// <summary>
/// Reads a query parameter from the browser's current URL.
/// </summary>
public TypedUrlSource<string> FromUrl(string paramName)
{
    return new TypedUrlSource<string>(paramName);
}

/// <summary>
/// Reads a query parameter with typed shape coercion.
/// Use FromUrl&lt;int&gt;("page") for numeric comparison.
/// </summary>
public TypedUrlSource<T> FromUrl<T>(string paramName)
{
    return new TypedUrlSource<T>(paramName);
}
```

Add `using Alis.Reactive.Builders.Conditions;`.

### Task 5: C# Builder — GatherBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add after existing methods:

```csharp
/// <summary>
/// Includes a URL query parameter value in the gather.
/// The parameter name is used as both the URL param to read and the HTTP request key.
/// </summary>
public GatherBuilder<TModel> FromUrl(string paramName)
{
    var value = ValueProducer.ReadUrl(paramName);
    Fields.Add(GatherField.Of(paramName, value));
    return this;
}

/// <summary>
/// Includes a URL query parameter with an explicit HTTP request parameter name.
/// </summary>
public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
{
    var value = ValueProducer.ReadUrl(paramName);
    Fields.Add(GatherField.Of(asParam, value));
    return this;
}
```

### Task 6: JSON Schema — Source union gains UrlSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to Source oneOf:

```json
{ "$ref": "#/$defs/UrlSource" }
```

Add new definition:

```json
"UrlSource": {
  "type": "object",
  "required": ["kind"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "url" }
  }
}
```

### Task 7: TS Types — Source union + UrlSource

**File:** `Scripts/types/plan.ts`

Expand Source union:

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource;
```

Add interface:

```typescript
export interface UrlSource {
  kind: "url";
}
```

### Task 8: TS Runtime — resolver.ts handles "url" kind

**File:** `Scripts/resolution/resolver.ts`

Add `"url"` case to `resolveSource` switch:

```typescript
case "url":
  return new URLSearchParams(window.location.search);
```

The returned `URLSearchParams` object is the "root" — same concept as component root (ej2 instance) or payload root (event data). The `member` navigates to the specific value.

### Task 9: TS Runtime — evaluate.ts handles URL source reads

**File:** `Scripts/core/evaluate.ts`

Add URL source branch between component and payload handling:

```typescript
// URL source: read query parameter by name
if (producer.from.kind === "url") {
  const params = root as URLSearchParams;
  const raw = params.get(producer.member);
  return raw == null ? raw : applyShape(raw, producer.shape);
}
```

`URLSearchParams.get(name)` returns `string | null`. Null propagation matches the existing pattern. `applyShape` converts the string to the target type (number, boolean, date) based on shape.

### Task 10: Playwright test

Test: navigate to `/Sandbox/Test?tab=meds&page=2`. Verify:
- `FromUrl("tab")` reads "meds"
- `FromUrl<int>("page").Gt(1)` evaluates to true
- `FromUrl("missing")` returns null — `.IsNull()` is true

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Schema validates plans with UrlSource
- [ ] `FromUrl("param")` in gather sends URL param value as HTTP param
- [ ] `FromUrl("param", "alias")` uses alias as HTTP param name
- [ ] `When(p.FromUrl("tab")).Eq("meds")` condition evaluates correctly
- [ ] `When(p.FromUrl<int>("page")).Gt(1)` does numeric comparison (not string)
- [ ] `p.Element("x").SetText(p.FromUrl("name"))` displays URL param value
- [ ] Missing URL param returns null — `.IsNull()` works, `.NotNull()` works
- [ ] Shape.String is default — no explicit shape needed for string comparisons
- [ ] Composes with Headers and URL Templates (all three use ValueProducer)
- [ ] All unit + Playwright tests pass
