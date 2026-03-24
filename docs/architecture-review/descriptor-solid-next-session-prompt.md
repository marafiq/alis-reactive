# Next Session Prompt — Descriptor SOLID Evolution

Copy-paste this into a new Claude Code session:

---

## Context

Read these two architecture review documents first:

1. `docs/architecture-review/descriptor-encapsulation-review.md` — concluded that descriptor mutability is appropriate for the request-scoped lifecycle. No immutability changes needed.
2. `docs/architecture-review/descriptor-solid-evolution-review.md` — identified 2 actionable items + 1 cheap insurance test for scaling to 100+ component vertical slices.

## Tasks (in priority order)

### Task 1: Schema-Kind Consistency Test (cheap insurance, do first)

Add a C# unit test that discovers ALL subclasses of Command, Trigger, Reaction, Mutation, MethodArg, BindSource, Guard, and GatherItem via reflection, reads each subclass's `Kind` property value, and asserts that `reactive-plan.schema.json` contains a matching definition for that kind.

**Why:** `WriteOnlyPolymorphicConverter` serializes any subclass via reflection — a new subclass without a schema definition is a silent bug. This test catches schema drift.

**Where:** `tests/Alis.Reactive.UnitTests/Schema/` — alongside existing `AllPlansConformToSchema.cs`.

**Acceptance:** Test discovers all current subclasses, passes green, and would fail if someone added a new Command subclass without updating the schema.

### Task 2: ComponentEventTrigger Factory Method

Create a static factory method on `ComponentEventTrigger` that reduces the 5-arg constructor call repeated in every `*ReactiveExtensions.cs`:

**Before** (repeated in every component's ReactiveExtensions):
```csharp
var trigger = new ComponentEventTrigger(
    componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
```

**After:**
```csharp
var trigger = ComponentEventTrigger.For<TComponent>(componentId, descriptor, bindingPath);
```

The factory resolves `Vendor` and `ReadExpr` from the `IComponent`/`IInputComponent` via `new()` constraint. Then update all existing `*ReactiveExtensions.cs` files to use the factory.

**Acceptance:** All existing tests pass unchanged. Every ReactiveExtensions file uses the factory. Zero plan shape change.

### Task 3: Shared Reactive Wiring Helper (if time permits)

Extract the ~15-line boilerplate from every `*ReactiveExtensions.cs` into an internal helper:

```csharp
internal static TriggerBuilder<TModel> WireComponentEvent<TModel, TComponent, TArgs>(...)
```

Each component's `.Reactive()` becomes a 3-line wrapper. Update all existing ReactiveExtensions to use it.

**Acceptance:** All tests pass. Each ReactiveExtensions file is 3 lines per event overload instead of 15.

## Rules

- Run ALL tests after each task (vitest + all dotnet test projects)
- Zero plan JSON shape changes — snapshot tests must pass unchanged
- Do NOT modify the TS runtime
- Do NOT change any public API — these are internal refactors only
- Read the CLAUDE.md before starting
