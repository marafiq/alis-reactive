# INCOMPLETE-014: NativeTextBox Missing Vertical Slice — Cannot Participate in Reactive Plan

## Status: Incomplete Component

## Files
Present:
- `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBox.cs` (phantom type)
- `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxBuilder.cs` (builder + factory)

Missing:
- `NativeTextBoxEvents.cs` — no Events singleton
- `NativeTextBoxExtensions.cs` — no `SetValue()`, `FocusIn()`, `Value()` extensions
- `NativeTextBoxReactiveExtensions.cs` — no `.Reactive()` wiring
- `Events/NativeTextBoxOnChanged.cs` — no change args class
- Zero tests in any test project

## How to Reproduce

1. Render a NativeTextBox in a view:
   ```csharp
   @Html.NativeTextBoxFor(plan, m => m.Name)
   ```
2. Attempt to wire a reactive trigger:
   ```csharp
   Html.On(plan, t => t.CustomEvent("name-changed", p => { ... }));
   // There is no: t.Component<NativeTextBox>(m => m.Name).Reactive(plan, ...)
   ```
3. Compile error: no `.Reactive()` extension method exists for `NativeTextBox`.
4. Attempt to mutate the text box value:
   ```csharp
   p.Component<NativeTextBox>(m => m.Name).SetValue("hello");
   ```
5. Compile error: no `SetValue()` extension method exists.

## Deep Reasoning: Why This Is a Real Issue

CLAUDE.md Rule 2 states: "Every new primitive needs all three layers." A component that can render HTML but cannot participate in the reactive plan is a **half-built vertical slice**. It creates a false sense of completeness — the developer sees `Html.NativeTextBoxFor(plan, m => m.Name)` working and assumes the component is fully integrated.

The component IS registered in the `ComponentsMap` (via the factory method in `NativeTextBoxBuilder.cs`), which means:
- Gather can read its value (via `ComponentGather` using `readExpr: "value"`)
- Validation can validate it (via enrichment from `ComponentsMap`)

But it cannot:
- Fire events when the user types (no `.Reactive()` → no `ComponentEventTrigger`)
- Be mutated from the plan (no `SetValue()` → no `MutateElementCommand` with vendor)
- Respond to other components' events (no reactive extension → no event wiring)

This makes NativeTextBox a **read-only component** in the reactive sense — it can be gathered and validated, but it cannot trigger or receive reactive interactions. This limitation is undocumented and will confuse developers who expect symmetry with NativeCheckBox and NativeDropDown.

The issue also extends to testing — with zero tests, there is no verification that even the existing builder and gather functionality works correctly. A regression in `NativeTextBox.ReadExpr` or `NativeTextBoxBuilder.WriteTo()` would go undetected.

## How Fixing This Improves the Codebase

1. **Complete the pattern**: Add all missing files following the NativeDropDown template (the most complete native component).
2. **Test coverage**: Add unit tests for builder output, event descriptors, extensions, and reactive wiring. Add Playwright tests for text input interactions.
3. **Developer confidence**: Developers can use NativeTextBox with the same API surface as every other component.
4. **Architecture test**: Add NativeTextBox to the Architecture page to exercise the full interaction set.

## How This Fix Will Not Break Existing Features

- Adding new files is purely additive — no existing code changes.
- The existing `NativeTextBox.cs` and `NativeTextBoxBuilder.cs` define the component type and rendering. The new files add reactive capabilities on top.
- The gather extensions (`NativeGatherExtensions`) already reference `NativeTextBox` as the default component (issue 003). Completing the slice does not change that behavior.
- The `ComponentsMap` registration in the factory method already works. The new reactive extensions will use the same registration.
