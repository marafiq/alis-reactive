# conditions

DSL grammar (AST edges) for the Conditions cluster, extracted from real public builder
signatures. Every row is a real public method with a `file:line`. Receiver/Returns are the
declared builder types. `Callback` is the callback param type if any (a callback handing back
a `PipelineBuilder` is a NESTING / recursion point). `ReturnsSelf=yes` means the member returns
its own receiver type and can be CHAINED/REPEATED.

Source files (all under `Alis.Reactive/Builders/Conditions/`):
`ConditionStart.cs`, `ConditionSourceBuilder.cs`, `GuardBuilder.cs`, `BranchBuilder.cs`.
`ConditionContinuation.cs` holds only `internal` continuation types (no public members) and is
excluded. The `ConditionSourceBuilder` constructors are `internal` (entered via `When`/`And`/
`Or`/`ElseIf`), so only its public operator members are rows.

Type params on every builder: `TModel : class`. `TProp` = the source value type (compile-time
operand type). `TPayload` = the event/response body carrier type.

## ConditionStart\<TModel>

Entry point for standalone condition expressions (used inside nested `And`/`Or` callbacks).
Constructor is `internal`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ConditionStart\<TModel> | When\<TPayload,TProp>(TPayload payload, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | ConditionStart.cs:16 |
| ConditionStart\<TModel> | When\<TPayload,TProp>(ResponseBody\<TPayload> responseBody, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | ConditionStart.cs:25 |
| ConditionStart\<TModel> | When\<TProp>(TypedSource\<TProp> source) | ConditionSourceBuilder\<TModel,TProp> | - | no | ConditionStart.cs:35 |
| ConditionStart\<TModel> | Confirm(string message) | GuardBuilder\<TModel> | - | no | ConditionStart.cs:42 |

## ConditionSourceBuilder\<TModel, TProp>

Typed comparison operators for a value source. Every operator returns a `GuardBuilder<TModel>`
(terminal-to-guard transition). All constructors are `internal`. No member returns its own type.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ConditionSourceBuilder\<TModel,TProp> | Eq(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:49 |
| ConditionSourceBuilder\<TModel,TProp> | NotEq(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:51 |
| ConditionSourceBuilder\<TModel,TProp> | Gt(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:53 |
| ConditionSourceBuilder\<TModel,TProp> | Gte(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:55 |
| ConditionSourceBuilder\<TModel,TProp> | Lt(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:57 |
| ConditionSourceBuilder\<TModel,TProp> | Lte(TProp operand) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:59 |
| ConditionSourceBuilder\<TModel,TProp> | Truthy() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:63 |
| ConditionSourceBuilder\<TModel,TProp> | Falsy() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:65 |
| ConditionSourceBuilder\<TModel,TProp> | IsNull() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:67 |
| ConditionSourceBuilder\<TModel,TProp> | NotNull() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:69 |
| ConditionSourceBuilder\<TModel,TProp> | IsEmpty() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:71 |
| ConditionSourceBuilder\<TModel,TProp> | NotEmpty() | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:73 |
| ConditionSourceBuilder\<TModel,TProp> | In(params TProp[] values) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:77 |
| ConditionSourceBuilder\<TModel,TProp> | NotIn(params TProp[] values) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:79 |
| ConditionSourceBuilder\<TModel,TProp> | Between(TProp low, TProp high) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:83 |
| ConditionSourceBuilder\<TModel,TProp> | Contains(string substring) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:88 |
| ConditionSourceBuilder\<TModel,TProp> | StartsWith(string prefix) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:91 |
| ConditionSourceBuilder\<TModel,TProp> | EndsWith(string suffix) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:94 |
| ConditionSourceBuilder\<TModel,TProp> | Matches(string pattern) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:97 |
| ConditionSourceBuilder\<TModel,TProp> | MinLength(int length) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:100 |
| ConditionSourceBuilder\<TModel,TProp> | ArrayContains(object item) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:105 |
| ConditionSourceBuilder\<TModel,TProp> | Eq(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:112 |
| ConditionSourceBuilder\<TModel,TProp> | NotEq(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:114 |
| ConditionSourceBuilder\<TModel,TProp> | Gt(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:116 |
| ConditionSourceBuilder\<TModel,TProp> | Gte(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:118 |
| ConditionSourceBuilder\<TModel,TProp> | Lt(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:120 |
| ConditionSourceBuilder\<TModel,TProp> | Lte(TypedSource\<TProp> right) | GuardBuilder\<TModel> | - | no | ConditionSourceBuilder.cs:122 |

## GuardBuilder\<TModel>

Composes conditions with `And`/`Or`/`Not` and opens branches with `Then`. All constructors are
`internal`. `And`/`Or`/`Not` are `ReturnsSelf=yes` (chainable/repeatable composition).
`And(Func)`/`Or(Func)` take a nested-condition callback (`ConditionStart` -> `GuardBuilder`):
a recursion point that builds a sub-condition, NOT a `PipelineBuilder` nesting.
`Then(Action<PipelineBuilder>)` is the NESTING point that opens a reaction pipeline.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| GuardBuilder\<TModel> | And\<TPayload,TProp>(TPayload payload, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:43 |
| GuardBuilder\<TModel> | And\<TPayload,TProp>(ResponseBody\<TPayload> responseBody, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:52 |
| GuardBuilder\<TModel> | Or\<TPayload,TProp>(TPayload payload, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:62 |
| GuardBuilder\<TModel> | Or\<TPayload,TProp>(ResponseBody\<TPayload> responseBody, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:71 |
| GuardBuilder\<TModel> | And\<TProp>(TypedSource\<TProp> source) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:81 |
| GuardBuilder\<TModel> | Or\<TProp>(TypedSource\<TProp> source) | ConditionSourceBuilder\<TModel,TProp> | - | no | GuardBuilder.cs:88 |
| GuardBuilder\<TModel> | And(Func\<ConditionStart\<TModel>,GuardBuilder\<TModel>> inner) | GuardBuilder\<TModel> | Func\<ConditionStart\<TModel>,GuardBuilder\<TModel>> | yes | GuardBuilder.cs:95 |
| GuardBuilder\<TModel> | Or(Func\<ConditionStart\<TModel>,GuardBuilder\<TModel>> inner) | GuardBuilder\<TModel> | Func\<ConditionStart\<TModel>,GuardBuilder\<TModel>> | yes | GuardBuilder.cs:106 |
| GuardBuilder\<TModel> | Not() | GuardBuilder\<TModel> | - | yes | GuardBuilder.cs:118 |
| GuardBuilder\<TModel> | Then(Action\<PipelineBuilder\<TModel>> pipeline) | BranchBuilder\<TModel> | Action\<PipelineBuilder\<TModel>> (NESTING) | no | GuardBuilder.cs:126 |

## BranchBuilder\<TModel>

Chains `ElseIf` (re-enters `ConditionSourceBuilder`) and `Else` (default case) after a `Then`.
`ElseIf` is repeatable (first-match routing) but returns `ConditionSourceBuilder`, not its own
type, so `ReturnsSelf=no`. `Else(Action<PipelineBuilder>)` is a NESTING point (`void` terminal).
All constructors are `internal`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| BranchBuilder\<TModel> | ElseIf\<TPayload,TProp>(TPayload payload, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | BranchBuilder.cs:29 |
| BranchBuilder\<TModel> | ElseIf\<TPayload,TProp>(ResponseBody\<TPayload> responseBody, Expression\<Func\<TPayload,TProp>> path) | ConditionSourceBuilder\<TModel,TProp> | - | no | BranchBuilder.cs:40 |
| BranchBuilder\<TModel> | ElseIf\<TProp>(TypedSource\<TProp> source) | ConditionSourceBuilder\<TModel,TProp> | - | no | BranchBuilder.cs:52 |
| BranchBuilder\<TModel> | Else(Action\<PipelineBuilder\<TModel>> pipeline) | void | Action\<PipelineBuilder\<TModel>> (NESTING) | no | BranchBuilder.cs:61 |

## Nesting & recursion summary

- `GuardBuilder.Then(Action<PipelineBuilder<TModel>>)` — NESTING into a reaction pipeline
  (`GuardBuilder.cs:126`). The `Action<PipelineBuilder>` callback is the recursion point back
  into the full pipeline grammar.
- `BranchBuilder.Else(Action<PipelineBuilder<TModel>>)` — NESTING into a reaction pipeline
  (`BranchBuilder.cs:61`), `void` terminal of a branch chain.
- `GuardBuilder.And(Func<ConditionStart,GuardBuilder>)` / `Or(...)` — recursion into a NESTED
  CONDITION expression (`GuardBuilder.cs:95`, `:106`), not a pipeline. Both `ReturnsSelf=yes`.
- `GuardBuilder.And`/`Or`/`Not` returning `GuardBuilder<TModel>` — chainable/repeatable
  condition composition.
- `BranchBuilder.ElseIf(...)` — repeatable branch case (first-match), re-enters
  `ConditionSourceBuilder` to attach an operator before the next `Then`.

Proven param type: `Then` and `Else` accept `Action<PipelineBuilder<TModel>>`
(`GuardBuilder.cs:126`, `BranchBuilder.cs:61`) — confirmed from source, not inferred.
