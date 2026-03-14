# DESIGN-024: ElementBuilder SetText/SetHtml Return Type Inconsistency

## Status: Design Issue — API Ergonomics

## File
`Alis.Reactive/Builders/ElementBuilder.cs`

## How to Reproduce

1. Call `SetText(string)` — returns `PipelineBuilder<TModel>`:
   ```csharp
   p.Element("status").SetText("loaded").Dispatch("done"); // Compiles ✓
   ```
2. Call `SetText(BindSource)` — returns `ElementBuilder<TModel>`:
   ```csharp
   p.Element("status").SetText(someSource).Dispatch("done"); // Compile ERROR ✗
   ```
3. The developer must break the chain:
   ```csharp
   var el = p.Element("status");
   el.SetText(someSource);
   // How do I get back to PipelineBuilder to call Dispatch()?
   ```

## Deep Reasoning: Why This Is a Design Issue

The `ElementBuilder` has 7 overloads of `SetText` and `SetHtml` with two different return type patterns:

| Method | Returns | Chain continues with |
|--------|---------|---------------------|
| `SetText(string)` | `PipelineBuilder` | Pipeline commands (Dispatch, Element, Post, etc.) |
| `SetText(source, expression)` | `PipelineBuilder` | Pipeline commands |
| `SetText(ResponseBody, expression)` | `PipelineBuilder` | Pipeline commands |
| `SetText(BindSource)` | `ElementBuilder` | Element mutations (AddClass, Show, When, etc.) |
| `SetText(TypedSource)` | `ElementBuilder` | Element mutations |

The same method name (`SetText`) has different return types depending on the overload. The developer cannot predict the chaining behavior from the method name alone.

The design intent appears to be:
- Static value overloads (string, expression) → you're done with this element, return to pipeline
- Source-binding overloads (BindSource, TypedSource) → you might want to add a `.When()` guard, stay on element

But this intent is implicit and undocumented. The API violates the Principle of Least Surprise — a developer who learns the `SetText(string)` pattern expects all `SetText` overloads to behave the same way.

## How Fixing This Improves the Codebase

1. **Consistent return types**: All `SetText`/`SetHtml` overloads should return `PipelineBuilder` (ending the element chain) OR all should return `ElementBuilder` (continuing element operations). Given that most overloads return `PipelineBuilder`, that should be the standard.
2. **When() on pipeline level**: The `When()` guard can be attached at the pipeline level (on the last command) rather than requiring the element builder to stay open.
3. **Predictable API**: Developers learn one pattern and it works for all overloads.

## How This Fix Will Not Break Existing Features

- Changing `ElementBuilder` return types to `PipelineBuilder` only affects chaining after the call. Any existing code that chains element methods after `SetText(BindSource)` would need adjustment — but no existing code does this because the `ElementBuilder` does not expose useful chaining methods beyond `When()`.
- The `When()` method can be moved to or duplicated on `PipelineBuilder` to preserve the guard-attachment capability.
- Snapshot tests for plan JSON are unaffected — the fix changes return types, not serialized output.
