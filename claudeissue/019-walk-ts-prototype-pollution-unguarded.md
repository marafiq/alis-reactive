# SECURITY-019: walk.ts Has No Prototype Pollution Guard

## Status: Security Concern

## File
`Scripts/walk.ts:11-18`

## How to Reproduce

1. Consider a plan where an event payload contains a user-controlled property name.
2. The event fires with detail: `{ "__proto__": { "isAdmin": true } }`.
3. The resolver walks: `walk(ctx, "evt.__proto__.isAdmin")` → traverses the prototype chain.
4. While this specific attack would require user-controlled dot-paths (which the C# DSL prevents), the `walk` function itself has no defense.

More practically:
```typescript
walk({}, "constructor")          // returns Object constructor
walk({}, "constructor.prototype") // returns Object.prototype
walk({}, "__proto__")             // returns Object.prototype
```

## Deep Reasoning: Why This Is a Real Issue

The `walk()` function is the framework's universal dot-path resolver. It is used by:
- `resolver.ts` — resolves event payload paths (`evt.address.city`)
- `component.ts` — resolves component read expressions (`value`, `checked`)
- `validation.ts` — resolves validation field values
- `conditions.ts` (via resolver) — resolves guard source values

Currently, all dot-paths come from the C# DSL (`ExpressionPathHelper`) which generates paths from C# property expressions. These paths are always safe (e.g., `evt.address.city`). The C# side cannot generate `__proto__` or `constructor` paths.

However, `walk()` is a **shared primitive** with no knowledge of where its inputs come from. If a future feature introduces user-controlled paths (e.g., a dynamic binding expression, a configurable display template, or a custom event payload constructed from user input), the `walk` function would blindly traverse the prototype chain.

The defense-in-depth principle says: even if the current call sites are safe, the function should guard against known dangerous paths. This is especially important for a framework that will scale to 100+ components and potentially be used by external developers.

## How Fixing This Improves the Codebase

1. **Defense in depth**: Guard against `__proto__`, `constructor`, and `prototype` segments:
   ```typescript
   const BANNED = new Set(["__proto__", "constructor", "prototype"]);
   for (const part of parts) {
     if (BANNED.has(part)) return undefined; // or throw
   }
   ```
2. **Consistent with fail-fast**: The guard makes the function's safety contract explicit.
3. **No performance impact**: A `Set.has()` check per path segment is negligible.

## How This Fix Will Not Break Existing Features

- No existing dot-path in the framework contains `__proto__`, `constructor`, or `prototype`. These are JavaScript runtime properties that have no equivalent in C# model property names.
- The `ExpressionPathHelper` generates paths like `evt.address.city`, `evt.intValue`, `responseBody.name` — all lowercase first-segment (camelCase) followed by PascalCase property names. None of these match the banned set.
- The guard is a pure addition — it only rejects paths that were never valid inputs.
