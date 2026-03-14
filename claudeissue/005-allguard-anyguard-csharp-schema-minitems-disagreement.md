# BUG-005: AllGuard/AnyGuard C# Allows 1 Guard, Schema Requires minItems: 2

## Status: Bug — Build-Time / Schema Contract Disagreement

## Files
- `Alis.Reactive/Descriptors/Guards/Guard.cs:96-97` (C# validation)
- `Alis.Reactive/Schemas/reactive-plan.schema.json:209,222` (JSON schema)

## How to Reproduce

1. Create a plan with a single guard inside an `And()` / `Or()` combination:
   ```csharp
   p.When(payload, x => x.Value)
    .Eq("active")
    .And()              // Creates AllGuard with [inner guard] — count=1
    .Then(t => { ... });
   ```
   Internally, `GuardBuilder.FlattenAllStatic` may produce an `AllGuard` with a single child if the builder chain contains only one guard before the `And()`.

2. The C# code accepts this — `Guard.cs:96` validates `guards.Count == 0` but allows `guards.Count == 1`:
   ```csharp
   if (guards == null || guards.Count == 0)
       throw new ArgumentException("AllGuard requires at least one guard.", nameof(guards));
   ```

3. The JSON schema requires `minItems: 2`:
   ```json
   "guards": { "type": "array", "items": { "$ref": "#/$defs/Guard" }, "minItems": 2 }
   ```

4. `plan.Render()` produces valid C# output that **fails schema validation**. The `AllPlansConformToSchema` test catches this, but only if a test exercises this exact path.

## Deep Reasoning: Why This Is a Real Bug

The JSON schema is the **single contract** between C# and JS (CLAUDE.md: "The plan is the only contract between C# and JS"). When the C# layer can produce JSON that the schema rejects, the contract is broken.

Semantically, an `AllGuard` with one child is logically equivalent to just the child guard — there is no need for a wrapper. The schema is correct to require at least 2 because a single-child `AllGuard` is wasted nesting. The C# validation message says "at least one" when the schema says "at least two" — the error message itself is inconsistent with the contract.

This matters because `AllPlansConformToSchema` is meant to be the comprehensive correctness gate. If the C# layer can produce plans that pass C# validation but fail schema validation, the gate has a hole.

## How Fixing This Improves the Codebase

1. **Contract alignment**: C# validation and JSON schema agree on the invariant.
2. **Simpler plans**: A single-child AllGuard is flattened to just the child, producing smaller JSON.
3. **Error message accuracy**: The exception message will match the actual contract.

## How This Fix Will Not Break Existing Features

- The `FlattenAllStatic`/`FlattenAnyStatic` methods in `GuardBuilder` already optimize guard trees. Adding a "single-child unwrap" step is consistent with existing flattening behavior.
- If the C# validation is tightened to require `>= 2`, the builder must ensure it never creates single-child groups. The existing `And()`/`Or()` builder chain already accumulates guards, so this should only trigger if the developer writes a degenerate chain.
- The fix does not change the schema — the schema is already correct.
- Existing tests that produce multi-guard groups will continue to pass unchanged.
