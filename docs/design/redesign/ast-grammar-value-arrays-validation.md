# value-arrays-validation

AST grammar (DSL edges) for the Value/Arrays + Validation cluster, derived from REAL
public builder signatures. Every row is a public method/property in source with a `file:line`.
Paths are relative to `Alis.Reactive/` (the C# project root).

Legend:
- **Callback** — the callback param type if any (`Action<...>`, `Func<...>`, `Expression<Func<...>>`); `-` when none.
- **ReturnsSelf** — `yes` if the member returns its own receiver type (chainable/repeatable).
- A `Callback` that hands back a builder is a **NESTING (recursion) point**.
- `ReturnsSelf = yes` means the member can be **CHAINED/REPEATED**.

> Cluster-name reconciliation (source is authority): the cluster brief names
> `ReactiveArray` ops `OrderBy/Count/Sum/Min/Max/Average/FindFirst/AsArraySource` and
> Validation `ReactiveValidator<T>`, `ClientRule(+Each/From/nested)`, `WhenField/WhenFields`,
> `FieldGuard And/Or/Not`. In actual source the array ops are
> `Where/Select/OrderBy/OrderByDescending/Count/Any/All/Sum/Find` and `AsSource`
> (no `Min/Max/Average/FindFirst/AsArraySource`). The validation authoring grammar is the
> `ClientValidation*` builder family (no public type named `ReactiveValidator`, `WhenField`,
> `FieldGuard`, or `Each`). `ClientRule` exists but is `internal` (not a DSL edge). The
> conceptual roles map as: `ReactiveValidator<T>` -> `ClientValidationRulesBuilder<TModel>` +
> `ReactiveClientValidationBuilder`; `WhenField/WhenFields` -> `.When(...)` over
> `ClientValidationConditionBuilder<TModel>`; `FieldGuard And/Or/Not` ->
> `ClientValidationCondition<TModel>.And/Or/Not`. Only real public members are recorded below.

---

## ReactiveArray&lt;TElement&gt;

Source: `Builders/Arrays/ReactiveArray.cs`. Deferred typed array transform; operators capture
intent and compile to plan-JSON `array-op` nodes. Chains compose. `Expression<Func<...>>`
lambdas are captured (never invoked). `Where/Select/OrderBy/OrderByDescending` continue the
array chain; the terminal aggregates return a `ReactiveValue<T>` scalar.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ReactiveArray&lt;TElement&gt; | Where(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate) | ReactiveArray&lt;TElement&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; | yes | Builders/Arrays/ReactiveArray.cs:28 |
| ReactiveArray&lt;TElement&gt; | Select&lt;TResult&gt;(Expression&lt;Func&lt;TElement,TResult&gt;&gt; selector) | ReactiveArray&lt;TResult&gt; | Expression&lt;Func&lt;TElement,TResult&gt;&gt; | no (re-types element) | Builders/Arrays/ReactiveArray.cs:34 |
| ReactiveArray&lt;TElement&gt; | OrderBy&lt;TKey&gt;(Expression&lt;Func&lt;TElement,TKey&gt;&gt; key) | ReactiveArray&lt;TElement&gt; | Expression&lt;Func&lt;TElement,TKey&gt;&gt; | yes | Builders/Arrays/ReactiveArray.cs:43 |
| ReactiveArray&lt;TElement&gt; | OrderByDescending&lt;TKey&gt;(Expression&lt;Func&lt;TElement,TKey&gt;&gt; key) | ReactiveArray&lt;TElement&gt; | Expression&lt;Func&lt;TElement,TKey&gt;&gt; | yes | Builders/Arrays/ReactiveArray.cs:47 |
| ReactiveArray&lt;TElement&gt; | Count() | ReactiveValue&lt;int&gt; | - | no (terminal) | Builders/Arrays/ReactiveArray.cs:70 |
| ReactiveArray&lt;TElement&gt; | Count(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate) | ReactiveValue&lt;int&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:74 |
| ReactiveArray&lt;TElement&gt; | Any() | ReactiveValue&lt;bool&gt; | - | no (terminal) | Builders/Arrays/ReactiveArray.cs:78 |
| ReactiveArray&lt;TElement&gt; | Any(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate) | ReactiveValue&lt;bool&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:82 |
| ReactiveArray&lt;TElement&gt; | All(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate) | ReactiveValue&lt;bool&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:86 |
| ReactiveArray&lt;TElement&gt; | Sum(Expression&lt;Func&lt;TElement,int&gt;&gt; selector) | ReactiveValue&lt;int&gt; | Expression&lt;Func&lt;TElement,int&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:90 |
| ReactiveArray&lt;TElement&gt; | Sum(Expression&lt;Func&lt;TElement,decimal&gt;&gt; selector) | ReactiveValue&lt;decimal&gt; | Expression&lt;Func&lt;TElement,decimal&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:94 |
| ReactiveArray&lt;TElement&gt; | Sum(Expression&lt;Func&lt;TElement,double&gt;&gt; selector) | ReactiveValue&lt;double&gt; | Expression&lt;Func&lt;TElement,double&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:98 |
| ReactiveArray&lt;TElement&gt; | Find(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate) | ReactiveValue&lt;TElement&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:102 |
| ReactiveArray&lt;TElement&gt; | Find&lt;TField&gt;(Expression&lt;Func&lt;TElement,bool&gt;&gt; predicate, Expression&lt;Func&lt;TElement,TField&gt;&gt; selector) | ReactiveValue&lt;TField&gt; | Expression&lt;Func&lt;TElement,bool&gt;&gt; + Expression&lt;Func&lt;TElement,TField&gt;&gt; | no (terminal) | Builders/Arrays/ReactiveArray.cs:107 |
| ReactiveArray&lt;TElement&gt; | AsSource() | TypedSource&lt;TElement[]&gt; | - | no (exits to source) | Builders/Arrays/ReactiveArray.cs:121 |

## PipelineBuilder&lt;TModel&gt; (array entry points)

Source: `Builders/PipelineBuilder.Arrays.cs` (`partial`). These are the only public entry
points that construct a `ReactiveArray<TElement>` to begin a transform chain.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| PipelineBuilder&lt;TModel&gt; | From&lt;TElement&gt;(TypedSource&lt;TElement[]&gt; source) | ReactiveArray&lt;TElement&gt; | - | no (enters array chain) | Builders/PipelineBuilder.Arrays.cs:15 |
| PipelineBuilder&lt;TModel&gt; | From&lt;TArgs,TElement&gt;(TArgs args, Expression&lt;Func&lt;TArgs,TElement[]&gt;&gt; selector) | ReactiveArray&lt;TElement&gt; | Expression&lt;Func&lt;TArgs,TElement[]&gt;&gt; | no (enters array chain) | Builders/PipelineBuilder.Arrays.cs:23 |
| PipelineBuilder&lt;TModel&gt; | FromDom(string elementId, string member) | ReactiveArray&lt;string&gt; | - | no (enters array chain) | Builders/PipelineBuilder.Arrays.cs:37 |
| PipelineBuilder&lt;TModel&gt; | FromDom&lt;TElement&gt;(string elementId, string member) | ReactiveArray&lt;TElement&gt; | - | no (enters array chain) | Builders/PipelineBuilder.Arrays.cs:41 |

## ReactiveValue&lt;TValue&gt;

Source: `Builders/Arrays/ReactiveValue.cs`. A scalar value produced by an array aggregate
(`Count`/`Sum`/`Any`/`All`/`Find`). It is a `TypedSource<TValue>`, so it plugs into any place
that accepts the base `TypedSource<T>` (e.g. `SetText`, `When`, dispatch payloads) with no new
overloads. No public members of its own; its public role is being a `TypedSource<TValue>`.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ReactiveValue&lt;TValue&gt; | _(no public members — inherits TypedSource&lt;TValue&gt;; ctor internal)_ | — | - | - | Builders/Arrays/ReactiveValue.cs:13 |

## TypedSource&lt;TProp&gt;

Source: `Builders/Conditions/TypedSource.cs`. The base typed-source abstraction that carries
the property type through the condition/mutation pipeline. All members are `internal`
(`ToValueExpression`, `Shape`, `ElementShape`) — it exposes **no public DSL edges**; it is the
shared parameter/return type that `ReactiveArray.AsSource()` and `ReactiveValue<T>` flow into.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| TypedSource&lt;TProp&gt; | _(no public members — internal ToValueExpression/Shape/ElementShape)_ | — | - | - | Builders/Conditions/TypedSource.cs:9 |

---

## ReactiveClientValidationServiceCollectionExtensions

Source: `Validation/ReactiveClientValidationServiceCollectionExtensions.cs`. DI entry point that
registers app-level browser-validation metadata. The `configure` callback hands back a
`ReactiveClientValidationBuilder` — a **NESTING point** into the validation grammar.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| IServiceCollection (ext) | AddReactiveClientValidation(this IServiceCollection services, Action&lt;ReactiveClientValidationBuilder&gt; configure) | IServiceCollection | Action&lt;ReactiveClientValidationBuilder&gt; | yes (IServiceCollection) | Validation/ReactiveClientValidationServiceCollectionExtensions.cs:13 |

## ReactiveClientValidationBuilder

Source: `Validation/ReactiveClientValidationServiceCollectionExtensions.cs`. Registers rules per
validation-source type. The `define` callback hands back a `ClientValidationRulesBuilder<TModel>`
— a **NESTING point**. `Add` returns itself, so multiple sources can be **CHAINED/REPEATED**.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ReactiveClientValidationBuilder | Add&lt;TValidationSource,TModel&gt;(Action&lt;ClientValidationRulesBuilder&lt;TModel&gt;&gt; define) | ReactiveClientValidationBuilder | Action&lt;ClientValidationRulesBuilder&lt;TModel&gt;&gt; | yes | Validation/ReactiveClientValidationServiceCollectionExtensions.cs:39 |

## ClientValidationRulesBuilder&lt;TModel&gt;

Source: `Validation/ClientValidationRulesBuilder.cs`. Builds one model's deterministic browser
rules. `Field` opens a per-field rule builder (**NESTING via return**); `When` takes a condition
factory plus a `define` callback that hands back **this same builder** — a recursive
**NESTING point** that scopes nested rules under a `FieldCondition` activation.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationRulesBuilder&lt;TModel&gt; | Field&lt;TValue&gt;(Expression&lt;Func&lt;TModel,TValue&gt;&gt; field) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | no (opens field rule builder) | Validation/ClientValidationRulesBuilder.cs:18 |
| ClientValidationRulesBuilder&lt;TModel&gt; | Field&lt;TValue&gt;(ClientValidationFieldToken&lt;TModel,TValue&gt; field) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | no (opens field rule builder) | Validation/ClientValidationRulesBuilder.cs:22 |
| ClientValidationRulesBuilder&lt;TModel&gt; | When(Func&lt;ClientValidationConditionBuilder&lt;TModel&gt;,ClientValidationCondition&lt;TModel&gt;&gt; condition, Action&lt;ClientValidationRulesBuilder&lt;TModel&gt;&gt; define) | void | Func&lt;ClientValidationConditionBuilder&lt;TModel&gt;,ClientValidationCondition&lt;TModel&gt;&gt; + Action&lt;ClientValidationRulesBuilder&lt;TModel&gt;&gt; | no (recurses via define) | Validation/ClientValidationRulesBuilder.cs:34 |

## ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt;

Source: `Validation/ClientValidationFieldRuleBuilder.cs`. Writes rule verbs for one field. Every
verb returns **this builder**, so verbs **CHAIN/REPEAT** (multiple rules per field). `message`
is the browser error text. Peer-field overloads compare against another model field
(`Expression` or pre-built token).

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Required(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:27 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Empty(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:30 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Email(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:33 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Url(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:36 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | CreditCard(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:39 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | AtLeastOne(string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:42 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | MinLength(int length, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:45 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | MaxLength(int length, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:51 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Regex(string pattern, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:57 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Range(TValue lowerBound, TValue upperBound, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:65 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | ExclusiveRange(TValue lowerBound, TValue upperBound, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:71 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Min(TValue minimum, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:77 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Max(TValue maximum, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:80 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThanOrEqualTo(TValue minimum, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:83 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThanOrEqualTo(TValue maximum, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:86 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThan(TValue value, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:89 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThan(TValue value, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:92 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | EqualTo(TValue expected, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:95 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | EqualTo(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:98 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | EqualTo(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:103 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | NotEqual(TValue forbidden, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:108 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | NotEqualTo(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:111 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | NotEqualTo(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:116 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThan(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:121 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThan(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:126 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThanOrEqualTo(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:131 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | GreaterThanOrEqualTo(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:136 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThan(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:141 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThan(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:146 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThanOrEqualTo(Expression&lt;Func&lt;TModel,TValue&gt;&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | yes | Validation/ClientValidationFieldRuleBuilder.cs:151 |
| ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | LessThanOrEqualTo(ClientValidationFieldToken&lt;TModel,TValue&gt; peerField, string message) | ClientValidationFieldRuleBuilder&lt;TModel,TValue&gt; | - | yes | Validation/ClientValidationFieldRuleBuilder.cs:156 |

## ClientValidationConditionBuilder&lt;TModel&gt;

Source: `Validation/ClientValidationConditionBuilder.cs`. The condition factory handed to
`When`. `Field` opens a typed condition start (**NESTING via return**) over a model field.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationConditionBuilder&lt;TModel&gt; | Field&lt;TValue&gt;(Expression&lt;Func&lt;TModel,TValue&gt;&gt; field) | ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | no (opens condition start) | Validation/ClientValidationConditionBuilder.cs:17 |
| ClientValidationConditionBuilder&lt;TModel&gt; | Field&lt;TValue&gt;(ClientValidationFieldToken&lt;TModel,TValue&gt; field) | ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | - | no (opens condition start) | Validation/ClientValidationConditionBuilder.cs:21 |

## ClientValidationFieldConditionStart&lt;TModel,TValue&gt;

Source: `Validation/ClientValidationConditionBuilder.cs`. Comparison verbs over a typed field;
each produces a completed `ClientValidationCondition<TModel>` (exits to the composable
condition). These are the `WhenField` predicate verbs.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Truthy() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:42 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Falsy() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:45 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Eq(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:48 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Neq(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:51 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Gt(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:54 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Gte(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:57 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Lt(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:60 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Lte(TValue value) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:63 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | IsNull() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:66 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | NotNull() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:69 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | IsEmpty() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:72 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | NotEmpty() | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:75 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | In(params TValue[] values) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:78 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | NotIn(params TValue[] values) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:84 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Between(TValue lowerBound, TValue upperBound) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:90 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Contains(string substring) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:93 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | StartsWith(string prefix) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:96 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | EndsWith(string suffix) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:99 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | Matches(string pattern) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:102 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | MinLength(int minLength) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:110 |
| ClientValidationFieldConditionStart&lt;TModel,TValue&gt; | ArrayContains&lt;TItem&gt;(TItem item) | ClientValidationCondition&lt;TModel&gt; | - | no (exits to condition) | Validation/ClientValidationConditionBuilder.cs:119 |

## ClientValidationCondition&lt;TModel&gt;

Source: `Validation/ClientValidationConditionBuilder.cs`. The composable completed condition —
the `FieldGuard And/Or/Not` of the cluster. `And`/`Or` take another condition and return a new
composed condition (chainable); `Not` negates. Each returns a `ClientValidationCondition<TModel>`
so guards **CHAIN/REPEAT**.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationCondition&lt;TModel&gt; | And(ClientValidationCondition&lt;TModel&gt; other) | ClientValidationCondition&lt;TModel&gt; | - | yes | Validation/ClientValidationConditionBuilder.cs:191 |
| ClientValidationCondition&lt;TModel&gt; | Or(ClientValidationCondition&lt;TModel&gt; other) | ClientValidationCondition&lt;TModel&gt; | - | yes | Validation/ClientValidationConditionBuilder.cs:199 |
| ClientValidationCondition&lt;TModel&gt; | Not() | ClientValidationCondition&lt;TModel&gt; | - | yes | Validation/ClientValidationConditionBuilder.cs:207 |

## ClientValidationFieldToken&lt;TModel,TValue&gt;

Source: `Validation/ClientValidationFieldToken.cs`. Opaque typed reference to a model field;
`For` is the public factory used by the `Field`/peer-field overloads above to pre-build a token
from a model expression.

| Receiver | Member(params) | Returns | Callback | ReturnsSelf | Source |
|----------|----------------|---------|----------|-------------|--------|
| ClientValidationFieldToken&lt;TModel,TValue&gt; (static) | For(Expression&lt;Func&lt;TModel,TValue&gt;&gt; field) | ClientValidationFieldToken&lt;TModel,TValue&gt; | Expression&lt;Func&lt;TModel,TValue&gt;&gt; | no (factory) | Validation/ClientValidationFieldToken.cs:20 |

---

## Non-edges (verified, excluded with reason)

These types are in the cluster's source folders but expose **no public DSL authoring edge**:

- `ReactiveArraySource<TElement>` — `internal sealed`; constructor + override only. (`Builders/Arrays/ReactiveArray.cs:137`)
- `ElementExpressionCompiler` — `internal static` predicate/projection compiler, not authoring API. (`Builders/Arrays/ElementExpressionCompiler.cs:15`)
- `ClientRule` — `internal sealed` plan-model record (the cluster's "ClientRule"); `public Message`/`public Shape` are read-side accessors, not DSL edges. (`Validation/ClientRule.cs:11`)
- `RuleName` — `internal sealed`; `public` members are `IEquatable`/object overrides only. (`Validation/RuleName.cs:10`)
- `RuleOperand`, `FieldCondition`, `ClientValidationRuleSet`, `ClientValidationFieldBinding`,
  `ClientValidationLiteral`, `ValidationTerms` (`ValidationFieldPath`/`ValidationMessage`),
  `ClientValidationFieldReference` — internal plan-model/serialization types; only object
  overrides are `public`. Nested/array field expansion (the cluster's "nested") happens inside
  `ClientValidationFieldBinding` (`Validation/ClientValidationFieldBinding.cs:91`), not via a
  public builder method.
- `ClientValidationField` — `public sealed`; `public string FieldName` is a read accessor on
  the serialized output, not an authoring edge. (`Validation/ClientValidationField.cs:13`)
- `ClientValidationRuleSource` — `public` runtime metadata lookup service
  (`GetClientRules`/`TryGetClientRules`), not part of the authoring grammar. (`Validation/ClientValidationRuleSource.cs`)
- `ReactiveValidator<T>`, `WhenField`/`WhenFields`, `FieldGuard`, `.Each(...)` — **no such public
  type/method exists in source.** Their roles are filled by the rows above (see header note).
