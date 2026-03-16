# FAIL-FAST-025: validation.ts evalCondition Returns true When Source Field Not Found

## Status: Closed — Fixed

evalCondition returns null when source unavailable; orchestrator blocks (adds to summary).

## File
`Scripts/validation.ts:144-148`

## How to Reproduce

1. Define a conditional validation rule:
   ```csharp
   WhenField(x => x.IsActive, true, () => {
       RuleFor(x => x.Email).NotEmpty();
   });
   ```
2. The validation descriptor contains a condition: `{ "field": "IsActive", "op": "eq", "value": "True" }`.
3. Remove the `IsActive` checkbox from the form (or typo the field name in the validator).
4. `evalCondition` runs:
   ```typescript
   const srcField = byName.get(cond.field);
   if (!srcField || !srcField.fieldId || !srcField.vendor || !srcField.readExpr) return true;
   ```
5. `srcField` is `undefined` because the field is not in the descriptor.
6. The condition returns `true` — the conditional rule **always applies**.
7. Email validation fires even when `IsActive` is unchecked/not present.

## Deep Reasoning: Why This Is a Real Bug

Returning `true` for a missing condition field means "the condition is met — apply the rule." This is the wrong default. If the condition field cannot be evaluated (because it does not exist in the form), the correct behavior is either:
- `return false` — "condition cannot be verified, skip the rule" (safer for optional fields)
- `throw` — "condition field missing, this is a configuration error" (fail-fast, per Rule 8)

The current `return true` default means: if a conditional validation rule references a field that is not present (perhaps it was removed during a UI refactor), ALL rules that were conditional on that field become unconditional. Validation that was supposed to fire only when "IsActive" is checked now fires always.

This is dangerous because it causes **over-validation**, not under-validation. The form becomes harder to submit (unexpected required fields), and the developer has no clear error message to diagnose why.

## How Fixing This Improves the Codebase

1. **Correct default**: Return `false` when the condition field is missing — the condition cannot be evaluated, so the conditional rule should not apply.
2. **Log warning**: Add `log.warn("condition field not found", { field: cond.field })` to help developers diagnose missing fields.
3. **Consistent with isHidden check**: Line 40 already skips validation for hidden elements. A missing condition field should similarly skip the conditional rule, not force it.

## How This Fix Will Not Break Existing Features

- All existing forms have their condition source fields present in the descriptor. The `byName.get(cond.field)` lookup succeeds, and the existing evaluation logic runs normally.
- The fix only changes behavior when a condition field is missing — which is always a misconfiguration.
- Changing `return true` to `return false` makes the behavior more permissive (fewer rules fire), not more restrictive. Users will not see new validation errors.
