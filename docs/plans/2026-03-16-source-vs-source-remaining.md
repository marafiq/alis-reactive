# Remaining: Source-vs-Source — TS Tests + Playwright + Sandbox View

## Context

Commit `0c1b5b5` added source-vs-source comparison to C# conditions DSL.
C# side is complete: 6 overloads, ValueGuard.RightSource, schema, 12 BDD tests.
Runtime change is in conditions.ts (resolves rightSource when present).

Missing per CLAUDE.md "every new primitive needs all three layers":

## 1. TS Unit Test

**File:** `Scripts/__tests__/when-comparing-two-sources.test.ts`

Test the runtime's `evaluateValueGuard` with `rightSource` present.
Boot a plan with two component entries and a conditional reaction where
the guard has `rightSource` instead of `operand`.

Tests needed:
- numeric left lte right — resolves both components, compares
- numeric left gt right — opposite result
- string left eq right — string coercion on both sides
- rightSource absent falls back to operand (existing behavior preserved)
- rightSource with different vendors (fusion vs native)

Pattern: same as `when-branching-on-conditions.test.ts` — create plan JSON
inline with `rightSource` field, boot, dispatch event, assert DOM.

## 2. Playwright Test

**File:** `tests/Alis.Reactive.PlaywrightTests/Conditions/WhenComparingTwoComponentValues.cs`

Tests needed:
- Two numeric inputs: set rate < budget → "Within budget" shown
- Change rate > budget → "Over budget" shown
- Two dropdowns: same value → "Match" shown, different → hidden

Uses the sandbox view from step 3.

## 3. Sandbox View

**File:** `Areas/Sandbox/Views/Conditions/SourceComparison.cshtml`
**Controller:** Add action to existing `ConditionsController`
**Model:** Use existing sandbox model or create minimal one with Rate + Budget + CareLevel + PreferredLevel

View demonstrates:
- Two FusionNumericTextBox components compared with Lte
- Two FusionDropDownList components compared with Eq
- Event payload vs component with Gt
- Source-vs-source in AND composition

## 4. CLAUDE.md

Add to the Conditions section:
- Source-vs-source: `p.When(comp1.Value()).Lte(comp2.Value())`
- ValueGuard.RightSource field
- Note: works in both Syntax 1 (direct) and Syntax 2 (lambda)

## Files to Touch

| File | Action |
|------|--------|
| `Scripts/__tests__/when-comparing-two-sources.test.ts` | ADD |
| `tests/.../Conditions/WhenComparingTwoComponentValues.cs` | ADD |
| `Areas/Sandbox/Views/Conditions/SourceComparison.cshtml` | ADD |
| `Areas/Sandbox/Controllers/ConditionsController.cs` | MODIFY (add action) |
| `CLAUDE.md` | MODIFY (document feature) |

Zero changes to any C# source, runtime, or schema files.
