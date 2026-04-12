# ReadProducer Args — Method Reads With Arguments

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Allow `evaluateValue` to call methods WITH arguments and return the result. Today, method reads are zero-arg only (`callMethod(root, method, [])`). This extends ReadProducer with optional `args` so any source (component, plugin, payload, URL) can call methods with args and use the return value.

**This is NOT a plugin-specific feature.** It's a shared framework concept that happens to be needed by Plugin Source. Once landed, any component method read can also pass args.

**Tech Stack:** C# plan model, JSON schema, TypeScript types + runtime

---

## The Gap

The framework already supports all four JS object operations:

| Operation | Mechanism | Works? |
|---|---|---|
| Read property | `evaluateValue` → `readProperty(root, prop)` | ✓ |
| Write property | `executeSet` → `setProperty(root, prop, value)` | ✓ |
| Call method with args | `executeCall` → `callMethod(root, method, args)` | ✓ |
| **Call method with args + return value** | `evaluateValue` → `callMethod(root, method, [])` | **❌ hardcoded `[]`** |

`callMethod` (resolver.ts:125) already accepts args and returns a value:
```typescript
export function callMethod(root: unknown, method: Method, args: unknown[]): unknown {
  const { fn, owner } = resolveCallable(root, method.path);
  return fn.apply(owner, args);  // args IN, return value OUT — already works
}
```

The ONLY change: `evaluateValue` passes `producer.args` instead of `[]`.

---

## Step-by-Step Implementation

### Task 1: C# Plan Model — ReadProducer gains optional Args

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

```csharp
public sealed class ReadProducer : ValueProducer
{
    public string Kind => "read";
    public Source From { get; }
    public string Member { get; }
    public Path Path { get; }
    public Shape Shape { get; }
    public IReadOnlyList<ValueProducer> Args { get; }  // NEW — null when no args

    internal ReadProducer(Source from, string member, Path path, Shape shape,
        List<ValueProducer> args = null)  // NEW param with default
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        Member = member ?? throw new ArgumentNullException(nameof(member));
        Path = path == null || path.IsNone ? null : path;
        Shape = shape == null || shape.IsNone ? null : shape;
        Args = args != null && args.Count > 0 ? args : null;  // null when empty — omitted from JSON
    }
}
```

Backward compatible — existing callers pass no `args` → default `null` → field omitted from JSON.

### Task 2: C# Plan Model — ValueProducer.Read gains args param

**File:** `Alis.Reactive/PlanModel/ValueProducer.cs`

Update existing factory:
```csharp
internal static ValueProducer Read(Source from, string member, Path path = null, 
    Shape shape = null, List<ValueProducer> args = null) =>
    new ReadProducer(from, member, path, shape, args);
```

One new optional param. All existing callers unchanged.

### Task 3: JSON Schema — ReadProducer gains args

**File:** `Alis.Reactive/Schemas/reactive-plan.schema.json`

Add to ReadProducer properties:
```json
"args": {
  "type": "array",
  "items": { "$ref": "#/$defs/ValueProducer" }
}
```

Optional — not in `required`. Plans without args validate as before.

### Task 4: TS Types — ReadProducer gains args

**File:** `Alis.Reactive.SandboxApp/Scripts/types/plan.ts`

```typescript
export interface ReadProducer {
  kind: "read";
  from: Source;
  member: string;
  path?: Path;
  shape?: Shape;
  args?: ValueProducer[];  // NEW
}
```

### Task 5: TS Runtime — evaluateValue passes args

**File:** `Alis.Reactive.SandboxApp/Scripts/core/evaluate.ts`

Change the method call in the component/plugin branch:

```typescript
// BEFORE:
const raw = callMethod(root, method, []);

// AFTER:
const evaluatedArgs = producer.args
  ? producer.args.map(a => evaluateValue(a, plan, ctx))
  : [];
const raw = callMethod(root, method, evaluatedArgs);
```

Backward compatible — `producer.args` is undefined for existing plans → `[]` → same behavior.

### Task 6: C# Unit Tests

| Test | What It Proves |
|---|---|
| `read_without_args_omits_args_field` | Existing behavior unchanged — no args in JSON |
| `read_with_args_includes_args_in_json` | Args serialize as ValueProducer array + AssertSchemaValid |
| `read_with_literal_args_produces_correct_json` | Literal args carry shape |
| `read_with_component_source_args` | Component read as arg |

### Task 7: vitest Tests

| Test | What It Proves |
|---|---|
| `method read with zero args works as before` | Backward compat |
| `method read evaluates args and passes to callMethod` | Args flow through |
| `method read with multiple args passes all` | Multi-arg |

---

## Verification Checklist

- [ ] `dotnet build` — 0 errors
- [ ] `npm run typecheck` — clean
- [ ] `npm run build` — bundle builds
- [ ] Plans without args validate (backward compat)
- [ ] Plans with args validate against schema
- [ ] `evaluateValue` passes evaluated args to `callMethod`
- [ ] Existing zero-arg method reads work unchanged
- [ ] All existing tests pass (no regressions)
