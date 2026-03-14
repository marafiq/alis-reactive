# FAIL-FAST-007: ElementBuilder.When() Guard Silently Discarded When No Commands Exist

## Status: Fail-Fast Violation (Rule 8)

## File
`Alis.Reactive/Builders/ElementBuilder.cs:146-148`

## How to Reproduce

1. Write a plan where `.When()` is called before any element mutation:
   ```csharp
   p.Element("status")
    .When(payload, x => x.IsAdmin, c => c.Eq(true).Then())  // Guard configured
    // Developer forgot to add .SetText() or .AddClass() BEFORE the .When()
    .SetText("admin");  // This command has NO guard
   ```
2. The `When()` method (line 146) checks `_pipeline.Commands.Count > 0`:
   ```csharp
   if (_pipeline.Commands.Count > 0)
       _pipeline.Commands[_pipeline.Commands.Count - 1].When = gb.Guard;
   ```
3. Since `When()` is called before `SetText()`, the commands list is empty.
4. The guard is silently discarded. The `SetText("admin")` added afterward has no guard.
5. The text "admin" is shown to ALL users, not just admins.

## Deep Reasoning: Why This Is a Real Bug

The `When()` method's design attaches a guard to the **last command** in the pipeline. This is a post-hoc mutation pattern — the guard is not part of the command's construction; it is stapled on after the fact via index-based access.

When there are no commands to attach to, the guard is silently dropped. No exception, no trace warning, no indication that the developer's security-critical conditional was ignored.

This is especially dangerous because the API's fluent chaining syntax makes the ordering non-obvious. The developer reads `.When(...).SetText(...)` left-to-right and assumes the guard applies to the `SetText`. But the internal execution order is: `When()` runs first (finds no commands), then `SetText()` runs (adds an unguarded command).

The framework's Rule 8 says: "if a readExpr is missing — throw immediately with a clear error message telling the developer what they forgot." A silently discarded guard is the same class of problem — the developer configured something important, and the framework ignored it.

## How Fixing This Improves the Codebase

1. **Fail-fast**: Throw `InvalidOperationException("When() must be called after at least one element command (SetText, AddClass, etc.)")` when `Commands.Count == 0`.
2. **Developer guidance**: The error message tells the developer exactly what to fix — reorder the chain.
3. **Eliminates the post-hoc mutation pattern**: An alternative design would make the guard part of the command construction rather than attaching it afterward. This would make the API order-independent.

## How This Fix Will Not Break Existing Features

- Any existing code that calls `.When()` after a command (the correct usage) is unaffected — `Commands.Count > 0` is true, and the guard attaches normally.
- The only code that would break is code that calls `.When()` before any command — which is already broken (the guard is silently lost). Making it throw surfaces an existing bug rather than creating a new one.
- The Roslyn analyzer `ALIS001` already detects dangling `GuardBuilder` results, but that analyzer only catches cases where the `GuardBuilder` return value is discarded at the expression statement level. The `When()` method consumes the `GuardBuilder` internally, so the analyzer cannot detect this misuse pattern. The runtime check is the only defense.
