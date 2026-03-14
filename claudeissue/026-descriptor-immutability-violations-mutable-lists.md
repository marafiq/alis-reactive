# DESIGN-026: Descriptor Immutability Violations — Mutable List Properties

## Status: Design Issue — Immutability Contract

## Files
- `Alis.Reactive/Descriptors/Reactions/Reaction.cs:19` — `SequentialReaction.Commands` is `List<Command>`
- `Alis.Reactive/Descriptors/Reactions/Reaction.cs` — `ConditionalReaction.Commands`, `HttpReaction`, `ParallelHttpReaction.Requests`
- `Alis.Reactive/Validation/ValidationDescriptor.cs:12` — `Fields` is `List<ValidationField>`
- `Alis.Reactive/Validation/ValidationField.cs:14` — `Rules` is `List<ValidationRule>`
- `Alis.Reactive/Descriptors/Commands/Command.cs:15` — `When` has `internal set`
- `Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs:56,78` — `Gather` and `Validation` have `internal set`

## How to Reproduce

1. Build a plan with a sequential reaction:
   ```csharp
   var plan = CreatePlan();
   // ... add entries
   var json = plan.Render();
   ```
2. The `SequentialReaction` exposes `public List<Command> Commands { get; }`.
3. Any code with access to the reaction can mutate the list after construction:
   ```csharp
   reaction.Commands.Add(new DispatchCommand("injected"));
   reaction.Commands.Clear();
   reaction.Commands.RemoveAt(0);
   ```
4. The mutation is not prevented by the type system.

## Deep Reasoning: Why This Is a Design Issue

Descriptors are the **data layer** of the plan. They represent the serialized contract between C# and JS. Once constructed, descriptors should be immutable — their content is the truth that gets serialized to JSON.

The framework already demonstrates awareness of this principle:
- `AllGuard.Guards` uses `IReadOnlyList<Guard>` (Guard.cs:92)
- `AnyGuard.Guards` uses `IReadOnlyList<Guard>` (Guard.cs:104)
- `InvertGuard.Inner` is a readonly property (Guard.cs:116)

But `SequentialReaction.Commands`, `ValidationDescriptor.Fields`, and `ValidationField.Rules` use mutable `List<T>`. This inconsistency means some descriptors are protected against post-construction mutation and some are not.

The `Command.When` property with `internal set` is particularly concerning. It is mutated by `ElementBuilder.When()` (line 147) via index-based access: `_pipeline.Commands[_pipeline.Commands.Count - 1].When = gb.Guard`. This means:
1. The guard is attached **after** the command is constructed (post-hoc mutation).
2. The attachment depends on list ordering (fragile).
3. If the list is reordered between command creation and guard attachment, the guard goes to the wrong command.

The `RequestDescriptor.Validation` with `internal set` is mutated by `ValidationResolver` after the descriptor is constructed. The `Gather` property is set during builder finalization. Both break the immutability contract.

## How Fixing This Improves the Codebase

1. **Type-system enforcement**: Change `List<Command>` to `IReadOnlyList<Command>` on all descriptor properties. The builder can work with `List<T>` internally and convert to `IReadOnlyList<T>` at construction.
2. **Eliminate post-hoc mutation**: Make `Command.When` a constructor parameter. The builder can create the command with its guard in one step.
3. **Construction-time completeness**: `RequestDescriptor` should receive all data at construction (including `Gather`, `Validation`, `ValidatorType`). The resolver can run before construction, not after.
4. **Consistent with Guard pattern**: Follow the same `IReadOnlyList<T>` pattern used by `AllGuard` and `AnyGuard`.

## How This Fix Will Not Break Existing Features

- Changing `List<T>` to `IReadOnlyList<T>` on public properties is a **source-compatible** change for read-only consumers. Code that iterates (`foreach`, `Count`, indexer) continues to work.
- Code that mutates the list (`.Add()`, `.Remove()`) will get a compile error, pointing to locations that need refactoring to use the builder pattern instead of post-hoc mutation.
- JSON serialization with `System.Text.Json` works identically for `IReadOnlyList<T>` and `List<T>`.
- All snapshot tests verify the serialized JSON, not the C# type of the property. They pass unchanged.
