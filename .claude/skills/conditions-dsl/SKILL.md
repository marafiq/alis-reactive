---
name: conditions-dsl
description: Use when writing conditional logic in reactive plans — When/Then/ElseIf/Else, all condition operators (Eq, Gt, Contains, NotNull, etc.), guard composition (And/Or/Not), source types (event args, component reads, source-vs-source), and Confirm guards. The conditions grammar for Alis.Reactive.
---

# Conditions DSL Grammar

## Full Shape

```
CONDITION :=
  p.When(SOURCE).OPERATOR [.COMPOSE]
    .Then(t => { PIPELINE })
   [.ElseIf(SOURCE).OPERATOR [.COMPOSE]
    .Then(t => { PIPELINE })]     -- repeatable
   [.Else(e => { PIPELINE })]     -- terminal (void)

CONFIRM :=
  p.Confirm("message")
    .Then(t => { PIPELINE })
```

Each `When/Then/Else` block is an **independent reaction**. Multiple blocks in one
pipeline don't interfere — they produce separate conditional reactions in the plan.

## Source — Where the Value Comes From

```
SOURCE :=
  -- Event payload (inside .Reactive): args is the event's object, its members are sources
  | args.Value                                           -- TProp from the member's ReadOnly<T>/Prop<T>
  | args.Slot.Get(x => x.Quiet)                          -- a member whose value is data, read further

  -- Data a callback received (CustomEvent<T>, ServerPush, SignalR, OnSuccess<T>, OnError<T>)
  | payload.Get(x => x.Score)                            -- Data<T>: a member by its camel-case JSON name
  | json.Get(j => j.Status)                              -- a response body

  -- Component read (anywhere: pipeline, response handler, DomReady)
  | comp.Value                                           -- any declared property
  | comp.StartDate                                       -- DateRangePicker
  | grid.GetSelectedRowIndexes()                         -- a method that returns, e.g. FusionGrid

  -- The page's URL, plugins and arrays
  | p.Url<TQuery>().Get(q => q.Tab)
  | slugify.Slug(planner.Wing)
  | p.Element("dom-card").ClassList.Items.Any(x => x == "care-memory")

-- Getting a component:
var comp = p.Component<FusionNumericTextBox>(m => m.Amount);
var comp = p.Component<FusionAutoComplete>(m => m.Name);
var comp = s.Component<FusionSwitch>(m => m.IsActive);    -- 's' in response handler
```

**A response body as source: supported.** Inside a response route, `json` is `Data<T>`:
`s.When(json.Get(j => j.Status)).Eq("approved")` starts a condition on the response body, and guards
compose with `.And(json.Get(j => j.Count)).Gt(0)` / `.Or(..)`
(`Conditions/ResponseConditions/Index.cshtml`).

## Operator — What to Check

```
OPERATOR :=
  -- Comparison (operand must match TProp type)
  | .Eq(value)              -- == value
  | .NotEq(value)           -- != value
  | .Gt(value)              -- > value
  | .Gte(value)             -- >= value
  | .Lt(value)              -- < value
  | .Lte(value)             -- <= value

  -- Presence (no operand)
  | .Truthy()               -- JS truthiness (non-null, non-0, non-"", non-false)
  | .Falsy()                -- JS falsiness (null, 0, "", false)
  | .IsNull()               -- === null or undefined
  | .NotNull()              -- !== null and !== undefined
  | .IsEmpty()              -- null or ""
  | .NotEmpty()             -- not null and not ""

  -- Membership (operands must match TProp)
  | .In(v1, v2, ...)        -- value in set
  | .NotIn(v1, v2, ...)     -- value not in set

  -- Range (operands must match TProp)
  | .Between(low, high)     -- low <= value <= high

  -- Text (TProp must be string)
  | .Contains("sub")        -- substring match
  | .StartsWith("pre")      -- prefix match
  | .EndsWith("suf")        -- suffix match
  | .Matches("regex")       -- regex test
  | .MinLength(n)           -- length >= n

  -- Array (TProp must be array, e.g. string[])
  | .ArrayContains(item)    -- array includes item

  -- Source-vs-source (right side is TypedSource, not literal)
  | .Eq(otherSource)        -- left == right (runtime values)
  | .NotEq(otherSource)
  | .Gt(otherSource)
  | .Gte(otherSource)
  | .Lt(otherSource)
  | .Lte(otherSource)
```

Every operator applies to every type except where the compiler limits it: ordering (`Gt`, `Gte`,
`Lt`, `Lte`, `Between`) to comparable values (numbers, dates, times and their nullable forms), the
text operators to text, `ArrayContains` to arrays, lists and enumerables. `Gt` on a string or
`Contains` on an int does not compile. These are extension methods in the `Alis.Reactive`
namespace.

## Compose — Combining Guards

```
COMPOSE :=
  | .And(SOURCE).OPERATOR                               -- after And, only And or Then
  | .Or(SOURCE).OPERATOR                                -- after Or, only Or or Then
  | .And(g => g.When(SOURCE).OPERATOR [.COMPOSE])       -- a group: evaluated as one term
  | .Or(g => g.When(SOURCE).OPERATOR [.COMPOSE])
  | .Not()                                              -- only right after one comparison: negates it

GROUP :=
  | g.When(SOURCE).OPERATOR [.And(..)... | .Or(..)...]  -- a group has no Then
  | g.Confirm("msg")
  | g.Not(n => n.When(..)...)                           -- negates a whole group
```

`And` and `Or` never mix in one chain, so precedence is always written, never folded:

```csharp
p.When(a).Eq(1).And(b).Gt(2).Then(..)                          // a && b
p.When(a).Eq(1).Or(b).Gt(2).Then(..)                           // a || b
p.When(a).Eq(1).And(g => g.When(b).Gt(2).Or(c).Truthy()).Then(..)   // a && (b || c)
p.When(a).Eq(1).And(b).Gt(2).Or(c)                             // does not compile
p.When(a).Eq(1).And(g => g.When(b).Gt(2).Then(..))             // does not compile: a group has no Then
p.When(a).Eq(1).And(b).Gt(2).Not()                             // does not compile; negate a group with g.Not(..)
```

## Type Safety — TProp Flows Through

```
Source Property Type        Available Operators
────────────────────       ────────────────────
string                     Eq, NotEq, In, NotIn, Contains, StartsWith, EndsWith,
                           Matches, MinLength, Truthy, Falsy, IsNull, NotNull,
                           IsEmpty, NotEmpty

int, long, decimal, double Eq, NotEq, Gt, Gte, Lt, Lte, Between, In, NotIn,
(and their nullable forms) Truthy, Falsy, IsNull, NotNull

bool                       Truthy, Falsy, Eq, NotEq, IsNull, NotNull

DateTime, DateOnly, TimeOnly Eq, NotEq, Gt, Gte, Lt, Lte, Between, IsNull, NotNull

string[], List<string>     ArrayContains, IsEmpty, NotEmpty, IsNull, NotNull, Truthy, Falsy
```

Coercion is automatic from C# type:

```
string    → coerce: "string"  (null → "")
int/long/decimal/double → coerce: "number" (NaN → 0)
bool      → coerce: "boolean" ("false" → false)
DateTime  → compared by its time (a Date on both sides)
string[]  → coerce: "raw"     (no coercion)
```

## Branch — Then / ElseIf / Else

```
BRANCH :=
  .Then(t => { PIPELINE })                              -- → BranchBuilder

BRANCH_CHAIN :=
  | .ElseIf(SOURCE).OPERATOR.Then(t => { PIPELINE })    -- next condition
  | .ElseIf(typedSource).OPERATOR.Then(...)             -- component source
  | .Else(e => { PIPELINE })                            -- fallback (terminal, void)
```

Each branch body is a full pipeline — Element, Component, Dispatch, HTTP, nested When.

## No Per-Command Guard

There is no per-command `.When()`: `HtmlElement` and a component's members have no `When`.
Guards are pipeline-level only: wrap the steps in a `p.When(SOURCE).OPERATOR.Then(...)` branch.
To guard a single `Text.Set`, put that one step inside the branch body.

For guarding Show/Hide/ClassList.Add, use full `When/Then`:
```
p.When(SOURCE).OPERATOR.Then(t => t.Element("id").Show());
```

## Confirm Guard

```
p.Confirm("Are you sure?")
 .Then(t => { PIPELINE })
```

Async halt — browser confirm dialog. OK → Then branch executes. Cancel → skipped.
Only in user-initiated pipelines (Click). Never in DomReady.

## Source-vs-Source

From `Conditions/NumericCondition/Index.cshtml`:

```
var hr = p.Component<FusionNumericTextBox>(m => m.HeartRate);
var threshold = p.Component<FusionNumericTextBox>(m => m.ThresholdValue);
p.When(hr.Value).Gte(threshold.Value)
    .Then(t => t.Element("threshold-result").Text.Set("Above threshold"))
    .Else(e => e.Element("threshold-result").Text.Set("Below threshold"));
```

Both sides must be `TypedSource<TProp>` with same `TProp`.

## Recipes — Common Patterns

In `Conditions/Guards/Index.cshtml` the payload is a custom event's (`CustomEvent<ScorePayload>`),
so `args` is `Data<ScorePayload>` and a member reads with `args.Get(x => x.Score)`. Inside a
component's `.Reactive`, the payload's members are read directly: `args.Value`.

**ElseIf chain (multi-tier):**
```
p.When(args.Get(x => x.Score)).Gte(90)
    .Then(then => then.Element("grade").Text.Set("A"))
    .ElseIf(args.Get(x => x.Score)).Gte(80)
    .Then(then => then.Element("grade").Text.Set("B"))
    .Else(else_ => else_.Element("grade").Text.Set("F"))
```

**And + Or (mixed — needs a group)** from `Conditions/EscalationRouting/Index.cshtml`:
```
p.When(assessment.Value).Gte(85m)
    .And(cs => cs.When(supervisorOverride.Checked).Truthy()
        .Or(careTrack.Value).Eq("memory"))
    .Then(then => then.Dispatch(EscalationRoutingEvents.ComponentRouteIsolation))
```

**Multiple independent conditions in one pipeline** from `Components/Fusion/DropDownList/Index.cshtml`:
```
p.When(args.Value).Eq("Electronics")
    .Then(t => t.Element("args-condition").Text.Set("electronics selected"))
    .Else(e => e.Element("args-condition").Text.Set("other category"));
var comp = p.Component<FusionDropDownList>(m => m.Category);
p.When(comp.Value).NotNull()
    .Then(then => then.Element("selected-indicator").Show())
    .Else(else_ => else_.Element("selected-indicator").Hide());
```
These are independent — both evaluate, neither blocks the other.

**Condition inside HTTP response (body or component source)** from `Conditions/ResponseConditions/Index.cshtml`:
```
.OnSuccess<ApprovalResponse>((json, s) =>
{
    s.When(json.Get(j => j.Status)).Eq("approved")
     .Then(then =>
     {
         then.Element("s1-badge").Show();
         then.Element("s1-result").Text.Set("approved");
     })
     .Else(el => el.Element("s1-result").Text.Set("not approved"));
})
```
