# Reactive Plan Interface Breadth

**Severity:** Medium
**SOLID Principle:** Interface Segregation
**Priority:** P1

## What is the issue?

`IReactivePlan<TModel>` exposes 7 members. Every consumer — whether it only adds entries, only renders, or only reads components — sees the full surface. Builders that call `AddEntry()` can also call `Render()`. Views that call `Render()` can also call `AddToComponentsMap()`.

## Why is it an issue?

Interface Segregation says: no client should depend on methods it doesn't use. When a builder receives `IReactivePlan<TModel>`, it has access to `Render()` — but calling it mid-build would produce an incomplete plan. There's no compile-time signal that "builders write, views render."

As the framework grows (partials, validation resolution, multi-plan composition), each new consumer will see all 7 members even if it only needs 1-2. This makes the interface harder to implement for testing (mock 7 members) and harder to reason about (what's this consumer allowed to do?).

## Evidence

**File:** `Alis.Reactive/IReactivePlan.cs:6-15`

```csharp
public interface IReactivePlan<TModel> where TModel : class
{
    string PlanId { get; }                                              // identity
    bool IsPartial { get; }                                             // identity
    void AddEntry(Entry entry);                                         // write
    void AddToComponentsMap(string bindingPath, ComponentRegistration entry);  // write
    IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap { get; }  // read
    string Render();                                                    // render
    string RenderFormatted();                                           // render
}
```

**Consumers and what they actually need:**

| Consumer | Uses | Doesn't use |
|----------|------|-------------|
| `TriggerBuilder` | `AddEntry()` | Render, ComponentsMap |
| `InputFieldExtensions` | `AddToComponentsMap()` | Render, AddEntry |
| `PlanExtensions.RenderPlan()` | `Render()`, `IsPartial`, `PlanId` | AddEntry, AddToComponentsMap |
| `GatherResolver` | `ComponentsMap` (read) | AddEntry, Render |
| `ValidationResolver` | `ComponentsMap` (read) | AddEntry, Render |

No consumer needs all 7 members.

## How to solve it

Split into focused interfaces. The main interface composes them:

```csharp
public interface IPlanEntryWriter<TModel> where TModel : class
{
    void AddEntry(Entry entry);
}

public interface IComponentMapWriter<TModel> where TModel : class
{
    void AddToComponentsMap(string bindingPath, ComponentRegistration entry);
}

public interface IComponentMapReader<TModel> where TModel : class
{
    IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap { get; }
}

public interface IPlanRenderer
{
    string PlanId { get; }
    bool IsPartial { get; }
    string Render();
    string RenderFormatted();
}

// Composed interface — backward compatible
public interface IReactivePlan<TModel> :
    IPlanEntryWriter<TModel>,
    IComponentMapWriter<TModel>,
    IComponentMapReader<TModel>,
    IPlanRenderer
    where TModel : class
{
}
```

Then update consumers to accept the narrow interface:

```csharp
// Before: TriggerBuilder sees everything
internal TriggerBuilder(IReactivePlan<TModel> plan) { ... }

// After: TriggerBuilder sees only what it needs
internal TriggerBuilder(IPlanEntryWriter<TModel> plan) { ... }
```

## Why the solution is better

1. **Compile-time role enforcement** — a builder that only writes entries cannot accidentally call `Render()`.
2. **Easier to mock in tests** — testing a builder only requires implementing `IPlanEntryWriter` (1 method), not all 7.
3. **Self-documenting** — parameter type tells you what the consumer does: "this method adds entries" vs "this method renders."
4. **Backward compatible** — `IReactivePlan<TModel>` still exists, composes all sub-interfaces. No breaking changes.

## Summary of improvement

| Before | After |
|--------|-------|
| All consumers see 7 members | Each consumer sees only what it needs |
| Builders can accidentally call `Render()` | Builders can't — type prevents it |
| Mocking requires 7 members | Mocking requires 1-2 members |
| Parameter type doesn't signal intent | `IPlanEntryWriter` signals "I write entries" |
| One interface for all roles | Composed interfaces with clear boundaries |
