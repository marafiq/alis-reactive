# Plan: Source-vs-Source Comparison

## Problem

Two component values cannot be compared in conditions:
```csharp
var rate = p.Component<FusionNumericTextBox>(m => m.MonthlyRate);
var budget = p.Component<FusionNumericTextBox>(m => m.Budget);

p.When(rate.Value()).Lte(budget.Value())  // ❌ Lte takes TProp, not TypedSource<TProp>
```

`ConditionSourceBuilder.Lte(TProp operand)` only accepts literal values.
No overload accepts another `TypedSource<TProp>`.

## Solution

Add 6 overloads to `ConditionSourceBuilder` that accept `TypedSource<TProp>` as the right-hand side.
Add `RightSource` property to `ValueGuard` for the plan JSON.
Add 3 lines to the JS runtime to resolve `rightSource`.

## Changes

### 1. ValueGuard.cs — add RightSource (1 file)

```csharp
// Alis.Reactive/Descriptors/Guards/ValueGuard.cs

[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public BindSource? RightSource { get; }

// New constructor for source-vs-source:
public ValueGuard(BindSource left, string coerceAs, string op, BindSource right)
{
    Source = left;
    CoerceAs = coerceAs;
    Op = op;
    RightSource = right;
}
```

Backward compatible: `RightSource` is null for all existing guards. Ignored in JSON when null.

### 2. ConditionSourceBuilder.cs — add 6 overloads (1 file)

```csharp
// Alis.Reactive/Builders/Conditions/ConditionSourceBuilder.cs

// Source-vs-source comparison — right side is a TypedSource, not a literal
public GuardBuilder<TModel> Eq(TypedSource<TProp> right)   => BuildVsSource(GuardOp.Eq, right);
public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource(GuardOp.Neq, right);
public GuardBuilder<TModel> Gt(TypedSource<TProp> right)   => BuildVsSource(GuardOp.Gt, right);
public GuardBuilder<TModel> Gte(TypedSource<TProp> right)  => BuildVsSource(GuardOp.Gte, right);
public GuardBuilder<TModel> Lt(TypedSource<TProp> right)   => BuildVsSource(GuardOp.Lt, right);
public GuardBuilder<TModel> Lte(TypedSource<TProp> right)  => BuildVsSource(GuardOp.Lte, right);

private GuardBuilder<TModel> BuildVsSource(string op, TypedSource<TProp> right)
{
    var leftSource = _typedSource.ToBindSource();
    var rightSource = right.ToBindSource();
    var guard = new ValueGuard(leftSource, _coerceAs, op, rightSource);
    return WrapGuard(guard);
}
```

### 3. Scripts/types.ts — add rightSource (1 line)

```typescript
export interface ValueGuard {
  kind: "value";
  source: BindSource;
  coerceAs: "string" | "number" | "boolean" | "date" | "raw";
  op: GuardOp;
  operand?: unknown;
  rightSource?: BindSource;  // NEW
}
```

### 4. Scripts/conditions.ts — resolve rightSource (3 lines)

```typescript
// In evaluateValueGuard(), after resolving the left side:
const rawOp = guard.rightSource
  ? resolveSourceAs(guard.rightSource, guard.coerceAs, ctx)
  : guard.operand;
// Then continue with existing coercion + comparison logic
```

### 5. Schemas/reactive-plan.schema.json — add rightSource

Add `rightSource` as optional `$ref` to `BindSource` in the ValueGuard definition.

### 6. Tests

**C# unit tests (new):**
```
tests/Alis.Reactive.UnitTests/Conditions/WhenComparingTwoSources.cs
├── Two_numeric_components_lte
├── Two_string_components_eq
├── Two_date_components_gt
├── Event_source_vs_component_source
├── Component_vs_component_different_vendors
├── Source_vs_source_guard_has_null_operand
├── Source_vs_source_guard_has_right_source
├── Source_vs_source_combined_with_and
├── Source_vs_source_combined_with_confirm
├── Source_vs_source_in_elseif_chain
├── Source_vs_source_in_on_success_handler
└── Schema_validates_source_vs_source_plan
```

**TS unit tests (new):**
```
Scripts/__tests__/when-comparing-two-sources.test.ts
├── numeric_left_lte_right
├── string_left_eq_right
├── right_source_resolved_from_component
├── right_source_resolved_from_event
├── right_source_with_coercion
└── missing_right_source_falls_back_to_operand
```

**Playwright test (new):**
```
tests/Alis.Reactive.PlaywrightTests/Conditions/WhenComparingTwoComponentValues.cs
├── rate_within_budget_shows_status
├── rate_exceeds_budget_shows_warning
└── changing_either_component_updates_comparison
```

**Sandbox view (new):**
```
Areas/Sandbox/Views/Conditions/SourceComparison.cshtml
```

## File Count

| Action | Count | Files |
|--------|-------|-------|
| MODIFY | 4 | ValueGuard.cs, ConditionSourceBuilder.cs, types.ts, conditions.ts |
| MODIFY | 1 | reactive-plan.schema.json |
| ADD | 1 | C# unit test file |
| ADD | 1 | TS unit test file |
| ADD | 1 | Playwright test file |
| ADD | 1 | Sandbox view |
| **Total** | **9** | |

## What Does NOT Change

- All existing condition syntax (When, Then, ElseIf, Else, And, Or, Not)
- All existing descriptors (Guard, AllGuard, AnyGuard, InvertGuard)
- All existing builder classes (ConditionSourceBuilder, GuardBuilder, BranchBuilder)
- All existing component extensions (.Value() returns TypedComponentSource)
- Confirm guard
- Per-action When guard
- FluentValidator
- Analyzers
- All 907 existing tests

## Usage Examples

```csharp
// ── Two numeric components ──
var rate = p.Component<FusionNumericTextBox>(m => m.MonthlyRate);
var budget = p.Component<FusionNumericTextBox>(m => m.Budget);

p.When(rate.Value()).Lte(budget.Value())
    .Then(t => t.Element("status").SetText("Within budget"))
    .Else(e => e.Element("status").SetText("Over budget"));

// ── Event payload vs component ──
// CustomEvent<InvoicePayload>("invoice-received", (args, p) => {
var maxRate = p.Component<FusionNumericTextBox>(m => m.MaxAllowedRate);

p.When(args, x => x.InvoiceAmount).Gt(maxRate.Value())
    .Then(t => t.Element("warning").Show())
    .Else(e => e.Element("warning").Hide());

// ── Source-vs-source in AND composition ──
p.When(rate.Value()).Gt(0m)
    .And(rate.Value()).Lte(budget.Value())
    .Then(t => t.Element("status").SetText("Valid"))
    .Else(e => e.Element("status").SetText("Invalid"));

// ── Source-vs-source in OnSuccess ──
p.Post("/api/load")
 .Response(r => r.OnSuccess(s =>
 {
     var current = p.Component<FusionNumericTextBox>(m => m.CurrentBalance);
     var minimum = p.Component<FusionNumericTextBox>(m => m.MinimumBalance);

     s.When(current.Value()).Gte(minimum.Value())
         .Then(t => t.Element("balance-status").SetText("OK"))
         .Else(e => e.Element("balance-status").AddClass("warning"));
 }));

// ── Source-vs-source with Confirm ──
p.When(rate.Value()).Gt(budget.Value())
    .And(g => g.Confirm("Rate exceeds budget. Continue?"))
    .Then(t => t.Dispatch("submit"))
    .Else(e => e.Element("status").SetText("Cancelled"));
```

## Plan JSON (new shape)

```json
{
  "kind": "value",
  "source": { "kind": "component", "componentId": "MonthlyRate", "vendor": "fusion", "readExpr": "value" },
  "coerceAs": "number",
  "op": "lte",
  "rightSource": { "kind": "component", "componentId": "Budget", "vendor": "fusion", "readExpr": "value" }
}
```

When `rightSource` is absent (all existing plans), behavior is identical.
When `rightSource` is present, runtime resolves it instead of using literal `operand`.

## Quality Gate

1. `dotnet build` — all projects compile
2. `npm run build` — JS bundle builds
3. `npm test` — all TS tests pass (432 + new)
4. `dotnet test Alis.Reactive.UnitTests` — pass (150 + new)
5. `dotnet test Alis.Reactive.Native.UnitTests` — pass (35, unchanged)
6. `dotnet test Alis.Reactive.Fusion.UnitTests` — pass (61, unchanged)
7. `dotnet test Alis.Reactive.FluentValidator.UnitTests` — pass (43, unchanged)
8. `dotnet test Alis.Reactive.PlaywrightTests` — pass (186 + new)
9. Schema validates all new plan JSON
10. All 907 existing tests pass BEFORE any new tests are added
