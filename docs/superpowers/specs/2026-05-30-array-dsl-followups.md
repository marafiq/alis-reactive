# Array DSL as a first-class citizen — value-routing law, completion + follow-ups

Date: 2026-05-30
Supersedes the earlier "recorded follow-ups" framing, which was a patch mentality. The array DSL
is not a feature bolted on; it completes one law the framework already had.

## The one law

A browser object (component, plugin, app object, DOM node) has members. A member's value is a
scalar, an object, or an **array**. The framework's whole job is:

> read any member of any browser object; route any value to any member of any browser object —
> expressed in typed C#, compiled to a deterministic plan, generated into TS, executed by a dumb
> runtime that keeps pure reads **sync** and only HTTP / confirm / inject **async**.

The array DSL completes that law for **array-typed members**: before it you could *read* an array
member (`multiSelect.Value() : string[]`, `kanban.Cards() : Card[]`) but it was hard to express
intent over the **elements' own members**. Now `x => x.Status == Active`, `x => x.Balance`,
`x => x.GetDay()` are first-class — the full member graph of every element, deterministic and typed.

## What was already possible (no new code) — the honest correction

`TypedSource<T>` is the universal value intake and has **no array restriction**. So a
DSL-transformed array (`ReactiveArray.AsSource()`) and a scalar result (`ReactiveValue<T>`) already
flow today into:

- `p.When(...).NotEmpty()` / comparisons — `ConditionStart`/`GuardBuilder` take `TypedSource<TProp>`.
- `Element.SetText(TypedSource<TProp>)` — `ElementBuilder.cs:87`.
- `DispatchPayloadBuilder.Set(field, TypedSource<T>)`.
- `gather.Include(...)` / `gather.Plugin(...)` — array members route into the HTTP payload.

And the read+transform half already spans components: `p.From(component.arrayMember())` seeds a
`ReactiveArray<T>` from any `TypedSource<T[]>` (`PipelineBuilder.Arrays.cs:15`). No reinvention
happened — `ObjectExpression`, `ArrayExpression`, the element scope, and `RuntimePath.call` were all
reused, not duplicated.

## What shipped in this pass (the missing edge — DONE)

The one asymmetry was the **sink**: a transformed array could reach `When`/`SetText`/`dispatch`/
`gather`, but **not** a component's `dataSource` member — `SetDataSource` only accepted
`ResponseBody`/event-payload. Completing it makes routing symmetric: *any value, including a
transformed array, → any array-typed member sink.* Implemented:

- `SetDataSource<TModel, TElement>(TypedSource<TElement[]>)` on **FusionGrid, FusionDropDownList,
  FusionMultiSelect, FusionAutoComplete, FusionMultiColumnComboBox** (single `EmitSet`) and
  **FusionKanban** (`EmitSet` + `EmitCall(dataBind)`, matching its established pattern). Pattern is
  `ElementBuilder.SetText<TProp>(TypedSource<TProp>)` — the abstract base with a free type
  parameter (NOT `TypedComponentSource<string>`). `EmitSet` + `ToValueExpression()` already existed;
  `InternalsVisibleTo` already grants Fusion access. Zero plan-domain / TS / runtime change.
- `FusionGrid.Data<TModel, TRow>()` — the read counterpart (mirrors `FusionKanban.Cards()`),
  so the rows on screen can be re-filtered and rebound without re-fetching.
- Comparison Shape in `ElementExpressionCompiler` is now **member-driven** (from the typed member
  operand), matching `ConditionSourceBuilder` instead of taking the left operand positionally. The
  runtime coerces both operands to one Shape before `===`, so the Shape must come from the member.

Browser proof: `WhenBindingArrayToGrid` — roster loads once, `OrderBy` transform routes into the
grid (`SetDataSource(TypedSource<T[]>)`), and "Show Active Only" re-filters the grid's own rows
client-side (`Data()` → `Where` → rebind), no HTTP round-trip. Sandbox: `/Sandbox/Components/ArrayGrid`.

## Genuinely-remaining follow-ups (NOT made — broader framework)

### 1. C# sync-condition marker for array-op predicates — LOW priority, defense-in-depth

`ArrayOperationExpression.Predicate` is typed `ConditionGraph?` (the broad base). The expression
compiler only ever emits `compare/all/any/not` — there is no path to `Confirm` — so the sync-only
constraint is already enforced behaviorally at lambda-compile time. Narrowing the type to an
`ISyncCondition` marker would make the compiler a hard gate, matching the generated TS
`ValidationCondition` union. Add the marker to `ConditionGraph` only when that hierarchy is being
touched for another reason; not standalone work.

### 2. Per-element side-effecting reaction (foreach) — distinct primitive, narrower than first thought

The "update each row in a grid" case is **already solved** by the functional rebind shipped above:
`grid.SetDataSource(roster.Where(...).Select(...).OrderBy(...).AsSource())` replaces the whole data
source — which is how every existing sandbox view updates a grid. A `ForEach` reaction is needed
ONLY for true imperative per-rendered-row mutation (e.g. EJ2 `Row.setCellValue` on an
already-rendered row without replacing the dataSource). That is a genuinely new reaction kind
(`ReactionGraph` has Sequence/Parallel/Branch/Set/Call/Request/Dispatch/Inject/ShowValidationErrors
— no iteration). It reuses the element-scope stack already in place, is a Layer-1→3 primitive, and
belongs in its own slice. Until then, imperative per-element mutation stays in the plugin escape hatch.

A known DSL boundary worth recording: element predicates compare members to **literals/captured
constants** only (the compiler evaluates the non-element side to a literal). Comparing an element
member to a *runtime* value (e.g. a dropdown's current selection) is not yet expressible in a
predicate — it would need the predicate's right operand to accept a `ValueExpression` source, not
just a literal. Recorded as a future capability, not made here.
