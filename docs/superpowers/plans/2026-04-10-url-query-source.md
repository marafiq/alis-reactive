# URL Query Parameter Source (UrlSource)

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow devs to READ values from the browser's current URL query string (`window.location.search`) and use them anywhere ValueProducer is accepted — gather, conditions, pipeline commands. This is a new value SOURCE, not a new value kind.

**Tech Stack:** C# plan model + Source union, JSON schema, TypeScript types + runtime resolver

**Prerequisite:** None. This plan is self-contained but should land BEFORE Plugin Source (which also widens the Source union — doing URL first validates the pattern on a simpler case).

---

## Architecture

A new `UrlSource` kind is added to the `Source` union (alongside `ComponentSource` and `PayloadSource`). The `member` field on `ReadProducer` serves as the query parameter name. At runtime, `resolveSource` dispatches to `new URLSearchParams(window.location.search)`, and `evaluateValue` reads the specific param via `.get(member)`.

Shape is critical: URL query params are inherently strings. Without shape, `"42" > "1"` is string comparison (wrong). With `Shape.Number`, `applyShape("42", Shape.Number)` converts to `42`, enabling correct numeric comparison.

### DSL — Four Usage Contexts

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

1. **UrlSource is a singleton** — no per-instance state. Unlike ComponentSource (carries component ID via `Component` property — Source.cs:15) or PayloadSource (carries `Scope` and `Type` — Source.cs:28-29), the URL source is always `window.location.search`. The query param name is the `member` on ReadProducer (ValueProducer.cs:73).

2. **TypedUrlSource<T> extends TypedSource<T>** — plugs into ALL existing typed infrastructure with ZERO changes to downstream consumers:
   - `ConditionSourceBuilder` via `When<TProp>(TypedSource<TProp>)` — PipelineBuilder already has this
   - `ElementBuilder.SetText<TProp>(TypedSource<TProp>)` — ElementBuilder already has this
   - Guards (And/Or) and branches (ElseIf) — all accept TypedSource<T>

3. **FromUrl<int>("page")** uses `Shape.FromClrType(typeof(int))` = `Shape.Number` (Shape.cs:63) — `applyShape` converts the URL string "42" to number 42 at runtime (core/shape-convert.ts).

4. **Null semantics** — `URLSearchParams.get(name)` returns `null` for absent params. This flows through the existing `raw == null ? raw : applyShape(...)` pattern in evaluateValue (core/evaluate.ts:28,33,42,46). Conditions like `.IsNull()` and `.NotNull()` work correctly.

5. **Source union widening** — Adding UrlSource to the Source union means `SetReaction.on` and `CallReaction.on` could theoretically target a URL. This is semantically invalid but harmless: the C# builders won't generate it, and the runtime would throw (fail-fast). No special guard needed.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — Add UrlSource to Source union

**File:** `Alis.Reactive/PlanModel/Source.cs`

After `PayloadSource` class (after line 43), add:

```csharp
public sealed class UrlSource : Source
{
    public string Kind => "url";
    private UrlSource() { }
    internal static UrlSource Instance { get; } = new UrlSource();
}
```

Singleton — one instance, no state. The `WriteOnlyPolymorphicConverter<Source>` (Serialization/WriteOnlyPolymorphicConverter.cs:9-10) dispatches on `value.GetType()` — handles new subclass automatically, zero converter changes.

**Verification:** `dotnet build`. Serialize a UrlSource — produces `{ "kind": "url" }`.

### Task 2: C# Plan Model — ValueProducer.ReadUrl factory

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

Add after the `Read` factory (after line 46):

```csharp
internal static ValueProducer ReadUrl(string paramName, Shape shape = null) =>
    new ReadProducer(UrlSource.Instance, paramName, shape: shape ?? Shape.String);
```

Default shape is `Shape.String` because URL params are inherently strings. Callers override for typed reads (e.g., `FromUrl<int>` → `Shape.Number`).

### Task 3: C# Builder — TypedUrlSource<T> class

**New file:** `Alis.Reactive/Builders/Conditions/TypedUrlSource.cs`

```csharp
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a URL query parameter from the browser's current location.
    /// Returned by <c>PipelineBuilder.FromUrl()</c> and <c>PipelineBuilder.FromUrl&lt;T&gt;()</c>.
    /// </summary>
    public sealed class TypedUrlSource<TProp> : TypedSource<TProp>
    {
        private readonly string _paramName;

        internal TypedUrlSource(string paramName)
        {
            _paramName = paramName;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.ReadUrl(_paramName, shape: Shape);
    }
}
```

Extends `TypedSource<TProp>` (TypedSource.cs:11) — automatically plugs into all typed infrastructure. The `Shape` property on TypedSource (TypedSource.cs:33) returns `Shape.FromClrType(typeof(TProp))`.

`ToComponentSource()` and `ReadMember` are NOT overridden — they throw (TypedSource.cs:22,28). This is correct: UrlSource is not a component source.

### Task 4: C# Builder — PipelineBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs`

Add using:
```csharp
using Alis.Reactive.Builders.Conditions;
```

Add after `Component<TComponent>()` overloads (after line 78):

```csharp
/// <summary>
/// Reads a query parameter from the browser's current URL as a string.
/// </summary>
public TypedUrlSource<string> FromUrl(string paramName)
{
    return new TypedUrlSource<string>(paramName);
}

/// <summary>
/// Reads a query parameter with typed shape coercion.
/// Use <c>FromUrl&lt;int&gt;("page")</c> for numeric comparison.
/// </summary>
public TypedUrlSource<T> FromUrl<T>(string paramName)
{
    return new TypedUrlSource<T>(paramName);
}
```

### Task 5: C# Builder — GatherBuilder.FromUrl()

**File:** `Alis.Reactive/Builders/Requests/GatherBuilder.cs`

Add after existing methods (after RouteParam overloads if those landed, or after FromEvent):

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

**Verified references:**
- `ValueProducer.ReadUrl(string)` — added in Task 2
- `GatherField.Of(string, ValueProducer)` — Request.cs:89-90

### Task 6: JSON Schema — Source union gains UrlSource

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to Source oneOf (line 141-146):

```json
"Source": {
  "oneOf": [
    { "$ref": "#/$defs/ComponentSource" },
    { "$ref": "#/$defs/PayloadSource" },
    { "$ref": "#/$defs/UrlSource" }
  ]
},
```

Add new definition (after PayloadSource definition, after line 166):

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

**Note:** `additionalProperties: false` as per quality bar. UrlSource has ONLY `kind`.

### Task 7: TS Types — Source union + UrlSource

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

Expand Source union (line 88):

```typescript
export type Source = ComponentSource | PayloadSource | UrlSource;
```

Add interface (after PayloadSource interface, after line 101):

```typescript
export interface UrlSource {
  kind: "url";
}
```

### Task 8: TS Runtime — resolver.ts handles "url" kind

**File:** `Alis.Reactive.SandboxApp/Scripts/resolution/resolver.ts`

Add `"url"` case to `resolveSource` switch (line 29-37):

```typescript
export function resolveSource(plan: Plan, source: Source, ctx?: ExecContext): unknown {
  switch (source.kind) {
    case "component":
      return resolveComponent(plan, source.component);
    case "payload":
      return resolvePayload(source, ctx);
    case "url":
      return new URLSearchParams(window.location.search);
    default:
      assertNever(source, "source kind");
  }
}
```

The returned `URLSearchParams` object is the "root" — same concept as a component root (ej2 instance) or payload root (event data). The `member` on ReadProducer navigates to the specific value.

**Note:** The `assertNever` at line 35 currently catches the default. Adding "url" keeps exhaustiveness — `assertNever` fires for truly unknown kinds.

### Task 9: TS Runtime — evaluate.ts handles URL source reads

**File:** `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

In the `"read"` case (line 19), add URL source branch between component and payload handling. After the component block (after line 35), before the payload walk:

```typescript
// URL source: read query parameter by name
if (producer.from.kind === "url") {
  const params = root as URLSearchParams;
  const raw = params.get(producer.member);
  return raw == null ? raw : applyShape(raw, producer.shape);
}
```

`URLSearchParams.get(name)` returns `string | null`. Null propagation matches the existing pattern (line 28: `raw == null ? raw : applyShape(...)`). `applyShape` converts the string to the target type based on shape:
- `Shape.Number` → `Number("42")` = 42
- `Shape.Boolean` → `"true"` → true
- `Shape.Date` → ISO string → Date → timestamp
- `Shape.String` (default) → passthrough

### Task 10: Playwright test

Test: navigate to `/Sandbox/Test?tab=meds&page=2`. Verify:
- `FromUrl("tab")` reads "meds"
- `FromUrl<int>("page").Gt(1)` evaluates to true (numeric comparison)
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
