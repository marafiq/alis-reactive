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
  -- Event args (inside .Reactive or CustomEvent<T> handler)
  | args, x => x.Property                              -- TProp inferred from property type

  -- Component read (anywhere — pipeline, response handler, DomReady)
  | comp.Value()                                        -- TypedComponentSource<T>
  | comp.StartDate()                                    -- DateRangePicker only
  | comp.EndDate()                                      -- DateRangePicker only

-- Getting a component reference:
var comp = p.Component<FusionNumericTextBox>(m => m.Amount);
var comp = p.Component<FusionAutoComplete>(m => m.Name);
var comp = s.Component<FusionSwitch>(m => m.IsActive);    -- 's' in response handler
```

**NOT supported as source:** `ResponseBody<T>` — `When(json, x => x.Prop)` does NOT compile.
`ResponseBody<T>` is a phantom. Only `SetText(json, x => x.Prop)` has that overload.
For conditions on response data, use component reads or dispatch to a typed CustomEvent.

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

All operators return `GuardBuilder<TModel>` → chain to `.Then()` or `.COMPOSE`.

## Compose — Combining Guards

```
COMPOSE :=
  -- Direct (flat — produces AllGuard or AnyGuard)
  | .And(SOURCE).OPERATOR                               -- both must be true
  | .Or(SOURCE).OPERATOR                                -- either can be true

  -- Lambda (nested — for complex boolean trees)
  | .And(cs => cs.When(SOURCE).OPERATOR [.COMPOSE])     -- inner guard group
  | .Or(cs => cs.When(SOURCE).OPERATOR [.COMPOSE])

  -- Negation
  | .Not()                                              -- invert guard

-- Chaining:
  .And().And().And()  →  flat AllGuard([g1, g2, g3])
  .Or().Or()          →  flat AnyGuard([g1, g2])
  .And(cs => cs...)   →  nested tree (for mixed And/Or)
```

## Type Safety — TProp Flows Through

```
Source Property Type        Available Operators
────────────────────       ────────────────────
string                     Eq, NotEq, In, NotIn, Contains, StartsWith, EndsWith,
                           Matches, MinLength, Truthy, Falsy, IsNull, NotNull,
                           IsEmpty, NotEmpty

int, long, decimal, double Eq, NotEq, Gt, Gte, Lt, Lte, Between, In, NotIn,
                           Truthy, Falsy, IsNull, NotNull

bool                       Truthy, Falsy, Eq, NotEq, IsNull, NotNull

DateTime                   Eq, NotEq, Gt, Gte, Lt, Lte, Between

string[]                   ArrayContains, IsNull, NotNull, Truthy, Falsy
```

Coercion is automatic from C# type:

```
string    → coerce: "string"  (null → "")
int/long/decimal/double → coerce: "number" (NaN → 0)
bool      → coerce: "boolean" ("false" → false)
DateTime  → coerce: "string"  (compared as ISO string)
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

## Per-Command Guard

```
PER_COMMAND_GUARD :=
  var el = p.Element("id");
  el.SetText(typedSource);                               -- returns ElementBuilder
  el.When(payload, x => x.Prop, csb => csb.OPERATOR);   -- guard on previous command
```

`.When()` is ONLY on `ElementBuilder` — returned by `.SetText(TypedSource)`,
`.SetText(BindSource)`, `.SetHtml(TypedSource)`, `.SetHtml(BindSource)`.

**NOT after:** `.Show()`, `.Hide()`, `.AddClass()`, `.SetText("static")`,
`.SetText(payload, x => x.Prop)` — these return `PipelineBuilder`.

For guarding Show/Hide/AddClass, use full `When/Then`:
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

```
var left = p.Component<FusionNumericTextBox>(m => m.Rate);
var right = p.Component<FusionNumericTextBox>(m => m.Budget);

p.When(left.Value()).Gt(right.Value())
 .Then(t => t.Element("warning").Show())
 .Else(e => e.Element("warning").Hide());
```

Both sides must be `TypedSource<TProp>` with same `TProp`.

## Recipes — Common Patterns

**ElseIf chain (multi-tier):**
```
p.When(args, x => x.Score).Gte(90)
 .Then(t => t.Element("grade").SetText("A"))
 .ElseIf(args, x => x.Score).Gte(80)
 .Then(t => t.Element("grade").SetText("B"))
 .ElseIf(args, x => x.Score).Gte(70)
 .Then(t => t.Element("grade").SetText("C"))
 .Else(e => e.Element("grade").SetText("F"));
```

**And + Or (mixed — needs lambda):**
```
p.When(args, x => x.Score).Gte(90)
 .And(cs => cs.When(args, x => x.Role).Eq("admin")
              .Or(args, x => x.Role).Eq("nurse"))
 .Then(t => t.Element("badge").Show());
```

**Multiple independent conditions in one pipeline:**
```
p.When(args, x => x.Value).Eq("smith")
 .Then(t => t.Element("name-match").Show())
 .Else(e => e.Element("name-match").Hide());

p.When(comp.Value()).NotNull()
 .Then(t => t.Element("has-value").Show())
 .Else(e => e.Element("has-value").Hide());
```
These are independent — both evaluate, neither blocks the other.

**Condition inside HTTP response (component source only):**
```
.OnSuccess<TResp>((json, s) =>
{
    s.Element("name").SetText(json, x => x.Name);       -- SetText works with ResponseBody
    var comp = s.Component<FusionSwitch>(m => m.Active);
    s.When(comp.Value()).Truthy()                        -- When needs component source
     .Then(t => t.Element("status").SetText("Active"));
})
```
