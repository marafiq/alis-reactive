# DEAD-CODE-015: INativeInputComponent / IFusionInputComponent Are Unused Marker Interfaces

## Status: Dead Code

## Files
- `Alis.Reactive.Native/NativeComponent.cs:19` — `public interface INativeInputComponent { }`
- `Alis.Reactive.Fusion/FusionComponent.cs:19` — `public interface IFusionInputComponent { }`

## How to Reproduce

1. Search the entire codebase for `INativeInputComponent` as a generic constraint:
   ```
   grep -r "INativeInputComponent" --include="*.cs" | grep -v "interface INativeInputComponent"
   ```
   Result: Only `NativeDropDown.cs` implements it. It is never used as a constraint.

2. Search for `IFusionInputComponent` as a generic constraint:
   ```
   grep -r "IFusionInputComponent" --include="*.cs" | grep -v "interface IFusionInputComponent"
   ```
   Result: `FusionDropDownList.cs` and `FusionNumericTextBox.cs` implement it. Never used as a constraint.

3. Note the inconsistency: `NativeCheckBox` implements `IInputComponent` but NOT `INativeInputComponent`. `NativeDropDown` implements BOTH.

## Deep Reasoning: Why This Is a Real Issue

These interfaces duplicate the concept already expressed by the core `IInputComponent` interface (from `Alis.Reactive/IComponent.cs`). The core `IInputComponent` declares `Vendor` (which distinguishes native from fusion) and `ReadExpr` (which declares the read property). There is nothing that a vendor-specific marker adds.

The generic constraints throughout the codebase use the pattern:
```csharp
where TComponent : NativeComponent, IInputComponent, new()
```

The vendor is already constrained by `NativeComponent` (base class) and the input contract by `IInputComponent`. The marker `INativeInputComponent` adds zero type safety.

The inconsistent application proves they serve no purpose:
- NativeCheckBox: only `IInputComponent` — works fine
- NativeDropDown: `INativeInputComponent` + `IInputComponent` — no behavioral difference
- NativeTextBox: only `IInputComponent` — works fine

If these markers were enforcing something, the inconsistency would cause compile errors. The fact that it doesn't proves they are inert.

Dead interfaces are worse than no interfaces — they create the illusion of a design pattern that doesn't exist. A developer onboarding a new component might waste time wondering whether they should implement the marker, and if so, why some components do and others don't.

## How Fixing This Improves the Codebase

1. **Remove confusion**: New component authors don't need to ask "do I need INativeInputComponent?"
2. **Consistent pattern**: All components implement only `IComponent` or `IInputComponent` — no vendor-specific markers.
3. **Less code**: Two fewer interfaces to maintain.

## How This Fix Will Not Break Existing Features

- The interfaces are never used as constraints. Removing them removes only the `implements` clause from 3 component classes.
- No runtime behavior changes — these are compile-time-only artifacts.
- No serialization impact — the interfaces have no properties.
- All tests continue to pass because no test references these interfaces.
