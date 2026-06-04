# Validation — Implementation Spec

> One of the 12 redesign micro-modules. Read alongside
> [`00-design.md`](../00-design.md), [`02-micro-modules.md`](../02-micro-modules.md),
> [`03-naming.md`](../03-naming.md), and the acceptance matrix
> [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md), **Band A**.
>
> **Goal of this file.** A developer opens it, sees the exact surface + fixtures,
> and types the obvious body. Every type and signature below is grounded in actual
> source (cited inline). Where a name changes, the old name is named so the move is
> mechanical, not a redesign.

---

## 1. Responsibility

**Validation records explicit, deterministic client-side rules for a model field at
render time, lowers them to a plan node graph keyed by the deterministic component
id, and runs them inline/summary in the browser — while FluentValidation stays the
server authority.**

### What it owns

`→` **Authoring (C#)**

- `ReactiveValidator<T>` — the `AbstractValidator<T>` subclass where a developer
  records `ClientRule(...)`, `ClientRuleEach(...)`, the `WhenField*` family, and
  `WhenFields(...)`. *(kept;
  `Alis.Reactive.FluentValidator/ReactiveValidator.cs`)*
- `ClientValidationFieldRuleBuilder<TModel,TValue>` — the per-field fluent surface
  for the 18 rule methods + peer overloads. *(kept;
  `Alis.Reactive/Validation/ClientValidationFieldRuleBuilder.cs`)*
- `ClientRule` (the authoring rule the developer records) vs **`ValidationRuleNode`**
  (the plan node the plan carries) — the renamed collision. Today **two** types are
  named `ValidationRule`: `Validation/ValidationRule.cs` (authoring) and the nested
  `PlanModel.ValidationRule` (plan node). After the rename: authoring is `ClientRule`,
  plan node is `ValidationRuleNode`. **No two types share a name.**
- `RuleName` — the ONE rule-name source (18 tokens), `internal` value object
  projected into the TS `ValidationRuleName` union by **Kind**'s
  `PlanContractGenerator`. *(renamed from `ValidationRuleName`;
  `Alis.Reactive/Validation/ValidationTerms.cs:86`)*
- `RuleOperand` — the ONE operand model with exactly three execution variants:
  `none` / `constraint` (literal) / `peer` (reads another field). *(renamed from
  `ValidationRuleOperand`; `Alis.Reactive/Validation/ValidationRule.cs:71`)*
- `CollectionItemBinding` — a real value object that binds a rule to one rendered
  item of a collection field (replaces today's substring/bracket path arithmetic).
- `ValidationGraph` — validation's **own home** for the plan-model node family
  (`ValidationRuleNode`, `ValidationRuleExecution`, `ValidationRuleActivation`,
  `ComponentValidation`, `ContainerScope`), **extracted** from the
  `ComponentObject.cs` god-file where it lives today
  (`PlanModel/ComponentObject.cs:383-677`) and flattened.
- `ClientValidationRuleBinder` — the render-time binder that, at `RenderPlan`,
  attaches the resolved rules to the container `BrowserObject` keyed by the
  `IdGenerator` id. *(kept; `Alis.Reactive/ClientValidationRuleBinder.cs`)*

`⇒` **Runtime (TS)**

- `validationOrchestrator` (`validation/orchestrator.ts`) — reads each component
  value through the shared `evaluateValue`, runs `ruleEngine`, reports via
  `errorDisplay`, and — for `WhenField` activation — **reuses Condition's
  `evaluateCondition`/`CompareEngine`**.
- `ruleEngine` (`validation/rule-engine.ts`) — pure rule evaluation, no DOM, no
  vendor, no side effects.
- `errorDisplay` (`validation/error-display.ts`) — the only DOM error writer
  (inline span + summary div).
- `liveClear` (`validation/live-clear.ts`) — per-field blur/change/input wiring.
- `ErrorElementNaming` — the ONE constant for `{id}_error` and
  `{planId}_validation_summary`, shared across `errorDisplay` and `orchestrator`
  (replaces today's ad-hoc string building scattered in both files).

### What it depends on (from [`02-micro-modules.md`](../02-micro-modules.md))

| Depends on | For |
|---|---|
| **Condition** | `WhenField`/`WhenFields` activation lowers to a `ConditionGraph`; the runtime reuses `evaluateCondition`/`CompareEngine` — **no second evaluator**. |
| **Component** | The deterministic `IdGenerator` id is the join key; rules attach to the container `BrowserObject` (`ComponentRole.ValidationContainer`); `RuntimeComponents`/`RuntimeObject` resolve fields. |
| **Value** | A field value and a peer operand are read through one `ValueExpression`/`evaluateValue` — the same `read` node a condition/gather consumes (no second resolver). |
| **Plan** | Rules live in the `PlanDocument` under the container component's `ValidationGraph`; the binder runs inside `RenderPlan` over `PlanBuildContext.ValidationJobs`. |
| **Kind** (kernel) | Every plan node carries its `kind`; `PlanContractGenerator` emits the TS union; `ContractDriftGate` proves agreement; `assertNever` proves the runtime switch is exhaustive. |
| **Shape** (kernel) | Each rule carries a `comparisonShape`; literals/ranges/peers are shaped once at authoring. |

It does **not** depend on Reaction or Request. The `show-validation-errors`
reaction (`p.…OnError(e => e.ValidationErrors(formId))`) and the `Validate<T>(formId)`
gate are **owned by Reaction/Request** (they emit the node and queue the
`ValidationJob`); Validation only supplies `showServerErrors`, which the Reaction
runtime handler calls. (Matrix A4, "Server validation errors" row.)

---

## 2. Public Surface (exact C# types + signatures)

Visibility follows the frozen API rule: developers touch only `ReactiveValidator<T>`
protected methods and the `ClientValidationFieldRuleBuilder` public methods. All
plan-model node types are `internal`; all constructors are `internal`. Nothing here
widens the current public surface — it renames within it.

### 2.1 Authoring entry — `ReactiveValidator<T>` (kept)

These are the developer-facing protected methods. Signatures are **exact** from
`ReactiveValidator.cs`; the spec only renames the recorded type (`ClientRule` not
`ValidationRule`). Do not change a signature.

```csharp
public abstract class ReactiveValidator<T> : AbstractValidator<T>, IClientValidationMetadataSource
    where T : class
{
    /// <summary>Opens the fluent rule surface for one client-validated field.</summary>
    protected ReactiveClientRuleBuilder<T, TValue> ClientRule<TValue>(
        Expression<Func<T, TValue>> field);

    /// <summary>Records per-item rules for each element of a collection field.</summary>
    protected ReactiveClientCollectionRuleBuilder<T, TItem> ClientRuleEach<TItem>(
        Expression<Func<T, IEnumerable<TItem>>> field) where TItem : class;

    /// <summary>Merges a child validator's client rules under the field's path prefix.</summary>
    protected void ClientRule<TChild>(
        Expression<Func<T, TChild>> field, ReactiveValidator<TChild> validator) where TChild : class;

    /// <summary>Merges another validator's client rules at the root path.</summary>
    protected void ClientRulesFrom(ReactiveValidator<T> validator);
    protected void ClientRulesFrom<TSource>(ReactiveValidator<TSource> validator) where TSource : class;

    // WhenField family — each opens a FieldStart guard, enters a ClientConditionScope,
    // and stamps RuleActivation.When(condition) on every ClientRule declared inside.
    protected void WhenField(Expression<Func<T, bool>> field, Action defineRules);
    protected void WhenField<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldNot(Expression<Func<T, bool>> field, Action defineRules);
    protected void WhenFieldNot<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldGt<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldGte<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldLt<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldLte<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules);
    protected void WhenFieldNull<TProp>(Expression<Func<T, TProp>> field, Action defineRules);
    protected void WhenFieldNotNull<TProp>(Expression<Func<T, TProp>> field, Action defineRules);
    protected void WhenFieldEmpty(Expression<Func<T, string?>> field, Action defineRules);
    protected void WhenFieldNotEmpty(Expression<Func<T, string?>> field, Action defineRules);
    protected void WhenFieldIn<TProp>(Expression<Func<T, TProp>> field, TProp[] values, Action defineRules);
    protected void WhenFieldNotIn<TProp>(Expression<Func<T, TProp>> field, TProp[] values, Action defineRules);
    protected void WhenFieldBetween<TProp>(Expression<Func<T, TProp>> field, TProp low, TProp high, Action defineRules);
    protected void WhenFieldContains(Expression<Func<T, string?>> field, string substring, Action defineRules);
    protected void WhenFieldStartsWith(Expression<Func<T, string?>> field, string prefix, Action defineRules);
    protected void WhenFieldEndsWith(Expression<Func<T, string?>> field, string suffix, Action defineRules);
    protected void WhenFieldMatches(Expression<Func<T, string?>> field, string pattern, Action defineRules);
    protected void WhenFieldMinLength(Expression<Func<T, string?>> field, int minLength, Action defineRules);
    protected void WhenFieldArrayContains<TProp>(
        Expression<Func<T, IEnumerable<TProp>?>> field, TProp value, Action defineRules);
    protected void WhenFields(
        Func<FieldConditionBuilder<T>, FieldGuard<T>> buildCondition, Action defineRules);

    // Server-only FluentValidation conditions — declaring a ClientRule inside throws
    // (the deliberate authoring boundary; matrix A "decision needed" #3).
    public new IConditionBuilder When(Func<T, bool> predicate, Action action);
    public new IConditionBuilder Unless(Func<T, bool> predicate, Action action);
    public new IConditionBuilder WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action action);
    public new IConditionBuilder UnlessAsync(Func<T, CancellationToken, Task<bool>> predicate, Action action);
    // (+ the ValidationContext<T> overloads, identical shape)
}
```

### 2.2 Per-field fluent surface — `ClientValidationFieldRuleBuilder<TModel,TValue>` (kept, public sealed)

The 18 rule methods + peer overloads. Signatures are **exact** from source. Each
returns `this` so rules chain. The only internal change: each lowers to a
`ClientRule` carrying a `RuleName` + `RuleOperand` + `RuleActivation` + `Shape`.

```csharp
public sealed class ClientValidationFieldRuleBuilder<TModel, TValue> where TModel : class
{
    internal ClientValidationFieldRuleBuilder(
        ClientValidationRuleSet rules,
        ClientValidationFieldToken<TModel, TValue> field,
        ClientRuleActivation activation);

    // No-operand (RuleOperand.None, Shape.None)
    public ClientValidationFieldRuleBuilder<TModel, TValue> Required(string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> Empty(string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> Email(string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> Url(string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> CreditCard(string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> AtLeastOne(string message);

    // Length (literal int operand; guard length >= 0)
    public ClientValidationFieldRuleBuilder<TModel, TValue> MinLength(int length, string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> MaxLength(int length, string message);

    // Regex (literal string operand; guard non-empty pattern)
    public ClientValidationFieldRuleBuilder<TModel, TValue> Regex(string pattern, string message);

    // Range (two literal bounds, same shape)
    public ClientValidationFieldRuleBuilder<TModel, TValue> Range(TValue lo, TValue hi, string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> ExclusiveRange(TValue lo, TValue hi, string message);

    // Ordered literal — note Gte→min, Lte→max collapse (same RuleName token)
    public ClientValidationFieldRuleBuilder<TModel, TValue> Min(TValue minimum, string message);                 // RuleName.Min
    public ClientValidationFieldRuleBuilder<TModel, TValue> Max(TValue maximum, string message);                 // RuleName.Max
    public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(TValue minimum, string message);// RuleName.Min
    public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(TValue maximum, string message);   // RuleName.Max
    public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(TValue value, string message);           // RuleName.Gt
    public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(TValue value, string message);              // RuleName.Lt

    // Equality literal
    public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(TValue expected, string message);            // RuleName.EqualTo
    public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqual(TValue forbidden, string message);          // RuleName.NotEqual

    // Peer overloads (Expression form + Token form). Each EnsureField(peer) then RuleOperand.PeerField.
    public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(Expression<Func<TModel, TValue>> peer, string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqualTo(Expression<Func<TModel, TValue>> peer, string message);
    public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(Expression<Func<TModel, TValue>> peer, string message);          // RuleName.Gt peer
    public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(Expression<Func<TModel, TValue>> peer, string message); // RuleName.Min peer
    public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(Expression<Func<TModel, TValue>> peer, string message);             // RuleName.Lt peer
    public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(Expression<Func<TModel, TValue>> peer, string message);    // RuleName.Max peer
    // (+ the ClientValidationFieldToken<TModel,TValue> overload for each peer method)
}
```

### 2.3 Plan-model node family — `ValidationGraph` (renamed/extracted, `internal`)

Extracted from `ComponentObject.cs:383-677` into its own home
(`PlanModel/Validation/`). **The wire shape is unchanged** — only the C# type name
`PlanModel.ValidationRule` → `ValidationRuleNode` and the file move. The JSON it
writes is the current `{ name, message, execution:{ kind, value?, activation, comparisonShape } }`.

```csharp
// PlanModel/Validation/ValidationRuleNode.cs
internal sealed class ValidationRuleNode            // was PlanModel.ValidationRule
{
    public string Name { get; }                     // RuleName.Value
    public string Message { get; }
    internal ValidationRuleExecution Execution { get; }
    internal ValidationRuleNode(RuleName name, ValidationMessage message, ValidationRuleExecution execution);
}

internal abstract class ValidationRuleExecution      // Kind: "none" | "constraint" | "peer"
{
    public ValidationRuleActivation Activation { get; }
    public Shape ComparisonShape { get; }
    public abstract string Kind { get; }
    internal static ValidationRuleExecution WithoutTarget(ValidationRuleActivation a, Shape s);   // none
    internal static ValidationRuleExecution WithConstraint(ValueExpression literal, ...);          // constraint (literal only)
    internal static ValidationRuleExecution WithPeer(ValueExpression read, ...);                   // peer (read only)
}

internal abstract class ValidationRuleActivation     // Kind: "always" | "when"
{
    public abstract string Kind { get; }
    internal static ValidationRuleActivation Always { get; }
    internal static ValidationRuleActivation When(ConditionGraph condition);  // reuses Condition's graph
}

internal sealed class ComponentValidation            // one per field, keyed by component id
{
    public string Component { get; }                 // deterministic IdGenerator id
    public ValueExpression Value { get; }            // how to read the field value (evaluateValue)
    public string ServerFieldName { get; }           // model path, for server-error mapping
    public IReadOnlyList<ValidationRuleNode> Rules { get; }
    internal static ComponentValidation ForServerField(string component, ValueExpression value,
        IReadOnlyList<ValidationRuleNode> rules, string serverFieldName);
}
// ContainerScope.ValidationRules: IReadOnlyList<ComponentValidation> lives on the
// container BrowserObject's ValidationContainerBinding (ComponentRole.ValidationContainer).
```

### 2.4 TS counterpart (generated by **Kind**'s `PlanContractGenerator` — do not hand-edit)

The contract Validation crosses is `runtime/types/plan.ts`. The generator emits it
from §2.3; the developer never writes it. It must match exactly (verified by
`ContractDriftGate` + `npm run typecheck`):

```ts
export interface ValidationContainerScope { kind: "validation-container"; validationRules: ComponentValidation[]; }
export interface ComponentValidation { component: string; value: ValueExpression; serverFieldName: string; rules: ValidationRule[]; }

export type ValidationRuleName =                      // the 18 tokens, generated from C# RuleName
  | "required" | "empty" | "minLength" | "maxLength" | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange" | "min" | "max" | "gt" | "lt" | "equalTo" | "notEqual" | "notEqualTo" | "atLeastOne";

export type ValidationRule =                          // the 6 wire-evaluation families (narrowed by generator)
  | NoOperandValidationRule | LengthValidationRule | RegexValidationRule | RangeValidationRule
  | OrderedComparisonValidationRule | PeerOrderedComparisonValidationRule
  | LiteralEqualityValidationRule | PeerEqualityValidationRule;

export type ValidationRuleExecution =
  | NoOperandValidationRuleExecution                          // { kind:"none", activation, comparisonShape }
  | ScalarConstraintValidationRuleExecution                   // { kind:"constraint", value: LiteralExpression, ... }
  | NumericConstraintValidationRuleExecution                  // { kind:"constraint", value: NumericLiteralExpression, ... }
  | TextConstraintValidationRuleExecution                     // { kind:"constraint", value: TextLiteralExpression, ... }
  | RangeConstraintValidationRuleExecution                    // { kind:"constraint", value: RangeLiteralExpression, ... }
  | PeerValidationRuleExecution;                              // { kind:"peer", value: ReadExpression, ... }

export type ValidationRuleActivation =
  | { kind: "always" }
  | { kind: "when"; condition: ValidationCondition };
```

### 2.5 Runtime public functions (TS — `validation/index.ts` barrel)

These are the only exports the rest of the runtime (boot, Reaction `show-validation-errors`)
calls. Signatures are **exact** from source.

```ts
// validation/orchestrator.ts
export function validateContainer(plan: PlanDocument, containerKey: string, ctx?: ExecContext): boolean;
export function showServerErrors(plan: PlanDocument, containerKey: string, data: unknown): void;
export function clearContainerValidation(plan: PlanDocument, containerKey: string): void;
export function revalidateField(plan: PlanDocument, containerKey: string, componentKey: string): void;

// validation/live-clear.ts
export function wireLiveValidation(plan: PlanDocument, containerKey: string, signal?: AbortSignal): void;
export function unwireField(componentId: string): void;
// NOTE: resetLiveClearForTests is one of the four reset*ForTests functions the
// redesign DELETES (Plan module owns explicit ActivePlan). Do NOT re-export it.

// validation/rule-engine.ts (pure)
export function ruleFails(evaluation: RuleEvaluation): boolean;
```

---

## 3. Input → Output Contract

### Authoring side (sync, render-time)

| In | Out |
|---|---|
| `ReactiveValidator<T>` subclass with `ClientRule`/`WhenField*` declarations, registered through DI (`AddReactiveClientValidation`) or `IClientValidationMetadataSource` | `IReadOnlyList<ClientValidationField>` — each field carries an ordered `ClientRule` list + item-fields, with `RuleActivation` already stamped by the enclosing `WhenField` scope |
| `ClientValidationRuleBinder.BindQueuedJobs()` over `PlanBuildContext.ValidationJobs`, with the registered-input map | The container `BrowserObject` gains a `ValidationContainerBinding` (`ContainerScope`) holding one `ComponentValidation` per resolved field, keyed by `IdGenerator` id, with `serverFieldName` set |

### Wire side

`PlanDocument.components[containerId].container = { kind:"validation-container", validationRules: ComponentValidation[] }`, camelCase, written by **Kind**'s `PlanSerializer`.

### Runtime side (sync; the only async path is `WhenField` reusing Condition's `confirm` wrapper, which does not occur here — `WhenField` activation is a pure compare)

| In | Out |
|---|---|
| `validateContainer(plan, containerId, ctx?)` | `boolean` (form valid); side effect: inline error spans + summary div updated through `errorDisplay` |
| `showServerErrors(plan, containerId, data)` | `void`; side effect: each `{errors:{field:[msg]}}` entry placed on its component (by `serverFieldName`) or in the summary |
| `revalidateField(plan, containerId, componentId)` | `void`; re-runs one field's rules on blur/change |
| `ruleFails(evaluation)` | `boolean` — pure, no DOM |

### Invariants (value-object construction — null is unrepresentable by construction, NOT guarded by exceptions at the runtime boundary)

1. **A `RuleName` is one of 18 tokens, period.** `RuleName` is an `internal` value
   object over a closed `Known` dictionary; `From(string)` throws at the *authoring*
   boundary for an unknown token (developer error). The runtime never constructs one
   — it switches on the generated union and `assertNever`s the default.
2. **A `RuleOperand` is exactly `none | constraint | peer`.** A `constraint` always
   carries a `LiteralExpression`; a `peer` always carries a `ReadExpression`.
   `WithConstraint` rejects a non-literal and `WithPeer` rejects a non-read **at
   construction** (`ValidationRuleExecution.WithConstraint/WithPeer`,
   `ComponentObject.cs:536-562`) — the invalid pairing is unrepresentable in the plan,
   not defended in TS.
3. **`comparisonShape` is always present and non-null.** A no-operand rule carries
   `Shape.None`; a literal/range carries the inferred literal shape; a peer carries
   the peer field's shape. There is no nullable shape and no `[JsonIgnore]` shape
   marker — `Shape.None` is the sentinel-by-construction, not `null`.
4. **A field declared with conflicting shapes is rejected at authoring.**
   `ClientValidationRuleSetField.AssertSameShape` throws when one path gets two shapes
   (`ClientValidationRuleSet.cs:88`). This is the authoring boundary, not a runtime
   check.
5. **An empty value passes every conditional/format rule except `Required`.**
   `Required` owns emptiness; `email/url/creditCard/regex/min/max/range/length/equalTo`
   all return `false` (pass) on an empty subject (`rule-engine.ts`: each `*Fails`
   guards `subject.isEmpty`). `gt` and `notEqual`/`notEqualTo` are the documented
   exceptions: `gt` treats empty as failing, `notEqual` passes empty. Preserve these —
   they are matrix rows A1, not bugs.
6. **`WhenField` activation nests, never replaces.** The activation is a wrapper over
   any A1/A2 rule; an inactive rule is skipped, and an unmounted field with only
   inactive rules stays valid (`orchestrator.ts:isRuleActive`,
   `allRulesInactiveForUnmountedField`).
7. **A `ClientRule` inside FluentValidation `When/Unless/WhenAsync/UnlessAsync`
   throws.** `ClientConditionScope.ActiveClientRuleActivation` throws when
   `_serverOnlyDepth > 0` (`ReactiveValidator.cs:267`). The deliberate authoring
   boundary — server conditions stay server-only; use `WhenField` for the browser.
8. **The server stays the authority.** Async/`MustAsync`/server-only rules never
   extract; only the 18 deterministic rule types lower to the client. `showServerErrors`
   maps server `{errors}` by the build-time `serverFieldName` — never by a runtime
   heuristic.

---

## 4. File Layout

> Author side under `Alis.Reactive/Validation/` (the rule surface) and
> `Alis.Reactive.FluentValidator/` (the `ReactiveValidator<T>` entry). Plan-model
> nodes move into a dedicated `PlanModel/Validation/` folder — the **own home** the
> redesign calls for, extracted from `ComponentObject.cs`. Runtime under
> `runtime/validation/` (already cohesive — keep).

```
Alis.Reactive.FluentValidator/
  ReactiveValidator.cs                  (kept — authoring entry; renames recorded type to ClientRule)
  ReactiveClientRuleBuilder.cs          (kept)
  FieldConditionBuilder.cs              (kept — WhenFields guard tree)
  SelectedClientValidationField.cs      (kept — WhenFieldArrayContains guard)
  IClientValidationMetadataSource.cs    (kept)

Alis.Reactive/Validation/
  ClientRule.cs                         (renamed from ValidationRule.cs — the AUTHORING rule)
  RuleOperand.cs                        (renamed from the operand types inside ValidationRule.cs)
  RuleName.cs                           (renamed from ValidationRuleName in ValidationTerms.cs)
  CollectionItemBinding.cs              (NEW — replaces substring path arithmetic)
  ClientValidationFieldRuleBuilder.cs   (kept — the 18 rule methods)
  ClientValidationRuleSet.cs            (kept)
  ClientValidationField.cs              (kept)
  ClientValidationFieldToken.cs         (kept)
  FieldCondition.cs                     (kept — symbolic WhenField tree → ConditionGraph)
  ErrorElementNaming.cs                 (NEW — one constant for {id}_error / {planId}_validation_summary;
                                         C# const projected to TS, closing the duplicate-id smell)
  ReactiveClientValidationServiceCollectionExtensions.cs  (kept — DI)
  IClientValidationRuleSource.cs        (kept)

Alis.Reactive/
  ClientValidationRuleBinder.cs         (kept — render-time binder)

Alis.Reactive/PlanModel/Validation/     (NEW FOLDER — ValidationGraph's own home, extracted from ComponentObject.cs:383-677)
  ValidationGraph.cs                    (ContainerScope + ComponentValidation + ComponentValidationRules)
  ValidationRuleNode.cs                 (renamed from PlanModel.ValidationRule — the PLAN node)
  ValidationRuleExecution.cs            (none | constraint | peer)
  ValidationRuleActivation.cs           (always | when → ConditionGraph)

Alis.Reactive.Assets/runtime/validation/
  index.ts            (kept — barrel; drop resetLiveClearForTests export)
  orchestrator.ts     (kept — validateContainer / showServerErrors / revalidateField / clearContainerValidation)
  rule-engine.ts      (kept — pure ruleFails)
  rule-operands.ts    (kept — ValidationSubject / ValidationScalarTarget / ValidationRangeTarget / ShapedComparison)
  error-display.ts    (kept — inline + summary; consume ErrorElementNaming TS constant)
  live-clear.ts       (kept — per-field wiring; drop resetLiveClearForTests)
```

---

## 5. Compile-Ready Skeleton

Bodies are `// TODO` referencing the fixtures in §6. A dev fills each in by reading
the cited source line — no design decisions remain.

### 5.1 `Alis.Reactive/Validation/RuleName.cs` (renamed from `ValidationRuleName`)

```csharp
namespace Alis.Reactive.Validation
{
    /// <summary>The single source of the 18 client validation rule names. Projected
    /// into the TS <c>ValidationRuleName</c> union by PlanContractGenerator.</summary>
    internal sealed class RuleName : System.IEquatable<RuleName>
    {
        // TODO: copy the closed Known dictionary verbatim from ValidationTerms.cs:88-109
        //       (18 tokens, StringComparer.Ordinal).
        internal string Value { get; }
        private RuleName(string value) { Value = value; }

        // TODO: 18 static accessors (Required..AtLeastOne) — ValidationTerms.cs:118-135.
        internal static System.Collections.Generic.IReadOnlyCollection<string> Values { get; } // = Known.Keys
        internal static RuleName From(string value); // TODO: throw at authoring boundary on unknown (ValidationTerms.cs:138)

        public bool Equals(RuleName? other) => /* TODO: Ordinal value equality */;
        public override int GetHashCode() => /* TODO */;
    }
}
```

### 5.2 `Alis.Reactive/Validation/ClientRule.cs` (renamed from authoring `ValidationRule`)

```csharp
namespace Alis.Reactive.Validation
{
    /// <summary>One client validation rule a developer records. Lowers to a plan
    /// <see cref="PlanModel.Validation.ValidationRuleNode"/> at render time.</summary>
    internal sealed class ClientRule
    {
        // TODO: fields _rule(RuleName) _message(ValidationMessage) _operand(RuleOperand)
        //       _activation(ClientRuleActivation) _shape(Shape) — ValidationRule.cs:13-17.
        internal ClientRule(RuleName rule, ValidationMessage message, RuleOperand operand,
            ClientRuleActivation activation, Shape shape) { /* TODO assign */ }

        // TODO: ToPlanRule(ValidationPlanBinding) → ValidationRuleNode — ValidationRule.cs:44-53.
        // TODO: PrefixedBy(prefix, parentActivation) for nested child validators — ValidationRule.cs:55-65.
        // TODO: PeerFieldReferences passthrough — ValidationRule.cs:67-68.
        internal System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }
    }
}
```

### 5.3 `Alis.Reactive/Validation/RuleOperand.cs` (renamed from `ValidationRuleOperand`)

```csharp
namespace Alis.Reactive.Validation
{
    internal abstract class RuleOperand   // none | constraint(literal/range) | peer
    {
        internal static RuleOperand None { get; } // TODO new NoRuleOperand()
        internal static RuleOperand Literal(object? value, Shape shape);  // TODO
        internal static RuleOperand Range(ValidationRangeBounds bounds);  // TODO
        internal static RuleOperand PeerField(ValidationFieldPath path, Shape shape); // TODO EnsureField at caller

        internal abstract System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }
        internal abstract RuleOperand PrefixedBy(ValidationFieldPath prefix);

        /// <summary>Lower to the plan execution: none→WithoutTarget, literal/range→WithConstraint, peer→WithPeer.</summary>
        internal abstract PlanModel.Validation.ValidationRuleExecution ToPlanExecution(
            PlanModel.Validation.ValidationRuleActivation activation, ValidationPlanBinding binding, Shape comparisonShape);
        // TODO: copy the 4 concrete subclasses verbatim from ValidationRule.cs:103-223
        //       (No/Literal/Range/PeerField) — the redesign keeps the bodies, renames the type.
    }
}
```

### 5.4 `Alis.Reactive/Validation/CollectionItemBinding.cs` (NEW)

```csharp
namespace Alis.Reactive.Validation
{
    /// <summary>Binds one validation rule to a single rendered item of a collection
    /// field, replacing today's substring/bracket path arithmetic.</summary>
    internal sealed class CollectionItemBinding
    {
        // TODO: a real value object carrying (collectionPath, itemIndex, itemFieldPath)
        //       → the rendered item's deterministic component id + serverFieldName "Lines[i].Field".
        //       Construct from the registered-input map in ClientValidationFieldBinder.ResolveAll
        //       (matrix A4 "ClientRuleEach"). NO string arithmetic — index is a field, not a parse.
        internal string ComponentId { get; }
        internal string ServerFieldName { get; }
    }
}
```

### 5.5 `Alis.Reactive/Validation/ErrorElementNaming.cs` (NEW, shared constant)

```csharp
namespace Alis.Reactive.Validation
{
    /// <summary>The one source of the validation DOM element ids. C# side; the same
    /// rule is emitted into a TS constant so plan and DOM agree by construction.</summary>
    internal static class ErrorElementNaming
    {
        // TODO: InlineErrorId(componentId)  => componentId + "_error"        (error-display.ts:98)
        // TODO: SummaryId(planId)           => Sanitize(planId) + "_validation_summary" (error-display.ts:70)
        // TODO: Sanitize: replace [.+] with "_" — match the TS regex EXACTLY.
    }
}
```

### 5.6 `Alis.Reactive/PlanModel/Validation/ValidationRuleNode.cs` (renamed/moved)

```csharp
namespace Alis.Reactive.PlanModel.Validation
{
    [System.Text.Json.Serialization.JsonConverter(typeof(ValidationRuleNodeJsonConverter))]
    internal sealed class ValidationRuleNode   // was PlanModel.ValidationRule (ComponentObject.cs:451)
    {
        public string Name { get; }        // TODO RuleName.Value
        public string Message { get; }     // TODO ValidationMessage.Value
        internal ValidationRuleExecution Execution { get; }
        internal ValidationRuleNode(/* RuleName, ValidationMessage, ValidationRuleExecution */) { /* TODO */ }
        // TODO: JsonConverter writes { "name", "message", "execution":{...} } — ComponentObject.cs:472-499.
    }
}
```

### 5.7 `Alis.Reactive/PlanModel/Validation/ValidationRuleExecution.cs` (moved)

```csharp
namespace Alis.Reactive.PlanModel.Validation
{
    internal abstract class ValidationRuleExecution     // kind: none | constraint | peer
    {
        public ValidationRuleActivation Activation { get; }
        public Shape ComparisonShape { get; }
        public abstract string Kind { get; }

        // TODO: WriteTo writes { "kind", <operand>, "activation", "comparisonShape" } — ComponentObject.cs:517-525.
        internal static ValidationRuleExecution WithoutTarget(ValidationRuleActivation a, Shape s);                  // none
        internal static ValidationRuleExecution WithConstraint(ValueExpression literal, ValidationRuleActivation a, Shape s); // TODO: reject non-LiteralExpression (536-548)
        internal static ValidationRuleExecution WithPeer(ValueExpression read, ValidationRuleActivation a, Shape s);          // TODO: reject non-ReadExpression (550-562)
        // TODO: 3 sealed subclasses (None/Constraint/Peer) — ComponentObject.cs:564-618. Constraint writes "value":literal; Peer writes "value":read.
    }
}
```

### 5.8 `Alis.Reactive/PlanModel/Validation/ValidationRuleActivation.cs` (moved)

```csharp
namespace Alis.Reactive.PlanModel.Validation
{
    [System.Text.Json.Serialization.JsonConverter(typeof(ValidationRuleActivationJsonConverter))]
    internal abstract class ValidationRuleActivation    // kind: always | when
    {
        public abstract string Kind { get; }
        internal static ValidationRuleActivation Always { get; }              // TODO kind:"always", no payload
        internal static ValidationRuleActivation When(ConditionGraph condition); // TODO kind:"when", writes "condition"
        // TODO: copy verbatim from ComponentObject.cs:621-676 — Condition's ConditionGraph is reused, NOT re-modeled.
    }
}
```

### 5.9 `Alis.Reactive/PlanModel/Validation/ValidationGraph.cs` (extracted home)

```csharp
namespace Alis.Reactive.PlanModel.Validation
{
    /// <summary>The plan's validation rules, in their own home (extracted from the
    /// ComponentObject god-file). Held by the container BrowserObject's
    /// ValidationContainerBinding.</summary>
    internal sealed class ComponentValidation
    {
        public string Component { get; }            // deterministic IdGenerator id
        public ValueExpression Value { get; }       // read-the-field-value expression (evaluateValue)
        public string ServerFieldName { get; }      // model path for server-error mapping
        public System.Collections.Generic.IReadOnlyList<ValidationRuleNode> Rules { get; }
        internal static ComponentValidation ForServerField(string component, ValueExpression value,
            System.Collections.Generic.IReadOnlyList<ValidationRuleNode> rules, string serverFieldName);
        // TODO: copy verbatim from ComponentObject.cs:383-420; only ValidationRule→ValidationRuleNode.
    }
    // TODO: ContainerScope/ValidationContainerBinding/ComponentValidationRules — ComponentObject.cs:266-448.
}
```

### 5.10 Runtime — `validation/rule-engine.ts` (kept; no change, listed so it is filled exhaustively)

```ts
// Pure: switch on rule.name, narrow literal-vs-peer, return *Fails boolean.
export function ruleFails(evaluation: RuleEvaluation): boolean {
  // TODO: exact body — rule-engine.ts:46-51 (peer-vs-field split) then the
  //       fieldRuleFails / peerTargetRuleFails switches (each ends in assertNever).
  //       Reuse RuntimeShape via rule-operands.ts; NO new compare engine — the
  //       compareTo/equalsTarget helpers already shape once.
}
```

### 5.11 Runtime — `validation/orchestrator.ts` (kept)

```ts
export function validateContainer(plan: PlanDocument, containerKey: string, ctx?: ExecContext): boolean {
  // TODO: resolve container via RuntimePlan; read each ComponentValidation; for each
  //       rule, isRuleActive(activation) — "always"→true, "when"→evaluateCondition
  //       (Condition's CompareEngine, NOT a local evaluator); failsRule via ruleFails
  //       (peer read through evaluateValue); reportRuleFailure inline-or-summary by
  //       isErrorSpanHidden. Exact: orchestrator.ts:88-145, 267-442.
}
export function showServerErrors(plan: PlanDocument, containerKey: string, data: unknown): void {
  // TODO: map {errors:{field:[msg]}} by serverFieldName to ComponentValidation;
  //       inline if {id}_error visible else summary. Exact: orchestrator.ts:147-204.
}
export function revalidateField(plan, containerKey, componentKey): void { /* TODO orchestrator.ts:210-245 */ }
export function clearContainerValidation(plan, containerKey): void { /* TODO orchestrator.ts:247-263 */ }
```

---

## 6. Acceptance Fixtures (matrix cases this module must satisfy)

Every fixture below is a **named row** in
[`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md),
**Band A**. The module is not done until each is covered by a C# domain test (the DSL
call produces the node), a TS runtime test (`ruleFails`/`validateContainer` behavior
in jsdom), and a Playwright slice (page-visible). Cases are listed by name.

### A1 — the 18 rule types (one `RuleName` × operand-execution cell each)

1. **Required** (no-operand) — empty fails, non-empty clears.
2. **Empty / Email / Url / CreditCard / AtLeastOne** (no-operand family) — empty
   passes format rules; `atLeastOne` checks a multi-value subject.
3. **MinLength / MaxLength** (length, literal int) — non-empty out-of-bound fails;
   empty passes.
4. **Regex** (literal string) — non-empty non-match fails; bad pattern fails closed.
5. **Range / ExclusiveRange** (two literal bounds, same shape) — outside `[lo,hi]` /
   `(lo,hi)` fails; empty passes.
6. **Min / Max / GreaterThanOrEqualTo / LessThanOrEqualTo** (ordered literal;
   `Gte→min`, `Lte→max` collapse) — below min / above max fails; empty passes.
7. **GreaterThan / LessThan** (ordered literal, strict) — strict; `gt` treats empty
   as failing (preserve).
8. **EqualTo / NotEqual** (literal equality) — `equalTo` non-equal fails; `notEqual`
   non-empty equal-to-forbidden fails; empty passes.

### A2 — peer-field comparison rules (6 variants)

9. **EqualTo / NotEqualTo (peer)** — field compared to live peer value;
   `execution.kind:"peer"`, peer read through `evaluateValue`.
10. **GreaterThan / GreaterThanOrEqualTo / LessThan / LessThanOrEqualTo (peer)** —
    ordered comparison against live peer (`Gte→min`, `Lte→max` peer collapse).

### A3 — conditional activation (`WhenField` family) — reuses Condition's `CompareEngine`

11. **WhenField truthy** — `WhenField(m=>m.IsMember, () => {...})`; enclosed rules
    carry `activation:{kind:"when", condition}`; inactive rule skipped; unmounted
    field with only inactive rules stays valid.
12. **WhenField equals value** — `WhenField(m=>m.Country, "US", ...)`; `compare op:"eq"`.
13. **WhenFieldNot / WhenFieldNot(value) / WhenFieldGt / WhenFieldGte / WhenFieldLt /
    WhenFieldLte / WhenFieldNull / WhenFieldNotNull / WhenFieldEmpty / WhenFieldNotEmpty /
    WhenFieldIn / WhenFieldNotIn / WhenFieldBetween / WhenFieldContains / WhenFieldStartsWith /
    WhenFieldEndsWith / WhenFieldMatches / WhenFieldMinLength** — each maps to ONE
    `CompareOp` (`Falsy/Neq/Gt/Gte/Lt/Lte/IsNull/NotNull/IsEmpty/NotEmpty/In/NotIn/Between/Contains/StartsWith/EndsWith/Matches/MinLength`).
14. **WhenFieldArrayContains** — collection-item guard; `compare op:"arrayContains"`.
15. **WhenFields (composed guard)** — `And→all`, `Or→any`, `Not→not` over `compare`
    leaves; deterministic flattening.

### A4 — collection, server errors, display surfaces (5)

16. **ClientRuleEach (per-item)** — one `ValidationRuleNode` per item field, keyed by
    the item's deterministic id via `CollectionItemBinding` (NOT string arithmetic).
17. **Nested child validator** — `ClientRule(m=>m.Address, new AddressValidator())`;
    child rules merge under `Address.X` paths with the parent's activation.
18. **Server validation errors** — `showServerErrors` maps `{errors}` by
    `serverFieldName`; inline if `{id}_error` visible else summary. (Reaction owns the
    `show-validation-errors` node; Validation owns the placement.)
19. **Inline error display** — `{id}_error` span via `ErrorElementNaming`; one constant,
    no drift.
20. **Validation summary fallback** — `{planId}_validation_summary` div; collects
    hidden-field/server errors; hidden when empty.

### Cross-cutting acceptance (the redesign's named fixes)

- **No two `ValidationRule` types** — `ClientRule` (authoring) vs `ValidationRuleNode`
  (plan). A grep for `class ValidationRule` returns zero.
- **One `RuleName` source** — the TS `ValidationRuleName` union is generated from C#
  `RuleName`; `ContractDriftGate` proves agreement (matrix "decision needed" #1
  resolved: `PlanContractGenerator` owns the narrowing).
- **One `ErrorElementNaming`** — `{id}_error` / `{planId}_validation_summary` come
  from one C#-const-projected-to-TS source (matrix "decision needed" #2 pattern).
- **`WhenField` reuses Condition's engine** — `orchestrator.isRuleActive` calls
  `evaluateCondition`; there is no validation-local compare engine.
- **No `resetLiveClearForTests` shipped** — the four `reset*ForTests` functions are
  deleted; `ActivePlan` is passed explicitly (Plan module).
