# BUG-006: FluentValidationAdapter Silently Drops GreaterThan, LessThan, NotEqual Comparisons

## Status: Bug — Silent Data Loss

## File
`Alis.Reactive.FluentValidator/FluentValidationAdapter.cs:244-266`

## How to Reproduce

1. Define a FluentValidation rule with `GreaterThan`, `LessThan`, or `NotEqual`:
   ```csharp
   public class OrderValidator : ReactiveValidator<OrderModel>
   {
       public OrderValidator()
       {
           RuleFor(x => x.Quantity).GreaterThan(0);
           RuleFor(x => x.Discount).LessThan(100);
           RuleFor(x => x.Password).NotEqual("password");
       }
   }
   ```
2. Register this validator with `.Validate<OrderValidator>("order-form")`.
3. Call `ExtractRules(typeof(OrderValidator), "order-form")`.
4. The adapter's `MapComponent` method hits the `case IComparisonValidator cv:` branch (line 244).
5. The switch only handles three of six `Comparison` enum values:
   - `Comparison.Equal` → extracts `"equalTo"` rule
   - `Comparison.GreaterThanOrEqual` → extracts `"min"` rule
   - `Comparison.LessThanOrEqual` → extracts `"max"` rule
6. `Comparison.GreaterThan`, `Comparison.LessThan`, and `Comparison.NotEqual` fall through with no extraction.
7. The `result` list is empty for these validators. **No client-side rule is generated.**
8. The form submits without validating these constraints. Server-side FluentValidation still catches them, but the client-side experience is broken.

## Deep Reasoning: Why This Is a Real Bug

The FluentValidation `Comparison` enum has exactly 6 values: `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`. The adapter handles exactly half of them. The missing three are not exotic — they are among the most commonly used validators.

This is not a "future feature" gap — it is an **incomplete implementation** of an existing capability. The framework already has the JSON schema support for `"min"` and `"max"` rules, and the runtime `validation.ts` already handles `min`, `max` checks (lines 184-187). The only missing piece is the adapter's extraction logic.

The silent nature is the real danger: the developer writes `.GreaterThan(0)`, expects client validation, sees no errors during development (because they test with valid data), and ships. In production, users discover they can submit `0` or negative values because the client validation is missing. The server rejects the request, but the UX is degraded — the whole point of client-side validation is to catch errors before the round-trip.

## How Fixing This Improves the Codebase

1. **Complete the comparison mapping**: Map `GreaterThan` → `"min"` (with adjusted constraint to be exclusive), `LessThan` → `"max"` (with adjusted constraint), `NotEqual` → new rule type or guard.
2. **Parity with server-side**: Every FluentValidation comparison that can be expressed client-side should be extracted.
3. **Fail-fast for truly unsupported validators**: If a comparison type cannot be mapped client-side, the adapter should log a warning or throw, not silently skip.

## How This Fix Will Not Break Existing Features

- The fix adds new `else if` branches to the existing `IComparisonValidator` switch. Existing branches for `Equal`, `GreaterThanOrEqual`, `LessThanOrEqual` are unchanged.
- The runtime `validation.ts` already supports `min` and `max` rule types — no runtime changes needed for the numeric comparisons.
- For `NotEqual`, a decision is needed: either add a new `"notEqual"` rule type (requires schema + runtime addition) or map it to a guard-based approach. Either way, existing validation behavior is additive, not destructive.
- Existing FluentValidator tests will continue to pass. New tests should be added for the three missing comparison types.
