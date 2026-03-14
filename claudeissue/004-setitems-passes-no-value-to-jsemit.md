# BUG-004: TestWidgetSyncFusion SetItems() Emits jsEmit Referencing val But Passes No Value

## Status: Bug — Runtime Undefined Argument

## File
`Alis.Reactive.Fusion/Components/TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs:13-15`

## How to Reproduce

1. Use the `SetItems()` method in a plan:
   ```csharp
   p.Component<TestWidgetSyncFusion>("widget").SetItems();
   ```
2. The serialized plan command will be:
   ```json
   { "kind": "mutate-element", "target": "widget", "jsEmit": "el.setItems(val)", "vendor": "fusion" }
   ```
3. At runtime, `element.ts:13` resolves `val` from `cmd.value` (undefined) and `cmd.source` (undefined).
4. `new Function("el", "val", "el.setItems(val)").call(null, root, undefined)` is executed.
5. The widget receives `setItems(undefined)` — either a no-op or an error depending on the widget implementation.

## Deep Reasoning: Why This Is a Real Bug

The method signature `SetItems()` takes no parameters, yet the jsEmit expression `el.setItems(val)` references the `val` variable. This is a **contract mismatch** between the C# DSL method signature and the JS expression it emits.

Compare with the correct patterns in the same file:
- `SetValue(string value)` → `self.Emit("el.value=val", value)` — passes value to Emit
- `Focus()` → `self.Emit("el.focus()")` — jsEmit does NOT reference val

`SetItems()` follows neither pattern: it references `val` but passes nothing. This is almost certainly an incomplete implementation — the method should either accept an items parameter (like `SetValue` does) or the jsEmit should not reference `val` (like `Focus` does).

While this is a test widget (used for architecture regression tests), the TestWidget is documented as the **canonical example** for onboarding new components. A bug in the canonical example will be replicated across all future component vertical slices.

## How Fixing This Improves the Codebase

1. **Correct the canonical pattern**: Fix `SetItems` to either accept a parameter or change the jsEmit to not reference `val`.
2. **Architecture test integrity**: The TestWidget exercises ALL interaction types. If `SetItems` is broken, the method-call-with-args interaction type is not being validated.

## How This Fix Will Not Break Existing Features

- `SetItems()` is only used in test contexts. No production view calls it.
- If the signature changes to accept a parameter, any existing call sites will get a compile error (caught at build time).
- The TestWidget is not consumed by end users — it exists purely for architecture verification.
