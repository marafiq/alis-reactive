# BUG-003: Non-Generic Include() Silently Reads Wrong Property for Non-TextBox Components

## Status: Bug — Silent Data Corruption

## File
`Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs:15-34`

## How to Reproduce

1. Create a form with a checkbox bound to a boolean model property:
   ```csharp
   Html.NativeCheckBoxFor(plan, m => m.IsActive)
   ```
2. In the submit pipeline, use the non-generic `Include()` overload:
   ```csharp
   p.Post("/api/save", g => {
       g.Include(m => m.IsActive);   // BUG: uses NativeTextBox defaults
   });
   ```
3. The gathered value uses `readExpr: "value"` (from `NativeTextBox.ReadExpr`) instead of `readExpr: "checked"` (from `NativeCheckBox.ReadExpr`).
4. At runtime, `gather.ts` calls `walk(resolveRoot(el, "native"), "value")` on the checkbox element.
5. `el.value` on a checkbox returns the HTML `value` attribute (default: `"on"`), NOT the checked state.
6. The server receives `{ "IsActive": "on" }` instead of `{ "IsActive": true }`.

## Deep Reasoning: Why This Is a Real Bug

The non-generic `Include()` overload on line 21 is a "convenience" shorthand that hardcodes `NativeTextBox` as the default component via the static field on line 15:

```csharp
private static readonly NativeTextBox _defaultComponent = new NativeTextBox();
```

This means `g.Include(m => m.IsActive)` silently uses `NativeTextBox.Vendor` ("native") and `NativeTextBox.ReadExpr` ("value") for ALL model properties, regardless of what component was actually rendered for that property.

The framework's own architecture says: "Every component explicitly declares readExpr via IInputComponent — no heuristics, no fallbacks." This convenience overload violates that principle by guessing that every form field is a text input.

The danger scales with the component count. As 100+ components are onboarded, the probability of a developer using the shorter `g.Include(m => m.Prop)` instead of `g.Include<NativeCheckBox, TModel>(m => m.Prop)` increases. The shorter form looks correct, compiles without error, and produces silently wrong data.

This is especially insidious because the wrong `readExpr` still returns a value (`el.value` exists on all input elements), so there is no runtime error — just wrong data.

## How Fixing This Improves the Codebase

1. **Eliminate the wrong-readExpr path**: The fix should either remove the non-generic overload entirely (forcing `Include<TComponent, TModel>()`) or look up the registered component from `plan.ComponentsMap` to resolve the correct `readExpr`.
2. **Consistency with ComponentsMap**: The `ComponentsMap` already stores the correct `readExpr` for every registered component. The gather should use it.
3. **Compile-time safety**: Requiring the component type parameter makes the gather declaration self-documenting: `g.Include<NativeCheckBox, TModel>(m => m.IsActive)` clearly states what component is being read.

## How This Fix Will Not Break Existing Features

- The generic `Include<TComponent, TModel>()` overload (line 40) is unaffected — it correctly reads from the component instance.
- If the non-generic overload is removed, any existing call sites will produce a compile error pointing the developer to the correct generic form. No silent breakage.
- If instead the fix resolves from ComponentsMap, all existing text input usages continue to work because NativeTextBox is registered with the same `readExpr: "value"` that the current default uses.
- The Fusion `FusionGatherExtensions` does not have this non-generic overload, so Fusion components are already safe.
