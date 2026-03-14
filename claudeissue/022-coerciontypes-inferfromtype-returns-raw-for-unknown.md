# FAIL-FAST-022: CoercionTypes.InferFromType Returns "raw" for Unknown Types

## Status: Fail-Fast Violation (Rule 8)

## File
`Alis.Reactive/Descriptors/Guards/Guard.cs:49-62`

## How to Reproduce

1. Create a model with a `Guid` property:
   ```csharp
   public class MyModel { public Guid TransactionId { get; set; } }
   ```
2. Use it in a condition:
   ```csharp
   p.When(payload, x => x.TransactionId).Eq(someGuid).Then(t => { ... });
   ```
3. `InferFromType(typeof(Guid))` is called.
4. `Guid` is not `string`, not `bool`, not numeric, not `DateTime`, not `DateOnly`, not an enum.
5. Line 61: `return Raw;` — the coercion type is `"raw"`.
6. At runtime, `coerce(value, "raw")` returns the value as-is — no coercion.
7. The guard compares the raw JSON-deserialized value (a string like `"3fa85f64-5717-4562-b3fc-2c963f66afa6"`) against the operand. The comparison semantics depend on JavaScript's `===` operator on whatever types happen to be present.

## Deep Reasoning: Why This Is a Real Issue

The `"raw"` coercion is a catch-all that means "I don't know what type this is, so don't touch it." This is a fallback, and the framework's Rule 8 says "No Fallbacks — Fail Fast."

The danger is subtle: `"raw"` coercion makes the guard work for some types by accident and fail for others without warning. For example:
- `Guid` → JSON deserializes as a string → `"raw"` works because string `===` comparison happens to be correct
- `TimeSpan` → JSON serializes as `"00:15:00"` → `"raw"` works for equality but fails for ordering (`"gt"` compares strings lexicographically)
- `object` → could be anything → `"raw"` produces unpredictable results

The correct behavior is to throw at build time: "Unsupported type for guard coercion: Guid. Supported types: string, bool, int, long, double, float, decimal, short, byte, DateTime, DateTimeOffset, DateOnly, enums." This tells the developer to either use a supported type or to add explicit coercion support for their type.

## How Fixing This Improves the Codebase

1. **No silent "works sometimes" behavior**: The developer knows at build time that their type needs explicit coercion mapping.
2. **Extensibility story**: If `Guid` should be supported, it should be mapped to `"string"` coercion explicitly. The fix forces this decision rather than hiding it behind `"raw"`.
3. **Consistent with typed conditions**: The `TypedSource<TProp>` and `ConditionSourceBuilder<TModel, TProp>` design was built for type safety. A raw fallback undermines this design.

## How This Fix Will Not Break Existing Features

- Every type currently used in conditions (int, long, double, string, bool, decimal, DateTime, enums) is already handled and mapped to a specific coercion type. The `"raw"` branch is never reached by existing code.
- If any existing code path does reach `"raw"` (which would require a type not in the handled set), it is already producing silently unreliable guard evaluations. Throwing surfaces this latent bug.
- The fix can be phased: first add a trace warning, then upgrade to a throw after confirming no existing code reaches the branch.
