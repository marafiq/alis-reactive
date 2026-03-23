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

<!-- @comment: how-to-solve-it | Well as per DDD or encapsulation principals, constructor is not only way to protect how internal logic enforce its invaraints, current design protect it with private Setter, with constructor great but as params increase it become not maintainable. But we could use factory methods but as you will have to see this bigger change as part of whole descriptor design complexity. This is one area which is sort of weeek. Can use proper design review to enforce one way to protect encapsuation. | 3/22/2026, 8:18:42 PM -->
<!-- @response: how-to-solve-it | You're right — constructor bloat is real. Factory methods (e.g., MutateElementCommand.WithGuard(guard)) preserve immutability without param explosion. But the deeper point stands: we need ONE encapsulation strategy across ALL descriptors (Command, Trigger, Reaction, Guard), not ad-hoc fixes per class. Proposing: a descriptor-wide design review as a separate epic — audit every mutable setter, decide factory vs builder vs constructor per descriptor family, enforce consistently. Adding to the architecture review backlog. | 3/22/2026, 8:30:00 PM -->
<!-- @comment: how-to-solve-it | Yes. Lets agree on approach. Current proposed solution is not accurate as a principal, we are not dogmatic about these, the whole system descriptors needs a reviw. I suggest creating a class or activity or any diagram which will let us view it as a whole, So update this document with new approach so we can actually solve it. Follow same structure as of this document. | 3/22/2026, 8:28:13 PM -->
<!-- @response: how-to-solve-it | Agreed — fixing GuardWith() in isolation is a band-aid. The real deliverable is a descriptor-wide encapsulation review. I'll audit every descriptor family (Command, Trigger, Reaction, Guard, Entry, BindSource, Mutation, MethodArg) for mutability, map their current construction patterns, and produce a class diagram showing the full descriptor graph. From that we can decide ONE encapsulation strategy (factory methods, builders, or constructor+required-init) that applies uniformly. Updating this document now with the holistic approach below. | 3/23/2026, 10:00:00 AM -->
<!-- @status: how-to-solve-it | delegated | docs/architecture-review/descriptor-encapsulation-review.md -->

---

## Revised Approach: Descriptor-Wide Encapsulation Review

> The original Options A/B above addressed `Command.GuardWith()` in isolation.
> Per feedback, the real issue is system-wide: **all descriptors need ONE consistent
> encapsulation strategy.** This section replaces the per-class fix with a holistic approach.

### Current Descriptor Graph (Class Diagram)

```
Entry
├── trigger: Trigger (polymorphic)
│   ├── DomReadyTrigger
│   ├── CustomEventTrigger
│   ├── ComponentEventTrigger
│   ├── ServerPushTrigger
│   └── SignalRTrigger
├── reaction: Reaction (polymorphic)
│   └── SequentialReaction
│       └── commands: Command[] (polymorphic)
│           ├── DispatchCommand
│           ├── MutateElementCommand
│           │   ├── mutation: Mutation (polymorphic)
│           │   │   ├── SetPropMutation
│           │   │   └── CallMutation
│           │   │       └── args: MethodArg[] (polymorphic)
│           │   │           ├── LiteralArg
│           │   │           └── SourceArg
│           │   ├── source?: BindSource (polymorphic)
│           │   │   ├── EventSource
│           │   │   └── ComponentSource
│           │   └── vendor?: string
│           ├── HttpCommand
│           ├── InjectCommand
│           └── ... (other command kinds)
│       └── when?: Guard (per-command)
└── (no guard at entry level currently)
```

### What Needs Auditing

For each descriptor in the graph above, answer:

| Question | Why it matters |
|----------|---------------|
| Is it immutable after construction? | Plan = contract. Mutable descriptors = unreliable contract. |
| How is it constructed? (ctor / factory / builder mutation) | Need ONE pattern, not three. |
| Does it have `private set` + mutation methods? | `GuardWith()` pattern — identify all instances. |
| Does it have `init` setters? | C# 9+ — immutable but flexible. |
| Can it be a `record`? | Records give value equality + `with` expressions for free. |

### Proposed Approach

1. **Audit** — read every descriptor class, catalog construction pattern and mutability
2. **Diagram** — produce a class diagram (like above but with mutability annotations)
3. **Decide** — pick ONE strategy per descriptor category:
   - **Leaf descriptors** (Mutation, MethodArg, BindSource): `sealed record` — immutable by default, `with` for variants
   - **Guarded descriptors** (Command): factory method or `required init` — guard set once at creation
   - **Aggregate descriptors** (Reaction, Entry): constructor with required params
4. **Implement** — apply the chosen strategy across all descriptors in one pass
5. **Test** — all existing snapshot + schema tests validate the change is shape-preserving

### Success Criteria

- Zero `private set` + post-construction mutation methods remain
- Every descriptor is either a `record` or has `{ get; init; }` / `{ get; }` properties
- No temporal coupling in any builder (no "attach X to last Y" patterns)
- All 1,700+ tests pass unchanged (plan shape must not change)

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
