# Condition — Implementation Spec (scaffold)

> Grounded in actual source. Authoring: `Builders/PipelineBuilder.Conditions.cs`,
> `Builders/Conditions/{ConditionStart,ConditionSourceBuilder,GuardBuilder,BranchBuilder,ConditionContinuation}.cs`.
> Plan model: `PlanModel/ConditionGraph.cs`, `PlanModel/CompareOp.cs`, `CompareOperator`/`MinimumTextLength`
> in `PlanModel/PlanTerms.cs`, `BranchReaction`/`BranchCase`/`BranchGuard` in `PlanModel/ReactionGraph.cs`.
> Runtime: `Alis.Reactive.Assets/runtime/conditions/{sync-condition,conditions}.ts`,
> `runtime/execution/execute.ts` (`executeBranchFrom`), wire shapes `runtime/types/plan.ts:886-1105`.
> Names from [`03-naming.md`](../03-naming.md); responsibility/deps from [`02-micro-modules.md`](../02-micro-modules.md);
> acceptance cases from [`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md).
>
> A developer opening this file should be able to type the obvious body of every member below
> without making a design decision. Where a decision was made, it is stated and grounded.

---

## 1. Responsibility

**Condition is the `if / else-if / else` decision over readable values — first match wins — authored by
`When/Confirm` + the guard/branch builders, lowered to a `ConditionGraph` predicate, and evaluated by ONE
`CompareEngine` on both lanes.**

### Owns

- **Author (`→`)**: `PipelineBuilder.When/Confirm` entry, `ConditionStart` (for nested And/Or), the
  `ConditionSourceBuilder<TModel,TProp>` operator surface (the 21 `CompareOp` tokens + source-vs-source),
  `GuardBuilder` (And/Or/Not + Then), `BranchBuilder` (ElseIf/Else), the `ConditionContinuation` family
  (pipeline / branch / **standalone-with-no-`Then`**), and `ConditionComposition` (the All/Any flatten algebra).
- **Plan node**: `ConditionGraph` (`Compare`/`All`/`Any`/`Not`/`Confirm`), `ComparisonOperands` +
  `ComparisonRightOperand` (the left value + the present/absent right operand), `CompareOperator` (the single
  21-token op-list value object), and the `MinimumTextLength` operand value object. The `branch` *executable*
  (`BranchReaction`/`BranchCase`/`BranchGuard`) is a **Reaction** node, but its guard is a `ConditionGraph`,
  so the first-match wiring lives here.
- **Runtime (`⇒`)**: ONE `CompareEngine` (today `evaluateCompare` in `sync-condition.ts`); `evaluateCondition`
  (sync entry, validation lane) and `confirmThenEvaluate`/`evaluateConditionInCurrentLane` (the thin async
  wrapper — confirm is the only term that crosses to async).

### Depends on

| Module | Why | Used as |
|---|---|---|
| **Value** | The left operand and every right operand is a `ValueExpression`; the runtime reads them via `evaluateValue`. | `TypedSource<TProp>.ToValueExpression()`, `ValueExpression.LiteralRaw/Array`; `evaluateValue` injected into `CompareEngine` as the `ValueEvaluator`. |
| **Shape** | Each compare carries `shape` (left/operand) and `itemShape` (collection element); the engine `applyShape`s before comparing. | `Shape`, `Shape.ArrayOf`, `Shape.String/Number`, `RuntimeShape`, `applyShape`. |
| **Kind** | Every node carries `kind`; the runtime switch is exhaustive via `assertNever`. | `WriteOnlyPolymorphicConverter<ConditionGraph>`, `assertNever`. |

Condition does **not** depend on Reaction, Trigger, Plan, Request, Component, Validation, or Plugin. (Validation
*reuses* Condition's `CompareEngine` for `WhenField`, and Reaction *consumes* the `branch` node — both are
downward dependencies on Condition, not the reverse.)

### Fresh-design wins this module must lock in

- **`Standalone.Then` is unrepresentable.** The `standalone` continuation exposes no `Then` — a compile error
  replaces today's runtime `InvalidOperationException` in `StandaloneConditionContinuation.Then`.
- **ONE `CompareEngine` on both lanes.** The dual `conditions.ts` / `sync-condition.ts` divergence collapses:
  `sync-condition.ts` *is* the engine; `conditions.ts` becomes the async wrapper that delegates `compare` to it.
- **The 21 tokens come from ONE `CompareOperator`.** No second op array.
- **The branch lane is carried** (`ReactionLane`, stamped by `ReactionPipelineDraft`): a branch whose every
  guard is a compare/all/any/not is `SYNC`; a branch reaching a `confirm` is `ASYNC`. The runtime routes on
  that fact, never on `instanceof Promise`. (Today's `evaluateConditionInCurrentLane` probes `instanceof Promise`;
  the fresh wrapper keeps the same *behavior* — sync stays sync, confirm lifts to a Promise — but is reached
  only on the lane the plan already declared.)

---

## 2. Input → Output Contract

### What flows in (authoring)

```
When(TypedSource<TProp> source)            // component / url / plugin read, shape from TProp
When(TPayload phantom, x => x.Path)        // event-payload read,  scope "event"
When(ResponseBody<TPayload> body, x => …)  // HTTP success/error body read
Confirm(string message)                    // user-decision guard
```

`When` opens a `ConditionSourceBuilder<TModel,TProp>` carrying the left `ValueExpression`
(`source.ToValueExpression()`) and `source.Shape`. An operator method (`.Eq`, `.Gt`, `.Truthy`, `.In`,
`.Between`, `.Contains`, `.MinLength`, `.ArrayContains`, the source-vs-source overloads, …) builds one
`ComparisonOperands` and calls `ConditionGraph.Compare(op, operands)`, then `_continuation.Wrap(...)` returns a
`GuardBuilder`. `GuardBuilder` composes (`And`/`Or`/`Not`) or branches (`Then` → `BranchBuilder` → `ElseIf`/`Else`).

### What it produces (plan node)

One `ConditionGraph` predicate, and — when routed through `.Then` — one `BranchReaction` whose ordered
`Cases` are `BranchCase`s (`When(condition)` guards in author order, optional trailing `Default` guard).

### Output wire shape (camelCase, from `plan.ts`)

```jsonc
// compare node — every compare is this shape; the family only changes `op` + the `right` operand variant
{ "kind": "compare",
  "left":  <ValueExpression>,
  "op":    "<token>",                 // one of the 21 CompareOp tokens
  "right": { "kind": "none" }         // unary
         | { "kind": "value", "value": <ValueExpression | ArrayExpression(2 for between) | TextLiteral | NumericLiteral | Literal> },
  "shape":     <Shape>,               // left/operand shape (Shape.None when unspecified)
  "itemShape": <Shape> }              // element shape; Shape.None except array-contains

{ "kind": "all", "terms": [ <ConditionGraph>, … ] }   // flattened, no nested "all"
{ "kind": "any", "terms": [ <ConditionGraph>, … ] }   // flattened, no nested "any"
{ "kind": "not", "term":  <ConditionGraph> }
{ "kind": "confirm", "message": "<text>" }

// branch executable (Reaction node; guard is a ConditionGraph)
{ "kind": "branch", "cases": [
    { "guard": { "kind": "when",    "condition": <ConditionGraph> }, "reaction": <ReactionGraph> },
    { "guard": { "kind": "default" },                                "reaction": <ReactionGraph> }  // optional, last, ≤1
] }
```

### Invariants (enforced by construction — null is unrepresentable, NOT guarded by runtime exceptions)

| Invariant | How it holds | Source |
|---|---|---|
| Left operand is always present | `ComparisonOperands` always has a `Left` (set in ctor); never null. | `ConditionGraph.cs:90-100` |
| Unary ops carry no right operand | `ComparisonOperands.Unary` → `ComparisonRightOperand.Absent` (`{"kind":"none"}`); `CompareOperator.RequiresRightOperand` is `false` only for the 6 unary tokens. | `ConditionGraph.cs:107-112`, `PlanTerms.cs:600` |
| Binary ops carry a present right operand | `ComparisonOperands.Binary`/`CollectionItem` → `ComparisonRightOperand.Present(value)`. | `ConditionGraph.cs:114-130` |
| `itemShape` is `Shape.None` except for `array-contains` | `Unary`/`Binary` pass `Shape.None`; only `CollectionItem` passes the element shape. | `ConditionGraph.cs:112,119,130` |
| `between` right is exactly two endpoints | `RangeOperands` builds a 2-element `ValueExpression.Array`; wire type is `RangeComparisonExpression = ArrayExpression & {items:[VE,VE]}`. | `ConditionSourceBuilder.cs:198-210`, `plan.ts:1070` |
| `min-length` is ≥ 0 | `MinimumTextLength.From(length, …)` throws `ArgumentOutOfRangeException` on negative — a real author-boundary check, not a plan-shape guard. | `PlanTerms.cs:176-196` |
| All/Any are flattened, never nested in their own kind | `ConditionComposition.Flatten{All,Any}` splices an existing `All`/`Any`'s terms before adding. | `ConditionContinuation.cs:17-41` |
| `confirm` message is non-empty | `Confirm(message)` → `ConfirmCondition` carries the raw string; emptiness is the author's call (no `PlanString` wrap today). Keep as-is unless the matrix demands a value object. | `ConditionGraph.cs:240-251` |
| First match wins | Runtime `executeBranchFrom` returns on the first matching guard; `default` always matches; `no-match` is a silent `void` (logged `branch.no-match`), not an error. | `execute.ts:304-322` |
| `Then` only after a pipeline/branch continuation | `standalone` continuation has no `Then` method (fresh design) — unrepresentable. | §1 win |

---

## 3. Public Surface

> Visibility mirrors the codebase: builder ctors are `internal` (devs reach them only through
> `p.When/p.Confirm`); operator + composition + branch methods are `public`; plan-model nodes have
> `internal` ctors + factory methods and `public` read-only properties. No new public API beyond what
> source already exposes.

### 3a. C# authoring surface

`PipelineBuilder<TModel>` (partial — `PipelineBuilder.Conditions.cs`):

```csharp
/// <summary>Starts a conditional branch from a typed source (component, plugin, or URL value).</summary>
public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source);

/// <summary>Starts a conditional branch from an event payload property.</summary>
public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path);

/// <summary>Starts a conditional branch from an HTTP response body property.</summary>
public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
    where TPayload : class;

/// <summary>Adds a user confirmation guard before proceeding with the pipeline.</summary>
public GuardBuilder<TModel> Confirm(string message);
```

Each `When`/`Confirm` calls `_draft.BeginBranch()` first (flushes any pending async reaction, opens the branch
block). `Confirm` wraps `ConditionGraph.Confirm(message)` directly into a `GuardBuilder`.

`ConditionSourceBuilder<TModel, TProp>` — the operator surface (every method returns `GuardBuilder<TModel>`):

```csharp
// Equality / Ordered (typed operand)
public GuardBuilder<TModel> Eq(TProp operand);      // CompareOperator.Eq
public GuardBuilder<TModel> NotEq(TProp operand);   // CompareOperator.Neq
public GuardBuilder<TModel> Gt(TProp operand);      // Gt / Gte / Lt / Lte …
public GuardBuilder<TModel> Gte(TProp operand);
public GuardBuilder<TModel> Lt(TProp operand);
public GuardBuilder<TModel> Lte(TProp operand);

// Presence (unary — no operand)
public GuardBuilder<TModel> Truthy();   public GuardBuilder<TModel> Falsy();
public GuardBuilder<TModel> IsNull();   public GuardBuilder<TModel> NotNull();
public GuardBuilder<TModel> IsEmpty();  public GuardBuilder<TModel> NotEmpty();

// Membership / Range
public GuardBuilder<TModel> In(params TProp[] values);
public GuardBuilder<TModel> NotIn(params TProp[] values);
public GuardBuilder<TModel> Between(TProp low, TProp high);

// Text / Regex / Length (operand is string/int regardless of TProp)
public GuardBuilder<TModel> Contains(string substring);
public GuardBuilder<TModel> StartsWith(string prefix);
public GuardBuilder<TModel> EndsWith(string suffix);
public GuardBuilder<TModel> Matches(string pattern);
public GuardBuilder<TModel> MinLength(int length);

// Collection-item
public GuardBuilder<TModel> ArrayContains(object item);

// Source-vs-source (right operand is another TypedSource<TProp>, lowered as a read)
public GuardBuilder<TModel> Eq(TypedSource<TProp> right);   // + NotEq/Gt/Gte/Lt/Lte overloads
```

`GuardBuilder<TModel>` — composition + branch routing:

```csharp
internal ConditionGraph ConditionGraph { get; }  // the predicate built so far

// And / Or — typed source, event payload, response body, or nested group
public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source);
public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source);
public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path);
public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path);
public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(ResponseBody<TPayload> body, Expression<Func<TPayload, TProp>> path) where TPayload : class;
public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(ResponseBody<TPayload> body, Expression<Func<TPayload, TProp>> path) where TPayload : class;
public GuardBuilder<TModel> And(Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner);   // nested group → flattened all
public GuardBuilder<TModel> Or(Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner);    // nested group → flattened any
public GuardBuilder<TModel> Not();

/// <summary>Executes the pipeline when the condition is true. Returns a branch builder for ElseIf/Else.</summary>
public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline);
```

`BranchBuilder<TModel>` — first-match chaining:

```csharp
public ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source);
public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path);
public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(ResponseBody<TPayload> body, Expression<Func<TPayload, TProp>> path) where TPayload : class;

/// <summary>Executes the pipeline when no previous condition matched (default case).</summary>
public void Else(Action<PipelineBuilder<TModel>> pipeline);   // appends BranchCase.Default; guards against ElseIf/Else after Else
```

`ConditionStart<TModel>` — the standalone entry used only inside nested `And`/`Or`. Exposes `When` overloads +
`Confirm`, builds against the **standalone** continuation (so the returned guard's `Then` is unreachable — fresh
design makes that a compile-time absence, not the runtime throw at `ConditionContinuation.cs:138`).

### 3b. C# plan-model surface

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ConditionGraph>))]
public abstract class ConditionGraph
{
    private protected ConditionGraph();
    internal static ConditionGraph Compare(CompareOperator op, ComparisonOperands operands);
    internal static ConditionGraph All(params ConditionGraph[] terms);
    internal static ConditionGraph Any(params ConditionGraph[] terms);
    internal static ConditionGraph Not(ConditionGraph term);
    internal static ConditionGraph Confirm(string message);
}

public sealed class CompareCondition : ConditionGraph   // kind "compare"
{ public string Kind; public ValueExpression Left; public string Op; public Shape Shape; public Shape ItemShape;
  internal ComparisonRightOperand RightOperand; }       // serialized via CompareConditionJsonConverter
public sealed class AllCondition  : ConditionGraph { public string Kind; public IReadOnlyList<ConditionGraph> Terms; }   // "all"
public sealed class AnyCondition  : ConditionGraph { public string Kind; public IReadOnlyList<ConditionGraph> Terms; }   // "any"
public sealed class NotCondition  : ConditionGraph { public string Kind; public ConditionGraph Term; }                  // "not"
public sealed class ConfirmCondition : ConditionGraph { public string Kind; public string Message; }                    // "confirm"

internal sealed class ComparisonOperands        // Left + ComparisonRightOperand + Shape + itemShape; factories Unary/Binary/CollectionItem
internal abstract class ComparisonRightOperand  // Absent {"kind":"none"} | Present(ValueExpression) {"kind":"value","value":…}
internal sealed class CompareOperator : PlanString  // the 21 tokens + the family arrays + RequiresRightOperand
internal sealed class MinimumTextLength             // ≥0 invariant for .MinLength
```

`branch` executable (lives in `ReactionGraph.cs`, guard owned by Condition):

```csharp
public sealed class BranchReaction : ReactionGraph { public string Kind /* "branch" */; public IReadOnlyList<BranchCase> Cases; }
public sealed class BranchCase { public BranchGuard Guard; public ReactionGraph Reaction;
    internal static BranchCase Of(ConditionGraph when, ReactionGraph reaction);   // {"guard":{"kind":"when","condition":…}}
    internal static BranchCase Default(ReactionGraph reaction); }                  // {"guard":{"kind":"default"}}
public abstract class BranchGuard { public abstract string Kind;  // "when" | "default"
    internal static BranchGuard When(ConditionGraph condition); internal static BranchGuard Else { get; } }
```

### 3c. TS runtime counterpart (the contract crossing)

Generated wire types (`plan.ts`) — Condition produces the `ConditionGraph` union, the 9 `*CompareCondition`
families, the `CompareOp` token unions, the `*ComparisonRightOperand` variants, and the `BranchGuard` union.
A dev does not hand-edit these; `PlanContractGenerator` emits them and the drift gate proves agreement.

Runtime engine + wrappers:

```ts
// conditions/compare-engine.ts  (today: sync-condition.ts — the SYNC core, the ONE CompareEngine)
export type ValueEvaluator = (expr: ValueExpression, plan: PlanDocument, ctx?: ExecContext) => unknown;

/** Evaluate one compare node to true/false. Pure, synchronous, no IO/DOM-write. The single op switch. */
export function evaluateCompare(condition: CompareCondition, plan: PlanDocument, context: ExecutionContext, evalValue: ValueEvaluator): boolean;

/** Evaluate the sync condition subset (compare/all/any/not). Confirm is NOT in this subset. */
export function evaluateSyncCondition(condition: ValidationCondition, plan: PlanDocument, context: ExecutionContext, evalValue: ValueEvaluator): boolean;

// conditions/conditions.ts  (the public entry + the async wrapper)
/** Sync entry for the validation lane (compare/all/any/not). */
export function evaluateCondition(condition: ValidationCondition, plan: PlanDocument, ctx?: ExecContext): boolean;

/** Current-lane evaluation. Crosses to async only when a reached `confirm` term requires it. */
export function evaluateConditionInCurrentLane(condition: ConditionGraph, plan: PlanDocument, ctx?: ExecContext): boolean | Promise<boolean>;
```

`evaluateConditionInCurrentLane` dispatches `compare → evaluateCompare`, `all/any/not` recurse short-circuiting
(promote to Promise only when a term returns one), and `confirm → confirmThenEvaluate` (awaits
`window.alis.confirm`, throwing only at the real boundary — missing dialog). The `branch` executable's runtime
reader (`executeBranchFrom`) belongs to **Reaction** but calls `evaluateConditionInCurrentLane` per guard, first
match wins, `default` → `true`, no-match → silent `void`.

---

## 4. File Layout (files to create)

Cohesion rule: one folder, end-to-end. Authoring builders, plan node, and runtime engine sit together under the
Condition slice; the `branch` Reaction node is referenced from Reaction (it is a `ReactionGraph`).

```
Alis.Reactive/Condition/                              ← NEW slice home (C# authoring + plan node)
├── PipelineBuilder.Conditions.cs                     ← When/Confirm entry (partial of PipelineBuilder)
├── ConditionStart.cs                                 ← standalone entry for nested And/Or (no reachable Then)
├── ConditionSourceBuilder.cs                         ← the 21 operators + source-vs-source
├── GuardBuilder.cs                                   ← And/Or/Not + Then
├── BranchBuilder.cs                                  ← ElseIf/Else (first-match cases)
├── ConditionContinuation.cs                          ← ConditionComposition (flatten algebra) + the 3 continuations
├── ConditionGraph.cs                                 ← Compare/All/Any/Not/Confirm + ComparisonOperands + ComparisonRightOperand + CompareConditionJsonConverter
└── CompareOp.cs / CompareOperator.cs / MinimumTextLength.cs   ← the single op-list source + value objects

Alis.Reactive.Assets/runtime/conditions/             ← runtime engine (existing folder; rename core)
├── compare-engine.ts                                 ← was sync-condition.ts: evaluateCompare + evaluateSyncCondition (the ONE engine)
└── conditions.ts                                     ← evaluateCondition (sync) + evaluateConditionInCurrentLane / confirmThenEvaluate (async wrapper)
```

> The `branch` executable (`BranchReaction`/`BranchCase`/`BranchGuard`) stays in `Alis.Reactive/Reaction/ReactionGraph.cs`
> and `executeBranchFrom` stays in `runtime/execution/execute.ts` — Reaction owns the executable; Condition owns the
> guard predicate it evaluates. Do not move them into Condition; that would invert the dependency direction.

---

## 5. Compile-Ready Skeleton

> Fill each `// TODO(<fixture>)` from the named acceptance fixture in §6. The structure is fixed; only the
> bodies are blank. C# bodies are 1-3 lines each (factory calls); the TS engine is a single switch with one arm
> per `CompareOp` family.

### 5a. `ConditionGraph.cs` (plan node)

```csharp
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ConditionGraph>))]
public abstract class ConditionGraph
{
    private protected ConditionGraph() { }

    internal static ConditionGraph Compare(CompareOperator op, ComparisonOperands operands) => /* TODO(cond_compare_node): new CompareCondition(op, operands) */;
    internal static ConditionGraph All(params ConditionGraph[] terms) => /* TODO(cond_and_chain): new AllCondition(terms) */;
    internal static ConditionGraph Any(params ConditionGraph[] terms) => /* TODO(cond_or_chain): new AnyCondition(terms) */;
    internal static ConditionGraph Not(ConditionGraph term) => /* TODO(cond_not): new NotCondition(term) */;
    internal static ConditionGraph Confirm(string message) => /* TODO(cond_confirm): new ConfirmCondition(message) */;
}

[JsonConverter(typeof(CompareConditionJsonConverter))]
public sealed class CompareCondition : ConditionGraph
{
    public string Kind => "compare";
    public ValueExpression Left => /* TODO: _operands.Left */;
    public string Op => /* TODO: _op.Value */;
    public Shape Shape => /* TODO: _operands.ShapeForJson */;
    public Shape ItemShape => /* TODO: _operands.ItemShapeForJson */;
    internal ComparisonRightOperand RightOperand => /* TODO: _operands.Right */;
    internal CompareCondition(CompareOperator op, ComparisonOperands operands) { /* TODO: assign fields */ }
}

// AllCondition / AnyCondition (kind "all"/"any", IReadOnlyList<ConditionGraph> Terms),
// NotCondition (kind "not", ConditionGraph Term), ConfirmCondition (kind "confirm", string Message)
// — each sealed, internal ctor, public read-only props. TODO(cond_*).

internal sealed class ComparisonOperands
{
    internal ValueExpression Left { get; }
    internal ComparisonRightOperand Right { get; }
    internal Shape ShapeForJson { get; }
    internal Shape ItemShapeForJson { get; }
    internal static ComparisonOperands Unary(ValueExpression left, Shape shape) => /* TODO(cond_unary): Right=Absent, itemShape=None */;
    internal static ComparisonOperands Binary(ValueExpression left, ValueExpression right, Shape shape) => /* TODO(cond_eq..cond_range): Right=Present, itemShape=None */;
    internal static ComparisonOperands CollectionItem(ValueExpression left, ValueExpression right, Shape collectionShape, Shape itemShape) => /* TODO(cond_array_contains): Right=Present, itemShape=element */;
}

// ComparisonRightOperand: Absent {"kind":"none"} | Present(VE) {"kind":"value","value":…}  — TODO per-arm WritePayload.
// CompareConditionJsonConverter.Write: kind, left, op, right(=RightOperand), shape, itemShape — see source order.
```

### 5b. `ConditionSourceBuilder.cs` (operators)

```csharp
public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
{
    private readonly TypedSource<TProp> _typedSource;
    private readonly Shape _shape;                                          // = source.Shape
    private readonly Func<ConditionGraph, ConditionGraph> _composeCondition; // None | All(existing) | Any(existing)
    private readonly ConditionContinuation<TModel> _continuation;

    // ctors: (source, pipeline) | (source) standalone | (source, branchBuilder) | (source, continuation, compose)

    public GuardBuilder<TModel> Eq(TProp operand)   => /* TODO(cond_eq): BuildLiteral(CompareOperator.Eq, operand) */;
    public GuardBuilder<TModel> Gt(TProp operand)   => /* TODO(cond_ordered): BuildLiteral(CompareOperator.Gt, operand) */;
    public GuardBuilder<TModel> Truthy()            => /* TODO(cond_unary): BuildUnary(CompareOperator.Truthy) */;
    public GuardBuilder<TModel> NotNull()           => /* TODO(cond_unary): BuildUnary(CompareOperator.NotNull) */;
    public GuardBuilder<TModel> In(params TProp[] v)=> /* TODO(cond_membership): BuildArray(CompareOperator.In, v) */;
    public GuardBuilder<TModel> Between(TProp lo, TProp hi) => /* TODO(cond_range): Build(Between, RangeOperands(lo,hi)) */;
    public GuardBuilder<TModel> Contains(string s) => /* TODO(cond_text): BuildTextLiteral(CompareOperator.Contains, s) */;
    public GuardBuilder<TModel> Matches(string p)  => /* TODO(cond_regex): BuildTextLiteral(CompareOperator.Matches, p) */;
    public GuardBuilder<TModel> MinLength(int n)   => /* TODO(cond_min_length): Build(MinLength, MinimumLengthOperands(n)) */;
    public GuardBuilder<TModel> ArrayContains(object item) => /* TODO(cond_array_contains): Build(ArrayContains, CollectionItemOperands(item)) */;
    public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => /* TODO(cond_source_vs_source): BuildVsSource(CompareOperator.Eq, right) */;
    // … NotEq/Gte/Lt/Lte (literal + source overloads), Falsy/IsNull/IsEmpty/NotEmpty, NotIn, StartsWith/EndsWith …

    // private helpers — all return GuardBuilder via Build(op, operands):
    //   LeftValue() => _typedSource.ToValueExpression()
    //   UnaryOperands / LiteralOperands / TextLiteralOperands / MinimumLengthOperands / ArrayOperands /
    //   SourceOperands / CollectionItemOperands / RangeOperands  → ComparisonOperands.*
    //   Build(op, operands) => ComposeAndWrap(ConditionGraph.Compare(op, operands))
    //   ComposeAndWrap(c)   => _continuation.Wrap(_composeCondition(c))
    // TODO: copy operand-builder bodies from the matrix family rows (right-operand JSON column).
}
```

### 5c. `compare-engine.ts` (runtime — the ONE engine)

```ts
export type ValueEvaluator = (expr: ValueExpression, plan: PlanDocument, ctx?: ExecContext) => unknown;

export function evaluateSyncCondition(condition: ValidationCondition, plan: PlanDocument, context: ExecutionContext, evalValue: ValueEvaluator): boolean {
  switch (condition.kind) {
    case "compare": return evaluateCompare(condition, plan, context, evalValue);
    case "all":     return condition.terms.every(t => evaluateSyncCondition(t, plan, context, evalValue));
    case "any":     return condition.terms.some(t  => evaluateSyncCondition(t, plan, context, evalValue));
    case "not":     return !evaluateSyncCondition(condition.term, plan, context, evalValue);
    default:        return assertNever(condition, "condition kind");
  }
}

export function evaluateCompare(condition: CompareCondition, plan: PlanDocument, context: ExecutionContext, evalValue: ValueEvaluator): boolean {
  const left = resolveComparisonLeft(condition, plan, context, evalValue); // { raw, shaped: applyShape(raw, condition.shape) }
  switch (condition.op) {
    case "is-null": case "not-null": case "is-empty": case "not-empty": case "truthy": case "falsy":
      return /* TODO(cond_unary): unaryMatches(condition.op, left) */;
    case "eq": case "neq":
      return /* TODO(cond_eq): equalityMatches(op, left, shaped right) */;
    case "gt": case "gte": case "lt": case "lte":
      return /* TODO(cond_ordered): orderedComparisonMatches(op, left.shaped, shaped right) */;
    case "in": case "not-in":
      return /* TODO(cond_membership): membershipMatches(op, left, shaped items) */;
    case "between":
      return /* TODO(cond_range): inclusiveRangeContains(lower, upper, left.shaped) */;
    case "array-contains":
      return /* TODO(cond_array_contains): shapeCollectionItems(left.shaped, itemShape).includes(shaped right) */;
    case "contains": case "starts-with": case "ends-with":
      return /* TODO(cond_text): evaluateTextComparison(op, left, textOperand, condition) */;
    case "matches":
      return /* TODO(cond_regex): new RegExp(pattern).test(text(left)) */;
    case "min-length":
      return /* TODO(cond_min_length): text(left).length >= n */;
    default:
      return assertNever(condition, "compare condition");
  }
}
// Helpers (resolveComparisonLeft, resolveRightValue, unaryMatches, equalityMatches, membershipMatches,
//  orderedComparisonMatches, inclusiveRangeContains, evaluateTextComparison, asText, isEmpty, isMissingValue):
//  copy 1:1 from the existing sync-condition.ts — they are the proven semantics for the matrix rows.
```

### 5d. `conditions.ts` (runtime — public entry + async wrapper)

```ts
export function evaluateCondition(condition: ValidationCondition, plan: PlanDocument, ctx?: ExecContext): boolean {
  return /* TODO: evaluateSyncCondition(condition, plan, ExecutionContext.from(ctx), evaluateValue) */;
}

export function evaluateConditionInCurrentLane(condition: ConditionGraph, plan: PlanDocument, ctx?: ExecContext): boolean | Promise<boolean> {
  switch (condition.kind) {
    case "compare": return evaluateCompare(condition, plan, ExecutionContext.from(ctx), evaluateValue);
    case "all":     return /* TODO(cond_and_chain): short-circuit all, promote to Promise only if a term is a Promise */;
    case "any":     return /* TODO(cond_or_chain): short-circuit any, same promotion */;
    case "not":     return /* TODO(cond_not): negate, propagating Promise */;
    case "confirm": return /* TODO(cond_confirm): confirmThenEvaluate(condition.message) */;
    default:        return assertNever(condition, "condition kind");
  }
}
// confirmThenEvaluate: const fn = window.alis?.confirm; if (!fn) throw boundary error; return await fn(message).
```

---

## 6. Acceptance Fixtures (matrix cases this module must satisfy)

From [`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md) — Condition band.
Each name is the fixture a generated case must pass (one DSL input → one plan JSON → one browser behavior).

### Condition source — left operand (5)
- `cond_source_component` — `When(p.Component<X>(m=>m.Care).Value(c=>c.Level))` → `left = read(component,id,member)`, shape from `TProp`.
- `cond_source_url` — `When(p.FromUrl<int>("page")).Gt(1)` → `left = read(url,"page")`, shape Number.
- `cond_source_plugin` — `When(p.PluginProperty<bool>("net","online"))` → `left = read(plugin,…)`.
- `cond_source_event_payload` — `When(args, x=>x.Total)` → `left = read(payload, scope "event", path)`.
- `cond_source_response_body` — `When(success, x=>x.Status)` → `left = read(payload, scope "success", path)`.

### Compare families — the 9 operand-shape families / 21 tokens (10 incl. source-vs-source)
- `cond_unary` — `.Truthy()/.Falsy()/.IsNull()/.NotNull()/.IsEmpty()/.NotEmpty()` → `right {"kind":"none"}`; raw vs shaped semantics.
- `cond_eq` — `.Eq(x)/.NotEq(x)` (equality) → `shaped(left)===shaped(right)`.
- `cond_ordered` — `.Gt/.Gte/.Lt/.Lte` → numeric/string/bool ordered; mismatched types ⇒ `false` (no throw).
- `cond_membership` — `.In(...)/.NotIn(...)` → `{value:{array}}`; `array.includes(shaped(left))`.
- `cond_range` — `.Between(lo,hi)` → `{value:{array[2]}}`; inclusive `lo ≤ left ≤ hi`; un-orderable ⇒ `false`.
- `cond_text` — `.Contains/.StartsWith/.EndsWith` → `{value:{textLiteral}}`; non-text left ⇒ `false`.
- `cond_regex` — `.Matches(pattern)` → `new RegExp(pattern).test(text(left))`; non-text ⇒ `false`.
- `cond_min_length` — `.MinLength(n)` → `text(left).length >= n`; non-text ⇒ `false`; `n ≥ 0` invariant.
- `cond_array_contains` — `.ArrayContains(item)` → `{value:{literal}}` + `itemShape` set; shape each item, `items.includes(item)`.
- `cond_source_vs_source` — `.Eq(otherSource)/.Gt(otherSource)…` → right operand `value` is a `read` instead of a `literal` (same families).

### Guard composition (6)
- `cond_single` — `When(s).Eq(1)` → bare `compare`, no wrapper.
- `cond_and_chain` — `…Eq(1).And(s2).Gt(0)` → flattened `all`, short-circuit `false`.
- `cond_or_chain` — `…Eq(1).Or(s2).Eq(2)` → flattened `any`, short-circuit `true`.
- `cond_and_group` — `.And(inner => inner.When(s).Gt(0))` → nested group flattened into one `all`.
- `cond_or_group` — `.Or(inner => …)` → flattened `any`.
- `cond_not` — `…Eq(1).Not()` → `{"kind":"not","term":{compare}}`, inverts.

### Branch routing — first-match `branch` ReactionGraph (4)
- `cond_then` — `When(s).Eq(1).Then(p2=>…)` → `branch` with one `when` case; runs only if guard true. SYNC.
- `cond_else_if` — `.ElseIf(s).Gt(0).Then(p3=>…)` → another `when` case appended; first match wins, rest skipped. SYNC.
- `cond_else` — `.Else(p4=>…)` → trailing `default` case; only if no prior match; only one allowed (post-`Else` add throws at author time). SYNC.
- `cond_no_match` — guards all false, no `Else` → runtime no-op, logs `branch.no-match`, returns `void`. SYNC.

### Confirm — the one async opener (2)
- `cond_confirm_then` — `Confirm("Delete?").Then(p2=>…)` → guard `{"kind":"confirm","message":"Delete?"}`; awaits dialog, runs only on accept; missing dialog throws at boundary. ASYNC.
- `cond_confirm_in_composition` — `Confirm("Sure?")` then `.And(s).Gt(0)` → `all([confirm, compare])`; compares may short-circuit before the dialog. ASYNC if confirm reached.

### Determinism wins to prove (3 cross-cutting fixtures)
- `cond_standalone_then_unrepresentable` — the `standalone` continuation exposes no `Then` (compile-time absence, not the runtime throw at `ConditionContinuation.cs:138`).
- `cond_single_compare_engine` — `evaluateCondition` (sync) and `evaluateConditionInCurrentLane` (async) both route `compare` through the one `evaluateCompare`; no second evaluator.
- `cond_branch_lane_carried` — a branch of pure compares is stamped SYNC; a branch reaching `confirm` is stamped ASYNC; the runtime routes on the carried `ReactionLane`, not `instanceof Promise`.

> Coverage note: the matrix counts the Condition band as 5 source + (9 families + source-vs-source) + 6 composition
> + 4 routing + 2 confirm. Every fixture above maps to one of those rows by name; the 3 determinism-win fixtures
> are the design properties §1 lists and must be asserted (a compile error for `Standalone.Then`, a single-engine
> test, a lane-stamp test) before this slice is declared done.
