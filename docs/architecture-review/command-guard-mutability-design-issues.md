# Command Guard Mutability

**Severity:** Medium
**SOLID Principle:** Single Responsibility + Immutability
**Priority:** P1

## What is the issue?

`Command.GuardWith()` mutates a command's `When` guard **after construction**. Descriptors should be immutable once created — the plan is the contract between C# and JS. A mutable descriptor means the plan shape can change between construction and serialization.

## Why is it an issue?

The framework's architecture says "descriptors are data, builders build them." But `GuardWith()` blurs that line — it's a setter disguised as a method, called from `ElementBuilder` to attach a per-action guard to the most recently added command.

This creates a temporal coupling: the guard must be attached **after** the command is added to the pipeline's command list but **before** serialization. If the order changes, the guard attaches to the wrong command or throws.

## Evidence

**File:** `Alis.Reactive/Descriptors/Commands/Command.cs:14-20`

```csharp
public abstract class Command
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guard? When { get; private set; }

    internal void GuardWith(Guard guard)
    {
        if (!(When is null))
            throw new InvalidOperationException(
                "Command already has a guard. Each command can only have one When guard.");
        When = guard;
    }
}
```

**Caller:** `Alis.Reactive/Builders/ElementBuilder.cs` (the `When()` method)

```csharp
// Attaches guard to the LAST command added to the pipeline
if (_pipeline.Commands.Count > 0)
    _pipeline.Commands[_pipeline.Commands.Count - 1].GuardWith(gb.Guard);
```

This mutates a command that was already constructed and added to the list. The command was built as "AddClass without guard" and later mutated to "AddClass with guard."

## How to solve it

> **Full descriptor-wide review:** [descriptor-encapsulation-review.md](descriptor-encapsulation-review.md) — audits all 34 descriptor classes, identifies 7 mutation methods across 3 classes, and proposes Phase 1 (list freezing via `.ToArray()`) as the safe, actionable fix. Option A below was found to break the fluent API — see the review for details.

**Option A: Constructor-based guard (immutable)**

Commands receive their guard at construction time. The builder creates the command with the guard already attached:

```csharp
public abstract class Command
{
    public Guard? When { get; }

    protected Command(Guard? when = null)
    {
        When = when;
    }
}

public sealed class MutateElementCommand : Command
{
    public MutateElementCommand(string target, Mutation mutation,
        string? value = null, BindSource? source = null,
        string? vendor = null, Guard? when = null)
        : base(when)
    {
        // ...
    }
}
```

The builder collects the guard first, then creates the command:

```csharp
// ElementBuilder — instead of mutating after the fact
public PipelineBuilder<TModel> AddClass(string className)
{
    var command = new MutateElementCommand(_elementId,
        new CallMutation("add", chain: "classList", args: [...]),
        when: _pendingGuard);  // guard passed at construction
    _pipeline.AddCommand(command);
    _pendingGuard = null;
    return _pipeline;
}
```

**Option B: Document the mutable phase (accept the tradeoff)**

Add a comment to `Command.cs` making it explicit:

```csharp
/// Commands are mutable during the builder phase (GuardWith can be called once).
/// After plan.Render(), no mutations occur — the plan is frozen.
```

## Why the solution is better

**Option A** makes the contract enforceable at compile time. A `Command` with `{ get; }` on `When` cannot be mutated — period. No temporal coupling, no "attach guard to last command" pattern.

**Option B** is pragmatic — the current code works and the mutation window is narrow (builder phase only). But it requires discipline: future developers must know not to call `GuardWith()` after serialization.

## Summary of improvement

| Before | After (Option A) |
|--------|-----------------|
| Command is mutable after construction | Command is immutable once created |
| Guard attaches to "last command" by index | Guard is part of command constructor |
| Temporal coupling between Add + GuardWith | No ordering dependency |
| Runtime validation (throw if already guarded) | Compile-time enforcement (no setter) |
| Risk: wrong command gets guarded if order changes | Impossible to attach guard to wrong command |
