# Condition Guard Composition Duplication

**Severity:** Low
**SOLID Principle:** DRY (Don't Repeat Yourself)
**Priority:** P2

## What is the issue?

`ConditionSourceBuilder` has identical guard composition logic in three places: `Build()`, `BuildVsSource()`, and inline in `ArrayContains()`. The pattern — flatten existing guard, add new guard, create composite — repeats with the same `if/else` on `CompositionMode`.

## Why is it an issue?

If the composition logic changes (e.g., adding a new composition mode, changing flattening behavior, or fixing a composition bug), the fix must be applied in 3 places. Missing one creates inconsistent behavior between literal comparisons (`Eq("value")`), source comparisons (`Eq(comp.Value())`), and array operations (`ArrayContains(item)`).

## Evidence

**File:** `Alis.Reactive/Builders/Conditions/ConditionSourceBuilder.cs`

**Instance 1 — `Build()` at lines 199-226:**

```csharp
private GuardBuilder<TModel> Build(string op, object? operand = null)
{
    var bindSource = _typedSource.ToBindSource();
    var guard = new ValueGuard(bindSource, _coerceAs, op, operand);

    if (_mode != CompositionMode.None && _existingGuard != null)
    {
        Guard combined;
        if (_mode == CompositionMode.All)
        {
            var guards = new List<Guard>();
            GuardBuilder<TModel>.FlattenAllStatic(_existingGuard, guards);
            guards.Add(guard);
            combined = new AllGuard(guards);
        }
        else
        {
            var guards = new List<Guard>();
            GuardBuilder<TModel>.FlattenAnyStatic(_existingGuard, guards);
            guards.Add(guard);
            combined = new AnyGuard(guards);
        }
        return WrapGuard(combined);
    }
    return WrapGuard(guard);
}
```

**Instance 2 — `BuildVsSource()` at lines 168-195:** Same pattern, different guard constructor (takes `rightSource` instead of `operand`).

**Instance 3 — `ArrayContains()` at lines 131-157:** Same pattern, different guard type (`ArrayContainsGuard` variant).

The duplication is 15 lines x 3 = 45 lines of identical structural code.

## How to solve it

Extract a private helper that takes the new guard and composes it with the existing one:

```csharp
private GuardBuilder<TModel> ComposeAndWrap(Guard newGuard)
{
    if (_mode == CompositionMode.None || _existingGuard == null)
        return WrapGuard(newGuard);

    var guards = new List<Guard>();
    if (_mode == CompositionMode.All)
        GuardBuilder<TModel>.FlattenAllStatic(_existingGuard, guards);
    else
        GuardBuilder<TModel>.FlattenAnyStatic(_existingGuard, guards);

    guards.Add(newGuard);
    var combined = _mode == CompositionMode.All
        ? (Guard)new AllGuard(guards)
        : new AnyGuard(guards);

    return WrapGuard(combined);
}
```

Then each method simplifies to:

```csharp
private GuardBuilder<TModel> Build(string op, object? operand = null)
{
    var guard = new ValueGuard(_typedSource.ToBindSource(), _coerceAs, op, operand);
    return ComposeAndWrap(guard);
}

private GuardBuilder<TModel> BuildVsSource(string op, TypedSource<TProp> right)
{
    var guard = new ValueGuard(_typedSource.ToBindSource(), _coerceAs, op, right.ToBindSource());
    return ComposeAndWrap(guard);
}
```

## Why the solution is better

1. **Single fix point** — composition logic changes in one place, not three
2. **Smaller methods** — `Build()` drops from 25 lines to 4
3. **Easier to verify** — one `ComposeAndWrap()` to test, not three identical branches
4. **New operators** (future: `Between`, `Matches`, custom) automatically get correct composition

## Summary of improvement

| Before | After |
|--------|-------|
| 45 lines of duplicated composition logic | 12 lines in one helper |
| Bug fix requires updating 3 methods | Bug fix in one place |
| Each method is 25 lines | Each method is 4 lines |
| New composition modes need 3 updates | New modes need 1 update |
