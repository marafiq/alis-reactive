# Conditions

A condition and the branch it decides: the pipeline's door, `When`, `ElseIf` and `Else` over a typed condition built
from comparisons, tests and `And`, `Or` and `Not`; and the one condition core that this door, a list's predicate and a
validator's condition all write. Module 3 of `spec/CLAUDE.md`.

## Core questions

- Q4: a term over a source that reads nothing does not hold, and a term with a method call runs only when it is
  reached (D4.7, D8.5).
- Q5: a method's result a term reads is the call at its line (D5.11).
- Q6: a list's predicate is a lambda the lambda door translates into the same core (D8.2, N8.1).
- Q7: every operand takes a literal or a source, `Matches` a literal pattern only (D7.5, N7.3).
- Q8: one condition core (D8.1), two doors that write it (D8.2, D8.3), one syntax for a pipeline condition (D8.6), a
  branch as a block (D8.7) and a lexical scope (D8.8), one meaning of comparison and logic (D8.9), and the forms a
  source is held in (D8.10).
- Q11: the steps after a branch that awaits wait for it (D11.5).
- Q17: a user's decision is a component's `MethodAsync<bool>` read in a condition (D17.9, N17.3).

## Boundaries

The validator's own condition and rule types (`ClientValidation*` on main, `IConditionBuilder`, `ComparisonRule`,
`FixedComparisonRule`, `MemberComparisonRule`, `BrowserRule` and `BrowserRuleSet` on the redesign) are module 8's, which
writes the same core through the lambda door. A Fusion template's conditional (`FusionConditionalBuilder<TModel>`) is
the template engine's, not this core (D8.4), and module 22's. The sources a condition reads (`TypedSource<T>` and its
kin) are module 2's. A date's or a time's comparison defects, X6.57 and X6.58, are module 4's, whose row holds a
condition's promise for each (D6.12).

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M3.N1` new: the value tests (`IsNull`, `NotNull`, `IsEmpty`, `NotEmpty`), the text tests (`Contains`, `StartsWith`,
  `EndsWith`, `Matches`, `MinLength`) and the membership tests (`In`, `NotIn`, `Between`) that main's
  `ConditionSourceBuilder<TModel, TProp>` and the redesign's `ConditionOperators` over
  `ConditionSourceBuilder<TModel, TProp, TNext>` carry, in N8.2's form: extension methods on a typed source that return
  a `TypedSource<bool>`, which `When` and `ElseIf` take and a list's predicate returns, each operand a literal or, where
  N7.3 gives one, a source (M3.D1). They replace both lines' tests, as N8.2 replaces their comparisons.

The redesign's other doors go to these and the core's: its staged group types (`Guard`, `AllGuard`, `AnyGuard`,
`GroupGuard`, `GroupAll` and `GroupAny`) and `ConditionExtensions.When` to N8.2's nesting of `TypedSource<bool>`;
`ConditionExtensions.Confirm` to N17.3; `ArrayContains` over a list a source holds to the list operations'
`Any(predicate)` with a comparison of its item (D6.2, N6.1, N8.2); and its public condition core, `Condition`,
`Comparison`, `TextTest` and `ValueTest`, which it opens to a validation integration through
`BrowserRule.When(Condition)` and Fusion's grid validation reads
(`Alis.Reactive.Fusion/Components/FusionGrid/FusionGridValidation.cs:136-138` at 65fdf118d), to the internal one (D8.1,
N8.1): D8.2's lambda door writes a validator's condition from its lambda, so no author names a node type.

The meaning `M3.D1` is in `spec/Conditions/features.json`, on the feature it gives.

## Improvements

- One form for every condition: N8.2 and M3.N1 make each comparison and test a `TypedSource<bool>`, so `When(..)`,
  `ElseIf(..)`, a list's predicate and a group read alike, and a group is C# nesting (`a.And(b.Or(c))`). Main writes a
  group through a builder lambda (`.And(g => g.When(..))`) and two syntaxes for an operand
  (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.Conditions.cs:15-49` at 623dd4a27), and the redesign keeps the
  lambda group and stages it through six more guard types, `Guard`, `AllGuard`, `AnyGuard`, `GroupGuard`, `GroupAll`
  and `GroupAny` (`Alis.Reactive/PlanAuthoring/Conditions/GuardBuilder.cs:29-185` at
  65fdf118d).
- Logic with nothing in it reads nothing (D8.9). The redesign's browser computes `and` and `or` as two-valued
  JavaScript, `not` as `!truthy` and `neq` as `!equal`, so `not` of a term that reads nothing holds and `neq` against a
  missing value holds (`Alis.Reactive.Assets/runtime/grammar/evaluate.ts:93-99` at 65fdf118d).
- A condition is a boolean, not JavaScript's truthiness. Both lines test any value with `Truthy()` and `Falsy()`
  (`Alis.Reactive.Assets/runtime/grammar/operators.ts:71-73` at 65fdf118d), so 0 and the empty text are false where .NET
  has no such reading; a `TypedSource<bool>` is its own condition and `Not()` its negation, and any other value is
  tested by a comparison or an M3.N1 test.
- A user's decision is no core door (D17.9). `Confirm` writes a call to a plugin only Fusion's browser code provides
  (`Alis.Reactive/PlanAuthoring/Conditions/ConditionExtensions.cs:30` at 65fdf118d); the condition reads a component's
  `MethodAsync<bool>` (N17.3), as any source is read.
- The value tests' meaning over nothing is stated once (M3.D1). The redesign writes `IsNull` as `eq` with null
  (`Alis.Reactive/PlanModel/Conditions/Condition.cs:232` at 65fdf118d), and its `equal` takes a missing value as null,
  so `IsNull` holds over a member past a holder that holds null (`Alis.Reactive.Assets/runtime/grammar/operators.ts:4-8`
  at 65fdf118d), where D8.9 holds no comparison with it; M3.D1 reads each test as the .NET term it stands for, under
  D8.9.
- A date or a time compares as .NET compares it, to the tick (D6.12, X8.27). The redesign compares two `Date`s by
  `getTime` and orders by it (`Alis.Reactive.Assets/runtime/grammar/operators.ts:4-8`,
  `Alis.Reactive.Assets/runtime/grammar/operators.ts:17-18` at 65fdf118d), so a wall time daylight saving skips orders
  as the hour after it and a tick past a time equals it, and it refuses ordering a `TimeSpan`
  (`Alis.Reactive/Members/BrowserOrder.cs:10-15` at 65fdf118d). The core reads each operand through its key
  (`spec/Core/conversions.md`), keeps the redesign's 16 date tests and orders a `TimeSpan`; `In`, `NotIn` and `Between`
  over a date compare keys too (M3.D1).
