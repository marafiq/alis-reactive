# Gather Extensions Vendor Duplication

**Severity:** Low
**SOLID Principle:** DRY + Open/Closed
**Priority:** P2

## What is the issue?

`NativeGatherExtensions` and `FusionGatherExtensions` contain nearly identical code. Both create `ComponentGather` items by instantiating a component, reading its `Vendor` and `ReadExpr`, and calling `AddItem()`. The only difference is the generic constraint: `NativeComponent` vs `FusionComponent`.

## Why is it an issue?

When a third vendor is added (or the gather pattern changes), the same logic must be replicated in a third file. If the `ComponentGather` constructor changes, two files must update. If a bug is found in ID generation or property name resolution, it must be fixed in both.

At 23 components this is manageable. At 3 vendors x 5 overloads, it's 15 near-identical methods.

## Evidence

**File 1:** `Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs:40-55`

```csharp
public static GatherBuilder<TModel> Include<TComponent, TModel>(
    this GatherBuilder<TModel> self,
    Expression<Func<TModel, object?>> expr)
    where TComponent : NativeComponent, IInputComponent, new()
    where TModel : class
{
    var component = new TComponent();
    var elementId = IdGenerator.For<TModel>(expr);
    var propertyName = ExpressionPathHelper.ToPropertyName(expr);
    self.AddItem(new ComponentGather(
        elementId, component.Vendor, propertyName, component.ReadExpr));
    return self;
}
```

**File 2:** `Alis.Reactive.Fusion/Extensions/FusionGatherExtensions.cs:20-35`

```csharp
public static GatherBuilder<TModel> Include<TComponent, TModel>(
    this GatherBuilder<TModel> self,
    Expression<Func<TModel, object?>> expr)
    where TComponent : FusionComponent, IInputComponent, new()
    where TModel : class
{
    var component = new TComponent();
    var elementId = IdGenerator.For<TModel>(expr);
    var propertyName = ExpressionPathHelper.ToPropertyName(expr);
    self.AddItem(new ComponentGather(
        elementId, component.Vendor, propertyName, component.ReadExpr));
    return self;
}
```

Lines 3-8 of each method body are **character-for-character identical**. The only difference is the generic constraint on line 4: `NativeComponent` vs `FusionComponent`.

## How to solve it

Move the gather extension to the core project with a constraint on `IComponent` (the shared interface), not `NativeComponent` or `FusionComponent`:

```csharp
// In Alis.Reactive (core) — works for ALL vendors
public static class GatherExtensions
{
    public static GatherBuilder<TModel> Include<TComponent, TModel>(
        this GatherBuilder<TModel> self,
        Expression<Func<TModel, object?>> expr)
        where TComponent : IComponent, IInputComponent, new()
        where TModel : class
    {
        var component = new TComponent();
        var elementId = IdGenerator.For<TModel>(expr);
        var propertyName = ExpressionPathHelper.ToPropertyName(expr);
        self.AddItem(new ComponentGather(
            elementId, component.Vendor, propertyName, component.ReadExpr));
        return self;
    }

    public static GatherBuilder<TModel> Include<TComponent, TModel>(
        this GatherBuilder<TModel> self,
        string refId,
        string name)
        where TComponent : IComponent, IInputComponent, new()
        where TModel : class
    {
        var component = new TComponent();
        self.AddItem(new ComponentGather(
            refId, component.Vendor, name, component.ReadExpr));
        return self;
    }
}
```

This eliminates both vendor-specific files. The constraint `IComponent, IInputComponent, new()` is sufficient — it works for both `NativeTextBox` and `FusionDropDownList` because both implement `IComponent` and `IInputComponent`.

**Note:** The untyped `Include<TModel>()` overload in `NativeGatherExtensions` (which defaults to `NativeTextBox`) would remain in the Native project since it's vendor-specific by design.

## Why the solution is better

1. **One implementation** — any vendor's component works with the same gather code
2. **Third vendor = zero gather work** — adding a "BlazorComponent" vendor needs no gather extension file
3. **Bug fixes propagate** — fix `IdGenerator.For()` usage in one place
4. **Core owns the pattern** — vendor projects don't need to know about `ComponentGather` internals

## Summary of improvement

| Before | After |
|--------|-------|
| 2 files with identical method bodies | 1 file in core |
| Adding a vendor requires new gather extension file | Adding a vendor requires nothing |
| Bug fix in ID/property resolution needs 2 updates | Bug fix in 1 place |
| Constraint: `NativeComponent` or `FusionComponent` | Constraint: `IComponent, IInputComponent` (universal) |
| 75 lines across 2 files | 30 lines in 1 file |
