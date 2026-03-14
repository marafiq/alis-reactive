# DESIGN-013: NativeCheckBoxBuilder Compiles Expression Per Render + Bare Catch Swallows All Exceptions

## Status: Design Issue + Fail-Fast Violation (Rule 8)

## File
`Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxBuilder.cs:110-126`

## How to Reproduce

### Part A — Expression Compiled Per Render
1. Use a model-bound checkbox in a Razor view:
   ```csharp
   @Html.NativeCheckBoxFor(plan, m => m.IsActive)
   ```
2. Every time this view renders, `WriteTo()` is called.
3. Line 112: `var compiled = _expression.Compile();` invokes the Expression tree compiler.
4. `Expression.Compile()` generates IL at runtime — it is one of the most expensive operations in .NET, typically taking 100-500 microseconds per call.
5. For a page with 10 checkboxes, this adds 1-5ms of pure compilation overhead per render.

### Part B — Bare Catch Swallows Everything
1. If the model property throws during evaluation (e.g., a property getter that accesses a disposed DbContext):
   ```csharp
   catch
   {
       // Model not available
   }
   ```
2. ALL exceptions are swallowed — `NullReferenceException`, `ObjectDisposedException`, `StackOverflowException`, `OutOfMemoryException`.
3. The checkbox renders without the `checked` attribute, regardless of the model's actual state.
4. No error is logged, no indication of failure.

## Deep Reasoning: Why This Is a Real Issue

### Part A — Performance
Expression compilation is the heaviest operation in the .NET expression tree API. The compiled delegate is deterministic — for the same `Expression<Func<TModel, TProp>>`, the compiled function is always the same. It should be compiled once in the constructor and cached as a `Func<TModel, TProp>` field.

Compare with `IdGenerator.For<TModel, TProp>(expression)` which is called in the same constructor and does NOT compile the expression (it walks the expression tree structurally). The inconsistency is clear: the ID generator is efficient, but the checked-state resolver is not.

### Part B — Safety
The bare `catch` block comment says "Model not available," but it catches far more than that. In ASP.NET Core, a view rendering exception should surface to the developer, not be silently swallowed. A checkbox that renders unchecked when the model says it should be checked is a data display bug — the developer needs to know about it, not have it masked.

The framework's Rule 8 says "Throw, don't guess." The bare catch is the ultimate guess — it guesses that ANY exception means "model not available" and proceeds with a default state.

## How Fixing This Improves the Codebase

1. **Cache the compiled delegate**: Move `_expression.Compile()` to the constructor, store as `private readonly Func<TModel, TProp> _compiled`.
2. **Remove the bare catch**: Replace with a specific catch for the model-not-available case (e.g., `if (model == null) return;`). Let all other exceptions propagate.
3. **Pattern for new components**: As 100+ components are onboarded, any model-bound builder that reads model state at render time should follow the cached-delegate pattern, not the compile-per-render pattern.

## How This Fix Will Not Break Existing Features

- Caching the compiled delegate is a performance optimization with identical behavior — the same function is called, just compiled once instead of per-render.
- Removing the bare catch will surface exceptions that were previously hidden. This is intentionally breaking "silent failure" behavior to replace it with visible failure. Any exception surfaced is a bug that was already present but masked.
- The `NativeCheckBoxBuilder<TModel>` (non-model-bound) variant on lines 17-60 does not use expression compilation at all, so it is unaffected.
