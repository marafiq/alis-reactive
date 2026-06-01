# 09 — Final DSL Naming Sheet

The single authoritative naming decision for the green-field rewrite. Synthesized
from the seven area surveys (Triggers/Reactions, Conditions/Value, HTTP/Gather/Response,
Array ops, Components/InputField, Validation, Kernel/Plan) and reconciled to
**one concept → one name across C#, JSON, TS, tests, and docs**.

This is a clean rewrite, not a refactor. Every final name passes **the cold
one-breath read**: shown alone to a .NET dev who never saw the codebase, they say
what it does in one breath *and are right*. A name is renamed only when it **lies,
collides, or needs a paragraph** — never for novelty.

Banned-word guard applied throughout: `{artifact, contribution, claim, reject,
fallback, registry, lifecycle, Manager, Helper, Util, Info, Data, Context}`
(the lone allowed `Context` is `PlanBuildContext` / `ExecutionContext`, a *true*
ambient build/execution scope — not generic bag naming).

---

## 0. Kept Core Vocabulary — DO NOT CHURN (confirmed)

These names already pass the cold read and are stable across every area. They
survive **verbatim**. Touching them is churn, not improvement.

### 0.1 Kernel + spine

| Name | One-breath meaning |
|---|---|
| `Shape` | The **value-structure** tag on a value — scalar vs object vs array (the structural-typing axis, never CSS box / layout / form). Sacred spine name (Directive 5 do-not-churn); kept verbatim, gloss tightened to bind it to the value-typing axis. |
| `ShapeStructure` / `ShapeContractCompatibility` / `ShapeConverter` | The structure axis, the flow-compat rule, and the one runtime convert engine. |
| `Kind` | The one string discriminator each plan node carries so C# and TS agree which node it is. |
| `Plan` | The document the slices write into and the runtime boots from. |
| `PlanDocument` | The immutable, serializable plan for one model identity (version 3). |
| `PlanId` | The stable model-derived key that composes root + same-model partials. |
| `PlanScope` | Whether a plan is root (SSR-merged) or partial (slot-loadable). |
| `PlanBuildContext` | The authoring sink slices write into — narrow Declare/Wire verbs (true ambient scope). |
| `PlanSerializer` | The single owner of plan-document → camelCase JSON. |
| `PlanNodeDiscriminator<T>` | The one mechanism that writes `kind` + props from a compile-enforced base. |
| `PlanContractGenerator` | Reflects the C# node families and writes `plan.ts` from them. |
| `ContractDriftGate` / `ContractDriftResult` | The build gate that fails if `plan.ts` disagrees with the generator. |
| `IdGenerator` | Generates a component's deterministic id from model type + expression. |

### 0.2 Value spine

| Name | One-breath meaning |
|---|---|
| `ValueExpression` | The flat plan node for a value: literal, read, object, array, array-op. |
| `TypedSource<T>` | **The one typed authoring surface for any readable value** (absorbs Component/Url/Plugin/Payload/Element source families). |
| `Literal` / `Read` / `ObjectValue` / `ArrayValue` / `ArrayOp` | The five value-node variants. |
| `evaluateValue` | Reads a value from its node — pure, no IO, no DOM writes. |
| `assertNever` | Compile-time proof a switch handled every `Kind`. |

### 0.3 Graph nodes + operators

| Name | One-breath meaning |
|---|---|
| `ConditionGraph` | The deterministic predicate node: compare, all, any, not, confirm. |
| `ReactionGraph` | The executable action node: set, call, dispatch, inject, show-validation, sequence, branch, request. |
| `StartsWhen` | The trigger node: page-load, event, component-event, server-push, SignalR. |
| `CompareOp` / `CompareOperator` | The single source of the comparison token list (the wire constants + the value object). |
| `RuleName` | The single source of the validation rule tokens, projected to TS `ValidationRuleName`. |
| `ValidationGraph` / `ValidationRuleNode` | The validation slice of the plan graph, and one rule node in it. |

### 0.4 Builders + authoring surfaces (kept verbatim)

`Html.On`, `TriggerBuilder`, `PipelineBuilder`, `ElementBuilder`, `GuardBuilder`,
`BranchBuilder`, `ConditionStart`, `ConditionContinuation`, `HttpRequestBuilder`,
`GatherBuilder`, `ResponseBuilder`, `ParallelBuilder`, `ReactiveArray<T>`,
`ReactiveValue<T>`, `Html.InputField`, `InputBoundField`, `ReactiveValidator<T>`,
`ClientRule`, `WhenField`, `Plugin`, `PluginTypeBuilder`, `ComponentRef`,
`IComponent` / `IInputComponent` / `IAppLevelComponent`.

---

## 1. Grammar Fixes (Directive-4 + math-grounded)

The cross-cutting decisions, decided once, before the per-area tables. Every area
table below conforms to these.

### 1.1 And / Or collapse — THREE shapes → TWO

The `And`/`Or` guard composition had **three** authoring shapes:
`And(TypedSource)`, `And(payload, path)` / `And(ResponseBody, path)`, and
`And(nested lambda)`. The middle shape is a *second spelling* of the first — a
payload or response read already yields a `TypedSource`. **Decision: collapse to
two genuine shapes.**

| Survivor | Role |
|---|---|
| `And(TypedSource<T>)` / `Or(TypedSource<T>)` | The one flat value shape. The `(payload, path)` and `(ResponseBody, path)` overloads **fold in** via a `TypedSource` factory (`FromEvent(args, x => x.Prop)`, `responseBody.Read(path)`). |
| `And(Func<ConditionStart, GuardBuilder>)` / `Or(nested)` | The one grouping shape — `(a OR b) AND c` that the flat shape cannot express. |

`Not()` is unchanged. This same two-shape grammar is the **single** And/Or
vocabulary across the Conditions area (`GuardBuilder`), the FluentValidator side
(`FieldGuard.And/Or/Not`), and the app-level validation builder (`ClientCondition.And/Or/Not`).
`FieldCondition.All/Any/Not` stays the internal n-ary lowering only.

### 1.2 Presence vocab — KEEP ALL SIX (not nonsense)

`Truthy / Falsy / IsNull / NotNull / IsEmpty / NotEmpty` are **three orthogonal
axes**, each with a positive and negative pole:

| Axis | Positive | Negative | Tests |
|---|---|---|---|
| JS truthiness | `Truthy` | `Falsy` | null+0+""+false collapse |
| Null identity | `NotNull` | `IsNull` | exact null (0 is falsy but not null) |
| Emptiness | `NotEmpty` | `IsEmpty` | length/size (distinct from null and falsy) |

They are six genuinely distinct deterministic operators (`RequiresRightOperand=false`),
not synonyms. Collapsing any pair is a many-to-one determinism violation. **No 7th
is invented; none is removed.**

### 1.3 Array / string length pairs + numeric folds — complete the families

| Gap | Fix |
|---|---|
| `MinLength` (condition compare token) sits with no upper bound | **Add `MaxLength`** (token `max-length`, VO `MaximumTextLength` mirroring `MinimumTextLength`). |
| `Sum` exists; `Min`/`Max`/`Average` do not | **Add `Min` / `Max` / `Average`** as array-op variants (`min` / `max` / `average` wire ops). `Average` spelled in full (matches LINQ, screaming intent over `Avg`); terminal types `double` (mean is non-integral). |

Empty-input contract for the new folds: `empty → null` (same null-on-empty
contract as `FindFirst`). NaN/Infinity ordering is the already-solved `compareKeys`.

> **Validation `MinLength`/`MaxLength` ≠ condition `MinLength`/`MaxLength`.** The
> validation `minLength`/`maxLength` are `RuleName` tokens (client-rule names); the
> condition `MinLength`/`MaxLength` are `CompareOp` tokens (runtime compare). Same
> word, two concepts in two layers — they are **not** merged. The validation pair
> is already complete (no add needed there).

### 1.4 LINQ-shaped array names — KEEP the per-op verbs, NAME the closure

`ReactiveArray<T>` is deliberately **not** `IEnumerable`/`IQueryable`, so LINQ
extension methods cannot bind and there is no collision. The per-op verbs
(`Where/Select/OrderBy/OrderByDescending/Count/Any/All/Sum`) name their op
truthfully cold and **stay**. The lie was *suite-level* — the LINQ identity implies
an open surface that is really a fixed switch. **Fix: declare the closed op set
explicitly** as the per-op-variant union `ArrayOp` (see §1.5). One rename inside
the suite — `Find` lies (it is `List<T>.Find`, not on `IEnumerable`, and is
first-match-or-null without saying so): **`Find` → `FindFirst`** (wire token stays
`find`).

### 1.5 Array-op node — per-op variants, no nullable operands

`ArrayOperationExpression` (one node + two nullable `[JsonIgnore]`
`Predicate`/`Projection`) becomes one **variant per op**:
`FilterOp / MapOp / SumOp / MinOp / MaxOp / AverageOp / CountOp / AnyOp / AllOp /
FindOp / OrderByOp / OrderByDescendingOp`, each carrying only the fields it needs.
This removes the two audited null-escape-hatches by construction. The op-token
list `ArrayOp` is the single source of the array op set (mirrors `CompareOp`).

### 1.6 Confirm reclassification — surface verb KEEP, plan term RENAME

`Confirm` is a **user-decision async guard**, not a deterministic compare. It is
authored on the same guard surface (`p.Confirm("...")`, `ConditionStart.Confirm("...")`)
so it composes via `And`, but its **distinct name** correctly marks the async lane
— `Confirm` reads cold as a gate, not a comparison. **The surface verb `Confirm`
stays.** The plan-model class lies: `ConfirmCondition : ConditionGraph` lumps it
with deterministic comparisons. **Rename the plan term `ConfirmCondition` →
`ConfirmGuard`** (JSON/TS `kind:"confirm"` already correct, only the C# class
name changes). One concept, one set of names: surface `Confirm`, plan `ConfirmGuard`,
wire `confirm`.

### 1.7 RuleName 18 → 6 — narrowing is RUNTIME families, not a rename

`|Known| = 18` is fixed by the algebra. The "18 → 6" is a **runtime evaluation-family**
grouping derived/generated from the one C# `RuleName` source — **no authoring
token is renamed or removed**. The six derived families:

| Family | Members |
|---|---|
| no-operand | `required`, `empty`, `email`, `url`, `creditCard`, `atLeastOne` |
| length | `minLength`, `maxLength` |
| regex | `regex` |
| range | `range`, `exclusiveRange` |
| ordered-comparison | `min`, `max`, `gt`, `lt` |
| equality | `equalTo`, `notEqual`, `notEqualTo` |

`notEqual` (literal-only) and `notEqualTo` (peer-only) **stay distinct names**
(D1: collapsing is a determinism violation). All 18 authoring tokens survive; the
6 families are generated, never hand-named.

### 1.8 Plugin member verbs — collapse synonyms to one pair

`PluginTypeBuilder` carried `Method`/`Function` (both = value op) and `Void`/`Command`
(both = void op) — two names per concept inside one type, while the `Plugin` base
uses only `Function`/`Command`. **Collapse to one pair** across both declarers:

| Survivor | Folded-in synonym | Concept |
|---|---|---|
| `Function` | `Method` (deleted) | value-returning op |
| `Command` | `Void` (deleted — `Void` leaked the return type, not a domain verb) | void op |
| `Property` | — | readable member |

---

## 2. New Kinds + New Plan-Carried Fact

Three new names close real holes. Each is decided once **here** so consuming areas
do not re-coin them. All three are math-grounded (`08-determinism-formalization.md`
§6.1, §6.2).

| New name | Wire | C# → / TS ⇒ | What it closes |
|---|---|---|---|
| **`WholeResponseBody`** | `kind: "whole-response-body"` | C# `WholeResponseBody` node (carries no member) → / TS `WholeResponseBody` ⇒ `evaluateValue` arm | The `responseBody` sentinel collided with a real DSL property `ResponseBody` (the single live many-to-one D1 violation). A distinct kind carries no member, so the property lowers to ordinary `read` and can never collide. |
| **`WholeElement`** | `kind: "whole-element"` | C# `WholeElement` node (carries no member) → / TS `WholeElement` ⇒ `evaluateValue` arm | Same fix for the `elementValue` sentinel vs property `ElementValue`. Distinct, member-less, structurally disjoint. **Done together with `WholeResponseBody` (one §6.1 change).** |
| **`ReactionTiming`** | `timing` field, values `sync` / `async` | C# enum `ReactionTiming { Sync, Async }` on each reaction node → / TS `ReactionTiming` ⇒ routed by `executeReaction` | **Renamed from `ReactionLane` (blind-fail): bare "lane" reads cold as a scheduling/concurrency channel, not the sync-vs-async classification.** The timing is a deterministic projection of the reaction kind (`{set,call,dispatch,inject,show} → sync`, `{request,parallel} → async`). Stamped at lower-time and routed on the carried `Sync`/`Async` tag — deletes the `instanceof Promise` / `crossedAsyncBoundary` re-detection (restores D3). `Timing` not `Mode`/`Lane`/`Kind` (the `Sync`/`Async` members carry the meaning; avoids the `Kind` discriminator collision and the scheduling-channel misread). |

**Distinctness is the point.** `WholeResponseBody` and `WholeElement` are **two**
names, not one shared name — a response read and an element read are different
sources. Both are owned by the Value/Kernel area; HTTP `Into` and
Element/Array whole-reads **consume** these kinds and must not mint a third.

---

## 3. Per-Area Tables

`→` = C# authoring/plan side. `⇒` = TS runtime side. Verdict ∈ {KEEP, RENAME,
GRAMMAR-FIX, NEW, DELETE}. Every Final name passes the cold one-breath read.

### 3.1 Triggers + Reactions + Element/Dispatch

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `Html.On` | `On` | KEEP | Trigger-attach verb; `Behavior.On(trigger, reaction)` reuses it — already one-concept-one-name. |
| `TriggerBuilder<TModel>` | `TriggerBuilder` | KEEP | Builds triggers; screams intent. |
| `DomReady(pipeline)` | `PageLoad` | RENAME | Killed a 3-name spread (`DomReady` / `PageReadyTrigger` / `page-ready`). `DomReady` leaked the DOM mechanism. Now verb `PageLoad` + `PageLoadTrigger` + kind `page-load`. |
| `CustomEvent(name, …)` | `Event` | RENAME | 3-name spread (`CustomEvent` / `DocumentEventTrigger` / `document-event`); "Custom" is filler (every event is custom). Pairs exactly with emitter `p.Dispatch(name)`: listen `t.Event("x")` / emit `p.Dispatch("x")`. + `EventTrigger` + kind `event`. |
| `CustomEvent<TPayload>(name, …)` | `Event<TPayload>` | RENAME | Typed-payload overload; rename for parity, kept as a distinct arity (load-bearing lane). |
| `ServerPush(...)` (3 overloads) | `ServerPush` | KEEP | `ServerPush` / `ServerPushTrigger` / `server-push` already aligned; "server pushes events to me" (SSE) in one breath. |
| `SignalR(...)` (2 overloads) | `SignalR` | KEEP | Proper-noun protocol; aligned across layers. |
| `StartsWhen` | `StartsWhen` | KEEP | Screaming-intent trigger node; reads as a sentence head (`StartsWhen.PageLoad()`). Shared trigger concept — do not churn. |
| `StartsWhen.ComponentEvent(...)` | `ComponentEvent` | KEEP | Component-fires-event trigger (the `.Reactive()` target); `component-event` aligned. |
| `ServerPushEventFilter` (Any/Named) | `ServerPushEventFilter` | KEEP | Names which SSE events match; `AnyServerPushEvent`/`NamedServerPushEvent` read cold. |
| `PayloadContract` | `PayloadContract` | KEEP | The typed-payload shape a trigger/dispatch carries; not a banned word. |
| `Behavior` | `Behavior` | KEEP | **Reconciled to 03-naming KEEP.** "One trigger-to-reaction edge." The earlier `TriggerHandler` proposal is rejected — `Behavior` is the locked spine name (`BehaviorGraph` = all behaviors), and the internal/public asymmetry fix is structural, not a rename. |
| `PipelineBuilder<TModel>` | `PipelineBuilder` | KEEP | Builds the ordered reaction sequence (the `p` of `t.X(p => ...)`). |
| `ReactionGraph` (+ `SequenceReaction`/`ParallelReaction`/`BranchReaction`/`BranchCase`/`BranchGuard`) | unchanged | KEEP | Do-not-churn set; first-match branch routing reads as English (`BranchGuard.When`/`.Else`). |
| `ParallelCompletion` (None/Settled) | `ParallelCompletion` | KEEP | "What runs after all branches settle"; `SettledParallelCompletion` / kind `on-settled`. |
| `Set` family (`SetReaction`, `SetText`, `SetHtml`, `Set`) | unchanged | KEEP | Property-assign verb; the `SetText(TypedSource<T>)` / `(source, path)` / `(literal)` overloads are one verb over distinct value-source arities — do not collapse. |
| `Call` (`CallReaction`, `Element.Call`) | `Call` | KEEP | Method-invoke verb; `call` aligned. |
| `Dispatch(name)` / `DispatchReaction` | `Dispatch` | KEEP | Emit-event verb; mirrors `t.Event(name)`. |
| `Dispatch<TPayload>(name, payload)` | `Dispatch<TPayload>(name, payload)` | GRAMMAR-FIX | Kept as the **literal-payload** lane (compile-time literal object). Distinct from the source-backed lane below. |
| `DispatchWith<TPayload>(name, configure)` | `DispatchFrom<TPayload>` | RENAME | The fields come from live component/URL/plugin sources — the same "From a source" idea (`FromUrl`, `SetText(source)`). Pair reads cleanly: `Dispatch(name, literal)` vs `DispatchFrom(name, b => ...)`. |
| `Element(elementId)` / `ElementBuilder` | `Element` | KEEP | "Target a DOM element by id"; the non-input display escape hatch. |
| `AddClass/RemoveClass/ToggleClass` | unchanged | KEEP | Direct CSS-class verbs (mutate a live DOM element at runtime). |
| `Show()` / `Hide()` | unchanged | KEEP | Visibility verbs (`hidden=false/true`). |
| `Component<TComponent>(...)` (4 overloads) | `Component` | KEEP | "Reference a typed component"; overloads split on how you identify it (model expr / cross-model / id / layout singleton). |
| `ComponentRef<TComponent,TModel>` | `ComponentRef` | KEEP | Typed handle for Set/Call. |
| `FromUrl<T>(paramName)` | `FromUrl` | KEEP | "Read a value from the URL"; consistent with `DispatchFrom`. |
| `Plugin<T>(...)` / `PluginProperty<T>` (reaction overloads) | unchanged | KEEP | Plugin escape hatch; stringly names allowed at this boundary. |
| `Into(elementId)` | `Into` | GRAMMAR-FIX | Verb kept ("put the response into this element"). Its emitted whole-read must lower to the **`WholeResponseBody` kind** (§2), not the `responseBody` sentinel. Name stays; wiring joins the shared kind. |
| `ValidationErrors(formId)` | `ShowValidationErrors` | RENAME | A noun method reads like a getter; the cold dev expects it to *return* errors. Verb `ShowValidationErrors` = domain `ShowValidationErrorsReaction` = kind `show-validation-errors`. |
| `Inject` / `InjectReaction` | `Inject` | KEEP | Partial-slot injection verb; slot identity uses `SlotId`. |

### 3.2 Conditions + Value

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `When(...)` (3 overloads) | `When` | KEEP | Fluent entry to a condition. **Owned by Conditions** — no other area may reuse the bare `When` (see §3.5 template collision). |
| `Confirm(message)` | `Confirm` | KEEP | User-decision async guard; the distinct name marks the async lane. Plan term → `ConfirmGuard` (§1.6). |
| `ConditionStart` / `ConditionSourceBuilder` / `GuardBuilder` / `BranchBuilder` / `ConditionContinuation` | unchanged | KEEP | Names the seams; `Standalone.Then` becomes unrepresentable by typing (structural fix, not a rename). |
| `Then` / `ElseIf` / `Else` | unchanged | KEEP | First-match if/else-if/else chain; universally read. |
| `And(TypedSource<TProp>)` | `And` | KEEP | The one canonical flat And shape (§1.1). |
| `And(payload, path)` / `And(ResponseBody, path)` | folded into `And(TypedSource)` | GRAMMAR-FIX | Second spelling of the same node — folds via a `TypedSource` factory (§1.1). |
| `And(nested lambda)` | `And` (nested) | KEEP | The one grouping shape — `(a OR b) AND c`. |
| `Or<...>` (all shapes) | `Or(TypedSource)` + `Or(nested)` | GRAMMAR-FIX | Mirror of And — payload/responseBody fold in, nested stays. |
| `Not()` | `Not` | KEEP | Boolean negation; unambiguous. |
| `Eq` / `NotEq` (tokens `eq`/`neq`) | `Eq` / `NotEq` | KEEP | In conditions there is **no** literal-vs-peer split, so a single `NotEq` → one `neq` token is correct (the validation `notEqual`/`notEqualTo` distinction lives in §3.6, not here). |
| `Gt/Gte/Lt/Lte` | unchanged | KEEP | Standard ordering shorthands every .NET dev reads instantly. |
| `Truthy/Falsy/IsNull/NotNull/IsEmpty/NotEmpty` | unchanged | KEEP | The six-way presence vocab — three orthogonal axes (§1.2). |
| `In/NotIn/Between/Contains/StartsWith/EndsWith/Matches` | unchanged | KEEP | Standard membership/range/string predicates; cold reader is never wrong. |
| `MinLength(length)` | `MinLength` + **add** `MaxLength` | GRAMMAR-FIX | Complete the length pair (§1.3); token `max-length`, VO `MaximumTextLength`. |
| `ArrayContains(object item)` | `ArrayContains` | KEEP | Element-membership over an array-shaped left value; distinct from string `Contains` and from `In`. (Operand typed `object` is a weakness, but the **name** is right.) |
| `TypedSource<TProp>` | `TypedSource<T>` | KEEP | Sacred — the one typed authoring surface, shared everywhere. |
| `PayloadTypedSource` / `TypedComponentSource` / `TypedPluginSource` / `TypedPluginPropertySource` / `TypedUrlSource` | unchanged | KEEP | Each names its source family; all self-describe. |
| `ValueExpression` | `ValueExpression` | KEEP | Sacred. The 590-line god-facade is split (structure fix, not a rename). |
| `LiteralExpression`/`ReadExpression`/`ObjectExpression`/`ArrayExpression` | `Literal`/`Read`/`ObjectValue`/`ArrayValue` (kinds `literal`/`read`/`object`/`array`) | KEEP | The five value-node variants; wire kinds canonical. |
| `ValueReadAccess` / `PropertyValueReadAccess` / `MethodValueReadAccess` | unchanged | KEEP | property-read vs method-invoke discriminator; exact. |
| `ValueRead → ValueReadTarget → ValueReadPath → PayloadReadPath` | flatten into the `Read` node | RENAME | Internal 4-type indirection with no DSL-graph node of its own — collapse into `Read`. Not public surface. |
| `ReadWholeResponseBody` / sentinel `responseBody` | `WholeResponseBody` node, `kind:"whole-response-body"` | RENAME (NEW kind) | §2 — closes the one live D1 collision. |
| `ReadWholeElement` / sentinel `elementValue` | `WholeElement` node, `kind:"whole-element"` | RENAME (NEW kind) | §2 — done together with `WholeResponseBody`. |
| `ArrayOperationExpression` (nullable Predicate/Projection) | per-op variants `FilterOp`/`MapOp`/`SumOp`/`MinOp`/`MaxOp`/`AverageOp`/`CountOp`/`AnyOp`/`AllOp`/`FindOp`/`OrderByOp`/`OrderByDescendingOp` | RENAME | §1.5 — one variant per op, removes the two null-escape-hatches by construction. |

### 3.3 Array ops (ReactiveArray)

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `ReactiveArray<TElement>` | `ReactiveArray<TElement>` | KEEP | "A reactive (deferred, plan-compiled) array transform"; documents NOT-IEnumerable. No collision (§1.4). |
| `ReactiveValue<TValue>` | `ReactiveValue<TValue>` | KEEP | Terminal scalar from a fold, exposed as `TypedSource<T>`. |
| `ReactiveArraySource<TElement>` (internal) | unchanged | KEEP | Internal adapter exposing the chain as `TypedSource<T[]>`. |
| `Where` / `Select` / `OrderBy` / `OrderByDescending` | unchanged | KEEP | Truthful per-op verbs; closed transform set, no IEnumerable binding to mislead. |
| `Count` / `Any` / `All` (+ predicated overloads) | unchanged | KEEP | Cold-true; predicated overloads are C#-side sugar over one wire node. |
| `Sum(int/decimal/double)` | unchanged | KEEP | One `sum` wire node; the CLR return type only types the terminal `ReactiveValue<T>`. |
| `Find(predicate)` / `Find(predicate, selector)` | `FindFirst(...)` | GRAMMAR-FIX | `Find` lies (it is `List<T>.Find`, not on `IEnumerable`; first-match-or-null without saying so). `FindFirst` screams the first-match semantics. Wire token stays `find`. |
| `AsSource()` | `AsArraySource()` | RENAME | **`AsSource` reads cold as a generic cast — "Source" is overloaded (data/event/HTTP source) and hides that the result is a typed array source (blind-fail).** `AsArraySource` names the `TypedSource<T[]>` result so the conversion target is unambiguous and the array shape is screamed. |
| `From(...)` / `FromDom(...)` (pipeline entries) | unchanged | KEEP | Array-source entry verbs; `Dom` suffix screams the boundary. Same `From` voice as the HTTP pipeline. |
| *(no declared suite name)* | closed op set `ArrayOp` { count·filter·map·sum·**min·max·average**·any·all·find·orderBy·orderByDescending } | GRAMMAR-FIX | §1.4/§1.5 — name the closure explicitly; `ArrayOp` is the single source of the array op list. |
| *(missing)* | **`Min(selector)` → `ReactiveValue<TNum>`** | NEW | §1.3 — completes the numeric folds; wire op `min`, empty→null. |
| *(missing)* | **`Max(selector)` → `ReactiveValue<TNum>`** | NEW | §1.3 — pairs `Min`; wire op `max`, empty→null. |
| *(missing)* | **`Average(selector)` → `ReactiveValue<double>`** | NEW | §1.3 — full word (matches LINQ); wire op `average`, terminal `double`. |

> Cross-area: the per-element predicate inside every array op is the **sync
> condition subset** — it shares the one Conditions name `ConditionGraph`
> (never `Confirm` — async would break the immediate lane). The projection/sort-key
> is the shared `ValueExpression`; the whole-element identity (`x => x`) is the
> distinct `WholeElement` kind, never the `elementValue` sentinel.

### 3.4 HTTP + Gather + Response

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `Get` / `Post` / `Put` / `Delete` (+ gather overloads) | unchanged | KEEP | HTTP verbs; URLs may carry `{placeholder}` route templates. Overloads are legitimate inline-gather shorthands, not synonyms. |
| `Gather(Action<GatherBuilder>)` | `Gather` | KEEP | Screaming-intent verb: collect values from many sources into the request. |
| `AsJson()` / `AsFormData()` | unchanged | KEEP | The two-member body-format vocab; no third, no ambiguity. |
| `WhileLoading(pipeline)` | `WhileLoading` | KEEP | "Run during the in-flight window" — the load-bearing loading lane. |
| `Finally(pipeline)` | `OnSettled(pipeline)` | RENAME | **`Finally` over-promises C# try/finally semantics — a cold dev expects to read the response body inside it; the real contract has NO body available (blind-fail).** `OnSettled` pairs exactly with `Parallel`'s `OnAllSettled` and signals "after the request **settles** — success, error, or network failure; **no response body**" without the try/finally lie. `WhileLoading`+`OnSettled` are the bracket pair. |
| `Validate<TSource>(formId)` | `Validate` | KEEP | Cross-area: the **same** `Validate` verb as the Validation gate — do not fork. |
| `Response(Action<ResponseBuilder>)` | `Response` | KEEP | Opens the success/error routing scope. |
| `Include<...>` (4 overloads) | `Include` | KEEP | The one verb for "put this readable value into the payload"; coherent overload set. |
| `IncludeAll()` | `IncludeAll` | KEEP | "Include every registered input"; pairs `Include`. |
| `Static(string param, object value)` | `Literal(param, value)` | RENAME | `Static` lies — reads as a C# modifier/lifetime keyword, not a gather source. **`Constant` was the first pick but still did not scream "gather source" cold; `Literal` aligns one-concept-one-name with `ValueExpression.Literal` (the sheet's own value-node), so a literal-in-a-gather reads as "a fixed value put into the payload" the same way it reads in the value spine.** |
| `FromEvent<...>` / `FromUrl(...)` (+ typed overloads) | unchanged | KEEP | `From*` source grammar reads cold; typed overloads add shape conversion, not synonyms. |
| `Plugin<T>(source, paramName)` | `Plugin` | KEEP | The plugin escape-hatch source; intentional stringly boundary, shared with Plugins area. |
| `Header(...)` (3 overloads) | `Header` | KEEP | HTTP header target; overloads = literal/typed-source/event-arg value shapes (scalar-only guard preserved). |
| `RouteParam(...)` (5 overloads) | `RouteParam` | KEEP | Fills a `{placeholder}` in the URL template; every placeholder ↔ RouteParam cross-checked. |
| `OnSuccess` / `OnError` (+ typed/status overloads) | unchanged | KEEP | Success/error scope openers; the `int statusCode` overload is first-match status routing, not a new name. |
| `Chained(Action<HttpRequestBuilder>)` | `Chained` | KEEP | "After this response succeeds, run the next request"; one-chain-link rule matches the name. |
| `ResponseBody<T>` / `.Read<TProp>(expr)` | `ResponseBody` / `Read` | KEEP | Typed payload handle; `.Read` yields a `TypedSource<T>` — shares the value spine. The property `ResponseBody` lowers to ordinary `read` (its whole-read identity is the `WholeResponseBody` kind). |
| `Into(string elementId)` | `Into` | GRAMMAR-FIX | Verb kept ("inject the response body into this element"). Its value-node lowers to the **`WholeResponseBody` kind** (§2), de-sentineled. |
| `Parallel(params Action<HttpRequestBuilder>[] branches)` | `Parallel` | KEEP | "Run these requests concurrently"; `branches` is the right noun. |
| `OnAllSettled(pipeline)` | `OnAllSettled` | KEEP | "Run after every branch settles"; borrows `Promise.allSettled` exactly — cold reader gets settle-not-success right. |

### 3.5 Components + InputField

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `Html.InputField` (2 overloads) | `Html.InputField` | KEEP | "Render a model-bound input + its label/validation slot." |
| `InputBoundField` / `InputFieldOptions` | unchanged | KEEP | The model-bound field awaiting a component; the public config the dev fills. |
| `InputFieldConfiguration` (+ Default/Configured) | unchanged | KEEP | `internal` Strategy; names the branch honestly, no churn. |
| `IComponent` / `.Vendor` / `IInputComponent` / `.ValueMember` / `IAppLevelComponent` / `.DefaultId` | unchanged | KEEP | Role/marker names; `ValueMember` screams the gather/validation read; `DefaultId` is the fixed well-known id. |
| `Fusion*` / `Native*` prefixes | unchanged | KEEP | HARD RULE — vendor-isolation screaming names, one-concept-one-name across slices. |
| `.Reactive(plan, eventSelector, pipeline)` | `.Reactive` | KEEP | The one shared verb "wire this browser event into a reactive pipeline" — identical shape everywhere. |
| `TypedEvent<TArgs>` props (`Changed`, …) | unchanged | KEEP | Past-tense event nouns ("it changed"); consistent across slices. |
| `ElementBuilder` / `Element` / `AddClass`/`RemoveClass`/`ToggleClass` / `SetText`/`SetHtml` / `Show`/`Hide` | unchanged | KEEP | DOM-mutation verbs; `SetText`/`SetHtml` overloads are one verb over the unified value sources (ValueExpression one-value-path). |
| `ComponentRef.FocusIn()` (native) | `Focus()` | RENAME | `FocusIn` lies — the DOM method is `focus`; `focusin` is a *different* bubbling event. Other slices use `ComponentMethod.Named("focus")` — collapse to `Focus()`. |
| `.SetValue(value)` / `.Value()` | unchanged | KEEP | Mirror the JS members; the read verb is the member name `Value` (feeds conditions/gather via `TypedComponentSource`). |
| `FusionTemplate.Create<TModel>` / `FusionTemplateBuilder` | unchanged | KEEP | Typed Syncfusion row/item template builder; vendor-prefixed factory. |
| `.Id(string)` (template) | `Id` | KEEP | Sets the div `id`; mirrors the HTML attribute. |
| `.Class(string)` (template) | `CssClass` | GRAMMAR-FIX | Bare `Class` glares against the C# keyword and reads ambiguously cold. `CssClass` screams the HTML attribute. **Not** `AddClass` — that mutates a live DOM element at runtime; this appends to an SSR string (different lane). |
| `.Attr(name,value)` (template) | `Attribute` | GRAMMAR-FIX | `Attr` is an abbreviation — spell it. "Add an HTML attribute." |
| `.Text<TProp>(expr)` / `.Text(string)` | `Text` | KEEP | Bind/emit text content; two overloads of one noun. |
| `.Span` / `.Img` / `.Div` / `.Badge` / `.Icon` / `.Button` / `.Link` / `.Raw` / `.Render` (template) | unchanged | KEEP | Each names the HTML element/escape-hatch/terminal it emits; element-named, reads cold. |
| `.ButtonFor<TProp>(...)` | `ButtonFor` | KEEP | `*For` = bound-to-a-model-property variant; matches the `XxxFor` factory convention. |
| `.EventButton<TProp>(...)` | `DispatchButton<TProp>(...)` | RENAME | **`EventButton` reads cold as "a button that has events" (every button does) — uninformative (blind-fail).** `DispatchButton` names the action the way `ButtonFor` names the binding: it emits a `Dispatch` carrying the row id. Pairs with the `Dispatch` reaction verb so listen/emit read consistently. |
| `.When(condition, then[, else])` (template) | `WhenTemplate` | RENAME (collision) | **Cross-area collision** with Conditions `When`, but a different concept: emits an SF `${if(...)}` **SSR string**, not a runtime `ConditionGraph`. The lane is load-bearing — rename to scream the template lane. Conditions owns the bare `When`. |
| `.ShowIf(condition, content)` (template) | `ShowTemplateIf` | RENAME (collision) | Thin alias of `When`; also collides cross-area. Keep the one template-lane pair `WhenTemplate`/`ShowTemplateIf`. |
| `FusionConditionalBuilder` / `TemplateElseBranch` (+ Missing/Present) | unchanged | KEEP | Internal then/else body builder and else-presence Strategy; no `null` else. |

### 3.6 Validation

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `ReactiveValidator<T>` | `ReactiveValidator<T>` | KEEP | The one base devs subclass; reads in one breath. |
| `IClientValidationMetadataSource` | `IClientValidationRules` | RENAME | It IS the client-rule source (`GetClientRules()`), not vague "metadata" + banned-flavor "…Source/…Data". Name says "the client validation rules of this validator". |
| `ClientRule<TValue>(field)` / `ClientRuleEach` / nested `ClientRule` / `ClientRulesFrom` | unchanged | KEEP | Authoring verbs mirroring FluentValidation; each reads cold. |
| `ClientRule` (class) / `RuleName` / `RuleOperand` (+ `No`/`Literal`/`Range`/`PeerField` arms) | unchanged | KEEP | The recorded rule and its operand model; `…Operand` is load-bearing domain (left=field, right=operand). |
| 18 `RuleName` tokens | unchanged (all 18) | KEEP | `|Known|=18` is fixed; the 18→6 is a runtime evaluation-family grouping generated from C#, not a rename (§1.7). |
| `ClientRuleActivation` (`Always`/`When`) / `ValidationRangeBounds` / `ValidationPlanBinding` (+ field/prefix) | unchanged | KEEP | "When does this rule run"; the range bounds VO; render-time binders ("Binding" = what is remembered for lowering). |
| `ClientValidationFieldRuleBuilder<TModel,TValue>` | `ClientFieldRuleBuilder<TModel,TValue>` | GRAMMAR-FIX | "Validation" is redundant noise in the Validation namespace. Drop the filler across the whole `ClientValidation*` family → one consistent `Client*` prefix. |
| `Required`/`Empty`/`Email`/`Url`/`CreditCard`/`AtLeastOne` | unchanged | KEEP | No-operand rules; 1:1 with `RuleName` tokens. |
| `MinLength`/`MaxLength` (validation) | unchanged | KEEP | Already a complete pair; the **validation** length pair is distinct from the **condition** compare tokens (§1.3 note). |
| `Regex` / `Range` / `ExclusiveRange` / `Min` / `Max` | unchanged | KEEP | Each names its constraint; inclusive vs exclusive distinct. |
| `GreaterThan` / `LessThan` / `GreaterThanOrEqualTo` / `LessThanOrEqualTo` (literal + peer) | unchanged | KEEP | Spelled-out comparison verbs read better than gt/lt in the constraint lane; peer overload reuses the verb (one concept, two operand kinds). |
| `EqualTo` (literal + peer) | unchanged | KEEP | Same verb, two operand overloads; both → `equalTo`. |
| `NotEqual(literal)` vs `NotEqualTo(peerField)` | **kept distinct** | KEEP | **MUST stay two names** — `notEqual` (literal-only) vs `notEqualTo` (peer-only). Merging is a determinism violation (D1). The `NotEqual`/`NotEqualTo` asymmetry is the intentional literal-vs-peer signal. |
| `WhenField` / `WhenField<TProp>(value)` / `WhenFieldNot(...)` | unchanged | KEEP | The conditional-activation entry; truthy / equals / falsy gates read cold. |
| `WhenFieldGt/Gte/Lt/Lte` | `WhenFieldGreaterThan` / `…GreaterThanOrEqualTo` / `…LessThan` / `…LessThanOrEqualTo` | GRAMMAR-FIX | The rule-builder spells comparisons out; the WhenField suffix used cramped `Gt/Gte`. One-concept-one-name across the area demands the same spelling. |
| `WhenFieldNull/NotNull/Empty/NotEmpty` + In/NotIn/Between/Contains/StartsWith/EndsWith/Matches/MinLength/ArrayContains | unchanged | KEEP | Maps onto the **shared** `CompareOperator` set — no second vocabulary, no 7th presence op. |
| `WhenFields(...)` | unchanged | KEEP | Plural = compose multiple field guards via And/Or/Not; distinct from singular. |
| `When/Unless/WhenAsync/UnlessAsync` (server-only) | unchanged | KEEP | Deliberately shadow FluentValidation's names (server-only lane); renaming would lie about the inherited API. |
| `FieldConditionBuilder<T>` → `FieldStart` → `FieldGuard` | unchanged | KEEP | Three-stage typed condition builder for `WhenFields`; reads as a pipeline. |
| `FieldGuard.And/.Or/.Not` | unchanged signatures | GRAMMAR-FIX | The **one** And/Or/Not composition grammar survives (§1.1); `FieldCondition.All/Any/Not` is the internal n-ary lowering only. |
| `ClientValidationConditionBuilder` → `ClientValidationFieldConditionStart` → `ClientValidationCondition` | `ClientConditionBuilder` → `ClientFieldConditionStart` → `ClientCondition` | GRAMMAR-FIX | Drop redundant `Validation`; align the app-level trio with the FluentValidator trio so a dev meets ONE And/Or composition vocabulary, not two. |
| Condition op methods on the condition starts (`Truthy/Falsy/Eq/Neq/Gt/.../ArrayContains`) | unchanged | KEEP | The shared **conditions** vocabulary (terse, condition lane), distinct surface from the spelled-out **constraint** lane on the rule builder. |
| `ClientValidationFieldToken` (public) / `ClientValidationFieldReference` (internal) | `ClientFieldToken` / `ClientFieldReference` | GRAMMAR-FIX | Same `Validation`-filler drop; consistent `Client*` prefix. |
| `ValidationFieldPath` / `ValidationMessage` / `FieldCondition` (+ arms) | unchanged | KEEP | Dotted-path VO (with `Empty` identity), message VO, internal symbolic condition tree. |
| `ValidationRuleNode` / `ValidationRuleExecution` (none/constraint/peer) / `ValidationRuleActivation` / `ValidationGraph` | unchanged | KEEP | The plan carriers; `Kind` discriminator and `Always` identity named, not null. Aligns with the `ConditionGraph`/`ReactionGraph` family. |

### 3.7 App-level services + Plugin builders

| Current | Final | Verdict | Rationale |
|---|---|---|---|
| `NativeDrawer` / `.Open` / `.Close` / `.SetSize` / `DrawerSize` / `html.NativeDrawer()` / `.ElementId` | unchanged | KEEP | Vendor+thing; symmetric Open/Close verbs; type==renderer name. |
| `DrawerPosition` (Right/Bottom) | **DELETE** | DELETE | Dead — zero references, no setter consumes it. A clean rewrite kills the unused enum (it implies a feature that does not exist). |
| `NativeLoader` / `.Show` / `.Hide` / `.SetTarget` / `html.NativeLoader()` | unchanged | KEEP | Vendor+thing; symmetric verbs; type==renderer name. |
| `NativeLoader.SetTimeout(ms)` | `SetAutoHide(ms)` | GRAMMAR-FIX | `SetTimeout` collides with JS `setTimeout` AND with Toast's `SetTimeout` (different meaning). The doc itself says "auto-hide timeout" — `SetAutoHide` reads true. |
| `FusionToast` / `.SetTitle` / `.SetContent` / `.ShowCloseButton` / `.ShowProgressBar` / `.Success/Warning/Danger/Info` / `.Show` / `.Hide` / `html.FusionToast()` / `.ElementId` | unchanged | KEEP | Vendor+thing; severity verbs are the **real** severity vocab. |
| `FusionToast.SetTimeout(ms)` | `SetDuration(ms)` | GRAMMAR-FIX | This is display duration (`timeOut` prop), not a scheduler. `SetDuration` names the thing and removes the two-`SetTimeout`s-mean-different-things collision. |
| `ToastType` (Success/Warning/Danger/Info) | **DELETE** | DELETE | Dead — the live severity API is the `Success()/Warning()/…` methods. Two names for one concept; delete the orphan enum. |
| `ToastPosition` (6 corners) | **DELETE** | DELETE | Dead — renderer hardcodes Right/Bottom; no setter exposes it. Delete until a real `SetPosition` needs it. |
| `FusionConfirm` / `.Show` / `.Hide` / `.ElementId` | unchanged | KEEP | Vendor+thing; the confirm dialog service. |
| `FusionConfirm.SetContent(message)` | `SetMessage(message)` | GRAMMAR-FIX | The param is `message` and the dialog shows a confirmation *message*, not generic *content*. Stops "content" meaning two things across services. |
| `html.FusionConfirmDialog()` renderer | `html.FusionConfirm()` | GRAMMAR-FIX | Renderer diverged from the type; every other service uses type==renderer. Drop the `Dialog` suffix. |
| `NativeActionLink` / `NativeActionLinkBuilder.CssClass` / `.Attr` | `NativeActionLink` / `CssClass` / `Attr` | KEEP | Per-link component (generated id), not a fixed-id singleton; vendor+thing reads cold. (`Attr` kept here — established on this builder; the template `Attr`→`Attribute` fix is the SSR-string lane only.) |
| `Plugin` (base) / `.Function` / `.Property` / `.Command` / `PluginFunction` / `PluginCommand` / `PluginProperty` / `PluginOperation` | `PluginOperation` → `PluginMember`; rest unchanged | RENAME | The escape-hatch noun; value→`Function`, void→`Command`, read→`Property`. **`PluginOperation` mislead cold as a single concrete invocation, and "Operation" wrongly implies only the invokable arms (Function/Command), excluding `Property` (blind-fail).** Rename the abstract supertype to `PluginMember` — `Function`/`Command`/`Property` are all *members*, so the supertype reads as exactly their honest union. |
| `PluginTypeBuilder` | `PluginTypeBuilder` | KEEP | The fluent declarer for `plan.RegisterPlugin("n", p => ...)`. |
| `PluginTypeBuilder.Method<T>(...)` | `Function<T>(...)` | RENAME | Synonym collision inside one type — `Method` and `Function` both = value op (§1.8). Collapse to `Function` (matches the `Plugin` base). |
| `PluginTypeBuilder.Void(...)` | `Command(...)` | RENAME | `Void`/`Command` are exact synonyms (§1.8); `Void` leaks the return type, not a domain verb. Collapse to `Command`. |
| `PluginTypeBuilder.Function/.Command/.Property` (survivors) / `PluginArgumentTypes` / `.Arg<T>` | unchanged | KEEP | The unified value/void/read pair plus the arg-type contract. |
| `PluginMemberBuilder<TReturn,TModel>` (read face) | `PluginReadBuilder<TReturn,TModel>` | RENAME | "Member" hides the lane; this is the READ/value face (its twin is the void-CALL face). `Read` vs `Call` names the load-bearing lane. |
| `PluginCallBuilder<TModel>` / `.Fire()` | unchanged | KEEP | Void-call face; `Fire()` = "emit the call into the pipeline"; the lane is already in the name. |
| `.Arg(...)` overloads / `.Arg<TArg>(TypedSource)` / `.Arg<...>(ResponseBody/args, path)` / `.ArgValue<TValue>` | unchanged | KEEP | One `Arg` gloss over literal/source/payload value spines; `ArgValue` suffix names the generic-shaped variant. |
| `PluginArgumentCollector` (internal) | unchanged | KEEP | Accumulates + contract-checks args; "collector" names the job. |
| `PipelineBuilder.Plugin<T>(...)` / `.PluginProperty<T>` / void `.Plugin(...)` | unchanged | KEEP | Read/property/void-call entry overloads; split on member kind. |
| `TypedPluginSource` / `TypedPluginPropertySource` | unchanged | KEEP | Typed sources reading a plugin function return vs a plugin property; consistent with `TypedUrlSource`. |
| `PipelineBuilder.Confirm(message)` / `ConditionStart.Confirm(message)` | `Confirm` | KEEP | The user-decision async guard surface verb (§1.6). |
| (plan term) `ConfirmCondition` / `kind:"confirm"` | `ConfirmGuard` / `kind:"confirm"` | RENAME | The C# class lied (`…Condition` lumps it with deterministic compares). It is a user-decision async guard; JSON/TS `kind:"confirm"` already correct (§1.6). |

---

## 4. Cross-Area Conflicts Resolved (same concept → same name everywhere)

Each row is a concept that surfaced in more than one area; the **owner** mints the
name and every consumer uses it verbatim across C# / JSON / TS / tests / docs.

| Concept | Final name | Owner | Consumers | Resolution |
|---|---|---|---|---|
| Whole success body, as a node | `WholeResponseBody` / `kind:"whole-response-body"` | Value/Kernel | HTTP `Into`, Conditions `When/And/Or` | One name; `Into` consumes the kind, does not mint a third literal. De-sentineled (was `responseBody`). |
| Whole element value, as a node | `WholeElement` / `kind:"whole-element"` | Value/Kernel | Element `SetText/SetHtml` whole-reads, Array identity (`x => x`) | A **distinct** name from `WholeResponseBody` (different source); de-sentineled (was `elementValue`). |
| Sync/async tag on a reaction | `ReactionTiming` (`sync`/`async`, wire field `timing`) | Kernel | Reaction `executeReaction` router, `ReactionPipelineDraft` stamper | One enum + member tokens identical in C#, generated TS, runtime router. (Renamed from `ReactionLane` per the blind read.) |
| The one typed value-authoring surface | `TypedSource<T>` | Value | Conditions, Reaction (set/dispatch), HTTP gather, headers, route-params, component data-source, plugin args, array projection | No second source abstraction anywhere. |
| The comparison token list | `CompareOp` / `CompareOperator` | Kernel/Conditions | Condition `CompareEngine`, Validation `WhenField` reuse of the same engine | Single source; `notEqual` (literal-only) and `notEqualTo` (peer-only) stay **distinct** across both areas (D1). |
| The validation rule token list | `RuleName` | Validation | Generated TS `ValidationRuleName`, the 6 runtime families | One C# source; 18 tokens authored, 6 families generated, never hand-named. |
| User-decision async guard | surface `Confirm`, plan `ConfirmGuard`, wire `confirm` | App-level (surface) + Conditions (plan term) | Pipeline + nested condition composition | Surface verb `Confirm` kept; plan class `ConfirmCondition` → `ConfirmGuard`. |
| Root vs partial plan discriminant | `PlanScope` | Kernel | Slot composition | Kernel owns it; Slot does not rename. |
| The client-validation gate verb | `Validate` | Validation | HTTP `HttpRequestBuilder.Validate` | One verb; the HTTP entry is the same concept, not a fork. |
| The plugin escape-hatch source | `Plugin` / `kind:"plugin"` | Plugins | HTTP gather `Plugin<T>`, reaction `Plugin<T>`, `TypedPluginSource` | Intentional stringly boundary name, shared verbatim. |
| Template condition verb vs runtime condition verb | runtime `When` (Conditions) vs template `WhenTemplate`/`ShowTemplateIf` (Components) | Conditions owns bare `When` | Fusion template builder | The SSR `${if(...)}` string lane gets `WhenTemplate`/`ShowTemplateIf`; the runtime `ConditionGraph` keeps the bare `When` (it is foundational). Collision ended. |
| Element-CSS-class verb: live DOM vs SSR string | runtime `AddClass` (ElementBuilder) vs template `CssClass` (FusionTemplateBuilder) | each lane owns its verb | — | Deliberately **not** shared — different lanes (live mutation vs SSR append). `CssClass` ≠ `AddClass`. |
| Display-duration vs auto-hide timer | Toast `SetDuration` vs Loader `SetAutoHide` | each service | — | The two former `SetTimeout`s meant different things — split so neither lies nor collides. |

---

## 5. Decisive Rename / Add / Delete Ledger

Author-facing changes only (internal flattenings omitted). Every entry is decided
— nothing is "maybe".

**Renames (lie / collide / need-a-paragraph):**
`DomReady → PageLoad` · `CustomEvent → Event` · `DispatchWith → DispatchFrom` ·
`ValidationErrors → ShowValidationErrors` · `Static → Literal` (gather) · `Finally → OnSettled` (HTTP) ·
`Find → FindFirst` (array) · `AsSource → AsArraySource` (array) · `FocusIn → Focus` · `EventButton → DispatchButton` (template) · `.Class → CssClass` (template) ·
`.Attr → Attribute` (template) · `.When → WhenTemplate` (template) ·
`.ShowIf → ShowTemplateIf` (template) · `IClientValidationMetadataSource → IClientValidationRules` ·
`ClientValidation*Field*/Condition* → Client*` (prefix family drop) ·
`WhenFieldGt/Gte/Lt/Lte → WhenFieldGreaterThan/…OrEqualTo/LessThan/…OrEqualTo` ·
`PluginTypeBuilder.Method → Function` · `PluginTypeBuilder.Void → Command` ·
`PluginMemberBuilder → PluginReadBuilder` · `PluginOperation → PluginMember` (plan supertype) · `NativeLoader.SetTimeout → SetAutoHide` ·
`FusionToast.SetTimeout → SetDuration` · `FusionConfirm.SetContent → SetMessage` ·
`html.FusionConfirmDialog → html.FusionConfirm` · plan `ConfirmCondition → ConfirmGuard` ·
value-node `responseBody → WholeResponseBody` / `elementValue → WholeElement`.

**Adds (complete a family):**
condition `MaxLength` (+ VO `MaximumTextLength`) · array `Min` · array `Max` · array `Average`.

**Deletes (dead — zero references):**
`DrawerPosition` · `ToastType` · `ToastPosition`.

**New (close a hole):**
kind `WholeResponseBody` · kind `WholeElement` · `ReactionTiming` enum (`Sync`/`Async`, wire field `timing`).

---

*Every Final name in this sheet was checked against the cold one-breath read.
One concept has exactly one name across C#, JSON, TS, tests, and docs.*
