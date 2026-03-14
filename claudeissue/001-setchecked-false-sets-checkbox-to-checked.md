# BUG-001: SetChecked(false) Sets Checkbox to CHECKED in Browser

## Status: Bug — Runtime Behavior Incorrect

## File
`Alis.Reactive.Native/Components/NativeCheckBox/NativeCheckBoxExtensions.cs:19`

## How to Reproduce

1. Create a plan with a checkbox mutation:
   ```csharp
   p.Component<NativeCheckBox>("myCheck").SetChecked(false);
   ```
2. Inspect the serialized plan JSON — the command will be:
   ```json
   { "kind": "mutate-element", "target": "myCheck", "jsEmit": "el.checked=val", "value": "false" }
   ```
3. When the JS runtime executes `new Function("el", "val", "el.checked=val").call(null, el, "false")`, the string `"false"` is assigned to `el.checked`.
4. In JavaScript, **any non-empty string is truthy**: `el.checked = "false"` sets the checkbox to **CHECKED**.
5. The checkbox turns ON when the developer intended to turn it OFF.

## Deep Reasoning: Why This Is a Real Bug

This is not a surface-level style issue — it is a **data corruption bug** that produces the exact opposite of the intended behavior.

The root cause is a **type impedance mismatch** between the C# DSL layer and the JS runtime layer. The SOLID loop architecture (C# DSL -> JSON Plan -> JS Runtime) requires that the plan carries values in a format the runtime can execute correctly. The plan schema defines `value` as `"type": "string"` (schema line 283), but `el.checked` is a **boolean DOM property**, not a string attribute.

The `jsEmit` pattern `el.checked=val` expects `val` to be a JavaScript boolean (`true`/`false`), but the C# DSL serializes it as the C# strings `"true"/"false"`. Every other jsEmit expression works with string values (`el.textContent = val`, `el.classList.add(val)`) because those DOM APIs accept strings. `el.checked` is the one DOM property in the current framework where the receiver type is boolean, not string.

This bug was masked because:
- The `SetChecked(true)` path works correctly — the string `"true"` is truthy, so `el.checked = "true"` happens to set checked to true (correct result, wrong reason).
- Only `SetChecked(false)` reveals the bug because `"false"` is truthy.
- There are no Playwright tests that verify unchecking a checkbox via `SetChecked(false)`.

## How Fixing This Improves the Codebase

1. **Correctness**: The DSL will produce the intended DOM state — `SetChecked(false)` will uncheck the checkbox.
2. **Architectural honesty**: The fix aligns the jsEmit expression with the DOM API contract. The framework's principle is "the browser API IS the API" — and `el.checked` is a boolean API.
3. **Pattern precedent**: This establishes that jsEmit expressions must handle type coercion when the target DOM property is non-string. As more components are onboarded (radio buttons, contenteditable, etc.), this pattern becomes critical.

## How This Fix Will Not Break Existing Features

- The fix changes the jsEmit string from `el.checked=val` to `el.checked=(val==='true')`. This evaluates identically for both `"true"` (produces `true`) and `"false"` (produces `false`).
- `SetChecked(true)` continues to work — `'true' === 'true'` is `true`.
- No other component uses the `el.checked=val` jsEmit — this is isolated to `NativeCheckBoxExtensions`.
- The schema does not change — `value` remains a string.
- All existing snapshot tests for checkbox will update their `.verified.txt` to reflect the new jsEmit string, which is a legitimate change that the reviewer can approve.
- The runtime (`element.ts`) executes whatever jsEmit string the plan carries — zero runtime changes needed.
