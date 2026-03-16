# BUG-002: validate() Returns true When Form Container Not Found

## Status: Closed — Fixed

orchestrator.ts now returns false when container missing and fields.length > 0.

## File
`Scripts/validation.ts:28`

## How to Reproduce

1. Configure a plan with validation on a form:
   ```csharp
   p.Post("/api/save", g => g.IncludeAll())
    .Validate<MyValidator>("my-form")
    .Response(r => r.OnError(e => e.ValidationErrors("my-form")));
   ```
2. In the view, typo the form container ID — use `id="myform"` instead of `id="my-form"`.
3. Click the submit button.
4. The `validate(desc)` function calls `document.getElementById("my-form")` which returns `null`.
5. Line 28: `if (!container) return true;` — validation passes.
6. The HTTP request fires with no validation. Invalid data reaches the server.

The same scenario occurs when the form container is dynamically rendered (partial view loaded via `Into()`) and the validation fires before the partial has loaded.

## Deep Reasoning: Why This Is a Real Bug

This violates the framework's own Rule 8: "No Fallbacks — Fail Fast." The current behavior is a **silent fallback** — when the form container is missing, validation silently passes instead of alerting the developer that something is misconfigured.

The danger is compounding: the developer has explicitly configured validation by calling `.Validate<MyValidator>("my-form")`. They expect the form to be validated. The runtime finds no container and says "everything is fine" — the exact opposite of what should happen.

Consider the production scenario: a layout change moves the form container to a partial view with a different ID. All validation silently stops working. No console error, no trace warning, no visual indicator. Invalid data starts flowing to the server, and the team only discovers this when production data is corrupted.

This is not a theoretical concern — ID mismatches between C# plan configuration and HTML views are among the most common developer mistakes in any server-rendered framework.

## How Fixing This Improves the Codebase

1. **Fail-fast on misconfiguration**: Throwing or logging an error when the container is missing catches bugs at development time, not in production.
2. **Consistent with `element.ts:10`**: The `mutateElement` function already throws `[alis] target not found: ${cmd.target}` when an element ID is wrong. Validation should follow the same pattern.
3. **Consistent with `trigger.ts:36`**: Component event triggers already throw `[alis] element not found: ${trigger.componentId}`. Same principle.

## How This Fix Will Not Break Existing Features

- The fix changes `return true` to `throw new Error(...)` (or at minimum `log.error(...)` + `return false`).
- No existing code path intentionally calls `validate()` with a non-existent container ID. If it did, that would itself be a bug.
- The `wireLiveClearing` function (line 91) already guards against missing containers with an early return — but that function is a setup function, not a correctness gate. The distinction is: wiring live clearing on a missing container is a no-op; validating on a missing container is a data integrity failure.
- All Playwright tests use correct form container IDs and will continue to pass.
