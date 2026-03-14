# FAIL-FAST-010: trigger.ts readExpr Falls Back to "value" Instead of Throwing

## Status: Fail-Fast Violation (Rule 8)

## File
`Scripts/trigger.ts:40`

## How to Reproduce

1. Create a `ComponentEventTrigger` in the C# DSL that somehow omits `readExpr` (e.g., through a future component that forgets to set it, or a plan constructed via manual JSON).
2. The trigger JSON will have no `readExpr` field.
3. At runtime, `trigger.ts:40`:
   ```typescript
   const expr = trigger.readExpr ?? "value";
   ```
4. `readExpr` is `undefined`, so `expr` becomes `"value"`.
5. The runtime reads `el.value` from the component, which may be the wrong property (e.g., a checkbox should read `el.checked`).

## Deep Reasoning: Why This Is a Real Bug

The framework's Cardinal Rule states: "No fallback defaults — every component explicitly declares readExpr via IInputComponent." The runtime should be a dumb executor that reads what the plan tells it to read, with zero guessing.

The `?? "value"` fallback directly contradicts this principle. If `readExpr` is missing from the plan, something went wrong in the C# DSL layer — either a component forgot to declare its `ReadExpr` property, or the serialization dropped the field. Either way, the correct response is an error, not a guess.

The insidiousness: `"value"` happens to be the correct `readExpr` for the most common component type (text inputs). So this fallback silently "works" for most components. It only breaks for checkboxes (`readExpr: "checked"`), radio buttons, or custom components with non-standard read paths. These bugs would only surface in production with those specific component types.

## How Fixing This Improves the Codebase

1. **Removes the last runtime heuristic**: After this fix, the runtime truly has zero guessing — every property read is plan-driven.
2. **Catches C# DSL bugs immediately**: If a component vertical slice forgets to set `ReadExpr`, the error surfaces the first time the component event fires, with a clear message.
3. **Consistent with `component.ts:22`**: The Fusion vendor root resolution already throws when `ej2_instances` is missing. Missing `readExpr` should throw similarly.

## How This Fix Will Not Break Existing Features

- Every existing component in the framework (`NativeCheckBox`, `NativeDropDown`, `NativeTextBox`, `FusionNumericTextBox`, `FusionDropDownList`) declares `ReadExpr` as a non-null instance property. Their triggers always include `readExpr` in the plan JSON.
- The `ComponentEventTrigger` C# constructor requires `readExpr` as a constructor parameter (nullable but always passed by the builder).
- The only scenario where `readExpr` would be missing is a malformed plan — which is exactly the scenario where throwing is correct.
- All existing Playwright tests and TS unit tests pass valid `readExpr` values and will continue to pass.
